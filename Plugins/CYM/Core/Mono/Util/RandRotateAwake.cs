//------------------------------------------------------------------------------
// RandRotateY.cs
// Created by CYM on 2022/6/6
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public class RandRotateAwake : MonoBehaviour
    {
        private void Awake()
        {
            transform.rotation = Quaternion.Euler(0,RandUtil.Range(0,360),0);
        }
    }
}