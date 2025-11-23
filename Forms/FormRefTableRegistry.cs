// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма для добавления в nsi.RefTableRegistry
    /// </summary>
    public partial class FormRefTableRegistry : Form
    {
        /// <summary>
        /// Конструктор FormRefTableRegistry
        /// </summary>
        public FormRefTableRegistry()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormRefTableRegistry", this, MainWindow.Task.LogFile);
        }

        /// <summary>
        /// Нажата кнопка Отмена
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Нажата кнопка Сгенерировать скрипт
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormRefTableRegistry_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormRefTableRegistry", this);
        }
    }
}
