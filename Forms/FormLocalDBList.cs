// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Форма описания локального справочника
    /// </summary>
    public partial class FormLocalDBList : Form
    {

        /// <summary>Подключение к БД</summary>
        public ConnectDB Connect = null;

        /// <summary>Подключение к promedtest ПГ</summary>
        public ConnectDB ConnectPG = null;

        /// <summary>Подключение к promedadygea ПГ</summary>
        public ConnectDB ConnectPromedadygea = null;

        /// <summary>
        /// Список локальных справочников
        /// </summary>
        private List<LocalDBList> listSprav = new List<LocalDBList>();

        /// <summary>
        /// Конструктор FormLocalDBList
        /// </summary>
        public FormLocalDBList()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormLocalDBList", this, MainWindow.Task.LogFile);
        }

        /// <summary>
        /// Нажата кнопка "Сгенерировать скрипт"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Нажата нопка "Отмена"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Поиск локального справочника
        /// </summary>
        public void FindClick()
        {

            try
            {
                this.Cursor = Cursors.WaitCursor;

                DataTable info = Connect.GetLocalDBList(tbModule.Text, new List<string> { tbSchema.Text + "." + tbNick.Text }, out string _query, out string _message);
                listSprav.Clear();

                if (info != null) //-V3022
                {
                    foreach (DataRow row in info.Rows)
                    {
                        LocalDBList item = new LocalDBList();

                        foreach (DataColumn column in info.Columns)
                        {
                            if (column.ColumnName.ToLower() == "LocalDBList_id".ToLower())
                            {
                                item.LocalDBList_id = row[column].ToString();
                            }
                            if (column.ColumnName.ToLower() == "LocalDBList_Name".ToLower())
                            {
                                item.LocalDBList_Name = row[column].ToString();
                            }
                            if (column.ColumnName.ToLower() == "LocalDBList_Prefix".ToLower())
                            {
                                item.LocalDBList_Prefix = row[column].ToString();
                            }
                            if (column.ColumnName.ToLower() == "LocalDBList_Nick".ToLower())
                            {
                                item.LocalDBList_Nick = row[column].ToString();
                            }
                            if (column.ColumnName.ToLower() == "LocalDBList_Schema".ToLower())
                            {
                                item.LocalDBList_Schema = row[column].ToString();
                            }
                            if (column.ColumnName.ToLower() == "LocalDBList_Key".ToLower())
                            {
                                item.LocalDBList_Key = row[column].ToString();
                            }
                            if (column.ColumnName.ToLower() == "LocalDBList_Module".ToLower())
                            {
                                item.LocalDBList_Module = row[column].ToString();
                            }
                            if (column.ColumnName.ToLower() == "LocalDBList_Descr".ToLower())
                            {
                                item.LocalDBList_Descr = row[column].ToString();
                            }
                            if (column.ColumnName.ToLower() == "Region_id".ToLower())
                            {
                                item.Region_id = row[column].ToString();
                            }
                            if (column.ColumnName.ToLower() == "RegionalLocalDBList_sql".ToLower())
                            {
                                item.RegionalLocalDBList_sql = row[column].ToString();
                            }
                            if (column.ColumnName.ToLower() == "RegionalLocalDBList_pgsql".ToLower())
                            {
                                item.RegionalLocalDBList_pgsql = row[column].ToString();
                            }
                        }
                        listSprav.Add(item);
                    }
                }

                tbName.Items.Clear();
                bool first = true;
                foreach (var item in listSprav)
                {
                    tbName.Items.Add(new ComboItem(item.LocalDBList_Name, item.LocalDBList_id));

                    if (first)
                    {
                        SetInfo(item);
                        first = false;
                    }
                }



            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Arrow;
                App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
            }
            this.Cursor = Cursors.Arrow;
        }


        private void btFind_Click(object sender, EventArgs e)
        {
            FindClick();
        }


        private void SetPgSql()
        {
            try
            {
                tbPGSQL.Text = ConnectPG.GetRegionalLocalDbList_PgSql(tbModule.Text, tbSchema.Text, tbName.Text);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
        }

        private void btFromPG_Click(object sender, EventArgs e)
        {
            SetPgSql();
        }

        private void SetInfo(LocalDBList item)
        {
            tbName.Text = item.LocalDBList_Name;
            tbPrefix.Text = item.LocalDBList_Prefix;
            tbSchema.Text = item.LocalDBList_Schema;
            tbNick.Text = item.LocalDBList_Nick;
            tbMSSQL.Text = (item.RegionalLocalDBList_sql ?? "")
                .Replace("\r\n", "\n")
                .Replace("\n", "\r\n");
            tbPGSQL.Text = (item.RegionalLocalDBList_pgsql ?? "")
                .Replace("\r\n", "\n")
                .Replace("\n", "\r\n");
            tbKey.Text = item.LocalDBList_Key;
            tbModule.Text = item.LocalDBList_Module;
            tbDescr.Text = item.LocalDBList_Descr;
            tbRegion.Text = item.Region_id;

            if (Connect.DBType == "MSSQL")
            {
                SetPgSql();
            }
        }


        private void tbName_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (tbName.SelectedIndex != -1)
            {
                ComboItem item = (ComboItem)tbName.SelectedItem;

                foreach (var sprav in listSprav.Where(x => x.LocalDBList_id == item.Tag()))
                {
                    SetInfo(sprav);
                    break;
                }
            }

        }

        private void btFromPromedadygea_Click(object sender, EventArgs e)
        {
            try
            {
                tbPGSQL.Text = ConnectPromedadygea.GetRegionalLocalDbList_PgSql(tbModule.Text, tbSchema.Text, tbName.Text);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
        }

        private void FormLocalDBList_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormLocalDBList", this);
        }
    }

    /// <summary>
    /// Элемент списка
    /// </summary>
    internal class ComboItem : object
    {

        private string m_Name;
        private string m_Value;

        /// <summary>
        /// Конструктор ComboItem
        /// </summary>
        /// <param name="name">отображаемое в списке наименование</param>
        /// <param name="in_value">идентификатор</param>
        public ComboItem(string name, string in_value)
        {
            m_Name = name;
            m_Value = in_value;
        }

        /// <summary>
        /// Отображаемое в списке наименование
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return m_Name;
        }

        /// <summary>
        /// Идентификатор
        /// </summary>
        /// <returns></returns>
        public string Tag()
        {
            return m_Value;
        }
    };
}
