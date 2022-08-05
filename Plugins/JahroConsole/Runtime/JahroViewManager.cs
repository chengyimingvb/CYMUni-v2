using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using JahroConsole.View;
using JahroConsole.Core.Data;
using UnityEngine.Events;

namespace JahroConsole
{

    public sealed class JahroViewManager : MonoBehaviour
    {

        public enum ConsoleViewStates
        {
            MainWindowShow,
            MainWindowHide   
        }       

        public UnityAction<ConsoleViewStates> OnStateChanged;

        private ConsoleOpenButton _launchButton;

        private ConsoleMainWindow _mainWindow;

        private Canvas _canvas;

        private bool _launchButtonEnabled;
        private bool _keyboardShortcutEnabled;
        private bool _tapAreaEnabled;

        private void Awake()
        {
            _launchButton = GetComponentInChildren<ConsoleOpenButton>(true);
            _mainWindow = GetComponentInChildren<ConsoleMainWindow>(true);
            _canvas = GetComponentInChildren<Canvas>(true);

            DontDestroyOnLoad(this.gameObject);
        }

        private void Start()
        {
            LoadState(ConsoleStorageController.Instance.ConsoleStorage);
            if (_tapAreaEnabled)
            {
                InitTouch();
            }
            if (_keyboardShortcutEnabled)
            {
                InitKeyboard();
            }

            if (!_launchButtonEnabled)
            {
                _launchButton.Hide();
            }
            else
            {
                _launchButton.Show();
            }
        }


        internal void SetCanvasSortingOrder(int sortingOrder)
        {
            _canvas.sortingOrder = sortingOrder;
        }

        public void HideWindow()
        {
            if (_launchButtonEnabled)
            {
                _launchButton.Show(); 
            }
            _mainWindow.Close();
            if (OnStateChanged != null)
            {
                OnStateChanged(ConsoleViewStates.MainWindowHide);
            }
        }

        public void ShowWindow()
        {
            _launchButton.Hide();
            _mainWindow.Show();
            if (OnStateChanged != null)
            {
                OnStateChanged(ConsoleViewStates.MainWindowShow);
            }
        }

        public bool IsWindowOpen()
        {
            return _mainWindow.IsOpen();
        }

        public void ShowLaunchButton()
        {
            _launchButton.Show();
        }

        public void HideLaunchButton()
        {
            _launchButton.Hide();
        }

        public bool IsOpenButtonVisible()
        {
            return _launchButton.gameObject.activeSelf;
        }

        public void Release()
        {

        }

        private void InitTouch()
        {
            var tapTracker = gameObject.AddComponent<GestureTrackerTap>();
            tapTracker.OnTapsTracked += ShowWindow;
        }

        private void InitKeyboard()
        {
            var keyboardTracker = gameObject.AddComponent<KeyboardTracker>();
            keyboardTracker.OnTildaPressed += delegate ()
            {
                if (IsWindowOpen())
                {  
                    HideWindow();
                }
                else
                {
                    ShowWindow();
                }
            };
            keyboardTracker.OnEscPressed += delegate ()
            {
                if (IsWindowOpen())
                {
                    HideWindow();
                }
            };
            keyboardTracker.SwitchToTextMode += delegate ()
            {
                if (IsWindowOpen())
                {
                    _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Text);
                }
            };
            keyboardTracker.SwitchToVisualMode += delegate ()
            {
                if (IsWindowOpen())
                {
                    _mainWindow.SwitchToMode(ConsoleMainWindow.Mode.Visual);
                }
            };
        }

        private void OnApplicationQuit()
        {            
            ConsoleStorageController.SaveState();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause)
            {     
                ConsoleStorageController.SaveState();
            }
        }

        private void LoadState(ConsoleStorage storage)
        {
            _launchButtonEnabled = storage.ProjectSettings.ShowLaunchButton;
            _keyboardShortcutEnabled = storage.ProjectSettings.UseLaunchKeyboardShortcut;
            _tapAreaEnabled = storage.ProjectSettings.UseLaunchTapArea;
        }   

        public static void DestroyInstance()
        {
            if (IsViewInstantiated() == false)
            {
                return; 
            }

            var viewManager = GetInstance();
            viewManager.Release();
            GameObject.DestroyImmediate(viewManager.gameObject);
        }

        public static bool IsViewInstantiated()
        {
            return GetInstance() != null;
        }

        public static JahroViewManager GetInstance()
        {
            return GameObject.FindObjectOfType<JahroViewManager>();
        }

        internal static JahroViewManager InstantiateView()
        {
            var prefab = Resources.Load<GameObject>("Prefabs/JahroConsole");
            var consoleObject = GameObject.Instantiate(prefab);
            var viewManager = consoleObject.GetComponent<JahroViewManager>();

            var sceneCanvases = Canvas.FindObjectsOfType<Canvas>();
            int maxSortingOrder = 0;
            foreach(var canvas in sceneCanvases)
            {
                if (canvas.sortingOrder > maxSortingOrder)
                {
                    maxSortingOrder = canvas.sortingOrder;
                }
            }

            viewManager.SetCanvasSortingOrder(maxSortingOrder + 1);
            viewManager.HideWindow();
            return viewManager;
        }
    }
}