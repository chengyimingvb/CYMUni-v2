//**********************************************
// Discription	：CYMBaseCoreComponent
// Author	：CYM
// Team		：MoBaGame
// Date		：2015-11-1
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Video;

namespace CYM
{
    public static class ExtensionNormal
    {
        #region prop
        static BaseGRMgr GRMgr => BaseGlobal.GRMgr;
        #endregion

        #region str
        /// <summary>
        /// Wraps a class around a json array so that it can be deserialized by JsonUtility
        /// </summary>
        /// <param name="source"></param>
        /// <param name="topClass"></param>
        /// <returns></returns>
        public static string WrapToClass(this string source, string topClass)
        {
            return string.Format("{{\"{0}\": {1}}}", topClass, source);
        }
        ///<summary>
        /// 移除前缀字符串
        ///</summary>
        ///<param name="val">原字符串</param>
        ///<param name="str">前缀字符串</param>
        ///<returns></returns>
        public static string TrimStart(this string val, string str)
        {
            string strRegex = @"^(" + str + ")";
            return Regex.Replace(val, strRegex, "");
        }
        ///<summary>
        /// 移除后缀字符串
        ///</summary>
        ///<param name="val">原字符串</param>
        ///<param name="str">后缀字符串</param>
        ///<returns></returns>
        public static string TrimEnd(this string val, string str)
        {
            string strRegex = @"(" + str + ")" + "$";
            return Regex.Replace(val, strRegex, "");
        }
        #endregion

        #region cost
        public static float GetVal<T>(this List<Cost<T>> data, T type) where T : Enum
        {
            if (data == null)
            {
                return 0;
            }
            foreach (var item in data)
            {
                if (item.RealVal == 0)
                    continue;
                if (Enum<T>.Int(item.Type) == Enum<T>.Int(type))
                    return item.RealVal;

            }
            return 0;
        }
        public static Cost<T> GetCost<T>(this List<Cost<T>> data, T type) where T : Enum
        {
            if (data == null)
            {
                return null;
            }
            foreach (var item in data)
            {
                if (item.RealVal == 0)
                    continue;
                if (Enum<T>.Int(item.Type) == Enum<T>.Int(type))
                    return item;

            }
            return null;
        }
        public static string GetDescV<T>(this List<Cost<T>> data) where T : Enum
        {
            string retStr = "";
            string costStr = "";
            int index = 0;
            foreach (var item in data)
            {
                if (item.RealVal == 0) continue;
                string temp = item.GetDesc(false, false);
                temp = Const.STR_Indent + temp;
                if (index < data.Count - 1) temp = temp + "\n";
                index++;
                costStr += temp;
            }
            if (costStr != "")
                retStr = BaseLangMgr.Get("AC_消耗", UIUtil.Yellow(costStr));
            return retStr;
        }
        public static string GetDesc<T>(this List<Cost<T>> data, string Separator = "", string EndSeparator = ",", bool isHaveSign = false) where T : Enum
        {
            string temp = "";
            if (data == null)
            {
                CLog.Error("data is null");
            }
            foreach (var item in data)
            {
                if (item.RealVal == 0)
                    continue;
                temp += Separator + item.GetDesc(isHaveSign, false) + EndSeparator;
            }
            return temp.TrimEnd(EndSeparator.ToCharArray());
        }
        #endregion

        #region enum
        /// <summary>
        /// 通过枚举获得翻译的名称
        /// </summary>
        /// <param name="myEnum"></param>
        /// <returns></returns>
        public static string[] GetEnumTransNames(this Type myEnum)
        {
            List<string> ret = new List<string>();
            var array = GetFullEnumArray(myEnum);
            if (array != null)
            {
                foreach (var item in array)
                    ret.Add(BaseLangMgr.Get(item));
            }
            return ret.ToArray();
        }
        /// <summary>
        /// 通过全名获得枚举翻译
        /// </summary>
        /// <returns></returns>
        public static string GetName(this Enum myEnum)
        {
            return BaseLangMgr.Get(GetFull(myEnum));
        }
        /// <summary>
        /// 通过全名获得枚举翻译
        /// </summary>
        /// <returns></returns>
        public static Sprite GetIcon(this Enum myEnum)
        {
            return GRMgr.Icon.Get(GetFull(myEnum));
        }
        /// <summary>
        /// 获取枚举翻译描述
        /// </summary>
        /// <param name="myEnum"></param>
        /// <returns></returns>
        public static string GetDesc(this Enum myEnum, params string[] objs)
        {
            return BaseLangMgr.Get(Const.Prefix_Desc + GetFull(myEnum), objs);
        }
        /// <summary>
        /// 获得枚举的全名
        /// </summary>
        /// <param name="myEnum"></param>
        /// <returns></returns>
        public static string GetFull(this Enum myEnum)
        {
            return string.Format("{0}.{1}", myEnum.GetType().Name, myEnum.ToString());
        }
        /// <summary>
        /// 获得枚举类型的全名数组
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string[] GetFullEnumArray(this Type enumType)
        {
            List<string> list = new List<string>();
            Array temp = Enum.GetValues(enumType);
            foreach (var item in temp)
            {
                list.Add(((Enum)item).GetFull());
            }
            return list.ToArray();
        }

