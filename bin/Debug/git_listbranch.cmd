@echo off
rem вернуть список веток (всех)
rem %1 папка проекта GIT
rem Возвращает ERRORLEVEL или 10001 (неверные параметры запуска)

set GIT=%1

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

%GITEXE% --no-pager -C %GIT% branch -r --list
%GITEXE% --no-pager -C %GIT% branch --list

set ERROR=%ERRORLEVEL%
exit %ERROR%
got :eof

rem ---------------------------------------------------------------------
:help
Echo.
Echo Type list branches
Echo.
Echo %0 folder
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo.
Echo Returned code and environment variable ERROR return:
Echo    ERRORLEVEL
Echo    10001 (wrong input parameters)
Echo.
Echo Example:
Echo    %0 D:\Projects\dev_promed_pg

set ERROR=10001
exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
