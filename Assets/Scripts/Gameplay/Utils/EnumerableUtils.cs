#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace TicTac.Gameplay.Utils
{
    /// <summary>Utility methods related to <see cref="IEnumerable{T}"/>.</summary>
    public static class EnumerableUtils
    {
        /// <summary>Returns values except for the one at index <paramref name="i"/>.</summary>
        public static IEnumerable<T> SkipAt<T>(this IEnumerable<T> values,  int i) => values.Where((_, j) => j != i);

        /// <summary>Returns <paramref name="value"/> wrapped in <see cref="IEnumerable{T}"/>.</summary>
        public static IEnumerable<T> AsEnumerable<T>(this T value) { yield return value; }
    }
}
