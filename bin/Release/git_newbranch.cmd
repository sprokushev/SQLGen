@echo off
set STEP=start %0

rem Создаем новую ветку
rem Если ветка уже есть - обновляем ее с удаленного репозитория и делаем в нее merge из master
rem %1 папка проекта GIT
rem %2 ветка задачи
rem %3 ветка от которой нужно создать новую ветку (не обязательный, по умолчанию master)
rem %3, %4 или %5 NOUPPERCASE (не обязательный) - НЕ переводить принудительно в верхний регистр имя ветки задачи
rem %3, %4 или %5 DEBUG (не обязательный) - режим отладки (пауза после каждой команды git)
rem в %ERROR% возвращается результат: 
rem = 0 (ok)
rem = 10001 (неверные параметры запуска)
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

set MASTER=%3
if .%3.==.NOUPPERCASE. set MASTER=master
if .%3.==.DEBUG. set MASTER=master
if .%3.==.. set MASTER=master

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

set NOEXIT=NO

IF NOT .%UPPERCASE%. EQU .YES. goto :start

rem переведем имя ветки задачи в верхний регистр
set "str=%TASK%"
call git_touppercase.cmd str
set TASK=%str%

rem ---------------------------------------------------------------------
rem основной код

:start
rem Обновляем локальные ветки origin/.. из upstream
set STEP=fetch all
set GITCMD=%GITEXE% -C %GIT% fetch --all
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo fetched all origin/.. from upstream)
echo.

rem Проверяем наличие ветки %MASTER%
set STEP=verify %MASTER%
set GITCMD=%GITEXE% -C %GIT% rev-parse --verify %MASTER%
CALL git_run.cmd

rem если %ERROR% <> 0 - локальная ветка %MASTER% НЕ существует, попробуем ее создать
IF NOT .%ERROR%. EQU .0. goto :create_master

goto :switch_master
rem ---------------------------------------------------------------------

:create_master
rem создаем локальную ветку %MASTER%
set STEP=create local %MASTER%
set GITCMD=%GITEXE% -C %GIT% pull %MASTER% --ff-only
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo created local %MASTER%)
echo.
goto :switch_master
rem ---------------------------------------------------------------------

:switch_master
rem переключаемся на %MASTER%
set STEP=switch to %MASTER%
set GITCMD=%GITEXE% -C %GIT% checkout %MASTER%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo switched to %MASTER%)
echo.

rem merge from origin/%MASTER%
set STEP=merge from origin/%MASTER%
set GITCMD=%GITEXE% -C %GIT% merge origin/%MASTER% --ff-only
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo merged origin/%MASTER% to %MASTER%)
echo.


:verifylocal
rem Проверяем наличие локальной ветки
set STEP=verify local %TASK%
set GITCMD=%GITEXE% -C %GIT% rev-parse --verify %TASK%
CALL git_run.cmd

rem если %ERROR%==0 - локальная ветка %TASK% существует
IF .%ERROR%. EQU .0. (goto :local_exist) ELSE (goto :local_notexist)

goto :finish
rem ---------------------------------------------------------------------





rem ---------------------------------------------------------------------
:local_exist
rem локальная ветка %TASK% существует
echo local %TASK% exist
echo.

rem Проверяем, есть ли у локальной ветки %TASK% upstream-ветка и ее имя
set STEP=get upstream %TASK%
set GITCMD=%GITEXE% -C %GIT% rev-parse --abbrev-ref --symbolic-full-name %TASK%@{u} 
CALL git_run.cmd

rem если %ERROR%==0 - в файле %TEMP%\gitcmd_stdout.log имя upstream-ветки
IF .%ERROR%. EQU .0. (goto :upstream_set) ELSE (goto :upstream_notset)
goto :finish

:upstream_set
rem в репозитории есть upstream-ветка, возьмем ее имя из %TEMP%\gitcmd_stdout.log
for /f "delims=" %%x in (%TEMP%\gitcmd_stdout.log) do set UPSTREAM=%%x
echo local %TASK% has upstream %UPSTREAM%
echo.

rem переключаемся на ветку %TASK%
set STEP=switch to %TASK%
set GITCMD=%GITEXE% -C %GIT% checkout %TASK%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo switched to %TASK%)
echo.

rem git pull для ветки %TASK%
set STEP=pull %TASK%
set GITCMD=%GITEXE% -C %GIT% pull --ff-only 
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo pulled %TASK%)
echo.

goto :finish


:upstream_notset
rem в репозитории нет информации об upstream-ветке
echo local %TASK% not set upstream!
echo.

rem соберем предполагаемое имя upstream-ветки
set UPSTREAM=origin/%TASK%

rem Проверяем, существует ли upstream-ветка %UPSTREAM%
set STEP=verify upstream %UPSTREAM%
set GITCMD=%GITEXE% -C %GIT% rev-parse --verify %UPSTREAM% 
CALL git_run.cmd

