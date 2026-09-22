// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.SqlServer.Management.Smo.Agent;
using SQLGen.Controls;
using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace SQLGen
{
    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Окно для сборки JSON-файла со списком заданий
    /// </summary>
    public partial class WinJenkinsExec : Window
    {
        /// <summary>
        /// лог-файл
        /// </summary>
        string logFile;

        /// <summary>
        /// список заданий
        /// </summary>
        ObservableCollection<JenkinsJob> listJenkinsJob;

        /// <summary>
        /// фильтрация заданий
        /// </summary>
        private CollectionViewSource _filteredJenkins;

        /// <summary>
        /// фильтрация заданий
        /// </summary>
        private ICollectionView FilteredJenkins
        {
            get
            {
                if (_filteredJenkins != null)
                {
                    return _filteredJenkins.View;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// =true - строка входит в фильтр
        /// </summary>
        /// <param name="e">экземпляр YMLFileInfo</param>
        /// <returns></returns>
        public bool ShowOnlyFilter(object e)
        {
            var job = e as JenkinsJob;
            if (job != null)
            {
                if (job.isFiltered == true) 
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>Конструктор WinJenkinsExec</summary>
        public WinJenkinsExec(ObservableCollection<JenkinsJob> _list, string _logFile)
        {
            InitializeComponent();

            logFile = _logFile;

            listJenkinsJob = _list;
            if (listJenkinsJob == null)
            {
                listJenkinsJob = new ObservableCollection<JenkinsJob>();
            }

            cbFilterStand.ItemsSource = MainWindow.APPinfo.ListStands;
            cbFilterStand.SelectedItem = "ВСЕ";

            // сбросим флаги
            foreach (var item in listJenkinsJob)
            {
                //item.isFiltered = true;
                item.isExecuted = false;
            }

            // установить фильтр
            _filteredJenkins = new CollectionViewSource();
            _filteredJenkins.Source = listJenkinsJob;
            if (_filteredJenkins.View != null)
            {
                _filteredJenkins.View.Filter = delegate (object o) { return ShowOnlyFilter(o); };
            }
            if (_filteredJenkins.View != null && _filteredJenkins.View.CanSort == true)
            {
                _filteredJenkins.View.SortDescriptions.Clear();
                _filteredJenkins.View.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
            }

            dgList.ItemsSource = FilteredJenkins;

            dgListRefresh();

            // пользовательские настройки GUI
            Default.InitGUI(
                "WinJenkinsExec",
                this,
                mainGrid,
                null,
                null,
                null,
                logFile
                );
        }

        /// <summary>При открытии окна WinJenkinsExec</summary>
        private void winJenkinsExec_Activated(object sender, EventArgs e)
        {
        }

        /// <summary>При закрытии окна WinJenkinsExec</summary>
        private void winJenkinsExec_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI(
                "WinJenkinsExec",
                this,
                null
                );
        }

        /// <summary>
        /// обновление списка заданий
        /// </summary>
        public void JenkinsRefresh()
        {
            if (FilteredJenkins != null)
            {
                var lcv = (ListCollectionView)FilteredJenkins;
                if (lcv.IsAddingNew) lcv.CommitNew();
                if (lcv.IsEditingItem) lcv.CommitEdit();
                FilteredJenkins.Refresh();
            }

            // уникально перенумеруем
            int _cnt = 0;
            foreach (JenkinsJob job in listJenkinsJob
                .OrderBy(x => x.Order)
            )
            {
                _cnt++;
                job.Order = _cnt;
            }

            dgListSelect();
        }

        /// <summary>Обновить список dgList</summary>
        public void dgListRefresh()
        {
            JenkinsRefresh();
        }

        /// <summary>Выбрана запись в dgList </summary>
        private void dgListSelect()
        {
        }

        /// <summary>Выбрана запись в dgList</summary>
        private void dgList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgListSelect();
        }

        /// <summary>Двойной клик мышью на строке в dgList</summary>
        private void dgList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            //btEdit_Click(sender, e);
        }

        /// <summary>
        /// Нажата кнопка Up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btUp_Click(object sender, RoutedEventArgs e)
        {
            if (!dgList.IsFocused) dgList.Focus();

            if (dgList.SelectedCells.Count > 0)
            {
                var selectCell = dgList.SelectedCells[0];
                JenkinsJob job = selectCell.Item as JenkinsJob;

                if (job != null)
                {
                    var prev_job = listJenkinsJob
                        .Where(x => x.Order < job.Order)
                        .OrderByDescending(x => x.Order)
                        .FirstOrDefault();

                    if (prev_job != null)
                    {
                        // есть предыдущая строка
                        var prev_order = prev_job.Order;
                        prev_job.Order = job.Order;
                        job.Order = prev_order;
                    }
                    else
                    {
                        if (job.Order > 1)
                        {
                            job.Order--;
                        }
                    }
                }

                var column = selectCell.Column;

                dgListRefresh();

                // Исправленный возврат фокуса на ячейку
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var row = dgList.ItemContainerGenerator.ContainerFromItem(job) as DataGridRow;
                    if (row != null)
                    {
                        var cellContent = column.GetCellContent(row);
                        var cell = cellContent?.Parent as DataGridCell;
                        if (cell != null)
                        {
                            // 1. Очищаем старое выделение (если нужно, чтобы светилась только одна ячейка)
                            dgList.SelectedCells.Clear();

                            // 2. Создаем новую информацию о ячейке и добавляем в выделенные
                            DataGridCellInfo newCellInfo = new DataGridCellInfo(cell);
                            dgList.SelectedCells.Add(newCellInfo);

                            // 3. Возвращаем фокус клавиатуры на саму ячейку
                            cell.Focus();
                        }
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        /// <summary>
        /// Нажата кнопка Down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDown_Click(object sender, RoutedEventArgs e)
        {
            if (!dgList.IsFocused) dgList.Focus();

            if (dgList.SelectedCells.Count > 0)
            {
                var selectCell = dgList.SelectedCells[0];
                JenkinsJob job = selectCell.Item as JenkinsJob;

                if (job != null)
                {
                    var next_job = listJenkinsJob
                        .Where(x => x.Order > job.Order)
                        .OrderBy(x => x.Order)
                        .FirstOrDefault();

                    if (next_job != null)
                    {
                        // есть предыдущая строка
                        var next_order = next_job.Order;
                        next_job.Order = job.Order;
                        job.Order = next_order;
                    }
                }

                var column = selectCell.Column;

                dgListRefresh();

                // Исправленный возврат фокуса на ячейку
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    var row = dgList.ItemContainerGenerator.ContainerFromItem(job) as DataGridRow;
                    if (row != null)
                    {
                        var cellContent = column.GetCellContent(row);
                        var cell = cellContent?.Parent as DataGridCell;
                        if (cell != null)
                        {
                            // 1. Очищаем старое выделение (если нужно, чтобы светилась только одна ячейка)
                            dgList.SelectedCells.Clear();

                            // 2. Создаем новую информацию о ячейке и добавляем в выделенные
                            DataGridCellInfo newCellInfo = new DataGridCellInfo(cell);
                            dgList.SelectedCells.Add(newCellInfo);

                            // 3. Возвращаем фокус клавиатуры на саму ячейку
                            cell.Focus();
                        }
                    }
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
        }

        /// <summary>
        /// выполнить задания Jenkins
        /// </summary>
        /// <param name="fromOrder"></param>
        /// <param name="toOrder"></param>
        private void ExecuteJenkins(int? fromOrder, int? toOrder)
        {
            // список заданий для выполнения пачкой
            List<JenkinsJob> jobs = new List<JenkinsJob>();

            // находим первую НЕ выполненную задачу
            var first = listJenkinsJob
                .OrderBy(x => x.Order)
                .FirstOrDefault(x =>
                    x.ExecutionMode == true &&
                    x.isFiltered == true &&
                    x.isExecuted == false &&
                    (x.Order >= fromOrder || fromOrder == null)
                );

            if (first != null)
            {
                // собираем список для выполнения
                foreach (var item in listJenkinsJob
                    .Where(x =>
                        x.Order >= first.Order &&
                        (x.Order <= toOrder || toOrder == null) &&
                        x.isFiltered == true &&
                        x.isExecuted == false
                    )
                    .OrderBy(x => x.Order)
                )
                {
                    if (item.ExecutionMode == false)
                    {
                        // нашли паузу, завершаем
                        break;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(item.AliasName))
                        {
                            App.AddLog($"Пустой алиас в строке {item.Order}", null, App.ShowMessageMode.SHOW, true, logFile);
                            return;
                        }

                        var _alias = MainWindow.APPinfo.ListAliases
                            .Where(x => x.AliasName.ToLower() == item.AliasName.ToLower())
                            .FirstOrDefault();

                        if (_alias == null)
                        {
                            App.AddLog($"Алиас {item.AliasName} в строке {item.Order} не известен!!!", null, App.ShowMessageMode.SHOW, true, logFile);
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(item.FileName))
                        {
                            App.AddLog($"Пустое имя файла в строке {item.Order}", null, App.ShowMessageMode.SHOW, true, logFile);
                            return;
                        }

                        if (string.IsNullOrWhiteSpace(item.Branch))
                        {
                            App.AddLog($"Пустое имя ветки в строке {item.Order}", null, App.ShowMessageMode.SHOW, true, logFile);
                            return;
                        }

                        // добавляем задание в список для выполнения
                        jobs.Add(new JenkinsJob()
                        {
                            Order = item.Order,
                            JobName = _alias.JobName,
                            AliasName = _alias.AliasName,
                            FileName = item.FileName,
                            Branch = item.Branch,
                            ExecutionMode = true,
                            Stand = item.Stand
                        });
                    }
                }
            }

            if (jobs.Count > 0)
            {
                // выполняем
                //JenkinsCLI.Execute(jobs, true, logFile);

                // проставляем результат
                foreach (var job in jobs)
                {
                    //находим задание из общего списка
                    var found = listJenkinsJob.FirstOrDefault(x => x.Order == job.Order);

                    if (found != null)
                    {
                        found.isExecuted = true;// job.isExecuted;

                        App.AddLog($"Выполнено задание Jenkins - файл {found.FileName} алиас {found.AliasName} ветка {found.Branch}", null, App.ShowMessageMode.NONE, true, logFile);
                    }
                }
            }

            // еще остались не выполненные задания
            var no_exec = listJenkinsJob.FirstOrDefault(x => x.ExecutionMode == true && x.isExecuted == false);
            if (no_exec != null)
            {
                btRun.Content = "Продолжить выполнение";
            }
        }

        /// <summary>
        /// Нажата кнопка Выполнить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btRun_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show($"{btRun.Content} ?", "ВНИМАНИЕ!", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
            {
                ExecuteJenkins(null, null);
            }
        }

        /// <summary>
        /// Нажата кнопка Общий лог
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAllLog_Click(object sender, RoutedEventArgs e)
        {
            WinInfo WinInfo = new WinInfo(logFile);
            WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
            WinInfo.tbInfo.Text = File.ReadAllText(logFile);
            WinInfo.Title = "Лог в файле " + logFile;
            WinInfo.Show();
        }

        /// <summary>
        /// Выбран пункт контекстного меню "Снять флаг успешного выполнения"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCleanIsExecuted_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.SelectedCells.Count > 0)
            {
                var selectCell = dgList.SelectedCells[0];
                var _column = selectCell.Column;
                JenkinsJob job = selectCell.Item as JenkinsJob;

                if (job != null && job.isExecuted == true)
                {
                    MessageBoxResult res = MessageBox.Show($"Снять флаг успешного выполнения у задания {job.Order} (для повторного выполнения) ?", "ВНИМАНИЕ!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (res == MessageBoxResult.Yes)
                    {
                        job.isExecuted = false;

                        App.AddLog($"Снят флаг успешного выполнения задания {job.Order} файл {job.FileName} алиас {job.AliasName} из списка заданий Jenkins", null, App.ShowMessageMode.NONE, true, logFile);
                    }
                }
            }
        }

        /// <summary>
        /// Выбран пункт контекстного меню "Выполнять ПО это задание включительно"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btRunTo_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.SelectedCells.Count > 0)
            {
                var selectCell = dgList.SelectedCells[0];
                var _column = selectCell.Column;
                JenkinsJob job = selectCell.Item as JenkinsJob;

                if (job != null)
                {
                    MessageBoxResult res = MessageBox.Show($"Выполнять задания по {job.Order} включительно ?", "ВНИМАНИЕ!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (res == MessageBoxResult.Yes)
                    {
                        ExecuteJenkins(null, job.Order);
                    }
                }
            }
        }

        private void btRunFrom_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.SelectedCells.Count > 0)
            {
                var selectCell = dgList.SelectedCells[0];
                var _column = selectCell.Column;
                JenkinsJob job = selectCell.Item as JenkinsJob;

                if (job != null)
                {
                    MessageBoxResult res = MessageBox.Show($"Выполнять задания начиная с {job.Order} включительно ?", "ВНИМАНИЕ!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (res == MessageBoxResult.Yes)
                    {
                        ExecuteJenkins(job.Order, null);
                    }
                }
            }
        }

        private void dgList_CurrentCellChanged(object sender, EventArgs e)
        {

        }

        private void dgList_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void dgList_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }

        private void dgList_PreviewKeyDown(object sender, KeyEventArgs e)
        {

        }

        /// <summary>
        /// Выбран пункт контекстного меню "Удалить строку"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDelRow_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.SelectedCells.Count > 0)
            {
                var selectCell = dgList.SelectedCells[0];
                var _column = selectCell.Column;
                JenkinsJob job = selectCell.Item as JenkinsJob;

                if (job != null)
                {
                    MessageBoxResult res = MessageBox.Show($"Удалить задание {job.Order} ?", "ВНИМАНИЕ!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (res == MessageBoxResult.Yes)
                    {
                        listJenkinsJob.Remove(job);

                        dgListRefresh();

                        App.AddLog($"Удалено задание {job.Order} файл {job.FileName} алиас {job.AliasName} из списка заданий Jenkins", null, App.ShowMessageMode.NONE, true, logFile);
                    }
                }
            }
        }

        /// <summary>
        /// Нажата кнопка Удалить ВСЕ лишние пункты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDelBad_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult res = MessageBox.Show($"Удалить ВСЕ лишние пункты (пустые алиасы или ветки) ?", "ВНИМАНИЕ!", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
            {
                JenkinsJob job = null;
                do
                {
                    job = listJenkinsJob
                        .FirstOrDefault(x =>
                            x.isFiltered == true && 
                            (
                                string.IsNullOrWhiteSpace(x.JobName) ||
                                string.IsNullOrWhiteSpace(x.FileName) ||
                                string.IsNullOrWhiteSpace(x.AliasName) ||
                                string.IsNullOrWhiteSpace(x.Branch)
                            )
                        );

                    if (job != null)
                    {
                        listJenkinsJob.Remove(job);

                        App.AddLog($"Удалено задание {job.Order} файл {job.FileName} алиас {job.AliasName} из списка заданий Jenkins", null, App.ShowMessageMode.NONE, true, logFile);
                    }
                }
                while (job != null);

                dgListRefresh();
            }
        }

        /// <summary>
        /// Нажата кнопка Скопировать в буфер ВСЕ лишние пункты
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCopyBad_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder result = new StringBuilder(100000);

            string vers = "";

            foreach (var job in listJenkinsJob
                .Where(x =>
                    x.isFiltered == true &&
                    (
                        string.IsNullOrWhiteSpace(x.JobName) ||
                        string.IsNullOrWhiteSpace(x.FileName) ||
                        string.IsNullOrWhiteSpace(x.AliasName) ||
                        string.IsNullOrWhiteSpace(x.Branch)
                    )
                )
                .OrderBy(x => x.Order)
             )
            {
                if (vers != job.Version)
                {
                    result.Append(Environment.NewLine + job.Version + Environment.NewLine);
                }

                result.Append(Environment.NewLine + job.FileName + Environment.NewLine);

                vers = job.Version;
            }

            System.Windows.Clipboard.SetText(result.ToString()
                            .TrimInnerNewLine()
                            .TrimAllSpace()
                            );
        }

        /// <summary>
        /// Выбран Фильтр по стенду
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFilterStand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFilterStand.SelectedIndex >= 0)
            {
                string stand = cbFilterStand.SelectedItem.ToString().Trim();

                foreach (var item in listJenkinsJob)
                {
                    item.isFiltered = false;

                    if (stand == "ВСЕ")
                    {
                        item.isFiltered = true;
                    }
                    else 
                    {
                        item.isFiltered = (stand == item.Stand);
                    }
                }

                dgListRefresh();
            }
        }
    }
}
