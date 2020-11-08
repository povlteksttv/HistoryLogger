## HistoryLogger: A light-weight Browser History Logger

HistoryLogger monitors the history databases of Chrome, Firefox, and Microsoft Edge every single minute. The service then updates a readable log file for each browser containing the users activity. Chrome and Edge does not update their respective SQLite databases until the browser is closed, which unfortunately means that HistoryLogger does not check Chrome and Edge while those browsers are running. 

For more information, read the article at Medium: https://medium.com/@povl.teksttv/monitoring-browser-history-using-historylogger-nxlog-and-elk-606877d656fb


## Why HistoryLogger? 

Web-browsers are used for almost everything these days: Reading PDFs, browsing the web, and for internal web-based applications. The 'issue' for most business (and perhaps also parents) is the fact, that almost all internet usage has become encrypted, and unless you are using some form of MiTM (Man-in-the-Middle) proxy with SSL bumping [(see more)](https://wiki.squid-cache.org/Features/SslBump), then you are not seeing the complete picture. Browser history is often used in forensic situation, but why not collect all the information while the activity is occurring? 



## Where are the logs and how do they look? 

For each browser, HistoryLogger, will create a log file located in C:\Program Files\PovlTekstTV\HistoryLogger\Logs. The log files are named: chrome_log.txt, firefox_log.txt, and edge_log.txt. An example of one of these files is shown below: *Line 3, is an example of a full Google-search (Google-søgning is Danish )* 



<img src="https://raw.githubusercontent.com/povlteksttv/HistoryLogger/master/img/example.JPG" style="zoom: 100%;" />



Syntax: Timestamp;User;Title;URL

The logs can easily be collected by programs such as NXLog and be shipped to Security-Onion, ELK or whatever.



## Where to download HistoryLogger?

HistoryLogger is currently available in both 32-bit and 64-bit. It's been tested on both Windows 7 and Windows 10. HistoryLogger can be downloaded from: https://github.com/povlteksttv/HistoryLogger



## What about private web-browsing?

Most browsers support some sort of "private" web-browsing where the users activity is not saved. Such features can be disabled by administrators: [https://www.thewindowsclub.com/disable-private-browsing-internet-explorer-chrome-firefox](https://www.thewindowsclub.com/disable-private-browsing-internet-explorer-chrome-firefox). Private browsing sessions are of course not logged by HistoryLogger.  



## References

I have utilized a bunch of work contributed by others. Please do check out there stuff for more information: 

How to search the relevant sqlite-db's: [https://gist.github.com/dropmeaword/9372cbeb29e8390521c2](https://gist.github.com/dropmeaword/9372cbeb29e8390521c2) 

How to create a Windows installer using Wix: [https://www.youtube.com/watch?v=6Yf-eDsRrnM&t=6593s](https://www.youtube.com/watch?v=6Yf-eDsRrnM&t=6593s)

How to parse Firefox history using PowerShell: [https://laconicwolf.com/2017/12/12/parsing-firefox-history-powershell/](https://laconicwolf.com/2017/12/12/parsing-firefox-history-powershell/) 

... And naturally everyone over on Stackoverflow.com.
