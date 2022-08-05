using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JahroConsole.Logging;
using JahroConsole.Core.Registry;

namespace JahroConsole.Core.Internal
{
    public static class JahroSystemCommands
    {
        private const string _groupName = "Jahro Built-In";


        [JahroCommand("clrscr", "Clean console view", _groupName)]
        public static void Clrscr()
        {
            JahroLogger.ClearAllLogs();
        }

        [JahroCommand("groups", "Show all groups", _groupName)]
        public static string ShowGroups()
        {
            string result = "";
            var groups = ConsoleCommandsRegistry.Holder.Groups;
            foreach(var group in groups)
            {
                result += group.Name;
                result += "\n";
                foreach(var commandName in group.GetCommandsNames())
                {
                    result += commandName + "; ";
                }
                result += "\n";
            }
            return result;
        }

        [JahroCommand("help", "Bring me some help", _groupName)]
        private static string HelpMain()
        {
            string help = "Help is on the way bro:\n" 
            + "Create a public static or private static method anywhere in your project.\n" 
            + "Put an attribute for this method - [JahroCommand(\"command name\", \"description\", \"group name\")]\n" 
            + "Use string returning value to output some info or just use to void return.\n\n" 
            + "What parameters can you use to those methods?\n" 
            + "Supported parameters are:\n" 
            + "- int\n" 
            + "- float\n" 
            + "- bool\n" 
            + "- double\n" 
            + "- string\n" 
            + "- Vector2\n" 
            + "- Vector3\n" 
            + "- array of types described above\n" 
            + "- Enums\n" 
            + "Use as many parameters as you can deal with!\n"
            + "How to enter value for command parameter? Just type them with spaces. Example: command value1 value2\n"
            + "Don't forget to check Tools -> Jahro Console section to get more control over the console.\n"; 
            return help;
        }
    }
}