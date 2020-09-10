using System;
using CardGame.Utils.Factory;
using CardGame.Utils.Value;

namespace CardGame.Domain.Round
{
    public class DrawDeckCount : StructValue<int>, IEquatable<DrawDeckCount>
    {
        public const int MaxCount = 16;
        protected DrawDeckCount(int value) : base(value)
        {
        }

        public static FactoryResult<DrawDeckCount> Factory(int value = MaxCount)
        {
            if (value < 0 || value > MaxCount)
            {
                return FactoryResult<DrawDeckCount>.Error("Invalid deck count");
            }
            return FactoryResult<DrawDeckCount>.Success(new DrawDeckCount(value));
        }
        bool IEquatable<DrawDeckCount>.Equals(DrawDeckCount other)
        {
            if (other is null) return false;
            return Equals(other);
        }
    }
}