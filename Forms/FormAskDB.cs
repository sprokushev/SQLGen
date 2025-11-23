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
    public partial class FormAskDB : Form
    {

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Конструктор FormAskNumFile</summary>
        public FormAskDB()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormAskDB", this, "");
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата клавиша</summary>
        private void tbNewServer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btOk_Click(sender, e);
                e.Handled = true;
            }

        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Ok</summary>
        private void btOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbNewServer.Text))
            {
                MessageBox.Show("Необходимо заполнить имя или адрес нового сервера!");
                tbNewServer.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(tbNewDB.Text))
            {
                MessageBox.Show("Необходимо заполнить имя БД!");
                tbNewDB.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void FormAskDB_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAskDB", this);
        }
    }
}
