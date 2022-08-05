//------------------------------------------------------------------------------
// BaseSLGSceneObj.cs
// Copyright 2019 2019/3/29 
// Created by CYM on 2019/3/29
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.AI.NodesMap;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace CYM
{
    [Serializable]
    public class CastleDic : Dictionary<string, Transform>
    {

    }
    [ExecuteInEditMode]
    public class BaseSLGSceneRoot<T> : BaseSceneRoot where T : BaseUnit
    {
        #region Inspector
        [FoldoutGroup("SLG"), HideInInspector]
        public CastleDic CastlesPoint = new CastleDic();
        #endregion

        #region prop
        public new static BaseSLGSceneRoot<T> Ins { get; private set; }
        public Dictionary<string, T> Castles { get; private set; } = new Dictionary<string, T>();
        #endregion

        #region life
        public override void Awake()
        {
            base.Awake();
            Ins = this;
            Pos = Vector3.zero;
            Trans.hideFlags = HideFlags.HideInInspector;
        }
        protected override void Parse()
        {
            base.Parse();
            OnParseUnitStart();
            if (NodeMap.Ins != null)
            {
                ForEachUnits(x=>OnParseUnit(x));
                NodeMap.Ins.Refresh();
            }
            OnParseUnitEnd();
        }
        protected virtual void OnParseUnitStart()
        { 
        
        }
        protected virtual void OnParseUnitEnd()
        { 
        
        }
        protected void ForEachUnits(Callback<T> callback)
        {
            CastlesPoint.Clear();
            Castles.Clear();
            var temps = NodeMap.Ins.GetComponentsInChildren<T>();
            foreach (var item in temps)
            {
                if (item == NodeMap.Ins) continue;
                if (item.name.StartsWith("#")) continue;
                if (item != null)
                {
                    callback?.Invoke(item);
                    Transform tempTrans = item.transform;
                    CastlesPoint.Add(tempTrans.name, tempTrans);
                }
                Castles.Add(item.Trans.name, item);
            }
        }
        #endregion

        #region get
        public T GetCastle(string tdid)
        {
            string id = tdid.TrimStart(Const.Prefix_Castle);
            if (Castles.ContainsKey(id))
                return Castles[id];
            else
            {
                CLog.Error("场景里没有这个城市:{0}",tdid);
                return null;
            }
        }
        #endregion

        #region virtual
        protected virtual void OnParseUnit(T unit)
        {

        }
        #endregion

        #region inspector
        [Button(nameof(ExortCastleMap))]
        public void ExortCastleMap()
        {
            Texture2D texture = new Texture2D(1024, 1024);

            for (int i = 0; i < texture.width; ++i)
            {
                for (int j = 0; j < texture.height; ++j)
                {
                    texture.SetPixel(i, j, Color.gray);
                }
            }
            int size = 5;
            foreach (var item in CastlesPoint)
            {
                int x = Mathf.CeilToInt(item.Value.position.x);
                int y = Mathf.CeilToInt(item.Value.position.z);
                texture.SetPixel(x, y, Color.red);
                for (int up = 0; up < size; ++up)
                    texture.SetPixel(x, y + up, Color.red);
                for (int down = 0; down < size; ++down)
                    texture.SetPixel(x, y - down, Color.red);
                for (int left = 0; left < size; ++left)
                    texture.SetPixel(x - left, y, Color.red);
                for (int right = 0; right < size; ++right)
                    texture.SetPixel(x + right, y, Color.red);
            }
            texture.Apply();            
            FileUtil.SaveTextureToPNG(texture, Path.Combine(Const.Path_ResourcesTemp, "CastleMapPoint.png"));
        }

        [Button(nameof(ExortPosData))]
        void ExortPosData()
        {
            PosData posData = new PosData();
            foreach (var item in CastlesPoint)
            {
                posData.Datas.Add(new PosItem() { ID = item.Key, Pos = item.Value.position.ToVec3() });
            }
            FileUtil.SaveJson(Path.Combine(Const.Path_Resources, "CastlePosData.json"), posData);
        }
        #endregion
    }
}