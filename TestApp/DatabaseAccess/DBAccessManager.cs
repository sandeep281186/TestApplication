using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseAccess
{
    public class TurretData
    {
        public string USER { get; set; }
        public string iTurretLogonName { get; set; }
        public string KeyType { get; set; }
        public string KeyLabel { get; set; }
        public string PageName { get; set; }
        public string PageNumber { get; set; }
        public string KeyIndex { get; set; }
        public string KeyPagePolicy { get; set; }
        public string SpeakerPage { get; set; }
        public string SpeakerChannel { get; set; }
        public string AppearanceType { get; set; }
        public string DirectoryType { get; set; }
        public string Address { get; set; }
        public string LineReference { get; set; }

        public void AddItem(int index, string item)
        {
            switch(index)
            {
                case 1:
                    USER = item;
                break;

                case 2:
                    iTurretLogonName = item;
                break;

                case 3:
                    KeyType = item;
                break;

                case 4:
                    KeyLabel = item;
                break;

                case 5:
                    PageName = item;
                break;

                case 6:
                    PageNumber = item;
                break;

                case 7:
                    KeyIndex = item;
                break;

                case 8:
                    KeyPagePolicy = item;
                break;

                case 9:
                    SpeakerPage = item;
                break;

                case 10:
                    SpeakerChannel = item;
                break;

                case 11:
                    AppearanceType = item;
                break;

                case 12:
                    DirectoryType = item;
                break;

                case 13:
                    Address = item;
                break;

                case 14:
                    LineReference = item;
                break;
            }
        }
    }

    public class DBAccessManager : IDisposable
    {
        private string connectionString = @"Data Source=C:\Users\amit1\source\repos\GithubProjects\TestApplication\TestApp\TestApp\DB\TurretDB.db";
        private SqliteConnection conn = null;
        private static DBAccessManager manager;

        private DBAccessManager()
        {
            Connect();
        }

        public static DBAccessManager GetInstance()
        {
            return manager==null? (manager=new DBAccessManager()) : manager;
        }

        public void Dispose()
        {
            if (conn != null)
            {
                conn.Dispose();
                conn = null;
            }
        }

        private void Connect()
        {
            conn = new SqliteConnection(connectionString);
            SQLitePCL.Batteries.Init();
            conn.Open();
        }

        public bool ExecuteSql(List<TurretData> trDataList)
        {
            bool dataInserted = true;

            foreach (TurretData trData in trDataList)
            {
                try
                {
                    string commandText = "INSERT INTO iTurretData (" +
                    "'" + "USER" + "','" + "iTurret Logon Name" + "','" + "Key Type"+"','"+ "Key Label"+"','" + "Page Name"+ "','"+"Page Number"+ "','"+"Key Index" +
                    "','"+"Key Page Policy"+ "','"+ "Speaker Page" + "','"+"Speaker Channel"+ "','"+"Appearance Type"+ "','"+"Directory Type"+ "','"+"Address" +
                    "','" + "Line Reference"+ "'"+") VALUES ("
                    + "'" + trData.USER.Replace("'", "''") + "',"
                    + "'" + trData.iTurretLogonName.Replace("'", "''") + "',"
                    + "'" + trData.KeyType.Replace("'", "''") + "',"
                    + "'" + trData.KeyLabel.Replace("'", "''") + "',"
                    + "'" + trData.PageName.Replace("'", "''") + "',"
                    + trData.PageNumber + ","
                    + trData.KeyIndex + ","
                    + "'" + trData.KeyPagePolicy.Replace("'", "''") + "',"
                    + "'" + trData.SpeakerPage.Replace("'", "''") + "',"
                    + "'" + trData.SpeakerChannel.Replace("'", "''") + "',"
                    + "'" + trData.AppearanceType.Replace("'", "''") + "',"
                    + "'" + trData.DirectoryType.Replace("'", "''") + "',"
                    + "'" + trData.Address.Replace("'", "''") + "',"
                    + "'" + trData.LineReference.Replace("'", "''") + "'" + ");";

                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = commandText;
                        command.ExecuteNonQuery();
                    }                 
                }
                catch (Exception ex)
                {
                    dataInserted = false;
                }
            }

            return dataInserted;
        }

        public DataTable GetUsers()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("USER");

            try
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "Select distinct(USER) from iTurretData";
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            dt.Rows.Add(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
               
            }

            return dt;
        }

        public DataTable GetUserData(string userName)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Key Label");
            dt.Columns.Add("Page Name");
            dt.Columns.Add("Page Number");
            dt.Columns.Add("Key Index");

            try
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "Select * from iTurretData where USER='" + userName + "'";

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dt.Rows.Add(new object[] { reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6) });
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return dt;
        }

        public DataTable LoadTurretData()
        {
            DataTable dt = new DataTable();
           
            dt.Columns.Add("User");
            dt.Columns.Add("iTurret Logon Name");
            dt.Columns.Add("Key Type");
            dt.Columns.Add("Key Label");
            dt.Columns.Add("Page Name");
            dt.Columns.Add("Page Number");
            dt.Columns.Add("Key Index");
            dt.Columns.Add("Key Page");
            dt.Columns.Add("Policy Speaker");
            dt.Columns.Add("Page Speaker Channel");
            dt.Columns.Add("Appearance Type");
            dt.Columns.Add("Directory Type");
            dt.Columns.Add("Address");
            dt.Columns.Add("Line Reference");

            try
            {
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "Select * from iTurretData";

                    using (SqliteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dt.Rows.Add(new object[] {
                                reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3),
                                reader.GetString(4), reader.GetString(5), reader.GetString(6), reader.GetString(7),
                                reader.GetString(8), reader.GetString(9), reader.GetString(10), reader.GetString(11),
                                reader.GetString(12), reader.GetString(13)});
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return dt;
        }
    }
}
