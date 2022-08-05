//------------------------------------------------------------------------------
// UTMP.cs
// Created by CYM on 2021/9/25
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector;
using TMPro;
using System;

namespace CYM.UI
{
    public class UTMPData : UData
    {
        public bool IsTrans = true;

        #region 函数
        public Func<string> Name = null;
        #endregion

        #region 简化设置
        public string DefaultStr = Const.STR_Inv;
        public string NameKey = Const.STR_Inv;
        #endregion

        #region get
        public string GetName()
        {
            string dynStr = Const.STR_Inv;
            string staStr = NameKey;
            if (Name != null)
            {
                dynStr = Name.Invoke();
            }
            if (staStr.IsInv() && dynStr.IsInv())
            {
                return DefaultStr;
            }
            if (!dynStr.IsInv())
            {
                return dynStr;
            }
            else return GetTransStr(staStr);
        }
        public string GetTransStr(string str)
        {
            if (IsTrans)
                return BaseLangMgr.Get(str);
            return str;
        }
        #endregion
    }
    [AddComponentMenu("UI/Control/UTMP")]
    [HideMonoScript]
    public class UTMP : UPres<UTMPData>
    {
        #region 组建
        [FoldoutGroup("Inspector"), SerializeField, SceneObjectsOnly, Tooltip("可以位空")]
        public TextMeshProUGUI IName;
        #endregion

        #region life
        public override bool IsAtom => true;
        public override void Refresh()
        {
            base.Refresh();
            if (IName != null)
            {
                NameText = Data.GetName();
            }
        }
        #endregion

        #region wrap
        public string NameText
        {
            get { return IName.text; }
            set { IName.text = value; }
        }
        #endregion

        #region inspector
        public override void AutoSetup()
        {
            base.AutoSetup();
            if (IName == null)
                IName = GetComponent<TextMeshProUGUI>();
        }
        #endregion
    }
}