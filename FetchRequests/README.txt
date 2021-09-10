How to use:
	 -args[0] Request URL to catch. Multiple URLs must be separated by pipe | "All" to monitor all requests -- MANDATORY
	 -args[1] Execution time threshold in seconds - integer -- MANDATORY
	 -args[2] Rounds to execute - integer -- MANDATORY
	 -args[3] Sleep time between monitorization rounds in seconds - integer -- OPTIONAL; default is 600 seconds
	 
 Examples:
	- FetchRequests.exe "MyApp/WebPage" 70 3 300 
		** Monitors only "MyApp/WebPage" url for execution times larger than 70 seconds. Collects a maximum of 3 threads dumps then exits the program. Waits 300 seconds between each round (round only increments if the request is caught)
	- FetchRequests.exe "MyApp|MySecondApp|MyThirdApp" 40 1 100
		** Monitors the requests with URLs "MyApp", "MySecondApp", "MyThirdApp" for execution times larger than 40 seconds. Collects one thread dump and exits the program. Waits 100 seconds between each monitorization round.
	- FetchRequests.exe "All" 500 5 600
		** Monitors all requests for execution times larger than 500 seconds. Collects a maximum of 5 threads dumps then exits the program. Waits 600 seconds between each monitorization round.
		