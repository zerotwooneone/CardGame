using System;
using System.Collections;
using System.Collections.Generic;

namespace CardGame.Core.Hand
{
    public class Hand : IEnumerable<Card.Card>
    {
        private readonly List<Card.Card> _hand;
        public Card.Card Previous { get; }
        public Card.Card Drawn { get; }

        public Hand(Card.Card previous, Card.Card drawn = null)
        {
            _hand = new List<Card.Card>();
            Previous = previous;
            _hand.Add(Previous);
            Drawn = drawn;
            if (Drawn != null)
            {
                _hand.Add(Drawn);
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

        public Hand Append(Card.Card drawn)
        {
            if (Previous != null && Drawn != null)
            {
                throw new InvalidOperationException("Cannot draw when holding two cards.");
            }

            if (Previous == null && Drawn == null)
            {
                return new Hand(drawn);
            }

            var previous = Previous ?? Drawn;
            return new Hand(previous, drawn);
        }

        public Hand Discard(Guid cardId)
        {
            if (Previous?.Id == cardId)
            {
                if (Drawn == null)
                {
                    return null;
                }
                return new Hand(Drawn);
            }

            if (Drawn?.Id == cardId)
            {
                if (Previous == null)
                {
                    return null;
                }
                return new Hand(Previous);
            }
            throw new InvalidOperationException("Cannot discard a card which does not exist in the hand.");
        }
    }
}