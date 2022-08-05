//**********************************************
// Class Name	: CYMBaseSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using UnityEngine;

namespace CYM
{
    public class BaseProjectorMgr : BaseMgr
    {
        #region prop
        protected GameObject ProjectorGO;
        protected Projector Projector;
        #endregion

        #region life
        public sealed override MgrType MgrType => MgrType.Unit;
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            ProjectorGO = GameObject.Instantiate(BaseGlobal.GRMgr.Prefab.Get(ProjectorGOStr));
            Projector = ProjectorGO.GetComponent<Projector>();
            ProjectorGO.transform.SetParent(SelfMono.Trans);
            ProjectorGO.transform.localPosition = Vector3.up;
            ProjectorGO.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }
        #endregion

        #region get
        protected virtual string ProjectorGOStr => "Projector_BlobShadow";
        #endregion
    }

}