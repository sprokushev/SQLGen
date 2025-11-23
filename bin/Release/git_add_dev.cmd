@echo off
set STEP=start %0

rem Добавляем файлы во все клонированные репозитории dev_ в одну и ту же ветку
rem %1 папка проектов GIT
rem %2 ветка

set GIT=%1

if .%GIT%.==.. goto :help
echo -------------------------------------
echo GIT project = %GIT%
echo current dir = %cd%
echo command line parameters:
echo %0 %1 %2 %3 %4 %5
echo -------------------------------------
echo.

set LOCATION=%cd%
SET LOCATION2=%LOCATION:'=%
SET LOCATION2=%LOCATION2:"=%
set LOCDISK=%LOCATION2:~0,2%

set BRANCH=%2
if .%BRANCH%.==.. goto :help

forfiles /P %1 /M dev_* /C "cmd /c if @isdir==TRUE CALL %LOCATION%\git_exec.cmd %LOCATION% git_add.cmd @path %BRANCH%" 

echo cd %LOCATION%
%LOCDISK%
cd %LOCATION%

CALL git_push_dev %1 %2

exit
rem ---------------------------------------------------------------------




rem ---------------------------------------------------------------------
:help
Echo.
Echo %0 folder branch
Echo    folder - GIT project folder: D:\Projects
Echo    branch - GIT branch name: test
Echo.
Echo Example:
Echo    %0 D:\Projects test

exit
rem ---------------------------------------------------------------------


