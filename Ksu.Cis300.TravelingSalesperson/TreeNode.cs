/*
 * TreeNode.cs
 * Author: Matthew Wilson
 */
using Accessibility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ksu.Cis300.TravelingSalesperson
{
    /// <summary>
    /// Defines a tree node for a point in a circuit
    /// </summary>
    public class TreeNode
    {
        /// <summary>
        /// Minimun number of points for circuit.
        /// </summary>
        public static readonly int _minNumPointsForCircuit = 3;

        /// <summary>
        /// points to be included in the circuit
        /// </summary>
        private readonly Point[] _points;

        /// <summary>
        /// the distances between each point
        /// </summary>
        private readonly double[,] _distance;

        /// <summary>
        /// Points included on the current path
        /// </summary>
        private readonly int[] _pathIndices;

        /// <summary>
        /// Points yet to be added to the path
        /// </summary>
        private readonly int[] _remainingIndices;

        /// <summary>
        /// Length of the current path
        /// </summary>
        private readonly double _length;

        /// <summary>
        /// Whether all of the circuits should be found
        /// </summary>
        private readonly bool _findAllCircuits;

        /// <summary>
        /// Lower bound to help prune the potential paths
        /// </summary>
        public double LowerBound
        {
            get;
            private set;
        }

        /// <summary>
        /// Whether or not a circuit is complete
        /// </summary>
        public bool IsComplete
        {
            get
            {
                if (_remainingIndices.Length == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// gets the points on the path
        /// </summary>
        public Point[] Path
        {
            get
            {
                Point[] temp = new Point[_pathIndices.Length];
                for (int i = 0; i < _pathIndices.Length; i++)
                {
                    temp[i] = _points[_pathIndices[i]];
                }
                return temp;
            }
        }

        /// <summary>
        /// Children for each point
        /// </summary>
        public List<TreeNode> Children
        {
            get
            {
                if (IsComplete)
                {
                    return new List<TreeNode>();
                }
                else
                {
                    List<TreeNode> children = new List<TreeNode>();
                    int loopStart = 1;
                    if (_findAllCircuits)
                    {
                        loopStart = 0;
                    }
                    children.Add(GetChild(_remainingIndices.Length - 1));
                    for (int i = loopStart; i < _remainingIndices.Length - 1; i++)
                    {

                        children.Add(GetChild(i));
                    }
                    return children;
                }
            }
        }

        public TreeNode(List<Point> temp)
        {
            if (temp == null)
            {
                throw new ArgumentNullException();
            }
            if (temp.Count < _minNumPointsForCircuit)
            {
                throw new ArgumentException();
            }
            _distance = FindDistance(temp);
            _points = temp.ToArray();
            _pathIndices = new int[1];
            _pathIndices[0] = temp.Count - 1;
            _remainingIndices = new int[temp.Count - 1];
            for (int i = 0; i < temp.Count-1; i++)
            {
                _remainingIndices[i] = i;
            }
            _length = 0;
            _findAllCircuits = false;
            LowerBound = FindLowerBound();
        }

        private TreeNode(TreeNode parent, int[] pointsOnPath, int[] pointsOffPath, double length, bool findAll)
        {
            _pathIndices = pointsOnPath;
            _remainingIndices = pointsOffPath;
            _length = length;
            _findAllCircuits = findAll;
            _points = parent._points;
            _distance = parent._distance;
            LowerBound = FindLowerBound();
        }

        /// <summary>
        /// Finds the distances between each point.
        /// </summary>
        /// <param name="points">Points in the circuit</param>
        /// <returns>the distances of each point</returns>
        private double[,] FindDistance(List<Point> points)
        {
            double[,] distances = new double[points.Count, points.Count];
            for (int i = 0; i < points.Count; i++)
            {
                for (int j = 0; j < points.Count; j++)
                {
                    if (i == j)
                    {
                        distances[i, j] = double.PositiveInfinity;
                    }
                    else
                    {
                        distances[i, j] = Distance(points[i], points[j]);
                    }
                }
            }
            return distances;
        }

        /// <summary>
        /// Finds the distance between two specific points
        /// </summary>
        /// <param name="one">First point</param>
        /// <param name="two">Second point</param>
        /// <returns>distance between the two points</returns>
        private double Distance(Point one, Point two)
        {
            double x = Math.Pow(one.X - two.X, 2);
            double y = Math.Pow(one.Y - two.Y,2);
            return Math.Sqrt(x + y);
        }

        /// <summary>
        /// Finds the lower bound for a given circuit
        /// </summary>
        /// <returns>the lower bound for a given circuit</returns>
        private double FindLowerBound()
        {
            double distanceStore = _length;
            foreach(int i in _remainingIndices)
            {
                distanceStore += FindShortestDistance(i);
            }
            distanceStore += FindShortestDistance(_pathIndices[0]);
            return distanceStore;
        }

        /// <summary>
        /// Finds the shortest distance two another point from a point
        /// </summary>
        /// <param name="index">the starting point</param>
        /// <returns>the shortest distance</returns>
        private double FindShortestDistance(int index)
        {
            double currentMin = _distance[index, _pathIndices[_pathIndices.Length - 1]];

            for (int i = 0; i< _remainingIndices.Length; i++)
            {
                currentMin = Math.Min(currentMin, _distance[index, _remainingIndices[i]]);
            }
            return currentMin;
        }
        
        /// <summary>
        /// Gets the child of a specific index
        /// </summary>
        /// <param name="i">index</param>
        /// <returns>the child</returns>
        private TreeNode GetChild(int i)
        {
            int[] indiciesOfPath = new int[_pathIndices.Length + 1];
            int[] indiciesNotInPath = new int[_remainingIndices.Length - 1];
            _pathIndices.CopyTo(indiciesOfPath, 0);
            indiciesOfPath[indiciesOfPath.Length - 1] = _remainingIndices[i];
            int temp = 0;
            for (int j = 0; j < _remainingIndices.Length; j++)
            {
                if (i != j)
                {
                    indiciesNotInPath[temp] = _remainingIndices[j];
                    temp++;
                }
            }

            double otherLength = _length + _distance[_remainingIndices[i], _pathIndices[_pathIndices.Length - 1]];

            TreeNode other = new TreeNode(this, indiciesOfPath, indiciesNotInPath, otherLength, _findAllCircuits || i == 1);
            return other;
        }
    }
}
