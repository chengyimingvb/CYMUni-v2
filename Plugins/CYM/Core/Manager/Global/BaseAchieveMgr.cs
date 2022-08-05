namespace CYM
{
    public class BaseAchieveMgr<TData> : BaseGFlowMgr where TData : TDBaseAchieveData, new()
    {
        #region set
        /// <summary>
        /// 刷新成就数据
        /// </summary>
        public void RefreshData()
        {
            RefreshTryTriger();
        }
        public void RefreshTryTriger()
        {
        }
        /// <summary>
        /// 解锁成就
        /// </summary>
        /// <param name="data"></param>
        public void Triger(TData data)
        {
            if (data == null)
                return;
            data.State = true;
            data.UnlockTime = System.DateTime.Now;
        }
        public void TryTriger(TData data)
        {
            if (data.State)
                return;
            if (!IsCanTrigger(data))
                return;
            Triger(data);
        }
        /// <summary>
        /// 重置成就
        /// </summary>
        /// <param name="data"></param>
        public void Reset(TData data)
        {
            if (data == null)
                return;
            data.State = false;
        }
        #endregion

        #region is
        /// <summary>
        /// 是否可以触发
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public bool IsCanTrigger(TData data)
        {
            BaseGlobal.ACM.Reset(BaseGlobal.ScreenMgr.Player);
            BaseGlobal.ACM.Add(data.Targets);
            if (!BaseGlobal.ACM.IsTrue())
            {
                return false;
            }
            return true;
        }
        #endregion
    }

}