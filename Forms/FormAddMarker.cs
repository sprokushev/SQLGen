// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма добавления FreeDocMarker
    /// </summary>
    public partial class FormAddMarker : Form
    {
        private WinMarker win;

        /// <summary>
        /// Конструктор FormAddMarker
        /// </summary>
        /// <param name="_win">экземпляр WinMarker</param>
        public FormAddMarker(WinMarker _win)
        {
            InitializeComponent();
            win = _win;

            // пользовательские настройки GUI
            Default.InitGUI("FormAddMarker", this, MainWindow.Task.LogFile);
        }

        /// <summary>
        /// Нажата кнопка Сохранить
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void btOk_Click(object sender, EventArgs e)
        {
            if (EvnClass_SysNick.SelectedIndex == -1)
            {
                MessageBox.Show("Необходимо выбрать EvnClass_SysNick !");
                EvnClass_SysNick.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(FreeDocMarker_Name.Text))
            {
                MessageBox.Show("Необходимо заполнить FreeDocMarker_Name !");
                FreeDocMarker_Name.Focus();
                return;
            }

            // дублирование по имени
            foreach (var chk in win.ListFreeDocMarker.Where(x => x.FreeDocMarker_id != FreeDocMarker_id.Text.Trim()))
            {
                if ((chk.FreeDocMarker_Name.ToLower() == FreeDocMarker_Name.Text.Trim().ToLower()) &&
                     (chk.EvnClass_SysNick.ToLower() == EvnClass_SysNick.Text.Trim().ToLower())
                     )
                {
                    MessageBox.Show("Маркер с таким именем уже существует !");
                    FreeDocMarker_Name.Focus();
                    return;
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Нажата кнопка Отменить
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void FormAddMarker_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAddMarker", this);
        }
    }
}
