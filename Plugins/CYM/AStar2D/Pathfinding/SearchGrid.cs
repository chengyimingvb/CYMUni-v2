using System;
using UnityEngine;

namespace CYM.AStar2D
{
    /// <summary>
    /// The different types of diagonal movement that are allowed.
    /// </summary>
    public enum DiagonalMode
    {
        /// <summary>
        /// Paths containing diagonal movement will not be considered.
        /// </summary>
        NoDiagonal,
        /// <summary>
        /// Paths containing diagonal movement will be considered.
        /// </summary>
        Diagonal,
        /// <summary>
        /// Paths containing diagonal movement will be considered but situations where corner cutting may occur will default to non-diagonal movement.
        /// </summary>
        DiagonalNoCutting,
    }

    /// <summary>
    /// Represents a grid that can be used for pathfinding.
    /// Use this class if you want to achieve pathfinding without relying on Unity components.
    /// </summary>
    public class SearchGrid
    {
        #region Events
        //检查Node是否被占领
        public Func<Index, bool> IsIndexObstacle;
        #endregion

        #region Private
        private NodeQueue<PathNode> orderedMap = null;
        private OpenNodeMap<PathNode> closedMap = null;
        private OpenNodeMap<PathNode> openMap = null;
        private OpenNodeMap<PathNode> runtimeMap = null;

        private PathNode[,] nodeGrid = null;
        private PathNode[,] searchGrid = null;
        private IBaseNode[] adjacentNodes = new IBaseNode[8]; // Allocate once and reuse (8 possible directions)
        private IBaseNode[] connectionNodes = new IBaseNode[8]; // Allocate once and reuse (8 possible directions)
        private int width = 0;
        private int height = 0;
        private float nodeSpacing = 0.2f;
        private float weightingInfluence = 1;
        #endregion

        #region Public
        // The heuristic method to use.
        public HeuristicProvider provider = HeuristicProvider.defaultProvider;
        // The maximum amount of nodes that a path should contain. Use -1 for an unlimited node count.
        public int maxPathLength = -1;
        #endregion

        #region Properties
        /// <summary>
        /// Attempts to access an element of the grid at the specified index.
        /// </summary>
        /// <param name="x">The X component of the index</param>
        /// <param name="y">The Y component of the index</param>
        /// <returns>The <see cref="IBaseNode"/> at the specified index</returns>
        public IBaseNode this[int x, int y]
        {
            get
            {
                if (x >= Width || y >= Height) return null;
                if (x < 0 || y < 0) return null;
                return nodeGrid[x, y];
            }
        }
        //宽度
        public int Width => width;
        //高度
        public int Height => height;
        /// <summary>
        /// The distance between 2 nodes. Settings this value can dramaticaly increase the performance of <see cref="FindNearestIndex(Vector3)"/>.
        /// As it will prevent an exhaustive search if the method has already found the best matching node.
        /// Only set this value if the user node grid has equal spacing in both the X and Y axis.
        /// </summary>
        public float NodeSpacing
        {
            get { return nodeSpacing; }
            set { nodeSpacing = value; }
        }
        /// <summary>
        /// Determines how much the <see cref="IBaseNode.Weighting"/> will influence the path. 
        /// </summary>
        public float WeightingInfluence
        {
            get { return weightingInfluence; }
            set { weightingInfluence = value; }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Create a <see cref="SearchGrid"/> provider based on the specified input grid. 
        /// </summary>
        /// <param name="inputNodes">The 2D input array of <see cref="IBaseNode"/></param>
        public SearchGrid(IBaseNode[,] inputNodes)
        {
            // Make sur ethe input is acceptable
            ValidateInputGrid(inputNodes);

            // Get sizes
            width = inputNodes.GetLength(0);
            height = inputNodes.GetLength(1);

            // Cache and allocate
            nodeGrid = new PathNode[width, height];
            searchGrid = new PathNode[width, height];

            closedMap = new OpenNodeMap<PathNode>(width, height);
            openMap = new OpenNodeMap<PathNode>(width, height);
            runtimeMap = new OpenNodeMap<PathNode>(width, height);
            orderedMap = new NodeQueue<PathNode>(new PathNode(Index.Zero, null));

            // Create the grid wrapper
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Create a wrapper path node over the interface
                    nodeGrid[x, y] = new PathNode(new Index(x, y), inputNodes[x, y]);
                }
            }

            void ValidateInputGrid(IBaseNode[,] grid)
            {
                // CHeck for null arrays
                if (grid == null)
                    throw new ArgumentException("A search grid cannot be created from a null reference");

                // Check for 0 lenght arrays
                if (grid.GetLength(0) == 0 || grid.GetLength(1) == 0)
                    throw new ArgumentException("A search grid cannot be created because one or more dimensions have a length of 0");
            }
        }
        #endregion

