//------------------------------------------------------------------------------
// TDBaseNation.cs
// Copyright 2019 2019/5/16 
// Created by CYM on 2019/5/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    [Serializable]
    public class TDBaseNationData<TType, TSelf> : TDBaseAttrConfData<TType,TSelf>, ITDNationData 
        where TType : Enum
        where TSelf : TDBaseData
    {
        #region prop
        public string Capital
        {
            get
            {
                if (Castles == null || Castles.Count == 0)
                    return Const.STR_Inv;
                return Castles[0];
            }
        }
        public Color NationColor { get; private set; }
        #endregion

        #region lua config
        //文化
        public string Culture { get; set; } = "";
        //文明
        public string Civil { get; set; } = "";
        //旗帜
        public string Flag { get; set; } = "";
        //颜色
        public string Color { get; set; } = "#bbb004";
        //国姓
        public string LastName { get; set; } = "S_姬";
        //爵位
        public string RankTitle { get; set; } = "RankTitle_侯爵";
        //城市,第一位为首都
        public List<string> Castles { get; set; } = new List<string>();
        //特质
        public List<string> Traits { get; set; } = new List<string>();
        #endregion

        #region get
        string GetFlagID()
        {
            string flagID = Flag;
            if (flagID.IsInv()) flagID = TDID;
            return flagID;
        }
        //普通正方形的旗帜
        public Sprite GetFlag()
        {
            string flagID = GetFlagID();
            Sprite ret = null;
            if (ret == null)
                ret = GRMgr.Flag.Get(flagID, false);
            if (ret == null)
                ret = GRMgr.Flag.Get(Const.RES_FlagEmpty, false);
            return ret;
        }
        //圆形的旗帜
        public Sprite GetFlagCircle()
        {
            string flagID = GetFlagID();
            Sprite ret = null;
            if (ret == null)
                ret = GRMgr.Flag.Get(flagID + Const.Suffix_FlagCircle, false);
            if (ret == null)
                ret = GRMgr.Flag.Get(Const.RES_FlagEmpty + Const.Suffix_FlagCircle, false);
            return ret;
        }
        //长方形的旗帜
        public Sprite GetFlagSquare()
        {
            string flagID = GetFlagID();
            Sprite ret = null;
            if (ret == null)
                ret = GRMgr.Flag.Get(flagID + Const.Suffix_FlagSquare, false);
            if (ret == null)
                ret = GRMgr.Flag.Get(Const.RES_FlagEmpty + Const.Suffix_FlagSquare, false);
            return ret;
        }
        #endregion

        #region life
        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            NationColor = UIUtil.FromHex(Color);
        }
        #endregion
    }
}