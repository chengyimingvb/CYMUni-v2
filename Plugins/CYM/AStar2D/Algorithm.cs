using UnityEngine;

namespace CYM.AStar2D
{
    /// <summary>
    /// Provides a Euclidean heuristic.
    /// </summary>
    public class EuclideanProvider : HeuristicProvider
    {
        // Methods
        /// <summary>
        /// Calcualtes the Euclidean heuristic.
        /// </summary>
        /// <param name="start">The first node</param>
        /// <param name="end">The second node</param>
        /// <returns>The heuristic between the 2 nodes</returns>
        public override float heuristic(PathNode start, PathNode end)
        {
            float x = Mathf.Pow(end.Index.X - start.Index.X, 2);
            float y = Mathf.Pow(end.Index.Y - start.Index.Y, 2);

            // Require sqrt
            return Mathf.Sqrt(x + y);
        }
    }

    /// <summary>
    /// Provider class that may be inherited to provide alternative heuirstic and adjacent calculations.
    /// The default heuristic is Euclidean.
    /// </summary>
    public abstract class HeuristicProvider
    {
        // Public
        /// <summary>
        /// Cached value representing the square root of '2'.
        /// </summary>
        public static readonly float squareDistance = Mathf.Sqrt(2);
        /// <summary>
        /// Cached value representing the default Euclidean heuristic provide.
        /// </summary>
        public static readonly HeuristicProvider defaultProvider = new EuclideanProvider();

        // Methods
        /// <summary>
        /// Heuristic method that must be implemented by the inheriting class.
        /// </summary>
        /// <param name="start">The <see cref="PathNode"/> representing the start position</param>
        /// <param name="end">The <see cref="PathNode"/> representing the end position</param>
        /// <returns>A float value representing the heuristic between the nodes</returns>
        public abstract float heuristic(PathNode start, PathNode end);

        /// <summary>
        /// Adjecant method that can be overidden to provide alternative behaviour.
        /// </summary>
        /// <param name="start">The <see cref="PathNode"/> representing the start position</param>
        /// <param name="end">The <see cref="PathNode"/> representing the end position</param>
        /// <returns>A float value representing the distance between the two nodes</returns>
        public virtual float adjacentDistance(PathNode start, PathNode end)
        {
            // Calculate the difference
            int x = Mathf.Abs(start.Index.X - end.Index.X);
            int y = Mathf.Abs(start.Index.Y - end.Index.Y);

            switch (x + y)
            {
                default:
                case 0:
                    return 0;

                case 1:
                    return 1;

                case 2:
                    return squareDistance;
            }
        }
    }
}
