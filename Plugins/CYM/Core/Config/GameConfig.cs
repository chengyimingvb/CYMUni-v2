//------------------------------------------------------------------------------
// GameConfig.cs
// Copyright 2018 2018/12/14 
// Created by CYM on 2018/12/14
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
using CYM;
using UnityEngine;

namespace CYM
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Config/Game", order = 4)]
    public sealed class GameConfig : CustomScriptableObjectConfig<GameConfig>
    {
        #region reference
        [SerializeField, FoldoutGroup("Reference")]
        Dictionary<string, Texture2D> RefTexture2D = new Dictionary<string, Texture2D>();
        [SerializeField, FoldoutGroup("Reference")]
        Dictionary<string, GameObject> RefGameObject = new Dictionary<string, GameObject>();
        [SerializeField, FoldoutGroup("Reference")]
        Dictionary<string, AnimationCurve> RefAnimationCurve = new Dictionary<string, AnimationCurve>();
        #endregion

        #region DB
        [SerializeField, FoldoutGroup("DB")]
        public bool DBCompressed = false;
        [SerializeField, FoldoutGroup("DB")]
        public bool DBHash = false;
        [SerializeField, FoldoutGroup("DB")]
        public bool DBSaveAsyn = true;
        [SerializeField, FoldoutGroup("DB")]
        public bool DBLoadAsyn = true;
        #endregion

        #region Dyn
        [FoldoutGroup("Dyn"), ReadOnly, SerializeField, HideInInspector]
        private List<string> DynStrName = new List<string>();
        [FoldoutGroup("Dyn"), ReadOnly, SerializeField, HideInInspector]
        private List<MethodInfo> DynStrMethodInfo = new List<MethodInfo>();
        [FoldoutGroup("Dyn"), SerializeField]
        public Dictionary<string, MethodInfo> DynStrFuncs = new Dictionary<string, MethodInfo>();
        [FoldoutGroup("Dyn"), ReadOnly, SerializeField, HideInInspector]
        public string MonoTypeName = "";
#if UNITY_EDITOR
        [FoldoutGroup("Dyn"), SerializeField]
        public MonoScript DynamicFuncScript;
#endif
        #endregion

        #region Url
        [FoldoutGroup("URL")]
        public string URLWebsite = "https://store.steampowered.com/developer/Yiming";
        #endregion

        #region Person
        [SerializeField, FoldoutGroup("Person"),Tooltip("是否随机年龄头像,否则一一对应年龄头像")]
        public bool IsRandChildHeadIcon = true;
        [SerializeField, FoldoutGroup("Person"), Tooltip("是否随机年龄头像,否则一一对应年龄头像")]
        public bool IsRandOldHeadIcon = true;
        [SerializeField, FoldoutGroup("Person"),DictionaryDrawerSettings(IsReadOnly = true)]
        public RangeDic<AgeRange> AgeRangeData = new RangeDic<AgeRange>()
        {
            { AgeRange.Child,   new Range(1,16) },
            { AgeRange.Adult,   new Range(16,40) },
            { AgeRange.Middle,  new Range(40,45) },
            { AgeRange.Old,     new Range(45,80) },
        };
        [SerializeField, FoldoutGroup("Person"), DictionaryDrawerSettings(IsReadOnly = true)]
        public Dictionary<AgeRange, float> DeathProb = new Dictionary<AgeRange, float>()
        {
            { AgeRange.Child,   0.0015f },
            { AgeRange.Adult,   0.001f },
            { AgeRange.Middle,  0.0015f },
            { AgeRange.Old,     0.01f },
        };
        #endregion

        #region SLG Weather
        [SerializeField, FoldoutGroup("SLG Weather")]
        public int StartCount = 30;
        [SerializeField, FoldoutGroup("SLG Weather")]
        public int TotalCount = 40;
        [SerializeField, FoldoutGroup("SLG Weather")]
        public int CellCount = 30;
        [SerializeField, FoldoutGroup("SLG Weather")]
        public int Squar = 1024;
        [SerializeField, FoldoutGroup("SLG Weather")]
        public List<SLGWeatherConfigData> SLGWeather = new List<SLGWeatherConfigData>();
        #endregion

        #region Season
        [SerializeField, FoldoutGroup("Season")]
        public bool IsShowSnow = false;
        [SerializeField, FoldoutGroup("Season")]
        public float WindPowerAdt = 0.8f;
        [SerializeField, FoldoutGroup("Season"), DictionaryDrawerSettings(IsReadOnly = true)]
        public Dictionary<SeasonType, SeasonData> Season = new Dictionary<SeasonType, SeasonData>()
        {
            {
                SeasonType.Spring,new SeasonData
                {
                        SunIntensity = 0.85f,
                        AccumulatedSnow = 0.0f,
                        WindzonePower = 0.2f,
                }
            },
            {
                SeasonType.Summer,new SeasonData
                {
                        SunIntensity = 0.9f,
                        AccumulatedSnow = 0.0f,
                        WindzonePower = 0.25f,
                }
            },
            {
                SeasonType.Fall,new SeasonData
                {
                        SunIntensity = 0.75f,
                        AccumulatedSnow = 0.0f,
                        WindzonePower = 0.25f,
                }
            },
            {
                SeasonType.Winter,new SeasonData
                {
                        SunIntensity = 0.7f,
                        AccumulatedSnow = 0.2f,
                        WindzonePower = 0.29f,
                }
            }
        };
        #endregion

        #region Realtime
        [SerializeField, FoldoutGroup("Realtime")]
        public List<float> GameSpeed = new List<float>
        {
            1,2,3,4
        };
        #endregion

        #region Job system
        [SerializeField, FoldoutGroup("Job System")]
        public int UnitJobPerFrame = 100;
        [SerializeField, FoldoutGroup("Job System")]
        public int GlobalJobPerFrame = 100;
        [SerializeField, FoldoutGroup("Job System")]
        public int ViewJobPerFrame = 10;
        [SerializeField, FoldoutGroup("Job System")]
        public int NormalJobPerFrame = 100;
        #endregion

        #region Ref
        public Texture2D GetTexture2D(string id)
        {
            if (RefTexture2D.ContainsKey(id)) return RefTexture2D[id];
            return null;
        }
        public GameObject GetGameObject(string id)
        {
            if (RefGameObject.ContainsKey(id)) return RefGameObject[id];
            return null;
        }
        public AnimationCurve GetAnimationCurve(string id)
        {
            if (RefAnimationCurve.ContainsKey(id)) return RefAnimationCurve[id];
            return null;
        }
        #endregion

        #region life
        private void OnEnable()
        {
#if UNITY_EDITOR
            if (DynamicFuncScript != null)
            {
                MonoTypeName = DynamicFuncScript.GetClass().FullName;
            }
#endif
            DynStrName.Clear();
            DynStrMethodInfo.Clear();
            var type = Type.GetType(MonoTypeName);
            if (type == null)
                return;
            var array = type.GetMethods();
            foreach (var item in array)
            {
                var attrArray = item.GetCustomAttributes(true);
                foreach (var attr in attrArray)
                {
                    if (attr is DynStr)
                    {
                        DynStrName.Add(item.Name);
                        DynStrMethodInfo.Add(item);
                    }
                }
            }
            DynStrFuncs.Clear();
            for (int i = 0; i < DynStrName.Count; ++i)
            {
                DynStrFuncs.Add(DynStrName[i], DynStrMethodInfo[i]);
            }
        }
        #endregion
    }
}