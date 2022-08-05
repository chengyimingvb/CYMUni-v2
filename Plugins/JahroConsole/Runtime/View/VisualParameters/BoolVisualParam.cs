using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using JahroConsole.Core.Registry;

namespace JahroConsole.View
{
    internal class BoolVisualParam : BaseVisualParam
    {

        public Image ImageOn;

        public Image ImageOff;

        private Toggle _toggle;

        public override void Init(string paramName, object defaultParam)
        {
            base.Init(paramName, defaultParam);

            _toggle = GetComponentInChildren<Toggle>();
            _toggle.onValueChanged.AddListener(onValueChanged);
            
            if (defaultParam == null)
            {
                _toggle.isOn = false;
            }
            else
            {
                _toggle.isOn = TypesConverter.ToBool(defaultParam);
            }
            onValueChanged(_toggle.isOn);
        }

        public override object GetResult()
        {
            return _toggle.isOn;
        }

        private void onValueChanged(bool value)
        {
            ImageOn.gameObject.SetActive(!value);
            ImageOff.gameObject.SetActive(value);
        }
    }
}
