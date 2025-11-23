// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма запроса нового имени для таблицы
    /// </summary>
    public partial class FormNewTableName : Form
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Конструктор FormNewTableName</summary>
        public FormNewTableName()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormNewTableName", this, MainWindow.Task.LogFile);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Сменить</summary>
        private void btReplace_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Отмена</summary>
        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormNewTableName_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormNewTableName", this);
        }
    }
}
