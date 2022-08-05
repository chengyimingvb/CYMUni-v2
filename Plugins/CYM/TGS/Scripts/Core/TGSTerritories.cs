using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace CYM.TGS {

    public partial class TGS : MonoBehaviour {

        [NonSerialized]
        public List<Territory> territories;
        public Texture2D territoriesTexture;
        public Color territoriesTextureNeutralColor;
        public bool territoriesHideNeutralCells;
        [SerializeField]
        int _numTerritories = 3;
        [SerializeField]
        bool _showTerritories = false;
        [SerializeField]
        bool _colorizeTerritories = false;
        [SerializeField]
        float _colorizedTerritoriesAlpha = 0.7f;
        [SerializeField]
        Color _territoryHighlightColor = new Color(1, 0, 0, 0.8f);
        [SerializeField]
        Color _territoryHighlightColor2 = new Color(0, 1, 0, 0.8f);
        [SerializeField]
        Color _territoryFrontierColor = new Color(0, 1, 0, 1.0f);
        [SerializeField]
        float _territoryFrontiersThickness = 1f;
        [SerializeField]
        Color _territoryDisputedFrontierColor = new Color(0, 1, 0, 1.0f);
        [SerializeField]
        bool _showTerritoriesOuterBorder = true;
        [SerializeField]
        bool _allowTerritoriesInsideTerritories;

        #region prop
        /// <summary>
        /// Gets or sets the number of territories.
        /// </summary>
        public int numTerritories {
            get { return _numTerritories; }
            set {
                if (_numTerritories != value) {
                    _numTerritories = Mathf.Clamp(value, 1, MAX_TERRITORIES);
                    needGenerateMap = true;
                    isDirty = true;
                }
            }
        }
        /// <summary>
        /// Toggle frontiers visibility.
        /// </summary>
        public bool showTerritories {
            get {
                return _showTerritories;
            }
            set {
                if (value != _showTerritories) {
                    _showTerritories = value;
                    isDirty = true;
                    if (territoryLayer != null) {
                        territoryLayer.SetActive(_showTerritories);
                        ClearLastOver();
                    } else {
                        //Redraw();
                    }
                }
            }
        }
        /// <summary>
        /// Toggle colorize countries.
        /// </summary>
        public bool colorizeTerritories {
            get {
                return _colorizeTerritories;
            }
            set {
                if (value != _colorizeTerritories) {
                    _colorizeTerritories = value;
                    isDirty = true;
                    if (!_colorizeTerritories && surfacesLayer != null) {
                        DestroyTerritorySurfaces();
                    } else {
                        //Redraw();
                    }
                }
            }
        }
        public float colorizedTerritoriesAlpha {
            get { return _colorizedTerritoriesAlpha; }
            set {
                if (_colorizedTerritoriesAlpha != value) {
                    _colorizedTerritoriesAlpha = value;
                    isDirty = true;
                    UpdateColorizedTerritoriesAlpha();
                }
            }
        }
        /// <summary>
        /// Fill color to use when the mouse hovers a territory's region.
        /// </summary>
        public Color territoryHighlightColor {
            get {
                return _territoryHighlightColor;
            }
            set {
                if (value != _territoryHighlightColor) {
                    _territoryHighlightColor = value;
                    isDirty = true;
                    if (hudMatTerritoryOverlay != null && _territoryHighlightColor != hudMatTerritoryOverlay.color) {
                        hudMatTerritoryOverlay.color = _territoryHighlightColor;
                    }
                    if (hudMatTerritoryGround != null && _territoryHighlightColor != hudMatTerritoryGround.color) {
                        hudMatTerritoryGround.color = _territoryHighlightColor;
                    }
                }
            }
        }
        /// <summary>
        /// Alternate fill color to use when the mouse hovers a territory's region.
        /// </summary>
        public Color territoryHighlightColor2 {
            get {
                return _territoryHighlightColor2;
            }
            set {
                if (value != _territoryHighlightColor2) {
                    _territoryHighlightColor2 = value;
                    isDirty = true;
                    if (hudMatTerritoryOverlay != null) {
                        hudMatTerritoryOverlay.SetColor(TGSConst.Color2, _territoryHighlightColor2);
                    }
                    if (hudMatTerritoryGround != null) {
                        hudMatTerritoryGround.SetColor(TGSConst.Color2, _territoryHighlightColor2);
                    }
                }
            }
        }
        /// <summary>
        /// Territories border color
        /// </summary>
        public Color territoryFrontiersColor {
            get {
                if (territoriesMat != null) {
                    return territoriesMat.color;
                } else {
                    return _territoryFrontierColor;
                }
            }
            set {
                if (value != _territoryFrontierColor) {
                    _territoryFrontierColor = value;
                    isDirty = true;
                    if (territoriesThinMat != null && _territoryFrontierColor != territoriesThinMat.color) {
                        territoriesThinMat.color = _territoryFrontierColor;
                    }
                    if (territoriesGeoMat != null && _territoryFrontierColor != territoriesGeoMat.color) {
                        territoriesGeoMat.color = _territoryFrontierColor;
                    }
                }
            }
        }
        public float territoryFrontiersAlpha {
            get {
                return _territoryFrontierColor.a;
            }
            set {
                if (_territoryFrontierColor.a != value) {
                    _territoryFrontierColor = new Color(_territoryFrontierColor.r, _territoryFrontierColor.g, _territoryFrontierColor.b, value);
                }
                if (_territoryDisputedFrontierColor.a != value) {
                    _territoryDisputedFrontierColor = new Color(_territoryDisputedFrontierColor.r, _territoryDisputedFrontierColor.g, _territoryDisputedFrontierColor.b, value);
                }
            }
        }
        /// <summary>
        /// Territory frontier thickness
        /// </summary>
        public float territoryFrontiersThickness {
            get {
                return _territoryFrontiersThickness;
            }
            set {
                if (value != _territoryFrontiersThickness) {
                    _territoryFrontiersThickness = Mathf.Max(1f, value);
                    if (_showTerritories) {
                        DrawTerritoryFrontiers();
                    }
                    isDirty = true;
                }
            }
        }
        /// <summary>
        /// Territories disputed borders color
        /// </summary>
        public Color territoryDisputedFrontierColor {
            get {
                if (territoriesDisputedMat != null) {
                    return territoriesDisputedMat.color;
                } else {
                    return _territoryDisputedFrontierColor;
                }
            }
            set {
                if (value != _territoryDisputedFrontierColor) {
                    _territoryDisputedFrontierColor = value;
                    isDirty = true;
                    if (territoriesDisputedThinMat != null && _territoryDisputedFrontierColor != territoriesDisputedThinMat.color) {
                        territoriesDisputedThinMat.color = _territoryDisputedFrontierColor;
                    }
                    if (territoriesDisputedGeoMat != null && _territoryDisputedFrontierColor != territoriesDisputedGeoMat.color) {
                        territoriesDisputedGeoMat.color = _territoryDisputedFrontierColor;
                    }
                }
            }
        }
        /// <summary>
        /// Shows perimetral/outer border of territories?
        /// </summary>
        /// <value><c>true</c> if show territories outer borders; otherwise, <c>false</c>.</value>
        public bool showTerritoriesOuterBorders {
            get { return _showTerritoriesOuterBorder; }
            set {
                if (_showTerritoriesOuterBorder != value) {
                    _showTerritoriesOuterBorder = value;
                    isDirty = true;
                    //Redraw();
                }
            }
        }
        /// <summary>
        /// Set this property to true to allow territories to be surrounded by other territories.
        /// </summary>
        public bool allowTerritoriesInsideTerritories {
            get { return _allowTerritoriesInsideTerritories; }
            set {
                if (_allowTerritoriesInsideTerritories != value) {
                    _allowTerritoriesInsideTerritories = value;
                    isDirty = true;
                }
            }
        }
        /// <summary>
        /// Returns Territory under mouse position or null if none.
        /// </summary>
        public Territory territoryHighlighted { get { return _territoryHighlighted; } }
        /// <summary>
        /// Returns currently highlighted territory index in the countries list.
        /// </summary>
        public int territoryHighlightedIndex { get { return _territoryHighlightedIndex; } }
        /// <summary>
        /// Returns currently highlighted territory region index.
        /// </summary>
        public int territoryHighlightedRegionIndex { get { return _territoryHighlightedRegionIndex; } }
        /// <summary>
        /// Returns Territory index which has been clicked
        /// </summary>
        public int territoryLastClickedIndex { get { return _territoryLastClickedIndex; } }
        /// <summary>
        /// Returns Territory region index which has been clicked
        /// </summary>
        public int territoryRegionLastClickedIndex { get { return _territoryRegionLastClickedIndex; } }
        #endregion

        #region Public Territories Functions
        /// <summary>
        /// Uncolorize/hide all territories.
        /// </summary>
        public void TerritoryHideRegionSurfaces() {
            if (territories == null)
                return;
            int terrCount = territories.Count;
            for (int k = 0; k < terrCount; k++) {
                TerritoryHideRegionSurface(k);
            }
        }
        /// <summary>
        /// Uncolorize/hide specified territory by index in the territories collection.
        /// </summary>
        public void TerritoryHideRegionSurface(int territoryIndex, int regionIndex = 0) {
            if (!ValidTerritoryIndex(territoryIndex, regionIndex)) return;
            if ((_territoryHighlightedIndex != territoryIndex && _territoryHighlightedRegionIndex != regionIndex) || _highlightedObj == null) {
                int cacheIndex = GetCacheIndexForTerritoryRegion(territoryIndex, regionIndex);
                GameObject surf;
                if (surfaces.TryGetValue(cacheIndex, out surf)) {
                    if (surf == null) {
                        surfaces.Remove(cacheIndex);
                    } else {
                        surf.SetActive(false);
                    }
                }
            }
            territories[territoryIndex].regions[regionIndex].customMaterial = null;
        }
        /// <summary>
        /// Colorize specified territory by index.
        /// </summary>
        /// <param name="territoryIndex">Territory index.</param>
        /// <param name="visible">If the colored surface will be visible or not.</param>
        /// <param name="color">Color.</param>
        public GameObject TerritoryToggleRegionSurface(int territoryIndex, bool visible, Color color, bool refreshGeometry = false, int regionIndex = 0) {
            return TerritoryToggleRegionSurface(territoryIndex, visible, color, refreshGeometry, null, TGSConst.Vector2one, TGSConst.Vector2zero, 0, false, regionIndex);
        }
        /// <summary>
        /// Colorize specified territory by index.
        /// </summary>
        /// <param name="territoryIndex">Territory index.</param>
        /// <param name="visible">If the colored surface will be visible or not.</param>
        /// <param name="color">Color.</param>
        /// <param name="refreshGeometry">If set to <c>true</c> any cached surface will be destroyed and regenerated. Usually you pass false to improve performance.</param>
        /// <param name="texture">Texture, which will be tinted according to the color. Use Color.white to preserve original texture colors.</param>
        public GameObject TerritoryToggleRegionSurface(int territoryIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture, int regionIndex = 0) {
            return TerritoryToggleRegionSurface(territoryIndex, visible, color, refreshGeometry, texture, TGSConst.Vector2one, TGSConst.Vector2zero, 0, false, regionIndex);
        }
        /// <summary>
        /// Colorize specified territory by index.
        /// </summary>
        /// <param name="territoryIndex">Territory index.</param>
        /// <param name="visible">If the colored surface will be visible or not.</param>
        /// <param name="color">Color.</param>
        /// <param name="refreshGeometry">If set to <c>true</c> any cached surface will be destroyed and regenerated. Usually you pass false to improve performance.</param>
        /// <param name="texture">Texture, which will be tinted according to the color. Use Color.white to preserve original texture colors.</param>
        /// <param name="textureScale">Texture scale.</param>
        /// <param name="textureOffset">Texture offset.</param>
        /// <param name="textureRotation">Texture rotation.</param>
        public GameObject TerritoryToggleRegionSurface(int territoryIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool rotateInLocalSpace, int regionIndex = 0) {
            return TerritoryToggleRegionSurface(territoryIndex, visible, color, refreshGeometry, texture, textureScale, textureOffset, textureRotation, false, rotateInLocalSpace, regionIndex);
        }
        /// <summary>
        /// Colorize specified territory by index.
        /// </summary>
        /// <param name="territoryIndex">Territory index.</param>
        /// <param name="visible">If the colored surface will be visible or not.</param>
        /// <param name="color">Color.</param>
        /// <param name="refreshGeometry">If set to <c>true</c> any cached surface will be destroyed and regenerated. Usually you pass false to improve performance.</param>
        /// <param name="texture">Texture, which will be tinted according to the color. Use Color.white to preserve original texture colors.</param>
        /// <param name="textureScale">Texture scale.</param>
        /// <param name="textureOffset">Texture offset.</param>
        /// <param name="textureRotation">Texture rotation.</param>
        /// <param name="overlay">If set to <c>true</c> the colored surface will be shown over any object.</param>
        public GameObject TerritoryToggleRegionSurface(int territoryIndex, bool visible, Color color, bool refreshGeometry, Texture2D texture, Vector2 textureScale, Vector2 textureOffset, float textureRotation, bool overlay, bool rotateInLocalSpace, int regionIndex = 0) {

            if (!ValidTerritoryIndex(territoryIndex, regionIndex))
                return null;

            if (!visible) {
                TerritoryHideRegionSurface(territoryIndex);
                return null;
            }

            //if (needGenerateMap || needResortCells || issueRedraw != RedrawType.None) {
            //    CheckChanges();
            //}

            Region region = territories[territoryIndex].regions[regionIndex];
            int cacheIndex = GetCacheIndexForTerritoryRegion(territoryIndex, regionIndex);

            GameObject surf;
            // Checks if current cached surface contains a material with a texture, if it exists but it has not texture, destroy it to recreate with uv mappings
            bool existsInCache = surfaces.TryGetValue(cacheIndex, out surf);
            if (existsInCache && surf == null) {
                surfaces.Remove(cacheIndex);
                existsInCache = false;
            }
            if (refreshGeometry && existsInCache) {
                surfaces.Remove(cacheIndex);
                DestroyImmediate(surf);
                existsInCache = false;
                surf = null;
            }

            Material coloredMat = overlay ? coloredMatOverlayTerritory : coloredMatGroundTerritory;
            Material texturizedMat = overlay ? texturizedMatOverlayTerritory : texturizedMatGroundTerritory;

            // Should the surface be recreated?
            Material surfMaterial;
            if (surf != null) {
                surfMaterial = surf.GetComponent<Renderer>().sharedMaterial;
                if (texture != null && (region.customMaterial == null || textureScale != region.customTextureScale || textureOffset != region.customTextureOffset ||
                    textureRotation != region.customTextureRotation || !region.customMaterial.name.Equals(texturizedMat.name))) {
                    surfaces.Remove(cacheIndex);
                    DestroyImmediate(surf);
                    surf = null;
                }
            }
            // If it exists, activate and check proper material, if not create surface
            bool isHighlighted = territoryHighlightedIndex == territoryIndex;
            Territory territory = territories[territoryIndex];
            if (surf != null) {
                if (!surf.activeSelf) {
                    surf.SetActive(true);
                }
                // Check if material is ok
                Renderer renderer = surf.GetComponent<Renderer>();
                surfMaterial = renderer.sharedMaterial;
                if ((texture == null && !surfMaterial.name.Equals(coloredMat.name)) || (texture != null && !surfMaterial.name.Equals(texturizedMat.name))
                    || (surfMaterial.color != color && !isHighlighted) || (texture != null && (region.customMaterial == null || region.customMaterial.mainTexture != texture))) {
                    Material goodMaterial = GetColoredTexturedMaterialForTerritory(region, color, texture, overlay);
                    region.customMaterial = goodMaterial;
                    ApplyMaterialToSurface(region, goodMaterial);
                }
            } else {
                surfMaterial = GetColoredTexturedMaterialForTerritory(region, color, texture, overlay);
                surf = GenerateTerritoryRegionSurface(territoryIndex, surfMaterial, textureScale, textureOffset, textureRotation, rotateInLocalSpace, regionIndex);
                region.customMaterial = surfMaterial;
                region.customTextureOffset = textureOffset;
                region.customTextureRotation = textureRotation;
                region.customTextureScale = textureScale;
                region.customRotateInLocalSpace = rotateInLocalSpace;
            }
            // If it was highlighted, highlight it again
            if (isHighlighted && region.customMaterial != null && _highlightedObj != null) {
                if (hudMatTerritory.HasProperty(TGSConst.MainTex)) {
                    if (region.customMaterial != null) {
                        hudMatTerritory.mainTexture = region.customMaterial.mainTexture;
                    } else {
                        hudMatTerritory.mainTexture = null;
                    }
                }
                surf.GetComponent<Renderer>().sharedMaterial = hudMatTerritory;
                _highlightedObj = surf;
            }
            return surf;
        }
        /// <summary>
        /// Specifies if a given cell is visible.
        /// </summary>
        public void TerritorySetBorderVisible(int territoryIndex, bool visible) {
            if (territoryIndex < 0 || territoryIndex >= territories.Count)
                return;
            territories[territoryIndex].borderVisible = visible;
        }
        /// <summary>
        /// Returns a list of neighbour territories for specificed cell index.
        /// </summary>
        public List<Territory> TerritoryGetNeighbours(int territoryIndex) {
            if (!ValidTerritoryIndex(territoryIndex)) return null;
            return territories[territoryIndex].neighbours;
        }
        /// <summary>
        /// Returns a list of neighbour territories for specificed territory index in the territories parameter.
        /// </summary>
        public int TerritoryGetNeighbours(int territoryIndex, List<Territory> territories) {
            if (!ValidTerritoryIndex(territoryIndex) || territories == null) return 0;
            territories.Clear();
            territories.AddRange(territories[territoryIndex].neighbours);
            return territories.Count;
        }
        /// <summary>
        /// Returns a list of cells that form the frontiers of a given territory
        /// </summary>
        /// <returns>The number of cells found.</returns>
        /// <param name="territoryIndex">Territory index.</param>
        /// <param name="cellIndices">Cells that form the frontier. You need to pass an already initialized list, which will be cleared and filled with the cells.</param>
        public int TerritoryGetFrontierCells(int territoryIndex, ref List<int> cellIndices) {
            return TerritoryGetFrontierCells(territoryIndex, -1, ref cellIndices);
        }
        /// <summary>
        /// Returns a copy of all cells belonging to a territory. Use Territory.cells to access the list without making a copy
        /// </summary>
        public List<Cell> TerritoryGetCells(int territoryIndex) {
            if (!ValidTerritoryIndex(territoryIndex)) return null;
            List<Cell> cells = new List<Cell>();
            Territory terr = territories[territoryIndex];
            if (terr.cells != null) {
                cells.AddRange(terr.cells);
            }
            return cells;
        }
        /// <summary>
        /// Returns all cells belonging to a territory into an user given list. See alto territory.cells list.
        /// </summary>
        public void TerritoryGetCells(int territoryIndex, List<Cell> cells) {
            if (!ValidTerritoryIndex(territoryIndex)) return;
            cells.Clear();
            Territory terr = territories[territoryIndex];
            if (terr.cells != null) {
                cells.AddRange(terr.cells);
            }
        }
        /// <summary>
        /// Returns all cells belonging to a territory region
        /// </summary>
        public List<Cell> TerritoryGetCells(int territoryIndex, int regionIndex) {
            List<Cell> cells = new List<Cell>();
            TerritoryGetCells(territoryIndex, regionIndex, cells);
            return cells;
        }
        /// <summary>
        /// Returns all cells belonging to a territory region into an user given cell list
        /// </summary>
        public void TerritoryGetCells(int territoryIndex, int regionIndex, List<Cell> cells) {
            if (!ValidTerritoryIndex(territoryIndex, regionIndex)) return;
            Territory territory = territories[territoryIndex];
            Region region = territory.regions[regionIndex];
            if (territory.isDirty || region.cells == null || region.cells.Count == 0) {
                ComputeTerritoryRegionCells(region);
            }
            cells.Clear();
            cells.AddRange(region.cells);
        }
        public void ComputeTerritoryRegionCells(Region region) {
            Territory territory = (Territory)region.entity;
            // Optimization: if territory only contains one region, make region.cells point to territory.cells
            if (territory.regions.Count == 1) {
                region.cells = territory.cells;
                return;
            }
            int cellsCount = territory.cells.Count;
            List<Cell> regionCells = new List<Cell>();
            for (int k = 0; k < cellsCount; k++) {
                Cell cell = territory.cells[k];
                if (region.Contains(cell.scaledCenter.x, cell.scaledCenter.y)) {
                    regionCells.Add(cell);
                }
            }
            region.cells = regionCells;
        }
        /// <summary>
        /// Returns a list of cells that forms the frontiers between a given territory and another one.
        /// </summary>
        /// <returns>The get frontier cells.</returns>
        /// <param name="territoryIndex">Territory index.</param>
        /// <param name="otherTerritoryIndex">Other territory index.</param>
        /// <param name="cellIndices">Cells that form the frontier. You need to pass an already initialized list, which will be cleared and filled with the cells.</param>
        /// <param name="regionIndex">Limit search to a given region. -1 means include all regions.</param>
        public int TerritoryGetFrontierCells(int territoryIndex, int otherTerritoryIndex, ref List<int> cellIndices, int regionIndex = -1) {
            if (territoryIndex < 0 || territories == null || territoryIndex >= territories.Count || territories[territoryIndex].cells == null || cells == null)
                return 0;

            List<Cell> regionCells = null;
            if (regionIndex >= 0) regionCells = TerritoryGetCells(territoryIndex, regionIndex);

            cellIndices.Clear();
            foreach (KeyValuePair<Segment, Frontier> kv in territoryNeighbourHit) {
                Segment segment = kv.Key;
                Frontier frontier = kv.Value;
                if (frontier.region1 == null || frontier.region2 == null)
                    continue;
                Cell cell1 = (Cell)frontier.region1.entity;
                Cell cell2 = (Cell)frontier.region2.entity;
                if (cell1.territoryIndex == territoryIndex && (otherTerritoryIndex < 0 || cell2.territoryIndex == otherTerritoryIndex)) {
                    if (!cellIndices.Contains(cell1.index) && (regionIndex < 0 || regionCells.Contains(cell1))) {
                        cellIndices.Add(cell1.index);
                    }
                } else if (cell2.territoryIndex == territoryIndex && (otherTerritoryIndex < 0 || cell1.territoryIndex == otherTerritoryIndex)) {
                    if (!cellIndices.Contains(cell2.index) && (regionIndex < 0 || regionCells.Contains(cell2))) {
                        cellIndices.Add(cell2.index);
                    }
                }
            }
            return cellIndices.Count;
        }
        /// <summary>
        /// Colors a territory and fades it out during "duration" in seconds.
        /// </summary>
        public void TerritoryFadeOut(int territoryIndex, Color color, float duration, int repetitions = 1) {
            TerritoryAnimate(FADER_STYLE.FadeOut, territoryIndex, color, duration, repetitions);
        }
        /// <summary>
        /// Flashes a territory with "color" and "duration" in seconds.
        /// </summary>
        public void TerritoryFlash(int territoryIndex, Color color, float duration, int repetitions = 1) {
            TerritoryAnimate(FADER_STYLE.Flash, territoryIndex, color, duration, repetitions);
        }
        /// <summary>
        /// Blinks a territory with "color" and "duration" in seconds.
        /// </summary>
        public void TerritoryBlink(int territoryIndex, Color color, float duration, int repetitions = 1) {
            TerritoryAnimate(FADER_STYLE.Blink, territoryIndex, color, duration, repetitions);
        }
        /// <summary>
        /// Temporarily colors a territory for "duration" in seconds.
        /// </summary>
        public void TerritoryColorTemp(int territoryIndex, Color color, float duration) {
            TerritoryAnimate(FADER_STYLE.ColorTemp, territoryIndex, color, duration, 1);
        }
        /// <summary>
        /// Cancels any ongoing visual effect on a territory
        /// </summary>
        /// <param name="cellIndex">Cell index.</param>
        public void TerritoryCancelAnimations(int territoryIndex, float fadeOutDuration = 0) {
            TerritoryCancelAnimation(territoryIndex, fadeOutDuration);
        }
        /// <summary>
        /// Specifies if a given territory is visible.
        /// </summary>
        public void TerritorySetVisible(int territoryIndex, bool visible) {
            if (!ValidTerritoryIndex(territoryIndex)) return;
            territories[territoryIndex].visible = visible;
            if (territoryIndex == _territoryLastOverIndex) {
                ClearLastOver();
            }
            needUpdateTerritories = true;
            issueRedraw = RedrawType.Full;
        }
        /// <summary>
        /// Returns true if territory is visible
        /// </summary>
        public bool TerritoryIsVisible(int territoryIndex) {
            if (territoryIndex < 0 || territoryIndex >= territories.Count)
                return false;
            return territories[territoryIndex].visible;
        }
        /// <summary>
        /// Specifies if a given territory is neutral.
        /// </summary>
        public void TerritorySetNeutral(int territoryIndex, bool neutral) {
            if (territoryIndex < 0 || territoryIndex >= territories.Count)
                return;
            territories[territoryIndex].neutral = neutral;
            needUpdateTerritories = true;
            issueRedraw = RedrawType.Full;
        }
        /// <summary>
        /// Returns true if territory is neutral
        /// </summary>
        public bool TerritoryIsNeutral(int territoryIndex) {
            if (territoryIndex < 0 || territoryIndex >= territories.Count)
                return false;
            return territories[territoryIndex].neutral;
        }
        /// <summary>
        /// Specifies the color of the territory borders.
        /// </summary>
        public void TerritorySetFrontierColor(int territoryIndex, Color color) {
            if (territoryIndex < 0 || territoryIndex >= territories.Count)
                return;
            Territory terr = territories[territoryIndex];
            if (terr.frontierColor != color) {
                terr.frontierColor = color;
                DrawTerritoryFrontiers();
            }
        }
        /// <summary>
        /// Creates a gameobject with the frontier for the given territory. Optionally, the frontier could be limited to those segments adjacent to another territory.
        /// </summary>
        /// <returns></returns>
        public GameObject TerritoryDrawFrontier(int territoryIndex, int adjacentTerritoryIndex = -1, Material material = null, Color color = default(Color)) {

            TerritoryMesh tm = new TerritoryMesh { territoryIndex = territoryIndex };

            if (!GenerateTerritoryMesh(tm, true, adjacentTerritoryIndex)) return null;

            if (material == null) {
                if (tm.territoryIndex < 0) {
                    material = territoriesDisputedMat;
                } else {
                    Color frontierColor = territories[tm.territoryIndex].frontierColor;
                    if (frontierColor.a == 0 && frontierColor.r == 0 && frontierColor.g == 0 && frontierColor.b == 0) {
                        material = territoriesMat;
                    } else {
                        material = GetFrontierColorMaterial(frontierColor);
                    }
                }
            }

            material = Instantiate(material);
            material.renderQueue--; // ensure it writes to stencil before normal territory material
            if (color != default(Color)) {
                material.color = color;
            }

            Transform root;
            root = transform.Find(TERRITORIES_CUSTOM_FRONTIERS_ROOT);
            if (root == null) {
                GameObject go = new GameObject(TERRITORIES_CUSTOM_FRONTIERS_ROOT);
                disposalManager.MarkForDisposal(go);
                go.layer = gameObject.layer;
                root = go.transform;
                root.SetParent(transform, false);
                root.localRotation = Quaternion.Euler(TGSConst.Vector3zero);
            }
            root.localPosition = new Vector3(0, 0, -0.001f * _gridMinElevationMultiplier);

            return DrawTerritoryFrontier(tm, material, root);
        }
        /// <summary>
        /// Returns the territory object under position in local coordinates
        /// </summary>
        public Territory TerritoryGetAtPosition(Vector2 localPosition) {
            return GetTerritoryAtPoint(localPosition, false);
        }
        /// <summary>
        /// Returns the territory object under position in local or worldSpace coordinates
        /// </summary>
        public Territory TerritoryGetAtPosition(Vector3 position, bool worldSpace) {
            return GetTerritoryAtPoint(position, worldSpace);
        }
        /// <summary>
        /// Gets the territory's center position in world space.
        /// </summary>
        public Vector3 TerritoryGetPosition(int territoryIndex) {
            if (territories == null || territoryIndex < 0 || territoryIndex >= territories.Count)
                return TGSConst.Vector3zero;
            Vector2 territoryGridCenter = territories[territoryIndex].scaledCenter;
            return GetWorldSpacePosition(territoryGridCenter);
        }
        /// <summary>
        /// Returns the rect enclosing the territory in world space
        /// </summary>
        public Bounds TerritoryGetRectWorldSpace(int territoryIndex, int regionIndex = 0) {
            if (!ValidTerritoryIndex(territoryIndex, regionIndex))
                return new Bounds(TGSConst.Vector3zero, TGSConst.Vector3zero);
            Rect rect = territories[territoryIndex].regions[regionIndex].rect2D;
            Vector3 min = GetWorldSpacePosition(rect.min);
            Vector3 max = GetWorldSpacePosition(rect.max);
            Bounds bounds = new Bounds((min + max) * 0.5f, max - min);
            return bounds;
        }
        /// <summary>
        /// Returns the number of vertices of the territory
        /// </summary>
        public int TerritoryGetVertexCount(int territoryIndex, int regionIndex = 0) {
            if (!ValidTerritoryIndex(territoryIndex, regionIndex))
                return 0;
            return territories[territoryIndex].regions[regionIndex].points.Count;
        }
        /// <summary>
        /// Returns the world space position of the vertex of a territory
        /// </summary>
        public Vector3 TerritoryGetVertexPosition(int territoryIndex, int vertexIndex, int regionIndex = 0) {
            if (!ValidTerritoryIndex(territoryIndex, regionIndex))
                return TGSConst.Vector3zero;
            Vector2 localPosition = territories[territoryIndex].regions[regionIndex].points[vertexIndex];
            return GetWorldSpacePosition(localPosition);
        }
        /// <summary>
        /// Returns the shape/surface gameobject of the territory.
        /// </summary>
        /// <returns>The get game object.</returns>
        /// <param name="cellIndex">Cell index.</param>
        public GameObject TerritoryGetGameObject(int territoryIndex, int regionIndex) {
            if (!ValidTerritoryIndex(territoryIndex, regionIndex))
                return null;
            Territory territory = territories[territoryIndex];
            if (territory.regions[regionIndex].surfaceGameObject != null)
                return territory.regions[regionIndex].surfaceGameObject;
            GameObject go = TerritoryToggleRegionSurface(territoryIndex, true, TGSConst.ColorNull);
            TerritoryToggleRegionSurface(territoryIndex, false, TGSConst.ColorNull);
            return go;
        }
        /// <summary>
        /// Scales the gameobject of a colored/textured surface
        /// </summary>
        public void TerritoryScaleSurface(int territoryIndex, float scale, int regionIndex = 0) {
            if (!ValidTerritoryIndex(territoryIndex, regionIndex)) return;
            Territory territory = territories[territoryIndex];
            GameObject surf = territory.regions[regionIndex].surfaceGameObject;
            ScaleSurface(surf, territory.center, scale);
        }
        public void ExportTerritoryMesh(int territoryIndex, int regionIndex = 0) {
            if (territoryIndex < 0 || territoryIndex >= territories.Count || regionIndex < 0 || regionIndex >= territories[territoryIndex].regions.Count) return;

            GameObject surf = TerritoryGetGameObject(territoryIndex, regionIndex);
            if (surf == null)
                return;
            MeshFilter mf = surf.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                return;

            Mesh mesh = mf.sharedMesh;
            if (mesh != null && (mesh.uv == null || mesh.uv.Length == 0)) {
                // forces mesh to have UV mappings
                Material oldMat = territories[territoryIndex].regions[regionIndex].customMaterial;
                Color color = oldMat != null ? oldMat.color : TGSConst.ColorNull;
                surf = TerritoryToggleRegionSurface(territoryIndex, true, color, true, Texture2D.whiteTexture);
                TerritoryToggleRegionSurface(territoryIndex, false, TGSConst.ColorNull);

                mf = surf.GetComponent<MeshFilter>();
                if (mf == null || mf.sharedMesh == null)
                    return;
            }

            mesh = Instantiate<Mesh>(mf.sharedMesh);
            mesh.name = "Territory " + territoryIndex;
            mesh.hideFlags = 0;

            GameObject newSurf = new GameObject("Copy of territory " + territoryIndex);
            newSurf.layer = gameObject.layer;
            newSurf.transform.position = transform.position;
            newSurf.transform.rotation = transform.rotation;
            newSurf.transform.localScale = transform.localScale;
            mf = newSurf.AddComponent<MeshFilter>();
            mf.mesh = mesh;
            MeshRenderer mr = newSurf.AddComponent<MeshRenderer>();
            mr.material = new Material(Shader.Find("Diffuse"));
        }
        #endregion
    }
}

