//------------------------------------------------------------------------------
// BaseMainCS.cs
// Copyright 2019 2019/4/9 
// Created by CYM on 2019/4/9
// Owner: CYM
// 填写类的描述...
// 外部CSharp执行脚本的基类
//------------------------------------------------------------------------------

namespace CYM
{
    public class BaseMainCS
    {
        #region prop
        protected BaseGlobal BaseGlobal => BaseGlobal.Ins;
        protected IBattleMgr<TDBaseBattleData> BaseBattleMgr => BaseGlobal.BattleMgr;
        protected BaseLuaMgr BaseLuaMgr => BaseGlobal.LuaMgr;
        protected BaseLoaderMgr BaseLoaderMgr => BaseGlobal.LoaderMgr;
        protected BaseGRMgr BaseGRMgr => BaseGlobal.GRMgr;
        protected BaseCSMgr BaseCSMgr => BaseGlobal.CSMgr;
        #endregion

        #region life   
        public BaseMainCS()
        {
            if (BaseBattleMgr!=null)
            {
                BaseBattleMgr.Callback_OnStartNewGame += OnStartNewGame;
                BaseBattleMgr.Callback_OnGameStartOver += OnGameStartOver;
                BaseBattleMgr.Callback_OnBackToStart += OnBackToStart;
                BaseBattleMgr.Callback_OnLoad += OnBattleLoad;
                BaseBattleMgr.Callback_OnLoaded += OnBattleLoaded;
                BaseBattleMgr.Callback_OnLoadedScene += OnBattleLoadedScene;
                BaseBattleMgr.Callback_OnUnLoad += OnBattleUnLoad;
                BaseBattleMgr.Callback_OnUnLoaded += OnBattleUnLoaded;
                BaseBattleMgr.Callback_OnGameStart += OnGameStart;
                BaseBattleMgr.Callback_OnGameStarted += OnGameStarted;
                BaseBattleMgr.Callback_OnLoadingProgress += OnLoadingProgress;
                BaseBattleMgr.Callback_OnStartCustomFlow += OnStartCustomFlow;
                BaseBattleMgr.Callback_OnEndCustomFlow += OnEndCustomFlow;
            }

            if (BaseLoaderMgr != null)
            {
                BaseLoaderMgr.Callback_OnLoadEnd += OnLoadEnd;
                BaseLoaderMgr.Callback_OnAllLoadEnd1 += OnAllLoadEnd1;
                BaseLoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;
            }

        }
        //主函数,脚本被加载的时候执行
        protected virtual void Main() 
        {
        
        }
        #endregion

        #region set
        public T AddComponent<T>()
            where T : BaseGFlowMgr, new()
        {
            return BaseGlobal.Ins.AddComponent<T>();
        }
        #endregion

        #region Callback
        protected virtual void OnStartNewGame() { }
        protected virtual void OnGameStartOver() { }
        protected virtual void OnBackToStart() { }
        protected virtual void OnBattleLoad() { }
        protected virtual void OnBattleLoaded() { }
        protected virtual void OnBattleLoadedScene() { }
        protected virtual void OnBattleUnLoad() { }
        protected virtual void OnBattleUnLoaded() { }
        protected virtual void OnGameStart() { }
        protected virtual void OnGameStarted() { }
        protected virtual void OnLoadingProgress(string info, float val) { }
        protected virtual void OnLoadEnd(LoadEndType type, string info) { }
        protected virtual void OnAllLoadEnd1() { }
        protected virtual void OnAllLoadEnd2() { }
        protected virtual void OnStartCustomFlow() { }
        protected virtual void OnEndCustomFlow() { }
        #endregion

    }
}