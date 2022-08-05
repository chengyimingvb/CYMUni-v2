//------------------------------------------------------------------------------
// TDBaseOfficial.cs
// Created by CYM on 2022/6/17
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
    public class TDBaseOfficialData : TDBaseData
    {

    }

    public class TDBaseOfficial<TData, TConfig> : TDBaseGlobalConfig<TData, TConfig>
        where TData : TDBaseOfficialData, new()
        where TConfig : TDBaseConfig<TData>, new()
    {
        public TDBaseOfficial() : base()
        {
            BaseGlobal.TDOfficial = this;
        }
    }
}