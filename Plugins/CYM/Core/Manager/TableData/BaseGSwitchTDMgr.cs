//------------------------------------------------------------------------------
// BaseGSwitchTDMgr.cs
// Created by CYM on 2022/6/14
// 用于政体,宗教等唯一且带有切换功能的系统
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
   public class BaseGSwitchTDMgr<TData> : BaseGFlowMgr
        where TData : TDBaseData, new()
    {

    }
}