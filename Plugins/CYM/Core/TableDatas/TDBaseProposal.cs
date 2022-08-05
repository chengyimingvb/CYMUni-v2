//------------------------------------------------------------------------------
// TDBaseProposalData.cs
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
    public class TDBaseProposalData : TDBaseData
    {
        #region config
        //触发条件
        public BaseTarget Condition1 { get; set; }
        public BaseTarget Condition2 { get; set; }
        public BaseTarget Condition3 { get; set; }
        public BaseTarget Condition4 { get; set; }
        //完成条件
        public BaseTarget Target1 { get; set; }
        public BaseTarget Target2 { get; set; }
        public BaseTarget Target3 { get; set; }
        public BaseTarget Target4 { get; set; }
        //奖励
        public BaseReward Reward1 { get; set; }
        public BaseReward Reward2 { get; set; }
        public BaseReward Reward3 { get; set; }
        // 任务冷却CD，防止重复刷出任务的冷却时间
        public int CD { get; set; } = 10;
        // 出现条件，任务的触发概率
        public float Prob { get; set; } = 0.4f;
        // 最大过期时间
        public int MaxExpireCD { get; set; } = 25;
        #endregion

        #region Runtime
        public List<BaseTarget> Conditions { get; private set; } = new List<BaseTarget>();
        public List<BaseReward> Rewards { get; private set; } = new List<BaseReward>();
        public List<BaseTarget> Targets { get; private set; } = new List<BaseTarget>();
        public long Person { get; set; } = Const.LONG_Inv;
        public int CurExpireCD { get; set; } //当前任务过期的CD时间
        public bool IsAccept { get; set; } = false;
        #endregion

        #region life
        public override void OnBeAdded(BaseCoreMono selfMono, params object[] obj)
        {
            base.OnBeAdded(selfMono, obj);
            CurExpireCD = 0;
            IsAccept = false;
            foreach (var item in Rewards)
            {
                item.SetTarget(SelfBaseUnit);
            }
        }
        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            Rewards = new List<BaseReward>();
            if (Reward1 != null) Rewards.Add(Reward1);
            if (Reward2 != null) Rewards.Add(Reward2);
            if (Reward3 != null) Rewards.Add(Reward3);

            Conditions = new List<BaseTarget>();
            if (Condition1 != null) Conditions.Add(Condition1);
            if (Condition2 != null) Conditions.Add(Condition2);
            if (Condition3 != null) Conditions.Add(Condition3);
            if (Condition4 != null) Conditions.Add(Condition4);

            Targets = new List<BaseTarget>();
            if (Target1 != null) Targets.Add(Target1);
            if (Target2 != null) Targets.Add(Target2);
            if (Target3 != null) Targets.Add(Target3);
            if (Target4 != null) Targets.Add(Target4);
        }
        public override void ManualUpdate()
        {
            base.ManualUpdate();
            CurExpireCD++;
        }
        #endregion

        #region is
        public bool IsExpire()
        {
            return CurExpireCD >= MaxExpireCD;
        }
        #endregion

        #region get
        public float GetExpireProgress()
        {
            return (float)CurExpireCD / (float)MaxExpireCD;
        }
        #endregion
    }
}