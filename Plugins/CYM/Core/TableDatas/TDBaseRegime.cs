//------------------------------------------------------------------------------
// TDBaseRegime.cs
// Created by CYM on 2022/6/14
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using System.Collections.Generic;

namespace CYM
{
    [Serializable]
    public class TDBaseRegimeData : TDBaseData
    {
        #region Config
        //是否拥有王后
        public bool IsHaveQueue { get; set; } = true;
        //任期,0表示终身执政
        public int MaxTerm { get; set; } = -1;
        //UI模板
        public string UITemp { get; set; } = "";
        //最大的继承人数量
        public Func<int> MaxPrince = ()=> 4;
        //最大美人数量
        public Func<int> MaxBeauty = ()=> 3;
        //最大候选人数量
        public Func<int> MaxCandidate = () => 3;
        //官员
        public List<string> Officials { get; set; } = new()
        {
            "Official_太宰",
            "Official_大司马",
            "Official_大司空",
            "Official_大司寇",
            "Official_大司农",
        };
        #endregion

        #region Runtime
        public int CurTerm { get; set; } = 0;
        #endregion

        #region Set
        public void ResetTerm()
        {
            CurTerm = 0;
        }
        //开局产生多个继承人
        public void GeneratePrinces()
        {
            var count = RandUtil.Range(1, MaxPrince());
            for (int i = 0; i < count; ++i)
            {
                ActionPrince?.Invoke(this,true);
            }
        }
        //开局产生多个候选人
        public void GenerateCandidates()
        {
            var count = RandUtil.Range(1, MaxCandidate());
            for (int i = 0; i < count; ++i)
            {
                ActionCandidate?.Invoke(this, true);
            }
        }
        //开局产生多个美人
        public void GenerateBeautys()
        {
            var count = RandUtil.Range(1, MaxBeauty());
            for (int i = 0; i < count; ++i)
            {
                ActionBeauty?.Invoke(this, true);
            }
        }
        //产生君主
        public void GenerateMonarch()
        {
            ActionMonarch?.Invoke(this);
        }
        //产生继承人
        public void GeneratePrince()
        {
            ActionPrince?.Invoke(this,false);
        }
        //产生美人
        public void GenerateBeauty()
        {
            ActionBeauty?.Invoke(this,false);
        }
        //产生候选人
        public void GenerateCandidate()
        {
            ActionCandidate?.Invoke(this,false);
        }
        //君主登基
        public void Throne()
        {
            ActionThrone?.Invoke(this);
        }
        #endregion

        #region Action
        //开局产生君主
        public Callback<TDBaseRegimeData> ActionMonarch = (x) => { };
        //新的君主登基
        public Callback<TDBaseRegimeData> ActionThrone = (x) => { };
        //单次产生继承人
        public Callback<TDBaseRegimeData,bool> ActionPrince = (x,b) => { };
        //产生新的美人
        public Callback<TDBaseRegimeData, bool> ActionBeauty = (x, b) => { };
        //产生新的候选人
        public Callback<TDBaseRegimeData, bool> ActionCandidate = (x, b) => { };
        #endregion

    }
}