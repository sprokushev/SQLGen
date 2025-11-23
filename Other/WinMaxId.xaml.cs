// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Окно для поиска информации
    /// </summary>
    public partial class WinMaxId : Window
    {

        /// <summary>Подключение к БД</summary>
        public ConnectDB ConnectSQL = null;

        /// <summary>Проект GIT</summary>
        public string GITProject = "";

        /// <summary>Флаг инициализации окна</summary>
        public bool isStart = true;

        /// <summary>
        /// Поисковый запрос
        /// </summary>
        public string SearchSQL = "";

        /// <summary>
        /// Результаты поиска
        /// </summary>
        public DataTable SearchData = new DataTable();

        /// <summary>Конструктор WinMaxId</summary>
        public WinMaxId()
        {
            InitializeComponent();

            tbTable.InitHistory("HistoryMaxId.json", "");

            SearchData = new DataTable();

            var column = new DataColumn("ServerName");
            column.DataType = Utilities.Databases.ConvertType("VARCHAR");
            SearchData.Columns.Add(column);

            column = new DataColumn("DBName");
            column.DataType = Utilities.Databases.ConvertType("VARCHAR");
            SearchData.Columns.Add(column);

            column = new DataColumn("MaxID");
            column.DataType = Utilities.Databases.ConvertType("BIGINT");
            SearchData.Columns.Add(column);

            dgSearchGrid.ItemsSource = SearchData.DefaultView;

            // пользовательские настройки GUI
            Default.InitGUI("WinMaxId", this, mainGrid, null, null, null, MainWindow.Task.LogFile);
        }

        /// <summary>При активации окна WinMaxId</summary>
        private void winMaxId_Activated(object sender, EventArgs e)
        {
        }

        /// <summary>При закрытии окна WinMaxId</summary>
        private void winMaxId_Closed(object sender, EventArgs e)
        {
            if (ConnectSQL != null)
            {
                ConnectSQL.CloseConnect();
                ConnectSQL.Dispose();
            }

            // пользовательские настройки GUI
            Default.SaveGUI("WinMaxId", this, null);
        }

        /// <summary>
        /// Переоткрытие подключения ConnectSQL
        /// </summary>
        /// <returns>=true - успешное переподключение</returns>
        private bool OpenConnectSQL(string ConnectionName)
        {
            ConnectSQL = Utilities.Controls.OpenConnectByName(ConnectionName, false);

            if (ConnectSQL == null)
            {
                return false;
            }

            if (ConnectSQL.isNotConnected)
            {
                ConnectSQL.ReConnect();
            }

            return ConnectSQL.isConnected;
        }

        /// <summary>Изменился проект GIT</summary>
        public void cbGITProjectChanged()
        {
            ComboBoxItem cbItem = null;
            if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
            if ((cbItem == null) || (cbItem.Tag == null)) GITProject = "";
            else GITProject = cbItem.Tag.ToString();

            tbConnections.Text = "";
        }

        /// <summary>Выбран проект GIT</summary>
        private void cbGITProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbGITProjectChanged();
        }

        /// <summary>Обновить результаты поиска</summary>
        private void dgSearchGridRefresh()
        {
            if (string.IsNullOrWhiteSpace(tbTable.Text))
            {
                App.AddLog($"Введите имя таблицы", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tbTable.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tbPK.Text))
            {
                App.AddLog($"Укажите название поля для подсчета маскимального значения", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tbPK.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tbUpper.Text))
            {
                App.AddLog($"Укажите верхний предел значений при подсчете маскимального значения", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tbUpper.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tbSQL.Text))
            {
                App.AddLog($"Должен быть заполнен текст запроса к БД", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tbSQL.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tbConnections.Text))
            {
                btConnections_Click(null, null);
                if (string.IsNullOrWhiteSpace(tbConnections.Text))
                {
                    App.AddLog($"Выберите подключения к БД", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    return;
                }
            }

            this.Cursor = Cursors.Wait;
            SearchData.Rows.Clear();

            var conns = tbConnections.Text.ToList(new char[] { '\r', '\n' }, true);
            long allmax = 0;

            foreach (var conn in conns)
            {
                if (!OpenConnectSQL(conn))
                {
                    App.AddLog($"Подключение {conn} не открыто !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
                else
                {
                    try
                    {
                        using (DbDataReader reader = ConnectSQL.OpenQuery(tbSQL.Text, false))
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    string s = reader[0].ToString();
                                    if (string.IsNullOrWhiteSpace(s)) s = "0";
                                    else if (s.Trim().ToLower() == "null") s = "0";
                                    long maxValue = long.Parse(s);

                                    object[] row = new object[SearchData.Columns.Count];
                                    row[0] = ConnectSQL.ServerName;
                                    row[1] = ConnectSQL.DBName;
                                    row[2] = maxValue;
                                    SearchData.Rows.Add(row);

                                    if (maxValue > allmax)
                                    {
                                        allmax = maxValue;
                                    }
                                    break; //-V3020
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        App.AddLog(conn, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                }
            }

            tbMaxId.Text = allmax.ToString();
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>Нажата кнопка Посчитать</summary>
        private void btCount_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbTable.Text))
            {
                MessageBox.Show("Необходимо заполнить имя таблицы");
                tbTable.Focus();
                return;
            }

            dgSearchGridRefresh();

            // Добавить в историю
            tbTable.AddHistory(tbTable.Text);
        }

        /// <summary>Нажата клавиша Enter в поле tbTable</summary>
        private void tbTable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btCount.Focus();
                btCount_Click(sender, e);
                e.Handled = true;
            }
        }

        /// <summary>
        /// выбрано значение из списка с историей поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbTable.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbTable.SelectedItem;

                tbTable.Text = (string)cbItem.Tag;
            }
        }

        private void btConnections_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(GITProject))
            {
                string git_project = Utilities.GITProjects.GetGITProject(GITProject);
                string dev_project = Utilities.GITProjects.GetDEVProject(GITProject);

                // выбрать подключения
                FormCheckedListBox dlg2 = new FormCheckedListBox();
                dlg2.Text = "Подключения к БД";
                dlg2.clbList.Items.Clear();

                foreach (var item in MainWindow.ListConnects
                    .Where(x =>
                        //(x.isTest || x.isRelease) &&
                        (x.GITProject == git_project || x.GITProject == dev_project)
                        )
                    .OrderBy(x => ((int)x.ServerDB.DBRoleType).ToString() + x.DBConnectionName)
                )
                {
                    if (!dlg2.clbList.Items.Contains(item.DBConnectionName))
                    {
                        dlg2.clbList.Items.Add(item.DBConnectionName, (item.isTest || item.isRelease));
                    }
                }

                tbConnections.Text = "";

                if (dlg2.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (object itemChecked in dlg2.clbList.CheckedItems)
                    {
                        tbConnections.Text += itemChecked.ToString() + Environment.NewLine;
                    }
                }

                dlg2.Dispose();
            }

        }

        private void SetSQL()
        {
            tbSQL.Text = "SELECT MAX(" + tbPK.Text + ") FROM " + tbTable.Text + " WHERE " + tbPK.Text + " < " + tbUpper.Text;
        }

        private void tbPK_LostFocus(object sender, RoutedEventArgs e)
        {
            SetSQL();
        }

        private void tbUpper_LostFocus(object sender, RoutedEventArgs e)
        {
            SetSQL();
        }

        private string oldSQL = "";
        private void tbSQL_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (oldSQL != tbSQL.Text)
            {
                tbMaxId.Text = "";
                SearchData.Rows.Clear();
                oldSQL = tbSQL.Text;
            }
        }

        private string oldConnections = "";
        private void tbConnections_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (oldConnections != tbConnections.Text)
            {
                tbMaxId.Text = "";
                SearchData.Rows.Clear();
                oldConnections = tbConnections.Text;
            }
        }

        private void tbTable_LostFocus(object sender, RoutedEventArgs e)
        {
            string _table = Utilities.Databases.GetTableName(tbTable.Text.Trim());

            if (_table.ToLower().StartsWith("v_"))
            {
                _table = _table.Substring(2);
            }
            if (_table.ToLower().StartsWith("\"v_"))
            {
                _table = "\"" + _table.Substring(3);
            }

            if (_table.ToLower() == "uslugacomplex")
            {
                tbUpper.Text = "20000000";
            }
            else
            {
                tbUpper.Text = "10000000";
            }

            if (_table.Contains("\""))
            {
                tbPK.Text = "\"" + _table.Replace("\"", "") + "_id\"";
            }
            else
            {
                tbPK.Text = _table + "_id";
            }


            SetSQL();
        }
    }

    /// <summary>
    /// Главное окно программы
    /// </summary>
    public partial class MainWindow
    {
        private void btMaxId_Click(object sender, RoutedEventArgs e)
        {

            WinMaxId WinMaxId = new WinMaxId();
            string _dbconngit = "";

            if (MainConnect != null)
            {
                _dbconngit = MainConnect.GITProject;
            }

            Utilities.Controls.RefreshGITProjectItems(WinMaxId.cbGITProject, _dbconngit);

            WinMaxId.isStart = false;

            WinMaxId.cbGITProjectChanged();

            WinMaxId.Show();


        }
    }

}