using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class BottomPanelBehaviour : MonoBehaviour
    {
        public Button CloseButton;

        public Text CBText;

        private SizeDragBehaviour _sizeDragBehaviour;

        private LayoutElement _layoutElement;

        private ConsoleMainWindow _mainWindow;

        void Awake()
        {
            _sizeDragBehaviour = GetComponentInChildren<SizeDragBehaviour>();
            _sizeDragBehaviour.OnWindowRectChanged += OnWindowRectChanged;
            _layoutElement = GetComponent<LayoutElement>();
        }

        internal void Init(ConsoleMainWindow mainWindow)
        {
            _mainWindow = mainWindow;

            if (mainWindow.IsMobileMode)
            {
                mainWindow.OnSafeAreaChanged += OnSafeAreaChanged;
                CloseButton.onClick.AddListener(OnCloseButtonClick);
                _layoutElement.preferredHeight = 75f;
                _sizeDragBehaviour.gameObject.SetActive(false);
                OnWindowRectChanged(mainWindow.WindowTransform.rect);
            }
            else
            {
                CloseButton.gameObject.SetActive(false);
                _layoutElement.preferredHeight = 30f;
                _sizeDragBehaviour.Init(mainWindow.Canvas.GetComponent<RectTransform>(), mainWindow.WindowTransform);
            }
        }

        private void OnWindowRectChanged(Rect obj)
        {
            _mainWindow.WindowRectChanged(obj);
        }

        private void OnCloseButtonClick()
        {   
            _mainWindow.Close();
        }

        private void OnSafeAreaChanged(Rect safeArea, float scaleFactor)
        {
            OnWindowRectChanged(_mainWindow.WindowTransform.rect);
            if (safeArea.y > 0)
            {
                CBText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 25);
            }
        }
    }
}