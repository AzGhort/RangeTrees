using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeTrees
{
    /// <summary>
    /// Class for testing RangeTrees.
    /// </summary>
    class RangeTreeTester
    {
        int curNodes = 0;
        RangeTree<int> Tree;
        double alpha;
        long queryMaxCount = 0;
        long insertMaxCount = 0;
        long queryAvgCount = 0;
        long insertAvgCount = 0;
        long totalQueries = 0;
        long totalInserts = 0;

        public StreamWriter writer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="output">Name of the output file.</param>
        /// <param name="alpha">Alpha of the Range Tree. </param>
        public RangeTreeTester(string output, double alpha)
        {
            writer = new StreamWriter(output);
            this.alpha = alpha;
        }

        /// <summary>
        /// Main testing method, reads input and executes commands.
        /// </summary>
        public void CreateRangeTree()
        {
            string s = "";
            while ((s = Console.ReadLine()) != null)
            {
                ExecuteCommand(s);
            }
        }

        /// <summary>
        /// Executes one command from the input.
        /// </summary>
        /// <param name="command">Command to be executed.</param>
        private void ExecuteCommand(string command)
        {
            string[] tokens = command.Split(new char[] { ' ' });
            switch (tokens[0])
            {
                // new tree
                case "#":
                    if (curNodes > 0) { FinishCurrentTree(); }
                    curNodes = int.Parse(tokens[1]);
                    Tree = new RangeTree<int>(alpha);
                    break;
                // insert
                case "I":
                    int x = int.Parse(tokens[1]);
                    int y = int.Parse(tokens[2]);
                    Tree.Insert((x, y), 0);
                    totalInserts++;
                    insertAvgCount += Tree.lastOpVisitedNodes;
                    if (Tree.lastOpVisitedNodes > insertMaxCount) { insertMaxCount = Tree.lastOpVisitedNodes; }
                    break;
                // range count
                case "C":
                    int x1 = int.Parse(tokens[1]);
                    int y1 = int.Parse(tokens[2]);
                    int x2 = int.Parse(tokens[3]);
                    int y2 = int.Parse(tokens[4]);
                    var count = Tree.RangeCount((x1, y1), (x2, y2));
                    totalQueries++;
                    queryAvgCount += Tree.lastOpVisitedNodes;
                    if (Tree.lastOpVisitedNodes > queryMaxCount) { queryMaxCount = Tree.lastOpVisitedNodes; }
                    break;
            }
            /*
            if (Tree != null)
            {
                Tree.ValidateLastOperation();
                Tree.ValidateTree();
            }*/
        }

        /// <summary>
        /// Outputs stats of current tree, resets counters.
        /// </summary>
        public void FinishCurrentTree()
        {
            writer.WriteLine($"{curNodes} {queryAvgCount * 1.0 / totalQueries } {queryMaxCount} {insertAvgCount * 1.0 / totalInserts} {insertMaxCount}");
            queryMaxCount = 0;
            queryAvgCount = 0;
            insertMaxCount = 0;
            insertAvgCount = 0;
            totalInserts = 0;
            totalQueries = 0;
        }
    }
}