        #region find path
        public Index FindNearestIndex(Vector3 worldPosition)
        {
            Index index = new Index(0, 0);
            float closest = float.MaxValue;
            float sqrSpacing = Mathf.Pow(nodeSpacing, 2);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 a = nodeGrid[x, y].Pos;
                    float elementX = (a.x - worldPosition.x);
                    float elementY = (a.y - worldPosition.y);
                    float elementZ = (a.z - worldPosition.z);
                    float sqrDistance = (elementX * elementX + elementY * elementY + elementZ * elementZ);
                    if (sqrDistance < closest)
                    {
                        index = nodeGrid[x, y].Index;
                        closest = sqrDistance;
                        if (sqrDistance < sqrSpacing)
                            return index;
                    }
                }
            }

            return index;
        }
        public void FindPath(Index start, Index end, DiagonalMode diagonal, BaseTraversal2D traversal, PathRequestDelegate callback)
        {

            // Already at the destination
            if (start.Equals(end))
            {
                callback(null, PathRequestStatus.SameStartEnd);
                return;
            }

            // Get the nodes
            PathNode startNode = nodeGrid[start.X, start.Y];
            PathNode endNode = nodeGrid[end.X, end.Y];

            // Clear all previous data
            ClearSearchData();

            // Starting scores
            startNode.g = 0;
            startNode.h = provider.heuristic(startNode, endNode);
            startNode.f = startNode.h;

            // Add the start node
            openMap.add(startNode);
            runtimeMap.add(startNode);
            orderedMap.Push(startNode);

            while (openMap.Count > 0)
            {
                // Get the front value
                PathNode value = orderedMap.Pop();

                if (value == endNode)
                {
                    // We have found the path
                    Path result = ConstructPath(searchGrid[endNode.Index.X, endNode.Index.Y]);

                    // Last node
                    if (maxPathLength == -1 || result.NodeCount < maxPathLength)
                        result.Push(endNode);

                    // Trigger the delegate with success
                    callback(result, PathRequestStatus.PathFound);

                    // Exit the method
                    return;
                }
                else
                {
                    openMap.remove(value);
                    closedMap.add(value);

                    // Fill our array with surrounding nodes
                    ConstructAdjacentNodes(value, adjacentNodes, diagonal);

                    // Process each neighbor
                    foreach (PathNode pathNode in adjacentNodes)
                    {
                        bool isBetter = false;

                        // Skip null nodes
                        if (pathNode == null)
                            continue;

                        // Make sure the node is walkable
                        if (pathNode.IsWalkable == false)
                            continue;

                        // Check for occupied
                        if (IsIndexObstacle != null)
                            if (IsIndexObstacle(pathNode.Index) == true)
                                continue;

                        //检查Traversal
                        if (traversal != null)
                        {
                            if (traversal.Filter(pathNode.TileNode) == false)
                                continue;
                        }

                        // Make sure it has not already been excluded
                        if (closedMap.contains(pathNode) == true)
                            continue;

                        // Check for custom exclusion descisions
                        if (ValidateConnection(value, pathNode) == false)
                            continue;

                        // Calculate the score for the node
                        float score = runtimeMap[value].g + provider.adjacentDistance(value, pathNode) + (pathNode.Weighting * weightingInfluence);
                        bool added = false;

                        // Make sure it can be added to the open map
                        if (openMap.contains(pathNode) == false)
                        {
                            openMap.add(pathNode);
                            isBetter = true;
                            added = true;
                        }
                        else if (score < runtimeMap[pathNode].g)
                        {
                            // The score is better
                            isBetter = true;
                        }
                        else
                        {
                            // The score is not better
                            isBetter = false;
                        }

                        // CHeck if a better score has been found
                        if (isBetter == true)
                        {
                            // Update the search grid
                            searchGrid[pathNode.Index.X, pathNode.Index.Y] = value;

                            // Add the adjacent node
                            if (runtimeMap.contains(pathNode) == false)
                                runtimeMap.add(pathNode);

                            // Update the score values for the node
                            runtimeMap[pathNode].g = score;
                            runtimeMap[pathNode].h = provider.heuristic(pathNode, endNode);
                            runtimeMap[pathNode].f = runtimeMap[pathNode].g + runtimeMap[pathNode].h;

                            // CHeck if we added to the open map
                            if (added == true)
                            {
                                // Push the adjacent node to the set
                                orderedMap.Push(pathNode);
                            }
                            else
                            {
                                // Refresh the set
                                orderedMap.Refresh(pathNode);
                            }
                        }
                    }

                }
            } // End while

            // Failure
            callback(null, PathRequestStatus.PathNotFound);

            void ClearSearchData()
            {
                // Reset all data
                closedMap.clear();
                openMap.clear();
                runtimeMap.clear();
                orderedMap.Clear();

                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        searchGrid[x, y] = null;
            }
            //构造路径
            Path ConstructPath(PathNode current)
            {
                // Create the path
                Path path = new Path(this);

                // Call the dee construct method
                DeepConstructPath(current, path);

                return path;
            }
            void DeepConstructPath(PathNode inputCurrent, Path output)
            {
                // Get the node from the search grid
                PathNode node = searchGrid[inputCurrent.Index.X, inputCurrent.Index.Y];

                // Make sure we have a valid node
                if (node != null)
                {
                    // Call through reccursive
                    DeepConstructPath(node, output);
                }

                // Limit the maximumnumber of nodes in the path
                if (maxPathLength != -1)
                    if (output.NodeCount > maxPathLength)
                        return;

                // Push the node to the path
                output.Push(inputCurrent);
            }
        }
        #endregion

        #region is
        //检查是否连接
        public virtual bool ValidateConnection(PathNode center, PathNode neighbor)
        {
            // Default behaviour - all nodes are valid and can be included
            return true;
        }
        #endregion

        #region utile
        //获取邻接节点
        public int ConstructAdjacentNodes(IBaseNode center, IBaseNode[] nodes, DiagonalMode diagonal)
        {
            // Get the center node
            Index node = center.Index;

            // Clear the shared array so that old data is not used
            for (int i = 0; i < nodes.Length; i++)
                nodes[i] = null;

            int index = 0;

            // Check for per node diagonal status
            if (center.DiagonalMode != PathNodeDiagonalMode.UseGlobal)
            {
                switch (center.DiagonalMode)
                {
                    case PathNodeDiagonalMode.Diagonal: diagonal = DiagonalMode.Diagonal; break;
                    case PathNodeDiagonalMode.NoDiagonal: diagonal = DiagonalMode.NoDiagonal; break;
                    case PathNodeDiagonalMode.DiagonalNoCutting: diagonal = DiagonalMode.DiagonalNoCutting; break;
                }
            }

            // Check if diagonal movements can be used
            if (diagonal != DiagonalMode.NoDiagonal)
            {
                // Cache the adjacent nodes
                PathNode left = SafeGetNode(node.X - 1, node.Y);
                PathNode right = SafeGetNode(node.X + 1, node.Y);
                PathNode top = SafeGetNode(node.X, node.Y + 1);
                PathNode bottom = SafeGetNode(node.X, node.Y - 1);

                bool canAdd = true;

                // Bottom left
                {
                    canAdd = true;

                    if (diagonal == DiagonalMode.DiagonalNoCutting)
                    {
                        // Left cutting
                        if (left != null && left.IsWalkable == false)
                            canAdd = false;

                        // Bottom cutting
                        if (bottom != null && bottom.IsWalkable == false)
                            canAdd = false;
                    }

                    // Make sure the diagonal movement is allowed
                    if (canAdd == true)
                        nodes[index++] = SafeGetNode(node.X - 1, node.Y - 1);
                } // End bottom left

                // Top right
                {
                    canAdd = true;

                    if (diagonal == DiagonalMode.DiagonalNoCutting)
                    {
                        // Right cutting
                        if (right != null && right.IsWalkable == false)
                            canAdd = false;

                        // Top cutting
                        if (top != null && top.IsWalkable == false)
                            canAdd = false;
                    }

                    // Make sure the diagonal movement is allowed
                    if (canAdd == true)
                        nodes[index++] = SafeGetNode(node.X + 1, node.Y + 1);
                } // End top right

                // Top Left
                {
                    canAdd = true;

                    if (diagonal == DiagonalMode.DiagonalNoCutting)
                    {
                        // Left cutting
                        if (left != null && left.IsWalkable == false)
                            canAdd = false;

                        // Top cutting
                        if (top != null && top.IsWalkable == false)
                            canAdd = false;
                    }

                    // Make sure the diagonal movement is allowed
                    if (canAdd == true)
                        nodes[index++] = SafeGetNode(node.X - 1, node.Y + 1);
                } // End top left

                // Bottom right
                {
                    canAdd = true;

                    if (diagonal == DiagonalMode.DiagonalNoCutting)
                    {
                        // Right cutting
                        if (right != null && right.IsWalkable == false)
                            canAdd = false;

                        // Bottom cutting
                        if (bottom != null && bottom.IsWalkable == false)
                            canAdd = false;
                    }

                    // Make sure the diagonal movement is allowed
                    if (canAdd == true)
                        nodes[index++] = SafeGetNode(node.X + 1, node.Y - 1);
                } // End bottom right
            }

            // Bottom
            nodes[index++] = SafeGetNode(node.X, node.Y - 1);

            // Left
            nodes[index++] = SafeGetNode(node.X - 1, node.Y);

            // Right
            nodes[index++] = SafeGetNode(node.X + 1, node.Y);

            // Top
            nodes[index++] = SafeGetNode(node.X, node.Y + 1);

            return index + 1;
        }
        private PathNode SafeGetNode(int x, int y)
        {
            // Validate index
            if (x >= 0 && x < width &&
                y >= 0 && y < height)
                return nodeGrid[x, y];

            return null;
        }
        #endregion
    }
}