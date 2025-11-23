// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    // -------------------------------------------------------------------------------------------------------
    /// <summary>Типы операций с заданиями</summary>
    public enum CronOperType
    {
        /// <summary>
        /// выбрать существующее задание
        /// </summary>
        CHOOSE,
        /// <summary>
        /// добавить
        /// </summary>
        ADD,
        /// <summary>
        /// изменить
        /// </summary>
        EDIT,
        /// <summary>
        /// скопировать и сделать дубликат
        /// </summary>
        COPY
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Окно для сборки JSON-файла со списком заданий
    /// </summary>
    public partial class WinCron : Window
    {
        /// <summary>Флаг инициализации окна</summary>
        public bool isStart = true;

        /// <summary>ссылка на экземпляр основного окна программы</summary>
        public MainWindow mainWindow;

        /// <summary>
        /// лог-файл
        /// </summary>
        public string logFile;

        /// <summary>
        /// какой DataGrid сейчас выбран
        /// </summary>
        private DataGrid GridFocused;

        /// <summary>
        /// Отображать вкладку для сохранения json-файла для MS
        /// </summary>
        private bool isVisibleMSJson = true;

        /// <summary>
        /// Отображать вкладку для сохранения json-файла для PG
        /// </summary>
        private bool isVisiblePGJson = true;

        /// <summary>
        /// Проект для хранения заданий MS 
        /// </summary>
        public string ProjectCronMS = null;

        /// <summary>
        /// Проект для хранения заданий PG 
        /// </summary>
        public string ProjectCronPG = null;

        /// <summary>
        /// Проект по умолчанию
        /// </summary>
        public string ProjectDefault = null;

        /// <summary>
        /// ветка в проекте для MS
        /// </summary>
        public string BranchMS = "";

        /// <summary>
        /// ветка в проекте для PG
        /// </summary>
        public string BranchPG = "";

        /// <summary>Конструктор WinCron</summary>
        public WinCron()
        {
            InitializeComponent();

            isStart = true;

            // загрузить историю
            tbFilterMS.InitHistory("HistoryCronMS.json", "");
            tbFilterPG.InitHistory("HistoryCronPG.json", "");

            dgCronMS.ItemsSource = MainWindow.Task.ListCronMS;
            dgCronMSRefresh();

            dgCronPG.ItemsSource = MainWindow.Task.ListCronPG;
            dgCronPGRefresh();

            // пользовательские настройки GUI
            Default.InitGUI(
                "WinCron",
                this,
                mainGrid,
                null,
                null,
                null,
                MainWindow.Task.LogFile
                );
        }

        /// <summary>При открытии окна WinCron</summary>
        private void winCron_Activated(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MainWindow.Task.ReleaseVersion))
            {
                this.Title = $"Задания версии {MainWindow.Task.ReleaseVersion}, релизная задача {MainWindow.Task.TaskNumber}";
            }
            else
            {
                this.Title = $"Задания для задачи {MainWindow.Task.TaskNumber}";
            }

            isVisibleMSJson = !string.IsNullOrWhiteSpace(this.ProjectCronMS);
            isVisiblePGJson = !string.IsNullOrWhiteSpace(this.ProjectCronPG);

            if (!string.IsNullOrWhiteSpace(ProjectDefault))
            {
                isVisibleMSJson = (ProjectDefault == this.ProjectCronMS);
                isVisiblePGJson = (ProjectDefault == this.ProjectCronPG);
            }

            if (!isVisiblePGJson)
            {
                tiCronPG.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (isStart)
                {
                    tiCronPG.Focus();
                }
            }

            if (!isVisibleMSJson)
            {
                tiCronMS.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (isStart)
                {
                    tiCronMS.Focus();
                }
            }

            isStart = false;
        }

        /// <summary>При закрытии окна WinCron</summary>
        private void winCron_Closed(object sender, EventArgs e)
        {
            // Сохраним
            if (isVisibleMSJson)
            {
                Save("MS SQL", false);
            }

            if (isVisiblePGJson)
            {
                Save("PG SQL", false);
            }

            // пользовательские настройки GUI
            Default.SaveGUI(
                "WinCron",
                this,
                null
                );
        }

        /// <summary>
        /// фильтрация заданий
        /// </summary>
        /// <param name="de"></param>
        /// <returns></returns>
        public bool FilterContains(object de)
        {
            Cron cron = de as Cron;
            return (cron.isFiltered == true);
        }

        /// <summary>
        /// обновление списка заданий
        /// </summary>
        /// <param name="list">список</param>
        /// <param name="grid">grid</param>
        /// <param name="selectItem">действия после выбора</param>
        /// <param name="filterbox">поле для фильтра</param>
        public void CronRefresh(ObservableCollection<Cron> list, DataGrid grid, Action selectItem, HistoryTextBox filterbox)
        {
            // применяем фильтр
            string filter = filterbox.Text.Trim().ToLower();

            bool isFilteredAll = string.IsNullOrWhiteSpace(filter);

            foreach (Cron cron in list)
            {
                cron.isFiltered = isFilteredAll;

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var arr_filter = filter.Split(' ');

                    // фильтрация по элементам задания
                    for (int i = 0; i < arr_filter.Length; i++)
                    {
                        string s_filter = arr_filter[i].Trim();

                        if (
                            cron.task.ToLower().Contains(s_filter) ||
                            cron.application_name.ToLower().Contains(s_filter) ||
                            cron.comment.ToLower().Contains(s_filter) ||
                            cron.command.ToLower().Contains(s_filter) ||
                            cron.schedule.ToLower().Contains(s_filter) ||
                            cron.state.ToLower().Contains(s_filter) ||
                            cron.database.ToLower().Contains(s_filter) ||
                            cron.stage.ToLower().Contains(s_filter) ||
                            cron.regions_str.ToLower().Contains(s_filter) ||
                            cron.exclude_regions_str.ToLower().Contains(s_filter) ||
                            cron.hosts.ToLower().Contains(s_filter) ||
                            cron.check.ToLower().Contains(s_filter) ||
                            cron.team.ToLower().Contains(s_filter)
                        )
                        {
                            cron.isFiltered = true;
                        }
                    }
                }
            }

            // обновляем, сортируем, фильтруем
            ListCollectionView cvTasks = (ListCollectionView)CollectionViewSource.GetDefaultView(grid.ItemsSource);

            if (cvTasks != null)
            {
                if (cvTasks.IsAddingNew) cvTasks.CommitNew();
                if (cvTasks.IsEditingItem) cvTasks.CommitEdit();
            }

            if (cvTasks != null && cvTasks.CanFilter == true)
            {
                cvTasks.Filter = new Predicate<object>(FilterContains);
            }


            if (cvTasks != null && cvTasks.CanSort == true)
            {
                cvTasks.SortDescriptions.Clear();
                cvTasks.SortDescriptions.Add(new SortDescription("order", ListSortDirection.Ascending));
            }

            grid.Items.Refresh();
            selectItem();
        }

        /// <summary>Обновить список dgCronMS</summary>
        public void dgCronMSRefresh()
        {
            CronRefresh(MainWindow.Task.ListCronMS, dgCronMS, dgCronMSSelect, tbFilterMS);
        }

        /// <summary>Обновить список dgCronPG</summary>
        public void dgCronPGRefresh()
        {
            CronRefresh(MainWindow.Task.ListCronPG, dgCronPG, dgCronPGSelect, tbFilterPG);
        }

        /// <summary>Выбрана запись в dgCronMS </summary>
        private void dgCronMSSelect()
        {
            tiCronMS_GotFocus(null, null);
        }

        /// <summary>Выбрана запись в dgCronPG </summary>
        private void dgCronPGSelect()
        {
            tiCronPG_GotFocus(null, null);
        }

        /// <summary>Выбрана запись в dgCronMS</summary>
        private void dgCronMS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgCronMSSelect();
        }

        /// <summary>Выбрана запись в dgCronPG</summary>
        private void dgCronPG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgCronPGSelect();
        }

        /// <summary>
        /// Добавить в ListTemp
        /// </summary>
        /// <param name="_cron"></param>
        private void AddListTemp(Cron _cron)
        {
            if (_cron == null) return;

            string _application_name = _cron.application_name;

            Cron found = null;

            // сначала удалим все существующие
            do
            {
                found = MainWindow.Task.ListCronTemp.Where(x => x.IsKeyEqual(_cron)).FirstOrDefault();
                if (found != null)
                {
                    MainWindow.Task.ListCronTemp.Remove(found);
                }
                else
                {
                    break;
                }

            } while (true);

            // добавим новую
            MainWindow.Task.ListCronTemp.Add(_cron);
        }

        /// <summary>
        /// Добавить или изменить запись в Cron
        /// </summary>
        /// <param name="oper">операция</param>
        /// <param name="cronMS">для MS</param>
        /// <param name="cronPG">для PG</param>
        /// <returns></returns>
        private bool AddEditCron(CronOperType oper, ref Cron cronMS, ref Cron cronPG)
        {
            bool result = false;

            string _dbregion = null;

            if (GridFocused != null)
            {
                if (GridFocused.Name == "dgCronPG")
                {
                    _dbregion = "PG SQL";
                }
                else
                {
                    _dbregion = "MS SQL";
                }
            }
            else
            {
                return result;
            }

            Cron new_cron = null;
            //int current = 1;

            if (_dbregion == "MS SQL")
            {
                if (cronMS != null)
                {
                    //current = cronMS.order;

                    if (oper == CronOperType.COPY || oper == CronOperType.EDIT || oper == CronOperType.CHOOSE)
                    {
                        new_cron = cronMS.Copy();
                    }
                }
            }

            if (_dbregion == "PG SQL")
            {
                if (cronPG != null)
                {
                    //current = cronPG.order;

                    if (oper == CronOperType.COPY || oper == CronOperType.EDIT || oper == CronOperType.CHOOSE)
                    {
                        new_cron = cronPG.Copy();
                    }
                }
            }

            int max = 0;
            int maxPG = 0;

            if (MainWindow.Task.ListCronMS.Count() > 0)
            {
                max = MainWindow.Task.ListCronMS.Max(x => x.order);
            }
            if (MainWindow.Task.ListCronPG.Count() > 0)
            {
                maxPG = MainWindow.Task.ListCronPG.Max(x => x.order);
            }
            if (maxPG > max)
            {
                max = maxPG;
            }

            max++;

            if (new_cron == null)
            {
                new_cron = new Cron();
            }

            try
            {
                WinAddCron WinAddCron = new WinAddCron();
                WinAddCron.mainWindow = this.mainWindow;
                WinAddCron.logFile = this.logFile;

                if (oper == CronOperType.CHOOSE)
                {
                    new_cron.order = max;
                    new_cron.task = MainWindow.Task.TaskNumber;
                    new_cron.DBRegion = _dbregion;

                    WinAddCron.Title = "Изменить выбранное задание";
                }
                else if (oper == CronOperType.ADD)
                {
                    new_cron.SetCron(max, MainWindow.Task.TaskNumber, _dbregion, "present", "promed");

                    WinAddCron.Title = "Добавить новое задание";
                }
                else if (oper == CronOperType.EDIT)
                {
                    WinAddCron.Title = "Изменить задание";
                }
                else if (oper == CronOperType.COPY)
                {
                    new_cron.order = max;

                    WinAddCron.Title = "Клонировать задание";
                    WinAddCron.cbDBRegions.IsEnabled = true;
                }
                else
                {
                    return result;
                }

                new_cron.Set_lcbListRegions(null);
                new_cron.Set_lcbListExcludeRegions();
                WinAddCron.DataContext = new_cron;
                WinAddCron.ShowDialog();
                result = WinAddCron.isOk == true;

                if (result)
                {
                    new_cron.Set_regions();
                    new_cron.Set_exclude_regions();

                    if (new_cron.dbregion == "MS SQL")
                    {
                        // сначала добавляем в раздел для MS

                        if (oper != CronOperType.EDIT || cronMS == null)
                        {
                            cronMS = new Cron();
                        }

                        cronMS.SetCron(new_cron.order, new_cron.task, new_cron.dbregion, new_cron.state, new_cron.database, new_cron.stage, new_cron.application_name, new_cron.comment, new_cron.command, new_cron.schedule, new_cron.regions, new_cron.exclude_regions, new_cron.timeout, new_cron.hosts, new_cron.check, new_cron.team, new_cron.isTemp);

                        result = true;

                        cronPG = null;

                        if (
                            oper != CronOperType.EDIT &&
                            MainWindow.APPinfo.ListDBAlias_ForALL.Contains(new_cron.database) 
                        )
                        {
                            if (System.Windows.Forms.MessageBox.Show($"Добавить аналогичное задание в список для регионов PG SQL (ЕЦП)?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                            {
                                // добавляем в раздел для PG
                                cronPG = new Cron();

                                cronPG.SetCron(new_cron.order, new_cron.task, "PG SQL", new_cron.state, new_cron.database, new_cron.stage, new_cron.application_name, new_cron.comment, new_cron.command, new_cron.schedule, new_cron.regions, new_cron.exclude_regions, new_cron.timeout, new_cron.hosts, new_cron.check, new_cron.team, new_cron.isTemp);

                                AddListTemp(cronPG);
                            }
                        }
                    }

                    if (new_cron.dbregion == "PG SQL")
                    {
                        // сначала добавляем в раздел для PG

                        if (oper != CronOperType.EDIT || cronPG == null)
                        {
                            cronPG = new Cron();
                        }

                        cronPG.SetCron(new_cron.order, new_cron.task, new_cron.dbregion, new_cron.state, new_cron.database, new_cron.stage, new_cron.application_name, new_cron.comment, new_cron.command, new_cron.schedule, new_cron.regions, new_cron.exclude_regions, new_cron.timeout, new_cron.hosts, new_cron.check, new_cron.team, new_cron.isTemp);

                        result = true;

                        cronMS = null;

                        if (
                            oper != CronOperType.EDIT &&
                            MainWindow.APPinfo.ListDBAlias_ForALL.Contains(new_cron.database) 
                        )
                        {
                            if (System.Windows.Forms.MessageBox.Show($"Добавить аналогичное задание в список для регионов MS SQL (Промед)?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                            {
                                // добавляем в раздел для MS
                                cronMS = new Cron();

                                cronMS.SetCron(new_cron.order, new_cron.task, "MS SQL", new_cron.state, new_cron.database, new_cron.stage, new_cron.application_name, new_cron.comment, new_cron.command, new_cron.schedule, new_cron.regions, new_cron.exclude_regions, new_cron.timeout, new_cron.hosts, new_cron.check, new_cron.team, new_cron.isTemp);

                                AddListTemp(cronMS);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile);
                result = false;
            }
            
            return result;
        }

        /// <summary>
        /// Обновить и сохранить
        /// </summary>
        /// <param name="cron">задание</param>
        /// <param name="isReorder">=true - пересчитать order</param>
        private void RefreshAndSave(Cron cron, bool isReorder)
        {
            if (GridFocused != null)
            {
                var old = GridFocused.Name;

                if (isReorder)
                {
                    int cnt = 0;
                    foreach (var item in MainWindow.Task.ListCronMS.OrderBy(x => x.order))
                    {
                        cnt ++;
                        item.order = cnt;
                    }

                    cnt = 0;
                    foreach (var item in MainWindow.Task.ListCronPG.OrderBy(x => x.order))
                    {
                        cnt++;
                        item.order = cnt;
                    }
                }

                dgCronMSRefresh();
                dgCronPGRefresh();

                GridFocused.SelectedItem = cron;
                if (GridFocused.SelectedItem != null) //-V3022
                {
                    GridFocused.UpdateLayout();
                    GridFocused.ScrollIntoView(GridFocused.SelectedItem);
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                if (old == "dgCronPG")
                {
                    dgCronPGSelect();
                }
                else
                {
                    dgCronMSSelect();
                }
            }
        }

        /// <summary>
        /// Выбрать ветку в проекте GIT с заданиями
        /// </summary>
        /// <param name="projectCron">проект GIT</param>
        /// <param name="branchCron">ветка</param>
        /// <param name="isSelectedPrev">ветка уже выбрана в предыдущем проекте с заданиями</param>
        /// <param name="logFile">лог-файл</param>
        /// <returns></returns>
        public static bool SelectGITBranch(string projectCron, ref string branchCron, ref bool isSelectedPrev, string logFile)
        {
            if (!string.IsNullOrWhiteSpace(projectCron))
            {
                // есть проект cron, выберем в нем ветку

                if (isSelectedPrev)
                {
                    // попробуем выбрать ту же ветку, что и для предыдущего проекта cron
                    GIT.GitPull(new string[] { projectCron }, branchCron, false, true, false, logFile, false);

                    // определяем текущую ветку
                    string current_branch = GIT.GitCurrentBranch(projectCron, out string err, logFile);
                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        App.AddLog($"Ошибка определения текущей ветки в проекте {projectCron}:\n{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                        return false;
                    }

                    if (current_branch != branchCron)
                    {
                        // этой ветки нет, выберем другую ветку
                        if (GIT.SelectGITBranch(projectCron, branchCron, out string branch, logFile, false, false))
                        {
                            branchCron = branch;
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    // выбираем ветку
                    if (GIT.SelectGITBranch(projectCron, branchCron, out string branch, logFile, true, false))
                    {
                        branchCron = branch;
                        isSelectedPrev = true;
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Проверить и выбрать ветку PG
        /// </summary>
        /// <returns></returns>
        private bool CheckBranchPG()
        {
            if (string.IsNullOrWhiteSpace(this.BranchPG))
            {
                string branch = MainWindow.Task.TaskNumber;
                bool isSelectedPrev = false;

                if (string.IsNullOrWhiteSpace(this.ProjectCronPG))
                {
                    App.AddLog($"Не настроен проект для хранения заданий на PG", null, App.ShowMessageMode.SHOW, true, logFile);
                    return false;
                }

                if (
                    !SelectGITBranch(this.ProjectCronPG, ref branch, ref isSelectedPrev, logFile) ||
                    string.IsNullOrWhiteSpace(branch)
                    )
                {
                    App.AddLog($"Не выбрана ветка в проекте {this.ProjectCronPG}", null, App.ShowMessageMode.SHOW, true, logFile);
                    return false;
                }

                this.BranchPG = branch;
            }

            return true;
        }

        /// <summary>
        /// Проверить и выбрать ветку MS
        /// </summary>
        /// <returns></returns>
        private bool CheckBranchMS()
        {
            if (string.IsNullOrWhiteSpace(this.BranchMS))
            {
                string branch = MainWindow.Task.TaskNumber;
                bool isSelectedPrev = false;

                if (string.IsNullOrWhiteSpace(this.ProjectCronMS))
                {
                    App.AddLog($"Не настроен проект для хранения заданий на MS", null, App.ShowMessageMode.SHOW, true, logFile);
                    return false;
                }

                if (
                    !SelectGITBranch(this.ProjectCronMS, ref branch, ref isSelectedPrev, logFile) ||
                        string.IsNullOrWhiteSpace(branch)
                    )
                {
                    App.AddLog($"Не выбрана ветка в проекте {this.ProjectCronMS}", null, App.ShowMessageMode.SHOW, true, logFile);
                    return false;
                }

                this.BranchMS = branch;
            }

            return true;
        }

        private void SaveCron(Cron cronMS, Cron cronPG)
        {
            if (
                    cronMS != null &&
                    cronMS.dbregion == "MS SQL"
            )
            {
                var found = MainWindow.Task.ListCronMS.Where(x => x.IsKeyEqual(cronMS)).FirstOrDefault();

                if (found == null)
                {
                    MainWindow.Task.ListCronMS.Add(cronMS);
                    RefreshAndSave(cronMS, true);
                }

                if (GridFocused.Name == "dgCronPG")
                {
                    AddListTemp(cronMS);
                }
            }

            if (
                cronPG != null &&
                cronPG.dbregion == "PG SQL"
            )
            {
                var found = MainWindow.Task.ListCronPG.Where(x => x.IsKeyEqual(cronPG)).FirstOrDefault();

                if (found == null)
                {
                    MainWindow.Task.ListCronPG.Add(cronPG);
                    RefreshAndSave(cronPG, true);
                }

                if (GridFocused.Name == "dgCronMS")
                {
                    AddListTemp(cronPG);
                }
            }
        }

        /// <summary>Выбор задания</summary>
        private void btChoose_Click(object sender, RoutedEventArgs e)
        {
            if (GridFocused != null)
            {
                // Собрать список заданий
                var ListCron = new ObservableCollection<Cron>();

                if (GridFocused.Name == "dgCronPG")
                {
                    if (!CheckBranchPG())
                    {
                        return;
                    }

                    ListCron = Cron.ListAllCron(this.ProjectCronPG, "PG SQL", logFile);
                }
                else
                {
                    if (!CheckBranchMS())
                    {
                        return;
                    }

                    ListCron = Cron.ListAllCron(this.ProjectCronMS, "MS SQL", logFile);
                }

                // Выбрать задание из списка
                FormFindInList dlg1 = new FormFindInList(logFile);
                string result = "";

                dlg1.AddItems(ListCron.Select(x => x.ChooseName).OrderBy(x => x).ToList());

                var res = dlg1.ShowDialog();

                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (System.Windows.Forms.DataGridViewRow row in dlg1.dgListValues.SelectedRows)
                    {
                        result = row.Cells[0].Value.ToString();
                        // берем только первую
                        break; //-V3020
                    }

                    if (!string.IsNullOrWhiteSpace(result))
                    {
                        foreach (var item in ListCron.Where(x => x.ChooseName == result).OrderBy(x => x.order))
                        {
                            Cron cronMS = null;
                            Cron cronPG = null;

                            if (GridFocused.Name == "dgCronPG")
                            {
                                cronPG = item;
                                cronPG.order = item.order + 1000000;
                            }
                            else
                            {
                                cronMS = item;
                                cronMS.order = item.order + 1000000;
                            }

                            // добавить задание в конец списка
                            SaveCron(cronMS, cronPG);
                        }
                    }
                }

                dlg1.Dispose();

                if (res != System.Windows.Forms.DialogResult.OK)
                {
                    return;
                }

            }
        }

        /// <summary>Добавление задания</summary>
        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            Cron cronMS = null;
            Cron cronPG = null;

            if (AddEditCron(CronOperType.ADD, ref cronMS, ref cronPG))
            {
                SaveCron(cronMS, cronPG);
            }
        }

        /// <summary>Изменение задания</summary>
        private void btEdit_Click(object sender, RoutedEventArgs e)
        {
            Cron cronMS = null;
            Cron cronPG = null;

            if (GridFocused != null)
            {
                if (GridFocused.Name == "dgCronMS")
                {
                    if (GridFocused.SelectedIndex >= 0)
                    {
                        cronMS = GridFocused.SelectedItem as Cron;
                    }
                }

                if (GridFocused.Name == "dgCronPG")
                {
                    if (GridFocused.SelectedIndex >= 0)
                    {
                        cronPG = GridFocused.SelectedItem as Cron;
                    }
                }
            }
            else
            {
                return;
            }

            if (AddEditCron(CronOperType.EDIT, ref cronMS, ref cronPG))
            {
                if (
                    cronMS != null &&
                    cronMS.dbregion == "MS SQL"
                )
                {
                    RefreshAndSave(cronMS, true);
                }

                if (
                    cronPG != null &&
                    cronPG.dbregion == "PG SQL"
                )
                {
                    RefreshAndSave(cronPG, true);
                }
            }
        }

        /// <summary>Копирование задания</summary>
        private void btCopy_Click(object sender, RoutedEventArgs e)
        {
            Cron cronMS = null;
            Cron cronPG = null;

            if (GridFocused != null)
            {
                if (GridFocused.Name == "dgCronMS")
                {
                    if (GridFocused.SelectedIndex >= 0)
                    {
                        cronMS = GridFocused.SelectedItem as Cron;
                        cronMS = cronMS.Copy();
                    }
                }

                if (GridFocused.Name == "dgCronPG")
                {
                    if (GridFocused.SelectedIndex >= 0)
                    {
                        cronPG = GridFocused.SelectedItem as Cron;
                        cronPG = cronPG.Copy();
                    }
                }
            }
            else
            {
                return;
            }

            if (AddEditCron(CronOperType.COPY, ref cronMS, ref cronPG))
            {
                SaveCron(cronMS, cronPG);
            }
        }

        /// <summary>Нажата кнопка "Удалить"</summary>
        private void btDel_Click(object sender, RoutedEventArgs e)
        {
            if (GridFocused != null && GridFocused.SelectedIndex >= 0)
            {
                Cron cron = GridFocused.SelectedItem as Cron;

                if (System.Windows.Forms.MessageBox.Show($"Удалить задание {cron.order}. {cron.application_name}?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    MainWindow.Task.ListCronMS.Remove(cron);
                    MainWindow.Task.ListCronPG.Remove(cron);
                    RefreshAndSave(null, true);
                }
            }
        }

        /// <summary>Сгенерировать JSON-файл для MS</summary>
        private void btGenJSON_MS_Click(object sender, RoutedEventArgs e)
        {
            // сохранить файл
            Save("MS SQL", true);
        }

        /// <summary>Сгенерировать JSON-файл для PG</summary>
        private void btGenJSON_PG_Click(object sender, RoutedEventArgs e)
        {
            // сохранить файл
            Save("PG SQL", true);
        }

        /// <summary>Двойной клик мышью на строке в dgCron</summary>
        private void dgCron_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            btEdit_Click(sender, e);
        }

        /// <summary>
        /// Сохранить задания в файлы
        /// </summary>
        /// <param name="project">проект</param>
        /// <param name="dbregion">Тип региона по типу основной БД</param>
        /// <param name="BranchDefault">ветка</param>
        /// <param name="PathDefault">папка</param>
        /// <param name="FilenameDefault">файл</param>
        /// <param name="isBranchCanChanged">=true - можно изменить ветку</param>
        /// <param name="ListCron">список заданий</param>
        /// <param name="isForce">=true - сохраняем принудительно, =false - сохраняем если изменился</param>
        /// <param name="jsonFilepath">итоговый путь к файлу</param>
        /// <param name="jsonUrl">url к файлу в web-репозитории</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="version">версия</param>
        /// <returns></returns>
        public static bool SaveJSON(string project, string dbregion, string BranchDefault, string PathDefault, string FilenameDefault, bool isBranchCanChanged, ObservableCollection<Cron> ListCron, bool isForce, out string jsonFilepath, out string jsonUrl, string logFile, string version)
        {
            jsonFilepath = "";
            jsonUrl = "";

            if (
                string.IsNullOrWhiteSpace(project) ||
                string.IsNullOrWhiteSpace(BranchDefault) ||
                string.IsNullOrWhiteSpace(PathDefault) && !string.IsNullOrWhiteSpace(version) ||
                string.IsNullOrWhiteSpace(FilenameDefault) && !string.IsNullOrWhiteSpace(version)
            )
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = MainWindow.Task.LogFile;
            }

            if (string.IsNullOrWhiteSpace(FilenameDefault))
            {
                FilenameDefault = "";
            }

            // в режиме сборки версии всегда сохраняем принудительно
            if (!string.IsNullOrWhiteSpace(version))
            {
                isForce = true;
            }

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            bool isSaved = false;

            if (ListCron != null)
            {
                // перебираем задания
                foreach (var cron in ListCron.OrderBy(x => x.order))
                {
                    // сгенерим новый текст файла
                    string jsonText = "";

                    string PathCron = cron.folder;
                    string FilenameCron = cron.filename;

                    ObservableCollection<Cron> new_list = new ObservableCollection<Cron>();

                    int _order = 0;

                    foreach (var item in ListCron.Where(x => x.IsInFile(cron)).OrderBy(x => x.order))
                    {
                        _order++;
                        var new_cron = item.Copy();
                        new_cron.order = _order;
                        new_list.Add(new_cron);
                    }

                    jsonText = Cron.GenerateJSON(null, new_list, logFile, null, false);

                    // загрузим существующий файл
                    string jsonText_before = "";
                    bool isChanged = false;

                    if (!isForce)
                    {
                        string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, PathCron);
                        string filename = Path.Combine(folder, FilenameCron);

                        if (File.Exists(filename))
                        {
                            try
                            {
                                jsonText_before = File.ReadAllText(filename);

                                if (string.IsNullOrWhiteSpace(jsonText_before))
                                {
                                    jsonText_before = "";
                                }
                            }
                            catch (Exception ex)
                            {
                                App.AddLog($"Ошибка загрузки файла {filename} :", ex, App.ShowMessageMode.SHOW, true, logFile);

                                jsonText_before = "";
                            }
                        }

                        jsonText_before = jsonText_before
                            .Replace(Environment.NewLine, "\n")
                            .TrimEndNewLine("\n");

                        // проверим, изменился ли скрипт
                        if (jsonText_before != jsonText)
                        {
                            isChanged = (System.Windows.Forms.MessageBox.Show($"Список заданий для {BranchDefault} в проекте {project} изменился, сохранить в файл {PathCron}\\{FilenameCron}?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes);
                        }
                    }

                    // сохраняем - если принудительно или если содержимое изменилось
                    if (isForce || isChanged)
                    {
                        // сохранить json-файл с новым содержимым
                        isSaved = GIT.SaveFileToGIT(project, BranchDefault, PathCron, FilenameCron, isBranchCanChanged, false, false, jsonText, out string _file, out string _url, logFile, false, ".json", "(*.json)|*.json|Все файлы (*.*)|*.*", true)
                        || isSaved;

                        if (isSaved)
                        {
                            isBranchCanChanged = false;
                        }
                    }
                }
            }

            // сгенерим и сохраним версию
            if (
                !string.IsNullOrWhiteSpace(version) &&
                !string.IsNullOrWhiteSpace(FilenameDefault)
            )
            {
                // все задания
                var AllCron = Cron.ListAllCron(project, dbregion, logFile);

                // сгенерить json со всеми заданиями
                string jsonText = Cron.GenerateJSON(null, AllCron, logFile, version, false);

                // сохранить json-файл с новым содержимым
                isSaved = GIT.SaveFileToGIT(project, BranchDefault, PathDefault, FilenameDefault, isBranchCanChanged, false, false, jsonText, out jsonFilepath, out jsonUrl, logFile, false, ".json", "(*.json)|*.json|Все файлы (*.*)|*.*", true);

                if (isSaved)
                {
                    isBranchCanChanged = false;

                    if (dbregion == "MS SQL")
                    {
                        MainWindow.Task.JSONFilenameCronMS = jsonUrl;
                    }
                    else if (dbregion == "PG SQL")
                    {
                        MainWindow.Task.JSONFilenameCronPG = jsonUrl;
                    }

                    // задания для box-версии
                    ObservableCollection<Cron> BoxCron = new ObservableCollection<Cron>();

                    int _order = 0;
                    foreach (var item in AllCron.Where(x =>
                            x.istemp == 1 && // постоянные
                            x.regions.Contains("all") && // для всех регионов
                            x.state == "present" // действующие
                        ))
                    {
                        _order++;
                        var _cron = item.Copy();
                        _cron.order = _order; // перенумеровываем
                        _cron.exclude_regions = new List<string>(); // убираем регионы-исключения
                        BoxCron.Add(_cron);
                    }

                    // новое имя файла
                    string _boxfilename = FilenameDefault.Replace(".json", ".box");
                    string _boxversion = "box";

                    // сгенерить json с заданиями для box-версии
                    jsonText = Cron.GenerateJSON(null, BoxCron, logFile, _boxversion, true);

                    // сохранить box-файл с новым содержимым
                    GIT.SaveFileToGIT(project, BranchDefault, PathDefault, _boxfilename, isBranchCanChanged, false, false, jsonText, out string boxFilepath, out string boxUrl, logFile, false, ".box", "(*.box)|*.box|Все файлы (*.*)|*.*", true);
                }
            }

            return isSaved;
        }

        /// <summary>
        /// Сохранить скрипты заданий
        /// </summary>
        /// <param name="_dbregion">с какой вкладки сохраняем</param>
        /// <param name="isForce">=true - сохраняем принудительно, =false - сохраняем если изменился</param>
        private void Save(string _dbregion, bool isForce)
        {
            string _branch = "";
            string _path = "";
            string _file = "";  
            string _project = "";
            ObservableCollection<Cron> _list = null;
            bool isBranchCanChanged = false;

            if (_dbregion == "MS SQL")
            {
                _project = this.ProjectCronMS;
                _list = MainWindow.Task.ListCronMS;
            }
            else if (_dbregion == "PG SQL")
            {
                _project = this.ProjectCronPG;
                _list = MainWindow.Task.ListCronPG;
            }

            if (!string.IsNullOrWhiteSpace(MainWindow.Task.ReleaseVersion))
            {
                _branch = MainWindow.Task.ReleaseVersion;
                _path = "version";
                isBranchCanChanged = false;

                if (_dbregion == "MS SQL")
                {
                    _file = Path.GetFileName(MainWindow.Task.JSONFilenameCronMS);
                }
                else if (_dbregion == "PG SQL")
                {
                    _file = Path.GetFileName(MainWindow.Task.JSONFilenameCronPG);
                }
            }
            else
            {
                _branch = MainWindow.Task.TaskNumber;
                _path = null;
                _file = null;
                isBranchCanChanged = true;
            }

            bool isSaved = WinCron.SaveJSON(_project, _dbregion, _branch, _path, _file, isBranchCanChanged, _list, isForce, out string jsonFilepath, out string jsonUrl, logFile, MainWindow.Task.ReleaseVersion);

            if (isSaved)
            {
                if (!string.IsNullOrWhiteSpace(MainWindow.Task.ReleaseVersion))
                {
                    if (_dbregion == "MS SQL")
                    {
                        MainWindow.Task.JSONFilenameCronMS = jsonUrl;
                    }
                    else if (_dbregion == "PG SQL")
                    {
                        MainWindow.Task.JSONFilenameCronPG = jsonUrl;
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }
            }
        }

        /// <summary>
        /// Нажата клавиша на вкладе Cron Plan для MS SQL (Промед)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tiCronMS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                btGenJSON_MS_Click(null, null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Нажата клавиша на вкладе Cron Plan для PostgreSQL (ЕЦП)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tiCronPG_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                btGenJSON_PG_Click(null, null);
                e.Handled = true;
            }
        }

        private void btUpMS_Click(object sender, RoutedEventArgs e)
        {
            if (!dgCronMS.IsFocused) dgCronMS.Focus();

            if (dgCronMS.SelectedIndex >= 0)
            {
                // выбранная строка
                Cron cron = dgCronMS.SelectedItem as Cron;

                if (cron != null)
                {
                    var prev_cron = MainWindow.Task.ListCronMS
                        .Where(x => x.order < cron.order)
                        .OrderByDescending(x => x.order)
                        .FirstOrDefault();

                    if (prev_cron != null)
                    {
                        // есть предыдущая строка
                        var prev_order = prev_cron.order;
                        prev_cron.order = cron.order;
                        cron.order = prev_order;
                    }
                    else
                    {
                        if (cron.order > 1)
                        {
                            cron.order--;
                        }
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgCronMSRefresh();
            }
        }

        private void btUpPG_Click(object sender, RoutedEventArgs e)
        {
            if (!dgCronPG.IsFocused) dgCronPG.Focus();

            if (dgCronPG.SelectedIndex >= 0)
            {
                // выбранная строка
                Cron cron = dgCronPG.SelectedItem as Cron;

                if (cron != null)
                {
                    var prev_cron = MainWindow.Task.ListCronPG
                        .Where(x => x.order < cron.order)
                        .OrderByDescending(x => x.order)
                        .FirstOrDefault();

                    if (prev_cron != null)
                    {
                        // есть предыдущая строка
                        var prev_order = prev_cron.order;
                        prev_cron.order = cron.order;
                        cron.order = prev_order;
                    }
                    else
                    {
                        if (cron.order > 1)
                        {
                            cron.order--;
                        }
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgCronPGRefresh();
            }
        }
        private void btDownMS_Click(object sender, RoutedEventArgs e)
        {
            if (!dgCronMS.IsFocused) dgCronMS.Focus();

            if (dgCronMS.SelectedIndex >= 0)
            {
                // выбранная строка
                Cron cron = dgCronMS.SelectedItem as Cron;

                if (cron != null)
                {
                    var next_cron = MainWindow.Task.ListCronMS
                        .Where(x => x.order > cron.order)
                        .OrderBy(x => x.order)
                        .FirstOrDefault();

                    if (next_cron != null)
                    {
                        // есть предыдущая строка
                        var next_order = next_cron.order;
                        next_cron.order = cron.order;
                        cron.order = next_order;
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgCronMSRefresh();
            }
        }

        private void btDownPG_Click(object sender, RoutedEventArgs e)
        {
            if (!dgCronPG.IsFocused) dgCronPG.Focus();

            if (dgCronPG.SelectedIndex >= 0)
            {
                // выбранная строка
                Cron cron = dgCronPG.SelectedItem as Cron;

                if (cron != null)
                {
                    var next_cron = MainWindow.Task.ListCronPG
                        .Where(x => x.order > cron.order)
                        .OrderBy(x => x.order)
                        .FirstOrDefault();

                    if (next_cron != null)
                    {
                        // есть предыдущая строка
                        var next_order = next_cron.order;
                        next_cron.order = cron.order;
                        cron.order = next_order;
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgCronPGRefresh();
            }
        }

        private void tiCronMS_GotFocus(object sender, RoutedEventArgs e)
        {
            GridFocused = dgCronMS;
        }

        private void tiCronPG_GotFocus(object sender, RoutedEventArgs e)
        {
            GridFocused = dgCronPG;
        }

        /// <summary>Нажата Enter в строке фильтрации</summary>
        private void tbFilterMS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btFilterMS_Click(sender, e);
                e.Handled = true;
            }
        }

        /// <summary>Нажата кнопка "Фильтр"</summary>
        private void btFilterMS_Click(object sender, RoutedEventArgs e)
        {
            dgCronMSRefresh();

            // Добавить в историю
            if (!string.IsNullOrWhiteSpace(tbFilterMS.Text))
            {
                tbFilterMS.AddHistory(tbFilterMS.Text);
            }
        }

        /// <summary>Нажата кнопка "Очистить фильтр"</summary>
        private void btDelFilterMS_Click(object sender, RoutedEventArgs e)
        {
            tbFilterMS.Text = "";
            dgCronMSRefresh();
        }

        /// <summary>
        /// Выбор из истории фильтра
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFilterMS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbFilterMS.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbFilterMS.SelectedItem;

                tbFilterMS.Text = (string)cbItem.Tag;
            }
        }

        /// <summary>
        /// Выход из поля Фильтр
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFilterMS_LostFocus(object sender, RoutedEventArgs e)
        {
            btFilterMS_Click(sender, e);
        }

        /// <summary>
        /// Закрыт список выбора из истории фильтра
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFilterMS_DropDownClosed(object sender, EventArgs e)
        {
            btFilterMS_Click(sender, null);
        }


        /// <summary>Нажата Enter в строке фильтрации</summary>
        private void tbFilterPG_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btFilterPG_Click(sender, e);
                e.Handled = true;
            }
        }

        /// <summary>Нажата кнопка "Фильтр"</summary>
        private void btFilterPG_Click(object sender, RoutedEventArgs e)
        {
            dgCronPGRefresh();

            // Добавить в историю
            if (!string.IsNullOrWhiteSpace(tbFilterPG.Text))
            {
                tbFilterPG.AddHistory(tbFilterPG.Text);
            }
        }

        /// <summary>Нажата кнопка "Очистить фильтр"</summary>
        private void btDelFilterPG_Click(object sender, RoutedEventArgs e)
        {
            tbFilterPG.Text = "";
            dgCronPGRefresh();
        }

        /// <summary>
        /// Выбор из истории фильтра
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFilterPG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbFilterPG.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbFilterPG.SelectedItem;

                tbFilterPG.Text = (string)cbItem.Tag;
            }
        }

        /// <summary>
        /// Выход из поля Фильтр
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFilterPG_LostFocus(object sender, RoutedEventArgs e)
        {
            btFilterPG_Click(sender, e);
        }

        /// <summary>
        /// Закрыт список выбора из истории фильтра
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFilterPG_DropDownClosed(object sender, EventArgs e)
        {
            btFilterPG_Click(sender, null);
        }

    }
}
