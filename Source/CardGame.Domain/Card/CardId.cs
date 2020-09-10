using System;
using CardGame.Utils.Factory;
using CardGame.Utils.Value;

namespace CardGame.Domain.Card
{
    public class CardId : Value, IEquatable<CardId>
    {
        public CardValue CardValue { get; }
        public int Variant { get; }
        protected CardId(CardValue cardValue, int variant)
        {
            CardValue = cardValue;
            Variant = variant;
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return (((magicAdd * magicFactor)
                         + CardValue.GetHashCode())
                        * magicFactor)
                       + Variant.GetHashCode();
            }
            
        }

        public override bool Equals(object obj)
        {
            var other = obj as CardId;
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return CardValue.Equals(other.CardValue)
                   && Variant == other.Variant;
        }

        public static FactoryResult<CardId> Factory(CardValue cardValue, int varient)
        {
            if (cardValue is null)
            {
                return FactoryResult<CardId>.Error("card value is required");
            }
            return FactoryResult<CardId>.Success(new CardId(cardValue, varient));
        }
        bool IEquatable<CardId>.Equals(CardId other)
        {
            if (other is null) return false;
            return Equals(other);
        }
    }
}