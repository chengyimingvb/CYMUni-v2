using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using JahroConsole.Core.Data;

namespace JahroConsole.View
{
    public class ConsoleMainWindow : MonoBehaviour
    {
        private readonly float DELTA_TO_FULLSCREEN = Screen.width * 0.01f;

        public RectTransform HeaderPanelTransform;

        public Canvas Canvas { get; private set; }

        public RectTransform WindowTransform { get; private set; }

        [SerializeField]
        internal HeaderPanelBehaviour HeaderPanelBehaviour { get; private set; }

        [SerializeField]
        internal BottomPanelBehaviour BottomPanelBehaviour { get; private set; }

        internal CanvasScalingBehaviour ScalingBehaviour { get; private set; }

        internal bool Fullscreen { get; private set; }

        public bool IsMobileMode { get { return Application.isMobilePlatform;}}

        private ConsoleOpenButton OpenButton;

        private RectTransform _canvasTransform;

        private ConsoleBaseView[] _views;

        private ConsoleBaseView _activeView;

        private int _lastScreenWidth;

        private int _lastScreenHeight;

        private Vector3[] _cornetsArray = new Vector3[4];

        private Rect _lastSafeArea = new Rect(0,0,0,0);

        public Mode CurrentMode { get; private set; }

        public Action OnWindowSizeChanged;

        public Action<Rect, float> OnSafeAreaChanged;

        public enum Mode
        {
            Text,
            Visual
        }

        private void Awake()
        {
            WindowTransform = GetComponent<RectTransform>();
            Canvas = GetComponentInParent<Canvas>();
            _canvasTransform = Canvas.GetComponent<RectTransform>();
            ScalingBehaviour = GetComponent<CanvasScalingBehaviour>();
            ScalingBehaviour.OnScaleChanged += OnScaleChanged;

            HeaderPanelBehaviour = GetComponentInChildren<HeaderPanelBehaviour>();
            BottomPanelBehaviour = GetComponentInChildren<BottomPanelBehaviour>();

            _views = GetComponentsInChildren<ConsoleBaseView>(true);
            foreach(var view in _views)
            {
                view.Init(this);
                view.Deactivate();
            }
        }

        private void Start()
        {
            HeaderPanelBehaviour.Init(this);
            BottomPanelBehaviour.Init(this);            

            ConsoleStorageController.Instance.OnStorageSave += OnStateSave;

            LoadState(ConsoleStorageController.Instance.ConsoleStorage);
        }

        private void Update()
        {
            if (_lastScreenWidth != Screen.width || _lastScreenHeight != Screen.height)
            {
                _lastScreenWidth = Screen.width;
                _lastScreenHeight = Screen.height;
                
                if (Fullscreen)
                {
                    SetFullscreenMode();
                }
                OnWindowSizeChanged?.Invoke();
            }   

            var safeArea = Screen.safeArea;
            if (safeArea != _lastSafeArea)
            {
                _lastSafeArea = safeArea;
                OnSafeAreaChanged?.Invoke(safeArea, Canvas.scaleFactor);
            }
        }

        internal void SetFullscreenMode()
        {
            Fullscreen = true;
            
            WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, _canvasTransform.rect.width);
            WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, _canvasTransform.rect.height);
        }


        private void OnScaleChanged(float scale)
        {
            if (Fullscreen)
            {
                WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, _canvasTransform.rect.width);
                WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, _canvasTransform.rect.height);
            }
        }

        internal void WindowRectChanged(Rect rect)
        {
            Fullscreen = IsCloseToFullscreen();
            foreach (var view in _views)
            {
                view.OnWindowRectChanged(rect);
            }
        }

        internal void WindowPositionChanged(Vector2 anchoredPosition)
        {
            Fullscreen = IsCloseToFullscreen();
        }

        internal void SetOpenButton(ConsoleOpenButton openButton)
        {
            OpenButton = openButton;
        }

        internal void Show()
        {
            gameObject.SetActive(true);
        }

        internal void Close()
        {
            this.gameObject.SetActive(false);
            if (OpenButton != null)
            {
                OpenButton.Show();
            }
        }

        internal bool IsOpen()
        {
            return gameObject.activeInHierarchy;
        }

        internal void SwitchToMode(Mode mode)
        {
            CurrentMode = mode;
            HeaderPanelBehaviour.UpdateToggleStates();
            OnActiveViewChanged();
        }

        internal void OpenOptionsMenu()
        {
            if (_activeView.OptionsView == null)
            {
                return;
            }

            if (_activeView.OptionsView.IsOpen())
                _activeView.CloseOptions();
            else
                _activeView.ShowOptions();
        }

        internal void ForceCloseOptions()
        {
            _activeView.CloseOptions();
        }

        private void OnStateSave(ConsoleStorage storage)
        {
            storage.GeneralSettings.WindowAnchoredPosition = WindowTransform.anchoredPosition;
            storage.GeneralSettings.WindowSize = WindowTransform.rect.size;
            storage.GeneralSettings.Fullscreen = Fullscreen;
            storage.GeneralSettings.Mode = CurrentMode.ToString();
            storage.GeneralSettings.scale = ScalingBehaviour.GetScale();
            foreach (var view in _views)
            {
                view.OnStateSave(storage);
            }
        }

        private void LoadState(ConsoleStorage storage)
        {
            Fullscreen = storage.GeneralSettings.Fullscreen;
            if (Fullscreen)
            {
                WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0, _canvasTransform.rect.width);
                WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, _canvasTransform.rect.height);      
            }
            else
            {
                var size = storage.GeneralSettings.WindowSize;
                WindowTransform.anchoredPosition = storage.GeneralSettings.WindowAnchoredPosition;
                WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, WindowTransform.anchoredPosition.x, size.x);
                WindowTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, - WindowTransform.anchoredPosition.y, size.y);
            }
            if (string.IsNullOrEmpty(storage.GeneralSettings.Mode) == false)
            {
                SwitchToMode((Mode)Enum.Parse(typeof(Mode), storage.GeneralSettings.Mode));
            }
            else
            {
                SwitchToMode(Mode.Text);
            }

            ScalingBehaviour.SetScale(storage.GeneralSettings.scale);

            foreach (var view in _views)
            {
                view.OnStateLoad(storage);
            }
        }

        private void OnActiveViewChanged()
        {
            var previoutActiveView = _activeView;
            foreach(var view in _views)
            {
                if (CurrentMode == Mode.Text && view is ConsoleTextView)
                {
                    _activeView = view;
                }
                else if (CurrentMode == Mode.Visual && view is ConsoleVisualView)
                {
                    _activeView = view;
                }
            }
            if (previoutActiveView != _activeView)
            {
                if (previoutActiveView != null)
                    previoutActiveView.Deactivate();
                _activeView.Activate();
            }
        }

        private bool IsCloseToFullscreen()
        {
            Vector3 left, top, right, bottom;
            _canvasTransform.GetWorldCorners(_cornetsArray);
            left = _cornetsArray[0];
            top = _cornetsArray[1];
            right = _cornetsArray[2];
            bottom = _cornetsArray[3];

            WindowTransform.GetWorldCorners(_cornetsArray);
            left -= _cornetsArray[0];
            top -= _cornetsArray[1];
            right -= _cornetsArray[2];
            bottom -= _cornetsArray[3];

            return (left.magnitude < DELTA_TO_FULLSCREEN && top.magnitude < DELTA_TO_FULLSCREEN
                && right.magnitude < DELTA_TO_FULLSCREEN && bottom.magnitude < DELTA_TO_FULLSCREEN);
        }
    }
}