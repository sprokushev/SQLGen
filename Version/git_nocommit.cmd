@echo off
rem проверить, есть ли отличия от индекса, требуется ли commit?
rem %1 папка проекта GIT, например E:\Projects\dev_promed_pg
rem в %ERROR% возвращается результат: 
rem = 0 (ok)
rem = 10001 (неверные параметры запуска)
rem = 10006 (требуется COMMIT)
rem также выводит в stdout:
rem OK
rem NEEDCOMMIT

set GIT=%1
set TMPFILE=%TEMP%\git_nocommit.log

if .%GIT%.==.. goto :help

rem echo %GIT% %TMPFILE%

echo. > %TMPFILE%

echo Y | start /B /wait git_nocommit.sh %GIT% %TMPFILE%

type %TMPFILE%

set ERROR=%ERRORLEVEL%
exit %ERROR%
got :eof

rem ---------------------------------------------------------------------
:help
Echo.
Echo Type OK (if commit is not required) or NEEDCOMMIT (if need to commit)
Echo Execute git_nocommit.sh
Echo.
Echo %0 folder
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo.
Echo Returned code and environment variable ERROR return:
Echo    0 (commit is not required)
Echo    10001 (wrong input parameters)
Echo    10006 (need command "git commit")
Echo.
Echo Example:
Echo    %0 D:\Projects\dev_promed_pg

set ERROR=10001
exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
