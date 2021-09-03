using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.Administration;
using System.IO;
namespace FetchRequests {
    class Program {

        private static string _currDir = AppDomain.CurrentDomain.BaseDirectory;

        static void Main(string[] args) {

            // args[0] Request URL to catch. Multiple URLs must be separated by pipe |
            // args[1] Execution time threshold in seconds - integer
            // args[2] Rounds to execute - integer

            string urls;
            int execTime, rounds;

            if (args.Length.Equals(0) || args[1].Length.Equals(0)) {

                // implement help section with examples
                Console.WriteLine("Invalid arguments. Run FetchRequests.exe help for more information or read the README");

            } else {

                // input validations
                try {

                    urls = args[0].Trim().ToString();
                    execTime = Convert.ToInt32(args[1]);

                    if (args[2].Length.Equals(0)) {

                        rounds = 1; // default rounds - single

                    } else {

                        rounds = Convert.ToInt32(args[2]);
                    }

                } catch (Exception e) {

                    Console.WriteLine("Invalid arguments. Run FetchRequests.exe help for more information or read the README");
                    Console.ReadLine();
                    return;

                }

            }

            // implement program
                using (ServerManager manager = new ServerManager()) {

                foreach (WorkerProcess proc in manager.WorkerProcesses) {

                    if (proc.AppPoolName.Equals("ServiceCenterAppPool")) {


                        List<string> requests = new List<string>();
                        RequestCollection rc;

                        rc = proc.GetRequests(1000);

                        foreach(var request in rc){

                            if (request.Url.ToLower().Contains("/servicecenter")) {

                                CmdHelper.RunCommand(Path.Combine(_currDir, @"OSDiagTool\OSDiagTool.exe RunCmdLine")); // Get thread dumps; Configuration file must be set for thread dumps only

                            }

                        }                    

                        Console.ReadLine();


                    }
                }
            }
        }
    }
}
