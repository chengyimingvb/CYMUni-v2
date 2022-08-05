using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CYM.TGS
{
    public partial class TGS : MonoBehaviour
    {
        #region Instance
        public static TGS Instance { get; private set; }
        #endregion

        #region Gameloop events
        private void Awake()
        {
            Instance = this;
        }
        public void OnEnable()
        {
            // Migration from hexSize
            if (_regularHexagonsWidth == 0)
            {
                _regularHexagonsWidth = _hexSize * transform.lossyScale.x;
            }
            if (cameraMain == null)
            {
                cameraMain = Camera.main;
            }
            if (cameraMain != null)
            {
                if ((cameraMain.cullingMask & (1 << gameObject.layer)) == 0)
                {
                    Debug.LogWarning("Camera is culling Terrain Grid System objects! Check the layer of Terrain Grid System and the culling mask of the camera.");
                }
            }
            if (cells == null)
            {
                Init();
            }
            if (hudMatTerritoryOverlay != null && hudMatTerritoryOverlay.color != _territoryHighlightColor)
            {
                hudMatTerritoryOverlay.color = _territoryHighlightColor;
            }
            hudMatTerritoryOverlay.SetColor(TGSConst.Color2, _territoryHighlightColor2);
            if (hudMatTerritoryGround != null && hudMatTerritoryGround.color != _territoryHighlightColor)
            {
                hudMatTerritoryGround.color = _territoryHighlightColor;
            }
            hudMatTerritoryGround.SetColor(TGSConst.Color2, _territoryHighlightColor2);
            if (hudMatCellOverlay != null && hudMatCellOverlay.color != _cellHighlightColor)
            {
                hudMatCellOverlay.color = _cellHighlightColor;
            }
            hudMatCellOverlay.SetColor(TGSConst.Color2, _cellHighlightColor2);
            if (hudMatCellGround != null && hudMatCellGround.color != _cellHighlightColor)
            {
                hudMatCellGround.color = _cellHighlightColor;
            }
            hudMatCellGround.SetColor(TGSConst.Color2, _cellHighlightColor2);
            if (territoriesThinMat != null && territoriesThinMat.color != _territoryFrontierColor)
            {
                territoriesThinMat.color = _territoryFrontierColor;
            }
            if (_territoryDisputedFrontierColor == new Color(0, 0, 0, 0))
            {
                _territoryDisputedFrontierColor = _territoryFrontierColor;
            }
            if (territoriesDisputedThinMat != null && territoriesDisputedThinMat.color != _territoryDisputedFrontierColor)
            {
                territoriesDisputedThinMat.color = _territoryDisputedFrontierColor;
            }
            if (territoriesDisputedGeoMat != null && territoriesDisputedGeoMat.color != _territoryDisputedFrontierColor)
            {
                territoriesDisputedGeoMat.color = _territoryDisputedFrontierColor;
            }
            if (cellsThinMat != null && cellsThinMat.color != _cellBorderColor)
            {
                cellsThinMat.color = _cellBorderColor;
            }
        }
        void OnDestroy()
        {
            if (_terrainWrapper != null)
            {
                _terrainWrapper.Dispose();
            }
            if (disposalManager != null)
            {
                disposalManager.DisposeAll();
            }
        }
        void LateUpdate()
        {
            CheckChanges();
            if (_circularFadeEnabled && _circularFadeTarget != null)
            {
                Shader.SetGlobalVector(TGSConst.CircularFadePosition, _circularFadeTarget.position);
            }
            if (Application.isMobilePlatform && !_allowHighlightWhileDragging)
            {
                if (Input.touchCount != 1)
                    return;
            }
            bool mobileTouch = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
            // Check whether the points is on an UI element, then avoid user interaction
            if (respectOtherUI)
            {
                if (!canInteract && Application.isMobilePlatform && !mobileTouch)
                    return;

                canInteract = true;
                if (UnityEngine.EventSystems.EventSystem.current != null)
                {
                    if (Application.isMobilePlatform && Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    {
                        canInteract = false;
                    }
                    else if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
                        canInteract = false;
                }
                if (!canInteract)
                {
                    HideTerritoryRegionHighlight();
                    HideCellRegionHighlight();
                    return;
                }
            }
            // check mouse pos and input events
            // on mobile only check when touch start and on desktop do not check if dragging
            bool usingDragEvents = OnCellDrag != null || OnCellDragStart != null || OnCellDragEnd != null;
            bool checkInputOnDesktop = !Application.isMobilePlatform && (!Input.GetMouseButton(0) || usingDragEvents);
            if (_allowHighlightWhileDragging || checkInputOnDesktop || mobileTouch)
            {
                CheckMousePos();
            }
            // dispatch events
            TriggerEvents();
            UpdateHighlightFade();
        }
        void OnMouseEnter()
        {
            mouseIsOver = true;
            ClearLastOver();
        }
        void OnMouseExit()
        {
            if (cameraMain == null)
                return;
            // Make sure it's outside of grid
            Vector3 mousePos = Input.mousePosition;
            Ray ray = cameraMain.ScreenPointToRay(mousePos);
            int hitCount = Physics.RaycastNonAlloc(ray, hits);
            if (hitCount > 0)
            {
                for (int k = 0; k < hitCount; k++)
                {
                    if (hits[k].collider.gameObject == gameObject)
                        return;
                }
            }
            mouseIsOver = false;
            ClearLastOver();
        }
        #endregion

        #region Initialization
        public void Init()
        {
            disposalManager = new DisposalManager();
            tempFaders = new List<SurfaceFader>();
            tempListCells = new List<int>();
            tempPoints = new List<Vector3>();
            tempUVs = new List<Vector4>();
            tempIndices = new List<int>();
            coloredMatCacheGroundCell = new Dictionary<int, Material>();
            coloredMatCacheOverlayCell = new Dictionary<int, Material>();
            frontierColorCache = new Dictionary<Color, Material>();

            LoadGeometryShaders();
            LoadMaterials();           
            UpdatePreventOverdrawSettings();

            if (hits == null || hits.Length == 0)
            {
                hits = new RaycastHit[100];
            }

            if (factoryColors == null || factoryColors.Length < MAX_TERRITORIES)
            {
                factoryColors = new Color[MAX_TERRITORIES];
                for (int k = 0; k < factoryColors.Length; k++)
                    factoryColors[k] = new Color(UnityEngine.Random.Range(0.0f, 0.5f), UnityEngine.Random.Range(0.0f, 0.5f), UnityEngine.Random.Range(0.0f, 0.5f));
            }
            if (_sortedTerritoriesRegions == null)
                _sortedTerritoriesRegions = new List<Region>(MAX_TERRITORIES);

            BuildTerrainWrapper(); 
            if (issueRedraw == RedrawType.None)
            {
                issueRedraw = RedrawType.Full;
            }
            CheckChanges();
            CreateTerritories(territoriesTexture, territoriesTextureNeutralColor, territoriesHideNeutralCells);
        }
        /// <summary>
        /// Automatically generates territories based on the different colors included in the texture.
        /// </summary>
        /// <param name="neutral">This color won't generate any texture.</param>
        public void CreateTerritories(Texture2D texture, Color neutral, bool hideNeutralCells = false)
        {
            if (texture == null || cells == null)
                return;
            List<Color> dsColors = new List<Color>();
            Dictionary<Color, int> dsColorDict = new Dictionary<Color, int>();
            int cellCount = cells.Count;
            Color[] colors;
            try
            {
                colors = texture.GetPixels();
            }
            catch
            {
                Debug.Log("Texture used to create territories is not readable. Check import settings.");
                return;
            }
            for (int k = 0; k < cellCount; k++)
            {
                if (!cells[k].visible)
                    continue;
                Vector2 uv = cells[k].center;
                uv.x += 0.5f;
                uv.y += 0.5f;

                int x = (int)(uv.x * texture.width);
                int y = (int)(uv.y * texture.height);
                int pos = y * texture.width + x;
                if (pos < 0 || pos >= colors.Length)
                    continue;
                Color pixelColor = colors[pos];
                int territoryIndex;
                if (!dsColorDict.TryGetValue(pixelColor, out territoryIndex))
                {
                    dsColors.Add(pixelColor);
                    territoryIndex = dsColors.Count - 1;
                    dsColorDict[pixelColor] = territoryIndex;
                }
                cells[k].territoryIndex = territoryIndex;
                if (territoryIndex >= MAX_TERRITORIES - 1)
                    break;
            }
            needUpdateTerritories = true;
            if (dsColors.Count > 0)
            {
                _numTerritories = dsColors.Count;
                _showTerritories = true;

                if (territories == null)
                {
                    territories = new List<Territory>(_numTerritories);
                }
                else
                {
                    territories.Clear();
                }
                for (int c = 0; c < _numTerritories; c++)
                {
                    Territory territory = new Territory(c.ToString());
                    Color territoryColor = dsColors[c];
                    if (territoryColor.r != neutral.r || territoryColor.g != neutral.g || territoryColor.b != neutral.b)
                    {
                        territory.fillColor = territoryColor;
                    }
                    else
                    {
                        territory.fillColor = new Color(0, 0, 0, 0);
                        territory.visible = false;
                    }
                    // Add cells to territories
                    for (int k = 0; k < cellCount; k++)
                    {
                        Cell cell = cells[k];
                        if (cell.territoryIndex == c)
                        {
                            territory.cells.Add(cell);
                            territory.center += cell.center;
                        }
                    }
                    if (territory.cells.Count > 0)
                    {
                        territory.center /= territory.cells.Count;
                        // Ensure center belongs to territory
                        Cell cellAtCenter = CellGetAtPosition(territory.center);
                        if (cellAtCenter != null && cellAtCenter.territoryIndex != c)
                        {
                            territory.center = territory.cells[0].center;
                        }
                    }
                    territories.Add(territory);
                }

                isDirty = true;
                issueRedraw = RedrawType.Full;
                //Redraw();
            }
        }
        void CheckChanges()
        {
            if (applyingChanges) return;
            applyingChanges = true;
            if (needGenerateMap)
            {
                GenerateMap(true);
            }
            if (needResortCells)
            {
                ResortCells();
            }
            FitToTerrain();         // Verify if there're changes in container and adjust the grid mesh accordingly
            if (issueRedraw != RedrawType.None)
            {
                Redraw(issueRedraw == RedrawType.IncrementalTerritories);
            }
            applyingChanges = false;
        }
        void LoadMaterials()
        {
            if (territoriesThinMat == null)
            {
                territoriesThinMat = Instantiate(Resources.Load<Material>("Materials/Territory"));
                disposalManager.MarkForDisposal(territoriesThinMat);
            }
            if (territoriesDisputedThinMat == null)
            {
                territoriesDisputedThinMat = Instantiate(territoriesThinMat);
                disposalManager.MarkForDisposal(territoriesDisputedThinMat);
                territoriesDisputedThinMat.color = _territoryDisputedFrontierColor;
            }
            if (cellsThinMat == null)
            {
                cellsThinMat = Instantiate(Resources.Load<Material>("Materials/Cell"));
                disposalManager.MarkForDisposal(cellsThinMat);
            }
            if (hudMatTerritoryOverlay == null)
            {
                hudMatTerritoryOverlay = new Material(Shader.Find("Terrain Grid System/Unlit Highlight Ground Texture"));
                hudMatTerritoryOverlay.SetInt(TGSConst.Cull, (int)UnityEngine.Rendering.CullMode.Off);
                hudMatTerritoryOverlay.SetInt(TGSConst.ZTest, (int)UnityEngine.Rendering.CompareFunction.Always);
                disposalManager.MarkForDisposal(hudMatTerritoryOverlay);
            }
            if (hudMatTerritoryGround == null)
            {
                hudMatTerritoryGround = Instantiate(hudMatTerritoryOverlay) as Material;
                hudMatTerritoryGround.SetInt(TGSConst.Cull, (int)UnityEngine.Rendering.CullMode.Back);
                hudMatTerritoryGround.SetInt(TGSConst.ZTest, (int)UnityEngine.Rendering.CompareFunction.LessEqual);
                disposalManager.MarkForDisposal(hudMatTerritoryGround);
            }
            if (hudMatCellOverlay == null)
            {
                hudMatCellOverlay = Instantiate(hudMatTerritoryOverlay) as Material;
                hudMatCellOverlay.SetInt(TGSConst.Cull, (int)UnityEngine.Rendering.CullMode.Off);
                hudMatCellOverlay.SetInt(TGSConst.ZTest, (int)UnityEngine.Rendering.CompareFunction.Always);
                disposalManager.MarkForDisposal(hudMatCellOverlay);
            }
            if (hudMatCellGround == null)
            {
                hudMatCellGround = Instantiate(hudMatTerritoryOverlay) as Material;
                hudMatCellGround.SetInt(TGSConst.Cull, (int)UnityEngine.Rendering.CullMode.Back);
                hudMatCellGround.SetInt(TGSConst.ZTest, (int)UnityEngine.Rendering.CompareFunction.LessEqual);
                disposalManager.MarkForDisposal(hudMatCellGround);
            }
            // Materials for cells
            if (coloredMatGroundCell == null)
            {
                coloredMatGroundCell = Instantiate(Resources.Load<Material>("Materials/ColorizedRegionGround"));
                coloredMatGroundCell.renderQueue += 50;
                disposalManager.MarkForDisposal(coloredMatGroundCell);
            }
            if (coloredMatOverlayCell == null)
            {
                coloredMatOverlayCell = Instantiate(Resources.Load<Material>("Materials/ColorizedRegionOverlay"));
                coloredMatOverlayCell.renderQueue += 50;
                disposalManager.MarkForDisposal(coloredMatOverlayCell);
            }
            if (texturizedMatGroundCell == null)
            {
                texturizedMatGroundCell = Instantiate(Resources.Load<Material>("Materials/TexturizedRegionGround"));
                texturizedMatGroundCell.renderQueue += 50;
                disposalManager.MarkForDisposal(texturizedMatGroundCell);
            }
            if (texturizedMatOverlayCell == null)
            {
                texturizedMatOverlayCell = Instantiate(Resources.Load<Material>("Materials/TexturizedRegionOverlay"));
                texturizedMatOverlayCell.renderQueue += 50;
                disposalManager.MarkForDisposal(texturizedMatOverlayCell);
            }
            // Materials for territories
            if (coloredMatGroundTerritory == null)
            {
                coloredMatGroundTerritory = Instantiate(Resources.Load<Material>("Materials/ColorizedRegionGround"));
                disposalManager.MarkForDisposal(coloredMatGroundTerritory);
            }
            if (coloredMatOverlayTerritory == null)
            {
                coloredMatOverlayTerritory = Instantiate(Resources.Load<Material>("Materials/ColorizedRegionOverlay"));
                disposalManager.MarkForDisposal(coloredMatOverlayTerritory);
            }
            if (texturizedMatGroundTerritory == null)
            {
                texturizedMatGroundTerritory = Instantiate(Resources.Load<Material>("Materials/TexturizedRegionGround"));
                disposalManager.MarkForDisposal(texturizedMatGroundTerritory);
            }
            if (texturizedMatOverlayTerritory == null)
            {
                texturizedMatOverlayTerritory = Instantiate(Resources.Load<Material>("Materials/TexturizedRegionOverlay"));
                disposalManager.MarkForDisposal(texturizedMatOverlayTerritory);
            }
        }
        void LoadGeometryShaders()
        {
            canUseGeometryShaders = _useGeometryShaders && SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Metal;
            if (territoriesGeoMat == null)
            {
                if (canUseGeometryShaders)
                {
                    territoriesGeoMat = new Material(Shader.Find("Terrain Grid System/Unlit Single Color Territory Geo"));
                }
                else
                {
                    territoriesGeoMat = new Material(Shader.Find("Terrain Grid System/Unlit Single Color Territory Thick Hack"));
                }
                disposalManager.MarkForDisposal(territoriesGeoMat);
            }
            if (territoriesGeoMat != null && territoriesGeoMat.color != _territoryFrontierColor)
            {
                territoriesGeoMat.color = _territoryFrontierColor;
            }
            if (territoriesDisputedGeoMat == null)
            {
                territoriesDisputedGeoMat = Instantiate(territoriesGeoMat) as Material;
                disposalManager.MarkForDisposal(territoriesDisputedGeoMat);
            }
            if (territoriesDisputedGeoMat != null && territoriesDisputedGeoMat.color != _territoryDisputedFrontierColor)
            {
                territoriesDisputedGeoMat.color = _territoryDisputedFrontierColor;
            }
            if (cellsGeoMat == null)
            {
                if (canUseGeometryShaders)
                {
                    cellsGeoMat = new Material(Shader.Find("Terrain Grid System/Unlit Single Color Cell Geo"));
                }
                else
                {
                    cellsGeoMat = new Material(Shader.Find("Terrain Grid System/Unlit Single Color Cell Thick Hack"));
                }
                disposalManager.MarkForDisposal(cellsGeoMat);
            }
            if (cellsGeoMat != null && cellsGeoMat.color != _cellBorderColor)
            {
                cellsGeoMat.color = _cellBorderColor;
            }
            if (frontierColorCache != null)
            {
                frontierColorCache.Clear();
            }
        }
        void CreateSurfacesLayer()
        {
            Transform t = transform.Find("Surfaces");
            if (t != null)
            {
                DestroyImmediate(t.gameObject);
            }
            _surfacesLayer = new GameObject("Surfaces");
            _surfacesLayer.transform.SetParent(transform, false);
            _surfacesLayer.transform.localPosition = TGSConst.Vector3zero;
            _surfacesLayer.layer = gameObject.layer;
        }
        void DestroySurfaces()
        {
            HideTerritoryRegionHighlight();
            HideCellRegionHighlight();
            if (segmentHit != null)
                segmentHit.Clear();
            if (surfaces != null)
                surfaces.Clear();
            if (_surfacesLayer != null)
                DestroyImmediate(_surfacesLayer);
        }
        void DestroyTerritorySurfaces()
        {
            HideTerritoryRegionHighlight();
            if (territories != null)
            {
                int territoriesCount = territories.Count;
                for (int k = 0; k < territoriesCount; k++)
                {
                    Territory terr = territories[k];
                    for (int r = 0; r < terr.regions.Count; r++)
                    {
                        if (terr.regions[r] != null)
                        {
                            terr.regions[r].DestroySurface();
                        }
                    }
                }
            }
        }
        void DestroySurfacesDirty()
        {
            if (cells != null)
            {
                int cellCount = cells.Count;
                for (int k = 0; k < cellCount; k++)
                {
                    Cell cell = cells[k];
                    if (cell != null && cell.isDirty)
                    {
                        int cacheIndex = GetCacheIndexForCellRegion(k);
                        if (cell.region != null)
                        {
                            cell.region.DestroySurface();
                        }
                        surfaces[cacheIndex] = null;
                    }
                }
                int territoryCount = territories.Count;
                for (int k = 0; k < territoryCount; k++)
                {
                    Territory territory = territories[k];
                    if (territory != null && territory.isDirty)
                    {
                        for (int r = 0; r < territory.regions.Count; r++)
                        {
                            int cacheIndex = GetCacheIndexForTerritoryRegion(k, r);
                            if (territory.regions[r] != null)
                            {
                                territory.regions[r].DestroySurface();
                            }
                            surfaces[cacheIndex] = null;
                        }
                    }
                }
            }
        }
        #endregion

        #region public
        public void GenerateMap(bool reuseTerrainData = false)
        {
            needGenerateMap = false;
            recreateCells = true;
            recreateTerritories = true;
            if (cells != null)
                cells.Clear();
            if (territories != null)
                territories.Clear();
            Redraw(reuseTerrainData);
            CreateTerritories(territoriesTexture, territoriesTextureNeutralColor, territoriesHideNeutralCells);
        }
        /// <summary>
        /// Refresh grid. Set reuseTerrainData to true to avoid computation of terrain heights and slope (useful if terrain is not changed).
        /// </summary>
        public void Redraw(bool reuseTerrainData)
        {
            if (!gameObject.activeInHierarchy || redrawing)
                return;
            redrawing = true;
            if (issueRedraw == RedrawType.IncrementalTerritories)
            {
                DestroySurfacesDirty();
            }
            else
            {
                // Initialize surface cache
                List<GameObject> cached = new List<GameObject>(surfaces.Values);
                int cachedCount = cached.Count;
                for (int k = 0; k < cachedCount; k++)
                {
                    if (cached[k] != null)
                    {
                        DestroyImmediate(cached[k]);
                    }
                }
                DestroySurfaces();
            }
            ClearLastOver();
            if (UpdateTerrainReference(reuseTerrainData))
            {
                if (issueRedraw != RedrawType.IncrementalTerritories)
                {
                    refreshCellMesh = true;
                    _lastVertexCount = 0;
                    ComputeGridScale();
                    CheckCells();
                    if (_showCells)
                    {
                        DrawCellBorders();
                    }
                    DrawColorizedCells();
                }
                refreshTerritoriesMesh = true;
                CheckTerritories();
                if (_showTerritories)
                {
                    DrawTerritoryFrontiers();
                }
                if (_colorizeTerritories)
                {
                    DrawColorizedTerritories();
                }
                UpdateMaterialDepthOffset();
                UpdateMaterialNearClipFade();
                UpdateMaterialFarFade();
                UpdateHighlightEffect();
            }

            if (issueRedraw == RedrawType.IncrementalTerritories)
            {
                int territoryCount = territories.Count;
                for (int k = 0; k < territoryCount; k++)
                {
                    Territory territory = territories[k];
                    territory.isDirty = false;
                }
            }
            issueRedraw = RedrawType.None;
            redrawing = false;
        }
        /// <summary>
        /// Hides any color/texture from all cells and territories
        /// </summary>
        public void HideAll()
        {
            foreach (GameObject go in surfaces.Values)
            {
                if (go != null)
                {
                    go.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Removes any color/texture from all cells and territories
        /// </summary>
        public void ClearAll()
        {
            if (cells != null)
            {
                int cellsCount = cells.Count;
                for (int k = 0; k < cellsCount; k++)
                {
                    if (cells[k] != null && cells[k].region != null)
                    {
                        cells[k].region.customMaterial = null;
                    }
                }
            }
            if (territories != null)
            {
                int territoriesCount = territories.Count;
                for (int k = 0; k < territoriesCount; k++)
                {
                    Territory territory = territories[k];
                    for (int r = 0; r < territory.regions.Count; r++)
                    {
                        Region region = territory.regions[r];
                        if (region != null)
                        {
                            region.customMaterial = null;
                        }
                    }
                }
            }
            DestroySurfaces();
            issueRedraw = RedrawType.Full;
        }
        #endregion
    }
}