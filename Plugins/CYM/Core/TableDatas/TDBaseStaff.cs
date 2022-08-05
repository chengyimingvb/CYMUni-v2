//------------------------------------------------------------------------------
// TDBaseStaff.cs
// Created by CYM on 2022/6/9
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;
using System;

namespace CYM
{
    public enum StaffType
    {
        Monarch,      //君主
        Deputy,       //副手
        Queue,        //王后
        Crown,        //储君
        Merchant,     //商人
        Diplomat,     //外交官
        Spy,          //间谍
        Prince,       //继承人
        Harem,        //后宫
        Official,     //庭臣
        General,      //将领
        Prefect,      //太守
        Candidate,    //候选人
        Beauty,       //美人
        Hangeron,     //门客
        Jail,         //监禁
        Captive,      //俘虏
    }
    public enum StaffAttrType
    {
        Adm,
        Inte,
        Fire,
        Shock,
        Impact,
        Tactic,
        Loyalty,
        Power,
    }
    public enum ThroneType
    {
        Crown,
        Prince,
        Usurp,
    }
    public enum StaffDeathType
    {
        Death,
        Retire,
        Despear,
        Execution,
    }
    public class StaffFilterData
    {
        public BaseUnit Target;
        public HashList<StaffType> Include = new HashList<StaffType>();
        public HashList<StaffType> Exclude = new HashList<StaffType>();
        public bool NoChild = true;
        public bool NoFemale = true;
        public Func<StaffFilterData, TDBaseStaffData, bool> IsFilter = (data, x) => true;

        public StaffFilterData SetTargetNation(BaseUnit nation)
        {
            Target = nation;
            return this;
        }
    }
    public class TDBaseStaffData : TDBasePersonData
    {
        #region Const
        public static readonly string Tag_Monach = "Monach";
        public static readonly string Tag_General = "General";
        #endregion

        #region Runtime prop
        public HashList<StaffType> Staffs { get; set; } = new HashList<StaffType>();
        public StaffDeathType StaffDeathType { get; set; } = StaffDeathType.Death;
        #endregion

        #region Config Attr
        public int Adm { get; set; } = Const.INT_Inv;
        public int Inte { get; set; } = Const.INT_Inv;
        public int Fire { get; set; } = Const.INT_Inv;
        public int Shock { get; set; } = Const.INT_Inv;
        public int Impact { get; set; } = Const.INT_Inv;
        public int Tactic { get; set; } = Const.INT_Inv;
        #endregion

        #region Config Attr
        public int Power { get; set; } = Const.INT_Inv;
        public int Loyalty { get; set; } = Const.INT_Inv;
        public string GeneralHelmet { get; set; } = Const.STR_Inv;
        public string MonarchHelmet { get; set; } = Const.STR_Inv;
        #endregion

        #region Runtime Attr
        public virtual float AdmInputVal => RealAdm * 0.1f;
        public virtual float InteInputVal => RealInte * 0.1f;
        public virtual int RealAdm => Adm;
        public virtual int RealInte => Inte;
        public virtual int RealFire => IsFemale ? 0 : Fire;
        public virtual int RealShock => IsFemale ? 0 : Shock;
        public virtual int RealImpact => IsFemale ? 0 : Impact;
        public virtual int RealTactic => IsFemale ? 0 : Tactic;
        public virtual int RealPower => Power;
        public virtual int RealLoyalty => Loyalty;
        public int MiliPt => RealFire + RealShock + RealImpact + RealTactic;
        public int CiviPt => RealAdm + RealInte;
        #endregion

        #region Staff
        //随机职位
        public void RandStaff()
        {
            Staffs.Clear();
            Staffs.Add(typeof(StaffType).Rand<StaffType>());
        }
        //增加职位
        public void AddStaff(StaffType tag)
        {
            Staffs.Add(tag);
        }
        //移除职位
        public void RemoveStaff(StaffType tag)
        {
            Staffs.Remove(tag);
        }
        //获得职位
        public string GetStaff(bool isFisrt = false)
        {
            return GetCustomStaff(Staffs, isFisrt);

            string GetCustomStaff(List<StaffType> staff, bool isFisrt)
            {
                if (staff.Count == 0)
                    return "";
                string ret = "";
                int index = 0;
                foreach (var item in staff)
                {
                    string splite = "";
                    if (index > 0)
                        splite = ",";
                    ret += splite + item.GetName();
                    index++;
                    if (isFisrt)
                        break;
                }
                return ret;
            }
        }
        #endregion

