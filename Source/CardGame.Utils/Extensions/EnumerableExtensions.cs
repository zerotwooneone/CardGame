using System.Collections.Generic;
using System.Linq;

namespace CardGame.Utils.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable is null || !enumerable.Any();
        }
    }
}
