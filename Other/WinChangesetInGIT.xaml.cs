// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using SQLGen.Controls;
using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace SQLGen
{
    /// <summary>
    /// Окно Изменить файлы в проекте GIT
    /// </summary>
    public partial class WinChangesetInGIT : Window
    {
        /// <summary>ссылка на экземпляр основного окна программы</summary>
        public MainWindow mainWindow;

        /// <summary>
        /// признак выполнения через командную строку
        /// </summary>
        private bool isAutoChange = false;

        /// <summary>
        /// фоновый воркер
        /// </summary>
        private BackgroundWorker backgroundWorker1;
        private int highestPercentageReached = 0;

        /// <summary>
        /// флаг выполнения git pull
        /// </summary>
        bool isRefreshed = false;

        /// <summary>Конструктор WinChangesetInGIT</summary>
        public WinChangesetInGIT()
        {
            InitializeComponent();

            rbSearchStartsWith.IsChecked = true;
            rbSearchContains_Unchecked(null, null);

            // загружаем данные предыдущего сеанса
            if (
                (MainWindow.Task != null) &&
                (!string.IsNullOrWhiteSpace(MainWindow.Task.TaskNumber)) &&
                (!string.IsNullOrWhiteSpace(MainWindow.Task.OtherJsonInfo))
            )
            {
                try
                {
                    var param = JsonSerializer.Deserialize<ChangeParam>(MainWindow.Task.OtherJsonInfo, Other.oldOptionsJSON);

                    tbMask.Text = Path.Combine(param.pathMask, param.fileMask);
                    tbExclude.Text = param.excludePath;

                    switch (param.searchAction)
                    {
                        case SearchAction.StartsWith:
                            rbSearchStartsWith.IsChecked = true;
                            break;
                        case SearchAction.Contains:
                            rbSearchContains.IsChecked = true;
                            break;
                        case SearchAction.EndsWith:
                            rbSearchEndsWith.IsChecked = true;
                            break;
                        default:
                            break;
                    }

                    switch (param.replaceAction)
                    {
                        case ReplaceAction.ReplaceAll:
                            rbReplaceAll.IsChecked = true;
                            break;
                        case ReplaceAction.Replace:
                            rbReplace.IsChecked = true;
                            break;
                        case ReplaceAction.ReplaceAddStart:
                            rbReplaceAddStart.IsChecked = true;
                            break;
                        case ReplaceAction.ReplaceAddEnd:
                            rbReplaceAddEnd.IsChecked = true;
                            break;
                        case ReplaceAction.InsertBefore:
                            rbInsertBefore.IsChecked = true;
                            break;
                        case ReplaceAction.InsertAfter:
                            rbInsertAfter.IsChecked = true;
                            break;
                        case ReplaceAction.Delete:
                            rbDelete.IsChecked = true;
                            break;
                        default:
                            break;
                    }

                    tbSearch.Text = param.searchText;
                    tbReplace.Text = param.replaceText;
                    cbCaseSensitivity.IsChecked = param.isCaseSensitivity == true;
                    tbFileContains.Text = param.fileContains;
                    tbFileNotContains.Text = param.fileNotContains;
                    cbRegex.IsChecked = param.isRegex == true;
                }
                catch (Exception)
                {
                }
            }

            // инициализация фонового воркера
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(ChangeFiles);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);

            // пользовательские настройки GUI
            Default.InitGUI("WinChangesetInGIT", this, mainGrid, null, null, null, MainWindow.Task.LogFile);
        }

        /// <summary>При открытии окна WinChangesetInGIT</summary>
        private void winChangesetInGIT_Activated(object sender, EventArgs e)
        {
            this.Title = "Заменить строку в файлах проекта GIT";

            tbMask.Focus();
        }

        /// <summary>При закрытии окна WinChangesetInGIT</summary>
        private void winChangesetInGIT_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinChangesetInGIT", this, null);
        }

        // выполнить git pull, если он еще не сделан
        private void GitPull()
        {
            if (
                (!isRefreshed) &&
                (cbGITProject != null) &&
                (cbGITProject.SelectedIndex != -1)
                )
            {
                string project = "";
                ComboBoxItem cbItem = null;
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem; //-V3022
                if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                else project = cbItem.Tag.ToString();

                string err = "";
                string branch = GIT.GitCurrentBranch(project, out err, MainWindow.Task.LogFile);

                btChangeBranch.Content = branch.Replace("_", "__");
                btChangeBranch.IsEnabled = Utilities.GITProjects.IsDEVProject(project);

                // git pull
                GIT.GitPull(new string[] { project }, branch, false, false, false, MainWindow.Task.LogFile, false);

                isRefreshed = true;
            }
        }


        /// <summary>Меняем файлы с использованием фонового воркера</summary>
        private void ChangeFiles(object sender, DoWorkEventArgs e)
        {
            if (e.Argument == null) return;

            BackgroundWorker worker = sender as BackgroundWorker;

            ChangeParam param = (ChangeParam)e.Argument;

            bool isError = false;
            //bool isFirst = true;
            string error = "";
            //string path = param.Folder;
            int maximum = param.allObjects.Count();
            int current = 0;

            bool isCheckFileContains = !string.IsNullOrWhiteSpace(param.fileContains);
            bool isFoundFileContains = false;
            bool isCheckFileNotContains = !string.IsNullOrWhiteSpace(param.fileNotContains);
            bool isFoundFileNotContains = false;

            if (!param.isCaseSensitivity)
            {
                // если выбран поиск НЕ чувствительный к регистру, то переводим все искомые фразы в нижний регистр
                param.fileContains = param.fileContains.ToLower();
                param.fileNotContains = param.fileNotContains.ToLower();
                param.searchText = param.searchText.ToLower();
            }
            string searchPattern = "";

            if (param.isRegex)
            {
                searchPattern = param.searchText;
            }
            else
            {
                searchPattern = param.searchText.Replace("(", @"\(").Replace(")", @"\)");
            }

            worker.ReportProgress(0);

            int countChangedFiles = 0;

            foreach (var fullfilename in param.allObjects)
            {
                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }

                current++;

                long i = -1;

                try
                {
                    i = -1;

                    if (File.Exists(fullfilename))
                    {
                        isFoundFileContains = false;
                        isFoundFileNotContains = false;

                        // читаем файл в буфер
                        string[] buffer = File.ReadAllLines(fullfilename);
                        List<long> deleted = new List<long>();

                        bool isChanged = false;

                        // перебираем строки
                        for (i = 0; i < buffer.LongLength; i++)
                        {
                            string _text = buffer[i];

                            if (!param.isCaseSensitivity)
                            {
                                // если выбран поиск НЕ чувствительный к регистру, то переводим строку из файла в нижний регистр
                                _text = _text.ToLower();
                            }

                            if (
                                isCheckFileContains && // проверяем наличие фразы в файле
                                (!isFoundFileContains) && // еще не находили
                                _text.Contains(param.fileContains) // нашли
                            )
                            {
                                isFoundFileContains = true;
                            }

                            if (
                                isCheckFileContains && // проверяем отсутствие фразы в файле
                                (!isFoundFileNotContains) && // еще не находили
                                _text.Contains(param.fileNotContains) // нашли
                            )
                            {
                                isFoundFileNotContains = true;
                            }

                            long found = -1;

                            // ищем в файле 
                            switch (param.searchAction)
                            {
                                case SearchAction.StartsWith:
                                    if (_text.StartsWith(param.searchText)) found = i;
                                    break;
                                case SearchAction.Contains:
                                    /*
                                    if (_text.Contains(param.searchText))
                                    {
                                        found = i;
                                    }
                                    */
                                    if (Regex.IsMatch(_text, searchPattern)) found = i;
                                    break;
                                case SearchAction.EndsWith:
                                    if (_text.EndsWith(param.searchText)) found = i;
                                    break;
                                default:
                                    break;
                            }

                            //изменяем, если нашли
                            if (found > -1)
                            {
                                switch (param.replaceAction)
                                {
                                    case ReplaceAction.ReplaceAll:
                                        buffer[i] = param.replaceText;
                                        isChanged = true;
                                        break;
                                    case ReplaceAction.Replace:
                                        if (param.isCaseSensitivity)
                                        {
                                            //buffer[i] = buffer[i].Replace(param.searchText, param.replaceText);
                                            buffer[i] = Regex.Replace(buffer[i], searchPattern, param.replaceText);
                                        }   
                                        else
                                        {
                                            buffer[i] = Regex.Replace(buffer[i], searchPattern, param.replaceText, RegexOptions.IgnoreCase);
                                        }
                                        isChanged = true;
                                        break;
                                    case ReplaceAction.ReplaceAddStart:
                                        buffer[i] = param.replaceText + buffer[i];
                                        isChanged = true;
                                        break;
                                    case ReplaceAction.ReplaceAddEnd:
                                        buffer[i] = buffer[i] + param.replaceText;
                                        isChanged = true;
                                        break;
                                    case ReplaceAction.InsertBefore:
                                        buffer[i] = param.replaceText + "\n" + buffer[i];
                                        isChanged = true;
                                        break;
                                    case ReplaceAction.InsertAfter:
                                        buffer[i] = buffer[i] + "\n" + param.replaceText;
                                        isChanged = true;
                                        break;
                                    case ReplaceAction.Delete:
                                        deleted.Add(i);
                                        isChanged = true;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        i = -1;

                        if (isCheckFileContains && (!isFoundFileContains)) 
                        {
                            // если "нужная" фраза в файле НЕ найдена - переходим к следующему файлу
                        }
                        else if (isCheckFileNotContains && isFoundFileNotContains)
                        {
                            // если "запретная" фраза в файле найдена - переходим к следующему файлу
                        }
                        else 
                        {
                            // сохраняем файл
                            if (isChanged)
                            {
                                string text = "";
                                if (
                                    (param.replaceAction == ReplaceAction.Delete) &&
                                    (deleted.Count > 0)
                                    )
                                {
                                    // исключаем удаленные строки
                                    string[] newbuffer = new string[buffer.Length - deleted.Count];
                                    long cnt = -1;
                                    for (long j = 0; j < buffer.Length; j++)
                                    {
                                        if (!deleted.Contains(j))
                                        {
                                            cnt++;
                                            newbuffer[cnt] = buffer[j];
                                        }
                                    }
                                    text = string.Join("\n", newbuffer) + "\n";
                                }
                                else
                                {
                                    text = string.Join("\n", buffer) + "\n";
                                }
                                File.WriteAllText(fullfilename, text);
                                countChangedFiles++;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    error += Environment.NewLine + fullfilename + ": ";
                    if (i != -1)
                    {
                        error += $"строка {i} - ";
                    }
                    error += ex.Message;
                    isError = true;
                }

                int percentComplete =
                                    (int)((float)current / (float)maximum * 100);
                if (percentComplete > highestPercentageReached)
                {
                    highestPercentageReached = percentComplete;
                    worker.ReportProgress(percentComplete);
                }
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (!isAutoChange)
                {
                    if (countChangedFiles > 0)
                    {
                        App.AddLog($"Изменено {countChangedFiles} файлов", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                    else
                    {
                        App.AddLog($"Ни одного файла не изменено !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                }

                if (isError)
                {
                    string logfileinfo = "";
                    if (
                        (!string.IsNullOrWhiteSpace(MainWindow.Task.TaskNumber)) &&
                        (!string.IsNullOrWhiteSpace(MainWindow.Task.LogFile))
                    )
                    {
                        try
                        {
                            logfileinfo = " в процессе выгрузки - записан в " + MainWindow.Task.LogFile;

                            File.AppendAllText(MainWindow.Task.LogFile, Environment.NewLine +
                                DateTime.Now.ToString("G") + Environment.NewLine +
                                error +
                                "-------------------------------------------------------------------------------------------" + Environment.NewLine);
                        }
                        catch (Exception ex)
                        {
                            logfileinfo = "";
                            App.AddLog("", ex, App.ShowMessageMode.SHOW, false, null);
                        }
                    }

                    WinInfo WinInfo = new WinInfo(null);
                    WinInfo.Title = "Список ошибок и предупреждений" + logfileinfo;
                    WinInfo.tbInfo.Text = error;
                    WinInfo.Show();
                }

                if (isAutoChange)
                {
                    Application.Current.Shutdown();
                }
            });
        }

        private void ChangeClick()
        {
            if (rbSearchStartsWith.IsChecked == false && rbSearchEndsWith.IsChecked == false && rbSearchContains.IsChecked == false)
            {
                MessageBox.Show("Выберите тип поиска");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbSearch.Text))
            {
                MessageBox.Show("Введите строку поиска");
                return;
            }

            if (
                rbReplaceAll.IsChecked == false && rbReplace.IsChecked == false && rbReplaceAddStart.IsChecked == false && rbReplaceAddEnd.IsChecked == false &&
                rbInsertBefore.IsChecked == false && rbInsertAfter.IsChecked == false && rbDelete.IsChecked == false
                )
            {
                MessageBox.Show("Выберите тип замены");
                return;
            }

            /*if (
                string.IsNullOrWhiteSpace(tbReplace.Text) &&
                rbDelete.IsChecked == false
            )
            {
                MessageBox.Show("Введите текст замены");
                return;
            }*/

            if (
                tbMask.Text.StartsWith("*") ||
                (!tbMask.Text.Contains(":"))
                )
            {
                MessageBox.Show("В маске изменяемых файлов должен быть указан каталог");
                return;
            }

            if (backgroundWorker1.IsBusy == false)
            {
                // блокируем нажатие кнопок на время выполнения выгрузки
                this.btChange.IsEnabled = false;
                this.btCancel.IsEnabled = true;

                GitPull();

                // собираем параметры для фоновой задачи
                ChangeParam param = new ChangeParam();

                if (rbSearchStartsWith.IsChecked == true) param.searchAction = SearchAction.StartsWith;
                if (rbSearchContains.IsChecked == true) param.searchAction = SearchAction.Contains;
                if (rbSearchEndsWith.IsChecked == true) param.searchAction = SearchAction.EndsWith;

                if (rbReplaceAll.IsChecked == true) param.replaceAction = ReplaceAction.ReplaceAll;
                if (rbReplace.IsChecked == true) param.replaceAction = ReplaceAction.Replace;
                if (rbReplaceAddStart.IsChecked == true) param.replaceAction = ReplaceAction.ReplaceAddStart;
                if (rbReplaceAddEnd.IsChecked == true) param.replaceAction = ReplaceAction.ReplaceAddEnd;
                if (rbInsertBefore.IsChecked == true) param.replaceAction = ReplaceAction.InsertBefore;
                if (rbInsertAfter.IsChecked == true) param.replaceAction = ReplaceAction.InsertAfter;
                if (rbDelete.IsChecked == true) param.replaceAction = ReplaceAction.Delete;

                param.searchText = tbSearch.Text;
                param.replaceText = tbReplace.Text;
                param.fileContains = tbFileContains.Text;
                param.fileNotContains = tbFileNotContains.Text;
                param.isCaseSensitivity = cbCaseSensitivity.IsChecked == true;
                param.isRegex = cbRegex.IsChecked == true;

                // формируем список изменяемых файлов
                param.pathMask = Path.GetDirectoryName(tbMask.Text);
                param.fileMask = Path.GetFileName(tbMask.Text);
                param.excludePath = tbExclude.Text;

                // сохраняем задачу
                if (
                    (mainWindow != null) &&
                    (MainWindow.Task != null) &&
                    (!string.IsNullOrWhiteSpace(MainWindow.Task.TaskNumber))
                   )
                {
                    // сохраним для следующего сеанса
                    MainWindow.Task.OtherJsonInfo = JsonSerializer.Serialize<ChangeParam>(param, Other.OptionsJSON);

                    MainWindow.SaveTask(MainWindow.Task, true);
                }

                List<string> list = Utilities.Files.ListFilesInDir(param.pathMask, false, true, false, param.excludePath, true, param.fileMask);
                param.allObjects = list.Distinct().ToList();

                // запускаем фоновую задачу по выгрузке
                highestPercentageReached = 0;
                backgroundWorker1.RunWorkerAsync(param);
            }
        }

        /// <summary>Нажата кнопка Выгрузить для задачи</summary>
        private void btChange_Click(object sender, RoutedEventArgs e)
        {
            if ((cbGITProject == null) || (cbGITProject.SelectedIndex == -1)) return;

            ChangeClick();
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

            this.btChange.IsEnabled = true;
            this.btCancel.IsEnabled = false;
        }

        /// <summary>Фоновая задача отменена</summary>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            // Cancel the asynchronous operation.
            if (backgroundWorker1 != null) backgroundWorker1.CancelAsync();

            this.btCancel.IsEnabled = false;
        }

        private void btFolder_Click(object sender, RoutedEventArgs e)
        {
            // выбранный проект
            ComboBoxItem cbItem = null;
            string GITProject = "";
            if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
            if ((cbItem == null) || (cbItem.Tag == null)) GITProject = "";
            else GITProject = cbItem.Tag.ToString();

            string dir = Controls.Dialogs.FolderBrowserDialog(Path.Combine(MainWindow.APPinfo.GITFolder, GITProject));
            if (!string.IsNullOrWhiteSpace(dir))
            {
                tbMask.Text = dir + "\\*.sql";
            }
        }

        /// <summary>
        /// Выбор проекта GIT
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void cbGITProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cbGITProject != null) && (cbGITProject.SelectedIndex != -1))
            {
                string project = "";
                ComboBoxItem cbItem = null;
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem; //-V3022
                if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                else project = cbItem.Tag.ToString();

                string err = "";
                string branch = GIT.GitCurrentBranch(project, out err, MainWindow.Task.LogFile);

                btChangeBranch.Content = branch.Replace("_", "__");
                btChangeBranch.IsEnabled = Utilities.GITProjects.IsDEVProject(project);

                isRefreshed = false;
            }
            else
            {
                btChangeBranch.Content = "";
                btChangeBranch.IsEnabled = false;
            }
        }

        /// <summary>
        /// Нажата кнопка по смене ветки
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void btChangeBranch_Click(object sender, RoutedEventArgs e)
        {
            if ((cbGITProject != null) && (cbGITProject.SelectedIndex != -1))
            {
                string project = "";
                ComboBoxItem cbItem = null;
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem; //-V3022
                if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                else project = cbItem.Tag.ToString();

                // переключение на выбранную ветку
                if (GIT.SelectGITBranch(project, null, out string branch, MainWindow.Task.LogFile, true, false, ""))
                {
                    isRefreshed = true;

                    // Показать ветку
                    btChangeBranch.Content = branch.Replace("_", "__");
                    btChangeBranch.IsEnabled = Utilities.GITProjects.IsDEVProject(project);
                }
                else
                {
                    cbGITProject_SelectionChanged(null, null);
                }
            }
        }

        private void btGitPull_Click(object sender, RoutedEventArgs e)
        {
            isRefreshed = false;
            GitPull();
        }

        private void rbSearchContains_Checked(object sender, RoutedEventArgs e)
        {
            rbReplace.IsEnabled = true;
        }

        private void rbSearchContains_Unchecked(object sender, RoutedEventArgs e)
        {
            rbReplace.IsChecked = false;
            rbReplace.IsEnabled = false;
        }

        private void rbDelete_Checked(object sender, RoutedEventArgs e)
        {
            tbReplace.IsEnabled = false;
        }

        private void rbDelete_Unchecked(object sender, RoutedEventArgs e)
        {
            tbReplace.IsEnabled = true;
        }

        private void btExclude_Click(object sender, RoutedEventArgs e)
        {
            // выбранный проект
            ComboBoxItem cbItem = null;
            string GITProject = "";
            if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
            if ((cbItem == null) || (cbItem.Tag == null)) GITProject = "";
            else GITProject = cbItem.Tag.ToString();

            string dir = Controls.Dialogs.FolderBrowserDialog(Path.Combine(MainWindow.APPinfo.GITFolder, GITProject));
            if (!string.IsNullOrWhiteSpace(dir))
            {
                tbExclude.Text = dir;
            }
        }

        /// <summary>
        /// Выгрузка на основании командной строки
        /// </summary>
        public void AutoChange()
        {
            if (
                (App.Args.Length >= 8) &&
                (!string.IsNullOrWhiteSpace(MainWindow.Task.TaskNumber))
            )
            {
                tbMask.Text = App.Args[2];
                tbExclude.Text = App.Args[3];

                if (App.Args[4].ToLower() == "startswith")
                {
                    rbSearchStartsWith.IsChecked = true;
                }
                else if (App.Args[4].ToLower() == "contains")
                {
                    rbSearchContains.IsChecked = true;
                }
                else if (App.Args[4].ToLower() == "endswith")
                {
                    rbSearchEndsWith.IsChecked = true;
                }

                tbSearch.Text = App.Args[5];

                if (App.Args[6].ToLower() == "replaceall")
                {
                    rbReplaceAll.IsChecked = true;
                }
                else if (App.Args[6].ToLower() == "replace")
                {
                    rbReplace.IsChecked = true;
                }
                else if (App.Args[6].ToLower() == "replaceaddstart")
                {
                    rbReplaceAddStart.IsChecked = true;
                }
                else if (App.Args[6].ToLower() == "replaceaddend")
                {
                    rbReplaceAddEnd.IsChecked = true;
                }
                else if (App.Args[6].ToLower() == "insertbefore")
                {
                    rbInsertBefore.IsChecked = true;
                }
                else if (App.Args[6].ToLower() == "insertafter")
                {
                    rbInsertAfter.IsChecked = true;
                }
                else if (App.Args[6].ToLower() == "delete")
                {
                    rbDelete.IsChecked = true;
                }

                tbReplace.Text = App.Args[7].Replace("|", "\n");

                isAutoChange = true;

                ChangeClick();
            }
        }
    }

    /// <summary>
    /// Тип поиска
    /// </summary>
    internal enum SearchAction
    {
        /// <summary>
        /// искать строку, начинающуюся с фразы
        /// </summary>
        StartsWith,
        /// <summary>
        /// искать строку, содержащую фразу
        /// </summary>
        Contains,
        /// <summary>
        /// искать строку, оканчивающуюся на фразу
        /// </summary>
        EndsWith
    }

    /// <summary>
    /// Тип изменения
    /// </summary>
    internal enum ReplaceAction
    {
        /// <summary>
        /// заменить всю найденной строку
        /// </summary>
        ReplaceAll,
        /// <summary>
        /// заменить часть найденной строки
        /// </summary>
        Replace,
        /// <summary>
        /// добавить в начало найденной строки
        /// </summary>
        ReplaceAddStart,
        /// <summary>
        /// добавить в конец найденной строки
        /// </summary>
        ReplaceAddEnd,
        /// <summary>
        /// добавить перед найденной строкой
        /// </summary>
        InsertBefore,
        /// <summary>
        /// добавить после найденной строки
        /// </summary>
        InsertAfter,
        /// <summary>
        /// удалить найденную строку
        /// </summary>
        Delete
    }
    /// <summary>
    /// параметры для изменения файлов
    /// </summary>
    internal class ChangeParam
    {

        /// <summary>
        /// Путь поиска изменяемых файлов
        /// </summary>
        public string pathMask { get; set; }

        /// <summary>
        /// Маска изменяемых файлов
        /// </summary>
        public string fileMask { get; set; }

        /// <summary>
        /// Исключаемые папки
        /// </summary>
        public string excludePath { get; set; }

        /// <summary>
        /// Тип поиска
        /// </summary>
        public SearchAction searchAction { get; set; }

        /// <summary>
        /// Тип изменения
        /// </summary>
        public ReplaceAction replaceAction { get; set; }

        /// <summary>
        /// список файлов для изменения
        /// </summary>
        public List<string> allObjects { get; set; }

        /// <summary>
        /// строка поиска
        /// </summary>
        public string searchText { get; set; }

        /// <summary>
        /// строка изменения
        /// </summary>
        public string replaceText { get; set; }

        /// <summary>
        /// файл содержит фразу
        /// </summary>
        public string fileContains { get; set; }

        /// <summary>
        /// файл НЕ содержит фразу
        /// </summary>
        public string fileNotContains { get; set; }

        /// <summary>
        /// =true - поиск учитывает регистр
        /// </summary>
        public bool isCaseSensitivity { get; set; }

        /// <summary>
        /// =true - регулярное выражение при поиске фразы
        /// </summary>
        public bool isRegex { get; set; }
    }
}
