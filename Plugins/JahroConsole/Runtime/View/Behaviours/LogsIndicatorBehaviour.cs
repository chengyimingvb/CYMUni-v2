using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JahroConsole.Core.Data;
using JahroConsole.Core.NotificationService;
using JahroConsole.Logging;

namespace JahroConsole.View
{
    internal class LogsIndicatorBehaviour : MonoBehaviour
    {
        [SerializeField]
        private BlinkIndicator debugIndicator;

        [SerializeField]
        private BlinkIndicator warningIndicator;
         
        [SerializeField]
        private BlinkIndicator errorsIndicator;

        [SerializeField]
        private BlinkIndicator commandsIndicator;

        public JahroCommandsDataSourceCounter DataSourceCounter { get; set; }

        private void Awake()
        {
            
        }

        private void OnEnable()
        {
            NotificationService.Instance.OnLogAdded += OnLogAdded;
            NotificationService.Instance.OnLogsClear += OnLogsClear;
        }

        private void OnDisable()
        {
            NotificationService.Instance.OnLogAdded -= OnLogAdded;
            NotificationService.Instance.OnLogsClear -= OnLogsClear;
        }

        private void OnLogAdded(JahroLogGroup.EJahroLogGroup group)
        {
            switch (group)
            {
                case JahroLogGroup.EJahroLogGroup.Internal:
                    break;
                case JahroLogGroup.EJahroLogGroup.Debug:
                    debugIndicator.Blink(DataSourceCounter.Debug);
                    break;
                case JahroLogGroup.EJahroLogGroup.Warning:
                    warningIndicator.Blink(DataSourceCounter.Warning);
                    break;
                case JahroLogGroup.EJahroLogGroup.Error:
                    errorsIndicator.Blink(DataSourceCounter.Error);
                    break;
                case JahroLogGroup.EJahroLogGroup.Command:
                    commandsIndicator.Blink(DataSourceCounter.Commands);
                    break;
            }
        }

        private void OnLogsClear()
        {
            debugIndicator.Clear();
            warningIndicator.Clear();
            errorsIndicator.Clear();
            commandsIndicator.Clear();
        }
    }
}