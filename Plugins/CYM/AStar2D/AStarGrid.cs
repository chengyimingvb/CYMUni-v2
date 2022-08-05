using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.AStar2D
{
    public enum MapType
    { 
        XY,
        XZ,
    }
    //寻路请求
    public delegate void PathRequestDelegate(Path path, PathRequestStatus status);

    public enum PathRequestStatus
    {
        //无效的节点
        InvalidIndex = 0,
        //相同的起点和终点
        SameStartEnd,
        //路径没有找到
        PathNotFound,
        //搜索成功
        PathFound,
    }

    internal struct DynamicObstacleData
    {
        // Public
        public float lastUpdate;
        public Obstacle obstacle;
        public List<Index> obstructedIndexes;
    }

    [RequireComponent(typeof(PathView))]
    [RequireComponent(typeof(GridView))]
    [RequireComponent(typeof(ThreadManager))]
    [RequireComponent(typeof(WeightPainter))]
    [HideMonoScript]
    public class AStarGrid : MonoBehaviour
    {
        private static List<AStarGrid> activeGrids = new List<AStarGrid>();

        // Properties
        /// <summary>
        /// Attempts to access the default grid, typically the grid that is created first.
        /// </summary>
        public static AStarGrid DefaultGrid
        {
            get { return (activeGrids.Count > 0) ? activeGrids[0] : null; }
        }

        // Methods
        internal static void RegisterGrid(AStarGrid grid)
        {
            // Add the grid to the active grids
            activeGrids.Add(grid);
        }

        internal static void UnregisterGrid(AStarGrid grid)
        {
            // Remove the grid from the list
            activeGrids.Remove(grid);
        }
        public static AStarGrid Ins { get; private set; }

        #region Callback
        public event Callback<Node> Callback_OnNodeHover;
        #endregion

        #region private
        private IBaseNode[] connectionNodes = new IBaseNode[8]; // Allocate once and reuse (8 possible directions)
        private bool isDirty = false;
        private Node[,] Nodes;
        private WeightPainter painter = null;
        private SearchGrid searchGrid = null;
        private List<DynamicObstacleData> obstacles = new List<DynamicObstacleData>();
        private HashSet<Obstacle> hashObstacles = new HashSet<Obstacle>();
        private HashSet<Index> obstructedIndexes = new HashSet<Index>();
        #endregion

        #region Inspector
        [SerializeField]
        public MapType Type = MapType.XY;
        [SerializeField]
        public GameObject NodePrefab;
        [SerializeField]
        public bool showPreviewPath = false;
        [SerializeField]
        public bool allowThreading = true;
        [SerializeField]
        public DiagonalMode diagonalMovement = DiagonalMode.Diagonal;
        [SerializeField]
        public int nodeSpacing = 1;
        [SerializeField]
        [Tooltip("-1表示无限制")]
        public int maxPathLength = 999;
        [SerializeField]
        public float weightingInfluence = 1;
        [SerializeField]
        public int gridX = 16;
        [SerializeField]
        public int gridY = 16;
        #endregion

        #region public
        [HideInInspector]
        public int PosFactionX { get; private set; } = 0;
        [HideInInspector]
        public int PosFactionY { get; private set; } = 0;
        #endregion

        #region Properties
        public IBaseNode this[int x, int y] => searchGrid[x, y];
        public HeuristicProvider Provider { set { searchGrid.provider = value; } }
        public int Width => searchGrid == null ? 0 : searchGrid.Width;
        public int Height => searchGrid == null ? 0 : searchGrid.Height;
        #endregion

        #region life
        public virtual void Awake()
        {
            Ins = this;
            PosFactionX = (gridX / 2);
            PosFactionY = (gridY / 2);
            AStarGrid.RegisterGrid(this);
            Nodes = new Node[gridX, gridY];
            for (int i = 0; i < gridX; i++)
            {
                for (int j = 0; j < gridY; j++)
                {
                    GameObject obj = null;
                    if (NodePrefab != null)
                    {
                        obj = Instantiate(NodePrefab, Const.VEC_FarawayPos, Quaternion.identity);
                        obj.name = string.Format("Node:{0},{1}", i, j);
                    }
                    else
                    {
                        obj = new GameObject(string.Format("Node:{0},{1}", i, j));
                        var temp = obj.AddComponent<Node>();
                        temp.IsWalkable = true;
                    }
                    obj.transform.localPosition = Const.VEC_FarawayPos;
                    obj.transform.rotation = Quaternion.identity;
                    obj.transform.SetParent(transform);
                    if(Type == MapType.XY)
                        obj.transform.localPosition = new Vector3((i - PosFactionX) * nodeSpacing, (j - PosFactionY) * nodeSpacing);
                    else
                        obj.transform.localPosition = new Vector3((i - PosFactionX) * nodeSpacing, 0,(j - PosFactionY) * nodeSpacing);
                    Nodes[i, j] = obj.GetComponent<Node>();
                    Nodes[i, j].Index = new Index(i, j);
                    Nodes[i, j].OnNodeSelected += OnNodeSelected;
                    if (showPreviewPath == true)
                        Nodes[i, j].OnNodeHover += OnNodeHover;


                }
            }

            searchGrid = new SearchGrid(Nodes);
            searchGrid.IsIndexObstacle = IsObstacle;
        }
        public void Start()
        {
            painter = gameObject.GetComponent<WeightPainter>();// FindObjectOfType<WeightPainter2D>();
        }
        public virtual void Update()
        {
            if (isDirty)
            {
                isDirty = false;
                RebuildGraph();
            }
        }
        public virtual void OnValidate()
        {
            if (searchGrid != null)
            {
                searchGrid.WeightingInfluence = weightingInfluence;
            }
        }
        public virtual void OnDestroy()
        {
            AStarGrid.UnregisterGrid(this);
            connectionNodes = null;
            Nodes = null;
            searchGrid = null;
            obstacles.Clear();
            hashObstacles.Clear();
            obstructedIndexes.Clear();
        }
        #endregion

        #region create
        //注册障碍物
        public void RegisterObstacle(Obstacle obstacle)
        {
            if (hashObstacles.Contains(obstacle)) return;
            hashObstacles.Add(obstacle);
            obstacles.Add(new DynamicObstacleData
            {
                lastUpdate = 0,
                obstacle = obstacle,
                obstructedIndexes = new List<Index>(),
            });
        }
        //取消障碍物
        public void UnregisterObstacle(Obstacle obstacle)
        {
            if (!hashObstacles.Contains(obstacle)) return;
            hashObstacles.Remove(obstacle);
            for (int i = 0; i < obstacles.Count; i++)
            {
                if (obstacles[i].obstacle == obstacle)
                {
                    obstacles.RemoveAt(i);
                    return;
                }
            }
        }
        #endregion

        #region set
        //重建Graph
        public void RebuildGraph()
        {
            obstructedIndexes.Clear();
            for (int i = 0; i < obstacles.Count; i++)
            {
                DynamicObstacleData data = obstacles[i];
                bool isChanged = false;
                if (obstacles[i].obstacle.IsDirty == true)
                {
                    isChanged = true;
                    data.lastUpdate = Time.time;
                    data.obstructedIndexes.Clear();
                    if (data.obstacle.IsObstructing == true)
                    {
                        Bounds bounds = data.obstacle.GetObstacleBounds();
                        Index min = GetIndex(bounds.min);
                        Index max = GetIndex(bounds.max);
                        for (int x = min.X; x < max.X; x++)
                        {
                            for (int y = min.Y; y < max.Y; y++)
                            {
                                if (x >= searchGrid.Width || y >= searchGrid.Height) continue;
                                if (x < 0 || y < 0) continue;
                                var node = searchGrid[x, y];
                                if (node == null) continue;
                                Vector3 pos = node.Pos;
                                if (data.obstacle.IsOccupiedByObstacle(pos) == true)
                                {
                                    var newIndex = new Index(x, y);
                                    data.obstructedIndexes.Add(newIndex);
                                    obstructedIndexes.Add(newIndex);
                                }
                            }
                        }
                    }
                    data.obstacle.OnObstacleUpdated();
                }
                if (isChanged == true)
                    obstacles[i] = data;
            }
        }
        public void SetDirty() => isDirty = true;
        #endregion

        #region fina path
        public void FindPath(Index start, Index end, BaseTraversal2D traversal2D, PathRequestDelegate callback)
        {
            FindPath(start, end, diagonalMovement, traversal2D, callback);
        }
        public void FindPath(Index start, Index end, DiagonalMode diagonal, BaseTraversal2D traversal2D, PathRequestDelegate callback)
        {
            if (!IsValid(start) || !IsValid(end)) return;
            searchGrid.maxPathLength = maxPathLength;
            bool useThreading = allowThreading;
            if (useThreading == true)
            {
                AsyncPathRequest request = new AsyncPathRequest(searchGrid, start, end, diagonal, traversal2D, (Path path, PathRequestStatus status) =>
                {
                    PathView.setRenderPath(this, path);
                    callback(path, status);
                });
                ThreadManager.Active.asyncRequest(request);
            }
            else
            {
                PathRequestStatus status;
                Path result = FindPathImmediate(start, end, out status, diagonal);
                PathView.setRenderPath(this, result);
                callback(result, status);
            }

            Path FindPathImmediate(Index subStart, Index subEnd, out PathRequestStatus subStatus, DiagonalMode subDiagonal)
            {
                searchGrid.maxPathLength = maxPathLength;
                Path path = null;
                PathRequestStatus temp = PathRequestStatus.InvalidIndex;
                searchGrid.FindPath(subStart, subEnd, subDiagonal, traversal2D, (Path result, PathRequestStatus resultStatus) =>
                {
                    temp = resultStatus;
                    if (resultStatus == PathRequestStatus.PathFound)
                    {
                        path = result;
                        PathView.setRenderPath(this, path);
                    }
                });

                subStatus = temp;
                return path;
            }
        }
        #endregion

        #region is
        //是否为障碍物
        public bool IsObstacle(Index index) => obstructedIndexes.Contains(index);
        public bool IsValid(Index index)
        {
            if (index.X < 0 || index.Y < 0) return false;
            if (index.X >= Width || index.Y >= Height) return false;
            return true;
        }
        #endregion

        #region get
        public HashList<Node> GetConnection(Node center, DiagonalMode? diagonal = null)
        {
            searchGrid.ConstructAdjacentNodes(center, connectionNodes, diagonal == null ? diagonalMovement : diagonal.Value);
            HashList<Node> ret = new HashList<Node>();
            foreach (var item in connectionNodes)
            {
                if (item == null) continue;
                ret.Add(item.TileNode);
            }
            return ret;
        }
        public Node GetNode(Index index)
        {
            if (!IsValid(index))
            {
                //CLog.Error("无效节点:{0},{1}", index.X, index.Y);
                return null;
            }
            return Nodes[index.X, index.Y];
        }
        //获得节点之间的距离
        public int GetDistance(Index start, Index end, DiagonalMode diagonal)
        {
            int count = 1;
            int deltaX = Mathf.Abs(start.X - end.X);
            int deltaY = Mathf.Abs(start.Y - end.Y);
            if (diagonal == DiagonalMode.NoDiagonal)
            {
                count += (deltaX + deltaY);
            }
            else
            {
                int smallest = deltaX;
                if (deltaY < smallest)
                    smallest = deltaY;
                int diagonalSteps = Mathf.Abs(deltaX - deltaY);
                count += (diagonalSteps + smallest);
            }
            return count;
        }
        #endregion

        #region get Index
        //获得Index,可能为无效
        public Index GetIndex(Vector3 pos)
        {
            Vector2 newPos = pos;
            newPos = transform.InverseTransformPoint(pos);
            float xPos = newPos.x / nodeSpacing + PosFactionX;
            float yPos = newPos.y / nodeSpacing + PosFactionY;
            return new Index(Mathf.RoundToInt(xPos), Mathf.RoundToInt(yPos));
        }
        //获得安全的Index
        public Index GetSafeIndex(Vector3 pos)
        {
            var index = GetIndex(pos);
            if (IsValid(index))
                return index;
            return searchGrid.FindNearestIndex(pos);
        }
        #endregion

        #region Callback
        protected virtual void OnNodeSelected(Node BaseNode2D, int mouseButton)
        {
            // Check for a valid painter
            if (painter != null)
            {
                // Check if we are using the painter - if so, dont bother finding paths
                if (painter.IsPainting == true)
                    return;
            }

            if (mouseButton == 0)
            {

            }
            else if (mouseButton == 1)
            {

            }
        }

        private void OnNodeHover(Node BaseNode2D)
        {
            // Check for a valid painter
            if (painter != null)
            {
                // Check if we are using the painter - if so, dont bother finding paths
                if (painter.IsPainting == true)
                    return;
            }
            Callback_OnNodeHover?.Invoke(BaseNode2D);
        }
        #endregion
    }
}
