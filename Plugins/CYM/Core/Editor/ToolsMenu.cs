//------------------------------------------------------------------------------
// ToolsMenu.cs
// Created by CYM on 2022/1/2
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using System;
using System.Reflection;

namespace CYM
{
   public class ToolsMenu : EditorWindow
   {
        [MenuItem("Tools/RefreshConfig")]
        static void RefreshConfig()
        {
            BuildWindow.RefreshInternalConfig();
            if (GameConfig.Ins != null)
            { 
            
            }
        }
        [MenuItem("Tools/Option  %#q")]
        static void ShowOptionWindow()
        {
            StaticInspectorWindow.InspectType(typeof(Options), StaticInspectorWindow.AccessModifierFlags.Public, StaticInspectorWindow.MemberTypeFlags.Groups);
        }
    }
}