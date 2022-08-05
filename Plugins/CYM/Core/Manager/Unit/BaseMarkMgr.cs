//------------------------------------------------------------------------------
// BaseFlagmarkMgr.cs
// Copyright 2020 2020/3/20 
// Created by CYM on 2020/3/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM
{
    public class BaseMarkData
    {
        public BaseMarkData(BaseMarkMgr mgr)
        {
            Mgr = mgr;
        }
        BaseMarkMgr Mgr;
        BaseConditionMgr ACM => BaseGlobal.ACM;
        public Dictionary<string, CD> Data { get; protected set; } = new Dictionary<string, CD>();

        public void Update()
        {
            List<string> clear = new List<string>();
            foreach (var item in Data)
            {
                item.Value.Update();
                if (item.Value.IsOver())
                    clear.Add(item.Key);
            }
            foreach (var item in clear)
                Data.Remove(item);
        }

        public void TryAddMarkExCondition()
        {
            ACM.Add(new ExCondition(!IsIn(), Util.GetStr("AC_无法操作的标记") + "\n" + GetListStr()).NoTrans());
        }


        public void AddData(string tdid, int cd)
        {
            if (!Data.ContainsKey(tdid))
                Data.Add(tdid, new CD(cd));
            Data[tdid].Reset(cd);
        }
        public string GetListStr()
        {
            string ret = "";
            ret = StrUtil.List(Data, (k, v) =>
            {
                return string.Format(Const.STR_Indent + "{0}({1})", k.GetName(), UIUtil.KMG(v.CurCount));
            });
            return ret;
        }
        public void Clear()
        {
            Data.Clear();
        }

        public bool IsIn()
        {
            if (Data == null) return false;
            return Data.Count > 0;
        }

        public void LoadDBData(Dictionary<string, CD> data)
        {
            if (data == null) return;
            Data = data;
        }
        public Dictionary<string, CD> GetDBData()
        {
            return Data;
        }
    }
    public class BaseMarkMgr : BaseMgr
    {
        #region flag
        List<BaseMarkData> Datas = new List<BaseMarkData>();
        //不能发动战争的理由
        protected BaseMarkData NoDecalarWar { get; set; }
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        public override void OnEnable()
        {
            base.OnEnable();
            foreach (var item in Datas) item.Clear();
            Datas.Clear();
            NoDecalarWar = CreateMarkData();
        }
        #endregion

        #region Manual Update
        protected void ManualUpdateData()
        {
            foreach (var item in Datas)
                item.Update();
        }
        #endregion

        #region set
        public BaseMarkData CreateMarkData()
        {
            var ret = new BaseMarkData(this);
            Datas.Add(ret);
            return ret;
        }
        public void AddNoDecalarWar(string tdid, int cd) => NoDecalarWar.AddData(tdid, cd);
        public void TryAddNoDecalarWarExCondition()
        {
            NoDecalarWar.TryAddMarkExCondition();
        }
        #endregion

        #region get
        public string GetNoDecalarWarListStr() => NoDecalarWar.GetListStr();
        #endregion

        #region is
        public bool IsInNoDecalarWar()
        {
            return NoDecalarWar.IsIn();
        }
        #endregion
    }
}