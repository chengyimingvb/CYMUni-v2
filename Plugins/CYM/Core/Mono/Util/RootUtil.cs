//------------------------------------------------------------------------------
// AdjTransToTerrain.cs
// Copyright 2019 2019/10/23 
// Created by CYM on 2019/10/23
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CYM
{
    [ExecuteInEditMode]
    public sealed class RootUtil : MonoBehaviour
    {
        void Awake()
        {
            if (Application.isPlaying)
            {
                Destroy(this);
            }

        }
        [Button("CheckMissing")]
        void DoCheckMissing()
        {
            Transform[] trans = GetComponentsInChildren<Transform>(true);
            foreach (var item in trans)
            {

                foreach (var com in item.gameObject.GetComponents<Component>())
                {

                    if (com == null)
                        Debug.LogError("Missing:" + GetPath(item.gameObject), item.gameObject);
                }
            }
            static string GetPath(GameObject go)
            {
                return go.transform.parent == null ? "/" + go.name : GetPath(go.transform.parent.gameObject) + "/" + go.name;
            }
        }
        [Button("AdjToTerrain")]
        void AdjToTerrain()
        {
            List<Transform> childs = new List<Transform>();
            childs.Clear();
            for (int i = 0; i < transform.childCount; ++i)
            {
                var item = transform.GetChild(i);
                childs.Add(item);
                item.position = item.position.SetY(SampleHeight(item.position));

            }

            float SampleHeight(Vector3 point)
            {
                if (Terrain.activeTerrain == null)
                    return 0;
                if (Terrain.activeTerrain.terrainData == null)
                    return 0;
                return Terrain.activeTerrain.SampleHeight(point) + Terrain.activeTerrain.transform.position.y;
            }
        }

        [Button("PlayParticle")]
        void PlayParticle()
        {
            var PS = gameObject.GetComponentsInChildren<ParticleSystem>();
            foreach (var item in PS)
            {
                item.Play();
            }
        }
        [Button("StopParticle")]
        void StopParticle()
        {
            var PS = gameObject.GetComponentsInChildren<ParticleSystem>();
            foreach (var item in PS)
            {
                item.Stop();
            }
        }
        [Button("ExportAStarPathMap")]
        void ExportAStarPathMap()
        {
            var terrainWidth = 1024;
            var terrainHeight = 1024;
            Texture2D texture = new Texture2D(terrainWidth, terrainHeight);

            var terrainResolution = Terrain.activeTerrain.terrainData.heightmapResolution;
            for (int x = 0; x < terrainWidth; ++x)
            {
                for (int y = 0; y < terrainHeight; ++y)
                {
                    var realX = (terrainResolution / terrainWidth) * x;
                    var realY = (terrainResolution / terrainHeight) * y;
                    var splatMap = Terrain.activeTerrain.terrainData.GetAlphamaps(realX, realY, 1, 1);
                    int index = 0;
                    float val = 0;
                    for (int i = 0; i < splatMap.GetLength(2); ++i)
                    {
                        if (splatMap[0, 0, i] > val)
                        {
                            val = splatMap[0, 0, i];
                            index = i;
                        }
                    }
                    texture.SetPixel(x, y, index == 0 ? Color.white:Color.black) ;
                }
            }
            texture.Apply();
            FileUtil.SaveTextureToPNG(texture, Path.Combine(Const.Path_ResourcesTemp, "AStarPathMap_NC.png"));
        }
    }
}