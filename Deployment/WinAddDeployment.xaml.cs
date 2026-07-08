// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using SQLGen.Controls;
using SQLGen.Utilities;
using System.Windows.Data;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;


namespace SQLGen
{
    /// <summary>
    /// Окно добавления\изменения FK
    /// </summary>
    public partial class WinAddDeployment : Window
    {
        /// <summary>
        /// Признак, что можно сохранить изменения
        /// </summary>
        public bool isOk = false;

        /// <summary>ссылка на экземпляр основного окна программы</summary>
        public MainWindow mainWindow;

        /// <summary>
        /// лог-файл
        /// </summary>
        public string logFile;

        /// <summary>
        /// ссылка на редактируемое Действие при обновлении
        /// </summary>
        Deployment deployment = new Deployment();

        /// <summary>Конструктор WinAddDeployment</summary>
        public WinAddDeployment()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("WinAddDeployment", this, mainGrid, null, null, null, MainWindow.Task.LogFile);
        }

        /// <summary>При открытии окна WinAddDeployment</summary>
        private void winAddDeployment_Activated(object sender, EventArgs e)
        {
            deployment = this.DataContext as Deployment;

            cbWhenTimeoutActions_SelectionChanged(null, null);
            cbWhenFailedActions_SelectionChanged(null, null);
        }

        /// <summary>При закрытии окна WinAddDeployment</summary>
        private void winAddDeployment_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinAddDeployment", this, null);
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            isOk = false;

