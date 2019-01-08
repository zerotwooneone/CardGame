using System;
using System.Linq;
using CardGame.Core.Card;

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
        public Hand.Hand Hand { get; private set; }

        public void SetHand(Hand.Hand newHand)
        {
            Hand = newHand;
        }

        public void Discard(CardValue cardValue)
        {
            var newHand = Hand.Discard(cardValue);
            SetHand(newHand);
        }
    }
}