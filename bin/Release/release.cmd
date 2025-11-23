rem SET RELEASE_DIR=D:\OneDrive\Source\SQLGen\bin\Release
SET RELEASE_DIR=.
rem SET BETA_DIR=D:\OneDrive\Source\SQLGen\Version_Beta
SET BETA_DIR=..\..\Version_Beta
rem SET VERSION_DIR=D:\OneDrive\Source\SQLGen\Version
SET VERSION_DIR=..\..\Version
rem SET BACKUP_DIR=D:\OneDrive\Source\SQLGen\Version_Backup
SET BACKUP_DIR=..\..\Version_Backup
rem SET SOURCE_DIR=D:\OneDrive\Source\SQLGen
SET SOURCE_DIR=..\..

set now=%DATE: =0% %TIME: =0%
for /f "tokens=1-7 delims=/-:., " %%a in ( "%now%" ) do (
    set now=%%c%%b%%a_%%d%%e
)
SET LOGFILE=%RELEASE_DIR%\release.log

SET EXCLUDE_FILES=/XF *.bak* /XF thumbs.db /XF ~$*.* /XF release*.cmd /XF *.log /XF *.json /XF *.tmp

SET PARAMS= /S /Z /A /DST /NP /X /FP /NDL /R:2 /W:5 %EXCLUDE_FILES% /LOG+:%LOGFILE% 

mkdir %VERSION_DIR% 
mkdir %BETA_DIR% 
mkdir %BACKUP_DIR%

move /Y %VERSION_DIR%\SQLGen.zip %BACKUP_DIR%\SQLGen_%now%.zip >> %LOGFILE% 
del %VERSION_DIR%\*.* /F /Q /S >> %LOGFILE% 
rem copy /y %SOURCE_DIR%\CHANGELOG %RELEASE_DIR%\changelog.txt >> %LOGFILE% 
rem copy /y %SOURCE_DIR%\README.md %RELEASE_DIR%\readme.txt >> %LOGFILE% 
Robocopy.exe %BETA_DIR% %VERSION_DIR% %PARAMS% 

"C:\Program Files\7-Zip\7z.exe" a -r -y -x!*.zip -x!*.tmp -x!*.bak* -x!*.log -x!*.json -x!~$*.* %VERSION_DIR%\SQLGen.zip %VERSION_DIR%\*.*

