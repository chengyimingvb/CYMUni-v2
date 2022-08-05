using System.Collections;
using System.Collections.Generic;
using JahroConsole.Core.Data;
using JahroConsole.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class ConsoleTextView : ConsoleBaseView
    {
        public ConsoleWindowOutputBehaviour OutputBehavior { get; private set; }

        public ConsoleWindowInputBehaviour InputBehavior { get; private set; }

        public LayoutElement MobilePlaceholder;

        public GameObject Controls;

        public GameObject ControlsCancelButton;

        public GameObject SelectButton;

        public GameObject CancelButton;

        public GameObject FilterArea;

        private VerticalLayoutGroup _layoutGroup;

        private RectTransform _rectTransform;

        private void Start()
        {
            _rectTransform = GetComponent<RectTransform>();
            _layoutGroup = GetComponent<VerticalLayoutGroup>();
            
            RefreshSafeArea();
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            CancelSelectMode();
        }

        public override void Init(ConsoleMainWindow mainWindow)
        {
            base.Init(mainWindow);

            OutputBehavior = GetComponentInChildren<ConsoleWindowOutputBehaviour>();
            OutputBehavior.Init(mainWindow.IsMobileMode);
            
            InputBehavior = GetComponentInChildren<ConsoleWindowInputBehaviour>();
            InputBehavior.DidEnterCommand += DidEnterCommand;

            mainWindow.HeaderPanelBehaviour.LogsIndicator.DataSourceCounter = OutputBehavior.DataSource.GetCounter();

            CancelSelectMode();

            if (mainWindow.IsMobileMode)
            {
                FilterArea.SetActive(false);
            }
            else
            {
                FilterArea.SetActive(true);
            }
        }

        public void EnterSelectMode()
        {
            InputBehavior.Disable();
            Controls.SetActive(true);
            OutputBehavior.SelectMode(true);
            SelectButton.gameObject.SetActive(false);
            CancelButton.gameObject.SetActive(true);
            OptionsView?.Close();
            if (MainWindow.IsMobileMode)
            {
                ControlsCancelButton.SetActive(true);
            }
        }

        public void CancelSelectMode()
        {
            InputBehavior.Enable();
            Controls.SetActive(false);
            OutputBehavior.SelectMode(false);
            SelectButton.gameObject.SetActive(true);
            CancelButton.gameObject.SetActive(false);
            ControlsCancelButton.SetActive(false);
        }

        public void Clear()
        {
            CancelSelectMode();
            JahroLogger.ClearAllLogs();
            OptionsView?.Close();
        }

        public void CopySelected()
        {
            OutputBehavior.CopySelectedItems();
            CancelSelectMode();
        }

        public void ShareSelected()
        {
            OutputBehavior.ShareSelectedItems();
            CancelSelectMode();
        }

        public void SelectAll()
        {
            OutputBehavior.SelectAll();   
        }

        public void SearchQueryUpdated(string searchQuery)
        {
            OutputBehavior.SetFilter(searchQuery);
        }

        public override void OnStateSave(ConsoleStorage storage)
        {
            base.OnStateSave(storage);
            storage.GeneralSettings.filterDebug = OutputBehavior.FilterTogglesBehaviour.ToggleDebug.isOn;
            storage.GeneralSettings.filterWarning = OutputBehavior.FilterTogglesBehaviour.ToggleWarnings.isOn;
            storage.GeneralSettings.filterError = OutputBehavior.FilterTogglesBehaviour.ToggleErrors.isOn;
            storage.GeneralSettings.filterCommands = OutputBehavior.FilterTogglesBehaviour.ToggleCommands.isOn;
        }

        public override void OnStateLoad(ConsoleStorage storage)
        {
            OutputBehavior.FilterTogglesBehaviour.UpdateStates(storage.GeneralSettings.filterDebug,
                storage.GeneralSettings.filterWarning,
                storage.GeneralSettings.filterError,
                storage.GeneralSettings.filterCommands);
        }

        internal override void OnWindowRectChanged(Rect rect)
        {
            base.OnWindowRectChanged(rect);
            OutputBehavior.OnMainWindowRectChanged(rect);
        }

        private void Update()
        {
            if (InputBehavior.IsFocused() && TouchScreenKeyboard.visible)
            {
                float keyboardHeight = KeyboardTracker.GetSoftKeyboardHeight();

                Vector2 screenKeyboard = new Vector2(0, keyboardHeight*Screen.height);
                Vector2 rectPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, screenKeyboard, null, out rectPoint);                

                float holderHeight = rectPoint.y - _rectTransform.rect.min.y;
                MobilePlaceholder.preferredHeight = holderHeight + 3;
            }
            else
            {
                MobilePlaceholder.preferredHeight = 0;
            }
        }


        private void DidEnterCommand(string obj)
        {
            OutputBehavior.ScrollToBottom();
        }

        private void RefreshSafeArea()
        {
            int leftPadding = (int)Mathf.Max(SafeArea.x/ScaleFactor, 0);
            int rightPadding = (int)Mathf.Max((Screen.width - (SafeArea.x + SafeArea.width))/ScaleFactor, 0);
            if (_layoutGroup != null)
            {
                _layoutGroup.padding = new RectOffset(leftPadding, rightPadding, 0, 0);
            }
        }

        protected override void OnSafeAreaChanged(Rect safeArea, float scaleFactor)
        {
            base.OnSafeAreaChanged(safeArea, scaleFactor);
            RefreshSafeArea();
        }
    }
}