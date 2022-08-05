//#define SHOW_DEBUG_GIZMOS
using UnityEngine;
using System;
using System.Collections.Generic;
using Poly2Tri;

namespace CYM.TGS
{
    [ExecuteInEditMode]
    [Serializable]
    public partial class TGS : MonoBehaviour {

        #region Const
        // internal fields
        const double MIN_VERTEX_DISTANCE = 0.002;
        const double SQR_MIN_VERTEX_DIST = 0.0002 * 0.0002;
        const string SKW_NEAR_CLIP_FADE = "NEAR_CLIP_FADE";
        const string SKW_FAR_FADE = "FAR_FADE";
        const string SKW_CIRCULAR_FADE = "CIRCULAR_FADE";
        const string SKW_TEX_HIGHLIGHT_ADDITIVE = "TGS_TEX_HIGHLIGHT_ADDITIVE";
        const string SKW_TEX_HIGHLIGHT_MULTIPLY = "TGS_TEX_HIGHLIGHT_MULTIPLY";
        const string SKW_TEX_HIGHLIGHT_COLOR = "TGS_TEX_HIGHLIGHT_COLOR";
        const string SKW_TEX_HIGHLIGHT_SCALE = "TGS_TEX_HIGHLIGHT_SCALE";
        const string SKW_TEX_DUAL_COLORS = "TGS_TEX_DUAL_COLORS";
        const string SKW_TRANSPARENT = "TGS_TRANSPARENT";
        public const int MAX_ROWS_OR_COLUMNS = 1000;
        const int STENCIL_MASK_HUD = 2;
        const int STENCIL_MASK_CELL = 4;
        const int STENCIL_MASK_TERRITORY = 8;
        Rect canvasRect = new Rect(-0.5f, -0.5f, 1, 1);
        readonly int[] hexIndices = new int[] { 0, 5, 1, 1, 5, 2, 5, 4, 2, 2, 4, 3 };
        readonly int[] quadIndices = new int[] { 0, 2, 1, 0, 3, 2 };
        // Custom inspector stuff
        public const int MAX_TERRITORIES = 512;
        public int maxCellsSqrt = 100;
        public const int MAX_CELLS_FOR_CURVATURE = 500;
        public const int MAX_CELLS_FOR_RELAXATION = 1000;
        // Cell mesh data
        const string CELLS_LAYER_NAME = "Cells";
        // Territory mesh data
        const string TERRITORIES_LAYER_NAME = "Territories";
        const string TERRITORIES_CUSTOM_FRONTIERS_ROOT = "Custom Territory Frontier";
        readonly Connector tempConnector = new Connector();
        #endregion

        #region Material
        // Materials and resources
        [SerializeField] Material territoriesThinMat, territoriesGeoMat;
        [SerializeField] Material cellsThinMat, cellsGeoMat;
        [SerializeField] Material hudMatTerritoryOverlay, hudMatTerritoryGround, hudMatCellOverlay, hudMatCellGround;
        [SerializeField] Material territoriesDisputedThinMat, territoriesDisputedGeoMat;
        [SerializeField] Material coloredMatGroundCell, coloredMatOverlayCell;
        [SerializeField] Material coloredMatGroundTerritory, coloredMatOverlayTerritory;
        [SerializeField] Material texturizedMatGroundCell, texturizedMatOverlayCell;
        [SerializeField] Material texturizedMatGroundTerritory, texturizedMatOverlayTerritory;
        [SerializeField] Material cellLineMat;
        #endregion

        #region prop
        Vector3[][] cellMeshBorders;
        int[][] cellMeshIndices;
        float meshStep;
        bool recreateCells, recreateTerritories;
        Dictionary<int, Cell> cellTagged;
        bool needUpdateTerritories;
        Dictionary<Segment, Frontier> territoryNeighbourHit;
        Frontier[] frontierPool;
        List<Segment> territoryFrontiers;
        List<Region> _sortedTerritoriesRegions;
        List<TerritoryMesh> territoryMeshes;
        Connector[] territoryConnectors;
        // Common territory & cell structures
        Vector3[] frontiersPoints;
        int frontiersPointsCount;
        Dictionary<Segment, bool> segmentHit;
        List<TriangulationPoint> steinerPoints;
        PolygonPoint[] tempPolyPoints;
        PolygonPoint[] tempPolyPoints2;
        Dictionary<TriangulationPoint, int> surfaceMeshHit;
        List<Vector3> meshPoints;
        int[] triNew;
        int triNewIndex;
        int newPointsCount;
        List<Vector3> tempPoints;
        List<Vector4> tempUVs;
        List<int> tempIndices;
        bool canUseGeometryShaders;
        // Terrain data
        float[,] terrainHeights;
        float[] terrainRoughnessMap;
        float[] tempTerrainRoughnessMap;
        int terrainRoughnessMapWidth, terrainRoughnessMapHeight;
        float[] acumY;
        int heightMapWidth, heightMapHeight;
        const int TERRAIN_CHUNK_SIZE = 8;
        float maxRoughnessWorldSpace;
        // Caches
        readonly Dictionary<int, GameObject> surfaces = new Dictionary<int, GameObject>();
        Dictionary<Territory, int> _territoryLookup;
        int lastTerritoryLookupCount = -1;
        Dictionary<int, Material> coloredMatCacheGroundCell;
        Dictionary<int, Material> coloredMatCacheOverlayCell;
        Dictionary<Color, Material> frontierColorCache;
        Color[] factoryColors;
        bool refreshCellMesh, refreshTerritoriesMesh;
        List<Cell> sortedCells;
        bool needResortCells;
        bool needGenerateMap;
        RedrawType issueRedraw;
        DisposalManager disposalManager;
        List<int> tempListCells;
        int cellIteration;
        bool gridNeedsUpdate;
        bool applyingChanges, redrawing;
        // Z-Figther & LOD
        Vector3 lastCamPos, lastPos, lastScale, lastParentScale;
        Quaternion lastRot;
        float lastGridElevation, lastGridCameraOffset, lastGridMinElevationMultiplier;
        float terrainWidth;
        float terrainHeight;
        float terrainDepth;
        int _territoryHighlightedIndex = -1;
        int _territoryHighlightedRegionIndex = -1;
        Cell _cellHighlighted;
        int _cellHighlightedIndex = -1;
        float highlightFadeStart;
        int _territoryLastClickedIndex = -1, _territoryRegionLastClickedIndex = -1;
        int _cellLastClickedIndex = -1;
        int _territoryLastOverIndex = -1, _territoryRegionLastOverIndex = -1;
        int _cellLastOverIndex = -1;
        Cell _cellLastOver;
        bool canInteract;
        List<string> highlightKeywords;
        RaycastHit[] hits;
        Vector3 localCursorClickedPos, localCursorPos;
        bool dragging;
        // Misc
        int _lastVertexCount = 0;
        bool useEditorRay;
        Ray editorRay;
        List<SurfaceFader> tempFaders;
        public bool mouseIsOver;
        Region _territoryRegionHighlighted;
        int territoriesRegionsCount;
        [SerializeField]
        bool _isDirty;
        // Placeholders and layers
        GameObject territoryLayer;
        GameObject _surfacesLayer;
        GameObject _highlightedObj;
        GameObject cellLayer;
        #endregion

        #region prop
        public bool isDirty {
            get { return _isDirty; }
            set {
                if (value) {
                    if (OnGridSettingsChanged != null) {
                        OnGridSettingsChanged(this);
                    }
                    _isDirty = value;
                }
            }
        }
        GameObject surfacesLayer {
            get {
                if (_surfacesLayer == null)
                    CreateSurfacesLayer();
                return _surfacesLayer;
            }
        }
        Territory _territoryHighlighted {
            get {
                if (_territoryRegionHighlighted != null) {
                    return (Territory)_territoryRegionHighlighted.entity;
                }
                return null;
            }
        }
        Region _territoryRegionLastOver {
            get {
                if (_territoryLastOverIndex >= 0) {
                    return territories[_territoryLastOverIndex].regions[_territoryRegionLastOverIndex];
                } else {
                    return null;
                }
            }
        }
        Territory _territoryLastOver {
            get {
                if (_territoryLastOverIndex >= 0) {
                    return territories[_territoryLastOverIndex];
                } else {
                    return null;
                }
            }
        }
        public int lastVertexCount { get { return _lastVertexCount; } }
        bool territoriesAreUsed {
            get { return (_showTerritories || _colorizeTerritories || _highlightMode == HIGHLIGHT_MODE.Territories); }
        }
        List<Region> sortedTerritoriesRegions {
            get {
                if (_sortedTerritoriesRegions.Count != territoriesRegionsCount) {
                    ComputeSortedTerritoriesRegions();
                }
                return _sortedTerritoriesRegions;
            }
            set {
                _sortedTerritoriesRegions = value;
            }
        }
        Dictionary<Territory, int> territoryLookup {
            get {
                if (_territoryLookup != null && territories.Count == lastTerritoryLookupCount)
                    return _territoryLookup;
                if (_territoryLookup == null) {
                    _territoryLookup = new Dictionary<Territory, int>();
                } else {
                    _territoryLookup.Clear();
                }
                int terrCount = territories.Count;
                for (int k = 0; k < terrCount; k++) {
                    _territoryLookup.Add(territories[k], k);
                }
                lastTerritoryLookupCount = territories.Count;
                return _territoryLookup;
            }
        }
        Material cellsMat {
            get {
                if (_cellBorderThickness > 1)
                    return cellsGeoMat;
                else
                    return cellsThinMat;
            }
        }
        Material territoriesMat {
            get {
                if (_territoryFrontiersThickness > 1)
                    return territoriesGeoMat;
                else
                    return territoriesThinMat;
            }
        }
        Material territoriesDisputedMat {
            get {
                if (_territoryFrontiersThickness > 1)
                    return territoriesDisputedGeoMat;
                else
                    return territoriesDisputedThinMat;
            }
        }
        #endregion

