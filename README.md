# FivemWindowsManager
Fivem server manager for Windows

## Features
* Schedule automatic server restarts
* Check if server is running every X minutes
* in the folder there is on each reboot a log file

## Usage
Simply download latest [Release](https://github.com/Trsak/FivemWindowsManager/releases/latest), edit config and run  **FivemManager.exe**.

## Config file
* config/manager.conf

```
#FXServer DIR location
FXServerLocation = "D:/Projects/FXServer/server"

#Server data location, place where ConfigFile is located
ServerLocation = "D:/Projects/FiveM"

#main server config file
ConfigFile = "server.cfg"

#Server restart times, split by ";"
RestartTimes = "02:00:05;12:00:05;18:00:05"

#Interval for checking if server is running or not
CrashCheckInterval = 5
```
