using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole.Logging
{
    internal static class JahroEntriesCopyHelper
    {
        public static void CopyToClipboard(string data)
        {
            GUIUtility.systemCopyBuffer = data;
        }
    }
}