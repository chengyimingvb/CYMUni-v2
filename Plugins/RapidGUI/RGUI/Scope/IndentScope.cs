using UnityEngine;

namespace RapidGUI
{
    public partial class RGUI
    {
        public static void BeginIndent(float width = 32f)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(width);
            GUILayout.BeginVertical();
        }

        public static void EndIndent()
        {
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

        }
        public class IndentScope : GUI.Scope
        {
            public IndentScope(float width = 32f) => BeginIndent(width);

            protected override void CloseScope() => EndIndent();
        }
    }
}