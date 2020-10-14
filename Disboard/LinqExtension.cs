using System.Collections.Generic;
using System.Linq;

namespace Disboard
{
    public static class LinqExtension
    {
        public static IEnumerable<(int index, TSource elem)> Enumerate<TSource>(this IEnumerable<TSource> source)
            => source.Select((elem, index) => (index, elem));
    }
}
