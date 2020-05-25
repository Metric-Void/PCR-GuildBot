﻿using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;

namespace com.metricv.pcrguild.Code {
    public class InitEventHandlerDB : IAppEnable, IAppDisable {
        public void AppDisable(object sender, CQAppDisableEventArgs e) {
            DBManager.down(e);
        }

        public void AppEnable(object sender, CQAppEnableEventArgs e) {
            ConfigHandler.loadConfig(e);

            String dbFile = e.CQApi.AppDirectory + "com.metricv.pcrguild.db";
            e.CQLog.Info("Info.Init","com.metricv.pcrguild is initializing...");

            DBManager.up(e);
            DBManager.addManager();

            e.CQLog.Info("Info.Init", "com.metricv.pcrguild initialized");
        }
    }
}