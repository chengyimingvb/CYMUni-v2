using System;
using System.Collections.Generic;
//**********************************************
// Class Name	: TDBuff
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    [Serializable]
    public class TDBaseAchieveData : TDBaseData
    {
        #region 属性
        public List<BaseTarget> Targets { get; set; } = new List<BaseTarget>();
        public bool State { get; set; } = false;
        public DateTime UnlockTime { get; set; }
        public string SourceName { get; set; }
        public string SourceDesc { get; set; }
        #endregion

        #region get
        // 获得总数
        public virtual int GetTotalCount()
        {
            return 0;
        }
        // 当前百分比
        public virtual float GetPercent()
        {
            if (State)
                return 1.0f;
            return 0.0f;
        }
        #endregion

    }
}
