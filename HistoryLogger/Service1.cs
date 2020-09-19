using System.Diagnostics;
using System.ServiceProcess;
using System.Timers;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System;

namespace PovlBrowserHistory
{
    public partial class Service1 : ServiceBase
    {


        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Timer timer = new Timer();
            timer.Interval = 60000;
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        protected override void OnStop()
        {
        }


        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            // Check Firefox
            CheckHistory("firefox");

            // Check Chrome
            Process[] cname = Process.GetProcessesByName("chrome");
            if (cname.Length <= 0)
            {
                CheckHistory("chrome");
            }
            //Check Edge
            Process[] ename = Process.GetProcessesByName("edge");
            if (ename.Length <= 0)
            {
                CheckHistory("edge");
            }
        }

        static void CheckHistory(string browser)
        {
            Dictionary<string, string> sqliteFilename = new Dictionary<string, string>();
            sqliteFilename.Add("firefox", "places.sqlite");
            sqliteFilename.Add("chrome", "History");
            sqliteFilename.Add("edge", "History");

            SQLiteConnection sqlite_conn;   //Initiate a sqliteconnection variable

            List<string> paths = Helper.GetPaths(browser); //Query the Registry for %APPDATA% paths for each users used for Firefox
            foreach (string path in paths)
            {
                string[] files = Directory.GetFiles(path, sqliteFilename[browser], SearchOption.AllDirectories);    //Search for sqlite files

                foreach (string file in files)
                {
                    string[] path_array = file.Split('\\'); //Get array of users
                    sqlite_conn = Helper.CreateConnection(file);   //Foreach SQLite file, a connection is created
                    CheckForNewData(sqlite_conn, browser, path_array[2]);
                }
            }
        }

        static void CheckForNewData(SQLiteConnection conn, string browser, string user)
        {
            Dictionary<string, string> commandText = new Dictionary<string, string>();
            commandText.Add("firefox", "SELECT datetime(moz_historyvisits.visit_date/1000000,'unixepoch') as last_visited FROM moz_places, moz_historyvisits WHERE moz_places.id = moz_historyvisits.place_id ORDER BY last_visited DESC LIMIT 1");
            commandText.Add("chrome", "SELECT datetime(last_visit_time/1000000-11644473600, 'unixepoch') as last_visited, url, title FROM urls ORDER BY last_visited DESC LIMIT 1");
            commandText.Add("edge", "SELECT datetime(last_visit_time/1000000-11644473600, 'unixepoch') as last_visited, url, title FROM urls ORDER BY last_visited DESC LIMIT 1");

            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = commandText[browser];
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            bool needUpdate = false;
            string log_path = AppDomain.CurrentDomain.BaseDirectory + @"\Logs\" +  browser + "_log.txt";
            ;
            if (File.Exists(log_path))
            {
                string lastLine = Helper.UsersLastEntry(user, log_path);
                DateTime lastEntry = DateTime.Parse(lastLine.Substring(0, lastLine.IndexOf(';'))); ;
                Helper.CheckFileSize(browser, lastLine);
                while (sqlite_datareader.Read())
                {
                    DateTime fromLastRow = sqlite_datareader.GetDateTime(0);

                    if (fromLastRow > lastEntry)
                    {
                        needUpdate = true;
                    }
                }
                if (needUpdate)
                {
                    Console.WriteLine("Updating...");
                    GetNewData(conn, browser, user, lastEntry);
                }
                else
                {
                    Console.WriteLine("Not updating...");
                    conn.Close();
                }
            }
            else
            {

                DateTime begin = new DateTime();
                begin = DateTime.Today;

                while (sqlite_datareader.Read())
                {
                    DateTime fromLastRow = sqlite_datareader.GetDateTime(0);

                    if (fromLastRow > begin)
                    {
                        needUpdate = true;
                    }
                }

                if (needUpdate)
                {
                    Console.WriteLine("Updating...");
                    GetNewData(conn, browser, user, begin);
                }
                else
                {
                    Console.WriteLine("Not updating...");
                    conn.Close();
                }
            }

        }

        static void GetNewData(SQLiteConnection conn, string browser, string user, DateTime lastEntry)
        {
            Dictionary<string, string> commandText = new Dictionary<string, string>();
            commandText.Add("firefox", "SELECT datetime(moz_historyvisits.visit_date/1000000,'unixepoch') as last_visited, moz_places.url, moz_places.rev_host, moz_places.title FROM moz_places, moz_historyvisits WHERE moz_places.id = moz_historyvisits.place_id AND last_visited > @lastEntry ORDER BY last_visited ASC ");
            commandText.Add("chrome", "SELECT datetime(last_visit_time/1000000-11644473600, 'unixepoch') as last_visited, url, title FROM urls WHERE last_visited > @lastEntry");
            commandText.Add("edge", "SELECT datetime(last_visit_time / 1000000 - 11644473600, 'unixepoch') as last_visited, url, title FROM urls WHERE last_visited > @lastEntry");

            SQLiteDataReader sqlite_datareader;
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = conn.CreateCommand();
            sqlite_cmd.CommandText = commandText[browser];
            sqlite_cmd.Parameters.AddWithValue("@lastEntry", lastEntry);
            sqlite_cmd.Prepare();
            sqlite_datareader = sqlite_cmd.ExecuteReader();

            string log_path = AppDomain.CurrentDomain.BaseDirectory + @"\Logs\" + browser + "_log.txt";

            using (StreamWriter sw = File.AppendText(log_path))
            {
                while (sqlite_datareader.Read())
                {
                    DateTime time = sqlite_datareader.GetDateTime(0);
                    string url = sqlite_datareader.GetString(1);
                    string title = "";
                    if (browser == "firefox")
                    {
                        TextReader getTitle = sqlite_datareader.GetTextReader(3);
                        title = getTitle.ReadToEnd();
                    }
                    else
                    {
                        title = sqlite_datareader.GetString(2);
                    }
                    string line = time + ";" + user + ";" + title + ";" + url;
                    sw.WriteLine(line);
                }
            }
            conn.Close();
        }

    }
}
