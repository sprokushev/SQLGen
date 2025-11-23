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
    public partial class FormAskModule : Form
    {
        /// <summary>
        /// конструктор FormAskModule
        /// </summary>
        public FormAskModule()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormAskModule", this, MainWindow.Task.LogFile);
        }

        private void FormAskModule_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAskModule", this);
        }

        private void btnSMP_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            Close();
        }

        private void btnRPMS_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            Close();
        }

        private void btnPRMD_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {

        }

        private void btnBI_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Retry;
            Close();
        }
    }
}
