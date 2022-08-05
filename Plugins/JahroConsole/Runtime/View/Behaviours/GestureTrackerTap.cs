using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JahroConsole.View
{
    internal class GestureTrackerTap : MonoBehaviour
    {
        public const int TAP_COUNT_LAUNCH = 4;

        public const float TAPS_DELTA = 0.5f;

        private int _tapCount;

        private float _prevTapTimestemp;

        public Action OnTapsTracked = delegate{}; 

        private void Update()
        {
            if (Input.touches.Length > 0)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Ended && touch.position.y > Screen.height * 0.75f)
                {
                    float timeSincePreviousClick = Time.realtimeSinceStartup - _prevTapTimestemp;        
                    if (timeSincePreviousClick < TAPS_DELTA)
                    {
                        _tapCount++;
                        if (_tapCount == TAP_COUNT_LAUNCH)
                        {
                            OnTapsTracked();
                        }
                    }
                    else
                    {
                        _tapCount = 1;
                    }
                    _prevTapTimestemp = Time.realtimeSinceStartup;
                }
            }
        }
    }
}