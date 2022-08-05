//**********************************************
// Class Name	: Unit
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************


using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace CYM
{
    public struct DeathParam
    {
        public static DeathParam Default { get; private set; } = new DeathParam { IsDelayDespawn = true, Caster = null };
        public bool IsDelayDespawn ;
        public BaseUnit Caster;
    }
    /// <summary>
    /// 1.BaseUnit 的 NeedUpdate 和 NeedFixedUpdate 默认必须关掉,
    /// 2.因为其他的物件可能会继承BaseUnit,而很多物件不需要Update和FixedUpdate
    /// 3.OnEnable 时候会自动归位缩放
    /// </summary>
    public class BaseUnit : BaseCoreMono
    {
        #region list Componet
        public List<ISenseMgr> SenseMgrs { get; protected set; } = new List<ISenseMgr>();
        public Dictionary<Type, IUnitMgr> UnitMgrs { get; private set; } = new Dictionary<Type, IUnitMgr>();
        #endregion

        #region entity mgr
        public IUnitMgr UnitMgr { get; set; }
        public IUnitSpawnMgr<BaseUnit> SpawnMgr { get; set; }
        #endregion

        #region Componet
        public ISenseMgr SenseMgr { get; protected set; }
        public IAttrMgr AttrMgr { get; protected set; }
        public IBuffMgr BuffMgr { get; protected set; }
        public IHUDMgr HUDMgr { get; protected set; }
        public IMoveMgr MoveMgr { get; protected set; }
        public IAStarMoveMgr AStarMoveMgr => MoveMgr as IAStarMoveMgr;
        public IDipMgr DipMgr { get; protected set; }
        public IUStaffMgr StaffMgr { get; protected set; }
        public ICastleStationedMgr<BaseUnit> CastleStationedMgr { get; protected set; }
        public ILegionStationedMgr<BaseUnit> LegionStationedMgr { get; protected set; }
        public ISurfaceMgr<BaseModel> SurfMgr { get; protected set; }
        public IEventMgr<TDBaseEventData> EventMgr { get; protected set; }
        public IAlertMgr<TDBaseAlertData> AlertMgr { get; protected set; }
        public BaseDetectionMgr DetectionMgr { get; protected set; }
        public BaseNodeMgr NodeMgr { get; protected set; }
        public BaseAnimMgr AnimMgr { get; protected set; }
        public BaseVoiceMgr VoiceMgr { get; protected set; }
        public BaseAIMgr AIMgr { get; protected set; }
        public BaseFOWRevealerMgr FOWMgr { get; protected set; }
        public BasePerformMgr PerformMgr { get; protected set; }
        public BaseAStarMove2DMgr Move2DMgr { get; protected set; }
        public BaseMarkMgr MarkMgr { get; protected set; }
        public BaseTerritoryMgr TerritoryMgr { get; protected set; }
        #endregion

        #region inspector
        [FoldoutGroup("Base"), SerializeField, TextArea, Tooltip("用户自定义描述")]
        protected string Desc = "";
        [FoldoutGroup("Base"), SerializeField, Tooltip("单位的TDID")]
        protected new string TDID = "";
        [FoldoutGroup("Base"), MinValue(0), SerializeField, Tooltip("单位的队伍")]
        public int Team = 0;
        #endregion

        #region base
        public TDBaseData BaseConfig { get; protected set; } = new TDBaseData();
        public DBBaseUnit DBBaseData { get; protected set; } = new DBBaseUnit();
        //如果没有Owner，则Owner会设置为自身
        public BaseUnit BaseOwner { get; protected set; }
        public BaseUnit BasePreOwner { get; protected set; }
        public DeathParam DeathParam { get; private set; }
        #endregion

        #region prop
        protected bool IsCanGamePlayInput => BaseInputMgr.IsCanGamePlayInput();
        #endregion

        #region timer
        Timer DeathRealTimer = new Timer();
        Timer DeathEffStartTimer = new Timer();
        #endregion

        #region global mgr
        protected BaseInputMgr InputMgr => BaseGlobal.InputMgr;
        protected IScreenMgr<BaseUnit> ScreenMgr => BaseGlobal.ScreenMgr;
        #endregion

        #region Callback
        public event Callback<bool> Callback_OnTurnStart;
        public event Callback Callback_OnTurnEnd;
        public event Callback Callback_OnCantEndTurn;
        public event Callback Callback_OnPreEndTurn;
        public event Callback Callback_OnTurnOperating;
        public event Callback Callback_OnUnBeSetPlayer;
        public event Callback Callback_OnBeSetPlayer;
        public event Callback Callback_OnMouseDown;
        public event Callback Callback_OnMouseUp;
        public Callback Callback_OnMouseEnter { get; set; }
        public Callback Callback_OnMouseExit { get; set; }
        public event Callback<bool> Callback_OnBeSelected;
        public event Callback Callback_OnUnBeSelected;
        public event Callback<BaseUnit> Callback_OnSetOwner;
        public static Callback<BaseUnit> Callback_OnRealDeathG { get; internal set; }
        public static Callback<BaseUnit> Callback_OnDeathG { get; internal set; }
        #endregion

        #region time
        public virtual float DeathDespawnTime => DeathRealTime + 0.1f; // 彻底消除的时间
        public virtual float DeathRealTime => 3.0f; // 从Death到RealDeath的时间
        public virtual float DeathEffStartTime => 1.0f;// 死亡效果开始的时间
        #endregion

        #region life
        public override MonoType MonoType => MonoType.Unit;
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedFixedUpdate = true;
        }
        public override void OnEnable()
        {
            Trans.localScale = Vector3.one;
            base.OnEnable();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            UpdateRendered();
            if (!IsLive && !IsRealDeath)
            {
                if (DeathRealTimer.CheckOverOnce())
                    OnRealDeath();
                else if (DeathEffStartTimer.CheckOverOnce())
                    OnDissolve();
            }
        }
        public void OnTurnStart(bool isForce)
        {
            Callback_OnTurnStart?.Invoke(isForce);
        }
        public void OnTurnEnd()
        {
            Callback_OnTurnEnd?.Invoke();
        }
        public void OnCantEndTurn()
        {
            Callback_OnCantEndTurn?.Invoke();
        }
        public void OnPreEndTurn()
        {
            Callback_OnPreEndTurn?.Invoke();
        }
        public void OnTurnOperating()
        {
            Callback_OnTurnOperating?.Invoke();
        }
        #endregion

        #region unit life
        public override void OnInit()
        {
            IsLive = false;
            base.OnInit();
            if (DeathEffStartTime > DeathRealTime)
                throw new Exception("溶解的时间不能大于死亡时间");
        }
        public override void OnDeath()
        {
            if (!IsLive) return;
            IsLive = false;
            Callback_OnDeathG?.Invoke(this);
            DeathRealTimer.Restart(DeathRealTime);
            DeathEffStartTimer.Restart(DeathEffStartTime);
            UnitMgr?.Despawn(this);
            base.OnDeath();
        }
        public override void OnBirth()
        {
            if (IsLive) return;
            IsLive = true;
            IsRealDeath = false;
            base.OnBirth();
        }
        public override void OnRealDeath()
        {
            Callback_OnRealDeathG?.Invoke(this);
            IsRealDeath = true;
            base.OnRealDeath();
        }
        protected virtual void UpdateRendered()
        {
            if (BaseGlobal.CameraMgr == null)
                return;
            if (!BaseGlobal.CameraMgr.IsEnable)
                return;
            if (BaseGlobal.CameraMgr.MainCamera == null)
                return;
            Vector3 pos = BaseGlobal.CameraMgr.MainCamera.WorldToViewportPoint(Trans.position);
            IsRendered = (pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);
            if (IsRendered != IsLastRendered)
            {
                if (IsRendered) OnBeRender();
                else OnBeUnRender();

                IsLastRendered = IsRendered;
            }
        }
        protected virtual void OnBecameInvisible() => IsVisible = false;
        protected virtual void OnBecameVisible() => IsVisible = true;
        #endregion

        #region life set
        public override T AddComponent<T>()
        {
            var ret = base.AddComponent<T>();
            //加入组件列表
            if (ret is IUnitMgr entityMgr) UnitMgrs.Add(entityMgr.UnitType, entityMgr);
            if (ret is ISenseMgr senseMgr) SenseMgrs.Add(senseMgr);
            //添加组件
            if (ret is ISenseMgr && SenseMgr == null) SenseMgr = ret as ISenseMgr;
            else if (ret is IAttrMgr && AttrMgr == null) AttrMgr = ret as IAttrMgr;
            else if (ret is IBuffMgr && BuffMgr == null) BuffMgr = ret as IBuffMgr;
            else if (ret is IHUDMgr && HUDMgr == null) HUDMgr = ret as IHUDMgr;
            else if (ret is IMoveMgr && MoveMgr == null) MoveMgr = ret as IMoveMgr;
            else if (ret is IDipMgr && DipMgr == null) DipMgr = ret as IDipMgr;
            else if (ret is IUStaffMgr && StaffMgr == null) StaffMgr = ret as IUStaffMgr;
            else if (ret is ILegionStationedMgr<BaseUnit> && LegionStationedMgr == null) LegionStationedMgr = ret as ILegionStationedMgr<BaseUnit>;
            else if (ret is ICastleStationedMgr<BaseUnit> && CastleStationedMgr == null) CastleStationedMgr = ret as ICastleStationedMgr<BaseUnit>;
            else if (ret is ISurfaceMgr<BaseModel> && SurfMgr == null) SurfMgr = ret as ISurfaceMgr<BaseModel>;
            else if (ret is IEventMgr<TDBaseEventData> && EventMgr == null) EventMgr = ret as IEventMgr<TDBaseEventData>;
            else if (ret is IAlertMgr<TDBaseAlertData> && AlertMgr == null) AlertMgr = ret as IAlertMgr<TDBaseAlertData>;
            else if (ret is BaseDetectionMgr && DetectionMgr == null) DetectionMgr = ret as BaseDetectionMgr;
            else if (ret is BaseNodeMgr && NodeMgr == null) NodeMgr = ret as BaseNodeMgr;
            else if (ret is BaseAnimMgr && AnimMgr == null) AnimMgr = ret as BaseAnimMgr;
            else if (ret is BaseVoiceMgr && VoiceMgr == null) VoiceMgr = ret as BaseVoiceMgr;
            else if (ret is BaseAIMgr && AIMgr == null) AIMgr = ret as BaseAIMgr;
            else if (ret is BaseFOWRevealerMgr && FOWMgr == null) FOWMgr = ret as BaseFOWRevealerMgr;
            else if (ret is BasePerformMgr && PerformMgr == null) PerformMgr = ret as BasePerformMgr;
            else if (ret is BaseAStarMove2DMgr && Move2DMgr == null) Move2DMgr = ret as BaseAStarMove2DMgr;
            else if (ret is BaseMarkMgr && MarkMgr == null) MarkMgr = ret as BaseMarkMgr;
            else if (ret is BaseTerritoryMgr && TerritoryMgr == null) TerritoryMgr = ret as BaseTerritoryMgr;
            return ret;
        }
        #endregion

        #region set
        // 设置小队
        public void SetTeam(int? team)
        {
            if (team.HasValue)
                Team = team.Value;
        }
        // 设置TDID
        public void SetTDID(string tdid)
        {
            if (tdid.IsInv())
            {
                base.TDID = TDID = gameObject.name;
            }
            else
            {
                base.TDID = TDID = tdid;
                if (int.TryParse(tdid,out int intID))
                {
                    INID = intID;
                }
            }
        }
        public virtual void SetRTID(long rtid) => ID = rtid;
        public virtual void SetConfig(TDBaseData config)
        {
            if (config == null) 
                config = new TDBaseData();
            BaseConfig = config;
            BaseConfig.OnBeAdded(this);
        }
        public void SetOwner(BaseUnit owner)
        {
            BasePreOwner = BaseOwner;
            BaseOwner = owner;
            //只有非读取数据阶段才能触发回调
            if (BaseGlobal.IsUnReadData)
            {
                Callback_OnSetOwner?.Invoke(owner);
            }
        }
        //设置DB数据
        public virtual void SetDBData(DBBaseUnit dbData)
        {
            DBBaseData = dbData;
        }
        //设置死亡参数
        public void SetDeathParam(DeathParam param)
        {
            DeathParam = param;
        }
        //执行删除动作，死亡，解散等
        public virtual void DoDeath()
        {
            OnDeath();
            Clear();
            SpawnMgr?.Despawn(this, DeathParam.IsDelayDespawn ? DeathDespawnTime : 0);
        }
        public override void OnDisable()
        {
            base.OnDisable();
            Clear();
        }
        #endregion

        #region get
        public virtual string GetTDID() => TDID;
        //获得综合评分
        public float Score =>0;
        public override string ToString()
        {
            if (BaseConfig == null) return base.ToString();
            return BaseConfig.GetName();
        }
        public string GetName()
        {
            if (BaseConfig == null) return base.ToString();
            return BaseConfig.GetName();
        }
        public string GetDesc()
        {
            if (BaseConfig == null) return base.ToString();
            return BaseConfig.GetDesc();
        }
        public Sprite GetIcon()
        {
            if (BaseConfig == null) return null;
            return BaseConfig.GetIcon();
        }
        public IUnitMgr GetUnitMgr(Type unitType)
        {
            if (UnitMgrs.ContainsKey(unitType))
            {
                return UnitMgrs[unitType];
            }
            return null;
        }
        #endregion

        #region is
        // 是否为系统类型
        public bool IsSystem { get; set; }
        // 是否为荒野类型
        public virtual bool IsWild => BaseConfig.IsWild;
        // 是否死亡
        public bool IsLive { get; protected set; } = false;
        // 是否真的死亡
        public bool IsRealDeath { get; protected set; } = false;
        // 是否被渲染(计算位置是否在摄像机中)
        public bool IsRendered { get; private set; } = false;
        // 是否被摄像机渲染
        public bool IsVisible { get; private set; } = false;
        // 上一帧被渲染
        public bool IsLastRendered { get; private set; } = false;
        // 是否为本地玩家
        public virtual bool IsPlayer() => ScreenMgr.Player == this;
        // 是否为其他玩家
        public virtual bool IsPlayerCtrl() => IsPlayer();
        public virtual bool IsAI() => !IsPlayerCtrl();
        // 是否是敌人
        public virtual bool IsEnemy(BaseUnit other)
        {
            if (other == null)
                return false;
            return other.Team != Team;
        }
        // 是否是友军
        public virtual bool IsFriend(BaseUnit other)
        {
            if (other == null)
                return false;
            return other.Team == Team;
        }
        // Self or Friend
        public virtual bool IsSOF(BaseUnit other)
        {
            if (other == null)
                return false;
            return IsFriend(other) || IsSelf(other);
        }
        // 是否为本地玩家的对立面
        public virtual bool IsOpposite() => false;
        // 是否为自己
        public virtual bool IsSelf(BaseUnit other)
        {
            if (other == null)
                return false;
            return this == other;
        }
        // 是否为中立怪
        public virtual bool IsNeutral() => Team == 2;
        // 非中立怪 敌人
        public virtual bool IsUnNeutralEnemy(BaseUnit other)
        {
            if (other == null)
                return false;
            if (other.IsNeutral())
                return false;
            return IsEnemy(other);
        }
        #endregion

        #region other
        public float Importance { get; set; }
        #endregion

        #region Callback
        protected virtual void OnBeRender() { }
        protected virtual void OnBeUnRender() { }
        protected virtual void OnMouseDown()
        {
            if (Application.isMobilePlatform) return;
            if (!IsCanGamePlayInput) return;
            if (!BaseGlobal.BattleMgr.IsLoadBattleEnd) return;
            Callback_OnMouseDown?.Invoke();
        }
        protected virtual void OnMouseUp()
        {
            if (Application.isMobilePlatform) return;
            if (!IsCanGamePlayInput) return;
            if (!BaseGlobal.BattleMgr.IsLoadBattleEnd) return;
            Callback_OnMouseUp?.Invoke();
        }
        protected virtual void OnMouseEnter()
        {
            if (Application.isMobilePlatform) 
                return;
            if (!BaseGlobal.BattleMgr.IsLoadBattleEnd)
                return;
            if (BaseInputMgr.IsStayInUI)
                return;
            InputMgr.DoEnterUnit(this);
        }

        protected virtual void OnMouseExit()
        {
            if (Application.isMobilePlatform) 
                return;
            if (!BaseGlobal.BattleMgr.IsLoadBattleEnd) 
                return;
            if (BaseInputMgr.IsStayInUI)
                return;
            InputMgr.DoExitUnit(this);
        }

        public virtual void OnBeSelected(bool isRepeat)
        {
            if (!IsCanGamePlayInput) return;
            Callback_OnBeSelected?.Invoke(isRepeat);
        }
        public virtual void OnUnBeSelected()
        {
            Callback_OnUnBeSelected?.Invoke();
        }
        public virtual void OnUnBeSetPlayer()
        {
            Callback_OnUnBeSetPlayer?.Invoke();
        }
        public virtual void OnBeSetPlayer()
        {
            Callback_OnBeSetPlayer?.Invoke();
        }

        #endregion

        #region inspector
        [Button("CopyName")]
        void CopyName()
        {
            Util.CopyTextToClipboard(GOName);
        }
        public virtual void AdjHeight()
        { 
        
        }
        #endregion
    }
}