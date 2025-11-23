@echo off

rem переключиться на папку с командным файлом, выполнить и вернуться в прежний каталог
rem %1 папка с коммандным файлом 
rem %2 командный файл
rem %3-%6 параметры для командного файла

if .%1.==.. goto :help
if .%2.==.. goto :help

set CURRENTPATH=%cd%
SET CURRENTPATH2=%CURRENTPATH:'=%
SET CURRENTPATH2=%CURRENTPATH2:"=%
set CURRENTDISK=%CURRENTPATH2:~0,2%

set EXECPATH=%1
SET EXECPATH2=%EXECPATH:'=%
SET EXECPATH2=%EXECPATH2:"=%
set EXECDISK=%EXECPATH2:~0,2%

%EXECDISK%
cd %EXECPATH%
CALL %2 %3 %4 %5 %6
%CURRENTDISK%
cd %CURRENTPATH%

goto :eof

rem ---------------------------------------------------------------------
:help
Echo.
Echo Switch to the folder with the cmd-file, execute cmd-file and return to the previous folder
Echo.
Echo %0 folder file [param1] [param2] [param3] [param4]
Echo    folder - folder with the cmd-file: D:\SQLGen
Echo    file - cmd-file to execute: git_currentbranch.cmd
Echo    param1, param2, param3, param4 - cmd-file parameters (maximum 4)
Echo.
Echo Example:
Echo    %0 D:\SQLGen git_currentbranch.cmd D:\Projects\dev_promed_pg

goto :eof
rem ---------------------------------------------------------------------
