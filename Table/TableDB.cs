// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using Microsoft.Office.Interop.Excel;
using Microsoft.SqlServer.Management.Smo;
using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;

namespace SQLGen
{

    // =========================================================================================================
    /// <summary>Класс Таблица</summary>
    public class TableDB : INotifyPropertyChanged
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

        // номер скрипта
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
                if (!int.TryParse(_script_num, out int _num)) _script_num = "0";
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

        /// <summary>Тип скрипта</summary>
        public Utilities.ScriptType ScriptType { get; set; }

        /// <summary>Тип скрипта - для использования в имени файла</summary>
        public string ScriptTypeToFilename
        {
            get
            {
                switch (this.ScriptType)
                {
                    case Utilities.ScriptType.CREATE:
                        return "create";
                    case Utilities.ScriptType.DROP:
                        return "drop";
                    case Utilities.ScriptType.ALTER:
                    default:
                        return "alter";
                }
            }
        }

        /// <summary>Тип таблицы</summary>
        public Utilities.TableType TableType { get; set; }

        /// <summary>Оригинальная таблица (до изменений)</summary>
        public TableInfo TableOrig { get; set; }

        /// <summary>Измененная таблица</summary>
        public TableInfo TableEdit { get; set; }

        /// <summary>Измененная таблица - для генерации скриптов</summary>
        public TableInfo TableGen { get; set; }

        /// <summary>Список индексов</summary>
        public List<IndexDB> ListIndex { get; set; }

        /// <summary>
        /// список таблиц в EvnClass
        /// </summary>
        public List<string> ListEvnClass = new List<string>();

        /// <summary>Информация для nsi.RefTableRegistry</summary>
        public RefTableRegistry RefTableRegistry { get; set; }

        /// <summary>Информация для stg.LocalDBList</summary>
        public LocalDBList LocalDBList { get; set; }

        private bool _isadddrop;
        /// <summary>Флаг добавления Drop</summary>
        public bool isAddDrop
        {
            get
            {
                return _isadddrop;
            }
            set
            {
                _isadddrop = value == true;

                OnPropertyChanged("isAddDrop");
            }
        }

        private bool _isreglament;
        /// <summary>=true - Генерация скрипта по регламенту</summary>
        [JsonIgnore]
        public bool isReglament
        {
            get
            {
                return _isreglament;
            }
            set
            {
                _isreglament = value == true;

                OnPropertyChanged("isReglament");
            }
        }

        private bool _isaddindex;
        /// <summary>=true - Генерация скрипта вместе с индексом</summary>
        [JsonIgnore]
        public bool isAddIndex
        {
            get
            {
                return _isaddindex;
            }
            set
            {
                _isaddindex = value == true;

                OnPropertyChanged("isAddIndex");
            }
        }

        private bool _isonlyexist;
        /// <summary>=true - Генерация скрипта по существующим данным таблицы, не пытаться ее "улучшить"</summary>
        [JsonIgnore]
        public bool isOnlyExist
        {
            get
            {
                return _isonlyexist;
            }
            set
            {
                _isonlyexist = value == true;

                OnPropertyChanged("isOnlyExist");
            }
        }

        /// <summary>Флаг региональной схемы</summary>
        [JsonIgnore]
        public bool isSchemaRegion;

        /// <summary>Флаг проверки на региональность</summary>
        [JsonIgnore]
        public bool isAddRegion;

        /// <summary>текст итогового SQL-скрипта</summary>
        public string SQLScript { get; set; }

        /// <summary>текст команды для генерации хранимок</summary>
        public string SQLProcCommand { get; set; }

        /// <summary>текст итогового скрипта хранимок</summary>
        public string SQLProcScript { get; set; }

        /// <summary>
        /// имя файла со скриптом
        /// </summary>
        /// <param name="tableinfo">экземпляр TableInfo</param>
        /// <returns></returns>
        public static string ScriptFilename(TableInfo tableinfo)
        {
            string s = 
                tableinfo.ParentTableDB.PrefixToFilename + " " + 
                MainWindow.Task.TaskNumberToFilename + " " + 
                tableinfo.ParentTableDB.ScriptNumberToFilename + " " + 
                tableinfo.ParentTableDB.ScriptTypeToFilename;

            if (!string.IsNullOrWhiteSpace(tableinfo.TableNameToFilename))
            {
                s = s + " " + tableinfo.TableNameToFilename;
            }

            s = s + ".sql";
            
            return s;
        }

        /// <summary>
        /// имя файла со скриптом для индекса
        /// </summary>
        /// <param name="tableinfo">экземпляр TableInfo</param>
        /// <returns></returns>
        public static string IndexFilename(TableInfo tableinfo)
        {
            string s = 
                tableinfo.ParentTableDB.PrefixToFilename + " " + 
                MainWindow.Task.TaskNumberToFilename + " " + 
                tableinfo.ParentTableDB.ScriptNumberToFilename + " " + 
                "index";

            if (!string.IsNullOrWhiteSpace(tableinfo.TableNameToFilename))
            {
                s = s + " " + tableinfo.TableNameToFilename;
            }

            s = s + ".sql";
            
            return s;
        }

        /// <summary>имя файла со скриптом для добавления в справочник локальных таблиц stg.LocalDBList</summary>
        public string LocalDBListFilename
        {
            get
            {
                {
                    return 
                        this.PrefixToFilename + " " + 
                        MainWindow.Task.TaskNumberToFilename + " " + 
                        this.ScriptNumberToFilename + " " +
                        "insert stg LocalDBList.sql";
                }
            }
        }

        /// <summary>имя файла со скриптом для добавления в справочник nsi.RefTableRegistry</summary>
        public string RefTableRegistryFilename
        {
            get
            {
                {
                    return 
                        this.PrefixToFilename + " " + 
                        MainWindow.Task.TaskNumberToFilename + " " + 
                        this.ScriptNumberToFilename + " " + 
                        "insert nsi RefTableRegistry.sql";
                }
            }
        }

        /// <summary>Конструктор TableDB - инициализация значений по умолчанию</summary>
        public TableDB()
        {
            this.GITProject = "dev_promed_pg";
            this.ScriptType = Utilities.ScriptType.CREATE;
            this.TableType = Utilities.TableType.DICT;
            this.isAddDrop = false;
            this.isReglament = true;
            this.isAddIndex = false;
            this.isOnlyExist = false;
            TableOrig = new TableInfo(this);
            TableEdit = new TableInfo(this);
            TableGen = null; // не инициализируем!
            ListIndex = new List<IndexDB>();
        }

        /*
        /// <summary>
        /// Заполнить текущий экземпляр TableDB значениями из другого экземпляра TableDB
        /// </summary>
        /// <param name="_table">экземпляр TableDB</param>
        public void Fill(TableDB _table)
        {
            if (_table != null)
            {
                this.GITProject = _table.GITProject;
                this.ScriptType = _table.ScriptType;
                this.TableType = _table.TableType;
                this.TableOrig.Fill(this, _table.TableOrig);
                this.TableEdit.Fill(this, _table.TableEdit);
                this.isAddDrop = _table.isAddDrop;
                this.isReglament = _table.isReglament;
                this.isAddIndex = _table.isAddIndex;
                this.isOnlyExist = _table.isOnlyExist;
                this.SQLScript = _table.SQLScript;

                if (_table.ListIndex != null)
                {
                    foreach (var index in _table.ListIndex) this.AddIndex(index.IndexName, index.IsUnique_string, index.Predicat, index.Include, index.Where, index.IsNullsNotDistinct_string, index.IndexToDel, index.IsProd_string, index.IsReg_string, index.IsReport_string);
                }

                //this.ScriptFilename = _table.ScriptFilename;
            }
        }
        */

        /// <summary>
        /// Найти индекс в списке
        /// </summary>
        /// <param name="index">описание индекса</param>
        /// <returns></returns>
        public IndexDB FindIndex(IndexDB index)
        {
            return this.ListIndex.Find(x => x.FullIndexNameCompare == index.FullIndexNameCompare);
        }

        /// <summary>
        /// Добавить индекс в список
        /// </summary>
        /// <param name="IndexName">Название индекса</param>
        /// <param name="IsUnique_string">уникальность индекса</param>
        /// <param name="Predicat">список индексируемых полей</param>
        /// <param name="Include">список полей для INCLUDE</param>
        /// <param name="Where">условие для WHERE</param>
        /// <param name="IsNullsNotDistinct_string">nulls not distinct для уникальных индексов на ПГ</param>
        /// <param name="IndexToDel">список индексов для удаления</param>
        /// <param name="IsProd_string">для основной БД</param>
        /// <param name="IsReg_string">для реестровой БД</param>
        /// <param name="IsReport_string">для отчетной БД</param>
        /// <param name="pAddisDeleted_string">значение параметра paddisdeleted</param>
        /// <param name="pAddisRegion_string">значение параметра paddisregion</param>
        /// <returns></returns>
        public IndexDB AddIndex(string IndexName, string IsUnique_string, string Predicat, string Include, string Where, string IsNullsNotDistinct_string, string IndexToDel, string IsProd_string, string IsReg_string, string IsReport_string, string pAddisDeleted_string, string pAddisRegion_string)
        {
            IndexDB newIndex = new IndexDB(this);

            newIndex.IndexName = IndexName;
            newIndex.IsUnique_string = IsUnique_string;
            newIndex.Predicat = Predicat;
            newIndex.Include = Include;
            newIndex.Where = Where;
            newIndex.IsNullsNotDistinct_string = IsNullsNotDistinct_string;
            newIndex.IndexToDel = IndexToDel;
            newIndex.IsProd_string = IsProd_string;
            newIndex.IsReg_string = IsReg_string;
            newIndex.IsReport_string = IsReport_string;
            newIndex.pAddisDeleted_string = pAddisDeleted_string;
            newIndex.pAddisRegion_string = pAddisRegion_string;

            IndexDB existIndex = FindIndex(newIndex);

            if (existIndex == null)
            {
                this.ListIndex.Add(newIndex);
                existIndex = newIndex;
            }
            else
            {
                existIndex.IndexName = newIndex.IndexName;
                existIndex.IsUnique_string = newIndex.IsUnique_string;
                existIndex.Predicat = newIndex.Predicat;
                existIndex.Include = newIndex.Include;
                existIndex.Where = newIndex.Where;
                existIndex.IsNullsNotDistinct_string = newIndex.IsNullsNotDistinct_string;
                existIndex.IndexToDel = newIndex.IndexToDel;
                existIndex.IsProd_string = newIndex.IsProd_string;
                existIndex.IsReg_string = newIndex.IsReg_string;
                existIndex.IsReport_string = newIndex.IsReport_string;
                existIndex.pAddisDeleted_string = newIndex.pAddisDeleted_string;
                existIndex.pAddisRegion_string = newIndex.pAddisRegion_string;
            }

            return existIndex;
        }

