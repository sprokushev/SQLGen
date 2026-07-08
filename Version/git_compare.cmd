@echo off
rem сравнить 2 файла
rem %1 1-й файл (который сейчас в GIT)
rem %2 2-й файл (новый)

if .%1.==.. goto :help
if .%2.==.. goto :help

rem echo .\CompareIt\wincmp3.exe %1 %2 /R
rem .\CompareIt\wincmp3.exe %1 %2 /R

echo start /wait TortoiseGitProc.exe /command:push /path:%GIT% /closeonend 2
start /wait TortoiseGitProc.exe /command:diff /path:%2 /path2:%1

goto :eof

rem ---------------------------------------------------------------------
:help
Echo.
Echo Compare changed file with original file in GIT project
Echo.
Echo %0 file1 file2
Echo    file1 - original file in GIT project: D:\Projects\dev_promed_pg\dbo\table\yesno.sql
Echo    file2 - changed file: D:\TMP\yesno.sql
Echo.
Echo Example:
Echo    %0 D:\Projects\dev_promed_pg\dbo\table\yesno.sql D:\TMP\yesno.sql

goto :eof
rem ---------------------------------------------------------------------
