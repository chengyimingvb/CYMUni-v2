//------------------------------------------------------------------------------
// BaseMove2DMgr.cs
// Copyright 2020 2020/1/21 
// Created by CYM on 2020/1/21
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.AStar2D;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class BaseAStarMove2DMgr : BaseMgr
    {
        #region Properties
        protected List<IBaseNode> ExtraSeizedNodes = new List<IBaseNode>();
        protected Vector3 LastPosition { get; private set; } = Vector3.zero;
        public BaseTraversal2D Traversal2D { get; private set; }
        public PathSession Session { get; private set; }
        public AStarGrid AstarGrid => AStarGrid.DefaultGrid;
        public Node MoveTargetNode { get; private set; }
        public BaseUnit MoveTargetUnit { get; private set; }
        public AgentState2D State { get; private set; } = AgentState2D.Idle;
        public AgentDirection2D MoveDirect { get; private set; } = AgentDirection2D.Default;
        public Direct FaceDirect { get; private set; } = Direct.Right;
        public BasicMoveType2D MoveType { get; private set; } = BasicMoveType2D.None;
        public float MoveSpeed { get; set; } = 2.0f;
        public virtual bool IsMoving => MovingFlag;
        protected bool MovingFlag { get; set; }
        protected virtual float AwaitingFollowingTime => 1;
        protected virtual int MaxAwaitingFollowingCount => 3;
        #endregion

        #region prop
        protected BaseAStar2DMgr AStarMgr => BaseGlobal.AStar2DMgr;
        public Node PreNode { get; protected set; }
        public Node CurNode { get; protected set; }
        public Quaternion NewQuateration { get; private set; } = Quaternion.identity;
        Timer AwaitingFollowingTimer = new Timer(1);
        int AwaitFollowingCount = 0;
        #endregion

        #region Callback val
        public event Callback Callback_OnAwaitingFollowing;
        public event Callback Callback_OnAdvanceNodeFailed;
        public event Callback Callback_OnMoveStart;
        public event Callback Callback_OnMoveEnd;
        public event Callback Callback_OnMoveUnreachable;
        public event Callback Callback_OnAdvanceNode;
        public event Callback Callback_OnReachedNode;
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnEnable()
        {
            base.OnEnable();
            MoveTargetNode = null;
            MoveTargetUnit = null;
            MoveType = BasicMoveType2D.None;
            MoveDirect = AgentDirection2D.Default;
            Session = null;
            LastPosition = Vector3.zero;
            MovingFlag = false;
            State = AgentState2D.Idle;
            AwaitingFollowingTimer.Restart(AwaitingFollowingTime);
            AwaitFollowingCount = 0;
            AStarMgr.Callback_OnSeizeNode += OnSeizeNode;
        }
        public override void OnDisable()
        {
            base.OnDisable();
            AStarMgr.Callback_OnSeizeNode -= OnSeizeNode;
        }
        public override void OnAffterAwake()
        {
            base.OnAffterAwake();
            OnAddMoveComponet();
        }

        protected virtual void OnAddMoveComponet()
        {
            Traversal2D = new BaseTraversal2D(SelfBaseUnit);
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (SelfBaseUnit != null)
            {
                OnUpdatePath();
                SelfBaseUnit.Rot = Quaternion.Lerp(NewQuateration, SelfBaseUnit.Rot, Time.deltaTime * 0.1f);
            }
        }
        public override void OnBirth3()
        {
            base.OnBirth3();
            AdjToNode();
        }
        public override void OnDeath()
        {
            base.OnDeath();
            ChangeState(AgentState2D.Idle);
            ClearCurNode();
        }
        #endregion

        #region update
        protected virtual void OnUpdatePath()
        {
            if (!SelfBaseUnit.IsInited) return;
            if (!SelfBaseUnit.IsLive) return;
            if (SelfBaseUnit.IsRealDeath) return;
            switch (State)
            {
                default:
                case AgentState2D.Idle:
                    {
                        LastPosition = SelfBaseUnit.Pos;
                        OnUpdateIdle();
                    }
                    return;
                case AgentState2D.FollowingPath:
                    {
                        if (Session == null)
                        {
                            ChangeState(AgentState2D.Idle);
                            return;
                        }

                        //到达当前节点
                        if (Session.HasReachedCurrentNode(SelfBaseUnit.Trans))
                        {
                            OnReachedNode();
                            if (Session == null) return;
                            if (Session.IsLastNode) //到达终点
                            {
                                SelfBaseUnit.Pos = Session.Pos;
                                DoReached();
                            }
                            else //计算下一个节点
                            {
                                DoAdvanceNode();
                            }
                        }
                        //移动到下一个节点
                        else
                        {
                            OnUpdateToNodePos(Session.Pos);
                        }
                    }
                    break;
                case AgentState2D.AwaitingFollowing:
                    {
                        if (AwaitFollowingCount >= MaxAwaitingFollowingCount)
                        {
                            OnAdvanceNodeFailed();
                            DoReached();
                            return;
                        }
                        else if (AwaitingFollowingTimer.CheckOver())
                        {
                            AwaitFollowingCount++;
                            OnAwaitingFollowing();
                        }
                    }
                    break;
            }
        }

        protected virtual void OnUpdateToNodePos(Vector3 target)
        {
            float speedFaction = MoveSpeed * Time.deltaTime;
            Vector3 updatedPos = Vector3.MoveTowards(LastPosition, target, speedFaction);
            Vector3 direct = (updatedPos - LastPosition).normalized;
            if (direct.sqrMagnitude != 0)
            {
                if (direct.y <= 0.002f)
                {
                    MoveDirect = AgentDirection2D.Forward;
                }
                else
                {
                    MoveDirect = AgentDirection2D.Backward;
                }

                if (direct.x > 0)
                {
                    MoveDirect |= AgentDirection2D.Right;
                }
                else if (direct.x < 0)
                {
                    MoveDirect |= AgentDirection2D.Left;
                }
            }
            MovingFlag = (updatedPos != SelfBaseUnit.Pos);
            OnUpdateRotation(speedFaction, direct, updatedPos, target);
            OnUpdatePosition(speedFaction, direct, updatedPos, target);
            LastPosition = SelfBaseUnit.Pos;
        }
        #endregion

        #region set
        public void Look(Transform trans)
        {
            if (trans == null) return;
            Look(trans.position);
        }
        public void Look(Vector3 pos)
        {
            if (SelfBaseUnit.Pos.x >= pos.x)
            {
                NewQuateration = Quaternion.Euler(0, 180, 0);
                FaceDirect = Direct.Left;
            }
            else
            {
                NewQuateration = Quaternion.Euler(0, 0, 0);
                FaceDirect = Direct.Right;
            }
        }
        public void Look(BaseUnit unit)
        {
            if (unit == null) return;
            Look(unit.Trans);
        }
        public virtual bool AdjToNode()
        {
            ClearCurNode();
            Node node = AStarMgr.GetNode(SelfMono.Pos);
            if (node == null) return false;
            SelfMono.Pos = node.Pos;
            CalcCurNode();
            return true;
        }
        public virtual void SetToNode(IBaseNode node)
        {
            ClearCurNode();
            if (node == null) return;
            SelfMono.Pos = node.Pos;
            CalcCurNode();
        }
        protected void ChangeState(AgentState2D state)
        {
            if (state == AgentState2D.AwaitingFollowing)
            {
                AwaitFollowingCount = 0;
                AwaitingFollowingTimer.Restart(AwaitingFollowingTime);
                this.State = state;
            }
            else
            {
                this.State = state;
            }
        }
        protected virtual void DoAdvanceNode()
        {
            if (Session == null) return;
            if (Session.AdvanceNode())
                OnAdvanceNode();
        }
        #endregion

        #region move
        public bool MoveToUnitQueue(BaseUnit unit)
        {
            if (unit == null) return false;
            if (unit == SelfBaseUnit) return false;
            if (unit.Pos.IsInv()) return false;
            AStarMgr.AddMoveQueue(SelfBaseUnit, unit);
            return true;
        }
        public bool MoveToNode(Node node)
        {
            if (node == null) return false;
            if (node.Pos.IsInv()) return false;
            IsCanUnitOverlap = false;
            MoveType = BasicMoveType2D.MoveToNode;
            var ret = _moveIntoNode(node);
            IsTriggerDoTraversal = true;
            return ret;
        }
        // 移动到指定单位
        public bool MoveIntoUnit(BaseUnit unit)
        {
            if (unit == null) return false;
            if (unit.Pos.IsInv()) return false;
            MoveTargetUnit = unit;
            IsCanUnitOverlap = true;
            MoveType = BasicMoveType2D.MoveIntoUnit;
            var targetNode = AStarMgr.GetNode(unit.Pos);
            var ret = _moveIntoNode(targetNode);
            IsTriggerDoTraversal = true;
            return ret;
        }
        // 移动到指定单位边上
        public bool MoveToUnit(BaseUnit unit)
        {
            if (unit == null) return false;
            if (unit == SelfBaseUnit) return false;
            if (unit.Pos.IsInv()) return false;
            Node closetNode = AStarMgr.GetClosedNode(SelfBaseUnit, unit);
            if (closetNode == null) return false;
            MoveTargetUnit = unit;
            IsCanUnitOverlap = false;
            MoveType = BasicMoveType2D.MoveToUnit;
            var ret = _moveIntoNode(closetNode);
            IsTriggerDoTraversal = true;
            return ret;
        }
        public void DoReached()
        {
            Session = null;
            OnMoveEnd();
            ChangeState(AgentState2D.Idle);
        }
        #endregion

        #region private
        // 移动到指定节点
        bool _moveIntoNode(Node node)
        {
            if (!IsCanMove) return false;
            if (node == null) return false;
            if (MathUtil.Approximately(node.Pos, SelfBaseUnit.Pos)) return false;
            MoveTargetNode = node;
            _findPath(node.Index);
            return true;
        }
        private void _findPath(AStar2D.Index target)
        {
            ChangeState(AgentState2D.Idle);
            var curIndex = AstarGrid.GetIndex(SelfBaseUnit.transform.position);
            AstarGrid.FindPath(curIndex, target, IsTriggerDoTraversal ? Traversal2D : null, OnPathFound);
        }
        #endregion

        #region is
        public bool IsCanUnitOverlap { get; private set; } = false;
        public bool IsTriggerDoTraversal { get; set; } = true;
        public virtual bool IsCanMove => true;
        public bool IsCanTraversal(IBaseNode node)
        {
            if (Traversal2D == null) return true;
            return Traversal2D.Filter(node);
        }
        public bool IsInPath(IBaseNode node)
        {
            if (Session == null)
                return false;
            return Session.Contain(node);
        }
        #endregion

        #region private
        public void CalcCurNode()
        {
            if (!SelfBaseUnit.IsLive) return;
            PreNode = CurNode;
            CurNode = AStarMgr.GetNode(SelfBaseUnit.Pos);
            if (CurNode == null) CLog.Error("CalcCurNode:没有获取到寻路节点!!!{0}", SelfBaseUnit.GOName);
            else SelfBaseUnit.Pos = CurNode.Pos;
            AStarMgr.SetSeizeNode(CurNode, SelfBaseUnit);
        }
        public void ClearCurNode()
        {
            PreNode = CurNode;
            CurNode = AStarMgr.GetNode(SelfBaseUnit.Pos);
            if (CurNode == null) CLog.Error("ClearCurNode:没有获取到寻路节点!!!{0}", SelfBaseUnit.GOName);
            AStarMgr.ClearSeizeNode(CurNode, SelfBaseUnit);
        }
        #endregion

        #region Callback
        protected virtual void OnMoveStart()
        {
            Callback_OnMoveStart?.Invoke();
        }
        protected virtual void OnUpdateRotation(float speedFaction, Vector3 direct, Vector3 updatedPos, Vector3 sessionPos)
        {
            Look(sessionPos);
        }
        protected virtual void OnUpdatePosition(float speedFaction, Vector3 direct, Vector3 updatedPos, Vector3 sessionPos)
        {
            SelfBaseUnit.Pos = updatedPos;
        }
        protected virtual void OnUpdateIdle()
        {

        }
        private void OnMoveUnreachable()
        {
            MoveType = BasicMoveType2D.None;
            Callback_OnMoveUnreachable?.Invoke();
        }
        protected virtual void OnMoveEnd()
        {
            MoveType = BasicMoveType2D.None;
            Callback_OnMoveEnd?.Invoke();
        }
        protected virtual void OnSameStartEnd()
        {
        }
        protected virtual void OnSeizeNode(BaseUnit unit, IBaseNode node)
        {
        }
        private void OnPathFound(Path path, PathRequestStatus status)
        {
            if (status == PathRequestStatus.PathFound)
            {
                this.Session = new PathSession(path);
                OnMoveStart();
                DoAdvanceNode();
                ChangeState(AgentState2D.FollowingPath);
            }
            else if (status == PathRequestStatus.SameStartEnd)
            {
                ChangeState(AgentState2D.Idle);
                OnSameStartEnd();
            }
            else if (status == PathRequestStatus.PathNotFound)
            {
                OnMoveUnreachable();
                ChangeState(AgentState2D.Idle);
            }
            else
            {
                ChangeState(AgentState2D.Idle);
            }
        }
        protected virtual void OnAdvanceNode()
        {
            Callback_OnAdvanceNode?.Invoke();
        }
        protected virtual void OnReachedNode()
        {
            Callback_OnReachedNode?.Invoke();
        }
        protected virtual void OnAwaitingFollowing()
        {
            Callback_OnAwaitingFollowing?.Invoke();
        }
        protected virtual void OnAdvanceNodeFailed()
        {
            Callback_OnAdvanceNodeFailed?.Invoke();
        }
        #endregion
    }
}