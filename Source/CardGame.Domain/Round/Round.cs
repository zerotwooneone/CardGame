using System;
using System.Collections.Generic;
using System.Linq;
using CardGame.Domain.Card;
using CardGame.Domain.Game;
using CardGame.Utils.Factory;
using CardGame.Utils.Value;

namespace CardGame.Domain.Round
{
    public class Round : Value, IEquatable<Round>
    {
        public int Id { get; }
        public IEnumerable<PlayerId> EliminatedPlayers { get; }
        public Turn Turn { get; }
        public DrawDeckCount DrawDeckCount { get; }
        public IEnumerable<CardId> Discard { get; }
        protected Round(int id, IEnumerable<PlayerId> eliminatedPlayers, Turn turn, DrawDeckCount drawDeckCount, IEnumerable<CardId> discard)
        {
            Id = id;
            EliminatedPlayers = eliminatedPlayers;
            Turn = turn;
            DrawDeckCount = drawDeckCount;
            Discard = discard;
        }

        public override int GetHashCode()
        {
            var eliminated = EliminatedPlayers.ToArray();
            var discard = Discard.ToArray();
            return ((magicAdd * magicFactor) +
                    (Id.GetHashCode() * magicFactor) +
                    (Turn.GetHashCode() * magicFactor) +
                    (DrawDeckCount.GetHashCode() * magicFactor) +
                    GetHashCode(eliminated) +
                    GetHashCode(discard));

        }

        public override bool Equals(object obj)
        {
            var other = obj as Round;
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            var eliminated = EliminatedPlayers.ToArray();
            var discard = Discard.ToArray();

            var otherElim = other.EliminatedPlayers.ToArray();
            var otherDiscard = other.Discard.ToArray();
            return Id == other.Id &&
                    Turn.Equals(other.Turn) &&
                    DrawDeckCount.Equals(other.DrawDeckCount) &&
                    eliminated.SequenceEqual(otherElim) &&
                    discard.SequenceEqual(otherDiscard);
        }

        public static FactoryResult<Round> Factory(int id,
            Turn turn,
            DrawDeckCount drawDeckCount = null,
            IEnumerable<PlayerId> eliminated = null,
            IEnumerable<CardId> discard = null)
        {
            if (turn is null)
            {
                return FactoryResult<Round>.Error("Turn is required");
            }

            if (drawDeckCount is null)
            {
                var result = DrawDeckCount.Factory();
                if (result.IsError)
                {
                    return FactoryResult<Round>.Error("Error creating new deck");
                }

                drawDeckCount = result.Value;
            }
            eliminated = eliminated ?? new PlayerId[0];
            discard = discard ?? new CardId[0];
            return FactoryResult<Round>.Success(new Round(id, eliminated, turn, drawDeckCount, discard));
        }

        public Round GetNext(Turn turn)
        {
            var result = Factory(Id + 1, turn);
            if (result.IsError)
            {
                throw new Exception(result.ErrorMessage);
            }

            return result.Value;
        }
        bool IEquatable<Round>.Equals(Round other)
        {
            if (other is null) return false;
            return Equals(other);
        }
    }
}