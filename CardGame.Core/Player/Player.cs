using System;

namespace CardGame.Core.Player
{
    public class Player
    {
        public Player(Guid id)
        {
            Id = id;
        }

        public Guid? Id { get; }

        
    }
}
