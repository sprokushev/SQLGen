// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SQLGen.Controls;
using SQLGen.Utilities;
using SQLGen.Forms;
using Excel = Microsoft.Office.Interop.Excel;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace SQLGen
{
    /// <summary>
    /// Главное окно программы
    /// </summary>
    public partial class MainWindow
    {
        //public static Cursor WaitCursor = new Cursor(Path.Combine(App.AppPath, "Busy.cur"));

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Флаг успешного считывания настроек</summary>
        public bool isLoadInitSuccess = false;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Конструктор MainWindow</summary>
        public MainWindow()
        {
            MainWindow.LoadAPPinfo();

            InitializeComponent();
            tbStatusLeft.Value = MainWindow.AppStatus;

            isLoadInitSuccess = true;
            tbTaskNumber.InitHistory("HistoryTask.json", "");
            //LoadHistoryTask();
            KeyWord.FillKeyWords();

            cbMainConnect.SelectedIndex = -1;
            Utilities.Controls.RefreshConnectItems(cbMainConnect, APPinfo.LastDBConnectionName, null, null);

            tbFileEditor.Text = APPinfo.FileEditor;
            tbDirectoryEditor.Text = APPinfo.DirectoryEditor;
            tbScaleX.Text = APPinfo.GUI.scaleWindow.ScaleX.ToString();
            tbScaleY.Text = APPinfo.GUI.scaleWindow.ScaleY.ToString();

            try
            {
                cbDefaultFontFamily.ItemsSource = System.Windows.Media.Fonts.SystemFontFamilies.OrderBy(f => f.Source);
                cbDefaultFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
                if (string.IsNullOrWhiteSpace(APPinfo.GUI.scriptBoxDefault.ScriptBoxFont))
                {
                    cbDefaultFontFamily.SelectedItem = tbTaskNumber.FontFamily;
                }
                else
                {
                    cbDefaultFontFamily.SelectedItem = new System.Windows.Media.FontFamily(APPinfo.GUI.scriptBoxDefault.ScriptBoxFont);
                }

                if (string.IsNullOrWhiteSpace(APPinfo.GUI.scriptBoxDefault.ScriptBoxFontSize))
                {
                    cbDefaultFontSize.Text = tbTaskNumber.FontSize.ToString();
                }
                else
                {
                    cbDefaultFontSize.Text = APPinfo.GUI.scriptBoxDefault.ScriptBoxFontSize;
                }
            }
            catch (Exception)
            {
            }

            isExtendedLog.IsChecked = (APPinfo.ExtendedLog == "true");
            isImproveSQLinVersion.IsChecked = (APPinfo.ImproveSQLinVersion == "true");
            isUseNewFunc.IsChecked = (APPinfo.UseNewFunc == "true");
            isTaskReleaseCooperative.IsChecked = (APPinfo.TaskReleaseCooperative == "true");

            dgListGITProjects.ItemsSource = APPinfo.GITProjects;
            dgListGITProjects.Items.Refresh();

            // Проверяем наличие обязательных настроек
            if (!IsSettingsOk())
            {
                tabTask.Visibility = Visibility.Collapsed;
                tabSettings.IsSelected = true;

                if (string.IsNullOrWhiteSpace(APPinfo.TaskFolder))
                {
                    tbTaskFolder.Focus();
                }

                if (string.IsNullOrWhiteSpace(MainWindow.APPinfo.GITFolder))
                {
                    tbGITFolder.Focus();
                }
            }

            // обработка загрузки задачи из командной строки
            if ((App.Args != null) && (App.Args.Length > 0) && (App.Args[0] != ""))
            {

                try
                {
                    if (File.Exists(App.Args[0]))
                    {
                        // открыть задачу из task-файла, указанного в args[0]
                        string jsonString = File.ReadAllText(App.Args[0]);
                        Task loadTask = JsonSerializer.Deserialize<Task>(jsonString);
                        SetTask(loadTask);
                    }
                    else
                    {
                        SetTask(null);

                        // возможно это номер задачи
                        if (App.Args[0].ToLower() == "test")
                        {
                            tbTaskNumber.Text = "test";
                        }
                        else if (App.Args[0].ToUpper().StartsWith("PROMEDWEB-"))
                        {
                            tbTaskNumber.Text = App.Args[0].ToUpper();
                        }
                        else if (App.Args[0].ToUpper().StartsWith("BIP-"))
                        {
                            tbTaskNumber.Text = App.Args[0].ToUpper();
                        }
                        else if (App.Args[0].ToUpper().StartsWith("SMP-"))
                        {
                            tbTaskNumber.Text = App.Args[0].ToUpper();
                        }
                        else if (App.Args[0].ToUpper().StartsWith("RPMS-"))
                        {
                            tbTaskNumber.Text = App.Args[0].ToUpper();
                        }
                        else if (App.Args[0].ToUpper().StartsWith("RM-"))
                        {
                            tbTaskNumber.Text = App.Args[0].ToUpper();
                        }
                        else if (App.Args[0].ToUpper().StartsWith("CM-"))
                        {
                            tbTaskNumber.Text = App.Args[0].ToUpper();
                        }
                        else if (App.Args[0].ToUpper().StartsWith("OPS-"))
                        {
                            tbTaskNumber.Text = App.Args[0].ToUpper();
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, null);
                    SetTask(null);
                }
            }
            else SetTask(null);

            isStartup = false;

            DataContext = Task;

            // пользовательские настройки GUI
            Default.InitGUI("MainWindow", this, mainGrid, null, null, null, "");

            this.Title = "SQLGen " + AppVersion;

            if (!string.IsNullOrWhiteSpace(tbTaskNumber.Text))
            {
                tbTaskNumber_LostFocus(null, null);
            }

            if ((App.Args != null) && (App.Args.Length > 0) && (
                (App.Args[0].ToUpper() == "FIXCONNECTS") ||
                (App.Args[0].ToUpper() == "-FIXCONNECTS") ||
                (App.Args[0].ToUpper() == "/FIXCONNECTS")
                )
            )
            {
                // удаляем старые сервера
                APPinfo.DelDatabase("077-1-SQLDEV-02", "promeddev", "MSSQL");
                APPinfo.DelDatabase("172.29.4.5", "promeddev", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLDEV-01", "ProMedTest", "MSSQL");
                APPinfo.DelDatabase("172.29.4.4", "ProMedTest", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLDEV-01", "ProMedUfa", "MSSQL");
                APPinfo.DelDatabase("172.29.4.4", "ProMedUfa", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLDEV-01", "php_log", "MSSQL");
                APPinfo.DelDatabase("172.29.4.4", "php_log", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLDEV-01", "log_service", "MSSQL");
                APPinfo.DelDatabase("172.29.4.4", "log_service", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLDEV-01", "UserPortal", "MSSQL");
                APPinfo.DelDatabase("172.29.4.4", "UserPortal", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLDEV-01", "ac_mlo", "MSSQL");
                APPinfo.DelDatabase("172.29.4.4", "ac_mlo", "MSSQL");

                APPinfo.DelDatabase("172.29.4.2", "promedtest", "PGSQL");
                APPinfo.DelDatabase("172.29.4.2", "promedadygea", "PGSQL");
                APPinfo.DelDatabase("172.29.4.2", "promedlistest2", "PGSQL");
                APPinfo.DelDatabase("172.29.4.2", "promedlistest_ufa", "PGSQL");
                APPinfo.DelDatabase("172.29.4.2", "fer_log", "PGSQL");
                APPinfo.DelDatabase("172.29.4.2", "smp2dev", "PGSQL");
                APPinfo.DelDatabase("172.29.4.41", "EMD_dev", "PGSQL");
                APPinfo.DelDatabase("172.29.4.41", "EMD", "PGSQL");
                APPinfo.DelDatabase("172.29.4.41", "php_log", "PGSQL");
                APPinfo.DelDatabase("172.29.4.41", "log_service", "PGSQL");
                APPinfo.DelDatabase("172.29.4.41", "userportaltest", "PGSQL");
                APPinfo.DelDatabase("172.29.4.41", "ac_mlo", "PGSQL");

                APPinfo.DelDatabase("077-1-SQLREL-01", "ProMedWebRelease", "MSSQL");
                APPinfo.DelDatabase("172.29.6.62", "ProMedWebRelease", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLREL-01", "promedwebufarelease", "MSSQL");
                APPinfo.DelDatabase("172.29.6.62", "promedwebufarelease", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLREL-01", "log_service", "MSSQL");
                APPinfo.DelDatabase("172.29.6.62", "log_service", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLREL-01", "php_log", "MSSQL");
                APPinfo.DelDatabase("172.29.6.62", "php_log", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLREL-01", "userportalrelease", "MSSQL");
                APPinfo.DelDatabase("172.29.6.62", "userportalrelease", "MSSQL");
                APPinfo.DelDatabase("077-1-SQLREL-01", "ac_mlo", "MSSQL");
                APPinfo.DelDatabase("172.29.6.62", "ac_mlo", "MSSQL");

                APPinfo.DelDatabase("172.29.6.61", "userportalrelease", "PGSQL");
                APPinfo.DelDatabase("172.29.6.61", "lisrelease", "PGSQL");
                APPinfo.DelDatabase("172.29.6.61", "lisrelease_ufa", "PGSQL");
                APPinfo.DelDatabase("172.29.6.61", "php_log", "PGSQL");
                APPinfo.DelDatabase("172.29.6.61", "promedrelease", "PGSQL");
                APPinfo.DelDatabase("172.29.6.61", "EMDrelease", "PGSQL");
                APPinfo.DelDatabase("172.29.6.61", "log_service", "PGSQL");
                APPinfo.DelDatabase("172.29.6.61", "ac_mlo", "PGSQL");
                APPinfo.DelDatabase("172.29.6.61", "smp2release", "PGSQL");

                Application.Current.Shutdown();
            }

            if ((App.Args != null) && (App.Args.Length > 1) && (App.Args[1].ToUpper() == "UPLOADTOGIT"))
            {
                // Выгрузка БД для GIT
                if (
                    (App.Args.Length >= 5) &&
                    (!string.IsNullOrWhiteSpace(Task.TaskNumber))
                    )
                {
                    WinUploadFromBD WinUploadFromBD = new WinUploadFromBD();

                    if (MainConnect != null)
                    {
                        Utilities.Controls.RefreshConnectItems(WinUploadFromBD.cbConnectSQL, MainConnect.DBConnectionName, null, null);
                    }

                    WinUploadFromBD.Show();
                    WinUploadFromBD.AutoUpload();
                }
            }

            if (
                (App.Args != null) &&
                (App.Args.Length > 1) &&
                (App.Args[1].ToUpper() == "CHANGEINGIT") &&
                (!string.IsNullOrWhiteSpace(Task.TaskNumber))
               )
            {
                // Изменения файлов в проекте GIT
                WinChangesetInGIT WinChangesetInGIT = new WinChangesetInGIT();
                WinChangesetInGIT.mainWindow = this;
                WinChangesetInGIT.Show();
                WinChangesetInGIT.AutoChange();
            }

            if (
                (App.Args != null) &&
                (App.Args.Length >= 6) &&
                (App.Args[1].ToUpper() == "EXPORT") &&
                (!string.IsNullOrWhiteSpace(Task.TaskNumber))
                )
            {
                // Выгрузить результат запроса
                try
                {
                    string Connection = App.Args[2];
                    string SQL = App.Args[3];
                    string ExportFile = App.Args[4];
                    string ExportFormat = App.Args[5];

                    // выбираем подключение
                    if (!string.IsNullOrWhiteSpace(Connection))
                    {
                        Utilities.Controls.SetComboBoxConnectByName(cbMainConnect, Connection);
                        cbMainConnect_SelectionChanged(null, null);

                        if (
                            (cbMainConnect != null) &&
                            (cbMainConnect.SelectedIndex != -1) &&
                            (MainConnect != null) &&
                            (!string.IsNullOrWhiteSpace(MainConnect.DBConnectionName)) &&
                            (!string.IsNullOrWhiteSpace(ExportFile)) &&
                            (!string.IsNullOrWhiteSpace(SQL))
                        )
                        {
                            QueryDB Query = new QueryDB();

                            Query.InsUpdDTType = Utilities.InsUpdDTType.NONE;
                            Query.ScriptType = Utilities.ScriptType.INSERT_BULK_TABLE;
                            if (MainConnect.DBType == "PGSQL")
                            {
                                Query.SQLQuery = SQL
                                    .Replace("[", "\"")
                                    .Replace("]", "\"");
                            }
                            else
                            {
                                Query.SQLQuery = SQL;
                            }

                            Query.DataTable = new DataTable();
                            Query.isAddCheckUnique = false;
                            Query.isAddEmptyString = false;
                            Query.isAddDel = false;

                            Query.DataTable = MainConnect.FillDataTable(Query.SQLQuery, out string Messages);

                            this.Cursor = Cursors.Wait;

                            if (ExportFormat == "PG_CSV")
                            {
                                Query.GITProject = "dev_promed_pg";
                                Query.GenCSV(null, ExportFile).GetAwaiter();
                            }

                            if (ExportFormat == "MS_CSV")
                            {
                                Query.GITProject = "dev_promed_ms";
                                Query.GenCSV(null, ExportFile).GetAwaiter();
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, null);
                    this.Cursor = Cursors.Arrow;
                }

                Application.Current.Shutdown();
            }
           
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Переключение на основное окно приложения</summary>
        private void winMain_Activated(object sender, EventArgs e)
        {
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Обработчик закрытия основного окна приграммы</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void winMain_Closed(object sender, EventArgs e)
        {
            if (isLoadInitSuccess == true)
            {
                // если стартовая инициализация была успешной, значит при завершении сохраним 
                // проверка нужна на случай сбоя при начальной загрузке, чтобы не затереть все настройки пустым файлом

                // пользовательские настройки GUI
                Default.SaveGUI("MainWindow", this, null);

                SaveConnects();
                SaveAPPinfo();
                Search.SaveSearches();
            }

            SaveTask(Task, true);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Новый скрипт - данные
        /// </summary>
        /// <param name="Connect">соединение</param>
        /// <param name="Text">текст запроса</param>
        public static void NewQuery(ConnectDB Connect, string Text)
        {
            WinQuery WinQuery = new WinQuery();

            string project = "";

            if (Connect != null)
            {
                Utilities.Controls.RefreshConnectItems(WinQuery.cbConnectSQL, Connect.DBConnectionName, null, null);
                Utilities.Controls.RefreshConnectItems(WinQuery.cbConnectTarget, Connect.DBConnectionName, null, null);

                project = Connect.GITProject;
            }

            Utilities.Controls.RefreshRegionItems(WinQuery.cbRegion, project, true);

            WinQuery.cbConnectSQL_LostFocus(null, null);

            WinQuery.tbSQL.Text = Text;

            WinQuery.Show();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Новый скрипт - данные</summary>
        private void btNewQuery_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            NewQuery(MainConnect, "");
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Новый скрипт - структура таблицы</summary>
        private void btNewStructure_Click(object sender, RoutedEventArgs e)
        {

            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            WinTable WinTable = new WinTable();
            //App.AddLog("new WinTable()", null, App.ShowMessageMode.NONE);

            string project = "";

            if (MainConnect != null)
            {
                Utilities.Controls.RefreshConnectItems(WinTable.cbConnectSQL, MainConnect.DBConnectionName, null, null);
                project = MainConnect.GITProject;
            }

            Utilities.Controls.RefreshRegionItems(WinTable.cbRegion, project);

            WinTable.isStart = false;
            WinTable.cbConnectSQLChanged();

            //App.AddLog("before WinTable.Show()", null, App.ShowMessageMode.NONE);

            WinTable.Show();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Разобрать скрипт с процедурами</summary>
        private void btProcScript_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                tbTaskNumber.Focus();
                return;
            }

            string filename = Controls.Dialogs.OpenFileDialog(Task.TaskPath);

            // определить номер скрипта 
            string ScriptNumber = (Utilities.Files.MaxScriptNumber(Task.TaskPath) + 1).ToString();

            // разобрать скрипт
            Utilities.Databases.SaveProcScript("", Task.TaskNumber, filename, ScriptNumber, cbMainConnect);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выгрузить процедуры и представления из БД</summary>
        private void btUnloadFromBD_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                tbTaskNumber.Focus();
                return;
            }

            WinUploadFromBD WinUploadFromBD = new WinUploadFromBD();

            if (MainConnect != null)
            {
                Utilities.Controls.RefreshConnectItems(WinUploadFromBD.cbConnectSQL, MainConnect.DBConnectionName, null, null);
            }

            WinUploadFromBD.Show();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Открыть окно "Формирование релизной версии"</summary>
        private void btBuildRelease_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                tbTaskNumber.Focus();
                return;
            }

            WinRelease WinRelease = new WinRelease();
            WinRelease.mainWindow = this;

            WinRelease.listProjects.Clear();
            foreach (var item in Utilities.Files.ListFilesInDir(MainWindow.APPinfo.GITFolder, true, false, false))
            {
                string project = Utilities.GITProjects.GetProjectByFolder(item);
                string DBType = Utilities.GITProjects.GetDBTypeByProject(project);
                if (
                    GITProjects.IsDEVProject(project) && // собираем версии только для "новых" проектов
                    ((DBType == "MSSQL") || (DBType == "PGSQL"))
                )
                {
                    WinRelease.listProjects.Add(project);
                }
            }

            WinRelease.cbFilterList1.Items.Clear();
            WinRelease.cbFilterList2.Items.Clear();
            WinRelease.cbFilterList3.Items.Clear();

            WinRelease.cbFilterList1.Items.Add("ВСЕ");
            WinRelease.cbFilterList2.Items.Add("ВСЕ");
            WinRelease.cbFilterList3.Items.Add("ВСЕ");
            WinRelease.cbFilterList1.Items.Add("Изменившиеся после обновления");
            WinRelease.cbFilterList2.Items.Add("Изменившиеся после обновления");
            WinRelease.cbFilterList3.Items.Add("Изменившиеся после обновления");

            WinRelease.cbFilterList1.Items.Add("ПРОВЕРИТЬ");

            WinRelease.cbFilterList1.Items.Add("Включено в релиз");
            WinRelease.cbFilterList2.Items.Add("Включено в релиз");
            WinRelease.cbFilterList3.Items.Add("Включено в релиз");
            WinRelease.cbFilterList1.Items.Add("НЕ включено в релиз");
            WinRelease.cbFilterList2.Items.Add("НЕ включено в релиз");
            WinRelease.cbFilterList3.Items.Add("НЕ включено в релиз");

            WinRelease.cbFilterList1.Items.Add("Выбранный проект");
            WinRelease.cbFilterList2.Items.Add("Merge НЕ прошел");

            foreach (var item in APPinfo.GITProjects)
            {
                WinRelease.cbFilterList1.Items.Add("Проект: " + item.GITProject);
                WinRelease.cbFilterList2.Items.Add("Проект: " + item.GITProject);
                WinRelease.cbFilterList3.Items.Add("Проект: " + item.GITProject);
                WinRelease.cbFilterList1.Items.Add("Проект: " + item.DEVProject);
                WinRelease.cbFilterList2.Items.Add("Проект: " + item.DEVProject);
                WinRelease.cbFilterList3.Items.Add("Проект: " + item.DEVProject);
            }

            WinRelease.cbFilterList1.Items.Add("Проект: неизвестный");
            WinRelease.cbFilterList2.Items.Add("Проект: неизвестный");
            WinRelease.cbFilterList3.Items.Add("Проект: неизвестный");
            WinRelease.cbFilterList1.Items.Add("Задача без yml-файла");
            WinRelease.cbFilterList2.Items.Add("Задача без yml-файла");
            WinRelease.cbFilterList3.Items.Add("Задача без yml-файла");
            WinRelease.cbFilterList1.Items.Add("Задача с yml-файлом");
            WinRelease.cbFilterList2.Items.Add("Задача с yml-файлом");
            WinRelease.cbFilterList3.Items.Add("Задача с yml-файлом");
            WinRelease.cbFilterList1.Items.Add("Регион не указан");
            WinRelease.cbFilterList2.Items.Add("Регион не указан");
            WinRelease.cbFilterList3.Items.Add("Регион не указан");
            WinRelease.cbFilterList1.Items.Add("Регион БАЗОВЫЙ");
            WinRelease.cbFilterList2.Items.Add("Регион БАЗОВЫЙ");
            WinRelease.cbFilterList3.Items.Add("Регион БАЗОВЫЙ");
            WinRelease.cbFilterList1.Items.Add("Регион НЕ базовый");
            WinRelease.cbFilterList2.Items.Add("Регион НЕ базовый");
            WinRelease.cbFilterList3.Items.Add("Регион НЕ базовый");
            WinRelease.cbFilterList1.Items.Add("Есть Базовая региональность БД");
            WinRelease.cbFilterList2.Items.Add("Есть Базовая региональность БД");
            WinRelease.cbFilterList3.Items.Add("Есть Базовая региональность БД");
            WinRelease.cbFilterList1.Items.Add("Нет Базовая региональность БД");
            WinRelease.cbFilterList2.Items.Add("Нет Базовая региональность БД");
            WinRelease.cbFilterList3.Items.Add("Нет Базовая региональность БД");
            WinRelease.cbFilterList1.Items.Add("Есть Downtime");
            WinRelease.cbFilterList2.Items.Add("Есть Downtime");
            WinRelease.cbFilterList3.Items.Add("Есть Downtime");
            WinRelease.cbFilterList1.Items.Add("Нет Downtime");
            WinRelease.cbFilterList2.Items.Add("Нет Downtime");
            WinRelease.cbFilterList3.Items.Add("Нет Downtime");
            WinRelease.cbFilterList1.Items.Add("Есть Действия при обновлении");
            WinRelease.cbFilterList2.Items.Add("Есть Действия при обновлении");
            WinRelease.cbFilterList3.Items.Add("Есть Действия при обновлении");
            WinRelease.cbFilterList1.Items.Add("Нет Действия при обновлении");
            WinRelease.cbFilterList2.Items.Add("Нет Действия при обновлении");
            WinRelease.cbFilterList3.Items.Add("Нет Действия при обновлении");
            WinRelease.cbFilterList1.Items.Add("Есть Данные в БД");
            WinRelease.cbFilterList2.Items.Add("Есть Данные в БД");
            WinRelease.cbFilterList3.Items.Add("Есть Данные в БД");
            WinRelease.cbFilterList1.Items.Add("Нет Данные в БД");
            WinRelease.cbFilterList2.Items.Add("Нет Данные в БД");
            WinRelease.cbFilterList3.Items.Add("Нет Данные в БД");
            WinRelease.cbFilterList1.Items.Add("Есть Объекты БД");
            WinRelease.cbFilterList2.Items.Add("Есть Объекты БД");
            WinRelease.cbFilterList3.Items.Add("Есть Объекты БД");
            WinRelease.cbFilterList1.Items.Add("Нет Объекты БД");
            WinRelease.cbFilterList2.Items.Add("Нет Объекты БД");
            WinRelease.cbFilterList3.Items.Add("Нет Объекты БД");
            WinRelease.cbFilterList1.SelectedIndex = 0;
            WinRelease.cbFilterList2.SelectedIndex = 0;
            WinRelease.cbFilterList3.SelectedIndex = 0;

            WinRelease.Show();
            WinRelease.dgYMLFilesRefresh();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Открыть окно "Формирование Deployment Plan релиза"</summary>
        private void btBuildReleaseDeployment_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                tbTaskNumber.Focus();
                return;
            }

            // Определим, для какого префикса версии (сервиса) собираем версию с действиями при обновлении: prmd, rpms, smp, bi
            if (string.IsNullOrWhiteSpace(Task.ReleaseVersion))
            {
                App.AddLog("Сначала на форме 'Собрать релиз' заполните номер версии", null, App.ShowMessageMode.SHOW, true, null);
                return;
            }

            string prefix = Task.ReleaseVersion.Split(new char[] { '.' })[0];

            if (
                string.IsNullOrWhiteSpace(prefix) ||
                (prefix != "prmd" && prefix != "rpms" && prefix != "smp" && prefix != "bi")
            )
            {
                // Спросим, для какого префикса версии (сервиса) собираем версию с действиями при обновлении: prmd, rpms, smp, bi
                prefix = GIT.SelectGITModule(Task.LogFile);
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                return;
            }

            WinReleaseDeployment WinReleaseDeployment = new WinReleaseDeployment();
            WinReleaseDeployment.mainWindow = this;
            WinReleaseDeployment.ProjectDeploymentMS = Utilities.GITProjects.GetProjectDeployment("MS SQL", prefix);
            WinReleaseDeployment.ProjectDeploymentPG = Utilities.GITProjects.GetProjectDeployment("PG SQL", prefix);

            WinReleaseDeployment.cbGITProject.Items.Clear();
            WinReleaseDeployment.cbGITProject.Items.Add("");
            WinReleaseDeployment.cbGITProject.Items.Add(WinReleaseDeployment.ProjectDeploymentMS);
            WinReleaseDeployment.cbGITProject.Items.Add(WinReleaseDeployment.ProjectDeploymentPG);

            WinReleaseDeployment.Show();
            WinReleaseDeployment.dgJSONFilesRefresh();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Открыть окно "Формирование Cron релиза"</summary>
        private void btBuildReleaseCron_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                tbTaskNumber.Focus();
                return;
            }

            // Определим, для какого префикса версии (сервиса) собираем версию с действиями при обновлении: prmd, rpms, smp, bi
            if (string.IsNullOrWhiteSpace(Task.ReleaseVersion))
            {
                App.AddLog("Сначала на форме 'Собрать релиз' заполните номер версии", null, App.ShowMessageMode.SHOW, true, null);
                return;
            }

            string prefix = Task.ReleaseVersion.Split(new char[] { '.' })[0];

            if (
                string.IsNullOrWhiteSpace(prefix) ||
                (prefix != "prmd" && prefix != "rpms" && prefix != "smp" && prefix != "bi")
            )
            {
                // Спросим, для какого префикса версии (сервиса) собираем версию с действиями при обновлении: prmd, rpms, smp, bi
                prefix = GIT.SelectGITModule(Task.LogFile);
            }

            if (string.IsNullOrWhiteSpace(prefix))
            {
                return;
            }

            WinReleaseCron WinReleaseCron = new WinReleaseCron();
            WinReleaseCron.mainWindow = this;
            WinReleaseCron.ProjectCronMS = Utilities.GITProjects.GetProjectCron("MS SQL", prefix);
            WinReleaseCron.ProjectCronPG = Utilities.GITProjects.GetProjectCron("PG SQL", prefix);

            WinReleaseCron.cbGITProject.Items.Clear();
            WinReleaseCron.cbGITProject.Items.Add("");
            WinReleaseCron.cbGITProject.Items.Add(WinReleaseCron.ProjectCronMS);
            WinReleaseCron.cbGITProject.Items.Add(WinReleaseCron.ProjectCronPG);

            WinReleaseCron.Show();
            WinReleaseCron.dgJSONFilesRefresh();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Открыть папку задачи</summary>
        private void btOpenTaskPath_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Task.TaskPath)) //System.Diagnostics.Process.Start(Task.TaskPath);
            {
                Utilities.External.OpenDirectory(Task.TaskPath);
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Открыть лог-файл задачи</summary>
        private void btOpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            if (cbLogFile.SelectedIndex != -1)
            {
                Utilities.External.OpenExternalFile(cbLogFile.Text);
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Открыть в браузере страницу задачи</summary>
        private void btGoUrl_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Task.TaskUrl)) System.Diagnostics.Process.Start(Task.TaskUrl);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка добавления нового подключения</summary>
        private void btAddMainConnect_Click(object sender, RoutedEventArgs e)
        {
            var connect = Utilities.Controls.OpenConnectFromComboBox(cbMainConnect, true);

            if (connect.ConnType != Utilities.ConnType.None)
            {
                Utilities.Controls.RefreshConnectItems(cbMainConnect, connect.DBConnectionName, null, null);
            }

            connect.CloseConnect();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Смена подключения</summary>
        private void cbMainConnect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainConnect != null)
            {
                // закроем предыдущий коннект
                MainConnect.CloseConnect();
            }

            MainConnect = Utilities.Controls.SetConnectFromComboBox(cbMainConnect);

            if ((MainConnect != null) && (!string.IsNullOrWhiteSpace(MainConnect.DBConnectionName))) //-V3063
            {
                APPinfo.LastDBConnectionName = MainConnect.DBConnectionName;
            }

            tbStatusLeft.Value = MainWindow.AppStatus;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка "Сохранить настройки"</summary>
        private void btSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            SaveAPPinfo();
            LogAPPinfo();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка "Разделить файл на части"</summary>
        private void btSplitFile_Click(object sender, RoutedEventArgs e)
        {
            FormSplitFile dlg1 = new FormSplitFile();

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.Cursor = Cursors.Wait;

                FileStream fs_source = null;
                FileStream fs_dest = null;
                StreamReader file_source_text = null;
                StreamWriter file_dest_text = null;
                Encoding encoding = new UTF8Encoding(false);

                if (dlg1.rbUTF8.Checked)
                {
                    encoding = new UTF8Encoding(false);
                }

                if (dlg1.rbUTF8BOM.Checked)
                {
                    encoding = new UTF8Encoding(true);
                }

                if (dlg1.rbANSI.Checked)
                {
                    encoding = Encoding.GetEncoding("windows-1251");
                }

                if (dlg1.rbUTF16LE.Checked)
                {
                    encoding = new UnicodeEncoding();
                }

                long size = 0; // текущий размер текущей части в байтах
                const int size_buffer = 4096; // размер буфера
                char[] buffer_char = new char[size_buffer]; // буфер в символах
                byte[] buffer_byte = new byte[size_buffer]; // буфер в байтах
                bool isEOF = true;

                try
                {
                    fs_source = new FileStream(dlg1.Filename, FileMode.Open, FileAccess.Read);

                    int part_num = 1;
                    string part_name = Path.Combine(Path.GetDirectoryName(dlg1.Filename), Path.GetFileNameWithoutExtension(dlg1.Filename) + "_part_" + part_num.ToString() + Path.GetExtension(dlg1.Filename));
                    fs_dest = new FileStream(part_name, FileMode.Create, FileAccess.Write);

                    switch (dlg1.SplitType)
                    {
                        case Utilities.SplitType.BYTE:
                            isEOF = false;
                            break;
                        case Utilities.SplitType.CHAR:
                        case Utilities.SplitType.LINE:
                        case Utilities.SplitType.KEYWORDS:
                        default:
                            file_source_text = new StreamReader(fs_source, encoding);
                            isEOF = file_source_text.EndOfStream;
                            file_dest_text = new StreamWriter(fs_dest, encoding);
                            break;
                    }


                    while (!isEOF)
                    {
                        switch (dlg1.SplitType)
                        {
                            case Utilities.SplitType.BYTE:
                                {
                                    // делим точно по размеру
                                    int readed = fs_source.Read(buffer_byte, 0, size_buffer); // прочитано байт

                                    if (size + readed > dlg1.SizePart)
                                    {
                                        // превышаем максимальный размер части - дописываем в текущий файл
                                        int write_bytes = (int)(dlg1.SizePart - size); // сколько дописать байтов

                                        fs_dest.Write(buffer_byte, 0, write_bytes);
                                        fs_dest.Close();
                                        fs_dest.Dispose();

                                        //  создаем следующий файл
                                        part_num++;
                                        part_name = Path.Combine(Path.GetDirectoryName(dlg1.Filename), Path.GetFileNameWithoutExtension(dlg1.Filename) + "_part_" + part_num.ToString() + Path.GetExtension(dlg1.Filename));
                                        fs_dest = new FileStream(part_name, FileMode.Create, FileAccess.Write); //-V3114
                                        fs_dest.Write(buffer_byte, write_bytes, readed - write_bytes); // дописываем остаток буфера в новый файл
                                        size = readed - write_bytes;
                                    }
                                    else
                                    {
                                        // еще не превышаем максимальный размер части - пишем в текущий файл
                                        fs_dest.Write(buffer_byte, 0, readed);
                                        size += readed;
                                    }
                                    isEOF = readed == 0;
                                }
                                break;
                            case Utilities.SplitType.CHAR:
                                {
                                    // делим точно по размеру, но с учетом размера символа
                                    int readed = file_source_text.Read(buffer_char, 0, size_buffer); // прочитано символов //-V3080
                                    int readed_bytes = encoding.GetByteCount(buffer_char); // прочитано байт

                                    if (size + readed_bytes > dlg1.SizePart)
                                    {
                                        // превышаем максимальный размер части - дописываем в текущий файл
                                        int write_bytes = (int)(dlg1.SizePart - size); // сколько дописать байтов
                                        int cnt_bytes = 0;
                                        int write_char = 0;
                                        for (int i = 0; i < buffer_char.Length; i++)
                                        {
                                            cnt_bytes += encoding.GetByteCount(buffer_char, i, 1);
                                            if (cnt_bytes > write_bytes)
                                            {
                                                write_char = i; // сколько дописать символов
                                                break;
                                            }
                                        }
                                        file_dest_text.Write(buffer_char, 0, write_char);
                                        file_dest_text.Close();
                                        file_dest_text.Dispose();

                                        //  создаем следующий файл
                                        part_num++;
                                        part_name = Path.Combine(Path.GetDirectoryName(dlg1.Filename), Path.GetFileNameWithoutExtension(dlg1.Filename) + "_part_" + part_num.ToString() + Path.GetExtension(dlg1.Filename));
                                        fs_dest = new FileStream(part_name, FileMode.Create, FileAccess.Write);
                                        file_dest_text = new StreamWriter(fs_dest, encoding);
                                        file_dest_text.Write(buffer_char, write_char, readed - write_char); // дописываем остаток буфера в новый файл
                                        size = readed_bytes - write_bytes;
                                    }
                                    else
                                    {
                                        // еще не превышаем максимальный размер части - пишем в текущий файл
                                        file_dest_text.Write(buffer_char, 0, readed);
                                        size += readed_bytes;
                                    }
                                    isEOF = file_source_text.EndOfStream;
                                }
                                break;
                            case Utilities.SplitType.KEYWORDS:
                            case Utilities.SplitType.LINE:
                            default:
                                {
                                    // делим точно по строкам
                                    string line = file_source_text.ReadLine(); // прочитана строка 
                                    int readed_bytes = encoding.GetByteCount(line); // прочитано байт

                                    if (size + readed_bytes > dlg1.SizePart)
                                    {
                                        // превышаем максимальный размер части - дописываем считанную строку в текущий файл
                                        file_dest_text.WriteLine(line);

                                        if (dlg1.SplitType == Utilities.SplitType.KEYWORDS)
                                        {
                                            // разбиение с учетом ключевых слов (экспериментальный режим)

                                            if ((!file_source_text.EndOfStream) && line.ToUpper().StartsWith("IF NOT EXISTS"))
                                            {
                                                line = file_source_text.ReadLine(); // предположительно INSERT INTO
                                                file_dest_text.WriteLine(line);
                                            }
                                            if ((!file_source_text.EndOfStream) && line.ToUpper().StartsWith("INSERT INTO "))
                                            {
                                                line = file_source_text.ReadLine(); // предположительно VALUES
                                                file_dest_text.WriteLine(line);
                                            }

                                            if ((!file_source_text.EndOfStream) && line.ToUpper().StartsWith("UPDATE "))
                                            {
                                                line = file_source_text.ReadLine(); // предположительно SET
                                                file_dest_text.WriteLine(line);
                                            }
                                            if ((!file_source_text.EndOfStream) && line.ToUpper().StartsWith("SET "))
                                            {
                                                line = file_source_text.ReadLine(); // предположительно WHERE
                                                file_dest_text.WriteLine(line);
                                            }
                                        }

                                        file_dest_text.Close();
                                        file_dest_text.Dispose();

                                        //  создаем следующий файл
                                        if (!file_source_text.EndOfStream)
                                        {
                                            part_num++;
                                            part_name = Path.Combine(Path.GetDirectoryName(dlg1.Filename), Path.GetFileNameWithoutExtension(dlg1.Filename) + "_part_" + part_num.ToString() + Path.GetExtension(dlg1.Filename));
                                            fs_dest = new FileStream(part_name, FileMode.Create, FileAccess.Write);
                                            file_dest_text = new StreamWriter(fs_dest, encoding);
                                            size = 0;
                                        }
                                    }
                                    else
                                    {
                                        // еще не превышаем максимальный размер части - пишем в текущий файл
                                        file_dest_text.WriteLine(line);
                                        size += readed_bytes;
                                    }
                                    isEOF = file_source_text.EndOfStream;
                                }
                                break;
                        }
                    }

                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
                finally
                {
                    if (file_dest_text != null)
                    {
                        file_dest_text.Close();
                        file_dest_text.Dispose();
                    }
                    if (file_source_text != null)
                    {
                        file_source_text.Close();
                        file_source_text.Dispose();
                    }

                    if (fs_source != null)
                    {
                        fs_source.Close();
                        fs_source.Dispose();
                    }
                    if (fs_dest != null)
                    {
                        fs_dest.Close();
                        fs_dest.Dispose();
                    }
                }

                string path = Path.GetDirectoryName(dlg1.Filename);
                if (Directory.Exists(path)) System.Diagnostics.Process.Start(path);

                this.Cursor = Cursors.Arrow;
            }
            dlg1.Dispose();
        }

        // -------------------------------------------------------------------------------------------------------
        private void btHelp_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://confluence.rtmis.ru/display/RTMISDEV/SQLGen");
        }

        // -------------------------------------------------------------------------------------------------------
        private void btChangelog_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://git.promedweb.ru/rtmisdb/sqlgen/-/blob/master/CHANGELOG");
        }

        // -------------------------------------------------------------------------------------------------------
        private void btFeedback_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://jira.rtmis.ru/browse/PROMEDWEB-89788");
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка "Маркеры"</summary>
        private void btFreeDocMarker_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            WinMarker WinMarker = new WinMarker();

            string project = "";

            if (MainConnect != null)
            {
                Utilities.Controls.RefreshConnectItems(WinMarker.cbConnectSQL, MainConnect.DBConnectionName, null, null, true);
                project = MainConnect.GITProject;
            }

            Utilities.Controls.RefreshRegionItems(WinMarker.cbRegion, project);

            WinMarker.isStart = false;
            WinMarker.cbConnectSQLChanged();
            WinMarker.btRefresh_Click(null, null);

            WinMarker.Show();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// при смене проекта GIT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGITProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbGITProject.SelectedIndex >= 1)
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();
                string err = "";
                string branch = GIT.GitCurrentBranch(project, out err, null);

                lbGITBranch.Content = "Текущая ветка GIT: " + branch;

                if (Task.SendYMLFiles.ContainsKey(project))
                {
                    // возьмем имя файла из последней отправки
                    tbYMLFile.Text = Task.SendYMLFiles[project];
                }
            }
            else
            {
                lbGITBranch.Content = "Текущая ветка GIT:";
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Двойной клик мыши по скрипту
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgScriptsInTask_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgScriptsInTask.SelectedIndex >= 0)
            {
                GITScript script = dgScriptsInTask.SelectedItem as GITScript;

                App.AddLog("Открытие внешнего файла " + script.FullSourceScriptname_TO_GIT, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                Utilities.External.OpenExternalFile(script.FullSourceScriptname_TO_GIT);
            }

        }

        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Кнопка "Список задач не вошедших в релиз"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btReportTaskNotRelease_Click(object sender, RoutedEventArgs e)
        {
            // выбрать проекты
            FormCheckedListBox dlg1 = new FormCheckedListBox();
            dlg1.clbList.Items.Clear();

            dlg1.clbList.Items.Add("liquibase_project_new", false);
            dlg1.clbList.Items.Add("msdbupdate_new", false);

            foreach (var item in cbGITProject.Items.OfType<string>()
                .Where(x => x != "ВСЕ")
                .Where(x => IsGITProject(x)) // только старые проекты
                .OrderBy(x => x)
                )
            {
                if (!dlg1.clbList.Items.Contains(item))
                {
                    dlg1.clbList.Items.Add(item);
                }
            }

            List<string> Projects = new List<string>();

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    Projects.Add(itemChecked.ToString());
                    string dev_project = Utilities.GIT.GetDEVProject(itemChecked.ToString());
                    if (!string.IsNullOrWhiteSpace(dev_project))
                    {
                        Projects.Add(dev_project);
                    }
                }

                // делаем git pull
                GitPull(Projects.ToArray(), "dev", true, true);

                WinInfo WinInfo = new WinInfo();
                WinInfo.Title = "Список yml-файлов, которых нет в версиях";

                //перебираем проекты
                foreach (var project in Projects
                                    .Where(x => IsGITProject(x)) // только старые проекты
                )
                {
                    string GITProjectFolder = Utilities.GIT.GetFolderByProject(project);
                    string git_taskdir = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, "task");
                    string versiondir = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, "version");

                    // соотвествующий dev-проект
                    string dev_project = Utilities.GIT.GetDEVProject(project);
                    string DEVProjectFolder = Utilities.GIT.GetFolderByProject(dev_project);
                    string dev_taskdir = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, "task");

                    // список yml, которых нет в версиях
                    List<string> ListYML = new List<string>();

                    // список всех yml в версиях
                    List<string> task_in_version = new List<string>();

                    // считаем все строки из yml-файлов в version
                    foreach (var file in Utilities.ListFilesInDir(versiondir, false, true, false))
                    {
                        if (file.EndsWith(".yml"))
                        {
                            task_in_version.AddRange(
                            File.ReadAllLines(Path.Combine(versiondir, file))
                                .ToList()
                                .Where(x => x.Contains("include"))
                                );
                        }
                    }

                    // перебираем yml-Файлы в task и проверяем, есть ли на них ссылки в version
                    foreach (var file in Utilities.ListFilesInDir(git_taskdir, false, true, false))
                    {
                        if (
                                file.EndsWith(".yml") &&
                                (!file.StartsWith("BILLINGDEV")) &&
                                (!file.StartsWith("CRQ")) &&
                                (!file.StartsWith("INC")) &&
                                (!file.StartsWith("OPS")) &&
                                (!file.StartsWith("PROMEDDEVOPS")) &&
                                (!file.StartsWith("PROMEDREP")) &&
                                (!file.StartsWith("PROMEDSKUF"))
                        )
                        {
                            string taskymlfile = Path.GetFileName(file);

                            //ищем yml-файл в папке version
                            if (!task_in_version.Exists(x => x.Contains(taskymlfile)))
                            {
                                // в версиях нет

                                if (
                                    // в содержимом есть скрипты по изменению таблицы, вьюхи, хранимки
                                    (File.ReadAllLines(Path.Combine(git_taskdir, taskymlfile))
                                        .ToList()
                                        .Where(x =>
                                            x.Contains("include")
                                        )
                                        .Where(x =>
                                            x.Contains("/TABLE/") ||
                                            x.Contains("/VIEW/") ||
                                            x.Contains("/FUNCTION/") ||
                                            x.Contains("/PROCEDURE/") ||
                                            x.Contains("/TRIGGER/")
                                        )
                                        .Where(x =>
                                            !x.Contains("../rpt") ||
                                            x.Contains("../rpt/")
                                        )
                                        .Count() > 0
                                    ) &&
                                    // yml нет в dev-проекте
                                    (!File.Exists(Path.Combine(dev_taskdir, taskymlfile)))
                                )
                                {
                                    ListYML.Add(taskymlfile);
                                }
                            }

                        }
                    }

                    // добавляем информацию в вывод
                    if (ListYML.Count > 0)
                    {
                        WinInfo.tbInfo.AppendText(
                            "--------------------------------------------------------------------" + Environment.NewLine +
                            project + Environment.NewLine +
                            "--------------------------------------------------------------------" + Environment.NewLine +
                            string.Join(Environment.NewLine, ListYML.ToArray()) + Environment.NewLine +
                            Environment.NewLine
                        );
                    }
                }

                // выводим информацию
                WinInfo.Show();
            }

            dlg1.Dispose();
        }
        */

        
        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Кнопка "Перенос из GIT в DEV"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCopyYMLtoDEV_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            if (System.Windows.Forms.MessageBox.Show("Перенести " + tbYMLFile.Text + Environment.NewLine +
                                $"из \"старого\" проекта в \"новый\" проект разработки ?",
                                "ВНИМАНИЕ",
                                System.Windows.Forms.MessageBoxButtons.YesNo
                            ) == System.Windows.Forms.DialogResult.No
                        )
            {
                return;
            }

            // выбрать проекты
            FormCheckedListBox dlg1 = new FormCheckedListBox();
            dlg1.clbList.Items.Clear();

            foreach (var item in cbGITProject.Items.OfType<string>()
                .Where(x => x != "ВСЕ")
                .Where(x => Utilities.GITProjects.IsGITProject(x)) // только старые проекты
                .OrderBy(x =>
                {
                    string ord = "999";
                    if (x == "liquibase_project_new") ord = "001";
                    else if (x == "msdbupdate_new") ord = "002";
                    else if (x == "promedlistest2") ord = "003";
                    return ord + x;
                }
                ))
            {
                if (!dlg1.clbList.Items.Contains(item))
                {
                    // чекаем те проекты, где есть yml
                    string project = item;
                    string GITProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder);
                    string git_taskdir = Path.Combine(folder, "task");
                    string git_taskfile = Path.Combine(git_taskdir, tbYMLFile.Text);
                    bool isChecked = File.Exists(git_taskfile);

                    if (
                        (!string.IsNullOrWhiteSpace(GITProjectFolder)) &&
                        Directory.Exists(folder)
                        )
                    {
                        dlg1.clbList.Items.Add(item, isChecked);
                    }
                }
            }

            List<string> Projects = new List<string>();

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    string project = itemChecked.ToString();
                    Projects.Add(project);
                    string dev_project = Utilities.GITProjects.GetDEVProject(project);
                    if (!string.IsNullOrWhiteSpace(dev_project))
                    {
                        Projects.Add(dev_project);
                    }

                }

                // делаем git pull + git-refresh.sh
                GIT.GitPull(Projects.ToArray(), "dev", true, true, false, MainWindow.Task.LogFile);
            }

            string Info = "";

            //перебираем проекты
            foreach (var project in Projects
                                .Where(x => Utilities.GITProjects.IsGITProject(x)) // только старые проекты
            )
            {
                string GITProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
                string git_taskdir = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, "task");
                string git_taskfile = Path.Combine(git_taskdir, tbYMLFile.Text);
                string DBType = Utilities.GITProjects.GetDBTypeByProject(project);

                if (File.Exists(git_taskfile))
                {
                    //yml-файл есть в GIT-проекте

                    // соответствующий dev-проект
                    string dev_project = Utilities.GITProjects.GetDEVProject(project);
                    string DEVProjectFolder = Utilities.GITProjects.GetFolderByProject(dev_project);
                    string dev_taskdir = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, "task");
                    string dev_taskfile = Path.Combine(dev_taskdir, tbYMLFile.Text);

                    if (!string.IsNullOrWhiteSpace(dev_project))
                    {
                        // проверяем наличие yml в "новом" проекте, может уже скопировали
                        if (File.Exists(dev_taskfile))
                        {
                            App.AddLog($"Файл {dev_taskfile} уже существует, копирование пропущено!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                            Info += $"Файл {dev_taskfile} уже существует, копирование пропущено!" + Environment.NewLine;
                        }
                        else
                        {
                            //yml-файл нет в DEV-проекте

                            // переключиться на ветку задачи в dev-проекте
                            GIT.GitNewBranch(new string[] { dev_project }, Task.TaskNumber, "master", MainWindow.Task.LogFile);

                            string yml = "";

                            //читаем содержимое yml-файла в GIT-проекте
                            var list = (File.ReadAllLines(git_taskfile)
                                .ToList()
                                .Where(x =>
                                    x.Contains("include")
                                )
                            );

                            yml = "";

                            // перебираем строки yml-файла
                            foreach (var item in list)
                            {
                                string line = "";

                                var arr = item.Split('/');
                                if (arr.Length >= 5)
                                {
                                    // таблицы, вьюхи, хранимки

                                    // разбираем строку
                                    string schema = arr[1];
                                    string type = arr[2];
                                    string folder = arr[3];
                                    string name = arr[4].Split('\"')[0];
                                    string ext = Path.GetExtension(name);
                                    string file_from = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, schema, type, folder, name);

                                    if (File.Exists(file_from))
                                    {
                                        // Исходный файл существует 

                                        // собираем новое имя
                                        string newname = folder.ToLower();

                                        // корректируем имя и проверям наличие 
                                        if (newname.EndsWith("_de")) newname = newname + "l";
                                        if (newname.EndsWith("_in")) newname = newname + "s";
                                        if (newname.EndsWith("_a")) newname = newname + "ll";

                                        string checkname = newname + ext;
                                        string checkfile = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, schema, type, checkname);

                                        if (!File.Exists(checkfile))
                                        {
                                            checkname = newname + "l" + ext;
                                            checkfile = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, schema, type, checkname);

                                            if (!File.Exists(checkfile))
                                            {
                                                checkname = newname + "ll" + ext;
                                                checkfile = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, schema, type, checkname);

                                                if (!File.Exists(checkfile))
                                                {
                                                    checkname = newname + "s" + ext;
                                                    checkfile = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, schema, type, checkname);

                                                    if (!File.Exists(checkfile))
                                                    {
                                                        checkname = newname + "ss" + ext;
                                                        checkfile = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, schema, type, checkname);

                                                        if (!File.Exists(checkfile))
                                                        {
                                                            checkname = newname + "ls" + ext;
                                                            checkfile = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, schema, type, checkname);

                                                            if (!File.Exists(checkfile))
                                                            {
                                                                checkname = newname + "q" + ext;
                                                                checkfile = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, schema, type, checkname);

                                                                if (!File.Exists(checkfile))
                                                                {
                                                                    checkname = newname + ext;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                        //if (checkname == "personprivilegereq.sql")
                                        //{
                                        //    int test = 0;
                                        //}


                                        newname = checkname;
                                        Directory.CreateDirectory(Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, schema));
                                        Directory.CreateDirectory(Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, schema, type));
                                        string file_to = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, schema, type, newname);

                                        if (type == "TABLE")
                                        {
                                            // это таблица = дописываем в конец

                                            // делаем копию sql-файла во временный файл, меняем в нем chageset
                                            string tmp_file = Path.GetTempFileName();
                                            App.tempFiles.Add(tmp_file);

                                            YML.CopyFileSetChangeset(file_from, type, DBType, tmp_file);

                                            // сначала поищем и выделим кусок с нужным changeset и сравним с копируемым файлом
                                            // =-1 - найден, не совпадает хеш
                                            // =0 - найден, совпадает хеш
                                            // =1 - НЕ найден 
                                            int isFound = SQLChangeset.FindChangeset(tmp_file, file_to, Task.TaskNumber, out string changeset_text, MainWindow.Task.LogFile);

                                            if (isFound == 0)
                                            {
                                                // changeset найден, совпадает хеш

                                                // собираем новую строку
                                                if (MainWindow.APPinfo.relativeToChangelogFile == "true")
                                                {
                                                    line = "- include: { file: \"../" + $"{schema}/{type}/{newname}" + "\", relativeToChangelogFile: \"true\" }\n";
                                                }
                                                else
                                                {
                                                    line = "- include: { file: \"" + $"{schema}/{type}/{newname}" + "\", relativeToChangelogFile: \"false\" }\n";
                                                }

                                                Info += $"Файл {file_from} пропущен, т.к. файл {file_to} уже существует и в нем есть идентичный changeset" + Environment.NewLine;
                                            }
                                            else
                                            {
                                                // changeset не найден, допишем в конец
                                                try
                                                {
                                                    string text = File.ReadAllText(tmp_file);
                                                    if (File.Exists(file_to))
                                                    {
                                                        text = SQLChangeset.RemoveLiquibaseTag(text);
                                                    }
                                                    File.AppendAllText(file_to, "\n" + text);
                                                    File.SetLastWriteTime(file_to, DateTime.Now);
                                                    File.SetCreationTime(file_to, DateTime.Now);

                                                    // собираем новую строку
                                                    if (MainWindow.APPinfo.relativeToChangelogFile == "true")
                                                    {
                                                        line = "- include: { file: \"../" + $"{schema}/{type}/{newname}" + "\", relativeToChangelogFile: \"true\" }\n";
                                                    }
                                                    else
                                                    {
                                                        line = "- include: { file: \"" + $"{schema}/{type}/{newname}" + "\", relativeToChangelogFile: \"false\" }\n";
                                                    }

                                                    Info += $"Файл {file_from} дописан в конец файла {file_to}" + Environment.NewLine;
                                                }
                                                catch (Exception ex)
                                                {
                                                    Info += App.AddLog($"Ошибка при дописывании файла {file_from} в конец файла {file_to}", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile).showMessage + Environment.NewLine;

                                                    line = "#Ошибка при дописывании в конец: " + item + "\n";
                                                }
                                            }

                                            // удаляем временный файл
                                            if (!tmp_file.StartsWith(MainWindow.APPinfo.GITFolder))
                                            {
                                                File.Delete(tmp_file);
                                            }
                                        }
                                        else if (
                                            (type == "PROCEDURE") ||
                                            (type == "FUNCTION") ||
                                            (type == "TRIGGER") ||
                                            (type == "VIEW")
                                        )
                                        {
                                            // это вьюхи, хранимки - перезаписываем файл

                                            // делаем копию sql-файла во временный файл, меняем в нем chageset
                                            string tmp_file = Path.GetTempFileName();
                                            App.tempFiles.Add(tmp_file);

                                            YML.CopyFileSetChangeset(file_from, type, DBType, tmp_file);

                                            try
                                            {
                                                File.Copy(tmp_file, file_to, true);
                                                File.SetLastWriteTime(file_to, DateTime.Now);
                                                File.SetCreationTime(file_to, DateTime.Now);

                                                // собираем новую строку
                                                if (MainWindow.APPinfo.relativeToChangelogFile == "true")
                                                {
                                                    line = "- include: { file: \"../" + $"{schema}/{type}/{newname}" + "\", relativeToChangelogFile: \"true\" }\n";
                                                }
                                                else
                                                {
                                                    line = "- include: { file: \"" + $"{schema}/{type}/{newname}" + "\", relativeToChangelogFile: \"false\" }\n";
                                                }

                                                Info += $"Файл {file_from} скопирован в файл {file_to}" + Environment.NewLine;
                                            }
                                            catch (Exception ex)
                                            {
                                                Info += App.AddLog($"Ошибка при копировании файла {file_from} в файл {file_to}", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile) + Environment.NewLine;

                                                line = "#Ошибка при копировании: " + item + "\n";
                                            }

                                            // удаляем временный файл
                                            if (!tmp_file.StartsWith(MainWindow.APPinfo.GITFolder))
                                            {
                                                File.Delete(tmp_file);
                                            }

                                        }
                                        else
                                        {
                                            line = "#Строка НЕ распознана: " + item + "\n";

                                            App.AddLog(line, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);

                                            Info += "Строка НЕ распознана: " + item + Environment.NewLine;
                                        }
                                    }
                                    else
                                    {
                                        line = "#Файл НЕ существует: " + item + "\n";

                                        App.AddLog(line, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);

                                        Info += $"Файл {file_from} не существует, нечего копировать!" + Environment.NewLine;
                                    }
                                }
                                else if (
                                        (arr.Length == 4) &&
                                        (
                                            item.Contains("/data_new/") ||
                                            item.Contains("/data/")
                                        )
                                    )
                                {
                                    // это данные

                                    // разбираем строку
                                    string schema = arr[1];
                                    string folder = arr[2];
                                    string name = arr[3].Split('\"')[0];
                                    string file_from = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, schema, folder, name);

                                    if (File.Exists(file_from))
                                    {
                                        // собираем новое имя
                                        string newschema = schema;
                                        if (newschema == "data_new") newschema = "data";

                                        string newfolder = folder.ToLower();

                                        string newname = name.ToLower();
                                        Directory.CreateDirectory(Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, newschema));
                                        Directory.CreateDirectory(Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, newschema, newfolder));
                                        string file_to = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, newschema, newfolder, newname);

                                        if (File.Exists(file_to))
                                        {
                                            // файл найден, пропускаем

                                            // собираем новую строку
                                            if (MainWindow.APPinfo.relativeToChangelogFile == "true")
                                            {
                                                line = "- include: { file: \"../" + $"{newschema}/{newfolder}/{newname}" + "\", relativeToChangelogFile: \"true\" }\n";
                                            }
                                            else
                                            {
                                                line = "- include: { file: \"" + $"{newschema}/{newfolder}/{newname}" + "\", relativeToChangelogFile: \"false\" }\n";
                                            }

                                            Info += $"Файл {file_to} уже существует, копирование пропущено!" + Environment.NewLine;

                                            App.AddLog($"Файл {file_to} уже существует, копирование пропущено!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                                        }
                                        else
                                        {
                                            // копируем файл
                                            try
                                            {
                                                File.Copy(file_from, file_to, true);
                                                File.SetLastWriteTime(file_to, DateTime.Now);
                                                File.SetCreationTime(file_to, DateTime.Now);

                                                // собираем новую строку
                                                if (MainWindow.APPinfo.relativeToChangelogFile == "true")
                                                {
                                                    line = "- include: { file: \"../" + $"{newschema}/{newfolder}/{newname}" + "\", relativeToChangelogFile: \"true\" }\n";
                                                }
                                                else
                                                {
                                                    line = "- include: { file: \"" + $"{newschema}/{newfolder}/{newname}" + "\", relativeToChangelogFile: \"false\" }\n";
                                                }

                                                Info += $"Файл {file_from} скопирован в файл {file_to}" + Environment.NewLine;
                                            }
                                            catch (Exception ex)
                                            {
                                                Info += App.AddLog($"Ошибка при копировании файла {file_from} в файл {file_to}", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile) + Environment.NewLine;

                                                line = "#Ошибка при копировании: " + item + "\n";
                                            }
                                        }
                                    }
                                    else
                                    {
                                        line = "#Файл НЕ существует: " + item + "\n";

                                        App.AddLog(line, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);

                                        Info += $"Файл {file_from} не существует, нечего копировать!" + Environment.NewLine;
                                    }
                                }
                                else
                                {
                                    // это неизвестный тип скрипта
                                    line = "#Строка НЕ распознана: " + item + "\n";

                                    App.AddLog(line, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);

                                    Info += "Строка НЕ распознана: " + item + Environment.NewLine;
                                }

                                yml += line;
                            }

                            if (!string.IsNullOrWhiteSpace(yml))
                            {
                                // есть что копировать, собираем yml

                                yml = "databaseChangeLog:\n" + yml;
                                Utilities.Files.WriteScript(dev_taskfile, null, yml, false, out string err, FileMode.Create);

                                // Отправляем в GIT
                                GIT.GitAdd(new string[] { dev_project }, Task.TaskNumber, false, true, MainWindow.Task.LogFile);

                                App.AddLog($"{tbYMLFile.Text} скопирован из {project} в {dev_project}", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                            }
                        }
                    }
                    else
                    {
                        Info += $"Для проекта {project} не существует проекта разработки!" + Environment.NewLine;

                        App.AddLog($"Для проекта {project} не существует проекта разработки!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                }
                else
                {
                    Info += $"Файл {git_taskfile} не существует, копировать нечего!" + Environment.NewLine;

                    App.AddLog($"Файл {git_taskfile} не существует, копировать нечего!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            WinInfo WinInfo = new WinInfo(Task.LogFile);
            WinInfo.Title = "Лог копирования";
            WinInfo.tbInfo.Text = Info;
            WinInfo.Show();
        }
        */

        /*
        // -------------------------------------------------------------------------------------------------------
        private void ReleaseTaskNotInBranch(string branch)
        {
            // выбрать проекты
            FormCheckedListBox dlg1 = new FormCheckedListBox();
            dlg1.clbList.Items.Clear();

            dlg1.clbList.Items.Add("liquibase_project_new", false);
            dlg1.clbList.Items.Add("msdbupdate_new", false);

            foreach (var item in cbGITProject.Items.OfType<string>()
                .Where(x => x != "ВСЕ")
                .Where(x => IsGITProject(x)) // только старые проекты
                .OrderBy(x => x)
                )
            {
                if (!dlg1.clbList.Items.Contains(item))
                {
                    dlg1.clbList.Items.Add(item);
                }
            }

            List<string> Projects = new List<string>();

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    Projects.Add(itemChecked.ToString());
                    string dev_project = Utilities.GIT.GetDEVProject(itemChecked.ToString());
                    if (!string.IsNullOrWhiteSpace(dev_project))
                    {
                        Projects.Add(dev_project);
                    }
                }

                // делаем git pull, новые проекты переключаем в ветку branch
                GitPull(Projects.ToArray(), branch, true, true);

                WinInfo WinInfo = new WinInfo();
                WinInfo.Title = $"Список yml-файлов, которые добавлены в версии начиная с 8.16.2.5, но которых нет в ветке {branch}";

                //перебираем проекты
                foreach (var project in Projects
                                    .Where(x => IsGITProject(x)) // только старые проекты
                )
                {
                    string GITProjectFolder = Utilities.GIT.GetFolderByProject(project);
                    string git_taskdir = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, "task");
                    string versiondir = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, "version");

                    // соотвествующий dev-проект
                    string dev_project = Utilities.GIT.GetDEVProject(project);
                    string DEVProjectFolder = Utilities.GIT.GetFolderByProject(dev_project);
                    string dev_taskdir = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, "task");

                    WinInfo.tbInfo.AppendText(
                        "--------------------------------------------------------------------" + Environment.NewLine +
                        project + Environment.NewLine +
                        "--------------------------------------------------------------------" + Environment.NewLine
                        );

                    // список yml в версиях
                    Dictionary<string, YMLStruct> versions = new Dictionary<string, YMLStruct>();

                    // считаем все строки из yml-файлов в version
                    foreach (var file in Utilities.ListFilesInDir(versiondir, false, true, false))
                    {
                        if (
                            file.EndsWith(".yml") &&
                            (!file.Contains("_rpt")) &&
                            (!file.Contains("_ots"))
                        )
                        {
                            // читаем версию из файла
                            var yml = new YMLStruct(null);
                            yml.LoadYML(project, "version", file, false, null, false);

                            if (yml.NumVersionOrder >= 8016002005000)
                            {
                                // добавляем начиная с 8.16.2.5
                                versions.Add(file, yml);
                            }
                        }
                    }

                    // перебираем yml-файлы в version и ищем их в новых проектах
                    foreach (var yml in versions)
                    {
                        bool isFirst = true;

                        foreach (var task in yml.Value.Lines.Where(x => x.type == YMLLineType.TASK))
                        {
                            if (
                                File.Exists(Path.Combine(git_taskdir, task.file)) &&
                                // в содержимом есть скрипты по изменению таблицы, вьюхи, хранимки
                                (File.ReadAllLines(Path.Combine(git_taskdir, task.file))
                                    .ToList()
                                    .Where(x =>
                                        x.Contains("include")
                                    )
                                    .Where(x =>
                                        x.Contains("/TABLE/") ||
                                        x.Contains("/VIEW/") ||
                                        x.Contains("/FUNCTION/") ||
                                        x.Contains("/PROCEDURE/") ||
                                        x.Contains("/TRIGGER/")
                                    )
                                    .Where(x =>
                                        !x.Contains("../rpt") ||
                                        x.Contains("../rpt/")
                                    )
                                    .Count() > 0
                                ) &&
                                // yml нет в dev-проекте
                                (!File.Exists(Path.Combine(dev_taskdir, task.file)))
                            )
                            {
                                if (isFirst)
                                {
                                    WinInfo.tbInfo.AppendText(Environment.NewLine + yml.Key + Environment.NewLine);
                                    isFirst = false;
                                }
                                WinInfo.tbInfo.AppendText(task.file + Environment.NewLine);
                            }
                        }
                    }
                }

                // выводим информацию
                WinInfo.Show();
            }

            dlg1.Dispose();
        }
        */

        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// список задач, которые включены в версии (начиная с 8.16.2.5) но которых нет в новых проектах (в ветке dev)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btReportReleaseTaskNotInDev_Click(object sender, RoutedEventArgs e)
        {
            ReleaseTaskNotInBranch("dev");
        }
        */

        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// список задач, которые включены в версии (начиная с 8.16.2.5) но которых нет в новых проектах (в ветке master)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btReportReleaseTaskNotInMaster_Click(object sender, RoutedEventArgs e)
        {
            ReleaseTaskNotInBranch("master");
        }
        */

        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Задачи, которые есть в master, но нет вошли в релиз
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btReportDevTaskInMaster_Click(object sender, RoutedEventArgs e)
        {
            // выбрать проекты
            FormCheckedListBox dlg1 = new FormCheckedListBox();
            dlg1.clbList.Items.Clear();

            dlg1.clbList.Items.Add("dev_promed_pg", false);
            dlg1.clbList.Items.Add("dev_promed_ms", false);

            foreach (var item in cbGITProject.Items.OfType<string>()
                .Where(x => x != "ВСЕ")
                .Where(x => IsDEVProject(x)) // только новые проекты
                .OrderBy(x => x)
                )
            {
                if (!dlg1.clbList.Items.Contains(item))
                {
                    dlg1.clbList.Items.Add(item);
                }
            }

            List<string> Projects = new List<string>();

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    Projects.Add(itemChecked.ToString());
                    string dev_project = Utilities.GIT.GetDEVProject(itemChecked.ToString());
                    if (!string.IsNullOrWhiteSpace(dev_project))
                    {
                        Projects.Add(dev_project);
                    }
                }

                // делаем git pull, новые проекты переключаем в master
                GitPull(Projects.ToArray(), "master", true, true);

                WinInfo WinInfo = new WinInfo();
                WinInfo.Title = "Список yml-файлов, которые есть в master, но которых нет версиях";

                //перебираем проекты
                foreach (var dev_project in Projects
                                    .Where(x => IsDEVProject(x)) // только новые проекты
                )
                {
                    string DEVProjectFolder = Utilities.GIT.GetFolderByProject(dev_project);
                    string dev_taskdir = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, "task");
                    string dev_versiondir = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, "version");

                    // соотвествующий git-проект
                    string git_project = Utilities.GIT.GetGITProject(dev_project);
                    string GITProjectFolder = Utilities.GIT.GetFolderByProject(git_project);
                    string git_taskdir = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, "task");
                    string git_versiondir = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, "version");

                    WinInfo.tbInfo.AppendText(
                        "--------------------------------------------------------------------" + Environment.NewLine +
                        dev_project + Environment.NewLine +
                        "--------------------------------------------------------------------" + Environment.NewLine
                        );

                    // список yml в версиях
                    Dictionary<string, YMLStruct> versions = new Dictionary<string, YMLStruct>();

                    // считаем все строки из yml-файлов в version
                    foreach (var file in Utilities.ListFilesInDir(git_versiondir, false, true, false))
                    {
                        if (
                            file.EndsWith(".yml") &&
                            (!file.Contains("_rpt")) &&
                            (!file.Contains("_ots"))
                        )
                        {
                            // читаем версию из файла
                            //WinRelease.GetVersionFromFile(git_project, git_versiondir, file, out string Num, out string PrevNum, out bool isNoCumulative);
                            var yml = new YMLStruct(null);
                            yml.LoadYML(git_project, "version", file, false, null, false);
                            versions.Add(file, yml);
                        }
                    }

                    // перебираем yml-файлы в master и ищем их в версиях
                    foreach (var yml in Utilities.ListFilesInDir(dev_taskdir, false, true, false))
                    {
                        if (
                            yml.EndsWith(".yml") &&
                            (!yml.Contains("_rpt")) &&
                            (!yml.Contains("_ots"))
                        )
                        {
                            bool found = false;

                            foreach (var version in versions)
                            {
                                var task = version.Value.Lines
                                    .Where(x =>
                                        (x.type == YMLLineType.TASK) &&
                                        (x.file == yml)
                                    )
                                    .FirstOrDefault();

                                if (task != null)
                                {
                                    found = true;
                                }
                            }

                            if (!found)
                            {
                                // если не нашли
                                WinInfo.tbInfo.AppendText(yml + Environment.NewLine);
                            }
                        }
                    }
                }

                // выводим информацию
                WinInfo.Show();
            }

            dlg1.Dispose();
        }
        */

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Нажата кнопка Выполнить YML-файл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btExecuteYML_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            //сборка и проверка имени yml-файла 
            string YMLFile = tbYMLFile.Text.Trim();
            if ((YMLFile.Length < 4) || (YMLFile.Substring(YMLFile.Length - 4, 4).ToLower() != ".yml")) YMLFile += ".yml";
            tbYMLFile.Text = YMLFile;

            if (string.IsNullOrWhiteSpace(tbYMLFile.Text))
            {
                MessageBox.Show("Необходимо заполнить имя YML-файла");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbYMLFile.IsFocused) tbYMLFile.Focus();
                return;
            }

            // проверка подключения
            if (MainConnect == null)
            {
                MessageBox.Show("Необходимо выбрать подключение к БД");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!cbMainConnect.IsFocused) cbMainConnect.Focus();
                return;
            }

            //сначала спросим
            if (System.Windows.Forms.MessageBox.Show($"Выполнить sql-файлы из {tbYMLFile.Text}\nчерез подключение\n{MainConnect.DBConnectionName} ?",
                    "ВНИМАНИЕ",
                    System.Windows.Forms.MessageBoxButtons.YesNo
                ) == System.Windows.Forms.DialogResult.No
            )
            {
                return;
            }

            // выбрать проекты
            FormCheckedListBox dlg1 = new FormCheckedListBox();

            // заполнить список проектов
            Utilities.Controls.FillCheckedListBoxProjects(
                dlg1.clbList, 
                MainWindow.ListExistedProjects,
                MainWindow.MainConnect, 
                null, 
                true, 
                true, 
                true
                );

            // заполнить список выбранных проектов
            List<string> Projects = new List<string>();

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    Projects.Add(itemChecked.ToString());
                }
            }
            dlg1.Dispose();

            if (Projects.Count > 0)
            {
                List<string> DevProjects = new List<string>();
                foreach (var item in Projects.Where(x => Utilities.GITProjects.IsDEVProject(x)))
                {
                    DevProjects.Add(item);
                }

                string branch = Task.TaskNumber;
                if (DevProjects.Count > 0)
                {
                    // выбрать ветку (в новых проектах)
                    FormAskBranch dlg2 = new FormAskBranch(null, null, MainWindow.Task.LogFile, "");

                    foreach (var project in DevProjects)
                    {
                        // Заполнить ListBranches
                        foreach (var item in GIT.GitListBranches(project, "git_listbranch.cmd", MainWindow.Task.LogFile, true, out double n))
                        {
                            string _branch = item.Replace("*", "").Trim();

                            if (
                                (!string.IsNullOrWhiteSpace(_branch)) &&
                                (!dlg2.ListBranches.Contains(_branch))
                                )
                            {
                                dlg2.ListBranches.Add(_branch);
                            }
                        }
                    }

                    var res = dlg2.ShowDialog();

                    if (!string.IsNullOrWhiteSpace(dlg2.Branch))
                    {
                        branch = dlg2.Branch;
                    }

                    dlg2.Dispose();

                    if (res == System.Windows.Forms.DialogResult.Abort)
                    {
                        return;
                    }
                }

                // git pull и переключение на выбранную ветку
                GIT.GitPull(Projects.ToArray(), branch, false, true, false, MainWindow.Task.LogFile, false);

                foreach (var project in Projects)
                {
                    // загружаем yml-файл 
                    YMLStruct yml = new YMLStruct(null, MainWindow.Task.LogFile);
                    string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
                    string file = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "task", tbYMLFile.Text);

                    if (!File.Exists(file))
                    {
                        App.AddLog($"Файла {file} не существует и НЕ будет выполнен", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                    else
                    {
                        yml.LoadYML(project, "task", tbYMLFile.Text, false, null, true, true);

                        // выполнение
                        yml.ExecuteYML(false, false, MainConnect, MainConnect, true);
                    }

                }
            }

        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// нажатие кнопка GIT PULL ALL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGitPullAll_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("Выполнить GIT PULL для всех клонированных проектов?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                GIT.GitPull(MainWindow.ListExistedProjects.ToArray(), "master", false, true, false, null, true);
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Создать новую ветку по номеру задачи или переключиться на существующую
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGitSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                return;
            }

            // выбрать проекты
            FormCheckedListBox dlg1 = new FormCheckedListBox();
            dlg1.clbList.Items.Clear();

            foreach (var item in MainWindow.ListExistedProjects
                .Where(x => Utilities.GITProjects.IsDEVProject(x)) // только новые проекты
                .OrderBy(x =>
                {
                    string ord = "999";
                    if (x == "dev_promed_pg") ord = "001";
                    else if (x == "dev_promed_ms") ord = "003";
                    else if (x == "dev_lis_pg") ord = "005";
                    else if (x == "dev_emd_pg") ord = "007";
                    return ord + x;
                }
                ))
            {
                dlg1.clbList.Items.Add(item, false);
            }

            // заполнить список выбранных проектов
            List<string> ListProjects = new List<string>();

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    ListProjects.Add(itemChecked.ToString());
                }
            }
            dlg1.Dispose();

            // Получим имя новой ветки и выберем имя родительской ветки
            FormNewBranch dlg2 = new FormNewBranch();
            dlg2.ListProjects = ListProjects;
            dlg2.tbNewBranchName.Text = Task.TaskNumber.Trim();
            dlg2.tbParentBranchName.Text = "master";

            if (dlg2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string NewBranchName = dlg2.tbNewBranchName.Text.Trim();
                string ParentBranchName = dlg2.tbParentBranchName.Text.Trim();

                //перебираем выбранные проекты
                foreach (var project in ListProjects)
                {
                    // определяем текущую ветку
                    string branch = GIT.GitCurrentBranch(project, out string err, Task.LogFile);
                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        App.AddLog($"Ошибка определения текущей ветки в проекте {project}:\n{err}", null, App.ShowMessageMode.SHOW, true, Task.LogFile);

                        continue;
                    }

                    // проверим, нужен ли commit в текущую ветку
                    if (!GIT.CheckCommit(project, Task.LogFile, $"Создание\\переключение на ветку {NewBranchName} прервано"))
                    {
                        continue;
                    }

                    // git pull и переключение на ветку NewBranchName
                    GIT.GitPull(new string[] { project }, NewBranchName, false, true, false, Task.LogFile, false);

                    // определяем текущую ветку
                    err = "";
                    branch = GIT.GitCurrentBranch(project, out err, Task.LogFile);
                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        App.AddLog($"Ошибка определения текущей ветки в проекте {project}:\n{err}", null, App.ShowMessageMode.SHOW, true, Task.LogFile);

                        continue;
                    }

                    if (branch != NewBranchName)
                    {
                        // новой ветки нет в проекте
                        if (System.Windows.Forms.MessageBox.Show($"Создать новую ветку {NewBranchName} от ветки {ParentBranchName} в проекте {project}?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            // переключение на ветку ParentBranchName
                            if (!GIT.GitSwitch(project, ParentBranchName, Task.LogFile, out branch, out err))
                            {
                                App.AddLog($"Ошибка переключения на ветку {ParentBranchName} в проекте {project}\n\nТекущая ветка - {branch}\n{err}", null, App.ShowMessageMode.SHOW, true, Task.LogFile);

                                continue;
                            }

                            // создание ветки NewBranchName
                            GIT.GitNewBranch(new string[] { project }, NewBranchName, ParentBranchName, Task.LogFile);

                            // сновая определяем текущую ветку
                            err = "";
                            branch = GIT.GitCurrentBranch(project, out err, Task.LogFile);
                            if (!string.IsNullOrWhiteSpace(err))
                            {
                                App.AddLog($"Ошибка определения текущей ветки в проекте {project}:\n{err}", null, App.ShowMessageMode.SHOW, true, Task.LogFile);

                                continue;
                            }

                            if (branch != NewBranchName)
                            {
                                App.AddLog($"Создание ветки {NewBranchName} в проекте {project} НЕ было выполнено\n\nВ проекте {project} текущая ветка - {branch}", null, App.ShowMessageMode.SHOW, true, Task.LogFile);
                            }
                            else
                            {
                                App.AddLog($"Создание ветки {NewBranchName} в проекте {project} выполнено успешно", null, App.ShowMessageMode.SHOW, true, Task.LogFile);
                            }
                        }
                        else
                        {
                            App.AddLog($"Пользователь отказался создавать ветку {NewBranchName} в проекте {project}\n\nВ проекте {project} текущая ветка - {branch}", null, App.ShowMessageMode.SHOW, true, Task.LogFile);
                        }
                    }
                    else
                    {
                        App.AddLog($"Переключение на ветку {NewBranchName} в проекте {project} выполнено успешно", null, App.ShowMessageMode.SHOW, true, Task.LogFile);
                    }
                }
            }

            dlg2.Dispose();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// выход из поля коэффициента масштаба по горизонтали
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbScaleX_LostFocus(object sender, RoutedEventArgs e)
        {
            string s = tbScaleX.Text
                .Replace(",", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator)
                .Replace(".", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);

            if (
                (double.TryParse(s, out double _scale)) &&
                (_scale >= 1)
                )
            {
                APPinfo.GUI.scaleWindow.ScaleX = _scale;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// выход из поля коэффициента масштаба по вертикали
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbScaleY_LostFocus(object sender, RoutedEventArgs e)
        {
            string s = tbScaleY.Text
                .Replace(",", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator)
                .Replace(".", CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator);

            if (
                (double.TryParse(s, out double _scale)) &&
                (_scale >= 1)
                )
            {
                APPinfo.GUI.scaleWindow.ScaleY = _scale;
            }

        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// нажата кнопка сброса коэффициентов масштаба по умолчанию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btResetScale_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("Сбросить масштаб и размеры окон на значения по умолчанию и завершить программу ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                Default.isResetGUI = true;
                APPinfo.GUI = new GUI();
                this.Close();
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// нажата кнопка сброса шрифтов в textbox редактирования на выбранный
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btChangeFont_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("Сбросить в окнах редактирования шрифты на выбранный ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (var win in MainWindow.APPinfo.GUI.ListWindows)
                {
                    foreach (var item in win.ListScriptBox)
                    {
                        if (cbDefaultFontFamily.SelectedItem != null)
                        {
                            try
                            {
                                item.ScriptBoxFont = ((System.Windows.Media.FontFamily)cbDefaultFontFamily.SelectedItem).ToString();
                                tbExampleFont.FontFamily = (System.Windows.Media.FontFamily)cbDefaultFontFamily.SelectedItem;
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                            }
                        }

                        if (
                            (!string.IsNullOrWhiteSpace(cbDefaultFontSize.Text)) &&
                            double.TryParse(cbDefaultFontSize.Text, out double d)
                        )
                        {
                            item.ScriptBoxFontSize = d.ToString();
                            tbExampleFont.FontSize = d;
                        }
                    }
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// нажата кнопка сброса шрифтов в textbox редактирования на значение по умолчанию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btResetFont_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.Forms.MessageBox.Show("Сбросить в окнах редактирования шрифты на значения по умолчанию ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (var win in MainWindow.APPinfo.GUI.ListWindows)
                {
                    foreach (var item in win.ListScriptBox)
                    {
                        try
                        {
                            var _font = tbTaskNumber.FontFamily;
                            item.ScriptBoxFont = _font.ToString();
                            tbExampleFont.FontFamily = _font;
                            cbDefaultFontFamily.SelectedItem = _font;

                            item.ScriptBoxFontSize = tbTaskNumber.FontSize.ToString();
                            tbExampleFont.FontSize = tbTaskNumber.FontSize;
                            cbDefaultFontSize.Text = tbTaskNumber.FontSize.ToString();
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }

                    }
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// выбран шрифт по умолчанию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDefaultFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbDefaultFontFamily.SelectedItem != null)
            {
                try
                {
                    tbExampleFont.FontFamily = (System.Windows.Media.FontFamily)cbDefaultFontFamily.SelectedItem;
                    MainWindow.APPinfo.GUI.scriptBoxDefault.ScriptBoxFont = ((System.Windows.Media.FontFamily)cbDefaultFontFamily.SelectedItem).ToString();
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// выбран размер шрифта по умолчанию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDefaultFontSize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (
                (!string.IsNullOrWhiteSpace(cbDefaultFontSize.Text)) &&
                double.TryParse(cbDefaultFontSize.Text, out double d)
            )
            {
                tbExampleFont.FontSize = d;
                MainWindow.APPinfo.GUI.scriptBoxDefault.ScriptBoxFontSize = d.ToString();
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выбран флаг "Расширенное логирование"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isExtendedLog_Checked(object sender, RoutedEventArgs e)
        {
            App.AddLog($"Включено расширенное логирование", null, App.ShowMessageMode.NONE, true, null);
            MainWindow.APPinfo.ExtendedLog = "true";
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Снят флаг "Расширенное логирование"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isExtendedLog_Unchecked(object sender, RoutedEventArgs e)
        {
            App.AddLog($"Отключено расширенное логирование", null, App.ShowMessageMode.NONE, true, null);
            MainWindow.APPinfo.ExtendedLog = "false";
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выбран флаг "Улучшать скрипты релиза (метки, changeset и пр.)"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isImproveSQLinVersion_Checked(object sender, RoutedEventArgs e)
        {
            App.AddLog($"Включено улучшение скриптов релиза (метки, changeset и пр.)", null, App.ShowMessageMode.NONE, true, null);
            MainWindow.APPinfo.ImproveSQLinVersion = "true";
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Снят флаг "Улучшать скрипты релиза (метки, changeset и пр.)"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isImproveSQLinVersion_Unchecked(object sender, RoutedEventArgs e)
        {
            App.AddLog($"Отключено улучшение скриптов релиза (метки, changeset и пр.)", null, App.ShowMessageMode.NONE, true, null);
            MainWindow.APPinfo.ImproveSQLinVersion = "false";
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Нажата кнопка "Выгрузить объекты из GIT"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btUnloadFromGIT_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                tbTaskNumber.Focus();
                return;
            }

            WinUploadFromGIT WinUploadFromGIT = new WinUploadFromGIT();

            // заполним список проектов
            WinUploadFromGIT.cbGITProject.Items.Clear();
            WinUploadFromGIT.cbGITProject.Items.Add("");
            foreach (var item in Utilities.Files.ListFilesInDir(MainWindow.APPinfo.GITFolder, true, false, false))
            {
                string project = Utilities.GITProjects.GetProjectByFolder(item);
                string DBType = Utilities.GITProjects.GetDBTypeByProject(project);
                if ((DBType == "MSSQL") || (DBType == "PGSQL"))
                {
                    WinUploadFromGIT.cbGITProject.Items.Add(project);
                }
            }

            if (MainConnect != null) //-V3022
            {
                Utilities.Controls.RefreshGITProjectItems(WinUploadFromGIT.cbGITProject, MainConnect.GITProject);
                //WinUploadFromGIT.cbGITProject_SelectionChanged(null, null);
            }

            WinUploadFromGIT.Show();

        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Нажата кнопка "Изменить файлы в проекте GIT"
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void btWinChangesetInGIT_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            WinChangesetInGIT WinChangesetInGIT = new WinChangesetInGIT();
            WinChangesetInGIT.mainWindow = this;

            // заполним список проектов
            WinChangesetInGIT.cbGITProject.Items.Clear();
            WinChangesetInGIT.cbGITProject.Items.Add("");
            foreach (var item in Utilities.Files.ListFilesInDir(MainWindow.APPinfo.GITFolder, true, false, false))
            {
                string project = Utilities.GITProjects.GetProjectByFolder(item);
                string DBType = Utilities.GITProjects.GetDBTypeByProject(project);
                if ((DBType == "MSSQL") || (DBType == "PGSQL"))
                {
                    WinChangesetInGIT.cbGITProject.Items.Add(project);
                }
            }

            if (MainConnect != null) //-V3022
            {
                Utilities.Controls.RefreshGITProjectItems(WinChangesetInGIT.cbGITProject, MainConnect.GITProject);
            }

            WinChangesetInGIT.Show();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Нажата нопка "Влить в dev"
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void btToDev_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            if (System.Windows.Forms.MessageBox.Show($"Влить ветку {MainWindow.Task.TaskNumber} в ветку dev ?",
                                "ВНИМАНИЕ",
                                System.Windows.Forms.MessageBoxButtons.YesNo
                            ) == System.Windows.Forms.DialogResult.No
                        )
            {
                return;
            }

            // очистим лог-файл
            File.WriteAllText(MainWindow.Task.LogFileMerge, "");

            // заполнить список возможных проектов
            List<string> ListProjects = new List<string>();

            foreach (var item in MainWindow.ListExistedProjects
                .Where(x => Utilities.GITProjects.IsDEVProject(x)) // только новые проекты
                .OrderBy(x =>
                {
                    string ord = "999";
                    if (x == "dev_promed_pg") ord = "001";
                    else if (x == "dev_promed_ms") ord = "003";
                    else if (x == "dev_lis_pg") ord = "005";
                    else if (x == "dev_emd_pg") ord = "007";
                    return ord + x;
                }
                ))
            {
                if (!ListProjects.Contains(item))
                {
                    ListProjects.Add(item);
                }
            }

            // выбрать проект
            string project = "";

            FormFindInList dlg1 = new FormFindInList(MainWindow.Task.LogFileMerge);

            dlg1.AddItems(ListProjects);

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string result = "";

                foreach (System.Windows.Forms.DataGridViewRow row in dlg1.dgListValues.SelectedRows)
                {
                    result = row.Cells[0].Value.ToString();
                    // берем только первую
                    break; //-V3020
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    project = result;
                }
            }

            dlg1.Dispose();

            if (! string.IsNullOrWhiteSpace(project))
            {
                // определяем текущую ветку
                string branch = GIT.GitCurrentBranch(project, out string err, Task.LogFileMerge);

                // проверим, нужен ли commit в текущую ветку
                if (!GIT.CheckCommit(project, Task.LogFileMerge, $"Merge ветки {Task.TaskNumber} в ветку dev прерван"))
                {
                    return;
                }

                // ветка задачи
                branch = Task.TaskNumber.Trim();

                // переключение на ветку задачи
                if (! GIT.GitSwitch(project, branch, Task.LogFileMerge, out branch, out err))
                {
                    App.AddLog($"Ветка {Task.TaskNumber} в проекте {project} не существует или при переключении на ветку {Task.TaskNumber} возникла ошибка: {err}\n\nMerge ветки {Task.TaskNumber} в ветку dev прерван\n\nТекущая ветка - {branch}", null, App.ShowMessageMode.SHOW, true, Task.LogFileMerge);

                    return;
                }

                // переключение на ветку dev
                if (! GIT.GitSwitch(project, "dev", Task.LogFileMerge, out string currentbranch, out err))
                {
                    App.AddLog($"Не получилось переключиться на ветку dev в проекте {project} или при переключении на ветку dev возникла ошибка: {err}\n\nMerge ветки {Task.TaskNumber} в ветку dev прерван\n\nТекущая ветка - {currentbranch}", null, App.ShowMessageMode.SHOW, true, Task.LogFileMerge);

                    return;
                }

                // merge
                if (GIT.GitMerge(project, branch, "dev", true, false, Task.LogFileMerge, false))
                {
                    // merge успешный, делаем push
                    App.AddLog($"Успешный merge ветки {Task.TaskNumber} в проекте {project} в ветку dev\n\nВ проекте {project} текущая ветка - dev", null, App.ShowMessageMode.NONE, true, Task.LogFileMerge);

                    if (System.Windows.Forms.MessageBox.Show($"Успешный merge ветки {Task.TaskNumber} в проекте {project} в ветку dev\n\nВ проекте {project} текущая ветка - dev\n\nВыполнить push ветки dev ?",
                            "ВНИМАНИЕ",
                            System.Windows.Forms.MessageBoxButtons.YesNo
                        ) == System.Windows.Forms.DialogResult.Yes
                    )
                    {
                        // git pull
                        GIT.GitPull(new string[] { project }, "dev", false, false, true, Task.LogFileMerge, false);
                        // git push
                        GIT.GitPush(new string[] { project }, "dev", true, Task.LogFileMerge);

                        // ветка задачи
                        branch = Task.TaskNumber.Trim();

                        // переключение на ветку задачи
                        if (!GIT.GitSwitch(project, branch, Task.LogFileMerge, out branch, out err))
                        {
                            App.AddLog($"Ветка {Task.TaskNumber} в проекте {project} не существует или при переключении на ветку {Task.TaskNumber} возникла ошибка: {err}\n\nТекущая ветка - {branch}", null, App.ShowMessageMode.SHOW, true, Task.LogFileMerge);

                            return;
                        }

                    }
                }
                else
                {
                    App.AddLog($"Merge ветки {Task.TaskNumber} в проекте {project} в ветку dev НЕ был выполнен\n\nВ проекте {project} текущая ветка - dev", null, App.ShowMessageMode.SHOW, true, Task.LogFileMerge);
                }

                // показываем лог
                if (
                    (System.Windows.Forms.MessageBox.Show("Посмотреть лог merge ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                )
                {
                    WinInfo WinInfo = new WinInfo(MainWindow.Task.LogFileMerge);
                    WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                    WinInfo.tbInfo.Text = File.ReadAllText(MainWindow.Task.LogFileMerge);
                    WinInfo.Title = "Лог merge в файле " + Task.LogFileMerge;
                    WinInfo.ShowDialog();
                }

            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Нажата кнопка "Изменить файлы для совместимости с Liquibase 4"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btForLiquibase4_Click(object sender, RoutedEventArgs e)
        {
            WinForLiquibase4 WinForLiquibase4 = new WinForLiquibase4();
            WinForLiquibase4.mainWindow = this;

            // заполним список проектов
            WinForLiquibase4.cbGITProject.Items.Clear();
            WinForLiquibase4.cbGITProject.Items.Add("");
            foreach (var item in Utilities.Files.ListFilesInDir(MainWindow.APPinfo.GITFolder, true, false, false))
            {
                string project = Utilities.GITProjects.GetProjectByFolder(item);
                string DBType = Utilities.GITProjects.GetDBTypeByProject(project);
                if ((DBType == "MSSQL") || (DBType == "PGSQL"))
                {
                    WinForLiquibase4.cbGITProject.Items.Add(project);
                }
            }

            if (MainConnect != null) //-V3022
            {
                Utilities.Controls.RefreshGITProjectItems(WinForLiquibase4.cbGITProject, MainConnect.GITProject);
            }

            WinForLiquibase4.Show();
        }

        /// <summary>
        /// Нажата кнопка "Влить в master"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btToMaster_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            // Спросим, для какого префикса версии (сервиса) вливаем версии: prmd, rpms, smp, bi
            string prefix = GIT.SelectGITModule(Task.LogFileMerge);

            if (string.IsNullOrWhiteSpace(prefix))
            {
                return;
            }

            // заполнить список возможных проектов
            List<string> ListProjects = new List<string>();

            foreach (var project in MainWindow.ListExistedProjects
                .Where(x => Utilities.GITProjects.IsDEVProject(x)) // только новые проекты
                .Where(x => Utilities.GITProjects.GetPrefixFileReleaseByProject(x) == prefix) // только проекты с выбранным префиксом
                .OrderBy(x =>
                {
                    string ord = "999";
                    if (x == "dev_promed_pg") ord = "001";
                    else if (x == "dev_promed_ms") ord = "003";
                    else if (x == "dev_lis_pg") ord = "005";
                    else if (x == "dev_emd_pg") ord = "007";
                    return ord + x;
                }
                ))
            {
                if (
                    (!ListProjects.Contains(project)) &&
                    Directory.Exists(Path.Combine(MainWindow.APPinfo.GITFolder, Utilities.GITProjects.GetFolderByProject(project)))
                )
                {
                    ListProjects.Add(project);
                }
            }

            // выполнить git pull всех проектов
            Utilities.GIT.GitPull(ListProjects.ToArray(), "master", true, true, false, Task.LogFileMerge, true);

            // Выберем вливаемую версию (по умолчанию - первая не влитая)
            FormAskVersion dlg2 = new FormAskVersion();
            dlg2.ListProjects = ListProjects;
            dlg2.FillProjectInfo();
            dlg2.ShowDialog();
            dlg2.Dispose();
        }

        /// <summary>
        /// Выбран флаг "Использовать p__SetNotNull и p__FKCreate"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isUseNewFunc_Checked(object sender, RoutedEventArgs e)
        {
            App.AddLog($"Включено использование p_SetNotNull и p_FKCreate в скриптах", null, App.ShowMessageMode.NONE, true, null);
            MainWindow.APPinfo.UseNewFunc = "true";
        }

        /// <summary>
        /// Выключен флаг "Использовать p__SetNotNull и p__FKCreate"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isUseNewFunc_Unchecked(object sender, RoutedEventArgs e)
        {
            App.AddLog($"Отключено использование p_SetNotNull и p_FKCreate в скриптах", null, App.ShowMessageMode.NONE, true, null);
            MainWindow.APPinfo.UseNewFunc = "false";
        }

        /// <summary>
        /// Нажата кнопка "Поднять строку выше"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btUp_Click(object sender, RoutedEventArgs e)
        {
            if (!dgScriptsInTask.IsFocused) dgScriptsInTask.Focus();

            if (dgScriptsInTask.SelectedIndex >= 0)
            {
                // выбранная строка
                GITScript script = dgScriptsInTask.SelectedItem as GITScript;

                if (script != null) 
                {
                    var prev_script = Task.Scripts
                        .Where(x => x.GITOrder < script.GITOrder)
                        .OrderByDescending(x => x.GITOrder)
                        .FirstOrDefault();

                    if (prev_script != null)
                    {
                        // есть предыдущая строка
                        var prev_order = prev_script.GITOrder;
                        prev_script.GITOrder = script.GITOrder;
                        script.GITOrder = prev_order;
                    }
                }

                try
                {
                    // сохранить текущую задачу
                    SaveTask(Task, false);
                }
                catch (Exception ex)
                {
                    App.AddLog(null, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

                dgScriptsInTaskRefresh();
            }
        }

        /// <summary>
        /// Нажата кнопка "Опустить строку ниже"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDown_Click(object sender, RoutedEventArgs e)
        {
            if (!dgScriptsInTask.IsFocused) dgScriptsInTask.Focus();

            if (dgScriptsInTask.SelectedIndex >= 0)
            {
                // выбранная строка
                GITScript script = dgScriptsInTask.SelectedItem as GITScript;

                if (script != null)
                {
                    var next_script = Task.Scripts
                        .Where(x => x.GITOrder > script.GITOrder)
                        .OrderBy(x => x.GITOrder)
                        .FirstOrDefault();

                    if (next_script != null)
                    {
                        // есть предыдущая строка
                        var next_order = next_script.GITOrder;
                        next_script.GITOrder = script.GITOrder;
                        script.GITOrder = next_order;
                    }
                }

                try
                {
                    // сохранить текущую задачу
                    SaveTask(Task, false);
                }
                catch (Exception ex)
                {
                    App.AddLog(null, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

                dgScriptsInTaskRefresh();
            }
        }

        /// <summary>
        /// Нажата кнопка Действия при обновлении
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDeployment_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            if (Task.TaskNumber.ToUpper().StartsWith("RM-"))
            {
                MessageBox.Show("Действия при обновлении для релизной задачи заполняем через форму 'Собрать Deployment Plan'");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            // Спросим, для какого префикса версии (сервиса) собираем версию с действиями при обновлении: prmd, rpms, smp, bi
            string prefix = GIT.SelectGITModule(Task.LogFile);
            if (string.IsNullOrWhiteSpace(prefix))
            {
                MessageBox.Show("Необходимо выбрать префикс версии (сервис) !");
                return;
            }
            string projectDeploymentMS = Utilities.GITProjects.GetProjectDeployment("MS SQL", prefix);
            string projectDeploymentPG = Utilities.GITProjects.GetProjectDeployment("PG SQL", prefix);

            WinDeployment WinDeployment = new WinDeployment();
            WinDeployment.mainWindow = this;
            WinDeployment.logFile = Task.LogFile;
            WinDeployment.prevversion = "";
            WinDeployment.ProjectDeploymentMS = projectDeploymentMS;
            WinDeployment.ProjectDeploymentPG = projectDeploymentPG;

            if (string.IsNullOrWhiteSpace(Task.JSONFilenameDeploymentMS))
            {
                Task.JSONFilenameDeploymentMS = Task.TaskNumber.Trim() + ".json";
            }
            if (string.IsNullOrWhiteSpace(Task.JSONFilenameDeploymentPG))
            {
                Task.JSONFilenameDeploymentPG = Task.TaskNumber.Trim() + ".json";
            }

            WinDeployment.Show();
        }

        /// <summary>
        /// Нажата кнопка Задания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCron_Click(object sender, RoutedEventArgs e)
        {
            if (!tabTask.IsSelected) tabTask.IsSelected = true;
            if (!btOpenTask.IsFocused) btOpenTask.Focus();

            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            if (Task.TaskNumber.ToUpper().StartsWith("RM-"))
            {
                MessageBox.Show("Задания для релизной задачи заполняем через форму 'Собрать Cron'");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            // Спросим, для какого префикса версии (сервиса) добавляем задание: prmd, rpms, smp, bi
            string prefix = GIT.SelectGITModule(Task.LogFile);
            if (string.IsNullOrWhiteSpace(prefix))
            {
                MessageBox.Show("Необходимо выбрать префикс версии (сервис) !");
                return;
            }
            string projectCronMS = Utilities.GITProjects.GetProjectCron("MS SQL", prefix);
            string projectCronPG = Utilities.GITProjects.GetProjectCron("PG SQL", prefix);

            // форма редактирования заданий
            WinCron WinCron = new WinCron();
            WinCron.mainWindow = this;
            WinCron.logFile = Task.LogFile;
            WinCron.ProjectCronMS = projectCronMS;
            WinCron.ProjectCronPG = projectCronPG;
            WinCron.BranchMS = "";
            WinCron.BranchPG = "";

            // выбрать ветки в проектах
            bool isChoosedInMS = false;
            string BranchDefault = Task.TaskNumber;
            string branch = "";
            string err = "";

            // сначала проверим наличие ветки по номеру задачи
            if (Utilities.GIT.GitSwitch(projectCronMS, Task.TaskNumber, Task.LogFile, out branch, out err))
            {
                BranchDefault = branch;
                WinCron.BranchMS = branch;
                isChoosedInMS = true;
            }
            else
            {
                // есть проект для MS, выберем в нем ветку
                if (WinCron.SelectGITBranch(projectCronMS, ref branch, ref isChoosedInMS, Task.LogFile, $"Ветка {Task.TaskNumber} не найдена"))
                {
                    // выбрали ветку
                    BranchDefault = branch;
                    WinCron.BranchMS = branch;
                }
            }

            branch = "";
            err = "";

            // сначала проверим наличие ветки по номеру задачи
            if (Utilities.GIT.GitSwitch(projectCronPG, Task.TaskNumber, Task.LogFile, out branch, out err))
            {
                BranchDefault = branch;
                WinCron.BranchPG = branch;
            }
            else
            {
                branch = BranchDefault;

                // есть проект для PG, выберем в нем ветку
                if (WinCron.SelectGITBranch(projectCronPG, ref branch, ref isChoosedInMS, Task.LogFile, $"Ветка {Task.TaskNumber} не найдена"))
                {
                    // выбрали ветку
                    WinCron.BranchPG = branch;
                    BranchDefault = branch; //-V3137
                }
            }

            WinCron.Show();
        }

        /// <summary>
        /// Выбран пункт меню Загрузить xls-список cron
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btLoadCron_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            // Спросим, для какого префикса версии (сервиса) собираем версию с действиями при обновлении: prmd, rpms, smp, bi
            string prefix = GIT.SelectGITModule(Task.LogFile);
            if (string.IsNullOrWhiteSpace(prefix))
            {
                MessageBox.Show("Необходимо выбрать префикс версии (сервис) !");
                return;
            }
            string projectCronMS = Utilities.GITProjects.GetProjectCron("MS SQL", prefix);
            string projectCronPG = Utilities.GITProjects.GetProjectCron("PG SQL", prefix);

            // заполнить список возможных проектов
            List<string> ListProjects = new List<string>();

            ListProjects.Add(projectCronMS);
            ListProjects.Add(projectCronPG);

            // выбрать проект и ветку
            if (
                !Utilities.GIT.SelectGITProject(ListProjects, null, out string project, out string branch, MainWindow.Task.LogFile, false) ||
                string.IsNullOrWhiteSpace(project) ||
                string.IsNullOrWhiteSpace(branch)
                )
            {
                return;
            }

            string _dbregion = "";
            string dbtype = Utilities.GITProjects.GetDBTypeByProject(project);
            ConnectDB connect;

            if (dbtype == "MSSQL")
            {
                _dbregion = "MS SQL";
                connect = MainWindow.GetConnectByGITProject(project, "promeddev", true);
            }
            else
            {
                _dbregion = "PG SQL";
                connect = MainWindow.GetConnectByGITProject(project, "promedtest", true);
            }

            if (
                connect != null &&
                !connect.isConnected
            )
            {
                connect.OpenConnect(false);
            }

            if (
                connect == null ||
                !connect.isConnected
            )
            {
                App.AddLog($"Требуется действующее подключение к тестовой БД {project}", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);

                return;
            }

            // выбираем excel
            string xlsfile = "";
            bool isExit = false;
            string _proctext = "";
            string _jsontext = "";

            xlsfile = Controls.Dialogs.OpenExcelDialog(MainWindow.Task.TaskPath);

            if (!string.IsNullOrWhiteSpace(xlsfile))
            {
                // считывем Excel 
                this.Cursor = Cursors.Wait;

                Excel.Application excelApp = null;
                Excel.Workbook excelWorkBook = null;

                // итоговый список
                ObservableCollection<Cron> cron_list = new ObservableCollection<Cron>();

                try
                {
                    excelApp = new Excel.Application();
                    excelApp.Visible = false;
                    excelWorkBook = excelApp.Workbooks.Open(xlsfile, null, true);
                    Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets[1]; // с первой страницы книги

                    // Начиная с 2-й строки - данные
                    int maxPass = 10; // максимальное кол-во пустых строк для завершения считывания
                    int cntRow = 1;

                    // заполняем список из excel
                    do
                    {
                        cntRow++;
                        string _application_name = "";

                        try
                        {
                            string _database = excelWorkSheet.Cells[cntRow, 1].Text;
                            _database = _database.Trim();
                            string _dbms = excelWorkSheet.Cells[cntRow, 2].Text;
                            _dbms = _dbms.Trim();
                            string _order_str = excelWorkSheet.Cells[cntRow, 3].Text;
                            _order_str = _order_str.Trim();

                            if (!int.TryParse(_order_str, out int _order))
                            {
                                _order = 0;
                            }

                            string _comment = excelWorkSheet.Cells[cntRow, 4].Text;
                            _comment = _comment
                                .Trim()
                                .Replace("'", "''");
                            string _command = excelWorkSheet.Cells[cntRow, 5].Text;
                            _command = _command
                                .Trim()
                                .Replace("'", "''");
                            _application_name = excelWorkSheet.Cells[cntRow, 6].Text;
                            _application_name = _application_name.Trim();
                            string _state = excelWorkSheet.Cells[cntRow, 7].Text;
                            _state = _state.Trim();
                            string _regions = excelWorkSheet.Cells[cntRow, 8].Text;
                            _regions = _regions.Trim();
                            string _exclude_regions = excelWorkSheet.Cells[cntRow, 9].Text;
                            _exclude_regions = _exclude_regions.Trim();
                            string _timeout_str = excelWorkSheet.Cells[cntRow, 10].Text;
                            _timeout_str = _timeout_str.Trim();

                            if (!int.TryParse(_timeout_str, out int t))
                            {
                                _timeout_str = "NULL";
                            }

                            if (t == 0)
                            {
                                _timeout_str = "NULL";
                            }

                            string _schedule = excelWorkSheet.Cells[cntRow, 11].Text;
                            _schedule = _schedule.Trim();
                            string _stage = excelWorkSheet.Cells[cntRow, 12].Text;
                            _stage = _stage.Trim();
                            string _team = excelWorkSheet.Cells[cntRow, 13].Text;
                            _team = _team.Trim();
                            string _version = excelWorkSheet.Cells[cntRow, 14].Text;
                            _version = _version.Trim();
                            string _check = excelWorkSheet.Cells[cntRow, 15].Text;
                            _check = _check
                                .Trim()
                                .Replace("'", "''");
                            string _istemp_str = excelWorkSheet.Cells[cntRow, 16].Text;
                            _istemp_str = _istemp_str.Trim();

                            if (!int.TryParse(_istemp_str, out int _istemp))
                            {
                                _istemp = 1;
                            }

                            string _task = MainWindow.Task.TaskNumber;

                            if (
                                _dbregion == "MS SQL" &&
                                (
                                    MainWindow.APPinfo.ListDBAlias_ForALL.Contains(_database)
                                    ||
                                    _dbms == "MS SQL"
                                    ||
                                    _database == "lis"
                                )
                                ||
                                _dbregion == "PG SQL" &&
                                (
                                    MainWindow.APPinfo.ListDBAlias_ForALL.Contains(_database)
                                    ||
                                    _dbms == "PG SQL" && (_database != "lis")
                                )
                            )
                            {
                                // вызываем dbo.xp_Gen_Cron
                                _proctext = "";
                                _jsontext = "";

                                if (_dbregion == "MS SQL")
                                {
                                    _proctext = $"DECLARE @res VARCHAR(max); exec dbo.xp_gen_cron @task = '{_task}', @order = {_order}, @application_name = '{_application_name}', @comment = '{_comment}', @command = '{_command}', @schedule = '{_schedule}', @database = '{_database}', @dbms = '{_dbms}', @state = '{_state}', @stage = '{_stage}', @regions = '{_regions}', @exclude_regions = '{_exclude_regions}', @timeout = {_timeout_str}, @check = '{_check}', @team = '{_team}', @istemp = {_istemp}, @res = @res OUTPUT; select @res";
                                }
                                else
                                {
                                    _proctext = $"select xp_gen_cron from dbo.xp_gen_cron (p_task := '{_task}', p_order := {_order}, p_application_name := '{_application_name}', p_comment := '{_comment}', p_command := '{_command}', p_schedule := '{_schedule}', p_database := '{_database}', p_dbms := '{_dbms}', p_state := '{_state}', p_stage := '{_stage}', p_regions := '{_regions}', p_exclude_regions := '{_exclude_regions}', p_timeout := {_timeout_str}, p_check := '{_check}', p_team := '{_team}', p_istemp := {_istemp});";
                                }

                                if (!string.IsNullOrWhiteSpace(_proctext))
                                {
                                    using (DbDataReader reader = connect.OpenQuery(_proctext))
                                    {
                                        if (reader != null)
                                        {
                                            while (reader.Read())
                                            {
                                                _jsontext = reader[0].ToString();
                                                break; //-V3020
                                            }
                                        }
                                    }

                                    // разбираем полученный json-текст и добавляем в общий список
                                    var xls_cron = Cron.DeserializeJSON(_jsontext, MainWindow.Task.LogFile, false);

                                    if (
                                        xls_cron != null &&
                                        xls_cron.Count > 0
                                    )
                                    {
                                        // проверяем корректность\исправляем содержимое загруженного файла
                                        if (!Cron.CheckJSON(xls_cron, null, MainWindow.Task.LogFile, out string err, App.ShowMessageMode.NONE))
                                        {
                                            MessageBox.Show(err);

                                            if (System.Windows.Forms.MessageBox.Show($"Прервать выполнение?", "ВНИМАНИЕ",
                                            System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                            {
                                                isExit = true;
                                                break;
                                            }
                                        }

                                        foreach (var item in xls_cron
                                            .Select(x => x.ToCron(_dbregion, null, null))
                                            .OrderBy(x => x.order)
                                        )
                                        {
                                            cron_list.Add(item);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Cursor = Cursors.Arrow;
                            App.AddLog($"Ошибка в строке {cntRow}: " + Environment.NewLine, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                            isExit = true;
                            break;
                        }

                        if (string.IsNullOrWhiteSpace(_application_name))
                        {
                            maxPass--;
                        }
                    } while (maxPass > 0);
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog(null, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

                if (excelWorkBook != null) excelWorkBook.Close();
                if (excelApp != null) excelApp.Quit();
                connect.CloseConnect();

                // сохраняем json
                if (!isExit && cron_list.Count() > 0)
                {
                    // сохранить json-файл с новым содержимым
                    WinCron.SaveJSON(project, _dbregion, branch, null, null, false, cron_list, true, out string jsonFilepath, out string jsonUrl, MainWindow.Task.LogFile, null);
                }

                if (isExit)
                {
                    MessageBox.Show("Загрузка прервана");
                }
                else
                {
                    MessageBox.Show("Загрузка завершена");
                }

                this.Cursor = Cursors.Arrow;
            }
        }

        private void btSendLog_Click(object sender, RoutedEventArgs e)
        {
            // спрашиваем, куда сохранить
            string zipFilePath = Controls.Dialogs.SaveZIPDialog(Task.TaskPath, Task.ZipLogFiles);

            if (string.IsNullOrWhiteSpace(zipFilePath))
            {
                return;
            }

            App.AddLog($"Создаем архив {zipFilePath} с log-файлами и SQLGen.json", null, App.ShowMessageMode.NONE, true, "");

            try
            {
                using (FileStream zipToOpen = new FileStream(zipFilePath, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                    {
                        if (File.Exists(App.AppLogFile))
                        {
                            archive.CreateEntryFromFile(App.AppLogFile, System.IO.Path.GetFileName(App.AppLogFile));
                        }
                        if (File.Exists(System.IO.Path.Combine(App.AppPath, "SQLGen.json")))
                        {
                            archive.CreateEntryFromFile(System.IO.Path.Combine(App.AppPath, "SQLGen.json"), "SQLGen.json");
                        }
                        if (File.Exists(Task.LogFile))
                        {
                            archive.CreateEntryFromFile(Task.LogFile, System.IO.Path.GetFileName(Task.LogFile));
                        }
                        if (File.Exists(Task.LogFileMerge))
                        {
                            archive.CreateEntryFromFile(Task.LogFileMerge, System.IO.Path.GetFileName(Task.LogFileMerge));
                        }
                        if (File.Exists(Task.LogFileRelease))
                        {
                            archive.CreateEntryFromFile(Task.LogFileRelease, System.IO.Path.GetFileName(Task.LogFileRelease));
                        }
                        if (File.Exists(Task.LogFileTable))
                        {
                            archive.CreateEntryFromFile(Task.LogFileTable, System.IO.Path.GetFileName(Task.LogFileTable));
                        }
                        if (File.Exists(Task.TaskFile))
                        {
                            archive.CreateEntryFromFile(Task.TaskFile, System.IO.Path.GetFileName(Task.TaskFile));
                        }
                    }
                }

                if (File.Exists(zipFilePath))
                {
                    Utilities.External.OpenDirectory(Path.GetDirectoryName(zipFilePath));
                    System.Diagnostics.Process.Start(zipFilePath);
                }
            }
            catch (Exception ex)
            {
                App.AddLog(null, ex, App.ShowMessageMode.SHOW, true, "");
            }
        }

        private void isTaskReleaseCooperative_Checked(object sender, RoutedEventArgs e)
        {
            App.AddLog($"Включен кооперативный режим сборки версий", null, App.ShowMessageMode.NONE, true, null);
            MainWindow.APPinfo.TaskReleaseCooperative = "true";

        }

        private void isTaskReleaseCooperative_Unchecked(object sender, RoutedEventArgs e)
        {
            App.AddLog($"Выключен кооперативный режим сборки версий", null, App.ShowMessageMode.NONE, true, null);
            MainWindow.APPinfo.TaskReleaseCooperative = "false";
        }
    }
}

