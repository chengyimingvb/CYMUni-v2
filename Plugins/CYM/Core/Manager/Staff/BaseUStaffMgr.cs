//------------------------------------------------------------------------------
// BaseOfficialMgr.cs
// Created by CYM on 2022/5/27
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System;
using System.Collections.Generic;
using System.Linq;
/// <summary>
/// 模拟古代官职的管理器,UnitMgr
/// </summary>
namespace CYM
{
    public class BaseUStaffMgr<TData, TUnit, TLegion, TCastle> : BaseTDMgr<TData, TUnit>, IUStaffMgr
        where TData : TDBaseStaffData, new()
        where TUnit : BaseUnit
        where TLegion : BaseUnit
        where TCastle : BaseUnit
    {
        #region Callback
        public event Callback<bool, TData> Callback_OnMonarch;
        public event Callback<bool, TData> Callback_OnQueen;
        public event Callback<bool, TData> Callback_OnDeputy;
        public event Callback<bool, TData> Callback_OnCrown;
        public event Callback<bool, TData> Callback_OnDiplomat;
        public event Callback<bool, TData> Callback_OnSpy;
        public event Callback<bool, TData> Callback_OnPrince;
        public event Callback<bool, TData> Callback_OnHarem;
        public event Callback<bool, int, TData> Callback_OnOfficial;
        public event Callback<bool, TLegion, TData> Callback_OnGeneral;
        public event Callback<bool, TCastle, TData> Callback_OnPrefect;
        public event Callback<bool, TData> Callback_OnCandidate;
        public event Callback<bool, TData> Callback_OnBeauty;
        public event Callback<bool, TData> Callback_OnHangeron;
        public event Callback<bool, TData> Callback_OnJailer;
        public event Callback<bool, string, TData> Callback_OnMerchant;
        public event Callback<bool, TData> Callback_OnCaptive;
        public event Callback<TData> Callback_OnStaffChange;
        #endregion

        #region Life
        public override void OnClear()
        {
            base.OnClear();
            Monarch = null;
            Queen = null;
            Deputy = null;
            Crown = null;
            Diplomat = null;
            Spy = null;
            Prince.Clear();
            Harem.Clear();
            Official.Clear();
            General.Clear();
            Prefect.Clear();
            Candidate.Clear();
            Beauty.Clear();
            Hangeron.Clear();
            Jailer.Clear();
            Captive.Clear();
            Merchant.Clear();
        }
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedTurnbase = true;
        }
        public override void OnTurnbase(bool day, bool month, bool year)
        {
            base.OnTurnbase(day, month, year);
            if (month)
            {
                CurStruggleProp += 0.03f;
            }
        }
        #endregion

        #region Prop
        BaseGStaffMgr<TData> GStaffMgr => GMgr as BaseGStaffMgr<TData>;
        //当前通过斗争产生君主的概率
        public float CurStruggleProp { get; private set; } = 0;
        #endregion

        #region Data
        //君主
        public TData Monarch { get; protected set; } = null;
        //王后
        public TData Queen { get; protected set; } = null;
        //副手
        public TData Deputy { get; protected set; } = null;
        //储君
        public TData Crown { get; protected set; } = null;
        //外交官
        public TData Diplomat { get; protected set; } = null;
        //间谍
        public TData Spy { get; protected set; } = null;
        //继承人
        public HashList<TData> Prince { get; protected set; } = new();
        //后宫
        public HashList<TData> Harem { get; protected set; } = new();
        //庭臣
        public DulDic<int, TData> Official { get; protected set; } = new();
        //将领
        public DulDic<TLegion, TData> General { get; protected set; } = new();
        //太守
        public DulDic<TCastle, TData> Prefect { get; protected set; } = new();
        //候选人
        public HashList<TData> Candidate { get; protected set; } = new();
        //美人
        public HashList<TData> Beauty { get; protected set; } = new();
        //门客
        public HashList<TData> Hangeron { get; protected set; } = new();
        //监狱
        public HashList<TData> Jailer { get; protected set; } = new();
        //俘虏
        public HashList<TData> Captive { get; protected set; } = new();
        //商人
        public DulDic<string, TData> Merchant { get; protected set; } = new DulDic<string, TData>();
        #endregion

        #region Count
        public int PrinceCount => Prince.Count;
        public int HaremCount => Harem.Count;
        public int OfficialCount => Official.Count;
        public int GeneralCount => General.Count;
        public int PrefectCount => Prefect.Count;
        public int CandidateCount => Candidate.Count;
        public int BeautyCount => Beauty.Count;
        public int HangeronCount => Hangeron.Count;
        public int JailerCount => Jailer.Count;
        public int CaptiveCount => Captive.Count;
        public int MerchantCount => Merchant.Count;
        //储君和继承人的数量
        public int CrownPrinceCount => (IsHaveCrown() ? 1 : 0) + Prince.Count;
        //王后,后宫,美人的数量
        public int QueenHaremBeautyCount => (IsHaveQueen() ? 1 : 0) + Harem.Count + Beauty.Count;
        #endregion

