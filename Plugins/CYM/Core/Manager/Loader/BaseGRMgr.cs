//**********************************************
// Class Name	: LoaderManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

using CYM.DLC;
using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Video;

namespace CYM
{
    public class BundleCacher<T>:IBundleCacher where T : UnityEngine.Object
    {
        public string Bundle { get; private set; }
        public ObjectRegister<T> Data { get; private set; } = new ObjectRegister<T>();
        public BundleCacher(string bundleName)
        {
            Bundle = bundleName;
            BaseGRMgr.BundleCachers.Add(bundleName,this);
        }

        public bool IsHave(string name)
        {
            if (DLCManager.IsHave(Bundle, name))
                return true;
            return false;
        }

        public T Get(string name,bool isLogError=true)
        {
            return GetResWithCache(Bundle, name, Data, isLogError);
        }
        public T Spawn(string name)
        {
            return GameObject.Instantiate<T>(Get(name));
        }

        public List<T> Get(List<string> names)
        {
            List<T> clips = new List<T>();
            if (names == null)
                return clips;
            for (int i = 0; i < names.Count; ++i)
            {
                if (names[i].IsInv())
                    continue;
                var temp = Get(names[i]);
                if (temp != null)
                    clips.Add(temp);
            }
            return clips;
        }
        public List<T> Get()
        {
            List<T> clips = new List<T>();
            foreach (var item in DLCManager.GetAssetsByBundle(Bundle))
            {
                var temp = Get(item);
                if (temp != null)
                    clips.Add(temp);
            }
            return clips;
        }
        public List<string> GetStrsByCategory(string category)
        {
            return DLCManager.GetAssetsByCategory(Bundle, category);
        }
        public List<string> GetStrs()
        {
            return DLCManager.GetAssetsByBundle(Bundle);
        }
        public List<T> GetByCategory(string category)
        {
            List<T> clips = new List<T>();
            foreach (var item in DLCManager.GetAssetsByCategory(Bundle, category))
            {
                var temp = Get(item);
                if (temp != null)
                    clips.Add(temp);
            }
            return clips;
        }
        private T GetResWithCache(string bundle, string name, ObjectRegister<T> cache, bool logError = true)
        {
            if (name.IsInv())
                return null;
            if (
                BaseGlobal.LoaderMgr!=null &&
                !BaseGlobal.LoaderMgr.IsLoadEnd
                )
                return null;
            if (cache.ContainsKey(name))
            {
                return cache[name];
            }
            else
            {
                DLCManager.IsNextLogError = logError;
                var asset = DLCManager.LoadAsset<T>(bundle, name);
                if (asset == null)
                    return null;
                T retGO = asset.Object as T;
                if (retGO == null)
                {
                    if (logError)
                        CLog.Error("no this res in bundle {0}, name = {1}", bundle, name);
                }
                else
                {
                    if (!cache.ContainsKey(retGO.name)) cache.Add(retGO);
                    else cache[retGO.name] = retGO;
                }
                return retGO;
            }
        }

        public void RemoveNull()
        {
            Data.RemoveNull();
        }
    }
    /// <summary>
    /// $LocalPlayer表示动态ID
    /// </summary>
    public class BaseGRMgr : BaseGFlowMgr, ILoader
    {
        #region member variable
        private DLCConfig DLCConfig => DLCConfig.Ins;
        public static Dictionary<string, IBundleCacher> BundleCachers { get; private set; } = new Dictionary<string, IBundleCacher>();
        #endregion

        #region bundle
        public BundleCacher<Material> Material { get; private set; } = new BundleCacher<Material>(Const.BN_Materials);
        public BundleCacher<GameObject> Prefab { get; private set; } = new BundleCacher<GameObject>(Const.BN_Prefab);
        public BundleCacher<GameObject> System { get; private set; } = new BundleCacher<GameObject>(Const.BN_System);
        public BundleCacher<GameObject> Perfom { get; private set; } = new BundleCacher<GameObject>(Const.BN_Perform);
        public BundleCacher<Sprite> Icon { get; private set; } = new BundleCacher<Sprite>(Const.BN_Icon);
        public BundleCacher<Sprite> Sprite { get; private set; } = new BundleCacher<Sprite>(Const.BN_Sprite);
        public BundleCacher<Sprite> Head { get; private set; } = new BundleCacher<Sprite>(Const.BN_Head);
        public BundleCacher<Sprite> Flag { get; private set; } = new BundleCacher<Sprite>(Const.BN_Flag);
        public BundleCacher<AudioClip> Audio { get; private set; } = new BundleCacher<AudioClip>(Const.BN_Audio);
        public BundleCacher<AudioClip> Music { get; private set; } = new BundleCacher<AudioClip>(Const.BN_Music);
        public BundleCacher<GameObject> UI { get; private set; } = new BundleCacher<GameObject>(Const.BN_UI);
        public BundleCacher<VideoClip> Video { get; private set; } = new BundleCacher<VideoClip>(Const.BN_Video);
        public BundleCacher<AudioMixer> AudioMixer { get; private set; } = new BundleCacher<AudioMixer>(Const.BN_AudioMixer);
        public BundleCacher<RuntimeAnimatorController> Animator { get; private set; } = new BundleCacher<RuntimeAnimatorController>(Const.BN_Animator);
        public BundleCacher<PhysicMaterial> PhysicMaterial { get; private set; } = new BundleCacher<PhysicMaterial>(Const.BN_PhysicMaterial);
        public BundleCacher<Sprite> BG { get; private set; } = new BundleCacher<Sprite>(Const.BN_BG);
        public BundleCacher<Texture2D> Texture { get; private set; } = new BundleCacher<Texture2D>(Const.BN_Texture);
        public BundleCacher<Sprite> Illustration { get; private set; } = new BundleCacher<Sprite>(Const.BN_Illustration);
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            Util.CreateGlobalObj<DLCManager>("DLCAssetMgr");
        }
        public override void OnEnable()
        {
            base.OnEnable();
            OnAddDynamicRes();
        }
        public override void OnDestroy()
        {
            base.OnDestroy();

        }
        protected virtual void OnAddDynamicRes() { }
        #endregion

