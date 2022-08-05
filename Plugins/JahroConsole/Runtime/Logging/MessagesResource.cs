using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole.Logging
{
    internal static class MessagesResource
    {
        public static readonly string LogCommandInvokeException = "Command <{0}> execution cause exception: {1}";

        public static readonly string LogCommandCastError = "Input for command <{0}> can not be mapped to parameters";

        public static readonly string LogCommandNotDefined = "Command <{0}> is not defined";

        public static readonly string LogCommandNameHasDublicate = "Command <{0}> has a duplicate";

        public static readonly string LogCommandNameHasSpacing = "Command <{0}> with space in naming does not work in text mode (FYI)";

        public static readonly string LogSavesLoadingError = "Failed to load saves";

        public static readonly string LogSavesParsingError = "Failed to parse saves";

        public static readonly string MailSubjectForSharing = "Jahro: Logs -> ";

        public static readonly string LogCommandUnsupportedParamters = "Unsupported parameter type: Parameter {0} of type {1}";

        public static readonly string LogCommandMonoObjectsNotFound = "Objects’s implemented command <{0}> not found";

        public static readonly string LogWelcomeMessage =// "<size=70><b><color=#0F0F11FF>Jahro\n</color></b></size>" +
                                                     "<color=#FF5C01><b>Command Visual Console\n</b></color>" +
                                                     "Made by CYM\n" +
                                                     //"Feel free to reach support - <i>support@jahro.io\n</i>" +
                                                     "Try <color=#17E96B>help</color> command for details;";
    }
}