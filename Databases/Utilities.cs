// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using SQLGen.Utilities;

namespace SQLGen.Utilities
{
    // -------------------------------------------------------------------------------------------------------
    /// <summary>Типы скриптов</summary>
    public enum ScriptType
    {
        /// <summary>
        /// создание таблицы CREATE TABLE
        /// </summary>
        CREATE,
        /// <summary>
        /// изменение таблицы ALTER TABLE
        /// </summary>
        ALTER,
        /// <summary>
        /// добавление данных в таблицу INSERT INTO
        /// </summary>
        INSERT,
        /// <summary>
        /// добавление и обновление данных в таблице INSERT + UPDATE (или MERGE)
        /// </summary>
        UPSERT,
        /// <summary>
        /// добавление и обновление данных в таблице INSERT + UPDATE (или MERGE) - через временную таблицу
        /// </summary>
        UPSERT_TMP,
        /// <summary>
        /// обновление данных в таблице UPDATE
        /// </summary>
        UPDATE,
        /// <summary>
        /// удаление данных в таблице DELETE FROM
        /// </summary>
        DELETE,
        /// <summary>
        /// удаление таблицы DROP TABLE
        /// </summary>
        DROP,
        /// <summary>
        /// добавление данных в таблицу пакетами INSERT ... FROM VALUES
        /// </summary>
        INSERT_VALUES,
        /// <summary>
        /// добавление данных в таблицу - через временную таблицу
        /// </summary>
        INSERT_TMP,
        /// <summary>
        /// добавление данных в таблицу BULK\COPY
        /// </summary>
        INSERT_BULK_TABLE,
        /// <summary>
        /// добавление данных в таблицу BULK\COPY через представление
        /// </summary>
        INSERT_BULK_VIEW,
        /// <summary>
        /// шаблон скрипта
        /// </summary>
        SHABLON
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>Базовые типы скриптов</summary>
    public enum BaseScriptType
    {
        /// <summary>
        /// изменение структуры
        /// </summary>
        ALTER,
        /// <summary>
        /// изменение данных
        /// </summary>
        DATA
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>Целевые БД</summary>
    public enum TargetDBType
    {
        /// <summary>
        /// MS SQL
        /// </summary>
        MSSQL,
        /// <summary>
        /// Postgres
        /// </summary>
        PGSQL,
        /// <summary>
        /// Postgres c особенностями схемы EMD
        /// </summary>
        EMD
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>Типы таблиц</summary>
    public enum TableType
    {
        /// <summary>
        /// Справочник (по умолчанию)
        /// </summary>
        DICT,
        /// <summary>
        /// Событие Evn
        /// </summary>
        EVN,
        /// <summary>
        /// Периодика пациента PersonEvn
        /// </summary>
        PERSONEVN,
        /// <summary>
        /// Заболевание Morbus
        /// </summary>
        MORBUS,
        /// <summary>
        /// Временная таблица
        /// </summary>
        TEMP
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>Типы соединения</summary>
    public enum ConnType
    {
        /// <summary>
        /// Соединение не установлено
        /// </summary>
        None,
        /// <summary>
        /// MS SQL
        /// </summary>
        MSSQL,
        /// <summary>
        /// Postgres
        /// </summary>
        PGSQL,
        /// <summary>
        /// dBase
        /// </summary>
        DBF
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>Типы авторизации</summary>
    public enum AuthType
    {
        /// <summary>
        /// авторизация Active Directory
        /// </summary>
        WINDOWS,
        /// <summary>
        /// авторизация СУБД
        /// </summary>
        DATABASE
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>Базовые типы полей</summary>
    public enum GeneralType
    {
        /// <summary>
        /// Неизвестный тип
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// Строковый
        /// </summary>
        STRING,
        /// <summary>
        /// Числовой
        /// </summary>
        NUMBER,
        /// <summary>
        /// Дата\время
        /// </summary>
        DATETIME,
        /// <summary>
        /// Логический
        /// </summary>
        BOOLEAN
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>Типы обновления даты в полях insDT/updDT</summary>
    public enum InsUpdDTType
    {
        /// <summary>
        /// без изменения
        /// </summary>
        NONE,
        /// <summary>
        /// getdate() или localtimestamp
        /// </summary>
        GETDATE,
        /// <summary>
        /// через переменную
        /// </summary>
        VARI
    }

