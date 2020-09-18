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
        private readonly IEnumerator<IPlayerId> _turnOrder;
        public int Id { get; }
        public IEnumerable<PlayerId> RemainingPlayers { get; }
        public Turn Turn { get; }
        public Deck Deck { get; }
        public IEnumerable<ICardId> Discard { get; }
        public PlayerId WinningPlayer { get; }

        protected Round(int id,
            IEnumerable<PlayerId> remainingPlayers,
            Turn turn, Deck deck,
            IEnumerable<ICardId> discard,
            IEnumerator<IPlayerId> turnOrder, 
            PlayerId winningPlayer)
        {
            _turnOrder = turnOrder;
            Id = id;
            RemainingPlayers = remainingPlayers;
            Turn = turn;
            Deck = deck;
            Discard = discard;
            WinningPlayer = winningPlayer;
        }

        public override int GetHashCode()
        {
            var eliminated = RemainingPlayers.ToArray();
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
            var eliminated = RemainingPlayers.ToArray();
            var discard = Discard.ToArray();

            var otherElim = other.RemainingPlayers.ToArray();
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
            IEnumerable<PlayerId> remaining,
            Deck deck = null,
            IEnumerable<CardId> discard = null)
        {
            if (deck is null)
            {
                var result = Deck.Factory(deckBuilder);
                if (result.IsError)
                {
                    return FactoryResult<Round>.Error("Error creating new deck");
                }

                deck = result.Value;
            }
            return Factory(id, turn, deck, remaining, discard);
        }

        public static FactoryResult<Round> Factory(int id,
            Turn turn,
            Deck deck,
            IEnumerable<PlayerId> remaining,
            IEnumerable<ICardId> discard = null)
        {
            if (turn is null)
            {
                return FactoryResult<Round>.Error("Turn is required");
            }

            if (deck is null)
            {
                return FactoryResult<Round>.Error("Deck is required");
            }

            var playerIds = remaining as PlayerId[] ?? remaining.ToArray();
            const int minPlayers = 1;
            if (playerIds.Length < minPlayers)
            {
                return FactoryResult<Round>.Error($"minimum players {minPlayers} required. but got {playerIds.Count()}");
            }
            discard = discard ?? new CardId[0];
            var turnOrder = GetTurnOrder(playerIds, turn.CurrentPlayer);
            if (turnOrder == null)
            {
                return FactoryResult<Round>.Error($"Could not find player {turn.CurrentPlayer} to start the round");
            }

            var winningPlayer = playerIds.Count() == 1
                ? playerIds.First()
                : null;
            return FactoryResult<Round>.Success(new Round(id, playerIds, turn, deck, discard, turnOrder, winningPlayer));
        }

        private static IEnumerator<IPlayerId> GetTurnOrder(IEnumerable<IPlayerId> playerIds, IPlayerId currentPlayer)
        {
            var turnOrder = GetTurnOrder(playerIds);
            turnOrder.MoveNext();
            const int maxPlayers = 4;
            for (int i = 0; i < maxPlayers; i++)
            {
                if (turnOrder.Current.Equals(currentPlayer))
                {
                    return turnOrder;
                }
                turnOrder.MoveNext();
            }

            return null;
        }

        private static IEnumerator<IPlayerId> GetTurnOrder(IEnumerable<IPlayerId> playerIds)
        {
            while (true)
            {
                foreach (var player in playerIds)
                {
                    yield return player;
                }
            }
        }

        public Round NextRound(PlayerId winningPlayer,
            IEnumerable<PlayerId> players,
            Notification note)
        {
            //dont do an end of round check here
            return NextTurn(note, 1, Id + 1, winningPlayer, players);
        }

        public Round NextTurn(Notification note)
        {
            if (Ended())
            {
                return RoundEndedError(note);
            }
            var nextTurnPlayer = GetNextPlayer();
            var turnId = Turn.Id + 1;

            return NextTurn(note, turnId, Id, nextTurnPlayer, RemainingPlayers);
        }

        private IPlayerId GetNextPlayer()
        {
            _turnOrder.MoveNext();
            return _turnOrder.Current;
        }

        private Round NextTurn(Notification note, 
            int turnId, 
            int roundId, 
            IPlayerId nextTurnPlayer,
            IEnumerable<PlayerId> remainingPlayers)
        {
            var turnResult = Turn.Factory(turnId, nextTurnPlayer);
            if (turnResult.IsError)
            {
                note.AddError(turnResult.ErrorMessage);
                return this;
            }

            note.AddStateChange(nameof(Turn));

            var result = Factory(roundId, turnResult.Value, Deck, remainingPlayers);
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
            var result = Factory(Id, Turn, Deck, RemainingPlayers, newDiscard);
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
            var newDrawDeckCount = Deck.Draw(note, out cardId);
            var result = Factory(Id, Turn, newDrawDeckCount, RemainingPlayers, Discard);
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
            if (RemainingPlayers.Any(e => e.Equals(targetId)))
            {
                var remaining = RemainingPlayers.Except(new []{targetId});
                var result = Factory(Id, Turn, Deck, remaining, Discard);
                if (result.IsError)
                {
                    note.AddError(result.ErrorMessage);
                    return this;
                }

                return result.Value;
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
            return Deck.IsEmpty() || WinningPlayer != null;
        }
    }
}