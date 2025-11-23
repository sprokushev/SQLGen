@echo off
set STEP=start %0

rem Merge другой ветки в текущую
rem Для смены текущей ветки используем git_switch.cmd 
rem %1 папка проекта GIT
rem %2 ветка задачи, которую надо merge в текущую ветку
rem %3, %4, %5 NOUPPERCASE (не обязательный) - НЕ переводить принудительно в верхний регистр имя ветки задачи
rem %3, %4, %5 ORIGIN (не обязательный) - вливать из origin
rem в %ERROR% возвращается результат: 
rem = 0 (ok)
rem = 10001 (неверные параметры запуска)
rem = 10003 (ошибка команды GIT)
rem = 10005 (ошибка при разрешении конфликта MERGE)
rem = 10006 (требуется COMMIT)
rem = 10007 (НЕ требуется COMMIT)
rem = 10008 (ветка содержит BRANCH_DEV)
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

set ORIGIN=
if .%3.==.ORIGIN. set ORIGIN=origin/
if .%4.==.ORIGIN. set ORIGIN=origin/
if .%5.==.ORIGIN. set ORIGIN=origin/

set DEBUG=NO
set NOEXIT=NO

IF NOT .%UPPERCASE%. EQU .YES. goto :start

rem переведем имя ветки задачи в верхний регистр
set "str=%TASK%"
call git_touppercase.cmd str
set TASK=%str%

rem ---------------------------------------------------------------------
rem основной код

:start
rem в переменной %BRANCH% возвращаем текущую ветку
if NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"
echo Now branch is %BRANCH%
echo.

:check
rem проверяем наличие ветки dev в ветке задачи
set STEP=check dev in %ORIGIN%%TASK%
set GITCMD=%GITEXE% -C %GIT% ls-tree --name-only --full-tree %ORIGIN%%TASK% -- BRANCH_DEV
CALL git_run_merge.cmd

IF .%ERROR%. EQU .10008. (CALL :error 10008)
echo %ORIGIN%%TASK% not contains dev - Ok
echo.

:merge
rem вызываем merge
set STEP=first merge %ORIGIN%%TASK%
set GITCMD=%GITEXE% -C %GIT% merge --no-ff %ORIGIN%%TASK% 
CALL git_run_merge.cmd

IF .%ERROR%. EQU .10006. (CALL :commitaftermerge)
IF NOT .%ERROR%. EQU .0. (goto :resolve) ELSE (goto :commit)
echo.

:resolve
rem вызываем диалог разрешения конфликта
rem set STEP=mergetool
rem set GITCMD=%GITEXE% -C %GIT% mergetool -y
rem @echo on
rem CALL git_run_merge.cmd

rem IF NOT .%ERROR%. EQU .0. (CALL :error 10005) ELSE (goto :commit)
rem @echo off
rem echo.
rem Вызываем диалог resolve
echo start /wait TortoiseGitProc.exe /command:resolve /path:%GIT% /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:resolve /path:%GIT% /closeonend 2

rem вызываем merge повторно для получения exit code
set STEP=merge %ORIGIN%%TASK% after resolve
set GITCMD=%GITEXE% -C %GIT% merge --no-ff %ORIGIN%%TASK%
CALL git_run_merge.cmd

IF .%ERROR%. EQU .10006. (CALL :commitaftermerge)
IF NOT .%ERROR%. EQU .0. (CALL :error 10005) ELSE (goto :commit)
echo.

:commit
rem проверяем необходмость commit
set STEP=verify commit
set GITCMD=%GITEXE% -C %GIT% commit --verify
CALL git_run_merge.cmd

IF .%ERROR%. EQU .10007. (goto :finish)
echo.

rem Вызываем диалог commit
echo start /wait TortoiseGitProc.exe /command:commit /path:%GIT% /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:commit /path:%GIT% /closeonend 2
goto :eof

:finish
rem в переменной %BRANCH% возвращаем текущую ветку
if NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"
echo Now branch is %BRANCH%
echo.

rem в переменной ERROR возвращаем результат 0 (Ok)
echo Merge %ORIGIN%%TASK% to %BRANCH% is success
echo.
set ERROR=0

exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------


rem ---------------------------------------------------------------------
:commitaftermerge
rem Вызываем диалог commit
echo start /wait TortoiseGitProc.exe /command:commit /path:%GIT% /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:commit /path:%GIT% /closeonend 2

rem вызываем merge повторно после commit
set STEP=merge %ORIGIN%%TASK% after commit
set GITCMD=%GITEXE% -C %GIT% merge %ORIGIN%%TASK% 
CALL git_run_merge.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10005)
echo.

goto :eof
rem ---------------------------------------------------------------------




rem ---------------------------------------------------------------------
:help
Echo.
Echo Merge branch into current
Echo.
Echo %0 folder branch [NOUPPERCASE] [ORIGIN]
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo    branch - GIT branch name: PROMEDWEB-00000
Echo    NOUPPERCASE - do not change branch name to upper case
Echo    ORIGIN - merge from origin/branch
Echo.
Echo Returned code and environment variable ERROR return: 
Echo    0 (ok)
Echo    10001 (wrong input parameters)
Echo    10003 (error after command "git")
Echo    10005 (error after resolve conflict "git merge")
Echo    10006 (need command "git commit")
Echo    10007 (not need command "git commit")
Echo    10008 (file BRANCH_DEV exists in branch)
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
IF .%ERROR%. EQU .10008. (echo %ORIGIN%%TASK% cant merged into %BRANCH% because contains dev)
echo.

exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
