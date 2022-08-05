//------------------------------------------------------------------------------
// TDBaseAttrConf.cs
// Created by CYM on 2022/4/4
// 填写类的描述...
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace CYM
{
    [AttributeUsage(AttributeTargets.Property)]
    public class AttrValueAttribute: Attribute
    { 
    
    }
    [Serializable]
    public class TDBaseAttrConfData<TType,TSelf> : TDBaseData
        where TType : Enum
        where TSelf : TDBaseData
    {
        #region prop
        //属性
        public Dictionary<TType, float> Attrs { get; set; } = new Dictionary<TType, float>();
        #endregion

        public override void OnBeAddedToData()
        {
            base.OnBeAddedToData();
            var flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            Type selfType = typeof(TSelf);

            foreach (var item in selfType.GetProperties(flag))
            {
                if (item.IsDefined(typeof(AttrValueAttribute)))
                {
                    string strName = item.Name;
                    TType typeEnum = Enum<TType>.Parse(strName);
                    float val = (float)item.GetValue(this);
                    if (!val.IsInv())
                    {
                        Attrs[typeEnum] = val;
                    }
                    else
                    {
                        item.SetValue(this, Attrs[typeEnum]);
                    }
                }
            }
        }
    }
}