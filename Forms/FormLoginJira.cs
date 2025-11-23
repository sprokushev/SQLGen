// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма для коннекта в Jira
    /// </summary>
    public partial class FormLoginJira : Form
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Конструктор FormLoginJira</summary>
        /// <param name="_logfile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public FormLoginJira(string _logfile)
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormLoginJira", this, _logfile);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Подключиться</summary>
        private void btConnect_Click(object sender, EventArgs e)
        {
            if (tbPassword.Text.Trim() == "")
            {
                MessageBox.Show("Введите пароль !");
                tbPassword.Focus();
                return;
            }

            if (tbUsername.Text.Trim() == "")
            {
                MessageBox.Show("Заполните имя пользователя !");
                tbUsername.Focus();
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        /// <summary>
        /// Нажата кнопка Отмена
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
        private void FormLoginJira_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormLoginJira", this);
        }

        private void FormLoginJira_Shown(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbUsername.Text))
            {
                tbPassword.Focus();
            }

        }
    }
}
