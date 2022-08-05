﻿using System.Collections.Generic;
using System.Linq;
using JahroConsole.View;
using UnityEngine;
using UnityEngine.UI;
using JahroConsole.Logging;

namespace JahroConsole.Core.Data
{
    internal partial class JahroCommandsDataSource
    {
        internal JahroCommandsDataSource(JahroScrollView scrollView)
        {
            jahroScrollView = scrollView;
            filter = new ConsoleCommandsFilter(filterSettings);
            filter.FilteringFinished += OnFilteringFinished;
            
            var separatorsCount = Screen.height > Screen.width ? SEPARATORS_FOR_PORTRAIGHT : SEPARATORS_FOR_LANDSCAPE;
            for (int i = 0; i < separatorsCount; i++)
            {
                allCommandsCache.Insert(0,new JahroCommandEntity(string.Empty, string.Empty, EJahroLogType.JahroInfo));
            }
            allCommandsCache.Insert(separatorsCount,new JahroCommandEntity(MessagesResource.LogWelcomeMessage, string.Empty, EJahroLogType.JahroInfo));

            jahroScrollView.OnGetHeightForIndexPath += ScrollOnGetOnGetHeightForIndexPath;
            jahroScrollView.OnFillAtIndexPath += OnFillAtIndexPath;
            jahroScrollView.OnItemExpanded += OnItemExpanded;
            jahroScrollView.OnResetExpand += OnResetExpand;
			
            var text = jahroScrollView.Prefab.GetComponent<JahroScrollItem>();
            TextHeightCaluculator.Instance.SetTextComponents(text.ConsoleCommandTextComponent, text.ConsoleDetailsTextComponent);
        }

        private void OnFilteringFinished(List<JahroCommandEntity> obj)
        {
            filteredCommandsCache = obj;
            ResetScroller();
        }

        public void Update()
        {
            var shouldScrollDown = false;
            if (commandsBatch.Count > 0)
            {
                shouldScrollDown = commandsBatch.Any(x => x.LogType == EJahroLogType.JahroCommand);
                allCommandsCache.AddRange(commandsBatch);
                filter.FilterCommands(allCommandsCache);
                commandsBatch.Clear();
            }
            filter.Update();
            if (shouldScrollDown)
            {
                jahroScrollView.ScrollDown();
            }
        }

        public void SelectAll()
        {
            foreach (var item in filteredCommandsCache)
            {
                if (item.Selectable)
                {
                    item.Selected = true;
                }
            }
            jahroScrollView.SelectAll();
        }

        public void ResetSelection()
        {
            foreach (var item in filteredCommandsCache)
            {
                item.Selected = false;
            }
        }

        public List<JahroCommandEntity> GetSelectedItems()
        {
            List<JahroCommandEntity> items = new List<JahroCommandEntity>();
            foreach (var item in filteredCommandsCache)
            {
                if (item.Selected)
                {
                    items.Add(item);
                }
            }
            return items;
        }

        public JahroCommandsDataSourceCounter GetCounter()
        {
            return counter;
        }

        public void UpdateReferenceSize(Rect rect)
        {
            TextHeightCaluculator.Instance.UpdateReferenceSize(rect.width-80);
        }
        
        private void OnFillAtIndexPath(int arg1, JahroScrollItem item)
        {
            item.gameObject.SetActive(true);
            item.SetUp(filteredCommandsCache[arg1], filterSettings.SearchString, jahroScrollView.IsSelectMode);
        }
        
        private int ScrollOnGetOnGetHeightForIndexPath(int index)
        {
            return GetHeightAtIndexPath(index);
        }

        private void OnItemExpanded(JahroScrollItem item)
        {
            
        }

        private void OnResetExpand()
        {
            
        }

        public void Append(string message, string context, EJahroLogType logType)
        {
            commandsBatch.Add(new JahroCommandEntity(message, context, logType));
            counter.AppendLog(logType);
        }

        public void Clear()
        {
            allCommandsCache.Clear();
            filter.FilterCommands(allCommandsCache);
            counter.Clear();
        }
    }    
}