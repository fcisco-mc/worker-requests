using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Web.Administration;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using CommandLine;

/* RequestMonitor is a console application to run in Windows servers. It can monitor IIS requests and capture thread dumps based on request execution times or machine CPU thresholds
 * Run RequestMonitor.exe help for more information */  

namespace RequestMonitor {
    class Program {

        private static string _currDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        private static int currentRound, cpuThreshold;
        private static bool caughtRequests, monitorAll = false;
        private static string command = "\"" + Path.Combine(_currDir, Path.Combine("OSDiagTool", "OSDiagTool.exe")) + "\"" + " RunCmdLine";
        private static bool sleep = false;
        private static string invalidArgumentsErr = "Invalid arguments. Read the README to see a working example or run .exe help";

        static void Main(string[] args) {

            Process p = Process.GetCurrentProcess();
            p.PriorityClass = ProcessPriorityClass.High;

            var options = Parser.Default.ParseArguments<ArgumentOptions>(args)
                .WithParsed<ArgumentOptions>(o =>
                {
                    switch (o.monitor)
                    {
                        case ArgumentOptions.MonitorType.request:
                            //Monitor execution time of requests based on url or All requests
                            RequestMonitor(o.RequestUrls, o.RequestThreshold*1000, o.Rounds, o.SleepTime*1000);
                            break;

                        case ArgumentOptions.MonitorType.cpuMin:
                            // Monitor CPU - minimum threshold provided to trigger thread dump collection
                            CPUMonitor(o.Rounds, o.SleepTime*1000, o.CpuThreshold, o.monitor);
                            break;

                        case ArgumentOptions.MonitorType.cpuBase:
                            // Monitor CPU - current CPU must be above base for the defined period to trigger thread dump collection
                            CPUMonitor(o.Rounds, o.SleepTime*1000, o.CpuThreshold, o.monitor, o.CpuTimeBase);
                            break;

                        default:
                            //handle error
                            Console.WriteLine(invalidArgumentsErr);
                            break;
                    }
                });

            Console.ReadLine();
        }

        private static void CPUMonitor(int rounds, int sleepTime, float cpuThreshold, ArgumentOptions.MonitorType monitorType, int timeBaseSec = 0)
        {
            float cpuCounterVal;
            Console.WriteLine("CPU monitorization started");
            PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            Thread.Sleep(500); // used to allow the counter to establish a baseline and avoid returning a 0.0 value in the first call of NextValue()
            cpuCounterVal = cpuCounter.NextValue();

            Stopwatch sw = new Stopwatch();

            currentRound = 0;
            while (currentRound < rounds)
            {
                caughtRequests = false;
                if (sleep) Thread.Sleep(sleepTime);

                cpuCounterVal = cpuCounter.NextValue();

                if (monitorType.Equals(ArgumentOptions.MonitorType.cpuMin) & cpuCounterVal >= cpuThreshold)
                {
                    caughtRequests = true;
                    Logger.TraceLog("Caught requests with CPU over " + cpuThreshold);
                    Console.WriteLine("Caught requests with CPU over " + cpuThreshold + ". Current CPU: " + cpuCounterVal);

                    CmdHelper.RunCommand(command); // Get thread dumps; Configuration file must be set for thread dumps only

                    currentRound++;
                    Logger.TraceLog("Thread dumps collected. Current round: " + currentRound);
                    Console.WriteLine("Thread dumps collected. Current round: " + currentRound);
                }
                else if (monitorType.Equals(ArgumentOptions.MonitorType.cpuBase) & cpuCounterVal >= cpuThreshold)
                {
                    while (currentRound < rounds & (cpuCounterVal = cpuCounter.NextValue()) >= cpuThreshold)
                    {
                        if (!sw.IsRunning) { sw.Start(); }

                        Logger.TraceLog("CPU Counter value: " + cpuCounterVal + ". Elapsed time: " + sw.ElapsedMilliseconds/1000 + "s");
                        if (sw.ElapsedMilliseconds/1000 >= timeBaseSec)
                        {
                            caughtRequests = true;
                            Logger.TraceLog("Caught requests with CPU greater than " + cpuThreshold + " and over " + timeBaseSec + " seconds");
                            Console.WriteLine("Caught requests with CPU over " + cpuThreshold + " and over " + timeBaseSec + " seconds" + ". Current CPU: " + cpuCounterVal);

                            CmdHelper.RunCommand(command); // Get thread dumps; Configuration file must be set for thread dumps only

                            currentRound++;
                            sw.Reset();
                            Logger.TraceLog("Thread dumps collected. Current round: " + currentRound);
                            Console.WriteLine("Thread dumps collected. Current round: " + currentRound);
                        } else
                        {
                            Logger.TraceLog("Sleeping for 10 seconds...");
                            Thread.Sleep(10000);
                        }
                    }

                    sw.Reset();
                    Logger.TraceLog("Stopwatch reset due to low cpu counter. Round: " + currentRound);

                }

                sleep = true;
            }

            Console.WriteLine("Monitorization finished. Click enter to exit");
            Logger.TraceLog("Monitorization finished");
        }


    private static void RequestMonitor(string rawUrls, int execTime, int rounds, int sleepTime)
        {
            // inputs processing
            List<string> parsedUrls = new List<string>();

            parsedUrls = rawUrls.ToLower().Split('|').ToList();

            Logger.TraceLog("Urls: " + rawUrls + "; " + "Execution time thresold: " + execTime + "ms ; " + "Rounds: " + rounds + "; " + "Sleep time: " + sleepTime + "ms ;");

            // program
            using (ServerManager manager = new ServerManager())
            {
                Console.WriteLine("Request Monitorization started");
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

            Console.WriteLine("Monitorization finished. Click enter to exit");
            Logger.TraceLog("Monitorization finished");
        }
    }
}
