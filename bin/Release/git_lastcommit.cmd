@echo off
rem вернуть дату последнего комита
rem %1 папка проекта GIT
rem %2 ветка задачи
rem Возвращает ERRORLEVEL или 10001 (неверные параметры запуска)

set GIT=%1
set TASK=%2
set TMPFILE=%TEMP%\git-branchage.log

if .%GIT%.==.. goto :help
if .%TASK%.==.. goto :help

echo. > %TMPFILE%

echo Y | start /B /wait git_lastcommit.sh %GIT% %TASK% %TMPFILE%

type %TMPFILE%

set ERROR=%ERRORLEVEL%
exit %ERROR%
goto :eof

rem ---------------------------------------------------------------------
:help
Echo.
Echo Type last commit date
Echo Execute git_lastcommit.sh
Echo.
Echo %0 folder branch
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo    branch - GIT branch name: PROMEDWEB-00000
Echo.
Echo Returned code and environment variable ERROR return:
Echo    ERRORLEVEL
Echo    10001 (wrong input parameters)
Echo.
Echo Example:
Echo    %0 D:\Projects\dev_promed_pg PROMEDWEB-00000

set ERROR=10001
exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
