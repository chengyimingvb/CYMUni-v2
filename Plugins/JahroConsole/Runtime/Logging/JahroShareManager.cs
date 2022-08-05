using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace JahroConsole.Logging
{
    internal static class JahroShareManager
    {
        public static void Share(string data)
        {
            var resultData = RemoveTags(data);
            string subject = UnityWebRequest.EscapeURL(MessagesResource.MailSubjectForSharing + " \" " + DateTime.Now.ToString() + "\"!").Replace("+", "%20");
            string body = UnityWebRequest.EscapeURL(resultData).Replace("+", "%20");
            
            Application.OpenURL("mailto:" + "?subject=" + subject + "&body=" + body);
        }

        private static string[] tags = new string[] { "align","alpha","color","b","i","cspace","font","indent","line-height","line-indent","link","lowercase",
            "uppercase","smallcaps","margin","mark","mspace","noparse","nobr","page","pos","size","space","sprite","s","u","style","sub","sup","voffset","width"};
        private static string RemoveTags(string source)
        {
            var result = source;
            foreach (var tag in tags)
            {
                string regTagStartEx = string.Format("<{0}[^>]*>*", tag);
                Regex reg = new Regex(regTagStartEx, RegexOptions.IgnoreCase);
                result = reg.Replace(result, "");

                string regTagEndEx = string.Format("</{0}>*", tag);
                reg = new Regex(regTagEndEx, RegexOptions.IgnoreCase);
                result = reg.Replace(result, "");

            }
            return result;
        }
    }
}