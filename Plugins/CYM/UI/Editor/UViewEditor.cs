//------------------------------------------------------------------------------
// UViewEditor.cs
// Created by CYM on 2021/9/18
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector.Editor;
using UnityEditor;

namespace CYM.UI
{
    [CustomEditor(typeof(UView))]
    [CanEditMultipleObjects]
    public class UViewEditor : OdinEditor
    {
        private UView targetView;
        protected override void OnEnable()
        {
            base.OnEnable();
            targetView = target as UView;
        }
        void OnSceneGUI()
        {
            //Tools.hidden = true;
            //if (!targetView.IsMove)
            //    return;
            //targetView.TweenMove.StartPos = Handles.PositionHandle(targetView.TweenMove.StartPos, Quaternion.identity);

            //if (Event.current.type == EventType.KeyDown)
            //{
            //    if (Event.current.control)
            //    {
            //    }
            //}
            //else if (Event.current.type == EventType.MouseDrag)
            //{
            //    Node node = target as Node;
            //    if (NodeMap.Ins.redrawThreshhold != 0f && Vector3.Distance(node.transform.position, lastDrawnPosition) > NodeMap.Ins.redrawThreshhold)
            //    {
            //        lastDrawnPosition = node.transform.position;
            //    }
            //}
        }
    }
}