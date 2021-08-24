using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Web.Administration;

namespace FetchRequests {
    class Program {
        static void Main(string[] args) {

            using (ServerManager manager = new ServerManager()) {

                foreach (WorkerProcess proc in manager.WorkerProcesses) {

                    if (proc.AppPoolName.Equals("OutSystemsApplications")) {


                        List<string> requests = new List<string>();
                        RequestCollection rc;

                        rc = proc.GetRequests(10000);

                        // Not tested
                        foreach(var request in rc){

                            if (request.Url.ToLower().Contains("/servicecenter")) {

                                CmdHelper.RunCommand(@"\OSDiagTool\OSDiagTool.exe RunCmdLine"); // Get thread dumps; Configuration file must be set for thread dumps only
                            }

                        }                    

                        Console.ReadLine();


                    }
                }
            }
        }
    }
}
