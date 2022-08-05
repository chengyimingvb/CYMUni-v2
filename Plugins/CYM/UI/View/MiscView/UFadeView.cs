//**********************************************
// Class Name	: BaseFadeView
// Discription	：None
// Author	：CYM
// Team		：BloodyMary
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************

namespace CYM.UI
{
    public class UFadeView : UStaticUIView<UFadeView>
    {
        bool isFirst = true;
        protected override void OnCreatedView()
        {
            base.OnCreatedView();
        }
        public override void OnAffterStart()
        {
            base.OnAffterStart();
        }
        protected override void OnOpenDelay(UView baseView, bool useGroup)
        {
            base.OnOpenDelay(baseView, useGroup);
            if (isFirst)
            {
                Close(0.5f);
                isFirst = false;
            }
        }
    }
}