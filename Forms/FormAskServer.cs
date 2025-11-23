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
    public partial class FormAskServer : Form
    {

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Конструктор FormAskNumFile</summary>
        public FormAskServer()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormAskServer", this, "");
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата клавиша</summary>
        private void tbNewName_KeyDown(object sender, KeyEventArgs e)
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
            if (string.IsNullOrWhiteSpace(tbNewName.Text))
            {
                MessageBox.Show("Необходимо заполнить имя или адрес нового сервера!");
                tbNewName.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAskServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAskServer", this);
        }
    }
}