    /// <summary>
    /// Вспомогательные функции для работы с базами данных
    /// </summary>
    public static class Databases
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>регулярное выражение для региональной схемы</summary>
        public static Regex regex_region = new Regex(@"^r(\d+)");

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Проверяет наличие схемы в имени таблицы и добавляет схему по умолчанию</summary>
        /// <param name="tablename">Имя таблицы</param>
        /// <param name="defaultschema">Схема по умолчанию</param>
        public static string GetFullTableName(string tablename, string defaultschema = "dbo")
        {
            if (string.IsNullOrWhiteSpace(tablename)) return "";

            var arr = tablename.Trim().Split('.');
            if (arr.Length < 2) return defaultschema.Trim() + "." + tablename.Trim();
            else return tablename.Trim();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выделяет имя таблицы</summary>
        /// <param name="tablename">Имя таблицы</param>
        /// <param name="defaultschema">Схема по умолчанию</param>
        public static string GetTableName(string tablename, string defaultschema = "dbo")
        {
            if (string.IsNullOrWhiteSpace(tablename)) return "";

            tablename = GetFullTableName(tablename, defaultschema);
            var arr = tablename.Trim().Split('.');
            return arr[1].Trim();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выделяет схему таблицы</summary>
        /// <param name="tablename">Имя таблицы</param>
        /// <param name="defaultschema">Схема по умолчанию</param>
        public static string GetSchemaName(string tablename, string defaultschema = "dbo")
        {
            if (string.IsNullOrWhiteSpace(tablename)) return defaultschema;

            tablename = GetFullTableName(tablename, defaultschema);
            var arr = tablename.Trim().Split('.');
            return arr[0].Trim();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Проверяет, что все поля из pk есть в keys</summary>
        /// <param name="keys">Список уникальных полей через запятую</param>
        /// <param name="pk">Список полей primary key через запятую</param>
        public static bool IsPK(string keys, string pk)
        {
            keys = keys.ToLower().Replace("\"", "").Replace(';', ',').Trim();
            pk = pk.ToLower().Replace("\"", "").Replace(';', ',').Trim();

            var keys_arr = keys.Split(',');
            var pk_arr = pk.Split(',');

            int cnt = 0;

            foreach (var pk_item in pk_arr)
            {
                foreach (var key_item in keys_arr)
                {
                    if (key_item.Trim() == pk_item.Trim())
                    {
                        cnt += 1;
                    }
                }
            }
            return cnt == pk_arr.Length; // если все поля в pk есть в keys
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Получим список локальных справочников из всех тестовых БД
        /// </summary>
        /// <param name="connect">подключение к БД</param>
        /// <param name="tables">список таблиц</param>
        /// <param name="query">запрос</param>
        /// <param name="localerror">информация об отсутствии локальных справочниках в БД</param>
        /// <returns></returns>
        public static List<string> GetAllLocalDBList(ConnectDB connect, List<string> tables, out string query, out string localerror)
        {
            query = "";
            localerror = "";

            bool isPromeddevMS = false;
            bool isPromedtestMS = false;
            bool isPromedufaMS = false;
            bool isPromedwebreleaseMS = false;
            bool isPromedwebufareleaseMS = false;
            bool isPromedtestPG = false;
            bool isPromedadygeaPG = false;
            bool isPromedreleasePG = false;

            if (tables == null) return new List<string>();

            if (
                (connect != null) &&  //-V3063
                connect.isNotConnected
            )
            {
                // открываем текущее соединение
                try
                {
                    connect.OpenConnect(false);
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            string lastconn = "";
            List<string> list_tables = new List<string>();
            List<LocalInDBInstance> result = new List<LocalInDBInstance>();

            // отберем только таблицы-справочники
            foreach (var item in tables.Distinct())
            {
                try
                {
                    if (
                        (connect != null) &&  //-V3063
                        connect.isConnected
                    )
                    {
                        var tabletype = connect.GetTableType(new TableInfo(new TableDB() { GITProject = connect.GITProject }) { SchemaName = Utilities.Databases.GetSchemaName(item), TableName = Utilities.Databases.GetTableName(item) });

                        if (tabletype ==  Utilities.TableType.DICT)
                        {
                            list_tables.Add(item);
                        }
                    }
                    else
                    {
                        list_tables.Add(item);
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                    list_tables.Add(item);
                }
            }

            if (list_tables.Count() == 0) return new List<string>();

            // получаем список локальных справочников из текущей БД
            if (
                (connect != null) &&  //-V3063
                connect.isConnected
            )
            {
                if (connect.InstanceDBType == InstanceDBType.PROMEDDEV_MS) isPromeddevMS = true;
                if (connect.InstanceDBType == InstanceDBType.PROMEDTEST_MS) isPromedtestMS = true;
                if (connect.InstanceDBType == InstanceDBType.PROMEDUFA_MS) isPromedufaMS = true;
                if (connect.InstanceDBType == InstanceDBType.PROMEDWEBRELEASE_MS) isPromedwebreleaseMS = true;
                if (connect.InstanceDBType == InstanceDBType.PROMEDWEBUFARELEASE_MS) isPromedwebufareleaseMS = true;
                if (connect.InstanceDBType == InstanceDBType.PROMEDTEST_PG) isPromedtestPG = true;
                if (connect.InstanceDBType == InstanceDBType.PROMEDADYGEA_PG) isPromedadygeaPG = true;
                if (connect.InstanceDBType == InstanceDBType.PROMEDRELEASE_PG) isPromedreleasePG = true;

                try
                {
                    foreach (DataRow row in connect.GetLocalDBList("promed", list_tables, out query, out string _message).Rows)
                    {
                        string s = "MODULE=" + row[6].ToString() + ", SCHEMA=" + row[4].ToString() + ", NAME=" + row[1].ToString();
                        var found = result.Find(x => x.Key == s);

                        if (found == null)
                        {
                            found = new LocalInDBInstance() { Key = s, Name = row[1].ToString() };
                            result.Add(found);
                        }

                        if (connect.InstanceDBType == InstanceDBType.PROMEDDEV_MS) found.PromeddevMS = LocalInDB.LOCAL_EXIST;
                        if (connect.InstanceDBType == InstanceDBType.PROMEDTEST_MS) found.PromedtestMS = LocalInDB.LOCAL_EXIST;
                        if (connect.InstanceDBType == InstanceDBType.PROMEDUFA_MS) found.PromedufaMS = LocalInDB.LOCAL_EXIST;
                        if (connect.InstanceDBType == InstanceDBType.PROMEDWEBRELEASE_MS) found.PromedwebreleaseMS = LocalInDB.LOCAL_EXIST;
                        if (connect.InstanceDBType == InstanceDBType.PROMEDWEBUFARELEASE_MS) found.PromedwebufareleaseMS = LocalInDB.LOCAL_EXIST;
                        if (connect.InstanceDBType == InstanceDBType.PROMEDTEST_PG) found.PromedtestPG = LocalInDB.LOCAL_EXIST;
                        if (connect.InstanceDBType == InstanceDBType.PROMEDADYGEA_PG) found.PromedadygeaPG = LocalInDB.LOCAL_EXIST;
                        if (connect.InstanceDBType == InstanceDBType.PROMEDRELEASE_PG) found.PromedreleasePG = LocalInDB.LOCAL_EXIST;
                    }
                    lastconn = connect.DBConnectionName;
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                }
                connect.CloseConnect();
            }

            if (System.Windows.Forms.MessageBox.Show($"Проверим наличие локальных справочников на тестовых и релизных БД?",
               "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                // добавляем с других тестовых
                connect = MainWindow.GetConnectByGITProject("dev_promed_ms", "promeddev", true);
                if (
                    (connect != null) &&
                    (connect.DBConnectionName != lastconn)
                )
                {
                    connect.OpenConnect(false);
                    if (connect.isConnected)
                    {
                        if (connect.InstanceDBType == InstanceDBType.PROMEDDEV_MS) isPromeddevMS = true;

                        try
                        {
                            foreach (DataRow row in connect.GetLocalDBList("promed", list_tables, out query, out string _message).Rows)
                            {
                                string s = "MODULE=" + row[6].ToString() + ", SCHEMA=" + row[4].ToString() + ", NAME=" + row[1].ToString();
                                var found = result.Find(x => x.Key == s);

                                if (found == null)
                                {
                                    found = new LocalInDBInstance() { Key = s, Name = row[1].ToString() };
                                    result.Add(found);
                                }

                                if (connect.InstanceDBType == InstanceDBType.PROMEDDEV_MS) found.PromeddevMS = LocalInDB.LOCAL_EXIST;
                            }
                            lastconn = connect.DBConnectionName;
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }
                        connect.CloseConnect();
                    }
                }

                connect = MainWindow.GetConnectByGITProject("dev_promed_ms", "promedtest", true);
                if (
                    (connect != null) &&
                    (connect.DBConnectionName != lastconn)
                )
                {
                    connect.OpenConnect(false);
                    if (connect.isConnected)
                    {
                        if (connect.InstanceDBType == InstanceDBType.PROMEDTEST_MS) isPromedtestMS = true;

                        try
                        {
                            foreach (DataRow row in connect.GetLocalDBList("promed", list_tables, out query, out string _message).Rows)
                            {
                                string s = "MODULE=" + row[6].ToString() + ", SCHEMA=" + row[4].ToString() + ", NAME=" + row[1].ToString();
                                var found = result.Find(x => x.Key == s);

                                if (found == null)
                                {
                                    found = new LocalInDBInstance() { Key = s, Name = row[1].ToString() };
                                    result.Add(found);
                                }

                                if (connect.InstanceDBType == InstanceDBType.PROMEDTEST_MS) found.PromedtestMS = LocalInDB.LOCAL_EXIST;
                            }
                            lastconn = connect.DBConnectionName;
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }
                        connect.CloseConnect();
                    }
                }

                connect = MainWindow.GetConnectByGITProject("dev_promed_ms", "promedufa", true);
                if (
                    (connect != null) &&
                    (connect.DBConnectionName != lastconn)
                )
                {
                    connect.OpenConnect(false);
                    if (connect.isConnected)
                    {
                        if (connect.InstanceDBType == InstanceDBType.PROMEDUFA_MS) isPromedufaMS = true;

                        try
                        {
                            foreach (DataRow row in connect.GetLocalDBList("promed", list_tables, out query, out string _message).Rows)
                            {
                                string s = "MODULE=" + row[6].ToString() + ", SCHEMA=" + row[4].ToString() + ", NAME=" + row[1].ToString();
                                var found = result.Find(x => x.Key == s);

                                if (found == null)
                                {
                                    found = new LocalInDBInstance() { Key = s, Name = row[1].ToString() };
                                    result.Add(found);
                                }

                                if (connect.InstanceDBType == InstanceDBType.PROMEDUFA_MS) found.PromedufaMS = LocalInDB.LOCAL_EXIST;
                            }
                            lastconn = connect.DBConnectionName;
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }
                        connect.CloseConnect();
                    }
                }

                connect = MainWindow.GetConnectByGITProject("dev_promed_ms", "promedwebrelease", false, false, "RELEASE");
                if (
                    (connect != null) &&
                    (connect.DBConnectionName != lastconn)
                )
                {
                    connect.OpenConnect(false);
                    if (connect.isConnected)
                    {
                        if (connect.InstanceDBType == InstanceDBType.PROMEDWEBRELEASE_MS) isPromedwebreleaseMS = true;

                        try
                        {
                            foreach (DataRow row in connect.GetLocalDBList("promed", list_tables, out query, out string _message).Rows)
                            {
                                string s = "MODULE=" + row[6].ToString() + ", SCHEMA=" + row[4].ToString() + ", NAME=" + row[1].ToString();
                                var found = result.Find(x => x.Key == s);

                                if (found == null)
                                {
                                    found = new LocalInDBInstance() { Key = s, Name = row[1].ToString() };
                                    result.Add(found);
                                }

                                if (connect.InstanceDBType == InstanceDBType.PROMEDWEBRELEASE_MS) found.PromedwebreleaseMS = LocalInDB.LOCAL_EXIST;
                            }
                            lastconn = connect.DBConnectionName;
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }
                        connect.CloseConnect();
                    }
                }

                connect = MainWindow.GetConnectByGITProject("dev_promed_ms", "promedwebufarelease",false, false, "RELEASE");
                if (
                    (connect != null) &&
                    (connect.DBConnectionName != lastconn)
                )
                {
                    connect.OpenConnect(false);
                    if (connect.isConnected)
                    {
                        if (connect.InstanceDBType == InstanceDBType.PROMEDWEBUFARELEASE_MS) isPromedwebufareleaseMS = true;

                        try
                        {
                            foreach (DataRow row in connect.GetLocalDBList("promed", list_tables, out query, out string _message).Rows)
                            {
                                string s = "MODULE=" + row[6].ToString() + ", SCHEMA=" + row[4].ToString() + ", NAME=" + row[1].ToString();
                                var found = result.Find(x => x.Key == s);

                                if (found == null)
                                {
                                    found = new LocalInDBInstance() { Key = s, Name = row[1].ToString() };
                                    result.Add(found);
                                }

                                if (connect.InstanceDBType == InstanceDBType.PROMEDWEBUFARELEASE_MS) found.PromedwebufareleaseMS = LocalInDB.LOCAL_EXIST;
                            }
                            lastconn = connect.DBConnectionName;
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }
                        connect.CloseConnect();
                    }
                }

                connect = MainWindow.GetConnectByGITProject("dev_promed_pg", "promedtest", true);
                if (
                    (connect != null) &&
                    (connect.DBConnectionName != lastconn)
                )
                {
                    connect.OpenConnect(false);
                    if (connect.isConnected)
                    {
                        if (connect.InstanceDBType == InstanceDBType.PROMEDTEST_PG) isPromedtestPG = true;

                        try
                        {
                            foreach (DataRow row in connect.GetLocalDBList("promed", list_tables, out query, out string _message).Rows)
                            {
                                string s = "MODULE=" + row[6].ToString() + ", SCHEMA=" + row[4].ToString() + ", NAME=" + row[1].ToString();
                                var found = result.Find(x => x.Key == s);

                                if (found == null)
                                {
                                    found = new LocalInDBInstance() { Key = s, Name = row[1].ToString() };
                                    result.Add(found);
                                }

                                if (connect.InstanceDBType == InstanceDBType.PROMEDTEST_PG) found.PromedtestPG = LocalInDB.LOCAL_EXIST;
                            }
                            lastconn = connect.DBConnectionName; //-V3137
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }
                        connect.CloseConnect();
                    }
                }

                connect = MainWindow.GetConnectByGITProject("dev_promed_pg", "promedadygea", true);
                if (
                    (connect != null) &&
                    (connect.DBConnectionName != lastconn)
                )
                {
                    connect.OpenConnect(false);
                    if (connect.isConnected)
                    {
                        if (connect.InstanceDBType == InstanceDBType.PROMEDADYGEA_PG) isPromedadygeaPG = true;

                        try
                        {
                            foreach (DataRow row in connect.GetLocalDBList("promed", list_tables, out query, out string _message).Rows)
                            {
                                string s = "MODULE=" + row[6].ToString() + ", SCHEMA=" + row[4].ToString() + ", NAME=" + row[1].ToString();
                                var found = result.Find(x => x.Key == s);

                                if (found == null)
                                {
                                    found = new LocalInDBInstance() { Key = s, Name = row[1].ToString() };
                                    result.Add(found);
                                }

                                if (connect.InstanceDBType == InstanceDBType.PROMEDADYGEA_PG) found.PromedadygeaPG = LocalInDB.LOCAL_EXIST;
                            }
                            lastconn = connect.DBConnectionName;
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }
                        connect.CloseConnect();
                    }
                }

                connect = MainWindow.GetConnectByGITProject("dev_promed_pg", "promedrelease", false, false, "RELEASE");
                if (
                    (connect != null) &&
                    (connect.DBConnectionName != lastconn)
                )
                {
                    connect.OpenConnect(false);
                    if (connect.isConnected)
                    {
                        if (connect.InstanceDBType == InstanceDBType.PROMEDRELEASE_PG) isPromedreleasePG = true;

                        try
                        {
                            foreach (DataRow row in connect.GetLocalDBList("promed", list_tables, out query, out string _message).Rows)
                            {
                                string s = "MODULE=" + row[6].ToString() + ", SCHEMA=" + row[4].ToString() + ", NAME=" + row[1].ToString();
                                var found = result.Find(x => x.Key == s);

                                if (found == null)
                                {
                                    found = new LocalInDBInstance() { Key = s, Name = row[1].ToString() };
                                    result.Add(found);
                                }

                                if (connect.InstanceDBType == InstanceDBType.PROMEDRELEASE_PG) found.PromedreleasePG = LocalInDB.LOCAL_EXIST;
                            }
                            lastconn = connect.DBConnectionName; //-V3137
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }
                        connect.CloseConnect();
                    }
                }
            }

            foreach (var item in result)
            {
                if ((
                    item.PromeddevMS == LocalInDB.LOCAL_EXIST ||
                    item.PromedtestMS == LocalInDB.LOCAL_EXIST ||
                    item.PromedufaMS == LocalInDB.LOCAL_EXIST ||
                    item.PromedwebreleaseMS == LocalInDB.LOCAL_EXIST ||
                    item.PromedwebufareleaseMS == LocalInDB.LOCAL_EXIST ||
                    item.PromedtestPG == LocalInDB.LOCAL_EXIST ||
                    item.PromedadygeaPG == LocalInDB.LOCAL_EXIST ||
                    item.PromedreleasePG == LocalInDB.LOCAL_EXIST
                ) &&
                (
                    isPromeddevMS && (item.PromeddevMS == LocalInDB.LOCAL_NOT_EXIST) ||
                    isPromedtestMS && (item.PromedtestMS == LocalInDB.LOCAL_NOT_EXIST) ||
                    isPromedufaMS && (item.PromedufaMS == LocalInDB.LOCAL_NOT_EXIST) ||
                    isPromedwebreleaseMS && (item.PromedwebreleaseMS == LocalInDB.LOCAL_NOT_EXIST) ||
                    isPromedwebufareleaseMS && (item.PromedwebufareleaseMS == LocalInDB.LOCAL_NOT_EXIST) ||
                    isPromedtestPG && (item.PromedtestPG == LocalInDB.LOCAL_NOT_EXIST) ||
                    isPromedadygeaPG && (item.PromedadygeaPG == LocalInDB.LOCAL_NOT_EXIST) ||
                    isPromedreleasePG && (item.PromedreleasePG == LocalInDB.LOCAL_NOT_EXIST)
                ))
                {
                    localerror += Environment.NewLine + item.Name + ": отсутствует в";
                    if (isPromeddevMS && (item.PromeddevMS == LocalInDB.LOCAL_NOT_EXIST)) localerror += " promeddev(тестовая MS)";
                    if (isPromedtestMS && (item.PromedtestMS == LocalInDB.LOCAL_NOT_EXIST)) localerror += " promedtest(тестовая MS)";
                    if (isPromedufaMS && (item.PromedufaMS == LocalInDB.LOCAL_NOT_EXIST)) localerror += " promedufa(тестовая MS)";
                    if (isPromedwebreleaseMS && (item.PromedwebreleaseMS == LocalInDB.LOCAL_NOT_EXIST)) localerror += " promedwebrelease(релизная MS)";
                    if (isPromedwebufareleaseMS && (item.PromedwebufareleaseMS == LocalInDB.LOCAL_NOT_EXIST)) localerror += " promedwebufarelease(релизная MS)";
                    if (isPromedtestPG && (item.PromedtestPG == LocalInDB.LOCAL_NOT_EXIST)) localerror += " promedtest(тестовая PG)";
                    if (isPromedadygeaPG && (item.PromedadygeaPG == LocalInDB.LOCAL_NOT_EXIST)) localerror += " promedadygea(тестовая PG)";
                    if (isPromedreleasePG && (item.PromedreleasePG == LocalInDB.LOCAL_NOT_EXIST)) localerror += " promedrelease(релизная PG)";
                }
            }

            return result.Where(x =>
                x.PromeddevMS == LocalInDB.LOCAL_EXIST ||
                x.PromedtestMS == LocalInDB.LOCAL_EXIST ||
                x.PromedufaMS == LocalInDB.LOCAL_EXIST ||
                x.PromedwebreleaseMS == LocalInDB.LOCAL_EXIST ||
                x.PromedwebufareleaseMS == LocalInDB.LOCAL_EXIST ||
                x.PromedtestPG == LocalInDB.LOCAL_EXIST ||
                x.PromedadygeaPG == LocalInDB.LOCAL_EXIST ||
                x.PromedreleasePG == LocalInDB.LOCAL_EXIST
            ).Select(x => x.Key).ToList();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выбрать локальный справочник
        /// </summary>
        /// <param name="cbConnectSQL">ComboBox с подключением к БД</param>
        /// <param name="project">проект</param>
        /// <param name="fulltable">таблица</param>
        /// <returns></returns>
        public static List<string> ChooseLocalDBList(ComboBox cbConnectSQL, string project, string fulltable)
        {
            // Получить список локальных справочников
            List<string> listLocal = new List<string>();

            string dev_project = Utilities.GITProjects.GetDEVProject(project);

            if (
                (dev_project == "dev_promed_pg") ||
                (dev_project == "dev_promed_ms")
            )
            {
                FormCheckedListBox dlg1 = new FormCheckedListBox();
                dlg1.Text = "Выбрать локальные справочники из stg.LocalDBList для включения в скрипт";
                dlg1.clbList.Items.Clear();

                var connect = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, false);
                dlg1.clbList.Items.AddRange(GetAllLocalDBList(connect, new List<string> { fulltable }, out string _query, out string _localerror).ToArray());
                dlg1.SetAll();

                if (
                    (dlg1.clbList.Items.Count > 0) &&
                    (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                )
                {
                    foreach (object itemChecked in dlg1.clbList.CheckedItems)
                    {
                        string s = "";
                        foreach (var item in itemChecked.ToString().ToList(new char[] { ',' }, true))
                        {
                            string value = KeyWord.KeyValue(item, "NAME", new char[] { '=' });
                            if (!string.IsNullOrEmpty(value))
                            {
                                s = value;
                                break;
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(s))
                        {
                            var found = listLocal.Where(x => x.ToLower() == s.ToLower()).FirstOrDefault();
                            if (string.IsNullOrWhiteSpace(found))
                            {
                                listLocal.Add(s);
                            }
                        }
                    }
                }
                dlg1.Dispose();
            }

            return listLocal;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Подготовить скрипт с текстом объекта (таблица, хранимка, вьюха)
        /// </summary>
        /// <param name="isOriginal">=true - оригинальный скрипт</param>
        /// <param name="Connect">подключение к БД</param>
        /// <param name="schema">схема</param>
        /// <param name="objectname">объект</param>
        /// <param name="schemaseek">возвращается схема - для поиска</param>
        /// <param name="objectseek">возвращается имя таблицы - для поиска</param>
        /// <param name="isAddRegion">=true - добавить условие региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="scripttype">возвращается тип объекта</param>
        /// <param name="for_tablename">для какой таблицы (например если объект это индекс)</param>
        /// <param name="isIndexCreate">=true - генерировать индекс командой CREATE INDEX</param>
        /// <param name="error">ошибки</param>
        /// <param name="isInit">=true - первоначальное наполнение проекта GIT</param>
        /// <param name="project">проект GIT</param>
        /// <param name="isAddinExistFile">=true - добавление в существующий файл</param>
        /// <param name="isAddTestChangeset">=true - добавить тестовый changeset</param>
        /// <returns></returns>
        public static string GenerateProcText(bool isOriginal, ConnectDB Connect, ref string schema, ref string objectname, ref string schemaseek, ref string objectseek, bool isAddRegion, string txtRegion, ref string scripttype, ref string for_tablename, bool isIndexCreate, out string error, bool isInit, string project, bool isAddinExistFile, bool isAddTestChangeset)
        {
            string text = "";
            string full_text = "";
            string objecttype = "";
            for_tablename = "";
            error = "";
            scripttype = scripttype.ToLower();

            if (string.IsNullOrWhiteSpace(objectname)) return full_text;
            if ((Connect == null) || Connect.isNotConnected) return full_text;

            // определяем список дублей
            List<DBObjectOID> doubles = Connect.GetObjectOIDList(schema, objectname);
            if (doubles.Count() == 0)
            {
                doubles.Add(new DBObjectOID { Schema = schema, Name = objectname, OID = "" });
            }

            full_text = "";

            foreach (var item in doubles)
            {
                text = "";

                if (!isAddinExistFile)
                {
                    isAddinExistFile = !string.IsNullOrWhiteSpace(full_text);
                }

                schema = item.Schema;
                objectname = item.Name;

                List<string> lines = Connect.GetProcText(ref schema, ref objectname, ref schemaseek, ref objectseek, isAddRegion, txtRegion, ref scripttype, ref objecttype, ref for_tablename, project, item.OID, isIndexCreate);

                // собираем в одну строку
                string original_text = "";
                foreach (var line in lines)
                {
                    original_text += line;
                    if (!line.EndsWith(Environment.NewLine))
                    {
                        original_text += Environment.NewLine;
                    }
                }

                if (!isOriginal)
                {
                    // форматируем
                    /*if (isFormat == true)
                    {
                        string err = "";
                        original_text = Utilities.FormatProcText(original_text, schema, objectname, schemaseek, objectseek, Connect.DBType, ref isFormat, out err);
                        if (!string.IsNullOrWhiteSpace(err))
                        {
                            if (!string.IsNullOrWhiteSpace(error)) error += Environment.NewLine; //-V3022
                            error += err;
                        }
                    }*/

                    // убираем лишние переводы строки в конце 
                    original_text = original_text.TrimEndNewLine();

                    // добавляем обязательную обвязку скрипта
                    if (
                            ((scripttype == "view") || (scripttype == "procedure") || (scripttype == "function")) &&
                            (Connect.ConnType == ConnType.MSSQL) &&
                            (objecttype != "") &&
                            (lines.Count > 0)
                        )
                    {
                        text += Environment.NewLine;

                        if (isAddRegion)
                        {
                            text += "IF (dbo.getregion() = " + txtRegion.Trim() + " OR db_name() IN ('ProMedWebRelease', 'ProMedTest', 'promeddev'))";
                            text += Environment.NewLine + "AND EXISTS (SELECT 1 FROM sys.schemas s WITH (nolock) WHERE s.name = '" + schemaseek + "')";
                            text += Environment.NewLine + "BEGIN" + Environment.NewLine + Environment.NewLine;
                        }

                        text += "IF OBJECT_ID(N'" + schema + "." + objectname + "', N'" + objecttype + "') IS NOT NULL";

                        if (isAddRegion)
                        {
                            text += Environment.NewLine + "\tDROP " + scripttype.ToUpper() + " " + schema + "." + objectname;
                            text += Environment.NewLine;
                            text += Environment.NewLine + "IF OBJECT_ID(N'" + schema + "." + objectname + "', N'" + objecttype + "') IS NULL";
                            text += Environment.NewLine + "\tEXECUTE('";
                        }
                        else
                        {
                            text += Environment.NewLine + "\tDROP " + scripttype.ToUpper() + " " + schema + "." + objectname;
                            text += Environment.NewLine + "GO";
                        }

                        text += Environment.NewLine;

                        if (isAddRegion)
                        {
                            text += original_text.Replace("'", "''");
                            text += Environment.NewLine + "')";
                            text += Environment.NewLine + Environment.NewLine + "END";
                        }
                        else
                        {
                            text += original_text;
                        }

                        text += Environment.NewLine;

                        // добавляем в конце Environment.NewLine + GO + Environment.NewLine, если его нет
                        text = AddEndGO(text);
                    }

                    if (
                            (scripttype == "table") &&
                            (Connect.ConnType == ConnType.MSSQL) &&
                            (objecttype != "") &&
                            (lines.Count > 0)
                        )
                    {
                        text += Environment.NewLine + original_text + Environment.NewLine;
                        // добавляем в конце Environment.NewLine + GO + Environment.NewLine, если его нет
                        text = AddEndGO(text);
                    }

                    if (
                            (scripttype == "index") &&
                            (Connect.ConnType == ConnType.MSSQL) &&
                            (objecttype != "") &&
                            (!string.IsNullOrWhiteSpace(for_tablename)) &&
                            (lines.Count > 0)
                        )
                    {
                        text += Environment.NewLine + original_text + Environment.NewLine;
                        // добавляем в конце Environment.NewLine + GO + Environment.NewLine, если его нет
                        text = AddEndGO(text);
                    }

                    if (
                            ((scripttype == "view") || (scripttype == "materialized view")) &&
                            (Connect.ConnType == ConnType.PGSQL) &&
                            (lines.Count > 0)
                        )
                    {
                        string body = "";

                        if (isAddRegion)
                        {
                            text += Environment.NewLine + "DO $script$";
                            text += Environment.NewLine + "BEGIN";
                            text += Environment.NewLine + Environment.NewLine + "IF (dbo.getregion() = " + txtRegion + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))";
                            text += Environment.NewLine + "AND EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '" + schemaseek + "')";
                            text += Environment.NewLine + "THEN" + Environment.NewLine;
                            text += Environment.NewLine + "EXECUTE $reg$" + Environment.NewLine;
                        }
                        else body += Environment.NewLine;

                        body +=
                            "SELECT dbo.xp_gen_view('" + schema + "." + objectname + "'," + Environment.NewLine +
                            "$viewtext$" + Environment.NewLine +
                            original_text/*.Replace("'", "''")*/ + Environment.NewLine +
                            "$viewtext$" + Environment.NewLine +
                            ",2);";

                        if (isAddRegion)
                        {
                            text += body/*.Replace("'", "''")*/;
                            text += Environment.NewLine + "$reg$;" + Environment.NewLine;
                            text += Environment.NewLine + "END IF;" + Environment.NewLine;
                            text += Environment.NewLine + "END;" + Environment.NewLine + "$script$;";
                        }
                        else
                        {
                            text += body;
                        }

                        text += Environment.NewLine;
                    }

                    if (
                            ((scripttype == "procedure") || (scripttype == "function")) &&
                            (Connect.ConnType == ConnType.PGSQL) &&
                            (lines.Count > 0)
                        )
                    {
                        string body = "";

                        if (isAddRegion)
                        {
                            text += Environment.NewLine + "DO $script$";
                            text += Environment.NewLine + "BEGIN";
                            text += Environment.NewLine + Environment.NewLine + "IF (dbo.getregion() = " + txtRegion + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))";
                            text += Environment.NewLine + "AND EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '" + schemaseek + "')";
                            text += Environment.NewLine + "THEN" + Environment.NewLine;
                            text += Environment.NewLine + "EXECUTE $reg$" + Environment.NewLine;
                        }
                        else body += Environment.NewLine;

                        if (
                            original_text.ToLower().Contains("language c") ||
                            isAddinExistFile
                            )
                        {
                            body += original_text;
                        }
                        else
                        {
                            body +=
                                "SELECT dbo.xp_dropfns('" + schema.Replace("\"", "") + "." + objectname.Replace("\"", "") + "');" + Environment.NewLine +
                                Environment.NewLine + original_text;
                        }

                        if (isAddRegion)
                        {
                            text += body/*.Replace("'", "''")*/;
                            text += Environment.NewLine + "$reg$;" + Environment.NewLine;
                            text += Environment.NewLine + "END IF;" + Environment.NewLine;
                            text += Environment.NewLine + "END;" + Environment.NewLine + "$script$;";
                        }
                        else
                        {
                            text += body;
                        }

                        text += Environment.NewLine;
                    }

                    if (
                            (scripttype == "table") &&
                            (Connect.ConnType == ConnType.PGSQL) &&
                            (lines.Count > 0)
                        )
                    {
                        text += Environment.NewLine + original_text + Environment.NewLine;
                    }


                    if (
                            (scripttype == "index") &&
                            (Connect.ConnType == ConnType.PGSQL) &&
                            (!string.IsNullOrWhiteSpace(for_tablename)) &&
                            (lines.Count > 0)
                        )
                    {
                        text += Environment.NewLine + original_text + Environment.NewLine;
                    }

                    if (
                            (scripttype == "trigger") &&
                            (Connect.ConnType == ConnType.PGSQL) &&
                            (!string.IsNullOrWhiteSpace(for_tablename)) &&
                            (lines.Count > 0)
                        )
                    {
                        text += Environment.NewLine;

                        if (isAddRegion)
                        {
                            text += Environment.NewLine + "DO $script$";
                            text += Environment.NewLine + "BEGIN";
                            text += Environment.NewLine + Environment.NewLine + "IF (dbo.getregion() = " + txtRegion + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))";
                            text += Environment.NewLine + "AND EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '" + schemaseek + "')";
                            text += Environment.NewLine + "THEN" + Environment.NewLine + Environment.NewLine;
                        };

                        text += "DROP TRIGGER IF EXISTS " + objectname + " ON " + schema + "." + for_tablename + ";" + Environment.NewLine +
                            Environment.NewLine +
                            original_text;

                        if (isAddRegion)
                        {
                            text += Environment.NewLine + "END IF;" + Environment.NewLine;
                            text += Environment.NewLine + "END;" + Environment.NewLine + "$script$;";
                        }

                        text += Environment.NewLine;
                    }

                    if (
                            (scripttype == "sequence") &&
                            (Connect.ConnType == ConnType.PGSQL) &&
                            (lines.Count > 0)
                        )
                    {
                        text += Environment.NewLine;

                        if (isAddRegion)
                        {
                            text += Environment.NewLine + "DO $script$";
                            text += Environment.NewLine + "BEGIN";
                            text += Environment.NewLine + Environment.NewLine + "IF (dbo.getregion() = " + txtRegion + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))";
                            text += Environment.NewLine + "AND EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '" + schemaseek + "')";
                            text += Environment.NewLine + "THEN" + Environment.NewLine + Environment.NewLine;
                        };

                        text += original_text;

                        if (isAddRegion)
                        {
                            text += Environment.NewLine + "END IF;" + Environment.NewLine;
                            text += Environment.NewLine + "END;" + Environment.NewLine + "$script$;";
                        }

                        text += Environment.NewLine;
                    }

                    string title = "";
                    if (scripttype == "materialized view")
                    {
                        title = MainWindow.Task.TitleScript(Connect.TargetDBType, "view", isInit, isAddinExistFile);
                    }
                    else
                    {
                        title = MainWindow.Task.TitleScript(Connect.TargetDBType, scripttype, isInit, isAddinExistFile);
                    }

                    // убираем лишние переводы строки в конце, оставляем один
                    text = text.TrimEndNewLine(Environment.NewLine);

                    text = title + text;

                    // добавляем тестовый changeset
                    if (
                        isAddTestChangeset &&
                        ((scripttype == "procedure") || (scripttype == "function"))
                    )
                    {
                        string testchangeset = Connect.GetTestChangeset(schemaseek, objectseek, isAddRegion, txtRegion);

                        if (!string.IsNullOrWhiteSpace(testchangeset))
                        {
                            text += Environment.NewLine + testchangeset;

                            // убираем лишние переводы строки в конце, оставляем один
                            text = text.TrimEndNewLine(Environment.NewLine);
                        }
                    }

                }
                else
                {
                    text = original_text;
                }

                if (string.IsNullOrWhiteSpace(full_text))
                {
                    full_text = text;
                }
                else
                {
                    full_text += Environment.NewLine + text;
                }
            }
            return full_text;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// =true - имя таблицы корректное
        /// </summary>
        /// <param name="name">имя таблицы</param>
        /// <returns></returns>
        public static bool IsTableNameCorrect(string name) =>
            string.IsNullOrWhiteSpace(name) ||
            name.IsMatch(@"^[a-zA-Z0-9_""/.]+$");

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// =true - имя схемы корректное
        /// </summary>
        /// <param name="name">имя схемы</param>
        /// <returns></returns>
        public static bool IsSchemaNameCorrect(string name) =>
            string.IsNullOrWhiteSpace(name) ||
            name.IsMatch(@"^[a-zA-Z0-9_""]+$");

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// =true - имя поля корректное
        /// </summary>
        /// <param name="name">имя поля</param>
        /// <returns></returns>
        public static bool IsFieldNameCorrect(string name) =>
            string.IsNullOrWhiteSpace(name) ||
            name.IsMatch(@"^[a-zA-Z0-9_""]+$");

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// =true - имя поля FK-таблицы корректное
        /// </summary>
        /// <param name="name">имя поля</param>
        /// <returns></returns>
        public static bool IsFKFieldNameCorrect(string name) =>
            string.IsNullOrWhiteSpace(name) ||
            name.IsMatch(@"^[a-zA-Z0-9_,""]+$");

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// =true - имя констрейна корректное
        /// </summary>
        /// <param name="name">имя констрейна</param>
        /// <returns></returns>
        public static bool IsConstraintNameCorrect(string name) =>
            string.IsNullOrWhiteSpace(name) ||
            name.IsMatch(@"^[a-zA-Z0-9_""]+$");

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// =true - N п\п корректный
        /// </summary>
        /// <param name="name">N п\п</param>
        /// <returns></returns>
        public static bool IsOrderCorrect(string name) =>
            string.IsNullOrWhiteSpace(name) ||
            name.IsMatch(@"^[0-9]+$");

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// убираем GO + переводы строки и возвраты коретки в конце text и добавляем Environment.NewLine + GO + Environment.NewLine
        /// </summary>
        /// <param name="text">текст</param>
        /// <returns></returns>
        public static string AddEndGO(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                text = "";
            }

            text = text.TrimEndNewLine();

            if (text.ToUpper().EndsWith("GO"))
            {
                // тект заканчивается на GO, добавляем только Environment.NewLine
                return text + Environment.NewLine;
            }
            else
            {
                // тект НЕ заканчивается на GO, добавляем Environment.NewLine + GO + Environment.NewLine
                return text + Environment.NewLine + "GO" + Environment.NewLine;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Преобразовать тип в строке в Type
        /// </summary>
        /// <param name="typename">тип</param>
        /// <returns></returns>
        public static Type ConvertType(string typename)
        {
            if (typename == null) typename = "";
            typename = typename.Trim().ToUpper();

            switch (typename)
            {
                case "INTEGER":
                case "BIGINT":
                case "INT":
                case "BIT":
                case "TINYINT":
                case "SMALLINT":
                    return Type.GetType("System.Int64");

                case "DOUBLE PRECISION":
                case "DOUBLE":
                case "REAL":
                case "FLOAT":
                case "MONEY":
                case "DECIMAL":
                case "NUMERIC":
                    return Type.GetType("System.Decimal");

                case "TIMESTAMP WITHOUT TIME ZONE":
                case "TIMESTAMP WITH TIME ZONE":
                case "TIMESTAMP":
                case "TIMESTAMPTZ":
                case "DATE":
                case "DATETIME":
                case "TIME WITHOUT TIME ZONE":
                case "TIME WITH TIME ZONE":
                case "TIME":
                case "TIMETZ":
                case "DATETIME2":
                case "DATETIMEOFFSET":
                    return Type.GetType("System.DateTime");

                case "CHAR":
                case "VARCHAR":
                case "TEXT":
                case "NCHAR":
                case "NVARCHAR":
                case "NTEXT":
                case "SYSNAME":
                    return Type.GetType("System.String");

                case "BOOLEAN":
                    return Type.GetType("System.Boolean");

                default:
                    return Type.GetType("System.String");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Отделить от файла начальную часть до определенной строки</summary>
        /// <param name="OrigFile">исходный файл, в котором останется не отделенная часть</param>
        /// <param name="NewFile">Новый файл, куда сохраняется отделяемая часть</param>
        /// <param name="Lines">Строки исходного файла</param>
        /// <param name="FinishLine">Номер последней отделяемый строки (zero-based)</param>
        /// <param name="TypeDB">Тип БД</param>
        /// <param name="TypeScript">Тип скрипта</param>
        /// <param name="SchemaName">Схема</param>
        /// <param name="ObjectName">Объект</param>
        /// <param name="warning">Предупреждения, требующие внимания</param>
        /// <param name="_cbConnect">ComboBox с подключениями к БД</param>
        public static System.Windows.Forms.DialogResult SplitFile(string OrigFile, ref string NewFile, string[] Lines, int FinishLine, string TypeDB, string TypeScript, string SchemaName, string ObjectName, out string warning, System.Windows.Controls.ComboBox _cbConnect)
        {
            warning = "";

            if (FinishLine < 0) return System.Windows.Forms.DialogResult.Abort;

            System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.Cancel;

            int FirstCount = FinishLine + 1;
            string[] FirstLines = new string[FirstCount];
            Array.Copy(Lines, 0, FirstLines, 0, FirstCount);

            int FirstNotEmpty = -1;

            // точечная правка строк
            for (int i = 0; i < FirstLines.Count(); i++)
            {
                if ((FirstNotEmpty == -1) &&
                    (FirstLines[i].Trim() != "") &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("print 'autogen error")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("print 'warning! the object")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("warning! the object")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("--liquibase formatted sql")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("--changeset ")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("--comment:")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("-- sqlgen")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("-- внимание!")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("-- warning!")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("error:")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("-- error:")) &&
                    (!FirstLines[i].Trim().ToLower().StartsWith("warning!")) //-V3053
                    ) FirstNotEmpty = i;

                if (FirstLines[i].ToLower().StartsWith("completion time:"))
                {
                    FirstLines[i] = "";
                }

                if (FirstLines[i].ToLower() == "(1 row affected)")
                {
                    FirstLines[i] = "";
                }
                if (
                    FirstLines[i].ToLower().StartsWith("(") &&
                    FirstLines[i].ToLower().EndsWith(" rows affected)")

                    )
                {
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("print 'autogen error"))
                {
                    warning += FirstLines[i].Replace("print '", "") + Environment.NewLine;
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("print 'warning! the object"))
                {
                    warning += FirstLines[i].Replace("print 'warning!", "") + Environment.NewLine;
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("warning! the object"))
                {
                    warning += FirstLines[i].Replace("Warning!", "").Replace("warning!", "") + Environment.NewLine;
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("--liquibase formatted sql"))
                {
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("--changeset ") && !FirstLines[i].ToLower().Contains("dev:test"))
                {
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("--comment:"))
                {
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("-- sqlgen"))
                {
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("-- внимание!"))
                {
                    warning += FirstLines[i].Replace("-- Внимание!", "").Replace("-- внимание!", "") + Environment.NewLine;
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("-- warning!"))
                {
                    warning += FirstLines[i].Replace("-- Warning!", "").Replace("-- warning!", "") + Environment.NewLine;
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("warning!"))
                {
                    warning += FirstLines[i].Replace("Warning!", "").Replace("warning!", "") + Environment.NewLine;
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("error:"))
                {
                    warning += Environment.NewLine + FirstLines[i].Replace("Error:", "").Replace("error:", "") + Environment.NewLine + Environment.NewLine;
                    FirstLines[i] = "";
                }
                if (FirstLines[i].ToLower().StartsWith("-- error:"))
                {
                    warning += Environment.NewLine + FirstLines[i].Replace("-- Error:", "").Replace("-- error:", "") + Environment.NewLine + Environment.NewLine;
                    FirstLines[i] = "";
                }


                if ((TypeDB == "ms") && (TypeScript == "view") &&
                    (FirstLines[i].ToLower().StartsWith("if exists (select 1 from sys.sysobjects") ||
                      FirstLines[i].ToLower().StartsWith("if exists(select 1 from sys.sysobjects")) && (FirstNotEmpty == i))
                {
                    FirstLines[i] = "IF OBJECT_ID(N'" + SchemaName + '.' + ObjectName + "', 'V') IS NOT NULL";
                }

                if ((TypeDB == "ms") && (TypeScript == "proc") &&
                    (FirstLines[i].ToLower().StartsWith("if exists (select 1 from sys.sysobjects") ||
                      FirstLines[i].ToLower().StartsWith("if exists(select 1 from sys.sysobjects")) && (FirstNotEmpty == i))
                {
                    FirstLines[i] = "IF OBJECT_ID(N'" + SchemaName + '.' + ObjectName + "', 'P') IS NOT NULL";
                }

                if ((TypeDB == "pg") && (TypeScript == "view") && FirstLines[i].ToLower().StartsWith("drop view if exists") && (FirstNotEmpty == i))
                {
                    FirstLines[i] = "SELECT dbo.xp_gen_view('" + SchemaName.ToLower() + '.' + ObjectName.ToLower() + "'," + Environment.NewLine +
                        "$viewtext$";
                    FirstLines[FirstCount - 1] = FirstLines[FirstCount - 1] + Environment.NewLine +
                        "$viewtext$" + Environment.NewLine +
                        ",2);";
                }

                if ((TypeDB == "pg") && (TypeScript == "view") && FirstLines[i].Contains(" 'Autogen'"))
                {
                    //FirstLines[i] = FirstLines[i].Replace(" 'Autogen'", " ''Autogen''");
                }

                if ((TypeDB == "pg") && (TypeScript == "proc") && FirstLines[i].ToLower().StartsWith("drop function") && (FirstNotEmpty == i))
                {
                    FirstLines[i] = "SELECT dbo.xp_dropfns('" + SchemaName.ToLower() + '.' + ObjectName.ToLower() + "');" + Environment.NewLine;
                }

                if ((TypeDB == "pg") && (TypeScript == "proc") && FirstLines[i].ToLower().StartsWith("create or replace function ") && (FirstNotEmpty == i))
                {
                    string[] newLines = new string[FirstCount + 2];
                    newLines[0] = "SELECT dbo.xp_dropfns('" + SchemaName.ToLower() + '.' + ObjectName.ToLower() + "');";
                    newLines[1] = "";

                    for (int j = 0; j < FirstCount; j++)
                    {
                        newLines[j + 2] = FirstLines[j];
                    }
                    FirstLines = newLines;
                    break;
                }
            }

            if (FirstNotEmpty != -1)
            {
                // сохраняем отделяемую часть в новый файл

                string[] buffer = new string[FirstLines.Length - FirstNotEmpty];
                Array.Copy(FirstLines, FirstNotEmpty, buffer, 0, FirstLines.Length - FirstNotEmpty);

                if (TypeDB == "ms")
                {
                    buffer[0] = MainWindow.Task.TitleScript(TargetDBType.MSSQL, TypeScript, false, false) + Environment.NewLine + buffer[0];
                }
                if (TypeDB == "pg")
                {
                    buffer[0] = MainWindow.Task.TitleScript(TargetDBType.PGSQL, TypeScript, false, false) + Environment.NewLine + buffer[0];
                }

                FormAskProc dlg1 = new FormAskProc();
                dlg1.tbProcName.Text = ((TypeScript == "freedocmarker" || TypeScript == "freedocrelationship") ? "" : (SchemaName + '.')) + ObjectName;
                dlg1.tbProcFile.Text = NewFile;
                dlg1.tbWarning.Text = warning;
                dlg1.tbProcText.Lines = buffer;
                dlg1.cbConnect = _cbConnect;

                result = dlg1.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    NewFile = dlg1.tbProcFile.Text;
                    Utilities.Files.WriteScript(NewFile, null, dlg1.tbProcText.Lines, false, out string err, FileMode.Create);
                }
                dlg1.Dispose();

                if (result == System.Windows.Forms.DialogResult.Abort)
                {
                    return result;
                }
            }

            int SecondCount = Lines.Count() - FirstCount;
            if (SecondCount > 0)
            {
                // сохраняем остающуюся часть в оригинальный файл - для дальнейшего разбора/разделения

                string[] SecondLines = new string[SecondCount];
                Array.Copy(Lines, FinishLine + 1, SecondLines, 0, SecondCount);
                File.WriteAllLines(OrigFile, SecondLines);
            }
            else File.Delete(OrigFile);

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Конвертация списка в DataTable</summary>
        /// <param name="models">Список</param>
        /// <returns>DataTable</returns>
        public static System.Data.DataTable ConvertToDataTable<T>(List<T> models)
        {
            System.Data.DataTable dataTable = new System.Data.DataTable(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Loop through all the properties            
            // Adding Column to our datatable
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names  
                dataTable.Columns.Add(prop.Name);
            }
            // Adding Row
            foreach (T item in models)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows  
                    values[i] = Props[i].GetValue(item, null);
                }
                // Finally add value to datatable  
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Разобрать скрипт и сохранить в файлы
        /// </summary>
        /// <param name="Prefix">префикс</param>
        /// <param name="TaskNumber">номер задачи</param>
        /// <param name="filename">имя файла</param>
        /// <param name="NumFile">номер файла</param>
        /// <param name="_cbConnect">подключение к БД</param>
        public static void SaveProcScript(string Prefix, string TaskNumber, string filename, string NumFile, System.Windows.Controls.ComboBox _cbConnect)
        {
            string LogFile = Path.GetTempFileName();
            App.tempFiles.Add(LogFile);

            bool isWarning = false;

            if ((filename != "") && File.Exists(filename))
            {

                try
                {
                    // определим кодировку
                    Encoding encoding = new UTF8Encoding(false);
                    if (Utilities.Files.GetEncoding(filename, out bool isBOM) == "1251")
                    {
                        encoding = Encoding.GetEncoding("windows-1251");
                    }

                    // читаем файл
                    string[] lines = File.ReadAllLines(filename, encoding);

                    TaskNumber = TaskNumber.Trim().Replace("-", "").ToLower();

                    string TypeDB = "?";
                    string TypeScript = "?";
                    string SchemaName = "?";
                    string ObjectName = "?";

                    FormAskNumFile dlg1 = new FormAskNumFile();
                    dlg1.tbNumFile.Text = NumFile;

                    if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK) NumFile = dlg1.tbNumFile.Text.Trim();
                    if (NumFile == "") NumFile = "0";
                    dlg1.Dispose();

                    int cnt = lines.Count();
                    bool isnew = false;

                    while (cnt > 0)
                    {
                        // разбираем файл на части, ищем ключевые фразы
                        string prev_line = "";
                        string line = "";
                        string next_line = "";
                        bool isfound = false;

                        // перебираем строки
                        for (int i = 0; i < cnt; i++)
                        {
                            prev_line = "";
                            line = "";
                            next_line = "";

                            // предыдущая НЕ пустая строка
                            for (int j = i - 1; j >= 0; j--)
                            {
                                if (
                                        (j >= 0) &&  //-V3063
                                        (!string.IsNullOrWhiteSpace(lines[j]))
                                )
                                {
                                    prev_line = lines[j];
                                    break;
                                }
                            }

                            // текущая строка
                            line = lines[i];

                            // следующая НЕ пустая строка
                            for (int j = i + 1; j < cnt; j++)
                            {
                                if (
                                        (j < cnt) &&
                                        (!string.IsNullOrWhiteSpace(lines[j]))
                                )
                                {
                                    next_line = lines[j];
                                    break;
                                }
                            }

                            prev_line = prev_line.Trim(new char[] { '\t', ' ' }).ToLower();
                            line = line.Trim(new char[] { '\t', ' ' }).ToLower();
                            next_line = next_line.Trim(new char[] { '\t', ' ' }).ToLower();

                            if (line.StartsWith("-- sqlgen"))
                            {
                                isnew = true;
                            }

                            // определяем тип скрипта и имя объекта
                            if (prev_line.StartsWith("-- sqlgen: freedocrelationship"))
                            {
                                TypeScript = "freedocrelationship";
                                string s = prev_line.Substring(30).Trim();
                                SchemaName = "dbo";
                                ObjectName = s.Split(' ')[0];
                            }

                            if (prev_line.StartsWith("-- sqlgen: freedocmarker"))
                            {
                                TypeScript = "freedocmarker";
                                string s = prev_line.Substring(24).Trim();
                                SchemaName = "dbo";
                                ObjectName = s.Split(' ')[0];
                            }

                            if (prev_line.StartsWith("create view "))
                            {
                                TypeScript = "view";
                                string s = prev_line.Substring(11);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1];
                            }

                            if (prev_line.StartsWith("alter view "))
                            {
                                TypeScript = "view";
                                string s = prev_line.Substring(10);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1];
                            }

                            if (prev_line.StartsWith("create or replace view "))
                            {
                                TypeScript = "view";
                                string s = prev_line.Substring(22);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1];
                            }

                            if (prev_line.StartsWith("create or replace materialized view "))
                            {
                                TypeScript = "view";
                                string s = prev_line.Substring(35);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1];
                            }

                            if (prev_line.StartsWith("select xp_gen_view( '"))
                            {
                                TypeScript = "view";
                                string s = prev_line.Substring(21);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('\'')[0].Split(';')[0];
                            }

                            if (prev_line.StartsWith("select * from dbo.xp_gen_view('"))
                            {
                                TypeScript = "view";
                                string s = prev_line.Substring(31);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('\'')[0].Split(';')[0];
                            }

                            if (prev_line.StartsWith("select * from xp_gen_view('"))
                            {
                                TypeScript = "view";
                                string s = prev_line.Substring(27);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('\'')[0].Split(';')[0];
                            }

                            if (prev_line.StartsWith("select xp_gen_view('"))
                            {
                                TypeScript = "view";
                                string s = prev_line.Substring(20);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('\'')[0].Split(';')[0];
                            }

                            if (prev_line.StartsWith("select dbo.xp_gen_view('"))
                            {
                                TypeScript = "view";
                                string s = prev_line.Substring(24);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('\'')[0].Split(';')[0];
                            }

                            if (prev_line.StartsWith("create proc "))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(11);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }

                            if (prev_line.StartsWith("alter proc "))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(10);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }

                            if (prev_line.StartsWith("create or replace proc "))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(22);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }

                            if (prev_line.StartsWith("create func "))
                            {
                                TypeScript = "func";
                                string s = prev_line.Substring(11);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }

                            if (prev_line.StartsWith("alter func "))
                            {
                                TypeScript = "func";
                                string s = prev_line.Substring(10);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }

                            if (prev_line.StartsWith("create procedure "))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(16);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }

                            if (prev_line.StartsWith("alter procedure "))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(15);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }

                            if (prev_line.StartsWith("create or replace procedure "))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(27);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }

