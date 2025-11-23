// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма для коннекта в БД
    /// </summary>
    public partial class FormLogin : Form
    {

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Конструктор FormLogin</summary>
        /// <param name="_logfile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public FormLogin(string _logfile)
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormLogin", this, _logfile);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Наименование текущего подключения</summary>
        public string currentDBConnectionName;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Подключиться</summary>
        private void btConnect_Click(object sender, EventArgs e)
        {
            if ((cbAuthentication.SelectedIndex == 1) && string.IsNullOrWhiteSpace(tbPassword.Text))
            {
                MessageBox.Show("Введите пароль !");
                tbPassword.Focus();
                return;
            }

            if ((cbAuthentication.SelectedIndex == 1) && string.IsNullOrWhiteSpace(tbUsername.Text))
            {
                MessageBox.Show("Заполните имя пользователя !");
                tbUsername.Focus();
                return;
            }

            if ((cbTypeDB.SelectedIndex == 0 || cbTypeDB.SelectedIndex == 1) && string.IsNullOrWhiteSpace(tbServerName.Text))
            {
                MessageBox.Show("Заполните название или IP-адрес сервера !");
                tbServerName.Focus();
                return;
            }

            if ((cbTypeDB.SelectedIndex == 0 || cbTypeDB.SelectedIndex == 1) && string.IsNullOrWhiteSpace(tbDatabaseName.Text))
            {
                MessageBox.Show("Заполните название базы данных !");
                tbDatabaseName.Focus();
                return;
            }

            //App.AddLog("FormLogin - btConnect_Click", null, App.ShowMessageMode.NONE);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выбран Тип авторизации</summary>
        private void cbAuthentication_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbAuthentication.SelectedIndex == 0) // Windows Login
            {
                if (tbUsername != null)
                {
                    tbUsername.Enabled = false;
                }
                if (tbPassword != null)
                {
                    tbPassword.Enabled = false;
                }
            }
            else if (cbAuthentication.SelectedIndex == 1) //Database Login
            {

                if (tbUsername != null)
                {
                    tbUsername.Enabled = true;
                }
                if (tbPassword != null)
                {
                    tbPassword.Enabled = true;
                }
            }
            else
            {
                if (tbUsername != null)
                {
                    tbUsername.Enabled = false;
                }
                if (tbPassword != null)
                {
                    tbPassword.Enabled = false;
                }
            }

        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Собираем имя соединения из данных с полей формы</summary>
        public string SetConnectionName()
        {
            string newName = "";

            string newDBType = "";
            switch (cbTypeDB.SelectedIndex)
            {
                case 2:
                    newDBType = "DBF";
                    break;
                case 0:
                    newDBType = "Microsoft SQL";
                    break;
                case 1:
                default:
                    newDBType = "Postgre SQL";
                    break;
            }
            string newServer = tbServerName.Text.Trim();
            string newDatabase = tbDatabaseName.Text.Trim();

            newName = newDBType;
            if (
                (cbTypeDB.SelectedIndex == 0 || cbTypeDB.SelectedIndex == 1) &&
                (!string.IsNullOrWhiteSpace(newServer))
                )
            {
                newName = newName + " - " + newServer;
            }

            if (!string.IsNullOrWhiteSpace(newDatabase))
            {
                newName = newName + " ( " + newDatabase + " )";
            }

            return newName;
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Изменилось имя соединения</summary>
        private void tbConnectionName_TextChanged(object sender, EventArgs e)
        {
            if (tbConnectionName.Text == "") tbConnectionName.Text = SetConnectionName();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выбрано соединение из Истории соединений</summary>
        private void cbConnectionHistory_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = MainWindow.ListConnects.Find(x => x.DBConnectionName == cbConnectionHistory.SelectedItem.ToString());

            if (item != null)
            {
                switch (item.ConnType)
                {
                    case Utilities.ConnType.DBF:
                        cbTypeDB.SelectedIndex = 2;
                        break;
                    case Utilities.ConnType.MSSQL:
                        cbTypeDB.SelectedIndex = 0;
                        break;
                    case Utilities.ConnType.PGSQL:
                    case Utilities.ConnType.None:
                    default:
                        cbTypeDB.SelectedIndex = 1;
                        break;
                }

                tbServerName.Text = item.ServerName;
                tbDatabaseName.Text = item.DBName;

                switch (item.AuthType)
                {
                    case Utilities.AuthType.DATABASE:
                        cbAuthentication.SelectedIndex = 1;
                        break;
                    case Utilities.AuthType.WINDOWS:
                    default:
                        cbAuthentication.SelectedIndex = 0;
                        break;
                }

                tbUsername.Text = item.Username;
                tbPassword.Text = item.Password;

                tbConnectionName.Text = item.DBConnectionName;

            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Выбран тип БД</summary>
        private void cbTypeDB_SelectedIndexChanged(object sender, EventArgs e)
        {
            tbConnectionName.Text = SetConnectionName();

            switch (cbTypeDB.SelectedIndex)
            {
                case 2: // DBF
                    tbServerName.Enabled = false;
                    tbServerName.Text = "";
                    cbAuthentication.Enabled = false;
                    cbAuthentication.SelectedIndex = -1;
                    tbUsername.Enabled = false;
                    tbUsername.Text = "";
                    tbPassword.Enabled = false;
                    tbPassword.Text = "";
                    cbTrustServerCertificate.Checked = false;
                    tbConnectionAdd.Text = "";
                    break;
                case 0:
                    tbServerName.Enabled = true;
                    cbAuthentication.Enabled = true;
                    tbUsername.Enabled = true;
                    tbPassword.Enabled = true;
                    cbTrustServerCertificate.Checked = true;
                    tbConnectionAdd.Text = "";
                    break;
                case 1:
                default:
                    tbServerName.Enabled = true;
                    cbAuthentication.Enabled = true;
                    tbUsername.Enabled = true;
                    tbPassword.Enabled = true;
                    cbTrustServerCertificate.Checked = false;
                    tbConnectionAdd.Text = "";
                    break;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Изменилось имя/адрес сервера БД</summary>
        private void tbServerName_TextChanged(object sender, EventArgs e) //-V3013
        {
            tbConnectionName.Text = SetConnectionName();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Изменилось имя БД</summary>
        private void tbDatabaseName_TextChanged(object sender, EventArgs e)
        {
            tbConnectionName.Text = SetConnectionName();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка DEL для удаления соединения из Истории соединений</summary>
        private void btDel_Click(object sender, EventArgs e)
        {
            if (cbConnectionHistory.SelectedItem != null)
            {
                var item = MainWindow.ListConnects.Find(x => x.DBConnectionName == cbConnectionHistory.SelectedItem.ToString());
                if (item != null)
                {
                    MainWindow.ListConnects.Remove(item);
                    cbConnectionHistory.Items.RemoveAt(cbConnectionHistory.SelectedIndex);
                    cbConnectionHistory.Text = "";
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Отображение формы</summary>
        private void FormLogin_Shown(object sender, EventArgs e)
        {
            cbConnectionHistory.SelectedItem = currentDBConnectionName;
            if (cbAuthentication.SelectedIndex == 1) tbPassword.Focus();
            else btConnect.Focus();
        }

        /// <summary>
        /// Нажата кнопка Выбрать
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btChooseDB_Click(object sender, EventArgs e)
        {
            cbTrustServerCertificate.Checked = false;
            tbConnectionAdd.Text = "";

            if (cbTypeDB.SelectedIndex == 0 || cbTypeDB.SelectedIndex == 1)
            {
                FormEditDB dlg1 = new FormEditDB();
                dlg1.dgListDB.DataSource = MainWindow.APPinfo.ListDatabases;
                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    foreach (DataGridViewRow row in dlg1.dgListDB.SelectedRows)
                    {
                        tbServerName.Text = row.Cells[0].Value.ToString();
                        tbDatabaseName.Text = row.Cells[1].Value.ToString();
                        string project = row.Cells[2].Value.ToString();
                        string dbtype = Utilities.GITProjects.GetDBTypeByProject(project);
                        if (dbtype == "MSSQL")
                        {
                            cbTypeDB.SelectedIndex = 0;
                            cbTrustServerCertificate.Checked = true;
                        }
                        if (dbtype == "PGSQL") cbTypeDB.SelectedIndex = 1;
                        // берем только первую
                        break; //-V3020
                    }
                }
                dlg1.Dispose();

                // удалим дубликаты
                MainWindow.APPinfo.DelDublicateDatabases();
            }

            if (cbTypeDB.SelectedIndex == 2) // DBF 
            {
                string path = MainWindow.Task.TaskPath;
                if (string.IsNullOrWhiteSpace(path))
                {
                    path = MainWindow.APPinfo.TaskFolder;
                }

                path = Dialogs.FolderBrowserDialog(path);

                if (!string.IsNullOrWhiteSpace(path))
                {
                    tbDatabaseName.Text = path;
                }
            }
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
        private void FormLogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormLogin", this);
        }

        private void cbSavePassword_CheckedChanged(object sender, EventArgs e)
        {
            if (cbSavePassword.Checked == false) 
            {
                tbPassword.Text = "";
            }
        }

        private void btSave_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Ignore;
            this.Close();
        }
    }
}
