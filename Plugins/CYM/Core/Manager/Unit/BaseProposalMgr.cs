//------------------------------------------------------------------------------
// BaseProposalMgr.cs
// Created by CYM on 2022/3/4
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;

namespace CYM
{
    public class BaseProposalMgr<TData> : BaseMgr, IDBListConverMgr<DBBaseProposal>
        where TData : TDBaseProposalData, new()
    {
        #region Callback
        public event Callback<TData> Callback_OnAdded;
        public event Callback<TData> Callback_OnRemoved;
        public event Callback<TData> Callback_OnChange;
        #endregion

        #region prop
        BaseConditionMgr ACMgr => BaseGlobal.ACM;
        public IDDicList<TData> Data { get; private set; } = new IDDicList<TData>();
        public Dictionary<string, CD> TaskCD { get; protected set; } = new Dictionary<string, CD>();
        static int MaxRandCount = 4;
        ITDConfig ITDConfig;
        List<string> clearTasks = new List<string>();
        List<TData> clearTaskDatas = new List<TData>();
        protected virtual float GlobalProp => 1.0f;
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        public TData CurData { get; set; }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            ITDConfig = BaseLuaMgr.GetTDConfig(typeof(TData));
        }
        protected virtual void OnAddToData(TData data)
        {
            //自定义设置
        }
        #endregion

        #region Manual Update
        protected void ManualUpdateData()
        {
            Rand();
            clearTasks.Clear();
            foreach (var item in TaskCD)
            {
                item.Value.Update();
                if (item.Value.IsOver())
                    clearTasks.Add(item.Key);
            }
            foreach (var item in clearTasks)
                TaskCD.Remove(item);

            //更新任务时间,删除过期的任务
            clearTaskDatas.Clear();
            foreach (var item in Data)
            {
                item.ManualUpdate();
                if (item.IsExpire())
                {
                    clearTaskDatas.Add(item);
                }
            }
            foreach (var item in clearTaskDatas)
                Data.Remove(item);
        }
        #endregion

        #region get
        public TData Get(int id)
        {
            if (!Data.ContainsID(id))
                return null;
            return Data[id];
        }
        public TData First()
        {
            if (Data.Count > 0)
                return Data[0];
            return null;
        }
        /// <summary>
        /// 获得任务的进度
        /// </summary>
        /// <param name="proposal"></param>
        /// <returns></returns>
        public float GetProgress(TDBaseProposalData proposal)
        {
            return proposal.Targets.GetProgress(SelfBaseUnit);
        }
        /// <summary>
        /// 获得任务过期的进度
        /// </summary>
        /// <param name="proposal"></param>
        /// <returns></returns>
        public float GetExpireProgress(TDBaseProposalData proposal)
        {
            return proposal.GetExpireProgress();
        }
        #endregion

        #region set
        int CurTaskIndex = 0;
        public TData TestNext()
        {
            if (ITDConfig.ListKeys.Count == 0) return null;
            if (CurTaskIndex >= ITDConfig.ListKeys.Count) return null;
            string key = ITDConfig.ListKeys[CurTaskIndex];
            CurTaskIndex++;
            return Add(key);
        }
        //随机一个建议
        public TData Rand(bool isForce = false)
        {
            if (!RandUtil.Rand(GlobalProp) && !isForce)
                return null;
            if (Data.Count >= MaxRandCount)
            {
                return null;
            }
            if (!SelfBaseUnit.IsPlayerCtrl())
                return null;
            if (ITDConfig.ListKeys.Count == 0)
                return null;

            string key = ITDConfig.ListKeys.Rand();
            TData config = ITDConfig.Get<TData>(key);
            //不添加重复的任务
            if (Data.ContainsTDID(key))
                return null;
            //判断事件的触发条件
            if (IsInCondition(config))
            {
                //判断事件的概率
                if (IsInProp(config) || isForce)
                {
                    if (config.CD > 0)
                    {
                        if (!TaskCD.ContainsKey(key))
                            TaskCD.Add(key, new CD());
                        TaskCD[key] = new CD(config.CD);
                    }
                    var ret = Add(config.TDID);
                    return ret;
                }
                else
                {
                    return null;
                }
            }
            return Rand();
        }
        //添加一个建议
        public TData Add(string proposalName)
        {
            if (!ITDConfig.Contains(proposalName)) return null;
            TData tempProposal = ITDConfig.Get<TData>(proposalName).Copy<TData>();
            if (tempProposal == null)
            {
                CLog.Error("未找到 Task errorId=" + proposalName);
                return null;
            }
            tempProposal.ID = IDUtil.Gen();
            tempProposal.OnBeAdded(SelfBaseUnit);
            Data.Add(tempProposal);
            Callback_OnAdded?.Invoke(tempProposal);
            Callback_OnChange?.Invoke(tempProposal);
            OnAddToData(tempProposal);
            return tempProposal;
        }
        //删除一个建议
        public void Remove(TData proposal)
        {
            if (proposal == null) return;
            Data.Remove(proposal);
            Callback_OnRemoved?.Invoke(proposal);
            Callback_OnChange?.Invoke(proposal);
        }
        #endregion

