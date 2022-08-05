using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CYM.AStar2D
{
    /// <summary>
    /// Represents a series of nodes that make up a path to the destination.
    /// Provides additional helper methods for path traversal.
    /// </summary>
    public sealed class Path : IEnumerable<IBaseNode>
    {
        // Private       
        private SearchGrid grid = null;
        private IBaseNode lastNode = null;
        private IBaseNode startNode = null;

        private Vector3[] cachedVectorArray = null;
        private Index[] cachedIndexArray = null;



        // Properties
        public List<IBaseNode> PathNodes { get; private set; } = new List<IBaseNode>();
        public HashList<IBaseNode> Nodes { get; private set; } = new HashList<IBaseNode>();
        /// <summary>
        /// Can the path be reached.
        /// </summary>
        public bool IsReachable
        {
            get { return AllNodesWalkable(); }
        }

        /// <summary>
        /// Can the path be reached.
        /// This property will also ensure that no <see cref="Obstacle"/> are blocking the path 
        /// </summary>
        public bool IsFullyReachable
        {
            get { return AllNodesWalkable(true); }
        }

        /// <summary>
        /// Does the path contain any nodes.
        /// </summary>
        public bool IsEmpty
        {
            get { return PathNodes.Count == 0; }
        }

        /// <summary>
        /// The number of nodes that make up this path.
        /// </summary>
        public int NodeCount
        {
            get { return PathNodes.Count; }
        }

        /// <summary>
        /// The first node in the path.
        /// </summary>
        public IBaseNode StartNode
        {
            get { return startNode; }
        }

        /// <summary>
        /// The last node in the path.
        /// </summary>
        public IBaseNode LastNode
        {
            get { return lastNode; }
        }

        // Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        internal Path(SearchGrid grid)
        {
            this.grid = grid;
        }

        // Methods
        internal void Push(IBaseNode node)
        {
            if (IsEmpty == true)
                startNode = node;
            lastNode = node;
            PathNodes.Add(node);
            if (node.TileNode != null)
                Nodes.Add(node.TileNode);
        }

        /// <summary>
        /// Attempts to get the <see cref="PathRouteNode"/> at the specified index. 
        /// </summary>
        /// <param name="index">The index of the node</param>
        /// <returns>A <see cref="PathRouteNode"/> or null if the index was invalid</returns>
        public IBaseNode GetNode(int index)
        {
            // Validate index
            if (index >= 0 && index < NodeCount)
                return PathNodes[index];

            // Node not found
            return null;
        }

        /// <summary>
        /// Returns an array of <see cref="Vector3"/> representing each world position waypoint in the path.
        /// The first call will create a cached array which will be returned in subsequent calls.
        /// </summary>
        /// <returns>An array of world position waypoints</returns>
        public Vector3[] ToVectorArray()
        {
            // Check for a cached array
            if (cachedVectorArray == null)
            {
                List<Vector3> list = new List<Vector3>();

                // Add to the list
                foreach (IBaseNode node in this)
                    list.Add(node.Pos);

                // Get as array
                cachedVectorArray = list.ToArray();
            }

            // Use the cached version
            return cachedVectorArray;
        }

        /// <summary>
        /// Returns an array of <see cref="Index"/> representing the index of each waypoint node in the path.
        /// The first call will create a cached array which will be returned in subsequent calls.
        /// </summary>
        /// <returns>An array of grid indexes representing this path</returns>
        public Index[] ToIndexArray()
        {
            // Check for a cached array
            if (cachedIndexArray == null)
            {
                List<Index> list = new List<Index>();

                // Add to the list
                foreach (IBaseNode node in this)
                    list.Add(node.Index);

                // Get as array
                cachedIndexArray = list.ToArray();
            }

            // Use the cached version
            return cachedIndexArray;
        }

        private bool AllNodesWalkable(bool checkDynamicObstacles = false)
        {
            // If any path is not walkable then exits early with failure
            foreach (IBaseNode node in PathNodes)
            {
                if (node.IsWalkable == false)
                    return false;

                // Check for dynamic obstacles
                if (checkDynamicObstacles == true)
                {
                    // Check for valid grid
                    if (grid != null && grid.IsIndexObstacle != null)
                    {
                        // Check if the node is obstructed
                        if (grid.IsIndexObstacle(node.Index) == true)
                            return false;
                    }
                }
            }

            // The path can be walked until the end
            return true;
        }

        /// <summary>
        /// Overriden to string method.
        /// </summary>
        /// <returns>This <see cref="Path"/> as a string representation</returns>
        public override string ToString()
        {
            return ToString(false);
        }

        /// <summary>
        /// Additional to string method.
        /// </summary>
        /// <param name="detailed">Should detailed information for the path be included</param>
        /// <returns>This <see cref="Path"/> as a string representation</returns>
        public string ToString(bool detailed)
        {
            if (detailed == true)
            {
                StringBuilder builder = new StringBuilder();

                builder.AppendLine(string.Format("Path: ({0})", PathNodes.Count));

                foreach (IBaseNode node in PathNodes)
                    builder.AppendLine(string.Format("\tNode: ({0}, {1})", node.Pos.x, node.Pos.y));

                return builder.ToString();
            }
            else
            {
                return string.Format("Path: ({0})", PathNodes.Count);
            }
        }

        /// <summary>
        /// IEnumerator implementation.
        /// </summary>
        /// <returns>The enumerator for the inner collection</returns>
        public IEnumerator<IBaseNode> GetEnumerator()
        {
            return PathNodes.GetEnumerator();
        }

        /// <summary>
        /// IEnumerator implementation.
        /// </summary>
        /// <returns>The enumerator for the inner collection</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return PathNodes.GetEnumerator();
        }
    }
}
