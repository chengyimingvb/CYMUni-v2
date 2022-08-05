using CYM.UI;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
//**********************************************
// Class Name	: CYMBaseScreenController
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
namespace CYM
{
    public partial class UIUtil:BaseUIUtil
    {
        #region UIFormat
        // 条件
        public static string Condition(bool isTrue, string str)
        {
            if (isTrue)
                return Const.STR_Indent + "<Color=" + Const.COL_Green + ">" + str + "</Color>";
            return Const.STR_Indent + "<Color=" + Const.COL_Red + ">" + str + "</Color>";
        }
        // 标题内容
        public static string Line(string title, string content) => string.Format("{0}\n"+Const.STR_Line+"\n{1}", title, content);
        // 日期
        public static string TimeSpan(TimeSpan span)
        {
            int totalHours = span.Days * 24 + span.Hours;
            if (totalHours > 0)
            {
                return string.Format("{1}{0}{2}{0}{3}{0}{4}", BaseLangMgr.Space, totalHours, BaseLangMgr.Get("Unit_小时"), span.Minutes, BaseLangMgr.Get("Unit_分钟"));
            }
            else
            {
                return string.Format("{1}{0}{2}", BaseLangMgr.Space, span.Minutes, BaseLangMgr.Get("Unit_分钟"));
            }
        }
        // buff后缀
        public static string BuffSuffix(string str) => string.Format("{0}{1}{2}", str, BaseLangMgr.Space, BaseLangMgr.Get("Unit_Buff"));
        // Attr name 后缀
        public static string AttrTypeNameSuffix(string str, Enum type) => string.Format("{0}{1}{2}", str, BaseLangMgr.Space, type.GetName());
        // 天后缀
        public static string DaySuffix(string str) => string.Format("{0}{1}{2}", str, BaseLangMgr.Space, BaseLangMgr.Get("Unit_天"));
        // 天后缀
        public static string MonthSuffix(string str) => string.Format("{0}{1}{2}", str, BaseLangMgr.Space, BaseLangMgr.Get("Unit_月"));
        // 天后缀
        public static string YearSuffix(string str) => string.Format("{0}{1}{2}", str, BaseLangMgr.Space, BaseLangMgr.Get("Unit_年"));
        //分数
        public static string Fraction(float val, float denominator, bool isInt = false)
        {
            if (isInt) return Mathf.RoundToInt(val).ToString() + "/" + Mathf.RoundToInt(denominator).ToString();
            else return UIUtil.D1(val) + "/" + UIUtil.D1(denominator);
        }
        public static string FractionCol(float val, float denominator, bool isInt = false, bool onlyRed = false)
        {
            string ret = "";
            if (isInt)
                ret = ((int)val).ToString() + "/" + ((int)denominator).ToString();
            else
                ret = UIUtil.D1(val) + "/" + UIUtil.D1(denominator);

            if (val > denominator) return Red(ret);
            if (!onlyRed)
            {
                if (val == denominator) return Yellow(ret);
                return Green(ret);
            }
            else
            {
                return ret;
            }
        }
        #endregion

        #region other
        public static string GetStr(string key, params object[] ps) => BaseLangMgr.Get(key, ps);
        public static string GetPath(GameObject go) => go.transform.parent == null ? "/" + go.name : GetPath(go.transform.parent.gameObject) + "/" + go.name;
        // Finds the component in the game object's parents.
        public static T FindInParents<T>(GameObject go) where T : Component
        {
            if (go == null)
                return null;

            var comp = go.GetComponent<T>();

            if (comp != null)
                return comp;

            Transform t = go.transform.parent;

            while (t != null && comp == null)
            {
                comp = t.gameObject.GetComponent<T>();
                t = t.parent;
            }

            return comp;
        }
        public static IList GetTestScrollData(int count=30)
        {
            List<object> test = new List<object>();
            Util.For(count, (i) => test.Add(new object()));
            return test;
        }
        #endregion

