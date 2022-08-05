//------------------------------------------------------------------------------
// BaseAutoTileMove2DMgr.cs
// Copyright 2020 2020/2/17 
// Created by CYM on 2020/2/17
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.AStar2D;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace CYM
{
    public class BaseAStarTileMove2DMgr : BaseAStarMove2DMgr
    {

        private TweenerCore<Vector3, Vector3, VectorOptions> TweenLastMove;

        public override bool IsMoving => base.IsMoving || (TweenLastMove != null ? TweenLastMove.IsPlaying() : false);
        protected override float AwaitingFollowingTime => MoveSpeed;
        protected override int MaxAwaitingFollowingCount => 3;

        public override void OnDeath()
        {
            base.OnDeath();
            var curNode = AStarMgr.GetNode(SelfBaseUnit.Pos);
            AStarMgr.ExSeizeNodeData.ClearSeizeNode(curNode, SelfBaseUnit);
        }
        protected override void DoAdvanceNode()
        {
            if (Session == null) return;
            var targetNode = Session.NextTileNode;
            if (TryMove(targetNode))
            {
                base.DoAdvanceNode();
            }
            else
            {
                ChangeState(AgentState2D.AwaitingFollowing);
            }
        }
        protected virtual bool TryMove(IBaseNode node)
        {
            if (node == null ||
                AStarMgr.ExSeizeNodeData.IsHaveUnit(node) ||
                AStarMgr.IsHaveUnit(node))
            {
                return false;
            }
            else
            {
                ClearCurNode();
                var curNode = AStarMgr.GetNode(SelfBaseUnit.Pos);
                AStarMgr.ExSeizeNodeData.ClearSeizeNode(curNode, SelfBaseUnit);
                AStarMgr.ExSeizeNodeData.SetSeizeNode(node, SelfBaseUnit);
                TweenLastMove?.Kill();
                TweenLastMove = DOTween.To(() => SelfBaseUnit.Pos, x => SelfBaseUnit.Pos = x, node.Pos, MoveSpeed).SetEase(Ease.Linear);
                return true;
            }
        }
        protected override void OnMoveStart()
        {
            base.OnMoveStart();
            TweenLastMove?.Kill();
            MovingFlag = true;
            AStarMgr.RemoveMoveQueue(SelfBaseUnit);
        }
        protected override void OnMoveEnd()
        {
            base.OnMoveEnd();
            TweenLastMove?.Kill();
            CalcCurNode();
            MovingFlag = false;
            AStarMgr.RemoveMoveQueue(SelfBaseUnit);
        }
        protected override void OnAwaitingFollowing()
        {
            base.OnAwaitingFollowing();
            var targetNode = Session.NextTileNode;
            if (TryMove(targetNode))
            {
                ChangeState(AgentState2D.FollowingPath);
                base.DoAdvanceNode();
            }
        }

        protected override void OnUpdateToNodePos(Vector3 target)
        {
            // do nothing
        }
        protected override void OnUpdatePosition(float speedFaction, Vector3 direct, Vector3 updatedPos, Vector3 sessionPos)
        {
            // do noting
        }
        protected override void OnUpdateRotation(float speedFaction, Vector3 direct, Vector3 updatedPos, Vector3 sessionPos)
        {
            // do noting
        }
        protected override void OnUpdateIdle()
        {
            // do noting
        }
    }
}