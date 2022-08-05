﻿//**********************************************
// Class Name	: CYMBase
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using CYM.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using RapidGUI;
using System.Reflection;
using GameAnalyticsSDK;
using UnityEngine.EventSystems;

namespace CYM
{
    [HideMonoScript]
    [RequireComponent(typeof(FPSCounter))]
    [RequireComponent(typeof(Options))]
    [RequireComponent(typeof(GameAnalytics))]
    [RequireComponent(typeof(EventSystem))]
    [RequireComponent(typeof(StandaloneInputModule))]
    public class BaseGlobal : BaseCoreMono
    {
        #region Config
        public static ITDConfig TDCivil { get; set; }
        public static ITDConfig TDOfficial { get; set; }
        #endregion

        #region Global
        public static GameObject TempGO { get; private set; }
        public static Transform TempTrans => TempGO.transform;
        public static BaseGlobal Ins { get; protected set; }
        public static BuildConfig BuildConfig => BuildConfig.Ins;
        public static Dictionary<Type, IUnitSpawnMgr<BaseUnit>> UnitSpawnMgrs { get; private set; } = new Dictionary<Type, IUnitSpawnMgr<BaseUnit>>();
        public static Dictionary<Type, ITDSpawnMgr<TDBaseData>> TDSpawnMgrs { get; private set; } = new Dictionary<Type, ITDSpawnMgr<TDBaseData>>();
        public static List<BaseUIMgr> UIMgrs { get; private set; } = new List<BaseUIMgr>();
        public static HashSet<string> CommandLineArgs { get; private set; } = new HashSet<string>();
        private static BoolState BoolPause { get; set; } = new BoolState();
        public static Camera MainCamera { get; private set; }
        public static DBBaseSettings Settings => SettingsMgr?.Settings;
        public static BaseUnit Player => ScreenMgr?.Player;
        #endregion

        #region 内置自动添加的组件
        public static BaseLoaderMgr LoaderMgr { get; protected set; }
        public static BaseGRMgr GRMgr { get; protected set; }
        public static BaseLogoMgr LogoMgr { get; protected set; }
        public static BaseLuaMgr LuaMgr { get; protected set; }
        public static BaseTextAssetsMgr TextAssetsMgr { get; protected set; }
        public static BaseExcelMgr ExcelMgr { get; protected set; }
        public static BaseCSMgr CSMgr { get; protected set; }
        public static BaseLangMgr LangMgr { get; protected set; }
        public static BaseConditionMgr ACM { get; protected set; }
        public static BaseBGMMgr BGMMgr { get; protected set; }
        public static BaseAudioMgr AudioMgr { get; protected set; }
        public static BaseDateTimeMgr DateTimeMgr { get; protected set; }
        public static BasePoolMgr PoolMgr { get; protected set; }
        public static BaseLoggerMgr LoggerMgr { get; protected set; }
        public static BasePerformMgr PerformMgr { get; protected set; }
        #endregion

        #region 内置UI组件
        public static BaseHUDUIMgr HUDUIMgr { get; protected set; }
        public static BaseCommonUIMgr CommonUIMgr { get; protected set; }
        public static BaseMainUIMgr MainUIMgr { get; protected set; }
        public static BaseBattleUIMgr BattleUIMgr { get; protected set; }
        public static BaseLevelUIMgr LevelUIMgr { get; protected set; }
        #endregion

