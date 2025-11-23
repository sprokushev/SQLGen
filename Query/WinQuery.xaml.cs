// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Окно для сборки скриптов с данными
    /// </summary>
    public partial class WinQuery : Window
    {
        /// <summary>Экземпляр QueryDB, с которым работаем в данном окне</summary>
        public QueryDB Query = new QueryDB();

        /// <summary>Конструктор WinQuery</summary>
        public WinQuery()
        {
            InitializeComponent();
            DataContext = Query;
            SetQuery(null);
            cbHistorySQLQuery.InitHistory("HistorySQLQuery.json", "");
            tbTableNameSQL.InitHistory("HistoryTableNameSQL.json", "");

            // пользовательские настройки GUI
            Default.InitGUI(
                "WinQuery",
                this,
                mainGrid,
                new List<System.Windows.Controls.Control> { tbSQL, tbScriptIUD },
                new List<ComboBox> { cbSQLFontFamily, cbScriptIUDFontFamily },
                new List<ComboBox> { cbSQLFontSize, cbScriptIUDFontSize },
                MainWindow.Task.LogFile
            );

            // заполняем toolbar'ы
            tbSQL.AddToolbar(toolbarSQL, SQLEditor.ToolbarButtonType.ADD, null, tiSQL);
            tbSQL.AddToolbarDefault(toolbarSQL, tiSQL, true, true);
            tbScriptIUD.AddToolbarDefault(toolbarScriptIUD, tiScriptIUD, true, false);
            btCancel.IsEnabled = false;
        }

        /// <summary>При открытии окна WinQuery</summary>
        private void winQuery_Activated(object sender, EventArgs e)
        {
            this.Title = "Запрос - " + Query.FullTableName + ", задача " + MainWindow.Task.TaskNumber;
        }

        /// <summary>При закрытии окна WinQuery</summary>
        private void winQuery_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI(
                "WinQuery",
                this,
                new List<System.Windows.Controls.Control> { tbSQL, tbScriptIUD }
                );
        }

        /// <summary>
        /// Заполнение окна WinQuery
        /// </summary>
        /// <param name="_query">экземпляр QueryDB</param>
        public void SetQuery(QueryDB _query)
        {

            if (Query == null) Query = new QueryDB();

            // по умолчанию
            tbTableNameSQL.Text = "";
            tbSQL.Text = "";
            tbSQL.Filename = "";
            cbConnectSQL.SelectedIndex = -1;
            cbGITProject.SelectedIndex = -1;
            cbScriptType.SelectedIndex = -1;
            cbInsUpdDTType.SelectedIndex = -1;
            tbPrimaryKey.Text = "";
            tbScriptIUD.Text = "";
            tbScriptIUD.Filename = "";
            isUseInsertUpdate.IsChecked = true;
            isUseInsertUpdate.Visibility = Visibility.Hidden;
            isAddCheckUnique.IsChecked = false;
            //isAddCheckUnique.Visibility = Visibility.Hidden;
            isAddEmptyString.IsChecked = true;
            isAddDel.IsChecked = false;
            isAddCreateTable.IsChecked = false;
            isAddGO.IsChecked = false;
            cbScriptType.SelectedIndex = 0;
            cbInsUpdDTType.SelectedIndex = 1;
            //tabData.Header = Query.ScriptFilename;
            isUpdateLocalDBList.IsChecked = true;


            // новые значения
            if (_query != null)
            {

                Query.Fill(_query);

                tbTableNameSQL.Text = _query.FullTableName;
                tbSQL.Text = _query.SQLQuery;
                tbPrimaryKey.Text = _query.UniqueKey.Replace("\"", "");
                tbScriptIUD.Text = _query.SQLScript;
                isUseInsertUpdate.IsChecked = _query.isUseInsertUpdate == true;
                isAddCheckUnique.IsChecked = _query.isAddCheckUnique == true;
                isAddEmptyString.IsChecked = _query.isAddEmptyString == true;
                isAddDel.IsChecked = _query.isAddDel == true;
                isAddGO.IsChecked = _query.isAddGO == true;
                isAddCreateTable.IsChecked = _query.isAddCreateTable == true;
                isUpdateLocalDBList.IsChecked = _query.isUpdateLocalDBList == true;

                Utilities.Controls.RefreshGITProjectItems(cbGITProject, _query.GITProject);

                switch (_query.ScriptType)
                {
                    case Utilities.ScriptType.INSERT:
                        cbScriptType.SelectedIndex = 0;
                        break;
                    case Utilities.ScriptType.INSERT_VALUES:
                        cbScriptType.SelectedIndex = 1;
                        break;
                    case Utilities.ScriptType.INSERT_TMP:
                        cbScriptType.SelectedIndex = 2;
                        break;
                    case Utilities.ScriptType.INSERT_BULK_TABLE:
                        cbScriptType.SelectedIndex = 3;
                        break;
                    case Utilities.ScriptType.INSERT_BULK_VIEW:
                        cbScriptType.SelectedIndex = 4;
                        break;
                    case Utilities.ScriptType.UPSERT:
                        cbScriptType.SelectedIndex = 5;
                        break;
                    case Utilities.ScriptType.UPSERT_TMP:
                        cbScriptType.SelectedIndex = 6;
                        break;
                    case Utilities.ScriptType.UPDATE:
                        cbScriptType.SelectedIndex = 7;
                        break;
                    case Utilities.ScriptType.DELETE:
                        cbScriptType.SelectedIndex = 8;
                        break;
                    case Utilities.ScriptType.SHABLON:
                        cbScriptType.SelectedIndex = 9;
                        break;
                    case Utilities.ScriptType.ALTER:
                    case Utilities.ScriptType.CREATE:
                    case Utilities.ScriptType.DROP:
                    default:
                        break;
                }

                switch (_query.InsUpdDTType)
                {
                    case Utilities.InsUpdDTType.NONE:
                        cbInsUpdDTType.SelectedIndex = 0;
                        break;
                    case Utilities.InsUpdDTType.GETDATE:
                        cbInsUpdDTType.SelectedIndex = 1;
                        break;
                    case Utilities.InsUpdDTType.VARI:
                    default:
                        cbInsUpdDTType.SelectedIndex = 2;
                        break;
                }

                //tabData.Header = Query.ScriptFilename;
            }

            if (Query.DataTable != null) Query.DataTable.Clear();
            Query.DataTable = new DataTable();
            dgData.ItemsSource = null;

            tiSQL.IsSelected = true;
            tbSQL.Focus();
        }

        /// <summary>
        /// Флаг остановки генерации
        /// </summary>
        public bool isCancelGenereate = true;

        List<GridControl> listControls;

        /// <summary>
        /// Настройка интерфейса при старте генерации
        /// </summary>
        private void StartGenerate()
        {
            isCancelGenereate = false;

            this.Cursor = Cursors.Wait;

            tiSQL.IsEnabled = false;

            listControls = Utilities.Controls.DisabaleOnStart(gridData, new List<Control> { btCancel });

            //btCancel.IsEnabled = true;
        }

        /// <summary>
        /// Настройка интерфейса при остановке генерации
        /// </summary>
        /// <param name="isScriptIUDSelected">=true - выбрана вкладка с текстом скрипта</param>
        private void StopGenerate(bool isScriptIUDSelected)
        {
            isCancelGenereate = true;

            this.Cursor = Cursors.Arrow;

            tiSQL.IsEnabled = true;

            Utilities.Controls.EnableOnFinish(gridData, listControls);

            if (!isScriptIUDSelected)
            {
                tiData.IsSelected = true;
                tiData.Focus();
                gridData.Focus();
            }

            //btCancel.IsEnabled = false;
        }

        /// <summary>Нажата кнопка Выполнить запрос</summary>
        private void btSelectIUD_Click(object sender, RoutedEventArgs e)
        {
            if (btSelectIUD.IsEnabled)
            {
                string sql = tbSQL.Text;

                cbScriptType.SelectedIndex = 0;
                pbProgress.Value = 0;

                var arr = sql.ToLower().Replace('\n', ' ').Replace('\r', ' ').Replace('\t', ' ').Split(' ');
                int pos;

                pos = Array.IndexOf(arr, "delete");
                if (pos != -1)
                {
                    App.AddLog(sql, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                    App.AddLog("Только оператор SELECT !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    tbSQL.Focus();
                    return;
                };

                pos = Array.IndexOf(arr, "update");
                if (pos != -1)
                {
                    App.AddLog(sql, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                    App.AddLog("Только оператор SELECT !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    tbSQL.Focus();
                    return;
                };

                pos = Array.IndexOf(arr, "insert");
                if (pos != -1)
                {
                    App.AddLog(sql, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                    App.AddLog("Только оператор SELECT !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    tbSQL.Focus();
                    return;
                };

                pos = Array.IndexOf(arr, "create");
                if (pos != -1)
                {
                    App.AddLog(sql, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                    App.AddLog("Только оператор SELECT !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    tbSQL.Focus();
                    return;
                };

                pos = Array.IndexOf(arr, "drop");
                if (pos != -1)
                {
                    App.AddLog(sql, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                    App.AddLog("Только оператор SELECT !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    tbSQL.Focus();
                    return;
                };

                ExecSQLInThread(sql).GetAwaiter();
            }
        }

        


        /// <summary>
        /// выполнить скрипт в отдельном потоке
        /// </summary>
        /// <param name="_Script">Скрипт</param>
        public async System.Threading.Tasks.Task ExecSQLInThread(string _Script)
        {
            Utilities.Controls.ChangeButtonContent(btSelectIUD, "Выполнение...", FontWeights.Bold, false, out string old);

            tbSQLMessages.Text = "";

            this.Cursor = Cursors.Wait;

            lbCount.Content = "Строк: ";
            if (Query.DataTable != null) Query.DataTable.Clear();

            string Messages = "";

            ConnectDB ConnectSQL = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, false);
            if ((ConnectSQL == null) || ConnectSQL.isNotConnected) //-V3063
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    cbConnectSQL.Focus();
                    Messages = App.AddLog("Подключение " + cbConnectSQL.Text + " не открыто !", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile).showMessage;
                });
            }
            else
            {
                Messages = await System.Threading.Tasks.Task.Run(() =>
                {
                    string Info = "";
                    try
                    {
                        Query.DataTable = ConnectSQL.FillDataTable(_Script, out Info);
                        if (Query.DataTable != null) //-V3022
                        {
                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                dgData.ItemsSource = Query.DataTable.DefaultView;
                                lbCount.Content = "Строк: " + Query.DataTable.Rows.Count;

                                if (Query.DataTable.Rows.Count > 15) cbInsUpdDTType.SelectedIndex = 2;

                                tbPrimaryKey.Text = "";
                                tbPrimaryKey.Text = ConnectSQL.GetTablePK(tbTableNameSQL.Text).FieldNamesToString;

                                if (string.IsNullOrWhiteSpace(tbPrimaryKey.Text))
                                {
                                    tbPrimaryKey.Text = Utilities.Databases.GetTableName(tbTableNameSQL.Text)
                                        .Replace("\"", "") + "_id";
                                }

                                isRegion.IsChecked = false;
                                tiData.IsSelected = true;
                                tbTableNameSQL.Focus();
                                tbPrimaryKey.Focus();
                                btGenerateIUD.Focus();
                                isAddCreateTable.IsChecked = false;
                                isUpdateLocalDBList.IsChecked = true;
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.Cursor = Cursors.Arrow;
                            Info = App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile).showMessage;
                        });
                    }
                    return Info;
                });

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Utilities.Controls.RefreshGITProjectItems(cbGITProject, ConnectSQL.GITProject);
                    cbGITProjectChanged();
                    cbHistorySQLQuery.AddHistory(Query.SQLQuery);
                    tbSQLMessages.Text = Messages;
                });

                // закрываем соединение
                ConnectSQL.CloseConnect();
            }

            this.Cursor = Cursors.Arrow;

            Utilities.Controls.ChangeButtonContent(btSelectIUD, old, FontWeights.Normal, true, out old);
        }


        /// <summary>Сформировать скрипт</summary>
        private async System.Threading.Tasks.Task GenScript(StreamWriter file, List<string> listLocal, System.Action<WinQuery> _action_finish)
        {
            string script = "";
            bool isAddTitle = true;

            ConnectDB ConnectSQL = null;

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                tbScriptIUDMessages.Text = "";
                dgScriptIUDResults.ItemsSource = null;
                lbScriptIUDStatus.Content = "";
                taskBarItem.ProgressValue = 0;

                ConnectSQL = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, false);
                if ((ConnectSQL == null) || ConnectSQL.isNotConnected) //-V3063
                {
                    tiSQL.IsSelected = true;
                    cbConnectSQL.Focus();
                    App.AddLog("Подключение " + cbConnectSQL.Text + " не открыто !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    return;
                }
            });

            string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);

            // сформировать скрипты с INSERT значений внешних ключей
            if (cbAddFK.SelectedIndex == 1)
            {
                try
                {
                    // заполнить список FK для текущей таблицы и всех "вложенных" таблиц
                    Query.FillListTableFK(ConnectSQL);

                    float ProgressStep = 1;
                    ProgressStep = ProgressStep / Query.ListTableFK.Count;

                    // сформировать скрипты (перебираем FK в обратном порядке)
                    foreach (var item in Query.ListTableFK.Where(x => x.Value.FKDeep != 0).OrderByDescending(x => x.Value.FKDeep))
                    {
                        var TableFK = item.Value;

                        if ((TableFK != null) && (TableFK.FKSchema != "") && (TableFK.FKTable != "") && (TableFK.FKField != "") && (TableFK.Query != ""))
                        {
                            try
                            {
                                QueryDB QueryFK = new QueryDB();

                                QueryFK.InsUpdDTType = Query.InsUpdDTType;
                                QueryFK.UniqueKey = TableFK.FKField;
                                QueryFK.ScriptNumber = Query.ScriptNumber;
                                if ((Query.ScriptType == Utilities.ScriptType.INSERT) || (Query.ScriptType == Utilities.ScriptType.INSERT_VALUES))
                                    QueryFK.ScriptType = Query.ScriptType;
                                else
                                    QueryFK.ScriptType = Utilities.ScriptType.INSERT;
                                QueryFK.SQLQuery = TableFK.Query;
                                QueryFK.FullTableName = TableFK.FKSchema + "." + TableFK.FKTable;
                                QueryFK.GITProject = Query.GITProject;
                                QueryFK.DataTable = new DataTable();
                                QueryFK.isAddCheckUnique = Query.isAddCheckUnique;
                                QueryFK.isAddEmptyString = Query.isAddEmptyString;
                                QueryFK.isAddDel = Query.isAddDel;

                                QueryFK.DataTable = ConnectSQL.FillDataTable(QueryFK.SQLQuery, out string Messages);

                                if ((QueryFK.SQLQuery != "") && (QueryFK.DataTable.Rows.Count > 0))
                                {
                                    foreach (var fk in TableFK.ListFKQuery.Select(x => (x.SchemaQuery + "." + x.TableQuery + "." + x.FieldQuery).ToLower()).Distinct())
                                    {
                                        script += Environment.NewLine + "-- " + fk;
                                    }


                                    script = await QueryFK.GenerateScript(this, ConnectSQL, isAddTitle, file, script, isRegion.IsChecked == true, txtRegion, false, null, cbAddFK.SelectedIndex);

                                    isAddTitle = false;

                                    if (this.isCancelGenereate)
                                    {
                                        break;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                            }

                        }

                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            taskBarItem.ProgressValue += ProgressStep;
                        });
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            // сформировать скрипты с EXISTS значений внешних ключей
            if (cbAddFK.SelectedIndex == 2)
            {
                try
                {
                    // заполнить список FK для текущей таблицы
                    Query.FillListTableFK(ConnectSQL, 1, false);
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            if (!this.isCancelGenereate)
            {
                try
                {
                    // сгенерировать основной запрос
                    script = await Query.GenerateScript(this, ConnectSQL, isAddTitle, file, script, isRegion.IsChecked == true, txtRegion, true, listLocal, cbAddFK.SelectedIndex);
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            ConnectSQL.CloseConnect();

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                taskBarItem.ProgressValue = 1;

                if (file == null && !this.isCancelGenereate)
                {
                    tbScriptIUD.Text = script;
                    tiScriptIUD.IsSelected = true;
                }
                else
                {
                    tbScriptIUD.Text = "";
                }

                // выполнить финишное действие
                if (_action_finish != null)
                {
                    _action_finish(this);
                }
            });
        }


        /// <summary>Сформировать скрипт-шаблон</summary>
        private void GenScriptShablon(StreamWriter file, List<string> listLocal)
        {
            bool isAddTitle = true;

            string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);

            // сгенерировать основной запрос
            tbScriptIUD.Text = Query.GenerateScriptShablon(isAddTitle, file, isRegion.IsChecked == true, txtRegion, listLocal);
            tiScriptIUD.IsSelected = true;
        }

        /// <summary>
        /// выбрать поля в запрос
        /// </summary>
        private void SetListNotUsedColumns()
        {
            // список исключаемых полей
            Query.ListNotUsedColumns.Clear();

            if (!Query.isAddDel)
            {
                // исключим признак удаления
                foreach (DataColumn column in Query.DataTable.Columns)
                {
                    if (
                        (!Query.ListNotUsedColumns.Contains(column.ColumnName.ToLower())) &&
                        (
                            column.ColumnName.ToLower().EndsWith("_deleted") ||
                            column.ColumnName.ToLower().EndsWith("_delid") ||
                            column.ColumnName.ToLower().EndsWith("_deldt") ||
                            column.ColumnName.ToLower().EndsWith("_deldttz")
                        )
                    )
                    {
                        Query.ListNotUsedColumns.Add(column.ColumnName.ToLower());
                    }
                }
            }

            switch (Query.ScriptType)
            {
                case Utilities.ScriptType.INSERT:
                case Utilities.ScriptType.UPSERT:
                case Utilities.ScriptType.UPSERT_TMP:
                case Utilities.ScriptType.UPDATE:
                case Utilities.ScriptType.INSERT_VALUES:
                case Utilities.ScriptType.INSERT_TMP:
                case Utilities.ScriptType.INSERT_BULK_TABLE:
                case Utilities.ScriptType.INSERT_BULK_VIEW:
                    if (System.Windows.Forms.MessageBox.Show("Все поля учитывать при генерации скрипта?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    {
                        // выберем поля для включения в скрипт
                        FormCheckedListBox dlg1 = new FormCheckedListBox();
                        dlg1.clbList.Items.Clear();

                        foreach (DataColumn column in Query.DataTable.Columns)
                        {
                            if (!dlg1.clbList.Items.Contains(column.ColumnName))
                            {
                                dlg1.clbList.Items.Add(column.ColumnName, true);
                            }
                        }

                        for (int i = 0; i < dlg1.clbList.Items.Count; i++)
                        {
                            string item = dlg1.clbList.Items[i].ToString().ToLower();

                            if (Query.ListNotUsedColumns.Contains(item))
                            {
                                dlg1.clbList.SetItemChecked(i, false);
                            }
                        }

                        if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            for (int i = 0; i < dlg1.clbList.Items.Count; i++)
                            {
                                string item = dlg1.clbList.Items[i].ToString().ToLower();
                                bool ischecked = dlg1.clbList.GetItemChecked(i);

                                if (
                                    (!ischecked) &&
                                    (!Query.ListNotUsedColumns.Contains(item))
                                    )
                                {
                                    Query.ListNotUsedColumns.Add(item);
                                }
                            }
                        }
                        dlg1.Dispose();
                    }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Генерация скриптов для BULK\COPY 
        /// </summary>
        /// <param name="ConnectSQL">соединение</param>
        /// <param name="file">файл для записи скрипта</param>
        /// <param name="_action_finish">действия после формирования</param>
        /// <returns></returns>
        private async System.Threading.Tasks.Task GenBulkCopy(ConnectDB ConnectSQL, StreamWriter file, System.Action<WinQuery> _action_finish)
        {
            // формируем csv-файл
            string CSVFile = "";
            this.Cursor = Cursors.Wait;
            CSVFile = await Query.GenCSV(this, "");
            this.Cursor = Cursors.Arrow;

            if (! this.isCancelGenereate)
            {
                try
                {
                    // формируем sql-файл
                    string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);

                    switch (Query.TargetDB)
                    {
                        case Utilities.TargetDBType.EMD:
                        case Utilities.TargetDBType.PGSQL:
                            tbScriptIUD.Text = Query.GeneratePGCopy(ConnectSQL, file, CSVFile, isRegion.IsChecked == true, txtRegion, cbAddFK.SelectedIndex);
                            this.tiScriptIUD.IsSelected = true;
                            break;
                        case Utilities.TargetDBType.MSSQL:
                        default:
                            tbScriptIUD.Text = Query.GenerateMSBulk(ConnectSQL, file, CSVFile, isRegion.IsChecked == true, txtRegion, cbAddFK.SelectedIndex);
                            this.tiScriptIUD.IsSelected = true;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // выполнить финишное действие
                if (_action_finish != null)
                {
                    _action_finish(this);
                }
            });
        }

        /// <summary>
        /// Генерация скрипта
        /// </summary>
        private void GenerateIUD(bool tofile)
        {
            if (
                string.IsNullOrWhiteSpace(tbTableNameSQL.Text)
            )
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableNameSQL.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tbPrimaryKey.Text) &&
                (
                (isAddCheckUnique.IsChecked == true) ||
                ((Query.ScriptType != Utilities.ScriptType.INSERT) && (Query.ScriptType != Utilities.ScriptType.INSERT_VALUES))
                )
                )
            {
                MessageBox.Show("Необходимо заполнить Уникальные поля (через запятую) !");
                tbPrimaryKey.Focus();
                return;
            }

            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            // выбрать поля в запрос
            SetListNotUsedColumns();

            Encoding encoding = new UTF8Encoding(false);
            FileStream fs = null;
            string filename = "";
            StreamWriter file = null;
            pbProgress.Value = 0;

            // спрашиваем имя файла
            if (tofile)
            {
                try
                {
                    // определить номер скрипта 
                    Query.ScriptNumber = (Utilities.Files.MaxScriptNumber(MainWindow.Task.TaskPath) + 1).ToString();

                    // запросить номер скрипта
                    FormAskNumFile dlg1 = new FormAskNumFile();
                    dlg1.tbNumFile.Text = Query.ScriptNumber;
                    if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        Query.ScriptNumber = dlg1.tbNumFile.Text.Trim();
                    dlg1.Dispose();

                    filename = Controls.Dialogs.SaveSQLDialog(MainWindow.Task.TaskPath, Query.ScriptFilename, out fs, out FileMode fileMode);

                    if (fs != null)
                    {
                        file = new StreamWriter(fs, encoding);
                    }
                }
                catch (Exception ex)
                {
                    if (file != null) file.Dispose();
                    if (fs != null) fs.Dispose();
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    return;
                }
            }

            // формируем sql-скрипт
            tbScriptIUD.Text = "";
            tbScriptIUD.Filename = "";

            // Добавить в историю
            this.tbTableNameSQL.AddHistory(this.tbTableNameSQL.Text);

            if ((Query.ScriptType == Utilities.ScriptType.INSERT_BULK_TABLE) || (Query.ScriptType == Utilities.ScriptType.INSERT_BULK_VIEW))
            {
                // скрипт BULK\COPY

                ConnectDB ConnectSQL = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, false);
                if ((ConnectSQL == null) || ConnectSQL.isNotConnected) //-V3063
                {
                    if (file != null) file.Dispose();
                    if (fs != null) fs.Dispose();
                    tiSQL.IsSelected = true;
                    cbConnectSQL.Focus();
                    App.AddLog("Подключение " + cbConnectSQL.Text + " не открыто !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    return;
                }

                StartGenerate();

                GenBulkCopy(ConnectSQL, file,
                    x =>
                    {
                        ConnectSQL.CloseConnect();

                        if (file != null) file.Dispose();
                        if (fs != null) fs.Dispose();

                        if (tofile && !this.isCancelGenereate)
                        {
                            if ((!string.IsNullOrEmpty(filename)) && File.Exists(filename))
                            {
                                if (System.Windows.Forms.MessageBox.Show("Открыть файл " + filename + " ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                {
                                    App.AddLog("Открытие внешнего файла " + filename, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                    Utilities.External.OpenExternalFile(filename);
                                }
                            }
                        }

                        StopGenerate(this.tiScriptIUD.IsSelected == true);
                    }
                    ).GetAwaiter();
            }
            else
            {
                // остальные типы скриптов

                List<string> listLocal = new List<string>();

                if (
                    (isUpdateLocalDBList.IsChecked == true) &&
                    Query.isPromed
                )
                {
                    try
                    {
                        listLocal = Utilities.Databases.ChooseLocalDBList(cbConnectSQL, Query.GITProject, Query.FullTableNameReady);
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                        listLocal = new List<string>();
                    }
                }

                if (cbScriptType.SelectedIndex == 9)
                {
                    // шаблон скрипта

                    GenScriptShablon(file, listLocal);

                    if (file != null) file.Dispose();
                    if (fs != null) fs.Dispose();

                    if (tofile)
                    {
                        if ((!string.IsNullOrEmpty(filename)) && File.Exists(filename))
                        {
                            if (System.Windows.Forms.MessageBox.Show("Открыть файл " + filename + " ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                            {
                                App.AddLog("Открытие внешнего файла " + filename, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                Utilities.External.OpenExternalFile(filename);
                            }
                        }
                    }
                }
                else
                {
                    // остальные типы скриптов
                    StartGenerate();

                    if (tofile)
                    {
                        GenScript(file, listLocal,
                            x =>
                            {
                                if (file != null) file.Dispose();
                                if (fs != null) fs.Dispose();

                                if (tofile && !this.isCancelGenereate)
                                {
                                    if ((!string.IsNullOrEmpty(filename)) && File.Exists(filename))
                                    {
                                        if (System.Windows.Forms.MessageBox.Show("Открыть файл " + filename + " ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                        {
                                            App.AddLog("Открытие внешнего файла " + filename, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                            Utilities.External.OpenExternalFile(filename);
                                        }
                                    }
                                }

                                StopGenerate(this.tiScriptIUD.IsSelected == true);
                            }).GetAwaiter();
                    }
                    else
                    {
                        GenScript(null, listLocal,
                            x=>
                            {
                                StopGenerate(this.tiScriptIUD.IsSelected == true);

                            }).GetAwaiter();
                    }
                }
            }
        }


        /// <summary>Нажата кнопка Сгенерировать скрипт</summary>
        private void btGenerateIUD_Click(object sender, RoutedEventArgs e)
        {
            if (
                (Query.DataTable.Rows.Count > 5000) &&
                (Query.ScriptType != Utilities.ScriptType.INSERT_BULK_TABLE) &&
                (Query.ScriptType != Utilities.ScriptType.INSERT_BULK_VIEW) &&
                (Query.ScriptType != Utilities.ScriptType.SHABLON) &&
                (System.Windows.Forms.MessageBox.Show("Кол-во записей более 5000. Сохранить скрипт в файл?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                )
            {
                btGenerateIUDFile_Click(sender, e);
                return;
            }

            GenerateIUD(false);
        }


        /// <summary>Нажата кнопка Сгенерировать скрипт в файл</summary>
        private void btGenerateIUDFile_Click(object sender, RoutedEventArgs e)
        {
            GenerateIUD(true);
        }

        /// <summary>Корректировка имен колонок в Grid'е</summary>
        private void dgData_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) //-V3013
        {

            string header = e.Column.Header.ToString();

            // Replace all underscores with two underscores, to prevent AccessKey handling
            e.Column.Header = header.Replace("_", "__");

            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy HH:mm:ss.FFF";
        }

        /// <summary>Изменилось поле SQL</summary>
        private void tbSQLTextChanged()
        {
            var s_orig = tbSQL.Text
                .Replace(System.Environment.NewLine, " ")
                .Replace("\n", " ")
                .Replace("\t", " ")
                .TrimInner()
                .Trim();

            var s_lower = s_orig.ToLower();
            var arr = s_orig.Split(' ');
            var pos = Array.IndexOf(s_lower.Split(' '), "from");

            if ((pos != -1) && ((pos + 1) < arr.Count()))
            {
                tbTableNameSQL.Text = arr[pos + 1].Trim();
                TableNameSQLChanged();
            }

            Query.SQLQuery = tbSQL.Text;
            tbSQLMessages.Text = "";
        }

        /// <summary>Нажата кнопка Сохранить в файл</summary>
        private void btSaveIUD_Click(object sender, RoutedEventArgs e)
        {

            FileStream fs = null;
            string filename = "";

            try
            {
                // определить номер скрипта 
                Query.ScriptNumber = (Utilities.Files.MaxScriptNumber(MainWindow.Task.TaskPath) + 1).ToString();

                // запросить номер скрипта
                FormAskNumFile dlg1 = new FormAskNumFile();
                dlg1.tbNumFile.Text = Query.ScriptNumber;
                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    Query.ScriptNumber = dlg1.tbNumFile.Text.Trim();
                dlg1.Dispose();

                // имя файла для скрипта
                filename = Controls.Dialogs.SaveSQLDialog(MainWindow.Task.TaskPath, Query.ScriptFilename, out fs, out FileMode fileMode);

                // сохранить файл
                if (fs != null)
                {
                    this.Cursor = Cursors.Wait;
                    tbScriptIUD.Filename = "";
                    Utilities.Files.WriteScript(filename, fs, tbScriptIUD.Text, false, out string err, fileMode);
                    tbScriptIUD.Filename = filename;
                    this.Cursor = Cursors.Arrow;
                }
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
            this.Cursor = Cursors.Arrow;

            if ((!string.IsNullOrEmpty(filename)) && File.Exists(filename))
            {
                if (System.Windows.Forms.MessageBox.Show("Открыть файл " + filename + " ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    App.AddLog("Открытие внешнего файла " + filename, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                    Utilities.External.OpenExternalFile(filename);
                }
            }
        }

        /// <summary>Нажата кнопка В буфер обмена</summary>
        private void btClipboardIUD_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbScriptIUD.Text);
            /*            if (CurrentScript != null)
                        {
                            CurrentScript.ScriptFilename = Query.ScriptFilename;
                            CurrentScript.Query.Fill(Query);
                            tabTask.Focus();
                            dgScripts.Focus();
                        }*/
        }

        /// <summary>Изменилось поле Имя таблицы</summary>
        private void TableNameSQLChanged()
        {
            string name = tbTableNameSQL.Text.Trim().Replace("[", "").Replace("]", "").Replace(" ", ".").Replace("\t", ".").Trim();
            if (name == "") name = "dbo.";
            string schema;

            var arr = name.Split('.');
            if (arr.Length <= 1) { schema = "dbo"; name = "dbo." + name; }
            else schema = arr[0];

            if (Query.FullTableName != name)
            {
                Query.FullTableName = name;
                tbTableNameSQL.Text = name;
            }

            this.Title = "Запрос - " + Query.FullTableName + ", задача " + MainWindow.Task.TaskNumber;

            if (Utilities.Databases.regex_region.IsMatch(schema.ToLower()))
            {
                isRegion.IsChecked = true;
                Utilities.Controls.SetSelectedRegion(cbRegion, schema.Substring(1));
            }
            else
            {
                if (cbRegion.SelectedIndex == -1)
                {
                    isRegion.IsChecked = false;
                    //cbRegion.SelectedIndex = -1;
                }
            }
        }

        /// <summary>Выход из поля Имя таблицы</summary>
        private void tbTableNameSQL_LostFocus(object sender, RoutedEventArgs e)
        {
            TableNameSQLChanged();
        }

        /// <summary>Нажата клавиша в поле Имя таблицы</summary>
        private void tbTableNameSQL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TableNameSQLChanged();
                e.Handled = true;
            }

        }

        /// <summary>Выбран тип скрипта</summary>
        private void cbScriptType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isUseInsertUpdate != null)
            {
                isUseInsertUpdate.Visibility = Visibility.Hidden;
            }

            //if (isAddCheckUnique != null)
            //{
            //    isAddCheckUnique.Visibility = Visibility.Hidden;
            //}

            switch (cbScriptType.SelectedIndex)
            {
                case 1:
                    Query.ScriptType = Utilities.ScriptType.INSERT_VALUES;
                    if (isAddCheckUnique != null)
                    {
                        //isAddCheckUnique.Visibility = Visibility.Visible;
                        if (Query.DBType == "MSSQL")
                        {
                            isAddCheckUnique.IsChecked = false;
                        }

                        if (Query.DBType == "PGSQL")
                        {
                            isAddCheckUnique.IsChecked = false;
                        }
                    }
                    break;
                case 2:
                    Query.ScriptType = Utilities.ScriptType.INSERT_TMP;
                    if (isAddCheckUnique != null)
                    {
                        //isAddCheckUnique.Visibility = Visibility.Visible;
                        isAddCheckUnique.IsChecked = false;
                    }
                    break;
                case 3:
                    Query.ScriptType = Utilities.ScriptType.INSERT_BULK_TABLE;
                    if (isAddCheckUnique != null)
                    {
                        //isAddCheckUnique.Visibility = Visibility.Visible;
                        isAddCheckUnique.IsChecked = false;
                    }
                    break;
                case 4:
                    Query.ScriptType = Utilities.ScriptType.INSERT_BULK_VIEW;
                    if (isAddCheckUnique != null)
                    {
                        //isAddCheckUnique.Visibility = Visibility.Visible;
                        isAddCheckUnique.IsChecked = false;
                    }
                    break;
                case 5:
                    Query.ScriptType = Utilities.ScriptType.UPSERT;
                    if (isUseInsertUpdate != null)
                    {
                        isUseInsertUpdate.Visibility = Visibility.Visible;
                        if (isAddCheckUnique != null)
                        {
                            if (Query.DBType == "MSSQL")
                            {
                                isAddCheckUnique.IsChecked = isUseInsertUpdate.IsChecked == true;
                            }
                            if (Query.DBType == "PGSQL")
                            {
                                isAddCheckUnique.IsChecked = false;
                            }
                        }
                    }
                    break;
                case 6:
                    Query.ScriptType = Utilities.ScriptType.UPSERT_TMP;
                    if (isAddCheckUnique != null)
                    {
                        //isAddCheckUnique.Visibility = Visibility.Visible;
                        isAddCheckUnique.IsChecked = false;
                    }
                    break;
                case 7:
                    Query.ScriptType = Utilities.ScriptType.UPDATE;
                    if (isAddCheckUnique != null)
                    {
                        isAddCheckUnique.IsChecked = false;
                    }
                    break;
                case 8:
                    Query.ScriptType = Utilities.ScriptType.DELETE;
                    if (isAddCheckUnique != null)
                    {
                        isAddCheckUnique.IsChecked = false;
                    }
                    break;
                case 9:
                    Query.ScriptType = Utilities.ScriptType.SHABLON;
                    if (isAddCheckUnique != null)
                    {
                        isAddCheckUnique.IsChecked = false;
                    }
                    break;
                case 0:
                default:
                    Query.ScriptType = Utilities.ScriptType.INSERT;
                    if (isAddCheckUnique != null)
                    {
                        //isAddCheckUnique.Visibility = Visibility.Visible;
                        if (Query.DBType == "MSSQL")
                        {
                            isAddCheckUnique.IsChecked = true;
                        }

                        if (Query.DBType == "PGSQL")
                        {
                            isAddCheckUnique.IsChecked = false;
                        }
                    }
                    break;
            }

            if (
                (isAddCheckUnique != null) &&
                (cbAddFK.SelectedIndex == 2) 
            )
            {
                isAddCheckUnique.IsChecked = true;
            }
        }

        /// <summary>Выбран тип обновления даты в полях insDT/updDT</summary>
        private void cbInsUpdDTType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbInsUpdDTType.SelectedIndex)
            {
                case 0:
                    Query.InsUpdDTType = Utilities.InsUpdDTType.NONE;
                    break;
                case 1:
                    Query.InsUpdDTType = Utilities.InsUpdDTType.GETDATE;
                    break;
                case 2:
                default:
                    Query.InsUpdDTType = Utilities.InsUpdDTType.VARI;
                    break;
            }
        }

        /// <summary>Нажата кнопка Выгрузить в Excel</summary>
        private void btExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Utilities.MSOffice.GenerateExcel(Query.DataTable, false, "", true);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
        }

        /// <summary>Выбран регион</summary>
        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRegion.SelectedIndex != -1)
            {
                isRegion.IsChecked = true;
            }
        }

        /// <summary>Установлен флаг INSERT+UPDATE</summary>
        private void isUseInsertUpdate_Checked(object sender, RoutedEventArgs e)
        {
            Query.isUseInsertUpdate = true;
            if ((cbScriptType != null) && (cbScriptType.Items != null) && (cbScriptType.Items.Count > 5) && (cbScriptType.Items[5] != null))
            {
                ComboBoxItem cbi = (ComboBoxItem)cbScriptType.Items[5];
                cbi.Content = "INSERT+UPDATE";
            }
        }

        /// <summary>Снят флаг INSERT+UPDATE</summary>
        private void isUseInsertUpdate_Unchecked(object sender, RoutedEventArgs e)
        {
            Query.isUseInsertUpdate = false;
            if ((cbScriptType != null) && (cbScriptType.Items != null) && (cbScriptType.Items.Count > 5) && (cbScriptType.Items[5] != null))
            {
                ComboBoxItem cbi = (ComboBoxItem)cbScriptType.Items[5];
                cbi.Content = "MERGE";
            }
        }

        /// <summary>Установлен флаг Check Unique</summary>
        private void isAddCheckUnique_Checked(object sender, RoutedEventArgs e)
        {
            Query.isAddCheckUnique = true;
        }

        /// <summary>Снят флаг Check Unique</summary>
        private void isAddCheckUnique_Unchecked(object sender, RoutedEventArgs e)
        {
            Query.isAddCheckUnique = false;
        }

        /// <summary>Нажата кнопка Выбрать поля</summary>
        private void btChooseFields_Click(object sender, RoutedEventArgs e)
        {

            ConnectDB ConnectSQL = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, false);
            if ((ConnectSQL == null) || ConnectSQL.isNotConnected) //-V3063
            {
                App.AddLog("Подключение " + cbConnectSQL.Text + " не открыто !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tiSQL.IsSelected = true;
                cbConnectSQL.Focus();
                return;
            }

            FormCheckedListBox dlg1 = new FormCheckedListBox();
            dlg1.clbList.Items.Clear();

            string fields = ConnectSQL.GetListFields(tbTableNameSQL.Text);
            if (!string.IsNullOrWhiteSpace(fields)) 
            {
                // берем список полей из БД
                dlg1.clbList.Items.AddRange(fields.Split(','));
            }
            else
            {
                // берем список полей из grid
                if (dgData.Columns != null)
                {
                    foreach (var item in dgData.Columns)
                    {
                        string column = item.Header.ToString();
                        dlg1.clbList.Items.Add(column.Replace("__", "_"));
                    }
                }
            }

            string keys = tbPrimaryKey.Text.Trim().Replace("\"", "").Replace(";", ",");
            if (keys == "")
            {
                // уникальные ключи не указаны
                // ищем primary key
                keys = ConnectSQL.GetTablePK(tbTableNameSQL.Text).FieldNamesToString;
            }

            var arr = keys.Split(',');

            for (int i = 0; i < dlg1.clbList.Items.Count; i++)
            {
                string item = dlg1.clbList.Items[i].ToString().ToLower();

                foreach (var key in arr)
                {
                    if (key.ToLower().Trim() == item)
                    {
                        dlg1.clbList.SetItemChecked(i, true);
                    }
                }
            }

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string result = "";

                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    if (result != "") result += ", ";
                    result += itemChecked.ToString();
                }

                tbPrimaryKey.Text = result;

            }
            dlg1.Dispose();

            ConnectSQL.CloseConnect();
        }

        /// <summary>Нажата кнопка Загрузить из xls-файла</summary>
        private void btLoadXLS_Click(object sender, RoutedEventArgs e)
        {
            cbScriptType.SelectedIndex = 0;

            lbCount.Content = "Строк: ";
            if (Query.DataTable != null) Query.DataTable.Clear();
            pbProgress.Value = 0;

            FormLoad dlg1 = new FormLoad();

            ConnectDB ConnectSQL = Utilities.Controls.SetConnectFromComboBox(cbConnectSQL);

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    string tablename = Utilities.Databases.GetFullTableName(dlg1.tbTableName.Text);
                    tbTableNameSQL.Text = tablename;
                    TableNameSQLChanged();

                    this.Cursor = Cursors.Wait;
                    Query.DataTable = Utilities.MSOffice.LoadExcel(ConnectSQL, tbTableNameSQL.Text, dlg1.tbFilename.Text, (int)dlg1.tbNumSheet.Value, dlg1.cbTypeRow.Checked == true);
                    if (Query.DataTable != null)
                    {
                        dgData.ItemsSource = Query.DataTable.DefaultView;
                        lbCount.Content = "Строк: " + Query.DataTable.Rows.Count;

                        if (Query.DataTable.Rows.Count > 15) cbInsUpdDTType.SelectedIndex = 2;

                        tbPrimaryKey.Text = "";
                        try
                        {
                            if (ConnectSQL != null) //-V3022
                            {
                                tbPrimaryKey.Text = ConnectSQL.GetTablePK(tbTableNameSQL.Text).FieldNamesToString;
                            }
                        }
                        catch (Exception ex)
                        {
                            tbPrimaryKey.Text = "";
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }
                        if (string.IsNullOrWhiteSpace(tbPrimaryKey.Text)) tbPrimaryKey.Text = Utilities.Databases.GetTableName(tbTableNameSQL.Text) + "_id";

                        isRegion.IsChecked = false;
                        tiData.IsSelected = true;
                        tbTableNameSQL.Focus();
                        tbPrimaryKey.Focus();
                        btGenerateIUD.Focus();
                        isAddCreateTable.IsChecked = false;
                        isUpdateLocalDBList.IsChecked = true;
                    }
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

            }
            dlg1.Dispose();

            this.Cursor = Cursors.Arrow;

            if (ConnectSQL != null) //-V3022
            {
                Utilities.Controls.RefreshGITProjectItems(cbGITProject, ConnectSQL.GITProject);
                cbGITProjectChanged();
                ConnectSQL.CloseConnect();
            }

        }

        /// <summary>Нажата кнопка "Добавить подключение" на вкладке SQL</summary>
        private void btAddConnectSQL_Click(object sender, RoutedEventArgs e)
        {
            var connect = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, true);

            if ((connect != null) && (connect.ConnType != Utilities.ConnType.None)) //-V3063
            {
                Utilities.Controls.RefreshConnectItems(cbConnectSQL, connect.DBConnectionName, null, null);
            }

            connect.CloseConnect();
        }

        /// <summary>
        /// Выход из поля выбора подключения на вкладке SQL
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void cbConnectSQL_LostFocus(object sender, RoutedEventArgs e)
        {
            ConnectDB ConnectSQL = Utilities.Controls.SetConnectFromComboBox(cbConnectSQL);

            if (ConnectSQL != null) //-V3022
            {
                Utilities.Controls.RefreshGITProjectItems(cbGITProject, ConnectSQL.GITProject);
                cbGITProjectChanged();
                ConnectSQL.CloseConnect();
            }
        }

        /// <summary>Изменился проект GIT</summary>
        private void cbGITProjectChanged()
        {
            ComboBoxItem cbItem = null;
            if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
            if ((cbItem == null) || (cbItem.Tag == null)) Query.GITProject = "";
            else Query.GITProject = cbItem.Tag.ToString();

            string connname = "";
            if ((cbConnectSQL != null) && (cbConnectSQL.SelectedItem != null))
            {
                cbItem = (ComboBoxItem)cbConnectSQL.SelectedItem;
                connname = cbItem.Content.ToString();
            }

            Utilities.Controls.RefreshConnectItems(cbConnectTarget, connname, null, Query.ConnType);
            Utilities.Controls.SetComboBoxConnectByDefaultProject(cbConnectTarget, Query.GITProject);

            if (isAddCheckUnique != null)
            {
                isAddCheckUnique.IsChecked = false;
                if (((cbScriptType.SelectedIndex == 0) || (cbScriptType.SelectedIndex == 1) || (cbScriptType.SelectedIndex == 5)) && (Query.DBType == "MSSQL"))
                {
                    isAddCheckUnique.IsChecked = true;
                }

                if (cbAddFK.SelectedIndex == 2)
                {
                    isAddCheckUnique.IsChecked = true;
                }
            }
        }

        /// <summary>Выбран проект GIT</summary>
        private void cbGITProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbGITProjectChanged();
        }

        /// <summary>Нажата кнопка "Добавить подключение" на вкладке Script</summary>
        private void btAddConnectTarget_Click(object sender, RoutedEventArgs e)
        {
            var connect = Utilities.Controls.OpenConnectFromComboBox(cbConnectTarget, true);

            if ((connect != null) && (connect.ConnType != Utilities.ConnType.None)) //-V3063
            {
                string connname = "";
                if ((cbConnectTarget != null) && (cbConnectTarget.SelectedItem != null))
                {
                    var cbItem = (ComboBoxItem)cbConnectTarget.SelectedItem;
                    connname = cbItem.Content.ToString();
                }

                Utilities.Controls.RefreshConnectItems(cbConnectTarget, connname, null, Query.ConnType);
                Utilities.Controls.SetComboBoxConnectByDefaultProject(cbConnectTarget, Query.GITProject);
            }

            connect.CloseConnect();
        }

        /// <summary>Нажата кнопка "Выполнить скрипт" на вкладке Script</summary>
        private void btExecScriptIUD_Click(object sender, RoutedEventArgs e)
        {
            if (btExecScriptIUD.IsEnabled)
            {
                if (cbConnectTarget.SelectedIndex == -1)
                {
                    MessageBox.Show("Для выполнения скрипта необходимо выбрать подключение к БД !");
                    cbConnectTarget.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(tbScriptIUD.Text))
                {
                    MessageBox.Show("Скрипт пустой !");
                    tbScriptIUD.Focus();
                    return;
                }

                tbScriptIUDMessages.Text = "";
                dgScriptIUDResults.ItemsSource = null;
                lbScriptIUDStatus.Content = "";

                if (!string.IsNullOrWhiteSpace(tbScriptIUD.SelectedText))
                {
                    Utilities.External.ExecScriptInThread(Utilities.External.ExecType.QUERY, this, cbConnectTarget, tbScriptIUD.SelectedText, btExecScriptIUD, tiScriptIUDResults, dgScriptIUDResults, tiScriptIUDMessages, tbScriptIUDMessages, lbScriptIUDStatus);
                }
                else
                {
                    Utilities.External.ExecScriptInThread(Utilities.External.ExecType.DEFAULT, this, cbConnectTarget, tbScriptIUD.Text, btExecScriptIUD, null, null, tiScriptIUDMessages, tbScriptIUDMessages, lbScriptIUDStatus);
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выбран запрос из Истории запросов</summary>
        private void cbHistorySQLQuery_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbHistorySQLQuery.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)cbHistorySQLQuery.SelectedItem;
                tbSQL.Text = (string)cbItem.Tag;
                tbSQL.Filename = "";
                tbSQLTextChanged();
            }

        }

        /// <summary>
        /// Нажатие нопок на вкладе Script
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tiScriptIUD_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                btExecScriptIUD_Click(null, null);
                e.Handled = true;
            }

            if (e.Key == Key.F8)
            {
                btSaveIUD_Click(null, null);
                e.Handled = true;
            }
        }

        /// <summary>Корректиовка имен колонок в Grid'е</summary>
        private void dgScriptIUDResults_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();

            // Replace all underscores with two underscores, to prevent AccessKey handling
            e.Column.Header = header.Replace("_", "__");

            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy HH:mm:ss.FFF";
        }

        /// <summary>
        /// Нажатие кнопок на вкладке SQL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tiSQL_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                btSelectIUD_Click(null, null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Нажатие кнопок на вкладке Данные
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tiGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                btGenerateIUD_Click(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.F8)
            {
                btGenerateIUDFile_Click(null, null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// изменился текст скрипта
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbScriptIUD_TextChanged(object sender, EventArgs e)
        {
            Query.SQLScript = tbScriptIUD.Text;
        }

        /// <summary>
        /// Открыть форму "Таблицы"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btTable_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbTableNameSQL.Text))
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableNameSQL.Focus();
                return;
            }

            WinTable WinTable = new WinTable();

            if (cbConnectSQL != null)
            {
                ConnectDB ConnectSQL = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, false);
                Utilities.Controls.RefreshConnectItems(WinTable.cbConnectSQL, ConnectSQL.DBConnectionName, null, null);
            }

            Utilities.Controls.RefreshRegionItems(WinTable.cbRegion, Query.GITProject);

            WinTable.isStart = false;
            WinTable.cbConnectSQLChanged();

            WinTable.Show();

            WinTable.tbTableName.Text = tbTableNameSQL.Text;
            WinTable.tbTableName_LostFocus(null, null);
            WinTable.btFillFromDB_Click(null, null);
        }


        /// <summary>Изменилось поле SQL</summary>
        private void tbSQL_TextChanged(object sender, EventArgs e)
        {
            tbSQLTextChanged();
        }

        private void tbTableNameSQL_DropDownClosed(object sender, EventArgs e)
        {
            TableNameSQLChanged();
        }

        private void tbTableNameSQL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbTableNameSQL.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbTableNameSQL.SelectedItem;

                tbTableNameSQL.Text = (string)cbItem.Tag;
            }
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex()+1).ToString();
        }

        private void isRegion_Checked(object sender, RoutedEventArgs e)
        {
            if (cbRegion.SelectedIndex == -1)
            {
                Utilities.Controls.SetSelectedRegion(cbRegion, "0");
            }
        }

        private void isRegion_Unchecked(object sender, RoutedEventArgs e)
        {
            cbRegion.SelectedIndex = -1;
        }

        private void isAddGO_Checked(object sender, RoutedEventArgs e)
        {
            cbInsUpdDTType.SelectedIndex = 1;
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            App.AddLog("На форме \"Запрос\" нажата кнопка \"Остановить\"", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
            StopGenerate(false);
        }

        /// <summary>
        /// Скопировать заголовок поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeaderCopy_Click(object sender, RoutedEventArgs e)
        {
            if (
                (dgData.SelectedCells != null) &&
                (dgData.SelectedCells.Count > 0)
                )
            {
                DataGridCellInfo cellItem = dgData.SelectedCells[0];
                DataGridColumn col = cellItem.Column;
                string column = col.Header.ToString();
                Clipboard.SetText(column.Replace("__", "_"));
            }
        }

        private void CellCopy_Click(object sender, RoutedEventArgs e)
        {
            if (
                (dgData.SelectedCells != null) &&
                (dgData.SelectedCells.Count > 0)
                )
            {
                DataGridCellInfo cellItem = dgData.SelectedCells[0];
                DataGridColumn col = cellItem.Column;
                var data = col.GetCellContent(cellItem.Item);
                if (
                    (data != null) &&
                    (data is TextBlock)
                )
                {
                    string value = (data as TextBlock).Text;
                    Clipboard.SetText(value);
                }
            }
        }

        private void cbAddFK_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (
                (isAddCheckUnique != null) &&
                (cbAddFK.SelectedIndex == 2)
            )
            {
                isAddCheckUnique.IsChecked = true;
            }
        }
    }
}
