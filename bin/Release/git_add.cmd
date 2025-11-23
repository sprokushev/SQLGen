@echo off
set STEP=start %0

rem Добавить файлы в ветку задачи
rem Ветка задачи должны быть текущей. Для смены ветки используем git_newbranch.cmd или git_switch.cmd
rem %1 папка проекта GIT
rem %2 ветка задачи
rem %3, %4, %5 NOUPPERCASE (не обязательный) - НЕ переводить принудительно в верхний регистр имя ветки задачи
rem %3, %4, %5 DEBUG (не обязательный) - режим отладки (пауза после каждой команды git)
rem %3, %4, %5 NOMERGEREQUEST (не обязательный) - без Merge Request
rem в %ERROR% возвращается результат: 
rem = 0 (ok)
rem = 10001 (неверные параметры запуска)
rem = 10002 (текущая ветка не соответствует ветке задачи)
rem = 10003 (ошибка команды GIT)
rem = 10004 (ветка GIT не существует)
rem = 99999 (неизвестная ошибка)
rem в %BRANCH% возвращается текущая ветка

set GIT=%1
set TASK=%2
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

rem if .%TASK%.==.. CALL git_askbranch.cmd
if .%TASK%.==.. goto :help

set UPPERCASE=YES
if .%3.==.NOUPPERCASE. set UPPERCASE=NO
if .%4.==.NOUPPERCASE. set UPPERCASE=NO
if .%5.==.NOUPPERCASE. set UPPERCASE=NO
if .%TASK%.==.master. set UPPERCASE=NO
if .%TASK%.==.test. set UPPERCASE=NO
if .%TASK%.==.release. set UPPERCASE=NO
if .%TASK%.==.dev. set UPPERCASE=NO
if .%TASK%.==.utility. set UPPERCASE=NO
if .%TASK%.==.generator. set UPPERCASE=NO

set DEBUG=NO
if .%3.==.DEBUG. set DEBUG=YES
if .%4.==.DEBUG. set DEBUG=YES
if .%5.==.DEBUG. set DEBUG=YES

set NOMERGEREQUEST=NO
if .%3.==.NOMERGEREQUEST. set NOMERGEREQUEST=YES
if .%4.==.NOMERGEREQUEST. set NOMERGEREQUEST=YES
if .%5.==.NOMERGEREQUEST. set NOMERGEREQUEST=YES

set NOEXIT=NO

IF NOT .%UPPERCASE%. EQU .YES. goto :start

rem переведем имя ветки задачи в верхний регистр
set "str=%TASK%"
call git_touppercase.cmd str
set TASK=%str%

rem ---------------------------------------------------------------------
rem основной код

:start
rem Проверяем, что текущая ветка соответствует ветке задачи
rem в переменной BRANCH возвращаем текущую ветку
for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"
rem Если текущая ветка отличается - выдаем ошибку
IF NOT .%BRANCH%. EQU .%TASK%. CALL :error 10002

rem Если текущая ветка - ветка задачи, делаем git pull и переходим к добавлению файлов в индекс

:pull
rem Обновляем из удаленного репозитария
set STEP=switch to %TASK%
IF .%UPPERCASE%. EQU .NO. (CALL git_switch.cmd %GIT% %TASK% NOEXIT NOUPPERCASE) ELSE (CALL git_switch.cmd %GIT% %TASK% NOEXIT)
rem Если при переключении вышла ошибка - завершаем
rem echo %ERROR%
IF NOT .%ERROR%. EQU .0. goto :eof

:add
rem Вызываем диалог добавления новых файлов
echo start /wait TortoiseGitProc.exe /command:add /path:%GIT% /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:add /path:%GIT% /closeonend 2
IF .%DEBUG%. EQU .YES. pause

:commit
rem Вызываем диалог commit
echo start /wait TortoiseGitProc.exe /command:commit /path:%GIT% /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:commit /path:%GIT% /closeonend 2
IF .%DEBUG%. EQU .YES. pause

IF .%NOMERGEREQUEST%. EQU .YES. goto :push
IF .%TASK%. EQU .test. goto :push
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
echo Add changes to %BRANCH% is success
echo.
set ERROR=0

exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------


rem ---------------------------------------------------------------------
:help
Echo.
Echo Add files in index, commit, create merge request in current branch
Echo.
Echo %0 folder branch [NOUPPERCASE] [DEBUG] [NOMERGEREQUEST]
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo    branch - GIT branch name: PROMEDWEB-00000
Echo    NOUPPERCASE - do not change branch name to upper case
Echo    DEBUG - pause after command "git"
Echo    NOMERGEREQUEST - push after "commit". If parameter is missing - will be created "merge request"
Echo.
Echo Returned code and environment variable ERROR return: 
Echo    0 (ok)
Echo    10001 (wrong input parameters)
Echo    10002 (current branch does not match the parameter [branch])
Echo    10003 (error after command "git")
Echo    10004 (branch not exist)
Echo    99999 (unknown error)
Echo.
Echo Environment variable BRANCH return current branch
Echo.
Echo Example:
Echo    %0 D:\Projects\dev_promed_pg PROMEDWEB-00000

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
if NOT .%BRANCH%. EQU .%TASK%. echo Please switch to %TASK%
echo.

IF .%DEBUG%. EQU .YES. pause

exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
