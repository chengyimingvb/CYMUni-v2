using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using JahroConsole.Core.Registry;

namespace JahroConsole.View
{
    internal class AutocompliteCommandEntry : MonoBehaviour
    {
        private const string JAHRO_COMMAND_PARAMETERS = "<color=#6A6A6A>{0}</color>";

        private Text _textComponent;

        private ConsoleCommandEntry _currentCommand;

        private Action<ConsoleCommandEntry> _onSubmitAction;

        private void Awake()
        {
            _textComponent = GetComponentInChildren<Text>();
            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        public void Init(Action<ConsoleCommandEntry> onSubmitAction)
        {
            _onSubmitAction = onSubmitAction;         
        }

        public void Show(ConsoleCommandEntry entry)
        {
            _currentCommand = entry;
            string text = _currentCommand.Name + string.Format(JAHRO_COMMAND_PARAMETERS, _currentCommand.GetReadableParameters());
            _textComponent.text = text;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_textComponent.GetComponent<RectTransform>());
            this.gameObject.SetActive(true);
        }

        public void Hide()
        {
            this.gameObject.SetActive(false);
        }

        private void OnClicked()
        {
            _onSubmitAction.Invoke(_currentCommand);
        }
    }
}