//------------------------------------------------------------------------------
// TDBaseCastle.cs
// Created by CYM on 2021/11/6
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using System.Collections.Generic;

namespace NationWar
{
    public class TDBaseCastleData<TType, TSelf> : TDBaseAttrConfData<TType,TSelf>, ITDCastleData
        where TType : Enum
        where TSelf : TDBaseData
    {
        #region lua config
        public CastleType Type { get; set; } = CastleType.Town;
        public string Culture { get; set; } = "";
        public string Civil { get; set; } = "";
        #endregion

        #region is
        public bool IsTerritory
        {
            get
            {
                if (Type == CastleType.City || Type == CastleType.Town)
                    return true;
                return false;
            }
        }
        public bool IsBorough
        {
            get
            {
                if (Type == CastleType.Vill || Type == CastleType.Fort)
                    return true;
                return false;
            }
        }
        #endregion
    }
}