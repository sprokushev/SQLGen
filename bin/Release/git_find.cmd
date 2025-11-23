@echo off
rem вернуть список файлов, в имени которых встречается строка
rem %1 папка проекта GIT, например E:\Projects\dev_promed_pg
rem %2 путь внутри проекта GIT относительно корня (слеш как в линукс), например . или ./dbo
rem %3 искомая строка
rem Возвращает ERRORLEVEL или 10001 (неверные параметры запуска)

set GIT=%1
set GITPATH=%2
set SEARCH=%3
set TMPFILE=%TEMP%\git_find.log

if .%GIT%.==.. goto :help
if .%GITPATH%.==.. goto :help
if .%SEARCH%.==.. goto :help

rem echo %GIT% %GITPATH% %SEARCH% %TMPFILE%

echo. > %TMPFILE%

echo Y | start /B /wait git_find.sh %GIT% %GITPATH% %SEARCH% %TMPFILE%

type %TMPFILE%

set ERROR=%ERRORLEVEL%
exit %ERROR%
goto :eof

rem ---------------------------------------------------------------------
:help
Echo.
Echo Return a list of files whose names contain the string
Echo Execute git_find.sh
Echo.
Echo %0 folder path string
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo    path - relative path in GIT project folder by linux-style: ./dbo
Echo    string - searched string
Echo.
Echo Returned code and environment variable ERROR return:
Echo    ERRORLEVEL
Echo    10001 (wrong input parameters)
Echo.
Echo Example:
Echo    %0 D:\Projects\dev_promed_pg ./dbo yesno

set ERROR=10001
exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
