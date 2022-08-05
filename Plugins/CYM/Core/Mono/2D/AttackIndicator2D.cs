//------------------------------------------------------------------------------
// AttackIndicator2D.cs
// Copyright 2020 2020/2/20 
// Created by CYM on 2020/2/20
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public sealed class AttackIndicator2D : MonoBehaviour
    {
        [SerializeField]
        SpriteRenderer Sprite;
        [SerializeField]
        Sprite[] AnimSprites;

        bool IsShow = true;
        float curDetail = 0;
        int AnimIndex = 0;

        public void Look(BaseMono self, BaseMono mono)
        {
            if (mono != null && self != null && transform != null)
                transform.LookAt(mono.Trans, Vector3.forward);
        }
        public void Show(bool b)
        {
            if (IsShow == b) return;
            IsShow = b;
            gameObject.SetActive(b);
        }
        public void RefreshSort(int sort)
        {
            Sprite.sortingOrder = sort;
        }
        public void RefreshIcon(Sprite sprite)
        {
            Sprite.sprite = sprite;
        }
        public void UpdateAnim()
        {
            if (!IsShow) return;
            if (AnimSprites == null || AnimSprites.Length <= 1)
                return;
            RefreshIcon(AnimSprites[AnimIndex]);
            AnimIndex++;
            if (AnimIndex >= AnimSprites.Length)
                AnimIndex = 0;
        }
    }
}