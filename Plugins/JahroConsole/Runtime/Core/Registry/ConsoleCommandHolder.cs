using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Linq;
using JahroConsole.Logging;
using JahroConsole.Core.Data;
using System;
using JahroConsole.View;

namespace JahroConsole.Core.Registry
{
    internal class ConsoleCommandHolder
    {
        internal List<SimpleGroup> Groups { get { return _groups; } }

        private List<SimpleGroup> _groups;

        private RecentGroup _recentGroup;

        private FavoritesGroup _favoritesGroup;

        internal ConsoleCommandHolder()
        {
            _groups = new List<SimpleGroup>();
            _recentGroup = new RecentGroup();
            _groups.Add(_recentGroup);
            _favoritesGroup = new FavoritesGroup();
            _groups.Add(_favoritesGroup);
            _groups.Add(new DefaultGroup());
        }

        internal void AddCommandMethod(JahroCommandAttribute attribute, MethodInfo methodInfo)
        {
            var commandEntry = new ConsoleCommandEntry(attribute);
            var targetGroup = GetGroup(attribute.GroupName);
            if (targetGroup == null)
            {
                targetGroup = CreateNewGroup(attribute.GroupName);
            }
            
            commandEntry.SetMethodInfo(methodInfo);
            commandEntry.FavoritesStateChanged += delegate (bool state)
            {
                OnCommandFavoritesChanged(commandEntry, state);
            };
            commandEntry.OnExecuted += delegate
            {
                OnCommandExecuted(commandEntry);
            };

            targetGroup.AddCommandEntry(commandEntry);
            if (targetGroup.HasDublicate(commandEntry))
            {
                Jahro.LogWarning(string.Format(MessagesResource.LogCommandNameHasDublicate, commandEntry.Name));
            }
            if (commandEntry.Name.Contains(' '))
            {
                Jahro.LogWarning(string.Format(MessagesResource.LogCommandNameHasSpacing, commandEntry.Name));
            }
        }

        internal List<ConsoleCommandEntry> GetCommandEntries(string name, string[] args)
        {
            List<ConsoleCommandEntry> resultEntries = new List<ConsoleCommandEntry>();
            foreach (var group in _groups)
            {
                var entries = group.GetCommandEntries(name, args);
                if (entries != null)
                {
                    resultEntries.AddRange(entries);
                }
            }
            return resultEntries;
        }

        internal List<ConsoleCommandEntry> GetPossibleCommandsNames(string name)
        {
            string nameToFind = name.ToLower();
            List<ConsoleCommandEntry> resultEntries = new List<ConsoleCommandEntry>();
            foreach (var group in _groups)
            {
                if (group is RecentGroup || group is FavoritesGroup)
                {
                    continue;
                } 
                var entries = group.Entries;
                if (entries != null)
                {
                    foreach (var entryName in entries)
                    {
                        if (entryName.Name.ToLower().IndexOf(nameToFind) != -1)
                        {
                            resultEntries.Add(entryName);
                        }
                    }
                }
            }
            return resultEntries;
        }

        internal List<string> GetCommandsNames()
        {
            List<string> resultEntries = new List<string>();
            foreach (var group in _groups)
            {
                if (group is RecentGroup || group is FavoritesGroup)
                {
                    continue;
                }
                var entries = group.Entries;
                if (entries != null)
                {
                    foreach (var entryName in entries)
                    {
                        resultEntries.Add(entryName.Name);
                    }
                }
            }
            return resultEntries;
        }

        internal void Initialize(ConsoleStorage storage)
        {
            ConsoleStorageController.Instance.OnStorageSave += OnStateSave;

            LoadState(storage);
        }

        private SimpleGroup CreateNewGroup(string groupName)
        {
            var group = new SimpleGroup(groupName);
            _groups.Add(group);
            return group;
        }

        private SimpleGroup GetGroup(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return _groups.Where(g => g is DefaultGroup).First();
            }
            return _groups.Where(g => g.Name == name).FirstOrDefault();
        }

        private void LoadState(ConsoleStorage storage)
        {
            if (storage.Groups == null)
            {
                return;
            }
            foreach (var groupData in storage.Groups)
            {
                var group = _groups.Where(g => g.Name == groupData.Name).FirstOrDefault();
                if (group is RecentGroup)
                {
                    foreach (var entryData in groupData.CommandEntries)
                    {
                        var r = _groups.SelectMany(g => g.Entries).Where(e => e.SimpleName == entryData.SimpleName).FirstOrDefault();
                        if (r != null)
                            group.Entries.Add(r);
                    }
                    GroupData.ApplyData(groupData, group);
                }
                else if (group is FavoritesGroup)
                {
                    group.Foldout = groupData.Foldout;
                }
                else
                {
                    GroupData.ApplyData(groupData, group);
                }
            }
        }

        private void OnCommandFavoritesChanged(ConsoleCommandEntry entry, bool favorite)
        {
            _favoritesGroup.CommandFavoriteChanged(entry, favorite);
        }

        private void OnCommandExecuted(ConsoleCommandEntry entry)
        {
            _recentGroup.CommandExecuted(entry);
        }

        private void OnStateSave(ConsoleStorage storage)
        {
            storage.Groups = new List<GroupData>();
            foreach (var group in _groups)
            {
                GroupData groupData;
                if (group is FavoritesGroup)
                {
                    groupData = GroupData.ExtractFavoritesGroup(group as FavoritesGroup);
                }
                else
                {
                    groupData = GroupData.Extract(group);
                }
                storage.Groups.Add(groupData);
            }
        }
    }
}