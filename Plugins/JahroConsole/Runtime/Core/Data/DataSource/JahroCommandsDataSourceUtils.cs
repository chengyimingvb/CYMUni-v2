using JahroConsole.View;
using UnityEngine;
using JahroConsole.Logging;

namespace JahroConsole.Core.Data
{
    internal partial class JahroCommandsDataSource
    {
        public void SetFilter(string searchedString)
        {
            filterSettings.SearchString = searchedString;
            filter.FilterCommands(allCommandsCache);
        }
        
        public void SetFilter(bool showLog, bool showWarning, bool showError, bool showJahroLogs)
        {
            filterSettings.ShowLogs = showLog;
            filterSettings.ShowErrors = showError;
            filterSettings.ShowWarnings = showWarning;
            filterSettings.ShowJahroLogs = showJahroLogs;
            filter.FilterCommands(allCommandsCache);
        }

        private void ResetScroller()
        {
            jahroScrollView.UpdateData(filteredCommandsCache.Count);
        }

        private int GetHeightAtIndexPath(int index)
        {
            var item = filteredCommandsCache[index];
            var str = item.Message;
            if (string.IsNullOrEmpty(str))
            {
                return 1;
                //return Mathf.RoundToInt(DEFAULT_ITEM_SIZE);
            }
            if (str == MessagesResource.LogWelcomeMessage)
            {
                return 50;
            }

            var resultingHeight = TextHeightCaluculator.Instance.GetMainTextHeight(str);

            if (item.Expanded)
            {
                resultingHeight += TextHeightCaluculator.Instance.GetDetailsTextHeight(item.StackTrace);
            }
            return resultingHeight;
        }
    }
}