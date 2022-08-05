using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    [Serializable]
    internal class ConsoleStorage
    {
        [SerializeField]
        internal GeneralSettingsData GeneralSettings;

        [SerializeField]
        internal List<GroupData> Groups;

        [SerializeField]
        internal CommandsQueue CommandsQueue;

        [NonSerialized]
        public ProjectSettings ProjectSettings;

        internal ConsoleStorage()
        {
            GeneralSettings = new GeneralSettingsData();
            CommandsQueue = new CommandsQueue();
        }
    }
}