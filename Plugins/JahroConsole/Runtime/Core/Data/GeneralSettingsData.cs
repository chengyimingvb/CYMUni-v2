using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    [Serializable]
    internal class GeneralSettingsData
    {
        [SerializeField]
        internal Vector2 WindowAnchoredPosition;
        [SerializeField]
        internal Vector2 WindowSize;
        [SerializeField]
        internal bool Fullscreen = true;
        [SerializeField]
        internal Vector2 OpenButtonPosition;
        [SerializeField]
        internal string Mode;
        [SerializeField]
        internal bool filterDebug = true;
        [SerializeField]
        internal bool filterWarning = true;
        [SerializeField]
        internal bool filterError = true;
        [SerializeField]
        internal bool filterCommands = true;
        [SerializeField]
        internal float scale = 0.75f;
    }
}