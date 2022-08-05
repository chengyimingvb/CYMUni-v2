using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JahroConsole
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class JahroCommandAttribute : Attribute
    {
        private readonly string methodName;
        private readonly string methodDescription;

        private readonly string groupName;

        /// <summary>
        /// Used to mark a command that will be used as a Command in Console and Visual mode.
        /// </summary>
        /// <param name="methodName">Used to identify the command.</param>
        /// <returns></returns>
        public JahroCommandAttribute(string methodName) : this(methodName, "", "")
        {

        }

        /// <summary>
        /// Used to mark a command that will be used as a Command in Console and Visual mode.
        /// </summary>
        /// <param name="methodName">Used to identify the command.</param>
        /// <param name="methodDescription">Adds a description to a command.</param>
        /// <returns></returns>
        public JahroCommandAttribute(string methodName, string methodDescription) : this(methodName, methodDescription, "")
        {

        }

        /// <summary>
        /// Used to mark a command that will be used as a Command in Console and Visual mode.
        /// </summary>
        /// <param name="methodName">Used to identify the command.</param>
        /// <param name="methodDescription">Adds a description to a command.</param>
        /// <param name="groupName">Assigns a command to a group it should be related to.</param>
        public JahroCommandAttribute(string methodName, string methodDescription, string groupName)
        {
            this.methodName = methodName;
            this.methodDescription = methodDescription;
            this.groupName = groupName;
        }

        internal string MethodName
        {
            get { return methodName; }
        }

        internal string MethodDescription
        {
            get { return methodDescription; }
        }

        internal string GroupName
        {
            get { return groupName; }
        }
    }
}