using System;
using CardGame.Utils.Factory;
using CardGame.Utils.Value;

namespace CardGame.Domain.Card
{
    public class CardValue : StructValue<CardStrength>, IEquatable<CardValue>
    {
        protected CardValue(CardStrength value) : base(value)
        {
        }

        public static FactoryResult<CardValue> Factory(CardStrength cardStrength)
        {
            if (cardStrength < CardStrength.Min || cardStrength > CardStrength.Max)
            {
                return FactoryResult<CardValue>.Error("Invalid card strength");
            }
            return FactoryResult<CardValue>.Success(new CardValue(cardStrength));
        }
        bool IEquatable<CardValue>.Equals(CardValue other)
        {
            if (other is null) return false;
            return Equals(other);
        }
    }
}