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
                        rc.SelectMany(r => r.Url);
                        Console.WriteLine(rc.SelectMany(r => r.Url));

                        



                        Console.WriteLine(proc.GetRequests(1000));

                        Console.ReadLine();


                    }
                }
            }
        }
    }
}
