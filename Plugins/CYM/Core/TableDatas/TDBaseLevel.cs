//------------------------------------------------------------------------------
// TDBaseLevel.cs
// Copyright 2021 2021/9/5 
// Created by CYM on 2021/9/5
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;

namespace CYM
{
    [Serializable]
    public class TDBaseLevelData : TDBaseData
    {
        #region mgr
        DBBaseSettings SettingData => BaseGlobal.SettingsMgr.Settings;
        #endregion

        public string SceneName { get; set; } = Const.STR_Inv;
        public virtual string GetSceneName()
        {
            var ret = SceneName;
            if (ret.IsInv())
                ret = TDID.Replace(Const.Prefix_Battle, "");
            if (SettingData.IsSimpleTerrin)
                ret = ret + Const.Suffix_Simple;
            return ret;
        }
        public string GetRawSceneName()
        {
            var ret = SceneName;
            if (ret.IsInv())
                ret = TDID.Replace(Const.Prefix_Battle, "");
            return ret;
        }

    }
}