using System;
using System.Text;
using System.Diagnostics;

namespace RequestMonitor {
    class CmdHelper {

        public static void RunCommand(string command, bool isHidden = true) {

            try {

                Process process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();

                if (isHidden)
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                startInfo.FileName = "cmd.exe";
                startInfo.Arguments = "/C " + command;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.StandardOutputEncoding = Encoding.UTF8;

                startInfo.WindowStyle = ProcessWindowStyle.Minimized;
                startInfo.CreateNoWindow = true;

                process.StartInfo = startInfo;

                process.Start();
                process.WaitForExit();
                process.Close();

            } catch (Exception) {

            }


        }

    }

}
