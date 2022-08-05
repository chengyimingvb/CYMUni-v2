using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections.Generic;

namespace CYM.TGS
{
	[CustomEditor (typeof(TGS))]
	public class TGSInspector : Editor {

		TGS tgs;
		string[] selectionModeOptions, topologyOptions, overlayModeOptions;
		int[] topologyOptionsValues;
		GUIStyle titleLabelStyle, infoLabelStyle;
		int cellHighlightedIndex = -1;
		List <int> cellSelectedIndices;

		Color colorSelection, cellColor;
		SerializedProperty isDirty;
		Vector2 cellSize;

		void OnEnable () 
		{
			selectionModeOptions = new string[] {
				"None",
				"Territories",
				"Cells"
			};
			overlayModeOptions = new string[] { "Overlay", "Ground" };
			topologyOptions = new string[] { "Box", "Hexagonal" };
			topologyOptionsValues = new int[] {
				(int)GRID_TOPOLOGY.Box,
				(int)GRID_TOPOLOGY.Hexagonal
			};

			tgs = (TGS)target;
			if (tgs.cells == null) {
				tgs.Init ();
			}
			cellSelectedIndices = new List<int> ();
			colorSelection = new Color (1, 1, 0.5f, 0.85f);
			cellColor = Color.white;
			isDirty = serializedObject.FindProperty ("_isDirty");
			cellSize = tgs.cellSize;
			HideEditorMesh ();
		}

