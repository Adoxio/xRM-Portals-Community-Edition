@echo off

echo ----
echo Usage: %0
echo  - use the IIS Express system tray tool to browse the website
echo  - if IIS Express shows errors at start, you may need to close an existing IIS Express from the system tray or configure the site to run on another port
echo ----

set exec-iisexpress=%PROGRAMFILES%\IIS Express\iisexpress.exe
set appHostConfigPath=%~dp0applicationhost.config
set defaultSiteName="Adxstudio Portals"

@echo on

REM variables referenced within the applicationhost.config
set SCRIPT_PATH=%~dp0

start "" "%exec-iisexpress%" /config:"%appHostConfigPath%" /site:%defaultSiteName%
start http://localhost:7800