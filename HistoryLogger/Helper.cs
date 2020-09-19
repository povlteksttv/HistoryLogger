using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.IO;
using Microsoft.Win32;


namespace PovlBrowserHistory
{
    class Helper
    {

        public static SQLiteConnection CreateConnection(String file)
        {
            SQLiteConnection sqlite_conn;
            sqlite_conn = new SQLiteConnection("Data Source=" + file + ";Version=3;New=True;Compress=True;mode=ReadOnly");
            try
            {
                sqlite_conn.Open();
            }
            catch (Exception e)
            {

            }
            return sqlite_conn;
        }

        public static List<string> GetPaths(string browser)

        {
            Dictionary<string, string> appendPath = new Dictionary<string, string>();
            appendPath.Add("firefox", @"\Mozilla\Firefox\Profiles\");
            appendPath.Add("chrome", @"\Google\Chrome\User Data\Default");
            appendPath.Add("edge", @"\Microsoft\Edge\User Data\Default\");

            Dictionary<string, string> regValueAppData = new Dictionary<string, string>();
            regValueAppData.Add("firefox", @"AppData");
            regValueAppData.Add("chrome", @"Local AppData");
            regValueAppData.Add("edge", @"Local AppData");

            const string regKeyFolders = @"HKEY_USERS\<SID>\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";

            string[] keys = Registry.Users.GetSubKeyNames();
            List<string> paths = new List<string>();

            foreach (string sid in keys)
            {
                string appDataPath = Registry.GetValue(regKeyFolders.Replace("<SID>", sid), regValueAppData[browser], null) as string;
                if (appDataPath != null)
                {
                    string path = appDataPath + appendPath[browser];    //Append the rest of the path to avoid overtly long search-times
                    paths.Add(path);
                }
            }
            return paths;
        }

        public static string UsersLastEntry(string user, string log_path)
        {
            List<string> allLinesText = File.ReadAllLines(log_path).ToList();
            allLinesText.Reverse();
            foreach (string line in allLinesText)
            {
                string[] split_string = line.Split(';');
                string user_in_line = split_string[1];
                if (user_in_line == user)
                {
                    return line;

                }
            }
            DateTime begin = new DateTime();
            string lastEntry = begin + ";" + user; ;
            return lastEntry;
        }

        public static void CheckFileSize(string browser, string lastLine)
        {

            string log_path = AppDomain.CurrentDomain.BaseDirectory + @"\Logs\" + browser + "_log.txt";
            FileInfo file = new FileInfo(log_path);

            long bytes = file.Length;
            long kb = bytes / 1024;
            long mb = kb / 1024;

            if (mb >= 5)
            {
                System.IO.File.Move(log_path, log_path + "_old");

                using (StreamWriter sw = File.AppendText(log_path))
                {
                    sw.WriteLine(lastLine);
                }

            }
        }

    }
}
