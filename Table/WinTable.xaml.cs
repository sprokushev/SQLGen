// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Excel = Microsoft.Office.Interop.Excel;
using SQLGen.Controls;
using SQLGen.Utilities;
using static System.Windows.Forms.LinkLabel;

namespace SQLGen
{
    // -------------------------------------------------------------------------------------------------------
    /// <summary>Типы локальных скриптов</summary>
    public enum ScriptLocalType
    {
        /// <summary>
        /// Нет
        /// </summary>
        NONE,
        /// <summary>
        /// stg.LocalDBList
        /// </summary>
        LocalDBList,
        /// <summary>
        /// nsi.RefTableRegistry
        /// </summary>
        RefTableRegistry,
        /// <summary>
        /// Индекс
        /// </summary>
        Index,
        /// <summary>
        /// Сиквенс
        /// </summary>
        Sequence,
        /// <summary>
        /// Шаблон процедуры
        /// </summary>
        ShablonProc,
        /// <summary>
        /// Шаблон функции
        /// </summary>
        ShablonFunc,
        /// <summary>
        /// Шаблон представления
        /// </summary>
        ShablonView
    }

    /// <summary>
    /// Окно для сборки скриптов с модификацией таблиц
    /// </summary>
    public partial class WinTable : Window
    {

        /// <summary>Подключение к БД</summary>
        public ConnectDB ConnectSQL = null;

        /// <summary>Флаг инициализации окна</summary>
        public bool isStart = true;

        /// <summary>Экземпляр TableDB, с которым работаем в данном окне</summary>
        public TableDB Table = new TableDB();

        /// <summary>Признак локального скрипта</summary>
        public ScriptLocalType ScriptLocalType = ScriptLocalType.NONE;

        /// <summary>Список типов полей</summary>
        public List<string> ListTypes = new List<string> {
            "----- ОСНОВНЫЕ ТИПЫ -----",
            "BIGINT",
            "BYTEA",
            "DATETIME",
            "DOUBLE PRECISION",
            "FLOAT",
            "INT",
            "JSONB",
            "MONEY",
            "NUMERIC",
            "UNIQUEIDENTIFIER",
            "UUID",
            "VARBINARY",
            "VARCHAR",
            "XML",
            "-- ВСПОМОГАТЕЛЬНЫЕ ТИПЫ --",
            "BIT",
            "BOOLEAN",
            "CHAR",
            "DATE",
            "DECIMAL",
            "IMAGE",
            "INTEGER",
            "NTEXT",
            "NVARCHAR",
            "REAL",
            "SMALLINT",
            "TEXT",
            "TINYINT",
            "TIME",
            "TIME WITH TIME ZONE",
            "TIME WITHOUT TIME ZONE",
            "TIMESTAMP WITH TIME ZONE",
            "TIMESTAMP WITHOUT TIME ZONE",
            "TIMESTAMPTZ",
            "TIMETZ",
            "TIMESTAMP"
        };


        /// <summary>Конструктор WinTable</summary>
        public WinTable()
        {
            InitializeComponent();
            DataContext = Table;
            btClearFields_Click(null, null);
            tbTableName.InitHistory("HistoryTableName.json", "");

            lbParentEvnTable.Visibility = Visibility.Hidden;
            cbParentEvnTable.Visibility = Visibility.Hidden;

            // пользовательские настройки GUI
            Default.InitGUI(
                "WinTable",
                this,
                mainGrid,
                new List<System.Windows.Controls.Control> { tbScriptCreate, tbProcScript },
                new List<ComboBox> { cbScriptFontFamily, cbProcScriptFontFamily },
                new List<ComboBox> { cbScriptFontSize, cbProcScriptFontSize },
                MainWindow.Task.LogFile
                );

            // заполняем toolbar'ы
            tbScriptCreate.AddToolbarDefault(toolbarScriptCreate, tiScriptCreate, true, false);
            tbProcScript.AddToolbarDefault(toolbarProcScript, tiProcScript, false, false);

        }

        /// <summary>При активации окна WinTable</summary>
        private void winTable_Activated(object sender, EventArgs e)
        {
            this.Title = "Таблица - " + Table.TableEdit.FullTableName + ", задача " + MainWindow.Task.TaskNumber;
            //App.AddLog("winTable_Activated", null, App.ShowMessageMode.NONE);
        }

        /// <summary>При закрытии окна WinTable</summary>
        private void winTable_Closed(object sender, EventArgs e)
        {
            if (ConnectSQL != null)
            {
                ConnectSQL.CloseConnect();
            }

            // пользовательские настройки GUI
            Default.SaveGUI(
                "WinTable",
                this,
                new List<System.Windows.Controls.Control> { tbScriptCreate, tbProcScript }
                );
        }

        /*
        /// <summary>
        /// Заполнение окна WinTable
        /// </summary>
        /// <param name="_table">экземпляр TableDB</param>
        public void SetTable(TableDB _table)
        {
            Table = new TableDB();

            // по умолчанию
            tbSchemaName.Text = "";
            tbTableName.Text = "";
            tbPKName.Text = "";
            tbTableDesc.Text = "";
            cbTableType.SelectedIndex = -1;
            isAddDrop.IsChecked = false;
            isReglament.IsChecked = true;
            isAddIndex.IsChecked = false;
            cbScriptCreateType.SelectedIndex = -1;
            tbScriptCreate.Text = "";
            tbScriptCreate.Filename = "";
            cbGITProject.SelectedIndex = -1;
            cbParentEvnTable.SelectedIndex = -1;
            //tabAlter.Header = Table.ScriptFilename;

            Table.TableOrig.ListField.Clear();
            Table.TableEdit.ListField.Clear();
            Table.ListIndex.Clear();

            tbTableName.IsReadOnly = false;
            tbSchemaName.IsReadOnly = false;
            cbParentEvnTable.IsReadOnly = false;
            //tbTableName.IsEnabled = tbTableName.IsReadOnly == false; //-V3022
            //tbSchemaName.IsEnabled = tbSchemaName.IsReadOnly == false; //-V3022
            cbParentEvnTable.IsEnabled = cbParentEvnTable.IsReadOnly == false; //-V3022


            // новые значения
            if (_table != null)
            {
                Table.Fill(_table);

                tbSchemaName.Text = _table.TableEdit.SchemaName;
                tbTableName.Text = _table.TableEdit.TableName;
                tbPKName.Text = _table.TableEdit.PKName;
                tbTableDesc.Text = _table.TableEdit.TableDesc;
                cbParentEvnTable.SelectedItem = _table.TableEdit.ParentEvnTable;

                Utilities.Controls.RefreshGITProjectItems(cbGITProject, _table.GITProject);

                switch (_table.TableType)
                {
                    case Utilities.TableType.EVN:
                        cbTableType.SelectedIndex = 1;
                        break;
                    case Utilities.TableType.PERSONEVN:
                        cbTableType.SelectedIndex = 2;
                        break;
                    case Utilities.TableType.MORBUS:
                        cbTableType.SelectedIndex = 3;
                        break;
                    case Utilities.TableType.DICT:
                    default:
                        cbTableType.SelectedIndex = 0;
                        break;
                }

                if (_table.isAddDrop == true) isAddDrop.IsChecked = true; else isAddDrop.IsChecked = false;
                if (_table.isReglament == true) isReglament.IsChecked = true; else isReglament.IsChecked = false;
                if (_table.isAddIndex == true) isAddIndex.IsChecked = true; else isAddIndex.IsChecked = false;

                switch (_table.ScriptType)
                {
                    case Utilities.ScriptType.ALTER:
                        cbScriptCreateType.SelectedIndex = 1;
                        break;
                    case Utilities.ScriptType.CREATE:
                    default:
                        cbScriptCreateType.SelectedIndex = 0;
                        break;
                }

                tbScriptCreate.Text = _table.SQLScript;
                //tabAlter.Header = Table.ScriptFilename;

            }
            TableNameChanged();

            FieldType.ItemsSource = ListTypes;
            dgFields.ItemsSource = Table.TableEdit.ListField;
            dgIndexes.ItemsSource = Table.ListIndex;

            tiStructure.IsSelected = true;
            dgFieldsRefresh();
            dgIndexesRefresh();
            tbTableName.Focus();
        }
        */

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

        /*
        /// <summary>Нажата кнопка Кто менял</summary>
        private void btLastChange_Click(object sender, RoutedEventArgs e)
        {
            // кто последний менял таблицу

            if (tbTableName.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableName.Focus();
                return;
            }

            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                dgDopInfoGrid.ItemsSource = ConnectSQL.GetLastChangeList(Table.TableEdit).DefaultView;
                tabDopInfoGrid.IsSelected = true;
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Arrow;
                App.AddLog("",ex, App.ShowMessageMode.SHOW);
            }
            this.Cursor = Cursors.Arrow;
        }
        */

        /// <summary>Корректировка имен колонок в Grid'е</summary>
        private void dgScriptCreateResults_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e) //-V3013
        {
            string header = e.Column.Header.ToString();

            // Replace all underscores with two underscores, to prevent AccessKey handling
            e.Column.Header = header.Replace("_", "__");

            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy HH:mm:ss.FFF";
        }

        /// <summary>Корректировка имен колонок в Grid'е</summary>
        private void dgProcScriptResults_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();

