using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_STANDALONE_WIN  
using Steamworks;
using Steamworks.Data;
#endif

namespace CYM
{
    public class BaseSteamSDKMgr : BasePlatSDKMgr
    {
#if UNITY_STANDALONE_WIN  

        #region life
        public override void OnBeAdded(IMono mono)
        {
            base.OnBeAdded(mono);
            uint appId = GetAppId();
            // 使用try防止崩溃
            try
            {
                SteamClient.Init(appId);
            }
            catch (Exception e)
            {
                CLog.Info("Error starting steam client: {0}", e);
                SteamClient.Shutdown();
            }
            if (SteamClient.IsValid)
            {
                IsSDKInited = true;
            }
            else
            {
                SteamClient.Shutdown();
            }
        }
        public override void OnEnable()
        {
            base.OnEnable();
            SteamUserStats.OnAchievementProgress += OnAchievementProgress;
        }

        public override void OnStart()
        {
            base.OnStart();
            RefreshFriends();
        }
        public override void OnDisable()
        {
            SteamUserStats.OnAchievementProgress -= OnAchievementProgress;
            SteamClient.Shutdown();
            base.OnDisable();
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            SteamClient.RunCallbacks();
        }
        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
        }
        public override void OnDestroy()
        {
            SteamClient.Shutdown();
            base.OnDestroy();
        }
        #endregion

        #region is
        /// <summary>
        /// 是否支持这类语言
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        protected override bool IsSupportLanguage(string lang)
        {
            if (SteamApps.AvailableLanguages == null)
                return false;
            if (SteamApps.AvailableLanguages.Length == 0)
                return false;
            return SteamApps.AvailableLanguages[0].Contains(lang);
        }
        /// <summary>
        /// 是否为正版
        /// </summary>
        public override bool IsLegimit
        {
            get
            {
                if (BuildConfig.Ins.IsDevelop)return true;
                if (!SteamClient.IsValid)return false;
                if (IsDifferentAppId) return false;
                return true;
            }
        }
        /// <summary>
        /// 是否支持云存档
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportCloudArchive()
        {
            return IsLegimit;
        }
        /// <summary>
        /// 通过id判断此人是否为自己
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override bool IsSelf(ulong id)
        {
            return SteamClient.SteamId == id;
        }
        /// <summary>
        /// 是否支持平台UI
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportPlatformUI()
        {
            return IsLegimit;
        }
        /// <summary>
        /// 是否支持平台语言
        /// </summary>
        /// <returns></returns>
        public override bool IsSuportPlatformLanguage()
        {
            return true;
        }
        #endregion

        #region Get
        public override string GetName()
        {            
            return SteamClient.Name;
        }
        public override ulong GetUserID()
        {
            return SteamClient.SteamId;
        }
        protected override uint GetAppId()
        {
            return BuildConfig.Ins.SteamAppID;
        }
        /// <summary>
        /// 本地APP文件路径
        /// </summary>
        /// <returns></returns>
        protected override string LocalAppIDFilePath()
        {
            return "steam_appid.txt";
        }
        /// <summary>
        /// 得到错误信息
        /// </summary>
        /// <returns></returns>
        public override string GetErrorInfo()
        {
            if (IsDifferentAppId) return "The game is not activated, app found the different app id,do you changed any thing?";
            if (!IsLegimit) return "Unable to connect to Steam\n" + Util.GetStr("Msg_需要连接Steam");
            if (!SteamClient.IsValid) return "The game is not activated";
            return "Error";
        }
        /// <summary>
        /// 得到云存档路径
        /// </summary>
        /// <returns></returns>
        public override string GetCloudArchivePath()=> Application.persistentDataPath;
        /// <summary>
        /// steam平台
        /// </summary>
        public override Distribution DistributionType => Distribution.Steam;
        public override string GetCurLanguageStr()
        {
            if(IsSDKInited)
                return SteamApps.GameLanguage;
            return "";
        }
        #endregion

