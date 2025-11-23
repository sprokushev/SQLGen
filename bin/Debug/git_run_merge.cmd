rem ---------------------------------------------------------------------
rem процедура выполнения команды из переменной %GITCMD%
rem результат возвращается в %ERROR%
rem %TEMP%\gitcmd_stdout.log - stdout выполнения команды git
rem %TEMP%\gitcmd_stderr.log - stderr выполнения команды git

rem выполнение команды
set ERROR=0
echo STEP: %STEP%
echo %GITCMD%
%GITCMD% 1> %TEMP%\gitcmd_stdout.log 2> %TEMP%\gitcmd_stderr.log
set ERROR=%ERRORLEVEL%

rem отображение результата выполнения команды
IF NOT .%ERROR%. EQU .0. echo GIT error code = %ERROR%
IF NOT .%ERROR%. EQU .0. echo.

type %TEMP%\gitcmd_stderr.log
type %TEMP%\gitcmd_stdout.log

findstr /i /c:"BRANCH_DEV" %TEMP%\gitcmd_stderr.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10008
IF .%ERROR%. EQU .10008. goto :eof

findstr /i /c:"BRANCH_DEV" %TEMP%\gitcmd_stdout.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10008
IF .%ERROR%. EQU .10008. goto :eof

findstr /i /c:"Already up to date" %TEMP%\gitcmd_stderr.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=0
IF .%ERROR%. EQU .0. goto :eof

findstr /i /c:"Already up to date" %TEMP%\gitcmd_stdout.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=0
IF .%ERROR%. EQU .0. goto :eof

findstr /i /c:"not something we can merge" %TEMP%\gitcmd_stderr.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=0
IF .%ERROR%. EQU .0. goto :eof

findstr /i /c:"not something we can merge" %TEMP%\gitcmd_stdout.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=0
IF .%ERROR%. EQU .0. goto :eof



findstr /i /c:"Please, commit your changes before you merge" %TEMP%\gitcmd_stderr.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10006
IF .%ERROR%. EQU .10006. goto :eof

findstr /i /c:"Please, commit your changes before you merge" %TEMP%\gitcmd_stdout.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10006
IF .%ERROR%. EQU .10006. goto :eof


findstr /i /c:"nothing added to commit" %TEMP%\gitcmd_stderr.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10007
IF .%ERROR%. EQU .10007. goto :eof

findstr /i /c:"nothing added to commit" %TEMP%\gitcmd_stdout.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10007
IF .%ERROR%. EQU .10007. goto :eof


findstr /i /c:"nothing to commit" %TEMP%\gitcmd_stderr.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10007
IF .%ERROR%. EQU .10007. goto :eof

findstr /i /c:"nothing to commit" %TEMP%\gitcmd_stdout.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10007
IF .%ERROR%. EQU .10007. goto :eof


findstr /i /c:error %TEMP%\gitcmd_stderr.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10003
IF .%ERROR%. EQU .10003. goto :eof

findstr /i /c:error %TEMP%\gitcmd_stdout.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10003
IF .%ERROR%. EQU .10003. goto :eof


findstr /i /c:fatal %TEMP%\gitcmd_stderr.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10003
IF .%ERROR%. EQU .10003. goto :eof

findstr /i /c:fatal %TEMP%\gitcmd_stdout.log
IF .%ERRORLEVEL%. EQU .0. set ERROR=10003
IF .%ERROR%. EQU .10003. goto :eof

rem анализ результата выполнения команды
IF NOT .%ERROR%. EQU .0. goto :eof

rem нет ошибок
set ERROR=0
goto :eof
rem ---------------------------------------------------------------------
