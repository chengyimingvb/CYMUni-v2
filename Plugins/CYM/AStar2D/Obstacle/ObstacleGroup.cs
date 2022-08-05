//------------------------------------------------------------------------------
// ObstacleGroup.cs
// Copyright 2020 2020/1/31 
// Created by CYM on 2020/1/31
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace CYM.AStar2D
{
    [HideMonoScript]
    public class ObstacleGroup : BaseMono
    {
        [SerializeField, Tooltip("中心点,是否可以被包含在其他障碍物里面")]
        public bool IsCanContainIn = false;
        [SerializeField, Tooltip("是否可以和其他障碍物相交")]
        public bool IsCanIntersects = true;
        public List<Obstacle> Obstacles { get; private set; } = new List<Obstacle>();
        public List<Collider> ObsColliders { get; private set; } = new List<Collider>();
        public List<Collider2D> ObsColliders2D { get; private set; } = new List<Collider2D>();
        Collider MainCollider;
        Collider2D MainCollider2D;

        #region life
        public override void Awake()
        {
            base.Awake();
            ParseObstacle();
        }
        #endregion

        #region set
        void ParseObstacle()
        {
            Obstacles.Clear();
            ObsColliders.Clear();
            ObsColliders2D.Clear();
            Obstacle[] temps = GetComponentsInChildren<Obstacle>();
            Collider[] colTemps = GetComponentsInChildren<Collider>();
            Collider2D[] colTemps2D = GetComponentsInChildren<Collider2D>();
            MainCollider = GetComponent<Collider>();
            MainCollider2D = GetComponent<Collider2D>();
            if (temps != null)
            {
                foreach (var item in temps)
                {
                    Obstacles.Add(item);
                }
            }
            if (colTemps != null)
            {
                foreach (var item in colTemps)
                {
                    ObsColliders.Add(item);
                }
            }
            if (colTemps2D != null)
            {
                foreach (var item in colTemps2D)
                {
                    ObsColliders2D.Add(item);
                }
            }
        }
        public void EnableObstacle(bool b)
        {
            foreach (var item in Obstacles)
            {
                item.gameObject.SetActive(b);
            }
        }
        #endregion

        #region is
        public bool IsInObstacle(Vector2 pos)
        {
            if (MainCollider != null && MainCollider.bounds.Contains(pos))
            {
                return true;
            }
            if (MainCollider2D != null && MainCollider2D.bounds.Contains(pos))
            {
                return true;
            }
            foreach (var item in ObsColliders)
            {
                if (item.bounds.Contains(pos))
                    return true;
            }
            foreach (var item in ObsColliders2D)
            {
                if (item.bounds.Contains(pos))
                    return true;
            }
            return false;
        }
        public bool IsIntersects(Bounds bounds)
        {
            if (MainCollider != null && MainCollider.bounds.Intersects(bounds))
            {
                return true;
            }
            if (MainCollider2D != null && MainCollider2D.bounds.Intersects(bounds))
            {
                return true;
            }
            foreach (var item in ObsColliders)
            {
                if (item.bounds.Intersects(bounds))
                    return true;
            }
            foreach (var item in ObsColliders2D)
            {
                if (item.bounds.Intersects(bounds))
                    return true;
            }
            return false;
        }
        #endregion

        #region inspector
        [Button("AddObstacle")]
        void AddObstacle()
        {
            gameObject.AddComponent<Obstacle>();
            gameObject.AddComponent<AutoSort2D>();
            gameObject.AddComponent<SpriteRenderer>();
        }
        #endregion
    }
}