                            if (prev_line.StartsWith("create function "))
                            {
                                TypeScript = "func";
                                string s = prev_line.Substring(15);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }

                            if (prev_line.StartsWith("alter function "))
                            {
                                TypeScript = "func";
                                string s = prev_line.Substring(14);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }

                            if (prev_line.StartsWith("create or replace function "))
                            {
                                TypeScript = "func";
                                string s = prev_line.Substring(26);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('(')[0];
                            }
                            if (prev_line.StartsWith("select xp_dropfns( '"))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(20);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('\'')[0].Split(';')[0];

                                if (prev_line.Contains("create or replace func"))
                                {
                                    TypeScript = "func";
                                }
                            }

                            if (prev_line.StartsWith("select xp_dropfns('"))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(19);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('\'')[0].Split(';')[0];

                                if (prev_line.Contains("create or replace func"))
                                {
                                    TypeScript = "func";
                                }
                            }

                            if (prev_line.StartsWith("select dbo.xp_dropfns('"))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(23);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('\'')[0].Split(';')[0];

                                if (prev_line.Contains("create or replace func"))
                                {
                                    TypeScript = "func";
                                }
                            }

                            if (prev_line.StartsWith("select * from xp_dropfns('"))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(26);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('\'')[0].Split(';')[0];

                                if (prev_line.Contains("create or replace func"))
                                {
                                    TypeScript = "func";
                                }
                            }