            if (string.IsNullOrWhiteSpace(tbTask.Text))
            {
                App.AddLog("Укажите номер задачи", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (cbDBRegions.SelectedIndex == -1)
            {
                App.AddLog("Выберите тип региона по типу основной БД", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (cbTypes.SelectedIndex == -1)
            {
                App.AddLog("Выберите тип автоматизации", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (cbPositions.SelectedIndex == -1)
            {
                cbPositions.SelectedIndex = 2;
            }

            if (cbDatabases.SelectedIndex == -1)
            {
                App.AddLog("Выберите целевую БД", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (cbStages.SelectedIndex == -1)
            {
                App.AddLog("Выберите тип целевой БД", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (deployment.type == "liquibase")
            {
                if (
                    string.IsNullOrWhiteSpace(deployment.script) || 
                    (
                        !deployment.script.TrimAllSpace().ToLower().EndsWith(".yml") && //-V3080
                        !deployment.script.TrimAllSpace().ToLower().EndsWith(".sql") 
                    )
                )
                {
                    App.AddLog("Заполните поле \"Скрипт\" - в нем должна быть ссылка на yml или sql-файл из проекта GIT", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }

                if (
                    deployment.database != deployment.DBAliasFromText &&
                    !(
                        (
                            deployment.database == "main" || 
                            deployment.database == "reestr" || 
                            deployment.database == "report" ||
                            deployment.database == "promed_rpt"
                        ) && 
                        deployment.DBAliasFromText == "promed"
                    )
                )
                {
                    App.AddLog("Необходимо выбрать целевую БД в соответствии с проектом GIT, который указан в поле \"Скрипт\"", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }

                if (
                    (
                        deployment.GITProjectFromText == "liquibase_project_new" ||
                        deployment.GITProjectFromText == "msdbupdate_new"
                    ) &&
                    deployment.database != "promed_rpt"
                )
                {
                    App.AddLog("Необходимо выбрать целевую БД в соответствии с проектом GIT, который указан в поле \"Скрипт\"", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }

                if (
                    (deployment.dbregion == "PG SQL" && 
                    (
                        deployment.DBTypeFromText == "MSSQL" ||
                        MainWindow.APPinfo.ListProjects_ONLY_MS.Contains(deployment.GITProjectFromText)
                    )) ||
                    (
                        deployment.dbregion == "MS SQL" &&
                        MainWindow.APPinfo.ListProjects_ONLY_PG.Contains(deployment.GITProjectFromText)
                    )
                )
                {
                    App.AddLog("Необходимо выбрать \"Тип региона по типу основной БД\" в соответствии с проектом GIT, который указан в поле \"Скрипт\"", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }
            }

            if (deployment.type == "upload_file")
            {
                if (
                    string.IsNullOrWhiteSpace(deployment.file) || 
                    !deployment.file.TrimAllSpace().ToLower().EndsWith(".zip") //-V3080
                )
                {
                    App.AddLog("Заполните поле \"Файл\" - в нем должна быть ссылка на zip-архив из проекта GIT", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }

                if (
                   deployment.GITProjectFromText != "msdbupdate_new" &&
                   deployment.GITProjectFromText != "liquibase_project_new"
                )
                {
                    App.AddLog("Проект в ссылке на файл должен быть liquibase_project_new или msdbupdate_new", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }
            }

            if (
                cbTimeout.IsChecked == true &&
                tbHour.Text == "0" &&
                tbMinute.Text == "0" &&
                tbSecond.Text == "0"
            )
            {
                App.AddLog("Укажите продолжительность таймаута (часы, минуты, секунды)", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (
                cbWhenTimeout.IsChecked == true &&
                (
                    cbTimeout.IsChecked == false ||
                    deployment.timeout == 0
                )
            )
            {
                App.AddLog("Укажите продолжительность таймаута (часы, минуты, секунды)", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (
                cbWhenFailed.IsChecked == true &&
                cbWhenFailedActions.SelectedIndex == -1
                )
            {
                App.AddLog("Укажите тип действия в блоке действий в случае ошибки", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (
                cbWhenFailed.IsChecked == true &&
                deployment.when_failed_action == "cron"
                )
            {
                if (string.IsNullOrWhiteSpace(tbWhenFailedName.Text))
                {
                    App.AddLog("Укажите наименование для задания в блоке действий в случае ошибки", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }
                if (string.IsNullOrWhiteSpace(tbWhenFailedJob.Text))
                {
                    App.AddLog("Укажите команду для задания в блоке действий в случае ошибки", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }
                if (string.IsNullOrWhiteSpace(tbWhenFailedSchedule.Text))
                {
                    App.AddLog("Укажите расписание для задания в блоке действий в случае ошибки", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }
            }

            if (
                cbWhenFailed.IsChecked == true &&
                deployment.when_failed_action == "liquibase"
                )
            {
                if (string.IsNullOrWhiteSpace(tbWhenFailedScript.Text))
                {
                    App.AddLog("Укажите скрипт в блоке действий в случае ошибки", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }
            }

            if (
                cbWhenTimeout.IsChecked == true &&
                cbWhenTimeoutActions.SelectedIndex == -1
                )
            {
                App.AddLog("Укажите тип действия в блоке действий в случае превышения таймаута", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }


            if (
                cbWhenTimeout.IsChecked == true &&
                deployment.when_timeout_action == "cron"
                )
            {
                if (string.IsNullOrWhiteSpace(tbWhenTimeoutName.Text))
                {
                    App.AddLog("Укажите наименование для задания в блоке действий в случае превышения таймаута", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }
                if (string.IsNullOrWhiteSpace(tbWhenTimeoutJob.Text))
                {
                    App.AddLog("Укажите команду для задания в блоке действий в случае превышения таймаута", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }
                if (string.IsNullOrWhiteSpace(tbWhenTimeoutSchedule.Text))
                {
                    App.AddLog("Укажите расписание для задания в блоке действий в случае превышения таймаута", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }
            }

            if (
                cbWhenTimeout.IsChecked == true &&
                deployment.when_timeout_action == "liquibase"
                )
            {
                if (string.IsNullOrWhiteSpace(tbWhenTimeoutScript.Text))
                {
                    App.AddLog("Укажите скрипт в блоке действий в случае превышения таймаута", null, App.ShowMessageMode.SHOW, false, "");
                    return;
                }
            }

            isOk = true;
            this.Close();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            isOk = false;
            this.Close();
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !_regex.IsMatch(text);
        }

        private new void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }

        // Use the DataObject.Pasting Handler 
        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void btSetWhenFailedSchedule_Click(object sender, RoutedEventArgs e)
        {
            WinCronBuilder WinCronBuilder = new WinCronBuilder();
            WinCronBuilder.ShowDialog();

            if (WinCronBuilder.isOk == true)
            {
                tbWhenFailedSchedule.Text = WinCronBuilder.CronExpression;
            }
        }

        private void btSetWhenTimeoutSchedule_Click(object sender, RoutedEventArgs e)
        {
            WinCronBuilder WinCronBuilder = new WinCronBuilder();
            WinCronBuilder.ShowDialog();

            if (WinCronBuilder.isOk == true)
            {
                tbWhenTimeoutSchedule.Text = WinCronBuilder.CronExpression;
            }
        }

        private void tbScript_TextChanged(object sender, TextChangedEventArgs e)
        {
            var project = Deployment.GetGITProjectByScript(tbScript.Text);

            if (
                    project == "liquibase_project_new" ||
                    project == "msdbupdate_new"
            )
            {
                cbDatabases.SelectedItem = "promed_rpt";
            }
            else
            {
                var found = Deployment.ListDatabases.Where(x =>
                   x.Split(new char[] { '-' })[0].Trim() == GITProjects.GetDBAliasByProject(project)
               )
               .FirstOrDefault();

                if (found != null)
                {
                    found = found.Split(new char[] { '-' })[0].Trim();

                    if (
                        (
                            deployment.database == "main" ||
                            deployment.database == "reestr" ||
                            deployment.database == "report"
                        ) &&
                        found == "promed"
                    )
                    {
                        // ничего не меняем
                    }
                    else
                    {
                        cbDatabases.SelectedItem = found;
                    }
                }
            }

            if (tbScript.Text.Contains("/version/"))
            {
                deployment.Position = "primary";
            }
        }

        private void tbFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            var found = Deployment.ListDatabases.Where(x =>
                   x.Split(new char[] { '-' })[0].Trim() == GITProjects.GetDBAliasByProject(Deployment.GetGITProjectByScript(tbFile.Text))
               )
               .FirstOrDefault();

            if (found != null)
            {
                cbDatabases.SelectedItem = found.Split(new char[] { '-' })[0].Trim();
            }
        }

        private void cbTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbTypes.SelectedIndex != -1)
            {
                if (
                    deployment.type == "liquibase" ||
                    deployment.type == "sql" ||
                    deployment.type == "restart_replica"
                )
                {
                    tbFile.Text = "";
                }

                if (
                    deployment.type == "upload_file" ||
                    deployment.type == "restart_replica"
                )
                {
                    tbScript.Text = "";
                }

                if (deployment.type == "upload_file")
                {
                    deployment.Position = "before";
                }
            }
        }

        private void cbDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (
                cbDatabases.SelectedIndex != -1 &&
                deployment.database == "lis"
            )
            {
                deployment.Set_lcbListRegions(new List<string> { "Башкирия" });
                lcbRegions.Items.Refresh();
            }
        }

        /// <summary>
        /// Нажата кнопка "Найти скрипт или файл"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFind_Click(object sender, RoutedEventArgs e)
        {
            string url = FindFile(deployment.dbregion, deployment.type);

            if (!string.IsNullOrWhiteSpace(url))
            {
                // сохраним url
                if (deployment.isScriptEnabled)
                {
                    tbScript.Text = url;

                    /*if (url.Contains("/version/"))
                    {
                        deployment.Position = "primary";
                    }*/
                }
                if (deployment.isFileEnabled)
                {
                    tbFile.Text = url;
                }
            }
        }

        /// <summary>
        /// Найти файл в git
        /// </summary>
        /// <param name="_dbregion">регионы основной БД</param>
        /// <param name="_type">тип действия</param>
        /// <returns></returns>
        private string FindFile(string _dbregion, string _type)
        {
            // заполнить список возможных проектов
            List<string> ListProjects = new List<string>();

            foreach (var item in MainWindow.ListExistedProjects
                .OrderBy(x =>
                {
                    string ord = "999";
                    if (x == "dev_promed_pg") ord = "001";
                    else if (x == "dev_promed_ms") ord = "002";
                    else if (x == "dev_lis_pg") ord = "003";
                    else if (x == "dev_emd_pg") ord = "004";
                    else if (x == "liquibase_project_new") ord = "005";
                    else if (x == "msdbupdate_new") ord = "006";
                    else if (x == "promedlistest2") ord = "007";
                    else if (x == "emd") ord = "008";
                    return ord + x;
                }
                ))
            {
                string _dbtype = Utilities.GITProjects.GetDBTypeByProject(item);

                if (
                    (_dbregion == "PG SQL" &&
                    (
                        _dbtype == "MSSQL" ||
                        MainWindow.APPinfo.ListProjects_ONLY_MS.Contains(item)
                    )) ||
                    (
                       _dbregion == "MS SQL" &&
                        MainWindow.APPinfo.ListProjects_ONLY_PG.Contains(item)
                    )
                )
                {
                    // пропускаем
                }
                else
                {
                    if (!ListProjects.Contains(item))
                    {
                        ListProjects.Add(item);
                    }
                }
            }

            // выбрать проект и ветку
            if (
                !Utilities.GIT.SelectGITProject(ListProjects, null, out string project, out string branch, logFile, false) ||
                string.IsNullOrWhiteSpace(project)
                )
            {
                return "";
            }

            // выбрать файл в проекте
            string folder = Utilities.GITProjects.GetFolderByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, folder);
            string file = "";
            try
            {
                if (_type == "upload_file")
                {
                    file = Controls.Dialogs.OpenFileDialog(path, ".zip", "(*.zip)|*.zip|Все файлы (*.*)|*.*");
                }
                else
                {
                    file = Controls.Dialogs.OpenFileDialog(path, "", "Все файлы (*.*)|*.*");
                }
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile);
            }

            if (!string.IsNullOrWhiteSpace(file))
            {
                // конвертируем путь к локальному файлу в url в web-репозитории
                file = Utilities.GITProjects.ConvertFilepathToUrl(project, file, branch, logFile);
            }

            return file;
        }



        private void btFindWhenFailedScript_Click(object sender, RoutedEventArgs e)
        {
            string url = FindFile(deployment.dbregion, "liquibase");

            if (!string.IsNullOrWhiteSpace(url))
            {
                // сохраним url
                if (deployment.hasWhenFailed)
                {
                    tbWhenFailedScript.Text = url;
                }
            }
        }

        private void btFindWhenFailedScriptAfter_Click(object sender, RoutedEventArgs e)
        {
            string url = FindFile(deployment.dbregion, "liquibase");

            if (!string.IsNullOrWhiteSpace(url))
            {
                // сохраним url
                if (deployment.hasWhenFailed)
                {
                    tbWhenFailedScriptAfter.Text = url;
                }
            }
        }

        private void btFindWhenTimeoutScript_Click(object sender, RoutedEventArgs e)
        {
            string url = FindFile(deployment.dbregion, "liquibase");

            if (!string.IsNullOrWhiteSpace(url))
            {
                // сохраним url
                if (deployment.hasWhenTimeout)
                {
                    tbWhenTimeoutScript.Text = url;
                }
            }
        }

        private void btFindWhenTimeoutScriptAfter_Click(object sender, RoutedEventArgs e)
        {
            string url = FindFile(deployment.dbregion, "liquibase");

            if (!string.IsNullOrWhiteSpace(url))
            {
                // сохраним url
                if (deployment.hasWhenTimeout)
                {
                    tbWhenTimeoutScriptAfter.Text = url;
                }
            }
        }

        private void tbTask_LostFocus(object sender, RoutedEventArgs e)
        {
            tbTask.Text = tbTask.Text
                .ToUpper()
                .Replace("https://jira.is-mis.ru/browse/".ToUpper(), "")
                .Replace("https://jira.rtmis.ru/browse/".ToUpper(), "")
                .Trim();
        }

        private void cbWhenTimeoutActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbWhenTimeoutActions.SelectedIndex != -1)
            {
                tbWhenTimeoutName.IsEnabled = false;
                tbWhenTimeoutJob.IsEnabled = false;
                tbWhenTimeoutSchedule.IsEnabled = false;
                tbWhenTimeoutScript.IsEnabled = false;
                tbWhenTimeoutScriptAfter.IsEnabled = false;

                if (
                    deployment.when_timeout != null &&
                    deployment.when_timeout.action == "cron"
                )
                {
                    tbWhenTimeoutName.IsEnabled = true;
                    tbWhenTimeoutJob.IsEnabled = true;
                    tbWhenTimeoutSchedule.IsEnabled = true;
                    tbWhenTimeoutScript.Text = "";
                    tbWhenTimeoutScriptAfter.Text = "";
                }

                if (
                    deployment.when_timeout != null &&
                    deployment.when_timeout.action == "liquibase"
                )
                {
                    tbWhenTimeoutName.Text = "";
                    tbWhenTimeoutJob.Text = "";
                    tbWhenTimeoutSchedule.Text = "";
                    tbWhenTimeoutScript.IsEnabled = true;
                    tbWhenTimeoutScriptAfter.IsEnabled = true;
                }
            }
        }

        private void cbWhenFailedActions_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbWhenFailedActions.SelectedIndex != -1)
            {
                tbWhenFailedName.IsEnabled = false;
                tbWhenFailedJob.IsEnabled = false;
                tbWhenFailedSchedule.IsEnabled = false;
                tbWhenFailedScript.IsEnabled = false;
                tbWhenFailedScriptAfter.IsEnabled = false;

                if (
                    deployment.when_failed != null &&
                    deployment.when_failed.action == "cron"
                )
                {
                    tbWhenFailedName.IsEnabled = true;
                    tbWhenFailedJob.IsEnabled = true;
                    tbWhenFailedSchedule.IsEnabled = true;
                    tbWhenFailedScript.Text = "";
                    tbWhenFailedScriptAfter.Text = "";
                }

                if (
                    deployment.when_failed != null &&
                    deployment.when_failed.action == "liquibase"
                )
                {
                    tbWhenFailedName.Text = "";
                    tbWhenFailedJob.Text = "";
                    tbWhenFailedSchedule.Text = "";
                    tbWhenFailedScript.IsEnabled = true;
                    tbWhenFailedScriptAfter.IsEnabled = true;
                }
            }
        }
    }

    /// <summary>
    /// класс для реализации ListCheckedBox
    /// </summary>
    public class BoolStringClass
    {
        /// <summary>
        /// строка
        /// </summary>
        public string TheText { get; set; }

        /// <summary>
        /// =true - checked
        /// </summary>
        public bool IsSelected { get; set; }
    }

}
