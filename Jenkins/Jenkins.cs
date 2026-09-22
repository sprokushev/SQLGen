// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.VisualStudio.VCProjectEngine;
using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
            dlg1.Text = "Введите логин и пароль для Jenkins";

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

            // выполним по порядку
            foreach (JenkinsJob job in jobs
                .Where(x => !x.isExecuted) // только те, что еще не выполнены
                .OrderBy(x => x.Order)
            )
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
                if (job.ExecutionMode == true) _mode = "true";

                WinExecute.AddCommand(
                    App.AppPath,
                    "java",
                    $"-jar jenkins-cli.jar -s https://jenkins-dev.dev.k8s.rtmis.ru/ -auth {MainWindow.APPinfo.UsernameJenkins}:{MainWindow.APPinfo.PasswordJenkins} build \"{job.JobName}\" -s -p ALIASChoice=\"{job.AliasName}\" -p EnvPathFile=\"{job.FileName}\" -p EnvRepobranch=\"{job.Branch}\" -p ExecutionMode=\"{_mode}\"",
                    $"java -jar jenkins-cli.jar -s https://jenkins-dev.dev.k8s.rtmis.ru/\n-auth %USERNAME%:%PASSWORD%\nbuild \"{job.JobName}\" -s\n-p ALIASChoice=\"{job.AliasName}\"\n-p EnvPathFile=\"{job.FileName}\"\n-p EnvRepobranch=\"{job.Branch}\"\n-p ExecutionMode=\"{_mode}\"",
                    job.Order
                );
            }

            bool result;

            if (WinExecute.ListCommands.Count > 0)
            {
                App.AddLog($"Сейчас выполним задания Jenkins: {JsonSerializer.Serialize(jobs)}", null, App.ShowMessageMode.NONE, true, logFile);

                WinExecute.Start(true);

                result = WinExecute.LastExitCode == 0;
                
                // зафиксируем результат выполнения заданий
                foreach (var numCommand in WinExecute.ListNumSuccess)
                {
                    var found = jobs.FirstOrDefault(x => x.Order == numCommand);
                    if (found != null)
                    {
                        found.isExecuted = true;
                    }
                }
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
    public class JenkinsJob : INotifyPropertyChanged
    {
        /// <summary>
        /// реализация OnPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// реализация OnPropertyChanged
        /// </summary>
        /// <param name="prop">prop</param>
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop)); //-V3083
        }

        /// <summary>
        /// Флаг для фильтрации
        /// </summary>
        public bool isFiltered { get; set; } = true;

        int _order = 0;
        /// <summary>
        /// Номер по порядку
        /// </summary>
        public int Order
        {
            get => _order;
            set
            {
                if (_order == value) return; // Защита от лишних срабатываний

                _order = value;

                OnPropertyChanged();
            }
        }

        string _jobname = "";
        /// <summary>
        /// Название задания Jenkins
        /// </summary>
        public string JobName
        {
            get => _jobname ?? "";
            set
            {
                if (_jobname == value) return; // Защита от лишних срабатываний

                _jobname = value;

                if (!string.IsNullOrWhiteSpace(_jobname))
                {
                    _jobname = _jobname.TrimAllSpace();
                }
                else
                {
                    _jobname = "";
                }

                OnPropertyChanged();
            }
        }

        string _alias = "";
        /// <summary>
        /// Алиас БД
        /// </summary>
        public string AliasName 
        {
            get => _alias ?? "";
            set
            {
                if (_alias == value) return; // Защита от лишних срабатываний

                _alias = value;

                if (!string.IsNullOrWhiteSpace(_alias))
                {
                    _alias = _alias.TrimAllSpace();
                }
                else
                {
                    _alias = "";
                }

                Project = Utilities.GITProjects.GetProjectByLuquibotAlias(_alias);

                OnPropertyChanged();
            }
        }

        string _filename = "";
        /// <summary>
        /// Скрипт в формате "path/filename.ext", path - это путь в проекте GIT от корня проекта
        /// </summary>
        public string FileName
        {
            get => _filename ?? "";
            set
            {
                if (_filename == value) return; // Защита от лишних срабатываний

                _filename = value;

                if (!string.IsNullOrWhiteSpace(_filename))
                {
                    _filename = _filename.TrimAllSpace();
                }
                else
                {
                    _filename = "";
                }

                OnPropertyChanged();
            }
        }

        string _branch = "";
        /// <summary>
        /// Ветка в проекте GIT
        /// </summary>
        public string Branch
        {
            get => _branch ?? "";
            set
            {
                if (_branch == value) return; // Защита от лишних срабатываний

                _branch = value;

                if (!string.IsNullOrWhiteSpace(_branch))
                {
                    _branch = _branch.TrimAllSpace();
                }
                else
                {
                    _branch = "";
                }

                OnPropertyChanged();
            }
        }

        private bool _execmode;
        /// <summary>
        /// режим выполнения: true - выполнить, false - имитация выполнения
        /// </summary>
        public bool ExecutionMode
        {
            get => _execmode;
            set
            {
                if (_execmode == value) return; // Защита от лишних срабатываний

                _execmode = (value == true);

                OnPropertyChanged();
            }
        }

        private string _project = "";
        /// <summary>
        /// Проект GIT
        /// </summary>
        public string Project
        {
            get => _project ?? "";
            private set
            {
                if (_project == value) return; // Защита от лишних срабатываний

                _project = value;

                if (!string.IsNullOrWhiteSpace(_project))
                {
                    _project = _project.TrimAllSpace();
                }
                else
                {
                    _project = "";
                }

                OnPropertyChanged();
            }
        }

        private bool _isexecuted;
        /// <summary>
        /// результат выполнения
        /// </summary>
        public bool isExecuted
        {
            get => _isexecuted;
            set
            {
                if (_isexecuted == value) return; // Защита от лишних срабатываний

                _isexecuted = (value == true);

                OnPropertyChanged();
            }
        }

        string _version = "";
        /// <summary>
        /// Версия, для которой создано задание
        /// </summary>
        public string Version
        {
            get => _version ?? "";
            set
            {
                if (_version == value) return; // Защита от лишних срабатываний

                _version = value;

                if (!string.IsNullOrWhiteSpace(_version))
                {
                    _version = _version.TrimAllSpace();
                }
                else
                {
                    _version = "";
                }

                OnPropertyChanged();
            }
        }

        string _stand = "";
        /// <summary>
        /// Стенд, для которого создано задание
        /// </summary>
        public string Stand
        {
            get => _stand ?? "";
            set
            {
                if (_stand == value) return; // Защита от лишних срабатываний

                _stand = value;

                if (!string.IsNullOrWhiteSpace(_stand))
                {
                    _stand = _stand.TrimAllSpace();
                }
                else
                {
                    _stand = "";
                }

                OnPropertyChanged();
            }
        }
    }
}
