using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class CanvasScalingBehaviour : MonoBehaviour
    {
        private const float MAX_SCALE = 1.5f;
        private const float MIN_SCALE = -2f;
        private const float SCALE_STEP = 0.25f;
        internal const float OPTIMAL_SCALE = 0.75f;
        private readonly Vector2 minResolution = new Vector2(678, 418);

        private float currentScale;
        private Vector2 initialReferenceResolution;

        public CanvasScaler CanvasScaler;

        public event Action<float> OnScaleChanged;

        private void Awake()
        {
            //initialReferenceResolution = CanvasScaler.referenceResolution;
            initialReferenceResolution = minResolution;

            currentScale = float.MinValue;
        }

        internal void SetScale(float scaleValue)
        {
            
            if (currentScale == scaleValue)
            {
                return;
            }

            currentScale = scaleValue;
            CanvasScaler.referenceResolution = initialReferenceResolution * (2 - currentScale);

            StartCoroutine(EventFireScale());
        }

        private IEnumerator EventFireScale()
        {
            yield return new WaitForEndOfFrame();
            OnScaleChanged?.Invoke(currentScale);
        }

        internal void ScaleIn()
        {
            SetScale(Mathf.Clamp(currentScale + SCALE_STEP, MIN_SCALE, MAX_SCALE));
        }

        internal void ScaleOut()
        {
            SetScale(Mathf.Clamp(currentScale - SCALE_STEP, MIN_SCALE, MAX_SCALE));
        }

        internal string GetReadableScale()
        {
            float percent = (OPTIMAL_SCALE - currentScale) / Mathf.Abs(MIN_SCALE - OPTIMAL_SCALE);
            return (100 - percent * 100).ToString("0") + "%"; 
        }

        internal float GetScale()
        {
            return currentScale;
        }
    }
}