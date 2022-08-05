//------------------------------------------------------------------------------
// Options.cs
// Copyright 2020 2020/7/14 
// Created by CYM on 2020/7/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using JahroConsole;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CYM
{
    [Unobfus][HideMonoScript]
    public sealed partial class Options:MonoSingleton<Options>
    {
        #region Config
        [FoldoutGroup("Base")]
        public bool IsNoPlot   = false;
        [FoldoutGroup("Base")]
        public bool IsIgnoreCondition  = false;
        [FoldoutGroup("Base")]
        public bool IsFastPersonDeath   = false;
        [FoldoutGroup("Base")]
        public bool IsNoEvent   = false;
        [FoldoutGroup("Base")]
        public bool IsMustEvent   = false;
        [FoldoutGroup("Base")]
        public bool IsMustProposal   = false;
        [FoldoutGroup("Base")]
        public bool IsAllAlert   = false;
        [FoldoutGroup("Base")]
        public bool IsOnlyPlayerAI   = false;
        [FoldoutGroup("Base")]
        public bool IsNoMilitaryAI   = false;
        [FoldoutGroup("Base")]
        public bool IsAutoEndTurn   = false;
        [FoldoutGroup("Base")]
        public bool IsLockCamera   = false;
        #endregion

        #region life
        static bool IsStarted = false;
        public partial void OnGMUpdate(); 
        void Start()
        {
            Jahro.InitView();
            if (BuildConfig.Ins.IsShowConsoleBnt)
                Jahro.ShowLaunchButton();
            else 
                Jahro.HideLaunchButton();
            IsStarted = true;
        }
        void Update()
        {
            if (IsGMMode ||
                Application.isEditor)
            {
                OnGMUpdate();
                if (BaseInputMgr.GetKeyDown(KeyCode.F1))
                {
                    Toggle();
                }
            }
        }
        #endregion

        #region set
        public static void Toggle()
        {
            if (!Jahro.IsOpen)
                Jahro.ShowConsoleView();
            else
                Jahro.CloseConsoleView();
        }
        public static void Enable(bool b)
        {
            if (b)
                Jahro.ShowConsoleView();
            else
                Jahro.CloseConsoleView();
        }
        #endregion

        #region is
        public static bool IsShow
        {
            get
            {
                if (!IsStarted)
                    return false;
                return Jahro.IsOpen;
            }
        }
        public static bool IsGMMode
        {
            get
            {
                if (BuildConfig.Ins.IsDevelop)
                    return true;
                if (BaseGlobal.DiffMgr == null)
                    return false;
                return BaseGlobal.DiffMgr.IsGMMode();
            }
        }
        #endregion
    }
}