using UnityEngine;
using System;

namespace CYM.TGS {
	public partial class TGS : MonoBehaviour {

        #region Inspector
        [SerializeField]
		Terrain _terrain;
		[SerializeField]
		GameObject _terrainObject;
		[SerializeField]
		string _terrainObjectsPrefix;
		// Terrain data wrapper
		ITerrainWrapper _terrainWrapper;
		[SerializeField]
		Vector2 _terrainMeshPivot = new Vector2(0.5f, 0.5f);
		public Texture2D canvasTexture;
		[SerializeField]
		bool _transparentBackground;
		[SerializeField]
		bool _useGeometryShaders = true;
		[SerializeField]
		int _sortingOrder;
		int _heightmapSize = 513;
		[SerializeField]
		GRID_TOPOLOGY _gridTopology = GRID_TOPOLOGY.Box;
		[SerializeField]
		bool _evenLayout = false;
		[SerializeField]
		bool _regularHexagons;
		[SerializeField]
		float _hexSize = 0.01f;
		[SerializeField]
		float _regularHexagonsWidth;
		[SerializeField]
		int _gridRelaxation = 1;
		[SerializeField]
		float _gridCurvature = 0.0f;
		[SerializeField]
		HIGHLIGHT_MODE _highlightMode = HIGHLIGHT_MODE.Cells;
		[SerializeField]
		bool _allowHighlightWhileDragging = false;
		[SerializeField]
		float _highlightFadeMin = 0f;
		[SerializeField]
		float _highlightFadeAmount = 0.5f;
		[SerializeField]
		float _highlightScaleMin = 0.75f;
		[SerializeField]
		float _highlightScaleMax = 1.1f;
		[SerializeField]
		float _highlightFadeSpeed = 1f;
		[SerializeField]
		float _highlightMinimumTerrainDistance = 1f;
		[SerializeField]
		HIGHLIGHT_EFFECT _highlightEffect = HIGHLIGHT_EFFECT.Default;
		[SerializeField]
		OVERLAY_MODE _overlayMode = OVERLAY_MODE.Overlay;
		[SerializeField]
		Vector2 _gridCenter;
		[SerializeField]
		Vector2 _gridScale = new Vector2(1, 1);
		[SerializeField]
		float _gridElevation = 0;
		[SerializeField]
		float _gridElevationBase = 0;
		[SerializeField]
		float _gridMinElevationMultiplier = 1f;
		[SerializeField]
		float _gridCameraOffset = 0;
		[SerializeField]
		float _gridNormalOffset = 0;
		[SerializeField]
		int _gridMeshDepthOffset = -1;
		[SerializeField]
		int _gridSurfaceDepthOffset = -1;
		[SerializeField]
		int _gridSurfaceDepthOffsetTerritory = -1;
		[SerializeField]
		float _gridRoughness = 0.01f;
		[SerializeField]
		int _cellRowCount = 8;
		[SerializeField]
		int _cellColumnCount = 8;
		[SerializeField]
		bool _respectOtherUI = true;
		[SerializeField]
		bool _nearClipFadeEnabled;
		[SerializeField]
		float _nearClipFade = 25f;
		[SerializeField]
		float _nearClipFadeFallOff = 50f;
		[SerializeField]
		bool _farFadeEnabled;
		[SerializeField]
		float _farFadeDistance = 500f;
		[SerializeField]
		float _farFadeFallOff = 50f;
		[SerializeField]
		bool _circularFadeEnabled;
		[SerializeField]
		Transform _circularFadeTarget;
		[SerializeField]
		float _circularFadeDistance = 500f;
		[SerializeField]
		float _circularFadeFallOff = 50f;
		[SerializeField]
		bool _enableGridEditor = true;
		/// <summary>
		/// The camera reference used in certain computations
		/// </summary>
		public Camera cameraMain;
        #endregion

