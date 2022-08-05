using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using JahroConsole.Logging;

namespace JahroConsole.Core.Data
{
    internal class ConsoleCommandsFilter
    {
        private List<JahroCommandEntity> filteredList = new List<JahroCommandEntity>(JahroCommandsDataSource.DEFAULT_SCROLL_CAPACITY);
        private bool isFiltering;
        private bool isFilteringFinished;
        private JahroDatasourceFilterSettings filterSettings;
        public event Action<List<JahroCommandEntity>> FilteringFinished; 

        public ConsoleCommandsFilter(JahroDatasourceFilterSettings settings)
        {
            filterSettings = settings;
        }

        public void Update()
        {
            if (isFiltering && isFilteringFinished)
            {
                FilteringFinished?.Invoke(new List<JahroCommandEntity>(filteredList));
                isFiltering = false;
                filteredList.Clear();
            }
        }
        
        public void FilterCommands(List<JahroCommandEntity> unfiltered)
        {
            ThreadPool.QueueUserWorkItem(PerformFiltering, unfiltered);
        }

        private void PerformFiltering(object unfiltered)
        {
            isFiltering = true;
            isFilteringFinished = false;
            filteredList.Clear();
            var unfilteredList = unfiltered as List<JahroCommandEntity>;
            if (unfilteredList == null)
            {
                throw new InvalidDataException("Data seems to be corrupted");
            }

            foreach (var jahroCommandEntity in unfilteredList)
            {
                if (IsCommandCorrespondsToFilter(jahroCommandEntity))
                {
                    filteredList.Add(jahroCommandEntity);
                }
            }

            isFilteringFinished = true;
        }

        public bool IsCommandCorrespondsToFilter(JahroCommandEntity command)
        {
            var messageAtIndexType = command.LogType;

            if (messageAtIndexType == EJahroLogType.JahroInfo)
            {
                return true;
            }

            if (!filterSettings.ShowErrors && (messageAtIndexType == EJahroLogType.Assert || messageAtIndexType == EJahroLogType.Error
                || messageAtIndexType == EJahroLogType.Exception))
            {
                return false;
            }

            if (!filterSettings.ShowWarnings && messageAtIndexType == EJahroLogType.Warning)
            {
                return false;
            }

            if (!filterSettings.ShowLogs && (messageAtIndexType == EJahroLogType.Log))
            {
                return false;
            }

            if (!filterSettings.ShowJahroLogs && (messageAtIndexType == EJahroLogType.JahroCommand ||
                messageAtIndexType == EJahroLogType.JahroDebug ||
                messageAtIndexType == EJahroLogType.JahroError ||
                messageAtIndexType == EJahroLogType.JahroException ||
                messageAtIndexType == EJahroLogType.JahroWarning))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(filterSettings.SearchString) && (!command.Message.Contains(filterSettings.SearchString)
                                                                       && !command.StackTrace.Contains(filterSettings.SearchString)))
            {
                return false;
            }
            
            return true;
        }
    }
}