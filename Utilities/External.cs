// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SQLGen.Utilities
{
    /// <summary>
    /// Работа с внешним кодом
    /// </summary>
    public static class External
    {
        /// <summary>
        /// Тип выполнения скрипта
        /// </summary>
        public enum ExecType
        {
            /// <summary>
            /// выполнить список команд (по умолчанию)
            /// </summary>
            DEFAULT,
            /// <summary>
            /// выполнить запрос, возвращающий строки
            /// </summary>
            QUERY,
            /// <summary>
            /// вызов генератора хранимок 
            /// </summary>
            GENERATOR
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Открыть внешний текстовый файл</summary>
        /// <param name="filename">Путь к файлу</param>
        public static string OpenExternalFile(string filename)
        {
            string error = "";

            if (string.IsNullOrWhiteSpace(filename) || (!File.Exists(filename)))
            {
                error = "Файл " + filename + " не существует!";
                App.AddLog(error, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                return error;
            }

            // Открываемый файл
            filename = filename.Trim();
            if (filename.Contains(" ")) filename = "\"" + filename + "\"";

            // программа для открытия файла
            string application = MainWindow.APPinfo.FileEditor;

            try
            {
                if (string.IsNullOrWhiteSpace(application) || (!File.Exists(application)))
                {
                    System.Diagnostics.Process.Start(filename);
                }
                else
                {
                    using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                    {
                        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(application);

                        if (application.Contains(" ")) application = "\"" + application + "\"";

                        process.StartInfo.FileName = application;
                        process.StartInfo.Arguments = filename;

                        App.AddLog(process.StartInfo.FileName + " " + process.StartInfo.Arguments, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                        process.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                error = App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile).showMessage;
                return error;
            }

            return "";
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Открыть каталог для просмотра</summary>
        /// <param name="path">Путь к каталогу</param>
        public static string OpenDirectory(string path)
        {
            string error = "";

            if (string.IsNullOrWhiteSpace(path) || (!Directory.Exists(path)))
            {
                error = "Каталог " + path + " не существует!";
                App.AddLog(error, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                return error;
            }

            // каталог
            path = path.Trim();
            if (path.Contains(" ")) path = "\"" + path + "\"";

            // программа для открытия каталога
            string application = MainWindow.APPinfo.DirectoryEditor;

            try
            {
                if (string.IsNullOrWhiteSpace(application) || (!File.Exists(application)))
                {
                    System.Diagnostics.Process.Start(path);
                }
                else
                {
                    using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                    {

                        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(application);

                        if (application.Contains(" ")) application = "\"" + application + "\"";

                        process.StartInfo.FileName = application;
                        process.StartInfo.Arguments = path;

                        App.AddLog(process.StartInfo.FileName + " " + process.StartInfo.Arguments, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                        process.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                error = App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile).showMessage;
                return error;
            }

            return "";
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выполнить внешний исполняемый файл</summary>
        /// <param name="workdir">рабочий каталог</param>
        /// <param name="filename">Путь к исполняемому файлу</param>
        /// <param name="param">Параметры командной строки для исполняемого файла</param>
        /// <param name="isWait">Ждать завершения выполнения</param>
        /// <param name="isShow">Показать окно в котором выполняется файл</param>
        /// <param name="isOutput">Получить вывод программы</param>
        /// <param name="isShowError">Выводить сообщения об ошибках</param>
        /// <param name="Idle">Максимальный период ожидания, если isWait = true, isShow = false</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        public static string ExecuteFile(string workdir, string filename, string param, bool isWait, bool isShow, bool isOutput, bool isShowError, string logFile, int Idle = 300)
        {
            string error = "";
            string output = "";

            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            if (isOutput)
            {
                isShow = false;
                isWait = true;
            }

            if (string.IsNullOrWhiteSpace(workdir)) workdir = "";
            else workdir = workdir.Trim();

            if (string.IsNullOrWhiteSpace(filename)) filename = "";
            else filename = filename.Trim();

            if (string.IsNullOrWhiteSpace(param)) param = "";
            else param = param.Trim();

            if (
                string.IsNullOrWhiteSpace(filename) ||
                (
                    (!string.IsNullOrWhiteSpace(Path.GetExtension(filename))) &&
                    (!File.Exists(filename))
                )
            )
            {
                error = "Файл " + filename + " не существует!";
                if (isShowError)
                {
                    App.AddLog(error, null, App.ShowMessageMode.SHOW, true, logFile);
                }
                return error;
            }

            if (workdir.Contains(" ")) workdir = "\"" + workdir + "\"";
            if (filename.Contains(" ")) filename = "\"" + filename + "\"";

            try
            {
                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    if (isShow)
                    {
                        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                    }
                    else
                    {
                        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    }

                    process.StartInfo.WorkingDirectory = workdir;
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = "/C " + filename + " " + param /*+ " 2>&1"*/;

                    if (isOutput)
                    {
                        process.StartInfo.UseShellExecute = false;
                        process.StartInfo.CreateNoWindow = true;
                        process.StartInfo.RedirectStandardOutput = true;
                    }
                    else
                    {
                        process.StartInfo.UseShellExecute = true;
                    }

                    App.AddLog(process.StartInfo.FileName + " " + process.StartInfo.Arguments, null, App.ShowMessageMode.NONE, true, logFile);

                    process.Start();

                    if (isOutput)
                    {
                        StreamReader reader = process.StandardOutput;
                        output = reader.ReadToEnd();
                    }

                    if (isWait)
                    {
                        if (isShow)
                        {
                            process.WaitForExit();
                        }
                        else
                        {
                            process.WaitForExit(Idle * 1000);
                        }

                        if (
                            (process.ExitCode != 0) &&
                            (
                                 (process.ExitCode != 10004) ||
                                 filename.ToLower().Contains("git_switch.cmd")
                            )
                        )
                        {
                            error = $"Ошибка {process.ExitCode}";
                        }

                        App.AddLog($"ExitCode = {process.ExitCode}", null, App.ShowMessageMode.NONE, true, logFile);
                    }

                }
            }
            catch (Exception ex)
            {
                if (isShowError)
                {
                    error = App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile).showMessage;
                }
                else
                {
                    error = App.AddLog("", ex, App.ShowMessageMode.NONE, true, logFile).showMessage;
                }
                return error;
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                return error;
            }
            else
            {
                return output;
            }
        }

        /// <summary>
        /// выполнить скрипт в отдельном потоке
        /// </summary>
        /// <param name="_type">Тип выполнения скрипта</param>
        /// <param name="_window">Окно</param>
        /// <param name="_cbConnect">ComboBox с подключениями к БД</param>
        /// <param name="_Script">Скрипт</param>
        /// <param name="_btExec">Button, которая вызывает выполнение выделенного запроса</param>
        /// <param name="_tiResults">TabItem с результатами</param>
        /// <param name="_dgResults">DataGrid с результатами</param>
        /// <param name="_tiMessages">TabItem с ошибками и предупреждениями</param>
        /// <param name="_tbMessages">TextBox с ошибками и предупреждениями</param>
        /// <param name="_lbStatus">Label с кол-вом записей</param>
        public static async void ExecScriptInThread(ExecType _type, System.Windows.Window _window, System.Windows.Controls.ComboBox _cbConnect, string _Script, System.Windows.Controls.Button _btExec, System.Windows.Controls.TabItem _tiResults, System.Windows.Controls.DataGrid _dgResults, System.Windows.Controls.TabItem _tiMessages, System.Windows.Controls.Control _tbMessages, System.Windows.Controls.Label _lbStatus)
        {
            if (_window == null) return;
            if (_cbConnect == null) return;
            if (string.IsNullOrWhiteSpace(_Script)) return;

            // убираем тестовые changeset из скрипта
            string txt = "";
            foreach (var item in SQLChangeset.ReadChangeset(_Script, true, MainWindow.Task.LogFile)
                .Where(x => x.name != "test" || x.author != "dev")
            )
            {
                txt += item.text + "\n\n";
            }
            _Script = txt;

            // выполняем скрипт
            _window.Cursor = Cursors.Wait;

            string old = "";

            if (_btExec != null)
            {
                Utilities.Controls.ChangeButtonContent(_btExec, "Выполнение...", FontWeights.Bold, false, out old);
            }

            if (_dgResults != null)
            {
                _dgResults.ItemsSource = null;
            }
            if (_lbStatus != null)
            {
                _lbStatus.Content = "Кол-во записей:";
            }

            if (_tbMessages != null)
            {
                if (_tbMessages is System.Windows.Controls.TextBox)
                {
                    ((System.Windows.Controls.TextBox)_tbMessages).Text = "";
                }
                if (_tbMessages is System.Windows.Controls.RichTextBox)
                {
                    ((System.Windows.Controls.RichTextBox)_tbMessages).SetText("");
                }
                if (_tbMessages is TextEditor)
                {
                    ((TextEditor)_tbMessages).Text = "";
                }
            }

            System.Data.DataTable result = null;
            string Messages = "";

            ConnectDB Connect = Utilities.Controls.OpenConnectFromComboBox(_cbConnect, false);
            if ((Connect == null) || Connect.isNotConnected) //-V3063
            {
                _cbConnect.Focus();
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Подключение " + _cbConnect.Text + " не открыто !");
                });
                if (_tbMessages != null)
                {
                    string s = "Подключение " + _cbConnect.Text + " не открыто !";

                    if (_tbMessages is System.Windows.Controls.TextBox)
                    {
                        ((System.Windows.Controls.TextBox)_tbMessages).Text = s;
                    }
                    if (_tbMessages is System.Windows.Controls.RichTextBox)
                    {
                        ((System.Windows.Controls.RichTextBox)_tbMessages).SetText(s);
                    }
                    if (_tbMessages is TextEditor)
                    {
                        ((TextEditor)_tbMessages).Text = s;
                    }
                }
            }
            else
            {
                Messages = await System.Threading.Tasks.Task.Run(() =>
                {
                    string Info = "";
                    try
                    {
                        if (_type == ExecType.GENERATOR)
                        {
                            {
                                using (DbDataReader reader = Connect.OpenQuery(_Script))
                                {
                                    if (reader != null)
                                    {
                                        while (reader.Read())
                                        {
                                            for (int i = 0; i < reader.FieldCount; i++)
                                            {
                                                Info += reader[i].ToString() + Environment.NewLine;
                                            }

                                        }
                                    }
                                }
                            }

                            if (string.IsNullOrWhiteSpace(Info))
                            {
                                Connect.ExecuteNonQuery(_Script, out Info);
                            }
                        }
                        else if (
                                (_type == ExecType.QUERY) &&
                                (_dgResults != null)
                                )
                        {
                            result = Connect.FillDataTable(_Script, out Info);
                            if (!string.IsNullOrWhiteSpace(Info))
                            {
                                Info = "Выполнено успешно, но есть предупреждения:" + Environment.NewLine + Info;
                            }
                            else
                            {
                                Info = "Выполнено успешно!";
                            }
                        }
                        else
                        {
                            Connect.ExecuteNonQuery(_Script, out Info);
                            if (!string.IsNullOrWhiteSpace(Info))
                            {
                                Info = "Выполнено успешно, но есть предупреждения:" + Environment.NewLine + Info;
                            }
                            else
                            {
                                Info = "Выполнено успешно!";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Info = App.AddLog("Ошибка выполнения: " + Environment.NewLine, ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile).showMessage;
                        result = null;
                    }
                    return Info;
                });

                Connect.CloseConnect();
            }

            _window.Cursor = Cursors.Arrow;

            if (_btExec != null)
            {
                Utilities.Controls.ChangeButtonContent(_btExec, old, FontWeights.Normal, true, out old);
            }

            if (result != null)
            {
                if (_dgResults != null)
                {
                    _dgResults.ItemsSource = result.DefaultView;
                    _dgResults.Focus();
                }

                if (_tiResults != null)
                {
                    _tiResults.IsSelected = true;
                }

                if (_lbStatus != null)
                {
                    _lbStatus.Content = "Кол-во записей: " + result.Rows.Count.ToString();
                }
            }

            if (!string.IsNullOrWhiteSpace(Messages))
            {
                if (_tbMessages != null)
                {
                    string s = Messages;

                    if (_tbMessages is System.Windows.Controls.TextBox)
                    {
                        ((System.Windows.Controls.TextBox)_tbMessages).Text = s;
                    }
                    if (_tbMessages is System.Windows.Controls.RichTextBox)
                    {
                        ((System.Windows.Controls.RichTextBox)_tbMessages).SetText(s);
                    }
                    if (_tbMessages is TextEditor)
                    {
                        ((TextEditor)_tbMessages).Text = s;
                    }
                    _tbMessages.Focus();
                }

                if (
                    (_tiResults != null) &&
                    (_dgResults != null) &&
                    (_dgResults.ItemsSource != null)
                )
                {
                    _tiResults.IsSelected = true;
                }
                else
                {
                    if (_tbMessages == null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            MessageBox.Show(Messages);
                        });

                    }
                    else
                    {
                        if (_tiMessages != null)
                        {
                            _tiMessages.IsSelected = true;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Выполнить команду bash
        /// Пример: var output = ExecuteBashCommand("t=$(echo 'this is a test'); echo \"$t\" | grep -o 'is a'");
        /// </summary>
        /// <param name="command">Команда bash</param>
        /// <returns></returns>
        public static string ExecuteBashCommand(string command)
        {
            // according to: https://stackoverflow.com/a/15262019/637142
            // thans to this we will pass everything as one command
            command = command.Replace("\"", "\"\"");

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Registry.GetRegistryValue("HKEY_CURRENT_USER\\SOFTWARE\\TortoiseGit", "MSysGit", "C:\\Program Files\\Git\\bin"), "bash.exe"),
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            proc.Start();
            proc.WaitForExit();

            return proc.StandardOutput.ReadToEnd();
        }
    }
}
