using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole.Core.Registry
{
    internal class RecentGroup : SimpleGroup
    {

        internal const int RECENT_LIMIT = 10;

        internal RecentGroup() : base("Recent")
        {
            Foldout = true;
            LIFO = true;
        }

        internal void CommandExecuted(ConsoleCommandEntry entry)
        {
            if (Entries.Contains(entry))
            {
                Entries.Remove(entry);
            }
            AddCommandEntry(entry);
            if (_entries.Count > RECENT_LIMIT)
            {
                _entries.RemoveAt(0);
            }
        }
    }
}