                            if (prev_line.StartsWith("select * from dbo.xp_dropfns('"))
                            {
                                TypeScript = "proc";
                                string s = prev_line.Substring(30);
                                SchemaName = s.Split('.')[0];
                                ObjectName = s.Split('.')[1].Split('\'')[0].Split(';')[0];

                                if (prev_line.Contains("create or replace func"))
                                {
                                    TypeScript = "func";
                                }
                            }

                            SchemaName = SchemaName.Trim();
                            ObjectName = ObjectName.Trim();

                            // определение типа БД
                            if (line.Contains("object_id(")) TypeDB = "ms";
                            if (line.Contains("object_id (")) TypeDB = "ms";
                            if (line.Contains("identity_insert")) TypeDB = "ms";
                            if (line.Contains("set nocount")) TypeDB = "ms";
                            if (line.Contains("(nolock)")) TypeDB = "ms";
                            if (line.Contains("(rowlock)")) TypeDB = "ms";
                            if (line == "go") TypeDB = "ms";

                            if (line.Contains("language plpgsql")) TypeDB = "pg";
                            if (line.StartsWith("create or replace ")) TypeDB = "pg";
                            if (line.Contains("xp_gen_view")) TypeDB = "pg";
                            if (line.Contains("xp_dropfns")) TypeDB = "pg";
                            if (line.Contains("on conflict do nothing")) TypeDB = "pg";
                            if (line.Contains("localtimestamp")) TypeDB = "pg";
                            if (line.Contains(":=")) TypeDB = "pg";

