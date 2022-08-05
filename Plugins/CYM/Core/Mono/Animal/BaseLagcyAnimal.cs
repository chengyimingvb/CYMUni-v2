//------------------------------------------------------------------------------
// BaseLagcyAnimal.cs
// Copyright 2020 2020/6/23 
// Created by CYM on 2020/6/23
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public class BaseLagcyAnimal : BaseAnimal
    {
        Animation _animator;
        public override void Awake()
        {
            _animator = GetComponent<Animation>();
            base.Awake();
        }
        public override void Start()
        {
            base.Start();
        }
        protected override void StopMove()
        {
            base.StopMove();
            _animator.CrossFade("idle" + RandUtil.RandInt(0, 2), 0.1f);
        }
        protected override void StartMove()
        {
            base.StartMove();
            _animator.CrossFade("walk", 0.1f);
        }
    }
}