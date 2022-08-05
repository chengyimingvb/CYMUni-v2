//------------------------------------------------------------------------------
// BaseCastleStationedMgr.cs
// Copyright 2020 2020/3/24 
// Created by CYM on 2020/3/24
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM
{
    public class BaseCastleStationedMgr<TUnit, TLegion> : BaseUFlowMgr<TUnit>, ICastleStationedMgr<TUnit>, IDBConverMgr<DBBaseCastleStationed>
        where TUnit : BaseUnit
        where TLegion : BaseUnit
    {
        #region Callback
        public event Callback<TLegion, TLegion, bool> Callback_OnDefendChanged;
        public event Callback<HashList<TLegion>, TLegion> Callback_OnAttackChanged;
        #endregion

        #region prop
        protected BaseAStarMgr AStarMgr => BaseGlobal.AStarMgr;
        #endregion

        #region val
        public TLegion Defender { get; protected set; }
        public TLegion PreDefendLegion { get; protected set; }
        public HashList<TLegion> Attackers { get; protected set; } = new HashList<TLegion>();
        #endregion

        #region set
        public void MoveoutLegion()
        {
            var inBlockerUnits = AStarMgr.GetBlockerUnits(SelfUnit, false);
            foreach (var item in inBlockerUnits)
            {
                if (SelfUnit.IsSelf(item)) continue;
                if (item is TLegion legion)
                    legion.LegionStationedMgr.LeaveBlocker(SelfUnit);
            }
            if (IsHaveDefender())
            {
                Defender.LegionStationedMgr.LeaveDefend(true);
            }
            if (IsInSiege())
            {
                Attackers.ForSafe(x =>
                {
                    x.LegionStationedMgr.LeaveSiege(true);
                });
            }
        }
        #endregion

        #region is
        public virtual bool IsCanDefend()
        {
            return true;
        }
        public bool IsHaveDefender() => Defender != null;
        public bool IsInSiege() => Attackers.Count > 0;
        #endregion

        #region Callback
        public virtual void OnBeDefend(BaseUnit unit)
        {
            var legion = unit as TLegion;
            PreDefendLegion = Defender;
            Defender = legion;
            Callback_OnDefendChanged?.Invoke(PreDefendLegion, Defender, IsHaveDefender());
        }
        public virtual void OnUnBeDefend(BaseUnit unit)
        {
            var legion = unit as TLegion;
            PreDefendLegion = Defender;
            Defender = null;
            Callback_OnDefendChanged?.Invoke(PreDefendLegion, Defender, IsHaveDefender());
        }
        public virtual void OnBeSiege(BaseUnit unit)
        {
            var legion = unit as TLegion;
            Attackers.Add(legion);
            Callback_OnAttackChanged?.Invoke(Attackers, legion);

        }
        public virtual void OnUnSiege(BaseUnit unit)
        {
            var legion = unit as TLegion;
            Attackers.Remove(legion);
            Callback_OnAttackChanged?.Invoke(Attackers, legion);
        }
        #endregion

        #region DB
        public void LoadDBData(ref DBBaseCastleStationed data)
        {
            if (data == null) return;
            Defender = GetEntity<TLegion>(data.DefendLegion);
            PreDefendLegion = GetEntity<TLegion>(data.PreDefendLegion, false);
            if (data.AttackLegion != null)
            {
                foreach (var item in data.AttackLegion)
                {
                    Attackers.Add(GetEntity<TLegion>(item));
                }
            }
        }

        public void SaveDBData(ref DBBaseCastleStationed ret)
        {
            if (!Defender.IsInv()) ret.DefendLegion = Defender.ID;
            if (!PreDefendLegion.IsInv()) ret.PreDefendLegion = PreDefendLegion.ID;
            if (Attackers != null)
            {
                ret.AttackLegion = new List<long>();
                foreach (var item in Attackers)
                {
                    ret.AttackLegion.Add(item.ID);
                }
            }
        }
        #endregion
    }
}