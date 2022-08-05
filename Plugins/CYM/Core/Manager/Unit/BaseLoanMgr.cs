//------------------------------------------------------------------------------
// BaseLoanMgr.cs
// Copyright 2019 2019/11/10 
// Created by CYM on 2019/11/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace CYM
{
    /// <summary>
    /// 贷款结构体
    /// </summary>
    [Serializable]
    public class LoanData : Queue
    {
        public string Date;
        public float Gold;//贷款金额
        public float Interest;//每月利率
        public float Increase = 0;//违约增加利息
        public LoanData():base()
        {
        }
        //获得利息
        public float GetInterestGold()
        {
            return (Gold * Interest / Mathf.Max(1, TotalCout)) * (1 + Increase);
        }
        public float GetCount()
        {
            return MathUtil.Clamp0(CurCount);
        }
        public override void Update(float step)
        {
            CurCount -= 1;
            if (IsOver())
            {
                Increase += 0.033f;
            }
        }
        public override bool IsOver()
        {
            return CurCount <= 0;
        }

        #region data
        public LoanData(SerializationInfo info, StreamingContext context): base(info, context)
        {
            Date = (string)info.GetValue("Date", typeof(string));
            Gold = (float)info.GetValue("Gold", typeof(float));
            Interest= (float)info.GetValue("Interest", typeof(float));
            Increase = (float)info.GetValue("Increase", typeof(float));
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Date", Date);
            info.AddValue("Gold", Gold);
            info.AddValue("Interest", Interest);
            info.AddValue("Increase", Increase);
        }
        #endregion
    }

    public class BaseLoanMgr : BaseMgr, IDBConverMgr<DBBaseLoan>
    {
        #region Callback val
        public event Callback Callback_OnLoanChanged;
        #endregion

        #region prop
        BaseDateTimeMgr DateTimeMgr => BaseGlobal.DateTimeMgr;
        // 当前已经贷款的额度
        public float CurLoan { get; protected set; } = 0;
        // 贷款的数据
        public List<LoanData> LoanDatas { get; protected set; } = new List<LoanData>();
        public List<LoanData> ExpireLoan { get; protected set; } = new List<LoanData>();
        #endregion

        #region must override
        public virtual float CurGold => 100;
        public virtual float OnceLoanGold => MathUtil.Clamp(MaxLoan / 10, 100, 300);
        public virtual float Interest => 0.05f;
        public virtual float LoanPeriod => 10;
        public virtual float MaxLoanAdt => 10000;
        public virtual float MaxLoan => 10000;
        protected virtual void ChangeGold(float gold) { }
        #endregion 

        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        public override void OnGameStarted1()
        {
            base.OnGameStarted1();
            RefreshState();
        }
        private void RefreshState()
        {
            ExpireLoan.Clear();
            foreach (var item in LoanDatas)
            {
                if (IsLoanExpire(item))
                {
                    ExpireLoan.Add(item);
                }
            }
        }
        #endregion

        #region Manual Update
        protected void ManualUpdateLoan()
        {
            //更新贷款数据
            foreach (var item in LoanDatas)
            {
                item.Update(1);
            }
            RefreshState();
        }
        #endregion

        #region is can
        public virtual bool IsCanAddLoan()
        {
            ACM.Reset(SelfBaseUnit);
            ACM.Add(new ExCondition(!IsInMaxLoan(), "AC_超出贷款上限", UIUtil.FractionCol(CurLoan, MaxLoan)));
            return ACM.IsTrue();
        }
        public virtual bool IsCanPayLoan(LoanData data)
        {
            ACM.Reset(SelfBaseUnit);
            ACM.Add(new ExCondition(IsGoldEnough(data), "AC_金币不够", data.Gold));
            return ACM.IsTrue();
        }
        public virtual bool IsCanPayLoan()
        {
            ACM.Reset(SelfBaseUnit);
            ACM.Add(new ExCondition(IsHaveLoan(), "AC_没有债务"));
            return ACM.IsTrue();
        }
        public virtual bool IsCanRemoveLoan(LoanData data)
        {
            ACM.Reset(SelfBaseUnit);
            ACM.Add(new ExCondition(IsLoanExpire(data), "AC_债务没有到期"));
            return ACM.IsTrue();
        }
        #endregion

        #region is
        public bool IsHaveLoan() => LoanDatas.Count > 0;
        public bool IsInMaxLoan() => CurLoan >= MaxLoan;
        public bool IsGoldEnough(LoanData data) => CurGold >= data.Gold;
        public bool IsLoanExpire(LoanData data) => data.IsOver();
        public bool IsHaveExpireLoan() => ExpireLoan.Count > 0;
        #endregion

        #region get
        public float CalcTotalInterst()
        {
            var ret = 0.0f;
            foreach (var item in LoanDatas)
            {
                ret += item.GetInterestGold();
            }
            return ret;
        }
        #endregion

        #region set
        /// <summary>
        /// 向银行贷款
        /// </summary>
        public virtual LoanData AddLoan()
        {
            if (!IsCanAddLoan()) return null;
            LoanData tempData = new LoanData();
            tempData.Gold = OnceLoanGold;
            tempData.Interest = Interest;
            tempData.Date = DateTimeMgr.GetCurYearMonth();
            tempData.SetCurCount(LoanPeriod);
            tempData.SetTotalCount(LoanPeriod);
            CurLoan += tempData.Gold;
            LoanDatas.Add(tempData);
            OnAddLoan(tempData);
            ChangeGold(tempData.Gold);
            return tempData;
        }
        /// <summary>
        /// 偿还贷款
        /// </summary>
        /// <param name="index"></param>
        public virtual bool PayLoan(LoanData data)
        {
            if (!IsCanPayLoan(data)) return false;
            CurLoan -= data.Gold;
            CurLoan = MathUtil.Clamp0(CurLoan);
            LoanDatas.Remove(data);
            OnPayLoan(data);
            ChangeGold(-data.Gold);
            return true;
        }
        public virtual bool RemoveLoan(LoanData data)
        {
            LoanDatas.Remove(data);
            ExpireLoan.Remove(data);
            CurLoan -= data.Gold;
            CurLoan = MathUtil.Clamp0(CurLoan);
            OnRemoveLoan(data);
            return true;
        }
        #endregion

        #region Callback
        protected virtual void OnAddLoan(LoanData data) => Callback_OnLoanChanged?.Invoke();
        protected virtual void OnPayLoan(LoanData data) => Callback_OnLoanChanged?.Invoke();
        protected virtual void OnRemoveLoan(LoanData data) => Callback_OnLoanChanged?.Invoke();
        #endregion

        #region DB
        public void LoadDBData(ref DBBaseLoan data)
        {
            CurLoan = data.CurLoan;
            LoanDatas = data.Loan;
        }

        public void SaveDBData(ref DBBaseLoan ret)
        {
            //DBBaseLoan ret = new DBBaseLoan();
            ret.CurLoan = CurLoan;
            ret.Loan = LoanDatas;
            //return ret;
        }
        #endregion
    }
}