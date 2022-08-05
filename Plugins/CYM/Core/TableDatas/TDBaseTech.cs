//------------------------------------------------------------------------------
// TDBaseTech.cs
// Copyright 2019 2019/11/10 
// Created by CYM on 2019/11/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace CYM
{
    [Serializable]
    public class TDBaseTechData : TDBaseData
    {
        #region Config
        public UnlockData UnlockRight { get; set; }
        public UnlockData UnlockBottom { get; set; }
        public int MaxLevel { get; set; } = 1;
        #endregion

        #region runtime
        public int CurLevel { get; set; } = 0;
        public int NextLevel
        {
            get
            {
                if (IsMaxLevel())
                    return MaxLevel;
                return CurLevel + 1;
            }
        }
        public List<UnlockData> FrontTech { get; private set; } = new List<UnlockData>();
        #endregion

        #region is
        public bool IsHaveLevel()
        {
            return CurLevel > 0;
        }
        public bool IsMaxLevel()
        {
            return CurLevel >= MaxLevel;
        }
        public bool IsLinkRight()
        {
            return UnlockRight != null;
        }
        public bool IsLinkBottom()
        {
            return UnlockBottom != null;
        }
        public bool IsUnLockRight()
        {
            if (UnlockRight == null) return true;
            return CurLevel >= UnlockRight.Level;
        }
        public bool IsUnLockBottom()
        {
            if (UnlockBottom == null) return true;
            return CurLevel >= UnlockBottom.Level;
        }
        #endregion
    }

    public class TDBaseGlobalTech<T, TConfig> : TDBaseConfig<T>
        where T : TDBaseTechData, new()
        where TConfig : TDBaseGlobalTech<T, TConfig>
    {
        public override void OnLuaParseEnd()
        {
            base.OnLuaParseEnd();
            foreach (var tech in ListValues)
            {
                //设置科技链
                if (tech.UnlockBottom != null)
                {
                    var tempTech = Get(tech.UnlockBottom.TDID);
                    tempTech.FrontTech.Add(new UnlockData { TDID = tech.TDID, Level = tech.UnlockBottom.Level });
                }
                if (tech.UnlockRight != null)
                {
                    var tempTech = Get(tech.UnlockRight.TDID);
                    tempTech.FrontTech.Add(new UnlockData { TDID = tech.TDID, Level = tech.UnlockRight.Level });
                }
            }
        }
    }
}