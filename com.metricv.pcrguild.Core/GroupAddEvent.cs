using Native.Sdk.Cqp.Enum;
using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.metricv.pcrguild.Code {
    public class GroupAddEvent : IGroupAddRequest {

        /**
         * For some reason, this method does not always get called on given events.
         * As a fix, all group managers are grant the permission when they execute a command.
         */ 
        public void GroupAddRequest(object sender, CQGroupAddRequestEventArgs e) {
            if(e.SubType == CQGroupAddRequestType.RobotBeInviteAddGroup) {
                e.CQLog.Info("Info.AddGroup", $"机器人被邀请进群 {e.FromGroup?.Id ?? -1}");
                e.Request.SetGroupAddRequest(CQGroupAddRequestType.RobotBeInviteAddGroup, CQResponseType.PASS);

                DBManager.addManager(e.FromGroup.Id, e.FromQQ.Id);
                e.FromGroup.SendGroupMessage($"被 {e.FromQQ.Id} 邀请进群，已设为插件管理。");
            }
        }
    }
}
