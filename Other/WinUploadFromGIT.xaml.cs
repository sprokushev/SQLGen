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
    /// Окно выгрузки процедур и представлений из БД
    /// </summary>
    public partial class WinUploadFromGIT : Window
    {

        /// <summary>Список результатов поиска</summary>
        public List<RowInfo> ListResults = new List<RowInfo>();

        // номер предыдущей задачи
        private string LastTaskNumber = "";

        /// <summary>
        /// фоновый воркер
        /// </summary>
        private BackgroundWorker backgroundWorker1;
        private int highestPercentageReached = 0;

        /// <summary>
        /// флаг выполнения git pull
        /// </summary>
        bool isRefreshed = false;

        /// <summary>Конструктор WinUploadFromGIT</summary>
        public WinUploadFromGIT()
        {
            InitializeComponent();

            addTables.IsChecked = false;
            addViews.IsChecked = true;
            addProcs.IsChecked = true;
            addTriggers.IsChecked = false;
            addSequences.IsChecked = false;
            addMarkers.IsChecked = false;

            // инициализация фонового воркера
            backgroundWorker1 = new BackgroundWorker();
            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(UploadScripts);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(worker_RunWorkerCompleted);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(worker_ProgressChanged);


            tbFolder.Text = MainWindow.Task.TaskPath;
            tbFilter.InitHistory("HistoryUploadFromGIT.json", "");

            // пользовательские настройки GUI
            Default.InitGUI("WinUploadFromGIT", this, mainGrid, null, null, null, MainWindow.Task.LogFile);
        }

        /// <summary>При открытии окна WinUploadFromGIT</summary>
        private void winUploadFromGIT_Activated(object sender, EventArgs e)
        {
            this.Title = "Выгрузить объекты GIT - " + tbFilter.Text + ", задача " + MainWindow.Task.TaskNumber;

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

        /// <summary>При закрытии окна WinUploadFromGIT</summary>
        private void winUploadFromGIT_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinUploadFromGIT", this, null);
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
            this.Title = "Выгрузить объекты GIT - " + tbFilter.Text + ", задача " + MainWindow.Task.TaskNumber;
            btFind.Focus();
        }


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


        /// <summary>
        /// Заполнить список объектов GIT
        /// </summary>
        /// <param name="name">строка для поиска</param>
        public void FillResult(string name)
        {
            if ((cbGITProject != null) && (cbGITProject.SelectedIndex != -1))
            {
                GitPull();

                string project = "";

                ComboBoxItem cbItem = null;
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem; //-V3022
                if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                else project = cbItem.Tag.ToString();

                string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
                string GITPath = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);

                this.Cursor = Cursors.Wait;
                try
                {
                    var list = MainWindow.SearchInGIT("STRUCTNAME", true, false, false, project, GITPath, "", name);

                    ListResults = new List<RowInfo>();

                    if (addTables.IsChecked == true)
                    {
                        ListResults.AddRange(list.Where(x => x.objecttype == "TABLE"));
                    }

                    if (addViews.IsChecked == true)
                    {
                        ListResults.AddRange(list.Where(x => x.objecttype == "VIEW"));
                    }

                    if (addProcs.IsChecked == true)
                    {
                        ListResults.AddRange(list.Where(x => x.objecttype == "PROCEDURE" || x.objecttype == "FUNCTION"));
                    }

                    if (addTriggers.IsChecked == true)
                    {
                        ListResults.AddRange(list.Where(x => x.objecttype == "TRIGGER"));
                    }

                    if (addSequences.IsChecked == true)
                    {
                        ListResults.AddRange(list.Where(x => x.objecttype == "SEQUENCE"));
                    }

                    if (addMarkers.IsChecked == true)
                    {
                        ListResults.AddRange(list.Where(x => x.objecttype == "FREEDOCMARKER" || x.objecttype == "FREEDOCRELATIONSHIP"));
                    }

                    dgResults.ItemsSource = ListResults;
                    dgResults.Items.Refresh();
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
                this.Cursor = Cursors.Arrow;
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
                RowInfo value = dgResults.SelectedItem as RowInfo;

                // добавить результат в список
                tbObjects.Text = tbObjects.Text.Trim() + Environment.NewLine + value.sql;

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
                sb.Append(Environment.NewLine + item.sql);
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

            string project = "";

            Application.Current.Dispatcher.Invoke(() =>
            {
                if ((cbGITProject == null) || (cbGITProject.SelectedIndex == -1)) return;
                ComboBoxItem cbItem = null;
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem; //-V3022
                if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                else project = cbItem.Tag.ToString();
            });

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string GITPath = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
            RowInfo rowinfo = new RowInfo();

            BackgroundWorker worker = sender as BackgroundWorker;

            UploadParam param = (UploadParam)e.Argument;

            bool isError = false;
            bool isFirst = true;
            string error = "";
            string path = param.Folder;
            int maximum = param.allObjects.Count();
            int current = 0;

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
                // определяем тип, схему и имя объекта
                rowinfo.sql = item.Trim().Replace('/', Path.DirectorySeparatorChar);
                string scripttype = rowinfo.objecttype;
                string objectname = rowinfo.objectname;
                string objectschema = rowinfo.objectschema;

                // что копируем
                string from_file = Path.Combine(GITPath, rowinfo.sql);
                FileStream fs = null;
                FileMode fileMode = FileMode.Create;

                if (
                    (!string.IsNullOrWhiteSpace(scripttype)) && //-V3063
                    (!string.IsNullOrWhiteSpace(objectschema)) &&
                    (!string.IsNullOrWhiteSpace(objectname)) &&
                    File.Exists(from_file)
                    )
                {
                    try
                    {
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

                        // куда копируем
                        string filename = param.prefix + " " + param.task + " " + scriptnum + " " + scripttype.ToLower() + " " + objectschema + " " + objectname + ".sql";
                        string to_file = "";

                        if (isFirst)
                        {
                            // в первый раз спрашиваем, куда сохранить
                            to_file = Controls.Dialogs.SaveSQLDialog(path, filename, out fs, out fileMode);

                            if (fs != null)
                            {
                                // все последующие файлы сохраняем в эту же папку
                                path = Path.GetDirectoryName(to_file);
                                isFirst = false;
                            }
                        }
                        else
                        {
                            to_file = Path.Combine(path, filename);
                            fileMode = FileMode.Create;

                            if (File.Exists(to_file))
                            {
                                if (System.Windows.Forms.MessageBox.Show("Файл " + to_file + " уже существует!" + Environment.NewLine + "Перезаписать ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                {
                                    App.AddLog("Файл " + to_file + " уже существует, выбрано - Перезаписать", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                    fileMode = FileMode.Create;
                                }
                                else if (System.Windows.Forms.MessageBox.Show("Добавить в существующий файл ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                                {
                                    App.AddLog("Файл " + to_file + " уже существует, выбрано - Добавить в существующий файл", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                    fileMode = FileMode.Append;
                                }
                                else
                                {
                                    error += "Выгрузка прервана!" + Environment.NewLine;
                                    isError = true;
                                    break;
                                }
                            }

                            fs = new FileStream(to_file, fileMode);
                        }

                        //сохранить файл
                        if (fs != null)
                        {
                            string text = File.ReadAllText(from_file);

                            // заменить changeset

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                string err = "";
                                Utilities.Files.WriteScript(to_file, fs, text, false, out err, fileMode);

                                if (!string.IsNullOrWhiteSpace(err))
                                {
                                    error += objectschema + "." + objectname + " : " + err + Environment.NewLine;
                                    isError = true;
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        error += App.AddLog(objectschema + "." + objectname + " : ", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile).showMessage + Environment.NewLine;

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
        }

        private void UploadClick(UploadType UlpoadType)
        {
            if (MainWindow.Task == null) return;
            if ((cbGITProject == null) || (cbGITProject.SelectedIndex == -1)) return;

            if (backgroundWorker1.IsBusy == false)
            {
                // блокируем нажатие кнопок на время выполнения выгрузки
                this.btUpload.IsEnabled = false;
                this.btCancel.IsEnabled = true;

                GitPull();

                // собираем параметры для фоновой задачи
                UploadParam param = new UploadParam();

                param.Folder = tbFolder.Text.Trim();

                param.project = "";

                ComboBoxItem cbItem = null;
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem; //-V3022
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

                param.UploadType = UlpoadType;

                // запускаем фоновую задачу по выгрузке
                highestPercentageReached = 0;
                backgroundWorker1.RunWorkerAsync(param);
            }
        }

        /// <summary>Нажата кнопка Выгрузить для задачи</summary>
        private void btUpload_Click(object sender, RoutedEventArgs e)
        {
            UploadClick(UploadType.TASK);
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
            this.btCancel.IsEnabled = false;
        }

        /// <summary>Фоновая задача отменена</summary>
        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            // Cancel the asynchronous operation.
            if (backgroundWorker1 != null) backgroundWorker1.CancelAsync();

            this.btCancel.IsEnabled = false;
        }

        /// <summary>Просмотр текста хранимки</summary>
        private void ViewProcText()
        {
            if ((cbGITProject == null) || (cbGITProject.SelectedIndex == -1)) return;

            // выбранный проект
            string project = "";
            ComboBoxItem cbItem = null;
            if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem; //-V3022
            if ((cbItem == null) || (cbItem.Tag == null)) project = "";
            else project = cbItem.Tag.ToString();

            // просмотр текста скрипта из GIT
            if (
                (!string.IsNullOrWhiteSpace(project))
            )
            {
                string content = Utilities.Controls.GetSelectedValue(dgResults, dgResults.CurrentColumn.DisplayIndex);
                string file = "";
                if (!string.IsNullOrWhiteSpace(content))
                {
                    if (content.EndsWith(".sql"))
                    {
                        string folder = Utilities.GITProjects.GetFolderByProject(project);
                        file = Path.Combine(MainWindow.APPinfo.GITFolder, folder, content.Replace('/', Path.DirectorySeparatorChar));
                    }
                }

                if (
                    (!string.IsNullOrWhiteSpace(file)) &&
                    File.Exists(file)
                    )
                {
                    WinInfo WinInfo = new WinInfo(null);
                    WinInfo.Title = file;
                    WinInfo.tbInfo.Text = File.ReadAllText(file);
                    WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL");
                    WinInfo.Show();
                }
            }
        }


        /// <summary>Просмотр текста хранимки</summary>
        private void btView_Click(object sender, RoutedEventArgs e)
        {
            ViewProcText();
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
    }
}