        #region Set
        public override void Purchase()
        {
            string orderID = IDUtil.GenOrderNumber();
            string key = BuildConfig.Ins.SteamWebAPI;
            //void SendUserInfo()
            //{
            //    CLog.Info("开始发送");
            //    string str = "key="+ key + "&steamid=" + GetUserID().ToString();
            //    var result = HTTPUtil.SendGetHttp("https://partner.steam-api.com/ISteamMicroTxn/GetUserInfo/v2/?" + str,null);
            //    CLog.Info(result.ToString());
            //}

            //void GetUserInfo()
            //{
            //    CLog.Info("开始发送");
            //    string str = "key=" + key + "&steamid=" + GetUserID().ToString();
            //    var result = HTTPUtil.SendGetHttp("https://partner.steam-api.com/ISteamMicroTxn/GetUserInfo/v2/" + str,null);
            //    CLog.Info(result.ToString());
            //}

            //void SendInitTxn()
            //{
            //    Dictionary<string, string> dic = new Dictionary<string, string>();
            //    dic.Add("key", key);
            //    dic.Add("orderid", orderID.ToString());
            //    dic.Add("steamid", GetUserID().ToString());
            //    dic.Add("appid", GetAppId().ToString());
            //    dic.Add("itemcount", "1");
            //    dic.Add("language", GetCurLanguageStr());
            //    dic.Add("currency", "CNY");
            //    dic.Add("itemid[0]", IDUtil.GenString());
            //    dic.Add("qty[0]", "1");
            //    dic.Add("amount[0]", "100");
            //    dic.Add("description[0]", "此物品非常凶残");
            //    var result = HTTPUtil.SendPostHttp("https://partner.steam-api.com/ISteamMicroTxnSandbox/InitTxn/v3/", dic);
            //    CLog.Info(result.ToString());

            //}

            //void SendFinalizeTxn()
            //{
            //    Dictionary<string, string> dic = new Dictionary<string, string>();
            //    dic.Add("key", key);
            //    dic.Add("orderid", orderID);
            //    dic.Add("appid", GetAppId().ToString());
            //    var result = HTTPUtil.SendPostHttp("https://partner.steam-api.com/ISteamMicroTxnSandbox/FinalizeTxn/v2/", dic);
            //    CLog.Info(result.ToString());
            //}
        }
        #endregion

        #region Callback
        #endregion

        #region 成就
        public override void RandomTriggerAllAchievement()
        {
            ResetAllAchievement();
            foreach (var item in SteamUserStats.Achievements)
            {
                bool b = RandUtil.Rand(0.5f);
                if (b)
                    item.Trigger(b);
            }
        }
        public override void ResetAllAchievement()
        {
            foreach (var item in SteamUserStats.Achievements)
            {
                item.Clear();
            }
        }
        public override void TriggerAchievement(TDBaseAchieveData data)
        {
            //Client?.Achievements.Trigger(data.TDID, true);
        }
        public override void ResetAchievement(TDBaseAchieveData data)
        {
            //Client?.Achievements.Reset(data.TDID);
        }
        public override void RefreshAchievements()
        {
            foreach (var item in SteamUserStats.Achievements)
            {
                if (Achievements.ContainsKey(item.Identifier))
                {

                }
                else
                {

                    var tempData = GetAchieveData.Copy<TDBaseAchieveData>();
                    {
                        tempData.State = item.State;
                        tempData.UnlockTime = item.UnlockTime.Value;
                        tempData.SourceName = item.Name;
                        tempData.SourceDesc = item.Description;
                    };
                    if (tempData == null)
                    {
                        CLog.Error("没有配置这个成就:{0}", item.Identifier);
                        continue;
                    }
                    Achievements.Add(item.Identifier, tempData);
                }
            }
        }
        private void OnAchievementProgress(Achievement arg1, int arg2, int arg3)
        {
            RefreshAchievements();
            Callback_OnFatchAchievementSuccess?.Invoke();
        }
        #endregion

        #region 统计
        public override void SetStats(string id, float val)
        {
            SteamUserStats.SetStat(id, val);
        }
        public override void SetStats(string id, int val)
        {
            SteamUserStats.SetStat(id, val);
        }
        public override void AddStats(string id, int val)
        {
            SteamUserStats.AddStat(id, val);
        }
        public override float GetFloatStats(string id)
        {
            return SteamUserStats.GetStatFloat(id);
        }
        public override int GetIntStats(string id)
        {
            return SteamUserStats.GetStatInt(id);
        }
        public override void StoreStats()
        {
            SteamUserStats.StoreStats();
        }
        #endregion

        #region 好友
        public override void RefreshFriends()
        {
            //Friends.Clear();
            //FriendsID.Clear();
            //var friend = SteamFriends.GetFriends();
            //if (friend == null)
            //    return;
            //foreach (var item in friend)
            //{
            //    var temp = new PlatFriend()
            //    {
            //        Name = item.Name,
            //        ID = item.Id,
            //        IsPlayingThisGame = item.IsPlayingThisGame,
            //        IsSnoozing = item.IsSnoozing,
            //        IsBusy = item.IsBusy,
            //        IsAway = item.IsAway,
            //        IsOnline = item.IsOnline,
            //        IsFriend = item.IsFriend,
            //        IsBlocked = item.IsBlocked,

            //    };
            //    Friends.Add(temp);
            //    FriendsID.Add(temp.ID);
            //}
            //Friends.Sort((X, Y) =>
            //{
            //    if (X.IsOnline && !Y.IsOnline)
            //        return -1;
            //    return 1;
            //});
        }

