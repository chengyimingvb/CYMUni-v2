using Sirenix.OdinInspector;
using UnityEngine;

namespace CYM.AStar2D
{
    [HideMonoScript]
    internal sealed class PathView : MonoBehaviour
    {
        private LineRenderer line = null;
        [HideInInspector]
        AStarGrid visualizeGrid;
        public Material material;
        public float width = 0.2f;

        // Propeties
        public static Material DefaultMaterial
        {
            get
            {
                Material mat = new Material(Shader.Find("Sprites/Default"));

                mat.color = Color.magenta;

                return mat;
            }
        }
        private void Awake()
        {
            visualizeGrid = GetComponent<AStarGrid>();
        }
        // Methods
        public void Start()
        {
             
            // Get the renderer
            line = GetComponent<LineRenderer>();

            // Attatch at runtime
            if (line == null)
                line = gameObject.AddComponent<LineRenderer>();

            // Check for material
            if (material == null)
                material = DefaultMaterial;

            // Setup
            line.material = material;
            line.sortingOrder = -1;

#if UNITY_5_5_OR_NEWER
            line.startWidth = width;
            line.endWidth = width;
#else
            line.SetWidth(width, width);
#endif
        }

        public void setRenderPath(Path path)
        {
            // Set the number of vertices required
#if UNITY_5_5_OR_NEWER
            line.positionCount = path.NodeCount;
#else
            line.SetVertexCount(path.NodeCount);
#endif

            int index = 0;

            // Set each vertex
            foreach (IBaseNode node in path)
            {
                // Set the vertex
                line.SetPosition(index, node.Pos);

                // Increment the index
                index++;
            }
        }

        public static void setRenderPath(AStarGrid grid, Path path)
        {
            if (!Application.isEditor)
                return;
            // Make sure the path is valid
            if (path == null)
                return;

            // Find the component 
            PathView view = findViewForGrid(grid);

            // Check for error
            if (view == null)
                return;

            // Udpate the render path
            view.setRenderPath(path);
        }

        public static PathView findViewForGrid(AStarGrid grid)
        {
            // Find all components
            foreach (PathView view in Component.FindObjectsOfType<PathView>())
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
