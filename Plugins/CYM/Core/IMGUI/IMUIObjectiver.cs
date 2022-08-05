//------------------------------------------------------------------------------
// IMUIObjectiver.cs
// Copyright 2020 2020/7/10 
// Created by CYM on 2020/7/10
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.AI.Objective;
using RapidGUI;
using System;
using UnityEngine;

namespace CYM
{
    public abstract class IMUIObjectiver<TObjective, TEnum, TUnit, TTarget> : IMUIBase
        where TObjective : Objectiver<TEnum, TUnit, TTarget>
        where TEnum : Enum
        where TUnit : BaseUnit
        where TTarget : BaseUnit
    {
        #region prop
        public override string Title => "Objectiver";
        public override bool IsWindow => true;
        public override bool IsDragble => true;
        public override bool IsResizable => true;
        public override float XPos => 300;
        public override float YPos => 0;
        public override float Height => 800;
        public override bool IsOptionView => true;
        public override bool IsBlock => false;
        protected abstract TObjective Objectiver { get; }
        #endregion

        #region life
        protected override void OnDrawWindow()
        {
            base.OnDrawWindow();
            if (Objectiver != null)
            {
                RGUI.BeginHorizontal();
                Enum<TEnum>.For(x =>
                {
                    RGUI.BeginVertical();
                    RGUI.Label(x.ToString(), RGUIStyle.warningLabel, RGUI.Width(200));
                    foreach (var objective in Objectiver.GetTasks(x))
                    {
                        GUILayout.Label(objective.Target.GetName());
                        var units = Objectiver.GetExcuteUnits(objective);
                        if (units != null)
                        {
                            foreach (var legion in units)
                            {
                                RGUI.Label("-" + legion.GetName());
                            }
                        }
                    }
                    RGUI.EndVertical();
                });

                RGUI.BeginVertical();
                RGUI.Label("Idle", RGUIStyle.warningLabel, RGUI.Width(200));
                foreach (var legion in Objectiver.AllUnits)
                {
                    if (!Objectiver.IsHaveTask(legion))
                    {
                        GUILayout.Label(legion.GetName());
                    }
                }
                RGUI.EndVertical();
                RGUI.EndHorizontal();
            }
        }
        #endregion
    }
}