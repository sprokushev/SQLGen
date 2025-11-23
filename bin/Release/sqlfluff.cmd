rem %1 путь к sql-файлу
rem %2 диалект
rem %3 путь к cfg-файлу

echo. > %TMP%\fix.log
echo sqlfluff fix %1 --dialect %2 -f --config %3 >> %TMP%\fix.log
sqlfluff fix %1 --dialect %2 -f --config %3 >> %TMP%\fix.log