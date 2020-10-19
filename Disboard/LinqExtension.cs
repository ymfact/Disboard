using System;
using System.Collections.Generic;
using System.Linq;

namespace Disboard
{
    /// <summary>
    /// Linq에서 지원하지 않는 명령어들이 포함되어 있습니다.
    /// </summary>
    public static class LinqExtension
    {
        /// <summary>
        /// python의 enumerate에 대응합니다. 목록에 인덱스를 붙여줍니다.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to invoke a transform function on.</param>
        /// <returns>(index, element)의 나열을 반환합니다.</returns>
        public static IEnumerable<(int index, TSource elem)> Enumerate<TSource>(this IEnumerable<TSource> source)
            => source.Select((elem, index) => (index, elem));

        /// <summary>
        /// 목록에서 조건에 맞는 대상을 찾아 인덱스를 반환합니다.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">An System.Collections.Generic.IEnumerable`1 to return an element from.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>찾은 결과를 반환합니다. 결과가 없으면 null을 반환합니다.</returns>
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