                            // тестовая заглушка
                            if (line.ToLower().StartsWith("--liquibase formatted sql"))
                            {
                                //string test = "drop";
                            }

                            // ищем разрыв файла
                            if (
                                    (
                                        (
                                            (i > 8) &&
                                            (!isnew) &&
                                            (prev_line == "go") &&
                                            line.Replace(" ", "").StartsWith("ifexists(select1")
                                        ) ||
                                        (
                                            (i > 8) &&
                                            (!isnew) &&
                                            (
                                                (prev_line == "print '" + SchemaName + "." + ObjectName + " - complete'") ||
                                                (prev_line == "print '" + ObjectName + " - complete'") ||
                                                prev_line.StartsWith("print 'autogen error object") ||
                                                (
                                                    prev_line.StartsWith("print 'warning! the object - ") &&
                                                    prev_line.EndsWith(" can''t auto generate")
                                                )
                                            ) &&
                                            line.Replace(" ", "").StartsWith("ifexists(select1")
                                        ) ||
                                        (
                                            (i > 8) &&
                                            (!isnew) &&
                                            prev_line.StartsWith("--comment:") &&
                                            (line == "do") &&
                                            (next_line == "$$")
                                        ) ||
                                        (
                                            (i > 8) &&
                                            (!isnew) &&
                                            prev_line.StartsWith("--comment:") &&
                                            (
                                                line.StartsWith("select xp_dropfns") ||
                                                line.StartsWith("select dbo.xp_dropfns") ||
                                                line.StartsWith("select * from xp_dropfns") ||
                                                line.StartsWith("select * from dbo.xp_dropfns") ||
                                                line.StartsWith("select xp_gen_view") ||
                                                line.StartsWith("select dbo.xp_gen_view") ||
                                                line.StartsWith("select * from xp_gen_view") ||
                                                line.StartsWith("select * from dbo.xp_gen_view")
                                            )
                                        ) ||
                                        (
                                            (i > 8) &&
                                            (!isnew) &&
                                            (
                                                prev_line.EndsWith(" row affected)") ||
                                                prev_line.EndsWith(" rows affected)") ||
                                                (prev_line == "go") ||
                                                prev_line.StartsWith("--comment:")
                                            ) &&
                                            line.Replace(" ", "").StartsWith("ifobject_id(") &&
                                            next_line.StartsWith("drop ")
                                        ) ||
                                        (
                                            (i > 8) &&
                                            (!isnew) &&
                                            line.StartsWith("--liquibase formatted sql")
                                        ) ||
                                        (
                                            (i > 0) &&
                                            isnew &&
                                            line.StartsWith("-- sqlgen")
                                        ) ||
                                        (i == cnt - 1)
                                    ) &&
                                    (TypeScript != "?")
                            )
                            {
                                // найден разрыв между хранимками или конец файла
                                isfound = true;

                                // определяем префикс файла
                                if (string.IsNullOrWhiteSpace(Prefix))
                                {
                                    Prefix = TypeDB;
                                }

                                string newFilename = Path.Combine(Path.GetDirectoryName(filename), Prefix + " " + TaskNumber + " " + NumFile + " " + TypeScript + " " + SchemaName.Replace("\"", "") + " " + ObjectName.Replace("\"", "") + ".sql");

                                int index;
                                if (i == cnt - 1) index = i; else index = i - 1;

                                var result = SplitFile(filename, ref newFilename, lines, index, TypeDB, TypeScript, SchemaName, ObjectName, out string warning, _cbConnect);
                                if (!string.IsNullOrWhiteSpace(warning))
                                {
                                    try
                                    {
                                        File.AppendAllText(LogFile, warning + Environment.NewLine);
                                    }
                                    catch (Exception ex)
                                    {
                                        App.AddLog("", ex, App.ShowMessageMode.SHOW, false, null);
                                    }
                                    
                                    isWarning = true;
                                }

                                if (result == System.Windows.Forms.DialogResult.OK)
                                {
                                    App.AddLog("Выделен файл '" + Path.GetFileName(newFilename) + "' из '" + Path.GetFileName(filename) + "'", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                }
                                else if (result == System.Windows.Forms.DialogResult.Cancel)
                                {
                                    App.AddLog("Пропущен файл '" + Path.GetFileName(newFilename) + "' при выделении из '" + Path.GetFileName(filename) + "'", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                }
                                else
                                {
                                    App.AddLog("Разбор файла '" + Path.GetFileName(filename) + "' прерван", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                    cnt = 0;
                                    break;
                                }

                                if (File.Exists(filename))
                                {
                                    lines = File.ReadAllLines(filename);
                                    cnt = lines.Count();
                                    TypeDB = "?";
                                    TypeScript = "?";
                                    SchemaName = "?";
                                    ObjectName = "?";
                                    prev_line = "";
                                }
                                else cnt = 0;

                                break;
                            }
                        }

                        // показываем нераспознанную концовку
                        if (
                            (!isfound) &&
                            (cnt > 0)
                        )
                        {
                            App.AddLog("В файле '" + Path.GetFileName(filename) + "' осталась нераспознанная часть скрипта", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                            FormAskProc dlg2 = new FormAskProc();
                            dlg2.Text = "ВНИМАНИЕ! Осталась нераспознанная часть скрипта";
                            dlg2.tbProcName.Text = "";
                            dlg2.tbProcFile.Text = "";
                            dlg2.tbWarning.Lines = lines;
                            dlg2.tbProcText.Text = "";
                            dlg2.btOk.Enabled = false;
                            dlg2.cbConnect = null;
                            var result = dlg2.ShowDialog();

                            try
                            {
                                File.AppendAllText(LogFile, dlg2.tbWarning.Text + Environment.NewLine);
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("", ex, App.ShowMessageMode.SHOW, false, null);
                            }

                            isWarning = true;

                            dlg2.Dispose();

                            if (
                                (result != System.Windows.Forms.DialogResult.Abort) &&
                                File.Exists(filename)
                            )
                            {
                                File.Delete(filename);
                            }
                            cnt = 0;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            if (
                isWarning &&
                File.Exists(LogFile) &&
                (System.Windows.Forms.MessageBox.Show("Посмотреть предупреждения, требующие внимания ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            )
            {
                WinInfo WinInfo = new WinInfo(null);
                WinInfo.tbInfo.Text = File.ReadAllText(LogFile);
                WinInfo.Title = "Предупреждения";
                WinInfo.Show();
            }

            if (File.Exists(LogFile))
            {
                File.Delete(LogFile);
            }
        }

        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Отформатировать текст хранимки
        /// </summary>
        /// <param name="original_text"></param>
        /// <param name="schema"></param>
        /// <param name="objectname"></param>
        /// <param name="schemaseek"></param>
        /// <param name="objectseek"></param>
        /// <param name="DBType"></param>
        /// <param name="isSQLFormat"></param>
        /// <param name="error"></param>
        /// <returns></returns>
        public static string FormatProcText(string original_text, string schema, string objectname, string schemaseek, string objectseek, string DBType, ref bool isSQLFormat, out string error)
        {
            string result = original_text;
            error = "";

            // попытка форматирования текста с использованием внешней уитилиты

            // сохраняем текст во временный файл
            string command = Path.Combine(App.AppPath, "sqlfluff.cmd");

            string filesql = Path.Combine(Path.GetTempPath(), schemaseek + "_" + objectseek + ".sql");
            if (filesql.Contains(" ")) filesql = "\"" + filesql + "\"";

            string filecfg = Path.Combine(App.AppPath, "tsql.cfg");
            if (filecfg.Contains(" ")) filecfg = "\"" + filecfg + "\"";

            string param = filesql + " tsql " + filecfg;

            string formated_text = "";

            try
            {
                if (DBType == "MSSQL")
                {
                    File.WriteAllText(filesql, original_text);

                    ExecuteFile(
                        App.AppPath,
                        command,
                        param,
                        true,
                        true,
                        false,
                        true);

                    formated_text = File.ReadAllText(filesql);
                    File.Delete(filesql);
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(error)) error += Environment.NewLine; //-V3022
                error += command + " " + param + Environment.NewLine;
                error += FullExceptionMessage(ex) + Environment.NewLine;
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                App.AddLog(error, null, App.ShowMessageMode.NONE);

                isSQLFormat = System.Windows.Forms.MessageBox.Show("При форматировании скрипта " + schema + "." + objectname + " с использованием утилиты SQLFluff возникла ошибка: " + Environment.NewLine + error + Environment.NewLine + Environment.NewLine + "Продолжить форматировать скрипты с использованием SQLFluff ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes;

                if (isSQLFormat)
                {
                    App.AddLog("Выбрано - Продолжить форматировать скрипты с использованием SQLFluff", null, App.ShowMessageMode.NONE);
                }
            }
            else
            {
                result = formated_text;
            }

            return result;
        }
        */


        /// <summary>
        /// выделяем порт из адреса
        /// </summary>
        /// <param name="ServerName">Имя сервера или ip-адрес базы данных с портом</param>
        /// <returns></returns>
        public static string GetPortFromServerName(string ServerName)
        {
            if (string.IsNullOrWhiteSpace(ServerName)) ServerName = "";

            string port = ServerName.Replace(" ", "");
            var list = port.ToList(new char[] { ',', ':' }, true);
            if (list.Count > 1)
            {
                port = list[1];
            }
            else
            {
                port = "";
            }

            return port.Trim().ToLower();
        }


        /// <summary>
        /// Для сравнения по порту
        /// </summary>
        /// <param name="basePort">порт - где ищем</param>
        /// <param name="baseDBType">Тип базы данных - где ищем</param>
        /// <param name="seekServerName">Имя сервера или ip-адрес базы данных с портом - что ищем</param>
        /// <param name="seekDBType">Тип базы данных - что ищем</param>
        /// <returns></returns>
        public static bool ServerPortEqual(string basePort, string baseDBType, string seekServerName, string seekDBType)
        {
            if (string.IsNullOrWhiteSpace(baseDBType)) baseDBType = "";
            baseDBType = baseDBType.Replace(" ", "").Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(basePort)) basePort = "";
            basePort = basePort.Trim().ToLower();
            if (basePort == "" && baseDBType == "MSSQL") basePort = "1433";
            if (basePort == "" && baseDBType == "PGSQL") basePort = "5432";

            if (string.IsNullOrWhiteSpace(seekDBType)) seekDBType = "";
            seekDBType = seekDBType.Replace(" ", "").Trim().ToUpper();

            string seekPort = GetPortFromServerName(seekServerName);
            if (seekPort == "" && seekDBType == "MSSQL") seekPort = "1433";
            if (seekPort == "" && seekDBType == "PGSQL") seekPort = "5432";

            return basePort == seekPort;
        }

        /// <summary>
        /// убираем порт из адреса сервера
        /// </summary>
        /// <param name="ServerName">Имя сервера или ip-адрес базы данных с портом</param>
        /// <returns></returns>
        public static string GetAddrFromServerName(string ServerName)
        {
            if (string.IsNullOrWhiteSpace(ServerName)) ServerName = "";

            string addr = ServerName.Replace(" ", "");
            var list = addr.ToList(new char[] { ',', ':' }, true);
            if (list.Count > 0)
            {
                addr = list[0];
            }
            else
            {
                addr = "";
            }

            return addr.Trim().ToLower();
        }

        /// <summary>
        /// Для сравнения по имени/адресу сервера без порта
        /// </summary>
        /// <param name="baseAddr">Имя сервера или ip-адрес базы данных (без порта!) - где ищем</param>
        /// <param name="seekServerName">Имя сервера или ip-адрес базы данных (с портом) - что ищем</param>
        /// <returns></returns>
        public static bool ServerAddrEqual(string baseAddr, string seekServerName)
        {
            if (string.IsNullOrWhiteSpace(baseAddr)) baseAddr = "";
            baseAddr = baseAddr.Replace(" ", "").Trim().ToLower();

            string seekAddr = GetAddrFromServerName(seekServerName);

            return baseAddr == seekAddr;
        }

        /// <summary>
        /// Для сравнения по имени БД
        /// </summary>
        /// <param name="baseDBName">наименование БД - где ищем</param>
        /// <param name="seekDBName">наименование БД - что ищем</param>
        /// <returns></returns>
        public static bool DBNameEqual(string baseDBName, string seekDBName)
        {
            if (string.IsNullOrWhiteSpace(baseDBName)) baseDBName = "";
            baseDBName = baseDBName.Replace(" ", "").Trim().ToLower();

            if (string.IsNullOrWhiteSpace(seekDBName)) seekDBName = "";
            seekDBName = seekDBName.Replace(" ", "").Trim().ToLower();

            return baseDBName == seekDBName;
        }

        /// <summary>
        /// Для сравнения по типу БД
        /// </summary>
        /// <param name="baseDBType">Тип базы данных - где ищем</param>
        /// <param name="seekDBType">Тип базы данных - что ищем</param>
        /// <returns></returns>
        public static bool DBTypeEqual(string baseDBType, string seekDBType)
        {
            if (string.IsNullOrWhiteSpace(baseDBType)) baseDBType = "";
            baseDBType = baseDBType.Replace(" ", "").Trim().ToUpper();

            if (string.IsNullOrWhiteSpace(seekDBType)) seekDBType = "";
            seekDBType = seekDBType.Replace(" ", "").Trim().ToUpper();

            return baseDBType == seekDBType;
        }
    }

    // -------------------------------------------------------------------------------------------------------
    internal enum LocalInDB
    {
        NOT_CONNECT,
        LOCAL_EXIST,
        LOCAL_NOT_EXIST
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// наличие локального справочника в БД
    /// </summary>
    internal class LocalInDBInstance
    {
        public string Key = "";
        public string Name = "";

        public LocalInDB PromedtestPG = LocalInDB.LOCAL_NOT_EXIST;
        public LocalInDB PromedadygeaPG = LocalInDB.LOCAL_NOT_EXIST;
        public LocalInDB PromedreleasePG = LocalInDB.LOCAL_NOT_EXIST;
        public LocalInDB PromeddevMS = LocalInDB.LOCAL_NOT_EXIST;
        public LocalInDB PromedtestMS = LocalInDB.LOCAL_NOT_EXIST;
        public LocalInDB PromedufaMS = LocalInDB.LOCAL_NOT_EXIST;
        public LocalInDB PromedwebreleaseMS = LocalInDB.LOCAL_NOT_EXIST;
        public LocalInDB PromedwebufareleaseMS = LocalInDB.LOCAL_NOT_EXIST;
    }
}
