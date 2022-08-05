//------------------------------------------------------------------------------
// BaseUnlockMgr.cs
// Copyright 2019 2019/11/9 
// Created by CYM on 2019/11/9
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace CYM
{
    public class UnlockData
    {
        public string TDID { get; set; } = Const.STR_Inv;
        public int Level { get; set; } = 1;
    }
    public class BaseUnlockMgr : BaseGFlowMgr
    {
        #region prop
        public Dictionary<string, List<TDBaseData>> AllUnLockKeys { get; private set; } = new Dictionary<string, List<TDBaseData>>();
        public Dictionary<string, Dictionary<string, UnlockData>> UnlockKeyVal { get; private set; } = new Dictionary<string, Dictionary<string, UnlockData>>();
        #endregion

        #region life
        protected virtual void OnAddUnlockCondition(object fromData, UnlockData unlockData) => throw new System.NotImplementedException();
        #endregion

        #region set
        protected void ReadUnlockData(TDBaseData data)
        {
            //设置科技解锁
            foreach (var unlockData in data.Unlock)
            {
                if (!AllUnLockKeys.ContainsKey(unlockData.TDID)) AllUnLockKeys.Add(unlockData.TDID, new List<TDBaseData>());
                if (!UnlockKeyVal.ContainsKey(data.TDID)) UnlockKeyVal.Add(data.TDID, new Dictionary<string, UnlockData>());

                AllUnLockKeys[unlockData.TDID].Add(data);
                UnlockKeyVal[data.TDID].Add(unlockData.TDID, unlockData);
            }
        }
        //尝试增加Unlock条件,tdid表示需要被解锁的id
        public void TryAddUnLockCondition(string tdid)
        {
            if (!BaseGlobal.ACM.IsReseted)
            {
                CLog.Error("ACM 组件没有Reset,无法添加条件");
                return;
            }
            if (!IsInUnLockData(tdid)) return;
            List<TDBaseData> tempTechs = GetUnLockParent(tdid);
            foreach (var item in tempTechs)
            {
                UnlockData unlockData = GetUnlockData(item, tdid);
                OnAddUnlockCondition(item, unlockData);
            }
        }
        #endregion

        #region get
        //获得与这个ID关联的unlock数据
        public UnlockData GetUnlockData(TDBaseData data, string unlockID)
        {
            if (data == null) return null;
            if (unlockID == null) return null;
            if (!UnlockKeyVal.ContainsKey(data.TDID)) return null;
            if (UnlockKeyVal[data.TDID].ContainsKey(unlockID))
                return UnlockKeyVal[data.TDID][unlockID];
            return null;
        }
        public List<UnlockData> GetWillUnlockedDatas(TDBaseData data, float? curLevel)
        {
            List<UnlockData> ret = new List<UnlockData>();
            if (data == null || curLevel==null) return ret;
            if (!UnlockKeyVal.ContainsKey(data.TDID)) return ret;
            foreach (var item in UnlockKeyVal[data.TDID].Values)
            {
                if ((curLevel.Value + 1) >= item.Level)
                    ret.Add(item);
            }
            return ret;
        }
        //获得可以解锁的科技
        private List<TDBaseData> GetUnLockParent(string tdid)
        {
            if (!IsInUnLockData(tdid)) return null;
            return AllUnLockKeys[tdid];
        }
        #endregion

        #region is
        //是否拥有锁定状态
        public bool IsInUnLockData(string tdid)
        {
            return AllUnLockKeys.ContainsKey(tdid);
        }
        #endregion

        #region Callback
        protected override void OnLuaParseEnd()
        {
            base.OnLuaParseEnd();
            foreach (var item in BaseLuaMgr.TDConfigList)
            {
                foreach (var tData in item.BaseDatas.Values)
                    ReadUnlockData(tData);
            }
        }
        #endregion
    }
}