//------------------------------------------------------------------------------
// InspectorBaseGlobalMonoMgr.cs
// Copyright 2018 2018/6/1 
// Created by CYM on 2018/6/1
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using UnityEngine.EventSystems;
using GameAnalyticsSDK.Events;

namespace CYM
{
    [CustomEditor(typeof(BaseGlobal), true)]
    public class IPTBaseGlobal : OdinEditor
    {
        BaseGlobal BaseGlobal;

        protected override void OnEnable()
        {
            base.OnEnable();
            BaseGlobal = (BaseGlobal)target;
            BaseGlobal.transform.hideFlags = HideFlags.NotEditable;
            BaseGlobal.GetComponent<EventSystem>().hideFlags = HideFlags.NotEditable;
            BaseGlobal.GetComponent<StandaloneInputModule>().hideFlags = HideFlags.NotEditable;
            BaseGlobal.GetComponent<GA_SpecialEvents>().hideFlags = HideFlags.NotEditable;
            BaseGlobal.GetComponent<GameAnalyticsSDK.GameAnalytics>().hideFlags = HideFlags.NotEditable;
            SceneVisibilityManager.instance.EnablePicking(BaseGlobal.transform.gameObject, false);
            SceneView.RepaintAll();
        }

        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            if (BaseGlobal == null) 
                return;

            EditorGUILayout.BeginVertical();
            GlobalMonoMgr.ToggleUpdate = EditorGUILayout.Toggle("ToggleUpdate",GlobalMonoMgr.ToggleUpdate);
            GlobalMonoMgr.ToggleFixedUpdate = EditorGUILayout.Toggle("ToggleFixedUpdate",GlobalMonoMgr.ToggleFixedUpdate);
            GlobalMonoMgr.ToggleLateUpdate = EditorGUILayout.Toggle("ToggleLateUpdate",GlobalMonoMgr.ToggleLateUpdate);
            EditorGUILayout.EndVertical();

            UpdateUI("Normal", GlobalMonoMgr.Normal);
            UpdateUI("Unit", GlobalMonoMgr.Unit);
            UpdateUI("Global", GlobalMonoMgr.Global);
            UpdateUI("View", GlobalMonoMgr.View);
        }

        void UpdateUI(string tite, MonoUpdateData updateData)
        {
            if (Application.isPlaying)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(tite + " Update:", updateData.UpdateIns.Count.ToString());
                EditorGUILayout.LabelField(tite + " FixedUpdate:", updateData.FixedUpdateIns.Count.ToString());
                EditorGUILayout.LabelField(tite + " LateUpdate:", updateData.LateUpdateIns.Count.ToString());
                EditorGUILayout.LabelField(tite + " GUI:", updateData.GUIIns.Count.ToString());
                EditorGUILayout.EndVertical();
            }
        }
    }
}