// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма для добавления новой БД или изменения существующей БД
    /// </summary>
    public partial class FormAddDB : Form
    {
        /// <summary>Конструктор FormAddDB</summary>
        public FormAddDB()
        {
            InitializeComponent();
            foreach (var item in MainWindow.APPinfo.GITProjects) cbGITProject.Items.Add(item.GITProject);
            foreach (var item in MainWindow.APPinfo.GITProjects) cbGITProject.Items.Add(item.DEVProject);

            // пользовательские настройки GUI
            Default.InitGUI("FormAddDB", this, "");
        }

        /// <summary>Нажата кнопка Отмена</summary>
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>Нажата кнопка Сохранить</summary>
        private void btOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbServerName.Text)) tbServerName.Text = "новый";
            if (string.IsNullOrWhiteSpace(tbDBName.Text)) tbDBName.Text = "новая";
            if ((cbGITProject.SelectedIndex == -1) && (cbGITProject.Items.Count > 0)) cbGITProject.SelectedIndex = 0;
            if ((cbDBRole.SelectedIndex == -1) && (cbDBRole.Items.Count > 0)) cbDBRole.SelectedItem = "PROD";

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// При закрытии формы FormAddDB
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAddDB_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAddDB", this);
        }

        /// <summary>
        /// После смены роли БД
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDBRole_SelectedIndexChanged(object sender, EventArgs e)
        {
            isMainTest.Enabled = (cbDBRole.SelectedIndex == 0);
        }
    }
}