        /// <summary>
        /// 获得枚举类型的全名数组
        /// </summary>
        /// <param name="enumType"></param>
        /// <returns></returns>
        public static string[] GetEnumArray(this Type enumType)
        {
            List<string> list = new List<string>();
            Array temp = Enum.GetValues(enumType);
            foreach (var item in temp)
            {
                list.Add(((Enum)item).GetName());
            }
            return list.ToArray();
        }
        //随意枚举的值,枚举的序号必须从0开始
        public static T Rand<T>(this Type enumType)
        {
            Array temp = Enum.GetValues(enumType);
            return (T)temp.GetValue(RandUtil.RandInt(0, temp.Length));
        }
        public static T GetEnum<T>(this string str) where T : struct
        {
            return (T)Enum.Parse(typeof(T), str);
        }

        #endregion

        #region unit
        /// <summary>
        /// 判断单位是否有效
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public static bool IsInv(this BaseUnit unit)
        {
            if (unit == null) return true;
            if (!unit.IsLive) return true;
            return false;
        }
        public static bool IsValid(this BaseUnit unit)
        {
            return !unit.IsInv();
        }
        #endregion

        #region vector3
        public static Vec3 ToVec3(this Vector3 vector)
        {
            Vec3 vec3S = new Vec3();
            vec3S.Fill(vector);
            return vec3S;
        }
        #endregion

        #region Misc
        public static string DisplayName(this InputAction action, int index = 0)
        {
            if (action == null)
                return Const.STR_Inv;
            if (action.controls.Count <= index)
                return Const.STR_Inv;
            return action.controls[index].displayName;
        }

        public static string GetName(this string str, params object[] param)
        {
            return BaseLangMgr.Get(str, param);
        }
        public static string GetDesc(this string str, params object[] param)
        {
            return BaseLangMgr.Get(Const.Prefix_Desc + str, param);
        }

        public static VideoClip GetVideoClip(this string str)
        {
            return GRMgr.Video.Get(str);
        }
        public static Sprite GetBG(this string str, bool isLogError = true)
        {
            return GRMgr.BG.Get(str, isLogError);
        }
        public static Sprite GetIcon(this string str, bool isLogError = true)
        {
            return GRMgr.Icon.Get(str, isLogError);
        }
        public static Sprite GetIllustration(this string str, bool isLogError = true)
        {
            return GRMgr.Illustration.Get(str, isLogError);
        }
        public static Sprite GetFlag(this string str, bool isLogError = true)
        {
            return GRMgr.Flag.Get(str, isLogError);
        }
        public static Sprite GetIconOrFlag(this string str, bool isLogError = false)
        {
            Sprite ret = str.GetIcon(isLogError);
            if (ret == null)
                return str.GetFlag(isLogError);
            return ret;
        }
        public static Sprite GetSprite(this string str, bool isLogError = true)
        {
            return GRMgr.Sprite.Get(str, isLogError);
        }
        public static Sprite GetIconPrefix(this string str, string prefix, bool isLogError = true)
        {
            return GRMgr.Icon.Get(prefix + str, isLogError);
        }
        public static Sprite GetIconSuffix(this string str, string suffix, bool isLogError = true)
        {
            return GRMgr.Icon.Get(str + suffix, isLogError);
        }
        public static AudioClip GetAudioClip(this string str, bool isLogError = true)
        {
            return GRMgr.Audio.Get(str);
        }
        public static GameObject GetPrefab(this string str)
        {
            return GRMgr.Prefab.Get(str);
        }
        #endregion

        #region Rand       
        public static T Rand<T>(this List<T> array)
        {
            if (array == null) return default;
            if (array.Count <= 0) return default;
            return array[RandUtil.RangeArray(array.Count)];
        }
        public static void Rand(this ref int i, int min, int max)
        {
            i = RandUtil.RandInt(min, max);
        }
        public static List<T> RandomSort<T>(this List<T> list)
        {
            var random = new System.Random();
            var newList = new List<T>();
            foreach (var item in list)
            {
                newList.Insert(random.Next(newList.Count), item);
            }
            return newList;
        }
        public static int GetAge(this List<AgeRange> ages)
        {
            return RandUtil.RandAge(ages);
        }
        #endregion