            // Replace all underscores with two underscores, to prevent AccessKey handling
            e.Column.Header = header.Replace("_", "__");

            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy HH:mm:ss.FFF";
        }

        /// <summary>Нажата кнопка Сохранить в файл на вкладке Script</summary>
        private void btSaveCreate_Click(object sender, RoutedEventArgs e)
        {
            FileStream fs = null;
            Encoding encoding = new UTF8Encoding(false);

            try
            {
                // определить номер скрипта 
                Table.ScriptNumber = (Utilities.Files.MaxScriptNumber(MainWindow.Task.TaskPath) + 1).ToString();

                // чтобы исключить повторное нажатие на кнопку клавишей Enter
                tiScriptCreateMessages.Focus();

                // запросить номер скрипта
                FormAskNumFile dlg1 = new FormAskNumFile();
                dlg1.tbNumFile.Text = Table.ScriptNumber;
                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    Table.ScriptNumber = dlg1.tbNumFile.Text.Trim();
                dlg1.Dispose();

                // имя файла для скрипта
                string filename;
                switch (ScriptLocalType)
                {
                    case ScriptLocalType.LocalDBList:
                        filename = Table.LocalDBListFilename;
                        break;
                    case ScriptLocalType.RefTableRegistry:
                        filename = Table.RefTableRegistryFilename;
                        break;
                    case ScriptLocalType.Index:
                        filename = TableDB.IndexFilename(Table.TableEdit);
                        break;
                    case ScriptLocalType.NONE:
                    default:
                        filename = TableDB.ScriptFilename(Table.TableEdit);
                        break;
                }
                filename = Controls.Dialogs.SaveSQLDialog(MainWindow.Task.TaskPath, filename, out fs, out FileMode fileMode);

                //сохранить файл
                if (fs != null)
                {
                    tbScriptCreate.Filename = "";
                    Utilities.Files.WriteScript(filename, fs, tbScriptCreate.Text, true, out string err, fileMode);
                    tbScriptCreate.Filename = filename;
                }
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
        }

        /// <summary>Нажата кнопка В буфер обмена на вкладке Script</summary>
        private void btClipboardCreate_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(tbScriptCreate.Text);
        }

        /// <summary>Обновить список полей</summary>
        public void dgFieldsRefresh()
        {
            ListCollectionView cvTasks = (ListCollectionView)CollectionViewSource.GetDefaultView(dgFields.ItemsSource);

            if (cvTasks != null)
            {
                if (cvTasks.IsAddingNew) cvTasks.CommitNew();
                if (cvTasks.IsEditingItem) cvTasks.CommitEdit();
            }

            if (cvTasks != null && cvTasks.CanSort == true)
            {
                cvTasks.SortDescriptions.Clear();
                cvTasks.SortDescriptions.Add(new SortDescription("FieldOrder", ListSortDirection.Ascending));
            }

            dgFields.Items.Refresh();
        }

        /// <summary>Обновить список индексов</summary>
        private void dgIndexesRefresh()
        {
            dgIndexes.Items.Refresh();
        }


        private void TableNameChanged(bool isAlways = false)
        {
            string name = tbTableName.Text.Trim().Replace("[", "").Replace("]", "").Replace("\"", "").Replace(" ", ".").Replace("\t", ".").Trim();
            string schema = tbSchemaName.Text.Trim().Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
            //if (name == null) name = "";
            //if (schema == null) schema = "";

            if (string.IsNullOrWhiteSpace(schema))
            {
                schema = "dbo";
                tbSchemaName.Text = schema;
                tbSchemaName_TextChanged(null, null);
            }

            if (schema == "EMD")
            {
                schema = "\"" + schema + "\"";
                name = "\"" + name + "\"";

                //Utilities.SetComboBoxGITProjectByName(cbGITProject, "emd");

                tbSchemaName.Text = schema;
                tbSchemaName_TextChanged(null, null);
                tbTableName.Text = name;
            }

            if ((Table.TableEdit.TableName != name) || isAlways)
            {
                var arr = name.Split('.');

                if (arr.Length > 1)
                {
                    schema = arr[0];
                    name = arr[1];
                }

                if (!Utilities.Databases.IsTableNameCorrect(name))
                {
                    MessageBox.Show("Имя таблицы " + name + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), двойные кавычки (\")");
                }
                if (!Utilities.Databases.IsSchemaNameCorrect(schema))
                {
                    MessageBox.Show("Имя схемы " + schema + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), двойные кавычки (\")");
                }

                Table.TableEdit.TableName = name;
                tbTableName.Text = Table.TableEdit.TableName;
                tbSchemaName.Text = schema;
                tbSchemaName_TextChanged(null, null);

                if (Table.TableEdit.SchemaNameCompare == "dbo")
                    tbLocalDBListName.Text = Table.TableEdit.TableNameReady;
                else
                    tbLocalDBListName.Text = Table.TableEdit.FullTableNameReady;

                tbProcSchemaFilter.Text = Table.TableEdit.SchemaNameReady;
                tbProcNameFilter.Text = @"%" + Table.TableEdit.TableNameReady + "%";
            }

            this.Title = "Таблица - " + Table.TableEdit.FullTableName + ", задача " + MainWindow.Task.TaskNumber;
        }

        /*
        /// <summary>Изменилось значение в поле Таблица</summary>
        private void tbTableName_TextChanged(object sender, TextChangedEventArgs e)
        {
            TableNameChanged();
        }
        */

        /// <summary>Изменилось значение в поле Схема</summary>
        private void tbSchemaName_TextChanged(object sender, TextChangedEventArgs e)
        {
            //string name = tbTableName.Text.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
            string schema = tbSchemaName.Text.Replace("\"", "").Trim();
            //if (name == null) name = "";
            //if (schema == null) schema = "";

            if (schema == "EMD")
            {
                schema = "\"" + schema + "\"";
                //name = "\"" + name + "\"";

                //Utilities.SetComboBoxGITProjectByName(cbGITProject, "emd");

                tbSchemaName.Text = schema;
                //tbTableName.Text = name;
            }

            if (Table.TableEdit.SchemaName != schema)
            {
                if (!Utilities.Databases.IsSchemaNameCorrect(schema))
                {
                    MessageBox.Show("Имя схемы " + schema + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), двойные кавычки (\")");
                }

                Table.TableEdit.SchemaName = schema;

                if (Utilities.Databases.regex_region.IsMatch(Table.TableEdit.SchemaNameCompare))
                {
                    isRegion.IsChecked = true;
                    Utilities.Controls.SetSelectedRegion(cbRegion, Table.TableEdit.SchemaNameReady.Substring(1));
                }
                else
                {
                    isRegion.IsChecked = false;
                    cbRegion.SelectedIndex = -1;
                }
                if (Table.TableEdit.SchemaNameCompare == "dbo")
                    tbLocalDBListName.Text = Table.TableEdit.TableNameReady;
                else
                    tbLocalDBListName.Text = Table.TableEdit.FullTableNameReady;
            }
        }

        /// <summary>Изменилось значение в поле Описание таблицы</summary>
        private void tbTableDesc_TextChanged(object sender, TextChangedEventArgs e)
        {
            Table.TableEdit.TableDesc = tbTableDesc.Text;
        }

        /// <summary>Изменилось значение в поле Primary Key</summary>
        private void tbPKName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Table.TableEdit.PKName = tbPKName.Text;
        }


        /// <summary>Нажата кнопка Сменить - для изменения имени таблицы</summary>
        private void btChangeTableName_Click(object sender, RoutedEventArgs e)
        {
            FormNewTableName dlg1 = new FormNewTableName();

            dlg1.tbOldTableName.Text = Table.TableEdit.TableName;

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Table.TableEdit.RenameTable(dlg1.tbNewTableName.Text.Trim());

                tbTableName.Text = Table.TableEdit.TableName;
                TableNameChanged(true);
                tbPKName.Text = Table.TableEdit.PKName;
            };

            dlg1.Dispose();

            dgFieldsRefresh();
        }

        /// <summary>Нажата кнопка Авто - для заполнения названия Primary Key</summary>
        private void btAutoPKName_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(Table.TableEdit.TableNameReady))
            {
                tbPKName.Text = "pk_" + Table.TableEdit.TableNameReady + "_id";
            }
        }

        /// <summary>Удаление поля из контекстного меню</summary>
        private void DeleteField_Click(object sender, RoutedEventArgs e)
        {

            if (dgFields.SelectedIndex >= 0)
            {
                FieldDB field = dgFields.SelectedItem as FieldDB;

                if (!string.IsNullOrWhiteSpace(field.InheritParentTable))
                {
                    MessageBox.Show($"Поле {field.FieldName} унаследовано из таблицы {field.InheritParentTable}, удаляем только в ней");
                }
                else
                {
                    Table.TableEdit.ListField.Remove(field);
                    dgFieldsRefresh();
                }
            }
        }

        /// <summary>Добавление поля из контекстного меню</summary>
        private void AddField_Click(object sender, RoutedEventArgs e)
        {
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_", "BIGINT");
            dgFieldsRefresh();
        }

        /// <summary>Нажата кнопка Очистить список полей и индексов</summary>
        private void btClearFields_Click(object sender, RoutedEventArgs e)
        {

            FieldType.ItemsSource = ListTypes;

            Table.TableOrig.ListField.Clear();
            Table.TableEdit.ListField.Clear();
            Table.ListIndex.Clear();
            dgProcListGrid.ItemsSource = null;

            dgFields.ItemsSource = Table.TableEdit.ListField;
            dgIndexes.ItemsSource = Table.ListIndex;

            tbPKName.Text = "";
            tbTableDesc.Text = "";

            cbScriptCreateType.SelectedIndex = 0;

            cbParentEvnTable.SelectedIndex = -1;

            Table.TableOrig.ParentEvnTable = "";
            Table.TableEdit.ParentEvnTable = "";
            Table.TableOrig.ForeignWord = "";
            Table.TableEdit.ForeignWord = "";
            Table.TableOrig.ForeignServer = "";
            Table.TableEdit.ForeignServer = "";
            Table.TableOrig.ForeignOptions = "";
            Table.TableEdit.ForeignOptions = "";
            Table.TableOrig.HasRegionDescr = false;
            Table.TableEdit.HasRegionDescr = false;
            Table.TableOrig.HasSequence = false;
            Table.TableEdit.HasSequence = false;
            Table.TableOrig.HasInherit = false;
            Table.TableEdit.HasInherit = false;

            Table.TableOrig.PartitionFunctionName = "";
            Table.TableEdit.PartitionFunctionName = "";
            Table.TableOrig.PartitionFieldType = "";
            Table.TableEdit.PartitionFieldType = "";
            Table.TableOrig.PartitionFieldSize = "";
            Table.TableEdit.PartitionFieldSize = "";
            Table.TableOrig.PartitionFieldDec = "";
            Table.TableEdit.PartitionFieldDec = "";
            Table.TableOrig.PartitionType = "";
            Table.TableEdit.PartitionType = "";
            Table.TableOrig.PartitionBoundary = "";
            Table.TableEdit.PartitionBoundary = "";
            Table.TableOrig.PartitionRangeValues = "";
            Table.TableEdit.PartitionRangeValues = "";
            Table.TableOrig.PartitionSchemeName = "";
            Table.TableEdit.PartitionSchemeName = "";
            Table.TableOrig.PartitionField = "";
            Table.TableEdit.PartitionField = "";

            cbTableType.SelectedIndex = 0;

            tbTableName.IsReadOnly = false;
            tbSchemaName.IsReadOnly = false;
            cbParentEvnTable.IsReadOnly = false;
            //tbTableName.IsEnabled = tbTableName.IsReadOnly == false; //-V3022
            //tbSchemaName.IsEnabled = tbSchemaName.IsReadOnly == false; //-V3022
            cbParentEvnTable.IsEnabled = cbParentEvnTable.IsReadOnly == false; //-V3022

            dgFieldsRefresh();
            dgIndexesRefresh();
        }


        /// <summary>Завершено редактирование поля</summary>
        private void dgFields_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string fieldname = e.Column.SortMemberPath;
            FieldDB Row = (FieldDB)e.Row.Item;

            if (fieldname == "FieldName")
            {
                TextBox t = e.EditingElement as TextBox;
                if (t != null)
                {
                    if (!Utilities.Databases.IsFieldNameCorrect(t.Text.Trim()))
                    {
                        MessageBox.Show("Имя поля " + t.Text.Trim() + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), двойные кавычки (\")");
                    }
                }
            }

            if (fieldname == "FKName")
            {
                TextBox t = e.EditingElement as TextBox;
                if (t != null)
                {
                    if (!Utilities.Databases.IsConstraintNameCorrect(t.Text.Trim()))
                    {
                        MessageBox.Show("Имя констрейна " + t.Text.Trim() + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), двойные кавычки (\")");
                    }
                }
            }

            if (fieldname == "FKField")
            {
                TextBox t = e.EditingElement as TextBox;
                if (t != null)
                {
                    if (!Utilities.Databases.IsFKFieldNameCorrect(t.Text.Trim()))
                    {
                        MessageBox.Show("Имя поля " + t.Text.Trim() + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), запятая (,), знак подчеркивания (_), двойные кавычки (\")");
                    }
                }
            }

            if (
                fieldname == "FKOrder" ||
                fieldname == "PKOrder"
                )
            {
                TextBox t = e.EditingElement as TextBox;
                if (t != null)
                {
                    if (!Utilities.Databases.IsOrderCorrect(t.Text.Trim()))
                    {
                        MessageBox.Show("Имя поля " + t.Text.Trim() + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены только цифры");
                    }
                }
            }

            if (fieldname == "IsPK")
            {
                CheckBox t = e.EditingElement as CheckBox;
                if (t != null)
                {
                    if (
                        t.IsChecked == true &&
                        string.IsNullOrWhiteSpace(Row.PKOrder)
                     )
                    {
                        int order = Table.TableEdit.ListField
                            .Where(x => x.FieldNameCompare != Row.FieldNameCompare)
                            .Max(x =>
                            {
                                if (int.TryParse(x.PKOrder, out int n))
                                {
                                    return n;
                                }

                                return 0;
                            });

                        Row.PKOrder = (order + 1).ToString();
                    }

                    if (
                        t.IsChecked == false &&
                        !string.IsNullOrWhiteSpace(Row.PKOrder)
                     )
                    {
                        Row.PKOrder = "";
                    }
                }
            }

            if (fieldname == "FKTable")
            {
                TextBox t = e.EditingElement as TextBox;
                if (t != null)
                {
                    if (!Utilities.Databases.IsTableNameCorrect(t.Text.Trim()))
                    {
                        MessageBox.Show("Имя таблицы " + t.Text.Trim() + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), двойные кавычки (\")");
                    }

                    string tablename = Utilities.Databases.GetFullTableName(t.Text.Trim());
                    Row.FKTable = tablename;

                    if (
                        (!string.IsNullOrWhiteSpace(Row.FKTable)) && 
                        string.IsNullOrWhiteSpace(Row.FieldDesc)
                    )
                    {
                        string desc = "";

                        try
                        {
                            if (CheckConnectSQL())
                            {
                                desc = ConnectSQL.GetTableDecription(Row.FKTable);
                            }
                        }
                        catch (Exception ex)
                        {
                            desc = "";
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }

                        Row.FieldDesc = desc;
                    }

                    if (
                        (!string.IsNullOrWhiteSpace(Row.FKTable)) && 
                        string.IsNullOrWhiteSpace(Row.FKName)
                    )
                    {
                        Row.FKName = Table.TableEdit.GetFKNameDefault(Row.FieldNameReady);
                    }

                    if (!string.IsNullOrWhiteSpace(Row.FKTable)) 
                    {
                        string fkfield = "";
                        string pktype = "";

                        try
                        {
                            if (CheckConnectSQL())
                            {
                                var pkinfo = ConnectSQL.GetTablePK(Row.FKTable);

                                // ищем поле
                                var found = pkinfo.GetField(Row.FieldNameCompare);

                                if (found != null) 
                                {
                                    // есть совпадения по имени поля
                                    fkfield = found.FieldName;
                                    pktype = found.FieldType;
                                    Row.FKTable = pkinfo.TableName;
                                }
                                else if (pkinfo.HasPK) 
                                {
                                    // нет совпадения по имени поля - берем первое
                                    fkfield = pkinfo.FirstFieldName;
                                    pktype = pkinfo.FirstFieldType;
                                    Row.FKTable = pkinfo.TableName;
                                }
                                else
                                {
                                    MessageBox.Show("Не найден Primary Key для таблицы " + Row.FKTable);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            fkfield = "";
                            pktype = "";
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }

                        if (string.IsNullOrWhiteSpace(fkfield))
                        {
                            fkfield = Utilities.Databases.GetTableName(Row.FKTableNameReady) + "_id";
                        }

                        Row.FKField = fkfield;

                        if (!string.IsNullOrWhiteSpace(pktype))
                        {
                            Row.FieldType = pktype;
                        }
                    }
                }
            }
        }

        /// <summary>Добавлено поле id</summary>
        private void AddFieldId_Click(object sender, RoutedEventArgs e)
        {
            // добавляю id
            dgFields.Focus();

            if (
                (!GITProjects.GetisEvnInheritByProject(Table.GITProject)) &&
                (Table.TargetDB == Utilities.TargetDBType.PGSQL) &&
                (Table.TableType == Utilities.TableType.EVN)
            )
            {
                Table.TableEdit.AddField("", "evn_id", "BIGINT", "", "", "Уникальный идентификатор", "true", "true", "true");
            }
            else
            {
                Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_id", "BIGINT", "", "", "Уникальный идентификатор", "true", "true", "true");
            }

            dgFieldsRefresh();
            if (string.IsNullOrWhiteSpace(tbPKName.Text)) btAutoPKName_Click(sender, e);
        }

        /// <summary>Добавлены поля - id, Code, Name</summary>
        private void AddFieldIdCodeName_Click(object sender, RoutedEventArgs e)
        {
            // добавляю id
            AddFieldId_Click(null, null);

            // добавляю Code, Name
            dgFields.Focus();

            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_Code", "VARCHAR", "15", "", "Код");
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_Name", "VARCHAR", "100", "", "Наименование");

            dgFieldsRefresh();
        }

        /// <summary>Добавлены поля - кто добавил, кто изменил</summary>
        private void AddFieldInsUpdID_Click(object sender, RoutedEventArgs e)
        {
            // добавляю кто добавил, кто изменил
            dgFields.Focus();
            Table.TableEdit.AddField("", "pmUser_insID", "BIGINT", "", "", "Пользователь, добавивший запись", "true");
            Table.TableEdit.AddField("", "pmUser_updID", "BIGINT", "", "", "Пользователь, обновивший запись", "true");
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_insDT", "DATETIME", "", "", "Дата и время добавления записи", "true");
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_updDT", "DATETIME", "", "", "Дата и время обновления записи", "true");
            dgFieldsRefresh();
        }

        /// <summary>Добавлены поля - SysNick</summary>
        private void AddFieldSysNick_Click(object sender, RoutedEventArgs e)
        {
            // добавляю SysNick
            dgFields.Focus();
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_SysNick", "VARCHAR", "20", "", "Системное наименование");
            dgFieldsRefresh();
        }

        /// <summary>Добавлены поля - Descr</summary>
        private void AddFieldDescr_Click(object sender, RoutedEventArgs e)
        {
            // добавляю Descr
            dgFields.Focus();
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_Descr", "VARCHAR", "200", "", "Описание");
            dgFieldsRefresh();
        }

        /// <summary>Добавлены поля - дата начала/окончания</summary>
        private void AddFieldBegEndDate_Click(object sender, RoutedEventArgs e)
        {
            // добавляю Период действия (begDate, endDate)
            dgFields.Focus();
            Table.TableEdit.AddField("0", Table.TableEdit.TableNameReady + "_begDate", "DATETIME", "", "", "Дата начала действия");
            Table.TableEdit.AddField("0", Table.TableEdit.TableNameReady + "_endDate", "DATETIME", "", "", "Дата окончания действия");
            dgFieldsRefresh();
        }

        /// <summary>Добавлены поля - дата/время начала/окончания</summary>
        private void AddFieldBegEndDT_Click(object sender, RoutedEventArgs e)
        {
            // добавляю Период действия (begDate, endDate)
            dgFields.Focus();
            Table.TableEdit.AddField("0", Table.TableEdit.TableNameReady + "_begDT", "DATETIME", "", "", "Дата и время начала действия");
            Table.TableEdit.AddField("0", Table.TableEdit.TableNameReady + "_endDT", "DATETIME", "", "", "Дата и время окончания действия");
            dgFieldsRefresh();
        }

        /// <summary>Добавлены поля - SetDate</summary>
        private void AddFieldSetDate_Click(object sender, RoutedEventArgs e)
        {
            // добавляю Дата (setDate)
            dgFields.Focus();
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_setDate", "DATETIME", "", "", "Дата документа");
            dgFieldsRefresh();
        }

        /// <summary>Добавлены поля - SetDT</summary>
        private void AddFieldSetDT_Click(object sender, RoutedEventArgs e)
        {
            // добавляю Дата (setDT)
            dgFields.Focus();
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_setDT", "DATETIME", "", "", "Дата и время документа");
            dgFieldsRefresh();
        }

        /// <summary>Добавлены поля - признак удаления</summary>
        private void AddFieldDelID_Click(object sender, RoutedEventArgs e)
        {
            // добавляю Признак удаления (deleted)
            dgFields.Focus();
            string isNotNull = "false";

            if (Table.ScriptType == Utilities.ScriptType.CREATE)
            {
                isNotNull = "true";
            }
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_deleted", "BIGINT", "", "", "Признак удаления", isNotNull, "false", "false", "", "1", "fk_" + Table.TableEdit.TableNameReady + "_deleted", "dbo.YesNo", "YesNo_id");
            Table.TableEdit.AddField("", "pmUser_delID", "BIGINT", "", "", "Пользователь, удаливший запись");
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_delDT", "DATETIME", "", "", "Дата и время удаления записи");
            dgFieldsRefresh();
        }

        /// <summary>Добавлены поля - is</summary>
        private void AddFieldIs_Click(object sender, RoutedEventArgs e)
        {
            // добавляю Да/Нет (is)
            dgFields.Focus();
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_is", "BIGINT", "", "", "", "false", "false", "false", "", "", "", "dbo.YesNo", "YesNo_id");
            dgFieldsRefresh();
        }

        /// <summary>Добавлены поля - region_id</summary>
        private void AddFieldRegion_Click(object sender, RoutedEventArgs e)
        {
            // добавляю Регион (Region_id)
            dgFields.Focus();
            Table.TableEdit.AddField("", "Region_id", "BIGINT", "", "", "Идентификатор региона");
            dgFieldsRefresh();
        }

        private void AddConstraint()
        {
            dgFields.Focus();

            if (tbTableName.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableName.Focus();
                return;
            }

            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            if (dgFields.SelectedIndex >= 0)
            {
                FieldDB field = dgFields.SelectedItem as FieldDB;

                WinAddFK WinAddFK = new WinAddFK();
                WinAddFK.winTable = this;
                WinAddFK.Connect = this.ConnectSQL;
                WinAddFK.SchemaName = Table.TableEdit.SchemaNameReady;
                WinAddFK.TableName = Table.TableEdit.TableNameReady;

                WinAddFK.dgFields.ItemsSource = Table.TableEdit.ListField
                    .OrderBy(x => x.FieldOrder)
                    .Select(x => new FieldToList() { Name = x.FieldNameReady, Order = x.FieldOrder })
                    .ToList();

                WinAddFK.dgFieldsRefresh();

                WinAddFK.FieldName.ItemsSource = Table.TableEdit.ListField
                    .OrderBy(x => x.FieldOrder)
                    .Select(x => x.FieldNameReady)
                    .ToList();

                if (
                    string.IsNullOrWhiteSpace(field.FKTable) &&
                    string.IsNullOrWhiteSpace(field.FieldCheck)
                )
                {
                    WinAddFK.Title = $"Добавление нового констрейна для поля {field.FieldName} в таблице {Table.TableEdit.FullTableName}";
                    WinAddFK.rbFKOther.IsChecked = true;
                    WinAddFK.tbFKTable.Text = "";
                    WinAddFK.FKTableChanged();
                    WinAddFK.AddFieldToLink(field.FieldNameReady, "", 0);
                }
                else
                {
                    WinAddFK.Title = $"Изменение констрейна {field.FKName} в таблице {Table.TableEdit.FullTableName}";

                    /*if (field.FKFullTableNameCompare == "dbo.yesno")
                    {
                        WinAddFK.rbFKIs.IsChecked = true;
                        WinAddFK.AddFieldToLink(field.FieldNameReady, "YesNo_id", 1);
                    }
                    else if (field.FKFullTableNameCompare == "dbo.klarea")
                    {
                        WinAddFK.rbFKRegion.IsChecked = true;
                        WinAddFK.AddFieldToLink(field.FieldNameReady, "KLArea_id", 1);
                    }
                    else */
                    if (!string.IsNullOrWhiteSpace(field.FKTable))
                    {
                        WinAddFK.rbFKOther.IsChecked = true;
                        WinAddFK.tbFKTable.Text = field.FKFullTableNameReady;
                        WinAddFK.FKTableChanged();

                        foreach (var row in Table.TableEdit.ListField
                            .Where(x => x.FKNameCompare == field.FKNameCompare)
                            .OrderBy(x => x.FKOrderToSort))
                        {
                            WinAddFK.AddFieldToLink(row.FieldNameReady, row.FKFieldReady, row.FKOrderToInt);
                        }
                    } 
                    else if (!string.IsNullOrWhiteSpace(field.FieldCheck))
                    {
                        WinAddFK.tbCHECK.Text = field.FieldCheck;
                        WinAddFK.rbCHECK.IsChecked = true;
                        WinAddFK.AddFieldToLink(field.FieldNameReady, "", 0);
                    }

                    if (!string.IsNullOrWhiteSpace(field.FKNameReady))
                    {
                        WinAddFK.tbFKName.Text = field.FKNameReady;
                    }
                }

                WinAddFK.ShowDialog();
            }
        }

        /// <summary>Добавляем констрейн</summary>
        private void AddConstraint_Click(object sender, RoutedEventArgs e) //-V3013
        {
            AddConstraint();
        }

        /// <summary>Изменяем констрейн</summary>
        private void EditConstraint_Click(object sender, RoutedEventArgs e)
        {
            AddConstraint();
        }

        /// <summary>
        /// Удалить констрейн
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DelConstraint_Click(object sender, RoutedEventArgs e)
        {
            if (dgFields.SelectedIndex >= 0)
            {
                FieldDB field = dgFields.SelectedItem as FieldDB;

                if (!string.IsNullOrWhiteSpace(field.FKName))
                {
                    if (System.Windows.Forms.MessageBox.Show($"Удалить констрейн {field.FKName} для поля {field.FieldName} в таблице  {Table.TableEdit.FullTableName} ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        string _fkname = field.FKNameCompare;

                        foreach (var row in Table.TableEdit.ListField
                            .Where(x => x.FKNameCompare == _fkname)
                        )
                        {
                            row.FKName = "";
                            row.FKTable = "";
                            row.FKField = "";
                            row.FKOrder = "";
                            row.FieldCheck = "";
                        }

                        dgFieldsRefresh();
                    }
                }
            }
        }


        /// <summary>
        /// Нажата кнопка Заполнить из БД
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void btFillFromDB_Click(object sender, RoutedEventArgs e)
        {
            if (tbTableName.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableName.Focus();
                return;
            }

            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                return;
            }

            btClearFields_Click(sender, e);

            Utilities.Controls.RefreshGITProjectItems(cbGITProject, ConnectSQL.GITProject);
            cbGITProjectChanged();

            bool isfound = false;

            try
            {
                Table.TableOrig.TableName = tbTableName.Text;
                Table.TableOrig.SchemaName = tbSchemaName.Text;

                // 1. Информация о таблице
                while (!isfound)
                {
                    this.Cursor = Cursors.Wait;
                    isfound = ConnectSQL.FillTableInfo(Table.TableOrig);
                    this.Cursor = Cursors.Arrow;

                    if (isfound)
                    {
                        tbSchemaName.Text = Table.TableOrig.SchemaName;
                        tbTableName.Text = Table.TableOrig.TableName;
                        TableNameChanged();
                        tbTableDesc.Text = Table.TableOrig.TableDesc;
                        tbPKName.Text = Table.TableOrig.PKName;
                        cbParentEvnTable.SelectedItem = Table.TableOrig.ParentEvnTable;

                        if (Table.TableOrig.isForeignTable)
                        {
                            App.AddLog("Таблица " + Table.TableOrig.FullTableName + " - внешняя (FOREIGN) !" + Environment.NewLine + "Возможна некорректная генерация скриптов либо потребуется ручная доработка, например имена внешних таблиц и полей для маппинга!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);

                            Table.TableEdit.ForeignServer = Table.TableOrig.ForeignServer;
                            Table.TableEdit.ForeignWord = Table.TableOrig.ForeignWord;
                            Table.TableEdit.ForeignOptions = Table.TableOrig.ForeignOptions;
                        }

                        Table.TableEdit.HasRegionDescr = Table.TableOrig.HasRegionDescr;
                        Table.TableEdit.HasSequence = Table.TableOrig.HasSequence;
                        Table.TableEdit.HasInherit = Table.TableOrig.HasInherit;

                        if (Table.TableOrig.isPartitionTable)
                        {
                            App.AddLog("Таблица " + Table.TableOrig.FullTableName + " - секционированная !" + Environment.NewLine + "Возможна некорректная генерация скриптов либо потребуется ручная доработка, например добавление секций!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);

                            Table.TableEdit.PartitionFunctionName = Table.TableOrig.PartitionFunctionName;
                            Table.TableEdit.PartitionFieldType = Table.TableOrig.PartitionFieldType;
                            Table.TableEdit.PartitionFieldSize = Table.TableOrig.PartitionFieldSize;
                            Table.TableEdit.PartitionFieldDec = Table.TableOrig.PartitionFieldDec;
                            Table.TableEdit.PartitionType = Table.TableOrig.PartitionType;
                            Table.TableEdit.PartitionBoundary = Table.TableOrig.PartitionBoundary;
                            Table.TableEdit.PartitionRangeValues = Table.TableOrig.PartitionRangeValues;
                            Table.TableEdit.PartitionSchemeName = Table.TableOrig.PartitionSchemeName;
                            Table.TableEdit.PartitionField = Table.TableOrig.PartitionField;
                        }
                    }
                    else
                    {
                        if (System.Windows.Forms.MessageBox.Show($"Таблица {Table.TableOrig.FullTableName} не найдена !" + Environment.NewLine + Environment.NewLine + "Поискать в БД похожие по имени таблицы ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            var list = ConnectSQL.FillAlternateTable(Table.TableOrig.TableNameToSeek);
                            bool ischoosed = false;

                            FormFindInList dlg1 = new FormFindInList(MainWindow.Task.LogFile);

                            dlg1.AddItems(list);

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
                                    Table.TableOrig.SchemaName = Utilities.Databases.GetSchemaName(result);
                                    Table.TableOrig.TableName = Utilities.Databases.GetTableName(result);
                                    ischoosed = true;
                                }
                            }

                            dlg1.Dispose();

                            if (!ischoosed)
                            {
                                App.AddLog($"Таблица {Table.TableOrig.FullTableName} не найдена !", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                break;
                            }
                        }
                        else
                        {
                            App.AddLog($"Таблица {Table.TableOrig.FullTableName} не найдена !", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                            break;
                        }

                    }
                }

                // 2. Поля
                this.Cursor = Cursors.Wait;
                isfound = ConnectSQL.FillListField(Table.TableOrig);
                this.Cursor = Cursors.Arrow;

                if (isfound)
                {

                    foreach (var item in Table.TableOrig.ListField)
                    {
                        if (!ListTypes.Contains(item.FieldType)) { ListTypes.Add(item.FieldType); }
                    }

                    Table.TableEdit.ListField.Clear();
                    Table.TableEdit.ListField = Table.TableOrig.ListField.Select(item => new FieldDB(Table, Table.TableEdit, item)).ToList();

                    cbScriptCreateType.SelectedIndex = 1;
                    tbTableName.IsReadOnly = true;
                    tbSchemaName.IsReadOnly = true;
                    cbParentEvnTable.IsReadOnly = true;
                    //tbTableName.IsEnabled = tbTableName.IsReadOnly == false; //-V3022
                    //tbSchemaName.IsEnabled = tbSchemaName.IsReadOnly == false; //-V3022
                    cbParentEvnTable.IsEnabled = cbParentEvnTable.IsReadOnly == false; //-V3022

                    dgFields.ItemsSource = Table.TableEdit.ListField;

                    dgFieldsRefresh();
                }

                // 3. Индексы
                isfound = false;
                try
                {
                    this.Cursor = Cursors.Wait;
                    isfound = ConnectSQL.FillListIndex(Table, "");
                    this.Cursor = Cursors.Arrow;
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
                if (isfound)
                {
                    dgIndexes.ItemsSource = Table.ListIndex;
                    dgIndexesRefresh();
                }

                // 4. Информация о типе таблицы
                cbTableType.SelectedIndex = 0;
                try
                {
                    switch (ConnectSQL.GetTableType(Table.TableOrig))
                    {
                        case Utilities.TableType.EVN:
                            cbTableType.SelectedIndex = 1;
                            break;
                        case Utilities.TableType.PERSONEVN:
                            cbTableType.SelectedIndex = 2;
                            break;
                        case Utilities.TableType.MORBUS:
                            cbTableType.SelectedIndex = 3;
                            break;
                        case Utilities.TableType.DICT:
                        default:
                            cbTableType.SelectedIndex = 0;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                }

                // Добавить в историю
                tbTableName.AddHistory(Table.TableEdit.FullTableName);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
            this.Cursor = Cursors.Arrow;
        }


        /// <summary>Нажата кнопка Сгенерировать скрипт</summary>
        private void btGenerateCreate_Click(object sender, RoutedEventArgs e)
        {
            if (
                (!CheckConnectSQL()) &&
                (cbScriptCreateType.SelectedIndex != 5) &&
                (cbScriptCreateType.SelectedIndex != 6) &&
                (cbScriptCreateType.SelectedIndex != 7)
            )
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                return;
            }

            if (
                (tbTableName.Text.Trim() == "") &&
                (cbScriptCreateType.SelectedIndex != 5) &&
                (cbScriptCreateType.SelectedIndex != 6) &&
                (cbScriptCreateType.SelectedIndex != 7)
            )
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableName.Focus();
                return;
            }

            if (
                (!Utilities.Databases.IsTableNameCorrect(tbTableName.Text.Trim())) &&
                (cbScriptCreateType.SelectedIndex != 5) &&
                (cbScriptCreateType.SelectedIndex != 6) &&
                (cbScriptCreateType.SelectedIndex != 7)
            )
            {
                MessageBox.Show("Имя таблицы " + tbTableName.Text.Trim() + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), двойные кавычки (\")");
                tbTableName.Focus();
                return;
            }

            if (
                (cbScriptCreateType.SelectedIndex != 5) &&
                (cbScriptCreateType.SelectedIndex != 6) &&
                (cbScriptCreateType.SelectedIndex != 7)
            )
            {
                foreach (var item in Table.TableEdit.ListFilteredField(null, false))
                {
                    if (!Utilities.Databases.IsFieldNameCorrect(item.FieldName))
                    {
                        MessageBox.Show("Имя поля " + item.FieldName + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), двойные кавычки (\")");
                        dgFields.Focus();
                        return;
                    }
                    if (!Utilities.Databases.IsConstraintNameCorrect(item.FKName))
                    {
                        MessageBox.Show("Имя констрейна " + item.FKName + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), двойные кавычки (\")");
                        dgFields.Focus();
                        return;
                    }
                    if (!Utilities.Databases.IsTableNameCorrect(item.FKTable))
                    {
                        MessageBox.Show("В констрейне " + item.FKName + " имя таблицы " + item.FKTable + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), двойные кавычки (\")");
                        dgFields.Focus();
                        return;
                    }
                    if (!Utilities.Databases.IsFKFieldNameCorrect(item.FKField))
                    {
                        MessageBox.Show("В констрейне " + item.FKName + " имя поля " + item.FKField + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), запятая (,), знак подчеркивания (_), двойные кавычки (\")");
                        dgFields.Focus();
                        return;
                    }
                }
            }

            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            if (
                (cbScriptCreateType.SelectedIndex != 5) &&
                (cbScriptCreateType.SelectedIndex != 6) &&
                (cbScriptCreateType.SelectedIndex != 7)
            )
            {
                // Добавить в историю
                tbTableName.AddHistory(Table.TableEdit.FullTableName);
            }

            this.Cursor = Cursors.Wait;

            // дозаполним имя FK
            foreach (var item in Table.TableEdit.ListField)
            {
                if (
                    (!string.IsNullOrWhiteSpace(item.FKTable)) &&
                    string.IsNullOrWhiteSpace(item.FKName)
                )
                {
                    item.FKName = Table.TableEdit.GetFKNameDefault(item.FieldNameReady);
                }
            }

            // установим флаг использования поля при генерации скриптов
            foreach (var item in Table.TableOrig.ListField)
            {
                item.IsUsed = true;
            }
            foreach (var item in Table.TableEdit.ListField)
            {
                item.IsUsed = true;
            }

            if (cbScriptCreateType.SelectedIndex == 8)
            {
                // выберем поля, для которых надо принудительно собрать ALTER TABLE ADD COLUMN
                FormCheckedListBox dlg1 = new FormCheckedListBox();
                dlg1.clbList.Items.Clear();

                foreach (var item in Table.TableOrig.ListFilteredField(null, false))
                {
                    if (!dlg1.clbList.Items.Contains(item.FieldNameReady))
                    {
                        dlg1.clbList.Items.Add(item.FieldNameReady, false);
                    }
                }

                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (var item in Table.TableOrig.ListFilteredField(null, false))
                    {
                        item.IsUsed = true;

                        if (dlg1.clbList.CheckedItems.Contains(item.FieldNameReady))
                        {
                            item.IsUsed = false;
                        }
                    }
                }
                dlg1.Dispose();
            }

            if ((cbScriptCreateType.SelectedIndex == 0) || (cbScriptCreateType.SelectedIndex == 1))
            {
                if (System.Windows.Forms.MessageBox.Show("Все поля учитывать при генерации скрипта?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                {
                    // выберем поля для включения в скрипт
                    FormCheckedListBox dlg1 = new FormCheckedListBox();
                    dlg1.clbList.Items.Clear();

                    foreach (var item in Table.TableOrig.ListFilteredField(null, false))
                    {
                        if (!dlg1.clbList.Items.Contains(item.FieldNameReady))
                        {
                            dlg1.clbList.Items.Add(item.FieldNameReady, true);
                        }
                    }

                    foreach (var item in Table.TableEdit.ListFilteredField(null, false))
                    {
                        if (!dlg1.clbList.Items.Contains(item.FieldNameReady))
                        {
                            dlg1.clbList.Items.Add(item.FieldNameReady, true);
                        }
                    }

                    if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        foreach (var item in Table.TableOrig.ListFilteredField(null, false))
                        {
                            item.IsUsed = false;

                            if (dlg1.clbList.CheckedItems.Contains(item.FieldNameReady))
                            {
                                item.IsUsed = true;
                            }
                        }

                        foreach (var item in Table.TableEdit.ListFilteredField(null, false))
                        {
                            item.IsUsed = false;

                            if (dlg1.clbList.CheckedItems.Contains(item.FieldNameReady))
                            {
                                item.IsUsed = true;
                            }
                        }
                    }
                    dlg1.Dispose();
                }
            }

            tbScriptCreate.Text = "";
            tbScriptCreate.Filename = "";
            tbScriptCreateMessages.Text = "";
            dgScriptCreateResults.ItemsSource = null;
            lbScriptCreateStatus.Content = "";

            tbProcScriptMessages.Text = "";
            tbProcScript.Text = "";
            tbProcScript.Filename = "";
            dgProcScriptResults.ItemsSource = null;
            lbProcScriptStatus.Content = "";

            if ((cbScriptCreateType.SelectedIndex == 0) || (cbScriptCreateType.SelectedIndex == 1) || (cbScriptCreateType.SelectedIndex == 8))
            {
                // CREATE/ALTER
                try
                {
                    if (string.IsNullOrWhiteSpace(tbPKName.Text)) btAutoPKName_Click(sender, e);

                    List<string> ProcCommand = new List<string>();
                    int ProcCommandNum = -1;
                    string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);
                    Table.isOnlyExist = false;
                    tbScriptCreate.Text = TableDB.GenerateTableScript(ConnectSQL, true, isRegion.IsChecked == true, txtRegion, out ProcCommand, out ProcCommandNum, true, out string RowInfo, Table);

                    if (
                        (ProcCommand != null) &&
                        (ProcCommandNum >= 0)
                        )
                    {
                        tbProcCommand.ItemsSource = ProcCommand;
                        tbProcCommand.SelectedIndex = ProcCommandNum;
                    }

                    tiScriptCreate.IsSelected = true;
                    ScriptLocalType = ScriptLocalType.NONE;

                    // добавление описаний в файл задача.log
                    try
                    {
                        string Info = "";

                        if (Table.ScriptType == ScriptType.CREATE)
                        {
                            Info += Environment.NewLine + "Создание таблицы " + Table.TableEdit.FullTableName + " - " + Table.TableEdit.TableDesc + ":";
                            Info += Environment.NewLine;

                            foreach (var item in Table.TableEdit.ListFilteredField(null, false))
                            {
                                Info += Environment.NewLine + $"{item.FieldName} {item.FullFieldTypeToScript} - {item.FieldDesc}";
                                if (!string.IsNullOrWhiteSpace(item.FKTable)) Info += " (" + item.FKTable + ")";
                            }

                        }

                        if (Table.ScriptType == ScriptType.ALTER)
                        {
                            Info += Environment.NewLine + "Изменение таблицы " + Table.TableEdit.FullTableName + " - " + Table.TableEdit.TableDesc + ":";
                            Info += Environment.NewLine;
                            if (!string.IsNullOrWhiteSpace(RowInfo))
                            {
                                Info += RowInfo;
                            }
                        }

                        Info = Info + Environment.NewLine + "----------------------------------------------------------------------------------------";
                        if (!string.IsNullOrWhiteSpace(MainWindow.Task.TaskNumber))
                        {
                            File.AppendAllText(MainWindow.Task.LogFileTable, Info);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Cursor = Cursors.Arrow;
                        App.AddLog("", ex, App.ShowMessageMode.SHOW, false, null);
                    }
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            if (cbScriptCreateType.SelectedIndex == 2)
            {
                // stg.LocalDBList
                try
                {
                    FormLocalDBList dlg1 = new FormLocalDBList();

                    if (Table.LocalDBList == null)
                    {
                        Table.LocalDBList = new LocalDBList();

                        Table.LocalDBList.LocalDBList_Name = Table.TableEdit.LocalDBListName;
                        Table.LocalDBList.LocalDBList_Prefix = Table.TableEdit.TableNameReady;
                        Table.LocalDBList.LocalDBList_Schema = Table.TableEdit.SchemaNameReady;
                        Table.LocalDBList.LocalDBList_Nick = Table.TableEdit.TableNameReady;
                        Table.LocalDBList.LocalDBList_Key = TableInfo.PKListFields(false, Table.TableEdit);
                        Table.LocalDBList.LocalDBList_Descr = Table.TableEdit.TableDesc;
                        Table.LocalDBList.LocalDBList_id = "0";
                        Table.LocalDBList.LocalDBList_Module = "promed";
                        Table.LocalDBList.RegionalLocalDBList_sql = "";
                        Table.LocalDBList.RegionalLocalDBList_pgsql = "";
                        if (isRegion.IsChecked == true)
                        {
                            Table.LocalDBList.Region_id = Utilities.Controls.GetSelectedRegion(cbRegion);
                        }
                        else
                        {
                            Table.LocalDBList.Region_id = "";
                        }
                    }

                    dlg1.tbName.Text = Table.LocalDBList.LocalDBList_Name;
                    dlg1.tbPrefix.Text = Table.LocalDBList.LocalDBList_Prefix;
                    dlg1.tbSchema.Text = Table.LocalDBList.LocalDBList_Schema;
                    dlg1.tbNick.Text = Table.LocalDBList.LocalDBList_Nick;
                    dlg1.tbKey.Text = Table.LocalDBList.LocalDBList_Key;
                    dlg1.tbDescr.Text = Table.LocalDBList.LocalDBList_Descr;
                    dlg1.tbModule.Text = Table.LocalDBList.LocalDBList_Module;
                    dlg1.tbRegion.Text = Table.LocalDBList.Region_id;
                    dlg1.tbMSSQL.Text = (Table.LocalDBList.RegionalLocalDBList_sql ?? "")
                        .Replace("\r\n", "\n")
                        .Replace("\n", "\r\n"); ;
                    dlg1.tbPGSQL.Text = (Table.LocalDBList.RegionalLocalDBList_pgsql ?? "")
                        .Replace("\r\n", "\n")
                        .Replace("\n", "\r\n"); ;

                    dlg1.Connect = ConnectSQL;
                    dlg1.ConnectPG = MainWindow.GetConnectByGITProject("dev_promed_pg", "", true, true);
                    dlg1.ConnectPromedadygea = MainWindow.GetConnectByGITProject("dev_promed_pg", "promedadygea", true);
                    dlg1.FindClick();

                    if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Table.LocalDBList.LocalDBList_Name = dlg1.tbName.Text;
                        Table.LocalDBList.LocalDBList_Prefix = dlg1.tbPrefix.Text;
                        Table.LocalDBList.LocalDBList_Schema = dlg1.tbSchema.Text;
                        Table.LocalDBList.LocalDBList_Nick = dlg1.tbNick.Text;
                        Table.LocalDBList.LocalDBList_Key = dlg1.tbKey.Text;
                        Table.LocalDBList.LocalDBList_Descr = dlg1.tbDescr.Text;
                        Table.LocalDBList.LocalDBList_Module = dlg1.tbModule.Text;
                        Table.LocalDBList.Region_id = dlg1.tbRegion.Text;
                        Table.LocalDBList.RegionalLocalDBList_sql = dlg1.tbMSSQL.Text;
                        Table.LocalDBList.RegionalLocalDBList_pgsql = dlg1.tbPGSQL.Text;

                        Table.TableEdit.LocalDBListName = Table.LocalDBList.LocalDBList_Name;
                        tbLocalDBListName.Text = Table.LocalDBList.LocalDBList_Name;

                        this.Cursor = Cursors.Wait;
                        tbScriptCreate.Text = TableDB.GenerateLocalDBList(Table);
                        tbProcCommand.Text = "";
                        tiScriptCreate.IsSelected = true;
                        ScriptLocalType = ScriptLocalType.LocalDBList;
                    }
                    dlg1.Dispose();
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            if (cbScriptCreateType.SelectedIndex == 3)
            {
                // nsi.RefTableRegistry
                try
                {
                    FormRefTableRegistry dlg1 = new FormRefTableRegistry();

                    if (Table.RefTableRegistry == null)
                    {
                        Table.RefTableRegistry = new RefTableRegistry();

                        Table.RefTableRegistry.CreateDate = DateTime.Now;
                        Table.RefTableRegistry.PublishDate = DateTime.Now;
                        Table.RefTableRegistry.RefTableRegistry_id = 0;
                        Table.RefTableRegistry.RefTableRegistryVersion_id = 0;
                        Table.RefTableRegistry.Version = "";
                        long RefTableRegistry_maxid = 0;
                        long RefTableRegistryVersion_maxid = 0;

                        try
                        {
                            this.Cursor = Cursors.Wait;
                            DataTable info = ConnectSQL.GetRefTableRegistry(Table.TableEdit.FullTableNameToSeek);
                            if (info != null) //-V3022
                            {
                                foreach (DataRow row in info.Rows)
                                {
                                    foreach (DataColumn column in info.Columns)
                                    {
                                        if (column.ColumnName.ToLower() == "RefTableRegistry_Oid".ToLower())
                                        {
                                            Table.RefTableRegistry.OID = row[column].ToString();
                                        }
                                        if (column.ColumnName.ToLower() == "RefTableRegistry_FullName".ToLower())
                                        {
                                            Table.RefTableRegistry.FullName = row[column].ToString();
                                        }
                                        if (column.ColumnName.ToLower() == "RefTableRegistry_Nick".ToLower())
                                        {
                                            Table.RefTableRegistry.ShortName = row[column].ToString();
                                        }
                                        if (column.ColumnName.ToLower() == "RefTableRegistry_createDT".ToLower())
                                        {
                                            DateTime d;
                                            if (row.IsNull(column)) d = DateTime.Now;
                                            else
                                            {
                                                try
                                                {
                                                    d = (DateTime)row[column];
                                                }
                                                catch
                                                {
                                                    d = DateTime.Now;
                                                }
                                            }

                                            Table.RefTableRegistry.CreateDate = d;
                                        }
                                        if (column.ColumnName.ToLower() == "RefTableRegistry_id".ToLower())
                                        {
                                            if (row.IsNull(column)) Table.RefTableRegistry.RefTableRegistry_id = 0;
                                            else
                                            {
                                                Table.RefTableRegistry.RefTableRegistry_id = (long)row[column];
                                            }
                                        }
                                        if (column.ColumnName.ToLower() == "RefTableRegistry_maxid".ToLower())
                                        {
                                            if (row.IsNull(column)) RefTableRegistry_maxid = 0;
                                            else
                                            {
                                                RefTableRegistry_maxid = (long)row[column];
                                            }
                                        }
                                        if (column.ColumnName.ToLower() == "RefTableRegistryVersion_maxid".ToLower())
                                        {
                                            if (row.IsNull(column)) RefTableRegistryVersion_maxid = 0;
                                            else
                                            {
                                                RefTableRegistryVersion_maxid = (long)row[column];
                                            }
                                        }
                                    }
                                    break; //-V3020
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            this.Cursor = Cursors.Arrow;
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        }
                        this.Cursor = Cursors.Arrow;

                        if (Table.RefTableRegistry.RefTableRegistry_id == 0) Table.RefTableRegistry.RefTableRegistry_id = RefTableRegistry_maxid + 1;
                        if (Table.RefTableRegistry.RefTableRegistryVersion_id == 0) Table.RefTableRegistry.RefTableRegistryVersion_id = RefTableRegistryVersion_maxid + 1;
                    }

                    dlg1.tbTableName.Text = Table.TableEdit.FullTableName;
                    dlg1.tbOID.Text = Table.RefTableRegistry.OID;
                    dlg1.tbFullName.Text = Table.RefTableRegistry.FullName;
                    dlg1.tbShortName.Text = Table.RefTableRegistry.ShortName;
                    dlg1.tbCreateDate.Value = Table.RefTableRegistry.CreateDate;
                    dlg1.tbPublishDate.Value = Table.RefTableRegistry.PublishDate;
                    dlg1.tbRefTableRegistry_id.Text = Table.RefTableRegistry.RefTableRegistry_id.ToString();
                    dlg1.tbRefTableRegistryVersion_id.Text = Table.RefTableRegistry.RefTableRegistryVersion_id.ToString();
                    dlg1.tbVersion.Text = Table.RefTableRegistry.Version;

                    if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        Table.RefTableRegistry.OID = dlg1.tbOID.Text;
                        Table.RefTableRegistry.FullName = dlg1.tbFullName.Text;
                        Table.RefTableRegistry.ShortName = dlg1.tbShortName.Text;
                        Table.RefTableRegistry.CreateDate = dlg1.tbCreateDate.Value;
                        Table.RefTableRegistry.PublishDate = dlg1.tbPublishDate.Value;

                        long i = 0;

                        if (long.TryParse(dlg1.tbRefTableRegistry_id.Text, out i))
                            Table.RefTableRegistry.RefTableRegistry_id = i;
                        else
                            Table.RefTableRegistry.RefTableRegistry_id = 0;

                        if (long.TryParse(dlg1.tbRefTableRegistryVersion_id.Text, out i))
                            Table.RefTableRegistry.RefTableRegistryVersion_id = i;
                        else
                            Table.RefTableRegistry.RefTableRegistryVersion_id = 0;

                        Table.RefTableRegistry.Version = dlg1.tbVersion.Text;

                        this.Cursor = Cursors.Wait;
                        tbScriptCreate.Text = Table.GenerateRefTableRegistry();
                        tbProcCommand.Text = "";
                        tiScriptCreate.IsSelected = true;
                        ScriptLocalType = ScriptLocalType.RefTableRegistry;
                    }
                    dlg1.Dispose();
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            if (cbScriptCreateType.SelectedIndex == 4)
            {
                try
                {
                    // sequence
                    this.Cursor = Cursors.Wait;

                    string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);
                    tbScriptCreate.Text = TableDB.GenerateSequence(isRegion.IsChecked == true, txtRegion, Table.TableEdit);
                    tbProcCommand.Text = "";
                    tiScriptCreate.IsSelected = true;
                    ScriptLocalType = ScriptLocalType.Sequence;
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            if (cbScriptCreateType.SelectedIndex == 5)
            {
                try
                {
                    // шаблон procedure
                    this.Cursor = Cursors.Wait;

                    string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);
                    tbScriptCreate.Text = TableDB.GenerateShablonProc(isRegion.IsChecked == true, txtRegion, Table);
                    tbProcCommand.Text = "";
                    tiScriptCreate.IsSelected = true;
                    ScriptLocalType = ScriptLocalType.ShablonProc;
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            if (cbScriptCreateType.SelectedIndex == 6)
            {
                try
                {
                    // шаблон function
                    this.Cursor = Cursors.Wait;

                    string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);
                    tbScriptCreate.Text = TableDB.GenerateShablonFunc(isRegion.IsChecked == true, txtRegion, Table);
                    tbProcCommand.Text = "";
                    tiScriptCreate.IsSelected = true;
                    ScriptLocalType = ScriptLocalType.ShablonFunc;
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            if (cbScriptCreateType.SelectedIndex == 7)
            {
                try
                {
                    // шаблон view
                    this.Cursor = Cursors.Wait;

                    string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);
                    tbScriptCreate.Text = TableDB.GenerateShablonView(isRegion.IsChecked == true, txtRegion, Table);
                    tbProcCommand.Text = "";
                    tiScriptCreate.IsSelected = true;
                    ScriptLocalType = ScriptLocalType.ShablonView;
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            this.Cursor = Cursors.Arrow;
        }

        /// <summary>Выбран тип таблицы</summary>
        private void cbTableType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbParentEvnTable != null)
            {
                lbParentEvnTable.Visibility = Visibility.Hidden;
                cbParentEvnTable.Visibility = Visibility.Hidden;
            }
            switch (cbTableType.SelectedIndex)
            {
                case 0:
                    Table.TableType =  Utilities.TableType.DICT;
                    break;
                case 1:
                    Table.TableType =  Utilities.TableType.EVN;

                    // Обновить список из EvnClass
                    if (cbParentEvnTable != null)
                    {
                        string oldvalue = Table.TableEdit.ParentEvnTable;

                        if (CheckConnectSQL())
                        {
                            cbParentEvnTable.Items.Clear();
                            var ListEvn = ConnectSQL.GetListEvnChilds("", true, false);
                            foreach (var item in ListEvn)
                            {
                                cbParentEvnTable.Items.Add(item);
                            }
                        }

                        lbParentEvnTable.Visibility = Visibility.Visible;
                        cbParentEvnTable.Visibility = Visibility.Visible;
                        cbParentEvnTable.SelectedItem = oldvalue;
                    }

                    break;
                case 2:
                    Table.TableType =  Utilities.TableType.PERSONEVN;
                    break;
                case 3:
                    Table.TableType =  Utilities.TableType.MORBUS;
                    break;
                default:
                    Table.TableType =  Utilities.TableType.DICT;
                    break;
            }

        }

        /// <summary>Выбран Тип скрипта</summary>
        private void cbScriptCreateType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (cbScriptCreateType.SelectedIndex)
            {
                case 1:
                case 8:
                    Table.ScriptType = Utilities.ScriptType.ALTER;
                    break;
                case 0:
                default:
                    Table.ScriptType = Utilities.ScriptType.CREATE;
                    break;
            }

        }

        /// <summary>Изменилось значение в поле Script (при условии, что разрешено его редактирование) на вкладке Script</summary>
        private void tbScriptCreate_TextChanged(object sender, EventArgs e)
        {
            Table.SQLScript = tbScriptCreate.Text;
        }


        /// <summary>Изменилось значение поля Локальный справочник</summary>
        private void tbLocalDBListName_TextChanged(object sender, TextChangedEventArgs e)
        {
            string name = tbLocalDBListName.Text.Replace("[", "").Replace("]", "").Replace(".", "_");

            //if (name == null) name = "";
            tbLocalDBListName.Text = name;

            if (Table.TableEdit.LocalDBListName != name)
            {
                Table.TableEdit.LocalDBListName = name;
                Table.LocalDBList = null;
            }

        }

        /// <summary>Нажата кнопка "Выгрузить в Excel"</summary>
        private void btExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Utilities.MSOffice.GenerateExcel(
                    Utilities.Databases.ConvertToDataTable(
                        Table.TableEdit.ListFilteredField(null, false)
                    ), 
                    false, 
                    "", 
                    true
                );
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
        }

        /// <summary>Выбран регион</summary>
        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRegion.SelectedIndex != -1) isRegion.IsChecked = true;
        }

        /// <summary>Выбрано подключение на вкладке Script</summary>
        private void cbConnect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isStart)
            {
                string connname = "";
                if ((cbConnect != null) && (cbConnect.SelectedItem != null))
                {
                    var cbItem = (ComboBoxItem)cbConnect.SelectedItem;
                    connname = cbItem.Content.ToString();
                }

                Utilities.Controls.SetComboBoxConnectByName(cbConnectProcScript, connname);
            }
        }

        /// <summary>Нажата кнопка "Выполнить скрипт" на вкладке Script</summary>
        private void btExecScript_Click(object sender, RoutedEventArgs e)
        {
            if (btExecScript.IsEnabled)
            {
                if (cbConnect.SelectedIndex == -1)
                {
                    MessageBox.Show("Для выполнения скрипта необходимо выбрать подключение к БД !");
                    cbConnect.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(tbScriptCreate.Text))
                {
                    MessageBox.Show("Скрипт пустой !");
                    tbScriptCreate.Focus();
                    return;
                }

                tbScriptCreateMessages.Text = "";
                dgScriptCreateResults.ItemsSource = null;
                lbScriptCreateStatus.Content = "";

                if (!string.IsNullOrWhiteSpace(tbScriptCreate.SelectedText))
                {
                    Utilities.External.ExecScriptInThread(Utilities.External.ExecType.QUERY, this, cbConnect, tbScriptCreate.SelectedText, btExecScript, tiScriptCreateResults, dgScriptCreateResults, tiScriptCreateMessages, tbScriptCreateMessages, lbScriptCreateStatus);
                }
                else
                {
                    Utilities.External.ExecScriptInThread(Utilities.External.ExecType.DEFAULT, this, cbConnect, tbScriptCreate.Text, btExecScript, null, null, tiScriptCreateMessages, tbScriptCreateMessages, lbScriptCreateStatus);
                }
            }
        }

        /// <summary>Изменилось значение в поле "Команда для генерации хранимок:" на вкладке Script</summary>
        private void tbProcCommand_TextChanged(object sender, TextChangedEventArgs e)
        {
            Table.SQLProcCommand = tbProcCommand.Text;
        }

        /// <summary>Нажата кнопка "Вызвать генератор хранимок" на вкладке Script</summary>
        private void btExecProc_Click(object sender, RoutedEventArgs e)
        {

            if (btExecProc.IsEnabled)
            {
                if (cbConnect.SelectedIndex == -1)
                {
                    MessageBox.Show("Для выполнения скрипта необходимо выбрать подключение к БД !");
                    cbConnect.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(Table.SQLProcCommand))
                {
                    MessageBox.Show("Пустая команда для генерации хранимок !");
                    tbProcCommand.Focus();
                    return;
                }

                tbProcScript.Text = "";
                tbProcScript.Filename = "";
                tbProcScriptMessages.Text = "";
                dgProcScriptResults.ItemsSource = null;
                lbProcScriptStatus.Content = "";

                if (!string.IsNullOrWhiteSpace(tbProcCommand.Text))
                {
                    Utilities.External.ExecScriptInThread(Utilities.External.ExecType.GENERATOR, this, cbConnect, tbProcCommand.Text, btExecProc, null, null, tiProcScript, tbProcScript, lbProcScriptStatus);
                }
            }
        }

        /// <summary>Нажата кнопка Сохранить в файл на вкладке Script - Хранимки</summary>
        private void btSaveProcScript_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Task.TaskNumber == "")
            {
                MessageBox.Show("Необходимо заполнить Номер задачи в основном окне программы!");
                return;
            }

            string filename = "";

            try
            {
                // имя временного файла для скрипта
                var generator = new RandomGenerator();
                filename = System.IO.Path.Combine(MainWindow.Task.TaskPath, generator.RandomString(8) + ".tmp");

                // создаем и заполняем временный файл
                Encoding encoding = new UTF8Encoding(false);
                File.WriteAllText(filename, tbProcScript.Text, encoding);

                // определить номер скрипта 
                string ScriptNumber = (Utilities.Files.MaxScriptNumber(MainWindow.Task.TaskPath) + 1).ToString();

                // разбираем на хранимки временный файл
                Utilities.Databases.SaveProcScript(Table.PrefixToFilename, MainWindow.Task.TaskNumber, filename, ScriptNumber, cbConnectProcScript);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }

            try
            {
                if (
                    (!string.IsNullOrWhiteSpace(filename)) &&
                    File.Exists(filename)
                )
                {
                    File.Delete(filename);
                }
            }
            catch
            {
            }
        }

        /// <summary>Изменился текст на вкладке Script - Хранимки</summary>
        private void tbProcScript_TextChanged(object sender, EventArgs e)
        {
            Table.SQLProcScript = tbProcScript.Text;
        }

        /// <summary>Нажата кнопка В буфер обмена на вкладке Script - Хранимки</summary>
        private void btClipboardProcScript_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbProcScript.Text);
        }

        /// <summary>Нажата кнопка Выполнить скрипт на вкладке Script - Хранимки</summary>
        private void btExecProcScript_Click(object sender, RoutedEventArgs e)
        {
            if (btExecProcScript.IsEnabled)
            {
                if (cbConnectProcScript.SelectedIndex == -1)
                {
                    MessageBox.Show("Для выполнения скрипта необходимо выбрать подключение к БД !");
                    cbConnectProcScript.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(tbProcScript.Text))
                {
                    MessageBox.Show("Скрипт пустой !");
                    tbProcScript.Focus();
                    return;
                }

                tbProcScriptMessages.Text = "";
                dgProcScriptResults.ItemsSource = null;
                lbProcScriptStatus.Content = "";

                if (!string.IsNullOrWhiteSpace(tbProcScript.SelectedText))
                {
                    Utilities.External.ExecScriptInThread(Utilities.External.ExecType.QUERY, this, cbConnectProcScript, tbProcScript.SelectedText, btExecProcScript, tiProcScriptResults, dgProcScriptResults, tiProcScriptMessages, tbProcScriptMessages, lbProcScriptStatus);
                }
                else
                {
                    Utilities.External.ExecScriptInThread(Utilities.External.ExecType.DEFAULT, this, cbConnectProcScript, tbProcScript.Text, btExecProcScript, null, null, tiProcScriptMessages, tbProcScriptMessages, lbProcScriptStatus);
                }
            }
        }

        private void tbTableName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if ((dgFields.Items.Count == 0) ||
                    (System.Windows.Forms.MessageBox.Show("Список полей заполнен. Очистить?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    )
                {
                    btFillFromDB.Focus();
                    btFillFromDB_Click(sender, e);
                }
                e.Handled = true;
            }
        }

        /// <summary>Добавление индекса (из контекстного меню)</summary>
        private void AddIndex_Click(object sender, RoutedEventArgs e)
        {
            if (tbTableName.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableName.Focus();
                return;
            }

            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            FormAddIndex dlg1 = new FormAddIndex();
            dlg1.Text = "Новый индекс";
            dlg1.parent = this.Table;
            dlg1.tbIndexName.Text = "idx_" + Table.TableEdit.TableNameReady + "_";
            dlg1.cbIsProd.Checked = true;
            dlg1.cbScriptType.SelectedItem = "CREATE";

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string isuniq = "";
                string isprod = "";
                string isreg = "";
                string isreport = "";
                string isnullsnotdistinct = "";
                string paddisdeleted = "";
                string paddisregion = "";

                if (dlg1.cbIsUnique.Checked) isuniq = "true";
                if (dlg1.cbIsProd.Checked) isprod = "true";
                if (dlg1.cbIsReg.Checked) isreg = "true";
                if (dlg1.cbIsReport.Checked) isreport = "true";
                if (dlg1.cbIsNullsNotDistinct.Checked) isnullsnotdistinct = "true";

                if (dlg1.rbDeleted_NULL.Checked) paddisdeleted = null;
                if (dlg1.rbDeleted_1.Checked) paddisdeleted = "false";
                if (dlg1.rbDeleted_2.Checked) paddisdeleted = "true";

                if (dlg1.rbRegion_NULL.Checked) paddisregion = null;
                if (dlg1.rbRegion_1.Checked) paddisregion = "false";
                if (dlg1.rbRegion_2.Checked) paddisregion = "true";

                var index = Table.AddIndex(dlg1.tbIndexName.Text, isuniq, dlg1.tbIndexPredicat.Text, dlg1.tbIndexInclude.Text, dlg1.tbIndexWhere.Text, isnullsnotdistinct, dlg1.tbIndexToDel.Text, isprod, isreg, isreport, paddisdeleted, paddisregion);
                dgIndexesRefresh();

                tbScriptCreate.Text = "";
                tbScriptCreate.Filename = "";
                try
                {
                    this.Cursor = Cursors.Wait;

                    // генерация скрипта
                    string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);
                    tbScriptCreate.Text = TableDB.GenerateIndexScript(dlg1.ScriptType, true, false, isRegion.IsChecked == true, txtRegion, index);
                    tbProcCommand.Text = "";
                    tiScriptCreate.IsSelected = true;
                    ScriptLocalType = ScriptLocalType.Index;
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    tbScriptCreate.Text = "";
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

                this.Cursor = Cursors.Arrow;
            }
            dlg1.Dispose();
        }


        /// <summary>Добавление индекса (по выбранному полю)</summary>
        private void AddIndexByField_Click(object sender, RoutedEventArgs e)
        {
            if (tbTableName.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableName.Focus();
                return;
            }

            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            if (dgFields.SelectedIndex >= 0)
            {
                FieldDB field = dgFields.SelectedItem as FieldDB;

                FormAddIndex dlg1 = new FormAddIndex();
                dlg1.Text = "Новый индекс";
                dlg1.parent = this.Table;
                dlg1.tbIndexPredicat.Text = field.FieldNameToScript;
                dlg1.btAutoName_Click(null, null);

                dlg1.cbIsProd.Checked = true;
                dlg1.cbScriptType.SelectedItem = "CREATE";

                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string isuniq = "";
                    string isprod = "";
                    string isreg = "";
                    string isreport = "";
                    string isnullsnotdistinct = "";
                    string paddisdeleted = "";
                    string paddisregion = "";

                    if (dlg1.cbIsUnique.Checked) isuniq = "true";
                    if (dlg1.cbIsProd.Checked) isprod = "true";
                    if (dlg1.cbIsReg.Checked) isreg = "true";
                    if (dlg1.cbIsReport.Checked) isreport = "true";
                    if (dlg1.cbIsNullsNotDistinct.Checked) isnullsnotdistinct = "true";

                    if (dlg1.rbDeleted_NULL.Checked) paddisdeleted = null;
                    if (dlg1.rbDeleted_1.Checked) paddisdeleted = "false";
                    if (dlg1.rbDeleted_2.Checked) paddisdeleted = "true";

                    if (dlg1.rbRegion_NULL.Checked) paddisregion = null;
                    if (dlg1.rbRegion_1.Checked) paddisregion = "false";
                    if (dlg1.rbRegion_2.Checked) paddisregion = "true";

                    var index = Table.AddIndex(dlg1.tbIndexName.Text, isuniq, dlg1.tbIndexPredicat.Text, dlg1.tbIndexInclude.Text, dlg1.tbIndexWhere.Text, isnullsnotdistinct, dlg1.tbIndexToDel.Text, isprod, isreg, isreport, paddisdeleted, paddisregion);
                    dgIndexesRefresh();

                    tbScriptCreate.Text = "";
                    tbScriptCreate.Filename = "";
                    try
                    {
                        this.Cursor = Cursors.Wait;

                        // генерация скрипта
                        string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);
                        tbScriptCreate.Text = TableDB.GenerateIndexScript(dlg1.ScriptType, true, false, isRegion.IsChecked == true, txtRegion, index);
                        tbProcCommand.Text = "";
                        tiScriptCreate.IsSelected = true;
                        ScriptLocalType = ScriptLocalType.Index;
                    }
                    catch (Exception ex)
                    {
                        this.Cursor = Cursors.Arrow;
                        tbScriptCreate.Text = "";
                        App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }

                    this.Cursor = Cursors.Arrow;
                }
                dlg1.Dispose();
            }
        }

        /// <summary>Изменение индекса (из контекстного меню)</summary>
        private void EditIndex_Click(object sender, RoutedEventArgs e)
        {
            if (tbTableName.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableName.Focus();
                return;
            }

            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            if (dgIndexes.SelectedIndex >= 0)
            {
                IndexDB index = dgIndexes.SelectedItem as IndexDB;

                FormAddIndex dlg1 = new FormAddIndex();
                dlg1.Text = "Изменить индекс " + index.FullIndexName;
                dlg1.parent = this.Table;
                dlg1.index = index;
                dlg1.OriginalName = index.IndexName;
                dlg1.cbScriptType.SelectedItem = "ALTER";

                dlg1.tbIndexName.Text = index.IndexName;
                dlg1.cbIsUnique.Checked = index.IsUnique;
                dlg1.tbIndexPredicat.Text = index.Predicat;
                dlg1.tbIndexInclude.Text = index.Include;
                dlg1.tbIndexWhere.Text = index.Where;
                dlg1.cbIsNullsNotDistinct.Checked = index.IsNullsNotDistinct;
                dlg1.tbIndexToDel.Text = index.IndexToDel;
                dlg1.cbIsProd.Checked = index.IsProd;
                dlg1.cbIsReg.Checked = index.IsReg;
                dlg1.cbIsReport.Checked = index.IsReport;

                if (index.pAddisDeleted == null) dlg1.rbDeleted_NULL.Checked = true;
                else if (index.pAddisDeleted == false) dlg1.rbDeleted_1.Checked = true;
                else if (index.pAddisDeleted == true) dlg1.rbDeleted_2.Checked = true;

                if (index.pAddisRegion == null) dlg1.rbRegion_NULL.Checked = true;
                else if (index.pAddisRegion == false) dlg1.rbRegion_1.Checked = true;
                else if (index.pAddisRegion == true) dlg1.rbRegion_2.Checked = true;

                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    index.IndexName = dlg1.tbIndexName.Text;
                    index.IsUnique = dlg1.cbIsUnique.Checked;
                    index.Predicat = dlg1.tbIndexPredicat.Text;
                    index.Include = dlg1.tbIndexInclude.Text;
                    index.Where = dlg1.tbIndexWhere.Text;
                    index.IsNullsNotDistinct = dlg1.cbIsNullsNotDistinct.Checked;
                    index.IndexToDel = dlg1.tbIndexToDel.Text;
                    index.IsProd = dlg1.cbIsProd.Checked;
                    index.IsReg = dlg1.cbIsReg.Checked;
                    index.IsReport = dlg1.cbIsReport.Checked;

                    if (dlg1.rbDeleted_NULL.Checked) index.pAddisDeleted = null;
                    else if (dlg1.rbDeleted_1.Checked) index.pAddisDeleted = false;
                    else if (dlg1.rbDeleted_2.Checked) index.pAddisDeleted = true;

                    if (dlg1.rbRegion_NULL.Checked) index.pAddisRegion = null;
                    else if (dlg1.rbRegion_1.Checked) index.pAddisRegion = false;
                    else if (dlg1.rbRegion_2.Checked) index.pAddisRegion = true;

                    Utilities.ScriptType ScriptType = dlg1.ScriptType;
                    dlg1.Dispose();
                    tiStructure.IsSelected = true;
                    dgIndexesRefresh();

                    tbScriptCreate.Text = "";
                    tbScriptCreate.Filename = "";
                    try
                    {
                        this.Cursor = Cursors.Wait;

                        // генерация скрипта
                        string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);
                        tbScriptCreate.Text = TableDB.GenerateIndexScript(ScriptType, true, false, isRegion.IsChecked == true, txtRegion, index);
                        tbProcCommand.Text = "";
                        tiScriptCreate.IsSelected = true;
                        ScriptLocalType = ScriptLocalType.Index;
                    }
                    catch (Exception ex)
                    {
                        this.Cursor = Cursors.Arrow;
                        tbScriptCreate.Text = "";
                        App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }

                    this.Cursor = Cursors.Arrow;
                }
                else dlg1.Dispose();
            }
        }

        /// <summary>Генерация скрипта на Создание индекса</summary>
        private void CreateIndex(bool isDDL = false)
        {
            if (tbTableName.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableName.Focus();
                return;
            }

            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            if (dgIndexes.SelectedIndex >= 0)
            {
                IndexDB index = dgIndexes.SelectedItem as IndexDB;

                tbScriptCreate.Text = "";
                tbScriptCreate.Filename = "";
                try
                {
                    this.Cursor = Cursors.Wait;

                    // генерация скрипта
                    string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);
                    tbScriptCreate.Text = TableDB.GenerateIndexScript(Utilities.ScriptType.CREATE, true, isDDL, isRegion.IsChecked == true, txtRegion, index);
                    tbProcCommand.Text = "";
                    tiScriptCreate.IsSelected = true;
                    ScriptLocalType = ScriptLocalType.Index;
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    tbScriptCreate.Text = "";
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

                this.Cursor = Cursors.Arrow;
            }
        }


        /// <summary>Генерация скрипта на Создание индекса через p_IndexCreate (из контекстного меню)</summary>
        private void CreateIndex_Click(object sender, RoutedEventArgs e)
        {
            CreateIndex(false);
        }

        /// <summary>Генерация скрипта на Создание индекса через CREATE INDEX (из контекстного меню)</summary>
        private void CreateIndexDDL_Click(object sender, RoutedEventArgs e)
        {
            CreateIndex(true);
        }

        /// <summary>Генерация скрипта на Удаление индекса</summary>
        private void DropIndex(bool isDDL = false)
        {
            if (tbTableName.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableName.Focus();
                return;
            }

            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            if (dgIndexes.SelectedIndex >= 0)
            {
                IndexDB index = dgIndexes.SelectedItem as IndexDB;

                tbScriptCreate.Text = "";
                tbScriptCreate.Filename = "";
                try
                {
                    this.Cursor = Cursors.Wait;

                    // генерация скрипта
                    string txtRegion = Utilities.Controls.GetSelectedRegion(cbRegion);
                    tbScriptCreate.Text = TableDB.GenerateIndexScript(Utilities.ScriptType.DROP, true, isDDL, isRegion.IsChecked == true, txtRegion, index);
                    tbProcCommand.Text = "";
                    tiScriptCreate.IsSelected = true;
                    ScriptLocalType = ScriptLocalType.Index;
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    tbScriptCreate.Text = "";
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

                this.Cursor = Cursors.Arrow;
            }
        }


        /// <summary>Генерация скрипта на Удаление индекса через p_IndexDelete (из контекстного меню)</summary>
        private void DropIndex_Click(object sender, RoutedEventArgs e)
        {
            DropIndex(false);
        }

        /// <summary>Генерация скрипта на Удаление индекса через DROP INDEX (из контекстного меню)</summary>
        private void DropIndexDDL_Click(object sender, RoutedEventArgs e)
        {
            DropIndex(true);
        }

        private void dgIndexes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            EditIndex_Click(sender, e);
        }

        /// <summary>
        /// Изменилось подключение к БД
        /// </summary>
        public void cbConnectSQLChanged()
        {
            if (ConnectSQL != null)
            {
                ConnectSQL.CloseConnect();
            }

            ConnectSQL = Utilities.Controls.SetConnectFromComboBox(cbConnectSQL);

            if (ConnectSQL != null) //-V3022
            {
                Utilities.Controls.RefreshGITProjectItems(cbGITProject, ConnectSQL.GITProject);
                cbGITProjectChanged();
            }
        }

        /// <summary>
        /// Выбор подключения на вкладке Таблица
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void cbConnectSQL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isStart) cbConnectSQLChanged();
        }

        /// <summary>Изменился проект GIT</summary>
        private void cbGITProjectChanged()
        {
            ComboBoxItem cbItem = null;
            if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
            if ((cbItem == null) || (cbItem.Tag == null)) Table.GITProject = "";
            else Table.GITProject = cbItem.Tag.ToString();

            string connname = "";
            if ((cbConnectSQL != null) && (cbConnectSQL.SelectedItem != null))
            {
                cbItem = (ComboBoxItem)cbConnectSQL.SelectedItem;
                connname = cbItem.Content.ToString();
            }

            Utilities.Controls.RefreshConnectItems(cbConnectProcScript, connname, null, Table.ConnType);
            Utilities.Controls.SetComboBoxConnectByDefaultProject(cbConnectProcScript, Table.GITProject);

            Utilities.Controls.RefreshConnectItems(cbConnect, connname, null, Table.ConnType);
            Utilities.Controls.SetComboBoxConnectByDefaultProject(cbConnect, Table.GITProject);
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

        /// <summary>Выбран проект GIT</summary>
        private void cbGITProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbGITProjectChanged();
        }

        /// <summary>Нажата кнопка "Добавить подключение" на вкладке Script</summary>
        private void btAddConnect_Click(object sender, RoutedEventArgs e)
        {
            var connect = Utilities.Controls.OpenConnectFromComboBox(cbConnect, true);

            if (connect.ConnType != Utilities.ConnType.None)
            {
                Utilities.Controls.RefreshConnectItems(cbConnect, connect.DBConnectionName, null, null);
            }

            connect.CloseConnect();
        }

        /// <summary>Нажата кнопка "Добавить подключение" на вкладке Script - Хранимки</summary>
        private void btAddConnectProcScript_Click(object sender, RoutedEventArgs e)
        {
            var connect = Utilities.Controls.OpenConnectFromComboBox(cbConnectProcScript, true);

            if (connect.ConnType != Utilities.ConnType.None)
            {
                Utilities.Controls.RefreshConnectItems(cbConnectProcScript, connect.DBConnectionName, null, null);
            }

            connect.CloseConnect();
        }

        /// <summary>Нажата кнопка "Заполнить из Excel" на вкладке Script - Хранимки</summary>
        private void btFillFromExcel_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                return;
            }

            btClearFields_Click(sender, e);

            // ищем excel
            string xlsfile = "";

            if (! string.IsNullOrWhiteSpace(MainWindow.Task.TaskPath))
            {
                xlsfile = Controls.Dialogs.OpenExcelDialog(MainWindow.Task.TaskPath);
            }

            if (!string.IsNullOrWhiteSpace(xlsfile))
            {
                // считывем Excel 
                this.Cursor = Cursors.Wait;
                Excel.Application excelApp = null;
                Excel.Workbook excelWorkBook = null;
                try
                {
                    excelApp = new Excel.Application();
                    excelApp.Visible = false;
                    excelWorkBook = excelApp.Workbooks.Open(xlsfile, null, true);
                    Excel.Worksheet excelWorkSheet = excelWorkBook.Sheets[1]; // с первой страницы книги

                    // B1 - имя таблицы
                    string name = excelWorkSheet.Cells[1, 2].Text;
                    tbSchemaName.Text = Utilities.Databases.GetSchemaName(name);
                    tbTableName.Text = Utilities.Databases.GetTableName(name);
                    TableNameChanged();
                    btAutoPKName_Click(sender, e);
                    // B2 - описание таблицы
                    tbTableDesc.Text = excelWorkSheet.Cells[2, 2].Text;

                    Table.TableOrig.TableName = tbTableName.Text;
                    Table.TableOrig.SchemaName = tbSchemaName.Text;

                    dgFields.ItemsSource = Table.TableEdit.ListField;

                    // Начиная с 4-й строки - поля таблицы
                    int maxPass = 10; // максимальное кол-во пустых строк для завершения считывания
                    int cntRow = 3;
                    bool isDelID = false;

                    do
                    {
                        cntRow++;

                        string _fieldname = excelWorkSheet.Cells[cntRow, 1].Text;
                        _fieldname = _fieldname.Trim();
                        string _fielddesc = excelWorkSheet.Cells[cntRow, 2].Text;
                        _fielddesc = _fielddesc.Trim();
                        string _fieldtype = excelWorkSheet.Cells[cntRow, 3].Text;
                        _fieldtype = _fieldtype.Trim();
                        string _fieldsize = excelWorkSheet.Cells[cntRow, 4].Text;
                        _fieldsize = _fieldsize.Trim();
                        string _fielddec = excelWorkSheet.Cells[cntRow, 5].Text;
                        _fielddec = _fielddec.Trim();
                        string _fieldnotnull = excelWorkSheet.Cells[cntRow, 6].Text;
                        if (!string.IsNullOrWhiteSpace(_fieldnotnull)) _fieldnotnull = "true";
                        else _fieldnotnull = "false";
                        string _fieldidentity = excelWorkSheet.Cells[cntRow, 7].Text;
                        if (!string.IsNullOrWhiteSpace(_fieldidentity)) _fieldidentity = "true";
                        else _fieldidentity = "false";
                        string _fieldpk = excelWorkSheet.Cells[cntRow, 8].Text;
                        if (!string.IsNullOrWhiteSpace(_fieldpk)) _fieldpk = "true";
                        else _fieldpk = "false";
                        string _fktable = excelWorkSheet.Cells[cntRow, 9].Text;
                        _fktable = _fktable.Trim();
                        string _fkname = "";
                        string _fkfield = "";
                        string _fieldcheck = excelWorkSheet.Cells[cntRow, 10].Text;

                        if (_fieldname.ToLower().Contains(Table.TableEdit.TableNameCompare + "_is"))
                        {
                            _fieldtype = "BIGINT";
                            _fktable = "dbo.YesNo";
                        }

                        if (!string.IsNullOrWhiteSpace(_fktable))
                        {
                            _fktable = Utilities.Databases.GetFullTableName(_fktable);

                            if (string.IsNullOrWhiteSpace(_fielddesc))
                            {
                                _fielddesc = "";
                                try
                                {
                                    _fielddesc = ConnectSQL.GetTableDecription(_fktable);
                                }
                                catch (Exception ex)
                                {
                                    _fielddesc = "";
                                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                }
                            }

                            if (string.IsNullOrWhiteSpace(_fkname)) //-V3022
                            {
                                _fkname = Table.TableEdit.GetFKNameDefault(_fieldname);
                            }

                            if (string.IsNullOrWhiteSpace(_fkfield)) //-V3022
                            {
                                try
                                {
                                    string _pktype = "";

                                    try
                                    {
                                         var pkinfo = ConnectSQL.GetTablePK(_fktable);

                                        _fkfield = pkinfo.FirstFieldName;
                                        _pktype = pkinfo.FirstFieldType;
                                    }
                                    catch (Exception ex)
                                    {
                                        _fkfield = "";
                                        _pktype = "";
                                        App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                    }

                                    if (string.IsNullOrWhiteSpace(_fkfield))
                                    {
                                        _fkfield = Utilities.Databases.GetTableName(_fktable) + "_id";
                                    }

                                    if (!string.IsNullOrWhiteSpace(_pktype))
                                    {
                                        _fieldtype = _pktype;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _fkfield = "";
                                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                }

                                if (string.IsNullOrWhiteSpace(_fkfield))
                                {
                                    _fkfield = Utilities.Databases.GetTableName(_fktable) + "_id";
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(_fieldname))
                        {
                            if (_fieldname.ToLower().Contains("_deleted"))
                            {
                                isDelID = true;
                            }

                            if (_fieldname.ToLower() == Table.TableEdit.TableNameCompare + "_id")
                            {
                                Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_id", "BIGINT", "", "", "Уникальный идентификатор", "true", "true", "true");
                            }
                            else if (_fieldname.ToLower() == "region_id")
                            {
                                AddFieldRegion_Click(sender, e);
                            }
                            else
                            {
                                Table.TableEdit.AddField("", _fieldname, _fieldtype, _fieldsize, _fielddec, _fielddesc, _fieldnotnull, _fieldidentity, _fieldpk, "", "", _fkname, _fktable, _fkfield, "", "", _fieldcheck);
                            }
                        }
                        else
                        {
                            maxPass--;
                        }
                    } while (maxPass > 0);

                    if (isDelID)
                    {
                        AddFieldDelID_Click(sender, e);
                    }

                    AddFieldInsUpdID_Click(sender, e);

                    if (!string.IsNullOrWhiteSpace(Table.TableEdit.TableName))
                    {
                        //Добавить в историю
                        tbTableName.AddHistory(Table.TableEdit.FullTableName);
                    }
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

                if (excelWorkBook != null) excelWorkBook.Close();
                if (excelApp != null) excelApp.Quit();

                cbScriptCreateType.SelectedIndex = 0;
                cbTableType.SelectedIndex = 0;
                if (Table.TableEdit.ListField.Count() > 0)
                {
                    tbTableName.IsReadOnly = true;
                    tbSchemaName.IsReadOnly = true;
                    cbParentEvnTable.IsReadOnly = true;
                    //tbTableName.IsEnabled = tbTableName.IsReadOnly == false; //-V3022
                    //tbSchemaName.IsEnabled = tbSchemaName.IsReadOnly == false; //-V3022
                    cbParentEvnTable.IsEnabled = cbParentEvnTable.IsReadOnly == false; //-V3022
                }
                dgFieldsRefresh();
                this.Cursor = Cursors.Arrow;
            }
        }

        private void ProcListRefresh()
        {
            ListCollectionView cvTasks = (ListCollectionView)CollectionViewSource.GetDefaultView(dgProcListGrid.ItemsSource);

            if (cvTasks != null && cvTasks.CanFilter == true)
            {
                cvTasks.Filter = new Predicate<object>(ProcListFilter);
            }

            dgProcListGrid.Items.Refresh();
        }

        /// <summary>Обновить список хранимок</summary>
        private void dgProcListGridRefresh()
        {
            this.Cursor = Cursors.Wait;

            string filter_schema = tbProcSchemaFilter.Text.Trim();
            if (string.IsNullOrWhiteSpace(filter_schema))
            {
                filter_schema = "%";
                tbProcSchemaFilter.Text = "%";
            }
            string filter_table = tbProcNameFilter.Text.Trim();

            //bool isFilteredAll = string.IsNullOrWhiteSpace(filter_table);

            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                var list = ConnectSQL.GetObjectList(filter_schema + "." + filter_table,false, false, false, true, true, true, true, true, false);
                foreach (var item in list)
                {
                    string scripttype = item.Type;
                    bool isIndexCreate = false;
                    string error = "";
                    string for_tablename = "";
                    string schema = item.Schema;
                    string objectname = item.Name;
                    string schemaseek = "";
                    string objectseek = "";
                    item.Text = Utilities.Databases.GenerateProcText(true, ConnectSQL, ref schema, ref objectname, ref schemaseek, ref objectseek, false, "", ref scripttype, ref for_tablename, isIndexCreate, out error, false, Table.GITProject, false, false);
                }
                dgProcListGrid.ItemsSource = list;
                ProcListRefresh();
                this.Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Фильтрация хранимок
        /// </summary>
        /// <param name="de">объект</param>
        /// <returns></returns>
        public bool ProcListFilter(object de)
        {
            DBObjectInfo procInfo = de as DBObjectInfo;
            return string.IsNullOrWhiteSpace(procInfo.isAutogen) || (cbWithAutogen.IsChecked == true);
        }

        /// <summary>Нажата кнопка Фильтр на вкладке Список хранимок</summary>
        private void btProcListFilter_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbProcNameFilter.Text))
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbProcNameFilter.Focus();
                return;
            }

            dgProcListGridRefresh();
        }

        /// <summary>Нажата клавиша Enter в поле tbProcNameFilter на вкладке Список хранимок</summary>
        private void tbProcNameFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btProcListFilter.Focus();
                btProcListFilter_Click(sender, e);
                e.Handled = true;
            }
        }

        /// <summary>Нажата клавиша Enter в поле tbProcSchemaFilter на вкладке Список хранимок</summary>
        private void tbProcSchemaFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tbProcNameFilter.Focus();
                e.Handled = true;
            }
        }

        /// <summary>Выбрана таблица из истории</summary>
        private void tbTableName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbTableName.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbTableName.SelectedItem;

                tbTableName.Text = (string)cbItem.Tag;
            }
        }

        /// <summary>
        /// Вышел из поля Имя таблицы
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void tbTableName_LostFocus(object sender, RoutedEventArgs e)
        {
            TableNameChanged();
        }

        /// <summary>Закрылся список истории имен таблиц</summary>
        private void tbTableName_DropDownClosed(object sender, EventArgs e)
        {
            TableNameChanged(true);
        }

        private void ViewProc()
        {
            if (dgProcListGrid.SelectedIndex >= 0)
            {
                DBObjectInfo procinfo = dgProcListGrid.SelectedItem as DBObjectInfo;
                if (!string.IsNullOrWhiteSpace(procinfo.Text))
                {
                    WinInfo WinInfo = new WinInfo(null);
                    WinInfo.Title = procinfo.FullName;
                    WinInfo.tbInfo.Text = procinfo.Text;
                    WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL");
                    WinInfo.Show();
                }
            }
        }


        /// <summary>Просмотр оригинального текста хранимки</summary>
        private void btViewOriginal_Click(object sender, RoutedEventArgs e)
        {
            ViewProc();
        }

        private void dgProcListGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewProc();
        }

        private void cbWithAutogen_Checked(object sender, RoutedEventArgs e)
        {
            ProcListRefresh();
        }

        private void cbWithAutogen_Unchecked(object sender, RoutedEventArgs e)
        {
            ProcListRefresh();
        }

        private void cbParentEvnTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbParentEvnTable.SelectedIndex != -1)
            {
                Table.TableEdit.ParentEvnTable = cbParentEvnTable.SelectedItem as string;
            }
        }

        /// <summary>
        /// Открыть таблицу Foreign Key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenForeignKeyTable_Click(object sender, RoutedEventArgs e)
        {
            if (tbTableName.Text.Trim() == "")
            {
                MessageBox.Show("Необходимо заполнить Имя таблицы !");
                tbTableName.Focus();
                return;
            }

            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            if (dgFields.SelectedIndex >= 0)
            {
                FieldDB field = dgFields.SelectedItem as FieldDB;

                if (!string.IsNullOrWhiteSpace(field.FKTable))
                {
                    WinTable WinTable = new WinTable();

                    if (cbConnectSQL != null)
                    {
                        Utilities.Controls.RefreshConnectItems(WinTable.cbConnectSQL, ConnectSQL.DBConnectionName, null, null);
                    }

                    Utilities.Controls.RefreshRegionItems(WinTable.cbRegion, Table.GITProject);

                    WinTable.isStart = false;
                    WinTable.cbConnectSQLChanged();

                    WinTable.Show();

                    WinTable.tbTableName.Text = field.FKFullTableNameReady;
                    WinTable.tbTableName_LostFocus(null, null);
                    WinTable.btFillFromDB_Click(null, null);
                }
            }
        }

        /// <summary>
        /// Обработка нажатий клавиш на вкладке Таблица
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tiStructure_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                btFillFromDB_Click(null, null);
                e.Handled = true;
            }

            if (e.Key == Key.F5)
            {
                btGenerateCreate_Click(null, null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработка нажатий клавиш на вкладке Script
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tiScriptCreate_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                btExecScript_Click(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.F6)
            {
                btExecProc_Click(null, null);
                e.Handled = true;
            }

            if (e.Key == Key.F8)
            {
                btSaveCreate_Click(null, null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработка нажатий клавиш на вкладке Script - Хранимки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tiProcScript_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                btExecProcScript_Click(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.F8)
            {
                btSaveProcScript_Click(null, null);
                e.Handled = true;
            }
        }

        /// <summary>
        /// при выходе из поля "Команда для генерации хранимок"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbProcCommand_LostFocus(object sender, RoutedEventArgs e)
        {
            Table.SQLProcCommand = tbProcCommand.Text;
        }

        private void btQuery_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbTableName.Text))
            {
                MainWindow.NewQuery(ConnectSQL, $"select * from {tbSchemaName.Text}.{tbTableName.Text}");
            }
        }

        /// <summary>Добавить insDTTZ, updDTTZ</summary>
        private void AddFieldInsUpdDTTZ_Click(object sender, RoutedEventArgs e)
        {
            dgFields.Focus();
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_insDTTZ", "TIMESTAMPTZ", "", "", "Дата добавления записи (включая часовой пояс)", "false");
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_updDTTZ", "TIMESTAMPTZ", "", "", "Дата обновления записи (включая часовой пояс)", "false");
            dgFieldsRefresh();
        }

        /// <summary>Добавить delDTTZ</summary>
        private void AddFieldDelDTTZ_Click(object sender, RoutedEventArgs e)
        {
            dgFields.Focus();
            Table.TableEdit.AddField("", Table.TableEdit.TableNameReady + "_delDTTZ", "TIMESTAMPTZ", "", "", "Дата удаления записи (включая часовой пояс)", "false");
            dgFieldsRefresh();
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        // для запрета редактирования унаследованных полей
        private void dgFields_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            string fieldname = e.Column.SortMemberPath;
            FieldDB Row = (FieldDB)e.Row.Item;

            if (
                (Row != null) &&
                Row.IsInherit && 
                (
                    (fieldname == "FieldName") ||
                    (fieldname == "FieldType") ||
                    (fieldname == "FieldSize") ||
                    (fieldname == "FieldDec") ||
                    (fieldname == "IsNotNull") ||
                    (fieldname == "IsIdentity") 
                )
            )
            {
                e.Cancel = true;
            }
        }
    }
}