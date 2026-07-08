// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using Microsoft.SqlServer.Management.Smo;
using SQLGen.Controls;
using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>Класс Задача</summary>
    public class Task : INotifyPropertyChanged
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

        // предыдущий номер
        //public string LastTaskNumber = "";

        // -------------------------------------------------------------------------------------------------------
        private string _task;
        /// <summary>Номер задачи</summary>
        public string TaskNumber
        {
            get
            {
                return _task ?? "";
            }
            set
            {
                _task = value;
                if (string.IsNullOrWhiteSpace(_task)) _task = "";
                _task = _task.Trim();

                OnPropertyChanged("TaskNumber");
                OnPropertyChanged("TaskNumberToFilename");
                OnPropertyChanged("TaskUrl");
                OnPropertyChanged("TaskPath");
                OnPropertyChanged("LogFile");
                OnPropertyChanged("LogFileMerge");
                OnPropertyChanged("LogFileRelease");
                OnPropertyChanged("LogFileTable");
                OnPropertyChanged("TaskFile");
                OnPropertyChanged("TaskSQL");
                OnPropertyChanged("TaskTO_GIT");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// =true - номер задачи корректный
        /// </summary>
        /// <param name="name">номер задачи</param>
        /// <returns></returns>
        public static bool IsTaskNumberCorrect(string name) =>
            string.IsNullOrWhiteSpace(name) ||
            name.IsMatch(@"^[A-Z0-9_-]+$");

        /// <summary>Форматирование номера задачи для включения в имя файла</summary>
        public string TaskNumberToFilename
        {
            get
            {
                return this.TaskNumber.Replace("-", String.Empty).Replace(" ", String.Empty).ToLower();
            }
        }

        /// <summary>
        /// Очищаем номер задачи от приписок
        /// </summary>
        /// <param name="_tasknumber">номер задачи, который надо очистить</param>
        /// <returns></returns>
        public static string ClearTaskNumber(string _tasknumber)
        {
            MatchCollection matches = SQLChangeset.regex_changesetname.Matches(_tasknumber);

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    return match.Groups[1].Value + "-" +
                        match.Groups[2].Value;
                }
            }

            return _tasknumber;
        }

        /// <summary>
        /// Очищенный от приписок номер задачи
        /// </summary>
        public string TaskNumberCleared => Task.ClearTaskNumber(TaskNumber);

        /// <summary>
        /// Сравнение номеров задач без учета приписок
        /// </summary>
        /// <param name="_tasknumber"></param>
        /// <param name="_check"></param>
        /// <returns></returns>
        public static bool IsMatchTaskNumber(string _tasknumber, string _check)
        {
            string _pattern = @"^" + Task.ClearTaskNumber(_tasknumber) + @"(.*)";

            return Regex.IsMatch(_check, _pattern, RegexOptions.IgnoreCase);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>URL задачи в Jira</summary>
        public string TaskUrl
        {
            get { return MainWindow.APPinfo.TaskUrlDefault + this.TaskNumber; }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _taskdesc;
        /// <summary>Описание задачи, дополнительные подробности</summary>
        public string TaskDesc
        {
            get { return _taskdesc ?? ""; }
            set
            {
                _taskdesc = value;
                if (string.IsNullOrWhiteSpace(_taskdesc)) _taskdesc = "";
                _taskdesc = _taskdesc.Trim();
                OnPropertyChanged("TaskDesc");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _taskexecutor;
        /// <summary>Исполнитель задачи (автор скриптов, входящих в задачу)</summary>
        public string TaskExecutor
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_taskexecutor)) return _taskexecutor;
                else return MainWindow.APPinfo.TaskExecutor;
            }
            set
            {
                _taskexecutor = value;
                if (string.IsNullOrWhiteSpace(_taskexecutor)) _taskexecutor = "";
                _taskexecutor = _taskexecutor.Trim();

                MainWindow.APPinfo.TaskExecutor = _taskexecutor;

                OnPropertyChanged("TaskExecutor");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        private string _lastymlfile;
        /// <summary>Последний yml-файл по задаче</summary>
        public string LastYMLFile
        {
            get { return _lastymlfile ?? ""; }
            set
            {
                _lastymlfile = value;
                if (string.IsNullOrWhiteSpace(_lastymlfile)) _lastymlfile = "";
                _lastymlfile = _lastymlfile.Trim();
                OnPropertyChanged("YMLFile");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Каталог задачи</summary>
        public string TaskPath
        {
            get
            {
                return Path.Combine(MainWindow.APPinfo.TaskFolder, this.TaskNumber);
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>task-файл задачи</summary>
        public string TaskFile
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.TaskNumber)) return "";
                else return Path.Combine(this.TaskPath, this.TaskNumber + ".task");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>log-файл задачи</summary>
        public string LogFile
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.TaskNumber)) return "";
                else return Path.Combine(this.TaskPath, this.TaskNumber + ".log");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>zip-архив для log-файлов задачи</summary>
        public string ZipLogFiles
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.TaskNumber)) return "";
                else return Path.Combine(this.TaskPath, this.TaskNumber + "_logs.zip");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>log-файл для merge</summary>
        public string LogFileMerge
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.TaskNumber)) return "";
                else return Path.Combine(this.TaskPath, this.TaskNumber + "_merge.log");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>log-файл для сборки версии</summary>
        public string LogFileRelease
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.TaskNumber)) return "";
                else return Path.Combine(this.TaskPath, this.TaskNumber + "_release.log");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>log-файл для информации о созданных таблицах</summary>
        public string LogFileTable
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.TaskNumber)) return "";
                else return Path.Combine(this.TaskPath, this.TaskNumber + "_table.log");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>sql-файл задачи</summary>
        public string TaskSQL
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.TaskNumber)) return "";
                else return Path.Combine(this.TaskPath, this.TaskNumber + ".sql");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>папка с отправленными в GIT файлами</summary>
        public string TaskTO_GIT
        {
            get
            {
                return Path.Combine(this.TaskPath, "TO_GIT");
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список скриптов для отправки в GIT</summary>
        public List<GITScript> Scripts { get; set; }

        /// <summary>Список YML-файлов для включения в версию</summary>
        public List<YMLFileInfo> ReleaseYMLFiles { get; set; }

        /// <summary>История yml-файлов</summary>
        public List<string> HistoryYMLFile { get; set; }

        /// <summary>Список задач для включения в версию</summary>
        public string ReleaseTaskList { get; set; }

        /// <summary>Номер версии</summary>
        public string ReleaseVersion { get; set; }

        /// <summary>Кумулятивная версия</summary>
        public bool ReleaseIsCumulative { get; set; }

        /// <summary>
        /// Разная информация в формате json, которую надо сохранить в задаче
        /// </summary>
        public string OtherJsonInfo { get; set; }

        /// <summary>
        /// Список действий при обновлении регионов MS
        /// </summary>
        public ObservableCollection<Deployment> ListDeploymentMS { get; set; }

        /// <summary>
        /// Список действий при обновлении регионов PG
        /// </summary>
        public ObservableCollection<Deployment> ListDeploymentPG { get; set; }

        /// <summary>
        /// Временный список действий, для обмена между MS и PG при сборке версий
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<Deployment> ListDeploymentTemp { get; set; } = new ObservableCollection<Deployment>();

        /// <summary>
        /// URL для json-файла Действий при обновлении регионов MS
        /// </summary>
        public string JSONFilenameDeploymentMS { get; set; } = "";

        /// <summary>
        /// URL для json-файла Действий при обновлении регионов PG
        /// </summary>
        public string JSONFilenameDeploymentPG { get; set; } = "";

        /// <summary>
        /// Список заданий для регионов MS
        /// </summary>
        public ObservableCollection<Cron> ListCronMS { get; set; }

        /// <summary>
        /// Список заданий для регионов PG
        /// </summary>
        public ObservableCollection<Cron> ListCronPG { get; set; }

        /// <summary>
        /// Временный список заданий, для обмена между MS и PG при сборке версий
        /// </summary>
        [JsonIgnore]
        public ObservableCollection<Cron> ListCronTemp { get; set; } = new ObservableCollection<Cron>();

        /// <summary>
        /// URL для json-файла Заданий для регионов MS (только для версии)
        /// </summary>
        public string JSONFilenameCronMS { get; set; } = "";

        /// <summary>
        /// URL для json-файла Заданий для регионов PG (только для версии)
        /// </summary>
        public string JSONFilenameCronPG { get; set; } = "";

        /// <summary>
        /// Имена yml-файлов, отправленных в последний раз
        /// </summary>
        public Dictionary<string, string> SendYMLFiles { get; set; } = new Dictionary<string, string>();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Текст с информацией о задаче для использования в Jira</summary>
        public string TaskInfoForJira()
        {
            string res = "";

            res += "Данные БД:" + Environment.NewLine;
            foreach (var item in Scripts.Where(w => w.JIRADataBD != "")
                                        .GroupBy(g => g.JIRADataBD.ToLower())
                                        .Select(s => s.First())
                                        .OrderBy(o => o.JIRADataBD)
                                        .ToList())
            {
                res += item.JIRADataBD + Environment.NewLine;
            }
            res += Environment.NewLine;

            res += "Объекты в БД:" + Environment.NewLine;
            foreach (var item in Scripts.Where(w => w.JIRAObjectBD != "")
                                        .GroupBy(g => g.JIRAObjectBD.ToLower())
                                        .Select(s => s.First())
                                        .OrderBy(o => o.JIRAObjectBD)
                                        .ToList())
            {
                res += item.JIRAObjectBD + Environment.NewLine;
            }
            res += Environment.NewLine;


            res += "Ссылка на yml:" + Environment.NewLine;
            foreach (var item in Scripts.GroupBy(g => g.GITProject)
                                         .Select(s => s.First())
                                         .OrderBy(o => o.GITProject)
                                         .ToList())
            {

                string mask = Utilities.GITProjects
                    .GetURLTaskByProject(item.GITProject)
                    .Replace("%BRANCH%", TaskNumber);

                if (!string.IsNullOrWhiteSpace(mask))
                {
                    string _ymlfile = LastYMLFile;

                    if (SendYMLFiles.ContainsKey(item.GITProject))
                    {
                        _ymlfile = SendYMLFiles[item.GITProject];
                    }

                    res = res + mask + _ymlfile + Environment.NewLine;
                }
            }

            if (
                !string.IsNullOrWhiteSpace(JSONFilenameDeploymentMS) &&
                JSONFilenameDeploymentMS.Contains("https://")
                )
            {
                res = res + JSONFilenameDeploymentMS + Environment.NewLine;
            }

            if (
                !string.IsNullOrWhiteSpace(JSONFilenameDeploymentPG) &&
                JSONFilenameDeploymentPG.Contains("https://")
                )
            {
                res = res + JSONFilenameDeploymentPG + Environment.NewLine;
            }


            if (!string.IsNullOrWhiteSpace(this.ReleaseVersion))
            {
                if (
                    !string.IsNullOrWhiteSpace(this.JSONFilenameCronMS) &&

                    this.JSONFilenameCronMS.Contains("https://")
                    )
                {
                    res = res + this.JSONFilenameCronMS + Environment.NewLine;
                }

                if (
                    !string.IsNullOrWhiteSpace(this.JSONFilenameCronPG) &&
                    this.JSONFilenameCronPG.Contains("https://")
                    )
                {
                    res = res + this.JSONFilenameCronPG + Environment.NewLine;
                }
            }
            else
            {
                foreach (var _url in ListCronMS
                    .Select(x => x.Fileurl(this.LogFile))
                    .Distinct()
                    )
                {
                    if (
                        !string.IsNullOrWhiteSpace(_url) &&
                        _url.Contains("https://")
                    )
                    {
                        res = res + _url + Environment.NewLine;
                    }
                }

                foreach (var _url in ListCronPG
                    .Select(x => x.Fileurl(this.LogFile))
                    .Distinct()
                    )
                {
                    if (
                        !string.IsNullOrWhiteSpace(_url) &&
                        _url.Contains("https://")
                    )
                    {
                        res = res + _url + Environment.NewLine;
                    }
                }
            }

            res += Environment.NewLine;

            // Локальные справочники
            var locallist = Utilities.Databases.GetAllLocalDBList(
                    MainWindow.MainConnect,
                    Scripts.Where(w => w.JIRADataBD != "" && w.JIRADataBD.ToLower() != "stg.localdblist")
                              .GroupBy(g => g.JIRADataBD.ToLower())
                              .Select(s => s.First())
                              .OrderBy(o => o.JIRADataBD)
                              .Select(x => x.JIRADataBD)
                              .ToList(),
                    out string queryString,
                    out string localError
                );

            if (locallist.Count() > 0)
            {
                res += "Действия при обновлении:" + Environment.NewLine + "Пересобрать локальные справочники:" + Environment.NewLine;

                //bool hasFreeDocMarker = false;

                foreach (var local in locallist)
                {
                    string s = "";
                    foreach (var item in local.ToList(new char[] { ',' }, true))
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
                        res += s + Environment.NewLine;

                        /*if (s.ToLower().Contains("freedocmarker"))
                        {
                            hasFreeDocMarker = true;
                        }*/
                    }
                }

                /*if (hasFreeDocMarker)
                {
                    res += "Очистить кэш спецмаркеров. Для этого:\nЗайти в \"АРМ ЦОД\" / Боковое меню / система / Управление кэшируемыми объектами / на открывшейся форме проставить флаг возле поля \"Включая созданные автоматически\" и в поле ввода текста ввести слово \"FreeDocMarker\" / Enter / выбрать все пункты списка / Удалить" + Environment.NewLine;
                }*/

                if (!string.IsNullOrWhiteSpace(localError))
                {
                    res += Environment.NewLine + "Отсутствие локальных справочников в тестовых и релизных БД:" + localError;
                }
                res += Environment.NewLine;
            }

            string checks = "";
            foreach (var item in Scripts
                .Where(w =>
                    w.JIRAObjectBD != "" &&
                    Utilities.GITProjects.GetDBTypeByProject(w.GITProject) == "PGSQL" &&
                    (
                        w.GITTypeObject.ToLower() == "procedure" ||
                        w.GITTypeObject.ToLower() == "function"
                    )
                )
                .GroupBy(g => g.JIRAObjectBD.ToLower())
                .Select(s => s.First())
                .OrderBy(o => o.JIRAObjectBD)
                .ToList())
            {
                checks += $"SELECT FROM dbo.xp_CheckScript (Obj := '{item.JIRAObjectBD}', isNoChild := 2);" + Environment.NewLine;
            }

            if (!string.IsNullOrWhiteSpace(checks))
            {
                res += "Проверить хранимки:" + Environment.NewLine;
                res += checks;
            }
            res += Environment.NewLine;

            if (locallist.Count() > 0)
            {
                res += "Запрос для проверки сущестования локальных справочников:" + Environment.NewLine;
                res += queryString + Environment.NewLine + Environment.NewLine;
            }
            res += Environment.NewLine;

            return res;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Инициализация задачи значениями по умолчанию</summary>
        public Task()
        {
            this.Scripts = new List<GITScript>();
            this.ReleaseYMLFiles = new List<YMLFileInfo>();
            this.HistoryYMLFile = new List<string>();
            this.ListDeploymentMS = new ObservableCollection<Deployment>();
            this.ListDeploymentPG = new ObservableCollection<Deployment>();
            this.ListDeploymentTemp = new ObservableCollection<Deployment>();
            this.ListCronMS = new ObservableCollection<Cron>();
            this.ListCronPG = new ObservableCollection<Cron>();
            this.ListCronTemp = new ObservableCollection<Cron>();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Заголовок в скрипты
        /// </summary>
        /// <param name="TargetDB">Целевая БД</param>
        /// <param name="Changeset">название changeset</param>
        /// <param name="isInit">=true - changset начальной инициализации проекта GIT</param>
        /// <param name="isAddinExistFile">=true - добавление в существующий файл</param>
        /// <returns></returns>
        public string TitleScript(Utilities.TargetDBType? TargetDB, string Changeset, bool isInit, bool isAddinExistFile)
        {
            string s = "";

            if (string.IsNullOrWhiteSpace(Changeset)) Changeset = "1";

            if (TargetDB == null)
            {
                s += "--comment: " + this.TaskNumber + Environment.NewLine;
            }
            else 
            {
                if (!isAddinExistFile)
                {
                    string DBType = "";
                    if (TargetDB == Utilities.TargetDBType.MSSQL)
                    {
                        DBType = "MSSQL";
                    }
                    else if ((TargetDB == Utilities.TargetDBType.PGSQL) || (TargetDB == Utilities.TargetDBType.EMD))
                    {
                        DBType = "PGSQL";
                    }

                    s += "--liquibase formatted sql" + Environment.NewLine;
                    s += YML.MakeChangeset("stripComments:false", true, isInit, Changeset, DBType, false, "") + Environment.NewLine;

                    if (isInit)
                    {
                        if (
                            (Changeset.ToUpper() == "SCHEMA") ||
                            (Changeset.ToUpper() == "TABLE") ||
                            (Changeset.ToUpper() == "SEQUENCE") ||
                            (Changeset.ToUpper() == "TYPE")
                            )
                        {
                            s += 
                                "--preConditions onFail:MARK_RAN" + Environment.NewLine +
                                "--precondition-sql-check expectedResult:execute select 'skip'" + Environment.NewLine;
                        }
                    }
                }
            }

            if (this.TaskDesc != "")
            {
                s = s + Environment.NewLine + "/* Description: " + Environment.NewLine;
                s = s + this.TaskDesc + Environment.NewLine;
                s = s + "*/" + Environment.NewLine;
            }

            return s;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Добавить новый скрипт в список для отправки в GIT</summary>
        /// <param name="GITOrder">Номер скрипта по порядку в yml-файле</param>
        /// <param name="GITScriptname">Имя исходного файла скрипта, подготовленного для отправки в GIT</param>
        /// <param name="GITProject">Проект GIT</param>
        /// <param name="GITTypeObject">Тип объекта (data, TABLE, VIEW, FUNCTION и т.п.)</param>
        /// <param name="GITSchemaObject">Схема в проекте GIT</param>
        /// <param name="GITNameObject">Название объекта GIT</param>
        /// <param name="GITFilename">Отформатирование уникальное имя файла для GIT</param>
        /// <param name="IsExistInGIT">Скрипт уже в GIT, не надо копировать</param>
        /// <param name="FirstChangesetName">Имя первого changeset</param>
        /// <returns>экземпляр  GITScript</returns>
        public GITScript AddScript(int? GITOrder, string GITScriptname, string GITProject, string GITTypeObject, string GITSchemaObject, string GITNameObject, string GITFilename, bool IsExistInGIT, string FirstChangesetName)
        {
            GITScript newScript = new GITScript();

            newScript.GITScriptname = Path.GetFileName(GITScriptname);
            newScript.GITProject = GITProject;
            newScript.GITTypeObject = GITTypeObject;
            newScript.GITShemaObject = GITSchemaObject;
            newScript.GITNameObject = GITNameObject;
            newScript.GITFilename = GITFilename;
            newScript.IsExistInGIT = IsExistInGIT;
            newScript.FirstChangesetName = FirstChangesetName;
            newScript.GITOrder = GITOrder;

            if (this.Scripts == null) this.Scripts = new List<GITScript>();

            if (newScript.GITOrder is null)
            {
                // заполним N п\п
                var max_order = this.Scripts.Max(x => x.GITOrder);
                if (max_order is null) max_order = 0;
                newScript.GITOrder = max_order + 10;
            }

            this.Scripts.Add(newScript);

            return newScript;
        }
    }


    // =========================================================================================================
    public partial class MainWindow
    {

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Текущая задача, с которой работает пользователь</summary>
        public static Task Task = new Task();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Собрать текущий статус основного окна программы</summary>
        public static string AppStatus
        {
            get
            {
                if ((MainConnect == null) || string.IsNullOrWhiteSpace(MainConnect.DBConnectionName))
                    return "Версия SQLGen: " + AppVersion + ", " + App.GITVersion + ", задача " + Task.TaskNumber;
                else
                    return "Версия SQLGen: " + AppVersion + ", " + App.GITVersion + ", задача " + Task.TaskNumber + ", подключение " + MainConnect.DBConnectionName;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Версия программы</summary>
        public static string AppVersion
        {
            get
            {
                Assembly thisAssem = typeof(MainWindow).Assembly;
                AssemblyName thisAssemName = thisAssem.GetName();
                var ver = thisAssemName.Version.ToString();

#if DEBUG
                ver = ver + " (DEBUG)";
#endif
                return ver;
            }
        }

        /// <summary>
        /// Список всех доступных проектов
        /// </summary>
        public static List<string> ListExistedProjects = new List<string>();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Инициализация текущей задачи</summary>
        /// <param name="_task">Экземпляр задачи для инициализации текущей задачи или null</param>
        public void SetTask(Task _task)
        {
            tbTaskFolder.Text = APPinfo.TaskFolder;
            TaskFolderChanged();
            tbGITFolder.Text = APPinfo.GITFolder;
            GITFolderChanged();
            Utilities.Controls.FillComboBoxProjects(cbGITProject, true, true);
            cbGITProject.SelectedIndex = 0;

            ListExistedProjects.Clear();
            foreach (var item in cbGITProject.Items.OfType<string>().Where(x => x != "ВСЕ"))
            {
                if (!ListExistedProjects.Contains(item))
                {
                    ListExistedProjects.Add(item);
                }
            }

            // ------------------------------------------------------------------
            // диагностическая информация в лог-файл при запуске приложения
            if (isStartup)
            {
                LogAPPinfo();
            }
            // ------------------------------------------------------------------

            if (Task == null) Task = new Task();

            tbTaskNumber.Text = "";

            tbTaskDesc.Text = "";
            tbTaskExecutor.Text = APPinfo.TaskExecutor;
            tbYMLFile.Text = "";


            Task.Scripts.Clear();
            Task.HistoryYMLFile.Clear();
            Task.SendYMLFiles.Clear();

            if (_task != null)
            {
                tbTaskNumber.Text = _task.TaskNumber;
                tbTaskDesc.Text = _task.TaskDesc;
                tbTaskExecutor.Text = _task.TaskExecutor;
                tbYMLFile.Text = _task.LastYMLFile;
                Task.LastYMLFile = _task.LastYMLFile;

                if (_task.Scripts != null)
                {
                    foreach (var item in _task.Scripts)
                    {
                        Task.AddScript(item.GITOrder, item.GITScriptname, item.GITProject, item.GITTypeObject, item.GITShemaObject, item.GITNameObject, item.GITFilename, item.IsExistInGIT, item.FirstChangesetName);
                    }
                }

                if (_task.HistoryYMLFile != null)
                {
                    foreach (var item in _task.HistoryYMLFile)
                    {
                        AddHistoryYMLFile(item);
                    }
                }

                if (_task.SendYMLFiles != null)
                {
                    foreach (var item in _task.SendYMLFiles)
                    {
                        Task.SendYMLFiles.Add(item.Key, item.Value);
                    }
                }
            }
            else Task.TaskNumber = "";

            Task.ReleaseYMLFiles.Clear();
            Task.ReleaseTaskList = null;
            Task.ReleaseVersion = null;
            Task.ReleaseIsCumulative = true;
            Task.OtherJsonInfo = null;
            Task.ListDeploymentMS.Clear();
            Task.ListDeploymentPG.Clear();
            Task.ListDeploymentTemp.Clear();
            Task.ListCronMS.Clear();
            Task.ListCronPG.Clear();
            Task.ListCronTemp.Clear();
            Task.JSONFilenameDeploymentMS = "";
            Task.JSONFilenameDeploymentPG = "";
            Task.JSONFilenameCronMS = "";
            Task.JSONFilenameCronPG = "";

            if (_task != null)
            {
                Task.ReleaseTaskList = _task.ReleaseTaskList;
                Task.ReleaseVersion = _task.ReleaseVersion;
                Task.ReleaseIsCumulative = _task.ReleaseIsCumulative;
                Task.OtherJsonInfo = _task.OtherJsonInfo;
                Task.JSONFilenameDeploymentMS = _task.JSONFilenameDeploymentMS;
                Task.JSONFilenameDeploymentPG = _task.JSONFilenameDeploymentPG;
                Task.JSONFilenameCronMS = _task.JSONFilenameCronMS;
                Task.JSONFilenameCronPG = _task.JSONFilenameCronPG;

                if (_task.ReleaseYMLFiles != null)
                {
                    foreach (var item in _task.ReleaseYMLFiles)
                    {
                        item.IsFiltered1 = true;
                        item.IsFiltered2 = true;
                        item.IsFiltered3 = true;
                        item.IsFiltered4 = true;
                        Task.ReleaseYMLFiles.Add(item.Copy());
                    }
                }

                if (_task.ListDeploymentMS != null)
                {
                    foreach (var item in _task.ListDeploymentMS)
                    {
                        Task.ListDeploymentMS.Add(item.Copy());
                    }
                }

                if (_task.ListDeploymentPG != null)
                {
                    foreach (var item in _task.ListDeploymentPG)
                    {
                        Task.ListDeploymentPG.Add(item.Copy());
                    }
                }

                if (_task.ListCronMS != null)
                {
                    foreach (var item in _task.ListCronMS)
                    {
                        Task.ListCronMS.Add(item.Copy());
                    }
                }

                if (_task.ListCronPG != null)
                {
                    foreach (var item in _task.ListCronPG)
                    {
                        Task.ListCronPG.Add(item.Copy());
                    }
                }
            }

            TaskNumberChanged();
            RefreshYMLFileTask();

            dgScriptsInTask.ItemsSource = Task.Scripts;
            dgScriptsInTaskRefresh();

            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Сохранить описание задачи в файл номерзадачи.task</summary>
        /// <param name="_task">Экземпляр задачи</param>
        /// <param name="isPush">=true - выполнить push</param>
        public static void SaveTask(Task _task, bool isPush)
        {
            if (
                (_task != null) && 
                (!string.IsNullOrWhiteSpace(APPinfo.TaskFolder)) && 
                (!string.IsNullOrWhiteSpace(_task.TaskNumber))
            )
            {
                if (!Directory.Exists(APPinfo.TaskFolder))
                {
                    Directory.CreateDirectory(APPinfo.TaskFolder);
                }

                if (!Directory.Exists(_task.TaskPath))
                {
                    Directory.CreateDirectory(_task.TaskPath);
                }

                string _path = Path.GetDirectoryName(_task.TaskFile);
                string _file = Path.GetFileName(_task.TaskFile);

                if (
                    APPinfo.isTaskReleaseCooperative && // если включен кооперативный режим работы с релизными задачами
                    _task.TaskNumber.ToLower().StartsWith("rm-") // это релизная задача
                )
                {
                    _path = APPinfo.TaskReleasePath;
                }

                if (!Directory.Exists(_path))
                {
                    Directory.CreateDirectory(_path);
                }

                string filename = Path.Combine(_path, _file);

                Utilities.Files.BackupFile(filename, _task.TaskPath);

                try
                {
                    string jsonString = JsonSerializer.Serialize<Task>(_task, Other.OptionsJSON);
                    File.WriteAllText(filename, jsonString);

                    if (
                        isPush &&
                        APPinfo.isTaskReleaseCooperative && // если включен кооперативный режим работы с релизными задачами
                        _task.TaskNumber.ToLower().StartsWith("rm-") // это релизная задача //-V3125
                    )
                    {
                        string folder = APPinfo.SqlGenReleasePath;
                        if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                        App.AddLog($"git push в проект sqlgen-release в папке {APPinfo.SqlGenReleasePath}", null, App.ShowMessageMode.NONE, true, "");

                        Utilities.External.ExecuteFile(
                                APPinfo.SqlGenReleasePath,
                                Path.Combine(APPinfo.SqlGenReleasePath, "push.sh"),
                                _task.TaskNumber,
                                true,
                                false,
                                false,
                                false,
                                ""
                            );

                        if (
                            File.Exists(filename) &&
                            filename != _task.TaskFile &&
                            File.Exists(_task.TaskFile)
                        )
                        {
                            // удалим локальный дубль
                            File.Delete(_task.TaskFile);
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, "");
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Считать описание задачи из файла</summary>
        /// <param name="filename">Файл с описанием задачи</param>
        /// <param name="isManual">=true - файл выбран вручную</param>
        public bool LoadTask(string filename, bool isManual)
        {
            bool result = false;

            string _path = Path.GetDirectoryName(filename);
            string _file = Path.GetFileName(filename);

            if (
                !isManual &&
                APPinfo.isTaskReleaseCooperative && // если включен кооперативный режим работы с релизными задачами
                Task.TaskNumber.ToLower().StartsWith("rm-") // это релизная задача
            )
            {
                App.AddLog($"git pull из проекта sqlgen-release в папке {APPinfo.SqlGenReleasePath}", null, App.ShowMessageMode.NONE, true, "");

                string folder = APPinfo.SqlGenReleasePath;
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                Utilities.External.ExecuteFile(
                        APPinfo.SqlGenReleasePath,
                        Path.Combine(APPinfo.SqlGenReleasePath, "pull.sh"),
                        folder,
                        true,
                        false,
                        false,
                        false,
                        ""
                    );

                _path = APPinfo.TaskReleasePath;

                // проверим наличие файла в кооперативном проекте
                if (File.Exists(Path.Combine(_path, _file))) 
                {
                    filename = Path.Combine(_path, _file);
                }
            }

            if ((!string.IsNullOrWhiteSpace(filename)) && File.Exists(filename))
            {
                try
                {
                    string jsonString = File.ReadAllText(filename);
                    Task loadTask = JsonSerializer.Deserialize<Task>(jsonString, Other.oldOptionsJSON);
                    SetTask(loadTask);

                    result = true;
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, "");
                }
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Изменился номер задачи
        /// </summary>
        /// <param name="isAlways">=true - принудительно</param>
        /// <returns></returns>
        private bool TaskNumberChanged(bool isAlways = false)
        {
            // если случайно добавили имя файла + переводим в верхний регистр (кроме test)
            tbTaskNumber.Text = tbTaskNumber.Text
                .Replace("\"","")
                .Replace("/", "")
                .Trim();

            if (!APPinfo.NoUpperBranch.Contains(tbTaskNumber.Text, StringComparer.OrdinalIgnoreCase))
            {
                tbTaskNumber.Text = tbTaskNumber.Text
                    .ToUpper()
                    .Replace(".YML", "")
                    .Replace(".JSON", "")
                    .Replace(".SQL", "");
            }

            if (isAlways || (Task.TaskNumber.Trim().ToUpper() != tbTaskNumber.Text.Trim().ToUpper()))
            {
                // сохранить предыдущую задачу
                SaveTask(Task, false);

                if (!string.IsNullOrWhiteSpace(tbTaskNumber.Text))
                {
                    // сохраняем новый номер
                    string num = tbTaskNumber.Text;

                    // сбрасываем задачу
                    SetTask(null);

                    // восстанавливаем номер
                    tbTaskNumber.Text = num;
                }
                else
                {
                    // сбрасываем задачу
                    SetTask(null);
                }

                string dir = Path.Combine(APPinfo.TaskFolder, tbTaskNumber.Text.Trim()); //-V3095

                if ((!string.IsNullOrWhiteSpace(tbTaskNumber.Text)) && (!Directory.Exists(dir)))
                {
                    Directory.CreateDirectory(dir);
                }

                //Task.LastTaskNumber = Task.TaskNumber;
                Task.TaskNumber = tbTaskNumber.Text;
                tbTaskNumber.AddHistory(tbTaskNumber.Text);
                //AddHistoryTask(Task.TaskNumber);

                if (!string.IsNullOrWhiteSpace(Task.TaskNumber))
                {
                    if (!LoadTask(Task.TaskFile, false))
                    {
                        // создать task-файл, если его еще нет
                        SaveTask(Task, false);
                    }
                }

                if ((!string.IsNullOrWhiteSpace(Task.TaskNumber)) && (!File.Exists(Task.TaskSQL)))
                {
                    try
                    {
                        // создать sql-файл, если его еще нет
                        File.WriteAllText(Task.TaskSQL, Task.TitleScript(null, null, false, false));
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("", ex, App.ShowMessageMode.SHOW, true, Task.LogFile);
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(tbYMLFile.Text))
            {
                if (!string.IsNullOrWhiteSpace(Task.TaskNumber))
                    tbYMLFile.Text = Task.TaskNumber + ".yml";
                else
                    tbYMLFile.Text = "";
            }

            if (!string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                cbLogFile.Items.Clear();
                cbLogFile.Items.Add(Task.LogFile);
                cbLogFile.Items.Add(Task.LogFileTable);
                cbLogFile.Items.Add(Task.LogFileMerge);
                cbLogFile.Items.Add(Task.LogFileRelease);
                cbLogFile.SelectedIndex = 0;
            }

            tbStatusLeft.Value = MainWindow.AppStatus;
            return true;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выход из поля Номер задачи</summary>
        private void tbTaskNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!TaskNumberChanged()) //-V3022
            {
                Dispatcher.BeginInvoke((ThreadStart)delegate
                {
                    if (!tabTask.IsSelected) tabTask.IsSelected = true;
                    if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                });
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата клавиша в поле Номер задачи</summary>
        private void tbTaskNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //TaskNumberChanged();
                btOpenTask.Focus();
                e.Handled = true;
            }
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выбран пункт меню Новая задача</summary>
        private void miNewTask_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            // сохранить предыдущую задачу
            SaveTask(Task, false);
            // инициализировать новую
            SetTask(null);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выбран пункт меню Сохранить задачу
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void miSaveTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                return;
            }

            SaveTask(Task, true);

            MessageBox.Show("Задача " + Task.TaskNumber + " сохранена !");
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выбран пункт меню Открыть задачу</summary>
        private void miOpenTask_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            // обнуляем задачу
            miNewTask_Click(sender, e);
            // открыть задачу из файла
            string filename = Controls.Dialogs.OpenTaskDialog(APPinfo.TaskFolder);
            LoadTask(filename, true);
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Изменилось поле Описание задачи</summary>
        private void tbTaskDesc_TextChanged(object sender, TextChangedEventArgs e)
        {
            Task.TaskDesc = tbTaskDesc.Text;
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Обновить из Jira</summary>
        private void btFromJira_Click(object sender, RoutedEventArgs e)
        {
            if ((!string.IsNullOrWhiteSpace(Task.TaskUrl)) && JiraHTML.OpenLoginJira(MainWindow.Task.LogFile))
            {
                JiraHTML html = new JiraHTML();

                var task = new Dictionary<string, string>
                {
                    { Task.TaskNumber, Task.TaskUrl }
                };

                // парсинг
                html.LoadJiraPages(task, this, null, null, x => tbTaskDesc.Text = x.Description, null, null, MainWindow.Task.LogFile).GetAwaiter();
            }
        }

        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Обновить историю задач в tbTaskNumber</summary>
        public void RefreshHistoryTask()
        {
            string _task = tbTaskNumber.Text;
            tbTaskNumber.SelectedIndex = -1;
            tbTaskNumber.Items.Clear();

            foreach (var line in HistoryTask)
            {
                tbTaskNumber.Items.Add(line);
            }
            tbTaskNumber.Text = _task;
            if (HistoryTask.Contains(_task)) tbTaskNumber.SelectedItem = _task;
        }
        */
        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Загрузить историю задач</summary>
        public void LoadHistoryTask()
        {
            string filename = Path.Combine(App.AppPath, "HistoryTask.json");
            if (File.Exists(filename))
                try
                {
                    string jsonString = File.ReadAllText(filename);
                    HistoryTask = JsonSerializer.Deserialize<List<string>>(jsonString, new JsonSerializerOptions { IgnoreReadOnlyProperties = true, WriteIndented = true });
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE);
                }

            RefreshHistoryTask();
        }
        */

        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Добавить запрос и Сохранить историю задач</summary>
        public void AddHistoryTask(string _task, int max = 20)
        {
            if (!string.IsNullOrWhiteSpace(_task))
            {
                if ((HistoryTask.Count==0) || (HistoryTask[0] != _task))
                    HistoryTask.Insert(0, _task);
            }
            if (HistoryTask.Count > max) HistoryTask.RemoveAt(HistoryTask.Count - 1);

            string filename = Path.Combine(App.AppPath, "HistoryTask.json");
            string jsonString = "";

            if (!string.IsNullOrWhiteSpace(filename)) //-V3022
            {
                try
                {
                    jsonString = JsonSerializer.Serialize<List<string>>(HistoryTask, new JsonSerializerOptions { IgnoreReadOnlyProperties = true, WriteIndented = true });
                    File.WriteAllText(filename, jsonString);
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE);
                }
            }

            RefreshHistoryTask();
        }
        */

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выбрана задача из Истории задач</summary>
        private void tbTaskNumber_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbTaskNumber.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbTaskNumber.SelectedItem;
                tbTaskNumber.Text = (string)cbItem.Tag;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Закрылся список из Истории задач</summary>
        private void tbTaskNumber_DropDownClosed(object sender, EventArgs e)
        {
            btOpenTask.Focus();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Изменилось поле YML-файл</summary>
        private void tbYMLFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            Task.LastYMLFile = tbYMLFile.Text;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Обновить историю yml в tbYMLFile</summary>
        public void RefreshYMLFileTask()
        {
            string _yml = tbYMLFile.Text;
            tbYMLFile.SelectedIndex = -1;
            tbYMLFile.Items.Clear();

            foreach (var line in Task.HistoryYMLFile)
            {
                tbYMLFile.Items.Add(line);
            }
            tbYMLFile.Text = _yml;
            if (Task.HistoryYMLFile.Contains(_yml)) tbYMLFile.SelectedItem = _yml;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить yml в историю
        /// </summary>
        /// <param name="_yml">yml-файл</param>
        public void AddHistoryYMLFile(string _yml)
        {
            if (!string.IsNullOrWhiteSpace(_yml))
            {
                if (!Task.HistoryYMLFile.Contains(_yml)) Task.HistoryYMLFile.Insert(0, _yml);
            }

            Task.LastYMLFile = _yml;

            RefreshYMLFileTask();
        }

       
    }

}


