using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class ScaleControlBehaviour : MonoBehaviour
    {
        public Button ScaleInButton;

        public Button ScaleOutButton;

        public Text ScaleText;

        private CanvasScalingBehaviour _scalingBehaviour;

        public void Init(CanvasScalingBehaviour scalingBehaviour)
        {
            _scalingBehaviour = scalingBehaviour;
            _scalingBehaviour.OnScaleChanged += OnScaleChanged;

            ScaleText.text = _scalingBehaviour.GetReadableScale();
        }

        private void OnScaleChanged(float scale)
        {
            ScaleText.text = _scalingBehaviour.GetReadableScale();
        }

        public void OnScaleInClick()
        {
            _scalingBehaviour.ScaleIn();
        }

        public void OnScaleOutClick()
        {
            _scalingBehaviour.ScaleOut();
        }


    }
}