using UnityEngine;
using UnityEngine.Events;
using JahroConsole.Core.Registry;
using JahroConsole.Core.Data;

namespace JahroConsole
{
    /// <summary>
    /// Main API File
    /// </summary>
    public static partial class Jahro
    {
        /// <summary>
        /// Checks if the console window is open.
        /// </summary>
        /// <returns></returns>
        public static bool IsOpen { get { return _viewManager.IsWindowOpen();} }

        /// <summary>
        /// Checks if console is Enabled in ProjectSettings.
        /// </summary>
        /// <value></value>
        public static bool Enabled { get; private set; }

        /// <summary>
        /// Event, that fires when the console has been shown.
        /// </summary>
        public static UnityAction OnConsoleShow;

        /// <summary>
        /// Event, that fires the console has been hidden.
        /// </summary>
        public static UnityAction OnConsoleHide;

        private static JahroViewManager _viewManager;

        [RuntimeInitializeOnLoadMethod]
        static void OnRuntimeMethodLoad()
        {
            var projectSettings = ConsoleStorageController.ReadOrCreateProjectSettings();
            Enabled = projectSettings.JahroEnabled;
            if (!Enabled)
            {
                Debug.Log("Jahro Console: Disabled");
                return;
            }

            if (!ConsoleCommandsRegistry.Initialized)
            {
                ConsoleCommandsRegistry.Initialize(projectSettings);
            }

#if UNITY_EDITOR

            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += OnAssemblyReload;
#endif
            InitView();
        }

        /// <summary>
        /// Initializes console view
        /// </summary>
        public static void InitView()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't init view");
                return;
            }
            if (!JahroViewManager.IsViewInstantiated())
            {
                _viewManager = JahroViewManager.InstantiateView();
            }
            _viewManager = JahroViewManager.GetInstance();   
            _viewManager.OnStateChanged += (state) =>
            {
                if (state == JahroViewManager.ConsoleViewStates.MainWindowShow)
                {
                    if (OnConsoleShow != null) OnConsoleShow();
                }
                else if (state == JahroViewManager.ConsoleViewStates.MainWindowHide)
                {
                    if (OnConsoleHide != null) OnConsoleHide();
                }
            };
        }

        /// <summary>
        /// Shows main console view.
        /// </summary>
        public static void ShowConsoleView()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't show console view");
                return;
            }
            _viewManager.ShowWindow();
        }

        /// <summary>
        /// Closes main console view.
        /// </summary>
        public static void CloseConsoleView()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't hide view");
                return;
            }
            _viewManager.HideWindow();
        }

        /// <summary>
        /// Enables and shows the Launch button.
        /// </summary>
        public static void ShowLaunchButton()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't show Status Button");
                return;
            }
            _viewManager.ShowLaunchButton();
        }

        /// <summary>
        /// Hides the Launch button.
        /// </summary>
        public static void HideLaunchButton()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Can't hide Status Button");
                return;
            }
            _viewManager.HideLaunchButton();
        }

        /// <summary>
        /// Returns status of the Launch button visibility.
        /// </summary>
        /// <returns></returns>
        public static bool IsStatusButtonEnabled()
        {
            if (!Enabled)
            {
                Debug.LogWarning("Jahro Console disabled. Status button disabled");
                return false;
            }
            return _viewManager.IsOpenButtonVisible();
        }

        /// <summary>
        /// Releases and destroys console instance.
        /// </summary>
        public static void Release()
        {
            JahroViewManager.DestroyInstance();
            ConsoleStorageController.Release();
        }

        private static void OnAssemblyReload()
        {
#if UNITY_EDITOR            
            if (UnityEditor.EditorApplication.isPlaying)
            {
                ConsoleStorageController.SaveState();
                Release();
            }
#endif
        }
    }
}