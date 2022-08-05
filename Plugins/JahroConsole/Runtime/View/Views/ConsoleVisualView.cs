using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using JahroConsole.Core.Registry;
using UnityEngine.UI;

namespace JahroConsole.View
{
    internal class ConsoleVisualView : ConsoleBaseView
    {
        public Transform ContentRoot;

        public GameObject GroupLayoutPrefab;

        public GameObject VisualCommandPrefab;

        public RectTransform ModalViewHolderTransform;

        public RectTransform Viewport;

        public ParamsModalView ParamsModalView { get; private set; }

        private ConsoleCommandHolder _commandsHolder;

        private VerticalLayoutGroup _contentLayoutGroup;

        public void Start()
        {
            _commandsHolder = ConsoleCommandsRegistry.Holder;    
            _contentLayoutGroup = ContentRoot.GetComponent<VerticalLayoutGroup>();
            ParamsModalView = ModalViewHolderTransform.GetComponentInChildren<ParamsModalView>(true);

            BuildGroups();

            RefreshSafeArea();
        }

        public void CloseModalView()
        {
            if (ParamsModalView != null)
            {
                ParamsModalView.Close();
            }
        }

        private void BuildGroups()
        {
            foreach(var group in _commandsHolder.Groups)
            {
                CreateGroup(group);
            }
        }

        public void CommandClicked(ConsoleVisualCommand visualCommand)
        {
            ParamsModalView.Open(visualCommand, MainWindow, Viewport);
        }

        protected override void OnActivate()
        {
            
        }

        protected override void OnDeactivate()
        {
            CloseModalView();
        }

        internal override void OnWindowRectChanged(Rect rect)
        {
            base.OnWindowRectChanged(rect);
            if (ParamsModalView != null)
            {
                ParamsModalView.Close();
            }
        }

        protected override void OnSafeAreaChanged(Rect safeArea, float scaleFactor)
        {
            base.OnSafeAreaChanged(safeArea, scaleFactor);

            RefreshSafeArea();
        }

        private void RefreshSafeArea()
        {
            int leftPadding = (int)Mathf.Max(SafeArea.x/ScaleFactor, 0);
            int rightPadding = (int)Mathf.Max((Screen.width - (SafeArea.x + SafeArea.width))/ScaleFactor, 0);
            if (_contentLayoutGroup != null)
            {
                _contentLayoutGroup.padding = new RectOffset(leftPadding, rightPadding, 0, 0);
            }
        }

        private ConsoleGroupLayout CreateGroup(SimpleGroup group)
        {
            var groupLayoutObject = GameObject.Instantiate(GroupLayoutPrefab);
            var groupLayoutTransform = groupLayoutObject.GetComponent<RectTransform>();
            groupLayoutTransform.SetParent(ContentRoot);
            groupLayoutTransform.localScale = Vector3.one;
            var groupLayout = groupLayoutObject.GetComponent<ConsoleGroupLayout>();
            groupLayout.Init(group, VisualCommandPrefab, this);
            return groupLayout;
        }
    }
}