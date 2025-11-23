// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

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
using static SQLGen.App;

namespace SQLGen
{
    /// <summary>
    /// Выполнить sql-файлы и в окне отобразить вывод или ошибки
    /// </summary>
    public partial class WinSQLExecute : Window
    {
        /// <summary>
        /// подключение к БД для выполнения скрипта
        /// </summary>
        private ConnectDB ConnectSQL;

        /// <summary>
        /// подключение к БД для фиксации результата выполнения
        /// </summary>
        private ConnectDB ConnectLog;

        /// <summary>
        /// фоновый воркер
        /// </summary>
        private BackgroundWorker Worker;

        /// <summary>
        /// максимальный достигнутый прогресс в выполнении
        /// </summary>
        private int highestPercentageReached = 0;

        /// <summary>
        /// список sql-скриптов на последовательное исполнение
        /// </summary>
        public List<SQLFileInfo> ListSQL = new List<SQLFileInfo>();

        /// <summary>
        /// =true результат выполнения скрипта будет фиксироваться в таблице dbo.SQLGenDBLog и скрипт будет выполняться однократно
        /// </summary>
        private bool isExecuteOnce = false;

        /// <summary>
        /// Общее наименование пакета sql-скриптов для фиксации в dbo.SQLGenDBLog, например название yml-файла
        /// </summary>
        private string YMLName;

        /// <summary>
        ///  Полный путь к файлу для лога выполнения
        /// </summary>
        public string execLogFile = "";

        /// <summary>
        /// Результат выполнения внешних команд для записи в лог-файл
        /// </summary>
        private StringBuilder execLogText = new StringBuilder(100000);

        /// <summary>
        /// Вернуть лог 
        /// </summary>
        /// <returns></returns>
        public string GetLog()
        {
            string result = "";

            if (execLogText != null)
            {
                result = execLogText.ToString();
            }

            return result;
        }

        /// <summary>
        /// записать лог его в LogFile
        /// </summary>
        private void SaveLog()
        {
            if (!string.IsNullOrWhiteSpace(execLogFile) && execLogText.Length > 0)
            {
                // Контроль размера лог-файла
                Files.CutEndFileMaxSize(execLogFile);

                try
                {
                    // Дописываем в конец лог-файла
                    File.AppendAllText(execLogFile, Environment.NewLine + Environment.NewLine + execLogText.ToString());
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, ShowMessageMode.SHOW, false, null);
                }
            }
        }

        /// <summary>
        /// Добавить информацию в лог
        /// </summary>
        /// <param name="info">Строка для добавления в лог-файл</param>
        /// <param name="ex">Exception с подробностями ошибки</param>
        /// <param name="showMessageMode">SHOW вывести информационное сообщение</param>
        public string AddExecLog(string info, Exception ex, ShowMessageMode showMessageMode)
        {
            var result = App.AddLog(info, ex, showMessageMode, false, execLogFile);

            if (
                (execLogText != null) &&
                (!string.IsNullOrWhiteSpace(result.fileMessage))
            )
            {
                execLogText.Append(result.fileMessage + Environment.NewLine);
            }

            return result.showMessage;
        }

        /// <summary>Конструктор WinSQLExecute</summary>
        public WinSQLExecute()
        {
            InitializeComponent();
            //Width = APPinfo.minWindowWidth;
            //Height = APPinfo.minWindowHeight;
            Title = "Выполнение SQL-скриптов";

            // инициализация фонового воркера
            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
            Worker.DoWork += new DoWorkEventHandler(Execute);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            Worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);

