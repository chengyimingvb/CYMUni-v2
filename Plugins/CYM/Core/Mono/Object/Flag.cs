//------------------------------------------------------------------------------
// BaseFlag.cs
// Copyright 2019 2019/2/6 
// Created by CYM on 2019/2/6
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public sealed class Flag : BaseMono
    {
        #region inspector
        [SerializeField]
        MeshRenderer Banner;
        [SerializeField]
        GameObject Wangguan;
        #endregion

        #region life
        public override void OnEnable()
        {
            base.OnEnable();
            if (Wangguan!=null)
            {
                Wangguan?.SetActive(false);
            }
        }
        #endregion

        #region set
        public void Change(Texture tex)
        {
            if (Banner != null)
            {
                Banner.material.SetTexture("_MainTex", tex);
            }
        }
        public void ShowWangguan(bool b)
        {
            if (Wangguan == null)
                return;
            if (Wangguan.activeSelf == b)
                return;
            Wangguan?.SetActive(b);
        }
        public void AdjTerrainHeight()
        {
            Util.RaycastY(Trans, 0.0f, Const.Layer_Terrain);
        }
        #endregion
    }
}