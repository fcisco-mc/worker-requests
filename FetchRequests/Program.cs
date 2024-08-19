using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Web.Administration;
using System.IO;
using System.Reflection;
using System.Diagnostics;

/* RequestMonitor is a console application to run in Windows servers. It can monitor IIS requests and capture thread dumps based on request execution times or machine CPU thresholds
 * Run RequestMonitor.exe help for more information */  

namespace RequestMonitor {
    class Program {

        private static string _currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static string rawUrls;
        private static int execTime, rounds, sleepTime, currentRound, cpuThreshold;
        private static bool caughtRequests, monitorAll = false;
        private static MonitorType monitorType;
        private static string command = "\"" + Path.Combine(_currDir, Path.Combine("OSDiagTool", "OSDiagTool.exe")) + "\"" + " RunCmdLine";
        private static bool sleep = false;
        private static string invalidArgumentsErr = "Invalid arguments. Read the README to see a working example or run .exe help";

        private enum MonitorType
        {
            request,
            cpu
        }

        static void Main(string[] args) {

            try {

                if (!ValidInputs(args)) {
                    throw new Exception(invalidArgumentsErr);
                }

                if (monitorType.Equals(MonitorType.request))
                {
                    RequestMonitor();
                    return;
                }

                if (monitorType.Equals(MonitorType.cpu))
                {
                    CPUMonitor();
                }

            } catch (Exception e) {

                Console.WriteLine(e.Message + "\n" + e.StackTrace);
                Console.ReadLine();
                
                return;
            }
        }

        private static void CPUMonitor()
        {
            float cpuCounterVal;
            Console.WriteLine("CPU monitorization started");
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            Thread.Sleep(1000); // used to allow the counter to establish a baseline and avoid returning a 0.0 value in the first call of NextValue()
            cpuCounterVal = cpuCounter.NextValue();

            currentRound = 0;
            while (currentRound < rounds)
            {
                caughtRequests = false;
                if (sleep) Thread.Sleep(sleepTime);

                cpuCounterVal = cpuCounter.NextValue();

                if (cpuCounterVal >= cpuThreshold)
                {
                    caughtRequests = true;
                    Logger.TraceLog("Caught requests with CPU over " + cpuThreshold);
                    Console.WriteLine("Caught requests with CPU over " + cpuThreshold + ". Current CPU: " + cpuCounterVal);

                    CmdHelper.RunCommand(command); // Get thread dumps; Configuration file must be set for thread dumps only

                    currentRound++;
                    Logger.TraceLog("Thread dumps collected. Current round: " + currentRound);
                    Console.WriteLine("Thread dumps collected. Current round: " + currentRound);
                }

                sleep = true;
            }
        }



    private static void RequestMonitor()
        {
            // inputs processing
            List<string> parsedUrls = new List<string>();

            parsedUrls = rawUrls.ToLower().Split('|').ToList();

            Logger.TraceLog("Urls: " + rawUrls + "; " + "Execution time thresold: " + execTime + "ms ; " + "Rounds: " + rounds + "; " + "Sleep time: " + sleepTime + "ms ;");


            // program
            using (ServerManager manager = new ServerManager())
            {

                Console.WriteLine("Monitorization started");
                currentRound = 0;
                while (currentRound < rounds)
                {

                    caughtRequests = false;
                    if (sleep) Thread.Sleep(sleepTime);

                    foreach (WorkerProcess proc in manager.WorkerProcesses)
                    {

                        if (!(proc.AppPoolName.ToLower().Equals("outsystemsserverapiapppool") || proc.AppPoolName.ToLower().Equals("outsystemsserveridentityapppool")))
                        {

                            List<string> requests = new List<string>();
                            RequestCollection rc;

                            rc = proc.GetRequests(execTime);

                            foreach (string url in parsedUrls)
                            {

                                foreach (var request in rc)
                                {

                                    if (request.Url.ToLower().Contains(url) || (monitorAll & !request.Url.Length.Equals(0)))
                                    {

                                        Logger.TraceLog("Caught request: " + request.Url + "; " + "Execution time elapsed: " + request.TimeElapsed);
                                        Logger.TraceLog("Starting to catch thread dumps. Round: " + currentRound);
                                        Console.WriteLine("Starting to catch thread dumps. Round: " + currentRound);

                                        caughtRequests = true;

                                        Logger.TraceLog("Executing OSDiagTool on: " + command);

                                        CmdHelper.RunCommand(command); // Get thread dumps; Configuration file must be set for thread dumps only

                                        Logger.TraceLog("Finished catching thread dumps. Round: " + currentRound);
                                        Console.WriteLine("Finished catching thread dumps. Round: " + currentRound);

                                        currentRound++;
                                        break;

                                    }
                                }

                                if (caughtRequests) break;

                            }
                        }

                        if (caughtRequests) break;

                    }

                    sleep = true;

                }

                Logger.TraceLog("Exiting console app");
                Console.WriteLine("Exiting console app");
            }
        }

