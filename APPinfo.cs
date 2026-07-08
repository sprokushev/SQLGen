// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace SQLGen
{
    // -------------------------------------------------------------------------------------------------------
    /// <summary>Параметры приложения, сохраняемые в SQLGen.json </summary>
    public class APPinfo
    {
        /// <summary>конструктор класса APPinfo</summary>
        public APPinfo()
        {
            this.GITProjects = new List<GITInfo>();
            this.CheckNoIdTables = new List<string>();
            this.ListDatabases = new BindingList<DBInfo>();
            this.Regions = new Dictionary<string, string>();
            this.GUI = new GUI();
            this.IsNewGen = "true";
            this.NoUpperBranch = new List<string>();
            this.ReleaseBranch = new List<string>();
            this.CumulativeGap = new List<string>();
            this.relativeToChangelogFile = "true";
            this.ExtendedLog = "false";
            this.ImproveSQLinVersion = "true";
            this.UseNewFunc = "false";
            this.CheckLastCommit = "true";
            this.TaskReleaseCooperative = "false";
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список с проектами GIT</summary>
        public List<GITInfo> GITProjects { get; set; } = new List<GITInfo>();

        /// <summary>
        /// Проекты ТОЛЬКО для регионов MS
        /// </summary>
        [JsonIgnore]
        public List<string> ListProjects_ONLY_MS => GITProjects
                        .Where(x =>
                            x.DBRegion == "MS SQL" &&
                            !string.IsNullOrWhiteSpace(x.DEVProject)
                        )
                        .Select(x => x.DEVProject)
                        .Union(
                            GITProjects
                            .Where(x =>
                                x.DBRegion == "MS SQL" &&
                                !string.IsNullOrWhiteSpace(x.GITProject)
                            )
                            .Select(x => x.GITProject)
                        )
                        .Distinct()
                        .ToList();

        /// <summary>
        /// Проекты для регионов MS
        /// </summary>
        [JsonIgnore]
        public List<string> ListProjectsMS => GITProjects
                        .Where(x =>
                            (x.DBRegion == "MS SQL" || x.DBRegion == "ALL") &&
                            !string.IsNullOrWhiteSpace(x.DEVProject)
                        )
                        .Select(x => x.DEVProject)
                        .Union(
                            GITProjects
                            .Where(x =>
                                (x.DBRegion == "MS SQL" || x.DBRegion == "ALL") &&
                                !string.IsNullOrWhiteSpace(x.GITProject)
                            )
                            .Select(x => x.GITProject)
                        )
                        .Distinct()
                        .ToList();

        /// <summary>
        /// Проекты ТОЛЬКО для регионов PG
        /// </summary>
        [JsonIgnore]
        public List<string> ListProjects_ONLY_PG => GITProjects
                        .Where(x =>
                            (x.DBRegion == "PG SQL") &&
                            !string.IsNullOrWhiteSpace(x.DEVProject)
                        )
                        .Select(x => x.DEVProject)
                        .Union(
                            GITProjects
                            .Where(x =>
                                (x.DBRegion == "PG SQL") &&
                                !string.IsNullOrWhiteSpace(x.GITProject)
                            )
                            .Select(x => x.GITProject)
                        )
                        .Distinct()
                        .ToList();

        /// <summary>
        /// Проекты для регионов PG
        /// </summary>
        [JsonIgnore]
        public List<string> ListProjectsPG => GITProjects
                        .Where(x =>
                            (x.DBRegion == "PG SQL" || x.DBRegion == "ALL") &&
                            !string.IsNullOrWhiteSpace(x.DEVProject)
                        )
                        .Select(x => x.DEVProject)
                        .Union(
                            GITProjects
                            .Where(x =>
                                (x.DBRegion == "PG SQL" || x.DBRegion == "ALL") &&
                                !string.IsNullOrWhiteSpace(x.GITProject)
                            )
                            .Select(x => x.GITProject)
                        )
                        .Distinct()
                        .ToList();

        /// <summary>
        /// DBAlias проектов, общих для регионов MS и PG
        /// </summary>
        [JsonIgnore]
        public List<string> ListDBAlias_ForALL => GITProjects
                        .Where(x =>
                            x.DBRegion == "ALL" &&
                            !string.IsNullOrWhiteSpace(x.DBAlias)
                        )
                        .Select(x => x.DBAlias)
                        .Distinct()
                        .ToList();

        /// <summary>
        /// Найти проект GIT
        /// </summary>
        /// <param name="_project">проект GIT</param>
        /// <returns></returns>
        public GITInfo FindGITProject(string _project)
        {
            if (string.IsNullOrWhiteSpace(_project)) return null;

            if (GITProjects.Count == 0) return null;

            return this.GITProjects.Find(x =>
                (x.GITProject.ToLower() == _project.ToLower()) ||
                (x.DEVProject.ToLower() == _project.ToLower())
            );
        }

        /// <summary>
        /// Добавить проект GIT
        /// </summary>
        /// <param name="git_project">"старый" проект GIT</param>
        /// <param name="git_folder">папка "старого" проекта GIT</param>
        /// <param name="_prefixsql">префикс sql-файлов</param>
        /// <param name="_prefixrelease">префикс yml-файла версии</param>
        /// <param name="_postfixrelease">постфикс yml-файла версии</param>
        /// <param name="git_url">url к "старому" проекту GIT</param>
        /// <param name="git_urlalt">альтернативный url к "старому" проекту GIT</param>
        /// <param name="git_ymlfield">поле на форме Сборки релиза для "старого" проекта GIT</param>
        /// <param name="git_datafolder">папка для данных в "старом" проекте GIT</param>
        /// <param name="git_issinglescript">оYES/ДА - Один объект, один скрипт - для "старого" проекта GIT</param>
        /// <param name="_dbtype">тип БД</param>
        /// <param name="dev_project">"новый" проект GIT</param>
        /// <param name="dev_folder">папка "нового" проекта GIT</param>
        /// <param name="dev_url">url к "новому" проекту GIT</param>
        /// <param name="dev_urlalt">альтернативный url к "новому" проекту GIT</param>
        /// <param name="dev_ymlfield">поле на форме Сборки релиза для "нового" проекта GIT</param>
        /// <param name="dev_datafolder">папка для данных в "новом" проекте GIT</param>
        /// <param name="dev_issinglescriptstruct">YES/ДА - Один объект, один скрипт - для таблиц, схем, сиквенсов, индексов, типов "нового" проекта GIT</param>
        /// <param name="dev_issinglescriptcode">YES/ДА - Один объект, один скрипт - для процедур, функций, вьюх, триггеров "нового" проекта GIT</param>
        /// <param name="dev_issinglescriptdata">YES/ДА - Один объект, один скрипт - для данных "нового" проекта GIT</param>
        /// <param name="dev_startver">Номер версии, с которой начали собирать релизы в проекте DEV</param>
        /// <param name="cumulativegap">Список версий, на которых разрывается кумулятивность в данном проекте</param>
        /// <param name="git_isevninherit">YES/ДА - Таблицы событий используют наследование</param>
        /// <param name="git_dbalias">Алиас БД</param>
        /// <param name="git_dbregion">В какой тип региона может быть включен: MS SQL, PG SQL или ALL</param>
        /// <param name="git_LuquibotAliasOld">Алиас бота старый</param>
        /// <param name="git_LuquibotAliasOldUfa">Алиас бота старый Уфа</param>
        /// <param name="git_LuquibotAliasSP">Алиас бота для SP</param>
        /// <param name="git_LuquibotAliasSPUfa">Алиас бота для SP Уфа</param>
        /// <param name="git_LuquibotAliasHF">Алиас для HF</param>
        /// <param name="git_LuquibotAliasHFUfa">Алиас для HF Уфа</param>
        /// <param name="git_LuquibotAliasEHFAct">Алиас для EHF актуального</param>
        /// <param name="git_LuquibotAliasEHFActUfa">Алиас для EHF актуального Уфа</param>
        /// <param name="git_LuquibotAliasEHFUnAct">Алиас для EHF не актуального</param>
        /// <param name="git_LuquibotAliasEHFUnActUfa">Алиас для EHF не актуального Уфа</param>
        /// <param name="git_LuquibotAliasLTS">Алиас для LTS</param>
        /// <param name="git_LuquibotAliasLTSUfa">Алиас для LTS Уфа</param>
        /// <param name="git_LuquibotAliasQARel">Алиас для QA-Rel</param>
        /// <param name="git_LuquibotAliasQARelUfa">Алиас для QA-Rel Уфа</param>
        /// <param name="git_LuquibotAliasQA">Алиас для QA</param>
        /// <param name="git_LuquibotAliasQAUfa">Алиас для QA Уфа</param>
        /// <param name="git_ProjectDeploymentMS">Алиас для EHF2 N2 Уфы</param>
        /// <param name="git_ProjectDeploymentPG">Алиас для EHF2 N2 Уфы</param>
        /// <param name="git_ProjectCronMS">Алиас для EHF2 N2 Уфы</param>
        /// <param name="git_ProjectCronPG">Алиас для EHF2 N2 Уфы</param>
        /// <returns></returns>
        public GITInfo AddGITProject(string git_project, string git_folder, string _prefixsql, string _prefixrelease, string _postfixrelease, string git_url, string git_urlalt, string git_ymlfield, string git_datafolder, string git_issinglescript, string _dbtype, string dev_project, string dev_folder, string dev_url, string dev_urlalt, string dev_ymlfield, string dev_datafolder, string dev_issinglescriptstruct, string dev_issinglescriptcode, string dev_issinglescriptdata, string dev_startver, List<string> cumulativegap, string git_isevninherit, string git_dbalias, string git_dbregion, string git_LuquibotAliasOld, string git_LuquibotAliasOldUfa, string git_LuquibotAliasSP, string git_LuquibotAliasSPUfa, string git_LuquibotAliasHF, string git_LuquibotAliasHFUfa, string git_LuquibotAliasEHFAct, string git_LuquibotAliasEHFActUfa, string git_LuquibotAliasEHFUnAct, string git_LuquibotAliasEHFUnActUfa, string git_LuquibotAliasLTS, string git_LuquibotAliasLTSUfa, string git_LuquibotAliasQARel, string git_LuquibotAliasQARelUfa, string git_LuquibotAliasQA, string git_LuquibotAliasQAUfa, string git_ProjectDeploymentMS, string git_ProjectDeploymentPG, string git_ProjectCronMS, string git_ProjectCronPG
        )
        {
            if (string.IsNullOrWhiteSpace(git_project)) return null;

            if (string.IsNullOrWhiteSpace(_dbtype)) _dbtype = "MSSQL";

            GITInfo _git = FindGITProject(git_project);

            if (_git == null)
            {
                // не найден - добавляем новый
                _git = new GITInfo();
                _git.GITProject = git_project;
                this.GITProjects.Add(_git);
            }

            if ((!string.IsNullOrWhiteSpace(git_folder)) && string.IsNullOrWhiteSpace(_git.GITProjectFolder)) _git.GITProjectFolder = git_folder;
            if ((!string.IsNullOrWhiteSpace(_prefixsql)) && string.IsNullOrWhiteSpace(_git.PrefixFileSQL)) _git.PrefixFileSQL = _prefixsql;
            _git.PrefixFileRelease = _prefixrelease;
            _git.PostfixFileRelease = _postfixrelease;
            _git.GITUrl = git_url;
            _git.GITUrlAlt = git_urlalt;
            _git.GITYMLField = git_ymlfield;
            _git.GITDataFolder = git_datafolder;
            _git.GITisSingleScript = git_issinglescript;
            _git.DBType = _dbtype;
            _git.DEVProject = dev_project;
            if ((!string.IsNullOrWhiteSpace(dev_folder)) && string.IsNullOrWhiteSpace(_git.DEVProjectFolder)) _git.DEVProjectFolder = dev_folder;
            _git.DEVUrl = dev_url;
            _git.DEVUrlAlt = dev_urlalt;
            _git.DEVYMLField = dev_ymlfield;
            _git.DEVDataFolder = dev_datafolder;
            _git.DEVisSingleScriptStruct = dev_issinglescriptstruct;
            _git.DEVisSingleScriptCode = dev_issinglescriptcode;
            _git.DEVisSingleScriptData = dev_issinglescriptdata;
            _git.DEVStartVer = dev_startver;
            _git.isEvnInherit = git_isevninherit;
            _git.DBAlias = git_dbalias;
            _git.DBRegion = git_dbregion;
            _git.LuquibotAliasOld = git_LuquibotAliasOld;
            _git.LuquibotAliasOldUfa = git_LuquibotAliasOldUfa;
            _git.LuquibotAliasSP = git_LuquibotAliasSP;
            _git.LuquibotAliasSPUfa = git_LuquibotAliasSPUfa;
            _git.LuquibotAliasHF = git_LuquibotAliasHF;
            _git.LuquibotAliasHFUfa = git_LuquibotAliasHFUfa;
            _git.LuquibotAliasEHFAct = git_LuquibotAliasEHFAct;
            _git.LuquibotAliasEHFActUfa = git_LuquibotAliasEHFActUfa;
            _git.LuquibotAliasEHFUnAct = git_LuquibotAliasEHFUnAct;
            _git.LuquibotAliasEHFUnActUfa = git_LuquibotAliasEHFUnActUfa;
            _git.LuquibotAliasLTS = git_LuquibotAliasLTS;
            _git.LuquibotAliasLTSUfa = git_LuquibotAliasLTSUfa;
            _git.LuquibotAliasQARel = git_LuquibotAliasQARel;
            _git.LuquibotAliasQARelUfa = git_LuquibotAliasQARelUfa;
            _git.LuquibotAliasQA = git_LuquibotAliasQA;
            _git.LuquibotAliasQAUfa = git_LuquibotAliasQAUfa;
            _git.ProjectDeploymentMS = git_ProjectDeploymentMS;
            _git.ProjectDeploymentPG = git_ProjectDeploymentPG;
            _git.ProjectCronMS = git_ProjectCronMS;
            _git.ProjectCronPG = git_ProjectCronPG;

            var list = (_git.CumulativeGap ?? "").ToList(new char[] { ',', ';' }, true);

            foreach (var item in cumulativegap)
            {
                if (!list.Contains(item.Trim().ToLower())) list.Add(item.Trim().ToLower());
            }

            _git.CumulativeGap = string.Join(",", list);

            return _git;
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список с таблицами, которые надо проверять в скриптах - без явного заполнения поля таблица_id</summary>
        public List<string> CheckNoIdTables { get; set; }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список БД</summary>
        public BindingList<DBInfo> ListDatabases { get; set; }

        /// <summary>
        /// Найти БД
        /// </summary>
        /// <param name="_server">имя или адрес сервера с портом</param>
        /// <param name="_db">имя БД</param>
        /// <param name="_dbtype">тип БД</param>
        /// <returns></returns>
        public DBInfo FindDatabase(string _server, string _db, string _dbtype)
        {
            if (
                    string.IsNullOrWhiteSpace(_server) ||
                    string.IsNullOrWhiteSpace(_db) ||
                    string.IsNullOrWhiteSpace(_dbtype)
            )
            {
                return null;
            }

            if (ListDatabases.Count == 0)
            {
                return null;
            }

            return this.ListDatabases.FirstOrDefault(x =>
                Utilities.Databases.ServerAddrEqual(x.ServerAddr, _server) &&
                Utilities.Databases.ServerPortEqual(x.ServerPort, x.DBType, _server, _dbtype) &&
                Utilities.Databases.DBNameEqual(x.DBName, _db) &&
                Utilities.Databases.DBTypeEqual(x.DBType, _dbtype)
            );
        }


        /// <summary>
        /// Удалить БД
        /// </summary>
        /// <param name="_server">имя или адрес сервера с портом</param>
        /// <param name="_db">имя БД</param>
        /// <param name="_dbtype">тип БД</param>
        public void DelDatabase(string _server, string _db, string _dbtype)
        {
            if (
                    string.IsNullOrWhiteSpace(_server) ||
                    string.IsNullOrWhiteSpace(_db) ||
                    string.IsNullOrWhiteSpace(_dbtype)
            )
            {
                return;
            }

            if (ListDatabases.Count == 0)
            {
                return;
            }

            var _base = this.FindDatabase(_server, _db, _dbtype);
            while (_base != null)
            {
                this.ListDatabases.Remove(_base);
                _base = this.FindDatabase(_server, _db, _dbtype);
            }
        }

        /// <summary>
        /// Добавить БД
        /// </summary>
        /// <param name="_server">имя или адрес сервера с портом</param>
        /// <param name="_db">имя БД</param>
        /// <param name="_project">проект GIT</param>
        /// <param name="_role">роль БД</param>
        /// <param name="_ismaintest">true - основная тестовая БД для проекта GIT</param>
        /// <param name="isForced">=true - гарантировано добавить с указанными параметрами</param>
        /// <param name="editDB">!=null - прямое редактирование экземпляра БД</param>
        /// <returns></returns>
        public DBInfo AddDatabase(string _server, string _db, string _project, string _role, bool _ismaintest, bool isForced, DBInfo editDB = null)
        {
            if (
                    string.IsNullOrWhiteSpace(_server) ||
                    string.IsNullOrWhiteSpace(_db) ||
                    string.IsNullOrWhiteSpace(_project)
            )
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(_role))
            {
                _role = "PROD";
            }

            if (_ismaintest)
            {
                _role = "TEST";
            }

            string _dbtype = Utilities.GITProjects.GetDBTypeByProject(_project);

            if (_dbtype == "MSSQL")
            {
                _server = _server.Replace(":", ",");
            }

            if (_dbtype == "PGSQL")
            {
                _server = _server.Replace(",", ":");
            }

            DBInfo _database = editDB;
            if (_database == null)
            {
                _database = FindDatabase(_server, _db, _dbtype);
            }

            if (_database == null)
            {
                // не найдена - добавляем новую
                _database = new DBInfo();
                _database.ServerName = _server;
                _database.DBName = _db;
                _database.GITProject = _project;
                _database.DBRole = _role;
                _database.DBType = _dbtype;
                _database.isMainTest = false;
                this.ListDatabases.Add(_database);
            }
            else
            {
                // принудительно обновим информацию о БД
                _database.DBRole = _role;
                _database.DBType = _dbtype;

                if (editDB != null)
                {
                    _database.ServerName = _server;
                    _database.DBName = _db;
                    _database.GITProject = _project;
                }
            }

            if (_ismaintest)
            {
                if (isForced)
                {
                    // сбросим у всех БД этого проекта признак основной тестовой БД
                    foreach (var item in ListDatabases
                        .Where(x =>
                            (x.GITProject == _project) &&
                            x.isMainTest
                        )
                    )
                    {
                        item.isMainTest = false;
                    }

                    // принудительно ставим добавленную БД как основную тестовую для проекта GIT
                    _database.isMainTest = _ismaintest;
                }
                else
                {
                    // проставляем признак основной тестовой БД для проекта GIT только если еще нет других БД с этим признаком
                    var _found = MainWindow.APPinfo.ListDatabases.FirstOrDefault(x =>
                        x.GITProject == _project &&
                        x.isMainTest &&
                        !x.Equals(_database)
                        );
                    if (_found != null)
                    {
                        // уже есть другая основная тестовая БД для этого проекта GIT
                    }
                    else
                    {
                        _database.isMainTest = true;
                    }
                }
            }

            return _database;
        }

        /// <summary>
        /// удалить дубликаты БД
        /// </summary>
        public void DelDublicateDatabases()
        {
            // копируем текущий список и меняем "старые" проекты на "новые"
            BindingList<DBInfo> old = new BindingList<DBInfo>();
            foreach (var item in ListDatabases)
            {
                var db = item.Copy();
                if (Utilities.GITProjects.IsGITProject(db.GITProject))
                {
                    db.GITProject = Utilities.GITProjects.GetDEVProject(db.GITProject);
                }
                old.Add(db);
            }

            // очищаем текущий список
            ListDatabases.Clear();

            // заново заполняем текущий список
            foreach (var item in old
                // сортируем
                .OrderBy(x => (x.DBRoleType == DBRoleType.TEST ? "1" : "2") + " " + x.DBRole + " " + x.GITProject + " " + (x.isMainTest ? "1" : "2") + " " + x.DBName)
            )
            {
                AddDatabase(item.ServerName, item.DBName, item.GITProject, item.DBRole, item.isMainTest, item.isMainTest);
            }
        }


        /// <summary>
        /// Изменить адрес сервера во всех существующих подключениях и в списке БД
        /// </summary>
        /// <param name="from_server">с адреса</param>
        /// <param name="to_server">на адрес</param>
        /// <param name="dbtype">тип БД</param>
        public void ChangeServerInAllConnects(string from_server, string to_server, string dbtype)
        {
            // изменяем адрес сервера БД в списке подключений
            foreach (var item in MainWindow.ListConnects.Where(x =>
                Utilities.Databases.ServerAddrEqual(x.ServerAddr, from_server) &&
                Utilities.Databases.ServerPortEqual(x.ServerPort, x.DBType, from_server, dbtype) &&
                 Utilities.Databases.DBTypeEqual(x.DBType, dbtype)
            ))
            {
                item.ServerName = item.ServerName.Replace(from_server, to_server);
                item.DBConnectionName = item.DBConnectionName.Replace(from_server, to_server);
            }

            // перебираем имена БД
            foreach (var _db in ListDatabases.Where(x =>
                Utilities.Databases.ServerAddrEqual(x.ServerAddr, from_server) &&
                Utilities.Databases.ServerPortEqual(x.ServerPort, x.DBType, from_server, dbtype) &&
                Utilities.Databases.DBTypeEqual(x.DBType, dbtype)
            ).Select(x => x.DBName).Distinct())
            {
                // проверяем наличие одноименной БД по адресу to_server
                var _found = FindDatabase(to_server, _db, dbtype);
                if (_found == null)
                {
                    // если такой БД нет - меняем адрес
                    while (true)
                    {
                        _found = FindDatabase(from_server, _db, dbtype);
                        if (_found == null)
                        {
                            break;
                        }
                        else
                        {
                            _found.ServerName = _found.ServerName.Replace(from_server, to_server);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Перенос БД на другой сервер
        /// </summary>
        /// <param name="from_server">с адреса</param>
        /// <param name="from_db">имя БД до переноса</param>
        /// <param name="to_server">на адрес</param>
        /// <param name="to_db">имя БД после переноса</param>
        /// <param name="dbtype">тип БД</param>
        public void MoveDBtoOtherServer(string from_server, string from_db, string to_server, string to_db, string dbtype)
        {
            // изменяем адрес сервера БД в списке подключений
            foreach (var item in MainWindow.ListConnects.Where(x =>
                Utilities.Databases.DBNameEqual(x.DBName, from_db) &&
                Utilities.Databases.ServerAddrEqual(x.ServerAddr, from_server) &&
                Utilities.Databases.ServerPortEqual(x.ServerPort, x.DBType, from_server, dbtype) &&
                Utilities.Databases.DBTypeEqual(x.DBType, dbtype)
            ))
            {
                item.ServerName = item.ServerName.Replace(from_server, to_server); ;
                item.DBName = to_db;

                item.DBConnectionName = item.DBConnectionName.Replace(from_server, to_server).Replace(from_db, to_db);
            }

            // проверяем наличие to_db по адресу to_server
            var _found = FindDatabase(to_server, to_db, dbtype);
            if (_found == null)
            {
                // если такой БД нет - меняем адрес
                while (true)
                {
                    _found = FindDatabase(from_server, from_db, dbtype);
                    if (_found == null)
                    {
                        break;
                    }
                    else
                    {
                        _found.ServerName = _found.ServerName.Replace(from_server, to_server);
                        _found.DBName = to_db;
                    }
                }
            }
        }

        /// <summary>
        /// Добавить регион
        /// </summary>
        /// <param name="_num">номер региона</param>
        /// <param name="_name">название региона</param>
        public void AddRegion(string _num, string _name)
        {
            if (string.IsNullOrWhiteSpace(_num) || string.IsNullOrWhiteSpace(_name)) return;

            if (this.Regions.ContainsKey(_num)) return;

            this.Regions.Add(_num, _name);
        }

        // -------------------------------------------------------------------------------------------------------
        private string _taskfolder;
        /// <summary>Каталог задач</summary>
        public string TaskFolder
        {
            get
            {
                //if (!string.IsNullOrWhiteSpace(_taskfolder)) return _taskfolder;
                //else return Utilities.GetRegistryValue(MainWindow.keyName, "TaskFolder", "");
                return _taskfolder ?? "";
            }
            set
            {
                _taskfolder = value;
                if (string.IsNullOrWhiteSpace(_taskfolder)) _taskfolder = "";
                _taskfolder = _taskfolder.Trim();

                //Utilities.SetRegistryValue(MainWindow.keyName, "TaskFolder", _taskfolder);

                MainWindow.Task.OnPropertyChanged("TaskPath");
                MainWindow.Task.OnPropertyChanged("LogFile");
                MainWindow.Task.OnPropertyChanged("TaskFile");
                MainWindow.Task.OnPropertyChanged("TaskSQL");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _gitfolder;
        /// <summary>Каталог GIT</summary>
        public string GITFolder
        {
            get
            {
                //if (!string.IsNullOrWhiteSpace(_gitfolder)) return _gitfolder;
                //else return Utilities.GetRegistryValue(MainWindow.keyName, "GITFolder", "");
                return _gitfolder ?? "";
            }
            set
            {
                _gitfolder = value;
                if (string.IsNullOrWhiteSpace(_gitfolder)) _gitfolder = "";
                _gitfolder = _gitfolder.Trim();

                //Utilities.SetRegistryValue(MainWindow.keyName, "GITFolder", _gitfolder);
            }
        }


        // -------------------------------------------------------------------------------------------------------
        private string _taskexecutor;
        /// <summary>Исполнитель задачи (автор скриптов, входящих в задачу)</summary>
        public string TaskExecutor
        {
            get
            {
                //if (!string.IsNullOrWhiteSpace(_taskexecutor)) return _taskexecutor;
                //else return Utilities.GetRegistryValue(MainWindow.keyName, "TaskExecutor", "");
                return _taskexecutor ?? "";
            }
            set
            {
                _taskexecutor = value;
                if (string.IsNullOrWhiteSpace(_taskexecutor)) _taskexecutor = "";
                _taskexecutor = _taskexecutor.Trim();

                //Utilities.SetRegistryValue(MainWindow.keyName, "TaskExecutor", _taskexecutor);

                MainWindow.Task.OnPropertyChanged("TaskExecutor");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _taskurl;
        /// <summary>Начало URL для задач в Jira (без номера задачи)</summary>
        public string TaskUrlDefault
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_taskurl)) return _taskurl;
                //else return Utilities.GetRegistryValue(MainWindow.keyName, "TaskUrlDefault", "https://jira.rtmis.ru/browse/");
                else return "https://jira.rtmis.ru/browse/";
            }
            set
            {
                _taskurl = value;
                if (string.IsNullOrWhiteSpace(_taskurl)) _taskurl = "";
                _taskurl = _taskurl.Trim();

                //Utilities.SetRegistryValue(MainWindow.keyName, "TaskUrlDefault", _taskurl);

                MainWindow.Task.OnPropertyChanged("TaskUrl");
            }
        }


        // -------------------------------------------------------------------------------------------------------
        private string _conn;
        /// <summary>Последнее соединение</summary>
        public string LastDBConnectionName
        {
            get
            {
                //if (!string.IsNullOrWhiteSpace(_conn)) return _conn;
                //else return Utilities.GetRegistryValue(MainWindow.keyName, "LastDBConnectionName", "");

                return _conn ?? "";
            }
            set
            {
                _conn = value;
                if (string.IsNullOrWhiteSpace(_conn)) _conn = "";
                _conn = _conn.Trim();

                //Utilities.SetRegistryValue(MainWindow.keyName, "LastDBConnectionName", _conn);

                //MainWindow.Task.OnPropertyChanged("TaskUrl");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _fileeditor;
        /// <summary>Редактор файлов</summary>
        public string FileEditor
        {
            get
            {
                return _fileeditor ?? "";
            }
            set
            {
                _fileeditor = value;
                if (string.IsNullOrWhiteSpace(_fileeditor)) _fileeditor = "";
                _fileeditor = _fileeditor.Trim();

                MainWindow.Task.OnPropertyChanged("FileEditor");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _direditor;
        /// <summary>Редактор файловых папок</summary>
        public string DirectoryEditor
        {
            get
            {
                return _direditor ?? "";
            }
            set
            {
                _direditor = value;
                if (string.IsNullOrWhiteSpace(_direditor)) _direditor = "";
                _direditor = _direditor.Trim();

                MainWindow.Task.OnPropertyChanged("DirectoryEditor");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _usernamejira;
        /// <summary>Пользователь Jira</summary>
        public string UsernameJira
        {
            get
            {
                return _usernamejira ?? "";
            }
            set
            {
                _usernamejira = value;
                if (string.IsNullOrWhiteSpace(_usernamejira)) _usernamejira = "";
                _usernamejira = _usernamejira.Trim();

                MainWindow.Task.OnPropertyChanged("UsernameJira");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Зашифрованный пароль Jira</summary>
        public string CryptedPasswordJira { get; set; }

        /// <summary>Пароль Jira</summary>
        [JsonIgnore]
        internal string PasswordJira
        {
            get
            {
                string decrypt = "";

                try
                {
                    decrypt = CryptoClass.decrypt_from_string(CryptedPasswordJira);
                }
                catch (Exception ex)
                {
                    App.AddLog("Ошибка дешифровки пароля, надо его ввести повторно: ", ex, App.ShowMessageMode.SHOW, true, "");
                    decrypt = "";
                }

                return decrypt;
            }
            set
            {
                CryptedPasswordJira = "";

                try
                {
                    CryptedPasswordJira = CryptoClass.encrypt_to_string(value);
                }
                catch (Exception ex)
                {
                    App.AddLog("Ошибка шифровки пароля, надо его ввести повторно: ", ex, App.ShowMessageMode.SHOW, true, "");
                    CryptedPasswordJira = "";
                }
            }
        }

        /// <summary>Флаг сохранения пароля Jira</summary>
        public bool isSavePasswordJira { get; set; }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Список регионов
        /// </summary>
        public Dictionary<string, string> Regions { get; set; }


        // -------------------------------------------------------------------------------------------------------
        private string _isaddtogit;
        /// <summary>
        /// Флаг добавления в "старый" проект
        /// </summary>
        public string IsAddToGIT
        {
            get
            {
                return _isaddtogit ?? "true";
            }
            set
            {
                _isaddtogit = value;
                if (string.IsNullOrWhiteSpace(_isaddtogit)) _isaddtogit = "true";
                _isaddtogit = _isaddtogit.Trim().ToLower();

                if (
                    (_isaddtogit == "да") ||
                    (_isaddtogit == "yes") ||
                    (_isaddtogit == "true")
                    )
                {
                    _isaddtogit = "true";
                }
                else
                {
                    _isaddtogit = "false";
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _isaddtodev;
        /// <summary>
        /// Флаг добавления в "новый" проект
        /// </summary>
        public string IsAddToDEV
        {
            get
            {
                return _isaddtodev ?? "true";
            }
            set
            {
                _isaddtodev = value;
                if (string.IsNullOrWhiteSpace(_isaddtodev)) _isaddtodev = "true";
                _isaddtodev = _isaddtodev.Trim().ToLower();

                if (
                    (_isaddtodev == "да") ||
                    (_isaddtodev == "yes") ||
                    (_isaddtodev == "true")
                    )
                {
                    _isaddtodev = "true";
                }
                else
                {
                    _isaddtodev = "false";
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _isnewgen;
        /// <summary>
        /// Флаг использования новых генераторов
        /// </summary>
        public string IsNewGen
        {
            get
            {
                return _isnewgen ?? "true";
            }
            set
            {
                _isnewgen = value;
                if (string.IsNullOrWhiteSpace(_isnewgen)) _isnewgen = "true";
                _isnewgen = _isnewgen.Trim().ToLower();

                if (
                    (_isnewgen == "да") ||
                    (_isnewgen == "yes") ||
                    (_isnewgen == "true")
                    )
                {
                    _isnewgen = "true";
                }
                else
                {
                    _isnewgen = "false";
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _relativeToChangelogFile;
        /// <summary>
        /// true (по умолчанию) - при сохранении yml-файла пути внутри него будут записаны в относительном виде от места расположения yml-файла
        /// false - при сохранении yml-файла пути внутри него будут записаны в абсолютном виде от места расположения корня проекта
        /// </summary>
        public string relativeToChangelogFile
        {
            get
            {
                return _relativeToChangelogFile ?? "true";
            }
            set
            {
                _relativeToChangelogFile = value;
                if (string.IsNullOrWhiteSpace(_relativeToChangelogFile)) _relativeToChangelogFile = "true";
                _relativeToChangelogFile = _relativeToChangelogFile.Trim().ToLower();

                if (
                    (_relativeToChangelogFile == "да") ||
                    (_relativeToChangelogFile == "yes") ||
                    (_relativeToChangelogFile == "true")
                    )
                {
                    _relativeToChangelogFile = "true";
                }
                else
                {
                    _relativeToChangelogFile = "false";
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _extendedlog;
        /// <summary>
        /// true - включить расширенное логирование
        /// false - отключить расширенное логирование
        /// </summary>
        public string ExtendedLog
        {
            get
            {
                return _extendedlog ?? "true";
            }
            set
            {
                _extendedlog = value;
                if (string.IsNullOrWhiteSpace(_extendedlog)) _extendedlog = "true";
                _extendedlog = _extendedlog.Trim().ToLower();

                if (
                    (_extendedlog == "да") ||
                    (_extendedlog == "yes") ||
                    (_extendedlog == "true")
                    )
                {
                    _extendedlog = "true";
                }
                else
                {
                    _extendedlog = "false";
                }
            }
        }

        /// <summary>
        /// =true - расширенное логирование
        /// </summary>
        public bool isExtendedLog => this.ExtendedLog == "true";

        // -------------------------------------------------------------------------------------------------------
        private string _improvesqlinversion;
        /// <summary>
        /// true - включить улучшение скриптов релиза (метки, changeset и пр.)
        /// false - отключить улучшение скриптов релиза (метки, changeset и пр.)
        /// </summary>
        public string ImproveSQLinVersion
        {
            get
            {
                return _improvesqlinversion ?? "true";
            }
            set
            {
                _improvesqlinversion = value;
                if (string.IsNullOrWhiteSpace(_improvesqlinversion)) _improvesqlinversion = "true";
                _improvesqlinversion = _improvesqlinversion.Trim().ToLower();

                if (
                    (_improvesqlinversion == "да") ||
                    (_improvesqlinversion == "yes") ||
                    (_improvesqlinversion == "true")
                    )
                {
                    _improvesqlinversion = "true";
                }
                else
                {
                    _improvesqlinversion = "false";
                }
            }
        }

        /// <summary>
        /// =true - улучшение скриптов релиза (метки, changeset и пр.)
        /// </summary>
        public bool isImproveSQLinVersion => this.ImproveSQLinVersion == "true";

        // -------------------------------------------------------------------------------------------------------
        private string _usenewfunc;
        /// <summary>
        /// true - включить использование p_SetNotNull и p_FKCreate в скриптах
        /// false - отключить использование p_SetNotNull и p_FKCreate в скриптах
        /// </summary>
        public string UseNewFunc
        {
            get
            {
                return _usenewfunc ?? "true";
            }
            set
            {
                _usenewfunc = value;
                if (string.IsNullOrWhiteSpace(_usenewfunc)) _usenewfunc = "true";
                _usenewfunc = _usenewfunc.Trim().ToLower();

                if (
                    (_usenewfunc == "да") ||
                    (_usenewfunc == "yes") ||
                    (_usenewfunc == "true")
                    )
                {
                    _usenewfunc = "true";
                }
                else
                {
                    _usenewfunc = "false";
                }
            }
        }

        /// <summary>
        /// =true - использовать p_SetNotNull и p_FKCreate в скриптах
        /// </summary>
        public bool isUseNewFunc => this.UseNewFunc == "true";

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Пользовательские настройки интерфейса
        /// </summary>
        public GUI GUI { get; set; }

        // -------------------------------------------------------------------------------------------------------
        private string _dbfconn;
        /// <summary>Строка подключения к DBF через ODBC</summary>
        public string DBFConn
        {
            get
            {
                return _dbfconn ?? "Driver={Microsoft dBase Driver (*.dbf)};DBQ=%PATH%;";
            }
            set
            {
                _dbfconn = value;
                if (string.IsNullOrWhiteSpace(_dbfconn)) _dbfconn = "Driver={Microsoft dBase Driver (*.dbf)};DBQ=%PATH%;";
                _dbfconn = _dbfconn.Trim();

                MainWindow.Task.OnPropertyChanged("DBFConn");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список веток, имя которых не надо приводить в верхний регистр</summary>
        public List<string> NoUpperBranch { get; set; }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список веток, от которых можно создавать релизы</summary>
        public List<string> ReleaseBranch { get; set; }

        /// <summary>
        /// (ПАРАМЕТР УСТАРЕЛ и НЕ ИСПОЛЬЗУЕТСЯ) Список версий, на которых разрывается кумулятивность
        /// </summary>
        public List<string> CumulativeGap { get; set; }

        // -------------------------------------------------------------------------------------------------------
        private string _checklastcommit;
        /// <summary>
        /// true - проверять дату последнего коммита при Merge
        /// false - НЕ проверять дату последнего коммита при Merge
        /// </summary>
        public string CheckLastCommit
        {
            get
            {
                return _checklastcommit ?? "true";
            }
            set
            {
                _checklastcommit = value;
                if (string.IsNullOrWhiteSpace(_checklastcommit)) _checklastcommit = "true";
                _checklastcommit = _checklastcommit.Trim().ToLower();

                if (
                    (_checklastcommit == "да") ||
                    (_checklastcommit == "yes") ||
                    (_checklastcommit == "true")
                    )
                {
                    _checklastcommit = "true";
                }
                else
                {
                    _checklastcommit = "false";
                }
            }
        }

        /// <summary>
        /// =true - добавлять метки в скрипты
        /// </summary>
        public bool isCheckLastCommit => this.CheckLastCommit == "true";

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// через сколько минут повторно выполнять git-refresh.sh
        /// </summary>
        public int GitRefreshDelay { get; set; } = 3;

        // -------------------------------------------------------------------------------------------------------
        private string _taskreleasecooperative;
        /// <summary>
        /// true - использовать проект sqlgen-release для хранения файлов *.task релизных задач (кооперативный режим)
        /// false - работать с релизными задачами индивидуально, без использования проекта sqlgen-release
        /// </summary>
        public string TaskReleaseCooperative
        {
            get
            {
                return _taskreleasecooperative ?? "true";
            }
            set
            {
                _taskreleasecooperative = value;
                if (string.IsNullOrWhiteSpace(_taskreleasecooperative)) _taskreleasecooperative = "true";
                _taskreleasecooperative = _taskreleasecooperative.Trim().ToLower();

                if (
                    (_taskreleasecooperative == "да") ||
                    (_taskreleasecooperative == "yes") ||
                    (_taskreleasecooperative == "true")
                    )
                {
                    _taskreleasecooperative = "true";
                }
                else
                {
                    _taskreleasecooperative = "false";
                }
            }
        }

        /// <summary>
        /// каталог проекта sqlgen-release
        /// </summary>
        public string SqlGenReleasePath => Path.Combine(MainWindow.APPinfo.GITFolder, "sqlgen-release");

        /// <summary>
        /// папка task в проекте sqlgen-release
        /// </summary>
        public string TaskReleasePath => Path.Combine(SqlGenReleasePath, "task");

        /// <summary>
        /// =true - использовать проект sqlgen-release для хранения файлов *.task релизных задач (кооперативный режим)
        /// </summary>
        public bool isTaskReleaseCooperative =>
            this.TaskReleaseCooperative == "true" && // если включен кооперативный режим работы с релизными задачами
            Directory.Exists(this.TaskReleasePath) // есть проект для кооперативного режима и папка для хранения файлов
        ;
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary> Главное окно программы </summary>
    public partial class MainWindow
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Параметры приложения из файла SQLGen.json</summary>
        public static APPinfo APPinfo = new APPinfo();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Флаг "Запуск приложения"</summary>
        public static bool isStartup = true;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Проверка наличия всех обязательных настроек</summary>
        public bool IsSettingsOk()
        {
            return (!string.IsNullOrWhiteSpace(APPinfo.TaskFolder)) && (!string.IsNullOrWhiteSpace(MainWindow.APPinfo.GITFolder));
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Сохранить параметры приложения в SQLGen.json</summary>
        public static void SaveAPPinfo()
        {
            string filename = System.IO.Path.Combine(App.AppPath, "SQLGen.json");

            if (File.Exists(filename)) Utilities.Files.BackupFile(filename);

            try
            {
                string jsonString = JsonSerializer.Serialize<APPinfo>(APPinfo, Other.OptionsJSON);
                if (!APPinfo.isSavePasswordJira)
                {
                    APPinfo info = JsonSerializer.Deserialize<APPinfo>(jsonString, Other.oldOptionsJSON);
                    info.CryptedPasswordJira = "";
                    jsonString = JsonSerializer.Serialize<APPinfo>(info, Other.OptionsJSON);
                }
                File.WriteAllText(filename, jsonString);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, null);
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Считать параметры приложения из SQLGen.json</summary>
        public static void LoadAPPinfo()
        {

            string filename = System.IO.Path.Combine(App.AppPath, "SQLGen.json");

            if (File.Exists(filename))
            {
                try
                {
                    string jsonString = File.ReadAllText(filename);
                    APPinfo = JsonSerializer.Deserialize<APPinfo>(jsonString, Other.oldOptionsJSON);
                    App.AddLog("загружен SQLGen.json", null, App.ShowMessageMode.NONE, true, null);
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, null);
                }
            }

            // принудительно включаем улучшение скриптов релиза
            APPinfo.ImproveSQLinVersion = "true";

            if (APPinfo.GITProjects.Count() == 0)
            {
                App.AddLog("нет файла SQLGen.json либо в SQLGen.json нет перечня проектов GIT, используем значения по умолчанию", null, App.ShowMessageMode.NONE, true, null);
            }

            // обновим список проектов GIT
            APPinfo.AddGITProject(
                "msdbupdate_new",
                "msdbupdate_new",
                "ms",
                "prmd",
                "",
                "https://git.promedweb.ru/msdbteam/msdbupdate_new/-/blob/release/",
                "https://git.promedweb.ru/msdbteam/msdbupdate_new/-/tree/release/",
                "YMLFile_MS",
                "data",
                "NO",
                "MSSQL",
                "dev_promed_ms",
                "dev_promed_ms",
                "https://git.promedweb.ru/msdbteam/dev_promed_ms/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/msdbteam/dev_promed_ms/-/tree/%BRANCH%/",
                "YMLFile_dev_MS",
                "data",
                "YES",
                "YES",
                "NO",
                "11.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0", "12.0.0" },
                "NO",
                "promed",
                "MS SQL",
                "rel_promed_ms",
                "rel_promed_ms_ufa",
                "rel_promed_ms_sp1",
                "rel_promed_ms_ufa_sp1",
                "rel_promed_ms_hf",
                "rel_promed_ms_ufa_hf",
                "rel_promed_ms_ehf_act",
                "rel_promed_ms_ufa_ehf_act",
                "rel_promed_ms_ehf_unact",
                "rel_promed_ms_ufa_ehf_unact",
                "rel_promed_ms_lts",
                "rel_promed_ms_ufa_lts",
                "",
                "",
                "",
                "",
                "dev_promed_ms",
                "",
                "dev_promed_ms",
                ""
                );

            APPinfo.AddGITProject(
                "liquibase_project_new",
                "liquibase_project_new",
                "pg",
                "prmd",
                "",
                "https://git.promedweb.ru/postgresql_group/liquibase_project_new/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/liquibase_project_new/-/tree/release/",
                "YMLFile_PG",
                "data_new",
                "NO",
                "PGSQL",
                "dev_promed_pg",
                "dev_promed_pg",
                "https://git.promedweb.ru/postgresql_group/dev_promed_pg/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/postgresql_group/dev_promed_pg/-/tree/%BRANCH%/",
                "YMLFile_dev_PG",
                "data",
                "YES",
                "YES",
                "NO",
                "11.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0", "12.0.0" },
                "NO",
                "promed",
                "PG SQL",
                git_LuquibotAliasOld: "rel_promed_pg",
                "",
                "rel_promed_pg_sp1",
                "",
                "rel_promed_pg_hf",
                "",
                "rel_promed_pg_ehf_act",
                "",
                "rel_promed_pg_ehf_unact",
                "",
                "rel_promed_pg_lts",
                "",
                "rel_qa_promed",
                "",
                "qa_promed",
                "",
                "",
                "dev_promed_pg",
                "",
                "dev_promed_pg"
                );

            APPinfo.AddGITProject(
                "emd",
                "emd",
                "emd",
                "prmd",
                "_EMD",
                "https://git.promedweb.ru/postgresql_group/emd/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/emd/-/tree/release/",
                "YMLFile_EMD",
                "data",
                "NO",
                "PGSQL",
                "dev_emd_pg",
                "dev_emd_pg",
                "https://git.promedweb.ru/postgresql_group/dev_emd_pg/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/postgresql_group/dev_emd_pg/-/tree/%BRANCH%/",
                "YMLFile_dev_EMD",
                "data",
                "YES",
                "YES",
                "NO",
                "11.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0", "12.0.0" },
                "NO",
                "emd",
                "ALL",
                "rel_emd_pg",
                "",
                "rel_emd_pg_sp1",
                "",
                "rel_emd_pg_hf",
                "",
                "rel_emd_pg_ehf_act",
                "",
                "rel_emd_pg_ehf_unact",
                "",
                "rel_emd_pg_lts",
                "",
                "rel_qa_EMD",
                "",
                "qa_EMD",
                "",
                "dev_promed_ms",
                "dev_promed_pg",
                "dev_promed_ms",
                "dev_promed_pg"
                );

            APPinfo.AddGITProject(
                "promedlistest2",
                "promedlistest2",
                "lis",
                "prmd",
                "_LIS",
                "https://git.promedweb.ru/postgresql_group/promedlistest2/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/promedlistest2/-/tree/release/",
                "YMLFile_LIS",
                "data",
                "NO",
                "PGSQL",
                "dev_lis_pg",
                "dev_lis_pg",
                "https://git.promedweb.ru/postgresql_group/dev_lis_pg/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/postgresql_group/dev_lis_pg/-/tree/%BRANCH%/",
                "YMLFile_dev_LIS",
                "data",
                "YES",
                "YES",
                "NO",
                "11.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0", "12.0.0" },
                "YES",
                "lis",
                "MS SQL",
                "rel_lis_pg",
                "rel_lis_pg_ufa",
                "rel_lis_pg_sp1",
                "rel_lis_pg_ufa_sp1",
                "rel_lis_pg_hf",
                "rel_lis_pg_ufa_hf",
                "rel_lis_pg_ehf_act",
                "rel_lis_pg_ufa_ehf_act",
                "rel_lis_pg_ehf_unact",
                "rel_lis_pg_ufa_ehf_unact",
                "rel_lis_pg_lts",
                "rel_lis_pg_ufa_lts",
                "",
                "",
                "",
                "",
                "",
                "dev_promed_pg",
                "",
                "dev_promed_pg"
                );

            APPinfo.AddGITProject(
                "log_service_pg",
                "log_service",
                "logpg",
                "prmd",
                "_LOG_SERVICE",
                "https://git.promedweb.ru/postgresql_group/log_service/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/log_service/-/tree/release/",
                "YMLFile_log_service_pg",
                "data",
                "NO",
                "PGSQL",
                "dev_logservice_pg",
                "dev_logservice_pg",
                 "https://git.promedweb.ru/postgresql_group/dev_logservice_pg/-/blob/%BRANCH%/",
                 "https://git.promedweb.ru/postgresql_group/dev_logservice_pg/-/tree/%BRANCH%/",
                 "YMLFile_dev_log_service_pg",
                "data",
                "YES",
                "YES",
                "NO",
                "10.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0" },
                "NO",
                "log_service",
                "PG SQL",
                "rel_logservice_pg",
                "",
                "rel_logservice_pg_sp1",
                "",
                "rel_logservice_pg_hf",
                "",
                "rel_logservice_pg_ehf_act",
                "",
                "rel_logservice_pg_ehf_unact",
                "",
                "rel_logservice_pg_lts",
                "",
                "rel_qa_log_service",
                "",
                "qa_log_service",
                "",
                "",
                "dev_promed_pg",
                "",
                "dev_promed_pg"
                );

            APPinfo.AddGITProject(
                "log_service_ms",
                "log_service_ms",
                "logms",
                "prmd",
                "_LOG_SERVICE",
                "https://git.promedweb.ru/msdbteam/log_service/-/blob/master/",
                "https://git.promedweb.ru/msdbteam/log_service/-/tree/master/",
                "YMLFile_log_service_ms",
                "data",
                "NO",
                "MSSQL",
                "dev_logservice_ms",
                "dev_logservice_ms",
                "https://git.promedweb.ru/msdbteam/dev_logservice_ms/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/msdbteam/dev_logservice_ms/-/tree/%BRANCH%/",
                "YMLFile_dev_log_service_ms",
                "data",
                "YES",
                "YES",
                "NO",
                "10.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0" },
                "NO",
                "log_service",
                "MS SQL",
                "rel_logservice_ms",
                "",
                "rel_logservice_ms_sp1",
                "",
                "rel_logservice_ms_hf",
                "",
                "rel_logservice_ms_ehf_act",
                "",
                "rel_logservice_ms_ehf_unact",
                "",
                "rel_logservice_ms_lts",
                "",
                "",
                "",
                "",
                "",
                "dev_promed_ms",
                "",
                "dev_promed_ms",
                ""
                );

            APPinfo.AddGITProject(
                "php_log_pg",
                "php_log",
                "phppg",
                "prmd",
                "_PHP_LOG",
                "https://git.promedweb.ru/postgresql_group/php_log/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/php_log/-/tree/release/",
                "YMLFile_php_log_pg",
                "data",
                "NO",
                "PGSQL",
                "dev_phplog_pg",
                "dev_phplog_pg",
                "https://git.promedweb.ru/postgresql_group/dev_phplog_pg/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/postgresql_group/dev_phplog_pg/-/tree/%BRANCH%/",
                "YMLFile_dev_php_log_pg",
                "data",
                "YES",
                "YES",
                "NO",
                "10.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0" },
                "NO",
                "php_log",
                "PG SQL",
                "rel_phplog_pg",
                "",
                "rel_phplog_pg_sp1",
                "",
                "rel_phplog_pg_hf",
                "",
                "rel_phplog_pg_ehf_act",
                "",
                "rel_phplog_pg_ehf_unact",
                "",
                "rel_phplog_pg_lts",
                "",
                "qa_rel_php_log",
                "",
                "qa_php_log",
                "",
                "",
                "dev_promed_pg",
                "",
                "dev_promed_pg"
                );

            APPinfo.AddGITProject(
                "php_log_ms",
                "php_log_ms",
                "phpms",
                "prmd",
                "_PHP_LOG",
                "https://git.promedweb.ru/msdbteam/php_log/-/blob/master/",
                "https://git.promedweb.ru/msdbteam/php_log/-/tree/master/",
                "YMLFile_php_log_ms",
                "data",
                "NO",
                "MSSQL",
                "dev_phplog_ms",
                "dev_phplog_ms",
                "https://git.promedweb.ru/msdbteam/dev_phplog_ms/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/msdbteam/dev_phplog_ms/-/tree/%BRANCH%/",
                "YMLFile_dev_php_log_ms",
                "data",
                "YES",
                "YES",
                "NO",
                "10.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0" },
                "NO",
                "php_log",
                "MS SQL",
                "rel_phplog_ms",
                "",
                "rel_phplog_ms_sp1",
                "",
                "rel_phplog_ms_hf",
                "",
                "rel_phplog_ms_ehf_act",
                "",
                "rel_phplog_ms_ehf_unact",
                "",
                "rel_phplog_ms_lts",
                "",
                "",
                "",
                "",
                "",
                "dev_promed_ms",
                "",
                "dev_promed_ms",
                ""
                );

            APPinfo.AddGITProject(
                "userportal_pg",
                "userportaltest",
                "portalpg",
                "rpms",
                "",
                "https://git.promedweb.ru/postgresql_group/userportaltest/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/userportaltest/-/tree/release/",
                "YMLFile_userportal_pg",
                "data",
                "NO",
                "PGSQL",
                "dev_userportal_pg",
                "dev_userportal_pg",
                "https://git.promedweb.ru/postgresql_group/dev_userportal_pg/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/postgresql_group/dev_userportal_pg/-/tree/%BRANCH%/",
                "YMLFile_dev_userportal_pg",
                "data",
                "YES",
                "YES",
                "NO",
                "4.15.0",
                new List<string> { "4.15.0" },
                "NO",
                "userportal",
                "PG SQL",
                "rel_userportal_pg",
                "",
                "rel_userportal_pg_sp1",
                "",
                "rel_userportal_pg_hf",
                "",
                "rel_userportal_pg_ehf_act",
                "",
                "rel_userportal_pg_ehf_unact",
                "",
                "rel_userportal_pg_lts",
                "",
                "rel_qa_userportal",
                "",
                "qa_userportal",
                "",
                "",
                "dev_userportal_pg",
                "",
                "dev_userportal_pg"
                );

            APPinfo.AddGITProject(
                "userportal_ms",
                "userportal",
                "portalms",
                "rpms",
                "",
                "https://git.promedweb.ru/msdbteam/userportal/-/blob/release/",
                "https://git.promedweb.ru/msdbteam/userportal/-/tree/release/",
                "YMLFile_userportal_ms",
                "data",
                "NO",
                "MSSQL",
                "dev_userportal_ms",
                "dev_userportal_ms",
                 "https://git.promedweb.ru/msdbteam/dev_userportal_ms/-/blob/%BRANCH%/",
                 "https://git.promedweb.ru/msdbteam/dev_userportal_ms/-/tree/%BRANCH%/",
                 "YMLFile_dev_userportal_ms",
                "data",
                "YES",
                "YES",
                "NO",
                "4.15.0",
                new List<string> { "4.15.0" },
                "NO",
                "userportal",
                "MS SQL",
                "rel_userportal_ms",
                "",
                "rel_userportal_ms_sp1",
                "",
                "rel_userportal_ms_hf",
                "",
                "rel_userportal_ms_ehf_act",
                "",
                "rel_userportal_ms_ehf_unact",
                "",
                "rel_userportal_ms_lts",
                "",
                "",
                "",
                "",
                "",
                "dev_userportal_ms",
                "",
                "dev_userportal_ms",
                ""
                );

            APPinfo.AddGITProject(
                "fer_log",
                "fer_log",
                "fer",
                "prmd",
                "_FER_LOG",
                "https://git.promedweb.ru/postgresql_group/fer_log/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/fer_log/-/tree/release/",
                "YMLFile_fer_log",
                "data",
                "NO",
                "PGSQL",
                "dev_ferlog_pg",
                "dev_ferlog_pg",
                "https://git.promedweb.ru/postgresql_group/dev_ferlog_pg/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/postgresql_group/dev_ferlog_pg/-/tree/%BRANCH%/",
                "YMLFile_dev_fer_log",
                "data",
                "YES",
                "YES",
                "NO",
                "10.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0" },
                "NO",
                "fer_log",
                "PG SQL",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "dev_promed_pg",
                "",
                "dev_promed_pg"
                );

            APPinfo.AddGITProject(
                "ac_mlo_pg",
                "ac_mlo_pg",
                "acmlopg",
                "prmd",
                "_AC_MLO",
                "https://git.promedweb.ru/postgresql_group/ac_mlo_pg/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/ac_mlo_pg/-/tree/release/",
                "YMLFile_ac_mlo_pg",
                "data",
                "NO",
                "PGSQL",
                "dev_acmlo_pg",
                "dev_acmlo_pg",
                "https://git.promedweb.ru/postgresql_group/dev_acmlo_pg/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/postgresql_group/dev_acmlo_pg/-/tree/%BRANCH%/",
                "YMLFile_dev_ac_mlo_pg",
                "data",
                "YES",
                "YES",
                "NO",
                "10.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0" },
                "NO",
                "ac_mlo",
                "PG SQL",
                "rel_acmlo_pg",
                "",
                "rel_acmlo_pg_sp1",
                "",
                "rel_acmlo_pg_hf",
                "",
                "rel_acmlo_pg_ehf_act",
                "",
                "rel_acmlo_pg_ehf_unact",
                "",
                "rel_acmlo_pg_lts",
                "",
                "",
                "",
                "",
                "",
                "",
                "dev_promed_pg",
                "",
                "dev_promed_pg"
                );

            APPinfo.AddGITProject(
                "smp2_pg",
                "smp2_pg",
                "smp",
                "smp",
                "",
                "https://git.promedweb.ru/postgresql_group/smp2_pg/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/smp2_pg/-/tree/release/",
                "YMLFile_smp2_pg",
                "data",
                "NO",
                "PGSQL",
                "dev_smp2_pg",
                "dev_smp2_pg",
                "https://git.promedweb.ru/postgresql_group/dev_smp2_pg/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/postgresql_group/dev_smp2_pg/-/tree/%BRANCH%/",
                "YMLFile_dev_smp2_pg",
                "data",
                "YES",
                "YES",
                "NO",
                "1.0.0",
                new List<string> { "1.0.0" },
                "NO",
                "smp2",
                "PG SQL",
                "rel_smp2_pg",
                "",
                "rel_smp2_pg_sp1",
                "",
                "rel_smp2_pg_hf",
                "",
                "rel_smp2_pg_ehf_act",
                "",
                "rel_smp2_pg_ehf_unact",
                "",
                "rel_smp2_pg_lts",
                "",
                "",
                "",
                "",
                "",
                "",
                "dev_smp2_pg",
                "",
                "dev_smp2_pg"
                );

            APPinfo.AddGITProject(
                "ac_mlo_ms",
                "ac_mlo_ms",
                "acmloms",
                "prmd",
                "_AC_MLO",
                "https://git.promedweb.ru/msdbteam/ac_mlo_ms/-/blob/release/",
                "https://git.promedweb.ru/msdbteam/ac_mlo_ms/-/tree/release/",
                "YMLFile_ac_mlo_ms",
                "data",
                "NO",
                "MSSQL",
                "dev_acmlo_ms",
                "dev_acmlo_ms",
                "https://git.promedweb.ru/msdbteam/dev_acmlo_ms/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/msdbteam/dev_acmlo_ms/-/tree/%BRANCH%/",
                "YMLFile_dev_ac_mlo_ms",
                "data",
                "YES",
                "YES",
                "NO",
                "10.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0" },
                "NO",
                "ac_mlo",
                "MS SQL",
                "rel_acmlo_ms",
                "",
                "rel_acmlo_ms_sp1",
                "",
                "rel_acmlo_ms_hf",
                "",
                "rel_acmlo_ms_ehf_act",
                "",
                "rel_acmlo_ms_ehf_unact",
                "",
                "rel_acmlo_ms_lts",
                "",
                "",
                "",
                "",
                "",
                "dev_promed_ms",
                "",
                "dev_promed_ms",
                ""
                );

            APPinfo.AddGITProject(
                "gar_pg",
                "gar_pg",
                "gar",
                "prmd",
                "_GAR",
                "https://git.promedweb.ru/postgresql_group/gar_pg/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/gar_pg/-/tree/release/",
                "YMLFile_gar_pg",
                "data",
                "NO",
                "PGSQL",
                "dev_gar_pg",
                "dev_gar_pg",
                "https://git.promedweb.ru/postgresql_group/dev_gar_pg/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/postgresql_group/dev_gar_pg/-/tree/%BRANCH%/",
                "YMLFile_dev_gar_pg",
                "data",
                "YES",
                "YES",
                "NO",
                "10.0.0",
                new List<string> { "9.0.0", "10.0.0", "11.0.0" },
                "NO",
                "gar",
                "ALL",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "dev_promed_pg",
                "",
                "dev_promed_pg"
                );

            APPinfo.AddGITProject(
                "proxy_pg",
                "proxy_pg",
                "proxy",
                "prmd",
                "_PROXY",
                "https://git.promedweb.ru/postgresql_group/proxy_pg/-/blob/release/",
                "https://git.promedweb.ru/postgresql_group/proxy_pg/-/tree/release/",
                "YMLFile_proxy_pg",
                "data",
                "NO",
                "PGSQL",
                "dev_proxy_pg",
                "dev_proxy_pg",
                "https://git.promedweb.ru/postgresql_group/dev_proxy_pg/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/postgresql_group/dev_proxy_pg/-/tree/%BRANCH%/",
                "YMLFile_dev_proxy_pg",
                "data",
                "YES",
                "YES",
                "NO",
                "10.0.0",
                new List<string> { "10.0.0", "11.0.0" },
                "NO",
                "proxy",
                "PG SQL",
                "rel_proxy_pg",
                "",
                "rel_proxy_pg_sp1",
                "",
                "rel_proxy_pg_hf",
                "",
                "rel_proxy_pg_ehf_act",
                "",
                "rel_proxy_pg_ehf_unact",
                "",
                "rel_proxy_pg_lts",
                "",
                "",
                "",
                "",
                "",
                "",
                "dev_promed_pg",
                "",
                "dev_promed_pg"
            );

            APPinfo.AddGITProject(
                "bi",
                "bi",
                "bi",
                "bi",
                "",
                "https://git.promedweb.ru/bi/bi/-/blob/release/",
                "https://git.promedweb.ru/bi/bi/-/tree/release/",
                "YMLFile_bi",
                "data",
                "NO",
                "PGSQL",
                "dev_bi",
                "dev_bi",
                "https://git.promedweb.ru/bi/dev_bi/-/blob/%BRANCH%/",
                "https://git.promedweb.ru/bi/dev_bi/-/tree/%BRANCH%/",
                "YMLFile_dev_bi",
                "data",
                "YES",
                "YES",
                "NO",
                "1.0.0",
                new List<string> { "1.4.0" },
                "NO",
                "bi",
                "PG SQL",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "dev_bi",
                "",
                "dev_bi"
                );

            // принудительно меняем
            /*foreach (var item in APPinfo.GITProjects)
            {
                if (APPinfo.isImproveSQLinVersion)
                {
                    item.DEVisSingleScriptStruct = "NO";
                }
                else
                {
                    item.DEVisSingleScriptStruct = "YES";
                }
            }*/

            Utilities.GITProjects.ListGITProjects = Utilities.Databases.ConvertToDataTable(APPinfo.GITProjects);

            // добавим дефолтные значения
            if (!APPinfo.CheckNoIdTables.Contains("stg.localdblist", StringComparer.OrdinalIgnoreCase)) APPinfo.CheckNoIdTables.Add("stg.localdblist");
            if (!APPinfo.CheckNoIdTables.Contains("UslugaComplexAttribute", StringComparer.OrdinalIgnoreCase)) APPinfo.CheckNoIdTables.Add("UslugaComplexAttribute");
            if (!APPinfo.CheckNoIdTables.Contains("PortalAccessRightsDiag", StringComparer.OrdinalIgnoreCase)) APPinfo.CheckNoIdTables.Add("PortalAccessRightsDiag");
            if (!APPinfo.CheckNoIdTables.Contains("FreeDocMarker", StringComparer.OrdinalIgnoreCase)) APPinfo.CheckNoIdTables.Add("FreeDocMarker");
            if (!APPinfo.CheckNoIdTables.Contains("FreeDocRelationship", StringComparer.OrdinalIgnoreCase)) APPinfo.CheckNoIdTables.Add("FreeDocRelationship");

            // уберем дубли
            List<string> lst = APPinfo.CheckNoIdTables.Distinct().ToList();
            APPinfo.CheckNoIdTables = lst;

            // пусть будут все в нижнем регистре
            for (int i = 0; i < APPinfo.NoUpperBranch.Count; i++)
            {
                APPinfo.NoUpperBranch[i] = APPinfo.NoUpperBranch[i].ToLower();
            }
            // добавим дефолтные значения
            if (!APPinfo.NoUpperBranch.Contains("dev", StringComparer.OrdinalIgnoreCase)) APPinfo.NoUpperBranch.Add("dev");
            if (!APPinfo.NoUpperBranch.Contains("test", StringComparer.OrdinalIgnoreCase)) APPinfo.NoUpperBranch.Add("test");
            if (!APPinfo.NoUpperBranch.Contains("master", StringComparer.OrdinalIgnoreCase)) APPinfo.NoUpperBranch.Add("master");
            if (!APPinfo.NoUpperBranch.Contains("release", StringComparer.OrdinalIgnoreCase)) APPinfo.NoUpperBranch.Add("release");
            if (!APPinfo.NoUpperBranch.Contains("fpumprelease", StringComparer.OrdinalIgnoreCase)) APPinfo.NoUpperBranch.Add("fpumprelease");
            if (!APPinfo.NoUpperBranch.Contains("maintain", StringComparer.OrdinalIgnoreCase)) APPinfo.NoUpperBranch.Add("maintain");
            if (!APPinfo.NoUpperBranch.Contains("utility", StringComparer.OrdinalIgnoreCase)) APPinfo.NoUpperBranch.Add("utility");
            if (!APPinfo.NoUpperBranch.Contains("generator", StringComparer.OrdinalIgnoreCase)) APPinfo.NoUpperBranch.Add("generator");

            // добавим дефолтные значения
            if (!APPinfo.ReleaseBranch.Contains("master", StringComparer.OrdinalIgnoreCase)) APPinfo.ReleaseBranch.Add("master");
            if (!APPinfo.ReleaseBranch.Contains("release", StringComparer.OrdinalIgnoreCase)) APPinfo.ReleaseBranch.Add("release");

            if (APPinfo.ListDatabases.Count() == 0)
            {
                App.AddLog("нет файла SQLGen.json либо в SQLGen.json нет перечня баз данных, используем значения по умолчанию", null, App.ShowMessageMode.NONE, true, null);
            }
            APPinfo.ListDatabases = new BindingList<DBInfo>(APPinfo.ListDatabases.OrderBy(x => x.GITProject + x.DBName + x.ServerName).ToList());

            // проставляем дефолтные значения для пустых полей
            foreach (var item in APPinfo.ListDatabases)
            {
                item.ServerName = item.ServerName;
                item.DBName = item.DBName;
                item.DBType = item.DBType;
                item.GITProject = item.GITProject;
                item.DBRole = item.DBRole;
                item.isMainTest = item.isMainTest;
            }

            // удалим дубликаты
            APPinfo.DelDublicateDatabases();

            // добавим дефолтные значения
            APPinfo.AddDatabase("172.29.3.254,1434", "promeddev", "dev_promed_ms", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254", "ProMedTest", "dev_promed_ms", "TEST", false, false);
            APPinfo.AddDatabase("172.29.3.254", "ProMedUfa", "dev_promed_ms", "TEST", false, false);
            APPinfo.AddDatabase("172.29.3.254", "php_log", "dev_phplog_ms", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254", "log_service", "dev_logservice_ms", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254", "UserPortal", "dev_userportal_ms", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254", "ac_mlo", "dev_acmlo_ms", "TEST", true, false);

            APPinfo.AddDatabase("172.29.3.254", "promedtest", "dev_promed_pg", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254", "promedadygea", "dev_promed_pg", "TEST", false, false);
            APPinfo.AddDatabase("172.29.3.254", "promedlistest2", "dev_lis_pg", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254", "promedlistest_ufa", "dev_lis_pg", "TEST", false, false);
            APPinfo.AddDatabase("172.29.3.254", "fer_log", "dev_ferlog_pg", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254", "smp2dev", "dev_smp2_pg", "TEST", false, false);
            APPinfo.AddDatabase("172.29.3.254:5433", "EMD_dev", "dev_emd_pg", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254:5433", "EMD", "dev_emd_pg", "TEST", false, false);
            APPinfo.AddDatabase("172.29.3.254:5433", "EMD_33", "dev_emd_pg", "TEST", false, false);
            APPinfo.AddDatabase("172.29.3.254:5433", "php_log", "dev_phplog_pg", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254:5433", "log_service", "dev_logservice_pg", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254:5433", "userportaltest", "dev_userportal_pg", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254:5433", "ac_mlo", "dev_acmlo_pg", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254", "smptest3_integrms", "dev_smp2_pg", "TEST", false, false);
            APPinfo.AddDatabase("172.29.3.254", "smptest3", "dev_smp2_pg", "TEST", true, false);
            APPinfo.AddDatabase("172.29.3.254:5433", "gar", "dev_gar_pg", "TEST", true, true);
            APPinfo.AddDatabase("172.29.3.254:5433", "proxy", "dev_proxy_pg", "TEST", true, true);

            // релизные MS "старые"
            APPinfo.AddDatabase("172.29.3.254,1432", "ProMedWebRelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1432", "promedwebufarelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1432", "log_service", "dev_logservice_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1432", "php_log", "dev_phplog_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1432", "userportalrelease", "dev_userportal_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1432", "ac_mlo", "dev_acmlo_ms", "RELEASE", false, true);

            // релизные MS SP
            APPinfo.AddDatabase("172.29.3.254,1461", "ProMedWebRelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1461", "promedwebufarelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1461", "log_service", "dev_logservice_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1461", "php_log", "dev_phplog_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1461", "userportalrelease", "dev_userportal_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1461", "ac_mlo", "dev_acmlo_ms", "RELEASE", false, true);

            // релизные MS HF
            APPinfo.AddDatabase("172.29.3.254,1462", "ProMedWebRelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1462", "promedwebufarelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1462", "log_service", "dev_logservice_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1462", "php_log", "dev_phplog_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1462", "userportalrelease", "dev_userportal_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1462", "ac_mlo", "dev_acmlo_ms", "RELEASE", false, true);

            // релизные MS EHF_ACT
            APPinfo.AddDatabase("172.29.3.254,1463", "ProMedWebRelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1463", "promedwebufarelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1463", "log_service", "dev_logservice_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1463", "php_log", "dev_phplog_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1463", "userportalrelease", "dev_userportal_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1463", "ac_mlo", "dev_acmlo_ms", "RELEASE", false, true);

            // релизные MS EHF_UNACT
            APPinfo.AddDatabase("172.29.3.254,1464", "ProMedWebRelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1464", "promedwebufarelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1464", "log_service", "dev_logservice_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1464", "php_log", "dev_phplog_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1464", "userportalrelease", "dev_userportal_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1464", "ac_mlo", "dev_acmlo_ms", "RELEASE", false, true);

            // релизные MS LTS
            APPinfo.AddDatabase("172.29.3.254,1465", "ProMedWebRelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1465", "promedwebufarelease", "dev_promed_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1465", "log_service", "dev_logservice_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1465", "php_log", "dev_phplog_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1465", "userportalrelease", "dev_userportal_ms", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254,1465", "ac_mlo", "dev_acmlo_ms", "RELEASE", false, true);

            // релизные PG "старые"
            APPinfo.AddDatabase("172.29.3.254:5438", "userportalrelease", "dev_userportal_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5438", "lisrelease", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5438", "lisrelease_ufa", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5438", "php_log", "dev_phplog_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5438", "promedrelease", "dev_promed_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5438", "EMDrelease", "dev_emd_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5438", "log_service", "dev_logservice_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5438", "ac_mlo", "dev_acmlo_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5438", "smp2release", "dev_smp2_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5438", "proxyrelease", "dev_proxy_pg", "RELEASE", false, true);

            // релизные PG SP
            APPinfo.AddDatabase("172.29.3.254:5461", "userportalrelease", "dev_userportal_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5461", "lisrelease", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5461", "lisrelease_ufa", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5461", "php_log", "dev_phplog_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5461", "promedrelease", "dev_promed_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5461", "EMDrelease", "dev_emd_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5461", "log_service", "dev_logservice_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5461", "ac_mlo", "dev_acmlo_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5461", "smp2release", "dev_smp2_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5461", "proxyrelease", "dev_proxy_pg", "RELEASE", false, true);

            // релизные PG HF
            APPinfo.AddDatabase("172.29.3.254:5462", "userportalrelease", "dev_userportal_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5462", "lisrelease", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5462", "lisrelease_ufa", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5462", "php_log", "dev_phplog_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5462", "promedrelease", "dev_promed_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5462", "EMDrelease", "dev_emd_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5462", "log_service", "dev_logservice_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5462", "ac_mlo", "dev_acmlo_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5462", "smp2release", "dev_smp2_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5462", "proxyrelease", "dev_proxy_pg", "RELEASE", false, true);

            // релизные PG EHF_ACT
            APPinfo.AddDatabase("172.29.3.254:5463", "userportalrelease", "dev_userportal_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5463", "lisrelease", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5463", "lisrelease_ufa", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5463", "php_log", "dev_phplog_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5463", "promedrelease", "dev_promed_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5463", "EMDrelease", "dev_emd_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5463", "log_service", "dev_logservice_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5463", "ac_mlo", "dev_acmlo_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5463", "smp2release", "dev_smp2_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5463", "proxyrelease", "dev_proxy_pg", "RELEASE", false, true);

            // релизные PG EHF_UNACT
            APPinfo.AddDatabase("172.29.3.254:5464", "userportalrelease", "dev_userportal_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5464", "lisrelease", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5464", "lisrelease_ufa", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5464", "php_log", "dev_phplog_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5464", "promedrelease", "dev_promed_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5464", "EMDrelease", "dev_emd_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5464", "log_service", "dev_logservice_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5464", "ac_mlo", "dev_acmlo_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5464", "smp2release", "dev_smp2_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5464", "proxyrelease", "dev_proxy_pg", "RELEASE", false, true);

            // релизные PG LTS
            APPinfo.AddDatabase("172.29.3.254:5465", "userportalrelease", "dev_userportal_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5465", "lisrelease", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5465", "lisrelease_ufa", "dev_lis_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5465", "php_log", "dev_phplog_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5465", "promedrelease", "dev_promed_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5465", "EMDrelease", "dev_emd_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5465", "log_service", "dev_logservice_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5465", "ac_mlo", "dev_acmlo_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5465", "smp2release", "dev_smp2_pg", "RELEASE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5465", "proxyrelease", "dev_proxy_pg", "RELEASE", false, true);

            // прод MS Уфы
            APPinfo.AddDatabase("10.62.17.10", "ProMedUfa", "dev_promed_ms", "PROD", false, false);
            APPinfo.AddDatabase("10.62.17.11", "PromedUfaRegistry", "dev_promed_ms", "REESTR", false, false);
            APPinfo.AddDatabase("10.62.17.41", "PromedUfaReport", "dev_promed_ms", "REPORT", false, false);
            APPinfo.AddDatabase("10.62.17.23", "log_service", "dev_logservice_ms", "PROD", false, false);
            APPinfo.AddDatabase("10.62.17.23", "php_log", "dev_phplog_ms", "PROD", false, false);
            APPinfo.AddDatabase("10.62.17.23", "UserPortal", "dev_userportal_ms", "PROD", false, false);

            // QA-Rel
            APPinfo.AddDatabase("172.29.3.254:5532", "userportal", "dev_userportal_pg", "PRODLIKE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5532", "php_log", "dev_phplog_pg", "PRODLIKE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5532", "promed", "dev_promed_pg", "PRODLIKE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5532", "log_service", "dev_logservice_pg", "PRODLIKE", false, true);
            APPinfo.AddDatabase("172.29.3.254:5533", "emd", "dev_emd_pg", "PRODLIKE", false, true);

            // удалим дубликаты
            APPinfo.DelDublicateDatabases();

            // загрузим список подключений
            LoadConnects();

            // обновим список регионов
            APPinfo.Regions.Clear();
            APPinfo.AddRegion("1", "Адыгея");
            APPinfo.AddRegion("2", "Башкирия");
            APPinfo.AddRegion("3", "Бурятия");
            APPinfo.AddRegion("4", "Республика Алтай");
            APPinfo.AddRegion("5", "Дагестан");
            APPinfo.AddRegion("6", "Ингушетия");
            APPinfo.AddRegion("7", "КБР");
            //APPinfo.AddRegion("8", "Калмыкия");
            //APPinfo.AddRegion("9", "Карачаево-Черкесия");
            APPinfo.AddRegion("10", "Карелия");
            APPinfo.AddRegion("11", "Коми");
            APPinfo.AddRegion("12", "Марий Эл");
            APPinfo.AddRegion("13", "Мордовия");
            APPinfo.AddRegion("14", "Якутия");
            //APPinfo.AddRegion("15", "Северная Осетия");
            //APPinfo.AddRegion("16", "Татарстан");
            //APPinfo.AddRegion("17", "Тыва");
            APPinfo.AddRegion("18", "Удмуртия");
            APPinfo.AddRegion("19", "Хакасия");
            //APPinfo.AddRegion("20", "Чечня");
            //APPinfo.AddRegion("21", "Чувашия");
            //APPinfo.AddRegion("22", "Алтайский край");
            //APPinfo.AddRegion("23", "Краснодар");
            APPinfo.AddRegion("24", "Красноярск");
            //APPinfo.AddRegion("25", "Приморский край");
            //APPinfo.AddRegion("26", "Ставрополь");
            //APPinfo.AddRegion("27", "Хабаровск");
            //APPinfo.AddRegion("28", "Амур");
            //APPinfo.AddRegion("29", "Архангельск");
            APPinfo.AddRegion("30", "Астрахань");
            //APPinfo.AddRegion("31", "Белгород");
            //APPinfo.AddRegion("32", "Брянск");
            APPinfo.AddRegion("33", "Владимир");
            //APPinfo.AddRegion("34", "Волгоград");
            APPinfo.AddRegion("35", "Вологда");
            //APPinfo.AddRegion("36", "Воронеж");
            //APPinfo.AddRegion("37", "Иваново");
            APPinfo.AddRegion("38", "Иркутск");
            //APPinfo.AddRegion("39", "Калининград");
            APPinfo.AddRegion("40", "Калуга");
            //APPinfo.AddRegion("41", "Камчатка");
            //APPinfo.AddRegion("42", "Кемерово");
            APPinfo.AddRegion("43", "Киров");
            //APPinfo.AddRegion("44", "Кострома");
            APPinfo.AddRegion("45", "Курган");
            //APPinfo.AddRegion("46", "Курск");
            //APPinfo.AddRegion("47", "Ленинградская область");
            //APPinfo.AddRegion("48", "Липецк");
            //APPinfo.AddRegion("49", "Магадан");
            APPinfo.AddRegion("50", "Московская область");
            //APPinfo.AddRegion("51", "Мурманск");
            APPinfo.AddRegion("52", "Нижний Новгород");
            //APPinfo.AddRegion("53", "Новгород");
            //APPinfo.AddRegion("54", "Новосибирск");
            APPinfo.AddRegion("55", "Омск");
            APPinfo.AddRegion("56", "Оренбург");
            //APPinfo.AddRegion("57", "Орел");
            APPinfo.AddRegion("58", "Пенза");
            APPinfo.AddRegion("59", "Пермь");
            //APPinfo.AddRegion("60", "Псков");
            //APPinfo.AddRegion("61", "Ростов");
            APPinfo.AddRegion("62", "Рязань");
            //APPinfo.AddRegion("63", "Самара");
            //APPinfo.AddRegion("64", "Саратов");
            //APPinfo.AddRegion("65", "Сахалин");
            APPinfo.AddRegion("66", "Свердловская область");
            //APPinfo.AddRegion("67", "Смоленск");
            //APPinfo.AddRegion("68", "Тамбов");
            //APPinfo.AddRegion("69", "Тверь");
            //APPinfo.AddRegion("70", "Томск");
            //APPinfo.AddRegion("71", "Тула");
            //APPinfo.AddRegion("72", "Тюмень");
            //APPinfo.AddRegion("73", "Ульяновск");
            //APPinfo.AddRegion("74", "Челябинск");
            //APPinfo.AddRegion("75", "Забайкальский край");
            APPinfo.AddRegion("76", "Ярославль");
            //APPinfo.AddRegion("77", "Москва");
            //APPinfo.AddRegion("78", "Санкт-Петербург");
            APPinfo.AddRegion("79", "Еврейская АО");
            //APPinfo.AddRegion("81", "Корякский");
            //APPinfo.AddRegion("82", "НАО");
            //APPinfo.AddRegion("83", "Таймырский");
            //APPinfo.AddRegion("85", "ХМАО");
            //APPinfo.AddRegion("86", "Чукотка");
            //APPinfo.AddRegion("87", "Эвенкийский");
            //APPinfo.AddRegion("88", "Байконур");
            APPinfo.AddRegion("89", "ЯНАО");
            APPinfo.AddRegion("91", "Крым");
            //APPinfo.AddRegion("92", "Севастополь");
            //APPinfo.AddRegion("101", "Казахстан");
            //APPinfo.AddRegion("201", "Белоруссия");
            APPinfo.AddRegion("301", "ФМБА");
            APPinfo.AddRegion("401", "МВД");
            APPinfo.AddRegion("477", "Мин. Обороны");

            // Версии, на которых разрывается кумулятивность
            // добавим дефолтные значения
            // if (!APPinfo.CumulativeGap.Contains("9.0.0")) APPinfo.CumulativeGap.Add("9.0.0");
            // if (!APPinfo.CumulativeGap.Contains("10.0.0")) APPinfo.CumulativeGap.Add("10.0.0");

            // загрузим список поисковых запросов
            Search.LoadSearches();

            // принудительно включаем новые генераторы
            APPinfo.IsNewGen = "true";

            // принудительно включаем использование p_FKCreate, p_SetNotNull
            APPinfo.UseNewFunc = "true";

            // принудительно включаем совместимость с Liquibase4
            APPinfo.relativeToChangelogFile = "false";

            // принудительно определяем url в Jira
            APPinfo.TaskUrlDefault = "https://jira.rtmis.ru/browse/";

            // принудительно включаем кооперативный режим сборки версий
            // APPinfo.TaskReleaseCooperative = "true";
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Записать в лог параметры, для диагностики</summary>
        public static void LogAPPinfo()
        {
            App.AddLog("APPinfo.TaskFolder=" + APPinfo.TaskFolder, null, App.ShowMessageMode.NONE, true, null);
            if (!string.IsNullOrWhiteSpace(APPinfo.TaskFolder))
            {
                if (!Directory.Exists(APPinfo.TaskFolder))
                {
                    App.AddLog("Каталог " + APPinfo.TaskFolder + " НЕ существует", null, App.ShowMessageMode.NONE, true, null);
                }
            }

            App.AddLog("APPinfo.GITFolder=" + APPinfo.GITFolder, null, App.ShowMessageMode.NONE, true, null);
            if (!string.IsNullOrWhiteSpace(MainWindow.APPinfo.GITFolder))
            {
                if (!Directory.Exists(MainWindow.APPinfo.GITFolder))
                {
                    App.AddLog("Каталог " + APPinfo.GITFolder + " НЕ существует", null, App.ShowMessageMode.NONE, true, null);
                }
            }

            App.AddLog("APPinfo.FileEditor=" + APPinfo.FileEditor, null, App.ShowMessageMode.NONE, true, null);
            if (!string.IsNullOrWhiteSpace(APPinfo.FileEditor))
            {
                if (!File.Exists(APPinfo.FileEditor))
                {
                    App.AddLog("Приложение " + APPinfo.TaskFolder + " НЕ существует", null, App.ShowMessageMode.NONE, true, null);
                }
            }

            App.AddLog("APPinfo.DirectoryEditor=" + APPinfo.DirectoryEditor, null, App.ShowMessageMode.NONE, true, null);
            if (!string.IsNullOrWhiteSpace(APPinfo.DirectoryEditor))
            {
                if (!File.Exists(APPinfo.DirectoryEditor))
                {
                    App.AddLog("Приложение " + APPinfo.DirectoryEditor + " НЕ существует", null, App.ShowMessageMode.NONE, true, null);
                }
            }

            App.AddLog("APPinfo.SqlGenReleasePath=" + APPinfo.SqlGenReleasePath, null, App.ShowMessageMode.NONE, true, null);
            if (!Directory.Exists(MainWindow.APPinfo.SqlGenReleasePath))
            {
                App.AddLog("Каталог " + APPinfo.SqlGenReleasePath + " НЕ существует", null, App.ShowMessageMode.NONE, true, null);
            }

            foreach (DataRow row in Utilities.GITProjects.ListGITProjects.Rows)
            {
                string project = (string)row["GITProject"];
                string folder = (string)row["GITProjectFolder"];

                if (string.IsNullOrWhiteSpace(folder))
                {
                    App.AddLog("В SQLGen.json нет информации о папке GIT-проекта " + project, null, App.ShowMessageMode.NONE, true, null);
                }
                else if (!string.IsNullOrWhiteSpace(MainWindow.APPinfo.GITFolder))
                {
                    string path = Path.Combine(MainWindow.APPinfo.GITFolder, folder);
                    if (!Directory.Exists(path))
                    {
                        App.AddLog("В каталоге " + APPinfo.GITFolder + " НЕТ папки " + folder + " для GIT-проекта " + project, null, App.ShowMessageMode.NONE, true, null);
                    }
                    else
                    {
                        App.AddLog("В каталоге " + APPinfo.GITFolder + " ЕСТЬ папка " + folder + " для GIT-проекта " + project, null, App.ShowMessageMode.NONE, true, null);
                    }
                }
            }

            foreach (DataRow row in Utilities.GITProjects.ListGITProjects.Rows)
            {
                string project = (string)row["DEVProject"];
                string folder = (string)row["DEVProjectFolder"];

                if (string.IsNullOrWhiteSpace(folder))
                {
                    App.AddLog("В SQLGen.json нет информации о папке GIT-проекта " + project, null, App.ShowMessageMode.NONE, true, null);
                }
                else if (!string.IsNullOrWhiteSpace(MainWindow.APPinfo.GITFolder))
                {
                    string path = Path.Combine(MainWindow.APPinfo.GITFolder, folder);
                    if (!Directory.Exists(path))
                    {
                        App.AddLog("В каталоге " + APPinfo.GITFolder + " НЕТ папки " + folder + " для GIT-проекта " + project, null, App.ShowMessageMode.NONE, true, null);
                    }
                    else
                    {
                        App.AddLog("В каталоге " + APPinfo.GITFolder + " ЕСТЬ папка " + folder + " для GIT-проекта " + project, null, App.ShowMessageMode.NONE, true, null);
                    }
                }
            }


        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Изменился Каталог для задач</summary>
        private bool TaskFolderChanged()
        {
            if (APPinfo.TaskFolder != tbTaskFolder.Text.Trim())
            {
                if (!string.IsNullOrWhiteSpace(Task.TaskNumber))
                {
                    MessageBox.Show("Каталог задач можно изменить только при пустом поле Номер задачи !");
                    tbTaskFolder.Text = APPinfo.TaskFolder;
                    return false;
                }

                string dir = tbTaskFolder.Text.Trim();

                if ((!string.IsNullOrWhiteSpace(dir)) && (!Directory.Exists(dir)))
                {
                    if (System.Windows.Forms.MessageBox.Show("Создать каталог для задач " + dir + " ?", "Создать", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        App.AddLog($"Выбрано создание каталога {dir} для задач", null, App.ShowMessageMode.NONE, true, null);
                        Directory.CreateDirectory(dir);
                    }
                    else
                    {
                        tbTaskFolder.Text = APPinfo.TaskFolder;
                        return false;
                    }
                }

                APPinfo.TaskFolder = dir;
                tbTaskFolder.Text = APPinfo.TaskFolder;

                TaskNumberChanged(true);
            }

            if (IsSettingsOk())
            {
                tabTask.Visibility = Visibility.Visible;
            }
            else
            {
                tabTask.Visibility = Visibility.Collapsed;
                if (!tabSettings.IsSelected) tabSettings.IsSelected = true;
            }
            return true;
        }

        /// <summary>Выход из поля Каталог для задач</summary>
        private void tbTaskFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!TaskFolderChanged())
            {
                Dispatcher.BeginInvoke((ThreadStart)delegate
                {
                    if (!tabSettings.IsSelected) tabSettings.IsSelected = true;
                    if (!tbTaskFolder.IsFocused) tbTaskFolder.Focus();
                });
            }
        }

        /// <summary>Нажата клавиша в поле Каталог для задач</summary>
        private void tbTaskFolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TaskFolderChanged();
                tbGITFolder.Focus();
                e.Handled = true;
            }
        }

        /// <summary>Нажата кнопка Выбрать рядом с полем Каталог для задач</summary>
        private void btTaskFolder_Click(object sender, RoutedEventArgs e)
        {
            string dir = Controls.Dialogs.FolderBrowserDialog(APPinfo.TaskFolder);
            if (dir != "")
            {
                tbTaskFolder.Text = dir;
                TaskFolderChanged();
                dgScriptsInTaskRefresh();
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Изменился каталог GIT</summary>
        private bool GITFolderChanged()
        {
            if (MainWindow.APPinfo.GITFolder != tbGITFolder.Text.Trim())
            {
                if (!string.IsNullOrWhiteSpace(Task.TaskNumber))
                {
                    MessageBox.Show("Каталог GIT можно изменить только при пустом поле Номер задачи !");
                    tbGITFolder.Text = APPinfo.GITFolder;
                    return false;
                }

                string dir = tbGITFolder.Text.Trim();

                if ((!string.IsNullOrWhiteSpace(dir)) && (!Directory.Exists(dir)))
                {
                    if (System.Windows.Forms.MessageBox.Show("Создать каталог для проектов GIT " + dir + " ?", "Создать", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        App.AddLog($"Выбрано создание каталога {dir} для проектов GIT", null, App.ShowMessageMode.NONE, true, null);
                        Directory.CreateDirectory(dir);
                    }
                    else
                    {
                        tbGITFolder.Text = APPinfo.GITFolder;
                        return false;
                    }
                }

                APPinfo.GITFolder = dir;
                tbGITFolder.Text = APPinfo.GITFolder;

                Utilities.Controls.FillComboBoxProjects(cbGITProject, true, true);
            }
            if (IsSettingsOk())
            {
                tabTask.Visibility = Visibility.Visible;
            }
            else
            {
                tabTask.Visibility = Visibility.Collapsed;
            }

            return true;
        }

        /// <summary>Нажата кнопка Выбрать рядом с полем Каталог GIT</summary>
        private void btGITFolder_Click(object sender, RoutedEventArgs e)
        {
            string dir = Controls.Dialogs.FolderBrowserDialog(MainWindow.APPinfo.GITFolder);
            if (dir != "")
            {
                tbGITFolder.Text = dir;
                GITFolderChanged();
            }
        }

        /// <summary>Выход из поля Каталог GIT</summary>
        private void tbGITFolder_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!GITFolderChanged())
            {
                Dispatcher.BeginInvoke((ThreadStart)delegate
                {
                    if (!tabSettings.IsSelected) tabSettings.IsSelected = true;
                    if (!tbGITFolder.IsFocused) tbGITFolder.Focus();
                });
            }
        }

        /// <summary>Нажата клавиша в поле Каталог GIT</summary>
        private void tbGITFolder_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                GITFolderChanged();
                tbFileEditor.Focus();
                e.Handled = true;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Изменился редактор файлов</summary>
        private void FileEditorChanged()
        {
            if (APPinfo.FileEditor != tbFileEditor.Text.Trim())
            {
                APPinfo.FileEditor = tbFileEditor.Text;
                tbFileEditor.Text = APPinfo.FileEditor;
            }
        }

        /// <summary>Нажата клавиша в поле "Редактор файлов"</summary>
        private void tbFileEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                FileEditorChanged();
                tbDirectoryEditor.Focus();
                e.Handled = true;
            }
        }
        /// <summary>
        /// вышли из поля "Редактор файлов"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFileEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            FileEditorChanged();
        }
        /// <summary>
        /// Нажата кнопка выбора редактора файлов
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFileEditor_Click(object sender, RoutedEventArgs e)
        {
            string file = Controls.Dialogs.OpenExeDialog(MainWindow.APPinfo.GITFolder);
            if ((!string.IsNullOrWhiteSpace(file)) && File.Exists(file))
            {
                tbFileEditor.Text = file;
                FileEditorChanged();
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Изменился редактор папок</summary>
        private void DirectoryEditorChanged()
        {
            if (APPinfo.DirectoryEditor != tbDirectoryEditor.Text.Trim())
            {
                APPinfo.DirectoryEditor = tbDirectoryEditor.Text;
                tbDirectoryEditor.Text = APPinfo.DirectoryEditor;
            }
        }

        /// <summary>Нажата клавиша в поле "Редактор папок"</summary>
        private void tbDirectoryEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                DirectoryEditorChanged();
                tbTaskFolder.Focus();
                e.Handled = true;
            }
        }
        /// <summary>
        /// Выход из поля "Редактор папок"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbDirectoryEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            DirectoryEditorChanged();
        }
        /// <summary>
        /// Нажата кнопка выбора редактора папок
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDirectoryEditor_Click(object sender, RoutedEventArgs e)
        {
            string file = Controls.Dialogs.OpenExeDialog(APPinfo.DirectoryEditor);
            if ((!string.IsNullOrWhiteSpace(file)) && File.Exists(file))
            {
                tbDirectoryEditor.Text = file;
                DirectoryEditorChanged();
            }
        }

    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Описание цвета RGB+alpha
    /// </summary>
    public class ColorInfo
    {
        /// <summary>
        /// Красный (R)
        /// </summary>
        public byte redButtonMouseOver { get; set; } = 0;
        /// <summary>
        /// Зеленый (G)
        /// </summary>
        public byte greenButtonMouseOver { get; set; } = 0;
        /// <summary>
        /// Синий (B)
        /// </summary>
        public byte blueButtonMouseOver { get; set; } = 0;
        /// <summary>
        /// Alpha
        /// </summary>
        public byte alphaButtonMouseOver { get; set; } = 0;
        /// <summary>
        /// Соответствующий цвет
        /// </summary>
        public System.Windows.Media.Brush Brush
        {
            get
            {
                if (
                (redButtonMouseOver != 0) ||
                (greenButtonMouseOver != 0) ||
                (blueButtonMouseOver != 0) ||
                (alphaButtonMouseOver != 0)
                )
                {
                    return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(alphaButtonMouseOver, redButtonMouseOver, greenButtonMouseOver, blueButtonMouseOver));
                }
                else
                {
                    return null;
                }
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Описание масштабирования
    /// </summary>
    public class ScaleInfo
    {
        /// <summary>
        /// минимальный масштаб
        /// </summary>
        public const double minScale = 1;

        double _scaleX = minScale;
        /// <summary>
        /// масштаб по оси X
        /// </summary>
        public double ScaleX
        {
            get
            {
                return _scaleX;
            }
            set
            {
                this._scaleX = value;
                if (_scaleX < minScale) _scaleX = minScale;
            }
        }

        double _scaleY = minScale;
        /// <summary>
        /// масштаб по оси Y
        /// </summary>
        public double ScaleY
        {
            get
            {
                return _scaleY;
            }
            set
            {
                this._scaleY = value;
                if (_scaleY < minScale) _scaleY = minScale;
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Размер окна
    /// </summary>
    public class WindowInfo
    {
        /// <summary>
        /// Название окна (формы)
        /// </summary>
        public string Name { get; set; } = "Default Window";

        /// <summary>
        /// Высота окна
        /// </summary>
        public double Height { get; set; } = 0;

        /// <summary>
        /// Ширина окна
        /// </summary>
        public double Width { get; set; } = 0;

        // -------------------------------------------------------------------------------------------------------
        WindowState _State = WindowState.Normal;
        /// <summary>
        /// Состояние окна
        /// </summary>
        public WindowState State
        {
            get
            {
                return _State;
            }
            set
            {
                this._State = value;
                if (_State == WindowState.Minimized) _State = WindowState.Normal;
            }
        }

        /// <summary>
        /// Высота с учетом масштаба
        /// </summary>
        /// <param name="scale">масштаб</param>
        /// <returns></returns>
        public double ScaleHeight(ScaleInfo scale) => Height * scale.ScaleY;

        /// <summary>
        /// Ширина с учетом масштаба
        /// </summary>
        /// <param name="scale">масштаб</param>
        /// <returns></returns>
        public double ScaleWidth(ScaleInfo scale) => Width * scale.ScaleX;

        /// <summary>
        /// Установить новую высоту окна с учетом масштаба
        /// </summary>
        /// <param name="scale">масштаб</param>
        /// <param name="newHeight">высота окна</param>
        public void SetHeight(ScaleInfo scale, double newHeight)
        {
            Height = Math.Round(newHeight / scale.ScaleY, 0);
        }

        /// <summary>
        /// Установить новую ширину окна с учетом масштаба
        /// </summary>
        /// <param name="scale">масштаб</param>
        /// <param name="newWidth">ширина окна</param>
        public void SetWidth(ScaleInfo scale, double newWidth)
        {
            Width = Math.Round(newWidth / scale.ScaleY, 0);
        }

        /// <summary>
        /// шрифты для окон со скриптами
        /// </summary>
        public List<ScriptBoxInfo> ListScriptBox { get; set; } = new List<ScriptBoxInfo>();

    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Пользовательская настройка интерфейса
    /// </summary>
    public class GUI
    {
        /// <summary>
        /// Цвет кнопок при проходе над ними курсора мыши
        /// </summary>
        public ColorInfo colorButtonMouseOver = new ColorInfo();

        /// <summary>
        /// Масштабирование интерфейса
        /// </summary>
        public ScaleInfo scaleWindow { get; set; } = new ScaleInfo();

        /// <summary>
        /// шрифт и размер шрифта по умолчанию для textbox редактирования
        /// </summary>
        public ScriptBoxInfo scriptBoxDefault { get; set; } = new ScriptBoxInfo();

        /// <summary>
        /// Пользовательские настройки окон
        /// </summary>
        public List<WindowInfo> ListWindows { get; set; } = new List<WindowInfo>();
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// класс описания шрифта для textbox редактирования
    /// </summary>
    public class ScriptBoxInfo
    {
        /// <summary>
        /// Наименование элемента со списком шрифтов
        /// </summary>
        public string ScriptBoxName { get; set; } = "default";
        /// <summary>
        /// Шрифт
        /// </summary>
        public string ScriptBoxFont { get; set; } = "";
        /// <summary>
        /// Размер
        /// </summary>
        public string ScriptBoxFontSize { get; set; } = "";
    }
}
