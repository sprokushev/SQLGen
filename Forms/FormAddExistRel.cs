// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма привязки FreeDocRelationship к FreeDocMarker
    /// </summary>
    public partial class FormAddExistRel : Form
    {
        private WinMarker win;

        /// <summary>
        /// конструктор FormAddExistRel
        /// </summary>
        /// <param name="_win">экземпляр WinMarker</param>
        public FormAddExistRel(WinMarker _win)
        {
            InitializeComponent();
            win = _win;

            // пользовательские настройки GUI
            Default.InitGUI("FormAddExistRel", this, MainWindow.Task.LogFile);
        }

        /// <summary>
        /// Нажата кнопка Сохранить
        /// </summary>
        private void btOk_Click(object sender, EventArgs e)
        {
            if (FreeDocRelationship_AliasName.SelectedIndex == -1)
            {
                MessageBox.Show("Необходимо выбрать FreeDocRelationship_AliasName !");
                FreeDocRelationship_AliasName.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Нажата кнопка Отменить
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Изменился FreeDocRelationship_AliasName
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FreeDocRelationship_AliasName_SelectedIndexChanged(object sender, EventArgs e)
        {
            var item = win.ListFreeDocRelationship.Find(x => x.FreeDocRelationship_AliasName == FreeDocRelationship_AliasName.SelectedItem.ToString());

            if (item != null)
            {
                FreeDocRelationship_id.Text = item.FreeDocRelationship_id;
                EvnClass_SysNick.Text = item.EvnClass_SysNick;
                FreeDocRelationship_AliasTable.Text = item.FreeDocRelationship_AliasTable;
                FreeDocRelationship_AliasQuery.Text = item.FreeDocRelationship_AliasQuery;
                FreeDocRelationship_LinkedAlias.Text = item.FreeDocRelationship_LinkedAlias;
                FreeDocRelationship_LinkDescription.Text = item.FreeDocRelationship_LinkDescription;
            }
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormAddExistRel_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAddExistRel", this);
        }
    }
}
