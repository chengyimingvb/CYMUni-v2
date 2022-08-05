using System.Collections.Generic;
using JahroConsole.View;
using UnityEngine;

namespace JahroConsole.Core.Data
{
    internal partial class JahroCommandsDataSource
    {
        public const int DEFAULT_SCROLL_CAPACITY = 1000;
        private const int DEFAULT_ITEM_SIZE = 20;
        private const int SEPARATORS_FOR_PORTRAIGHT = 9;
        private const int SEPARATORS_FOR_LANDSCAPE = 2;

        private List<JahroCommandEntity> allCommandsCache = new List<JahroCommandEntity>(DEFAULT_SCROLL_CAPACITY);
        private List<JahroCommandEntity> filteredCommandsCache = new List<JahroCommandEntity>(DEFAULT_SCROLL_CAPACITY);
        private List<JahroCommandEntity> commandsBatch = new List<JahroCommandEntity>(DEFAULT_SCROLL_CAPACITY);
        private JahroDatasourceFilterSettings filterSettings = new JahroDatasourceFilterSettings();
        private ConsoleCommandsFilter filter; 
        private JahroScrollView jahroScrollView;
        private JahroCommandsDataSourceCounter counter = new JahroCommandsDataSourceCounter();
    }
}