            // пользовательские настройки GUI
            Default.InitGUI(
                "WinSQLExecute",
                this,
                mainGrid,
                new List<System.Windows.Controls.Control> { tbInfo },
                new List<ComboBox> { cbInfoFontFamily },
                new List<ComboBox> { cbInfoFontSize },
                MainWindow.Task.LogFile
                );
        }

        /// <summary>
        /// Добавить команду для выполнения
        /// </summary>
        /// <param name="filename">полный путь к исполняемому файлу</param>
        /// <param name="uniqname">имя файла для его уникальной идентификации, например путь в проекте git</param>
        /// <param name="scriptkind">вид скрипта</param>
        public void AddSQL(string uniqname, string filename, string scriptkind)
        {
            if (string.IsNullOrWhiteSpace(filename)) filename = "";
            else filename = filename.Trim();

            if (string.IsNullOrWhiteSpace(filename) || (!File.Exists(filename)))
            {
                AddExecLog("Файл " + filename + " не существует", null, App.ShowMessageMode.SHOW);
                return;
            }

            if (Path.GetExtension(filename).ToLower() != ".sql")
            {
                AddExecLog("Файл " + filename + " должен быть с расширением sql", null, App.ShowMessageMode.SHOW);
                return;
            }

            if (filename.Contains(" ")) filename = "\"" + filename + "\"";

            if (scriptkind == "STRUCT" || scriptkind == "CODE")
            {
                // если это скрипт для таблицы/хранимки/вьюхи - делим на chageset и каждый changeset рассматриваем как отдельный файл
                var list = SQLChangeset.ReadChangeset(filename, false, execLogFile);

                foreach (var item in list)
                {
                    string file = Path.GetTempFileName();
                    App.tempFiles.Add(file);

                    File.WriteAllText(file, item.text);
                    SQLFileInfo sql = new SQLFileInfo(uniqname, file, item.name, item.author, true, item.isExecuteSkip, item.isTestChangeset, item.Tags, execLogFile);
                    if (
                        (sql != null) && //-V3063
                        sql.isOk
                    )
                    {
                        ListSQL.Add(sql);
                    }
                }

            }
            else
            {
                // остальные скрипты на chageset не делим, берем все целиком

                string changeset_name = SQLChangeset.FirstChangesetName(filename, out SQLChangeset _changeset);
                SQLFileInfo sql = new SQLFileInfo(uniqname, filename, changeset_name, _changeset.author, false, _changeset.isExecuteSkip, false, _changeset.Tags, execLogFile);
                if (
                    (sql != null) && //-V3063
                    sql.isOk
                )
                {
                    ListSQL.Add(sql);
                }
            }
        }

        /// <summary>Нажата кнопка Ok</summary>
        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            Worker.CancelAsync();
            this.Close();
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
                AddExecLog(e.Error.Message, null, App.ShowMessageMode.SHOW);
            }
        }

        /// <summary>
        /// старт выполнения sql-скриптов
        /// </summary>
        /// <param name="_ConnectSQL">подключение к БД в которой надо выполнить скрипт</param>
        /// <param name="_ConnectLog">подключение к БД в которой находится dbo.SQLGenDBLog</param>
        /// <param name="_isExecuteOnce">=true Фиксировать результат выполнения в dbo.SQLGenDBLog и выполнять скрипт однократно</param>
        /// <param name="_isWait">=true Ожидать завершения</param>
        /// <param name="_YMLName">Общее наименование пакета sql-скриптов для фиксации в dbo.SQLGenDBLog, например название yml-файла</param>
        /// <param name="_idle">максимальное время ожидания (сек)</param>
        public void Start(bool _isWait, ConnectDB _ConnectSQL, ConnectDB _ConnectLog = null, bool _isExecuteOnce = false, string _YMLName = "", int _idle = 600)
        {
            ConnectSQL = _ConnectSQL;
            ConnectLog = _ConnectLog;
            isExecuteOnce = _isExecuteOnce;
            YMLName = _YMLName;
            if (string.IsNullOrWhiteSpace(YMLName))
            {
                YMLName = "Набор sql-скриптов";
            }

            if (ConnectSQL == null)
            {
                AddExecLog("Попытка выполнить скрипт без подключения к БД", null, App.ShowMessageMode.SHOW);
                return;
            }

            if (
                (Worker.IsBusy == false) &&
                (ListSQL.Count > 0)
                )
            {
                ConnectSQL.Timeout = _idle;
                ConnectSQL.ReConnect();

                if (ConnectSQL.isNotConnected)
                {
                    AddExecLog("Подключение " + ConnectSQL.DBConnectionName + " не открыто", null, App.ShowMessageMode.SHOW);
                    return;
                }

                if (
                    (ConnectLog != null) &&
                    (ConnectLog != ConnectSQL)
                    )
                {
                    // если подключение для dbo.SQLGenDBLog находится в другой БД
                    ConnectLog.Timeout = _idle;
                    ConnectLog.ReConnect();
                }

                if (isExecuteOnce)
                {
                    if (
                        (ConnectLog == null) ||
                        ConnectLog.isNotConnected
                    )
                    {
                        AddExecLog("Не удалось подключиться к БД с таблицей dbo.SQLGenDBLog", null, App.ShowMessageMode.SHOW);
                        return;
                    }

                    // создаем dbo.SQLGenDBLog, если его нет
                    if (!ConnectLog.SQLGenDBLogInit(out string Error))
                    {
                        AddExecLog("Не удалось создать dbo.SQLGenDBLog: " + Error, null, App.ShowMessageMode.SHOW);
                        return;
                    }

                    // Пишем стартовую запись
                    AddDBLog(YMLName, "start", false, $"Выполнение {YMLName} через {ConnectSQL.DBConnectionName}" + Environment.NewLine, DateTime.Now.ToString());
                }

                // запускаем фоновую задачу по выгрузке
                highestPercentageReached = 0;
                Worker.RunWorkerAsync();
            }

            if (_isWait)
            {
                this.ShowDialog();
                this.Close();
            }
            else
            {
                this.Show();
            }
        }

        /// <summary>
        /// запись информации в лог
        /// </summary>
        /// <param name="uniqname">уникальное имя скрипта</param>
        /// <param name="changeset">имя changset</param>
        /// <param name="isError">=true - выполнен с ошибкой</param>
        /// <param name="message">сообщение</param>
        /// <param name="checksum">контрольная сумма</param>
        private void AddDBLog(string uniqname, string changeset, bool isError, string message, string checksum)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    this.tbInfo.AppendText(message);
                    string s = tbInfo.GetText();
                    this.tbInfo.ScrollToEnd();
                    if (execLogText != null)
                    {
                        execLogText.Append(message);
                    }
                }

                if (
                    isExecuteOnce &&
                    (ConnectLog != null) &&
                    ConnectLog.isConnected &&
                    (!string.IsNullOrWhiteSpace(uniqname)) &&
                    (!string.IsNullOrWhiteSpace(changeset)) &&
                    (!string.IsNullOrWhiteSpace(checksum)) &&
                    (!isError)
                )
                {
                    // если скрипт выполнен без ошибки - фиксируем результат успешного выполнения
                    ConnectLog.SQLGenDBLogWrite(uniqname, changeset, checksum);
                }
            });
        }

        /// <summary>
        /// выполнить sql-скрипты и закрыть окно
        /// </summary>
        private void Execute(object sender, DoWorkEventArgs e)
        {
            if (ListSQL.Count == 0) return;

            BackgroundWorker worker = sender as BackgroundWorker;

            int maximum = ListSQL.Count;
            int current = 0;
            int result = 0;

            worker.ReportProgress(0);

            // перебираются скрипты/changeset, причем тестовые changeset выполняются после основных changeset
            foreach (var sql in ListSQL.Where(x => x.isOk).OrderBy(x => x.Order))
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.Close();
                    });
                    return;
                }

                current++;

                bool isExecute = true;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.tbCommand.Text = sql.Filename;
                    this.tbInfo.ScrollToEnd();
                });

                string Messages = "";

                if (
                    isExecuteOnce &&
                    (ConnectLog != null) &&
                    ConnectLog.isConnected &&
                    (!sql.isTest)
                )
                {
                    if (string.IsNullOrWhiteSpace(sql.Checksum))
                    {
                        AddExecLog($"Не удалось рассчитать контрольную сумму у файла {sql.Filename}", null, App.ShowMessageMode.SHOW);
                        result = -1;
                        break;
                    }

                    // проверяем, может скрипт уже выполнялся, либо его не надо выполнять
                    if (sql.isExecuteSkip)
                    {
                        isExecute = false;
                        Messages = "Пропущен - содержит теги пропуска (context:newdb или newdb или preConditions onFail:MARK_RAN";
                    }

                    if (ConnectLog.SQLGenDBLogExecuted(sql.Uniqname, sql.Changeset, sql.Checksum))
                    {
                        isExecute = false;
                        Messages = "Пропущен - выполнялся ранее";
                    }
                }

                if (isExecute)
                {
                    // Проверяем имя changeset на соответствие номеру задачи
                    if (!SQLGen.Task.IsMatchTaskNumber(MainWindow.Task.TaskNumber, sql.Changeset) && (!sql.isTest))
                    {
                        Messages = $"Пропущен - имя changeset не соответствует номеру задачи {MainWindow.Task.TaskNumber}";
                        Messages = Environment.NewLine + $"{sql.Uniqname} changeset {sql.Changeset}" + Environment.NewLine + Messages + Environment.NewLine;
                        AddDBLog(sql.Uniqname, sql.Changeset, true, Messages, sql.Checksum); //-V3022
                    }
                    else
                    {
                        try
                        {
                            string text = File.ReadAllText(sql.Filename);
                            ConnectSQL.ExecuteNonQuery(text, out Messages);

                            if (string.IsNullOrWhiteSpace(Messages))
                            {
                                Messages = "Выполнен успешно";
                            }
                            else
                            {
                                Messages = "Выполнен успешно, но есть предупреждения: " + Environment.NewLine + Messages;
                            }
                        }
                        catch (Exception ex)
                        {
                            result = -1;
                            Messages = App.GetFullExceptionMessage(ex).showMessage;
                        }

                        Messages = Environment.NewLine + $"{sql.Uniqname} changeset {sql.Changeset}" + Environment.NewLine + Messages + Environment.NewLine;
                        AddDBLog(sql.Uniqname, sql.Changeset, (result == -1), Messages, sql.Checksum);

                        if (result == -1)
                        {
                            // если ошибка - прерываем исполнение
                            break;
                        }
                    }
                }
                else
                {
                    Messages = Environment.NewLine + $"{sql.Uniqname} changeset {sql.Changeset}" + Environment.NewLine + Messages + Environment.NewLine;
                    AddDBLog(sql.Uniqname, sql.Changeset, true, Messages, sql.Checksum); //-V3022
                }

                int percentComplete = (int)((float)current / (float)maximum * 100);
                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete);
                }
            }

            //worker.ReportProgress(100);

            // завершаем
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (
                    (ConnectLog != null) &&
                    ConnectLog.isConnected
                    )
                {
                    if (result != -1)
                    {
                        AddDBLog(YMLName, "finish", false, Environment.NewLine + "Все скрипты выполнены успешно", DateTime.Now.ToString());
                    }
                    else
                    {
                        AddDBLog(YMLName, "error", false, Environment.NewLine + "Есть ошибки!!!", DateTime.Now.ToString());
                    }
                    ConnectLog.CloseConnect();
                }

                ConnectSQL.CloseConnect();

                //удаляем временные файлы
                foreach (var item in ListSQL.Where(x => x.isTemp))
                {
                    if (item.Filename.ToLower().StartsWith(MainWindow.APPinfo.GITFolder.ToLower()))
                    {
                        // это файл в проекте GIT, его не трогаем!!!
                    }
                    {
                        File.Delete(item.Filename);
                    }
                }

                /*if (result != -1)
                {
                    // закрываем форму если не было ошибок
                    this.Close();
                }*/
            });
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (
                (e.Key == Key.Escape) ||
                (e.Key == Key.Enter)
            )
            {
                btOk_Click(sender, e);
                e.Handled = true;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveLog();

            // пользовательские настройки GUI
            Default.SaveGUI(
                "WinSQLExecute",
                this,
                new List<System.Windows.Controls.Control> { tbInfo }
                );
        }

        private void tbInfo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.End)
            {
                tbInfo.ScrollToEnd();
                e.Handled = true;
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Home)
            {
                tbInfo.ScrollToHome();
                e.Handled = true;
            }
            else if (e.Key == Key.PageUp)
            {
                tbInfo.PageUp();
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown)
            {
                tbInfo.PageDown();
                e.Handled = true;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            tbInfo.Focus();
        }

        private void tbInfo_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }

    /// <summary>
    /// описание выполляемого sql-файла
    /// </summary>
    public class SQLFileInfo
    {
        /// <summary>
        /// Признак корректности sql-файла
        /// </summary>
        public bool isOk = false;

        /// <summary>
        ///  Полное имя sql-файла
        /// </summary>
        public string Filename;

        /// <summary>
        /// имя changeset
        /// </summary>
        public string Changeset;

        /// <summary>
        /// авто changeset
        /// </summary>
        public string Author;

        /// <summary>
        /// Контрольная сумма sql-файла
        /// </summary>
        public string Checksum = "";

        /// <summary>
        /// Уникальное имя sql-файла
        /// </summary>
        public string Uniqname;

        /// <summary>
        /// =true - временный файл
        /// </summary>
        public bool isTemp = false;

        /// <summary>
        /// =true - при исполнении надо пропустить
        /// </summary>
        public bool isExecuteSkip = false;

        /// <summary>
        /// =true - тестовый changeset
        /// </summary>
        public bool isTest = false;

        /// <summary>
        /// метка скрипта: struct, code, data, finish
        /// </summary>
        public string ScriptLabel = "";

        /// <summary>
        /// =99-тестовый changeset, =1-структура =2-код+данные =3-остальные changeset
        /// </summary>
        public int Order
        {
            get
            {
                if (isTest)
                {
                    return 99;
                }
                else
                {
                    if (MainWindow.APPinfo.isImproveSQLinVersion)
                    {
                        if (ScriptLabel == "struct" || string.IsNullOrWhiteSpace(ScriptLabel))
                        {
                            return 1;
                        }
                        else if (ScriptLabel == "code" || ScriptLabel == "data")
                        {
                            return 2;
                        }
                        else
                        {
                            return 3;
                        }
                    }
                    else
                    {
                        return 3;
                    }
                }
            }
        }

        /// <summary>
        /// инициализация экземпляра с описанием sql-файла
        /// </summary>
        /// <param name="_uniqname">уникальное имя файла</param>
        /// <param name="_filename">полный путь к файлу</param>
        /// <param name="_changeset">название changeset в файле</param>
        /// <param name="_author">автор changeset в файле</param>
        /// <param name="_istemp">=true - временный файл</param>
        /// <param name="_isskip">=true - пропустить выполнение</param>
        /// <param name="_istest">=true - тестовый changeset</param>
        /// <param name="_tags">список тегов в строке --changeset</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public SQLFileInfo(string _uniqname, string _filename, string _changeset, string _author, bool _istemp, bool _isskip, bool _istest, Dictionary<string, string> _tags, string logFile)
        {
            if (string.IsNullOrWhiteSpace(_filename))
            {
                App.AddLog("Не указан полный путь к sql-файлу", null, App.ShowMessageMode.SHOW, true, logFile);
                isOk = false;
                return;
            }

            if (string.IsNullOrWhiteSpace(_changeset))
            {
                _changeset = "unknown";
            }

            if (string.IsNullOrWhiteSpace(_author))
            {
                _author = "unknown";
            }

            if (string.IsNullOrWhiteSpace(_uniqname))
            {
                App.AddLog("Не указано уникальное имя sql-файла", null, App.ShowMessageMode.SHOW, true, logFile);
                isOk = false;
                return;
            }

            if (!File.Exists(_filename))
            {
                App.AddLog($"Файл {_filename} не существует", null, App.ShowMessageMode.SHOW, true, logFile);
                isOk = false;
                return;
            }

            if (
                (!_istemp) &&
                (Path.GetExtension(_filename).ToLower() != ".sql")
            )
            {
                App.AddLog($"У файла {_filename} должно быть расширение .sql", null, App.ShowMessageMode.SHOW, true, logFile);
                isOk = false;
                return;
            }

            // вычисляем контрольную сумму
            try
            {
                Checksum = Utilities.Files.ComputeMD5ChecksumFile(_filename);
            }
            catch (Exception ex)
            {
                App.AddLog($"Не удалось рассчитать контрольную сумму у файла {_filename}", ex, App.ShowMessageMode.NONE, true, logFile);
                Checksum = "";
            }

            Filename = _filename;
            Uniqname = _uniqname;
            Changeset = _changeset;
            Author = _author;
            isTemp = _istemp;
            isOk = true;
            isExecuteSkip = _isskip;
            isTest = _istest;

            if (
                _tags != null &&
                _tags.TryGetValue("labels", out string tag_value)
            )
            {
                ScriptLabel = tag_value ?? "";
            }
        }
    }

}
