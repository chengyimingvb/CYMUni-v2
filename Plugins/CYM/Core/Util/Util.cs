using Invoke;
using Sirenix.Utilities;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace CYM
{
    public partial class Util:BaseUtil
    {
        #region get
        public static string GetStr(string key, params object[] ps)
        {
            return BaseLangMgr.Get(key, ps);
        }
        public static string Joint(string key,params object[] ps)
        {
            return BaseLangMgr.Joint(key,ps);
        }
        public static float GetUpFactionVal(IUpFactionData an, float? inputVal)
        {
            float newInputVal = 0;
            if (inputVal == null) newInputVal = an.InputVal;
            else newInputVal = inputVal.Value;
            //计算最终的InputVal
            var realInputVal = newInputVal - an.InputValStart;
            //百分比相乘
            if (an.FactionType == UpFactionType.Percent) return an.Val * realInputVal * an.Faction + an.Add;
            //百分比累加
            else if (an.FactionType == UpFactionType.PercentAdd) return an.Val * (1 + realInputVal * an.Faction) + an.Add;
            //线性累加
            else if (an.FactionType == UpFactionType.LinerAdd) return an.Val + realInputVal * an.Faction + an.Add;
            //指数累加
            else if (an.FactionType == UpFactionType.PowAdd) return an.Val + FormulaUtil.Pow(realInputVal, an.Faction) + an.Add;
            throw new Exception("错误的增长类型");
        }
        public static BaseUnit GetPlayer(params BaseUnit[] units)
        {
            foreach (var item in units)
            {
                if (item.IsPlayer())
                    return item;
            }
            return null;
        }
        public static Transform GetTrans(Vector3 pos)
        {
            BaseGlobal.TempTrans.position = pos;
            return BaseGlobal.TempTrans;
        }
        #endregion

        #region other
        public static T CreateGlobalObj<T>(string name) where T : MonoBehaviour
        {
            if (BaseGlobal.Ins == null)
            {
                CLog.Error("CreateGlobalObj:错误!Global还未初始化!!!");
                return null;
            }
            var go = new GameObject(name);
            go.transform.SetParent(BaseGlobal.Ins.Trans);
            T instance = go.AddComponent<T>();
            return instance;
        }
        public static T CreateGlobalResourceObj<T>(string name) where T : MonoBehaviour
        {
            if (BaseGlobal.Ins == null)
            {
                CLog.Error("CreateGlobalObj:错误!Global还未初始化!!!");
                return null;
            }
            var prefab = Resources.Load(name);
            if (prefab == null)
            {
                CLog.Error("CreateGlobalObj:错误:"+ name);
                return null;
            }
            var go = GameObject.Instantiate(prefab) as GameObject;
            go.transform.SetParent(BaseGlobal.Ins.Trans);
            T instance = go.GetComponent<T>();
            return instance;
        }
        public static GameObject CreateGlobalResourceGameObject(string name)
        {
            if (BaseGlobal.Ins == null)
            {
                CLog.Error("CreateGlobalResourceGameObject:错误!Global还未初始化!!!");
                return null;
            }
            var prefab = Resources.Load(name);
            if (prefab == null)
            {
                CLog.Error("CreateGlobalResourceGameObject:错误:" + name);
                return null;
            }
            var go = GameObject.Instantiate(prefab) as GameObject;
            go.transform.SetParent(BaseGlobal.Ins.Trans);
            return go;
        }
        // 计算平均位置
        public static Vector3 AveragePos<T>(List<T> units) where T : BaseUnit
        {
            if (units == null || units.Count == 0)
                return Const.VEC_FarawayPos;
            Vector3 sum = new Vector3();
            foreach (var unit in units)
            {
                sum += unit.Pos;
            }
            return sum / units.Count;
        }
        #endregion

        #region is
        //检测组件和子组件是否有冲突
        public static bool IsFit(BaseMgr main, BaseMgr sub)
        {
            if (main.MgrType == MgrType.All || sub.MgrType == MgrType.All)
                return true;
            return main.MgrType == sub.MgrType;
        }
        //检测组件和Mono是否有冲突
        public static bool IsFit(BaseCoreMono main, BaseMgr com)
        {
            if (main.MonoType == MonoType.None ||
                main.MonoType == MonoType.Normal)
                return false;

            if (com.MgrType == MgrType.All)
                return true;

            if (main.MonoType == MonoType.Global ||
                main.MonoType == MonoType.View)
            {
                if (com.MgrType == MgrType.Global)
                    return true;
                return false;
            }

            if (main.MonoType == MonoType.Unit)
            {
                if (com.MgrType == MgrType.Unit)
                    return true;
                return false;
            }

            return false;
        }
        public static bool IsAnyPlayer(params BaseUnit[] unit)
        {
            foreach (var item in unit)
            {
                if (item == null)
                    continue;
                if (item.IsPlayer())
                    return true;
            }
            return false;
        }
        public static bool IsAllPlayer(params BaseUnit[] unit)
        {
            foreach (var item in unit)
            {
                if (item == null)
                    continue;
                if (item.IsPlayer())
                    return false;
            }
            return true;
        }
        #endregion

        #region invoke
        public static IJob Invoke(Action action, float delay=0.5f)
        {
            return SuperInvoke.Run(action, delay);
        }
        #endregion

        #region DB
        public static void CopyToDB<TConfig, TData>(TConfig config, TData data, string tdid, long rtid) where TData : DBBase
        {
            CopyToDB(config, data);
            data.ID = rtid;
            data.TDID = tdid;
        }
        // 将配置数据复制到DB里,通过反射,减少操作,字段必须包含AttrCopyData特性
        // Config对象只能使用属性来映射
        // DBData对象可以使用属性或者字段来映射
        public static void CopyToDB<TConfig, TData>(TConfig config, TData data)
        {
            if (config == null || data == null) return;
            var cType = config.GetType();
            var dType = data.GetType();
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            //设置属性
            foreach (var dAttr in dType.GetMembers(flag))
            {
                var cAttr = cType.GetProperty(dAttr.Name, flag);
                if (cAttr != null)
                {
                    if (cAttr.GetReturnType().Name != dAttr.GetReturnType().Name)
                    {
                        CLog.Error("CopyToDB错误!名称相同但是类型不同,{0},{1}--{2}", dAttr.Name, data.ToString(), config.ToString());
                        continue;
                    }
                    dAttr.SetMemberValue(data, cAttr.GetValue(config));
                }
            }
        }
        // Config对象只能使用属性来映射
        // DBData对象可以使用属性或者字段来映射
        public static void CopyToTD<TData, TConfig>(TData data, TConfig config)
        {
            if (config == null || data == null) return;
            var cType = config.GetType();
            var dType = data.GetType();
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            //设置属性
            foreach (var dAttr in dType.GetMembers(flag))
            {
                var cAttr = cType.GetProperty(dAttr.Name, flag);
                if (cAttr != null)
                {
                    if (cAttr.GetReturnType().Name != dAttr.GetReturnType().Name)
                    {
                        CLog.Error("CopyToTD错误!名称相同但是类型不同,{0},{1}--{2}", dAttr.Name, data.ToString(), config.ToString());
                        continue;
                    }
                    cAttr.SetValue(config, dAttr.GetMemberValue(data));
                }
            }
        }
        #endregion

        #region ray cast
        public static bool ScreenRayCast(out RaycastHit hit, LayerMask layer)
        {
            return ScreenRayCast(out hit, BaseInputMgr.ScreenPos, layer);
        }
        public static (Collider col, Collider2D col2D) PickCollider(LayerMask layer)
        {
            return PickCollider(BaseInputMgr.ScreenPos, layer);
        }
        public static Component PickColliderCom(LayerMask layer)
        {
            return PickColliderCom(BaseInputMgr.ScreenPos, layer);
        }
        public static Vector3 GetHitTerrinPoint()
        {
            Util.ScreenRayCast(out RaycastHit hit,BaseInputMgr.ScreenPos, (LayerMask)Const.Layer_Terrain);
            return hit.point;
        }
        #endregion

        #region misc
        public static string GetPlatformName()
        {
#if UNITY_STANDALONE_OSX
			return "OSX";
#elif UNITY_STANDALONE_WIN
			return "WIN";
#elif UNITY_STANDALONE_LINUX
			return "LINUX";
#elif UNITY_STANDALONE
			return "STANDALONE";
#elif UNITY_WII
			return "WII";
#elif UNITY_IOS
			return "IOS";
#elif UNITY_IPHONE
			return "IPHONE";
#elif UNITY_ANDROID
            return "ANDROID";
#elif UNITY_PS3
			return "PS3";
#elif UNITY_PS4
			return "PS4";
#elif UNITY_SAMSUNGTV
			return "SAMSUNGTV";
#elif UNITY_XBOX360
			return "XBOX360";
#elif UNITY_XBOXONE
			return "XBOXONE";
#elif UNITY_TIZEN
			return "TIZEN";
#elif UNITY_TVOS
			return "TVOS";
#elif UNITY_WP_8_1
			return "WP_8_1";
#elif UNITY_WSA_10_0
			return "WSA_10_0";
#elif UNITY_WSA_8_1
			return "WSA_8_1";
#elif UNITY_WSA
			return "WSA";
#elif UNITY_WINRT_10_0
			return "WINRT_10_0";
#elif UNITY_WINRT_8_1
			return "WINRT_8_1";
#elif UNITY_WINRT
			return "WINRT";
#elif UNITY_WEBGL
			return "WEBGL";
#else
            return "UNKNOWNHW";
#endif
        }
        public static void UnifyChildName(GameObject GO)
        {
            if (GO == null)
                return;
            for (int i = 0; i < GO.transform.childCount; ++i)
            {
                if (GO.transform.GetChild(i).GetComponent<LayoutElement>() != null)
                    continue;
                GO.transform.GetChild(i).name = "Item" + i;
            }
        }
        public static T GetSetting<T>(GameObject go) where T : PostProcessEffectSettings
        {
            var postProcessVolume = go.GetComponentInChildren<PostProcessVolume>();
            if (postProcessVolume == null)
                return null;
            T ret = default;
            if (postProcessVolume && postProcessVolume.profile)
                postProcessVolume.profile.TryGetSettings(out ret);
            return ret;
        }
        #endregion

        #region mail
        void OpenEmail(string toEmail, string emailSubject, string emailBody)
        {
            emailSubject = System.Uri.EscapeUriString(emailSubject);
            emailBody = System.Uri.EscapeUriString(emailSubject);
            Application.OpenURL("mailto:" + toEmail + "?subject=" + emailSubject + "&body=" + emailBody);
        }
        #endregion

        #region DB
        public static void CopyToData(TDBaseData config, DBBase data)
        {
            data.CustomName = config.CustomName;
            data.ID = config.ID;
            data.TDID = config.TDID;
        }
        public static void CopyToConfig(DBBase data, TDBaseData config)
        {
            config.CustomName = data.CustomName;
            config.ID = data.ID;
            config.TDID = data.TDID;
        }
        #endregion
    }

}