        #region property
        /// <summary>
        /// Terrain reference. Assign a terrain to this property to fit the grid to terrain height and dimensions
        /// </summary>
        public GameObject terrainObject {
			get {
				return _terrainObject;
			}
			set {
				if (_terrainObject != value) {
					_terrainObject = value;
					_terrainWrapper = null;
					BuildTerrainWrapper ();
					isDirty = true;
					//Redraw ();
				}
			}
		}
		public string terrainObjectsPrefix {
			get {
				return _terrainObjectsPrefix;
			}
			set {
				if (_terrainObjectsPrefix != value) {
					_terrainObjectsPrefix = value;	
					_terrainWrapper = null;
					BuildTerrainWrapper ();
					isDirty = true;
					//Redraw ();
				}
			}
		}
		public ITerrainWrapper terrain {
			get { return _terrainWrapper; }
		}
		/// <summary>
		/// Returns the terrain center in world space.
		/// </summary>
		public Vector3 terrainCenter {
			get {
				return _terrainWrapper.transform.position + new Vector3 (terrainWidth * 0.5f, 0, terrainDepth * 0.5f);
			}
		}
        public Vector2 terrainMeshPivot {
            get { return _terrainMeshPivot;  }
            set {  if (_terrainMeshPivot != value) {
                    _terrainMeshPivot = value;
                    if (_terrainWrapper != null && _terrainWrapper is MeshTerrainWrapper) {
                        ((MeshTerrainWrapper)_terrainWrapper).pivot = _terrainMeshPivot;
                        gridNeedsUpdate = true;
                    }
                }
            }
        }
		/// <summary>
		/// When enabled, grid is visible without background mesh
		/// </summary>
		public bool transparentBackground {
			get {
				return _transparentBackground;
			}
			set {
				if (_transparentBackground != value) {
					_transparentBackground = value;
					isDirty = true;
					//Redraw (true);
				}
			}
		}
		/// <summary>
		/// When enabled, geometry shaders will be used (if platform supports them)
		/// </summary>
		public bool useGeometryShaders {
			get {
				return _useGeometryShaders;
			}
			set {
				if (_useGeometryShaders != value) {
					_useGeometryShaders = value;
					isDirty = true;
					territoriesGeoMat = territoriesDisputedGeoMat = cellsGeoMat = null;
					LoadGeometryShaders();
					//Redraw(true);
				}
			}
		}
        /// <summary>
        /// Sets the sorting layer for the grid elements (only valid when rendering in transparent queue)
        /// </summary>
        public int sortingOrder {
            get { return _sortingOrder; }
            set {
                if (_sortingOrder != value) {
                    _sortingOrder = value;
                    //Redraw(true);
                }
            }
        }
		public int heightmapSize {
			get { return _heightmapSize; }
			set { 
				value = GetFittedSizeForHeightmap(value);
				if (_heightmapSize != value) {
					_heightmapSize = value;
					isDirty = true;
				}
			}
		}
		/// <summary>
		/// The grid type (boxed, hexagonal or irregular)
		/// </summary>
		public GRID_TOPOLOGY gridTopology { 
			get { return _gridTopology; } 
			set {
				if (_gridTopology != value) {
					_gridTopology = value;
					needGenerateMap = true;
					isDirty = true;
				}
			}
		}
		/// <summary>
		/// Returns the actual number of cells created according to the current grid topology
		/// </summary>
		/// <value>The cell count.</value>
		public int cellCount {
			get {
				return _cellRowCount * _cellColumnCount;
			}
		}
		/// <summary>
		/// Toggle even corner in hexagonal topology.
		/// </summary>
		public bool evenLayout { 
			get {
				return _evenLayout; 
			}
			set {
				if (value != _evenLayout) {
					_evenLayout = value;
					isDirty = true;
					needGenerateMap = true;
				}
			}
		}
		public bool regularHexagons {
			get { return _regularHexagons; }
			set {
				if (value != _regularHexagons) {
					_regularHexagons = value;
					isDirty = true;
					CellsUpdateBounds ();
					UpdateTerritoryBoundaries ();
					//Redraw ();
				}
			}
		}
		public float regularHexagonsWidth {
			get { return _regularHexagonsWidth; }
			set {
				if (value != _regularHexagonsWidth) {
					_regularHexagonsWidth = value;
					ComputeGridScale ();
					isDirty = true;
					CellsUpdateBounds ();
					UpdateTerritoryBoundaries ();
					//Redraw ();
				}
			}
		}
		/// <summary>
		/// Sets the relaxation iterations used to normalize cells sizes in irregular topology.
		/// </summary>
		public int gridRelaxation { 
			get { return _gridRelaxation; } 
			set {
				if (_gridRelaxation != value) {
					_gridRelaxation = value;
					needGenerateMap = true;
					isDirty = true;
				}
			}
		}
		/// <summary>
		/// Gets or sets the grid's curvature factor.
		/// </summary>
		public float gridCurvature { 
			get { return _gridCurvature; } 
			set {
				if (_gridCurvature != value) {
					_gridCurvature = value;
					needGenerateMap = true;
					isDirty = true;
				}
			}
		}
		public HIGHLIGHT_MODE highlightMode {
			get {
				return _highlightMode;
			}
			set {
				if (_highlightMode != value) {
					_highlightMode = value;
					isDirty = true;
					ClearLastOver ();
					HideCellRegionHighlight ();
					HideTerritoryRegionHighlight ();
					CheckCells();
					CheckTerritories ();
				}
			}
		}
		public bool allowHighlightWhileDragging {
			get {
				return _allowHighlightWhileDragging;
			}
			set {
				if (_allowHighlightWhileDragging != value) {
					_allowHighlightWhileDragging = value;
					isDirty = true;
				}
			}
		}
		public float highlightFadeMin {
			get {
				return _highlightFadeMin;
			}
			set {
				if (_highlightFadeMin != value) {
					_highlightFadeMin = value;
					isDirty = true;
				}
			}
		}
		public float highlightFadeAmount {
			get {
				return _highlightFadeAmount;
			}
			set {
				if (_highlightFadeAmount != value) {
					_highlightFadeAmount = value;
					isDirty = true;
				}
			}
		}
		public float highlightScaleMin {
			get {
				return _highlightScaleMin;
			}
			set {
				if (_highlightScaleMin != value) {
					_highlightScaleMin = value;
					isDirty = true;
				}
			}
		}
		public float highlightScaleMax {
			get {
				return _highlightScaleMax;
			}
			set {
				if (_highlightScaleMax != value) {
					_highlightScaleMax = value;
					isDirty = true;
				}
			}
		}
		public float highlightFadeSpeed {
			get {
				return _highlightFadeSpeed;
			}
			set {
				if (_highlightFadeSpeed != value) {
					_highlightFadeSpeed = value;
					isDirty = true;
				}
			}
		}
		/// <summary>
		/// Minimum distance from camera for cells to be highlighted on terrain
		/// </summary>
		public float highlightMinimumTerrainDistance {
			get {
				return _highlightMinimumTerrainDistance;
			}
			set {
				if (_highlightMinimumTerrainDistance != value) {
					_highlightMinimumTerrainDistance = value;
					isDirty = true;
				}
			}
		}
		public HIGHLIGHT_EFFECT highlightEffect {
			get {
				return _highlightEffect;
			}
			set {
				if (_highlightEffect != value) {
					_highlightEffect = value;
					isDirty = true;
					UpdateHighlightEffect ();
				}
			}
		}
		public OVERLAY_MODE overlayMode {
			get {
				return _overlayMode;
			}
			set {
				if (_overlayMode != value) {
					_overlayMode = value;
					isDirty = true;
				}
			}
		}
		/// <summary>
		/// Center of the grid relative to the Terrain (by default, 0,0, which means center of terrain)
		/// </summary>
		public Vector2 gridCenter { 
			get { return _gridCenter; } 
			set {
				if (_gridCenter != value) {
					_gridCenter = value;
					isDirty = true;
					CellsUpdateBounds ();
					UpdateTerritoryBoundaries ();
					//Redraw ();
				}
			}
		}
		/// <summary>
		/// Center of the grid in world space coordinates. You can also use this property to reposition the grid on a given world position coordinate.
		/// </summary>
		public Vector3 gridCenterWorldPosition { 
			get { return GetWorldSpacePosition (_gridCenter); } 
			set { SetGridCenterWorldPosition (value, false); }
		}
		/// <summary>
		/// Scale of the grid on the Terrain (by default, 1,1, which means occupy entire terrain)
		/// </summary>
		public Vector2 gridScale { 
			get { return _gridScale; } 
			set {
				if (_gridScale != value) {
					_gridScale = value;
					ComputeGridScale ();
					isDirty = true;
					CellsUpdateBounds ();
					UpdateTerritoryBoundaries ();
					//Redraw ();
				}
			}
		}
		public float gridElevation { 
			get { return _gridElevation; } 
			set {
				if (_gridElevation != value) {
					_gridElevation = value;
					isDirty = true;
					FitToTerrain ();
				}
			}
		}
		public float gridElevationBase { 
			get { return _gridElevationBase; } 
			set {
				if (_gridElevationBase != value) {
					_gridElevationBase = value;
					isDirty = true;
					FitToTerrain ();
				}
			}
		}
		public float gridElevationCurrent { get { return _gridElevation + _gridElevationBase; } }
		public float gridMinElevationMultiplier { 
			get { return _gridMinElevationMultiplier; } 
			set {
				if (_gridMinElevationMultiplier != value && value>=0) {
					_gridMinElevationMultiplier = value;
					isDirty = true;
					//Redraw (true);
				}
			}
		}
		public float gridCameraOffset { 
			get { return _gridCameraOffset; } 
			set {
				if (_gridCameraOffset != value) {
					_gridCameraOffset = value;
					isDirty = true;
					FitToTerrain ();
				}
			}
		}
		public float gridNormalOffset { 
			get { return _gridNormalOffset; } 
			set {
				if (_gridNormalOffset != value) {
					_gridNormalOffset = value;
					isDirty = true;
					//Redraw ();
				}
			}
		}
		public int gridMeshDepthOffset { 
			get { return _gridMeshDepthOffset; } 
			set {
				if (_gridMeshDepthOffset != value) {
					_gridMeshDepthOffset = value;
					UpdateMaterialDepthOffset ();
					isDirty = true;
				}
			}
		}
		public int gridSurfaceDepthOffset { 
			get { return _gridSurfaceDepthOffset; } 
			set {
				if (_gridSurfaceDepthOffset != value) {
					_gridSurfaceDepthOffset = value;
					UpdateMaterialDepthOffset ();
					isDirty = true;
				}
			}
		}
		public int gridSurfaceDepthOffsetTerritory { 
			get { return _gridSurfaceDepthOffsetTerritory; } 
			set {
				if (_gridSurfaceDepthOffsetTerritory != value) {
					_gridSurfaceDepthOffsetTerritory = value;
					UpdateMaterialDepthOffset ();
					isDirty = true;
				}
			}
		}
		public float gridRoughness { 
			get { return _gridRoughness; } 
			set {
				if (_gridRoughness != value) {
					_gridRoughness = value;
					isDirty = true;
					//Redraw ();
				}
			}
		}
		/// <summary>
		/// Returns the number of rows for box and hexagonal grid topologies
		/// </summary>
		public int rowCount { 
			get {
				return _cellRowCount;
			}
			set {
				if (value != _cellRowCount) {
					_cellRowCount = Mathf.Clamp (value, 2, MAX_ROWS_OR_COLUMNS);
					isDirty = true;
					needGenerateMap = true;
					CheckChanges ();
				}
			}

		}
		/// <summary>
		/// Returns the number of columns for box and hexagonal grid topologies
		/// </summary>
		public int columnCount { 
			get {
				return _cellColumnCount;
			}
			set {
				if (value != _cellColumnCount) {
					_cellColumnCount = Mathf.Clamp (value, 2, MAX_ROWS_OR_COLUMNS);
					isDirty = true;
					needGenerateMap = true;
					CheckChanges ();
				}
			}
		}
		/// <summary>
		/// Sets the dimensions of the grid in one step. This is faster than setting rowCount and columnCount separately.
		/// </summary>
		/// <param name="rows">Rows.</param>
		/// <param name="columns">Columns.</param>
        /// <param name="keepCellSize">Ensures the individual cell size is preserved</param>
		public void SetDimensions(int rows, int columns, bool keepCellSize = false) {
			_cellRowCount = Mathf.Clamp (rows, 2, MAX_ROWS_OR_COLUMNS);
			_cellColumnCount = Mathf.Clamp (columns, 2, MAX_ROWS_OR_COLUMNS);
			if (keepCellSize) {
				SetScaleByCellSize();
			}
			isDirty = true;
			needGenerateMap = true;
		}
		/// <summary>
		/// When enabled, will prevent interaction if pointer is over an UI element
		/// </summary>
		public bool	respectOtherUI {
			get { return _respectOtherUI; }
			set {
				if (value != _respectOtherUI) {
					_respectOtherUI = value;
					isDirty = true;
				}
			}
		}
		/// <summary>
		/// When enabled, lines near the camera will fade out gracefully
		/// </summary>
		public bool	nearClipFadeEnabled {
			get { return _nearClipFadeEnabled; }
			set {
				if (value != _nearClipFadeEnabled) {
					_nearClipFadeEnabled = value;
					isDirty = true;
					UpdateMaterialNearClipFade ();
				}
			}
		}
		public float nearClipFade { 
			get { return _nearClipFade; } 
			set {
				if (_nearClipFade != value) {
					_nearClipFade = Mathf.Max(0, value);
					isDirty = true;
					UpdateMaterialNearClipFade ();
				}
			}
		}
		public float nearClipFadeFallOff { 
			get { return _nearClipFadeFallOff; } 
			set {
				if (_nearClipFadeFallOff != value) {
					_nearClipFadeFallOff = Mathf.Max (value, 0.001f);
					isDirty = true;
					UpdateMaterialNearClipFade ();
				}
			}
		}
		/// <summary>
		/// When enabled, lines far the camera will fade out gracefully
		/// </summary>
		public bool farFadeEnabled {
			get { return _farFadeEnabled; }
			set {
				if (value != _farFadeEnabled) {
					_farFadeEnabled = value;
					isDirty = true;
					UpdateMaterialFarFade();
				}
			}
		}
		public float farFadeDistance {
			get { return _farFadeDistance; }
			set {
				if (_farFadeDistance != value) {
					_farFadeDistance = Mathf.Max(0, value);
					isDirty = true;
					UpdateMaterialFarFade();
				}
			}
		}
		public float farFadeFallOff {
			get { return _farFadeFallOff; }
			set {
				if (_farFadeFallOff != value) {
					_farFadeFallOff = Mathf.Max(value, 0.001f);
					isDirty = true;
					UpdateMaterialFarFade();
				}
			}
		}
		/// <summary>
		/// When enabled, grid is faded out according to distance to a gameoject
		/// </summary>
		public bool circularFadeEnabled {
			get { return _circularFadeEnabled; }
			set {
				if (value != _circularFadeEnabled) {
					_circularFadeEnabled = value;
					isDirty = true;
					UpdateMaterialCircularFade();
				}
			}
		}
		/// <summary>
        /// The gameobject reference for the circular fade out effect
        /// </summary>
		public Transform circularFadeTarget {
			get { return _circularFadeTarget; }
			set {
				if (_circularFadeTarget != value) {
					_circularFadeTarget = value;
					isDirty = true;
					UpdateMaterialCircularFade();
				}
			}
		}
		public float circularFadeDistance {
			get { return _circularFadeDistance; }
			set {
				if (_circularFadeDistance != value) {
					_circularFadeDistance = Mathf.Max(0, value);
					isDirty = true;
					UpdateMaterialCircularFade();
				}
			}
		}
		public float circularFadeFallOff {
			get { return _circularFadeFallOff; }
			set {
				if (_circularFadeFallOff != value) {
					_circularFadeFallOff = Mathf.Max(value, 0.001f);
					isDirty = true;
					UpdateMaterialCircularFade();
				}
			}
		}
		/// <summary>
		/// Enabled grid editing options in Scene View
		/// </summary>
		public bool enableGridEditor { 
			get {
				return _enableGridEditor; 
			}
			set {
				if (value != _enableGridEditor) {
					_enableGridEditor = value;
					if (!_enableGridEditor && !Application.isPlaying) {
						HideTerritoryRegionHighlight();
						HideCellRegionHighlight();
					}
					isDirty = true;
				}
			}
		}
		/// <summary>
		/// Returns a reference of the currently highlighted gameobject (cell or territory)
		/// </summary>
		public GameObject highlightedObj { get { return _highlightedObj; } }
        #endregion 

