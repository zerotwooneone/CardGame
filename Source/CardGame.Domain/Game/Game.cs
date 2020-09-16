using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Domain.Card;
using CardGame.Domain.Player;
using CardGame.Utils.Entity;
using CardGame.Utils.Extensions;
using CardGame.Utils.Factory;
using CardGame.Utils.Validation;

namespace CardGame.Domain.Game
{
    public class Game : Entity<GameId>
    {
        public IEnumerable<Player.Player> Players { get; }
        public Round.Round Round { get; protected set; }

        protected Game(GameId id,
            IEnumerable<Player.Player> players,
            Round.Round round) : base(id)
        {
            Players = players;
            Round = round;
        }

        public static FactoryResult<Game> Factory(Guid id,
            IEnumerable<Player.Player> players,
            Round.Round round)
        {
            var idResult = GameId.Factory(id);
            if (idResult.IsError)
            {
                return FactoryResult<Game>.Error("invalid id");
            }

            if (round is null)
            {
                return FactoryResult<Game>.Error("Round is required");
            }

            var pa = players as Player.Player[] ?? players.ToArray();
            if (pa.IsNullOrEmpty())
            {
                return FactoryResult<Game>.Error("players are required");
            }
            var ps = pa.Distinct().ToArray();
            const int playerMin = 2;
            const int playerMax = 4;
            if (ps.Length < playerMin || ps.Length > playerMax)
            {
                return FactoryResult<Game>.Error($"player count must be between {playerMin} and {playerMax} inclusive");
            }

            return FactoryResult<Game>.Success(new Game(idResult.Value, ps, round));
        }

        public Notification Play(PlayerId playerId, 
            CardId cardId,
            PlayerId target,
            CardValue guessValue)
        {
            var player = GetPlayerById(playerId);
            var targetPlayer = GetPlayerById(target);

            var note = new Notification();

            if (!Round.IsTurn(playerId))
            {
                note.AddError($"not player's turn {playerId}");
                return note;
            }

            //discard
            var targetCard = GetCard(targetPlayer.Hand.Card1, note);
            var playContext = new PlayContext(player, targetPlayer, guessValue,this, targetCard, note);
            var card = GetCard(cardId, note);
            card.Discard(playContext);
            player.Discard(cardId, note);
            Round = Round.DiscardThis(cardId, note);

            //change state of round
            if (targetPlayer != null && targetPlayer.Protected.Value)
            {
                //nothing
            }
            else
            {
                if (card.PreventPlay(playContext))
                {
                    note.AddError($"Card {cardId} cannot be played.");
                }
                else
                {
                    card.Play(playContext, note);   
                }
                 
            }

            
            //next round
            var newRound = Round.Ended()
                ? Round.NextRound(GetRoundWinner(),note)
                : Round.NextTurn(note);
            
            var nextPlayerId = newRound.Turn.CurrentPlayer;
            var nextPlayer = GetPlayerById(nextPlayerId);
            nextPlayer.ClearProtection(note);

            Round = newRound;
            return note;
        }

        private PlayerId GetRoundWinner()
        {
            var remainingPlayers = Round.RemainingPlayers.ToArray();
            if (remainingPlayers.Count() == 1)
            {
                return remainingPlayers.First();
            }

            var players = remainingPlayers.Select(GetPlayerById);
            var hands = players
                .Select(p => new {player = p.Id, card = p.Hand.Card1.CardValue.Value})
                .ToLookup(pc => pc.card);
            var max = hands.Max(g => g.Key);
            
            //todo: come up with a better way to pick a winner
            var random = new Random();
            var winner = hands[max].Count() == 1
                ? hands[max].First()
                : hands[max].ToArray()[random.Next(0, hands[max].Count())];
            return winner.player;
        }

        private Player.Player GetPlayerById(PlayerId playerId)
        {
            return Players.FirstOrDefault(p => p.Id.Equals(playerId));
        }

        private Card.Card GetCard(CardId cardId, Notification note)
        {
            FactoryResult<Card.Card> result;
            switch (cardId.CardValue.Value)
            {
                case CardStrength.Princess:
                    result = Princess.Factory(cardId.Variant);
                    break;
                case CardStrength.Countess:
                    result = Countess.Factory(cardId.Variant);
                    break;
                case CardStrength.King:
                    result = King.Factory(cardId.Variant);
                    break;
                case CardStrength.Prince:
                    result = Prince.Factory(cardId.Variant);
                    break;
                case CardStrength.Handmaid:
                    result = Handmaid.Factory(cardId.Variant);
                    break;
                case CardStrength.Baron:
                    result = Baron.Factory(cardId.Variant);
                    break;
                case CardStrength.Priest:
                    result = Priest.Factory(cardId.Variant);
                    break;
                case CardStrength.Guard:
                    result = Guard.Factory(cardId.Variant);
                    break;
                default:
                    note.AddError($"unknown card id {cardId}");
                    return null;
            }

            if (result.IsError)
            {
                note.AddError(result.ErrorMessage);
                return null;
            }

            return result.Value;
        }

        public void Discard(CardId cardId, Notification note)
        {
            Round = Round.DiscardThis(cardId, note);
        }

        public CardId Draw(Notification note)
        {
            Round = Round.Draw(note, out var cardId);
            return cardId;
        }

        public void EliminateFromRound(PlayerId targetId, Notification note)
        {
            if (Players.All(p => !p.Id.Equals(targetId)))
            {
                note.AddError($"Player does not exist in game {targetId}");
            }
            else
            {
                Round = Round.Eliminate(targetId, note);    
            }
        }
    }
}