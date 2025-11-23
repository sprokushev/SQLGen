// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Startup, обработка ошибок, ведение логов
    /// </summary>
    public partial class App : Application
    {

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Аргументы командной строки</summary>
        public static string[] Args;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Каталог приложения</summary>
        public static string AppPath;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Путь к лог-файлу приложения</summary>
        public static string AppLogFile;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Версия GIT</summary>
        public static string GITVersion;

        /// <summary>Клиент GIT установлен и найден на ПК</summary>
        public static bool IsGITExists;

        /// <summary>Список временных файлов приложения</summary>
        public static List<string> tempFiles = new List<string>();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>При запуске приложения</summary>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            //AppPath = AppContext.BaseDirectory;
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            AppPath = Path.GetDirectoryName(thisAssembly.Location);

            AppLogFile = Path.Combine(AppPath, "SQLGen.log");

            if (File.Exists(AppLogFile))
            {
                Encoding encoding = new UTF8Encoding(false);

                // 10 MB max file size
                const long maxsize = 10 * 1024 * 1024;

                // контроль размера лог-файла
                FileInfo txtfile = new FileInfo(AppLogFile);
                if (txtfile.Length > maxsize)
                {
                    var lines = File.ReadAllLines(AppLogFile);
                    long size = 0;
                    for (int i = lines.Count() - 1; i >= 0; i--)
                    {
                        size += encoding.GetByteCount(lines[i]);
                        if (size >= maxsize)
                        {
                            lines = lines.Skip(i).ToArray();
                            File.WriteAllLines(AppLogFile, lines);
                            break;
                        }
                    }
                    //var lines = File.ReadAllLines(AppLogFile).Skip(20).ToArray();  // ## skip first 20 lines
                }
            }

            AddLog("------------------------------------------------------------", null, App.ShowMessageMode.NONE, true, null);
            AddLog("Запуск приложения", null, App.ShowMessageMode.NONE, true, null);
            AddLog(thisAssembly.Location, null, App.ShowMessageMode.NONE, true, null);

            if (e.Args.Length > 0)
            {
                Args = e.Args;
                AddLog("Параметры командной строки:", null, App.ShowMessageMode.NONE, true, null);
                foreach (var item in Args)
                {
                    AddLog(item, null, App.ShowMessageMode.NONE, true, null);
                }
            }

            AssemblyName thisAssemblyName = thisAssembly.GetName();
            AddLog("Версия приложения: " + thisAssemblyName.Version, null, App.ShowMessageMode.NONE, true, null);
            AddLog("ОС: " + System.Runtime.InteropServices.RuntimeInformation.OSDescription, null, App.ShowMessageMode.NONE, true, null);
            AddLog("Архитектура ОС: " + System.Runtime.InteropServices.RuntimeInformation.OSArchitecture.ToString(), null, App.ShowMessageMode.NONE, true, null);
            AddLog("Архитектура процессора: " + System.Runtime.InteropServices.RuntimeInformation.ProcessArchitecture.ToString(), null, App.ShowMessageMode.NONE, true, null);
            AddLog("Версия Net: " + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription, null, App.ShowMessageMode.NONE, true, null);
            GITVersion = GetGitVersion();
            IsGITExists = (!string.IsNullOrWhiteSpace(GITVersion));
            AddLog("Версия GIT: " + GITVersion, null, App.ShowMessageMode.NONE, true, null);

            // Global exception handling  
            Application.Current.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(AppDispatcherUnhandledException);
        }

        /// <summary>
        /// Определить версию GIT
        /// </summary>
        public static string GetGitVersion()
        {
            // проверить текущую ветку
            string ver = Utilities.External.ExecuteFile(
                    App.AppPath,
                    Path.Combine(App.AppPath, "git_version.cmd"),
                    "",
                    true,
                    false,
                    true,
                    false,
                    null
                );

            if (string.IsNullOrWhiteSpace(ver))
            {
                ver = "";
            }
            else
            {
                ver = ver.TrimAllSpace();
            }

            if (
                ver.ToLower().Contains("ошибка") ||
                ver.ToLower().Contains("error") ||
                ver.ToLower().Contains("fatal")
                )
            {
                return "";
            }

            ver = ver.Replace('\n', '|').Replace('\r', '|');

            string result = "";

            foreach (var item in ver.Split('|'))
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    result = item;
                }
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>При завершении приложения</summary>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            // удаляем временные файлы приложения
            foreach (var item in tempFiles)
            {
                try
                {
                    File.Delete(item);
                }
                catch (Exception)
                {
                }
            }

            AddLog("Завершение работы приложения", null, App.ShowMessageMode.NONE, true, null);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Обработчик ошибок</summary>
        private void AppDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
