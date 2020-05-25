using Native.Sdk.Cqp.EventArgs;
using Native.Tool.IniConfig;
using Native.Tool.IniConfig.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Mapping;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.metricv.pcrguild.Code {
    class ConfigHandler {
        public static long master_qq { get; set; }

        public static void loadConfig(CQEventArgs e) {
            String iniFile = e.CQApi.AppDirectory + "config.ini";
            if(!File.Exists(iniFile)) {
                File.Create(iniFile).Close();
                IniConfig iniConfig = new IniConfig(iniFile);
                iniConfig.Object["Master"] = new ISection("Master") {
                    {"MasterQQ", 0}
                };
                iniConfig.Save();
                e.CQLog.Info("Info.Init", "config.ini created. Please update.");
            } else {
                IniConfig iniConfig = new IniConfig(iniFile);
                try {
                    //master_qq = 
                    e.CQLog.Info("Debug", iniConfig.Load());
                    e.CQLog.Info("Debug", iniConfig.Object["Master"].TryGetValue("MasterQQ", out IValue value));
                    e.CQLog.Info("Debug", value.ToString());
                    ConfigHandler.master_qq = value.ToInt64();
                    e.CQLog.Info("Config Loaded. Master is " + master_qq.ToString());
                } catch {
                    e.CQLog.Error("Info.Init", "Error reading config.ini");
                }
            }
        }
    }
}
