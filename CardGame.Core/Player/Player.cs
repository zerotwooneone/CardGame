using System;

namespace CardGame.Core.Player
{
    public class Player
    {
        public Player(Guid id,
            string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        public Guid Id { get; }
        public string DisplayName { get; }
    }
}