        #region 非必要组件
        public static ISettingsMgr<DBBaseSettings> SettingsMgr { get; protected set; }
        public static IDBMgr<DBBaseGame> DBMgr { get; protected set; }
        public static IDiffMgr<DBBaseGameDiff> DiffMgr { get; protected set; }
        public static IScreenMgr<BaseUnit> ScreenMgr { get; protected set; }
        public static ITurnbaseMgr LogicTurnMgr { get; protected set; }
        public static IPlotMgr PlotMgr { get; protected set; }
        public static ITalkMgr TalkMgr { get; protected set; }
        public static INarrationMgr<TDBaseNarrationData> NarrationMgr { get; protected set; }
        public static IStoryMgr<TDBaseStoryData> StoryMgr { get; protected set; }
        public static IBattleMgr<TDBaseBattleData> BattleMgr { get; protected set; }
        public static ILevelMgr<TDBaseLevelData> LevelMgr { get; protected set; }
        public static IAttrMgr AttrMgr { get; protected set; }
        public static IBuffMgr BuffMgr { get; protected set; }
        public static IArticleMgr<TDBaseArticleData> ArticleMgr { get; protected set; }
        public static IRelationMgr RelationMgr { get; protected set; }
        public static IPersonTestMgr PersonTestMgr { get; protected set; }
        public static BaseRealtimeMgr RealtimeMgr { get; protected set; }
        public static BaseInputMgr InputMgr { get; protected set; }
        public static BaseLoginMgr LoginMgr { get; protected set; }
        public static BaseUnlockMgr UnlockMgr { get; protected set; }
        public static BaseCameraMgr CameraMgr { get; protected set; }
        public static BaseNetMgr NetMgr { get; protected set; }
        public static BasePlatSDKMgr PlatSDKMgr { get; protected set; }
        public static BaseCursorMgr CursorMgr { get; protected set; }
        public static BaseAStarMgr AStarMgr { get; protected set; }
        public static BaseAStar2DMgr AStar2DMgr { get; protected set; }
        public static BaseRefMgr RefMgr { get; protected set; }
        public static BaseTerrainGridMgr TerrainGridMgr { get; protected set; }
        public static BaseFOWMgr FOWMgr { get; protected set; }
        public static BasePathRenderMgr PathRenderMgr { get; protected set; }
        public static BasePostProcesMgr PostProcesMgr { get; protected set; }
        #endregion

        #region prop
        public static Corouter CommonCorouter { get; protected set; }
        public static Corouter MainUICorouter { get; protected set; }
        public static Corouter BattleCorouter { get; protected set; }
        public static Corouter LevelCorouter { get; protected set; }
        #endregion

