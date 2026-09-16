// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>Класс для работы c Jenkins CLI</summary>
    public class JenkinsCLI
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Окно подключения к Jenkins</summary>
        /// <param name="_logfile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static bool OpenLoginJenkins(string _logfile)
        {
            FormLoginJira dlg1 = new FormLoginJira(_logfile);

            dlg1.tbUsername.Text = MainWindow.APPinfo.UsernameJenkins;
            dlg1.tbPassword.Text = MainWindow.APPinfo.PasswordJenkins;
            dlg1.cbSavePassword.Checked = MainWindow.APPinfo.isSavePasswordJenkins;

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MainWindow.APPinfo.UsernameJenkins = dlg1.tbUsername.Text;
                MainWindow.APPinfo.PasswordJenkins = dlg1.tbPassword.Text;
                MainWindow.APPinfo.isSavePasswordJenkins = dlg1.cbSavePassword.Checked == true;

                dlg1.Dispose();
                return true;
            }

            dlg1.Dispose();
            return false;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выполнить последовательно список заданий Jenkins
        /// </summary>
        /// <param name="jobs">Список заданий</param>
        /// <param name="isShowLogAfterFinish">=true - отображать лог после завершения</param>
        /// <param name="logFile">лог-файл</param>
        /// <returns>=true - выполнился успешно</returns>
        public static bool Execute(List<JenkinsJob> jobs, bool isShowLogAfterFinish, string logFile)
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            if (
                jobs == null ||
                jobs.Count == 0
            )
            {
                App.AddLog($"Нет заданий Jenkins для выполнения!", null, App.ShowMessageMode.NONE, true, logFile);
                return true;
            }

            // запросить логин/пароль
            if (!OpenLoginJenkins(logFile))
            {
                return false;
            }

            WinExecute WinExecute = new WinExecute(logFile);
            WinExecute.Title = "Выполняем задания Jenkins";
            WinExecute.isShowAllErrors = true;
            WinExecute.isStopAfterFirstError = true;
            WinExecute.isShowLogAfterError = isShowLogAfterFinish;
            WinExecute.isShowLogAfterSuccess = isShowLogAfterFinish;
            WinExecute.OutputEncoding = Encoding.UTF8; //Encoding.GetEncoding(866);

            foreach (JenkinsJob job in jobs)
            {
                // проверка
                if (string.IsNullOrWhiteSpace(job.JobName))
                {
                    App.AddLog($"Попытка запустить задание Jenkins без указания его названия!", null, App.ShowMessageMode.SHOW, true, logFile);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(job.AliasName))
                {
                    App.AddLog($"Попытка запустить задание Jenkins {job.JobName} без указания алиаса!", null, App.ShowMessageMode.SHOW, true, logFile);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(job.FileName))
                {
                    App.AddLog($"Попытка запустить задание Jenkins {job.JobName} алиас {job.AliasName} без указания имени файла скрипта!", null, App.ShowMessageMode.SHOW, true, logFile);
                    return false;
                }

                if (string.IsNullOrWhiteSpace(job.Branch))
                {
                    App.AddLog($"Попытка запустить задание Jenkins {job.JobName} алиас {job.AliasName} скрипт {job.FileName} без указания ветки!", null, App.ShowMessageMode.SHOW, true, logFile);
                    return false;
                }

                string _mode = "false";
                if (job.ExecutionMode) _mode = "true";

                WinExecute.AddCommand(
                    App.AppPath,
                    "java",
                    $"-jar jenkins-cli.jar -s https://jenkins-dev.dev.k8s.rtmis.ru/ -auth {MainWindow.APPinfo.UsernameJenkins}:{MainWindow.APPinfo.PasswordJenkins} build \"{job.JobName}\" -s -p ALIASChoice=\"{job.AliasName}\" -p EnvPathFile=\"{job.FileName}\" -p EnvRepobranch=\"{job.Branch}\" -p ExecutionMode=\"{_mode}\"",
                    $"java -jar jenkins-cli.jar -s https://jenkins-dev.dev.k8s.rtmis.ru/\n-auth %USERNAME%:%PASSWORD%\nbuild \"{job.JobName}\" -s\n-p ALIASChoice=\"{job.AliasName}\"\n-p EnvPathFile=\"{job.FileName}\"\n-p EnvRepobranch=\"{job.Branch}\"\n-p ExecutionMode=\"{_mode}\""
                );
            }

            bool result;

            if (WinExecute.ListCommands.Count > 0)
            {
                App.AddLog($"Сейчас выполним задания Jenkins: {JsonSerializer.Serialize(jobs)}", null, App.ShowMessageMode.NONE, true, logFile);

                WinExecute.Start(true);

                result = WinExecute.LastExitCode == 0;
            }
            else
            {
                WinExecute.Close();

                result = true;
            }

            return result;
        }
    }

    /// <summary>
    /// Задание для Jenkins
    /// </summary>
    public class JenkinsJob
    {
        /// <summary>
        /// Название задания Jenkins
        /// </summary>
        public string JobName { get; set; }

        /// <summary>
        /// Алиас БД
        /// </summary>
        public string AliasName { get; set; }

        /// <summary>
        /// Скрипт в формате "path/filename.ext", path - это путь в проекте GIT от корня проекта
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Ветка в проекте GIT
        /// </summary>
        public string Branch { get; set; }

        /// <summary>
        /// режим выполнения: true - выполнить, false - имитация выполнения
        /// </summary>
        public bool ExecutionMode { get; set; } = false;
    }
}
