using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CYM
{
    [Serializable]
    public partial class Config : TDBaseData
    {
        public static DateTimeAgeType StartDateTimeAgeType { get; set; } = DateTimeAgeType.BC;
        public static DateTime StartDateTime { get; set; } = new DateTime(1, 1, 1);

        public static string StartMusics { get; set; } = "MainMenu";
        public static string BattleMusics { get; set; } = "Battle";
        public static string CreditsMusics { get; set; } = "Credits";
    }
}