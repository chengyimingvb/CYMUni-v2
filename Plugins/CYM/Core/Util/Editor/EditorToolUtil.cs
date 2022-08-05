using System.Collections.Generic;
//#if UNITY_EDITOR
using UnityEditor;
//#endif
using UnityEngine;
namespace CYM
{
    public partial class EditorToolUtil
    {
        static Dictionary<int, string[]> layerNames = new Dictionary<int, string[]>();
        static long lastUpdateTick;

        /// <summary>
        /// Tag names and an additional 'Edit Tags...' entry.
        /// Used for SingleTagField
        /// </summary>
        static string[] tagNamesAndEditTagsButton;

        /// <summary>
        /// Last time tagNamesAndEditTagsButton was updated.
        /// Uses EditorApplication.timeSinceStartup
        /// </summary>
        static double timeLastUpdatedTagNames;

        public static int TagField(string label, int value, System.Action editCallback)
        {
            // Make sure the tagNamesAndEditTagsButton is relatively up to date
            if (tagNamesAndEditTagsButton == null || EditorApplication.timeSinceStartup - timeLastUpdatedTagNames > 1)
            {
                timeLastUpdatedTagNames = EditorApplication.timeSinceStartup;
                var tagNames = AstarPath.FindTagNames();
                tagNamesAndEditTagsButton = new string[tagNames.Length + 1];
                tagNames.CopyTo(tagNamesAndEditTagsButton, 0);
                tagNamesAndEditTagsButton[tagNamesAndEditTagsButton.Length - 1] = "Edit Tags...";
            }

            // Tags are between 0 and 31
            value = Mathf.Clamp(value, 0, 31);

            var newValue = EditorGUILayout.IntPopup(label, value, tagNamesAndEditTagsButton, new[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, -1 });

            // Last element corresponds to the 'Edit Tags...' entry. Open the tag editor
            if (newValue == -1)
            {
                editCallback();
            }
            else
            {
                value = newValue;
            }

            return value;
        }

        /// <summary>Displays a LayerMask field.</summary>
        /// <param name="label">Label to display</param>
        /// <param name="selected">Current LayerMask</param>
        public static LayerMask LayerMaskField(string label, LayerMask selected)
        {
            if (Event.current.type == EventType.Layout && System.DateTime.UtcNow.Ticks - lastUpdateTick > 10000000L)
            {
                layerNames.Clear();
                lastUpdateTick = System.DateTime.UtcNow.Ticks;
            }

            string[] currentLayerNames;
            if (!layerNames.TryGetValue(selected.value, out currentLayerNames))
            {
                var layers = Pathfinding.Util.ListPool<string>.Claim();

                int emptyLayers = 0;
                for (int i = 0; i < 32; i++)
                {
                    string layerName = LayerMask.LayerToName(i);

                    if (layerName != "")
                    {
                        for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                        layers.Add(layerName);
                    }
                    else
                    {
                        emptyLayers++;
                        if (((selected.value >> i) & 1) != 0 && selected.value != -1)
                        {
                            for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i + 1 - emptyLayers));
                        }
                    }
                }

                currentLayerNames = layerNames[selected.value] = layers.ToArray();
                Pathfinding.Util.ListPool<string>.Release(ref layers);
            }

            selected.value = EditorGUILayout.MaskField(label, selected.value, currentLayerNames);
            return selected;
        }
    }

}