using System;
using CardGame.Domain.Abstractions.Game;
using CardGame.Utils.Factory;
using CardGame.Utils.Value;

namespace CardGame.Domain.Player
{
    public class PlayerId : StructValue<Guid>, IPlayerId
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

        public bool Equals(IPlayerId other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }
}