        /// <summary>
        /// Переименовать таблицу - скрипт
        /// </summary>
        /// <param name="oldTableInfo">оригинальная таблица</param>
        /// <param name="newTableInfo">измененная таблица</param>
        /// <returns></returns>
        public static string RenameTableToScript(TableInfo oldTableInfo, TableInfo newTableInfo)
        {
            switch (newTableInfo.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        return
                            "EXEC sys.sp_rename '" + oldTableInfo.FullTableNameToScript + "', '" + newTableInfo.TableNameToScript + "'";
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        string foreign = " ";

                        if (newTableInfo.isForeignTable)
                        {
                            // если это внешняя таблица
                            foreign = " " + newTableInfo.ForeignWord + " ";
                        }

                        return
                            "ALTER" + foreign + "TABLE IF EXISTS " + oldTableInfo.FullTableNameToScript + Environment.NewLine +
                            "\tRENAME TO " + newTableInfo.FullTableNameToScript + ";";
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// Переименовать поле - скрипт
        /// </summary>
        /// <param name="oldRow">предыдущее описание поля</param>
        /// <param name="newRow">новое описание поля</param>
        /// <returns></returns>
        public static string RenameFieldToScript(FieldDB oldRow, FieldDB newRow)
        {
            switch (newRow.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        return "EXEC sys.sp_rename '" + oldRow.ParentTableInfo.FullTableNameToScript + "." + oldRow.FieldNameToScript + "', '" + newRow.FieldNameToScript + "', 'COLUMN'" + Environment.NewLine;
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        string foreign = " ";
                        if (newRow.ParentTableInfo.isForeignTable)
                        {
                            // если это внешняя таблица
                            foreign = " " + newRow.ParentTableInfo.ForeignWord + " ";
                        }

                        return
                            "ALTER" + foreign + "TABLE IF EXISTS " + oldRow.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                            "\tRENAME COLUMN " + oldRow.FieldNameToScript + " TO " + newRow.FieldNameToScript + ";" + Environment.NewLine;
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// Собрать скрипт для индекса
        /// </summary>
        /// <param name="ScriptType">Тип скрипта</param>
        /// <param name="isAddTitle">=true - добавить заголовок</param>
        /// <param name="isDDL">=true - генерация индекса командой CREATE INDEX</param>
        /// <param name="isAddRegion">=true - добавить условие региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="index">описание индекса</param>
        /// <returns></returns>
        public static string GenerateIndexScript(Utilities.ScriptType ScriptType, bool isAddTitle, bool isDDL, bool isAddRegion, string txtRegion, IndexDB index)
        {
            string ScriptIndex = "";
            string ScriptRegionBegin = "";
            string ScriptRegionEnd = "";
            index.ParentTableDB.isAddRegion = isAddRegion;
            index.ParentTableDB.isSchemaRegion = false;

            if (
                (
                    isAddRegion ||
                    (ScriptType == Utilities.ScriptType.DROP && index.IsUnique)
                ) && (
                    (index.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || 
                    (index.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)
                )
            )
            {
                ScriptRegionBegin +=
                    "DO $scriptindex$" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine +
                    Environment.NewLine;
            }

            // Проверка на региональность
            if (isAddRegion)
            {
                index.ParentTableDB.isSchemaRegion = Utilities.Databases.regex_region
                    .IsMatch(index.ParentTableDB.TableEdit.SchemaNameCompare);

                if (index.ParentTableDB.TargetDB == Utilities.TargetDBType.MSSQL)
                {
                    ScriptRegionBegin +=
                        "IF (dbo.getregion() = " + txtRegion.Trim() + " OR db_name() IN ('ProMedWebRelease', 'ProMedTest', 'promeddev'))";

                    if (index.ParentTableDB.isSchemaRegion)
                    {
                        ScriptRegionBegin += Environment.NewLine +
                            "IF EXISTS (" + Environment.NewLine +
                            "\tSELECT 1" + Environment.NewLine +
                            "\tFROM sys.schemas s WITH (nolock)" + Environment.NewLine +
                            "\tWHERE s.name = '" + index.ParentTableDB.TableEdit.SchemaNameToSeek + "'" + Environment.NewLine +
                            ")";
                    }

                    ScriptRegionBegin += Environment.NewLine +
                        "BEGIN" + Environment.NewLine;


                    ScriptRegionEnd += Environment.NewLine +
                        Environment.NewLine +
                        "END" + Environment.NewLine;
                }

                if (
                    (index.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || 
                    (index.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)
                )
                {
                    ScriptRegionBegin +=
                        "IF (dbo.getregion() = " + txtRegion + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))";

                    if (index.ParentTableDB.isSchemaRegion)
                    {
                        ScriptRegionBegin += Environment.NewLine +
                            "AND EXISTS (" + Environment.NewLine +
                            "\tSELECT 1" + Environment.NewLine +
                            "\tFROM pg_namespace" + Environment.NewLine +
                            "\tWHERE nspname = '" + index.ParentTableDB.TableEdit.SchemaNameToSeek + "'" + Environment.NewLine +
                            ")";
                    }

                    ScriptRegionBegin += Environment.NewLine +
                        "THEN" + Environment.NewLine +
                        Environment.NewLine;

                    ScriptRegionEnd += Environment.NewLine +
                        "END IF;" + Environment.NewLine;
                }
            }

            if (
                (
                    isAddRegion ||
                    (ScriptType == Utilities.ScriptType.DROP && index.IsUnique)
                ) && (
                    (index.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || 
                    (index.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)
                )
            )
            {
                ScriptRegionEnd += Environment.NewLine +
                    "END;" + Environment.NewLine +
                    "$scriptindex$;" + Environment.NewLine;
            }

            // Основной скрипт
            switch (ScriptType)
            {
                case Utilities.ScriptType.DROP:

                    ScriptIndex += "-- Drop index: " + index.IndexNameToScript;

                    if (index.ParentTableDB.TargetDB == Utilities.TargetDBType.MSSQL)
                    {
                        if (isDDL)
                        {
                            if (index.IsUnique)
                            {
                                ScriptIndex += Environment.NewLine +
                                "IF OBJECT_ID(N'" + index.ParentTableDB.TableEdit.FullTableNameToScript + "', 'U') IS NOT NULL" + Environment.NewLine +
                                "BEGIN" + Environment.NewLine +Environment.NewLine +
                                "\tALTER TABLE " + index.ParentTableDB.TableEdit.FullTableNameToScript +
                                " DROP CONSTRAINT IF EXISTS " + index.IndexNameToScript + Environment.NewLine +
                                Environment.NewLine +
                                "\tDROP INDEX IF EXISTS " + index.IndexNameToScript + 
                                " ON " + index.ParentTableDB.TableEdit.FullTableNameToScript + Environment.NewLine +
                                "END";
                            }
                            else
                            {
                                ScriptIndex += Environment.NewLine +
                                "IF OBJECT_ID(N'" + index.ParentTableDB.TableEdit.FullTableNameToScript + "', 'U') IS NOT NULL" + Environment.NewLine +
                                "BEGIN" + Environment.NewLine +
                                "\tDROP INDEX IF EXISTS " + index.IndexNameToScript +
                                " ON " + index.ParentTableDB.TableEdit.FullTableNameToScript + Environment.NewLine +
                                "END";
                            }
                        }
                        else
                        {
                            if (index.IsUnique)
                            {
                                ScriptIndex += Environment.NewLine +
                                "ALTER TABLE " + index.ParentTableDB.TableEdit.FullTableNameToScript +
                                " DROP CONSTRAINT IF EXISTS " + index.IndexNameToScript + Environment.NewLine +
                                Environment.NewLine +
                                "EXEC dbo.p_IndexDelete" + Environment.NewLine +
                                "\t@pIndexName = '" + index.IndexNameToScript + "'," + Environment.NewLine +
                                "\t@task = '" + MainWindow.Task.TaskNumber + "'," + Environment.NewLine +
                                "\t@pSchema = '" + index.ParentTableDB.TableEdit.SchemaNameToScript + "'," + Environment.NewLine +
                                "\t@pTable = '" + index.ParentTableDB.TableEdit.TableNameToScript + "'";
                            }
                            else
                            {
                                ScriptIndex += Environment.NewLine +
                                "EXEC dbo.p_IndexDelete" + Environment.NewLine +
                                "\t@pIndexName = '" + index.IndexNameToScript + "'," + Environment.NewLine +
                                "\t@task = '" + MainWindow.Task.TaskNumber + "'," + Environment.NewLine +
                                "\t@pSchema = '" + index.ParentTableDB.TableEdit.SchemaNameToScript + "'," + Environment.NewLine +
                                "\t@pTable = '" + index.ParentTableDB.TableEdit.TableNameToScript + "'";
                            }
                        }
                    }
                    else if (
                        (index.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || 
                        (index.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD) //-V3063
                    )
                    {
                        if (isDDL)
                        {
                            if (index.IsUnique)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "ALTER TABLE IF EXISTS " + index.ParentTableDB.TableEdit.FullTableNameToScript +
                                    " DROP CONSTRAINT IF EXISTS " + index.IndexNameToScript + ";" + Environment.NewLine +
                                    Environment.NewLine +
                                    "DROP INDEX IF EXISTS " + index.FullIndexNameToScript + ";";
                            }
                            else
                            {
                                ScriptIndex += Environment.NewLine +
                                    "DROP INDEX IF EXISTS " + index.FullIndexNameToScript + ";";
                            }
                        }
                        else
                        {
                            if (index.IsUnique)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "ALTER TABLE IF EXISTS " + index.ParentTableDB.TableEdit.FullTableNameToScript + 
                                    " DROP CONSTRAINT IF EXISTS " + index.IndexNameToScript + ";" + Environment.NewLine +
                                    Environment.NewLine +
                                    "PERFORM dbo.fn_indexdelete (" + Environment.NewLine +
                                    "\tptask := '" + MainWindow.Task.TaskNumber + "'," + Environment.NewLine +
                                    "\tpindexname := '" + index.IndexNameToSeek + "'," + Environment.NewLine +
                                    "\tpschema := '" + index.ParentTableDB.TableEdit.SchemaNameToScript + "'," + Environment.NewLine +
                                    "\tptable := '" + index.ParentTableDB.TableEdit.TableNameToScript + "'" + Environment.NewLine +
                                    ");";
                            }
                            else
                            {
                                if (isAddRegion)
                                {
                                    ScriptIndex += Environment.NewLine +
                                        "PERFORM dbo.fn_indexdelete (";
                                }
                                else
                                {
                                    ScriptIndex += Environment.NewLine +
                                        "SELECT * FROM dbo.fn_indexdelete (";
                                }

                                ScriptIndex += Environment.NewLine +
                                    "\tptask := '" + MainWindow.Task.TaskNumber + "'," + Environment.NewLine +
                                    "\tpindexname := '" + index.IndexNameToSeek + "'," + Environment.NewLine +
                                    "\tpschema := '" + index.ParentTableDB.TableEdit.SchemaNameToScript + "'," + Environment.NewLine +
                                    "\tptable := '" + index.ParentTableDB.TableEdit.TableNameToScript + "'" + Environment.NewLine +
                                    ");";
                            }
                        }
                    }
                    break;
                case Utilities.ScriptType.ALTER:
                case Utilities.ScriptType.CREATE:
                default:

                    ScriptIndex += "-- Add index: " + index.IndexNameToScript;

                    if (index.ParentTableDB.TargetDB == Utilities.TargetDBType.MSSQL)
                    {
                        if (index.ParentTableDB.TableEdit.isPartitionTable)
                        {
                            // индексы для секционированных таблиц на MS создаем командой
                            isDDL = true;
                        }

                        if (isDDL)
                        {
                            ScriptIndex += Environment.NewLine +
                                "IF OBJECT_ID(N'" + index.ParentTableDB.TableEdit.FullTableNameToScript + "', 'U') IS NOT NULL" + Environment.NewLine +
                                "AND NOT EXISTS (" + Environment.NewLine +
                                "\tSELECT TOP(1) 1" + Environment.NewLine +
                                "\tFROM sys.sysindexes WITH (nolock)" + Environment.NewLine +
                                "\tWHERE name = '" + index.IndexNameToScript + "'" + Environment.NewLine +
                                "\tAND id = OBJECT_ID(N'" + index.ParentTableDB.TableEdit.FullTableNameToScript + "', 'U')" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "BEGIN" + Environment.NewLine +
                                "\tCREATE";

                            if (index.IsUnique)
                            {
                                ScriptIndex += " UNIQUE";
                            }

                            ScriptIndex +=
                                " NONCLUSTERED INDEX " + index.IndexNameToScript + Environment.NewLine +
                                "\tON " + index.ParentTableDB.TableEdit.FullTableNameToScript + " (" + index.PredicatToScript + ")";

                            if (!string.IsNullOrWhiteSpace(index.IncludeToScript))
                            {
                                ScriptIndex += Environment.NewLine + "\tINCLUDE (" + index.IncludeToScript + ")";
                            }

                            string _where = index.WhereToScript(false);

                            if (!string.IsNullOrWhiteSpace(_where))
                            {
                                ScriptIndex += Environment.NewLine + "\tWHERE " + _where;
                            }

                            ScriptIndex += Environment.NewLine +
                                "\tWITH (" + Environment.NewLine +
                                "\t\tMAXDOP = 12," + Environment.NewLine +
                                "\t\tONLINE = ON," + Environment.NewLine +
                                "\t\tFILLFACTOR = 75," + Environment.NewLine +
                                "\t\tSTATISTICS_NORECOMPUTE = ON" + Environment.NewLine +
                                "\t)" + Environment.NewLine;

                            if (index.ParentTableDB.TableEdit.isPartitionTable)
                            {
                                ScriptIndex += $"\tON {index.ParentTableDB.TableEdit.PartitionSchemeName} ({index.ParentTableDB.TableEdit.PartitionField})" + Environment.NewLine;
                            }
                            else
                            {
                                ScriptIndex += "\tON [SECONDARY]" + Environment.NewLine;
                            }

                            ScriptIndex += "END";

                            if (!string.IsNullOrWhiteSpace(index.IndexToDelToScript))
                            {
                                ScriptIndex += Environment.NewLine;

                                var arr = index.IndexToDelToScript
                                    .Replace(" ", string.Empty)
                                    .Replace("\t", ",")
                                    .Replace("\r", ",")
                                    .Replace("\n", ",")
                                    .Replace(",,", ",")
                                    .Replace(",,", ",")
                                    .Replace(";", ",")
                                    .Replace(",,", ",")
                                    .Split(',');

                                foreach (var indextodel in arr)
                                {
                                    ScriptIndex += Environment.NewLine +
                                        "IF OBJECT_ID(N'" + index.ParentTableDB.TableEdit.FullTableNameToScript + "', 'U') IS NOT NULL" + Environment.NewLine +
                                        "BEGIN" + Environment.NewLine +
                                        "\tDROP INDEX IF EXISTS " + indextodel + 
                                        " ON " + index.ParentTableDB.TableEdit.FullTableNameToScript + Environment.NewLine +
                                        "END";
                                }
                            }
                        }
                        else
                        {
                            ScriptIndex += Environment.NewLine +
                                "EXEC dbo.p_IndexCreate" + Environment.NewLine +
                                "\t@pIndexName = '" + index.IndexNameToScript + "'," + Environment.NewLine +
                                "\t@task = '" + MainWindow.Task.TaskNumber + "'," + Environment.NewLine +
                                "\t@pSchema = '" + index.ParentTableDB.TableEdit.SchemaNameToScript + "'," + Environment.NewLine +
                                "\t@pTable = '" + index.ParentTableDB.TableEdit.TableNameToScript + "'," + Environment.NewLine +
                                "\t@pPredicat = '" + index.PredicatToScript + @"',";

                            if (!string.IsNullOrWhiteSpace(index.IncludeToScript))
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\t@pInclude = '" + index.IncludeToScript + "',";
                            }

                            string _where = index.WhereToScript(false);

                            if (!string.IsNullOrWhiteSpace(_where))
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\t@pWhere = '" + _where.Replace("'","''") + "',";
                            }

                            if (!string.IsNullOrWhiteSpace(index.IndexToDelToScript))
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\t@pIndexToDel = '" + index.IndexToDelToScript + "',";
                            }

                            if (index.IsUnique)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\t@pIsUnique = " + index.IsUniqueToScript + ",";
                            }

                            if (index.IsProd)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\t@pIsProd = " + index.IsProdToScript + ",";
                            }

                            if (index.IsReg)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\t@pIsReg = " + index.IsRegToScript + ",";
                            }

                            if (index.IsReport)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\t@pIsReport = " + index.IsReportToScript + ",";
                            }

                            ScriptIndex = ScriptIndex.TrimEnd(',');
                        }
                    }
                    else if (
                        (index.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || 
                        (index.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD) //-V3063
                    ) 
                    {
                        if (!isDDL)
                        {
                            if (isAddRegion)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "PERFORM dbo.fn_indexcreate (";
                            }
                            else
                            {
                                ScriptIndex += Environment.NewLine +
                                    "SELECT * FROM dbo.fn_indexcreate (";
                            }

                            ScriptIndex += Environment.NewLine +
                                "\tptask := '" + MainWindow.Task.TaskNumber + "'," + Environment.NewLine +
                                "\tpindexname := '" + index.IndexNameToSeek + "'," + Environment.NewLine +
                                "\tpschema := '" + index.ParentTableDB.TableEdit.SchemaNameToScript + "'," + Environment.NewLine +
                                "\tptable := '" + index.ParentTableDB.TableEdit.TableNameToScript + "'," + Environment.NewLine +
                                "\tppredicat := '" + index.PredicatToScript + "',";

                            if (!string.IsNullOrWhiteSpace(index.IncludeToScript))
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\tpinclude := '" + index.IncludeToScript + "',";
                            }

                            string _where = index.WhereToScript(true);

                            if (!string.IsNullOrWhiteSpace(_where))
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\tpwhere := '" + _where.Replace("'", "''") + "',";
                            }

                            if (!string.IsNullOrWhiteSpace(index.IndexToDelToScript))
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\tpindextodel := '" + index.IndexToDelToScript + "',";
                            }

                            if (index.IsUnique)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\tpisunique := " + index.IsUniqueToScript + ",";
                            }

                            if (index.IsNullsNotDistinct)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\tpisnotdistinct := " + index.IsNullsNotDistinctToScript + ",";
                            }

                            if (index.pAddisDeleted != null)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\tpaddisdeleted := " + index.pAddisDeletedToScript + ",";
                            }
                            else if (index.hasWhereDeleted)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\tpaddisdeleted := 2,";
                            }

                            if (index.pAddisRegion != null)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\tpaddisregion := " + index.pAddisRegionToScript + ",";
                            } 
                            else if (index.hasWhereRegionId)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\tpaddisregion := 2,";
                            }

                            ScriptIndex = ScriptIndex.TrimEnd(',');

                            ScriptIndex += Environment.NewLine +
                                ");";
                        }
                        else
                        {
                            if (!isAddRegion)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "DO $scriptindex$" + Environment.NewLine +
                                    "BEGIN" + Environment.NewLine + Environment.NewLine;
                            }

                            if (index.IsUnique)
                            {
                                ScriptIndex += Environment.NewLine +
                                "IF EXISTS (" + Environment.NewLine +
                                "\tSELECT 1" + Environment.NewLine +
                                "\tFROM pg_tables" + Environment.NewLine +
                                "\tWHERE schemaname iLIKE '" + index.ParentTableDB.TableEdit.SchemaNameToSeekForLike + @"'" + Environment.NewLine +
                                "\tAND tablename iLIKE '" + index.ParentTableDB.TableEdit.TableNameToSeekForLike + @"'" + Environment.NewLine +
                                "\tLIMIT 1" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "THEN" + Environment.NewLine +
                                "\tCREATE UNIQUE";
                            }
                            else
                            {
                                ScriptIndex += Environment.NewLine +
                                "IF EXISTS (" + Environment.NewLine +
                                "\tSELECT 1" + Environment.NewLine +
                                "\tFROM pg_tables" + Environment.NewLine +
                                "\tWHERE schemaname iLIKE '" + index.ParentTableDB.TableEdit.SchemaNameToSeekForLike + @"'" + Environment.NewLine +
                                "\tAND tablename iLIKE '" + index.ParentTableDB.TableEdit.TableNameToSeekForLike + @"'" + Environment.NewLine +
                                "\tLIMIT 1" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "AND NOT EXISTS (" + Environment.NewLine +
                                "\tSELECT 1" + Environment.NewLine +
                                "\tFROM pg_index i" + Environment.NewLine +
                                "\tINNER JOIN LATERAL (" + Environment.NewLine +
                                "\t\tSELECT string_agg(concat_ws(' ',pg_get_indexdef(i.indexrelid, p_i, true)), ', ' ORDER BY p_i)" + Environment.NewLine +
                                "\t\tFROM generate_series(1, i.indnkeyatts) AS p_i" + Environment.NewLine +
                                "\t) predicat(x) on true" + Environment.NewLine +
                                "\tINNER JOIN pg_class t ON t.oid = i.indrelid" + Environment.NewLine +
                                "\tINNER JOIN pg_catalog.pg_namespace AS n ON n.oid = t.relnamespace" + Environment.NewLine +
                                "\tWHERE 1=1" + Environment.NewLine +
                                "\tAND n.nspname iLIKE '" + index.ParentTableDB.TableEdit.SchemaNameToSeekForLike + @"'" + Environment.NewLine +
                                "\tAND t.relname iLIKE '" + index.ParentTableDB.TableEdit.TableNameToSeekForLike + @"'" + Environment.NewLine +
                                "\tAND COALESCE (predicat.x,'')::varchar iLIKE '" + index.PredicatToSeekForLike+ @"%'" + Environment.NewLine +
                                "\tLIMIT 1" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "THEN" + Environment.NewLine +
                                "\tCREATE";
                            }

                            ScriptIndex +=
                                " INDEX IF NOT EXISTS " + index.IndexNameToScript + Environment.NewLine +
                                "\t\tON " + index.ParentTableDB.TableEdit.FullTableNameToScript + Environment.NewLine +
                                "\t\tUSING btree (" + index.PredicatToScript + ")";

                            if (!string.IsNullOrWhiteSpace(index.IncludeToScript))
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\t\tINCLUDE (" + index.IncludeToScript + ")";
                            }

                            if (index.IsNullsNotDistinct)
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\t\tnulls not distinct";
                            }

                            ScriptIndex += Environment.NewLine +
                                   "\t\tWITH (fillfactor = 75)";

                            string _where = index.WhereToScript(false);

                            if (!string.IsNullOrWhiteSpace(_where))
                            {
                                ScriptIndex += Environment.NewLine +
                                    "\t\tWHERE " + _where;
                            }

                            ScriptIndex += ";";

                            if (!string.IsNullOrWhiteSpace(index.IndexToDelToScript))
                            {
                                ScriptIndex += Environment.NewLine;

                                var arr = index.IndexToDelToScript
                                    .Replace(" ", string.Empty)
                                    .Replace("\t", ",")
                                    .Replace("\r", ",")
                                    .Replace("\n", ",")
                                    .Replace(",,", ",")
                                    .Replace(",,", ",")
                                    .Replace(";", ",")
                                    .Replace(",,", ",")
                                    .Split(',');

                                foreach (var indextodel in arr)
                                {
                                    ScriptIndex += Environment.NewLine +
                                        "\tDROP INDEX IF EXISTS " + index.ParentTableDB.TableEdit.SchemaNameToScript + "." + indextodel + ";";
                                }
                            }

                            ScriptIndex += Environment.NewLine +
                                "END IF;";

                            if (!isAddRegion)
                            {
                                ScriptIndex += Environment.NewLine +
                                    Environment.NewLine +
                                    "END;" + Environment.NewLine +
                                    "$scriptindex$;";
                            }

                        }
                    }
                    break;
            }

            ScriptIndex += Environment.NewLine;

            // заголовок скрипта (информация о задаче)
            string TitleScript = "";

            if (isAddTitle)
            {
                TitleScript = MainWindow.Task.TitleScript(index.ParentTableDB.TargetDB, "index", false, false);

                if (TitleScript == null)
                {
                    TitleScript = "";
                }

                if (!string.IsNullOrWhiteSpace(TitleScript))
                {
                    TitleScript = TitleScript + Environment.NewLine;
                }
            }

            // собираем скрипт
            return (
                TitleScript +
                ScriptRegionBegin +
                ScriptIndex +
                ScriptRegionEnd
                )
                .TrimInnerNewLine()
                .TrimNewLine(Environment.NewLine);
        }

        /// <summary>Собрать скрипт</summary>
        /// <param name="Connect">Подключение к БД-источнику</param>
        /// <param name="isAddTitle">Добавлять комментарий-заголовок с тегами ликвибейз</param>
        /// <param name="isAddRegion">Добавлять проверку региональности</param>
        /// <param name="txtRegion">значение region_id в текстовом виде</param>
        /// <param name="ProcCommand">для возвращения списка с командами вызова генератора</param>
        /// <param name="ProcCommandNum">для возвращения позиции в списке с командами вызова генератора</param>
        /// <param name="isAddProc">Добавить в скрипт вызов генератора</param>
        /// <param name="RowInfo">информация об измененных полях</param>
        /// <param name="table">информация о таблице</param>
        public static string GenerateTableScript(ConnectDB Connect, bool isAddTitle, bool isAddRegion, string txtRegion, out List<string> ProcCommand, out int ProcCommandNum, bool isAddProc, out string RowInfo, TableDB table)
        {
            string ScriptCreateTable = "";
            string ScriptAlterTable = "";
            string ScriptRenameTable = "";
            string ScriptCreatePK = "";
            string ScriptTableDesc = "";

            string ScriptRow = "";
            string ScriptRenameField = "";
            string ScriptRowFK = "";
            string ScriptFieldDesc = "";
            string ScriptSequence = "";
            string ScriptDropFK = "";
            string ScriptDropField = "";
            string ScriptParentFK = "";

            string ScriptDrop = "";
            string ScriptProc = "";
            string ScriptRegionBegin = "";
            string ScriptRegionEnd = "";

            //string ScriptTrigger = "";
            //string ScriptEvnClassCheck = "";
            string ScriptIndex = "";
            string ScriptCreateSetNotNull = "";
            string ScriptAlterList = "";
            List<string> ScriptColumnList = new List<string>();

            RowInfo = "";

            ProcCommand = new List<string>();
            ProcCommandNum = -1;

            table.isAddRegion = isAddRegion;
            table.isSchemaRegion = false;

            // подготовим рабочую копию для последующего авто-улучшения и использования при генерации скрипта
            table.TableGen = table.TableEdit.Copy();

            // Проверка на региональность
            if (isAddRegion)
            {
                table.isSchemaRegion = Utilities.Databases.regex_region.IsMatch(table.TableGen.SchemaNameCompare);

                if (table.TargetDB == Utilities.TargetDBType.MSSQL)
                {
                    ScriptRegionBegin = "IF (dbo.getregion() = " + txtRegion.Trim() + " OR db_name() IN ('ProMedWebRelease', 'ProMedTest', 'promeddev'))";

                    if (table.isSchemaRegion)
                    {
                        ScriptRegionBegin += Environment.NewLine + "IF EXISTS (SELECT 1 FROM sys.schemas s WITH (nolock) WHERE s.name = '" + table.TableGen.SchemaNameToSeek + "')";
                    }

                    ScriptRegionBegin += Environment.NewLine + "BEGIN";

                    ScriptRegionEnd += "END";
                }

                if (
                    (table.TargetDB == Utilities.TargetDBType.PGSQL) || 
                    (table.TargetDB == Utilities.TargetDBType.EMD)
                )
                {
                    ScriptRegionBegin = "DO $script$" + Environment.NewLine + 
                        "BEGIN" + Environment.NewLine + 
                        Environment.NewLine + 
                        "IF (dbo.getregion() = " + txtRegion.Trim() + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))";

                    if (table.isSchemaRegion)
                    {
                        ScriptRegionBegin += Environment.NewLine + "AND EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '" + table.TableGen.SchemaNameToSeek + "') ";
                    }

                    ScriptRegionBegin += Environment.NewLine + "THEN";

                    ScriptRegionEnd += "END IF;" + Environment.NewLine + 
                        Environment.NewLine + 
                        "END;" + Environment.NewLine +
                        "$script$;";
                }
            }

            // DROP
            if (table.isAddDrop == true) // Добавляем DROP
            {
                if (table.TargetDB == Utilities.TargetDBType.MSSQL) // MSSQL
                {
                    ScriptDrop = @"DROP VIEW IF EXISTS " + table.TableGen.FullViewNameToScript + @"
DROP PROCEDURE IF EXISTS " + table.TableGen.FullProcINSToScript + @"
DROP PROCEDURE IF EXISTS " + table.TableGen.FullProcUPDToScript + @"
DROP PROCEDURE IF EXISTS " + table.TableGen.FullProcDELToScript + @"
DROP TABLE IF EXISTS " + table.TableGen.FullTableNameToScript;
                }
                else // PGSQL 
                {
                    ScriptDrop = "DROP VIEW IF EXISTS " + table.TableGen.FullViewNameToScript + @";
DROP FUNCTION IF EXISTS " + table.TableGen.FullProcINSToScript + @";
DROP FUNCTION IF EXISTS " + table.TableGen.FullProcUPDToScript + @";
DROP FUNCTION IF EXISTS " + table.TableGen.FullProcDELToScript + @";
DROP TABLE IF EXISTS " + table.TableGen.FullTableNameToScript + @";";
                }
            }

            if (table.TableOrig.SchemaNameToScript != table.TableGen.SchemaNameToScript)
            {
                // изменилась схема, создаем все заново
                table.ScriptType = Utilities.ScriptType.CREATE;
            }

            if (
                (table.ScriptType == Utilities.ScriptType.ALTER) && 
                (table.TableOrig.TableNameToScript != table.TableGen.TableNameToScript))
            {
                // изменилось имя таблицы, переименовываем
                ScriptRenameTable = RenameTableToScript(table.TableOrig, table.TableGen);
            }

            // заполним список таблиц в EvnClass
            if (
                (Connect != null) &&
                Connect.isConnected
                )
            {
                table.ListEvnClass = Connect.GetListEvnChilds("", true, false);
            }

            // список дочерних таблиц Evn, которые созданы как наследники текущей таблицы
            List<string> ListEvnInheritChilds = null;

            if (
                (table.TableType ==  Utilities.TableType.EVN) &&
                (Connect != null) &&
                Connect.isConnected
            )
            {
                ListEvnInheritChilds = Connect.GetListEvnChilds(table.TableGen.TableNameReady, true, true);
            }

            // Дополним комментарии полей по данным из родительской таблицы (улучшение, только для целевой ПГ)
            if (
                (!table.isOnlyExist) &&
                (table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                (table.TableType == Utilities.TableType.EVN) &&
                (!string.IsNullOrWhiteSpace(table.TableGen.ParentEvnTable)) &&
                table.TableGen.HasInherit &&
                (Connect != null) &&
                Connect.isConnected
            )
            {
                foreach (var item in Connect.FillListEvnParentFields(table.TableGen.FullTableNameToSeek, table.TableGen.FullParentEvnTableToSeek, table.TableGen.HasInherit)
                    .Where(x =>
                        (
                            (table.ScriptType == Utilities.ScriptType.CREATE) ||
                            x.isUpdatedFromParent
                        ) && 
                        (!string.IsNullOrWhiteSpace(x.Desc))
                    )
                )
                {
                    // находим поле
                    var field = table.TableGen.ListField
                        .Where(x => x.FieldNameCompare == item.Name.ToLower())
                        .FirstOrDefault();

                    // описание отсутствует
                    if (
                        field != null &&
                        string.IsNullOrWhiteSpace(field.FieldDesc)
                    )
                    {
                        // дозаполним комментарий поля
                        field.FieldDesc = item.Desc;
                    }
                    /*
                    ScriptFieldDesc += Environment.NewLine + "COMMENT ON COLUMN " + table.TableEdit.FullTableNameToScript + "." + item.Name.ToLower() + " IS '" + item.Desc + "';";
                    */
                }
            }

            // Дополним список FK по данным от родительских таблиц EvnXXX (улучшение, только для целевой ПГ)
            if (
                (!table.isOnlyExist) &&
                (table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                (table.TableType ==  Utilities.TableType.EVN) &&
                (!string.IsNullOrWhiteSpace(table.TableGen.ParentEvnTable)) &&
                table.TableGen.HasInherit &&
                (Connect != null) &&
                Connect.isConnected
            )
            {
                var ListEvnParentFKs = Connect.FillListEvnParentFKs(table.isOnlyExist, table.TableGen.FullTableNameToSeek, table.TableGen.FullParentEvnTableToSeek, table.ScriptType);

                if (ListEvnParentFKs != null) //-V3022
                {
                    foreach (var item in ListEvnParentFKs
                        .Where(x => 
                            !string.IsNullOrWhiteSpace(x.FieldName) &&
                            !string.IsNullOrWhiteSpace(x.FKName) &&
                            !string.IsNullOrWhiteSpace(x.FKTable) &&
                            !string.IsNullOrWhiteSpace(x.FKField)
                        )
                    )
                    {
                        // находим поле
                        var field = table.TableGen.ListField
                            .Where(x => x.FieldNameCompare == item.FieldName.ToLower())
                            .FirstOrDefault();

                        // констрейн отсутствует
                        if (
                            field != null &&
                            string.IsNullOrWhiteSpace(field.FKName) &&
                            string.IsNullOrWhiteSpace(field.FKTable) &&
                            string.IsNullOrWhiteSpace(field.FKField)
                        )
                        {
                            field.FKName = item.FKName;
                            field.FKTable = item.FKTable;
                            field.FKField = item.FKField;
                            field.FKOrder = item.FKOrder;
                        }
                    }
                }
            }

            // Дополним список CHECK по данным от родительских таблиц EvnXXX (улучшение, только для целевой ПГ)
            if (
                (!table.isOnlyExist) &&
                (table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                (table.TableType ==  Utilities.TableType.EVN) &&
                (!string.IsNullOrWhiteSpace(table.TableGen.ParentEvnTable)) &&
                table.TableGen.HasInherit &&
                (Connect != null) &&
                Connect.isConnected
            )
            {
                var ListEvnParentCHECKs = Connect.FillListEvnParentCHECKs(table.isOnlyExist, table.TableGen.FullTableNameToSeek, table.TableGen.FullParentEvnTableToSeek, table.ScriptType);

                if (ListEvnParentCHECKs != null) //-V3022
                {
                    foreach (var item in ListEvnParentCHECKs
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.FieldName) &&
                            !string.IsNullOrWhiteSpace(x.FKName) &&
                            !string.IsNullOrWhiteSpace(x.FieldCheck)
                        )
                    )
                    {
                        // находим поле
                        var field = table.TableGen.ListField
                            .Where(x => x.FieldNameCompare == item.FieldName.ToLower())
                            .FirstOrDefault();

                        // констрейн отсутствует
                        if (
                            field != null &&
                            string.IsNullOrWhiteSpace(field.FKName) &&
                            string.IsNullOrWhiteSpace(field.FieldCheck)
                        )
                        {
                            field.FKName = item.FKName;
                            field.FieldCheck = item.FieldCheck;
                        }
                    }
                }
            }

            // список FK таблицы (было)
            List<TableFKInfo> ListOrigFKs = table.TableOrig.ListFKs;

            // список FK таблицы (стало)
            List<TableFKInfo> ListGenFKs = table.TableGen.ListFKs;

            // при генерации CREATE-скрипта для MS для Evn-таблиц - проверяем наличие и добавляем поля-идентификаторы текущей и родительской таблиц
            if (
                (table.TargetDB == Utilities.TargetDBType.MSSQL) &&
                (table.TableType ==  Utilities.TableType.EVN) &&
                (!string.IsNullOrWhiteSpace(table.TableGen.ParentEvnTable)) &&
                (table.ScriptType == Utilities.ScriptType.CREATE)
            )
            {
                string _desc = "";

                var row = table.TableGen.ListField
                    .FirstOrDefault(x => x.FieldNameCompare == (table.TableGen.TableNameCompare + "_id"));

                if (row == null)
                {
                    if (string.IsNullOrWhiteSpace(table.TableGen.TableDesc))
                    {
                        _desc = "Уникальный идентификатор";
                    }
                    else
                    {
                        _desc = "Идентификатор " + table.TableGen.TableDesc;
                    }

                    table.TableGen.AddField("-100002", table.TableGen.TableNameReady + "_id", "BIGINT", "", "", _desc, "true", "false", "true");
                }

                row = table.TableGen.ListField
                    .FirstOrDefault(x => 
                        x.FieldNameCompare == (table.TableGen.ParentEvnTableCompare + "_id")
                    );

                if (row == null)
                {
                    if (
                        (Connect != null) &&
                        Connect.isConnected
                        )
                    {
                        _desc = Connect.GetTableDecription(table.TableGen.FullParentEvnTableToSeek);
                    }

                    if (string.IsNullOrWhiteSpace(_desc))
                    {
                        _desc = "Идентификатор родительской таблицы";
                    }
                    else
                    {
                        _desc = "Идентификатор " + _desc;
                    }

                    table.TableGen.AddField("-100001", table.TableGen.ParentEvnTableReady + "_id", "BIGINT", "", "", _desc, "false", "false", "false", "","",
                        "fk_" + table.TableGen.TableNameReady + "_" + table.TableGen.ParentEvnTableReady + "_id",
                        table.TableGen.FullParentEvnTableReady,
                        table.TableGen.ParentEvnTableReady + "_id");
                }
            }

            // при генерации CREATE-скрипта для ПГ для Evn-таблиц - проверяем наличие и добавляем evn_id
            if (
                (table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                (table.TableType == Utilities.TableType.EVN) &&
                (!string.IsNullOrWhiteSpace(table.TableGen.ParentEvnTable)) &&
                (!table.TableGen.HasInherit) &&
                (!GITProjects.GetisEvnInheritByProject(table.GITProject)) &&
                (table.ScriptType == Utilities.ScriptType.CREATE)
            )
            {
                var row = table.TableGen.ListField.FirstOrDefault(x => x.FieldNameCompare == "evn_id");

                if (row == null)
                {
                    table.TableGen.AddField("-100003", "evn_id", "BIGINT", "", "", "Идентификатор события", "true", "false", "true");
                }
            }

            /*
            // триггер evncache (при созданиие таблицы на целевой ПГ)
            if (
                (table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                (table.TableType ==  Utilities.TableType.EVN) &&
                (table.ScriptType == Utilities.ScriptType.CREATE)                
            )
            {
                ScriptTrigger += Environment.NewLine +
                                $"DROP TRIGGER IF EXISTS trigger_{table.TableGen.TableNameToScript}intoevncache ON {table.TableGen.FullTableNameToScript};" + Environment.NewLine +
                                $"CREATE TRIGGER trigger_{table.TableGen.TableNameToScript}intoevncache" + Environment.NewLine +
                                $"\tAFTER INSERT OR DELETE OR UPDATE ON {table.TableGen.FullTableNameToScript}" + Environment.NewLine +
                                $"\tFOR EACH ROW EXECUTE PROCEDURE trigger_evnintoevncache();" + Environment.NewLine;
            }

            // триггер evncache (при создании дочерних таблиц для dbo.EvnDirection, dbo.EvnUsluga, dbo.EvnPrescr на целевой ПГ)
            if (
                (table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                (table.TableType ==  Utilities.TableType.EVN) &&
                (table.ScriptType == Utilities.ScriptType.CREATE) &&
                (Connect != null) &&
                Connect.isConnected
            )
            {
                var childs = Connect.GetListEvnChilds("EvnDirection", true, true);

                if (
                    (childs != null) &&
                    childs.Contains(table.TableGen.TableNameCompare, StringComparer.OrdinalIgnoreCase)
                )
                {
                    ScriptTrigger += Environment.NewLine +
                                    $"DROP TRIGGER IF EXISTS trigger_{table.TableGen.TableNameToScript}intoevndirectioncache ON {table.TableGen.FullTableNameToScript};" + Environment.NewLine +
                                    $"CREATE TRIGGER trigger_{table.TableGen.TableNameToScript}intoevndirectioncache" + Environment.NewLine +
                                    $"\tAFTER INSERT OR DELETE OR UPDATE ON {table.TableGen.FullTableNameToScript}" + Environment.NewLine +
                                    $"\tFOR EACH ROW EXECUTE PROCEDURE trigger_evnintoevndirectioncache();" + Environment.NewLine;
                }
                else
                {
                    childs = Connect.GetListEvnChilds("EvnUsluga", true, true);

                    if (
                        (childs != null) &&
                        childs.Contains(table.TableGen.TableNameCompare, StringComparer.OrdinalIgnoreCase)
                    )
                    {
                        ScriptTrigger += Environment.NewLine +
                                        $"DROP TRIGGER IF EXISTS trigger_{table.TableGen.TableNameToScript}intoevnuslugacache ON {table.TableGen.FullTableNameToScript};" + Environment.NewLine +
                                        $"CREATE TRIGGER trigger_{table.TableGen.TableNameToScript}intoevnuslugacache" + Environment.NewLine +
                                        $"\tAFTER INSERT OR DELETE OR UPDATE ON {table.TableGen.FullTableNameToScript}" + Environment.NewLine +
                                        $"\tFOR EACH ROW EXECUTE PROCEDURE trigger_evnintoevnuslugacache();" + Environment.NewLine;
                    }
                    else
                    {
                        childs = Connect.GetListEvnChilds("EvnPrescr", true, true);

                        if (
                            (childs != null) &&
                            childs.Contains(table.TableGen.TableNameCompare, StringComparer.OrdinalIgnoreCase)
                        )
                        {
                            ScriptTrigger += Environment.NewLine +
                                            $"DROP TRIGGER IF EXISTS trigger_{table.TableGen.TableNameToScript}intoevnprescrcache ON {table.TableGen.FullTableNameToScript};" + Environment.NewLine +
                                            $"CREATE TRIGGER trigger_{table.TableGen.TableNameToScript}intoevnprescrcache" + Environment.NewLine +
                                            $"\tAFTER INSERT OR DELETE OR UPDATE ON {table.TableGen.FullTableNameToScript}" + Environment.NewLine +
                                            $"\tFOR EACH ROW EXECUTE PROCEDURE trigger_evnintoevnprescrcache();" + Environment.NewLine;
                        }
                    }
                }
            }
            */

            /*
            // Добавить ограничение по EvnClass_id на новую таблицу и родительские за исключением dbo.Evn
            if (
                (table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                (table.TableType ==  Utilities.TableType.EVN) &&
                (table.ScriptType == Utilities.ScriptType.CREATE) &&
                (Connect != null) &&
                Connect.isConnected
                )
            {
                ScriptEvnClassCheck += Connect.GetEvnClassCheck(table.TableGen.TableNameReady);
            }
            */

            // перебираем поля оригинальной таблицы, ищем удаленные поля
            if (table.ScriptType == Utilities.ScriptType.ALTER)
            {
                foreach (FieldDB oldRow in table.TableOrig
                    .ListFilteredField(true, false)
                    .Where(x => x.FieldName != "")
                    .OrderBy(x => x.FieldOrder)
                )
                {
                    FieldDB row = table.TableGen.FindFieldByName(oldRow.FieldNameCompare, true, false);

                    if (row == null)
                    {
                        /* // удаляем PK
                         if (oldRow.IsPK == true)
                         {
                             if (ScriptDropField_before != "") ScriptDropField_before = ScriptDropField_before + Environment.NewLine; else ScriptDropField_before = ScriptDropField_before + Environment.NewLine + Environment.NewLine;
                             ScriptDropField_before = ScriptDropField_before + this.TableOrig.DropPKToScript();
                         }*/

                        // удаляем констрейн
                        if (!string.IsNullOrWhiteSpace(oldRow.FKFullTableToScript))
                        {
                            if (!string.IsNullOrWhiteSpace(ScriptDropFK))
                            {
                                ScriptDropFK = ScriptDropFK + Environment.NewLine;
                            }
                            else
                            {
                                ScriptDropFK = ScriptDropFK + Environment.NewLine + Environment.NewLine;
                            }

                            ScriptDropFK = ScriptDropFK + TableInfo.DropFKToScript(oldRow, ListOrigFKs);
                        }
                    }
                }
            }

            // перебираем поля обновленной таблицы
            foreach (FieldDB row in table.TableGen.ListField
                .Where(x =>
                    (!string.IsNullOrWhiteSpace(x.FieldName)) &&
                    (!string.IsNullOrWhiteSpace(x.FieldType)) &&
                    x.IsUsed 
                )
                .OrderBy(x => x.FieldOrder)
            )
            {
                if (
                    (table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                    (table.TableType ==  Utilities.TableType.EVN) &&
                    (!string.IsNullOrWhiteSpace(table.TableGen.ParentEvnTable)) &&
                    (row.FieldNameCompare != "evn_id") &&
                    (
                        (row.FieldNameCompare == table.TableGen.TableNameCompare + "_id") ||
                        (row.FieldNameCompare == table.TableGen.ParentEvnTableCompare + "_id")
                    )
                )
                {
                    // только для EvnXXX на ПГ
                    // пропускаем поле-идентификатор текущей таблицы и поле-идентификатор родительской таблицы, если это не evn_id
                    continue;
                }

                if (
                    (table.TargetDB == Utilities.TargetDBType.MSSQL) &&
                    (table.TableType == Utilities.TableType.EVN) &&
                    (!string.IsNullOrWhiteSpace(table.TableGen.ParentEvnTable)) &&
                    (table.TableGen.FullTableNameCompare != "dbo.evn") &&
                    (row.FieldNameCompare == "evn_id")
                )
                {
                    // только для EvnXXX на МС
                    // пропускаем evn_id, если это не dbo.Evn
                    continue;
                }

                // находим поле в оригинальной таблице
                FieldDB oldRow = table.TableOrig.FindFieldByName(row.FieldNameCompare, true, null);

                // field ALTER
                if (
                    !row.IsInherit &&
                    (table.ScriptType == Utilities.ScriptType.ALTER)
                )
                {
                    if (oldRow != null)
                    {
                        if (oldRow.FieldNotEquals(row)) // что-то изменилось
                        {
                            if (oldRow.FieldNameToScript != row.FieldNameToScript)
                            {
                                // переименование поля
                                ScriptRenameField += Environment.NewLine + 
                                    RenameFieldToScript(oldRow, row);

                                RowInfo += Environment.NewLine + 
                                    $"{oldRow.FieldName} переименован в {row.FieldName} {row.FullFieldTypeToScript} - {row.FieldDesc}";

                                if (!string.IsNullOrWhiteSpace(row.FKTable))
                                {
                                    RowInfo += " (" + row.FKTable + ")";
                                }
                            }

                            // изменение ревизитов поля
                            ScriptRow += Environment.NewLine +
                                TableInfo.AlterFieldToScript(oldRow, row, ref ScriptAlterList, ref ScriptColumnList);

                            RowInfo += Environment.NewLine + 
                                $"Изменен {row.FieldName} {row.FullFieldTypeToScript} - {row.FieldDesc}";

                            if (!string.IsNullOrWhiteSpace(row.FKTable))
                            {
                                RowInfo += " (" + row.FKTable + ")";
                            }
                        }
                    }
                    else
                    {
                        // добавление нового поля
                        ScriptRow += Environment.NewLine +
                            TableInfo.AddFieldToScript(row);

                        RowInfo += Environment.NewLine + $"Добавлен {row.FieldName} {row.FullFieldTypeToScript} - {row.FieldDesc}";

                        if (!string.IsNullOrWhiteSpace(row.FKTable))
                        {
                            RowInfo += " (" + row.FKTable + ")";
                        }
                    }
                }

                // field CREATE
                if (
                    !row.IsInherit &&
                    (table.ScriptType == Utilities.ScriptType.CREATE)
                )
                {
                    // список полей для создания таблицы
                    if (!string.IsNullOrWhiteSpace(ScriptRow))
                    {
                        ScriptRow = ScriptRow + "," + Environment.NewLine;
                    }
                    else
                    {
                        ScriptRow = ScriptRow + Environment.NewLine;
                    }

                    ScriptRow = ScriptRow + "\t" + TableInfo.AddFieldToCreateScript(row, ref ScriptCreateSetNotNull);
                }

                /*
                // ВСЕГДА ПОСЛЕ field ------------------------------------------------------------
                if (
                    (table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                    (table.TableType == Utilities.TableType.EVN) &&
                    table.TableGen.HasInherit &&
                    (!string.IsNullOrWhiteSpace(table.TableGen.ParentEvnTable)) &&
                    (row.FieldName.ToLower() == "evn_id")
                )
                {
                    // только для наследника EvnXXX на PG
                    // убираем у evn_id признак "наследуется от родительской таблицы",
                    // чтобы в дальнейшем оно включалось в скрипты
                    row.IsInherit_string = "false";
                }
                //--------------------------------------------------------------------------------
                */

                // foreign key or check
                if (!string.IsNullOrWhiteSpace(row.FKTable))
                {
                    if (string.IsNullOrWhiteSpace(row.FKName))
                    {
                        row.FKName = table.TableGen.GetFKNameDefault(row.FieldNameReady);
                    }

                    if (string.IsNullOrWhiteSpace(row.FKField))
                    {
                        row.FKField = Utilities.Databases.GetTableName(row.FKTableNameReady) + "_id";
                    }
                }

                if ((oldRow != null) && (table.ScriptType == Utilities.ScriptType.ALTER))
                {
                    if (oldRow.FKNotEquals(row))
                    {
                        if (!string.IsNullOrWhiteSpace(ScriptDropFK))
                        {
                            ScriptDropFK = ScriptDropFK + Environment.NewLine;
                        }
                        else
                        {
                            ScriptDropFK = Environment.NewLine;
                        }

                        ScriptDropFK = ScriptDropFK + TableInfo.DropFKToScript(oldRow, ListOrigFKs);

                        string _script = TableInfo.AddFKToScript(row, ListGenFKs);

                        if (!string.IsNullOrWhiteSpace(_script))
                        {
                            if (!string.IsNullOrWhiteSpace(ScriptRowFK))
                            {
                                ScriptRowFK = ScriptRowFK + Environment.NewLine;
                            }
                            else
                            {
                                ScriptRowFK = Environment.NewLine;
                            }

                            ScriptRowFK = ScriptRowFK + _script;
                        }
                    }
                }
                else
                {
                    string _script = TableInfo.AddFKToScript(row, ListGenFKs);

                    if (!string.IsNullOrWhiteSpace(_script))
                    {
                        if (!string.IsNullOrWhiteSpace(ScriptRowFK))
                        {
                            ScriptRowFK = ScriptRowFK + Environment.NewLine;
                        }
                        else
                        {
                            ScriptRowFK = Environment.NewLine;
                        }

                        ScriptRowFK = ScriptRowFK + _script;
                    }
                }

                // index for foreign key
                if (
                    (oldRow != null) && 
                    (table.ScriptType == Utilities.ScriptType.ALTER)
                )
                {
                    string old_script = TableInfo.AddIndexFKToScript(oldRow, ListOrigFKs);
                    string new_script = TableInfo.AddIndexFKToScript(row, ListGenFKs);

                    if (old_script != new_script)
                    {
                        /* убираем для ALTER COLUMN генерацию индексов для foreign key
                        if (!string.IsNullOrWhiteSpace(ScriptFK)) ScriptFK = ScriptFK + Environment.NewLine; else ScriptFK = Environment.NewLine;
                        ScriptFK = ScriptFK + TableOrig.DropIndexFKToScript(oldRow);

                        if (TableGen.AddIndexFKToScript(row) != "")
                        {
                            if (!string.IsNullOrWhiteSpace(ScriptFK)) ScriptFK = ScriptFK + Environment.NewLine; else ScriptFK = Environment.NewLine;
                            ScriptFK = ScriptFK + TableGen.AddIndexFKToScript(row);
                        }
                        */
                    }
                }
                else
                {
                    string _script = TableInfo.AddIndexFKToScript(row, ListGenFKs);

                    if (!string.IsNullOrWhiteSpace(_script))
                    {
                        if (!string.IsNullOrWhiteSpace(ScriptRowFK))
                        {
                            ScriptRowFK = ScriptRowFK + Environment.NewLine;
                        }
                        else
                        {
                            ScriptRowFK = Environment.NewLine;
                        }

                        ScriptRowFK = ScriptRowFK + _script;
                    }
                }

                // description SWAN_RegionalTable
                if (
                    (oldRow != null) && 
                    (table.ScriptType == Utilities.ScriptType.ALTER)
                )
                {
                    if (
                        (!table.TableOrig.HasRegionDescr) ||
                        (TableInfo.AddSWAN_RegionalTableScript(oldRow) != TableInfo.AddSWAN_RegionalTableScript(row))
                    )
                    {
                        if (TableInfo.AddSWAN_RegionalTableScript(row) != "")
                        {
                            if (!string.IsNullOrWhiteSpace(ScriptFieldDesc))
                            {
                                ScriptFieldDesc = ScriptFieldDesc + Environment.NewLine;
                            }
                            else
                            {
                                ScriptFieldDesc = Environment.NewLine;
                            }

                            ScriptFieldDesc = ScriptFieldDesc + TableInfo.AddSWAN_RegionalTableScript(row);
                        }
                    }
                }
                else if (TableInfo.AddSWAN_RegionalTableScript(row) != "")
                {
                    if (!string.IsNullOrWhiteSpace(ScriptFieldDesc))
                    {
                        ScriptFieldDesc = ScriptFieldDesc + Environment.NewLine;
                    }
                    else
                    {
                        ScriptFieldDesc = Environment.NewLine;
                    }

                    ScriptFieldDesc = ScriptFieldDesc + TableInfo.AddSWAN_RegionalTableScript(row);
                }

                // field description
                if (
                    (oldRow != null) && 
                    (table.ScriptType == Utilities.ScriptType.ALTER)
                )
                {
                    if (oldRow.FieldDesc != row.FieldDesc)
                    {
                        if (!string.IsNullOrWhiteSpace(oldRow.FieldDesc))
                        {
                            ScriptFieldDesc += Environment.NewLine + TableInfo.ChangeFieldDescToScript(ListEvnInheritChilds, row);
                        }
                        else
                        {
                            ScriptFieldDesc += Environment.NewLine + TableInfo.AddFieldDescToScript(ListEvnInheritChilds, row);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(row.FieldDesc))
                    {
                        ScriptFieldDesc += Environment.NewLine + TableInfo.AddFieldDescToScript(ListEvnInheritChilds, row);
                    }
                }

                /*
                // ВСЕГДА В КОНЦЕ ----------------------------------------------------------------
                if (
                    (table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                    (table.TableType == Utilities.TableType.EVN) &&
                    table.TableGen.HasInherit &&
                    (!string.IsNullOrWhiteSpace(table.TableGen.ParentEvnTable)) &&
                    (row.FieldName.ToLower() == "evn_id")
                )
                {
                    // только для наследника EvnXXX на PG
                    // возвращаем evn_id признак "наследуется от родительской таблицы",
                    row.IsInherit_string = "true";
                }
                //--------------------------------------------------------------------------------
                */
            }

            // при генерации могли быть добавлены поля, надо их удалить
            FieldDB found = null;
            do
            {
                found = table.TableGen.ListField
                    .Where(x =>
                        x.FieldOrder < -100000 // добавлены принудительно
                    )
                    .FirstOrDefault();

                if (found != null)
                {
                    table.TableGen.ListField.Remove(found);
                }
                else
                {
                    break;
                }
            } while (true);

            // таблица и поля
            if (table.ScriptType == Utilities.ScriptType.CREATE)
            {
                ScriptCreateTable = TableInfo.CreateTableToScript(ScriptRow, table.TableGen);
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(ScriptAlterList))
                {
                    switch (table.TargetDB)
                    {
                        case TargetDBType.MSSQL:
                            {
                                ScriptRow =
                                    "EXEC dbo.p_AlterTypeColumn" + Environment.NewLine +
                                    "\t@Task = '" + MainWindow.Task.TaskNumber + "'," + Environment.NewLine +
                                    "\t@SchemaName = '" + table.TableGen.SchemaNameToScript + "'," + Environment.NewLine +
                                    "\t@TableName = '" + table.TableGen.TableNameToScript + "'," + Environment.NewLine +
                                    "\t@ColumnName = '" + Utilities.Strings.ListToString(ScriptColumnList, ", ") + "'," + Environment.NewLine +
                                    "\t@cmd = '" + Environment.NewLine +
                                    ScriptAlterList.TrimNewLine().Replace("'", "''") + Environment.NewLine +
                                    "'" + Environment.NewLine +
                                    ScriptRow;
                            }
                            break;
                        case TargetDBType.PGSQL:
                        case TargetDBType.EMD:
                            {
                                string _prefix = "SELECT * FROM";

                                if (isAddRegion) _prefix = "PERFORM";

                                ScriptRow =
                                _prefix + " dbo.xp_gen_view('" + table.TableGen.SchemaNameToScript + "." + table.TableGen.TableNameToScript + "'," + Environment.NewLine +
                                "$altertext$" + Environment.NewLine +
                                ScriptAlterList.TrimNewLine() + Environment.NewLine +
                                "$altertext$" + Environment.NewLine +
                                ", 2);" + Environment.NewLine +
                                ScriptRow;
                            }
                            break;
                        default:
                            break;
                    }
                }

                ScriptAlterTable = ScriptAlterTable.TrimNewLine();

                ScriptRow = ScriptRow.TrimNewLine();

                ScriptAlterTable =
                    (string.IsNullOrWhiteSpace(ScriptAlterTable) ? "" : Environment.NewLine + Environment.NewLine + ScriptAlterTable) +
                    (string.IsNullOrWhiteSpace(ScriptRow) ? "" : Environment.NewLine + Environment.NewLine + ScriptRow);
            }

            // перебираем поля оригинальной таблицы, ищем удаленные поля
            if (table.ScriptType == Utilities.ScriptType.ALTER)
            {
                foreach (FieldDB oldRow in table.TableOrig
                    .ListFilteredField(true, false)
                    .Where(x => x.FieldName != "")
                    .OrderBy(x => x.FieldOrder)
                )
                {
                    FieldDB row = table.TableGen.FindFieldByName(oldRow.FieldNameCompare, true, false);

                    if (row == null)
                    {
                        // удаляем поле
                        if (!string.IsNullOrWhiteSpace(ScriptDropField))
                        {
                            ScriptDropField = ScriptDropField + Environment.NewLine;
                        }
                        else
                        {
                            ScriptDropField = ScriptDropField + Environment.NewLine + Environment.NewLine;
                        }

                        ScriptDropField = ScriptDropField + TableInfo.DropFieldToScript(oldRow);

                        RowInfo += Environment.NewLine + $"Удален {oldRow.FieldName}";
                    }
                }
            }

            // Primary Key
            if (
                (TableInfo.AddPKToScript(table.TableOrig) != TableInfo.AddPKToScript(table.TableGen)) || 
                (table.ScriptType == Utilities.ScriptType.CREATE)
            )
            {
                if (
                    (!string.IsNullOrWhiteSpace(table.TableOrig.PKNameToScript)) && 
                    (table.ScriptType == Utilities.ScriptType.ALTER)
                )
                {
                    ScriptCreatePK = TableInfo.DropPKToScript(table.TableOrig);
                }

                if (!string.IsNullOrWhiteSpace(TableInfo.AddPKToScript(table.TableGen)))
                {
                    ScriptCreatePK += Environment.NewLine + Environment.NewLine + TableInfo.AddPKToScript(table.TableGen);
                }
            }

            // Table description
            if (
                (table.TableOrig.TableDesc != table.TableGen.TableDesc) || 
                (table.ScriptType == Utilities.ScriptType.CREATE)
            )
            {
                if (
                    (!string.IsNullOrWhiteSpace(table.TableOrig.TableDesc)) && 
                    (table.ScriptType == Utilities.ScriptType.ALTER)
                )
                {
                    ScriptTableDesc = TableInfo.ChangeTableDescToScript(table.TableGen);
                }
                else
                {
                    ScriptTableDesc = TableInfo.AddTableDescToScript(table.TableGen);
                }
            }

            // Table sequence
            if (
                (TableInfo.AddSequienceToScript(table.TableGen) != "") && 
                (table.ScriptType == Utilities.ScriptType.CREATE)
            )
            {
                ScriptSequence = TableInfo.AddSequienceToScript(table.TableGen);
            }

            // Пересоздание хранимок
            if (
                isAddProc && 
                (table.TargetDB == Utilities.TargetDBType.MSSQL) && 
                (table.TableType != Utilities.TableType.TEMP)
            )
            {
                string proc = "";
                string proc_dop = "";

                switch (table.TableType)
                {
                    case Utilities.TableType.EVN:
                        proc = $"EXEC dbo.xp_GenScriptList @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @OutMode = 2, @Separator = '-- SQLGen --'";
                        ProcCommand.Add(proc);
                        ProcCommandNum = ProcCommand.Count - 1;

                        proc_dop = $"EXEC dbo.xp_GenScriptEvn_ins @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL, @FieldsExclude = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptEvn_upd @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL, @FieldsExclude = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptEvn_set @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL, @FieldsExclude = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptEvn_del @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptEvn_setdel @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptEvn_setdelafter @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptEvn_view @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL, @FieldsExclude = NULL";
                        ProcCommand.Add(proc_dop);

                        ScriptProc += Environment.NewLine + "--" + proc;
                        break;
                    case Utilities.TableType.PERSONEVN:
                        proc = $"EXEC dbo.xp_GenScriptList @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @OutMode = 2, @Separator = '-- SQLGen --'";
                        ProcCommand.Add(proc);
                        ProcCommandNum = ProcCommand.Count - 1;

                        proc_dop = $"EXEC dbo.xp_GenScriptPersonEvn_ins @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptPersonEvn_upd @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptPersonEvn_del @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptPersonEvn_view @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        ScriptProc += Environment.NewLine + "--" + proc;
                        break;
                    case Utilities.TableType.MORBUS:
                        proc = $"EXEC dbo.xp_GenScriptList @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @OutMode = 2, @Separator = '-- SQLGen --'";
                        ProcCommand.Add(proc);
                        ProcCommandNum = ProcCommand.Count - 1;

                        proc_dop = $"EXEC dbo.xp_GenScriptDict_ins @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptDict_upd @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptMorbus_del @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptMorbus_view @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        ProcCommand.Add(proc_dop);

                        ScriptProc += Environment.NewLine + "--" + proc;
                        break;
                    case Utilities.TableType.DICT:
                        proc = $"EXEC dbo.xp_GenScriptList @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @OutMode = 2, @Separator = '-- SQLGen --'";
                        if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion)) proc += ", @Region = '" + txtRegion + "'";
                        ProcCommand.Add(proc);
                        ProcCommandNum = ProcCommand.Count - 1;

                        proc_dop = $"EXEC dbo.xp_GenScriptDict_ins @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL, @FieldsExclude = NULL";
                        if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion)) proc_dop += ", @Region = '" + txtRegion + "'";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptDict_upd @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL, @FieldsExclude = NULL";
                        if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion)) proc_dop += ", @Region = '" + txtRegion + "'";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptDict_del @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL";
                        if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion)) proc_dop += ", @Region = '" + txtRegion + "'";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"EXEC dbo.xp_GenScriptDict_view @TableName = '{table.TableGen.FullTableNameToScript}', @TaskNumber = '{MainWindow.Task.TaskNumber}', @isExec = NULL, @isRebuild = NULL, @FieldsExclude = NULL";
                        if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion)) proc_dop += ", @Region = '" + txtRegion + "'";
                        ProcCommand.Add(proc_dop);

                        ScriptProc += Environment.NewLine + "--" + proc;
                        break;
                    default:
                        break;
                }

                ScriptProc += Environment.NewLine;
            }

            // Пересоздание хранимок
            if (
                isAddProc && 
                (
                    (table.TargetDB == Utilities.TargetDBType.PGSQL) || 
                    (table.TargetDB == Utilities.TargetDBType.EMD)
                ) && 
                (table.TableType != Utilities.TableType.TEMP)
            )
            {
                string proc = "";
                string proc_dop = "";

                switch (table.TableType)
                {
                    case Utilities.TableType.EVN:
                        proc = $"SELECT * FROM dbo.xp_genscriptlist(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isjson := NULL::integer, outmode := 2, separator := '-- SQLGen --'";
                        proc += ");";
                        ProcCommand.Add(proc);
                        ProcCommandNum = ProcCommand.Count - 1;

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptevn_ins(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer, isjson := NULL::integer, fieldsexclude := NULL::varchar";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptevn_upd(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer, isjson := NULL::integer, fieldsexclude := NULL::varchar";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptevn_set(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer, isjson := NULL::integer, fieldsexclude := NULL::varchar";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptevn_del(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptevn_setdel(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptevn_setdelafter(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptevn_view(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer, fieldsexclude := NULL::varchar";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        ScriptProc += Environment.NewLine + "--" + proc;
                        break;
                    case Utilities.TableType.PERSONEVN:
                        proc = $"SELECT * FROM dbo.xp_genscriptlist(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, outmode := 2, separator := '-- SQLGen --'";
                        proc += ");";
                        ProcCommand.Add(proc);
                        ProcCommandNum = ProcCommand.Count - 1;

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptpersonevn_ins(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptpersonevn_upd(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptpersonevn_del(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptpersonevn_view(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        ScriptProc += Environment.NewLine + "--" + proc;
                        break;
                    case Utilities.TableType.MORBUS:
                        proc = $"SELECT * FROM dbo.xp_genscriptlist(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, outmode := 2, separator := '-- SQLGen --'";
                        proc += ");";
                        ProcCommand.Add(proc);
                        ProcCommandNum = ProcCommand.Count - 1;

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptdict_ins(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptdict_upd(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptmorbus_del(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptmorbus_view(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        ScriptProc += Environment.NewLine + "--" + proc;
                        break;
                    case Utilities.TableType.DICT:
                        proc = $"SELECT * FROM dbo.xp_genscriptlist(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isjson := NULL::integer, outmode := 2, separator := '-- SQLGen --'";
                        if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion)) proc += ", region := '" + txtRegion + "'";
                        proc += ");";
                        ProcCommand.Add(proc);
                        ProcCommandNum = ProcCommand.Count - 1;

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptdict_ins(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer, isjson := NULL::integer, ispacket := NULL::integer, fieldsexclude := NULL::varchar";
                        if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion)) proc_dop += ", region := '" + txtRegion + "'";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptdict_upd(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer, isjson := NULL::integer, ispacket := NULL::integer, fieldsexclude := NULL::varchar";
                        if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion)) proc_dop += ", region := '" + txtRegion + "'";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptdict_del(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer, ispacket := NULL::integer";
                        if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion)) proc_dop += ", region := '" + txtRegion + "'";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        proc_dop = $"SELECT * FROM dbo.xp_genscriptdict_view(tablename := '{table.TableGen.FullTableNameToScript}', tasknumber := '{MainWindow.Task.TaskNumber}', isexec := NULL::integer, isrebuild := NULL::integer, fieldsexclude := NULL::varchar";
                        if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion)) proc_dop += ", region := '" + txtRegion + "'";
                        proc_dop += ");";
                        ProcCommand.Add(proc_dop);

                        ScriptProc += Environment.NewLine + "--" + proc;
                        break;
                    default:
                        break;
                }

                ScriptProc += Environment.NewLine;
            }

            // собираем скрипт
            string TitleScript = "";

            if (isAddTitle)
            {
                TitleScript = MainWindow.Task.TitleScript(table.TargetDB, "table", false, false);
            }

            ScriptRegionBegin = ScriptRegionBegin.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptRegionBegin))
            {
                ScriptRegionBegin = Environment.NewLine + Environment.NewLine + ScriptRegionBegin;
            }

            ScriptRegionEnd = ScriptRegionEnd.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptRegionEnd))
            {
                ScriptRegionEnd = Environment.NewLine + Environment.NewLine + ScriptRegionEnd;
            }

            ScriptDropFK = ScriptDropFK.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptDropFK))
            {
                ScriptDropFK = Environment.NewLine + Environment.NewLine +
                    ScriptDropFK;
            }

            ScriptDrop = ScriptDrop.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptDrop))
            {
                ScriptDrop = Environment.NewLine + Environment.NewLine +
                    "Drop objects (WARNING!!! ACHTUNG!!! ВНИМАНИЕ!!!)" + Environment.NewLine +
                    ScriptDrop;
            }

            ScriptRenameTable = ScriptRenameTable.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptRenameTable))
            {
                ScriptRenameTable = Environment.NewLine + Environment.NewLine +
                    "-- Rename table" + Environment.NewLine +
                    ScriptRenameTable;
            }

            ScriptRenameField = ScriptRenameField.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptRenameField))
            {
                ScriptRenameField = Environment.NewLine + Environment.NewLine +
                    "-- Rename columns" + Environment.NewLine +
                    ScriptRenameField;
            }

            ScriptCreateTable = ScriptCreateTable.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptCreateTable))
            {
                ScriptCreateTable = Environment.NewLine + Environment.NewLine +
                    ScriptCreateTable;
            }

            ScriptCreatePK = ScriptCreatePK.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptCreatePK))
            {
                ScriptCreatePK = Environment.NewLine + Environment.NewLine +
                    ScriptCreatePK;
            }

            ScriptCreateSetNotNull = ScriptCreateSetNotNull.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptCreateSetNotNull))
            {
                ScriptCreateSetNotNull = Environment.NewLine + Environment.NewLine + 
                    "-- Set not null" + Environment.NewLine +
                    ScriptCreateSetNotNull;
            }

            ScriptAlterTable = ScriptAlterTable.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptAlterTable))
            {
                ScriptAlterTable = Environment.NewLine + Environment.NewLine +
                    ScriptAlterTable;
            }

            ScriptParentFK = ScriptParentFK.TrimNewLine();

            ScriptRowFK = ScriptRowFK.TrimNewLine();

            string ScriptFK = "";

            if (
                !string.IsNullOrWhiteSpace(ScriptParentFK) || 
                !string.IsNullOrWhiteSpace(ScriptRowFK)
            )
            {
                ScriptFK = Environment.NewLine + Environment.NewLine +
                    "-- Add FK" +
                    (string.IsNullOrWhiteSpace(ScriptParentFK) ? "" : Environment.NewLine + ScriptParentFK + Environment.NewLine) +
                    (string.IsNullOrWhiteSpace(ScriptRowFK) ? "" : Environment.NewLine + ScriptRowFK);
            }

            ScriptTableDesc = ScriptTableDesc.TrimNewLine();

            ScriptFieldDesc = ScriptFieldDesc.TrimNewLine();

            string ScriptDesc = "";

            if (
                !string.IsNullOrWhiteSpace(ScriptTableDesc) || 
                !string.IsNullOrWhiteSpace(ScriptFieldDesc)
            )
            {
                ScriptDesc = Environment.NewLine + Environment.NewLine +
                    "-- Comments" +
                    (string.IsNullOrWhiteSpace(ScriptTableDesc) ? "" : Environment.NewLine + ScriptTableDesc + Environment.NewLine) +
                    (string.IsNullOrWhiteSpace(ScriptFieldDesc) ? "" : Environment.NewLine + ScriptFieldDesc);
            }

            ScriptDropField = ScriptDropField.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptDropField))
            {
                ScriptDropField = Environment.NewLine + Environment.NewLine +
                    ScriptDropField;
            }

            ScriptSequence = ScriptSequence.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptSequence))
            {
                ScriptSequence = Environment.NewLine + Environment.NewLine +
                    "-- Add sequence" + Environment.NewLine +
                    ScriptSequence;
            }

            /*ScriptTrigger = ScriptTrigger.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptTrigger))
            {
                ScriptTrigger = Environment.NewLine + Environment.NewLine +
                    "-- Recreate triggers" + Environment.NewLine +
                    ScriptTrigger;
            }*/

            /*ScriptEvnClassCheck = ScriptEvnClassCheck.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptEvnClassCheck))
            {
                ScriptEvnClassCheck = Environment.NewLine + Environment.NewLine +
                    "-- Recreate EnvClass CHECK" + Environment.NewLine +
                    ScriptEvnClassCheck;
            }*/

            ScriptProc = ScriptProc.TrimNewLine();

            if (!string.IsNullOrWhiteSpace(ScriptProc))
            {
                ScriptProc = Environment.NewLine + Environment.NewLine +
                    ScriptProc;
            }

            if (table.isAddIndex == true)
            {
                foreach (var index in table.ListIndex)
                {
                    ScriptIndex += Environment.NewLine + GenerateIndexScript(Utilities.ScriptType.CREATE, false, false, isAddRegion, txtRegion, index);
                }

                ScriptIndex = Environment.NewLine + Environment.NewLine + ScriptIndex.TrimNewLine();
            }

            string alter_script = (
                ScriptDropFK +
                ScriptRenameField +
                ScriptAlterTable +
                ScriptCreatePK +
                ScriptFK +
                ScriptDesc +
                ScriptDropField
            )
            .TrimNewLine();

            if (!string.IsNullOrWhiteSpace(alter_script))
            {
                alter_script = Environment.NewLine + Environment.NewLine +
                    ((table.TargetDB == Utilities.TargetDBType.MSSQL) ? 
                        "IF OBJECT_ID(N'" + table.TableGen.FullTableNameToScript + "', 'U') IS NOT NULL" + Environment.NewLine +
                        "BEGIN" + Environment.NewLine : "") +
                     alter_script +
                     ((table.TargetDB == Utilities.TargetDBType.MSSQL) ?
                        Environment.NewLine + "END" + Environment.NewLine : "");
            }

            table.TableGen = null;

            return (
                TitleScript +
                ScriptRegionBegin +
                ScriptDrop +
                ScriptCreateTable +
                ScriptCreateSetNotNull +
                alter_script +
                ScriptSequence +
                //ScriptTrigger +
                //ScriptEvnClassCheck +
                ScriptRegionEnd +
                ScriptProc +
                ScriptIndex
                )
                .TrimInnerNewLine()
                .TrimNewLine(Environment.NewLine);
        }

        /// <summary>
        /// Собрать скрипт для добавления записи в stg.LocalDBList
        /// </summary>
        /// <param name="table">описание таблицы</param>
        /// <returns></returns>
        public static string GenerateLocalDBList(TableDB table)
        {
            string Script = "";

            if (table.LocalDBList == null) return Script;

            // заголовок скрипта (информация о задаче)
            string TitleScript = MainWindow.Task.TitleScript(table.TargetDB, "data", false, false);
            if (TitleScript == null) TitleScript = ""; //-V3022
            if (TitleScript != "") Script = Script + Environment.NewLine;

            // хинт для MS SQL
            string mshint_change = "";
            if (table.TargetDB == Utilities.TargetDBType.MSSQL) mshint_change = "WITH (rowlock) ";
            string mshint_sel = "";
            if (table.TargetDB == Utilities.TargetDBType.MSSQL) mshint_sel = "WITH (nolock) ";

            if (table.TargetDB == Utilities.TargetDBType.MSSQL) // для MS SQL
            {
                Script = Script +
                            "SET DATEFORMAT ymd" + Environment.NewLine +
                            "IF NOT EXISTS (" + Environment.NewLine +
                            "\tSELECT TOP(1) 1" + Environment.NewLine +
                            "\tFROM stg.LocalDBList " + mshint_sel + Environment.NewLine +
                            "\tWHERE LocalDbList_module = '" + table.LocalDBList.LocalDBList_Module + "'" + Environment.NewLine +
                            "\tAND LocalDbList_name = '" + table.LocalDBList.LocalDBList_Name + "'" + Environment.NewLine +
                            "\tAND localdblist_schema = '" + table.LocalDBList.LocalDBList_Schema + "'" + Environment.NewLine +
                            ")" + Environment.NewLine +
                            "INSERT INTO stg.LocalDBList " + mshint_change + "(" + Environment.NewLine +
                            "\tlocaldblist_name," + Environment.NewLine +
                            "\tlocaldblist_prefix," + Environment.NewLine +
                            "\tlocaldblist_nick," + Environment.NewLine +
                            "\tlocaldblist_schema," + Environment.NewLine +
                            "\tlocaldblist_sql," + Environment.NewLine +
                            "\tlocaldblist_key," + Environment.NewLine +
                            "\tlocaldblist_module," + Environment.NewLine +
                            "\tlocaldblist_descr," + Environment.NewLine +
                            "\tpmuser_insid," + Environment.NewLine +
                            "\tpmuser_updid," + Environment.NewLine +
                            "\tlocaldblist_insdt," + Environment.NewLine +
                            "\tlocaldblist_upddt" + Environment.NewLine +
                            ")" + Environment.NewLine +
                            "VALUES (" + Environment.NewLine +
                            "\t'" + table.LocalDBList.LocalDBList_Name + "'," + Environment.NewLine +
                            "\t'" + table.LocalDBList.LocalDBList_Prefix + "'," + Environment.NewLine +
                            "\t'" + table.LocalDBList.LocalDBList_Nick + "'," + Environment.NewLine +
                            "\t'" + table.LocalDBList.LocalDBList_Schema + "'," + Environment.NewLine +
                            "\tNULL," + Environment.NewLine +
                            "\t'" + table.LocalDBList.LocalDBList_Key + "'," + Environment.NewLine +
                            "\t'" + table.LocalDBList.LocalDBList_Module + "'," + Environment.NewLine +
                            "\t'" + table.LocalDBList.LocalDBList_Descr + "'," + Environment.NewLine +
                            "\t1, 1, getdate(), getdate()" + Environment.NewLine +
                            ")";

                Script = Script + Environment.NewLine + Environment.NewLine +
                            "UPDATE stg.LocalDBList " + mshint_change + " SET" + Environment.NewLine +
                            "\tlocaldblist_name = '" + table.LocalDBList.LocalDBList_Name + "', " + Environment.NewLine +
                            "\tlocaldblist_prefix = '" + table.LocalDBList.LocalDBList_Prefix + "', " + Environment.NewLine +
                            "\tlocaldblist_nick = '" + table.LocalDBList.LocalDBList_Nick + "', " + Environment.NewLine +
                            "\tlocaldblist_schema = '" + table.LocalDBList.LocalDBList_Schema + "', " + Environment.NewLine +
                            "\tlocaldblist_key = '" + table.LocalDBList.LocalDBList_Key + "', " + Environment.NewLine +
                            "\tlocaldblist_module = '" + table.LocalDBList.LocalDBList_Module + "', " + Environment.NewLine +
                            "\tlocaldblist_descr = '" + table.LocalDBList.LocalDBList_Descr + "', " + Environment.NewLine +
                            "\tpmuser_updid = 1, " + Environment.NewLine +
                            "\tlocaldblist_upddt = getdate()" + Environment.NewLine +
                            "-- select * from stg.LocalDBList" + Environment.NewLine +
                            "WHERE LocalDbList_module = '" + table.LocalDBList.LocalDBList_Module + "' AND LocalDbList_name = '" + table.LocalDBList.LocalDBList_Name + "' AND localdblist_schema = '" + table.LocalDBList.LocalDBList_Schema + "'";

                if (!string.IsNullOrWhiteSpace(table.LocalDBList.RegionalLocalDBList_sql))
                {
                    /*string reg = "";

                    if (string.IsNullOrWhiteSpace(table.LocalDBList.Region_id))
                    {
                        reg = "NULL";
                    }
                    else
                    {
                        reg = table.LocalDBList.Region_id;
                    }*/

                    Script = Script + Environment.NewLine + Environment.NewLine +
                                "INSERT INTO stg.RegionalLocalDBList " + mshint_change + "(" + Environment.NewLine +
                                "\tLocalDbList_id," + Environment.NewLine +
                                "\tRegion_id," + Environment.NewLine +
                                "\tpmUser_insID," + Environment.NewLine +
                                "\tpmUser_updID," + Environment.NewLine +
                                "\tRegionalLocalDbList_insDT," + Environment.NewLine +
                                "\tRegionalLocalDbList_updDT" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "SELECT" + Environment.NewLine +
                                "\tLocalDBList_id," + Environment.NewLine +
                                "\tNULL," + Environment.NewLine +
                                "\t1," + Environment.NewLine +
                                "\t1," + Environment.NewLine +
                                "\tgetdate()," + Environment.NewLine +
                                "\tgetdate()" + Environment.NewLine +
                                "FROM stg.LocalDBList " + mshint_sel + Environment.NewLine +
                                "WHERE LocalDbList_module = '" + table.LocalDBList.LocalDBList_Module + "'" + Environment.NewLine +
                                "AND LocalDbList_name = '" + table.LocalDBList.LocalDBList_Name + "'" + Environment.NewLine +
                                "AND localdblist_schema = '" + table.LocalDBList.LocalDBList_Schema + "'" + Environment.NewLine +
                                "AND NOT EXISTS (SELECT 1 FROM stg.RegionalLocalDBList t " + mshint_sel + "WHERE t.LocalDBList_id = LocalDBList.LocalDBList_id AND t.region_id IS NULL)" + Environment.NewLine +
                                Environment.NewLine + 
                                "UPDATE stg.regionallocaldblist " + mshint_change + Environment.NewLine +
                                "SET" + Environment.NewLine +
                                "\tregionallocaldblist_sql = '" + table.LocalDBList.RegionalLocalDBList_sql.Replace("'", "''") + "'," + Environment.NewLine +
                                "\tpmuser_updid = 1," + Environment.NewLine +
                                "\tregionallocaldblist_upddt = getdate()" + Environment.NewLine +
                                "-- select localdblist_id, regionallocaldblist_id, regionallocaldblist_sql, pmuser_updid, regionallocaldblist_upddt from stg.RegionalLocalDBList" + Environment.NewLine +
                                "WHERE LocalDBList_id IN (" + Environment.NewLine +
                                "\tSELECT LocalDBList_id" + Environment.NewLine +
                                "\tFROM stg.LocalDBList " + mshint_sel + Environment.NewLine +
                                "\tWHERE LocalDbList_module = '" + table.LocalDBList.LocalDBList_Module + "'" + Environment.NewLine +
                                "\tAND LocalDbList_name = '" + table.LocalDBList.LocalDBList_Name + "'" + Environment.NewLine +
                                "\tAND localdblist_schema = '" + table.LocalDBList.LocalDBList_Schema + "'" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "AND coalesce(region_id, 0) <> 101";
                }

                // Проверка
                Script = Script + Environment.NewLine + Environment.NewLine + "-- Проверка";
                Script = Script + Environment.NewLine + "/*";
                Script = Script + Environment.NewLine + "SELECT * FROM stg.LocalDBList WHERE LocalDbList_module = '" + table.LocalDBList.LocalDBList_Module + "' AND LocalDbList_name = '" + table.LocalDBList.LocalDBList_Name + "' AND localdblist_schema = '" + table.LocalDBList.LocalDBList_Schema + "'";
                Script = Script + Environment.NewLine + "SELECT * FROM stg.RegionalLocalDBList WHERE LocalDBList_id IN (SELECT LocalDBList_id FROM stg.LocalDBList WHERE LocalDbList_module = '" + table.LocalDBList.LocalDBList_Module + "' AND LocalDbList_name = '" + table.LocalDBList.LocalDBList_Name + "' AND localdblist_schema = '" + table.LocalDBList.LocalDBList_Schema + "') AND coalesce(region_id,0) <> 101";
                Script = Script + Environment.NewLine + "*/";
            }
            else
            {
                Script = Script +
                    "DO $script$" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine + Environment.NewLine +
                    "\tIF NOT EXISTS (" + Environment.NewLine + 
                    "\t\tSELECT 1" + Environment.NewLine + 
                    "\t\tFROM stg.LocalDBList " + mshint_sel + Environment.NewLine + 
                    "\t\tWHERE LocalDbList_module iLIKE '" + table.LocalDBList.LocalDBList_Module + "'" + Environment.NewLine + 
                    "\t\tAND LocalDbList_name iLIKE '" + table.TableEdit.LocalDBListNameForLike+ "'" + Environment.NewLine + 
                    "\t\tAND localdblist_schema iLIKE '" + table.TableEdit.SchemaNameToSeekForLike + "'" + Environment.NewLine + 
                    "\t\tLIMIT 1" + Environment.NewLine + 
                    "\t)" + Environment.NewLine +
                    "\tTHEN " + Environment.NewLine +
                    "\t\tINSERT INTO stg.LocalDBList " + mshint_change + "(" + Environment.NewLine +
                    "\t\t\tlocaldblist_name," + Environment.NewLine +
                    "\t\t\tlocaldblist_prefix," + Environment.NewLine +
                    "\t\t\tlocaldblist_nick," + Environment.NewLine +
                    "\t\t\tlocaldblist_schema," + Environment.NewLine +
                    "\t\t\tlocaldblist_sql," + Environment.NewLine +
                    "\t\t\tlocaldblist_key," + Environment.NewLine +
                    "\t\t\tlocaldblist_module," + Environment.NewLine +
                    "\t\t\tlocaldblist_descr," + Environment.NewLine +
                    "\t\t\tpmuser_insid," + Environment.NewLine +
                    "\t\t\tpmuser_updid," + Environment.NewLine +
                    "\t\t\tlocaldblist_insdt," + Environment.NewLine +
                    "\t\t\tlocaldblist_upddt" + Environment.NewLine +
                    "\t\t)" + Environment.NewLine +
                    "\t\tVALUES (" + Environment.NewLine +
                    "\t\t\t'" + table.LocalDBList.LocalDBList_Name + "'," + Environment.NewLine +
                    "\t\t\t'" + table.LocalDBList.LocalDBList_Prefix + "'," + Environment.NewLine +
                    "\t\t\t'" + table.LocalDBList.LocalDBList_Nick + "'," + Environment.NewLine +
                    "\t\t\t'" + table.LocalDBList.LocalDBList_Schema + "'," + Environment.NewLine +
                    "\t\t\tNULL," + Environment.NewLine +
                    "\t\t\t'" + table.LocalDBList.LocalDBList_Key + "'," + Environment.NewLine +
                    "\t\t\t'" + table.LocalDBList.LocalDBList_Module + "'," + Environment.NewLine +
                    "\t\t\t'" + table.LocalDBList.LocalDBList_Descr + "'," + Environment.NewLine +
                    "\t\t\t1," + Environment.NewLine +
                    "\t\t\t1," + Environment.NewLine +
                    "\t\t\tlocaltimestamp," + Environment.NewLine +
                    "\t\t\tlocaltimestamp" + Environment.NewLine +
                    "\t\t);" + Environment.NewLine +
                    "\tEND IF;";

                Script = Script + Environment.NewLine + Environment.NewLine +
                            "\tUPDATE stg.LocalDBList " + mshint_change + Environment.NewLine +
                            "\tSET" + Environment.NewLine +
                            "\t\tlocaldblist_name = '" + table.LocalDBList.LocalDBList_Name + "'," + Environment.NewLine +
                            "\t\tlocaldblist_prefix = '" + table.LocalDBList.LocalDBList_Prefix + "'," + Environment.NewLine +
                            "\t\tlocaldblist_nick = '" + table.LocalDBList.LocalDBList_Nick + "'," + Environment.NewLine +
                            "\t\tlocaldblist_schema = '" + table.LocalDBList.LocalDBList_Schema + "'," + Environment.NewLine +
                            "\t\tlocaldblist_key = '" + table.LocalDBList.LocalDBList_Key + "'," + Environment.NewLine +
                            "\t\tlocaldblist_module = '" + table.LocalDBList.LocalDBList_Module + "'," + Environment.NewLine +
                            "\t\tlocaldblist_descr = '" + table.LocalDBList.LocalDBList_Descr + "'," + Environment.NewLine +
                            "\t\tpmuser_updid = 1," + Environment.NewLine +
                            "\t\tlocaldblist_upddt = localtimestamp" + Environment.NewLine +
                            "\t-- select * from stg.LocalDBList" + Environment.NewLine +
                            "\tWHERE LocalDbList_module iLIKE '" + table.LocalDBList.LocalDBList_Module + "'" + Environment.NewLine +
                            "\tAND LocalDbList_name iLIKE '" + table.TableEdit.LocalDBListNameForLike + "'" + Environment.NewLine +
                            "\tAND localdblist_schema iLIKE '" + table.TableEdit.SchemaNameToSeekForLike + "';";

                if (!string.IsNullOrWhiteSpace(table.LocalDBList.RegionalLocalDBList_pgsql))
                {

                    /*string reg = "";

                    if (string.IsNullOrWhiteSpace(table.LocalDBList.Region_id))
                    {
                        reg = "NULL";
                    }
                    else
                    {
                        reg = table.LocalDBList.Region_id;
                    }*/

                    string sql = table.LocalDBList.RegionalLocalDBList_sql ?? "";

                    Script = Script + Environment.NewLine + Environment.NewLine +
                                "\tINSERT INTO stg.RegionalLocalDBList " + mshint_change + "(" + Environment.NewLine +
                                "\t\tLocalDbList_id," + Environment.NewLine +
                                "\t\tRegion_id," + Environment.NewLine +
                                "\t\tpmUser_insID," + Environment.NewLine +
                                "\t\tpmUser_updID," + Environment.NewLine +
                                "\t\tRegionalLocalDbList_insDT," + Environment.NewLine +
                                "\t\tRegionalLocalDbList_updDT" + Environment.NewLine +
                                "\t)" + Environment.NewLine +
                                "\tSELECT" + Environment.NewLine +
                                "\t\tLocalDBList_id," + Environment.NewLine +
                                "\t\tNULL," + Environment.NewLine +
                                "\t\t1," + Environment.NewLine +
                                "\t\t1," + Environment.NewLine +
                                "\t\tlocaltimestamp," + Environment.NewLine +
                                "\t\tlocaltimestamp" + Environment.NewLine +
                                "\tFROM stg.LocalDBList " + mshint_sel + Environment.NewLine +
                                "\tWHERE LocalDbList_module iLIKE '" + table.LocalDBList.LocalDBList_Module + "'" + Environment.NewLine +
                                "\tAND LocalDbList_name iLIKE '" + table.LocalDBList.LocalDBList_Name + "'" + Environment.NewLine +
                                "\tAND localdblist_schema iLIKE '" + table.LocalDBList.LocalDBList_Schema + "'" + Environment.NewLine +
                                "\tAND NOT EXISTS (SELECT 1 FROM stg.RegionalLocalDBList t WHERE t.LocalDBList_id = LocalDBList.LocalDBList_id AND t.region_id IS NULL);" + Environment.NewLine +
                                Environment.NewLine +
                                "\tUPDATE stg.regionallocaldblist " + mshint_change + Environment.NewLine +
                                "\tSET" + Environment.NewLine +
                                "\t\tregionallocaldblist_sql = '" + sql.Replace("'", "''") + "'," + Environment.NewLine +
                                "\t\tregionallocaldblist_pgsql = '" + table.LocalDBList.RegionalLocalDBList_pgsql.Replace("'", "''") + "'," + Environment.NewLine +
                                "\t\tpmuser_updid = 1," + Environment.NewLine +
                                "\t\tregionallocaldblist_upddt = localtimestamp" + Environment.NewLine +
                                "\t-- select localdblist_id, regionallocaldblist_id, regionallocaldblist_pgsql, regionallocaldblist_sql, pmuser_updid, regionallocaldblist_upddt from stg.RegionalLocalDBList" + Environment.NewLine +
                                "\tWHERE LocalDBList_id IN (" + Environment.NewLine +
                                "\t\tSELECT LocalDBList_id" + Environment.NewLine +
                                "\t\tFROM stg.LocalDBList " + mshint_sel + Environment.NewLine +
                                "\t\tWHERE LocalDbList_module iLIKE '" + table.LocalDBList.LocalDBList_Module + "'" + Environment.NewLine +
                                "\t\tAND LocalDbList_name iLIKE '" + table.LocalDBList.LocalDBList_Name + "'" + Environment.NewLine +
                                "\t\tAND localdblist_schema iLIKE '" + table.LocalDBList.LocalDBList_Schema + "'" + Environment.NewLine +
                                "\t)" + Environment.NewLine +
                                "\tAND coalesce(region_id, 0) <> 101;" + Environment.NewLine;
                }

                Script = Script +
                    Environment.NewLine + "END;" + Environment.NewLine + "$script$;";

                // Проверка
                Script = Script + Environment.NewLine + Environment.NewLine + "-- Проверка";
                Script = Script + Environment.NewLine + "/*";
                Script = Script + Environment.NewLine + "SELECT * FROM stg.LocalDBList WHERE LocalDbList_module iLIKE '" + table.LocalDBList.LocalDBList_Module + "' AND LocalDbList_name iLIKE '" + table.LocalDBList.LocalDBList_Name + "' AND localdblist_schema iLIKE '" + table.LocalDBList.LocalDBList_Schema + "'";
                Script = Script + Environment.NewLine + "SELECT * FROM stg.RegionalLocalDBList WHERE LocalDBList_id IN (SELECT LocalDBList_id FROM stg.LocalDBList " + mshint_sel + "WHERE LocalDbList_module iLIKE '" + table.LocalDBList.LocalDBList_Module + "' AND LocalDbList_name iLIKE '" + table.LocalDBList.LocalDBList_Name + "' AND localdblist_schema iLIKE '" + table.LocalDBList.LocalDBList_Schema + "') AND coalesce(region_id,0) <> 101";
                Script = Script + Environment.NewLine + "*/";
            }

            return (
                TitleScript +
                Environment.NewLine +
                Script
                )
                .TrimInnerNewLine()
                .TrimNewLine(Environment.NewLine);
        }

        /// <summary>Собрать скрипт для добавления записи в nsi.RefTableRegistry</summary>
        public string GenerateRefTableRegistry()
        {
            string Script = "";

            if (this.RefTableRegistry == null) return Script;

            // заголовок скрипта (информация о задаче)
            string TitleScript = MainWindow.Task.TitleScript(this.TargetDB, "data", false, false);
            if (TitleScript == null) TitleScript = ""; //-V3022
            if (TitleScript != "") Script = Script + Environment.NewLine;

            // хинт для MS SQL
            string mshint_change = "";
            if (this.TargetDB == Utilities.TargetDBType.MSSQL) mshint_change = "WITH (rowlock) ";
            string mshint_sel = "";
            if (this.TargetDB == Utilities.TargetDBType.MSSQL) mshint_sel = "WITH (nolock) ";

            // концовка оператора INSERT
            string insert_end = ")";
            if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD)) insert_end = ") ON CONFLICT DO NOTHING;";

            if (this.TargetDB == Utilities.TargetDBType.MSSQL) // для MS SQL
            {
                Script = Script +
                            "SET DATEFORMAT ymd" + Environment.NewLine + Environment.NewLine +
                            "SET IDENTITY_INSERT nsi.RefTableRegistry ON" + Environment.NewLine + Environment.NewLine +
                            "IF NOT EXISTS (SELECT TOP(1) 1 FROM nsi.RefTableRegistry " + mshint_sel + "WHERE RefTableRegistry_id = " + RefTableRegistry.RefTableRegistry_id + ")" + Environment.NewLine +
                            "INSERT INTO nsi.RefTableRegistry " + mshint_change + "(RefTableRegistry_id, RefTableRegistry_Oid, RefTableRegistry_SysNick, RefTableRegistry_createDT, RefTableRegistry_publishDT, RefTableRegistry_IsArchive, RefTableRegistry_FullName, RefTableRegistry_Nick, pmUser_insID, pmUser_updID, RefTableRegistry_insDT, RefTableRegistry_updDT) " + Environment.NewLine +
                            "VALUES (" + RefTableRegistry.RefTableRegistry_id + ", '" + RefTableRegistry.OID + "', '" + TableEdit.FullTableNameReady + "', '" + RefTableRegistry.CreateDate.ToString("yyyy-MM-dd HH:mm") + "', '" +
                              RefTableRegistry.CreateDate.ToString("yyyy-MM-dd HH:mm") + "', 1, '" + RefTableRegistry.FullName + "', '" + RefTableRegistry.ShortName + "',  1, 1, getdate(), getdate()" + insert_end + Environment.NewLine + Environment.NewLine +
                            "SET IDENTITY_INSERT nsi.RefTableRegistry OFF" + Environment.NewLine + Environment.NewLine;

                Script = Script +
                            "SET IDENTITY_INSERT nsi.RefTableRegistryVersion ON" + Environment.NewLine + Environment.NewLine +
                            "IF NOT EXISTS(SELECT TOP 1 1 FROM nsi.RefTableRegistryVersion " + mshint_sel + "WHERE RefTableRegistryVersion_id = " + RefTableRegistry.RefTableRegistryVersion_id + ")" + Environment.NewLine +
                            "INSERT INTO nsi.RefTableRegistryVersion " + mshint_change + "(RefTableRegistryVersion_id, RefTableRegistry_id, RefTableRegistryVersion_Num, RefTableRegistryVersion_createDate, RefTableRegistryVersion_publishDate, RefTableRegistryVersion_lastUpdateDate, pmUser_insID, pmUser_updID, RefTableRegistryVersion_insDT, RefTableRegistryVersion_updDT) " + Environment.NewLine +
                            "VALUES(" + RefTableRegistry.RefTableRegistryVersion_id + ", " + RefTableRegistry.RefTableRegistry_id + ", '" + RefTableRegistry.Version + "', '" + RefTableRegistry.PublishDate.ToString("yyyy-MM-dd HH:mm") + "', '" + RefTableRegistry.PublishDate.ToString("yyyy-MM-dd HH:mm") + "', '" + RefTableRegistry.PublishDate.ToString("yyyy-MM-dd HH:mm") + "', 1, 1, getdate(), getdate()" + insert_end + Environment.NewLine + Environment.NewLine +
                            "SET IDENTITY_INSERT nsi.RefTableRegistryVersion OFF" + Environment.NewLine + Environment.NewLine;

                Script = Script +
                            "UPDATE nsi.RefTableRegistry " + mshint_change + " SET" + Environment.NewLine +
                            "\tRefTableRegistryVersion_id = " + RefTableRegistry.RefTableRegistryVersion_id + "," + Environment.NewLine +
                            "\tpmUser_updID = 1," + Environment.NewLine +
                            "\tRefTableRegistry_updDT = getdate()" + Environment.NewLine +
                            "WHERE RefTableRegistry_id = " + RefTableRegistry.RefTableRegistry_id + Environment.NewLine + Environment.NewLine;
            }
            else
            {
                Script = Script +
                    "DO $script$" + Environment.NewLine +
                    "BEGIN" + Environment.NewLine + Environment.NewLine;

                Script = Script +
                            "INSERT INTO nsi.RefTableRegistry " + mshint_change + "(RefTableRegistry_id, RefTableRegistry_Oid, RefTableRegistry_SysNick, RefTableRegistry_createDT, RefTableRegistry_publishDT, RefTableRegistry_IsArchive, RefTableRegistry_FullName, RefTableRegistry_Nick, pmUser_insID, pmUser_updID, RefTableRegistry_insDT, RefTableRegistry_updDT) " + Environment.NewLine +
                            "VALUES (" + RefTableRegistry.RefTableRegistry_id + ", '" + RefTableRegistry.OID + "', '" + TableEdit.FullTableNameReady + "', '" + RefTableRegistry.CreateDate.ToString("yyyy-MM-dd HH:mm") + "', '" +
                              RefTableRegistry.CreateDate.ToString("yyyy-MM-dd HH:mm") + "', 1, '" + RefTableRegistry.FullName + "', '" + RefTableRegistry.ShortName + "',  1, 1, localtimestamp, localtimestamp" + insert_end + Environment.NewLine + Environment.NewLine;

                Script = Script +
                            "INSERT INTO nsi.RefTableRegistryVersion " + mshint_change + "(RefTableRegistryVersion_id, RefTableRegistry_id, RefTableRegistryVersion_Num, RefTableRegistryVersion_createDate, RefTableRegistryVersion_publishDate, RefTableRegistryVersion_lastUpdateDate, pmUser_insID, pmUser_updID, RefTableRegistryVersion_insDT, RefTableRegistryVersion_updDT) " + Environment.NewLine +
                            "VALUES(" + RefTableRegistry.RefTableRegistryVersion_id + ", " + RefTableRegistry.RefTableRegistry_id + ", '" + RefTableRegistry.Version + "', '" + RefTableRegistry.PublishDate.ToString("yyyy-MM-dd HH:mm") + "', '" + RefTableRegistry.PublishDate.ToString("yyyy-MM-dd HH:mm") + "', '" + RefTableRegistry.PublishDate.ToString("yyyy-MM-dd HH:mm") + "', 1, 1, localtimestamp, localtimestamp" + insert_end + Environment.NewLine + Environment.NewLine;

                Script = Script +
                            "UPDATE nsi.RefTableRegistry " + mshint_change + " SET" + Environment.NewLine +
                            "\tRefTableRegistryVersion_id = " + RefTableRegistry.RefTableRegistryVersion_id + "," + Environment.NewLine +
                            "\tpmUser_updID = 1," + Environment.NewLine +
                            "\tRefTableRegistry_updDT = localtimestamp" + Environment.NewLine +
                            "WHERE RefTableRegistry_id = " + RefTableRegistry.RefTableRegistry_id + ";" + Environment.NewLine + Environment.NewLine;

                Script = Script +
                    "END;" + Environment.NewLine + "$script$;" + Environment.NewLine + Environment.NewLine;
            }

            // Проверка
            Script = Script + "-- Проверка" + Environment.NewLine;
            Script = Script + "/*" + Environment.NewLine;
            Script = Script + "SELECT * FROM nsi.RefTableRegistry WHERE RefTableRegistry_id = " + RefTableRegistry.RefTableRegistry_id + ";" + Environment.NewLine;
            Script = Script + "SELECT * FROM nsi.RefTableRegistryVersion WHERE RefTableRegistry_id = " + RefTableRegistry.RefTableRegistry_id + ";" + Environment.NewLine;
            Script = Script + "*/";

            return (
                TitleScript +
                Environment.NewLine +
                Script
                )
                .TrimInnerNewLine()
                .TrimNewLine(Environment.NewLine);
        }

        /// <summary>
        /// Собрать скрипт для создания/сдвига Sequence
        /// </summary>
        /// <param name="isAddRegion">=true - добавить условие региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="table">описание таблицы</param>
        /// <returns></returns>
        public static string GenerateSequence(bool isAddRegion, string txtRegion, TableInfo table)
        {
            string Script = "";
            string ScriptRegionBegin = "";
            string ScriptRegionEnd = "";

            // заголовок скрипта (информация о задаче)
            string TitleScript = MainWindow.Task.TitleScript(table.ParentTableDB.TargetDB, "alter", false, false);
            if (TitleScript == null) TitleScript = ""; //-V3022
            TitleScript += Environment.NewLine;

            table.ParentTableDB.isAddRegion = isAddRegion;

            // Проверка на региональность
            if (isAddRegion)
            {
                if (table.ParentTableDB.TargetDB == Utilities.TargetDBType.MSSQL)
                {
                    ScriptRegionBegin += "IF (dbo.getregion() = " + txtRegion.Trim() + " OR db_name() IN ('ProMedWebRelease', 'ProMedTest', 'promeddev'))";

                    ScriptRegionBegin += Environment.NewLine + "BEGIN" + Environment.NewLine;

                    ScriptRegionEnd += Environment.NewLine + "END";
                    ScriptRegionEnd += Environment.NewLine;
                }

                if (table.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL)
                {
                    ScriptRegionBegin += "DO $script$";
                    ScriptRegionBegin += Environment.NewLine + "BEGIN";
                    ScriptRegionBegin += Environment.NewLine + Environment.NewLine + "IF (dbo.getregion() = " + txtRegion.Trim() + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))";

                    ScriptRegionBegin += Environment.NewLine + "THEN" + Environment.NewLine;

                    ScriptRegionEnd += Environment.NewLine + "END IF;";
                    ScriptRegionEnd += Environment.NewLine + Environment.NewLine +
                        "END;" + Environment.NewLine +
                        "$script$;" + Environment.NewLine;
                }
            }

            if (TableInfo.AddSequienceToScript(table) != "")
            {
                Script = TableInfo.AddSequienceToScript(table) + Environment.NewLine;
            }

            return (
                TitleScript +
                ScriptRegionBegin +
                Script +
                ScriptRegionEnd
                )
                .TrimInnerNewLine()
                .TrimNewLine(Environment.NewLine);
        }

        /// <summary>
        /// Собрать скрипт для шаблона procedure
        /// </summary>
        /// <param name="isAddRegion">=true - добавить условие региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="table">описание таблицы</param>
        /// <returns></returns>
        public static string GenerateShablonProc(bool isAddRegion, string txtRegion, TableDB table)
        {
            string Script = "";
            string schema = "";

            if (isAddRegion)
            {
                schema = "r" + txtRegion;
            }
            else
            {
                schema = "dbo";
            }

            string objectname = "newprocedurename";
            string objecttype = "P";
            string scripttype = "PROCEDURE";

            // заголовок скрипта (информация о задаче)
            string TitleScript = MainWindow.Task.TitleScript(table.TargetDB, "proc", false, false);
            if (TitleScript == null) TitleScript = ""; //-V3022
            TitleScript += Environment.NewLine;

            table.isAddRegion = isAddRegion;

            if (table.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                string original_text =
"CREATE " + scripttype.ToUpper() + " " + schema + "." + objectname + " (" + Environment.NewLine +
"\t@Error_Code int = null output," + Environment.NewLine +
"\t@Error_Message varchar(4000) = null output" + Environment.NewLine +
@")
AS
SET NOCOUNT ON

BEGIN TRY" + Environment.NewLine +
Environment.NewLine +
"\tBEGIN TRAN" + Environment.NewLine +
Environment.NewLine +
"\tDECLARE @datetime DATETIME = dbo.tzgetdate()" + Environment.NewLine +
Environment.NewLine +
"\tCOMMIT TRAN" + Environment.NewLine +
Environment.NewLine +
@"END TRY

BEGIN CATCH" + Environment.NewLine +
"\tSET @Error_Code = ISNULL(@Error_Code, error_number())" + Environment.NewLine +
"\tSET @Error_Message = ISNULL(@Error_Message, SUBSTRING('" + schema + "." + objectname + ": Ошибка [' + COALESCE(CAST(@Error_Code AS NVARCHAR(max)), 'N\\A') + '] в строке [' + COALESCE(CAST(ERROR_LINE() AS NVARCHAR(max)), 'N\\A') + '] ' + ERROR_MESSAGE(), 0, 4000))" + Environment.NewLine +
"\tIF @@trancount > 0" + Environment.NewLine +
"\t\tROLLBACK TRAN" + Environment.NewLine +
@"END CATCH

SET NOCOUNT OFF";

                if (isAddRegion)
                {
                    Script += "IF (dbo.getregion() = " + txtRegion.Trim() + " OR db_name() IN ('ProMedWebRelease', 'ProMedTest', 'promeddev'))";
                    Script += Environment.NewLine + "AND EXISTS (SELECT 1 FROM sys.schemas s WITH (nolock) WHERE s.name = '" + schema + "')";
                    Script += Environment.NewLine + "BEGIN" + Environment.NewLine + Environment.NewLine;
                }

                Script += "IF OBJECT_ID(N'" + schema + "." + objectname + "', N'" + objecttype + "') IS NOT NULL";

                if (isAddRegion)
                {
                    Script += Environment.NewLine + "\tDROP " + scripttype.ToUpper() + " " + schema + "." + objectname;
                    Script += Environment.NewLine;
                    Script += Environment.NewLine + "IF OBJECT_ID(N'" + schema + "." + objectname + "', N'" + objecttype + "') IS NULL";
                    Script += Environment.NewLine + "\tEXECUTE('";
                }
                else
                {
                    Script += Environment.NewLine + "\tDROP " + scripttype.ToUpper() + " " + schema + "." + objectname;
                    Script += Environment.NewLine + "GO";
                }

                Script += Environment.NewLine;

                if (isAddRegion)
                {
                    Script += original_text.Replace("'", "''");
                    Script += Environment.NewLine + "')";
                    Script += Environment.NewLine + Environment.NewLine + "END";
                }
                else
                {
                    Script += original_text;
                }

                Script += Environment.NewLine;

                // добавляем в конце Environment.NewLine + GO + Environment.NewLine, если его нет
                Script = Utilities.Databases.AddEndGO(Script);
            }
            else
            {
                string original_text =
"CREATE OR REPLACE " + scripttype.ToUpper() + " " + schema + "." + objectname + " (" + Environment.NewLine +
"\tINOUT error_code character varying DEFAULT NULL::character varying," + Environment.NewLine +
"\tINOUT error_message character varying DEFAULT NULL::character varying" + Environment.NewLine +
@")
LANGUAGE plpgsql
AS $procedure$
DECLARE" + Environment.NewLine +
"\tp_datetime TIMESTAMP = localtimestamp;" + Environment.NewLine +
//"\tp_datetimetz TIMESTAMPTZ = p_datetime::TIMESTAMPTZ;" + Environment.NewLine +
"\terror_RETURNED_SQLSTATE varchar;" + Environment.NewLine +
"\terror_MESSAGE_TEXT varchar;" + Environment.NewLine +
"\terror_PG_EXCEPTION_CONTEXT varchar;" + Environment.NewLine +
@"BEGIN

EXCEPTION" + Environment.NewLine +
"\tWHEN OTHERS THEN" + Environment.NewLine +
"\t\tGET STACKED DIAGNOSTICS" + Environment.NewLine +
"\t\t\terror_RETURNED_SQLSTATE = RETURNED_SQLSTATE," + Environment.NewLine +
"\t\t\terror_MESSAGE_TEXT = MESSAGE_TEXT," + Environment.NewLine +
"\t\t\terror_PG_EXCEPTION_CONTEXT = PG_EXCEPTION_CONTEXT;" + Environment.NewLine +
Environment.NewLine +
"\t\terror_code := COALESCE(error_code, error_RETURNED_SQLSTATE);" + Environment.NewLine +
"\t\terror_message := COALESCE(error_message, CONCAT('" + schema + "." + objectname + ": ', 'Ошибка \"', COALESCE(error_MESSAGE_TEXT, 'N\\A'), '\". Место: ', chr(10), error_PG_EXCEPTION_CONTEXT));" + Environment.NewLine +
@"END; 
$procedure$;";

                if (isAddRegion)
                {
                    Script += "DO $script$";
                    Script += Environment.NewLine + "BEGIN";
                    Script += Environment.NewLine + Environment.NewLine + "IF (dbo.getregion() = " + txtRegion + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))";
                    Script += Environment.NewLine + "AND EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '" + schema + "')";
                    Script += Environment.NewLine + "THEN" + Environment.NewLine;
                    Script += Environment.NewLine + "EXECUTE $reg$" + Environment.NewLine;
                }

                Script +=
                        "SELECT dbo.xp_dropfns('" + schema.Replace("\"", "") + "." + objectname.Replace("\"", "") + "');" + Environment.NewLine +
                        Environment.NewLine + original_text;

                if (isAddRegion)
                {
                    Script += Environment.NewLine + "$reg$;" + Environment.NewLine;
                    Script += Environment.NewLine + "END IF;" + Environment.NewLine;
                    Script += Environment.NewLine + "END;" + Environment.NewLine + "$script$;";
                }

                Script += Environment.NewLine;
            }

            // убираем лишние переводы строки в конце, оставляем один
            return (TitleScript + Script)
                .TrimEndNewLine(Environment.NewLine);
        }

        /// <summary>
        /// Собрать скрипт для шаблона function
        /// </summary>
        /// <param name="isAddRegion">=true - добавить условие региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="table">описание таблицы</param>
        /// <returns></returns>
        public static string GenerateShablonFunc(bool isAddRegion, string txtRegion, TableDB table)
        {
            string Script = "";
            string schema = "";

            if (isAddRegion)
            {
                schema = "r" + txtRegion;
            }
            else
            {
                schema = "dbo";
            }

            string objectname = "newfunctionname";
            string objecttype = "FN";
            string scripttype = "FUNCTION";

            // заголовок скрипта (информация о задаче)
            string TitleScript = MainWindow.Task.TitleScript(table.TargetDB, "func", false, false);
            if (TitleScript == null) TitleScript = ""; //-V3022
            TitleScript += Environment.NewLine;

            table.isAddRegion = isAddRegion;

            if (table.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                string original_text =
"CREATE " + scripttype.ToUpper() + " " + schema + "." + objectname + " (" + Environment.NewLine +
"\t@Error_Code int = null output," + Environment.NewLine +
"\t@Error_Message varchar(4000) = null output" + Environment.NewLine +
@")
RETURNS VARCHAR(max)
AS
BEGIN
SET NOCOUNT ON" + Environment.NewLine +
Environment.NewLine +
"\tBEGIN TRY" + Environment.NewLine +
Environment.NewLine +
"\tDECLARE @datetime DATETIME = dbo.tzgetdate()" + Environment.NewLine +
"\tDECLARE @result VARCHAR(max)" + Environment.NewLine +
Environment.NewLine +
"\tRETURN @result" + Environment.NewLine +
@"END TRY

BEGIN CATCH" + Environment.NewLine +
"\tSET @Error_Code = ISNULL(@Error_Code, error_number())" + Environment.NewLine +
"\tSET @Error_Message = ISNULL(@Error_Message, SUBSTRING('" + schema + "." + objectname + ": Ошибка [' + COALESCE(CAST(@Error_Code AS NVARCHAR(max)), 'N\\A') + '] в строке [' + COALESCE(CAST(ERROR_LINE() AS NVARCHAR(max)), 'N\\A') + '] ' + ERROR_MESSAGE(), 0, 4000))" + Environment.NewLine +
"\tIF @@trancount > 0" + Environment.NewLine +
"\t\tROLLBACK TRAN" + Environment.NewLine +
@"END CATCH

SET NOCOUNT OFF
END";

                if (isAddRegion)
                {
                    Script += "IF (dbo.getregion() = " + txtRegion.Trim() + " OR db_name() IN ('ProMedWebRelease', 'ProMedTest', 'promeddev'))";
                    Script += Environment.NewLine + "AND EXISTS (SELECT 1 FROM sys.schemas s WITH (nolock) WHERE s.name = '" + schema + "')";
                    Script += Environment.NewLine + "BEGIN" + Environment.NewLine + Environment.NewLine;
                }

                Script += "IF OBJECT_ID(N'" + schema + "." + objectname + "', N'" + objecttype + "') IS NOT NULL";

                if (isAddRegion)
                {
                    Script += Environment.NewLine + "\tDROP " + scripttype.ToUpper() + " " + schema + "." + objectname;
                    Script += Environment.NewLine;
                    Script += Environment.NewLine + "IF OBJECT_ID(N'" + schema + "." + objectname + "', N'" + objecttype + "') IS NULL";
                    Script += Environment.NewLine + "\tEXECUTE('";
                }
                else
                {
                    Script += Environment.NewLine + "\tDROP " + scripttype.ToUpper() + " " + schema + "." + objectname;
                    Script += Environment.NewLine + "GO";
                }

                Script += Environment.NewLine;

                if (isAddRegion)
                {
                    Script += original_text.Replace("'", "''");
                    Script += Environment.NewLine + "')";
                    Script += Environment.NewLine + Environment.NewLine + "END";
                }
                else
                {
                    Script += original_text;
                }

                Script += Environment.NewLine;

                // добавляем в конце Environment.NewLine + GO + Environment.NewLine, если его нет
                Script = Utilities.Databases.AddEndGO(Script);
            }
            else
            {
                string original_text = "CREATE OR REPLACE " + scripttype.ToUpper() + " " + schema + "." + objectname + " (" + Environment.NewLine +
"\tINOUT error_code character varying DEFAULT NULL::character varying," + Environment.NewLine +
"\tINOUT error_message character varying DEFAULT NULL::character varying" + Environment.NewLine +
@")
RETURNS record
LANGUAGE plpgsql
AS $function$
DECLARE" + Environment.NewLine +
"\tp_datetime TIMESTAMP = localtimestamp;" + Environment.NewLine +
//"\tp_datetimetz TIMESTAMPTZ = p_datetime::TIMESTAMPTZ;" + Environment.NewLine +
"\terror_RETURNED_SQLSTATE varchar;" + Environment.NewLine +
"\terror_MESSAGE_TEXT varchar;" + Environment.NewLine +
"\terror_PG_EXCEPTION_CONTEXT varchar;" + Environment.NewLine +
@"BEGIN

EXCEPTION" + Environment.NewLine +
"\tWHEN OTHERS THEN" + Environment.NewLine +
"\t\tGET STACKED DIAGNOSTICS" + Environment.NewLine +
"\t\t\terror_RETURNED_SQLSTATE = RETURNED_SQLSTATE," + Environment.NewLine +
"\t\t\terror_MESSAGE_TEXT = MESSAGE_TEXT," + Environment.NewLine +
"\t\t\terror_PG_EXCEPTION_CONTEXT = PG_EXCEPTION_CONTEXT;" + Environment.NewLine +
Environment.NewLine +
"\t\terror_code := COALESCE(error_code, error_RETURNED_SQLSTATE);" + Environment.NewLine +
"\t\terror_message := COALESCE(error_message, CONCAT('" + schema + "." + objectname + ": ', 'Ошибка \"', COALESCE(error_MESSAGE_TEXT, 'N\\A'), '\". Место: ', chr(10), error_PG_EXCEPTION_CONTEXT));" + Environment.NewLine +
@"END; 
$function$;";

                if (isAddRegion)
                {
                    Script += "DO $script$";
                    Script += Environment.NewLine + "BEGIN";
                    Script += Environment.NewLine + Environment.NewLine + "IF (dbo.getregion() = " + txtRegion + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))";
                    Script += Environment.NewLine + "AND EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '" + schema + "')";
                    Script += Environment.NewLine + "THEN" + Environment.NewLine;
                    Script += Environment.NewLine + "EXECUTE $reg$" + Environment.NewLine;
                }

                Script +=
                        "SELECT dbo.xp_dropfns('" + schema.Replace("\"", "") + "." + objectname.Replace("\"", "") + "');" + Environment.NewLine +
                        Environment.NewLine + original_text;

                if (isAddRegion)
                {
                    Script += Environment.NewLine + "$reg$;" + Environment.NewLine;
                    Script += Environment.NewLine + "END IF;" + Environment.NewLine;
                    Script += Environment.NewLine + "END;" + Environment.NewLine + "$script$;";
                }

                Script += Environment.NewLine;
            }

            // убираем лишние переводы строки в конце, оставляем один
            return (TitleScript + Script)
                .TrimEndNewLine(Environment.NewLine);
        }

        /// <summary>
        /// Собрать скрипт для шаблона view
        /// </summary>
        /// <param name="isAddRegion">=true - добавить условие региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="table">описание таблицы</param>
        /// <returns></returns>
        public static string GenerateShablonView(bool isAddRegion, string txtRegion, TableDB table)
        {
            string Script = "";
            string schema = "";

            if (isAddRegion)
            {
                schema = "r" + txtRegion;
            }
            else
            {
                schema = "dbo";
            }

            string objectname = "v_newviewname";
            string tablename = "tablename";
            string objecttype = "V";
            string scripttype = "VIEW";

            // заголовок скрипта (информация о задаче)
            string TitleScript = MainWindow.Task.TitleScript(table.TargetDB, "view", false, false);
            if (TitleScript == null) TitleScript = ""; //-V3022
            TitleScript += Environment.NewLine;

            table.isAddRegion = isAddRegion;

            if (table.TargetDB == Utilities.TargetDBType.MSSQL)
            {
                string original_text = "CREATE " + scripttype.ToUpper() + " " + schema + "." + objectname + @"
AS
SELECT

FROM " + schema + "." + tablename + @"
WHERE (1 = 1)
GO";
                if (isAddRegion)
                {
                    Script += "IF (dbo.getregion() = " + txtRegion.Trim() + " OR db_name() IN ('ProMedWebRelease', 'ProMedTest', 'promeddev'))";
                    Script += Environment.NewLine + "AND EXISTS (SELECT 1 FROM sys.schemas s WITH (nolock) WHERE s.name = '" + schema + "')";
                    Script += Environment.NewLine + "BEGIN" + Environment.NewLine + Environment.NewLine;
                }

                Script += "IF OBJECT_ID(N'" + schema + "." + objectname + "', N'" + objecttype + "') IS NOT NULL";

                if (isAddRegion)
                {
                    Script += Environment.NewLine + "\tDROP " + scripttype.ToUpper() + " " + schema + "." + objectname;
                    Script += Environment.NewLine;
                    Script += Environment.NewLine + "IF OBJECT_ID(N'" + schema + "." + objectname + "', N'" + objecttype + "') IS NULL";
                    Script += Environment.NewLine + "\tEXECUTE('";
                }
                else
                {
                    Script += Environment.NewLine + "\tDROP " + scripttype.ToUpper() + " " + schema + "." + objectname;
                    Script += Environment.NewLine + "GO";
                }

                Script += Environment.NewLine;

                if (isAddRegion)
                {
                    Script += original_text.Replace("'", "''");
                    Script += Environment.NewLine + "')";
                    Script += Environment.NewLine + Environment.NewLine + "END";
                }
                else
                {
                    Script += original_text;
                }

                Script += Environment.NewLine;

                // добавляем в конце Environment.NewLine + GO + Environment.NewLine, если его нет
                Script = Utilities.Databases.AddEndGO(Script);
            }
            else
            {
                string original_text = "CREATE OR REPLACE " + scripttype.ToUpper() + " " + schema + "." + objectname + @"
AS
SELECT

FROM " + schema + "." + tablename + @"
WHERE (1 = 1)
;";

                if (isAddRegion)
                {
                    Script += "DO $script$";
                    Script += Environment.NewLine + "BEGIN";
                    Script += Environment.NewLine + Environment.NewLine + "IF (dbo.getregion() = " + txtRegion + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))";
                    Script += Environment.NewLine + "AND EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '" + schema + "')";
                    Script += Environment.NewLine + "THEN" + Environment.NewLine;
                    Script += Environment.NewLine + "EXECUTE $reg$" + Environment.NewLine;
                }

                Script +=
                    "SELECT dbo.xp_gen_view('" + schema + "." + objectname + "'," + Environment.NewLine +
                    "$viewtext$" + Environment.NewLine +
                    original_text + Environment.NewLine +
                    "$viewtext$" + Environment.NewLine +
                    ",2);";

                if (isAddRegion)
                {
                    Script += Environment.NewLine + "$reg$;" + Environment.NewLine;
                    Script += Environment.NewLine + "END IF;" + Environment.NewLine;
                    Script += Environment.NewLine + "END;" + Environment.NewLine + "$script$;";
                }

                Script += Environment.NewLine;
            }

            // убираем лишние переводы строки в конце, оставляем один
            return (TitleScript + Script)
                .TrimEndNewLine(Environment.NewLine);
        }
    }

    // =========================================================================================================
    /// <summary>Класс с информацией для nsi.RefTableRegistry</summary>
    public class RefTableRegistry
    {
        /// <summary>
        /// дата публикации первой версии справочника НСИ
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// дата публикации текущей версии справочника НСИ
        /// </summary>
        public DateTime PublishDate { get; set; }
        /// <summary>
        /// идентификатор записи в nsi.RefTableRegistry 
        /// </summary>
        public long RefTableRegistry_id { get; set; }
        /// <summary>
        /// идентификатор записи в sni.RefTableRegistryVersion
        /// </summary>
        public long RefTableRegistryVersion_id { get; set; }
        /// <summary>
        /// OID справочника НСИ
        /// </summary>
        public string OID { get; set; }
        /// <summary>
        /// полное наименование справочника НСИ
        /// </summary>
        public string FullName { get; set; }
        /// <summary>
        /// краткое наименование справочника НСИ
        /// </summary>
        public string ShortName { get; set; }
        /// <summary>
        /// версия справочника НСИ
        /// </summary>
        public string Version { get; set; }
    }

    // =========================================================================================================
    /// <summary>Класс с информацией для stg.LocalDBList</summary>
    public class LocalDBList
    {
        /// <summary>
        /// наименование локального справочника
        /// </summary>
        public string LocalDBList_Name { get; set; }
        /// <summary>
        /// primary key локального справочника
        /// </summary>
        public string LocalDBList_id { get; set; }
        /// <summary>
        /// идентификатор локального справочника
        /// </summary>
        public string LocalDBList_Prefix { get; set; }
        /// <summary>
        /// название таблицы локального справочника
        /// </summary>
        public string LocalDBList_Nick { get; set; }
        /// <summary>
        /// схема локального справочника
        /// </summary>
        public string LocalDBList_Schema { get; set; }
        /// <summary>
        /// запрос для MS SQL
        /// </summary>
        public string RegionalLocalDBList_sql { get; set; }
        /// <summary>
        /// запрос для Postgres
        /// </summary>
        public string RegionalLocalDBList_pgsql { get; set; }
        /// <summary>
        /// поле - primary key лоального справочника
        /// </summary>
        public string LocalDBList_Key { get; set; }
        /// <summary>
        /// модуль
        /// </summary>
        public string LocalDBList_Module { get; set; }
        /// <summary>
        /// описание
        /// </summary>
        public string LocalDBList_Descr { get; set; }
        /// <summary>
        /// целевой регион
        /// </summary>
        public string Region_id { get; set; }
    }

    // =========================================================================================================
    /// <summary>Класс Индекс</summary>
    public class IndexDB
    {
        /// <summary>Родительская таблица</summary>
        public TableDB ParentTableDB { get; set; }

        /// <summary>
        /// Конструктор IndexDB - инициализация значений по умолчанию
        /// </summary>
        /// <param name="parent">описание таблицы</param>
        public IndexDB(TableDB parent)
        {
            this.ParentTableDB = parent;
        }

        string _index_name;
        /// <summary>Имя индекса</summary>
        public string IndexName
        {
            get
            {
                return _index_name ?? "";
            }
            set
            {
                _index_name = value;
                if (string.IsNullOrWhiteSpace(_index_name)) _index_name = "";
                _index_name = _index_name
                    .Replace("\"", string.Empty)
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .Trim();
            }
        }

        /// <summary>Имя индекса - в оригинальном регистре, но без кавычек</summary>
        public string IndexNameReady => IndexName.Replace("\"", "");

        /// <summary>Имя индекса - для сравнения, в нижнем регистре и без кавычек</summary>
        public string IndexNameCompare => IndexNameReady.ToLower();

        /// <summary>Имя индекса - для использования в скрипте</summary>
        public string IndexNameToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                    case Utilities.TargetDBType.PGSQL:
                        return this.IndexNameReady.ToLower();
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        return this.IndexNameReady;
                }
            }
        }

        /// <summary>Имя индекса - для использования в имени файла</summary>
        public string IndexNameToFilename
        {
            get
            {
                return (
                    ParentTableDB.TableEdit.SchemaNameReady
                        .Replace(" ", string.Empty)
                        .Replace(".", string.Empty)
                    + " " +
                    this.IndexNameReady
                        .Replace(" ", string.Empty)
                        .Replace(".", string.Empty)
                ).ToLower();
            }
        }

        /// <summary>Имя индекса в целевой БД, для поиска в БД</summary>
        public string IndexNameToSeek => IndexNameToScript.Replace("\"", "");

        /// <summary>Имя индекса в целевой БД, для поиска в БД в like\ilike</summary>
        public string IndexNameToSeekForLike
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.MSSQL:
                        return IndexNameToSeek.Replace("_", "[_]");
                    case Utilities.TargetDBType.EMD:
                    case Utilities.TargetDBType.PGSQL:
                        return IndexNameToSeek.Replace("_", "\\_");
                    default:
                        return IndexNameToSeek;
                }
            }
        }

        /// <summary>Полное имя индекса (в базе-источнике)</summary>
        public string FullIndexName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(IndexName))
                {
                    return ParentTableDB.TableEdit.SchemaName + "." + IndexName;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Полное имя индекса - в оригинальном регистре но без кавычек</summary>
        public string FullIndexNameReady
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(IndexNameReady))
                {
                    return ParentTableDB.TableEdit.SchemaNameReady + "." + IndexNameReady;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Полное имя индекса для сравнения - в нижнем регистре и без кавычек</summary>
        public string FullIndexNameCompare
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(IndexNameCompare))
                {
                    return ParentTableDB.TableEdit.SchemaNameCompare + "." + IndexNameCompare;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Полное имя индекса в целевой БД, для использования в скрипте</summary>
        public string FullIndexNameToScript
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(IndexNameToScript))
                {
                    return ParentTableDB.TableEdit.SchemaNameToScript + "." + IndexNameToScript;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Полное имя индекса в целевой БД, для поиска в БД</summary>
        public string FullIndexNameToSeek
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(IndexNameToSeek))
                {
                    return ParentTableDB.TableEdit.SchemaNameToSeek + "." + IndexNameToSeek;
                }
                else
                {
                    return "";
                }
            }
        }

        string _predicat;
        /// <summary>Предикат индекса</summary>
        public string Predicat
        {
            get
            {
                return _predicat ?? "";
            }
            set
            {
                _predicat = value;
                if (string.IsNullOrWhiteSpace(_predicat)) _predicat = "";
                _predicat = _predicat.Replace("[", string.Empty).Replace("]", string.Empty).Trim();
            }
        }

        /// <summary>Предикат индекса - для использования в скрипте</summary>
        public string PredicatToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                        return Predicat;
                    case Utilities.TargetDBType.PGSQL:
                        return Predicat.ToLower();
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        return Predicat;
                }
            }
        }

        /// <summary>Предикат индекса - для поиска</summary>
        public string PredicatToSeek
        {
            get
            {
                string result = PredicatToScript.ToLower();

                if (result.EndsWith(" desc")) result = result.Substring(0, result.Length - 4);

                result = result
                    .Replace(" desc,", ",")
                    .Replace(" ", "")
                    .Replace("nullsfirst", "")
                    .Replace("nullslast", "")
                    .Replace("\"", "");

                return result;
            }
        }

        /// <summary>Предикат индекса - для поиска в like\ilike</summary>
        public string PredicatToSeekForLike
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.MSSQL:
                        return PredicatToSeek.Replace("_", "[_]");
                    case Utilities.TargetDBType.EMD:
                    case Utilities.TargetDBType.PGSQL:
                        return PredicatToSeek.Replace("_", "\\_");
                    default:
                        return PredicatToSeek;
                }
            }
        }

        string _include;
        /// <summary>include индекса</summary>
        public string Include
        {
            get
            {
                return _include ?? "";
            }
            set
            {
                _include = value;
                if (string.IsNullOrWhiteSpace(_include)) _include = "";
                _include = _include.Replace("[", string.Empty).Replace("]", string.Empty).Trim();
            }
        }

        /// <summary>include индекса - для использования в скрипте</summary>
        public string IncludeToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                        return Include;
                    case Utilities.TargetDBType.PGSQL:
                        return Include.ToLower();
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        return Include;
                }
            }
        }

        string _where;
        /// <summary>условия отбора для индекса</summary>
        public string Where
        {
            get
            {
                return _where ?? "";
            }
            set
            {
                _where = value;
                if (string.IsNullOrWhiteSpace(_where)) _where = "";
                _where = _where.Replace("[", string.Empty).Replace("]", string.Empty).Trim();
            }
        }

        /// <summary>условия отбора для индекса - для использования в скрипте</summary>
        public string WhereToScript(bool isDeleteRegionExclude)
        {
            string result = Where;

            if (isDeleteRegionExclude && (
                result.ToLower().Contains("_deleted") ||
                result.ToLower().Contains("region_id")
                )
            )
            {
                // строим дерево sql-фраз в условии where
                var treePhrases = new Databases.SQLPhraseWhere(result, null, 0, 0);

                // исключаем условия с полями _deleted
                result = treePhrases.AsText(isDeleteRegionExclude);
            }

            return result;
        }

        /// <summary>
        /// в WHERE есть условие с полем _deleted
        /// </summary>
        public bool hasWhereDeleted => Where.ToLower().Contains("_deleted");

        /// <summary>
        /// в WHERE есть условие с полем region_id
        /// </summary>
        public bool hasWhereRegionId => Where.ToLower().Contains("region_id");

        /// <summary>уникальный индекс</summary>
        public bool IsUnique { get; set; }

        /// <summary>IsUnique - в виде текста ("true" или "false")</summary>
        [JsonIgnore]
        public string IsUnique_string
        {
            get
            {
                if (IsUnique == true)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            set
            {
                if (
                    string.IsNullOrWhiteSpace(value) ||
                    (value.Trim().ToLower() != "true")
                )
                {
                    IsUnique = false;
                }
                else
                {
                    IsUnique = true;
                }
            }
        }

        /// <summary>уникальный индекс - для использования в скрипте</summary>
        public string IsUniqueToScript
        {
            get
            {
                if (IsUnique) return "2";
                else return "1";
            }
        }

        string _indextodel;
        /// <summary>Список индексов для удаления</summary>
        public string IndexToDel
        {
            get
            {
                return _indextodel ?? "";
            }
            set
            {
                _indextodel = value;
                if (string.IsNullOrWhiteSpace(_indextodel)) _indextodel = "";
                _indextodel = _indextodel.Replace("\"", string.Empty).Replace("[", string.Empty).Replace("]", string.Empty).Trim();
            }
        }

        /// <summary>Список индексов для удаления - для использования в скрипте</summary>
        public string IndexToDelToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                        return this.IndexToDel.Replace("\"", string.Empty);
                    case Utilities.TargetDBType.PGSQL:
                        return this.IndexToDel.Replace("\"", string.Empty).ToLower();
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        return this.IndexToDel.Replace("\"", string.Empty);
                }
            }
        }

        /// <summary>для рабочих БД</summary>
        public bool IsProd { get; set; }

        /// <summary>IsProd - в виде текста ("true" или "false")</summary>
        [JsonIgnore]
        public string IsProd_string
        {
            get
            {
                if (IsProd == true)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            set
            {
                if (
                    string.IsNullOrWhiteSpace(value) ||
                    (value.Trim().ToLower() != "true")
                )
                {
                    IsProd = false;
                }
                else
                {
                    IsProd = true;
                }
            }
        }

        /// <summary>признак "для рабочих БД" - для использования в скрипте</summary>
        public string IsProdToScript
        {
            get
            {
                if (IsProd) return "2";
                else return "1";
            }
        }

        /// <summary>для реестровых БД</summary>
        public bool IsReg { get; set; }

        /// <summary>IsReg - в виде текста ("true" или "false")</summary>
        [JsonIgnore]
        public string IsReg_string
        {
            get
            {
                if (IsReg == true)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            set
            {
                if (
                    string.IsNullOrWhiteSpace(value) ||
                    (value.Trim().ToLower() != "true")
                )
                {
                    IsReg = false;
                }
                else
                {
                    IsReg = true;
                }
            }
        }

        /// <summary>признак "для реестровых БД" - для использования в скрипте</summary>
        public string IsRegToScript
        {
            get
            {
                if (IsReg) return "2";
                else return "1";
            }
        }

        /// <summary>для отчетных БД</summary>
        public bool IsReport { get; set; }

        /// <summary>IsReport - в виде текста ("true" или "false")</summary>
        [JsonIgnore]
        public string IsReport_string
        {
            get
            {
                if (IsReport == true)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            set
            {
                if (
                    string.IsNullOrWhiteSpace(value) ||
                    (value.Trim().ToLower() != "true")
                )
                {
                    IsReport = false;
                }
                else
                {
                    IsReport = true;
                }
            }
        }

        /// <summary>признак "для отчетных БД" - для использования в скрипте</summary>
        public string IsReportToScript
        {
            get
            {
                if (IsReport) return "2";
                else return "1";
            }
        }

        /// <summary>pAddisDeleted</summary>
        public bool? pAddisDeleted { get; set; }

        /// <summary>pAddisDeleted - в виде текста ("true", "false" или пусто)</summary>
        [JsonIgnore]
        public string pAddisDeleted_string
        {
            get
            {
                if (pAddisDeleted == null)
                {
                    return null;
                }
                else if (pAddisDeleted == true)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    pAddisDeleted = null;
                }
                else if (value.Trim().ToLower() != "true")
                {
                    pAddisDeleted = false;
                }
                else
                {
                    pAddisDeleted = true;
                }
            }
        }

        /// <summary>параметр pAddisDeleted - для использования в скрипте</summary>
        public string pAddisDeletedToScript
        {
            get
            {
                if (pAddisDeleted == null)
                {
                    return null;
                }
                else if (pAddisDeleted == true)
                {
                    return "2";
                }
                else
                {
                    return "1";
                }
            }
        }

        /// <summary>pAddisRegion</summary>
        public bool? pAddisRegion { get; set; }

        /// <summary>pAddisRegion - в виде текста ("true", "false" или пусто)</summary>
        [JsonIgnore]
        public string pAddisRegion_string
        {
            get
            {
                if (pAddisRegion == null)
                {
                    return null;
                }
                else if (pAddisRegion == true)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    pAddisRegion = null;
                }
                else if (value.Trim().ToLower() != "true")
                {
                    pAddisRegion = false;
                }
                else
                {
                    pAddisRegion = true;
                }
            }
        }

        /// <summary>параметр pAddisRegion - для использования в скрипте</summary>
        public string pAddisRegionToScript
        {
            get
            {
                if (pAddisRegion == null)
                {
                    return null;
                }
                else if (pAddisRegion == true)
                {
                    return "2";
                }
                else
                {
                    return "1";
                }
            }
        }

        /// <summary>nulls not distinct для уникальных индексов на ПГ</summary>
        public bool IsNullsNotDistinct { get; set; }

        /// <summary>nulls not distinct - в виде текста ("true" или "false")</summary>
        [JsonIgnore]
        public string IsNullsNotDistinct_string
        {
            get
            {
                if (IsNullsNotDistinct == true)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            set
            {
                if (
                    string.IsNullOrWhiteSpace(value) ||
                    (value.Trim().ToLower() != "true")
                )
                {
                    IsNullsNotDistinct = false;
                }
                else
                {
                    IsNullsNotDistinct = true;
                }
            }
        }

        /// <summary>nulls not distinct - для использования в скрипте</summary>
        public string IsNullsNotDistinctToScript
        {
            get
            {
                if (IsNullsNotDistinct) return "2";
                else return "1";
            }
        }

        /*
        /// <summary>
        /// Заполнить текущий экземпляр IndexDB значениями из другого экземпляра IndexDB
        /// </summary>
        /// <param name="parent">описание таблицы</param>
        /// <param name="_index">описание индекса</param>
        public void Fill(TableDB parent, IndexDB _index)
        {
            if (_index != null)
            {
                this.ParentTableDB = parent;
                this.IndexName = _index.IndexName;
                this.IsUnique = _index.IsUnique;
                this.Predicat = _index.Predicat;
                this.Include = _index.Include;
                this.Where = _index.Where;
                this.IsNullsNotDistinct = _index.IsNullsNotDistinct;
                this.IndexToDel = _index.IndexToDel;
                this.IsProd = _index.IsProd;
                this.IsReg = _index.IsReg;
                this.IsReport = _index.IsReport;
            }
        }
        */

    }

    // =========================================================================================================
    /// <summary>Класс Версия таблицы</summary>
    public class TableInfo
    {

        TableDB _parent;
        /// <summary>Родительская таблица</summary>
        public TableDB ParentTableDB
        {
            get
            {
                return this._parent;
            }
            set
            {
                this._parent = value;
                if (ListField != null)
                {
                    foreach (var row in ListField)
                    {
                        row.ParentTableDB = this._parent;
                    }
                }
            }
        }

        /// <summary>
        /// Конструктор TableInfo - инициализация значений по умолчанию
        /// </summary>
        /// <param name="parent">описание таблицы</param>
        public TableInfo(TableDB parent)
        {
            this.ParentTableDB = parent;
            this.ListField = new List<FieldDB>();
        }

        string _schema_name;
        /// <summary>Имя схемы</summary>
        public string SchemaName
        {
            get
            {
                return _schema_name ?? "";
            }
            set
            {
                _schema_name = value;

                if (string.IsNullOrWhiteSpace(_schema_name))
                {
                    if (this.ParentTableDB.TableType ==  Utilities.TableType.TEMP)
                    {
                        _schema_name = "";
                    }
                    else
                    {
                        _schema_name = "dbo";
                    }
                }

                _schema_name = _schema_name.Trim();
            }
        }

        /// <summary>Имя схемы - в оригинальном регистре, но без кавычек</summary>
        public string SchemaNameReady => SchemaName.Replace("\"", "");

        /// <summary>Имя схемы - для сравнения, в нижнем регистре и без кавычек</summary>
        public string SchemaNameCompare => SchemaNameReady.ToLower();

        /// <summary>Имя схемы - для использования в скрипте</summary>
        public string SchemaNameToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                        return this.SchemaName;
                    case Utilities.TargetDBType.MSSQL:
                        return this.SchemaNameReady;
                    case Utilities.TargetDBType.PGSQL:
                        return this.SchemaNameReady.ToLower();
                    default:
                        return this.SchemaNameReady;
                }
            }
        }

        string _table_name;
        /// <summary>Имя таблицы</summary>
        public string TableName
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

                if (this.ParentTableDB.TableType ==  Utilities.TableType.TEMP)
                {
                    if (this.ParentTableDB.TargetDB == Utilities.TargetDBType.MSSQL)
                    {
                        if (string.IsNullOrWhiteSpace(_table_name))
                        {
                            _table_name = "#tmp";
                        }
                        else
                        {
                            _table_name = "#" + _table_name;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(_table_name))
                        {
                            _table_name = "tmp";
                        }
                    }
                }
            }
        }

        /// <summary>Имя таблицы - в оригинальном регистре, но без кавычек</summary>
        public string TableNameReady => TableName.Replace("\"", "");

        /// <summary>Имя таблицы - для сравнения, в нижнем регистре и без кавычек</summary>
        public string TableNameCompare => TableNameReady.ToLower();

        /// <summary>Имя таблицы - для использования в скрипте</summary>
        public string TableNameToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                        return this.TableName;
                    case Utilities.TargetDBType.MSSQL:
                        return this.TableNameReady;
                    case Utilities.TargetDBType.PGSQL:
                        return this.TableNameReady.ToLower();
                    default:
                        return this.TableNameReady;
                }
            }
        }

        /// <summary>Имя таблицы - для использования в имени файла</summary>
        public string TableNameToFilename
        {
            get
            {
                if (this.ParentTableDB.TableType ==  Utilities.TableType.TEMP)
                {
                    return 
                        this.TableNameReady
                        .Replace(" ", string.Empty)
                        .Replace(".", string.Empty)
                        .ToLower();
                }
                else
                {
                    return (
                        this.SchemaNameReady
                            .Replace(" ", string.Empty)
                            .Replace(".", string.Empty)
                        + " " +
                        this.TableNameReady
                            .Replace(" ", string.Empty)
                            .Replace(".", string.Empty)
                    ).ToLower();
                }
            }
        }

        string _local_name;
        /// <summary>Имя локальной таблицы</summary>
        public string LocalDBListName
        {
            get
            {
                return _local_name ?? "";
            }
            set
            {
                _local_name = value;
                if (string.IsNullOrWhiteSpace(_local_name)) _local_name = "";
                _local_name = _local_name.Trim();
            }
        }

        /// <summary>Имя локальной таблицы - дл поиска в like\ilike</summary>
        public string LocalDBListNameForLike
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.MSSQL:
                        return LocalDBListName.Replace("_", "[_]");
                    case Utilities.TargetDBType.EMD:
                    case Utilities.TargetDBType.PGSQL:
                        return LocalDBListName.Replace("_", "\\_");
                    default:
                        return LocalDBListName;
                }
            }
        }

        /// <summary>Полное имя таблицы (в базе-источнике)</summary>
        public string FullTableName
        {
            get
            {
                if (
                    (this.ParentTableDB.TableType == Utilities.TableType.TEMP) ||
                    string.IsNullOrWhiteSpace(TableName)
                )
                {
                    return TableName;
                }
                else
                {
                    return SchemaName + "." + TableName;
                }
            }
        }

        /// <summary>Полное имя таблицы - в оригинальном регистре но без кавычек</summary>
        public string FullTableNameReady
        {
            get
            {
                if (
                    (this.ParentTableDB.TableType == Utilities.TableType.TEMP) ||
                    string.IsNullOrWhiteSpace(TableNameReady)
                )
                {
                    return TableNameReady;
                }
                else
                {
                    return SchemaNameReady + "." + TableNameReady;
                }
            }
        }

        /// <summary>Полное имя таблицы - для сравнения, в нижнем регистре и без кавычек</summary>
        public string FullTableNameCompare
        {
            get
            {
                if (
                    (this.ParentTableDB.TableType == Utilities.TableType.TEMP) ||
                    string.IsNullOrWhiteSpace(TableNameCompare)
                )
                {
                    return TableNameCompare;
                }
                else
                {
                    return SchemaNameCompare + "." + TableNameCompare;
                }
            }
        }

        /// <summary>Полное имя таблицы в целевой БД, для использования в скрипте</summary>
        public string FullTableNameToScript
        {
            get
            {
                if (
                    (this.ParentTableDB.TableType ==  Utilities.TableType.TEMP) ||
                    string.IsNullOrWhiteSpace(TableNameToScript)
                )
                {
                    return TableNameToScript;
                }
                else
                {
                    return SchemaNameToScript + "." + TableNameToScript;
                }
            }
        }

        /// <summary>Полное имя таблицы в целевой БД, для поиска в БД</summary>
        public string FullTableNameToSeek
        {
            get
            {
                if (
                    (this.ParentTableDB.TableType ==  Utilities.TableType.TEMP) ||
                    string.IsNullOrWhiteSpace(TableNameToSeek)
                )
                {
                    return TableNameToSeek;
                }
                else
                {
                    return SchemaNameToSeek + "." + TableNameToSeek;
                }
            }
        }

        /// <summary>Схема в целевой БД, для поиска в БД</summary>
        public string SchemaNameToSeek => SchemaNameToScript.Replace("\"", "");

        /// <summary>Имя таблицы в целевой БД, для поиска в БД</summary>
        public string TableNameToSeek => TableNameToScript.Replace("\"", "");

        /// <summary>Схема в целевой БД, для поиска в БД с помощью like\ilike</summary>
        public string SchemaNameToSeekForLike
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
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

        /// <summary>Имя таблицы в целевой БД, для поиска в БД с помощью like\ilike</summary>
        public string TableNameToSeekForLike
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
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

        string _parentevntable;
        /// <summary>Родительское событие</summary>
        public string ParentEvnTable
        {
            get
            {
                return _parentevntable ?? "";
            }
            set
            {
                _parentevntable = value;
                if (string.IsNullOrWhiteSpace(_parentevntable)) _parentevntable = "";
                _parentevntable = _parentevntable.Trim();
            }
        }

        /// <summary>Родительское событие в целевой БД - в оригинальном регистре но без кавычек</summary>
        public string ParentEvnTableReady => ParentEvnTable.Replace("\"", "");

        /// <summary>Родительское событие в целевой БД - для сравнения, в нижнем регистре и без кавычек</summary>
        public string ParentEvnTableCompare => ParentEvnTableReady.ToLower();

        /// <summary>Родительское событие - для использования в скрипте</summary>
        public string ParentEvnTableToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                        return this.ParentEvnTable;
                    case Utilities.TargetDBType.MSSQL:
                        return this.ParentEvnTableReady;
                    case Utilities.TargetDBType.PGSQL:
                        return this.ParentEvnTableReady.ToLower();
                    default:
                        return this.ParentEvnTableReady;
                }
            }
        }

        /// <summary>Родительское событие в целевой БД, для поиска в БД</summary>
        public string ParentEvnTableToSeek => ParentEvnTableToScript.Replace("\"", "");

        /// <summary>Полное имя Родительское событие (в базе-источнике)</summary>
        public string FullParentEvnTable
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ParentEvnTable))
                {
                    return SchemaName + "." + ParentEvnTable;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Полное имя Родительское событие - в оригинальном регистре но без кавычек</summary>
        public string FullParentEvnTableReady
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ParentEvnTableReady))
                {
                    return SchemaNameReady + "." + ParentEvnTableReady;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Полное имя Родительское событие в целевой БД, для использования в скрипте</summary>
        public string FullParentEvnTableToScript
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ParentEvnTableToScript))
                {
                    return SchemaNameToScript + "." + ParentEvnTableToScript;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Полное имя Родительское событие в целевой БД, для поиска в БД</summary>
        public string FullParentEvnTableToSeek
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ParentEvnTableToSeek))
                {
                    return SchemaNameToSeek + "." + ParentEvnTableToSeek;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Полное имя представления в целевой БД, для использования в скрипте</summary>
        public string FullViewNameToScript
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TableNameReady))
                {
                    string result = "v_" + TableNameReady;

                    switch (this.ParentTableDB.TargetDB)
                    {
                        case Utilities.TargetDBType.EMD:
                            if (result != result.ToLower())
                            {
                                // заворачиваем в кавычки, только если все наименование НЕ в нижнем регистре
                                result = "\"" + result + "\"";
                            }
                            return SchemaNameToScript + "." + result;
                        case Utilities.TargetDBType.MSSQL:
                            return SchemaNameToScript + "." + result;
                        case Utilities.TargetDBType.PGSQL:
                            return (SchemaNameToScript + "." + result).ToLower();
                        default:
                            return SchemaNameToScript + "." + result;
                    }
                }
                else
                {
                    return "";
                }
            }
        }
        /// <summary>Полное имя процедуры добавления в целевой БД, для использования в скрипте</summary>
        public string FullProcINSToScript
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TableNameReady))
                {
                    string result = "p_" + TableNameReady + "_ins";

                    switch (this.ParentTableDB.TargetDB)
                    {
                        case Utilities.TargetDBType.EMD:
                            if (result != result.ToLower())
                            {
                                // заворачиваем в кавычки, только если все наименование НЕ в нижнем регистре
                                result = "\"" + result + "\"";
                            }
                            return SchemaNameToScript + "." + result;
                        case Utilities.TargetDBType.MSSQL:
                            return SchemaNameToScript + "." + result;
                        case Utilities.TargetDBType.PGSQL:
                            return (SchemaNameToScript + "." + result).ToLower();
                        default:
                            return SchemaNameToScript + "." + result;
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Полное имя процедуры обновления в целевой БД, для использования в скрипте</summary>
        public string FullProcUPDToScript
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TableNameReady))
                {
                    string result = "p_" + TableNameReady + "_upd";

                    switch (this.ParentTableDB.TargetDB)
                    {
                        case Utilities.TargetDBType.EMD:
                            if (result != result.ToLower())
                            {
                                // заворачиваем в кавычки, только если все наименование НЕ в нижнем регистре
                                result = "\"" + result + "\"";
                            }
                            return SchemaNameToScript + "." + result;
                        case Utilities.TargetDBType.MSSQL:
                            return SchemaNameToScript + "." + result;
                        case Utilities.TargetDBType.PGSQL:
                            return (SchemaNameToScript + "." + result).ToLower();
                        default:
                            return SchemaNameToScript + "." + result;
                    }
                }
                else
                {
                    return "";
                }
            }
        }
        /// <summary>Полное имя процедуры удаления в целевой БД, для использования в скрипте</summary>
        public string FullProcDELToScript
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(TableNameReady))
                {
                    string result = "p_" + TableNameReady + "_del";

                    switch (this.ParentTableDB.TargetDB)
                    {
                        case Utilities.TargetDBType.EMD:
                            if (result != result.ToLower())
                            {
                                // заворачиваем в кавычки, только если все наименование НЕ в нижнем регистре
                                result = "\"" + result + "\"";
                            }
                            return SchemaNameToScript + "." + result;
                        case Utilities.TargetDBType.MSSQL:
                            return SchemaNameToScript + "." + result;
                        case Utilities.TargetDBType.PGSQL:
                            return (SchemaNameToScript + "." + result).ToLower();
                        default:
                            return SchemaNameToScript + "." + result;
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        string _table_desc;
        /// <summary>Описание таблицы</summary>
        public string TableDesc
        {
            get
            {
                return _table_desc ?? "";
            }
            set
            {
                _table_desc = value;
                if (string.IsNullOrWhiteSpace(_table_desc)) _table_desc = "";
                _table_desc = _table_desc.Trim();
            }
        }

        string _pkname;
        /// <summary>название Primary Key</summary>
        public string PKName
        {
            get
            {
                return _pkname ?? "";
            }
            set
            {
                _pkname = value;
                if (string.IsNullOrWhiteSpace(_pkname)) _pkname = "";
                _pkname = _pkname.Trim();
            }
        }

        /// <summary>название Primary Key - в оргинальном регистре но без кавычек</summary>
        public string PKNameReady => PKName.Replace("\"", "");

        /// <summary>название Primary Key - для сравнения, в нижнем регистре и без кавычек</summary>
        public string PKNameCompare => PKNameReady.ToLower();

        /// <summary>название Primary Key - для использования в скрипте</summary>
        public string PKNameToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                        return this.PKName;
                    case Utilities.TargetDBType.MSSQL:
                        return this.PKNameReady;
                    case Utilities.TargetDBType.PGSQL:
                        return this.PKNameReady.ToLower();
                    default:
                        return this.PKNameReady;
                }
            }
        }

        /// <summary>название Primary Key - для поиска в БД</summary>
        public string PKNameToSeek => PKNameToScript.Replace("\"", "");
        
        string _foreign_word;
        /// <summary>Слово FOREIGN для добавления в команду</summary>
        public string ForeignWord
        {
            get
            {
                return _foreign_word ?? "";
            }
            set
            {
                _foreign_word = value;
                if (string.IsNullOrWhiteSpace(_foreign_word)) _foreign_word = "";
                _foreign_word = _foreign_word.Trim();
            }
        }

        /// <summary>
        /// =true - это foreign table
        /// </summary>
        public bool isForeignTable => this.ForeignWord == "FOREIGN";

        string _foreign_server;
        /// <summary>Сервер FOREIGN</summary>
        public string ForeignServer
        {
            get
            {
                return _foreign_server ?? "";
            }
            set
            {
                _foreign_server = value;
                if (string.IsNullOrWhiteSpace(_foreign_server)) _foreign_server = "";
                _foreign_server = _foreign_server.Trim();
            }
        }

        string _foreign_options;
        /// <summary>Опции FOREIGN для таблицы</summary>
        public string ForeignOptions
        {
            get
            {
                return _foreign_options ?? "";
            }
            set
            {
                _foreign_options = value;
                if (string.IsNullOrWhiteSpace(_foreign_options)) _foreign_options = "";
                _foreign_options = _foreign_options.Trim();
            }
        }

        /// <summary>
        /// Схема из OPTIONS
        /// </summary>
        public string ForeignSchemaFromOptions
        {
            get
            {
                var list = ForeignOptions.ToList(new char[] { ',' }, true);
                string _schema = "";

                foreach (var item in list)
                {
                    _schema = KeyWord.KeyValue(item.Trim(), "schema_name", new char[] { ' ' })
                        .Replace("'", "");

                    if (!string.IsNullOrWhiteSpace(_schema))
                    {
                        return _schema;
                    }
                }

                string _table = "";

                foreach (var item in list)
                {
                    _table = KeyWord.KeyValue(item.Trim(), "table_name", new char[] { ' ' })
                        .Replace("'", "");

                    if (!string.IsNullOrWhiteSpace(_table))
                    {
                        _schema = Utilities.Databases.GetSchemaName(_table);
                        return _schema;
                    }
                }

                return "";
            }
        }

        /// <summary>
        /// Таблица из OPTIONS
        /// </summary>
        public string ForeignTableFromOptions
        {
            get
            {
                var list = ForeignOptions.ToList(new char[] { ',' }, true);
                string _table = "";

                foreach (var item in list)
                {
                    _table = KeyWord.KeyValue(item.Trim(), "table_name", new char[] { ' ' })
                        .Replace("'", "");

                    if (!string.IsNullOrWhiteSpace(_table))
                    {
                        return Utilities.Databases.GetSchemaName(_table);
                    }
                }

                return "";
            }
        }

        /// <summary>
        /// Опции FOREIGN для таблицы - в скрипт
        /// </summary>
        public string ForeignOptionsToScript
        {
            get
            {
                string _schema = this.ForeignSchemaFromOptions;

                if (_schema == "EMD")
                {
                    return this.ForeignOptions.Replace("\"", string.Empty);
                }
                else
                {
                    return this.ForeignOptions.Replace("\"", string.Empty).ToLower();
                }
            }
        }

        /// <summary>
        /// у таблицы есть комментарий SWAN_RegionalTable
        /// </summary>
        public bool HasRegionDescr { get; set; }

        /// <summary>
        /// у таблицы есть сиквенс
        /// </summary>
        public bool HasSequence { get; set; }

        /// <summary>
        /// у таблицы есть запись в pg_inherits, т.е. таблица создана через наследование
        /// </summary>
        public bool HasInherit { get; set; }

        string _partition_function_name;
        /// <summary>Функция секционирования (MSSQL)</summary>
        public string PartitionFunctionName
        {
            get
            {
                return _partition_function_name ?? "";
            }
            set
            {
                _partition_function_name = value;
                if (string.IsNullOrWhiteSpace(_partition_function_name)) _partition_function_name = "";
                _partition_function_name = _partition_function_name.Trim();
            }
        }

        string _partition_field_type;
        /// <summary>Тип поля секционирования</summary>
        public string PartitionFieldType
        {
            get
            {
                return _partition_field_type ?? "";
            }
            set
            {
                _partition_field_type = value;
                if (string.IsNullOrWhiteSpace(_partition_field_type)) _partition_field_type = "";
                _partition_field_type = _partition_field_type.Trim();
            }
        }

        string _partition_field_size;
        /// <summary>Точность типа поля секционирования</summary>
        public string PartitionFieldSize
        {
            get
            {
                return _partition_field_size ?? "";
            }
            set
            {
                _partition_field_size = value;
                if (string.IsNullOrWhiteSpace(_partition_field_size)) _partition_field_size = "";
                _partition_field_size = _partition_field_size.Trim();
            }
        }

        string _partition_field_dec;
        /// <summary>Масштаб типа поля секционирования</summary>
        public string PartitionFieldDec
        {
            get
            {
                return _partition_field_dec ?? "";
            }
            set
            {
                _partition_field_dec = value;
                if (string.IsNullOrWhiteSpace(_partition_field_dec)) _partition_field_dec = "";
                _partition_field_dec = _partition_field_dec.Trim();
            }
        }

        string _partition_type;
        /// <summary>Тип секционирования</summary>
        public string PartitionType
        {
            get
            {
                return _partition_type ?? "";
            }
            set
            {
                _partition_type = value;
                if (string.IsNullOrWhiteSpace(_partition_type)) _partition_type = "";
                _partition_type = _partition_type.Trim();
            }
        }

        string _partition_boundary;
        /// <summary>Выравнивание границ диапазона (MSSQL)</summary>
        public string PartitionBoundary
        {
            get
            {
                return _partition_boundary ?? "";
            }
            set
            {
                _partition_boundary = value;
                if (string.IsNullOrWhiteSpace(_partition_boundary)) _partition_boundary = "";
                _partition_boundary = _partition_boundary.Trim();
            }
        }

        string _partition_range_values;
        /// <summary>Диапазоны значений</summary>
        public string PartitionRangeValues
        {
            get
            {
                return _partition_range_values ?? "";
            }
            set
            {
                _partition_range_values = value;
                if (string.IsNullOrWhiteSpace(_partition_range_values)) _partition_range_values = "";
                _partition_range_values = _partition_range_values.Trim();
            }
        }

        string _partition_scheme_name;
        /// <summary>Схема секционирования (MSSQL)</summary>
        public string PartitionSchemeName
        {
            get
            {
                return _partition_scheme_name ?? "";
            }
            set
            {
                _partition_scheme_name = value;
                if (string.IsNullOrWhiteSpace(_partition_scheme_name)) _partition_scheme_name = "";
                _partition_scheme_name = _partition_scheme_name.Trim();
            }
        }

        string _partition_field;
        /// <summary>Поле секционирования</summary>
        public string PartitionField
        {
            get
            {
                return _partition_field ?? "";
            }
            set
            {
                _partition_field = value;
                if (string.IsNullOrWhiteSpace(_partition_field)) _partition_field = "";
                _partition_field = _partition_field.Trim();
            }
        }

        /// <summary>
        /// =true - это таблица с секционированием
        /// </summary>
        public bool isPartitionTable => !string.IsNullOrWhiteSpace(this.PartitionField);

        /// <summary>Полный список полей, в т.ч. унаследованных от родительской таблице</summary>
        public List<FieldDB> ListField { get; set; }

        /// <summary>
        /// Фильтрованный рабочий список полей для генерации скриптов
        /// </summary>
        /// <param name="_isUsed">=true - вернуть только выбранные поля, =null - вернуть все поля, =false - вернуть не выбранные поля</param>
        /// <param name="_isInherit">=false - вернуть без унаследованных полей, =null - вернуть все поля, =true - вернуть только унаследованные поля </param>
        /// <returns></returns>
        public List<FieldDB> ListFilteredField (bool? _isUsed = true, bool? _isInherit = false)
        {
            return ListField
                    .Where(x =>
                    {
                        if (_isUsed != null && _isUsed != x.IsUsed) return false;
                        if (_isInherit != null && _isInherit != x.IsInherit) return false;
                        return true;
                    }).ToList();
        }

        /// <summary>
        /// список FK таблицы
        /// </summary>
        public List<TableFKInfo> ListFKs
        {
            get
            {
                var result = new List<TableFKInfo>();

                foreach (FieldDB row in this.ListFilteredField(true, null)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.FieldName) &&
                        !string.IsNullOrWhiteSpace(x.FKTable) &&
                        !string.IsNullOrWhiteSpace(x.FKName) &&
                        !string.IsNullOrWhiteSpace(x.FKField)
                    )
                    .OrderBy(x => x.FieldOrder)
                )
                {
                    result.Add(new TableFKInfo()
                    {
                        FieldName = row.FieldNameReady,
                        FKTable = row.FKFullTableNameReady,
                        FKName = row.FKNameReady,
                        FKField = row.FKFieldReady,
                        FKOrder = row.FKOrder
                    });
                }

                return result;
            }
        }

        /// <summary>
        /// список CHECK таблицы
        /// </summary>
        public List<TableCHECKInfo> ListCHECKs
        {
            get
            {
                var result = new List<TableCHECKInfo>();

                foreach (FieldDB row in this.ListFilteredField(true, null)
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.FieldName) &&
                        !string.IsNullOrWhiteSpace(x.FKName) &&
                        !string.IsNullOrWhiteSpace(x.FieldCheck)
                    )
                    .OrderBy(x => x.FieldOrder)
                )
                {
                    result.Add(new TableCHECKInfo()
                    {
                        FieldName = row.FieldNameReady,
                        FKName = row.FKNameReady,
                        FieldCheck = row.FieldCheck
                    });
                }

                return result;
            }
        }

        /*
        /// <summary>
        /// Инициализировать текущий экземпляр TableInfo значениями из другого экземпляра TableInfo
        /// </summary>
        /// <param name="parent">экземпляр-назачение TableDB</param>
        /// <param name="_table">экземпляр-источник TableInfo</param>
        public void Fill(TableDB parent, TableInfo _table)
        {
            if (_table != null)
            {
                this.ParentTableDB = parent;
                this.SchemaName = _table.SchemaName;
                this.TableName = _table.TableName;
                this.ParentEvnTable = _table.ParentEvnTable;
                this.TableDesc = _table.TableDesc;
                this.PKName = _table.PKName;
                this.ForeignTable = _table.ForeignTable;
                this.ForeignServer = _table.ForeignServer;
                this.ForeignOptions = _table.ForeignOptions;
                this.HasRegionDescr = _table.HasRegionDescr;
                this.HasSequence = _table.HasSequence;
                this.HasInherit = _table.HasInherit;

                if (_table.ListField != null)
                {
                    foreach (var item in _table.ListField
                        .OrderBy(x => x.FieldOrder)
                        )
                    {
                        this.AddField(item.FieldOrder_string, item.FieldName, item.FieldType, item.FieldSize, item.FieldDec, item.FieldDesc, item.IsNotNull_string, item.IsIdentity_string, item.IsPK_string, item.PKOrder, item.FieldDefault, item.FKName, item.FKTable, item.FKField, item.FKOrder, item.ForeignColumn, item.FieldCheck, item.IsInherit_string, item.InheritParentTable, item.IsUsed);
                    }
                }
            }
        }
        */

        /// <summary>
        /// Клонирование текущего экземпляра TableInfo
        /// </summary>
        /// <returns></returns>
        public TableInfo Copy()
        {
            TableInfo copy = (TableInfo)this.MemberwiseClone();

            copy.ListField = this.ListField.Select(item => new FieldDB(this.ParentTableDB, this, item)).ToList();

            return copy;
        }

        /// <summary>
        /// Переименование таблицы
        /// </summary>
        /// <param name="newName">новое имя таблицы</param>
        public void RenameTable(string newName)
        {
            string oldName = this.TableNameCompare;
            newName = newName.Trim();
            this.TableName = newName;
            string find_s = "";

            int pos = -1;
            int len = 0;

            find_s = "_" + oldName + "_";
            len = find_s.Length;
            pos = this.PKNameCompare.IndexOf(find_s);
            if (pos > -1) this.PKName = this.PKNameReady.Remove(pos, len).Insert(pos, "_" + this.TableNameReady + "_");

            foreach (var row in this.ListField)
            {
                find_s = oldName + "_";
                len = find_s.Length;
                if (row.FieldNameCompare.StartsWith(find_s))
                    row.FieldName = row.FieldNameReady.Remove(0, len).Insert(0, this.TableNameReady + "_");

                find_s = "_" + oldName + "_";
                len = find_s.Length;
                pos = row.FKNameCompare.IndexOf(find_s);
                if (pos > -1) row.FKName = row.FKNameReady.Remove(pos, len).Insert(pos, "_" + this.TableNameReady + "_");

                // второй раз, если в названии повторяется имя таблицы
                pos = row.FKNameCompare.IndexOf(find_s);
                if (pos > -1) row.FKName = row.FKNameReady.Remove(pos, len).Insert(pos, "_" + this.TableNameReady + "_");
            }

        }

        /// <summary>
        /// Найти поле по имени
        /// </summary>
        /// <param name="name">имя поля</param>
        /// <param name="_isUsed">=true - ищем среди выбранных полей, =null - ищем среди всех полей, =false - ищем среди не выбранных полей</param>
        /// <param name="_isInherit">=false - ищем среди без унаследованных полей, =null - ищем среди среди всех полей, =true - ищем среди только унаследованных полей</param>
        /// <returns></returns>
        public FieldDB FindFieldByName(string name, bool? _isUsed, bool? _isInherit)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            name = name.Replace("\"", "").ToLower();

            return this.ListFilteredField(_isUsed, _isInherit)
                .Find(x => x.FieldNameCompare == name);
        }

        // Найти поле по Id
        /*        public FieldDB FindFieldById(int Id)
                {
                    return this.ListField.Find(x => x.FieldId == Id);
                }*/

        /// <summary>
        /// Добавить поле в список
        /// </summary>
        /// <param name="FieldOrder">номер поля по порядку</param>
        /// <param name="FieldName">имя поля</param>
        /// <param name="FieldType">тип поля</param>
        /// <param name="FieldSize">размер поля</param>
        /// <param name="FieldDec">кол-во разрядов после десятичного знака</param>
        /// <param name="FieldDesc">описание поля</param>
        /// <param name="IsNotNull">=true - NOT NULL</param>
        /// <param name="IsIdentity">=true - поле с автогенерацией значений (identity)</param>
        /// <param name="IsPK">=true - поле входит в primary key</param>
        /// <param name="PKOrder">N п\п в primary key</param>
        /// <param name="FieldDefault">значение по умолчанию</param>
        /// <param name="FKName">имя констрейна</param>
        /// <param name="FKTable">таблица констрейна</param>
        /// <param name="FKField">primary key таблицы констрейна</param>
        /// <param name="FKOrder">N п\п в primary key таблицы констрейна</param>
        /// <param name="ForeignColumn">название поля внешней таблицы (FOREIGN TABLE)</param>
        /// <param name="FieldCheck">условие для CHECK</param>
        /// <param name="IsInherit">=true - поле унаследовано от родительской таблицы</param>
        /// <param name="InheritParentTable">родительская таблица, от которой унаследовано поле</param>
        /// <param name="IsUsed">=true - поле выбрано для генерации скрипта</param>
        public void AddField(string FieldOrder, string FieldName, string FieldType, string FieldSize = "", string FieldDec = "", string FieldDesc = "",string IsNotNull = "false", string IsIdentity = "false", string IsPK = "false", string PKOrder = "", string FieldDefault = "", string FKName = "", string FKTable = "", string FKField = "", string FKOrder = "", string ForeignColumn = "", string FieldCheck = "", string IsInherit = "false", string InheritParentTable = "", bool IsUsed = true)
        {
            FieldDB newField = new FieldDB(this.ParentTableDB, this);

            if (string.IsNullOrWhiteSpace(FieldOrder)) FieldOrder = "0";
            if (string.IsNullOrWhiteSpace(IsNotNull)) IsNotNull = "false";
            if (string.IsNullOrWhiteSpace(IsIdentity)) IsIdentity = "false";
            if (string.IsNullOrWhiteSpace(IsPK)) IsPK = "false";
            if (string.IsNullOrWhiteSpace(IsInherit)) IsInherit = "false";

            /*
            if ((this.ParentTableDB.ScriptType == Utilities.ScriptType.CREATE) && (IsNotNull == "false") && FieldName.ToLower().EndsWith("_deleted"))
            {
               IsNotNull = "true";
            }
            */
            newField.FieldOrder_string = FieldOrder;
            newField.FieldName = FieldName;
            newField.FieldDesc = FieldDesc;
            newField.FieldType = FieldType.ToUpper();
            newField.FieldSize = FieldSize;
            newField.FieldDec = FieldDec;
            newField.IsNotNull_string = IsNotNull;
            newField.IsIdentity_string = IsIdentity;
            newField.IsPK_string = IsPK;
            newField.FieldDefault = FieldDefault;
            newField.FKName = FKName;
            newField.FKTable = FKTable;
            newField.FKField = FKField;
            newField.ForeignColumn = ForeignColumn;
            newField.FieldCheck = FieldCheck;
            newField.PKOrder = PKOrder;
            newField.FKOrder = FKOrder;
            newField.IsInherit_string = IsInherit;
            newField.InheritParentTable = InheritParentTable;
            newField.IsUsed = IsUsed;

            FieldDB existField = this.ListField
                .Find(x => x.FieldNameCompare == newField.FieldNameCompare); 

            if (existField == null)
            {
                //Номер поля по порядку
                if (newField.FieldOrder == 0)
                {
                    int order = 0;

                    foreach (FieldDB row in this.ListField)
                    {
                        if (row.FieldOrder > order)
                        {
                            order = row.FieldOrder;
                        }
                    }

                    newField.FieldOrder = order + 1;
                }

                //Номер поля в PK по порядку
                if (
                    newField.IsPK && 
                    string.IsNullOrWhiteSpace(newField.PKOrder)
                )
                {
                    int order = 0;

                    foreach (FieldDB row in this.ListField.Where(x => x.IsPK))
                    {
                        if (row.PKOrderToInt > order)
                        {
                            order = row.PKOrderToInt;
                        }
                        else
                        {
                            order++;
                        }
                    }

                    newField.PKOrder = (order + 1).ToString();
                }

                //Номер поля в FK по порядку
                if (
                    !string.IsNullOrWhiteSpace(newField.FKName) &&
                    !string.IsNullOrWhiteSpace(newField.FKTable) &&
                    string.IsNullOrWhiteSpace(newField.FKOrder)
                )
                {
                    int order = 0;

                    foreach (FieldDB row in this.ListField
                        .Where(x => 
                            x.FKNameCompare == newField.FKNameCompare &&
                            !string.IsNullOrWhiteSpace(x.FKName)
                        )
                    )
                    {
                        if (row.FKOrderToInt > order)
                        {
                            order = row.FKOrderToInt;
                        }
                        else
                        {
                            order++;
                        }
                    }

                    newField.FKOrder = (order + 1).ToString();
                }

                this.ListField.Add(newField);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(existField.FieldDesc)) existField.FieldDesc = newField.FieldDesc;
                if (string.IsNullOrWhiteSpace(existField.FieldType)) existField.FieldType = newField.FieldType;
                if (string.IsNullOrWhiteSpace(existField.FieldSize)) existField.FieldSize = newField.FieldSize;
                if (string.IsNullOrWhiteSpace(existField.FieldDec)) existField.FieldDec = newField.FieldDec;
                if (existField.IsNotNull == false) existField.IsNotNull = newField.IsNotNull;
                if (existField.IsIdentity == false) existField.IsIdentity = newField.IsIdentity;
                if (existField.IsPK == false) existField.IsPK = newField.IsPK;
                if (string.IsNullOrWhiteSpace(existField.FieldDefault)) existField.FieldDefault = newField.FieldDefault;
                if (string.IsNullOrWhiteSpace(existField.FKName)) existField.FKName = newField.FKName;
                if (string.IsNullOrWhiteSpace(existField.FKTable)) existField.FKTable = newField.FKTable;
                if (string.IsNullOrWhiteSpace(existField.FKField)) existField.FKField = newField.FKField;
                if (string.IsNullOrWhiteSpace(existField.ForeignColumn)) existField.ForeignColumn = newField.ForeignColumn;
                if (string.IsNullOrWhiteSpace(existField.FieldCheck)) existField.FieldCheck = newField.FieldCheck;
                if (existField.IsInherit == false) existField.IsInherit = newField.IsInherit;
                if (string.IsNullOrWhiteSpace(existField.InheritParentTable)) existField.InheritParentTable = newField.InheritParentTable;
            }
        }

        /// <summary>
        /// Скрипт создания таблицы
        /// </summary>
        /// <param name="fields">список полей</param>
        /// <param name="tableinfo">описание иаблицы</param>
        /// <returns></returns>
        public static string CreateTableToScript(string fields, TableInfo tableinfo)
        {
            //if (fields == "") return "";

            string res = "";

            switch (tableinfo.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        if (tableinfo.ParentTableDB.TableType ==  Utilities.TableType.TEMP)
                        {
                            res = /*"DROP TABLE IF EXISTS " + this.FullTableNameToScript + Environment.NewLine +*/
                                "CREATE TABLE " + tableinfo.FullTableNameToScript + " (" + 
                                fields + Environment.NewLine + 
                                ")" + Environment.NewLine;

                            return res;                        }
                        else
                        {
                            res = "IF OBJECT_ID(N'" + tableinfo.FullTableNameToScript + "', 'U') IS NULL" + Environment.NewLine +
                                "BEGIN" + Environment.NewLine;

                            if (tableinfo.isPartitionTable)
                            {
                                res += Environment.NewLine + $"IF NOT EXISTS (SELECT 1 FROM sys.partition_functions with (nolock) WHERE name = '{tableinfo.PartitionFunctionName}')" + Environment.NewLine +
                                    $"CREATE PARTITION FUNCTION {tableinfo.PartitionFunctionName} ({tableinfo.PartitionFieldType}) AS {tableinfo.PartitionType} {tableinfo.PartitionBoundary}" + Environment.NewLine +
                                    $"FOR VALUES (" + Environment.NewLine +
                                    $"\t{tableinfo.PartitionRangeValues}" + Environment.NewLine +
                                    $")" + Environment.NewLine +
                                    Environment.NewLine +
                                    $"IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes with (nolock) WHERE name = '{tableinfo.PartitionSchemeName}')" + Environment.NewLine +
                                    $"CREATE PARTITION SCHEME {tableinfo.PartitionSchemeName}" + Environment.NewLine +
                                    $"AS PARTITION {tableinfo.PartitionFunctionName}" + Environment.NewLine +
                                    "ALL TO([PRIMARY])" + Environment.NewLine +
                                    Environment.NewLine;
                            }

                            res += $"CREATE TABLE {tableinfo.FullTableNameToScript} (" +
                                fields + Environment.NewLine +
                                ")" + Environment.NewLine;

                            if (tableinfo.isPartitionTable)
                            {
                                res += $"ON {tableinfo.PartitionSchemeName} ({tableinfo.PartitionField})" + Environment.NewLine +
                                    Environment.NewLine +
                                    $"ALTER TABLE {tableinfo.FullTableNameToScript} SET (LOCK_ESCALATION = AUTO)" + Environment.NewLine
                                     + Environment.NewLine;
                            }

                            res += "END" + Environment.NewLine;

                            return res;
                        }
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        if (tableinfo.ParentTableDB.TableType ==  Utilities.TableType.TEMP)
                        {
                            res = /*"DROP TABLE IF EXISTS " + tableinfo.FullTableNameToScript + ";" + Environment.NewLine + */
                            "CREATE TEMP TABLE IF NOT EXISTS " + tableinfo.FullTableNameToScript + " (" + fields + Environment.NewLine + ") ON COMMIT DROP;" + Environment.NewLine;
                        }
                        else
                        {
                            res = "";

                            string inherit = "";
                            if (
                                (tableinfo.ParentTableDB.TableType ==  Utilities.TableType.EVN) && 
                                (!string.IsNullOrWhiteSpace(tableinfo.ParentEvnTable)) && 
                                (tableinfo.HasInherit || GITProjects.GetisEvnInheritByProject(tableinfo.ParentTableDB.GITProject) )
                            )
                            {
                                // если это таблица создается через наследование
                                inherit = " INHERITS (" + tableinfo.FullParentEvnTableToScript + ")";
                            }

                            string foreign = " ";
                            string options = "WITH (oids = false);";

                            if (tableinfo.isForeignTable)
                            {
                                // если это внешняя таблица
                                foreign = " " + tableinfo.ForeignWord + " ";
                                options = "SERVER " + tableinfo.ForeignServer + Environment.NewLine +
                                    "OPTIONS (" + tableinfo.ForeignOptionsToScript + ");";

                                if (!tableinfo.ParentTableDB.isAddRegion)
                                {
                                    res += $"SELECT FROM dbo.xp_gen_view('{tableinfo.FullTableNameToScript}', $table$" + Environment.NewLine;
                                }
                                else
                                {
                                    res += $"PERFORM dbo.xp_gen_view('{tableinfo.FullTableNameToScript}', $table$" + Environment.NewLine;
                                }
                            }

                            string partition = "";
                            if (tableinfo.isPartitionTable)
                            {
                                partition = $"PARTITION BY {tableinfo.PartitionType} ({tableinfo.PartitionField})" + Environment.NewLine;
                            }

                            res += "CREATE" + foreign + "TABLE IF NOT EXISTS " + tableinfo.FullTableNameToScript + " (" + fields + Environment.NewLine + ")" + inherit + Environment.NewLine +
                                partition +
                                options + Environment.NewLine;

                            if (tableinfo.isForeignTable)
                            {
                                res += "$table$, 2);" + Environment.NewLine;
                            }
                        }

                        return res;
                    }
                default:
                    return res + ";";
            }
        }

        /// <summary>
        /// Скрипт изменения поля в таблице (таблица уже существует)
        /// </summary>
        /// <param name="oldRow">предыдущее описаие поля</param>
        /// <param name="newRow">новое описание поля</param>
        /// <param name="ScriptAlterList">список команд, которые надо выполнить общей пачкой, обернув в xp_gen_view или p_AlterTypeColumn</param>
        /// <param name="ScriptColumnList">список полей для p_AlterTypeColumn</param>
        /// <returns></returns>
        public static string AlterFieldToScript(FieldDB oldRow, FieldDB newRow, ref string ScriptAlterList, ref List<string> ScriptColumnList)
        {
            string ScriptRow = "";

            if (oldRow == null) return "";
            if (newRow == null) return "";

            bool isChanged_Row = false;
            bool isChanged_List = false;
            bool use_p_AlterTypeColumn = newRow.ParentTableDB.isPromed;

            bool use_NewFunc = newRow.ParentTableDB.isPromed && MainWindow.APPinfo.isUseNewFunc;

            //для foreign table - NOT NULL в команде 
            if (newRow.ParentTableInfo.isForeignTable)
            {
                use_NewFunc = false;
            }

            switch (newRow.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        bool isChanged = false;

                        // Сначала меняем тип поля, оставляя NULL\NOT NULL без изменения
                        string cmd = "ALTER TABLE " + newRow.ParentTableInfo.FullTableNameToScript + " ALTER COLUMN " + newRow.FieldNameToScript;

                        if (oldRow.FieldTypeNotEquals(newRow)) //тип изменился
                        {
                            cmd += " " + newRow.FullFieldTypeToScript;

                            if (oldRow.IsNotNull == true)
                                cmd += " " + oldRow.IsNotNullToScript;
                            else
                                cmd += " " + oldRow.IsNullToScript;

                            isChanged = true;
                        }

                        if (oldRow.IsIdentity != newRow.IsIdentity)
                        {
                            if (newRow.IsIdentity == true)
                            {
                                cmd += " " + newRow.IsIdentityToScript;
                                isChanged = true;
                            }
                        }

                        if (isChanged)
                        {
                            cmd = 
                                "IF EXISTS (" + Environment.NewLine +
                                "\tSELECT TOP(1) 1" + Environment.NewLine +
                                "\tFROM information_schema.columns WITH (nolock)" + Environment.NewLine +
                                "\tWHERE table_schema = N'" + newRow.ParentTableInfo.SchemaNameToSeek + "'" + Environment.NewLine +
                                "\tAND table_name = N'" + newRow.ParentTableInfo.TableNameToSeek + "'" + Environment.NewLine +
                                "\tAND column_name = N'" + newRow.FieldNameToSeek + "'" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "BEGIN" + Environment.NewLine +
                                "\t" + cmd + Environment.NewLine +
                                "END";

                            if (use_p_AlterTypeColumn)
                            {
                                // используем p_AlterTypeColumn
                                /*
                                ScriptRow +=
                                    "IF EXISTS (" + Environment.NewLine +
                                    "\tSELECT TOP(1) 1" + Environment.NewLine +
                                    "\tFROM information_schema.columns WITH (nolock)" + Environment.NewLine +
                                    "\tWHERE table_schema = N'" + this.SchemaNameToSeek + "'" + Environment.NewLine +
                                    "\tAND table_name = N'" + this.TableNameToSeek + "'" + Environment.NewLine +
                                    "\tAND column_name = N'" + newrow.FieldNameToSeek + "'" + Environment.NewLine +
                                    ")" + Environment.NewLine +
                                    "BEGIN" + Environment.NewLine +
                                    "\tEXEC dbo.p_AlterTypeColumn" + Environment.NewLine +
                                    "\t\t@Task = '" + MainWindow.Task.TaskNumber + "'," + Environment.NewLine +
                                    "\t\t@SchemaName = '" + this.SchemaNameToScript + "'," + Environment.NewLine +
                                    "\t\t@TableName = '" + this.TableNameToScript + "'," + Environment.NewLine +
                                    "\t\t@ColumnName = '" + newrow.FieldNameToScript + "'," + Environment.NewLine +
                                    "\t\t@cmd = '" + cmd + "'" + Environment.NewLine +
                                    "END" + Environment.NewLine +
                                    Environment.NewLine;
                                */
                                ScriptAlterList += Environment.NewLine + Environment.NewLine + cmd;

                                if (!ScriptColumnList.Contains(newRow.FieldNameToScript))
                                {
                                    ScriptColumnList.Add(newRow.FieldNameToScript);
                                }
    ;
                                isChanged_List = true;
                            }
                            else
                            {
                                // генерим скрипт без использования p_AlterTypeColumn
                                ScriptRow += Environment.NewLine + Environment.NewLine + 
                                    $"-- Alter column {newRow.FieldNameToScript}" + Environment.NewLine +
                                    cmd;

                                isChanged_Row = true;
                            }
                        }

                        // теперь меняем NULL\NOT NULL
                        if (oldRow.IsNotNull != newRow.IsNotNull)
                        {
                            if (use_NewFunc && newRow.IsNotNull)
                            {
                                // используем p_SetNotNull
                                ScriptRow += Environment.NewLine + Environment.NewLine +
                                    "EXEC dbo.p_SetNotNull " +
                                    "@TableName = '" + newRow.ParentTableInfo.FullTableNameToScript + "', " +
                                    "@ColumnName = '" + newRow.FieldNameToScript + "', " +
                                    "@Task = '" + MainWindow.Task.TaskNumber + "', " +
                                    "@Script = ''";

                                isChanged_Row = true;
                            }
                            else
                            {
                                // генерим скрипт без использования p_SetNotNull
                                string info;
                                if (newRow.IsNotNull == true)
                                {
                                    info = $"-- {newRow.FieldNameToScript} set NOT NULL";
                                }
                                else
                                {
                                    info = $"-- {newRow.FieldNameToScript} drop NOT NULL";
                                }

                                cmd = "ALTER TABLE " + newRow.ParentTableInfo.FullTableNameToScript + " ALTER COLUMN " + newRow.FieldNameToScript;
                                cmd += " " + newRow.FullFieldTypeToScript;
                                if (newRow.IsNotNull == true)
                                    cmd += " " + newRow.IsNotNullToScript;
                                else
                                    cmd += " " + newRow.IsNullToScript;

                                info += Environment.NewLine +
                                        "IF EXISTS (" + Environment.NewLine +
                                        "\tSELECT TOP(1) 1" + Environment.NewLine +
                                        "\tFROM information_schema.columns WITH (nolock)" + Environment.NewLine +
                                        "\tWHERE table_schema = N'" + newRow.ParentTableInfo.SchemaNameToSeek + "'" + Environment.NewLine +
                                        "\tAND table_name = N'" + newRow.ParentTableInfo.TableNameToSeek + "'" + Environment.NewLine +
                                        "\tAND column_name = N'" + newRow.FieldNameToSeek + "'" + Environment.NewLine;

                                if (newRow.IsNotNull == true)
                                {
                                    info +=
                                        "\tAND is_nullable = 'YES'" + Environment.NewLine;
                                }
                                else
                                {
                                    info +=
                                        "\tAND is_nullable = 'NO'" + Environment.NewLine;
                                }

                                info +=
                                        ")" + Environment.NewLine;

                                cmd = info +
                                    "BEGIN" + Environment.NewLine +
                                    "\t" + cmd + Environment.NewLine +
                                    "END";

                                if (use_p_AlterTypeColumn)
                                {
                                    // используем p_AlterTypeColumn
                                    /*
                                    ScriptRow +=
                                        "BEGIN" + Environment.NewLine +
                                        "\tEXEC dbo.p_AlterTypeColumn" + Environment.NewLine +
                                        "\t\t@Task = '" + MainWindow.Task.TaskNumber + "'," + Environment.NewLine +
                                        "\t\t@SchemaName = '" + this.SchemaNameToScript + "'," + Environment.NewLine +
                                        "\t\t@TableName = '" + this.TableNameToScript + "'," + Environment.NewLine +
                                        "\t\t@ColumnName = '" + newrow.FieldNameToScript + "'," + Environment.NewLine +
                                        "\t\t@cmd = '" + cmd + "'" + Environment.NewLine +
                                        "END" + Environment.NewLine +
                                        Environment.NewLine;
                                    */

                                    ScriptAlterList += Environment.NewLine + Environment.NewLine + cmd;

                                    if (!ScriptColumnList.Contains(newRow.FieldNameToScript))
                                    {
                                        ScriptColumnList.Add(newRow.FieldNameToScript);
                                    }
    ;
                                    isChanged_List = true;
                                }
                                else
                                {
                                    // генерим скрипт без использования p_AlterTypeColumn
                                    ScriptRow += Environment.NewLine + Environment.NewLine + cmd;

                                    isChanged_Row = true;
                                }
                            }
                        }

                        // меняем констрейн DEFAULT
                        if (oldRow.FieldDefault != newRow.FieldDefault)
                        {
                            if (
                                (!string.IsNullOrWhiteSpace(oldRow.FieldDefault)) ||
                                string.IsNullOrWhiteSpace(newRow.FieldDefault)
                            )
                            {
                                ScriptRow += Environment.NewLine + Environment.NewLine +
                                    $"-- {newRow.FieldNameToScript} drop DEFAULT" + Environment.NewLine +
                                    "DECLARE @defsql NVARCHAR(max) = (" + Environment.NewLine +
                                    "\tSELECT TOP(1) def.name" + Environment.NewLine +
                                    "\tFROM sys.tables t with (nolock)" + Environment.NewLine +
                                    "\tINNER JOIN sys.columns c with (nolock) ON c.object_id = t.object_id" + Environment.NewLine +
                                    "\tINNER JOIN sys.default_constraints def with (nolock) ON def.parent_object_id = t.object_id and def.parent_column_id = c.column_id" + Environment.NewLine +
                                    "\tWHERE t.object_id = OBJECT_ID(N'" + newRow.ParentTableInfo.FullTableNameToScript + @"', 'U')" + Environment.NewLine +
                                    "\tAND c.name = N'" + newRow.FieldNameToSeek + "'" + Environment.NewLine +
                                    ")" + Environment.NewLine +
                                    "IF COALESCE(@defsql, '') <> ''" + Environment.NewLine +
                                    "BEGIN" + Environment.NewLine +
                                    "\tSET @defsql = 'ALTER TABLE " + newRow.ParentTableInfo.FullTableNameToScript + " DROP CONSTRAINT ' + @defsql" + Environment.NewLine +
                                    "\tEXECUTE sp_executesql @defsql" + Environment.NewLine +
                                    "END";

                                isChanged_Row = true;
                            }

                            if (!string.IsNullOrWhiteSpace(newRow.FieldDefault))
                            {
                                ScriptRow += Environment.NewLine + Environment.NewLine +
                                    $"-- {newRow.FieldNameToScript} set DEFAULT" + Environment.NewLine +
                                    "IF NOT EXISTS (" + Environment.NewLine +
                                    "\tSELECT TOP(1) 1" + Environment.NewLine +
                                    "\tFROM sys.tables t with (nolock)" + Environment.NewLine +
                                    "\tINNER JOIN sys.columns c with (nolock) ON c.object_id = t.object_id" + Environment.NewLine +
                                    "\tINNER JOIN sys.default_constraints def with (nolock) ON def.parent_object_id = t.object_id and def.parent_column_id = c.column_id" + Environment.NewLine +
                                    "\tWHERE t.object_id = OBJECT_ID(N'" + newRow.ParentTableInfo.FullTableNameToScript + @"', 'U')" + Environment.NewLine +
                                    "\tAND c.name = N'" + newRow.FieldNameToSeek + "'" + Environment.NewLine +
                                    ")" + Environment.NewLine +
                                    "BEGIN" + Environment.NewLine +
                                    "\tALTER TABLE " + newRow.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                    "\t\tADD CONSTRAINT DF_" + newRow.FieldNameToScript + " " + newRow.FieldDefaultToScript + Environment.NewLine +
                                    "\t\tFOR " + newRow.FieldNameToScript + Environment.NewLine +
                                    "END";

                                isChanged_Row = true;
                            }
                        }
                    }
                    break;

                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        bool isAddDO_Row = false;
                        bool isAddDO_List = false;
                        string ScriptRowToList = "";

                        string foreign = " ";
                        if (newRow.ParentTableInfo.isForeignTable)
                        {
                            // если это внешняя таблица
                            foreign = " " + newRow.ParentTableInfo.ForeignWord + " ";
                        }

                        string cmd = "";

                        // Сначала меняем тип поля, оставляя NULL\NOT NULL без изменения
                        if (oldRow.FieldTypeNotEquals(newRow))
                        {
                            cmd += Environment.NewLine +
                                "ALTER" + foreign + "TABLE IF EXISTS " + newRow.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                "\tALTER COLUMN " + newRow.FieldNameToScript + Environment.NewLine +
                                "\tTYPE " + newRow.FullFieldTypeToScript + ";";
                        }

                        if (oldRow.IsIdentity != newRow.IsIdentity)
                        {
                            if (newRow.IsIdentity == true)
                                cmd += Environment.NewLine +
                                    "ALTER" + foreign + "TABLE IF EXISTS " + newRow.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                    "\tALTER COLUMN " + newRow.FieldNameToScript + Environment.NewLine +
                                    "\tADD " + newRow.IsIdentityToScript + ";";
                            else
                                cmd += Environment.NewLine +
                                    "ALTER" + foreign + "TABLE IF EXISTS " + newRow.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                    "\tALTER COLUMN " + newRow.FieldNameToScript + Environment.NewLine +
                                    "\tDROP IDENTITY IF EXISTS;";
                        }

                        if (oldRow.FieldDefault != newRow.FieldDefault)
                        {
                            if (newRow.FieldDefault != "")
                                cmd += Environment.NewLine +
                                    "ALTER" + foreign + "TABLE IF EXISTS " + newRow.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                    "\tALTER COLUMN " + newRow.FieldNameToScript + Environment.NewLine +
                                    "\tSET " + newRow.FieldDefaultToScript + ";";
                            else
                                cmd += Environment.NewLine +
                                    "ALTER" + foreign + "TABLE IF EXISTS " + newRow.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                    "\tALTER COLUMN " + newRow.FieldNameToScript + Environment.NewLine +
                                    "\tDROP " + newRow.FieldDefaultToScript + ";";
                        }

                        if (!string.IsNullOrWhiteSpace(cmd))
                        {
                            /*
                             ScriptRow +=
                                "IF EXISTS(" + Environment.NewLine +
                                "\tSELECT 1" + Environment.NewLine +
                                "\tFROM information_schema.columns" + Environment.NewLine +
                                "\tWHERE table_schema = '" + this.SchemaNameToSeek + "'" + Environment.NewLine +
                                "\tAND table_name = '" + this.TableNameToSeek + "'" + Environment.NewLine +
                                "\tAND column_name = '" + newrow.FieldNameToSeek + "'" + Environment.NewLine +
                                "\tLIMIT 1" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "THEN" + Environment.NewLine +
                                "PERFORM dbo.xp_gen_view('" + this.SchemaNameToScript + "." + this.TableNameToScript + "'," + Environment.NewLine +
                                "$viewtext$" +
                                cmd + Environment.NewLine +
                                "$viewtext$" + Environment.NewLine +
                                ", 2);" + Environment.NewLine +
                                "END IF;" + Environment.NewLine +
                                Environment.NewLine;
                            */

                            ScriptRowToList += Environment.NewLine + Environment.NewLine +
                               $"-- Alter column {newRow.FieldNameToScript}" + Environment.NewLine +
                               "IF EXISTS(" + Environment.NewLine +
                               "\tSELECT 1" + Environment.NewLine +
                               "\tFROM information_schema.columns" + Environment.NewLine +
                               "\tWHERE table_schema = '" + newRow.ParentTableInfo.SchemaNameToSeek + "'" + Environment.NewLine +
                               "\tAND table_name = '" + newRow.ParentTableInfo.TableNameToSeek + "'" + Environment.NewLine +
                               "\tAND column_name = '" + newRow.FieldNameToSeek + "'" + Environment.NewLine +
                               "\tLIMIT 1" + Environment.NewLine +
                               ")" + Environment.NewLine +
                               "THEN" +
                               cmd + Environment.NewLine +
                               "END IF;";

                            isAddDO_List = true;
                            isChanged_List = true;
                        }

                        // теперь меняем NULL\NOT NULL
                        if (oldRow.IsNotNull != newRow.IsNotNull)
                        {
                            if (newRow.IsNotNull && use_NewFunc)
                            {
                                string prefix = "SELECT FROM";

                                if (newRow.ParentTableDB.isAddRegion || isAddDO_Row) //-V3063
                                {
                                    prefix = "PERFORM";
                                }

                                // используем p_SetNotNull
                                ScriptRow += Environment.NewLine + Environment.NewLine +
                                    prefix + " dbo.p_setnotnull (" +
                                    "tablename := '" + newRow.ParentTableInfo.FullTableNameToScript + "', " +
                                    "columnname := '" + newRow.FieldNameToScript + "', " +
                                    "task := '" + MainWindow.Task.TaskNumber + "', " +
                                    "script := ''" +
                                    ");";

                                isChanged_Row = true;
                            }
                            else
                            {
                                string info;
                                if (newRow.IsNotNull == true)
                                {
                                    info = $"-- {newRow.FieldNameToScript} set NOT NULL";
                                }
                                else
                                {
                                    info = $"-- {newRow.FieldNameToScript} drop NOT NULL";
                                }

                                // генерим скрипт без использования p_SetNotNull
                                ScriptRowToList += Environment.NewLine + Environment.NewLine + 
                                    info + Environment.NewLine +
                                    "IF EXISTS(" + Environment.NewLine +
                                    "\tSELECT 1" + Environment.NewLine +
                                    "\tFROM information_schema.columns" + Environment.NewLine +
                                    "\tWHERE table_schema = '" + newRow.ParentTableInfo.SchemaNameToSeek + "'" + Environment.NewLine +
                                    "\tAND table_name = '" + newRow.ParentTableInfo.TableNameToSeek + "'" + Environment.NewLine +
                                    "\tAND column_name = '" + newRow.FieldNameToSeek + "'" + Environment.NewLine;

                                if (oldRow.IsNotNull != newRow.IsNotNull)
                                {
                                    if (newRow.IsNotNull == true)
                                        ScriptRowToList +=
                                            "\tAND is_nullable = 'YES'" + Environment.NewLine;
                                    else
                                        ScriptRowToList +=
                                            "\tAND is_nullable = 'NO'" + Environment.NewLine;
                                }

                                /*
                                ScriptRow += "\tLIMIT 1" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "THEN" + Environment.NewLine +
                                "PERFORM dbo.xp_gen_view('" + this.SchemaNameToScript + "." + this.TableNameToScript + "'," + Environment.NewLine +
                                "$viewtext$";
                                */
                                ScriptRowToList += 
                                    "\tLIMIT 1" + Environment.NewLine +
                                    ")" + Environment.NewLine +
                                    "THEN";

                                if (newRow.IsNotNull == true)
                                {
                                    ScriptRowToList += Environment.NewLine +
                                        "ALTER" + foreign + "TABLE IF EXISTS " + newRow.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                        "\tALTER COLUMN " + newRow.FieldNameToScript + Environment.NewLine +
                                        "\tSET NOT NULL" + Environment.NewLine + ";";
                                }
                                else
                                {
                                    ScriptRowToList += Environment.NewLine +
                                        "ALTER" + foreign + "TABLE IF EXISTS " + newRow.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                        "\tALTER COLUMN " + newRow.FieldNameToScript + Environment.NewLine +
                                        "\tDROP NOT NULL;";
                                }

                                /*
                                ScriptRow +=
                                    Environment.NewLine +
                                    "$viewtext$" + Environment.NewLine +
                                    ", 2);" + Environment.NewLine +
                                    "END IF;" + Environment.NewLine +
                                    Environment.NewLine;
                                */
                                ScriptRowToList +=
                                    Environment.NewLine +
                                    "END IF;";

                                isChanged_List = true;
                                isAddDO_List = true;
                            }
                        }

                        if (isChanged_Row)
                        {
                            if (isAddDO_Row) //-V3022
                            {
                                ScriptRow = 
                                    "DO $" + newRow.FieldNameReady + "$ BEGIN" + Environment.NewLine +
                                    ScriptRow.TrimNewLine() + Environment.NewLine +
                                    "END; $" + newRow.FieldNameReady + "$;";
                            }
                        }

                        if (isChanged_List)
                        {
                            ScriptRowToList = ScriptRowToList.TrimNewLine();

                            if (isAddDO_List)
                            {
                                ScriptRowToList = 
                                    "DO $" + newRow.FieldNameReady + "$ BEGIN" + Environment.NewLine +
                                     ScriptRowToList + Environment.NewLine +
                                    "END; $" + newRow.FieldNameReady + "$;";
                            }

                            ScriptAlterList += Environment.NewLine + Environment.NewLine + ScriptRowToList;
                        }
                    }

                    break;
                default:
                    break;
            }

            return Environment.NewLine + Environment.NewLine + ScriptRow.TrimNewLine();
        }

        /// <summary>
        /// Скрипт добавления поля в таблицу (таблица уже существует)
        /// </summary>
        /// <param name="row">ссылка на поле</param>
        /// <returns></returns>
        public static string AddFieldToScript(FieldDB row)
        {
            if (row == null) return "";

            bool use_NewFunc = row.ParentTableDB.isPromed && MainWindow.APPinfo.isUseNewFunc;

            //для foreign table - NOT NULL в команде 
            if (row.ParentTableInfo.isForeignTable)
            {
                use_NewFunc = false;
            }

            string ScriptRow = "";

            switch (row.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    ScriptRow += Environment.NewLine +
                        $"-- Add column {row.FieldNameToScript}" + Environment.NewLine +
                        "IF NOT EXISTS (" + Environment.NewLine +
                        "\tSELECT TOP(1) 1" + Environment.NewLine +
                        "\tFROM information_schema.columns WITH (nolock)" + Environment.NewLine +
                        "\tWHERE table_schema = N'" + row.ParentTableInfo.SchemaNameToSeek + "'" + Environment.NewLine +
                        "\tAND table_name = N'" + row.ParentTableInfo.TableNameToSeek + "'" + Environment.NewLine +
                        "\tAND column_name = N'" + row.FieldNameToSeek + "'" + Environment.NewLine +
                        ")" + Environment.NewLine +
                        "BEGIN" + Environment.NewLine +
                        "\tALTER TABLE " + row.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                        "\t\tADD " + row.FieldNameToScript + " " + row.FullFieldTypeToScript;

                    if (row.IsIdentity == true) ScriptRow += " " + row.IsIdentityToScript;
                    if ( (row.IsNotNull == true) && (!use_NewFunc) ) ScriptRow += " " + row.IsNotNullToScript;
                    if (row.FieldDefault != "") ScriptRow += " " + row.FieldDefaultToScript;

                    ScriptRow += Environment.NewLine +
                        "END" + Environment.NewLine;

                    if ((row.IsNotNull == true) && use_NewFunc)
                    {
                        // используем p_SetNotNull
                        ScriptRow += Environment.NewLine +
                            "EXEC dbo.p_SetNotNull " +
                            "@TableName = '" + row.ParentTableInfo.FullTableNameToScript + "', " + 
                            "@ColumnName = '" + row.FieldNameToScript + "', " +
                            "@Task = '" + MainWindow.Task.TaskNumber + "', " +
                            "@Script = ''" + 
                            Environment.NewLine;
                    }

                    break;

                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:

                    string prefix = "";

                    if (row.ParentTableDB.isAddRegion)
                    {
                        ScriptRow += "";
                        prefix = "PERFORM";
                    }
                    else if (!row.ParentTableInfo.isForeignTable)
                    {
                        ScriptRow += "";
                        prefix = "SELECT FROM";
                    }
                    else
                    {
                        ScriptRow += Environment.NewLine + "DO $" + row.FieldNameReady + "$ BEGIN" + Environment.NewLine;
                        prefix = "PERFORM";
                    }

                    string foreign = " ";
                    if (row.ParentTableInfo.isForeignTable)
                    {
                        // если это внешняя таблица
                        foreign = " " + row.ParentTableInfo.ForeignWord + " ";
                    }

                    ScriptRow += Environment.NewLine +
                        $"-- Add column {row.FieldNameToScript}" + Environment.NewLine +
                        "ALTER" + foreign + "TABLE IF EXISTS " + row.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                        "\tADD COLUMN IF NOT EXISTS " + row.FieldNameToScript + " " + row.FullFieldTypeToScript;
                    if (row.IsIdentity == true) ScriptRow = ScriptRow + " " + row.IsIdentityToScript;
                    if ((row.IsNotNull == true) && (!use_NewFunc)) ScriptRow = ScriptRow + " " + row.IsNotNullToScript;
                    if (row.FieldDefault != "") ScriptRow = ScriptRow + " " + row.FieldDefaultToScript;
                    ScriptRow += ";" + Environment.NewLine;

                    if (!string.IsNullOrWhiteSpace(row.ForeignColumnToScript))
                    {
                        // если это внешняя таблица
                        ScriptRow += Environment.NewLine +
                        "IF EXISTS(" + Environment.NewLine +
                        "\tSELECT 1" + Environment.NewLine +
                        "\tFROM pg_attribute a" + Environment.NewLine +
                        "\tWHERE a.attrelid = to_regclass('" + row.ParentTableInfo.FullTableNameToScript + "')" + Environment.NewLine +
                        "\tAND a.attname = '" + row.FieldNameToScript + "'" + Environment.NewLine +
                        "\tAND a.attisdropped = false" + Environment.NewLine +
                        "\tAND a.attinhcount = 0" + Environment.NewLine +
                        "\tAND a.attfdwoptions IS NOT NULL" + Environment.NewLine +
                        ") THEN" + Environment.NewLine +
                        "\tALTER" + foreign + "TABLE IF EXISTS " + row.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                        "\t\tALTER COLUMN " + row.FieldNameToScript + " OPTIONS (SET column_name '" + row.ForeignColumnToScript + "');" + Environment.NewLine +
                        "ELSE" + Environment.NewLine +
                        "\tALTER" + foreign + "TABLE IF EXISTS " + row.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                        "\t\tALTER COLUMN " + row.FieldNameToScript + " OPTIONS (ADD column_name '" + row.ForeignColumnToScript + "');" + Environment.NewLine +
                        "END IF;" + Environment.NewLine;
                    }

                    if ((row.IsNotNull == true) && use_NewFunc)
                    {
                        // используем p_SetNotNull
                        ScriptRow += Environment.NewLine +
                           prefix + " dbo.p_setnotnull (" + 
                           "tablename := '" + row.ParentTableInfo.FullTableNameToScript + "', " + 
                           "columnname := '" + row.FieldNameToScript + "', " +
                           "task := '" + MainWindow.Task.TaskNumber + "', " +
                           "script := ''" + 
                           ");" + Environment.NewLine;
                    }

                    if (row.ParentTableDB.isAddRegion) { }
                    else if (!row.ParentTableInfo.isForeignTable) { }
                    else ScriptRow += Environment.NewLine + "END; $" + row.FieldNameReady + "$;" + Environment.NewLine;

                    break;
                default:
                    break;
            }
            return ScriptRow;
        }

        /// <summary>
        /// Скрипт добавления поля в скрипт создания таблицы
        /// </summary>
        /// <param name="row">описание поля</param>
        /// <param name="ScriptSetNotNull">скрипты для SET NOT NULL</param>
        /// <returns></returns>
        public static string AddFieldToCreateScript(FieldDB row, ref string ScriptSetNotNull)
        {
            if (row == null) return "";

            string options = "";
            if (
                (!string.IsNullOrWhiteSpace(row.ForeignColumn)) &&
                (
                    (row.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || 
                    (row.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)
                )
            )
            {
                // если это внешняя таблица
                options = " OPTIONS (column_name '" + row.ForeignColumnToScript + "')";
            }

            bool use_NewFunc = row.ParentTableDB.isPromed && MainWindow.APPinfo.isUseNewFunc;

            //для всех полей новой таблицы - оставляем NOT NULL в CREATE
            use_NewFunc = false; //-V3008

            string ScriptRow = row.FieldNameToScript + " " + row.FullFieldTypeToScript + options;

            if (row.IsIdentity == true) ScriptRow += " " + row.IsIdentityToScript;
            if ((row.IsNotNull == true) && (!use_NewFunc)) ScriptRow += " " + row.IsNotNullToScript; //-V3063
            if (row.FieldDefault != "") ScriptRow += " " + row.FieldDefaultToScript;

            switch (row.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:

                    if ((row.IsNotNull == true) && use_NewFunc) //-V3022
                    {
                        // используем p_SetNotNull
                        ScriptSetNotNull += Environment.NewLine + 
                            "EXEC dbo.p_SetNotNull " + 
                            "@TableName = '" + row.ParentTableInfo.FullTableNameToScript + "', " + 
                            "@ColumnName = '" + row.FieldNameToScript + "', " +
                            "@Task = '" + MainWindow.Task.TaskNumber + "', " +
                            "@Script = ''";
                    }

                    break;

                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:

                    string prefix = "SELECT FROM";

                    if (row.ParentTableDB.isAddRegion)
                    {
                        prefix = "PERFORM";
                    }

                    if ((row.IsNotNull == true) && use_NewFunc) //-V3022
                    {
                        // используем p_SetNotNull
                        ScriptSetNotNull += Environment.NewLine +
                            prefix + " dbo.p_setnotnull (" + 
                            "tablename := '" + row.ParentTableInfo.FullTableNameToScript + "', " + 
                            "columnname := '" + row.FieldNameToScript + "', " +
                            "task := '" + MainWindow.Task.TaskNumber + "', " +
                            "script := ''" + 
                            ");";
                    }

                    break;
                default:
                    break;
            }

            return ScriptRow;
        }


        /// <summary>
        /// Скрипт добавления генерации сиквенса
        /// </summary>
        /// <param name="tableinfo">описание таблицы</param>
        /// <returns></returns>
        public static string AddSequienceToScript(TableInfo tableinfo)
        {

            if (tableinfo.ParentTableDB.isOnlyExist && (!tableinfo.HasSequence))
            {
                return "";
            }

            if (tableinfo.ParentTableDB.TableType == Utilities.TableType.EVN)
            {
                return "";
            }

            string fields = "";

            foreach (FieldDB row in tableinfo.ListFilteredField(true, null)
                .Where(x => 
                    (x.IsIdentity == true) && 
                    (x.FieldName != "") && 
                    (x.FieldType != "")
                )
                .OrderBy(x => x.FieldOrder)
            )
            {
                if (fields != "") fields = fields + ", ";
                fields = fields + row.FieldNameToScript;
            }

            if (fields == "")
            {
                return "";
            }

            switch (tableinfo.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        return "";
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        if (tableinfo.ParentTableDB.isAddRegion)
                            return "PERFORM dbo.xp_GenIdentity('" + tableinfo.FullTableNameToScript + "', '" + fields + "');";
                        else
                            return "SELECT FROM dbo.xp_GenIdentity('" + tableinfo.FullTableNameToScript + "', '" + fields + "');";
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// Скрипт добавления разрешений
        /// </summary>
        /// <param name="tableinfo">описание таблицы</param>
        /// <returns></returns>
        public static string AddGrantsToScript(TableInfo tableinfo)
        {
            return "";
        }

        /// <summary>
        /// Скрипт удаления поля
        /// </summary>
        /// <param name="row">описание поля</param>
        /// <returns></returns>
        public static string DropFieldToScript(FieldDB row)
        {
            switch (row.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        return
                            $"-- {row.FieldNameToScript} drop DEFAULT" + Environment.NewLine +
                            "DECLARE @defsql NVARCHAR(max) = (" + Environment.NewLine +
                            "\tSELECT TOP(1) def.name" + Environment.NewLine +
                            "\tFROM sys.tables t with (nolock)" + Environment.NewLine +
                            "\tINNER JOIN sys.columns c with (nolock) ON c.object_id = t.object_id" + Environment.NewLine +
                            "\tINNER JOIN sys.default_constraints def with (nolock) ON def.parent_object_id = t.object_id and def.parent_column_id = c.column_id" + Environment.NewLine +
                            "\tWHERE t.object_id = OBJECT_ID(N'" + row.ParentTableInfo.FullTableNameToScript + @"', 'U')" + Environment.NewLine +
                            "\tAND c.name = N'" + row.FieldNameToSeek + "'" + Environment.NewLine +
                            ")" + Environment.NewLine +
                            "IF COALESCE(@defsql, '') <> ''" + Environment.NewLine +
                            "BEGIN" + Environment.NewLine +
                            "\tSET @defsql = 'ALTER TABLE " + row.ParentTableInfo.FullTableNameToScript + " DROP CONSTRAINT ' + @defsql" + Environment.NewLine +
                            "\tEXECUTE sp_executesql @defsql" + Environment.NewLine +
                            "END" + Environment.NewLine +
                            Environment.NewLine +
                            $"-- Drop column {row.FieldNameToScript}" + Environment.NewLine +
                            "IF EXISTS (" + Environment.NewLine +
                            "\tSELECT TOP(1) 1" + Environment.NewLine +
                            "\tFROM information_schema.columns WITH (nolock)" + Environment.NewLine +
                            "\tWHERE table_schema = N'" + row.ParentTableInfo.SchemaNameToSeek + "'" + Environment.NewLine +
                            "\tAND table_name = N'" + row.ParentTableInfo.TableNameToSeek + "'" + Environment.NewLine +
                            "\tAND column_name = N'" + row.FieldNameToSeek + "'" + Environment.NewLine +
                            ")" + Environment.NewLine +
                            "BEGIN" + Environment.NewLine +
                            "\tALTER TABLE " + row.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                            "\t\tDROP COLUMN " + row.FieldNameToScript + Environment.NewLine +
                            "END";
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        string foreign = " ";

                        if (row.ParentTableInfo.isForeignTable)
                        {
                            // если это внешняя таблица
                            foreign = " " + row.ParentTableInfo.ForeignWord + " ";
                        }

                        return
                            $"-- Drop column {row.FieldNameToScript}" + Environment.NewLine +
                            "ALTER" + foreign + "TABLE IF EXISTS " + row.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                            "\tDROP COLUMN IF EXISTS " + row.FieldNameToScript + ";";
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// Скрипт добавления foreign key или check
        /// </summary>
        /// <param name="row">информация о поле таблицы</param>
        /// <param name="list_fk">Список FK</param>
        /// <returns></returns>
        public static string AddFKToScript(FieldDB row, List<TableFKInfo> list_fk)
        {
            string _FKSchemaNameToScript = row.FKSchemaNameToScript;
            string _FKTableNameToScript = row.FKTableNameToScript;
            string _FKFullTableToScript = row.FKFullTableToScript;
            string _FKFieldToScript = row.FKFieldToScript;
            string _FieldCheck = row.FieldCheck;
            string _FieldNameToScript = row.FieldNameToScript;

            // генерим FK только для первого поля констрейна
            if (
                (list_fk != null) &&
                (list_fk.Find(x => x.FieldName.ToLower() == row.FieldNameCompare) != null)
            )
            {
                // первое поле в FK
                var first_fk = list_fk
                    .Where(x => x.FKName.ToLower() == row.FKNameCompare)
                    .OrderBy(x => x.FKOrderToSort)
                    .FirstOrDefault();

                if (
                    first_fk != null && 
                    first_fk.FKOrderToInt != row.FKOrderToInt
                )
                {
                    return "";
                }

                // соберем поля FK
                _FKFieldToScript = "";
                _FieldNameToScript = "";

                foreach (var fk in list_fk
                    .Where(x => x.FKName.ToLower() == row.FKNameCompare)
                    .OrderBy(x => x.FKOrderToSort)
                    )
                {
                    var row_fk = row.ParentTableInfo.ListField
                        .Where(x => x.FieldNameCompare == fk.FieldName.ToLower())
                        .FirstOrDefault();

                    if (row_fk != null)
                    {
                        if (!string.IsNullOrWhiteSpace(_FKFieldToScript))
                        {
                            _FKFieldToScript += ", ";
                        }

                        if (!string.IsNullOrWhiteSpace(_FieldNameToScript))
                        {
                            _FieldNameToScript += ", ";
                        }

                        _FKFieldToScript = _FKFieldToScript + row_fk.FKFieldToScript;
                        _FieldNameToScript = _FieldNameToScript + row_fk.FieldNameToScript;
                    }
                }
            }
            else if (string.IsNullOrWhiteSpace(_FieldCheck))
            {
                return "";
                /*
                // оставил старый код
                if (_FieldNameToScript.ToLower() == "server_id")
                {
                    return "";
                }

                if (_FKFullTableToScript.ToLower() == "dbo.personevn")
                {
                    _FieldNameToScript = "server_id, " + _FieldNameToScript;
                    _FKFieldToScript = "server_id, personevn_id";
                }
                */
            }

            // унаследованные CHECK - пропускаем
            if (
                (!string.IsNullOrWhiteSpace(row.FKNameToScript)) &&
                (!string.IsNullOrWhiteSpace(_FieldCheck)) && 
                string.IsNullOrWhiteSpace(_FKFullTableToScript) &&
                row.IsInherit
            )
            {
                return "";
            }

            // Используем p_FKCreate
            bool use_NewFunc = row.ParentTableDB.isPromed && MainWindow.APPinfo.isUseNewFunc;

            if (row.ParentTableInfo.isForeignTable)
            {
                use_NewFunc = false;
            }

            // CHECK для НЕ унаследованных полей
            if (
                (!string.IsNullOrWhiteSpace(row.FKNameToScript)) &&
                (!string.IsNullOrWhiteSpace(_FieldCheck)) &&
                string.IsNullOrWhiteSpace(_FKFullTableToScript) &&
                (!row.IsInherit)
            )
            { 
                if (
                    use_NewFunc &&
                    (
                        row.FieldNameCompare.EndsWith("_deleted") ||
                        row.FieldNameCompare.StartsWith(row.ParentTableInfo.TableNameCompare + "_is")
                    )
                )
                {
                    // CHECK для полей is и deleted при использовании p_FKCreate переделываем на dbo.yesno
                    _FKSchemaNameToScript = "dbo";
                    _FKTableNameToScript = "yesno";
                    _FKFullTableToScript = "dbo.yesno";
                    _FKFieldToScript = "yesno_id";
                }
                else
                {
                    // остальные CHECK создаем без p_FKCreate
                    use_NewFunc = false;
                    _FKSchemaNameToScript = "";
                    _FKTableNameToScript = "";
                    _FKFullTableToScript = "";
                    _FKFieldToScript = "";
                }
            }

            // признак базы ГАР
            bool isGAR = (
                (Utilities.GITProjects.GetDEVProject(row.ParentTableDB.GITProject) == "dev_gar_pg") &&
                (row.ParentTableInfo.SchemaNameCompare == "gar")
            );

            if (use_NewFunc)
            {
                // используем p_FKCreate
                if (
                    (!string.IsNullOrWhiteSpace(row.FKNameToScript)) &&
                    (!string.IsNullOrWhiteSpace(_FKFullTableToScript)) &&
                    (!string.IsNullOrWhiteSpace(_FKFieldToScript))
                )
                {
                    switch (row.ParentTableDB.TargetDB)
                    {
                        case Utilities.TargetDBType.MSSQL:
                            {
                                string FK = "EXEC dbo.p_FKCreate " +
                                    "@TableName = '" + row.ParentTableInfo.FullTableNameToScript + "', " +
                                    "@ColumnNames = '" + _FieldNameToScript + "', " +
                                    "@FKTableName = '" + _FKFullTableToScript + "', " +
                                    "@FKColumnNames = '" + _FKFieldToScript + "', " +
                                    "@FKName = '" + row.FKNameToScript + "', " +
                                    "@Task = '" + MainWindow.Task.TaskNumber + "', " +
                                    "@Script = ''";

                                return FK;
                            }
                        case Utilities.TargetDBType.EMD:
                        case Utilities.TargetDBType.PGSQL:
                            {
                                string prefix = "SELECT FROM";

                                if (row.ParentTableDB.isAddRegion)
                                {
                                    prefix = "PERFORM";
                                }

                                // используем p_SetNotNull
                                string FK = prefix + " dbo.p_fkcreate (" +
                                    "tablename := '" + row.ParentTableInfo.FullTableNameToScript + "', " +
                                    "columnnames := '" + _FieldNameToScript + "', " +
                                    "fktablename := '" + _FKFullTableToScript + "', " +
                                    "fkcolumnnames := '" + _FKFieldToScript + "', " +
                                    "fkname := '" + row.FKNameToScript + "', " +
                                    "task := '" + MainWindow.Task.TaskNumber + "', " +
                                    "script := ''" +
                                    ");";

                                return FK;
                            }
                        default:
                            return "";
                    }
                }
            }
            else
            {
                // генерим скрипт без использования p_FKCreate
                if (
                       (row.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) &&
                       (!row.ParentTableDB.isOnlyExist) && //улучшение
                      (_FKFullTableToScript.ToLower() == "dbo.yesno")
                )
                {
                    // оптимизация dbo.yesno для таблиц, перечисленных в dbo.EvnClass
                    if (isGAR)
                    {
                        _FieldCheck = "(" + row.FieldNameToScript + " = ANY (ARRAY[(0)::bigint, ARRAY[(1)::bigint, (2)::bigint]))";
                    }
                    else
                    {
                        _FieldCheck = "(" + row.FieldNameToScript + " = ANY (ARRAY[(1)::bigint, (2)::bigint]))";
                    }

                    _FKSchemaNameToScript = "";
                    _FKTableNameToScript = "";
                    _FKFullTableToScript = "";
                    _FKFieldToScript = "";
                }

                if (
                    (!string.IsNullOrWhiteSpace(row.FKNameToScript)) &&
                    (!string.IsNullOrWhiteSpace(_FKFullTableToScript)) &&
                    (!string.IsNullOrWhiteSpace(_FKFieldToScript))
                    )
                {
                    // FOREIGN KEY

                    // в таблицах с наследованием на ПГ не делаем констрейны к таблицам, перечисленным в dbo.EvnClass
                    if (
                        (row.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) &&
                        (row.ParentTableInfo.HasInherit || GITProjects.GetisEvnInheritByProject(row.ParentTableDB.GITProject)) &&
                        (!row.ParentTableDB.isOnlyExist) && //улучшение
                        (!string.IsNullOrWhiteSpace(row.ParentTableDB.ListEvnClass
                            .Find(x =>
                                (x.ToLower() == _FKTableNameToScript.ToLower()) &&
                                (_FKSchemaNameToScript.ToLower() == "dbo")
                            ))
                        )
                    )
                    {
                        return "";
                    }

                    string FK = "-- Create foreign key " + row.FKNameToScript;

                    switch (row.ParentTableDB.TargetDB)
                    {
                        case Utilities.TargetDBType.MSSQL:
                            {
                                FK += Environment.NewLine +
                                    /*
                                    "IF NOT EXISTS (" + Environment.NewLine +
                                    "\tSELECT TOP(1) 1" + Environment.NewLine +
                                    "\tFROM information_schema.columns WITH (nolock)" + Environment.NewLine +
                                    "\tWHERE table_schema = N'" + _FKSchemaNameToSeek + "'" + Environment.NewLine +
                                    "\tAND table_name = N'" + _FKTableNameToSeek + "'" + Environment.NewLine +
                                    "\tAND column_name = N'" + _FKFieldToSeek + "'" + Environment.NewLine +
                                    ")" + Environment.NewLine +
                                    "\tprint '!!!WARNING!!! Table " + _FKFullTableToSeek + " or column " + _FKFullTableToSeek + "." + _FKFieldToSeek + " not found!'" + Environment.NewLine +
                                    "ELSE IF NOT EXISTS (" + Environment.NewLine +
                                    */
                                    "IF NOT EXISTS (" + Environment.NewLine +
                                    "\tSELECT TOP(1) 1" + Environment.NewLine +
                                    "\tFROM information_schema.key_column_usage WITH (nolock)" + Environment.NewLine +
                                    "\tWHERE table_schema = N'" + row.ParentTableInfo.SchemaNameToSeek + "'" + Environment.NewLine +
                                    "\tAND table_name = N'" + row.ParentTableInfo.TableNameToSeek + "'" + Environment.NewLine +
                                    "\tAND constraint_name = N'" + row.FKNameToSeek + "'" + Environment.NewLine +
                                    ")" + Environment.NewLine +
                                    "BEGIN" + Environment.NewLine +
                                    "\tALTER TABLE " + row.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                    "\t\tADD CONSTRAINT " + row.FKNameToScript + Environment.NewLine +
                                    "\t\tFOREIGN KEY (" + _FieldNameToScript + ")" + Environment.NewLine +
                                    "\t\tREFERENCES " + _FKFullTableToScript + " (" + _FKFieldToScript + ")" + Environment.NewLine +
                                    "END" + Environment.NewLine;

                                return FK;
                            }
                        case Utilities.TargetDBType.EMD:
                        case Utilities.TargetDBType.PGSQL:
                            {
                                string foreign = " ";

                                if (row.ParentTableInfo.isForeignTable)
                                {
                                    // если это внешняя таблица
                                    foreign = " " + row.ParentTableInfo.ForeignWord + " ";
                                }

                                if (row.ParentTableDB.isAddRegion) FK += "";
                                else FK += Environment.NewLine + "DO $$ BEGIN";

                                FK += Environment.NewLine +
                                    /*
                                    "IF NOT EXISTS (" + Environment.NewLine +
                                    "\tSELECT 1" + Environment.NewLine +
                                    "\tFROM information_schema.columns" + Environment.NewLine +
                                    "\tWHERE table_schema = '" + _FKSchemaNameToSeek + "'" + Environment.NewLine +
                                    "\tAND table_name = '" + _FKTableNameToSeek + "'" + Environment.NewLine +
                                    "\tAND column_name = '" + _FKFieldToSeek + "'" + Environment.NewLine +
                                    "\tLIMIT 1" + Environment.NewLine +
                                    ")" + Environment.NewLine +
                                    "THEN" + Environment.NewLine +
                                    "\traise notice '!!!WARNING!!! Table " + _FKFullTableToSeek + " or column " + _FKFullTableToSeek + "." + _FKFieldToSeek + " not found!';" + Environment.NewLine +
                                    "ELSIF NOT EXISTS (" + Environment.NewLine +
                                    */
                                    "IF NOT EXISTS (" + Environment.NewLine +
                                    "\tSELECT 1" + Environment.NewLine +
                                    "\tFROM pg_catalog.pg_constraint fk" + Environment.NewLine +
                                    "\tINNER JOIN pg_catalog.pg_class t ON t.oid = fk.conrelid AND t.relkind in ('r', 'f', 'p') AND t.relname = '" + row.ParentTableInfo.TableNameToSeek + "'" + Environment.NewLine +
                                    "\tINNER JOIN pg_catalog.pg_namespace s ON s.oid = t.relnamespace AND s.nspname = '" + row.ParentTableInfo.SchemaNameToSeek + "'" + Environment.NewLine +
                                    "\tWHERE lower(fk.conname) = lower('" + row.FKNameToSeek + "')" + Environment.NewLine +
                                    "\tLIMIT 1" + Environment.NewLine +
                                    ")" + Environment.NewLine +
                                    "THEN" + Environment.NewLine +
                                    "\tALTER" + foreign + "TABLE IF EXISTS " + row.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                    "\t\tADD CONSTRAINT " + row.FKNameToScript + Environment.NewLine +
                                    "\t\tFOREIGN KEY (" + _FieldNameToScript + ") " + Environment.NewLine +
                                    "\t\tREFERENCES " + _FKFullTableToScript + " (" + _FKFieldToScript + ")" + Environment.NewLine +
                                    "\t\tON DELETE NO ACTION" + Environment.NewLine +
                                    "\t\tON UPDATE NO ACTION" + Environment.NewLine +
                                    "\t\tNOT DEFERRABLE;" + Environment.NewLine +
                                    "END IF;";

                                if (!row.ParentTableDB.isAddRegion)
                                {
                                    FK += Environment.NewLine + "END; $$;";
                                }

                                FK += Environment.NewLine;

                                return FK;
                            }
                        default:
                            return "";
                    }
                }
                else if (
                    (!string.IsNullOrWhiteSpace(row.FKNameToScript)) &&
                    (!string.IsNullOrWhiteSpace(_FieldCheck))
                )
                {
                    // CHECK

                    string FK = "-- Create check " + row.FKNameToScript;

                    switch (row.ParentTableDB.TargetDB)
                    {
                        case Utilities.TargetDBType.EMD:
                        case Utilities.TargetDBType.PGSQL:
                            {
                                string foreign = " ";

                                if (row.ParentTableInfo.isForeignTable)
                                {
                                    // если это внешняя таблица
                                    foreign = " " + row.ParentTableInfo.ForeignWord + " ";
                                }

                                if (row.ParentTableDB.isAddRegion)
                                {
                                    FK += "";
                                }
                                else
                                {
                                    FK += Environment.NewLine + "DO $$ BEGIN";
                                }

                                FK += Environment.NewLine +
                                    "IF NOT EXISTS (" + Environment.NewLine +
                                    "\tSELECT 1" + Environment.NewLine +
                                    "\tFROM pg_catalog.pg_constraint fk" + Environment.NewLine +
                                    "\tINNER JOIN pg_catalog.pg_class t ON t.oid = fk.conrelid AND t.relkind in ('r', 'f', 'p') AND t.relname = '" + row.ParentTableInfo.TableNameToSeek + "'" + Environment.NewLine +
                                    "\tINNER JOIN pg_catalog.pg_namespace s ON s.oid = t.relnamespace AND s.nspname = '" + row.ParentTableInfo.SchemaNameToSeek + "'" + Environment.NewLine +
                                    "\tWHERE lower(fk.conname) = lower('" + row.FKNameToSeek + "')" + Environment.NewLine +
                                    "\tLIMIT 1" + Environment.NewLine +
                                    ")" + Environment.NewLine +
                                    "THEN" + Environment.NewLine +
                                    "\tALTER" + foreign + "TABLE IF EXISTS " + row.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                    "\t\tADD CONSTRAINT " + row.FKNameToScript + Environment.NewLine +
                                    "\t\tCHECK " + _FieldCheck + ";" + Environment.NewLine +
                                    "END IF;";

                                if (!row.ParentTableDB.isAddRegion)
                                {
                                    FK += Environment.NewLine + "END; $$;";
                                }

                                FK += Environment.NewLine;

                                return FK;
                            }
                        case Utilities.TargetDBType.MSSQL:
                        default:
                            return "";
                    }
                }
            }

            return "";
        }

        /// <summary>
        /// Скрипт добавления индекса для foreign key
        /// </summary>
        /// <param name="row">информация о поле таблицы</param>
        /// <param name="list_fk">Список FK</param>
        /// <returns></returns>
        public static string AddIndexFKToScript(FieldDB row, List<TableFKInfo> list_fk)
        {
            string _FieldNameToScript = row.FieldNameToScript;

            if (list_fk != null)
            {
                // первое поле в FK
                var first_fk = list_fk
                    .Where(x => x.FKName.ToLower() == row.FKNameCompare)
                    .OrderBy(x => x.FKOrderToSort)
                    .FirstOrDefault();

                if (
                    first_fk != null && 
                    first_fk.FKOrderToInt != row.FKOrderToInt
                )
                {
                    // генерим индекс только для первого поля констрейна
                    return "";
                }

                // соберем поля индекса
                _FieldNameToScript = "";

                foreach (var fk in list_fk
                    .Where(x => x.FKName.ToLower() == row.FKNameCompare)
                    .OrderBy(x => x.FKOrderToSort)
                    )
                {
                    var row_fk = row.ParentTableInfo.ListField
                        .Where(x => x.FieldNameCompare == fk.FieldName.ToLower())
                        .FirstOrDefault();

                    if (row_fk != null)
                    {
                        if (!string.IsNullOrWhiteSpace(_FieldNameToScript))
                        {
                            _FieldNameToScript += ", ";
                        }

                        _FieldNameToScript = _FieldNameToScript + row_fk.FieldNameToScript;
                    }
                }
            }
            /*else
            {
                return "";
                
                // сохранил старый код
                if (_FieldNameToScript.ToLower() == "server_id")
                {
                    return "";
                }

                if (row.FKFullTableNameCompare == "dbo.personevn")
                {
                    _FieldNameToScript = "server_id, " + _FieldNameToScript;
                }
                
            }*/

            if (string.IsNullOrWhiteSpace(_FieldNameToScript)) _FieldNameToScript = row.FieldNameToScript;

            bool use_NewFunc = row.ParentTableDB.isPromed && MainWindow.APPinfo.isUseNewFunc;

            if (row.ParentTableInfo.isForeignTable)
            {
                use_NewFunc = false;
            }

            if (
                // если генерим без использования p_FKCreate
                !use_NewFunc &&
                (
                (
                    // или есть fk
                    (!string.IsNullOrWhiteSpace(row.FKNameToScript)) &&
                    (!string.IsNullOrWhiteSpace(row.FKFullTableToScript)) &&
                    (!string.IsNullOrWhiteSpace(row.FKFieldToScript))
                ) ||
                (
                    // или есть check
                    (!string.IsNullOrWhiteSpace(row.FKNameToScript)) &&
                    (!string.IsNullOrWhiteSpace(row.FieldCheck))
                )
                )
            )
            {
                // для выгрузки текущего состояния БД в GIT решили убрать генерацию индексов для FK
                if (row.ParentTableDB.isOnlyExist)
                {
                    return "";
                }

                string FK = "";

                switch (row.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                    case Utilities.TargetDBType.PGSQL:
                        {
                            bool isAddIndex = true;

                            if (row.ParentTableDB.isOnlyExist) //-V3022
                            {
                                // если выгрузка существующей структуры - надо проверить наличие индекса
                                isAddIndex = false;

                                if (row.ParentTableDB.ListIndex != null)
                                {
                                    var index = row.ParentTableDB.ListIndex
                                        .Where(x => x.IndexNameToSeek.ToLower() == row.IndexFKNameToSeek.ToLower())
                                        .FirstOrDefault();

                                    isAddIndex = index != null;
                                }
                            }

                            if (isAddIndex) //-V3022
                            {
                                if (row.ParentTableDB.isAddRegion)
                                {
                                    FK += "";
                                }
                                else
                                {
                                    FK += Environment.NewLine;
                                }

                                FK += "CREATE INDEX IF NOT EXISTS " + row.IndexFKNameToScript + Environment.NewLine +
                                            "\tON " + row.ParentTableInfo.FullTableNameToScript + Environment.NewLine +
                                            "\tUSING btree (" + _FieldNameToScript + ")";

                                //фильтр удаленных записей для всех полей кроме _deleted
                                FieldDB fieldDeleted = null;

                                if (row.ParentTableDB.TableType == Utilities.TableType.EVN)
                                {
                                    fieldDeleted = row.ParentTableInfo.ListField
                                        .FirstOrDefault(x => x.FieldNameCompare == "evn_deleted");
                                }
                                else
                                {
                                    fieldDeleted = row.ParentTableInfo.ListField
                                        .FirstOrDefault(x => x.FieldNameCompare.EndsWith("_deleted"));
                                }

                                if (
                                    (fieldDeleted != null) &&
                                    fieldDeleted.IsNotNull &&
                                    (!row.FieldNameCompare.EndsWith("_deleted"))
                                )
                                {
                                    FK += Environment.NewLine + "\tWHERE " + fieldDeleted.FieldNameToScript + " = 1";
                                }
                                else if (
                                    (fieldDeleted != null) &&
                                    (!fieldDeleted.IsNotNull) &&
                                    (!row.FieldNameCompare.EndsWith("_deleted"))
                                )
                                {
                                    FK += Environment.NewLine + "\tWHERE " + fieldDeleted.FieldNameToScript + " = 1 OR " + fieldDeleted.FieldNameToScript + " IS NULL";
                                }

                                FK += ";" + Environment.NewLine;

                                if (!row.ParentTableDB.isAddRegion)
                                {
                                    FK += "";
                                }

                                FK += Environment.NewLine;
                            }

                            return FK;
                        }
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        return "";
                }
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Скрипт добавления комментария для region_id
        /// </summary>
        /// <param name="row">поле</param>
        /// <returns></returns>
        public static string AddSWAN_RegionalTableScript(FieldDB row)
        {
            switch (row.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        if (
                            (row.FieldNameCompare == "region_id") &&
                            (
                                (!row.ParentTableDB.isOnlyExist) ||
                                row.ParentTableInfo.HasRegionDescr
                            )
                        )
                        {
                            return Environment.NewLine +
                                "IF NOT EXISTS (" + Environment.NewLine +
                                "\tSELECT TOP(1) 1" + Environment.NewLine +
                                "\tFROM sys.extended_properties WITH (nolock)" + Environment.NewLine +
                                "\tWHERE major_id = OBJECT_ID(N'" + row.ParentTableInfo.FullTableNameToScript + "', 'U')" + Environment.NewLine +
                                "\tAND name = 'SWAN_RegionalTable'" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "BEGIN" + Environment.NewLine +
                                "\tEXEC sys.sp_addextendedproperty" + Environment.NewLine +
                                "\t\t@name = 'SWAN_RegionalTable'," + Environment.NewLine +
                                "\t\t@value = N'Региональный справочник'," + Environment.NewLine +
                                "\t\t@level0type = 'SCHEMA'," + Environment.NewLine +
                                "\t\t@level0name = N'" + row.ParentTableInfo.SchemaNameToScript + "'," + Environment.NewLine +
                                "\t\t@level1type = 'TABLE'," + Environment.NewLine +
                                "\t\t@level1name = N'" + row.ParentTableInfo.TableNameToScript + "'" + Environment.NewLine +
                                "END" + Environment.NewLine;
                        }
                        else
                        {
                            return "";
                        }
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                default:
                    return "";
            }
        }

        /// <summary>
        /// Скрипт удаления foreign key
        /// </summary>
        /// <param name="row">информация о поле таблицы</param>
        /// <param name="list_fk">Список FK</param>
        /// <returns></returns>
        public static string DropFKToScript(FieldDB row, List<TableFKInfo> list_fk)
        {
            if (string.IsNullOrWhiteSpace(row.FKNameToScript))
            {
                return "";
            }

            if (list_fk != null)
            {
                // первое поле в FK
                var first_fk = list_fk
                    .Where(x => x.FKName.ToLower() == row.FKNameCompare)
                    .OrderBy(x => x.FKOrderToSort)
                    .FirstOrDefault();

                if (
                    first_fk != null && 
                    first_fk.FKOrderToInt != row.FKOrderToInt
                )
                {
                    // генерим drop FK только для первого поля констрейна
                    return "";
                }
            }

            switch (row.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        return
                            "-- Drop foreign key " + row.FKNameToScript + Environment.NewLine +
                            "IF EXISTS (" + Environment.NewLine +
                            "\tSELECT TOP(1) 1" + Environment.NewLine +
                            "\tFROM information_schema.key_column_usage WITH (nolock)" + Environment.NewLine +
                            "\tWHERE table_schema = N'" + row.ParentTableInfo.SchemaNameToSeek + "'" + Environment.NewLine +
                            "\tAND table_name = N'" + row.ParentTableInfo.TableNameToSeek + "'" + Environment.NewLine +
                            "\tAND constraint_name = N'" + row.FKNameToSeek + "'" + Environment.NewLine +
                            ")" + Environment.NewLine +
                            "BEGIN" + Environment.NewLine +
                            "\tALTER TABLE " + row.ParentTableInfo.FullTableNameToScript + 
                            " DROP CONSTRAINT " + row.FKNameToScript + Environment.NewLine +
                            "END" + Environment.NewLine;
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        string foreign = " ";

                        if (row.ParentTableInfo.isForeignTable)
                        {
                            // если это внешняя таблица
                            foreign = " " + row.ParentTableInfo.ForeignWord + " ";
                        }

                        return
                            "-- Drop foreign key " + row.FKNameToScript + Environment.NewLine +
                            "ALTER" + foreign + "TABLE IF EXISTS " + row.ParentTableInfo.FullTableNameToScript + 
                            " DROP CONSTRAINT IF EXISTS " + row.FKNameToScript + ";" + Environment.NewLine;
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// Скрипт удаления индекса для foreign key
        /// </summary>
        /// <param name="row">информация о поле таблицы</param>
        /// <param name="list_fk">Список FK</param>
        /// <returns></returns>
        public static string DropIndexFKToScript(FieldDB row, List<TableFKInfo> list_fk)
        {
            if (string.IsNullOrWhiteSpace(row.FKNameToScript))
            {
                return "";
            }

            if (list_fk != null)
            {
                // первое поле в FK
                var first_fk = list_fk
                    .Where(x => x.FKName.ToLower() == row.FKNameCompare)
                    .OrderBy(x => x.FKOrderToSort)
                    .FirstOrDefault();

                if (
                    first_fk != null && 
                    first_fk.FKOrderToInt != row.FKOrderToInt
                )
                {
                    // генерим drop индекса только для первого поля констрейна
                    return "";
                }
            }

            switch (row.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        return "";
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        bool isDropIndex = true;

                        if (row.ParentTableDB.isOnlyExist)
                        {
                            // если выгрузка существующей структуры - надо проверить наличие индекса
                            isDropIndex = false;

                            if (row.ParentTableDB.ListIndex != null)
                            {
                                var index = row.ParentTableDB.ListIndex
                                    .Where(x => x.IndexNameToSeek.ToLower() == row.IndexFKNameToSeek.ToLower())
                                    .FirstOrDefault();

                                isDropIndex = index != null;
                            }
                        }

                        if (isDropIndex)
                        {
                            return
                                "DROP INDEX IF EXISTS " + row.IndexFullFKNameToScript + ";" + Environment.NewLine;
                        }
                        else
                        {
                            return "";
                        }
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// Список полей в primary key
        /// </summary>
        /// <param name="isToScript">=true - для включения в скрипт</param>
        /// <param name="tableinfo">описание таблицы</param>
        /// <returns></returns>
        public static string PKListFields(bool isToScript, TableInfo tableinfo)
        {
            string pk = "";

            if (
                (tableinfo.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) &&
                (tableinfo.ParentTableDB.TableType ==  Utilities.TableType.EVN)
                )
            {
                // для ПГ primary key таблиц событий (EvnXXX) всегда evn_id
                pk = "evn_id";
            }
            else if (
                (tableinfo.ParentTableDB.TargetDB == Utilities.TargetDBType.MSSQL) &&
                (tableinfo.ParentTableDB.TableType == Utilities.TableType.EVN)
            )
            {
                // для МС primary key таблиц событий (EvnXXX) всегда таблица_id
                pk = tableinfo.TableNameReady + "_id";
            }
            else
            {
                foreach (FieldDB row in tableinfo.ListField
                    .Where(x => 
                        (x.IsPK == true) && 
                        (x.FieldName != "") && 
                        (x.FieldType != "")
                     )
                    .OrderBy(x => x.PKOrderToSort)
                )
                {
                    if (pk != "")
                    {
                        pk = pk + ", ";
                    }

                    if (isToScript)
                    {
                        pk = pk + row.FieldNameToScript;
                    }
                    else
                    {
                        pk = pk + row.FieldName;
                    }
                }
            }

            if (pk == "")
            {
                return "";
            }

            return pk;
        }

        /// <summary>
        /// скрипт добавления primary key
        /// </summary>
        /// <param name="tableinfo">описание таблицы</param>
        /// <returns></returns>
        public static string AddPKToScript(TableInfo tableinfo)
        {
            if (tableinfo.PKNameToScript == "")
            {
                tableinfo.PKName = "pk_" + tableinfo.TableNameReady + "_id";
            }

            string cons = "-- Create primary key" + Environment.NewLine;
            string pk = PKListFields(true, tableinfo);

            if (string.IsNullOrWhiteSpace(pk))
            {
                return "";
            }

            switch (tableinfo.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        cons +=
                            "IF NOT EXISTS (" + Environment.NewLine +
                            "\tSELECT TOP(1) 1" + Environment.NewLine +
                            "\tFROM information_schema.table_constraints WITH (nolock)" + Environment.NewLine +
                            "\tWHERE table_schema = N'" + tableinfo.SchemaNameToSeek + "'" + Environment.NewLine +
                            "\tAND table_name = N'" + tableinfo.TableNameToSeek + "'" + Environment.NewLine +
                            "\tAND constraint_type = 'PRIMARY KEY'" + Environment.NewLine +
                            ")" + Environment.NewLine +
                            "BEGIN" + Environment.NewLine +
                            "\tALTER TABLE " + tableinfo.FullTableNameToScript + Environment.NewLine +
                            "\t\tADD CONSTRAINT " + tableinfo.PKNameToScript + Environment.NewLine +
                            "\t\tPRIMARY KEY CLUSTERED (" + pk + ")" + Environment.NewLine +
                            "END" + Environment.NewLine;

                        return cons;
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        string foreign = " ";

                        if (tableinfo.isForeignTable)
                        {
                            // если это внешняя таблица
                            foreign = " " + tableinfo.ForeignWord + " ";
                        }

                        if (tableinfo.ParentTableDB.isAddRegion)
                        {
                            cons += "";
                        }
                        else
                        {
                            cons += "DO $$ BEGIN" + Environment.NewLine;
                        }

                        cons +=
                            "IF NOT EXISTS (" + Environment.NewLine +
                            "\tSELECT 1" + Environment.NewLine +
                            "\tFROM pg_catalog.pg_constraint pk" + Environment.NewLine +
                            "\tINNER JOIN pg_catalog.pg_class t ON t.oid = pk.conrelid AND t.relkind in ('r', 'f', 'p') AND t.relname = '" + tableinfo.TableNameToSeek + "'" + Environment.NewLine +
                            "\tINNER JOIN pg_catalog.pg_namespace s ON s.oid = t.relnamespace AND s.nspname = '" + tableinfo.SchemaNameToSeek + "'" + Environment.NewLine +
                            "\tWHERE pk.contype = 'p'" + Environment.NewLine +
                            "\tLIMIT 1" + Environment.NewLine +
                            ")" + Environment.NewLine +
                            "THEN" + Environment.NewLine +
                            "\tALTER" + foreign + "TABLE IF EXISTS " + tableinfo.FullTableNameToScript + Environment.NewLine +
                            "\t\tADD CONSTRAINT " + tableinfo.PKNameToScript + Environment.NewLine +
                            "\t\tPRIMARY KEY (" + pk + ");" + Environment.NewLine +
                            "END IF;" + Environment.NewLine;

                        if (!tableinfo.ParentTableDB.isAddRegion)
                        {
                            cons += "END; $$;" + Environment.NewLine;
                        }

                        if (
                            (tableinfo.ParentTableDB.TableType ==  Utilities.TableType.EVN) &&
                            (
                                (!tableinfo.ParentTableDB.isOnlyExist) ||
                                tableinfo.HasSequence
                            )
                        )
                        {
                            cons += Environment.NewLine + "ALTER TABLE IF EXISTS ONLY " + tableinfo.FullTableNameToScript + Environment.NewLine +
                                "\tALTER COLUMN evn_id SET DEFAULT nextval('dbo.evn_evn_id_seq'::regclass);" + Environment.NewLine;
                        }

                        return cons;
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// скрипт удаления primary key
        /// </summary>
        /// <param name="tableinfo">описание таблицы</param>
        /// <returns></returns>
        public static string DropPKToScript(TableInfo tableinfo)
        {
            if (tableinfo.PKNameToScript == "")
            {
                return "";
            }

            switch (tableinfo.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        return
                            "-- Drop primary key" + Environment.NewLine +
                            "IF EXISTS (" + Environment.NewLine +
                            "\tSELECT TOP(1) 1" + Environment.NewLine +
                            "\tFROM information_schema.key_column_usage WITH (nolock)" + Environment.NewLine +
                            "\tWHERE table_schema = N'" + tableinfo.SchemaNameToSeek + "'" + Environment.NewLine +
                            "\tAND table_name = N'" + tableinfo.TableNameToSeek + "'" + Environment.NewLine +
                            "\tAND constraint_name = N'" + tableinfo.PKNameToSeek + "'" + Environment.NewLine +
                            ")" + Environment.NewLine +
                            "BEGIN" + Environment.NewLine +
                            "\tALTER TABLE " + tableinfo.FullTableNameToScript + 
                            " DROP CONSTRAINT " + tableinfo.PKNameToScript + Environment.NewLine +
                            "END";
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        string foreign = " ";

                        if (tableinfo.isForeignTable)
                        {
                            // если это внешняя таблица
                            foreign = " " + tableinfo.ForeignWord + " ";
                        }

                        return
                            "-- Drop primary key" + Environment.NewLine +
                            "ALTER" + foreign + "TABLE IF EXISTS " + tableinfo.FullTableNameToScript + 
                            " DROP CONSTRAINT IF EXISTS " + tableinfo.PKNameToScript + ";";
                    }
                default:
                    return "";
            }
        }


        /// <summary>
        /// скрипт изменения описания таблицы
        /// </summary>
        /// <param name="tableinfo">описание таблицы</param>
        /// <returns></returns>
        public static string ChangeTableDescToScript(TableInfo tableinfo)
        {
            if (string.IsNullOrWhiteSpace(tableinfo.TableDesc))
            {
                return "";
            }

            switch (tableinfo.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        string desc =
                                "IF NOT EXISTS (" + Environment.NewLine +
                                "\tSELECT TOP(1) 1" + Environment.NewLine +
                                "\tFROM sys.extended_properties WITH (nolock)" + Environment.NewLine +
                                "\tWHERE major_id = OBJECT_ID(N'" + tableinfo.FullTableNameToScript + @"', 'U')" + Environment.NewLine +
                                "\tAND name = 'MS_Description'" + Environment.NewLine +
                                "\tAND minor_id = 0" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "BEGIN" + Environment.NewLine +
                                "\tEXEC sys.sp_addextendedproperty" + Environment.NewLine +
                                "\t\t@name = 'MS_Description'," + Environment.NewLine +
                                "\t\t@value = N'" + tableinfo.TableDesc + @"'," + Environment.NewLine +
                                "\t\t@level0type = 'SCHEMA'," + Environment.NewLine +
                                "\t\t@level0name = N'" + tableinfo.SchemaNameToScript + @"'," + Environment.NewLine +
                                "\t\t@level1type = 'TABLE'," + Environment.NewLine +
                                "\t\t@level1name = '" + tableinfo.TableNameToScript + @"'" + Environment.NewLine +
                                "END" + Environment.NewLine +
                                "ELSE" + Environment.NewLine +
                                "BEGIN" + Environment.NewLine +
                                "\tEXEC sys.sp_updateextendedproperty" + Environment.NewLine +
                                "\t\t@name = 'MS_Description'," + Environment.NewLine +
                                "\t\t@value = N'" + tableinfo.TableDesc + @"'," + Environment.NewLine +
                                "\t\t@level0type = 'SCHEMA'," + Environment.NewLine +
                                "\t\t@level0name = N'" + tableinfo.SchemaNameToScript + @"'," + Environment.NewLine +
                                "\t\t@level1type = 'TABLE'," + Environment.NewLine +
                                "\t\t@level1name = N'" + tableinfo.TableNameToScript + @"'" + Environment.NewLine +
                                "END" + Environment.NewLine;

                        return desc;
                        }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                        {
                        return TableInfo.AddTableDescToScript(tableinfo);
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// скрипт добавления описания поля
        /// </summary>
        /// <param name="tableinfo">описание таблицы</param>
        /// <returns></returns>
        public static string AddTableDescToScript(TableInfo tableinfo)
        {
            if (string.IsNullOrWhiteSpace(tableinfo.TableDesc))
            {
                return "";
            }

            switch (tableinfo.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        string desc =
                                "IF NOT EXISTS (" + Environment.NewLine +
                                "\tSELECT TOP(1) 1" + Environment.NewLine +
                                "\tFROM sys.extended_properties WITH (nolock)" + Environment.NewLine +
                                "\tWHERE major_id = OBJECT_ID(N'" + tableinfo.FullTableNameToScript + "', 'U')" + Environment.NewLine +
                                "\tAND name = 'MS_Description'" + Environment.NewLine +
                                "\tAND minor_id = 0" + Environment.NewLine +
                                ")" + Environment.NewLine +
                                "BEGIN" + Environment.NewLine +
                                "\tEXEC sys.sp_addextendedproperty" + Environment.NewLine +
                                "\t\t@name = 'MS_Description'," + Environment.NewLine +
                                "\t\t@value = N'" + tableinfo.TableDesc + "'," + Environment.NewLine +
                                "\t\t@level0type = 'SCHEMA'," + Environment.NewLine +
                                "\t\t@level0name = N'" + tableinfo.SchemaNameToScript + "'," + Environment.NewLine +
                                "\t\t@level1type = 'TABLE'," + Environment.NewLine +
                                "\t\t@level1name = N'" + tableinfo.TableNameToScript + "'" + Environment.NewLine +
                                "END" + Environment.NewLine;

                        return desc;
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        string foreign = " ";

                        if (tableinfo.isForeignTable)
                        {
                            // если это внешняя таблица
                            foreign = " " + tableinfo.ForeignWord + " ";
                        }

                        string desc = "COMMENT ON" + foreign + "TABLE " + tableinfo.FullTableNameToScript + " IS '" + tableinfo.TableDesc + "';" + Environment.NewLine;

                        return desc;
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// скрипт изменения описания поля
        /// </summary>
        /// <param name="ListChildEvn">список полей родительских таблиц</param>
        /// <param name="row">поле</param>
        /// <returns></returns>
        public static string ChangeFieldDescToScript(List<string> ListChildEvn, FieldDB row)
        {
            if (string.IsNullOrWhiteSpace(row.FieldDesc))
            {
                return "";
            }

            switch (row.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        string desc =
                            "IF NOT EXISTS (" + Environment.NewLine +
                            "\tSELECT TOP(1) 1" + Environment.NewLine +
                            "\tFROM sys.extended_properties WITH (nolock)" + Environment.NewLine +
                            "\tWHERE major_id = OBJECT_ID(N'" + row.ParentTableInfo.FullTableNameToScript + @"', 'U')" + Environment.NewLine +
                            "\tAND minor_id = (" + Environment.NewLine +
                            "\t\tSELECT column_id" + Environment.NewLine +
                            "\t\tFROM sys.columns WITH (nolock)" + Environment.NewLine +
                            "\t\tWHERE name = N'" + row.FieldNameToScript + @"'" + Environment.NewLine +
                            "\t\tAND object_id = OBJECT_ID(N'" + row.ParentTableInfo.FullTableNameToScript + @"', 'U')" + Environment.NewLine +
                            "\t)" + Environment.NewLine +
                            ")" + Environment.NewLine +
                            "BEGIN" + Environment.NewLine +
                            "\tEXEC sys.sp_addextendedproperty" + Environment.NewLine +
                            "\t\t@name = 'MS_Description'," + Environment.NewLine +
                            "\t\t@value = N'" + row.FieldDesc + @"'," + Environment.NewLine +
                            "\t\t@level0type = 'SCHEMA'," + Environment.NewLine +
                            "\t\t@level0name = N'" + row.ParentTableInfo.SchemaNameToScript + @"'," + Environment.NewLine +
                            "\t\t@level1type = 'TABLE'," + Environment.NewLine +
                            "\t\t@level1name = N'" + row.ParentTableInfo.TableNameToScript + @"'," + Environment.NewLine +
                            "\t\t@level2type = 'COLUMN'," + Environment.NewLine +
                            "\t\t@level2name = N'" + row.FieldNameToScript + @"'" + Environment.NewLine +
                            "END" + Environment.NewLine +
                            "ELSE" + Environment.NewLine +
                            "BEGIN" + Environment.NewLine +
                            "\tEXEC sys.sp_updateextendedproperty" + Environment.NewLine +
                            "\t\t@name = 'MS_Description'," + Environment.NewLine +
                            "\t\t@value = N'" + row.FieldDesc + @"'," + Environment.NewLine +
                            "\t\t@level0type = 'SCHEMA'," + Environment.NewLine +
                            "\t\t@level0name = N'" + row.ParentTableInfo.SchemaNameToScript + @"'," + Environment.NewLine +
                            "\t\t@level1type = 'TABLE'," + Environment.NewLine +
                            "\t\t@level1name = N'" + row.ParentTableInfo.TableNameToScript + @"'," + Environment.NewLine +
                            "\t\t@level2type = 'COLUMN'," + Environment.NewLine +
                            "\t\t@level2name = N'" + row.FieldNameToScript + @"'" + Environment.NewLine +
                            "END" + Environment.NewLine;

                        return desc;
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        return TableInfo.AddFieldDescToScript(ListChildEvn, row);
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// скрипт добавления описания поля
        /// </summary>
        /// <param name="ListChildEvn">список дочерних таблиц, в которых унаследовано это поле</param>
        /// <param name="row">поле</param>
        /// <returns></returns>
        public static string AddFieldDescToScript(List<string> ListChildEvn, FieldDB row)
        {
            if (string.IsNullOrWhiteSpace(row.FieldDesc))
            {
                return "";
            }

            switch (row.ParentTableDB.TargetDB)
            {
                case Utilities.TargetDBType.MSSQL:
                    {
                        string desc =
                            "IF NOT EXISTS (" + Environment.NewLine +
                            "\tSELECT TOP(1) 1" + Environment.NewLine +
                            "\tFROM sys.extended_properties WITH (nolock)" + Environment.NewLine +
                            "\tWHERE major_id = OBJECT_ID(N'" + row.ParentTableInfo.FullTableNameToScript + "', 'U')" + Environment.NewLine +
                            "\tAND minor_id = (" + Environment.NewLine +
                            "\t\tSELECT column_id" + Environment.NewLine +
                            "\t\tFROM sys.columns WITH (nolock)" + Environment.NewLine +
                            "\t\tWHERE name = N'" + row.FieldNameToScript + "'" + Environment.NewLine +
                            "\t\tAND object_id = OBJECT_ID(N'" + row.ParentTableInfo.FullTableNameToScript + "', 'U')" + Environment.NewLine +
                            "\t)" + Environment.NewLine +
                            ")" + Environment.NewLine +
                            "BEGIN" + Environment.NewLine +
                            "\tEXEC sys.sp_addextendedproperty" + Environment.NewLine +
                            "\t\t@name = 'MS_Description'," + Environment.NewLine +
                            "\t\t@value = N'" + row.FieldDesc + "'," + Environment.NewLine +
                            "\t\t@level0type = 'SCHEMA'," + Environment.NewLine +
                            "\t\t@level0name = N'" + row.ParentTableInfo.SchemaNameToScript + "'," + Environment.NewLine +
                            "\t\t@level1type = 'TABLE'," + Environment.NewLine +
                            "\t\t@level1name = N'" + row.ParentTableInfo.TableNameToScript + "'," + Environment.NewLine +
                            "\t\t@level2type = 'COLUMN'," + Environment.NewLine +
                            "\t\t@level2name = N'" + row.FieldNameToScript + "'" + Environment.NewLine +
                            "END" + Environment.NewLine;

                        return desc;
                    }
                case Utilities.TargetDBType.EMD:
                case Utilities.TargetDBType.PGSQL:
                    {
                        string desc = "COMMENT ON COLUMN " + row.ParentTableInfo.FullTableNameToScript + "." + row.FieldNameToScript + " IS '" + row.FieldDesc + "';";

                        // на ПГ для таблиц Evn добавим команды для дочерних таблиц
                        if (
                            (row.ParentTableDB.TableType ==  Utilities.TableType.EVN) && 
                            (ListChildEvn != null) && 
                            (!row.ParentTableDB.isOnlyExist)
                        )
                        {
                            foreach (var item in ListChildEvn)
                            {
                                desc += Environment.NewLine + "COMMENT ON COLUMN dbo." + item + "." + row.FieldNameToScript + " IS '" + row.FieldDesc + "';";
                            }
                        }

                        return desc;
                    }
                default:
                    return "";
            }
        }

        /// <summary>
        /// Собрать имя fk по умолчанию
        /// </summary>
        /// <param name="FieldName">поле</param>
        /// <returns></returns>
        public string GetFKNameDefault(string FieldName)
        {
            if (string.IsNullOrWhiteSpace(FieldName))
            {
                return "";
            }

            FieldName = FieldName.Replace("\"", String.Empty);

            string result = "";

            if (FieldName.ToLower().StartsWith(this.TableNameCompare + "_"))
            {
                result = "fk_" + FieldName;
            }
            else
            {
                result = "fk_" + this.TableNameReady + "_" + FieldName;
            }

            if (result.Length > 62)
            {
                result = result.Substring(0, 62);
            }

            return result;
        }
    }

    // =========================================================================================================
    /// <summary>Класс Поле таблицы</summary>
    public class FieldDB : INotifyPropertyChanged
    {
        /// <summary>Список таблиц Evn</summary>
        public static List<string> ListEvn = new List<string>();

        /// <summary>
        /// реализация NotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// реализация NotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName">propertyName</param>
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); //-V3083
            }
        }

        // родительские таблицы
        /// <summary>Родительский экземпляр TableDB</summary>
        public TableDB ParentTableDB;

        /// <summary>Родительский экземпляр TableInfo</summary>
        public TableInfo ParentTableInfo;

        /// <summary>
        /// Конструктор FieldDB - инициализация значений по умолчанию
        /// </summary>
        /// <param name="parentDB">описание таблицы TableDB</param>
        /// <param name="parentInfo">экземпляр TableInfo</param>
        public FieldDB(TableDB parentDB, TableInfo parentInfo)
        {
            this.ParentTableDB = parentDB;
            this.ParentTableInfo = parentInfo;
        }

        /// <summary>
        /// Конструктор FieldDB - инициализация значений из другого экземпляра FieldDB
        /// </summary>
        /// <param name="parentDB">описание таблицы TableDB</param>
        /// <param name="parentInfo">экземпляр TableInfo</param>
        /// <param name="field">экземпляр FieldDB</param>
        public FieldDB(TableDB parentDB, TableInfo parentInfo, FieldDB field)
        {
            this.ParentTableDB = parentDB;
            this.ParentTableInfo = parentInfo;
            this.FieldOrder = field.FieldOrder;
            this.FieldName = field.FieldName;
            this.FieldDesc = field.FieldDesc;
            this.FieldType = field.FieldType;
            this.FieldSize = field.FieldSize;
            this.FieldDec = field.FieldDec;
            this.IsNotNull = field.IsNotNull;
            this.IsIdentity = field.IsIdentity;
            this.IsPK = field.IsPK;
            this.FieldDefault = field.FieldDefault;
            this.FKName = field.FKName;
            this.FKTable = field.FKTable;
            this.FKField = field.FKField;
            this.ForeignColumn = field.ForeignColumn;
            this.FieldCheck = field.FieldCheck;
            this.PKOrder = field.PKOrder;
            this.FKOrder = field.FKOrder;
            this.IsInherit = field.IsInherit;
            this.InheritParentTable = field.InheritParentTable;
        }

        int _order;
        /// <summary>Номер поля по порядку</summary>
        public int FieldOrder
        {
            get
            {
                return _order;
            }
            set
            {
                _order = value; 
                NotifyPropertyChanged("FieldOrder");
            }
        }

        /// <summary>Номер поля по порядку - в виде текста</summary>
        [JsonIgnore]
        public string FieldOrder_string
        {
            get
            {
                return _order.ToString();
            }
            set
            {
                if (!int.TryParse(value, out _order))
                {
                    _order = 0;
                }
            }
        }

        string _field_name;
        /// <summary>Имя поля</summary>
        public string FieldName
        {
            get
            {
                return _field_name ?? "";
            }
            set
            {
                _field_name = value;
                if (string.IsNullOrWhiteSpace(_field_name)) _field_name = "";
                _field_name = _field_name
                    .Trim()
                    .Replace("\"", String.Empty);

                NotifyPropertyChanged("FieldName");
                NotifyPropertyChanged("FKField");
                NotifyPropertyChanged("FKName");
            }
        }

        /// <summary>Имя поля - без кавычек, но с сохранением оригинального регистра</summary>
        public string FieldNameReady => this.FieldName.Replace("\"", String.Empty);

        /// <summary>Имя поля - для сравнения, в нижнем регистре и без кавычек</summary>
        public string FieldNameCompare => this.FieldNameReady.ToLower();

        /// <summary>Имя поля - для использования в скриптах</summary>
        public string FieldNameToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                        {
                            string result = this.FieldNameReady;
                            if (result != result.ToLower())
                            {
                                // заворачиваем в кавычки, только если все наименование НЕ в нижнем регистре
                                result = "\"" + result + "\"";
                            }

                            return result;
                        }
                    case Utilities.TargetDBType.MSSQL:
                        return this.FieldNameReady;
                    case Utilities.TargetDBType.PGSQL:
                        return this.FieldNameReady.ToLower();
                    default:
                        return this.FieldNameReady;
                }
            }
        }

        /// <summary>Имя поля - для поиска в БД: без кавычек и приведено к нужному регистру</summary>
        public string FieldNameToSeek => this.FieldNameToScript.Replace("\"", String.Empty);

        string _field_desc;
        /// <summary>Описание поля</summary>
        public string FieldDesc
        {
            get
            {
                return _field_desc ?? "";
            }
            set
            {
                _field_desc = value;
                if (string.IsNullOrWhiteSpace(_field_desc)) _field_desc = "";
                _field_desc = _field_desc.Trim();
                NotifyPropertyChanged("FieldDesc");
            }
        }

        string _field_type;
        /// <summary>Тип поля</summary>
        public string FieldType
        {
            get
            {
                return _field_type ?? "";
            }
            set
            {
                _field_type = value;
                if (string.IsNullOrWhiteSpace(_field_type)) _field_type = "";
                _field_type = _field_type.Trim().ToUpper();
                NotifyPropertyChanged("FieldType");
            }
        }

        string _field_size;
        /// <summary>Размер поля</summary>
        public string FieldSize
        {
            get
            {
                return _field_size ?? "";
            }
            set
            {
                _field_size = value;
                if (string.IsNullOrWhiteSpace(_field_size)) _field_size = "";
                _field_size = _field_size.Trim().ToLower();
                NotifyPropertyChanged("FieldSize");
            }
        }

        string _field_dec;
        /// <summary>Кол-во знаков после запятой</summary>
        public string FieldDec
        {
            get
            {
                return _field_dec ?? "";
            }
            set
            {
                _field_dec = value;
                if (string.IsNullOrWhiteSpace(_field_dec)) _field_dec = "";
                _field_dec = _field_dec.Trim().ToLower();
                NotifyPropertyChanged("FieldDec");
            }
        }

        /// <summary>Тип поля (кратко, без размеров)- для использования в скриптах</summary>
        public string FieldTypeToScript
        {
            get
            {
                switch (this.FieldType)
                {
                    // логический
                    case "SYSTEM.BOOLEAN":
                    case "BOOLEAN":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "BOOLEAN";
                        else return "BIT";

                    // целый 1 байт
                    case "SYSTEM.BYTE":
                    case "SYSTEM.SBYTE":
                    case "TINYINT":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "SMALLINT";
                        else return "TINYINT";

                    // целый 2 байта
                    case "INT2":
                    case "SYSTEM.INT16":
                    case "SYSTEM.UINT16":
                    case "SMALLINT":
                        return "SMALLINT";

                    // целый 4 байта
                    case "INT4":
                    case "SYSTEM.INT32":
                    case "SYSTEM.UINT32":
                    case "INTEGER":
                    case "INT":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "INTEGER";
                        else return "INT";

                    case "SERIAL":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return this.FieldType;
                        else return "INT";

                    // целый 8 байт
                    case "INT8":
                    case "SYSTEM.INT64":
                    case "SYSTEM.UINT64":
                        return "BIGINT";

                    case "BIGSERIAL":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return this.FieldType;
                        else return "BIGINT";

                    // строка
                    case "SYSTEM.STRING":
                        return "VARCHAR";

                    case "TEXT":
                    case "NTEXT":
                    case "NVARCHAR":
                    case "SYSNAME":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "VARCHAR";
                        else return this.FieldType;

                    // символ
                    case "SYSTEM.CHAR":
                        return "CHAR";

                    case "NCHAR":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "CHAR";
                        else return this.FieldType;

                    // фиксированная точность
                    case "DECIMAL":
                    case "NUMERIC":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "NUMERIC";
                        else return this.FieldType;

                    // плавающая одинарная точность
                    case "FLOAT4":
                    case "SYSTEM.SINGLE":
                        return "REAL";

                    // плавающая двойная точность
                    case "FLOAT8":
                    case "SYSTEM.DOUBLE":
                    case "SYSTEM.DECIMAL":
                    case "FLOAT":
                    case "DOUBLE PRECISION":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "DOUBLE PRECISION";
                        else return "FLOAT";

                    /*case "MONEY":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "DOUBLE PRECISION";
                        else return "MONEY";*/

                    // дата и время
                    case "SYSTEM.DATETIME":
                    case "DATETIME":
                    case "TIMESTAMP WITHOUT TIME ZONE":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "TIMESTAMP WITHOUT TIME ZONE";
                        else return "DATETIME";

                    case "SMALLDATETIME":
                    case "DATETIME2":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "TIMESTAMP WITHOUT TIME ZONE";
                        else return this.FieldType;

                    case "TIMESTAMP WITH TIME ZONE":
                    case "TIMESTAMPTZ":
                    case "DATETIMEOFFSET":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "TIMESTAMPTZ";
                        else return "DATETIMEOFFSET";

                    case "TIME":
                    case "TIME WITHOUT TIME ZONE":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "TIME WITHOUT TIME ZONE";
                        else return "TIME";

                    case "TIMETZ":
                    case "TIME WITH TIME ZONE":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "TIMETZ";
                        else return "TIME";

                    // blob
                    case "BYTEA":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return this.FieldType;
                        else return "VARBINARY";

                    case "SYSTEM.BYTE[]":
                    case "TIMESTAMP":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "BYTEA";
                        else return "TIMESTAMP";

                    case "VARBINARY":
                    case "IMAGE":
                    case "BINARY":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "BYTEA";
                        else return this.FieldType;

                    // guid
                    case "SYSTEM.GUID":
                    case "UUID":
                    case "UNIQUEIDENTIFIER":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "UUID";
                        else return "UNIQUEIDENTIFIER";

                    // нестандартные типы Postgres
                    case "INET":
                    case "ANYARRAY":
                    case "ARRAY":
                    case "INTERVAL":
                    case "JSON":
                    case "JSONB":
                    case "BIT VARYING":
                    case "VARBIT":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return this.FieldType;
                        else return "VARCHAR";

                    // нестандартные типы MS SQL
                    case "SQL_VARIANT":
                    case "HIERARCHYID":
                        if ((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) return "VARCHAR";
                        else return this.FieldType;

                    // прочее
                    default:
                        return this.FieldType;
                }
            }
        }

        /// <summary>Полный тип поля (с размером)- для использования в скриптах</summary>
        public string FullFieldTypeToScript
        {
            get
            {
                switch (this.FieldTypeToScript)
                {
                    case "BIT":
                    case "BIT VARYING":
                    case "VARBIT":
                        if (((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) && (this.FieldSize != "")) return this.FieldTypeToScript + "(" + this.FieldSize + ")";
                        else return this.FieldTypeToScript;

                    case "FLOAT":
                    case "CHAR":
                    case "NCHAR":
                    case "BINARY":
                    case "VARBINARY":
                    case "DATETIME2":
                    case "TIMESTAMP WITHOUT TIME ZONE":
                    case "TIMESTAMPTZ":
                    case "DATETIMEOFFSET":
                    case "TIME":
                    case "TIMETZ":
                        {
                            if (((this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)) && (this.FieldSize.ToLower() == "max")) return this.FieldTypeToScript;
                            if (
                                (this.FieldSize != "") &&
                                (this.FieldSize != "-1")
                            ) return this.FieldTypeToScript + "(" + this.FieldSize + ")";
                            else return this.FieldTypeToScript;
                        }

                    case "VARCHAR":
                    case "NVARCHAR":
                        {
                            if (
                                (this.ParentTableDB.TargetDB == Utilities.TargetDBType.PGSQL) || 
                                (this.ParentTableDB.TargetDB == Utilities.TargetDBType.EMD)
                            )
                            {
                                if (this.FieldSize.ToLower() == "max")
                                {
                                    return this.FieldTypeToScript;
                                }

                                if (!int.TryParse(this.FieldSize, out int _size))
                                {
                                    _size = 0;
                                }

                                if (
                                    (
                                        (_size > 400) ||
                                        (this.ParentTableDB.TableType ==  Utilities.TableType.EVN) ||
                                        (this.ParentTableDB.TableType ==  Utilities.TableType.PERSONEVN) ||
                                        (this.ParentTableDB.TableType ==  Utilities.TableType.MORBUS)
                                    ) &&
                                    (!this.ParentTableDB.isOnlyExist) &&
                                    this.ParentTableDB.isReglament
                                    )
                                {
                                    // требование регламента
                                    return this.FieldTypeToScript;
                                }

                                if (this.FieldSize != "")
                                {
                                    return this.FieldTypeToScript + "(" + this.FieldSize + ")";
                                }
                                else
                                {
                                    return this.FieldTypeToScript;
                                }
                            }
                            else
                            {
                                if (this.FieldSize != "")
                                {
                                    return this.FieldTypeToScript + "(" + this.FieldSize + ")";
                                }
                                else
                                {
                                    return this.FieldTypeToScript;
                                }
                            }
                        }

                    case "DECIMAL":
                    case "NUMERIC":
                        {
                            if ((this.FieldSize != "") && (this.FieldDec != ""))
                            {
                                return this.FieldTypeToScript + "(" + this.FieldSize + ", " + this.FieldDec + ")";
                            }
                            if (this.FieldSize != "")
                            {
                                return this.FieldTypeToScript + "(" + this.FieldSize + ")";
                            }
                            return this.FieldTypeToScript;
                        }

                    default:
                        return this.FieldTypeToScript;
                }

            }
        }

        /// <summary>Глобальный тип поля (строка, число, логическое)</summary>
        public Utilities.GeneralType FieldGeneralType
        {
            get
            {
                switch (this.FieldType)
                {
                    case "DOUBLE PRECISION":
                    case "FLOAT":
                    case "MONEY":
                    case "INTEGER":
                    case "BIGINT":
                    case "INT":
                    case "BIT":
                    case "TINYINT":
                    case "SMALLINT":
                    case "DECIMAL":
                    case "NUMERIC":
                    case "REAL":
                    case "SYSTEM.BYTE":
                    case "SYSTEM.SBYTE":
                    case "SYSTEM.INT16":
                    case "SYSTEM.UINT16":
                    case "SYSTEM.INT32":
                    case "SYSTEM.UINT32":
                    case "SYSTEM.INT64":
                    case "SYSTEM.UINT64":
                    case "SYSTEM.SINGLE":
                    case "SYSTEM.DOUBLE":
                    case "SYSTEM.DECIMAL":
                        return Utilities.GeneralType.NUMBER;

                    case "TIMESTAMP WITHOUT TIME ZONE":
                    case "TIMESTAMP WITH TIME ZONE":
                    case "TIMESTAMPTZ":
                    case "DATETIME":
                    case "TIME WITHOUT TIME ZONE":
                    case "TIME WITH TIME ZONE":
                    case "TIMETZ":
                    case "TIME":
                    case "SYSTEM.DATETIME":
                    case "DATETIME2":
                    case "DATETIMEOFFSET":
                        return Utilities.GeneralType.DATETIME;

                    case "CHAR":
                    case "VARCHAR":
                    case "TEXT":
                    case "NCHAR":
                    case "NVARCHAR":
                    case "NTEXT":
                    case "SYSTEM.CHAR":
                    case "SYSTEM.STRING":
                    case "SYSNAME":
                        return Utilities.GeneralType.STRING;

                    case "BOOLEAN":
                    case "SYSTEM.BOOLEAN":
                        return Utilities.GeneralType.BOOLEAN;

                    default:
                        return Utilities.GeneralType.UNKNOWN;
                }
            }
        }

        bool _isnotnull;
        /// <summary>признак NOT NULL</summary>
        public bool IsNotNull
        {
            get
            {
                return _isnotnull;
            }
            set
            {
                _isnotnull = value; 
                NotifyPropertyChanged("IsNotNull");
            }
        }

        /// <summary>признак NOT NULL - в виде текста ("true" или "false")</summary>
        [JsonIgnore]
        public string IsNotNull_string
        {
            get
            {
                if (_isnotnull == true)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            set
            {
                if (
                    string.IsNullOrWhiteSpace(value) ||
                    (value.Trim().ToLower() != "true")
                )
                {
                    _isnotnull = false;
                }
                else
                {
                    _isnotnull = true;
                }
            }
        }

        /// <summary>NOT NULL - для использования в скриптах</summary>
        public string IsNotNullToScript
        {
            get
            {
                if (this.IsNotNull == true) return "NOT NULL";
                else return "";
            }
        }

        /// <summary>NULL - для использования в скриптах</summary>
        public string IsNullToScript
        {
            get
            {
                if (this.IsNotNull == false) return "NULL";
                else return "";
            }
        }

        bool _isidentity;
        /// <summary>признак identity</summary>
        public bool IsIdentity
        {
            get
            {
                return _isidentity;
            }
            set
            {
                _isidentity = value; 
                NotifyPropertyChanged("IsIdentity");
            }
        }

        /// <summary>признак identity - в виде текста ("true" или "false")</summary>
        [JsonIgnore]
        public string IsIdentity_string
        {
            get { if (_isidentity == true) return "true"; else return "false"; }
            set
            {
                if (
                    string.IsNullOrWhiteSpace(value) ||
                    (value.Trim().ToLower() != "true")
                )
                {
                    _isidentity = false;
                }
                else
                {
                    _isidentity = true;
                }
            }
        }

        /// <summary>identity - для использования в скриптах</summary>
        public string IsIdentityToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.MSSQL:
                        if ((this.IsIdentity == true) && (this.ParentTableDB.TableType != Utilities.TableType.EVN)) return "IDENTITY(1,1)";
                        else return "";
                    case Utilities.TargetDBType.EMD:
                    case Utilities.TargetDBType.PGSQL:
                        if ((this.IsIdentity == true) && (this.ParentTableDB.TableType != Utilities.TableType.EVN)) return "GENERATED BY DEFAULT AS IDENTITY";
                        else return "";
                    default:
                        return "";
                }
            }
        }

        bool _ispk;
        /// <summary>признак primary key</summary>
        public bool IsPK
        {
            get { return _ispk; }
            set { _ispk = value; NotifyPropertyChanged("IsPK"); }
        }

        /// <summary>признак primary key - в виде текста ("true" или "false")</summary>
        [JsonIgnore]
        public string IsPK_string
        {
            get
            {
                if (_ispk == true)
                {
                    return "true";
                }
                else
                {
                    return "false";
                }
            }
            set
            {
                if (
                    string.IsNullOrWhiteSpace(value) ||
                    (value.Trim().ToLower() != "true")
                )
                {
                    _ispk = false;
                }
                else
                {
                    _ispk = true;
                }
            }
        }

        string _field_default;
        /// <summary>значение по умолчанию</summary>
        public string FieldDefault
        {
            get
            {
                return _field_default ?? "";
            }
            set
            {
                _field_default = value;
                if (string.IsNullOrWhiteSpace(_field_default)) _field_default = "";
                _field_default = _field_default.Trim();
                NotifyPropertyChanged("FieldDefault");
            }
        }

        /// <summary>значение по умолчанию - для использования в скриптах</summary>
        public string FieldDefaultToScript
        {
            get
            {
                switch (FieldGeneralType)
                {
                    case Utilities.GeneralType.STRING:
                        return "DEFAULT " + this.FieldDefault; //-V3139
                    case Utilities.GeneralType.DATETIME:
                        return "DEFAULT " + this.FieldDefault;
                    case Utilities.GeneralType.BOOLEAN:
                        return "DEFAULT '" + this.FieldDefault + "'";
                    case Utilities.GeneralType.NUMBER:
                        return "DEFAULT " + this.FieldDefault;
                    default:
                        return "";
                }
            }
        }

        string _fkname;
        /// <summary>Наименование внешнего ключа</summary>
        public string FKName
        {
            get
            {
                return _fkname ?? "";
                /*
                if (!string.IsNullOrWhiteSpace(_fkname)) return _fkname;
                else return ParentTableInfo.GetFKNameDefault(FieldName, FKTable);
                */
            }
            set
            {
                _fkname = value;
                if (string.IsNullOrWhiteSpace(_fkname)) _fkname = "";
                _fkname = _fkname
                    .Replace("\"", String.Empty)
                    .Trim();

                NotifyPropertyChanged("FKName");
            }
        }

        /// <summary>Наименование внешнего ключа - в оригинальном регистре, но без кавычек</summary>
        public string FKNameReady => FKName.Replace("\"", "");

        /// <summary>Наименование внешнего ключа - для сравнения, в нижнем регистре и без кавычек</summary>
        public string FKNameCompare => FKNameReady.ToLower();

        /// <summary>Наименование внешнего ключа - для использования в скриптах</summary>
        public string FKNameToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                        bool isQuotes = this.FKName.StartsWith("\"");
                        if (isQuotes)
                        {
                            return "\"" + this.FKNameReady.Truncate(63) + "\"";
                        }
                        else
                        {
                            return this.FKName.Truncate(63);
                        }
                    case Utilities.TargetDBType.PGSQL:
                        return this.FKNameReady.ToLower().Truncate(63);
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        return this.FKNameReady;
                }
            }
        }

        /// <summary>Наименование внешнего ключа - для поиска в БД</summary>
        public string FKNameToSeek => FKNameToScript.Replace("\"", "");

        /// <summary>Наименование индекса внешнего ключа - для использования в скриптах</summary>
        public string IndexFKNameToScript
        {
            get
            {
                bool isQuotes = this.FKNameToScript.StartsWith("\"");
                string idxname = this.FKNameToScript.Replace("\"", String.Empty);

                if (string.IsNullOrWhiteSpace(idxname))
                {
                    return "";
                }

                if (idxname.StartsWith("fk_"))
                {
                    idxname = "idx_" + idxname.Substring(3);
                }
                else
                {
                    idxname = "idx_" + idxname;
                }

                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.EMD:
                        if (isQuotes)
                        {
                            return "\"" + idxname.Truncate(63) + "\"";
                        }
                        else
                        {
                            return idxname.Truncate(63);
                        }
                    case Utilities.TargetDBType.PGSQL:
                        return idxname.ToLower().Truncate(63);
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        return idxname;
                }
            }
        }

        /// <summary>Полное наименование индекса внешнего ключа - для использования в скриптах</summary>
        public string IndexFullFKNameToScript
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(IndexFKNameToScript))
                {
                    return FKSchemaNameToScript + "." + IndexFKNameToScript;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Наименование индекса внешнего ключа - для поиска в БД</summary>
        public string IndexFKNameToSeek => IndexFKNameToScript.Replace("\"", "");

        string _fktable;
        /// <summary>Таблица внешнего ключа</summary>
        public string FKTable
        {
            get
            {
                return _fktable ?? "";
            }
            set
            {
                _fktable = value;
                if (string.IsNullOrWhiteSpace(_fktable)) _fktable = "";
                _fktable = _fktable.Trim();
                NotifyPropertyChanged("FKTable");
            }
        }

        /// <summary>Полное имя таблица внешнего ключа - для использования в скриптах</summary>
        public string FKFullTableToScript
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FKTableNameToScript))
                {
                    return FKSchemaNameToScript + "." + FKTableNameToScript;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Полное имя таблица внешнего ключа - в оригинальном регистре, но без кавычек</summary>
        public string FKFullTableNameReady => FKFullTableToScript.Replace("\"", "");

        /// <summary>Полное имя таблица внешнего ключа - для сравнения, в нижнем регистре и без кавычек</summary>
        public string FKFullTableNameCompare => FKFullTableNameReady.ToLower();

        /// <summary>Схема таблицы внешнего ключа - для использования в скриптах</summary>
        public string FKSchemaNameToScript
        {
            get
            {
                string res = this.FKTable.Replace("\"", "");
                var arr = res.Split('.');
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.PGSQL:
                        if (arr.Length < 2) return "dbo";
                        else return arr[0].ToLower();
                    case Utilities.TargetDBType.EMD:
                        if (arr.Length < 2) return "\"EMD\"";
                        else return "\"" + arr[0] + "\"";
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        if (arr.Length < 2) return "dbo";
                        else return arr[0];
                }
            }
        }

        /// <summary>Схема таблицы внешнего ключа - в оригинальном регистре, но без кавычек</summary>
        public string FKSchemaNameReady => FKSchemaNameToScript.Replace("\"", "");

        /// <summary>Схема таблицы внешнего ключа - для сравнения, в нижнем регистре и без кавычек</summary>
        public string FKSchemaNameCompare => FKSchemaNameReady.ToLower();

        /// <summary>Имя таблицы внешнего ключа - для использования в скриптах</summary>
        public string FKTableNameToScript
        {
            get
            {
                string res = this.FKTable.Replace("\"", "");
                var arr = res.Split('.');
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.PGSQL:
                        if (arr.Length < 2) return res.ToLower();
                        else return arr[1].ToLower();
                    case Utilities.TargetDBType.EMD:
                        if (arr.Length < 2) return res;
                        else return "\"" + arr[1] + "\"";
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        if (arr.Length < 2) return res;
                        else return arr[1];
                }
            }
        }

        /// <summary>Имя таблицы внешнего ключа - в оригинальном регистре, но без кавычек</summary>
        public string FKTableNameReady => FKTableNameToScript.Replace("\"", "");

        /// <summary>Имя таблицы внешнего ключа - для сравнения, в нижнем регистре и без кавычек</summary>
        public string FKTableNameCompare => FKTableNameReady.ToLower();

        /// <summary>Полное имя таблица внешнего ключа - для поиска в БД</summary>
        public string FKFullTableNameToSeek
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(FKTableNameToSeek))
                {
                    return FKSchemaNameToSeek + "." + FKTableNameToSeek;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Схема таблицы внешнего ключа - для поиска в БД</summary>
        public string FKSchemaNameToSeek => FKSchemaNameToScript.Replace("\"", "");

        /// <summary>Имя таблицы внешнего ключа - для поиска в БД</summary>
        public string FKTableNameToSeek => FKTableNameToScript.Replace("\"", "");

        string _fkorder;
        /// <summary>Порядок поля в констрейне внешнего ключа</summary>
        public string FKOrder
        {
            get
            {
                return _fkorder ?? "";
            }
            set
            {
                _fkorder = value;
                NotifyPropertyChanged("FKOrder");
            }
        }

        /// <summary>
        /// Порядок поля в констрейне внешнего ключа - число
        /// </summary>
        public int FKOrderToInt
        {
            get
            {
                if (int.TryParse(FKOrder, out int _nn))
                {
                    return _nn;
                }

                return 0;
            }
        }

        /// <summary>
        /// Порядок поля в констрейне внешнего ключа - для сортировки
        /// </summary>
        public int FKOrderToSort => FKOrderToInt * 1000000 + FieldOrder;

        string _pkorder;
        /// <summary>Порядок поля в primary key</summary>
        public string PKOrder
        {
            get
            {
                return _pkorder ?? "";
            }
            set
            {
                _pkorder = value;
                NotifyPropertyChanged("PKOrder");
            }
        }

        /// <summary>
        /// Порядок поля в primary key - число
        /// </summary>
        public int PKOrderToInt
        {
            get
            {
                if (int.TryParse(PKOrder, out int _nn))
                {
                    return _nn;
                }

                return 0;
            }
        }

        /// <summary>
        /// Порядок поля в primary key - для сортировки
        /// </summary>
        public int PKOrderToSort => PKOrderToInt * 1000000 + FieldOrder;

        string _fkfield;
        /// <summary>PK таблицы внешнего ключа</summary>
        public string FKField
        {
            get
            {
                return _fkfield ?? "";
            }
            set
            {
                _fkfield = value;
                if (string.IsNullOrWhiteSpace(_fkfield)) _fkfield = "";
                _fkfield = _fkfield.Trim();
                NotifyPropertyChanged("FKField");
            }
        }

        /// <summary>PK таблицы внешнего ключа - в оригинальном регистре, но без кавычек</summary>
        public string FKFieldReady => FKField.Replace("\"", "");

        /// <summary>PK таблицы внешнего ключа - для сравнения, в нижнем регистре и без кавычек</summary>
        public string FKFieldCompare => FKFieldReady.ToLower();

        /// <summary>PK таблицы внешнего ключа - для использования в скриптах</summary>
        public string FKFieldToScript
        {
            get
            {
                switch (this.ParentTableDB.TargetDB)
                {
                    case Utilities.TargetDBType.PGSQL:
                        {
                            foreach (var item in ListEvn
                                .Where(x => 
                                    (x.ToLower() == this.FKTableNameCompare) && 
                                    (this.FKSchemaNameCompare == "dbo")
                                )
                            )
                            {
                                return "evn_id";
                            }

                            return this.FKFieldReady.ToLower();
                        }
                    case Utilities.TargetDBType.EMD:
                        return "\"" + this.FKFieldReady + "\"";
                    case Utilities.TargetDBType.MSSQL:
                    default:
                        return this.FKFieldReady;
                }
            }
        }

        /// <summary>PK таблицы внешнего ключа - для поиска в БД</summary>
        public string FKFieldToSeek => FKFieldToScript.Replace("\"", "");

        string _foreign_column;
        /// <summary>Поле внешней таблицы</summary>
        public string ForeignColumn
        {
            get
            {
                return _foreign_column ?? "";
            }
            set
            {
                _foreign_column = value;
                if (string.IsNullOrWhiteSpace(_foreign_column)) _foreign_column = "";
                _foreign_column = _foreign_column
                    .Replace("'", "")
                    .Replace("column_name ", "")
                    .Trim();
            }
        }

        /// <summary>
        /// Поле внешней таблицы - для скриптов
        /// </summary>
        public string ForeignColumnToScript
        {
            get
            {
                string options = this.ForeignColumn;
                if (
                    this.ParentTableDB.TableEdit.isForeignTable &&
                    string.IsNullOrWhiteSpace(this.ForeignColumn)
                )
                {
                    options = this.FieldName;
                }

                string _schema = this.ParentTableDB.TableEdit.ForeignSchemaFromOptions;

                if (_schema == "EMD")
                {
                    return options.Replace("\"", string.Empty);
                }
                else
                {
                    return options.Replace("\"", string.Empty).ToLower();
                }
            }
        }

        string _fieldcheck;
        /// <summary>CHECK для поля</summary>
        public string FieldCheck
        {
            get
            {
                return _fieldcheck ?? "";
            }
            set
            {
                _fieldcheck = value;
                if (string.IsNullOrWhiteSpace(_fieldcheck)) _fieldcheck = "";
                _fieldcheck = _fieldcheck.Trim();
                NotifyPropertyChanged("FieldCheck");
            }
        }

        /// <summary>
        /// =true учитывать поле при генерации скриптов
        /// </summary>
        public bool IsUsed { get; set; } = true;

        bool _isinherit;
        /// <summary>=true поле унаследовано от родительской таблицы</summary>
        public bool IsInherit
        {
            get { return _isinherit; }
            set { _isinherit = value; NotifyPropertyChanged("isInherit"); }
        }

        /// <summary>поле унаследовано от родительской таблицы - в виде текста ("true" или "false")</summary>
        [JsonIgnore]
        public string IsInherit_string
        {
            get { if (_isinherit == true) return "true"; else return "false"; }
            set
            {
                if (
                    string.IsNullOrWhiteSpace(value) ||
                    (value.Trim().ToLower() != "true")
                )
                {
                    _isinherit = false;
                }
                else
                {
                    _isinherit = true;
                }
            }
        }

        string _inhparenttable;
        /// <summary>Родительская таблица, от которой унаследовано поле</summary>
        public string InheritParentTable
        {
            get
            {
                return _inhparenttable ?? "";
            }
            set
            {
                _inhparenttable = value;
                if (string.IsNullOrWhiteSpace(_inhparenttable)) _inhparenttable = "";
                _inhparenttable = _inhparenttable.Trim();
                _inhparenttable = Utilities.Databases.GetFullTableName(_inhparenttable);
                NotifyPropertyChanged("InheritParentTable");
            }
        }

        /// <summary>
        /// =true - идентичное поле
        /// </summary>
        /// <param name="otherrow">описание сравниваемого поля</param>
        /// <returns></returns>
        public bool FieldEquals(FieldDB otherrow)
        {
            if (otherrow == null) return false;

            if (
                (this.FieldNameToScript == otherrow.FieldNameToScript) &&
                (this.FieldTypeToScript == otherrow.FieldTypeToScript) &&
                (this.FieldSize == otherrow.FieldSize) &&
                (this.FieldDec == otherrow.FieldDec) &&
                (this.IsIdentityToScript == otherrow.IsIdentityToScript) &&
                (this.IsNotNullToScript == otherrow.IsNotNullToScript) &&
                (this.IsNullToScript == otherrow.IsNullToScript) &&
                (this.FieldDefaultToScript == otherrow.FieldDefaultToScript)
                )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// =true - НЕ идентичное поле
        /// </summary>
        /// <param name="otherrow">описание сравниваемого поля</param>
        /// <returns></returns>
        public bool FieldNotEquals(FieldDB otherrow) => !FieldEquals(otherrow);

        /// <summary>
        /// =true - идентичный тип поля
        /// </summary>
        /// <param name="otherrow">описание сравниваемого поля</param>
        /// <returns></returns>
        public bool FieldTypeEquals(FieldDB otherrow)
        {
            if (otherrow == null) return false;

            if (
                (this.FieldTypeToScript == otherrow.FieldTypeToScript) && 
                (this.FieldSize == otherrow.FieldSize) &&
                (this.FieldDec == otherrow.FieldDec)
                )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// =true - НЕ идентичный тип поля
        /// </summary>
        /// <param name="otherrow">описание сравниваемого поля</param>
        /// <returns></returns>
        public bool FieldTypeNotEquals(FieldDB otherrow) => !FieldTypeEquals(otherrow);

        /// <summary>
        /// =true - идентичный FK
        /// </summary>
        /// <param name="otherrow">описание сравниваемого поля</param>
        /// <returns></returns>
        public bool FKEquals(FieldDB otherrow)
        {
            if (otherrow == null) return false;

            if (
                (this.FKFieldToScript == otherrow.FKFieldToScript) && 
                (this.FKNameToScript == otherrow.FKNameToScript) &&
                (this.FKFullTableToScript == otherrow.FKFullTableToScript) &&
                (this.FieldCheck == otherrow.FieldCheck) &&
                (this.FKOrderToSort == otherrow.FKOrderToSort)
                )
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// =true - НЕ идентичный FK
        /// </summary>
        /// <param name="otherrow">описание сравниваемого поля</param>
        /// <returns></returns>
        public bool FKNotEquals(FieldDB otherrow) => !FKEquals(otherrow);

        /// <summary>
        /// Клонирование экземпляра FieldDB
        /// </summary>
        /// <returns></returns>
        public FieldDB Copy()
        {
            FieldDB copy = (FieldDB)this.MemberwiseClone();

            return copy;
        }
    }

    /// <summary>
    /// Информация о поле родительской таблицы
    /// </summary>
    public class ParentFieldInfo
    {
        /// <summary>
        /// =true - информация о поле взято из родительской таблицы
        /// </summary>
        public bool isUpdatedFromParent;
        
        /// <summary>
        /// имя поля
        /// </summary>
        public string Name;
        
        /// <summary>
        /// описание поля
        /// </summary>
        public string Desc;
    }

    /// <summary>
    /// Информация о FK таблицы
    /// </summary>
    public class TableFKInfo
    {
        /// <summary>
        /// имя поля
        /// </summary>
        public string FieldName;

        /// <summary>
        /// номер поля по порядку
        /// </summary>
        public int FieldOrder;

        /// <summary>
        /// имя констрейна
        /// </summary>
        public string FKName;
        
        /// <summary>
        /// таблица констрейна
        /// </summary>
        public string FKTable;

        /// <summary>
        /// порядок поля в FK
        /// </summary>
        public string FKOrder;

        /// <summary>
        /// порядок поля в FK - число
        /// </summary>
        public int FKOrderToInt
        {
            get
            {
                if (int.TryParse(FKOrder, out int _nn))
                {
                    return _nn;
                }

                return 0;
            }
        }

        /// <summary>
        /// порядок поля в FK - для сортировки
        /// </summary>
        public int FKOrderToSort => FKOrderToInt * 1000000 + FieldOrder;

        /// <summary>
        /// primary key таблицы констрейна
        /// </summary>
        public string FKField;
    }

    /// <summary>
    /// Информация о CHECK таблицы
    /// </summary>
    public class TableCHECKInfo
    {
        /// <summary>
        /// имя поля
        /// </summary>
        public string FieldName;
        
        /// <summary>
        /// имя constraint
        /// </summary>
        public string FKName;

        /// <summary>
        /// условие для CHECK
        /// </summary>
        public string FieldCheck;
    }
}



