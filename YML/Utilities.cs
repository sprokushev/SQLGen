// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace SQLGen.Utilities
{
    /// <summary>
    /// Вспомогательные функции для работы с YML-файлами
    /// </summary>
    public static class YML
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список папок в проекте GIT и соответствующих им меток</summary>
        public static Dictionary<string, string> List_GITType_Label = new Dictionary<string, string>
        {
            { "SCHEMA", "struct" },
            { "TABLE", "struct" },
            { "ALTER", "struct" },
            { "CREATE", "struct" },
            { "DROP", "struct" },
            { "INDEX", "struct" },
            { "IDX", "struct" },
            { "SEQUENCE", "struct" },
            { "SEQ", "struct" },
            { "TYPE", "struct" },
            { "INSERT", "data" },
            { "UPDATE", "data" },
            { "DELETE", "data" },
            { "UPSERT", "data" },
            { "MERGE", "data" },
            { "DATA", "data" },
            { "DATA_NEW", "data" },
            { "BULK", "data" },
            { "COPY", "data" },
            { "FREEDOCRELATIONSHIP", "data" },
            { "FREEDOCMARKER", "data" },
            { "VIEW", "code" },
            { "PROCEDURE", "code" },
            { "FUNCTION", "code" },
            { "PROC", "code" },
            { "FUNC", "code" },
            { "TRIGGER", "code" },
            { "UNKNOWN", "finish" },
            { "", "finish" }
        };

        /// <summary>
        ///  скопировать YML из GIT
        /// </summary>
        /// <param name="Projects">список проектов</param>
        /// <param name="YMLFile">копируемый yml-Файл</param>
        /// <param name="isFound">yml-Файл найден</param>
        /// <param name="mode">режим копирования</param>
        /// <param name="isCorrectEncoding">=true - исправлять кодировку</param>
        /// <param name="isCheckBOM">=true - проверять наличие BOM-флага в кодировке UTF8</param>
        /// <param name="errors">ошибки при копировании</param>
        /// <param name="ListVersions">список файлов версий</param>
        /// <param name="isPrevVersion">=true - копировать вложенные версии</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <returns>=true выполнено без ошибок</returns>
        public static bool GetYmlFromGIT(List<string> Projects, ref string YMLFile, out bool isFound, CopyType mode, bool isCorrectEncoding, bool isCheckBOM, out string errors, ref List<YMLText> ListVersions, bool isPrevVersion, string logFile)
        {
            isFound = false;
            errors = "";
            bool isError = false;

            foreach (string project in Projects)
            {
                string file = "";
                string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
                string root_from = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                string ymlpath = "task";

                file = Path.Combine(root_from, ymlpath, YMLFile);

                if (!File.Exists(file))
                {
                    ymlpath = "Report";
                    file = Path.Combine(root_from, ymlpath, YMLFile);
                }

                if (!File.Exists(file))
                {
                    ymlpath = "version";
                    file = Path.Combine(root_from, ymlpath, YMLFile);
                }

                if (File.Exists(file))
                {
                    isFound = true;

                    // надо определить реальное имя копируемого YML-файла
                    YMLFile = Path.GetFileName(
                        Utilities.Files.GetRealFilename(file)
                        );
                    //tbYMLFile.Text = YMLFile;

                    // загружаем yml-файл
                    YMLStruct loadyml = new YMLStruct(null, logFile);
                    loadyml.LoadYML(project, ymlpath, YMLFile, isPrevVersion, null, true, true);

                    // копируем
                    string root_to = Path.Combine(MainWindow.Task.TaskPath, "FROM_GIT", ProjectFolder);
                    try
                    {
                        Directory.CreateDirectory(root_to);

                        string FullSQLFile = Path.Combine(root_to, Path.GetFileNameWithoutExtension(YMLFile) + ".sql");
                        Encoding encoding = new UTF8Encoding(false);
                        File.WriteAllText(FullSQLFile, string.Empty, encoding);

                        // если копируем вложенные версии - отключаем копирование в один файл
                        if (isPrevVersion) FullSQLFile = "";

                        var logfile = Path.Combine(root_to, project + ".txt");
                        if (Directory.Exists(root_to)) File.WriteAllText(logfile, string.Empty, encoding);

                        var listfile = Path.Combine(root_to, project + "_list.txt");
                        if (Directory.Exists(root_to)) File.WriteAllText(listfile, string.Empty, encoding);

                        errors += Environment.NewLine + "".PadRight(100, '-');
                        errors += Environment.NewLine + project;
                        errors += Environment.NewLine + "".PadRight(100, '-');

                        if (mode == CopyType.COPY || mode == CopyType.CHECKCOPY)
                        {
                            loadyml.CopyYML(isPrevVersion, root_to, FullSQLFile, listfile);
                        }

                        if (mode == CopyType.CHECK || mode == CopyType.CHECKCOPY)
                        {
                            loadyml.CheckYML(isPrevVersion, isCorrectEncoding == true, isCheckBOM == true, null, ref errors, ref isError, false, true, ref ListVersions, false);
                        }

                        App.AddLog($"Файл {YMLFile} скопирован в {root_to}", null, App.ShowMessageMode.NONE, true, logFile); 
                    }
                    catch (Exception ex)
                    {
                        errors += Environment.NewLine + App.AddLog($"Ошибка при копировании файла {YMLFile} из папки {Path.Combine(root_from, ymlpath)} в папку {root_to} :", ex, App.ShowMessageMode.SHOW, true, logFile).showMessage;
                        return false;
                    }
                }
            }

            return !isError;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Проверка куска текста, без привязки к файлу, напрмер changeset</summary>
        /// <param name="Project">проект GIT</param>
        /// <param name="reportSQLFile">имя проверяемого файла (для отчета)</param>
        /// <param name="scripttype">тип проверяемого скрипта</param>
        /// <param name="lines">массив проверяемых строк</param>
        /// <param name="isCheckRegion">скрипт предназначен для определенных регионов (НЕ базовый), в скрипте должна быть проверка на региональность</param>
        /// <param name="isBaseRegion">у скрипта Базовая региональность БД, в скрипте НЕ должна быть проверка на региональность</param>
        /// <param name="isRelease">=true - особенности проверка при сборки версии, например доболнительно ищем в проверке на региональность слово release и т.д.</param>
        /// <param name="Errors">список обнаруженных ошибок</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <param name="isCheckLabel">=false - не проверять метки</param>
        /// <returns>возвращает SQLTextCheckResult</returns>
        public static SQLTextCheckResult SQLTextCheck(string Project, string reportSQLFile, string scripttype, string[] lines, bool isCheckRegion, bool isBaseRegion, bool isRelease, ref string Errors, string logFile, bool isCheckLabel)
        {
            if (Errors == null) Errors = "";

            SQLTextCheckResult result = new SQLTextCheckResult();

            if (lines == null || lines.LongLength == 0)
            {
                return result;
            }

            string DBType = Utilities.GITProjects.GetDBTypeByProject(Project); // тип БД = MSSQL или PGSQL

            // ----------------------------------------------------------
            // проверка на случай появления нового проекта GIT, не внесенного в настройки
            if (DBType == "")
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: Проект " + Project + " для неизвестного типа БД" + Environment.NewLine;
                return result;
            }

            // ================================================================
            // Анализ содержимого скрипта
            // ================================================================

            // инициализируем поиск ключевых слов
            KeyWord.InitKeyWords();

            bool isRegion = false;
            bool isRegionRelease = false;
            bool isCheckNotId_Table = false;
            bool isCheckNotId_FieldId = false;
            string CheckNotId_TableName = "";
            int cntINSERT = 0; // счетчик insert (+1) и on conflict (-1)
            int cntIDENTITY = 0; // счетчик idenity on (+1) и idenity off (-1)
            int row = 0;

            int isComment = 0; // кол-во стартов блока комментариев
            int ii;

            // находим ключевые фразы в файле, перебирая строки
            foreach (var line in lines)
            {
                if (line.Length > 10000)
                {
                    result.isBAD = true;
                    Errors += "ВНИМАНИЕ: Строка в файле " + reportSQLFile + " пропущена из-за того, в ней содержится больше 10000 символов" + Environment.NewLine;
                    continue;
                }

                // приводим строку к нужному для анализа виду
                string s = line.ToLower().Trim();
                // номер строки в файле
                row++;

                // прогоняем строку по поиску всех ключевых слов, которые могут быть в комментариях
                foreach (var item in KeyWord.ListKeyWords.Where(x => x.Value.CheckWithComment == true))
                {
                    item.Value.Check(row, s);
                }

                /* убираем комментарии из строки*/
                bool flag;
                do
                {
                    // флаг повторного выполнения
                    flag = false;

                    // словарь символов комментария, найденных в строке
                    SortedDictionary<int, string> com = new SortedDictionary<int, string>();

                    // заполняем словарь
                    ii = 0;
                    while (ii > -1)
                    {
                        ii = s.IndexOf("/*", ii);
                        if (ii > -1)
                        {
                            com.Add(ii, "/*");
                            ii++;
                        }
                    }
                    ii = 0;
                    while (ii > -1)
                    {
                        ii = s.IndexOf("*/", ii);
                        if (ii > -1)
                        {
                            com.Add(ii, "*/");
                            ii++;
                        }
                    }
                    ii = 0;
                    while (ii > -1)
                    {
                        ii = s.IndexOf("--", ii);
                        if (ii > -1)
                        {
                            com.Add(ii, "--");
                            ii++;
                        }
                    }

                    // анализируем наличие в строке символов комментария
                    if (com.Count > 0)
                    {
                        // первый символ с начала строки
                        var first = com.First();

                        if (first.Value == "--")
                        {
                            // если комментарий начинается с --, проверяем, может в предыдущих строках был /* и в этой строке есть */
                            var next = com.Where(x => (x.Key > first.Key) && (x.Value == "*/")).FirstOrDefault();
                            if ((isComment > 0) && (!next.Equals(default(KeyValuePair<int, string>))))
                            {
                                // если в предыдущих строках был /*, а в этой строке есть символ завершения комментария */ - вырезаем комментарий с начала строки по */
                                s = s.Substring(next.Key + 2).Trim();
                                // уменьшаем счетчик
                                isComment--;
                                // и начинаем анализ заново
                                flag = true;
                                continue;
                            }
                            else
                            {
                                // если НЕТ символа завершения комментария */ - убираем начиная с -- до конца строки
                                s = s.Substring(0, first.Key).Trim();
                            }
                        }
                        else if (first.Value == "/*")
                        {
                            // если комментарий начинается с /*, ищем в этой же строке символ */
                            var next = com.Where(x => (x.Key > first.Key) && (x.Value == "*/")).FirstOrDefault();
                            if (!next.Equals(default(KeyValuePair<int, string>)))
                            {
                                // если комментарий начинаяется и завершается в этой же строке - вырезаем комментарий
                                s = (s.Substring(0, first.Key) + s.Substring(next.Key + 2)).Trim();
                                // и начинаем анализ заново
                                flag = true;
                                continue;
                            }
                            else
                            {
                                // если комментарий начинается /*, но не завершается */ в этой же строке - убираем все начиная с /* до конца строки
                                s = s.Substring(0, first.Key).Trim();
                                // увеличиваем счетчик
                                isComment++;
                            }
                        }
                        else
                        {
                            // если первым идет символ */ - вырезаем все с начала строки по */ включительно
                            s = s.Substring(first.Key + 2).Trim();
                            // уменьшаем счетчик
                            isComment--;
                            // и начинаем анализ заново
                            flag = true;
                            continue;
                        }
                    }
                } while (flag);

                if (isComment > 0)
                {
                    // комментарий начат на предыдущей строке, убираем всю строку
                    s = "";
                }

                // прогоняем строку по поиску всех ключевых слов, игнорируя комментарии
                foreach (var item in KeyWord.ListKeyWords.Where(x => x.Value.CheckWithComment == false))
                {
                    //if (item.Key == "GET_REGION" && s.StartsWith("IF (SELECT".ToLower()))
                    //{
                    //    int iii = 0;
                    //}

                    item.Value.Check(row, s);
                }

                /*
                if (s.Contains("go"))
                {
                    // для тестирования
                    int i = 0;
                }
                */

                // прогоняем строку по списку таблиц, для которых в скриптах по дате не должно быть идентифкаторов
                foreach (var item in MainWindow.APPinfo.CheckNoIdTables)
                {
                    if (item != "")
                    {
                        string _schema = "dbo";
                        string _table = item.ToLower();

                        var arr = _table.Split('.');
                        if (arr.Length > 1)
                        {
                            _schema = arr[0];
                            _table = arr[1];
                        }

                        string _nick;
                        if (_schema == "dbo") _nick = _table;
                        else _nick = _schema + "." + _table;

                        string _id = _table + "_id";

                        if (!isCheckNotId_Table)
                        {
                            isCheckNotId_Table = s.Contains(_nick);
                            CheckNotId_TableName = _table;
                        }
                        if (!isCheckNotId_FieldId) isCheckNotId_FieldId = s.Contains(_id);
                    }
                }

                // кол-во НЕ пустых строк
                if (!string.IsNullOrWhiteSpace(s)) result.NoEmptyRow++;
            }


            /*
            if (fullSQLFile.Contains("dbo_curestandartref_70188_20210719.sql"))
            {
                // для тестирования
                int i = 0;
            }
            */

            // проверяем наличие ключевых фраз, проставляем флаги
            result.hasLiquibase = KeyWord.GetKeyWord("--liquibase formatted sql").isFound;
            result.hasChangeset = KeyWord.GetKeyWord("--changeset").isFound;
            bool isstripcomments = KeyWord.GetKeyWord("stripcomments:false").isFound;
            bool isdbmsMSsql = KeyWord.GetKeyWord("dbms:mssql").isFound;
            bool isdbmsPGsql = KeyWord.GetKeyWord("dbms:postgresql").isFound;
            bool isenddelimiterGO = KeyWord.GetKeyWord("enddelimiter:go").isFound;
            bool isenddelimiterSemicolon = KeyWord.GetKeyWord("enddelimiter:;;").isFound;
            result.hasAutogen = KeyWord.GetKeyWord("autogen").isFound;
            bool isGenView = KeyWord.GetKeyWord("xp_gen_view").isFound;
            bool isDropfns = KeyWord.GetKeyWord("xp_dropfns").isFound;
            result.hasGenIdentity = KeyWord.GetKeyWord("xp_genidentity").isFound;
            bool isDrop = KeyWord.GetKeyWord("DROP").isFound;
            result.hasCreateTable = KeyWord.GetKeyWord("CREATE").isFound;
            bool isCreateNotExists = KeyWord.GetKeyWord("CREATE_NOT_EXISTS").isFound;
            bool isInsert = KeyWord.GetKeyWord("INSERT").isFound;
            bool isDropColumn = KeyWord.GetKeyWord("DROP_COLUMN").isFound;
            bool isRenameColumn = KeyWord.GetKeyWord("RENAME_COLUMN").isFound;
            bool isSp_Rename = KeyWord.GetKeyWord("sp_rename").isFound;
            bool isWhile = KeyWord.GetKeyWord("WHILE").isFound;
            bool isLoop = KeyWord.GetKeyWord("LOOP").isFound;
            bool isCommit = (KeyWord.GetKeyWord("COMMIT").CountRows - KeyWord.GetKeyWord("ON_COMMIT").CountRows) > 0;
            bool isRollback = KeyWord.GetKeyWord("ROLLBACK").isFound;
            bool isRunInTransaction = KeyWord.GetKeyWord("runintransaction:false").isFound;

            bool isCreateStored = KeyWord.GetKeyWord("CREATE_STORED").isFound;
            int MinRowCreateStored = KeyWord.GetKeyWord("CREATE_STORED").MinRow.Key;

            bool isSetDate = KeyWord.GetKeyWord("SET_DATE").isFound;
            bool isSetRegion = KeyWord.GetKeyWord("SET_REGION").isFound;
            bool isGetRegion = KeyWord.GetKeyWord("GET_REGION").isFound;
            bool isGetRegionRelease = KeyWord.GetKeyWord("GET_REGION_RELEASE").isFound;

            bool isSet = KeyWord.GetKeyWord("SET").isFound;
            int MinRowSet = KeyWord.GetKeyWord("SET").MinRow.Key;

            bool isLabelStruct = false;
            bool isLabelCode = false;
            bool isLabelData = false;
            bool isLabelFinish = false;

            if (MainWindow.APPinfo.isImproveSQLinVersion && isCheckLabel)
            {
                isLabelStruct = KeyWord.GetKeyWord("labels:struct").isFound;
                isLabelCode = KeyWord.GetKeyWord("labels:code").isFound;
                isLabelData = KeyWord.GetKeyWord("labels:data").isFound;
                isLabelFinish = KeyWord.GetKeyWord("labels:finish").isFound;
            }

            /*
            bool isSetIdenity = KeyWord("SET_IDENTITY_ON").isFound;
            if (!isSetIdenity) isSetIdenity = KeyWord("SET_IDENTITY_OFF").isFound;
            */

            result.hasCommentInScript = KeyWord.GetKeyWord("COMMENT").isFound;

            cntINSERT = KeyWord.GetKeyWord("INSERT").CountRows - KeyWord.GetKeyWord("ON_CONFLICT").CountRows;
            cntIDENTITY = KeyWord.GetKeyWord("SET_IDENTITY_ON").CountRows - KeyWord.GetKeyWord("SET_IDENTITY_OFF").CountRows;

            if (isSetRegion && isGetRegion)
            {
                // подавляем ложное срабатывание
                //int i = 0;
            }
            else
            {
                isRegion = isGetRegion;
                isRegionRelease = isGetRegionRelease; //-V3137
            }

            bool isGO = KeyWord.GetKeyWord("GO").isFound;

            // ================================================================
            // Проверки содержимого скрипта
            // ================================================================

            // ----------------------------------------------------------
            // проверка на незавершенный комментарий
            if (isComment > 0)
            {
                result.isBAD = true;
                Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте есть незавершенный комментарий, начатый /*" + Environment.NewLine;
            }
            if (isComment < 0)
            {
                result.isBAD = true;
                Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте есть лишнее завершение комментария */" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // выполнить проверку скрипта на наличие ключевых фраз для liquibot
            if (result.hasChangeset)
            {
                if (
                    (scripttype != "table") &&
                    (scripttype != "marker") &&
                    (KeyWord.GetKeyWord("--changeset").Rows.Count > 1)
                )
                {
                    result.isBAD = true;
                    Errors += "ВНИМАНИЕ: " + reportSQLFile + " - строк --changeset больше одной и это не таблица или маркер!" + Environment.NewLine;
                }

                if (
                    Utilities.GITProjects.IsDEVProject(Project) &&
                    (scripttype != "test")
                )
                {
                    // проверка на наличие номера задачи в changeset
                    bool isexist = false;
                    foreach (var rowpair in KeyWord.GetKeyWord("--changeset").Rows)
                    {
                        var line = rowpair.Value;
                        var arr = line.Split(' ');
                        foreach (var pair in arr)
                        {
                            var arrw = pair.Split(':');
                            if (
                                (arrw.Length > 1) &&
                                (!string.IsNullOrWhiteSpace(arrw[1])) &&
                                (!string.IsNullOrWhiteSpace(Utilities.Task.GetTaskNumber(arrw[1])))
                            )
                            {
                                isexist = true;
                            }
                        }

                    }

                    if (!isexist)
                    {
                        result.isBAD = true;
                        Errors += "ОШИБКА: " + reportSQLFile + " - changeset не содержит номер задачи" + Environment.NewLine;
                    }

                }

                // проверка на корректность тегов в составе changset
                foreach (var rowpair in KeyWord.GetKeyWord("--changeset").Rows)
                {
                    var line = rowpair.Value;
                    var arr = line.Split(' ');
                    foreach (var pair in arr)
                    {
                        if (
                            (!string.IsNullOrWhiteSpace(pair)) &&
                            (pair != "--changeset")
                        )
                        {
                            var arrw = pair.Split(':');
                            if (
                                (arrw.Length == 1) ||
                                (string.IsNullOrWhiteSpace(arrw[1]))
                            )
                            {
                                result.isBAD = true;
                                Errors += "ОШИБКА: " + reportSQLFile + $" - в строке --changeset тег {arrw[0]} не содержит значение, либо знак ':' отделен пробелами" + Environment.NewLine;
                            }
                        }
                    }

                }

                if (!isstripcomments)
                {
                    result.isBAD = true;
                    Errors += "ОШИБКА: " + reportSQLFile + " - не содержит stripComments:false" + Environment.NewLine;
                }

                if (MainWindow.APPinfo.isImproveSQLinVersion && isCheckLabel)
                {
                    if (scripttype == "table" && !isLabelStruct && !isLabelFinish)
                    {
                        result.isBAD = true;
                        Errors += "ОШИБКА: " + reportSQLFile + " - не содержит labels:struct или labels:finish" + Environment.NewLine;
                    }

                    if ((scripttype == "data" || scripttype == "marker") && !isLabelData)
                    {
                        result.isBAD = true;
                        Errors += "ОШИБКА: " + reportSQLFile + " - не содержит labels:data" + Environment.NewLine;
                    }

                    if ((scripttype == "proc" || scripttype == "view") && !isLabelCode && !isLabelFinish)
                    {
                        result.isBAD = true;
                        Errors += "ОШИБКА: " + reportSQLFile + " - не содержит labels:code или labels:finish" + Environment.NewLine;
                    }
                }

                if ((DBType == "MSSQL") && (!isdbmsMSsql))
                {
                    result.isBAD = true;
                    Errors += "ОШИБКА: " + reportSQLFile + " - не содержит dbms:mssql" + Environment.NewLine;
                }

                if ((DBType == "PGSQL") && (!isdbmsPGsql))
                {
                    result.isBAD = true;
                    Errors += "ОШИБКА: " + reportSQLFile + " - не содержит dbms:postgresql" + Environment.NewLine;
                }

                if ((DBType == "PGSQL") && (!isenddelimiterSemicolon))
                {
                    result.isBAD = true;
                    Errors += "ОШИБКА: " + reportSQLFile + " - не содержит endDelimiter:;;" + Environment.NewLine;
                }
            }

            if ((DBType == "MSSQL") && isdbmsPGsql)
            {
                result.isBAD = true;
                Errors += "ОШИБКА: " + reportSQLFile + " - скрипт для MS SQL содержит тег dbms:postgresql" + Environment.NewLine;
            }

            if ((DBType == "PGSQL") && isdbmsMSsql)
            {
                result.isBAD = true;
                Errors += "ОШИБКА: " + reportSQLFile + " - скрипт для Postgre SQL содержит тег dbms:mssql" + Environment.NewLine;
            }

            if ((DBType == "MSSQL") && (!isenddelimiterGO) && isGO)
            {
                result.isBAD = true;
                Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте есть GO, но нет тега endDelimiter:GO" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // выполнить проверку скрипта на наличие ON CONFLICT после INSERT
            if ((DBType == "PGSQL") && (cntINSERT > 0) && (scripttype == "data") && (!reportSQLFile.ToLower().Contains("stg.localdblist")))
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - INSERT без ON CONFLICT" + Environment.NewLine;
            }
            if ((DBType == "PGSQL") && (cntINSERT < 0) && (scripttype == "data"))
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - ON CONFLICT без INSERT" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // наличие/отсутствие проверки на региональность
            if (isBaseRegion && isRegion)
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в задаче стоит Базовая региональность БД, но в скрипте есть проверка на региональность" + Environment.NewLine;
            }

            /*if (isRelease)
            {
                if ((!isBaseRegion) && isCheckRegion && (!isRegionRelease))
                {
                    result.isBAD = true;

                    Errors += reportSQLFile + ": в скрипте возможно нет проверки на региональность + ProMedWebRelease/promedrelease" + Environment.NewLine;
                }
            }
            else
            {*/
            if ((!isBaseRegion) && isCheckRegion && (!isRegion))
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте возможно нет проверки на региональность" + Environment.NewLine;
            }
            //}

            // ----------------------------------------------------------
            // наличие DROP
            if ((DBType == "PGSQL") && (scripttype == "proc") && (!isDropfns))
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте нет xp_dropfns" + Environment.NewLine;
            }
            if ((DBType == "PGSQL") && (scripttype == "view") && (!isGenView))
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте нет xp_gen_view" + Environment.NewLine;
            }

            if ((DBType == "MSSQL") && (scripttype == "proc") && (!isDrop))
            {
                result.isBAD = true;
                Errors += reportSQLFile + ": в скрипте нет DROP PROCEDURE / DROP FUNCTION" + Environment.NewLine;
            }
            if ((DBType == "MSSQL") && (scripttype == "view") && (!isDrop))
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте нет DROP VIEW" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // выполнить проверку скрипта на одинаковое кол-во SET IDENTITY ON, SET IDENTITY OFF
            if ((DBType == "MSSQL") && (cntIDENTITY != 0) && (scripttype == "data"))
            {
                result.isBAD = true;
                Errors += "ОШИБКА: " + reportSQLFile + " - кол-во SET IDENTITY_INSERT ON не совпадает с кол-вом SET IDENTITY_INSERT OFF" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // наличие CREATE TABLE IF NOT EXISTS
            if ((DBType == "PGSQL") && (scripttype == "table") && result.hasCreateTable && (!isCreateNotExists))
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте нет IF NOT EXISTS для CREATE TABLE" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // наличие ID при добавлении в таблицы, в которых не следует заполнять ID
            if ((scripttype == "data" || scripttype == "marker") && isInsert && isCheckNotId_Table && isCheckNotId_FieldId)
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - возможно, что в INSERT INTO " + CheckNotId_TableName + " заполняется " + CheckNotId_TableName + "_id" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // наличие drop column
            if ((scripttype == "table") && isDropColumn)
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте есть DROP COLUMN" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // наличие rename column
            if ((scripttype == "table") && isRenameColumn)
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте есть RENAME COLUMN" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // наличие sp_rename
            if ((DBType == "MSSQL") && (scripttype == "table") && isSp_Rename)
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте есть команда переименования поля в таблице sp_rename" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // выполнить проверку скрипта на наличие SET
            if ((DBType == "MSSQL") && (scripttype == "table") && isSet)
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте есть команда SET" + Environment.NewLine;
            }
            if ((DBType == "MSSQL") && ((scripttype == "view") || (scripttype == "proc")) && (MinRowSet > 0) && (MinRowSet < MinRowCreateStored))
            {
                result.isBAD = true;
                Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте есть команда SET до команды CREATE" + Environment.NewLine;
            }

            // ----------------------------------------------------------
            // выполнить проверки на LOOP, WHILE, COMMIT, runInTransaction:false
            if (
                (scripttype == "data") &&
                isWhile &&
                (!isCommit)
                )
            {
                result.isBAD = true;
                Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте есть WHILE, но нет COMMIT" + Environment.NewLine;
            }
            if (
                (scripttype == "data") &&
                isLoop &&
                (!isCommit)
                )
            {
                result.isBAD = true;
                Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте есть LOOP, но нет COMMIT" + Environment.NewLine;
            }
            if (
                (scripttype == "data" || scripttype == "test") &&
                (isCommit || isRollback) &&
                (!isRunInTransaction)
                )
            {
                result.isBAD = true;
                Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте есть COMMIT или ROLLBACK, но нет тега runInTransaction:false" + Environment.NewLine;
            }

            return result;
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Проверка содержимого sql-файла</summary>
        /// <param name="Project">проект GIT</param>
        /// <param name="path">тип объекта GIT или путь к папке GIT</param>
        /// <param name="fullYMLFile">yml-файл, в котором указан проверяемый файл</param>
        /// <param name="fullSQLFile">полный путь к проверяемому файлу</param>
        /// <param name="IsCorrectEncoding">исправлять неправильную кодировку</param>
        /// <param name="isCheckBOM">проверять наличие BOM для кодировки UTF8</param>
        /// <param name="isCheckRegion">скрипт предназначен для определенных регионов (НЕ базовый), в скрипте должна быть проверка на региональность</param>
        /// <param name="isBaseRegion">у скрипта Базовая региональность БД, в скрипте НЕ должна быть проверка на региональность</param>
        /// <param name="isRelease">=true - особенности проверка при сборки версии, например доболнительно ищем в проверке на региональность слово release и т.д.</param>
        /// <param name="Errors">список обнаруженных ошибок</param>
        /// <param name="isSkipLargeFile">=true - пропускать большие файлы (>1Мб)</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <param name="isCheckLabel">=false - не проверять метки</param>
        /// <returns>true - есть ошибки</returns>
        public static bool IsSQLFileBAD(string Project, string path, string fullYMLFile, string fullSQLFile, bool IsCorrectEncoding, bool isCheckBOM, bool isCheckRegion, bool isBaseRegion, bool isRelease, ref string Errors, bool isSkipLargeFile, string logFile, bool isCheckLabel)
        {
            if (Errors == null) Errors = "";

            string DBType = Utilities.GITProjects.GetDBTypeByProject(Project); // тип БД = MSSQL или PGSQL
            string GITProjectFolder = Utilities.GITProjects.GetFolderByProject(Project);
            string ProjectPath = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder);
            bool result = false;
            string reportSQLFile = fullSQLFile;

            if (ProjectPath.EndsWith("" + Path.DirectorySeparatorChar))
                reportSQLFile = reportSQLFile.Replace(ProjectPath, "");
            else
                reportSQLFile = reportSQLFile.Replace(ProjectPath + Path.DirectorySeparatorChar, "");

            path = path.ToLower();

            string scripttype = "";
            if (
                path.Contains("/data/") ||
                path.Contains("\\data\\") ||
                path.StartsWith("data\\") ||
                path.StartsWith("data/") ||
                path.EndsWith("/data") ||
                path.EndsWith("\\data") ||
                (path == "data") ||
                path.Contains("/data_new/") ||
                path.Contains("\\data_new\\") ||
                path.StartsWith("data_new\\") ||
                path.StartsWith("data_new/") ||
                path.EndsWith("/data_new") ||
                path.EndsWith("\\data_new") ||
                (path == "data_new")
                )
            {
                scripttype = "data";
            }
            else if (
                path.Contains("/table/") ||
                path.Contains("\\table\\") ||
                path.StartsWith("table\\") ||
                path.StartsWith("table/") ||
                path.EndsWith("\\table") ||
                path.EndsWith("/table") ||
                (path == "table")
                )
            {
                scripttype = "table";
            }
            else if (
                path.Contains("/view/") ||
                path.Contains("\\view\\") ||
                path.StartsWith("view\\") ||
                path.StartsWith("view/") ||
                path.EndsWith("\\view") ||
                path.EndsWith("/view") ||
                (path == "view")
                )
            {
                scripttype = "view";
            }
            else if (
                path.Contains("/procedure/") ||
                path.Contains("\\procedure\\") ||
                path.StartsWith("procedure\\") ||
                path.StartsWith("procedure/") ||
                path.EndsWith("\\procedure") ||
                path.EndsWith("/procedure") ||
                (path == "procedure") ||
                (path == "proc") ||
                path.Contains("/function/") ||
                path.Contains("\\function\\") ||
                path.StartsWith("function\\") ||
                path.StartsWith("function/") ||
                path.EndsWith("\\function") ||
                path.EndsWith("/function") ||
                (path == "function") ||
                (path == "func")
                )
            {
                scripttype = "proc";
            }
            else if (
                path.Contains("/freedocrelationship/") ||
                path.Contains("\\freedocrelationship\\") ||
                path.StartsWith("freedocrelationship\\") ||
                path.StartsWith("freedocrelationship/") ||
                path.EndsWith("\\freedocrelationship") ||
                path.EndsWith("/freedocrelationship") ||
                (path == "freedocrelationship") ||
                path.Contains("/freedocmarker/") ||
                path.Contains("\\freedocmarker\\") ||
                path.StartsWith("freedocmarker\\") ||
                path.StartsWith("freedocmarker/") ||
                path.EndsWith("\\freedocmarker") ||
                path.EndsWith("/freedocmarker") ||
                (path == "freedocmarker") 
                )
            {
                scripttype = "marker";
            }

            // ----------------------------------------------------------
            // проверка на случай появления нового проекта GIT, не внесенного в настройки
            if (DBType == "")
            {
                result = true;
                Errors += "ВНИМАНИЕ: Проект " + Project + " для неизвестного типа БД" + Environment.NewLine;
                return result;
            }

            // ----------------------------------------------------------
            // проверка имени файла
            string _file = Path.GetFileName(fullSQLFile);

            if (
                    (_file.Contains(" ") && isRelease) ||
                    _file.Contains("\t")
            )
            {
                result = true;
                Errors += $"ОШИБКА: В имени файла {_file} есть пробелы или символы табуляции !" + Environment.NewLine;
            }

            if (File.Exists(fullSQLFile) && fullSQLFile.ToLower().EndsWith(".sql"))
            {
                App.AddLog($"Проверяем файл {fullSQLFile}", null, App.ShowMessageMode.NONE, true, logFile);

                // ----------------------------------------------------------
                // проверка скрипта на размер
                double size = ((double)new FileInfo(fullSQLFile).Length) / 1024 / 1024;

                if ((size > 0.5) && isSkipLargeFile)
                {
                    result = true;
                    Errors += "ВНИМАНИЕ: Проверка файла " + reportSQLFile + " прервана из-за того, что размер больше 0.5 Мб" + Environment.NewLine;
                    return result;
                }

                if (size > 1)
                {
                    result = true;
                    Errors += "ВНИМАНИЕ: Проверка файла " + reportSQLFile + " прервана из-за того, что размер больше 1 Мб" + Environment.NewLine;
                    return result;
                }

                // ================================================================
                // Анализ содержимого скрипта
                // ================================================================

                bool isBOM;
                string encoding = Utilities.Files.GetEncoding(fullSQLFile, out isBOM);

                string[] lines = new string[0];

                bool hasContextNewDb = false;
                bool hasMarkRun = false;
                bool hasTestChangeset = false;

                // в скриптах для таблиц/хранимок/вьюх - исключить changeset context:newdb, а также changeset, у которых есть тег --preConditions onFail:MARK_RAN, а также тестовый changeset
                if (
                    (scripttype == "table") ||
                    (scripttype == "view") ||
                    (scripttype == "proc") ||
                    (scripttype == "marker")
                )
                {
                    // делим на changeset
                    var list = SQLChangeset.ReadChangeset(fullSQLFile, false, logFile);
                    string buffer = "";

                    foreach (var item in list)
                    {
                        if (item.hasLiquibaseFormattedSQL)
                        {
                            buffer += "\n--liquibase formatted sql";
                        }

                        if (item.isContextNewDb)
                        {
                            hasContextNewDb = true;
                        }

                        if (item.isMarkRun)
                        {
                            hasMarkRun = true;
                        }

                        if (item.isTestChangeset)
                        {
                            hasTestChangeset = true;

                            // проверим тестовый changeset
                            string[] testlines = item.text.Trim('\n').Split('\n');

                            var checktest = SQLTextCheck(Project, reportSQLFile, "test", testlines, false, true, isRelease, ref Errors, logFile, isCheckLabel);

                            if (checktest.isBAD)
                            {
                                result = true;
                            }
                        }

                        if (!item.isContextNewDb && !item.isMarkRun && !item.isTestChangeset)
                        {
                            buffer += "\n" + item.text;
                        }
                    }

                    lines = buffer.Trim('\n').Split('\n');
                }
                else
                {
                    lines = File.ReadAllLines(fullSQLFile);
                }

                App.AddLog($"Файл {fullSQLFile} прочитан", null, App.ShowMessageMode.NONE, true, logFile);

                // ================================================================
                // Анализируем содержимое, выполняем первичную проверку
                // ================================================================
                var checkresult = SQLTextCheck(Project, reportSQLFile, scripttype, lines, isCheckRegion, isBaseRegion, isRelease, ref Errors, logFile, isCheckLabel);
                if (checkresult.isBAD)
                {
                    result = true;
                }

                // ================================================================
                // Анализируем файл в целом
                // ================================================================
                if (checkresult.NoEmptyRow == 0)
                {
                    result = true;
                    if (hasContextNewDb || hasMarkRun || hasTestChangeset)
                    {
                        //Errors += "ВНИМАНИЕ: " + reportSQLFile + " - кроме --changset init в файле больше ничего нет!" + Environment.NewLine;
                    }
                    else
                    {
                        Errors += "ВНИМАНИЕ: " + reportSQLFile + " - файл пустой!" + Environment.NewLine;
                    }
                }

                // ----------------------------------------------------------
                // выполнить проверку скрипта на наличие ключевых фраз для liquibot
                if (!checkresult.hasLiquibase)
                {
                    result = true;
                    Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте нет --liquibase formatted sql" + Environment.NewLine;
                }

                if ((!checkresult.hasLiquibase) && (checkresult.hasChangeset || hasContextNewDb || hasMarkRun || hasTestChangeset))
                {
                    result = true;
                    Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте нет --liquibase formatted sql, но есть --changeset" + Environment.NewLine;
                }

                if (!checkresult.hasChangeset)
                {
                    result = true;
                    if (hasContextNewDb)
                    {
                        Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте есть --changeset с тегом context:newdb, но нет выполняемых --changeset" + Environment.NewLine;
                    }
                    if (hasTestChangeset)
                    {
                        Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте есть тестовый --changeset, но нет выполняемых --changeset" + Environment.NewLine;
                    }
                    if (hasMarkRun)
                    {
                        Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте есть --changeset с тегом --preConditions onFail:MARK_RAN, но нет выполняемых --changeset" + Environment.NewLine;
                    }
                    if (!hasContextNewDb && !hasMarkRun && !hasTestChangeset)
                    {
                        Errors += "ОШИБКА: " + reportSQLFile + " - в скрипте нет --changeset" + Environment.NewLine;
                    }
                }

                // ----------------------------------------------------------
                // выполнить проверку кодировки скрипта
                if (

                    ((scripttype == "data" || scripttype == "marker") &&
                     (encoding != "UTF8") && (encoding != "UTF-16LE") && (encoding != "UTF32")
                    ) ||

                    (((scripttype == "view") || (scripttype == "proc")) && (!checkresult.hasAutogen) &&
                     (encoding != "UTF8") && (encoding != "UTF-16LE") && (encoding != "UTF32")
                    ) ||

                    ((scripttype == "table") && checkresult.hasCommentInScript &&
                     (encoding != "UTF8") && (encoding != "UTF-16LE") && (encoding != "UTF32")
                    )
                )
                {
                    if (
                            (encoding == "1251") && IsCorrectEncoding &&
                            (System.Windows.Forms.MessageBox.Show("Кодировка файла " + reportSQLFile + " - Windows 1251 !\nИсправить на UTF8 ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        )
                    {
                        App.AddLog("Кодировка файла " + reportSQLFile + " - Windows 1251, выбрано исправление на UTF8", null, App.ShowMessageMode.NONE, true, logFile);

                        if (!Utilities.Files.SetEncodingToUTF8(ref fullSQLFile, ref Errors))
                        {
                            result = true;
                            Errors += "ОШИБКА: " + reportSQLFile + " - кодировка отличается от UTF8" + Environment.NewLine;
                        }
                        else { result = true; encoding = "UTF8"; isBOM = false; }
                    }
                    else
                    {
                        result = true;
                        Errors += "ОШИБКА: " + reportSQLFile + " - кодировка отличается от UTF8" + Environment.NewLine;
                    }
                }

                // ----------------------------------------------------------
                // выполнить проверку скрипта на BOM если кодировка UTF8
                if ((encoding == "UTF8") && isBOM && isCheckBOM)
                {
                    if (
                            IsCorrectEncoding &&
                            (System.Windows.Forms.MessageBox.Show("Кодировка файла " + reportSQLFile + " UTF8 BOM !\nИсправить на UTF8 ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        )
                    {
                        App.AddLog("Кодировка файла " + reportSQLFile + " - UTF8 BOM, выбрано исправление на UTF8", null, App.ShowMessageMode.NONE, true, logFile);

                        if (!Utilities.Files.SetEncodingToUTF8(ref fullSQLFile, ref Errors))
                        {
                            result = true;
                            Errors += "ВНИМАНИЕ: " + reportSQLFile + " - кодировка осталась UTF8 BOM" + Environment.NewLine;
                        }
                        else
                        {
                            result = true;
                            isBOM = false; //-V3137
                        }
                    }
                    else
                    {
                        Errors += "ВНИМАНИЕ: " + reportSQLFile + " - кодировка осталась UTF8 BOM" + Environment.NewLine;
                        result = true;
                    }
                }

                // ----------------------------------------------------------
                // наличие xp_genidentity
                if ((DBType == "PGSQL") && (scripttype == "table") && checkresult.hasCreateTable && (!hasContextNewDb) && (!hasMarkRun) && (!checkresult.hasGenIdentity))
                {
                    result = true;
                    Errors += "ВНИМАНИЕ: " + reportSQLFile + " - в скрипте нет xp_genidentity" + Environment.NewLine;
                }

                App.AddLog($"Файл {fullSQLFile} проверен" + Environment.NewLine, null, App.ShowMessageMode.NONE, true, logFile);
            }

            return result;
        }

        /// <summary>
        /// Возвращает тег liquibase "labels" по имени папки в проекте git
        /// </summary>
        /// <param name="author">автор chanheset</param>
        /// <param name="id">id changeset</param>
        /// <param name="scripttype">тип скрипта или папка в проекте git</param>
        /// <param name="value">текущее значение</param>
        /// <returns></returns>
        public static string LiquibaseLabelTag (string author, string id, string scripttype, string value)
        {
            if (! MainWindow.APPinfo.isImproveSQLinVersion) return "";

            if (author == "dev" && id == "test") return "";

            if (string.IsNullOrWhiteSpace(value))
            {
                List_GITType_Label.TryGetValue(scripttype.ToUpper(), out value);
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                value = "finish";
            }

            return "labels:" + value;
        }

        /// <summary>
        /// Возвращает тег liquibase "runInTransaction"
        /// </summary>
        /// <param name="value">текущее значение</param>
        /// <returns></returns>
        public static string LiquibaseRunInTransactionTag(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }
            else
            {
                return "runInTransaction:" + value;
            }
        }

        /// <summary>
        /// Возвращает тег liquibase "context"
        /// </summary>
        /// <param name="value">текущее значение</param>
        /// <returns></returns>
        public static string LiquibaseContextTag(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }
            else
            {
                return "context:" + value;
            }
        }

        /// <summary>
        /// Возвращает тег liquibase "runAlways"
        /// </summary>
        /// <param name="value">текущее значение</param>
        /// <returns></returns>
        public static string LiquibaseRunAlwaysTag(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }
            else
            {
                return "runAlways:" + value;
            }
        }

        /// <summary>
        /// Возвращает тег liquibase "dbms" по типу БД
        /// </summary>
        /// <param name="dbtype">тип БД</param>
        /// <returns></returns>
        public static string LiquibaseDbmsTag (string dbtype)
        {
            if (dbtype.ToUpper() == "PGSQL")
            {
                return "dbms:postgresql";
            }
            else if (dbtype.ToUpper() == "MSSQL")
            {
                return "dbms:mssql";
            }
            else {
                return "dbms:unknown";
            }
        }

        /// <summary>
        /// Возвращает тег liquibase "endDelimiter" по типу БД
        /// </summary>
        /// <param name="dbtype">тип БД</param>
        /// <returns></returns>
        public static string LiquibaseEndDelimiterTag(string dbtype)
        {
            if (dbtype.ToUpper() == "PGSQL")
            {
                return "endDelimiter:;;";
            }
            else if (dbtype.ToUpper() == "MSSQL")
            {
                return "endDelimiter:GO";
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Возвращает тег liquibase "stripComments" по типу БД
        /// </summary>
        /// <param name="value">текущее значение</param>
        /// <returns></returns>
        public static string LiquibaseStripCommentsTag(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }
            else
            {
                return "stripComments:" + value;
            }
        }

        /// <summary>
        /// Получить список текущих значений тегов из строки --changeset
        /// </summary>
        /// <param name="changesetStr">строка --changeset</param>
        /// <returns></returns>
        public static Dictionary<string, string> GetTagsFromChangeset(string changesetStr)
        {
            if (string.IsNullOrWhiteSpace(changesetStr)) changesetStr = "";

            var Tags = new Dictionary<string, string>();

            var list = changesetStr
                .Replace("\t", " ")
                .TrimInner()
                .TrimAllSpace()
                .Split(' ')
                .ToList();

            foreach (var tag in list)
            {
                var pair = tag.ToList(new char[] { ':' }, true);
                if (pair.Count > 0)
                {
                    string key = pair[0];
                    string value = null;
                    if (pair.Count > 1) value = pair[1];

                    if (key.ToLower() == "--changeset") key = "";
                    if (key.ToLower() == "changeset") key = "";

                    // добавляем в список
                    if (
                        (!string.IsNullOrWhiteSpace(key)) &&
                        (!Tags.ContainsKey(key))
                    )
                    {
                        Tags.Add(key, value);
                    }
                }
            }

            return Tags;
        }

        /// <summary>
        /// Собрать/заполнить/дозаполнить строку changeset
        /// </summary>
        /// <param name="changesetStr">текущая строка changeset</param>
        /// <param name="setAuthor">=true - надо изменить автора</param>
        /// <param name="isInit">=true - changeset для начальной инициализации таблицы</param>
        /// <param name="ScriptType">тип скрипта или папка в проекте git</param>
        /// <param name="DBType">тип БД</param>
        /// <param name="isAddVersion">=true - добавить номер версии к имени changeset</param>
        /// <param name="version_no_prefix">номер версии БЕЗ префикса</param>
        /// <returns></returns>
        public static string MakeChangeset (string changesetStr, bool setAuthor, bool isInit, string ScriptType, string DBType, bool isAddVersion, string version_no_prefix)
        {
            if (string.IsNullOrWhiteSpace(changesetStr)) changesetStr = "";

            if (string.IsNullOrWhiteSpace(ScriptType)) ScriptType = "1";
            ScriptType = ScriptType.ToUpper();

            // заполняем текущие значения тегов
            var Tags = GetTagsFromChangeset(changesetStr);

            string labels = null;
            string runInTransaction = null;
            string context = null;
            string runAlways = null;
            string stripComments = null;
            var OtherTags = new Dictionary<string, string>();

            foreach (var tag in Tags)
            {
                string key = tag.Key ?? "";
                string value = tag.Value ?? "";

                // распознаем известные теги
                if (key.ToLower() == "--changeset") key = "";
                if (key.ToLower() == "changeset") key = "";
                if (key.ToLower() == "dbms") key = "";
                if (key.ToLower() == "enddelimiter") key = "";
                if (key.ToLower() == "stripcomments")
                {
                    key = "";
                    stripComments = value;
                }
                if (key.ToLower() == "runalways")
                {
                    key = "";
                    runAlways = value;
                }
                if (key.ToLower() == "context")
                {
                    key = "";
                    context = value;
                }
                if (key.ToLower() == "labels")
                {
                    key = "";
                    labels = value;
                }
                if (key.ToLower() == "runintransaction")
                {
                    key = "";
                    runInTransaction = value;
                }

                // добавляем в список неизвестные теги
                if (
                    (!string.IsNullOrWhiteSpace(key)) &&
                    (!OtherTags.ContainsKey(key))
                )
                {
                    OtherTags.Add(key, value);
                }
            }

            // собираем нераспознанные теги в одну строку
            string other = "";
            int cnt = 0;
            string Author = "";
            string Id = "";

            foreach (var tag in OtherTags)
            {
                string key = tag.Key;
                string value = tag.Value;

                cnt++;

                if (cnt == 1)
                {
                    // получаем автора и id скрипта, первый нераспознанный тег
                    Author = key;
                    Id = value;
                }
                else
                {
                    if (value != null)
                    {
                        other += key + ":" + value + " ";
                    }
                    else
                    {
                        other += key + " ";
                    }
                }
            }
            other = other.Trim();

            // проставляем автора
            if (string.IsNullOrWhiteSpace(Author) || setAuthor || isInit)
            {
                if (isInit)
                {
                    Author = "init";
                }
                else
                {
                    Author = MainWindow.Task.TaskExecutor;
                }
            }

            // проставляем идентификатор changeset
            if (string.IsNullOrWhiteSpace(Id) || isInit)
            {
                if (isInit)
                {
                    Id = ScriptType;
                }
                else
                {
                    Id = MainWindow.Task.TaskNumber;

                    // Добавляем номер версии для кода и маркеров
                    if (
                        isAddVersion &&
                        !string.IsNullOrWhiteSpace(version_no_prefix) &&
                        (
                            ScriptType == "FUNCTION" ||
                            ScriptType == "PROCEDURE" ||
                            ScriptType == "TRIGGER" ||
                            ScriptType == "VIEW" ||
                            ScriptType == "FREEDOCRELATIONSHIP" ||
                            ScriptType == "FREEDOCMARKER"
                        )
                    )
                    {
                        Id += "_" + version_no_prefix.Replace(".", "_");
                    }
                }
            }
            else
            {
                // Добавляем номер версии для кода и маркеров
                if (
                    isAddVersion && 
                    !string.IsNullOrWhiteSpace(version_no_prefix) &&
                    (
                        ScriptType == "FUNCTION" ||
                        ScriptType == "PROCEDURE" ||
                        ScriptType == "TRIGGER" ||
                        ScriptType == "VIEW" ||
                        ScriptType == "FREEDOCRELATIONSHIP" ||
                        ScriptType == "FREEDOCMARKER"
                    )
                )
                {
                    if (SQLChangeset.IsGoodChangesetName(Id))
                    {
                        Id = SQLGen.Task.ClearTaskNumber(Id) + "_" + version_no_prefix.Replace(".", "_");
                    }
                }
            }

            // собираем и возвращаем строку
            return
                $"--changeset {Author}:{Id} {other} {YML.LiquibaseLabelTag(Author, Id, ScriptType, labels)} {YML.LiquibaseContextTag(context)} {YML.LiquibaseRunAlwaysTag(runAlways)} {YML.LiquibaseStripCommentsTag(stripComments)} {YML.LiquibaseRunInTransactionTag(runInTransaction)} {YML.LiquibaseDbmsTag(DBType)} {YML.LiquibaseEndDelimiterTag(DBType)}"
                .TrimInner()
                .Trim();
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Исправим/добавим теги ликвибейз в sql-файле, сохраним его по другому маршруту
        /// </summary>
        /// <param name="from_file">оригинальный файл</param>
        /// <param name="ScriptType">тип скрипта или папка в проекте git</param>
        /// <param name="DBType">тип БД</param>
        /// <param name="to_file">сохранить в этот файл</param>
        /// <returns></returns>
        public static void CopyFileSetChangeset(string from_file, string ScriptType, string DBType, string to_file)
        {
            try
            {
                // считываем содержимое файла
                string text = File.ReadAllText(from_file);

                // находим тег changeset
                int start = text.IndexOf("--changeset");
                if (start == -1)
                {
                    // тега нет, добавляем
                    text = MakeChangeset("stripComments:false", true, false, ScriptType, DBType, false, "") + "\n" + text;
                }
                else
                {
                    // тег есть, исправляем

                    // находим ближаший перевод строки
                    int finish = text.ToLower().IndexOf("\n", start);

                    if (finish > start)
                    {
                        string orig_changeset = text.Substring(start, finish - start);
                        if (!orig_changeset.ToLower().Contains("stripcomments"))
                        {
                            orig_changeset = orig_changeset + " stripComments:false";
                        }
                        string new_changeset = MakeChangeset(orig_changeset, false, false, ScriptType, DBType, false, "");
                            
                        text = text.Replace(orig_changeset, new_changeset);
                    }
                }

                // находим тег --liquibase
                start = text.IndexOf("--liquibase");
                if (start == -1)
                {
                    // тега нет, добавляем
                    text = "--liquibase formatted sql\n" + text;
                }

                // сохраняем в файл
                File.WriteAllText(to_file, text);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
        }
    }

    /// <summary>
    /// результат проверки sql-текста
    /// </summary>
    public class SQLTextCheckResult
    {
        /// <summary>
        /// sql-текст содержит ошибки
        /// </summary>
        public bool isBAD { get; set; } = false;

        /// <summary>
        /// кол-во НЕ пустых строк
        /// </summary>
        public long NoEmptyRow { get; set; } = 0;

        /// <summary>
        /// есть строка --liquibase formatted sql
        /// </summary>
        public bool hasLiquibase { get; set; } = false;

        /// <summary>
        /// есть строка --changeset
        /// </summary>
        public bool hasChangeset { get; set; } = false;

        /// <summary>
        /// в скрипте есть CREATE TABLE
        /// </summary>
        public bool hasCreateTable { get; set; } = false;

        /// <summary>
        /// в скрипте есть xp_genidentity
        /// </summary>
        public bool hasGenIdentity { get; set; } = false;

        /// <summary>
        /// в скрипте есть AUTOGEN
        /// </summary>
        public bool hasAutogen { get; set; } = false;

        /// <summary>
        /// в скрипте есть изменения описаний таблиц и полей (ms_description или comment on)
        /// </summary>
        public bool hasCommentInScript { get; set; } = false;
    }
}
