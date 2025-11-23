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
    /// Форма добавления FreeDocRelationship
    /// </summary>
    public partial class FormAddRelationship : Form
    {
        private WinMarker win;

        /// <summary>
        /// Конструктор FormAddRelationship
        /// </summary>
        /// <param name="_win">экземпляр WinMarker</param>
        public FormAddRelationship(WinMarker _win)
        {
            InitializeComponent();
            win = _win;

            // пользовательские настройки GUI
            Default.InitGUI("FormAddRelationship", this, MainWindow.Task.LogFile);
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

            if (string.IsNullOrWhiteSpace(FreeDocRelationship_AliasName.Text))
            {
                MessageBox.Show("Необходимо заполнить FreeDocRelationship_AliasName !");
                FreeDocRelationship_AliasName.Focus();
                return;
            }

            // дублирование по имени
            foreach (var chk in win.ListFreeDocRelationship.Where(x => x.FreeDocRelationship_id != FreeDocRelationship_id.Text.Trim()))
            {
                if ((chk.FreeDocRelationship_AliasName.ToLower() == FreeDocRelationship_AliasName.Text.Trim().ToLower()) &&
                     (chk.EvnClass_SysNick.ToLower() == EvnClass_SysNick.Text.Trim().ToLower())
                    )
                {
                    MessageBox.Show("Связь с таким алиасом уже существует !");
                    FreeDocRelationship_AliasName.Focus();
                    return;
                }
            }


            DialogResult = DialogResult.OK;
            Close();
        }

        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormAddRelationship_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAddRelationship", this);
        }
    }
}
