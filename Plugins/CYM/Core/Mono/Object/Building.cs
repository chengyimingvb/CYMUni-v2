//------------------------------------------------------------------------------
// Building.cs
// Created by CYM on 2022/3/26
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using UnityEngine;
namespace CYM
{
    [HideMonoScript]
    public class Building : BaseMono
    {
        #region inspector
        [SerializeField]
        bool FixedNormal = false;
        [SerializeField]
        float YOffset = 0;
        #endregion

        #region set
        [Button()]
        public void AdjTerrainHeight()
        {
            Util.RaycastY(Trans, YOffset, Const.Layer_Terrain);
            if (FixedNormal)
            {
                RaycastHit hitInfo;
                Vector3 opos = Trans.position + Vector3.up * 999.0f;
                Physics.Raycast(new Ray(opos, Trans.position - opos), out hitInfo, float.MaxValue, (LayerMask)Const.Layer_Terrain);
                Quaternion xzRot = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
                xzRot.eulerAngles = new Vector3(xzRot.eulerAngles.x, Trans.rotation.eulerAngles.y, xzRot.eulerAngles.z);
                Trans.rotation = xzRot;
            }
            else
            {
                Quaternion xzRot = Quaternion.FromToRotation(Vector3.up, Vector3.up);
                xzRot.eulerAngles = new Vector3(xzRot.eulerAngles.x, Trans.rotation.eulerAngles.y, xzRot.eulerAngles.z);
                Trans.rotation = xzRot;
            }
        }
        #endregion
    }
}