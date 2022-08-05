using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class BlinkIndicator : MonoBehaviour
    {
        private readonly Color32 DEFAULT_COLOR = new Color32(131, 130, 144, 255);

        private const float BLINK_DURATION = 0.2f;

        [SerializeField]
        private Color TargetColor;

        private Image _image;

        private Text _text;

        private Color _targetColor;

        private void Awake()
        {
            _image = GetComponent<Image>();
            _text = GetComponentInChildren<Text>();
            _text.text = "0";

            _targetColor = TargetColor;
        }

        public void Blink(int count)
        {
            _text.text = "" + Mathf.Clamp(count, 0, 99);
            StartCoroutine(BlinkColor());
        }

        public void Clear()
        {
            _text.text = "0";
            _targetColor = TargetColor;
        }

        private void Update()
        {
            _image.color = Color.Lerp(_image.color, _targetColor, 0.1f);
        }

        private IEnumerator BlinkColor()
        {
            _targetColor = Color.white;
            yield return new WaitForSeconds(BLINK_DURATION);
            _targetColor = TargetColor;
        }
    }
}