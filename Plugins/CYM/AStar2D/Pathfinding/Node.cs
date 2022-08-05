//------------------------------------------------------------------------------
// BaseTitle2D.cs
// Copyright 2020 2020/1/17 
// Created by CYM on 2020/1/17
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using UnityEngine;

namespace CYM.AStar2D
{
    public class Node : BaseMono, IBaseNode
    {
        AStarGrid Ins => AStarGrid.DefaultGrid;

        public delegate void NodeSelectedDelegate(Node node, int mouseButton);
        public delegate void NodeHoverDelegate(Node node);

        // Events
        public event NodeSelectedDelegate OnNodeSelected;
        public event NodeHoverDelegate OnNodeHover;
        public event NodeHoverDelegate OnNodeExit;

        // Private
        [SerializeField]
        private float weighting = 0;
        [SerializeField]
        private bool walkable = true;

        // Public
        public Index Index { get; set; } = new Index();
        public PathNodeDiagonalMode diagonalMode = PathNodeDiagonalMode.UseGlobal;

        // Properties
        public bool IsWalkable
        {
            get { return walkable; } // Only need to implement the get but set is useful
            set { walkable = value; }
        }
        public float Weighting
        {
            get { return weighting; }
            set { weighting = Mathf.Clamp01(value); }
        }
        public PathNodeDiagonalMode DiagonalMode => diagonalMode;
        IBaseNode IBaseNode.TileNode => throw new NotImplementedException();

        int IBaseNode.TileNodeIndex { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        protected virtual void OnMouseEnter()
        {
            OnNodeHover?.Invoke(this);
        }
        protected virtual void OnMouseExit()
        {
            OnNodeExit?.Invoke(this);
        }
        protected virtual void OnMouseOver()
        {
            // Check for mouse button
            if (Input.GetMouseButtonDown(0) == true)
            {
                OnNodeSelected?.Invoke(this, 0);
            }
            else if (Input.GetMouseButtonDown(1) == true)
            {
                OnNodeSelected?.Invoke(this, 1);
            }
        }
        public void ToggleWalkable()
        {
            walkable = !walkable;
            UpdateTileColor();
        }
        public void UpdateTileColor()
        {
            SpriteRenderer renderer = GetComponent<SpriteRenderer>();
            if (renderer == null)
                return;
            if (IsWalkable == true)
            {
                Color targetColour = Color.Lerp(Color.white, Color.yellow, weighting);
                renderer.color = targetColour;
            }
            else
            {
                renderer.color = Color.red;
            }
        }

        public void GetConnections(Action<Node> action)
        {
            var nodes = Ins.GetConnection(this);
            foreach (var item in nodes)
                action(item);
        }

        public bool IsShowInDebugView()
        {
            return true;
        }
    }
}