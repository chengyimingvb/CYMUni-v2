using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using JahroConsole;
using System.Text;

namespace JahroConsole.Core.Registry
{
    internal class ConsoleCommandEntry
    {
        internal string Name { get; private set; }

        internal string Description { get; private set; }

        internal MethodInfo MethodInfo { get; private set; }

        internal string SimpleName { get; private set; }

        internal bool Favorite { get; private set; } 

        internal object[] LatestParams { get; set; }

        internal Action<bool> FavoritesStateChanged;

        internal Action OnExecuted;

        internal ConsoleCommandEntry(JahroCommandAttribute attribute)
        {
            Name = attribute.MethodName.Trim();
            Description = attribute.MethodDescription.Trim(); 
        }

        internal void SetMethodInfo(MethodInfo methodInfo)
        {
            MethodInfo = methodInfo;
            SimpleName = GetSimpleName();
            LatestParams = GetParametersDefaultValues();
        }

        internal void SetFavorite(bool favorite)
        {
            if (Favorite != favorite)
            {
                FavoritesStateChanged(favorite);
            }
            Favorite = favorite;
        }

        internal void SetFavorite(bool favorite, bool notify)
        {
            if (Favorite != favorite && notify)
            {
                FavoritesStateChanged(favorite);
            }
            Favorite = favorite;
        }

        internal void Executed(object[] parameters)
        {
            LatestParams = parameters;
            OnExecuted();
        }

        internal string GetSimpleName()
        {
            string result = Name;
            var parameters = MethodInfo.GetParameters();
            foreach(var p in parameters)
            {
                result += "|" + p.ParameterType.Name;
            }
            return result;
        }

        internal string GetReadableParameters()
        {
            StringBuilder sb = new StringBuilder();
            
            var parameters = MethodInfo.GetParameters();
            if (parameters.Length > 0)
            {
                sb.Append("(");
            }
            else
            {
                return string.Empty;
            }

            for (int i=0; i<parameters.Length; i++)
            {
                sb.Append(parameters[i].ParameterType.Name.ToLower());
                if (i + 1 < parameters.Length)
                {
                    sb.Append(",");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }

        private object[] GetParametersDefaultValues()
        {
            var parametersInfo = MethodInfo.GetParameters();
            if (parametersInfo.Length == 0)
            {
                return null;
            }

            object[] defaultValues = new object[parametersInfo.Length];
            for (int i=0; i<parametersInfo.Length; i++)
            {
                var parameter = parametersInfo[i];
                if (parameter.HasDefaultValue)
                {
                    defaultValues[i] = parameter.DefaultValue;
                }
            }
            return defaultValues;
        }
    }
}