using System;
using System.Collections.Generic;

namespace CYM
{
    #region 基类
    // 动作条件
    public class BaseCondition
    {
        #region prop
        protected BaseGlobal SelfBaseGlobal => BaseGlobal.Ins;
        protected BaseConditionMgr ACM => BaseGlobal.ACM;
        protected float val = 0.0f;
        protected ACCompareType CompareType;

        public bool IsOnlyPlayer { get; private set; } = false;
        public bool IsInvert { get; protected set; } = false;
        public bool IsIgnore { get; protected set; } = false;
        public bool IsCost { get; protected set; } = false;
        public bool IsTrue { get; protected set; } = false;
        public bool IsCondition { get; protected set; } = true;
        #endregion

        #region set
        public bool DoAction()
        {
            if (!BaseGlobal.BattleMgr.IsLoadBattleEnd)
                return false;
            if (!OnCheckError())
                return false;
            if (IsIgnore || !IsCondition)
            {
                IsTrue = true;
            }
            else
            {
                bool r = OnDoActionImpl();
                IsTrue = IsInvert ? !r : r;
            }
            return GetRet();
        }
        public BaseCondition Reset()
        {
            this.IsTrue = false;
            this.IsInvert = false;
            this.IsIgnore = false;
            this.IsCondition = true;
            this.IsOnlyPlayer = false;
            return this;
        }
        public BaseCondition OnlyPlayer()
        {
            IsOnlyPlayer = true;
            return this;
        }
        public BaseCondition NoCondition()
        {
            IsCondition = false;
            return this;
        }
        public BaseCondition Invert()
        {
            IsInvert = true;
            return this;
        }
        public BaseCondition Ignore()
        {
            IsIgnore = true;
            return this;
        }
        public BaseCondition Compare(ACCompareType type, float val)
        {
            this.CompareType = type;
            this.val = val;
            return this;
        }
        public bool GetRet()
        {
            if (IsIgnore) return true;
            return IsTrue;
        }
        protected string GetStr(string str = "", params object[] obs)
        {
            str = BaseLangMgr.Get(str, obs);
            return UIUtil.Condition(GetRet(), str);
        }
        protected string JointColon(string key, params object[] param)
        {
            var str = BaseLangMgr.JointColon(key, param);
            return UIUtil.Condition(GetRet(), str);
        }
        public virtual string GetDesc()
        {
            if (IsInvert)
                return GetStr(IDescKey);
            return GetStr(FDescKey);
        }
        protected virtual bool DoCompare(float inputVal)
        {
            if (CompareType == ACCompareType.Equal)
            {
                if (inputVal == val) return true;
            }
            else if (CompareType == ACCompareType.More)
            {
                if (inputVal > val) return true;
            }
            else if (CompareType == ACCompareType.MoreEqual)
            {
                if (inputVal >= val) return true;
            }
            else if (CompareType == ACCompareType.Less)
            {
                if (inputVal < val) return true;
            }
            else if (CompareType == ACCompareType.LessEqual)
            {
                if (inputVal <= val) return true;
            }
            return false;
        }
        #endregion

        #region get
        public virtual string GetCost() => "";
        public virtual string FDescKey => "";
        public virtual string IDescKey => "";
        #endregion

        protected virtual bool OnDoActionImpl() => true;
        protected virtual bool OnCheckError() { return true; }
    }
    public class BasePlayerCondition<TPlayer>: BaseCondition where TPlayer:BaseUnit
    {
        #region prop
        protected TPlayer SelfPlayer =>BaseGlobal.ACM.ParamSelfBaseUnit as TPlayer;
        protected TPlayer OtherPlayer;
        #endregion

        #region set
        protected override bool OnCheckError()
        {
            if (SelfPlayer == null)
            {
                CLog.Error($"错误:没有设置{nameof(SelfPlayer)}");
                return false;
            }
            return true;
        }
        public BasePlayerCondition<TPlayer> SetTarget(TPlayer other)
        {
            OtherPlayer = other;
            return this;
        }
        #endregion
    }
    public class BaseUnitCondition<TUnit> : BaseCondition where TUnit : BaseUnit
    {
        protected TUnit SelfUnit;
        public BaseUnitCondition<TUnit> SetUnit(TUnit chara)
        {
            SelfUnit = chara;
            return this;
        }
    }
    #endregion