        private static bool ValidInputs(string[] args)
        {
            if (args[0].ToLower().Equals("help"))
            {
                Console.WriteLine(@"** Utility tool to help monitor long running and CPU costly requests ** " + "\n" +
                                        "Argument 1: Monitorization type - acccepts 'cpu' or 'request'" + "\n");
                Console.WriteLine(@"* Monitorization type cpu *");
                Console.WriteLine("\t Argument 2: CPU threshold - accepts value between 1 and 99" + "\n" +
                    "\t Argument 3: Rounds to execute - integer" + "\n" +
                    "\t Argument 4: Sleep time between checks in seconds - integer" + "\n" +
                    "\t Example: .exe cpu 90 5 20" + "\n");


                Console.WriteLine(@"* Monitorization type request *");
                Console.WriteLine("\t Argument 2:  Request URL to catch. Multiple URLs must be separated by pipe | - \"All\" to monitor all requests" + "\n" +
                    "\t Argument 3: Execution time threshold in seconds - integer" + "\n" +
                    "\t Argument 4: Rounds to execute - integer" + "\n" +
                    "\t Argument 5: Sleep time between checks in seconds - integer" + "\n" +
                    "\t Examples: \n" +
                    "\t \t.exe request \"MyApp/WebPage\" 70 3 300 \n" +
                    "\t \t.exe request \"All\" 40 10 20");

                Console.ReadLine();
                return false;
            }

            // validate length
            if (args.Length.Equals(0) || args[1].Length.Equals(0) || args[2].Length.Equals(0))
            {
                Console.WriteLine(invalidArgumentsErr + " - argument 1, 2 or 3 is missing");
                Console.ReadLine();
                return false;
            }

            if (args[0].ToLower().Equals("cpu")) {
                monitorType = MonitorType.cpu;
                return ValidCPUInputs(args);
            }

            if (args[0].ToLower().Equals("request"))
            {
                monitorType = MonitorType.request;
                return ValidRequestInputs(args);
            }

            return false;
        }

        private static bool ValidRequestInputs(string[] args)
        {
            // Request case input validations
            try
            {
                rawUrls = args[1].Trim().ToString();
                if (rawUrls.Equals("All")) monitorAll = true;
                execTime = Convert.ToInt32(args[2]) * 1000;

                if (args[3].Length.Equals(0))
                {
                    rounds = 1; // default rounds - single
                }
                else
                {
                    rounds = Convert.ToInt32(args[2]);
                }

                if (args[4].Length.Equals(0))
                {
                    sleepTime = 600 * 1000; // default sleep time - 600 seconds
                }
                else
                {
                    sleepTime = Convert.ToInt32(args[3]) * 1000;
                }

                return true;

            }
            catch (Exception e)
            {

                Console.WriteLine(invalidArgumentsErr + " - error validating request inputs" + "\n" + e.StackTrace);
                Console.ReadLine();
                return false;

            }
        }

        private static bool ValidCPUInputs(string[] args)
        {
            try
            {
                cpuThreshold = Convert.ToInt32(args[1]);
                rounds = Convert.ToInt32(args[2]);
                sleepTime = Convert.ToInt32(args[3]) * 1000;

                return true;

            } catch (Exception e)
            {
                Console.WriteLine(invalidArgumentsErr + " - error validating cpu inputs" + "\n" + e.StackTrace);
                Console.ReadLine();
                return false;
            }
        }

    }
}
