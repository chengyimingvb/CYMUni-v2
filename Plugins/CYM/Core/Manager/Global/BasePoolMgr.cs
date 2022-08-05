using CYM.Pool;
using System.Collections.Generic;
using UnityEngine;

//**********************************************
// Class Name	: CYMPoolManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM
{
    public class PoolItem
    {
        private SpawnPool spawn;
        public string Name { get; private set; }
        public PoolItem(string name)
        {
            Name = name;
            BasePoolMgr.PoolItems.Add(this);
        }

        public GameObject Spawn(GameObject go, Vector3? position = null, Quaternion? quaternion = null, Transform paraent = null)=> spawn?.Spawn(go, position, quaternion, paraent);
        public Transform SpawnTrans(GameObject go, Vector3? position = null, Quaternion? quaternion = null, Transform paraent = null)=> spawn?.SpawnTrans(go, position, quaternion, paraent);
        public void Despawn(BaseMono mono, float delayTime = 0.0f)=> spawn?.Despawn(mono, delayTime);
        public void Despawn(GameObject mono, float delayTime = 0.0f)=> spawn?.Despawn(mono, delayTime);
        public void DoCreate()=> spawn = PoolManager.Create(Name);
        public void DoDestroy()
        {
            if(spawn.gameObject!=null)
                GameObject.Destroy(spawn.gameObject);
            spawn = null;
        }
    }
    public class BasePoolMgr : BaseGFlowMgr
    {
        #region member variable
        public static List<PoolItem> PoolItems { get; private set; } = new List<PoolItem>();
        public PoolItem Common { get; private set; } = new PoolItem(nameof(Common));
        public PoolItem Unit { get; private set; } = new PoolItem(nameof(Unit));
        public PoolItem Perform { get; private set; } = new PoolItem(nameof(Perform));
        public PoolItem HUD { get; private set; } = new PoolItem(nameof(HUD));
        public PoolItem JumpText { get; private set; } = new PoolItem(nameof(JumpText));
        public PoolItem Scene { get; private set; } = new PoolItem(nameof(Scene));
        #endregion

        #region methon
        public void CreatePool()
        {
            foreach (var item in PoolItems)
                item.DoCreate();
        }
        public void DestroyPool()
        {
            PoolManager.DestroyAll();
            foreach (var item in PoolItems)
                item.DoDestroy();
        }
        public GameObject Spawn(string name)
        {
            return Common.Spawn(GRMgr.Prefab.Get(name));
        }
        public void Despawn(BaseMono go)
        {
            Common.Despawn(go);
        }
        public GameObject SpawnFX(string name)
        {
            return Common.Spawn(GRMgr.Perfom.Get(name));
        }
        public void DespawnFX(BaseMono go)
        {
            Common.Despawn(go);
        }
        #endregion

        #region Callback
        protected override void OnBattleLoad()
        {
            base.OnBattleLoad();
            CreatePool();
        }
        protected override void OnBattleUnLoaded()
        {
            base.OnBattleUnLoaded();
            DestroyPool();
        }
        #endregion
    }

}

