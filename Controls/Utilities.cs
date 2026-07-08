// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using AngleSharp.Dom;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace SQLGen.Utilities
{
    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Вспомогательные функции для работы с элементами интерфейса
    /// </summary>
    public static class Controls
    {
        /// <summary>
        /// Выбрать в ComboBox подключение к БД по наименованию
        /// </summary>
        /// <param name="cb">экземпляр System.Windows.Controls.ComboBox</param>
        /// <param name="connname">наименование подключения</param>
        public static void SetComboBoxConnectByName(ComboBox cb, string connname)
        {
            if ((cb != null) && (cb.Items != null))
            {
                foreach (ComboBoxItem item in cb.Items)
                {
                    if ((item != null) && (item.Content != null) && (item.Content.ToString() == connname))
                    {
                        cb.SelectedItem = item;
                    }
                }
            }
        }

        /// <summary>
        /// Выбрать в ComboBox подключение к основной тестовой БД по TargetDBType
        /// </summary>
        /// <param name="cb">экземпляр System.Windows.Controls.ComboBox</param>
        /// <param name="target">целевая БД</param>
        public static void SetComboBoxConnectByDefaultTarget(ComboBox cb, Utilities.TargetDBType target)
        {
            if ((cb != null) && (cb.Items != null))
            {
                foreach (var item in MainWindow.ListConnects.Where(x => (x.TargetDBType == target) && (x.isMainTest == true)))
                {
                    SetComboBoxConnectByName(cb, item.DBConnectionName);
                    return;
                }
            }
            return;
        }

        /// <summary>
        /// Выбрать в ComboBox подключение к основной тестовой БД по проекту GIT
        /// </summary>
        /// <param name="cb">экземпляр System.Windows.Controls.ComboBox</param>
        /// <param name="GITProject">проект GIT</param>
        public static void SetComboBoxConnectByDefaultProject(ComboBox cb, string GITProject)
        {
            if ((cb != null) && (cb.Items != null))
            {
                foreach (var item in MainWindow.ListConnects.Where(x => (x.GITProject == GITProject) && (x.isMainTest == true)))
                {
                    SetComboBoxConnectByName(cb, item.DBConnectionName);
                    return;
                }
            }
            return;
        }

        /// <summary>
        /// Выбрать в ComboBox проект GIT по наименованию
        /// </summary>
        /// <param name="cb">экземпляр System.Windows.Controls.ComboBox</param>
        /// <param name="project">проект GIT</param>
        public static void SetComboBoxGITProjectByName(ComboBox cb, string project)
        {
            if ((cb != null) && (cb.Items != null))
            {
                foreach (ComboBoxItem item in cb.Items)
                {
                    if ((item != null) && (item.Tag != null) && (item.Tag.ToString() == project))
                    {
                        cb.SelectedItem = item;
                    }
                }
            }
        }

        /// <summary>
        /// Обновить в ComboBox список подключений, с учетом TargetDBType или ConnType, и выбрать значение defaultDBConnectionName
        /// </summary>
        /// <param name="cbConnect">экземпляр System.Windows.Controls.ComboBox</param>
        /// <param name="defaultDBConnectionName">выбрать подключение после обновления</param>
        /// <param name="target">целевая БД</param>
        /// <param name="conntype">тип соединения</param>
        /// <param name="isOnlyPromed">=true - только promed</param>
        public static void RefreshConnectItems(ComboBox cbConnect, string defaultDBConnectionName, Utilities.TargetDBType? target, Utilities.ConnType? conntype, bool isOnlyPromed = false)
        {
            ComboBoxItem cbMainItem = null;
            bool select = false;

            if ((cbConnect != null) && (cbConnect.Items != null))
            {
                cbConnect.Items.Clear();

                foreach (var item in MainWindow.ListConnects
                    .Where(x => x.isPromed || !isOnlyPromed)
                    .OrderBy(x => x.DBConnectionName)
                )
                {
                    if (
                        ((target != null) && (item.TargetDBType == target)) ||
                        ((conntype != null) && (item.ConnType == conntype)) ||
                        ((target == null) && (conntype == null))
                        )
                    {
                        ComboBoxItem cbItem = new ComboBoxItem { Content = item.DBConnectionName };
                        if (item.isMainTest)
                        {
                            cbItem.FontWeight = FontWeights.Bold;
                            cbMainItem = cbItem;
                        }

                        cbConnect.Items.Add(cbItem);

                        if ((!string.IsNullOrWhiteSpace(defaultDBConnectionName)) && (item.DBConnectionName == defaultDBConnectionName))
                        {
                            cbConnect.SelectedItem = cbItem;
                            select = true;
                        }
                    }
                }

                if ((!select) && (cbMainItem != null))
                {
                    cbConnect.SelectedItem = cbMainItem;
                }
            }

        }

        /// <summary>
        /// Выбрать соединение по выбранному значению из ComboBox, вернуть ConnectDB
        /// </summary>
        /// <param name="cbConnect">экземпляр System.Windows.Controls.ComboBox</param>
        /// <returns></returns>
        public static ConnectDB SetConnectFromComboBox(ComboBox cbConnect)
        {
            if ((cbConnect != null) && (cbConnect.SelectedItem != null))
            {
                ComboBoxItem cbItem = (ComboBoxItem)cbConnect.SelectedItem;

                // ищем подключение по его наименованию
                string connname = cbItem.Content.ToString();
                foreach (var item in MainWindow.ListConnects.Where(x => x.DBConnectionName == connname))
                {
                    return item.Copy();
                }
            }

            return new ConnectDB();
        }

        /// <summary>
        /// Открыть соединение по наименованию из ComboBox, вернуть ConnectDB
        /// </summary>
        /// <param name="cbConnect">экземпляр System.Windows.Controls.ComboBox</param>
        /// <param name="isNew">=true принудительно открыть форму "Подключение к БД"</param>
        /// <returns></returns>
        public static ConnectDB OpenConnectFromComboBox(ComboBox cbConnect, bool isNew)
        {
            ConnectDB result = SetConnectFromComboBox(cbConnect);
            result.OpenConnect(isNew);
            return result;
        }

        /// <summary>
        /// Открыть соединение по наименованию, вернуть ConnectDB
        /// </summary>
        /// <param name="ConnectionName">наименование подключения</param>
        /// <param name="isNew">=true принудительно открыть форму "Подключение к БД"</param>
        /// <returns></returns>
        public static ConnectDB OpenConnectByName(string ConnectionName, bool isNew)
        {
            ConnectDB result = null;

            foreach (var item in MainWindow.ListConnects.Where(x => (x.DBConnectionName.ToLower() == ConnectionName.ToLower())))
            {
                result = item;
                result.OpenConnect(isNew);
                break;
            }
            return result;
        }

        /// <summary>
        /// Обновить в ComboBox список проектов GIT с учетом filterConnType и выбрать значение defaultGITProject
        /// </summary>
        /// <param name="cbGITProject">экземпляр System.Windows.Controls.ComboBox</param>
        /// <param name="defaultGITProject">выбрать проект после обновления</param>
        /// <param name="filterConnType">с учетом указанного типа соединения</param>
        /// <param name="isOnlyDevPromed">=true - только "новые" проекты promed</param>
        /// <param name="isAddAll">=true - добавить первым ВСЕ</param>
        /// <param name="filterPrefix">только проекты с нужным префиксом</param>
        public static void RefreshGITProjectItems(ComboBox cbGITProject, string defaultGITProject, Utilities.ConnType? filterConnType = null, bool isOnlyDevPromed = false, bool isAddAll = false, string filterPrefix = "")
        {
            bool select = false;

            if ((cbGITProject != null) && (cbGITProject.Items != null))
            {
                cbGITProject.Items.Clear();

                if (isAddAll)
                {
                    ComboBoxItem cbItem = new ComboBoxItem { Content = "ВСЕ", Tag = "ВСЕ" };
                    cbGITProject.Items.Add(cbItem);
                }

                foreach (var item in Utilities.Files.ListFilesInDir(MainWindow.APPinfo.GITFolder, true, false, false))
                {
                    string project = Utilities.GITProjects.GetProjectByFolder(item);
                    if (!string.IsNullOrWhiteSpace(project))
                    {
                        if (
                            isOnlyDevPromed &&
                            (
                                Utilities.GITProjects.GetDBAliasByProject(project) != "promed" ||
                                !Utilities.GITProjects.IsDEVProject(project)
                            )
                        )
                        {
                            continue;
                        }

                        string DBType = Utilities.GITProjects.GetDBTypeByProject(project);
                        if (DBType == "MSSQL") DBType = "Microsoft SQL";
                        else if (DBType == "PGSQL") DBType = "Postgre SQL";

                        string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);

                        var item_target = ConnectDB.GetConnTypeByProject(project);

                        if (
                            ((filterConnType == null) || (item_target == filterConnType)) &&
                            (string.IsNullOrWhiteSpace(filterPrefix) || (prefix == filterPrefix))
                        )
                        {
                            ComboBoxItem cbItem = new ComboBoxItem { Content = DBType + " - " + project, Tag = project };

                            if ((!string.IsNullOrWhiteSpace(defaultGITProject)) && (project == defaultGITProject))
                            {
                                cbItem.FontWeight = FontWeights.Bold;
                            }

                            cbGITProject.Items.Add(cbItem);

                            if ((!string.IsNullOrWhiteSpace(defaultGITProject)) && (project == defaultGITProject))
                            {
                                cbGITProject.SelectedItem = cbItem;
                                select = true;
                            }
                        }
                    }
                }
                if ((!select) && (cbGITProject.Items.Count > 0))
                {
                    cbGITProject.SelectedIndex = 0;
                }
            }

        }

        /// <summary>Заполнить список регионов в ComboBox</summary>
        /// <param name="cb">ComboBox</param>
        /// <param name="project">проект GIT</param>
        /// <param name="addall">=true добавить строку "ВСЕ"</param>
        public static void RefreshRegionItems(ComboBox cb, string project, bool addall = false)
        {
            if ((cb != null) && (cb.Items != null))
            {
                cb.Items.Clear();
                List<string> dirs = new List<string>();

                if (addall)
                {
                    ComboBoxItem cbItem = new ComboBoxItem { Content = "ВСЕ регионы", Tag = "0" };
                    if (!cb.Items.Contains(cbItem)) cb.Items.Add(cbItem);

                    cbItem = new ComboBoxItem { Content = "Продуктовые БД", Tag = "100001" };
                    if (!cb.Items.Contains(cbItem)) cb.Items.Add(cbItem);

                    cbItem = new ComboBoxItem { Content = "Продлайк БД", Tag = "100002" };
                    if (!cb.Items.Contains(cbItem)) cb.Items.Add(cbItem);

                    cbItem = new ComboBoxItem { Content = "Реестровые БД", Tag = "100003" };
                    if (!cb.Items.Contains(cbItem)) cb.Items.Add(cbItem);

                    cbItem = new ComboBoxItem { Content = "Отчетные БД", Tag = "100004" };
                    if (!cb.Items.Contains(cbItem)) cb.Items.Add(cbItem);

                    cbItem = new ComboBoxItem { Content = "Тестовые и релизные БД", Tag = "100005" };
                    if (!cb.Items.Contains(cbItem)) cb.Items.Add(cbItem);
                }

                string folder = Utilities.GITProjects.GetFolderByProject("liquibase_project_new");
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    dirs.AddRange(Utilities.Files.ListFilesInDir(System.IO.Path.Combine(MainWindow.APPinfo.GITFolder, folder), true, false, false));
                }

                folder = Utilities.GITProjects.GetFolderByProject("dev_promed_pg");
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    dirs.AddRange(Utilities.Files.ListFilesInDir(System.IO.Path.Combine(MainWindow.APPinfo.GITFolder, folder), true, false, false));
                }

                folder = Utilities.GITProjects.GetFolderByProject("msdbupdate_new");
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    dirs.AddRange(Utilities.Files.ListFilesInDir(System.IO.Path.Combine(MainWindow.APPinfo.GITFolder, folder), true, false, false));
                }

                folder = Utilities.GITProjects.GetFolderByProject("dev_promed_ms");
                if (!string.IsNullOrWhiteSpace(folder))
                {
                    dirs.AddRange(Utilities.Files.ListFilesInDir(System.IO.Path.Combine(MainWindow.APPinfo.GITFolder, folder), true, false, false));
                }

                if (!string.IsNullOrWhiteSpace(project))
                {
                    folder = Utilities.GITProjects.GetFolderByProject(project);
                    if (!string.IsNullOrWhiteSpace(folder))
                    {
                        dirs.AddRange(Utilities.Files.ListFilesInDir(System.IO.Path.Combine(MainWindow.APPinfo.GITFolder, folder), true, false, false));
                    }
                }

                var sorted = dirs.Distinct()
                    .Where(x => Utilities.Databases.regex_region.IsMatch(x.ToLower()))
                    .OrderBy(x =>
                    {
                        int i = 0;
                        if (!int.TryParse(x.Substring(1), out i)) i = 0;
                        return i;
                    }
                ).ToList();

                foreach (var item in sorted)
                {
                    string num = item.Substring(1);
                    string tag = num;

                    if (MainWindow.APPinfo.Regions.ContainsKey(num))
                    {
                        num += " - " + MainWindow.APPinfo.Regions[num];
                    }

                    ComboBoxItem cbItem = new ComboBoxItem { Content = num, Tag = tag };
                    if (!cb.Items.Contains(cbItem)) cb.Items.Add(cbItem);
                }
            }
        }

        /// <summary>Заполнить список в ComboBox по запросу</summary>
        /// <param name="cb">ComboBox</param>
        /// <param name="connect">подключение</param>
        /// <param name="query">запрос</param>
        public static void RefreshComboBoxItems(System.Windows.Forms.ComboBox cb, ConnectDB connect, string query)
        {
            if ((cb != null) && (cb.Items != null) && (connect != null) && connect.isConnected)
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    using (DbDataReader reader = connect.OpenQuery(query))
                    {
                        if (reader != null)
                        {
                            cb.Items.Clear();
                            while (reader.Read())
                            {
                                string s = reader[0].ToString();
                                if (!cb.Items.Contains(s)) cb.Items.Add(s);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Заполнить список в ComboBox по запросу</summary>
        /// <param name="cb">ComboBox</param>
        /// <param name="connect">подключение</param>
        /// <param name="query">запрос</param>
        public static void RefreshComboBoxItems(System.Windows.Controls.ComboBox cb, ConnectDB connect, string query)
        {
            if ((cb != null) && (cb.Items != null) && (connect != null) && connect.isConnected)
            {
                if (!string.IsNullOrWhiteSpace(query))
                {
                    using (DbDataReader reader = connect.OpenQuery(query))
                    {
                        if (reader != null)
                        {
                            cb.Items.Clear();
                            while (reader.Read())
                            {
                                string s = reader[0].ToString();
                                if (!cb.Items.Contains(s)) cb.Items.Add(s);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>Определить выбранный регион</summary>
        /// <param name="cb">ComboBox</param>
        public static string GetSelectedRegion(ComboBox cb)
        {
            string result = "";

            if ((cb != null) && (cb.Items != null) && (cb.SelectedIndex != -1))
            {
                ComboBoxItem cbItem = (ComboBoxItem)cb.SelectedItem;

                if ((cbItem != null) && (cbItem.Tag != null))
                {
                    var arr = cbItem.Tag.ToString().Split('-');
                    result = arr[0].Trim();
                }
            }

            return result;
        }


        /// <summary>Установить выбранный регион</summary>
        /// <param name="cb">ComboBox</param>
        /// <param name="region">Регион</param>
        public static void SetSelectedRegion(ComboBox cb, string region)
        {
            if ((cb != null) && (cb.Items != null) && (region != null))
            {
                foreach (ComboBoxItem item in cb.Items)
                {
                    if ((item != null) && (item.Tag != null) && (item.Tag.ToString() == region.Trim()))
                    {
                        cb.SelectedItem = item;
                    }
                }
            }
        }

        /// <summary>
        /// Получить содержимое текущей выбранной ячейки в таблице DataGrid
        /// </summary>
        /// <param name="grid">экземпляр DataGrid</param>
        /// <param name="ColumnIndex">Номер колонки</param>
        /// <returns></returns>
        public static string GetSelectedValue(DataGrid grid, int ColumnIndex)
        {
            string res = "";

            if (
                (grid != null) &&
                (grid.SelectedCells != null) &&
                (grid.SelectionUnit == DataGridSelectionUnit.FullRow) &&
                (grid.SelectedCells.Count > ColumnIndex)
                )
            {

                DataGridCellInfo cellInfo = grid.SelectedCells[ColumnIndex];
                if (cellInfo == null) return null;

                DataGridBoundColumn column = cellInfo.Column as DataGridBoundColumn;
                if (column == null) return null;

                FrameworkElement element = new FrameworkElement() { DataContext = cellInfo.Item };
                BindingOperations.SetBinding(element, FrameworkElement.TagProperty, column.Binding);

                if (element.Tag != null) res = element.Tag.ToString();
            }

            if (
                (grid != null) &&
                (grid.SelectedCells != null) &&
                (grid.SelectionUnit == DataGridSelectionUnit.Cell) &&
                (grid.SelectedCells.Count > 0)
                )
            {

                DataGridCellInfo cellInfo = grid.SelectedCells[0];
                if (cellInfo == null) return null;

                DataGridBoundColumn column = cellInfo.Column as DataGridBoundColumn;
                if (column == null) return null;

                FrameworkElement element = new FrameworkElement() { DataContext = cellInfo.Item };
                BindingOperations.SetBinding(element, FrameworkElement.TagProperty, column.Binding);

                if (element.Tag != null) res = element.Tag.ToString();
            }

            if (string.IsNullOrWhiteSpace(res)) res = "";

            return res;
        }

        /// <summary>
        /// Изменить название кнопки
        /// </summary>
        /// <param name="button">кнопка</param>
        /// <param name="content">новый текст</param>
        /// <param name="fontWeigth">выделение текста в кнопке</param>
        /// <param name="isEnabled">доступность кнопки</param>
        /// <param name="prev_content">предыдущий текст</param>
        public static void ChangeButtonContent(Button button, string content, FontWeight fontWeigth, bool isEnabled, out string prev_content)
        {
            bool hasTextBox = false;
            prev_content = "";

            //ищем в кнопке StackPanel
            List<System.Windows.Controls.StackPanel> lisStackPanels = new List<System.Windows.Controls.StackPanel>();
            GetLogicalChildCollection(button, lisStackPanels);
            foreach (var panel in lisStackPanels)
            {
                // нашли StackPanel

                // ищем TextBlock в StackPanel 
                List<System.Windows.Controls.TextBlock> listTextBlocks = new List<System.Windows.Controls.TextBlock>();
                GetLogicalChildCollection(panel, listTextBlocks);
                foreach (var block in listTextBlocks)
                {
                    prev_content = block.Text;
                    block.Text = content;
                    hasTextBox = true;
                    break;
                }
            }

            if (!hasTextBox)
            {
                try
                {
                    prev_content = (string)button.Content;
                    button.Content = content.Replace("__", "_").Replace("_", "__");
                }
                catch (Exception)
                {
                }
            }

            button.FontWeight = fontWeigth;
            button.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Найти дочерные элементы интерфейса
        /// </summary>
        /// <typeparam name="T">тип элемента интерфейса</typeparam>
        /// <param name="parent">родительский элемента интерфейса</param>
        /// <param name="logicalCollection">список найденных элементов интерфейса</param>
        public static void GetLogicalChildCollection<T>(DependencyObject parent, List<T> logicalCollection) where T : DependencyObject
        {
            IEnumerable children = LogicalTreeHelper.GetChildren(parent);
            foreach (object child in children)
            {
                if (child is DependencyObject)
                {
                    DependencyObject depChild = child as DependencyObject;
                    if (child is T)
                    {
                        logicalCollection.Add(child as T);
                    }
                    GetLogicalChildCollection(depChild, logicalCollection);
                }
            }
        }

        /// <summary>
        /// Заполнить ComboBox список проектов GIT
        /// </summary>
        /// <param name="cb">экземпляр System.Windows.Controls.ComboBox</param>
        /// <param name="AddGIT">=true - добавить "старые" проекты GIT</param>
        /// <param name="AddDEV">=true - добавить "новые" проекты GIT</param>
        public static void FillComboBoxProjects(ComboBox cb, bool AddGIT, bool AddDEV)
        {
            if (!string.IsNullOrWhiteSpace(MainWindow.APPinfo.GITFolder))
            {
                cb.Items.Clear();
                cb.Items.Add("ВСЕ");
                cb.Text = "ВСЕ";

                if (AddGIT)
                {
                    foreach (var item in Utilities.Files.ListFilesInDir(MainWindow.APPinfo.GITFolder, true, false, false))
                    {
                        string project = Utilities.GITProjects.GITProjectsParam("GITProjectFolder", item, "GITProject");
                        string DBType = Utilities.GITProjects.GITProjectsParam("GITProjectFolder", item, "DBType");
                        if ((DBType == "MSSQL") || (DBType == "PGSQL"))
                        {
                            cb.Items.Add(project);
                        }
                    }
                }

                if (AddDEV)
                {
                    foreach (var item in Utilities.Files.ListFilesInDir(MainWindow.APPinfo.GITFolder, true, false, false))
                    {
                        string project = Utilities.GITProjects.GITProjectsParam("DEVProjectFolder", item, "DEVProject");
                        string DBType = Utilities.GITProjects.GITProjectsParam("DEVProjectFolder", item, "DBType");
                        if ((DBType == "MSSQL") || (DBType == "PGSQL"))
                        {
                            cb.Items.Add(project);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Заполнить CheckedListBox список проектов GIT на основе эталонного списка проектов
        /// </summary>
        /// <param name="clb">экземпляр System.Windows.Forms.CheckedListBox</param>
        /// <param name="listProjects">эталонный список проектов</param>
        /// <param name="mainConnect">экземляр ConnectDB, если указано, то ограничить значением его GITProject</param>
        /// <param name="listChecked">отметить указанные проекты</param>
        /// <param name="AddGIT">=true - добавить "старые" проекты</param>
        /// <param name="AddDEV">=true - добавить "новые" проекты</param>
        /// <param name="isSeparate">=true - разделить проекты (сначала "новые", потом "старые")</param>
        public static void FillCheckedListBoxProjects(System.Windows.Forms.CheckedListBox clb, List<string> listProjects, ConnectDB mainConnect, List<string> listChecked, bool AddGIT, bool AddDEV, bool isSeparate)
        {
            clb.Items.Clear();

            if (listProjects == null) return;
            if (listProjects.Count == 0) return;

            if (listChecked  == null) listChecked = new List<string>();

            string dev_project = "";
            string git_project = "";

            if (mainConnect != null)
            {
                dev_project = Utilities.GITProjects.GetDEVProject(mainConnect.GITProject);
                git_project = Utilities.GITProjects.GetGITProject(mainConnect.GITProject);
            }

            if (AddDEV)
            {
                foreach (var item in listProjects
                    .Where(x => x != "ВСЕ")
                    .Where(x => Utilities.GITProjects.IsDEVProject(x)) // только новые проекты
                    .Where(x => x == dev_project || string.IsNullOrWhiteSpace(dev_project)) // только проекты, связанные с подключением
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
                    if (!clb.Items.Contains(item))
                    {
                        string project = item;
                        string GITProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
                        string folder = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder);
                        bool isChecked = listChecked.Contains(project, StringComparer.OrdinalIgnoreCase);

                        if (
                            (!string.IsNullOrWhiteSpace(project)) &&
                            Directory.Exists(folder)
                            )
                        {
                            clb.Items.Add(item, isChecked);
                        }

                        if (AddGIT && !isSeparate)
                        {
                            string alt_project = Utilities.GITProjects.GetGITProject(project);
                            GITProjectFolder = Utilities.GITProjects.GetFolderByProject(alt_project);
                            folder = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder);
                            isChecked = listChecked.Contains(alt_project, StringComparer.OrdinalIgnoreCase);

                            if (
                                (!string.IsNullOrWhiteSpace(alt_project)) &&
                                Directory.Exists(folder)
                                )
                            {
                                clb.Items.Add(alt_project, isChecked);
                            }
                        }
                    }
                }
            }

            if (AddGIT)
            {
                foreach (var item in listProjects
                .Where(x => x != "ВСЕ")
                .Where(x => Utilities.GITProjects.IsGITProject(x)) // только старые проекты
                .Where(x => x == git_project || string.IsNullOrWhiteSpace(git_project)) // только проекты, связанные с подключением
                .OrderBy(x =>
                {
                    string ord = "999";
                    if (x == "liquibase_project_new") ord = "001";
                    else if (x == "msdbupdate_new") ord = "003";
                    else if (x == "promedlistest2") ord = "005";
                    else if (x == "emd") ord = "007";
                    return ord + x;
                }
                ))
                {
                    if (!clb.Items.Contains(item))
                    {
                        string project = item;
                        string GITProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
                        string folder = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder);
                        bool isChecked = listChecked.Contains(project, StringComparer.OrdinalIgnoreCase);

                        if (
                            (!string.IsNullOrWhiteSpace(project)) &&
                            Directory.Exists(folder)
                            )
                        {
                            clb.Items.Add(item, isChecked);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Блокирование элементов интерфейса при старте
        /// </summary>
        /// <param name="mainGrid">экземпляр Grid</param>
        /// <param name="enabledControls">список элементов интерфейса, которые надо оставить включенными (isEnabled = true)</param>
        /// <returns></returns>
        public static List<GridControl> DisabaleOnStart(Grid mainGrid, List<Control> enabledControls)
        {
            List<GridControl> listControls = new List<GridControl>();

            if (mainGrid == null) return listControls;

            // список ComboBox
            List<System.Windows.Controls.ComboBox> listComboBox = new List<System.Windows.Controls.ComboBox>();
            Utilities.Controls.GetLogicalChildCollection(mainGrid, listComboBox);
            foreach (var item in listComboBox)
            {
                listControls.Add(new GridControl { control = item, isEnabled = item.IsEnabled });
                item.IsEnabled = false;
            }

            // список TextBox
            List<System.Windows.Controls.TextBox> listTextBox = new List<System.Windows.Controls.TextBox>();
            Utilities.Controls.GetLogicalChildCollection(mainGrid, listTextBox);
            foreach (var item in listTextBox)
            {
                listControls.Add(new GridControl { control = item, isEnabled = item.IsEnabled });
                item.IsEnabled = false;
            }

            // список CheckBox
            List<System.Windows.Controls.CheckBox> listCheckBox = new List<System.Windows.Controls.CheckBox>();
            Utilities.Controls.GetLogicalChildCollection(mainGrid, listCheckBox);
            foreach (var item in listCheckBox)
            {
                listControls.Add(new GridControl { control = item, isEnabled = item.IsEnabled });
                item.IsEnabled = false;
            }

            // список Button
            List<System.Windows.Controls.Button> listButton = new List<System.Windows.Controls.Button>();
            Utilities.Controls.GetLogicalChildCollection(mainGrid, listButton);
            foreach (var item in listButton)
            {
                listControls.Add(new GridControl { control = item, isEnabled = item.IsEnabled });
                item.IsEnabled = false;
            }

            // исключения
            if (enabledControls != null)
            {
                foreach (var item in enabledControls)
                {
                    item.IsEnabled = true;
                }
            }

            return listControls;
        }

        /// <summary>
        /// Восстановление статуса (isEnabled) элементов интерфейса при завершении
        /// </summary>
        /// <param name="mainGrid">экземпляр Grid</param>
        /// <param name="controls">список элементов интерфейса</param>
        /// <returns></returns>
        public static void EnableOnFinish(Grid mainGrid, List<GridControl> controls)
        {
            if (mainGrid == null) return;

            if (controls != null)
            {
                foreach (var item in controls)
                {
                    item.control.IsEnabled = item.isEnabled;
                }
            }
        }

    }

    /// <summary>
    /// Элемент интерфейса
    /// </summary>
    public class GridControl
    {
        /// <summary>
        /// экземпляр Control
        /// </summary>
        public Control control = null;
        /// <summary>
        /// Статус до блокирования
        /// </summary>
        public bool isEnabled = false;
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// расширение для RichTextBox
    /// </summary>
    public static class RichTextBoxExt
    {
        /// <summary>
        /// Записать текст
        /// </summary>
        /// <param name="richTextBox">экземпляр RichTextBox</param>
        /// <param name="text">новое значение</param>
        public static void SetText(this RichTextBox richTextBox, string text)
        {
            richTextBox.Document.Blocks.Clear();
            richTextBox.Document.Blocks.Add(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(text)));
        }

        /// <summary>
        /// Получить текст
        /// </summary>
        /// <param name="richTextBox">экземпляр RichTextBox</param>
        /// <returns></returns>
        public static string GetText(this RichTextBox richTextBox)
        {
            return new System.Windows.Documents.TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
        }
    }
}
