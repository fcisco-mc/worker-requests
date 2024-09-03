RequestMonitor is a utility tool that collects thread dumps and the list of requests running in IIS.
The thread dump collection is initiated when defined conditions are met. These conditions can be based on request execution time (for all requests or specific endpoints),
minimum CPU used or constant CPU above a defined base level.


  -m, --monitor             Required. Monitorization type to be used: request, cpuMin or cpuBase
 _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _
|** Common to all Monitor options                                                                                             |
|  -r, --rounds              (Group: common) (Default: 5) Rounds to execute - use with all monitor options                    |
|                                                                                                                             |
|  -s, --sleepTime           (Group: common) (Default: 30) Sleep time between checks in seconds - use with all monitor        |
|                            options                                                                                          |
|_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _|
 _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _
|** -m request options                                                                                                        |
|  -u, --requestUrl          (Default: All) Url to monitor: multiple Urls must be separated by pipe | - "All" to monitor      |
|                           all requests. Use with monitor option request                                                     |
|  -t, --requestThreshold    (Default: 100) Execution time threshold in seconds - use with monitor option request|            |
|_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _|
 _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _
|** -m cpuMin & cpuBase options                                                                                               |
|  -c, --cpuThreshold        (Default: 90) CPU threshold - accepts value between 1 and 99. Use with monitor option            |
|                            cpuBase and cpuMin                                                                               |
|_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _|
 _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _
|** -m cpuBase options																									                                                      |
|  -b, --cpuTimeBase         (Default: 300) CPU Time base in seconds - use with monitor option cpuBase when monitoring for    |
|							 constant CPU usage above cpuThreshold for this time period										                                  |
|_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _|

  --help                    Display this help screen.

  --version                 Display version information.

  Usage examples:
	- RequestMonitor.exe -m request -u MyApp/Page -t 100 -r 10 -s 30
	- RequestMonitor.exe -m cpuMin -c 80 -r 10 -s 20
	- RequestMonitor.exe -m cpuBase -c 90 -b 180
