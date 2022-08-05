//------------------------------------------------------------------------------
// BaseTransactionMgr.cs
// Copyright 2019 2019/7/29 
// Created by CYM on 2019/7/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM
{
    public class BaseTransact<TUnit> where TUnit : BaseUnit
    {
        #region prop
        public string Type { get; protected set; }
        //给出方
        public TUnit Self { get; protected set; }
        //收入方
        public TUnit Target { get; protected set; }
        public float Value { get; protected set; }
        public CD Time { get; protected set; } = new CD();
        #endregion

        public BaseTransact(string type, TUnit self, TUnit target, float val)
        {
            Self = self;
            Target = target;
            Value = val;
            Type = type;
        }

    }
    public class BaseTransactionMgr<TUnit> : BaseGFlowMgr, IDBListConverMgr<DBBaseTransact> where TUnit : BaseUnit
    {
        #region prop
        const string TypeIndemnity = "Indemnity";
        Dictionary<string, Dictionary<string, BaseTransact<TUnit>>> Data = new Dictionary<string, Dictionary<string, BaseTransact<TUnit>>>();
        List<BaseTransact<TUnit>> needRemoveTransact = new List<BaseTransact<TUnit>>();
        #endregion

        #region Callback
        public event Callback<TUnit, TUnit> Callback_OnTransactChange;
        #endregion

        #region Manual Update
        protected void ManualUpdateTransact()
        {
            needRemoveTransact.Clear();
            foreach (var item in Data)
            {
                foreach (var item2 in item.Value)
                {
                    item2.Value.Time.Update();
                    if (item2.Value.Time.IsOver())
                        needRemoveTransact.Add(item2.Value);
                }
            }
            foreach (var item in Data)
            {
                foreach (var item2 in needRemoveTransact)
                    RemoveTransact(item2.Type, item2.Self, item2.Target);
            }
        }
        #endregion

        #region set
        //添加赔款
        public virtual void AddIndemnity(TUnit self, TUnit target, float val, int cd) => AddTransact(TypeIndemnity, self, target, val, cd);
        //移除赔款
        public virtual void RemoveIndemnity(TUnit self, TUnit target) => RemoveTransact(TypeIndemnity, self, target);
        public List<BaseTransact<TUnit>> GetIndemnityIncome(TUnit unit) => GetTransactIncome(TypeIndemnity, unit);
        public List<BaseTransact<TUnit>> GetIndemnityExpense(TUnit unit) => GetTransactExpense(TypeIndemnity, unit);
        #endregion

        #region utitle
        protected virtual void AddTransact(string type, TUnit self, TUnit target, float val, int cd)
        {
            if (!Data.ContainsKey(type))
                Data[type] = new Dictionary<string, BaseTransact<TUnit>>();
            string key = self.TDID + target.TDID;
            BaseTransact<TUnit> data = new BaseTransact<TUnit>(type, self, target, val);
            data.Time.Reset(cd);
            if (Data[type].ContainsKey(key)) Data[type][key] = data;
            else Data[type].Add(key, data);
            Callback_OnTransactChange?.Invoke(self, target);
        }
        protected virtual void RemoveTransact(string type, TUnit self, TUnit target)
        {
            if (!Data.ContainsKey(type))
                Data[type] = new Dictionary<string, BaseTransact<TUnit>>();
            Data[type].Remove(self.TDID + target.TDID);
            Data[type].Remove(target.TDID + self.TDID);
            Callback_OnTransactChange?.Invoke(self, target);
        }
        protected List<BaseTransact<TUnit>> GetTransactIncome(string type, TUnit unit)
        {
            if (!Data.ContainsKey(type))
                Data[type] = new Dictionary<string, BaseTransact<TUnit>>();
            List<BaseTransact<TUnit>> ret = new List<BaseTransact<TUnit>>();
            foreach (var item in Data[type])
            {
                if (item.Value.Target == unit)
                    ret.Add(item.Value);
            }
            return ret;
        }
        protected List<BaseTransact<TUnit>> GetTransactExpense(string type, TUnit unit)
        {
            if (!Data.ContainsKey(type))
                Data[type] = new Dictionary<string, BaseTransact<TUnit>>();
            List<BaseTransact<TUnit>> ret = new List<BaseTransact<TUnit>>();
            foreach (var item in Data[type])
            {
                if (item.Value.Self == unit)
                    ret.Add(item.Value);
            }
            return ret;
        }

        #endregion

        #region Callback
        protected override void OnBattleUnLoaded()
        {
            Data.Clear();
            base.OnBattleUnLoaded();
        }
        #endregion

        #region DB
        public void LoadDBData(ref List<DBBaseTransact> data)
        {
            Data.Clear();
            foreach (var item in data)
            {
                if (!Data.ContainsKey(item.Type))
                    Data[item.Type] = new Dictionary<string, BaseTransact<TUnit>>();
                BaseTransact<TUnit> transact = new BaseTransact<TUnit>(item.Type, GetEntity<TUnit>(item.Self), GetEntity<TUnit>(item.Target), item.Value);
                transact.Time.Reset(item.CD);
                Data[item.Type].Add(item.TDID, transact);
            }
        }
        public void SaveDBData(ref List<DBBaseTransact> ret)
        {
            ret = new List<DBBaseTransact>();
            foreach (var item in Data)
            {
                foreach (var item2 in item.Value)
                {
                    DBBaseTransact data = new DBBaseTransact();
                    data.Type = item2.Value.Type;
                    data.TDID = item2.Key;
                    data.Self = item2.Value.Self.ID;
                    data.Target = item2.Value.Target.ID;
                    data.Value = item2.Value.Value;
                    data.CD = (int)item2.Value.Time.GetRemainder();
                    ret.Add(data);
                }
            }
        }
        #endregion
    }
}