// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Path = System.IO.Path;
using SQLGen.Controls;
using SQLGen.Utilities;
using System.Collections.ObjectModel;
using ICSharpCode.AvalonEdit.Highlighting;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>
    /// Окно Формирование заданий релизной версии
    /// </summary>
    public partial class WinReleaseCron
    {
        /// <summary>ссылка на экземпляр основного окна программы</summary>
        public MainWindow mainWindow;

        /// <summary>
        /// фильтрация задач
        /// </summary>
        private CollectionViewSource _filteredJSON;

        /// <summary>
        /// Проект для хранения заданий MS 
        /// </summary>
        public string ProjectCronMS = null;

        /// <summary>
        /// Проект для хранения заданий PG 
        /// </summary>
        public string ProjectCronPG = null;

        /// <summary>
        /// фильтрация задач
        /// </summary>
        private ICollectionView FilteredJSON
        {
            get
            {
                if (_filteredJSON != null)
                {
                    return _filteredJSON.View;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>Полный список версий</summary>
        public SortedDictionary<double, Version> Versions = new SortedDictionary<double, Version>();

        /// <summary>Список СЛЕДУЮЩИХ версий</summary>
        public SortedDictionary<double, Version> NextVersions = new SortedDictionary<double, Version>();

        /// <summary>Конструктор WinReleaseCron</summary>
        public WinReleaseCron()
        {
            InitializeComponent();

            // установить фильтр
            _filteredJSON = new CollectionViewSource();
            _filteredJSON.Source = MainWindow.Task.ReleaseYMLFiles;
            if (_filteredJSON.View != null)
            {
                _filteredJSON.View.Filter = delegate (object o) { return ShowOnlyFilter(o); };
            }
            if (_filteredJSON.View != null && _filteredJSON.View.CanSort == true)
            {
                _filteredJSON.View.SortDescriptions.Clear();
                _filteredJSON.View.SortDescriptions.Add(new SortDescription("YMLOrder", ListSortDirection.Ascending));
            }

            dgJSONFiles.ItemsSource = FilteredJSON;

            ClearFields();

            // Контроль размера лог-файла
            Files.CutEndFileMaxSize(MainWindow.Task.LogFileRelease);

            // пользовательские настройки GUI
            Default.InitGUI("WinReleaseCron", this, mainGrid, null, null, null, MainWindow.Task.LogFileRelease);
        }

        /// <summary>При открытии окна WinReleaseCron</summary>
        private void winReleaseCron_Activated(object sender, EventArgs e)
        {
            this.Title = "Сборка Cron релиза " + MainWindow.Task.ReleaseVersion + ", задача " + MainWindow.Task.TaskNumber;

            dgJSONFiles.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            dgJSONFilesRefresh();

            tbNumVersion.Text = MainWindow.Task.ReleaseVersion;
            tbReleaseTaskNumber.Text = MainWindow.Task.TaskNumber;

            // url на Deployment Plan в Confluence
            if (tbNumVersion.Text.StartsWith("smp"))
            {
                tbDeploymentConfVersion.Text = "https://confluence.rtmis.ru/pages/viewpage.action?spaceKey=RMISRELEASES&title=SMP-DeploymentPlan-" + tbNumVersion.Text;
            }
            else if (tbNumVersion.Text.StartsWith("bi"))
            {
                tbDeploymentConfVersion.Text = "https://confluence.rtmis.ru/pages/viewpage.action?spaceKey=RMISRELEASES&title=BI-DeploymentPlan-" + tbNumVersion.Text;
            }
            else if (tbNumVersion.Text.StartsWith("rpms"))
            {
                tbDeploymentConfVersion.Text = "https://confluence.rtmis.ru/pages/viewpage.action?spaceKey=RMISRELEASES&title=UserPortal-DeploymentPlan-" + tbNumVersion.Text;
            }
            else
            {
                tbDeploymentConfVersion.Text = "https://confluence.rtmis.ru/pages/viewpage.action?spaceKey=RMISRELEASES&title=PROMED-DeploymentPlan-" + tbNumVersion.Text;
            }
        }

        /// <summary>При закрытии окна WinReleaseCron</summary>
        private void winReleaseCron_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinReleaseCron", this, null);
        }

        /// <summary>
        /// Очистить поля на форме
        /// </summary>
        private void ClearFields()
        {
            tbBranch.Text = "";
            tbCronFileVersion.Text = "";
            tbCronGITVersion.Text = "";

            btGitPull.IsEnabled = false;
            btGitPush.IsEnabled = false;

            btGitChangeBranch.IsEnabled = false;

            dgJSONFilesRefresh();
        }

        /// <summary>
        /// Видимость полей и кнопок для текущего выбранного проекта
        /// </summary>
        private void SetVisiblyForProject(string project)
        {
            tbNumVersion.IsReadOnly = true;

            btGitPull.IsEnabled = true;
            btGitPush.IsEnabled = true;

            tbCronFileVersion.IsEnabled = true;
            //tbCronFileVersion.IsReadOnly = false;

            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);

            string branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);

            btGitChangeBranch.IsEnabled = true;
            btGitChangeBranch.Content = $"Переключиться на ветку {branchversion.Replace("_", "__")}";

            if (tbBranch.Text.Trim().ToLower() == branchversion.ToLower())
            {
                btGitChangeBranch.IsEnabled = false;
            }
        }

        /// <summary>
        /// Переключение проекта GIT
        /// </summary>
        /// <param name="branchversion">ветка версии</param>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        private void cbGITProject_Sync(string branchversion, bool isForcedGitRefresh)
        {
            ClearFields();

            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                return;
            }

            string project = cbGITProject.SelectedItem.ToString().Trim();
            cbGITProject.SelectedItem = project;

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            string URL = Utilities.GITProjects.GetURLVersionByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string postfix = Utilities.GITProjects.GetPostfixFileReleaseByProject(project);

            if (string.IsNullOrWhiteSpace(branchversion))
            {
                branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);
            }

            // обновить проект GIT
            GitPull_Sync(branchversion, isForcedGitRefresh);

            string err = "";
            tbBranch.Text = GIT.GitCurrentBranch(project, out err, MainWindow.Task.LogFileRelease);

            // Найдем среди версий json-файл с заданиями
            bool isexist = false;
            tbCronFileVersion.Text = Release.GetJsonFile(project, tbNumVersion.Text, "cron", out isexist);
            tbCronFileVersion.IsReadOnly = isexist;

            // url на версию в GIT
            URL = URL.Replace("%BRANCH%", tbBranch.Text);
            tbCronGITVersion.Text = URL + tbCronFileVersion.Text.Trim();

            // проверка текущей ветки и видимость кнопок и полей
            CheckBranch(out string branch);
        }

        /// <summary>Выбран проект GIT</summary>
        private void cbGITProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbGITProject_Sync("", false);
        }

        /// <summary>
        /// =true - строка входит в фильтр
        /// </summary>
        /// <param name="e">экземпляр YMLFileInfo</param>
        /// <returns></returns>
        public bool ShowOnlyFilter(object e)
        {
            var _json = e as YMLFileInfo;
            if (_json != null)
            {
                return _json.PathInGIT.ToLower().StartsWith("cron");
            }
            else
            {
                return true;
            }
        }

        /// <summary>Обновить dgJSONFiles</summary>
        public void dgJSONFilesRefresh()
        {
            // обновить грид
            if (FilteredJSON != null)
            {
                var lcv = (ListCollectionView)FilteredJSON;
                if (lcv.IsAddingNew) lcv.CommitNew();
                if (lcv.IsEditingItem) lcv.CommitEdit();
                FilteredJSON.Refresh();
            }

            // обновить итоги
            TaskCalcCount();

            // скрыть/показать колонки
            YMLFile_dev_PG.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_MS.Visibility = System.Windows.Visibility.Hidden;

            YMLFile_dev_userportal_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_userportal_ms.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_smp2_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_bi.Visibility = System.Windows.Visibility.Hidden;

            YMLFile_unknown.Visibility = System.Windows.Visibility.Hidden;
            BranchName.Visibility = System.Windows.Visibility.Hidden;

            if (cbGITProject.SelectedIndex != -1)
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();
                if (Utilities.GITProjects.IsDEVProject(project))
                {
                    BranchName.Visibility = System.Windows.Visibility.Visible;
                }
            }

            foreach (var item in MainWindow.Task.ReleaseYMLFiles.Where(x => x.PathInGIT.ToLower().StartsWith("cron")))
            {
                if ((YMLFile_dev_PG.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_PG)))
                    YMLFile_dev_PG.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_MS.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_MS)))
                    YMLFile_dev_MS.Visibility = System.Windows.Visibility.Visible;

                if ((YMLFile_dev_userportal_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_userportal_pg)))
                    YMLFile_dev_userportal_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_userportal_ms.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_userportal_ms)))
                    YMLFile_dev_userportal_ms.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_smp2_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_smp2_pg)))
                    YMLFile_dev_smp2_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_bi.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_bi)))
                    YMLFile_dev_bi.Visibility = System.Windows.Visibility.Visible;

                if ((YMLFile_unknown.Visibility == System.Windows.Visibility.Hidden) && (item.YMLFile_unknown != null) && (item.YMLFile_unknown != ""))
                    YMLFile_unknown.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>Подсчет итогов</summary>
        public void TaskCalcCount()
        {
            int JsonCount = 0;
            int JsonCountRelease = 0;
            int TaskCount = 0;

            if (MainWindow.Task.ReleaseYMLFiles.Count() != 0)
            {
                foreach (var item in MainWindow.Task.ReleaseYMLFiles.Where(x => x.PathInGIT.ToLower().StartsWith("cron")))
                {
                    JsonCount++;
                    if (item.IsAddRelease)
                    {
                        JsonCountRelease++;
                    }
                }

                TaskCount = MainWindow.Task.ReleaseYMLFiles.Where(x => x.PathInGIT.ToLower().StartsWith("cron"))
                    .Select(x => x.TaskNumber).Distinct().Count();

                lbCount.Content = "Всего задач " + TaskCount.ToString() + ", файлов " + JsonCount.ToString() + ", из них включаем в релиз " + JsonCountRelease.ToString();
            }
            else
            {
                lbCount.Content = "Всего задач 0";
            }
        }

        /// <summary>
        /// Выбрана ячейка
        /// </summary>
        private void SelectCell()
        {
            if ((dgJSONFiles.SelectedCells.Count > 0) && (dgJSONFiles.CurrentColumn != null))
            {
                var CellValue = Utilities.Controls.GetSelectedValue(dgJSONFiles, dgJSONFiles.CurrentColumn.DisplayIndex);

                if (dgJSONFiles.CurrentColumn.DisplayIndex == 2)
                {
                    tbSelectedCell.Text = MainWindow.APPinfo.TaskUrlDefault + CellValue;
                }
                else
                {
                    tbSelectedCell.Text = CellValue;
                }
            }
        }

        /// <summary>Изменилось содержимое ячейки в таблице файлов</summary>
        private void dgJSONFiles_CurrentCellChanged(object sender, EventArgs e)
        {
            TaskCalcCount();
            SelectCell();
        }

        /// <summary>
        /// Показать файл в целевой ветке GIT
        /// </summary>
        /// <param name="default_branch">целевая ветка</param>
        private void ViewFileInGIT(string default_branch)
        {
            if (dgJSONFiles.CurrentColumn != null)
            {
                var field = dgJSONFiles.CurrentColumn.SortMemberPath;

                if ((dgJSONFiles.SelectedCells.Count > 0) && field.Contains("YMLFile_"))
                {
                    // получаем имя файла
                    var CellValue = Utilities.Controls.GetSelectedValue(dgJSONFiles, dgJSONFiles.CurrentColumn.DisplayIndex) ?? "";
                    CellValue = CellValue.Trim();

                    // очищаем имя файла
                    var arrf = CellValue.Split('.');

                    string branch = default_branch;

                    if (string.IsNullOrWhiteSpace(default_branch))
                    {
                        branch = arrf[0];
                    }

                    if (arrf.Length > 1)
                    {
                        if (arrf[1].StartsWith("json"))
                        {
                            CellValue = arrf[0] + ".json";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(CellValue))
                    {
                        var col_path = dgJSONFiles.Columns.Where(x => x.SortMemberPath == "PathInGIT").FirstOrDefault();
                        string path = "";
                        if (col_path != null)
                        {
                            path = Utilities.Controls.GetSelectedValue(dgJSONFiles, col_path.DisplayIndex);
                        }

                        if (string.IsNullOrWhiteSpace(default_branch))
                        {
                            var col_branch = dgJSONFiles.Columns.Where(x => x.SortMemberPath == "BranchName").FirstOrDefault();
                            if (col_branch != null)
                            {
                                branch = Utilities.Controls.GetSelectedValue(dgJSONFiles, col_branch.DisplayIndex);
                            }
                        }

                        // собираем url
                        string project = Utilities.GITProjects.GetProjectByYMLField(field);
                        string url = Utilities.GITProjects.GetURLByProject(project)
                            .Replace("%BRANCH%", branch) +
                            (string.IsNullOrWhiteSpace(path) ? "" : path + "/");

                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            System.Diagnostics.Process.Start(url + CellValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Показать текст файла в ветке версии в GIT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgJSONFiles_MenuItemShowJSONinVer(object sender, RoutedEventArgs e)
        {
            if (dgJSONFiles.CurrentColumn != null)
            {
                var field = dgJSONFiles.CurrentColumn.SortMemberPath;

                if ((dgJSONFiles.SelectedCells.Count > 0) && field.Contains("YMLFile_"))
                {
                    string branch = tbBranch.Text.Trim();

                    if (string.IsNullOrWhiteSpace(branch))
                    {
                        string prefix = Utilities.GITProjects.GITProjectsParam("GITYMLField", field, "PrefixFileRelease");
                        branch = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);
                    }

                    ViewFileInGIT(branch);
                }
            }
        }

        /// <summary>Двойной клик мышкой на ячейке в таблице JSON-файлов</summary>
        private void dgJSONFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (dgJSONFiles.CurrentColumn != null)
            {
                var field = dgJSONFiles.CurrentColumn.SortMemberPath;

                if ((dgJSONFiles.SelectedCells.Count > 0) && field.Contains("YMLFile_"))
                {
                    // Показать текст файла в ветке задачи в GIT
                    ViewFileInGIT("");
                }

                if ((dgJSONFiles.SelectedCells.Count > 0) && (field == "TaskNumber"))
                {
                    var CellValue = Utilities.Controls.GetSelectedValue(dgJSONFiles, dgJSONFiles.CurrentColumn.DisplayIndex);

                    if (!string.IsNullOrWhiteSpace(CellValue))
                    {
                        // Показать задачу в Jira
                        System.Diagnostics.Process.Start(MainWindow.APPinfo.TaskUrlDefault + CellValue);
                    }
                }

                if ((dgJSONFiles.SelectedCells.Count > 0) && (field == "BranchName"))
                {
                    var CellValue = Utilities.Controls.GetSelectedValue(dgJSONFiles, dgJSONFiles.CurrentColumn.DisplayIndex);

                    if (!string.IsNullOrWhiteSpace(CellValue))
                    {
                        if (cbGITProject.SelectedIndex != -1)
                        {
                            // Показать ветку задачу в GIT
                            string project = cbGITProject.SelectedItem.ToString().Trim();
                            string url = Utilities.GITProjects.GetURLByProject(project)
                                .Replace("%BRANCH%", CellValue)
                                .TrimEnd(new char[] { '/' });

                            System.Diagnostics.Process.Start(url);
                        }
                    }
                }
            }
        }

        /// <summary>Нажата клавиша в таблице JSON-файлов</summary>
        private void dgJSONFiles_KeyDown(object sender, KeyEventArgs e)
        {
        }

        /// <summary>Нажата кнопка Выгрузить в Excel</summary>
        private void btExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Utilities.MSOffice.GenerateExcel(Utilities.Databases.ConvertToDataTable(
                    MainWindow.Task.ReleaseYMLFiles
                    .Where(x => 
                        x.PathInGIT.ToLower().StartsWith("cron")
                        )
                    .OrderBy(x => x.YMLOrder)
                    .ToList()
                    ), false, "", true);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
            }
        }

        /// <summary>Выбор строки в dgJSONFiles</summary>
        private void dgJSONFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgJSONFiles.SelectedItem == null)
            {
                tbTaskNum.Text = "";
                tbTaskStatus.Text = "";
                tbTaskVersion.Text = "";
                tbTaskDT.Text = "";
                tbTaskBaseRegion.Text = "";
                tbTaskRegion.Text = "";
                tbTaskUpdActions.Text = "";
                tbTaskDataBD.Text = "";
                tbTaskObjectsBD.Text = "";
                tbSelectedCell.Text = "";
            }
            else
            {
                var row = (YMLFileInfo)dgJSONFiles.SelectedItem;
                tbTaskNum.Text = row.TaskNumber;
                tbTaskStatus.Text = row.TaskStatus;
                tbTaskVersion.Text = row.Version;
                if (row.IsDowntime) tbTaskDT.Text = "Требуется Downtime";
                else tbTaskDT.Text = "";
                if (row.IsBaseRegion) tbTaskBaseRegion.Text = "Базовая региональность БД";
                else tbTaskBaseRegion.Text = "";
                tbTaskRegion.Text = row.Region;
                tbTaskUpdActions.Text = row.UpdActions;
                tbTaskDataBD.Text = row.DataBD;
                tbTaskObjectsBD.Text = row.ObjectsBD;

                SelectCell();
            }
        }

        /// <summary>Разрешение действия по умолчанию при нажатии клавиши в dgJSONFiles</summary>
        private void dgJSONFiles_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Device.Target.GetType().Name == "DataGridCell") && (dgJSONFiles.SelectedCells.Count > 0))
            {
                if (e.Key == Key.Delete)
                {
                    string CellValue = "";
                    foreach (var col in dgJSONFiles.Columns)
                    {
                        if (col.SortMemberPath == "TaskNumber")
                        {
                            CellValue = Utilities.Controls.GetSelectedValue(dgJSONFiles, col.DisplayIndex);
                            break;
                        }
                    }
                    MessageBoxResult res = MessageBox.Show("Удалить задачу " + CellValue + "?", "ВНИМАНИЕ!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (res == MessageBoxResult.Yes)
                    {
                        App.AddLog($"Выбрано удаление задачи {CellValue} из списка задач", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFileRelease);
                    }

                    e.Handled = (res != MessageBoxResult.Yes);
                }
            }
        }

        /// <summary>пункт меню Удалить задачу в dgJSONFiles</summary>
        private void dgJSONFiles_MenuItemDelete(object sender, RoutedEventArgs e)
        {
            if (dgJSONFiles.SelectedIndex >= 0)
            {
                var _json = dgJSONFiles.SelectedItem as YMLFileInfo;
                MainWindow.Task.ReleaseYMLFiles.Remove(_json);
                dgJSONFilesRefresh();

                // сохраняем задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                    btSaveTask.Focus();
                }
            }
        }

        /// <summary>Нажата кнопка Сохранить задачу</summary>
        private void btSaveTask_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow != null)
            {
                mainWindow.miSaveTask_Click(sender, e);
                btSaveTask.Focus();
            }
        }

        /// <summary>
        /// Нажа кнопка Отправить в GIT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btTortoiseGIT_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            string GITProject = cbGITProject.SelectedItem.ToString().Trim();

            // Отправляем в GIT
            GIT.GitAdd(new string[] { GITProject }, tbBranch.Text.Trim(), true, false, MainWindow.Task.LogFileRelease);
        }

        /// <summary>
        /// Выбран пункт меню "Текст ошибки в отдельном окне"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgJSONFiles_MenuItemShowError(object sender, RoutedEventArgs e)
        {
            if (dgJSONFiles.CurrentColumn != null)
            {

                var field = dgJSONFiles.CurrentColumn.SortMemberPath;

                if ((dgJSONFiles.SelectedCells.Count > 0) && field.Contains("YMLFile_"))
                {
                    var CellValue = Utilities.Controls.GetSelectedValue(dgJSONFiles, dgJSONFiles.CurrentColumn.DisplayIndex);

                    if (CellValue != "")
                    {
                        var row = (YMLFileInfo)dgJSONFiles.CurrentItem;

                        WinInfo WinInfo = new WinInfo(null);
                        WinInfo.Title = "Ошибка";
                        WinInfo.tbInfo.Text = row.GetYMLFile_Comment(field);
                        WinInfo.Show();
                    }
                }

                if ((dgJSONFiles.SelectedCells.Count > 0) && (field == "TaskNumber"))
                {
                    var CellValue = Utilities.Controls.GetSelectedValue(dgJSONFiles, dgJSONFiles.CurrentColumn.DisplayIndex);

                    if (CellValue != "")
                    {
                        var row = (YMLFileInfo)dgJSONFiles.CurrentItem;

                        WinInfo WinInfo = new WinInfo(null);
                        WinInfo.Title = "Ошибка";
                        WinInfo.tbInfo.Text = row.ErrorInfo;
                        WinInfo.Show();
                    }
                }
            }
        }

        /// <summary>
        /// Проверка заполнения нужных полей для продолжения
        /// </summary>
        /// <param name="branch">текущая ветка</param>
        /// <returns></returns>
        private bool CheckBranch(out string branch)
        {
            branch = "";

            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return false;
            }

            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                {
                    MessageBox.Show("Выберите проект ГИТ");
                    cbGITProject.Focus();
                    return false;
                }
            }

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string err = "";
            branch = GIT.GitCurrentBranch(project, out err, MainWindow.Task.LogFileRelease);
            tbBranch.Text = branch;

            // Видимость полей и кнопок
            SetVisiblyForProject(project);

            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);

            if (branch.ToLower() != branchversion.ToLower())
            {
                App.AddLog("У проекта " + project + " ветка " + branch + " не соответствует номеру версии " + tbNumVersion.Text.Trim(), null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
         
                cbGITProject.Focus();
                
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Проверка на отсутствие merged в задачах
        /// </summary>
        /// <returns></returns>
        private bool CheckMerged()
        {
            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                return false;
            }

            string project = cbGITProject.SelectedItem.ToString().Trim();

            foreach (var info in MainWindow.Task.ReleaseYMLFiles
            .Where(x => x.IsAddRelease == true && x.PathInGIT.ToLower().StartsWith("cron"))
            .OrderBy(x => x.YMLOrder)
            )
            {
                string jsonfield = Utilities.GITProjects.GetYMLFieldByProject(project);
                string file = info.GetYMLFile(jsonfield);

                if (!string.IsNullOrWhiteSpace(file)) 
                {
                    string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
                    file = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, info.PathInGIT, file);

                    if (!File.Exists(file))
                    {
                        App.AddLog($"Файл {file} отсутствует!{Environment.NewLine}Возможно ветка {info.BranchName} не влита в версию {tbNumVersion.Text.Trim()}", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Нажата кнопка Переключиться на ветку
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGitChangeBranch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return;
            }

            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                {
                    MessageBox.Show("Выберите проект ГИТ");
                    cbGITProject.Focus();
                    return;
                }
            }

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);

            string err = "";
            string branch = GIT.GitCurrentBranch(project, out err, MainWindow.Task.LogFileRelease);
            string ask = "";
            string ask1 = "";

            if (branch.ToLower() != branchversion.ToLower())
            {
                ask = "Сейчас в проекте " + project + " текущая ветка " + branch + Environment.NewLine +
                                Environment.NewLine +
                                "Переключить в проекте " + project + " на ветку " + branchversion + " ?";
                ask1 = "Переключение в проекте " + project + " на ветку " + branchversion;
            }
            else
            {
                ask = "Сейчас в проекте " + project + " текущая ветка " + branch + Environment.NewLine +
                                Environment.NewLine +
                                "Обновить (git pull) ветку " + branchversion + " ?";
                ask1 = "Обновление (git pull) ветки " + branchversion;
            }

            if (System.Windows.Forms.MessageBox.Show(
                               ask,
                                "ВНИМАНИЕ",
                                System.Windows.Forms.MessageBoxButtons.YesNo
                            ) == System.Windows.Forms.DialogResult.Yes
                        )
            {
                App.AddLog(ask1, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFileRelease);

                // Переключиться на существующую ветку версии
                cbGITProject_Sync(branchversion, false);

                branch = GIT.GitCurrentBranch(project, out err, MainWindow.Task.LogFileRelease);
                tbBranch.Text = branch;

                if (branch.ToLower() != branchversion.ToLower())
                {
                    App.AddLog("В проекте " + project + " текущая ветка осталась " + branch, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
                }
                else
                {
                    btGitChangeBranch.IsEnabled = false;
                }

                string URL = Utilities.GITProjects.GetURLVersionByProject(project);
                URL = URL.Replace("%BRANCH%", tbBranch.Text);
                tbCronGITVersion.Text = URL + tbCronFileVersion.Text.Trim();
            }
        }

        /// <summary>
        /// Обновить текущий проект из GIT
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="isForcedGitRefresh"></param>
        private void GitPull_Sync(string branch, bool isForcedGitRefresh)
        {
            string project = cbGITProject.SelectedItem.ToString().Trim();

            GIT.GitPull(new string[] { project }, branch, false, true, false, MainWindow.Task.LogFileRelease, isForcedGitRefresh);

            dgJSONFilesRefresh();
        }

        /// <summary>
        /// Нажата кнопка GIT PULL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGitPull_Click(object sender, RoutedEventArgs e)
        {
            string project = cbGITProject.SelectedItem.ToString().Trim();

            // по номеру версии определить ветку
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);

            cbGITProject_Sync(branchversion, true);
        }

        /// <summary>
        /// Нажата кнопка GIT PUSH
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGitPush_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();

            if (System.Windows.Forms.MessageBox.Show($"Выполнить GIT PUSH для ветки {branch} в проекте {project} ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                // git push
                GIT.GitPush(new string[] { project }, branch, true, MainWindow.Task.LogFileRelease);
            }
        }

        /// <summary>
        /// Действия при выборе мышкой ячейки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgJSONFiles_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dgJSONFiles_SelectionChanged(sender, null);
        }

        /// <summary>Изменилось значение в поле "Имя файла версии"</summary>
        private void tbCronFileVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbGITProject.SelectedIndex != -1)
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();
                string URL = Utilities.GITProjects.GetURLVersionByProject(project);
                URL = URL.Replace("%BRANCH%", tbBranch.Text);
                tbCronGITVersion.Text = URL + tbCronFileVersion.Text.Trim();
            }
        }

        /// <summary>
        /// Двойной клик мыши на поле с url версии в GIT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbCronGITVersion_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(tbCronGITVersion.Text.Trim());
        }

        /// <summary>
        /// Двойной клик мыши на поле с url Deployment Plan в confluence
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbDeploymentConfVersion_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(tbDeploymentConfVersion.Text.Trim());
        }

        /// <summary>
        /// Нажата кнопка для копирования URL версии в буфер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCopyCronGITVersion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbCronGITVersion.Text.Trim());
            }
            catch (Exception ex)
            {
                App.AddLog($"Неизвестная ошибка при копировании в буфер:\n", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
            }
        }

        /// <summary>
        /// Нажата кнопка для копирования URL Deployment Plan в буфер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCopyDeploymentConfVersion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbDeploymentConfVersion.Text.Trim());
            }
            catch (Exception ex)
            {
                App.AddLog($"Неизвестная ошибка при копировании в буфер:\n", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
            }
        }

        /// <summary>
        /// Нажата кнопка Вверх
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btUp_Click(object sender, RoutedEventArgs e)
        {
            if (!dgJSONFiles.IsFocused) dgJSONFiles.Focus();

            if (dgJSONFiles.SelectedIndex >= 0)
            {
                // выбранная строка
                YMLFileInfo _json = dgJSONFiles.SelectedItem as YMLFileInfo;

                if (_json != null)
                {
                    var prev_json = MainWindow.Task.ReleaseYMLFiles
                        .Where(x => x.YMLOrder < _json.YMLOrder)
                        .OrderByDescending(x => x.YMLOrder)
                        .FirstOrDefault();

                    if (prev_json != null)
                    {
                        // есть предыдущая строка
                        var prev_order = prev_json.YMLOrder;
                        prev_json.YMLOrder = _json.YMLOrder;
                        _json.YMLOrder = prev_order;
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgJSONFilesRefresh();
            }
        }

        /// <summary>
        /// Нажата кнопка Вниз
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDown_Click(object sender, RoutedEventArgs e)
        {
            if (!dgJSONFiles.IsFocused) dgJSONFiles.Focus();

            if (dgJSONFiles.SelectedIndex >= 0)
            {
                // выбранная строка
                YMLFileInfo _json = dgJSONFiles.SelectedItem as YMLFileInfo;

                if (_json != null)
                {
                    var next_json = MainWindow.Task.ReleaseYMLFiles
                        .Where(x => x.YMLOrder > _json.YMLOrder)
                        .OrderBy(x => x.YMLOrder)
                        .FirstOrDefault();

                    if (next_json != null)
                    {
                        // есть предыдущая строка
                        var next_order = next_json.YMLOrder;
                        next_json.YMLOrder = _json.YMLOrder;
                        _json.YMLOrder = next_order;
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgJSONFilesRefresh();
            }
        }

        /// <summary>
        /// Нажата кнопка Редактировать задания
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCronEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string path_ver = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");

            WinCron WinCron = new WinCron();
            WinCron.mainWindow = mainWindow;
            WinCron.logFile = MainWindow.Task.LogFileRelease;
            WinCron.ProjectCronMS = this.ProjectCronMS;
            WinCron.ProjectCronPG = this.ProjectCronPG;
            WinCron.ProjectDefault = project;
            WinCron.BranchMS = tbBranch.Text;
            WinCron.BranchPG = tbBranch.Text;

            if (project == this.ProjectCronMS)
            {
                MainWindow.Task.JSONFilenameCronMS = tbCronGITVersion.Text;
            }
            else if (project == this.ProjectCronPG)
            {
                MainWindow.Task.JSONFilenameCronPG = tbCronGITVersion.Text;
            }
            else
            {
                return;
            }

            // редактируем
            WinCron.Show();
        }

        /// <summary>
        /// генерация json-файла заданий
        /// </summary>
        private void GenerateCron()
        {
            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            double numversion = Release.VerAsNum(tbNumVersion.Text);

            string dbregion_ver = "";
            if (project == this.ProjectCronMS)
            {
                dbregion_ver = "MS SQL";
            }
            else if (project == this.ProjectCronPG)
            {
                dbregion_ver = "PG SQL";
            }
            else
            {
                return;
            }

            // Ищем существующий файл версии
            if (Cron.FindVersion(project, tbNumVersion.Text, out string filename, MainWindow.Task.LogFileRelease))
            {
                tbCronFileVersion.Text = Path.GetFileName(filename);
            }

            // список заданий для версии
            ObservableCollection<Cron> ListEditCron = new ObservableCollection<Cron>();

            // добавляем задания из задач, включенных в релиз
            string jsonfield = Utilities.GITProjects.GetYMLFieldByProject(project);

            foreach (var json in MainWindow.Task.ReleaseYMLFiles
                                .Where(x =>
                                    x.IsAddRelease == true &&
                                    x.PathInGIT.ToLower().StartsWith("cron")
                                )
                                .OrderBy(x => x.YMLOrder))
            {
                string taskfile = json.GetYMLFile(jsonfield);

                if (!string.IsNullOrWhiteSpace(taskfile))
                {
                    // json-файл задачи
                    taskfile = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, json.PathInGIT, taskfile);

                    // добавляем json-файл задачи
                    Cron.LoadJSON(project, dbregion_ver, ListEditCron, taskfile, MainWindow.Task.LogFileRelease, false, true);
                }
            }

            // добавляем задания из ранее добавленных
            if (project == this.ProjectCronMS)
            {
                foreach (var item in MainWindow.Task.ListCronMS)
                {
                    // проверяем, может задание уже добавлено
                    var found = ListEditCron
                        .Where(x => x.IsKeyEqual(item))
                        .FirstOrDefault();

                    if (found == null)
                    {
                        Cron.LoadJSON(project, dbregion_ver, ListEditCron, item.Filepath, MainWindow.Task.LogFileRelease, false, true);
                    }
                }
            }
            else if (project == this.ProjectCronPG)
            {
                foreach (var item in MainWindow.Task.ListCronPG)
                {
                    // проверяем, может задание уже добавлено
                    var found = ListEditCron
                        .Where(x => x.IsKeyEqual(item))
                        .FirstOrDefault();

                    if (found == null)
                    {
                        Cron.LoadJSON(project, dbregion_ver, ListEditCron, item.Filepath, MainWindow.Task.LogFileRelease, false, true);
                    }
                }
            }

            // добавляем "общие" пункты, добавленные вручную при текущей сборке
            foreach (var item in MainWindow.Task.ListCronTemp.Where(x => x.dbregion == dbregion_ver))
            {
                // проверяем, может задание уже добавлено
                var found = ListEditCron
                    .Where(x => x.IsKeyEqual(item))
                    .FirstOrDefault();

                if (found == null)
                {
                    ListEditCron.Add(item);
                }
            }

            // сохраним списки
            if (project == this.ProjectCronMS)
            {
                MainWindow.Task.ListCronMS = ListEditCron;
            }
            else if (project == this.ProjectCronPG)
            {
                MainWindow.Task.ListCronPG = ListEditCron;
            }

            // сгенерировать и сохранить json-файл
            bool isSaved = WinCron.SaveJSON(project, dbregion_ver, tbBranch.Text, "version", tbCronFileVersion.Text, false, ListEditCron, true, out string jsonfile, out string jsonurl, MainWindow.Task.LogFileRelease, tbNumVersion.Text);

            if (isSaved)
            {
                tbCronFileVersion.Text = Path.GetFileName(jsonfile);

                if (project == this.ProjectCronMS)
                {
                    MainWindow.Task.JSONFilenameCronMS = jsonurl;
                }
                else if (project == this.ProjectCronPG)
                {
                    MainWindow.Task.JSONFilenameCronPG = jsonurl;
                }

                // завершение            
                if (System.Windows.Forms.MessageBox.Show($"Открыть файл {tbCronFileVersion.Text} ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    App.AddLog($"Для версии {tbNumVersion.Text} в проекте {project} собран файл {tbCronFileVersion.Text}, открытие внешнего файла", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFileRelease);

                    Utilities.External.OpenExternalFile(jsonfile);
                }
                else
                {
                    App.AddLog($"Для версии {tbNumVersion.Text} в проекте {project} собран файл {tbCronFileVersion.Text}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFileRelease);
                }
            }
        }

        /// <summary>Нажата кнопка Сгенерировать json-файл</summary>
        private void btGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;
            if (!CheckMerged()) return;

            if (System.Windows.Forms.MessageBox.Show($"Собрать задания для версии {tbNumVersion.Text} в файл {tbCronFileVersion.Text}?",
            "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                GenerateCron();
            }
        }

        /// <summary>
        /// Заполнить Versions 
        /// </summary>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        public void FillVersions(bool isForcedGitRefresh)
        {
            string project = cbGITProject.SelectedItem.ToString().Trim();
            if (Utilities.GITProjects.IsDEVProject(project))
            {
                // Очистить Versions
                Versions.Clear();

                // Очистить NextVersions
                NextVersions.Clear();

                // текущая версия
                string currentbranch = tbBranch.Text;

                // обновить проект GIT
                GitPull_Sync(currentbranch, isForcedGitRefresh);

                // заполнить Versions и NextVersions
                WinRelease.FillVersionsInDev(project, ref currentbranch, tbNumVersion.Text, "", Versions, NextVersions, MainWindow.Task.LogFileRelease, false);

                if (!string.IsNullOrWhiteSpace(currentbranch))
                {
                    tbBranch.Text = currentbranch;
                }
            }
        }

        /// <summary>
        /// Нажата кнопка Влить дальше
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btMergeNextVersion_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string cur_branch)) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();
            if (!Utilities.GITProjects.IsDEVProject(project)) return;

            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string branch = tbBranch.Text;
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, branch));

            // Заполним Versions
            FillVersions(false);

            if (
                Versions == null ||
                Versions.Count == 0 ||
                Versions[numversion] == null ||
                Versions[numversion].YMLFile == null
            )
            {
                MessageBox.Show($"Список версий пуст или не полный");
                return;
            }

            if (!Versions[numversion].YMLFile.IsFileExist)
            {
                App.AddLog($"Для версии {tbNumVersion.Text} в проекте {project} НЕ собран основной yml-файл версии", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);

                return;
            }

            bool isNoCumulative = Versions[numversion].YMLFile.IsNoCumulative;

            if (!Versions
                .Where(x =>
                    (x.Value != null) &&
                    (x.Value.YMLFile != null) &&
                    (x.Value.NumOrder > numversion) && // следующие версии
                    (x.Value.PrevNumOrder > 0) && // у которых есть ссылка на предыдущую версию
                    (x.Value.PrevNumOrder == numversion) // ссылаются на текущую ветку
                ).Any()
            )
            {
                MessageBox.Show($"После версии {branch} нет последующих версий, в которые ее можно влить");
                return;
            }

            // нашли минимум одну версию после СЛЕДУЮЩЕЙ
            if (System.Windows.Forms.MessageBox.Show($"Вольем в проекте {project} ветку {branch} во все последующие ветки ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                // вливаем по все последующие после СЛЕДУЮЩЕЙ
                GIT.GitMergeNextVersion(project, branch, isNoCumulative, Versions, MainWindow.Task.LogFileRelease);

                // ----------------------------------------------------------------------------
                // показываем лог
                if (
                    (System.Windows.Forms.MessageBox.Show("Посмотреть итоговый лог ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                )
                {
                    WinInfo WinInfo = new WinInfo(MainWindow.Task.LogFileRelease);
                    WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                    WinInfo.tbInfo.Text = File.ReadAllText(MainWindow.Task.LogFileRelease);
                    WinInfo.Title = "Лог в файле " + MainWindow.Task.LogFileRelease;
                    WinInfo.Show();
                }

                // проверка текущей ветки и видимость кнопок и полей
                CheckBranch(out branch);
            }
        }
    }
}
