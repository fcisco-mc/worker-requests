using System;
using System.IO;
using System.Reflection;

namespace RequestMonitor{
    class Logger {

        private static string _consoleLog = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "ConsoleLog.txt");

        public static void TraceLog(string message) {

            File.AppendAllText(_consoleLog, DateTime.Now + "\t" + "[INFO] " + message + Environment.NewLine);

        }

    }
}