        #region Filter
        public bool IsCanFilter(TDBaseStaffData item, StaffFilterData filter)
        {
            if (filter.NoChild && item.AgeRange == AgeRange.Child) return false;
            if (filter.NoFemale && item.Gender == Gender.Female) return false;
            if (filter.Exclude.Contains(item.Staffs)) return false;
            if (!filter.Include.Contains(item.Staffs)) return false;
            if (!filter.IsFilter(filter, item)) return false;
            return true;
        }
        public List<T> GetFilter<T>(StaffFilterData filter) where T : class
        {
            List<T> ret = new List<T>();
            if (filter == null) return ret;
            foreach (var item in Data)
            {
                if (!IsCanFilter(item, filter)) continue;
                ret.Add(item as T);
            }
            return ret;
        }
        public List<TData> GetCanGenerals() => GetFilter<TData>(FilterGeneral);
        public List<TData> GetFilter(StaffFilterData filter) => GetFilter<TData>(filter);
        #endregion

        #region Set
        protected void DoStaffChange(TData data)
        {
            Callback_OnStaffChange?.Invoke(data);
        }
        public override void Despawn(TDBaseData data)
        {
            base.Despawn(data);
            TData staff = data as TData;
            RemoveAllStaff(staff);
        }
        //登基
        public bool Throne(TData person)
        {
            if (person == null) return false;
            ThroneType throneType = ThroneType.Crown;
            //原来的君主下台,进监狱
            if (IsHaveMonarch())
            {
                RemoveMonach(Monarch);
                AddJail(Monarch);
            }

            //储君
            if (person.IsCrown)
            {
                RemoveCrown(person);
                AddMonach(person);
                throneType = ThroneType.Crown;
            }
            //继承人上位
            else if (person.IsPrince)
            {
                RemovePrince(person);
                AddMonach(person);
                throneType = ThroneType.Prince;
            }
            //其他人篡位
            else
            {
                RemoveAllStaff(person);
                AddMonach(person);
                throneType = ThroneType.Usurp;
            }
            OnThrone(throneType, person);

            //清除前朝人物
            List<TData> clearData = new List<TData>();
            clearData.AddRange(Prince);
            clearData.AddRange(Harem);
            if (Queen != null) clearData.Add(Queen);
            if (Crown != null) clearData.Add(Crown);
            foreach (var item in clearData)
            {

                if (throneType == ThroneType.Usurp)
                {
                    //篡位者上台后除了后宫所有人都杀光
                    if (!item.IsHarem)
                        item.DoRetire();
                }
                else
                {
                    //小孩和女人直接人道毁灭,废除王子地位
                    if (item.IsWomenOrChildren) 
                        item.DoRetire();
                }
            }

            DoStaffChange(person);
            return true;
        }
        //君主退位
        public void Abdication()
        {
            var person = Monarch;
            if (IsHaveMonarch())
            {
                Monarch.DoRetire();
            }
            DoStaffChange(person);
        }
        //王位斗争
        public void RandStruggle()
        {
            if (!RandUtil.Rand(CurStruggleProp))
                return;
            if (IsHavePrince() && RandUtil.Rand(0.5f))
            {
                var temp = Prince.Rand();
                Throne(temp);
            }
            else if (IsHaveOfficial() && RandUtil.Rand(0.5f))
            {
                var temp = Official.Values.ToList().Rand();
                Throne(temp);
            }
            else if (IsHaveGeneral() && RandUtil.Rand(0.5f))
            {
                var temp = General.First().Value;
                Throne(temp);
            }
            else
            {
                if (IsHavePrince() || IsHaveOfficial() || IsHaveGeneral())
                {
                    RandStruggle();
                }
                else
                {
                    var temp = RandCandiate();
                    Throne(temp);
                }
            }
            CurStruggleProp = 0;
        }
        #endregion

