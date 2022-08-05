//------------------------------------------------------------------------------
// BaseRegimeMgr.cs
// Created by CYM on 2022/6/17
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
namespace CYM
{
    public class BaseRegimeMgr<TData,TOfficialData,TUnit> : BaseUSwitchTDMgr<TData, TUnit>, IDBConverMgr<DBBaseRegime>
        where TData:TDBaseRegimeData,new()
        where TOfficialData:TDBaseOfficialData,new()
        where TUnit:BaseUnit
    {

        #region prop
        IUStaffMgr StaffMgr => SelfBaseUnit.StaffMgr;
        #endregion

        #region Run time
        public DulDic<int, TOfficialData> OfficialPos { get; private set; } = new DulDic<int, TOfficialData>();
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedTurnbase = true;
        }
        public override void OnTurnbase(bool day, bool month, bool year)
        {
            base.OnTurnbase(day, month, year);
            if (CurData == null)
                return;
            if (month)
            {
                //每月50%的概率产生新的君主
                if (RandUtil.Rand(0.5f))
                {
                    if (!StaffMgr.IsHaveMonarch())
                    {
                        CurData.Throne();
                    }
                }
                //每月1/10的概率产生新的继承人
                if (RandUtil.Rand(0.12f))
                {
                    if (StaffMgr.CrownPrinceCount < CurData.MaxPrince())
                    {
                        CurData.GeneratePrince();
                    }
                }
                //每月50%的概率产生新的美人
                if (RandUtil.Rand(0.5f))
                {
                    if (StaffMgr.HaremCount < CurData.MaxBeauty())
                    {
                        CurData.GenerateBeauty();
                    }
                }
                //每月50%的概率产生新的候选人
                if (RandUtil.Rand(0.5f))
                {
                    if (StaffMgr.CandidateCount < CurData.MaxCandidate())
                    {
                        CurData.GenerateCandidate();
                    }
                }
            }
            if (year)
            {
                if (StaffMgr.IsHaveMonarch())
                {
                    //增加任期计数
                    CurData.CurTerm++;
                    CurData.CurTerm = Mathf.Clamp(CurData.CurTerm, 0, int.MaxValue);
                    //任期结束
                    if (CurData.CurTerm > CurData.MaxTerm)
                    {
                        CurData.ResetTerm();
                        
                    }
                }
            }
        }
        #endregion

        #region set
        //刷新职位
        public void RefreshOfficial()
        {
            OfficialPos.Clear();
            int index = 1;
            foreach (var item in CurData.Officials)
            {
                var data = BaseGlobal.TDOfficial.Get<TOfficialData>(item);
                OfficialPos.Add(index, data);
                index++;
            }
        }
        //添加官职buff
        public void AddOfficialBuff(int index,int inputVal)
        {
            var data = GetOfficial(index);
            if (data != null)
            {
                var buffId = data.GetBuff();
                var buff = SelfUnit.BuffMgr.AddBase(buffId);
                buff.SetInput(inputVal);
            }
        }
        //移除所有职位的buff
        public void RemoveAllOfficialBuff()
        {
            //移除所有官职的buff
            foreach (var item in OfficialPos)
            {
                SelfUnit.BuffMgr.Remove(item.Value.Buff);
            }
        }
        //一移除自定官位的buff
        public void RemoveOfficialBuff(int index)
        {
            var data = GetOfficial(index);
            if (data != null)
            {
                SelfUnit.BuffMgr.Remove(data.Buff);
            }
        }
        #endregion

        #region get
        //获得指定的官位
        public TOfficialData GetOfficial(int index)
        {
            if (OfficialPos.ContainsKey(index))
                return OfficialPos[index];
            return default;
        }
        #endregion

        #region DB
        public void LoadDBData(ref DBBaseRegime data)
        {
            LoadDBData(ref data,(config,data)=> {
                config.CurTerm = data.CurTerm;
            });
        }
        public void SaveDBData(ref DBBaseRegime data) 
        {
            SaveDBData(ref data, (config, data) =>
            {
                data.CurTerm = config.CurTerm;
            });
        }
        #endregion
    }
}