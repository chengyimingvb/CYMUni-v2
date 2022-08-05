//------------------------------------------------------------------------------
// Analytics.cs
// Copyright 2021 2021/7/19 
// Created by CYM on 2021/7/19
// Owner: CYM
// 填写类的描述...
//------------------------------------------------------------------------------


using GameAnalyticsSDK;

namespace CYM
{
    public sealed class Analytics 
    {
        public static void Resource(GAResourceFlowType flowType, string currency /* Gems */, float amount /* 400 */, string itemType /* IAP */, string itemId /* Coins400 */)
        {
            GameAnalytics.NewResourceEvent(flowType, currency, amount, itemType, itemId);
        }
        public static void Progression(GAProgressionStatus progressionStatus, string progression01, int score)
        {
            GameAnalytics.NewProgressionEvent(progressionStatus, progression01, score);
        }
        public static void Event(string eventName, float eventValue)
        {
            GameAnalytics.NewDesignEvent( eventName, eventValue);
        }
        public static void Error(GAErrorSeverity severity, string message)
        {
            GameAnalytics.NewErrorEvent(severity, message);
        }
    }
}