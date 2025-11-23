// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.IO;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма разделения файла на части
    /// </summary>
    public partial class FormSplitFile : Form
    {
        /// <summary>
        /// Размер части
        /// </summary>
        public long SizePart = 0;

        /// <summary>
        /// Имя файла
        /// </summary>
        public string Filename = "";

        /// <summary>
        /// Тип разделения файла
        /// </summary>
        public Utilities.SplitType SplitType = Utilities.SplitType.LINE;

        /// <summary>
        /// Конструктор FormSplitFile
        /// </summary>
        public FormSplitFile()
        {
            InitializeComponent();
            cbUnit.SelectedIndex = 2;

            // пользовательские настройки GUI
            Default.InitGUI("FormSplitFile", this, MainWindow.Task.LogFile);
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
                file = Dialogs.OpenFileDialog(MainWindow.Task.TaskPath);
            }

            if (file != "")
            {
                tbFilename.Text = file;
            }

        }

        /// <summary>
        /// Нажата кнопка Разделить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btSplit_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbFilename.Text) || (!File.Exists(tbFilename.Text.Trim())))
            {
                MessageBox.Show("Необходимо выбрать существующий файл");
                tbFilename.Focus();
                return;
            }

            Filename = tbFilename.Text.Trim();

            SizePart = 0;
            long.TryParse(tbSize.Text, out SizePart);
            if (SizePart == 0)
            {
                MessageBox.Show("Необходимо указать размер части");
                tbSize.Focus();
                return;
            }

            switch (cbUnit.SelectedIndex)
            {
                case 1:
                    SizePart = SizePart * 1024;
                    break;
                case 2:
                    SizePart = SizePart * 1024 * 1024;
                    break;
                case 0:
                default:
                    break;
            }

            if (rbByte.Checked) SplitType = Utilities.SplitType.BYTE;
            else if (rbChar.Checked) SplitType = Utilities.SplitType.CHAR;
            else if (rbKeywords.Checked) SplitType = Utilities.SplitType.KEYWORDS;
            else SplitType = Utilities.SplitType.LINE;

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
        private void FormSplitFile_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormSplitFile", this);
        }
    }
}
