//------------------------------------------------------------------------------
// BaseAnimal.cs
// Copyright 2020 2020/6/22 
// Created by CYM on 2020/6/22
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM.AI;
namespace CYM
{
    public class BaseAnimal : BaseCoreMono
    {
        [Tooltip("动物会在这个大小的内走动")]
        public float _wanderRadius = 10f;
        public float _moveSpeed = 1f;
        public float _rotateSpeed = 60f;
        [Tooltip("动物平均停下来休息的时间")]
        public float _averageIdleTime = 20f;
        public AIWanderPosFinder _posFinder;
        Renderer[] _renderers;
        Transform cacheTrans;
        protected bool IsRendered { get; private set; }

        bool _isMoving;
        // 处于休息状态时，这个参数用来表示剩余时间
        float _remainIdleTime;

        // 处于移动状态时，这是它的目的地
        Vector3 _moveTarget;
        // 刚开始动物所处的地点
        Vector3 _originalPos;
        public override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void Start()
        {
            base.Start();
            _originalPos = transform.position;
            _originalPos.y = TerrainObj.Ins.SampleHeight(_originalPos);
            transform.position = _originalPos;
            _isMoving = false;
            _remainIdleTime = GetNewIdleTime();
            _renderers = gameObject.GetComponentsInChildren<Renderer>();
            foreach (var item in _renderers)
            {
                item.receiveShadows = false;
            }
            _posFinder = new AIWanderPosFinder(this, _wanderRadius * 0.2f, _wanderRadius * 0.5f);
            cacheTrans = transform;
            StopMove();
        }

        // Update is called once per frame
        public override void OnUpdate()
        {
            base.OnUpdate();
            OnUpdateRendered();
            if (!IsRendered) return;
            if (_isMoving)
            {
                Vector3 pos = transform.position;
                pos = Vector3.MoveTowards(pos, _moveTarget, _moveSpeed * Time.deltaTime);
                pos.y = TerrainObj.Ins.SampleHeight(pos);
                transform.position = pos;

                // 到达在旋转之前判断
                // 若到达则无需旋转，避免零距离旋转误差可能出现的错误
                Vector3 step = transform.position - _moveTarget;
                if (Mathf.Abs(step.x) <= 0.1f &&
                    Mathf.Abs(step.y) <= 0.1f &&
                    Mathf.Abs(step.z) <= 0.1f)
                {
                    _remainIdleTime = GetNewIdleTime();
                    StopMove();
                }
                else
                {

                    Vector3 dir = _moveTarget - transform.position;
                    dir.y = 0;
                    Quaternion dirRot = Quaternion.LookRotation(dir);
                    Quaternion rot = transform.rotation;
                    rot = Quaternion.RotateTowards(rot, dirRot, _rotateSpeed * Time.deltaTime);
                    transform.rotation = rot;
                }
            }
            else
            {
                _remainIdleTime -= Time.deltaTime;
                if (_remainIdleTime < 0)
                {
                    _moveTarget = _posFinder.GetNewTarget();
                    _moveTarget.y = TerrainObj.Ins.SampleHeight(_moveTarget);
                    StartMove();
                }
            }
        }

        float GetNewIdleTime()
        {
            return _averageIdleTime * Random.Range(1f, 1.5f);
        }
        protected virtual void OnUpdateRendered()
        {
            if (BaseGlobal.MainCamera == null) return;
            if (cacheTrans == null) return;
            Vector3 pos = BaseGlobal.MainCamera.WorldToViewportPoint(cacheTrans.position);
            IsRendered = (pos.x > 0f && pos.x < 1f && pos.y > 0f && pos.y < 1f);
        }

        #region virtual
        protected virtual void StopMove()
        {
            _isMoving = false;
        }
        protected virtual void StartMove()
        {
            _isMoving = true;
        }
        #endregion
    }
}