using JahroConsole.Logging;

namespace JahroConsole.Core.Data
{
    internal class JahroDatasourceFilterSettings
    {
        public bool ShowErrors { get; set; } = true;
        public bool ShowLogs { get; set; } = true;
        public bool ShowWarnings { get; set; } = true;
        public bool ShowJahroLogs { get; set; } = true;
        public string SearchString { get; set; }
    }
    
    internal class JahroCommandEntity
    {
        private string message;
        private string stackTrace;
        private EJahroLogType logType;

        public string Message => message;
        public string StackTrace => stackTrace;
        public EJahroLogType LogType => logType;

        public bool Expanded { get; set; }
        public bool Selected { get; set; }
        public bool HasDetails { get { return !string.IsNullOrEmpty(stackTrace); } }
        public bool Selectable { get { return logType != EJahroLogType.JahroInfo; } }
            
        public JahroCommandEntity(string message, string stackTrace, EJahroLogType logType)
        {
            this.message = message;
            this.stackTrace = stackTrace;
            this.logType = logType;
        }
    }
}