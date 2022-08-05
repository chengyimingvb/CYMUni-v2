using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JahroConsole.Logging;

namespace JahroConsole.Core.NotificationService
{
    internal sealed class NotificationService
    {
        public static NotificationService Instance
        {
            get
            {
                if (_instance == null) _instance = new NotificationService();
                return _instance;
            }
        }

        private static NotificationService _instance;

        public event Action<JahroLogGroup.EJahroLogGroup> OnLogAdded;

        public event Action OnLogsClear;

        private NotificationService()
        {

        }

        public void InvokeLogAdded(JahroLogGroup.EJahroLogGroup logGroup)
        {
            OnLogAdded?.Invoke(logGroup);
        }

        public void InvokeLogsClear()
        {
            OnLogsClear?.Invoke();
        }
    }
}