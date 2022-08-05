using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class ViewToggleItem : MonoBehaviour
    {
        public Image Image;

        public Text Text;

        private Color ActiveColor = new Color(0.949f, 0.294f, 0.086f);

        private Color DeactiveColor = new Color(0.616f, 0.616f, 0.616f);

        private Toggle _toggle;

        private void Start()
        {
            _toggle = GetComponent<Toggle>();
            _toggle.onValueChanged.AddListener(onValueChanged);
            onValueChanged(_toggle.isOn);
        }

        private void onValueChanged(bool active)
        {
            if (active)
            {
                Text.color = ActiveColor;
                // Text.CrossFadeColor(ActiveColor, 0.1f, false, false);
                // Image.CrossFadeColor(ActiveColor, 0.1f, false, false);
                Image.color = ActiveColor;
            }
            else
            {
                Text.color = DeactiveColor;
                // Text.CrossFadeColor(DeactiveColor, 0.1f, false, false);
                // Image.CrossFadeColor(DeactiveColor, 0.1f, false, false);
                Image.color = DeactiveColor;
            }
        }
    }
}