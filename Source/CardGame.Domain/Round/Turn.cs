using CardGame.Domain.Game;
using CardGame.Utils.Factory;
using CardGame.Utils.Value;
using System;

namespace CardGame.Domain.Round
{
    public class Turn : Value, IEquatable<Turn>
    {
        public int Id { get; }
        public PlayerId CurrentPlayer { get; }
        protected Turn(int id, PlayerId currentPlayer)
        {
            Id = id;
            CurrentPlayer = currentPlayer;
        }

        public override int GetHashCode()
        {
            return ((magicAdd * magicFactor) +
                    (Id.GetHashCode() * magicFactor) +
                    (CurrentPlayer.GetHashCode() * magicFactor));
        }

        public override bool Equals(object obj)
        {
            var other = obj as Turn;
            if (other == null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id && CurrentPlayer.Equals(other.CurrentPlayer);
        }

        public static FactoryResult<Turn> Factory(int id, PlayerId player)
        {
            if (id == default)
            {
                return FactoryResult<Turn>.Error("Id is required");
            }

            if (player == null)
            {
                return FactoryResult<Turn>.Error("Player is required");
            }
            return FactoryResult<Turn>.Success(new Turn(id, player));
        }
        bool IEquatable<Turn>.Equals(Turn other)
        {
            if (other is null) return false;
            return Equals(other);
        }
    }
}