// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Path = System.IO.Path;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Тип выгрузки
    /// </summary>
    public enum UploadType
    {
        /// <summary>
        /// скрипт для задачи
        /// </summary>
        TASK,
        /// <summary>
        /// скрипт для "старого" проекта GIT
        /// </summary>
        GIT,
        /// <summary>
        /// скрипт для "нового" проекта GIT
        /// </summary>
        DEV
    }

    /// <summary>
    /// Окно выгрузки процедур и представлений из БД
    /// </summary>
    public partial class WinUploadFromBD : Window
    {

        /// <summary>Список результатов поиска</summary>
        public List<DBObjectInfo> ListResults = new List<DBObjectInfo>();

        /// <summary>
        /// основное соединение формы
        /// </summary>
        ConnectDB ConnectSQL = null;

        // номер предыдущей задачи
        private string LastTaskNumber = "";

        /// <summary>
        /// фоновый воркер
        /// </summary>
        private BackgroundWorker backgroundWorker1;
        private int highestPercentageReached = 0;

        /// <summary>
        /// признак автовыгрузки
        /// </summary>
        private bool isAutoUpload = false;

        /// <summary>Конструктор WinUploadFromBD</summary>
        public WinUploadFromBD()
        {
            InitializeComponent();

            checkAddRegion.IsChecked = true;
            addTables.IsChecked = false;
            addForeignTables.IsChecked = false;
            addViews.IsChecked = true;
            addProcs.IsChecked = true;
            addProcsNotSQL.IsChecked = false;
            addTriggers.IsChecked = false;
            addSequences.IsChecked = false;
            addIndexes.IsChecked = false;
            checkNewdb.IsChecked = false;
            checkIndexCreate.IsChecked = false;

            // инициализация фонового воркера
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(UploadScripts);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);


            tbFolder.Text = MainWindow.Task.TaskPath;
            tbFilter.InitHistory("HistoryUploadFromBD.json", "");

            // пользовательские настройки GUI
            Default.InitGUI("WinUploadFromBD", this, mainGrid, null, null, null, MainWindow.Task.LogFile);
        }

        /// <summary>При открытии окна WinUploadFromBD</summary>
        private void winUploadFromBD_Activated(object sender, EventArgs e)
        {
            this.Title = "Выгрузить объекты БД - " + tbFilter.Text + ", задача " + MainWindow.Task.TaskNumber;

            if (!string.IsNullOrWhiteSpace(MainWindow.Task.TaskNumber))
            {
                if (
                    (!string.IsNullOrWhiteSpace(LastTaskNumber)) &&
                    tbFolder.Text.Contains(LastTaskNumber)
                   )
                {
                    tbFolder.Text = tbFolder.Text.Replace(LastTaskNumber, MainWindow.Task.TaskNumber);
                }
                LastTaskNumber = MainWindow.Task.TaskNumber;
            }

            tbFilter.Focus();
        }

        /// <summary>При закрытии окна WinUploadFromBD</summary>
        private void winUploadFromBD_Closed(object sender, EventArgs e)
        {
            if (ConnectSQL != null)
            {
                ConnectSQL.CloseConnect();
            }

            // пользовательские настройки GUI
            Default.SaveGUI("WinUploadFromBD", this, null);
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

        /// <summary>Нажата клавиша в поле Поиск</summary>
        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                tbFilter_LostFocus(sender, e);
                btFind_Click(sender, e);
                dgResults.Focus();
                e.Handled = true;
            }

        }

        /// <summary>При выходе из поля Поиск</summary>
        private void tbFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            tbFilter.Text = tbFilter.Text.Trim().Replace("[", "").Replace("]", "").Replace("\"", "").Replace(" ", ".").Replace("\t", ".").Trim();
            this.Title = "Выгрузить объекты БД - " + tbFilter.Text + ", задача " + MainWindow.Task.TaskNumber;
            btFind.Focus();
        }

        /// <summary>
        /// Заполнить список объектов БД
        /// </summary>
        /// <param name="name">строка для поиска</param>
        public void FillResult(string name)
        {
            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                App.AddLog("Подключение " + cbConnectSQL.Text + " не открыто !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                return;
            }

            try
            {
                this.Cursor = Cursors.Wait;
                ListResults = ConnectSQL.GetObjectList(
                    name,
                    false,
                    addTables.IsChecked == true,
                    addForeignTables.IsChecked == true,
                    addViews.IsChecked == true,
                    addProcs.IsChecked == true,
                    addProcsNotSQL.IsChecked == true,
                    addTriggers.IsChecked == true,
                    addSequences.IsChecked == true,
                    addIndexes.IsChecked == true
                    );
                dgResults.ItemsSource = ListResults;
                dgResults.Items.Refresh();
                this.Cursor = Cursors.Arrow;

            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
        }

        /// <summary>
        /// Нажата кнопка Найти
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void btFind_Click(object sender, RoutedEventArgs e)
        {
            // заполнить результат поиска 
            FillResult(tbFilter.Text.Trim());
            btAdd.Focus();

            // Добавить в историю
            tbFilter.AddHistory(tbFilter.Text.Trim());
        }

        /// <summary>Добавление найденного объекта в список через контекстного меню</summary>
        private void btAdd_Click(object sender, RoutedEventArgs e)
        {
            if (dgResults.SelectedIndex >= 0)
            {
                DBObjectInfo value = dgResults.SelectedItem as DBObjectInfo;

                // добавить результат в список
                tbObjects.Text = tbObjects.Text.Trim() + Environment.NewLine + value.FullName;

                // удалить из результатов поиска
                ListResults.Remove(value);
                dgResults.Items.Refresh();
            }
        }

        /// <summary>Двойной клик мыши на результатах поиска</summary>
        private void dgResults_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            btAdd_Click(sender, e);
        }

        /// <summary>
        /// Нажата кнопка Добавить ВСЕ
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void btAddAll_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder(100000);
            sb.Append(tbObjects.Text.Trim());

            foreach (var item in ListResults)
            {
                sb.Append(Environment.NewLine + item.FullName);
            }
            tbObjects.Text = sb.ToString().Trim();

            // удалить из результатов поиска
            ListResults.Clear();
            dgResults.Items.Refresh();
        }

        /// <summary>Нажата кнопка Очистить</summary>
        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            tbObjects.Text = "";
        }


        /// <summary>Выгружаем скрипты с использованием фонового воркера</summary>
        private void UploadScripts(object sender, DoWorkEventArgs e)
        {
            if (e.Argument == null) return;

            BackgroundWorker worker = sender as BackgroundWorker;

            UploadParam param = (UploadParam)e.Argument;

            bool isError = false;
            bool isFirst = true;
            string error = "";
            string path = param.Folder;
            int maximum = param.allObjects.Count();
            int current = 0;
            //bool isBreak = true;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            worker.ReportProgress(0);

            foreach (var item in param.allObjects)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                current++;
                string scripttype = "";
                string objectname = "";
                string schema = "";
                string filename = "";
                string for_tablename = "";

                objectname = item.Trim();
                // имя и схема объекта
                objectname = objectname.Replace("[", "").Replace("]", "");
                schema = Utilities.Databases.GetSchemaName(objectname);
                objectname = Utilities.Databases.GetTableName(objectname);
                string schemaseek = "";
                string objectseek = "";


                if (!string.IsNullOrWhiteSpace(objectname))
                {
                    FileStream fs = null;
                    string full = "";

                    string txtRegion = "";
                    if (Utilities.Databases.regex_region.IsMatch(schema)) txtRegion = schema.Substring(1);
                    bool isAddRegion = param.AddRegion_IsChecked && (!string.IsNullOrWhiteSpace(txtRegion));

                    try
                    {
                        string err = "";
                        string text = Utilities.Databases.GenerateProcText(false, ConnectSQL, ref schema, ref objectname, ref schemaseek, ref objectseek, isAddRegion, txtRegion, ref scripttype, ref for_tablename, param.isIndexCreate, out err, param.isNewdb, param.project, false, true);
                        
                        //isSQLFormat.IsChecked = param.isFormat;

                        if (!string.IsNullOrWhiteSpace(err))
                        {
                            if (!string.IsNullOrWhiteSpace(error)) error += Environment.NewLine;
                            error += schema + "." + objectname + " :" + Environment.NewLine;
                            error += err + Environment.NewLine;

                            isError = true;
                        }

                        if (!string.IsNullOrWhiteSpace(scripttype))
                        {

                            fs = null;
                            FileMode fileMode = FileMode.Create;

                            full = "";

                            if (param.UploadType == UploadType.DEV)
                            {
                                // выгружаем структуру для DEV-проекта

                                string maindir = Path.Combine(path, param.project);
                                if (!Directory.Exists(maindir))
                                {
                                    Directory.CreateDirectory(maindir);
                                }

                                string datadir = Path.Combine(maindir, "data");
                                if (!Directory.Exists(datadir))
                                {
                                    Directory.CreateDirectory(datadir);

                                    // создать пустой .gitkeep
                                    string gitkeep = Path.Combine(datadir, ".gitkeep");
                                    File.WriteAllText(gitkeep, String.Empty);
                                }

                                string dir = Path.Combine(maindir, schemaseek);
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);

                                    // создать скрипт для схемы
                                    string fileschema = Path.Combine(dir, schemaseek + ".sql");

                                    string title = MainWindow.Task.TitleScript(ConnectSQL.TargetDBType, "SCHEMA", param.isNewdb, false);

                                    string textschema = "";

                                    switch (ConnectSQL.TargetDBType)
                                    {
                                        case Utilities.TargetDBType.MSSQL:
                                            textschema = Environment.NewLine +
                                                "IF SCHEMA_ID('" + schema + "') IS NULL" + Environment.NewLine +
                                                "\tEXEC ('CREATE SCHEMA " + schema + "')";
                                            break;
                                        case Utilities.TargetDBType.PGSQL:
                                        case Utilities.TargetDBType.EMD:
                                            textschema = Environment.NewLine +
                                                "CREATE SCHEMA IF NOT EXISTS " + schema + ";";
                                            break;
                                        default:
                                            break;
                                    }

                                    // собираем и сохраняем скрипт
                                    err = "";
                                    Utilities.Files.WriteScript(fileschema, null, title + textschema, false, out err, FileMode.Create);
                                }


                                if (scripttype == "index")
                                {
                                    if (for_tablename.ToLower().StartsWith("v_"))
                                    {
                                        dir = Path.Combine(dir, "VIEW");
                                    }
                                    else
                                    {
                                        dir = Path.Combine(dir, "TABLE");
                                    }

                                }
                                else if (scripttype == "materialized view")
                                {
                                    dir = Path.Combine(dir, "VIEW");
                                }
                                else
                                {
                                    dir = Path.Combine(dir, scripttype.ToUpper());
                                }
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }

                                /*
                               
                                if (scripttype == "index")
                                {
                                    dir = Path.Combine(dir, for_tablename);
                                }
                                else
                                {
                                    dir = Path.Combine(dir, objectseek);
                                }
                                
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }*/

                                // имя файла для скрипта
                                if (scripttype == "index")
                                {
                                    filename = for_tablename.ToLower() + ".sql";
                                }
                                else
                                {
                                    filename = objectseek.ToLower() + ".sql";
                                }

                                if (scripttype == "table")
                                {

                                    // создаем папку для таблицы в data 
                                    string tabledatadir = Path.Combine(datadir, schemaseek.ToLower() + "." + objectseek.ToLower());
                                    if (!Directory.Exists(tabledatadir))
                                    {
                                        Directory.CreateDirectory(tabledatadir);

                                        // создать пустой .gitkeep
                                        string gitkeep = Path.Combine(tabledatadir, ".gitkeep");
                                        File.WriteAllText(gitkeep, String.Empty);
                                    }
                                }

                                // создаем файл скрипта
                                full = Path.Combine(dir, filename);

                                fileMode = FileMode.Create;

                                if (scripttype == "index")
                                {
                                    // допишем индекс в существующий файл для таблицы
                                    fileMode = FileMode.Append;
                                }

                                if (File.Exists(full))
                                {
                                    // если файл уже существует - перегенерим текст и уберем --liquibase formatted sql и SELECT dbo.xp_dropfns
                                    err = "";
                                    text = Utilities.Databases.GenerateProcText(false, ConnectSQL, ref schema, ref objectname, ref schemaseek, ref objectseek, isAddRegion, txtRegion, ref scripttype, ref for_tablename, param.isIndexCreate, out err, param.isNewdb, param.project, true, true);

                                    fileMode = FileMode.Append;

                                    /*
                                                                        if (scripttype == "index")
                                                                        {
                                                                            // если при добавлении индекса файл уже существует - уберем --liquibase formatted sql
                                                                            text = text.Replace("--liquibase formatted sql", "");
                                                                        }
                                                                        else 
                                                                        {
                                                                            if ( isBreak &&
                                                                                (System.Windows.Forms.MessageBox.Show("Файл " + full + " уже существует!" + Environment.NewLine + "Прервать дальнейшую выгрузку ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                                                                )
                                                                            {
                                                                                error += "Выгрузка прервана!" + Environment.NewLine;
                                                                                isError = true;
                                                                                break;
                                                                            }

                                                                            isBreak = false;
                                    */
                                    /*string existed_file = Directory.GetFiles(dir).ToList().Where(x => x.ToLower() == full.ToLower()).FirstOrDefault();

                                    if (
                                        (existed_file != null) &&
                                        (Path.GetFileName(existed_file) != filename)
                                        )
                                    {*/

                                    // имя совпадает - это другая хранимка, изменим имя файла
                                    /*                                       int i = 0;

                                                                           do
                                                                           {
                                                                               i++;
                                                                               full = Path.Combine(dir, Path.GetFileNameWithoutExtension(filename).ToLower() + "_" + i.ToString() + Path.GetExtension(filename).ToLower());

                                                                           } while (File.Exists(full));

                                                                           error += "Создан файл-дубликат " + full + Environment.NewLine;
                                                                           isError = true;
                                                                       }*/
                                }

                                fs = new FileStream(full, fileMode);
                                isFirst = false;
                            }
                            else if (param.UploadType == UploadType.GIT)
                            {
                                // выгружаем структуру для GIT-проекта

                                // папка проекта
                                string maindir = Path.Combine(path, param.project);
                                if (!Directory.Exists(maindir))
                                {
                                    Directory.CreateDirectory(maindir);
                                }

                                // папка данных
                                string datadir = Path.Combine(maindir, "data");
                                if (param.project == "liquibase_project_new") datadir = Path.Combine(maindir, "data_new");

                                if (!Directory.Exists(datadir))
                                {
                                    Directory.CreateDirectory(datadir);

                                    // создать пустой .gitkeep
                                    string gitkeep = Path.Combine(datadir, ".gitkeep");
                                    File.WriteAllText(gitkeep, String.Empty);
                                }

                                // схема
                                string dir = Path.Combine(maindir, schemaseek);
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);

                                    // создать скрипт для схемы
                                    string fileschema = Path.Combine(dir, schemaseek + ".sql");

                                    string title = MainWindow.Task.TitleScript(ConnectSQL.TargetDBType, "SCHEMA", param.isNewdb, false);

                                    string textschema = "";

                                    switch (ConnectSQL.TargetDBType)
                                    {
                                        case Utilities.TargetDBType.MSSQL:
                                            textschema = Environment.NewLine +
                                                "IF SCHEMA_ID('" + schema + "') IS NULL" + Environment.NewLine +
                                                "\tEXEC ('CREATE SCHEMA " + schema + "')";
                                            break;
                                        case Utilities.TargetDBType.PGSQL:
                                        case Utilities.TargetDBType.EMD:
                                            textschema = Environment.NewLine +
                                                "CREATE SCHEMA IF NOT EXISTS " + schema + ";";
                                            break;
                                        default:
                                            break;
                                    }

                                    // собираем и сохраняем скрипт
                                    err = "";
                                    Utilities.Files.WriteScript(fileschema, null, title + textschema, false, out err, FileMode.Create);
                                }

                                // тип объекта
                                if (scripttype == "index")
                                {
                                    if (for_tablename.ToLower().StartsWith("v_"))
                                    {
                                        dir = Path.Combine(dir, "VIEW");
                                    }
                                    else
                                    {
                                        dir = Path.Combine(dir, "TABLE");
                                    }

                                }
                                else if (scripttype == "materialized view")
                                {
                                    dir = Path.Combine(dir, "VIEW");
                                }
                                else
                                {
                                    dir = Path.Combine(dir, scripttype.ToUpper());
                                }
                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }

                                // папка объекта
                                if (scripttype == "index")
                                {
                                    dir = Path.Combine(dir, for_tablename);
                                }
                                else
                                {
                                    dir = Path.Combine(dir, objectseek);
                                }

                                if (!Directory.Exists(dir))
                                {
                                    Directory.CreateDirectory(dir);
                                }

                                // имя файла для скрипта
                                if (scripttype == "index")
                                {
                                    filename = for_tablename.ToLower() + ".sql";
                                }
                                else
                                {
                                    filename = objectseek.ToLower() + ".sql";
                                }

                                if (scripttype == "table")
                                {

                                    // создаем папку для таблицы в data 
                                    string tabledatadir = Path.Combine(datadir, schemaseek.ToLower() + "." + objectseek.ToLower());
                                    if (!Directory.Exists(tabledatadir))
                                    {
                                        Directory.CreateDirectory(tabledatadir);

                                        // создать пустой .gitkeep
                                        string gitkeep = Path.Combine(tabledatadir, ".gitkeep");
                                        File.WriteAllText(gitkeep, String.Empty);
                                    }
                                }

                                // создаем файл скрипта
                                full = Path.Combine(dir, filename);

                                fileMode = FileMode.Create;

                                if (scripttype == "index")
                                {
                                    // допишем индекс в существующий файл для таблицы
                                    fileMode = FileMode.Append;
                                }

                                if (File.Exists(full))
                                {
                                    // если файл уже существует - перегенерим текст и уберем --liquibase formatted sql и SELECT dbo.xp_dropfns
                                    err = "";
                                    text = Utilities.Databases.GenerateProcText(false, ConnectSQL, ref schema, ref objectname, ref schemaseek, ref objectseek, isAddRegion, txtRegion, ref scripttype, ref for_tablename, param.isIndexCreate, out err, param.isNewdb, param.project, true, true);

                                    fileMode = FileMode.Append;
                                }

                                fs = new FileStream(full, fileMode);
                                isFirst = false;
                            }
                            else if (param.UploadType == UploadType.TASK)
                            {
                                // выгружаем скрипты для задачи

                                // определить номер скрипта 
                                string scriptnum = (Utilities.Files.MaxScriptNumber(path) + 1).ToString();

                                if (isFirst)
                                {
                                    // в первый раз спрашиваем номер скрипта
                                    FormAskNumFile dlg1 = new FormAskNumFile();
                                    dlg1.tbNumFile.Text = scriptnum;
                                    if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                        scriptnum = dlg1.tbNumFile.Text.Trim();
                                    dlg1.Dispose();
                                }

                                // имя файла для скрипта
                                filename = param.prefix + " " + param.task + " " + scriptnum + " " + scripttype + " " + schemaseek + " " + objectseek + ".sql";

                                if (isFirst)
                                {
                                    // в первый раз спрашиваем, куда сохранить
                                    full = Controls.Dialogs.SaveSQLDialog(path, filename, out fs, out fileMode);

                                    if (fs != null)
                                    {
                                        // все последующие файлы сохраняем в эту же папку
                                        path = Path.GetDirectoryName(full);
                                        isFirst = false;
                                    }
                                }
                                else
                                {
                                    full = Path.Combine(path, filename);
                                    fileMode = FileMode.Create;

                                    if (File.Exists(full))
                                    {
                                        if (System.Windows.Forms.MessageBox.Show("Файл " + full + " уже существует!" + Environment.NewLine + "Перезаписать ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                        {
                                            App.AddLog("Файл " + full + " уже существует, выбрано - Перезаписать", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                            fileMode = FileMode.Create;
                                        }
                                        else if (System.Windows.Forms.MessageBox.Show("Добавить в существующий файл ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                        {
                                            App.AddLog("Файл " + full + " уже существует, выбрано - Добавить в существующий файл", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                            fileMode = FileMode.Append;
                                        }
                                        else
                                        {
                                            error += "Выгрузка прервана!" + Environment.NewLine;
                                            isError = true;
                                            break;
                                        }
                                    }

                                    fs = new FileStream(full, fileMode);
                                }

                            }

                            //сохранить файл
                            if (fs != null)
                            {
                                if (!string.IsNullOrWhiteSpace(text))
                                {
                                    err = "";
                                    Utilities.Files.WriteScript(full, fs, text, false, out err, fileMode);

                                    if (!string.IsNullOrWhiteSpace(err))
                                    {
                                        error += schema + "." + objectname + " : " + err + Environment.NewLine;
                                        isError = true;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        error += App.AddLog(schema + "." + objectname + " : ", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile).showMessage + Environment.NewLine;

                        isError = true;
                    }
                    finally
                    {
                        if (fs != null) fs.Dispose();
                    }

                }

                int percentComplete =
                                    (int)((float)current / (float)maximum * 100);
                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete);
                }
            }


            if (!isAutoUpload)
            {
                if (isFirst)
                {
                    MessageBox.Show("Ничего не было выгружено!");
                }
                else
                {
                    MessageBox.Show("Выгрузка в " + path + " завершена!");
                }
            }

            if (isError)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        File.AppendAllText(MainWindow.Task.LogFile, Environment.NewLine +
                            DateTime.Now.ToString("G") + Environment.NewLine +
                            error +
                            "-------------------------------------------------------------------------------------------" + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("", ex, App.ShowMessageMode.SHOW, false, null);
                    }

                    WinInfo WinInfo = new WinInfo(null);
                    WinInfo.Title = "Список ошибок и предупреждений в процессе выгрузки - записан в " + MainWindow.Task.LogFile;
                    WinInfo.tbInfo.Text = error;
                    WinInfo.Show();
                });
            }

            if (isAutoUpload)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Application.Current.Shutdown();
                });
            }

        }

        private void UploadClick(UploadType UlpoadType)
        {
            if (MainWindow.Task == null) return;

            /*if (!Directory.Exists(tbFolder.Text.Trim()))
            {
                tbFolder.Focus();
                MessageBox.Show(tbFolder.Text + " не существует!");
                return;
            }*/

            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                App.AddLog("Подключение " + cbConnectSQL.Text + " не открыто !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                return;
            }

            if (backgroundWorker1.IsBusy == false)
            {
                // блокируем нажатие кнопок на время выполнения выгрузки
                this.btUpload.IsEnabled = false;
                this.btUploadGIT.IsEnabled = false;
                this.btUploadDEV.IsEnabled = false;
                this.btCancel.IsEnabled = true;

                // собираем параметры для фоновой задачи
                UploadParam param = new UploadParam();

                param.Folder = tbFolder.Text.Trim();
                param.AddRegion_IsChecked = checkAddRegion.IsChecked == true;

                ComboBoxItem cbItem = null;
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
                if ((cbItem == null) || (cbItem.Tag == null)) param.project = "";
                else param.project = cbItem.Tag.ToString();


                param.prefix = Utilities.GITProjects.GetPrefixFileSQLByProject(param.project);
                if (string.IsNullOrWhiteSpace(param.prefix))
                {
                    param.prefix = "ms";
                }

                param.task = MainWindow.Task.TaskNumber.ToLower().Replace("-", "");

                List<string> list = new List<string>();
                list.AddRange(tbObjects.Text.Trim().Replace("\r", "").Split('\n'));
                param.allObjects = list.Distinct().ToList();

                param.isNewdb = checkNewdb.IsChecked == true;
                param.isIndexCreate = checkIndexCreate.IsChecked == true;

                param.UploadType = UlpoadType;

                // запускаем фоновую задачу по выгрузке
                highestPercentageReached = 0;
                backgroundWorker1.RunWorkerAsync(param);
            }
        }


        /// <summary>
        /// Выгрузка на основании командной строки
        /// </summary>
        public void AutoUpload()
        {
            if (
                (App.Args.Length >= 5) &&
                (!string.IsNullOrWhiteSpace(MainWindow.Task.TaskNumber))
            )
            {

                Utilities.Controls.SetComboBoxConnectByName(cbConnectSQL, App.Args[2]);
                cbConnect_SelectionChanged(null, null);
                if (cbConnectSQL.SelectedIndex != -1)
                {
                    Utilities.Controls.SetComboBoxGITProjectByName(cbGITProject, App.Args[3]);
                    if (cbGITProject.SelectedIndex != -1) //-V3095
                    {
                        string project = "";
                        ComboBoxItem cbItem = null;
                        cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
                        if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                        else project = cbItem.Tag.ToString();

                        if (!string.IsNullOrWhiteSpace(project))
                        {

                            tbFolder.Text = App.Args[4];
                            if (Directory.Exists(tbFolder.Text))
                            {
                                // целевая папка
                                string dest = Path.Combine(tbFolder.Text, project);

                                try
                                {
                                    // очищаем целевую папку
                                    if (Directory.Exists(dest))
                                    {
                                        Directory.Delete(dest, true);
                                    }
                                    Directory.CreateDirectory(dest);
                                }
                                catch (Exception ex)
                                {
                                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, "");
                                }

                                // выгружаем в нее по новому
                                addTables.IsChecked = true;
                                addForeignTables.IsChecked = true;
                                addViews.IsChecked = true;
                                addProcs.IsChecked = true;
                                addTriggers.IsChecked = true;
                                checkNewdb.IsChecked = true;

                                btFind_Click(null, null);
                                //btAddAll_Click(null, null);

                                // получаем список объектов
                                StringBuilder sb = new StringBuilder(100000);

                                foreach (var item in ListResults)
                                {
                                    sb.Append($"{item.Schema}/{item.Type}/{item.Name.ToLower()}.sql" + Environment.NewLine);
                                }
                                tbObjects.Text = sb.ToString().Trim();

                                // сохраняем список в файл
                                string list = Path.Combine(dest, "list.txt");
                                try
                                {
                                    File.AppendAllText(list, tbObjects.Text);
                                }
                                catch (Exception ex)
                                {
                                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, "");
                                }

                                /*
                                // вызываем скрипт
                                string ProjectFolder = Utilities.GIT.GetFolderByProject(project);
                                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                                Utilities.ExecuteFile(
                                    App.AppPath,
                                    Path.Combine(App.AppPath, "git_???.cmd"),
                                    folder + " " + list,
                                    true,
                                    false,
                                    true,
                                    false
                                );

                                // считываем скрипт 
                                string result = Path.Combine(dest, "result.txt");
                                tbObjects.Text = File.ReadAllText(result);
                                */
                                isAutoUpload = true;

                                if (Utilities.GITProjects.IsGITProject(project))
                                {
                                    UploadClick(UploadType.GIT);
                                }

                                if (Utilities.GITProjects.IsDEVProject(project))
                                {
                                    UploadClick(UploadType.GIT);
                                }


                            }
                        }
                    }
                }
            }

        }

        /// <summary>Нажата кнопка Выгрузить для задачи</summary>
        private void btUpload_Click(object sender, RoutedEventArgs e)
        {
            UploadClick(UploadType.TASK);
        }

        /// <summary>
        /// Нажата кнопка Выгрузить для GIT
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void btUploadGIT_Click(object sender, RoutedEventArgs e)
        {
            if (cbGITProject != null)
            {
                string project = "";
                ComboBoxItem cbItem = null;
                cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
                if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                else project = cbItem.Tag.ToString();

                if (Utilities.GITProjects.IsDEVProject(project))
                {
                    project = Utilities.GITProjects.GetGITProject(project);
                    Utilities.Controls.SetComboBoxGITProjectByName(cbGITProject, project);
                }

                UploadClick(UploadType.GIT);
            }
        }

        /// <summary>Обновление progressbar</summary>
        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            pbStatus.Value = e.ProgressPercentage;
        }

        /// <summary>Фоновая задача завершена</summary>
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                App.AddLog(e.Error.Message, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }

            this.btUpload.IsEnabled = true;
            this.btUploadGIT.IsEnabled = true;
            this.btUploadDEV.IsEnabled = true;
            this.btCancel.IsEnabled = false;
        }

        /// <summary>Фоновая задача отменена</summary>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            // Cancel the asynchronous operation.
            if (backgroundWorker1 != null) backgroundWorker1.CancelAsync();

            this.btCancel.IsEnabled = false;
        }

        /// <summary>
        /// Смена подключения к БД
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void cbConnect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ConnectSQL != null)
            {
                ConnectSQL.CloseConnect();
            }

            ConnectSQL = Utilities.Controls.SetConnectFromComboBox(cbConnectSQL);

            if (ConnectSQL != null) //-V3022
            {
                Utilities.Controls.RefreshGITProjectItems(cbGITProject, ConnectSQL.GITProject, ConnectSQL.ConnType);
            }
        }

        /// <summary>Просмотр текста хранимки</summary>
        private void ViewProcText(bool isOriginal)
        {
            if (dgResults.SelectedIndex >= 0)
            {
                DBObjectInfo item = dgResults.SelectedItem as DBObjectInfo;

                string objectname = item.FullName;

                if (!CheckConnectSQL())
                {
                    cbConnectSQL.Focus();
                    App.AddLog("Подключение " + cbConnectSQL.Text + " не открыто !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    return;
                }

                this.Cursor = Cursors.Wait;

                string scripttype = "";

                // имя и схема объекта
                objectname = objectname.Replace("[", "").Replace("]", "");
                string schema = Utilities.Databases.GetSchemaName(objectname);
                objectname = Utilities.Databases.GetTableName(objectname);
                string schemaseek = "";
                string objectseek = "";

                if (!string.IsNullOrWhiteSpace(objectname))
                {
                    string txtRegion = "";
                    if (Utilities.Databases.regex_region.IsMatch(schema)) txtRegion = schema.Substring(1);
                    bool isAddRegion = (checkAddRegion.IsChecked == true) && (!string.IsNullOrWhiteSpace(txtRegion));

                    string project = "";
                    ComboBoxItem cbItem = null;
                    if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
                    if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                    else project = cbItem.Tag.ToString();

                    try
                    {
                        bool isIndexCreate = checkIndexCreate.IsChecked == true;
                        string error = "";
                        string for_tablename = "";

                        string text = Utilities.Databases.GenerateProcText(isOriginal, ConnectSQL, ref schema, ref objectname, ref schemaseek, ref objectseek, isAddRegion, txtRegion, ref scripttype, ref for_tablename, isIndexCreate, out error, checkNewdb.IsChecked == true, project, false, true);

                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            WinInfo WinInfo = new WinInfo(null);
                            WinInfo.Title = schema + "." + objectname;
                            WinInfo.tbInfo.Text = text;
                            WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL");
                            WinInfo.Show();
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Cursor = Cursors.Arrow;
                        App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                }
                this.Cursor = Cursors.Arrow;
            }

        }

        /// <summary>Просмотр оригинального текста хранимки</summary>
        private void btViewOriginal_Click(object sender, RoutedEventArgs e)
        {
            ViewProcText(true);
        }

        /// <summary>Просмотр текста хранимки</summary>
        private void btView_Click(object sender, RoutedEventArgs e)
        {
            ViewProcText(false);
        }

        private void tbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbFilter.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbFilter.SelectedItem;

                tbFilter.Text = (string)cbItem.Tag;
            }
        }

        private void btTFolder_Click(object sender, RoutedEventArgs e)
        {
            string dir = Controls.Dialogs.FolderBrowserDialog(tbFolder.Text);
            if (dir != "")
            {
                tbFolder.Text = dir;
            }

        }


        /// <summary>
        /// Нажата кнопка Выгрузить в DEV
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btUploadDEV_Click(object sender, RoutedEventArgs e)
        {
            if (cbGITProject != null)
            {
                string project = "";
                ComboBoxItem cbItem = null;
                cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
                if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                else project = cbItem.Tag.ToString();

                if (Utilities.GITProjects.IsGITProject(project))
                {
                    project = Utilities.GITProjects.GetDEVProject(project);
                    Utilities.Controls.SetComboBoxGITProjectByName(cbGITProject, project);
                }

                UploadClick(UploadType.DEV);
            }
        }
    }

    /// <summary>
    /// параметры для выгрузки
    /// </summary>
    public class UploadParam
    {
        /// <summary>
        /// выгружаем скрипты для GIT
        /// </summary>
        public UploadType UploadType;

        /// <summary>
        /// папка, куда выгружаем
        /// </summary>
        public string Folder;

        /// <summary>
        /// добавляем проверку региональности для объектов в схемах rXXX 
        /// </summary>
        public bool AddRegion_IsChecked;

        /// <summary>
        /// проект GIT
        /// </summary>
        public string project;

        /// <summary>
        /// префикс для sql-файлов проекта GIT
        /// </summary>
        public string prefix;

        /// <summary>
        /// номер задачи
        /// </summary>
        public string task;

        /// <summary>
        /// список объектов для выгрузки
        /// </summary>
        public List<string> allObjects;

        /// <summary>
        /// добавляем тег ликвибейз для первоначальной выгрузки БД в GIT
        /// </summary>
        public bool isNewdb;

        /// <summary>
        /// формируем индексы командой CREATE INDEX
        /// </summary>
        public bool isIndexCreate;
    }

}