        #region Get
        //从候选人和门客中随便挑一个
        public List<TData> FetchFromCandidateHangeron(Func<TData, int> keySelector)
        {
            List<TData> person = new List<TData>();
            person.AddRange(Candidate);
            person.AddRange(Hangeron);
            person = person.OrderByDescending(keySelector).ToList();
            return person;
        }
        //选取一个可以成为将军的人
        public List<TData> FetchCanBeGeneral(Func<TData, int> keySelector)
        {
            List<TData> person = new List<TData>();
            person.AddRange(Candidate);
            person.AddRange(Hangeron);
            person.AddRange(Prince);
            if (person.Count <= 0 && Monarch != null && !Monarch.IsGeneral) person.Add(Monarch);
            person = person.OrderByDescending(keySelector).TakeWhile(x => IsCanFilter(x, FilterGeneral)).ToList();
            return person;
        }
        public List<TData> FetchFromPrince(Func<TData, int> keySelector)
        {
            List<TData> person = new List<TData>();
            person.AddRange(Prince);
            person = person.OrderByDescending(keySelector).ToList();
            return person;
        }
        public List<TData> FetchFromHarem(Func<TData, int> keySelector)
        {
            List<TData> person = new List<TData>();
            person.AddRange(Harem);
            person = person.OrderByDescending(keySelector).ToList();
            return person;
        }
        public List<TData> FetchFromBeauty()
        {
            List<TData> person = new List<TData>();
            person.AddRange(Beauty);
            return person;
        }
        #endregion

        #region Is
        public bool IsHaveCaptive() => Captive.Count > 0;
        public bool IsMaxPrinces() => CrownPrinceCount >= 5;
        public bool IsHaveMonarch() => Monarch != null;
        public bool IsHaveCrown() => Crown != null;
        public bool IsHaveQueen() => Queen != null;
        public bool IsHaveDeputy() => Deputy != null;
        public bool IsHaveDiplomat() => Diplomat != null;
        public bool IsHaveHarem() => Harem.Count > 0;
        public bool IsHaveBeauty() => Beauty.Count > 0;
        public bool IsHaveCandidate() => Candidate.Count > 0;
        public bool IsHaveHangeron() => Hangeron.Count > 0;
        public bool IsHaveCOH() => IsHaveCandidate() || IsHaveHangeron();
        public bool IsHaveCOHP() => IsHaveCandidate() || IsHaveHangeron() || IsHavePrince();
        public bool IsHaveOfficial() => Official.Count > 0;
        public bool IsFullOfficial() => Official.Count >= 5;
        public bool IsFullMerchant() => Merchant.Count >= 5;
        public bool IsFullGeneral() => General.Count >= 5;
        public bool IsHavePrince() => Prince.Count > 0;
        public bool IsHaveMerchant() => Merchant.Count > 0;
        public bool IsHaveMerchant(string tradeID) => Merchant.ContainsKey(tradeID);
        public bool IsHaveTrade(TData person) => Merchant.ContainsValue(person);
        public bool IsHaveOffical(TData person) => Official.ContainsValue(person);
        public bool IsHaveOffical(int officialID) => Official.ContainsKey(officialID);
        public bool IsHaveOffical() => Official.Count > 0;
        public bool IsHaveGeneral() => General.Count > 0;
        public bool IsHavePrisoner() => Jailer.Count > 0;
        public bool IsSelf(TData data) => Data.Contains(data);
        public bool IsHaveGeneral(TLegion legion)
        {
            if (legion == null) return false;
            if (General.ContainsKey(legion))
                return true;
            return false;
        }
        public bool IsOverPower(TData person)
        {
            if (person == null) return true;
            if (!person.IsGeneral) return true;
            if (!IsHaveMonarch()) return false;
            if (Monarch == person) return true;
            return Monarch.RealPower >= person.RealPower;
        }
        #endregion