        public override bool IsFriend(ulong id)
        {
            return FriendsID.Contains(id);
        }
        #endregion

        #region overlay
        public override void OpenAchievement(ulong id)
        {
            //SteamFriends.OpenOverlay(,id);
        }
        public override void OpenChat(ulong id)
        {
            //Client?.Overlay.OpenChat(id);
        }
        public override void OpenProfile(ulong id)
        {
            //Client?.Overlay.OpenProfile(id);
        }
        public override void OpenStats(ulong id)
        {
            //Client?.Overlay.OpenStats(id);
        }
        public override void OpenTrade(ulong id)
        {
            //Client?.Overlay.OpenTrade(id);
        }
        public override void OpenAddFriend(ulong id)
        {
            //Client?.Overlay.AddFriend(id);
        }
        public override void OpenURL(string URL)
        {
            if (Application.isEditor)
            {
                base.OpenURL(URL);
            }
            else
            {
                SteamFriends.OpenWebOverlay(URL,true);
            }
        }
        #endregion

        #region shop
        public override void GoToShop()
        {
        }
        #endregion

        #region leader board
        Dictionary<string, Leaderboard> tempLeaderboards = new Dictionary<string, Leaderboard>();
        string curRefreshLeaderBoardId;
        Leaderboard curLeaderBoard;
        public override void RefreshLeaderBoard(string id = "")
        {
            curRefreshLeaderBoardId = id;
            curLeaderBoard = GetLeaderboard(id);
            BaseGlobal.CommonCorouter.Run(_UpdateFetchLeaderBoard());
        }
        IEnumerator<float> _UpdateFetchLeaderBoard()
        {
            yield break;
            //yield return Timing.WaitUntilTrue(() => { return curLeaderBoard.is; });
            //curLeaderBoard.FetchScores(
            //    Leaderboard.RequestType.GlobalAroundUser, 0, 20,
            //    OnFetchScoreSuccess,
            //    OnFecthScoreFaild);
        }
        public override void SetScore(string leaderBoardId, int step = 1)
        {
            //Leaderboard temp = GetLeaderboard(leaderBoardId);
            //if (temp == null)
            //    return;
            //temp.AddScore(true, step, null, OnAddScoreSuccess, OnAddScoreFail);
        }

        private Leaderboard GetLeaderboard(string leaderBoardId)
        {
            return default;
            //Leaderboard temp = null;
            //if (Client == null)
            //    return temp;
            //if (tempLeaderboards.ContainsKey(leaderBoardId))
            //{
            //    temp = tempLeaderboards[leaderBoardId];
            //}
            //else
            //{
            //    temp = Client.GetLeaderboard(leaderBoardId, Client.LeaderboardSortMethod.Ascending, Client.LeaderboardDisplayType.Numeric);
            //    tempLeaderboards.Add(leaderBoardId, temp);
            //}
            //return temp;
        }
        //void OnFetchScoreSuccess(Entry[] results)
        //{
        //    PlatLeaderBoard tempLeaderBoard;
        //    if (LeaderBoards.ContainsKey(curRefreshLeaderBoardId))
        //    {
        //        tempLeaderBoard = LeaderBoards[curRefreshLeaderBoardId];
        //        tempLeaderBoard.Items.Clear();
        //    }
        //    else
        //    {
        //        tempLeaderBoard = new PlatLeaderBoard();
        //        LeaderBoards.Add(curRefreshLeaderBoardId, tempLeaderBoard);
        //    }
        //    if (results != null)
        //    {
        //        foreach (var item in results)
        //        {
        //            var newItem = new PlatLeaderBoardItem()
        //            {
        //                Name = item.Name,
        //                Score = item.Score,
        //                GlobalRank = item.GlobalRank,
        //                Id = item.SteamId,
        //            };
        //            tempLeaderBoard.Items.Add(newItem);
        //        }
        //    }
        //    Callback_OnFatchScoreSuccess?.Invoke(tempLeaderBoard);
        //}
        //void OnFecthScoreFaild(Result result)
        //{
        //    Callback_OnFatchScoreFaild?.Invoke();
        //}
        //private void OnAddScoreFail(Result reason)
        //{
        //    CLog.Error("AddScore Fail :" + reason.ToString());
        //}

        //private void OnAddScoreSuccess(AddScoreResult result)
        //{

        //}
        #endregion

        #region test
        protected override void Test()
        {
        }
        #endregion
#endif

    }

}