#if DEBUG   // In debug mode do not custom-handle the exception, let Visual Studio handle it

            e.Handled = false;

#else

                ShowUnhandledException(e);    

#endif
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Обработчик для необработанных в приложении ошибок</summary>
        private void ShowUnhandledException(DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;

            if (SQLGen.MainWindow.isStartup)
            {
                string errorMessage = string.Format(@"Необработанная ошибка приложения:" + Environment.NewLine + Environment.NewLine + @"{0}" + Environment.NewLine + Environment.NewLine + @"Работа в приложении будет завершена!", e.Exception.Message + (e.Exception.InnerException != null ? Environment.NewLine + e.Exception.InnerException.Message : ""));

                AddLog(errorMessage, null, App.ShowMessageMode.SHOW, true, null);

                Application.Current.Shutdown();
            }
            else
            {
                string errorMessage = string.Format(@"Необработанная ошибка приложения:" + Environment.NewLine + Environment.NewLine + @"{0}" + Environment.NewLine + Environment.NewLine + @"Продолжить ?" + Environment.NewLine + @" (Yes/Да - работа в приложении будет продолжена, No/Нет - работа приложения будет завершена)", e.Exception.Message + (e.Exception.InnerException != null ? Environment.NewLine + e.Exception.InnerException.Message : ""));

                AddLog(errorMessage, e.Exception, App.ShowMessageMode.NONE, true, null);

                if (MessageBox.Show(errorMessage, "Application Error", MessageBoxButton.YesNoCancel, MessageBoxImage.Error) == MessageBoxResult.No)
                {
                    AddLog("Выбрано - заврешить работу приложения", null, App.ShowMessageMode.NONE, true, null);
                    Application.Current.Shutdown();
                }
            }
        }

        /// <summary>
        /// Режим отображения сообщений
        /// </summary>
        public enum ShowMessageMode 
        {
            /// <summary>
            /// Только запись в лог-файл
            /// </summary>
            NONE,
            /// <summary>
            /// Отображение на экране + запись в лог-файл
            /// </summary>
            SHOW
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Запись в лог-файл и вывод информационного сообщения</summary>
        /// <param name="info">Строка для добавления в лог-файл</param>
        /// <param name="ex">Exception с подробностями ошибки</param>
        /// <param name="showMessageMode">SHOW вывести информационное сообщение</param>
        /// <param name="isSaveToLogFile">=true - писать в лог-файл</param>
        /// <param name="logFileName">полный путь к лог-файлу. Если не указан - в App.AppLogFile</param>
        /// <returns>текст сообщения для вывода на экран</returns>
        public static AppLogInfo AddLog(string info, Exception ex, ShowMessageMode showMessageMode, bool isSaveToLogFile, string logFileName)
        {
            if (string.IsNullOrWhiteSpace(info))
            {
                info = "";
            }

            if (string.IsNullOrWhiteSpace(logFileName))
            {
                logFileName = App.AppLogFile;
            }

            var result = GetFullExceptionMessage(ex);

            result.fileMessage = info + result.fileMessage;

            if (!string.IsNullOrWhiteSpace(result.fileMessage) && isSaveToLogFile)
            {
                try
                {
                    File.AppendAllText(logFileName, "\n" + DateTime.Now.ToString("G", System.Globalization.CultureInfo.CreateSpecificCulture("de-DE")) + " " + result.fileMessage);
                }
                catch (Exception ex_f)
                {
                    MessageBox.Show(App.GetFullExceptionMessage(ex_f).showMessage, "ВНИМАНИЕ!");
                }
            }

            if (!string.IsNullOrWhiteSpace(info))
            {
                info += "\n";
            }

            if (!string.IsNullOrWhiteSpace(result.showMessage))
            {
                result.showMessage += "\n";
            }

            result.showMessage = info + result.showMessage;

            if ((showMessageMode == ShowMessageMode.SHOW) && (!string.IsNullOrWhiteSpace(result.showMessage)))
            {
                MessageBox.Show(result.showMessage, "ВНИМАНИЕ!");
            }

            return result;
        }

        /// <summary>
        /// сообщение из Exception с подробностями
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns></returns>
        public static AppLogInfo GetFullExceptionMessage(Exception ex)
        {
            AppLogInfo result = new AppLogInfo();
            
            if (ex == null) return result;

            string error = (ex.Message ?? "").Trim();
            string error_trace = "";

            try
            {
                error_trace =
                "\n====== Exception ============" + //-V3022
                "\n------ Source ---------------" +
                "\n" + (ex.Source ?? "") +
                "\n------ StackTrace -----------" +
                "\n" + (ex.StackTrace ?? "");

                if (ex.TargetSite != null)
                {
                    error_trace +=
                    "\n------ TargetSite -----------" +
                    "\n" + ex.TargetSite +
                    "\n=============================";
                }
            }
            catch
            {
            }

            string error1 = "";
            string error1_trace = "";
            var ex1 = ex.InnerException;

            try
            {
                if (
                    (ex1 != null) &&
                    (!string.IsNullOrWhiteSpace(ex1.Message))
                )
                {
                    error1 = ex1.Message.Trim();

                    error1_trace =
                        "\n====== Inner Exception ======" +
                        "\n------ Source ---------------" +
                        "\n" + (ex1.Source ?? "") +
                        "\n------ StackTrace -----------" +
                        "\n" + (ex1.StackTrace ?? "");

                    if (ex1.TargetSite != null)
                    {
                        error1_trace +=
                        "\n------ TargetSite -----------" +
                        "\n" + ex1.TargetSite +
                        "\n=============================";
                    }
                }
            }
            catch
            {
            }

            string error2 = "";
            string error2_trace = "";
            var ex2 = ex.GetBaseException();

            try
            {
                if (
                   (ex2 != null) &&
                    (!string.IsNullOrWhiteSpace(ex2.Message))
                )
                {
                    error2 = ex2.Message.Trim();

                    error2_trace =
                        "\n====== Base Exception =======" +
                        "\n------ Source ---------------" +
                        "\n" + (ex2.Source ?? "") +
                        "\n------ StackTrace -----------" +
                        "\n" + (ex2.StackTrace ?? "");

                    if (ex2.TargetSite != null)
                    {
                        error2_trace +=
                        "\n------ TargetSite -----------" +
                        "\n" + ex2.TargetSite +
                        "\n=============================";
                    }
                }
            }
            catch
            {
            }

            result.showMessage = error;
            result.fileMessage = error + error_trace;

            if (error != error1)
            {
                result.showMessage += "\n" + error1;
                result.fileMessage += "\n" + error1 + error1_trace;
            }

            if ((error != error2) && (error1 != error2))
            {
                result.showMessage += "\n" + error2;
                result.fileMessage += "\n" + error2 + error2_trace;
            }

            return result;
        }
    }

    /// <summary>
    /// Сообщение
    /// </summary>
    public class AppLogInfo
    {
        /// <summary>
        /// Сообщение для вывода на экран
        /// </summary>
        public string showMessage { get; set; } = "";
        /// <summary>
        /// Сообщение для записи в файл
        /// </summary>
        public string fileMessage { get; set; } = "";
    }
}
