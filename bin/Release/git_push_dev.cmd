@echo off
set STEP=start %0

rem Переключаемся на одну и ту же ветку во всех клонированных репозиториях dev_ и делаем git push
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

set LOCATION=%cd%
echo %LOCATION%
echo.

set BRANCH=%2
if .%BRANCH%.==.. goto :help

forfiles /P %1 /M dev_* /C "cmd /c if @isdir==TRUE CALL %LOCATION%\git_exec.cmd %LOCATION% git_push.cmd @path %BRANCH%" 

exit /b
rem ---------------------------------------------------------------------




rem ---------------------------------------------------------------------
:help
Echo.
Echo %0 folder branch
Echo 	folder - GIT project folder: D:\Projects
Echo 	branch - GIT branch name: test
Echo.
Echo Example:
Echo 	%0 D:\Projects test

exit
rem ---------------------------------------------------------------------


