using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole.Core.Data
{    
    
    public class ProjectSettings : ScriptableObject
    {
        public bool JahroEnabled;

        public bool ShowLaunchButton;

        public bool UseLaunchKeyboardShortcut;

        public bool UseLaunchTapArea;

        public List<string> ActiveAssemblies;

        public static ProjectSettings CreateDefault()
        {
            var settings = ScriptableObject.CreateInstance<ProjectSettings>();
            settings.JahroEnabled = true;
            settings.ShowLaunchButton = false;
            settings.UseLaunchKeyboardShortcut = true;
            settings.UseLaunchTapArea = true; 
            settings.ActiveAssemblies = new List<string>();

#if UNITY_EDITOR
            var assemblies = UnityEditor.Compilation.CompilationPipeline.GetAssemblies(UnityEditor.Compilation.AssembliesType.Player);
            foreach(var assembly in assemblies)
            {
                settings.ActiveAssemblies.Add(assembly.name);
            }
#endif
            return settings;
        }
    }
}