		public override void OnInspectorGUI () {
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();
			EditorGUILayout.BeginHorizontal ();
			DrawTitleLabel ("Grid Configuration");
			GUILayout.FlexibleSpace ();
			if (GUILayout.Button ("Redraw")) {
				tgs.Redraw (false);
                GUIUtility.ExitGUI();
			}
			if (GUILayout.Button ("Clear")) {
				if (EditorUtility.DisplayDialog ("Clear All", "Remove any color/texture from cells and territories?", "Ok", "Cancel")) {
					tgs.ClearAll ();
				}
			}
			EditorGUILayout.EndHorizontal ();
			EditorGUI.BeginChangeCheck ();
			tgs.terrainObject = (GameObject)EditorGUILayout.ObjectField ("Terrain", tgs.terrainObject, typeof(GameObject), true);
			if (EditorGUI.EndChangeCheck ()) {
				GUIUtility.ExitGUI ();
				return;
			}
			if (tgs.terrain != null) {
				if (tgs.terrain.supportsMultipleObjects) {
					tgs.terrainObjectsPrefix = EditorGUILayout.TextField (new GUIContent ("Terrain Name Prefix", "Use terrain gameobjects which has this prefix in their names."), tgs.terrainObjectsPrefix);
				}
				if (tgs.terrain.supportsCustomHeightmap) {
					tgs.heightmapSize = EditorGUILayout.IntField (new GUIContent ("Heightmap Size"), tgs.heightmapSize);
				}
			}
			tgs.gridTopology = (GRID_TOPOLOGY)EditorGUILayout.IntPopup ("Topology", (int)tgs.gridTopology, topologyOptions, topologyOptionsValues);
			EditorGUILayout.LabelField ("Territories", tgs.numTerritories.ToString());
			tgs.columnCount = EditorGUILayout.IntField("Columns", tgs.columnCount);
			tgs.rowCount = EditorGUILayout.IntField("Rows", tgs.rowCount);
			if (tgs.gridTopology == GRID_TOPOLOGY.Hexagonal)
			{
				tgs.regularHexagons = EditorGUILayout.Toggle("Regular Hexes", tgs.regularHexagons);
				if (tgs.regularHexagons)
				{
					tgs.regularHexagonsWidth = EditorGUILayout.FloatField("   Hex Width", tgs.regularHexagonsWidth);
				}
				tgs.evenLayout = EditorGUILayout.Toggle("Even Layout", tgs.evenLayout);
			}
			if (tgs.rowCount > TGS.MAX_ROWS_OR_COLUMNS || tgs.columnCount > TGS.MAX_ROWS_OR_COLUMNS) {
				EditorGUILayout.HelpBox ("Total row or column count exceeds recommended maximum of " + TGS.MAX_ROWS_OR_COLUMNS + "!", MessageType.Warning);
			}
			if (tgs.numCells > TGS.MAX_CELLS_FOR_CURVATURE)
			{
				EditorGUILayout.LabelField("Curvature", "Not available with >" + TGS.MAX_CELLS_FOR_CURVATURE + " cells");
			}
			else
			{
				tgs.gridCurvature = EditorGUILayout.Slider("Curvature", tgs.gridCurvature, 0, 0.1f);
			}
			if (tgs.numCells > TGS.MAX_CELLS_FOR_RELAXATION)
			{
				EditorGUILayout.LabelField("Relaxation", "Not available with >" + TGS.MAX_CELLS_FOR_RELAXATION + " cells");
			}
			else
			{
				tgs.gridRelaxation = EditorGUILayout.IntSlider("Relaxation", tgs.gridRelaxation, 1, 32);
			}
			if (tgs.terrain != null) {
				tgs.gridRoughness = EditorGUILayout.Slider ("Roughness", tgs.gridRoughness, 0f, 0.2f);
				tgs.cellsMaxSlope = EditorGUILayout.Slider ("Max Slope", tgs.cellsMaxSlope, 0, 1f);
			
				EditorGUILayout.BeginHorizontal ();
				tgs.cellsMinimumAltitude = EditorGUILayout.FloatField ("Minimum Altitude", tgs.cellsMinimumAltitude);
				if (tgs.cellsMinimumAltitude == 0)
					DrawInfoLabel ("(0 = not used)");
				EditorGUILayout.EndHorizontal ();
				if (tgs.cellsMinimumAltitude != 0) {
					tgs.cellsMinimumAltitudeClampVertices = EditorGUILayout.Toggle (new GUIContent ("   Clamp Vertices", "Clamp vertices altitude to the minimum altitude."), tgs.cellsMinimumAltitudeClampVertices);
				}

				EditorGUILayout.BeginHorizontal();
				tgs.cellsMaximumAltitude = EditorGUILayout.FloatField("Maximum Altitude", tgs.cellsMaximumAltitude);
				if (tgs.cellsMaximumAltitude == 0)
					DrawInfoLabel("(0 = not used)");
				EditorGUILayout.EndHorizontal();
				if (tgs.cellsMaximumAltitude != 0) {
					tgs.cellsMaximumAltitudeClampVertices = EditorGUILayout.Toggle(new GUIContent("   Clamp Vertices", "Clamp vertices altitude to the maximum altitude."), tgs.cellsMaximumAltitudeClampVertices);
				}
			}
			EditorGUILayout.BeginHorizontal ();
			tgs.territoriesTexture = (Texture2D)EditorGUILayout.ObjectField (new GUIContent ("Territories Texture", "Quickly create territories assigning a color texture in which each territory corresponds to a color."), tgs.territoriesTexture, typeof(Texture2D), true);
			if (tgs.territoriesTexture != null) {
				EditorGUILayout.EndHorizontal ();
				CheckTextureImportSettings (tgs.territoriesTexture);
				tgs.territoriesTextureNeutralColor = EditorGUILayout.ColorField (new GUIContent ("   Neutral Color", "Color to be ignored."), tgs.territoriesTextureNeutralColor, false, false, false);
				EditorGUILayout.BeginHorizontal ();
				tgs.territoriesHideNeutralCells = EditorGUILayout.Toggle (new GUIContent ("   Hide Neutral Cells", "Cells belonging to neutral territories will be invisible."), tgs.territoriesHideNeutralCells);
				EditorGUILayout.Space ();
				if (GUILayout.Button ("Generate Territories", GUILayout.Width (120))) {
					tgs.CreateTerritories (tgs.territoriesTexture, tgs.territoriesTextureNeutralColor, tgs.territoriesHideNeutralCells);
				}
			}

			EditorGUILayout.EndHorizontal ();
			int cellsCreated = tgs.cells == null ? 0 : tgs.cells.Count;
			int territoriesCreated = tgs.territories == null ? 0 : tgs.territories.Count;
			EditorGUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			DrawInfoLabel ("Cells Created: " + cellsCreated + " / Territories Created: " + territoriesCreated + " / Vertex Count: " + tgs.lastVertexCount);
			GUILayout.FlexibleSpace ();
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();
			DrawTitleLabel ("Grid Positioning");
			EditorGUILayout.BeginHorizontal ();
			GUILayout.Label ("Hide Objects", GUILayout.Width (EditorGUIUtility.labelWidth));
			if (tgs.terrain != null && GUILayout.Button ("Toggle Terrain")) {
				tgs.terrain.enabled = !tgs.terrain.enabled;
				tgs.transparentBackground = !tgs.terrain.enabled;
				if (tgs.transparentBackground && tgs.gridSurfaceDepthOffsetTerritory < 20) {
					tgs.gridSurfaceDepthOffsetTerritory = 20;
				} else if (!tgs.transparentBackground && tgs.gridSurfaceDepthOffsetTerritory > 0) {
					tgs.gridSurfaceDepthOffsetTerritory = -1;
				}
			}
			EditorGUILayout.EndHorizontal ();
            if (tgs.terrain != null && tgs.terrain.supportsPivot) {
                tgs.terrainMeshPivot = EditorGUILayout.Vector2Field(new GUIContent("Mesh Pivot", "Specify a center correction if mesh center is not at 0,0,0"), tgs.terrainMeshPivot);
            }
            tgs.gridCenter = EditorGUILayout.Vector2Field (new GUIContent("Center", "The position of the grid center."), tgs.gridCenter);
			if (tgs.gridTopology == GRID_TOPOLOGY.Hexagonal && tgs.regularHexagons) {
				GUI.enabled = false;
			}
			tgs.gridScale = EditorGUILayout.Vector2Field ("Scale", tgs.gridScale);
			GUI.enabled = true;
			if (tgs.gridTopology == GRID_TOPOLOGY.Hexagonal && tgs.regularHexagons) {
				EditorGUILayout.BeginHorizontal ();
				EditorGUILayout.Space ();
				EditorGUILayout.HelpBox ("Scale is driven by regular hexagons option.", MessageType.Info);
				EditorGUILayout.Space ();
				EditorGUILayout.EndHorizontal ();
				EditorGUILayout.Separator ();
			} 
			cellSize = EditorGUILayout.Vector2Field("Match Cell Size", cellSize);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label("", GUILayout.Width(EditorGUIUtility.labelWidth));
			if (GUILayout.Button("Update Cell Size"))
			{
				tgs.cellSize = cellSize;
			}
			EditorGUILayout.EndHorizontal();
			tgs.gridMeshDepthOffset = EditorGUILayout.IntField("Mesh Depth Offset", tgs.gridMeshDepthOffset);
			tgs.gridSurfaceDepthOffset = EditorGUILayout.IntField("Cells Depth Offset", tgs.gridSurfaceDepthOffset);
			tgs.gridSurfaceDepthOffsetTerritory = EditorGUILayout.IntField ("Territories Depth Offset", tgs.gridSurfaceDepthOffsetTerritory);
			if (tgs.terrain != null) {
				tgs.gridElevation = EditorGUILayout.Slider ("Elevation", tgs.gridElevation, 0f, 5f);
				tgs.gridElevationBase = EditorGUILayout.FloatField ("Elevation Base", tgs.gridElevationBase);
				tgs.gridMinElevationMultiplier = EditorGUILayout.FloatField (new GUIContent ("Min Elevation Multiplier", "Grid, cells and territories meshes are rendered with a minimum gap to preserve correct order. This value is the scale for that gap."), tgs.gridMinElevationMultiplier);
				tgs.gridCameraOffset = EditorGUILayout.Slider ("Camera Offset", tgs.gridCameraOffset, 0, 0.1f);
				tgs.gridNormalOffset = EditorGUILayout.Slider ("Normal Offset", tgs.gridNormalOffset, 0.00f, 5f);
			}
			tgs.cameraMain = (Camera)EditorGUILayout.ObjectField (new GUIContent ("Camera", "The camera used for some calculations. Main camera is picked by default."), tgs.cameraMain, typeof(Camera), true);
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();
			DrawTitleLabel ("Grid Appearance");
			tgs.showTerritories = EditorGUILayout.Toggle ("Show Territories", tgs.showTerritories);
			EditorGUI.indentLevel++;
			tgs.territoryFrontiersColor = EditorGUILayout.ColorField ("Frontier Color", tgs.territoryFrontiersColor);
			tgs.territoryFrontiersThickness = EditorGUILayout.FloatField("Thickness", tgs.territoryFrontiersThickness);
			tgs.territoryHighlightColor = EditorGUILayout.ColorField ("Highlight Color", tgs.territoryHighlightColor);
			tgs.territoryDisputedFrontierColor = EditorGUILayout.ColorField (new GUIContent ("Disputed Frontier", "Color for common frontiers between two territories."), tgs.territoryDisputedFrontierColor);
			tgs.colorizeTerritories = EditorGUILayout.Toggle ("Colorize Territories", tgs.colorizeTerritories);
			tgs.colorizedTerritoriesAlpha = EditorGUILayout.Slider ("Alpha", tgs.colorizedTerritoriesAlpha, 0.0f, 1.0f);
			tgs.showTerritoriesOuterBorders = EditorGUILayout.Toggle ("Outer Borders", tgs.showTerritoriesOuterBorders);
			tgs.allowTerritoriesInsideTerritories = EditorGUILayout.Toggle (new GUIContent ("Internal Territories", "Allows territories to be contained by other territories."), tgs.allowTerritoriesInsideTerritories);
			EditorGUI.indentLevel--;
			tgs.showCells = EditorGUILayout.Toggle ("Show Cells", tgs.showCells);
			EditorGUI.indentLevel++;
			if (tgs.showCells) {
				tgs.cellBorderColor = EditorGUILayout.ColorField ("Border Color", tgs.cellBorderColor);
				tgs.cellBorderThickness = EditorGUILayout.FloatField ("Thickness", tgs.cellBorderThickness);
			}
			tgs.cellHighlightColor = EditorGUILayout.ColorField ("Highlight Color", tgs.cellHighlightColor);
			tgs.cellFillPadding = EditorGUILayout.FloatField(new GUIContent("Fill Padding", "Padding or separation applied to cells when they're filled with color/texture."), tgs.cellFillPadding);
			EditorGUI.indentLevel--;
			float highlightFadeMin = tgs.highlightFadeMin;
			float highlightFadeAmount = tgs.highlightFadeAmount;
			EditorGUILayout.MinMaxSlider ("Highlight Fade", ref highlightFadeMin, ref highlightFadeAmount, 0.0f, 1.0f);
			EditorGUI.indentLevel++;
			tgs.highlightFadeMin = highlightFadeMin;
			tgs.highlightFadeAmount = highlightFadeAmount;
			tgs.highlightFadeSpeed = EditorGUILayout.Slider ("Highlight Speed", tgs.highlightFadeSpeed, 0.1f, 5.0f);
			tgs.highlightEffect = (HIGHLIGHT_EFFECT)EditorGUILayout.EnumPopup ("Highlight Effect", tgs.highlightEffect);
			if (tgs.highlightEffect == HIGHLIGHT_EFFECT.TextureScale) {
				EditorGUILayout.BeginHorizontal ();
				float highlightScaleMin = tgs.highlightScaleMin;
				float highlightScaleMax = tgs.highlightScaleMax;
				EditorGUILayout.MinMaxSlider ("      Scale Range", ref highlightScaleMin, ref highlightScaleMax, 0.0f, 2.0f);
				if (GUILayout.Button ("Default", GUILayout.Width (60))) {
					highlightScaleMin = 0.75f;
					highlightScaleMax = 1.1f;
				}
				tgs.highlightScaleMin = highlightScaleMin;
				tgs.highlightScaleMax = highlightScaleMax;
				EditorGUILayout.EndHorizontal ();
			} else if (tgs.highlightEffect == HIGHLIGHT_EFFECT.DualColors) {
				tgs.cellHighlightColor2 = EditorGUILayout.ColorField ("Cell Alternate Color", tgs.cellHighlightColor2);
				tgs.territoryHighlightColor2 = EditorGUILayout.ColorField ("Territory Alternate Color", tgs.territoryHighlightColor2);
			}
			EditorGUI.indentLevel--;
			if (tgs.terrain != null) {
				tgs.nearClipFadeEnabled = EditorGUILayout.Toggle (new GUIContent ("Near Clip Fade", "Fades out the cell and territories lines near to the camera."), tgs.nearClipFadeEnabled);
				if (tgs.nearClipFadeEnabled) {
					tgs.nearClipFade = EditorGUILayout.FloatField ("   Distance", tgs.nearClipFade);
					tgs.nearClipFadeFallOff = EditorGUILayout.FloatField ("   FallOff", tgs.nearClipFadeFallOff);
				}
				tgs.farFadeEnabled = EditorGUILayout.Toggle(new GUIContent("Far Distance Fade", "Fades out the cell and territories lines far from the camera."), tgs.farFadeEnabled);
				if (tgs.farFadeEnabled) {
					tgs.farFadeDistance = EditorGUILayout.FloatField("   Distance", tgs.farFadeDistance);
					tgs.farFadeFallOff = EditorGUILayout.FloatField("   FallOff", tgs.farFadeFallOff);
				}
			}
			tgs.circularFadeEnabled = EditorGUILayout.Toggle(new GUIContent("Circular Fade", "Fades out the cell and territories lines with respect to a gameobject position."), tgs.circularFadeEnabled);
			if (tgs.circularFadeEnabled) {
				tgs.circularFadeTarget = (Transform)EditorGUILayout.ObjectField("   Target", tgs.circularFadeTarget, typeof(Transform), true);
				tgs.circularFadeDistance = EditorGUILayout.FloatField("   Distance", tgs.circularFadeDistance);
				tgs.circularFadeFallOff = EditorGUILayout.FloatField("   FallOff", tgs.circularFadeFallOff);
			}
			tgs.useGeometryShaders = EditorGUILayout.Toggle(new GUIContent("Use Geometry Shaders", "Use geometry shaders if platform supports them."), tgs.useGeometryShaders);
			tgs.transparentBackground = EditorGUILayout.Toggle ("No Background", tgs.transparentBackground);
            if (tgs.transparentBackground) {
                tgs.sortingOrder = EditorGUILayout.IntField("Sorting Order", tgs.sortingOrder);
            }
            tgs.canvasTexture = (Texture2D)EditorGUILayout.ObjectField ("Canvas Texture", tgs.canvasTexture, typeof(Texture2D), true);
			EditorGUILayout.EndVertical ();
			EditorGUILayout.Separator ();
			EditorGUILayout.BeginVertical ();		
			DrawTitleLabel ("Grid Behaviour");
			tgs.highlightMode = (HIGHLIGHT_MODE)EditorGUILayout.Popup ("Selection Mode", (int)tgs.highlightMode, selectionModeOptions);
			EditorGUI.indentLevel++;
			tgs.cellHighlightNonVisible = EditorGUILayout.Toggle ("Include Invisible Cells", tgs.cellHighlightNonVisible);
			tgs.highlightMinimumTerrainDistance = EditorGUILayout.FloatField (new GUIContent ("Minimum Distance", "Minimum distance of cell/territory to camera to be selectable. Useful in first person view to prevent selecting cells already under character."), tgs.highlightMinimumTerrainDistance);
			tgs.allowHighlightWhileDragging = EditorGUILayout.Toggle (new GUIContent ("Highlight While Drag", "Allows highlight while dragging."), tgs.allowHighlightWhileDragging);
			EditorGUI.indentLevel--;
			tgs.overlayMode = (OVERLAY_MODE)EditorGUILayout.Popup ("Overlay Mode", (int)tgs.overlayMode, overlayModeOptions);
			tgs.respectOtherUI = EditorGUILayout.Toggle ("Respect Other UI", tgs.respectOtherUI);
			EditorGUILayout.EndVertical ();
			if (tgs.isDirty) {
				serializedObject.UpdateIfRequiredOrScript ();
				if (isDirty == null)
					OnEnable ();
				isDirty.boolValue = false;
				serializedObject.ApplyModifiedProperties ();
				EditorUtility.SetDirty (target);
				// Hide mesh in Editor
				HideEditorMesh ();
				SceneView.RepaintAll ();
			}
		}

