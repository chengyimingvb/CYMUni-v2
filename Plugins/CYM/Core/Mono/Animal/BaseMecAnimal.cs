//------------------------------------------------------------------------------
// BaseAnimal.cs
// Copyright 2019 2019/3/26 
// Created by CYM on 2019/3/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public class BaseMecAnimal : BaseAnimal
    {
        Animator _animator;
        public override void Awake()
        {
            _animator = GetComponent<Animator>();
            if (_animator != null)
            {
                _animator.applyRootMotion = false;
                _animator.cullingMode = AnimatorCullingMode.CullCompletely;
                _animator.updateMode = AnimatorUpdateMode.Normal;
            }
            base.Awake();
        }
        protected override void StopMove()
        {
            base.StopMove();
            _animator.CrossFade("Idle" + RandUtil.RandInt(0, 4), 0.1f, 0, RandUtil.Range(0.0f, 0.9f));
        }
        protected override void StartMove()
        {
            base.StartMove();
            _animator.CrossFade("Move", 0.1f);
        }
    }
}