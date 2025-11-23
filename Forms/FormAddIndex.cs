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
    /// Форма добавления/изменения индексов
    /// </summary>
    public partial class FormAddIndex : Form
    {

        /// <summary>Родительский объект TableDB</summary>
        public TableDB parent;

        /// <summary>Изменяемый IndexDB</summary>
        public IndexDB index;

        /// <summary>Родительский объект TableDB</summary>
        public string OriginalName;

        /// <summary>Тип скрипта</summary>
        public Utilities.ScriptType ScriptType = Utilities.ScriptType.CREATE;

        /// <summary>
        /// Конструктор FormAddIndex
        /// </summary>
        public FormAddIndex()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormAddIndex", this, MainWindow.Task.LogFile);
        }

        /// <summary>Нажата кнопка Сгенерировать скрипт</summary>
        private void btGenerate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbIndexName.Text))
            {
                MessageBox.Show("Необходимо заполнить Наименование индекса !");
                tbIndexName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tbIndexPredicat.Text))
            {
                MessageBox.Show("Необходимо заполнить Предикат индекса !");
                tbIndexPredicat.Focus();
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>Нажата кнопка Отмена</summary>
        private void btCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>Выход из поля "Наименование"</summary>
        private void tbIndexName_Leave(object sender, EventArgs e)
        {
            if ((ScriptType == Utilities.ScriptType.ALTER) && string.IsNullOrWhiteSpace(tbIndexToDel.Text) && (OriginalName != tbIndexName.Text.Trim()))
            {
                // в режиме пересоздания изменилось назименование индекса
                tbIndexToDel.Text = OriginalName;
            }

            string indextodel = tbIndexToDel.Text.Trim();

            indextodel = indextodel
                .Replace(" ", string.Empty)
                .Replace("\t", ",")
                .Replace("\r", ",")
                .Replace("\n", ",")
                .Replace(",,", ",")
                .Replace(",,", ",")
                .Replace(";", ",")
                .Replace(",,", ",");

            if (indextodel == tbIndexName.Text.Trim()) indextodel = string.Empty;

            indextodel = indextodel.Replace("," + tbIndexName.Text.Trim(), string.Empty);
            indextodel = indextodel.Replace(tbIndexName.Text.Trim() + ",", string.Empty);

            tbIndexToDel.Text = indextodel.Replace(",", ", ");
        }

        /// <summary>
        /// Нажата кнопка Сгенерировать (имя)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void btAutoName_Click(object sender, EventArgs e)
        {
            string name = "idx_" + parent.TableEdit.TableNameReady
                + "_";
            string predicat = tbIndexPredicat.Text.Trim()
                .Replace(" ", string.Empty)
                .Replace("\t", ",")
                .Replace("\r", ",")
                .Replace("\n", ",")
                .Replace(",,", ",")
                .Replace(",,", ",")
                .Replace(";", ",")
                .Replace(",,", ",")
                .Replace(",", "_");
            string include = tbIndexInclude.Text.Trim()
                .Replace(" ", string.Empty)
                .Replace("\t", ",")
                .Replace("\r", ",")
                .Replace("\n", ",")
                .Replace(",,", ",")
                .Replace(",,", ",")
                .Replace(";", ",")
                .Replace(",,", ",")
                .Replace(",", "_");

            name += predicat;
            if (!string.IsNullOrWhiteSpace(include)) name += "_inc";

            tbIndexName.Text = name;
            tbIndexName_Leave(sender, e);
        }

        private void FillListBoxItems(CheckedListBox cb, string txt_fields)
        {
            string keys = txt_fields.Trim()
                .Replace(" ", string.Empty)
                .Replace("\t", ",")
                .Replace("\r", ",")
                .Replace("\n", ",")
                .Replace(";", ",")
                .Replace(",,", ",")
                .Replace(",,", ",")
                .Replace(",,", ",")
                .Replace(",,", ",")
                .Replace("\"", "");

            var arr = keys.Split(',');

            foreach (var key in arr)
            {
                foreach (var field in parent.TableEdit.ListField
                    .Where(x => x.FieldNameCompare.Trim() == key.ToLower().Trim())
                    )
                {
                    cb.Items.Add(field.FieldNameReady);
                }
            }

            foreach (var field in parent.TableEdit.ListField.OrderBy(x => x.FieldOrder))
            {
                if (cb.Items.IndexOf(field.FieldNameReady) == -1)
                {
                    cb.Items.Add(field.FieldNameReady);
                }
            }

            for (int i = 0; i < cb.Items.Count; i++)
            {
                string item = cb.Items[i].ToString().ToLower();

                foreach (var key in arr)
                {
                    if (key.ToLower().Trim() == item)
                    {
                        cb.SetItemChecked(i, true);
                    }
                }
            }
        }

        /// <summary>Нажата кнопка Выбор поля (Предикат)</summary>
        private void btPredicatFill_Click(object sender, EventArgs e)
        {
            FormCheckedListBox dlg1 = new FormCheckedListBox();
            dlg1.clbList.Items.Clear();

            FillListBoxItems(dlg1.clbList, tbIndexPredicat.Text);

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string result = "";

                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    if (result != "") result += ", ";
                    result += itemChecked.ToString();
                }

                tbIndexPredicat.Text = result;
            }
            dlg1.Dispose();
        }

        /// <summary>Нажата кнопка Выбор поля (Include)</summary>
        private void btIncludeFill_Click(object sender, EventArgs e)
        {
            FormCheckedListBox dlg1 = new FormCheckedListBox();
            dlg1.clbList.Items.Clear();

            FillListBoxItems(dlg1.clbList, tbIndexInclude.Text);

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string result = "";

                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    if (result != "") result += ", ";
                    result += itemChecked.ToString();
                }

                tbIndexInclude.Text = result;
            }
            dlg1.Dispose();
        }

        private void cbScriptType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbScriptType.SelectedIndex != -1)
            {
                switch (cbScriptType.SelectedItem)
                {
                    case "ALTER":
                        ScriptType = Utilities.ScriptType.ALTER;
                        break;
                    case "DROP":
                        ScriptType = Utilities.ScriptType.DROP;
                        break;
                    case "CREATE":
                    default:
                        ScriptType = Utilities.ScriptType.CREATE;
                        break;
                }
            }
            else ScriptType = Utilities.ScriptType.CREATE;
        }

        private void tbIndexName_TextChanged(object sender, EventArgs e)
        {
            tbIndexName.Text = tbIndexName.Text.Replace("[", string.Empty).Replace("]", string.Empty).Replace("\"", string.Empty);
        }

        private void tbIndexPredicat_TextChanged(object sender, EventArgs e)
        {
            tbIndexPredicat.Text = tbIndexPredicat.Text.Replace("[", string.Empty).Replace("]", string.Empty);
        }

        private void tbIndexInclude_TextChanged(object sender, EventArgs e)
        {
            tbIndexInclude.Text = tbIndexInclude.Text.Replace("[", string.Empty).Replace("]", string.Empty);
        }

        private void tbIndexWhere_TextChanged(object sender, EventArgs e)
        {
            tbIndexWhere.Text = tbIndexWhere.Text.Replace("[", string.Empty).Replace("]", string.Empty);
        }

        private void tbIndexToDel_TextChanged(object sender, EventArgs e)
        {
            tbIndexToDel.Text = tbIndexToDel.Text.Replace("[", string.Empty).Replace("]", string.Empty);
        }

        private void FormAddIndex_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAddIndex", this);
        }
    }
}

