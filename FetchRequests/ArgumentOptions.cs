using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace RequestMonitor
{
    class ArgumentOptions
    {
        public enum MonitorType
        {
            request,
            cpuMin,
            cpuBase
        }

        [Option(shortName:'m', longName:"monitor", Required = true, HelpText = "Monitorization type to be used: request, cpuMin or cpuBase")]
        public MonitorType monitor { get; set; }

        [Option(shortName:'u', longName:"requestUrl", SetName = "request", Default = "All", HelpText = "Url to monitor: multiple Urls must be separated by pipe | - \"All\" to monitor all requests. Use with monitor option request")]
        public string RequestUrls { get; set; }

        [Option(shortName:'t', longName:"requestThreshold", SetName = "request", Default = 100, HelpText = "Execution time threshold in seconds - use with monitor option request")]
        public int RequestThreshold { get; set; }

        [Option(shortName:'r', longName:"rounds", Group = "common", Default = 5, HelpText = "Rounds to execute - use with all monitor options")]
        public int Rounds{ get; set; }

        [Option(shortName:'s', longName:"sleepTime", Group ="common", Default = 30, HelpText = "Sleep time between checks in seconds - use with all monitor options")]
        public int SleepTime { get; set; }

        [Option(shortName:'c', longName:"cpuThreshold", SetName = "cpu", Default = 90, HelpText = "CPU threshold - accepts value between 1 and 99. Use with monitor option cpuBase and cpuMin")]
        public int CpuThreshold { get; set; }

        [Option(shortName: 'b', longName: "cpuTimeBase", SetName = "cpu", Default = 300, HelpText = "CPU Time base in seconds - use with monitor option cpuBase when monitoring for constant CPU usage above cpuThreshold for this time period")]
        public int CpuTimeBase { get; set; }

    }
}
