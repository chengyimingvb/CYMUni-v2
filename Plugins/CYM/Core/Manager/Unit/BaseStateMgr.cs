//------------------------------------------------------------------------------
// BaseStateMgr.cs
// Copyright 2019 2019/4/3 
// Created by CYM on 2019/4/3
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.AI.USM;
using System;
namespace CYM
{
    [Serializable]
    public class DBBaseState
    {
        public int State = 0;
        public long Target = Const.INT_Inv;
    }
    public class BaseStateMgr<TUnit, TState> : BaseMgr 
        where TState : Enum 
        where TUnit : BaseUnit
    {
        #region Callback
        public event Callback<TState, TState> Callback_OnChangeState;
        #endregion

        #region prop
        public CharaStateMachine<TState, TUnit, BaseCharaState> Machine { get; private set; } = new CharaStateMachine<TState, TUnit, BaseCharaState>();
        #endregion

        #region Val
        public TState CurState { get; private set; }
        public TState PreState { get; private set; }
        public BaseUnit Target { get; protected set; }
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
            NeedFixedUpdate = true;
        }
        public override void OnInit()
        {
            base.OnInit();
            Machine.Init(SelfBaseUnit as TUnit);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            Machine.OnUpdate();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            Machine.OnFixedUpdate();
        }
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            SetState(CurState, false);
        }
        #endregion

        #region set
        protected virtual void SetTarget(BaseUnit target)
        {
            Target = target;
        }
        /// <summary>
        ///直接改变一个状态
        /// </summary>
        /// <param name="state"></param>
        public virtual void Change(TState state, bool isForce = false, bool isManual = true)
        {
            Machine.ChangeState(state, isForce, isManual);
            Callback_OnChangeState?.Invoke(CurState, PreState);
        }
        /// <summary>
        /// 设置状态
        /// </summary>
        public void SetState(TState state, bool isManual = true)
        {
            Machine.SetCurState(state, isManual);
        }
        /// <summary>
        /// 添加一个状态
        /// </summary>
        /// <param name="type"></param>
        /// <param name="state"></param>
        public void AddState(TState type, BaseCharaState state)
        {
            Machine.AddState(type, state);

        }
        #endregion

        #region get
        public CharaState<TState, TUnit> GetState(TState state)
        {
            return Machine.GetState(state);
        }
        public CharaState<TState, TUnit> GetCurState()
        {
            return GetState(CurState);
        }
        #endregion

        #region is
        /// <summary>
        /// 是否在指定状态
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public bool IsIn(TState state)
        {
            return Machine.IsIn(state);
        }
        /// <summary>
        /// 判断上一个状态
        /// </summary>
        /// <param name="state"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool IsInPreState(TState state, int index)
        {
            return Machine.IsInPreState(state, index);
        }
        #endregion

        #region DB
        public void Load(DBBaseState data)
        {
            CurState = (TState)(object)data.State;
            Target = GetEntity(data.Target);
        }
        public DBBaseState GetDBData()
        {
            DBBaseState ret = new DBBaseState();
            ret.State = Enum<TState>.Int(CurState);
            ret.Target = Target != null ? Target.ID : Const.INT_Inv;
            return ret;
        }
        #endregion

        #region base state
        public class BaseCharaState : CharaState<TState, TUnit>
        {
            public BaseStateMgr<TUnit, TState> StateMgr { get; set; }
            public BaseCharaState() : base() { }
            public override void Update()
            {
                base.Update();
                if (UpdateTime >= Wait) { }
            }
            protected void Change(TState state, bool isForce = false, bool isManual = true) => StateMgr.Change(state, isForce, isManual);
            protected bool IsCurrentState() => StateMgr.IsIn(State);
        }
        #endregion
    }
}