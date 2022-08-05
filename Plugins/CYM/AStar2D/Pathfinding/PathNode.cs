using System.Collections.Generic;
using UnityEngine;

namespace CYM.AStar2D
{
    /// <summary>
    /// Internal class used to store more detailed information about a specified <see cref="IBaseNode"/> instance.
    /// The class acts as a wrapper and is passed to the active <see cref="AStar2D.Pathfinding.Algorithm.HeuristicProvider"/> in order to calculate the heuristic and adjacent distances.
    /// </summary>
    public sealed class PathNode : IComparer<PathNode>, IBaseNode
    {
        // Private
        private Index index = Index.Zero;
        private IBaseNode tileNode = null;
        private int nodeIndex = 0;

        // Public
        /// <summary>
        /// The h value for this node.
        /// </summary>
        public float h = 0;
        /// <summary>
        /// The g value for this node.
        /// </summary>
        public float g = 0;
        /// <summary>
        /// The f value for this node.
        /// </summary>
        public float f = 0;

        // Properties
        public Index Index => index;
        public IBaseNode TileNode => tileNode;
        /// <summary>
        /// Sorting value used by the algorithm to determine how viable a node is.
        /// </summary>
        public int TileNodeIndex
        {
            get { return nodeIndex; }
            set { nodeIndex = value; }
        }

        /// <summary>
        /// Is the node walkable.
        /// </summary>
        public bool IsWalkable => tileNode.IsWalkable;
        /// <summary>
        /// The weighting value for the node.
        /// </summary>
        public float Weighting => tileNode.Weighting;
        /// <summary>
        /// The position in 3D space of the node.
        /// </summary>
        public Vector3 Pos => tileNode.Pos;
        /// <summary>
        /// Get the diagonal mode used by this node.
        /// </summary>
        public PathNodeDiagonalMode DiagonalMode => tileNode.DiagonalMode;
        // Constructor
        internal PathNode(Index index, IBaseNode node)
        {
            this.index = index;
            this.tileNode = node;
        }

        // Methods
        /// <summary>
        /// Implementation of IComparer interface.
        /// </summary>
        /// <param name="a">The first <see cref="PathNode"/></param>
        /// <param name="b">The second <see cref="PathNode"/></param>
        /// <returns>-1 if a is better than b. 1 if b is better than a. 0 if both are equal</returns>
        public int Compare(PathNode a, PathNode b)
        {
            if (a.f < b.f)
            {
                // A is smaller
                return -1;
            }
            else if (a.f > b.f)
            {
                // A is larger
                return 1;
            }

            return 0;
        }

        public bool IsShowInDebugView()
        {
            return true;
        }
    }
}
