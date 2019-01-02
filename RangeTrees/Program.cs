using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeTrees
{
    class Program
    {
        static void TestQuickSortAndMedian()
        {
            Random r = new Random();
            List<ValueTuple<int, int>> list = new List<ValueTuple<int, int>>();
            for (int i = 0; i < 100; i++)
            {
                int a = r.Next(1000);
                list.Add(new ValueTuple<int, int>(a, 1000 - a)); 
            }
            var xMedian = list.Median(new XCoordinateComparer<int>());
            var yMedian = list.Median(new YCoordinateComparer<int>());
            var xSorted = QuickSort<int>.Sort(list, new XCoordinateComparer<int>());
            var ySorted = QuickSort<int>.Sort(list, new YCoordinateComparer<int>());
        }

        static void Main(string[] args)
        {
            //TestQuickSortAndMedian();
            if (args.Length != 2)
            {
                Console.WriteLine("Two arguments expected, name of the output file and the alpha value (in percents - eg. int 1..99).");
            }
            double percent = int.Parse(args[1]) * 1.0 / 100.0;
            var tester = new RangeTreeTester(args[0], percent);
            tester.CreateRangeTree();
            tester.FinishCurrentTree();
            tester.writer.Close();
        }
    }
}
