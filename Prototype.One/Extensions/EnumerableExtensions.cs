using System.Collections.Generic;
using System.Linq;

namespace Prototype.One.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool DoesNotContain<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            return !source.Contains(value);
        }

        public static bool ContainsAll<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> value)
        {
            return !source.Except(value).Any();
        }
    }
}