rem если %ERROR%==0 - upstream-ветка %UPSTREAM% существует
IF .%ERROR%. EQU .0. (goto :loc_yes_up_yes) ELSE (goto :loc_yes_up_no)
goto :finish


:loc_yes_up_yes
rem локальная ветка есть, upstream-ветка существует
echo upstream %UPSTREAM% exist
echo.

rem привяжем upstream-ветку %UPSTREAM% к локальной ветке %TASK%
set STEP=set %UPSTREAM% to %TASK%
set GITCMD=%GITEXE% -C %GIT% branch %TASK% --set-upstream-to=%UPSTREAM%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo upstream %UPSTREAM% linked to local %TASK%)
echo.

rem переключаемся на ветку %TASK%
set STEP=switch to %TASK%
set GITCMD=%GITEXE% -C %GIT% checkout %TASK%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo switched to %TASK%)
echo.

rem git pull для ветки %TASK%
set STEP=pull %TASK%
set GITCMD=%GITEXE% -C %GIT% pull --ff-only 
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo pulled %TASK%)
echo.

goto :finish



:loc_yes_up_no
rem локальная ветка есть, upstream-ветка НЕ существует
echo upstream %UPSTREAM% not exist!
echo.

rem переключаемся на ветку %TASK%
set STEP=switch to %TASK%
set GITCMD=%GITEXE% -C %GIT% checkout %TASK%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo switched to %TASK%)
echo.
goto :finish
rem ---------------------------------------------------------------------


rem ---------------------------------------------------------------------
:local_notexist
rem локальная ветка %TASK% НЕ существует
echo local %TASK% not exist!
echo.

rem соберем предполагаемое имя upstream-ветки
set UPSTREAM=origin/%TASK%

rem Проверяем, существует ли upstream-ветка %UPSTREAM%
set STEP=verify upstream %UPSTREAM%
set GITCMD=%GITEXE% -C %GIT% rev-parse --verify %UPSTREAM% 
CALL git_run.cmd

rem если %ERROR%==0 - upstream-ветка %UPSTREAM% существует
IF .%ERROR%. EQU .0. (goto :loc_no_up_yes) ELSE (goto :loc_no_up_no)
goto :finish


:loc_no_up_yes
rem локальная ветка НЕ существует, upstream-ветка существует
echo upstream %UPSTREAM% exist
echo.

rem создаем локальную ветку %TASK% из %UPSTREAM% 
set STEP=create %TASK% from %UPSTREAM% 
set GITCMD=%GITEXE% -C %GIT% checkout -b %TASK% %UPSTREAM%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo created local %TASK% from upstream %UPSTREAM%)
echo.

goto :finish



:loc_no_up_no
rem локальная ветка НЕ существует, upstream-ветка НЕ существует
echo upstream %UPSTREAM% not exist!
echo.

rem создаем локальную ветку %TASK% из %MASTER%
set STEP=create %TASK% from %MASTER%
set GITCMD=%GITEXE% -C %GIT% checkout -b %TASK%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (CALL :error 10003) ELSE (echo created %TASK% from %MASTER%)
echo.

goto :finish
rem ---------------------------------------------------------------------


:finish
rem в переменной %BRANCH% возвращаем текущую ветку
echo timeout /t 3 /nobreak
timeout /t 3 /nobreak >nul
if NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"

rem Если текущая ветка отличается - выдаем ошибку
IF NOT .%BRANCH%. EQU .%TASK%. CALL :error 10004

rem Выполняем merge из %MASTER% в %TASK%
if .%BRANCH%. EQU .%TASK%. goto :merge_from_master

goto :finish2
rem ---------------------------------------------------------------------

rem ---------------------------------------------------------------------
:merge_from_master
rem Процедура merge из %MASTER% в %TASK%
set STEP=merge %MASTER% to %TASK%
set GITCMD=%GITEXE% -C %GIT% merge %MASTER% --ff-only
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. (echo Warning!!! %MASTER% not merged to %TASK%) ELSE (echo %MASTER% merged to %TASK%)
echo.
goto :finish2
rem ---------------------------------------------------------------------

:finish2
rem в переменной %ERROR% возвращаем результат 0 (Ok)
echo Create or switch to %BRANCH% is success
echo.
set ERROR=0

exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------



rem ---------------------------------------------------------------------
:help
Echo.
Echo Create new branch (or switch to existing branch) and pull
Echo.
Echo %0 folder branch [parent] [NOUPPERCASE] [DEBUG]
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg 
Echo    branch - GIT branch name: PROMEDWEB-00000
Echo    parent - GIT branch name: master (default)
Echo    NOUPPERCASE - do not change branch name to upper case
Echo    DEBUG - pause after command "git"
Echo.
Echo Returned code and environment variable ERROR return: 
Echo    0 (ok)
Echo    10001 (wrong input parameters)
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
if NOT .%BRANCH%. EQU .%TASK%. echo NOT switched to %TASK%
echo.

IF .%DEBUG%. EQU .YES. pause

exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
