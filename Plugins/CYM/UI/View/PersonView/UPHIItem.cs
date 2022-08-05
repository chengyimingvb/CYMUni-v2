//------------------------------------------------------------------------------
// BasePHIItem.cs
// Copyright 2019 2019/5/16 
// Created by CYM on 2019/5/16
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CYM.UI
{
    public class UPHIItemData : UData
    {
        public Func<ITDPersonData> Person = null;
    }

    public class UPHIItem : UPres<UPHIItemData>
    {
        #region inspector
        [FoldoutGroup("Full")]
        public Image Full;

        [FoldoutGroup("Face")]
        public Image Bare;
        [FoldoutGroup("Face")]
        public Image Eye;
        [FoldoutGroup("Face")]
        public Image Nose;
        [FoldoutGroup("Face")]
        public Image Hair;
        [FoldoutGroup("Face")]
        public Image Mouse;
        [FoldoutGroup("Face")]
        public Image Brow;
        [FoldoutGroup("Face")]
        public Image Beard;
        [FoldoutGroup("Face")]
        public Image Decorate;

        [FoldoutGroup("Other")]
        public bool IsShowAgeStr = false;//是否显示年龄字符
        [FoldoutGroup("Other")]
        public Image BG;
        [FoldoutGroup("Other")]
        public Image Helmet;
        [FoldoutGroup("Other")]
        public Image Body;
        [FoldoutGroup("Other")]
        public Image Frame;
        [FoldoutGroup("Other")]
        public Image Star;
        [FoldoutGroup("Other")]
        public Image Theory;

        [FoldoutGroup("Text")]
        public Text Name;
        [FoldoutGroup("Text")]
        public Text Age;
        #endregion

        #region prop
        ITDPersonData LastPerson;
        #endregion

        #region life
        public void Refresh(ITDPersonData person)
        {
            LastPerson = person;
            if (LastPerson != null)
            {
                if (Name) Name.text = LastPerson.GetName();
                if (Age) Age.text = LastPerson.GetAgeStr(IsShowAgeStr);

                if (Full) Full.overrideSprite = LastPerson.GetPSprite(PHIPart.PFull);

                if (Bare) Bare.overrideSprite = LastPerson.GetPSprite(PHIPart.PBare);
                if (Eye) Eye.overrideSprite = LastPerson.GetPSprite(PHIPart.PEye);
                if (Nose) Nose.overrideSprite = LastPerson.GetPSprite(PHIPart.PNose);
                if (Hair) Hair.overrideSprite = LastPerson.GetPSprite(PHIPart.PHair);
                if (Mouse) Mouse.overrideSprite = LastPerson.GetPSprite(PHIPart.PMouse);
                if (Brow) Brow.overrideSprite = LastPerson.GetPSprite(PHIPart.PBrow);

                if (BG) BG.overrideSprite = LastPerson.GetPSprite(PHIPart.PBG);
                if (Body) Body.overrideSprite = LastPerson.GetPSprite(PHIPart.PBody);
                if (Frame) Frame.overrideSprite = LastPerson.GetPSprite(PHIPart.PFrame);

                if (Beard)
                {
                    var sprite = LastPerson.GetPSprite(PHIPart.PBeard);
                    Beard.gameObject.SetActive(sprite != null);
                    Beard.overrideSprite = sprite;
                }
                if (Decorate)
                {
                    var sprite = LastPerson.GetPSprite(PHIPart.PDecorate);
                    Decorate.gameObject.SetActive(sprite != null);
                    Decorate.overrideSprite = sprite;
                }
                if (Helmet)
                {
                    var sprite = LastPerson.GetPSprite(PHIPart.PHelmet);
                    Helmet.gameObject.SetActive(sprite != null);
                    Helmet.overrideSprite = sprite;
                }

                if (Star)
                {
                    Star.overrideSprite = LastPerson.GetStarIcon();
                }

                if (Theory)
                {
                    Theory.overrideSprite = LastPerson.Theory.GetIcon();
                }
            }
        }
        public override void Refresh()
        {
            base.Refresh();
            Refresh(Data?.Person?.Invoke());
        }
        #endregion

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);
        }
    }
}