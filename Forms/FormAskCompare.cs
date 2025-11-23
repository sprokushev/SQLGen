// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
//using Microsoft.Office.Interop.Excel;
using System;
using System.IO;
//using System.Windows.Controls;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{

    /// <summary>
    /// Форма добавления скриптов в задачу
    /// </summary>
    public partial class FormAskCompare : Form
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Текст changeset</summary>
        public string changesetText;

        /// <summary>Исходный файл</summary>
        public string fromFile;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>конструктор Формы добавления скриптов в задачу</summary>
        public FormAskCompare()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormAskCompare", this, MainWindow.Task.LogFile);
        }

        /// <summary>Кнопка Завершить</summary>
        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btCompare_Click(object sender, EventArgs e)
        {
            // файл-источник
            string from_file = fromFile;

            if (string.IsNullOrWhiteSpace(from_file) || (!File.Exists(from_file)))
            {
                return;
            }

            if (from_file.Contains(" ")) from_file = "\"" + from_file + "\"";

            if (string.IsNullOrWhiteSpace(tbChangesetName.Text))
            {
                // сравнение 
                Utilities.External.ExecuteFile(
                    App.AppPath,
                    Path.Combine(App.AppPath, "git_compare.cmd"),
                    tbGITFilename.Text + " " + from_file,
                    true,
                    true,
                    false,
                    true,
                    MainWindow.Task.LogFile
                );
            }
            else
            {
                // временный файл с содержанием changeset
                string to_file = Path.GetTempFileName();
                App.tempFiles.Add(to_file);

                string s = changesetText.TrimEndNewLine("\n");

                File.WriteAllText(to_file, s);

                if (to_file.Contains(" ")) to_file = "\"" + to_file + "\"";

                // сравнение 
                Utilities.External.ExecuteFile(
                    App.AppPath,
                    Path.Combine(App.AppPath, "git_compare.cmd"),
                    to_file + " " + from_file,
                    true,
                    true,
                    false,
                    true,
                    MainWindow.Task.LogFile
                );
            }

        }

        private void btOwerwrite_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();

        }

        private void FormAskCompare_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAskCompare", this);
        }

        private void btAppend_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }
    }

}