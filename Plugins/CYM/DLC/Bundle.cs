using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CYM.DLC
{
    public class Bundle
    {
        public int References { get; private set; }
        public virtual string Error { get; protected set; }
        public virtual float Progress { get { return 1; } }
        public virtual bool IsDone { get { return true; } }
        public virtual AssetBundle AssetBundle { get { return assetBundle; } }
        public readonly HashList<Bundle> Dependencies = new HashList<Bundle>();
        //防止Asset bundle产生循环依赖的巧妙设计
        public Bundle Parent { get; set; }
        public string Path { get; protected set; }
        public string Name { get; internal set; }
        AssetBundle assetBundle;

        internal Bundle(string url)
        {
            Path = url;
        }

        internal void Load()
        {
            OnLoad();
        }

        internal void Unload(bool all)
        {
            OnUnload(all);
        }

        public void AddDependencies(Bundle subBundle)
        {
            subBundle.Parent = this;
            Dependencies.Add(subBundle);
        }

        public T LoadAsset<T>(string assetName) where T : Object
        {
            if (Error != null)
            {
                return null;
            }
            return AssetBundle.LoadAsset(assetName, typeof(T)) as T;
        }

        public Object LoadAsset(string assetName, System.Type assetType)
        {
            if (Error != null)
            {
                return null;
            }
            return AssetBundle.LoadAsset(assetName, assetType);
        }

        public T[] LoadAllAssets<T>() where T : Object
        {
            if (Error != null)
            {
                return null;
            }
            return AssetBundle.LoadAllAssets<T>();
        }

        public AssetBundleRequest LoadAssetAsync(string assetName, System.Type assetType)
        {
            if (Error != null)
            {
                return null;
            }
            return AssetBundle.LoadAssetAsync(assetName, assetType);
        }

        /// <summary>
        /// 不要手动调用此函数
        /// </summary>
        internal void Retain()
        {
            References++;
        }
        /// <summary>
        /// 不要手动调用此函数
        /// </summary>
        internal void Release()
        {
            if (--References < 0)
            {
                CLog.Error("refCount < 0");
            }
        }

        protected virtual void OnLoad()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (File.Exists(Path))
#endif
                assetBundle = AssetBundle.LoadFromFile(Path);
            if (assetBundle == null)
            {
                Error = Path + "=> LoadFromFile failed.";
            }
        }

        protected virtual void OnUnload(bool all)
        {
            if (assetBundle != null)
            {
                assetBundle.Unload(all);
                assetBundle = null;
            }
        }
    }

    public class BundleAsync : Bundle, IEnumerator
    {
        #region IEnumerator implementation

        public bool MoveNext()
        {
            return !IsDone;
        }

        public void Reset()
        {
        }

        public object Current
        {
            get
            {
                return AssetBundle;
            }
        }

        #endregion

        public override AssetBundle AssetBundle
        {
            get
            {
                if (Error != null)
                {
                    return null;
                }

                if (Dependencies.Count == 0)
                {
                    return request.assetBundle;
                }

                //for (int i = 0, I = Dependencies.Count; i < I; i++)
                //{
                //    var item = Dependencies[i];
                //    if (item.AssetBundle == null)
                //    {
                //        return null;
                //    }
                //}

                foreach (var item in Dependencies)
                {
                    //if (item.Parent == this)
                    //    continue;
                    if (item.Dependencies.Contains(this))
                        continue;

                    if (item.AssetBundle == null)
                    {
                        return null;
                    }
                }

                return request.assetBundle;
            }
        }

        public override float Progress
        {
            get
            {
                if (Error != null)
                {
                    return 1;
                }

                if (Dependencies.Count == 0)
                {
                    return request.progress;
                }

                //for (int i = 0, I = Dependencies.Count; i < I; i++)
                //{
                //    var item = Dependencies[i];
                //    value += item.Progress;
                //}

                float value = request.progress;
                foreach (var item in Dependencies)
                {
                    //if (item.Parent == this)
                    //    continue;
                    if (item.Dependencies.Contains(this))
                        continue;
                    value += item.Progress;
                }
                return value / (Dependencies.Count + 1);
            }
        }

        public override bool IsDone
        {
            get
            {
                if (Error != null)
                {
                    return true;
                }

                if (Dependencies.Count == 0)
                {
                    return request.isDone;
                }

                foreach (var item in Dependencies)
                {
                    if (item.Error != null)
                    {
                        Error = "Falied to load Dependencies " + item;
                        return true;
                    }
                    if (item.Dependencies.Contains(this))
                        continue;

                    if (!item.IsDone)
                    {
                        return false;
                    }
                }

                //for (int i = 0, I = Dependencies.Count; i < I; i++)
                //{
                //    var item = Dependencies[i];
                //    if (item.Error != null)
                //    {
                //        Error = "Falied to load Dependencies " + item;
                //        return true;
                //    }
                //    if (!item.IsDone)
                //    {
                //        return false;
                //    }
                //}
                return request.isDone;
            }
        }

        AssetBundleCreateRequest request;

        protected override void OnLoad()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            if (File.Exists(Path))
#endif
                request = AssetBundle.LoadFromFileAsync(Path);
            if (request == null)
            {
                Error = Path + "=> LoadFromFileAsync falied.";
            }
        }

        protected override void OnUnload(bool all)
        {
            if (request != null)
            {
                if (request.assetBundle != null)
                {
                    request.assetBundle.Unload(all);
                }
                request = null;
            }
        }

        internal BundleAsync(string url) : base(url)
        {

        }
    }
}
