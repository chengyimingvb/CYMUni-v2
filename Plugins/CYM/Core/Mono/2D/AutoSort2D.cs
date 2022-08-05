//------------------------------------------------------------------------------
// SpriteRenderAutoSortOrder.cs
// Copyright 2020 2020/1/31 
// Created by CYM on 2020/1/31
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    [ExecuteInEditMode]
    public sealed class AutoSort2D : MonoBehaviour
    {
        SpriteRenderer SpriteRenderer;
        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }
        private void Start()
        {
            RefreshSort();
        }
        private void OnEnable()
        {
            RefreshSort();
        }

        void RefreshSort()
        {
            int y = Mathf.RoundToInt(-Mathf.CeilToInt(transform.position.y * 10));
            if (SpriteRenderer != null)
            {
                SpriteRenderer.sortingOrder = y;
            }
        }
    }
}