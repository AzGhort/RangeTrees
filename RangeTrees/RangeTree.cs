using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeTrees
{   
    /// <summary>
    /// Auxiliary enum for testing.
    /// </summary>
    enum OperationType
    {
        INSERT, RANGE_COUNT, CREATE
    }

    /// <summary>
    /// Main 2D RangeTree class.
    /// </summary>
    /// <typeparam name="TValue">Type to be saved in tree.</typeparam>
    class RangeTree<TValue> where TValue : IComparable<TValue>
    {
        public RangeTreeNode<TValue> Root { get; private set; }
        public double Alpha { get; private set; }
        private IComparer<ValueTuple<TValue, TValue>> xComparer = new XCoordinateComparer<TValue>();
        private IComparer<ValueTuple<TValue, TValue>> yComparer = new YCoordinateComparer<TValue>();
        int lastNodeCount = 0;
        OperationType lastOp = OperationType.CREATE;
        public long lastOpVisitedNodes = 0;

        /// <summary>
        /// Main constructor.
        /// </summary>
        /// <param name="alpha">Alpha of BB-alpha trees implementing range tree</param>
        public RangeTree(double alpha)
        {
            Alpha = alpha;
        }
        /// <summary>
        /// Constructor using a given tree (used for Node-Tree conversion).
        /// </summary>
        /// <param name="root">Root of new tree.</param>
        /// <param name="alpha">Alpha of BB-alpha trees implementing range tree</param>
        public RangeTree(RangeTreeNode<TValue> root, double alpha)
        {
            Root = root;
            Alpha = alpha;
        }

        #region Required operations
        /// <summary>
        /// Inserts a new value to tree.
        /// </summary>
        /// <param name="newvalue">2D point to be inserted</param>
        /// <param name="currentCoordinate">Current coordinate to navigate by</param>
        public void Insert(ValueTuple<TValue, TValue> newvalue, int currentCoordinate)
        {
            // for tree validity check
            //SaveLastState(OperationType.INSERT);
            lastOpVisitedNodes = 0;
            if (Root == null)
            {
                lastOpVisitedNodes = 1;
                Root = new RangeTreeNode<TValue>(newvalue, Alpha, currentCoordinate);
                return;
            }
            var comparer = (currentCoordinate == 0) ? xComparer : yComparer;
            var currentNode = Root;
            var parent = Root;

            // find a place for new leaf
            while (currentNode != null)
            {
                lastOpVisitedNodes++;
                parent = currentNode;
                if (currentCoordinate == 0) { currentNode.OtherDimensionSubtree.Insert(newvalue, 1); }
                // greater
                if (comparer.Compare(newvalue, currentNode.Value) > 0)
                {
                    currentNode.SubtreeNodesCount++;
                    currentNode = currentNode.Right;
                }
                // lesser
                else if (comparer.Compare(newvalue, currentNode.Value) < 0)
                {
                    currentNode.SubtreeNodesCount++;
                    currentNode = currentNode.Left;
                }
                // equal
                else
                {
                    currentNode.AddMiddleSon(new RangeTreeNode<TValue>(newvalue, Alpha, currentCoordinate));
                    // we are done here, this cannot break the invariant
                    return;
                }
            }

            // add the new leaf
            RangeTreeNode<TValue> leaf = new RangeTreeNode<TValue>(newvalue, Alpha, currentCoordinate);
            if (comparer.Compare(parent.Value, newvalue) > 0) { parent.SetLeftSon(leaf); }
            else { parent.SetRightSon(leaf); }
            parent.SubtreeNodesCount--;

            // rebuild the unbalanced trees
            currentNode = parent;
            while (currentNode != null)
            {                 
                if (!currentNode.IsBalanced())
                {
                    var nodes = QuickSort<TValue>.Sort(currentNode.GetSubtreeValuesInOrder(), yComparer);
                    var rebuild = Build(nodes, currentCoordinate);
                    // for x and y tree
                    lastOpVisitedNodes += (rebuild.SubtreeNodesCount);
                    // we rebuit entire tree
                    if (currentNode.Parent == null)
                    {
                        Root = rebuild;
                        currentNode = rebuild;
                    }
                    else
                    {
                        if (comparer.Compare(currentNode.Parent.Value, rebuild.Value) > 0) { currentNode.Parent.SetLeftSon(rebuild); }
                        else { currentNode.Parent.SetRightSon(rebuild); }
                    }
                }
                currentNode = currentNode.Parent;
            }

        }
          
        /// <summary>
        /// Get nodes count in given rectangle.
        /// </summary>
        /// <param name="corner1">Lower-left corner of rectangle.</param>
        /// <param name="corner2">Upper-right corner of rectangle</param>
        /// <returns>Nodes count in rectangle</returns>
        public int RangeCount(ValueTuple<TValue, TValue> corner1, ValueTuple<TValue, TValue> corner2)
        {
            // for tree validity check
            // SaveLastState(OperationType.RANGE_COUNT);
            (int res, int qc) = Root.Query(corner1, corner2, 0);
            lastOpVisitedNodes = qc;
            return res;
        }
        #endregion

        #region Building tree
        /// <summary>
        /// Main build method, build a tree from a list of points, given that the nodes are sorted by last coordinate.
        /// </summary>
        /// <param name="rangeTreePoints">Points to store in new range tree. </param>
        /// <param name="sortingCoordinate">Sorting coordinate of new tree.</param>
        /// <returns>New balanced range tree.</returns>
        private RangeTreeNode<TValue> Build(List<ValueTuple<TValue, TValue>> rangeTreePoints,  int sortingCoordinate)
        {
            if (rangeTreePoints.Count == 0)
            {
                return null;
            }
            if (rangeTreePoints.Count == 1)
            {
                var node = new RangeTreeNode<TValue>(rangeTreePoints[0], Alpha, sortingCoordinate);
                return node;
            }
            if (sortingCoordinate == 1)
            {
                // another y tree
                lastOpVisitedNodes += rangeTreePoints.Count;
                return CreateBalancedTree(rangeTreePoints);
            }

            // find median, build its other dimension tree
            var medVal = rangeTreePoints.Median(xComparer);
            var median = new RangeTreeNode<TValue>(medVal, Alpha, 0);
            median.OtherDimensionSubtree = new RangeTree<TValue>(Build(new List<(TValue, TValue)>(rangeTreePoints), sortingCoordinate + 1), Alpha);

            // find nodes with same x-value, set is as middle nodes
            rangeTreePoints.Remove(medVal);
            var midsons = FindMiddleSons(rangeTreePoints, medVal, sortingCoordinate);
            median.SetMiddleSons(midsons);

            // split nodes by x-value of median
            var smallerInX = GetSmallerThanPoint(rangeTreePoints, medVal, xComparer);
            
            // build sons
            median.SetLeftSon(Build(smallerInX, sortingCoordinate));
            median.SetRightSon(Build(rangeTreePoints, sortingCoordinate));
            return median;
        }

        #region Filtering nodes for range tree
        /// <summary>
        /// Find middle sons - e.g. points with the same coordinate.
        /// </summary>
        /// <param name="rangeTreePoints">Points to be searched from</param>
        /// <param name="median">Reference point for the same coordinate (median)</param>
        /// <param name="sortingDimension">Current coordinate</param>
        /// <returns></returns>
        private List<RangeTreeNode<TValue>> FindMiddleSons(List<ValueTuple<TValue, TValue>> rangeTreePoints, ValueTuple<TValue, TValue> median, int sortingDimension)
        {
            List<RangeTreeNode<TValue>> list = new List<RangeTreeNode<TValue>>();
            var comparer = (sortingDimension == 0) ? xComparer : yComparer;
            for (int i = rangeTreePoints.Count -1; i >= 0; i--)
            {
                var node = rangeTreePoints[i];
                if (comparer.Compare(node, median) == 0)
                {
                    list.Add(new RangeTreeNode<TValue>(node, Alpha, sortingDimension));
                    rangeTreePoints.RemoveAt(i);
                }
            }
            return list;
        }
        /// <summary>
        /// Find all points smaller than the given point.
        /// </summary>
        /// <param name="rangeTreePoints">List of points to search from</param>
        /// <param name="node">Reference point</param>
        /// <param name="comparer">Comparer to use</param>
        /// <returns></returns>
        private List<ValueTuple<TValue, TValue>> GetSmallerThanPoint(List<ValueTuple<TValue, TValue>> rangeTreePoints, ValueTuple<TValue, TValue> node, IComparer<ValueTuple<TValue, TValue>> comparer)
        {
            List<ValueTuple<TValue, TValue>> list = new List<ValueTuple<TValue, TValue>>();
            for (int i = rangeTreePoints.Count - 1; i >= 0; i--)
            {
                var next = rangeTreePoints[i];
                if (comparer.Compare(next, node) < 0)    
                {
                    list.Add(next);
                    rangeTreePoints.RemoveAt(i);
                }
            }
            return list;
        }
        #endregion
       
        /// <summary>
        /// Creates balanced tree from a sorted list.
        /// </summary>
        /// <param name="rangeTreePoints">Points to store in the tree</param>
        /// <returns>New (Y) range tree.</returns>
        private RangeTreeNode<TValue> CreateBalancedTree(List<ValueTuple<TValue, TValue>> rangeTreePoints)
        {
            if (rangeTreePoints.Count == 0)
            {
                return null;
            }
            var medVal = rangeTreePoints[rangeTreePoints.Count / 2];
            var median = new RangeTreeNode<TValue>(medVal, Alpha, 1);
            if (rangeTreePoints.Count == 1)
            {
                return median;
            }

            rangeTreePoints.Remove(medVal);
            var midsons = FindMiddleSons(rangeTreePoints, medVal, 1);
            median.SetMiddleSons(midsons);

            var smallerInY = GetSmallerThanPoint(rangeTreePoints, medVal, yComparer);

            // build sons
            median.SetLeftSon(CreateBalancedTree(smallerInY));
            median.SetRightSon(CreateBalancedTree(rangeTreePoints));

            return median;
        }
        #endregion

        #region Validation
        /// <summary>
        /// Validate all nodes of the tree.
        /// </summary>
        public void ValidateTree()
        {
            if (Root == null) { return; }
            var allNodes = Root.GetSubtreeNodesInOrder();
            foreach (var node in allNodes)
            {
                node.Validate();
            }
        }
        /// <summary>
        /// Saves last operation and nodes count.
        /// </summary>
        /// <param name="type">Type of last operation</param>
        private void SaveLastState(OperationType type)
        {
            lastOp = type;
            if (Root != null) lastNodeCount = Root.SubtreeNodesCount;
        }
        /// <summary>
        /// Validate last operation.
        /// </summary>
        public void ValidateLastOperation()
        {
            if (lastOp == OperationType.INSERT)
            {
                Debug.Assert(lastNodeCount + 1 == Root.SubtreeNodesCount);
            }
            else if (lastOp == OperationType.RANGE_COUNT)
            {
                Debug.Assert(lastNodeCount == Root.SubtreeNodesCount);
            }
        }
        #endregion
    }
}
