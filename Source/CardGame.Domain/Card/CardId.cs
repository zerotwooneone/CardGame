using System;
using CardGame.Domain.Abstractions.Card;
using CardGame.Utils.Factory;
using CardGame.Utils.Validation;
using CardGame.Utils.Value;

namespace CardGame.Domain.Card
{
    public class CardId : Value, ICardId
    {
        public ICardValue CardValue { get; }
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
            var other = obj as ICardId;
            return Equals(other);
        }

        public bool Equals(ICardId other)
        {
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

        public override string ToString()
        {
            return $"{CardValue} Variant:{Variant}";
        }

        public static FactoryResult<CardId> Factory(CardValue cardValue, int varient)
        {
            if (cardValue is null)
            {
                return FactoryResult<CardId>.Error("card value is required");
            }
            return FactoryResult<CardId>.Success(new CardId(cardValue, varient));
        } public static FactoryResult<CardId> Factory(CardStrength cardStrength, int varient = default)
        {
            var result = Domain.Card.CardValue.Factory(cardStrength);
            if (result.IsError)
            {
                return FactoryResult<CardId>.Error(result.ErrorMessage);
            }
            return FactoryResult<CardId>.Success(new CardId(result.Value, varient));
        }

        public bool IsWeaker(CardId targeCard, Notification note)
        {
            return CardValue.IsWeaker(targeCard.CardValue, note);
        }
    }
}