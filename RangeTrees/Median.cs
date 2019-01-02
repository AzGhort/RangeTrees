using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeTrees
{
    /// <summary>
    /// Extension class for finding a median in list of value tuples.
    /// </summary>
    static class MedianExtensions
    {      
        /// <summary>
        /// Finds median by quicksorting.
        /// </summary>
        /// <typeparam name="T">Type of ValueTuple.</typeparam>
        /// <param name="list">List of ValueTuples.</param>
        /// <param name="comparer">ValueTuple comparer to be used.</param>
        /// <returns>Median ValueTuple.</returns>
        public static ValueTuple<T,T> Median<T>(this List<ValueTuple<T,T>> list, IComparer<ValueTuple<T,T>> comparer) 
        {
            var sorted = QuickSort<T>.Sort(list, comparer);
            return sorted[sorted.Count / 2];
        }
    }
}
