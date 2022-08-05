using Sirenix.OdinInspector;
using UnityEngine;

namespace CYM.AStar2D
{
    [HideMonoScript]
    internal sealed class GridView : MonoBehaviour
    {
        AStarGrid visualizeGrid;
        public Color colour = Color.green;

        private void Awake()
        {
            visualizeGrid = GetComponent<AStarGrid>();
        }
        // Methods
        public void Update()
        {
            for (int x = 0; x < visualizeGrid.Width; x++)
            {
                for (int y = 0; y < visualizeGrid.Height; y++)
                {
                    // Get the current node
                    IBaseNode current = visualizeGrid[x, y];

                    // Make sure the node is walkable
                    if (current.IsWalkable == false)
                        continue;

                    // Check for occupied 
                    if (visualizeGrid.IsObstacle(new Index(x, y)) == true)
                        continue;

                    if (!current.IsShowInDebugView())
                        continue;

                    // Process surrounding tiles
                    IBaseNode[] nodes = getSurroundingNodes(x, y);

                    // Process all surrounding tiles
                    foreach (IBaseNode node in nodes)
                    {
                        // Check for null
                        if (node == null)
                            continue;

                        // Check for walkable
                        if (node.IsWalkable == false)
                            continue;

                        // Add line
                        Debug.DrawLine(node.Pos, current.Pos, colour);
                    }
                }
            }
        }

        private IBaseNode[] getSurroundingNodes(int x, int y)
        {
            // Create the array
            IBaseNode[] nodes = new IBaseNode[(visualizeGrid.diagonalMovement == DiagonalMode.NoDiagonal) ? 4 : 8];

            // Center node
            IBaseNode center = safeGetNode(x, y);

            // Left node
            nodes[0] = safeGetNode(x - 1, y);

            // Right node
            nodes[1] = safeGetNode(x + 1, y);

            // Up node
            nodes[2] = safeGetNode(x, y + 1);

            // Down node
            nodes[3] = safeGetNode(x, y - 1);


            DiagonalMode diagonalMode = visualizeGrid.diagonalMovement;

            if (center.DiagonalMode != PathNodeDiagonalMode.UseGlobal)
            {
                switch (center.DiagonalMode)
                {
                    case PathNodeDiagonalMode.Diagonal: diagonalMode = DiagonalMode.Diagonal; break;
                    case PathNodeDiagonalMode.NoDiagonal: diagonalMode = DiagonalMode.NoDiagonal; break;
                    case PathNodeDiagonalMode.DiagonalNoCutting: diagonalMode = DiagonalMode.DiagonalNoCutting; break;
                }
            }


            // Diagonal neighbors
            if (diagonalMode != DiagonalMode.NoDiagonal)
            {
                IBaseNode left = safeGetNode(x - 1, y);
                IBaseNode right = safeGetNode(x + 1, y);
                IBaseNode top = safeGetNode(x, y + 1);
                IBaseNode bottom = safeGetNode(x, y - 1);

                bool canAdd = true;

                // Top left
                if (diagonalMode == DiagonalMode.DiagonalNoCutting)
                {
                    if (left != null && left.IsWalkable == false)
                        canAdd = false;

                    if (top != null && top.IsWalkable == false)
                        canAdd = false;
                }
                if (canAdd == true)
                    nodes[4] = safeGetNode(x - 1, y + 1);

                // Top right
                canAdd = true;
                if (diagonalMode == DiagonalMode.DiagonalNoCutting)
                {
                    if (right != null && right.IsWalkable == false)
                        canAdd = false;

                    if (top != null && top.IsWalkable == false)
                        canAdd = false;
                }
                if (canAdd == true)
                    nodes[5] = safeGetNode(x + 1, y + 1);

                // Bottom left
                canAdd = true;
                if (diagonalMode == DiagonalMode.DiagonalNoCutting)
                {
                    if (left != null && left.IsWalkable == false)
                        canAdd = false;

                    if (bottom != null && bottom.IsWalkable == false)
                        canAdd = false;
                }
                if (canAdd == true)
                    nodes[6] = safeGetNode(x - 1, y - 1);

                // Bottom right
                canAdd = true;
                if (diagonalMode == DiagonalMode.DiagonalNoCutting)
                {
                    if (right != null && right.IsWalkable == false)
                        canAdd = false;

                    if (bottom != null && bottom.IsWalkable == false)
                        canAdd = false;
                }
                if (canAdd == true)
                    nodes[7] = safeGetNode(x + 1, y - 1);
            }
            return nodes;
        }

        private IBaseNode safeGetNode(int x, int y)
        {
            // Check for occupied 
            if (visualizeGrid.IsObstacle(new Index(x, y)) == true)
                return null;

            if (x >= 0 && x < visualizeGrid.Width &&
                y >= 0 && y < visualizeGrid.Height)
                return visualizeGrid[x, y];

            return null;
        }

        public static GridView findViewForGrid(AStarGrid grid)
        {
            // Find all components
            foreach (GridView view in Component.FindObjectsOfType<GridView>())
            {
                // Check for observed grid
                if (view.visualizeGrid == grid)
                {
                    // Get the component
                    return view;
                }
            }

            // Not found
            return null;
        }
    }
}
