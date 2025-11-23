// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows;
using System.Windows.Media;
using System.Linq;
using System.Collections.Generic;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// цвет ячейки с yml-файлом, вариант 1
    /// </summary>
    public class ValueColorConverter_YMLFile : IValueConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var str = value as string;
            if (str == null) return DependencyProperty.UnsetValue;

            Regex yellow_ymlname = new Regex(@"^(promedweb|rpms|smp|bip)-(\d+)\S(\w+)(.yml|.json)\z");
            Regex green_ymlname = new Regex(@"^(promedweb|rpms|smp|bip)-(\d+)(.yml|.json)\z");

            if (!green_ymlname.IsMatch(str.ToLower()))
            {
                if (!yellow_ymlname.IsMatch(str.ToLower())) return Brushes.Red;
                else return Brushes.DarkOrange;
            }
            else return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// цвет ячейки с yml-файлом, вариант 2
    /// </summary>
    public class ValueColorConverter_YMLFile2 : IValueConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string))
                return DependencyProperty.UnsetValue;
            else
                return Brushes.DarkOrange;
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// цвет ячейки для нераспознанного проекта 
    /// </summary>
    public class ValueColorConverter_Unknown : IValueConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string))
                return DependencyProperty.UnsetValue;
            else
                return Brushes.Red;
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// цвет ячейки с Nпп
    /// </summary>
    public class ValueColorConverter_YMLOrder : IValueConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (string.IsNullOrWhiteSpace(value as string))
                return DependencyProperty.UnsetValue;
            else
                return Brushes.DarkSeaGreen;
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// цвет ячейки "merged?"
    /// </summary>
    public class ValueColorConverter_Merged : IValueConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (!string.IsNullOrWhiteSpace(value as string))
                return DependencyProperty.UnsetValue;
            else
                return Brushes.Red;
        }
        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// цвет ячейки "Дата комита"
    /// </summary>
    public class ValueColorConverter_LastCommit : IValueConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string s = (value as string) ?? "";

            if (s.StartsWith("! "))
                return Brushes.DarkOrange;
            else
                return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// ConvertBack
        /// </summary>
        /// <param name="value">value</param>
        /// <param name="targetType">targetType</param>
        /// <param name="parameter">parameter</param>
        /// <param name="culture">culture</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // =========================================================================================================
    /// <summary>
    /// Окно Формирование релизной версии
    /// </summary>
    public partial class WinRelease
    {
        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Перебор задач в отдельном потоке и выполнение над ними действий
        /// </summary>
        /// <param name="_action_name">описание действия над задачей</param>
        /// <param name="ListYMLFiles">список задач</param>
        /// <param name="_action_before_all">применить действие _action_before_all перед всеми задачами</param>
        /// <param name="_action_task">применить действие _action_task к каждой задаче</param>
        /// <param name="_action_after_task">применить действие _action_after_task после _action_task к каждой задаче</param>
        /// <param name="_action_after_all">применить действие _action_after поcле проверки всех задач</param>
        /// <param name="_action_finish">применить финишное действие _action_finish при любом результате</param>
        public async System.Threading.Tasks.Task TaskListAction(string _action_name, List<YMLFileInfo> ListYMLFiles,
            System.Action<WinRelease> _action_before_all,
            System.Action<YMLFileInfo> _action_task,
            System.Action<YMLFileInfo> _action_after_task,
            System.Action<WinRelease> _action_after_all,
            System.Action<WinRelease> _action_finish
        )
        {
            // проверки
            if (
                (ListYMLFiles == null) ||
                (ListYMLFiles.Count == 0)
            )
            {
                // выполнить финишное действие
                if (_action_finish != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _action_finish(this);
                    });
                }

                // курсор нормальный
                Cursor = System.Windows.Input.Cursors.Arrow;

                return;
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                // курсор в ожидание
                Cursor = System.Windows.Input.Cursors.Wait;

                // выполнить действие перед всеми задачами
                if (_action_before_all != null)
                {
                    _action_before_all(this);
                }
            });

            foreach (var item in ListYMLFiles)
            {
                try
                {
                    // выполнить действие с экземпляром задачи
                    if (_action_task != null)
                    {
                        await System.Threading.Tasks.Task.Run(() =>
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _action_task(item);
                            })
                         );
                    }

                    // выполнить действие с экземпляром задачи
                    if (_action_after_task != null)
                    {
                        await System.Threading.Tasks.Task.Run(() =>
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _action_after_task(item);
                            })
                         );
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFileRelease);

                    if (System.Windows.Forms.MessageBox.Show($"Завершить {_action_name}?",
                        "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        App.AddLog($"Выбрано - завершить {_action_name}", null, App.ShowMessageMode.NONE, true, logFileRelease);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // выполнить финишное действие
                            if (_action_finish != null)
                            {
                                _action_finish(this);
                            }

                            // курсор нормальный
                            Cursor = System.Windows.Input.Cursors.Arrow;
                        });
                        return;
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                // выполнить действие после парсинга задач
                if (_action_after_all != null)
                {
                    _action_after_all(this);
                }

                // выполнить финишное действие
                if (_action_finish != null)
                {
                    _action_finish(this);
                }

                // курсор нормальный
                Cursor = System.Windows.Input.Cursors.Arrow;
            });
        }
        */

        /*
        /// <summary>
        /// Проверка YML и простановка комментария
        /// </summary>
        /// <param name="YMLField">поле на форме "Сборка релиза"</param>
        /// <param name="YMLFile">yml-файл</param>
        /// <param name="newlist">список yml-файлов с информацией из Jira</param>
        /// <param name="YMLFile_Comment">комментарий</param>
        /// <param name="ListVersions">список версий</param>
        /// <returns></returns>
        public string CheckYMLSetComment(string YMLField, string YMLFile, List<YMLFileInfo> newlist, string YMLFile_Comment, ref List<YMLText> ListVersions)
        {
            string result = YMLFile_Comment;

            string project = Utilities.GITProjects.GetProjectByYMLField(YMLField);
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            if (ListVersions == null) ListVersions = new List<YMLText>();

            if (File.Exists(Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "task", YMLFile)))
            {
                // загружаем yml-файл
                YMLStruct loadyml = new YMLStruct(null, logFileRelease);
                loadyml.LoadYML(project, "task", YMLFile, false, null, true, false);

                // проверяем yml-файл
                string Errors = "";
                bool isError = false;
                loadyml.CheckYML(false, isCorrectEncoding.IsChecked == true, isCheckBOM.IsChecked == true, newlist, ref Errors, ref isError, true, true, ref ListVersions, isImproveSQLinVersionRelease);

                if (isError) result += Environment.NewLine + Errors;
            }

            return result;
        }
        */

        /*
        /// <summary>
        /// Нажата кнопка Продолжить Merge
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btMergeTaskNext_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            MergeTaskAll(false);
        }
        */

        /*
        /// <summary>
        /// Проверка на наличие файла версии и на наличие yml-файла задачи в файле версии
        /// </summary>
        /// <param name="ymlfile">yml-файл задачи</param>
        /// <returns></returns>
        private bool CheckYMLFileInVer(string ymlfile = "")
        {
            string project_dev = cbGITProject.SelectedItem.ToString().Trim();
            string DEVProjectFolder = Utilities.GITProjects.GetFolderByProject(project_dev);

            string file_ver = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, "version", tbFileVersion.Text.Trim());
            if (!File.Exists(file_ver))
            {
                App.AddLog($"Файл версии {file_ver} НЕ собран!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                return false;
            }

            // загружаем yml-файл версии
            YMLStruct releaseYML = new YMLStruct(null, logFileRelease);
            releaseYML.LoadYML(project_dev, "version", tbFileVersion.Text.Trim(), false, null, true, false);

            // проверяем наличие задач в файле верси
            if (!string.IsNullOrWhiteSpace(ymlfile))
            {
                // если в параметрах передано имя yml-файла задачи
                if (!releaseYML.ContainsYML(ymlfile))
                {
                    App.AddLog($"{ymlfile} отсутствует в файле версии {file_ver} !", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                    return false;
                }
            }
            else
            {
                // ищем yml-файл задачи в результатах парсинга из Jira, с учетом включения в версию
                foreach (var info in MainWindow.Task.ReleaseYMLFiles
                .Where(x => x.IsAddRelease == true && x.PathInGIT.ToLower() == "task")
                .OrderBy(x => x.YMLOrder)
                )
                {
                    string ymlfield_dev = Utilities.GITProjects.GetYMLFieldByProject(project_dev);
                    string file_dev = info.GetYMLFile(ymlfield_dev);

                    if (
                        (!string.IsNullOrWhiteSpace(file_dev)) &&
                        (!releaseYML.ContainsYML(file_dev))
                    )
                    {
                        if (System.Windows.Forms.MessageBox.Show($"{file_dev} отсутствует в файле версии {file_ver}\nПрекратить выполнение ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            App.AddLog($"{file_dev} отсутствует в файле версии {file_ver} !", null, App.ShowMessageMode.NONE, true, logFileRelease);
                            return false;
                        }
                    }
                }

            }
            return true;
        }
        */

        /*
        /// <summary>
        /// Выполнить transfer.sh для одной задачи
        /// </summary>
        /// <param name="isTransfered"></param>
        /// <param name="info"></param>
        /// <param name="project_dev"></param>
        /// <param name="TrasferedYML"></param>
        private void TransferTask(ref bool isTransfered, YMLFileInfo info, string project_dev, ref string TrasferedYML)
        {
            if (string.IsNullOrWhiteSpace(TrasferedYML)) TrasferedYML = "";

            string DEVProjectFolder = Utilities.GITProjects.GetFolderByProject(project_dev);

            // поле с именем копируемого yml-файла задачи в нужном проекте
            string ymlfield_dev = Utilities.GITProjects.GetYMLFieldByProject(project_dev);

            // имя копируемого yml-файла задачи
            string file_dev = info.GetYMLFile(ymlfield_dev);

            // если поле пустое - выходим
            if (string.IsNullOrWhiteSpace(file_dev)) return;

            // проверяем наличие файла версии и задачи в файле версии
            if (!CheckYMLFileInVer(file_dev)) return;

            // читаем структуру yml-файла задачи
            YMLStruct taskYML = new YMLStruct(null, logFileRelease);
            taskYML.LoadYML(project_dev, "task", file_dev, false, null, true, false);

            // список для transfer.sh
            List<string> ListTask = new List<string>();

            {
                // выделяем имя файла без расширения и в верхнем регистре - для передачи в параметрах transfer.sh
                string task_dev = file_dev.Split('.')[0].ToUpper(); //-V3095

                // добавим в список
                ListTask.Add(task_dev);
                TrasferedYML = file_dev;
            }

            // проверяем, есть ли в копируемой задаче скрипты по изменению таблиц\хранимок\вьюх
            if (taskYML.Lines.Where(x => x.type == YMLLineType.SQLSTRUCT).Count() == 0)
            {
                // в копируемой задаче нет скриптов по изменению таблиц\хранимок\вьюх, запускаем transfer.sh только для данной задачи
            }
            else
            {
                // в копируемой задаче есть скрипты по изменению таблиц\хранимок\вьюх, ищем задачи с такими же скриптами

                // загружаем yml-файл версии
                YMLStruct releaseYML = new YMLStruct(null, logFileRelease);
                releaseYML.LoadYML(project_dev, "version", tbFileVersion.Text.Trim(), false, null, true, false);

                // перебираем задачи версии, кроме копируемой задачи
                foreach (var task in releaseYML.Lines.Where(x => x.type == YMLLineType.TASK && x.file.ToLower() != file_dev.ToLower()))
                {
                    if (
                        task.isLoaded &&
                        (task.loadYMLStruct != null) &&
                        (!string.IsNullOrWhiteSpace(task.file))
                    )
                    {
                        // перебираем скрипты копируемой задачи по изменению таблиц\хранимок\вьюх
                        foreach (var item in taskYML.Lines.Where(x => x.type == YMLLineType.SQLSTRUCT))
                        {
                            if (task.loadYMLStruct.ContainsSQL(item.search))
                            {
                                // выделяем имя файла без расширения и в верхнем регистре - для передачи в параметрах transfer.sh
                                string task_dev = task.file.Split('.')[0].ToUpper(); //-V3095

                                // есть другая задача с аналогичным скриптом - добавляем ее в список, если она еще не добавлена
                                if (!ListTask.Contains(task_dev, StringComparer.OrdinalIgnoreCase))
                                {
                                    ListTask.Add(task_dev);
                                    TrasferedYML += "\n" + task.file;
                                }
                            }
                        }
                    }

                }
            }

            // вызываем transfer.sh
            if (ListTask.Count > 0) //-V3022
            {
                string project_git = Utilities.GITProjects.GetGITProject(project_dev);
                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                WinExecute WinExecute = new WinExecute(logFileRelease);
                WinExecute.Title = $"Выполнить transfer.sh из проекта {project_dev} в проект {project_git}";

                // формируем список команд
                foreach (var task_dev in ListTask)
                {
                    WinExecute.AddCommand(
                                App.AppPath,
                                Path.Combine(App.AppPath, "git_runsh.cmd"),
                                folder + " transfer.sh " + task_dev
                            );
                }

                // выполняем
                WinExecute.Start(true, -1);

                // Полный путь к копируемому yml-файлу в "старом" проекте
                string GITProjectFolder = Utilities.GITProjects.GetFolderByProject(project_git);
                string fullfile_git = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, "task", file_dev);

                if (File.Exists(fullfile_git))
                {
                    // yml-файл успешно скопирован
                    isTransfered = true;
                }
            }
        }
        */

        /*
        /// <summary>
        /// выполнить transfer.sh
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            if (!CheckMerged()) return;

            string project_dev = cbGITProject.SelectedItem.ToString().Trim();
            string project_git = Utilities.GITProjects.GetGITProject(project_dev);

            // только если "новый" проект
            if (Utilities.GITProjects.IsDEVProject(project_dev))
            {
                if (System.Windows.Forms.MessageBox.Show($"Запустить transfer.sh для копирования скриптов из проекта {project_dev} в проект {project_git} ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    // git pull "старого" проекта
                    GIT.GitPull(new string[] { project_git }, tbBranch.Text.Trim(), false, true, false, logFileRelease);

                    // проверяем наличие файла версии и задач в файле версии
                    if (!CheckYMLFileInVer()) return;

                    // запускаем transfer.sh для всей версии
                    WinExecute WinExecute = new WinExecute(logFileRelease);
                    WinExecute.Title = "Выполнить transfer.sh из проекта " + project_dev + " в проект " + project_git;

                    string DEVProjectFolder = Utilities.GITProjects.GetFolderByProject(project_dev);
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder);
                    if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                    WinExecute.AddCommand(
                                App.AppPath,
                                Path.Combine(App.AppPath, "git_runsh.cmd"),
                                folder + " transfer.sh"
                            );

                    // выполняем
                    WinExecute.Start(true, -1);

                    // заполняем имя файла для "старого" проекта, если еще не заполнено
                    SetYMLField();

                    // сохраняем задачу
                    if (mainWindow != null)
                    {
                        mainWindow.SaveTaskNoShow();
                        btSaveTask.Focus();
                    }
                }
            }
        }
        */

        /*
        /// <summary>
        /// Запуск transfer.sh для выбранной задачи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgYMLFiles_MenuItemTransferSH(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            if (!CheckMerged()) return;

            if (dgYMLFiles.SelectedIndex >= 0)
            {
                var _yml = dgYMLFiles.SelectedItem as YMLFileInfo;

                string project_dev = cbGITProject.SelectedItem.ToString().Trim();
                string project_git = Utilities.GITProjects.GetGITProject(project_dev);

                // только если "новый" проект
                if (Utilities.GITProjects.IsDEVProject(project_dev))
                {
                    // git pull "старого" проекта
                    GIT.GitPull(new string[] { project_git }, tbBranch.Text.Trim(), false, true, false, logFileRelease);

                    // выполняем трансфер одиночной задачи
                    bool isTransfered = false;
                    string TrasferedYML = "";
                    TransferTask(ref isTransfered, _yml, project_dev, ref TrasferedYML);

                    if (!isTransfered)
                    {
                        //App.AddLog($"Переносить из проекта {project_dev} в проект {project_git} ничего дополнительно не требуется!", null, App.ShowMessageMode.SHOW);
                    }
                    else
                    {
                        App.AddLog($"Выполнен перенос из проекта {project_dev} в проект {project_git} следующих yml-файлов задач:\n{TrasferedYML}\n\nВНИМАНИЕ: Не забудьте сделать add + commit в проекте {project_git} для скопированных файлов!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                    }

                    // заполняем имя файла для "старого" проекта, если еще не заполнено
                    SetYMLField();

                    // сохраняем задачу
                    if (mainWindow != null)
                    {
                        mainWindow.SaveTaskNoShow();
                        btSaveTask.Focus();
                    }
                }
            }
        }
        */
    }
}
