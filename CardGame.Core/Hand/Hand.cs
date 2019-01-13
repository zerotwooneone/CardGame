using System;
using System.Collections;
using System.Collections.Generic;

namespace CardGame.Core.Hand
{
    public class Hand : IEnumerable<Guid>
    {
        public Guid Previous { get; }
        public Guid? Drawn { get; }

        public Hand(Guid previous, Guid? drawn = null)
        {
            Previous = previous;
            Drawn = drawn;
        }

        public IEnumerator<Guid> GetEnumerator()
        {
            yield return Previous;
            if (Drawn.HasValue)
            {
                yield return Drawn.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Hand Append(Guid drawn)
        {
            if (Previous != null && Drawn != null)
            {
                throw new InvalidOperationException("Cannot draw when holding two cards.");
            }

            return new Hand(Previous, drawn);
        }

        public Hand Discard(Guid cardId)
        {
            if (Previous == cardId)
            {
                if (Drawn == null)
                {
                    return null;
                }
                return new Hand(Drawn.Value);
            }

            if (Drawn == cardId)
            {
                return new Hand(Previous);
            }
            throw new InvalidOperationException("Cannot discard a card which does not exist in the hand.");
        }
    }
}