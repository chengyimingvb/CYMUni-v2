using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class HeaderPanelBehaviour : MonoBehaviour
    {
        [SerializeField]
        private Button FullscreenButton;

        [SerializeField]
        private Button CloseButton;

        [SerializeField]
        private Button OptionsButton;

        [SerializeField]
        private Toggle ConsoleViewToggle;

        [SerializeField]
        private Toggle VisualViewToggle;

        [SerializeField]
        internal LogsIndicatorBehaviour LogsIndicator;

        private ConsoleMainWindow _mainWindow;

        private ScaleControlBehaviour _scaleControl;

        private RectTransform _windowTransform;

        private RectTransform _canvasTransform;

        private HorizontalLayoutGroup _layoutGroup;

        private LayoutElement _layoutElement;

        private Vector2 _dragOffset;

        private bool _dragging;

        public void Init(ConsoleMainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            _windowTransform = mainWindow.GetComponent<RectTransform>();
            _canvasTransform = mainWindow.Canvas.GetComponent<RectTransform>();
            _layoutGroup = GetComponent<HorizontalLayoutGroup>();
            _layoutElement = GetComponent<LayoutElement>();
            _scaleControl = GetComponentInChildren<ScaleControlBehaviour>();

            mainWindow.OnSafeAreaChanged += OnSafeAreaChanged;
            if (mainWindow.IsMobileMode)
            {
                FullscreenButton.gameObject.SetActive(false);
                CloseButton.gameObject.SetActive(false);
                OptionsButton.gameObject.SetActive(true);
                OptionsButton.onClick.AddListener(OnOptionsClick);
                _scaleControl.gameObject.SetActive(false);
            }
            else
            {
                InitDrag();
                OptionsButton.gameObject.SetActive(false);
                _scaleControl.gameObject.SetActive(true);
                FullscreenButton.onClick.AddListener(OnFullscreenClick);
                CloseButton.onClick.AddListener(OnCloseButtonClick);
            }

            _scaleControl.Init(mainWindow.ScalingBehaviour);

            ConsoleViewToggle.onValueChanged.AddListener(OnConsoleTabValueChange);
            VisualViewToggle.onValueChanged.AddListener(OnVisualTabValueChange);
        }

        public void OnCloseButtonClick()
        {
            _mainWindow.Close();
        }

        public void OnFullscreenClick()
        {
            _mainWindow.SetFullscreenMode();
        }

        public void UpdateToggleStates()
        {
            switch(_mainWindow.CurrentMode)
            {
                case ConsoleMainWindow.Mode.Text:
                    ConsoleViewToggle.isOn = true;
                break;
                case ConsoleMainWindow.Mode.Visual:
                    VisualViewToggle.isOn = true;
                break;
            }
        }

        public void OnOptionsClick()
        {
            _mainWindow.OpenOptionsMenu();
        }
        
        private void OnSafeAreaChanged(Rect safeArea, float scaleFactor)
        {

            var currentOffset = _layoutGroup.padding;
            int leftPadding = (int)Mathf.Max(safeArea.x/scaleFactor, 14);
            int topPadding = (int)Mathf.Max((Screen.height - (safeArea.y + safeArea.height))/scaleFactor, 0);
            float heightOffset = 52 + topPadding;

            _layoutGroup.padding = new RectOffset(leftPadding, currentOffset.right, topPadding, currentOffset.bottom);
            _layoutElement.preferredHeight = heightOffset;
        }

        private void InitDrag()
        {   
            var headerDragEventTrigger = gameObject.GetComponent<EventTrigger>();
            headerDragEventTrigger.triggers[0].callback.AddListener(OnHeaderPointerDown);
            headerDragEventTrigger.triggers[1].callback.AddListener(OnHeaderPointerDrag);
            headerDragEventTrigger.triggers[2].callback.AddListener(OnHeaderPointerUp);
        }

        private void OnConsoleTabValueChange(bool active)
        {
            if (active)
            {
                _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Text);
            }
        }

        private void OnVisualTabValueChange(bool active)
        {
            if (active)
            {
                _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Visual);
            }
        }

        private void OnHeaderPointerDown(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            Vector2 clickLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_windowTransform, pointerEventData.position, pointerEventData.pressEventCamera, out clickLocalPoint);

            _dragOffset.x = clickLocalPoint.x;
            _dragOffset.y = clickLocalPoint.y;

            _dragging = true;
        }

        private void OnHeaderPointerDrag(BaseEventData eventData)
        {
            if (_dragging == false)
            {
                return;
            }

            PointerEventData pointerEventData = (PointerEventData)eventData;

            if ((pointerEventData.pressPosition - pointerEventData.position).magnitude < EventSystem.current.pixelDragThreshold)
            {
                return;
            }
            
            Vector2 dragLocalPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasTransform, pointerEventData.position, pointerEventData.pressEventCamera, out dragLocalPoint);

            dragLocalPoint.x +=  _canvasTransform.rect.width/2f - _dragOffset.x;
            dragLocalPoint.y -=  _canvasTransform.rect.height/2f + _dragOffset.y;
            _windowTransform.anchoredPosition = dragLocalPoint;
            _mainWindow.WindowPositionChanged(_windowTransform.anchoredPosition);
        }

        private void OnHeaderPointerUp(BaseEventData eventData)
        {
            _dragging = false;
        }
    }
}