    #region Cost
    // 消耗判断
    public class BaseCostCondition<TUnit, Type> : BaseCondition 
        where Type : Enum 
        where TUnit : BaseUnit
    {
        public List<Cost<Type>> CostDatas;
        public TUnit Unit => ACM.ParamSelfBaseUnit as TUnit;

        public BaseCostCondition() : base()
        {
            CostDatas = null;
            IsCost = true;
        }
        public BaseCostCondition<TUnit, Type> SetCost(List<Cost<Type>> datas)
        {
            foreach (var item in datas)
                item.SetUnit(Unit);
            CostDatas = datas;
            return this;
        }
        public BaseCostCondition<TUnit, Type> SetCost(Cost<Type> datas)
        {
            datas.SetUnit(Unit);
            CostDatas = new List<Cost<Type>>();
            CostDatas.Add(datas);
            return this;
        }
        protected override bool OnCheckError()
        {
            if (CostDatas == null)
            {
                CLog.Error("错误:没有设置CostDatas");
                return false;
            }
            if (Unit == null)
            {
                CLog.Error("错误!没有设置Unit!");
                return false;
            }
            return true;
        }
        protected override bool OnDoActionImpl()
        {
            foreach (var item in CostDatas)
            {
                if (item.IsCondition)
                {
                    if (GetAttrVal(item.Type) < item.RealVal)
                        return false;
                }
            }
            return true;
        }
        //此类型,基本不显示条件(保留)
        public override string GetDesc()
        {
            string retStr = "";
            int index = 0;
            if (CostDatas == null) return retStr;
            foreach (var item in CostDatas)
            {
                if (item.RealVal == 0) continue;
                if (!item.IsCondition) continue;

                bool tempBool = true;
                if (MathUtil.Round(GetAttrVal(item.Type), 2) < item.RealVal)
                    tempBool = false;
                string tempstr = BaseLangMgr.Get("AC_IsAttrToAct", item.Type.GetName(), ACCompareType.MoreEqual.GetFull().GetName(), item.GetDesc(false, false, false));
                if (tempBool) retStr += Const.STR_Indent + UIUtil.Green(tempstr);
                else retStr += Const.STR_Indent + UIUtil.Red(tempstr);

                if (index < CostDatas.Count - 1)
                    retStr += "\n";
                index++;
            }
            return retStr;
        }
        public override string GetCost()
        {
            int index = 0;
            string ret = "";
            string retStr = "";
            if (CostDatas == null) return retStr;
            foreach (var item in CostDatas)
            {
                if (item.RealVal == 0) continue;

                bool tempBool = true;
                if (MathUtil.Round(GetAttrVal(item.Type), 2) < item.RealVal) tempBool = false;

                string temp = item.GetDesc(false, false);
                if (tempBool || !item.IsCondition) temp = Const.STR_Indent + UIUtil.Green(temp);
                else temp = Const.STR_Indent + UIUtil.Red(temp);

                if (index < CostDatas.Count - 1)
                    temp += "\n";
                index++;
                ret += temp;
            }
            return ret;
        }
        protected float GetAttrVal(Type type) => Unit.AttrMgr.GetVal(Enum<Type>.Int(type));
    }
    #endregion

    #region Attr
    public class BaseAttrCondition<TUnit, Type> : BaseCondition 
        where Type : Enum 
        where TUnit : BaseUnit
    {
        protected Type AttrType;
        public TUnit Unit => ACM.ParamSelfBaseUnit as TUnit;

        public BaseCondition SetAttr(Type type)
        {
            AttrType = type;
            return this;
        }
        protected override bool OnDoActionImpl()
        {
            return DoCompare(GetAttrVal(AttrType));
        }
        public override string GetDesc()
        {
            TDBaseAttrData attrData = BaseAttrMgr<Type>.GetAttrDataList()[Enum<Type>.Int(AttrType)];
            return GetStr("AC_IsAttrToAct", (AttrType as Enum).GetName(), CompareType.GetName(), BaseAttrMgr<Type>.GetAttrNumberStr(AttrType, val));
        }

        #region 必须重载的函数
        protected virtual float GetAttrVal(Type type) => Unit.AttrMgr.GetVal(Enum<Type>.Int(type));
        #endregion
    }
    #endregion

    #region 简单条件
    // 简单的条件
    public class SimpleCondition
    {
        //只有玩家才会生效
        public bool IsOnlyPlayer { get; private set; } = false;
        public bool IsAutoTrans { get; private set; } = true;
        public bool IsCondition { get; private set; } = true;
        public SimpleCondition(bool b, string key, params object[] objs)
        {
            isTrue = b;
            this.objs = objs;
            this.key = key;
        }

        protected string key;
        protected object[] objs;
        protected bool isTrue;

        public bool DoAction()
        {
            return isTrue || !IsCondition;
        }
        public virtual string GetDesc()
        {
            string str = IsAutoTrans ? BaseLangMgr.Get(key, objs) : key;
            return UIUtil.Condition(isTrue, str);
        }
        public SimpleCondition OnlyPlayer()
        {
            IsOnlyPlayer = true;
            return this;
        }
        public SimpleCondition NoTrans()
        {
            IsAutoTrans = false;
            return this;
        }
        public SimpleCondition NoCondition()
        {
            IsCondition = false;
            return this;
        }
    }
    // 独占条件
    public class ExCondition : SimpleCondition
    {
        public ExCondition(bool b, string key, params object[] objs) : base(b, key, objs) { }
        public override string GetDesc()
        {
            string str = BaseLangMgr.Get(key, objs);
            if (!isTrue) return UIUtil.Red(str);
            else return UIUtil.Green(str);
        }
        public new ExCondition OnlyPlayer()
        {
            base.OnlyPlayer();
            return this;
        }
        public new ExCondition NoTrans()
        {
            base.NoTrans();
            return this;
        }
    }
    #endregion
}