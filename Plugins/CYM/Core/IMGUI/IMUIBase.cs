//------------------------------------------------------------------------------
// IMUIBase.cs
// Copyright 2020 2020/6/26 
// Created by CYM on 2020/6/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using RapidGUI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    [HideMonoScript]
    public abstract class IMUIBase : MonoBehaviour
    {
        static BoolState IsHaveOpenedViewState { get; set; } = new BoolState();
        static GUISkin globalSkin = null;
        #region prop
        public static List<IMUIBase> AllIMUI { get; private set; } = new List<IMUIBase>();
        public static List<IMUIBase> AllOptionUI { get; private set; } = new List<IMUIBase>();
        public bool IsShow { get; private set; } = false;
        public abstract string Title { get; }
        public virtual KeyCode KeyCode => KeyCode.None;
        public virtual float Width => 800f;
        public virtual float Height => 400f;
        public virtual float XPos => (Screen.width - Width) / 2f;
        public virtual float YPos => (Screen.height - Height) / 3f;
        public virtual bool IsWindow => true;
        public virtual bool IsResizable => true;
        public virtual bool IsDragble => true;
        public virtual bool IsOptionView => false;
        public virtual bool IsBlock => true;
        public virtual bool IsBlockInput => false;
        public float XWidthPos => XPos + Width;
        Rect mainRect = new Rect();
        #endregion

        #region life
        protected virtual void Awake()
        {
            AllIMUI.Add(this);
            if (IsOptionView) AllOptionUI.Add(this);
            if (globalSkin == null)
            {
                globalSkin = Resources.Load<GUISkin>("ProtoGUISkin");
            }
        }
        protected virtual void OnDestroy()
        {
            AllIMUI.Remove(this);
            AllOptionUI.Remove(this);
        }
        protected virtual void Start()
        {
            if (IsOptionView)
            {
                mainRect = new Rect(IMUIOptions.Ins.XWidthPos+20, IMUIOptions.Ins.YPos, Width, Height);
            }
            else
            {
                mainRect = new Rect(XPos, YPos, Width, Height);
            }
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode))
            {
                if (!IsShow) Show();
                else Close();
            }
        }
        private void OnGUI()
        {
            if (IsShow)
            {
                if (IsWindow)
                {
                    if (IsResizable)
                    {
                        mainRect = RGUI.ResizableWindow(GetHashCode(), mainRect,
                            (id) =>
                            {
                                OnDrawWindow();
                                if (IsDragble)
                                {
                                    GUI.DragWindow();
                                }
                            },
                            Title,
                            RGUIStyle.darkWindow);
                    }
                    else
                    {
                        mainRect = RGUI.Window(GetHashCode(), mainRect,
                            (id) =>
                            {
                                OnDrawWindow();
                                if (IsDragble)
                                {
                                    GUI.DragWindow();
                                }
                            },
                            Title,
                            RGUIStyle.darkWindow);
                    }
                }
                else
                {
                    OnDrawWindow();
                }
            }
        }
        protected virtual void OnDrawWindow()
        {
            if (RGUI.Button("X"))
            {
                Close();
            }
            if (RGUI.Button("Refresh"))
            {
                OnOpen();
            }
        }
        #endregion

        #region set
        public void Toggle()
        {
            if (!IsShow)
                Show();
            else
                Close();
        }
        public virtual void Show()
        {
            if (IsShow == true)
                return;
            IsShow = true;
            OnOpen();
        }
        public virtual void Close()
        {
            if (IsShow == false)
                return;
            IsShow = false;
            OnClose();
        }
        #endregion

        #region is
        public static bool CheckUI()
        {
            if (!IsHaveOpenedViewState.IsIn())
                return false;
            foreach (var item in AllIMUI)
            {
                if (!item.IsShow) continue;
                if (!item.IsBlock) continue;
                if (item.mainRect.Contains(new Vector2(Input.mousePosition.x,Screen.height-Input.mousePosition.y)))
                    return true;
            }
            return false;
        }
        #endregion

        #region Callback
        protected virtual void OnOpen()
        {
            IsHaveOpenedViewState.Push(true);
            if (IsBlockInput)
                BaseInputMgr.PushPlayerInputState(false);
            if (IsOptionView)
            {
                foreach (var item in AllOptionUI)
                {
                    if (item != this)
                        item.Close();
                }
            }
        }
        protected virtual void OnClose()
        {
            IsHaveOpenedViewState.Push(false);
            if (IsBlockInput)
                BaseInputMgr.PushPlayerInputState(true);
        }
        #endregion
    }
}