//------------------------------------------------------------------------------
// PersonTestView.cs
// Copyright 2019 2019/5/16 
// Created by CYM on 2019/5/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CYM.UI
{
    public class UPersonTestView : UUIView
    {
        #region Inspector
        [SerializeField]
        UDupplicate DPPerson;
        [SerializeField]
        UButton BntRefresh;
        #endregion

        #region life
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            BntRefresh.Init(new UButtonData { NameKey = "Refresh", IsTrans = false, OnClick = OnClickBntRefresh });
            DPPerson.Init(new UDupplicateData { FixedCount = 21 },()=>BaseGlobal.PersonTestMgr.Data, (tp, td) =>
            {
                UPHIItem p = tp as UPHIItem;
                ITDPersonData d = td as ITDPersonData;
                UPHIItemData data = new UPHIItemData() { Person = () => d };
                p.Refresh(data);
            });
        }

        protected override void OnOpen(UView baseView, bool useGroup)
        {
            base.OnOpen(baseView, useGroup);
            BaseGlobal.PersonTestMgr.RandTestPerson();
        }
        #endregion

        #region Callback
        private void OnClickBntRefresh(UControl arg1, PointerEventData arg2)
        {
            BaseGlobal.PersonTestMgr.RandTestPerson();
            DPPerson.SetDirtyRefresh();
        }
        #endregion
    }
}