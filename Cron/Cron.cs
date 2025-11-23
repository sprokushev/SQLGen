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
using System.Text.RegularExpressions;

namespace SQLGen
{
    /// <summary>
    /// Класс описания пункта "Задание"
    /// </summary>
    public class Cron : INotifyPropertyChanged
    {
        /// <summary>
        /// Копирование пункта "Задание"
        /// </summary>
        /// <returns></returns>
        public Cron Copy()
        {
            Cron copy = (Cron)this.MemberwiseClone();

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

            return copy;
        }

        /// <summary>
        /// создать экземпляр CronToJson
        /// </summary>
        /// <returns></returns>
        public CronToJson ToJson()
        {
            var json = new CronToJson();

            if (this.order == 0)
            {
                json.order = 1;
            }
            else
            {
                json.order = this.order;
            }

            json.task = this.task;
            json.dbms = this.DBMS;
            json.application_name = this.application_name;
            json.comment = this.comment;
            json.command = this.command;
            json.schedule = this.schedule;
            json.timeout = this.timeout;
            json.state = this.state;
            json.database = this.database;
            json.stage = this.stage;

            json.regions = new List<string>(); 
            if (this.regions != null)
            {
                json.regions.AddRange(this.regions);
            }
            if (json.regions.Count == 0) json.regions.Add("all");

            json.exclude_regions = null;
            if (this.exclude_regions != null && this.exclude_regions.Count > 0)
            {
                json.exclude_regions = new List<string>();
                json.exclude_regions.AddRange(this.exclude_regions);
            }

            json.hosts = this.hosts;
            json.check = this.check;
            json.team = this.team;

            if (this.isTemp)
            {
                json.istemp = 2;
            }
            else
            {
                json.istemp = 1;
            }

            return json;
        }

        /// <summary>
        /// создать экземпляр CronToJsonVersionSave
        /// </summary>
        /// <returns></returns>
        public CronToJsonVersionSave ToJsonVersionSave()
        {
            var json = new CronToJsonVersionSave();

            if (this.order == 0)
            {
                json.order = 1;
            }
            else
            {
                json.order = this.order;
            }

            json.task = this.task;
            json.dbms = this.DBMS;
            json.application_name = this.application_name;
            json.comment = this.comment;
            json.command = this.command;
            json.schedule = this.schedule;
            json.timeout = this.timeout;
            json.state = this.state;
            json.database = this.database;
            json.stage = this.stage;

            json.regions = new List<string>();
            if (this.regions != null)
            {
                json.regions.AddRange(this.regions);
            }
            if (json.regions.Count == 0) json.regions.Add("all");

            json.exclude_regions = new List<string>();
            if (this.exclude_regions != null && this.exclude_regions.Count > 0)
            {
                json.exclude_regions.AddRange(this.exclude_regions);
            }

            json.hosts = this.hosts;
            json.check = this.check;
            json.team = this.team;

            if (this.isTemp)
            {
                json.istemp = 2;
            }
            else
            {
                json.istemp = 1;
            }

            return json;
        }

