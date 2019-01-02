using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeTrees
{
    /// <summary>
    /// Class with static quicksort method.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    class QuickSort<TValue>
    {
        /// <summary>
        /// Quicksorts a list of value tuples.
        /// </summary>
        /// <param name="list">List to be sorted.</param>
        /// <param name="comparer">ValueTuple comparer to be used.</param>
        /// <returns>Sorted list.</returns>
        public static List<ValueTuple<TValue, TValue>> Sort(List<ValueTuple<TValue, TValue>> list, IComparer<ValueTuple<TValue, TValue>> comparer)
        {
            int N = list.Count;
            Stack<int> stack = new Stack<int>();
            var retval = new List<ValueTuple<TValue, TValue>>(list);
            int startIndex = 0;
            int endIndex = N - 1;
            stack.Push(startIndex);
            stack.Push(endIndex);

            while (stack.Count != 0)
            {
                endIndex = stack.Pop();
                startIndex = stack.Pop();

                int pivotIndex = DivideByPivot(retval, startIndex, endIndex, comparer);

                if (pivotIndex - 1 > startIndex)
                {
                    stack.Push(startIndex);
                    stack.Push(pivotIndex - 1);
                }
                if (pivotIndex + 1 < endIndex)
                {
                    stack.Push(pivotIndex + 1);
                    stack.Push(endIndex);
                }
            }
            return retval;
        }        

        /// <summary>
        /// Divides the list by pivot in place.
        /// </summary>
        /// <param name="list">List to sort</param>
        /// <param name="startIndex">Start index in list</param>
        /// <param name="endIndex">End index in list</param>
        /// <param name="comparer">Comparer to be used</param>
        /// <returns></returns>
        private static int DivideByPivot(List<ValueTuple<TValue, TValue>> list, int startIndex, int endIndex, IComparer<ValueTuple<TValue, TValue>> comparer)
        {
            ValueTuple<TValue, TValue> pivot = list[endIndex];
            int i = startIndex - 1;
            for (int j = startIndex; j < endIndex; j++)
            {
                if (comparer.Compare(list[j], pivot) < 0) 
                {
                    i++;
                    Swap(list, i, j);
                }
            }
            Swap(list, i + 1, endIndex);
            return i + 1;
        }

        /// <summary>
        /// Swaps two elements in list.
        /// </summary>
        /// <param name="list">List of elements</param>
        /// <param name="index1">Index of first swapped element</param>
        /// <param name="index2">Index of second swapped element</param>
        private static void Swap(List<ValueTuple<TValue, TValue>> list, int index1, int index2)
        {
            var tmp = list[index1];
            list[index1] = list[index2];
            list[index2] = tmp;
        }
    }
}
