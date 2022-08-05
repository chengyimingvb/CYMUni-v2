using UnityEngine;

namespace CYM.AStar2D
{
    public sealed class PathSession
    {
        // Private
        private IBaseNode currentNode = null;


        // Properties
        public Path Path { get; private set; } = null;
        public int CurrentIndex { get; private set; } = 0;
        public IBaseNode CurTileNode => currentNode.TileNode;
        public bool IsLastNode => currentNode == Path.LastNode;
        public int RemainingPathLength => PathLength - CurrentIndex;
        public IBaseNode NextTileNode
        {
            get
            {
                var nextNodeIndex = CurrentIndex + 1;
                if (nextNodeIndex < Path.NodeCount)
                {
                    return Path.GetNode(nextNodeIndex).TileNode;
                }
                return null;
            }
        }

        public int PathLength
        {
            get
            {
                if (Path == null) return 0;
                return Path.NodeCount;
            }
        }
        public Vector3 Pos
        {
            get
            {
                if (currentNode == null)
                    return Vector3.zero;
                return currentNode.Pos;
            }
        }


        // Constructor
        public PathSession(Path path) => UsePath(path);
        // Methods
        public void UsePath(Path path)
        {
            this.Path = path;
            this.CurrentIndex = 0;
            this.currentNode = path.StartNode;
        }
        public bool AdvanceNode()
        {
            if (CurrentIndex < Path.NodeCount - 1)
            {
                CurrentIndex++;
                currentNode = Path.GetNode(CurrentIndex);
                return currentNode != null;
            }
            return false;
        }
        public bool HasReachedCurrentNode(Transform transform)
        {
            return Mathf.Approximately(transform.position.x, Pos.x) &&
                Mathf.Approximately(transform.position.y, Pos.y) &&
                Mathf.Approximately(transform.position.z, Pos.z);
        }
        public bool Contain(IBaseNode node)
        {
            if (Path == null) return false;
            return Path.Nodes.Contains(node);
        }
    }
}