        /// <summary>
        /// создать экземпляр CronToJsonBox
        /// </summary>
        /// <returns></returns>
        public CronToJsonBox ToJsonBox()
        {
            var json = new CronToJsonBox();

            if (this.order == 0)
            {
                json.order = 1;
            }
            else
            {
                json.order = this.order;
            }

            json.task = this.task;
            json.dbms = this.DBMS;
            json.application_name = this.application_name;
            json.comment = this.comment;
            json.command = this.command;
            json.schedule = this.schedule;
            json.timeout = this.timeout;
            json.state = this.state;
            json.database = this.database;
            json.stage = this.stage;

            json.regions = new List<string>();
            if (this.regions != null)
            {
                json.regions.AddRange(this.regions);
            }
            if (json.regions.Count == 0) json.regions.Add("all");

            json.exclude_regions = new List<string>();
            if (this.exclude_regions != null && this.exclude_regions.Count > 0)
            {
                json.exclude_regions.AddRange(this.exclude_regions);
            }

            json.hosts = this.hosts;
            json.check = this.check;
            json.team = this.team;

            if (this.isTemp)
            {
                json.istemp = 2;
            }
            else
            {
                json.istemp = 1;
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

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// =true - наименование задания корректное
        /// </summary>
        /// <param name="name">наименование задания</param>
        /// <returns></returns>
        public static bool IsApplicationNameCorrect(string name) => 
            string.IsNullOrWhiteSpace(name) ||
            name.IsMatch(@"^[a-zA-Z0-9_-]+$");

        /// <summary>
        /// Заполнить объект Cron
        /// </summary>
        /// <param name="cron_order">N п/п</param>
        /// <param name="cron_task">Номер задачи</param>
        /// <param name="cron_dbregion">тип региона по типу основной БД</param>
        /// <param name="cron_application_name">наименование задания</param>
        /// <param name="cron_comment">описание задания</param>
        /// <param name="cron_command">команда задания</param>
        /// <param name="cron_schedule">расписание задания</param>
        /// <param name="cron_state">признак актуальности задания</param>
        /// <param name="cron_database">Целевая база данных</param>
        /// <param name="cron_stage">Тип целевой БД</param>
        /// <param name="cron_regions">Целевой регион</param>
        /// <param name="cron_exclude_regions">кроме указанных регионов</param>
        /// <param name="cron_timeout">ограничение времени выполнения задания</param>
        /// <param name="cron_hosts">возможность параллельного запуска задачи</param>
        /// <param name="cron_check">проверочный запрос</param>
        /// <param name="cron_team">команда РТМИС, ответственная за задание</param>
        /// <param name="cron_istemp">флаг временного задания</param>
        public void SetCron(
            int cron_order, 
            string cron_task, 
            string cron_dbregion, 
            string cron_state, 
            string cron_database, 
            string cron_stage = null, 
            string cron_application_name = null, 
            string cron_comment = null, 
            string cron_command = null, 
            string cron_schedule = null, 
            List<string> cron_regions = null, 
            List<string> cron_exclude_regions = null, 
            int? cron_timeout = null, 
            string cron_hosts = null, 
            string cron_check = null, 
            string cron_team = null, 
            bool cron_istemp = false
        )
        {
            this.order = cron_order;
            this.task = (cron_task ?? "").Trim();
            this.DBRegion = (cron_dbregion ?? "").Trim();
            this.application_name = (cron_application_name ?? "").Trim();
            this.comment = (cron_comment ?? "").Trim();
            this.command = (cron_command ?? "").Trim();
            this.schedule = (cron_schedule ?? "").Trim();
            this.State = (cron_state ?? "").Trim();
            this.Database = (cron_database ?? "").Trim();
            this.Stage = (cron_stage ?? "").Trim();
            this.timeout = cron_timeout;
            this.Hosts = (cron_hosts ?? "").Trim();
            this.check = (cron_check ?? "").Trim();
            this.team = (cron_team ?? "").Trim();
            this.isTemp = cron_istemp;
            this.isFiltered = true;

            string _db = this.database.Trim();

                if (
                    this.database == "main" ||
                    this.database == "report" ||
                    this.database == "reestr"
                )
                {
                    _db = "promed";
                }

            this.folder = Path.Combine("cron", _db)
                .Replace('/', Path.DirectorySeparatorChar)
                .ToLower();

            this.filename = "";

            if (!string.IsNullOrWhiteSpace(this.application_name))
            {
                this.filename = this.application_name.ToLower() + ".json";
            }

            this.regions = new List<string>();
            if (cron_regions != null)
            {
                foreach (var item in cron_regions)
                {
                    this.regions.Add(item);
                }
            }
            else
            {
                this.regions.Add("all");
            }

            this.exclude_regions = null;
            if (cron_exclude_regions != null)
            {
                this.exclude_regions = new List<string>();
                foreach (var item in cron_exclude_regions)
                {
                    this.exclude_regions.Add(item);
                }
            }
        }

        //--------------------------------------------------------------------------------------------------
        /// <summary>Флаг - включено в фильтр</summary>
        public bool isFiltered { get; set; } = true;

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
        /// для сортировки
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

        string _application_name;
        /// <summary>
        /// Наименование задания
        /// </summary>
        public string application_name
        {
            get
            {
                return _application_name;
            }
            set
            {
                _application_name = value;

                if (!string.IsNullOrWhiteSpace(_application_name))
                {
                    _application_name = _application_name.Trim();
                }
                else
                {
                    _application_name = null;
                }

                OnPropertyChanged(nameof(application_name));
                OnPropertyChanged(nameof(filename));
                OnPropertyChanged(nameof(Filepath));
            }
        }

        string _comment;
        /// <summary>
        /// Описание задания
        /// </summary>
        public string comment
        {
            get
            {
                return _comment;
            }
            set
            {
                _comment = value;

                if (string.IsNullOrWhiteSpace(_comment))
                {
                    _comment = null;
                }

                OnPropertyChanged(nameof(comment));
            }
        }

        string _command;
        /// <summary>
        /// команда задания
        /// </summary>
        public string command
        {
            get
            {
                return _command;
            }
            set
            {
                _command = value;

                if (string.IsNullOrWhiteSpace(_command))
                {
                    _command = null;
                }

                OnPropertyChanged(nameof(command));
            }
        }

        string _schedule;
        /// <summary>
        /// расписание задания
        /// </summary>
        public string schedule
        {
            get
            {
                return _schedule;
            }
            set
            {
                _schedule = value;

                if (string.IsNullOrWhiteSpace(_schedule))
                {
                    _schedule = null;
                }

                OnPropertyChanged(nameof(schedule));
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

        /// <summary>
        /// Список возможных статусов задания
        /// </summary>
        [JsonIgnore]
        public static List<string> ListStates
        {
            get
            {
                return new List<string> {
                    "present - задание актуально и должно быть включено",
                    "absent - задание должно быть выключено"
                };
            }
        }

        /// <summary>
        /// Статус задания - для json
        /// </summary>
        public string state { get; set; } = "present";

        /// <summary>
        /// Статус задания - для формы
        /// </summary>
        [JsonIgnore]
        public string State
        {
            get
            {
                return ListStates.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == state
                    )
                    .FirstOrDefault();
            }
            set
            {
                state = value;

                if (string.IsNullOrWhiteSpace(state))
                {
                    state = "present";
                }

                state = state.Split(new char[] { '-' })[0].Trim();

                var found = ListStates.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == state
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    state = "present";
                }

                OnPropertyChanged(nameof(state));
                OnPropertyChanged(nameof(State));
            }
        }

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
                OnPropertyChanged(nameof(folder));
                OnPropertyChanged(nameof(Filepath));
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
        /// Проект для хранения заданий
        /// </summary>
        string Project
        {
            get
            {
                string result = this.database;

                if (
                    this.database == "main" ||
                    this.database == "report" ||
                    this.database == "reestr"
                )
                {
                    result = "promed";
                }

                return GITProjects.GetProjectCron(this.dbregion, result);
            }
        }

        /// <summary>
        /// Путь к папке задания внутри проекта на диске
        /// </summary>
        public string folder { get; set; }

        /// <summary>
        /// Имя файла задания
        /// </summary>
        public string filename { get; set; }

        /// <summary>
        /// Путь к файлу задания на диске
        /// </summary>
        public string Filepath
        {
            get
            {
                if (
                    string.IsNullOrWhiteSpace(this.Project) ||
                    string.IsNullOrWhiteSpace(this.folder) ||
                    string.IsNullOrWhiteSpace(this.filename)
                )
                {
                    return "";
                }

                return Path.Combine(MainWindow.APPinfo.GITFolder, GITProjects.GetFolderByProject(this.Project), this.folder, this.filename);
            }
        }

        /// <summary>
        /// Путь к файлу задания в web
        /// </summary>
        /// <param name="logFile"></param>
        /// <returns></returns>
        public string Fileurl(string logFile)
        {
            if (
                string.IsNullOrWhiteSpace(this.Project) ||
                string.IsNullOrWhiteSpace(this.Filepath)
            )
            {
                return "";
            }

            // определяем текущую ветку в проекте
            string err = "";
            string branch = GIT.GitCurrentBranch(this.Project, out err, logFile);
            if (
                !string.IsNullOrWhiteSpace(err) ||
                string.IsNullOrWhiteSpace(branch)
            )
            {
                App.AddLog($"Ошибка определения текущей ветки в проекте {this.Project}:\n{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                return "";
            }

            // собираем url
            return GITProjects.ConvertFilepathToUrl(this.Project, this.Filepath, branch, logFile);
        }

        /// <summary>
        /// Название задания для выбора
        /// </summary>
        public string ChooseName => $"{this.folder} - {this.application_name}";

        /// <summary>
        /// Список возможных типов целевых БД
        /// </summary>
        [JsonIgnore]
        public static List<string> ListStages
        {
            get
            {
                return new List<string> {
                    "all - все типы стендов (прод + продлайк + релизные + тестовые)",
                    "prod - только на прод + релизные + тестовые",
                    "prodlike - только на продлайк + релизные + тестовые",
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

        /// <summary>
        /// Список возможных параллельного запуска задачи
        /// </summary>
        [JsonIgnore]
        public static List<string> ListHosts
        {
            get
            {
                return new List<string> {
                    "single - запуск в единственном экземпляре",
                    "multi - параллельный запуск"
                };
            }
        }

        /// <summary>
        /// Возможность параллельного запуска задачи - для json
        /// </summary>
        public string hosts { get; set; } = "single";

        /// <summary>
        /// Возможность параллельного запуска задачи - для формы
        /// </summary>
        [JsonIgnore]
        public string Hosts
        {
            get
            {
                return ListHosts.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == hosts
                    )
                    .FirstOrDefault();
            }
            set
            {
                hosts = value;

                if (!string.IsNullOrWhiteSpace(hosts))
                {
                    hosts = hosts.Split(new char[] { '-' })[0].Trim();
                }
                else
                {
                    hosts = "single";
                }

                var found = ListHosts.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == hosts
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    hosts = "single";
                }

                OnPropertyChanged(nameof(hosts));
                OnPropertyChanged(nameof(Hosts));
            }
        }

        string _check;
        /// <summary>
        /// проверочный запрос
        /// </summary>
        public string check
        {
            get
            {
                return _check;
            }
            set
            {
                _check = value;

                if (string.IsNullOrWhiteSpace(_check))
                {
                    _check = null;
                }

                OnPropertyChanged(nameof(check));
            }
        }

        string _team;
        /// <summary>
        /// команда РТМИС, ответственная за задание
        /// </summary>
        public string team
        {
            get
            {
                return _team;
            }
            set
            {
                _team = value;

                if (string.IsNullOrWhiteSpace(_team))
                {
                    _team = null;
                }

                OnPropertyChanged(nameof(team));
            }
        }

        /// <summary>
        /// Флаг временного задания - для json
        /// </summary>
        public int? istemp { get; set; }

        /// <summary>
        /// Флаг временного задания - для формы
        /// </summary>
        [JsonIgnore]
        public bool isTemp
        {
            get
            {
                return istemp == 2;
            }
            set
            {
                istemp = value ? 2 : 1;

                OnPropertyChanged(nameof(istemp));
                OnPropertyChanged(nameof(isTemp));
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
        /// проверяем корректность содержимого загруженного json-файла и по возможности исправляем
        /// </summary>
        /// <param name="cron_list">список заданий</param>
        /// <param name="json_filepath">путь к json-файлу</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="err">ошибки</param>
        /// <param name="showMessageMode">режим отображения сообщений</param>
        /// <returns></returns>
        public static bool CheckJSON(List<CronToJson> cron_list, string json_filepath, string logFile, out string err, App.ShowMessageMode showMessageMode)
        {
            err = "";
            string info = "";
            string info_file = "";

            if (cron_list == null)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(json_filepath))
            {
                info_file = $"В файле {json_filepath} ";
            }

            // перебираем содержимое загруженного файла
            foreach (CronToJson _json in cron_list.OrderBy(x => x.order))
            {
                // application_name
                if (string.IsNullOrWhiteSpace(_json.application_name))
                {
                    info = $"{info_file}задача {_json.task} номер {_json.order} - пустой application_name или без application_name";
                    App.AddLog(info, null, showMessageMode, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.application_name = null;
                }
                else
                {
                    _json.application_name = _json.application_name.Trim();
                }

                if (!Cron.IsApplicationNameCorrect(_json.application_name))
                {
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - наименование задания содержит не разрешенные символы!{Environment.NewLine}Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), знак тире (-)";
                    App.AddLog(info, null, showMessageMode, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;
                }

                // order
                if (_json.order == 0)
                {
                    info = $"{info_file}задание {_json.application_name} - order = 0 (или отсутствует), исправлено на минимальное значение 1";
                    App.AddLog(info, null, showMessageMode, true, logFile);

                    _json.order = 1;
                }

                // task
                if (string.IsNullOrWhiteSpace(_json.task))
                {
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - пустой task или без task";
                    App.AddLog(info, null, showMessageMode, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.task = null;
                }
                else
                {
                    _json.task = _json.task.Trim();
                }

                if (!Task.IsTaskNumberCorrect(_json.task))
                {
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - номер задачи {_json.task} содержит не разрешенные символы!{Environment.NewLine}Разрешены символы латинского алфавита в верхнем регистре (A-Z), знак подчеркивания (_), знак тире (-)";
                    App.AddLog(info, null, showMessageMode, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;
                }

                // comment
                if (string.IsNullOrWhiteSpace(_json.comment))
                {
                    _json.comment = null;
                }
                else
                {
                    _json.comment = _json.comment.Trim();
                }

                // command
                if (string.IsNullOrWhiteSpace(_json.command))
                {
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - пустой command или без command";
                    App.AddLog(info, null, showMessageMode, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.command = null;
                }
                else
                {
                    _json.command = _json.command.Trim();
                }

                // schedule
                if (string.IsNullOrWhiteSpace(_json.schedule))
                {
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - пустой schedule или без schedule";
                    App.AddLog(info, null, showMessageMode, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.schedule = null;
                }
                else
                {
                    _json.schedule = _json.schedule.Trim();
                }

                // state
                if (string.IsNullOrWhiteSpace(_json.state))
                {
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - пустой state или без state";
                    App.AddLog(info, null, showMessageMode, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.state = null;
                }

                if (_json.state != null)
                {
                    _json.state = _json.state.Trim().ToLower();
                }

                var found = ListStates.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == _json.state
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - ошибочное значение state = {_json.state}";
                    App.AddLog(info, null, showMessageMode, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.state = null;
                }

                // database
                if (string.IsNullOrWhiteSpace(_json.database))
                {
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - пустой database или без database";
                    App.AddLog(info, null, showMessageMode, true, logFile);
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
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - ошибочное значение database = {_json.database}";
                    App.AddLog(info, null, showMessageMode, true, logFile);
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
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - ошибочное значение stage = {_json.stage}";
                    App.AddLog(info, null, showMessageMode, true, logFile);
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
                        info = $"{info_file}задание {_json.application_name} номер {_json.order} - ошибочное значение в поле regions = {_reg}";
                        App.AddLog(info, null, showMessageMode, true, logFile);
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
                            info = $"{info_file}задание {_json.application_name} номер {_json.order} - ошибочное значение в поле exclude_regions = {_reg}";
                            App.AddLog(info, null, showMessageMode, true, logFile);
                            err += Environment.NewLine + Environment.NewLine + info;
                        }
                    }
                }

                // hosts
                if (string.IsNullOrWhiteSpace(_json.hosts)) _json.hosts = "single";

                _json.hosts = _json.hosts.Trim().ToLower();

                found = ListHosts.Where(x =>
                        x.Split(new char[] { '-' })[0].Trim() == _json.hosts
                    )
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - ошибочное значение hosts = {_json.hosts}";
                    App.AddLog(info, null, showMessageMode, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.hosts = "single";
                }

                // check
                if (string.IsNullOrWhiteSpace(_json.check))
                {
                    _json.check = null;
                }
                else
                {
                    _json.check = _json.check.Trim();
                }

                // team
                if (string.IsNullOrWhiteSpace(_json.team))
                {
                    _json.team = null;
                }
                else
                {
                    _json.team = _json.team.Trim();
                }

                // istemp
                _json.istemp = _json.istemp ?? 1;

                if (_json.istemp != 1 && _json.istemp != 2)
                {
                    info = $"{info_file}задание {_json.application_name} номер {_json.order} - ошибочное значение istemp = {_json.istemp}";
                    App.AddLog(info, null, showMessageMode, true, logFile);
                    err += Environment.NewLine + Environment.NewLine + info;

                    _json.istemp = 1;
                }
            }

            // проверяем на наличие дублей "наименование задания + номер"
            foreach (var item in cron_list
                .GroupBy(
                    x => $"application_name = {x.application_name.ToLower()}, order = {x.order}",
                    (uniq_name, crons) => new
                    {
                        uniq_name = uniq_name,
                        Count = crons.Count()
                    }
                )
                .Where(x => x.Count > 1)
            )
            {
                info = $"{info_file}больше одного задания {item.uniq_name}. Будет загружено только первое.";
                App.AddLog(info, null, showMessageMode, true, logFile);
                err += Environment.NewLine + Environment.NewLine + info;
            }

            return string.IsNullOrWhiteSpace(err);
        }


        /// <summary>
        /// разобрать json-текст в CronToJson
        /// </summary>
        /// <param name="_jsontext">json-текст</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="isVersion">=true - версия</param>
        /// <returns></returns>
        public static List<CronToJson> DeserializeJSON(string _jsontext, string logFile, bool isVersion)
        {
            List<CronToJson> result = null;

            if (!string.IsNullOrWhiteSpace(_jsontext))
            {
                if (isVersion)
                {
                    // версия

                    var json_version = JsonSerializer.Deserialize<CronVersionLoad>(_jsontext, new JsonSerializerOptions
                    {
                        IgnoreReadOnlyProperties = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                    if (json_version != null)
                    {
                        // соберем все в один список
                        if (
                            json_version.listcron != null &&
                            json_version.listcron.Count > 0
                        )
                        {
                            if (result == null) result = new List<CronToJson>();

                            foreach (var item in json_version.listcron)
                            {
                                item.istemp = 1;
                            }

                            result.AddRange(json_version.listcron);
                        }


                        if (
                            json_version.listtemp != null &&
                            json_version.listtemp.Count > 0
                        )
                        {
                            if (result == null) result = new List<CronToJson>();

                            foreach (var item in json_version.listtemp)
                            {
                                item.istemp = 2;
                            }

                            result.AddRange(json_version.listtemp);
                        }
                    }
                }
                else
                {
                    // задача

                    bool isMulti = _jsontext
                        .TrimStart(new char[] { '\n', '\r', ' ', '\t' })
                        .StartsWith("[");

                    if (isMulti)
                    {
                        // если несколько заданий
                        result = JsonSerializer.Deserialize<List<CronToJson>>(_jsontext, new JsonSerializerOptions
                        {
                            IgnoreReadOnlyProperties = true,
                            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        });
                    }
                    else
                    {
                        // если одно задание
                        result = new List<CronToJson>();
                        result.Add(JsonSerializer.Deserialize<CronToJson>(_jsontext, new JsonSerializerOptions
                        {
                            IgnoreReadOnlyProperties = true,
                            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        }));
                    }
                }
            }

            // перенумеруем
            if (result != null)
            {
                int _order = 0;

                foreach (var item in result)
                {
                    _order++;
                    item.order = _order;
                }
            }

            return result;
        }

        /// <summary>
        /// Загрузить json-файл со списком заданий
        /// </summary>
        /// <param name="project">проект</param>
        /// <param name="_dbregion">Тип региона по типу основной БД</param>
        /// <param name="all_list">Итоговый список</param>
        /// <param name="json_filepath">загружаем задания из json-файла</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="isVersion">=true - версия</param>
        /// <param name="isCheck">=true - проверять корректность\исправлять содержимое загружаемого файла</param>
        /// <returns></returns>
        public static bool LoadJSON(string project, string _dbregion, ObservableCollection<Cron> all_list, string json_filepath, string logFile, bool isVersion, bool isCheck)
        {
            if (all_list == null)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(json_filepath))
            {
                json_filepath = "";
            }

            // список заданий
            List<CronToJson> json_list = null;

            // загружаем json-файл
            if (
                !string.IsNullOrWhiteSpace(json_filepath) && 
                File.Exists(json_filepath)
            )
            {
                // загружаем файл
                try
                {
                    string jsonString = File.ReadAllText(json_filepath);

                    json_list = DeserializeJSON(jsonString, logFile, isVersion);
                }
                catch (Exception ex)
                {
                    App.AddLog($"Ошибка загрузки файла {json_filepath} :", ex, App.ShowMessageMode.SHOW, true, logFile);

                    return false;
                }

                if (json_list == null)
                {
                    json_list = new List<CronToJson>();
                }

                // проверяем корректность\исправляем содержимое загруженного файла
                if (
                    isCheck &&
                    !CheckJSON(json_list, json_filepath, logFile, out string err, App.ShowMessageMode.SHOW) &&
                    (System.Windows.Forms.MessageBox.Show($"Использовать данные из файла {json_filepath} не смотря на ошибки?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                )
                {
                    return false;
                }
            }

            if (json_list == null)
            {
                json_list = new List<CronToJson>();
            }

            int _order = 0;

            // выделяем папку внутри проекта и имя файла
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string gitfolder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
            string cronfolder = "";
            string cronfilename = "";

            if (json_filepath.StartsWith(gitfolder, StringComparison.OrdinalIgnoreCase))
            {
                cronfolder = Regex.Replace(json_filepath, gitfolder.Replace(@"\", @"\\"), "", RegexOptions.IgnoreCase)
                    .TrimStart(new char[] { Path.DirectorySeparatorChar });

                cronfilename = Path.GetFileName(cronfolder);
                cronfolder = Path.GetDirectoryName(cronfolder);
            }

            // новый список
            var new_list = new ObservableCollection<Cron>();

            // список уже загруженных заданий
            var ReLoadedCron = new List<string>();

            // перебираем старый список
            foreach (var old_item in all_list
                .Where(x => !string.IsNullOrWhiteSpace(x.application_name))
                .OrderBy(x => x.order)
            )
            {
                // проверяем, есть ли "старое" задание в загружаемом файле
                bool isreload = false;

                foreach (var item in json_list
                    .Select(x => x.ToCron(_dbregion, cronfolder, cronfilename))
                    .Where(x => x.IsKeyEqual(old_item))
                    .OrderBy(x => x.order)
                )
                {
                    isreload = true;

                    var new_item = item;
                    _order++;
                    new_item.order = _order;

                    new_list.Add(new_item);
                }

                if (isreload)
                {
                    // если "старая" задача есть среди загружаемых - берем новое содержимое, старое удаляем
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

            // перебираем новый список
            foreach (var item in json_list
                .Select(x => x.ToCron(_dbregion, cronfolder, cronfilename))
                .OrderBy(x => x.order)
            )
            {
                // проверяем, загружена ли "новая" задача в итоговый список
                bool isreloaded = false;

                foreach (var old_item in all_list
                    .Where(x => x.IsKeyEqual(item))
                    .OrderBy(x => x.order)
                )
                {
                    isreloaded = true;
                    break;
                }

                if (!isreloaded)
                {
                    // если "новой" задачи еще нет в итоговом списке - добавляем
                    var new_item = item;
                    _order++;
                    new_item.order = _order;

                    new_list.Add(new_item);
                }
            }

            all_list.Clear();
            _order = 0;
            foreach (var item in new_list.OrderBy(x => x.order))
            {
                _order++;
                item.order = _order;
                all_list.Add(item);
            }

            return true;
        }


        /// <summary>
        /// Список заданий
        /// </summary>
        /// <param name="project">проект</param>
        /// <param name="_dbregion">Тип региона по типу основной БД</param>
        /// <param name="logFile">лог-файл</param>
        /// <returns></returns>
        public static ObservableCollection<Cron> ListAllCron(string project, string _dbregion, string logFile)
        {
            // общий список заданий
            var ListCron = new ObservableCollection<Cron>();

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "cron");

            // перебираем базы
            foreach (var db in Utilities.Files.ListFilesInDir(folder, true, false, false))
            {
                // перебираем задания
                foreach (var file in Utilities.Files.ListFilesInDir(Path.Combine(folder, db), false, true, false, "", true, "*.json"))
                {
                    // загружаем задания
                    var jsonlist_cron = new ObservableCollection<Cron>();
                    Cron.LoadJSON(project, _dbregion, jsonlist_cron, file, logFile, false, false);
                    foreach (var item in jsonlist_cron)
                    {
                        ListCron.Add(item);
                    }
                }
            }

            return ListCron;
        }

        /// <summary>
        /// Сгенерировать текст json-файла заданий
        /// </summary>
        /// <param name="cron">Одно задание</param>
        /// <param name="cron_list">Список заданий</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="version">номер версии</param>
        /// <param name="isBox">собираем коробочную версию</param>
        /// <returns></returns>
        public static string GenerateJSON(Cron cron, ObservableCollection<Cron> cron_list, string logFile, string version, bool isBox)
        {
            string result = "";

            if (
                cron_list != null &&
                cron_list.Count > 0
            )
            {
                List<CronToJson> ListToJson = new List<CronToJson>();
                List<CronToJsonVersionSave> ListToJsonForVersion = new List<CronToJsonVersionSave>();
                List<CronToJsonVersionSave> ListToJsonForVersionTemp = new List<CronToJsonVersionSave>();
                List<CronToJsonBox> ListToJsonBox = new List<CronToJsonBox>();

                // соберем список, исправим нумерацию
                if (!string.IsNullOrWhiteSpace(version))
                {
                    // собираем задания версии
                    if (isBox)
                    {
                        // нумерация в разрезе folder

                        foreach (var _folder in cron_list
                            .Select(x => x.folder)
                            .Distinct()
                        )
                        {
                            // заполним список постоянных заданий
                            int _order = 0;
                            foreach (var item in cron_list
                                .Where(x =>
                                    x.folder == _folder &&
                                    x.istemp == 1 &&
                                    x.regions.Contains("all") &&
                                    x.state == "present"
                                )
                            )
                            {
                                _order++;
                                var _json = item.ToJsonBox();
                                _json.order = _order;
                                ListToJsonBox.Add(_json);
                            }
                        }
                    }
                    else
                    {
                        // сквозная нумерация

                        // заполним список постоянных заданий
                        int _order = 0;
                        foreach (var item in cron_list
                            .Where(x =>
                                x.istemp == 1
                            )
                        )
                        {
                            _order++;
                            var _json = item.ToJsonVersionSave();
                            _json.order = _order;
                            ListToJsonForVersion.Add(_json);
                        }

                        // заполним список временных заданий
                        _order = 0;
                        foreach (var item in cron_list
                            .Where(x =>
                                x.istemp == 2
                            )
                        )
                        {
                            _order++;
                            var _json = item.ToJsonVersionSave();
                            _json.order = _order;
                            ListToJsonForVersionTemp.Add(_json);
                        }
                    }
                }
                else
                {
                    // собираем задания задачи
                    // сквозная нумерация
                    int _order = 0;
                    foreach (var item in cron_list)
                    {
                        _order++;
                        var _json = item.ToJson();
                        _json.order = _order;
                        ListToJson.Add(_json);
                    }
                }

                // сгенерируем json
                try
                {
                    // сохрянем в версию
                    if (!string.IsNullOrWhiteSpace(version) && !isBox)
                    {
                        var v = new CronVersionSave();
                        v.version = version;
                        v.listcron = ListToJsonForVersion;
                        v.listtemp = ListToJsonForVersionTemp;

                        result = JsonSerializer.Serialize<CronVersionSave>(v,
                            new JsonSerializerOptions
                            {
                                IgnoreReadOnlyProperties = true,
                                WriteIndented = true,
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            });
                    }

                    // сохрянем в коробочную версию
                    if (!string.IsNullOrWhiteSpace(version) && isBox)
                    {
                        var v = new CronBoxSave();
                        v.version = version;
                        v.listcron = ListToJsonBox;
                        v.listtemp = new List<CronToJsonBox>();

                        result = JsonSerializer.Serialize<CronBoxSave>(v,
                            new JsonSerializerOptions
                            {
                                IgnoreReadOnlyProperties = true,
                                WriteIndented = true,
                                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                            });
                    }

                    // сохраняем задания задачи
                    if (string.IsNullOrWhiteSpace(version) &&
                        ListToJson != null &&
                        ListToJson.Count() > 0
                    )
                    {
                        if (ListToJson.Count == 1)
                        {
                            result = JsonSerializer.Serialize<CronToJson>(ListToJson.First(),
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
                            result = JsonSerializer.Serialize<List<CronToJson>>(ListToJson,
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
            else if (cron != null)
            {
                try
                {
                    // сохраняем одно задание
                    result = JsonSerializer.Serialize<CronToJson>(cron.ToJson(),
                        new JsonSerializerOptions
                        {
                            IgnoreReadOnlyProperties = true,
                            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        });
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
        /// Найти по номеру json-файл версии
        /// </summary>
        /// <param name="project">проект</param>
        /// <param name="version">номер версии</param>
        /// <param name="json_filepath">полный путь к найденному файлу</param>
        /// <param name="logFile">лог-файл</param>
        /// <returns></returns>
        public static bool FindVersion(string project, string version, out string json_filepath, string logFile)
        {
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string path_ver = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            double numversion = Release.VerAsNum(version);
            json_filepath = "";

            // Ищем существующий файл в папке version по номеру версии
            var files = Directory.GetFiles(path_ver, version + "*_cron.json").ToList();
            if (files == null) files = new List<string>(); //-V3022
            foreach (var filepath in files)
            {
                if (numversion == Release.VerAsNum(Release.GetNumVersion(prefix, Path.GetFileName(filepath))))  //-V3024
                {
                    // нашли
                    json_filepath = filepath;
                    string file = Path.GetFileName(json_filepath);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// =true - оба экземпляра совпадают по ключевым параметрам
        /// </summary>
        /// <param name="_cron">экземпляр Cron для сравнения с текущей</param>
        /// <returns></returns>
        public bool IsKeyEqual(Cron _cron) => (this.dbregion == _cron.dbregion) && this.ToJson().IsKeyEqual(_cron.ToJson());

        /// <summary>
        /// =true - оба экземпляра входят в один файл
        /// </summary>
        /// <param name="_cron">экземпляр Cron для сравнения с текущей</param>
        /// <returns></returns>
        public bool IsInFile(Cron _cron) => (this.dbregion == _cron.dbregion) && this.ToJson().IsInFile(_cron.ToJson());

    }

    /// <summary>
    /// Класс для сохранения заданий конкретной задачи в json-файл
    /// </summary>
    public class CronToJson
    {
        /// <summary>
        /// Конструктор CronToJson
        /// </summary>
        public CronToJson()
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
        /// СУБД
        /// </summary>
        public string dbms { get; set; }

        /// <summary>
        /// наименование задания
        /// </summary>
        public string application_name { get; set; }

        /// <summary>
        /// описание задания
        /// </summary>
        public string comment { get; set; }

        /// <summary>
        /// команда задания
        /// </summary>
        public string command {  get; set; }

        /// <summary>
        /// расписание задания
        /// </summary>
        public string schedule {  get; set; }

        /// <summary>
        /// Ограничение времени выполнения скрипта или команды
        /// </summary>
        public int? timeout { get; set; }

        /// <summary>
        /// признак актуальности задания
        /// </summary>
        public string state { get; set; }

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
        /// возможность параллельного запуска задачи
        /// </summary>
        public string hosts { get; set; }

        /// <summary>
        /// проверочный запрос
        /// </summary>
        public string check { get; set; }

        /// <summary>
        /// команда РТМИС, ответственная за задание
        /// </summary>
        public string team { get; set; }

        /// <summary>
        /// флаг временного задания
        /// </summary>
        public int? istemp { get; set; }

        /// <summary>
        /// Копирование
        /// </summary>
        /// <returns></returns>
        public CronToJson Copy()
        {
            CronToJson copy = (CronToJson)this.MemberwiseClone();

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

            return copy;
        }

        /// <summary>
        /// создать экземпляр Cron
        /// </summary>
        /// <param name="_dbregion">тип региона по типу основной БД</param>
        /// <param name="_folder">папка файла</param>
        /// <param name="_filename">имя файла</param>
        /// <returns></returns>
        public Cron ToCron(string _dbregion, string _folder, string _filename)
        {
            List<string> _regions = new List<string>();
            if (this.regions != null)
            {
                _regions.AddRange(this.regions);
            }
            if (_regions.Count == 0) _regions.Add("all");

            List<string> _exclude_regions = null;
            if (this.exclude_regions != null && this.exclude_regions.Count > 0)
            {
                _exclude_regions = new List<string>();
                _exclude_regions.AddRange(this.exclude_regions);
            }

            var cron = new Cron();

            cron.SetCron(this.order, this.task, _dbregion, this.state, this.database, this.stage, this.application_name, this.comment, this.command, this.schedule, _regions, _exclude_regions, this.timeout, this.hosts, this.check, this.team, this.istemp == 2);

            if (!string.IsNullOrWhiteSpace(_folder))
            {
                cron.folder = _folder;
            }

            if (!string.IsNullOrWhiteSpace(_filename))
            {
                cron.filename = _filename;
            }

            return cron;
        }

        /// <summary>
        /// =true - оба экземпляра совпадают по ключевым параметрам
        /// </summary>
        /// <param name="_cron">экземпляр Cron для сравнения с текущей</param>
        /// <returns></returns>
        public bool IsKeyEqual(CronToJson _cron)
        {
            // сравниваем основные ключевые параметры задания
            bool result =
                    this.dbms == _cron.dbms &&
                    this.application_name.ToLower() == _cron.application_name.ToLower() &&
                    this.database == _cron.database &&
                    this.stage == _cron.stage;

            if (result == false) return result;

            // сравниваем регионы
            if (
                this.regions != null && (this.regions.Count() > 0) && _cron.regions == null ||
                this.regions == null && _cron.regions != null && (_cron.regions.Count() > 0)
            )
            {
                return false;
            }

            if (
                this.regions != null &&
                _cron.regions != null &&
                (this.regions.Count() > 0) &&
                (_cron.regions.Count() > 0)
            )
            {
                // сравниваем по кол-ву
                if (this.regions.Count() != _cron.regions.Count())
                {
                    return false;
                }

                // сравниваем значения
                foreach (var item in _cron.regions)
                {
                    if (!this.regions.Contains(item))
                    {
                        return false;
                    }
                }
            }

            // сравниваем регионы-исключения
            if (
                this.exclude_regions != null && (this.exclude_regions.Count() > 0) && _cron.exclude_regions == null ||
                this.exclude_regions == null && _cron.exclude_regions != null && (_cron.exclude_regions.Count() > 0)
            )
            {
                return false;
            }

            if (
                this.exclude_regions != null &&
                _cron.exclude_regions != null &&
                (this.exclude_regions.Count() > 0) &&
                (_cron.exclude_regions.Count() > 0)
            )
            {
                // сравниваем по кол-ву
                if (this.exclude_regions.Count() != _cron.exclude_regions.Count())
                {
                    return false;
                }

                // сравниваем значения
                foreach (var item in _cron.exclude_regions)
                {
                    if (!this.exclude_regions.Contains(item))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// =true - оба экземпляра входят в один файл
        /// </summary>
        /// <param name="_cron">экземпляр Cron для сравнения с текущей</param>
        /// <returns></returns>
        public bool IsInFile(CronToJson _cron)
        {
            // сравниваем необходимые параметры задания для включения в один файл
            bool result =
                    this.dbms == _cron.dbms &&
                    this.application_name.ToLower() == _cron.application_name.ToLower() &&
                    this.database == _cron.database;

            return result;

        }
    }

    /// <summary>
    /// Класс для сохранения задания в json-файл со списком всех заданий в ветке версии для отображения в Confluence
    /// </summary>
    public class CronToJsonVersionSave
    {
        /// <summary>
        /// Конструктор CronToJsonVersionSave
        /// </summary>
        public CronToJsonVersionSave()
        {
            this.istemp = 1;
            this.order = 0;
            this.regions = new List<string>();
            this.exclude_regions = new List<string>();
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
        /// СУБД
        /// </summary>
        public string dbms { get; set; }

        /// <summary>
        /// наименование задания
        /// </summary>
        public string application_name { get; set; }

        /// <summary>
        /// описание задания
        /// </summary>
        public string comment { get; set; }

        /// <summary>
        /// команда задания
        /// </summary>
        public string command { get; set; }

        /// <summary>
        /// расписание задания
        /// </summary>
        public string schedule { get; set; }

        /// <summary>
        /// Ограничение времени выполнения скрипта или команды
        /// </summary>
        public int? timeout { get; set; }

        /// <summary>
        /// признак актуальности задания
        /// </summary>
        public string state { get; set; }

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
        /// возможность параллельного запуска задачи
        /// </summary>
        public string hosts { get; set; }

        /// <summary>
        /// проверочный запрос
        /// </summary>
        public string check { get; set; }

        /// <summary>
        /// команда РТМИС, ответственная за задание
        /// </summary>
        public string team { get; set; }

        /// <summary>
        /// флаг временного задания
        /// </summary>
        public int? istemp { get; set; }
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Класс для сохранения задания в json-файл со списком заданий коробочной версии и для отображения в Confluence
    /// </summary>
    public class CronToJsonBox
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public CronToJsonBox()
        {
            this.istemp = 1;
            this.order = 0;
            this.regions = new List<string>();
            this.exclude_regions = new List<string>();
        }

        /// <summary>
        /// флаг временного задания
        /// </summary>
        public int istemp { get; set; }

        /// <summary>
        /// Целевая база данных
        /// </summary>
        public string database { get; set; }

        /// <summary>
        /// СУБД
        /// </summary>
        public string dbms { get; set; }

        /// <summary>
        /// Номер по порядку
        /// </summary>
        public int order { get; set; }

        /// <summary>
        /// наименование задания
        /// </summary>
        public string application_name { get; set; }

        /// <summary>
        /// описание задания
        /// </summary>
        public string comment { get; set; }

        /// <summary>
        /// признак актуальности задания
        /// </summary>
        public string state { get; set; }

        /// <summary>
        /// команда задания
        /// </summary>
        public string command { get; set; }

        /// <summary>
        /// расписание задания
        /// </summary>
        public string schedule { get; set; }

        /// <summary>
        /// Целевые регионы
        /// </summary>
        public List<string> regions { get; set; }

        /// <summary>
        /// Кроме указанных регионов
        /// </summary>
        public List<string> exclude_regions { get; set; }

        /// <summary>
        /// Тип целевой БД
        /// </summary>
        public string stage { get; set; }

        /// <summary>
        /// Ограничение времени выполнения скрипта или команды
        /// </summary>
        public int? timeout { get; set; }

        /// <summary>
        /// возможность параллельного запуска задачи
        /// </summary>
        public string hosts { get; set; }

        /// <summary>
        /// проверочный запрос
        /// </summary>
        public string check { get; set; }

        /// <summary>
        /// команда РТМИС, ответственная за задание
        /// </summary>
        public string team { get; set; }

        /// <summary>
        /// Задача
        /// </summary>
        public string task { get; set; }
    }

    /// <summary>
    /// Файл версии Cron для загрузки версии
    /// </summary>
    public class CronVersionLoad
    {
        /// <summary>
        /// конструктор CronVersionLoad
        /// </summary>
        public CronVersionLoad()
        {
            this.listcron = new List<CronToJson>();
            this.listtemp = new List<CronToJson>();
        }

        /// <summary>
        /// номер версии
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// список постоянных заданий
        /// </summary>
        public List<CronToJson> listcron { get; set; }

        /// <summary>
        /// список временных заданий
        /// </summary>
        public List<CronToJson> listtemp { get; set; }
    }

    /// <summary>
    /// Файл версии Cron для сохранения и отображения версии в Confluence
    /// </summary>
    public class CronVersionSave
    {
        /// <summary>
        /// конструктор CronVersionSave
        /// </summary>
        public CronVersionSave()
        {
            this.listcron = new List<CronToJsonVersionSave>();
            this.listtemp = new List<CronToJsonVersionSave>();
        }

        /// <summary>
        /// номер версии
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// список постоянных заданий
        /// </summary>
        public List<CronToJsonVersionSave> listcron { get; set; }

        /// <summary>
        /// список временных заданий
        /// </summary>
        public List<CronToJsonVersionSave> listtemp { get; set; }
    }

    /// <summary>
    /// Файл версии Cron для сохранения и отображения в Confluence заданий коробочной версии
    /// </summary>
    public class CronBoxSave
    {
        /// <summary>
        /// конструктор CronVersionSave
        /// </summary>
        public CronBoxSave()
        {
            this.listcron = new List<CronToJsonBox>();
            this.listtemp = new List<CronToJsonBox>();
        }

        /// <summary>
        /// номер версии
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// список постоянных заданий
        /// </summary>
        public List<CronToJsonBox> listcron { get; set; }

        /// <summary>
        /// список временных заданий
        /// </summary>
        public List<CronToJsonBox> listtemp { get; set; }
    }
}
