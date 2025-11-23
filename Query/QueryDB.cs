// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using SQLGen.Utilities;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>Класс Запрос значений для Внешнего ключа</summary>
    public class ForeignKeyQuery
    {
        /// <summary>Запрос значений внешнего ключа</summary>
        public string Query { get; set; }

        /// <summary>Поле из запроса со значениями внешнего ключа</summary>
        public string FieldQuery { get; set; }

        /// <summary>схема таблицы со значениями внешнего ключа</summary>
        public string SchemaQuery { get; set; }

        /// <summary>таблица со значениями внешнего ключа</summary>
        public string TableQuery { get; set; }

    }

    // =========================================================================================================
    /// <summary>Класс Внешний ключ</summary>
    public class ForeignKey
    {
        /// <summary>Глубина вложения</summary>
        public int FKDeep { get; set; }

        /// <summary>схема таблицы-внешнего ключа</summary>
        public string FKSchema { get; set; }

        /// <summary>таблица-внешний ключ</summary>
        public string FKTable { get; set; }

        /// <summary>Поле-идентификатор таблицы внешнего ключа</summary>
        public string FKField { get; set; }

        /// <summary>Поле-внешний ключ</summary>
        public string ParentField { get; set; }

        /// <summary>Список запросов со значениями внешнего ключа</summary>
        public List<ForeignKeyQuery> ListFKQuery { get; set; }

        /// <summary>
        /// Конструктор ForeignKey
        /// </summary>
        /// <param name="_fkdeep">Глубина вложения</param>
        /// <param name="_schema">схема</param>
        /// <param name="_table">таблица</param>
        /// <param name="_pk">primary key</param>
        /// <param name="_query">запрос значений внешнего ключа</param>
        /// <param name="_schemaquery">схема таблицы со значениями внешнего ключа</param>
        /// <param name="_tablequery">таблица со значениями внешнего ключа</param>
        /// <param name="_fieldquery">поле из запроса со значением внешнего ключа</param>
        public ForeignKey(int _fkdeep, string _schema, string _table, string _pk, string _query, string _schemaquery, string _tablequery, string _fieldquery)
        {
            this.ListFKQuery = new List<ForeignKeyQuery>();

            this.FKDeep = _fkdeep;

            if (_schema == null) _schema = ""; else _schema = _schema.Trim();
            if (_schema == "") _schema = "dbo";
            if (_table == null) _table = ""; else _table = _table.Trim();
            if (_pk == null) _pk = ""; else _pk = _pk.Trim();
            if (_query == null) _query = ""; else _query = _query.Trim();
            if (_schemaquery == null) _schemaquery = ""; else _schemaquery = _schemaquery.Trim();
            if (_tablequery == null) _tablequery = ""; else _tablequery = _tablequery.Trim();
            if (_fieldquery == null) _fieldquery = ""; else _fieldquery = _fieldquery.Trim();

            this.FKSchema = _schema;
            this.FKTable = _table;
            this.FKField = _pk;
            this.ParentField = _fieldquery;

            if ((_fieldquery.Trim() != "") && (_query.Trim() != "")) this.ListFKQuery.Add(new ForeignKeyQuery() { FieldQuery = _fieldquery, Query = _query, SchemaQuery = _schemaquery, TableQuery = _tablequery });
        }

        /// <summary>Итоговый запрос значений внешнего ключа</summary>
        public string Query
        {
            get
            {
                string res = "";

                if ((FKSchema != "") && (FKTable != "") && (FKField != "") && (this.ListFKQuery != null) && (this.ListFKQuery.Count > 0))
                {
                    res = "SELECT * \nFROM " + FKSchema + "." + FKTable + " \nWHERE " + FKField + " IN (";

                    string q = "";

                    foreach (var _query in this.ListFKQuery)
                        if ((_query.FieldQuery != null) && (_query.SchemaQuery != null) && (_query.TableQuery != null) && (_query.Query != null) &&
                             (_query.FieldQuery != "") && (_query.SchemaQuery != "") && (_query.TableQuery != "") && (_query.Query != "")
                            )
                        {
                            string s = "\nSELECT distinct t." + _query.FieldQuery.Trim() + " FROM (\n" + _query.Query.Trim() + "\n) as t";

                            if (q != "") q = s + " \nUNION \n" + q;
                            else q = s;
                        }

                    res += q + ")\n";
                }

                return res;
            }
        }

        /// <summary>
        /// Условие существования для значения внешнего ключа в WHERE
        /// </summary>
        /// <param name="prefix">префикс строки</param>
        /// <param name="alias">алиас основной таблицы</param>
        /// <param name="hint_top">хинт TOP</param>
        /// <param name="hint_nolock">хинт NOLOCK</param>
        /// <param name="hint_limit">хинт LIMIT</param>
        /// <returns></returns>
        public string WhereExistsCondition(string prefix, string alias, string hint_top, string hint_nolock, string hint_limit)
        {
            return Environment.NewLine +
                $"{prefix}AND ({alias}.{this.ParentField} IS NULL OR EXISTS (SELECT {hint_top}1 FROM {this.FKSchema}.{this.FKTable} ttt {hint_nolock}WHERE ttt.{this.FKField} = {alias}.{this.ParentField}{hint_limit}))";
        }

        /// <summary>
        /// Условие существования для значения внешнего ключа в IF
        /// </summary>
        /// <param name="value">значение из поля основной таблицы</param>
        /// <param name="hint_top">хинт TOP</param>
        /// <param name="hint_nolock">хинт NOLOCK</param>
        /// <param name="hint_limit">хинт LIMIT</param>
        /// <returns></returns>
        public string IfExistsCondition(string value, string hint_top, string hint_nolock, string hint_limit)
        {
            return Environment.NewLine +
                $"AND EXISTS (SELECT {hint_top}1 FROM {this.FKSchema}.{this.FKTable} ttt {hint_nolock}WHERE ttt.{this.FKField} = {value}{hint_limit})";
        }
    }


    // =========================================================================================================
    /// <summary>Класс Запрос</summary>
    public class QueryDB : INotifyPropertyChanged
    {
        /// <summary>
        /// реализация OnPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// реализация OnPropertyChanged
        /// </summary>
        /// <param name="prop">prop</param>
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop)); //-V3083
        }

        private string _script_num;
        /// <summary>Номер скрипта</summary>
        [JsonIgnore]
        public string ScriptNumber
        {
            get
            {
                return _script_num ?? "0";
            }
            set
            {
                _script_num = value;
                if (string.IsNullOrWhiteSpace(_script_num)) _script_num = "0";
                _script_num = _script_num.Trim();
            }
        }

        /// <summary>Номер скрипта - для использования в имени файла</summary>
        public string ScriptNumberToFilename
        {
            get
            {
                return this.ScriptNumber.Replace("-", String.Empty).Replace(" ", String.Empty).ToLower();
            }
        }

        private string _project;
        /// <summary>проект GIT</summary>
        public string GITProject
        {
            get
            {
                return Utilities.GITProjects.GetProjectByProject(_project ?? "");
            }
            set
            {
                _project = value;
                if (string.IsNullOrWhiteSpace(_project)) _project = "";
                _project = _project.Trim();
            }
        }

        /// <summary>
        /// Это база promed
        /// </summary>
        public bool isPromed
        {
            get
            {
                return
                    (Utilities.GITProjects.GetDEVProject(this.GITProject) == "dev_promed_ms") ||
                    (Utilities.GITProjects.GetDEVProject(this.GITProject) == "dev_promed_pg");
            }
        }

        /// <summary>
        /// Это база ЛИС
        /// </summary>
        public bool isLIS
        {
            get
            {
                return
                    (Utilities.GITProjects.GetDEVProject(this.GITProject) == "dev_lis_pg");
            }
        }

        /// <summary>
        /// Это база РЭМД
        /// </summary>
        public bool isEMD
        {
            get
            {
                return
                    (Utilities.GITProjects.GetDEVProject(this.GITProject) == "dev_emd_pg");
            }
        }

        /// <summary>
        /// Список тестовых и релизных БД
        /// </summary>
        public List<string> ListTestReleaseDB
        {
            get
            {
                List<string> list = new List<string>();

                foreach (var item in MainWindow.APPinfo.ListDatabases
                    .Where(x =>
                        x.GITProject == this.GITProject &&
                        (x.DBRole == "TEST" || x.DBRole == "RELEASE")
                    )
                    .OrderBy(x =>
                    {
                        string ord = "999";
                        if (x.DBName.ToLower() == "promedadygea") ord = "001";
                        else if (x.DBName.ToLower() == "promedtest") ord = "002";
                        else if (x.DBName.ToLower() == "promeddev") ord = "003";

                        return x.DBRole + " " + ord + " " + x.DBName;
                    })
                )
                {
                    if (!list.Contains(item.DBName, StringComparer.OrdinalIgnoreCase))
                    {
                        list.Add(item.DBName);
                    }
                }

                if (list.Count == 0)
                {
                    list.Add("?");
                }

                return list;
            }
        }

        /// <summary>
        /// Полный список тестовых и релизных БД, в одинарных кавычках через запятую
        /// </summary>
        public string ListTestReleaseDB_full => Utilities.Strings.ListToString(ListTestReleaseDB, ", ", "'");

        /// <summary>
        /// Полный список тестовых и релизных БД, для региональной проверки с getregion, в одинарных кавычках через запятую
        /// </summary>
        public string ListTestReleaseDB_forRegionalCheck => 
            Utilities.Strings.ListToString(
                ListTestReleaseDB.Where(
                    x => 
                        x.ToLower() != "promedufa" &&
                        x.ToLower() != "promedwebufarelease" &&
                        x.ToLower() != "promedlistest_ufa" &&
                        x.ToLower() != "lisrelease_ufa"
                    ).ToList()
                , ", ", "'");

        /// <summary>целевая БД</summary>
        public Utilities.TargetDBType TargetDB
        {
            get
            {
                return ConnectDB.GetTargetDBTypeByProject(this.GITProject);
            }
        }

        /// <summary>тип целевой БД</summary>
        public Utilities.ConnType ConnType
        {
            get
            {
                return ConnectDB.GetConnTypeByProject(this.GITProject);
            }
        }

        /// <summary>префикс для использования в имени файла</summary>
        public string PrefixToFilename
        {
            get
            {
                return Utilities.GITProjects.GetPrefixFileSQLByProject(GITProject);
            }
        }

        /// <summary>тип БД</summary>
        public string DBType
        {
            get
            {
                return Utilities.GITProjects.GetDBTypeByProject(GITProject);
            }
        }

        /// <summary>Тип скрипта</summary>
        public Utilities.ScriptType ScriptType { get; set; }

        /// <summary>Тип скрипта - для использования в имени файла</summary>
        public string ScriptTypeToFilename
        {
            get
            {
                switch (this.ScriptType)
                {
                    case Utilities.ScriptType.UPDATE:
                        return "update";
                    case Utilities.ScriptType.DELETE:
                        return "delete";
                    case Utilities.ScriptType.UPSERT:
                    case Utilities.ScriptType.UPSERT_TMP:
                        return "upsert";
                    case Utilities.ScriptType.INSERT_BULK_TABLE:
                    case Utilities.ScriptType.INSERT_BULK_VIEW:
                        return "bulk";
                    case Utilities.ScriptType.INSERT:
                    case Utilities.ScriptType.INSERT_VALUES:
                    case Utilities.ScriptType.INSERT_TMP:
                    default:
                        return "insert";
                }
            }
        }

        /// <summary>Конструктор QueryDB - инициализация значений по умолчанию</summary>
        public QueryDB()
        {
            this.GITProject = "dev_promed_pg";
            this.ScriptType = Utilities.ScriptType.INSERT;
            this.DataTable = new DataTable();
            this.InsUpdDTType = Utilities.InsUpdDTType.VARI;
            this.isUseInsertUpdate = true;
            this.isAddCheckUnique = false;
            this.isAddEmptyString = true;
            this.isAddDel = false;
            this.SQLScript = "";
            this.SQLQuery = "";
            this.isAddGO = false;
            this.isAddCreateTable = false;
            this.isUpdateLocalDBList = true;
            this.ListNotUsedColumns = new List<string>();
        }

        string _table_name;
        /// <summary>Имя таблицы</summary>
        public string FullTableName
        {
            get
            {
                return _table_name ?? "";
            }
            set
            {
                _table_name = value;
                if (string.IsNullOrWhiteSpace(_table_name)) _table_name = "";
                _table_name = _table_name.Trim();
            }
        }

        /// <summary>Схема из имени таблицы - для использования в скрипте</summary>
        public string SchemaNameToScript
        {
            get
            {
                var arr = FullTableNameToScript.Split('.');
                if (arr.Length <= 1) return "dbo";
                else return arr[0];
            }
        }

        /// <summary>Схема - в оригинальном регистре, но без кавычек</summary>
        public string SchemaNameReady => SchemaNameToScript.Replace("\"", "");

        /// <summary>Схема - для сравнения, в нижнем регистре и без кавычек</summary>
        public string SchemaNameCompare => SchemaNameReady.ToLower();

        /// <summary>Схема в целевой БД, для поиска в БД</summary>
        public string SchemaNameToSeek => SchemaNameToScript.Replace("\"", "");

        /// <summary>Схема в целевой БД, для поиска в БД с помощью like\ilike</summary>
        public string SchemaNameToSeekForLike
        {
            get
            {
                switch (this.TargetDB)
                {
                    case Utilities.TargetDBType.MSSQL:
                        return SchemaNameToSeek.Replace("_", "[_]");
                    case Utilities.TargetDBType.EMD:
                    case Utilities.TargetDBType.PGSQL:
                        return SchemaNameToSeek.Replace("_", "\\_");
                    default:
                        return SchemaNameToSeek;
                }
            }
        }

        /// <summary>Имя из имени таблицы - для использования в скрипте</summary>
        public string TableNameToScript
        {
            get
            {
                var arr = FullTableNameToScript.Split('.');
                if (arr.Length <= 1) return FullTableNameToScript;
                else return arr[1];
            }
        }

        /// <summary>Имя таблицы - в оригинальном регистре, но без кавычек</summary>
        public string TableNameReady => TableNameToScript.Replace("\"", "");

        /// <summary>Имя таблицы - для сравнения, в нижнем регистре и без кавычек</summary>
        public string TableNameCompare => TableNameReady.ToLower();

        /// <summary>Имя таблицы в целевой БД, для поиска в БД</summary>
        public string TableNameToSeek => TableNameToScript.Replace("\"", "");

        /// <summary>Имя таблицы в целевой БД, для поиска в БД с помощью like\ilike</summary>
        public string TableNameToSeekForLike
        {
            get
            {
                switch (this.TargetDB)
                {
                    case Utilities.TargetDBType.MSSQL:
                        return TableNameToSeek.Replace("_", "[_]");
                    case Utilities.TargetDBType.EMD:
                    case Utilities.TargetDBType.PGSQL:
                        return TableNameToSeek.Replace("_", "\\_");
                    default:
                        return TableNameToSeek;
                }
            }
        }

        /// <summary>Имя таблицы - для использования в скрипте</summary>
        public string FullTableNameToScript
        {
            get
            {
                switch (this.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                    case Utilities.TargetDBType.MSSQL:
                        return this.FullTableName;
                    case Utilities.TargetDBType.PGSQL:
                        return this.FullTableName.ToLower();
                    default:
                        return this.FullTableName;
                }
            }
        }

        /// <summary>Полное имя таблицы - в оригинальном регистре, но без кавычек</summary>
        public string FullTableNameReady => FullTableNameToScript.Replace("\"", "");

        /// <summary>Имя таблицы - для сравнения, в нижнем регистре и без кавычек</summary>
        public string FullTableNameCompare => FullTableNameReady.ToLower();

        /// <summary>Имя таблицы - для использования в скрипте в запросе на существование записи</summary>
        public string FullTableNameToExistScript
        {
            get
            {
                if (TableNameCompare == "uslugacomplexattribute")
                {
                    return SchemaNameToScript + ".v_" + TableNameReady;
                }
                else
                {
                    return FullTableNameToScript;
                }
            }
        }

        /// <summary>Имя таблицы - для использования в имени файла</summary>
        public string TableNameToFilename
        {
            get
            {
                var arr = FullTableNameToFilename.Split(' ');
                if (arr.Length <= 1) return FullTableNameToFilename;
                else return arr[1];
            }
        }

        /// <summary>Полное имя таблицы - для использования в имени файла</summary>
        public string FullTableNameToFilename
        {
            get
            {
                return this.FullTableName.Replace("\"", String.Empty).Replace(" ", String.Empty).Replace(".", " ").ToLower();
            }
        }

        /// <summary>Как обновлять insDT/UpdDT</summary>
        public Utilities.InsUpdDTType InsUpdDTType { get; set; }

        private bool _isaddcreatetable;
        /// <summary>Добавить создание таблицы</summary>
        public bool isAddCreateTable
        {
            get
            {
                return _isaddcreatetable;
            }
            set
            {
                _isaddcreatetable = value == true;

                OnPropertyChanged("isAddCreateTable");
            }
        }

        private bool _isupdatelocaldblist;
        /// <summary>Добавить обновление stg.LocalDBList</summary>
        public bool isUpdateLocalDBList
        {
            get
            {
                return _isupdatelocaldblist;
            }
            set
            {
                _isupdatelocaldblist = value == true;

                OnPropertyChanged("isUpdateLocalDBList");
            }
        }


        private bool _isuseinsertupdate;
        /// <summary>Использовать INSERT+UPDATE</summary>
        public bool isUseInsertUpdate
        {
            get
            {
                return _isuseinsertupdate;
            }
            set
            {
                _isuseinsertupdate = value == true;

                OnPropertyChanged("isUseInsertUpdate");
            }
        }


        private bool _isaddcheckunique;
        /// <summary>Добавлять для INSERT и UPDATE проверку уникальности</summary>
        public bool isAddCheckUnique
        {
            get
            {
                return _isaddcheckunique;
            }
            set
            {
                _isaddcheckunique = value == true;

                OnPropertyChanged("isAddCheckUnique");
            }
        }

        private bool _isaddemptystring;
        /// <summary>Добавить пустую строку между командами INSERT/UPDATE/DELETE</summary>
        public bool isAddEmptyString
        {
            get
            {
                return _isaddemptystring;
            }
            set
            {
                _isaddemptystring = value == true;

                OnPropertyChanged("isAddEmptyString");
            }
        }

        private bool _isadddel;
        /// <summary>Добавить в скрипт поля Признака удаления </summary>
        public bool isAddDel
        {
            get
            {
                return _isadddel;
            }
            set
            {
                _isadddel = value == true;

                OnPropertyChanged("isAddDel");
            }
        }

        string _uk;
        /// <summary>Список уникальных полей</summary>
        public string UniqueKey
        {
            get
            {
                return _uk ?? "";
            }
            set
            {
                _uk = value;
                if (string.IsNullOrWhiteSpace(_uk)) _uk = "";
                _uk = _uk.Trim();

                OnPropertyChanged("UniqueKey");
            }
        }

        private bool _isaddgo;
        /// <summary>добавлять GO ?</summary>
        public bool isAddGO
        {
            get
            {
                return _isaddgo;
            }
            set
            {
                _isaddgo = value == true;

                OnPropertyChanged("isAddGO");
            }
        }

        /// <summary>текст начального SQL-запроса по выборке данных в будущий скрипт</summary>
        public string SQLQuery { get; set; }

        /// <summary>таблица с результатами SQL-запроса</summary>
        internal DataTable DataTable;

        /// <summary>
        /// список исключаемых из запроса полей
        /// </summary>
        public List<string> ListNotUsedColumns;

        /// <summary>список используемых полей</summary>
        internal List<DataColumn> DataTableUsedColumns
        {
            get
            {
                List<DataColumn> result = new List<DataColumn>();
                foreach (DataColumn column in this.DataTable.Columns)
                {
                    if (
                        (!result.Contains(column)) &&
                        (!ListNotUsedColumns.Select(x => x.ToLower()).Contains(column.ColumnName.ToLower()))
                        )
                    {
                        result.Add(column);
                    }
                }
                return result;
            }
        }

        private string _sqlscript;
        /// <summary>текст итогового SQL-скрипта</summary>
        public string SQLScript
        {
            get
            {
                return _sqlscript ?? "";
            }
            set
            {
                _sqlscript = value;
                if (string.IsNullOrWhiteSpace(_sqlscript)) _sqlscript = "";

                OnPropertyChanged("SQLScript");
            }
        }


        /// <summary>Имя файла с SQL-скриптом</summary>
        public string ScriptFilename
        {
            get
            {
                {
                    string s = this.PrefixToFilename + " " + MainWindow.Task.TaskNumberToFilename + " " + this.ScriptNumberToFilename + " " + this.ScriptTypeToFilename;
                    if (this.FullTableName != "") s = s + " " + this.FullTableNameToFilename;
                    s = s + ".sql";
                    return s.Replace("\"", "");
                }
            }
        }

        /// <summary>Имя файла с CSV-скриптом</summary>
        public string CSVFilename
        {
            get
            {
                string postfix = "";
                switch (this.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                    case Utilities.TargetDBType.PGSQL:
                        postfix = "copy";
                        break;
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        postfix = "bulk";
                        break;
                }

                string s = this.FullTableNameToFilename.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + MainWindow.Task.TaskNumber.ToLower() + "_" + postfix + ".csv";
                return s.Replace("\"", "");
            }
        }

        /// <summary>Все внешние ключи для текущей таблицы - заполняется в момент генерации скрипта</summary>
        public Dictionary<string, ForeignKey> ListTableFK = new Dictionary<string, ForeignKey>();


        /// <summary>
        /// Добавить в список ListTableFK
        /// </summary>
        /// <param name="Connect">подключение к БД</param>
        /// <param name="FKDeep">Глубина вложения</param>
        /// <param name="_schema">схема</param>
        /// <param name="_table">таблица</param>
        /// <param name="_pk">primary key</param>
        /// <param name="_query">запрос значений внешнего ключа</param>
        /// <param name="_schemaquery">схема таблицы со значениями внешнего ключа</param>
        /// <param name="_tablequery">таблица со значениями внешнего ключа</param>
        /// <param name="_fieldquery">поле из запроса со значением внешнего ключа</param>
        /// <param name="MaxFKDeep">маскимальная глубина вложения</param>
        /// <param name="isUniqueFKTables">=true - в итоговом списке будет уникальность по имени таблиця</param>
        private void AddListTableFK(ConnectDB Connect, int FKDeep, string _schema, string _table, string _pk, string _query, string _schemaquery, string _tablequery, string _fieldquery, int MaxFKDeep, bool isUniqueFKTables)
        {
            if (_schema == null) _schema = ""; else _schema = _schema.Trim();
            if (_schema == "") _schema = "dbo";
            if (_table == null) _table = ""; else _table = _table.Trim();
            if (_pk == null) _pk = ""; else _pk = _pk.Trim();
            if (_query == null) _query = ""; else _query = _query.Trim();
            if (_schemaquery == null) _schemaquery = ""; else _schemaquery = _schemaquery.Trim();
            if (_schemaquery == "") _schemaquery = "dbo";
            if (_tablequery == null) _tablequery = ""; else _tablequery = _tablequery.Trim();
            if (_fieldquery == null) _fieldquery = ""; else _fieldquery = _fieldquery.Trim();

            if (
                (FKDeep > 0) && 
                (_schema.ToLower() == _schemaquery.ToLower()) &&
                (_table.ToLower() == _tablequery.ToLower()) &&
                (_pk.ToLower() == _fieldquery.ToLower())
            )
            {
                // не добавляем констрейн на саму себя
                return;
            }

            if (FKDeep > MaxFKDeep)
            {
                // не добавляем констрейн большей глубины
                return;
            }

            string key = "";
            if (isUniqueFKTables)
            {
                key = _schema + "." + _table;
                if (FKDeep == 0)
                {
                    key = "start-" + key;
                }
            }
            else
            {
                key = _fieldquery + "-" +_schema + "." + _table;
            }
            key = key.ToLower();
            ForeignKey TableFK;

            if (ListTableFK.ContainsKey(key))
            {
                TableFK = ListTableFK[key];
                if (
                    (_schemaquery != "") &&  //-V3063
                    (_tablequery != "") && (_fieldquery != "") && (_query != "") && (TableFK.FKDeep >= FKDeep)
                    )
                    TableFK.ListFKQuery.Add(new ForeignKeyQuery() { FieldQuery = _fieldquery, Query = _query, SchemaQuery = _schemaquery, TableQuery = _tablequery });
                return;
            }

            TableFK = new ForeignKey(FKDeep, _schema, _table, _pk, _query, _schemaquery, _tablequery, _fieldquery);

            if (TableFK.FKTable != "") 
            {
                ListTableFK.Add(key, TableFK);

                try
                {
                    using (DataTable data = Connect.GetTableFKList(TableFK.FKSchema, TableFK.FKTable))
                    {
                        FKDeep++;
                        foreach (DataRow row in data.Rows)
                        {
                            if ((row["field"].ToString() != "") && (row["fkschema"].ToString() != "") && (row["fktable"].ToString() != "") && (row["fkfield"].ToString() != "") &&
                                (!string.IsNullOrWhiteSpace(TableFK.Query)) && (!string.IsNullOrWhiteSpace(TableFK.FKSchema)) &&
                                (!string.IsNullOrWhiteSpace(TableFK.FKTable)))
                            {
                                AddListTableFK(Connect, FKDeep, row["fkschema"].ToString(), row["fktable"].ToString(), row["fkfield"].ToString(), TableFK.Query, TableFK.FKSchema, TableFK.FKTable, row["field"].ToString(), MaxFKDeep, isUniqueFKTables);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }
        }

        /// <summary>
        /// Заполнить иерархический список внешних ключей начиная с текущей таблицы
        /// </summary>
        /// <param name="Connect">подключение к БД</param>
        /// <param name="MaxFKDeep">маскимальная глубина вложения</param>
        /// <param name="isUniqueFKTables">=true - в итоговом списке будет уникальность по имени таблиця</param>
        public void FillListTableFK(ConnectDB Connect, int MaxFKDeep = 99, bool isUniqueFKTables = true)
        {
            ListTableFK.Clear();
            int FKDeep = 0;
            AddListTableFK(Connect, FKDeep, this.SchemaNameReady, this.TableNameReady, this.UniqueKey, this.SQLQuery, this.SchemaNameReady, this.TableNameReady, this.UniqueKey, MaxFKDeep, isUniqueFKTables);
        }

        /// <summary>
        /// Заполнить текущий экземпляр QueryDB значениями из другого экземпляра QueryDB
        /// </summary>
        /// <param name="_query">экземпляр QueryDB</param>
        public void Fill(QueryDB _query)
        {
            if (_query != null)
            {
                this.GITProject = _query.GITProject;
                this.ScriptType = _query.ScriptType;
                this.FullTableName = _query.FullTableName;
                this.InsUpdDTType = _query.InsUpdDTType;
                this.UniqueKey = _query.UniqueKey;
                this.SQLQuery = _query.SQLQuery;
                this.SQLScript = _query.SQLScript;
                this.isUseInsertUpdate = _query.isUseInsertUpdate;
                this.isAddCheckUnique = _query.isAddCheckUnique;
                this.isAddEmptyString = _query.isAddEmptyString;
                this.isAddDel = _query.isAddDel;
                this.isAddGO = _query.isAddGO;
                this.isAddCreateTable = _query.isAddCreateTable;
                //this.ScriptFilename = _query.ScriptFilename;
                this.isUpdateLocalDBList = _query.isUpdateLocalDBList;
            }
        }

        /// <summary>
        /// Значение поля в текстовом виде для SQL-скрипта
        /// </summary>
        /// <param name="row">строка</param>
        /// <param name="column">поле</param>
        /// <param name="isAddType">добавлять тип поля</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        string ColumnSQLValue(DataRow row, DataColumn column, bool isAddType = false)
        {
            // пропускаем rowversion и timestamp
            if ((column.ColumnName.ToLower().IndexOf("rowversion") != -1) || (column.ColumnName.ToLower().IndexOf("timestamp") != -1))
            {
                string Info = "Необрабатываемый тип данных: " + this.FullTableName + "." + column.ColumnName + " " + column.DataType.FullName;

                if (MessageBox.Show(Info + Environment.NewLine + Environment.NewLine + "В скрипте значение будет изменено на NULL!" + Environment.NewLine + "Продолжить ?", "ВНИМАНИЕ", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    throw new ArgumentException(Info);
                }
                else
                {
                    return "NULL";
                }
            }

            if ((this.InsUpdDTType == Utilities.InsUpdDTType.GETDATE) && (
                column.ColumnName.ToLower().EndsWith("_insdt") ||
                column.ColumnName.ToLower().EndsWith("_upddt")
                ))
            {
                if (
                    (this.TargetDB == Utilities.TargetDBType.EMD) ||
                    (this.TargetDB == Utilities.TargetDBType.PGSQL)
                    )
                {
                    return "localtimestamp";
                }
                else
                {
                    return "getdate()";
                }
            }

            if ((this.InsUpdDTType == Utilities.InsUpdDTType.GETDATE) && (
                column.ColumnName.ToLower().EndsWith("_insdttz") ||
                column.ColumnName.ToLower().EndsWith("_upddttz")
                ))
            {
                if (
                    (this.TargetDB == Utilities.TargetDBType.EMD) ||
                    (this.TargetDB == Utilities.TargetDBType.PGSQL)
                    )
                {
                    return "localtimestamp::TIMESTAMPTZ";
                }
                else
                {
                    return "getdate()";
                }
            }

            if ((this.InsUpdDTType == Utilities.InsUpdDTType.VARI) && (
                column.ColumnName.ToLower().EndsWith("_insdt") ||
                column.ColumnName.ToLower().EndsWith("_upddt")
                ))
            {
                if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                    return "@datetime";
                else
                    return "p_datetime";
            }

            if ((this.InsUpdDTType == Utilities.InsUpdDTType.VARI) && (
                column.ColumnName.ToLower().EndsWith("_insdttz") ||
                column.ColumnName.ToLower().EndsWith("_upddttz")
                ))
            {
                if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                    return "@datetime";
                else
                    return "p_datetime::TIMESTAMPTZ";
            }

            if (
                column.ColumnName.ToLower().EndsWith("_insid") ||
                column.ColumnName.ToLower().EndsWith("_updid")
                )
            {
                return "1";
            }
            else if (((!isAddType) || (this.TargetDB == Utilities.TargetDBType.MSSQL)) && row.IsNull(column))
            {
                return "NULL";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Byte)) ||
                Object.ReferenceEquals(column.DataType, typeof(SByte)) ||
                Object.ReferenceEquals(column.DataType, typeof(Single)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int16)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt16)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int32)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt32)) ||
                Object.ReferenceEquals(column.DataType, typeof(Decimal)) ||
                Object.ReferenceEquals(column.DataType, typeof(Double)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt64)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int64))
                )
            {
                if (isAddType && row.IsNull(column) && (this.TargetDB != Utilities.TargetDBType.MSSQL))
                {
                    return "NULL::integer";
                }
                else
                {
                    return "" + row[column].ToString().Replace(",", ".");
                }
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Char)) ||
                Object.ReferenceEquals(column.DataType, typeof(Guid)) ||
                Object.ReferenceEquals(column.DataType, typeof(String))
                )
            {
                if (isAddType && row.IsNull(column) && (this.TargetDB != Utilities.TargetDBType.MSSQL))
                {
                    return "NULL::varchar";
                }
                else
                {
                    if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                        return "N'" + row[column].ToString().Replace("'", "''") + "'";
                    else
                        return "'" + row[column].ToString().Replace("'", "''") + "'";
                }
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(DateTime)) &&
                (this.InsUpdDTType == Utilities.InsUpdDTType.NONE) &&
                (
                column.ColumnName.ToLower().EndsWith("_insdttz") ||
                column.ColumnName.ToLower().EndsWith("_upddttz") ||
                column.ColumnName.ToLower().EndsWith("_locktimetz")
                )
                )
            {
                if (isAddType && row.IsNull(column) && (this.TargetDB != Utilities.TargetDBType.MSSQL))
                {
                    return "NULL::TIMESTAMPTZ";
                }
                else
                {
                    DateTime d = (DateTime)(row[column]);
                    string s = "'" + d.ToString("yyyy-MM-dd HH:mm:ss.FFF") + "'";
                    if (isAddType && (this.TargetDB != Utilities.TargetDBType.MSSQL))
                        return s + "::TIMESTAMPTZ";
                    else
                        return s;
                }
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(DateTime))
                )
            {
                if (isAddType && row.IsNull(column) && (this.TargetDB != Utilities.TargetDBType.MSSQL))
                {
                    return "NULL::timestamp";
                }
                else
                {
                    DateTime d = (DateTime)(row[column]);
                    string s = "'" + d.ToString("yyyy-MM-dd HH:mm:ss.FFF") + "'";
                    if (isAddType && (this.TargetDB != Utilities.TargetDBType.MSSQL))
                        return s + "::timestamp";
                    else
                        return s;
                }
            }
            if (
                Object.ReferenceEquals(column.DataType, typeof(TimeSpan))
                )
            {
                if (isAddType && row.IsNull(column) && (this.TargetDB != Utilities.TargetDBType.MSSQL))
                {
                    return "NULL::timestamp";
                }
                else
                {
                    TimeSpan t = (TimeSpan)(row[column]);
                    string s = "'" + t/*.ToString(@"hh\:mm\:ss.fff")*/ + "'";
                    if (isAddType && (this.TargetDB != Utilities.TargetDBType.MSSQL))
                        return s + "::timestamp";
                    else
                        return s;
                }
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Boolean))
                )
            {
                string s = row[column].ToString().ToLower();
                if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                {
                    if (s == "true") return "1";
                    else if (s == "false") return "0";
                    else return s;

                }
                else
                {
                    if (s == "true") return "'1'";
                    else if (s == "false") return "'0'";
                    else return "'" + s + "'";
                }
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(byte[]))
                )
            {
                if (isAddType && row.IsNull(column) && (this.TargetDB != Utilities.TargetDBType.MSSQL))
                {
                    return "NULL::BYTEA";
                }
                else
                {
                    byte[] arr = (byte[])row[column];

                    string literal = String.Join("", arr.Select(n => n.ToString("X2")));

                    if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                        return "0x" + literal;
                    else
                        return "decode('" + literal + "', 'hex')";
                }
            }
            else
            {
                string Info = "Необрабатываемый тип данных: " + this.FullTableName + "." + column.ColumnName + " " + column.DataType.FullName;

                if (MessageBox.Show(Info + Environment.NewLine + Environment.NewLine + "В скрипте значение будет изменено на NULL!" + Environment.NewLine + "Продолжить ?", "ВНИМАНИЕ", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    throw new ArgumentException(Info);
                }
                else
                {
                    return "NULL";
                }
            }
        }

        /// <summary>
        /// Значение по умолчанию в текстовом виде для SQL-скрипта
        /// </summary>
        /// <param name="row">строка</param>
        /// <param name="column">поле</param>
        /// <returns></returns>
        string ColumnDefValue(DataRow row, DataColumn column) //-V3203
        {
            if (
                Object.ReferenceEquals(column.DataType, typeof(Byte)) ||
                Object.ReferenceEquals(column.DataType, typeof(SByte)) ||
                Object.ReferenceEquals(column.DataType, typeof(Single)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int16)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt16)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int32)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt32)) ||
                Object.ReferenceEquals(column.DataType, typeof(Decimal)) ||
                Object.ReferenceEquals(column.DataType, typeof(Double)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt64)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int64))
                )
            {
                return "0";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Char)) ||
                Object.ReferenceEquals(column.DataType, typeof(Guid)) ||
                Object.ReferenceEquals(column.DataType, typeof(String))
                )
            {
                return "''";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(DateTime))
                )
            {
                return "'1900-01-01'";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Boolean))
                )
            {
                if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                {
                    return "0";
                }
                else
                {
                    return "'0'";
                }
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(byte[]))
                )
            {
                if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                    return "0";
                else
                    return "decode('0', 'hex')";
            }
            else
            {
                return "0";
            }
        }

        /// <summary>
        /// Тип поля в текстовом виде для SQL-скрипта
        /// </summary>
        /// <param name="column">поле</param>
        /// <returns></returns>
        string ColumnPGType(DataColumn column)
        {
            if (
                Object.ReferenceEquals(column.DataType, typeof(Byte)) ||
                Object.ReferenceEquals(column.DataType, typeof(SByte)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int16)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt16))
                )
            {
                return "smallint";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Int32)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt32))
                )
            {
                return "integer";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(UInt64)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int64))
                )
            {
                return "bigint";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Char))
                )
            {
                return "char";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(DateTime)) &&
                (
                column.ColumnName.ToLower().EndsWith("_insdttz") ||
                column.ColumnName.ToLower().EndsWith("_upddttz") ||
                column.ColumnName.ToLower().EndsWith("_locktimetz")
                )
                )
            {
                return "timestamptz";
            }
            if (
                Object.ReferenceEquals(column.DataType, typeof(DateTime))
                )
            {
                return "timestamp";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Decimal))
                )
            {
                return "numeric";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Double))
                )
            {
                return "double precision";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Single))
                )
            {
                return "real";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Guid))
                )
            {
                return "uuid";
            }

            else if (
                Object.ReferenceEquals(column.DataType, typeof(String))
                )
            {
                return "varchar";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Boolean))
                )
            {
                return "boolean";
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(byte[]))
                )
            {
                return "bytea";
            }
            else
            {
                return "unknown";
            }
        }

        /// <summary>
        /// Значение поля в текстовом виде для CSV-скрипта
        /// </summary>
        /// <param name="row">строка</param>
        /// <param name="column">поле</param>
        /// <param name="now">значение текущей даты\времени в виде строки</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        string ColumnCSVValue(DataRow row, DataColumn column, string now)
        {
            // пропускаем rowversion и timestamp
            if ((column.ColumnName.ToLower().IndexOf("rowversion") != -1) || (column.ColumnName.ToLower().IndexOf("timestamp") != -1))
            {
                string Info = "Необрабатываемый тип данных: " + this.FullTableName + "." + column.ColumnName + " " + column.DataType.FullName;

                if (MessageBox.Show(Info + Environment.NewLine + "В скрипте значение будет изменено на NULL!" + Environment.NewLine + "Продолжить ?", "ВНИМАНИЕ", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    throw new ArgumentException(Info);
                }
                else
                {
                    return "";
                }
            }

            if (((this.InsUpdDTType == Utilities.InsUpdDTType.GETDATE) || (this.InsUpdDTType == Utilities.InsUpdDTType.VARI)) && (
                column.ColumnName.ToLower().EndsWith("_insdt") ||
                column.ColumnName.ToLower().EndsWith("_upddt") ||
                column.ColumnName.ToLower().EndsWith("_insdttz") ||
                column.ColumnName.ToLower().EndsWith("_upddttz")
                ))
            {
                return now;
            }
            else if (
                column.ColumnName.ToLower().EndsWith("_insid") ||
                column.ColumnName.ToLower().EndsWith("_updid")
                )
            {
                return "1";
            }
            else if (row.IsNull(column))
            {
                if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD))
                {
                    return "null";
                }
                else
                {
                    return "";
                }
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Byte)) ||
                Object.ReferenceEquals(column.DataType, typeof(SByte)) ||
                Object.ReferenceEquals(column.DataType, typeof(Single)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int16)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt16)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int32)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt32)) ||
                Object.ReferenceEquals(column.DataType, typeof(Decimal)) ||
                Object.ReferenceEquals(column.DataType, typeof(Double)) ||
                Object.ReferenceEquals(column.DataType, typeof(UInt64)) ||
                Object.ReferenceEquals(column.DataType, typeof(Int64))
                )
            {
                return row[column].ToString().Replace(".", ",");
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Char)) ||
                Object.ReferenceEquals(column.DataType, typeof(Guid)) ||
                Object.ReferenceEquals(column.DataType, typeof(String))
                )
            {
                if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD))
                {
                    return row[column].ToString().Replace("\"","''");
                }
                else
                {
                    return row[column].ToString();
                }
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(DateTime))
                )
            {
                DateTime d = (DateTime)row[column];
                string s = d.ToString("dd.MM.yyyy H:mm:ss");
                return s;
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(Boolean))
                )
            {
                string s = row[column].ToString().ToLower();
                if (s == "true") return "1";
                else if (s == "false") return "0";
                else return s;
            }
            else if (
                Object.ReferenceEquals(column.DataType, typeof(byte[]))
                )
            {
                byte[] arr = (byte[])row[column];

                string literal = string.Join("", arr.Select(n => n.ToString("X2")));

                return "0x" + literal;
            }
            else
            {
                string Info = "Необрабатываемый тип данных: " + this.FullTableName + "." + column.ColumnName + " " + column.DataType.FullName;

                if (MessageBox.Show(Info + Environment.NewLine + "В скрипте значение будет изменено на NULL!" + Environment.NewLine + "Продолжить ?", "ВНИМАНИЕ", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    throw new ArgumentException(Info);
                }
                else
                {
                    return "";
                }
            }
        }

        private string StartRegion(string txtRegion, string region_id, string postfix)
        {
            string result = "";

            if (string.IsNullOrWhiteSpace(txtRegion)) return result;
            if (txtRegion == "0" && (string.IsNullOrWhiteSpace(region_id) || region_id == "NULL")) return result;

            string schema = "dbo";
            var arr = FullTableNameCompare.Split('.');
            if (arr.Length > 1) schema = arr[0];
            bool isSchemaRegion = Utilities.Databases.regex_region.IsMatch(schema.ToLower());

            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                if (txtRegion == "0")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + $"IF (dbo.getregion() = {region_id} OR dbo.GetDBType() = 'db_test')";
                    }
                    else if (this.isLIS)
                    {
                        result +=
                            Environment.NewLine + $"IF (dbo.getregion() = {region_id} OR db_name() IN ({this.ListTestReleaseDB_forRegionalCheck}))";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }
                else if (txtRegion == "100001")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + "IF dbo.GetDBType() = 'db_product'";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }
                else if (txtRegion == "100002")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + "IF dbo.GetDBType() = 'db_prodlike'";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }
                else if (txtRegion == "100003")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + "IF dbo.GetDBType() = 'db_registry'";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }
                else if (txtRegion == "100004")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + "IF dbo.GetDBType() = 'db_report'";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }
                else if (txtRegion == "100005")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + "IF dbo.GetDBType() = 'db_test'";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF db_name() IN ({this.ListTestReleaseDB_full})";
                    }
                }
                else
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + $"IF (dbo.getregion() = {txtRegion} OR dbo.GetDBType() = 'db_test')";
                    }
                    else if (this.isLIS)
                    {
                        result +=
                            Environment.NewLine + $"IF (dbo.getregion() = {txtRegion} OR db_name() IN ({this.ListTestReleaseDB_forRegionalCheck}))";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }

                result += Environment.NewLine + "BEGIN" + postfix;

                if (isSchemaRegion)
                {
                    result += Environment.NewLine + "AND OBJECT_ID(N'" + FullTableNameToScript + "', 'U') IS NOT NULL";
                }

            }
            else
            {
                if (txtRegion == "0")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + $"IF (dbo.getregion() = {region_id} OR dbo.GetDBType() = 'db_test')";
                    }
                    else if (this.isLIS)
                    {
                        result +=
                            Environment.NewLine + $"IF (dbo.getregion() = {region_id} OR current_database() IN ({this.ListTestReleaseDB_forRegionalCheck}))";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }
                else if (txtRegion == "100001")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + "IF dbo.GetDBType() = 'db_product'";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }
                else if (txtRegion == "100002")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + "IF dbo.GetDBType() = 'db_prodlike'";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }
                else if (txtRegion == "100003")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + "IF dbo.GetDBType() = 'db_registry'";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }
                else if (txtRegion == "100004")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + "IF dbo.GetDBType() = 'db_report'";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }
                else if (txtRegion == "100005")
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + "IF dbo.GetDBType() = 'db_test'";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF current_database() IN ({this.ListTestReleaseDB_full})";
                    }
                }
                else
                {
                    if (this.isPromed)
                    {
                        result +=
                            Environment.NewLine + $"IF (dbo.getregion() = {txtRegion} OR dbo.GetDBType() = 'db_test')";
                    }
                    else if (this.isLIS)
                    {
                        result +=
                            Environment.NewLine + $"IF (dbo.getregion() = {txtRegion} OR current_database() IN ({this.ListTestReleaseDB_forRegionalCheck}))";
                    }
                    else
                    {
                        result +=
                            Environment.NewLine + $"IF 1=1";
                    }
                }

                if (isSchemaRegion)
                {
                    result += Environment.NewLine +
                        $"AND EXISTS (SELECT 1 FROM pg_tables WHERE schemaname iLIKE '{this.SchemaNameToSeekForLike}' AND tablename iLIKE '{this.TableNameToSeekForLike}' LIMIT 1)";
                }

                result += Environment.NewLine + "THEN" + postfix;
            }

            return result;
        }

        private string FinishRegion(string txtRegion, string region_id, string prefix)
        {
            string result = "";

            if (string.IsNullOrWhiteSpace(txtRegion)) return result;
            if (txtRegion == "0" && string.IsNullOrWhiteSpace(region_id)) return result;
            if (txtRegion == "0" && (region_id == "NULL")) return prefix;

            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                result += prefix + "END" + Environment.NewLine;
            }
            else
            {
                result += prefix + "END IF;" + Environment.NewLine;
            }

            return result;
        }

        /// <summary>
        /// Собрать SQL-скрипт
        /// </summary>
        /// <param name="win">Окно, из котрого вызвана функция</param>
        /// <param name="Connect">подключение к БД</param>
        /// <param name="isAddTitle">=true - добавить в скрипт заголовок</param>
        /// <param name="file">экземпляр StreamWriter для записи скрипты сразу в файл</param>
        /// <param name="Script">текст сгенерированного скрипта</param>
        /// <param name="isAddRegion">=true - добавить условие региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="isAddComment">добавить в конец скрипты комментарий "Проверка"</param>
        /// <param name="listLocal">список локальных справочников</param>
        /// <param name="cbAddFK">=1 добавить INSERT внешних ключей =2 добавить EXISTS внешних ключей</param>
        /// <exception cref="ArgumentException"></exception>
        public async System.Threading.Tasks.Task<string> GenerateScript(WinQuery win, ConnectDB Connect, bool isAddTitle, StreamWriter file, string Script, bool isAddRegion, string txtRegion, bool isAddComment, List<string> listLocal, int cbAddFK)
        {
            StringBuilder sb = new StringBuilder(100000);

            txtRegion = txtRegion.Trim();

            // заголовок скрипта (информация о задаче)
            if (isAddTitle)
            {
                string TitleScript = MainWindow.Task.TitleScript(this.TargetDB, "data", false, false);
                if (!string.IsNullOrWhiteSpace(TitleScript)) sb.Append(TitleScript);
            }

            if (Script == null) Script = "";

            if (!string.IsNullOrWhiteSpace(Script))
            {
                sb.Append(Script);
                sb.Append(Environment.NewLine);
            }

            bool local_isAddGO = isAddGO;

            // Текст запроса в комментарии
            if (isAddComment && (!string.IsNullOrWhiteSpace(this.SQLQuery)))
            {
                sb.Append(Environment.NewLine + "/* запрос к базе");
                sb.Append(Environment.NewLine + this.SQLQuery.TrimNewLine());
                sb.Append(Environment.NewLine + "*/");
                sb.Append(Environment.NewLine);
            }
            /*
                        if (file != null)
                        {
                            // пишем в файл по частям
                            file.Write(sb.ToString());
                            sb.Clear();
                        }
            */

            string keys = this.UniqueKey;
            string[] UK = keys.ToLower().Split(',');
            for (int i = 0; i < UK.Count(); i++)
            {
                UK[i] = UK[i].Trim();
                if (this.TargetDB == Utilities.TargetDBType.EMD)
                {
                    UK[i] = "\"" + UK[i] + "\"";
                }
            }

            string pk = "";
            bool isIdentity = false;
            try
            {
                if (Connect != null)
                {
                    var pkinfo = Connect.GetTablePK(this.FullTableNameReady);

                    pk = pkinfo.FieldNamesToString;
                    isIdentity = pkinfo.HasIdentity;
                }
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                pk = "";
            }

            if (string.IsNullOrWhiteSpace(pk)) pk = Utilities.Databases.GetTableName(this.FullTableNameReady) + "_id";

            bool isPK = Utilities.Databases.IsPK(keys, pk);

            // Добавить формат даты
            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                sb.Append(Environment.NewLine + "SET DATEFORMAT ymd" + Environment.NewLine);
            }

            // добавить объявление переменных 
            if (isAddRegion || (InsUpdDTType == Utilities.InsUpdDTType.VARI) || (ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP) || (isAddCheckUnique && ScriptType == Utilities.ScriptType.INSERT))
            {
                if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD))
                {
                    sb.Append(Environment.NewLine + "DO $script$");
                    sb.Append(Environment.NewLine + "DECLARE");
                    sb.Append(Environment.NewLine + "\tp_datetime timestamp = localtimestamp;");
                    //sb.Append(Environment.NewLine + "\tp_datetimetz timestamptz = p_datetime::timestamptz;");
                    if ((ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP))
                    {
                        sb.Append(Environment.NewLine + "\tp_rc INT;");
                        sb.Append(Environment.NewLine + "\tp_offset INT;");
                    }
                    sb.Append(Environment.NewLine + "BEGIN" + Environment.NewLine);
                }
            }

            if ((InsUpdDTType == Utilities.InsUpdDTType.VARI) && (this.TargetDB == Utilities.TargetDBType.MSSQL))
            {
                sb.Append(Environment.NewLine + "DECLARE @datetime DATETIME = dbo.tzgetdate()" + Environment.NewLine);
                local_isAddGO = false;
            }

            if (((ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP)) && (this.TargetDB == Utilities.TargetDBType.MSSQL))
            {
                sb.Append(Environment.NewLine + "DECLARE @rc INT");
                sb.Append(Environment.NewLine + "DECLARE @offset INT" + Environment.NewLine);
                local_isAddGO = false;
            }

            // Проверка на региональность в начале скрипта
            if (isAddRegion)
            {
                sb.Append(StartRegion(txtRegion, "", Environment.NewLine));
                local_isAddGO = false;
            }

            // добавляем создание таблицы
            TableDB table = new TableDB();
            if (isAddCreateTable || (ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP))
            {
                table.GITProject = this.GITProject;
                table.ScriptType = Utilities.ScriptType.CREATE;
                if ((ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP))
                {
                    table.TableType = Utilities.TableType.TEMP;
                    table.TableEdit.SchemaName = "";
                    table.TableEdit.TableName = "tmp_" + this.TableNameReady;
                }
                else
                {
                    table.TableType = Utilities.TableType.DICT;
                    table.TableEdit.SchemaName = this.SchemaNameToScript;
                    table.TableEdit.TableName = this.TableNameToScript;
                }
                table.isAddDrop = false;

                foreach (DataColumn item in this.DataTableUsedColumns)
                {
                    string _size = item.MaxLength.ToString();
                    if (item.DataType.ToString().ToUpper() == "SYSTEM.STRING") _size = "max";

                    table.TableEdit.AddField(
                        item.Ordinal.ToString(),
                        item.ColumnName,
                        item.DataType.ToString(),
                        _size,
                        "",
                        "",
                        (item.AllowDBNull != true) ? "true" : "else",
                        (item.AutoIncrement == true) ? "true" : "else",
                        (item.Unique == true) ? "true" : "else");
                }


                List<string> ProcCommand = new List<string>();
                int ProcCommandNum = -1;

                if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                {
                    if (table.TableType == Utilities.TableType.TEMP)
                    {
                        sb.Append(Environment.NewLine + "IF OBJECT_ID('tempdb.." + table.TableEdit.FullTableNameToScript + "') IS NOT NULL");
                        sb.Append(Environment.NewLine + "BEGIN");
                        sb.Append(Environment.NewLine + "\tDROP TABLE IF EXISTS " + table.TableEdit.FullTableNameToScript);
                        sb.Append(Environment.NewLine + "END");
                    }
                    else
                    {
                        sb.Append(Environment.NewLine + "--DROP TABLE IF EXISTS " + table.TableEdit.FullTableNameToScript);
                    }
                }
                else
                {
                    if (table.TableType == Utilities.TableType.TEMP)
                    {
                        sb.Append(Environment.NewLine + "DROP TABLE IF EXISTS " + table.TableEdit.FullTableNameToScript + ";");
                    }
                    else
                    {
                        sb.Append(Environment.NewLine + "--DROP TABLE IF EXISTS " + table.TableEdit.FullTableNameToScript + ";");
                    }
                }

                table.isOnlyExist = true;
                sb.Append(Environment.NewLine + TableDB.GenerateTableScript(Connect, false, false, "", out ProcCommand, out ProcCommandNum, false, out string RowInfo, table));
            }

            // хинт для MS SQL
            string mshint_change = "";
            if (this.TargetDB == Utilities.TargetDBType.MSSQL) mshint_change = "WITH (rowlock) ";
            string mshint_sel = "";
            if (this.TargetDB == Utilities.TargetDBType.MSSQL) mshint_sel = "WITH (nolock) ";

            // концовка оператора INSERT
            string insert_end = "";
            if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD))
            {
                if (isPK)
                    insert_end = " ON CONFLICT DO NOTHING;";
                else
                    insert_end = ";";
            }
            else if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                if (local_isAddGO) 
                    insert_end += Environment.NewLine + "GO";
            }

            // концовка оператора UPDATE
            string update_end = "";
            if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD)) update_end = ";";

            // концовка оператора DELETE
            string delete_end = "";
            if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD)) delete_end = ";";

            // концовка оператора SELECT
            string select_end = "";
            if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD)) select_end = ";";

            // лимит
            string limit_5000_ms = "";
            string limit_5000_pg = "";
            int limit_count = 5000;
            string limit_1_ms = "";
            string limit_1_pg = "";

            if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD))
            {
                limit_5000_ms = "";
                limit_5000_pg = "LIMIT " + limit_count.ToString();

                limit_1_ms = "";
                limit_1_pg = " LIMIT 1";
            }
            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                limit_5000_ms = "TOP(" + limit_count.ToString() + ") ";
                limit_5000_pg = "";

                if (isPK) limit_1_ms = "TOP(1) ";
                limit_1_pg = "";
            }

            bool isFinishNewLine = false;
            int CountRowsInValues = 0;
            int MaxRowsInValues = 1000;
            if (isAddCheckUnique && (this.TargetDB == Utilities.TargetDBType.MSSQL)) MaxRowsInValues = 200;

            string fields = "";
            string tmp_fields = "";
            string where = "";
            string target_is_null = "";
            string tmp_updvalues = "";
            string tmp_wherevalues = "";
            string order = "";

            // Перед INSERT, INSERT(VALUES) или INSERT/UPDATE
            if ((ScriptType == Utilities.ScriptType.INSERT) || (ScriptType == Utilities.ScriptType.INSERT_VALUES) || (ScriptType == Utilities.ScriptType.UPSERT))
            {
                if ((this.TargetDB == Utilities.TargetDBType.MSSQL) && isPK && isIdentity)
                {
                    sb.Append(Environment.NewLine + "SET IDENTITY_INSERT " + this.FullTableNameToScript + " ON");
                    if (local_isAddGO)
                        sb.Append(Environment.NewLine + "GO" + Environment.NewLine);
                    else
                        sb.Append(Environment.NewLine);
                }
            }

            if ((!isPK) && (this.TargetDB == Utilities.TargetDBType.PGSQL))
            {
                // в скриптах, где идет вставка без явного первичного ключа, добавлять сдвиг сиквенса.
                if (isAddRegion || (InsUpdDTType == Utilities.InsUpdDTType.VARI))
                    sb.Append(Environment.NewLine + "PERFORM dbo.xp_alter_sequence(table_name := '" + FullTableNameToScript + "', isexec := 2);" +
                        Environment.NewLine);
                else
                    sb.Append(Environment.NewLine + "SELECT dbo.xp_alter_sequence(table_name := '" + FullTableNameToScript + "', isexec := 2);" +
                        Environment.NewLine);
            }

            if (file != null)
            {
                // пишем в файл по частям
                file.Write(sb.ToString().ScriptPartReady());
                sb.Clear();
            }

            // сортируем строки по полю Region_id
            if (txtRegion == "0")
            {
                foreach (DataColumn column in this.DataTable.Columns)
                {
                    if (column.ColumnName.ToLower() == "region_id")
                    {
                        this.DataTable.DefaultView.Sort = "region_id ASC";
                        this.DataTable = this.DataTable.DefaultView.ToTable();

                        break;
                    }
                }
            }

            string lastRegion_id = "";
            string Region_id = "";
            bool isBreak = false;

            // отображаем progress bar
            int current = 0;
            int maximum = this.DataTable.Rows.Count;
            int step = this.DataTable.Rows.Count / 100;
            if (step == 0) step = 1;
            WinProgress winProgress = new WinProgress();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (win != null)
                {
                    win.pbProgress.Minimum = 0;
                    win.pbProgress.Maximum = 100;
                    win.pbProgress.Value = 0;
                }
                else
                {
                    winProgress.pbProgress.Minimum = 0;
                    winProgress.pbProgress.Maximum = 100;
                    winProgress.pbProgress.Value = 0;
                    winProgress.Show();
                }
            });

            await System.Threading.Tasks.Task.Run(() =>
            {
                // Перебираем строки
                foreach (DataRow row in this.DataTable.Rows)
                {
                    string addLine = "";
                    fields = "";
                    tmp_fields = "";
                    string insvalues = "";
                    string insvalues_typenull = "";
                    string updvalues = "";
                    where = "";
                    string keyvalues = "";
                    target_is_null = "";
                    tmp_updvalues = "";
                    tmp_wherevalues = "";
                    order = "";

                    string ifExists = "";

                    // для PG SQL имя колонки в нижний регистр
                    if (this.TargetDB == Utilities.TargetDBType.PGSQL) keys = keys.ToLower();

                    isBreak = false;

                    foreach (DataColumn column in this.DataTableUsedColumns)
                    {
                        string ColumnName = column.ColumnName;
                        if (this.TargetDB == Utilities.TargetDBType.EMD)
                        {
                            ColumnName = "\"" + ColumnName + "\"";
                        }

                        // для PG SQL имя колонки в нижний регистр
                        if (this.TargetDB == Utilities.TargetDBType.PGSQL) ColumnName = ColumnName.ToLower();

                        // пропускаем rowversion
                        if (ColumnName.ToLower().IndexOf("rowversion") != -1) continue;
                        if (ColumnName.ToLower().IndexOf("timestamp") != -1) continue;

                        if ((!isAddDel) && ColumnName.ToLower().EndsWith("_deleted")) continue;
                        if ((!isAddDel) && ColumnName.ToLower().EndsWith("_delid")) continue;
                        if ((!isAddDel) && ColumnName.ToLower().EndsWith("_deldt")) continue;
                        if ((!isAddDel) && ColumnName.ToLower().EndsWith("_deldttz")) continue;

                        if ((!isPK) && Utilities.Databases.IsPK(pk, ColumnName))
                        {
                            // если в качестве уникальных полей выбран НЕ primary key - не включаем primary key в список полей 
                        }
                        else
                        {
                            string field_value = ColumnSQLValue(row, column);

                            if (
                                (cbAddFK == 2) &&
                                (field_value != "NULL")
                            )
                            {
                                // добавить проверку на существование внешнего ключа
                                foreach (var item in this.ListTableFK
                                           .Where(x => 
                                                x.Value.FKDeep == 1 &&
                                                x.Value.ParentField.ToLower() == column.ColumnName.ToLower()
                                           )
                                       )
                                {
                                    ifExists += item.Value
                                        .IfExistsCondition(field_value, limit_1_ms, mshint_sel, limit_1_pg);
                                    break;
                                }
                            }

                            if (
                                isAddRegion &&
                                (txtRegion == "0") &&
                                (ColumnName.ToLower() == "region_id")
                            )
                            {
                                Region_id = field_value;
                            }

                            if (fields != "") fields = fields + ", ";
                            fields = fields + ColumnName;

                            if (tmp_fields != "") tmp_fields = tmp_fields + ", ";
                            tmp_fields = tmp_fields + "source." + ColumnName;

                            if (insvalues != "") insvalues = insvalues + ", ";
                            insvalues = insvalues + field_value;

                            if (insvalues_typenull != "") insvalues_typenull = insvalues_typenull + ", ";
                            insvalues_typenull = insvalues_typenull + ColumnSQLValue(row, column, true);

                            bool isUK = Array.IndexOf(UK, ColumnName.ToLower()) != -1;

                            if ((!ColumnName.ToLower().EndsWith("_insid")) &&
                                 (!ColumnName.ToLower().EndsWith("_insdt")) &&
                                 (!ColumnName.ToLower().EndsWith("_insdttz")) &&
                                 (!isUK)
                                )
                            {
                                if (tmp_updvalues != "") tmp_updvalues = tmp_updvalues + "," + Environment.NewLine;
                                tmp_updvalues += "\t\t" + ColumnName + " = " + "source." + ColumnName;
                            }

                            if ((!ColumnName.ToLower().EndsWith("_insid")) &&
                                 (!ColumnName.ToLower().EndsWith("_insdt")) &&
                                 (!ColumnName.ToLower().EndsWith("_insdttz")) &&
                                 (!ColumnName.ToLower().EndsWith("_updid")) &&
                                 (!ColumnName.ToLower().EndsWith("_upddt")) &&
                                 (!ColumnName.ToLower().EndsWith("_upddttz")) &&
                                 (!ColumnName.ToLower().EndsWith("_delid")) &&
                                 (!ColumnName.ToLower().EndsWith("_deldt")) &&
                                 (!ColumnName.ToLower().EndsWith("_deldttz")) &&
                                 (!isUK)
                                )
                            {
                                if (tmp_wherevalues != "") tmp_wherevalues = tmp_wherevalues + " OR" + Environment.NewLine;
                                string def = ColumnDefValue(row, column);

                                if (
                                    (this.TargetDB == Utilities.TargetDBType.MSSQL) &&
                                    (
                                    Object.ReferenceEquals(column.DataType, typeof(Char)) ||
                                    Object.ReferenceEquals(column.DataType, typeof(String))
                                    )
                                )
                                {
                                    tmp_wherevalues += "\t\t\tCOALESCE(target." + ColumnName + " COLLATE Cyrillic_General_CS_AS, " + def + ") <> COALESCE(source." + ColumnName + " COLLATE Cyrillic_General_CS_AS, " + def + ")";
                                }
                                else
                                {
                                    tmp_wherevalues += "\t\t\tCOALESCE(target." + ColumnName + ", " + def + ") <> COALESCE(source." + ColumnName + ", " + def + ")";
                                }
                            }

                            if (isUK)
                            {
                                if (where != "") where = where + " AND ";
                                if (target_is_null != "") target_is_null = target_is_null + " AND ";
                                if (order != "") order = order + ", ";

                                if (((ScriptType == Utilities.ScriptType.UPSERT) && (!isUseInsertUpdate)) || (ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP) || (ScriptType == Utilities.ScriptType.INSERT_VALUES)) // для MERGE или INSERT_TMP или UPSERT_TMP или INSERT_VALUES
                                {
                                    where = where + "target." + ColumnName + " = source." + ColumnName;
                                    target_is_null = target_is_null + "target." + ColumnName + " IS NULL";
                                    order = order + ColumnName;
                                }
                                else // для UPDATE или DELETE или INSERT
                                {
                                    where = where + ColumnName + " = " + field_value;
                                }

                                if (keyvalues != "") keyvalues = keyvalues + ", ";
                                keyvalues = keyvalues + field_value;
                            }
                            else
                            {

                                if (
                                    (!ColumnName.ToLower().EndsWith("_insdt")) &&
                                    (!ColumnName.ToLower().EndsWith("_insid")) &&
                                    (!ColumnName.ToLower().EndsWith("_insdttz"))
                                    ) // поля _insdt, _insid, _insdttz не обновляем
                                {
                                    if (updvalues != "") updvalues = updvalues + ", ";
                                    updvalues = updvalues + ColumnName + " = " + field_value;
                                }
                            }
                        }
                    }

                    // проверка на региональность - первая строка
                    if (
                        (lastRegion_id != Region_id) &&
                        (lastRegion_id == "")
                    )
                    {
                        string reg = StartRegion(txtRegion, Region_id, Environment.NewLine);
                        if (!string.IsNullOrWhiteSpace(reg)) sb.Append(Environment.NewLine);
                        sb.Append(reg);
                        lastRegion_id = Region_id;
                    }

                    // разрыв региональности
                    if (
                        (lastRegion_id != Region_id) &&
                        (lastRegion_id != "")
                    )
                    {
                        isBreak = true;
                    }

                    // INSERT, INSERT(VALUES), INSERT(TMP) или INSERT+UPDATE
                    if ((ScriptType == Utilities.ScriptType.INSERT) || (ScriptType == Utilities.ScriptType.INSERT_VALUES) || (ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP) || ((ScriptType == Utilities.ScriptType.UPSERT) && isUseInsertUpdate)
                       )
                    {
                        if ((ScriptType == Utilities.ScriptType.INSERT_VALUES) || (ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP))
                        {
                            if (CountRowsInValues == MaxRowsInValues || isBreak)
                            {
                                if (isAddCheckUnique && (ScriptType == Utilities.ScriptType.INSERT_VALUES))
                                {
                                    addLine += ") source (" + fields + ")";
                                    addLine += Environment.NewLine + "LEFT JOIN " + FullTableNameToScript + " target " + mshint_sel + "ON " + where;
                                    addLine += Environment.NewLine + "WHERE " + target_is_null + select_end;

                                    if (cbAddFK == 2)
                                    {
                                        // добавить проверку на существование внешних ключей
                                        foreach (var item in this.ListTableFK
                                            .Where(x => x.Value.FKDeep == 1)
                                            )
                                        {
                                            addLine += item.Value
                                            .WhereExistsCondition("", "source", limit_1_ms, mshint_sel, limit_1_pg);
                                        }
                                    }

                                    addLine += Environment.NewLine;
                                }
                                else
                                {
                                    addLine += insert_end.Trim() + Environment.NewLine;
                                }

                                /*
                                if (isAddCheckUnique && (ScriptType != Utilities.ScriptType.INSERT_TMP) && (ScriptType != Utilities.ScriptType.UPSERT_TMP))
                                {
                                    if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD)) // для PG SQL
                                    {
                                        //addLine += Environment.NewLine + "END IF;";
                                    }
                                }
                                */

                                CountRowsInValues = 0;
                            }

                        }
                    }

                    // разрыв региональности в середине скрипта
                    if (isBreak)
                    {
                        addLine += FinishRegion(txtRegion, lastRegion_id, (this.isAddEmptyString ? "" : Environment.NewLine));

                        addLine += StartRegion(txtRegion, Region_id, (this.isAddEmptyString ? "" : Environment.NewLine));

                        lastRegion_id = Region_id;
                    }

                    // INSERT, INSERT(VALUES), INSERT(TMP) или INSERT+UPDATE
                    if ((ScriptType == Utilities.ScriptType.INSERT) || (ScriptType == Utilities.ScriptType.INSERT_VALUES) || (ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP) || ((ScriptType == Utilities.ScriptType.UPSERT) && isUseInsertUpdate)
                       )
                    {

                        if (addLine != "") //-V3022
                        {
                            addLine += Environment.NewLine;
                            if (isAddEmptyString) addLine += Environment.NewLine;
                        }


                        if (isAddCheckUnique && (ScriptType != Utilities.ScriptType.INSERT_TMP) && (ScriptType != Utilities.ScriptType.UPSERT_TMP) &&
                                (
                                    (ScriptType != Utilities.ScriptType.INSERT_VALUES)  /*||
                                (CountRowsInValues == 0)*/
                                )
                            )
                        {
                            if (this.TargetDB == Utilities.TargetDBType.MSSQL) // для MS SQL
                            {
                                addLine += "IF NOT EXISTS (SELECT TOP(1) 1 FROM " + FullTableNameToExistScript + " " + mshint_sel + "WHERE " + where + ")";

                                if (cbAddFK == 2)
                                {
                                    // добавить проверку на существование внешних ключей
                                    addLine += ifExists;
                                }

                                addLine += Environment.NewLine;
                            }

                            if (
                                (this.TargetDB == Utilities.TargetDBType.PGSQL) || 
                                (this.TargetDB == Utilities.TargetDBType.EMD)
                                ) // для PG SQL
                            {
                                addLine += "IF NOT EXISTS (SELECT 1 FROM " + FullTableNameToExistScript + " " + mshint_sel + "WHERE " + where + " limit 1)";

                                if (cbAddFK == 2)
                                {
                                    // добавить проверку на существование внешних ключей
                                    addLine += ifExists;
                                }

                                addLine += " THEN" + Environment.NewLine;
                            }
                        }


                        if ((ScriptType == Utilities.ScriptType.INSERT_VALUES) || (ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP))
                        {
                            if (CountRowsInValues == 0)
                            {
                                if ((ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP))
                                {
                                    addLine +=
                                    "-- Пакетами по " + MaxRowsInValues.ToString() + Environment.NewLine +
                                    "INSERT INTO " + table.TableEdit.FullTableNameToScript + " (" + fields + ") " + Environment.NewLine +
                                    "VALUES " + Environment.NewLine +
                                    "    (" + insvalues + ")";

                                }
                                else
                                {
                                    addLine +=
                                    "-- Пакетами по " + MaxRowsInValues.ToString() + Environment.NewLine +
                                    "INSERT INTO " + FullTableNameToScript + " " + mshint_change + "(" + fields + ") " + Environment.NewLine;

                                    // дополнения в скрипте для проверки на уникальность при INSERT(VALUES)
                                    if (isAddCheckUnique)
                                    {
                                        addLine +=
                                        "SELECT " + tmp_fields + Environment.NewLine +
                                        "FROM (" + Environment.NewLine;

                                        addLine += "VALUES " + Environment.NewLine +
                                        "    (" + insvalues_typenull + ")";
                                    }
                                    else
                                    {
                                        addLine += "VALUES " + Environment.NewLine +
                                        "    (" + insvalues + ")";
                                    }
                                }
                            }
                            else
                            {
                                addLine += "   ,(" + insvalues + ")";
                            }

                            CountRowsInValues++;
                        }
                        else
                        {
                            addLine +=
                            "INSERT INTO " + FullTableNameToScript + " " + mshint_change + "(" + fields + ") " + Environment.NewLine +
                            "VALUES (" + insvalues + ")" + insert_end;

                            if (isAddCheckUnique && (ScriptType != Utilities.ScriptType.INSERT_TMP) && (ScriptType != Utilities.ScriptType.UPSERT_TMP)) //-V3063
                            {
                                if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD)) // для PG SQL
                                {
                                    addLine += Environment.NewLine + "END IF;";
                                }
                            }
                        }
                    }

                    // MERGE
                    if ((ScriptType == Utilities.ScriptType.UPSERT) && (!isUseInsertUpdate))
                    {
                        if (where == "") throw new ArgumentException($"В данных для скрипта не найдены поля: " + keys + " !");

                        if (this.TargetDB == Utilities.TargetDBType.MSSQL) // для MS SQL
                        {
                            if (addLine != "")
                            {
                                addLine += Environment.NewLine;
                                if (isAddEmptyString) addLine += Environment.NewLine;
                            }

                            addLine += "MERGE " + FullTableNameToScript + " " + mshint_change + "AS target " +
                            Environment.NewLine + "USING (SELECT " + keyvalues + ") AS source (" + keys + ")" +
                            Environment.NewLine + "ON " + where;

                            if (cbAddFK == 2)
                            {
                                // добавить проверку на существование внешних ключей
                                addLine += ifExists;
                            }

                            addLine += Environment.NewLine + "WHEN MATCHED THEN UPDATE SET " + updvalues +
                            Environment.NewLine + "WHEN NOT MATCHED BY TARGET THEN INSERT (" + fields + ")" +
                            Environment.NewLine + "VALUES (" + insvalues + ")";
                        }

                        if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD)) // для PG SQL
                        {
                            if (addLine != "")
                            {
                                addLine += Environment.NewLine;
                                if (isAddEmptyString) addLine += Environment.NewLine;
                            }

                            addLine += "INSERT INTO " + FullTableNameToScript + " (" + fields + ") " +
                            Environment.NewLine + "VALUES (" + insvalues + ") " +
                            Environment.NewLine + "ON CONFLICT (" + keys + ") DO UPDATE SET " + updvalues;

                            if (cbAddFK == 2)
                            {
                                // добавить проверку на существование внешних ключей
                                addLine += Environment.NewLine + "WHERE 1 = 1" + ifExists;
                            }

                            addLine += ";";
                        }
                    }

                    // UPDATE или INSERT+UPDATE
                    if ((ScriptType == Utilities.ScriptType.UPDATE) ||
                         ((ScriptType == Utilities.ScriptType.UPSERT) && isUseInsertUpdate)
                       )
                    {
                        if (where == "") throw new ArgumentException($"В данных для скрипта не найдены поля: " + keys + " !");

                        if (addLine != "")
                        {
                            addLine += Environment.NewLine;
                            if (isAddEmptyString) addLine += Environment.NewLine;
                        }

                        addLine += "UPDATE " + limit_1_ms + FullTableNameToScript + " " + mshint_change +
                        Environment.NewLine + "SET " + updvalues + " " +
                        Environment.NewLine + "WHERE " + where;

                        if (cbAddFK == 2)
                        {
                            // добавить проверку на существование внешних ключей
                            addLine += ifExists;
                        }

                        addLine += update_end;

                    }

                    // DELETE
                    if (ScriptType == Utilities.ScriptType.DELETE)
                    {
                        if (where == "") throw new ArgumentException($"В данных для скрипта не найдены поля: " + keys + " !");

                        if (addLine != "")
                        {
                            addLine += Environment.NewLine;
                            if (isAddEmptyString) addLine += Environment.NewLine;
                        }

                        addLine += "DELETE " + limit_1_ms + "FROM " + FullTableNameToScript + " " + mshint_change + "WHERE " + where + delete_end;
                    }

                    if (!string.IsNullOrWhiteSpace(addLine))
                    {
                        addLine = Environment.NewLine + addLine;

                        if (((ScriptType == Utilities.ScriptType.INSERT_VALUES) || (ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP)) && (CountRowsInValues != 0))
                        {
                            // не добавляем перевод строки при вставке блоками, только после блока целиком
                        }
                        else
                        {
                            if (isAddEmptyString) addLine += Environment.NewLine;
                        }

                        isFinishNewLine = addLine.EndsWith(Environment.NewLine);

                        sb.Append(addLine);

                        if ((file != null) && ((sb.Length + addLine.Length) > (sb.Capacity - 10)))
                        {
                            // пишем в файл по частям
                            file.Write(sb.ToString().ScriptPartReady());
                            sb.Clear();
                        }
                    }

                    // обновляем progress bar
                    current++;
                    if (current % step == 0)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (win != null)
                            {
                                win.pbProgress.Value = (int)((float)current / (float)maximum * 100);
                            }
                            else
                            {
                                winProgress.pbProgress.Value = (int)((float)current / (float)maximum * 100);
                            }
                        });

                        if (win != null)
                        {
                            if (win.isCancelGenereate)
                            {
                                return;
                            }
                        }
                    }
                }
            });

            if (file != null)
            {
                // пишем в файл по частям
                file.Write(sb.ToString().ScriptPartReady());
                sb.Clear();
            }

            if (win != null)
            {
                if (win.isCancelGenereate)
                {
                    return "";
                }
            }

            // добавить перевод строки если его нет
            if (!isFinishNewLine)
            {
                sb.Append(Environment.NewLine);
            }

            // После INSERT(VALUES) или INSERT(TMP)
            if (((ScriptType == Utilities.ScriptType.INSERT_VALUES) || (ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP)) && (CountRowsInValues > 0))
            {
                if (isAddCheckUnique && (ScriptType == Utilities.ScriptType.INSERT_VALUES) /*&& (this.TargetDB == Utilities.TargetDBType.MSSQL)*/)
                {
                    sb.Append(") source (" + fields + ")");
                    sb.Append(Environment.NewLine + "LEFT JOIN " + FullTableNameToScript + " target " + mshint_sel + "ON " + where);
                    sb.Append(Environment.NewLine + "WHERE " + target_is_null + select_end);

                    if (cbAddFK == 2)
                    {
                        // добавить проверку на существование внешних ключей
                        foreach (var item in this.ListTableFK
                            .Where(x => x.Value.FKDeep == 1)
                            )
                        {
                            sb.Append(item.Value.WhereExistsCondition("", "source", limit_1_ms, mshint_sel, limit_1_pg));
                        }
                    }

                    sb.Append(Environment.NewLine);
                }
                else
                {
                    sb.Append(insert_end.Trim() + Environment.NewLine);
                }

                if (isAddCheckUnique && (ScriptType != Utilities.ScriptType.INSERT_TMP) && (ScriptType != Utilities.ScriptType.UPSERT_TMP))
                {
                    if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD)) // для PG SQL
                    {
                        //sb.Append(Environment.NewLine + "END IF;");
                        //sb.Append(Environment.NewLine);
                    }
                }
            }

            // финальный разрыв региональности
            if (Region_id != "")
            {
                sb.Append(FinishRegion(txtRegion, Region_id, (this.isAddEmptyString ? "" : Environment.NewLine)));
            }

            // После INSERT, INSERT(VALUES) или INSERT/UPDATE
            if ((ScriptType == Utilities.ScriptType.INSERT) || (ScriptType == Utilities.ScriptType.INSERT_VALUES) || (ScriptType == Utilities.ScriptType.UPSERT))
            {
                if ((this.TargetDB == Utilities.TargetDBType.MSSQL) && isPK && isIdentity)
                {
                    sb.Append(Environment.NewLine + "SET IDENTITY_INSERT " + FullTableNameToScript + " OFF");
                    if (local_isAddGO)
                        sb.Append(Environment.NewLine + "GO" + Environment.NewLine);
                    else
                        sb.Append(Environment.NewLine);
                }
            }

            // После INSERT(TMP) или INSERT+UPDATE(TMP)
            if ((ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP))
            {
                if (ScriptType == Utilities.ScriptType.UPSERT_TMP)
                {
                    if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                    {
                        sb.Append(Environment.NewLine + "-- Обновить");
                        sb.Append(Environment.NewLine + "SET NOCOUNT ON");
                        sb.Append(Environment.NewLine + "SET @rc = 1");
                        sb.Append(Environment.NewLine + "SET @offset = 0");
                        sb.Append(Environment.NewLine + "WHILE @rc > 0");
                        sb.Append(Environment.NewLine + "BEGIN" + Environment.NewLine);
                    }
                    else
                    {
                        sb.Append(Environment.NewLine + "-- Обновить");
                        sb.Append(Environment.NewLine + "p_rc := 1;");
                        sb.Append(Environment.NewLine + "p_offset := 0;");
                        sb.Append(Environment.NewLine + "WHILE p_rc > 0");
                        sb.Append(Environment.NewLine + "LOOP" + Environment.NewLine);
                    }

                    sb.Append(Environment.NewLine +
                        "\tWITH source AS (" + Environment.NewLine +
                        "\t\tSELECT " + limit_5000_ms + "source.*" + Environment.NewLine +
                        "\t\tFROM " + table.TableEdit.FullTableNameToScript + " source" + Environment.NewLine +
                        "\t\tINNER JOIN " + this.FullTableNameToScript + " target " + mshint_change + " ON " + where + Environment.NewLine +
                        "\t\t-- обновляем только отличающиеся" + Environment.NewLine +
                        "\t\tWHERE (" + Environment.NewLine +
                        tmp_wherevalues + Environment.NewLine +
                        "\t\t)" + Environment.NewLine
                    );

                    if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                    {
                        sb.Append(
                        "\t\t-- ORDER BY " + order + Environment.NewLine +
                        "\t\t-- OFFSET @offset ROWS FETCH NEXT " + limit_count.ToString() + " ROWS ONLY" + Environment.NewLine
                        );

                        sb.Append(
                            "\t)" + Environment.NewLine +
                            "\tUPDATE target" + Environment.NewLine +
                            "\tSET " + Environment.NewLine +
                            tmp_updvalues + Environment.NewLine +
                            "\tFROM source" + Environment.NewLine +
                            "\tINNER JOIN " + this.FullTableNameToScript + " target " + mshint_change + " ON " + where
                        );

                        if (cbAddFK == 2)
                        {
                            // добавить проверку на существование внешних ключей
                            sb.Append(Environment.NewLine + "\tWHERE 1 = 1");
                            foreach (var item in this.ListTableFK
                                .Where(x => x.Value.FKDeep == 1)
                                )
                            {
                                sb.Append(item.Value.WhereExistsCondition("\t", "source", limit_1_ms, mshint_sel, limit_1_pg));
                            }
                        }

                        sb.Append(Environment.NewLine);

                        sb.Append(Environment.NewLine + "\tSET @rc = @@rowcount");
                        sb.Append(Environment.NewLine + "\t-- SET @offset = @offset + " + limit_count.ToString());
                        sb.Append(Environment.NewLine + "END" + Environment.NewLine);
                    }
                    else
                    {
                        sb.Append(
                        "\t\t-- ORDER BY " + order + Environment.NewLine +
                        "\t\t" + limit_5000_pg + Environment.NewLine +
                        "\t\t-- OFFSET p_offset" + Environment.NewLine
                        );

                        sb.Append(
                            "\t)" + Environment.NewLine +
                            "\tUPDATE " + this.FullTableNameToScript + " target " + mshint_change + Environment.NewLine +
                            "\tSET " + Environment.NewLine +
                            tmp_updvalues + Environment.NewLine +
                            "\tFROM source" + Environment.NewLine +
                            "\tWHERE " + where
                        );

                        if (cbAddFK == 2)
                        {
                            // добавить проверку на существование внешних ключей
                            foreach (var item in this.ListTableFK
                                .Where(x => x.Value.FKDeep == 1)
                                )
                            {
                                sb.Append(item.Value.WhereExistsCondition("\t","source", limit_1_ms, mshint_sel, limit_1_pg));
                            }
                        }

                        sb.Append(";" + Environment.NewLine);

                        sb.Append(Environment.NewLine + "\tget diagnostics p_rc := row_count;");
                        sb.Append(Environment.NewLine + "\t-- p_offset := p_offset + " + limit_count.ToString() + ";");
                        sb.Append(Environment.NewLine + "END LOOP;" + Environment.NewLine);
                    }
                }

                if ((this.TargetDB == Utilities.TargetDBType.MSSQL) && isPK && isIdentity)
                {
                    sb.Append(Environment.NewLine + "SET IDENTITY_INSERT " + this.FullTableNameToScript + " ON");
                    sb.Append(Environment.NewLine);
                }

                if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                {
                    sb.Append(Environment.NewLine + "-- Добавить");
                    sb.Append(Environment.NewLine + "SET NOCOUNT ON");
                    sb.Append(Environment.NewLine + "SET @rc = 1");
                    sb.Append(Environment.NewLine + "WHILE @rc > 0");
                    sb.Append(Environment.NewLine + "BEGIN" + Environment.NewLine);
                }
                if (this.TargetDB == Utilities.TargetDBType.PGSQL)
                {
                    sb.Append(Environment.NewLine + "-- Добавить");
                    sb.Append(Environment.NewLine + "p_rc := 1;");
                    sb.Append(Environment.NewLine + "WHILE p_rc > 0");
                    sb.Append(Environment.NewLine + "LOOP" + Environment.NewLine);
                }

                sb.Append(Environment.NewLine +
                "\tINSERT INTO " + this.FullTableNameToScript + " " + mshint_change + "(" + fields + ") " + Environment.NewLine +
                "\tSELECT " + limit_5000_ms + tmp_fields + Environment.NewLine +
                "\tFROM " + table.TableEdit.FullTableNameToScript + " source" + Environment.NewLine +
                "\tLEFT JOIN " + this.FullTableNameToScript + " target " + mshint_sel + "ON " + where + Environment.NewLine +
                "\tWHERE " + target_is_null
                );

                if (cbAddFK == 2)
                {
                    // добавить проверку на существование внешних ключей
                    foreach (var item in this.ListTableFK
                        .Where(x => x.Value.FKDeep == 1)
                        )
                    {
                        sb.Append(item.Value.WhereExistsCondition("\t","source", limit_1_ms, mshint_sel, limit_1_pg));
                    }
                }

                sb.Append(Environment.NewLine);

                if (this.TargetDB == Utilities.TargetDBType.PGSQL)
                {
                    sb.Append(
                        "\t" + limit_5000_pg + select_end + Environment.NewLine
                    );
                }

                if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                {
                    sb.Append(Environment.NewLine + "\tSET @rc = @@rowcount");
                    sb.Append(Environment.NewLine + "END" + Environment.NewLine);
                }
                if (this.TargetDB == Utilities.TargetDBType.PGSQL)
                {
                    sb.Append(Environment.NewLine + "\tget diagnostics p_rc := row_count;");
                    sb.Append(Environment.NewLine + "END LOOP;" + Environment.NewLine);
                }

                if ((this.TargetDB == Utilities.TargetDBType.MSSQL) && isPK && isIdentity)
                {
                    sb.Append(Environment.NewLine + "SET IDENTITY_INSERT " + FullTableNameToScript + " OFF");
                    sb.Append(Environment.NewLine);
                }

            }

            // UPDATE stg.LocalDBList
            if (
                isUpdateLocalDBList &&
                isPromed &&
                (listLocal != null) &&
                (listLocal.Count > 0)
                )
            {
                string sch = Utilities.Databases.GetSchemaName(this.FullTableNameReady);

                foreach (var localdblist_name in listLocal)
                {
                    if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                    {
                        sb.Append(Environment.NewLine + "UPDATE stg.LocalDBList WITH (rowlock)");
                        sb.Append(Environment.NewLine + "SET LocalDbList_updDT = CURRENT_TIMESTAMP");
                        sb.Append(Environment.NewLine + "-- SELECT* FROM stg.LocalDbList");
                        sb.Append(Environment.NewLine + $"WHERE LocalDbList_name = '{localdblist_name}'");
                        sb.Append(Environment.NewLine + $"AND LocalDbList_schema = '{sch}'");
                        sb.Append(Environment.NewLine + $"AND LocalDbList_module = 'promed'");
                        sb.Append(Environment.NewLine);
                    }

                    if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD))
                    {
                        sb.Append(Environment.NewLine + "UPDATE stg.LocalDBList");
                        sb.Append(Environment.NewLine + "SET LocalDbList_updDT = localtimestamp");
                        sb.Append(Environment.NewLine + "-- SELECT* FROM stg.LocalDbList");
                        sb.Append(Environment.NewLine + $"WHERE LocalDbList_name ilike '{localdblist_name}'");
                        sb.Append(Environment.NewLine + $"AND LocalDbList_schema ilike '{sch}'");
                        sb.Append(Environment.NewLine + $"AND LocalDbList_module = 'promed';");
                        sb.Append(Environment.NewLine);
                    }
                }
            }

            // Проверка на региональность в конце скрипта
            if (isAddRegion)
            {
                sb.Append(FinishRegion(txtRegion,"", Environment.NewLine));
            }

            if (isAddRegion || (InsUpdDTType == Utilities.InsUpdDTType.VARI) || (ScriptType == Utilities.ScriptType.INSERT_TMP) || (ScriptType == Utilities.ScriptType.UPSERT_TMP) || (isAddCheckUnique && ScriptType == Utilities.ScriptType.INSERT))
            {
                if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD))
                {
                    sb.Append(Environment.NewLine + "END;" + Environment.NewLine + "$script$;" + Environment.NewLine);
                }
            }

            // для MS - финальный GO
            if (this.TargetDB == Utilities.TargetDBType.MSSQL && !local_isAddGO)
            {
                sb.Append(Environment.NewLine + "GO");
                sb.Append(Environment.NewLine);
            }

            // Проверка
            if (isAddComment)
            {
                sb.Append(Environment.NewLine + "-- Проверка");
                sb.Append(Environment.NewLine + "-- SELECT * FROM " + FullTableNameToScript + Environment.NewLine);
            }

            if (file != null)
            {
                // пишем в файл по частям
                file.Write(sb.ToString().ScriptPartReady());
                sb.Clear();
            }

            Script = sb.ToString();

            // финальные действия
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (win != null)
                {
                    win.pbProgress.Value = 100;
                }
                else
                {
                    winProgress.Close();
                }
            });

            return Script;
        }

        /// <summary>
        /// Собрать SQL-скрипт-шаблон
        /// </summary>
        /// <param name="isAddTitle">=true - добавить в скрипт заголовок</param>
        /// <param name="file">экземпляр StreamWriter для записи скрипты сразу в файл</param>
        /// <param name="isAddRegion">=true - добавить условие региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="listLocal">список локальных справочников</param>
        public string GenerateScriptShablon(bool isAddTitle, StreamWriter file, bool isAddRegion, string txtRegion, List<string> listLocal)
        {
            StringBuilder sb = new StringBuilder(100000);

            string Script = "";

            // заголовок скрипта (информация о задаче)
            if (isAddTitle)
            {
                string TitleScript = MainWindow.Task.TitleScript(this.TargetDB, "data", false, false);
                if (!string.IsNullOrWhiteSpace(TitleScript))
                {
                    sb.Append(TitleScript);
                    sb.Append("--Если используется COMMIT - надо добавить в чейнджсет атрибут runInTransaction:false" + Environment.NewLine);
                }
            }

            string keys = this.UniqueKey;
            string[] UK = keys.ToLower().Split(',');
            for (int i = 0; i < UK.Count(); i++)
            {
                UK[i] = UK[i].Trim();
                if (this.TargetDB == Utilities.TargetDBType.EMD)
                {
                    UK[i] = "\"" + UK[i] + "\"";
                }
            }

            string pk = Utilities.Databases.GetTableName(this.FullTableNameReady) + "_id";
            bool isPK = Utilities.Databases.IsPK(keys, pk);

            // имя временной таблицы
            string tmp_table = "odb_???_" + MainWindow.Task.TaskNumberToFilename + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + TableNameToFilename;
            tmp_table = tmp_table.ToLower();
            string tmp_full = "tmp." + tmp_table;

            // ----------------------------------------------------------------------------------------
            // собираем скрипт
            // ----------------------------------------------------------------------------------------

            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                sb.Append(Environment.NewLine + "/*");
                sb.Append(Environment.NewLine + "BEGIN TRAN");
                sb.Append(Environment.NewLine + $"DROP TABLE IF EXISTS {tmp_full}");
                sb.Append(Environment.NewLine + "*/");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine + "DECLARE @datetime DATETIME = dbo.tzgetdate()");
                sb.Append(Environment.NewLine + "DECLARE @rc INT");
                sb.Append(Environment.NewLine + "--DECLARE @Error_Code INT");
                sb.Append(Environment.NewLine + "--DECLARE @Error_Message VARCHAR(4000)");
                sb.Append(Environment.NewLine);
                // Добавить формат даты
                sb.Append(Environment.NewLine + "SET DATEFORMAT ymd" + Environment.NewLine);
            }
            else
            {
                sb.Append(Environment.NewLine + "/*");
                sb.Append(Environment.NewLine + "BEGIN;");
                sb.Append(Environment.NewLine + $"DROP TABLE IF EXISTS {tmp_full};");
                sb.Append(Environment.NewLine + "*/");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine + "DO $script$");
                sb.Append(Environment.NewLine + "DECLARE");
                sb.Append(Environment.NewLine + "\tp_datetime timestamp = localtimestamp;");
                //sb.Append(Environment.NewLine + "\tp_datetimetz timestamptz = p_datetime::timestamptz;");
                sb.Append(Environment.NewLine + "\tp_rc INT;");
                sb.Append(Environment.NewLine + "\t--error_RETURNED_SQLSTATE VARCHAR;");
                sb.Append(Environment.NewLine + "\t--error_MESSAGE_TEXT VARCHAR;");
                sb.Append(Environment.NewLine + "\t--error_PG_EXCEPTION_CONTEXT VARCHAR;");

                sb.Append(Environment.NewLine + "BEGIN");
                sb.Append(Environment.NewLine);

                if (
                    isAddRegion && 
                    txtRegion != "0" &&
                    (this.isPromed || this.isLIS)
                )
                {
                    if (this.isPromed)
                    {
                        sb.Append(Environment.NewLine + "IF dbo.GetDBType() = 'db_test'");
                    }
                    else if (this.isLIS)
                    {
                        sb.Append(Environment.NewLine + $"IF current_database() IN ({this.ListTestReleaseDB_full})");
                    }

                    sb.Append(Environment.NewLine + "THEN");
                    sb.Append(Environment.NewLine + $"\tPERFORM set_config('Session.Region', '{txtRegion}', False);");
                    sb.Append(Environment.NewLine + "END IF;");
                    sb.Append(Environment.NewLine);
                }

            }

            // Проверка на региональность в начале скрипта
            if (isAddRegion && txtRegion != "0")
            {
                sb.Append(StartRegion(txtRegion, "", Environment.NewLine));
            }

            // блокировка повторного выполнения BEGIN
            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                sb.Append(Environment.NewLine + $"IF OBJECT_ID(N'{tmp_full}', 'U') IS NULL");
                sb.Append(Environment.NewLine + "BEGIN");
                sb.Append(Environment.NewLine);
            }
            else
            {
                sb.Append(Environment.NewLine + $"IF NOT EXISTS (SELECT 1 FROM pg_tables WHERE schemaname='tmp' and tablename=lower('{tmp_table}') LIMIT 1)");
                sb.Append(Environment.NewLine + "THEN");
                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine + "--------------------------------------------------------");
            sb.Append(Environment.NewLine + "-- резервная копия");
            sb.Append(Environment.NewLine + "--------------------------------------------------------");
            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                sb.Append(Environment.NewLine + "SELECT a.*");
                sb.Append(Environment.NewLine + "INTO " + tmp_full);
                sb.Append(Environment.NewLine + "FROM " + FullTableNameToScript + " a WITH (nolock)");
                sb.Append(Environment.NewLine);
            }
            else
            {
                sb.Append(Environment.NewLine + "CREATE TABLE IF NOT EXISTS " + tmp_full + " AS");
                sb.Append(Environment.NewLine + "SELECT a.*");
                sb.Append(Environment.NewLine + "FROM " + FullTableNameToScript + " a;");
                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine + "-- индексы по временной таблице");
            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                sb.Append(Environment.NewLine + $"IF NOT EXISTS ({Environment.NewLine}\tSELECT 1 FROM sys.sysindexes WITH(nolock){Environment.NewLine}\tWHERE name = 'idx_{tmp_table}_1'{Environment.NewLine}\tAND id = OBJECT_ID(N'{tmp_full}', 'U'){Environment.NewLine})");
                sb.Append(Environment.NewLine + $"CREATE NONCLUSTERED INDEX idx_{tmp_table}_1 ON {tmp_full} ({keys})");
                sb.Append(Environment.NewLine);
            }
            else
            {
                sb.Append(Environment.NewLine + $"CREATE INDEX IF NOT EXISTS idx_{tmp_table}_1 ON {tmp_full} USING btree ({keys});");
                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine + "--------------------------------------------------------");
            sb.Append(Environment.NewLine + "-- обновление");
            sb.Append(Environment.NewLine + "--------------------------------------------------------");
            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                sb.Append(Environment.NewLine + "SET NOCOUNT ON");
                sb.Append(Environment.NewLine + "SET @rc = 1");
                sb.Append(Environment.NewLine + "WHILE @rc > 0");
                sb.Append(Environment.NewLine + "BEGIN");
                sb.Append(Environment.NewLine + "/*");
                sb.Append(Environment.NewLine + "BEGIN TRY");
                sb.Append(Environment.NewLine + "\tBEGIN TRAN");
                sb.Append(Environment.NewLine + "*/");
                sb.Append(Environment.NewLine + "\tUPDATE TOP(5000) a");
                sb.Append(Environment.NewLine + "\tSET");
                sb.Append(Environment.NewLine + "\t\tfield123 = t.field123_new,");
                sb.Append(Environment.NewLine + $"\t\t{TableNameReady}_upddt = @datetime,");
                sb.Append(Environment.NewLine + "\t\tpmuser_updid = 1");
                sb.Append(Environment.NewLine + $"\tFROM {FullTableNameToScript} a WITH(rowlock)");
                sb.Append(Environment.NewLine + $"\tINNER JOIN {tmp_full} t WITH(nolock) ON ");
                for (int i = 0; i < UK.Length; i++)
                {
                    if (i > 0) sb.Append(" AND ");
                    sb.Append($"t.{UK[i]} = a.{UK[i]}");
                }
                sb.Append(Environment.NewLine + $"\tWHERE COALESCE(a.field123, 0) <> COALESCE(t.field123_new, 0)");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine + "\tSET @rc = @@rowcount");
                sb.Append(Environment.NewLine + "\t--print 'rows = ' + CAST(@rc as VARCHAR(100))");
                sb.Append(Environment.NewLine + "/*");
                sb.Append(Environment.NewLine + "\tCOMMIT TRAN");
                sb.Append(Environment.NewLine + "END TRY");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine + "BEGIN CATCH");
                sb.Append(Environment.NewLine + "\tSET @Error_Code = ISNULL(@Error_Code, error_number())");
                sb.Append(Environment.NewLine + "\tSET @Error_Message = ISNULL(@Error_Message, SUBSTRING('Ошибка [' + COALESCE(CAST(@Error_Code AS NVARCHAR(max)), 'N\\A') + '] в строке [' + COALESCE(CAST(ERROR_LINE() AS NVARCHAR(max)), 'N\\A') + '] ' + ERROR_MESSAGE(), 0, 4000))");
                sb.Append(Environment.NewLine + "\tIF @@trancount > 0");
                sb.Append(Environment.NewLine + "\t\tROLLBACK TRAN");
                sb.Append(Environment.NewLine + "END CATCH");
                sb.Append(Environment.NewLine + "*/");
                sb.Append(Environment.NewLine + "END");
                sb.Append(Environment.NewLine);
            }
            else
            {
                sb.Append(Environment.NewLine + "p_rc := 1;");
                sb.Append(Environment.NewLine + "WHILE p_rc > 0");
                sb.Append(Environment.NewLine + "LOOP");
                sb.Append(Environment.NewLine + "--BEGIN");
                sb.Append(Environment.NewLine + "\tWITH cte AS (");
                sb.Append(Environment.NewLine + $"\t\tSELECT ");
                for (int i = 0; i < UK.Length; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append($"a.{UK[i]}");
                }
                sb.Append($", t.field123_new");
                sb.Append(Environment.NewLine + $"\t\tFROM {FullTableNameToScript} a");
                sb.Append(Environment.NewLine + $"\t\tINNER JOIN {tmp_full} t ON ");
                for (int i = 0; i < UK.Length; i++)
                {
                    if (i > 0) sb.Append(" AND ");
                    sb.Append($"t.{UK[i]} = a.{UK[i]}");
                }
                sb.Append(Environment.NewLine + $"\t\tWHERE COALESCE(a.field123, 0) <> COALESCE(t.field123_new, 0)");
                sb.Append(Environment.NewLine + $"\t\tLIMIT 5000");
                sb.Append(Environment.NewLine + $"\t)");
                sb.Append(Environment.NewLine + $"\tUPDATE {FullTableNameToScript}");
                sb.Append(Environment.NewLine + "\tSET");
                sb.Append(Environment.NewLine + "\t\tfield123 = cte.field123_new,");
                sb.Append(Environment.NewLine + $"\t\t{TableNameReady}_upddt = p_datetime,");
                //sb.Append(Environment.NewLine + $"\t\t{TableNameToScript}_upddttz = p_datetimetz,");
                sb.Append(Environment.NewLine + "\t\tpmuser_updid = 1");
                sb.Append(Environment.NewLine + "\tFROM cte");
                sb.Append(Environment.NewLine + $"\tWHERE ");
                for (int i = 0; i < UK.Length; i++)
                {
                    if (i > 0) sb.Append(" AND ");
                    sb.Append($"cte.{UK[i]} = {TableNameToScript}.{UK[i]}");
                }
                sb.Append(";");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine + "\tGET DIAGNOSTICS p_rc := row_count;");
                sb.Append(Environment.NewLine + "\t--RAISE NOTICE 'rows = %', CAST(p_rc AS VARCHAR)");
                sb.Append(Environment.NewLine + "/*");
                sb.Append(Environment.NewLine + "EXCEPTION WHEN OTHERS THEN GET STACKED DIAGNOSTICS");
                sb.Append(Environment.NewLine + "\terror_RETURNED_SQLSTATE = RETURNED_SQLSTATE,");
                sb.Append(Environment.NewLine + "\terror_MESSAGE_TEXT = MESSAGE_TEXT,");
                sb.Append(Environment.NewLine + "\terror_PG_EXCEPTION_CONTEXT = PG_EXCEPTION_CONTEXT;");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine + "\terror_code := COALESCE(error_code, error_RETURNED_SQLSTATE);");
                sb.Append(Environment.NewLine + "\terror_message := COALESCE(error_message, CONCAT('Ошибка \"', COALESCE(error_MESSAGE_TEXT, 'N\\A'), '\". Место: ', CHR(10), error_PG_EXCEPTION_CONTEXT));");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine + "\tp_rc := 0;");
                sb.Append(Environment.NewLine + "\tEXIT;");
                sb.Append(Environment.NewLine + "END;");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine + "COMMIT;");
                sb.Append(Environment.NewLine + "*/");
                sb.Append(Environment.NewLine + "END LOOP;");
                sb.Append(Environment.NewLine);
            }

            if (
                isUpdateLocalDBList &&
                isPromed &&
                (listLocal != null) &&
                (listLocal.Count > 0)
                )
            {
                sb.Append(Environment.NewLine + "--------------------------------------------------------");
                sb.Append(Environment.NewLine + "-- локальный справочник");
                sb.Append(Environment.NewLine + "--------------------------------------------------------");

                string sch = Utilities.Databases.GetSchemaName(this.FullTableNameReady);

                foreach (var localdblist_name in listLocal)
                {
                    if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                    {
                        sb.Append(Environment.NewLine + "UPDATE stg.LocalDbList WITH(rowlock)");
                        sb.Append(Environment.NewLine + "SET LocalDbList_updDT = CURRENT_TIMESTAMP");
                        sb.Append(Environment.NewLine + "-- SELECT * FROM stg.LocalDbList");
                        sb.Append(Environment.NewLine + $"WHERE LocalDbList_name = '{localdblist_name}'");
                        sb.Append(Environment.NewLine + $"AND LocalDbList_schema = '{sch}'");
                        sb.Append(Environment.NewLine + "AND LocalDbList_module = 'promed'");
                        sb.Append(Environment.NewLine);
                    }
                    else
                    {
                        sb.Append(Environment.NewLine + "UPDATE stg.LocalDbList");
                        sb.Append(Environment.NewLine + "SET LocalDbList_updDT = localtimestamp");
                        sb.Append(Environment.NewLine + "-- SELECT * FROM stg.LocalDbList");
                        sb.Append(Environment.NewLine + $"WHERE LocalDbList_name ilike '{localdblist_name}'");
                        sb.Append(Environment.NewLine + $"AND LocalDbList_schema ilike '{sch}'");
                        sb.Append(Environment.NewLine + "AND LocalDbList_module = 'promed';");
                        sb.Append(Environment.NewLine);
                    }
                }
            }

            // блокировка повторного выполнения END
            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                sb.Append(Environment.NewLine + "END");
                sb.Append(Environment.NewLine);
            }
            else
            {
                sb.Append(Environment.NewLine + "END IF;");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine + "--------------------------------------------------------");
                sb.Append(Environment.NewLine + "-- исправляем owner временных таблиц");
                sb.Append(Environment.NewLine + "--------------------------------------------------------");
                sb.Append(Environment.NewLine + $"IF current_database() NOT IN ({this.ListTestReleaseDB_full})");
                sb.Append(Environment.NewLine + "THEN");
                sb.Append(Environment.NewLine + $"\tALTER TABLE IF EXISTS {tmp_full} OWNER TO promed_role_tmp;");
                sb.Append(Environment.NewLine + "END IF;");
                sb.Append(Environment.NewLine);
            }

            // Проверка на региональность в конце скрипта
            if (isAddRegion && txtRegion != "0")
            {
                sb.Append(FinishRegion(txtRegion, "", Environment.NewLine));
            }

            // финиш
            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                sb.Append(Environment.NewLine + "GO");
                sb.Append(Environment.NewLine);
            }
            else
            {

                sb.Append(Environment.NewLine + "END;" + Environment.NewLine + "$script$;");
                sb.Append(Environment.NewLine);
            }

            sb.Append(Environment.NewLine + "--------------------------------------------------------");
            sb.Append(Environment.NewLine + "-- Проверка");
            sb.Append(Environment.NewLine + "--------------------------------------------------------");
            sb.Append(Environment.NewLine + "/*");
            sb.Append(Environment.NewLine + $"SELECT count(*) FROM {tmp_full};");
            sb.Append(Environment.NewLine + "SELECT * FROM " + FullTableNameToScript + " WHERE ?");
            sb.Append(Environment.NewLine + "ROLLBACK;");
            sb.Append(Environment.NewLine + "*/");
            sb.Append(Environment.NewLine);

            if (file != null)
            {
                // пишем в файл
                file.Write(sb.ToString().ScriptPartReady());
            }

            Script = sb.ToString();
            return Script;
        }

        /// <summary>
        /// Собрать SQL-скрипт bulk для MS
        /// </summary>
        /// <param name="Connect">подключение к БД</param>
        /// <param name="file">экземпляр StreamWriter для записи скрипты сразу в файл</param>
        /// <param name="CSVFile">путь в csv-файлу</param>
        /// <param name="isAddRegion">=true - добавить условие региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="cbAddFK">=1 добавить INSERT внешних ключей =2 добавить EXISTS внешних ключей</param> 
        /// <returns></returns>
        public string GenerateMSBulk(ConnectDB Connect, StreamWriter file, string CSVFile, bool isAddRegion, string txtRegion, int cbAddFK)
        {

            if (this.TargetDB != Utilities.TargetDBType.MSSQL) return "";
            if ((ScriptType != Utilities.ScriptType.INSERT_BULK_TABLE) && (ScriptType != Utilities.ScriptType.INSERT_BULK_VIEW)) return "";

            txtRegion = txtRegion.Trim();

            StringBuilder sb = new StringBuilder(100000);
            var generator = new RandomGenerator();

            // заголовок скрипта (информация о задаче)
            string TitleScript = MainWindow.Task.TitleScript(this.TargetDB, "data", false, false);
            if (!string.IsNullOrWhiteSpace(TitleScript)) sb.Append(TitleScript);

            // Текст запроса в комментарии
            if (!string.IsNullOrWhiteSpace(this.SQLQuery))
            {
                sb.Append(Environment.NewLine + "/* запрос к базе");
                sb.Append(Environment.NewLine + this.SQLQuery);
                sb.Append(Environment.NewLine + "*/");
                sb.Append(Environment.NewLine);
            }

            // первичный ключ
            List<string> pk = new List<string>();
            bool isIdentity = false;
            try
            {
                if (Connect != null)
                {
                    var pkinfo = Connect.GetTablePK(this.FullTableNameReady);

                    pk = pkinfo.ListFieldNames;
                    isIdentity = pkinfo.HasIdentity;
                }
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                pk = new List<string>();
            }

            if (pk.Count == 0)
            {
                pk.Add(Utilities.Databases.GetTableName(this.FullTableNameReady) + "_id");
            }

            string pk_join = "";
            string pk_where = "";
            string pk_is_null = "";

            foreach (var UK in pk)
            {
                if (!string.IsNullOrWhiteSpace(UK))
                {
                    if (pk_join != "") pk_join = pk_join + " AND ";
                    pk_join = pk_join + "d." + UK + " = t." + UK;

                    if (pk_where != "") pk_where = pk_where + " AND ";
                    pk_where = pk_where + TableNameToScript + "." + UK + " = d." + UK;

                    if (pk_is_null != "") pk_is_null = pk_is_null + " AND ";
                    pk_is_null = pk_is_null + "d." + UK + " IS NULL";
                }
            }

            // Список полей
            string fields = "";
            string set_fields = "";
            string ins_fields = "";
            foreach (DataColumn column in this.DataTableUsedColumns)
            {
                string ColumnName = column.ColumnName;

                // пропускаем rowversion
                if (ColumnName.ToLower().IndexOf("rowversion") != -1) continue;

                if (fields != "") fields = fields + ", ";
                fields = fields + ColumnName;

                var pk_found = pk.Where(x => x.ToLower() == ColumnName.ToLower()).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(pk_found))
                {
                    // если это не PK - обновляем
                    if (set_fields != "") set_fields = set_fields + Environment.NewLine;
                    set_fields = set_fields + "\t" + ColumnName + " = t." + ColumnName + ",";
                }

                if (ins_fields != "") ins_fields = ins_fields + Environment.NewLine;
                ins_fields = ins_fields + "\t" + "t." + ColumnName + ",";
            }
            set_fields = set_fields.Trim().TrimEnd(',');
            ins_fields = ins_fields.Trim().TrimEnd(',');

            //настройка
            sb.Append(Environment.NewLine + "SET DATEFORMAT dmy");
            sb.Append(Environment.NewLine + "GO");
            sb.Append(Environment.NewLine);

            //Временная вьюха
            string tmpview = "tmp.v_bulk_" + DateTime.Now.ToString("yyyyMMdd") + "_" + generator.RandomString(8);
            if (ScriptType == Utilities.ScriptType.INSERT_BULK_VIEW)
            {
                sb.Append(Environment.NewLine + @"CREATE VIEW " + tmpview + @"
AS
SELECT " + fields + @"
FROM " + FullTableNameToScript + @"
GO" + Environment.NewLine);
            }

            //Временная хранимка на добавление
            sb.Append(Environment.NewLine + @"CREATE PROCEDURE #bulk_ins 
    @vfile_name sysname,
    @vtable_name sysname,
    @vrows_per_bacth int = null,
    @delimiter varchar(20)
AS
BEGIN

    DECLARE @psql_text nvarchar(1024)
    DECLARE @crlf char(2) = CHAR(13) + CHAR(10)
    DECLARE @codepage varchar(20) = ''

    --IF SUBSTRING(@@VERSION, CHARINDEX('Windows', @@VERSION, 0), 100) like '%Windows%'
    --BEGIN
    --    SET @codepage = '65001'
    --END
    --ELSE
    --BEGIN
    --    SET @codepage = '''RAW'''
    --END

    SET @psql_text = 'SET DATEFORMAT dmy;
                        BULK INSERT ' + @vtable_name + @crlf +
                        'FROM ' + '''' + @vfile_name + '''' + @crlf +
                        'WITH (FIRSTROW = 1,
                            FIELDTERMINATOR = '''+@delimiter+''',
                            DATAFILETYPE = ''WIDECHAR'',
                            KEEPNULLS,
                            KEEPIDENTITY
                         );'
    -- print @psql_text

    EXEC sp_executesql @psql_text
END
GO" + Environment.NewLine);

            // Проверка на региональность в начале скрипта
            if (isAddRegion && txtRegion != "0")
            {
                sb.Append(StartRegion(txtRegion, "", Environment.NewLine));
            }

            //Временная промежуточная таблица
            string tmptable = $"tmp.odb_???_{MainWindow.Task.TaskNumberToFilename}_{DateTime.Now.ToString("yyyyMMdd")}_{generator.RandomString(8)}";
            if (ScriptType == Utilities.ScriptType.INSERT_BULK_TABLE)
            {
                sb.Append(Environment.NewLine + "-- Промежуточная таблица со структурой аналогичной таблице назначения");
                sb.Append(Environment.NewLine + @"SELECT TOP(0) " + fields + @"
INTO " + tmptable + @"
FROM " + FullTableNameToScript + @" WITH (nolock)" + Environment.NewLine);

                //Модификация структуры и данных в промежуточной таблицы
                sb.Append(Environment.NewLine + "-- Модификация структуры промежуточной таблицы (НЕ ОБЯЗАТЕЛЬНО)");
                sb.Append(Environment.NewLine + "-- ALTER TABLE " + tmptable);
                sb.Append(Environment.NewLine);
            }

            if (ScriptType == Utilities.ScriptType.INSERT_BULK_VIEW)
            {
                //Очистить таблицу назначения
                sb.Append(Environment.NewLine + "-- Очистить таблицу назначения");
                sb.Append(Environment.NewLine + "-- DELETE FROM " + FullTableNameToScript + @" WITH (rowlock)");
                sb.Append(Environment.NewLine);
            }

            //Загрузить данные
            string CSVFileName = Path.GetFileName(CSVFile);
            sb.Append(Environment.NewLine + "-- Загрузить данные из CSV-файла");
            sb.Append(Environment.NewLine + @"DECLARE @OSversion varchar(200)
DECLARE @path varchar(200)
DECLARE @vfilepath varchar(200)
SET @OSversion = SUBSTRING(@@VERSION, CHARINDEX('Windows', @@VERSION, 0), 100)
IF @OSversion like '%Windows%'
BEGIN
    SET @path = concat('C:', CHAR(92), 'temp', CHAR(92), 'ms', CHAR(92)) 
END
ELSE
BEGIN
    SET @path = '/tmp/ms/'
END

SET @vfilepath = @path + '" + CSVFileName + "'");
            if (ScriptType == Utilities.ScriptType.INSERT_BULK_TABLE)
            {
                sb.Append(Environment.NewLine + "EXEC #bulk_ins @vfile_name = @vfilepath, @vtable_name = '" + tmptable + @"', @vrows_per_bacth = 100100, @delimiter = '$'");
            }
            else
            {
                sb.Append(Environment.NewLine + "EXEC #bulk_ins @vfile_name = @vfilepath, @vtable_name = '" + tmpview + @"', @vrows_per_bacth = 100100, @delimiter = '$'");
            }
            sb.Append(Environment.NewLine);

            if (ScriptType == Utilities.ScriptType.INSERT_BULK_TABLE)
            {
                //Модификация данных в промежуточной таблице
                sb.Append(Environment.NewLine + "-- Модификация данных в промежуточной таблице (НЕ ОБЯЗАТЕЛЬНО)");
                sb.Append(Environment.NewLine + "-- UPDATE " + tmptable + " WITH (rowlock) SET" + Environment.NewLine);
            }

            if (ScriptType == Utilities.ScriptType.INSERT_BULK_TABLE)
            {
                //Очистить таблицу назначения
                sb.Append(Environment.NewLine + "-- Очистить таблицу назначения");
                sb.Append(Environment.NewLine + "-- DELETE FROM " + FullTableNameToScript + @" WITH (rowlock)" + Environment.NewLine);

                //Обновить данные в таблице назначения
                sb.Append(Environment.NewLine + "-- Обновить данные в таблице назначения");
                sb.Append(Environment.NewLine + "UPDATE d SET");
                sb.Append(Environment.NewLine + "\t" + set_fields);
                sb.Append(Environment.NewLine + "FROM " + tmptable + " t");
                sb.Append(Environment.NewLine + "INNER JOIN " + FullTableNameToScript + " d WITH (rowlock) ON " + pk_join);

                if (cbAddFK == 2)
                {
                    // добавить проверку на существование внешних ключей
                    foreach (var item in this.ListTableFK
                        .Where(x => x.Value.FKDeep == 1)
                        )
                    {
                        sb.Append(item.Value.WhereExistsCondition("", "t", "TOP(1) ", "WITH (NOLOCK) ", ""));
                    }
                }

                sb.Append(Environment.NewLine);

                //Добавить данные в таблицу назначения
                sb.Append(Environment.NewLine + "-- Добавить данные в таблицу назначения");
                if (isIdentity)
                {
                    sb.Append(Environment.NewLine + "SET IDENTITY_INSERT " + FullTableNameToScript + " ON" + Environment.NewLine);
                }
                sb.Append(Environment.NewLine + "INSERT INTO " + FullTableNameToScript + " WITH (rowlock) (" + fields + ")");
                sb.Append(Environment.NewLine + "SELECT");
                sb.Append(Environment.NewLine + "\t" + ins_fields);
                sb.Append(Environment.NewLine + "FROM " + tmptable + " t");
                sb.Append(Environment.NewLine + "LEFT JOIN " + FullTableNameToScript + " d WITH (nolock) ON " + pk_join);
                sb.Append(Environment.NewLine + "WHERE " + pk_is_null);

                if (cbAddFK == 2)
                {
                    // добавить проверку на существование внешних ключей
                    foreach (var item in this.ListTableFK
                        .Where(x => x.Value.FKDeep == 1)
                        )
                    {
                        sb.Append(item.Value.WhereExistsCondition("", "t", "TOP(1) ", "WITH (NOLOCK) ", ""));
                    }
                }

                sb.Append(Environment.NewLine);

                if (isIdentity)
                {
                    sb.Append(Environment.NewLine + "SET IDENTITY_INSERT " + FullTableNameToScript + " OFF" + Environment.NewLine);
                }
            }

            //Модификация данных в таблице назначения
            sb.Append(Environment.NewLine + "-- Модификация данных в таблице назначения (НЕ ОБЯЗАТЕЛЬНО)");
            sb.Append(Environment.NewLine + "-- UPDATE " + FullTableNameToScript + " WITH (rowlock) SET" + Environment.NewLine);

            //Удаление временных объектов
            sb.Append(Environment.NewLine + "-- Удаление временных объектов");
            if (ScriptType == Utilities.ScriptType.INSERT_BULK_TABLE)
            {
                sb.Append(Environment.NewLine + "DROP TABLE IF EXISTS " + tmptable);
            }
            else
            {
                sb.Append(Environment.NewLine + "DROP VIEW IF EXISTS " + tmpview);
            }
            sb.Append(Environment.NewLine);

            // Проверка на региональность в конце скрипта
            if (isAddRegion && txtRegion != "0")
            {
                sb.Append(FinishRegion(txtRegion, "", Environment.NewLine));
            }

            sb.Append(Environment.NewLine + @"DROP PROCEDURE #bulk_ins");
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine + "SET DATEFORMAT ymd");
            sb.Append(Environment.NewLine);
            // Проверка
            sb.Append(Environment.NewLine + "-- Проверка");
            sb.Append(Environment.NewLine + "-- SELECT * FROM " + FullTableNameToScript);

            if (file != null)
            {
                // пишем в файл по частям
                file.Write(sb.ToString().ScriptPartReady());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Собрать SQL-скрипт copy для PG
        /// </summary>
        /// <param name="Connect">подключение к БД</param>
        /// <param name="file">экземпляр StreamWriter для записи скрипты сразу в файл</param>
        /// <param name="CSVFile">путь в csv-файлу</param>
        /// <param name="isAddRegion">=true - добавить условие региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="cbAddFK">=1 добавить INSERT внешних ключей =2 добавить EXISTS внешних ключей</param> 
        /// <returns></returns>
        public string GeneratePGCopy(ConnectDB Connect, StreamWriter file, string CSVFile, bool isAddRegion, string txtRegion, int cbAddFK)
        {

            if ((this.TargetDB != Utilities.TargetDBType.PGSQL) && (this.TargetDB != Utilities.TargetDBType.EMD)) return "";
            if ((ScriptType != Utilities.ScriptType.INSERT_BULK_TABLE) && (ScriptType != Utilities.ScriptType.INSERT_BULK_VIEW)) return "";

            txtRegion = txtRegion.Trim();

            StringBuilder sb = new StringBuilder(100000);
            var generator = new RandomGenerator();

            // заголовок скрипта (информация о задаче)
            string TitleScript = MainWindow.Task.TitleScript(this.TargetDB, "data", false, false);
            if (!string.IsNullOrWhiteSpace(TitleScript)) sb.Append(TitleScript);

            // Текст запроса в комментарии
            if (!string.IsNullOrWhiteSpace(this.SQLQuery))
            {
                sb.Append(Environment.NewLine + "/* запрос к базе");
                sb.Append(Environment.NewLine + this.SQLQuery);
                sb.Append(Environment.NewLine + "*/");
                sb.Append(Environment.NewLine);
            }

            // первичный ключ
            List<string> pk = new List<string>();
            List<string> pktype = new List<string>();
            try
            {
                if (Connect != null)
                {
                    var pkinfo = Connect.GetTablePK(this.FullTableNameReady);

                    pk = pkinfo.ListFieldNames;
                    pktype = pkinfo.ListFieldTypes;
                }
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                pk = new List<string>();
                pktype = new List<string>();
            }

            if (pk.Count == 0)
            {
                pk.Add(Utilities.Databases.GetTableName(this.FullTableNameReady) + "_id");
                pktype.Add("BIGINT");
            }

            string pk_join = "";
            string pk_where = "";
            string pk_is_null = "";

            for (int i = 0; i < pk.Count(); i++)
            {
                string UK = pk[i].Trim();
                string UKType = pktype[i].Trim();

                if (this.TargetDB == Utilities.TargetDBType.EMD) //-V3022
                {
                    UK = "\"" + UK + "\"";
                }

                if (
                    (!string.IsNullOrWhiteSpace(UK)) &&
                    (!string.IsNullOrWhiteSpace(UKType))
                )
                {
                    if (pk_join != "") pk_join = pk_join + " AND ";
                    pk_join = pk_join + "d." + UK + " = t." + UK + "::" + UKType;

                    if (pk_where != "") pk_where = pk_where + " AND ";
                    pk_where = pk_where + TableNameToScript + "." + UK + " = d." + UK;

                    if (pk_is_null != "") pk_is_null = pk_is_null + " AND ";
                    pk_is_null = pk_is_null + "d." + UK + " IS NULL";
                }
            }

            //Временная промежуточная таблица
            string tmptable = $"tmp.odb_???_{MainWindow.Task.TaskNumberToFilename}_{DateTime.Now.ToString("yyyyMMdd")}_{generator.RandomString(8)}";

            // Список полей
            string fields = "";
            string alters = "";
            string updates = "";
            string set_fields = "";
            string ins_fields = "";
            foreach (DataColumn column in this.DataTableUsedColumns)
            {
                string ColumnName = column.ColumnName;

                if (this.TargetDB == Utilities.TargetDBType.EMD) //-V3022
                {
                    ColumnName = "\"" + ColumnName + "\"";
                }

                // для PG SQL имя колонки в нижний регистр
                if (this.TargetDB == Utilities.TargetDBType.PGSQL) //-V3022
                {
                    ColumnName = ColumnName.ToLower();
                }

                // пропускаем rowversion
                if (ColumnName.ToLower().IndexOf("rowversion") != -1) continue;

                if (fields != "") fields = fields + ", ";
                fields = fields + ColumnName;

                if (alters != "") alters = alters + Environment.NewLine;
                alters = alters + "ALTER TABLE " + tmptable + " ALTER COLUMN " + ColumnName + " TYPE VARCHAR;";

                if (updates != "") updates = updates + Environment.NewLine;
                updates = updates + "UPDATE " + tmptable + " SET " + ColumnName + " = NULL WHERE LOWER(" + ColumnName + ") = 'null';";

                var pk_found = pk.Where(x => x.ToLower() == column.ColumnName.ToLower()).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(pk_found))
                {
                    // если это не PK - обновляем
                    if (set_fields != "") set_fields = set_fields + Environment.NewLine;
                    set_fields = set_fields + "\t" + ColumnName + " = t." + ColumnName + "::" + ColumnPGType(column) + ",";
                }

                if (ins_fields != "") ins_fields = ins_fields + Environment.NewLine;
                ins_fields = ins_fields + "\t" + "t." + ColumnName + "::" + ColumnPGType(column) + ",";
            }
            set_fields = set_fields.Trim().TrimEnd(',');
            ins_fields = ins_fields.Trim().TrimEnd(',');

            // Проверка на региональность в начале скрипта
            if (isAddRegion && txtRegion != "0")
            {
                sb.Append(Environment.NewLine + "DO $script$");
                sb.Append(Environment.NewLine + "BEGIN");
                sb.Append(Environment.NewLine);
                sb.Append(StartRegion(txtRegion, "", Environment.NewLine));
            }

            //Временная промежуточная таблица
            sb.Append(Environment.NewLine + "-- Промежуточная таблица со структурой аналогичной таблице назначения");
            sb.Append(Environment.NewLine + @"CREATE TABLE " + tmptable + @" AS 
SELECT " + fields + @"
FROM " + FullTableNameToScript + @"
LIMIT 0;");
            sb.Append(Environment.NewLine);

            //Модификация структуры и данных в промежуточной таблицы
            sb.Append(Environment.NewLine + "-- Модификация структуры промежуточной таблицы");
            sb.Append(Environment.NewLine + alters);
            sb.Append(Environment.NewLine);

            //Загрузить данные в промежуточную таблицу
            string CSVFileName = Path.GetFileName(CSVFile);
            sb.Append(Environment.NewLine + "-- Загрузить данные из CSV-файла");
            sb.Append(Environment.NewLine + "-- select count(*) from " + tmptable);
            sb.Append(Environment.NewLine + "COPY " + tmptable + " FROM '/tmp/pg/" + CSVFileName + "' WITH (format 'csv', delimiter '$');");
            sb.Append(Environment.NewLine);

            //Модификация данных в промежуточной таблице
            sb.Append(Environment.NewLine + "-- Модификация данных в промежуточной таблице");
            sb.Append(Environment.NewLine + updates);
            sb.Append(Environment.NewLine);

            //Обновить данные в таблице назначения
            sb.Append(Environment.NewLine + "-- Обновить данные в таблице назначения");
            sb.Append(Environment.NewLine + "UPDATE " + FullTableNameToScript + " SET");
            sb.Append(Environment.NewLine + "\t" + set_fields);
            sb.Append(Environment.NewLine + "FROM " + tmptable + " t");
            sb.Append(Environment.NewLine + "INNER JOIN " + FullTableNameToScript + " d ON " + pk_join);

            if (cbAddFK == 2)
            {
                // добавить проверку на существование внешних ключей
                foreach (var item in this.ListTableFK
                    .Where(x => x.Value.FKDeep == 1)
                    )
                {
                    sb.Append(item.Value.WhereExistsCondition("", "t", "", "", " LIMIT 1"));
                }
            }

            sb.Append(Environment.NewLine + ";");
            sb.Append(Environment.NewLine);

            //Добавить данные в таблицу назначения
            sb.Append(Environment.NewLine + "-- Добавить данные в таблицу назначения");
            sb.Append(Environment.NewLine + "INSERT INTO " + FullTableNameToScript + " (" + fields + ")");
            sb.Append(Environment.NewLine + "SELECT");
            sb.Append(Environment.NewLine + "\t" + ins_fields);
            sb.Append(Environment.NewLine + "FROM " + tmptable + " t");
            sb.Append(Environment.NewLine + "LEFT JOIN " + FullTableNameToScript + " d ON " + pk_join);
            sb.Append(Environment.NewLine + "WHERE " + pk_is_null);

            if (cbAddFK == 2)
            {
                // добавить проверку на существование внешних ключей
                foreach (var item in this.ListTableFK
                    .Where(x => x.Value.FKDeep == 1)
                    )
                {
                    sb.Append(item.Value.WhereExistsCondition("", "t", "", "", " LIMIT 1"));
                }
            }

            sb.Append(Environment.NewLine + ";");
            sb.Append(Environment.NewLine);

            //Удаление временных объектов
            sb.Append(Environment.NewLine + "-- Удаление временных объектов");
            sb.Append(Environment.NewLine + "DROP TABLE IF EXISTS " + tmptable + ";");
            sb.Append(Environment.NewLine);

            // Проверка на региональность в конце скрипта
            if (isAddRegion && txtRegion != "0")
            {
                sb.Append(FinishRegion(txtRegion, "", Environment.NewLine));
                sb.Append(Environment.NewLine + "END;" + Environment.NewLine + "$script$;");
                sb.Append(Environment.NewLine);
            }

            // Проверка
            sb.Append(Environment.NewLine + "-- Проверка");
            sb.Append(Environment.NewLine + "-- SELECT * FROM " + FullTableNameToScript);

            if (file != null)
            {
                // пишем в файл по частям
                file.Write(sb.ToString().ScriptPartReady());
            }

            return sb.ToString();
        }


        /// <summary>
        /// сгенерировать CSV-файл
        /// </summary>
        /// <param name="win">окно, из которого вызвана функция</param>
        /// <param name="filename">имя файла</param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<string> GenCSV(WinQuery win, string filename)
        {
            // запросить имя файла
            FileStream fs = null;
            Encoding encoding = null;

            switch (this.TargetDB)
            {
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    encoding = new UTF8Encoding(false);
                    break;
                case Utilities.TargetDBType.MSSQL:
                default:
                    encoding = new UnicodeEncoding(true, true);
                    break;
            }

            FileMode mode = FileMode.Create;

            if (string.IsNullOrWhiteSpace(filename))
            {
                filename = Controls.Dialogs.SaveCSVDialog(MainWindow.Task.TaskPath, this.CSVFilename, out fs, out mode);
            }
            else
            {
                fs = new FileStream(filename, mode);
            }

            if (fs != null)
            {
                // сгенерировать CSV
                {
                    StreamWriter file = null;
                    try
                    {
                        file = new StreamWriter(fs, encoding);

                        await this.GenerateCSV(win, file,
                            x =>
                            {
                                if (file != null) file.Dispose();
                                fs.Dispose();
                            });
                    }
                    catch (Exception ex)
                    {
                        if (file != null) file.Dispose();
                        fs.Dispose();

                        App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                }
            }

            return filename;
        }


        /// <summary>
        /// Собрать CSV-скрипт
        /// </summary>
        /// <param name="win">окно, из которого вызвана функция</param>
        /// <param name="file">экземпляр StreamWriter для записи скрипта сразу в файл</param>
        /// <param name="_action_finish">действия после генерации</param>
        public async System.Threading.Tasks.Task GenerateCSV(WinQuery win, StreamWriter file, System.Action<QueryDB> _action_finish)
        {
            StringBuilder sb = new StringBuilder(100000);

            string crlf;

            if (this.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                crlf = Environment.NewLine;
            }
            else
            {
                crlf = "\n";
            }

            /*    string fields = "";
            if (this.TargetDB == Utilities.TargetDBType.MSSQL) 
            {
                // Заголовок
                foreach (DataColumn column in this.DataTableUsedColumns)
                {
                    string ColumnName = column.ColumnName;
                    //if (this.TargetDB == Utilities.TargetDBType.EMD)
                    //{
                    //    ColumnName = "\"" + ColumnName + "\"";
                    //}

                    // для PG SQL имя колонки в нижний регистр
                    //if (this.TargetDB == Utilities.TargetDBType.PGSQL)
                    //{
                    //    ColumnName = ColumnName.ToLower();
                    //}

                    // пропускаем rowversion
                    if (ColumnName.ToLower().IndexOf("rowversion") != -1) continue;

                    if (fields != "") fields = fields + "$";
                    fields = fields + ColumnName;
                }
                sb.Append(fields + crlf);
            }
            */

            string now = DateTime.Now.ToString("dd.MM.yyyy H:mm:ss");


            // отображаем progress bar
            int current = 0;
            int maximum = this.DataTable.Rows.Count;
            int step = this.DataTable.Rows.Count / 100;
            if (step == 0) step = 1;
            WinProgress winProgress = new WinProgress();
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (win != null)
                {
                    win.pbProgress.Minimum = 0;
                    win.pbProgress.Maximum = 100;
                    win.pbProgress.Value = 0;
                }
                else
                {
                    winProgress.pbProgress.Minimum = 0;
                    winProgress.pbProgress.Maximum = 100;
                    winProgress.pbProgress.Value = 0;
                    winProgress.Show();
                }
            });

            await System.Threading.Tasks.Task.Run(() =>
            {
                // Перебираем строки
                foreach (DataRow row in this.DataTable.Rows)
                {
                    string values = "";

                    foreach (DataColumn column in this.DataTableUsedColumns)
                    {
                        string ColumnName = column.ColumnName;
                        if (this.TargetDB == Utilities.TargetDBType.EMD)
                        {
                            ColumnName = "\"" + ColumnName + "\"";
                        }
                        // для PG SQL имя колонки в нижний регистр
                        if (this.TargetDB == Utilities.TargetDBType.PGSQL) ColumnName = ColumnName.ToLower();

                        // пропускаем rowversion
                        if (ColumnName.ToLower().IndexOf("rowversion") != -1) continue;

                        if (values != "") values = values + "$";
                        if (this.TargetDB == Utilities.TargetDBType.MSSQL)
                        {
                            values = values + ColumnCSVValue(row, column, now);
                        }
                        else
                        {
                            values = values + "\"" + ColumnCSVValue(row, column, now) + "\"";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(values))
                    {
                        sb.Append(values + crlf);

                        if ((file != null) && ((sb.Length + values.Length) > (sb.Capacity - 10)))
                        {
                            // пишем в файл по частям
                            file.Write(sb.ToString());
                            sb.Clear();
                        }
                    }

                    // обновляем progress bar
                    current++;
                    if (current % step == 0)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (win != null)
                            {
                                win.pbProgress.Value = (int)((float)current / (float)maximum * 100);
                            }
                            else
                            {
                                winProgress.pbProgress.Value = (int)((float)current / (float)maximum * 100);
                            }
                        });

                        if (win != null)
                        {
                            if (win.isCancelGenereate)
                            {
                                return;
                            }
                        }
                    }
                }
                
            });

            if (file != null)
            {
                // пишем в файл по частям
                file.Write(sb.ToString());
            }

            // финальные действия
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                if (win != null)
                {
                    if (!win.isCancelGenereate)
                    {
                        win.pbProgress.Value = 100;
                    }
                }
                else
                {
                    winProgress.Close();
                }

                // выполнить финишное действие
                if (_action_finish != null)
                {
                    _action_finish(this);
                }
            });

        }
    }
}
