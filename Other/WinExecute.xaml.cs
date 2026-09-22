// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using SQLGen.Controls;
using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using static SQLGen.App;

namespace SQLGen
{
    /// <summary>
    /// Выполнить внешний исполняемый файл и в окне отобразить вывод или ошибки
    /// </summary>
    public partial class WinExecute : Window
    {
        /// <summary>
        /// фоновый воркер
        /// </summary>
        private BackgroundWorker Worker;

        /// <summary>
        /// максимальный достигнутый прогресс в выполнении
        /// </summary>
        private int highestPercentageReached = 0;

        /// <summary>
        /// список команд на последовательное исполнение
        /// </summary>
        public List<ProcessStartInfo> ListCommands = new List<ProcessStartInfo>();

        /// <summary>
        /// максимальное время ожидание
        /// </summary>
        private int Idle = 600;

        // спрашивать при запуске каждой программы 
        //private bool isAskByOne = false;

        /// <summary>
        ///  Выводить все сообщения об ошибка
        /// </summary>
        public bool isShowAllErrors = false;

        /// <summary>
        ///  Полный путь к файлу для лога выполнения
        /// </summary>
        public string execLogFile = "";

        /// <summary>
        /// Результат выполнения внешних команд для записи в лог-файл
        /// </summary>
        private StringBuilder execLogText = new StringBuilder(100000);

        /// <summary>
        /// Что вернула последняя команда
        /// 99999 - выполнение прервано пользователем
        /// -1 - неизвестная ошибка
        /// >0 - коды ошибок
        /// 0 - выполнено успешно
        /// </summary>
        public int LastExitCode = 0;

        /// <summary>
        /// Список внешних номеров выполненных успешно команд
        /// </summary>
        public List<int> ListNumSuccess = new List<int>();

        /// <summary>
        /// Последняя ошибка
        /// </summary>
        public string LastError = "";

        /// <summary>
        /// =true - Прервать выполнение списка комманд после первой ошибки (без запросов о продолжении)
        /// </summary>
        public bool isStopAfterFirstError = false;

        /// <summary>
        /// =true - оставлять окно открытым после ошибок
        /// </summary>
        public bool isShowLogAfterError = false;

        /// <summary>
        /// =true - оставлять окно открытым после успешного завершения
        /// </summary>
        public bool isShowLogAfterSuccess = false;

        /// <summary>
        /// если != null - принудительно читаем возвращаемый результат в указанной кодировке
        /// </summary>
        public Encoding OutputEncoding = null;

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

        /// <summary>
        /// Конструктор WinExecute
        /// </summary>
        /// <param name="_logfile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        public WinExecute(string _logfile)
        {
            InitializeComponent();
            //Width = APPinfo.minWindowWidth;
            //Height = APPinfo.minWindowHeight;
            Title = "Выполнение внешней команды";

            execLogFile = _logfile;

            // инициализация фонового воркера
            Worker = new BackgroundWorker();
            Worker.WorkerReportsProgress = true;
            Worker.WorkerSupportsCancellation = true;
            Worker.DoWork += new DoWorkEventHandler(Execute);
            Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            Worker.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);

            // пользовательские настройки GUI
            Default.InitGUI(
                "WinExecute",
                this,
                mainGrid,
                new List<System.Windows.Controls.Control> { tbInfo },
                new List<ComboBox> { cbInfoFontFamily },
                new List<ComboBox> { cbInfoFontSize },
                execLogFile
                );
        }

