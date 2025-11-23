// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using SQLGen.Controls;
using SQLGen.Utilities;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace SQLGen
{
    // -------------------------------------------------------------------------------------------------------
    /// <summary>Типы операций с действиями</summary>
    public enum DeploymentOperType
    {
        /// <summary>
        /// добавить в начало
        /// </summary>
        ADD_FIRST,
        /// <summary>
        /// вставить
        /// </summary>
        INSERT,
        /// <summary>
        /// добавить в конец
        /// </summary>
        ADD_LAST,
        /// <summary>
        /// изменить
        /// </summary>
        EDIT,
        /// <summary>
        /// скопировать и сделать дубликат, добавить в конец
        /// </summary>
        COPY
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Окно для сборки JSON-файла со списком действий при обновлении
    /// </summary>
    public partial class WinDeployment : Window
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

        private CollectionViewSource _filteredMS;
        private CollectionViewSource _filteredPG;

        private ICollectionView FilteredMS
        {
            get
            {
                if (_filteredMS != null)
                {
                    return _filteredMS.View;
                }
                else
                {
                    return null;
                }
            }
        }

        private ICollectionView FilteredPG
        {
            get
            {
                if (_filteredPG != null)
                {
                    return _filteredPG.View;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Отображать вкладку для сохранения json-файла для MS
        /// </summary>
        private bool isVisibleMSJson = true;

        /// <summary>
        /// Отображать вкладку для сохранения json-файла для PG
        /// </summary>
        private bool isVisiblePGJson = true;

        /// <summary>
        /// Проект для хранения действий при обновлении MS 
        /// </summary>
        public string ProjectDeploymentMS = null;

        /// <summary>
        /// Проект для хранения действий при обновлении PG 
        /// </summary>
        public string ProjectDeploymentPG = null;

        /// <summary>
        /// Проект по умолчанию
        /// </summary>
        public string ProjectDefault = null;

        /// <summary>Конструктор WinDeployment</summary>
        public WinDeployment()
        {
            InitializeComponent();

            isStart = true;

            _filteredPG = new CollectionViewSource();
            _filteredPG.Source = MainWindow.Task.ListDeploymentPG;
            if (_filteredPG.View != null && _filteredPG.View.CanSort == true)
            {
                _filteredPG.View.SortDescriptions.Clear();
                _filteredPG.View.SortDescriptions.Add(new SortDescription("SortBy", ListSortDirection.Ascending));
            }

            _filteredMS = new CollectionViewSource();
            _filteredMS.Source = MainWindow.Task.ListDeploymentMS;
            if (_filteredMS.View != null && _filteredMS.View.CanSort == true)
            {
                _filteredMS.View.SortDescriptions.Clear();
                _filteredMS.View.SortDescriptions.Add(new SortDescription("SortBy", ListSortDirection.Ascending));
            }

            dgDeploymentMS.ItemsSource = FilteredMS;
            dgDeploymentMSRefresh();

            dgDeploymentPG.ItemsSource = FilteredPG;
            dgDeploymentPGRefresh();

            // пользовательские настройки GUI
            Default.InitGUI(
                "WinDeployment",
                this,
                mainGrid,
                new List<System.Windows.Controls.Control> { tbMSJson, tbPGJson },
                new List<ComboBox> { cbMSFontFamily, cbPGFontFamily },
                new List<ComboBox> { cbMSFontSize, cbPGFontSize },
                MainWindow.Task.LogFile
                );

            // заполняем toolbar'ы
            tbMSJson.AddToolbarDefault(toolbarToolsMS, tiMS, true, false);
            tbPGJson.AddToolbarDefault(toolbarToolsPG, tiPG, true, false);
        }

        /// <summary>При открытии окна WinDeployment</summary>
        private void winDeployment_Activated(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(MainWindow.Task.ReleaseVersion))
            {
                this.Title = $"Deployment Plan версии {MainWindow.Task.ReleaseVersion}, релизная задача {MainWindow.Task.TaskNumber}";
            }
            else
            {
                this.Title = $"Действия при обновлении для задачи {MainWindow.Task.TaskNumber}";
            }

            isVisibleMSJson = !string.IsNullOrWhiteSpace(this.ProjectDeploymentMS);
            isVisiblePGJson = !string.IsNullOrWhiteSpace(this.ProjectDeploymentPG);


            if (!string.IsNullOrWhiteSpace(ProjectDefault))
            {
                isVisibleMSJson = (ProjectDefault == this.ProjectDeploymentMS);
                isVisiblePGJson = (ProjectDefault == this.ProjectDeploymentPG);
            }

            if (!isVisiblePGJson)
            {
                tiDeploymentPG.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (isStart)
                {
                    tiDeploymentPG.Focus();
                    tiListPG.Focus();
                }
            }

            if (!isVisibleMSJson)
            {
                tiDeploymentMS.Visibility = Visibility.Collapsed;
            }
            else
            {
                if (isStart)
                {
                    tiDeploymentMS.Focus();
                    tiListMS.Focus();
                }
            }

            isStart = false;
        }

        /// <summary>При закрытии окна WinDeployment</summary>
        private void winDeployment_Closed(object sender, EventArgs e)
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
                "WinDeployment",
                this,
                new List<System.Windows.Controls.Control> { tbMSJson, tbMSJson }
                );
        }

        /// <summary>Обновить список dgDeploymentMS</summary>
        public void dgDeploymentMSRefresh()
        {
            //dgDeploymentMS.Items.Refresh();
            if (FilteredMS != null)
            {
                var lcv = (ListCollectionView)FilteredMS;
                if (lcv.IsAddingNew) lcv.CommitNew();
                if (lcv.IsEditingItem) lcv.CommitEdit();
                FilteredMS.Refresh();
            }

            dgDeploymentMSSelect();
        }


        /// <summary>Обновить список dgDeploymentPG</summary>
        public void dgDeploymentPGRefresh()
        {
            //dgDeploymentPG.Items.Refresh();
            if (FilteredPG != null)
            {
                var lcv = (ListCollectionView)FilteredPG;
                if (lcv.IsAddingNew) lcv.CommitNew();
                if (lcv.IsEditingItem) lcv.CommitEdit();

                FilteredPG.Refresh();
            }

            dgDeploymentPGSelect();
        }

        /// <summary>Выбрана запись в dgDeploymentMS </summary>
        private void dgDeploymentMSSelect()
        {
            tiDeploymentMS_GotFocus(null, null);
        }

        /// <summary>Выбрана запись в dgDeploymentPG </summary>
        private void dgDeploymentPGSelect()
        {
            tiDeploymentPG_GotFocus(null, null);
        }

        /// <summary>Выбрана запись в dgDeploymentMS</summary>
        private void dgDeploymentMS_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgDeploymentMSSelect();
        }

        /// <summary>Выбрана запись в dgDeploymentPG</summary>
        private void dgDeploymentPG_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgDeploymentPGSelect();
        }

        /// <summary>
        /// Добавить в ListTemp
        /// </summary>
        /// <param name="_deployment"></param>
        private void AddListTemp(Deployment _deployment)
        {
            if (_deployment == null) return;

            MainWindow.Task.ListDeploymentTemp.Add(_deployment);
        }

        /// <summary>
        /// Добавить или изменить запись в Deployment
        /// </summary>
        /// <param name="oper">операция</param>
        /// <param name="deploymentMS">для MS</param>
        /// <param name="deploymentPG">для PG</param>
        /// <returns></returns>
        private bool AddEditDeployment(DeploymentOperType oper, ref Deployment deploymentMS, ref Deployment deploymentPG)
        {
            bool result = false;

            string _dbregion = null;

            if (GridFocused != null)
            {
                if (GridFocused.Name == "dgDeploymentPG")
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

            Deployment new_deployment = null;
            int current = 1;

            if (_dbregion == "MS SQL")
            {
                if (deploymentMS != null)
                {
                    current = deploymentMS.order;

                    if (oper == DeploymentOperType.COPY || oper == DeploymentOperType.EDIT)
                    {
                        new_deployment = deploymentMS.Copy();
                    }
                }
            }

            if (_dbregion == "PG SQL")
            {
                if (deploymentPG != null)
                {
                    current = deploymentPG.order;

                    if (oper == DeploymentOperType.COPY || oper == DeploymentOperType.EDIT)
                    {
                        new_deployment = deploymentPG.Copy();
                    }
                }
            }

            int max = 0;
            int maxPG = 0;

            if (MainWindow.Task.ListDeploymentMS.Count() > 0)
            {
                max = MainWindow.Task.ListDeploymentMS.Max(x => x.order);
            }
            if (MainWindow.Task.ListDeploymentPG.Count() > 0)
            {
                maxPG = MainWindow.Task.ListDeploymentPG.Max(x => x.order);
            }
            if (maxPG > max)
            {
                max = maxPG;
            }

            max++;

            if (new_deployment == null)
            {
                new_deployment = new Deployment();
            }

            try
            {
                WinAddDeployment WinAddDeployment = new WinAddDeployment();
                WinAddDeployment.mainWindow = this.mainWindow;
                WinAddDeployment.logFile = this.logFile;

                if (oper == DeploymentOperType.ADD_FIRST)
                {
                    oper = DeploymentOperType.ADD_FIRST;

                    new_deployment.SetDeployment(MainWindow.Task.TaskNumber, 1, _dbregion, "after", "liquibase", null, null, "promed");

                    WinAddDeployment.Title = "Добавить новое действие при обновлении в начало";
                }
                else if (oper == DeploymentOperType.INSERT)
                {
                    oper = DeploymentOperType.INSERT;

                    new_deployment.SetDeployment(MainWindow.Task.TaskNumber, current, _dbregion, "after", "liquibase", null, null, "promed");

                    WinAddDeployment.Title = "Вставить новое действие при обновлении";
                }
                else if (oper == DeploymentOperType.ADD_LAST)
                {
                    oper = DeploymentOperType.ADD_LAST;

                    new_deployment.SetDeployment(MainWindow.Task.TaskNumber, max, _dbregion, "after", "liquibase", null, null, "promed");

                    WinAddDeployment.Title = "Добавить новое действие при обновлении в конец";
                }
                else if (oper == DeploymentOperType.EDIT)
                {
                    WinAddDeployment.Title = "Изменить действие при обновлении";
                }
                else if (oper == DeploymentOperType.COPY)
                {
                    new_deployment.order = max;

                    WinAddDeployment.Title = "Клонировать действие при обновлении";
                    WinAddDeployment.cbDBRegions.IsEnabled = true;
                }
                else
                {
                    return result;
                }

                new_deployment.Set_lcbListRegions(null);
                new_deployment.Set_lcbListExcludeRegions();
                WinAddDeployment.DataContext = new_deployment;
                WinAddDeployment.ShowDialog();
                result = WinAddDeployment.isOk == true;

                if (result)
                {
                    new_deployment.Set_regions();
                    new_deployment.Set_exclude_regions();

                    if (new_deployment.dbregion == "MS SQL")
                    {
                        // сначала добавляем в раздел для MS

                        if (oper != DeploymentOperType.EDIT || deploymentMS == null)
                        {
                            deploymentMS = new Deployment();
                        }

                        deploymentMS.SetDeployment(new_deployment.task, new_deployment.order, new_deployment.dbregion, new_deployment.position, new_deployment.type, new_deployment.script, new_deployment.file, new_deployment.database, new_deployment.stage, new_deployment.regions, new_deployment.exclude_regions, new_deployment.timeout, new_deployment.when_failed_action, new_deployment.when_failed_name, new_deployment.when_failed_job, new_deployment.when_failed_schedule, new_deployment.when_failed_script, new_deployment.when_failed_script_after, new_deployment.when_timeout_action, new_deployment.when_timeout_name, new_deployment.when_timeout_job, new_deployment.when_timeout_schedule, new_deployment.when_timeout_script, new_deployment.when_timeout_script_after);


                        result = true;

                        deploymentPG = null;

                        if (
                            (oper == DeploymentOperType.ADD_FIRST) ||
                            (oper == DeploymentOperType.INSERT)
                        )
                        {
                            // сдвигаем все номера на +1
                            foreach (var item in MainWindow.Task.ListDeploymentMS)
                            {
                                if (item.order >= new_deployment.order)
                                {
                                    item.order++;
                                }
                            }
                        }

                        if (
                            oper != DeploymentOperType.EDIT &&
                            MainWindow.APPinfo.ListDBAlias_ForALL.Contains(new_deployment.database) 
                        )
                        {
                            if (System.Windows.Forms.MessageBox.Show($"Добавить аналогичное действие в список для регионов PG SQL (ЕЦП)?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                            {
                                // добавляем в раздел для PG
                                deploymentPG = new Deployment();

                                deploymentPG.SetDeployment(new_deployment.task, new_deployment.order, "PG SQL", new_deployment.position, new_deployment.type, new_deployment.script, new_deployment.file, new_deployment.database,new_deployment.stage, new_deployment.regions, new_deployment.exclude_regions, new_deployment.timeout, new_deployment.when_failed_action, new_deployment.when_failed_name, new_deployment.when_failed_job, new_deployment.when_failed_schedule, new_deployment.when_failed_script, new_deployment.when_failed_script_after, new_deployment.when_timeout_action, new_deployment.when_timeout_name, new_deployment.when_timeout_job, new_deployment.when_timeout_schedule, new_deployment.when_timeout_script, new_deployment.when_timeout_script_after);

                                if (
                                    (oper == DeploymentOperType.ADD_FIRST) ||
                                    (oper == DeploymentOperType.INSERT)
)
                                {
                                    // сдвигаем все номера на +1
                                    foreach (var item in MainWindow.Task.ListDeploymentPG)
                                    {
                                        if (item.order >= new_deployment.order)
                                        {
                                            item.order++;
                                        }
                                    }
                                }

                                AddListTemp(deploymentPG);
                            }
                        }
                    }

                    if (new_deployment.dbregion == "PG SQL")
                    {
                        // сначала добавляем в раздел для PG

                        if (oper != DeploymentOperType.EDIT || deploymentPG == null)
                        {
                            deploymentPG = new Deployment();
                        }

                        deploymentPG.SetDeployment(new_deployment.task, new_deployment.order, new_deployment.dbregion, new_deployment.position, new_deployment.type, new_deployment.script, new_deployment.file, new_deployment.database, new_deployment.stage, new_deployment.regions, new_deployment.exclude_regions, new_deployment.timeout, new_deployment.when_failed_action, new_deployment.when_failed_name, new_deployment.when_failed_job, new_deployment.when_failed_schedule, new_deployment.when_failed_script, new_deployment.when_failed_script_after, new_deployment.when_timeout_action, new_deployment.when_timeout_name, new_deployment.when_timeout_job, new_deployment.when_timeout_schedule, new_deployment.when_timeout_script, new_deployment.when_timeout_script_after);

                        result = true;

                        deploymentMS = null;

                        if (
                            (oper == DeploymentOperType.ADD_FIRST) ||
                            (oper == DeploymentOperType.INSERT)
                        )
                        {
                            // сдвигаем все номера на +1
                            foreach (var item in MainWindow.Task.ListDeploymentPG)
                            {
                                if (item.order >= new_deployment.order)
                                {
                                    item.order++;
                                }
                            }
                        }

                        if (
                            oper != DeploymentOperType.EDIT &&
                            MainWindow.APPinfo.ListDBAlias_ForALL.Contains(new_deployment.database) 
                        )
                        {
                            if (System.Windows.Forms.MessageBox.Show($"Добавить аналогичное действие в список для регионов MS SQL (Промед)?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                            {
                                // добавляем в раздел для MS
                                deploymentMS = new Deployment();

                                deploymentMS.SetDeployment(new_deployment.task, new_deployment.order, "MS SQL", new_deployment.position, new_deployment.type, new_deployment.script, new_deployment.file, new_deployment.database,new_deployment.stage, new_deployment.regions, new_deployment.exclude_regions, new_deployment.timeout, new_deployment.when_failed_action, new_deployment.when_failed_name, new_deployment.when_failed_job, new_deployment.when_failed_schedule, new_deployment.when_failed_script, new_deployment.when_failed_script_after, new_deployment.when_timeout_action, new_deployment.when_timeout_name, new_deployment.when_timeout_job, new_deployment.when_timeout_schedule, new_deployment.when_timeout_script, new_deployment.when_timeout_script_after);

                                if (
                                    (oper == DeploymentOperType.ADD_FIRST) ||
                                    (oper == DeploymentOperType.INSERT)
)
                                {
                                    // сдвигаем все номера на +1
                                    foreach (var item in MainWindow.Task.ListDeploymentMS)
                                    {
                                        if (item.order >= new_deployment.order)
                                        {
                                            item.order++;
                                        }
                                    }
                                }

                                AddListTemp(deploymentMS);
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
        /// <param name="deployment">действие</param>
        /// <param name="isReorder">=true - пересчитать order</param>
        private void RefreshAndSave(Deployment deployment, bool isReorder)
        {
            if (GridFocused != null)
            {
                if (isReorder)
                {
                    int cnt = 0;
                    foreach (var item in MainWindow.Task.ListDeploymentMS.OrderBy(x => x.SortBy))
                    {
                        cnt ++;
                        item.order = cnt;
                    }

                    cnt = 0;
                    foreach (var item in MainWindow.Task.ListDeploymentPG.OrderBy(x => x.SortBy))
                    {
                        cnt++;
                        item.order = cnt;
                    }
                }

                dgDeploymentMSRefresh();
                dgDeploymentPGRefresh();

                GridFocused.SelectedItem = deployment;
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
            }
        }

        private void SaveDeployment(Deployment deploymentMS, Deployment deploymentPG)
        {
            if (
                deploymentMS != null &&
                deploymentMS.dbregion == "MS SQL"
            )
            {
                MainWindow.Task.ListDeploymentMS.Add(deploymentMS);
                RefreshAndSave(deploymentMS, true);

                if (GridFocused.Name == "dgCronPG")
                {
                    AddListTemp(deploymentMS);
                }
            }

            if (
                deploymentPG != null &&
                deploymentPG.dbregion == "PG SQL"
            )
            {
                MainWindow.Task.ListDeploymentPG.Add(deploymentPG);
                RefreshAndSave(deploymentPG, true);

                if (GridFocused.Name == "dgCronMS")
                {
                    AddListTemp(deploymentPG);
                }
            }
        }

        /// <summary>Добавление действия в конец списка</summary>
        private void btAddLast_Click(object sender, RoutedEventArgs e)
        {
            Deployment deploymentMS = null;
            Deployment deploymentPG = null;

            if (AddEditDeployment(DeploymentOperType.ADD_LAST, ref deploymentMS, ref deploymentPG))
            {
                SaveDeployment(deploymentMS, deploymentPG);
            }
        }

        /// <summary>Добавление действия в начало списка</summary>
        private void btAddFirst_Click(object sender, RoutedEventArgs e)
        {
            Deployment deploymentMS = null;
            Deployment deploymentPG = null;

            if (AddEditDeployment(DeploymentOperType.ADD_FIRST, ref deploymentMS, ref deploymentPG))
            {
                SaveDeployment(deploymentMS, deploymentPG);
            }
        }

        /// <summary>Вставить действие в середину списка</summary>
        private void btInsert_Click(object sender, RoutedEventArgs e)
        {
            Deployment deploymentMS = null;
            Deployment deploymentPG = null;

            if (GridFocused != null)
            {
                if (GridFocused.Name == "dgDeploymentMS")
                {
                    if (GridFocused.SelectedIndex >= 0)
                    {
                        deploymentMS = GridFocused.SelectedItem as Deployment;
                    }
                }

                if (GridFocused.Name == "dgDeploymentPG")
                {
                    if (GridFocused.SelectedIndex >= 0)
                    {
                        deploymentPG = GridFocused.SelectedItem as Deployment;
                    }
                }
            }
            else
            {
                return;
            }

            if (AddEditDeployment(DeploymentOperType.INSERT, ref deploymentMS, ref deploymentPG))
            {
                SaveDeployment(deploymentMS, deploymentPG);
            }
        }

        /// <summary>Изменение действия</summary>
        private void btEdit_Click(object sender, RoutedEventArgs e)
        {
            Deployment deploymentMS = null;
            Deployment deploymentPG = null;

            if (GridFocused != null)
            {
                if (GridFocused.Name == "dgDeploymentMS")
                {
                    if (GridFocused.SelectedIndex >= 0)
                    {
                        deploymentMS = GridFocused.SelectedItem as Deployment;
                    }
                }

                if (GridFocused.Name == "dgDeploymentPG")
                {
                    if (GridFocused.SelectedIndex >= 0)
                    {
                        deploymentPG = GridFocused.SelectedItem as Deployment;
                    }
                }
            }
            else
            {
                return;
            }

            if (AddEditDeployment(DeploymentOperType.EDIT, ref deploymentMS, ref deploymentPG))
            {
                if (
                    deploymentMS != null &&
                    deploymentMS.dbregion == "MS SQL"
                )
                {
                    RefreshAndSave(deploymentMS, true);
                }

                if (
                    deploymentPG != null &&
                    deploymentPG.dbregion == "PG SQL"
                )
                {
                    RefreshAndSave(deploymentPG, true);
                }
            }
        }

        /// <summary>Копирование действия</summary>
        private void btCopy_Click(object sender, RoutedEventArgs e)
        {
            Deployment deploymentMS = null;
            Deployment deploymentPG = null;

            if (GridFocused != null)
            {
                if (GridFocused.Name == "dgDeploymentMS")
                {
                    if (GridFocused.SelectedIndex >= 0)
                    {
                        deploymentMS = GridFocused.SelectedItem as Deployment;
                        deploymentMS = deploymentMS.Copy();
                    }
                }

                if (GridFocused.Name == "dgDeploymentPG")
                {
                    if (GridFocused.SelectedIndex >= 0)
                    {
                        deploymentPG = GridFocused.SelectedItem as Deployment;
                        deploymentPG = deploymentPG.Copy();
                    }
                }
            }
            else
            {
                return;
            }

            if (AddEditDeployment(DeploymentOperType.COPY, ref deploymentMS, ref deploymentPG))
            {
                SaveDeployment(deploymentMS, deploymentPG);
            }
        }

        /// <summary>Нажата кнопка "Удалить"</summary>
        private void btDel_Click(object sender, RoutedEventArgs e)
        {
            if (GridFocused != null && GridFocused.SelectedIndex >= 0)
            {
                Deployment deployment = GridFocused.SelectedItem as Deployment;

                if (System.Windows.Forms.MessageBox.Show($"Удалить действие {deployment.order} для задачи {deployment.task}?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    MainWindow.Task.ListDeploymentMS.Remove(deployment);
                    MainWindow.Task.ListDeploymentPG.Remove(deployment);
                    RefreshAndSave(null, true);
                }
            }
        }

        /// <summary>Сгенерировать JSON-файл для MS</summary>
        private void btGenJSON_MS_Click(object sender, RoutedEventArgs e)
        {
            // сгенерировать файл для MS
            tbMSJson.Text = Deployment.GenerateJSON(MainWindow.Task.ListDeploymentMS, MainWindow.Task.ReleaseVersion, logFile, MainWindow.Task.ReleaseVersion);
            tbMSJson.Filename = "";

            if (!string.IsNullOrEmpty(tbMSJson.Text) && isVisibleMSJson)
            {
                tiMS.IsSelected = true;
            }
        }

        /// <summary>Сгенерировать JSON-файл для PG</summary>
        private void btGenJSON_PG_Click(object sender, RoutedEventArgs e)
        {
            // сгенерировать файл для PG
            tbPGJson.Text = Deployment.GenerateJSON(MainWindow.Task.ListDeploymentPG, MainWindow.Task.ReleaseVersion, logFile, MainWindow.Task.ReleaseVersion);
            tbPGJson.Filename = "";

            if (!string.IsNullOrEmpty(tbPGJson.Text) && isVisiblePGJson)
            {
                tiPG.IsSelected = true;
            }
        }

        /// <summary>Двойной клик мышью на строке в dgDeployment</summary>
        private void dgDeployment_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            btEdit_Click(sender, e);
        }

        /// <summary>
        /// Сохранить действия в файл
        /// </summary>
        /// <param name="project">проект</param>
        /// <param name="BranchDefault">ветка</param>
        /// <param name="PathDefault">папка</param>
        /// <param name="FilenameDefault">файл</param>
        /// <param name="isBranchCanChanged">=true - можно изменить ветку</param>
        /// <param name="isFileCanChanged">=true - можно изменить имя файла</param>
        /// <param name="ListDeployment">список действий</param>
        /// <param name="jsonText">текст скрипта</param>
        /// <param name="isForce">=true - сохраняем принудительно, =false - сохраняем если изменился</param>
        /// <param name="jsonFilepath">итоговый путь к файлу</param>
        /// <param name="jsonUrl">url к файлу в web-репозитории</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="version">версия</param>
        /// <returns></returns>
        public static bool SaveJSON(string project, string BranchDefault, string PathDefault, string FilenameDefault, bool isBranchCanChanged, bool isFileCanChanged, ObservableCollection<Deployment> ListDeployment, ref string jsonText, bool isForce, out string jsonFilepath, out string jsonUrl, string logFile, string version)
        {
            jsonFilepath = "";
            jsonUrl = "";

            if (
                string.IsNullOrWhiteSpace(project) ||
                string.IsNullOrWhiteSpace(BranchDefault) ||
                string.IsNullOrWhiteSpace(PathDefault) ||
                string.IsNullOrWhiteSpace(FilenameDefault) ||
                ListDeployment == null 
            )
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = MainWindow.Task.LogFile;
            }

            // в режиме сборки версии всегда сохраняем принудительно
            if (!string.IsNullOrWhiteSpace(version))
            {
                isForce = true;
            }

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, PathDefault);
            bool isSaved = false;

            // сгенерим текст, если необходимо
            if (string.IsNullOrWhiteSpace(jsonText)) 
            {
                jsonText = Deployment.GenerateJSON(ListDeployment, BranchDefault, logFile, version);
            }

            jsonText = jsonText
                .Replace(Environment.NewLine, "\n")
                .TrimEndNewLine("\n");

            // проверим, изменился ли скрипт
            bool isChanged = false;
            if (!isForce)
            {
                isFileCanChanged = false;

                // текущее содержимое файла
                string filename = Path.Combine(folder, FilenameDefault);
                string jsonText_before = "";
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

                if (jsonText_before != jsonText)
                {
                    isChanged = (System.Windows.Forms.MessageBox.Show($"Deployment Plan для {BranchDefault} в проекте {project} изменился, сохранить в файл {PathDefault}\\{FilenameDefault}?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes);
                }
            }

            // сохраняем - если принудительно или если содержимое изменилось
            if (isForce || isChanged)
            {
                // сохранить json-файл
                isSaved = GIT.SaveFileToGIT(project, BranchDefault, PathDefault, FilenameDefault, isBranchCanChanged, false, isFileCanChanged, jsonText, out jsonFilepath, out jsonUrl, logFile, false, ".json", "(*.json)|*.json|Все файлы (*.*)|*.*", true);

                if (isSaved)
                {
                    if (!string.IsNullOrWhiteSpace(GITProjects.GetProjectDeployment("MS SQL", project)))
                    {
                        MainWindow.Task.JSONFilenameDeploymentMS = jsonUrl;
                    }
                    else if (!string.IsNullOrWhiteSpace(GITProjects.GetProjectDeployment("PG SQL", project)))
                    {
                        MainWindow.Task.JSONFilenameDeploymentPG = jsonUrl;
                    }
                }
            }

            return isSaved;
        }

         /// <summary>
        /// Сохранить скрипт действия при обновлении
        /// </summary>
        /// <param name="_dbregion">с какой вкладки сохраняем</param>
        /// <param name="isForce">=true - сохраняем принудительно, =false - сохраняем если изменился</param>
        private void Save(string _dbregion, bool isForce)
        {
            string jsonfile = "";
            string jsonurl = "";
            string _branch = "";
            string _path = "";
            string _file = "";
            string _script = "";
            string _project = "";
            ObservableCollection<Deployment> _list = null;
            bool isBranchCanChanged = false;
            bool isFileCanChanged = false;

            if (_dbregion == "MS SQL")
            {
                _file = Path.GetFileName(MainWindow.Task.JSONFilenameDeploymentMS);
                _script = tbMSJson.Text;
                _project = this.ProjectDeploymentMS;
                _list = MainWindow.Task.ListDeploymentMS;
            }
            else if (_dbregion == "PG SQL")
            {
                _file = Path.GetFileName(MainWindow.Task.JSONFilenameDeploymentPG);
                _script = tbPGJson.Text;
                _project = this.ProjectDeploymentPG;
                _list = MainWindow.Task.ListDeploymentPG;
            }

            if (!string.IsNullOrWhiteSpace(MainWindow.Task.ReleaseVersion))
            {
                _branch = MainWindow.Task.ReleaseVersion;
                _path = "version";
                isBranchCanChanged = false;
                isFileCanChanged = false;
            }
            else
            {
                _branch = MainWindow.Task.TaskNumber;
                _path = "deployment";
                isBranchCanChanged = true;
                isFileCanChanged = true;
            }

            bool isSaved = WinDeployment.SaveJSON(_project, _branch, _path, _file, isBranchCanChanged, isFileCanChanged, _list, ref _script, isForce, out jsonfile, out jsonurl, logFile, MainWindow.Task.ReleaseVersion);

            if (isSaved)
            {
                if (_dbregion == "MS SQL")
                {
                    tbMSJson.Text = _script;
                    tbMSJson.Filename = jsonfile;
                    MainWindow.Task.JSONFilenameDeploymentMS = jsonurl;
                }
                else if (_dbregion == "PG SQL")
                {
                    tbPGJson.Text = _script;
                    tbPGJson.Filename = jsonfile;
                    MainWindow.Task.JSONFilenameDeploymentPG = jsonurl;
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }
            }
        }

        /// <summary>Нажата кнопка Сохранить в файл на вкладке JSON для MS SQL</summary>
        private void btSaveMS_Click(object sender, RoutedEventArgs e)
        {
            Save("MS SQL", true);
        }

        /// <summary>Нажата кнопка Сохранить в файл на вкладке JSON для Postgres</summary>
        private void btSavePG_Click(object sender, RoutedEventArgs e)
        {
            Save("PG SQL", true);
        }

        /// <summary>Нажата кнопка В буфер обмена на вкладке JSON для MS SQL</summary>
        private void btClipboardMS_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbMSJson.Text);
        }

        /// <summary>Нажата кнопка В буфер обмена на вкладке JSON для Postgres</summary>
        private void btClipboardPG_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbPGJson.Text);
        }

        /// <summary>
        /// Нажата клавиша на вкладе Deployment Plan для MS SQL (Промед)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tiDeploymentMS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                btGenJSON_MS_Click(null, null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Нажата клавиша на вкладе Deployment Plan для PostgreSQL (ЕЦП)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tiDeploymentPG_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                btGenJSON_PG_Click(null, null);
                e.Handled = true;
            }
        }


        private void tiMS_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                btSaveMS_Click(sender, e);
                e.Handled = true;
            }
        }

        private void tiPG_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F8)
            {
                btSavePG_Click(sender, e);
                e.Handled = true;
            }
        }

        private void btUpMS_Click(object sender, RoutedEventArgs e)
        {
            if (!dgDeploymentMS.IsFocused) dgDeploymentMS.Focus();

            if (dgDeploymentMS.SelectedIndex >= 0)
            {
                // выбранная строка
                Deployment deployment = dgDeploymentMS.SelectedItem as Deployment;

                if (deployment != null)
                {
                    var prev_deployment = MainWindow.Task.ListDeploymentMS
                        .Where(x => x.order < deployment.order)
                        .OrderByDescending(x => x.order)
                        .FirstOrDefault();

                    if (prev_deployment != null)
                    {
                        // есть предыдущая строка
                        var prev_order = prev_deployment.order;
                        prev_deployment.order = deployment.order;
                        deployment.order = prev_order;
                    }
                    else
                    {
                        if (deployment.order > 1)
                        {
                            deployment.order--;
                        }
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgDeploymentMSRefresh();
            }
        }

        private void btUpPG_Click(object sender, RoutedEventArgs e)
        {
            if (!dgDeploymentPG.IsFocused) dgDeploymentPG.Focus();

            if (dgDeploymentPG.SelectedIndex >= 0)
            {
                // выбранная строка
                Deployment deployment = dgDeploymentPG.SelectedItem as Deployment;

                if (deployment != null)
                {
                    var prev_deployment = MainWindow.Task.ListDeploymentPG
                        .Where(x => x.order < deployment.order)
                        .OrderByDescending(x => x.order)
                        .FirstOrDefault();

                    if (prev_deployment != null)
                    {
                        // есть предыдущая строка
                        var prev_order = prev_deployment.order;
                        prev_deployment.order = deployment.order;
                        deployment.order = prev_order;
                    }
                    else
                    {
                        if (deployment.order > 1)
                        {
                            deployment.order--;
                        }
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgDeploymentPGRefresh();
            }
        }

        private void btDownMS_Click(object sender, RoutedEventArgs e)
        {
            if (!dgDeploymentMS.IsFocused) dgDeploymentMS.Focus();

            if (dgDeploymentMS.SelectedIndex >= 0)
            {
                // выбранная строка
                Deployment deployment = dgDeploymentMS.SelectedItem as Deployment;

                if (deployment != null)
                {
                    var next_deployment = MainWindow.Task.ListDeploymentMS
                        .Where(x => x.order > deployment.order)
                        .OrderBy(x => x.order)
                        .FirstOrDefault();

                    if (next_deployment != null)
                    {
                        // есть предыдущая строка
                        var next_order = next_deployment.order;
                        next_deployment.order = deployment.order;
                        deployment.order = next_order;
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgDeploymentMSRefresh();
            }
        }

        private void btDownPG_Click(object sender, RoutedEventArgs e)
        {
            if (!dgDeploymentPG.IsFocused) dgDeploymentPG.Focus();

            if (dgDeploymentPG.SelectedIndex >= 0)
            {
                // выбранная строка
                Deployment deployment = dgDeploymentPG.SelectedItem as Deployment;

                if (deployment != null)
                {
                    var next_deployment = MainWindow.Task.ListDeploymentPG
                        .Where(x => x.order > deployment.order)
                        .OrderBy(x => x.order)
                        .FirstOrDefault();

                    if (next_deployment != null)
                    {
                        // есть предыдущая строка
                        var next_order = next_deployment.order;
                        next_deployment.order = deployment.order;
                        deployment.order = next_order;
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgDeploymentPGRefresh();
            }
        }

        private void tiDeploymentMS_GotFocus(object sender, RoutedEventArgs e)
        {
            GridFocused = dgDeploymentMS;
        }

        private void tiDeploymentPG_GotFocus(object sender, RoutedEventArgs e)
        {
            GridFocused = dgDeploymentPG;
        }

        /// <summary>
        /// Загрузить JSON для MS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btLoadJSON_MS_Click(object sender, RoutedEventArgs e)
        {
            // выбрать json-файл
            if (
                Utilities.GIT.OpenJson(mainWindow, "MS SQL", out string jsonfile, logFile, false) &&
                !string.IsNullOrWhiteSpace(jsonfile) &&
                File.Exists(jsonfile)
            )
            {
                // загрузить json-файл
                Deployment.LoadJSON("MS SQL", MainWindow.Task.ListDeploymentMS, jsonfile, logFile, !string.IsNullOrWhiteSpace(MainWindow.Task.ReleaseVersion), true);

                dgDeploymentMSRefresh();
            }
        }

        /// <summary>
        /// Загрузить JSON для PG
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btLoadJSON_PG_Click(object sender, RoutedEventArgs e)
        {
            // выбрать json-файл
            if (
                Utilities.GIT.OpenJson(mainWindow, "PG SQL", out string jsonfile, logFile, false) &&
                !string.IsNullOrWhiteSpace(jsonfile) &&
                File.Exists(jsonfile)
            )
            {
                // загрузить json-файл
                Deployment.LoadJSON("PG SQL", MainWindow.Task.ListDeploymentPG, jsonfile, logFile, !string.IsNullOrWhiteSpace(MainWindow.Task.ReleaseVersion), true);

                dgDeploymentPGRefresh();
            }
        }

    }
}
