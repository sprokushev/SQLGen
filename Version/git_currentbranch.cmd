@echo off
rem вернуть текущую ветку
rem %1 папка проекта GIT

if .%1.==.. goto :help

rem set GIT_TRACE=D:\TEMP\git_trace.log
rem set GIT_TRACE_PACK_ACCESS=D:\TEMP\git_trace_pack_access.log
rem set GIT_TRACE_PACKET=D:\TEMP\git_trace_packet.log
rem set GIT_TRACE_PERFORMANCE=D:\TEMP\git_trace_perfomance.log
rem set GIT_TRACE_SETUP=D:\TEMP\git_trace_setup.log

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

rem %GITEXE% --no-pager -C %1 branch --show-current
%GITEXE% -C %1 rev-parse --abbrev-ref HEAD
exit %ERRORLEVEL%
goto :eof

rem ---------------------------------------------------------------------
:help
Echo.
Echo Type current branch
Echo.
Echo %0 folder
Echo    folder - GIT project folder: D:\Projects\dev_promed_pg
Echo.
Echo Returned code ERRORLEVEL after command git
Echo.
Echo Example:
Echo    %0 D:\Projects\dev_promed_pg

goto :eof
rem ---------------------------------------------------------------------
