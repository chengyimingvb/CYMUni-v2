//------------------------------------------------------------------------------
// BaseCommonUIMgr.cs
// Copyright 2021 2021/9/5 
// Created by CYM on 2021/9/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BaseCommonUIMgr : BaseUIMgr 
    {
        #region prop

        #endregion

        #region life
        protected override string ViewName => "CommonUI";
        #endregion

        public static UFadeView FadeView { get; private set; }

        protected override void OnCreateUIView1()
        {
            base.OnCreateUIView1();
            FadeView = CreateView<UFadeView>();
        }

        #region Callback
        protected override void OnAllLoadEnd1()
        {
            base.OnAllLoadEnd1();
            DoCreateView();
        }
        #endregion
    }
}