@echo off
set STEP=start %0

rem Commit в ветку
rem Ветка должны быть текущей. Для смены ветки используем git_newbranch.cmd или git_switch.cmd
rem %1 папка проекта GIT
rem %2, %3 DEBUG (не обязательный) - режим отладки (пауза после каждой команды git)
rem %2, %3 NOMERGEREQUEST (не обязательный) - без Merge Request
rem в %ERROR% возвращается результат: 
rem = 0 (ok)
rem = 10001 (неверные параметры запуска)
rem = 10003 (ошибка команды GIT)
rem = 99999 (неизвестная ошибка)
rem в %BRANCH% возвращается текущая ветка

set GIT=%1
set ERROR=0
set BRANCH=
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
echo %0 %1 %2 %3
echo -------------------------------------

set DEBUG=NO
if .%2.==.DEBUG. set DEBUG=YES
if .%3.==.DEBUG. set DEBUG=YES

set NOMERGEREQUEST=NO
if .%2.==.NOMERGEREQUEST. set NOMERGEREQUEST=YES
if .%3.==.NOMERGEREQUEST. set NOMERGEREQUEST=YES

rem ---------------------------------------------------------------------
rem основной код

:start
rem в переменной BRANCH возвращаем текущую ветку
for /f "delims=" %%a in ('%GITEXE% -C %1 rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"
echo Now branch is %BRANCH%
echo.

rem Обновляем ветку %BRANCH%
set STEP=pull %BRANCH%
set GITCMD=%GITEXE% -C %GIT% pull --progress -v 
CALL :exec_gitcmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo pulled %BRANCH%)
echo.

:commit
rem Вызываем диалог commit
echo start /wait TortoiseGitProc.exe /command:commit /path:%GIT% /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:commit /path:%GIT% /closeonend 2
IF .%DEBUG%. EQU .YES. pause

IF .%NOMERGEREQUEST%. EQU .YES. goto :push
goto :mr

:push
rem Выполняем push
echo start /wait TortoiseGitProc.exe /command:push /path:%GIT% /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:push /path:%GIT% /closeonend 2
IF .%DEBUG%. EQU .YES. pause
goto :finish

:mr
CALL git_runsh.cmd %GIT% push-mr.sh

:finish
rem в переменной %BRANCH% возвращаем текущую ветку
if NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"

rem в переменной ERROR возвращаем результат 0 (Ok)
echo Commit to %BRANCH% is success
echo.
set ERROR=0

exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------



rem ---------------------------------------------------------------------
:help
Echo.
Echo Commit in current branch
Echo.
Echo %0 folder [DEBUG] [NOMERGEREQUEST]
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo    DEBUG - pause after command "git"
Echo    NOMERGEREQUEST - push after "commit". If parameter is missing - will be created "merge request"
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

IF .%DEBUG%. EQU .YES. pause

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
echo.

rem анализ результата выполнения команды
IF NOT .%ERROR%. EQU .0. goto :eof

rem нет ошибок
set ERROR=0
goto :eof
rem ---------------------------------------------------------------------
