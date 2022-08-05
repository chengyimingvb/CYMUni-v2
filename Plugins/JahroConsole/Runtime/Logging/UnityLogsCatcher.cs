using JahroConsole.Core;
using JahroConsole.Logging;
using UnityEngine;

namespace JahroConsole.View
{
    internal class UnityLogsCatcher : MonoBehaviour
    {
        private void Awake()
        {
            Application.logMessageReceived += ApplicationOnlogMessageReceived;
        }

        private void ApplicationOnlogMessageReceived(string condition, string stacktrace, LogType type)
        {
            var jahroLogType = (EJahroLogType)(int) type;
            JahroLogger.Log(condition, stacktrace, jahroLogType);
        }

        private void OnDestroy()
        {
            Application.logMessageReceived -= ApplicationOnlogMessageReceived;
        }
    }
}