        #region Public General Functions

        /// <summary>
        /// Used to cancel highlighting on a given gameobject. This call is ignored if go is not currently highlighted.
        /// </summary>
        public void HideHighlightedObject (GameObject go) {
			if (go != _highlightedObj)
				return;
			_cellHighlightedIndex = -1;
			_cellHighlighted = null;
			_territoryHighlightedIndex = -1;
			_territoryHighlightedRegionIndex = -1;
			_territoryRegionHighlighted = null;
			_territoryLastOverIndex = -1;
			_territoryRegionLastOverIndex = -1;
			_highlightedObj = null;
			ClearLastOver ();
		}
		public void SetGridCenterWorldPosition (Vector3 position, bool snapToGrid) {
			if (snapToGrid) {
				position = SnapToCell (position, true, false);
			}
			if (_terrainWrapper != null) {
                gridCenter = _terrainWrapper.GetLocalPoint(_terrainObject, position);
			} else {
				transform.position = position;
			}
		}
		/// <summary>
		/// Snaps a position to the grid
		/// </summary>
		public Vector3 SnapToCell (Vector3 position, bool worldSpace = true, bool snapToCenter = true) {

			if (worldSpace) {
				position = transform.InverseTransformPoint (position);
			}
			position.x = (float)Math.Round (position.x, 6);
			position.y = (float)Math.Round (position.y, 6);
			if (_gridTopology == GRID_TOPOLOGY.Box) {
				float stepX = _gridScale.x / _cellColumnCount;
				position.x -= _gridCenter.x;
				if (snapToCenter && _cellColumnCount % 2 == 0) {
					position.x = (Mathf.FloorToInt (position.x / stepX) + 0.5f) * stepX;
				} else {
					position.x = (Mathf.FloorToInt (position.x / stepX + 0.5f)) * stepX;
				}
				position.x += _gridCenter.x;
				float stepY = _gridScale.y / _cellRowCount;
				position.y -= _gridCenter.y;
				if (snapToCenter && _cellRowCount % 2 == 0) {
					position.y = (Mathf.FloorToInt (position.y / stepY) + 0.5f) * stepY;
				} else {
					position.y = (Mathf.FloorToInt (position.y / stepY + 0.5f)) * stepY;
				}
				position.y += _gridCenter.y;
			} else if (_gridTopology == GRID_TOPOLOGY.Hexagonal) {

				if (snapToCenter) {
					Cell cell = CellGetAtPosition (position);
					if (cell != null) {
						position = cell.scaledCenter;
					}
				} else {
					float qx = 1f + (_cellColumnCount - 1f) * 3f / 4f;
					float qy = _cellRowCount + 0.5f;

					float stepX = _gridScale.x / qx;
					float stepY = _gridScale.y / qy;

					float halfStepX = stepX * 0.5f;
					float halfStepY = stepY * 0.5f;

					int evenLayout = _evenLayout ? 1 : 0;

					float k = Mathf.FloorToInt (position.x * _cellColumnCount / _gridScale.x);
					float j = Mathf.FloorToInt (position.y * _cellRowCount / _gridScale.y);
					position.x = k * stepX; // + halfStepX;
					position.y = j * stepY;
					position.x -= k * halfStepX / 2;
					float offsetY = (k % 2 == evenLayout) ? 0 : -halfStepY;
					position.y += offsetY;
				}

			} else {
				// try to get cell under position and returns its center
				Cell c = CellGetAtPosition (position);
				if (c != null) {
					position = c.center;
				}
			}
            if (worldSpace) {
                position = transform.TransformPoint(position);
            }
			return position;
		}
		/// <summary>
		/// Returns the rectangle area where cells are drawn in local or world space coordinates.
		/// </summary>
		/// <returns>The rect.</returns>
		public Rect GetRect (bool worldSpace = true) {
			Rect rect = new Rect ();
			Vector3 min = GetScaledVector (new Vector3 (-0.5f, -0.5f, 0));
			Vector3 max = GetScaledVector (new Vector3 (0.5f, 0.5f, 0));
			if (worldSpace) {
				min = transform.TransformPoint (min);
				max = transform.TransformPoint (max);
			}
			rect.min = min;
			rect.max = max;
			return rect;
		}


