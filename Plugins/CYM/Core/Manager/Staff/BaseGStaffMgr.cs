//------------------------------------------------------------------------------
// BaseGStaffMgr.cs
// Created by CYM on 2022/6/10
// 填写类的描述...
//------------------------------------------------------------------------------

using UnityEngine;
using CYM;
using CYM.UI;
using CYM.AI;
using System.Collections.Generic;
/// <summary>
/// 模拟古代官职的管理器,GlobalMgr
/// </summary>
namespace CYM
{
    public class BaseGStaffMgr<TData> : BasePersonMgr<TData>, IDBListConverMgr<DBBaseStaff>
        where TData : TDBaseStaffData, new()
    {
        protected override bool IsNoConfig => true;

        #region DB
        public void LoadDBData(ref List<DBBaseStaff> data)
        {
            LoadDBData(ref data, (config, data) =>
            {
                CopyToConfig(data, config);

                config.Staffs = data.Staffs;

                config.Adm = data.Adm;
                config.Inte = data.Inte;
                config.Fire = data.Fire;
                config.Shock = data.Shock;
                config.Impact = data.Impact;
                config.Tactic = data.Tactic;

                config.Power = data.Power;
                config.Loyalty = data.Loyalty;
                config.GeneralHelmet = data.GeneralHelmet;
                config.MonarchHelmet = data.MonarchHelmet;
            });
        }
        public void SaveDBData(ref List<DBBaseStaff> data)
        {
            SaveDBData(ref data, (config, data) =>
            {
                CopyToDBData(config,data);

                data.Staffs = config.Staffs;

                data.Adm = config.Adm;
                data.Inte = config.Inte;
                data.Fire = config.Fire;
                data.Shock = config.Shock;
                data.Impact = config.Impact;
                data.Tactic = config.Tactic;

                data.Power = config.Power;
                data.Loyalty = config.Loyalty;
                data.GeneralHelmet = config.GeneralHelmet;
                data.MonarchHelmet = config.MonarchHelmet;
            });
        }
        #endregion
    }
}