        #region life
        public virtual int JobsPerFrame => 100;
        public override LayerData LayerData => Const.Layer_System;
        public override MonoType MonoType => MonoType.Global;
        protected override void OnAttachComponet() 
        {
            //添加内置组件
            LoaderMgr = AddComponent<BaseLoaderMgr>();
            GRMgr = AddComponent<BaseGRMgr>();
            LogoMgr = AddComponent<BaseLogoMgr>();
            LuaMgr = AddComponent<BaseLuaMgr>();
            TextAssetsMgr = AddComponent<BaseTextAssetsMgr>();
            ExcelMgr = AddComponent<BaseExcelMgr>();
            CSMgr = AddComponent<BaseCSMgr>();
            LangMgr = AddComponent<BaseLangMgr>();
            ACM = AddComponent<BaseConditionMgr>();
            BGMMgr = AddComponent<BaseBGMMgr>();
            AudioMgr = AddComponent<BaseAudioMgr>();
            DateTimeMgr = AddComponent<BaseDateTimeMgr>();
            PoolMgr = AddComponent<BasePoolMgr>();
            LoggerMgr = AddComponent<BaseLoggerMgr>();
            PerformMgr = AddComponent<BasePerformMgr>();
        }
        public override void Awake()
        {
            if (Ins == null) Ins = this;
            //创建临时对象
            TempGO = new GameObject("TempGO");
            TempGO.hideFlags = HideFlags.HideInHierarchy;
            //创建UICamera
            Util.CreateGlobalResourceObj<UICameraObj>("UICamera");
            UICameraObj.GO.transform.hideFlags = HideFlags.HideInHierarchy;
            //使应用程序无法关闭
            Application.wantsToQuit += OnWantsToQuit;
            WinUtil.DisableSysMenuButton();
            //创建必要的文件目录
            FileUtil.EnsureDirectory(Const.Path_Dev);
            FileUtil.EnsureDirectory(Const.Path_Screenshot);
            FileUtil.EnsureDirectory(Const.Path_LocalDB);
            FileUtil.EnsureDirectory(Const.Path_CloudDB);
            //添加必要的组件
            SetupComponent<UIVideoer>();
            SetupComponent<Videoer>();
            SetupComponent<Prefers>();
            SetupComponent<Feedback>();
            SetupComponent<FPSCounter>();
            SetupComponent<GlobalMonoMgr>();
            SetupComponent<GlobalUITextMgr>();
            SetupComponent<IMUIErrorCatcher>();
            SetupComponent<IMUIWaterMarker>();
            SetupComponent<IMUIOptions>();
            //初始化LuaReader
            InitConfig();
            CMail.Init(BuildConfig.FullName);
            LuaReader.Init(BuildConfig.NameSpace);
            DOTween.Init();
            DOTween.instance.transform.SetParent(Trans);
            Timing.Instance.transform.SetParent(Trans);
            QueueHub.Instance.transform.SetParent(Trans);
            Delay.Ins.transform.SetParent(Trans);
            RapidGUIBehaviour.Instance.transform.SetParent(Trans);
            GameAnalytics.Initialize();
            WinUtil.DisableSysMenuButton();
            //创建所有DataParse
            OnProcessAssembly();
            base.Awake();
            //添加SDK组件
            OnAddPlatformSDKComponet();
            //读取命令行参数
            OnProcessCMDArgs();
            DontDestroyOnLoad(this);
            //携程
            CommonCorouter = new Corouter("Common");
            MainUICorouter = new Corouter("MainUI");
            BattleCorouter = new Corouter("Battle");
            LevelCorouter = new Corouter("SubBattle");
            Pos = Const.VEC_GlobalPos;
            //CALLBACK
            LoaderMgr.Callback_OnAllLoadEnd1 += OnAllLoadEnd1;
            LoaderMgr.Callback_OnAllLoadEnd2 += OnAllLoadEnd2;
            LuaMgr.Callback_OnParseStart += OnLuaParseStart;
            LuaMgr.Callback_OnParseEnd += OnLuaParseEnd;
        }
        void InitConfig()
        {
            if (LogConfig.Ins != null &&
                UIConfig.Ins != null &&
                BuildConfig.Ins != null &&
                GameConfig.Ins != null &&
                DLCConfig.Ins != null)
            {

            }
        }
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
            NeedGUI = true;
            NeedUpdate = true;
            NeedLateUpdate = true;
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            MainCamera = Camera.main;
        }
        public override void OnDestroy()
        {
            //CALLBACK
            LoaderMgr.Callback_OnAllLoadEnd1 -= OnAllLoadEnd1;
            LoaderMgr.Callback_OnAllLoadEnd2 -= OnAllLoadEnd2;
            LuaMgr.Callback_OnParseStart -= OnLuaParseStart;
            LuaMgr.Callback_OnParseEnd -= OnLuaParseEnd;
            Application.wantsToQuit -= OnWantsToQuit;
            UIMgrs.Clear();
            UnitSpawnMgrs.Clear();
            TDSpawnMgrs.Clear();
            base.OnDestroy();
        }
        // 添加平台SDK组建
        protected void OnAddPlatformSDKComponet()
        {
            var type = BuildConfig.Ins.Distribution;
            if (type == Distribution.Steam) PlatSDKMgr = AddComponent<BaseSteamSDKMgr>();
            else if (type == Distribution.Rail) PlatSDKMgr = AddComponent<BaseRailSDKMgr>();
            else if (type == Distribution.Turbo) PlatSDKMgr = AddComponent<BaseTurboSDKMgr>();
            else if (type == Distribution.Trial) PlatSDKMgr = AddComponent<BaseTrialSDKMgr>();
            else if (type == Distribution.Gaopp) PlatSDKMgr = AddComponent<BaseGaoppSDKMgr>();
            else if (type == Distribution.Usual) PlatSDKMgr = AddComponent<BaseUsualSDKMgr>();
            else CLog.Error("未知SDK:" + type.ToString());
        }
        private void OnProcessAssembly()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
            foreach (var item in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (item.IsClass)
                {
                    if (item.IsSubclassOf(typeof(IMUIBase)))
                    {
                        if (item == typeof(IMUIOptions)) continue;
                        if (!item.IsAbstract && !item.IsGenericType)
                        {
                            GO.AddComponent(item);
                        }
                    }
                }
            }
#endif
        }
        private void OnProcessCMDArgs()
        {
#if UNITY_EDITOR
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            foreach (var item in commandLineArgs)
            {
                CommandLineArgs.Add(item);
                if (item == "-GM")
                {
                    DiffMgr.SetGMMod(true);
                }
                CLog.Info("CMDline arg:"+item);
            }
#endif
        }
        #endregion

        #region Callback
        protected bool OnWantsToQuit() => true;
        protected virtual void OnAllLoadEnd1() { }
        protected virtual void OnAllLoadEnd2() { }
        protected virtual void OnLuaParseStart() { }
        protected virtual void OnLuaParseEnd() { }
        #endregion

        #region pause
        // 停止游戏
        public static void PauseGame(bool b)
        {
            BoolPause.Push(b);
            DoPauseGame();
        }
        public static void ResumeGame()
        {
            BoolPause.Reset();
            DoPauseGame();
        }
        private static void DoPauseGame()
        {
            if (BoolPause.IsIn())
            {
                GlobalMonoMgr.SetPauseType(MonoType.Unit);
                BattleCorouter.Pause();
                LevelCorouter.Pause();
            }
            else
            {
                GlobalMonoMgr.SetPauseType(MonoType.None);
                BattleCorouter.Resume();
                LevelCorouter.Pause();
            }
        }
        #endregion

        #region add componet
        public override T AddComponent<T>()
        {
            var ret = base.AddComponent<T>();
            //自动赋值
            if (ret is IUnitSpawnMgr<BaseUnit> unitSpawner)
            {
                if(unitSpawner.IsAddToGlobalSpawnerMgr)
                    UnitSpawnMgrs.Add(unitSpawner.UnitType, unitSpawner);
            }
            if (ret is ITDSpawnMgr<TDBaseData> tdSpawner)
            {
                if (tdSpawner.IsAddToGlobalSpawnerMgr)
                    TDSpawnMgrs.Add(tdSpawner.UnitType,tdSpawner);
            }
            if (ret is BaseUIMgr uiMgr)
            {
                UIMgrs.Add(uiMgr);
            }

            if (ret is BaseLoaderMgr && LoaderMgr == null) LoaderMgr = ret as BaseLoaderMgr;
            else if (ret is BaseGRMgr && GRMgr == null) GRMgr = ret as BaseGRMgr;
            else if (ret is BaseLogoMgr && LogoMgr == null) LogoMgr = ret as BaseLogoMgr;
            else if (ret is BaseExcelMgr && ExcelMgr == null) ExcelMgr = ret as BaseExcelMgr;
            else if (ret is BaseLuaMgr && LuaMgr == null) LuaMgr = ret as BaseLuaMgr;
            else if (ret is BaseTextAssetsMgr && TextAssetsMgr == null) TextAssetsMgr = ret as BaseTextAssetsMgr;
            else if (ret is BaseLangMgr && LangMgr == null) LangMgr = ret as BaseLangMgr;
            else if (ret is BaseConditionMgr && ACM == null) ACM = ret as BaseConditionMgr;
            else if (ret is BaseBGMMgr && BGMMgr == null) BGMMgr = ret as BaseBGMMgr;
            else if (ret is BaseAudioMgr && AudioMgr == null) AudioMgr = ret as BaseAudioMgr;
            else if (ret is BaseDateTimeMgr && DateTimeMgr == null) DateTimeMgr = ret as BaseDateTimeMgr;
            else if (ret is BasePoolMgr && PoolMgr == null) PoolMgr = ret as BasePoolMgr;
            else if (ret is BaseLoggerMgr && LoggerMgr == null) LoggerMgr = ret as BaseLoggerMgr;

            //UI组件
            else if (ret is BaseHUDUIMgr && HUDUIMgr == null) HUDUIMgr = ret as BaseHUDUIMgr;
            else if (ret is BaseCommonUIMgr && CommonUIMgr == null) CommonUIMgr = ret as BaseCommonUIMgr;
            else if (ret is BaseMainUIMgr && MainUIMgr == null) MainUIMgr = ret as BaseMainUIMgr;
            else if (ret is BaseBattleUIMgr && BattleUIMgr == null) BattleUIMgr = ret as BaseBattleUIMgr;
            else if (ret is BaseLevelUIMgr && LevelUIMgr == null) LevelUIMgr = ret as BaseLevelUIMgr;

            //非必要接口
            else if (ret is ISettingsMgr<DBBaseSettings> && SettingsMgr == null) SettingsMgr = ret as ISettingsMgr<DBBaseSettings>;
            else if (ret is IDBMgr<DBBaseGame> && DBMgr == null) DBMgr = ret as IDBMgr<DBBaseGame>;
            else if (ret is IDiffMgr<DBBaseGameDiff> && DiffMgr == null) DiffMgr = ret as IDiffMgr<DBBaseGameDiff>;
            else if (ret is IScreenMgr<BaseUnit> && ScreenMgr == null) ScreenMgr = ret as IScreenMgr<BaseUnit>;
            else if (ret is IAttrMgr && AttrMgr == null) AttrMgr = ret as IAttrMgr;
            else if (ret is IBuffMgr && BuffMgr == null) BuffMgr = ret as IBuffMgr;
            else if (ret is IArticleMgr<TDBaseArticleData> && ArticleMgr == null) ArticleMgr = ret as IArticleMgr<TDBaseArticleData>;
            else if (ret is IPlotMgr && PlotMgr == null) PlotMgr = ret as IPlotMgr;
            else if (ret is ITalkMgr && TalkMgr == null) TalkMgr = ret as ITalkMgr;
            else if (ret is INarrationMgr<TDBaseNarrationData> && NarrationMgr == null) NarrationMgr = ret as INarrationMgr<TDBaseNarrationData>;
            else if (ret is IStoryMgr<TDBaseStoryData> && StoryMgr == null) StoryMgr = ret as IStoryMgr<TDBaseStoryData>;
            else if (ret is IRelationMgr && RelationMgr == null) RelationMgr = ret as IRelationMgr;
            else if (ret is ITurnbaseMgr && LogicTurnMgr == null) LogicTurnMgr = ret as ITurnbaseMgr;
            else if (ret is IBattleMgr<TDBaseBattleData> && BattleMgr == null) BattleMgr = ret as IBattleMgr<TDBaseBattleData>;
            else if (ret is ILevelMgr<TDBaseLevelData> && LevelMgr == null) LevelMgr = ret as ILevelMgr<TDBaseLevelData>;
            else if (ret is IPersonTestMgr && PersonTestMgr == null) PersonTestMgr = ret as IPersonTestMgr;

            //非必要组件
            else if (ret is BaseRealtimeMgr && RealtimeMgr == null) RealtimeMgr = ret as BaseRealtimeMgr;
            else if (ret is BaseInputMgr && InputMgr == null) InputMgr = ret as BaseInputMgr;
            else if (ret is BaseUnlockMgr && UnlockMgr == null) UnlockMgr = ret as BaseUnlockMgr;
            else if (ret is BaseNetMgr && NetMgr == null) NetMgr = ret as BaseNetMgr;
            else if (ret is BaseAStarMgr && AStarMgr == null) AStarMgr = ret as BaseAStarMgr;
            else if (ret is BaseAStar2DMgr && AStar2DMgr == null) AStar2DMgr = ret as BaseAStar2DMgr;
            else if (ret is BasePerformMgr && PerformMgr == null) PerformMgr = ret as BasePerformMgr;
            else if (ret is BaseRefMgr && RefMgr == null) RefMgr = ret as BaseRefMgr;
            else if (ret is BaseLoginMgr && LoginMgr == null) LoginMgr = ret as BaseLoginMgr;
            else if (ret is BasePlatSDKMgr && PlatSDKMgr == null) PlatSDKMgr = ret as BasePlatSDKMgr;
            else if (ret is BaseCursorMgr && CursorMgr == null) CursorMgr = ret as BaseCursorMgr;
            else if (ret is BaseCameraMgr && CameraMgr == null) CameraMgr = ret as BaseCameraMgr;
            else if (ret is BaseTerrainGridMgr && TerrainGridMgr == null) TerrainGridMgr = ret as BaseTerrainGridMgr;
            else if (ret is BaseFOWMgr && FOWMgr == null) FOWMgr = ret as BaseFOWMgr;
            else if (ret is BasePathRenderMgr && PathRenderMgr == null) PathRenderMgr = ret as BasePathRenderMgr;
            else if (ret is BasePostProcesMgr && PostProcesMgr == null) PostProcesMgr = ret as BasePostProcesMgr;

            return ret;
        }
        #endregion

        #region set
        public static List<IClear> ClearWhenBattleUnload { get; private set; } = new List<IClear>();
        public static List<IClear> ClearWhenLevelUnload { get; private set; } = new List<IClear>();
        public static void AddToClearWhenBattleUnload(IClear clear) => ClearWhenBattleUnload.Add(clear);
        public static void AddToClearWhenLevelUnload(IClear clear) => ClearWhenLevelUnload.Add(clear);
        public static void Quit()
        {
            if (!Application.isEditor)
            {
                Process.GetCurrentProcess().Kill();
                Application.Quit();
                Environment.Exit(0);
            }
        }
        public static void GCCollect()
        {
            if (Application.isMobilePlatform)
                return;
            GC.Collect();
        }
        public static void ForceGCCollect()
        {
            GC.Collect();
        }
        #endregion

        #region get
        public static Transform GetTransform(Vector3 pos)
        {
            TempTrans.position = pos;
            return TempTrans;
        }
        public static IUnitSpawnMgr<BaseUnit> GetUnitSpawnMgr(Type unitType)
        {
            if (UnitSpawnMgrs.ContainsKey(unitType))
            {
                return UnitSpawnMgrs[unitType];
            }
            return null;
        }
        public static ITDSpawnMgr<TDBaseData> GetTDSpawnerMgr(Type unityType)
        {
            if (TDSpawnMgrs.ContainsKey(unityType))
            {
                return TDSpawnMgrs[unityType];
            }
            return null;
        }
        public static TUnit GetUnit<TUnit>(long id, bool isLogError = true) where TUnit : BaseUnit
        {
            if (id.IsInv()) return null;
            var ret = GetUnit(id, typeof(TUnit)) as TUnit;
            if (ret == null)
            {
                if (isLogError)
                    CLog.Error("没有这个游戏实体!!!,{0}", id);
            }
            return ret;
        }
        public static TUnit GetUnit<TUnit>(string id, bool isLogError = true) where TUnit : BaseUnit
        {
            if (id.IsInv()) return null;
            var ret = GetUnit(id, typeof(TUnit)) as TUnit;
            if (ret == null)
            {
                if (isLogError)
                    CLog.Error("没有这个游戏实体!!!,{0}", id);
            }
            return ret;
        }
        public static BaseUnit GetUnit(long id, Type unitType = null, bool isLogError = true)
        {
            if (id.IsInv()) return null;
            if (unitType == null)
            {
                foreach (var item in UnitSpawnMgrs)
                {
                    var temp = item.Value.GetUnit(id);
                    if (temp != null)
                        return temp;
                }
            }
            else
            {
                if (UnitSpawnMgrs.ContainsKey(unitType))
                {
                    return UnitSpawnMgrs[unitType].GetUnit(id);
                }
            }
            if (isLogError)
                CLog.Error("无法获得Entity,ID:{0},Type:{1}", id, unitType != null ? unitType.ToString() : "None");
            return null;
        }
        public static BaseUnit GetUnit(string id, Type unitType = null, bool isLogError = true)
        {
            if (id.IsInv()) return null;
            if (unitType == null)
            {
                foreach (var item in UnitSpawnMgrs)
                {
                    var temp = item.Value.GetUnit(id);
                    if (temp != null)
                        return temp;
                }
            }
            else
            {
                if (UnitSpawnMgrs.ContainsKey(unitType))
                {
                    return UnitSpawnMgrs[unitType].GetUnit(id);
                }
            }
            if (isLogError)
                CLog.Error("无法获得Entity,ID:{0},Type:{1}", id, unitType != null ? unitType.ToString() : "None");
            return null;
        }
        public static HashList<BaseUnit> GetUnit(List<long> ids)
        {
            HashList<BaseUnit> data = new HashList<BaseUnit>();
            foreach (var item in ids)
            {
                var entity = GetUnit(item);
                if (entity == null) continue;
                data.Add(entity);
            }
            return data;
        }
        public static List<long> GetUnitIDs(HashList<BaseUnit> entity)
        {
            List<long> ids = new List<long>();
            foreach (var item in entity)
            {
                if (item.IsInv()) continue;
                ids.Add(item.ID);
            }
            return ids;
        }
        #endregion

        #region Get TDData
        public static TData GetTDData<TData>(long id, bool isLogError = true) where TData : TDBaseData
        {
            if (id.IsInv()) return null;
            var ret = GetTDData(id, typeof(TData)) as TData;
            if (ret == null)
            {
                if (isLogError)
                    CLog.Error("没有这个游戏配置对象!!!,{0}", id);
            }
            return ret;
        }
        public static TData GetTDData<TData>(string id, bool isLogError = true) where TData : TDBaseData
        {
            if (id.IsInv()) return null;
            var ret = GetTDData(id, typeof(TData)) as TData;
            if (ret == null)
            {
                if (isLogError)
                    CLog.Error("没有这个游戏配置对象!!!,{0}", id);
            }
            return ret;
        }
        public static TDBaseData GetTDData(long id, Type unitType = null, bool isLogError = true)
        {
            if (id.IsInv()) return null;
            if (unitType == null)
            {
                foreach (var item in TDSpawnMgrs)
                {
                    var temp = item.Value.GetUnit(id);
                    if (temp != null)
                        return temp;
                }
            }
            else
            {
                if (TDSpawnMgrs.ContainsKey(unitType))
                {
                    return TDSpawnMgrs[unitType].GetUnit(id);
                }
            }
            if (isLogError)
                CLog.Error("无法获得TDData,ID:{0},Type:{1}", id, unitType != null ? unitType.ToString() : "None");
            return null;
        }
        public static TDBaseData GetTDData(string id, Type unitType = null, bool isLogError = true)
        {
            if (id.IsInv()) return null;
            if (unitType == null)
            {
                foreach (var item in TDSpawnMgrs)
                {
                    var temp = item.Value.GetUnit(id);
                    if (temp != null)
                        return temp;
                }
            }
            else
            {
                if (TDSpawnMgrs.ContainsKey(unitType))
                {
                    return TDSpawnMgrs[unitType].GetUnit(id);
                }
            }
            if (isLogError)
                CLog.Error("无法获得TDData,ID:{0},Type:{1}", id, unitType != null ? unitType.ToString() : "None");
            return null;
        }
        #endregion

        #region is
        // 是否暂停游戏
        public static bool IsPause => BoolPause.IsIn();
        // 是否处于读取数据阶段，用于避免触发一些回调
        public static bool IsUnReadData { get; set; } = true;
        public static bool IsHaveCommandLineArg(string arg)=> CommandLineArgs.Contains(arg);
        public static bool Is3D
        {
            get
            {
                if (MainCamera == null)
                    return false;
                return !MainCamera.orthographic;
            }
        }
        public static bool Is2D
        {
            get
            {
                if (MainCamera == null)
                    return false;
                return MainCamera.orthographic;
            }
        }
        #endregion
    }
}