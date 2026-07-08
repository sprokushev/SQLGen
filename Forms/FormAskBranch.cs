// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма выбора ветки
    /// </summary>
    public partial class FormAskBranch : Form
    {
        /// <summary>
        /// Проект по умолчанию
        /// </summary>
        public string ProjectDefault = "";

        /// <summary>
        /// Список веток
        /// </summary>
        public List<string> ListBranches = new List<string>();

        /// <summary>
        /// Выбранная ветка
        /// </summary>
        public string Branch = "";

        /// <summary>
        /// лог-файл
        /// </summary>
        public string LogFile;

        /// <summary>
        /// Конструктор FormAskBranch
        /// </summary>
        /// <param name="projectDefault">проект по умолчанию</param>
        /// <param name="branchDefault">ветка по умолчанию</param>
        /// <param name="info">дополнительная информация</param>
        /// <param name="logFile">лог-файл</param>
        public FormAskBranch(string projectDefault, string branchDefault, string info, string logFile)
        {
            InitializeComponent();

            ProjectDefault = projectDefault;

            LogFile = logFile;
            if (string.IsNullOrWhiteSpace(LogFile))
            {
                LogFile = MainWindow.Task.LogFile;
            }

            string current_branch = null;

            if (!string.IsNullOrWhiteSpace(ProjectDefault))
            {
                ProjectDefault = ProjectDefault.Trim();

                // определяем текущую ветку
                current_branch = GIT.GitCurrentBranch(ProjectDefault, out string err, LogFile);
            }

            if (!string.IsNullOrWhiteSpace(current_branch))
            {
                btCurrentBranch.Text = current_branch + " (текущая ветка)";
            }
            else
            {
                btCurrentBranch.Text = "";
                btCurrentBranch.Visible = false;
            }

            Branch = branchDefault;

            if (string.IsNullOrWhiteSpace(Branch))
            {
                if (!string.IsNullOrWhiteSpace(MainWindow.Task.ReleaseVersion))
                {
                    Branch = MainWindow.Task.ReleaseVersion;
                }
                else
                {
                    Branch = MainWindow.Task.TaskNumber;
                }
            }
            Branch = Branch.Trim();

            if (string.IsNullOrWhiteSpace(Branch))
            {
                btTask.Visible = false;
                btTask.Text = "";
            }
            else
            {
                btTask.Text = Branch + " (по умолчанию)";
            }

            if (!string.IsNullOrWhiteSpace(ProjectDefault))
            {
                this.Text = $"Выбрать ветку в {ProjectDefault}";
            }

            if (!string.IsNullOrWhiteSpace(info))
            {
                this.lbInfo.Text = info;
            }
            else
            {
                this.lbInfo.Text = "";
            }

            // пользовательские настройки GUI
            Default.InitGUI("FormAskBranch", this, LogFile);
        }

        /// <summary>
        /// Выбрана ветка dev
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void btDev_Click(object sender, EventArgs e)
        {
            Branch = "dev";
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Выбрана ветка master
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void btMaster_Click(object sender, EventArgs e)
        {
            Branch = "master";
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Выбрана Ветка задачи
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void btTask_Click(object sender, EventArgs e)
        {
            string result = btTask.Text.Trim().Split(' ')[0];
            if (!string.IsNullOrWhiteSpace(result))
            {
                Branch = btTask.Text.Trim().Split(' ')[0];
                DialogResult = DialogResult.OK;
                Close();
            }

        }

        /// <summary>
        /// Выбрана Текущая ветка
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCurrentBranch_Click(object sender, EventArgs e)
        {
            string result = btCurrentBranch.Text.Trim().Split(' ')[0];
            if (!string.IsNullOrWhiteSpace(result))
            {
                Branch = btCurrentBranch.Text.Trim().Split(' ')[0];
                DialogResult = DialogResult.OK;
                Close();
            }
        }
        
        /// <summary>
                 /// Выбор другой ветки
                 /// </summary>
                 /// <param name="sender">sender</param>
                 /// <param name="e">event</param>
        private void btChoose_Click(object sender, EventArgs e)
        {
            FormFindInList dlg1 = new FormFindInList(MainWindow.Task.LogFile);

            dlg1.AddItems(ListBranches.OrderBy(x => x).ToList());

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
                    Branch = result;
                }

                dlg1.Dispose();

                DialogResult = DialogResult.OK;
            }
            else
            {
                dlg1.Dispose();

                DialogResult = DialogResult.Abort;
            }
            Close();
        }

        /// <summary>
        /// Нажата кнопка Прервать
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void btAbort_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Abort;
            Close();
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void FormAskBranch_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAskBranch", this);
        }
    }
}
