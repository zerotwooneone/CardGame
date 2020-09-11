using System.Collections.Generic;
using System.Linq;
using CardGame.Domain.Card;
using CardGame.Utils.Factory;
using CardGame.Utils.Value;

namespace CardGame.Domain.Game
{
    public class Hand : Value
    {
        public CardId Card1 { get; }
        public CardId Card2 { get; }
        public IEnumerable<CardId> Cards { get; }

        protected Hand(CardId card1, CardId card2, IEnumerable<CardId> cards)
        {
            Card1 = card1;
            Card2 = card2;
            Cards = cards;
        }
        public override int GetHashCode()
        {
            return (magicAdd * magicFactor)
                   + GetHashCode(Cards);

        }

        public override bool Equals(object obj)
        {
            var other = obj as Hand;
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var cards = Cards.ToArray();
            var otherCards = other.Cards.ToArray();
            return cards.Length == otherCards.Length &&
                   cards.SequenceEqual(otherCards);
        }

        public static FactoryResult<Hand> Factory(IEnumerable<CardId> cards = null)
        {
            var ca = (cards ?? new CardId[0]).ToArray();
            const int MaxHandSize = 2;
            if (ca.Length > MaxHandSize)
            {
                return FactoryResult<Hand>.Error($"Handsize {ca.Length} exceeds max of {MaxHandSize}");
            }

            var card1 = ca.Length > 0
                ? ca[0]
                : null;
            var card2 = ca.Length > 1
                ? ca[1]
                : null;
            return FactoryResult<Hand>.Success(new Hand(card1, card2, ca));
        }
    }
}