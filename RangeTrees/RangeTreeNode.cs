using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RangeTrees
{
    #region Auxiliary 2D points managing classes
    /// <summary>
    /// Class for Rectangle static methods.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    class Rectangle<TValue> where TValue : IComparable<TValue>
    {
        /// <summary>
        /// Checks whether the point is in a rectangle.
        /// </summary>
        /// <param name="point">Point given.</param>
        /// <param name="corner1">Lower-left corner of rectangle</param>
        /// <param name="corner2">Upper-right corner of rectangle</param>
        /// <returns>Whether the point is in a rectangle</returns>
        public static bool PointInRectangle(ValueTuple<TValue, TValue> point, ValueTuple<TValue, TValue> corner1, ValueTuple<TValue, TValue> corner2)
        {
            bool inX = (point.Item1.CompareTo(corner1.Item1) >= 0 && point.Item1.CompareTo(corner2.Item1) <= 0);
            bool inY = (point.Item2.CompareTo(corner1.Item2) >= 0 && point.Item2.CompareTo(corner2.Item2) <= 0);
            return inX && inY;
        }
    }

    /// <summary>
    /// X-coordinate value tuple comparer.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    class XCoordinateComparer<TValue> : IComparer<ValueTuple<TValue, TValue>>
        where TValue : IComparable<TValue>
    {
        /// <summary>
        /// Compares first item of two tuples.
        /// </summary>
        /// <param name="x">First tuple</param>
        /// <param name="y">Second tuple</param>
        /// <returns></returns>
        public int Compare((TValue, TValue) x, (TValue, TValue) y)
        {
            return x.Item1.CompareTo(y.Item1);
        }
    }

    /// <summary>
    /// Y-coordinate value tuple comparer.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    class YCoordinateComparer<TValue> : IComparer<ValueTuple<TValue, TValue>>
       where TValue : IComparable<TValue>
    {
        /// <summary>
        /// Compares second item of two tuples.
        /// </summary>
        /// <param name="x">First tuple</param>
        /// <param name="y">Second tuple</param>
        /// <returns></returns>
        public int Compare((TValue, TValue) x, (TValue, TValue) y)
        {
            return x.Item2.CompareTo(y.Item2);
        }
    }
    #endregion

    class RangeTreeNode<TValue> where TValue : IComparable<TValue>
    {
        public ValueTuple<TValue, TValue> Value { get; private set; }
        public RangeTreeNode<TValue> Parent { get; private set; }
        public RangeTreeNode<TValue> Left { get; private set; }
        public List<RangeTreeNode<TValue>> MiddleSons { get; set; } = new List<RangeTreeNode<TValue>>();
        public RangeTreeNode<TValue> Right { get; private set; }
        public RangeTree<TValue> OtherDimensionSubtree { get; set; }
        public int SubtreeNodesCount { get; set; } = 1;
        public int SortingDimension { get; private set; }
        public double Alpha { get; private set; }
        public bool IsLeaf { get { return (Left == null) && (Right == null); } }
        public bool IsRoot { get { return Parent == null; } }

        public RangeTreeNode(ValueTuple<TValue, TValue> value, double alpha, int sort)
        {
            Value = value;
            Alpha = alpha;
            SortingDimension = sort;
            if (sort == 0)
            {
                OtherDimensionSubtree = new RangeTree<TValue>(Alpha);
                OtherDimensionSubtree.Insert(value, 1);
            }
        }

        /// <summary>
        /// String representation for debug.
        /// </summary>
        /// <returns>String representation.</returns>
        public override string ToString()
        {
            var leftVal = (Left == null)? "()" : Left.Value.ToString();
            var rightVal = (Right == null) ? "()" : Right.Value.ToString();
            return $"{leftVal}<-{Value}->{rightVal}";
        }

        #region Balance and rebuilding
        /// <summary>
        /// Checks whether the node is balanced.
        /// </summary>
        /// <returns>Whether the node is balanced</returns>
        public bool IsBalanced()
        {
            double left = (Left == null) ? 0 : Left.SubtreeNodesCount;
            double right = (Right == null) ? 0 : Right.SubtreeNodesCount;
            return (left <= Alpha * SubtreeNodesCount && right <= Alpha * SubtreeNodesCount);
        }
        /// <summary>
        /// Get subtree points in order.
        /// </summary>
        /// <returns>List of subtree points in order.</returns>
        public List<ValueTuple<TValue, TValue>> GetSubtreeValuesInOrder()
        {
            var list = new List<ValueTuple<TValue, TValue>>();
            if (Left != null) { list.AddRange(Left.GetSubtreeValuesInOrder()); }
            list.Add(Value);
            foreach (var node in MiddleSons)
            {
                list.Add(node.Value);
            }
            if (Right != null) { list.AddRange(Right.GetSubtreeValuesInOrder()); }
            return list;
        }
        /// <summary>
        /// Get subtree nodes in order.
        /// </summary>
        /// <returns>list of subtree nodes in order</returns>
        public List<RangeTreeNode<TValue>> GetSubtreeNodesInOrder()
        {
            var list = new List<RangeTreeNode<TValue>>();
            if (Left != null) { list.AddRange(Left.GetSubtreeNodesInOrder()); }
            list.Add(this);
            foreach (var node in MiddleSons)
            {
                list.Add(node);
            }
            if (Right != null) { list.AddRange(Right.GetSubtreeNodesInOrder()); }
            return list;
        }
        #endregion

        #region Range Count Query
        /// <summary>
        /// Main query method.
        /// </summary>
        /// <param name="corner1">Lower-left corner of rectangle.</param>
        /// <param name="corner2">Upper-right corner of rectangle.</param>
        /// <param name="currentCoordinate">Current sorting coordinate</param>
        /// <returns>Tuple: (Points in rectangle, Visited nodes count)</returns>
        public (int, int) Query(ValueTuple<TValue,TValue> corner1, ValueTuple<TValue,TValue> corner2, int currentCoordinate)
        {
            TValue thisVal = (currentCoordinate == 0) ? Value.Item1 : Value.Item2;
            TValue corner1Val = (currentCoordinate == 0) ? corner1.Item1 : corner1.Item2;
            TValue corner2Val = (currentCoordinate == 0) ? corner2.Item1 : corner2.Item2;
            if (thisVal.CompareTo(corner1Val) < 0)
            {
                if (Right != null)
                {
                    (int nc, int qc) = Right.Query(corner1, corner2, currentCoordinate);
                    return (nc, qc + 1);
                }
                else return (0, 1);
            }
            else if (thisVal.CompareTo(corner2Val) > 0)
            {
                if (Left != null)
                {
                    (int nc, int qc) = Left.Query(corner1, corner2, currentCoordinate);
                    return (nc, qc + 1);
                }
                else return (0, 1);
            }
            else
            {
                int thisNodeCount = GetCurrentNodesCountInRectangle(corner1, corner2);
                int queryCount = MiddleSons.Count + 1;
                if (Left != null)
                {
                    (int nc, int qc) = Left.QueryLeft(corner1, corner2, currentCoordinate);
                    thisNodeCount += nc;
                    queryCount += qc;
                }
                if (Right != null)
                {
                   (int nc, int qc) = Right.QueryRight(corner1, corner2, currentCoordinate);
                    thisNodeCount += nc;
                    queryCount += qc;
                }
                return (thisNodeCount, queryCount);
            }
        }
        /// <summary>
        /// Auxiliary query-to-left method.
        /// </summary>
        /// <param name="corner1">Lower-left corner of rectangle.</param>
        /// <param name="corner2">Upper-right corner of rectangle.</param>
        /// <param name="currentCoordinate">Current sorting coordinate</param>
        /// <returns>Tuple: (Points in rectangle, Visited nodes count)</returns>
        private (int, int) QueryLeft(ValueTuple<TValue, TValue> corner1, ValueTuple<TValue, TValue> corner2, int currentCoordinate)
        {
            TValue thisVal = (currentCoordinate == 0) ? Value.Item1 : Value.Item2;
            TValue corner1Val = (currentCoordinate == 0) ? corner1.Item1 : corner1.Item2;

            if (thisVal.CompareTo(corner1Val) < 0)
            {
                if (Right != null)
                {
                    (int nc, int qc) = Right.QueryLeft(corner1, corner2, currentCoordinate);
                    return (nc, qc + 1);
                }
                else return (0, 1);
            }
            else
            {
                int thisNodeCount = GetCurrentNodesCountInRectangle(corner1, corner2);
                int queryCount = MiddleSons.Count + 1;
                if (Left != null)
                {
                    (int nc, int qc) = Left.QueryLeft(corner1, corner2, currentCoordinate);
                    thisNodeCount += nc;
                    queryCount += qc;
                }
                if (Right != null)
                {
                    if (currentCoordinate == 0)
                    {
                        (int nc, int qc) = Right.OtherDimensionSubtree.Root.Query(corner1, corner2, 1);
                        thisNodeCount += nc;
                        queryCount += qc;
                    }
                    else
                    {
                        thisNodeCount += Right.SubtreeNodesCount;
                        queryCount++;
                    }
                }
                return (thisNodeCount, queryCount);
            }

        }
        /// <summary>
        /// Auxiliary query-to-right method.
        /// </summary>
        /// <param name="corner1">Lower-left corner of rectangle.</param>
        /// <param name="corner2">Upper-right corner of rectangle.</param>
        /// <param name="currentCoordinate">Current sorting coordinate</param>
        /// <returns>Tuple: (Points in rectangle, Visited nodes count)</returns>
        private (int, int) QueryRight(ValueTuple<TValue, TValue> corner1, ValueTuple<TValue, TValue> corner2, int currentCoordinate)
        {
            TValue thisVal = (currentCoordinate == 0) ? Value.Item1 : Value.Item2;
            TValue corner2Val = (currentCoordinate == 0) ? corner2.Item1 : corner2.Item2;

            if (thisVal.CompareTo(corner2Val) > 0)
            {
                if (Left != null)
                {
                    (int nc, int qc) =  Left.QueryRight(corner1, corner2, currentCoordinate);
                    return (nc, qc + 1);
                }
                else return (0, 1);
            }
            else
            {
                int thisNodeCount = GetCurrentNodesCountInRectangle(corner1, corner2);
                int queryCount = MiddleSons.Count + 1;
                if (Right != null)
                {
                    (int nc, int qc) = Right.QueryRight(corner1, corner2, currentCoordinate);
                    thisNodeCount += nc;
                    queryCount += qc;
                }
                if (Left != null)
                {
                    if (currentCoordinate == 0)
                    {
                        (int nc, int qc) = Left.OtherDimensionSubtree.Root.Query(corner1, corner2, 1);
                        thisNodeCount += nc;
                        queryCount += qc;
                    }
                    else
                    {
                        thisNodeCount += Left.SubtreeNodesCount;
                        queryCount++;
                    }
                }
                return (thisNodeCount, queryCount);
            }
        }
        /// <summary>
        /// Auxiliary query method to get number of points in rectangle among middle sons.
        /// </summary>
        /// <param name="corner1">Lower-left corner</param>
        /// <param name="corner2">Upper-right corner</param>
        /// <returns>Number of points in rectangle among middle sons</returns>
        private int GetCurrentNodesCountInRectangle(ValueTuple<TValue, TValue> corner1, ValueTuple<TValue, TValue> corner2)
        {
            int buffer = 0;
            if (Rectangle<TValue>.PointInRectangle(Value, corner1, corner2)) { buffer++; }
            foreach (var val in MiddleSons)
            {
                if (Rectangle<TValue>.PointInRectangle(val.Value, corner1, corner2)) { buffer++; }
            }
            return buffer;
        }
        #endregion

        #region Children handling
        /// <summary>
        /// Sets new left son.
        /// </summary>
        /// <param name="left">New left son</param>
        public void SetLeftSon(RangeTreeNode<TValue> left)
        {
            if (Left != null) { SubtreeNodesCount -= Left.SubtreeNodesCount; }
            Left = left;
            if (left == null) { return; }
            left.Parent = this;
            SubtreeNodesCount += left.SubtreeNodesCount;
        }
        /// <summary>
        /// Sets new right son.
        /// </summary>
        /// <param name="right">New right son</param>
        public void SetRightSon(RangeTreeNode<TValue> right)
        {
            if (Right != null) { SubtreeNodesCount -= Right.SubtreeNodesCount; }
            Right = right;
            if (right == null) { return; }
            right.Parent = this;           
            SubtreeNodesCount += right.SubtreeNodesCount;
        }
        /// <summary>
        /// Sets middle sons.
        /// </summary>
        /// <param name="middleSons">New middle sons list.</param>
        public void SetMiddleSons(List<RangeTreeNode<TValue>> middleSons)
        {
            SubtreeNodesCount -= MiddleSons.Count;
            MiddleSons = middleSons;
            foreach (var son in middleSons)
            {
                son.Parent = this;
                SubtreeNodesCount++;
            }
        }
        /// <summary>
        /// Add new middle son.
        /// </summary>
        /// <param name="middleSon">New middle son</param>
        public void AddMiddleSon(RangeTreeNode<TValue> middleSon)
        {
            middleSon.Parent = this;
            SubtreeNodesCount++;
            MiddleSons.Add(middleSon);
        }
        #endregion

        #region Validation
        /// <summary>
        /// Validates this node.
        /// </summary>
        public void Validate()
        {
            // tree connections
            if (Left != null)
            {
                Debug.Assert(Left.Parent == this);
            }
            if (Right != null)
            {
                Debug.Assert(Right.Parent == this);
            }
            foreach (var son in MiddleSons)
            {
                Debug.Assert(son.Parent == this);
            }

            // nodes count
            double left = (Left == null) ? 0 : Left.SubtreeNodesCount;
            double right = (Right == null) ? 0 : Right.SubtreeNodesCount;
            Debug.Assert(SubtreeNodesCount == left + right + 1 + MiddleSons.Count);
            if (SortingDimension == 0) { Debug.Assert(SubtreeNodesCount == OtherDimensionSubtree.Root.SubtreeNodesCount); }
            
            // other dimension subtree
            if (SortingDimension == 0)
            {
                OtherDimensionSubtree.ValidateTree();
            }

            // bb tree property
            Debug.Assert(Alpha != 0);
            Debug.Assert(IsBalanced());
        }
        #endregion
    }
}
