// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Office.Interop.Excel;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen.Forms
{
    /// <summary>
    /// Форма выбора версии для влития в master
    /// </summary>
    public partial class FormAskVersion : Form
    {
        /// <summary>
        /// Конструктор FormAskVersion
        /// </summary>
        public FormAskVersion()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormAskVersion", this, MainWindow.Task.LogFile);
        }

        private void FormAskVersion_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAskVersion", this);
        }

        private void dgList_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (this.dgList.Columns[e.ColumnIndex].Name == "listNoMergedVersion")
            {
                if (e.Value != null)
                {
                    // Check for the string "pink" in the cell.
                    string value = (string)e.Value;
                    if (
                        !string.IsNullOrWhiteSpace(value) &&
                        value.StartsWith("Проблемные ветки:")
                        )
                    {
                        e.CellStyle.BackColor = Color.Pink;
                    }
                }
            }

            if (this.dgList.Columns[e.ColumnIndex].Name == "mergeStatus")
            {
                if (e.Value != null)
                {
                    // Check for the string "pink" in the cell.
                    string value = (string)e.Value;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        e.CellStyle.BackColor = Color.Pink;
                    }
                }
            }
        }

        /// <summary>
        /// список проектов
        /// </summary>
        public List<string> ListProjects;
        /// <summary>
        /// список версий
        /// </summary>
        public List<VersionInfo> allVersions;
        /// <summary>
        /// информация о проектах
        /// </summary>
        public BindingList<ProjectInfo> listProjectInfo;
        /// <summary>
        /// минимальная ветка
        /// </summary>
        public string minBranch;
        /// <summary>
        /// номер минимальной версии
        /// </summary>
        public double minNumVersion;


        /// <summary>
        /// первоначально заполнить список версий
        /// </summary>
        public void FillProjectInfo()
        {
            File.WriteAllText(MainWindow.Task.LogFileMerge, "");

            // перебрать проекты и собрать список версий, еще не влитых в master
            minBranch = "";
            minNumVersion = -1;
            listProjectInfo = new BindingList<ProjectInfo>();
            allVersions = new List<VersionInfo>();

            foreach (var project in ListProjects)
            {
                if (listProjectInfo.FirstOrDefault(x => x.project == project) == null)
                {
                    var newproject = new ProjectInfo();
                    newproject.project = project;
                    newproject.mergeOk = true;
                    newproject.mergeStatus = "";
                    newproject.projectStatus = "";

                    // получаем список не влитых версий
                    List<string> list = Utilities.GIT.GitListNoMergedVersions(project, false, out string err, MainWindow.Task.LogFileMerge, false);

                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        App.AddLog($"В проекте {project} есть ветки кумулятивных версий, не влитые в ветки последующих версий:\n{err}\n\nДля проверки выполните git-nomerged-ver.sh в корне проекта и исправьте\n\nВлитие в master в этом проекте возможно только после устранения данной проблемы!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileMerge);
                        
                        newproject.projectStatus = err;
                    }

                    err = "";
                    newproject.branch = Utilities.GIT.GitCurrentBranch(project, out err, MainWindow.Task.LogFileMerge);
                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        newproject.branch = err;
                    }

                    if (
                        string.IsNullOrWhiteSpace(newproject.projectStatus) &&
                        string.IsNullOrWhiteSpace(err)
                        )
                    {
                        foreach (var item in list)
                        {
                            var v = new VersionInfo() { branchName = item, numVersion = Release.VerAsNum(item) };

                            if (allVersions.Find(x => x.branchName == v.branchName) == null)
                            {
                                allVersions.Add(v);
                            }

                            if (minNumVersion > v.numVersion || minNumVersion < 0)
                            {
                                minNumVersion = v.numVersion;
                                minBranch = v.branchName;
                            }

                            newproject.listVersionsInProject.Add(v);
                        }
                    }

                    listProjectInfo.Add(newproject);
                }
            }

            cbList.Items.Clear();
            cbList.Items.AddRange(allVersions.OrderBy(x => x.numVersion).Select(x => x.branchName).ToArray());
            cbList.SelectedItem = minBranch;
            dgList.DataSource = listProjectInfo;
        }

        /// <summary>
        /// Нажата кнопка "Влить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMerge_Click(object sender, EventArgs e)
        {
            string maxVersion = "";

            if (cbList.SelectedIndex != -1)
            {
                maxVersion = cbList.SelectedItem.ToString();
            }

            if (string.IsNullOrWhiteSpace(maxVersion))
            {
                App.AddLog("Версия не выбрана!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFileMerge);
            }
            else
            {
                Merge(maxVersion);
            }
        }

        /// <summary>
        /// Влить версию в master и в dev
        /// </summary>
        /// <param name="maxVersion">версия, по которую включительно надо влить в master</param>
        private void Merge(string maxVersion)
        {
            // перебираем проекты
            foreach (var item in listProjectInfo.Where(x => x.mergeOk && string.IsNullOrWhiteSpace(x.projectStatus)))
            {
                // отдельный лог-файл для каждого проекта
                if (string.IsNullOrWhiteSpace(item.logFile))
                {
                    item.logFile = Path.GetTempFileName();
                    App.tempFiles.Add(item.logFile);
                }

                // пробуем влить
                foreach (var v in item.listVersionsInProject.Where(x => x.numVersion <= Release.VerAsNum(maxVersion)).OrderByDescending(x => x.numVersion))
                {
                    // есть такая ветка в этом проекте
                    if (System.Windows.Forms.MessageBox.Show($"Влить ветку {v.branchName} в проекте {item.project} в ветку master ?",
                        "ВНИМАНИЕ",
                        System.Windows.Forms.MessageBoxButtons.YesNo
                        ) == System.Windows.Forms.DialogResult.Yes
                    )
                    {
                        item.mergeStatus = "";
                        item.mergeOk = false;
                        File.WriteAllText(item.logFile, "");

                        // ----------------------------------------------------------------------------
                        // определяем текущую ветку
                        item.branch = GIT.GitCurrentBranch(item.project, out string err, item.logFile);

                        if (!string.IsNullOrWhiteSpace(err))
                        {
                            item.branch = err;
                            item.mergeStatus = "Ошибка при определении текущей ветки";

                            break;
                        }

                        // ----------------------------------------------------------------------------
                        // проверим, нужен ли commit в текущую ветку
                        if (!GIT.CheckCommit(item.project, item.logFile, "Merge прерван"))
                        {
                            item.mergeStatus = $"Требуется commit в {item.branch}";

                            break;
                        }

                        // ----------------------------------------------------------------------------
                        // переключение на ветку версии
                        if (!GIT.GitSwitch(item.project, v.branchName, item.logFile, out string cur_branch, out err))
                        {
                            // определяем текущую ветку
                            if (!string.IsNullOrWhiteSpace(err))
                            {
                                item.branch = err;
                            }
                            else
                            {
                                item.branch = cur_branch;
                            }

                            App.AddLog($"Не получилось переключиться на ветку {v.branchName} в проекте {item.project} или при переключении на ветку {v.branchName} возникла ошибка: {err}", null, App.ShowMessageMode.SHOW, true, item.logFile);

                            item.mergeStatus = $"Ошибка при переключении на {v.branchName}";

                            break;
                        }

                        // ----------------------------------------------------------------------------
                        // переключение на ветку master
                        if (!GIT.GitSwitch(item.project, "master", item.logFile, out cur_branch, out err))
                        {
                            if (!string.IsNullOrWhiteSpace(err))
                            {
                                item.branch = err;
                            }
                            else
                            {
                                item.branch = cur_branch;
                            }

                            App.AddLog($"Не получилось переключиться на ветку master в проекте {item.project} или при переключении на ветку master возникла ошибка: {err}", null, App.ShowMessageMode.SHOW, true, item.logFile);

                            item.mergeStatus = $"Ошибка при переключении на master";

                            break;
                        }

                        // ----------------------------------------------------------------------------
                        // вливаем ветку версии в master
                        if (!GIT.GitMerge(item.project, v.branchName, "master", true, false, item.logFile, false))
                        {
                            App.AddLog($"Merge ветки {v.branchName} в проекте {item.project} в ветку master НЕ был выполнен\n\nВ проекте {item.project} текущая ветка - master", null, App.ShowMessageMode.SHOW, true, item.logFile);

                            item.mergeStatus = $"Ошибка при влитии {v.branchName} в master";

                            break;
                        }
                        else
                        {
                            // ----------------------------------------------------------------------------
                            // проверим, нужен ли commit в текущую ветку
                            if (!GIT.CheckCommit(item.project, item.logFile, $"Merge ветки {v.branchName} в ветку master прерван"))
                            {
                                item.mergeStatus = $"Требуется commit в master или надо все откатить";

                                break;
                            }

                            // ----------------------------------------------------------------------------
                            // переключение на ветку dev
                            if (!GIT.GitSwitch(item.project, "dev", item.logFile, out cur_branch, out err))
                            {
                                if (!string.IsNullOrWhiteSpace(err))
                                {
                                    item.branch = err;
                                }
                                else
                                {
                                    item.branch = cur_branch;
                                }

                                App.AddLog($"Не получилось переключиться на ветку dev в проекте {item.project} или при переключении на ветку dev возникла ошибка: {err}", null, App.ShowMessageMode.SHOW, true, item.logFile);

                                item.mergeStatus = $"Ошибка при переключении на dev";

                                break;
                            }

                            // ----------------------------------------------------------------------------
                            // вливаем ветку master в dev
                            if (!GIT.GitMerge(item.project, "master", "dev", true, false, item.logFile, false))
                            {
                                App.AddLog($"Merge ветки master в проекте {item.project} в ветку dev НЕ был выполнен\n\nВ проекте {item.project} текущая ветка - dev", null, App.ShowMessageMode.SHOW, true, item.logFile);

                                item.mergeStatus = $"Ошибка при влитии master в dev";

                                break;
                            }
                            else
                            {
                                // ----------------------------------------------------------------------------
                                // проверим, нужен ли commit в текущую ветку
                                if (!GIT.CheckCommit(item.project, item.logFile, $"Merge ветки master в ветку dev прерван"))
                                {
                                    item.mergeStatus = $"Требуется commit в dev или надо все откатить";

                                    break;
                                }

                                // ----------------------------------------------------------------------------
                                // делаем push dev
                                GIT.GitPull(new string[] { item.project }, "dev", false, false, true, item.logFile, false);
                                GIT.GitPush(new string[] { item.project }, "dev", true, item.logFile);

                                // переключение на ветку master
                                if (!GIT.GitSwitch(item.project, "master", item.logFile, out cur_branch, out err))
                                {
                                    if (!string.IsNullOrWhiteSpace(err))
                                    {
                                        item.branch = err;
                                    }
                                    else
                                    {
                                        item.branch = cur_branch;
                                    }

                                    App.AddLog($"Не получилось переключиться на ветку master в проекте {item.project} или при переключении на ветку master возникла ошибка: {err}\n\nMerge ветки {v.branchName} в ветку master прерван", null, App.ShowMessageMode.SHOW, true, item.logFile);

                                    item.mergeStatus = $"Ошибка при переключении на master";

                                    break;
                                }

                                // ----------------------------------------------------------------------------
                                // делаем push master
                                GIT.GitPull(new string[] { item.project }, "master", false, false, true, item.logFile, false);
                                GIT.GitPush(new string[] { item.project }, "master", true, item.logFile);
                            }
                        }

                        //App.AddLog($"Ветка {v.branchName} в проекте {item.project} влита в ветку master и dev", null, App.ShowMessageMode.SHOW, true, item.logFile);

                        item.mergeStatus = "";
                        item.mergeOk = true;

                        // Копируем в общий лог
                        string text = File.ReadAllText(item.logFile);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            File.AppendAllText(MainWindow.Task.LogFileMerge, Environment.NewLine + text);
                        }

                        break;
                    }
                    else
                    {
                        App.AddLog($"Пользователь отказался вливать ветку {v.branchName} в проекте {item.project} в ветку master", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFileMerge);

                        item.mergeStatus = "Пользователь отказался";
                        item.mergeOk = true;

                        break;
                    }
                }

                // убираем влитые успешно версии
                if (string.IsNullOrWhiteSpace(item.mergeStatus))
                {
                    VersionInfo find = null;

                    do
                    {
                        find = item.listVersionsInProject.Find(x => x.numVersion <= Release.VerAsNum(maxVersion));
                        if (find != null)
                        {
                            item.listVersionsInProject.Remove(find);
                        }

                    } while (find != null);
                }
            }

            dgList.Refresh();
        }

        /// <summary>
        /// Закрыть форму
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            FillProjectInfo();
        }

        /// <summary>
        /// Нажата кнопка для просмотра лог-файла
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn &&
                e.RowIndex >= 0)
            {
                if (e.ColumnIndex == this.logMerge.Index)
                {
                    var row = this.dgList.Rows[e.RowIndex].DataBoundItem as ProjectInfo;
                    if (
                        (row != null) &&
                        (!string.IsNullOrWhiteSpace(row.logFile))
                    )
                    {
                        WinInfo WinInfo = new WinInfo(MainWindow.Task.LogFileMerge);
                        WinInfo.Title = "Лог ошибок";
                        WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                        WinInfo.tbInfo.Text = File.ReadAllText(row.logFile);
                        WinInfo.Show();
                    }
                }
            }
        }

        /// <summary>
        /// Нажата кнопка Общий лог
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAllLog_Click(object sender, EventArgs e)
        {
            WinInfo WinInfo = new WinInfo(MainWindow.Task.LogFileMerge);
            WinInfo.Title = "Лог merge в файле " + MainWindow.Task.LogFileMerge;
            WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
            WinInfo.tbInfo.Text = File.ReadAllText(MainWindow.Task.LogFileMerge);
            WinInfo.Show();
        }
    }


    /// <summary>
    /// Информация о версии
    /// </summary>
    public class VersionInfo
    {
        /// <summary>
        /// номер версии
        /// </summary>
        public double numVersion = -1;
        /// <summary>
        /// ветка версии
        /// </summary>
        public string branchName = "";
    }

    /// <summary>
    /// Информация о проекте
    /// </summary>
    public class ProjectInfo
    {
        /// <summary>
        /// проект
        /// </summary>
        public string project { get; set; } = "";
        /// <summary>
        /// статус проект
        /// </summary>
        public string projectStatus { get; set; } = "";
        /// <summary>
        /// текущая ветка в проекте
        /// </summary>
        public string branch { get; set; } = "";
        /// <summary>
        /// Список версий в проекте, не влитых в master (одной строкой, через запятую)
        /// </summary>
        public string listNoMergedVersion
        { 
            get
            {
                if (!string.IsNullOrEmpty(projectStatus))
                {
                    return $"Проблемные ветки:\n{projectStatus}";
                }
                else
                {
                    return string.Join(", ",
                    listVersionsInProject.OrderBy(x => x.numVersion).Select(x => x.branchName)
                    );
                }
            } 
        }
        /// <summary>
        /// Список версий в проекте, не влитых в master
        /// </summary>
        public List<VersionInfo> listVersionsInProject { get; set; } = new List<VersionInfo>();
        /// <summary>
        /// текущий статус
        /// </summary>
        public string mergeStatus { get; set; } = "";
        /// <summary>
        /// текущий статус
        /// </summary>
        public bool mergeOk { get; set; } = true;
        /// <summary>
        /// лог-файл
        /// </summary>
        public string logFile { get; set; } = "";
    }
}
