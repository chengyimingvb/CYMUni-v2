//------------------------------------------------------------------------------
// CastleObj.cs
// Created by CYM on 2022/3/28
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public class CastleObj : BaseMono
    {
        [SerializeField]
        public CastleType CastleType = CastleType.Town;
        [SerializeField]
        public float Radius = 1.0f;

        public bool IsTerritory => CastleType == CastleType.City || CastleType == CastleType.Town;
        public bool IsBorough => CastleType == CastleType.Vill || CastleType == CastleType.Fort;

        #region set
        public void ResetRotate()
        {
            transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0, transform.eulerAngles.z);
        }
        public void ResetPos()
        {
            transform.localPosition = Vector3.zero;
        }
        #endregion

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(Trans.position, Radius);
        }
#endif
    }
}