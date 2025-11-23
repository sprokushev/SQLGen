// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;
using System.Linq;

namespace SQLGen
{
    /// <summary>Форма для выбора базы данных</summary>
    public partial class FormEditDB : Form
    {
        /// <summary>Конструктор FormEditDB</summary>
        public FormEditDB()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormEditDB", this, "");
        }

        /// <summary>Нажата кнопка Выбрать</summary>
        private void btOk_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgListDB.SelectedRows)
            {
                DialogResult = DialogResult.OK;
                Close();
                return;
            }

            MessageBox.Show("Необходимо выбрать строку!");
            DialogResult = DialogResult.None;
        }

        /// <summary>Нажата кнопка Сохранить</summary>
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Нажата кнопка Добавить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAdd_Click(object sender, EventArgs e)
        {
            FormAddDB dlg1 = new FormAddDB();
            dlg1.Text = "Добавить базу данных";

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MainWindow.APPinfo.AddDatabase(dlg1.tbServerName.Text, dlg1.tbDBName.Text, dlg1.cbGITProject.Text, dlg1.cbDBRole.Text, dlg1.isMainTest.Checked == true, false);
            }
            dlg1.Dispose();
        }

        /// <summary>
        /// Нажата кнопка Удалить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDel_Click(object sender, EventArgs e)
        {
            bool isFound = false;
            foreach (DataGridViewRow row in dgListDB.SelectedRows)
            {
                if (System.Windows.Forms.MessageBox.Show("Удалить " + row.Cells[0].Value.ToString() + " - " + row.Cells[1].Value.ToString() + " ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    dgListDB.Rows.Remove(row);
                    isFound = true;
                    break;
                }
                else return;
            }

            if (!isFound) MessageBox.Show("Необходимо выбрать строку!");
        }

        /// <summary>
        /// Нажата кнопка Редактировать
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btEdit_Click(object sender, EventArgs e)
        {
            bool isFound = false;
            foreach (DataGridViewRow row in dgListDB.SelectedRows)
            {
                FormAddDB dlg1 = new FormAddDB();
                dlg1.Text = "Редактировать базу данных";

                string prev_ServerName = row.Cells[0].Value.ToString();
                string prev_DBName = row.Cells[1].Value.ToString();
                string prev_GITProject = row.Cells[2].Value.ToString();
                string prev_DBRole = row.Cells[3].Value.ToString();
                bool prev_isMainTest = (bool)(row.Cells[4].Value);

                dlg1.tbServerName.Text = prev_ServerName;
                dlg1.tbDBName.Text = prev_DBName;
                dlg1.cbGITProject.Text = prev_GITProject;
                dlg1.cbDBRole.Text = prev_DBRole;
                dlg1.isMainTest.Checked = prev_isMainTest;

                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    var found = MainWindow.APPinfo.ListDatabases.FirstOrDefault(x =>
                        x.ServerName == prev_ServerName &&
                        x.DBName == prev_DBName &&
                        x.GITProject == prev_GITProject &&
                        x.DBRole == prev_DBRole &&
                        x.isMainTest == prev_isMainTest
                        );

                    MainWindow.APPinfo.AddDatabase(
                        dlg1.tbServerName.Text,
                        dlg1.tbDBName.Text,
                        dlg1.cbGITProject.Text,
                        dlg1.cbDBRole.Text,
                        dlg1.isMainTest.Checked == true,
                        true,
                        found
                        );
                }
                dlg1.Dispose();
                isFound = true;
                break; //-V3020
            }
            if (!isFound)
            {
                MessageBox.Show("Необходимо выбрать строку!");
            }
        }

        /// <summary>
        /// Сменился адрес у сервера для всех БД
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btChangeServerInAllConnects_Click(object sender, EventArgs e)
        {
            bool isFound = false;
            foreach (DataGridViewRow row in dgListDB.SelectedRows)
            {
                FormAskServer dlg1 = new FormAskServer();
                dlg1.tbOldName.Text = row.Cells[0].Value.ToString();
                dlg1.tbDBType.Text = row.Cells[5].Value.ToString();

                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    MainWindow.APPinfo.ChangeServerInAllConnects(
                        dlg1.tbOldName.Text,
                        dlg1.tbNewName.Text,
                        dlg1.tbDBType.Text
                        );
                }
                dlg1.Dispose();
                isFound = true;
                break; //-V3020
            }
            if (!isFound)
            {
                MessageBox.Show("Необходимо выбрать строку!");
            }
        }

        /// <summary>
        /// Нажата кнопка Переезд БД на другой сервер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btMoveDBtoOtherServer_Click(object sender, EventArgs e)
        {
            bool isFound = false;
            foreach (DataGridViewRow row in dgListDB.SelectedRows)
            {
                FormAskDB dlg1 = new FormAskDB();
                dlg1.tbOldServer.Text = row.Cells[0].Value.ToString();
                dlg1.tbOldDB.Text = row.Cells[1].Value.ToString();
                dlg1.tbNewDB.Text = dlg1.tbOldDB.Text;
                dlg1.tbDBType.Text = row.Cells[5].Value.ToString();

                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    MainWindow.APPinfo.MoveDBtoOtherServer(
                        dlg1.tbOldServer.Text,
                        dlg1.tbOldDB.Text,
                        dlg1.tbNewServer.Text,
                        dlg1.tbNewDB.Text,
                        dlg1.tbDBType.Text
                        );
                }
                dlg1.Dispose();
                isFound = true;
                break; //-V3020
            }
            if (!isFound)
            {
                MessageBox.Show("Необходимо выбрать строку!");
            }
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormEditDB_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormEditDB", this);
        }

        /// <summary>
        /// Двойное нажатие мыши на ячейке
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgListDB_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            btEdit_Click(sender, e);
        }
    }
}
