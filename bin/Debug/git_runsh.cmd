@echo off
set STEP=start %0

rem Выполнить скрипт bash в папке проекта GIT
rem %1 папка проекта GIT
rem %2 путь к скрипту bash относительно корня проекта GIT
rem %3, %4, %5, %6 - параметры bash-скрипта
rem в %ERROR% возвращается результат: 
rem = 0 (ok)
rem = 10001 (неверные параметры запуска)
rem = 99999 (неизвестная ошибка)
rem в %BRANCH% возвращается текущая ветка

set GIT=%1
set SCRIPT=%2
set ERROR=0
set BRANCH=

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
echo %0 %1 %2 %3 %4 %5 %6
echo -------------------------------------

if .%SCRIPT%.==.. goto :help

rem ---------------------------------------------------------------------
rem основной код

:start
rem Выполняем merge request
set LOCATION=%cd%
SET LOCATION2=%LOCATION:'=%
SET LOCATION2=%LOCATION2:"=%
set LOCDISK=%LOCATION2:~0,2%

SET GIT2=%GIT:'=%
SET GIT2=%GIT2:"=%
set GITDISK=%GIT2:~0,2%

echo %GITDISK%
%GITDISK%
echo cd %GIT%
cd %GIT%
echo %cd%
echo start /wait %SCRIPT% %3 %4 %5 %6
echo Y | start /wait %SCRIPT% %3 %4 %5 %6
echo %LOCDISK%
%LOCDISK%
echo cd %GIT%
cd %GIT%
echo %cd%
echo.


:finish
rem в переменной %BRANCH% возвращаем текущую ветку
if NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"

rem в переменной ERROR возвращаем результат 0 (Ok)
echo Success
set ERROR=0

exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------



rem ---------------------------------------------------------------------
:help
Echo.
Echo Execute bash script
Echo.
Echo %0 folder script
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo    script - script file name: push-mr.sh
Echo.
Echo Returned code and environment variable ERROR return: 
Echo    0 (ok)
Echo    10001 (wrong input parameters)
Echo    99999 (unknown error)
Echo.
Echo Environment variable BRANCH return current branch
Echo.
Echo Example:
Echo    %0 D:\Projects\dev_promed_pg push-mr.sh

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

rem в переменной BRANCH возвращаем текущую ветку
if NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"

echo.
echo Error %ERROR% on %STEP%
echo Now branch is %BRANCH%

exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
