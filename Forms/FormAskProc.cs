// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Linq;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма запроса имени хранимки
    /// </summary>
    public partial class FormAskProc : Form
    {

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Конструктор FormAskProc</summary>
        public FormAskProc()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormAskProc", this, MainWindow.Task.LogFile);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата клавиша</summary>
        private void tbNumTask_KeyDown(object sender, KeyEventArgs e)
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
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void lbTitle_Click(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Нажата кнопка Пропустить текущий
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Нажата кнопка Прекратить разбор
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAbort_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAskProc_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAskProc", this);
        }

        /// <summary>
        /// Подключение к БД
        /// </summary>
        public System.Windows.Controls.ComboBox cbConnect = null;

        /// <summary>
        /// Нажата кнопка Выполнить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btExec_Click(object sender, EventArgs e)
        {
            if (
                (cbConnect != null) &&
                (cbConnect.SelectedIndex != -1) &&
                (!string.IsNullOrWhiteSpace(tbProcText.Text)) &&
                btExec.Enabled
            )
            {
                ConnectDB Connect = Utilities.Controls.OpenConnectFromComboBox(cbConnect, false);
                if ((Connect == null) || Connect.isNotConnected) //-V3063
                {
                    App.AddLog("Подключение " + cbConnect.Text + " не открыто !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    return;
                }

                this.Cursor = Cursors.WaitCursor;

                try
                {
                    // убираем тестовые changeset из скрипта
                    string txt = "";
                    foreach (var item in SQLChangeset.ReadChangeset(tbProcText.Text, true, MainWindow.Task.LogFile)
                        .Where(x => x.name != "test" || x.author != "dev")
                    )
                    {
                        txt += item.text + "\n\n";
                    }

                    // выполняем
                    Connect.ExecuteNonQuery(txt, out string Info);
                    if (!string.IsNullOrWhiteSpace(Info))
                    {
                        App.AddLog("Выполнено успешно, но есть предупреждения:" + Environment.NewLine + Info, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                    else
                    {
                        App.AddLog("Выполнено успешно!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("Ошибка выполнения: " + Environment.NewLine, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

                this.Cursor = Cursors.Arrow;

                Connect.CloseConnect();
            }
        }

        /// <summary>
        /// При отображении формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAskProc_Shown(object sender, EventArgs e)
        {
            if (
                (cbConnect != null) &&
                (cbConnect.SelectedIndex != -1)
                )
            {
                tbConnection.Text = cbConnect.Text;
                btExec.Enabled = true;
            }
            else
            {
                tbConnection.Text = "";
                btExec.Enabled = false;
            }
        }
    }
}
