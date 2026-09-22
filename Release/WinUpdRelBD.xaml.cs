// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Highlighting;
using SQLGen.Controls;
using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using Path = System.IO.Path;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>
    /// Окно генерации списка команд для обновления релизного стенда
    /// </summary>
    public partial class WinUpdRelBD
    {
        /// <summary>
        /// единый лог-файл текущего сеанса сборки релиза
        /// </summary>
        public string logFileRelease;

        /// <summary>
        /// список веток версий
        /// </summary>
        public List<string> ListBranch = new List<string>();

        /// <summary>
        /// Список Deployment Plan
        /// </summary>
        public List<DeploymentPlan> ListDeploymentPlan = new List<DeploymentPlan>();

        /// <summary>Конструктор WinUpdRelBD</summary>
        public WinUpdRelBD()
        {
            InitializeComponent();

            // лог-файл сборки релиза
            logFileRelease = MainWindow.Task.LogFileRelease;

            // инициализация фильтра
            SetProjectFilter();

            // подсветка лога
            tbList.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");

            // пользовательские настройки GUI
            Default.InitGUI("WinUpdRelBD", this, mainGrid, null, null, null, logFileRelease);
        }

        /// <summary>При закрытии окна WinUpdRelBD</summary>
        private void winUpdRelBD_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinUpdRelBD", this, null);
        }

        /// <summary>
        /// Разные сценарии генерации
        /// </summary>
        /// <param name="isJenkins">=false - заполняем tbList; =true - запускаем FormJenkinsExec</param>
        private void Generate(bool isJenkins)
        {
            // проверки
            if (
                cbPrefix == null ||
                cbPrefix.SelectedIndex < 0
            )
            {
                System.Windows.MessageBox.Show("Необходимо выбрать префикс");
                return;
            }

            if (
                cbStand == null ||
                cbStand.SelectedIndex < 0
            )
            {
                System.Windows.MessageBox.Show("Необходимо выбрать стенд");
                return;
            }

            if (
                cbFromVersion == null ||
                cbFromVersion.SelectedIndex < 0
            )
            {
                System.Windows.MessageBox.Show("Необходимо выбрать версию, с которой начинаем обновлять (включительно)");
                return;
            }

            if (
                cbToVersion == null ||
                cbToVersion.SelectedIndex < 0
            )
            {
                System.Windows.MessageBox.Show("Необходимо выбрать версию, по которую обновляем (включительно)");
                return;
            }

            // выбранные стартовые значения
            string project = "";
            ComboBoxItem cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
            if ((cbItem == null) || (cbItem.Tag == null)) project = "";
            else project = cbItem.Tag.ToString();

            if (project == "ВСЕ")
            {
                project = "";
            }

            if (isJenkins == false)
            {
                tbList.Text = "";
            }

            var prefix = cbPrefix.SelectedItem.ToString().ToLower();
            var stand = cbStand.SelectedItem.ToString().ToUpper();
            var FromVersion = Release.VerAsNum(Release.GetNumVersion(prefix, cbFromVersion.SelectedItem.ToString()));
            var ToVersion = Release.VerAsNum(Release.GetNumVersion(prefix, cbToVersion.SelectedItem.ToString()));
            ListDeploymentPlan.Clear();

            // подготовка списка DP
            foreach (var item in ListBranch
                .Where(x =>
                    Release.VerAsNum(Release.GetNumVersion(prefix, x)) >= FromVersion &&
                    Release.VerAsNum(Release.GetNumVersion(prefix, x)) <= ToVersion
                )
                .OrderBy(x => Release.VerAsNum(Release.GetNumVersion(prefix, x)))
            )
            {
                // url на Deployment Plan в Confluence
                string url = GITProjects.GetURLDeploymentPlan(item, prefix);

                var DP = new DeploymentPlan();

                DP.NumVersion = item;
                DP.URL = url;
                DP.isAddMS = (rbAll.IsChecked == true || rbMS.IsChecked == true);
                DP.isAddPG = (rbAll.IsChecked == true || rbPG.IsChecked == true);

                ListDeploymentPlan.Add(DP);
            }

            if (ListDeploymentPlan.Count > 0 && JiraHTML.OpenLoginJira(logFileRelease))
            {
                ConfluenceHTML html = new ConfluenceHTML();

                // Отключаем элементы интерфейса
                var listControls = Utilities.Controls.DisabaleOnStart(mainGrid, null);

                // парсинг
                html.LoadConfluencePages(stand, ListDeploymentPlan, this,
                    null,
                    null,
                    null,
                    null,
                    finish =>
                    {
                        StringBuilder result = new StringBuilder(100000);
                        var jobs = new ObservableCollection<JenkinsJob>();
                        int jobOrder = 0;

                        if (
                            stand == "SP" ||
                            stand == "HF" ||
                            stand == "EHF_ACT" ||
                            stand == "EHF_UNACT" ||
                            stand == "LTS"
                        )
                        {
                            string _txt = MainWindow.UpdateLiquibaseRTMIS(stand, cbToVersion.Text, prefix, out List<string> list_cmd, out string max_version);

                            result.Append(Environment.NewLine + _txt + Environment.NewLine);

                            jobOrder++;
                            jobs.Add(new JenkinsJob()
                            {
                                Order = jobOrder,
                                FileName = _txt,
                                ExecutionMode = false,
                                Version = max_version,
                                Stand = stand
                            });
                        }

                        // перебираем Deployment Plan
                        foreach (var page in ListDeploymentPlan)
                        {
                            // перенумеруем, чтобы исключить дубли и чтобы MS SQL был первым, а PG SQL последним
                            int _ord = 0;

                            foreach (var item in page.ListDBAction
                                .Where(x =>
                                    x.dbregion == "MS SQL"
                                )
                                .OrderBy(x => x.order)
                            )
                            {
                                _ord++;
                                item.order = _ord;
                            }

                            foreach (var item in page.ListDBAction
                                .Where(x =>
                                    x.dbregion == "PG SQL"
                                )
                                .OrderBy(x => x.order)
                            )
                            {
                                _ord++;
                                item.order = _ord;
                            }

                            // список совпадений emd
                            var emd_pos = new Dictionary<int, int>();

                            // перебираем действия для emd из раздела PG SQL
                            foreach (var item in page.ListDBAction
                                .Where(x =>
                                    x.dbregion == "PG SQL" &&
                                    x.database == "emd" &&
                                    !string.IsNullOrWhiteSpace(x.script) &&
                                    (string.IsNullOrWhiteSpace(project) || project == x.GITProjectFromText)
                                )
                                .OrderBy(x => x.order)
                            )
                            {
                                // ищем аналогичный пункт в разделе MS SQL
                                var found = page.ListDBAction
                                    .Where(x =>
                                        x.dbregion == "MS SQL" &&
                                        x.database == item.database &&
                                        x.type == item.type &&
                                        !string.IsNullOrWhiteSpace(x.script) &&
                                        x.script == item.script &&
                                        (string.IsNullOrWhiteSpace(project) || project == x.GITProjectFromText)
                                    ).FirstOrDefault();

                                if (found != null && !emd_pos.ContainsKey(found.order))
                                {
                                    // заполняем список совпадений (первый MS, второй PG)
                                    emd_pos.Add(found.order, item.order);
                                }
                            }

                            // перебираем список совпадений и совмещаем перечни MS SQL и PG SQL
                            _ord = 1000;
                            foreach (var emd_pair in emd_pos)
                            {
                                // сначала перенумеруем пункты MS SQL
                                foreach (var item in page.ListDBAction
                                    .Where(x =>
                                        x.dbregion == "MS SQL" &&
                                        x.order <= emd_pair.Key &&
                                        x.order < 1000 &&
                                        !string.IsNullOrWhiteSpace(x.script) &&
                                        (string.IsNullOrWhiteSpace(project) || project == x.GITProjectFromText)
                                    )
                                    .OrderBy(x => x.order)
                                )
                                {
                                    if (item.order == emd_pair.Key)
                                    {
                                        // чтобы не было дублей
                                        item.script = "";
                                    }
                                    else
                                    {
                                        _ord++;
                                        item.order = _ord;
                                    }
                                }

                                // затем перенумеруем пункты PG SQL
                                foreach (var item in page.ListDBAction
                                    .Where(x =>
                                        x.dbregion == "PG SQL" &&
                                        x.order <= emd_pair.Value &&
                                        x.order < 1000 &&
                                        !string.IsNullOrWhiteSpace(x.script) &&
                                        (string.IsNullOrWhiteSpace(project) || project == x.GITProjectFromText)
                                    )
                                    .OrderBy(x => x.order)
                                )
                                {
                                    _ord++;
                                    item.order = _ord;
                                }
                            }

                            // перенумеруем оставшиеся пункты
                            foreach (var item in page.ListDBAction
                                .Where(x =>
                                    x.order < 1000 &&
                                    !string.IsNullOrWhiteSpace(x.script) &&
                                    (string.IsNullOrWhiteSpace(project) || project == x.GITProjectFromText)
                                )
                                .OrderBy(x => x.order)
                            )
                            {
                                _ord++;
                                item.order = _ord;
                            }

                            // формируем результат
                            result.Append(Environment.NewLine + page.NumVersion + ":" + Environment.NewLine);

                            foreach (var item in page.ListDBAction
                                .Where(x =>
                                    !string.IsNullOrWhiteSpace(x.script) &&
                                    (string.IsNullOrWhiteSpace(project) || project == x.GITProjectFromText)
                                )
                                .OrderBy(l => l.order)
                            )
                            {
                                // добавляем команды для liquibot
                                string _command = item.ScriptToLiquibot(stand, page.NumVersion);

                                string _reg = "";

                                if (
                                    !string.IsNullOrWhiteSpace(_command) &&
                                    item.regions.Count > 0 &&
                                    item.regions[0] != "all"
                                )
                                {
                                    _reg = item.regions_str + ":" + Environment.NewLine;
                                }

                                string _title = "";

                                if (
                                    !string.IsNullOrWhiteSpace(_command) &&
                                    _command.ToLower().StartsWith("/update") //-V3125
                                )
                                {
                                    result.Append(Environment.NewLine + Environment.NewLine + _reg + _command);
                                }
                                else
                                {
                                    _title = item.dbregion + ": ";
                                    if (prefix != "prmd")
                                    {
                                        _title = "";
                                    }
                                    result.Append(Environment.NewLine + Environment.NewLine + _title + _reg + _command.TrimInnerNewLine(1));
                                }

                                // добавляем задания для Jenkins
                                var _newjobs = item.ScriptToJenkins(stand, page.NumVersion, _title);

                                foreach (var job in _newjobs)
                                {
                                    jobOrder++;
                                    job.Order = jobOrder;
                                    jobs.Add(job);
                                }
                            }

                            result.Append(Environment.NewLine);
                        }

                        if (isJenkins == false)
                        {
                            // заполним на форме
                            tbList.Text = result.ToString()
                                .TrimInnerNewLine()
                                .TrimAllSpace();
                        }
                        else
                        {
                            // откроем форму выполнения заданий Jenkins
                            WinJenkinsExec win_jobs = new WinJenkinsExec(jobs, MainWindow.Task.LogFileJenkins);
                            win_jobs.ShowDialog();
                            win_jobs.Close();
                        }

                        // Включаем элементы интерфейса
                        Utilities.Controls.EnableOnFinish(mainGrid, listControls);
                    },
                    logFileRelease
                ).GetAwaiter();
            }
        }

        /// <summary>
        /// генерация списка команд для liquibot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGenerate_Click(object sender, RoutedEventArgs e)
        {
            Generate(false);
        }

        /// <summary>
        /// Выполнение скриптов через Jenkins
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btJenkins_Click(object sender, RoutedEventArgs e)
        {
            Generate(true);
        }

        private void btClipboard_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Clipboard.SetText(tbList.Text);
        }

        /// <summary>
        /// Сформировать список версий
        /// </summary>
        public void FillBranches()
        {
            // Очистить Versions
            ListBranch.Clear();
            var oldFromIndex = cbFromVersion.SelectedIndex;
            var oldToIndex = cbToVersion.SelectedIndex;
            string oldFrom = "";
            string oldTo = "";
            if (oldFromIndex != -1) oldFrom = cbFromVersion.SelectedItem.ToString().Trim();
            if (oldToIndex != -1) oldTo = cbToVersion.SelectedItem.ToString().Trim();
            cbFromVersion.Items.Clear();
            cbToVersion.Items.Clear();

            var prefix = cbPrefix.SelectedItem.ToString().ToLower();

            foreach (var item in MainWindow.APPinfo.GITProjects
                .Where(x =>
                    !string.IsNullOrWhiteSpace(x.DEVProject) &&
                    // префикс версии
                    (x.PrefixFileRelease.ToLower() == prefix)
                )
            )
            {
                double minversion = Release.VerAsNum(Release.GetNumVersion(prefix, item.DEVStartVer));

                // получим все ветки версий этого проекта
                var lst = GIT.GitListBranches(item.DEVProject, "git_listversion.cmd", logFileRelease, true, out double MaxVersion);

                // заполним список веток
                foreach (var s in lst)
                {
                    if (
                        !string.IsNullOrWhiteSpace(s) &&
                        !ListBranch.Contains(s) &&
                        Release.VerAsNum(Release.GetNumVersion(prefix, s)) >= minversion
                    )
                    {
                        ListBranch.Add(s);
                    }
                }
            }

            // заполнить cbFromVersion и cbToVersion
            foreach (var item in ListBranch
                .OrderBy(x => Release.VerAsNum(Release.GetNumVersion(prefix, x)))
            )
            {
                cbFromVersion.Items.Add(item);
                cbToVersion.Items.Add(item);
            }

            if (oldFromIndex != -1) cbFromVersion.SelectedItem = oldFrom;
            if (oldToIndex != -1) cbToVersion.SelectedItem = oldTo;
        }

        /// <summary>
        /// После выбора префикса версии (сервиса)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbPrefix_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (
                cbPrefix != null &&
                cbPrefix.SelectedIndex >= 0
            )
            {
                FillBranches();
                SetProjectFilter();
            }
        }

        /// <summary>
        /// После выбора релизного стенда
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbStand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }

        /// <summary>
        /// После выбора версии от
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbFromVersion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var FromIndex = cbFromVersion.SelectedIndex;
            var ToIndex = cbToVersion.SelectedIndex;

            if (ToIndex < FromIndex) cbToVersion.SelectedIndex = FromIndex;
        }

        /// <summary>
        /// Получить минимальную версию из databasechangelog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDatabasechangelog_Click(object sender, RoutedEventArgs e)
        {
            if (
                cbPrefix.SelectedIndex >= 0 &&
                cbStand.SelectedIndex >= 0
            )
            {
                var prefix = cbPrefix.SelectedItem.ToString().ToLower();
                var stand = cbStand.SelectedItem.ToString().ToUpper();

                string projectMS = "";
                string projectPG = "";

                if (prefix == "prmd") 
                {
                    projectMS = "dev_promed_ms";
                    projectPG = "dev_promed_pg";
                }

                if (prefix == "rpms")
                {
                    projectMS = "dev_userportal_ms";
                    projectPG = "dev_userportal_pg";
                }

                if (prefix == "smp")
                {
                    projectPG = "dev_smp2_pg";
                }

                if (prefix == "bi")
                {
                    projectPG = "dev_bi";
                }

                // ищем последнюю установленную версию
                double minversion = 0;

                // сначала PG
                if (!string.IsNullOrWhiteSpace(projectPG))
                {
                    var connect = MainWindow.GetConnectByGITProject(projectPG, "", false, true, stand);

                    if (
                        connect != null &&
                        !connect.isConnected
                    )
                    {
                        connect.OpenConnect(false);
                    }

                    if (
                        connect != null &&
                        connect.isConnected
                    )
                    {
                        string ver = connect.GetLastDatabasechangelog();
                        if (Release.VerAsNum(ver) < minversion || minversion == 0)
                        {
                            minversion = Release.VerAsNum(ver);
                        }
                    }
                }

                // затем MS
                if (!string.IsNullOrWhiteSpace(projectMS))
                {
                    var connect = MainWindow.GetConnectByGITProject(projectMS, "", false, true, stand);

                    if (
                        connect != null &&
                        !connect.isConnected
                    )
                    {
                        connect.OpenConnect(false);
                    }

                    if (
                        connect != null &&
                        connect.isConnected
                    )
                    {
                        string ver = connect.GetLastDatabasechangelog();
                        if (Release.VerAsNum(ver) < minversion || minversion == 0)
                        {
                            minversion = Release.VerAsNum(ver);
                        }
                    }
                }

                cbFromVersion.SelectedIndex = -1;

                // определяем следующую версию
                var nextver = ListBranch
                    .Where(x => Release.VerAsNum(Release.GetNumVersion(prefix, x)) > minversion)
                    .OrderBy(x => Release.VerAsNum(Release.GetNumVersion(prefix, x)))
                    .FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(nextver) && minversion > 0)
                {
                    cbFromVersion.Text = nextver;
                }

                if (cbFromVersion.SelectedIndex == -1 && minversion > 0) //-V3063
                {
                    nextver = ListBranch
                    .OrderByDescending(x => Release.VerAsNum(Release.GetNumVersion(prefix, x)))
                    .FirstOrDefault();

                    cbFromVersion.Text = nextver;
                }
            }
        }

        /// <summary>
        /// Настроить список проектов
        /// </summary>
        void SetProjectFilter()
        {
            string old = "";

            if (
                cbGITProject != null &&
                cbGITProject.SelectedItem != null
            )
            {
                old = cbGITProject.SelectedItem.ToString();
            }

            ConnType? filter = null;

            if (
                rbMS != null &&
                rbMS.IsChecked == true
            )
            {
                filter = ConnType.MSSQL;
            }

            if (
                rbPG != null &&
                rbPG.IsChecked == true
            )
            {
                filter = ConnType.PGSQL;
            }

            string prefix = "";

            if (
                cbPrefix != null &&
                cbPrefix.SelectedIndex >= 0
            )
            {
                prefix = cbPrefix.SelectedItem.ToString().ToLower();
            }

            if (cbGITProject != null)
            {
                Utilities.Controls.RefreshGITProjectItems(cbGITProject, old, filter, false, true, prefix);
            }
        }


        private void rbAll_Checked(object sender, RoutedEventArgs e)
        {
            SetProjectFilter();
        }

        private void rbMS_Checked(object sender, RoutedEventArgs e)
        {
            SetProjectFilter();
        }

        private void rbPG_Checked(object sender, RoutedEventArgs e)
        {
            SetProjectFilter();
        }

        /// <summary>
        /// Нажата кнопка "Установить максимальную версию"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSetMaxVersion_Click(object sender, RoutedEventArgs e)
        {
            if (
                cbPrefix.SelectedIndex >= 0 &&
                cbStand.SelectedIndex >= 0 &&
                cbToVersion.SelectedIndex >= 0
            )
            {
                var prefix = cbPrefix.SelectedItem.ToString().ToLower();
                var stand = cbStand.SelectedItem.ToString().ToUpper();

                string projectPG = "";

                if (prefix == "prmd")
                {
                    projectPG = "dev_promed_pg";
                }

                if (prefix == "rpms")
                {
                    projectPG = "dev_userportal_pg";
                }

                if (prefix == "smp")
                {
                    projectPG = "dev_smp2_pg";
                }

                if (prefix == "bi")
                {
                    projectPG = "dev_bi";
                }

                string max_version = cbToVersion.SelectedItem.ToString().Trim();
                string Error = "";
                var connect = MainWindow.GetConnectByGITProject(projectPG, "", false, true, stand);

                if (
                    connect != null &&
                    !connect.isConnected
                )
                {
                    connect.OpenConnect(false);
                }

                if (
                    connect != null &&
                    connect.isConnected
                )
                {
                    try
                    {
                        string cmd = MainWindow.UpdateLiquibaseRTMIS(stand, max_version, prefix, out List<string> _list, out max_version);

                        if (
                            !string.IsNullOrWhiteSpace(max_version) &&
                            !string.IsNullOrWhiteSpace(projectPG) &&
                            System.Windows.Forms.MessageBox.Show($"Установить максимальную версию стенда {stand} = {max_version}?",
                        "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes
                        )
                        {
                            connect.ExecuteNonQuery(cmd, out Error);
                        }

                        if (!string.IsNullOrWhiteSpace(Error))
                        {
                            App.AddLog(Error, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                        }
                    }
                    catch (Exception ex)
                    {
                        App.AddLog(null, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Главное окно программы
    /// </summary>
    public partial class MainWindow
    {
        private void btUpdRelBD_Click(object sender, RoutedEventArgs e)
        {
            WinUpdRelBD WinUpdRelBD = new WinUpdRelBD();

            WinUpdRelBD.cbStand.Items.Add("SP");
            WinUpdRelBD.cbStand.Items.Add("HF");
            WinUpdRelBD.cbStand.Items.Add("EHF_ACT");
            WinUpdRelBD.cbStand.Items.Add("EHF_UNACT");
            WinUpdRelBD.cbStand.Items.Add("LTS");
            WinUpdRelBD.cbStand.Items.Add("QA-Rel");
            WinUpdRelBD.cbStand.Items.Add("QA");
            WinUpdRelBD.cbStand.SelectedIndex = -1;

            WinUpdRelBD.cbPrefix.Items.Add("prmd");
            WinUpdRelBD.cbPrefix.Items.Add("rpms");
            //WinUpdRelBD.cbPrefix.Items.Add("smp");
            //WinUpdRelBD.cbPrefix.Items.Add("bi");
            WinUpdRelBD.cbPrefix.SelectedIndex = -1;

            WinUpdRelBD.Show();
        }
    }
}
