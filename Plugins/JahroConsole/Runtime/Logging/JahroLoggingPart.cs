using JahroConsole.Logging;
using System;

namespace JahroConsole
{
    public static partial class Jahro
    {
        /// <summary>
        /// Log message to Jahro Console
        /// </summary>
        /// <param name="message">Main message</param>
        public static void Log(string message)
        {
            Log(message, string.Empty);
        }

        /// <summary>
        /// Log message to Jahro Console
        /// </summary>
        /// <param name="message">Main message</param>
        /// <param name="details">Details. They will be shown in expanded state</param>
        public static void Log(string message, string details)
        {
            LogDebug(message, details);
        }


        /// <summary>
        /// Add message to Jahro Console with the level of JahroDebug
        /// </summary>
        /// <param name="message">Main message</param>
        public static void LogDebug(string message)
        {
            LogDebug(message,string.Empty);
        }

        /// <summary>
        /// Add message to Jahro Console with the level of JahroDebug
        /// </summary>
        /// <param name="message">Main message</param>
        /// <param name="details">Details. They will be shown in expanded state</param>
        public static void LogDebug(string message, string details)
        {
            JahroLogger.Log(message, details, EJahroLogType.JahroDebug);
        }


        /// <summary>
        /// Add message to Jahro Console with the level of JahroWarning
        /// </summary>
        /// <param name="message">Main message</param>
        public static void LogWarning(string message)
        {
            LogWarning(message, string.Empty);
        }

        /// <summary>
        /// Add message to Jahro Console with the level of JahroWarning
        /// </summary>
        /// <param name="message">Main message</param>
        /// <param name="details">Details. They will be shown in expanded state</param>
        public static void LogWarning(string message, string details)
        {
            JahroLogger.Log(message, details, EJahroLogType.JahroWarning);
        }


        /// <summary>
        /// Log formatted exception to Jahro Console
        /// </summary>
        /// <param name="exception">Exception to log</param>
        public static void LogException(Exception exception)
        {
            JahroLogger.Log(exception.Message, exception.StackTrace, EJahroLogType.JahroException);
        }

        /// <summary>
        /// Log formatted exception to Jahro Console
        /// </summary>
        /// <param name="message">Descriptive message</param>
        /// <param name="exception">Exception to log</param>
        public static void LogException(string message, Exception exception)
        {
            JahroLogger.Log(message, exception.StackTrace, EJahroLogType.JahroException);
        }
        

        /// <summary>
        /// Add message to Jahro Console with the level of JahroError
        /// </summary>
        /// <param name="message">Main message</param>
        public static void LogError(string message)
        {
            Jahro.LogError(message, string.Empty);
        }

        /// <summary>
        /// Add message to Jahro Console with the level of JahroError
        /// </summary>
        /// <param name="message">Main message</param>
        /// <param name="details">Details. They will be shown in expanded state</param>
        public static void LogError(string message, string details)
        {
            JahroLogger.Log(message, details, EJahroLogType.JahroError);
        }

        /// <summary>
        /// Clears console
        /// </summary>
        public static void ClearAllLogs()
        {
            JahroLogger.ClearAllLogs();
        }
    }
}