
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JahroConsole.Core.Data;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace JahroConsole.Editor
{
    public class JahroSettingsProvider : EditorWindow
    {


        private ProjectSettings _projectSettings;

        private string[] _assembliesNames;

        private int _assembliesFlag;

        private Vector2 _scrollPosition;

        //[MenuItem("Tools/Jahro Settings")]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(JahroSettingsProvider), false, "Jahro Console Settings", true);
        }

        public void OnFocus()
        {
            _projectSettings = ConsoleStorageController.ReadOrCreateProjectSettings();

            if (_assembliesNames == null || _assembliesNames.Length == 0)
            {
                var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Player);
                _assembliesNames = new string[assemblies.Length];
                for (int i = 0; i < assemblies.Length; i++)
                {
                    _assembliesNames[i] = assemblies[i].name;
                }
            }
            _assembliesFlag = 0;
            for (int i = 0; i < _assembliesNames.Length; i++)
            {
                if (_projectSettings.ActiveAssemblies.Contains(_assembliesNames[i]))
                {
                    _assembliesFlag |= 1 << i;
                }
            }
        }

        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.Separator();
            GUILayout.Label("General", EditorStyles.boldLabel);
            EditorGUILayout.Separator();
            
            _projectSettings.JahroEnabled = EditorGUILayout.BeginToggleGroup("Enable Jahro", _projectSettings.JahroEnabled);
            
            EditorGUILayout.Separator();
            GUILayout.Label("Launch Options", EditorStyles.boldLabel);
            EditorGUILayout.Separator();

            _projectSettings.ShowLaunchButton = EditorGUILayout.Toggle("Launch button", _projectSettings.ShowLaunchButton);
            EditorGUILayout.HelpBox("Launch button is useful on mobile devices to get a quick access to console window.", MessageType.None, false);

            _projectSettings.UseLaunchKeyboardShortcut = EditorGUILayout.Toggle("Keyboard shortcuts", _projectSettings.UseLaunchKeyboardShortcut);
            EditorGUILayout.HelpBox("Shortkeys for Jahro Console Window:\n\n"
            + "Open/Close console - press '~' on keyboard\n"
            + "Switch to Console Mode - press 'Alt+1' on keyboard\n"
            + "Switch to Visual Mode - press 'Alt+2' on keyboard\n", MessageType.None, false);

            _projectSettings.UseLaunchTapArea = EditorGUILayout.Toggle("Tap Area", _projectSettings.UseLaunchTapArea);
            EditorGUILayout.HelpBox("You can always use API method to show/hide console", MessageType.None, false);

            EditorGUILayout.Separator();
            if (_assembliesNames != null)
            {
                GUILayout.Label("Assemblies", EditorStyles.boldLabel);

                _assembliesFlag = EditorGUILayout.MaskField("Included assemblies", _assembliesFlag, _assembliesNames);
                EditorGUILayout.HelpBox("Please select assemblies which contain Jahro Commands, others can be disabled.", MessageType.None, false);
                for (int i = 0; i < _assembliesNames.Length; i++)
                {
                    string name = _assembliesNames[i];
                    bool active = _projectSettings.ActiveAssemblies.Contains(name);
                    int layer = 1 << i;
                    bool selected = (_assembliesFlag & layer) != 0;
                    if (!active && selected)
                    {
                        _projectSettings.ActiveAssemblies.Add(name);
                    }
                    else if (active && !selected)
                    {
                        _projectSettings.ActiveAssemblies.Remove(name);
                    }
                }
            }
            EditorGUILayout.Separator();

            EditorGUILayout.EndToggleGroup();

            EditorGUILayout.EndScrollView();

            if (GUI.changed)
            {
                SaveChanges();
            }
        }

        private void SaveChanges()
        {
            
            EditorUtility.SetDirty(_projectSettings);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
