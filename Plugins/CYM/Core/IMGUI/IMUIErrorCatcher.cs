using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RapidGUI;
namespace CYM
{
    [HideMonoScript]
    public class IMUIErrorCatcher : MonoBehaviour
    {
        public static IMUIErrorCatcher Ins { get; private set; }

        class LogMessage
        {
            public string Message;

            public LogMessage(string msg)
            {
                Message = msg;
            }
        }

        public enum LogAnchor
        {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        [Range(0.3f, 1.0f)]
        public float Height = 1.0f;
        [Range(0.3f, 1.0f)]
        public float Width = 0.5f;
        public bool IsSendAchieve = true;

        public int Margin = 20;

        public LogAnchor AnchorPosition = LogAnchor.BottomLeft;

        public int FontSize = 14;

        [Range(0f, 01f)]
        public float BackgroundOpacity = 0.0f;
        public Color BackgroundColor = Color.black;
        public Color ErrorColor = new Color(1, 0.0f, 0.0f);

        static Queue<LogMessage> showQueue = new Queue<LogMessage>();
        static Queue<LogMessage> cacheQueue = new Queue<LogMessage>();

        GUIStyle styleContainer, styleText;
        int padding = 5;

        private bool destroying = false;
        private bool styleChanged = true;
        private bool dirtyCloseError = false;
        private bool dirtyCloseSafe = false;
        private string customDesc = "";

        public static bool IsShow => showQueue.Count > 0;

        public void Awake()
        {
            Ins = this;
            InitStyles();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            styleChanged = true;
        }

        private void InitStyles()
        {
            Texture2D back = new Texture2D(1, 1);
            BackgroundColor.a = BackgroundOpacity;
            back.SetPixel(0, 0, BackgroundColor);
            back.Apply();

            styleContainer = new GUIStyle();
            styleContainer.normal.background = back;
            styleContainer.wordWrap = false;
            styleContainer.padding = new RectOffset(padding, padding, padding, padding);

            styleText = new GUIStyle();
            styleText.fontSize = FontSize;

            styleChanged = false;
        }

        void OnEnable()
        {
            showQueue = new Queue<LogMessage>();
            Application.logMessageReceived += HandleLog;
        }

        void OnDisable()
        {
            if (destroying) return;
            Application.logMessageReceived -= HandleLog;
        }

        void Update()
        {
            if (showQueue != null && cacheQueue != null)
            {
                float InnerHeight = (Screen.height - 2 * Margin) * Height - 2 * padding;
                int TotalRows = (int)(InnerHeight / styleText.lineHeight);

                if (showQueue.Count > 0)
                {
                    while (showQueue.Count > TotalRows)
                        showQueue.Dequeue();
                }
                if (cacheQueue.Count > 0)
                { 
                    while (cacheQueue.Count > 500)
                        cacheQueue.Dequeue();
                }
            }
        }
        private void FixedUpdate()
        {
            if (dirtyCloseError)
            {
                Feedback.SendError(customDesc, "None",IsSendAchieve);
                showQueue.Clear();
                cacheQueue.Clear();
                dirtyCloseError = false;
            }
            if (dirtyCloseSafe)
            {
                showQueue.Clear();
                cacheQueue.Clear();
                dirtyCloseSafe = false;
            }
        }

        void OnGUI()
        {
            if (!BuildConfig.IsWindows)
                return;
            if (Application.isEditor)
                return;
            if (!IsShow) return;
            if (styleChanged) InitStyles();

            float w = (Screen.width - 2 * Margin) * Width;
            float h = (Screen.height - 2 * Margin) * Height;
            float x = 1, y = 1;

            switch (AnchorPosition)
            {
                case LogAnchor.BottomLeft:
                    x = Margin;
                    y = Margin + (Screen.height - 2 * Margin) * (1 - Height);
                    break;

                case LogAnchor.BottomRight:
                    x = Margin + (Screen.width - 2 * Margin) * (1 - Width);
                    y = Margin + (Screen.height - 2 * Margin) * (1 - Height);
                    break;

                case LogAnchor.TopLeft:
                    x = Margin;
                    y = Margin;
                    break;

                case LogAnchor.TopRight:
                    x = Margin + (Screen.width - 2 * Margin) * (1 - Width);
                    y = Margin;
                    break;
            }

            GUILayout.BeginArea(new Rect(x, y, w, h), styleContainer);
            GUILayout.Label(Util.GetStr("Title_ErrorCatcher"),RGUIStyle.warningLabel, GUILayout.Width(400));
            GUILayout.Label(Util.GetStr("Desc_ErrorCatcher"), RGUIStyle.warningLabel, GUILayout.Width(400));
            IsSendAchieve = GUILayout.Toggle(IsSendAchieve, "Send Achieve");
            customDesc = GUILayout.TextArea(customDesc, GUILayout.Width(300));
            if (GUILayout.Button(Util.GetStr("Bnt_ErrorSend"), GUILayout.Width(300)))
            {
                dirtyCloseError = true;
            }
            else if (GUILayout.Button("Close", GUILayout.Width(300)))
            {
                dirtyCloseSafe = true;
            }
            foreach (LogMessage m in showQueue)
            {
                styleText.normal.textColor = ErrorColor;
                GUILayout.Label(m.Message, styleText);
            }
            GUILayout.EndArea();
        }

        void HandleLog(string message, string stackTrace, LogType type)
        {
            if (!BuildConfig.IsWindows)
                return;
            if (Options.IsShow)
                return;
            if (BaseGlobal.DiffMgr!=null && 
                BaseGlobal.DiffMgr.IsSettedGMMod())
                return;
            if (type != LogType.Assert &&
                type != LogType.Error &&
                type != LogType.Exception) 
                return;

            if (message.StartsWith("Request error") ||
                message.StartsWith("Invalid editor window") ||
                message.StartsWith("Card description length is higher than maximum length") ||
                message.StartsWith("Error Making Request to Trello")||
                message.StartsWith("SocketException:")||
                message.StartsWith("Status Code 0"))
                return;
            string[] lines = message.Split(new char[] { '\n' });

            foreach (string l in lines)
            {
                showQueue.Enqueue(new LogMessage(l));
                cacheQueue.Enqueue(new LogMessage(l));
            }

            string[] trace = stackTrace.Split(new char[] { '\n' });

            int curLine = 0;
            int totalLine = 200;
            foreach (string t in trace)
            {
                if (curLine > totalLine)
                    break;
                if (t.Length != 0)
                {
                    cacheQueue.Enqueue(new LogMessage("  " + t));
                    curLine++;
                }
            }
        }

        public static string GetErrorString()
        {
            string ret = "";
            foreach (var item in cacheQueue)
            {
                ret += item.Message + "\n";
            }
            return ret;
        }
        public static string GetTitle()
        {
            if (cacheQueue == null || cacheQueue.Count <= 0)
                return "None";
            return cacheQueue.Dequeue().Message;
        }
    }
}