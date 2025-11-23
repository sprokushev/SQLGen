// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма запроса номера
    /// </summary>
    public partial class FormAskNumTask : Form
    {

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Конструктор FormAskNumTask</summary>
        public FormAskNumTask()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormAskNumTask", this, MainWindow.Task.LogFileRelease);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата клавиша</summary>
        private void tbNumTask_KeyDown(object sender, KeyEventArgs e)
        {
            /* if (e.KeyCode == Keys.Enter)
             {
                 btOk_Click(sender, e);
             }
            */
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Ok</summary>
        private void btOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void FormAskNumTask_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAskNumTask", this);
        }
    }
}
