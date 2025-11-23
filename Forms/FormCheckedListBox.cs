// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма выбора нескольких значений из списка
    /// </summary>
    public partial class FormCheckedListBox : Form
    {
        /// <summary>
        /// Конструктор FormCheckedListBox
        /// </summary>
        public FormCheckedListBox()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormCheckedListBox", this, MainWindow.Task.LogFile);
        }

        /// <summary>
        /// Нажата кнопка Выбрать только отмеченные
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOk_Click(object sender, EventArgs e)
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
        /// Выбрать все позиции
        /// </summary>
        public void SetAll()
        {
            for (int i = 0; i < clbList.Items.Count; i++)
            {
                clbList.SetItemChecked(i, true);
            }
        }

        /// <summary>
        /// Нажата кнопка Выбрать все
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btAll_Click(object sender, EventArgs e)
        {
            SetAll();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormCheckedListBox_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormCheckedListBox", this);
        }
    }
}
