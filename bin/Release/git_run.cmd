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
IF NOT .%ERROR%. EQU .0. echo.

IF .%DEBUG%. EQU .YES. pause

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
