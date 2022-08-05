//------------------------------------------------------------------------------
// Videoer.cs
// Copyright 2021 2021/3/21 
// Created by CYM on 2021/3/21
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using UnityEngine.Video;
using DG.Tweening;

namespace CYM
{
    public sealed class Videoer : MonoBehaviour
    {
        static VideoPlayer VideoPlayer { get; set; }
        private void Awake()
        {
            VideoPlayer = gameObject.SafeAddComponet<VideoPlayer>();
            Stop();
        }

        #region set
        public static void Play(string videoName, float startAlpha, float duration, float delay)
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.targetCamera = BaseGlobal.MainCamera;
                VideoPlayer.clip = BaseGlobal.GRMgr.Video.Get(videoName);
                VideoPlayer?.Play();
                VideoPlayer.targetCameraAlpha = startAlpha;
                DOTween.To(() => VideoPlayer.targetCameraAlpha, x => VideoPlayer.targetCameraAlpha = x, 1.0f, duration).SetDelay(delay).OnComplete(OnTweenPlay);
            }

        }
        public static void Stop()
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.targetCameraAlpha = 0.0f;
                VideoPlayer.targetCamera = null;
                VideoPlayer?.Stop();
            }
        }
        #endregion

        #region Callback
        static void OnTweenPlay()
        {

        }
        static void OnTweenStop()
        {

        }
        #endregion
    }
}