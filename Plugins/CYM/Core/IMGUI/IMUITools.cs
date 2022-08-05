//------------------------------------------------------------------------------
// IMUITools.cs
// Copyright 2020 2020/9/12 
// Created by CYM on 2020/9/12
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using RapidGUI;
using CYM;
using CYM.UI;
using CYM.AI;
using Sirenix.OdinInspector;

namespace CYM
{
    [HideMonoScript]
    public class IMUITools : IMUIBase
    {
        public override string Title => "Tools";
        public override KeyCode KeyCode => KeyCode.F5;
        public override float Width => 300;
        public override float Height => 600;
        public override bool IsResizable => false;
        public override bool IsDragble => false;
        public override float XPos => 10;
        public override float YPos => 10;

        protected override void OnDrawWindow()
        {
            base.OnDrawWindow();
            if (RGUI.Button("游戏存档"))
            {
                FileUtil.OpenExplorer(Application.persistentDataPath);
            }
            if (RGUI.Button("游戏资源"))
            {
                FileUtil.OpenExplorer(Const.Path_StreamingAssets);
            }
            if (RGUI.Button("游戏官网"))
            {
                FileUtil.OpenExplorer(GameConfig.Ins.URLWebsite);
            }
        }
    }
}