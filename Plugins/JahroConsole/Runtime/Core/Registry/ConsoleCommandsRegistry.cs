using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;
using JahroConsole.Core.Data;
using JahroConsole.Logging;
using JahroConsole.View;

namespace JahroConsole.Core.Registry
{
    internal static class ConsoleCommandsRegistry
    {
        internal static ConsoleCommandHolder Holder { get { return _holder; } }

        internal static bool Initialized { get; private set; }

        private static ConsoleCommandHolder _holder = new ConsoleCommandHolder();

        internal static void Initialize(ProjectSettings projectSettings)
        {
            var methods = RetrieveMethods(projectSettings);
            InitializeConsoleCommands(methods);
            Jahro.LogDebug("Jahro Console: Initialized");
        }

        private static List<MethodInfo> RetrieveMethods(ProjectSettings projectSettings)
        {
            
            List<MethodInfo> methods = new List<MethodInfo>();
            var allAssmblies = System.AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in allAssmblies)
            {
                if (projectSettings.ActiveAssemblies.Contains(assembly.GetName().Name))
                {
                    var methodsArray = assembly.GetTypes()
                        .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                        .Where(m => m.GetCustomAttributes(typeof(JahroCommandAttribute), false).Length > 0);
                    methods.AddRange(methodsArray);

                    var methodsMonoArray = assembly.GetTypes()
                        .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.DeclaredOnly))
                        .Where(m => m.GetCustomAttributes(typeof(JahroCommandAttribute), false).Length > 0);
                   
                    methods.AddRange(methodsMonoArray);
                }
            }
            return methods;
        }

        private static void InitializeConsoleCommands(List<MethodInfo> methods)
        {
            foreach (var methodInfo in methods)
            {
                var attribute = GetAttribute(methodInfo);
                Holder.AddCommandMethod(attribute, methodInfo);
            }

            ConsoleStorageController.LoadState();
            Holder.Initialize(ConsoleStorageController.Instance.ConsoleStorage);
            Initialized = true;
        }

        internal static void InvokeCommand(ConsoleCommandEntry targetEntry, object[] entryParams)
        {
            if (targetEntry.MethodInfo.IsStatic)
            {
                InvokeCommandForStaticObject(targetEntry, entryParams);
            }
            else
            {
                InvokeCommandForMultipleObjects(targetEntry, entryParams);
            }
        }

        internal static void InvokeCommand(string name, string[] args)
        {
            var entries = Holder.GetCommandEntries(name, args);
            if (entries.Count > 0)
            {
                ConsoleCommandEntry targetEntry = null;
                var entryParams = ConsoleCommandsParamsMapper.MapParams(entries, args, out targetEntry);
                if (targetEntry == null)
                {
                    Jahro.LogError(string.Format(MessagesResource.LogCommandCastError, name));
                }
				else
				{
                    if (targetEntry.MethodInfo.IsStatic)
                    {
                        InvokeCommand(targetEntry, entryParams);
                    }
                    else
                    {
                        InvokeCommandForMultipleObjects(targetEntry, entryParams);
                    }
				}
            }
            else
                Jahro.LogError(string.Format(MessagesResource.LogCommandNotDefined, name));
        }

        internal static void InvokeCommandForStaticObject(ConsoleCommandEntry targetEntry, object[] entryParams)
        {
            object result = null;

            try
            {
                result = targetEntry.MethodInfo.Invoke(typeof(ConsoleCommandsRegistry), entryParams);
            }
            catch (Exception e)
            {
                Jahro.LogException(e.Message, e);
            }
            finally
            {
                if (result != null)
                    JahroLogger.LogCommand(targetEntry.Name, entryParams, result.ToString());
                targetEntry.Executed(entryParams);
            }
        }

        internal static void InvokeCommandForMultipleObjects(ConsoleCommandEntry targetEntry, object[] entryParams)
        {
            List<object> results = new List<object>();
            var entryObjects = UnityEngine.GameObject.FindObjectsOfType(targetEntry.MethodInfo.DeclaringType);

            if (entryObjects.Length == 0)
            {
                Jahro.LogError(string.Format(MessagesResource.LogCommandMonoObjectsNotFound, targetEntry.Name));
                return;
            }

            try
            {
                if (entryObjects != null)
                {       
                    foreach (var entryObject in entryObjects)
                    {
                        var result = targetEntry.MethodInfo.Invoke(entryObject, entryParams);
                        results.Add(result);
                    }
                }
            }
            catch (Exception e)
            {
                Jahro.LogException(e.Message, e);
            }
            finally
            {
                JahroLogger.LogCommand(targetEntry.Name, entryParams, results);
                targetEntry.Executed(entryParams);
            }

        }

        private static JahroCommandAttribute GetAttribute(MethodInfo info)
        {
            return info.GetCustomAttributes(typeof(JahroCommandAttribute), false)[0] as JahroCommandAttribute;
        }
    }
}
