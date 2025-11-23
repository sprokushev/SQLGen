SET DEST_DIR=C:\SQLGen
IF "%1"=="" goto defaultDest
SET DEST_DIR=%1
:defaultDest
mkdir %DEST_DIR%

SET SOURCE_DIR=D:\OneDrive\Source\SQLGen\bin\Release
IF "%2"=="" goto defaultSource
SET SOURCE_DIR=%2
:defaultSource
mkdir %SOURCE_DIR%

SET LOGFILE=%DEST_DIR%\update.log
SET EXCLUDE_FILES=/XF *.bak /XF thumbs.db /XF ~$*.* /XF release.cmd /XF *.log /XF *.json
SET PARAMS= /S /Z /A /DST /NP /X /FP /NDL /R:2 /W:5 %EXCLUDE_FILES% /LOG+:%LOGFILE% 

Robocopy.exe %SOURCE_DIR% %DEST_DIR% %PARAMS% 

chdir %DEST_DIR%
start %DEST_DIR%\SQLGen.exe %3
