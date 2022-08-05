/*
Copyright (c) 2020 Omar Duarte
Unauthorized copying of this file, via any medium is strictly prohibited.
Writen by Omar Duarte, 2020.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
using UnityEngine;
using UnityEditor;
using UnityEditor.ShortcutManagement;

namespace CYM.TransTool
{
    public abstract class RandomizeToolWindow : BaseToolWindow
    {
        protected TransformTools.RandomizeData _data = new TransformTools.RandomizeData();

        protected enum Attribute
        {
            POSITION,
            ROTATION,
            SCALE
        }
        protected Attribute _attribute = Attribute.POSITION;

        protected override void OnGUI()
        {
            base.OnGUI();
            
            EditorGUIUtility.labelWidth = 30;
            EditorGUIUtility.fieldWidth = 70;

            OnGUIValue();
            GUILayout.Space(8);
            using (new GUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                GUILayout.FlexibleSpace();
                EditorGUIUtility.labelWidth = 60;
                _data.multiplier = Mathf.Max(EditorGUILayout.FloatField("Multiplier:", _data.multiplier), 0f);
            }
            GUILayout.Space(8);
            using (new GUILayout.HorizontalScope())
            {
                var statusStyle = new GUIStyle(EditorStyles.label);
                GUILayout.Space(8);
                var statusMessage = "";
                if (SelectionManager.topLevelSelection.Length == 0)
                {
                    statusMessage = "No objects selected.";
                    GUILayout.Label(new GUIContent(Resources.Load<Texture2D>("Sprites/Warning")),
                        new GUIStyle() { alignment = TextAnchor.LowerLeft });
                }
                else
                {
                    statusMessage = SelectionManager.topLevelSelection.Length + " objects selected.";
                }
                GUILayout.Label(statusMessage, statusStyle);
                GUILayout.FlexibleSpace();
                using (new EditorGUI.DisabledGroupScope(SelectionManager.topLevelSelection.Length == 0))
                {
                    if (GUILayout.Button(new GUIContent("Randomize", string.Empty
#if UNITY_2019_1_OR_NEWER
                            + (this is RandomizePositionsWindow ? RandomizePositionsWindow.shortcut
                            : this is RandomizeRotationsWindow ? RandomizeRotationsWindow.shortcut 
                            : this is RandomizeScalesWindow ? RandomizeScalesWindow.shortcut : string.Empty)
#endif
                        ), EditorStyles.miniButtonRight)) Randomize();
                }
            }
        }

        protected virtual void OnGUIValue()
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) //X
            {
                using (var toggleGroup = new EditorGUILayout.ToggleGroupScope("Randomize X", _data.x.randomizeAxis))
                {
                    _data.x.randomizeAxis = toggleGroup.enabled;
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Between:");
                        _data.x.offset.v1 = EditorGUILayout.FloatField(_data.x.offset.v1,
                            EditorStyles.textField);
                        GUILayout.Space(8);
                        _data.x.offset.v2 = EditorGUILayout.FloatField(_data.x.offset.v2,
                            EditorStyles.numberField);
                    }
                }
            }
            GUILayout.Space(8);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) //Y
            {
                using (var toggleGroup = new EditorGUILayout.ToggleGroupScope("Randomize Y", _data.y.randomizeAxis))
                {
                    _data.y.randomizeAxis = toggleGroup.enabled;
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Between:");
                        _data.y.offset.v1 = EditorGUILayout.FloatField(_data.y.offset.v1,
                            EditorStyles.textField);
                        GUILayout.Space(8);
                        _data.y.offset.v2 = EditorGUILayout.FloatField(_data.y.offset.v2,
                            EditorStyles.numberField);
                    }
                }
            }
            GUILayout.Space(8);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) //Z
            {
                using (var toggleGroup = new EditorGUILayout.ToggleGroupScope("Randomize Z", _data.z.randomizeAxis))
                {
                    _data.z.randomizeAxis = toggleGroup.enabled;
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Between:");
                        _data.z.offset.v1 = EditorGUILayout.FloatField(_data.z.offset.v1,
                            EditorStyles.textField);
                        GUILayout.Space(8);
                        _data.z.offset.v2 = EditorGUILayout.FloatField(_data.z.offset.v2,
                            EditorStyles.numberField);
                    }
                }
            }
        }

        protected abstract void Randomize();
    }

    public class RandomizePositionsWindow : RandomizeToolWindow
    {
        [MenuItem("Tools/Transform Tools/Randomize Positions", false, 1400)]
        public static void ShowWindow() => GetWindow<RandomizePositionsWindow>();

#if UNITY_2019_1_OR_NEWER
        public const string SHORTCUT_ID = "Transform Tools/Randomize Positions";
        [Shortcut(SHORTCUT_ID)]
        public static void RandomizePositions() => GetWindow<RandomizePositionsWindow>().Randomize();
        public static string shortcut => ShortcutManager.instance.GetShortcutBinding(SHORTCUT_ID).ToString();
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("Randomize Positions");
            _attribute = RandomizeToolWindow.Attribute.POSITION;
            _data.z.offset.v1 = _data.y.offset.v1 = _data.x.offset.v1 = -1f;
            _data.z.offset.v2 = _data.y.offset.v2 = _data.x.offset.v2 = 1f;
            minSize = new Vector2(240, 220);
        }

        protected override void Randomize()
            => TransformTools.RandomizePositions(SelectionManager.topLevelSelection,_data);
    }

    public class RandomizeRotationsWindow : RandomizeToolWindow
    {
        [MenuItem("Tools/Transform Tools/Randomize Rotations", false, 1400)]
        public static void ShowWindow() => GetWindow<RandomizeRotationsWindow>();

#if UNITY_2019_1_OR_NEWER
        public const string SHORTCUT_ID = "Transform Tools/Randomize Rotations";
        [Shortcut(SHORTCUT_ID)]
        public static void RandomizeRotations() => GetWindow<RandomizeRotationsWindow>().Randomize();
        public static string shortcut => ShortcutManager.instance.GetShortcutBinding(SHORTCUT_ID).ToString();
#endif

        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("Randomize Rotations");
            _attribute = RandomizeToolWindow.Attribute.ROTATION;
            _data.z.offset.v1 = _data.y.offset.v1 = _data.x.offset.v1 = -180f;
            _data.z.offset.v2 = _data.y.offset.v2 = _data.x.offset.v2 = 180f;
            minSize = new Vector2(240, 220);
        }

        protected override void Randomize()
        {
            TransformTools.RandomizeRotations(SelectionManager.topLevelSelection, _data);
        }
    }

    public class RandomizeScalesWindow : RandomizeToolWindow
    {
        private bool _separateAxes = false;

        [MenuItem("Tools/Transform Tools/Randomize Scales", false, 1400)]
        public static void ShowWindow() => GetWindow<RandomizeScalesWindow>();

#if UNITY_2019_1_OR_NEWER
        public const string SHORTCUT_ID = "Transform Tools/Randomize Scales";
        [Shortcut(SHORTCUT_ID)]
        public static void RandomizeScales() => GetWindow<RandomizeScalesWindow>().Randomize();
        public static string shortcut => ShortcutManager.instance.GetShortcutBinding(SHORTCUT_ID).ToString();
#endif
        protected override void OnEnable()
        {
            base.OnEnable();
            titleContent = new GUIContent("Randomize Scales");
            _attribute = RandomizeToolWindow.Attribute.SCALE;
            _data.z.offset.v1 = _data.y.offset.v1 = _data.x.offset.v1 = -0.1f;
            _data.z.offset.v2 = _data.y.offset.v2 = _data.x.offset.v2 = 0.1f;
        }

        protected override void OnGUIValue()
        {
            EditorGUIUtility.labelWidth = 90;
            _separateAxes = EditorGUILayout.Toggle("Separate Axes", _separateAxes);
            EditorGUIUtility.labelWidth = 30;

            if (_separateAxes)
            {
                minSize = new Vector2(240, 235);
                base.OnGUIValue();
            }
            else
            {
                minSize = new Vector2(240, 105);
                using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("Between:");
                        _data.x.offset.v1 = _data.y.offset.v1 = _data.z.offset.v1
                            = EditorGUILayout.FloatField(_data.x.offset.v1, EditorStyles.textField);
                        GUILayout.Space(8);
                        _data.x.offset.v2 = _data.y.offset.v2 = _data.z.offset.v2
                            = EditorGUILayout.FloatField("max:", _data.x.offset.v2, EditorStyles.numberField);

                    }
                }
            }
        }

        protected override void Randomize()
            => TransformTools.RandomizeScales(SelectionManager.topLevelSelection, _data, _separateAxes);
    }
}
