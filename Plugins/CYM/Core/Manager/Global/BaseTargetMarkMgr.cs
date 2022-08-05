//------------------------------------------------------------------------------
// BaseTargetMarkMgr.cs
// Copyright 2020 2020/7/13 
// Created by CYM on 2020/7/13
// Owner: CYM
// 用来记忆目标,通过CD来计时,Cd过完后自动移除记忆的目标,通常用于AI的战略判断
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM
{
    public class BaseTargetMarkMgr<TUnit, TTarget> : BaseGFlowMgr,IDBListConverMgr<DBBaseTargetMarker>
        where TUnit : BaseUnit
        where TTarget : BaseUnit
    {
        #region prop
        public HashSet<TTarget> Targets { get; private set; } = new HashSet<TTarget>();
        public CDUpdater<KeyValuePair<TUnit, TTarget>> UnitsCDUpdater { get; private set; } = new CDUpdater<KeyValuePair<TUnit, TTarget>>();
        public Dictionary<TTarget, HashSet<TUnit>> TargetUnits { get; private set; } = new Dictionary<TTarget, HashSet<TUnit>>();
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            UnitsCDUpdater.Callback_OnAdded += OnUnitsAdded;
            UnitsCDUpdater.Callback_OnRemoved += OnUnitsRemoved;
        }
        #endregion

        #region Manual Update
        protected void ManualUpdateData()
        {
            UnitsCDUpdater.Update();
        }
        #endregion

        #region set
        public void MarkTarget(TUnit unit, TTarget target, int cd = 3)
        {
            if (unit == null) return;
            if (target == null) return;
            var kv = new KeyValuePair<TUnit, TTarget>(unit, target);
            if (UnitsCDUpdater.ContainsKey(kv))
                UnitsCDUpdater[kv].Reset(cd);
            else 
                UnitsCDUpdater.AddCD(kv, cd);
        }
        #endregion

        #region is 
        public bool IsMarkedByEnemy(BaseUnit owner, TTarget target)
        {
            if (owner == null) return false;
            if (target == null) return false;
            if (!target.IsLive) return false;
            if (!TargetUnits.ContainsKey(target)) return false;
            bool ret = Targets.Contains(target);
            if (ret)
            {
                foreach (var item in TargetUnits[target])
                {
                    if (!item.IsLive)
                        continue;
                    if (owner.IsEnemy(item))
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        public bool IsMarkedBySelf(BaseUnit owner, TTarget target)
        {
            if (owner == null) return false;
            if (target == null) return false;
            if (!target.IsLive) return false;
            if (!TargetUnits.ContainsKey(target)) return false;
            bool ret = Targets.Contains(target);
            if (ret)
            {
                foreach (var item in TargetUnits[target])
                {
                    if (!item.IsLive)
                        continue;
                    if (owner.IsSelf(owner))
                    {
                        return true;
                    }
                }
                return false;
            }
            return false;
        }
        #endregion

        #region get
        public int GetMarkedBySelfCount(BaseUnit owner, TTarget target)
        {
            if (owner == null) return 0;
            if (target == null) return 0;
            if (!target.IsLive) return 0;
            if (!TargetUnits.ContainsKey(target)) return 0;
            bool ret = Targets.Contains(target);
            int count = 0;
            if (ret)
            {
                foreach (var item in TargetUnits[target])
                {
                    if (!item.IsLive)
                        continue;
                    if (owner.IsSelf(owner))
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        public int GetMarkedByEnemyCount(BaseUnit owner, TTarget target)
        {
            if (owner == null) return 0;
            if (target == null) return 0;
            if (!target.IsLive) return 0;
            if (!TargetUnits.ContainsKey(target)) return 0;
            bool ret = Targets.Contains(target);
            int count = 0;
            if (ret)
            {
                foreach (var item in TargetUnits[target])
                {
                    if (!item.IsLive)
                        continue;
                    if (owner.IsEnemy(item))
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        #endregion

        #region Callback
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            Targets.Clear();
            UnitsCDUpdater.Clear();
            TargetUnits.Clear();
        }
        private void OnUnitsAdded(KeyValuePair<TUnit, TTarget> arg1, CD arg2)
        {
            if (!TargetUnits.ContainsKey(arg1.Value)) 
                TargetUnits.Add(arg1.Value, new HashSet<TUnit>());

            TargetUnits[arg1.Value].Add(arg1.Key);

            if (TargetUnits[arg1.Value].Count > 0)
                Targets.Add(arg1.Value);
        }
        private void OnUnitsRemoved(KeyValuePair<TUnit, TTarget> arg1)
        {
            if (!TargetUnits.ContainsKey(arg1.Value))
                TargetUnits.Add(arg1.Value, new HashSet<TUnit>());

            TargetUnits[arg1.Value].Remove(arg1.Key);

            if (TargetUnits[arg1.Value].Count <= 0)
                Targets.Remove(arg1.Value);
        }
        #endregion

        #region DB
        public void LoadDBData(ref List<DBBaseTargetMarker> data)
        {
            if (data == null) return;
            foreach (var item in data)
            {
                var unit = GetEntity<TUnit>(item.Unit);
                var target = GetEntity<TTarget>(item.Target);
                MarkTarget(unit, target, (int)item.CD);
            }
        }

        public void SaveDBData(ref List<DBBaseTargetMarker> ret)
        {
            ret = new List<DBBaseTargetMarker>();
            foreach (var item in UnitsCDUpdater)
            {
                var cd = item.Value.GetRemainder();
                if (cd <= 0) 
                    continue;
                if (!item.Key.Key.IsLive ||
                   !item.Key.Value.IsLive)
                    continue;
                DBBaseTargetMarker data = new DBBaseTargetMarker();
                data.Unit = item.Key.Key.ID;
                data.Target = item.Key.Value.ID;
                data.CD = cd;
                ret.Add(data);
            }
        }
        #endregion
    }
}