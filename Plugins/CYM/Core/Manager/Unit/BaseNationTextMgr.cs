//------------------------------------------------------------------------------
// BaseNationTextMgr.cs
// Copyright 2019 2019/3/13 
// Created by CYM on 2019/3/13
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

namespace CYM
{
    public class BaseNationTextMgr<TUnit> : BaseUFlowMgr<TUnit>
        where TUnit : BaseUnit
    {
        #region prop
        BaseSLGCameraMgr CameraMgr => BaseGlobal.CameraMgr as BaseSLGCameraMgr;
        TextMeshPro TextMesh;
        object mapAlphaTween;
        object colorTween;
        bool IsShow = false;
        Vector3 sourceScale;
        BoxCollider Collider;
        #endregion

        #region life
        protected virtual CameraHightType CameraHightTypeFirst => CameraHightType.More;
        protected virtual CameraHightType CameraHightTypeFinal => CameraHightType.Most;
        //public override MgrType MgrType => MgrType.Unit;
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            TextMesh = SelfBaseUnit.GetComponentInChildren<TextMeshPro>();
            Collider = SelfBaseUnit.GetComponentInChildren<BoxCollider>();
        }
        public override void OnEnable()
        {
            base.OnEnable();
            BaseGlobal.CameraMgr.Callback_OnHightChanged += OnHightChanged;
            SelfBaseUnit.Callback_OnMouseDown += OnMouseDown;
            SelfBaseUnit.Callback_OnMouseEnter += OnMouseEnter;
            SelfBaseUnit.Callback_OnMouseExit += OnMouseExit;
            SelfBaseUnit.Callback_OnMouseUp += OnMouseUp;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            BaseGlobal.CameraMgr.Callback_OnHightChanged -= OnHightChanged;
            SelfBaseUnit.Callback_OnMouseDown -= OnMouseDown;
            SelfBaseUnit.Callback_OnMouseEnter -= OnMouseEnter;
            SelfBaseUnit.Callback_OnMouseExit -= OnMouseExit;
            SelfBaseUnit.Callback_OnMouseUp -= OnMouseUp;
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            Show(false, true);
        }
        #endregion

        #region overrride
        private void OnHightChanged(CameraHightType arg1,bool b)
        {
            if (CameraHightTypeFirst == arg1)
            {
                OnFirstStep(b);
            }
            if (CameraHightTypeFinal == arg1)
            {
                OnFinalStep(b);
            }
        }
        protected virtual bool HideCondition()
        {
            if (CameraMgr == null)
                return true;
            return false;
        }
        protected virtual Vector3 CenterPos() => throw new NotImplementedException("此函数必须实现");
        protected virtual string TextName() => "None";
        protected virtual Color TextColor() => Color.black;
        protected virtual Color MostTopTextColor() => Color.black;
        protected virtual float HightModify() => 1.0f;
        protected virtual float ScaleModify() => 1.0f;
        protected virtual float MaxScale() => 6.0f;
        #endregion

        #region set
        public void Show(bool b, bool isForce = false)
        {
            if (TextMesh == null) return;
            if (HideCondition()) return;
            if (IsShow == b && !isForce) return;
            IsShow = b;
            if (mapAlphaTween != null)
                DOTween.Kill(mapAlphaTween);
            if (b)
            {
                TextMesh.color = TextColor();
                TextMesh.color = new Color(TextMesh.color.r, TextMesh.color.g, TextMesh.color.b, 0.0f);
                TextMesh.text = TextName();
                Vector3 textPos = CenterPos();
                textPos.y += Mathf.Min(10.0f, HightModify());
                SelfBaseUnit.Pos = textPos;
                float scalefaction = Mathf.Min(MaxScale(), ScaleModify());
                sourceScale = SelfBaseUnit.LocalScale = Vector3.one * scalefaction;
            }
            else
            {

            }
            mapAlphaTween = DOTween.To(
                () => TextMesh.color.a,
                x => TextMesh.color = new Color(TextMesh.color.r, TextMesh.color.g, TextMesh.color.b, x),
                b ? 1.0f : 0.0f,
                0.3f).OnComplete(OnComplete);
        }
        private void OnComplete()
        {
            if (!IsShow)
            {
                SelfBaseUnit.Pos = Const.VEC_FarawayPos;
            }
        }
        #endregion

        #region Callback
        private void OnFirstStep(bool arg1)
        {
            Show(arg1);
        }
        private void OnFinalStep(bool arg1)
        {
            if (BaseGlobal.TerrainGridMgr != null &&
                BaseGlobal.TerrainGridMgr.IsHaveTSG)
            {
                if (Collider == null) 
                    return;
                Collider.enabled = !arg1;
                if (HideCondition())
                    return;
                if (colorTween != null)
                    DOTween.Kill(colorTween);
                colorTween = DOTween.To(
                    () => TextMesh.color,
                    x => TextMesh.color = x,
                    arg1 ? MostTopTextColor() : TextColor(),
                    0.1f).SetEase(Ease.Linear);
            }
        }
        private void OnMouseDown()
        {
        }
        private void OnMouseUp()
        {
            if (BaseInputMgr.IsStayInUI)
                return;
            if (BaseInputMgr.GetClick())
            {
                OnClicked();
            }
        }
        private void OnMouseEnter()
        {

        }
        private void OnMouseExit()
        {

        }
        protected virtual void OnClicked()
        {
            //BaseGlobal.AudioMgr.PlayUI(CAudio.AppTab);
        }
        #endregion
    }
}