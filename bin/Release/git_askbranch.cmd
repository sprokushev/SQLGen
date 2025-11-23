rem ---------------------------------------------------------------------
rem процедура запроса номера задачи
:askbranch
set /P TASK=Enter task number (empty for exit):
if .%TASK%.==.. goto :eof

rem переведем в верхний регистр
set "str=%TASK%"
call git_touppercase.cmd str
set TASK=%str%

rem проверим на корректность

echo %TASK% | findstr /b /c:^PROMEDWEB-
IF %ERRORLEVEL% EQU 0 goto :eof

echo %TASK% | findstr /b /c:^RM-
IF %ERRORLEVEL% EQU 0 goto :eof

echo %TASK% | findstr /b /c:^RPMS-
IF %ERRORLEVEL% EQU 0 goto :eof

echo %TASK% | findstr /b /c:^CM-
IF %ERRORLEVEL% EQU 0 goto :eof

echo %TASK% | findstr /b /c:^OPS-
IF %ERRORLEVEL% EQU 0 goto :eof

echo %TASK% | findstr /b /c:^SMP-
IF %ERRORLEVEL% EQU 0 goto :eof

echo %TASK% | findstr /b /c:^BIP-
IF %ERRORLEVEL% EQU 0 goto :eof

echo Wrong task number!
goto :askbranch
goto :eof
rem ---------------------------------------------------------------------

