using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace JahroConsole.Core.Registry
{
    internal class SimpleGroup
    {
        internal string Name { get; private set; }

        internal bool Foldout { get; set; }

        internal bool LIFO { get; set; }

        internal List<ConsoleCommandEntry> Entries { get {return _entries;} } 

        protected List<ConsoleCommandEntry> _entries;

        internal Action OnEntriesChanged = delegate{};

        internal SimpleGroup(string name)
        {
            Name = name;
            _entries = new List<ConsoleCommandEntry>(); //TODO serialize
  
            Foldout = true;
        }

        internal IEnumerable<ConsoleCommandEntry> GetCommandEntries(string name, string[] args)
        {
            return _entries.Where(e => e.Name == name);
        }

        internal string[] GetCommandsNames()
        {
            return _entries.Select(e => e.Name).ToArray();
        }

        internal void AddCommandEntry(ConsoleCommandEntry entry)
        {
            _entries.Add(entry);
            OnEntriesChanged();
        }

        internal void RemoveCommandEntry(ConsoleCommandEntry entry)
        {
            _entries.Remove(entry);
            OnEntriesChanged();
        } 

        internal bool HasDublicate(ConsoleCommandEntry entry)
        {
            return _entries
                .Where(e => e.Name == entry.Name && e.MethodInfo.Name != entry.MethodInfo.Name)
                .Count() > 0;
        }
    }
}