        #region Attr
        public void SetAttr(StaffAttrType type,int val)
        {
            switch (type)
            {
                case StaffAttrType.Adm:
                    Adm = val;
                    break;
                case StaffAttrType.Inte:
                    Inte = val;
                    break;
                case StaffAttrType.Shock:
                    Shock = val;
                    break;
                case StaffAttrType.Fire:
                    Fire = val;
                    break;
                case StaffAttrType.Impact:
                    Impact = val;
                    break;
                case StaffAttrType.Tactic:
                    Tactic = val;
                    break;
                case StaffAttrType.Loyalty:
                    Loyalty = val;
                    break;
                case StaffAttrType.Power:
                    Power = val;
                    break;
            }
        }
        public void ChangeAttr(StaffAttrType type, int val)
        {
            switch (type)
            {
                case StaffAttrType.Adm:
                    Adm += val;
                    break;
                case StaffAttrType.Inte:
                    Inte += val;
                    break;
                case StaffAttrType.Shock:
                    Shock += val;
                    break;
                case StaffAttrType.Fire:
                    Fire += val;
                    break;
                case StaffAttrType.Impact:
                    Impact += val;
                    break;
                case StaffAttrType.Tactic:
                    Tactic += val;
                    break;
                case StaffAttrType.Loyalty:
                    Loyalty += val;
                    break;
                case StaffAttrType.Power:
                    Power += val;
                    break;
            }
        }
        public int GetRealAttr(StaffAttrType type)
        {
            var ret = type switch
            {
               StaffAttrType.Adm => RealAdm,
               StaffAttrType.Inte => RealInte,
               StaffAttrType.Shock => RealShock,
               StaffAttrType.Fire => RealFire,
               StaffAttrType.Impact=>RealImpact,
               StaffAttrType.Tactic=>RealTactic,
               StaffAttrType.Loyalty=>RealLoyalty,
               StaffAttrType.Power=>RealPower,
                _ => 0
            };
            return ret;
        }
        #endregion

        #region Is
        public bool IsMonarch => Staffs.Contains(StaffType.Monarch);
        public bool IsQueue => Staffs.Contains(StaffType.Queue);
        public bool IsDeputy => Staffs.Contains(StaffType.Deputy);
        public bool IsCrown => Staffs.Contains(StaffType.Crown);
        public bool IsMerchant => Staffs.Contains(StaffType.Merchant);
        public bool IsDiplomat => Staffs.Contains(StaffType.Diplomat);
        public bool IsSpy => Staffs.Contains(StaffType.Spy);
        public bool IsPrince => Staffs.Contains(StaffType.Prince);
        public bool IsHarem => Staffs.Contains(StaffType.Harem);
        public bool IsOfficial => Staffs.Contains(StaffType.Official);
        public bool IsGeneral => Staffs.Contains(StaffType.General);
        public bool IsPrefect => Staffs.Contains(StaffType.Prefect);
        public bool IsCandidate => Staffs.Contains(StaffType.Candidate);
        public bool IsBeauty => Staffs.Contains(StaffType.Beauty);
        public bool IsHangeron => Staffs.Contains(StaffType.Hangeron);
        public bool IsJail => Staffs.Contains(StaffType.Jail);
        public bool IsCaptive => Staffs.Contains(StaffType.Captive);
        //长胡子的年纪
        public bool IsOverBearAge => Age >= 21;
        //没有职位
        public bool IsNoStaff => Staffs.Count <= 0;
        //有职位
        public bool IsHaveStaff => Staffs.Count > 0;
        //不忠诚
        public bool IsDisloyalty => RealLoyalty < 50;
        //较低的势力
        public bool IsLowPower => RealPower <= 20;
        //是否为中年以上
        public bool IsOverMidAge => AgeRange == AgeRange.Old && AgeRange == AgeRange.Middle;
        //是否为成年
        public bool IsAdult => AgeRange != AgeRange.Child;
        //是否为成年男子
        public bool IsAdultMan => IsAdult && Gender != Gender.Female;
        //是否可以被雇佣
        public bool IsCanHire => (IsCandidate || IsHangeron) && IsAdultMan;
        //是否为候选人或者是美女
        public bool IsCandidateOrBeauty => IsCandidate || IsBeauty;
        //是否为候选人或者是门客
        public bool IsCandidateOrHangeron => IsCandidate || IsHangeron;
        //核心官员
        public bool IsCoreStaff => IsCivilOfficial || IsGeneral;
        //家族成员
        public bool IsFamily => IsMonarch || IsQueue || IsCrown;
        //统治者
        public bool IsRuler => IsMonarch || IsQueue;
        //核心皇室成员
        public bool IsCoreRoyal => IsFamily || IsPrince;
        //泛皇室成员
        public bool IsGeneRoyal => IsCoreRoyal || IsHarem;
        //文职官员
        public bool IsCivilOfficial => IsOfficial || IsMerchant || IsDiplomat;
        //是否已经被雇佣
        public bool IsHired => IsMonarch || IsCrown || IsPrince || IsQueue || IsGeneral || IsOfficial || IsMerchant || IsHangeron || IsHarem || IsDiplomat;
        //没有地位的
        public bool IsUnStatus => IsJail || IsHangeron || IsCandidate || IsUnStatusFemale;
        //没有地位的女性
        public bool IsUnStatusFemale => IsHarem || IsBeauty;
        public bool IsCanRename
        {
            get
            {
                if (AgeRange == AgeRange.Child && IsPrince || IsCrown)
                    return true;
                return false;
            }
        }
        //是否为合格将领
        public bool IsQualifiedGeneral => MiliPt > 25;
        //优秀将领
        public bool IsExcellentGeneral => MiliPt > 30;
        #endregion

