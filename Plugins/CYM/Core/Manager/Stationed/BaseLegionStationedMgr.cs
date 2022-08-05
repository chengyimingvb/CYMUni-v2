//------------------------------------------------------------------------------
// BaseLegionStationedMgr.cs
// Copyright 2020 2020/3/24 
// Created by CYM on 2020/3/24
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

namespace CYM
{
    public class BaseLegionStationedMgr<TUnit, TCastle> : BaseUFlowMgr<TUnit>, ILegionStationedMgr<TUnit>, IDBConverMgr<DBBaseLegionStationed>
        where TUnit : BaseUnit
        where TCastle : BaseUnit
    {
        #region Callback
        public event Callback<TCastle, TCastle> Callback_OnDefendChanged;
        public event Callback<TCastle, TCastle> Callback_OnSiegeChanged;
        #endregion

        #region prop
        protected BaseAStarMgr AStarMgr => BaseGlobal.AStarMgr;
        #endregion

        #region val
        public TCastle DefendCastle { get; protected set; }
        public TCastle PreDefendCastle { get; protected set; }
        public TCastle SiegeCastle { get; protected set; }
        public TCastle PreSiegeCastle { get; protected set; }
        #endregion

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            SelfBaseUnit.MoveMgr.Callback_OnMoveStart += OnMoveStart;
            SelfBaseUnit.MoveMgr.Callback_OnFirstMovingAlone += OnFirstMovingAlone;
        }

        public override void OnDeath()
        {
            base.OnDeath();
            LeaveDefend();
            LeaveSiege();
        }
        #endregion

        #region set
        public void LeaveBlocker(BaseUnit unit)
        {
            if (unit == null) return;
            var castle = unit as TCastle;
            if (castle == null)
            {
                CLog.Error("LeaveBlocker:castle == null)");
                return;
            }
            if (IsInDefend()) return;
            AStarMgr.SetArroundUnit(castle, SelfUnit);
            OnLeaveBlocker(castle);
        }
        public void LeaveDefend(bool isForce = false)
        {
            if (!IsInDefend()) return;
            PreDefendCastle = DefendCastle;
            DefendCastle = null;
            PreDefendCastle.CastleStationedMgr.OnUnBeDefend(SelfUnit);
            if (isForce)
            {
                AStarMgr.SetArroundUnit(PreDefendCastle, SelfUnit);
            }
            OnLeaveDefend(isForce);
            Callback_OnDefendChanged?.Invoke(PreDefendCastle, null);
        }
        public void Defend(BaseUnit unit, bool isCustomMoveLegion = true)
        {
            if (unit == null) return;
            var castle = unit as TCastle;
            if (castle == null)
            {
                CLog.Error("Defend:castle == null)");
                return;
            }
            if (IsInDefend()) return;
            if (!SelfUnit.IsSelf(castle)) return;
            if (castle.CastleStationedMgr.IsHaveDefender()) return;
            PreDefendCastle = DefendCastle;
            DefendCastle = castle;
            castle.CastleStationedMgr.OnBeDefend(SelfUnit);
            OnDefend(castle, isCustomMoveLegion);
            Callback_OnDefendChanged?.Invoke(PreDefendCastle, DefendCastle);
        }
        public void LeaveSiege(bool isForce = false)
        {
            if (IsInDefend()) return;
            if (!IsInSiege()) return;
            PreSiegeCastle = SiegeCastle;
            SiegeCastle = null;
            PreSiegeCastle.CastleStationedMgr.OnUnSiege(SelfUnit);
            OnLeaveSiege(isForce);
            Callback_OnSiegeChanged?.Invoke(PreSiegeCastle, null);
        }
        public void Siege(BaseUnit unit)
        {
            if (unit == null) return;
            var castle = unit as TCastle;
            if (castle == null)
            {
                CLog.Error("Siege:castle == null)");
                return;
            }
            if (IsInDefend()) return;
            if (IsInSiege()) return;
            if (!SelfUnit.IsEnemy(castle)) return;
            PreSiegeCastle = SiegeCastle;
            SiegeCastle = castle;
            castle.CastleStationedMgr.OnBeSiege(SelfUnit);
            OnSiege();
            Callback_OnSiegeChanged?.Invoke(PreSiegeCastle, SiegeCastle);
        }
        public void TestLeaveSiege()
        {
            if (IsInSiege() && !SelfUnit.IsEnemy(SiegeCastle))
                LeaveSiege(true);
        }
        #endregion

        #region is
        public bool IsInSiegeDefend()
        {
            if (IsInDefend())
            {
                if (DefendCastle?.CastleStationedMgr != null)
                { 
                    return DefendCastle.CastleStationedMgr.IsInSiege();
                }
            }
            return false;
        }
        public bool IsInDefend() => DefendCastle != null;
        public bool IsInSiege() => SiegeCastle != null;
        public bool IsInDefend(TCastle castle) => DefendCastle == castle;
        public bool IsInSiege(TCastle castle) => SiegeCastle == castle;
        #endregion

        #region Callback
        protected virtual void OnMoveStart()
        {

        }
        protected virtual void OnFirstMovingAlone()
        {
            LeaveDefend();
            LeaveSiege();
        }
        protected virtual void OnLeaveBlocker(TCastle castle)
        {

        }
        protected virtual void OnLeaveDefend(bool isForce)
        {

        }
        protected virtual void OnDefend(TCastle castle, bool isCustomMoveLegion)
        {

        }
        protected virtual void OnSiege()
        {

        }
        protected virtual void OnLeaveSiege(bool isForce)
        {

        }
        #endregion

        #region DB
        public void LoadDBData(ref DBBaseLegionStationed data)
        {
            if (data == null) return;
            DefendCastle = GetEntity<TCastle>(data.DefendCastle);
            PreDefendCastle = GetEntity<TCastle>(data.PreDefendCastle);
            SiegeCastle = GetEntity<TCastle>(data.SiegeCastle);
            PreSiegeCastle = GetEntity<TCastle>(data.PreSiegeCastle);
        }

        public void SaveDBData(ref DBBaseLegionStationed ret)
        {
            if (DefendCastle) ret.DefendCastle = DefendCastle.ID;
            if (PreDefendCastle) ret.PreDefendCastle = PreDefendCastle.ID;
            if (SiegeCastle) ret.SiegeCastle = SiegeCastle.ID;
            if (PreSiegeCastle) ret.PreSiegeCastle = PreSiegeCastle.ID;
        }
        #endregion
    }
}