// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Data;
using SQLGen.Utilities;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.IO;
using System.Collections.ObjectModel;

namespace SQLGen
{
    /// <summary>
    /// Класс описания пункта "Действия при обновлении"
    /// </summary>
    public class Deployment : INotifyPropertyChanged
    {
        /// <summary>
        /// Копирование пункта "Действия при обновлении"
        /// </summary>
        /// <returns></returns>
        public Deployment Copy()
        {
            Deployment copy = (Deployment)this.MemberwiseClone();

            if (this.regions != null)
            {
                copy.regions = new List<string>();
                foreach (var item in this.regions)
                {
                    copy.regions.Add(item);
                }
            }

            if (this.exclude_regions != null)
            {
                copy.exclude_regions = new List<string>();
                foreach (var item in this.exclude_regions)
                {
                    copy.exclude_regions.Add(item);
                }
            }

            if (this.when_failed != null)
            {
                copy.when_failed = this.when_failed.Copy();
            }

            if (this.when_timeout != null)
            {
                copy.when_timeout = this.when_timeout.Copy();
            }

            return copy;
        }

        /// <summary>
        /// создать экземпляр DeploymentToJson
        /// </summary>
        /// <returns></returns>
        public DeploymentToJson ToJson()
        {
            var json = new DeploymentToJson();

            json.task = this.task;

            if (this.order == 0)
            {
                json.order = 1;
            }
            else
            {
                json.order = this.order;
            }

            json.position = this.position;
            json.type = this.type;
            json.database = this.database;
            json.dbms = this.DBMS;
            json.stage = this.stage;
            json.script = this.script;
            json.file = this.file;

            json.regions = null;
            if (this.regions != null)
            {
                json.regions = new List<string>();
                json.regions.AddRange(this.regions);
                if (json.regions.Count == 0) json.regions.Add("all");
            }

            json.exclude_regions = null;
            if (this.exclude_regions != null && this.exclude_regions.Count > 0)
            {
                json.exclude_regions = new List<string>();
                json.exclude_regions.AddRange(this.exclude_regions);
            }

            json.timeout = this.timeout;

            json.when_failed = null;
            if (this.when_failed != null)
            {
                json.when_failed = this.when_failed.Copy();
            }

            json.when_timeout = null;
            if (this.when_timeout != null)
            {
                json.when_timeout = this.when_timeout.Copy();
            }

            return json;
        }

        /// <summary>
        /// создать экземпляр DeploymentToJsonVersionSave
        /// </summary>
        /// <returns></returns>
        public DeploymentToJsonVersionSave ToJsonVersionSave()
        {
            var json = new DeploymentToJsonVersionSave();

            json.task = this.task;

            if (this.order == 0)
            {
                json.order = 1;
            }
            else
            {
                json.order = this.order;
            }

            json.position = this.position;
            json.type = this.type;
            json.database = this.database;
            json.dbms = this.DBMS;
            json.stage = this.stage;
            json.script = this.script;
            json.file = this.file;
            json.regions = null;

            json.regions = new List<string>();
            if (this.regions != null)
            {
                json.regions.AddRange(this.regions);
                if (json.regions.Count == 0) json.regions.Add("all");
            }

            json.exclude_regions = new List<string>();
            if (this.exclude_regions != null && this.exclude_regions.Count > 0)
            {
                json.exclude_regions.AddRange(this.exclude_regions);
            }

            json.timeout = this.timeout;

            json.when_failed = new WhenAction();
            if (this.when_failed != null)
            {
                json.when_failed = this.when_failed.Copy();
            }

            json.when_timeout = new WhenAction();
            if (this.when_timeout != null)
            {
                json.when_timeout = this.when_timeout.Copy();
            }

            return json;
        }

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

        /// <summary>
        /// Заполнить объект Deployment
        /// </summary>
        /// <param name="deployment_task">Номер задачи</param>
        /// <param name="deployment_order">N п/п</param>
        /// <param name="deployment_dbregion">тип региона по типу основной БД</param>
        /// <param name="deployment_position">позиция</param>
        /// <param name="deployment_type">тип автоматизации</param>
        /// <param name="deployment_script">Ссылка на скрипт</param>
        /// <param name="deployment_file">Ссылка на файл</param>
        /// <param name="deployment_database">Целевая база данных</param>
        /// <param name="deployment_stage">Тип целевой БД</param>
        /// <param name="deployment_regions">Целевой регион</param>
        /// <param name="deployment_exclude_regions">кроме указанных регионов</param>
        /// <param name="deployment_timeout">ограничение времени выполнения скрипта или команды</param>
        /// <param name="deployment_when_failed_action">действие в случае ошибки</param>
        /// <param name="deployment_when_failed_name">наименование задания</param>
        /// <param name="deployment_when_failed_job">команда задания</param>
        /// <param name="deployment_when_failed_schedule">расписание задания</param>
        /// <param name="deployment_when_failed_script">скрипт</param>
        /// <param name="deployment_when_failed_script_after">скрипт после успешного выполнения _when_failed_script</param>
        /// <param name="deployment_when_timeout_action">действие в случае превышения _timeout</param>
        /// <param name="deployment_when_timeout_name">наименование задания</param>
        /// <param name="deployment_when_timeout_job">команда задания</param>
        /// <param name="deployment_when_timeout_schedule">расписание задания</param>
        /// <param name="deployment_when_timeout_script">скрипт</param>
        /// <param name="deployment_when_timeout_script_after">скрипт после успешного выполнения _when_timeout_script</param>
        public void SetDeployment(
            string deployment_task, 
            int deployment_order, 
            string deployment_dbregion, 
            string deployment_position, 
            string deployment_type, 
            string deployment_script, 
            string deployment_file, 
            string deployment_database, 
            string deployment_stage = null, 
            List<string> deployment_regions = null, 
            List<string> deployment_exclude_regions = null, 
            int? deployment_timeout = null, 
            string deployment_when_failed_action = null, 
            string deployment_when_failed_name = null, 
            string deployment_when_failed_job = null, 
            string deployment_when_failed_schedule = null, 
            string deployment_when_failed_script = null, 
            string deployment_when_failed_script_after = null, 
            string deployment_when_timeout_action = null, 
            string deployment_when_timeout_name = null, 
            string deployment_when_timeout_job = null, 
            string deployment_when_timeout_schedule = null, 
            string deployment_when_timeout_script = null, 
            string deployment_when_timeout_script_after = null
        )
        {
            this.task = (deployment_task ?? "").Trim();
            this.order = deployment_order;
            this.DBRegion = (deployment_dbregion ?? "").Trim();
            this.Position = (deployment_position ?? "").Trim();
            this.Type = (deployment_type ?? "").Trim();
            this.script = (deployment_script ?? "").Trim();
            this.file = (deployment_file ?? "").Trim();
            this.Database = (deployment_database ?? "").Trim();
            this.Stage = (deployment_stage ?? "").Trim();
            this.timeout = deployment_timeout;

            this.regions = new List<string>();
            if (deployment_regions != null)
            {
                foreach (var item in deployment_regions)
                {
                    this.regions.Add(item);
                }
            }
            else
            {
                this.regions.Add("all");
            }

            this.exclude_regions = null;
            if (deployment_exclude_regions != null)
            {
                this.exclude_regions = new List<string>();
                foreach (var item in deployment_exclude_regions)
                {
                    this.exclude_regions.Add(item);
                }
            }

            this.when_failed_action = (deployment_when_failed_action ?? "").Trim();
            this.when_failed_name = (deployment_when_failed_name ?? "").Trim();
            this.when_failed_job = (deployment_when_failed_job ?? "").Trim();
            this.when_failed_schedule = (deployment_when_failed_schedule ?? "").Trim();
            this.when_failed_script = (deployment_when_failed_script ?? "").Trim();
            this.when_failed_script_after = (deployment_when_failed_script_after ?? "").Trim();

            this.when_timeout_action = (deployment_when_timeout_action ?? "").Trim();
            this.when_timeout_name = (deployment_when_timeout_name ?? "").Trim();
            this.when_timeout_job = (deployment_when_timeout_job ?? "").Trim();
            this.when_timeout_schedule = (deployment_when_timeout_schedule ?? "").Trim();
            this.when_timeout_script = (deployment_when_timeout_script ?? "").Trim();
            this.when_timeout_script_after = (deployment_when_timeout_script_after ?? "").Trim();
        }

        //--------------------------------------------------------------------------------------------------

        string _task;
        /// <summary>
        /// Задача из Jira
        /// </summary>
        public string task
        {
            get
            {
                return _task;
            }
            set
            {
                _task = value;

                if (!string.IsNullOrWhiteSpace(_task))
                {
                    _task = _task.Trim();
                }
                else
                {
                    _task = "";
                }

                OnPropertyChanged(nameof(task));
            }
        }

        int _order = 0;
        /// <summary>
        /// Номер по порядку
        /// </summary>
        public int order
        {
            get
            {
                return _order;
            }
            set
            {
                _order = value;

                OnPropertyChanged(nameof(order));
                OnPropertyChanged(nameof(SortBy));
            }
        }

        /// <summary>
        /// Список возможных типов регионов по типу основной БД
        /// </summary>
        [JsonIgnore]
        public static List<string> ListDBRegions
        {
            get
            {
                return new List<string> {
                    "MS SQL (Промед)",
                    "PG SQL (ЕЦП)"
                };
            }
        }

