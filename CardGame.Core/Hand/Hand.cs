using System.Collections;
using System.Collections.Generic;

namespace CardGame.Core.Hand
{
    public class Hand : IEnumerable<Card.Card>
    {
        private readonly List<Card.Card> _hand;
        public Card.Card Previous { get; private set; }
        public Card.Card Drawn { get; private set; }

        public Hand(Card.Card previous, Card.Card drawn = null)
        {
            _hand = new List<Card.Card>();
            Previous = previous;
            _hand.Add(Previous);
            Drawn = drawn;
            if (Drawn != null)
            {
                _hand.Add(Previous);
            }
        }

        public IEnumerator<Card.Card> GetEnumerator()
        {
            return _hand.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Card.Card Replace(Card.Card newCard)
        {
            var result = Previous;
            Previous = newCard;
            return result;
        }
    }
}