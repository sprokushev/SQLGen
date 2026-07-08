// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Highlighting;
using SQLGen.Controls;
using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Path = System.IO.Path;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>
    /// Окно Формирование Deployment Plan релизной версии
    /// </summary>
    public partial class WinReleaseDeployment
    {
        /// <summary>ссылка на экземпляр основного окна программы</summary>
        public MainWindow mainWindow;

        /// <summary>
        /// фильтрация задач
        /// </summary>
        private CollectionViewSource _filteredJSON;

        /// <summary>
        /// Проект для хранения действий при обновлении MS 
        /// </summary>
        public string ProjectDeploymentMS = null;

        /// <summary>
        /// Проект для хранения действий при обновлении PG 
        /// </summary>
        public string ProjectDeploymentPG = null;

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

        /// <summary>Список ВСЕХ версий (с учетом кумулятивности)</summary>
        public SortedDictionary<double, Version> Versions = new SortedDictionary<double, Version>();

        /// <summary>Список СЛЕДУЮЩИХ версий (с учетом кумулятивности)</summary>
        public SortedDictionary<double, Version> NextVersions = new SortedDictionary<double, Version>();

        /// <summary>
        /// признак кумулятивности текущей версии
        /// </summary>
        bool isCumulative = true;

        /// <summary>Следующая версия</summary>
        string lastNextVersion = "";

        /// <summary>Конструктор WinReleaseDeployment</summary>
        public WinReleaseDeployment()
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
            Default.InitGUI("WinReleaseDeployment", this, mainGrid, null, null, null, MainWindow.Task.LogFileRelease);
        }

        /// <summary>При открытии окна WinReleaseDeployment</summary>
        private void winReleaseDeployment_Activated(object sender, EventArgs e)
        {
            this.Title = "Сборка Deployment Plan релиза " + MainWindow.Task.ReleaseVersion + ", задача " + MainWindow.Task.TaskNumber;

            dgJSONFiles.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            dgJSONFilesRefresh();

            tbNumVersion.Text = MainWindow.Task.ReleaseVersion;
            tbReleaseTaskNumber.Text = MainWindow.Task.TaskNumber;

            // url на Deployment Plan в Confluence
            tbDeploymentConfVersion.Text = GITProjects.GetURLDeploymentPlan(tbNumVersion.Text);
        }

        /// <summary>При закрытии окна WinReleaseDeployment</summary>
        private void winReleaseDeployment_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinReleaseDeployment", this, null);

            // сохранить текущую задачу
            MainWindow.SaveTask(MainWindow.Task, true);
        }

        /// <summary>
        /// Очистить поля на форме
        /// </summary>
        private void ClearFields()
        {
            Versions.Clear();
            NextVersions.Clear();

            cbPrevVersion.Items.Clear();
            cbNextVersion.Items.Clear();
            cbPrevVersion.Items.Add("");
            cbNextVersion.Items.Add("");

            tbBranch.Text = "";
            cbPrevVersion.Text = "";
            tbDeploymentFileVersion.Text = "";
            tbDeploymentGITVersion.Text = "";
            cbNextVersion.Text = "";

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

            tbDeploymentFileVersion.IsEnabled = true;
            //tbDeploymentFileVersion.IsReadOnly = false;

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

            // Заполним Versions
            FillVersions(false, true);

            // текущая yml-версия
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));

            YMLStruct loadyml = new YMLStruct(null, MainWindow.Task.LogFileRelease);

            if (Versions.ContainsKey(numversion))
            {
                loadyml = Versions[numversion].YMLFile;
            }

            // Найдем среди версий json-файл с Действиями при обновлении
            bool isexist = false;
            tbDeploymentFileVersion.Text = Release.GetJsonFile(project, tbNumVersion.Text, "deployment", out isexist);
            tbDeploymentFileVersion.IsReadOnly = isexist;

            //проставляем флаг кумулятивности
            isCumulative = loadyml.IsCumulative;

            // определяем предыдущую версию
            string PrevVersion = loadyml.FirstPrevVersion?.NumVersionLine;

            string DEVStartVer = Utilities.GITProjects.GetDEVStartVerByProject(project);

            // выбираем предыдущую версию
            var ver = WinRelease.GetPrevVersion(project, path, tbDeploymentFileVersion.Text.Trim(), Release.GetNumVersion(prefix, tbNumVersion.Text), PrevVersion, Versions, false);
            if (
                  (ver != null) &&
                  (numversion >= Release.VerAsNum(DEVStartVer))
            )
            {
                cbPrevVersion.SelectedItem = ver.VisibleName;
            }
            else
            {
                cbPrevVersion.SelectedIndex = -1;
            }

            // выбираем следующую версию
            ver = WinRelease.GetNextVersion(project, path, tbDeploymentFileVersion.Text.Trim(), Release.GetNumVersion(prefix, tbNumVersion.Text), NextVersions, false);
            if (
                  (ver != null) &&
                  (numversion >= Release.VerAsNum(DEVStartVer))
            )
            {
                cbNextVersion.SelectedItem = ver.VisibleName;
                // сохраняем выбранную следующую версию
                lastNextVersion = ver.VisibleName;
            }
            else
            {
                cbNextVersion.SelectedIndex = -1;
                // сохраняем выбранную следующую версию
                lastNextVersion = "";
            }

            // url на версию в GIT
            URL = URL.Replace("%BRANCH%", tbBranch.Text);
            tbDeploymentGITVersion.Text = URL + tbDeploymentFileVersion.Text.Trim();

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
                return _json.PathInGIT.ToLower() == "deployment";
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
                BranchName.Visibility = System.Windows.Visibility.Visible;
            }

            foreach (var item in MainWindow.Task.ReleaseYMLFiles.Where(x => x.PathInGIT.ToLower() == "deployment"))
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
                foreach (var item in MainWindow.Task.ReleaseYMLFiles.Where(x => x.PathInGIT.ToLower() == "deployment"))
                {
                    JsonCount++;
                    if (item.IsAddRelease)
                    {
                        JsonCountRelease++;
                    }
                }

                TaskCount = MainWindow.Task.ReleaseYMLFiles.Where(x => x.PathInGIT.ToLower() == "deployment")
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
                Utilities.MSOffice.GenerateExcel(Utilities.Databases.ConvertToDataTable(MainWindow.Task.ReleaseYMLFiles
                    .Where(x => x.PathInGIT.ToLower() == "deployment")
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
                MainWindow.SaveTask(MainWindow.Task, false);
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
            if (!isCorrectPrevVersion(out string err))
            {
                cbPrevVersion.Focus();
                return;
            }

            string GITProject = cbGITProject.SelectedItem.ToString().Trim();

            // Проверяем, убрали следующую версию ?
            if (
                (!string.IsNullOrWhiteSpace(lastNextVersion)) &&
                (
                    (cbNextVersion.SelectedIndex == -1) ||
                    (cbNextVersion.SelectedItem == null) ||
                    string.IsNullOrWhiteSpace(cbNextVersion.SelectedItem.ToString())
                )
            )
            {
                App.AddLog($"У текущей версии {tbNumVersion.Text} была убрана следующая версия. Необходимо вручную поменять ссылку на предыдущую версию в версии {lastNextVersion}", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);

                lastNextVersion = "";
            }

            // Вливаем в следующую версию
            if (
                (cbNextVersion.SelectedIndex != -1) &&
                (cbNextVersion.SelectedItem != null) &&
                (!string.IsNullOrWhiteSpace(cbNextVersion.SelectedItem.ToString())) 
                //(cbNextVersion.SelectedItem.ToString() != lastNextVersion)
            )
            {
                SetNextVersion(true);

                lastNextVersion = cbNextVersion.SelectedItem.ToString();
            }
            else
            {
                // Отправляем в GIT
                GIT.GitAdd(new string[] { GITProject }, tbBranch.Text.Trim(), true, false, MainWindow.Task.LogFileRelease);
            }

            // сохранить текущую задачу
            MainWindow.SaveTask(MainWindow.Task, true);
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
            .Where(x => x.IsAddRelease == true && x.PathInGIT.ToLower() == "deployment")
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
        /// Проверяем корректность выбора\смены предыдущей версии, с учетом кумулятивности текущей версии
        /// </summary>
        /// <param name="error">текст ошибки</param>
        /// <returns></returns>
        private bool isCorrectPrevVersion(out string error)
        {
            error = "";

            if (
                // Если выбрана предыдущая ветка
                (cbPrevVersion.SelectedIndex != -1) &&
                !string.IsNullOrWhiteSpace(cbPrevVersion.SelectedItem.ToString())
                )
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();
                string prevbranch = cbPrevVersion.SelectedItem.ToString().Trim();
                double prevnumversion = Release.VerAsNum(prevbranch);

                if (
                        (!Versions.ContainsKey(prevnumversion)) ||
                        (!Versions[prevnumversion].isBranchExists)
                )
                {
                    error = $"ОШИБКА: Ветка {prevbranch} не найдена в проекте {project}";

                    App.AddLog(error, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);

                    return false;
                }

                Version prever = Versions[prevnumversion];

                if (
                    isCumulative == true &&
                    prever.isNoCumulative
                )
                {
                    error = $"ОШИБКА: Версия {prevbranch} в проекте {project} НЕ кумулятивная и не может быть предыдущей версией у КУМУЛЯТИВНОЙ версии {tbNumVersion.Text}";

                    App.AddLog(error, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);

                    return false;
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
                tbDeploymentGITVersion.Text = URL + tbDeploymentFileVersion.Text.Trim();
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

                // сохранить текущую задачу
                MainWindow.SaveTask(MainWindow.Task, true);
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
        private void tbDeploymentFileVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (cbGITProject.SelectedIndex != -1)
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();
                string URL = Utilities.GITProjects.GetURLVersionByProject(project);
                URL = URL.Replace("%BRANCH%", tbBranch.Text);
                tbDeploymentGITVersion.Text = URL + tbDeploymentFileVersion.Text.Trim();
            }
        }

        /// <summary>
        /// Двойной клик мыши на поле с url версии в GIT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbDeploymentGITVersion_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start(tbDeploymentGITVersion.Text.Trim());
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
        private void btCopyDeploymentGITVersion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbDeploymentGITVersion.Text.Trim());
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
                MainWindow.SaveTask(MainWindow.Task, false);

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
                MainWindow.SaveTask(MainWindow.Task, false);

                dgJSONFilesRefresh();
            }
        }

        /// <summary>
        /// Нажата кнопка Редактировать Deployment Plan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDeploymentEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string path_ver = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);

            WinDeployment WinDeployment = new WinDeployment();
            WinDeployment.mainWindow = mainWindow;
            WinDeployment.logFile = MainWindow.Task.LogFileRelease;
            WinDeployment.prevversion = "";

            if (
                (cbPrevVersion.SelectedIndex != -1) &&
                !string.IsNullOrWhiteSpace(cbPrevVersion.SelectedItem.ToString())
            )
            {
                WinDeployment.prevversion = cbPrevVersion.SelectedItem.ToString().Trim();
            }

            WinDeployment.ProjectDeploymentMS = this.ProjectDeploymentMS;
            WinDeployment.ProjectDeploymentPG = this.ProjectDeploymentPG;
            WinDeployment.ProjectDefault = project;

            if (project == this.ProjectDeploymentMS)
            {
                MainWindow.Task.JSONFilenameDeploymentMS = tbDeploymentGITVersion.Text;
            }
            else if (project == this.ProjectDeploymentPG)
            {
                MainWindow.Task.JSONFilenameDeploymentPG = tbDeploymentGITVersion.Text;
            }
            else
            {
                return;
            }

            // редактируем
            WinDeployment.Show();
        }

        /// <summary>
        /// генерация json-файла Deployment Plan
        /// </summary>
        private void GenerateDeployment()
        {
            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            double numversion = Release.VerAsNum(tbNumVersion.Text);

            // получаем номер предыдущей версии из yml-файла версии
            string prevver = "";

            if (
                (cbPrevVersion.SelectedIndex != -1) &&
                !string.IsNullOrWhiteSpace(cbPrevVersion.SelectedItem.ToString())
            )
            {
                prevver = cbPrevVersion.SelectedItem.ToString().Trim();
                string prevver_no_prefix = Release.GetNumVersion(prefix, prevver);
                double prevnum = Release.VerAsNum(prevver_no_prefix);

                if (
                    (prevnum == 0) ||
                    !Versions.ContainsKey(prevnum)
                )
                {
                    App.AddLog($"yml-файл версии для ветки {prevver} не найден!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
                    cbPrevVersion.Focus();
                    return;
                }
            }

            string dbregion_ver = "";
            if (project == this.ProjectDeploymentMS)
            {
                dbregion_ver = "MS SQL";
            }
            else if (project == this.ProjectDeploymentPG)
            {
                dbregion_ver = "PG SQL";
            }
            else
            {
                return;
            }

            // список действий при обновлении версии
            ObservableCollection<Deployment> ListDeployment = new ObservableCollection<Deployment>();

            // Ищем существующий файл версии и загружаем его, если он существует
            Deployment.LoadVersion(project, tbNumVersion.Text, ListDeployment, out string filename, MainWindow.Task.LogFileRelease, out bool isFound, out bool isError);

            if (isError)
            {
                return;
            }

            if (isFound)
            {
                tbDeploymentFileVersion.Text = Path.GetFileName(filename);
            }

            // копируем "общие" пункты, добавленные ранее при текущей сборке
            foreach (var item in MainWindow.Task.ListDeploymentTemp.Where(x => x.dbregion == dbregion_ver))
            {
                // проверяем, может действие уже добавлено
                var found = ListDeployment
                    .Where(x =>
                        x.dbregion == item.dbregion &&
                        x.position == item.position &&
                        x.type == item.type &&
                        (x.script??"").TrimStartAllSpace().TrimEndAllSpace() == (item.script??"").TrimStartAllSpace().TrimEndAllSpace()
                    ).FirstOrDefault();

                if (found == null)
                {
                    // если еще НЕ добавлено - добавляем
                    ListDeployment.Add(item);
                }

            }

            // определяем список проектов основных версий
            List<string> projects_in_ver = new List<string>();

            foreach (var item in MainWindow.Task.ReleaseYMLFiles
                .Where(x =>
                    x.IsAddRelease == true &&
                    x.PathInGIT.ToLower() == "task"
                )
            )
            {
                foreach (var _proj in item.ListProjectsByDBRegion(dbregion_ver))
                {
                    if (
                        this.ProjectDeploymentMS == GITProjects.GetProjectDeployment("MS SQL", _proj) ||
                        this.ProjectDeploymentPG == GITProjects.GetProjectDeployment("PG SQL", _proj)
                     )
                    {
                        if (!projects_in_ver.Contains(_proj))
                        {
                            projects_in_ver.Add(_proj);
                        }
                    }
                }
            }

            // добавляем основные проекты
            if (dbregion_ver == "PG SQL" && !string.IsNullOrWhiteSpace(this.ProjectDeploymentPG))
            {
                if (!projects_in_ver.Contains(this.ProjectDeploymentPG))
                {
                    projects_in_ver.Add(this.ProjectDeploymentPG);
                }

                if (this.ProjectDeploymentPG == "dev_promed_pg")
                {
                    // добавляем EMD
                    if (!projects_in_ver.Contains("dev_emd_pg"))
                    {
                        projects_in_ver.Add("dev_emd_pg");
                    }

                    // добавляем проекты отчетников
                    if (!projects_in_ver.Contains("liquibase_project_new"))
                    {
                        projects_in_ver.Add("liquibase_project_new");
                    }
                }
            }

            if (dbregion_ver == "MS SQL" && !string.IsNullOrWhiteSpace(this.ProjectDeploymentMS))
            {
                if (!projects_in_ver.Contains(this.ProjectDeploymentMS))
                {
                    projects_in_ver.Add(this.ProjectDeploymentMS);
                }

                if (this.ProjectDeploymentMS == "dev_promed_ms")
                {
                    // добавляем EMD
                    if (!projects_in_ver.Contains("dev_emd_pg"))
                    {
                        projects_in_ver.Add("dev_emd_pg");
                    }

                    // добавляем ЛИС
                    if (!projects_in_ver.Contains("dev_lis_pg"))
                    {
                        projects_in_ver.Add("dev_lis_pg");
                    }

                    // добавляем проекты отчетников
                    if (!projects_in_ver.Contains("msdbupdate_new"))
                    {
                        projects_in_ver.Add("msdbupdate_new");
                    }
                }
            }

            // git pull проектов
            GIT.GitPull(projects_in_ver.ToArray(), tbBranch.Text, false, false, false, MainWindow.Task.LogFileRelease, false);

            // перебираем проекты в нужном порядке, добавляем основные версии
            foreach (var project_ver in projects_in_ver
                .OrderBy(x =>
                {
                    string ord = "999";
                    if (x == "dev_promed_pg") ord = "001";
                    else if (x == "dev_promed_ms") ord = "002";
                    else if (x == "dev_lis_pg") ord = "003";
                    else if (x == "dev_emd_pg") ord = "004";
                    else if (x == "liquibase_project_new") ord = "005";
                    else if (x == "msdbupdate_new") ord = "006";
                    return ord + x;
                }
                )
            )
            {
                string folder_ver = Utilities.GITProjects.GetFolderByProject(project_ver);
                string path_ver = Path.Combine(MainWindow.APPinfo.GITFolder, folder_ver, "version");
                string prefix_ver = Utilities.GITProjects.GetPrefixFileReleaseByProject(project_ver);
                string dbalias_ver = Utilities.GITProjects.GetDBAliasByProject(project_ver);
                if (
                        project_ver == "liquibase_project_new" ||
                        project_ver == "msdbupdate_new"
                    )
                {
                    dbalias_ver = "promed_rpt";
                }

                string file_ver = "";

                // Ищем существующий yml-файл в папке version по номеру версии
                var files = Directory.GetFiles(path_ver, tbNumVersion.Text.Trim() + "_*.yml").ToList();
                if (files == null) files = new List<string>(); //-V3022
                files.AddRange(Directory.GetFiles(path_ver, tbNumVersion.Text.Trim() + ".yml").ToList());
                foreach (var file in files)
                {
                    if (file.ToLower().Contains("_ots"))
                    {
                        continue;
                    }

                    if (
                        file.ToLower().Contains("_rpt") &&
                        project_ver != "liquibase_project_new" &&
                        project_ver != "msdbupdate_new"
                    )
                    {
                        continue;
                    }

                    if (
                        numversion == Release.VerAsNum(Release.GetNumVersion(prefix, Path.GetFileName(file))) && //-V3024
                        prefix == prefix_ver
                    )
                    {
                        // нашли файл версии
                        file_ver = file;
                        break;
                    }
                }

                if (!string.IsNullOrEmpty(file_ver))
                {
                    string url_ver = Utilities.GITProjects.ConvertFilepathToUrl(project_ver, file_ver, tbBranch.Text, MainWindow.Task.LogFileRelease);

                    // определим order
                    int max = 0;
                    try
                    {
                        max = ListDeployment.Max(x => x.order);
                    }
                    catch
                    {
                    }
                    max++;

                    // создаем действие
                    var new_deployment = new Deployment();
                    new_deployment.SetDeployment(MainWindow.Task.TaskNumber, max, dbregion_ver, "primary", "liquibase", url_ver, "", dbalias_ver);

                    if (dbalias_ver == "lis")
                    {
                        new_deployment.regions.Clear();
                        new_deployment.regions.Add("Башкирия");
                    }

                    // проверяем, может действие уже добавлено
                    var found = ListDeployment
                        .Where(x =>
                            x.dbregion == new_deployment.dbregion &&
                            x.position == new_deployment.position &&
                            x.type == new_deployment.type &&
                            (x.script ?? "").TrimStartAllSpace().TrimEndAllSpace() == (new_deployment.script??"").TrimStartAllSpace().TrimEndAllSpace()
                        ).FirstOrDefault();

                    if (found == null)
                    {
                        // если еще НЕ добавлено - добавляем
                        ListDeployment.Add(new_deployment);
                    }

                    // обновим order в Deployment Plan
                    Deployment.LoadJSON(dbregion_ver, ListDeployment, null, MainWindow.Task.LogFileRelease, true, true);
                }
                else 
                {
                    if (
                        project_ver == "liquibase_project_new" ||
                        project_ver == "msdbupdate_new"
                    )
                    {
                        App.AddLog($"В проекте {project_ver} отсутствует версия ОТЧЕТНИКОВ для {tbNumVersion.Text}. Возможно ее и не должно быть, но прошу обратить внимание, что сейчас в Deployment Plan эта версия автоматически не будет добавлена! Выполнение продолжается...", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
                    }
                    else
                    {
                        App.AddLog($"В проекте {project_ver} отсутствует собранный yml-файл для версии {tbNumVersion.Text}. Возможно, что ее и не должно быть, но прошу обратить внимание, что сейчас в Deployment Plan эта версия автоматически не будет добавлена! Выполнение продолжается...", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
                    }
                }
            }

            // добавляем действия из задач, включенных в релиз
            string jsonfield = Utilities.GITProjects.GetYMLFieldByProject(project);

            foreach (var json in MainWindow.Task.ReleaseYMLFiles
                                .Where(x =>
                                    x.IsAddRelease == true &&
                                    x.PathInGIT.ToLower() == "deployment"
                                )
                                .OrderBy(x => x.YMLOrder))
            {
                string taskfile = json.GetYMLFile(jsonfield);

                if (!string.IsNullOrWhiteSpace(taskfile))
                {
                    // json-файл задачи
                    taskfile = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, json.PathInGIT, taskfile);

                    // добавляем json-файл задачи
                    Deployment.LoadJSON(dbregion_ver, ListDeployment, taskfile, MainWindow.Task.LogFileRelease, false, true);
                }
            }

            // сгенерировать текст json-файла
            string jsonString = Deployment.GenerateJSON(ListDeployment, tbBranch.Text, MainWindow.Task.LogFileRelease, tbNumVersion.Text, prevver);

            // сохранить json-файл
            bool isSaved = WinDeployment.SaveJSON(project, tbBranch.Text, "version", tbDeploymentFileVersion.Text, false, false, ListDeployment, ref jsonString, true, out string jsonfile, out string jsonurl, MainWindow.Task.LogFileRelease, tbNumVersion.Text, prevver);

            if (isSaved)
            {
                tbDeploymentFileVersion.Text = Path.GetFileName(jsonfile);

                if (project == this.ProjectDeploymentMS)
                {
                    MainWindow.Task.JSONFilenameDeploymentMS = jsonurl;
                    MainWindow.Task.ListDeploymentMS = ListDeployment;
                }
                else if (project == this.ProjectDeploymentPG)
                {
                    MainWindow.Task.JSONFilenameDeploymentPG = jsonurl;
                    MainWindow.Task.ListDeploymentPG = ListDeployment;
                }

                // завершение            
                if (System.Windows.Forms.MessageBox.Show($"Редактировать файл {tbDeploymentFileVersion.Text} ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    /*App.AddLog($"Для версии {tbNumVersion.Text} в проекте {project} собран файл {tbDeploymentFileVersion.Text}, открытие внешнего файла", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFileRelease);

                    Utilities.External.OpenExternalFile(jsonfile);*/

                    btDeploymentEdit_Click(null, null);
                }

                App.AddLog($"Для версии {tbNumVersion.Text} в проекте {project} собран файл {tbDeploymentFileVersion.Text}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFileRelease);
            }
        }

        /// <summary>Нажата кнопка Сгенерировать json-файл</summary>
        private void btGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;
            if (!CheckMerged()) return;
            if (!isCorrectPrevVersion(out string err))
            {
                cbPrevVersion.Focus();
                return;
            }

            if (System.Windows.Forms.MessageBox.Show($"Собрать Deployment Plan для версии {tbNumVersion.Text} в файл {tbDeploymentFileVersion.Text}?",
            "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                GenerateDeployment();
            }
        }

        /// <summary>
        /// Заполнить Versions 
        /// </summary>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        /// <param name="isLoadNextVersions">=true - заполнить NextVersions</param>
        public void FillVersions(bool isForcedGitRefresh, bool isLoadNextVersions)
        {
            string project = cbGITProject.SelectedItem.ToString().Trim();

            // Очистить Versions
            Versions.Clear();

            // Очистить NextVersions
            NextVersions.Clear();

            var oldPrevIndex = cbPrevVersion.SelectedIndex;
            var oldNextIndex = cbNextVersion.SelectedIndex;
            string oldPrev = "";
            string oldNext = "";
            if (oldPrevIndex != -1) oldPrev = cbPrevVersion.SelectedItem.ToString().Trim();
            if (oldNextIndex != -1) oldNext = cbNextVersion.SelectedItem.ToString().Trim();
            cbPrevVersion.Items.Clear();
            cbNextVersion.Items.Clear();
            cbPrevVersion.Items.Add("");
            cbNextVersion.Items.Add("");

            // текущая версия
            string currentbranch = tbBranch.Text;
            double numversion = Release.VerAsNum(currentbranch);

            // обновить проект GIT
            GitPull_Sync(currentbranch, isForcedGitRefresh);

            // заполнить Versions и NextVersions
            WinRelease.FillVersionsInDev(project, ref currentbranch, tbNumVersion.Text, "", null, Versions, NextVersions, MainWindow.Task.LogFileRelease, false, isLoadNextVersions);

            if (!string.IsNullOrWhiteSpace(currentbranch))
            {
                tbBranch.Text = currentbranch;
            }

            // заполнить cbPrevVersion (в обратном порядке)
            foreach (var item in Versions
                .Where(x => x.Key < numversion)
                .OrderByDescending(x => x.Key)
                )
            {
                cbPrevVersion.Items.Add(item.Value.VisibleName);
            }

            // заполнить cbNextVersion (в прямом порядке)
            foreach (var item in NextVersions
                .Where(x => x.Key > numversion)
                .OrderBy(x => x.Key)
                )
            {
                cbNextVersion.Items.Add(item.Value.VisibleName);
            }

            if (oldPrevIndex != -1) cbPrevVersion.SelectedItem = oldPrev;
            if (oldNextIndex != -1) cbNextVersion.SelectedItem = oldNext;
        }

        /// <summary>
        /// Нажата кнопка Влить дальше
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btMergeNextVersion_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string cur_branch)) return;
            if (!isCorrectPrevVersion(out string err))
            {
                cbPrevVersion.Focus();
                return;
            }

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string branch = tbBranch.Text;
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, branch));

            // Проверяем, убрали СЛЕДУЮЩУЮ версию ?
            if (
                (!string.IsNullOrWhiteSpace(lastNextVersion)) &&
                (
                    (cbNextVersion.SelectedIndex == -1) ||
                    (cbNextVersion.SelectedItem == null) ||
                    string.IsNullOrWhiteSpace(cbNextVersion.SelectedItem.ToString())
                )
            )
            {
                App.AddLog($"У текущей версии {tbNumVersion.Text} была убрана следующая версия. Необходимо вручную поменять ссылку на предыдущую версию в версии {lastNextVersion}", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);

                lastNextVersion = "";
            }

            if (
                (cbNextVersion.SelectedIndex != -1) &&
                (cbNextVersion.SelectedItem != null) &&
                (!string.IsNullOrWhiteSpace(cbNextVersion.SelectedItem.ToString())) 
            )
            {
                lastNextVersion = cbNextVersion.SelectedItem.ToString();
                double nextversion = Release.VerAsNum(Release.GetNumVersion(prefix, lastNextVersion));

                // Сначала вливаем в СЛЕДУЮЩУЮ, правим ссылку на ТЕКУЩУЮ
                SetNextVersion(false);

                // Теперь вливаем дальше
                if (
                    Versions == null ||
                    Versions.Count == 0 ||
                    Versions[numversion] == null ||
                    Versions[numversion].YMLFile == null ||
                    Versions[nextversion] == null ||
                    Versions[nextversion].YMLFile == null
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
                        (x.Value.NumOrder > numversion) && // есть версии после ТЕКУЩЕЙ
                        (x.Value.PrevNumOrder > 0) && // у которых есть ссылка на предыдущую версию
                        (x.Value.PrevNumOrder == numversion) // ссылаются на ТЕКУЩУЮ
                    ).Any()
                )
                {
                    MessageBox.Show($"После версии {branch} нет последующих версий, в которые ее можно влить");
                    return;
                }

                // нашли минимум одну версию после ТЕКУЩЕЙ
                if (System.Windows.Forms.MessageBox.Show($"Вольем в проекте {project} ветку {branch} во все последующие ветки ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    // вливаем во все последующие после ТЕКУЩЕЙ
                    GIT.GitMergeNextVersion(project, branch, isNoCumulative, Versions, MainWindow.Task.LogFileRelease);

                    // ----------------------------------------------------------------------------
                    // показываем лог
                    if (System.Windows.Forms.MessageBox.Show("Посмотреть итоговый лог ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
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

        /// <summary>
        /// Нажата кнопка "Влить в dev"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btToDEV_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string cur_branch)) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string branch = tbBranch.Text;
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, branch));

            // Заполним Versions
            // FillVersions(false, false);

            if (
                Versions == null ||
                Versions.Count == 0 ||
                Versions[numversion] == null ||
                Versions[numversion].YMLFile == null
            )
            {
                MessageBox.Show("Список версий пуст или не полный");
                return;
            }

            bool isNoCumulative = Versions[numversion].YMLFile.IsNoCumulative;

            if (isNoCumulative) return;

            if (System.Windows.Forms.MessageBox.Show($"Вольем в проекте {project} ветку {branch} во ветку dev ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                // проверим, нужен ли commit в текущую ветку
                if (!GIT.CheckCommit(project, MainWindow.Task.LogFileRelease, $"Merge ветки {branch} в ветку dev прерван"))
                {
                    return;
                }

                // переключение на ветку dev
                if (!GIT.GitSwitch(project, "dev", MainWindow.Task.LogFileRelease, out string currentbranch, out string err))
                {
                    App.AddLog($"Не получилось переключиться на ветку dev в проекте {project} или при переключении на ветку dev возникла ошибка: {err}\n\nMerge ветки {branch} в ветку dev прерван\n\nТекущая ветка - {currentbranch}", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);

                    return;
                }

                // merge
                if (GIT.GitMerge(project, branch, "dev", true, false, MainWindow.Task.LogFileRelease, false))
                {
                    // merge успешный, делаем push
                    App.AddLog($"Успешный merge ветки {branch} в проекте {project} в ветку dev\n\nВ проекте {project} текущая ветка - dev", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFileRelease);

                    if (System.Windows.Forms.MessageBox.Show($"Успешный merge ветки {branch} в проекте {project} в ветку dev\n\nВ проекте {project} текущая ветка - dev\n\nВыполнить push ветки dev ?",
                            "ВНИМАНИЕ",
                            System.Windows.Forms.MessageBoxButtons.YesNo
                        ) == System.Windows.Forms.DialogResult.Yes
                    )
                    {
                        // git pull
                        GIT.GitPull(new string[] { project }, "dev", false, false, true, MainWindow.Task.LogFileRelease, false);

                        // пересобрать cron.json в ветке dev
                        GIT.MakeCronJson(project);

                        // git push
                        GIT.GitPush(new string[] { project }, "dev", true, MainWindow.Task.LogFileRelease);

                        // возврат на ветку версии
                        if (!GIT.GitSwitch(project, branch, MainWindow.Task.LogFileRelease, out branch, out err))
                        {
                            App.AddLog($"Ветка {branch} в проекте {project} не существует или при переключении на ветку {branch} возникла ошибка: {err}\n\nТекущая ветка - {branch}", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);

                            return;
                        }
                    }
                }
                else
                {
                    App.AddLog($"Merge ветки {branch} в проекте {project} в ветку dev НЕ был выполнен\n\nВ проекте {project} текущая ветка - dev", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
                }

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

        /// <summary>
        /// Поиск предыдущей версии
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void btFindPrevVersion_Click(object sender, RoutedEventArgs e)
        {
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

            FormFindInList dlg1 = new FormFindInList(MainWindow.Task.LogFileRelease);

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);

            // текущая версия
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));

            dlg1.AddItems(Versions
                .Where(x => x.Key < numversion)
                .OrderByDescending(x => x.Key)
                .Select(x => x.Value.VisibleName)
                .ToList());

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
                    cbPrevVersion.SelectedItem = result;
                }
            }

            dlg1.Dispose();
        }

        /// <summary>
        /// Поиск следующей версии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFindNextVersion_Click(object sender, RoutedEventArgs e)
        {
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
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));

            FormFindInList dlg1 = new FormFindInList(MainWindow.Task.LogFileRelease);

            dlg1.AddItems(NextVersions
                .Where(x => x.Key > numversion)
                .OrderBy(x => x.Key)
                .Select(x => x.Value.VisibleName)
                .ToList()
            );

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
                    cbNextVersion.SelectedItem = result;
                }
            }

            dlg1.Dispose();
        }

        /// <summary>
        /// Сменить в следующей версии ссылку на текущую, влить текущую версию в следующую
        /// </summary>
        /// <param name="askShowLog">=true - показывать лог</param>
        /// <returns></returns>
        private bool SetNextVersion(bool askShowLog)
        {
            if (!CheckBranch(out string branch)) return false;

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");

            if (string.IsNullOrWhiteSpace(tbDeploymentFileVersion.Text))
            {
                MessageBox.Show("Не заполнено имя файла версии!");
                return false;
            }

            string file_ver = Path.Combine(path, tbDeploymentFileVersion.Text);
            if (!File.Exists(file_ver))
            {
                MessageBox.Show($"Файл версии {file_ver} не существует, соберите версию!");
                return false;
            }

            if (
                (cbNextVersion.SelectedIndex == -1) ||
                string.IsNullOrWhiteSpace(cbNextVersion.SelectedItem.ToString())
            )
            {
                App.AddLog("Выберите СЛЕДУЮЩУЮ версию", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
                return false;
            }

            Encoding encoding = new UTF8Encoding(false);

            string version_no_prefix = Release.GetNumVersion(prefix, tbNumVersion.Text);

            double nextnum = 0;
            string nextbranch = "";
            string json_filepath = "";

            //string ask = "";
            //string ask2 = "";

            bool isOk = true;

            if (
                (cbNextVersion.SelectedIndex != -1) &&
                (!string.IsNullOrWhiteSpace(cbNextVersion.SelectedItem.ToString()))
                )
            {
                // в списке - имена веток версий
                nextbranch = cbNextVersion.SelectedItem.ToString().Trim();

                // номер следующей версии
                nextnum = Release.VerAsNum(Release.GetNumVersion(prefix, nextbranch));

                // проверим, нужен ли commit
                if (!GIT.CheckCommit(project, MainWindow.Task.LogFileRelease, "Изменение следующей версии прервано"))
                {
                    cbNextVersion.Focus();
                    return false;
                }

                // переключимся на ветку следующей версии
                if (!GIT.GitSwitch(project, nextbranch, MainWindow.Task.LogFileRelease, out string cur_branch, out string err))
                {
                    App.AddLog($"В проекте {project} не смогли перелючиться на ветку {nextbranch} !\nТекущая ветка {cur_branch}\n{err}", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);

                    cbNextVersion.Focus();
                    isOk = false;
                }

                if (isOk)
                {
                    // Ищем существующий файл СЛЕДУЮЩЕЙ версии в папке version по номеру
                    bool isFound = Deployment.FindVersion(project, nextbranch, out json_filepath, MainWindow.Task.LogFileRelease);

                    if (!isFound)
                    {
                        App.AddLog($"json-файл для версии {nextbranch} не найден!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
                        cbNextVersion.Focus();
                        isOk = false;
                    }

                    //ask = $"Влить ветку {branch} в ветку {nextbranch} и изменить в файле версии {json_filepath} ссылку на предыдущую версию на {branch} ?";
                    //ask2 = $"Ветка {branch} влита в {nextbranch} и в файле {json_filepath} ссылка на предыдущую версию изменена на {branch}";
                }

                if (isOk && (nextnum <= Release.VerAsNum(version_no_prefix)))
                {
                    App.AddLog("Выберите СЛЕДУЮЩУЮ версию", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
                    cbNextVersion.Focus();

                    isOk = false;
                }

                if (isOk)
                {
                    /*if (System.Windows.Forms.MessageBox.Show(ask, "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (isOk)
                        {*/
                    // merge
                    if (GIT.GitMerge(project, branch, nextbranch, true, false, MainWindow.Task.LogFileRelease, askShowLog))
                    {
                        // merge успешный, делаем push
                        GIT.GitPush(new string[] { project }, nextbranch, true, MainWindow.Task.LogFileRelease);
                    }

                    // заменить ссылку на предыдущую версию
                    Deployment.SetPrevVersion(project, nextbranch, branch, json_filepath, MainWindow.Task.LogFileRelease, out bool isError);

                    // проверим, нужен ли commit
                    if (!GIT.CheckCommit(project, MainWindow.Task.LogFileRelease, $"Ветка {branch} влита в {nextbranch} и в файле {json_filepath} ссылка на предыдущую версию изменена на {branch},\nно для ветки {nextbranch} требуется commit & push!"))
                    {
                        isOk = false;
                    }

                    if (isOk)
                    {
                        // сохраняем выбранную следующую версию
                        lastNextVersion = cbNextVersion.SelectedItem.ToString();

                        App.AddLog($"Ветка {branch} влита в {nextbranch} и в файле {json_filepath} ссылка на предыдущую версию изменена на {branch}", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);
                    }
                    /*}
                }*/
                }

                // переключимся на ветку версии
                if (!GIT.GitSwitch(project, branch, MainWindow.Task.LogFileRelease, out cur_branch, out err))
                {
                    App.AddLog($"Не смогли вернуться на ветку {branch} !\nТекущая ветка {cur_branch}", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileRelease);

                    cbNextVersion.Focus();
                    isOk = false; //-V3137
                }
            }

            return isOk;
        }
    }
}