        #region Map generation
        void SetupBoxGrid() {

            int qx = _cellColumnCount;
            int qy = _cellRowCount;

            double stepX = 1.0 / qx;
            double stepY = 1.0 / qy;

            double halfStepX = stepX * 0.5;
            double halfStepY = stepY * 0.5;

            Segment[,,] sides = new Segment[qx, qy, 4]; // 0 = left, 1 = top, 2 = right, 3 = bottom
            int subdivisions = goodGridCurvature > 0 ? 3 : 1;
            int cellsCount = 0;
            Connector connector = new Connector();
            Point[] quadPoints = new Point[4];

            for (int j = 0; j < qy; j++) {
                for (int k = 0; k < qx; k++) {
                    Point center = new Point((double)k / qx - 0.5 + halfStepX, (double)j / qy - 0.5 + halfStepY);
                    Cell cell = new Cell(new Vector2((float)center.x, (float)center.y));
                    cell.column = k;
                    cell.row = j;

                    Point p1 = center.Offset(-halfStepX, -halfStepY);
                    Point p2 = center.Offset(-halfStepX, halfStepY);
                    Point p3 = center.Offset(halfStepX, halfStepY);
                    Point p4 = center.Offset(halfStepX, -halfStepY);

                    Segment left = k > 0 ? sides[k - 1, j, 2] : new Segment(p1, p2, true);
                    sides[k, j, 0] = left;

                    Segment top = new Segment(p2, p3, j == qy - 1);
                    sides[k, j, 1] = top;

                    Segment right = new Segment(p3, p4, k == qx - 1);
                    sides[k, j, 2] = right;

                    Segment bottom = j > 0 ? sides[k, j - 1, 1] : new Segment(p4, p1, true);
                    sides[k, j, 3] = bottom;

                    Region cr = new Region(cell, true);
                    if (subdivisions > 1) {
                        cr.segments.AddRange(top.Subdivide(subdivisions, _gridCurvature));
                        cr.segments.AddRange(right.Subdivide(subdivisions, _gridCurvature));
                        cr.segments.AddRange(bottom.Subdivide(subdivisions, _gridCurvature));
                        cr.segments.AddRange(left.Subdivide(subdivisions, _gridCurvature));
                        connector.Clear();
                        connector.AddRange(cr.segments);
                        cr.polygon = connector.ToPolygon(); // FromLargestLineStrip();
                    } else {
                        cr.segments.Add(top);
                        cr.segments.Add(right);
                        cr.segments.Add(bottom);
                        cr.segments.Add(left);
                        quadPoints[0] = p1;
                        quadPoints[1] = p4;
                        quadPoints[2] = p3;
                        quadPoints[3] = p2;
                        cr.polygon = new Polygon(quadPoints); // left.start, bottom.start, right.start, top.start);
                    }
                    if (cr.polygon != null) {
                        cell.region = cr;
                        cell.index = cellsCount++;
                        cells.Add(cell);
                    }
                }
            }
        }
        void SetupHexagonalGrid() {

            double qx = 1.0 + (_cellColumnCount - 1.0) * 3.0 / 4.0;
            double qy = _cellRowCount + 0.5;
            int qy2 = _cellRowCount;
            int qx2 = _cellColumnCount;

            double stepX = 1.0 / qx;
            double stepY = 1.0 / qy;

            double halfStepX = stepX * 0.5;
            double halfStepY = stepY * 0.5;
            int evenLayout = _evenLayout ? 1 : 0;

            Segment[,,] sides = new Segment[qx2, qy2, 6]; // 0 = left-up, 1 = top, 2 = right-up, 3 = right-down, 4 = down, 5 = left-down
            int subdivisions = goodGridCurvature > 0 ? 3 : 1;
            int cellsCount = 0;
            for (int j = 0; j < qy2; j++) {
                for (int k = 0; k < qx2; k++) {
                    Point center = new Point((double)k / qx - 0.5 + halfStepX, (double)j / qy - 0.5 + stepY);
                    center.x -= k * halfStepX / 2;
                    Cell cell = new Cell(new Vector2((float)center.x, (float)center.y));
                    cell.row = j;
                    cell.column = k;

                    double offsetY = (k % 2 == evenLayout) ? 0 : -halfStepY;

                    Point p1 = center.Offset(-halfStepX, offsetY);
                    Point p2 = center.Offset(-halfStepX / 2, halfStepY + offsetY);
                    Point p3 = center.Offset(halfStepX / 2, halfStepY + offsetY);
                    Point p4 = center.Offset(halfStepX, offsetY);
                    Point p5 = center.Offset(halfStepX / 2, -halfStepY + offsetY);
                    Point p6 = center.Offset(-halfStepX / 2, -halfStepY + offsetY);

                    Segment leftUp = (k > 0 && offsetY < 0) ? sides[k - 1, j, 3] : new Segment(p1, p2, k == 0 || (j == qy2 - 1 && offsetY == 0));
                    sides[k, j, 0] = leftUp;

                    Segment top = new Segment(p2, p3, j == qy2 - 1);
                    sides[k, j, 1] = top;

                    Segment rightUp = new Segment(p3, p4, k == qx2 - 1 || (j == qy2 - 1 && offsetY == 0));
                    sides[k, j, 2] = rightUp;

                    Segment rightDown = (j > 0 && k < qx2 - 1 && offsetY < 0) ? sides[k + 1, j - 1, 0] : new Segment(p4, p5, (j == 0 && offsetY < 0) || k == qx2 - 1);
                    sides[k, j, 3] = rightDown;

                    Segment bottom = j > 0 ? sides[k, j - 1, 1] : new Segment(p5, p6, true);
                    sides[k, j, 4] = bottom;

                    Segment leftDown;
                    if (offsetY < 0 && j > 0 && k > 0) {
                        leftDown = sides[k - 1, j - 1, 2];
                    } else if (offsetY == 0 && k > 0) {
                        leftDown = sides[k - 1, j, 2];
                    } else {
                        leftDown = new Segment(p6, p1, true);
                    }
                    sides[k, j, 5] = leftDown;

                    cell.center.y += (float)offsetY;

                    Region cr = new Region(cell, false);
                    if (subdivisions > 1) {
                        if (!top.deleted)
                            cr.segments.AddRange(top.Subdivide(subdivisions, _gridCurvature));
                        if (!rightUp.deleted)
                            cr.segments.AddRange(rightUp.Subdivide(subdivisions, _gridCurvature));
                        if (!rightDown.deleted)
                            cr.segments.AddRange(rightDown.Subdivide(subdivisions, _gridCurvature));
                        if (!bottom.deleted)
                            cr.segments.AddRange(bottom.Subdivide(subdivisions, _gridCurvature));
                        if (!leftDown.deleted)
                            cr.segments.AddRange(leftDown.Subdivide(subdivisions, _gridCurvature));
                        if (!leftUp.deleted)
                            cr.segments.AddRange(leftUp.Subdivide(subdivisions, _gridCurvature));
                    } else {
                        if (!top.deleted)
                            cr.segments.Add(top);
                        if (!rightUp.deleted)
                            cr.segments.Add(rightUp);
                        if (!rightDown.deleted)
                            cr.segments.Add(rightDown);
                        if (!bottom.deleted)
                            cr.segments.Add(bottom);
                        if (!leftDown.deleted)
                            cr.segments.Add(leftDown);
                        if (!leftUp.deleted)
                            cr.segments.Add(leftUp);
                    }
                    Connector connector = new Connector();
                    connector.AddRange(cr.segments);
                    cr.polygon = connector.ToPolygon();
                    if (cr.polygon != null) {
                        cell.region = cr;
                        cell.index = cellsCount++;
                        cells.Add(cell);
                    }
                }
            }
        }
        void CreateCells() {

            _numCells = Mathf.Max(_numTerritories, 2, cellCount);
            if (cells == null) {
                cells = new List<Cell>(_numCells);
            } else {
                cells.Clear();
            }
            if (cellTagged == null)
                cellTagged = new Dictionary<int, Cell>();
            else
                cellTagged.Clear();

            switch (_gridTopology) {
                case GRID_TOPOLOGY.Box:
                    SetupBoxGrid();
                    break;
                case GRID_TOPOLOGY.Hexagonal:
                    SetupHexagonalGrid();
                    break;
                default:
                    break;
            }

            CellsFindNeighbours();
            CellsUpdateBounds();

            // Update sorted cell list
            if (sortedCells == null) {
                sortedCells = new List<Cell>(cells);
            } else {
                sortedCells.Clear();
                sortedCells.AddRange(cells);
            }
            ResortCells();

            ClearLastOver();

            recreateCells = false;

        }
        void ResortCells() {
            needResortCells = false;
            sortedCells.Sort((cell1, cell2) => {
                float area1 = cell1.region.rect2DArea;
                float area2 = cell2.region.rect2DArea;
                if (area1 < area2) return -1;
                if (area1 > area2) return 1;
                return 0;
            });
        }
        void CellsUpdateBounds() {
            // Update cells polygon
            int count = cells.Count;
            for (int k = 0; k < count; k++) {
                CellUpdateBounds(cells[k]);
            }
        }
        void CellUpdateBounds(Cell cell) {
            if (cell == null) return;

            if (cell.region.polygon.contours.Count == 0)
                return;

            List<Vector2> points = cell.region.points;
            cell.region.polygon.contours[0].GetVector2Points(_gridCenter, _gridScale, ref points);
            cell.region.SetPoints(points);
            cell.scaledCenter = GetScaledVector(cell.center);
        }
        void CellsUpdateNeighbours() {
            int cellCount = cells.Count;
            for (int k = 0; k < cellCount; k++) {
                Cell cell = cells[k];
                if (cell == null) continue;
                Region region = cell.region;
                cell.neighbours.Clear();
                int segCount = region.segments.Count;
                for (int j = 0; j < segCount; j++) {
                    region.segments[j].cellIndex = k;
                }
            }
            CellsFindNeighbours();
        }
        void CellsFindNeighbours() {

            int cellCount = cells.Count;
            for (int k = 0; k < cellCount; k++) {
                Cell cell = cells[k];
                if (cell == null) continue;
                Region region = cell.region;
                int numSegments = region.segments.Count;
                for (int i = 0; i < numSegments; i++) {
                    Segment seg = region.segments[i];
                    if (seg.cellIndex < 0) {
                        seg.cellIndex = cell.index;
                    } else if (seg.cellIndex != cell.index) {
                        Cell neighbour = cells[seg.cellIndex];
                        cell.neighbours.Add(neighbour);
                        neighbour.neighbours.Add(cell);
                    }
                }
            }

        }
        void FindTerritoryFrontiers() {

            if (territories == null || territories.Count == 0)
                return;

            if (territoryFrontiers == null) {
                territoryFrontiers = new List<Segment>(50000);
            } else {
                territoryFrontiers.Clear();
            }
            if (territoryNeighbourHit == null) {
                territoryNeighbourHit = new Dictionary<Segment, Frontier>(50000);
            } else {
                territoryNeighbourHit.Clear();
            }
            int terrCount = territories.Count;
            if (territoryConnectors == null || territoryConnectors.Length != terrCount) {
                territoryConnectors = new Connector[terrCount];
            }
            int cellCount = cells.Count;
            if (frontierPool == null) {
                frontierPool = new Frontier[cellCount * 6];
                for (int f = 0; f < frontierPool.Length; f++) {
                    frontierPool[f] = new Frontier();
                }
            }
            int frontierPoolUsed = 0;
            for (int k = 0; k < terrCount; k++) {
                if (territoryConnectors[k] == null) {
                    territoryConnectors[k] = new Connector();
                } else {
                    territoryConnectors[k].Clear();
                }
                Territory territory = territories[k];
                territory.cells.Clear();
                territories[k].neighbours.Clear();
            }

            for (int k = 0; k < cellCount; k++) {
                Cell cell = cells[k];
                if (cell == null || cell.territoryIndex >= terrCount)
                    continue;
                bool validCell = cell.visible && cell.territoryIndex >= 0;
                if (validCell)
                    territories[cell.territoryIndex].cells.Add(cell);
                Region cellRegion = cell.region;
                int numSegments = cellRegion.segments.Count;
                for (int i = 0; i < numSegments; i++) {
                    Segment seg = cellRegion.segments[i];
                    if (seg.border) {
                        if (validCell) {
                            territoryFrontiers.Add(seg);
                            int territory1 = cell.territoryIndex;
                            territoryConnectors[territory1].Add(seg);
                            seg.disputingTerritory1Index = seg.disputingTerritory2Index = seg.territoryIndex = territory1;
                        }
                        continue;
                    }
                    Frontier frontier;
                    if (territoryNeighbourHit.TryGetValue(seg, out frontier)) {
                        Region neighbour = frontier.region1;
                        Cell neighbourCell = (Cell)neighbour.entity;
                        int territory1Index = cell.territoryIndex;
                        int territory2Index = neighbourCell.territoryIndex;
                        seg.disputingTerritory1Index = territory1Index;
                        seg.disputingTerritory2Index = territory2Index;
                        if (territory2Index != territory1Index) {
                            territoryFrontiers.Add(seg);
                            if (validCell) {
                                territoryConnectors[territory1Index].Add(seg);
                                Territory territory1 = territories[territory1Index];
                                // check segment ownership
                                if (territory2Index >= 0) {
                                    Territory territory2 = territories[territory2Index];
                                    if (!territory1.visible && !territory2.visible) {
                                        seg.territoryIndex = -9; // hide segment
                                        frontier.region2 = frontier.region1;
                                        frontier.region1 = cell.region;
                                    } else if (territory1.neutral && territory2.neutral) {
                                        seg.territoryIndex = territory1Index;
                                        frontier.region2 = frontier.region1;
                                        frontier.region1 = cell.region;
                                    } else if ((territory1.neutral || !territory1.visible) && !territory2.neutral) {
                                        seg.territoryIndex = territory2Index;
                                        frontier.region2 = cell.region;
                                    } else if (!territory1.neutral && (territory2.neutral || !territory2.visible)) {
                                        seg.territoryIndex = territory1Index;
                                        frontier.region2 = frontier.region1;
                                        frontier.region1 = cell.region;
                                    } else {
                                        seg.territoryIndex = -1; // if segment belongs to a visible cell and valid territory2, mark this segment as disputed. Otherwise make it part of territory1
                                        frontier.region2 = cell.region;
                                    }
                                } else {
                                    seg.territoryIndex = territory1Index;
                                    frontier.region2 = cell.region;
                                }

                                if (seg.territoryIndex < 0) {
                                    // add territory neigbhours
                                    Territory territory2 = territories[territory2Index];
                                    if (!territory1.neighbours.Contains(territory2)) {
                                        territory1.neighbours.Add(territory2);
                                    }
                                    if (!territory2.neighbours.Contains(territory1)) {
                                        territory2.neighbours.Add(territory1);
                                    }
                                }
                            }
                            if (territory2Index >= 0) {
                                territoryConnectors[territory2Index].Add(seg);
                            }
                        }
                    } else {
                        if (frontierPoolUsed >= frontierPool.Length) {
                            // Resize frontier pool
                            int newLen = frontierPool.Length * 2;
                            Frontier[] newPool = new Frontier[newLen];
                            Array.Copy(frontierPool, newPool, frontierPool.Length);
                            for (int f = frontierPool.Length; f < newLen; f++) {
                                newPool[f] = new Frontier();
                            }
                            frontierPool = newPool;
                        }
                        // Get it from pool
                        frontier = frontierPool[frontierPoolUsed++];
                        frontier.region1 = cellRegion;
                        frontier.region2 = null;
                        territoryNeighbourHit[seg] = frontier;
                        seg.disputingTerritory1Index = seg.disputingTerritory2Index = seg.territoryIndex = cell.territoryIndex;
                        if (!validCell) {
                            seg.territoryIndex = -9; // hide segment
                        }
                    }
                }
            }

            List<Region> newRegions = new List<Region>();
            territoriesRegionsCount = 0;
            for (int k = 0; k < terrCount; k++) {
                Territory terr = territories[k];
                if (issueRedraw == RedrawType.IncrementalTerritories && !terr.isDirty) continue;
                newRegions.Clear();
                if (territories[k].cells.Count > 0) {
                    Polygon polygon = territoryConnectors[k].ToPolygon();
                    // remove any contour that's contained by another contour
                    int contoursCount = polygon.contours.Count;
                    for (int c = 0; c < contoursCount; c++) {
                        Contour contour1 = polygon.contours[c];
                        if (contour1.points.Count < 3) {
                            polygon.contours.RemoveAt(c);
                            contoursCount--;
                            c--;
                            continue;
                        }
                        for (int d = 0; d < contoursCount; d++) {
                            if (d == c) continue;
                            Contour contour2 = polygon.contours[d];
                            int pointsCount = contour2.points.Count;
                            if (pointsCount < 3 || (contour1.ContainsPoint(contour2.points[0]) && contour1.ContainsPoint(contour2.points[pointsCount / 2]))) {
                                polygon.contours.RemoveAt(d);
                                contoursCount--;
                                d--;
                                c--;
                                continue;
                            }
                        }
                    }

                    for (int c = 0; c < polygon.contours.Count; c++) {
                        Polygon regionPoly = new Polygon();
                        regionPoly.AddContour(polygon.contours[c]);
                        Region region = new Region(terr, false);
                        region.polygon = regionPoly;
                        newRegions.Add(region);
                    }
                }
                // Update regions
                if (terr.regions == null) {
                    terr.regions = new List<Region>();
                    terr.regions.AddRange(newRegions);
                } else if (terr.regions.Count == 0) {
                    terr.regions.AddRange(newRegions);
                } else {
                    while (terr.regions.Count > newRegions.Count) {
                        terr.regions.RemoveAt(terr.regions.Count - 1);
                    }
                    for (int r = 0; r < newRegions.Count; r++) {
                        if (terr.regions.Count <= r) {
                            Region region = terr.regions[0].Clone();
                            terr.regions.Add(region);
                        }
                        terr.regions[r].polygon = newRegions[r].polygon;
                        terr.regions[r].points.Clear();
                        terr.regions[r].segments.Clear();
                    }
                }
                territoriesRegionsCount += terr.regions.Count;
            }
        }
        /// <summary>
        /// Subdivides the segment in smaller segments
        /// </summary>
        /// <returns><c>true</c>, if segment was drawn, <c>false</c> otherwise.</returns>
        void SurfaceSegmentForMesh(Vector3 p0, Vector3 p1) {

            // trace the line until roughness is exceeded
            float dist = (float)Math.Sqrt((p1.x - p0.x) * (p1.x - p0.x) + (p1.y - p0.y) * (p1.y - p0.y));
            Vector3 direction = p1 - p0;

            int numSteps = (int)(meshStep * dist);
            EnsureCapacityFrontiersPoints(numSteps * 2);

            Vector3 t0 = p0;
            float h0 = _terrainWrapper.SampleHeight(transform.TransformPoint(t0));
            if (_gridNormalOffset > 0) {
                Vector3 invNormal = transform.InverseTransformVector(_terrainWrapper.GetInterpolatedNormal(t0.x + 0.5f, t0.y + 0.5f));
                t0 += invNormal * _gridNormalOffset;
            }
            t0.z -= h0;
            Vector3 ta = t0;
            float h1, ha = h0;
            for (int i = 1; i < numSteps; i++) {
                Vector3 t1 = p0 + direction * ((float)i / numSteps);
                h1 = _terrainWrapper.SampleHeight(transform.TransformPoint(t1));
                if (h1 - h0 > maxRoughnessWorldSpace || h0 - h1 > maxRoughnessWorldSpace) {
                    frontiersPoints[frontiersPointsCount++] = t0;
                    if (t0 != ta) {
                        if (_gridNormalOffset > 0) {
                            Vector3 invNormal = transform.InverseTransformVector(_terrainWrapper.GetInterpolatedNormal(ta.x + 0.5f, ta.y + 0.5f));
                            ta += invNormal * _gridNormalOffset;
                        }
                        ta.z -= ha;
                        frontiersPoints[frontiersPointsCount++] = ta;
                        frontiersPoints[frontiersPointsCount++] = ta;
                    }
                    if (_gridNormalOffset > 0) {
                        Vector3 invNormal = transform.InverseTransformVector(_terrainWrapper.GetInterpolatedNormal(t1.x + 0.5f, t1.y + 0.5f));
                        t1 += invNormal * _gridNormalOffset;
                    }
                    t1.z -= h1;
                    frontiersPoints[frontiersPointsCount++] = t1;
                    t0 = t1;
                    h0 = h1;
                }
                ta = t1;
                ha = h1;
            }
            // Add last point
            h1 = _terrainWrapper.SampleHeight(transform.TransformPoint(p1));
            if (_gridNormalOffset > 0) {
                Vector3 invNormal = transform.InverseTransformVector(_terrainWrapper.GetInterpolatedNormal(p1.x + 0.5f, p1.y + 0.5f));
                p1 += invNormal * _gridNormalOffset;
            }
            p1.z -= h1;
            frontiersPoints[frontiersPointsCount++] = t0;
            frontiersPoints[frontiersPointsCount++] = p1;
        }
        void GenerateCellsMesh(List<Cell> cells) {

            if (segmentHit == null) {
                segmentHit = new Dictionary<Segment, bool>(50000);
            } else {
                segmentHit.Clear();
            }

            if (frontiersPoints == null || frontiersPoints.Length == 0) {
                frontiersPoints = new Vector3[100000];
            }
            frontiersPointsCount = 0;

            int cellCount = cells.Count;
            if (_terrainWrapper == null) {
                for (int k = 0; k < cellCount; k++) {
                    Cell cell = cells[k];
                    if (cell != null && cell.visible && cell.borderVisible) {
                        Region region = cell.region;
                        int numSegments = region.segments.Count;
                        EnsureCapacityFrontiersPoints(numSegments * 2);
                        for (int i = 0; i < numSegments; i++) {
                            Segment s = region.segments[i];
                            if (!segmentHit.ContainsKey(s)) {
                                segmentHit[s] = true;
                                frontiersPoints[frontiersPointsCount++] = GetScaledVector(s.start);
                                frontiersPoints[frontiersPointsCount++] = GetScaledVector(s.end);
                            }
                        }
                    }
                }
            } else {
                meshStep = (2.0f - _gridRoughness) / (float)MIN_VERTEX_DISTANCE;
                for (int k = 0; k < cellCount; k++) {
                    Cell cell = cells[k];
                    if (cell != null && cell.visible && cell.borderVisible) {
                        Region region = cell.region;
                        int numSegments = region.segments.Count;
                        for (int i = 0; i < numSegments; i++) {
                            Segment s = region.segments[i];
                            if (!segmentHit.ContainsKey(s)) {
                                segmentHit[s] = true;
                                SurfaceSegmentForMesh(GetScaledVector(s.start.vector3), GetScaledVector(s.end.vector3));
                            }
                        }
                    }
                }
            }

            int maxPointsPerMesh = 65504;
            if (isUsingVertexDispForVertexThickness) {
                maxPointsPerMesh /= 4;
            }

            int meshGroups = (frontiersPointsCount / maxPointsPerMesh) + 1;
            int meshIndex = -1;
            if (cellMeshIndices == null || cellMeshIndices.GetUpperBound(0) != meshGroups - 1) {
                cellMeshIndices = new int[meshGroups][];
                cellMeshBorders = new Vector3[meshGroups][];
            }
            if (frontiersPointsCount == 0) {
                cellMeshBorders[0] = new Vector3[0];
                cellMeshIndices[0] = new int[0];
            } else {
                // Clamp points to minimum elevation
                if (_cellsMinimumAltitudeClampVertices && _cellsMinimumAltitude != 0) {
                    float localMinima = -(_cellsMinimumAltitude - _terrainWrapper.bounds.min.y) / _terrainWrapper.transform.lossyScale.y;
                    for (int k = 0; k < frontiersPointsCount; k++) {
                        if (frontiersPoints[k].z > localMinima) {
                            frontiersPoints[k].z = localMinima;
                        }
                    }
                }

                // Clamp points to maximum elevation
                if (_cellsMaximumAltitudeClampVertices && _cellsMaximumAltitude != 0) {
                    float localMaxima = -(_cellsMaximumAltitude - _terrainWrapper.bounds.min.y) / _terrainWrapper.transform.lossyScale.y;
                    for (int k = 0; k < frontiersPointsCount; k++) {
                        if (frontiersPoints[k].z < localMaxima) {
                            frontiersPoints[k].z = localMaxima;
                        }
                    }
                }

                for (int k = 0; k < frontiersPointsCount; k += maxPointsPerMesh) {
                    int max = Mathf.Min(frontiersPointsCount - k, maxPointsPerMesh);
                    ++meshIndex;
                    if (cellMeshBorders[meshIndex] == null || cellMeshBorders[0].GetUpperBound(0) != max - 1) {
                        cellMeshBorders[meshIndex] = new Vector3[max];
                        cellMeshIndices[meshIndex] = new int[max];
                    }
                    for (int j = 0; j < max; j++) {
                        cellMeshBorders[meshIndex][j] = frontiersPoints[j + k];
                        cellMeshIndices[meshIndex][j] = j;
                    }
                }
            }
        }
        void CreateTerritories() {

            if (factoryColors == null) return;

            _numTerritories = Mathf.Clamp(_numTerritories, 1, cellCount);

            if (!_colorizeTerritories && !_showTerritories && _highlightMode != HIGHLIGHT_MODE.Territories) {
                if (territories != null)
                    territories.Clear();
                if (territoryLayer != null)
                    DestroyImmediate(territoryLayer);
                return;
            }

            if (territories == null) {
                territories = new List<Territory>(_numTerritories);
            } else {
                territories.Clear();
            }

            CheckCells();
            // Freedom for the cells!...
            int cellsCount = cells.Count;
            for (int k = 0; k < cellsCount; k++) {
                cells[k].territoryIndex = -1;
            }

            for (int c = 0; c < _numTerritories; c++) {
                Territory territory = new Territory(c.ToString());
                territory.fillColor = factoryColors[c];
                int territoryIndex = territories.Count;
                int p = UnityEngine.Random.Range(0, cellsCount);
                int z = 0;
                while ((cells[p].territoryIndex != -1 || !cells[p].visible) && z++ <= cellsCount) {
                    p++;
                    if (p >= cellsCount)
                        p = 0;
                }
                if (z > cellsCount)
                    break; // no more territories can be found - this should not happen
                Cell cell = cells[p];
                cell.territoryIndex = territoryIndex;
                territory.center = cell.center;
                territory.cells.Add(cell);
                territories.Add(territory);
            }

            // Continue conquering cells
            int[] territoryCellIndex = new int[territories.Count];

            // Iterate one cell per country (this is not efficient but ensures balanced distribution)
            bool remainingCells = true;
            while (remainingCells) {
                remainingCells = false;
                int terrCount = territories.Count;
                for (int k = 0; k < terrCount; k++) {
                    Territory territory = territories[k];
                    int territoryCellsCount = territory.cells.Count;
                    for (int p = territoryCellIndex[k]; p < territoryCellsCount; p++) {
                        Cell cell = territory.cells[p];
                        int nCount = cell.neighbours.Count;
                        for (int n = 0; n < nCount; n++) {
                            Cell otherCell = cell.neighbours[n];
                            if (otherCell.territoryIndex == -1 && otherCell.visible) {
                                otherCell.territoryIndex = k;
                                territory.cells.Add(otherCell);
                                territoryCellsCount++;
                                remainingCells = true;
                                p = territoryCellsCount;
                                break;
                            }
                        }
                        if (p < territoryCellsCount) {// no free neighbours left for this cell
                            territoryCellIndex[k]++;
                        }
                    }
                }
            }

            FindTerritoryFrontiers();
            UpdateTerritoryBoundaries();

            recreateTerritories = false;

        }
        void UpdateTerritoryBoundaries() {
            if (territories == null)
                return;

            // Update territory region
            int terrCount = territories.Count;
            for (int k = 0; k < terrCount; k++) {
                Territory territory = territories[k];
                if (issueRedraw == RedrawType.IncrementalTerritories && !territory.isDirty) continue;
                int regionsCount = territory.regions.Count;
                for (int r = 0; r < regionsCount; r++) {
                    Region territoryRegion = territory.regions[r];
                    if (territoryRegion.polygon == null) {
                        continue;
                    }

                    List<Vector2> regionPoints = territoryRegion.points;
                    territoryRegion.polygon.contours[0].GetVector2Points(_gridCenter, _gridScale, ref regionPoints);
                    territoryRegion.SetPoints(regionPoints);
                    territory.scaledCenter = GetScaledVector(territory.center);

                    regionPoints = null;

                    List<Point> points = territoryRegion.polygon.contours[0].points;
                    int pointCount = points.Count;

                    int segCount = territoryRegion.segments.Count;
                    if (segCount < pointCount) {
                        territoryRegion.segments.Clear();
                        for (int j = 0; j < pointCount; j++) {
                            Point p0 = points[j];
                            Point p1;
                            if (j == pointCount - 1) {
                                p1 = points[0];
                            } else {
                                p1 = points[j + 1];
                            }
                            territoryRegion.segments.Add(new Segment(p0, p1));
                        }
                    } else {
                        while (segCount > pointCount) {
                            territoryRegion.segments.RemoveAt(--segCount);
                        }
                        for (int j = 0; j < pointCount; j++) {
                            Point p0 = points[j];
                            Point p1;
                            if (j == pointCount - 1) {
                                p1 = points[0];
                            } else {
                                p1 = points[j + 1];
                            }
                            territoryRegion.segments[j].Init(p0, p1);
                        }
                    }
                }
            }

            ComputeSortedTerritoriesRegions();

            // For each region, compute if it's within another region and increase its sort index
            territoriesRegionsCount = _sortedTerritoriesRegions.Count;
            for (int k = territoriesRegionsCount - 1; k >= 0; k--) { // need to be >= 0 so cells are computed for all territory regions
                Region biggerRegion = _sortedTerritoriesRegions[k];
                // Compute region cells
                Territory territory = (Territory)biggerRegion.entity;
                if (issueRedraw != RedrawType.IncrementalTerritories || territory.isDirty) {
                    ComputeTerritoryRegionCells(biggerRegion);
                }
                biggerRegion.sortIndex = 0;
                for (int j = k - 1; j >= 0; j--) {
                    Region smallerRegion = _sortedTerritoriesRegions[j];
                    Vector2 bottomLeft = smallerRegion.rect2D.min;
                    Vector2 topRight = smallerRegion.rect2D.max;
                    Vector2 bottomRight = new Vector2(topRight.x, bottomLeft.y);
                    Vector2 topLeft = new Vector2(bottomLeft.x, topRight.y);
                    if (biggerRegion.rect2D.Contains(bottomLeft) && biggerRegion.rect2D.Contains(bottomRight) && biggerRegion.rect2D.Contains(topLeft) && biggerRegion.rect2D.Contains(topRight)) {
                        biggerRegion.sortIndex++;
                    }
                }
            }

        }
        void ComputeSortedTerritoriesRegions() {
            _sortedTerritoriesRegions.Clear();
            foreach (Territory terr in territories) {
                _sortedTerritoriesRegions.AddRange(terr.regions);
            }
            _sortedTerritoriesRegions.Sort(delegate (Region x, Region y) {
                return x.rect2DArea.CompareTo(y.rect2DArea);
            });
        }
        void GenerateTerritoriesMesh() {
            if (territories == null)
                return;

            if (frontiersPoints == null || frontiersPoints.Length == 0) {
                frontiersPoints = new Vector3[10000];
            }

            int terrCount = territories.Count;

            TerritoryMesh tm, tmDisputed = null;
            if (issueRedraw == RedrawType.IncrementalTerritories && territoryMeshes != null) {
                if (territoryFrontiers == null)
                    return;
                int territoryMeshesCount = territoryMeshes.Count;
                for (int k = 0; k < territoryMeshesCount; k++) {
                    int terrIndex = territoryMeshes[k].territoryIndex;
                    if (terrIndex >= 0) {
                        Territory territory = territories[terrIndex];
                        if (territory.isDirty) {
                            tm = territoryMeshes[k];
                            GenerateTerritoryMesh(tm, false);
                        }
                    } else {
                        tmDisputed = territoryMeshes[k];
                    }
                }
            } else {
                if (territoryMeshes == null) {
                    territoryMeshes = new List<TerritoryMesh>(terrCount + 1);
                } else {
                    territoryMeshes.Clear();
                }

                if (territoryFrontiers == null)
                    return;

                for (int k = 0; k < terrCount; k++) {
                    tm = new TerritoryMesh {
                        territoryIndex = k
                    };
                    if (GenerateTerritoryMesh(tm, false)) {
                        territoryMeshes.Add(tm);
                    }
                }
            }

            // Generate disputed frontiers
            if (tmDisputed == null) {
                tmDisputed = new TerritoryMesh {
                    territoryIndex = -1
                };
                if (GenerateTerritoryMesh(tmDisputed, false)) {
                    territoryMeshes.Add(tmDisputed);
                }
            } else if (!GenerateTerritoryMesh(tmDisputed, false)) {
                territoryMeshes.Remove(tmDisputed);
            }
        }
        /// <summary>
        /// Generates the territory mesh.
        /// </summary>
        /// <returns>True if something was produced.</returns>
        bool GenerateTerritoryMesh(TerritoryMesh tm, bool generateCustomFrontier, int adjacentTerritoryIndex = -1) {

            frontiersPointsCount = 0;

            int territoryFrontiersCount = territoryFrontiers.Count;
            if (_terrainWrapper == null) {
                EnsureCapacityFrontiersPoints(territoryFrontiersCount * 2);
                for (int k = 0; k < territoryFrontiersCount; k++) {
                    Segment s = territoryFrontiers[k];
                    if (generateCustomFrontier) {
                        if (s.disputingTerritory1Index != tm.territoryIndex && s.disputingTerritory2Index != tm.territoryIndex) continue;
                        if (adjacentTerritoryIndex != -1 && s.disputingTerritory1Index != adjacentTerritoryIndex && s.disputingTerritory2Index != adjacentTerritoryIndex) continue;
                    } else if (s.territoryIndex != tm.territoryIndex) {
                        continue;
                    }
                    if (s.territoryIndex >= 0 && !territories[s.territoryIndex].borderVisible)
                        continue;
                    if (!s.border || _showTerritoriesOuterBorder) {
                        frontiersPoints[frontiersPointsCount++] = GetScaledVector(s.startToVector3);
                        frontiersPoints[frontiersPointsCount++] = GetScaledVector(s.endToVector3);
                    }
                }
            } else {
                meshStep = (2.0f - _gridRoughness) / (float)MIN_VERTEX_DISTANCE;
                for (int k = 0; k < territoryFrontiersCount; k++) {
                    Segment s = territoryFrontiers[k];
                    if (generateCustomFrontier) {
                        if (s.disputingTerritory1Index != tm.territoryIndex && s.disputingTerritory2Index != tm.territoryIndex) continue;
                        if (adjacentTerritoryIndex != -1 && s.disputingTerritory1Index != adjacentTerritoryIndex && s.disputingTerritory2Index != adjacentTerritoryIndex) continue;
                    } else if (s.territoryIndex != tm.territoryIndex) {
                        continue;
                    }
                    if (s.territoryIndex >= 0 && !territories[s.territoryIndex].borderVisible)
                        continue;
                    if (!s.border || _showTerritoriesOuterBorder) {
                        SurfaceSegmentForMesh(GetScaledVector(s.start.vector3), GetScaledVector(s.end.vector3));
                    }
                }

            }

            int maxPointsPerMesh = 65504;
            if (isUsingVertexDispForTerritoryThickness) {
                maxPointsPerMesh /= 4;
            }

            int meshGroups = (frontiersPointsCount / maxPointsPerMesh) + 1;
            int meshIndex = -1;
            if (tm.territoryMeshIndices == null || tm.territoryMeshIndices.GetUpperBound(0) != meshGroups - 1) {
                tm.territoryMeshIndices = new int[meshGroups][];
                tm.territoryMeshBorders = new Vector3[meshGroups][];
            }

            // Clamp points to minimum elevation
            if (_cellsMinimumAltitudeClampVertices && _cellsMinimumAltitude != 0) {
                float localMinima = -(_cellsMinimumAltitude - _terrainWrapper.bounds.min.y) / _terrainWrapper.transform.lossyScale.y;
                for (int k = 0; k < frontiersPointsCount; k++) {
                    if (frontiersPoints[k].z > localMinima) {
                        frontiersPoints[k].z = localMinima;
                    }
                }
            }

            // Clamp points to maximum elevation
            if (_cellsMaximumAltitudeClampVertices && _cellsMaximumAltitude != 0) {
                float localMaxima = -(_cellsMaximumAltitude - _terrainWrapper.bounds.min.y) / _terrainWrapper.transform.lossyScale.y;
                for (int k = 0; k < frontiersPointsCount; k++) {
                    if (frontiersPoints[k].z < localMaxima) {
                        frontiersPoints[k].z = localMaxima;
                    }
                }
            }


            for (int k = 0; k < frontiersPointsCount; k += maxPointsPerMesh) {
                int max = Mathf.Min(frontiersPointsCount - k, maxPointsPerMesh);
                ++meshIndex;
                if (tm.territoryMeshBorders[meshIndex] == null || tm.territoryMeshBorders[meshIndex].GetUpperBound(0) != max - 1) {
                    tm.territoryMeshBorders[meshIndex] = new Vector3[max];
                    tm.territoryMeshIndices[meshIndex] = new int[max];
                }
                for (int j = 0; j < max; j++) {
                    tm.territoryMeshBorders[meshIndex][j] = frontiersPoints[j + k];
                    tm.territoryMeshIndices[meshIndex][j] = j;
                }
            }

            return frontiersPointsCount > 0;
        }
        void EnsureCapacityFrontiersPoints(int payload) {
            while (frontiersPointsCount + payload >= frontiersPoints.Length) {
                Vector3[] v = new Vector3[frontiersPoints.Length * 2];
                for (int k = 0; k < frontiersPointsCount; k++) {
                    v[k] = frontiersPoints[k];
                }
                frontiersPoints = v;
            }
        }
        void FitToTerrain() {
            if (_terrainWrapper == null || cameraMain == null)
                return;

            // Fit to terrain
            Vector3 terrainSize = _terrainWrapper.size;
            terrainWidth = terrainSize.x;
            terrainHeight = terrainSize.y;
            terrainDepth = terrainSize.z;
            float angleY = transform.eulerAngles.y;
            transform.localRotation = Quaternion.Euler(90, angleY, 0);
            Vector3 lossyScale = _terrainObject.transform.lossyScale;
            transform.localScale = new Vector3(terrainWidth / lossyScale.x, terrainDepth / lossyScale.z, 1);
            maxRoughnessWorldSpace = _gridRoughness * terrainHeight;

            if (_terrainWrapper is MeshTerrainWrapper) {
                ((MeshTerrainWrapper)_terrainWrapper).pivot = _terrainMeshPivot;
            }

            Vector3 camPos = cameraMain.transform.position;
            if (transform.parent != null && transform.parent.lossyScale != lastParentScale) {
                gridNeedsUpdate = true;
            } else if (transform.rotation != lastRot) {
                gridNeedsUpdate = true;
                issueRedraw = RedrawType.Full;
            } else if (transform.position != lastPos || transform.lossyScale != lastScale) {
                gridNeedsUpdate = true;
            }
            bool refresh = gridNeedsUpdate || gridElevationCurrent != lastGridElevation || _gridCameraOffset != lastGridCameraOffset || _gridMinElevationMultiplier != lastGridMinElevationMultiplier || camPos != lastCamPos;
            if (refresh) {
                if (gridNeedsUpdate) {
                    gridNeedsUpdate = false;
                    _terrainWrapper.Refresh();
                }
                Vector3 localPosition = _terrainWrapper.localCenter;
                localPosition.y += 0.01f * _gridMinElevationMultiplier + gridElevationCurrent;
                if (_gridCameraOffset > 0) {
                    localPosition += (camPos - transform.position).normalized * (camPos - _terrainWrapper.bounds.center).sqrMagnitude * _gridCameraOffset * 0.001f;
                }
                transform.localPosition = localPosition;
                lastPos = transform.position;
                lastRot = transform.rotation;
                lastScale = transform.lossyScale;
                if (transform.parent != null) {
                    lastParentScale = transform.parent.lossyScale;
                }
                lastCamPos = camPos;
                lastGridElevation = gridElevationCurrent;
                lastGridCameraOffset = _gridCameraOffset;
                lastGridMinElevationMultiplier = _gridMinElevationMultiplier;
            }
        }
        void BuildTerrainWrapper() {
            // migration
            if (_terrain != null && _terrainObject == null) {
                _terrainObject = _terrain.gameObject;
                _terrain = null;
            }
            if (_terrainObject == null) {
                _terrainWrapper = null;
            } else if ((_terrainWrapper == null && _terrainObject != null) || (_terrainWrapper != null && (_terrainWrapper.gameObject != _terrainObject || _terrainWrapper.heightmapWidth != _heightmapSize || _terrainWrapper.heightmapHeight != _heightmapSize))) {
                _terrainWrapper = UnityTerrainWrapper.GetTerrainWrapper(_terrainObject, ref _terrainObjectsPrefix, _heightmapSize, _heightmapSize);
            }
        }
        bool UpdateTerrainReference(bool reuseTerrainData) {

            MeshRenderer renderer = GetComponent<MeshRenderer>();
            renderer.sortingOrder = _sortingOrder;
            if (_terrainWrapper == null) {
                if (renderer.enabled && _transparentBackground) {
                    renderer.enabled = false;
                } else if (!renderer.enabled && !_transparentBackground) {
                    renderer.enabled = true;
                }

                if (transform.parent != null && transform.GetComponentInParent<Terrain>() != null) {
                    transform.SetParent(null);
                    transform.localScale = new Vector3(100, 100, 1);
                    transform.localRotation = Quaternion.Euler(0, 0, 0);
                }
                // Check if box collider exists
                BoxCollider2D bc = GetComponent<BoxCollider2D>();
                if (bc == null) {
                    MeshCollider mc = GetComponent<MeshCollider>();
                    if (mc == null)
                        gameObject.AddComponent<MeshCollider>();
                }
            } else {
                transform.SetParent(_terrainWrapper.transform, false);
                if (renderer.enabled) {
                    renderer.enabled = false;
                }
                if (Application.isPlaying) {
                    _terrainWrapper.SetupTriggers(this);
                }
                MeshCollider mc = GetComponent<MeshCollider>();
                if (mc != null) {
                    DestroyImmediate(mc);
                }
                lastCamPos = cameraMain.transform.position - Vector3.up; // just to force update on first frame
                FitToTerrain();
                lastCamPos = cameraMain.transform.position - Vector3.up; // just to force update on first update as well
                if (CalculateTerrainRoughness(reuseTerrainData)) {
                    refreshCellMesh = true;
                    refreshTerritoriesMesh = true;
                    // Clear geometry
                    if (cellLayer != null) {
                        DestroyImmediate(cellLayer);
                    }
                    if (territoryLayer != null) {
                        DestroyImmediate(territoryLayer);
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// Calculates the terrain roughness.
        /// </summary>
        /// <returns><c>true</c>, if terrain roughness has changed, <c>false</c> otherwise.</returns>
        bool CalculateTerrainRoughness(bool reuseTerrainData) {
            if (reuseTerrainData && _terrainWrapper.heightmapWidth == heightMapWidth && _terrainWrapper.heightmapHeight == heightMapHeight && terrainHeights != null && terrainRoughnessMap != null) {
                return false;
            }
            _terrainWrapper.Refresh();
            heightMapWidth = _terrainWrapper.heightmapWidth;
            heightMapHeight = _terrainWrapper.heightmapHeight;
            terrainHeights = _terrainWrapper.GetHeights(0, 0, heightMapWidth, heightMapHeight);
            terrainRoughnessMapWidth = heightMapWidth / TERRAIN_CHUNK_SIZE;
            terrainRoughnessMapHeight = heightMapHeight / TERRAIN_CHUNK_SIZE;
            int length = terrainRoughnessMapHeight * terrainRoughnessMapWidth;
            if (terrainRoughnessMap == null || terrainRoughnessMap.Length != length) {
                terrainRoughnessMap = new float[length];
                tempTerrainRoughnessMap = new float[length];
            } else {
                for (int k = 0; k < length; k++) {
                    terrainRoughnessMap[k] = 0;
                    tempTerrainRoughnessMap[k] = 0;
                }
            }

#if SHOW_DEBUG_GIZMOS
			if (GameObject.Find ("ParentDot")!=null) DestroyImmediate(GameObject.Find ("ParentDot"));
			GameObject parentDot = new GameObject("ParentDot");
            disposalManager.MarkForDisposal(parentDot);
			parentDot.transform.position = Misc.Vector3zero;
#endif

            float maxStep = (float)TERRAIN_CHUNK_SIZE / heightMapWidth;
            float minStep = 1.0f / heightMapWidth;
            float roughnessFactor = _gridRoughness * 5.0f + 0.00001f;
            for (int y = 0, l = 0; l < terrainRoughnessMapHeight; y += TERRAIN_CHUNK_SIZE, l++) {
                int linePos = l * terrainRoughnessMapWidth;
                for (int x = 0, c = 0; c < terrainRoughnessMapWidth; x += TERRAIN_CHUNK_SIZE, c++) {
                    int j0 = y == 0 ? 1 : y;
                    int j1 = y + TERRAIN_CHUNK_SIZE;
                    int k0 = x == 0 ? 1 : x;
                    int k1 = x + TERRAIN_CHUNK_SIZE;
                    float maxDiff = 0;
                    for (int j = j0; j < j1; j++) {
                        for (int k = k0; k < k1; k++) {
                            float mh = terrainHeights[j, k];
                            float diff = mh - terrainHeights[j, k - 1];
                            if (diff > maxDiff) {
                                maxDiff = diff;
                            } else if (-diff > maxDiff) {
                                maxDiff = -diff;
                            }
                            diff = mh - terrainHeights[j + 1, k - 1];
                            if (diff > maxDiff) {
                                maxDiff = diff;
                            } else if (-diff > maxDiff) {
                                maxDiff = -diff;
                            }
                            diff = mh - terrainHeights[j + 1, k];
                            if (diff > maxDiff) {
                                maxDiff = diff;
                            } else if (-diff > maxDiff) {
                                maxDiff = -diff;
                            }
                            diff = mh - terrainHeights[j + 1, k + 1];
                            if (diff > maxDiff) {
                                maxDiff = diff;
                            } else if (-diff > maxDiff) {
                                maxDiff = -diff;
                            }
                            diff = mh - terrainHeights[j, k + 1];
                            if (diff > maxDiff) {
                                maxDiff = diff;
                            } else if (-diff > maxDiff) {
                                maxDiff = -diff;
                            }
                            diff = mh - terrainHeights[j - 1, k + 1];
                            if (diff > maxDiff) {
                                maxDiff = diff;
                            } else if (-diff > maxDiff) {
                                maxDiff = -diff;
                            }
                            diff = mh - terrainHeights[j - 1, k];
                            if (diff > maxDiff) {
                                maxDiff = diff;
                            } else if (-diff > maxDiff) {
                                maxDiff = -diff;
                            }
                            diff = mh - terrainHeights[j - 1, k - 1];
                            if (diff > maxDiff) {
                                maxDiff = diff;
                            } else if (-diff > maxDiff) {
                                maxDiff = -diff;
                            }
                        }
                    }
                    maxDiff /= roughnessFactor;
                    float t = (1.0f - maxDiff) / (1.0f + maxDiff);
                    if (t <= 0)
                        maxDiff = minStep;
                    else if (t >= 1f)
                        maxDiff = maxStep;
                    else
                        maxDiff = minStep * (1f - t) + maxStep * t; // Mathf.Lerp (minStep, maxStep,t);

                    tempTerrainRoughnessMap[linePos + c] = maxDiff;
                }
            }

            // collapse chunks with low gradient
            float flatThreshold = maxStep * (1.0f - _gridRoughness * 0.1f);
            for (int j = 0; j < terrainRoughnessMapHeight; j++) {
                int jPos = j * terrainRoughnessMapWidth;
                for (int k = 0; k < terrainRoughnessMapWidth - 1; k++) {
                    if (tempTerrainRoughnessMap[jPos + k] >= flatThreshold) {
                        int i = k + 1;
                        while (i < terrainRoughnessMapWidth && tempTerrainRoughnessMap[jPos + i] >= flatThreshold)
                            i++;
                        while (k < i && k < terrainRoughnessMapWidth)
                            tempTerrainRoughnessMap[jPos + k] = maxStep * (i - k++);
                    }
                }
            }

            // spread min step
            for (int l = 0; l < terrainRoughnessMapHeight; l++) {
                int linePos = l * terrainRoughnessMapWidth;
                int prevLinePos = linePos - terrainRoughnessMapWidth;
                int postLinePos = linePos + terrainRoughnessMapWidth;
                for (int c = 0; c < terrainRoughnessMapWidth; c++) {
                    minStep = tempTerrainRoughnessMap[linePos + c];
                    if (l > 0) {
                        if (tempTerrainRoughnessMap[prevLinePos + c] < minStep)
                            minStep = tempTerrainRoughnessMap[prevLinePos + c];
                        if (c > 0)
                            if (tempTerrainRoughnessMap[prevLinePos + c - 1] < minStep)
                                minStep = tempTerrainRoughnessMap[prevLinePos + c - 1];
                        if (c < terrainRoughnessMapWidth - 1)
                            if (tempTerrainRoughnessMap[prevLinePos + c + 1] < minStep)
                                minStep = tempTerrainRoughnessMap[prevLinePos + c + 1];
                    }
                    if (c > 0 && tempTerrainRoughnessMap[linePos + c - 1] < minStep)
                        minStep = tempTerrainRoughnessMap[linePos + c - 1];
                    if (c < terrainRoughnessMapWidth - 1 && tempTerrainRoughnessMap[linePos + c + 1] < minStep)
                        minStep = tempTerrainRoughnessMap[linePos + c + 1];
                    if (l < terrainRoughnessMapHeight - 1) {
                        if (tempTerrainRoughnessMap[postLinePos + c] < minStep)
                            minStep = tempTerrainRoughnessMap[postLinePos + c];
                        if (c > 0)
                            if (tempTerrainRoughnessMap[postLinePos + c - 1] < minStep)
                                minStep = tempTerrainRoughnessMap[postLinePos + c - 1];
                        if (c < terrainRoughnessMapWidth - 1)
                            if (tempTerrainRoughnessMap[postLinePos + c + 1] < minStep)
                                minStep = tempTerrainRoughnessMap[postLinePos + c + 1];
                    }
                    terrainRoughnessMap[linePos + c] = minStep;
                }
            }


#if SHOW_DEBUG_GIZMOS
            for (int l = 0; l < terrainRoughnessMapHeight - 1; l++) {
                for (int c = 0; c < terrainRoughnessMapWidth - 1; c++) {
                    float r = terrainRoughnessMap[l * terrainRoughnessMapWidth + c];
                    GameObject marker = Instantiate(Resources.Load<GameObject>("Prefabs/Dot"));
                    marker.transform.SetParent(parentDot.transform, false);
                    disposalManager.MarkForDisposal(marker);
                    marker.transform.localPosition = new Vector3(-terrainCenter.x + terrainWidth * ((float)c / 64 + 0.5f / 64), r * terrainHeight, -terrainCenter.z + terrainDepth * ((float)l / 64 + 0.5f / 64));
                    marker.transform.localScale = Misc.Vector3one * 350 / 512.0f;

                }
            }
#endif

            return true;
        }
        void UpdateMaterialDepthOffset() {
            if (territories != null) {
                int territoriesCount = territories.Count;
                for (int c = 0; c < territoriesCount; c++) {
                    Territory terr = territories[c];
                    for (int r = 0; r < terr.regions.Count; r++) {
                        int cacheIndex = GetCacheIndexForTerritoryRegion(c, r);
                        GameObject surf;
                        if (surfaces.TryGetValue(cacheIndex, out surf)) {
                            if (surf != null) {
                                Material mat = surf.GetComponent<Renderer>().sharedMaterial;
                                mat.SetInt(TGSConst.Offset, _gridSurfaceDepthOffsetTerritory);
                                SetBlend(mat);
                            }
                        }
                    }
                }
            }
            if (cells != null) {
                int cellsCount = cells.Count;
                for (int c = 0; c < cellsCount; c++) {
                    int cacheIndex = GetCacheIndexForCellRegion(c);
                    GameObject surf;
                    if (surfaces.TryGetValue(cacheIndex, out surf)) {
                        if (surf != null) {
                            Material mat = surf.GetComponent<Renderer>().sharedMaterial;
                            mat.SetInt(TGSConst.Offset, _gridSurfaceDepthOffset);
                            SetBlend(mat);
                        }
                    }
                }
            }
            float depthOffset = _gridMeshDepthOffset / 10000.0f;

            cellsThinMat.SetFloat(TGSConst.Offset, depthOffset);
            SetBlend(cellsThinMat);

            cellsGeoMat.SetFloat(TGSConst.Offset, depthOffset);
            SetBlend(cellsGeoMat);

            territoriesThinMat.SetFloat(TGSConst.Offset, depthOffset);
            SetBlend(territoriesThinMat);

            territoriesGeoMat.SetFloat(TGSConst.Offset, depthOffset);
            SetBlend(territoriesGeoMat);

            territoriesDisputedThinMat.SetFloat(TGSConst.Offset, depthOffset);
            SetBlend(territoriesDisputedThinMat);

            territoriesDisputedGeoMat.SetFloat(TGSConst.Offset, depthOffset);
            SetBlend(territoriesDisputedGeoMat);

            foreach (Material mat in frontierColorCache.Values) {
                mat.SetFloat(TGSConst.Offset, depthOffset);
                SetBlend(mat);
            }
            hudMatCellOverlay.SetInt(TGSConst.Offset, _gridSurfaceDepthOffset);
            hudMatCellGround.SetInt(TGSConst.Offset, _gridSurfaceDepthOffset - 1);
            hudMatTerritoryOverlay.SetInt(TGSConst.Offset, _gridSurfaceDepthOffsetTerritory);
            hudMatTerritoryGround.SetInt(TGSConst.Offset, _gridSurfaceDepthOffsetTerritory - 1);
        }
        void SetBlend(Material mat) {
            if (_transparentBackground) {
                mat.SetInt(TGSConst.ZWrite, 0);
                mat.SetInt(TGSConst.SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt(TGSConst.DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (mat.renderQueue < 3000) {
                    mat.renderQueue += 1000;
                }
            } else {
                mat.SetInt(TGSConst.ZWrite, 0);
                mat.SetInt(TGSConst.SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt(TGSConst.DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (mat.renderQueue >= 3000) {
                    mat.renderQueue -= 1000;
                }
            }
        }
        void SetBlendHighlight(Material mat) {

            if (_transparentBackground) {
                mat.SetInt(TGSConst.ZWrite, 0);
                mat.SetInt(TGSConst.SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt(TGSConst.DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                if (mat.renderQueue < 3000) {
                    mat.renderQueue += 1000;
                }
            } else {
                mat.SetInt(TGSConst.ZWrite, 0);
                mat.SetInt(TGSConst.SrcBlend, (int)UnityEngine.Rendering.BlendMode.One);
                mat.SetInt(TGSConst.DstBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                if (mat.renderQueue >= 3000) {
                    mat.renderQueue -= 1000;
                }
            }
        }
        void UpdatePreventOverdrawSettings() {
            SetStencil(hudMatTerritoryGround, STENCIL_MASK_HUD);
            SetStencil(hudMatCellGround, STENCIL_MASK_HUD);
            SetStencil(coloredMatGroundCell, STENCIL_MASK_CELL);
            SetStencil(coloredMatOverlayCell, STENCIL_MASK_CELL);
            SetStencil(texturizedMatGroundCell, STENCIL_MASK_CELL);
            SetStencil(texturizedMatOverlayCell, STENCIL_MASK_CELL);
            SetStencil(coloredMatGroundTerritory, STENCIL_MASK_TERRITORY);
            SetStencil(texturizedMatGroundTerritory, STENCIL_MASK_TERRITORY);
            SetStencil(coloredMatOverlayTerritory, STENCIL_MASK_TERRITORY);
            SetStencil(texturizedMatOverlayTerritory, STENCIL_MASK_TERRITORY);

            if (territories != null) {
                int territoriesCount = territories.Count;
                for (int c = 0; c < territoriesCount; c++) {
                    Territory terr = territories[c];
                    for (int r = 0; r < terr.regions.Count; r++) {
                        int cacheIndex = GetCacheIndexForTerritoryRegion(c, r);
                        GameObject surf;
                        if (surfaces.TryGetValue(cacheIndex, out surf)) {
                            if (surf != null) {
                                Material mat = surf.GetComponent<Renderer>().sharedMaterial;
                                SetStencil(mat, STENCIL_MASK_TERRITORY);
                            }
                        }
                    }
                }
            }
            if (cells != null) {
                int cellsCount = cells.Count;
                for (int c = 0; c < cellsCount; c++) {
                    int cacheIndex = GetCacheIndexForCellRegion(c);
                    GameObject surf;
                    if (surfaces.TryGetValue(cacheIndex, out surf)) {
                        if (surf != null) {
                            Material mat = surf.GetComponent<Renderer>().sharedMaterial;
                            SetStencil(mat, STENCIL_MASK_CELL);
                        }
                    }
                }
            }
        }
        void SetStencil(Material mat, int stencilMask) {
            if (mat == null) return;
            mat.SetInt(TGSConst.StencilRef, stencilMask);
            mat.SetInt(TGSConst.StencilComp, (int)UnityEngine.Rendering.CompareFunction.NotEqual);
            mat.SetInt(TGSConst.StencilOp, (int)UnityEngine.Rendering.StencilOp.Replace);
        }
        void UpdateMaterialNearClipFade() {
            if (_nearClipFadeEnabled && _terrainWrapper != null) {
                cellsThinMat.EnableKeyword(SKW_NEAR_CLIP_FADE);
                cellsThinMat.SetFloat(TGSConst.NearClip, _nearClipFade);
                cellsThinMat.SetFloat(TGSConst.FallOff, _nearClipFadeFallOff);

                cellsGeoMat.EnableKeyword(SKW_NEAR_CLIP_FADE);
                cellsGeoMat.SetFloat(TGSConst.NearClip, _nearClipFade);
                cellsGeoMat.SetFloat(TGSConst.FallOff, _nearClipFadeFallOff);

                territoriesThinMat.EnableKeyword(SKW_NEAR_CLIP_FADE);
                territoriesThinMat.SetFloat(TGSConst.NearClip, _nearClipFade);
                territoriesThinMat.SetFloat(TGSConst.FallOff, _nearClipFadeFallOff);

                territoriesGeoMat.EnableKeyword(SKW_NEAR_CLIP_FADE);
                territoriesGeoMat.SetFloat(TGSConst.NearClip, _nearClipFade);
                territoriesGeoMat.SetFloat(TGSConst.FallOff, _nearClipFadeFallOff);

                territoriesDisputedThinMat.EnableKeyword(SKW_NEAR_CLIP_FADE);
                territoriesDisputedThinMat.SetFloat(TGSConst.NearClip, _nearClipFade);
                territoriesDisputedThinMat.SetFloat(TGSConst.FallOff, _nearClipFadeFallOff);

                territoriesDisputedGeoMat.EnableKeyword(SKW_NEAR_CLIP_FADE);
                territoriesDisputedGeoMat.SetFloat(TGSConst.NearClip, _nearClipFade);
                territoriesDisputedGeoMat.SetFloat(TGSConst.FallOff, _nearClipFadeFallOff);

                foreach (Material mat in frontierColorCache.Values) {
                    mat.EnableKeyword(SKW_NEAR_CLIP_FADE);
                    mat.SetFloat(TGSConst.NearClip, _nearClipFade);
                    mat.SetFloat(TGSConst.FallOff, _nearClipFadeFallOff);
                }
            } else {
                cellsThinMat.DisableKeyword(SKW_NEAR_CLIP_FADE);
                cellsGeoMat.DisableKeyword(SKW_NEAR_CLIP_FADE);
                territoriesThinMat.DisableKeyword(SKW_NEAR_CLIP_FADE);
                territoriesGeoMat.DisableKeyword(SKW_NEAR_CLIP_FADE);
                territoriesDisputedThinMat.DisableKeyword(SKW_NEAR_CLIP_FADE);
                territoriesDisputedGeoMat.DisableKeyword(SKW_NEAR_CLIP_FADE);
                foreach (Material mat in frontierColorCache.Values) {
                    mat.DisableKeyword(SKW_NEAR_CLIP_FADE);
                }
            }
        }
        void UpdateMaterialFarFade() {
            if (_farFadeEnabled && _terrainWrapper != null) {
                cellsThinMat.EnableKeyword(SKW_FAR_FADE);
                cellsThinMat.SetFloat(TGSConst.FarFadeDistance, _farFadeDistance);
                cellsThinMat.SetFloat(TGSConst.FarFadeFallOff, _farFadeFallOff);

                cellsGeoMat.EnableKeyword(SKW_FAR_FADE);
                cellsGeoMat.SetFloat(TGSConst.FarFadeDistance, _farFadeDistance);
                cellsGeoMat.SetFloat(TGSConst.FarFadeFallOff, _farFadeFallOff);

                territoriesThinMat.EnableKeyword(SKW_FAR_FADE);
                territoriesThinMat.SetFloat(TGSConst.FarFadeDistance, _farFadeDistance);
                territoriesThinMat.SetFloat(TGSConst.FarFadeFallOff, _farFadeFallOff);

                territoriesGeoMat.EnableKeyword(SKW_FAR_FADE);
                territoriesGeoMat.SetFloat(TGSConst.FarFadeDistance, _farFadeDistance);
                territoriesGeoMat.SetFloat(TGSConst.FarFadeFallOff, _farFadeFallOff);

                territoriesDisputedThinMat.EnableKeyword(SKW_FAR_FADE);
                territoriesDisputedThinMat.SetFloat(TGSConst.FarFadeDistance, _farFadeDistance);
                territoriesDisputedThinMat.SetFloat(TGSConst.FarFadeFallOff, _farFadeFallOff);

                territoriesDisputedGeoMat.EnableKeyword(SKW_FAR_FADE);
                territoriesDisputedGeoMat.SetFloat(TGSConst.FarFadeDistance, _farFadeDistance);
                territoriesDisputedGeoMat.SetFloat(TGSConst.FarFadeFallOff, _farFadeFallOff);

                foreach (Material mat in frontierColorCache.Values) {
                    mat.EnableKeyword(SKW_FAR_FADE);
                    mat.SetFloat(TGSConst.FarFadeDistance, _farFadeDistance);
                    mat.SetFloat(TGSConst.FarFadeFallOff, _farFadeFallOff);
                }
            } else {
                cellsThinMat.DisableKeyword(SKW_FAR_FADE);
                cellsGeoMat.DisableKeyword(SKW_FAR_FADE);
                territoriesThinMat.DisableKeyword(SKW_FAR_FADE);
                territoriesGeoMat.DisableKeyword(SKW_FAR_FADE);
                territoriesDisputedThinMat.DisableKeyword(SKW_FAR_FADE);
                territoriesDisputedGeoMat.DisableKeyword(SKW_FAR_FADE);
                foreach (Material mat in frontierColorCache.Values) {
                    mat.DisableKeyword(SKW_FAR_FADE);
                }
            }
        }
        void UpdateMaterialCircularFade() {
            if (_circularFadeEnabled && _circularFadeTarget != null) {
                float circularFadeDistanceSqr = _circularFadeDistance * _circularFadeDistance;
                float circularFadeFallOff = _circularFadeFallOff * _circularFadeFallOff;

                cellsThinMat.EnableKeyword(SKW_CIRCULAR_FADE);
                cellsThinMat.SetFloat(TGSConst.CircularFadeDistanceSqr, circularFadeDistanceSqr);
                cellsThinMat.SetFloat(TGSConst.CircularFadeFallOff, circularFadeFallOff);

                cellsGeoMat.EnableKeyword(SKW_CIRCULAR_FADE);
                cellsGeoMat.SetFloat(TGSConst.CircularFadeDistanceSqr, circularFadeDistanceSqr);
                cellsGeoMat.SetFloat(TGSConst.CircularFadeFallOff, circularFadeFallOff);

                territoriesThinMat.EnableKeyword(SKW_CIRCULAR_FADE);
                territoriesThinMat.SetFloat(TGSConst.CircularFadeDistanceSqr, circularFadeDistanceSqr);
                territoriesThinMat.SetFloat(TGSConst.CircularFadeFallOff, circularFadeFallOff);

                territoriesGeoMat.EnableKeyword(SKW_CIRCULAR_FADE);
                territoriesGeoMat.SetFloat(TGSConst.CircularFadeDistanceSqr, circularFadeDistanceSqr);
                territoriesGeoMat.SetFloat(TGSConst.CircularFadeFallOff, circularFadeFallOff);

                territoriesDisputedThinMat.EnableKeyword(SKW_CIRCULAR_FADE);
                territoriesDisputedThinMat.SetFloat(TGSConst.CircularFadeDistanceSqr, circularFadeDistanceSqr);
                territoriesDisputedThinMat.SetFloat(TGSConst.CircularFadeFallOff, circularFadeFallOff);

                territoriesDisputedGeoMat.EnableKeyword(SKW_CIRCULAR_FADE);
                territoriesDisputedGeoMat.SetFloat(TGSConst.CircularFadeDistanceSqr, circularFadeDistanceSqr);
                territoriesDisputedGeoMat.SetFloat(TGSConst.CircularFadeFallOff, circularFadeFallOff);

                foreach (Material mat in frontierColorCache.Values) {
                    mat.EnableKeyword(SKW_CIRCULAR_FADE);
                    mat.SetFloat(TGSConst.CircularFadeDistanceSqr, circularFadeDistanceSqr);
                    mat.SetFloat(TGSConst.CircularFadeFallOff, circularFadeFallOff);
                }
            } else {
                cellsThinMat.DisableKeyword(SKW_CIRCULAR_FADE);
                cellsGeoMat.DisableKeyword(SKW_CIRCULAR_FADE);
                territoriesThinMat.DisableKeyword(SKW_CIRCULAR_FADE);
                territoriesGeoMat.DisableKeyword(SKW_CIRCULAR_FADE);
                territoriesDisputedThinMat.DisableKeyword(SKW_CIRCULAR_FADE);
                territoriesDisputedGeoMat.DisableKeyword(SKW_CIRCULAR_FADE);
                foreach (Material mat in frontierColorCache.Values) {
                    mat.DisableKeyword(SKW_CIRCULAR_FADE);
                }
            }
        }
        void UpdateHighlightEffect() {
            if (highlightKeywords == null) {
                highlightKeywords = new List<string>();
            } else {
                highlightKeywords.Clear();
            }
            switch (_highlightEffect) {
                case HIGHLIGHT_EFFECT.TextureAdditive:
                    highlightKeywords.Add(SKW_TEX_HIGHLIGHT_ADDITIVE);
                    break;
                case HIGHLIGHT_EFFECT.TextureMultiply:
                    highlightKeywords.Add(SKW_TEX_HIGHLIGHT_MULTIPLY);
                    break;
                case HIGHLIGHT_EFFECT.TextureColor:
                    highlightKeywords.Add(SKW_TEX_HIGHLIGHT_COLOR);
                    break;
                case HIGHLIGHT_EFFECT.TextureScale:
                    highlightKeywords.Add(SKW_TEX_HIGHLIGHT_SCALE);
                    break;
                case HIGHLIGHT_EFFECT.DualColors:
                    highlightKeywords.Add(SKW_TEX_DUAL_COLORS);
                    break;
                case HIGHLIGHT_EFFECT.None:
                    HideCellHighlightObject();
                    break;
            }
            if (_transparentBackground) {
                highlightKeywords.Add(SKW_TRANSPARENT);
            }
            string[] keywords = highlightKeywords.ToArray();
            hudMatCellGround.shaderKeywords = keywords;
            hudMatCellOverlay.shaderKeywords = keywords;
            hudMatTerritoryGround.shaderKeywords = keywords;
            hudMatTerritoryOverlay.shaderKeywords = keywords;
            SetBlendHighlight(hudMatCellGround);
            SetBlendHighlight(hudMatCellOverlay);
            SetBlendHighlight(hudMatTerritoryGround);
            SetBlendHighlight(hudMatTerritoryOverlay);
        }
        #endregion

        #region Drawing stuff

        int GetCacheIndexForTerritoryRegion(int territoryIndex, int regionIndex) {
            return territoryIndex * 10000 + regionIndex;
        }

        Material hudMatTerritory { get { return _overlayMode == OVERLAY_MODE.Overlay ? hudMatTerritoryOverlay : hudMatTerritoryGround; } }

        Material hudMatCell { get { return _overlayMode == OVERLAY_MODE.Overlay ? hudMatCellOverlay : hudMatCellGround; } }

        int GetMaterialHash(Color color, Texture2D texture) {
            int hash = 17;
            hash = hash * 23 + color.GetHashCode();
            if (texture != null) {
                hash = hash * 23 + texture.GetHashCode();
            }
            return hash;
        }

        int mats = 0;
        Material GetColoredTexturedMaterialForCell(Color color, Texture2D texture, bool overlay) {
            Dictionary<int, Material> matCache = overlay ? coloredMatCacheOverlayCell : coloredMatCacheGroundCell;

            int materialHash = GetMaterialHash(color, texture);

            Material mat;
            if (matCache.TryGetValue(materialHash, out mat)) {
                mat.color = color;
                return mat;
            } else {
                Debug.Log("Mat count = " + ++mats);
                Material customMat;
                if (texture != null) {
                    mat = overlay ? texturizedMatOverlayCell : texturizedMatGroundCell;
                    customMat = Instantiate(mat);
                    customMat.mainTexture = texture;
                } else {
                    mat = overlay ? coloredMatOverlayCell : coloredMatGroundCell;
                    customMat = Instantiate(mat);
                }
                customMat.name = mat.name;
                customMat.color = color;
                customMat.SetFloat(TGSConst.Offset, _gridSurfaceDepthOffset);
                matCache[materialHash] = customMat;
                SetBlend(customMat);
                SetStencil(customMat, STENCIL_MASK_CELL);
                disposalManager.MarkForDisposal(customMat);
                return customMat;
            }
        }


        Material GetColoredTexturedMaterialForTerritory(Region region, Color color, Texture2D texture, bool overlay) {

            Material mat;
            if (texture != null) {
                mat = overlay ? texturizedMatOverlayTerritory : texturizedMatGroundTerritory;
            } else {
                mat = overlay ? coloredMatOverlayTerritory : coloredMatGroundTerritory;
            }
            Material customMat = region.cachedMat;
            if (customMat == null || customMat.name != mat.name) {
                customMat = Instantiate(mat);
                customMat.name = mat.name;
                region.cachedMat = customMat;
            }
            customMat.color = color;
            customMat.mainTexture = texture;
            customMat.SetFloat(TGSConst.Offset, _gridSurfaceDepthOffsetTerritory);
            customMat.renderQueue += region.sortIndex;
            SetBlend(customMat);
            SetStencil(customMat, STENCIL_MASK_TERRITORY);
            disposalManager.MarkForDisposal(customMat);
            return customMat;

        }


        Material GetFrontierColorMaterial(Color color) {
            if (color == territoriesMat.color)
                return territoriesMat;

            Material mat;
            if (frontierColorCache.TryGetValue(color, out mat)) {
                return mat;
            } else {
                Material customMat = Instantiate(territoriesMat);
                customMat.name = territoriesMat.name;
                customMat.color = color;
                disposalManager.MarkForDisposal(customMat);
                frontierColorCache[color] = customMat;
                return customMat;
            }
        }

        void ApplyMaterialToSurface(Region region, Material sharedMaterial) {
            if (region.renderer != null) {
                region.renderer.sharedMaterial = sharedMaterial;
                if (region.childrenSurfaces != null) {
                    int count = region.childrenSurfaces.Count;
                    for (int k = 0; k < count; k++) {
                        Renderer r = region.childrenSurfaces[k];
                        if (r != null) {
                            r.sharedMaterial = sharedMaterial;
                        }
                    }
                }
            }
        }

        void DrawColorizedTerritories() {
            if (territories == null)
                return;
            int territoriesCount = territories.Count;
            for (int k = 0; k < territoriesCount; k++) {
                Territory territory = territories[k];
                if (issueRedraw == RedrawType.IncrementalTerritories && !territory.isDirty) continue;
                for (int r = 0; r < territory.regions.Count; r++) {
                    Region region = territory.regions[r];
                    if (region.customMaterial != null) {
                        TerritoryToggleRegionSurface(k, true, region.customMaterial.color, false, (Texture2D)region.customMaterial.mainTexture, region.customTextureScale, region.customTextureOffset, region.customTextureRotation, region.customRotateInLocalSpace, regionIndex: r);
                    } else {
                        Color fillColor = territories[k].fillColor;
                        fillColor.a *= colorizedTerritoriesAlpha;
                        TerritoryToggleRegionSurface(k, true, fillColor, regionIndex: r);
                    }
                }
            }
        }
        void SetScaleByCellSize() {
            if (_gridTopology == GRID_TOPOLOGY.Hexagonal) {
                _gridScale.x = _cellSize.x * (1f + (_cellColumnCount - 1f) * 0.75f) / transform.lossyScale.x;
                _gridScale.y = _cellSize.y * (_cellRowCount + 0.5f) / transform.lossyScale.y;
            } else if (_gridTopology == GRID_TOPOLOGY.Box) {
                _gridScale.x = _cellSize.x * _cellColumnCount / transform.lossyScale.x;
                _gridScale.y = _cellSize.y * _cellRowCount / transform.lossyScale.y;
            }
        }

        void ComputeGridScale() {
            if (_regularHexagons && _gridTopology == GRID_TOPOLOGY.Hexagonal) {
                _gridScale = new Vector2(1f + (_cellColumnCount - 1f) * 0.75f, (_cellRowCount + 0.5f) * 0.8660254f); // cos(60), sqrt(3)/2
                float hexScale = Mathf.Max(0.00001f, _regularHexagonsWidth / transform.lossyScale.x);
                _gridScale.x *= hexScale;
                _gridScale.y *= hexScale;
                float aspectRatio = Mathf.Clamp(transform.lossyScale.x / transform.lossyScale.y, 0.01f, 10f);
                _gridScale.x = Mathf.Max(_gridScale.x, 0.0001f);
                _gridScale.y *= aspectRatio;
            }
            _gridScale.x = Mathf.Max(_gridScale.x, 0.0001f);
            _gridScale.y = Mathf.Max(_gridScale.y, 0.0001f);
            _cellSize.x = transform.lossyScale.x * _gridScale.x / _cellColumnCount;
            _cellSize.y = transform.lossyScale.y * _gridScale.y / _cellRowCount;
        }
        void CheckCells() {
            if (!_showCells && !_showTerritories && !_colorizeTerritories && _highlightMode == HIGHLIGHT_MODE.None)
                return;
            if (cells == null || recreateCells) {
                CreateCells();
                refreshCellMesh = true;
            }
            if (refreshCellMesh) {
                DestroyCellLayer();
                GenerateCellsMesh(cells);
                refreshCellMesh = false;
                refreshTerritoriesMesh = true;
            }
        }

        void DestroyCellLayer() {
            if (cellLayer != null) {
                DestroyImmediate(cellLayer);
            } else {
                Transform t = transform.Find(CELLS_LAYER_NAME);
                if (t != null)
                    DestroyImmediate(t.gameObject);
            }
        }

        bool isUsingVertexDispForVertexThickness { get { return _cellBorderThickness > 1 && !canUseGeometryShaders; } }
        bool isUsingVertexDispForTerritoryThickness { get { return _territoryFrontiersThickness > 1 && !canUseGeometryShaders; } }

        void DrawCellBorders() {

            DestroyCellLayer();
            if (cells.Count == 0)
                return;

            cellLayer = new GameObject(CELLS_LAYER_NAME);
            disposalManager.MarkForDisposal(cellLayer);
            cellLayer.transform.SetParent(transform, false);
            cellLayer.transform.localPosition = Vector3.back * 0.001f * _gridMinElevationMultiplier;
            cellLayer.layer = gameObject.layer;

            for (int k = 0; k < cellMeshBorders.Length; k++) {
                GameObject flayer = new GameObject("flayer");
                flayer.layer = gameObject.layer;
                disposalManager.MarkForDisposal(flayer);
                flayer.hideFlags |= HideFlags.HideInHierarchy;
                flayer.transform.SetParent(cellLayer.transform, false);
                flayer.transform.localPosition = TGSConst.Vector3zero;
                flayer.transform.localRotation = Quaternion.Euler(TGSConst.Vector3zero);

                Mesh mesh = new Mesh();
                if (isUsingVertexDispForVertexThickness) {
                    ComputeExplodedVertices(mesh, cellMeshBorders[k]);
                } else {
                    mesh.vertices = cellMeshBorders[k];
                    mesh.SetIndices(cellMeshIndices[k], MeshTopology.Lines, 0);
                }
                disposalManager.MarkForDisposal(mesh);

                MeshFilter mf = flayer.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                _lastVertexCount += mesh.vertexCount;

                MeshRenderer mr = flayer.AddComponent<MeshRenderer>();
                mr.sortingOrder = _sortingOrder;
                mr.receiveShadows = false;
                mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.sharedMaterial = cellsMat;
            }
            cellLayer.SetActive(_showCells);
            float thick = (_cellBorderThickness - 0.8f) * transform.lossyScale.x / 500f;
            cellsGeoMat.SetFloat(TGSConst.Thickness, thick);
        }

        void ComputeExplodedVertices(Mesh mesh, Vector3[] vertices) {
            tempPoints.Clear();
            tempUVs.Clear();
            tempIndices.Clear();
            Vector4 uv;
            for (int k = 0; k < vertices.Length; k += 2) {
                // Triangle points
                tempPoints.Add(vertices[k]);
                tempPoints.Add(vertices[k]);
                tempPoints.Add(vertices[k + 1]);
                tempPoints.Add(vertices[k + 1]);
                uv = vertices[k + 1]; uv.w = -1; tempUVs.Add(uv);
                uv = vertices[k + 1]; uv.w = 1; tempUVs.Add(uv);
                uv = vertices[k]; uv.w = 1; tempUVs.Add(uv);
                uv = vertices[k]; uv.w = -1; tempUVs.Add(uv);
                // First triangle
                tempIndices.Add(k * 2);
                tempIndices.Add(k * 2 + 1);
                tempIndices.Add(k * 2 + 2);
                // Second triangle
                tempIndices.Add(k * 2 + 1);
                tempIndices.Add(k * 2 + 3);
                tempIndices.Add(k * 2 + 2);
            }
            mesh.SetVertices(tempPoints);
            mesh.SetUVs(0, tempUVs);
            mesh.SetTriangles(tempIndices, 0);
        }

        void DrawColorizedCells() {
            if (cells == null)
                return;
            int cellsCount = cells.Count;
            for (int k = 0; k < cellsCount; k++) {
                Cell cell = cells[k];
                if (cell == null) continue;
                Region region = cell.region;
                if (region.customMaterial != null && cell.visible) {
                    CellToggleRegionSurface(k, true, region.customMaterial.color, false, (Texture2D)region.customMaterial.mainTexture, region.customTextureScale, region.customTextureOffset, region.customTextureRotation, region.customRotateInLocalSpace);
                }
            }
        }

        void CheckTerritories() {
            if (!territoriesAreUsed)
                return;
            if (territories == null || recreateTerritories) {
                CreateTerritories();
                refreshTerritoriesMesh = true;
            } else if (needUpdateTerritories) {
                FindTerritoryFrontiers();
                UpdateTerritoryBoundaries();
                needUpdateTerritories = false;
                refreshTerritoriesMesh = true;
            }

            if (refreshTerritoriesMesh) {
                DestroyTerritoryLayer();
                GenerateTerritoriesMesh();
                refreshTerritoriesMesh = false;
            }

        }

        void DestroyTerritoryLayer() {
            if (territoryLayer != null) {
                DestroyImmediate(territoryLayer);
            } else {
                Transform t = transform.Find(TERRITORIES_LAYER_NAME);
                if (t != null)
                    DestroyImmediate(t.gameObject);
            }
        }

        void DrawTerritoryFrontiers() {
            DestroyTerritoryLayer();
            if (territories.Count == 0)
                return;

            territoryLayer = new GameObject(TERRITORIES_LAYER_NAME);
            disposalManager.MarkForDisposal(territoryLayer);
            territoryLayer.transform.SetParent(transform, false);
            territoryLayer.transform.localPosition = Vector3.back * 0.001f * _gridMinElevationMultiplier;
            territoryLayer.layer = gameObject.layer;

            for (int t = 0; t < territoryMeshes.Count; t++) {
                TerritoryMesh tm = territoryMeshes[t];

                Material mat;
                if (tm.territoryIndex < 0) {
                    mat = territoriesDisputedMat;
                } else {
                    Color frontierColor = territories[tm.territoryIndex].frontierColor;
                    if (frontierColor.a == 0 && frontierColor.r == 0 && frontierColor.g == 0 && frontierColor.b == 0) {
                        mat = territoriesMat;
                    } else {
                        mat = GetFrontierColorMaterial(frontierColor);
                    }
                }
                DrawTerritoryFrontier(tm, mat, territoryLayer.transform);
            }

            territoryLayer.SetActive(_showTerritories);
            float thick = (_territoryFrontiersThickness - 0.8f) * transform.lossyScale.x / 500f;
            territoriesGeoMat.SetFloat(TGSConst.Thickness, thick);
            territoriesDisputedGeoMat.SetFloat(TGSConst.Thickness, thick);
        }

        GameObject DrawTerritoryFrontier(TerritoryMesh tm, Material mat, Transform parent) {
            GameObject root = new GameObject("Territory Frontier");
            root.layer = gameObject.layer;
            disposalManager.MarkForDisposal(root);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(0, 0, -0.001f * _gridMinElevationMultiplier);
            root.transform.localRotation = Quaternion.Euler(TGSConst.Vector3zero);

            for (int k = 0; k < tm.territoryMeshBorders.Length; k++) {
                GameObject flayer = new GameObject("flayer");
                flayer.layer = gameObject.layer;
                disposalManager.MarkForDisposal(flayer);
                flayer.transform.SetParent(root.transform, false);
                flayer.transform.localPosition = TGSConst.Vector3zero;
                flayer.transform.localRotation = Quaternion.Euler(TGSConst.Vector3zero);

                Mesh mesh = new Mesh();
                if (isUsingVertexDispForTerritoryThickness) {
                    ComputeExplodedVertices(mesh, tm.territoryMeshBorders[k]);
                } else {
                    mesh.vertices = tm.territoryMeshBorders[k];
                    mesh.SetIndices(tm.territoryMeshIndices[k], MeshTopology.Lines, 0);
                }

                disposalManager.MarkForDisposal(mesh);

                MeshFilter mf = flayer.AddComponent<MeshFilter>();
                mf.sharedMesh = mesh;
                _lastVertexCount += mesh.vertexCount;

                MeshRenderer mr = flayer.AddComponent<MeshRenderer>();
                mr.sortingOrder = _sortingOrder;
                mr.receiveShadows = false;
                mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.sharedMaterial = mat;
            }
            return root;
        }

        void PrepareNewSurfaceMesh(int pointCount) {
            if (meshPoints == null) {
                meshPoints = new List<Vector3>(pointCount);
            } else {
                meshPoints.Clear();
            }
            if (triNew == null || triNew.Length != pointCount) {
                triNew = new int[pointCount];
            }
            if (surfaceMeshHit == null)
                surfaceMeshHit = new Dictionary<TriangulationPoint, int>(20000);
            else {
                surfaceMeshHit.Clear();
            }

            triNewIndex = -1;
            newPointsCount = -1;
        }

        void AddPointToSurfaceMeshWithNormalOffset(TriangulationPoint p) {
            int tri;
            if (surfaceMeshHit.TryGetValue(p, out tri)) {
                triNew[++triNewIndex] = tri;
            } else {
                Vector3 np;
                np.x = p.Xf - 2;
                np.y = p.Yf - 2;
                np.z = -p.Zf;
                np += transform.InverseTransformVector(_terrainWrapper.GetInterpolatedNormal(np.x + 0.5f, np.y + 0.5f)) * _gridNormalOffset;
                meshPoints.Add(np);
                surfaceMeshHit[p] = ++newPointsCount;
                triNew[++triNewIndex] = newPointsCount;
            }
        }

        void AddPointToSurfaceMeshWithoutNormalOffset(TriangulationPoint p) {
            int tri;
            if (surfaceMeshHit.TryGetValue(p, out tri)) {
                triNew[++triNewIndex] = tri;
            } else {
                Vector3 np;
                np.x = p.Xf - 2;
                np.y = p.Yf - 2;
                np.z = -p.Zf;
                meshPoints.Add(np);
                surfaceMeshHit[p] = ++newPointsCount;
                triNew[++triNewIndex] = newPointsCount;
            }
        }


        void AddPointToSurfaceMeshWithNormalOffsetPrimitive(TriangulationPoint p) {
            Vector3 np;
            np.x = p.Xf - 2;
            np.y = p.Yf - 2;
            np.z = -p.Zf;
            np += transform.InverseTransformVector(_terrainWrapper.GetInterpolatedNormal(np.x + 0.5f, np.y + 0.5f)) * _gridNormalOffset;
            meshPoints.Add(np);
        }

        void AddPointToSurfaceMeshWithoutNormalOffsetPrimitive(TriangulationPoint p) {
            Vector3 np;
            np.x = p.Xf - 2;
            np.y = p.Yf - 2;
            np.z = -p.Zf;
            meshPoints.Add(np);
        }

        double polyShift;
        Poly2Tri.Polygon GetPolygon(Region region, ref PolygonPoint[] polyPoints, out int pointCount) {
            // Calculate region's surface points
            pointCount = 0;
            int numSegments = region.segments.Count;
            if (numSegments == 0)
                return null;

            tempConnector.Clear();
            if (_gridScale.x == 1f && _gridScale.y == 1f && _gridCenter.x == 0 && _gridCenter.y == 0) {
                // non scaling segments
                if (_terrainWrapper == null) {
                    for (int i = 0; i < numSegments; i++) {
                        Segment s = region.segments[i];
                        tempConnector.Add(s);
                    }
                } else {
                    for (int i = 0; i < numSegments; i++) {
                        Segment s = region.segments[i];
                        SurfaceSegmentForSurface(s, tempConnector);
                    }
                }
            } else {
                // scaling segments
                if (_terrainWrapper == null) {
                    for (int i = 0; i < numSegments; i++) {
                        Segment s = region.segments[i];
                        tempConnector.Add(GetScaledSegment(s));
                    }
                } else {
                    for (int i = 0; i < numSegments; i++) {
                        Segment s = region.segments[i];
                        SurfaceSegmentForSurface(GetScaledSegment(s), tempConnector);
                    }
                }
            }
            Polygon surfacedPolygon = tempConnector.ToPolygonFromLargestLineStrip();
            if (surfacedPolygon == null)
                return null;

            List<Point> surfacedPoints = surfacedPolygon.contours[0].points;

            int spCount = surfacedPoints.Count;
            if (polyPoints == null || polyPoints.Length < spCount) {
                polyPoints = new PolygonPoint[spCount];
            }

            bool usesTerrain = _terrainWrapper != null;
            for (int k = 0; k < spCount; k++) {
                double x = surfacedPoints[k].x;
                double y = surfacedPoints[k].y;
                if (!IsTooNearPolygon(x, y, polyPoints, pointCount)) {
                    float h = usesTerrain ? _terrainWrapper.SampleHeight(transform.TransformPoint((float)x, (float)y, 0)) : 0;
                    polyPoints[pointCount++] = new PolygonPoint(x, y, h);
                }
            }

            if (pointCount < 3)
                return null;

            double enlarge = _cellFillPadding / 100f;
            if (enlarge != 0) {
                double cx = region.rect2D.center.x;
                double cy = region.rect2D.center.y;
                for (int k = 0; k < pointCount; k++, polyShift++) {
                    PolygonPoint p = polyPoints[k];
                    // these additions are required to prevent issues with polytri library
                    double x = p.X;
                    double y = p.Y;
                    double dx = x - cx;
                    double dy = y - cy;
                    double len = Math.Sqrt(dx * dx + dy * dy);
                    x += 2 + polyShift / 1000000.0 + dx / len * enlarge;
                    y += 2 + polyShift / 1000000.0 + dy / len * enlarge;
                    polyPoints[k] = new PolygonPoint(x, y, p.Zf);
                }
            } else {
                for (int k = 0; k < pointCount; k++, polyShift++) {
                    PolygonPoint p = polyPoints[k];
                    // these additions are required to prevent issues with polytri library
                    double x = p.X;
                    double y = p.Y;
                    x += 2 + polyShift / 1000000.0;
                    y += 2 + polyShift / 1000000.0;
                    polyPoints[k] = new PolygonPoint(x, y, p.Zf);
                }
            }
            return new Poly2Tri.Polygon(polyPoints, pointCount);
        }

        bool SubstractInsideTerritories(Region region, ref PolygonPoint[] polyPoints, Poly2Tri.Polygon poly) {
            List<Region> negativeRegions = new List<Region>();
            int terrRegionsCount = sortedTerritoriesRegions.Count;
            for (int r = 0; r < terrRegionsCount; r++) {
                Region otherRegion = _sortedTerritoriesRegions[r];
                if (otherRegion != region && otherRegion.points.Count >= 3 && region.ContainsRegion(otherRegion)) {
                    Region clonedRegion = otherRegion.Clone();
                    clonedRegion.Enlarge(0.01f); // try to avoid floating point issues when merging cells or regions adjacents with shared vertices
                    negativeRegions.Add(clonedRegion);
                }
            }
            // Collapse negative regions in big holes
            for (int nr = 0; nr < negativeRegions.Count - 1; nr++) {
                for (int nr2 = nr + 1; nr2 < negativeRegions.Count; nr2++) {
                    if (negativeRegions[nr].Intersects(negativeRegions[nr2])) {
                        Clipper clipper = new Clipper();
                        clipper.AddPath(negativeRegions[nr], PolyType.ptSubject);
                        clipper.AddPath(negativeRegions[nr2], PolyType.ptClip);
                        clipper.Execute(ClipType.ctUnion);
                        negativeRegions.RemoveAt(nr2);
                        nr = -1;
                        break;
                    }
                }
            }

            // Substract holes
            bool hasHoles = false;
            for (int nr = 0; nr < negativeRegions.Count; nr++) {
                List<Vector2> surfacedPoints = negativeRegions[nr].points;
                int spCount = surfacedPoints.Count;
                double midx = 0, midy = 0;
                if (polyPoints == null || polyPoints.Length < spCount) {
                    polyPoints = new PolygonPoint[spCount];
                }

                bool usesTerrain = _terrainWrapper != null;
                int pointCount = 0;
                for (int k = 0; k < spCount; k++) {
                    double x = surfacedPoints[k].x;
                    double y = surfacedPoints[k].y;
                    if (!IsTooNearPolygon(x, y, polyPoints, pointCount)) {
                        float h = usesTerrain ? _terrainWrapper.SampleHeight(transform.TransformPoint((float)x, (float)y, 0)) : 0;
                        polyPoints[pointCount++] = new PolygonPoint(x, y, h);
                        midx += x;
                        midy += y;
                    }
                }

                if (pointCount >= 3) {
                    // reduce
                    midx /= pointCount;
                    midy /= pointCount;
                    for (int k = 0; k < pointCount; k++, polyShift++) {
                        PolygonPoint p = polyPoints[k];
                        double DX = midx - p.X;
                        double DY = midy - p.Y;
                        // these additions are required to prevent issues with polytri library
                        double x = p.X + 2 + polyShift / 1000000.0;
                        double y = p.Y + 2 + polyShift / 1000000.0;
                        polyPoints[k] = new PolygonPoint(x + DX * 0.001, y + DY * 0.001, p.Zf);
                    }
                    // add poly hole
                    Poly2Tri.Polygon polyHole = new Poly2Tri.Polygon(polyPoints, pointCount);
                    poly.AddHole(polyHole);
                    hasHoles = true;
                }
            }

            return hasHoles;
        }

        GameObject GenerateRegionSurface(Region region, int cacheIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace) {

            try {
                // Deletes potential residual surface
                if (region.surfaceGameObject != null) {
                    DestroyImmediate(region.surfaceGameObject);
                }

                int pointCount;
                polyShift = 0;
                Poly2Tri.Polygon poly = GetPolygon(region, ref tempPolyPoints, out pointCount);
                if (poly == null)
                    return null;

                // Support for internal territories
                bool hasHole = false;
                if (_allowTerritoriesInsideTerritories && region.entity is Territory) {
                    hasHole = SubstractInsideTerritories(region, ref tempPolyPoints2, poly);
                }

                if (!hasHole) {
                    if (_gridTopology == GRID_TOPOLOGY.Hexagonal && pointCount == 6) {
                        return GenerateRegionSurfaceSimpleHex(poly, region, cacheIndex, material, textureScale, textureOffset, textureRotation, rotateInLocalSpace);
                    } else if (_gridTopology == GRID_TOPOLOGY.Box && pointCount == 4) {
                        return GenerateRegionSurfaceSimpleQuad(poly, region, cacheIndex, material, textureScale, textureOffset, textureRotation, rotateInLocalSpace);
                    }
                }

                if (_terrainWrapper != null) {
                    if (steinerPoints == null) {
                        steinerPoints = new List<TriangulationPoint>(6000);
                    } else {
                        steinerPoints.Clear();
                    }

                    float stepX = 1.0f / heightMapWidth;
                    float smallStep = 1.0f / heightMapWidth;
                    float y = region.rect2D.yMin + smallStep;
                    float ymax = region.rect2D.yMax - smallStep;
                    if (acumY == null || acumY.Length != terrainRoughnessMapWidth) {
                        acumY = new float[terrainRoughnessMapWidth];
                    } else {
                        Array.Clear(acumY, 0, terrainRoughnessMapWidth);
                    }
                    int steinerPointsCount = 0;
                    while (y < ymax) {
                        int j = (int)((y + 0.5f) * terrainRoughnessMapHeight);
                        if (j >= terrainRoughnessMapHeight)
                            j = terrainRoughnessMapHeight - 1;
                        else if (j < 0)
                            j = 0;
                        int jPos = j * terrainRoughnessMapWidth;
                        float sy = y + 2;
                        float xin, xout;
                        GetFirstAndLastPointInRow(sy, pointCount, out xin, out xout);
                        xin += smallStep;
                        xout -= smallStep;
                        int k0 = -1;
                        for (float x = xin; x < xout; x += stepX) {
                            int k = (int)((x + 0.5f) * terrainRoughnessMapWidth); //)) / TERRAIN_CHUNK_SIZE;
                            if (k >= terrainRoughnessMapWidth)
                                k = terrainRoughnessMapWidth - 1;
                            else if (k < 0)
                                k = 0;
                            if (k0 != k) {
                                k0 = k;
                                stepX = terrainRoughnessMap[jPos + k];
                                if (acumY[k] >= stepX)
                                    acumY[k] = 0;
                                acumY[k] += smallStep;
                            }
                            if (acumY[k] >= stepX) {
                                // Gather precision height
                                float h = _terrainWrapper.SampleHeight(transform.TransformPoint(x, y, 0));
                                float htl = _terrainWrapper.SampleHeight(transform.TransformPoint(x - smallStep, y + smallStep, 0));
                                if (htl > h)
                                    h = htl;
                                float htr = _terrainWrapper.SampleHeight(transform.TransformPoint(x + smallStep, y + smallStep, 0));
                                if (htr > h)
                                    h = htr;
                                float hbr = _terrainWrapper.SampleHeight(transform.TransformPoint(x + smallStep, y - smallStep, 0));
                                if (hbr > h)
                                    h = hbr;
                                float hbl = _terrainWrapper.SampleHeight(transform.TransformPoint(x - smallStep, y - smallStep, 0));
                                if (hbl > h)
                                    h = hbl;
                                steinerPoints.Add(new PolygonPoint(x + 2, sy, h));
                                steinerPointsCount++;
                            }
                        }
                        y += smallStep;
                        if (steinerPointsCount > 80000) {
                            Debug.LogWarning("Terrain Grid System: number of adaptation points exceeded. Try increasing the Roughness or the number of points in polygon.");
                            break;
                        }
                    }
                    poly.AddSteinerPoints(steinerPoints);
                }

                P2T.Triangulate(poly);

                Rect rect = (canvasTexture != null && material != null && material.mainTexture == canvasTexture) ? canvasRect : region.rect2D;

                // Calculate & optimize mesh data
                int triCount = poly.Triangles.Count;
                GameObject parentSurf = null;

                for (int triBase = 0; triBase < triCount; triBase += 65500) {
                    int meshTriCount = 65500;
                    if (triBase + meshTriCount > triCount) {
                        meshTriCount = triCount - triBase;
                    }

                    PrepareNewSurfaceMesh(meshTriCount * 3);

                    if (_gridNormalOffset > 0 && _terrainWrapper != null) {
                        for (int k = 0; k < meshTriCount; k++) {
                            DelaunayTriangle dt = poly.Triangles[k + triBase];
                            AddPointToSurfaceMeshWithNormalOffset(dt.Points[0]);
                            AddPointToSurfaceMeshWithNormalOffset(dt.Points[2]);
                            AddPointToSurfaceMeshWithNormalOffset(dt.Points[1]);
                        }
                    } else {
                        for (int k = 0; k < meshTriCount; k++) {
                            DelaunayTriangle dt = poly.Triangles[k + triBase];
                            AddPointToSurfaceMeshWithoutNormalOffset(dt.Points[0]);
                            AddPointToSurfaceMeshWithoutNormalOffset(dt.Points[2]);
                            AddPointToSurfaceMeshWithoutNormalOffset(dt.Points[1]);
                        }
                    }
                    if (_terrainWrapper != null) {
                        if (_cellsMinimumAltitudeClampVertices && _cellsMinimumAltitude != 0) {
                            float localMinima = -(_cellsMinimumAltitude - _terrainWrapper.bounds.min.y) / _terrainWrapper.transform.lossyScale.y;
                            int meshPointsCount = meshPoints.Count;
                            for (int k = 0; k < meshPointsCount; k++) {
                                Vector3 p = meshPoints[k];
                                if (p.z > localMinima) {
                                    p.z = localMinima;
                                    meshPoints[k] = p;
                                }
                            }
                        }
                        if (_cellsMaximumAltitudeClampVertices && _cellsMaximumAltitude != 0) {
                            float localMaxima = -(_cellsMaximumAltitude - _terrainWrapper.bounds.min.y) / _terrainWrapper.transform.lossyScale.y;
                            int meshPointsCount = meshPoints.Count;
                            for (int k = 0; k < meshPointsCount; k++) {
                                Vector3 p = meshPoints[k];
                                if (p.z < localMaxima) {
                                    p.z = localMaxima;
                                    meshPoints[k] = p;
                                }
                            }
                        }
                    }

                    string surfName;
                    if (triBase == 0) {
                        surfName = cacheIndex.ToString();
                    } else {
                        surfName = "splitMesh";
                    }
                    Renderer surfRenderer = Drawing.CreateSurface(surfName, meshPoints, triNew, material, rect, textureScale, textureOffset, textureRotation, rotateInLocalSpace, _sortingOrder, disposalManager);
                    GameObject surf = surfRenderer.gameObject;
                    _lastVertexCount += meshPoints.Count;
                    if (triBase == 0) {
                        surf.transform.SetParent(surfacesLayer.transform, false);
                        surfaces[cacheIndex] = surf;
                        if (region.childrenSurfaces != null) {
                            region.childrenSurfaces.Clear();
                        }
                        region.renderer = surfRenderer;
                    } else {
                        surf.transform.SetParent(parentSurf.transform, false);
                        if (region.childrenSurfaces == null) {
                            region.childrenSurfaces = new List<Renderer>();
                        }
                        region.childrenSurfaces.Add(surfRenderer);
                    }
                    surf.transform.localPosition = TGSConst.Vector3zero;
                    surf.layer = gameObject.layer;
                    parentSurf = surf;
                }
                return parentSurf;
            } catch {
                return null;
            }
        }


        GameObject GenerateRegionSurfaceSimpleHex(Poly2Tri.Polygon poly, Region region, int cacheIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace) {

            // Calculate & optimize mesh data
            PrepareNewSurfaceMesh(6);
            IList<TriangulationPoint> points = (IList<TriangulationPoint>)poly.Points;
            if (_gridNormalOffset > 0 && _terrainWrapper != null) {
                for (int k = 0; k < 6; k++) {
                    AddPointToSurfaceMeshWithNormalOffsetPrimitive(points[k]);
                }
            } else {
                for (int k = 0; k < 6; k++) {
                    AddPointToSurfaceMeshWithoutNormalOffsetPrimitive(points[k]);
                }
            }
            Rect rect = (canvasTexture != null && material != null && material.mainTexture == canvasTexture) ? canvasRect : region.rect2D;
            Renderer surfRenderer = Drawing.CreateSurface(cacheIndex.ToString(), meshPoints, hexIndices, material, rect, textureScale, textureOffset, textureRotation, rotateInLocalSpace, _sortingOrder, disposalManager);
            GameObject surf = surfRenderer.gameObject;
            _lastVertexCount += 6;
            surf.transform.SetParent(surfacesLayer.transform, false);
            surfaces[cacheIndex] = surf;
            surf.transform.localPosition = TGSConst.Vector3zero;
            surf.layer = gameObject.layer;
            region.renderer = surfRenderer;
            return surf;
        }


        GameObject GenerateRegionSurfaceSimpleQuad(Poly2Tri.Polygon poly, Region region, int cacheIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace) {

            // Calculate & optimize mesh data
            PrepareNewSurfaceMesh(4);
            IList<TriangulationPoint> points = poly.Points;
            if (_gridNormalOffset > 0 && _terrainWrapper != null) {
                for (int k = 0; k < 4; k++) {
                    AddPointToSurfaceMeshWithNormalOffsetPrimitive(points[k]);
                }
            } else {
                for (int k = 0; k < 4; k++) {
                    AddPointToSurfaceMeshWithoutNormalOffsetPrimitive(points[k]);
                }
            }
            Rect rect = (canvasTexture != null && material != null && material.mainTexture == canvasTexture) ? canvasRect : region.rect2D;
            Renderer surfRenderer = Drawing.CreateSurface(cacheIndex.ToString(), meshPoints, quadIndices, material, rect, textureScale, textureOffset, textureRotation, rotateInLocalSpace, _sortingOrder, disposalManager);
            GameObject surf = surfRenderer.gameObject;
            _lastVertexCount += 4;
            surf.transform.SetParent(surfacesLayer.transform, false);
            surfaces[cacheIndex] = surf;
            surf.transform.localPosition = TGSConst.Vector3zero;
            surf.layer = gameObject.layer;
            region.renderer = surfRenderer;
            return surf;
        }



        #endregion

        #region Internal API
        void SetTerrain(GameObject terrain)
        {
            if (terrain == null)
            {
                terrainObject = null;
            }
            else
            {
                terrainObject = terrain;
            }
        }

        int GetFittedSizeForHeightmap(int value) {
            value = (int)Mathf.Log(value, 2);
            value = (int)Mathf.Pow(2, value);
            return value + 1;
        }


        public string GetMapData() {
            return "";
        }

        float goodGridRelaxation {
            get {
                if (_numCells > MAX_CELLS_FOR_RELAXATION) {
                    return 1;
                } else {
                    return _gridRelaxation;
                }
            }
        }

        float goodGridCurvature {
            get {
                if (_numCells > MAX_CELLS_FOR_CURVATURE) {
                    return 0;
                } else {
                    return _gridCurvature;
                }
            }
        }

        /// <summary>
        /// Issues a selection check based on a given ray. Used by editor to manipulate cells from Scene window.
        /// Returns true if ray hits the grid.
        /// </summary>
        public bool CheckRay(Ray ray) {
            useEditorRay = true;
            editorRay = ray;
            return CheckMousePos();
        }


        #endregion

        #region Highlighting
        void ClearLastOver() {
            NotifyExitingEntities();
            _cellLastOver = null;
            _cellLastOverIndex = -1;
            _territoryLastOverIndex = -1;
            _territoryRegionLastOverIndex = -1;
        }

        void NotifyExitingEntities() {
            if (_territoryLastOverIndex >= 0 && OnTerritoryExit != null)
                OnTerritoryExit(this, _territoryLastOverIndex);
            if (_cellLastOverIndex >= 0 && OnCellExit != null)
                OnCellExit(this, _cellLastOverIndex);
        }


        /// <summary>
        /// When using grid on a group of gameobjects, if the parent is not a mesh, it won't hold a collider so this method check collision with the grid plane
        /// </summary>
        /// <param name="localPoint"></param>
        /// <returns></returns>
        bool GetBaseLocalHitFromMousePos(Ray ray, out Vector3 localPoint) {
            localPoint = TGSConst.Vector3zero;

            if (_terrainWrapper == null) return false; // returns if grid is not over custom gameobjects

            // intersection with grid plane
            Plane gridPlane = new Plane(transform.forward, transform.position);
            float enter;
            if (gridPlane.Raycast(ray, out enter)) {
                Vector3 position = ray.origin + ray.direction * enter;
                localPoint = transform.InverseTransformPoint(position);
                return (localPoint.x >= -0.5f && localPoint.x <= 0.5f && localPoint.y >= -0.5f && localPoint.y <= 0.5f);
            }
            return false;
        }

        bool GetLocalHitFromMousePos(out Vector3 localPoint) {

            Ray ray;
            if (useEditorRay && !Application.isPlaying) {
                ray = editorRay;
            } else {
                Vector3 mousePos = Input.mousePosition;
                if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height) {
                    localPoint = TGSConst.Vector3zero;
                    return false;
                }
                ray = cameraMain.ScreenPointToRay(mousePos);
                if (!mouseIsOver && !Application.isMobilePlatform) {
                    return GetBaseLocalHitFromMousePos(ray, out localPoint);
                }
            }
            int hitCount = Physics.RaycastNonAlloc(ray, hits);
            if (hitCount > 0) {
                if (_terrainWrapper != null) {
                    float minDistance = _highlightMinimumTerrainDistance * _highlightMinimumTerrainDistance;
                    for (int k = 0; k < hitCount; k++) {
                        GameObject o = hits[k].collider.gameObject;
                        if (_terrainWrapper.Contains(o)) {
                            if ((hits[k].point - ray.origin).sqrMagnitude > minDistance) {
                                localPoint = transform.InverseTransformPoint(hits[k].point); // this method supports grid rotation on a terrain // _terrainWrapper.GetLocalPoint(o, hits[k].point);
                                return true;
                            }
                        }
                    }
                } else {
                    for (int k = 0; k < hitCount; k++) {
                        if (hits[k].collider.gameObject == gameObject) {
                            localPoint = transform.InverseTransformPoint(hits[k].point);
                            return true;
                        }
                    }
                }
            }
            return GetBaseLocalHitFromMousePos(ray, out localPoint);
        }

        bool GetLocalHitFromWorldPosition(ref Vector3 position) {
            if (_terrainWrapper != null) {
                Ray ray = new Ray(position - transform.forward * 100, transform.forward);
                int hitCount = Physics.RaycastNonAlloc(ray, hits);
                bool goodHit = false;
                if (hitCount > 0) {
                    for (int k = 0; k < hitCount; k++) {
                        if (hits[k].collider.gameObject == _terrainWrapper.gameObject) {
                            position = transform.InverseTransformPoint(hits[k].point);  // this method supports grid rotation on a terrain // _terrainWrapper.GetLocalPoint(hits[k].collider.gameObject, hits[k].point);
                            goodHit = true;
                            break;
                        }
                    }
                }
                if (!goodHit) {
                    // defaults to base grid
                    position = transform.InverseTransformPoint(position);
                    return (position.x >= -0.5f && position.x <= 0.5f && position.y >= -0.5f && position.y <= 0.5f);
                }
            } else {
                position = transform.InverseTransformPoint(position);
            }
            return true;
        }

        bool CheckMousePos() {
            HIGHLIGHT_MODE currentHighlightMode = _highlightMode;
#if UNITY_EDITOR
            if (!Application.isPlaying && useEditorRay) {
                currentHighlightMode = HIGHLIGHT_MODE.Cells;
            }
#endif

            if (currentHighlightMode == HIGHLIGHT_MODE.None)
                return false;

            bool goodHit = GetLocalHitFromMousePos(out localCursorPos);
            if (!goodHit) {
                NotifyExitingEntities();
                HideTerritoryRegionHighlight();
                ClearLastOver();
                return false;
            }

            // verify if last highlighted territory remains active
            bool sameTerritoryRegionHighlight = false;
            float sameTerritoryRegionArea = float.MaxValue;
            if (_territoryRegionLastOver != null) {
                if (_territoryLastOver.visible && _territoryRegionLastOver.Contains(localCursorPos.x, localCursorPos.y)) {
                    sameTerritoryRegionHighlight = true;
                    sameTerritoryRegionArea = _territoryRegionLastOver.rect2DArea;
                }
            }
            int newTerritoryHighlightedIndex = -1;
            int newTerritoryRegionHighlightedIndex = -1;

            // mouse if over the grid - verify if hitPos is inside any territory polygon
            if (territories != null) {
                int sortedTerrRegionsCount = sortedTerritoriesRegions.Count;
                for (int r = 0; r < sortedTerrRegionsCount; r++) {
                    Region sreg = _sortedTerritoriesRegions[r];
                    Territory sterr = (Territory)sreg.entity;
                    if (sreg != null && sterr.visible && sterr.cells != null && sterr.cells.Count > 0) {
                        if (sreg.Contains(localCursorPos.x, localCursorPos.y)) {
                            newTerritoryHighlightedIndex = TerritoryGetIndex(sterr);
                            newTerritoryRegionHighlightedIndex = sterr.regions.IndexOf(sreg);
                            sameTerritoryRegionHighlight = newTerritoryHighlightedIndex == _territoryLastOverIndex && newTerritoryRegionHighlightedIndex == _territoryRegionLastOverIndex;
                            break;
                        }
                        if (sreg.rect2DArea > sameTerritoryRegionArea) {
                            break;
                        }
                    }
                }
            }

            if (!sameTerritoryRegionHighlight) {
                if (_territoryLastOverIndex >= 0 && OnTerritoryExit != null) {
                    OnTerritoryExit(this, _territoryLastOverIndex);
                }
                if (_territoryLastOverIndex >= 0 && _territoryRegionLastOverIndex >= 0 && OnTerritoryRegionExit != null) {
                    OnTerritoryRegionExit(this, _territoryLastOverIndex, _territoryRegionLastOverIndex);
                }
                if (newTerritoryHighlightedIndex >= 0 && OnTerritoryEnter != null) {
                    OnTerritoryEnter(this, newTerritoryHighlightedIndex);
                }
                if (newTerritoryHighlightedIndex >= 0 && newTerritoryRegionHighlightedIndex >= 0 && OnTerritoryRegionEnter != null) {
                    OnTerritoryRegionEnter(this, newTerritoryHighlightedIndex, newTerritoryRegionHighlightedIndex);
                }
                _territoryLastOverIndex = newTerritoryHighlightedIndex;
                _territoryRegionLastOverIndex = newTerritoryRegionHighlightedIndex;
            }

            // verify if last highlited cell remains active
            bool sameCellHighlight = false;
            if (_cellLastOver != null) {
                if (_cellLastOver.region.Contains(localCursorPos.x, localCursorPos.y)) {
                    sameCellHighlight = true;
                }
            }
            int newCellHighlightedIndex = -1;

            if (!sameCellHighlight) {
                if (currentHighlightMode == HIGHLIGHT_MODE.Cells) {
                    Cell newCellHighlighted = GetCellAtPoint(localCursorPos, false, _territoryLastOverIndex);
                    if (newCellHighlighted != null) {
                        newCellHighlightedIndex = CellGetIndex(newCellHighlighted);
                    }
                }
            }

            if (!sameCellHighlight) {
                if (_cellLastOverIndex >= 0 && OnCellExit != null)
                    OnCellExit(this, _cellLastOverIndex);
                if (newCellHighlightedIndex >= 0 && OnCellEnter != null)
                    OnCellEnter(this, newCellHighlightedIndex);
                _cellLastOverIndex = newCellHighlightedIndex;
                if (newCellHighlightedIndex >= 0)
                    _cellLastOver = cells[newCellHighlightedIndex];
                else
                    _cellLastOver = null;
            }

            if (currentHighlightMode == HIGHLIGHT_MODE.Cells) {
                if (!sameCellHighlight) {
                    if (newCellHighlightedIndex >= 0 && (cells[newCellHighlightedIndex].visible || _cellHighlightNonVisible)) {
                        HighlightCellRegion(newCellHighlightedIndex, false);
                    } else {
                        HideCellRegionHighlight();
                    }
                }
            } else if (currentHighlightMode == HIGHLIGHT_MODE.Territories) {
                if (!sameTerritoryRegionHighlight) {
                    if (newTerritoryHighlightedIndex >= 0 && territories[newTerritoryHighlightedIndex].visible && newTerritoryRegionHighlightedIndex >= 0) {
                        HighlightTerritoryRegion(newTerritoryHighlightedIndex, false, newTerritoryRegionHighlightedIndex);
                    } else {
                        HideTerritoryRegionHighlight();
                    }
                }
            }

            return true;
        }

        void UpdateHighlightFade() {
            if (_highlightFadeAmount == 0)
                return;

            if (_highlightedObj != null) {
                float newAlpha = _highlightFadeMin + Mathf.PingPong(Time.time * _highlightFadeSpeed - highlightFadeStart, _highlightFadeAmount - _highlightFadeMin);
                Material mat = highlightMode == HIGHLIGHT_MODE.Territories ? hudMatTerritory : hudMatCell;
                mat.SetFloat(TGSConst.FadeAmount, newAlpha);
                float newScale = _highlightScaleMin + Mathf.PingPong(Time.time * highlightFadeSpeed, _highlightScaleMax - _highlightScaleMin);
                mat.SetFloat(TGSConst.Scale, 1f / (newScale + 0.0001f));
                _highlightedObj.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }

        void TriggerEvents() {
            int buttonIndex = -1;
            bool leftButtonClick = Input.GetMouseButtonDown(0);
            bool rightButtonClick = Input.GetMouseButtonDown(1);
            bool leftButtonUp = Input.GetMouseButtonUp(0);
            bool rightButtonUp = Input.GetMouseButtonUp(1);

            if (leftButtonUp || leftButtonClick) {
                buttonIndex = 0;
            } else if (rightButtonUp || rightButtonClick) {
                buttonIndex = 1;
            }
            if (leftButtonClick || rightButtonClick) {
                // record last clicked cell/territory
                _cellLastClickedIndex = _cellLastOverIndex;
                _territoryLastClickedIndex = _territoryLastOverIndex;
                _territoryRegionLastClickedIndex = _territoryRegionLastOverIndex;
                localCursorClickedPos = localCursorPos;

                if (_territoryLastOverIndex >= 0 && OnTerritoryMouseDown != null) {
                    OnTerritoryMouseDown(this, _territoryLastOverIndex, buttonIndex);
                }
                if (_territoryLastOverIndex >= 0 && _territoryRegionLastOverIndex >= 0 && OnTerritoryRegionMouseDown != null) {
                    OnTerritoryRegionMouseDown(this, _territoryLastOverIndex, _territoryRegionLastOverIndex, buttonIndex);
                }
                if (_cellLastOverIndex >= 0 && OnCellMouseDown != null) {
                    OnCellMouseDown(this, _cellLastOverIndex, buttonIndex);
                }
            } else if (leftButtonUp || rightButtonUp) {
                if (_territoryLastOverIndex >= 0) {
                    if (_territoryLastOverIndex == _territoryLastClickedIndex && OnTerritoryClick != null) {
                        OnTerritoryClick(this, _territoryLastOverIndex, buttonIndex);
                    }
                    if (_territoryLastOverIndex == _territoryLastClickedIndex && _territoryRegionLastOverIndex == _territoryRegionLastClickedIndex && OnTerritoryRegionClick != null) {
                        OnTerritoryRegionClick(this, _territoryLastOverIndex, _territoryRegionLastOverIndex, buttonIndex);
                    }
                    if (OnTerritoryMouseUp != null) {
                        OnTerritoryMouseUp(this, _territoryLastOverIndex, buttonIndex);
                    }
                    if (OnTerritoryRegionMouseUp != null) {
                        OnTerritoryRegionMouseUp(this, _territoryLastOverIndex, _territoryRegionLastOverIndex, buttonIndex);
                    }
                }
                if (_cellLastOverIndex >= 0) {
                    if (_cellLastOverIndex == _cellLastClickedIndex && OnCellClick != null) {
                        OnCellClick(this, _cellLastOverIndex, buttonIndex);
                    }
                    if (OnCellMouseUp != null) {
                        OnCellMouseUp(this, _cellLastOverIndex, buttonIndex);
                    }
                }
                if (dragging) {
                    if (OnCellDragEnd != null) OnCellDragEnd(this, _cellLastClickedIndex, _cellLastOverIndex);
                    dragging = false;
                }
            } else {
                bool leftButtonDown = Input.GetMouseButton(0);
                if (leftButtonDown && localCursorClickedPos != localCursorPos) {
                    if (dragging) {
                        if (OnCellDrag != null) {
                            OnCellDrag(this, _cellLastClickedIndex, _cellLastOverIndex);
                        }
                    } else {
                        dragging = true;
                        if (OnCellDragStart != null) {
                            OnCellDragStart(this, _cellLastClickedIndex);
                        }
                    }

                }
            }
        }

        #endregion

        #region Geometric functions

        Vector3 GetWorldSpacePosition(Vector2 localPosition) {
            if (_terrainWrapper != null) {
                Vector3 wPos = transform.TransformPoint(localPosition);
                wPos.y += _terrainWrapper.SampleHeight(wPos) * _terrainWrapper.transform.lossyScale.y;
                return wPos;
            } else {
                return transform.TransformPoint(localPosition);
            }
        }

        Vector3 GetScaledVector(Vector3 p) {
            p.x *= _gridScale.x;
            p.x += _gridCenter.x;
            p.y *= _gridScale.y;
            p.y += _gridCenter.y;
            return p;
        }

        Vector3 GetScaledVector(Point q) {
            Vector3 p;
            p.x = (float)q.x * _gridScale.x;
            p.x += _gridCenter.x;
            p.y = (float)q.y * _gridScale.y;
            p.y += _gridCenter.y;
            p.z = 0;
            return p;
        }


        void GetUnscaledVector(ref Vector2 p) {
            p.x -= _gridCenter.x;
            p.x /= _gridScale.x;
            p.y -= _gridCenter.y;
            p.y /= _gridScale.y;
        }

        Point GetScaledPoint(Point p) {
            p.x *= _gridScale.x;
            p.x += _gridCenter.x;
            p.y *= _gridScale.y;
            p.y += _gridCenter.y;
            return p;
        }

        Segment GetScaledSegment(Segment s) {
            Segment ss = new Segment(s.start, s.end, s.border);
            ss.start = GetScaledPoint(ss.start);
            ss.end = GetScaledPoint(ss.end);
            return ss;
        }


        #endregion

        #region Territory stuff

        void HideTerritoryRegionHighlight() {
            HideCellRegionHighlight();
            if (_territoryRegionHighlighted == null)
                return;
            if (_highlightedObj != null) {
                if (_territoryRegionHighlighted.customMaterial != null) {
                    ApplyMaterialToSurface(_territoryRegionHighlighted, _territoryRegionHighlighted.customMaterial);
                } else if (_highlightedObj.GetComponent<SurfaceFader>() == null) {
                    _highlightedObj.SetActive(false);
                }
                if (!_territoryHighlighted.visible) {
                    _highlightedObj.SetActive(false);
                }
                _highlightedObj = null;
            }
            _territoryRegionHighlighted = null;
            _territoryHighlightedIndex = -1;
            _territoryHighlightedRegionIndex = -1;
        }

        /// <summary>
        /// Highlights the territory region specified. Returns the generated highlight surface gameObject.
        /// Internally used by the Map UI and the Editor component, but you can use it as well to temporarily mark a territory region.
        /// </summary>
        /// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
        GameObject HighlightTerritoryRegion(int territoryIndex, bool refreshGeometry, int regionIndex = 0) {
            if (_territoryHighlighted != null)
                HideTerritoryRegionHighlight();
            if (!ValidTerritoryIndex(territoryIndex, regionIndex))
                return null;

            if (_highlightEffect != HIGHLIGHT_EFFECT.None) {
                if (OnTerritoryHighlight != null) {
                    bool cancelHighlight = false;
                    OnTerritoryHighlight(this, territoryIndex, ref cancelHighlight);
                    if (cancelHighlight)
                        return null;
                }

                int cacheIndex = GetCacheIndexForTerritoryRegion(territoryIndex, regionIndex);
                bool existsInCache = surfaces.ContainsKey(cacheIndex);
                if (refreshGeometry && existsInCache) {
                    GameObject obj = surfaces[cacheIndex];
                    surfaces.Remove(cacheIndex);
                    DestroyImmediate(obj);
                    existsInCache = false;
                }
                if (existsInCache) {
                    _highlightedObj = surfaces[cacheIndex];
                    if (_highlightedObj == null) {
                        surfaces.Remove(cacheIndex);
                    } else {
                        if (!_highlightedObj.activeSelf)
                            _highlightedObj.SetActive(true);
                        Renderer rr = _highlightedObj.GetComponent<Renderer>();
                        if (rr.sharedMaterial != hudMatTerritory)
                            rr.sharedMaterial = hudMatTerritory;
                    }
                } else {
                    _highlightedObj = GenerateTerritoryRegionSurface(territoryIndex, hudMatTerritory, TGSConst.Vector2one, TGSConst.Vector2zero, 0, false, regionIndex);
                }
                // Reuse territory texture
                Territory territory = territories[territoryIndex];
                if (territory.regions[regionIndex].customMaterial != null) {
                    if (hudMatTerritory.HasProperty(TGSConst.MainTex)) {
                        hudMatTerritory.mainTexture = territory.regions[regionIndex].customMaterial.mainTexture;
                    } else {
                        hudMatTerritory.mainTexture = null;
                    }
                }
                highlightFadeStart = Time.time;
            }

            _territoryHighlightedIndex = territoryIndex;
            _territoryHighlightedRegionIndex = regionIndex;
            _territoryRegionHighlighted = territories[territoryIndex].regions[regionIndex];

            return _highlightedObj;
        }

        GameObject GenerateTerritoryRegionSurface(int territoryIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace, int regionIndex = 0) {
            if (!ValidTerritoryIndex(territoryIndex, regionIndex))
                return null;
            Region region = territories[territoryIndex].regions[regionIndex];
            int cacheIndex = GetCacheIndexForTerritoryRegion(territoryIndex, regionIndex);
            return GenerateRegionSurface(region, cacheIndex, material, textureScale, textureOffset, textureRotation, rotateInLocalSpace);
        }

        void UpdateColorizedTerritoriesAlpha() {
            if (territories == null)
                return;
            int territoriesCount = territories.Count;
            for (int c = 0; c < territoriesCount; c++) {
                Territory territory = territories[c];
                for (int r = 0; r < territory.regions.Count; r++) {
                    int cacheIndex = GetCacheIndexForTerritoryRegion(c, r);
                    GameObject surf;
                    if (surfaces.TryGetValue(cacheIndex, out surf)) {
                        if (surf != null) {
                            Material mat = surf.GetComponent<Renderer>().sharedMaterial;
                            Color newColor = mat.color;
                            newColor.a = territory.fillColor.a * _colorizedTerritoriesAlpha;
                            mat.color = newColor;
                        }
                    }
                }
            }
        }

        Territory GetTerritoryAtPoint(Vector3 position, bool worldSpace) {
            if (territories == null)
                return null;
            if (worldSpace) {
                if (!GetLocalHitFromWorldPosition(ref position))
                    return null;
            }
            int territoriesCount = territories.Count;
            for (int p = 0; p < territoriesCount; p++) {
                Territory territory = territories[p];
                for (int r = 0; r < territory.regions.Count; r++) {
                    if (territory.regions[r].Contains(position.x, position.y)) {
                        return territory;
                    }
                }
            }
            return null;
        }

        void TerritoryAnimate(FADER_STYLE style, int territoryIndex, Color color, float duration, int repetitions, int regionIndex = 0) {
            if (!ValidTerritoryIndex(territoryIndex, regionIndex)) return;

            int cacheIndex = GetCacheIndexForTerritoryRegion(territoryIndex, regionIndex);
            GameObject territorySurface;
            surfaces.TryGetValue(cacheIndex, out territorySurface);
            if (territorySurface == null) {
                territorySurface = TerritoryToggleRegionSurface(territoryIndex, true, color, regionIndex: regionIndex);
                territories[territoryIndex].regions[regionIndex].customMaterial = null;
            } else {
                territorySurface.SetActive(true);
            }
            Renderer renderer = territorySurface.GetComponent<Renderer>();
            Material oldMaterial = renderer.sharedMaterial;
            Material fadeMaterial = Instantiate(hudMatTerritory);
            Region region = territories[territoryIndex].regions[regionIndex];
            fadeMaterial.color = region.customMaterial != null ? region.customMaterial.color : oldMaterial.color;
            if (fadeMaterial.HasProperty(TGSConst.MainTex) && oldMaterial.HasProperty(TGSConst.MainTex)) {
                fadeMaterial.mainTexture = oldMaterial.mainTexture;
            }
            disposalManager.MarkForDisposal(fadeMaterial);
            fadeMaterial.name = oldMaterial.name;
            renderer.sharedMaterial = fadeMaterial;
            SurfaceFader.Animate(style, this, territorySurface, region, fadeMaterial, color, duration, repetitions);
        }


        void TerritoryCancelAnimation(int territoryIndex, float fadeOutDuration, int regionIndex = 0) {
            if (territoryIndex < 0 || territoryIndex >= territories.Count)
                return;
            int cacheIndex = GetCacheIndexForTerritoryRegion(territoryIndex, regionIndex);
            GameObject territorySurface;
            surfaces.TryGetValue(cacheIndex, out territorySurface);
            if (territorySurface == null)
                return;
            territorySurface.GetComponents(tempFaders);
            int count = tempFaders.Count;
            for (int k = 0; k < count; k++) {
                tempFaders[k].Finish(fadeOutDuration);
            }
        }

        bool ValidTerritoryIndex(int territoryIndex) {
            return territories != null && territoryIndex >= 0 && territoryIndex < territories.Count;
        }

        bool ValidTerritoryIndex(int territoryIndex, int regionIndex) {
            return territories != null && territoryIndex >= 0 && territoryIndex < territories.Count && regionIndex >= 0 && regionIndex < territories[territoryIndex].regions.Count;
        }

        #endregion

        #region Cell stuff

        int GetCacheIndexForCellRegion(int cellIndex) {
            return 10000000 + cellIndex; // * 1000 + regionIndex;
        }

        /// <summary>
        /// Highlights the cell region specified. Returns the generated highlight surface gameObject.
        /// Internally used by the Map UI and the Editor component, but you can use it as well to temporarily mark a territory region.
        /// </summary>
        /// <param name="refreshGeometry">Pass true only if you're sure you want to force refresh the geometry of the highlight (for instance, if the frontiers data has changed). If you're unsure, pass false.</param>
        GameObject HighlightCellRegion(int cellIndex, bool refreshGeometry) {
            if (_cellHighlighted != null)
                HideCellRegionHighlight();
            if (cellIndex < 0 || cellIndex >= cells.Count)
                return null;

            if (_highlightEffect != HIGHLIGHT_EFFECT.None) {

                if (OnCellHighlight != null) {
                    bool cancelHighlight = false;
                    OnCellHighlight(this, cellIndex, ref cancelHighlight);
                    if (cancelHighlight)
                        return null;
                }

                int cacheIndex = GetCacheIndexForCellRegion(cellIndex);
                GameObject obj;
                bool existsInCache = surfaces.TryGetValue(cacheIndex, out obj);
                if (refreshGeometry && existsInCache) {
                    surfaces.Remove(cacheIndex);
                    DestroyImmediate(obj);
                    existsInCache = false;
                }
                if (existsInCache) {
                    _highlightedObj = surfaces[cacheIndex];
                    if (_highlightedObj != null) {
                        _highlightedObj.SetActive(true);
                        _highlightedObj.GetComponent<Renderer>().sharedMaterial = hudMatCell;
                    } else {
                        surfaces.Remove(cacheIndex);
                    }
                } else {
                    _highlightedObj = GenerateCellRegionSurface(cellIndex, hudMatCell, TGSConst.Vector2one, TGSConst.Vector2zero, 0, false);
                }
                // Reuse cell texture
                Cell cell = cells[cellIndex];
                if (cell.region.customMaterial != null && cell.region.customMaterial.HasProperty("_MainTex")) {
                    hudMatCell.mainTexture = cell.region.customMaterial.mainTexture;
                } else {
                    hudMatCell.mainTexture = null;
                }
                highlightFadeStart = Time.time;
            }

            _cellHighlighted = cells[cellIndex];
            _cellHighlightedIndex = cellIndex;
            return _highlightedObj;
        }

        /// <summary>
        /// Just hides the highlighted object. It doesn't release the highlighted cell.
        /// </summary>
        void HideCellHighlightObject() {
            if (_cellHighlighted != null) {
                if (_highlightedObj != null) {
                    if (cellHighlighted.region.customMaterial != null) {
                        ApplyMaterialToSurface(cellHighlighted.region, _cellHighlighted.region.customMaterial);
                    } else if (_highlightedObj.GetComponent<SurfaceFader>() == null) {
                        _highlightedObj.SetActive(false);
                    }
                    if (!cellHighlighted.visible) {
                        _highlightedObj.SetActive(false);
                    }
                }
            }
        }

        void HideCellRegionHighlight() {
            if (_cellHighlighted == null)
                return;
            if (_highlightedObj != null) {
                if (cellHighlighted.region.customMaterial != null) {
                    ApplyMaterialToSurface(cellHighlighted.region, _cellHighlighted.region.customMaterial);
                } else if (_highlightedObj.GetComponent<SurfaceFader>() == null) {
                    _highlightedObj.SetActive(false);
                }
                if (!cellHighlighted.visible) {
                    _highlightedObj.SetActive(false);
                }
                _highlightedObj = null;
            }
            _cellHighlighted = null;
            _cellHighlightedIndex = -1;
        }

        void SurfaceSegmentForSurface(Segment s, Connector connector) {

            // trace the line until roughness is exceeded
            double dist = s.magnitude; // (float)Math.Sqrt ( (p1.x-p0.x)*(p1.x-p0.x) + (p1.y-p0.y)*(p1.y-p0.y));
            Point direction = s.end - s.start;
            int numSteps = (int)(dist / MIN_VERTEX_DISTANCE);
            if (numSteps <= 1) {
                connector.Add(s);
                return;
            }

            Point t0 = s.start;
            Vector3 wp_start = transform.TransformPoint(t0.vector3);
            float h0 = _terrainWrapper.SampleHeight(wp_start);
            Point ta = t0;
            float h1;
            Vector3 wp_end = transform.TransformPoint(s.end.vector3);
            Vector3 wp_direction = wp_end - wp_start;
            for (int i = 1; i < numSteps; i++) {
                Point t1 = s.start + direction * ((double)i / numSteps);
                Vector3 v1 = wp_start + wp_direction * ((float)i / numSteps);
                h1 = _terrainWrapper.SampleHeight(v1);
                if (h1 - h0 > maxRoughnessWorldSpace || h0 - h1 > maxRoughnessWorldSpace) {
                    if (t0 != ta) {
                        Segment s0 = new Segment(t0, ta, s.border);
                        connector.Add(s0);
                        Segment s1 = new Segment(ta, t1, s.border);
                        connector.Add(s1);
                    } else {
                        Segment s0 = new Segment(t0, t1, s.border);
                        connector.Add(s0);
                    }
                    t0 = t1;
                    h0 = h1;
                }
                ta = t1;
            }
            // Add last point
            Segment finalSeg = new Segment(t0, s.end, s.border);
            connector.Add(finalSeg);
        }


        void GetFirstAndLastPointInRow(float y, int pointCount, out float first, out float last) {
            float minx = float.MaxValue;
            float maxx = float.MinValue;
            PolygonPoint p0 = tempPolyPoints[0], p1;
            for (int k = 1; k <= pointCount; k++) {
                if (k == pointCount) {
                    p1 = tempPolyPoints[0];
                } else {
                    p1 = tempPolyPoints[k];
                }
                // if line crosses the horizontal line
                if (p0.Yf >= y && p1.Yf <= y || p0.Yf <= y && p1.Yf >= y) {
                    float x;
                    if (p1.Xf == p0.Xf) {
                        x = p0.Xf;
                    } else {
                        float a = (p1.Xf - p0.Xf) / (p1.Yf - p0.Yf);
                        x = p0.Xf + a * (y - p0.Yf);
                    }
                    if (x > maxx)
                        maxx = x;
                    if (x < minx)
                        minx = x;
                }
                p0 = p1;
            }
            first = minx - 2f;
            last = maxx - 2f;
        }

        bool IsTooNearPolygon(double x, double y, PolygonPoint[] tempPolyPoints, int pointsCount) {
            for (int j = pointsCount - 1; j >= 0; j--) {
                PolygonPoint p1 = tempPolyPoints[j];
                double dx = x - p1.X;
                double dy = y - p1.Y;
                if (dx * dx + dy * dy < SQR_MIN_VERTEX_DIST) {
                    return true;
                }
            }
            return false;
        }

        GameObject GenerateCellRegionSurface(int cellIndex, Material material, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace) {
            if (cellIndex < 0 || cellIndex >= cells.Count)
                return null;
            Region region = cells[cellIndex].region;
            int cacheIndex = GetCacheIndexForCellRegion(cellIndex);
            return GenerateRegionSurface(region, cacheIndex, material, textureScale, textureOffset, textureRotation, rotateInLocalSpace);
        }

        Cell GetCellAtPoint(Vector3 position, bool worldSpace, int territoryIndex = -1) {
            // Compute local point
            if (worldSpace) {
                if (!GetLocalHitFromWorldPosition(ref position))
                    return null;
            }

            int cellsCount = cells.Count;
            if ((_gridTopology == GRID_TOPOLOGY.Hexagonal || _gridTopology == GRID_TOPOLOGY.Box) && cellsCount == _cellRowCount * _cellColumnCount) {
                float px = (position.x - _gridCenter.x) / _gridScale.x + 0.5f;
                float py = (position.y - _gridCenter.y) / _gridScale.y + 0.5f;
                int xc = (int)(_cellColumnCount * px);
                int yc = (int)(_cellRowCount * py);
                for (int y = yc - 1; y < yc + 2; y++) {
                    for (int x = xc - 1; x < xc + 2; x++) {
                        int index = y * _cellColumnCount + x;
                        if (index < 0 || index >= cellCount)
                            continue;
                        Cell cell = cells[index];
                        if (cell == null || cell.region == null || cell.region.points == null)
                            continue;
                        if (cell.region.Contains(position.x, position.y)) {
                            return cell;
                        }
                    }
                }
            } else {
                if (territoryIndex >= 0 && (territories == null || territoryIndex >= territories.Count))
                    return null;
                List<Cell> cells = territoryIndex >= 0 ? territories[territoryIndex].cells : sortedCells;
                cellsCount = cells.Count;
                for (int p = 0; p < cellsCount; p++) {
                    Cell cell = cells[p];
                    if (cell == null || cell.region == null || cell.region.points == null)
                        continue;
                    if (cell.region.Contains(position.x, position.y)) {
                        return cell;
                    }
                }
            }
            return null;
        }


        void Encapsulate(ref Rect r, Vector3 position) {
            if (position.x < r.xMin)
                r.xMin = position.x;
            if (position.x > r.xMax)
                r.xMax = position.x;
            if (position.y < r.yMin)
                r.yMin = position.y;
            if (position.y > r.yMax)
                r.yMax = position.y;
        }



        int GetCellInArea(Bounds bounds, List<int> cellIndices, float padding = 0, bool checkAllVertices = false) {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            Rect rect = new Rect(transform.InverseTransformPoint(min), TGSConst.Vector2zero);
            Encapsulate(ref rect, transform.InverseTransformPoint(new Vector3(min.x, min.y, max.z)));
            Encapsulate(ref rect, transform.InverseTransformPoint(new Vector3(max.x, min.y, min.z)));
            Encapsulate(ref rect, transform.InverseTransformPoint(new Vector3(max.x, min.y, max.z)));
            Encapsulate(ref rect, transform.InverseTransformPoint(new Vector3(min.x, max.y, min.z)));
            Encapsulate(ref rect, transform.InverseTransformPoint(new Vector3(min.x, max.y, max.z)));
            Encapsulate(ref rect, transform.InverseTransformPoint(new Vector3(max.x, max.y, min.z)));
            Encapsulate(ref rect, transform.InverseTransformPoint(new Vector3(max.x, max.y, max.z)));
            return GetCellInArea(rect, cellIndices, padding, checkAllVertices);
        }



        int GetCellInArea(Rect rect, List<int> cellIndices, float padding = 0, bool checkAllVertices = false) {
            rect.xMin -= padding;
            rect.xMax += padding;
            rect.yMin -= padding;
            rect.yMax += padding;
            cellIndices.Clear();
            int cellCount = cells.Count;
            if (checkAllVertices) {
                for (int k = 0; k < cellCount; k++) {
                    Cell cell = cells[k];
                    if (cell != null) {
                        if (cell.center.x >= rect.xMin && cell.center.x <= rect.xMax && cell.center.y >= rect.yMin && cell.center.y <= rect.yMax) {
                            cellIndices.Add(k);
                        } else {
                            int vertexCount = cell.region.points.Count;
                            for (int v = 0; v < vertexCount; v++) {
                                Vector3 vertexPos = cell.region.points[v];
                                if (vertexPos.x >= rect.xMin && vertexPos.x <= rect.xMax && vertexPos.y >= rect.yMin && vertexPos.y <= rect.yMax) {
                                    cellIndices.Add(k);
                                    break;
                                }
                            }
                        }
                    }
                }
            } else {
                for (int k = 0; k < cellCount; k++) {
                    Cell cell = cells[k];
                    if (cell != null) {
                        if (cell.center.x >= rect.xMin && cell.center.x <= rect.xMax && cell.center.y >= rect.yMin && cell.center.y <= rect.yMax) {
                            cellIndices.Add(k);
                        }
                    }
                }
            }
            return cellIndices.Count;
        }


        void CellAnimate(FADER_STYLE style, int cellIndex, Color initialColor, Color color, float duration, int repetitions) {
            if (cellIndex < 0 || cellIndex >= cells.Count)
                return;
            int cacheIndex = GetCacheIndexForCellRegion(cellIndex);
            GameObject cellSurface;
            surfaces.TryGetValue(cacheIndex, out cellSurface);
            if (cellSurface == null) {
                cellSurface = CellToggleRegionSurface(cellIndex, true, color, false);
                cells[cellIndex].region.customMaterial = null;
            } else {
                cellSurface.SetActive(true);
            }
            if (cellSurface == null)
                return;
            Renderer renderer = cellSurface.GetComponent<Renderer>();
            if (renderer == null)
                return;
            Material oldMaterial = renderer.sharedMaterial;
            Material fadeMaterial = Instantiate(hudMatCell);
            fadeMaterial.SetFloat(TGSConst.FadeAmount, 0);
            Region region = cells[cellIndex].region;
            fadeMaterial.color = (initialColor.a == 0 && region.customMaterial != null) ? region.customMaterial.color : initialColor;
            fadeMaterial.mainTexture = oldMaterial.mainTexture;
            disposalManager.MarkForDisposal(fadeMaterial);
            renderer.sharedMaterial = fadeMaterial;

            SurfaceFader.Animate(style, this, cellSurface, region, fadeMaterial, color, duration, repetitions);
        }


        void CellCancelAnimation(int cellIndex, float fadeOutDuration) {
            if (cellIndex < 0 || cellIndex >= cells.Count)
                return;
            int cacheIndex = GetCacheIndexForCellRegion(cellIndex);
            GameObject cellSurface = null;
            surfaces.TryGetValue(cacheIndex, out cellSurface);
            if (cellSurface == null)
                return;
            cellSurface.GetComponents<SurfaceFader>(tempFaders);
            int count = tempFaders.Count;
            for (int k = 0; k < count; k++) {
                tempFaders[k].Finish(fadeOutDuration);
            }
        }

        void CancelAnimationAll(float fadeOutDuration) {
            GetComponentsInChildren<SurfaceFader>(tempFaders);
            int count = tempFaders.Count;
            for (int k = 0; k < count; k++) {
                tempFaders[k].Finish(fadeOutDuration);
            }
        }

        bool ToggleCellsVisibility(Rect rect, bool visible, bool worldSpace = false) {
            if (cells == null)
                return false;
            int count = cells.Count;
            if (worldSpace) {
                Vector3 pos = rect.min;
                if (!GetLocalHitFromWorldPosition(ref pos))
                    return false;
                rect.min = pos;
                pos = rect.max;
                if (!GetLocalHitFromWorldPosition(ref pos))
                    return false;
                rect.max = pos;
            }
            if (rect.xMin < -0.4999f)
                rect.xMin = -0.4999f;
            if (rect.yMin < -0.4999f)
                rect.yMin = -0.4999f;
            if (rect.xMax > 0.4999f)
                rect.xMax = 0.4999f;
            if (rect.yMax > 0.4999f)
                rect.yMax = 0.4999f;

            Cell topLeft = CellGetAtPosition(rect.min, worldSpace);
            Cell bottomRight = CellGetAtPosition(rect.max, worldSpace);
            if (topLeft == null || bottomRight == null)
                return false;
            int row0 = topLeft.row;
            int col0 = topLeft.column;
            int row1 = bottomRight.row;
            int col1 = bottomRight.column;
            if (row1 < row0)
            {
                int tmp = row0;
                row0 = row1;
                row1 = tmp;
            }
            if (col1 < col0)
            {
                int tmp = col0;
                col0 = row1;
                col1 = tmp;
            }
            for (int k = 0; k < count; k++)
            {
                Cell cell = cells[k];
                if (cell.row >= row0 && cell.row <= row1 && cell.column >= col0 && cell.column <= col1)
                {
                    cell.visible = visible;
                }
            }

            ClearLastOver();
            //needRefreshRouteMatrix = true;
            refreshCellMesh = true;
            issueRedraw = RedrawType.Full;
            return true;
        }


        bool GetAdjacentCellCoordinates(int cellIndex, CELL_SIDE side, out int or, out int oc, out CELL_SIDE os) {
            Cell cell = cells[cellIndex];
            int r = cell.row;
            int c = cell.column;
            or = r;
            oc = c;
            os = side;
            if (_gridTopology == GRID_TOPOLOGY.Hexagonal) {
                int evenValue = _evenLayout ? 1 : 0;
                switch (side) {
                    case CELL_SIDE.Bottom:
                        or--;
                        os = CELL_SIDE.Top;
                        break;
                    case CELL_SIDE.Top:
                        or++;
                        os = CELL_SIDE.Bottom;
                        break;
                    case CELL_SIDE.BottomRight:
                        if (oc % 2 != evenValue) {
                            or--;
                        }
                        oc++;
                        os = CELL_SIDE.TopLeft;
                        break;
                    case CELL_SIDE.TopRight:
                        if (oc % 2 == evenValue) {
                            or++;
                        }
                        oc++;
                        os = CELL_SIDE.BottomLeft;
                        break;
                    case CELL_SIDE.TopLeft:
                        if (oc % 2 == evenValue) {
                            or++;
                        }
                        oc--;
                        os = CELL_SIDE.BottomRight;
                        break;
                    case CELL_SIDE.BottomLeft:
                        if (oc % 2 != evenValue) {
                            or--;
                        }
                        oc--;
                        os = CELL_SIDE.TopRight;
                        break;
                }
            } else if (_gridTopology == GRID_TOPOLOGY.Box) {
                switch (side) {
                    case CELL_SIDE.Bottom:
                        or--;
                        os = CELL_SIDE.Top;
                        break;
                    case CELL_SIDE.Top:
                        or++;
                        os = CELL_SIDE.Bottom;
                        break;
                    case CELL_SIDE.BottomRight:
                        or--;
                        oc++;
                        os = CELL_SIDE.TopLeft;
                        break;
                    case CELL_SIDE.TopRight:
                        or++;
                        oc++;
                        os = CELL_SIDE.BottomLeft;
                        break;
                    case CELL_SIDE.TopLeft:
                        or++;
                        oc--;
                        os = CELL_SIDE.BottomRight;
                        break;
                    case CELL_SIDE.BottomLeft:
                        or--;
                        oc--;
                        os = CELL_SIDE.TopRight;
                        break;
                    case CELL_SIDE.Left:
                        oc--;
                        os = CELL_SIDE.Right;
                        break;
                    case CELL_SIDE.Right:
                        oc++;
                        os = CELL_SIDE.Left;
                        break;
                }
            } else {
                return false;
            }
            return or >= 0 && or < _cellRowCount && oc >= 0 && oc < _cellColumnCount;
        }

        #endregion
    }
}