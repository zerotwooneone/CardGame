using System;
using CardGame.Domain.Abstractions.Game;
using CardGame.Utils.Factory;
using CardGame.Utils.Value;

namespace CardGame.Domain.Game
{
    public class GameId : StructValue<Guid>, IGameId
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
        public bool Equals(IGameId other)
        {
            if (other is null) return false;
            return base.Equals(other);
        }
    }

    
}