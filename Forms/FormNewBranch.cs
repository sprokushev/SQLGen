// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма запроса нового имени для таблицы
    /// </summary>
    public partial class FormNewBranch : Form
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Конструктор FormNewBranch</summary>
        public FormNewBranch()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormNewBranch", this, MainWindow.Task.LogFile);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Отмена</summary>
        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormNewBranch_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormNewBranch", this);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Создать</summary>
        private void btCreate_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// список проектов
        /// </summary>
        public List<string> ListProjects;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Нажата кнопка Выбрать для ветки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btChoose_Click(object sender, EventArgs e)
        {
            if (
                (ListProjects != null) &&
                (ListProjects.Count > 0)
            )
            {
                FormAskBranch dlg2 = new FormAskBranch(null, tbNewBranchName.Text, "", MainWindow.Task.LogFile);

                foreach (var project in ListProjects)
                {
                    // Заполнить ListBranches
                    foreach (var item in GIT.GitListBranches(project, "git_listbranch.cmd", MainWindow.Task.LogFile, true, out double n))
                    {
                        string _branch = item.Replace("*", "").Trim();

                        if (
                            (!string.IsNullOrWhiteSpace(_branch)) &&
                            (!dlg2.ListBranches.Contains(_branch))
                            )
                        {
                            dlg2.ListBranches.Add(_branch);
                        }
                    }
                }

                var res = dlg2.ShowDialog();

                if (!string.IsNullOrWhiteSpace(dlg2.Branch))
                {
                    tbNewBranchName.Text = dlg2.Branch;
                }

                dlg2.Dispose();

                if (res == System.Windows.Forms.DialogResult.Abort)
                {
                    return;
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Нажата кнопка Выбрать для родительской ветки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btChooseParent_Click(object sender, EventArgs e)
        {
            if (
                (ListProjects != null) &&
                (ListProjects.Count > 0)
            )
            {
                FormAskBranch dlg2 = new FormAskBranch(null, tbParentBranchName.Text, "", MainWindow.Task.LogFile);

                foreach (var project in ListProjects)
                {
                    // Заполнить ListBranches
                    foreach (var item in GIT.GitListBranches(project, "git_listbranch.cmd", MainWindow.Task.LogFile, true, out double n))
                    {
                        string _branch = item.Replace("*", "").Trim();

                        if (
                            (!string.IsNullOrWhiteSpace(_branch)) &&
                            (!dlg2.ListBranches.Contains(_branch))
                            )
                        {
                            dlg2.ListBranches.Add(_branch);
                        }
                    }
                }

                var res = dlg2.ShowDialog();

                if (!string.IsNullOrWhiteSpace(dlg2.Branch))
                {
                    tbParentBranchName.Text = dlg2.Branch;
                }

                dlg2.Dispose();

                if (res == System.Windows.Forms.DialogResult.Abort)
                {
                    return;
                }
            }
        }
    }
}
