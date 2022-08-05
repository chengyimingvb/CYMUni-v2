//------------------------------------------------------------------------------
// MakeMeshHole.cs
// Created by CYM on 2022/3/13
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEditor;

namespace CYM
{
    public class MakeMeshHole : MonoBehaviour
    {
        #region Inspector
        [SerializeField]
        Mesh Data;
        [SerializeField]
        Texture2D UnWalkable;
        #endregion

        #region Action

#if UNITY_EDITOR
        [Button("Convert")]
        void Convert()
        {
            for (int x = 0; x < UnWalkable.width; ++x)
            {
                for (int y = 0; y < UnWalkable.height; ++y)
                {
                    var color = UnWalkable.GetPixel(x,y);
                    if (color.r==0)
                    {
                        bool ray = Physics.Raycast(new Ray(new Vector3(x, 99999,y),-Vector3.up),out RaycastHit hitData,int.MaxValue,(LayerMask)Const.Layer_Terrain);
                        if (ray)
                        {
                            Data.triangles[hitData.triangleIndex*3+2]=-1;
                            Data.triangles[hitData.triangleIndex * 3 + 1]=-1;
                            Data.triangles[hitData.triangleIndex * 3]=-1;
                        }
                    }
                }
            }
            var data = Data.triangles;
            List<int> tris = new List<int>(data);
            tris.RemoveAll(x=>x==-1);
            Mesh newMesh = new Mesh();
            newMesh.triangles = tris.ToArray();
            AssetDatabase.CreateAsset(newMesh,Const.Path_Resources+ "/Test.asset");
            AssetDatabase.SaveAssets();


        }
#endif

        #endregion
    }
}