//------------------------------------------------------------------------------
// BaseCSMgr.cs
// Copyright 2019 2019/4/9 
// Created by CYM on 2019/4/9
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.DLC;
using RoslynCSharp;
using RoslynCSharp.Compiler;
using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace CYM
{
    public class BaseCSMgr : BaseGFlowMgr, ILoader
    {
        #region callback val
        public event Callback Callback_OnParseEnd;
        public event Callback Callback_OnParseStart;
        #endregion

        #region prop
        protected ScriptDomain Domain { get; private set; }
        protected ScriptAssembly Assembly { get; private set; }
        protected string Sources { get; set; } = "";
        #endregion

        #region life
        public override void OnCreate()
        {
            base.OnCreate();
            //AppDomain appDomain = AppDomain.CurrentDomain;
            Domain = ScriptDomain.CreateDomain("ExtendCSharp", true, true);
        }
        #endregion

        #region loader
        public IEnumerator Load()
        {
            //加载DLC CSharp
            Callback_OnParseStart?.Invoke();
            foreach (var dlc in DLCManager.LoadedDLCItems.Values)
            {
                if (BuildConfig.Ins.IsEditorOrConfigMode)
                {
                    string[] fileList = dlc.GetAllCSharp();
                    foreach (var item in fileList)
                    {
                        Sources += File.ReadAllText(item) + "\n";
                        BaseGlobal.LoaderMgr.ExtraLoadInfo = "Load CSharp " + item;
                    }
                }
                else
                {
                    var assetBundle = dlc.LoadRawBundle(Const.BN_CSharp);
                    if (assetBundle != null)
                    {
                        foreach (var item in assetBundle.LoadAllAssets<TextAsset>())
                        {
                            Sources += item.text + "\n";
                            BaseGlobal.LoaderMgr.ExtraLoadInfo = "Load CSharp " + item.name;
                        }
                    }
                }
                yield return new WaitForEndOfFrame();
            }

            if (!Sources.IsInv())
            {
                Assembly = Domain.CompileAndLoadSource(Sources, ScriptSecurityMode.UseSettings);

                //打印编译错误
                if (Domain != null && Domain.CompileResult != null)
                {
                    if (Domain.CompileResult.Success == false)
                    {
                        foreach (CompilationError error in Domain.CompileResult.Errors)
                        {
                            if (error.IsError == true)
                                CLog.Error(error.ToString());
                            else if (error.IsWarning == true)
                                CLog.Warn(error.ToString());
                        }
                    }
                }

                //执行所有DLC的MainCalss
                if (Assembly != null)
                {
                    var allTypes = Assembly.FindAllSubTypesOf(typeof(BaseMainCS));
                    foreach (var item in allTypes)
                    {
                        ScriptProxy proxy = item.CreateInstance();
                        proxy.SafeCall("Main");
                    }
                }
            }
            Callback_OnParseEnd?.Invoke();
            yield break;
        }
        public string GetLoadInfo()
        {
            return "Load CS";
        }
        #endregion
    }
}