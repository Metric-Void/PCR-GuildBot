using Native.Sdk.Cqp.EventArgs;
using Native.Sdk.Cqp.Interface;
using Native.Sdk.Cqp.Model;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace com.metricv.pcrguild.Code {
    public class MessageDigestor : IPrivateMessage, IGroupMessage, IDiscussMessage {
        Regex rx_normalcmd = new Regex(@"[公工行]会战#(?<cmd>.*)");
        public void DiscussMessage(object sender, CQDiscussMessageEventArgs e) {
            if (e.Message.IsRegexMessage) {
                String cmd = "";
                e.Message.RegexResult.TryGetValue("cmd", out cmd);
                e.CQLog.Debug("Expected", $"cmd is {cmd}");
                e.CQApi.SendDiscussMessage(e.FromDiscuss, replyRaw("[CQ:at,qq=" + e.FromQQ.ToString() + "]", cmd, e.FromQQ.Id, e.FromDiscuss.Id));
            } else {
                String msg = e.Message;
                if (rx_normalcmd.IsMatch(msg)) {
                    DBManager.addGroupRelation(e.FromDiscuss, e.FromQQ);
                    Match matches = rx_normalcmd.Match(msg);
                    String cmd = matches.Groups["cmd"].Value;
                    e.CQLog.Debug("Expected", $"cmd is {cmd}");
                    e.CQApi.SendDiscussMessage(e.FromDiscuss, replyRaw("[CQ:at,qq=" + e.FromQQ.ToString() + "]", cmd, e.FromQQ.Id, e.FromDiscuss.Id));
                }
            }
        }

        public void GroupMessage(object sender, CQGroupMessageEventArgs e) {
            if (e.Message.IsRegexMessage) {
                String cmd = "";
                e.Message.RegexResult.TryGetValue("cmd", out cmd);
                e.CQLog.Debug("Expected", $"cmd is {cmd}");
                e.CQApi.SendGroupMessage(e.FromGroup, replyRaw("[CQ:at,qq="+e.FromQQ.ToString()+"]", cmd, e.FromQQ.Id, e.FromGroup.Id));
            } else {
                String msg = e.Message;
                if(rx_normalcmd.IsMatch(msg)) {
                    DBManager.addGroupRelation(e.FromGroup, e.FromQQ);
                    Match matches = rx_normalcmd.Match(msg);
                    String cmd = matches.Groups["cmd"].Value;
                    e.CQLog.Debug("Expected", $"cmd is {cmd}");
                    e.CQApi.SendGroupMessage(e.FromGroup, replyRaw("[CQ:at,qq=" + e.FromQQ.ToString() + "]", cmd, e.FromQQ.Id, e.FromGroup.Id));
                }
            }
        }

        public void PrivateMessage(object sender, CQPrivateMessageEventArgs e) {
            if (e.Message.IsRegexMessage) {
                String cmd = "";
                e.Message.RegexResult.TryGetValue("cmd", out cmd);
                e.CQLog.Debug("Expected", $"cmd is {cmd}");
                e.CQApi.SendPrivateMessage(e.FromQQ, replyRaw("", cmd, e.FromQQ.Id, 0));
            } else {
                String msg = e.Message;
                if (rx_normalcmd.IsMatch(msg)) {
                    Match matches = rx_normalcmd.Match(msg);
                    String cmd = matches.Groups["cmd"].Value;
                    e.CQLog.Debug("Expected", $"cmd is {cmd}");
                    e.CQApi.SendPrivateMessage(e.FromQQ, replyRaw("", cmd, e.FromQQ.Id, 0));
                }
            }
        }

        Regex rx_addRecord = new Regex(@"报战\s+队伍=(?<team>.+?)\s*(周目=(?<shukai>\d{1,5}))?\s*(BOSS=(?<boss>\d{1,3}))?\s*伤害=(?<damage>\d+)");
        Regex rx_seeRecord = new Regex(@"(查询\s+(队伍=(?<team>\S+))?\s*(周目=(?<shukai>\d{1,5}))?\s*(BOSS=(?<boss>\d{1,3}))?|查询$)");
        Regex rx_seeAllRecord = new Regex(@"(查[询看]所有\s+(队伍=(?<team>\S+))*\s*(周目=(?<shukai>\d{1,5}))*\s*(BOSS=(?<boss>\d{1,3}))*|查[询看]所有$)");
        Regex rx_rmvRecord = new Regex(@"(删除\s+(队伍=(?<team>\S+))?\s*(周目=(?<shukai>\d{1,5}))?\s*(BOSS=(?<boss>\d{1,3}))*\s*(伤害=(?<damage>\d+))?|清空$)");
        Regex rx_sumRecord = new Regex(@"统计\s+队伍=(?<team>.+)\s*(周目=(?<shukai>\d{1,5}))?\s*(BOSS=(?<boss>\d{1,3}))?");
        Regex rx_teamList = new Regex(@"队伍列表$");
        Regex rx_addAdmin = new Regex(@"添加群管\s*((\d+)|(\[CQ:at,qq=(\d+)\]))");
        Regex rx_arrange = new Regex(@"排刀\s*((?<percent>\d+)%)?\s*(周目=(?<shukai>\d{1,5}))?\s*(BOSS=(?<boss>\d{1,3}))?\s*残血=(?<rem>\d+)");
        Regex rx_removeAffinity = new Regex(@"解绑$");
        Regex rx_groupDefault = new Regex(@"预设\s*周目=(?<shukai>\d{1,5})\s*BOSS=(?<boss>\d{1,3})");
        private void proc_addRecord(ref StringBuilder sb, String cmd, long fromQQ, long fromGroup) {
            sb.AppendLine("登记战况中...");
            sb.AppendLine("===========");
            Match matches = rx_addRecord.Match(cmd);
            String team = matches.Groups["team"].Value;
            int shukai = -1;
            int boss = -1;
            int damage = 0;
            bool error = false;
            if(fromGroup != 0) {
                DBManager.getDefault(fromGroup, out shukai, out boss);
            }

            int shukai_override;
            int boss_override;

            if((matches.Groups["shukai"]!=null) && int.TryParse(matches.Groups["shukai"].Value, out shukai_override)) {
                shukai = shukai_override;
            }
            if ((matches.Groups["boss"] != null) && int.TryParse(matches.Groups["boss"].Value, out boss_override)) {
                boss = boss_override;
            }

            if (shukai == -1) {
                sb.AppendLine("周回读取失败，请给出一个数字。");
                sb.AppendLine("（周回数限制在五位数以内... 超过五位数的话我也不知道该说啥了");
                error = true;
            }
            if (boss == -1) {
                sb.AppendLine("BOSS读取失败，请给出一个数字。");
                sb.AppendLine("（BOSS序号限制在三位数以内）");
                error = true;
            }
            if (!int.TryParse(matches.Groups["damage"].Value, out damage)) {
                sb.AppendLine("伤害读取失败，请给出一个数字。");
                error = true;
            }
            if (error) {
                sb.AppendLine("至少一项读取失败，拒绝记录。");
                sb.AppendLine("===========");
                return;
            }

            sb.AppendLine($"队伍名：{team}");
            sb.AppendLine($"周回：{shukai}");
            sb.AppendLine($"Boss序号：{boss}");
            sb.AppendLine($"造成伤害：{damage}");
            sb.AppendLine("===========");
            try {
                long count = DBManager.addBattleRecord(fromQQ, team, shukai, boss, damage);
                if (count > 0) {
                    sb.AppendLine("以上信息已成功录入。");
                    sb.AppendLine($"同队伍同boss 您共有{count}条记录。");
                } else {
                    sb.AppendLine("录入失败(不是你的错)");
                }
            } catch (Exception e) {
                sb.AppendLine("录入失败(不是你的错)");
            }
        }
        
        private void proc_seeRecord(ref StringBuilder sb, String cmd, long fromQQ) {
            sb.AppendLine("查询你的战况中（默认显示最近10条）");
            sb.AppendLine("===========");
            Match matches = rx_seeRecord.Match(cmd);
            String team = "";
            int shukai = -1;
            int bossid = -1;
            if (matches.Groups["team"] != null && matches.Groups["team"].Value.Trim() != "") {
                team = matches.Groups["team"].Value.Trim();
                sb.AppendLine($"筛选队伍：{team}");
            } else {
                sb.AppendLine($"不筛选队伍");
                team = "";
            }
            if (matches.Groups["shukai"] != null && int.TryParse(matches.Groups["shukai"].Value, out shukai)) {
                sb.AppendLine($"筛选周目：{shukai}");
            } else {
                sb.AppendLine($"不筛选周目");
                shukai = -1;
            }
            if (matches.Groups["boss"] != null && int.TryParse(matches.Groups["boss"].Value, out bossid)) {
                sb.AppendLine($"筛选Boss:{bossid}");
            } else {
                sb.AppendLine("不筛选Boss");
                bossid = -1;
            }
            sb.AppendLine("===========");
            try {
                List<Dictionary<String, Object>> results = DBManager.searchRecord(fromQQ, team, shukai, bossid, true);
                foreach (Dictionary<String, Object> iter in results) {
                    sb.AppendLine($"[队伍:{iter["team"]}，周目：{iter["shukai"]}，Boss序号：{iter["boss"]}，伤害：{iter["damage"]}]");
                }
            } catch (Exception e) {
                sb.AppendLine("查询失败（这不是你的错）");
            }
        }

        private void proc_seeAll(ref StringBuilder sb, String cmd, long fromGroup) {
            sb.AppendLine("查询本群所有战况中（默认显示最近10条）");
            sb.AppendLine("===========");
            if (fromGroup == 0) {
                sb.AppendLine("请在群聊中使用。");
                return;
            }
            Match matches = rx_seeRecord.Match(cmd);
            String team = "";
            int shukai = -1;
            int bossid = -1;
            if (matches.Groups["team"] != null && matches.Groups["team"].Value.Trim() != "") {
                team = matches.Groups["team"].Value.Trim();
                sb.AppendLine($"筛选队伍：{team}");
            } else {
                sb.AppendLine($"不筛选队伍");
                team = "";
            }
            if (matches.Groups["shukai"] != null && int.TryParse(matches.Groups["shukai"].Value, out shukai)) {
                sb.AppendLine($"筛选周目：{shukai}");
            } else {
                sb.AppendLine($"不筛选周目");
                shukai = -1;
            }
            if (matches.Groups["boss"] != null && int.TryParse(matches.Groups["boss"].Value, out bossid)) {
                sb.AppendLine($"筛选Boss:{bossid}");
            } else {
                sb.AppendLine("不筛选Boss");
                bossid = -1;
            }
            sb.AppendLine("===========");
            try {
                List<Dictionary<String, Object>> results = DBManager.searchGroupRecord(fromGroup, team, shukai, bossid, true);
                foreach (Dictionary<String, Object> iter in results) {
                    sb.AppendLine($"{iter["qq_num"]} [队伍:{iter["team"]}，周目：{iter["shukai"]}，Boss序号：{iter["boss"]}，伤害：{iter["damage"]}]");
                }
            } catch (Exception e) {
                sb.AppendLine("查询失败（这不是你的错）");
            }
        }

        private void proc_sumRecord(ref StringBuilder sb, String cmd, long fromQQ, long fromGroup) {
            sb.AppendLine("正在统计您的战况");
            sb.AppendLine("===========");
            Match matches = rx_sumRecord.Match(cmd);
            String team = "";
            int shukai = -1;
            int bossid = -1;
            team = matches.Groups["team"].Value.Trim();

            if (fromGroup != 0) {
                DBManager.getDefault(fromGroup, out shukai, out bossid);
            }

            int shukai_override;
            int boss_override;

            if ((matches.Groups["shukai"] != null) && int.TryParse(matches.Groups["shukai"].Value, out shukai_override)) {
                shukai = shukai_override;
            }
            if ((matches.Groups["boss"] != null) && int.TryParse(matches.Groups["boss"].Value, out boss_override)) {
                bossid = boss_override;
            }

            if (shukai == -1) {
                sb.AppendLine("周目数识别失败。");
                sb.AppendLine("===========");
                return;
            }

            if (bossid == -1) {
                sb.AppendLine("Boss序号识别失败");
                sb.AppendLine("===========");
                return;
            }

            sb.AppendLine($"队伍：{team}");
            sb.AppendLine($"周目数：{shukai}");
            sb.AppendLine($"Boss序号：{bossid}");
            sb.AppendLine("===========");
            try {
                List<Dictionary<String, Object>> results = DBManager.searchRecord(fromQQ, team, shukai, bossid, false);
                sb.AppendLine($"在此条件下 您共有{results.Count}条记录。");
                if (results.Count == 0)
                    return;
                GaussianRV grv = new GaussianRV();
                foreach (Dictionary<String, Object> record in results) {
                    grv.addObservation((int)record["damage"]);
                }
                grv.calculate();
                sb.AppendLine($"在25%的情况下，您可打出{Math.Round(grv.topPercent(25), 2)}的伤害，置信度{Math.Round(grv.confidence(25) * 100)}%");
                sb.AppendLine($"在50%的情况下，您可打出{Math.Round(grv.topPercent(50), 2)}的伤害，置信度{Math.Round(grv.confidence(50) * 100)}%");
                sb.AppendLine($"在75%的情况下，您可打出{Math.Round(grv.topPercent(75), 2)}的伤害，置信度{Math.Round(grv.confidence(75) * 100)}%");
                sb.AppendLine($"在90%的情况下，您可打出{Math.Round(grv.topPercent(90), 2)}的伤害，置信度{Math.Round(grv.confidence(90) * 100)}%");
                sb.AppendLine("添加更多记录可能提高置信度。");
            } catch (Exception e) {
                sb.AppendLine("统计失败（什么东西炸了？）");
            }
        }
       
        private void proc_teamList(ref StringBuilder sb, long fromQQ) {
            try {
                sb.AppendLine("您登记过的队伍");
                sb.AppendLine("===========");
                List<String> teams = DBManager.getTeams(fromQQ);
                foreach (String team in teams) {
                    sb.Append(team + "，");
                }
                sb.Remove(sb.Length - 1, 1);
            } catch (Exception e) {

            }
        }
        
        private void proc_rmvRecord(ref StringBuilder sb, String cmd, long fromQQ) {
            Match matches = rx_rmvRecord.Match(cmd);
            sb.AppendLine("删除你的战况中");
            sb.AppendLine("===========");
            String team = "";
            int shukai = -1;
            int bossid = -1;
            int damage = -1;
            if (matches.Groups["team"] != null && matches.Groups["team"].Value.Trim() != "") {
                team = matches.Groups["team"].Value.Trim();
                sb.AppendLine($"筛选队伍：{team}");
            } else {
                sb.AppendLine($"不筛选队伍");
                team = "";
            }
            if (matches.Groups["shukai"] != null && int.TryParse(matches.Groups["shukai"].Value, out shukai)) {
                sb.AppendLine($"筛选周目：{shukai}");
            } else {
                sb.AppendLine($"不筛选周目");
                shukai = -1;
            }
            if (matches.Groups["boss"] != null && int.TryParse(matches.Groups["boss"].Value, out bossid)) {
                sb.AppendLine($"筛选Boss：{bossid}");
            } else {
                sb.AppendLine("不筛选Boss");
                bossid = -1;
            }
            if (matches.Groups["damage"] != null && int.TryParse(matches.Groups["damage"].Value, out damage)) {
                sb.AppendLine($"筛选伤害：{damage}");
            } else {
                sb.AppendLine("不筛选伤害");
                damage = -1;
            }
            sb.AppendLine("===========");
            try {
                int affected = DBManager.removeBattleRecord(fromQQ, team, shukai, bossid, damage);
                sb.AppendLine($"删除了{affected}条记录");
            } catch (Exception e) {
                sb.AppendLine("爆炸啦（可能删了 可能没删）");
            }
        }
        
        private void proc_addAdmin(ref StringBuilder sb, String cmd, long fromQQ, long fromGroup) {
            try {
                sb.AppendLine("添加群操作员中...");
                sb.AppendLine("===========");
                long new_admin;
                Regex rx_Admin_1 = new Regex(@"添加群管\s*(?<new_admin>\d+)");
                Regex rx_Admin_2 = new Regex(@"添加群管\s*\[CQ:at,qq=(?<new_admin>\d+)\]");
                if(rx_Admin_1.IsMatch(cmd)) {
                    Match match = rx_Admin_1.Match(cmd);
                    long.TryParse(match.Groups["new_admin"].Value.Trim(), out new_admin);
                } else if (rx_Admin_2.IsMatch(cmd)) {
                    Match match = rx_Admin_2.Match(cmd);
                    long.TryParse(match.Groups["new_admin"].Value.Trim(), out new_admin);
                } else {
                    sb.AppendLine("QQ号未识别");
                    return;
                }
                sb.AppendLine($"新的管理：{new_admin}");
                sb.AppendLine("===========");

                if (DBManager.checkManager(fromGroup, fromQQ)) {
                    DBManager.addManager(fromGroup, new_admin);
                    sb.AppendLine("添加成功");
                } else {
                    sb.AppendLine("您的权限不够");
                }
            } catch {
                sb.AppendLine("爆炸了...");
            }
        }
        
        private void proc_getOptimalSol(ref StringBuilder sb, String cmd, long fromQQ, long fromGroup) {
            sb.AppendLine("排刀中...");
            sb.AppendLine("===========");
            try {
                if (fromGroup == 0) {
                    sb.AppendLine("请在群中使用");
                } else if(!DBManager.checkManager(fromGroup, fromQQ)) {
                    sb.AppendLine("该操作需要大量资源，因此仅限群管使用。");
                    return;
                }
                Match match = rx_arrange.Match(cmd);
                int shukai = -1;
                int bossid = -1;
                int percent = 50;
                long remaining = -1;

                if (fromGroup != 0) {
                    DBManager.getDefault(fromGroup, out shukai, out bossid);
                }

                int shukai_override;
                int boss_override;

                if ((match.Groups["shukai"] != null) && int.TryParse(match.Groups["shukai"].Value, out shukai_override)) {
                    shukai = shukai_override;
                }
                if ((match.Groups["boss"] != null) && int.TryParse(match.Groups["boss"].Value, out boss_override)) {
                    bossid = boss_override;
                }

                if (match.Groups["percent"]!=null && int.TryParse(match.Groups["percent"].Value.Trim(), out percent)) {
                    sb.AppendLine($"使用概率{percent}%进行伤害计算。");
                } else {
                    sb.AppendLine($"使用概率50%进行伤害计算。");
                    percent = 50;
                }

                if (shukai == -1) {
                    sb.AppendLine("周目数读取失败");
                    return;
                }

                if (bossid == -1) {
                    sb.AppendLine("Boss序号读取失败");
                    return;
                }

                sb.AppendLine($"周目：{shukai}");
                sb.AppendLine($"Boss:{bossid}");

                if (long.TryParse(match.Groups["rem"].Value, out remaining)) {
                    sb.AppendLine($"残余血量:{remaining}");
                } else {
                    sb.AppendLine("残余血量读取失败。");
                    return;
                }
                sb.AppendLine("===========");

                List<Dictionary<String, Object>> results = DBManager.searchGroupRecord(fromGroup, "", shukai, bossid, false);
                sb.AppendLine($"有相关记录{results.Count}条");

                // Now group them into teams, find gaussian.
                Dictionary<String, List<int>> teamedDamages = new Dictionary<string, List<int>>();

                // Group into accounts and teams.
                foreach (Dictionary<String, Object> iter in results) {
                    String temp_key = $"[CQ:at,qq={iter["qq_num"]}] [队伍:{iter["team"]}]";
                    if (!teamedDamages.ContainsKey(temp_key)) {
                        teamedDamages[temp_key] = new List<int>();
                    }
                    teamedDamages[temp_key].Add((int)iter["damage"]);
                }

                sb.AppendLine($"整合后有{teamedDamages.Keys.Count}个队伍");

                // Find Gaussian for each team.
                Dictionary<String, Double> predictedDamages = new Dictionary<string, double>();
                foreach(KeyValuePair<String, List<int>> e in teamedDamages) {
                    GaussianRV ngv = new GaussianRV();
                    ngv.addAll(e.Value);
                    ngv.calculate();
                    predictedDamages.Add(e.Key, ngv.topPercent(percent));
                }

                // Start solving.
                SubSetSums solver = new SubSetSums();
                solver.addChoices(predictedDamages);
                List<String> solution = solver.sumTo(remaining);
                if (solution.Count == 0) {
                    sb.AppendLine("所有人一起上也不够哦");
                } else {
                    StringBuilder sb2 = new StringBuilder();
                    double count = 0;
                    foreach(String team in solution) {
                        sb2.AppendLine($"{team} 预估伤害{predictedDamages[team]}");
                        count += predictedDamages[team];
                    }
                    sb.AppendLine($"以下是解法，总伤害为{count}");
                    sb.AppendLine(sb2.ToString());
                }
            } catch (Exception e) {
                sb.AppendLine("该炸了");
            }
        }

        private void proc_deAffiliate(ref StringBuilder sb, long fromQQ, long fromGroup) {
            sb.AppendLine("解除关系中...");
            sb.AppendLine("===========");
            try {
                if(fromGroup == 0) {
                    sb.AppendLine("请在群聊中使用...");
                    return;
                }
                DBManager.removeGroupRelation(fromGroup, fromQQ);
                sb.AppendLine("你已解除于此群的关系");
                sb.AppendLine("在此群中使用任何指令 即可重新添加关系。");
            } catch {
                sb.AppendLine("啊哈 是bug");
            }
        }
        
        private void proc_groupDefault(ref StringBuilder sb, String cmd, long fromQQ, long fromGroup) {
            sb.AppendLine("设置群默认值");
            sb.AppendLine("===========");
            try {
                if (fromGroup == 0) {
                    sb.AppendLine("请在群聊中使用...");
                    return;
                }
                Match match = rx_groupDefault.Match(cmd);
                int shukai = -1;
                int boss = -1;
                bool error = false;
                if (!int.TryParse(match.Groups["shukai"].Value, out shukai)) {
                    sb.AppendLine("周回读取失败，请给出一个数字。");
                    sb.AppendLine("（周回数限制在五位数以内... 超过五位数的话我也不知道该说啥了）");
                    error = true;
                }
                if (!int.TryParse(match.Groups["boss"].Value, out boss)) {
                    sb.AppendLine("BOSS读取失败，请给出一个数字。");
                    sb.AppendLine("（BOSS序号限制在三位数以内）");
                    error = true;
                }
                if (error) {
                    sb.AppendLine("至少一项读取失败，拒绝记录。");
                    sb.AppendLine("===========");
                    return;
                }
                sb.AppendLine($"周回：{shukai}");
                sb.AppendLine($"Boss序号：{boss}");
                sb.AppendLine("===========");
                DBManager.setDefault(fromGroup, shukai, boss);
                sb.AppendLine("已成功设置群默认值");
            } catch (Exception e) {
                sb.AppendLine("bug了");
            }
        }
        
        private String replyRaw(String head, String cmd, long fromQQ, long fromGroup) {
            StringBuilder sb = new StringBuilder();
            if (head != "")
                sb.AppendLine(head);
            if (cmd == "版本") {
                sb.AppendLine("行会战计算器 by MetricVoid");
                sb.AppendLine("ver. 1.0.0 - ALPHA");
                sb.AppendLine("使用 行会战#帮助 查看帮助");
            } else if (rx_addRecord.IsMatch(cmd)) {
                proc_addRecord(ref sb, cmd, fromQQ, fromGroup);
            } else if (rx_seeRecord.IsMatch(cmd)) {
                proc_seeRecord(ref sb, cmd, fromQQ);
            } else if (rx_seeAllRecord.IsMatch(cmd)) {
                proc_seeAll(ref sb, cmd, fromGroup);
            } else if (rx_sumRecord.IsMatch(cmd)) {
                proc_sumRecord(ref sb, cmd, fromQQ, fromGroup);
            } else if (rx_teamList.IsMatch(cmd)) {
                proc_teamList(ref sb, fromQQ);
            } else if (rx_rmvRecord.IsMatch(cmd)) {
                proc_rmvRecord(ref sb, cmd, fromQQ);
            } else if (rx_addAdmin.IsMatch(cmd)) {
                proc_addAdmin(ref sb, cmd, fromQQ, fromGroup);
            } else if (rx_arrange.IsMatch(cmd)) {
                proc_getOptimalSol(ref sb, cmd, fromQQ, fromGroup);
            } else if (rx_removeAffinity.IsMatch(cmd)) {
                proc_deAffiliate(ref sb, fromQQ, fromGroup);
            } else if (rx_groupDefault.IsMatch(cmd)) {
                proc_groupDefault(ref sb, cmd, fromQQ, fromGroup);
            }
            else if (cmd.Equals("帮助")) {
                sb.AppendLine("方括号代表必要参数 尖括号代表可选参数。");
                sb.AppendLine("参数顺序必须正确。");
                sb.AppendLine(@"请前往 https://github.com/Metric-Void/PCR-GuildBot/blob/master/usage.md 查看更详细的解释");
                sb.AppendLine("====== 个人指令 ======");
                sb.AppendLine("报战 队伍=[队伍名] 周目=[数字] BOSS=[数字] 伤害=[数字]");
                sb.AppendLine("查询 <队伍=[队伍名]> <周目=[数字]> <BOSS=[数字]>");
                sb.AppendLine("统计 队伍=[队伍名] 周目=[数字] BOSS=[数字]");
                sb.AppendLine("删除 <队伍=[队伍名]> <周目=[数字]> <BOSS=[数字]> <伤害=[数字]>");
                sb.AppendLine("无参数指令 队伍列表、版本、帮助。");
                sb.AppendLine("====== 群聊指令 ======");
                sb.AppendLine("查询所有 <队伍=[队伍名]> <周目=[数字]> <BOSS=[数字]>");
                sb.AppendLine("预设 周目=[数字] BOSS=[数字]");
                sb.AppendLine("排刀<[百分比]%> <周目=[数字]> <BOSS=[数字]>");
                sb.AppendLine("添加群管 <QQ号 或 @>");
                sb.AppendLine("解绑");
            } else {
                sb.AppendLine("指令未识别。");
                sb.AppendLine("使用“帮助”来查看指令列表");
            }
            return sb.ToString();
        }
    }
}
