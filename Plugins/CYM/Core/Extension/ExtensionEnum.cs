//------------------------------------------------------------------------------
// ExtensionEnum.cs
// Created by CYM on 2021/9/13
// 填写类的描述...
//------------------------------------------------------------------------------

using System;

namespace CYM
{
    #region Attr
    // 消耗加城因子使用类型
    public enum UpFactionType
    {
        Percent,  // 百分比乘法 Val * InputVal * Faction + Add;
        PercentAdd,  // 百分比增加 Val * ((1 + InputVal) * Faction) + Add;
        LinerAdd, // 线性附加 Val + InputVal * Faction + Add;
        PowAdd,   // 指数附加 Pow(InputVal, Faction) + Add;
    }
    // 属性加成因子使用的类型
    public enum AttrFactionType
    {
        None,       //忽略加成因子
        DirectAdd,     //直接增加val+faction
        PercentAdd,    //百分比增加val*(1+faction)
        DirectMul,     //直接相乘val*faction
    }
    // 条件类型
    public enum ACCType
    {
        And,
        Or,
    }
    // 条件比较
    public enum ACCompareType
    {

        More,       // 大于
        MoreEqual,  // 大于等于  
        Less,       // 小于
        LessEqual,  // 小于等于
        Equal,      // 等于
        Have,       // 拥有
        NotHave,    // 没有
        Is,         // 是
        Not,        // 不是
    }
    // 数字类型
    public enum NumberType
    {
        Normal,     //正常显示,选择性2位小数
        D2,         //三位小数
        Percent,    //百分比 e.g. 10%,选择性1位小数
        KMG,        //单位显示 e.g. 1K/1M/1G,数字小于1K取整 
        Integer,    //取整
        Bool,       //布尔 0:false 1:true
    }
    // 属性操作类型
    public enum AttrOpType
    {
        DirectAdd = 0,  //直接累加
        PercentAdd,     //百分比累加
        Direct,         //直接赋值
        Percent,        //百分比赋值
    }
    // 属性类型
    public enum AttrType
    {
        Fixed,      // 固定值,比如最大的血量        
        Dynamic,    // 动态值,比如当前的血量
    }
    // 属性增益类型
    public enum AttrBuffType
    {
        Forward,
        Backward,
        Middle,
    }
    #endregion

    #region AStar 2D
    public enum BasicMoveType2D
    {
        None,
        MoveToNode,
        MoveToUnit,
        MoveIntoUnit,
    }
    public enum AgentState2D
    {
        //空闲
        Idle = 0,
        //跟随路径移动
        FollowingPath,
        //等待移动
        AwaitingFollowing,
    }

    [Flags]
    public enum AgentDirection2D
    {
        Forward = 1,
        Backward = 2,
        Left = 4,
        Right = 8,
        ForwardLeft = Forward | Left,
        ForwardtRight = Forward | Right,
        BackwardLeft = Backward | Left,
        BackwardRight = Backward | Right,
        Default = Forward,
    }
    #endregion

    #region Alert
    public enum AlertType
    {
        Continue,         //持续
        Interaction,      //交互
        Disposable,       //一次性
    }
    #endregion

    #region Article
    public enum ArticleDescType
    {
        Normal,
        Dialog,
        Hint,
    }
    public enum ArticleType
    {
        Give,
        Obtain,
    }
    public enum ArticleObjType
    {
        Self,
        Target,
    }
    #endregion

    #region DB
    public enum GameNetMode
    {
        PVP,
        PVE,
    }
    public enum GamePlayStateType
    {
        NewGame,//新游戏
        LoadGame,//加载游戏
        Tutorial,//教程
    }
    #endregion

    #region Balance
    public enum BalanceActionType
    {
        Hire,
        Fire,
    }
    #endregion

    #region BGM
    public enum BGMType
    {
        MainMenu,
        Battle,
        Credits,
    }
    #endregion

    #region Buff
    //buff 合并类型,BuffGroupID的优先级高于所有类型
    public enum BuffMergeType
    {
        None, // 重置CD
        CD,   // 增加CD  
        Group,// 叠加
    }
    public enum RemoveBuffType
    {
        Once,//移除一层
        Group,//移除一组
    }
    #endregion

    #region Date Time
    public enum DateTimeAgeType
    {
        AD,
        BC,
    }
    #endregion