        #region Try add
        protected virtual bool AddCandiate(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.Candidate);
            Candidate.Add(person);
            return true;
        }
        protected virtual bool AddMonach(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            if (IsHaveMonarch())
            {
                CLog.Error("AddMonach:错误");
                return false;
            }
            person.AddStaff(StaffType.Monarch);
            Monarch = person;
            return true;
        }
        protected virtual bool AddDeputy(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            if (IsHaveDeputy())
            {
                CLog.Error("AddDeputy:错误");
                return false;
            }
            person.AddStaff(StaffType.Deputy);
            Deputy = person;
            return true;
        }
        protected virtual bool AddCrown(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            if (IsHaveCrown())
            {
                CLog.Error("AddCrown:错误");
                return false;
            }
            person.AddStaff(StaffType.Crown);
            Crown = person;
            return true;
        }
        protected virtual bool AddQueen(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            if (IsHaveQueen())
            {
                CLog.Error("AddQueen:错误");
                return false;
            }
            person.AddStaff(StaffType.Queue);
            Queen = person;
            return true;
        }
        protected virtual bool AddPrince(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.Prince);
            Prince.Add(person);
            return true;
        }
        protected virtual bool AddOfficial(int official, TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            if (IsHaveOffical(official))
            {
                CLog.Error("AddOfficial:错误");
                return false;
            }
            person.AddStaff(StaffType.Official);
            Official.Add(official, person);
            return true;
        }
        protected virtual bool AddJail(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.Jail);
            Jailer.Add(person);
            return true;
        }
        protected virtual bool AddCaptive(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.Captive);
            Captive.Add(person);
            return true;
        }
        protected virtual bool AddHangeron(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            if (person.IsHaveStaff)
                return false;
            person.AddStaff(StaffType.Hangeron);
            Hangeron.Add(person);
            return true;
        }
        protected virtual bool AddMerchant(string tradeID, TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.Merchant);
            Merchant.Add(tradeID, person);
            return true;
        }
        protected virtual bool AddDiplomat(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.Diplomat);
            Diplomat = person;
            return true;
        }
        protected virtual bool AddSpy(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.Spy);
            Spy = person;
            return true;
        }
        protected virtual bool AddGeneral(TLegion legion, TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.General);
            General.Add(legion, person);
            return true;
        }
        protected virtual bool AddPrefect(TCastle castle, TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.Prefect);
            Prefect.Add(castle, person);
            return true;
        }
        protected virtual bool AddBeauty(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.Beauty);
            Beauty.Add(person);
            return true;
        }
        protected virtual bool AddHarem(TData person)
        {
            if (person == null || !person.IsLive)
                return false;
            person.AddStaff(StaffType.Harem);
            Harem.Add(person);
            return true;
        }

        #endregion

        #region Try remove
        protected virtual bool RemovePrince(TData person)
        {
            if (person == null)
                return false;
            person.RemoveStaff(StaffType.Prince);
            Prince.Remove(person);
            return true;
        }
        protected virtual bool RemoveCrown(TData person)
        {
            if (Crown != person ||
                person == null)
                return false;
            Crown.RemoveStaff(StaffType.Crown);
            Crown = null;
            return true;
        }
        protected virtual bool RemoveMonach(TData person)
        {
            if (Monarch != person ||
                person == null)
                return false;
            Monarch.RemoveStaff(StaffType.Monarch);
            Monarch = null;
            return true;
        }
        protected virtual bool RemoveDeputy(TData person)
        {
            if (Deputy != person ||
                person == null)
                return false;
            Deputy.RemoveStaff(StaffType.Deputy);
            Deputy = null;
            return true;
        }
        protected virtual bool RemoveQueen(TData person)
        {
            if (Queen != person ||
                person == null)
                return false;
            Queen.RemoveStaff(StaffType.Queue);
            Queen = null;
            return true;
        }
        protected virtual bool RemoveOfficial(TData person)
        {
            if (person == null)
                return false;
            person.RemoveStaff(StaffType.Official);
            Official.Remove(person);
            return true;
        }
        protected virtual bool RemoveMerchant(TData person)
        {
            if (person == null)
                return false;
            person.RemoveStaff(StaffType.Merchant);
            Merchant.Remove(person);
            return true;
        }
        protected virtual bool RemoveDiplomat(TData person)
        {
            if (Diplomat == person &&
                person!=null)
                return false;
            Diplomat.RemoveStaff(StaffType.Diplomat);
            Diplomat = null;
            return true;
        }
        protected virtual bool RemoveSpy(TData person)
        {
            if (Spy == person &&
                person != null)
                return false;
            Spy.RemoveStaff(StaffType.Spy);
            Spy = null;
            return true;
        }
        protected virtual bool RemoveGeneral(TData person)
        {
            if (person == null)
                return false;
            person.RemoveStaff(StaffType.General);
            General.Remove(person);
            return true;
        }
        protected virtual bool RemovePrefect(TData person)
        {
            if (person == null)
                return false;
            person.RemoveStaff(StaffType.Prefect);
            Prefect.Remove(person);
            return true;
        }
        protected virtual bool RemoveHarem(TData person)
        {
            if (person == null)
                return false;
            person.RemoveStaff(StaffType.Harem);
            Harem.Remove(person);
            return true;
        }
        protected virtual bool RemoveBeauty(TData person)
        {
            if (person == null)
                return false;
            person.Staffs.Remove(StaffType.Beauty);
            Beauty.Remove(person);
            return true;
        }
        protected virtual bool RemoveJail(TData person)
        {
            if (person == null)
                return false;
            person.RemoveStaff(StaffType.Jail);
            Jailer.Remove(person);
            return true;
        }
        protected virtual bool RemoveCaptive(TData person)
        {
            if (person == null)
                return false;
            person.RemoveStaff(StaffType.Captive);
            Captive.Remove(person);
            return true;
        }
        protected virtual bool RemoveHangeron(TData person)
        {
            if (person == null)
                return false;
            person.Staffs.Remove(StaffType.Hangeron);
            Hangeron.Remove(person);
            return true;
        }
        protected virtual bool RemoveCandidate(TData person)
        {
            if (person == null)
                return false;
            person.Staffs.Remove(StaffType.Candidate);
            Candidate.Remove(person);
            return true;
        }
        #endregion

        #region Compon try remove
        //移除闲职,候选人,门客,美人
        protected void RemoveIdleDuty(TData person)
        {
            if (person == null)
                return;
            RemoveCandidate(person);
            RemoveHangeron(person);
            RemoveBeauty(person);
        }
        //移除非核心职位,国君,王后,储君,继承人不会移除
        protected void RemoveNoCoreStaff(TData person)
        {
            RemoveCandidate(person);
            RemoveHangeron(person);
            RemoveDeputy(person);
            RemoveBeauty(person);
            RemoveHarem(person);
            RemoveJail(person);
            RemoveCaptive(person);
            RemoveGeneral(person);
            RemovePrefect(person);
            RemoveDiplomat(person);
            RemoveSpy(person);
            RemoveMerchant(person);
            RemoveOfficial(person);
        }
        protected void RemoveAllStaff(TData person)
        {
            RemovePrince(person);
            RemoveCrown(person);
            RemoveMonach(person);
            RemoveDeputy(person);
            RemoveQueen(person);
            RemoveOfficial(person);
            RemoveMerchant(person);
            RemoveDiplomat(person);
            RemoveSpy(person);
            RemoveGeneral(person);
            RemovePrefect(person);
            RemoveHarem(person);
            RemoveBeauty(person);
            RemoveJail(person);
            RemoveCaptive(person);
            RemoveHangeron(person);
            RemoveCandidate(person);
        }
        #endregion

        #region Filter data
        public static StaffFilterData FilterNormal { get; private set; } = new StaffFilterData
        {
            Include = new HashList<StaffType> { StaffType.Candidate, StaffType.Hangeron },
            NoChild = true,
            NoFemale = true,
        };
        public static StaffFilterData FilterGeneral { get; private set; } = new StaffFilterData
        {
            Include = new HashList<StaffType> { StaffType.Candidate, StaffType.Hangeron, StaffType.Crown, StaffType.Prince, StaffType.Monarch },
            Exclude = new HashList<StaffType> { StaffType.General },
            NoChild = true,
            NoFemale = true,
        };
        public static StaffFilterData FilterPrince { get; private set; } = new StaffFilterData
        {
            Include = new HashList<StaffType> { StaffType.Prince },
            NoChild = false,
            NoFemale = false,
        };
        public static StaffFilterData FilterCaptive { get; private set; } = new StaffFilterData
        {
            NoChild = false,
            NoFemale = false,
            IsFilter = (data, person) =>
            {
                if (person.IsJail)
                    return true;
                return false;
            }
        };
        #endregion

        #region Is Action
        public virtual bool IsCanHireQueen(TData person)=> true;
        public virtual bool IsCanFireQueen()=> true;
        public virtual bool IsCanHireDeputy(TData person)=> true;
        public virtual bool IsCanFireDeputy()=> true;
        public virtual bool IsCanHireCrown(TData person)=> true;
        public virtual bool IsCanFireCrown()=> true;
        public virtual bool IsCanHireDiplomat(TData person)=> true;
        public virtual bool IsCanFireDiplomat()=> true;
        public virtual bool IsCanHireSpy(TData person)=> true;
        public virtual bool IsCanFireSpy()=> true;
        public virtual bool IsCanHireHarem(TData person)=> true;
        public virtual bool IsCanFireHarem(TData person)=> true;
        public virtual bool IsCanHireGeneral(TLegion legion, TData person)=> true;
        public virtual bool IsCanFireGeneral(TData person)=> true;
        public virtual bool IsCanHirePrefect(TCastle castle, TData person)=> true;
        public virtual bool IsCanFirePrefect(TData person)=> true;
        public virtual bool IsCanHireOfficial(int officialID, TData person)=> true;
        public virtual bool IsCanFireOfficial(TData person)=> true;
        public virtual bool IsCanHireMerchant(string tradeID, TData person)=> true;
        public virtual bool IsCanFireMerchant(TData person)=> true;
        public virtual bool IsCanHireJail(TData person)=> true;
        public virtual bool IsCanFireJail(TData person)=> true;
        public virtual bool IsCanHireCaptive(TData person)=> true;
        public virtual bool IsCanFireCaptive(TData person)=> true;
        #endregion

        #region Staff Action
        //立王后
        public virtual bool HireQueen(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanHireQueen(person))
                return false;
            RemoveBeauty(person);
            RemoveHarem(person);
            AddQueen(person);
            Callback_OnQueen(true,person);
            DoStaffChange(person);
            return true;
        }
        //废王后
        public virtual bool FireQueen()
        {
            if (Queen == null)
                return false;
            if (!IsCanFireQueen())
                return false;

            var temp = Queen;
            RemoveQueen(Queen);
            AddHarem(temp);
            Callback_OnQueen(false, temp);
            DoStaffChange(temp);
            return true;
        }
        //立副手
        public virtual bool HireDeputy(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanHireDeputy(person))
                return false;
            RemoveCandidate(person);
            RemoveHangeron(person);
            RemoveBeauty(person);
            RemoveHarem(person);
            AddDeputy(person);
            Callback_OnDeputy(true,person);
            DoStaffChange(person);
            return true;
        }
        //废副手
        public virtual bool FireDeputy()
        {
            if (Deputy == null)
                return false;
            if (!IsCanFireDeputy())
                return false;

            var temp = Deputy;
            RemoveDeputy(Deputy);
            AddHangeron(temp);
            Callback_OnDeputy(false,temp);
            DoStaffChange(temp);
            return true;
        }

        //立储
        public virtual bool HireCrown(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanHireCrown(person))
                return false;
            RemovePrince(person);
            AddCrown(person);
            Callback_OnCrown(true,person);
            DoStaffChange(person);
            return true;
        }
        //废储
        public virtual bool FireCrown()
        {
            if (Crown == null)
                return false;
            if (!IsCanFireCrown())
                return false;

            var crown = Crown;
            RemoveCrown(Crown);
            AddPrince(crown);
            Callback_OnCrown(false, crown);
            DoStaffChange(crown);
            return true;
        }
        //雇佣外交官
        public virtual bool HireDiplomat(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanHireDiplomat(person))
                return false;

            RemoveIdleDuty(person);
            AddDiplomat(person);
            Callback_OnDiplomat(true,person);
            DoStaffChange(person);
            return true;
        }
        //解雇外交关
        public virtual bool FireDiplomat()
        {
            if (Diplomat == null)
                return false;
            if (!IsCanFireDiplomat())
                return false;

            var person = Diplomat;
            RemoveDiplomat(Diplomat);
            AddHangeron(person);
            Callback_OnDiplomat(false, person);
            DoStaffChange(person);
            return true;
        }

        //雇佣间谍
        public virtual bool HireSpy(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanHireSpy(person))
                return false;

            RemoveIdleDuty(person);
            AddSpy(person);
            Callback_OnSpy(true,person);
            DoStaffChange(person);
            return true;
        }
        //解雇间谍
        public virtual bool FireSpy()
        {
            if (Spy == null)
                return false;
            if (!IsCanFireSpy())
                return false;

            var person = Diplomat;
            RemoveSpy(Diplomat);
            AddHangeron(person);
            Callback_OnSpy(false, person);
            DoStaffChange(person);
            return true;
        }
        //纳为后宫
        public virtual bool HireHarem(TData person)
        {
            if (!IsCanHireHarem(person)) return false;
            RemoveBeauty(person);
            AddHarem(person);
            Callback_OnHarem(true,person);
            DoStaffChange(person);
            return true;
        }
        public virtual bool FireHarem(TData person)
        {
            if (!IsCanFireHarem(person)) return false;
            RemoveHarem(person);
            AddBeauty(person);
            Callback_OnHarem(false, person);
            DoStaffChange(person);
            return true;
        }
        //雇佣将军
        public virtual bool HireGeneral(TLegion legion, TData person)
        {
            if (person == null || legion == null)
                return false;
            if (!IsCanHireGeneral(legion, person))
                return false;

            RemoveIdleDuty(person);
            AddGeneral(legion, person);
            Callback_OnGeneral(true,legion, person);
            DoStaffChange(person);
            return true;
        }
        //解雇将领
        public virtual bool FireGeneral(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanFireGeneral(person))
                return false;
            TLegion legion = GetLegion(person);
            RemoveGeneral(person);
            AddHangeron(person);
            Callback_OnGeneral(false,legion, person);
            DoStaffChange(person);
            return true;
        }
        //雇佣太守
        public virtual bool HirePrefect(TCastle castle, TData person)
        {
            if (person == null || castle == null)
                return false;
            if (!IsCanHirePrefect(castle, person))
                return false;

            RemoveIdleDuty(person);
            AddPrefect(castle, person);
            Callback_OnPrefect(true,castle,person);
            DoStaffChange(person);
            return true;
        }
        //解雇太守
        public virtual bool FirePrefect(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanFirePrefect(person))
                return false;
            TCastle castle = GetCastle(person);
            RemovePrefect(person);
            AddHangeron(person);
            Callback_OnPrefect(false,castle, person);
            DoStaffChange(person);
            return true;
        }
        //招募顾问
        public virtual bool HireOfficial(int officialID, TData person)
        {
            if (person == null)
                return false;
            if (!IsCanHireOfficial(officialID, person))
                return false;

            RemoveIdleDuty(person);
            AddOfficial(officialID, person);
            Callback_OnOfficial(true,officialID,person);
            DoStaffChange(person);
            return true;
        }
        //解雇顾问
        public virtual bool FireOfficial(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanFireOfficial(person))
                return false;
            var officialID = GetOfficial(person);
            RemoveOfficial(person);
            AddHangeron(person);
            Callback_OnOfficial(false,officialID, person);
            DoStaffChange(person);
            return true;
        }
        //回收
        public virtual bool RecoveryOfficial()
        {
            foreach (var item in Official)
            {
                RemoveOfficial(item.Value);
                AddHangeron(item.Value);
            }
            Official.Clear();
            return true;
        }
        //招募商人
        public virtual bool HireMerchant(string tradeID, TData person)
        {
            if (person == null)
                return false;
            if (!IsCanHireMerchant(tradeID,person))
                return false;

            RemoveIdleDuty(person);
            AddMerchant(tradeID, person);
            Callback_OnMerchant(true,tradeID,person);
            DoStaffChange(person);
            return true;
        }
        //解雇商人
        public virtual bool FireMerchant(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanFireMerchant(person))
                return false;
            var tradeID = GetTrade(person);
            RemoveMerchant(person);
            AddHangeron(person);
            Callback_OnMerchant(false,tradeID, person);
            DoStaffChange(person);
            return true;
        }
        //关进监狱
        public virtual bool HireJail(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanHireJail(person))
                return false;

            AddJail(person);
            RemoveNoCoreStaff(person);
            Callback_OnJailer(true,person);
            DoStaffChange(person);
            return true;
        }
        //出狱
        public virtual bool FireJail(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanFireJail(person))
                return false;
            RemoveJail(person);
            AddHangeron(person);
            Callback_OnJailer(false,person);
            DoStaffChange(person);
            return true;
        }
        //抓捕俘虏
        public virtual bool HireCaptive(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanHireCaptive(person))
                return false;

            AddCaptive(person);
            RemoveNoCoreStaff(person);
            Callback_OnCaptive(true, person);
            DoStaffChange(person);
            return true;
        }
        //释放俘虏
        public virtual bool FireCaptive(TData person)
        {
            if (person == null)
                return false;
            if (!IsCanFireCaptive(person))
                return false;
            RemoveCaptive(person);
            AddHangeron(person);
            Callback_OnCaptive(false, person);
            DoStaffChange(person);
            return true;
        }
        #endregion

        #region Rand
        public sealed override TData Add(TData data)=> base.Add(data);
        public sealed override void Remove(TData data)=> base.Remove(data);
        //随机君主
        public TData RandMonarch(int age)
        {
            if (IsHaveMonarch())
            {
                CLog.Error($"{SelfUnit.GetName()} 已经存在以为君主");
                return null;
            }
            var civilData = GetNationCivilData();
            var config = GetNationConfig();
            var temp = GStaffMgr.RandPerson(civilData, age, Gender.Male, config.LastName);
            Add(temp);
            AddMonach(temp);
            return temp;
        }
        //随机继承人
        public TData RandPrince(int age,string lastname)
        {
            var civilData = GetNationCivilData();
            var temp = GStaffMgr.RandPerson(civilData, age, Gender.Male, lastname);
            Add(temp);
            AddPrince(temp);
            return temp;
        }
        //随机候选人
        public List<TData> RandCandiaties(int count)
        {
            List<TData> ret = new List<TData>();
            for (int i = 0; i < count; ++i)
            {
                ret.Add(RandCandiate());
            }
            return ret;
        }
        //随机候选人
        public TData RandCandiate()
        {
            //优先获得城市的NameData,根据每个城市的文明,生成候选人
            var nameData = GetRandCivilData();
            var age = new List<AgeRange> { AgeRange.Adult, AgeRange.Middle, AgeRange.Old }.GetAge();
            var temp = GStaffMgr.RandPerson(nameData, age, Gender.Male);
            Add(temp);
            AddCandiate(temp);
            return temp;
        }
        //随机美女
        public TData RandBeauty()
        {
            var nameData = GetRandCivilData();
            var temp = GStaffMgr.RandPerson(nameData,RandUtil.Range(18,25), Gender.Female);
            Add(temp);
            AddBeauty(temp);
            return temp;
        }
        #endregion

        #region Utile
        //获取随机人物的文明类型
        TDBaseCivilData GetRandCivilData()
        {
            //优先获得城市的NameData,根据每个城市的文明,生成人物
            var castleMgr = BaseGlobal.GetUnitSpawnMgr(typeof(TCastle));
            List<TCastle> castleList = castleMgr.IListData as List<TCastle>;
            TCastle castle = castleList.Rand();
            ITDCastleData castleConfig = castle.BaseConfig as ITDCastleData;
            var civilData = BaseGlobal.TDCivil.Get<TDBaseCivilData>(castleConfig.Civil);
            if (civilData == null)
                civilData = GetNationCivilData();
            return civilData;
        }
        //获得国家的文明类型
        TDBaseCivilData GetNationCivilData()
        {
            ITDNationData config = SelfUnit.BaseConfig as ITDNationData;
            var civilData = BaseGlobal.TDCivil.Get<TDBaseCivilData>(config.Civil);
            if (civilData == null)
            {
                CLog.Error($"国家:{config.GetName()}没有CivalData:{config.Civil}");
            }
            return civilData;
        }
        //获得国家的配置
        ITDNationData GetNationConfig()
        {
            ITDNationData config = SelfUnit.BaseConfig as ITDNationData;
            return config;
        }
        #endregion

        #region Get
        protected TData GetUnit(long id) => BaseGlobal.GetTDData<TData>(id);
        protected int GetOfficial(TData person)
        {
            if (!Official.ContainsValue(person))
                return 0;
            return Official[person];
        }
        protected string GetTrade(TData person)
        {
            if (!Merchant.ContainsValue(person))
                return "";
            return Merchant[person];
        }
        protected TLegion GetLegion(TData person)
        {
            if (person == null)
                return null;
            if(General.ContainsValue(person))
                return General[person];
            return null;
        }
        protected TCastle GetCastle(TData person)
        {
            if (person == null)
                return null;
            if (Prefect.ContainsValue(person))
                return Prefect[person];
            return null;
        }
        #endregion

        #region Callback
        protected virtual void OnThrone(ThroneType throneType, TData staffData)
        { 
        
        }
        #endregion

        #region DB
        public void LoadDBData(ref DBBaseStaffGroup data)
        {
            Monarch = GetUnit(data.Monarch);
            Crown = GetUnit(data.Crown);
            Diplomat = GetUnit(data.Diplomat);
            Queen = GetUnit(data.Queen);
            Deputy = GetUnit(data.Deputy);
            Spy = GetUnit(data.Spy);
            data.Prince.ForEach(x => Prince.Add(GetUnit(x)));
            data.Official.ForEach((k, v) => Official.Add(k, GetUnit(v)));
            data.Candidate.ForEach(x => Candidate.Add(GetUnit(x)));
            data.Hangeron.ForEach(x => Hangeron.Add(GetUnit(x)));
            data.Beauty.ForEach(x => Beauty.Add(GetUnit(x)));
            data.Harem.ForEach(x => Harem.Add(GetUnit(x)));
            data.Jailer.ForEach(x => Jailer.Add(GetUnit(x)));
            data.Captive.ForEach(x => Captive.Add(GetUnit(x)));
            data.Merchant.ForEach((k, v) => Merchant.Add(k,GetUnit(v)));
            data.General.ForEach((k, v) =>
            {
                var legion = BaseGlobal.GetUnit<TLegion>(k);
                var person = GetUnit(v);
                if (legion != null && person != null)
                    General.Add(legion, person);
            });
            data.Prefect.ForEach((k,v)=> {
                var castle = BaseGlobal.GetUnit<TCastle>(k);
                var person = GetUnit(v);
                if (castle != null && person != null)
                    Prefect.Add(castle, person);
            });
            data.Persons.ForEach(x =>
            {
                var person = GetUnit(x);
                if (person != null)
                    Data.Add(person);
            });
            CurStruggleProp = data.CurStruggleProp;
        }
        public void SaveDBData(ref DBBaseStaffGroup data)
        {
            if (Monarch != null) data.Monarch = Monarch.ID;
            if (Crown != null) data.Crown = Crown.ID;
            if (Diplomat != null) data.Diplomat = Diplomat.ID;
            if (Queen != null) data.Queen = Queen.ID;
            if (Deputy != null) data.Deputy = Deputy.ID;
            if (Spy != null) data.Spy = Spy.ID;
            data.Prince = Prince.Select(x => x.ID).ToList();
            data.Candidate = Candidate.Select(x => x.ID).ToList();
            data.Hangeron = Hangeron.Select(x => x.ID).ToList();
            data.Beauty = Beauty.Select(x => x.ID).ToList();
            data.Harem = Harem.Select(x => x.ID).ToList();
            data.Jailer = Jailer.Select(x => x.ID).ToList();
            data.Captive = Captive.Select(x => x.ID).ToList();
            data.Official = Official.ToDictionary(k => k.Key, v => v.Value.ID);
            data.Merchant = Merchant.ToDictionary(k => k.Key, v => v.Value.ID);
            data.General = General.ToDictionary(k => k.Key.ID, v => v.Value.ID);
            data.Prefect = Prefect.ToDictionary(k => k.Key.ID, v => v.Value.ID);
            data.Persons = Data.Select(x => x.ID).ToList();
            data.CurStruggleProp = CurStruggleProp;
        }
        #endregion
    }
}