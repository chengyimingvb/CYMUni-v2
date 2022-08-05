using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using JahroConsole.Core.Registry;

namespace JahroConsole.View
{
    internal class ConsoleGroupLayout : MonoBehaviour
    {
        internal SimpleGroup Group { get; private set; }

        public RectTransform ContentTransform;

        public Image FoldoutImageOn;

        public Image FoldoutImageOff;

        private Toggle _foldoutToggle;

        private Text _groupName;

        private Dictionary<ConsoleCommandEntry, ConsoleVisualCommand> _visualCommands;

        private GameObject _visualEntryPrefab;

        private ConsoleVisualView _visualView;

        internal void Init(SimpleGroup group, GameObject visualEntryPrefab, ConsoleVisualView visualView)
        {
            Group = group;
            _visualView = visualView;
            _visualEntryPrefab = visualEntryPrefab;

            _groupName = this.GetComponentInChildren<Text>();
            _groupName.text = Group.Name;

            _foldoutToggle = this.GetComponentInChildren<Toggle>();
            _foldoutToggle.SetIsOnWithoutNotify(group.Foldout);
            // _foldoutToggle.isOn = group.Foldout;
            _foldoutToggle.onValueChanged.AddListener(OnFoldoutStateChanged);
            OnFoldoutStateChanged(group.Foldout);

            Group.OnEntriesChanged += OnEntriesChanged;

            _visualCommands = new Dictionary<ConsoleCommandEntry, ConsoleVisualCommand>();
            foreach(var entry in group.Entries)
            {
                if (_visualCommands.ContainsKey(entry) == false)
                {
                    _visualCommands.Add(entry, CreateVisualEntry(entry, visualEntryPrefab));
                }
            }
        }

        private void OnFoldoutStateChanged(bool state)
        {
            Group.Foldout = state;
            FoldoutImageOn.gameObject.SetActive(state);
            FoldoutImageOff.gameObject.SetActive(!state);
            ContentTransform.gameObject.SetActive(state);
        }

        private void OnEntriesChanged()
        {
            //Adding commands that isn't in group yet
            foreach(var entry in Group.Entries)
            {
                if (_visualCommands.ContainsKey(entry) == false)
                {
                    _visualCommands.Add(entry, CreateVisualEntry(entry, _visualEntryPrefab));
                }
            }
            //Remove visual entries which is no longer in group
            List<ConsoleCommandEntry> entriesToRemove = new List<ConsoleCommandEntry>();
            foreach(var visualEntry in _visualCommands.Keys)
            {
                if (Group.Entries.Contains(visualEntry) == false)
                {
                    RemoveEntry(visualEntry);
                    entriesToRemove.Add(visualEntry);
                }
            }
            
            if (_visualView.ParamsModalView.IsOpen() 
                && entriesToRemove.Contains(_visualView.ParamsModalView.CurrentCommandEntry))
            {
                var visualCommand = _visualCommands[_visualView.ParamsModalView.CurrentCommandEntry];
                if (visualCommand == _visualView.ParamsModalView.CurrentVisualCommand)
                {
                    _visualView.CloseModalView();
                }
            }
            foreach(var entry in entriesToRemove)
            {
                _visualCommands.Remove(entry);
            }
            entriesToRemove.Clear();

            Reorder();
        }

        private void Reorder()
        {
            int siblingIndex = 0;
            if (Group.LIFO)
            {
                siblingIndex = _visualCommands.Count-1;
                foreach(var entry in Group.Entries)
                {
                    var visualCommand = _visualCommands[entry];
                    visualCommand.GetComponent<RectTransform>().SetSiblingIndex(siblingIndex);
                    siblingIndex--;
                }
            }
        }

        private ConsoleVisualCommand CreateVisualEntry(ConsoleCommandEntry entry, GameObject visualEntryPrefab)
        {
            var entryObject = GameObject.Instantiate(visualEntryPrefab);
            var entryTransform = entryObject.GetComponent<RectTransform>();
            entryTransform.SetParent(ContentTransform);
            entryTransform.localScale = Vector3.one;
            var visualEntry = entryObject.GetComponent<ConsoleVisualCommand>();
            visualEntry.Init(entry);
            visualEntry.OnClickedAction += delegate{
                _visualView.CommandClicked(visualEntry);
            };
            return visualEntry;
        }

        private void RemoveEntry(ConsoleCommandEntry entry)
        {
            var visualCommand = _visualCommands[entry];
            GameObject.Destroy(visualCommand.gameObject);
        }
    }
}