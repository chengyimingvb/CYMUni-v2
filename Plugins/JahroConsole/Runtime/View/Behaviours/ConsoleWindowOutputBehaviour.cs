using System;
using JahroConsole.Core.Data;
using UnityEngine;
using JahroConsole.Logging;

namespace JahroConsole.View
{
	internal class ConsoleWindowOutputBehaviour : MonoBehaviour
	{
		public JahroCommandsDataSource DataSource { get; private set; }

		internal FilterTogglesBehaviour FilterTogglesBehaviour;

		[SerializeField]
		internal JahroScrollView JahroScrollView;

		private void Update()
		{
			DataSource.Update();
		}

        public void Init(bool mobileMode)
        {
			DataSource = new JahroCommandsDataSource(JahroScrollView);
			FilterTogglesBehaviour = GetComponentInChildren<FilterTogglesBehaviour>();

			JahroLogger.OnLogEvent += OnLogEvent;
			JahroLogger.OnClearAllLogs += ClearAll;

			JahroScrollView.MobileMode = mobileMode;
			JahroScrollView.OnEntityCopy += CopyItem;
			JahroScrollView.OnEntityShare += ShareItem;
		}

		public void ScrollToBottom()
		{
			JahroScrollView.ScrollDown();
		}

		public void SelectMode(bool enabled)
		{
			if (!enabled)
			{
				DataSource?.ResetSelection();
			}
			JahroScrollView.SelectMode(enabled);
		}

		public void CopySelectedItems()
		{
			var items = DataSource.GetSelectedItems();
			if (items.Count > 0)
			{
				JahroEntriesCopyHelper.CopyToClipboard(JahroLogsFormatter.EntriesToReadableFormat(items));
			}
		}

		public void ShareSelectedItems()
		{
			var items = DataSource.GetSelectedItems();
			if (items.Count > 0)
			{
				JahroShareManager.Share(JahroLogsFormatter.EntriesToReadableFormat(items));
			}
		}

		public void SelectAll()
		{
			DataSource?.SelectAll();
		}

		private void CopyItem(JahroCommandEntity item)
		{
			JahroEntriesCopyHelper.CopyToClipboard(JahroLogsFormatter.EntryToReadableFormat(item));
		}

		private void ShareItem(JahroCommandEntity item)
		{
			JahroShareManager.Share(JahroLogsFormatter.EntryToReadableFormat(item));
		}

		public void SetFilter(bool showLogs, bool showWarnings, bool showErrors, bool showCommands)
		{
			DataSource?.SetFilter(showLogs, showWarnings, showErrors, showCommands);
		}

		public void SetFilter(string messageString)
		{
			DataSource?.SetFilter(messageString);
		}

		public void OnMainWindowRectChanged(Rect rect)
		{
			DataSource.UpdateReferenceSize(rect);
			JahroScrollView.UpdateOnScreenChanged();
        }

		private void ClearAll()
		{
			DataSource?.Clear();
		}

		private void OnLogEvent(string message, string context, EJahroLogType logType)
		{
			DataSource?.Append(message, context, logType);
		}

		private void OnDestroy()
		{
			JahroLogger.OnLogEvent -= OnLogEvent;
			JahroLogger.OnClearAllLogs -= ClearAll;
		}
	}
}