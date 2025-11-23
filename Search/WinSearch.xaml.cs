// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Path = System.IO.Path;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Окно для поиска информации
    /// </summary>
    public partial class WinSearch : Window
    {

        /// <summary>Подключение к БД</summary>
        public ConnectDB ConnectSQL = null;

        /// <summary>Проект GIT</summary>
        public string GITProject = "";

        /// <summary>Флаг инициализации окна</summary>
        public bool isStart = true;

        /// <summary>Экземпляр SearchInfo, с которым работаем в данном окне</summary>
        SearchInfo SearchInfo;

        /// <summary>
        /// Поисковый запрос
        /// </summary>
        public string SearchSQL = "";

        /// <summary>
        /// Результаты поиска
        /// </summary>
        public DataTable SearchData;

        /// <summary>
        /// Конструктор WinSearch
        /// </summary>
        /// <param name="_searchinfo">данные поиска</param>
        public WinSearch(SearchInfo _searchinfo)
        {
            InitializeComponent();
            this.SearchInfo = _searchinfo;

            if (this.SearchInfo != null)
            {
                tbSearch.InitHistory("HistorySearch.json", SearchInfo.Tag);
                Title = SearchInfo.Name;
            }
            dgSearchGrid.MaxColumnWidth = 500;

            // пользовательские настройки GUI
            Default.InitGUI("WinSearch", this, mainGrid, null, null, null, MainWindow.Task.LogFile);
        }

        /// <summary>При активации окна WinSearch</summary>
        private void winSearch_Activated(object sender, EventArgs e)
        {
        }

        /// <summary>При закрытии окна WinSearch</summary>
        private void winSearch_Closed(object sender, EventArgs e)
        {
            if (ConnectSQL != null)
            {
                ConnectSQL.CloseConnect();
            }

            // пользовательские настройки GUI
            Default.SaveGUI("WinSearch", this, null);
        }

        /// <summary>
        /// Переоткрытие основного подключения формы
        /// </summary>
        /// <returns>=true - успешное переподключение</returns>
        private bool CheckConnectSQL()
        {
            if (ConnectSQL == null)
            {
                ConnectSQL = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, false);
            }

            if (ConnectSQL.isNotConnected)
            {
                ConnectSQL.ReConnect();
            }

            return ConnectSQL.isConnected;
        }


        /// <summary>
        /// Смена подключения к БД
        /// </summary>
        public void cbConnectSQLChanged()
        {
            if (ConnectSQL != null)
            {
                ConnectSQL.CloseConnect();
            }

            ConnectSQL = Utilities.Controls.SetConnectFromComboBox(cbConnectSQL);
        }

        /// <summary>
        /// Выбор подключения
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void cbConnectSQL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isStart) cbConnectSQLChanged();
        }

        /// <summary>Нажата кнопка "Добавить подключение" на вкладке Таблица</summary>
        private void btAddConnectSQL_Click(object sender, RoutedEventArgs e)
        {
            var connect = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, true);

            if (connect.ConnType != Utilities.ConnType.None)
            {
                Utilities.Controls.RefreshConnectItems(cbConnectSQL, connect.DBConnectionName, null, null);
            }

            connect.CloseConnect();
        }

        /// <summary>Изменился проект GIT</summary>
        public void cbGITProjectChanged()
        {
            ComboBoxItem cbItem = null;
            if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
            if ((cbItem == null) || (cbItem.Tag == null)) GITProject = "";
            else GITProject = cbItem.Tag.ToString();

            if (!string.IsNullOrWhiteSpace(GITProject))
            {
                string branch = GIT.GitCurrentBranch(GITProject, out string err, MainWindow.Task.LogFile);
                lbGITBranch.Content = "Текущая ветка GIT: " + branch.Replace("_", "__");
            }
            else
            {
                lbGITBranch.Content = "Текущая ветка GIT:";
            }
        }

        /// <summary>Выбран проект GIT</summary>
        private void cbGITProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbGITProjectChanged();
        }

        /// <summary>Обновить результаты поиска</summary>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        private void dgSearchGridRefresh(bool isForcedGitRefresh)
        {
            this.Cursor = Cursors.Wait;
            dgSearchGrid.ItemsSource = null;
            SearchData = null;

            string search = tbSearch.Text.Trim();
            string withautogen = "1";
            if (cbWithAutogen.IsChecked == true) withautogen = "2";
            string withregistry = "1";
            if (cbWithRegistry.IsChecked == true) withregistry = "2";

            if (SearchInfo != null)
            {
                if (SearchInfo.Type == "SQL")
                {
                    if (!CheckConnectSQL())
                    {
                        cbConnectSQL.Focus();
                        MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                        return;
                    }

                    if (SearchInfo.DBType == "MSSQL")
                    {
                        search = search.Replace("_", "[_]");
                    }
                    else
                    {
                        search = search.Replace("_", @"\_");
                    }

                    if (
                        (SearchInfo.Tag == "FK_DEPEND") ||
                        (SearchInfo.Tag == "FK_ONDEPEND")
                        )
                    {
                        // для поиска FK нужно точное и полное название таблицы
                        search = Utilities.Databases.GetFullTableName(search);
                    }

                    try
                    {
                        SearchSQL = SearchInfo.Text
                            .Trim()
                            .Replace("%%SEARCH%%", search)
                            .Replace("%%WITHAUTOGEN%%", withautogen)
                            .Replace("%%WITHREGISTRY%%", withregistry)
                            ;

                        SearchData = ConnectSQL.FillDataTable(SearchSQL, out string Messages);

                        if (SearchData != null) //-V3022
                        {
                            dgSearchGrid.ItemsSource = SearchData.DefaultView;
                        }

                    }
                    catch (Exception ex)
                    {
                        App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                }

                if (SearchInfo.Type == "GIT")
                {
                    // выбранный проект
                    ComboBoxItem cbItem = null;
                    string GITProject = "";
                    if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
                    if ((cbItem == null) || (cbItem.Tag == null)) GITProject = "";
                    else GITProject = cbItem.Tag.ToString();

                    if (!string.IsNullOrWhiteSpace(GITProject))
                    {
                        // текущая ветка
                        string err = "";
                        string current_branch = GIT.GitCurrentBranch(GITProject, out err, MainWindow.Task.LogFile);
                        string GITProjectFolder = Utilities.GITProjects.GetFolderByProject(GITProject);
                        string GITPath = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder);

                        // переключение на выбранную ветку
                        if (GIT.SelectGITBranch(GITProject, current_branch, out string branch, MainWindow.Task.LogFile, true, isForcedGitRefresh))
                        {
                            lbGITBranch.Content = "Текущая ветка GIT: " + branch.Replace("_", "__");

                            // начинаем поиск
                            this.Cursor = Cursors.Wait;
                            try
                            {
                                List<RowInfo> list = MainWindow.SearchInGIT(SearchInfo.Tag, cbWithRegistry.IsChecked == true, true, true, GITProject, GITPath, tbFolder.Text, search);

                                // отображаем результат поиска
                                SearchData = Utilities.Databases.ConvertToDataTable(list);

                                if (SearchData != null) //-V3022
                                {
                                    dgSearchGrid.ItemsSource = SearchData.DefaultView;
                                }
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                            }
                        }
                    }
                }
            }
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>Нажата кнопка Искать</summary>
        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbSearch.Text))
            {
                MessageBox.Show("Необходимо заполнить значение для поиска");
                tbSearch.Focus();
                return;
            }

            dgSearchGridRefresh(false);

            // Добавить в историю
            if (this.SearchInfo != null)
            {
                tbSearch.AddHistory(tbSearch.Text);
            }

        }

        /// <summary>Нажата клавиша Enter в поле tbSearch на вкладке Список хранимок</summary>
        private void tbSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btSearch.Focus();
                btSearch_Click(sender, e);
                e.Handled = true;
            }
        }


        private void ViewText()
        {
            if (
                (SearchInfo != null) &&
                (SearchData != null) &&
                (dgSearchGrid.SelectedCells != null) &&
                (dgSearchGrid.SelectedCells.Count > 0)

               )
            {
                // находим выбранную строку и выбранную ячейку
                DataRow currentrow = null;
                DataGridCellInfo currentcell = dgSearchGrid.SelectedCells[0];
                if (currentcell.IsValid)
                {
                    //GetCellContent returns FrameworkElement
                    var content = currentcell.Column.GetCellContent(currentcell.Item);

                    //get the datacontext from FrameworkElement and typecast to DataRowView
                    var rowview = (DataRowView)content.DataContext;
                    if (rowview != null)
                    {
                        currentrow = rowview.Row;
                    }
                }

                // просмотр текста хранимки или вьюхи
                if (
                    (SearchInfo.Type == "SQL") &&
                    (SearchInfo.Tag == "VIEWTEXT" || SearchInfo.Tag == "PROCTEXT" || SearchInfo.Tag == "ALTEROBJECTLOG") &&
                    (!string.IsNullOrWhiteSpace(SearchInfo.FieldText)) &&
                    SearchData.Columns.Contains(SearchInfo.FieldText)
                )
                {
                    // определяем колонку с полем, содержащим текст
                    int ColumnNum = -1;
                    for (int i = 0; i < SearchData.Columns.Count; i++)
                    {
                        if (SearchData.Columns[i].ColumnName.ToLower().Trim() == SearchInfo.FieldText.ToLower().Trim())
                        {
                            ColumnNum = i;
                            break;
                        }
                    }

                    // открываем окно с текстом
                    if (
                        (currentrow != null) &&
                        (ColumnNum > 0) &&
                        (currentrow.ItemArray.Length > ColumnNum)
                    )
                    {
                        string value = (string)currentrow.ItemArray[ColumnNum];
                        WinInfo WinInfo = new WinInfo(null);
                        WinInfo.Title = "Просмотр";
                        WinInfo.tbInfo.Text = value;
                        WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL");
                        WinInfo.Show();
                    }
                }


                // открыть таблицу
                if (
                    (SearchInfo.Type == "SQL") &&
                    (
                        (SearchInfo.Tag == "TABLE") ||
                        (SearchInfo.Tag == "FK_DEPEND") ||
                        (SearchInfo.Tag == "FK_ONDEPEND")
                    ) &&
                    (!string.IsNullOrWhiteSpace(SearchInfo.FieldTable)) &&
                    SearchData.Columns.Contains(SearchInfo.FieldTable)
                )
                {
                    // определяем колонку с полем, содержащим имя таблицы
                    int ColumnNum = -1;
                    for (int i = 0; i < SearchData.Columns.Count; i++)
                    {
                        if (SearchData.Columns[i].ColumnName.ToLower().Trim() == SearchInfo.FieldTable.ToLower().Trim())
                        {
                            ColumnNum = i;
                            break;
                        }
                    }

                    // открываем таблицу
                    if (
                        (currentrow != null) &&
                        (ColumnNum > 0) &&
                        (currentrow.ItemArray.Length > ColumnNum)
                    )
                    {
                        string tablename = (string)currentrow.ItemArray[ColumnNum];
                        if (!string.IsNullOrWhiteSpace(tablename))
                        {
                            WinTable WinTable = new WinTable();

                            string project = "";

                            if (cbConnectSQL != null)
                            {
                                Utilities.Controls.RefreshConnectItems(WinTable.cbConnectSQL, ConnectSQL.DBConnectionName, null, null);
                                project = ConnectSQL.GITProject;
                            }

                            Utilities.Controls.RefreshRegionItems(WinTable.cbRegion, project);

                            WinTable.isStart = false;
                            WinTable.cbConnectSQLChanged();

                            WinTable.Show();

                            WinTable.tbTableName.Text = tablename;
                            WinTable.tbTableName_LostFocus(null, null);
                            WinTable.btFillFromDB_Click(null, null);
                        }
                    }
                }

                // выбранный проект
                ComboBoxItem cbItem = null;
                string GITProject = "";
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
                if ((cbItem == null) || (cbItem.Tag == null)) GITProject = "";
                else GITProject = cbItem.Tag.ToString();

                // просмотр текста скрипта из GIT
                if (
                    (SearchInfo.Type == "GIT") &&
                    (!string.IsNullOrWhiteSpace(GITProject))
                )
                {
                    string content = Utilities.Controls.GetSelectedValue(dgSearchGrid, dgSearchGrid.CurrentColumn.DisplayIndex);
                    string file = "";
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        if (content.EndsWith(".sql"))
                        {
                            string folder = Utilities.GITProjects.GetFolderByProject(GITProject);
                            file = Path.Combine(MainWindow.APPinfo.GITFolder, folder, content.Replace('/', Path.DirectorySeparatorChar));
                        }

                        if (content.EndsWith(".yml"))
                        {
                            string folder = Utilities.GITProjects.GetFolderByProject(GITProject);
                            file = Path.Combine(MainWindow.APPinfo.GITFolder, folder, "version", content);
                            if (!File.Exists(file))
                            {
                                file = Path.Combine(MainWindow.APPinfo.GITFolder, folder, "task", content);
                                if (!File.Exists(file))
                                {
                                    file = Path.Combine(MainWindow.APPinfo.GITFolder, folder, "Report", content);
                                }
                            }
                        }

                        if (content.EndsWith(".json"))
                        {
                            string folder = Utilities.GITProjects.GetFolderByProject(GITProject);
                            file = Path.Combine(MainWindow.APPinfo.GITFolder, folder, "version", content);
                            if (!File.Exists(file))
                            {
                                file = Path.Combine(MainWindow.APPinfo.GITFolder, folder, "deployment", content);
                                if (!File.Exists(file))
                                {
                                    file = Path.Combine(MainWindow.APPinfo.GITFolder, folder, "cron", content.Replace('/', Path.DirectorySeparatorChar));
                                }
                            }
                        }
                    }

                    if (
                        (!string.IsNullOrWhiteSpace(file)) &&
                        File.Exists(file)
                        )
                    {
                        WinInfo WinInfo = new WinInfo(null);
                        WinInfo.Title = file;
                        WinInfo.tbInfo.Text = File.ReadAllText(file);
                        WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL");
                        WinInfo.Show();
                    }
                }
            }
        }

        /// <summary>Просмотр</summary>
        private void btView_Click(object sender, RoutedEventArgs e)
        {
            ViewText();
        }

        private void dgSearchGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewText();
        }

        /// <summary>Просмотр текста поискового запроса</summary>
        private void btViewSQL_Click(object sender, RoutedEventArgs e)
        {
            if (SearchInfo != null)
            {
                if (SearchInfo.Type == "SQL")
                {
                    WinInfo WinInfo = new WinInfo(null);
                    WinInfo.Title = "SQL-запрос";
                    WinInfo.tbInfo.Text = SearchSQL;
                    WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL");
                    WinInfo.Show();
                }
            }
        }

        private void SearhChanged()
        {
        }

        /// <summary>
        /// Закрылся список истории поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbSearch_DropDownClosed(object sender, EventArgs e)
        {
            SearhChanged();
        }

        /// <summary>
        /// Вышел из поля поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbSearch_LostFocus(object sender, RoutedEventArgs e)
        {
            SearhChanged();
        }

        /// <summary>
        /// выбрано значение из списка с историей поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbSearch.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbSearch.SelectedItem;

                tbSearch.Text = (string)cbItem.Tag;
            }
        }

        /// <summary>
        /// кнопка Выгрузить в Excel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.Cursor = Cursors.Wait;
                Utilities.MSOffice.GenerateExcel(SearchData, false, "", true);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Нажат Enter в поле Фильтр
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btFilter_Click(sender, e);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Нажата кнопка "Фильтр"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFilter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbFilter.Text))
            {
                // сбросим фильтр
                SearchData.DefaultView.RowFilter = "";
                dgSearchGrid.ItemsSource = SearchData.DefaultView;
            }
            else
            {
                // поставим фильтр
                string filter = "";
                for (int i = 0; i < SearchData.Columns.Count; i++)
                {
                    if (SearchData.Columns[i].DataType == Type.GetType("System.String"))
                    {
                        if (filter != "")
                        {
                            filter += " OR ";
                        }

                        filter += SearchData.Columns[i].ColumnName + " LIKE '%" + tbFilter.Text + "%'";
                    }
                }

                SearchData.DefaultView.RowFilter = filter;
                dgSearchGrid.ItemsSource = SearchData.DefaultView;
            }
        }

        private void btViewText_Click(object sender, RoutedEventArgs e)
        {
            btViewSQL_Click(null, null);
        }

        private void btFolder_Click(object sender, RoutedEventArgs e)
        {
            // выбранный проект
            ComboBoxItem cbItem = null;
            string GITProject = "";
            if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
            if ((cbItem == null) || (cbItem.Tag == null)) GITProject = "";
            else GITProject = cbItem.Tag.ToString();

            string dir = Controls.Dialogs.FolderBrowserDialog(Path.Combine(MainWindow.APPinfo.GITFolder, GITProject));
            if (!string.IsNullOrWhiteSpace(dir))
            {
                tbFolder.Text = dir;
            }
        }
    }

    /// <summary>
    /// Строка в результатах поиска
    /// </summary>
    public class RowInfo
    {
        /// <summary>
        /// компоненты маршрута к файлу
        /// </summary>
        private string[] arr => this.sql.Trim().Replace('/', Path.DirectorySeparatorChar).Split(Path.DirectorySeparatorChar);


        /// <summary>
        /// yml\json-файл версии
        /// </summary>
        public string version { get; set; } = "";

        /// <summary>
        /// yml\json-файл задачи
        /// </summary>
        public string task { get; set; } = "";

        /// <summary>
        /// тип скрипта
        /// </summary>
        public string objecttype
        {
            get
            {
                if (arr.Length > 0)  //-V3022
                {
                    string t0 = arr[0].Trim().ToUpper();

                    if (
                        (t0 == "DATA") ||
                        (t0 == "DATA_NEW") ||
                        (t0 == "DATA(COPY)") ||
                        (t0 == "DATA(BULK)")
                        )
                    {
                        return "DATA";
                    }

                    if (t0 == "TASK")
                    {
                        return "TASK";
                    }

                    if (t0 == "DEPLOYMENT")
                    {
                        return "DEPLOYMENT";
                    }

                    if (t0 == "CRON")
                    {
                        return "CRON";
                    }

                    if (t0 == "VERSION")
                    {
                        return "VERSION";
                    }

                    if (t0 == "REPORT")
                    {
                        return "REPORT";
                    }
                }

                if (arr.Length > 1)
                {
                    string t0 = arr[0].Trim().ToUpper();
                    string t1 = arr[1].Trim().ToUpper();

                    if (t1 == "PROCEDURE") return "PROCEDURE";
                    if (t1 == "TABLE") return "TABLE";
                    if (t1 == "VIEW") return "VIEW";
                    if (t1 == "TRIGGER") return "TRIGGER";
                    if (t1 == "FUNCTION") return "FUNCTION";
                    if (t1 == "SEQUENCE") return "SEQUENCE";
                    if (t1 == "TYPE") return "TYPE";
                    if (
                        (Path.GetExtension(t1) == ".SQL") &&
                        t1.StartsWith(t0)
                        )
                    {
                        return "SCHEMA";
                    }
                }

                return "UNKNOWN";
            }
        }

        /// <summary>
        /// схема
        /// </summary>
        public string objectschema
        {
            get
            {
                if (
                    (arr.Length == 0) || //-V3063
                    (objecttype == "DATA") ||
                    (objecttype == "VERSION") ||
                    (objecttype == "TASK") ||
                    (objecttype == "DEPLOYMENT") ||
                    (objecttype == "CRON") ||
                    (objecttype == "REPORT") ||
                    (objecttype == "UNKNOWN")
                    )
                {
                    return "";
                }
                else
                {
                    string t0 = arr[0].Trim();

                    if (string.IsNullOrWhiteSpace(Path.GetExtension(t0)))
                    {
                        return t0;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

        /// <summary>
        /// имя объекта
        /// </summary>
        public string objectname
        {
            get
            {
                if (
                    (arr.Length > 2) &&
                    (
                        (objecttype == "PROCEDURE") ||
                        (objecttype == "TABLE") ||
                        (objecttype == "VIEW") ||
                        (objecttype == "TRIGGER") ||
                        (objecttype == "FUNCTION") ||
                        (objecttype == "SEQUENCE") ||
                        (objecttype == "TYPE")
                    )
                )
                {
                    string t2 = arr[2].Trim();
                    return Utilities.Databases.GetTableName(Path.GetFileNameWithoutExtension(t2)).Trim();
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// sql-файл
        /// </summary>
        public string sql { get; set; } = "";
    }


    /// <summary>
    /// Главное окно программы
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// Поиск
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSearch_Click(object sender, RoutedEventArgs e)
        {
            Search.LoadSearches();

            // выбрать вариант поиска
            FormFindInList dlg1 = new FormFindInList(MainWindow.Task.LogFile);
            dlg1.Text = "Выбор варианта поиска";

            foreach (var item in Search.ListSearches
                .OrderBy(x => x.Name)
                )
            {
                dlg1.AddItem(item.Name);
            }

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {

                foreach (System.Windows.Forms.DataGridViewRow row in dlg1.dgListValues.SelectedRows)
                {
                    string name = row.Cells[0].Value.ToString();

                    SearchInfo SearchInfo = Search.ListSearches.Find(x => x.Name == name);

                    if (SearchInfo != null)
                    {
                        WinSearch WinSearch = new WinSearch(SearchInfo);
                        string _dbconnname = "";
                        string _dbconngit = "";

                        if (MainConnect != null)
                        {
                            _dbconnname = MainConnect.DBConnectionName;
                            _dbconngit = MainConnect.GITProject;
                        }

                        Utilities.Controls.RefreshConnectItems(WinSearch.cbConnectSQL, _dbconnname, null, SearchInfo.ConnType);
                        Utilities.Controls.RefreshGITProjectItems(WinSearch.cbGITProject, _dbconngit);

                        WinSearch.isStart = false;

                        WinSearch.cbConnectSQLChanged();
                        WinSearch.cbGITProjectChanged();

                        if (SearchInfo.Type == "SQL")
                        {
                            WinSearch.lbGITProject.Visibility = Visibility.Hidden;
                            WinSearch.cbGITProject.Visibility = Visibility.Hidden;
                            WinSearch.lbGITBranch.Visibility = Visibility.Hidden;

                            if (
                                SearchInfo.Tag.StartsWith("FK_") ||
                                SearchInfo.Tag.StartsWith("ALTEROBJECTLOG")
                                )
                            {
                                WinSearch.cbWithAutogen.Visibility = Visibility.Hidden;
                                WinSearch.cbWithRegistry.Visibility = Visibility.Hidden;
                            }
                            WinSearch.tbFolder.Visibility = Visibility.Hidden;
                            WinSearch.btFolder.Visibility = Visibility.Hidden;
                        }
                        else
                        {
                            WinSearch.lbConnectSQL.Visibility = Visibility.Hidden;
                            WinSearch.cbConnectSQL.Visibility = Visibility.Hidden;
                            WinSearch.cbWithAutogen.Visibility = Visibility.Hidden;
                            WinSearch.btViewText.Visibility = Visibility.Hidden;
                        }

                        WinSearch.Show();
                    }

                    // берем только первую
                    break; //-V3020
                }
            }
            dlg1.Dispose();
        }


        /// <summary>
        /// Поиск файла в GIT
        /// </summary>
        /// <param name="SearchInfoTag">тип поиска STRUCTNAME или STRUCTCONTENT</param>
        /// <param name="isWithRegistry">=true добавлять файлы, у которых в названии есть Registry</param>
        /// <param name="isAddVersion">=true искать версию, в которой есть файл</param>
        /// <param name="isAddTask">=true искать задачу, в которой есть файл</param>
        /// <param name="GITProject">проект GIT</param>
        /// <param name="GITPath">полный путь к папке проекта GIT</param>
        /// <param name="folder">путь к папке внутри проекта GIT, в которой ищем</param>
        /// <param name="search">искомая строка</param>
        /// <returns></returns>

        public static List<RowInfo> SearchInGIT(
            string SearchInfoTag,
            bool isWithRegistry,
            bool isAddVersion,
            bool isAddTask,
            string GITProject,
            string GITPath,
            string folder,
            string search
            )
        {
            List<RowInfo> list = new List<RowInfo>();
            List<string> files = new List<string>();

            string dir = folder;
            dir = dir.Replace(GITPath + Path.DirectorySeparatorChar, "");
            dir = dir.Replace(GITPath, "");

            if (SearchInfoTag == "STRUCTNAME")
            {
                // Поиск в именах файлов
                files = GIT.GitFind(GITProject, dir, search, MainWindow.Task.LogFile)
                    .Where(x => isWithRegistry == true || !x.ToLower().Contains("registry"))
                    .OrderBy(x => x)
                    .ToList();
            }

            if (SearchInfoTag == "STRUCTCONTENT")
            {
                // Поиск в содержимом файлов
                files = GIT.GitGrep(GITProject, dir, search, MainWindow.Task.LogFile)
                    .Where(x => isWithRegistry == true || !x.ToLower().Contains("registry"))
                    .OrderBy(x => x)
                    .ToList();
            }

            // перебираем найденные документы
            foreach (var file in files)
            {
                if (
                    file.ToLower().StartsWith("version/") &&
                    file.ToLower().EndsWith(".yml")
                    )
                {
                    if (isAddVersion)
                    {
                        // Если это yml-файл из version 
                        list.Add(new RowInfo() { version = Path.GetFileName(file), task = "", sql = "" });
                    }
                }

                if (
                    file.ToLower().StartsWith("version/") &&
                    file.ToLower().EndsWith(".json")
                    )
                {
                    if (isAddVersion)
                    {
                        // Если это json-файл из version 
                        list.Add(new RowInfo() { version = Path.GetFileName(file), task = "", sql = "" });
                    }
                }

                if (
                    file.ToLower().StartsWith("task/") &&
                    file.ToLower().EndsWith(".yml")
                    )
                {
                    if (isAddTask)
                    {
                        bool inVersion = false;

                        if (isAddVersion)
                        {
                            // Если это yml-файл из task - ищем в каких версиях есть задача
                            List<string> versions = GIT.GitGrep(GITProject, "version", file, MainWindow.Task.LogFile);

                            // перебираем yml-файлы версий
                            foreach (var item in versions)
                            {
                                inVersion = true;
                                list.Add(new RowInfo() { version = Path.GetFileName(item), task = Path.GetFileName(file), sql = "" });
                            }
                        }

                        if (!inVersion)
                        {
                            list.Add(new RowInfo() { version = "", task = Path.GetFileName(file), sql = "" });
                        }
                    }
                }

                if (
                    file.ToLower().StartsWith("deployment/") &&
                    file.ToLower().EndsWith(".json")
                    )
                {
                    if (isAddTask)
                    {
                        list.Add(new RowInfo() { version = "", task = Path.GetFileName(file), sql = "" });
                    }
                }

                if (
                    file.ToLower().StartsWith("cron/") &&
                    file.ToLower().EndsWith(".json")
                    )
                {
                    if (isAddTask)
                    {
                        list.Add(new RowInfo() { version = "", task = Path.GetFileName(file), sql = "" });
                    }
                }

                if (file.ToLower().EndsWith(".sql"))
                {
                    bool inTask = false;

                    if (isAddTask)
                    {
                        // Если это sql-файл - ищем в какие yml-файлы задач он входит
                        List<string> tasks = GIT.GitGrep(GITProject, "task", file, MainWindow.Task.LogFile);

                        // перебираем yml-файлы задач
                        foreach (var task in tasks)
                        {
                            // Если это yml-файл из task - ищем в каких версиях есть задача
                            inTask = true;
                            bool inVersion = false;

                            if (isAddTask) //-V3022
                            {
                                List<string> versions = GIT.GitGrep(GITProject, "version", task, MainWindow.Task.LogFile);

                                // перебираем yml-файлы версий
                                foreach (var item in versions)
                                {
                                    inVersion = true;
                                    list.Add(new RowInfo() { version = Path.GetFileName(item), task = Path.GetFileName(task), sql = file });
                                }
                            }

                            if (!inVersion)
                            {
                                list.Add(new RowInfo() { version = "", task = Path.GetFileName(task), sql = file });
                            }
                        }
                    }

                    if (!inTask)
                    {
                        list.Add(new RowInfo() { version = "", task = "", sql = file });
                    }
                }
            }

            return list;
        }

    }

}