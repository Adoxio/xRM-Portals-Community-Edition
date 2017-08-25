REM Azure startup script for osFamily="3" (Windows Server 2012)

REM Check if this task is running on the compute emulator.

if "%ComputeEmulatorRunning%"=="true" goto :EOF

REM Write log files to E:\approot\bin\AzureStartup\

set logtxt=%~dp0Startup.log.txt
set errtxt=%~dp0Startup.err.txt

powershell.exe -ExecutionPolicy Unrestricted "%~dp0Startup.ps1" >>"%logtxt%" 2>>"%errtxt%"