        #region Action
        //接受建议
        public virtual void Accept(TData proposal)
        {
            if (!Data.Contains(proposal))
                return;
            if (proposal.IsAccept)
                return;
            if (!IsCanAccept(proposal))
                return;
            proposal.IsAccept = true;
            Announce(proposal);
        }
        //拒绝建议
        public virtual void Refuse(TData proposal)
        {
            if (!Data.Contains(proposal))
                return;
            if (proposal.IsAccept)
                return;
            if (!IsCanRefuse(proposal))
                return;
            Remove(proposal);
        }
        //宣布完成
        public virtual void Announce(TData proposal)
        {
            if (!Data.Contains(proposal))
                return;
            //只有接受的建议才可以宣布完成
            if (!proposal.IsAccept)
                return;
            if (!IsCanAnnounce(proposal))
                return;
            SelfBaseUnit?.AttrMgr?.DoReward(proposal.Rewards);
            Remove(proposal);
        }
        //宣布放弃
        public virtual void Renounce(TData proposal)
        {
            if (!Data.Contains(proposal))
                return;
            //只有接受的建议才可以宣布放弃
            if (!proposal.IsAccept)
                return;
            if (!IsCanRenounce(proposal))
                return;
            Remove(proposal);
        }
        #endregion

        #region is can
        //接受建议
        public virtual bool IsCanAccept(TData proposal) => true;
        //拒绝建议
        public virtual bool IsCanRefuse(TData proposal) => true;
        //宣布完成
        public virtual bool IsCanAnnounce(TData proposal)
        {
            if (proposal.Targets == null)
                return false;
            ACMgr.Reset(SelfBaseUnit);
            ACMgr.Add(proposal.Targets);
            if (!ACMgr.IsTrue())
            {
                return false;
            }
            return true;
        }
        //宣布放弃
        public virtual bool IsCanRenounce(TData proposal) => true;
        #endregion

        #region is
        public bool IsHave() => Data.Count > 0;
        // 是否可以触发
        bool IsInProp(TDBaseProposalData proposal)
        {
            if (Options.Ins.IsMustProposal) return true;
            if (RandUtil.Rand(proposal.Prob)) return true;
            return false;
        }
        //是否达成触发条件
        bool IsInCondition(TDBaseProposalData proposal)
        {
            if (proposal.Conditions == null)
                return false;
            if (TaskCD.ContainsKey(proposal.TDID))
            {
                if (!TaskCD[proposal.TDID].IsOver())
                    return false;
            }

            ACMgr.Reset(SelfBaseUnit);
            ACMgr.Add(proposal.Conditions);
            if (!ACMgr.IsTrue())
            {
                return false;
            }
            return true;
        }
        #endregion

        #region DB
        public void SaveDBData(ref List<DBBaseProposal> ret)
        {
            ret = new List<DBBaseProposal>();
            foreach (var item in Data)
            {
                DBBaseProposal temp = new DBBaseProposal();
                temp.ID = item.ID;
                temp.TDID = item.TDID;
                temp.CD = item.CD;
                temp.Person = item.Person;
                temp.CurExpireCD = item.CurExpireCD;
                temp.IsAccept = item.IsAccept;
                ret.Add(temp);
            }
        }
        public void LoadDBData(ref List<DBBaseProposal> data)
        {
            if (data == null) return;
            foreach (var item in data)
            {
                var temp = Add(item.TDID);
                temp.TDID = item.TDID;
                temp.ID = item.ID;
                temp.CD = item.CD;
                temp.Person = item.Person;
                temp.CurExpireCD = item.CurExpireCD;
                temp.IsAccept = item.IsAccept;
            }
        }
        public void SaveCDDBData(ref Dictionary<string, CD> ret)
        {
            ret = TaskCD;
        }
        public void LoadCDDBData(ref Dictionary<string, CD> cd)
        {
            TaskCD = cd;
        }
        #endregion
    }
}