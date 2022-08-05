//------------------------------------------------------------------------------
// IMUIFeedback.cs
// Copyright 2020 2020/6/26 
// Created by CYM on 2020/6/26
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------

using CYM.UI;
using RapidGUI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace CYM
{
    [HideMonoScript]
    public class IMUIFeedback : IMUIBase
    {
        public new static bool IsShow { get; private set; } = false;
        public override string Title => "Feedback";
        public override bool IsResizable => false;
        public override bool IsDragble => false;
        public override KeyCode KeyCode => KeyCode.F2;
        bool isPopShow = false;
        const string sourceTitle = "Please enter a title";
        const string sourceContact = "Please enter contact information";
        const string sourceDesc = "Please enter a description of the problem";
        string submitTitle = "";
        string submitContact = "";
        string submitDesc = "";
        protected override void Start()
        {
            base.Start();
            ResetText();
        }
        void ResetText()
        {
            submitTitle = sourceTitle;
            submitContact = sourceContact;
            submitDesc = sourceDesc;
        }
        protected override void OnDrawWindow()
        {
            base.OnDrawWindow();
            RGUI.BeginVertical();
            if (isPopShow)
            {
                RGUI.Label(Util.GetStr("Desc_Feedback_Error"), RGUIStyle.warningLabel);
            }
            RGUI.BeginHorizontal();
            RGUI.Label(Util.GetStr("Desc_Feedback_Title"), RGUI.Width(200));
            submitTitle = RGUI.TextField(submitTitle, RGUIStyle.warningLabel);
            RGUI.EndHorizontal();

            RGUI.BeginHorizontal();
            RGUI.Label(Util.GetStr("Desc_Feedback_Contact"), RGUI.Width(200));
            submitContact = GUILayout.TextField(submitContact, RGUIStyle.warningLabel);
            RGUI.EndHorizontal();

            RGUI.Label(Util.GetStr("Desc_Feedback_Desc"));
            submitDesc = GUILayout.TextArea(submitDesc, RGUIStyle.warningLabel, RGUI.Height(350));

            RGUI.BeginHorizontal();
            if (RGUI.Button(Util.GetStr("Bnt_取消")))
            {
                Close();
            }
            if (RGUI.Button(Util.GetStr("Bnt_发送")))
            {
                if (
                    submitDesc.IsInv() ||
                    submitDesc == sourceDesc
                    )
                {
                    isPopShow = true;
                    return;
                }
                Feedback.SendMail(submitTitle, submitDesc, submitContact);
                Close();

                UTipView.Default?.Show("Text_发送成功");
            }
            RGUI.EndHorizontal();
            RGUI.EndVertical();
        }

        public override void Show()
        {
            base.Show();
            ResetText();
            isPopShow = false;
            IsShow = true;
        }
        public override void Close()
        {
            base.Close();
            isPopShow = false;
            IsShow = false;
        }
    }
}