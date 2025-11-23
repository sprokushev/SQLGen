@echo off
rem вернуть список веток-версий еще не влитых в master
rem %1 папка проекта GIT
rem %2 префикс версии

set GIT=%1
set PREFIX=%2
if .%PREFIX%.==.. set PREFIX=prmd
set TMPFILE=%TEMP%\git-nomergedversion.log

if .%GIT%.==.. goto :help

rem echo %GIT% %PREFIX% %TMPFILE%

echo. > %TMPFILE%

echo Y | start /B /wait git-nomerged-ver.sh %GIT% %PREFIX% %TMPFILE%

type %TMPFILE% | grep.cmd %PREFIX%

set ERROR=%ERRORLEVEL%
exit %ERROR%
got :eof

rem ---------------------------------------------------------------------
:help
Echo.
Echo Type list version branches not merged in master
Echo Execute git-nomerged-ver.sh
Echo.
Echo %0 folder [prefix]
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo    prefix - GIT project version prefix: prmd (default)
Echo.
Echo Returned code and environment variable ERROR return:
Echo    ERRORLEVEL
Echo    10001 (wrong input parameters)
Echo.
Echo Example:
Echo    %0 D:\Projects\dev_promed_pg prmd

set ERROR=10001
exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
