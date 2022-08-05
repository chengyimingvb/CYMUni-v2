//------------------------------------------------------------------------------
// PersonTipView.cs
// Copyright 2019 2019/6/7 
// Created by CYM on 2019/6/7
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM;
using CYM.UI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UStaffTipView : UTooltipView
    {
        #region Inepstor
        [FoldoutGroup("Attr"), SerializeField]
        UText Name;
        [FoldoutGroup("Attr"), SerializeField]
        UText Desc;
        [FoldoutGroup("Attr"), SerializeField]
        UText 忠诚;
        [FoldoutGroup("Attr"), SerializeField]
        UText 势力;
        [FoldoutGroup("Attr"), SerializeField]
        UText 行政;
        [FoldoutGroup("Attr"), SerializeField]
        UText 谋略;
        [FoldoutGroup("Attr"), SerializeField]
        UText 肉搏;
        [FoldoutGroup("Attr"), SerializeField]
        UText 射击;
        [FoldoutGroup("Attr"), SerializeField]
        UText 冲击;
        [FoldoutGroup("Attr"), SerializeField]
        UText 战术;
        [FoldoutGroup("Attr"), SerializeField]
        UText Age;
        [FoldoutGroup("Attr"), SerializeField]
        UText Job;
        #endregion

        #region life
        TDBaseStaffData LastPerson;
        public void Show(TDBaseStaffData person)
        {
            LastPerson = person;
            Show(true, true, true);
        }
        public override void Refresh()
        {
            base.Refresh();
            if (LastPerson == null) return;
            if (Name) Name.NameText = LastPerson.GetName();
            if (忠诚) 忠诚.NameText = LastPerson.RealLoyalty.ToString();
            if (势力) 势力.NameText = LastPerson.RealPower.ToString();
            if (行政) 行政.NameText = LastPerson.RealAdm.ToString();
            if (谋略) 谋略.NameText = LastPerson.RealInte.ToString();
            if (肉搏) 肉搏.NameText = LastPerson.RealShock.ToString();
            if (射击) 射击.NameText = LastPerson.RealFire.ToString();
            if (冲击) 冲击.NameText = LastPerson.RealImpact.ToString();
            if (战术) 战术.NameText = LastPerson.RealTactic.ToString();
            if (Desc) Desc.NameText = LastPerson.GetEvaluation();
            //if (Age) Age.Refresh(new UTextData { IconStr = CIcon.Icon_年龄, Name = () => UIUtil.Green(LastPerson.GetAgeStr(false)) });
            //if (Job) Job.Refresh(new UTextData { IconStr = CIcon.Icon_职业, Name = () => UIUtil.Yellow(LastPerson.GetStaff()) });
        }
        #endregion
    }
}