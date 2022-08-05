//------------------------------------------------------------------------------
// UNationSelecView.cs
// Created by CYM on 2021/11/5
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using DG.Tweening;
using CYM.Pool;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using System;
using System.Collections.Generic;

namespace CYM.UI
{
    public class UNationSelectView : UStaticUIView<UNationSelectView>
    {
        #region inspector
        [SerializeField]
        TextAsset MapPostData;
        [SerializeField]
        UButton BntStartGame;
        [SerializeField]
        UButton BntRandNation;
        [SerializeField]
        UButton BntSwitchNation;
        [SerializeField]
        UButton BntBackToMainMenu;

        [SerializeField]
        UScroll NationScroll;
        [SerializeField]
        UDupplicate NationDP;
        [SerializeField]
        UTextScroll DescNation;
        [SerializeField]
        UText TitleNation;
        [SerializeField]
        UText TitleOption;
        [SerializeField]
        UImage NationFlag;
        [SerializeField]
        UPosMap PosMap;
        [SerializeField]
        UDupplicate DPAttr;
        [SerializeField]
        UDupplicate DPTraits;
        [SerializeField]
        UDupplicate DPOptionCheckbox;
        [SerializeField]
        UDupplicate DPOptionDroplist;
        [SerializeField]
        UDupplicate DPOptionSlider;
        #endregion

        #region prop
        protected bool IsMoreNation { get; private set; } = false;
        protected ITDNationData CurNation { get; private set; }
        protected IList AllNations { get; private set; }
        protected IList MajorNations { get; private set; }
        #endregion

        #region life
        protected override string TitleKey => "选择国家";
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
            BntStartGame?.Init(new UButtonData { NameKey = "开始游戏", OnClick = OnClickStartGame, HoverClip = "BntHover" });
            BntRandNation?.Init(new UButtonData { NameKey = "随机国家", OnClick = OnClickRandNation, HoverClip = "BntHover" });
            BntSwitchNation?.Init(new UButtonData { Name = () => IsMoreNation ? GetStr("主要国家") : GetStr("更多国家"), OnClick = OnClickMoreNation, HoverClip = "BntHover" });
            BntBackToMainMenu?.Init(new UButtonData { NameKey = "主菜单", OnClick = OnClickBackToMainMenu, HoverClip = "BntHover" });
            NationScroll?.Init(GetSubNationData, OnNationRefresh, OnSubNationItemClick);
            NationDP?.Init(GetMainNationData, OnNationRefresh, OnMainNationItemClick);
            TitleNation?.Init(new UTextData { Name = GetNationName, IsTrans = false });
            TitleOption?.Init(new UTextData { NameKey = "游戏选项" });
            DescNation?.Init(new UTextData { Name = GetNationDesc, IsTrans = false });
            NationFlag?.Init(new UImageData { Icon = GetNationFlag });
            PosMap?.Init(new UPosMapData { MapPosAssets = ()=> MapPostData,Capital = GetCapital,Castles = GetCastles,MapSize =()=>new Vector2(543,461) });
            DPAttr?.Init(GetDPAttrData());
            DPTraits?.Init(new UDupplicateData { FixedCount=2 },GetNationTraitsData,OnRefreshNationTraitsItem);
            DPOptionCheckbox?.Init(GetDPOptionCheckbox());
            DPOptionDroplist?.Init(GetDPOptionDroplist());
            DPOptionSlider?.Init(GetDPOptionSlider());
        }
        protected override void OnOpen(UView baseView, bool useGroup)
        {
            base.OnOpen(baseView, useGroup);
            MajorNations = GetMajorNations();
            AllNations = GetAllNations();
            var tempDatas = GetMainNationData();
            if (tempDatas.Count > 0)
            {
                CurNation = tempDatas[0] as ITDNationData;
            }
        }
        #endregion

