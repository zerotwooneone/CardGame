using System;
using CardGame.Utils.Factory;
using CardGame.Utils.Value;

namespace CardGame.Domain.Game
{
    public class GameId : StructValue<Guid>, IEquatable<GameId>
    {
        protected GameId(Guid value) : base(value)
        {
        }

        public static FactoryResult<GameId> Factory(Guid id)
        {
            if (Guid.Empty.Equals(id))
            {
                return FactoryResult<GameId>.Error("id is required");
            }
            return FactoryResult<GameId>.Success(new GameId(id));
        }
        bool IEquatable<GameId>.Equals(GameId other)
        {
            if (other is null) return false;
            return Equals(other);
        }
    }

    
}