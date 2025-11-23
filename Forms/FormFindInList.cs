// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Shapes;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>Форма для выбора из списка с поиском</summary>
    public partial class FormFindInList : Form
    {
        /// <summary>
        /// Список
        /// </summary>
        private DataTable list { get; set; }

        /// <summary>
        /// Добавление string в список
        /// </summary>
        /// <param name="item">строка</param>
        /// <returns></returns>
        public void AddItem(string item)
        {
            if (string.IsNullOrWhiteSpace(item)) return;

            //var listItem = list.Select(string.Format("itemValue = '{0}'", item)); 

            if (list.Rows.Find(item) == null)
            {
                list.Rows.Add(new object[] { item });
            }

            return;
        }

        /// <summary>
        /// Добавление List(string) в список
        /// </summary>
        /// <param name="items">список строк</param>
        public void AddItems(List<string> items)
        {
            Utilities.Other.ForEach(items, x => AddItem(x));
        }

        /// <summary>Конструктор FormFindInList</summary>
        /// <param name="_logfile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public FormFindInList(string _logfile)
        {
            InitializeComponent();
            list = new DataTable("list");
            list.Columns.Add(new DataColumn("itemValue", Type.GetType("System.String")));
            list.PrimaryKey = new DataColumn[] { list.Columns["itemValue"] };
            //BindingSource bs = new BindingSource();
            //bs.DataSource = list;
            dgListValues.DataSource = list;

            // пользовательские настройки GUI
            Default.InitGUI("FormFindInList", this, _logfile);
        }

        /// <summary>Нажата кнопка Выбрать</summary>
        private void btOk_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgListValues.SelectedRows)
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
        /// Изменена строка поиска
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbFind_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbFind.Text))
            {
                (dgListValues.DataSource as DataTable).DefaultView.RowFilter = String.Empty;
            }
            else
            {
                var list = tbFind.Text.ToList(new char[] { ' ' }, true);

                string result = "";
                string oper = "";
                foreach (var item in list)
                {
                    result += $" {oper} itemValue like '%{item}%'";
                    oper = "AND";
                }

                (dgListValues.DataSource as DataTable).DefaultView.RowFilter = result;
            }
        }

        /// <summary>
        /// Двойной клик мыши на позиции списка
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgListValues_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            btOk_Click(null, null);
        }

        /// <summary>
        /// Форма закрывается
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FormFindInList_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormFindInList", this);
        }
    }
}
