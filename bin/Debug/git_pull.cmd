@echo off
set STEP=start %0

rem Обновляем текущую ветку
rem %1 папка проекта GIT
rem в %ERROR% возвращается результат: 
rem = 0 (ok)
rem = 10001 (неверные параметры запуска)
rem = 10003 (ошибка команды GIT)
rem = 99999 (неизвестная ошибка)
rem в %BRANCH% возвращается текущая ветка

set GIT=%1
set ERROR=0
set GITCMD=

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
echo -------------------------------------
echo GIT project = %GIT%
echo current dir = %cd%
echo git = %GITEXE%
echo command line parameters:
echo %0 %1 %2 %3 %4
echo -------------------------------------

set DEBUG=NO
set NOEXIT=NO

rem ---------------------------------------------------------------------
rem основной код

:start
rem в переменной BRANCH возвращаем текущую ветку
for /f "delims=" %%a in ('%GITEXE% -C %1 rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"
echo Now branch is %BRANCH%
echo.

rem Обновляем ветку %BRANCH%
set STEP=pull %BRANCH%
set GITCMD=%GITEXE% -C %GIT% pull 
rem --progress -v 
CALL :exec_gitcmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) 
echo Pull %BRANCH% is success
echo.

:finish
rem в переменной ERROR возвращаем результат
exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------



rem ---------------------------------------------------------------------
:help
Echo.
Echo Pull current branch
Echo.
Echo %0 folder
Echo 	folder - GIT project folder: D:\Projects\dev_promed_pg
Echo.
Echo Returned code and environment variable ERROR return: 
Echo    0 (ok)
Echo    10001 (wrong input parameters)
Echo    10003 (error after command "git")
Echo    99999 (unknown error)
Echo.
Echo Environment variable BRANCH return current branch
Echo.
Echo Example:
Echo 	%0 D:\Projects\dev_promed_pg

CALL :error 10001
goto :eof
rem ---------------------------------------------------------------------




rem ---------------------------------------------------------------------
rem процедура завершения с ошибкой %1
:error
rem в переменной ERROR возвращаем ошибку <> 0
SET ERROR=%1
if .%1. EQU .. SET ERROR=99999
if .%1. EQU .0. SET ERROR=99999

echo.
echo Error %ERROR% on %STEP%
echo.

rem в переменной BRANCH возвращаем текущую ветку
if NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"

echo Now branch is %BRANCH%
echo.

exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------


rem ---------------------------------------------------------------------
rem процедура выполнения команды из переменной %GITCMD%
:exec_gitcmd
rem результат возвращается в %ERROR%

rem выполнение команды
set ERROR=0
echo STEP: %STEP%
echo %GITCMD%
%GITCMD% 
set ERROR=%ERRORLEVEL%

rem отображение результата выполнения команды
IF NOT .%ERROR%. EQU .0. echo GIT error code = %ERROR%

rem анализ результата выполнения команды
IF NOT .%ERROR%. EQU .0. goto :eof

rem нет ошибок
set ERROR=0
goto :eof
rem ---------------------------------------------------------------------
