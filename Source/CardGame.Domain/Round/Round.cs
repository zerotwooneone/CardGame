using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Domain.Abstractions.Card;
using CardGame.Domain.Abstractions.Game;
using CardGame.Domain.Card;
using CardGame.Domain.Player;
using CardGame.Utils.Factory;
using CardGame.Utils.Validation;
using CardGame.Utils.Value;

namespace CardGame.Domain.Round
{
    public class Round : Value, IEquatable<Round>
    {
        private readonly IDeckBuilder _deckBuilder;
        public int Id { get; }
        public IEnumerable<IPlayerId> PlayerOrder { get; }
        public Turn Turn { get; }
        public Deck Deck { get; }
        public IEnumerable<ICardId> Discard { get; }

        protected Round(int id,
            IEnumerable<IPlayerId> playerOrder,
            Turn turn, Deck deck,
            IEnumerable<ICardId> discard,
            IDeckBuilder deckBuilder)
        {
            _deckBuilder = deckBuilder;
            Id = id;
            PlayerOrder = playerOrder;
            Turn = turn;
            Deck = deck;
            Discard = discard;
        }

        public override int GetHashCode()
        {
            var eliminated = PlayerOrder.ToArray();
            var discard = Discard.ToArray();
            return ((magicAdd * magicFactor) +
                    (Id.GetHashCode() * magicFactor) +
                    (Turn.GetHashCode() * magicFactor) +
                    (Deck.GetHashCode() * magicFactor) +
                    GetHashCode(eliminated) +
                    GetHashCode(discard));

        }

        public override bool Equals(object obj)
        {
            var other = obj as Round;
            return Equals(other);
        }

        public bool Equals(Round other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            var eliminated = PlayerOrder.ToArray();
            var discard = Discard.ToArray();

            var otherElim = other.PlayerOrder.ToArray();
            var otherDiscard = other.Discard.ToArray();
            return Id == other.Id &&
                   Turn.Equals(other.Turn) &&
                   Deck.Equals(other.Deck) &&
                   eliminated.SequenceEqual(otherElim) &&
                   discard.SequenceEqual(otherDiscard);
        }

        public static FactoryResult<Round> Factory(int id,
            Turn turn,
            IDeckBuilder deckBuilder,
            Deck deck,
            IEnumerable<IPlayerId> playerOrder,
            IEnumerable<ICardId> discard)
        {
            //todo: if id < 0
            if (playerOrder is null)
            {
                return FactoryResult<Round>.Error("playerOrder is required");
            }
            if (turn is null)
            {
                return FactoryResult<Round>.Error("turn is required");
            }
            var playerIds = playerOrder as IPlayerId[] ?? playerOrder.ToArray();
            if (!playerIds.Any())
            {
                return FactoryResult<Round>.Error("playerIds cannot be empty");
            }
            if (deckBuilder is null)
            {
                return FactoryResult<Round>.Error("Deck builder is required");
            }
            if (deck is null)
            {
                var result = Deck.Factory(deckBuilder);
                if (result.IsError)
                {
                    return FactoryResult<Round>.Error(result.ErrorMessage);
                }

                deck = result.Value;
            }

            const int minPlayers = 1;
            if (playerIds.Length < minPlayers)
            {
                return FactoryResult<Round>.Error($"minimum players {minPlayers} required. but got {playerIds.Count()}");
            }
            discard = discard ?? new CardId[0];
            
            return FactoryResult<Round>.Success(new Round(id, playerIds, turn, deck, discard, deckBuilder));
        }

        public Round NextRound(IEnumerable<IPlayerId> playerOrder,
            Notification note)
        {
            //dont do an end of round check here
            return CreateRound(note, 1, Id + 1, playerOrder, null, null);
        }

        public Round NextTurn(Notification note)
        {
            if (Ended())
            {
                return RoundEndedError(note);
            }
            var turnId = Turn.Id + 1;
            
            var playerOrder = PlayerOrder.ToList();
            playerOrder.RemoveAt(0);
            playerOrder.Add(PlayerOrder.First());
            
            return CreateRound(note, turnId, Id, playerOrder, Discard, Deck);
        }

        private Round CreateRound(Notification note,
            int turnId,
            int roundId,
            IEnumerable<IPlayerId> playerOrder, 
            IEnumerable<ICardId> discard, 
            Deck deck)
        {
            var turnResult = Turn.Factory(turnId, playerOrder.First());
            if (turnResult.IsError)
            {
                note.AddError(turnResult.ErrorMessage);
                return this;
            }

            note.AddStateChange(nameof(Turn));
            var result = Factory(roundId, turnResult.Value, _deckBuilder, deck: deck, playerOrder: playerOrder, discard: discard);
            if (result.IsError)
            {
                note.AddError(result.ErrorMessage);
                return this;
            }

            note.AddStateChange(nameof(Round));
            return result.Value;
        }

        public bool IsTurn(IPlayerId playerId)
        {
            return Turn.CurrentPlayer.Equals(playerId);
        }

        public Round DiscardThis(ICardId cardId, Notification note)
        {
            if (Ended())
            {
                return RoundEndedError(note);
            }
            var newDiscard = Discard.Append(cardId).ToArray();
            var result = Factory(Id, Turn, _deckBuilder, Deck, PlayerOrder, discard: newDiscard);
            if (result.IsError)
            {
                note.AddError(result.ErrorMessage);
                return this;
            }

            note.AddStateChange(nameof(Round));
            return result.Value;
        }

        public Round Draw(Notification note, out CardId cardId)
        {
            if (Ended())
            {
                cardId = null; //todo: this seems fishy
                return RoundEndedError(note);
            }
            var newDeck = Deck.Draw(note, out cardId);
            var result = Factory(Id, Turn, _deckBuilder, newDeck, PlayerOrder, Discard);
            if (result.IsError)
            {
                note.AddError(result.ErrorMessage);
                return this;
            }
            note.AddStateChange(nameof(Round));
            return result.Value;
        }

        public Round Eliminate(PlayerId targetId, Notification note)
        {
            if (Ended())
            {
                return RoundEndedError(note);
            }
            if (PlayerOrder.Any(e => e.Equals(targetId)))
            {
                var playerOrder = PlayerOrder.Except(new []{targetId}).ToArray();
                var round = CreateRound(note, Turn.Id, Id, playerOrder, Discard, Deck);
                return round;
            }
            else
            {
                note.AddError("Player already eliminated");
                return this;
            }
        }

        private Round RoundEndedError(Notification note)
        {
            note.AddError("the round has ended");
            return this;
        }

        public bool Ended()
        {
            const int minRoundPlayers = 2;
            return Deck.IsEmpty() || PlayerOrder.Count() < minRoundPlayers;
        }
    }
}