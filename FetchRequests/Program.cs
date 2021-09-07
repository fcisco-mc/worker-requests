using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Web.Administration;
using System.IO;
using System.Reflection;


namespace FetchRequests {
    class Program {

        private static string _currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        // needs testing with multiple urls and no urls

        static void Main(string[] args) {

            // args[0] Request URL to catch. Multiple URLs must be separated by pipe | "All" to monitor all requests
            // args[1] Execution time threshold in seconds - integer
            // args[2] Rounds to execute - integer
            // args[3] Sleep time between checks in seconds - integer

            string rawUrls;
            int execTime, rounds, sleepTime, currentRound;
            bool caughtRequests;

            if (args.Length.Equals(0) || args[1].Length.Equals(0)) {

                // implement help section with examples
                Console.WriteLine("Invalid arguments. Run FetchRequests.exe help for more information or read the README");
                Console.ReadLine();
                return;

            } else {

                // input validations
                try {

                    rawUrls = args[0].Trim().ToString();
                    execTime = Convert.ToInt32(args[1]) * 1000;

                    if (args[2].Length.Equals(0)) {

                        rounds = 1; // default rounds - single

                    } else {

                        rounds = Convert.ToInt32(args[2]);
                    }

                    if (args[3].Length.Equals(0)) {

                        sleepTime = 60 * 1000; // default sleep time - 60 seconds

                    } else {

                        sleepTime = Convert.ToInt32(args[3]) * 1000;

                    }

                } catch (Exception e) {

                    Console.WriteLine("Invalid arguments. Run FetchRequests.exe help for more information or read the README");
                    Console.ReadLine();
                    return;

                }

            }


            // inputs processing
            List<string> parsedUrls = new List<string>();
            string command;

            parsedUrls = rawUrls.ToLower().Split('|').ToList();

            Logger.TraceLog("Urls: " + rawUrls + "; " + "Execution time thresold: " + execTime + "; " + "Rounds: " + rounds + "; " + "Sleep time: " + sleepTime + ";");


            // program
                using (ServerManager manager = new ServerManager()) {

                currentRound = 0;
                while(currentRound < rounds) {

                    caughtRequests = false;

                    foreach (WorkerProcess proc in manager.WorkerProcesses) {

                        if(!(proc.AppPoolName.ToLower().Equals("outsystemsserverapiapppool") || proc.AppPoolName.ToLower().Equals("outsystemsserveridentityapppool"))) {

                            List<string> requests = new List<string>();
                            RequestCollection rc;

                            rc = proc.GetRequests(execTime);

                            foreach (string url in parsedUrls) {

                                foreach (var request in rc) {

                                    if (request.Url.ToLower().Contains(url)) {

                                        Logger.TraceLog("Caught request: " + request.Url + "; " + "Execution time elapsed: " + request.TimeElapsed);
                                        Logger.TraceLog("Starting to catch thread dumps");
                                        Console.WriteLine("Starting to catch thread dumps");

                                        caughtRequests = true;
                                        command = Path.Combine(_currDir, Path.Combine("OSDiagTool", "OSDiagTool.exe"));
                                        command = "\"" + command + "\"" + " RunCmdLine";

                                        Logger.TraceLog("Executing OSDiagTool on: " + command);

                                        CmdHelper.RunCommand(command); // Get thread dumps; Configuration file must be set for thread dumps only

                                        Logger.TraceLog("Finished catching thread dumps");
                                        Console.WriteLine("Finished catching thread dumps");

                                        currentRound++;
                                        if(!currentRound.Equals(rounds)) Thread.Sleep(sleepTime);
                                        break;

                                    }
                                }

                                if(caughtRequests) break;

                            }
                        }

                        if (caughtRequests) break;

                    } 
                }

                Logger.TraceLog("Exiting console app");
                Console.WriteLine("Exiting console app");

            }
        }
    }
}
