rem SET DEBUG_DIR=D:\OneDrive\Source\SQLGen\bin\Debug
SET DEBUG_DIR=..\Debug
rem SET RELEASE_DIR=D:\OneDrive\Source\SQLGen\bin\Release
SET RELEASE_DIR=.
rem SET VERSION_DIR=D:\OneDrive\Source\SQLGen\Version_Beta
SET VERSION_DIR=..\..\Version_Beta
rem SET SOURCE_DIR=D:\OneDrive\Source\SQLGen
SET SOURCE_DIR=..\..

set now=%DATE: =0% %TIME: =0%
for /f "tokens=1-7 delims=/-:., " %%a in ( "%now%" ) do (
    set now=%%c%%b%%a_%%d%%e
)
SET LOGFILE=%RELEASE_DIR%\release_beta.log

SET EXCLUDE_FILES=/XF *.bak* /XF thumbs.db /XF ~$*.* /XF release*.cmd /XF *.log /XF *.json /XF *.tmp

SET PARAMS= /S /Z /A /DST /NP /X /FP /NDL /R:2 /W:5 %EXCLUDE_FILES% /LOG+:%LOGFILE% 

mkdir %VERSION_DIR% 

del %VERSION_DIR%\*.* /F /Q /S >> %LOGFILE% 
copy /y %SOURCE_DIR%\CHANGELOG %RELEASE_DIR%\changelog.txt >> %LOGFILE% 
copy /y %SOURCE_DIR%\README.md %RELEASE_DIR%\readme.txt >> %LOGFILE% 
copy /y %DEBUG_DIR%\*.cmd %RELEASE_DIR% >> %LOGFILE% 
copy /y %DEBUG_DIR%\*.sh %RELEASE_DIR% >> %LOGFILE% 
copy /y %DEBUG_DIR%\*.cur %RELEASE_DIR% >> %LOGFILE% 
copy /y %DEBUG_DIR%\*.txt %RELEASE_DIR% >> %LOGFILE% 
copy /y %DEBUG_DIR%\*.xshd %RELEASE_DIR% >> %LOGFILE% 
Robocopy.exe %RELEASE_DIR% %VERSION_DIR% %PARAMS% 

"C:\Program Files\7-Zip\7z.exe" a -r -y -x!*.zip -x!*.tmp -x!*.bak* -x!*.log -x!*.json -x!~$*.* %VERSION_DIR%\SQLGen.zip %VERSION_DIR%\*.*
