using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using JahroConsole.Core.Registry;

namespace JahroConsole.View
{
    internal class ConsoleVisualCommand : MonoBehaviour
    {
        internal ConsoleCommandEntry CommandEntry { get; private set; }

        private Text _nameText;

        public Action OnClickedAction = delegate {};

        internal void Init(ConsoleCommandEntry commandEntry)
        {
            CommandEntry = commandEntry;

            _nameText = GetComponentInChildren<Text>();
            SetName(CommandEntry.Name);

            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        private void OnClicked()
        {
            OnClickedAction();
        }

        private void SetName(string name)
        {
            _nameText.text = name;
        }
    }
}