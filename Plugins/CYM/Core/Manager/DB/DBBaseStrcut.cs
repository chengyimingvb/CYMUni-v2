//**********************************************
// Class Name	: UnitSurfaceManager
// Discription	：None
// Author	：CYM
// Team		：MoBaGame
// Date		：#DATE#
// Copyright ©1995 [CYMCmmon] Powered By [CYM] Version 1.0.0 
//**********************************************
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CYM
{
    [Serializable]
    public class DBBaseSettings
    {
        /// <summary>
        /// 摄像机移动速度
        /// </summary>
        public float CameraMoveSpeed = 0.5f;
        /// <summary>
        /// 摄像机滚动速度
        /// </summary>
        public float CameraScrollSpeed = 0.5f;
        /// <summary>
        /// 是否为简单地形
        /// </summary>
        public bool IsSimpleTerrin = false;
        /// <summary>
        /// 语言类型
        /// </summary>
        public LanguageType LanguageType = LanguageType.Chinese;
        /// <summary>
        /// 禁止背景音乐
        /// </summary>
        public bool MuteMusic = false;
        /// <summary>
        /// 禁止音效
        /// </summary>
        public bool MuteSFX = false;
        /// <summary>
        /// 静止所有音乐
        /// </summary>
        public bool Mute = false;
        /// <summary>
        /// 是否静止环境音效
        /// </summary>
        public bool MuteAmbient = false;
        /// <summary>
        /// 背景音乐音量
        /// </summary>
        public float VolumeMusic = 0.2f;
        /// <summary>
        /// 音效音量
        /// </summary>
        public float VolumeSFX = 1.0f;
        /// <summary>
        /// 主音量
        /// </summary>
        public float Volume = 1.0f;
        /// <summary>
        /// 语音音量
        /// </summary>
        public float VolumeVoice = 0.5f;
        /// <summary>
        /// 自动存储类型
        /// </summary>
        public bool IsAutoSave = true;
        public bool IsTGS = true;
        /// <summary>
        /// 是否开启后期特效
        /// </summary>
        public bool EnablePostProcess = true;
        /// <summary>
        /// 游戏画质
        /// </summary>
        public GamePropType Quality = GamePropType.Hight;
        /// <summary>
        /// 游戏分辨率,通常选择小一号的窗口模式
        /// </summary>
        public int Resolution = 1;
        /// <summary>
        /// 全屏
        /// </summary>
        public WindowType WindowType = WindowType.Windowed;
    }
    [Serializable]
    public class DBBaseGame
    {
        public string PlayerID = Const.STR_Inv;
        public string BattleID = Const.STR_Inv;
        public int PlayTime = 0;
        public int LoadBattleCount = 0;
        public int TurnCount = 0;
        public DateTime CurDateTime = new DateTime(1, 1, 1);
        public DateTimeAgeType CurDateTimeAgeType = DateTimeAgeType.BC;
        public GameNetMode GameNetMode = GameNetMode.PVE;
        public GamePlayStateType GamePlayStateType = GamePlayStateType.NewGame;
        public bool IsNewGame() => GamePlayStateType == GamePlayStateType.NewGame;
        public bool IsLoadGame() => GamePlayStateType == GamePlayStateType.LoadGame;
        public bool IsFirstLoadBattle() => LoadBattleCount == 1;
    }

    #region base
    [Serializable]
    public class DBBase
    {
        public long ID = Const.LONG_Inv;
        public string TDID = Const.STR_Inv;
        public string CustomName = Const.STR_Inv;

        public bool IsInv()
        {
            return TDID.IsInv() && ID.IsInv();
        }
    }
    [Serializable]
    public class DBBaseUnit : DBBase
    {
        public Vec3 Position = new Vec3(new Vector3(99999.0f, 99999.0f, 99999.0f));
        public Qua Rotation = new Qua(Quaternion.identity);
        public bool IsNewAdd = false;
    }
    [Serializable]
    public class DBBaseBuff : DBBase
    {
        public float CD = 0;
        public float Input;
        public bool Valid = true;
    }
    [Serializable]
    public class DBBaseWar : DBBase
    {
        public List<string> Attackers = new List<string>();
        public List<string> Defensers = new List<string>();
        public HashList<string> AllowOccupy = new HashList<string>();
        public int WarDay = 0;
        public float AttackersWarPoint;
        public float DefensersWarPoint;
    }
    [Serializable]
    public class DBBaseAlert : DBBase
    {
        public float CurTurn;
        public bool IsCommingTimeOutFalg;
        public long Cast;
        public string TipStr;
        public string DetailStr;
        public string TitleStr;
        public string Illustration;
        public AlertType Type = AlertType.Continue;
        public List<long> SelfArticle = new List<long>();
        public List<long> TargetArticle = new List<long>();
        public long War = Const.INT_Inv;
        public string StartSFX;
        public bool IsAutoTrigger;
        public string Bg;
        public string Icon;
    }
    [Serializable]
    public class DBBaseArticle : DBBase
    {
        public long Self;
        public long Target;

        public float Float1;
        public float Float2;
        public float Float3;
        public int Int1;
        public int Int2;
        public int Int3;
        public string Str1;
        public string Str2;
        public string Str3;
        public bool Bool1;
        public bool Bool2;
        public bool Bool3;
        public long Long1;
        public long Long2;
        public long Long3;

        public ArticleObjType ArticleObjType = ArticleObjType.Self;
    }
    [Serializable]
    public class DBBaseEvent : DBBase
    {
        public int CD;
    }
    [Serializable]
    public class DBBaseProposal : DBBase
    {
        public int CD;
        public long Person;
        public int CurExpireCD;
        public bool IsAccept = false;
    }
    [Serializable]
    public class DBBaseTransact : DBBase
    {
        public string Type;
        public int CD;
        public long Self;
        public long Target;
        public float Value;
    }
    [Serializable]
    public class DBBaseLegionStationed : DBBase
    {
        public long DefendCastle = Const.INT_Inv;
        public long PreDefendCastle = Const.INT_Inv;
        public long SiegeCastle = Const.INT_Inv;
        public long PreSiegeCastle = Const.INT_Inv;
    }
    [Serializable]
    public class DBBaseCastleStationed : DBBase
    {
        public long DefendLegion = Const.INT_Inv;
        public long PreDefendLegion = Const.INT_Inv;
        public List<long> AttackLegion = new List<long>();
    }
    [Serializable]
    public class DBBaseLoan : DBBase
    {
        public List<LoanData> Loan = new List<LoanData>();
        public float CurLoan = 0;
    }
    [Serializable]
    public class DBBaseTargetMarker : DBBase
    {
        public long Unit = Const.INT_Inv;
        public long Target = Const.INT_Inv;
        public float CD =Const.FLOAT_Inv;
    }
    [Serializable]
    public class DBBaseNarration : DBBase
    {
        public HashList<string> Showed = new HashList<string>();
    }
    [Serializable]
    public class DBBaseStaffGroup
    {
        public long Monarch = Const.LONG_Inv;
        public long Queen = Const.LONG_Inv;
        public long Deputy = Const.LONG_Inv;
        public long Crown = Const.LONG_Inv;
        public long Diplomat = Const.LONG_Inv;
        public long Spy = Const.LONG_Inv;
        public List<long> Prince = new();
        public List<long> Harem = new();
        public List<long> Candidate = new();
        public List<long> Beauty = new();
        public List<long> Hangeron = new();
        public List<long> Jailer = new();
        public List<long> Captive = new();
        public Dictionary<int, long> Official = new();
        public Dictionary<long, long> General = new();
        public Dictionary<long, long> Prefect = new();
        public Dictionary<string, long> Merchant = new();
        public List<long> Persons = new();
        public float CurStruggleProp = 0;
    }
    [Serializable]
    public class DBBasePerson : DBBase
    {
        public string Theory = Const.STR_Inv;
        public string Civil = "Name_中原";
        public string FirstName = Const.STR_None;
        public string LastName = Const.STR_None;
        public Gender Gender = Gender.Male;
        public int Age = Const.INT_Inv;
        public AgeRange AgeRange = AgeRange.Adult;
        public Dictionary<PHIPart, string> HeadIcon = new Dictionary<PHIPart, string>();
        public Dictionary<PHIPart, string> ChildHeadIcon = new Dictionary<PHIPart, string>();
        public Dictionary<PHIPart, string> OldHeadIcon = new Dictionary<PHIPart, string>();
    }
    [Serializable]
    public class DBBaseStaff : DBBasePerson
    {
        #region Runtime prop
        public HashList<StaffType> Staffs = new HashList<StaffType>();
        #endregion

        #region Config Attr
        public int Adm = Const.INT_Inv;
        public int Inte = Const.INT_Inv;
        public int Fire = Const.INT_Inv;
        public int Shock = Const.INT_Inv;
        public int Impact = Const.INT_Inv;
        public int Tactic = Const.INT_Inv;
        #endregion

        #region Config Attr
        public int Power = Const.INT_Inv;
        public int Loyalty = Const.INT_Inv;
        public string GeneralHelmet = Const.STR_Inv;
        public string MonarchHelmet = Const.STR_Inv;
        #endregion
    }
    [Serializable]
    public class DBBaseRegime : DBBase
    {
        public int CurTerm = 0;
    }
    #endregion
}