using System.Collections.Generic;
using System.Linq;

namespace CardGame.Utils.Value
{
    public abstract class Value
    {
        public new abstract int GetHashCode();
        public new abstract bool Equals(object obj);

        protected const int magicAdd = 37;
        protected const int magicFactor = 397;
        protected int GetHashCode<T>(IEnumerable<T> enumerable)
        {
            if (enumerable == null) return (typeof(T)).GetHashCode();
            return enumerable.Aggregate(magicAdd * magicFactor, (value, item) =>
            {
                return value +
                       (item.GetHashCode() * magicFactor);
            });
        }
    }
}
