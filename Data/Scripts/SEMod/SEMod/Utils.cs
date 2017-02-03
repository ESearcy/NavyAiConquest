using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;

namespace SEMod
{
    public class Util
    {
        public static Object _lock = new Object();

        private static String _errorFileName = "error";
        Dictionary<string, List<string>> logs = new Dictionary<string, List<string>>();

        private static Util _instance;
        public static bool debuggingOn = false;
        public bool DebuggingOn()
        {
            return debuggingOn;
        }

        internal static void NotifyHud(String message)
        {
            lock (_lock)
            {
                Initalize();
                MyAPIGateway.Utilities.ShowNotification(message, 5000);
            }
        }

        public static void Log(String path, String line)
        {
            lock (_lock)
            {
                Initalize();
                _instance.LOG(path, line);
            }
        }

        public static void LogError(String className, String error)
        {
            lock (_lock)
            {
                Initalize();
                _instance.LOG(_errorFileName, "["+className+"] "+error);
            }
        }


        public static void Initalize()
        {
            if (_instance == null)
                _instance = new Util();
        }

        public Util()
        {
            debuggingOn = false;
        }

        public static void SaveLogs()
        {
            lock (_lock)
            {
                Initalize();
                _instance.SubTaskSavLogs();
            }
        }

        public void LOG(String classname, String line)
        {
            String path = classname + ".log";
            lock (_lock)
            {
                if (!debuggingOn)
                    return;

                if (!path.Equals("none") && classname != null)
                {
                    if (logs.ContainsKey(path))
                        logs[path].Add("[" + classname + "][" + DateTime.Now + "] " + line + "\n");
                    else
                        logs.Add(path, new List<string>() { "[" + classname + "][" + DateTime.Now + "] " + line + "\n" });

                    while (logs[path].Count >= 5000)
                        logs[path].Remove(logs[path].First());
                }
            }
        }

        private void SubTaskSavLogs()
        {
            foreach (var path in logs)
            {
                using (var mWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(path.Key, typeof(Util)))
                {
                    foreach (var log in path.Value)
                    {
                        mWriter.WriteLine(log + "\n");
                    }

                    mWriter.Flush();
                }
            }
        }
    }
}