        /// <summary>
        /// Тип региона по типу основной БД - для json
        /// </summary>
        public string dbregion { get; set; }

        /// <summary>
        /// Тип региона по типу основной БД - для формы
        /// </summary>
        [JsonIgnore]
        public string DBRegion
        {
            get
            {
                return ListDBRegions.Where(x =>
                        x.Split(new char[] { '(' })[0].Trim() == dbregion
                    )
                    .FirstOrDefault();
            }
            set
            {
                dbregion = value;

                if (!string.IsNullOrWhiteSpace(dbregion))
                {
                    dbregion = dbregion.Split(new char[] { '(' })[0].Trim();
                }

                var found = ListDBRegions.Where(x =>
                        x.Split(new char[] { '(' })[0].Trim() == dbregion
                    )
                    .FirstOrDefault();

                if(string.IsNullOrWhiteSpace(found))
                {
                    dbregion = null;
                }

                OnPropertyChanged(nameof(DBRegion));
                OnPropertyChanged(nameof(dbregion));
                OnPropertyChanged(nameof(DBMS));
                OnPropertyChanged(nameof(Project));
            }
        }

        /// <summary>
        /// Список возможных позиций
        /// </summary>
        [JsonIgnore]
        public static List<string> ListPositions
        {
            get
            {
                return new List<string> {
                    "before - ДО основных скриптов версии",
                    "primary - основной скрипт версии",
                    "after - ПОСЛЕ основных скриптов версии"
                };
            }
        }

        /// <summary>
        /// Позиция - для json
        /// </summary>
        public string position { get; set; } = "after";

        /// <summary>
        /// Позиция - для формы
        /// </summary>
        [JsonIgnore]
        public string Position
        {
            get
            {
                return ListPositions.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == position
                    )
                    .FirstOrDefault();
            }
            set
            {
                position = value;

                if (string.IsNullOrWhiteSpace(position))
                {
                    position = "after";
                }

                position = position.Split(new char[] { '-' })[0].Trim();

                var found = ListPositions.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == position
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    position = "after";
                }

