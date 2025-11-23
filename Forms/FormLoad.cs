// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма загрузки Excel
    /// </summary>
    public partial class FormLoad : Form
    {
        /// <summary>
        /// Конструктор FormLoad
        /// </summary>
        public FormLoad()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormLoad", this, MainWindow.Task.LogFile);
        }

        /// <summary>
        /// Нажата кнопка Выбрать файл
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOpen_Click(object sender, EventArgs e)
        {
            string file = "";

            if ((MainWindow.Task != null) && (MainWindow.Task.TaskPath != ""))
            {
                // Выбрать файл
                file = Dialogs.OpenExcelDialog(MainWindow.Task.TaskPath);
            }

            if (file != "")
            {
                tbFilename.Text = file;
                tbTableName.Text = System.IO.Path.GetFileNameWithoutExtension(file);
            }

            SetTableNameFromExcel();
        }

        /// <summary>
        /// Определить имя таблицы по имени первого поля
        /// </summary>
        private void SetTableNameFromExcel()
        {
            if (tbTableName.Text.ToLower().StartsWith("книга") && System.IO.File.Exists(tbFilename.Text))
            {
                string A1 = Utilities.MSOffice.GetValueExcel(tbFilename.Text, (int)tbNumSheet.Value, 1, 1);
                if (!string.IsNullOrWhiteSpace(A1))
                {
                    // возьмем имя таблицы из значения в первой ячейке
                    var arr = A1.Split('_');
                    tbTableName.Text = arr[0];
                }
            }
        }

        /// <summary>
        /// Нажата кнопка Добавить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btLoad_Click(object sender, EventArgs e)
        {
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
        private void FormLoad_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormLoad", this);
        }

        /// <summary>
        /// Изменилось значение в поле Номер листа в книге
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbNumSheet_ValueChanged(object sender, EventArgs e)
        {
            SetTableNameFromExcel();
        }
    }
}