        #region Callback
        protected virtual void OnClickBackToMainMenu(UControl arg1, PointerEventData arg2)
        {
            Show(false);
        }
        protected virtual void OnClickMoreNation(UControl arg1, PointerEventData arg2)
        {
            IsMoreNation = !IsMoreNation;
            if (IsMoreNation)
            {
                NationDP.Close();
                NationScroll.Show();
                NationScroll.SelectIndex(-1);
                NationScroll.SetDirtyAll();
            }
            else
            {
                NationScroll.Close();
                NationDP.Show();
                NationDP.SelectIndex(-1);
                NationDP.SetDirtyAll();
            }
        }
        protected virtual void OnClickRandNation(UControl arg1, PointerEventData arg2)
        {
            int index = RandUtil.Range(0,AllNations.Count-1);
            var data = AllNations[index] as ITDNationData;
            BaseGlobal.ScreenMgr.SelectChara(data.GetTDID());
        }
        protected virtual void OnClickStartGame(UControl arg1, PointerEventData arg2)
        {
            BaseGlobal.ScreenMgr.SelectChara(CurNation.GetTDID());
        }
        protected virtual void OnSubNationItemClick(UControl pres)
        {
            CurNation = NationScroll.GetData<ITDNationData>(pres.DataIndex);
            SetDirtyRefresh();
        }
        protected virtual void OnMainNationItemClick(UControl pres)
        {
            CurNation = NationDP.GetData<ITDNationData>();
            SetDirtyRefresh();
        }
        protected virtual void OnNationRefresh(object arg1, object arg2)
        {

        }
        protected virtual IList GetMajorNations() => new HashList<ITDNationData>();
        protected virtual IList GetAllNations() => new HashList<ITDNationData>();
        private IList GetSubNationData()
        {
            return AllNations;
        }
        private IList GetMainNationData()
        {
            return MajorNations;
        }
        Sprite GetNationFlag()
        {
            if (CurNation != null)
                return CurNation.GetFlag();
            return null;
        }
        string GetNationName()
        {
            if (CurNation != null)
                return CurNation.GetName();
            return null;
        }
        string GetNationDesc()
        {
            if (CurNation != null)
                return CurNation.GetDesc();
            return null;
        }
        private string GetCapital()
        {
            if (CurNation != null)
                return CurNation.Capital;
            return null;
        }
        private List<string> GetCastles()
        {
            if (CurNation != null)
                return CurNation.Castles;
            return new List<string>();
        }
        protected virtual string GetNationTraits(ITDNationData nation)
        {
            return "国家特点描述";
        }
        protected virtual UCustomData[] GetDPAttrData()
        {
            return new UCustomData []{ };
        }
        protected virtual IList GetNationTraitsData()
        {
            if (CurNation == null)
                return new List<string>();
            return CurNation.Traits;
        }
        protected virtual void OnRefreshNationTraitsItem(object item, object data)
        {

        }
        protected string GetCapitalName()
        {
            if (CurNation == null)
                return "0";
            return CurNation.Capital.GetName();
        }

        protected string GetCastleCount()
        {
            if (CurNation == null)
                return "0";
            return CurNation.Castles.Count().ToString();
        }
        protected string GetCivilName()
        {
            if (CurNation == null)
                return "0";
            return CurNation.Civil.GetName();
        }
        protected string GetcultureName()
        {
            if (CurNation == null)
                return "0";
            return CurNation.Culture.GetName();
        }

        protected Sprite GetCultureIcon()
        {
            if (CurNation == null)
                return null;
            return CurNation.Culture.GetIcon();
        }
        protected virtual UDropdownData[] GetDPOptionDroplist()
        {
            return new UDropdownData[] { };
        }
        protected virtual UCheckData[] GetDPOptionCheckbox()
        {
            return new UCheckData[] { };
        }
        protected virtual USliderData[] GetDPOptionSlider()
        {
            return new USliderData[] { };
        }
        #endregion
    }
}