/* CircuitFinder.cs
 * Author: Matthew Wilson
 * 
 */
using Ksu.Cis300.PriorityQueueLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ksu.Cis300.TravelingSalesperson
{
    /// <summary>
    /// A class containing methods for finding a shortest circuit through a set of points.
    /// </summary>
    public static class CircuitFinder
    {
        /// <summary>
        /// Minimun number of points for circuit.
        /// </summary>
        public static readonly int _minNumPointsForCircuit = 3;

        /// <summary>
        /// Computes the distance between the given points.
        /// </summary>
        /// <param name="a">The first point.</param>
        /// <param name="b">The second point.</param>
        /// <returns>The distance from a to b.</returns>
        private static double Distance(Point a, Point b)
        {
            int diffX = a.X - b.X;
            int diffY = a.Y - b.Y;
            return Math.Sqrt(diffX * diffX + diffY * diffY);
        }

        /// <summary>
        /// Removes and returns the last point in the given list.
        /// </summary>
        /// <param name="points">The list of points.</param>
        /// <returns>The point removed.</returns>
        private static Point RemoveLast(List<Point> points)
        {
            Point last = points[points.Count - 1];
            points.RemoveAt(points.Count - 1);
            return last;
        }

        /// <summary>
        /// Finishes the given path to form a minimum-length circuit in which unused[1] precedes unused[0]
        /// if unused contains more than one point. If the length is greater than or 
        /// equal to the given bound, the circuit may not be complete.
        /// </summary>
        /// <param name="path">The path to complete.</param>
        /// <param name="unused">The points that remain to be included in the circuit.</param>
        /// <param name="len">The length of the path to complete.</param>
        /// <param name="bound">The bound on useful circuit lengths.</param>
        /// <param name="useAll">Indicates whether all permutations should be used when unused has more than one
        /// point. If false, no permutation in which unused[0] precedes unused[1] will be considered. Has no
        /// effect if unused has only one point.</param>
        /// <param name="circuit">The minimum-length circuit.</param>
        /// <returns>The length of the circuit found.</returns>
        private static double FinishShortestCircuit(List<Point> path, List<Point> unused, double len,
            double bound, bool useAll, out Point[] circuit)
        {
            double d = Distance(path[path.Count - 1], path[0]);
            if (unused.Count == 0 || len + d >= bound)
            {
                circuit = path.ToArray();
                return len + d;
            }
            Point end = path[path.Count - 1];
            Point p = RemoveLast(unused);
            path.Add(p);
            double first = FinishShortestCircuit(path, unused, len + Distance(end, p), bound, useAll, 
                out circuit);
            RemoveLast(path);
            bound = Math.Min(bound, first);
            int start = 1;
            if (useAll)
            {
                start = 0;
            }
            for (int i = start; i < unused.Count; i++)
            {
                Point temp = unused[i];
                path.Add(temp);
                unused[i] = p;
                double cLen = FinishShortestCircuit(path, unused, len + Distance(end, temp), bound, 
                    useAll || i == 1, out Point[] c);
                unused[i] = RemoveLast(path);
                if (cLen < bound)
                {
                    bound = cLen;
                    circuit = c;
                }
            }
            unused.Add(p);
            return bound;
        }

        /// <summary>
        /// Finds a shortest circuit containing the each of the given points exactly once.
        /// </summary>
        /// <param name="points">The points through which the circuit should be formed.</param>
        /// <param name="circuit">The shortest circuit.</param>
        /// <returns>The length of the shortest circuit.</returns>
        public static double FindShortestCircuit(List<Point> points, out Point[] circuit)
        {
            if (points == null)
            {
                throw new ArgumentNullException();
            }
            if (points.Count < 3)
            {
                throw new ArgumentException();
            }
            MinPriorityQueue<double, TreeNode> temp = new MinPriorityQueue<double, TreeNode>();
            TreeNode other = new TreeNode(points);
            while (!other.IsComplete)
            {
                List<TreeNode> listy = other.Children;
                for (int i = 0; i < listy.Count; i++)
                {
                    temp.Add(listy[i].LowerBound, listy[i]);
                }
                other = temp.RemoveMinPriorityElement();
            }
            circuit = other.Path;
            return other.LowerBound;
        }
    }
}
