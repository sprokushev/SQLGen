@echo off
rem вернуть список веток-версий
rem %1 папка проекта GIT
rem %2 префикс версии
rem Возвращает ERRORLEVEL или 10001 (неверные параметры запуска)

set GIT=%1
set PREFIX=%2
if .%PREFIX%.==.. set PREFIX=prmd

set GITEXE=
for /f "usebackq delims=;" %%f in (`where /F git`) do (
    set "GITEXE=%%f"
)
IF .%GITEXE%. EQU .. (
IF EXIST "C:\Program Files\Git\bin\git.exe" (
set GITEXE="C:\Program Files\Git\bin\git.exe"
)
)
IF .%GITEXE%. EQU .. set GITEXE=git

if .%GIT%.==.. goto :help
if .%PREFIX%.==.. goto :help

%GITEXE% --no-pager -C %GIT% branch -r --list origin/%PREFIX%.* 
%GITEXE% --no-pager -C %GIT% branch --list %PREFIX%.* 

set ERROR=%ERRORLEVEL%
exit %ERROR%
got :eof

rem ---------------------------------------------------------------------
:help
Echo.
Echo Type full list version branches
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
