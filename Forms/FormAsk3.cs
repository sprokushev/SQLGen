// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SQLGen.Controls;

namespace SQLGen.Forms
{
    /// <summary>
    /// Форма выбора модуля
    /// </summary>
    public partial class FormAsk3 : Form
    {
        /// <summary>
        /// конструктор FormAsk3
        /// </summary>
        public FormAsk3()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormAsk3", this, MainWindow.Task.LogFile);
        }

        private void FormAsk3_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAsk3", this);
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            Close();
        }

        private void btYes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            Close();
        }
    }
}
