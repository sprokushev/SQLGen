@echo off
set STEP=start %0

rem Скачиваем все ветки origin
rem %1 папка проекта GIT
rem %2, %3 DEBUG (не обязательный) - режим отладки (пауза после каждой команды git)
rem %2, %3 NOEXIT (не обязательный) - exit с параметром /b, при использовании в других cmd
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
echo %0 %1 %2 %3 %4 %5
echo -------------------------------------

set DEBUG=NO
if .%2.==.DEBUG. set DEBUG=YES
if .%3.==.DEBUG. set DEBUG=YES

set NOEXIT=NO
if .%2.==.NOEXIT. set NOEXIT=YES
if .%3.==.NOEXIT. set NOEXIT=YES

rem ---------------------------------------------------------------------
rem основной код

:start
rem Обновляем локальные ветки origin/.. из upstream
set STEP=fetch all
set GITCMD=%GITEXE% -C %GIT% fetch --all
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (set ERROR=10003)
IF NOT .%ERROR%. EQU .0. (goto :error) ELSE (echo fetched all origin/.. from upstream)
echo.

:finish
rem в переменной %BRANCH% возвращаем текущую ветку
if NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"
echo Now branch is %BRANCH%
echo.

rem в переменной %ERROR% возвращаем результат 0 (Ok)
echo Fetch all is success
echo.
set ERROR=0

if .%NOEXIT%. EQU .YES. (exit /b %ERROR%) ELSE (exit %ERROR%)
goto :eof
rem ---------------------------------------------------------------------


rem ---------------------------------------------------------------------
:help
Echo.
Echo Fetch all origin branches
Echo.
Echo %0 folder [DEBUG] [NOEXIT]
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg 
Echo    DEBUG - pause after command "git"
Echo    NOEXIT - exit /b after finish
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

set ERROR=10001
goto :error
rem ---------------------------------------------------------------------




rem ---------------------------------------------------------------------
rem процедура завершения
:error
rem в переменной ERROR возвращаем ошибку <> 0
if .%ERROR%. EQU .. SET ERROR=99999
if .%ERROR%. EQU .0. SET ERROR=99999

echo.
echo Error %ERROR% on %STEP%
echo.

rem в переменной BRANCH возвращаем текущую ветку
if NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"
echo Now branch is %BRANCH%
echo.

IF .%DEBUG%. EQU .YES. pause

if .%NOEXIT%. EQU .YES. (exit /b %ERROR%) ELSE (exit %ERROR%)
goto :eof
rem ---------------------------------------------------------------------



