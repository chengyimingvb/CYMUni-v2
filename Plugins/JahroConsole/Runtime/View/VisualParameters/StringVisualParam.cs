using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class StringVisualParam : BaseVisualParam
    {

        private InputField _inputField;

        public override void Init(string paramName, object defaultParam)
        {
            base.Init(paramName, defaultParam);
            _inputField = GetComponentInChildren<InputField>();
            
            if (defaultParam == null)
            {
                _inputField.text = string.Empty;
            }
            else
            {
                _inputField.text = (string)defaultParam;
            }
            // _inputField.textComponent.UpdateMeshPadding();
        }

        public override object GetResult()
        {
            return _inputField.text;
        }

    }
}