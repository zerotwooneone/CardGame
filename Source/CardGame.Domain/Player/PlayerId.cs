using System;
using CardGame.Utils.Factory;
using CardGame.Utils.Value;

namespace CardGame.Domain.Player
{
    public class PlayerId : StructValue<Guid>, IEquatable<PlayerId>
    {
        protected PlayerId(Guid value) : base(value)
        {
        }

        public static FactoryResult<PlayerId> Factory(Guid id)
        {
            if (Guid.Empty.Equals(id))
            {
                return FactoryResult<PlayerId>.Error("player id is required");
            }
            return FactoryResult<PlayerId>.Success(new PlayerId(id));
        }

        bool IEquatable<PlayerId>.Equals(PlayerId other)
        {
            if (other is null) return false;
            return Equals(other);
        }
    }
}