        #region Set
        //public void UnLoadLoadedAssetBundle()
        //{
        //    AssetBundle.UnloadAllAssetBundles(false);
        //    Resources.UnloadUnusedAssets();
        //    BaseGlobal.GCCollect();
        //}
        //public virtual void UnLoadBattleAssetBundle()
        //{
        //    BaseGlobal.GCCollect();
        //    Resources.UnloadUnusedAssets();
        //    BaseGlobal.GCCollect();
        //}
        #endregion

        #region scene
        public virtual Asset LoadScene(string bundleName)=> DLCManager.LoadScene(bundleName);
        public virtual void UnloadScene(Asset asset)=> DLCManager.UnloadScene(asset);
        #endregion

        #region load
        public virtual T LoadAsset<T>(string bundleName, string assetName,out Asset asset) 
            where T : UnityEngine.Object
        {
            asset = DLCManager.LoadAsset<T>(bundleName, assetName);
            if (asset == null) return null;
            return asset.Object as T;
        }
        public virtual Asset LoadAssetAsync<T>(string bundleName, string assetName)=> DLCManager.LoadAssetAsync<T>(bundleName, assetName);
        public virtual void UnloadAsset(Asset asset, bool all)=> DLCManager.UnloadAsset(asset, all);
        #endregion

        #region get
        public GameObject GetResources(string path, bool instance = false)
        {
            var temp = Resources.Load<GameObject>(path);
            if (temp == null)
            {
                CLog.Error("错误,没有这个对象:"+ path);
                return null;
            }
            if (instance)
            {
                var ret = GameObject.Instantiate(temp);
                ret.transform.SetParent(SelfBaseGlobal.Trans);
                return ret;
            }
            else return temp;
        }
        public T GetResources<T>(string path) where T: UnityEngine.Object
        {
            var temp = Resources.Load<T>(path);
            if (temp == null)
            {
                CLog.Error("错误,没有这个对象:" + path);
                return null;
            }
            return temp;
        }
        #endregion

        #region Callback
        protected override void OnBattleLoaded()
        {
            base.OnBattleLoaded();
            //GlobalMonoMgr.RemoveAllNull();
            //UnLoadBattleAssetBundle();
        }
        protected override void OnBattleUnLoaded()
        {
            base.OnBattleUnLoaded();
            Resources.UnloadUnusedAssets();
            GlobalMonoMgr.RemoveAllNull();
            BundleCachers.ForEach(x => x.Value.RemoveNull());
            BaseGlobal.GCCollect();
        }
        protected override void OnSubBattleUnLoaded()
        {
            base.OnSubBattleUnLoaded();
            //UnLoadBattleAssetBundle();
        }
        public IEnumerator Load()
        {
            foreach (var item in GetDLCItemConfigs())
            {
                DLCManager.LoadDLC(item);
                yield return new WaitForEndOfFrame();
            }

            //初始化的加载所有Bundle
            if (!BuildConfig.Ins.IsEditorMode)
            {
                if (BuildConfig.Ins.IsInitLoadSharedBundle)
                { 
                    foreach(var bundle in DLCManager.LoadAllSharedBundle())
                        while (!bundle.IsDone)
                            yield return new WaitForEndOfFrame();
                }
                if (BuildConfig.Ins.IsInitLoadDirBundle)
                {
                    foreach (var bundle in DLCManager.LoadAllDirBundle())
                        while (!bundle.IsDone)
                            yield return new WaitForEndOfFrame();
                }
            }

            if(BuildConfig.Ins.IsWarmupAllShaders)
                Shader.WarmupAllShaders();
            yield break;

            // 获得DLC的根目录
            List<DLCItemConfig> GetDLCItemConfigs()
            {
                if (BuildConfig.Ins.IsEditorOrAssetBundleMode)
                {
                    return DLCConfig.ConfigAll;
                }
                else
                {
                    List<DLCItemConfig> ret = new List<DLCItemConfig>();
                    string[] files = Directory.GetFiles(Const.Path_StreamingAssets, Const.STR_DLCItem + ".json", SearchOption.AllDirectories);
                    foreach (var item in files)
                    {
                        var dlcJson = FileUtil.LoadJson<DLCItemConfig>(item);
                        if(dlcJson.IsActive)
                            ret.Add(dlcJson);
                    }
                    return ret;
                }
            }
        }
        public string GetLoadInfo()
        {
            return "Load Resources";
        }
        #endregion

        #region 语法糖
        public Material FontRendering =>Material.Get("FontRendering");
        public Material ImageGrey => Material.Get("ImageGrey");
        #endregion
    }

}