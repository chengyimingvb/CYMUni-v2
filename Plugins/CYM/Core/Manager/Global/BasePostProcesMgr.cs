//------------------------------------------------------------------------------
// BasePostProcesMgr.cs
// Created by CYM on 2022/3/30
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace CYM
{
    public class BasePostProcesMgr : BaseGFlowMgr 
    {
        #region prop
        protected Camera Camera { get; private set; }
        protected PostProcessVolume Volume { get; private set; }
        protected Light Sun { get; private set; }
        protected Vector3 SourceSunAngle { get; private set; }
        #endregion

        #region life
        //是否需要太阳公转
        protected virtual bool NeedSunCircle => true;
        protected virtual float SunSpeed => 1.0f;
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();

        }
        protected override void OnBattleLoadedScene()
        {
            base.OnBattleLoadedScene();
            if (SunObj.Obj)
            {
                Sun = SunObj.Obj;
                SourceSunAngle = Sun.transform.eulerAngles;
            }
            if (CameraObj.Obj)
            {
                Camera = CameraObj.Obj;
            }
            Volume = Camera.GetComponentInChildren<PostProcessVolume>();

            if (NeedSunCircle && Sun != null)
            {
                Sun.transform.eulerAngles = new Vector3(SourceSunAngle.x, RandUtil.Range(0,360), SourceSunAngle.z);
            }

        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (BattleMgr==null || !BattleMgr.IsLoadBattleEnd)
                return;
            OnPostProcessUpdate();
            if (NeedSunCircle && Sun != null)
            {

                float yRotate = Sun.transform.eulerAngles.y;
                //if (yRotate > 360) yRotate = 0;
                //if (yRotate > 100 && yRotate < 270) speed = 5.0f;
                Sun.transform.eulerAngles = new Vector3(SourceSunAngle.x, yRotate + Time.smoothDeltaTime * SunSpeed, SourceSunAngle.z);
            }
        }
        protected virtual void OnPostProcessUpdate()
        { 
        
        }
        #endregion

        #region get
        public T GetSetting<T>() where T : PostProcessEffectSettings
        {
            T ret = default;
            if (Volume && Volume.profile)
                Volume.profile.TryGetSettings(out ret);
            return ret;
        }
        #endregion

        #region set
        public virtual void EnablePostProcess(bool b)
        {
            BaseGlobal.Settings.EnablePostProcess = b;
            if (Volume)
                Volume.enabled = b;
        }
        #endregion
    }
}