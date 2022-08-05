using UnityEngine;

namespace CYM.AStar2D
{
    /// <summary>
    /// The diagonal mode to use for pathfinding.
    /// This value is specified on a per node basis.
    /// </summary>
    public enum PathNodeDiagonalMode
    {
        /// <summary>
        /// Diagonal movement should not be allowed.
        /// </summary>
        NoDiagonal,
        /// <summary>
        /// Diagonal movement is allowed.
        /// </summary>
        Diagonal,
        /// <summary>
        /// Diagonal movement is allowed but corner cutting of non-walkable nodes is not allowed.
        /// </summary>
        DiagonalNoCutting,
        /// <summary>
        /// Use the global diagonal mode specified in the <see cref="AStarGrid"/>. 
        /// </summary>
        UseGlobal,
    }

    public interface IBaseNode
    {
        // Properties
        bool IsWalkable { get; }
        float Weighting { get; }
        Vector3 Pos { get; }
        PathNodeDiagonalMode DiagonalMode { get; }
        Index Index { get; }
        IBaseNode TileNode { get; }
        int TileNodeIndex { get; set; }

        bool IsShowInDebugView();
    }
}
