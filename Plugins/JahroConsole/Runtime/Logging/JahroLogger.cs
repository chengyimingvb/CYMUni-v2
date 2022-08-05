using System;
using System.Collections.Generic;
using JahroConsole.Core;
using JahroConsole.Core.NotificationService;

namespace JahroConsole.Logging
{
    internal delegate void JahroCommandInputHandler(string message, string context, EJahroLogType logType);

    internal enum EJahroLogType
    {
        Error,
        Assert,
        Warning,
        Log,
        Exception,
        JahroException,
        JahroWarning,
        JahroCommand,
        JahroError,
        JahroDebug,
        JahroInfo
    }

    public class JahroLogger
    {
        internal static event JahroCommandInputHandler OnLogEvent = delegate { };

        internal static event Action OnClearAllLogs = delegate { };

        internal static void Log(string message, string details, EJahroLogType logType)
        {
            OnLogEvent(message, details, logType);
            NotificationService.Instance.InvokeLogAdded(JahroLogGroup.MatchGroup(logType));
        }

        internal static void LogCommand(string command, object[] parameters, string result)
        {
            Log(JahroLogsFormatter.FormatCommand(command, parameters, result, 1), "", EJahroLogType.JahroCommand);
        }

        internal static void LogCommand(string command, object[] parameters, List<object> results)
        {
            string outputResults = "";
            foreach (var result in results)
            {
                if (result != null)
                    outputResults += "\n" + result.ToString();
            }
            Log(JahroLogsFormatter.FormatCommand(command, parameters, outputResults, results.Count), "", EJahroLogType.JahroCommand);
        }

        internal static void ClearAllLogs()
        {
            OnClearAllLogs();
            NotificationService.Instance.InvokeLogsClear();
        }
    }

    internal class JahroLogGroup
    {

        public enum EJahroLogGroup
        {
            Internal,
            Debug,
            Warning,
            Error,
            Command
        }

        public static EJahroLogGroup MatchGroup(EJahroLogType logType)
        {
            EJahroLogGroup group = EJahroLogGroup.Internal;
            switch (logType)
            {
                case EJahroLogType.JahroInfo:
                    break;

                case EJahroLogType.Assert:
                case EJahroLogType.Log:
                case EJahroLogType.JahroDebug:
                    group = EJahroLogGroup.Debug;
                    break;

                case EJahroLogType.Warning:
                case EJahroLogType.JahroWarning:
                    group = EJahroLogGroup.Warning;
                    break;

                case EJahroLogType.JahroCommand:
                    group = EJahroLogGroup.Command;
                    break;

                case EJahroLogType.Error:
                case EJahroLogType.Exception:
                case EJahroLogType.JahroException:
                case EJahroLogType.JahroError:
                    group = EJahroLogGroup.Error;
                    break;
            }
            return group;
        }
    }
}