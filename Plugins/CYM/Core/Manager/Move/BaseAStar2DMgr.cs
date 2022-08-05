//------------------------------------------------------------------------------
// BaseAStar2DMgr.cs
// Copyright 2020 2020/1/15 
// Created by CYM on 2020/1/15
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.AStar2D;
using CYM.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    public class GraphNodeRange2D
    {
        public GraphNodeRange2D Parent;
        public Node Node;
        public int Depth = 0;
        public float Distance = 0;

        public GraphNodeRange2D(GraphNodeRange2D parent, Node node)
        {
            Parent = parent;
            Node = node;
            if (Parent != null)
            {
                Depth = Parent.Depth + 1;
                Distance = Parent.Distance + Vector3.Distance(Node.Pos, Parent.Node.Pos);
            }
        }
    }
    public class BaseTraversal2D
    {
        protected BaseAStar2DMgr BaseAStar2DMgr => BaseGlobal.AStar2DMgr;
        protected AStarGrid Ins => AStarGrid.Ins;
        protected BaseUnit SelfUnit;

        public BaseTraversal2D(BaseUnit legion)
        {
            SelfUnit = legion;
        }

        public virtual bool Filter(IBaseNode node)
        {
            return !BaseAStar2DMgr.IsHaveUnit(node);
        }
    }
    public class BaseAStar2DMgr : BaseGFlowMgr
    {
        #region Callback
        public event Callback<BaseUnit, IBaseNode> Callback_OnSeizeNode;
        #endregion

        #region prop
        protected OrderDic<BaseUnit, BaseUnit> MoveQueue = new OrderDic<BaseUnit, BaseUnit>();
        public SeizeNodeData<IBaseNode> SeizeNodeData { get; private set; } = new SeizeNodeData<IBaseNode>();
        public SeizeNodeData<IBaseNode> ExSeizeNodeData { get; private set; } = new SeizeNodeData<IBaseNode>();
        protected AStarGrid Ins => AStarGrid.Ins;
        Timer MoveQueueTimer = new Timer(0.01f);
        #endregion

        #region life
        protected override void OnSetNeedFlag()
        {
            base.OnSetNeedFlag();
            NeedUpdate = true;
        }
        public override void OnCreate()
        {
            base.OnCreate();
            OnSetDefaultRVOPropty();
        }
        protected virtual void OnSetDefaultRVOPropty()
        {

        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (MoveQueueTimer.CheckOver())
            {
                if (MoveQueue.Count <= 0) return;
                var data = MoveQueue.First();
                if (data.Key == null || !data.Key.IsLive || data.Value == null || !data.Value.IsLive)
                {

                }
                else
                {
                    data.Key.Move2DMgr.MoveToUnit(data.Value);
                }
                RemoveMoveQueue(data.Key);
            }
        }
        #endregion

        #region set
        // 清理Node
        public void ClearSeizeNode(Node node, BaseUnit unit)
        {
            SeizeNodeData.ClearSeizeNode(node, unit);
        }
        // 设置占据的Node
        public void SetSeizeNode(Node node, BaseUnit unit)
        {
            SeizeNodeData.SetSeizeNode(node, unit);
            Callback_OnSeizeNode?.Invoke(unit, node);
        }
        void ClearAllData()
        {
            SeizeNodeData.Clear();
            ExSeizeNodeData.Clear();
            MoveQueue.Clear();
            ThreadManager.Active.DoDestroy();
        }
        public void SetDirtyGraph()
        {
            Ins.SetDirty();
        }
        public void RefreshGraph()
        {
            Ins.RebuildGraph();
        }
        public void AddMoveQueue(BaseUnit unit, BaseUnit target)
        {
            if (unit == null) return;
            if (target == null) return;
            if (MoveQueue.ContainsKey(unit))
            {
                MoveQueue[unit] = target;
                return;
            }
            MoveQueue.Add(unit, target);
        }
        public void RemoveMoveQueue(BaseUnit unit)
        {
            if (unit == null) return;
            MoveQueue.Remove(unit);
        }
        #endregion

        #region get
        // 获得某个节点上的所有单位
        public HashSet<BaseUnit> GetUnits(IBaseNode node)
        {
            return SeizeNodeData.GetUnits(node);
        }
        public Node GetNode(AStar2D.Index index)
        {
            return Ins.GetNode(index);
        }
        // 获得指定单位所在的节点
        public Node GetNode(BaseUnit unit)
        {
            return GetNode(unit.Pos);
        }
        // 获得指定位置的节点
        public Node GetNode(Vector2 pos)
        {
            var index = Ins.GetIndex(pos);
            return Ins.GetNode(index);
        }
        // 通过指定一个目标点,获得距离自己最近的一个点
        public Node GetClosedNode(BaseUnit selfUnit, BaseUnit targetUnit)
        {
            HashSet<Node> exclude = new HashSet<Node>();
            return GetCloset(0);

            Node GetCloset(int range)
            {
                if (range > Mathf.Max(Ins.gridX, Ins.gridY))
                    return null;
                List<Node> closedTargetNode = new List<Node>();
                Node closetNode = null;
                Node targetNode = GetNode(targetUnit);
                Node selfNode = GetNode(selfUnit);
                var nodes = GetBFS(targetUnit.Pos, range, selfUnit.Move2DMgr.IsCanTraversal, true);
                float maxDis_self = float.MaxValue;
                foreach (var item in nodes)
                {
                    if (exclude.Contains(item)) continue;
                    //过滤掉自身节点
                    if (targetNode == item) continue;
                    //过滤掉已经有单位并且不是自己的节点,防止来回反复移动
                    if (IsHaveUnit(item) && selfNode != item) continue;
                    //得到距离最近的一个节点
                    var dis_self = MathUtil.SqrDistance(selfUnit.Pos, item.Pos);
                    if (dis_self < maxDis_self)
                    {
                        closetNode = item;
                        maxDis_self = dis_self;
                    }
                    exclude.Add(item);
                }
                if (closetNode != null)
                    return closetNode;
                return GetCloset(range + 1);
            }
        }
        public Node GetSafeNode(Vector2 pos)
        {
            var index = Ins.GetSafeIndex(pos);
            return Ins.GetNode(index);
        }
        public Node GetSafeNode(BaseUnit selfUnit, Vector2 pos, bool isMaxDistance = true, bool isCanTraversal = true, bool noObstacle = true, bool noUnit = true, bool isDistanceWithEnd = true)
        {
            if (selfUnit.Pos.IsInv()) return null;
            var seed = GetSafeNode(pos);
            if (IsCondition(seed)) return seed;

            Node finalNearNode = null;
            Node noUnitNode = null;
            Node haveUnitNode = null;
            HashList<Node> links = new HashList<Node>();
            links = Ins.GetConnection(seed);
            float noUnitDistance = -1;
            float haveUnitDistance = -1;
            if (isMaxDistance)
            {
                noUnitDistance = -1;
                haveUnitDistance = -1;
            }
            else
            {
                noUnitDistance = float.MaxValue;
                haveUnitDistance = float.MaxValue;
            }
            foreach (var item in links)
            {
                var curDistance = 0.0f;
                if (isDistanceWithEnd) curDistance = MathUtil.AutoSqrDistance(seed.Pos, item.Pos);
                else curDistance = MathUtil.AutoSqrDistance(selfUnit.Pos, item.Pos);
                if (IsCondition(item))
                {
                    noUnitNode = item;
                    if (isMaxDistance)
                    {
                        if (curDistance > noUnitDistance)
                        {
                            noUnitDistance = curDistance;
                            finalNearNode = item;
                        }
                    }
                    else
                    {
                        if (curDistance < noUnitDistance)
                        {
                            noUnitDistance = curDistance;
                            finalNearNode = item;
                        }
                    }
                }
                else
                {
                    if (isMaxDistance)
                    {
                        if (curDistance > haveUnitDistance)
                        {
                            haveUnitDistance = curDistance;
                            haveUnitNode = item;
                        }
                    }
                    else
                    {
                        if (curDistance < haveUnitDistance)
                        {
                            haveUnitDistance = curDistance;
                            haveUnitNode = item;
                        }
                    }
                }
            }
            if (finalNearNode == null)
            {
                return GetSafeNode(selfUnit, haveUnitNode.Pos);
            }
            else
            {
                return finalNearNode;
            }

            bool IsCondition(Node node)
            {
                if (noObstacle)
                {
                    if (Ins.IsObstacle(node.Index))
                        return false;
                }
                if (noUnit)
                {
                    if (IsHaveUnit(node))
                        return false;
                }
                if (isCanTraversal)
                {
                    if (!selfUnit.Move2DMgr.Traversal2D.Filter(node))
                        return false;
                }
                return true;
            }
        }
        #endregion

        #region get range node
        static Queue<Node> BFSQueue = new Queue<Node>();
        static Dictionary<Node, int> BFSMap = new Dictionary<Node, int>();
        static HashList<Node> BFSResult = new HashList<Node>();
        static Dictionary<Node, GraphNodeRange2D> BFSRange = new Dictionary<Node, GraphNodeRange2D>();
        public HashList<Node> GetDistanceRange(Vector3 pos, float distance, Func<Node, bool> filter = null)
        {
            BFSQueue.Clear();
            BFSResult.Clear();
            BFSRange.Clear();
            Node seed = GetNode(pos);
            Action<GraphNodeRange2D, Node> callback = (parent, node) =>
            {
                if (IsWalkable(node) && !BFSResult.Contains(node))
                {
                    if (filter != null && !filter(node)) return;
                    var nRangeItem = new GraphNodeRange2D(parent, node);
                    if (nRangeItem.Distance > distance) return;

                    BFSResult.Add(node);
                    BFSQueue.Enqueue(node);
                    BFSRange.Add(node, nRangeItem);
                }
            };

            callback(null, seed);

            while (BFSQueue.Count > 0)
            {
                Node n = BFSQueue.Dequeue();
                GraphNodeRange2D nRangeItem = BFSRange[n];
                if (nRangeItem.Distance > distance) break;
                n.GetConnections((x) =>
                {
                    callback(nRangeItem, x);
                });
            }
            return BFSResult;
        }
        public HashList<Node> GetBFS(Vector3 pos, int depth, Func<Node, bool> filter = null, bool isJump = false)
        {
            BFSQueue.Clear();
            BFSMap.Clear();
            BFSResult.Clear();
            Node seed = GetNode(pos);
            if (seed == null) return new HashList<Node>();
            var currentDist = -1;
            Action<Node> callback = node =>
            {
                if (IsWalkable(node) && !BFSResult.Contains(node))
                {
                    var curTempDist = currentDist;
                    if (isJump)
                    {

                        if (filter != null && !filter(node)) { }
                        else curTempDist = currentDist + 1;
                    }
                    else
                    {
                        if (filter != null && !filter(node)) return;
                        else curTempDist = currentDist + 1;
                    }
                    BFSMap.Add(node, curTempDist);
                    BFSResult.Add(node);
                    BFSQueue.Enqueue(node);
                }
            };

            callback(seed);
            while (BFSQueue.Count > 0)
            {
                Node n = BFSQueue.Dequeue();
                currentDist = BFSMap[n];
                if (currentDist >= depth) break;
                n.GetConnections(callback);
            }
            return BFSResult;
        }
        #endregion

        #region is 
        public bool IsHaveNode(Vector2 pos)
        {
            return GetNode(pos) != null;
        }
        public bool IsHaveUnit(Vector2 pos)
        {
            var node = GetNode(pos);
            return IsHaveUnit(node);
        }
        public bool IsHaveUnit(IBaseNode node)
        {
            return SeizeNodeData.IsHaveUnit(node);
        }
        public bool IsWalkable(Node node)
        {
            return node.IsWalkable && !IsObstacle(node);
        }
        public bool IsObstacle(Node node)
        {
            return Ins.IsObstacle(node.Index);
        }
        public bool IsConnection(Node source, Node target)
        {
            var nodes = Ins.GetConnection(source);
            return nodes.Contains(target);
        }
        #endregion

        #region Callback
        protected override void OnSubBattleLoad()
        {
            base.OnSubBattleLoad();
            ClearAllData();
        }
        protected override void OnSubBattleLoaded()
        {
            base.OnSubBattleLoaded();
            Ins.Callback_OnNodeHover += OnNodeHover;
        }
        protected override void OnBattleUnLoad()
        {
            base.OnBattleUnLoad();
            ClearAllData();
        }
        protected override void OnSubBattleUnLoad()
        {
            base.OnSubBattleUnLoad();
            ClearAllData();
            Ins.Callback_OnNodeHover -= OnNodeHover;
        }
        private void OnNodeHover(Node node)
        {
        }
        #endregion

    }
}