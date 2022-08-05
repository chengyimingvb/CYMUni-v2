using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using JahroConsole.Core.Registry;

namespace JahroConsole.Core.Data
{
    [Serializable]
    internal struct GroupData
    {
        [SerializeField]
        internal string Name;
        [SerializeField]
        internal bool Foldout;
        [SerializeField]
        internal List<CommandEntryData> CommandEntries;

        internal static GroupData Extract(SimpleGroup group)
        {
            GroupData groupData = new GroupData();
            groupData.Name = group.Name;
            groupData.Foldout = group.Foldout;
            var entries = new List<CommandEntryData>();
            foreach(var entry in group.Entries)
            {
                var dataEntry = CommandEntryData.ExtractData(entry);
                entries.Add(dataEntry);
            }
            groupData.CommandEntries = entries;
            return groupData;
        }

        internal static GroupData ExtractFavoritesGroup(FavoritesGroup group)
        {
            GroupData groupData = new GroupData();
            groupData.Name = group.Name;
            groupData.Foldout = group.Foldout;
            return groupData;
        }

        internal static void ApplyData(GroupData data, SimpleGroup group)
        {
            if (group == null)
            {
                return;
            }
            group.Foldout = data.Foldout;
            foreach(var entryData in data.CommandEntries)
            {
                
                var entry = group.Entries.Where(e => e.SimpleName == entryData.SimpleName).FirstOrDefault();
                if (entry != null)
                {
                    CommandEntryData.ApplyData(entryData, entry);
                }
            }
        }
    }
}