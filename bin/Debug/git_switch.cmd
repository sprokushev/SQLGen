@echo off
set STEP=start %0

rem Переключаемся на ветку и выполняем для нее git pull
rem %1 папка проекта GIT
rem %2 ветка задачи
rem %3, %4, %5 NOUPPERCASE (не обязательный) - НЕ переводить принудительно в верхний регистр имя ветки задачи
rem %3, %4, %5 DEBUG (не обязательный) - режим отладки (пауза после каждой команды git)
rem %3, %4, %5 NOEXIT (не обязательный) - exit с параметром /b, при использовании в других cmd
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
if .%3.==.NOEXIT. set NOEXIT=YES
if .%4.==.NOEXIT. set NOEXIT=YES
if .%5.==.NOEXIT. set NOEXIT=YES

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

IF NOT .%ERROR%. EQU .0. set ERROR=10003
IF NOT .%ERROR%. EQU .0. goto :error
echo fetched all origin/.. from upstream
echo.

:verifylocal
rem Проверяем наличие локальной ветки
set STEP=verify local %TASK%
set GITCMD=%GITEXE% -C %GIT% rev-parse --verify %TASK%
CALL git_run.cmd

rem если %ERROR%==0 - локальная ветка %TASK% существует
IF .%ERROR%. EQU .0. goto :local_exist
goto :local_notexist
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
IF NOT .%ERROR%. EQU .0. goto :up_notset

:up_set
rem в репозитории есть upstream-ветка, возьмем ее имя из %TEMP%\gitcmd_stdout.log
for /f "delims=" %%x in (%TEMP%\gitcmd_stdout.log) do set UPSTREAM=%%x
echo local %TASK% has upstream %UPSTREAM%
echo.

rem переключаемся на ветку %TASK%
set STEP=switch to %TASK%
set GITCMD=%GITEXE% -C %GIT% checkout %TASK%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. set ERROR=10003
IF NOT .%ERROR%. EQU .0. goto :error
echo switched to %TASK%
echo.

rem git pull для ветки %TASK%
set STEP=pull %TASK%
set GITCMD=%GITEXE% -C %GIT% pull 
rem --ff-only 
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. set ERROR=10003
IF NOT .%ERROR%. EQU .0. goto :error
echo pulled %TASK%
echo.

goto :finish

:up_notset
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
IF NOT .%ERROR%. EQU .0. goto :loc_yes_up_no

:loc_yes_up_yes
rem локальная ветка есть, upstream-ветка существует
echo upstream %UPSTREAM% exist
echo.

rem привяжем upstream-ветку %UPSTREAM% к локальной ветке %TASK%
set STEP=set %UPSTREAM% to %TASK%
set GITCMD=%GITEXE% -C %GIT% branch %TASK% --set-upstream-to=%UPSTREAM%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. set ERROR=10003
IF NOT .%ERROR%. EQU .0. goto :error
echo upstream %UPSTREAM% linked to local %TASK%
echo.

rem переключаемся на ветку %TASK%
set STEP=switch to %TASK%
set GITCMD=%GITEXE% -C %GIT% checkout %TASK%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. set ERROR=10003
IF NOT .%ERROR%. EQU .0. goto :error
echo switched to %TASK%
echo.

rem git pull для ветки %TASK%
set STEP=pull %TASK%
set GITCMD=%GITEXE% -C %GIT% pull 
rem --ff-only 
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. set ERROR=10003
IF NOT .%ERROR%. EQU .0. goto :error
echo pulled %TASK%
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

IF NOT .%ERROR%. EQU .0. set ERROR=10003
IF NOT .%ERROR%. EQU .0. goto :error
echo switched to %TASK%
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
IF NOT .%ERROR%. EQU .0. goto :loc_no_up_no

:loc_no_up_yes
rem локальная ветка НЕ существует, upstream-ветка существует
echo upstream %UPSTREAM% exist
echo.

rem создаем локальную ветку %TASK% из %UPSTREAM% 
set STEP=create %TASK% from %UPSTREAM% 
set GITCMD=%GITEXE% -C %GIT% checkout -b %TASK% %UPSTREAM%
CALL git_run.cmd

IF NOT .%ERROR%. EQU .0. set ERROR=10003
IF NOT .%ERROR%. EQU .0. goto :error
echo created local %TASK% from upstream %UPSTREAM%
echo.

goto :finish

:loc_no_up_no
rem локальная ветка НЕ существует, upstream-ветка НЕ существует
echo upstream %UPSTREAM% not exist!
echo.

goto :finish
rem ---------------------------------------------------------------------

:finish
rem в переменной %BRANCH% возвращаем текущую ветку
if NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') do set "BRANCH=%%a"
rem Если текущая ветка отличается - выдаем ошибку
IF NOT .%BRANCH%. EQU .%TASK%. set ERROR=10004
IF NOT .%BRANCH%. EQU .%TASK%. goto :error

rem в переменной %ERROR% возвращаем результат 0 (Ok)
echo Switch to %BRANCH% is success
echo.
set ERROR=0

if .%NOEXIT%. EQU .YES. exit /b %ERROR%
exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------


rem ---------------------------------------------------------------------
:help
Echo.
Echo Switch to branch and pull
Echo.
Echo %0 folder branch [NOUPPERCASE] [DEBUG] [NOEXIT]
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg 
Echo    branch - GIT branch name: PROMEDWEB-00000
Echo    NOUPPERCASE - do not change branch name to upper case
Echo    DEBUG - pause after command "git"
Echo    NOEXIT - exit /b after finish
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

set ERROR=10001
goto :error
rem ---------------------------------------------------------------------

rem ---------------------------------------------------------------------
rem процедура завершения
:error
rem в переменной ERROR возвращаем ошибку <> 0
IF .%ERROR%. EQU .. SET ERROR=99999
IF .%ERROR%. EQU .0. SET ERROR=99999

echo.
echo Error %ERROR% on %STEP%
echo.

rem в переменной BRANCH возвращаем текущую ветку
IF NOT .%GIT%. EQU .. for /f "delims=" %%a in ('%GITEXE% -C %GIT% rev-parse --abbrev-ref HEAD') DO SET "BRANCH=%%a"

echo Now branch is %BRANCH%
IF NOT .%BRANCH%. EQU .%TASK%. echo NOT switched to %TASK%
echo.

IF .%DEBUG%. EQU .YES. pause

IF .%NOEXIT%. EQU .YES. exit /b %ERROR%
exit %ERROR%
goto :eof
rem ---------------------------------------------------------------------
