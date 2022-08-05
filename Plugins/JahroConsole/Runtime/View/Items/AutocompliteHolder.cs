using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JahroConsole.Core.Registry;

namespace JahroConsole.View
{
    internal class AutocompliteHolder : MonoBehaviour
    {
        private List<AutocompliteCommandEntry> _entriesObjects = new List<AutocompliteCommandEntry>();

        [SerializeField]
        private GameObject AutocompliteEntryPrefab;

        [SerializeField]
        private RectTransform HolderTransform;

        public event Action<ConsoleCommandEntry> OnAutocompliteCommandPress = delegate {};

        internal void UpdateEntries(List<ConsoleCommandEntry> entries)
        {
            if (entries.Count > _entriesObjects.Count)
            {
                for (int i=_entriesObjects.Count; i < entries.Count; i++)
                {
                    _entriesObjects.Add(CreateVisualCommandEntry());
                }
            }

            for (int i=0; i<_entriesObjects.Count; i++)
            {
                if (i < entries.Count)
                {
                    _entriesObjects[i].Show(entries[i]);
                }
                else
                {
                    _entriesObjects[i].Hide();
                }
            }
        }

        public void Clear()
        {
            foreach(var entry in _entriesObjects)
            {
                entry.Hide();
            }
        }

        private AutocompliteCommandEntry CreateVisualCommandEntry()
        {
            var entryObject = GameObject.Instantiate(AutocompliteEntryPrefab);
            var entryTransform = entryObject.GetComponent<RectTransform>();
            entryTransform.SetParent(HolderTransform);
            entryTransform.localScale = Vector3.one;
            var entry = entryObject.GetComponent<AutocompliteCommandEntry>();
            entry.Init(OnAutocompliteCommandPress);
            entry.Hide();
            return entry;
        }
    }
}