// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма выбора класса событий
    /// </summary>
    public partial class FormEvnClass : Form
    {
        /// <summary>
        /// Конструктор FormEvnClass
        /// </summary>
        public FormEvnClass()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormEvnClass", this, MainWindow.Task.LogFile);
        }

        /// <summary>
        /// Нажата кнопка Выбрать
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Нажата кнопка Отмены
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormEvnClass_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormEvnClass", this);
        }
    }
}
