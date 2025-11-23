
cd D:\OneDrive\Source\SQLGen\bin\Release\

rmdir /S /Q D:\PROMEDWEB-112501\ac_mlo_ms
CALL SQLGen.exe test UPLOADTOGIT "Microsoft SQL - 172.18.2.14 ( ac_mlo )" msdbupdate_new "D:\PROMEDWEB-112501"
move /Y D:\PROMEDWEB-112501\msdbupdate_new D:\PROMEDWEB-112501\ac_mlo_ms

CALL SQLGen.exe test UPLOADTOGIT "Microsoft SQL - 172.18.2.14 ( log_service )" log_service_ms "D:\PROMEDWEB-112501"
CALL SQLGen.exe test UPLOADTOGIT "Microsoft SQL - 172.18.2.14 ( php_log )" php_log_ms "D:\PROMEDWEB-112501"
CALL SQLGen.exe test UPLOADTOGIT "Microsoft SQL - 172.18.2.14 ( userportalrelease )" userportal_ms "D:\PROMEDWEB-112501"

CALL SQLGen.exe test UPLOADTOGIT "Postgre SQL - 172.18.2.15 ( ac_mlo )" ac_mlo_pg "D:\PROMEDWEB-112501"
CALL SQLGen.exe test UPLOADTOGIT "Postgre SQL - 172.18.2.15 ( EMDrelease )" emd "D:\PROMEDWEB-112501"
CALL SQLGen.exe test UPLOADTOGIT "Postgre SQL - 172.18.2.15 ( lisrelease )" promedlistest2 "D:\PROMEDWEB-112501"
CALL SQLGen.exe test UPLOADTOGIT "Postgre SQL - 172.18.2.15 ( log_service )" log_service_pg "D:\PROMEDWEB-112501"
CALL SQLGen.exe test UPLOADTOGIT "Postgre SQL - 172.18.2.15 ( php_log )" php_log_pg "D:\PROMEDWEB-112501"
CALL SQLGen.exe test UPLOADTOGIT "Postgre SQL - 172.18.2.15 ( userportalrelease )" userportal_pg "D:\PROMEDWEB-112501"
CALL SQLGen.exe test UPLOADTOGIT "Postgre SQL - 172.29.4.2 ( fer_log )" fer_log "D:\PROMEDWEB-112501"

CALL SQLGen.exe test UPLOADTOGIT "Microsoft SQL - 172.18.2.14 ( promedwebrelease )" msdbupdate_new "D:\PROMEDWEB-112501"
CALL SQLGen.exe test UPLOADTOGIT "Postgre SQL - 172.18.2.15 ( promedrelease )" liquibase_project_new "D:\PROMEDWEB-112501"

exit