                OnPropertyChanged(nameof(position));
                OnPropertyChanged(nameof(Position));
                OnPropertyChanged(nameof(SortBy));
            }
        }

        /// <summary>
        /// для сортировки
        /// </summary>
        public int SortBy
        {
            get
            {
                int step = 0;

                if (position == "before") step = 1000000;
                else if (position == "primary") step = 2000000;
                else step = 3000000;

                return step + order;
            }
        }

        /// <summary>
        /// Список возможных типов автоматизции
        /// </summary>
        [JsonIgnore]
        public static List<string> ListTypes
        {
            get
            {
                return new List<string> {
                    "liquibase - выполнить yml или sql через liquibase",
                    "upload_file - распаковать файлы из zip-архива во временную папку",
                    "sql - выполнить SQL-команду",
                    "restart_replica - перезапустить альтернативную репликацию"
                };
            }
        }

        /// <summary>
        /// Тип автоматизации - для json
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Тип автоматизации - для формы
        /// </summary>
        [JsonIgnore]
        public string Type
        {
            get
            {
                return ListTypes.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == type
                    )
                    .FirstOrDefault();
            }
            set
            {
                type = value;

                if (!string.IsNullOrWhiteSpace(type))
                {
                    type = type.Split(new char[] { '-' })[0].Trim();
                }
                else
                {
                    type = null;
                }

                var found = ListTypes.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == type
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    type = null;
                }

                OnPropertyChanged(nameof(type));
                OnPropertyChanged(nameof(Type));
                OnPropertyChanged(nameof(script_file));
                OnPropertyChanged(nameof(isScriptEnabled));
                OnPropertyChanged(nameof(isFileEnabled));
                OnPropertyChanged(nameof(isFindEnabled));
            }
        }

        string _script;
        /// <summary>
        /// Ссылка на скрипт
        /// </summary>
        public string script
        {
            get
            {
                return _script;
            }
            set
            {
                _script = value;

                if (string.IsNullOrWhiteSpace(_script))
                {
                    _script = null;
                }

                OnPropertyChanged(nameof(script));
                OnPropertyChanged(nameof(script_file));
                OnPropertyChanged(nameof(isScriptEnabled));
                OnPropertyChanged(nameof(isFindEnabled));
            }
        }

        bool _isscriptenabled = false;
        /// <summary>
        /// поле Скрипт доступно для редактирования
        /// </summary>
        [JsonIgnore]
        public bool isScriptEnabled
        {
            get
            {
                return _isscriptenabled || type == "liquibase" || type == "sql";
            }
            set
            {
                _isscriptenabled = value;

                OnPropertyChanged(nameof(isScriptEnabled));
                OnPropertyChanged(nameof(isFindEnabled));
            }
        }

        string _file;
        /// <summary>
        /// Ссылка на файл
        /// </summary>
        public string file
        {
            get
            {
                return _file;
            }
            set
            {
                _file = value;

                if (string.IsNullOrWhiteSpace(_file))
                {
                    _file = null;
                }

                OnPropertyChanged(nameof(file));
                OnPropertyChanged(nameof(script_file));
                OnPropertyChanged(nameof(isFileEnabled));
                OnPropertyChanged(nameof(isFindEnabled));
            }
        }

        bool _isfileenabled = false;
        /// <summary>
        /// поле Файл доступно для редактирования
        /// </summary>
        [JsonIgnore]
        public bool isFileEnabled
        {
            get
            {
                return _isfileenabled || type == "upload_file";
            }
            set
            {
                _isfileenabled = value;

                OnPropertyChanged(nameof(isFileEnabled));
                OnPropertyChanged(nameof(isFindEnabled));
            }
        }

        /// <summary>
        /// или скрипт или файл
        /// </summary>
        [JsonIgnore]
        public string script_file
        {
            get
            {
                if (
                    (type == "liquibase") ||
                    (type == "sql") ||
                    (type == "restart_replica")
                )
                {
                    return script;
                }
                else if (type == "upload_file")
                {
                    return file;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// кнопка "Найти файл" доступна
        /// </summary>
        public bool isFindEnabled => isScriptEnabled || isFileEnabled;

        /// <summary>
        /// Список возможных целевых БД
        /// </summary>
        [JsonIgnore]
        public static List<string> ListDatabases
        {
            get
            {
                List<string> db = new List<string>() {
                    "promed - основная + отчетная + реестровая базы promed",
                    "main - основная база promed",
                    "report - отчетная база promed",
                    "reestr - реестровая база promed",
                    "promed_rpt - версия отчетников",
                    "emd - база РЭМД",
                    "lis - база ЛИС"
                };

                foreach (var item in MainWindow.APPinfo.GITProjects
                    .OrderBy(x => x.DBAlias)
                    .Select(x => x.DBAlias)
                    .Distinct()
                )
                {
                    var found = db.Find(x => x.Split(new char[] { '-' })[0].Trim() == item.ToLower());
                    if (found == null)
                    {
                        db.Add(item);
                    }
                }

                return db;
            }
        }

        /// <summary>
        /// Целевая база данных - для json
        /// </summary>
        public string database { get; set; }

        /// <summary>
        /// Целевая база данных - для формы
        /// </summary>
        [JsonIgnore]
        public string Database
        {
            get
            {
                return ListDatabases.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == database
                    )
                    .FirstOrDefault();
            }
            set
            {
                database = value;

                if (!string.IsNullOrWhiteSpace(database))
                {
                    database = database.Split(new char[] { '-' })[0].Trim();
                }
                else
                {
                    database = null;
                }

                var found = ListDatabases.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == database
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    database = null;
                }

                OnPropertyChanged(nameof(database));
                OnPropertyChanged(nameof(Database));
                OnPropertyChanged(nameof(DBMS));
                OnPropertyChanged(nameof(Project));
            }
        }

        /// <summary>
        /// СУБД
        /// </summary>
        public string DBMS
        {
            get
            {
                string result = "";

                if (
                    database == "promed" ||
                    database == "promed_rpt" ||
                    database == "main" ||
                    database == "report" ||
                    database == "reestr" ||
                    database == "log_service" ||
                    database == "php_log" ||
                    database == "userportal" ||
                    database == "ac_mlo"
                 )
                {
                    result = dbregion;
                }
                else
                {
                    result = GITProjects.GITProjectsParam("DBAlias", database, "DBType");
                    if (result == "MSSQL") result = "MS SQL";
                    if (result == "PGSQL") result = "PG SQL";
                }

                return result;
            }
        }

        /// <summary>
        /// Проект для хранения действий при обновлении
        /// </summary>
        string Project
        {
            get
            {
                string result = this.database;

                if (
                    this.database == "promed_rpt" ||
                    this.database == "main" ||
                    this.database == "report" ||
                    this.database == "reestr"
                )
                {
                    result = "promed";
                }

                if (this.dbregion == "MS SQL")
                {
                    return GITProjects.GITProjectsParam("DBAlias", result, "ProjectCronMS");
                }
                else if (this.dbregion == "PG SQL")
                {
                    return GITProjects.GITProjectsParam("DBAlias", result, "ProjectCronPG");
                }

                return "";
            }
        }

        /// <summary>
        /// Список возможных типов целевых БД
        /// </summary>
        [JsonIgnore]
        public static List<string> ListStages
        {
            get
            {
                return new List<string> {
                    "all - продлайк + прод",
                    "prod - только на прод",
                    "prodlike - только на продлайк",
                    "release -  релизные + тестовые",
                    "test -  тестовые"
                };
            }
        }

        /// <summary>
        /// Тип целевой БД - для json
        /// </summary>
        public string stage { get; set; } = "all";

        /// <summary>
        /// Тип целевой БД - для формы
        /// </summary>
        [JsonIgnore]
        public string Stage
        {
            get
            {
                return ListStages.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == stage
                    )
                    .FirstOrDefault();
            }
            set
            {
                stage = value;

                if (string.IsNullOrWhiteSpace(stage))
                {
                    stage = "all";
                }

                stage = stage.Split(new char[] { '-' })[0].Trim();

                var found = ListStages.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == stage
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    stage = "all";
                }

                OnPropertyChanged(nameof(Stage));
            }
        }

        /// <summary>
        /// Список возможных регионов
        /// </summary>
        [JsonIgnore]
        public static List<string> ListRegions
        {
            get
            {
                List<string> regions = new List<string>() { "all" };

                regions.AddRange(MainWindow.APPinfo.Regions
                    .OrderBy(x => x.Value)
                    .Select(x => x.Value + $" ({x.Key})")
                );

                return regions;
            }
        }

        /// <summary>
        /// Разрешенные имена регионов
        /// </summary>
        [JsonIgnore]
        public static List<string> ListRegionsName
        {
            get
            {
                List<string> result = new List<string>();

                result.AddRange(MainWindow.APPinfo.Regions
                    .OrderBy(x => x.Value)
                    .Select(x => x.Value)
                );

                return result;
            }
        }

        /// <summary>
        /// Целевые регионы
        /// </summary>
        public List<string> regions { get; set; } = new List<string>() { "all" };

        /// <summary>
        /// список целевых регионов с выбором
        /// </summary>
        [JsonIgnore]
        public List<BoolStringClass> lcbListRegions { get; set; } = new List<BoolStringClass>();

        /// <summary>
        /// заполнить lcbListRegions
        /// </summary>
        /// <param name="_set_regions">список регионов по умолчанию</param>
        public void Set_lcbListRegions(List<string> _set_regions)
        {
            lcbListRegions.Clear();

            foreach (var item in ListRegions)
            {
                lcbListRegions.Add(new BoolStringClass() { TheText = item, IsSelected = false });
            }

            List<string> _default;

            if (
                _set_regions != null &&
                _set_regions.Count > 0
            )
            {
                _default = _set_regions;
            }
            else
            {
                _default = regions;

                if (_default != null && _default.Count == 1 && _default[0] == "all")
                {
                    _default = null;
                }
            }

            if (_default != null)
            {
                foreach (var item in _default)
                {
                    var _found = lcbListRegions
                        .Where(x =>
                            x.TheText.ToLower().Split(new char[] { '(' })[0].Trim() == item.ToLower().Split(new char[] { '(' })[0].Trim()
                            )
                        .FirstOrDefault();

                    if (_found == null)
                    {
                        lcbListRegions.Add(_found = new BoolStringClass() { TheText = item, IsSelected = true });
                    }
                    else
                    {
                        _found.IsSelected = true;
                    }
                }
            }
        }

        /// <summary>
        /// Заполнить regions
        /// </summary>
        public void Set_regions()
        {
            regions = new List<string>();

            var found = lcbListRegions
                .Where(x => x.IsSelected && x.TheText == "all")
                .FirstOrDefault();

            if (found != null)
            {
                regions.Add("all");
            }
            else
            {
                regions.AddRange(lcbListRegions
                    .Where(x => x.IsSelected)
                    .Select(x => x.TheText.Split(new char[] { '(' })[0].Trim())
                    .ToList()
                );
            }

            if (regions.Count == 0) regions.Add("all");
        }

        /// <summary>
        /// Список целевых регионов одной строкой
        /// </summary>
        [JsonIgnore]
        public string regions_str => string.Join(", ", regions);


        /// <summary>
        /// Кроме указанных регионов
        /// </summary>
        public List<string> exclude_regions { get; set; }

        /// <summary>
        /// список регионов-исключений с выбором
        /// </summary>
        [JsonIgnore]
        public List<BoolStringClass> lcbListExcludeRegions { get; set; } = new List<BoolStringClass>();


        /// <summary>
        /// заполнить lcbListExcludeRegions
        /// </summary>
        public void Set_lcbListExcludeRegions()
        {
            lcbListExcludeRegions.Clear();

            foreach (var item in ListRegions
                .Where(x => x != "all")
            )
            {
                lcbListExcludeRegions.Add(new BoolStringClass() { TheText = item, IsSelected = false });
            }

            if (exclude_regions != null)
            {
                foreach (var item in exclude_regions)
                {
                    var _found = lcbListExcludeRegions
                        .Where(x =>
                            x.TheText.ToLower().Split(new char[] { '(' })[0].Trim() == item.ToLower().Split(new char[] { '(' })[0].Trim()
                            )
                        .FirstOrDefault();

                    if (_found == null)
                    {
                        lcbListExcludeRegions.Add(_found = new BoolStringClass() { TheText = item, IsSelected = true });
                    }
                    else
                    {
                        _found.IsSelected = true;
                    }
                }
            }
        }

        /// <summary>
        /// Заполнить exclude_regions
        /// </summary>
        public void Set_exclude_regions()
        {
            exclude_regions = null;

            if (lcbListExcludeRegions.Where(x => x.IsSelected).Count() > 0)
            {
                exclude_regions = new List<string>();

                exclude_regions.AddRange(lcbListExcludeRegions
                    .Where(x => x.IsSelected)
                    .Select(x => x.TheText.Split(new char[] { '(' })[0].Trim())
                    .ToList()
                );
            }
        }

        /// <summary>
        /// Список целевых регионов-исключений одной строкой
        /// </summary>
        [JsonIgnore]
        public string exclude_regions_str
        {
            get
            {
                if (exclude_regions != null)
                {
                    return string.Join(", ", exclude_regions);
                }
                else
                {
                    return "";
                }
            }
        }

        bool _hasTimeout;
        /// <summary>
        /// Установлен флаг таймаута
        /// </summary>
        [JsonIgnore]
        public bool hasTimeout
        {
            get
            {
                return _hasTimeout || (timeout != null);
            }
            set
            {
                _hasTimeout = value;

                if (!_hasTimeout)
                {
                    timeout = null;
                }

                OnPropertyChanged(nameof(hasTimeout));
                OnPropertyChanged(nameof(timeout_hour));
                OnPropertyChanged(nameof(timeout_minute));
                OnPropertyChanged(nameof(timeout_second));
            }
        }


        int? _timeout;
        /// <summary>
        /// Ограничение времени выполнения скрипта или команды
        /// </summary>
        public int? timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;

                OnPropertyChanged(nameof(hasTimeout));
                OnPropertyChanged(nameof(timeout_hour));
                OnPropertyChanged(nameof(timeout_minute));
                OnPropertyChanged(nameof(timeout_second));
            }
        }

        /// <summary>
        /// Таймаут - часов
        /// </summary>
        [JsonIgnore]
        public string timeout_hour
        {
            get
            {
                if (timeout == null || timeout <= 0)
                {
                    return "0";
                }
                else
                {
                    return (timeout / 3600).ToString();
                }
            }
            set
            {
                int hh = 0;
                if (!int.TryParse(value, out hh))
                {
                    hh = 0;
                }
                int mm = 0;
                if (!int.TryParse(timeout_minute, out mm))
                {
                    mm = 0;
                }
                int ss = 0;
                if (!int.TryParse(timeout_second, out ss))
                {
                    ss = 0;
                }

                timeout = hh * 3600 + mm * 60 + ss;

                OnPropertyChanged(nameof(timeout_hour));
                OnPropertyChanged(nameof(hasTimeout));
            }
        }

        /// <summary>
        /// Таймаут - минут
        /// </summary>
        [JsonIgnore]
        public string timeout_minute
        {
            get
            {
                if (timeout == null || timeout <= 0)
                {
                    return "0";
                }
                else
                {
                    return (timeout % 3600 / 60).ToString();
                }
            }
            set
            {
                int hh = 0;
                if (!int.TryParse(timeout_hour, out hh))
                {
                    hh = 0;
                }
                int mm = 0;
                if (!int.TryParse(value, out mm))
                {
                    mm = 0;
                }
                int ss = 0;
                if (!int.TryParse(timeout_second, out ss))
                {
                    ss = 0;
                }

                timeout = hh * 3600 + mm * 60 + ss;

                OnPropertyChanged(nameof(timeout_minute));
                OnPropertyChanged(nameof(hasTimeout));
            }
        }

        /// <summary>
        /// Таймаут - секунд
        /// </summary>
        [JsonIgnore]
        public string timeout_second
        {
            get
            {
                if (timeout == null || timeout <= 0)
                {
                    return "0";
                }
                else
                {
                    return (timeout % 3600 % 60).ToString();
                }
            }
            set
            {
                int hh = 0;
                if (!int.TryParse(timeout_hour, out hh))
                {
                    hh = 0;
                }
                int mm = 0;
                if (!int.TryParse(timeout_minute, out mm))
                {
                    mm = 0;
                }
                int ss = 0;
                if (!int.TryParse(value, out ss))
                {
                    ss = 0;
                }

                timeout = hh * 3600 + mm * 60 + ss;

                OnPropertyChanged(nameof(timeout_second));
                OnPropertyChanged(nameof(hasTimeout));
            }
        }

        bool _hasWhenFailed;
        /// <summary>
        /// Есть блок WhenFailed
        /// </summary>
        [JsonIgnore]
        public bool hasWhenFailed
        {
            get
            {
                return _hasWhenFailed || (when_failed_action != null);
            }
            set
            {
                _hasWhenFailed = value;

                if (!_hasWhenFailed)
                {
                    when_failed = null;
                }

                OnPropertyChanged(nameof(hasWhenFailed));
                OnPropertyChanged(nameof(when_failed_action));
                OnPropertyChanged(nameof(when_failed_name));
                OnPropertyChanged(nameof(when_failed_job));
                OnPropertyChanged(nameof(when_failed_schedule));
                OnPropertyChanged(nameof(when_failed_script));
                OnPropertyChanged(nameof(when_failed_script_after));
            }
        }

        /// <summary>
        /// блок для описания действий, которые необходимо предпринять
        /// в случае ошибки
        /// </summary>
        public WhenAction when_failed
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.when_failed_action))
                {
                    var w = new WhenAction();
                    w.SetWhenAction(this.when_failed_action, this.when_failed_name, this.when_failed_job, this.when_failed_schedule, this.when_failed_script, this.when_failed_script_after);
                    return w;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    this.when_failed_action = value.action;
                    this.when_failed_name = value.name;
                    this.when_failed_job = value.job;
                    this.when_failed_schedule = value.schedule;
                    this.when_failed_script = value.script;
                    this.when_failed_script_after = value.script_after;
                }
                else
                {
                    this.when_failed_action = null;
                    this.when_failed_name = null;
                    this.when_failed_job = null;
                    this.when_failed_schedule = null;
                    this.when_failed_script = null;
                    this.when_failed_script_after = null;
                }

                OnPropertyChanged(nameof(hasWhenFailed));
                OnPropertyChanged(nameof(when_failed_action));
                OnPropertyChanged(nameof(when_failed_name));
                OnPropertyChanged(nameof(when_failed_job));
                OnPropertyChanged(nameof(when_failed_schedule));
                OnPropertyChanged(nameof(when_failed_script));
                OnPropertyChanged(nameof(when_failed_script_after));
            }
        }


        bool _hasWhenTimeout;
        /// <summary>
        /// Есть блок WhenTimeout
        /// </summary>
        [JsonIgnore]
        public bool hasWhenTimeout
        {
            get
            {
                return _hasWhenTimeout || (when_timeout_action != null);
            }
            set
            {
                _hasWhenTimeout = value;

                if (!_hasWhenTimeout)
                {
                    when_timeout = null;
                }

                OnPropertyChanged(nameof(hasWhenTimeout));
                OnPropertyChanged(nameof(when_timeout_action));
                OnPropertyChanged(nameof(when_timeout_name));
                OnPropertyChanged(nameof(when_timeout_job));
                OnPropertyChanged(nameof(when_timeout_schedule));
                OnPropertyChanged(nameof(when_timeout_script));
                OnPropertyChanged(nameof(when_timeout_script_after));
            }
        }

        /// <summary>
        /// блок для описания действий, которые необходимо предпринять
        /// в случае превышения ограничения времени выполнения скрипта или команды
        /// </summary>
        public WhenAction when_timeout
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.when_timeout_action))
                {
                    var w = new WhenAction();
                    w.SetWhenAction(this.when_timeout_action, this.when_timeout_name, this.when_timeout_job, this.when_timeout_schedule, this.when_timeout_script, this.when_timeout_script_after);
                    return w;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (value != null)
                {
                    this.when_timeout_action = value.action;
                    this.when_timeout_name = value.name;
                    this.when_timeout_job = value.job;
                    this.when_timeout_schedule = value.schedule;
                    this.when_timeout_script = value.script;
                    this.when_timeout_script_after = value.script_after;
                }
                else
                {
                    this.when_timeout_action = null;
                    this.when_timeout_name = null;
                    this.when_timeout_job = null;
                    this.when_timeout_schedule = null;
                    this.when_timeout_script = null;
                    this.when_timeout_script_after = null;
                }

                OnPropertyChanged(nameof(hasWhenTimeout));
                OnPropertyChanged(nameof(when_timeout_action));
                OnPropertyChanged(nameof(when_timeout_name));
                OnPropertyChanged(nameof(when_timeout_job));
                OnPropertyChanged(nameof(when_timeout_schedule));
                OnPropertyChanged(nameof(when_timeout_script));
                OnPropertyChanged(nameof(when_timeout_script_after));
            }
        }


        /// <summary>
        /// Список возможных действий
        /// </summary>
        [JsonIgnore]
        public static List<string> ListActions
        {
            get
            {
                return new List<string>() {
                    "cron - создать задание в cron или MS SQL Agent",
                    "liquibase - выполнить yml или sql через liquibase"
                };
            }
        }

        string _when_failed_action;
        /// <summary>
        /// действие
        /// </summary>
        [JsonIgnore]
        public string when_failed_action
        {
            get
            {
                if (_when_failed_action == null)
                {
                    return null;
                }
                else
                {
                    return ListActions.Where(x =>
                            x.Split(new char[] { '-' })[0].Trim() == _when_failed_action
                        )
                        .FirstOrDefault();
                }
            }
            set
            {
                _when_failed_action = value;

                if (!string.IsNullOrWhiteSpace(_when_failed_action))
                {
                    _when_failed_action = _when_failed_action.Split(new char[] { '-' })[0].Trim();
                }
                else
                {
                    _when_failed_action = null;
                }

                var found = ListActions.Where(x =>
                            x.Split(new char[] { '-' })[0].Trim() == _when_failed_action
                        )
                        .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    _when_failed_action = null;
                }

                OnPropertyChanged(nameof(hasWhenFailed));
                OnPropertyChanged(nameof(when_failed_action));
            }
        }

        string _when_failed_name;
        /// <summary>
        /// название задания
        /// </summary>
        [JsonIgnore]
        public string when_failed_name
        {
            get
            {
                return _when_failed_name;
            }
            set
            {
                _when_failed_name = value;

                if (string.IsNullOrWhiteSpace(_when_failed_name))
                {
                    _when_failed_name = null;
                }

                OnPropertyChanged(nameof(hasWhenFailed));
                OnPropertyChanged(nameof(when_failed_name));
            }
        }

        string _when_failed_job;
        /// <summary>
        /// команда задания
        /// </summary>
        [JsonIgnore]
        public string when_failed_job
        {
            get
            {
                return _when_failed_job;
            }
            set
            {
                _when_failed_job = value;

                if (string.IsNullOrWhiteSpace(_when_failed_job))
                {
                    _when_failed_job = null;
                }

                OnPropertyChanged(nameof(hasWhenFailed));
                OnPropertyChanged(nameof(when_failed_job));
            }
        }

        string _when_failed_schedule;
        /// <summary>
        /// расписание задания
        /// </summary>
        [JsonIgnore]
        public string when_failed_schedule
        {
            get
            {
                return _when_failed_schedule;
            }
            set
            {
                _when_failed_schedule = value;

                if (string.IsNullOrWhiteSpace(_when_failed_schedule))
                {
                    _when_failed_schedule = null;
                }

                OnPropertyChanged(nameof(hasWhenFailed));
                OnPropertyChanged(nameof(when_failed_schedule));
            }
        }

        string _when_failed_script;
        /// <summary>
        /// скрипт
        /// </summary>
        [JsonIgnore]
        public string when_failed_script
        {
            get
            {
                return _when_failed_script;
            }
            set
            {
                _when_failed_script = value;

                if (string.IsNullOrWhiteSpace(_when_failed_script))
                {
                    _when_failed_script = null;
                }

                OnPropertyChanged(nameof(hasWhenFailed));
                OnPropertyChanged(nameof(when_failed_script));
            }
        }

        string _when_failed_script_after;
        /// <summary>
        /// скрипт после успешного выполнения when_failed_script
        /// </summary>
        [JsonIgnore]
        public string when_failed_script_after
        {
            get
            {
                return _when_failed_script_after;
            }
            set
            {
                _when_failed_script_after = value;

                if (string.IsNullOrWhiteSpace(_when_failed_script_after))
                {
                    _when_failed_script_after = null;
                }

                OnPropertyChanged(nameof(hasWhenFailed));
                OnPropertyChanged(nameof(when_failed_script_after));
            }
        }

        string _when_timeout_action;
        /// <summary>
        /// действие
        /// </summary>
        [JsonIgnore]
        public string when_timeout_action
        {
            get
            {
                if (_when_timeout_action == null)
                {
                    return null;
                }
                else
                {
                    return ListActions.Where(x =>
                            x.Split(new char[] { '-' })[0].Trim() == _when_timeout_action
                        )
                        .FirstOrDefault();
                }
            }
            set
            {
                _when_timeout_action = value;

                if (!string.IsNullOrWhiteSpace(_when_timeout_action))
                {
                    _when_timeout_action = _when_timeout_action.Split(new char[] { '-' })[0].Trim();
                }
                else
                {
                    _when_timeout_action = null;
                }

                var found = ListActions.Where(x =>
                            x.Split(new char[] { '-' })[0].Trim() == _when_timeout_action
                        )
                        .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    _when_timeout_action = null;
                }

                OnPropertyChanged(nameof(hasWhenTimeout));
                OnPropertyChanged(nameof(when_timeout_action));
            }
        }

        string _when_timeout_name;
        /// <summary>
        /// название задания
        /// </summary>
        [JsonIgnore]
        public string when_timeout_name
        {
            get
            {
                return _when_timeout_name;
            }
            set
            {
                _when_timeout_name = value;

                if (string.IsNullOrWhiteSpace(_when_timeout_name))
                {
                    _when_timeout_name = null;
                }

                OnPropertyChanged(nameof(hasWhenTimeout));
                OnPropertyChanged(nameof(when_timeout_name));
            }
        }

        string _when_timeout_job;
        /// <summary>
        /// команда задания
        /// </summary>
        [JsonIgnore]
        public string when_timeout_job
        {
            get
            {
                return _when_timeout_job;
            }
            set
            {
                _when_timeout_job = value;

                if (string.IsNullOrWhiteSpace(_when_timeout_job))
                {
                    _when_timeout_job = null;
                }

                OnPropertyChanged(nameof(hasWhenTimeout));
                OnPropertyChanged(nameof(when_timeout_job));
            }
        }

        string _when_timeout_schedule;
        /// <summary>
        /// расписание задания
        /// </summary>
        [JsonIgnore]
        public string when_timeout_schedule
        {
            get
            {
                return _when_timeout_schedule;
            }
            set
            {
                _when_timeout_schedule = value;

                if (string.IsNullOrWhiteSpace(_when_timeout_schedule))
                {
                    _when_timeout_schedule = null;
                }

                OnPropertyChanged(nameof(hasWhenTimeout));
                OnPropertyChanged(nameof(when_timeout_schedule));
            }
        }

        string _when_timeout_script;
        /// <summary>
        /// скрипт
        /// </summary>
        [JsonIgnore]
        public string when_timeout_script
        {
            get
            {
                return _when_timeout_script;
            }
            set
            {
                _when_timeout_script = value;

                if (string.IsNullOrWhiteSpace(_when_timeout_script))
                {
                    _when_timeout_script = null;
                }

                OnPropertyChanged(nameof(hasWhenTimeout));
                OnPropertyChanged(nameof(when_timeout_script));
            }
        }

        string _when_timeout_script_after;
        /// <summary>
        /// скрипт после успешного выполнения when_timeout_script
        /// </summary>
        [JsonIgnore]
        public string when_timeout_script_after
        {
            get
            {
                return _when_timeout_script_after;
            }
            set
            {
                _when_timeout_script_after = value;

                if (string.IsNullOrWhiteSpace(_when_timeout_script_after))
                {
                    _when_timeout_script_after = null;
                }

                OnPropertyChanged(nameof(hasWhenTimeout));
                OnPropertyChanged(nameof(when_timeout_script_after));
            }
        }

        /// <summary>
        /// Определяем проект GIT по тексту скрипта
        /// </summary>
        public static string GetGITProjectByScript(string _script)
        {
            foreach (var item in MainWindow.APPinfo.GITProjects)
            {
                if (_script.Contains("/" + item.DEVProject + "/"))
                {
                    return item.DEVProject;
                }

                if (_script.Contains("/" + item.GITProject + "/"))
                {
                    return item.GITProject;
                }
            }

            return "";
        }

        /// <summary>
        /// Проект GIT по тексту script или file
        /// </summary>
        [JsonIgnore]
        public string GITProjectFromText
        {
            get
            {
                return GetGITProjectByScript(script_file);
            }
        }

        /// <summary>
        /// Алиас БД по тексту script или file
        /// </summary>
        [JsonIgnore]
        public string DBAliasFromText
        {
            get
            {
                return GITProjects.GetDBAliasByProject(GITProjectFromText);
            }
        }

        /// <summary>
        /// Тип БД по тексту script или file
        /// </summary>
        [JsonIgnore]
        public string DBTypeFromText
        {
            get
            {
                return GITProjects.GetDBTypeByProject(GITProjectFromText);
            }
        }

        /// <summary>
        /// проверяем корректность содержимого загруженного json-файла и по возможности исправляем
        /// </summary>
        /// <param name="deployment_list">список действий</param>
        /// <param name="json_filepath">путь к json-файлу</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="err">ошибки</param>
        /// <returns></returns>
        public static bool CheckJSON(List<DeploymentToJson> deployment_list, string json_filepath, string logFile, out string err)
        {
            err = "";
            string info = "";

            // перебираем содержимое загруженного файла
            foreach (DeploymentToJson _json in deployment_list.OrderBy(x => x.SortBy))
            {
                // task
                if (string.IsNullOrWhiteSpace(_json.task))
                {
                    info = $"В файле {json_filepath} строка {_json.order} - пустой task или без task";
                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.task = null;
                }
                else
                {
                    _json.task = _json.task.Trim();
                }

                // order
                if (_json.order == 0)
                {
                    info = $"В файле {json_filepath} задача {_json.task} - order = 0 (или отсутствует), исправлено на минимальное значение 1";
                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);

                    _json.order = 1;
                }

                // position
                if (string.IsNullOrWhiteSpace(_json.position)) _json.position = "after";

                _json.position = _json.position.Trim().ToLower();

                var found = ListPositions.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == _json.position
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - ошибочное значение position = {_json.position}";
                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.position = "after";
                }

                // type
                if (string.IsNullOrWhiteSpace(_json.type))
                {
                    info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - пустой type или без type";
                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.type = null;
                }

                if (_json.type != null)
                {
                    _json.type = _json.type.Trim().ToLower();
                }

                found = ListTypes.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == _json.type
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - ошибочное значение type = {_json.type}";
                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.type = null;
                }

                // script
                if (string.IsNullOrWhiteSpace(_json.script))
                {
                    _json.script = null;
                }
                else
                {
                    _json.script = _json.script.Trim();
                }

                if ((_json.type == "liquibase" || _json.type == "sql") && string.IsNullOrWhiteSpace(_json.script))
                {
                    info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - {_json.type} с пустым script или без script";
                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;
                }

                // file
                if (string.IsNullOrWhiteSpace(_json.file))
                {
                    _json.file = null;
                }
                else
                {
                    _json.file = _json.file.Trim();
                }

                if (_json.type == "upload_file" && string.IsNullOrWhiteSpace(_json.file))
                {
                    info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - {_json.type} с пустым file или без file";
                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;
                }

                // database
                if (string.IsNullOrWhiteSpace(_json.database))
                {
                    info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - пустой database или без database";
                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.database = null;
                }

                if (_json.database != null)
                {
                    _json.database = _json.database.Trim().ToLower();
                }

                found = ListDatabases.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == _json.database
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - ошибочное значение database = {_json.database}";
                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.database = null;
                }

                // stage
                if (string.IsNullOrWhiteSpace(_json.stage)) _json.stage = "all";

                _json.stage = _json.stage.Trim().ToLower();

                found = ListStages.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == _json.stage
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - ошибочное значение stage = {_json.stage}";
                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.stage = "all";
                }

                // regions
                if (_json.regions == null) _json.regions = new List<string>();
                if (_json.regions.Count == 0) _json.regions.Add("all");

                foreach (var _reg in _json.regions)
                {
                    if (!ListRegionsName.Contains(_reg, StringComparer.OrdinalIgnoreCase) && _reg != "all")
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - ошибочное значение в поле regions = {_reg}";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;
                    }
                }

                // exclude_regions
                if (_json.exclude_regions != null)
                {
                    foreach (var _reg in _json.exclude_regions)
                    {
                        if (!ListRegionsName.Contains(_reg, StringComparer.OrdinalIgnoreCase) && _reg != "all")
                        {
                            info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - ошибочное значение в поле exclude_regions = {_reg}";
                            App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                            err += Environment.NewLine + Environment.NewLine + info;
                        }
                    }
                }

                // when_failed
                if (_json.when_failed != null)
                {
                    // action
                    if (string.IsNullOrWhiteSpace(_json.when_failed.action))
                    {
                        _json.when_failed.action = null;
                    }
                    else 
                    {
                        _json.when_failed.action = _json.when_failed.action.ToLower().Trim();
                    }

                    if (
                        !string.IsNullOrWhiteSpace(_json.when_failed.action) &&
                        _json.when_failed.action != "cron" &&
                        _json.when_failed.action != "liquibase"
                     )
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - ошибочное значение в поле when_failed / action = {_json.when_failed.action}";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;
                    }

                    // name
                    if (string.IsNullOrWhiteSpace(_json.when_failed.name))
                    {
                        _json.when_failed.name = null;
                    }
                    else
                    {
                        _json.when_failed.name = _json.when_failed.name.Trim();
                    }

                    if (
                        _json.when_failed.action == "cron" &&
                        string.IsNullOrWhiteSpace(_json.when_failed.name)
                    )
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - не заполнено поле when_failed / name";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;
                    }

                    // job
                    if (string.IsNullOrWhiteSpace(_json.when_failed.job))
                    {
                        _json.when_failed.job = null;
                    }
                    else
                    {
                        _json.when_failed.job = _json.when_failed.job.Trim();
                    }

                    if (
                        _json.when_failed.action == "cron" &&
                        string.IsNullOrWhiteSpace(_json.when_failed.job)
                    )
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - не заполнено поле when_failed / job";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;
                    }

                    // schedule
                    if (string.IsNullOrWhiteSpace(_json.when_failed.schedule))
                    {
                        _json.when_failed.schedule = null;
                    }
                    else
                    {
                        _json.when_failed.schedule = _json.when_failed.schedule.Trim();
                    }

                    if (
                        _json.when_failed.action == "cron" &&
                        string.IsNullOrWhiteSpace(_json.when_failed.schedule)
                    )
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - не заполнено поле when_failed / schedule";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;
                    }

                    // script
                    if (string.IsNullOrWhiteSpace(_json.when_failed.script))
                    {
                        _json.when_failed.script = null;
                    }
                    else
                    {
                        _json.when_failed.script = _json.when_failed.script.Trim();
                    }

                    if (
                        _json.when_failed.action == "liquibase" &&
                        string.IsNullOrWhiteSpace(_json.when_failed.script)
                    )
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - не заполнено поле when_failed / script";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;
                    }

                    // script_after
                    if (string.IsNullOrWhiteSpace(_json.when_failed.script_after))
                    {
                        _json.when_failed.script_after = null;
                    }
                    else
                    {
                        _json.when_failed.script_after = _json.when_failed.script_after.Trim();
                    }
                }

                // when_timeout
                if (_json.when_timeout != null)
                {
                    // action
                    if (string.IsNullOrWhiteSpace(_json.when_timeout.action))
                    {
                        _json.when_timeout.action = null;
                    }
                    else
                    {
                        _json.when_timeout.action = _json.when_timeout.action.ToLower().Trim();
                    }

                    if (
                        !string.IsNullOrWhiteSpace(_json.when_timeout.action) &&
                        _json.when_timeout.action != "cron" &&
                        _json.when_timeout.action != "liquibase"
                     )
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - ошибочное значение в поле when_timeout / action = {_json.when_timeout.action}";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;

                        _json.when_timeout.action = null;
                    }

                    // name
                    if (string.IsNullOrWhiteSpace(_json.when_timeout.name))
                    {
                        _json.when_timeout.name = null;
                    }
                    else
                    {
                        _json.when_timeout.name = _json.when_timeout.name.Trim();
                    }

                    if (
                        _json.when_timeout.action == "cron" &&
                        string.IsNullOrWhiteSpace(_json.when_timeout.name)
                    )
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - не заполнено поле when_timeout / name";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;
                    }

                    // job
                    if (string.IsNullOrWhiteSpace(_json.when_timeout.job))
                    {
                        _json.when_timeout.job = null;
                    }
                    else
                    {
                        _json.when_timeout.job = _json.when_timeout.job.Trim();
                    }

                    if (
                        _json.when_timeout.action == "cron" &&
                        string.IsNullOrWhiteSpace(_json.when_timeout.job)
                    )
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - не заполнено поле when_timeout / job";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;
                    }

                    // schedule
                    if (string.IsNullOrWhiteSpace(_json.when_timeout.schedule))
                    {
                        _json.when_timeout.schedule = null;
                    }
                    else
                    {
                        _json.when_timeout.schedule = _json.when_timeout.schedule.Trim();
                    }

                    if (
                        _json.when_timeout.action == "cron" &&
                        string.IsNullOrWhiteSpace(_json.when_timeout.schedule)
                    )
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - не заполнено поле when_timeout / schedule";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;
                    }

                    // script
                    if (string.IsNullOrWhiteSpace(_json.when_timeout.script))
                    {
                        _json.when_timeout.script = null;
                    }
                    else
                    {
                        _json.when_timeout.script = _json.when_timeout.script.Trim();
                    }

                    if (
                        _json.when_timeout.action == "liquibase" &&
                        string.IsNullOrWhiteSpace(_json.when_timeout.script)
                    )
                    {
                        info = $"В файле {json_filepath} задача {_json.task} строка {_json.order} - не заполнено поле when_timeout / script";
                        App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFile);
                        err += Environment.NewLine + Environment.NewLine + info;
                    }

                    // script_after
                    if (string.IsNullOrWhiteSpace(_json.when_timeout.script_after))
                    {
                        _json.when_timeout.script_after = null;
                    }
                    else
                    {
                        _json.when_timeout.script_after = _json.when_timeout.script_after.Trim();
                    }
                }
            }

            return string.IsNullOrWhiteSpace(err);
        }

        /// <summary>
        /// Загрузить json-файл со списком действий при обновлении
        /// </summary>
        /// <param name="_dbregion">Тип региона по типу основной БД</param>
        /// <param name="destination_list">Итоговый список</param>
        /// <param name="json_filepath">загружаем действия из json-файла</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="isVersion">=true - версия</param>
        /// <param name="isCheck">=true - проверять корректность\исправлять содержимое загружаемого файла</param>
        /// <returns></returns>
        public static bool LoadJSON(string _dbregion, ObservableCollection<Deployment> destination_list, string json_filepath, string logFile, bool isVersion, bool isCheck)
        {
            if (destination_list == null)
            {
                return false;
            }

            // список действий для добавления
            List<DeploymentToJson> jsonlist_task = null;

            // загружаем json-файл
            if (
                !string.IsNullOrWhiteSpace(json_filepath) && 
                File.Exists(json_filepath)
            )
            {
                try
                {
                    string jsonString = File.ReadAllText(json_filepath);

                    if (!string.IsNullOrWhiteSpace(jsonString))
                    {
                        if (isVersion)
                        {
                            // загружаем версию
                            var json_version = JsonSerializer.Deserialize<DeploymentVersionLoad>(jsonString, new JsonSerializerOptions
                                {
                                    IgnoreReadOnlyProperties = true,
                                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                                    WriteIndented = true,
                                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                });

                            if (json_version != null)
                            {
                                jsonlist_task = json_version.listdeployment;
                            }

                        }
                        else
                        {
                            // проверим, есть ли в файле несколько действий
                            bool isMulti = jsonString
                                .TrimStart(new char[] { '\n', '\r', ' ', '\t' })
                                .StartsWith("[");

                            // загружаем задания задачи
                            if (isMulti)
                            {
                                // загружаем задачу
                                jsonlist_task = JsonSerializer.Deserialize<List<DeploymentToJson>>(jsonString, new JsonSerializerOptions
                                {
                                    IgnoreReadOnlyProperties = true,
                                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                                    WriteIndented = true,
                                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                });
                            }
                            else
                            {
                                jsonlist_task = new List<DeploymentToJson>();
                                jsonlist_task.Add(JsonSerializer.Deserialize<DeploymentToJson>(jsonString, new JsonSerializerOptions
                                {
                                    IgnoreReadOnlyProperties = true,
                                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                                    WriteIndented = true,
                                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                }));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog($"Ошибка загрузки файла {json_filepath} :", ex, App.ShowMessageMode.SHOW, true, logFile);

                    return false;
                }

                if (jsonlist_task == null)
                {
                    jsonlist_task = new List<DeploymentToJson>();
                }

                // проверяем корректность\исправляем содержимое загруженного файла
                if (
                    isCheck &&
                    !CheckJSON(jsonlist_task, json_filepath, logFile, out string err) &&
                    (System.Windows.Forms.MessageBox.Show($"Использовать данные из файла {json_filepath} не смотря на ошибки?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                )
                {
                    return false;
                }
            }

            if (jsonlist_task == null)
            {
                jsonlist_task = new List<DeploymentToJson>();
            }

            int _order = 0;

            // новый список
            var new_list = new ObservableCollection<Deployment>();

            // список уже загруженных задач
            var ReLoadedTask = new List<string>();

            // перебираем старый список
            foreach (var old_item in destination_list
                .Where(x => !string.IsNullOrWhiteSpace(x.task))
                .OrderBy(x => x.SortBy)
            )
            {
                // проверяем, уже загружали задачу
                if (!ReLoadedTask.Contains(old_item.task.ToLower()))
                {
                    // проверяем, есть ли "старая" задача в загружаемом файле
                    bool isreload = false;

                    foreach (var item in jsonlist_task
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.task) &&
                            x.task.ToLower() == old_item.task.ToLower()
                        )
                        .OrderBy(x => x.SortBy)
                    )
                    {
                        isreload = true;

                        var new_item = item.ToDeployment(_dbregion);
                        _order++;
                        new_item.order = _order;

                        new_list.Add(new_item);
                    }

                    if (isreload)
                    {
                        // если "старая" задача есть среди загружаемых - берем новое содержимое, старое удаляем
                        ReLoadedTask.Add(old_item.task.ToLower());
                        continue;
                    }
                    else
                    {
                        // если "старой" задачи нет среди загружаемых - оставляем
                        var new_item = old_item.Copy();
                        _order++;
                        new_item.order = _order;
                        new_item.DBRegion = _dbregion;

                        new_list.Add(new_item);
                    }
                }
            }

            // перебираем новый список
            foreach (var item in jsonlist_task
                .OrderBy(x => x.SortBy)
            )
            {
                // проверяем, загружена ли "новая" задача в итоговый список
                bool isreloaded = false;

                foreach (var old_item in destination_list
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(x.task) &&
                        !string.IsNullOrWhiteSpace(item.task) &&
                        x.task.ToLower() == item.task.ToLower()
                    )
                    .OrderBy(x => x.SortBy)
                )
                {
                    isreloaded = true;
                    break;
                }

                if (!isreloaded)
                {
                    // если "новой" задачи еще нет в итоговом списке - добавляем
                    var new_item = item.ToDeployment(_dbregion);
                    _order++;
                    new_item.order = _order;

                    new_list.Add(new_item);
                }
            }

            destination_list.Clear();
            _order = 0;
            foreach (var item in new_list.OrderBy(x => x.SortBy))
            {
                _order++;
                item.order = _order;
                destination_list.Add(item);
            }

            return true;
        }

        /// <summary>
        /// Сгенерировать текст json-файла действий при обновлении
        /// </summary>
        /// <param name="destination_list">Итоговый список</param>
        /// <param name="BranchDefault">Принудительно изменить ветку в скриптах</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="version">номер версии</param>
        /// <returns></returns>
        public static string GenerateJSON(ObservableCollection<Deployment> destination_list, string BranchDefault, string logFile, string version)
        {
            string result = "";

            if (
                !string.IsNullOrWhiteSpace(BranchDefault) &&
                !string.IsNullOrWhiteSpace(version) &&
                destination_list != null
            )
            {
                // исправляем в ссылках на файлы ветку задачи на ветку версии
                foreach (var item in destination_list
                    .Where(x =>
                        x.type == "liquibase" &&
                        x.script.ToLower().StartsWith("https:") 
                    )
                )
                {
                    string project = item.GITProjectFromText;
                    if (!string.IsNullOrWhiteSpace(project))
                    {
                        string _filepath = Utilities.GITProjects.ConvertUrlToFilepath(project, item.script);

                        if (File.Exists(_filepath))
                        {
                            // если это файл и он существует в ветке версии, соберем url обратно, но уже с веткой версии
                            item.script = Utilities.GITProjects.ConvertFilepathToUrl(project, _filepath, BranchDefault, MainWindow.Task.LogFileRelease);
                        }
                    }
                }

                foreach (var item in destination_list
                    .Where(x =>
                        x.when_failed != null &&
                        x.when_failed.action == "liquibase" &&
                        x.when_failed.script != null &&
                        x.when_failed.script.ToLower().StartsWith("https:")
                    )
                )
                {
                    string project = GetGITProjectByScript(item.when_failed_script); 
                    if (!string.IsNullOrWhiteSpace(project))
                    {
                        string _filepath = Utilities.GITProjects.ConvertUrlToFilepath(project, item.when_failed_script);

                        if (File.Exists(_filepath))
                        {
                            // если это файл и он существует в ветке версии, соберем url обратно, но уже с веткой версии
                            item.when_failed_script = Utilities.GITProjects.ConvertFilepathToUrl(project, _filepath, BranchDefault, MainWindow.Task.LogFileRelease);
                        }
                    }
                }

                foreach (var item in destination_list
                    .Where(x =>
                        x.when_failed != null &&
                        x.when_failed.action == "liquibase" &&
                        x.when_failed.script_after != null &&
                        x.when_failed.script_after.ToLower().StartsWith("https:")
                    )
                )
                {
                    string project = GetGITProjectByScript(item.when_failed_script_after);
                    if (!string.IsNullOrWhiteSpace(project))
                    {
                        string _filepath = Utilities.GITProjects.ConvertUrlToFilepath(project, item.when_failed_script_after);

                        if (File.Exists(_filepath))
                        {
                            // если это файл и он существует в ветке версии, соберем url обратно, но уже с веткой версии
                            item.when_failed_script_after = Utilities.GITProjects.ConvertFilepathToUrl(project, _filepath, BranchDefault, MainWindow.Task.LogFileRelease);
                        }
                    }
                }

                foreach (var item in destination_list
                    .Where(x =>
                        x.when_timeout != null &&
                        x.when_timeout.action == "liquibase" &&
                        x.when_timeout.script != null &&
                        x.when_timeout.script.ToLower().StartsWith("https:")
                    )
                )
                {
                    string project = GetGITProjectByScript(item.when_timeout_script);
                    if (!string.IsNullOrWhiteSpace(project))
                    {
                        string _filepath = Utilities.GITProjects.ConvertUrlToFilepath(project, item.when_timeout_script);

                        if (File.Exists(_filepath))
                        {
                            // если это файл и он существует в ветке версии, соберем url обратно, но уже с веткой версии
                            item.when_timeout_script = Utilities.GITProjects.ConvertFilepathToUrl(project, _filepath, BranchDefault, MainWindow.Task.LogFileRelease);
                        }
                    }
                }

                foreach (var item in destination_list
                    .Where(x =>
                        x.when_timeout != null &&
                        x.when_timeout.action == "liquibase" &&
                        x.when_timeout.script_after != null &&
                        x.when_timeout.script_after.ToLower().StartsWith("https:")
                    )
                )
                {
                    string project = GetGITProjectByScript(item.when_timeout_script_after);
                    if (!string.IsNullOrWhiteSpace(project))
                    {
                        string _filepath = Utilities.GITProjects.ConvertUrlToFilepath(project, item.when_timeout_script_after);

                        if (File.Exists(_filepath))
                        {
                            // если это файл и он существует в ветке версии, соберем url обратно, но уже с веткой версии
                            item.when_timeout_script_after = Utilities.GITProjects.ConvertFilepathToUrl(project, _filepath, BranchDefault, MainWindow.Task.LogFileRelease);
                        }
                    }
                }
            }

            if (
                destination_list != null &&
                destination_list.Count > 0
            )
            {
                List<DeploymentToJson> ListToJson = null;
                List<DeploymentToJsonVersionSave> ListToJsonVersionSave = null;

                if (!string.IsNullOrWhiteSpace(version))
                {
                    ListToJsonVersionSave = destination_list
                        .OrderBy(x => x.SortBy)
                        .Select(x => x.ToJsonVersionSave())
                        .ToList();
                }
                else
                {
                    ListToJson = destination_list
                        .OrderBy(x => x.SortBy)
                        .Select(x => x.ToJson())
                        .ToList();
                }

                try
                {
                    // сохрянем в версию
                    if (
                        !string.IsNullOrWhiteSpace(version) &&
                        ListToJsonVersionSave != null &&
                        ListToJsonVersionSave.Count() > 0
                    )
                    {
                        var v = new DeploymentVersionSave();
                        v.version = version;
                        v.listdeployment = ListToJsonVersionSave;

                        result = JsonSerializer.Serialize<DeploymentVersionSave>(v,
                            new JsonSerializerOptions
                            {
                                IgnoreReadOnlyProperties = true,
                                WriteIndented = true,
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            });
                    }

                    // сохраняем в задачу
                    if (
                        string.IsNullOrWhiteSpace(version) &&
                        ListToJson != null &&
                        ListToJson.Count() > 0
                    )
                    {
                        if (ListToJson.Count() == 1)
                        {
                            result = JsonSerializer.Serialize<DeploymentToJson>(ListToJson.First(),
                                new JsonSerializerOptions
                                {
                                    IgnoreReadOnlyProperties = true,
                                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                                    WriteIndented = true,
                                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                });
                        }
                        else
                        {
                            result = JsonSerializer.Serialize<List<DeploymentToJson>>(ListToJson,
                                new JsonSerializerOptions
                                {
                                    IgnoreReadOnlyProperties = true,
                                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                                    WriteIndented = true,
                                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                                });
                        }
                    }

                }
                catch (Exception ex)
                {
                    App.AddLog(null, ex, App.ShowMessageMode.SHOW, true, logFile);

                    return "";
                }
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = "";
            }

            result = result
                .Replace(Environment.NewLine, "\n")
                .TrimEndNewLine("\n");

            return result;
        }

        /// <summary>
        /// Найти по номеру и загрузить json-файл версии
        /// </summary>
        /// <param name="project">проект</param>
        /// <param name="version">номер версии</param>
        /// <param name="destination_list">список действий</param>
        /// <param name="json_filepath">полный путь к найденному файлу</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="isFound">=true - файл найден</param>
        /// <param name="isError">=true - файл не загружен, т.к. при загрузке была ошибка</param>
        /// <returns></returns>
        public static void LoadVersion(string project, string version, ObservableCollection<Deployment> destination_list, out string json_filepath, string logFile, out bool isFound, out bool isError)
        {
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string path_ver = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            double numversion = Release.VerAsNum(version);
            json_filepath = "";
            isFound = false;
            isError = false;

            string dbregion_ver = "";
            if (!string.IsNullOrWhiteSpace(GITProjects.GetProjectDeployment("MS SQL", project)))
            {
                dbregion_ver = "MS SQL";
            }
            else if (!string.IsNullOrWhiteSpace(GITProjects.GetProjectDeployment("PG SQL", project)))
            {
                dbregion_ver = "PG SQL";
            }
            else
            {
                isError = true;
                return;
            }

            // Ищем существующий файл в папке version по номеру версии
            var files = Directory.GetFiles(path_ver, version + "*_deployment.json").ToList();
            if (files == null) files = new List<string>(); //-V3022
            foreach (var filepath in files)
            {
                if (numversion == Release.VerAsNum(Release.GetNumVersion(prefix, Path.GetFileName(filepath))))  //-V3024
                {
                    // нашли
                    json_filepath = filepath;
                    string file = Path.GetFileName(json_filepath);
                    isFound = true;

                    // загружаем json-файл версии
                    isError = !Deployment.LoadJSON(dbregion_ver, destination_list, json_filepath, logFile, true, true);

                    return;
                }
            }

            return;
        }
    }
    
    /// <summary>
    /// действия в случае ошибки или в случае превышения ограничения времени
    /// </summary>
    public class WhenAction
    {
        /// <summary>
        /// Конструктор WhenAction
        /// </summary>
        public WhenAction()
        {
        }

        /// <summary>
        /// Копирование WhenAction
        /// </summary>
        /// <returns></returns>
        public WhenAction Copy()
        {
            WhenAction copy = (WhenAction)this.MemberwiseClone();

            return copy;
        }

        /// <summary>
        /// Заполнить объект WhenAction
        /// </summary>
        /// <param name="_action"></param>
        /// <param name="_name"></param>
        /// <param name="_job"></param>
        /// <param name="_schedule"></param>
        /// <param name="_script"></param>
        /// <param name="_script_after"></param>
        public void SetWhenAction(string _action, string _name, string _job, string _schedule, string _script, string _script_after)
        {
            this.action = _action.Split(new char[] { '-' })[0].Trim();
            this.name = _name;
            this.job = _job;
            this.schedule = _schedule;
            this.script = _script;
            this.script_after = _script_after;
        }

        //--------------------------------------------------------------------------------------------------

        /// <summary>
        /// действие
        /// </summary>
        public string action { get; set; }

        /// <summary>
        /// наименование задания
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// команда задания
        /// </summary>
        public string job {  get; set; }

        /// <summary>
        /// расписание задания
        /// </summary>
        public string schedule {  get; set; }

        /// <summary>
        /// скрипт
        /// </summary>
        public string script { get; set; }

        /// <summary>
        /// скрипт, который выполняется после упешного выполнения script
        /// </summary>
        public string script_after { get; set; }
    }

    /// <summary>
    /// Класс для сохранения Действий при обновлении в json-файл
    /// </summary>
    public class DeploymentToJson
    {
        /// <summary>
        /// Конструктор DeploymentToJson
        /// </summary>
        public DeploymentToJson()
        {
            this.order = 0;
        }

        /// <summary>
        /// Номер по порядку
        /// </summary>
        public int order { get; set; }

        /// <summary>
        /// Задача
        /// </summary>
        public string task { get; set; }

        /// <summary>
        /// Позиция
        /// </summary>
        public string position { get; set; }

        /// <summary>
        /// для сортировки
        /// </summary>
        public int SortBy
        {
            get
            {
                int step = 0;

                if (position == "before") step = 1000000;
                else if (position == "primary") step = 2000000;
                else step = 3000000;

                return step + order;
            }
        }

        /// <summary>
        /// СУБД
        /// </summary>
        public string dbms { get; set; }

        /// <summary>
        /// Тип автоматизации
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Ссылка на скрипт
        /// </summary>
        public string script {  get; set; }

        /// <summary>
        /// Ссылка на файл
        /// </summary>
        public string file {  get; set; }

        /// <summary>
        /// Целевая база данных
        /// </summary>
        public string database { get; set; }

        /// <summary>
        /// Тип целевой БД
        /// </summary>
        public string stage { get; set; }

        /// <summary>
        /// Целевые регионы
        /// </summary>
        public List<string> regions { get; set; }

        /// <summary>
        /// Кроме указанных регионов
        /// </summary>
        public List<string> exclude_regions { get; set; }

        /// <summary>
        /// Ограничение времени выполнения скрипта или команды
        /// </summary>
        public int? timeout {  get; set; }

        /// <summary>
        /// блок для описания действий, которые необходимо предпринять
        /// в случае ошибки
        /// </summary>
        public WhenAction when_failed { get; set; }

        /// <summary>
        /// блок для описания действий, которые необходимо предпринять
        /// в случае превышения ограничения времени выполнения скрипта или команды
        /// </summary>
        public WhenAction when_timeout { get; set; }


        /// <summary>
        /// Копирование
        /// </summary>
        /// <returns></returns>
        public DeploymentToJson Copy()
        {
            DeploymentToJson copy = (DeploymentToJson)this.MemberwiseClone();

            if (this.regions != null)
            {
                copy.regions = new List<string>();
                foreach (var item in this.regions)
                {
                    copy.regions.Add(item);
                }
            }

            if (this.exclude_regions != null)
            {
                copy.exclude_regions = new List<string>();
                foreach (var item in this.exclude_regions)
                {
                    copy.exclude_regions.Add(item);
                }
            }

            if (this.when_failed != null)
            {
                copy.when_failed = this.when_failed.Copy();
            }

            if (this.when_timeout != null)
            {
                copy.when_timeout = this.when_timeout.Copy();
            }

            return copy;
        }

        /// <summary>
        /// создать экземпляр Deployment
        /// </summary>
        /// <param name="_dbregion">тип региона по типу основной БД</param>
        /// <returns></returns>
        public Deployment ToDeployment(string _dbregion)
        {
            var deployment = new Deployment();

            deployment.task = this.task;
            deployment.order = this.order;
            deployment.DBRegion = _dbregion;
            deployment.Position = this.position;
            deployment.Type = this.type;
            deployment.script = this.script;
            deployment.file = this.file;
            deployment.Database = this.database;
            deployment.Stage = this.stage;
            deployment.regions = null;
            if (this.regions != null)
            {
                deployment.regions = new List<string>();
                deployment.regions.AddRange(this.regions);
                if (deployment.regions.Count == 0) deployment.regions.Add("all");
            }
            deployment.exclude_regions = null;
            if (this.exclude_regions != null && this.exclude_regions.Count > 0)
            {
                deployment.exclude_regions = new List<string>();
                deployment.exclude_regions.AddRange(this.exclude_regions);
            }
            deployment.timeout = this.timeout;

            deployment.when_failed = null;
            if (this.when_failed != null)
            {
                deployment.when_failed = this.when_failed.Copy();
            }

            deployment.when_timeout = null;
            if (this.when_timeout != null)
            {
                deployment.when_timeout = this.when_timeout.Copy();
            }

            return deployment;
        }
    }


    /// <summary>
    /// Класс для сохранения Действий при обновлении в json-файл со списком всех действий (в ветке dev или в ветке версии) и для отображения в Confluence
    /// </summary>
    public class DeploymentToJsonVersionSave
    {
        /// <summary>
        /// конструктор DeploymentToJsonVersionSave
        /// </summary>
        public DeploymentToJsonVersionSave()
        {
            this.order = 0;
            this.regions = new List<string>();
            this.exclude_regions= new List<string>();
            this.when_failed = new WhenAction();
            this.when_timeout = new WhenAction();
        }

        /// <summary>
        /// Номер по порядку
        /// </summary>
        public int order { get; set; }

        /// <summary>
        /// Задача
        /// </summary>
        public string task { get; set; }

        /// <summary>
        /// Позиция
        /// </summary>
        public string position { get; set; }

        /// <summary>
        /// СУБД
        /// </summary>
        public string dbms { get; set; }

        /// <summary>
        /// Тип автоматизации
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Ссылка на скрипт
        /// </summary>
        public string script { get; set; }

        /// <summary>
        /// Ссылка на файл
        /// </summary>
        public string file { get; set; }

        /// <summary>
        /// Целевая база данных
        /// </summary>
        public string database { get; set; }

        /// <summary>
        /// Тип целевой БД
        /// </summary>
        public string stage { get; set; }

        /// <summary>
        /// Целевые регионы
        /// </summary>
        public List<string> regions { get; set; }

        /// <summary>
        /// Кроме указанных регионов
        /// </summary>
        public List<string> exclude_regions { get; set; }

        /// <summary>
        /// Ограничение времени выполнения скрипта или команды
        /// </summary>
        public int? timeout { get; set; }

        /// <summary>
        /// блок для описания действий, которые необходимо предпринять
        /// в случае ошибки
        /// </summary>
        public WhenAction when_failed { get; set; }

        /// <summary>
        /// блок для описания действий, которые необходимо предпринять
        /// в случае превышения ограничения времени выполнения скрипта или команды
        /// </summary>
        public WhenAction when_timeout { get; set; }
    }

    /// <summary>
    /// Файл версии Deployment Plan для загрузки
    /// </summary>
    public class DeploymentVersionLoad
    {
        /// <summary>
        /// Конструктор DeploymentVersionLoad
        /// </summary>
        public DeploymentVersionLoad()
        {
        }

        /// <summary>
        /// номер версии
        /// </summary>
        public string version { get; set; } 

        /// <summary>
        /// список действий при обновлении версии
        /// </summary>
        public List<DeploymentToJson> listdeployment { get; set; }
    }

    /// <summary>
    /// Файл версии Deployment Plan для сохранения и отображения в Confluence
    /// </summary>
    public class DeploymentVersionSave
    {
        /// <summary>
        /// конструктор DeploymentVersionSave
        /// </summary>
        public DeploymentVersionSave()
        {
            this.listdeployment = new List<DeploymentToJsonVersionSave>();
        }

        /// <summary>
        /// номер версии
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// список действий при обновлении версии
        /// </summary>
        public List<DeploymentToJsonVersionSave> listdeployment { get; set; }
    }
}
