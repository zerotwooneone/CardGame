using System;
using CardGame.Core.CQRS;

namespace CardGame.Core.Player
{
    public class Player: IAggregate<Guid>
    {
        public Player(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
    }
}