        #region Rand
        //随机能力
        public int RandValue(int min = 1, int max = 100)=> RandUtil.RandInt(min, max);
        public void RandLoyalty()
        {
            Loyalty = RandValue(50,100);
        }
        public void RandPower()
        {
            Power = RandValue(50, 100);
        }
        //随机增长,减少忠诚度
        public void RandChangeLoyalty()
        {
            if (RandUtil.Rand(0.5f))
            {
                Loyalty = Mathf.Clamp(Loyalty + RandUtil.Range(-5, 5), 0, 100);
            }
        }
        //随机增长,减少势力
        public void RandChangePower()
        {
            if (RandUtil.Rand(0.5f))
            {
                if (IsMonarch && Power < 50)
                {
                    Power += 1;
                }
                else
                {
                    Power = Mathf.Clamp(Power + RandUtil.Range(-5, 5), 0, 100);
                }
            }
        }
        #endregion

        #region Life
        public override void ManualUpdate()
        {
            base.ManualUpdate();
            RandChangeLoyalty();
            RandChangePower();
        }
        #endregion

        #region Death Action
        //人物死亡:底层都是死亡逻辑
        public override void DoDeath()
        {
            StaffDeathType = StaffDeathType.Death;
            base.DoDeath();
        }
        //人物隐退:底层都是死亡逻辑
        public virtual void DoRetire()
        {
            StaffDeathType = StaffDeathType.Retire;
            base.DoDeath();
        }
        //人物消失:底层都是死亡逻辑
        public virtual void DoDespear()
        {
            StaffDeathType = StaffDeathType.Despear;
            base.DoDeath();
        }
        //人物处决:底层都是死亡逻辑
        public void DoExecution()
        {
            StaffDeathType = StaffDeathType.Execution;
            base.DoDeath();
        }
        #endregion

        #region Override
        //无中生有,随机创造一个人物的时候
        protected override void OnRandPerson()
        {
            base.OnRandPerson();
        }
        //通过表格数据,生成一个人物的时候
        protected override void OnGenPerson()
        {
            base.OnGenPerson();
        }
        //生成人物后设置人物信息的时候
        protected override void OnSetPersonInfo()
        {
            base.OnSetPersonInfo();
            Adm = RandValue();
            Inte = RandValue();
            Fire = RandValue();
            Shock = RandValue();
            Impact = RandValue();
            Tactic = RandValue();
            Power = RandValue(0, 100);
            Loyalty = RandValue(50, 100);
            GeneralHelmet = GetGenderInfo().GetSHelmet(Tag_General).Rand().Name;
            MonarchHelmet = GetGenderInfo().GetSHelmet(Tag_Monach).Rand().Name;
        }
        protected override string OnProcessIdentity(PHIPart part, string source)
        {
            string ret = source;
            if (AgeRange != AgeRange.Child)
            {
                if (part == PHIPart.PHelmet)
                {
                    //君主身份加工
                    if (IsMonarch && IsMale)
                    {
                        return MonarchHelmet;
                    }
                    //将军身份加工
                    else if (IsExcellentGeneral && IsMale)
                    {
                        return GeneralHelmet;
                    }
                }
            }
            return ret;
        }
        #endregion
    }
}