		void OnSceneGUI () {
			if (tgs == null || Application.isPlaying || !tgs.enableGridEditor)
				return;
			if (tgs.terrain != null) {
				// prevents terrain from being selected
				HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));
			}
			Event e = Event.current;
			bool gridHit = tgs.CheckRay (HandleUtility.GUIPointToWorldRay (e.mousePosition));
			if (cellHighlightedIndex != tgs.cellHighlightedIndex) {
				cellHighlightedIndex = tgs.cellHighlightedIndex;
				SceneView.RepaintAll ();
			}
			int controlID = GUIUtility.GetControlID (FocusType.Passive);
			EventType eventType = e.GetTypeForControl (controlID);
			if ((eventType == EventType.MouseDown && e.button == 0) || (eventType == EventType.MouseMove && e.shift)) {
				if (gridHit) {
					e.Use ();
				}
				if (cellHighlightedIndex < 0) {
					return;
				}
				if (!e.shift && cellSelectedIndices.Contains (cellHighlightedIndex)) {
					cellSelectedIndices.Remove (cellHighlightedIndex);
				} else {
					if (!e.shift || (e.shift && !cellSelectedIndices.Contains (cellHighlightedIndex))) {
						if (!e.shift && !e.control) {
							cellSelectedIndices.Clear ();
						}
						cellSelectedIndices.Add (cellHighlightedIndex);
						if (cellHighlightedIndex >= 0) {
							cellColor = tgs.CellGetColor (cellHighlightedIndex);
							if (cellColor.a == 0)
								cellColor = Color.white;
						}
					}
				}
				EditorUtility.SetDirty (target);
			}
			int count = cellSelectedIndices.Count;
			for (int k = 0; k < count; k++) {
				int index = cellSelectedIndices [k];
				Vector3 pos = tgs.CellGetPosition (index);
				Handles.color = colorSelection;
				// Handle size
				Rect rect = tgs.CellGetRect (index);
				Vector3 min = tgs.transform.TransformPoint (rect.min);
				Vector3 max = tgs.transform.TransformPoint (rect.max);
				float dia = Vector3.Distance (min, max);
				float handleSize = dia * 0.05f;
				Handles.DrawSolidDisc (pos, tgs.transform.forward, handleSize);
			}
		}

		#region Utility functions
		void HideEditorMesh () {
			Renderer[] rr = tgs.GetComponentsInChildren<Renderer> (true);
			for (int k = 0; k < rr.Length; k++) {
				EditorUtility.SetSelectedRenderState (rr [k], EditorSelectedRenderState.Hidden);		
			}
		}
		void DrawTitleLabel (string s) {
			if (titleLabelStyle == null)
				titleLabelStyle = new GUIStyle (GUI.skin.label);
			titleLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? new Color (0.52f, 0.66f, 0.9f) : new Color (0.22f, 0.36f, 0.6f);
			titleLabelStyle.fontStyle = FontStyle.Bold;
			GUILayout.Label (s, titleLabelStyle);
		}
		void DrawInfoLabel (string s) {
			if (infoLabelStyle == null)
				infoLabelStyle = new GUIStyle (GUI.skin.label);
			infoLabelStyle.normal.textColor = new Color (0.76f, 0.52f, 0.52f);
			GUILayout.Label (s, infoLabelStyle);
		}
		bool CheckTextureImportSettings (Texture2D tex) {
			if (tex == null)
				return false;
			string path = AssetDatabase.GetAssetPath (tex);
			if (string.IsNullOrEmpty (path))
				return false;
			TextureImporter imp = (TextureImporter)AssetImporter.GetAtPath (path);
			if (imp != null && !imp.isReadable) {
				EditorGUILayout.HelpBox ("Texture is not readable. Fix it?", MessageType.Warning);
				if (GUILayout.Button ("Fix texture import setting")) {
					imp.isReadable = true;
					imp.SaveAndReimport ();
					return true;
				}
			}
			return false;
		}
		#endregion
	}

}