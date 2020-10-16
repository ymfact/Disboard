using System;
using System.Collections.Generic;
using System.Linq;

namespace Disboard
{
    public static class LinqExtension
    {
        public static IEnumerable<(int index, TSource elem)> Enumerate<TSource>(this IEnumerable<TSource> source)
            => source.Select((elem, index) => (index, elem));

        public static int? FindIndex<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            try
            {
                return source.Enumerate().First(_ => predicate(_.elem)).index;
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}