    #region UI
    public enum Corner : int
    {
        BottomLeft = 0,
        TopLeft = 1,
        TopRight = 2,
        BottomRight = 3,
    }
    public enum Anchoring
    {
        None,
        Corners,     //Normal
        LeftOrRight, //左右
        TopOrBottom, //上下
        Cant,//斜面
    }

    public enum Anchor
    {
        None = -1,
        BottomLeft,
        BottomRight,
        TopLeft,
        TopRight,
        Left,
        Right,
        Top,
        Bottom
    }


    #endregion

    #region Imm
    /// <summary>
    /// 增益或者减益类型
    /// </summary>
    public enum ImmuneGainType
    {
        All = 0,
        Negative = 1,//负面
        Positive = 2,//正面
    }
    /// <summary>
    /// 效果的免疫类型
    /// </summary>
    public enum ImmuneType
    {
        All = 0,
        Normal = 1, //一般类型:普攻
        Magic = 2,  //魔法类型:技能
    }
    #endregion

    #region Level
    public enum LevelLoadType
    {
        Scene, //直接加载场景
        Prefab,//加载Prefab
    }
    #endregion

    #region Loader
    public enum LoadEndType
    {
        Success = 0,
        Failed = 1,
    }
    #endregion

    #region Perform
    public enum PerformFollowType
    {
        None = 0,//没有任何跟随
        Self = 1,//指定位置
        Follow = 3,//跟随
        Attach = 4,//附加在某个点
    }
    public enum PerformRotateType
    {
        None = 0,
        Normal = 1, //基于配置输入的角度
        Self = 2,   //基于载体角度
        Caster = 3, //基于释放者的角度
        CasterH = 4,//基于释放者的角度,适用于横版
    }
    public enum PerformScaleType
    {
        None = 0,//没有缩放
        Volume = 1,//更具单位的体积值缩放
        Width = 2,
        High = 3,

    }
    public enum PerformDispearType
    {
        None,
        Flicker,
        Scale,
    }
    public enum NodeType
    {
        None = -1,
        Footsteps = 0,
        Breast = 1,
        Spine = 2,
        LFoot = 3,
        RFoot = 4,
        Head = 5,
        LHand = 6,
        RHand = 7,

        //虚拟点位
        Top = 8,
        Center = 9,
        Pivot = 10,

        //可选骨骼,如果没有,则不设置
        Muzzle = 20,//发射口
    }
    #endregion

    #region UI
    public enum SaveOrLoad
    {
        Save,
        Load,
    }
    #endregion

    #region Skill
    public enum SkillInteraputType
    {
        /// <summary>
        /// 不可被打断
        /// </summary>
        None = 0,
        /// <summary>
        /// 移动打断
        /// </summary>
        Move = 1,
        /// <summary>
        /// 技能打断
        /// </summary>
        Skill = 2,
        /// <summary>
        /// 攻击打断
        /// </summary>
        Attack = 4,
        /// <summary>
        /// 被动打断，比如晕眩，击飞
        /// </summary>
        Passive = 8,
    }
    public enum SkillPhase
    {
        None = 0,
        Hold = 1,
        Start = 2,
        Cast = 4,
        CastEnd = 8,
    }
    public enum SkillReleaseDataType
    {
        /// <summary>
        /// 对点释放
        /// </summary>
        Point = 1,
        /// <summary>
        /// 对目标释放
        /// </summary>
        Target = 2,
    }
    public enum SkillUseType
    {
        /// <summary>
        /// 瞬发类型：点击立即释放
        /// </summary>
        Instant = 0,
        /// <summary>
        /// 指示器类型，使用后的操作委托给指示器
        /// </summary>
        Pointer = 1,
        /// <summary>
        /// 被动
        /// </summary>
        Passive = 2,
    }

    public enum SkillOpType
    {
        Attack = 0,
        Skill = 1,
    }

    public enum SkillReleaseResult
    {
        None,
        Succ,
        UnCheckDistance,
        UnCheckCost,
        UnCheckTarget,
        UnCheckCD,
        UnCheckUnlock,
        UnCheckState,
        Dead,
    }

    #endregion

    #region Misc
    public enum TalkType
    {
        Left,
        Right,
        Mid,
    }
    public enum WindowType
    {
        Fullscreen,
        Windowed,
    }
    [Serializable]
    public enum CastleType
    {
        System,
        City,  //城市
        Town,  //城镇 
        Fort,//城堡
        Vill, //村庄
    }
    public enum CameraHightType
    {
        Less,
        Low,
        Mid,
        More,
        Top,
        Most,
    }
    #endregion
}