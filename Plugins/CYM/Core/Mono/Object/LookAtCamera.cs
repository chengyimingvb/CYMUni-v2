//------------------------------------------------------------------------------
// LookAtCamera.cs
// Copyright 2019 2019/1/25 
// Created by CYM on 2019/1/25
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
namespace CYM
{
    public class LookAtCamera : MonoBehaviour
    {
        void Update()
        {
            if (BaseGlobal.MainCamera == null) 
                return;
            var lookPos = new Vector3(BaseGlobal.MainCamera.transform.position.x, transform.position.y, BaseGlobal.MainCamera.transform.position.z);
            transform.LookAt(lookPos);
        }
    }
}