        #region set
        public static void SetTrans(RectTransform rect, Vector2 pos, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 size)
        {
            rect.anchoredPosition = pos;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = size;
            rect.pivot = pivot;
        }
        public static void SetAnchorPosition(RectTransform rectTrans, Anchor anchor, Vector2 pos)
        {
            if (anchor == Anchor.None)
                return;
            if (anchor == Anchor.Bottom)
            {
                rectTrans.pivot = new Vector2(0.5f, 0);
                rectTrans.anchorMin = new Vector2(0.5f, 0);
                rectTrans.anchorMax = new Vector2(0.5f, 0);
                rectTrans.anchoredPosition = pos;
            }
            else if (anchor == Anchor.BottomLeft)
            {
                rectTrans.pivot = new Vector2(0.0f, 0);
                rectTrans.anchorMin = new Vector2(0.0f, 0);
                rectTrans.anchorMax = new Vector2(0.0f, 0);
                rectTrans.anchoredPosition = pos;
            }
            else if (anchor == Anchor.BottomRight)
            {
                rectTrans.pivot = new Vector2(1.0f, 0);
                rectTrans.anchorMin = new Vector2(1.0f, 0);
                rectTrans.anchorMax = new Vector2(1.0f, 0);
                rectTrans.anchoredPosition = pos;
            }
            else if (anchor == Anchor.Left)
            {
                rectTrans.pivot = new Vector2(0.0f, 0.5f);
                rectTrans.anchorMin = new Vector2(0.0f, 0.5f);
                rectTrans.anchorMax = new Vector2(0.0f, 0.5f);
                rectTrans.anchoredPosition = pos;
            }
            else if (anchor == Anchor.Right)
            {
                rectTrans.pivot = new Vector2(1.0f, 0.5f);
                rectTrans.anchorMin = new Vector2(1.0f, 0.5f);
                rectTrans.anchorMax = new Vector2(1.0f, 0.5f);
                rectTrans.anchoredPosition = pos;
            }
            else if (anchor == Anchor.Top)
            {
                rectTrans.pivot = new Vector2(0.5f, 1.0f);
                rectTrans.anchorMin = new Vector2(0.5f, 1.0f);
                rectTrans.anchorMax = new Vector2(0.5f, 1.0f);
                rectTrans.anchoredPosition = pos;
            }
            else if (anchor == Anchor.TopLeft)
            {
                rectTrans.pivot = new Vector2(0.0f, 1.0f);
                rectTrans.anchorMin = new Vector2(0.0f, 1.0f);
                rectTrans.anchorMax = new Vector2(0.0f, 1.0f);
                rectTrans.anchoredPosition = pos;
            }
            else if (anchor == Anchor.TopRight)
            {
                rectTrans.pivot = new Vector2(1.0f, 1.0f);
                rectTrans.anchorMin = new Vector2(1.0f, 1.0f);
                rectTrans.anchorMax = new Vector2(1.0f, 1.0f);
                rectTrans.anchoredPosition = pos;
            }
        }
        #endregion

        #region get res
        public static Sprite GetUISprite()
        {
#if UNITY_EDITOR
            return AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kStandardSpritePath);
#else
            return null;
#endif
        }
        public static Sprite GetUIBackground()
        {
#if UNITY_EDITOR
            return AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kBackgroundSpritePath);
#else
            return null;
#endif
        }
        public static Sprite GetUIUIMask()
        {
#if UNITY_EDITOR
            return AssetDatabase.GetBuiltinExtraResource<Sprite>(SysConst.kMaskPath);
#else
            return null;
#endif
        }
        #endregion

        #region fullscreen
        public static void CreateFullscreenBG(Transform Trans, bool IsFullScreen, bool IsAddBlocker, bool IsBlockerClose, Color BlockerCol, Callback close)
        {
            if (IsFullScreen && IsAddBlocker)
            {
                UImage Blocker;
                var temp = GameObject.Instantiate(BaseGlobal.GRMgr.GetResources("BaseBlocker"));
                temp.transform.SetParent(Trans);
                temp.transform.SetAsFirstSibling();
                Blocker = temp.GetComponent<UImage>();
                Blocker.Init(new UImageData { Color = () => BlockerCol });
                if (IsBlockerClose)
                {
                    Blocker.IsCanClick = true;
                    Blocker.Data.OnClick = (x, y) => close?.Invoke();
                }
                else
                {
                    if (UIConfig.Ins.Error != null)
                        Blocker.Data.ClickClip = UIConfig.Ins.Error.name;
                }
            }
        }
        #endregion
    }

}