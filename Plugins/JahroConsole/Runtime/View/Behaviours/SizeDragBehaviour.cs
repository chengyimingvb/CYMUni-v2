﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace JahroConsole.View
{
    internal class SizeDragBehaviour : MonoBehaviour
    {
        public const float MIN_WIDTH = 375;

        public const float MIN_HEIGHT = 260;

        private Vector2 _dragOffset;

        private bool _dragging;

        private RectTransform _canvasTransform;

        private RectTransform _windowTransform;

        public event Action<Rect> OnWindowRectChanged;

        void Awake()
        {
            var sizeDragEventTrigger = gameObject.GetComponent<EventTrigger>();
            sizeDragEventTrigger.triggers[0].callback.AddListener(OnSizeDragPointerDown);
            sizeDragEventTrigger.triggers[1].callback.AddListener(OnSizeDrag);
            sizeDragEventTrigger.triggers[2].callback.AddListener(OnSizePointerUp);
        }

        public void Init(RectTransform canvasTransform, RectTransform windowTransform)
        {
            _canvasTransform = canvasTransform;
            _windowTransform = windowTransform;
            OnWindowRectChanged?.Invoke(_windowTransform.rect);
        }

        private void OnSizeDragPointerDown(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            Vector2 clickLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_windowTransform, pointerEventData.position, pointerEventData.pressEventCamera, out clickLocalPoint);

            _dragOffset.x = _windowTransform.rect.xMax - clickLocalPoint.x;
            _dragOffset.y = _windowTransform.rect.yMin - clickLocalPoint.y;

            _dragging = true;
        }

        private void OnSizeDrag(BaseEventData eventData)
        {
            if (_dragging == false)
            {
                return;
            }

            PointerEventData pointerEventData = (PointerEventData)eventData;
            
            Vector2 dragLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_windowTransform, pointerEventData.position, pointerEventData.pressEventCamera, out dragLocalPoint);
            
            float sizeX = dragLocalPoint.x + _dragOffset.x;
            float sizeY = 0 - dragLocalPoint.y - _dragOffset.y;

            sizeX = Mathf.Clamp(sizeX, MIN_WIDTH, _canvasTransform.rect.width - _windowTransform.anchoredPosition.x);
            sizeY = Mathf.Clamp(sizeY, MIN_HEIGHT, _canvasTransform.rect.height + _windowTransform.anchoredPosition.y);

            _windowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, _windowTransform.anchoredPosition.x, sizeX);
            _windowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, - _windowTransform.anchoredPosition.y, sizeY);
            OnWindowRectChanged?.Invoke(_windowTransform.rect);
        }

        private void OnSizePointerUp(BaseEventData eventData)
        {
            _dragging = false;
        }
    }
}