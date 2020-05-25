using Native.Sdk.Cqp.EventArgs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.metricv.pcrguild.Code {
    static class DBManager {
        private static SQLiteConnection conn = null;
        public static void up(CQEventArgs e) {
            String dbFile = e.CQApi.AppDirectory + "com.metricv.pcrguild.db";
            String dbURI = @"URI=file:" + e.CQApi.AppDirectory + @"com.metricv.pcrguild.db;UseAffectedRows=True";

            if (!File.Exists(dbFile)) {
                SQLiteConnection.CreateFile(dbFile);
            }

            conn = new SQLiteConnection(dbURI);
            conn.OpenAsync();

            var sql_cmd = new SQLiteCommand(conn);
            sql_cmd.CommandText = create_schema_battlerec;
            sql_cmd.ExecuteNonQuery();

            sql_cmd.CommandText = create_schema_admintable;
            sql_cmd.ExecuteNonQuery();

            sql_cmd.CommandText = create_schema_groupmember;
            sql_cmd.ExecuteNonQuery();

            sql_cmd.CommandText = create_schema_groupsettings;
            sql_cmd.ExecuteNonQuery();
        }
        public static void down(CQEventArgs e) {
            conn.Close();
        }

        public static void addManager() {
            if(conn.State == ConnectionState.Closed) {
                conn.Open();
            }
            var sql_cmd = new SQLiteCommand(conn);
            sql_cmd.CommandText = "INSERT INTO admin_list(group_num, qq_num) SELECT 0, @new_admin WHERE NOT EXISTS(SELECT * FROM admin_list WHERE group_num = 0 AND qq_num = @new_admin);";
            sql_cmd.Parameters.AddWithValue("@new_admin", ConfigHandler.master_qq);
            sql_cmd.Prepare();
            sql_cmd.ExecuteNonQuery();
        }

        public static bool addManager(long group, long qq) {
            try {
                if (conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                sql_cmd.CommandText = "INSERT INTO admin_list(group_num, qq_num) SELECT @group, @admin WHERE NOT EXISTS(SELECT * FROM admin_list WHERE group_num = @group AND qq_num = @admin);";
                sql_cmd.Parameters.AddWithValue("group", group);
                sql_cmd.Parameters.AddWithValue("admin", qq);
                sql_cmd.Prepare();
                sql_cmd.ExecuteNonQuery();
                return true;
            } catch (Exception e) {
                return false;
            }
        }

        public static bool checkManager(long group, long qq) {
            try {
                if (conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                sql_cmd.CommandText = "SELECT COUNT(*) FROM admin_list WHERE (group_num = @group AND qq_num = @admin) OR (group_num=0 AND qq_num=@admin);";
                sql_cmd.Parameters.AddWithValue("group", group);
                sql_cmd.Parameters.AddWithValue("admin", qq);
                sql_cmd.Prepare();
                return ((long)sql_cmd.ExecuteScalar()) != 0;
            } catch (Exception e) {
                return false;
            }
        }

        public static long addBattleRecord(long qq, String team, int shukai, int boss_id, long damage) {
            try {
                if (conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                sql_cmd.CommandText = "INSERT INTO battle_records(qq_num, team_name, shukai, boss_id, damage) VALUES (@qq, @team, @shukai, @boss_id, @damage)";
                sql_cmd.Parameters.AddWithValue("@qq", qq);
                sql_cmd.Parameters.AddWithValue("@team", team);
                sql_cmd.Parameters.AddWithValue("@shukai", shukai);
                sql_cmd.Parameters.AddWithValue("@boss_id", boss_id);
                sql_cmd.Parameters.AddWithValue("@damage", damage);
                sql_cmd.Prepare();
                sql_cmd.ExecuteNonQuery();

                sql_cmd.CommandText = "SELECT COUNT(*) FROM battle_records WHERE qq_num=@qq AND team_name=@team AND shukai=@shukai AND boss_id=@boss_id";
                sql_cmd.Parameters.AddWithValue("@qq", qq);
                sql_cmd.Parameters.AddWithValue("@team", team);
                sql_cmd.Parameters.AddWithValue("@shukai", shukai);
                sql_cmd.Parameters.AddWithValue("@boss_id", boss_id);
                sql_cmd.Prepare();
                return (long)sql_cmd.ExecuteScalar();
            } catch (Exception e) {
                return -1;
            }
        }

        public static int removeBattleRecord(long qq, String team, int shukai, int bossid, int damage) {
            try {
                if (conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                StringBuilder remove_cmdline = new StringBuilder();
                remove_cmdline.Append("DELETE FROM battle_records ");
                bool appendAND = false;

                if (qq != -1) {
                    if (!appendAND)
                        remove_cmdline.Append("WHERE ");
                    remove_cmdline.Append("qq_num=@qq ");
                    sql_cmd.Parameters.AddWithValue("@qq", qq);
                    appendAND = true;
                }

                if (team != "") {
                    if (appendAND)
                        remove_cmdline.Append("AND ");
                    else
                        remove_cmdline.Append("WHERE ");
                    remove_cmdline.Append("team_name=@team ");
                    sql_cmd.Parameters.AddWithValue("@team", team);
                    appendAND = true;
                }

                if (shukai != -1) {
                    if (appendAND)
                        remove_cmdline.Append("AND ");
                    else
                        remove_cmdline.Append("WHERE ");
                    remove_cmdline.Append("shukai=@shukai ");
                    sql_cmd.Parameters.AddWithValue("@shukai", shukai);
                    appendAND = true;
                }

                if (bossid != -1) {
                    if (appendAND)
                        remove_cmdline.Append("AND ");
                    else
                        remove_cmdline.Append("WHERE ");
                    remove_cmdline.Append("boss_id=@boss ");
                    sql_cmd.Parameters.AddWithValue("@boss", bossid);
                }

                if (damage != -1) {
                    if (appendAND)
                        remove_cmdline.Append("AND ");
                    else
                        remove_cmdline.Append("WHERE ");
                    remove_cmdline.Append("damage=@damage ");
                    sql_cmd.Parameters.AddWithValue("@damage", damage);
                }

                sql_cmd.CommandText = remove_cmdline.ToString();
                return sql_cmd.ExecuteNonQuery();
            } catch (Exception e) {
                return -42;
            }
        }

        public static List<Dictionary<String, Object>> searchRecord(long qq, String team, int shukai, int bossid, bool limit) {
            try {
                if (conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                StringBuilder search_cmdline = new StringBuilder();
                search_cmdline.Append("SELECT qq_num, team_name, shukai, boss_id, damage FROM battle_records ");
                bool appendAND = false;

                if (qq != -1) {
                    if (!appendAND)
                        search_cmdline.Append("WHERE ");
                    search_cmdline.Append("qq_num=@qq ");
                    sql_cmd.Parameters.AddWithValue("@qq", qq);
                    appendAND = true;
                }

                if (team != "") {
                    if (appendAND)
                        search_cmdline.Append("AND ");
                    else
                        search_cmdline.Append("WHERE ");
                    search_cmdline.Append("team_name=@team ");
                    sql_cmd.Parameters.AddWithValue("@team", team);
                    appendAND = true;
                }

                if (shukai != -1) {
                    if (appendAND)
                        search_cmdline.Append("AND ");
                    else
                        search_cmdline.Append("WHERE ");
                    search_cmdline.Append("shukai=@shukai ");
                    sql_cmd.Parameters.AddWithValue("@shukai", shukai);
                    appendAND = true;
                }

                if (bossid != -1) {
                    if (appendAND)
                        search_cmdline.Append("AND ");
                    else
                        search_cmdline.Append("WHERE ");
                    search_cmdline.Append("boss_id=@boss ");
                    sql_cmd.Parameters.AddWithValue("@boss", bossid);
                }

                search_cmdline.Append("ORDER BY id DESC");
                if (limit)
                    search_cmdline.Append(" LIMIT 0,10");
                search_cmdline.Append(";");

                sql_cmd.CommandText = search_cmdline.ToString();
                sql_cmd.Prepare();
                SQLiteDataReader sdr = sql_cmd.ExecuteReader();

                List<Dictionary<String, Object>> result = new List<Dictionary<String, Object>>();
                while (sdr.Read()) {
                    Dictionary<String, Object> temp = new Dictionary<string, object>();
                    temp.Add("qq_num", sdr.GetInt64(0));
                    temp.Add("team", sdr.GetString(1));
                    temp.Add("shukai", sdr.GetInt16(2));
                    temp.Add("boss", sdr.GetInt16(3));
                    temp.Add("damage", sdr.GetInt32(4));
                    result.Add(temp);
                }
                return result;
            } catch (Exception e) {
                return null;
            }
        }

        public static List<Dictionary<String, Object>> searchGroupRecord(long group, String team, int shukai, int bossid, bool limit) {
            try {
                if (conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                StringBuilder search_cmdline = new StringBuilder();
                search_cmdline.Append("SELECT battle_records.qq_num, team_name, shukai, boss_id, damage FROM battle_records, group_members WHERE group_members.group_num=@group AND battle_records.qq_num=group_members.qq_num ");
                sql_cmd.Parameters.AddWithValue("@group", group);
                bool appendAND = true;

                if (team != "") {
                    if (appendAND)
                        search_cmdline.Append("AND ");
                    else
                        search_cmdline.Append("WHERE ");
                    search_cmdline.Append("team_name=@team ");
                    sql_cmd.Parameters.AddWithValue("@team", team);
                    appendAND = true;
                }

                if (shukai != -1) {
                    if (appendAND)
                        search_cmdline.Append("AND ");
                    else
                        search_cmdline.Append("WHERE ");
                    search_cmdline.Append("shukai=@shukai ");
                    sql_cmd.Parameters.AddWithValue("@shukai", shukai);
                    appendAND = true;
                }

                if (bossid != -1) {
                    if (appendAND)
                        search_cmdline.Append("AND ");
                    else
                        search_cmdline.Append("WHERE ");
                    search_cmdline.Append("boss_id=@boss ");
                    sql_cmd.Parameters.AddWithValue("@boss", bossid);
                }

                search_cmdline.Append("ORDER BY battle_records.id DESC");
                if(limit) search_cmdline.Append(" LIMIT 0,10");
                search_cmdline.Append(";");

                sql_cmd.CommandText = search_cmdline.ToString();
                sql_cmd.Prepare();
                SQLiteDataReader sdr = sql_cmd.ExecuteReader();

                List<Dictionary<String, Object>> result = new List<Dictionary<String, Object>>();
                while (sdr.Read()) {
                    Dictionary<String, Object> temp = new Dictionary<string, object>();
                    temp.Add("qq_num", sdr.GetInt64(0));
                    temp.Add("team", sdr.GetString(1));
                    temp.Add("shukai", sdr.GetInt16(2));
                    temp.Add("boss", sdr.GetInt16(3));
                    temp.Add("damage", sdr.GetInt32(4));
                    result.Add(temp);
                }
                return result;
            } catch (Exception e) {
                return null;
            }
        }
        public static List<String> getTeams(long qq) {
            try {
                if(conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                sql_cmd.CommandText = "SELECT DISTINCT team_name FROM battle_records WHERE qq_num=@qq;";
                sql_cmd.Parameters.AddWithValue("@qq", qq);

                SQLiteDataReader sdr = sql_cmd.ExecuteReader();
                List<String> teams = new List<string>();
                while(sdr.Read()) {
                    teams.Add(sdr.GetString(0));
                }
                return teams;
            } catch (Exception e) {
                return new List<String>();
            }
        }

        public static void addGroupRelation(long group_id, long qq_id) {
            try {
                if(conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                sql_cmd.CommandText = @"INSERT INTO group_members(group_num, qq_num) SELECT @group_id, @qq_id WHERE NOT EXISTS (SELECT * FROM group_members WHERE group_num=@group_id AND qq_num=@qq_id);";
                sql_cmd.Parameters.AddWithValue("@group_id", group_id);
                sql_cmd.Parameters.AddWithValue("@qq_id", qq_id);
                sql_cmd.ExecuteNonQuery();
            } catch (Exception e) {
                return;
            }
        }

        public static void removeGroupRelation(long group_id, long qq_id) {
            try {
                if (conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                sql_cmd.CommandText = @"DELETE FROM group_members WHERE group_num=@group_id AND qq_num=@qq_id;";
                sql_cmd.Parameters.AddWithValue("@group_id", group_id);
                sql_cmd.Parameters.AddWithValue("@qq_id", qq_id);
                sql_cmd.ExecuteNonQuery();
            } catch (Exception e) {
                return;
            }
        }

        public static void setDefault(long group_id, int shukai, int boss_id) {
            try {
                if (conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                sql_cmd.CommandText = @"DELETE FROM group_settings WHERE group_num=@group_id;";
                sql_cmd.Parameters.AddWithValue("@group_id", group_id);
                sql_cmd.ExecuteNonQuery();
                sql_cmd.CommandText = @"INSERT INTO group_settings(group_num, enabled, boss_id, shukai) VALUES (@group_id,true,@boss_id, @shukai);";
                sql_cmd.Parameters.AddWithValue("@group_id", group_id);
                sql_cmd.Parameters.AddWithValue("@boss_id", boss_id);
                sql_cmd.Parameters.AddWithValue("@shukai", shukai);
                sql_cmd.ExecuteNonQuery();
            } catch (Exception e) {
                return;
            }
        }

        public static void getDefault(long group_id, out int shukai, out int boss_id) {
            shukai = -1;
            boss_id = -1;
            try {
                if (conn.State == ConnectionState.Closed) {
                    conn.Open();
                }
                var sql_cmd = new SQLiteCommand(conn);
                sql_cmd.CommandText = @"SELECT boss_id,shukai FROM group_settings WHERE group_num=@group_id;";
                sql_cmd.Parameters.AddWithValue("@group_id", group_id);
                SQLiteDataReader sdr = sql_cmd.ExecuteReader();
                if(sdr.Read()) {
                    boss_id = sdr.GetInt32(0);
                    shukai = sdr.GetInt32(1);
                }
            } catch (Exception e) {
                return;
            }
        }

        private static String create_schema_battlerec =
            @"CREATE TABLE IF NOT EXISTS battle_records(
              id INTEGER PRIMARY KEY AUTOINCREMENT,
              qq_num UNSIGNED BIG INT,
              team_name TEXT,
              shukai TINYINT,
              boss_id TINYINT,
              damage INTEGER
            );";

        private static String create_schema_admintable =
            @"CREATE TABLE IF NOT EXISTS admin_list(
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                group_num UNSIGNED BIG INT,
                qq_num UNSIGNED BIG INT
            )";

        private static String create_schema_groupmember =
            @"CREATE TABLE IF NOT EXISTS group_members(
              id INTEGER PRIMARY KEY AUTOINCREMENT,
              group_num UNSIGNED BIT INT,
              qq_num UNSIGNED BIG INT
            );";

        private static String create_schema_groupsettings =
            @"CREATE TABLE IF NOT EXISTS group_settings(
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                group_num UNSIGNED BIT INT,
                enabled BOOLEAN,
                boss_id TINYINT,
                shukai TINYINT
            );";
    }
}

/*************** Below used for testing on http://www.sqlfiddle.com/ *************************
 * 
CREATE TABLE IF NOT EXISTS battle_records(
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  qq_num UNSIGNED BIG INT,
  team_name TEXT,
  shukai TINYINT,
  boss_id TINYINT,
  damage INTEGER
);

CREATE TABLE IF NOT EXISTS admin_list(
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  group_num UNSIGNED BIG INT,
  qq_num UNSIGNED BIG INT
);

CREATE TABLE IF NOT EXISTS group_members(
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  group_num UNSIGNED BIT INT,
  qq_num UNSIGNED BIG INT
);

INSERT INTO admin_list(group_num, qq_num) SELECT 0, 1124096029 WHERE NOT EXISTS(SELECT * FROM admin_list WHERE group_num=0 AND qq_num=1124096029);
INSERT INTO admin_list(group_num, qq_num) SELECT 0, 1124096029 WHERE NOT EXISTS(SELECT * FROM admin_list WHERE group_num=0 AND qq_num=1124096029);
INSERT INTO admin_list(group_num, qq_num) SELECT 0, 1124096029 WHERE NOT EXISTS(SELECT * FROM admin_list WHERE group_num=0 AND qq_num=1124096029);

SELECT * FROM admin_list;

SELECT COUNT(*) FROM admin_list WHERE group_num=0 AND qq_num=1124096029;

INSERT INTO battle_records(qq_num, team_name, shukai, boss_id, damage) VALUES (1124096029, "team1", 1, 1, 100000);
INSERT INTO battle_records(qq_num, team_name, shukai, boss_id, damage) VALUES (1124096029, "team1", 1, 1, 120000);
INSERT INTO battle_records(qq_num, team_name, shukai, boss_id, damage) VALUES (1124096029, "team1", 1, 1, 110000);

INSERT INTO battle_records(qq_num, team_name, shukai, boss_id, damage) VALUES (5133674, "team5", 1, 1, 100000);
INSERT INTO battle_records(qq_num, team_name, shukai, boss_id, damage) VALUES (5133674, "team5", 1, 1, 120000);
INSERT INTO battle_records(qq_num, team_name, shukai, boss_id, damage) VALUES (5133674, "team5", 1, 1, 110000);

INSERT INTO group_members(group_num, qq_num) SELECT 142857, 1124096029 WHERE NOT EXISTS (SELECT * FROM group_members WHERE group_num=142857 AND qq_num=1124096029);
INSERT INTO group_members(group_num, qq_num) SELECT 142857, 5133674 WHERE NOT EXISTS (SELECT * FROM group_members WHERE group_num=142857 AND qq_num=5133674);
INSERT INTO group_members(group_num, qq_num) SELECT 142857, 5133674 WHERE NOT EXISTS (SELECT * FROM group_members WHERE group_num=142857 AND qq_num=5133674);

SELECT battle_records.qq_num, team_name, shukai, boss_id, damage FROM group_members, battle_records WHERE group_members.qq_num = battle_records.qq_num;

SELECT * FROM group_members;

**************** End testing scheme ***************************/
