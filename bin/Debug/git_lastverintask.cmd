@echo off
rem вернуть последнюю версию влитую в текущую ветку
rem %1 папка проекта GIT
rem %2 префикс версии
rem %3 ветка задачи
rem Возвращает ERRORLEVEL или 10001 (неверные параметры запуска)

set GIT=%1
set PREFIX=%2
set TASK=%3
set TMPFILE=%TEMP%\git-lastverintask.log

if .%GIT%.==.. goto :help
if .%PREFIX%.==.. goto :help
if .%TASK%.==.. goto :help

rem echo %GIT% %PREFIX% %TMPFILE%

echo. > %TMPFILE%

echo Y | start /B /wait git-merged-ver.sh %GIT% %PREFIX% %TASK% %TMPFILE%

type %TMPFILE% | grep.cmd %PREFIX%

set ERROR=%ERRORLEVEL%
exit %ERROR%
got :eof

rem ---------------------------------------------------------------------
:help
Echo.
Echo Type last merged version branch
Echo Execute git-merged-ver.sh
Echo.
Echo %0 folder prefix branch
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo    prefix - GIT project version prefix: prmd
Echo    branch - GIT branch name: PROMEDWEB-00000
Echo.
Echo Returned code and environment variable ERROR return:
Echo    ERRORLEVEL
Echo    10001 (wrong input parameters)
Echo.
Echo Example:
Echo    %0 D:\Projects\dev_promed_pg prmd PROMEDWEB-00000

set ERROR=10001
exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