        #region UI
        // 获得奖励的字符窜
        public static string GetDesc(this List<BaseReward> data)
        {
            if (data == null) return "";
            string finalStr = "";
            int index = 0;
            foreach (var item in data)
            {
                if (index != 0) finalStr += "\n";
                finalStr += item.GetDesc();
                index++;
            }
            return finalStr;
        }
        public static void RebuildLayout(this LayoutGroup layoutGroup, float delay = 0)
        {
            if (delay == 0)
                (layoutGroup.transform as RectTransform).RebuildLayout();
            else
                Util.Invoke(() => (layoutGroup.transform as RectTransform).RebuildLayout(), delay);
        }
        public static void RebuildLayout(this RectTransform rectTrans)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTrans);
        }
        public static bool Contains(this LayerMask mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
        #endregion

        #region List
        public static void RemoveForEach<T>(this List<T> data, Func<T, bool> isTrue, Action<T> remove)
        {
            List<T> clear = new List<T>();
            foreach (var item in data)
            {
                if (isTrue(item))
                    clear.Add(item);
            }
            foreach (var item in clear)
                remove(item);
        }
        public static void ForEach<T1, T2>(this Dictionary<T1, T2> data, Action<T1, T2> action)
        {
            if (data == null) return;
            if (action == null) return;
            foreach (var item in data)
                action(item.Key, item.Value);
        }
        public static List<Cost<T>> SetInputVal<T>(this List<Cost<T>> cost, float input) where T : Enum
        {
            if (cost == null) return null;
            foreach (var item in cost)
                item.SetInputVal(input);
            return cost;
        }
        public static List<Cost<T>> SetAdd<T>(this List<Cost<T>> cost, float add) where T : Enum
        {
            if (cost == null) return null;
            foreach (var item in cost)
                item.SetAdd(add);
            return cost;
        }
        public static List<Cost<T>> SetPercent<T>(this List<Cost<T>> cost, float percent) where T : Enum
        {
            if (cost == null) return null;
            foreach (var item in cost)
                item.SetPercent(percent);
            return cost;
        }
        public static List<Cost<T>> SetUnit<T>(this List<Cost<T>> cost, BaseUnit unit) where T : Enum
        {
            if (cost == null) return null;
            foreach (var item in cost)
                item.SetUnit(unit);
            return cost;
        }
        public static void ForSafe<T>(this List<T> data, Callback<T> action)
        {
            for (int i = data.Count - 1; i >= 0; --i)
            {
                action.Invoke(data[i]);
            }
        }
        #endregion

        #region Dic
        public static void EnsureVal<T, V>(this Dictionary<T, V> data, T key, V value)
        {
            if (data == null)
            {
                CLog.Error("Dictionary:EnsureVal:data 为空");
                return;
            }
            if (!data.ContainsKey(key))
                data.Add(key, value);
            data[key] = value;
        }
        public static V GetVal<T, V>(this Dictionary<T, V> data, T key, V value)
        {
            if (data == null)
            {
                CLog.Error("Dictionary:GetVal:data 为空");
                return default;
            }
            if (!data.ContainsKey(key))
                return default;
            return data[key];
        }
        public static IList ToList<T,V>(this Dictionary<T,V> data)
        {
            if (data == null)
                return new List<KeyValuePair<T, V>>();
            return new List<KeyValuePair<T, V>>(data);
        }
        public static IList ToList<T, V>(this SortedDictionary<T, V> data)
        {
            if (data == null)
                return new List<KeyValuePair<T, V>>();
            return new List<KeyValuePair<T, V>>(data);
        }
        #endregion

        #region target
        public static float GetProgress(this List<BaseTarget> targets, BaseUnit selfUnit)
        {
            if (targets == null)
                return 1.0f;
            if (targets.Count == 0)
                return 1.0f;
            float total = 0;
            foreach (var item in targets)
            {
                BaseGlobal.ACM.Reset(selfUnit);
                BaseGlobal.ACM.Add(item);
                if (BaseGlobal.ACM.IsTrue())
                {
                    total += 1.0f;
                }
                else
                {
                    total += item.GetProgress();
                }
            }
            return total / targets.Count;
        }
        #endregion

        #region game objet
        public static T SafeAddComponet<T>(this GameObject gameObject) where T: Component
        {
            T ret = gameObject.GetComponent<T>();
            if (ret == null)
            {
                ret = gameObject.AddComponent<T>();
            }
            return ret;
        }
        public static bool IsActiveSelf(this Text text)
        {
            if (text == null)
                return false;
            return text.gameObject.activeSelf;
        }
        public static bool IsActiveSelf(this Image image)
        {
            if (image == null)
                return false;
            return image.gameObject.activeSelf;
        }
        public static void SetActive(this Text text,bool b)
        {
            if (text == null)
                return;
            if (text.gameObject.activeSelf == b)
                return;
            text.gameObject.SetActive(b);
        }
        public static void SetActive(this Image image,bool b)
        {
            if (image == null)
                return;
            if (image.gameObject.activeSelf == b)
                return;
            image.gameObject.SetActive(b);
        }
        #endregion

        #region reflection
        public static bool HasMethod(this object target, string methodName)
        {
            return target.GetType().GetMethod(methodName) != null;
        }

        public static bool HasField(this object target, string fieldName)
        {
            return target.GetType().GetField(fieldName) != null;
        }

        public static bool HasProperty(this object target, string propertyName)
        {
            return target.GetType().GetProperty(propertyName) != null;
        }
        #endregion

    }

}