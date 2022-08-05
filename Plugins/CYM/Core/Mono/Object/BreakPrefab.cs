//------------------------------------------------------------------------------
// BreakPrefab.cs
// Created by CYM on 2022/3/23
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    [ExecuteInEditMode]
    public class BreakPrefab : MonoBehaviour
    {
        private void Awake()
        {
#if UNITY_EDITOR
            UnityEditor.PrefabUtility.UnpackPrefabInstance(gameObject, UnityEditor.PrefabUnpackMode.Completely, UnityEditor.InteractionMode.AutomatedAction);
#endif
        }
    }
}