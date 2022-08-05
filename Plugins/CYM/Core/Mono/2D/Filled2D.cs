//------------------------------------------------------------------------------
// Filled2D.cs
// Copyright 2019 2019/12/26 
// Created by CYM on 2019/12/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public sealed class Filled2D : MonoBehaviour
    {
        #region Inspector
        [SerializeField]
        public SpriteRenderer Sprite;
        [SerializeField]
        public SpriteRenderer BG;
        #endregion

        #region life
        float sourceScaleX;
        Vector3 sourceScale;
        private void Awake()
        {
            Sprite = GetComponent<SpriteRenderer>();
            sourceScaleX = transform.localScale.x;
            sourceScale = transform.localScale;
        }
        private void OnEnable()
        {
            transform.localScale = sourceScale;
        }
        #endregion

        #region val
        public float Percent { get; set; } = 0.5f;
        #endregion

        #region set
        public void RefreshBlood(float percent)
        {
            Percent = percent;
            this.transform.localScale = new Vector3(Percent * sourceScaleX, this.transform.localScale.y, this.transform.localScale.z);
        }
        public void ChangeSprite(Sprite sprite)
        {
            if (Sprite)
                Sprite.sprite = sprite;
        }
        public void RefreshSort(int sort)
        {
            if (Sprite)
                Sprite.sortingOrder = sort + 1;
            if (BG)
                BG.sortingOrder = sort;
        }
        #endregion
    }
}