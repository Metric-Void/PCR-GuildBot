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
                e.CQLog.Info("工会战排刀器", "生成了config.ini,请更新其中的信息。");
            } else {
                IniConfig iniConfig = new IniConfig(iniFile);
                try {
                    iniConfig.Object["Master"].TryGetValue("MasterQQ", out IValue value);
                    e.CQLog.Info("Debug", value.ToString());
                    ConfigHandler.master_qq = value.ToInt64();
                    e.CQLog.Info("工会战排刀器", "配置已加载，master是" + master_qq.ToString());
                } catch {
                    e.CQLog.Error("工会战排刀器", "读取config.ini时发生错误");
                }
            }
        }
    }
}
