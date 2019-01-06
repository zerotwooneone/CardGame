using System;

namespace CardGame.Core.Player
{
    public class Player
    {
        public Player(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
        public Hand.Hand Hand { get; private set; }

        public void SetHand(Hand.Hand newHand)
        {
            Hand = newHand;
        }
    }
}