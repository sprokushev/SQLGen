@echo off

rem Выполняем pull
echo start /wait TortoiseGitProc.exe /command:pull /path:%1 /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:pull /path:%1 /closeonend 2

rem Вызываем диалог добавления новых файлов
echo start /wait TortoiseGitProc.exe /command:add /path:%1 /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:add /path:%1 /closeonend 2

rem Вызываем диалог commit
echo start /wait TortoiseGitProc.exe /command:commit /path:%1 /logmsg:"#%2" /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:commit /path:%1 /logmsg:"#%2" /closeonend 2

rem Выполняем pull
echo start /wait TortoiseGitProc.exe /command:pull /path:%1 /closeonend 2
echo.
echo Y | start /wait TortoiseGitProc.exe /command:pull /path:%1 /closeonend 2

rem Выполняем push
echo start /wait TortoiseGitProc.exe /command:push /path:%1
echo.
echo Y | start /wait TortoiseGitProc.exe /command:push /path:%1
