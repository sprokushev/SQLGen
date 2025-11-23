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
    public partial class FormSaveToGIT : Form
    {
        /// <summary>
        /// лог-файл
        /// </summary>
        public string LogFile;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Конструктор FormSaveToGIT
        /// </summary>
        /// <param name="logFile">лог-файл</param>
        public FormSaveToGIT(string logFile)
        {
            InitializeComponent();

            LogFile = logFile;
            if (string.IsNullOrWhiteSpace(LogFile))
            {
                LogFile = MainWindow.Task.LogFile;
            }

            // пользовательские настройки GUI
            Default.InitGUI("FormSaveToGIT", this, LogFile);
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
        private void FormSaveToGIT_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormSaveToGIT", this);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Сохранить</summary>
        private void btSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Нажата кнопка Выбрать для ветки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btChoose_Click(object sender, EventArgs e)
        {
            if (GIT.SelectGITBranch(tbProject.Text, tbBranch.Text, out string branch, LogFile, true, false))
            {
                tbBranch.Text = branch;
            }
        }
    }
}
