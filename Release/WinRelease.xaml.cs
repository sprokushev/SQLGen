// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using AngleSharp.Dom;
using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Path = System.IO.Path;
using SQLGen.Controls;
using SQLGen.Utilities;
using SQLGen.Forms;
using System.Text.Json;
using System.Windows.Shapes;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>
    /// Окно Формирование релизной версии
    /// </summary>
    public partial class WinRelease
    {
        /// <summary>
        /// единый лог-файл текущего сеанса сборки релиза
        /// </summary>
        public string logFileRelease;

        /// <summary>
        /// флаг улучшения скриптов релиза
        /// </summary>
        public bool isImproveSQLinVersionRelease;

        /// <summary>ссылка на экземпляр основного окна программы</summary>
        public MainWindow mainWindow;

        /// <summary>Полный список версий</summary>
        public SortedDictionary<double, Version> Versions = new SortedDictionary<double, Version>();

        /// <summary>Список СЛЕДУЮЩИХ версий</summary>
        public SortedDictionary<double, Version> NextVersions = new SortedDictionary<double, Version>();

        /// <summary>Флаг, что запущено заполнение</summary>
        bool isExecFill = false;

        /// <summary>Следующая версия</summary>
        string lastNextVersion = "";

        /// <summary>время последнего git pull</summary>
        DateTime lastGitPull = DateTime.MinValue;

        /// <summary>
        /// фильтрация задач
        /// </summary>
        private CollectionViewSource _filteredYML;

        /// <summary>
        /// фильтрация задач
        /// </summary>
        private ICollectionView FilteredYML
        {
            get
            {
                if (_filteredYML != null)
                {
                    return _filteredYML.View;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>Конструктор WinRelease</summary>
        public WinRelease()
        {
            InitializeComponent();

            // лог-файл сборки релиза
            logFileRelease = MainWindow.Task.LogFileRelease;

            // Контроль размера лог-файла
            Files.CutEndFileMaxSize(logFileRelease);

            // установить фильтр
            _filteredYML = new CollectionViewSource();
            _filteredYML.Source = MainWindow.Task.ReleaseYMLFiles;
            if (_filteredYML.View != null)
            {
                _filteredYML.View.Filter = delegate (object o) { return ShowOnlyFilter(o); };
            }
            if (_filteredYML.View != null && _filteredYML.View.CanSort == true)
            {
                _filteredYML.View.SortDescriptions.Clear();
                _filteredYML.View.SortDescriptions.Add(new SortDescription("YMLOrder", ListSortDirection.Ascending));
            }

            dgYMLFiles.ItemsSource = FilteredYML;

            // очистить поля
            ClearFields();

            // по умолчанию скрипты релиза не улучшаем
            isImproveSQLinVersionRelease = false;

            // проверять последний коммит
            isCheckLastCommit.IsChecked = MainWindow.APPinfo.isCheckLastCommit;

            // пользовательские настройки GUI
            Default.InitGUI("WinRelease", this, mainGrid, null, null, null, logFileRelease);
        }

        /// <summary>При открытии окна WinRelease</summary>
        private void winRelease_Activated(object sender, EventArgs e)
        {
            this.Title = "Сборка релиза " + MainWindow.Task.ReleaseVersion + ", задача " + MainWindow.Task.TaskNumber;

            dgYMLFiles.RowDetailsVisibilityMode = DataGridRowDetailsVisibilityMode.Collapsed;
            dgYMLFilesRefresh();

            tbNumVersion.Text = MainWindow.Task.ReleaseVersion;
            tbTasks.Text = MainWindow.Task.ReleaseTaskList;
            tbReleaseTaskNumber.Text = MainWindow.Task.TaskNumber;
            isCumulative.IsChecked = MainWindow.Task.ReleaseIsCumulative == true;

            NumVersionChanged();

            //TaskCalcCount();
        }

        /// <summary>При закрытии окна WinRelease</summary>
        private void winRelease_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinRelease", this, null);
        }

        /// <summary>
        /// Обновить список следующих версий
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <param name="current_branch">текущая ветка</param>
        /// <param name="current_version">номер текущей версии</param>
        /// <param name="isNoCumulative">=true - текущая версия НЕ кумулятивная</param>
        /// <param name="listBranches">список веток</param>
        /// <param name="Versions">список данных о версиях</param>
        /// <param name="NextVersions">список данных о СЛЕДУЮЩИХ версиях</param>
        /// <param name="logFile">лог-файл</param>
        /// <returns></returns>
        public static bool FillNextVersions(string project, string current_branch, string current_version, bool isNoCumulative, List<string> listBranches, SortedDictionary<double, Version> Versions, SortedDictionary<double, Version> NextVersions, string logFile)
        {
            if (
                string.IsNullOrWhiteSpace(project) ||
                string.IsNullOrWhiteSpace(current_branch) ||
                string.IsNullOrWhiteSpace(current_version) ||
                listBranches == null ||
                listBranches.Count == 0
                )
            {
                return false;
            }

            bool result = true;
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            bool isCumulative = isNoCumulative != true;

            double numversion = Release.VerAsNum(current_version);

            // проверим, нужен ли commit в текущей ветке
            if (!GIT.CheckCommit(project, logFile, $"Возможно, что список следующих версий будет НЕ корректным!!!"))
            {
                result = false;
            }

            double FirstVersionOrder = Utilities.GITProjects.GetFirstVersionOrderByProject(project, current_version);

            if (Versions == null)
            {
                Versions = new SortedDictionary<double, Version>();
            }
            if (NextVersions == null)
            {
                NextVersions = new SortedDictionary<double, Version>();
            }
            else
            {
                NextVersions.Clear();
            }

            bool isFirstNoCumulative = false;
            if (isNoCumulative) isFirstNoCumulative = true;

            // Добавить "будущие" версии
            foreach (var item in listBranches)
            {
                string branchname = item.Replace("*", "").Trim();
                string version = Release.GetNumVersion(prefix, branchname);
                double nn = Release.VerAsNum(version);

                if (
                    (nn > numversion) &&
                    (nn >= FirstVersionOrder) // начиная с разрыва кумулятивности 
                    )
                {
                    // переключимся на ветку следующей версии
                    if (GIT.GitSwitch(project, branchname, logFile, out string _branch, out string _err))
                    {
                        //проверяем наличие файла BRANCH_IGNORE в корне проекта
                        if (File.Exists(Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "BRANCH_IGNORE")))
                        {
                            break;
                        }

                        // читаем yml версии по номеру
                        var yml = new YMLStruct(null, logFile);
                        yml.LoadYMLByNumVersion(project, version, false, null, false, false);

                        if (isNoCumulative && yml.IsNoCumulative)
                        {
                            // пошли не кумулятивные версии
                            isFirstNoCumulative = true;
                        }

                        if (isNoCumulative && isFirstNoCumulative && yml.IsCumulative)
                        {
                            // закончились не кумулятивные версии
                            break;
                        }

                        if (
                            (!yml.IsIgnore) && //-V3063
                            (yml.IsFileExist)
                        )
                        {
                            Version ver = new Version(logFile) { Branch = branchname, YMLFile = yml, isBranchExists = true };

                            if (
                                isCumulative || // текущая версия - кумулятивная и все следующие - любые
                                isNoCumulative && yml.IsNoCumulative // текущая версия - НЕ кумулятивная и все следующие - только НЕ кумулятивные
                            )
                            {
                                if (!Versions.ContainsKey(nn))
                                {
                                    Versions.Add(nn, ver);
                                }
                                else
                                {
                                    Versions[nn].YMLFile = yml;
                                    Versions[nn].isBranchExists = true;
                                    Versions[nn].Branch = branchname;
                                }
                            }

                            if (
                                !NextVersions.ContainsKey(nn) &&
                                (
                                    isCumulative && yml.IsCumulative|| // текущая версия - кумулятивная и все следующие - тоже кумулятивные
                                    isNoCumulative && yml.IsNoCumulative // текущая версия - НЕ кумулятивная и все следующие - только НЕ кумулятивные
                                )
                            )
                            {
                                NextVersions.Add(nn, ver);
                            }
                        }
                    }
                }
            }

            // вернуть ветку версии
            GIT.GitSwitch(project, current_branch, logFile, out current_branch, out string err);

            return result;
        }

        /// <summary>
        /// Заполнить Versions и NextVersions для dev-проекта
        /// </summary>
        /// <param name="project">проект</param>
        /// <param name="currentbranch">текущая ветка</param>
        /// <param name="NumVersionText">номер текущей версии</param>
        /// <param name="FileVersionText">файл текущей версии</param>
        /// <param name="Versions">список ВСЕХ версий</param>
        /// <param name="NextVersions">список СЛЕДУЮЩИХ версий</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="isFetchAll">=true - выполнить git fetch --all</param>
        public static void FillVersionsInDev(string project, ref string currentbranch, string NumVersionText, string FileVersionText, SortedDictionary<double, Version> Versions, SortedDictionary<double, Version> NextVersions, string logFile, bool isFetchAll)
        {
            if (
                string.IsNullOrWhiteSpace(project) ||
                string.IsNullOrWhiteSpace(currentbranch) ||
                string.IsNullOrWhiteSpace(NumVersionText)
                )
            {
                return;
            }

            if (Versions == null)
            {
                Versions = new SortedDictionary<double, Version>();
            }
            else
            {
                Versions.Clear();
            }

            if (NextVersions == null)
            {
                NextVersions = new SortedDictionary<double, Version>();
            }
            else
            {
                NextVersions.Clear();
            }

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, NumVersionText));
            double FirstVersionOrder = Utilities.GITProjects.GetFirstVersionOrderByProject(project, NumVersionText);
            string err = "";

            // получим все ветки версий
            var listBranches = GIT.GitListBranches(project, "git_listversion.cmd", logFile, isFetchAll);

            // получим ветки, еще не влитые в master
            //var listNoMergedBranches = GIT.GitListNoMergedVersions(project, false, out string listBadBranch, logFileRelease, isForcedGitRefresh);

            // если текущая ветка - master
            if (currentbranch == "master")
            {
                // попробуем сменить на текущую или предыдущую 
                foreach (var _newbranch in listBranches
                    .Where(x => Release.VerAsNum(x) <= numversion)
                    .OrderByDescending(x => Release.VerAsNum(x))
                    )
                {
                    if (!string.IsNullOrWhiteSpace(_newbranch))
                    {
                        if (GIT.GitSwitch(project, _newbranch, logFile, out currentbranch, out err)) //-V3022
                        {
                            //проверяем наличие файла BRANCH_IGNORE в корне проекта
                            if (File.Exists(Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "BRANCH_IGNORE")))
                            {
                                break;
                            }

                            //проверяем, вдруг у версии есть флаг #IGNORE
                            var yml = new YMLStruct(null, logFile);
                            yml.LoadYMLByNumVersion(project, Release.GetNumVersion(prefix, _newbranch), false, null, false, false);

                            if (
                                (!yml.IsIgnore) && //-V3063
                                (yml.IsFileExist)
                            )
                            {
                                break;
                            }
                        }
                    }
                }
            }

            // сначала заполним Versions данными о существующих файлах версий в текущей ветке
            foreach (var file in Utilities.Files.ListFilesInDir(path, false, true, false))
            {
                string item = file.ToLower();

                if (
                    item.EndsWith(".yml") &&
                    (!item.Contains("_rpt")) &&
                    (!item.Contains("_ots"))
                    )
                {
                    // читаем yml версии
                    var yml = new YMLStruct(null, logFile);
                    yml.LoadYML(project, "version", file, false, null, false, false);
                    string version = yml.NumVersion;
                    string branchname = prefix + "." + version;
                    double nn = yml.NumVersionOrder;

                    // добавляем версию в список
                    if (
                        (nn > 0) &&
                        (nn >= FirstVersionOrder) && // начиная с разрыва кумулятивности
                        (!yml.IsIgnore) &&
                        (yml.IsFileExist) &&
                        (!Versions.ContainsKey(nn))
                        )
                    {
                        Version ver = new Version(logFile) { Branch = branchname, YMLFile = yml };
                        Versions.Add(nn, ver);
                    }
                }
            }

            // Затем заполнить Versions данными о ветках
            foreach (var item in listBranches)
            {
                string branchname = item.Replace("*", "").Trim();
                string version = Release.GetNumVersion(prefix, branchname);
                double nn = Release.VerAsNum(version);

                if (
                    (nn > 0) &&
                    (nn >= FirstVersionOrder) // начиная с разрыва кумулятивности
                    )
                {
                    if (!Versions.ContainsKey(nn))
                    {
                        // читаем yml версии по номеру версии
                        var yml = new YMLStruct(null, logFile);
                        yml.LoadYMLByNumVersion(project, version, false, null, false, false);
                        if (
                            (!yml.IsIgnore) && //-V3063
                            (yml.IsFileExist)

                        )
                        {
                            Version ver = new Version(logFile) { Branch = branchname, YMLFile = yml, isBranchExists = true };
                            Versions.Add(nn, ver);
                        }
                    }
                    else
                    {
                        Versions[nn].isBranchExists = true;
                    }
                }
            }

            // добавить информацию о текущей версии (если для нее еще не создан файл)
            if (!Versions.ContainsKey(numversion))
            {
                Version ver = new Version(logFile) { Branch = prefix + "." + Release.GetNumVersion(prefix, NumVersionText) };
                ver.YMLFile.IsFileExist = false;
                ver.YMLFile.Project = project;
                ver.YMLFile.Filepath = "version";
                ver.YMLFile.Filename = Path.GetFileName(FileVersionText);

                Versions.Add(numversion, ver);
            }

            // Добавить "будущие" версии
            bool isNoCumulative = Versions[numversion].YMLFile.IsNoCumulative;

            FillNextVersions(project, currentbranch, Release.GetNumVersion(prefix, NumVersionText), isNoCumulative, listBranches, Versions, NextVersions, logFile);

            currentbranch = GIT.GitCurrentBranch(project, out err, logFile);
        }

        /// <summary>
        /// Заполнить Versions и NextVersions
        /// </summary>
        public void FillVersions()
        {
            // Очистить Versions
            Versions.Clear();
            NextVersions.Clear();
            var oldPrevIndex = cbPrevVersion.SelectedIndex;
            var oldNextIndex = cbNextVersion.SelectedIndex;
            string oldPrev = "";
            string oldNext = "";
            if (oldPrevIndex != -1) oldPrev = cbPrevVersion.SelectedItem.ToString().Trim();
            if (oldNextIndex != -1) oldNext = cbNextVersion.SelectedItem.ToString().Trim();
            cbPrevVersion.Items.Clear();
            cbNextVersion.Items.Clear();
            cbPrevVersion.Items.Add("");
            cbNextVersion.Items.Add("");
            //Utilities.ForEach(MainWindow.APPinfo.ReleaseBranch, x => cbPrevVersion.Items.Add(x));

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);

            // текущая версия
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));
            string currentbranch = GIT.GitCurrentBranch(project, out string err, logFileRelease);
            double FirstVersionOrder = Utilities.GITProjects.GetFirstVersionOrderByProject(project, tbNumVersion.Text);

            if (Utilities.GITProjects.IsGITProject(project))
            {
                // В "старом" проекте заполнить Versions данными о версиях
                foreach (var file in Utilities.Files.ListFilesInDir(path, false, true, false))
                {
                    string item = file.ToLower();

                    if (
                        item.EndsWith(".yml") &&
                        (!item.Contains("_rpt")) &&
                        (!item.Contains("_ots"))
                        )
                    {
                        // читаем yml версии
                        var yml = new YMLStruct(null, logFileRelease);
                        yml.LoadYML(project, "version", file, false, null, false, false);
                        double nn = yml.NumVersionOrder;

                        // добавляем версию в список
                        if (
                            (nn > 0) &&
                            (nn >= FirstVersionOrder) && // начиная с разрыва кумулятивности
                            (!yml.IsIgnore) &&
                            (yml.IsFileExist) &&
                            (!Versions.ContainsKey(nn))
                            )
                        {
                            Version ver = new Version(logFileRelease) { YMLFile = yml };
                            Versions.Add(nn, ver);
                        }
                    }
                }

                // добавить информацию о текущей версии (если для нее еще не создан файл)
                if (!Versions.ContainsKey(numversion))
                {
                    Version ver = new Version(logFileRelease);
                    ver.YMLFile.IsFileExist = false;
                    ver.YMLFile.Project = project;
                    ver.YMLFile.Filepath = "version";
                    ver.YMLFile.Filename = Path.GetFileName(tbFileVersion.Text.Trim());

                    Versions.Add(numversion, ver);
                }

                // заполнить cbPrevVersion (в обратном порядке)
                foreach (var item in Versions
                    .Where(x => x.Key < numversion)
                    .OrderByDescending(x => x.Key)
                    )
                {
                    cbPrevVersion.Items.Add(item.Value.VisibleName);
                }

                // заполнить cbNextVersion (в прямом порядке)
                foreach (var item in Versions
                    .Where(x => x.Key > numversion)
                    .OrderBy(x => x.Key)
                    )
                {
                    cbNextVersion.Items.Add(item.Value.VisibleName);
                }
            }
            else
            {
                // в "новом" проекте заполнить Versions и NextVersions данными о версиях

                FillVersionsInDev(project, ref currentbranch, tbNumVersion.Text.Trim(), tbFileVersion.Text.Trim(), Versions, NextVersions, logFileRelease, true);

                if (!string.IsNullOrWhiteSpace(currentbranch))
                {
                    tbBranch.Text = currentbranch;
                }

                // заполнить cbPrevVersion (в обратном порядке)
                foreach (var item in Versions
                    .Where(x => x.Key < numversion)
                    .OrderByDescending(x => x.Key)
                    )
                {
                    cbPrevVersion.Items.Add(item.Value.VisibleName);
                }

                // заполнить cbNextVersion (в прямом порядке)
                foreach (var item in NextVersions
                    .Where(x => x.Key > numversion)
                    .OrderBy(x => x.Key)
                    )
                {
                    cbNextVersion.Items.Add(item.Value.VisibleName);
                }
            }

            if (oldPrevIndex != -1) cbPrevVersion.SelectedItem = oldPrev;
            if (oldNextIndex != -1) cbNextVersion.SelectedItem = oldNext;
        }

        /// <summary>
        /// Очистить все поля на форме
        /// </summary>
        private void ClearFields()
        {
            Versions.Clear();
            cbPrevVersion.Items.Clear();
            cbNextVersion.Items.Clear();
            cbPrevVersion.Items.Add("");
            cbNextVersion.Items.Add("");

            tbBranch.Text = "";
            cbPrevVersion.Text = "";
            tbFileVersion.Text = "";
            tbGITVersion.Text = "";
            cbLiquibot.Items.Clear();
            cbLiquibot.Text = "";
            cbNextVersion.Text = "";

            btGetNumVer.IsEnabled = true;
            tbNumVersion.IsReadOnly = false;
            btSetNumVersion.IsEnabled = false;

            //cbNextVersion.IsEnabled = false;
            //btFindNextVersion.IsEnabled = cbNextVersion.IsEnabled;
            //btSetNextVersion.IsEnabled = cbNextVersion.IsEnabled;

            //cbPrevVersion.IsEnabled = false;
            //btFindPrevVersion.IsEnabled = cbPrevVersion.IsEnabled;
            //btSetPrevVersion.IsEnabled = cbPrevVersion.IsEnabled;

            btGitPull.IsEnabled = false;
            btGitPush.IsEnabled = false;

            btGitNewBranch.IsEnabled = false;

            btMergeTask.IsEnabled = false;
            //btMergeTaskNext.IsEnabled = false;
            //btTransfer.IsEnabled = false;
            //miTaskTransferSH.IsEnabled = false;

            btClearFilter_Click(null, null);

            foreach (var info in MainWindow.Task.ReleaseYMLFiles)
            {
                info.MergeStatus = "";
            }
        }

        /// <summary>
        /// Видимость полей и кнопок для текущего выбранного проекта
        /// </summary>
        private void SetVisiblyForProject(string project)
        {
            btGetNumVer.IsEnabled = false;
            tbNumVersion.IsReadOnly = true;
            btSetNumVersion.IsEnabled = true;

            btGitPull.IsEnabled = true;
            btGitPush.IsEnabled = true;

            //btTransfer.IsEnabled = false;
            //miTaskTransferSH.IsEnabled = false;

            //cbPrevVersion.IsEnabled = true;
            //btFindPrevVersion.IsEnabled = cbPrevVersion.IsEnabled;
            //btSetPrevVersion.IsEnabled = cbPrevVersion.IsEnabled;

            tbFileVersion.IsEnabled = true;

            //cbNextVersion.IsEnabled = true;
            //btFindNextVersion.IsEnabled = cbNextVersion.IsEnabled;
            //btSetNextVersion.IsEnabled = cbNextVersion.IsEnabled;

            //tbFileVersion.IsReadOnly = false;

            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);

            string branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);

            string DEVStartVer = Utilities.GITProjects.GetDEVStartVerByProject(project);

            if (Utilities.GITProjects.IsDEVProject(project))
            {
                // "новый" проект

                //cbNextVersion.IsEnabled = false;
                //btFindNextVersion.IsEnabled = cbNextVersion.IsEnabled;
                //btSetNextVersion.IsEnabled = cbNextVersion.IsEnabled;

                //btSetPrevVersion.IsEnabled = false;

                btGitNewBranch.IsEnabled = true;
                btGitNewBranch.Content = $"Создать ветку {branchversion.Replace("_", "__")}";

                btMergeTask.IsEnabled = true;
                //btMergeTaskNext.IsEnabled = true;

                double nn = Release.VerAsNum(branchversion);


                // С 10-й версией transfer.sh не используем
                if (nn < Release.VerAsNum(DEVStartVer))
                {
                    //btTransfer.IsEnabled = true;
                    //miTaskTransferSH.IsEnabled = true;
                }

                if (tbBranch.Text.Trim().ToLower() != branchversion.ToLower())
                {
                    //cbPrevVersion.IsEnabled = true;
                    //btFindPrevVersion.IsEnabled = cbPrevVersion.IsEnabled;
                }
                else
                {
                    //cbPrevVersion.IsEnabled = false;
                    //btFindPrevVersion.IsEnabled = cbPrevVersion.IsEnabled;

                    btGitNewBranch.IsEnabled = false;
                }

                if (
                    Versions.ContainsKey(nn) &&
                    Versions[nn].isBranchExists
                  )
                {
                    // ветка существует
                    btGitNewBranch.Content = $"Переключиться на ветку {branchversion.Replace("_", "__")}";
                }
            }
        }

        /// <summary>
        /// список команд бота по алиасу
        /// </summary>
        /// <param name="_alias">алиас</param>
        /// <param name="_alias_ufa">алиас для Уфы</param>
        /// <param name="_project">проект</param>
        /// <param name="_numversion">номер версии</param>
        /// <returns></returns>
        private List<string> ListLiquibot(string _alias, string _alias_ufa, string _project, double _numversion)
        {
            List<string> list = new List<string>();

            foreach (var ver in Versions.Values
                .Where(x => 
                    (x.NumOrder == _numversion) || 
                    (x.NumOrder > _numversion) && (isCumulative.IsChecked == true)) //-V3024
                .OrderBy(x => x.NumOrder)
            )
            {
                string file_ver = "";
                string branch = "";

                if (_numversion == ver.NumOrder) //-V3024
                {
                    file_ver = tbFileVersion.Text.Trim();
                    branch = tbBranch.Text.Trim();
                }
                else
                {
                    file_ver = Path.GetFileName(ver.File);
                    branch = ver.Branch;
                }

                string cmd_alias = GITProjects.GetLuquibotAliasByProject(_project, _alias);
                if (!string.IsNullOrWhiteSpace(cmd_alias))
                {
                    cmd_alias = $"/update version/{file_ver} {cmd_alias} {branch}";
                }

                string cmd_alias_ufa = GITProjects.GetLuquibotAliasByProject(_project, _alias_ufa);
                if (!string.IsNullOrWhiteSpace(cmd_alias_ufa))
                {
                    cmd_alias_ufa = $"/update version/{file_ver} {cmd_alias_ufa} {branch}";
                }

                if (!string.IsNullOrWhiteSpace(cmd_alias)) list.Add(cmd_alias);
                if (!string.IsNullOrWhiteSpace(cmd_alias_ufa)) list.Add(cmd_alias_ufa);
            }

            return list;
        }

        /// <summary>
        /// Заполнить список в cbLiquibot
        /// </summary>
        /// <param name="project"></param>
        private void Fill_cbLiquibot(string project)
        {
            // текущая версия
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));

            cbLiquibot.Items.Clear();

            List<string> list = new List<string>();

            list.AddRange(ListLiquibot("LuquibotAliasOld", "LuquibotAliasOldUfa", project, numversion));
            list.Add("");
            list.AddRange(ListLiquibot("LuquibotAliasSP", "LuquibotAliasSPUfa", project, numversion));
            list.Add("");
            list.AddRange(ListLiquibot("LuquibotAliasHF", "LuquibotAliasHFUfa", project, numversion));
            list.Add("");
            list.AddRange(ListLiquibot("LuquibotAliasEHFAct", "LuquibotAliasEHFActUfa", project, numversion));
            list.Add("");
            list.AddRange(ListLiquibot("LuquibotAliasEHFUnAct", "LuquibotAliasEHFUnActUfa", project, numversion));
            list.Add("");
            list.AddRange(ListLiquibot("LuquibotAliasLTS", "LuquibotAliasLTSUfa", project, numversion));

            foreach (var item in list)
            {
                cbLiquibot.Items.Add(item);
            }

            cbLiquibot.SelectedIndex = 0;
        }

        /// <summary>
        /// Переключение проекта GIT
        /// </summary>
        /// <param name="branchversion">ветка версии</param>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        private void cbGITProject_Sync(string branchversion, bool isForcedGitRefresh)
        {
            ClearFields();

            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                return;
            }

            string project = cbGITProject.SelectedItem.ToString().Trim();

            // проверим, нужен ли commit в текущей ветке
            if (!GIT.CheckCommit(project, logFileRelease, ""))
            {
                cbGITProject.SelectedItem = null;
                return;
            }
            else
            {
                cbGITProject.SelectedItem = project;
            }

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            string URL = Utilities.GITProjects.GetURLVersionByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string postfix = Utilities.GITProjects.GetPostfixFileReleaseByProject(project);
            tbFileVersion.Text = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text) + "_" + DateTime.Now.ToString("ddMM") + "_" + MainWindow.Task.TaskNumber + postfix + ".yml";

            if (string.IsNullOrWhiteSpace(branchversion))
            {
                branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);
            }

            // обновить проект GIT
            GitPull_Sync(branchversion, isForcedGitRefresh);

            string err = "";
            tbBranch.Text = GIT.GitCurrentBranch(project, out err, logFileRelease);

            FillVersions();

            // текущая версия
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));

            YMLStruct loadyml = new YMLStruct(null, logFileRelease);

            if (Versions.ContainsKey(numversion))
            {
                loadyml = Versions[numversion].YMLFile;
            }

            if (loadyml.IsFileExist)
            {
                tbFileVersion.Text = loadyml.Filename;
                tbFileVersion.IsReadOnly = true;
            }
            else
            {
                tbFileVersion.IsReadOnly = false;
            }

            //проставляем флаг кумулятивности
            isCumulative.IsChecked = loadyml.IsCumulative;

            // определяем предыдущую версию
            string PrevVersion = loadyml.FirstPrevVersion?.NumVersionLine;

            string DEVStartVer = Utilities.GITProjects.GetDEVStartVerByProject(project);

            // выбираем предыдущую версию
            var ver = GetPrevVersion(project, path, tbFileVersion.Text.Trim(), Release.GetNumVersion(prefix, tbNumVersion.Text), PrevVersion);
            if (
                  (ver != null) &&
                  (
                        Utilities.GITProjects.IsGITProject(project) ||
                        numversion >= Release.VerAsNum(DEVStartVer)
                  )
            ) cbPrevVersion.SelectedItem = ver.VisibleName;
            else cbPrevVersion.SelectedIndex = -1;

            // выбираем следующую версию
            ver = GetNextVersion(project, path, tbFileVersion.Text.Trim(), Release.GetNumVersion(prefix, tbNumVersion.Text));
            if (
                  (ver != null) &&
                  (
                        Utilities.GITProjects.IsGITProject(project) ||
                        numversion >= Release.VerAsNum(DEVStartVer)
                  )
            )
            {
                cbNextVersion.SelectedItem = ver.VisibleName;
                // сохраняем выбранную следующую версию
                lastNextVersion = ver.VisibleName;
            }
            else
            {
                cbNextVersion.SelectedIndex = -1;
                // сохраняем выбранную следующую версию
                lastNextVersion = "";
            }

            // url на версию в GIT
            URL = URL.Replace("%BRANCH%", tbBranch.Text);
            tbGITVersion.Text = URL + tbFileVersion.Text.Trim();

            // Команды бота
            Fill_cbLiquibot(project);

            // Фильтр задач
            cbFilterList1.SelectedItem = "Выбранный проект";

            lastGitPull = DateTime.Now;

            // проверка текущей ветки и видимость кнопок и полей
            CheckBranch(out string branch);
        }

        /// <summary>Выбран проект GIT</summary>
        private void cbGITProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbGITProject_Sync("", false);
        }

        /// <summary>
        /// Найти предыдующую версию
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <param name="path">путь к файлу</param>
        /// <param name="file">файл версии</param>
        /// <param name="cur_ver">текущая версия</param>
        /// <param name="prev_ver">предыдущая версия</param>
        /// <returns></returns>
        public Version GetPrevVersion(string project, string path, string file, string cur_ver, string prev_ver) //-V3203
        {
            if (string.IsNullOrWhiteSpace(cur_ver)) cur_ver = "";
            cur_ver = cur_ver.Trim();
            double cur_ver_d = Release.VerAsNum(cur_ver);
            if (cur_ver_d <= 0) return null;

            if (string.IsNullOrWhiteSpace(prev_ver)) prev_ver = "";
            prev_ver = prev_ver.Trim();
            double prev_ver_d = Release.VerAsNum(prev_ver);

            if (prev_ver_d > 0)
            {
                // если в файле найдена предыдущая версия
                if (Versions.ContainsKey(prev_ver_d))
                    return Versions[prev_ver_d];
                else
                    return null;
            }

            if (!File.Exists(Path.Combine(path, file)))
            {
                // если файл не существует - найдем ближайшую предыдущую версию
                foreach (var ver in Versions.Where(x => x.Key < cur_ver_d).OrderByDescending(x => x.Key))
                {
                    return ver.Value;
                }

                return null;
            }

            return null;
        }

        /// <summary>
        /// Найти следующую версию
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <param name="path">путь к файлу</param>
        /// <param name="file">файл версии</param>
        /// <param name="cur_ver">номер текущей версии</param>
        /// <returns></returns>
        public Version GetNextVersion(string project, string path, string file, string cur_ver) //-V3203
        {
            if (string.IsNullOrWhiteSpace(cur_ver)) cur_ver = "";
            cur_ver = cur_ver.Trim();
            double cur_ver_d = Release.VerAsNum(cur_ver);
            if (cur_ver_d <= 0) return null;

            if (File.Exists(Path.Combine(path, file)))
            {
                // если файл существует - перебираем все последующие версии, ищем ссылку на текущую, возвращаем первую ближайшую
                foreach (var ver in NextVersions
                    .Where(x => 
                        (x.Key > cur_ver_d) && 
                        (x.Value.PrevNumOrder == cur_ver_d)
                        )
                    .OrderBy(x => x.Key)
                )
                {
                    return ver.Value;
                }

                return null;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Действия при изменении номера версии
        /// </summary>
        private void NumVersionChanged()
        {
            tbNumVersion.Text = tbNumVersion.Text.Replace(Path.DirectorySeparatorChar, '.').Replace('/', '.').Trim();

            if (!Release.IsNumVersionCorrect(tbNumVersion.Text))
            {
                MessageBox.Show($"Номер версии {tbNumVersion.Text} содержит не разрешенные символы!" + Environment.NewLine + "Разрешены буквы (от a до z), цифры (от 0 до 9) и точка (.)");
            }

            MainWindow.Task.ReleaseVersion = tbNumVersion.Text;
            this.Title = "Сборка релиза " + MainWindow.Task.ReleaseVersion + ", задача " + MainWindow.Task.TaskNumber;

            if (string.IsNullOrWhiteSpace(MainWindow.Task.ReleaseVersion))
            {
                cbGITProject.IsEnabled = false;
                btGitPull.IsEnabled = false;
                btGitPush.IsEnabled = false;

                btGitNewBranch.IsEnabled = false;

                btMergeTask.IsEnabled = false;
                //btMergeTaskNext.IsEnabled = false;
                //btTransfer.IsEnabled = false;
                //miTaskTransferSH.IsEnabled = false;

                //cbPrevVersion.IsEnabled = false;
                //btFindPrevVersion.IsEnabled = cbPrevVersion.IsEnabled;
                //btSetPrevVersion.IsEnabled = cbPrevVersion.IsEnabled;

                tbFileVersion.IsEnabled = false;

                //cbNextVersion.IsEnabled = false;
                //btFindNextVersion.IsEnabled = cbNextVersion.IsEnabled;
                //btSetNextVersion.IsEnabled = cbNextVersion.IsEnabled;

                btSetNumVersion.IsEnabled = false;

                btFillURL.Content = "https://jira.rtmis.ru/issues/?jql=fixVersion%20in%20";
                btFillURL.IsEnabled = false;
            }
            else
            {
                cbGITProject.IsEnabled = true;
                btFillURL.Content = $"https://jira.rtmis.ru/issues/?jql=fixVersion%20in%20({tbNumVersion.Text})";
                btFillURL.IsEnabled = true;
            }

            double numversion = Release.VerAsNum(tbNumVersion.Text);

            isImproveSQLinVersionRelease = MainWindow.APPinfo.isImproveSQLinVersion;
        }

        /// <summary>При выходе из поля Номер версии</summary>
        private void tbNumVersion_LostFocus(object sender, RoutedEventArgs e)
        {
            NumVersionChanged();
        }

        /// <summary>Изменилось значение в поле Номер версии</summary>
        private void tbNumVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            //NumVersionChanged();
        }

        /// <summary>
        /// =true - строка входит в фильтр
        /// </summary>
        /// <param name="e">экземпляр YMLFileInfo</param>
        /// <returns></returns>
        public bool ShowOnlyFilter(object e)
        {
            var yml = e as YMLFileInfo;
            if (yml != null)
            {
                if ((yml.IsFiltered1 == true) && (yml.IsFiltered2 == true) && (yml.IsFiltered3 == true) && (yml.IsFiltered4 == true))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>Обновить dgYMLFiles</summary>
        public void dgYMLFilesRefresh()
        {
            // обновить грид
            //dgYMLFiles.Items.Refresh();
            if (FilteredYML != null)
            {
                var lcv = (ListCollectionView)FilteredYML;
                if (lcv.IsAddingNew) lcv.CommitNew();
                if (lcv.IsEditingItem) lcv.CommitEdit();
                FilteredYML.Refresh();
            }

            // обновить итоги
            TaskCalcCount();

            // скрыть/показать колонки
            YMLFile_PG.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_MS.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_LIS.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_EMD.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_log_service_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_log_service_ms.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_php_log_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_php_log_ms.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_userportal_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_userportal_ms.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_fer_log.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_ac_mlo_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_ac_mlo_ms.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_PG.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_MS.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_LIS.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_EMD.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_log_service_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_log_service_ms.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_php_log_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_php_log_ms.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_userportal_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_userportal_ms.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_fer_log.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_ac_mlo_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_ac_mlo_ms.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_smp2_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_gar_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_proxy_pg.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_dev_bi.Visibility = System.Windows.Visibility.Hidden;
            YMLFile_unknown.Visibility = System.Windows.Visibility.Hidden;
            MergeStatus.Visibility = System.Windows.Visibility.Hidden;
            BranchName.Visibility = System.Windows.Visibility.Hidden;
            TaskCommitDate.Visibility = System.Windows.Visibility.Hidden;

            if (cbGITProject.SelectedIndex != -1)
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();
                if (Utilities.GITProjects.IsDEVProject(project))
                {
                    MergeStatus.Visibility = System.Windows.Visibility.Visible;
                    BranchName.Visibility = System.Windows.Visibility.Visible;
                    TaskCommitDate.Visibility = System.Windows.Visibility.Visible;
                }
            }

            foreach (var item in MainWindow.Task.ReleaseYMLFiles.Where(x =>
                (x.IsFiltered1 == true) &&
                (x.IsFiltered2 == true) &&
                (x.IsFiltered3 == true) &&
                (x.IsFiltered4 == true)
                ))
            {
                if ((YMLFile_PG.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_PG)))
                    YMLFile_PG.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_MS.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_MS)))
                    YMLFile_MS.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_LIS.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_LIS)))
                    YMLFile_LIS.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_EMD.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_EMD)))
                    YMLFile_EMD.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_log_service_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_log_service_pg)))
                    YMLFile_log_service_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_log_service_ms.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_log_service_ms)))
                    YMLFile_log_service_ms.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_php_log_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_php_log_pg)))
                    YMLFile_php_log_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_php_log_ms.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_php_log_ms)))
                    YMLFile_php_log_ms.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_userportal_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_userportal_pg)))
                    YMLFile_userportal_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_userportal_ms.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_userportal_ms)))
                    YMLFile_userportal_ms.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_fer_log.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_fer_log)))
                    YMLFile_fer_log.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_ac_mlo_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_ac_mlo_pg)))
                    YMLFile_ac_mlo_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_ac_mlo_ms.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_ac_mlo_ms)))
                    YMLFile_ac_mlo_ms.Visibility = System.Windows.Visibility.Visible;

                if ((YMLFile_dev_PG.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_PG)))
                    YMLFile_dev_PG.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_MS.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_MS)))
                    YMLFile_dev_MS.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_LIS.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_LIS)))
                    YMLFile_dev_LIS.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_EMD.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_EMD)))
                    YMLFile_dev_EMD.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_log_service_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_log_service_pg)))
                    YMLFile_dev_log_service_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_log_service_ms.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_log_service_ms)))
                    YMLFile_dev_log_service_ms.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_php_log_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_php_log_pg)))
                    YMLFile_dev_php_log_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_php_log_ms.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_php_log_ms)))
                    YMLFile_dev_php_log_ms.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_userportal_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_userportal_pg)))
                    YMLFile_dev_userportal_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_userportal_ms.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_userportal_ms)))
                    YMLFile_dev_userportal_ms.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_fer_log.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_fer_log)))
                    YMLFile_dev_fer_log.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_ac_mlo_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_ac_mlo_pg)))
                    YMLFile_dev_ac_mlo_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_ac_mlo_ms.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_ac_mlo_ms)))
                    YMLFile_dev_ac_mlo_ms.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_smp2_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_smp2_pg)))
                    YMLFile_dev_smp2_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_gar_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_gar_pg)))
                    YMLFile_dev_gar_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_proxy_pg.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_proxy_pg)))
                    YMLFile_dev_proxy_pg.Visibility = System.Windows.Visibility.Visible;
                if ((YMLFile_dev_bi.Visibility == System.Windows.Visibility.Hidden) && (!string.IsNullOrWhiteSpace(item.YMLFile_dev_bi)))
                    YMLFile_dev_bi.Visibility = System.Windows.Visibility.Visible;

                if ((YMLFile_unknown.Visibility == System.Windows.Visibility.Hidden) && (item.YMLFile_unknown != null) && (item.YMLFile_unknown != ""))
                    YMLFile_unknown.Visibility = System.Windows.Visibility.Visible;
            }
        }

        /// <summary>
        /// Парсинг HTML-страниц из Jira - заполнить список YML и JSON-файлов
        /// </summary>
        /// <param name="ListTask">список обновляемых задач (через запятую, точку запятую или на разных строках)</param>
        /// <param name="ListYMLFiles">итог парсинга</param>
        /// <param name="isRefresh">=true только обновить</param>
        /// <param name="_task">номер конкретной задачи</param>
        /// <returns></returns>
        void FillYMLFiles(string ListTask, List<YMLFileInfo> ListYMLFiles, bool isRefresh, string _task = "")
        {
            //bool isPreCheck = cbPreCheck.IsChecked == true;

            if (!string.IsNullOrWhiteSpace(_task))
            {
                // если процедура вызвана с номером задачи, значит надо обновить только эту задачу (список задач)
                _task = _task.Trim();
                ListTask = _task;
                isRefresh = true;
            }

            ListTask = ListTask.Trim().ToUpper()
                .Replace(';', ',')
                .Replace('\r', ',')
                .Replace('\n', ',')
                .Trim();

            var TaskArr = new Dictionary<string, string>();

            foreach (var item in ListTask.Split(',')
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .ToList()
                )
            {
                if (!TaskArr.ContainsKey(item)) TaskArr.Add(item, "");
            }

            if (!isRefresh)
            {
                // очищаем, заполняем заново
                ListYMLFiles.Clear();
                cbFilterList1.SelectedIndex = 0;
                cbFilterList2.SelectedIndex = 0;
                cbFilterList3.SelectedIndex = 0;
                tbFilterTask.Text = "";
                btFindTask_Click(null, null);

                //dgYMLFilesRefresh();
            }
            else
            {
                // обновляем, сбросим флаг обновления
                foreach (var yml in ListYMLFiles) yml.IsRefreshed = true;
                foreach (var yml in ListYMLFiles.Where(x => string.IsNullOrWhiteSpace(_task) || TaskArr.ContainsKey(x.TaskNumber))) yml.IsRefreshed = false;
            }

            TaskCalcCount();

            if ((!string.IsNullOrWhiteSpace(ListTask)) && HTML.OpenLoginJira(logFileRelease))
            {
                this.Cursor = Cursors.Wait;
                isExecFill = true;

                HTML html = new HTML();

                //List<ymlfile> ymllist = new List<ymlfile>();
                //int CountYml = 0;

                // Отключаем элементы интерфейса
                var listControls = Utilities.Controls.DisabaleOnStart(mainGrid, null);

                // Шаг 1 - парсинг страниц из Jira
                html.LoadJiraPages(TaskArr, this,
                    before_all =>
                    {
                        pbProgressFill.Maximum = TaskArr.Count;
                        pbProgressFill.Minimum = 0;
                        pbProgressFill.Value = 0;
                        lbCount.Content = "";
                    },
                    x =>
                    {
                        lbCount.Content = "Парсинг " + x.TaskNumber.Replace("_", "__");
                    },
                    y =>
                    {
                        lbCount.Content = "Парсинг " + y.TaskNumber.Replace("_", "__") + " - завершен";
                        pbProgressFill.Value = pbProgressFill.Value + 1;
                    },
                    after_all =>
                    {
                        // Шаг 2 - собираем промежуточный список файлов, сортируем
                        List<YMLFileInfo> newlist = new List<YMLFileInfo>();
                        Regex green_ymlname = new Regex(@"^(promedweb|rpms|smp|bip)-(\d+)(.yml|.json)\z");

                        foreach (var page in after_all.JiraPages.OrderBy(x => x.Order))
                        {
                            //lbCount.Content = "Анализ " + page.TaskNumber;

                            YMLFileInfo YMLFile = null;

                            // перебираем файлы, если они есть
                            foreach (var item in page.ListYML.OrderBy(l => l.OrderInTask))
                            {
                                YMLFile = null;

                                // определяем ветку
                                string checkbranch = item.Branch;
                                if (
                                        (
                                        string.IsNullOrWhiteSpace(checkbranch) ||
                                        checkbranch.ToLower() == "dev" ||
                                        MainWindow.APPinfo.ReleaseBranch.Contains(checkbranch, StringComparer.OrdinalIgnoreCase)
                                        ) &&
                                        (!string.IsNullOrWhiteSpace(item.Filename))
                                    )
                                {
                                    string _yml = item.Filename.Split('.')[0];
                                    checkbranch = Utilities.Task.GetTaskNumber(_yml);
                                }

                                // найдем уже добавленный файл
                                foreach (var findYMLFile in newlist
                                    .Where(x =>
                                            (x.TaskNumber == page.TaskNumber) &&
                                            (x.BranchName.ToLower() == checkbranch.ToLower()) &&
                                            (x.PathInGIT.ToLower() == item.Path.ToLower()) &&
                                            (x.GetYMLFileDefault == item.Filename)
                                    )
                                )
                                {
                                    YMLFile = findYMLFile.Copy();
                                    newlist.Remove(findYMLFile);
                                    break;
                                }

                                // не найден, надо создать новый
                                if (YMLFile == null) YMLFile = new YMLFileInfo();

                                YMLFile.TaskNumber = page.TaskNumber;
                                YMLFile.DataBD = page.DataBD;
                                YMLFile.IsDowntime = page.IsDowntime;
                                YMLFile.IsBaseRegion = page.IsBaseRegionBD;
                                YMLFile.ObjectsBD = page.ObjectsBD;
                                YMLFile.Region = page.Region;
                                YMLFile.TaskStatus = page.TaskStatus;
                                YMLFile.UpdActions = page.UpdActions;
                                YMLFile.Version = page.FixInVersion;
                                YMLFile.IsAddRelease = !string.IsNullOrWhiteSpace(item.Filename);
                                YMLFile.Order = page.Order + item.OrderInTask;
                                YMLFile.isUpdated = "";
                                YMLFile.ErrorInfo = page.ErrorInfo;
                                YMLFile.Branch = item.Branch;
                                YMLFile.PathInGIT = item.Path;

                                string ymlfield = Utilities.GITProjects.GetYMLFieldByProject(item.Project);
                                if (string.IsNullOrWhiteSpace(ymlfield))
                                {
                                    ymlfield = "YMLFile_unknown";
                                }

                                if (string.IsNullOrWhiteSpace(item.Filename)) item.Filename = "";
                                string YMLFile_Comment = "";

                                if (!string.IsNullOrWhiteSpace(item.Filename))
                                {
                                    if (!green_ymlname.IsMatch(item.Filename.ToLower()))
                                        YMLFile_Comment = "Нестандартное имя файла.";
                                }

                                YMLFile.SetYMLFile(ymlfield, item.Filename);
                                YMLFile.SetYMLFile_Comment(ymlfield, YMLFile_Comment);

                                newlist.Add(YMLFile.Copy());
                            }

                            // если файлов нет в задаче
                            if (page.ListYML.Count == 0)
                            {
                                // найдем уже добавленный файл
                                foreach (var findYMLFile in newlist
                                    .Where(x =>
                                        (x.TaskNumber == page.TaskNumber) &&
                                        string.IsNullOrWhiteSpace(x.GetYMLFileDefault)))
                                {
                                    YMLFile = findYMLFile.Copy();
                                    newlist.Remove(findYMLFile);
                                    break;
                                }

                                // не найден, надо создать новый
                                if (YMLFile == null) YMLFile = new YMLFileInfo();

                                YMLFile.TaskNumber = page.TaskNumber;
                                YMLFile.DataBD = page.DataBD;
                                YMLFile.IsDowntime = page.IsDowntime;
                                YMLFile.IsBaseRegion = page.IsBaseRegionBD;
                                YMLFile.ObjectsBD = page.ObjectsBD;
                                YMLFile.Region = page.Region;
                                YMLFile.TaskStatus = page.TaskStatus;
                                YMLFile.UpdActions = page.UpdActions;
                                YMLFile.Version = page.FixInVersion;
                                YMLFile.IsAddRelease = false;
                                YMLFile.Order = page.Order + "00000";
                                YMLFile.isUpdated = "";
                                YMLFile.ErrorInfo = page.ErrorInfo;
                                YMLFile.Branch = "";
                                YMLFile.PathInGIT = "";

                                newlist.Add(YMLFile.Copy());
                            }
                        }

                        // Шаг 3 - заполняем новый или обновляем текущий релизный список yml-файлов
                        int cnt = 0;
                        foreach (var item in newlist.OrderBy(l => l.Order))
                        {
                            //lbCount.Content = "Добавление\обновление " + item.TaskNumber;

                            if (string.IsNullOrWhiteSpace(_task))
                            {
                                // если обновляем все задачи - то увеличиваем номер на 10
                                cnt += 10;
                            }
                            else
                            {
                                // если обновляем конкретную задачу - то увеличиваем на 1
                                cnt += 1;
                            }
                            item.YMLOrder = cnt;
                            item.IsFiltered1 = true;
                            item.IsFiltered2 = true;
                            item.IsFiltered3 = true;
                            item.IsFiltered4 = true;

                            if (isRefresh)
                            {
                                //lbCount.Content = "Обновление " + item.TaskNumber;

                                bool isfind = false;

                                // задача уже есть в списке - обновляем информацию о задаче
                                foreach (var yml in ListYMLFiles
                                    .Where(x =>
                                        (x.TaskNumber == item.TaskNumber) &&
                                        (!x.IsRefreshed)
                                    )
                                    .OrderBy(l => l.YMLOrder)
                                )
                                {
                                    if (yml.TaskStatus != item.TaskStatus)
                                    {
                                        yml.TaskStatus = item.TaskStatus;
                                        yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменился статус задачи";
                                    }
                                    if (yml.Version != item.Version)
                                    {
                                        yml.Version = item.Version;
                                        yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменилась версия релиза в задаче";
                                    }
                                    if (yml.Branch != item.Branch)
                                    {
                                        yml.Branch = item.Branch;
                                        yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменилась ветка задачи";
                                    }
                                    if (yml.PathInGIT != item.PathInGIT)
                                    {
                                        if (!string.IsNullOrWhiteSpace(yml.PathInGIT)) //-V3022
                                        {
                                            yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменилась папка с файлом задачи";
                                        }
                                        yml.PathInGIT = item.PathInGIT;
                                    }
                                    if (yml.Region != item.Region)
                                    {
                                        yml.Region = item.Region;
                                        yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменился регион в задаче";
                                    }
                                    if (yml.DataBD != item.DataBD)
                                    {
                                        yml.DataBD = item.DataBD;
                                        yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменились данные в БД в задаче";
                                    }
                                    if (yml.ObjectsBD != item.ObjectsBD)
                                    {
                                        yml.ObjectsBD = item.ObjectsBD;
                                        yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменились объекты БД в задаче";
                                    }
                                    if (yml.IsDowntime != item.IsDowntime)
                                    {
                                        yml.IsDowntime = item.IsDowntime;
                                        yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменился флаг Downtime в задаче";
                                    }
                                    if (yml.IsBaseRegion != item.IsBaseRegion)
                                    {
                                        yml.IsBaseRegion = item.IsBaseRegion;
                                        yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменился флаг Базовая региональность БД в задаче";
                                    }
                                    if (yml.UpdActions != item.UpdActions)
                                    {
                                        yml.UpdActions = item.UpdActions;
                                        yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменились действия при обновлении в задаче";
                                    }

                                    if (string.IsNullOrWhiteSpace(_task))
                                        // если обновляем все задачи - то обновляем N п/п
                                        yml.YMLOrder = item.YMLOrder;
                                    else
                                        // если обновляем конкретную задачу - надо сохранить номер по порядку даже для новых yml-файлов
                                        cnt = yml.YMLOrder;

                                    if (
                                        ((yml.YMLFile_PG ?? "") != (item.YMLFile_PG ?? "")) ||
                                        ((yml.YMLFile_MS ?? "") != (item.YMLFile_MS ?? "")) ||
                                        ((yml.YMLFile_LIS ?? "") != (item.YMLFile_LIS ?? "")) ||
                                        ((yml.YMLFile_EMD ?? "") != (item.YMLFile_EMD ?? "")) ||
                                        ((yml.YMLFile_log_service_pg ?? "") != (item.YMLFile_log_service_pg ?? "")) ||
                                        ((yml.YMLFile_log_service_ms ?? "") != (item.YMLFile_log_service_ms ?? "")) ||
                                        ((yml.YMLFile_php_log_pg ?? "") != (item.YMLFile_php_log_pg ?? "")) ||
                                        ((yml.YMLFile_php_log_ms ?? "") != (item.YMLFile_php_log_ms ?? "")) ||
                                        ((yml.YMLFile_userportal_pg ?? "") != (item.YMLFile_userportal_pg ?? "")) ||
                                        ((yml.YMLFile_userportal_ms ?? "") != (item.YMLFile_userportal_ms ?? "")) ||
                                        ((yml.YMLFile_fer_log ?? "") != (item.YMLFile_fer_log ?? "")) ||
                                        ((yml.YMLFile_ac_mlo_pg ?? "") != (item.YMLFile_ac_mlo_pg ?? "")) ||
                                        ((yml.YMLFile_ac_mlo_ms ?? "") != (item.YMLFile_ac_mlo_ms ?? "")) ||
                                        ((yml.YMLFile_dev_PG ?? "") != (item.YMLFile_dev_PG ?? "")) ||
                                        ((yml.YMLFile_dev_MS ?? "") != (item.YMLFile_dev_MS ?? "")) ||
                                        ((yml.YMLFile_dev_LIS ?? "") != (item.YMLFile_dev_LIS ?? "")) ||
                                        ((yml.YMLFile_dev_EMD ?? "") != (item.YMLFile_dev_EMD ?? "")) ||
                                        ((yml.YMLFile_dev_log_service_pg ?? "") != (item.YMLFile_dev_log_service_pg ?? "")) ||
                                        ((yml.YMLFile_dev_log_service_ms ?? "") != (item.YMLFile_dev_log_service_ms ?? "")) ||
                                        ((yml.YMLFile_dev_php_log_pg ?? "") != (item.YMLFile_dev_php_log_pg ?? "")) ||
                                        ((yml.YMLFile_dev_php_log_ms ?? "") != (item.YMLFile_dev_php_log_ms ?? "")) ||
                                        ((yml.YMLFile_dev_userportal_pg ?? "") != (item.YMLFile_dev_userportal_pg ?? "")) ||
                                        ((yml.YMLFile_dev_userportal_ms ?? "") != (item.YMLFile_dev_userportal_ms ?? "")) ||
                                        ((yml.YMLFile_dev_fer_log ?? "") != (item.YMLFile_dev_fer_log ?? "")) ||
                                        ((yml.YMLFile_dev_ac_mlo_pg ?? "") != (item.YMLFile_dev_ac_mlo_pg ?? "")) ||
                                        ((yml.YMLFile_dev_ac_mlo_ms ?? "") != (item.YMLFile_dev_ac_mlo_ms ?? "")) ||
                                        ((yml.YMLFile_dev_smp2_pg ?? "") != (item.YMLFile_dev_smp2_pg ?? "")) ||
                                        ((yml.YMLFile_dev_gar_pg ?? "") != (item.YMLFile_dev_gar_pg ?? "")) ||
                                        ((yml.YMLFile_dev_proxy_pg ?? "") != (item.YMLFile_dev_proxy_pg ?? "")) ||
                                        ((yml.YMLFile_dev_bi ?? "") != (item.YMLFile_dev_bi ?? ""))
                                        )
                                    {
                                        // если хотя бы один yml-файл не совпадает, значит надо заполнить флаг включения в релиз, чтобы случайно не упустить
                                        yml.IsAddRelease = item.IsAddRelease;
                                        yml.isUpdated = yml.isUpdated + Environment.NewLine + "Изменилось имя yml-файла и включение в релиз";
                                    }

                                    yml.YMLFile_PG = item.YMLFile_PG;
                                    yml.YMLFile_PG_Comment = item.YMLFile_PG_Comment;
                                    yml.YMLFile_MS = item.YMLFile_MS;
                                    yml.YMLFile_MS_Comment = item.YMLFile_MS_Comment;
                                    yml.YMLFile_LIS = item.YMLFile_LIS;
                                    yml.YMLFile_LIS_Comment = item.YMLFile_LIS_Comment;
                                    yml.YMLFile_EMD = item.YMLFile_EMD;
                                    yml.YMLFile_EMD_Comment = item.YMLFile_EMD_Comment;
                                    yml.YMLFile_log_service_pg = item.YMLFile_log_service_pg;
                                    yml.YMLFile_log_service_pg_Comment = item.YMLFile_log_service_pg_Comment;
                                    yml.YMLFile_log_service_ms = item.YMLFile_log_service_ms;
                                    yml.YMLFile_log_service_ms_Comment = item.YMLFile_log_service_ms_Comment;
                                    yml.YMLFile_php_log_pg = item.YMLFile_php_log_pg;
                                    yml.YMLFile_php_log_pg_Comment = item.YMLFile_php_log_pg_Comment;
                                    yml.YMLFile_php_log_ms = item.YMLFile_php_log_ms;
                                    yml.YMLFile_php_log_ms_Comment = item.YMLFile_php_log_ms_Comment;
                                    yml.YMLFile_userportal_pg = item.YMLFile_userportal_pg;
                                    yml.YMLFile_userportal_pg_Comment = item.YMLFile_userportal_pg_Comment;
                                    yml.YMLFile_userportal_ms = item.YMLFile_userportal_ms;
                                    yml.YMLFile_userportal_ms_Comment = item.YMLFile_userportal_ms_Comment;
                                    yml.YMLFile_fer_log = item.YMLFile_fer_log;
                                    yml.YMLFile_fer_log_Comment = item.YMLFile_fer_log_Comment;
                                    yml.YMLFile_ac_mlo_pg = item.YMLFile_ac_mlo_pg;
                                    yml.YMLFile_ac_mlo_pg_Comment = item.YMLFile_ac_mlo_pg_Comment;
                                    yml.YMLFile_ac_mlo_ms = item.YMLFile_ac_mlo_ms;
                                    yml.YMLFile_ac_mlo_ms_Comment = item.YMLFile_ac_mlo_ms_Comment;
                                    yml.YMLFile_dev_PG = item.YMLFile_dev_PG;
                                    yml.YMLFile_dev_PG_Comment = item.YMLFile_dev_PG_Comment;
                                    yml.YMLFile_dev_MS = item.YMLFile_dev_MS;
                                    yml.YMLFile_dev_MS_Comment = item.YMLFile_dev_MS_Comment;
                                    yml.YMLFile_dev_LIS = item.YMLFile_dev_LIS;
                                    yml.YMLFile_dev_LIS_Comment = item.YMLFile_dev_LIS_Comment;
                                    yml.YMLFile_dev_EMD = item.YMLFile_dev_EMD;
                                    yml.YMLFile_dev_EMD_Comment = item.YMLFile_dev_EMD_Comment;
                                    yml.YMLFile_dev_log_service_pg = item.YMLFile_dev_log_service_pg;
                                    yml.YMLFile_dev_log_service_pg_Comment = item.YMLFile_dev_log_service_pg_Comment;
                                    yml.YMLFile_dev_log_service_ms = item.YMLFile_dev_log_service_ms;
                                    yml.YMLFile_dev_log_service_ms_Comment = item.YMLFile_dev_log_service_ms_Comment;
                                    yml.YMLFile_dev_php_log_pg = item.YMLFile_dev_php_log_pg;
                                    yml.YMLFile_dev_php_log_pg_Comment = item.YMLFile_dev_php_log_pg_Comment;
                                    yml.YMLFile_dev_php_log_ms = item.YMLFile_dev_php_log_ms;
                                    yml.YMLFile_dev_php_log_ms_Comment = item.YMLFile_dev_php_log_ms_Comment;
                                    yml.YMLFile_dev_userportal_pg = item.YMLFile_dev_userportal_pg;
                                    yml.YMLFile_dev_userportal_pg_Comment = item.YMLFile_dev_userportal_pg_Comment;
                                    yml.YMLFile_dev_userportal_ms = item.YMLFile_dev_userportal_ms;
                                    yml.YMLFile_dev_userportal_ms_Comment = item.YMLFile_dev_userportal_ms_Comment;
                                    yml.YMLFile_dev_fer_log = item.YMLFile_dev_fer_log;
                                    yml.YMLFile_dev_fer_log_Comment = item.YMLFile_dev_fer_log_Comment;
                                    yml.YMLFile_dev_ac_mlo_pg = item.YMLFile_dev_ac_mlo_pg;
                                    yml.YMLFile_dev_ac_mlo_pg_Comment = item.YMLFile_dev_ac_mlo_pg_Comment;
                                    yml.YMLFile_dev_ac_mlo_ms = item.YMLFile_dev_ac_mlo_ms;
                                    yml.YMLFile_dev_ac_mlo_ms_Comment = item.YMLFile_dev_ac_mlo_ms_Comment;
                                    yml.YMLFile_dev_smp2_pg = item.YMLFile_dev_smp2_pg;
                                    yml.YMLFile_dev_smp2_pg_Comment = item.YMLFile_dev_smp2_pg_Comment;
                                    yml.YMLFile_dev_gar_pg = item.YMLFile_dev_gar_pg;
                                    yml.YMLFile_dev_gar_pg_Comment = item.YMLFile_dev_gar_pg_Comment;
                                    yml.YMLFile_dev_proxy_pg = item.YMLFile_dev_proxy_pg;
                                    yml.YMLFile_dev_proxy_pg_Comment = item.YMLFile_dev_proxy_pg_Comment;
                                    yml.YMLFile_dev_bi = item.YMLFile_dev_bi;
                                    yml.YMLFile_dev_bi_Comment = item.YMLFile_dev_bi_Comment;

                                    yml.ErrorInfo = item.ErrorInfo;
                                    yml.IsRefreshed = true;
                                    isfind = true;
                                    break; //-V3020
                                };

                                if (!isfind)
                                {
                                    // добавляем
                                    item.isUpdated = "Добавлен при обновлении";
                                    item.IsRefreshed = true;
                                    ListYMLFiles.Add(item.Copy());
                                }

                            }
                            else
                            {
                                // добавляем
                                //lbCount.Content = "Добавление " + item.TaskNumber;
                                item.IsRefreshed = true;
                                ListYMLFiles.Add(item.Copy());
                            }
                        }

                        // Шаг 4 - удалим лишние
                        do
                        {
                            var yml = ListYMLFiles.Find(x => !x.IsRefreshed);
                            if (yml != null)
                                ListYMLFiles.Remove(yml);
                            else
                                break;

                        } while (true);

                        // Шаг 5 - Проверка yml-файла и включенных в него sql-файлов
                        /*if (isPreCheck)
                        {
                            List<YMLText> ListVersions = new List<YMLText>();

                            TaskListAction("проверку скриптов задач", ListYMLFiles,
                            before_all =>
                            {
                                pbProgressFill.Maximum = TaskArr.Count;
                                pbProgressFill.Minimum = 0;
                                pbProgressFill.Value = 0;
                                lbCount.Content = "";
                            },
                            task1 =>
                            {
                                if (string.IsNullOrWhiteSpace(_task) || TaskArr.ContainsKey(task1.TaskNumber))
                                {

                                    lbCount.Content = "Проверка " + task1.TaskNumber.Replace("_", "__");

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_PG))
                                    {
                                        task1.YMLFile_PG_Comment = CheckYMLSetComment("YMLFile_PG", task1.YMLFile_PG, ListYMLFiles, task1.YMLFile_PG_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_MS))
                                    {
                                        task1.YMLFile_MS_Comment = CheckYMLSetComment("YMLFile_MS", task1.YMLFile_MS, ListYMLFiles, task1.YMLFile_MS_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_LIS))
                                    {
                                        task1.YMLFile_LIS_Comment = CheckYMLSetComment("YMLFile_LIS", task1.YMLFile_LIS, ListYMLFiles, task1.YMLFile_LIS_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_EMD))
                                    {
                                        task1.YMLFile_EMD_Comment = CheckYMLSetComment("YMLFile_EMD", task1.YMLFile_EMD, ListYMLFiles, task1.YMLFile_EMD_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_log_service_pg))
                                    {
                                        task1.YMLFile_log_service_pg_Comment = CheckYMLSetComment("YMLFile_log_service_pg", task1.YMLFile_log_service_pg, ListYMLFiles, task1.YMLFile_log_service_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_log_service_ms))
                                    {
                                        task1.YMLFile_log_service_ms_Comment = CheckYMLSetComment("YMLFile_log_service_ms", task1.YMLFile_log_service_ms, ListYMLFiles, task1.YMLFile_log_service_ms_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_php_log_pg))
                                    {
                                        task1.YMLFile_php_log_pg_Comment = CheckYMLSetComment("YMLFile_php_log_pg", task1.YMLFile_php_log_pg, ListYMLFiles, task1.YMLFile_php_log_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_php_log_ms))
                                    {
                                        task1.YMLFile_php_log_ms_Comment = CheckYMLSetComment("YMLFile_php_log_ms", task1.YMLFile_php_log_ms, ListYMLFiles, task1.YMLFile_php_log_ms_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_userportal_pg))
                                    {
                                        task1.YMLFile_userportal_pg_Comment = CheckYMLSetComment("YMLFile_userportal_pg", task1.YMLFile_userportal_pg, ListYMLFiles, task1.YMLFile_userportal_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_userportal_ms))
                                    {
                                        task1.YMLFile_userportal_ms_Comment = CheckYMLSetComment("YMLFile_userportal_ms", task1.YMLFile_userportal_ms, ListYMLFiles, task1.YMLFile_userportal_ms_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_fer_log))
                                    {
                                        task1.YMLFile_fer_log_Comment = CheckYMLSetComment("YMLFile_fer_log", task1.YMLFile_fer_log, ListYMLFiles, task1.YMLFile_fer_log_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_ac_mlo_pg))
                                    {
                                        task1.YMLFile_ac_mlo_pg_Comment = CheckYMLSetComment("YMLFile_ac_mlo_pg", task1.YMLFile_ac_mlo_pg, ListYMLFiles, task1.YMLFile_ac_mlo_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_ac_mlo_ms))
                                    {
                                        task1.YMLFile_ac_mlo_ms_Comment = CheckYMLSetComment("YMLFile_ac_mlo_ms", task1.YMLFile_ac_mlo_ms, ListYMLFiles, task1.YMLFile_ac_mlo_ms_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_PG))
                                    {
                                        task1.YMLFile_dev_PG_Comment = CheckYMLSetComment("YMLFile_dev_PG", task1.YMLFile_dev_PG, ListYMLFiles, task1.YMLFile_dev_PG_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_MS))
                                    {
                                        task1.YMLFile_dev_MS_Comment = CheckYMLSetComment("YMLFile_dev_MS", task1.YMLFile_dev_MS, ListYMLFiles, task1.YMLFile_dev_MS_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_LIS))
                                    {
                                        task1.YMLFile_dev_LIS_Comment = CheckYMLSetComment("YMLFile_dev_LIS", task1.YMLFile_dev_LIS, ListYMLFiles, task1.YMLFile_dev_LIS_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_EMD))
                                    {
                                        task1.YMLFile_dev_EMD_Comment = CheckYMLSetComment("YMLFile_dev_EMD", task1.YMLFile_dev_EMD, ListYMLFiles, task1.YMLFile_dev_EMD_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_log_service_pg))
                                    {
                                        task1.YMLFile_dev_log_service_pg_Comment = CheckYMLSetComment("YMLFile_dev_log_service_pg", task1.YMLFile_dev_log_service_pg, ListYMLFiles, task1.YMLFile_dev_log_service_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_log_service_ms))
                                    {
                                        task1.YMLFile_dev_log_service_ms_Comment = CheckYMLSetComment("YMLFile_dev_log_service_ms", task1.YMLFile_dev_log_service_ms, ListYMLFiles, task1.YMLFile_dev_log_service_ms_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_php_log_pg))
                                    {
                                        task1.YMLFile_dev_php_log_pg_Comment = CheckYMLSetComment("YMLFile_dev_php_log_pg", task1.YMLFile_dev_php_log_pg, ListYMLFiles, task1.YMLFile_php_log_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_php_log_ms))
                                    {
                                        task1.YMLFile_dev_php_log_ms_Comment = CheckYMLSetComment("YMLFile_dev_php_log_ms", task1.YMLFile_dev_php_log_ms, ListYMLFiles, task1.YMLFile_dev_php_log_ms_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_userportal_pg))
                                    {
                                        task1.YMLFile_dev_userportal_pg_Comment = CheckYMLSetComment("YMLFile_dev_userportal_pg", task1.YMLFile_dev_userportal_pg, ListYMLFiles, task1.YMLFile_dev_userportal_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_userportal_ms))
                                    {
                                        task1.YMLFile_dev_userportal_ms_Comment = CheckYMLSetComment("YMLFile_dev_userportal_ms", task1.YMLFile_dev_userportal_ms, ListYMLFiles, task1.YMLFile_dev_userportal_ms_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_fer_log))
                                    {
                                        task1.YMLFile_dev_fer_log_Comment = CheckYMLSetComment("YMLFile_dev_fer_log", task1.YMLFile_dev_fer_log, ListYMLFiles, task1.YMLFile_dev_fer_log_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_ac_mlo_pg))
                                    {
                                        task1.YMLFile_dev_ac_mlo_pg_Comment = CheckYMLSetComment("YMLFile_dev_ac_mlo_pg", task1.YMLFile_dev_ac_mlo_pg, ListYMLFiles, task1.YMLFile_dev_ac_mlo_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_ac_mlo_ms))
                                    {
                                        task1.YMLFile_dev_ac_mlo_ms_Comment = CheckYMLSetComment("YMLFile_dev_ac_mlo_ms", task1.YMLFile_dev_ac_mlo_ms, ListYMLFiles, task1.YMLFile_dev_ac_mlo_ms_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_smp2_pg))
                                    {
                                        task1.YMLFile_dev_smp2_pg_Comment = CheckYMLSetComment("YMLFile_dev_smp2_pg", task1.YMLFile_dev_smp2_pg, ListYMLFiles, task1.YMLFile_dev_smp2_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(task1.YMLFile_dev_gar_pg))
                                    {
                                        task1.YMLFile_dev_gar_pg_Comment = CheckYMLSetComment("YMLFile_dev_gar_pg", task1.YMLFile_dev_gar_pg, ListYMLFiles, task1.YMLFile_dev_gar_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(item.YMLFile_dev_proxy_pg))
                                    {
                                        item.YMLFile_dev_proxy_pg_Comment = CheckYMLSetComment("YMLFile_dev_proxy_pg", item.YMLFile_dev_proxy_pg, ListYMLFiles, item.YMLFile_dev_proxy_pg_Comment, ref ListVersions);
                                    }

                                    if (!string.IsNullOrWhiteSpace(item.YMLFile_dev_bi))
                                    {
                                        task1.YMLFile_dev_bi_Comment = CheckYMLSetComment("YMLFile_dev_bi", task1.YMLFile_dev_bi, ListYMLFiles, task1.YMLFile_dev_bi_Comment, ref ListVersions);
                                    }
                                }
                            },
                            task2 =>
                            {
                                lbCount.Content = "Проверена " + task2.TaskNumber.Replace("_", "__");
                                pbProgressFill.Value = pbProgressFill.Value + 1;
                            },
                            null,
                            finish =>
                            {
                                // сброс фильтров и пересчет
                                cbFilterList1.SelectedIndex = 0;
                                cbFilterList2.SelectedIndex = 0;
                                cbFilterList3.SelectedIndex = 0;
                                tbFilterTask.Text = "";
                                btFindTask_Click(null, null);

                                isExecFill = false;

                                dgYMLFilesRefresh();

                                // выбрать последний
                                if (
                                    (!string.IsNullOrWhiteSpace(_task)) &&
                                    (dgYMLFiles.Items.Count > 0)
                                )
                                {
                                    dgYMLFiles.ScrollIntoView(dgYMLFiles.Items[dgYMLFiles.Items.Count - 1]);
                                }

                                // сохраняем задачу
                                if (mainWindow != null)
                                {
                                    mainWindow.SaveTaskNoShow();
                                    btSaveTask.Focus();
                                }

                                lbCount.Content = "";
                            }
                            ).GetAwaiter();
                        }
                        */
                    },
                    finish =>
                    {
                        // сброс фильтров и пересчет
                        cbFilterList1.SelectedIndex = 0;
                        cbFilterList2.SelectedIndex = 0;
                        cbFilterList3.SelectedIndex = 0;
                        tbFilterTask.Text = "";
                        btFindTask_Click(null, null);

                        isExecFill = false;

                        // если выбран проект - проставляем MergeStatus
                        if (
                            (cbGITProject.SelectedIndex != -1) &&
                            (cbGITProject.SelectedItem != null) &&
                            (!string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString()))
                        )
                        {
                            string project = cbGITProject.SelectedItem.ToString().Trim();

                            if (Utilities.GITProjects.IsDEVProject(project))
                            {
                                SetMerged(project, MainWindow.Task.ReleaseYMLFiles);
                            }
                        }

                        // обновим grid с учетом фильтров
                        dgYMLFilesRefresh();

                        // выбрать последний
                        if (
                            (!string.IsNullOrWhiteSpace(_task)) &&
                            (dgYMLFiles.Items.Count > 0)
                        )
                        {
                            dgYMLFiles.ScrollIntoView(dgYMLFiles.Items[dgYMLFiles.Items.Count - 1]);
                        }

                        // сохраняем задачу
                        if (mainWindow != null)
                        {
                            mainWindow.SaveTaskNoShow();
                            btSaveTask.Focus();
                        }

                        lbCount.Content = "";

                        // Включаем элементы интерфейса
                        Utilities.Controls.EnableOnFinish(mainGrid, listControls);
                    },
                    logFileRelease
                ).GetAwaiter();

            }
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Перебор yml в отдельном потоке и выполнение над ними действий
        /// </summary>
        /// <param name="_action_name">описание действия над yml-файлом</param>
        /// <param name="ListYMLFiles">список yml-файлов</param>
        /// <param name="_action_before_all">применить действие _action_before_all перед всеми yml</param>
        /// <param name="_action_yml">применить действие _action_yml к каждому yml</param>
        /// <param name="_action_after_yml">применить действие _action_after_yml после _action_yml к каждому yml</param>
        /// <param name="_action_after_all">применить действие _action_after_all поcле всех yml</param>
        /// <param name="_action_finish">применить финишное действие _action_finish при любом результате</param>
        public async System.Threading.Tasks.Task YMLListAction(string _action_name, List<YMLStruct> ListYMLFiles,
            System.Action<WinRelease> _action_before_all,
            System.Action<YMLStruct> _action_yml,
            System.Action<YMLStruct> _action_after_yml,
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

                // выполнить действие перед всеми yml
                if (_action_before_all != null)
                {
                    _action_before_all(this);
                }
            });

            foreach (var item in ListYMLFiles)
            {
                try
                {
                    // выполнить действие с экземпляром yml
                    if (_action_yml != null)
                    {
                        await System.Threading.Tasks.Task.Run(() =>
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _action_yml(item);
                            })
                         );
                    }

                    // выполнить действие с экземпляром yml
                    if (_action_after_yml != null)
                    {
                        await System.Threading.Tasks.Task.Run(() =>
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                _action_after_yml(item);
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
                // выполнить действие после всех yml
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

        /// <summary>Подсчет итогов</summary>
        public string TaskCalcCount()
        {
            int YmlCount = 0;
            int YmlCountRelease = 0;
            int TaskCount = 0;

            string result = tbTasks.Text
               .Trim()
               .ToUpper()
               .Replace(';', ',')
               .Replace('\r', ',')
               .Replace('\n', ',')
               .Replace('\t', ',')
               .Replace(' ', ',')
               .Replace('+', ',')
               .Replace("https://jira.is-mis.ru/browse/".ToUpper(), "")
               .Replace("https://jira.rtmis.ru/browse/".ToUpper(), "")
               .Trim()
               .Replace(',', '\n');

            while (result.Contains("\n\n"))
            {
                result = result.Replace("\n\n", "\n");
            }

            result = result.Trim('\n');

            if (MainWindow.Task.ReleaseYMLFiles.Count() != 0)
            {
                foreach (var item in MainWindow.Task.ReleaseYMLFiles.Where(x =>
                    (x.IsFiltered1 == true) &&
                    (x.IsFiltered2 == true) &&
                    (x.IsFiltered3 == true) &&
                    (x.IsFiltered4 == true)
                    ))
                {
                    YmlCount++;
                    if (item.IsAddRelease)
                    {
                        YmlCountRelease++;
                    }
                }

                TaskCount = MainWindow.Task.ReleaseYMLFiles.Where(x =>
                    (x.IsFiltered1 == true) &&
                    (x.IsFiltered2 == true) &&
                    (x.IsFiltered3 == true) &&
                    (x.IsFiltered4 == true)
                    ).Select(x => x.TaskNumber).Distinct().Count();

                lbCount.Content = "Всего задач " + TaskCount.ToString() + ", файлов " + YmlCount.ToString() + ", из них включаем в релиз " + YmlCountRelease.ToString();
            }
            else
            {
                var TaskArr = result.Split('\n');

                foreach (var item in TaskArr)
                {
                    if (item != "")
                    {
                        TaskCount++;
                    }
                }

                lbCount.Content = "Всего задач " + TaskCount.ToString();
            }

            return result;
        }

        /// <summary>Нажата кнопка Заполнить список YML-файлов</summary>
        private void btFill_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return;
            }

            if (isExecFill == true)
            {
                MessageBox.Show("Идет парсинг задач из Jira, подождите!");
                return;
            }

            FillYMLFiles(tbTasks.Text.Trim(), MainWindow.Task.ReleaseYMLFiles, false);
        }

        /// <summary>
        /// Заполнить по URL из Jira
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFillURL_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return;
            }

            if (isExecFill == true)
            {
                MessageBox.Show("Идет парсинг задач из Jira, подождите!");
                return;
            }

            if (System.Windows.Forms.MessageBox.Show($"Загрузить из Jira список задач по номеру версии {tbNumVersion.Text} ?",
                           "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
            {
                return;
            }

            bool isRefresh = false;

            if (
                    (!string.IsNullOrWhiteSpace(tbTasks.Text)) ||
                    (MainWindow.Task.ReleaseYMLFiles.Count > 0)
                )
            {
                isRefresh = System.Windows.Forms.MessageBox.Show($"Список задач уже заполнен.\nДА - добавить новые задачи, НЕТ - очистить и заполнить заново?",
                           "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes;
            }

            if (!isRefresh)
            {
                tbTasks.Text = "";
                MainWindow.Task.ReleaseYMLFiles.Clear();
            }

            // парсинг списка задач по URL из Jira
            if (
            (!string.IsNullOrWhiteSpace(MainWindow.Task.TaskUrl)) &&
            (!string.IsNullOrWhiteSpace(btFillURL.Content.ToString())) &&
            HTML.OpenLoginJira(logFileRelease)
            )
            {
                HTML html = new HTML();

                string url = btFillURL.Content.ToString().Trim();

                // Отключаем элементы интерфейса
                var listControls = Utilities.Controls.DisabaleOnStart(mainGrid, null);

                html.LoadTaskListJiraPages(url, this,
                    x =>
                    {
                        // Включаем элементы интерфейса
                        Utilities.Controls.EnableOnFinish(mainGrid, listControls);

                        if (x.TaskList.Count() > 0)
                        {
                            string NumTask = string.Join("\n", x.TaskList);

                            string ListTask = "";
                            if (isRefresh)
                            {
                                ListTask = AddNewTasks(NumTask);
                            }
                            else
                            {
                                tbTasks.Text = NumTask;
                            }

                            // парсинг самих задач
                            FillYMLFiles(tbTasks.Text.Trim(), MainWindow.Task.ReleaseYMLFiles, isRefresh, ListTask);
                        }
                        else
                        {
                            App.AddLog("Не удалось получить список задач!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                        }
                    },
                    null,
                    logFileRelease
                ).GetAwaiter();
            }
        }

        /// <summary>
        /// Выбор ячейки
        /// </summary>
        private void SelectCell()
        {
            if ((dgYMLFiles.SelectedCells.Count > 0) && (dgYMLFiles.CurrentColumn != null))
            {
                var CellValue = Utilities.Controls.GetSelectedValue(dgYMLFiles, dgYMLFiles.CurrentColumn.DisplayIndex);

                if (dgYMLFiles.CurrentColumn.DisplayIndex == 2)
                {
                    tbSelectedCell.Text = MainWindow.APPinfo.TaskUrlDefault + CellValue;
                }
                else
                {
                    tbSelectedCell.Text = CellValue;
                }
            }
        }

        /// <summary>Изменилось содержимое ячейки в таблице YML-файлов</summary>
        private void dgYMLFiles_CurrentCellChanged(object sender, EventArgs e)
        {
            TaskCalcCount();
            SelectCell();
        }

        /// <summary>
        /// Показать файл в целевой ветке GIT
        /// </summary>
        /// <param name="default_branch">целевая ветка</param>
        private void ViewFileInGIT(string default_branch)
        {
            if (dgYMLFiles.CurrentColumn != null)
            {
                var field = dgYMLFiles.CurrentColumn.SortMemberPath;

                if ((dgYMLFiles.SelectedCells.Count > 0) && field.Contains("YMLFile_"))
                {
                    // получаем имя файла
                    var CellValue = Utilities.Controls.GetSelectedValue(dgYMLFiles, dgYMLFiles.CurrentColumn.DisplayIndex) ?? "";
                    CellValue = CellValue.Trim();

                    // очищаем имя файла
                    var arrf = CellValue.Split('.');

                    string branch = default_branch;

                    if (string.IsNullOrWhiteSpace(default_branch))
                    {
                        branch = arrf[0];
                    }

                    if (arrf.Length > 1)
                    {
                        if (arrf[1].StartsWith("yml"))
                        {
                            CellValue = arrf[0] + ".yml";
                        }
                        else if (arrf[1].StartsWith("json"))
                        {
                            CellValue = arrf[0] + ".json";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(CellValue))
                    {
                        var col_path = dgYMLFiles.Columns.Where(x => x.SortMemberPath == "PathInGIT").FirstOrDefault();
                        string path = "";
                        if (col_path != null)
                        {
                            path = Utilities.Controls.GetSelectedValue(dgYMLFiles, col_path.DisplayIndex);
                        }

                        if (string.IsNullOrWhiteSpace(default_branch))
                        {
                            var col_branch = dgYMLFiles.Columns.Where(x => x.SortMemberPath == "BranchName").FirstOrDefault();
                            if (col_branch != null)
                            {
                                branch = Utilities.Controls.GetSelectedValue(dgYMLFiles, col_branch.DisplayIndex);
                            }
                        }

                        // собираем url
                        string project = Utilities.GITProjects.GetProjectByYMLField(field);
                        string url = Utilities.GITProjects.GetURLByProject(project)
                            .Replace("%BRANCH%", branch) +
                            (string.IsNullOrWhiteSpace(path) ? "" : path + "/");

                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            System.Diagnostics.Process.Start(url + CellValue);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Показать текст файла в ветке версии в GIT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgYMLFiles_MenuItemShowYMLinVer(object sender, RoutedEventArgs e)
        {
            if (dgYMLFiles.CurrentColumn != null)
            {
                var field = dgYMLFiles.CurrentColumn.SortMemberPath;

                if ((dgYMLFiles.SelectedCells.Count > 0) && field.Contains("YMLFile_"))
                {
                    string branch = tbBranch.Text.Trim();

                    if (string.IsNullOrWhiteSpace(branch))
                    {
                        string prefix = Utilities.GITProjects.GITProjectsParam("GITYMLField", field, "PrefixFileRelease");
                        branch = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);
                    }

                    ViewFileInGIT(branch);
                }
            }
        }

        /// <summary>Двойной клик мышкой на ячейке в таблице YML-файлов</summary>
        private void dgYMLFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (dgYMLFiles.CurrentColumn != null)
            {
                var field = dgYMLFiles.CurrentColumn.SortMemberPath;

                if ((dgYMLFiles.SelectedCells.Count > 0) && field.Contains("YMLFile_"))
                {
                    // Показать текст файла в ветке задачи в GIT
                    ViewFileInGIT("");
                }

                if ((dgYMLFiles.SelectedCells.Count > 0) && (field == "TaskNumber"))
                {
                    var CellValue = Utilities.Controls.GetSelectedValue(dgYMLFiles, dgYMLFiles.CurrentColumn.DisplayIndex);

                    if (!string.IsNullOrWhiteSpace(CellValue))
                    {
                        // Показать задачу в Jira
                        System.Diagnostics.Process.Start(MainWindow.APPinfo.TaskUrlDefault + CellValue);
                    }
                }

                if ((dgYMLFiles.SelectedCells.Count > 0) && (field == "BranchName"))
                {
                    var CellValue = Utilities.Controls.GetSelectedValue(dgYMLFiles, dgYMLFiles.CurrentColumn.DisplayIndex);

                    if (!string.IsNullOrWhiteSpace(CellValue))
                    {
                        if (cbGITProject.SelectedIndex != -1)
                        {
                            // Показать ветку задачу в GIT
                            string project = cbGITProject.SelectedItem.ToString().Trim();
                            string url = Utilities.GITProjects.GetURLByProject(project)
                                .Replace("%BRANCH%", CellValue)
                                .TrimEnd(new char[] { '/' });

                            System.Diagnostics.Process.Start(url);
                        }
                    }
                }

            }
        }

        /// <summary>
        /// генерация yml-файла
        /// </summary>
        /// <param name="isNew">=true - новый, =false - добавить в существующий</param>
        private void GenerateYML(bool isNew)
        {
            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string postfix = Utilities.GITProjects.GetPostfixFileReleaseByProject(project);
            string version = Release.GetNumVersion(prefix, tbNumVersion.Text);
            Encoding encoding = new UTF8Encoding(false);
            double numversion = Release.VerAsNum(version);

            // Файл выбранной предыдущей версии
            string prevfile = "";

            if (
                (cbPrevVersion.SelectedIndex > -1) &&
                (!string.IsNullOrWhiteSpace(cbPrevVersion.SelectedItem.ToString()))
                )
            {
                prevfile = cbPrevVersion.SelectedItem.ToString().Trim();

                if (Utilities.GITProjects.IsDEVProject(project))
                {
                    // имя файла версии по имени ветки
                    string prevver = Release.GetNumVersion(prefix, prevfile);
                    double prevnum = Release.VerAsNum(prevver);
                    if (Versions.ContainsKey(prevnum))
                    {
                        prevfile = Versions[prevnum].File;
                    }
                }
            }

            // changeset до
            YMLChangeset text_before = new YMLChangeset()
            {
                id = $"Version_{Release.GetNumVersion(prefix, tbNumVersion.Text)}{postfix}_begin",
                author = MainWindow.Task.TaskExecutor,
                comment = $"{prefix}.{Release.GetNumVersion(prefix, tbNumVersion.Text)}{postfix}"
            };

            // changeset после
            YMLChangeset text_after = new YMLChangeset()
            {
                id = $"Version_{Release.GetNumVersion(prefix, tbNumVersion.Text)}{postfix}_end",
                author = MainWindow.Task.TaskExecutor,
                comment = $"{prefix}.{Release.GetNumVersion(prefix, tbNumVersion.Text)}{postfix}"
            };

            if (isImproveSQLinVersionRelease)
            {
                text_after.labels = "finish";
            }

            string ff = "";

            var files = Directory.GetFiles(path, "*." + version + "_*.yml").ToList();
            if (files == null) files = new List<string>(); //-V3022
            files.AddRange(Directory.GetFiles(path, "*." + version + ".yml").ToList());
            foreach (var file in files)
            {
                if (
                    (!file.ToLower().Contains("_rpt")) &&
                    (!file.ToLower().Contains("_ots")) &&
                    (numversion == Release.VerAsNum(Release.GetNumVersion(prefix, Path.GetFileName(file)))) //-V3024
                    )
                {
                    ff = Path.Combine(path, file);

                    if (System.Windows.Forms.MessageBox.Show($"YML-файл {file} с версией {tbNumVersion.Text.Trim()} уже есть в папке {path}" + Environment.NewLine +
                        Environment.NewLine +
                        $"Добавить новые задачи (если они есть) в существующий YML-файл ?",
                        "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        App.AddLog($"Выбрано добавление новых задач в файл " + file + " в папке " + path + $" с версией {tbNumVersion.Text.Trim()}", null, App.ShowMessageMode.NONE, true, logFileRelease);

                        //isNew = false;

                        // загружаем yml-файл, добавляем новые задачи
                        var loadyml = Release.AddTasksToYML(
                            project,
                            ff,
                            MainWindow.Task.ReleaseYMLFiles
                                .Where(x => x.IsAddRelease == true && x.PathInGIT.ToLower() == "task")
                                .OrderBy(x => x.YMLOrder)
                                .ToList<YMLFileInfo>(),
                            text_before,
                            text_after,
                            isNew,
                            logFileRelease
                        );

                        // добавляем include предыдущей версии для GIT - проекта
                        if (!string.IsNullOrWhiteSpace(prevfile))
                        {
                            // удаляем существующие
                            foreach (var item in loadyml.Lines.Where(x => x.type == YMLLineType.VERSION).ToList())
                            {
                                loadyml.DeleteYML(item);
                            }

                            // добавляем новые - только если в существующем файле нет проверочного changeset с именем cumulative_gap
                            if (!loadyml.hasCumulativeGap)
                            {
                                loadyml.Lines.Add(new YMLLine(loadyml, logFileRelease)
                                {
                                    type = YMLLineType.VERSION,
                                    isLoaded = false,
                                    text = "",
                                    file = prevfile,
                                    order = -1,
                                    path = "version",
                                    relativeToChangelogFile = loadyml.relativeToChangelogFile
                                });
                            }
                        }

                        // учитываем признак кумулятивности
                        loadyml.IsNoCumulative = isCumulative.IsChecked != true;

                        // генерация yml-файл
                        loadyml.SaveYML(false, false, MainWindow.APPinfo.relativeToChangelogFile == "true", true, false, isImproveSQLinVersionRelease, "", loadyml.IsNoCumulative, version);

                        // если перешли на пути относительно корня проекта или улучшаем скрипты релиза - надо исправить yml-файлы задач
                        if (MainWindow.APPinfo.relativeToChangelogFile == "false" || isImproveSQLinVersionRelease)
                        {
                            // надо обновить вложенные задачи
                            loadyml.ReLoadYML(false, null, true, true);
                            loadyml.SaveYML(false, true, MainWindow.APPinfo.relativeToChangelogFile == "true", true, true, isImproveSQLinVersionRelease, "", loadyml.IsNoCumulative, version);
                        }

                        // обновляем в списке версий
                        if (Versions.ContainsKey(numversion))
                        {
                            Versions[numversion].YMLFile = loadyml;
                        }
                        else
                        {
                            Version ver = new Version(logFileRelease) { Branch = tbBranch.Text, YMLFile = loadyml };
                            Versions.Add(numversion, ver);
                        }

                        // завершение            
                        if (System.Windows.Forms.MessageBox.Show("Добавлены новые задачи в файл " + file + " в папке " + path + $" с версией {tbNumVersion.Text.Trim()}" +
                            Environment.NewLine + Environment.NewLine +
                            "Открыть файл для просмотра результата ?",
                            "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            App.AddLog("Добавлены новые задачи в файл " + file + " в папке " + path + $" с версией {tbNumVersion.Text.Trim()}, открытие внешнего файла", null, App.ShowMessageMode.NONE, true, logFileRelease);
                            Utilities.External.OpenExternalFile(ff);
                        }
                        else
                        {
                            App.AddLog($"Добавлены новые задачи в файл " + file + " в папке " + path + $" с версией {tbNumVersion.Text.Trim()}", null, App.ShowMessageMode.NONE, true, logFileRelease);
                        }
                    }
                    else
                    {
                        if (System.Windows.Forms.MessageBox.Show($"YML-файл {file} с версией {tbNumVersion.Text.Trim()} уже есть в папке {path}" + Environment.NewLine +
                            Environment.NewLine +
                            "Открыть файл для просмотра результата ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            App.AddLog($"YML-файл {ff} с версией {tbNumVersion.Text.Trim()} для проекта {project} уже существует, открытие внешнего файла", null, App.ShowMessageMode.NONE, true, logFileRelease);
                            Utilities.External.OpenExternalFile(ff);
                        }
                        else
                        {
                            App.AddLog($"YML-файл {ff} с версией {tbNumVersion.Text.Trim()} для проекта {project} уже существует", null, App.ShowMessageMode.NONE, true, logFileRelease);
                        }
                    }

                    return;
                }
            }

            string filename = tbFileVersion.Text.Trim();
            ff = Path.Combine(path, filename);

            if (File.Exists(ff))
            {
                App.AddLog("YML-файл " + ff + " уже существует !", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                tbFileVersion.Focus();
                return;
            }

            // собираем новый файл с версией
            var newyml = Release.AddTasksToYML(
                                project,
                                ff,
                                MainWindow.Task.ReleaseYMLFiles
                                    .Where(x => x.IsAddRelease == true && x.PathInGIT.ToLower() == "task")
                                    .OrderBy(x => x.YMLOrder)
                                    .ToList<YMLFileInfo>(),
                                text_before,
                                text_after,
                                isNew,
                                logFileRelease
                            );

            // добавляем include предыдущей версии для GIT-проекта
            if (!string.IsNullOrWhiteSpace(prevfile))
            {
                // удаляем существующие
                foreach (var item in newyml.Lines.Where(x => x.type == YMLLineType.VERSION).ToList())
                {
                    newyml.DeleteYML(item);
                }

                // добавляем новые
                newyml.Lines.Add(new YMLLine(newyml, logFileRelease)
                {
                    type = YMLLineType.VERSION,
                    isLoaded = false,
                    text = "",
                    file = prevfile,
                    order = -1,
                    path = "version",
                    relativeToChangelogFile = newyml.relativeToChangelogFile
                });
            }

            // учитываем признак кумулятивности
            newyml.IsNoCumulative = isCumulative.IsChecked != true;

            // генерация yml-файл
            newyml.SaveYML(false, false, MainWindow.APPinfo.relativeToChangelogFile == "true", true, false, isImproveSQLinVersionRelease, "", newyml.IsNoCumulative, version);

            // если перешли на пути относительно корня проекта или улучшаем скрипты релиза - надо исправить yml-файлы задач
            if (MainWindow.APPinfo.relativeToChangelogFile == "false" || isImproveSQLinVersionRelease)
            {
                newyml.ReLoadYML(false, null, true, true);
                newyml.SaveYML(false, true, MainWindow.APPinfo.relativeToChangelogFile == "true", true, true, isImproveSQLinVersionRelease, "", newyml.IsNoCumulative, version);
            }

            // обновляем в списке версий
            if (Versions.ContainsKey(numversion))
            {
                Versions[numversion].YMLFile = newyml;
            }
            else
            {
                Version ver = new Version(logFileRelease) { Branch = tbBranch.Text, YMLFile = newyml };
                Versions.Add(numversion, ver);
            }

            // завершение            
            if (System.Windows.Forms.MessageBox.Show("Релиз собран в файл " + filename + " в папке " + path + "." + Environment.NewLine + "Открыть ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                App.AddLog($"Версия {tbNumVersion.Text.Trim()} для проекта {project} собрана в новый файл {ff}, открытие внешнего файла", null, App.ShowMessageMode.NONE, true, logFileRelease);
                Utilities.External.OpenExternalFile(ff);
            }
            else
            {
                App.AddLog($"Версия {tbNumVersion.Text.Trim()} для проекта {project} собрана в новый файл {ff}", null, App.ShowMessageMode.NONE, true, logFileRelease);
            }
        }

        /// <summary>Нажата кнопка Собрать релиз</summary>
        private void btGenerate_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;
            if (!CheckMerged()) return;

            if (System.Windows.Forms.MessageBox.Show($"Заново собрать версию {tbNumVersion.Text} по списку задач из Jira ?",
            "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                GenerateYML(true);
            }
        }

        /// <summary>Нажата кнопка Добавить в релиз</summary>
        private void btGenerateAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;
            if (!CheckMerged()) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string file = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version", tbFileVersion.Text.Trim());

            if (!File.Exists(file))
            {
                App.AddLog($"Файл {file} не существует!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
            }
            else
            {
                GenerateYML(false);
            }
        }

        /// <summary>Нажата клавиша в таблице YML-файлов</summary>
        private void dgYMLFiles_KeyDown(object sender, KeyEventArgs e)
        {
            /* if (e.Key == Key.Space)
             {
                 YMLFileInfo Row = (YMLFileInfo)dgYMLFiles.SelectedItem;
                 Row.IsAddRelease = !Row.IsAddRelease;
             }*/
        }

        /// <summary>
        /// проверить скрипты yml-файла версии
        /// </summary>
        /// <param name="yml">yml-файл конкретной задачи</param>
        public void CheckYML(YMLStruct yml)
        {
            string Errors = "";
            bool isError = false;

            tbNumVersion.Focus();
            btCheckYML.Focus();

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);

            string YMLFile = "";
            string fullfilename = "";

            if (yml == null)
            {
                YMLFile = tbFileVersion.Text.Trim();
                if (!YMLFile.ToLower().EndsWith(".yml")) YMLFile += ".yml";
                fullfilename = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version", YMLFile);
            }
            else
            {
                YMLFile = yml.Filename;
                if (!YMLFile.ToLower().EndsWith(".yml")) YMLFile += ".yml";
                fullfilename = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, yml.Filepath, YMLFile);
            }

            this.Cursor = Cursors.Wait;

            if (!string.IsNullOrWhiteSpace(ProjectFolder))
            {
                if (!File.Exists(fullfilename))
                {
                    App.AddLog($"Файл {YMLFile} НЕ существует !", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                    tbFileVersion.Focus();
                    return;
                }

                // Отключаем элементы интерфейса
                var listControls = Utilities.Controls.DisabaleOnStart(mainGrid, null);

                // Проверка yml-файлов версии и включенных в них sql-файлов
                List<YMLText> ListVersions = new List<YMLText>();
                List<YMLStruct> ListYMLFiles = new List<YMLStruct>();

                if (yml == null)
                {
                    // загружаем yml-файл версии
                    YMLStruct loadyml = new YMLStruct(null, logFileRelease);
                    loadyml.LoadYML(project, "version", YMLFile, false, null, true, false);
                    ListYMLFiles = loadyml.ListYMLStruct(false);
                }
                else
                {
                    ListYMLFiles.Add(yml);
                }

                lbCount.Content = "Читаем файл " + YMLFile.Replace("_", "__");

                YMLListAction("проверку yml-файлов версии", ListYMLFiles,
                before_all =>
                {
                    pbProgressFill.Maximum = ListYMLFiles.Count;
                    pbProgressFill.Minimum = 0;
                    pbProgressFill.Value = 0;
                    lbCount.Content = "";
                },
                yml1 =>
                {
                    btCheckYML.IsEnabled = false;
                    lbCount.Content = "Проверка " + yml1.Filename.Replace("_", "__");
                },
                yml2 =>
                {
                    yml2.CheckYML(false, isCorrectEncoding.IsChecked == true, isCheckBOM.IsChecked == true, MainWindow.Task.ReleaseYMLFiles.Where(x => x.PathInGIT.ToLower() == "task").ToList(), ref Errors, ref isError, true, true, ref ListVersions, isImproveSQLinVersionRelease);

                    pbProgressFill.Value = pbProgressFill.Value + 1;
                    lbCount.Content = "Проверена " + yml2.Filename.Replace("_", "__");
                },
                null,
                finish =>
                {
                    this.Cursor = Cursors.Arrow;

                    if (isError)
                    {
                        WinInfo WinInfo = new WinInfo(logFileRelease);
                        WinInfo.Title = "Есть ошибки в скриптах!!! Лог проверки в файле " + logFileRelease;
                        WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                        WinInfo.tbInfo.Text = Errors;
                        WinInfo.Show();
                    }
                    else
                    {
                        WinInfo WinInfo = new WinInfo(null);
                        WinInfo.Title = "Результат проверки";
                        WinInfo.tbInfo.Text = "Файлы проверены, ошибок нет!" + Environment.NewLine + Errors;
                        WinInfo.Show();
                    }

                    btCheckYML.IsEnabled = true;
                    lbCount.Content = "";

                    // Включаем элементы интерфейса
                    Utilities.Controls.EnableOnFinish(mainGrid, listControls);
                }
                ).GetAwaiter();
            }
        }

        /// <summary>Нажата кнопка Проверить скрипты релиза</summary>
        private void btCheckYML_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;
            if (!CheckMerged()) return;

            CheckYML(null);
        }

        /// <summary>Изменилось значение в поле "Список задач для включения в релиз"</summary>
        private void tbTasks_TextChanged(object sender, TextChangedEventArgs e)
        {
            MainWindow.Task.ReleaseTaskList = TaskCalcCount();
        }

        /// <summary>Изменилось значение в поле "Имя файла версии"</summary>
        private void tbFileVersion_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (mainWindow != null)
            {
                mainWindow.tbYMLFile.Text = tbFileVersion.Text.Trim();
            }

            if (cbGITProject.SelectedIndex != -1)
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();
                string URL = Utilities.GITProjects.GetURLVersionByProject(project);
                URL = URL.Replace("%BRANCH%", tbBranch.Text);
                tbGITVersion.Text = URL + tbFileVersion.Text.Trim();

                // Команды бота
                Fill_cbLiquibot(project);
            }
        }

        /// <summary>Нажата кнопка Выгрузить в Excel</summary>
        private void btExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Utilities.MSOffice.GenerateExcel(Utilities.Databases.ConvertToDataTable(MainWindow.Task.ReleaseYMLFiles), false, "", true);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFileRelease);
            }
        }

        /// <summary>Выбор строки в dgYMLFiles</summary>
        private void dgYMLFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgYMLFiles.SelectedItem == null)
            {
                tbTaskNum.Text = "";
                tbTaskStatus.Text = "";
                tbTaskVersion.Text = "";
                tbTaskDT.Text = "";
                tbTaskBaseRegion.Text = "";
                tbTaskRegion.Text = "";
                tbTaskUpdActions.Text = "";
                tbTaskDataBD.Text = "";
                tbTaskObjectsBD.Text = "";
                tbSelectedCell.Text = "";
            }
            else
            {
                var row = (YMLFileInfo)dgYMLFiles.SelectedItem;
                tbTaskNum.Text = row.TaskNumber;
                tbTaskStatus.Text = row.TaskStatus;
                tbTaskVersion.Text = row.Version;
                if (row.IsDowntime) tbTaskDT.Text = "Требуется Downtime";
                else tbTaskDT.Text = "";
                if (row.IsBaseRegion) tbTaskBaseRegion.Text = "Базовая региональность БД";
                else tbTaskBaseRegion.Text = "";
                tbTaskRegion.Text = row.Region;
                tbTaskUpdActions.Text = row.UpdActions;
                tbTaskDataBD.Text = row.DataBD;
                tbTaskObjectsBD.Text = row.ObjectsBD;

                SelectCell();

            }
        }

        /// <summary>Разрешение действия по умолчанию при нажатии клавиши в dgYMLFiles</summary>
        private void dgYMLFiles_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Device.Target.GetType().Name == "DataGridCell") && (dgYMLFiles.SelectedCells.Count > 0))
            {
                if (e.Key == Key.Delete)
                {
                    string CellValue = "";
                    foreach (var col in dgYMLFiles.Columns)
                    {
                        if (col.SortMemberPath == "TaskNumber")
                        {
                            CellValue = Utilities.Controls.GetSelectedValue(dgYMLFiles, col.DisplayIndex);
                            break;
                        }
                    }
                    MessageBoxResult res = MessageBox.Show("Удалить задачу " + CellValue + "?", "ВНИМАНИЕ!", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (res == MessageBoxResult.Yes)
                    {
                        App.AddLog($"Выбрано удаление задачи {CellValue} из списка задач", null, App.ShowMessageMode.NONE, true, logFileRelease);
                    }

                    e.Handled = (res != MessageBoxResult.Yes);
                }
            }
        }

        /// <summary>пункт меню Удалить задачу в dgYMLFiles</summary>
        private void dgYMLFiles_MenuItemDelete(object sender, RoutedEventArgs e)
        {
            if (dgYMLFiles.SelectedIndex >= 0)
            {
                var _yml = dgYMLFiles.SelectedItem as YMLFileInfo;
                MainWindow.Task.ReleaseYMLFiles.Remove(_yml);
                dgYMLFilesRefresh();

                // сохраняем задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                    btSaveTask.Focus();
                }
            }
        }

        /// <summary>Выбор Фильтр 1</summary>
        private void cbFilterList1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFilterList1.SelectedIndex >= 0)
            {
                string project = "";

                if (cbFilterList1.SelectedItem.ToString().Trim() == "Выбранный проект")
                {
                    if (cbGITProject.SelectedIndex != -1)
                    {
                        project = Utilities.GITProjects.GetGITProject(cbGITProject.SelectedItem.ToString().Trim());
                    }
                }
                else
                {
                    project = cbFilterList1.SelectedItem.ToString().Trim().Replace("Проект:", "").Trim();
                }

                string ymlfield = "";

                if (project == "неизвестный")
                {
                    ymlfield = "YMLFile_unknown";
                }
                else
                {
                    ymlfield = Utilities.GITProjects.GetYMLFieldByProject(project);
                }

                Regex regex_rpt = new Regex(@"rpt\d*\.");

                foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                {
                    item.IsFiltered1 = false;

                    if (cbFilterList1.SelectedItem.ToString().Trim() == "ВСЕ")
                    {
                        item.IsFiltered1 = true;
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Выбранный проект")
                    {
                        item.IsFiltered1 =
                            (item.IsAddRelease == true) &&
                            (
                                ((ymlfield == "YMLFile_PG") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_PG)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_PG))
                                    )) ||
                                ((ymlfield == "YMLFile_MS") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_MS)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_MS))
                                    )) ||
                                ((ymlfield == "YMLFile_LIS") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_LIS)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_LIS))
                                    )) ||
                                ((ymlfield == "YMLFile_EMD") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_EMD)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_EMD))
                                    )) ||
                                ((ymlfield == "YMLFile_log_service_pg") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_log_service_pg)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_log_service_pg))
                                    )) ||
                                ((ymlfield == "YMLFile_log_service_ms") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_log_service_ms)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_log_service_ms))
                                    )) ||
                                ((ymlfield == "YMLFile_php_log_pg") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_php_log_pg)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_php_log_pg))
                                    )) ||
                                ((ymlfield == "YMLFile_php_log_ms") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_php_log_ms)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_php_log_ms))
                                    )) ||
                                ((ymlfield == "YMLFile_userportal_pg") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_userportal_pg)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_userportal_pg))
                                    )) ||
                                ((ymlfield == "YMLFile_userportal_ms") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_userportal_ms)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_userportal_ms))
                                    )) ||
                                ((ymlfield == "YMLFile_fer_log") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_fer_log)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_fer_log))
                                    )) ||
                                ((ymlfield == "YMLFile_ac_mlo_pg") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_ac_mlo_pg)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_ac_mlo_pg))
                                    )) ||
                                ((ymlfield == "YMLFile_ac_mlo_ms") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_ac_mlo_ms)) ||
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_ac_mlo_ms))
                                    )) ||
                                ((ymlfield == "YMLFile_smp2_pg") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_smp2_pg))
                                    )) ||
                                ((ymlfield == "YMLFile_gar_pg") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_gar_pg))
                                    )) ||
                                ((ymlfield == "YMLFile_proxy_pg") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_proxy_pg))
                                    )) ||
                                ((ymlfield == "YMLFile_bi") && (
                                    (!string.IsNullOrWhiteSpace(item.YMLFile_dev_bi))
                                    ))
                            )
                            ;
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Изменившиеся после обновления")
                    {
                        item.IsFiltered1 = !string.IsNullOrWhiteSpace(item.isUpdated);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Включено в релиз")
                    {
                        item.IsFiltered1 = item.IsAddRelease == true;
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "НЕ включено в релиз")
                    {
                        item.IsFiltered1 = !(item.IsAddRelease == true);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Регион не указан")
                    {
                        item.IsFiltered1 = string.IsNullOrWhiteSpace(item.Region);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Регион БАЗОВЫЙ")
                    {
                        item.IsFiltered1 = (item.Region != null) && item.Region.ToUpper().Contains("БАЗОВЫЙ"); //-V3063
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Регион НЕ базовый")
                    {
                        item.IsFiltered1 = (!string.IsNullOrWhiteSpace(item.Region)) && (!item.Region.ToUpper().Contains("БАЗОВЫЙ"));
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Есть Базовая региональность БД")
                    {
                        item.IsFiltered1 = item.IsBaseRegion == true;
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Нет Базовая региональность БД")
                    {
                        item.IsFiltered1 = !(item.IsBaseRegion == true);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Есть Downtime")
                    {
                        item.IsFiltered1 = item.IsDowntime == true;
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Нет Downtime")
                    {
                        item.IsFiltered1 = !(item.IsDowntime == true);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Есть Действия при обновлении")
                    {
                        item.IsFiltered1 = !string.IsNullOrWhiteSpace(item.UpdActions);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Нет Действия при обновлении")
                    {
                        item.IsFiltered1 = string.IsNullOrWhiteSpace(item.UpdActions);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Есть Данные в БД")
                    {
                        item.IsFiltered1 = !string.IsNullOrWhiteSpace(item.DataBD);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Нет Данные в БД")
                    {
                        item.IsFiltered1 = string.IsNullOrWhiteSpace(item.DataBD);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Есть Объекты БД")
                    {
                        item.IsFiltered1 = !string.IsNullOrWhiteSpace(item.ObjectsBD);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Нет Объекты БД")
                    {
                        item.IsFiltered1 = string.IsNullOrWhiteSpace(item.ObjectsBD);
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Задача без yml-файла")
                    {
                        item.IsFiltered1 = item.IsYMLFileNotExist;
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "Задача с yml-файлом")
                    {
                        item.IsFiltered1 = item.IsYMLFileExist;
                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim().StartsWith("Проект:"))
                    {
                        item.IsFiltered1 = (!string.IsNullOrWhiteSpace(item.GetYMLFile(ymlfield)));

                    }
                    else if (cbFilterList1.SelectedItem.ToString().Trim() == "ПРОВЕРИТЬ")
                    {
                        // неизвестный проект
                        if ((ymlfield == "YMLFile_unknown") && (!string.IsNullOrWhiteSpace(item.YMLFile_unknown))) item.IsFiltered1 = true;

                        // регион не указан
                        if (string.IsNullOrWhiteSpace(item.Region)) item.IsFiltered1 = true;

                        /*
                        НЕ включено в релиз ИЛИ Задача без yml-файла
                        И
                        Есть базовая региональность БД ИЛИ Есть downtime ИЛИ Есть действия при обновлении ИЛИ Есть данные в БД ИЛИ Есть объекты БД
                        */

                        if (
                            (
                                (item.IsAddRelease != true) ||
                                item.IsYMLFileNotExist
                            ) &&
                            (
                                (item.IsBaseRegion == true) ||
                                (item.IsDowntime == true) ||
                                (!string.IsNullOrWhiteSpace(item.UpdActions)) ||
                                (!string.IsNullOrWhiteSpace(item.DataBD)) ||
                                (!string.IsNullOrWhiteSpace(item.ObjectsBD))

                            )
                            ) item.IsFiltered1 = true;

                        /*
                         Есть базовая региональность БД 
                            И
                         Регион НЕ базовый
                         */

                        if (
                            (item.IsBaseRegion == true) &&
                            //(!string.IsNullOrWhiteSpace(item.Region)) &&
                            (!item.Region.ToUpper().Contains("БАЗОВЫЙ")) //-V3125
                           )
                        {
                            item.IsFiltered1 = true;
                        }

                        /*
                         задачи отчетников
                         */
                        if ((item.IsAddRelease == true) &&
                            (
                            regex_rpt.IsMatch(item.DataBD.ToLower()) || //-V3125
                            regex_rpt.IsMatch(item.ObjectsBD.ToLower()) || //-V3125
                            item.UpdActions.ToLower().Contains("report_ms") || //-V3125
                            item.UpdActions.ToLower().Contains("report_pg") //-V3125
                            )
                            )
                        {
                            item.IsFiltered1 = true;
                        }

                        /*
                         есть ошибки по результатам проверки
                        */
                        if (item.IsYMLFileCommentExist)
                        {
                            item.IsFiltered1 = true;
                        }

                        //для тестирования
                        //item.IsFiltered1 = !item.IsFiltered1;
                    }
                }
                dgYMLFilesRefresh();
            }
        }

        /// <summary>Выбор Фильтр 2</summary>
        private void cbFilterList2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFilterList2.SelectedIndex >= 0)
            {
                if (cbFilterList2.SelectedItem.ToString().Trim() == "ВСЕ")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                    {
                        item.IsFiltered2 = true;
                    }
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Изменившиеся после обновления")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = !string.IsNullOrWhiteSpace(item.isUpdated);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Включено в релиз")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = item.IsAddRelease == true;
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "НЕ включено в релиз")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = !(item.IsAddRelease == true);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Merge НЕ прошел")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = string.IsNullOrWhiteSpace(item.MergeStatus);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Регион не указан")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = string.IsNullOrWhiteSpace(item.Region);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Регион БАЗОВЫЙ")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = (item.Region != null) && item.Region.ToUpper().Contains("БАЗОВЫЙ"); //-V3063
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Регион НЕ базовый")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = (!string.IsNullOrWhiteSpace(item.Region)) && (!item.Region.ToUpper().Contains("БАЗОВЫЙ"));
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Есть Базовая региональность БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = item.IsBaseRegion == true;
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Нет Базовая региональность БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = !(item.IsBaseRegion == true);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Есть Downtime")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = item.IsDowntime == true;
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Нет Downtime")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = !(item.IsDowntime == true);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Есть Действия при обновлении")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = !string.IsNullOrWhiteSpace(item.UpdActions);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Нет Действия при обновлении")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = string.IsNullOrWhiteSpace(item.UpdActions);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Есть Данные в БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = !string.IsNullOrWhiteSpace(item.DataBD);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Нет Данные в БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = string.IsNullOrWhiteSpace(item.DataBD);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Есть Объекты БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = !string.IsNullOrWhiteSpace(item.ObjectsBD);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Нет Объекты БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered2 = string.IsNullOrWhiteSpace(item.ObjectsBD);
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Задача без yml-файла")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                    {
                        item.IsFiltered2 = item.IsYMLFileNotExist;
                    }
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim() == "Задача с yml-файлом")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                    {
                        item.IsFiltered2 = item.IsYMLFileExist;
                    }
                }
                else if (cbFilterList2.SelectedItem.ToString().Trim().StartsWith("Проект:"))
                {
                    string project = cbFilterList2.SelectedItem.ToString().Trim().Replace("Проект:", "").Trim();
                    string ymlfield = "";
                    if (project == "неизвестный")
                    {
                        ymlfield = "YMLFile_unknown";
                    }
                    else
                    {
                        ymlfield = Utilities.GITProjects.GetYMLFieldByProject(project);
                    }

                    foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                    {
                        item.IsFiltered2 = (!string.IsNullOrWhiteSpace(item.GetYMLFile(ymlfield)));
                    }
                }

                dgYMLFilesRefresh();
            }

        }

        /// <summary>Выбор Фильтр 3</summary>
        private void cbFilterList3_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbFilterList3.SelectedIndex >= 0)
            {
                if (cbFilterList3.SelectedItem.ToString().Trim() == "ВСЕ")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                    {
                        item.IsFiltered3 = true;
                    }
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Изменившиеся после обновления")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = !string.IsNullOrWhiteSpace(item.isUpdated);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Включено в релиз")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = item.IsAddRelease == true;
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "НЕ включено в релиз")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = !(item.IsAddRelease == true);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Регион не указан")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = string.IsNullOrWhiteSpace(item.Region);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Регион БАЗОВЫЙ")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = (item.Region != null) && item.Region.ToUpper().Contains("БАЗОВЫЙ"); //-V3063
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Регион НЕ базовый")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = (!string.IsNullOrWhiteSpace(item.Region)) && (!item.Region.ToUpper().Contains("БАЗОВЫЙ"));
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Есть Базовая региональность БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = item.IsBaseRegion == true;
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Нет Базовая региональность БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = !(item.IsBaseRegion == true);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Есть Downtime")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = item.IsDowntime == true;
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Нет Downtime")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = !(item.IsDowntime == true);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Есть Действия при обновлении")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = !string.IsNullOrWhiteSpace(item.UpdActions);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Нет Действия при обновлении")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = string.IsNullOrWhiteSpace(item.UpdActions);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Есть Данные в БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = !string.IsNullOrWhiteSpace(item.DataBD);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Нет Данные в БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = string.IsNullOrWhiteSpace(item.DataBD);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Есть Объекты БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = !string.IsNullOrWhiteSpace(item.ObjectsBD);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Нет Объекты БД")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles) item.IsFiltered3 = string.IsNullOrWhiteSpace(item.ObjectsBD);
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Задача без yml-файла")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                    {
                        item.IsFiltered3 = item.IsYMLFileNotExist;
                    }
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim() == "Задача с yml-файлом")
                {
                    foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                    {
                        item.IsFiltered3 = item.IsYMLFileExist;
                    }
                }
                else if (cbFilterList3.SelectedItem.ToString().Trim().StartsWith("Проект:"))
                {
                    string project = cbFilterList3.SelectedItem.ToString().Trim().Replace("Проект:", "").Trim();
                    string ymlfield = "";
                    if (project == "неизвестный")
                    {
                        ymlfield = "YMLFile_unknown";
                    }
                    else
                    {
                        ymlfield = Utilities.GITProjects.GetYMLFieldByProject(project);
                    }

                    foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                    {
                        item.IsFiltered3 = (!string.IsNullOrWhiteSpace(item.GetYMLFile(ymlfield)));
                    }
                }

                dgYMLFilesRefresh();
            }

        }

        /// <summary>Нажата кнопка Очистить фильтр</summary>
        private void btClearFilter_Click(object sender, RoutedEventArgs e)
        {
            cbFilterList1.SelectedIndex = 0;
            cbFilterList2.SelectedIndex = 0;
            cbFilterList3.SelectedIndex = 0;
            tbFilterTask.Text = "";
            btFindTask_Click(null, null);
        }

        /// <summary>Нажата кнопка Сохранить задачу</summary>
        private void btSaveTask_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow != null)
            {
                mainWindow.miSaveTask_Click(sender, e);
                btSaveTask.Focus();
            }
        }

        /// <summary>Нажата кнопка Обновить из Jira</summary>
        private void btRefreshJira_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return;
            }

            if (isExecFill == true)
            {
                MessageBox.Show("Идет парсинг задач из Jira, подождите!");
                return;
            }

            FillYMLFiles(tbTasks.Text.Trim(), MainWindow.Task.ReleaseYMLFiles, true);
        }

        /// <summary>Нажата кнопка Скопировать из GIT</summary>
        private void btFormGIT_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            tbNumVersion.Focus();
            btFormGIT.Focus();

            string GITProject = cbGITProject.SelectedItem.ToString().Trim();
            bool isFound = false;
            bool isError = false;
            string YMLFile = "";
            string errors = "";

            // копируем yml
            YMLFile = tbFileVersion.Text.Trim();
            if (!YMLFile.ToLower().EndsWith(".yml")) YMLFile += ".yml";

            this.Cursor = Cursors.Wait;

            List<YMLText> ListVersions = new List<YMLText>();
            isError = !Utilities.YML.GetYmlFromGIT(new List<string> { GITProject }, ref YMLFile, out isFound, CopyType.COPY, isCorrectEncoding.IsChecked == true, isCheckBOM.IsChecked == true, out errors, ref ListVersions, isCopyPrevVersion.IsChecked == true, logFileRelease);

            this.Cursor = Cursors.Arrow;

            if (!string.IsNullOrWhiteSpace(YMLFile))
            {
                if (!isFound)
                {
                    App.AddLog($"Файл {YMLFile} не найден в папке version в проекте {GITProject}", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                }
                else
                {
                    App.AddLog($"Файл {YMLFile} скопирован", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                }
            }
        }

        /// <summary>
        /// Нажа кнопка Отправить в GIT
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btTortoiseGIT_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            string GITProject = cbGITProject.SelectedItem.ToString().Trim();

            // Проверяем, убрали следующую версию ?
            if (
                (!string.IsNullOrWhiteSpace(lastNextVersion)) &&
                (
                    (cbNextVersion.SelectedIndex == -1) ||
                    (cbNextVersion.SelectedItem == null) ||
                    string.IsNullOrWhiteSpace(cbNextVersion.SelectedItem.ToString())
                )
            )
            {
                App.AddLog($"У текущей версии {tbNumVersion.Text} была убрана следующая версия. Необходимо вручную поменять ссылку на предыдущую версию в версии {lastNextVersion}", null, App.ShowMessageMode.SHOW, true, logFileRelease);

                lastNextVersion = "";
            }

            // Проверяем, менялась ли следующая версия ?
            if (
                (cbNextVersion.SelectedIndex != -1) &&
                (cbNextVersion.SelectedItem != null) &&
                (!string.IsNullOrWhiteSpace(cbNextVersion.SelectedItem.ToString())) &&
                (cbNextVersion.SelectedItem.ToString() != lastNextVersion)
            )
            {
                btSetNextVersion_Click(null, null);

                lastNextVersion = cbNextVersion.SelectedItem.ToString();
            }
            else
            {
                // Отправляем в GIT
                GIT.GitAdd(new string[] { GITProject }, tbBranch.Text.Trim(), true, false, logFileRelease);
            }
        }

        /// <summary>пункт меню Обновить из Jira в dgYMLFiles</summary>
        private void dgYMLFiles_MenuItemRefreshJira(object sender, RoutedEventArgs e)
        {
            if (isExecFill == true)
            {
                MessageBox.Show("Идет парсинг задач из Jira, подождите!");
                return;
            }

            if (dgYMLFiles.SelectedIndex >= 0)
            {
                var _yml = dgYMLFiles.SelectedItem as YMLFileInfo;

                FillYMLFiles(tbTasks.Text.Trim(), MainWindow.Task.ReleaseYMLFiles, true, _yml.TaskNumber);
            }
        }

        /// <summary>
        /// Выбран пункт меню "Текст ошибки в отдельном окне"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgYMLFiles_MenuItemShowError(object sender, RoutedEventArgs e)
        {
            if (dgYMLFiles.CurrentColumn != null)
            {

                var field = dgYMLFiles.CurrentColumn.SortMemberPath;

                if ((dgYMLFiles.SelectedCells.Count > 0) && field.Contains("YMLFile_"))
                {
                    var CellValue = Utilities.Controls.GetSelectedValue(dgYMLFiles, dgYMLFiles.CurrentColumn.DisplayIndex);

                    if (CellValue != "")
                    {
                        var row = (YMLFileInfo)dgYMLFiles.CurrentItem;

                        WinInfo WinInfo = new WinInfo(null);
                        WinInfo.Title = "Ошибка";
                        WinInfo.tbInfo.Text = row.GetYMLFile_Comment(field);
                        WinInfo.Show();
                    }
                }

                if ((dgYMLFiles.SelectedCells.Count > 0) && (field == "TaskNumber"))
                {
                    var CellValue = Utilities.Controls.GetSelectedValue(dgYMLFiles, dgYMLFiles.CurrentColumn.DisplayIndex);

                    if (CellValue != "")
                    {
                        var row = (YMLFileInfo)dgYMLFiles.CurrentItem;

                        WinInfo WinInfo = new WinInfo(null);
                        WinInfo.Title = "Ошибка";
                        WinInfo.tbInfo.Text = row.ErrorInfo;
                        WinInfo.Show();
                    }
                }
            }
        }

        /// <summary>
        /// Нажата кнопка для копирования URL версии в буфер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCopyGITVersion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(tbGITVersion.Text.Trim());
            }
            catch (Exception ex)
            {
                App.AddLog($"Неизвестная ошибка при копировании в буфер:\n", ex, App.ShowMessageMode.SHOW, true, logFileRelease);
            }
        }

        /// <summary>
        /// Проверка заполнения нужных полей для продолжения
        /// </summary>
        /// <param name="branch">текущая ветка</param>
        /// <returns></returns>
        private bool CheckBranch(out string branch)
        {
            branch = "";

            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return false;
            }

            if (isExecFill == true)
            {
                MessageBox.Show("Идет парсинг задач из Jira, подождите!");
                return false;
            }

            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                {
                    MessageBox.Show("Выберите проект ГИТ");
                    cbGITProject.Focus();
                    return false;
                }
            }

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string err = "";
            branch = GIT.GitCurrentBranch(project, out err, logFileRelease);
            tbBranch.Text = branch;

            // Видимость полей и кнопок
            SetVisiblyForProject(project);

            if (Utilities.GITProjects.IsDEVProject(project))
            {
                string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
                string branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);

                if (branch.ToLower() != branchversion.ToLower())
                {
                    App.AddLog("У проекта " + project + " ветка " + branch + " не соответствует номеру версии " + tbNumVersion.Text.Trim(), null, App.ShowMessageMode.SHOW, true, logFileRelease);
                    cbGITProject.Focus();
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Проверка на отсутствие merged в задачах
        /// </summary>
        /// <returns></returns>
        private bool CheckMerged()
        {
            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                return false;
            }

            string project_dev = cbGITProject.SelectedItem.ToString().Trim();

            if (Utilities.GITProjects.IsDEVProject(project_dev))
            {
                foreach (var info in MainWindow.Task.ReleaseYMLFiles
                .Where(x => x.IsAddRelease == true)
                .OrderBy(x => x.YMLOrder)
                )
                {
                    string ymlfield_dev = Utilities.GITProjects.GetYMLFieldByProject(project_dev);
                    string file_dev = info.GetYMLFile(ymlfield_dev);

                    if (
                        (!string.IsNullOrWhiteSpace(file_dev)) &&
                        string.IsNullOrWhiteSpace(info.MergeStatus)
                        )
                    {
                        App.AddLog("Ветка " + info.BranchName + " не влита в версию " + tbNumVersion.Text.Trim(), null, App.ShowMessageMode.SHOW, true, logFileRelease);
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Нажата Создать\переключиться на ветку
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGitNewBranch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return;
            }

            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                {
                    MessageBox.Show("Выберите проект ГИТ");
                    cbGITProject.Focus();
                    return;
                }
            }

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);
            double numversion = Release.VerAsNum(branchversion);
            string prevbranch = "";

            if (
                // Если не выбрана предыдущая ветка
                (cbPrevVersion.SelectedIndex == -1) ||
                string.IsNullOrWhiteSpace(cbPrevVersion.SelectedItem.ToString())
                )
            {
                if (
                    // если ветка версии отсутствует в GIT
                    (!Versions.ContainsKey(numversion)) ||
                    (!Versions[numversion].isBranchExists)
                )
                {
                    if (
                        (Versions.Where(x => x.Key < numversion).Count() <= 0) &&
                        (System.Windows.Forms.MessageBox.Show($"Создать новую ветку {branchversion} от ветки master ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        )
                    {
                        prevbranch = "master";
                    }
                    else
                    {

                        MessageBox.Show("Выберите, от какой ветки создать ветку версии");
                        cbPrevVersion.Focus();
                        return;
                    }
                }
            }
            else
            {
                prevbranch = cbPrevVersion.SelectedItem.ToString().Trim();
            }

            if (
                // если ветка версии отсутствует в GIT
                (!Versions.ContainsKey(numversion)) ||
                (!Versions[numversion].isBranchExists)
               )
            {
                if (!MainWindow.APPinfo.ReleaseBranch.Contains(prevbranch, StringComparer.OrdinalIgnoreCase) &&
                    (Release.VerAsNum(prevbranch) >= numversion)
                )
                {
                    MessageBox.Show("Выберите ПРЕДЫДУЩУЮ версию");
                    cbPrevVersion.Focus();
                    return;
                }
            }

            string err = "";
            string branch = GIT.GitCurrentBranch(project, out err, logFileRelease);
            string ask = "";
            string ask1 = "";

            if (branch.ToLower() != branchversion.ToLower())
            {
                ask = "Сейчас в проекте " + project + " текущая ветка " + branch + Environment.NewLine +
                                Environment.NewLine +
                                "Создать/переключить в проекте " + project + " на ветку " + branchversion + " ?";
                ask1 = "Создание/переключение в проекте " + project + " на ветку " + branchversion;
            }
            else
            {
                ask = "Сейчас в проекте " + project + " текущая ветка " + branch + Environment.NewLine +
                                Environment.NewLine +
                                "Обновить (git pull) ветку " + branchversion + " ?";
                ask1 = "Обновление (git pull) ветки " + branchversion;
            }

            if (System.Windows.Forms.MessageBox.Show(
                               ask,
                                "ВНИМАНИЕ",
                                System.Windows.Forms.MessageBoxButtons.YesNo
                            ) == System.Windows.Forms.DialogResult.Yes
                        )
            {
                App.AddLog(ask1, null, App.ShowMessageMode.NONE, true, logFileRelease);

                if (
                    Versions.ContainsKey(numversion) &&
                    Versions[numversion].isBranchExists
                    )
                {
                    // Переключиться на существующую ветку версии
                    cbGITProject_Sync(branchversion, false);
                }
                else
                {
                    // Создать ветку версии
                    GIT.GitNewBranch(new string[] { project }, branchversion, prevbranch, logFileRelease);
                }

                branch = GIT.GitCurrentBranch(project, out err, logFileRelease);
                tbBranch.Text = branch;

                if (branch.ToLower() != branchversion.ToLower())
                {
                    App.AddLog("В проекте " + project + " текущая ветка осталась " + branch, null, App.ShowMessageMode.SHOW, true, logFileRelease);
                }
                else
                {
                    //cbPrevVersion.IsEnabled = false; ;
                    //btFindPrevVersion.IsEnabled = cbPrevVersion.IsEnabled;

                    btGitNewBranch.IsEnabled = false;
                    if (Versions.ContainsKey(numversion)) Versions[numversion].isBranchExists = true;
                }

                string URL = Utilities.GITProjects.GetURLVersionByProject(project);
                URL = URL.Replace("%BRANCH%", tbBranch.Text);
                tbGITVersion.Text = URL + tbFileVersion.Text.Trim();

                // Команды бота
                Fill_cbLiquibot(project);
            }
        }

        /// <summary>
        /// Нажата кнопка "Получить номер версии"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGetNumVer_Click(object sender, RoutedEventArgs e)
        {
            if ((!string.IsNullOrWhiteSpace(MainWindow.Task.TaskUrl)) && HTML.OpenLoginJira(logFileRelease))
            {
                HTML html = new HTML();

                var task = new Dictionary<string, string>
                {
                    { MainWindow.Task.TaskNumber, MainWindow.Task.TaskUrl }
                };

                // парсинг
                html.LoadJiraPages(task, this,
                    null,
                    null,
                    x =>
                    {
                        string old = tbNumVersion.Text.Trim();

                        tbNumVersion.Text = x.EpicName
                            .Replace('\r', ' ')
                            .Replace('\n', ' ')
                            .Replace('\t', ' ')
                            //.Replace("prmd.", "")
                            //.Replace("rpms.", "")
                            //.Replace("bi.", "")
                            //.Replace("smp.", "")
                            //.Replace("-pg15", "")
                            .Trim();

                        if (old != tbNumVersion.Text)
                        {
                            NumVersionChanged();
                        }
                    },
                    null,
                    null,
                    logFileRelease
                ).GetAwaiter();
            }

            btSetNumVersion_Click(null, null);
        }

        /// <summary>
        /// Обновить текущий проект
        /// </summary>
        /// <param name="branch"></param>
        /// <param name="isForcedGitRefresh"></param>
        private void GitPull_Sync(string branch, bool isForcedGitRefresh)
        {
            string project = cbGITProject.SelectedItem.ToString().Trim();

            GIT.GitPull(new string[] { project }, branch, false, true, false, logFileRelease, isForcedGitRefresh);

            // проставляем merged
            if (Utilities.GITProjects.IsDEVProject(project))
            {
                SetMerged(project, MainWindow.Task.ReleaseYMLFiles);
                dgYMLFilesRefresh();
            }
        }

        /// <summary>
        /// Нажата кнопка GIT PULL
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGitPull_Click(object sender, RoutedEventArgs e)
        {
            //if (!CheckBranch(out string branch)) return;
            string project = cbGITProject.SelectedItem.ToString().Trim();

            // по номеру версии определить ветку
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);

            cbGITProject_Sync(branchversion, true);
        }

        /// <summary>
        /// Нажата кнопка Изменить для предыдущей версии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSetPrevVersion_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            Encoding encoding = new UTF8Encoding(false);

            string version = Release.GetNumVersion(prefix, tbNumVersion.Text);
            double num = Release.VerAsNum(version);
            string filename = tbFileVersion.Text.Trim();
            string fullfilename = Path.Combine(path, filename);

            string prevfile = "";

            if (File.Exists(fullfilename))
            {
                string ask = "";
                string ask2 = "";

                if (
                    (cbPrevVersion.SelectedIndex == -1) ||
                    string.IsNullOrWhiteSpace(cbPrevVersion.SelectedItem.ToString())
                    )
                {
                    ask = $"Убрать в файле {filename} ссылку на предыдущую версию ?";
                    ask2 = $"В файле {filename} убрана ссылка на предыдущую версию";
                }
                else
                {
                    string prevfullfile = "";
                    double prevnum = 0;

                    if (Utilities.GITProjects.IsGITProject(project))
                    {
                        // с списке - имена файлов
                        string name = cbPrevVersion.SelectedItem.ToString().Trim();

                        // ищем в списке версий
                        var found = Versions.Where(x => x.Value.VisibleName.ToLower() == name.ToLower()).Any();
                        if (found)
                        {
                            prevnum = Versions.Where(x => x.Value.VisibleName.ToLower() == name.ToLower()).First().Key;
                            prevfile = Versions[prevnum].YMLFile.Filename;
                            prevfullfile = Versions[prevnum].YMLFile.FullFilename;
                        }

                        if (string.IsNullOrWhiteSpace(prevfile))
                        {
                            App.AddLog($"Файл {name} не найден!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                            cbPrevVersion.Focus();
                            return;
                        }

                        if (!File.Exists(prevfullfile))
                        {
                            App.AddLog($"Файл {prevfile} не существует!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                            cbPrevVersion.Focus();
                            return;
                        }
                    }
                    else
                    {
                        // с списке - имена веток версий
                        string name = cbPrevVersion.SelectedItem.ToString().Trim();
                        string prevver = Release.GetNumVersion(prefix, name);
                        prevnum = Release.VerAsNum(prevver);

                        if (
                            (prevnum == 0) || //-V3024
                            (!Versions.ContainsKey(prevnum)) ||
                            string.IsNullOrWhiteSpace(Versions[prevnum].YMLFile.Filename)
                            )
                        {
                            App.AddLog($"Файл версии для ветки {prevver} не найден!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                            cbPrevVersion.Focus();
                            return;
                        }

                        prevfile = Versions[prevnum].YMLFile.Filename;
                        prevfullfile = Versions[prevnum].YMLFile.FullFilename;

                        if (!File.Exists(prevfullfile))
                        {
                            App.AddLog($"Файл {prevfile} не существует!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                            cbPrevVersion.Focus();
                            return;
                        }
                    }

                    if (prevnum >= num)
                    {
                        App.AddLog("Выберите ПРЕДЫДУЩУЮ версию", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                        cbPrevVersion.Focus();
                        return;
                    }

                    ask = $"Изменить в файле {filename} ссылку на предыдущую версию на {prevfile} ?";
                    ask2 = $"В файле {filename} ссылка на предыдущую версию изменена на {prevfile}";
                }

                if (System.Windows.Forms.MessageBox.Show(ask, "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    // загружаем файл с версией
                    YMLStruct loadyml = new YMLStruct(null, logFileRelease);
                    loadyml.LoadYML(project, "version", filename, false, null, false, false);

                    if (loadyml.IsFileExist)
                    {
                        if (loadyml.PrevVersions.Count() > 1)
                        {
                            App.AddLog($"В файле {filename} больше одной ссылки на предыдущую версию!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                            cbPrevVersion.Focus();
                            return;
                        }

                        // удаляем существующие ссылки на предыдущме версии
                        foreach (var item in loadyml.Lines.Where(x => x.type == YMLLineType.VERSION).ToList())
                        {
                            loadyml.DeleteYML(item);
                        }

                        // добавляем include предыдущей версии
                        if (!string.IsNullOrWhiteSpace(prevfile))
                        {
                            loadyml.Lines.Add(new YMLLine(loadyml, logFileRelease)
                            {
                                type = YMLLineType.VERSION,
                                isLoaded = false,
                                text = "",
                                file = prevfile,
                                order = 0,
                                path = "version",
                                relativeToChangelogFile = loadyml.relativeToChangelogFile
                            });
                        }

                        // генерация yml-файл
                        loadyml.SaveYML(false, false, MainWindow.APPinfo.relativeToChangelogFile == "true", true, false, isImproveSQLinVersionRelease, "", loadyml.IsNoCumulative, version);

                        // если перешли на пути относительно корня проекта или улучшаем скрипты релиза - надо исправить yml-файлы задач
                        if (MainWindow.APPinfo.relativeToChangelogFile == "false" || isImproveSQLinVersionRelease)
                        {
                            // надо обновить вложенные задачи
                            loadyml.ReLoadYML(false, null, true, true);
                            loadyml.SaveYML(false, true, MainWindow.APPinfo.relativeToChangelogFile == "true", true, true, isImproveSQLinVersionRelease, "", loadyml.IsNoCumulative, version);
                        }

                        // обновляем в списке версий
                        if (Versions.ContainsKey(num))
                        {
                            Versions[num].YMLFile = loadyml;
                        }
                        else
                        {
                            Version ver = new Version(logFileRelease) { Branch = tbBranch.Text.Trim(), YMLFile = loadyml };
                            Versions.Add(num, ver);
                        }

                        App.AddLog(ask2, null, App.ShowMessageMode.SHOW, true, logFileRelease);
                    }
                    else
                    {
                        App.AddLog($"Файл {filename} не существует!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                    }
                }
            }
        }

        /// <summary>
        /// Сменить в следующей версии ссылку на текущую, влить текущую версию в следующую
        /// </summary>
        /// <returns></returns>
        private bool SetNextVersion()
        {
            if (!CheckBranch(out string branch)) return false;

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");

            if (string.IsNullOrWhiteSpace(tbFileVersion.Text))
            {
                MessageBox.Show("Не заполнено имя файла версии!");
                return false;
            }

            string file_ver = Path.Combine(path, tbFileVersion.Text);
            if (!File.Exists(file_ver))
            {
                MessageBox.Show($"Файл версии {file_ver} не существует, соберите версию!");
                return false;
            }

            if (
                (cbNextVersion.SelectedIndex == -1) ||
                string.IsNullOrWhiteSpace(cbNextVersion.SelectedItem.ToString())
            )
            {
                App.AddLog("Выберите СЛЕДУЮЩУЮ версию", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                return false;
            }

            Encoding encoding = new UTF8Encoding(false);

            string version = Release.GetNumVersion(prefix, tbNumVersion.Text);

            var nextyml = new YMLStruct(null, logFileRelease);
            double nextnum = 0;
            string nextbranch = "";

            string ask = "";
            string ask2 = "";

            bool isOk = true;

            if (
                (cbNextVersion.SelectedIndex != -1) &&
                (!string.IsNullOrWhiteSpace(cbNextVersion.SelectedItem.ToString()))
                )
            {
                if (Utilities.GITProjects.IsGITProject(project))
                {
                    // в списке - имена файлов
                    string name = cbNextVersion.SelectedItem.ToString().Trim();

                    // ищем номер версии в списке версий
                    var found = Versions.Where(x => x.Value.VisibleName.ToLower() == name.ToLower()).Any();
                    if (found)
                    {
                        nextnum = Versions.Where(x => x.Value.VisibleName.ToLower() == name.ToLower()).First().Key;
                        nextyml = Versions[nextnum].YMLFile;
                    }

                    if (string.IsNullOrWhiteSpace(nextyml.Filename))
                    {
                        App.AddLog($"Файл {name} не найден!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                        cbPrevVersion.Focus();
                        return false;
                    }

                    if (!File.Exists(nextyml.FullFilename))
                    {
                        App.AddLog($"Файл {nextyml.Filename} не существует!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                        cbPrevVersion.Focus();
                        return false;
                    }

                    ask = $"Изменить в файле версии {nextyml.Filename} ссылку на предыдущую версию на {tbFileVersion.Text.Trim()} ?";
                    ask2 = $"В файле {nextyml.Filename} ссылка на предыдущую версию изменена на {tbFileVersion.Text.Trim()}";
                }
                else
                {
                    // в списке - имена веток версий
                    nextbranch = cbNextVersion.SelectedItem.ToString().Trim();

                    // проверим, нужен ли commit
                    if (!GIT.CheckCommit(project, logFileRelease, "Изменение следующей версии прервано"))
                    {
                        cbNextVersion.Focus();
                        return false;
                    }

                    // переключимся на ветку следующей версии
                    if (!GIT.GitSwitch(project, nextbranch, logFileRelease, out string cur_branch, out string err))
                    {
                        App.AddLog($"В проекте {project} не смогли перелючиться на ветку {nextbranch} !\nТекущая ветка {cur_branch}\n{err}", null, App.ShowMessageMode.SHOW, true, logFileRelease);

                        cbNextVersion.Focus();
                        isOk = false;
                    }

                    if (isOk)
                    {
                        // номер следующей версии
                        string nextver = Release.GetNumVersion(prefix, nextbranch);
                        nextnum = Release.VerAsNum(nextver);
                        nextyml = Versions[nextnum].YMLFile;

                        if (
                            (nextnum == 0) || //-V3024
                            (!Versions.ContainsKey(nextnum)) ||
                            string.IsNullOrWhiteSpace(nextyml.Filename)
                            )
                        {
                            App.AddLog($"Файл версии для ветки {nextver} не найден!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                            cbNextVersion.Focus();
                            isOk = false;
                        }

                        if (isOk)
                        {
                            if (!File.Exists(nextyml.FullFilename))
                            {
                                App.AddLog($"Файл {nextyml.Filename} не существует!", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                                cbNextVersion.Focus();
                                isOk = false;
                            }

                            ask = $"Влить ветку {branch} в ветку {nextbranch} и изменить в файле версии {nextyml.Filename} ссылку на предыдущую версию на {tbFileVersion.Text.Trim()} ?";
                            ask2 = $"Ветка {branch} влита в {nextbranch} и в файле {nextyml.Filename} ссылка на предыдущую версию изменена на {tbFileVersion.Text.Trim()}";
                        }
                    }
                }

                if (isOk && (nextnum <= Release.VerAsNum(version)))
                {
                    App.AddLog("Выберите СЛЕДУЮЩУЮ версию", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                    cbNextVersion.Focus();

                    isOk = false;
                }

                //if (Utilities.GIT.IsGITProject(project))

                if (isOk)
                {
                    if (System.Windows.Forms.MessageBox.Show(ask, "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        // обновим данные о файле
                        nextyml.ReLoadYML(false, null, false, false);

                        if (nextyml.PrevVersions.Count() > 1)
                        {
                            App.AddLog($"В файле {nextyml.Filename} больше одной ссылки на предыдущую версию! Завершаем без изменений !", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                            cbNextVersion.Focus();
                            isOk = false;
                        }

                        if (isOk)
                        {
                            // merge
                            if (Utilities.GITProjects.IsDEVProject(project))
                            {
                                if (GIT.GitMerge(project, branch, nextbranch, true, false, logFileRelease, true))
                                {
                                    // merge успешный, делаем push
                                    GIT.GitPush(new string[] { project }, nextbranch, true, logFileRelease);
                                }
                            }

                            // определяем текущий relativeToChangelogFile по первой записи в файле
                            string old_relativeToChangelogFile = "false";
                            var firstline = nextyml.Lines.Where(x => x.type == YMLLineType.VERSION || x.type == YMLLineType.TASK).FirstOrDefault();
                            if (firstline != null)
                            {
                                old_relativeToChangelogFile = firstline.relativeToChangelogFile;
                            }
                            nextyml.relativeToChangelogFile = old_relativeToChangelogFile;

                            // удаляем существующие ссылки на предыдущие версии
                            foreach (var item in nextyml.Lines.Where(x => x.type == YMLLineType.VERSION).ToList())
                            {
                                nextyml.DeleteYML(item);
                            }

                            // добавляем include предыдущей версии
                            nextyml.Lines.Add(new YMLLine(nextyml, logFileRelease)
                            {
                                type = YMLLineType.VERSION,
                                isLoaded = false,
                                text = "",
                                file = tbFileVersion.Text.Trim(),
                                order = -1,
                                path = "version",
                                relativeToChangelogFile = old_relativeToChangelogFile
                            });

                            // генерация yml-файл
                            Utilities.Files.WriteScript(nextyml.FullFilename, null, nextyml.ToString(), true, out string err, FileMode.Create, false, false);
                            nextyml.IsFileExist = true;

                            if (Utilities.GITProjects.IsDEVProject(project))
                            {
                                // обновляем в списке СЛЕДУЮЩИХ версий
                                if (NextVersions.ContainsKey(nextnum))
                                {
                                    NextVersions[nextnum].YMLFile = nextyml;
                                }
                                else
                                {
                                    Version ver = new Version(logFileRelease) { Branch = nextbranch, YMLFile = nextyml };
                                    NextVersions.Add(nextnum, ver);
                                }
                                // обновляем в списке версий
                                if (Versions.ContainsKey(nextnum))
                                {
                                    Versions[nextnum].YMLFile = nextyml;
                                }
                                else
                                {
                                    Version ver = new Version(logFileRelease) { Branch = nextbranch, YMLFile = nextyml };
                                    Versions.Add(nextnum, ver);
                                }

                                // проверим, нужен ли commit
                                if (!GIT.CheckCommit(project, logFileRelease, $"Ветка {branch} влита в {nextbranch} и в файле {nextyml.Filename} ссылка на предыдущую версию изменена на {tbFileVersion.Text.Trim()},\nно для ветки {nextbranch} требуется commit & push!"))
                                {
                                    isOk = false;
                                }
                            }

                            if (isOk)
                            {
                                // сохраняем выбранную следующую версию
                                lastNextVersion = cbNextVersion.SelectedItem.ToString();

                                App.AddLog(ask2, null, App.ShowMessageMode.SHOW, true, logFileRelease);
                            }

                        }
                    }
                }

                if (isOk && Utilities.GITProjects.IsDEVProject(project))
                {
                    // переключимся на ветку версии
                    if (!GIT.GitSwitch(project, branch, logFileRelease, out string cur_branch, out string err))
                    {
                        App.AddLog($"Не смогли вернуться на ветку {branch} !\nТекущая ветка {cur_branch}", null, App.ShowMessageMode.SHOW, true, logFileRelease);

                        cbNextVersion.Focus();
                        isOk = false; //-V3137
                    }
                }
            }

            return isOk;
        }

        /// <summary>
        /// Нажата кнопка Изменить для следующей версии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSetNextVersion_Click(object sender, RoutedEventArgs e)
        {
            SetNextVersion();
        }

        /// <summary>
        /// Нажата кнопка Влить дальше
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btMergeNextVersion_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string cur_branch)) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();
            if (!Utilities.GITProjects.IsDEVProject(project)) return;

            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string branch = tbBranch.Text;
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, branch));

            if (
                Versions == null ||
                Versions.Count == 0 ||
                Versions[numversion] == null ||
                Versions[numversion].YMLFile == null
            )
            {
                MessageBox.Show("Список версий пуст или не полный");
                return;
            }

            if (!Versions
                .Where(x =>
                    (x.Value != null) &&
                    (x.Value.YMLFile != null) &&
                    x.Value.YMLFile.IsFileExist && // есть файл-версии
                    (!x.Value.YMLFile.IsIgnore) && // нет флага-исключения
                    (x.Value.NumOrder > numversion) && // следующие версии
                    (x.Value.PrevNumOrder > 0) && // у которых есть ссылка на предыдущую версию
                    (x.Value.PrevNumOrder == numversion) // ссылаются на текущую ветку
                ).Any()
            )
            {
                MessageBox.Show($"После версии {branch} нет последующих версий, в которые ее можно влить");
                return;
            }

            // нашли минимум одну версию после СЛЕДУЮЩЕЙ
            if (System.Windows.Forms.MessageBox.Show($"Вольем в проекте {project} ветку {branch} во все последующие ветки ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                // вливаем по все последующие после СЛЕДУЮЩЕЙ
                GIT.GitMergeNextVersion(project, branch, isCumulative.IsChecked != true, Versions, logFileRelease);

                // ----------------------------------------------------------------------------
                // показываем лог
                if (
                    (System.Windows.Forms.MessageBox.Show("Посмотреть итоговый лог ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                )
                {
                    WinInfo WinInfo = new WinInfo(logFileRelease);
                    WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                    WinInfo.tbInfo.Text = File.ReadAllText(logFileRelease);
                    WinInfo.Title = "Лог в файле " + logFileRelease;
                    WinInfo.Show();
                }

                // проверка текущей ветки и видимость кнопок и полей
                CheckBranch(out branch);
            }
        }

        /// <summary>
        /// сменить номер в поле Номер версии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSetNumVersion_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            btSetNumVersion.IsEnabled = false;
            tbNumVersion.IsReadOnly = false;
            cbGITProject.SelectedIndex = -1;
            tbNumVersion.Focus();
            // сохраняем задачу
            if (mainWindow != null)
            {
                mainWindow.SaveTaskNoShow();
                btSaveTask.Focus();
            }
        }

        /// <summary>
        /// Поиск предыдущей версии
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void btFindPrevVersion_Click(object sender, RoutedEventArgs e)
        {
            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                {
                    MessageBox.Show("Выберите проект ГИТ");
                    cbGITProject.Focus();
                    return;
                }
            }

            FormFindInList dlg1 = new FormFindInList(logFileRelease);

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);

            // текущая версия
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));

            //if (Utilities.GIT.IsDEVProject(project))
            //{
            //    dlg1.AddItems(NoMergedVersions.OrderByDescending(x => x.Key).Select(x => x.Value.Filename).ToList());
            //}
            //else
            //{
            dlg1.AddItems(Versions
                .Where(x => x.Key < numversion)
                .OrderByDescending(x => x.Key)
                .Select(x => x.Value.VisibleName)
                .ToList());
            //}

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string result = "";

                foreach (System.Windows.Forms.DataGridViewRow row in dlg1.dgListValues.SelectedRows)
                {
                    result = row.Cells[0].Value.ToString();
                    // берем только первую
                    break; //-V3020
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    cbPrevVersion.SelectedItem = result;
                }
            }

            dlg1.Dispose();
        }

        /// <summary>
        /// Merge задач
        /// </summary>
        /// <param name="mergeType">тип merge</param>
        /// <param name="listTask">список строк из MainWindow.Task.ReleaseYMLFiles</param>
        private void MergeTask(MergeType mergeType, List<YMLFileInfo> listTask)
        {
            if (listTask == null) return;
            if (listTask.Where(x => x.IsAddRelease == true).Count() == 0) return;

            string project_dev = cbGITProject.SelectedItem.ToString().Trim();

            // только если "новый" проект
            if (Utilities.GITProjects.IsDEVProject(project_dev))
            {
                string project_git = Utilities.GITProjects.GetGITProject(project_dev);
                string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project_dev);
                string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project_dev);
                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                WinExecute WinExecute = new WinExecute(logFileRelease);
                WinExecute.Title = "Merge задач в ветку " + tbBranch.Text.Trim();

                // текущая версия
                double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));

                // Есть следующая версия ?
                var hasnext = Versions.Where(x => x.Key > numversion).Count() > 0;

                // Дополнительная информация об ошибках и предупреждениях
                string ErrInfo = "";

                // Добавляем git fetch -all
                WinExecute.AddCommand(
                                    App.AppPath,
                                    Path.Combine(App.AppPath, "git_fetchall.cmd"),
                                    folder
                                    );

                // Если нет последующих версий - принудительно вольем master
                if (
                    (!hasnext) &&
                    (mergeType != MergeType.SINGLE)
                )
                {
                    if (System.Windows.Forms.MessageBox.Show($"{tbBranch.Text} - это крайняя версия, желательно влить в нее ветку master! Вольем ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        WinExecute.AddCommand(
                                            App.AppPath,
                                            Path.Combine(App.AppPath, "git_merge.cmd"),
                                            folder + " master NOUPPERCASE"
                                            );
                    }
                    else
                    {
                        string info = $"Пользователь отказался вливать ветку master в ветку {tbBranch.Text}";
                        ErrInfo += info + Environment.NewLine + Environment.NewLine;
                        App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);
                    }
                }

                DateTime prevCommitDate = DateTime.MinValue;

                string badHronologyList = "";

                // перебираем все задачи, включенные в релиз, в том же порядке, что планируется в yml версии
                foreach (var ymlfile in listTask
                        .Where(x => x.IsAddRelease == true)
                        .OrderBy(x => x.YMLOrder)
                )
                {
                    ymlfile.MergeStatus = "";
                    ymlfile.TaskCommitDate = "";

                    string ymlfield_dev = Utilities.GITProjects.GetYMLFieldByProject(project_dev);
                    string ymlfield_git = Utilities.GITProjects.GetYMLFieldByProject(project_git);

                    string file_dev = ymlfile.GetYMLFile(ymlfield_dev);
                    string file_git = ymlfile.GetYMLFile(ymlfield_git);

                    // выделяем номер задания
                    string task_dev = Utilities.Task.GetTaskNumber(file_dev.Split('.')[0]);
                    string task_git = Utilities.Task.GetTaskNumber(file_git.Split('.')[0]);

                    if (
                        (!string.IsNullOrWhiteSpace(file_dev)) ||
                        (!string.IsNullOrWhiteSpace(file_git))
                        )
                    {
                        // определяем имя ветки
                        string branch = ymlfile.Branch;

                        if (string.IsNullOrWhiteSpace(branch))
                        {
                            if (!string.IsNullOrWhiteSpace(task_dev))
                            {
                                branch = task_dev;
                            }
                            else if (!string.IsNullOrWhiteSpace(task_git))
                            {
                                branch = task_git;
                            }
                            else
                            {
                                branch = "";
                            }
                        }

                        if (
                            string.IsNullOrWhiteSpace(branch) &&
                            !string.IsNullOrWhiteSpace(file_dev)
                            )
                        {
                            string info = $"Для файла {file_dev} не удалось распознать ветку";
                            ErrInfo += info + Environment.NewLine + Environment.NewLine;
                            App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);
                        }

                        if (
                            string.IsNullOrWhiteSpace(branch) &&
                            !string.IsNullOrWhiteSpace(file_git)
                            )
                        {
                            string info = $"Для файла {file_git} не удалось распознать ветку";
                            ErrInfo += info + Environment.NewLine + Environment.NewLine;
                            App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);
                        }

                        // определяем имя файла
                        string file = "";

                        if (!string.IsNullOrWhiteSpace(file_dev))
                        {
                            file = file_dev;
                        }
                        else
                        {
                            file = file_git;
                        }

                        file = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, ymlfile.PathInGIT, file);

                        if (
                            mergeType == MergeType.ALL ||
                            mergeType == MergeType.SINGLE ||
                            (!File.Exists(file)) // если не все, то только те которых еще нет в ветке
                        )
                        {
                            // Добавляем задание
                            if (!string.IsNullOrWhiteSpace(branch))
                            {

                                // Проверям, какая последняя версия в ветке задачи
                                string lastverintask = GIT.GitLastVersionInTask(project_dev, branch, logFileRelease);
                                if (string.IsNullOrWhiteSpace(lastverintask))
                                {
                                    string info = $"В ветке задачи {branch} не удалось определить последнюю версию, от которой она создана";
                                    ErrInfo += info + Environment.NewLine + Environment.NewLine;
                                    App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);

                                    // спросим
                                    FormAsk3 dlg1 = new FormAsk3();
                                    dlg1.tbText.Text = info + Environment.NewLine + Environment.NewLine + $"Все равно вливаем ветку {branch} в версию {tbBranch.Text}?";
                                    dlg1.btCancel.Text = "Прервать влитие всех задач";
                                    var res = dlg1.ShowDialog();
                                    dlg1.Dispose();

                                    if (res == System.Windows.Forms.DialogResult.Cancel)
                                    {
                                        info = $"Пользователь прервал влитие всех задач в ветку {tbBranch.Text}";
                                        ErrInfo += info + Environment.NewLine + Environment.NewLine;
                                        App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);

                                        return;
                                    }

                                    if (res == System.Windows.Forms.DialogResult.No)
                                    {
                                        info = $"Пользователь отказался вливать ветку {branch} в ветку {tbBranch.Text}";
                                        ErrInfo += info + Environment.NewLine + Environment.NewLine;
                                        App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);

                                        continue;
                                    }
                                    else
                                    {
                                        info = $"Пользователь решил влил ветку {branch} в ветку {tbBranch.Text}";
                                        ErrInfo += info + Environment.NewLine + Environment.NewLine;
                                        App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);
                                    }
                                }

                                // последняя версия в задаче
                                double lastversion = Release.VerAsNum(lastverintask);

                                if (lastversion > numversion)
                                {
                                    string info = $"Ветка задачи {branch} содержит версию {lastverintask}. Ветку {branch} нельзя влить в ветку версии {tbBranch.Text}";
                                    ErrInfo += info + Environment.NewLine + Environment.NewLine;
                                    App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFileRelease);

                                    if (System.Windows.Forms.MessageBox.Show(info + Environment.NewLine + Environment.NewLine + $"Да(Yes) - продолжим влитие остальных задач{Environment.NewLine}Нет(No) - Прервать влитие всех задач{Environment.NewLine}{Environment.NewLine}?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                                    {
                                        info = $"Пользователь прервал влитие всех задач в ветку {tbBranch.Text}";
                                        ErrInfo += info + Environment.NewLine + Environment.NewLine;
                                        App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);

                                        return;
                                    }
                                }
                                else
                                {
                                    // Проверям дату последнего комита
                                    var lastcommit = GIT.GitLastCommit(project_dev, branch, logFileRelease);

                                    ymlfile.TaskCommitDate = lastcommit.ToString("dd.MM.yyyy");

                                    if (lastcommit < prevCommitDate)
                                    {
                                        ymlfile.TaskCommitDate = "! " + ymlfile.TaskCommitDate;

                                        badHronologyList += (string.IsNullOrWhiteSpace(badHronologyList) ? "" : ", ") + branch;
                                    }

                                    prevCommitDate = lastcommit;

                                    if (MainWindow.APPinfo.isCheckLastCommit)
                                    {
                                        if (lastcommit < DateTime.Now.AddMonths(-3))
                                        {
                                            string info = $"В ветке задачи {branch} последний комит был {lastcommit.ToString("dd.MM.yyyy")} - более 3-х месяцев назад";
                                            ErrInfo += info + Environment.NewLine + Environment.NewLine;
                                            App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);

                                            // спросим
                                            FormAsk3 dlg1 = new FormAsk3();
                                            dlg1.tbText.Text = info + Environment.NewLine + Environment.NewLine + $"Все равно вливаем ветку {branch} в версию {tbBranch.Text}?";
                                            dlg1.btCancel.Text = "Прервать влитие всех задач";
                                            var res = dlg1.ShowDialog();
                                            dlg1.Dispose();

                                            if (res == System.Windows.Forms.DialogResult.Cancel)
                                            {
                                                info = $"Пользователь прервал влитие всех задач в ветку {tbBranch.Text}";
                                                ErrInfo += info + Environment.NewLine + Environment.NewLine;
                                                App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);

                                                return;
                                            }

                                            if (res == System.Windows.Forms.DialogResult.No)
                                            {
                                                info = $"Пользователь отказался вливать ветку {branch} в ветку {tbBranch.Text}";
                                                ErrInfo += info + Environment.NewLine + Environment.NewLine;
                                                App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);

                                                continue;
                                            }
                                            else
                                            {
                                                info = $"Пользователь решил влил ветку {branch} в ветку {tbBranch.Text}";
                                                ErrInfo += info + Environment.NewLine + Environment.NewLine;
                                                App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);
                                            }
                                        }
                                    }

                                    WinExecute.AddCommand(
                                        App.AppPath,
                                        Path.Combine(App.AppPath, "git_merge.cmd"),
                                        folder + " " + branch + " ORIGIN"
                                        );
                                }
                            }
                        }
                    }
                }

                // проверяем выполнение - наличие yml'файлов задач в папке task, проставляем MergeStatus
                SetMerged(project_dev, listTask);

                // проверяем наличие задач с нарушением хронологии
                if (!string.IsNullOrWhiteSpace(badHronologyList))
                {
                    string info = $"В ветках {badHronologyList} нарушена хронологическая поледовательность комитов";
                    ErrInfo += info + Environment.NewLine + Environment.NewLine;
                    App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);

                    // спросим
                    FormAsk3 dlg1 = new FormAsk3();
                    dlg1.tbText.Text = info + Environment.NewLine + Environment.NewLine + $"Все равно вливаем ветки задач в версию {tbBranch.Text}?";
                    dlg1.btCancel.Text = "Прервать влитие всех задач";
                    dlg1.btNo.Enabled = false; // в этом случае не даем выбрать этот вариант
                    var res = dlg1.ShowDialog();
                    dlg1.Dispose();

                    if (res == System.Windows.Forms.DialogResult.Cancel)
                    {
                        info = $"Пользователь прервал влитие всех задач в ветку {tbBranch.Text}";
                        ErrInfo += info + Environment.NewLine + Environment.NewLine;
                        App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);

                        dgYMLFilesRefresh();

                        return;
                    }
                    else
                    {
                        info = $"Пользователь решил влить ветки всех задач в ветку {tbBranch.Text}";
                        ErrInfo += info + Environment.NewLine + Environment.NewLine;
                        App.AddLog(info, null, App.ShowMessageMode.NONE, true, logFileRelease);
                    }
                }

                // выполняем merge
                if (WinExecute.ListCommands.Count > 0)
                {
                    WinExecute.Start(true, -1, true);
                }

                // проверяем выполнение - наличие yml'файлов задач в папке task, проставляем MergeStatus
                SetMerged(project_dev, listTask);

                dgYMLFilesRefresh();

                // сохраняем задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                    btSaveTask.Focus();
                }

                // показываем
                if (
                    (WinExecute.ListCommands.Count > 0) ||
                    (!string.IsNullOrWhiteSpace(ErrInfo))
                )
                {
                    if (mergeType == MergeType.SINGLE)
                    {
                        if (WinExecute.ListCommands.Count > 0)
                        {
                            string branch = listTask[0].BranchName;
                            App.AddLog($"Merge задачи {branch} в ветку {tbBranch.Text} завершен.", null, App.ShowMessageMode.NONE, true, logFileRelease);
                        }
                    }
                    else
                    {
                        App.AddLog($"Merge задач в ветку {tbBranch.Text} завершен.{Environment.NewLine}{Environment.NewLine}Теперь необходимо собрать yml-файл версии.", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                    }

                    if (System.Windows.Forms.MessageBox.Show("Посмотреть лог merge ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        WinInfo WinInfo = new WinInfo(null);
                        WinInfo.tbInfo.Text = ErrInfo + WinExecute.GetLog();
                        if (
                            (!string.IsNullOrWhiteSpace(WinExecute.execLogFile)) &&
                            File.Exists(WinExecute.execLogFile)
                         )
                        {
                            WinInfo.Title = "Лог merge в файле " + WinExecute.execLogFile;
                        }
                        else
                        {
                            WinInfo.Title = "Лог merge";
                        }
                        WinInfo.ShowDialog();
                    }
                }
                else
                {
                    WinExecute.Close();
                }

            }
        }

        /// <summary>
        /// проверяем выполнение - наличие yml'файлов задач в папке task, проставляем MergeStatus
        /// </summary>
        /// <param name="project_dev">dev-проект разработки</param>
        /// <param name="list">список строк из MainWindow.Task.ReleaseYMLFiles</param>
        private void SetMerged(string project_dev, List<YMLFileInfo> list)
        {
            string project_git = Utilities.GITProjects.GetGITProject(project_dev);
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project_dev);

            // проверяем выполнение - наличие yml'файлов задач в папке task, проставляем MergeStatus
            foreach (var info in list
                .Where(x => x.IsAddRelease == true)
                .OrderBy(x => x.YMLOrder)
            )
            {
                string ymlfield_dev = Utilities.GITProjects.GetYMLFieldByProject(project_dev);
                string ymlfield_git = Utilities.GITProjects.GetYMLFieldByProject(project_git);

                string file_dev = info.GetYMLFile(ymlfield_dev);
                string file_git = info.GetYMLFile(ymlfield_git);

                string file = "";

                if (!string.IsNullOrWhiteSpace(file_dev))
                {
                    file = file_dev;
                }
                else
                {
                    file = file_git;
                }

                file = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, info.PathInGIT, file);

                // Проверяем наличие файла в папке task
                if (File.Exists(file))
                {
                    info.MergeStatus = "merged";
                    info.SetYMLFile(ymlfield_dev, Path.GetFileName(file));
                }
                else
                {
                    info.MergeStatus = "";
                }
            }
        }

        /// <summary>
        /// Нажата кнопка Merge задач в версию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btMergeTask_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            MergeTask(MergeType.ALL, MainWindow.Task.ReleaseYMLFiles);
        }

        /// <summary>
        /// Поиск следующей версии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFindNextVersion_Click(object sender, RoutedEventArgs e)
        {
            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                {
                    MessageBox.Show("Выберите проект ГИТ");
                    cbGITProject.Focus();
                    return;
                }
            }

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));

            FormFindInList dlg1 = new FormFindInList(logFileRelease);

            if (Utilities.GITProjects.IsDEVProject(project))
            {
                dlg1.AddItems(NextVersions
                    .Where(x  => x.Key > numversion)
                    .OrderBy(x => x.Key)
                    .Select(x => x.Value.VisibleName)
                    .ToList()
                );
            }
            else
            {
                dlg1.AddItems(Versions
                    .Where(x => x.Key > numversion)
                    .OrderBy(x => x.Key)
                    .Select(x => x.Value.VisibleName)
                    .ToList()
                );
            }
 
            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string result = "";

                foreach (System.Windows.Forms.DataGridViewRow row in dlg1.dgListValues.SelectedRows)
                {
                    result = row.Cells[0].Value.ToString();
                    // берем только первую
                    break; //-V3020
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    cbNextVersion.SelectedItem = result;
                }
            }

            dlg1.Dispose();
        }

        /// <summary>
        /// Дополнить существующий список задач
        /// </summary>
        /// <param name="NumTask">список новых задач</param>
        /// <returns>список добавленных задач</returns>
        private string AddNewTasks(string NumTask)
        {
            string result = "";

            // убрать фильтры
            btClearFilter_Click(null, null);

            // список добавляемых задач (через запятую, точку запятую или на разных строках) 
            NumTask = NumTask.Trim().ToUpper()
                .Replace(';', ',')
                .Replace('\r', ',')
                .Replace('\n', ',')
                .Replace('\t', ',')
                .Replace(' ', ',')
                .Replace('+', ',')
                .Replace("https://jira.is-mis.ru/browse/".ToUpper(), "")
                .Replace("https://jira.rtmis.ru/browse/".ToUpper(), "")
                .Trim();
            var TaskArr = NumTask.Split(',').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()).ToList();

            foreach (var item in TaskArr)
            {
                string _task = item.Trim();

                if (!string.IsNullOrWhiteSpace(_task))
                {
                    NumTask = Utilities.Task.GetTaskNumber(_task.Trim()).ToUpper();

                    if (string.IsNullOrWhiteSpace(NumTask))
                    {
                        App.AddLog($"Нестандартное имя задачи {_task}" + Environment.NewLine +
                            "Название задачи должно соответствовать шаблону:" + Environment.NewLine +
                            "PROMEDWEB-число" + Environment.NewLine +
                            "RPMS-число" + Environment.NewLine +
                            "BIP-число" + Environment.NewLine +
                            "OPS-число" + Environment.NewLine +
                            "SMP-число" + Environment.NewLine +
                            "RM-число" + Environment.NewLine +
                            "CM-число" + Environment.NewLine +
                            "FERDTM-число"
                            , null, App.ShowMessageMode.SHOW, true, logFileRelease);

                        continue;
                    }

                    // проверить наличие задачи в grid
                    var found = MainWindow.Task.ReleaseYMLFiles.Where(x =>
                        (x.TaskNumber.ToUpper() == NumTask) ||
                        (x.BranchName.ToUpper() == NumTask) ||
                        (x.GetYMLFileDefault.ToUpper() == NumTask + ".YML") ||
                        (x.GetYMLFileDefault.ToUpper() == NumTask + ".JSON")
                        ).FirstOrDefault();

                    if (found != null)
                    {
                        App.AddLog($"Задача {NumTask} уже есть в перечне", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                        dgYMLFiles.SelectedItem = found;
                        if (dgYMLFiles.SelectedItem != null) //-V3022
                        {
                            dgYMLFiles.UpdateLayout();
                            dgYMLFiles.ScrollIntoView(dgYMLFiles.SelectedItem);
                            dgYMLFiles_SelectionChanged(null, null);
                        }
                        continue;
                    }

                    // добавить задачу в общий список задач Jira
                    tbTasks.Text += Environment.NewLine + NumTask;

                    // добавить задачу в список для парсинга
                    result += Environment.NewLine + NumTask;

                    // определить максимальный N п/п
                    int max = 0;
                    if (MainWindow.Task.ReleaseYMLFiles.Count > 0)
                    {
                        max = MainWindow.Task.ReleaseYMLFiles.Max(x => x.YMLOrder);
                    }
                    max += 10;

                    // добавить задачу в grid
                    MainWindow.Task.ReleaseYMLFiles.Add(new YMLFileInfo
                    {
                        IsAddRelease = false,
                        IsRefreshed = false,
                        isUpdated = "",
                        TaskNumber = NumTask,
                        IsFiltered1 = true,
                        IsFiltered2 = true,
                        IsFiltered3 = true,
                        IsFiltered4 = true,
                        YMLOrder = max,
                        PathInGIT = ""
                    });

                    // обновить grid и выбрать эту добавленную строку
                    dgYMLFilesRefresh();
                    found = MainWindow.Task.ReleaseYMLFiles.Where(x =>
                        (x.TaskNumber.ToUpper() == NumTask)
                        ).FirstOrDefault();

                    if (found != null)
                    {
                        dgYMLFiles.SelectedItem = found;
                        if (dgYMLFiles.SelectedItem != null) //-V3022
                        {
                            dgYMLFiles.UpdateLayout();
                            dgYMLFiles.ScrollIntoView(dgYMLFiles.SelectedItem);
                            dgYMLFiles_SelectionChanged(null, null);
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Добавить дополнительную задачу
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAddTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return;
            }

            if (isExecFill == true)
            {
                MessageBox.Show("Идет парсинг задач из Jira, подождите!");
                return;
            }

            // Запросить номер задачи
            FormAskNumTask dlg1 = new FormAskNumTask();
            dlg1.tbNumTask.Text = "";
            string NumTask = "";

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                NumTask = dlg1.tbNumTask.Text.Trim();
            }
            dlg1.Dispose();

            if (string.IsNullOrWhiteSpace(NumTask))
            {
                return;
            }

            string ListTask = AddNewTasks(NumTask);

            // обновить данные о задаче из Jira
            if (!string.IsNullOrWhiteSpace(ListTask))
            {
                FillYMLFiles(tbTasks.Text.Trim(), MainWindow.Task.ReleaseYMLFiles, true, ListTask);
            }
        }

        /// <summary>
        /// Merge выбранной задачи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgYMLFiles_MenuItemMerge(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            if (dgYMLFiles.SelectedIndex >= 0)
            {
                var _yml = dgYMLFiles.SelectedItem as YMLFileInfo;

                MergeTask(MergeType.SINGLE, new List<YMLFileInfo>() { _yml });
            }
        }

        /// <summary>
        /// Изменить имя ветки задачи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgYMLFiles_MenuItemChangeBranchName(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            if (dgYMLFiles.SelectedIndex >= 0)
            {
                var _yml = dgYMLFiles.SelectedItem as YMLFileInfo;

                string project_dev = cbGITProject.SelectedItem.ToString().Trim();
                string project_git = Utilities.GITProjects.GetGITProject(project_dev);

                // только если "новый" проект
                if (Utilities.GITProjects.IsDEVProject(project_dev))
                {
                    // Запросить имя ветки
                    FormAskNumTask dlg1 = new FormAskNumTask();
                    dlg1.tbNumTask.Text = _yml.BranchName;
                    dlg1.Text = "Новое имя ветки";
                    dlg1.lbTitle.Text = "Имя ветки:";

                    if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string newBranchName = dlg1.tbNumTask.Text.Trim();

                        //newBranchName = Utilities.Task.GetTaskNumber(newBranchName).ToUpper();

                        _yml.Branch = newBranchName;
                    }
                    dlg1.Dispose();

                    dgYMLFilesRefresh();

                    // сохраняем задачу
                    if (mainWindow != null)
                    {
                        mainWindow.SaveTaskNoShow();
                        btSaveTask.Focus();
                    }
                }
            }
        }

        /// <summary>
        /// ищем дубли в задачах Jira
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btTaskDoubles_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            if (System.Windows.Forms.MessageBox.Show("Выполнить поиск дублей по списку задач Jira ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                YMLStruct releaseYML = new YMLStruct(null, logFileRelease);

                int cnt = 0;

                string git_project = Utilities.GITProjects.GetGITProject(cbGITProject.SelectedItem.ToString().Trim());
                string dev_project = Utilities.GITProjects.GetDEVProject(cbGITProject.SelectedItem.ToString().Trim());

                string GITProjectFolder = Utilities.GITProjects.GetFolderByProject(git_project);
                string DEVProjectFolder = Utilities.GITProjects.GetFolderByProject(dev_project);

                string git_taskdir = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder, "task");
                string dev_taskdir = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder, "task");

                string git_ymlfield = Utilities.GITProjects.GetYMLFieldByProject(git_project);
                string dev_ymlfield = Utilities.GITProjects.GetYMLFieldByProject(dev_project);

                releaseYML.Project = git_project;

                // переключим новый проект на ветку dev
                if (!string.IsNullOrWhiteSpace(dev_project))
                {
                    GIT.GitSwitch(dev_project, "dev", logFileRelease, out string cur_branch, out string err);
                }

                // перебираем все задачи, включенные в релиз, в том же порядке, что планируется в yml версии
                foreach (var ymlfile in MainWindow.Task.ReleaseYMLFiles
                    .Where(x => x.IsAddRelease == true && x.PathInGIT.ToLower() == "task")
                    .OrderBy(x => x.YMLOrder)
                )
                {
                    if (File.Exists(Path.Combine(git_taskdir, ymlfile.GetYMLFileDefault)))
                    {
                        // Читаем yml-файл в "старом" проекте
                        cnt++;
                        var yml = new YMLLine(releaseYML, logFileRelease)
                        {
                            order = cnt,
                            path = "task",
                            type = YMLLineType.TASK,
                            file = ymlfile.GetYMLFileDefault,
                            isLoaded = false,
                            ReleaseTaskNumber = ymlfile.TaskNumber
                        };

                        yml.loadYMLStruct.LoadYML(git_project, "task", ymlfile.GetYMLFileDefault, false, null, true, false);

                        releaseYML.Lines.Add(yml);
                    }
                    else if (File.Exists(Path.Combine(dev_taskdir, ymlfile.GetYMLFileDefault)))
                    {
                        // Читаем yml-файл в "новом" проекте
                        cnt++;
                        var yml = new YMLLine(releaseYML, logFileRelease)
                        {
                            order = cnt,
                            path = "task",
                            type = YMLLineType.TASK,
                            file = ymlfile.GetYMLFileDefault,
                            isLoaded = false,
                            ReleaseTaskNumber = ymlfile.TaskNumber
                        };

                        yml.loadYMLStruct.LoadYML(dev_project, "task", ymlfile.GetYMLFileDefault, false, null, true, false);

                        releaseYML.Lines.Add(yml);
                    }
                }

                // переключим новый проект обратно на ветку версии
                if (!string.IsNullOrWhiteSpace(dev_project))
                {
                    string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(dev_project);
                    string branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);
                    GIT.GitSwitch(dev_project, branchversion, logFileRelease, out string cur_branch, out string err);
                }

                // на всякий случай проверим
                if (!CheckBranch(out branch)) return;

                // определяем список дублей
                string info = Release.ListDoubles(releaseYML.Lines);

                // выводим результат
                if (!string.IsNullOrWhiteSpace(info))
                {
                    App.AddLog("Есть дубли!", null, App.ShowMessageMode.NONE, true, logFileRelease);

                    WinInfo WinInfo = new WinInfo(null);
                    WinInfo.Title = "Есть дубли в скриптах!";
                    WinInfo.tbInfo.Text = info;

                    WinInfo.Show();
                }
                else
                {
                    App.AddLog("Дублей нет", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                }
            }
        }

        /// <summary>
        /// ищем дубли в yml-файле версии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btYmlDoubles_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            if (System.Windows.Forms.MessageBox.Show($"Выполнить поиск дублей в файле {tbFileVersion.Text.Trim()}?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();
                string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);

                // загружаем yml-файл версии
                YMLStruct releaseYML = new YMLStruct(null, logFileRelease);
                releaseYML.LoadYML(project, "version", tbFileVersion.Text.Trim(), false, null, true, false);

                // перебираем список yml-файлов из версии
                foreach (YMLLine yml in releaseYML.Lines.Where(x => x.type == YMLLineType.TASK))
                {
                    // ищем в списке из Jira информацию о релизной задаче
                    var found = MainWindow.Task.ReleaseYMLFiles
                        .Where(x => x.GetYMLFileDefault == yml.file)
                        .FirstOrDefault();

                    if (found != null)
                    {
                        yml.ReleaseTaskNumber = found.TaskNumber;
                    }
                }

                // определяем список дублей
                string info = Release.ListDoubles(releaseYML.Lines);

                // выводим результат
                if (!string.IsNullOrWhiteSpace(info))
                {
                    App.AddLog("Есть дубли!", null, App.ShowMessageMode.NONE, true, logFileRelease);

                    WinInfo WinInfo = new WinInfo(null);
                    WinInfo.Title = "Есть дубли в скриптах!";
                    WinInfo.tbInfo.Text = info;


                    WinInfo.Show();
                }
                else
                {
                    App.AddLog("Дублей нет", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                }

            }
        }

        /// <summary>
        /// Нажат Enter в поле Фильтр по номеру задачи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFilterTask_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btFindTask_Click(sender, e);
                e.Handled = true;
            }
        }

        /// <summary>
        /// Нажата кнопка Искать
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFindTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbFilterTask.Text))
            {
                // сбросим фильтр
                foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                {
                    item.IsFiltered4 = true;
                }
            }
            else
            {
                string seekvalue = tbFilterTask.Text.Trim().ToLower();

                foreach (var item in MainWindow.Task.ReleaseYMLFiles)
                {
                    item.IsFiltered4 =
                        item.BranchName.ToLower().Contains(seekvalue) ||
                        item.DataBD.ToLower().Contains(seekvalue) ||
                        item.ErrorInfo.ToLower().Contains(seekvalue) ||
                        item.GetYMLFileDefault.ToLower().Contains(seekvalue) ||
                        item.ObjectsBD.ToLower().Contains(seekvalue) ||
                        item.TaskNumber.ToLower().Contains(seekvalue) ||
                        item.UpdActions.ToLower().Contains(seekvalue);
                }
            }
            dgYMLFilesRefresh();
        }

        private void tbGITVersion_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //if (!CheckBranch(out string branch)) return;

            System.Diagnostics.Process.Start(tbGITVersion.Text.Trim());
        }

        /// <summary>
        /// Нажата кнопка GIT PUSH
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGitPush_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();

            if (System.Windows.Forms.MessageBox.Show($"Выполнить GIT PUSH для ветки {branch} в проекте {project} ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                // git push
                GIT.GitPush(new string[] { project }, branch, true, logFileRelease);
            }

        }

        /// <summary>
        /// Нажата кнопка Поиск задачи в другой версии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFindTaskInVersion_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            if (System.Windows.Forms.MessageBox.Show("Выполнить поиск задач из списка в других версиях ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                string git_project = Utilities.GITProjects.GetGITProject(cbGITProject.SelectedItem.ToString().Trim());

                string GITProjectFolder = Utilities.GITProjects.GetFolderByProject(git_project);
                string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(git_project);
                double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));

                this.Cursor = Cursors.Wait;

                string info = "";

                // Проверим, есть ли задачи в предыдущих версиях
                // перебираем все задачи, включенные в релиз, в том же порядке, что планируется в yml версии
                foreach (var ymlfile in MainWindow.Task.ReleaseYMLFiles
                .Where(x => x.IsAddRelease == true && x.PathInGIT.ToLower() == "task")
                .OrderBy(x => x.YMLOrder)
                )
                {
                    // ищем файл в других версиях
                    foreach (var version in Versions
                        .Where(x =>
                            (x.Value != null) &&
                            (x.Value.YMLFile != null) &&
                            (x.Value.NumOrder < numversion)
                            )
                        .OrderBy(x => x.Value.NumOrder))
                    {
                        var found = version.Value.YMLFile.Lines.Where(x =>
                            (x.type == YMLLineType.TASK) &&
                            (x.file.ToLower() == ymlfile.GetYMLFileDefault.ToLower())
                        ).FirstOrDefault();

                        if (found != null)
                        {
                            info += Environment.NewLine + $"{ymlfile.GetYMLFileDefault} уже добавлен в ПРЕДЫДУЩУЮ версию {version.Value.YMLFile.NumVersion}, файл {version.Value.YMLFile.Filename}";
                        }
                    }
                }

                // Проверим, есть ли задачи в следующих версиях
                // перебираем все задачи, включенные в релиз, в том же порядке, что планируется в yml версии
                foreach (var ymlfile in MainWindow.Task.ReleaseYMLFiles
                .Where(x => x.IsAddRelease == true && x.PathInGIT.ToLower() == "task")
                .OrderBy(x => x.YMLOrder)
                )
                {
                    // ищем файл в других версиях
                    foreach (var version in Versions
                        .Where(x =>
                            (x.Value != null) &&
                            (x.Value.YMLFile != null) &&
                            (x.Value.NumOrder > numversion)
                        )
                        .OrderBy(x => x.Value.NumOrder))
                    {
                        var found = version.Value.YMLFile.Lines.Where(x =>
                            (x.type == YMLLineType.TASK) &&
                            (x.file.ToLower() == ymlfile.GetYMLFileDefault.ToLower())
                        ).FirstOrDefault();

                        if (found != null)
                        {
                            info += Environment.NewLine + $"{ymlfile.GetYMLFileDefault} уже добавлен в СЛЕДУЮЩУЮ версию {version.Value.YMLFile.NumVersion}, файл {version.Value.YMLFile.Filename}";
                        }
                    }
                }

                this.Cursor = Cursors.Arrow;

                // выводим результат
                if (!string.IsNullOrWhiteSpace(info))
                {
                    App.AddLog("Задачи включены в другие версии!", null, App.ShowMessageMode.NONE, true, logFileRelease);

                    WinInfo WinInfo = new WinInfo(null);
                    WinInfo.Title = "Есть задачи, которые включены в другие версии!";
                    WinInfo.tbInfo.Text = info;


                    WinInfo.Show();
                }
                else
                {
                    App.AddLog("Задачи в других версиях не встречаются", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                }
            }
        }

        /// <summary>
        /// Проверка релиза на дефекты и связей с другими релизами
        /// </summary>
        /// <param name="project">проект ГИТ</param>
        /// <param name="releaseYML">загруженный yml-файл</param>
        /// <returns></returns>
        private string CheckRelease(string project, ref YMLStruct releaseYML)
        {
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, tbNumVersion.Text));
            double FirstVersionOrder = Utilities.GITProjects.GetFirstVersionOrderByProject(project, tbNumVersion.Text);
            string AllInfo = "";
            releaseYML = new YMLStruct(null, logFileRelease);

            //string git_project = Utilities.GIT.GetGITProject(project);
            //string GITProjectFolder = Utilities.GIT.GetFolderByProject(git_project);

            // загружаем yml-файл версии (со всей историей) из текущего выбранного проекта
            releaseYML.LoadYML(project, "version", tbFileVersion.Text.Trim(), true, null, false, false);

            // перебираем список yml-файлов из версии
            foreach (YMLLine yml in releaseYML.Lines.Where(x => x.type == YMLLineType.TASK))
            {
                // догрузим задачу версии
                yml.loadYMLStruct.ReLoadYML(false, null, true, false);

                // ищем в списке из Jira информацию о релизной задаче
                var found = MainWindow.Task.ReleaseYMLFiles
                    .Where(x => x.GetYMLFileDefault == yml.file)
                    .FirstOrDefault();

                if (found != null)
                {
                    yml.ReleaseTaskNumber = found.TaskNumber;
                }
            }

            // дозаполняем историю для следующих версий
            var cur_ver = releaseYML;
            double cur_num = numversion;

            if ((cur_ver != null) && (cur_ver.ParentYMLLine == null)) //-V3063
            {
                do
                {
                    YMLStruct yml = null;

                    foreach (var item in Versions
                        .Where(x =>
                        (x.Value != null) &&
                        (x.Value.YMLFile != null) &&
                        (x.Value.NumOrder > cur_num) && // следующие версии
                        (x.Value.PrevNumOrder > 0) && // у которых есть ссылка на предыдущую версию
                        (x.Value.PrevNumOrder == cur_num) // ссылаются на текущую 
                        )
                        // сначала кумулятивная версия, потом НЕ кумулятивные
                        .OrderBy(x =>
                        {
                            switch (x.Value.YMLFile.IsNoCumulative)
                            {
                                case true: return x.Value.NumOrder;
                                default: return 0;
                            }
                        }
                        )
                    )
                    {
                        yml = item.Value.YMLFile;
                        break;
                    }

                    if (yml != null)
                    {
                        cur_ver.ParentYMLLine = yml.FirstPrevVersion;
                        cur_num = yml.NumVersionOrder;
                    }

                    cur_ver = yml;

                } while (cur_ver != null);
            }

            // собираем информацию о связях между версиями
            AllInfo = "";
            string info = "";

            // Проверим, есть ли задачи в предыдущих версиях
            // перебираем все задачи, включенные в yml-файл
            foreach (var ymlfile in releaseYML.Lines
                .Where(x => x.type == YMLLineType.TASK)
                .OrderBy(x => x.order)
            )
            {
                // ищем файл в других версиях
                foreach (var version in Versions
                    .Where(x =>
                        (x.Value != null) &&
                        (x.Value.YMLFile != null) &&
                        (x.Value.NumOrder < numversion)
                        )
                    .OrderBy(x => x.Value.NumOrder))
                {
                    var found = version.Value.YMLFile.Lines.Where(x =>
                        (x.type == YMLLineType.TASK) &&
                        (x.file.ToLower() == ymlfile.file.ToLower())
                    ).FirstOrDefault();

                    if (found != null)
                    {
                        info += Environment.NewLine + $"{ymlfile.file} уже добавлен в ПРЕДЫДУЩУЮ версию {version.Value.YMLFile.NumVersion}, файл {version.Value.YMLFile.Filename}";
                    }
                }
            }

            // Проверим, есть ли задачи в следующих версиях
            // перебираем все задачи, включенные в yml-файл
            foreach (var ymlfile in releaseYML.Lines
                .Where(x => x.type == YMLLineType.TASK)
                .OrderBy(x => x.order)
            )
            {
                // ищем файл в других версиях
                foreach (var version in Versions
                    .Where(x =>
                        (x.Value != null) &&
                        (x.Value.YMLFile != null) &&
                        (x.Value.NumOrder > numversion)
                        )
                    .OrderBy(x => x.Value.NumOrder))
                {
                    var found = version.Value.YMLFile.Lines.Where(x =>
                        (x.type == YMLLineType.TASK) &&
                        (x.file.ToLower() == ymlfile.file.ToLower())
                    ).FirstOrDefault();

                    if (found != null)
                    {
                        info += Environment.NewLine + $"{ymlfile.file} уже добавлен в СЛЕДУЮЩУЮ версию {version.Value.YMLFile.NumVersion}, файл {version.Value.YMLFile.Filename}";
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(info))
            {
                if (!string.IsNullOrWhiteSpace(AllInfo)) AllInfo += Environment.NewLine; //-V3022
                AllInfo += "---------------------------------------------------------------------------------------------";
                AllInfo += Environment.NewLine + "Проверка на включение задач в другие версии:";
                AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                AllInfo += info;
                AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------" + Environment.NewLine;
            }

            info = "";

            // Проверим цепочку версий
            if (!string.IsNullOrWhiteSpace(AllInfo)) AllInfo += Environment.NewLine;
            AllInfo += "---------------------------------------------------------------------------------------------";
            AllInfo += Environment.NewLine + "Проверка текущей версии на дефекты:";
            AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";

            if (!File.Exists(Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version", tbFileVersion.Text.Trim())))
            {
                AllInfo += Environment.NewLine + $"ОШИБКА: файл {tbFileVersion.Text.Trim()} для текущей версии {tbNumVersion.Text.Trim()} не найден!";
            }
            else
            {
                if (releaseYML.changesetBefore == null)
                {
                    AllInfo += Environment.NewLine + $"ОШИБКА: в файле {tbFileVersion.Text.Trim()} нет стартового changeSet";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(releaseYML.changesetBefore.id))
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: в стартовом changeSet файла {tbFileVersion.Text.Trim()} не заполнен тег id";
                    }
                    if (string.IsNullOrWhiteSpace(releaseYML.changesetBefore.author))
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: в стартовом changeSet файла {tbFileVersion.Text.Trim()} не заполнен тег author";
                    }
                    if (string.IsNullOrWhiteSpace(releaseYML.changesetBefore.comment))
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: в стартовом changeSet файла {tbFileVersion.Text.Trim()} не заполнен тег comment";
                    }
                }

                if (releaseYML.changesetAfter == null)
                {
                    AllInfo += Environment.NewLine + $"ОШИБКА: в файле {tbFileVersion.Text.Trim()} нет финального changeSet";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(releaseYML.changesetAfter.id))
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: в финальном changeSet файла {tbFileVersion.Text.Trim()} не заполнен тег id";
                    }
                    if (string.IsNullOrWhiteSpace(releaseYML.changesetAfter.author))
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: в финальном changeSet файла {tbFileVersion.Text.Trim()} не заполнен тег author";
                    }
                    if (string.IsNullOrWhiteSpace(releaseYML.changesetAfter.comment))
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: в финальном changeSet файла {tbFileVersion.Text.Trim()} не заполнен тег comment";
                    }
                    if (isImproveSQLinVersionRelease &&
                        (
                            string.IsNullOrWhiteSpace(releaseYML.changesetAfter.labels) ||
                            releaseYML.changesetAfter.labels != "finish"
                        )
                    )
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: в финальном changeSet файла {tbFileVersion.Text.Trim()} не заполнен тег labels: finish";
                    }
                }

                if (releaseYML.IsIgnore)
                {
                    AllInfo += Environment.NewLine + $"ОШИБКА: файл текущей версии {tbFileVersion.Text.Trim()} содержит флаг #IGNORE!";
                }

                // проверяем yml-файл версии
                List<YMLText> ListVersions = new List<YMLText>();
                if (releaseYML.IsYMLFileBAD(ref info, ref ListVersions, false))
                {
                    AllInfo += Environment.NewLine + info;
                }

                // перебираем список yml-файлов из версии и проверяем их
                foreach (YMLLine yml in releaseYML.Lines.Where(x => x.type == YMLLineType.TASK))
                {
                    info = "";
                    if (yml.loadYMLStruct.IsYMLFileBAD(ref info, ref ListVersions, false))
                    {
                        AllInfo += Environment.NewLine + info;
                    }
                }

                if (string.IsNullOrWhiteSpace(releaseYML.NumVersion))
                {
                    AllInfo += Environment.NewLine + $"ОШИБКА: для файла {tbFileVersion.Text.Trim()} не определен номер версии!";
                }
                else if (releaseYML.NumVersion != Release.GetNumVersion(prefix, tbNumVersion.Text))
                {
                    AllInfo += Environment.NewLine + $"ОШИБКА: номер версии {releaseYML.NumVersion} в файле {tbFileVersion.Text.Trim()} не совпадает с текущей версией {tbNumVersion.Text.Trim()}";
                }
                else
                {
                    // список предыдущих версий
                    string err = "";
                    int cnt = 0;
                    string LastPrevVersion = "";
                    string LastPrevVersionFile = "";
                    double prevversion = -1;
                    bool isCycle = false;

                    foreach (var version in releaseYML.PrevVersions.OrderBy(x => x.NumVersionLineOrder))
                    {
                        err += Environment.NewLine + $"- ссылка на предыдущую версию {version.NumVersionLine} ( файл {version.file} )";
                        if (version.NumVersionLineOrder > numversion)
                        {
                            err += Environment.NewLine + $"\tОШИБКА: предыдущая версия {version.NumVersionLine} более поздняя, чем текущая версия {tbNumVersion.Text} !";
                            isCycle = true;
                        }
                        cnt++;

                        // выбираем последнюю предыдущую
                        LastPrevVersion = version.NumVersionLine;
                        LastPrevVersionFile = version.file;
                        prevversion = version.NumVersionLineOrder;
                    }

                    if (cnt == 0)
                    {
                        AllInfo += Environment.NewLine + $"ВНИМАНИЕ: у текущей версии {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ) НЕТ предыдущей версии:";
                        AllInfo += err;
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    }

                    if (cnt > 1)
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: у текущей версии {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ) БОЛЬШЕ ОДНОЙ предыдущей версии:";
                        AllInfo += err;
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    }

                    if (isCycle)
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: у текущей версии {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ) может быть зацикливание:";
                        AllInfo += err;
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";

                    }

                    // список версий, которые ссылаются на текущую
                    err = "";
                    cnt = 0;
                    string FirstNextVersion = "";
                    string FirstNextVersionFile = "";
                    double nextversion = -1;
                    isCycle = false;

                    foreach (var version in Versions
                        .Where(x =>
                            (x.Value != null) &&
                            (x.Value.YMLFile != null)
                        )
                        .OrderBy(x => x.Value.NumOrder))
                    {
                        foreach (var prev in version.Value.YMLFile.PrevVersions
                            .Where(x => x.NumVersionLineOrder == numversion) //-V3024
                            .OrderBy(x => x.NumVersionLineOrder)
                        )
                        {
                            err += Environment.NewLine + $"- в версии {version.Value.YMLFile.NumVersion} ( файл {version.Value.YMLFile.Filename} ) есть ссылка на текущую версию {tbNumVersion.Text.Trim()}";
                            if (version.Value.NumOrder < numversion)
                            {
                                err += Environment.NewLine + $"\tОШИБКА: следующая версия {version.Value.YMLFile.NumVersion} более ранняя, чем текущая версия {tbNumVersion.Text.Trim()} !";
                                isCycle = true;
                            }
                            cnt++;

                            if (nextversion == -1) //-V3024
                            {
                                // выбираем первую следующую версию
                                FirstNextVersion = prev.NumVersionLine;
                                FirstNextVersionFile = prev.file;
                                nextversion = prev.NumVersionLineOrder;
                            }

                        }
                    }

                    if (cnt == 0)
                    {
                        if (releaseYML.IsNoCumulative)
                        {
                            AllInfo += Environment.NewLine + $"У текущей версии {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ) НЕТ следующей версии - есть флаг #NOCUMULATIVE";
                        }
                        else
                        {
                            AllInfo += Environment.NewLine + $"ВНИМАНИЕ: у текущей версии {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ) НЕТ следующей версии";
                        }

                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    }

                    if (cnt > 1)
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: у текущей версии {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ) БОЛЬШЕ ОДНОЙ следующей версии:";
                        AllInfo += err;
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    }

                    if (isCycle)
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: у текущей версии {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ) может быть зацикливание:";
                        AllInfo += err;
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";

                    }

                    // ищем пропущенные версии между текущей и предыдущей
                    err = "";
                    cnt = 0;
                    foreach (var version in Versions
                        .Where(x =>
                            (x.Value != null) &&
                            (x.Value.YMLFile != null) &&
                            (x.Value.NumOrder < numversion) &&
                            (x.Value.NumOrder > prevversion) &&
                            (prevversion > 0)
                        )
                        .OrderBy(x => x.Value.NumOrder)
                    )
                    {
                        if (version.Value.YMLFile.IsNoCumulative)
                        {
                            err += Environment.NewLine + $"- версия {version.Value.YMLFile.NumVersion} ( файл {version.Value.YMLFile.Filename} ) - есть флаг #NOCUMULATIVE";
                        }
                        else
                        {
                            err += Environment.NewLine + $"ВНИМАНИЕ: - версия {version.Value.YMLFile.NumVersion} ( файл {version.Value.YMLFile.Filename} )";
                        }
                        cnt++;
                    }

                    if (cnt > 0)
                    {
                        AllInfo += Environment.NewLine + $"Между предыдущей версией {LastPrevVersion} ( файл {LastPrevVersionFile} ) и текущей {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ) есть пропущенные:";
                        AllInfo += err;
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    }


                    // ищем пропущенные версии между текущей и следующей
                    err = "";
                    cnt = 0;
                    foreach (var version in Versions
                        .Where(x =>
                            (x.Value != null) &&
                            (x.Value.YMLFile != null) &&
                            (x.Value.NumOrder > numversion) &&
                            (x.Value.NumOrder < nextversion) &&
                            (nextversion != -1) //-V3024
                        )
                        .OrderBy(x => x.Value.NumOrder)
                    )
                    {
                        if (version.Value.YMLFile.IsNoCumulative)
                        {
                            err += Environment.NewLine + $"- версия {version.Value.YMLFile.NumVersion} ( файл {version.Value.YMLFile.Filename} ) - есть флаг #NOCUMULATIVE";
                        }
                        else
                        {
                            err += Environment.NewLine + $"ВНИМАНИЕ: - версия {version.Value.YMLFile.NumVersion} ( файл {version.Value.YMLFile.Filename} )";
                        }
                        cnt++;
                    }

                    if (cnt > 0)
                    {
                        AllInfo += Environment.NewLine + $"Между текущей {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ) и следующей версией {FirstNextVersion} ( файл {FirstNextVersionFile} ) есть пропущенные:";
                        AllInfo += err;
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    }

                    // проверяем текущую версию на зацикливание
                    err = "";
                    if (releaseYML.isLooping(out err))
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: для текущей версии {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ) обнаружено зацикливание в {err}";
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    }
                    else
                    {
                        // цепочка кумулятивности для текущей версии
                        string _begin = releaseYML.СumulativeBegin();
                        string _end = releaseYML.СumulativeEnd();

                        if (_begin == _end)
                        {
                            if (releaseYML.IsNoCumulative)
                            {
                                AllInfo += Environment.NewLine + $"Текущая версия {tbNumVersion.Text.Trim()} не включена в кумулятивность! - есть флаг #NOCUMULATIVE";
                            }
                            else
                            {
                                AllInfo += Environment.NewLine + $"ВНИМАНИЕ: Текущая версия {tbNumVersion.Text.Trim()} не включена в кумулятивность!";
                            }
                        }
                        else
                        {
                            AllInfo += Environment.NewLine + $"Цепочка кумулятивности для текущей версии {tbNumVersion.Text.Trim()}:{Environment.NewLine}- начинается с версии {_begin}{Environment.NewLine}- завершается на версии {_end}";
                        }
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";

                        // пропущенные версии (возможно - не кумулятивные)
                        var chain = releaseYML.СumulativeChain();

                        double _first = -1;
                        try
                        {
                            _first = chain.FirstOrDefault().Key;
                        }
                        catch (Exception)
                        {
                        }

                        double _last = -1;
                        try
                        {
                            _last = chain.LastOrDefault().Key;
                        }
                        catch (Exception)
                        {
                        }

                        if (
                                (_last > _first) &&
                                (_first != -1) && //-V3024
                                (_last != -1) //-V3024
                        )
                        {
                            cnt = 0;
                            err = "";
                            foreach (var version in Versions
                                .Where(x =>
                                    (x.Value != null) &&
                                    (x.Value.YMLFile != null) &&
                                    (x.Value.NumOrder > _first) &&
                                    (x.Value.NumOrder < _last)

                                )
                                .OrderBy(x => x.Value.NumOrder)
                            )
                            {
                                if (!chain.ContainsKey(version.Value.NumOrder))
                                {
                                    if (version.Value.YMLFile.IsNoCumulative)
                                    {
                                        err += Environment.NewLine + $"- версия {version.Value.YMLFile.NumVersion} ( файл {version.Value.YMLFile.Filename} ) - есть флаг #NOCUMULATIVE";
                                    }
                                    else
                                    {
                                        err += Environment.NewLine + $"ВНИМАНИЕ: - версия {version.Value.YMLFile.NumVersion} ( файл {version.Value.YMLFile.Filename} )";
                                    }
                                    cnt++;
                                }
                            }

                            if (cnt > 0)
                            {
                                AllInfo += Environment.NewLine + $"Следующие версии пропущены и не входят в цепочку кумулятивности для текущей версии {tbNumVersion.Text.Trim()} ( файл {tbFileVersion.Text.Trim()} ):";
                                AllInfo += err;
                                AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                            }
                        }
                    }

                    // проверяем версии на дефекты
                    if (!string.IsNullOrWhiteSpace(AllInfo)) AllInfo += Environment.NewLine;
                    AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    AllInfo += Environment.NewLine + "Проверка прочих версий на дефекты:";
                    AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";

                    // номер версии в файле и в имени файла не совпадает
                    cnt = 0;
                    err = "";
                    foreach (var version in Versions
                        .Where(x =>
                            (x.Value != null) &&
                            (x.Value.YMLFile != null) &&
                            (x.Value.YMLFile.NumVersionFromChangeset != x.Value.YMLFile.NumVersionFromFilename) &&
                            (x.Value.NumOrder > FirstVersionOrder) // начиная с разрыва кумулятивности
                        )
                        .OrderBy(x => x.Value.NumOrder)
                    )
                    {
                        err += Environment.NewLine + $"- версия {version.Value.YMLFile.NumVersion} ( файл {version.Value.YMLFile.Filename} )";
                        cnt++;
                    }
                    if (cnt > 0)
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: у следующих версий номер версии в changeSet и номер версии в имени файла НЕ совпадают:";
                        AllInfo += err;
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    }

                    // кол-во предыдущих версий больше одной
                    cnt = 0;
                    err = "";
                    foreach (var version in Versions
                        .Where(x =>
                            (x.Value != null) &&
                            (x.Value.YMLFile != null) &&
                            (x.Value.YMLFile.PrevVersions.Count > 1) &&
                            (x.Value.NumOrder > FirstVersionOrder) // начиная с разрыва кумулятивности
                        )
                        .OrderBy(x => x.Value.NumOrder)
                    )
                    {
                        err += Environment.NewLine + $"- версия {version.Value.YMLFile.NumVersion} ( файл {version.Value.YMLFile.Filename} )";
                        cnt++;
                    }
                    if (cnt > 0)
                    {
                        AllInfo += Environment.NewLine + $"ОШИБКА: у следующих версий количество ссылок на предыдущие версии - больше одной:";
                        AllInfo += err;
                        AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    }
                }
            }

            return AllInfo;
        }

        /// <summary>
        /// кнопка Связи с другими версиями
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btFindTaskFromYMLInVersion_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            if (System.Windows.Forms.MessageBox.Show($"Собрать информацию о связях файла {tbFileVersion.Text.Trim()} с другими версиями?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();

                this.Cursor = Cursors.Wait;
                YMLStruct yml = null;
                string AllInfo = CheckRelease(project, ref yml);
                this.Cursor = Cursors.Arrow;

                // выводим результат
                if (!string.IsNullOrWhiteSpace(AllInfo))
                {
                    WinInfo WinInfo = new WinInfo(null);
                    WinInfo.Title = "Информация о связях между версиями!";
                    WinInfo.tbInfo.Text = AllInfo;
                    WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                    WinInfo.Show();
                }
            }
        }

        private void isCumulative_Checked(object sender, RoutedEventArgs e) //-V3013
        {
            MainWindow.Task.ReleaseIsCumulative = isCumulative.IsChecked == true;

            if (cbGITProject.SelectedIndex != -1)
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();
                Fill_cbLiquibot(project);
            }
        }

        private void isCumulative_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.Task.ReleaseIsCumulative = isCumulative.IsChecked == true;

            if (cbGITProject.SelectedIndex != -1)
            {
                string project = cbGITProject.SelectedItem.ToString().Trim();
                Fill_cbLiquibot(project);
            }
        }

        /// <summary>
        /// Нажата кнопка Code Review
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCodeReview_Click(object sender, RoutedEventArgs e)
        {
            if (isExecFill == true)
            {
                MessageBox.Show("Идет парсинг задач из Jira, подождите!");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return;
            }

            if (
                (cbGITProject.SelectedIndex == -1) ||
                (cbGITProject.SelectedItem == null) ||
                string.IsNullOrWhiteSpace(cbGITProject.SelectedItem.ToString())
                )
            {
                {
                    MessageBox.Show("Выберите проект ГИТ");
                    cbGITProject.Focus();
                    return;
                }
            }

            string current_project = cbGITProject.SelectedItem.ToString().Trim();

            string git_project = Utilities.GITProjects.GetGITProject(current_project);
            string git_YMLField = Utilities.GITProjects.GetYMLFieldByProject(git_project);
            string git_ProjectFolder = Utilities.GITProjects.GetFolderByProject(git_project);
            string git_ProjectPath = Path.Combine(MainWindow.APPinfo.GITFolder, git_ProjectFolder);

            string dev_project = Utilities.GITProjects.GetDEVProject(current_project);
            string dev_YMLField = Utilities.GITProjects.GetYMLFieldByProject(dev_project);
            string dev_ProjectFolder = Utilities.GITProjects.GetFolderByProject(dev_project);
            string dev_ProjectPath = Path.Combine(MainWindow.APPinfo.GITFolder, dev_ProjectFolder);

            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(git_project);
            string branchversion = prefix + "." + Release.GetNumVersion(prefix, tbNumVersion.Text);

            // проверим, нужен ли commit в текущей ветке
            if (!GIT.CheckCommit(current_project, logFileRelease, "Code review прерван"))
            {
                return;
            }

            // обновить данные о версии
            if (lastGitPull < DateTime.Now.AddMinutes(-3))
            {
                cbGITProject_Sync(branchversion, false);
            }

            if (!CheckMerged()) return;

            if (Utilities.GITProjects.IsGITProject(current_project))
            {
                // принудительно обновить "старый" и "новый" проекты GIT, в "новом" переключиться на ветку версии
                //GIT.GitPull(new string[] { git_project, dev_project }, branchversion, false, true);

                // принудительно обновить "новый" проект GIT, в "новом" переключиться на ветку версии
                GIT.GitPull(new string[] { dev_project }, branchversion, false, true, false, logFileRelease, false);
            }
            else
            {
                // принудительно обновить только "новый" проект GIT и переключиться на ветку версии
                //GIT.GitPull(new string[] { dev_project }, branchversion, false, true);
            }

            // проверяем ветку
            string branch = GIT.GitCurrentBranch(dev_project, out string err, logFileRelease);
            if (branch.ToLower() != branchversion.ToLower())
            {
                MessageBox.Show($"Что-то пошло не так, как ожидалось!!! Необходимо в проекте {dev_project} вручную изменить ветку на {branchversion}");
                cbGITProject.Focus();
                return;
            }

            string AllInfo = "";
            string info = "";

            // проверить наличие файла версии в обоих проектах
            string git_file = Path.Combine(git_ProjectPath, "version", tbFileVersion.Text.Trim());
            if (Utilities.GITProjects.IsGITProject(current_project))
            {
                if (!File.Exists(git_file))
                {
                    info += $"Для версии {tbNumVersion.Text.Trim()} отсутствует файл версии {git_file} в проекте {git_project}" + Environment.NewLine;
                }
            }

            string dev_file = Path.Combine(dev_ProjectPath, "version", tbFileVersion.Text.Trim());
            bool dev_exists = File.Exists(dev_file);
            if (!dev_exists)
            {
                info += $"Для версии {tbNumVersion.Text.Trim()} отсутствует файл версии {dev_file} в проекте {dev_project}" + Environment.NewLine;
            }

            if (!string.IsNullOrWhiteSpace(info))
            {
                AllInfo += info + Environment.NewLine;
            }

            YMLStruct git_yml = new YMLStruct(null, logFileRelease);
            YMLStruct dev_yml = new YMLStruct(null, logFileRelease);


            // соберем информацию о дефектах версии и связях с другими версиями
            this.Cursor = Cursors.Wait;
            if (Utilities.GITProjects.IsGITProject(current_project))
            {
                // заодно загрузим версию из "старого" проекта
                AllInfo += CheckRelease(git_project, ref git_yml) + Environment.NewLine;
            }
            else
            {
                // заодно загрузим версию из "нового" проекта
                AllInfo += CheckRelease(dev_project, ref dev_yml) + Environment.NewLine;
            }


            // проверка на существование файлов в "старом" проекте
            if (Utilities.GITProjects.IsGITProject(current_project))
            {
                this.Cursor = Cursors.Wait;
                info = "";
                foreach (var ymlfile in git_yml.Lines.Where(x =>
                                           (x.type == YMLLineType.TASK) &&
                                           (x.path == "task") &&
                                           (!string.IsNullOrWhiteSpace(x.file))
                                           ))
                {
                    if (!File.Exists(Path.Combine(git_ProjectPath, ymlfile.path, ymlfile.file)))
                    {
                        // не найден
                        info = info + Environment.NewLine + $"Файл {ymlfile.path}/{ymlfile.file} отсутствует в проекте {git_project}";
                    }
                }
                foreach (var sqlfile in git_yml.ListSQL(false))
                {
                    YMLLine line = sqlfile.Value;

                    if (
                        (line != null) &&
                        (!string.IsNullOrWhiteSpace(line.FullFilename)) &&
                        (!File.Exists(line.FullFilename))
                    )
                    {
                        // не найден
                        info = info + Environment.NewLine + $"Файл {sqlfile.Key} отсутствует в проекте {git_project}";
                    }
                }
                if (!string.IsNullOrWhiteSpace(AllInfo)) AllInfo += Environment.NewLine; //-V3022
                AllInfo += "---------------------------------------------------------------------------------------------";
                AllInfo += Environment.NewLine + $"Следующие файлы, указанные в файле версии, физически отсутствуют в проекте {git_project}";
                AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                AllInfo += info + Environment.NewLine;
            }

            if (dev_exists)
            {
                if (Utilities.GITProjects.IsGITProject(current_project))
                {
                    // загрузим версию из "нового" проекта
                    this.Cursor = Cursors.Wait;
                    dev_yml.LoadYML(dev_project, "version", tbFileVersion.Text.Trim(), false, null, true, false);
                }

                // проверка на существование файлов в "новом" проекте
                this.Cursor = Cursors.Wait;
                info = "";
                foreach (var ymlfile in dev_yml.Lines.Where(x =>
                                           (x.type == YMLLineType.TASK) &&
                                           (x.path == "task") &&
                                           (!string.IsNullOrWhiteSpace(x.file))
                                       ))
                {
                    if (!File.Exists(Path.Combine(dev_ProjectPath, ymlfile.path, ymlfile.file)))
                    {
                        // не найден
                        info = info + Environment.NewLine + $"Файл {ymlfile.path}/{ymlfile.file} отсутствует в проекте {dev_project}";
                    }
                }
                foreach (var sqlfile in dev_yml.ListSQL(false))
                {
                    YMLLine line = sqlfile.Value;

                    if (
                        (line != null) &&
                        (!string.IsNullOrWhiteSpace(line.FullFilename)) &&
                        (!File.Exists(line.FullFilename))
                    )
                    {
                        // не найден
                        info = info + Environment.NewLine + $"Файл {sqlfile.Key} отсутствует в проекте {dev_project}";
                    }
                }
                if (!string.IsNullOrWhiteSpace(AllInfo)) AllInfo += Environment.NewLine; //-V3022
                AllInfo += "---------------------------------------------------------------------------------------------";
                AllInfo += Environment.NewLine + $"Следующие файлы, указанные в файле версии, физически отсутствуют в проекте {dev_project}";
                AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                AllInfo += info + Environment.NewLine;

                if (Utilities.GITProjects.IsGITProject(current_project))
                {
                    // перебираем задачи в git_yml
                    this.Cursor = Cursors.Wait;
                    info = "";
                    foreach (var ymlfile in git_yml.Lines.Where(x =>
                                           (x.type == YMLLineType.TASK) &&
                                           (x.path == "task") &&
                                           (!string.IsNullOrWhiteSpace(x.file))
                                         ))
                    {
                        // ищем в dev_yml
                        var found = dev_yml.Lines.Where(x =>
                                           (x.type == YMLLineType.TASK) &&
                                           (x.path == "task") &&
                                           (x.file == ymlfile.file)
                                           )
                                        .FirstOrDefault();

                        if (found == null)
                        {
                            // не найден
                            info = info + Environment.NewLine + $"Файл {ymlfile.path}/{ymlfile.file} есть в версии {git_file}, но отсутствует в версии {dev_file}";
                        }
                    }

                    // перебираем задачи в dev_yml
                    foreach (var ymlfile in dev_yml.Lines.Where(x =>
                                           (x.type == YMLLineType.TASK) &&
                                           (x.path == "task") &&
                                           (!string.IsNullOrWhiteSpace(x.file))
                                           ))
                    {
                        // ищем в dev_yml
                        var found = git_yml.Lines.Where(x =>
                                           (x.type == YMLLineType.TASK) &&
                                           (x.path == "task") &&
                                           (x.file == ymlfile.file)
                                           )
                                        .FirstOrDefault();

                        if (found == null)
                        {
                            // не найден
                            info = info + Environment.NewLine + $"Файл {ymlfile.path}/{ymlfile.file} есть в версии {dev_file}, но отсутствует в версии {git_file}";
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(AllInfo)) AllInfo += Environment.NewLine; //-V3022
                    AllInfo += "---------------------------------------------------------------------------------------------";
                    AllInfo += Environment.NewLine + $"Сравнение файла версии {tbFileVersion.Text.Trim()} в проектах {git_project} и {dev_project}";
                    AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
                    AllInfo += info + Environment.NewLine;
                }
            }


            YMLStruct check_yml = null;

            if (Utilities.GITProjects.IsGITProject(current_project))
            {
                check_yml = git_yml;
            }
            else
            {
                check_yml = dev_yml;
            }

            info = "";

            // перебираем результат парсинга задач из Jira
            foreach (var task in MainWindow.Task.ReleaseYMLFiles
                .Where(x => x.IsAddRelease == true && x.PathInGIT.ToLower() == "task")
                .OrderBy(x => x.YMLOrder)
            )
            {
                // получаем название файла
                string taskfile = task.GetYMLFile(git_YMLField);
                if (string.IsNullOrWhiteSpace(taskfile))
                {
                    taskfile = task.GetYMLFile(dev_YMLField);
                }

                if (!string.IsNullOrWhiteSpace(taskfile))
                {
                    // ищем в yml-файле версии
                    var found = check_yml.Lines
                                   .Where(x =>
                                       (x.type == YMLLineType.TASK) &&
                                       (x.path == "task") &&
                                       (x.file.ToLower() == taskfile.ToLower())
                                   ).FirstOrDefault();

                    if (found == null)
                    {
                        // не найден
                        info = info + Environment.NewLine + $"Файл {taskfile} из задачи {task.TaskNumber} отсутствует в {check_yml.Filename}";
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(AllInfo)) AllInfo += Environment.NewLine; //-V3022
            AllInfo += "---------------------------------------------------------------------------------------------";
            AllInfo += Environment.NewLine + "Задачи есть в списке из Jira но отсутствуют в yml-файле версии";
            AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
            AllInfo += info + Environment.NewLine;

            info = "";
            // перебираем задачи из yml-файла версии
            foreach (var ymlfile in check_yml.Lines.Where(x =>
                                   (x.type == YMLLineType.TASK) &&
                                   (x.path == "task") &&
                                   (!string.IsNullOrWhiteSpace(x.file))
                                 ))
            {
                // ищем в списке задач из Jira
                var found = MainWindow.Task.ReleaseYMLFiles
                                .Where(x =>
                                    (x.IsAddRelease == true && x.PathInGIT.ToLower() == "task") &&
                                    (
                                        x.GetYMLFile(git_YMLField).ToLower() == ymlfile.file.ToLower() ||
                                        x.GetYMLFile(dev_YMLField).ToLower() == ymlfile.file.ToLower()
                                    )
                                )
                                .FirstOrDefault();

                if (found == null)
                {
                    // не найден
                    info = info + Environment.NewLine + $"Файл {ymlfile.file} есть в {check_yml.Filename}, но отсутствует в задачах из Jira";
                }
            }

            if (!string.IsNullOrWhiteSpace(AllInfo)) AllInfo += Environment.NewLine; //-V3022
            AllInfo += "---------------------------------------------------------------------------------------------";
            AllInfo += Environment.NewLine + "Задачи есть в yml-файле версии, но отсутствуют в списке из Jira";
            AllInfo += Environment.NewLine + "---------------------------------------------------------------------------------------------";
            AllInfo += info + Environment.NewLine;

            this.Cursor = Cursors.Arrow;

            if (Utilities.GITProjects.IsDEVProject(current_project))
            {
                string ProjectFolder = Utilities.GITProjects.GetFolderByProject(current_project);
                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                if (!string.IsNullOrWhiteSpace(ProjectFolder))
                {
                    // Отображаем информацию о необходимости push
                    Utilities.External.ExecuteFile(
                        App.AppPath,
                        Path.Combine(App.AppPath, "git_needpush.sh"),
                        folder,
                        false,
                        false,
                        false,
                        false,
                        logFileRelease
                    );

                    // Отображаем список веток, не влитых в master
                    Utilities.External.ExecuteFile(
                        App.AppPath,
                        Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "git-nomerged-ver.sh"),
                        folder,
                        false,
                        false,
                        false,
                        false,
                        logFileRelease
                    );
                }
            }

            // выводим результат проверок
            if (!string.IsNullOrWhiteSpace(AllInfo))
            {
                WinInfo WinInfo = new WinInfo(logFileRelease);
                WinInfo.Title = $"Результат проверки {check_yml.Filename}";
                WinInfo.tbInfo.Text = AllInfo;
                WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                WinInfo.Show();
            }

        }

        /// <summary>
        /// Нажата кнопка "Список на компиляцию"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCompileProc_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();

            string YMLFile = tbFileVersion.Text.Trim();
            if ((YMLFile.Length < 4) || (YMLFile.Substring(YMLFile.Length - 4, 4).ToLower() != ".yml")) YMLFile += ".yml";

            List<string> list = new List<string>();

            this.Cursor = Cursors.Wait;

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);

            if (!string.IsNullOrWhiteSpace(ProjectFolder))
            {
                if (!File.Exists(Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version", YMLFile)))
                {
                    App.AddLog("Файл " + Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version", YMLFile) + " НЕ существует !", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                    tbFileVersion.Focus();
                    return;
                }

                // загружаем yml-файл
                YMLStruct loadyml = new YMLStruct(null, logFileRelease);
                loadyml.LoadYML(project, "version", YMLFile, false, null, true, false);

                // Получаем список для проверки\компиляции хранимок
                list = loadyml.ListCheckProc(false);
            }

            this.Cursor = Cursors.Arrow;

            WinInfo WinInfo = new WinInfo(null);
            WinInfo.Title = "Список команд для проверки\\компиляции хранимок";
            WinInfo.tbInfo.Text = String.Join(Environment.NewLine, list.ToArray());
            WinInfo.Show();
        }

        /// <summary>
        /// Выход из поля "Список задач"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTasks_LostFocus(object sender, RoutedEventArgs e)
        {
            tbTasks.Text = TaskCalcCount();
        }

        /// <summary>
        /// Действия при выборе мышкой ячейки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgYMLFiles_MouseDown(object sender, MouseButtonEventArgs e)
        {
            dgYMLFiles_SelectionChanged(sender, null);
        }

        private void dgYMLFiles_MenuItemTaskCheck(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            if (!CheckMerged()) return;

            if (dgYMLFiles.SelectedIndex >= 0)
            {
                var _yml = dgYMLFiles.SelectedItem as YMLFileInfo;

                if (!string.IsNullOrWhiteSpace(_yml.GetYMLFileDefault)) {

                    string project = cbGITProject.SelectedItem.ToString().Trim();

                    YMLStruct yml = new YMLStruct(null, logFileRelease);
                    yml.LoadYML(project, _yml.PathInGIT, _yml.GetYMLFileDefault, false, null, true, false);

                    if (yml.IsFileExist)
                    {
                        CheckYML(yml);
                    }
                    else
                    {
                        App.AddLog($"В проекте {project} нет файла {_yml.PathInGIT}\\{_yml.GetYMLFileDefault}", null, App.ShowMessageMode.SHOW, true, logFileRelease);
                    }
                }
            }
        }

        /// <summary>
        /// Нажата кнопка "История таблиц"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCheckTableInPrevVers_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            if (System.Windows.Forms.MessageBox.Show($"Собрать информацию о таблицах версии?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                this.Cursor = Cursors.Wait;

                string project = cbGITProject.SelectedItem.ToString().Trim();
                string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);

                // загружаем yml-файл версии со всеми предыдущими версиями (до разрыва кумулятивности)
                YMLStruct releaseYML = new YMLStruct(null, logFileRelease);
                releaseYML.LoadYML(project, "version", tbFileVersion.Text.Trim(), true, null, true, false);

                string info =
                "--------------------------------------------------------------------" + Environment.NewLine +
                $"История таблиц текущей версии {releaseYML.NumVersion} - в каких версиях они встречаются в первый раз (с момента разрыва кумулятивности)" + Environment.NewLine +
                "--------------------------------------------------------------------" + Environment.NewLine +
                Environment.NewLine;

                // Соберем таблицы в предыдущих версиях
                List<YMLLine> listTablesInPrevVers = new List<YMLLine>();

                foreach (var prevvers in releaseYML.PrevVersions.OrderBy(x => x.order))
                {
                    if (prevvers.loadYMLStruct != null)
                    {
                        listTablesInPrevVers.AddRange(prevvers.loadYMLStruct
                            .ListSQL(true)
                            .Where(x => x.Value.GITKindObject == "STRUCT")
                            .Select(x => x.Value)
                            );
                    }
                }

                // перебираем список sql-файлов таблиц из текущей версии
                foreach (var sqltable in releaseYML.ListSQL(false).Where(x => x.Value.GITKindObject == "STRUCT").Select(x => x.Value))
                {
                    // по каждой таблице находим, в какой из предыдущих версий она встретилась в первый раз
                    var sql = listTablesInPrevVers.Where(x =>
                        x.FullFilename.Equals(sqltable.FullFilename)
                    ).FirstOrDefault();

                    if (sql != null)
                    {
                        info += $"Таблица {sqltable.GITSchemaObject}.{sqltable.GITNameObject} файл {sqltable.FullFilename} - задача {sql.parentYMLStruct.Filename} версия {sql.parentYMLStruct.ParentYMLLine.parentYMLStruct.Filename}" + Environment.NewLine;
                    }
                }

                this.Cursor = Cursors.Arrow;

                // выводим результат
                if (!string.IsNullOrWhiteSpace(info))
                {
                    WinInfo WinInfo = new WinInfo(null);
                    WinInfo.Title = "Информация о связях между версиями!";
                    WinInfo.tbInfo.Text = info;
                    WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                    WinInfo.Show();
                }
            }

        }

        private void isCheckLastCommit_Checked(object sender, RoutedEventArgs e)
        {
            MainWindow.APPinfo.CheckLastCommit = "true";
        }

        private void isCheckLastCommit_Unchecked(object sender, RoutedEventArgs e)
        {
            MainWindow.APPinfo.CheckLastCommit = "false";
        }

        /// <summary>
        /// Нажата кнопка "Добавить разрыв кумулятивности"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAddBreakCumulative_Click(object sender, RoutedEventArgs e)
        {
            if (!CheckBranch(out string branch)) return;

            string project = cbGITProject.SelectedItem.ToString().Trim();
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            double FirstVersionOrder = Utilities.GITProjects.GetFirstVersionOrderByProject(project, tbNumVersion.Text);

            double current_num = Release.VerAsNum(branch);

            if (
                Utilities.GITProjects.IsDEVProject(project) &&
                (Versions.Count() > 0)
            )
            {
                // Выбрать предыдущую версию, в которую надо добавить разрыв. По умолчанию - на 2 сервиса пака ранее
                FormFindInList dlg1 = new FormFindInList(logFileRelease);

                // предыдущий разрыв
                foreach (var item in Versions
                    .OrderByDescending(x => x.Key)
                    .Where(x =>
                        (x.Key < current_num) &&
                        (x.Key >= FirstVersionOrder) &&
                        (x.Value.YMLFile.changesetPreConditions != null) &&
                        (x.Value.YMLFile.changesetPreConditions
                            .FirstOrDefault(y => y.isPreConditions && y.id == "cumulative_gap") != null
                          )
                        )
                )
                {
                    if (FirstVersionOrder < item.Key)
                    {
                        FirstVersionOrder = item.Key;
                    }
                    break; //-V3020
                }

                // Заполняем список
                var list = Versions
                    .Where(x =>
                        (x.Key < current_num) &&
                        (x.Key > FirstVersionOrder) &&
                        YMLStruct.isSP(x.Value.NumOrder) // разрыв можно добавить только в SP
                        );

                if (list.Count() > 0)
                {
                    dlg1.AddItems(list
                        .OrderByDescending(x => x.Key)
                        .Select(x => x.Value.Branch)
                        .ToList()
                     );
                }

                // выбираем версию
                string prev_branch = "";
                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (System.Windows.Forms.DataGridViewRow row in dlg1.dgListValues.SelectedRows)
                    {
                        prev_branch = row.Cells[0].Value.ToString();
                        // берем только первую
                        break; //-V3020
                    }
                }
                dlg1.Dispose();

                // добавляем разрыв
                if (!string.IsNullOrWhiteSpace(prev_branch))
                {
                    double prev_num = Release.VerAsNum(prev_branch);

                    var prev_ver = Versions
                        .FirstOrDefault(x => x.Key == prev_num) //-V3024
                        .Value;

                    if (prev_ver != null)
                    {
                        var prev_yml = prev_ver.YMLFile;

                        if (
                            prev_yml.IsFileExist && //выбранный файл существует
                            (prev_yml.PrevVersions.Count > 0) && // в нем есть ссылка на предыдущую версию
                            (!prev_ver.YMLFile.hasCumulativeGap) // в нем нет разрыва кумулятивности
                        )
                        {
                            // Спросить о добавлении разрыва кумулятивности
                            if (System.Windows.Forms.MessageBox.Show($"Добавить разрыв кумулятивности в файл {prev_yml.Filename} ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                            {
                                // Выбрать ближайшую предыдущую версию
                                var prev_prev = prev_yml.PrevVersions.OrderByDescending(x => x.NumVersionLineOrder).FirstOrDefault();

                                prev_prev.loadYMLStruct.ReLoadYML(false, null, false, false); //-V3146

                                // Добавить разрыв кумулятивности
                                YMLChangeset cumulativeGAP = new YMLChangeset()
                                {
                                    id = "cumulative_gap",
                                    author = MainWindow.Task.TaskExecutor,
                                    runAlways = "true",
                                    preConditions = new YMLPreConditions()
                                    {
                                        onFail = "HALT",
                                        changeSetExecuted = new YMLChangeSetExecuted()
                                        {
                                            id = prev_prev.loadYMLStruct.changesetAfter.id,
                                            author = prev_prev.loadYMLStruct.changesetAfter.author,
                                            changeLogFile = prev_prev.path_to_file
                                        },
                                        onFailMessage = $"{prev_prev.path_to_file} not installed or not completed"
                                    }
                                };

                                prev_ver.YMLFile.changesetPreConditions = new List<YMLChangeset>();
                                prev_ver.YMLFile.changesetPreConditions.Add(cumulativeGAP);

                                string info = $"В проекте {project} в файл {prev_prev.path_to_file} добавлен разрыв кумулятивности. Необходимо выполнить commit + push";

                                // удаляем существующие ссылки на предыдущме версии
                                foreach (var item in prev_yml.Lines.Where(x => x.type == YMLLineType.VERSION).ToList())
                                {
                                    prev_yml.DeleteYML(item);
                                }

                                // генерация yml-файл
                                prev_yml.SaveYML(false, false, MainWindow.APPinfo.relativeToChangelogFile == "true", false, false, false, "", false, "");

                                App.AddLog(info, null, App.ShowMessageMode.SHOW, true, logFileRelease);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Нажата кнопка загрузки списка YML-файлов из резервной копии
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btLoadYMLFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return;
            }

            if (isExecFill == true)
            {
                MessageBox.Show("Идет парсинг задач из Jira, подождите!");
                return;
            }

            // выбрать резервную копию
            string filename = Controls.Dialogs.OpenFileDialog(Path.Combine(MainWindow.Task.TaskPath,"BACKUP"), ".backup", "(*.backup)|*.backup|Все файлы (*.*)|*.*");

            if (!string.IsNullOrWhiteSpace(filename))
            {
                if (File.Exists(filename))
                {
                    if (MainWindow.Task.ReleaseYMLFiles.Count() > 0)
                    {
                        // сделаем резервную копию текущих файлов
                        Release.BackupYMLFiles(tbNumVersion.Text, logFileRelease);
                    }

                    // загружаем выбранную резервную копию
                    try
                    {
                        string jsonString = File.ReadAllText(filename);

                        MainWindow.Task.ReleaseYMLFiles.Clear();

                        MainWindow.Task.ReleaseYMLFiles.AddRange(JsonSerializer.Deserialize<List<YMLFileInfo>>(jsonString, new JsonSerializerOptions { IgnoreReadOnlyProperties = true, WriteIndented = true }));

                        btClearFilter_Click(null, null);

                        dgYMLFiles.Focus();

                        cbGITProject.SelectedIndex = -1;

                        App.AddLog($"Загружен {filename} с резервной копией YML\\JSON-файлов версии {tbNumVersion.Text}", null, App.ShowMessageMode.NONE, true, logFileRelease);
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFileRelease);
                    }
                }
            }
        }

        /// <summary>
        /// Нажата кнопка очистки списка YML-файлов и сохранение в резервную копию
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btClearYMLFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return;
            }

            if (isExecFill == true)
            {
                MessageBox.Show("Идет парсинг задач из Jira, подождите!");
                return;
            }

            if (MainWindow.Task.ReleaseYMLFiles.Count() > 0)
            {
                // сделаем резервную копию текущих файлов
                Release.BackupYMLFiles(tbNumVersion.Text, logFileRelease);
            }

            // очищаем список файлов
            MainWindow.Task.ReleaseYMLFiles.Clear();

            btClearFilter_Click(null, null);

            cbGITProject.SelectedIndex = -1;

            App.AddLog($"Очищен список YML-файлов версии {tbNumVersion.Text}", null, App.ShowMessageMode.NONE, true, logFileRelease);
        }

        /// <summary>
        /// Нажата кнопка для копирования команды для ликвибота в буфер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCopyLiquibot_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(cbLiquibot.Text.Trim());
            }
            catch (Exception ex)
            {
                App.AddLog($"Неизвестная ошибка при копировании в буфер:\n", ex, App.ShowMessageMode.SHOW, true, logFileRelease);
            }
        }

        private void btUp_Click(object sender, RoutedEventArgs e)
        {
            if (!dgYMLFiles.IsFocused) dgYMLFiles.Focus();

            if (dgYMLFiles.SelectedIndex >= 0)
            {
                // выбранная строка
                YMLFileInfo yml = dgYMLFiles.SelectedItem as YMLFileInfo;

                if (yml != null)
                {
                    var prev_yml = MainWindow.Task.ReleaseYMLFiles
                        .Where(x => x.YMLOrder < yml.YMLOrder)
                        .OrderByDescending(x => x.YMLOrder)
                        .FirstOrDefault();

                    if (prev_yml != null)
                    {
                        // есть предыдущая строка
                        var prev_order = prev_yml.YMLOrder;
                        prev_yml.YMLOrder = yml.YMLOrder;
                        yml.YMLOrder = prev_order;
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgYMLFilesRefresh();
            }
        }

        private void btDown_Click(object sender, RoutedEventArgs e)
        {
            if (!dgYMLFiles.IsFocused) dgYMLFiles.Focus();

            if (dgYMLFiles.SelectedIndex >= 0)
            {
                // выбранная строка
                YMLFileInfo yml = dgYMLFiles.SelectedItem as YMLFileInfo;

                if (yml != null)
                {
                    var next_yml = MainWindow.Task.ReleaseYMLFiles
                        .Where(x => x.YMLOrder > yml.YMLOrder)
                        .OrderBy(x => x.YMLOrder)
                        .FirstOrDefault();

                    if (next_yml != null)
                    {
                        // есть предыдущая строка
                        var next_order = next_yml.YMLOrder;
                        next_yml.YMLOrder = yml.YMLOrder;
                        yml.YMLOrder = next_order;
                    }
                }

                // сохранить текущую задачу
                if (mainWindow != null)
                {
                    mainWindow.SaveTaskNoShow();
                }

                dgYMLFilesRefresh();
            }
        }

        /// <summary>
        /// Скопировать в буфер все команды ликвибота
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCopyAllLiquibot_Click(object sender, RoutedEventArgs e)
        {
            if (cbLiquibot.Items.Count > 0)
            {
                try
                {
                    string all = "";

                    foreach (var item in cbLiquibot.Items)
                    {
                        all += (string)item + Environment.NewLine;
                    }

                    Clipboard.SetText(all);
                }
                catch (Exception ex)
                {
                    App.AddLog($"Неизвестная ошибка при копировании в буфер:\n", ex, App.ShowMessageMode.SHOW, true, logFileRelease);
                }
            }
        }

        /// <summary>
        /// Добавить принудительно YML или JSON файл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgYMLFiles_MenuItemAddFile(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNumVersion.Text))
            {
                MessageBox.Show("Заполните номер версии!");
                tbNumVersion.Focus();
                return;
            }

            if (isExecFill == true)
            {
                MessageBox.Show("Идет парсинг задач из Jira, подождите!");
                return;
            }

            if (dgYMLFiles.SelectedIndex >= 0)
            {
                var _yml = dgYMLFiles.SelectedItem as YMLFileInfo;

                // Запросить добавляемые файлы
                FormAskNumTask dlg1 = new FormAskNumTask();
                dlg1.Text = "Добавить файл(ы) для задачи " + _yml.TaskNumber;
                dlg1.lbTitle.Text = "URL в web-репозитории:";
                dlg1.tbNumTask.Text = "";
                string urlfiles = "";

                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    urlfiles = dlg1.tbNumTask.Text.Trim();
                }
                dlg1.Dispose();

                if (string.IsNullOrWhiteSpace(urlfiles))
                {
                    return;
                }

                // убрать фильтры
                btClearFilter_Click(null, null);

                // список добавляемых файлов (через запятую, точку запятую или на разных строках) 
                urlfiles = urlfiles.Trim()
                    .Replace(';', ',')
                    .Replace('\r', ',')
                    .Replace('\n', ',')
                    .Replace('\t', ',')
                    .Replace(' ', ',')
                    .Replace('+', ',')
                    .Trim();

                var ListFiles = urlfiles
                    .Split(',')
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .ToList();

                int _cnt = 0;

                foreach (var item in ListFiles)
                {
                    string url = item.Trim();

                    if (!url.Contains("git.promedweb.ru"))
                    {
                        App.AddLog($"{url} должен быть ссылкой на файл в git.promedweb.ru", null, App.ShowMessageMode.SHOW, true, logFileRelease);

                        continue;
                    }

                    // выделяем составляющие из url
                    string url_project = "";
                    string url_file = "";
                    string url_branch = "";
                    string url_path = "";

                    foreach (var project in MainWindow.APPinfo.GITProjects
                            .Where(x => !string.IsNullOrWhiteSpace(x.DEVUrl))
                            .Select(x => x.DEVProject)
                            .Union(
                                MainWindow.APPinfo.GITProjects
                                    .Where(x => !string.IsNullOrWhiteSpace(x.GITUrl))
                                    .Select(x => x.GITProject)
                            )
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                        )
                    {
                        string mask = Utilities.GITProjects.GetURLTaskByProject(project);

                        if (HTML.GetYMLFileFromURL(url, mask, out string _file, out string _path, out string _branch, out string _rest))
                        {
                            url_file = _file;
                            url_project = project;
                            url_branch = _branch;
                            url_path = _path;
                            break;
                        }
                    }

                    if (
                        string.IsNullOrWhiteSpace(url_project) ||
                        string.IsNullOrWhiteSpace(url_branch) ||
                        (
                            url_path != "task" && 
                            url_path != "deployment" && 
                            url_path != "cron" && 
                            !url_path.StartsWith("cron")
                        ) ||
                        (!url_file.ToLower().EndsWith(".yml") && !url_file.ToLower().EndsWith(".json"))
                        )
                    {
                        App.AddLog($"Некорректный {url}. Должна быть ссылка на конкретный yml- или json-файл в проекте GIT в папках task, deployment или cron", null, App.ShowMessageMode.SHOW, true, logFileRelease);

                        continue;
                    }

                    string ymlfield = Utilities.GITProjects.GetYMLFieldByProject(url_project);

                    // проверить наличие файла в grid
                    var found = MainWindow.Task.ReleaseYMLFiles.Where(x =>
                        (x.TaskNumber.ToUpper() == _yml.TaskNumber.ToUpper()) &&
                        (x.BranchName.ToUpper() == url_branch.ToUpper()) &&
                        (x.PathInGIT.ToLower() == url_path.ToLower()) &&
                        (x.GetYMLFile(ymlfield).ToUpper() == url_file.ToUpper())
                        ).FirstOrDefault();

                    if (found != null)
                    {
                        App.AddLog($"Файл {url} уже есть в перечне", null, App.ShowMessageMode.SHOW, true, logFileRelease);

                        dgYMLFiles.SelectedItem = found;

                        if (dgYMLFiles.SelectedItem != null) //-V3022
                        {
                            dgYMLFiles.UpdateLayout();
                            dgYMLFiles.ScrollIntoView(dgYMLFiles.SelectedItem);
                            dgYMLFiles_SelectionChanged(null, null);
                        }

                        continue;
                    }

                    _cnt++;

                    // добавить задачу в grid
                    var new_yml = new YMLFileInfo();
                    new_yml.DataBD = _yml.DataBD;
                    new_yml.IsBaseRegion = _yml.IsBaseRegion;
                    new_yml.IsDowntime = _yml.IsDowntime;
                    new_yml.MergeStatus = _yml.MergeStatus;
                    new_yml.ObjectsBD = _yml.ObjectsBD;
                    new_yml.Order = _yml.Order;
                    new_yml.Region = _yml.Region;
                    new_yml.TaskCommitDate = _yml.TaskCommitDate;
                    new_yml.TaskNumber = _yml.TaskNumber;
                    new_yml.TaskStatus = _yml.TaskStatus;
                    new_yml.UpdActions = _yml.UpdActions;
                    new_yml.Version = _yml.Version;
                    new_yml.IsAddRelease = true;
                    new_yml.IsRefreshed = false;
                    new_yml.isUpdated = "Добавлен вручную";
                    new_yml.IsFiltered1 = true;
                    new_yml.IsFiltered2 = true;
                    new_yml.IsFiltered3 = true;
                    new_yml.IsFiltered4 = true;
                    new_yml.YMLOrder = _yml.YMLOrder + _cnt;
                    new_yml.PathInGIT = url_path;
                    new_yml.Branch = url_branch;

                    new_yml.SetYMLFile(ymlfield,url_file);

                    MainWindow.Task.ReleaseYMLFiles.Add(new_yml);

                    // обновить grid и выбрать эту добавленную строку
                    dgYMLFilesRefresh();

                    found = MainWindow.Task.ReleaseYMLFiles.Where(x =>
                        (x.TaskNumber.ToUpper() == _yml.TaskNumber.ToUpper()) &&
                        (x.BranchName.ToUpper() == url_branch.ToUpper()) &&
                        (x.PathInGIT.ToLower() == url_path.ToLower()) &&
                        (x.GetYMLFile(ymlfield).ToUpper() == url_file.ToUpper())
                        ).FirstOrDefault();

                    if (found != null)
                    {
                        dgYMLFiles.SelectedItem = found;
                        if (dgYMLFiles.SelectedItem != null) //-V3022
                        {
                            dgYMLFiles.UpdateLayout();
                            dgYMLFiles.ScrollIntoView(dgYMLFiles.SelectedItem);
                            dgYMLFiles_SelectionChanged(null, null);
                        }
                    }
                }
            }
        }
    }
}
