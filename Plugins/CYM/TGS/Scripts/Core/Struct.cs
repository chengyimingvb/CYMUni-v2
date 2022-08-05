using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM.TGS
{
    public enum FADER_STYLE
    {
        FadeOut = 0,
        Blink = 1,
        Flash = 2,
        ColorTemp = 3
    }
    public enum CELL_SIDE
    {
        TopLeft = 0,
        Top = 1,
        TopRight = 2,
        BottomRight = 3,
        Bottom = 4,
        BottomLeft = 5,
        Left = 6,
        Right = 7
    }
    public enum CELL_DIRECTION
    {
        Exiting = 0,
        Entering = 1,
        Both = 2
    }
    public enum HIGHLIGHT_MODE
    {
        None = 0,
        Territories = 1,
        Cells = 2
    }
    public enum OVERLAY_MODE
    {
        Overlay = 0,
        Ground = 1
    }
    public enum GRID_TOPOLOGY
    {
        Box = 1,
        Hexagonal = 3
    }
    public enum HIGHLIGHT_EFFECT
    {
        Default = 0,
        DualColors = 6,
        TextureAdditive = 1,
        TextureMultiply = 2,
        TextureColor = 3,
        TextureScale = 4,
        None = 5
    }
    enum RedrawType
    {
        None = 0,
        Full = 1,
        IncrementalTerritories = 2
    }
    public interface IAdmin
    {
        string name { get; set; }
        bool visible { get; set; }
        /// <summary>
        /// Used for incremental updates
        /// </summary>
        bool isDirty { get; set; }
        bool borderVisible { get; set; }
    }
    public interface ITerrainWrapper
    {
        GameObject gameObject { get; }
        T GetComponent<T>();
        Transform transform { get; }
        void Dispose();
        bool supportsMultipleObjects { get; }
        bool supportsCustomHeightmap { get; }
        bool supportsPivot { get; }
        bool enabled { get; set; }
        Bounds bounds { get; }
        TerrainData terrainData { get; }
        void Refresh();
        void SetupTriggers(TGS tgs);
        int heightmapMaximumLOD { get; set; }
        int heightmapWidth { get; }
        int heightmapHeight { get; }
        Vector3 localCenter { get; }
        Vector3 size { get; }
        float[,] GetHeights(int xBase, int yBase, int width, int height);
        void SetHeights(int xBase, int yBase, float[,] heights);
        float SampleHeight(Vector3 worldPosition);
        Vector3 GetInterpolatedNormal(float x, float y);
        bool Contains(GameObject gameObject);
        Vector3 GetLocalPoint(GameObject gameObject, Vector3 wsPosition);
    }
    public class AdminEntity : IAdmin
	{
		/// <summary>
		/// Optional entity name.
		/// </summary>
		public string name { get; set; }
		/// <summary>
		/// Unscaled center. Ranges from -0.5, -0.5 to 0.5, 0.5.
		/// </summary>
		public Vector2 center;
		/// <summary>
		/// Original entity center with applied offset & scale
		/// </summary>
		public Vector2 scaledCenter;
		public bool borderVisible { get; set; }
		/// <summary>
		/// Used internally to control incremental updates
		/// </summary>
		public bool isDirty { get; set; }
		public virtual bool visible { get; set; }
	}
	public partial class Cell : AdminEntity
	{
		/// <summary>
		/// The index of the cell in the cells array
		/// </summary>
		public int index;
		/// <summary>
		/// Physical surface-related data
		/// </summary>
		public Region region { get; set; }
		/// <summary>
		/// Cells adjacent to this cell
		/// </summary>
		public List<Cell> neighbours = new List<Cell>();
		/// <summary>
		/// The territory to which this cell belongs to. You can change it using CellSetTerritory method.
		/// WARNING: do not change this value directly, use CellSetTerritory instead.
		/// </summary>
		public int territoryIndex = -1;
		public override bool visible { get { return visibleSelf && visibleByRules; } set { visibleSelf = value; } }
		public bool visibleSelf { get; private set; }
		public bool visibleByRules = true;
		/// <summary>
		/// Optional value that can be set with CellSetTag. You can later get the cell quickly using CellGetWithTag method.
		/// </summary>
		public int tag;
		public int row, column;
		/// <summary>
		/// If this cell blocks path finding.
		/// </summary>
		public bool canCross = true;
		float[] _crossCost;
		/// <summary>
		/// Used by pathfinding in Cell mode. Cost for crossing a cell for each side. Defaults to 1.
		/// </summary>
		/// <value>The cross cost.</value>
		public float[] crossCost
		{
			get { return _crossCost; }
			set { _crossCost = value; }
		}
		bool[] _blocksLOS;
		/// <summary>
		/// Used by specify if LOS is blocked across cell sides.
		/// </summary>
		/// <value>The cross cost.</value>
		public bool[] blocksLOS
		{
			get { return _blocksLOS; }
			set { _blocksLOS = value; }
		}
		/// <summary>
		/// Group for this cell. A different group can be assigned to use along with FindPath cellGroupMask argument.
		/// </summary>
		public int group = 1;
		/// <summary>
		/// Used internally to optimize certain algorithms
		/// </summary>
		[NonSerialized]
		public int iteration;
		public Cell(string name, Vector2 center)
		{
			this.name = name;
			this.center = center;
			visible = true;
			borderVisible = true;
		}
		public Cell() : this("", Vector2.zero)
		{
		}
		public Cell(string name) : this(name, Vector2.zero)
		{
		}
		public Cell(Vector2 center) : this("", center)
		{
		}

		/// <summary>
		/// Gets the side cross cost.
		/// </summary>
		/// <returns>The side cross cost.</returns>
		/// <param name="side">Side.</param>
		public float GetSideCrossCost(CELL_SIDE side)
		{
			if (_crossCost == null) return 0;
			return _crossCost[(int)side];
		}
		/// <summary>
		/// Assigns a crossing cost for a given hexagonal side
		/// </summary>
		/// <param name="side">Side.</param>
		/// <param name="cost">Cost.</param>
		public void SetSideCrossCost(CELL_SIDE side, float cost)
		{
			if (_crossCost == null) _crossCost = new float[8];
			_crossCost[(int)side] = cost;
		}
		/// <summary>
		/// Sets the same crossing cost for all sides of the hexagon.
		/// </summary>
		public void SetAllSidesCost(float cost)
		{
			if (_crossCost == null) _crossCost = new float[8];
			for (int k = 0; k < _crossCost.Length; k++) { _crossCost[k] = cost; }
		}
		/// <summary>
		/// Returns true if side is blocking LOS
		/// </summary>
		public bool GetSideBlocksLOS(CELL_SIDE side)
		{
			if (_blocksLOS == null) return false;
			return _blocksLOS[(int)side];
		}
		/// <summary>
		/// Assigns a crossing cost for a given hexagonal side
		/// </summary>
		/// <param name="side">Side.</param>
		public void SetSideBlocksLOS(CELL_SIDE side, bool blocks)
		{
			if (_blocksLOS == null) _blocksLOS = new bool[8];
			_blocksLOS[(int)side] = blocks;
		}
	}
	public class Frontier
	{
		public Region region1;
		public Region region2;
	}
    public class Region
    {
        public Polygon polygon;
        /// <summary>
        /// Points coordinates with applied grid offset and scale
        /// </summary>
        public List<Vector2> points;
        /// <summary>
        /// Scaled rect (rect with grid offset and scale applied)
        /// </summary>
        public Rect rect2D;
        public float rect2DArea;
        /// <summary>
        /// Cells in this region.
        /// </summary>
        public List<Cell> cells;
        /// <summary>
        /// Original grid segments. Segments coordinates are not scaled.
        /// </summary>
        public List<Segment> segments;
        public IAdmin entity;
        public Renderer renderer;
        public GameObject surfaceGameObject { get { return renderer != null ? renderer.gameObject : null; } }
        public Material cachedMat;
        /// <summary>
        /// Used internally to ensure smaller territory surfaces are rendered before others
        /// </summary>
        public int sortIndex;
        public Material customMaterial { get; set; }
        public Vector2 customTextureScale, customTextureOffset;
        public float customTextureRotation;
        public bool customRotateInLocalSpace;
        public delegate bool ContainsFunction(float x, float y);
        public ContainsFunction Contains;
        public bool isBox;
        /// <summary>
        /// If the gameobject contains one or more children surfaces with name splitSurface due to having +65000 vertices
        /// </summary>
		public List<Renderer> childrenSurfaces;
        public Region(IAdmin entity, bool isBox)
        {
            this.entity = entity;
            this.isBox = isBox;
            if (isBox)
            {
                segments = new List<Segment>(4);
                Contains = PointInBox;
            }
            else
            {
                segments = new List<Segment>(6);
                Contains = PointInPolygon;
            }
        }
        public void Clear()
        {
            polygon = null;
            if (points != null)
            {
                points.Clear();
            }
            segments.Clear();
            rect2D.width = rect2D.height = 0;
            rect2DArea = 0;
            if (surfaceGameObject != null)
            {
                UnityEngine.Object.DestroyImmediate(surfaceGameObject);
            }
            customMaterial = null;
            childrenSurfaces = null;
            cells.Clear();
        }
        public void DestroySurface()
        {
            if (renderer != null)
            {
                UnityEngine.Object.DestroyImmediate(renderer.gameObject);
                renderer = null;
            }
        }
        public Region Clone()
        {
            Region c = new Region(entity, isBox);
            c.customMaterial = this.customMaterial;
            c.customTextureScale = this.customTextureScale;
            c.customTextureOffset = this.customTextureOffset;
            c.customTextureRotation = this.customTextureRotation;
            c.points = new List<Vector2>(points);
            c.polygon = polygon.Clone();
            c.segments = new List<Segment>(segments);
            c.rect2D = rect2D;
            c.rect2DArea = rect2DArea;
            return c;
        }
        public void SetPoints(List<Vector2> points)
        {
            this.points = points;
            UpdateBounds();
        }
        public void Enlarge(float amount)
        {
            Vector2 center = rect2D.center;
            int pointCount = points.Count;
            for (int k = 0; k < pointCount; k++)
            {
                Vector2 p = points[k];
                float DX = center.x - p.x;
                float DY = center.y - p.y;
                p.x -= DX * amount;
                p.y -= DY * amount;
                points[k] = p;
            }
        }
        public bool Intersects(Region other)
        {

            if (points == null || other == null || other.points == null)
                return false;

            Rect otherRect = other.rect2D;

            if (otherRect.xMin > rect2D.xMax)
                return false;
            if (otherRect.xMax < rect2D.xMin)
                return false;
            if (otherRect.yMin > rect2D.yMax)
                return false;
            if (otherRect.yMax < rect2D.yMin)
                return false;

            int pointCount = points.Count;
            int otherPointCount = other.points.Count;

            for (int k = 0; k < otherPointCount; k++)
            {
                int j = pointCount - 1;
                bool inside = false;
                Vector2 p = other.points[k];
                for (int i = 0; i < pointCount; j = i++)
                {
                    if (((points[i].y <= p.y && p.y < points[j].y) || (points[j].y <= p.y && p.y < points[i].y)) &&
                        (p.x < (points[j].x - points[i].x) * (p.y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
                        inside = !inside;
                }
                if (inside)
                    return true;
            }

            for (int k = 0; k < pointCount; k++)
            {
                int j = otherPointCount - 1;
                bool inside = false;
                Vector2 p = points[k];
                for (int i = 0; i < otherPointCount; j = i++)
                {
                    if (((other.points[i].y <= p.y && p.y < other.points[j].y) || (other.points[j].y <= p.y && p.y < other.points[i].y)) &&
                        (p.x < (other.points[j].x - other.points[i].x) * (p.y - other.points[i].y) / (other.points[j].y - other.points[i].y) + other.points[i].x))
                        inside = !inside;
                }
                if (inside)
                    return true;
            }

            return false;
        }
        bool PointInBox(float x, float y)
        {
            return x >= rect2D.xMin && x <= rect2D.xMax && y >= rect2D.yMin && y <= rect2D.yMax;
        }
        bool PointInPolygon(float x, float y)
        {
            if (points == null)
                return false;

            if (x > rect2D.xMax || x < rect2D.xMin || y > rect2D.yMax || y < rect2D.yMin)
                return false;

            int numPoints = points.Count;
            int j = numPoints - 1;
            bool inside = false;
            for (int i = 0; i < numPoints; j = i++)
            {
                if (((points[i].y <= y && y < points[j].y) || (points[j].y <= y && y < points[i].y)) &&
                    (x < (points[j].x - points[i].x) * (y - points[i].y) / (points[j].y - points[i].y) + points[i].x))
                    inside = !inside;
            }
            return inside;
        }
        public bool ContainsPoint(Vector2 point)
        {
            return PointInPolygon(point.x, point.y);
        }
        public bool ContainsRegion(Region otherRegion)
        {
            if (!rect2D.Overlaps(otherRegion.rect2D))
                return false;

            if (!Contains(otherRegion.rect2D.xMin, otherRegion.rect2D.yMin))
                return false;
            if (!Contains(otherRegion.rect2D.xMin, otherRegion.rect2D.yMax))
                return false;
            if (!Contains(otherRegion.rect2D.xMax, otherRegion.rect2D.yMin))
                return false;
            if (!Contains(otherRegion.rect2D.xMax, otherRegion.rect2D.yMax))
                return false;

            int opc = otherRegion.points.Count;
            for (int k = 0; k < opc; k++)
            {
                if (!Contains(otherRegion.points[k].x, otherRegion.points[k].y))
                    return false;
            }
            return true;
        }
        public void UpdateBounds()
        {
            float minx, miny, maxx, maxy;
            minx = miny = float.MaxValue;
            maxx = maxy = float.MinValue;
            int pointsCount = points.Count;
            for (int p = 0; p < pointsCount; p++)
            {
                Vector2 point = points[p];
                if (point.x < minx)
                    minx = point.x;
                if (point.x > maxx)
                    maxx = point.x;
                if (point.y < miny)
                    miny = point.y;
                if (point.y > maxy)
                    maxy = point.y;
            }
            float rectWidth = maxx - minx;
            float rectHeight = maxy - miny;
            rect2D = new Rect(minx, miny, rectWidth, rectHeight);
            rect2DArea = rectWidth * rectHeight;
        }
    }
    public partial class Territory : AdminEntity
    {
        /// <summary>
        /// List of other territories sharing some border with this territory
        /// </summary>
        public List<Territory> neighbours = new List<Territory>();
        /// <summary>
        /// List of physical regions. Usually territories have only one region, but they can be split by assigning cells to other territories
        /// </summary>
        public List<Region> regions;
        public List<Cell> cells;
        public Color fillColor = Color.gray;
        public Color frontierColor = new Color(0, 0, 0, 0);
        public bool neutral { get; set; }
        public Territory() : this("")
        {
        }
        public Territory(string name)
        {
            this.name = name;
            visible = true;
            borderVisible = true;
            cells = new List<Cell>();
        }
    }
    public class TerritoryMesh
    {
        public int territoryIndex;
        public Vector3[][] territoryMeshBorders;
        public int[][] territoryMeshIndices;
        public Color[][] territoryMeshColors;
    }
    public class DisposalManager
    {
        List<UnityEngine.Object> disposeObjects;
        public DisposalManager()
        {
            disposeObjects = new List<UnityEngine.Object>();
        }
        public void DisposeAll()
        {
            if (disposeObjects == null) return;
            int c = disposeObjects.Count;
            for (int k = 0; k < c; k++)
            {
                UnityEngine.Object o = disposeObjects[k];
                if (o != null)
                {
                    UnityEngine.Object.DestroyImmediate(o);
                }
            }
            disposeObjects.Clear();
        }
        public void MarkForDisposal(UnityEngine.Object o)
        {
            if (o != null)
            {
                o.hideFlags = HideFlags.DontSave;
                disposeObjects.Add(o);
            }
        }
    }
    public static class Drawing
    {
        static Rect dummyRect = new Rect();
        public static Renderer CreateSurface(string name, List<Vector3> surfPoints, int[] indices, Material material, int sortingOrder, DisposalManager disposalManager)
        {
            return CreateSurface(name, surfPoints, indices, material, dummyRect, TGSConst.Vector2one, TGSConst.Vector2zero, 0, false, sortingOrder, disposalManager);
        }
        /// <summary>
        /// Rotates one point around another
        /// </summary>
        /// <param name="pointToRotate">The point to rotate.</param>
        /// <param name="centerPoint">The centre point of rotation.</param>
        /// <param name="angleInDegrees">The rotation angle in degrees.</param>
        /// <returns>Rotated point</returns>
        static Vector2 RotatePoint(Vector2 pointToRotate, Vector2 centerPoint, float angleInDegrees)
        {
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;
            float cosTheta = Mathf.Cos(angleInRadians);
            float sinTheta = Mathf.Sin(angleInRadians);
            return new Vector2(cosTheta * (pointToRotate.x - centerPoint.x) - sinTheta * (pointToRotate.y - centerPoint.y) + centerPoint.x,
                sinTheta * (pointToRotate.x - centerPoint.x) + cosTheta * (pointToRotate.y - centerPoint.y) + centerPoint.y);
        }
        public static Renderer CreateSurface(string name, List<Vector3> surfPoints, int[] indices, Material material, Rect rect, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace, int sortingOrder, DisposalManager disposalManager)
        {
            GameObject hexa = new GameObject(name, typeof(MeshRenderer), typeof(MeshFilter));
            disposalManager.MarkForDisposal(hexa);
            Mesh mesh = new Mesh();
            disposalManager.MarkForDisposal(mesh);
            mesh.SetVertices(surfPoints);
            mesh.SetTriangles(indices, 0, true);
            // uv mapping
            if (material != null && material.HasProperty(TGSConst.MainTex) && material.mainTexture != null)
            {
                int len = surfPoints.Count;
                Vector2[] uv = new Vector2[len];
                for (int k = 0; k < len; k++)
                {
                    Vector2 coor = surfPoints[k];
                    Vector2 normCoor;
                    if (rotateInLocalSpace)
                    {
                        normCoor = new Vector2((coor.x - rect.xMin) / rect.width, (coor.y - rect.yMin) / rect.height);
                        if (textureRotation != 0)
                        {
                            normCoor = RotatePoint(normCoor, TGSConst.Vector2half, textureRotation);
                        }
                        normCoor.x = 0.5f + (normCoor.x - 0.5f) / textureScale.x;
                        normCoor.y = 0.5f + (normCoor.y - 0.5f) / textureScale.y;
                        normCoor -= textureOffset;
                    }
                    else
                    {
                        coor.x /= textureScale.x;
                        coor.y /= textureScale.y;
                        if (textureRotation != 0)
                        {
                            coor = RotatePoint(coor, Vector2.zero, textureRotation);
                        }
                        coor -= textureOffset;
                        normCoor = new Vector2((coor.x - rect.xMin) / rect.width, (coor.y - rect.yMin) / rect.height);
                    }
                    uv[k] = normCoor;
                }
                mesh.uv = uv;
            }
            mesh.RecalculateNormals();
            MeshFilter meshFilter = hexa.GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;
            Renderer rr = hexa.GetComponent<Renderer>();
            rr.sortingOrder = sortingOrder;
            rr.sharedMaterial = material;
            rr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rr.receiveShadows = false;
            return rr;
        }
    }
}