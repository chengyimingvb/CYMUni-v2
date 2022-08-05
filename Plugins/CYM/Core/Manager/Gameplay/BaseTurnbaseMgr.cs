//------------------------------------------------------------------------------
// BaseLogicTurnMgr.cs
// Copyright 2018 2018/11/10 
// Created by CYM on 2018/11/10
// Owner: CYM
// 回合制游戏的管理器
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace CYM
{
    public class BaseTurnbaseMustProcess
    {
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        public virtual string Desc => "";
        public virtual void DoClick() { }
    }

    public class BaseTurnbaseMgr<T> : BaseGFlowMgr, ITurnbaseMgr 
        where T : BaseUnit
    {
        #region Callback val
        /// <summary>
        /// T:当前执行操作的单位
        /// float:当前的进度
        /// </summary>
        public event Callback<T, float> Callback_OnOperate;
        public event Callback<T> Callback_OnEndTurn;
        public event Callback<T,bool> Callback_OnStartTurn;
        public event Callback Callback_OnEndPlayerTurn;
        public event Callback Callback_OnEndPlayerPostTurn;
        public event Callback Callback_OnStartPlayerTurn;
        #endregion

        #region global mgr
        protected IPlotMgr PlotMgr => BaseGlobal.PlotMgr;
        protected BaseInputMgr InputMgr => BaseGlobal.InputMgr;
        protected BaseCameraMgr CameraMgr => BaseGlobal.CameraMgr;
        protected IScreenMgr<BaseUnit> ScreenMgr => BaseGlobal.ScreenMgr;
        protected Corouter BattleCoroutine => BaseGlobal.BattleCorouter;
        #endregion

        #region prop
        protected BaseUnit Player => BaseGlobal.ScreenMgr.Player;
        //处于当前回合的单位
        public T CurOperatingUnit { get; protected set; }
        //当前回合的索引
        public int CurTurnIndex { get; protected set; } = 0;
        //当前必须处理事务的索引
        public int CurMustProcessIndex { get; protected set; } = 0;
        //已经结束回合的单位
        protected HashList<T> OperatedUnits = new HashList<T>();
        //所有拥有操作权的单位
        public List<T> AllUnits { get; protected set; } = new List<T>();
        //回合内必须处理的数据
        public List<BaseTurnbaseMustProcess> MustProcessDatas { get; protected set; } = new List<BaseTurnbaseMustProcess>();
        public CoroutineHandle OnInEndTurnCoroutine { get; private set; }
        public CoroutineHandle OnInPostEndTurnCoroutine { get; private set; }
        public int TurnCount { get; private set; } = 0;
        public BoolState _flagPause = new BoolState();
        protected virtual bool IsCustomPause => false;
        public bool IsPause => _flagPause.IsIn() || IsCustomPause || IsInGlobalMoveState;
        #endregion

        #region life
        protected virtual IEnumerable<T> CalcMoveableUnits(T unit) => throw new NotImplementedException();
        protected virtual IEnumerable<T> CalcAllUnits() => null;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedTurnbase = true;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            ScreenMgr.Callback_OnSetPlayer += OnSetPlayer;
        }
        public override void OnDisable()
        {
            ScreenMgr.Callback_OnSetPlayer -= OnSetPlayer;
            base.OnDisable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (!BattleMgr.IsInBattle) return;
            //如果是玩家回合则返回
            if (IsInPlayerTurn) return;
            if (CurOperatingUnit != null && AllUnits.Count > 0)
            {
                //触发回合执行事件
                OnTurnOperating(CurOperatingUnit);
                Callback_OnOperate?.Invoke(CurOperatingUnit, CurTurnIndex / (float)AllUnits.Count);
            }
        }
        public override void OnTurnbase(bool day, bool month, bool year)
        {
            base.OnTurnbase(day, month, year);
        }
        #endregion

        #region Must process
        //清空必须处理的事务
        public void ClearMustProcess()
        {
            MustProcessDatas.Clear();
        }
        //添加必须处理的事务
        public void AddMustProcess(BaseTurnbaseMustProcess data)
        {
            MustProcessDatas.Add(data);
        }
        public void NextProcess()
        {
            if (MustProcessDatas.Count <= 0) return;
            if (CurMustProcessIndex >= MustProcessDatas.Count)
                CurMustProcessIndex = 0;
            MustProcessDatas[CurMustProcessIndex].DoClick();
            CurMustProcessIndex++;
        }
        #endregion

        #region get
        public string GetMustProcessStr()
        {
            return StrUtil.List(MustProcessDatas, x => Const.STR_Indent + UIUtil.Red(x.Desc));
        }
        #endregion

        #region is can
        public virtual bool IsCanEndTurn(T unit)
        {
            if (BaseGlobal.DBMgr != null && BaseGlobal.DBMgr.IsHolding)
                return false;
            if (PlotMgr!=null && PlotMgr.IsInPlotPause()) return false;
            if (IsLockTurnState.IsIn()) return false;
            if (IsInEndTurn) return false;
            if (!IsInTurn(unit)) return false;
            if (CheckMustProcess(unit)) return false;
            return !IsPause;
        }
        #endregion

        #region is
        public bool CheckMustProcess(T unit)
        {
            OnSetMustProcess(unit);
            if (MustProcessDatas.Count > 0) 
                return true;
            return false;
        }
        //是否在回合内
        public bool IsTurnCount(int count)=> TurnCount % count == 0;
        //是否处于全局移动状态:回合制游戏每次只能有一个单位移动
        public bool IsInGlobalMoveState => BaseAStarMgr.GlobalMoveState.IsIn();
        public BoolState IsLockTurnState { get; private set; } = new BoolState();
        //是否处于本地玩家回合
        public bool IsInPlayerTurn
        {
            get
            {
                if (CurOperatingUnit == Player) return true;
                return false;
            }
        }
        //是否在某个游戏角色的回合内
        public bool IsInTurn(T unit)
        {
            if (CurOperatingUnit == unit) return true;
            return false;
        }
        //是否处于(后-结束回合)状态
        public bool IsInPostEndTurn => OnInPostEndTurnCoroutine.IsRunning;
        //是否处于玩家操作的回合
        public bool IsInValidPlayerTurn => IsInPlayerTurn && !IsInEndTurn;
        //是否处于(结束回合)状态
        public bool IsInEndTurn =>
            OnInEndTurnCoroutine.IsRunning ||
            OnInPostEndTurnCoroutine.IsRunning ||
            _isInEndTurnFlag;
        bool _isInEndTurnFlag = false;
        #endregion

        #region set
        public void Pause()
        {
            if (IsInValidPlayerTurn) return;
            _flagPause.Add();
        }
        public void Resume()
        {
            if (IsInValidPlayerTurn) return;
            _flagPause.Remove();
        }
        //结束当前回合
        public bool EndTurn(T endUnit = null)
        {
            if (IsPause || IsInEndTurn)
            {
                return false;
            }
            if (SubBattleMgr.IsInLevel)
            {
                CLog.Error("错误!试图在SubBattle阶段结束回合");
                return false;
            }
            if (endUnit == null) endUnit = Player as T;
            OnPreEndTurn(endUnit);
            if (!IsCanEndTurn(endUnit))
            {
                OnCantEndTurn(endUnit);
                return false;
            }
            _isInEndTurnFlag = true;
            BattleCoroutine.Kill(OnInEndTurnCoroutine);
            OnInEndTurnCoroutine = BattleCoroutine.Run(OnInEndTurn(endUnit));
            return true;
        }
        //开始
        public void StartTurn(bool isForce=false)
        {
            if (IsPause && !isForce)
            {
                return;
            }
            _isInEndTurnFlag = false;
            BattleCoroutine.Kill(OnInEndTurnCoroutine);
            BattleCoroutine.Kill(OnInPostEndTurnCoroutine);
            //新的一回合开始
            if (CurTurnIndex == 0)
            {
                AllUnits = CalcAllUnits().OrderByDescending(x => x.IsPlayerCtrl()).ToList();
                OnTurnAllStart();
            }
            if (AllUnits.Count <= 0)
            {
                CLog.Error("AllUnits人数太少");
                return;
            }
            var unit = AllUnits[CurTurnIndex];
            CurOperatingUnit = unit;
            OnSetMustProcess(unit);
            Callback_OnStartTurn?.Invoke(unit, isForce);
            OnTurnStart(unit, isForce);
            if (unit.IsPlayer())
            {
                OnTurnPlayerStart();
                Callback_OnStartPlayerTurn?.Invoke();
            }
        }
        //操作回合:回调函数
        #endregion

        #region IEnumerator
        IEnumerator<float> OnInEndTurn(T endUnit)
        {
            //触发本地玩家事件
            if (endUnit.IsPlayer())
            {
                OnTurnPlayerEnd();
                Callback_OnEndPlayerTurn?.Invoke();
            }
            //回合后期处理
            BattleCoroutine.Kill(OnInPostEndTurnCoroutine);
            OnInPostEndTurnCoroutine = BattleCoroutine.Run(OnInPostEndTurn(endUnit));
            yield return Timing.WaitUntilDone(OnInPostEndTurnCoroutine);

            while (IsPause)
            {
                yield return Timing.WaitForOneFrame;
            }

            //触发本地玩家事件
            if (endUnit.IsPlayer())
            {
                OnTurnPlayerPostEnd();
                Callback_OnEndPlayerPostTurn?.Invoke();
            }

            if (AllUnits.Count <= 0) CLog.Error("AllUnits人数太少");
            if (CurOperatingUnit != endUnit) CLog.Error("错误:当前回合单位不一致");
            OperatedUnits.Add(endUnit);
            Callback_OnEndTurn?.Invoke(endUnit);
            OnTurnEnd(endUnit);
            //开启下一个回合
            if (CurTurnIndex >= AllUnits.Count - 1)
            {
                CurTurnIndex = 0;
                OnTurnAllEnd();
            }
            //下一个单位
            else
            {
                CurTurnIndex++;
            }
            StartTurn();
        }
        protected virtual IEnumerator<float> OnInPostEndTurn(T unit)
        {
            var data = CalcMoveableUnits(unit);
            foreach (var item in data)
            {
                IAStarTBMoveMgr AStarMoveMgr = item.MoveMgr as IAStarTBMoveMgr;
                if (AStarMoveMgr == null)
                    continue;
                if (!AStarMoveMgr.IsCanAutoExcuteMoveTarget()) 
                    continue;
                yield return Timing.WaitForOneFrame;
                if (AStarMoveMgr.ExcuteMoveTarget(false))
                {
                    if (item.IsPlayer())
                    {
                        CameraMgr.Jump(item);
                    }
                    //当单位正在移动或者暂停的时候,直接等待
                    while (
                        item.MoveMgr.IsMoving ||
                        IsPause
                        )
                    {
                        yield return Timing.WaitForOneFrame;
                    }
                }
            }
        }
        #endregion

        #region Callback
        protected virtual void OnSetMustProcess(T unit)
        {
            ClearMustProcess();
        }
        protected virtual void OnTurnOperating(T unit) => unit.OnTurnOperating();
        //回合开始:回调函数
        protected virtual void OnTurnStart(T unit,bool isForce)
        {
            unit.OnTurnStart(isForce);
            if (unit.IsPlayer())
                BaseInputMgr.PushUnitSelect(true);
        }
        //回合结束:回调函数
        protected virtual void OnTurnEnd(T unit)
        {
            unit.OnTurnEnd();
            if (unit.IsPlayer())
                BaseInputMgr.PushUnitSelect(false);
        }
        //无法结束回合:回调函数
        protected virtual void OnCantEndTurn(T unit) => unit.OnCantEndTurn();
        //结束回合前:回调函数
        protected virtual void OnPreEndTurn(T unit) => unit.OnPreEndTurn();
        //全新回合开始:回调函数
        protected virtual void OnTurnAllStart() { }
        //所有单位的回合都结束:回调函数
        protected virtual void OnTurnAllEnd()
        {
            SelfBaseGlobal.OnTurnbase(true,true,true);
            OperatedUnits.Clear();
            TurnCount++;
            BaseGlobal.DBMgr.AutoSave();
        }
        //玩家回合开始:回调函数
        protected virtual void OnTurnPlayerStart() { }
        //玩家回合结束:回调函数
        protected virtual void OnTurnPlayerEnd() { }
        //玩家 后-回合结束:回调函数
        protected virtual void OnTurnPlayerPostEnd() { }
        private void OnSetPlayer(BaseUnit arg1, BaseUnit arg2)
        {
            CurTurnIndex = 0;
            StartTurn(true);
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            _flagPause.Reset();
            CurOperatingUnit = null;
            BattleCoroutine.Kill(OnInEndTurnCoroutine);
            BattleCoroutine.Kill(OnInPostEndTurnCoroutine);
        }
        #endregion

        #region db
        public override void OnRead1(DBBaseGame data)
        {
            base.OnRead1(data);
            TurnCount = data.TurnCount;
        }
        public override void OnWrite(DBBaseGame data)
        {
            base.OnWrite(data);
            data.TurnCount = TurnCount;
        }
        #endregion
    }
}