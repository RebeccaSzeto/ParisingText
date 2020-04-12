using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessLog
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderPath = @"C:\Users\Rebecca.Szeto\Downloads\";
            string line = string.Empty;
            List<AccessLog> accesses = new List<AccessLog>();
            List<AccessLog> accessesFiltered = new List<AccessLog>();
            List<string> users = new List<string>();
            List<User> logins = new List<User>();
            List<User> logouts = new List<User>();
            List<UserDuration> userDurations = new List<UserDuration>();
            DateTime tempDT = DateTime.Now;
            TimeSpan temp = DateTime.Now.TimeOfDay;
            TimeSpan temp2 = DateTime.Now.TimeOfDay;

            try
            {
                foreach (string file in Directory.EnumerateFiles(folderPath, "*.log"))
                {
                    accesses = (from c in
                                    (from row in File.ReadAllLines(file)
                                    let columns = row.Split(' ')
                                    where !columns[2].Contains("[")
                                    select new AccessLog
                                    {
                                        Time = DateTime.ParseExact(columns[0] + " " + columns[1], "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None),
                                        IP = columns[3].Replace(";", string.Empty),
                                        User = columns[4].Replace(";", string.Empty).Split('@')[0].ToString(),
                                        Computer = columns[4].Replace(";", string.Empty).Split('@')[1].ToString(),
                                        LoggedIn = columns[5].Split(';')[0].Replace(";", string.Empty).Contains("IN_")
                                    })
                            select c).ToList();
                }
                users = accesses.Select(x => x.User).Distinct().ToList();
                
                int newLoginDiff = 60;
                foreach (string u in users)
                {
                    accessesFiltered = accesses.Where(s => s.User == u).ToList();
                    int total = accessesFiltered.Count();
                    for (int x = 0; x < total-1; x++)
                    {
                        if (x == 0)
                        {
                            temp = accessesFiltered[x].Time.TimeOfDay;
                            temp2 = accessesFiltered[x].Time.TimeOfDay;
                            tempDT = accessesFiltered[x].Time;
                            logins.Add(new User(accessesFiltered[x].User, tempDT));
                        }
                        if (accessesFiltered[x+1].Time.TimeOfDay.Subtract(temp).TotalSeconds >= newLoginDiff )
                        {
                            logouts.Add(new User(accessesFiltered[x].User, accessesFiltered[x].Time));
                            //if (!temp2.Equals(accessesFiltered[x].Time))
                            //{
                                temp2 = accessesFiltered[x+1].Time.TimeOfDay;
                                tempDT = accessesFiltered[x+1].Time;
                            logins.Add(new User(accessesFiltered[x].User, tempDT));
                            //Console.WriteLine(tempDT + " " + accessesFiltered[x].Time.TimeOfDay);
                            temp = accessesFiltered[x+1].Time.TimeOfDay;
                            //}
                        }
                        else
                        {
                            temp = accessesFiltered[x+1].Time.TimeOfDay;    
                        }
                    }
                    logouts.Add(new User(accessesFiltered[total-1].User, accessesFiltered[total-1].Time));
                }
                for (int y = 0; y < logins.Count(); y++)
                {
                    temp = logouts[y].TimeRecorded.TimeOfDay.Subtract(logins[y].TimeRecorded.TimeOfDay);
                    userDurations.Add(new UserDuration(logins[y].UserName.Split('@')[0].ToString(), logins[y].UserName.Split('@')[1].ToString(), logins[y].TimeRecorded, logouts[y].TimeRecorded, (temp.TotalDays > 1) ? temp.TotalDays.ToString() : "0", temp.ToString()));
                }
                FileStream ostrm;
                StreamWriter writer;
                TextWriter oldOut = Console.Out;
                try
                {
                    ostrm = new FileStream("./AccessSummary_"+DateTime.Now.ToShortDateString()+ ".txt", FileMode.OpenOrCreate, FileAccess.Write);
                    writer = new StreamWriter(ostrm);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Cannot open Redirect.txt for writing");
                    Console.WriteLine(e.Message);
                    return;
                }
                Console.SetOut(writer);
                Console.WriteLine("User | Computer | Days logged in | Total time per day");
                foreach (UserDuration u in userDurations) {
                    Console.WriteLine(u.User + " | " + u.Computer + " | " + u.LoggedIn + " | " + u.LoggedOut + " | " + u.DaysLoggedIn + " | " + u.TotalTimePerDay);
                }
                Console.SetOut(oldOut);
                writer.Close();
                ostrm.Close();
                Console.WriteLine("Done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            Console.Read();
        }
    }
    class User
    {
        public User() { }
        public User(string username, DateTime timeRecorded)
        {
            this.UserName = username;
            this.TimeRecorded = timeRecorded;
        }
        public string UserName { get; set; }
        public DateTime TimeRecorded { get; set; }
    }
    class UserDuration
    {
        public UserDuration() { }
        public UserDuration(string username, string computer, DateTime din, DateTime dout, string daysLoggedIn, string timePerDay)
        {
            this.User = username;
            this.Computer = computer;
            this.LoggedIn = din;
            this.LoggedOut = dout;
            this.DaysLoggedIn = daysLoggedIn;
            this.TotalTimePerDay = timePerDay;
        }
        public string User { get; set; }
        public string Computer { get; set; }
        public DateTime LoggedIn { get; set; }
        public DateTime LoggedOut { get; set; }
        public string DaysLoggedIn { get; set; }
        public string TotalTimePerDay { get; set; }
    }
    class AccessLog
    {
        public AccessLog() { }
        public AccessLog(DateTime time, string ip, string user, string computer, string loginStatus, int userId, string timeId)
        {
            this.Time = time;
            this.IP = ip;
            this.User = user;
            this.Computer = computer
            this.LoggedIn = loginStatus.Contains ("IN_");
            this.UserId = userId;
            this.TimeId = timeId;
        }
        public List<AccessLog> userFilter(List<AccessLog> fullList)
        {
            List<AccessLog> filteredOneUser = new List<AccessLog>();
            return filteredOneUser;
        }
        public AccessLog(string user, DateTime time) { }
        public DateTime Time { get; set; }
        public string IP { get; set; }
        public string User { get; set; }
        public string Computer { get; set; }
        public bool LoggedIn { get; set; }
        public int UserId { get; set; }
        public string TimeId { get; set; }
       
    }
}
