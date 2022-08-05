//------------------------------------------------------------------------------
// IMUIOptions.cs
// Copyright 2020 2020/7/27 
// Created by CYM on 2020/7/27
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using System;
using System.Reflection;
using RapidGUI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Sirenix.OdinInspector;

namespace CYM
{
    public class IMUIOptions : IMUIBase
    {
        public static IMUIOptions Ins { get; private set; } 

        #region prop
        public override string Title => "Options";
        public override KeyCode KeyCode => KeyCode.F3;
        public override float Width => 300;
        public override float Height => 600;
        public override bool IsResizable => true;
        public override bool IsDragble => false;
        public override float XPos => 10;
        public override float YPos => 10;

        public Vector2 PropertyScrollVector { get; private set; }
        public Vector2 MethodScrollVector { get; private set; }

        Type TypeOptions;
        MemberInfo[] MemberInfos;
        Fold FoldView = new Fold("View");
        Dictionary<string, Fold> Folds = new Dictionary<string, Fold>();
        Dictionary<string, List<MemberInfo>> TypedMemberInfos = new Dictionary<string, List<MemberInfo>>();
        Dictionary<string, Vector2> ScrollVals = new Dictionary<string, Vector2>();
        #endregion

        #region life
        protected override void Awake()
        {
            Ins = this;
            base.Awake();
            TypeOptions = typeof(Options);
            MemberInfos = TypeOptions.GetMembers(BindingFlags.Public | BindingFlags.Static);
            foreach (var item in MemberInfos)
            {
                CategoryAttribute catyAttr = item.GetCustomAttribute(typeof(CategoryAttribute)) as CategoryAttribute;
                FoldoutGroupAttribute foldAttr = item.GetCustomAttribute(typeof(FoldoutGroupAttribute)) as FoldoutGroupAttribute;
                string typeName = "None";
                if (catyAttr != null)
                {
                    typeName = catyAttr.Category;
                }
                else if (foldAttr != null)
                {
                    typeName = foldAttr.GroupName;
                }
                if (!TypedMemberInfos.ContainsKey(typeName))
                {
                    TypedMemberInfos.Add(typeName, new List<MemberInfo>());
                    Folds.Add(typeName,new Fold(typeName));
                    ScrollVals.Add(typeName,new Vector2());
                }
                var list = TypedMemberInfos[typeName];
                list.Add(item);
            }

            foreach (var item in Folds)
            {
                item.Value.Add(()=> {
                    ScrollVals[item.Key] = DoMember(TypedMemberInfos[item.Key],ScrollVals[item.Key]);
                });
            }

            FoldView.Add(()=> {
                RGUI.BeginVertical();
                foreach (var item in AllIMUI)
                {
                    if (item.IsOptionView)
                    {
                        if (RGUI.Button(item.Title))
                        {
                            item.Toggle();
                        }
                    }
                }
                RGUI.EndVertical();
            });
        }
        protected override void OnDrawWindow()
        {
            base.OnDrawWindow();
            foreach (var item in Folds)
            {
                if(TypedMemberInfos[item.Key].Count>0)
                    item.Value.DoGUI();
            }
            FoldView.DoGUI();
        }

        private Vector2 DoMember(List<MemberInfo> memberInfos, Vector2 scrollVal)
        {
            int scrollNum = 30;
            if(memberInfos.Count>= scrollNum)
                scrollVal = RGUI.BeginScrollView(scrollVal,GUILayout.MaxHeight(500));
            for(int i=0;i< memberInfos.Count;++i)
            {
                var item = memberInfos[i];
                if (item.MemberType == MemberTypes.Property)
                {
                    PropertyInfo property = item as PropertyInfo;
                    if (property.PropertyType == typeof(bool))
                    {
                        property.SetValue(Options.Ins, RGUI.Toggle((bool)property.GetValue(Options.Ins), item.Name));
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(Options.Ins, RGUI.Field((int)property.GetValue(Options.Ins), item.Name));
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(Options.Ins, RGUI.Field((string)property.GetValue(Options.Ins), item.Name));
                    }
                }
                else if (item.MemberType == MemberTypes.Method)
                {
                    MethodInfo method = item as MethodInfo;
                    if (method.IsSpecialName)
                        continue;
                    var para = method.GetParameters();
                    if (para != null && para.Length > 0)
                        continue;
                    if (RGUI.Button(item.Name))
                    {
                        method.Invoke(Options.Ins, null);
                    }
                }
            }
            if (memberInfos.Count >= scrollNum)
                RGUI.EndScrollView();
            return scrollVal;
        }

        public override void Show()
        {
            if (!BaseGlobal.DiffMgr.IsGMMode())
                return;
            base.Show();
        }
        #endregion
    }
}