		/// <summary>
		/// Hides current highlighting effect
		/// </summary>
		public void HideHighlightedRegions () {
			HideTerritoryRegionHighlight ();
			HideCellRegionHighlight ();
		}

		/// <summary>
		/// Escales the gameobject of a colored/textured surface
		/// </summary>
		/// <param name="surf">Surf.</param>
		/// <param name="center">Center.</param>
		/// <param name="scale">Scale.</param>
		public void ScaleSurface (GameObject surf, Vector2 center, float scale) {
			if (surf == null)
				return;
			Transform t = surf.transform;

			t.localScale = new Vector3 (t.localScale.x * scale, t.localScale.y * scale, 1f);
			Vector3 originShift = center;
			originShift.x *= t.localScale.x;
			originShift.y *= t.localScale.y;
			originShift.x -= center.x;
			originShift.y -= center.y;
			originShift.z = 0;
			t.localPosition -= originShift;
		}
		/// <summary>
		/// Returns current bounds of grid in world space
		/// </summary>
		/// <value>The bounds.</value>
		public Bounds bounds {
			get {
				Vector3 min = transform.TransformPoint (GetScaledVector (new Vector3 (-0.5f, -0.5f, 0)));
				Vector3 max = transform.TransformPoint (GetScaledVector (new Vector3 (0.5f, 0.5f, 0)));
				Vector3 size = max - min;
				if (size.y <= 0) size.y = 0.0001f;
				return new Bounds ((min + max) * 0.5f, size);
			}
		}

		/// <summary>
        /// Returns true if a given position lies within this grid on the X/Z plane
        /// </summary>
        /// <returns></returns>
		public bool Contains(Vector3 position) {
			Bounds bb = bounds;
			return position.x >= bb.min.x && position.x <= bb.max.x && position.z >= bb.min.z && position.z <= bb.max.z;
        }
        #endregion
    }
}