        /// <summary>
        /// Добавить команду для выполнения
        /// </summary>
        /// <param name="workdir">рабочий каталог</param>
        /// <param name="filename">исполняемый файл</param>
        /// <param name="param">параметры</param>
        /// <param name="show">строка, которую надо показывать на экранах и записывать в лог вместо оригинальной команды</param>
        /// <param name="num">внешний номер задания</param>
        public void AddCommand(string workdir, string filename, string param, string show = null, int num = 0)
        {
            if (string.IsNullOrWhiteSpace(workdir)) workdir = "";
            else workdir = workdir.Trim();

            if (string.IsNullOrWhiteSpace(filename)) filename = "";
            else filename = filename.Trim();

            if (string.IsNullOrWhiteSpace(param)) param = "";
            else param = param.Trim();

            if (string.IsNullOrWhiteSpace(show)) show = filename + " " + param;
            else show = show.Trim();

            if (
                string.IsNullOrWhiteSpace(filename) ||
                (
                    (!string.IsNullOrWhiteSpace(Path.GetExtension(filename))) &&
                    (!File.Exists(filename))
                )
            )
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AddExecLog("Файл " + filename + " не существует!", null, App.ShowMessageMode.SHOW);
                });
            }
            else
            {
                if (workdir.Contains(" ")) workdir = "\"" + workdir + "\"";
                if (filename.Contains(" ")) filename = "\"" + filename + "\"";

                var startInfo = new System.Diagnostics.ProcessStartInfo
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    WorkingDirectory = workdir,
                    FileName = filename,
                    Arguments = param,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    //RedirectStandardError = true,
                    CreateNoWindow = true
                };

                startInfo.Environment.Add("sqlgen_showcommand", show);
                startInfo.Environment.Add("sqlgen_numcommand", num.ToString());

                ListCommands.Add(startInfo);
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

        /// <summary>Отображение вывода программы</summary>
        void process_DataReceived(object sender, DataReceivedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                this.tbInfo.AppendText(e.Data + Environment.NewLine);
                this.tbInfo.ScrollToEnd();
                if (execLogText != null)
                {
                    execLogText.Append(e.Data + Environment.NewLine);
                }
            });
        }

        /// <summary>
        /// старт выполнения команд
        /// </summary>
        /// <param name="isWait">=true Ожидать завершения</param>
        /// <param name="_idle">максимальное время ожидания</param>
        /// <param name="_isAskByOne">=true спрашивать при запуске каждой программы</param>
        public void Start(bool isWait, int _idle = 600, bool _isAskByOne = false)
        {
            Idle = _idle;
            //isAskByOne = _isAskByOne;

            if (
                (Worker.IsBusy == false) &&
                (ListCommands.Count > 0)
                )
            {
                // запускаем фоновую задачу по выгрузке
                highestPercentageReached = 0;
                Worker.RunWorkerAsync();
            }
            if (isWait)
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
        /// выполнить команды и закрыть окно
        /// </summary>
        private void Execute(object sender, DoWorkEventArgs e)
        {
            if (ListCommands.Count == 0) return;

            BackgroundWorker worker = sender as BackgroundWorker;

            int maximum = ListCommands.Count;
            int current = 0;
            ListNumSuccess = new List<int>();

            worker.ReportProgress(0);

            AddExecLog($"Начинаем выполнение внешних команд", null, App.ShowMessageMode.NONE);

            if (isStopAfterFirstError)
            {
                AddExecLog($"Включен режим принудительного прерывания после первой ошибки", null, App.ShowMessageMode.NONE);
            }

            foreach (var startInfo in ListCommands)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        LastExitCode = 99999;
                        LastError = "Пользователь прервал выполнение внешних команд";
                        AddExecLog(LastError, null, App.ShowMessageMode.NONE);

                        this.Close();
                    });
                    return;
                }

                current++;

                bool isExec = true;
                LastExitCode = 0;
                LastError = "";
                Process CommandProcess = new Process();

                // что показывать в логах вместо оригинальной команды
                string showCommand;
                if (!startInfo.Environment.ContainsKey("sqlgen_showcommand"))
                {
                    showCommand = startInfo.FileName + " " + startInfo.Arguments;
                }
                else
                {
                    showCommand = startInfo.Environment["sqlgen_showcommand"];
                }

                // внешний номер выполняемой команды
                int numCommand = 0;
                if (startInfo.Environment.ContainsKey("sqlgen_numcommand"))
                {
                    if (!int.TryParse(startInfo.Environment["sqlgen_numcommand"], out numCommand))
                    {
                        numCommand = 0;
                    }
                }

                Application.Current.Dispatcher.Invoke(() =>
                    {
                        this.tbCommand.Text = showCommand
                            .Replace(Environment.NewLine, " ")
                            .Replace("\n", " ")
                            .Replace("\r", " ")
                            .Replace("\t", " ")
                            .Replace("  ", " ")
                            .Replace("  ", " ")
                            .Replace("  ", " ")
                            ;
                        this.tbInfo.ScrollToEnd();

                        //this.tbInfo.Text = "";

                        /*if (isAskByOne)
                        {
                            var isAsk = System.Windows.Forms.MessageBox.Show($"Выполнить {showCommand} ?", "", System.Windows.Forms.MessageBoxButtons.YesNo);
                            if (isAsk == System.Windows.Forms.DialogResult.No)
                            {
                                isExec = false;
                            }
                        }*/

                        if (isExec)
                        {
                            this.tbInfo.AppendText(showCommand + Environment.NewLine + Environment.NewLine);
                            this.tbInfo.ScrollToEnd();
                        }
                    });

                if (isExec) //-V3022
                {
                    try
                    {
                        //CommandProcess.ErrorDataReceived += new DataReceivedEventHandler(process_DataReceived);
                        CommandProcess.OutputDataReceived += new DataReceivedEventHandler(process_DataReceived);

                        startInfo.Arguments = "/C " + startInfo.FileName + " " + startInfo.Arguments + " 2>&1";
                        startInfo.FileName = "cmd.exe";

                        //startInfo.Arguments = startInfo.Arguments;
                        //startInfo.FileName = startInfo.FileName;

                        AddExecLog("cmd.exe /C " + showCommand, null, App.ShowMessageMode.NONE);

                        if (OutputEncoding != null)
                        {
                            startInfo.StandardOutputEncoding = OutputEncoding;
                        }

                        CommandProcess.StartInfo = startInfo;
                        CommandProcess.Start();
                        CommandProcess.BeginOutputReadLine();
                        if (Idle <= 0)
                        {
                            CommandProcess.WaitForExit();
                        }
                        else
                        {
                            CommandProcess.WaitForExit(Idle * 1000);
                        }
                        //CommandProcess.Refresh();

                        if (CommandProcess.ExitCode != 0)
                        {
                            if (CommandProcess.ExitCode == 10001)
                            {
                                LastError = $"Ошибка {CommandProcess.ExitCode} - неверные параметры запуска внешней программы: " + Environment.NewLine +
                                    "cmd.exe /C " + showCommand;
                            }
                            else if (CommandProcess.ExitCode == 10002)
                            {
                                LastError = $"Ошибка {CommandProcess.ExitCode} - текущая ветка не соответствует ветке задачи";
                            }
                            else if (CommandProcess.ExitCode == 10003)
                            {
                                LastError = $"Ошибка {CommandProcess.ExitCode} - ошибка команды GIT";
                            }
                            else if (CommandProcess.ExitCode == 10004)
                            {
                                if (
                                    isShowAllErrors ||
                                    (!startInfo.Arguments.ToLower().Contains("git_switch.cmd"))
                                    )
                                {
                                    LastError = $"Ошибка {CommandProcess.ExitCode} - ветка GIT не существует";
                                }
                            }
                            else if (CommandProcess.ExitCode == 10005)
                            {
                                LastError = $"Ошибка {CommandProcess.ExitCode} - ошибка при разрешении конфликта MERGE";
                            }
                            else if (CommandProcess.ExitCode == 10006)
                            {
                                LastError = $"Ошибка {CommandProcess.ExitCode} - требуется COMMIT";
                            }
                            else if (CommandProcess.ExitCode == 10007)
                            {
                                LastError = $"Ошибка {CommandProcess.ExitCode} - НЕ требуется COMMIT";
                            }
                            else if (CommandProcess.ExitCode == 10008)
                            {
                                LastError = $"Ошибка {CommandProcess.ExitCode} - ветка содержит файл BRANCH_DEV, т.е. ветка сделана от dev или в ветку сделали merge dev. Такую ветку нельзя merge в ветки версий или в master!!!";
                            }
                            else
                            {
                                LastError = $"Ошибка {CommandProcess.ExitCode}";
                            }

                            AddExecLog($"{LastError}", null, App.ShowMessageMode.NONE);
                        }
                        else
                        {
                            // выполнено успешно
                            ListNumSuccess.Add(numCommand);
                        }

                        LastExitCode = CommandProcess.ExitCode;
                    }
                    catch (Exception ex)
                    {
                        LastExitCode = -1;
                        LastError = AddExecLog(null, ex, App.ShowMessageMode.NONE);
                    }

                    AddExecLog($"ExitCode = {LastExitCode}", null, App.ShowMessageMode.NONE);
                }

                int percentComplete = (int)((float)current / (float)maximum * 100);
                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete);
                }

                CommandProcess.Close();
                CommandProcess.Dispose();

                if (isExec) //-V3022
                {
                    if (!string.IsNullOrWhiteSpace(LastError))
                    {
                        bool isexit = false;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (
                                isStopAfterFirstError ||
                                System.Windows.Forms.MessageBox.Show(
                                    this.tbCommand.Text + " - ошибка при выполнении: " + Environment.NewLine +
                                    Environment.NewLine +
                                    LastError + Environment.NewLine +
                                    Environment.NewLine +
                                    "Прервать выполнение ?" + Environment.NewLine +
                                    "Yes\\Да - Прервать, No\\Нет - Продолжить",
                                "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes
                                )
                            {
                                AddExecLog($"Прервано выполнение внешних команд после ошибки", null, App.ShowMessageMode.NONE);

                                if (!isShowLogAfterError)
                                {
                                    this.Close();
                                }
                                else
                                {
                                    this.tbInfo.AppendText("Прервано выполнение внешних команд после ошибки");
                                    this.tbInfo.ScrollToEnd();
                                }

                                isexit = true;
                            }
                        });

                        if (isexit)
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                AddExecLog($"Успешно завершено выполнение внешних команд", null, App.ShowMessageMode.NONE);

                this.tbInfo.AppendText("Успешно завершено выполнение внешних команд");
                this.tbInfo.ScrollToEnd();
                this.tbCommand.Text = "";

                if (!isShowLogAfterSuccess)
                {
                    this.Close();
                }
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
                "WinExecute",
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
    }
}
