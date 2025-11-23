// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Path = System.IO.Path;
using SQLGen.Controls;
using SQLGen.Utilities;
using System.Windows.Data;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace SQLGen
{
    /// <summary>
    /// Окно добавления\изменения FK
    /// </summary>
    public partial class WinAddFK : Window
    {
        /// <summary>ссылка на экземпляр WinTable</summary>
        public WinTable winTable;

        /// <summary>соединение с БД</summary>
        public ConnectDB Connect;

        /// <summary>схема таблицы</summary>
        public string SchemaName;

        /// <summary>имя таблицы</summary>
        public string TableName;

        /// <summary>
        /// Имя таблицы внешнего ключа
        /// </summary>
        private string _fkTableName = "";

        /// <summary>
        /// Список связей полей для внешнего ключа
        /// </summary>
        public ObservableCollection<FKItem> FieldsLinkList = new ObservableCollection<FKItem>();

        /// <summary>
        /// Список полей таблицы внешнего ключа
        /// </summary>
        public ObservableCollection<FieldToList> FKFieldList = new ObservableCollection<FieldToList>();

        /// <summary>Конструктор WinAddFK</summary>
        public WinAddFK()
        {
            InitializeComponent();

            dgFieldsLink.ItemsSource = FieldsLinkList;

            // пользовательские настройки GUI
            Default.InitGUI("WinAddFK", this, mainGrid, null, null, null, MainWindow.Task.LogFile);
        }

        /// <summary>При открытии окна WinAddFK</summary>
        private void winAddFK_Activated(object sender, EventArgs e)
        {
            dgFields.Focus();
        }

        /// <summary>При закрытии окна WinAddFK</summary>
        private void winAddFK_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinAddFK", this, null);
        }

        /// <summary>
        /// Обновить список полей таблицы внешнего ключа
        /// </summary>
        public void FKTableChanged()
        {
            if ((tbFKTable.Text ?? "").Trim().ToLower() == (_fkTableName ?? "").Trim().ToLower())
            {
                // не изменился
                return;
            }

            FKFieldList.Clear();

            PKInfo pkinfo = new PKInfo();

            if (
                (Connect != null) &&
                (!string.IsNullOrWhiteSpace(tbFKTable.Text))
            )
            {
                pkinfo = Connect.GetTablePK(tbFKTable.Text);

                if (pkinfo.HasPK)
                {
                    tbFKTable.Text = pkinfo.TableName;
                }
            }

            foreach (var item in pkinfo.Fields.OrderBy(x => x.PKOrder))
            {
                FKFieldList.Add(new FieldToList() { Order = item.PKOrder, Name = item.FieldName });
            }

            FKField.ItemsSource = FKFieldList;

            // очистим значения FKField
            foreach (var item in FieldsLinkList)
            {
                item.FKField = "";
            }

            /*
            // очистим значения FKField
            foreach (var item in FieldsLinkList)
            {
                item.FKField = "";
                item.FKOrder = 999;
            }

            // пробуем подобрать по имени
            int cnt = 0;
            foreach (var item in FKFieldList)
            {
                cnt++;

                // ищем первое поле в списке связей с таким же именем
                var found = FieldsLinkList
                    .Where(x =>
                        x.FieldName.ToLower() == item.ToLower()
                    )
                    .FirstOrDefault();

                if (found != null)
                {
                    found.FKField = item;
                    found.FKOrder = cnt;
                }
                else
                {
                    // если поля в списке связей нет - добавим (если оно есть в списке полей таблицы)
                    foreach (FieldToList row in dgFields.Items)
                    {
                        if (row.FieldName.ToLower() == item.ToLower())
                        {
                            AddFieldToLink(row.FieldName, item, cnt);
                            break;
                        }
                    }
                }
            }
            */

            /*
            // затем попробуем проставить оставшиеся по порядку
            foreach (var item in FKFieldList)
            {
                // ищем первое поле с таким же именем
                var found = FieldsLinkList
                    .Where(x =>
                        x.FieldName.ToLower() == item.ToLower()
                    )
                    .OrderBy(x => x.FKOrder)
                    .FirstOrDefault();

                if (found == null)
                {
                    // поля с таким же именем нет, ищем первое пустое
                    found = FieldsLinkList
                    .Where(x =>
                        string.IsNullOrWhiteSpace(x.FKField)
                    )
                    .OrderBy(x => x.FKOrder)
                    .FirstOrDefault();

                    if (found != null)
                    {
                        found.FKField = item;
                    }
                }
            }
            */

            _fkTableName = tbFKTable.Text;

            dgFieldsLinkRefresh();

            if (!string.IsNullOrWhiteSpace(tbFKTable.Text))
            {
                rbFKOther.IsChecked = true;
            }
        }

        /// <summary>
        /// Нажата кнопка "Найти" для таблицы FOREIGN KEY
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btChooseFKTable_Click(object sender, RoutedEventArgs e)
        {
            if (Connect == null) return;

            var list = Connect.FillAlternateTable("");

            FormFindInList dlg1 = new FormFindInList(MainWindow.Task.LogFile);

            dlg1.AddItems(list);

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string result = "";

                foreach (System.Windows.Forms.DataGridViewRow row in dlg1.dgListValues.SelectedRows)
                {
                    result = row.Cells[0].Value.ToString();
                    // берем только первую
                    break; //-V3020
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    tbFKTable.Text = result;
                    FKTableChanged();
                }
            }

            dlg1.Dispose();
        }

        /// <summary>Обновить список полей</summary>
        public void dgFieldsRefresh()
        {
            ListCollectionView cvTasks = (ListCollectionView)CollectionViewSource.GetDefaultView(dgFields.ItemsSource);

            if (cvTasks != null)
            {
                if (cvTasks.IsAddingNew) cvTasks.CancelNew();
                if (cvTasks.IsEditingItem) cvTasks.CancelEdit();
            }

            if (cvTasks != null && cvTasks.CanSort == true)
            {
                cvTasks.SortDescriptions.Clear();
                cvTasks.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
            }

            dgFields.Items.Refresh();
        }

        /// <summary>Обновить список связей</summary>
        public void dgFieldsLinkRefresh()
        {
            ListCollectionView cvTasks = (ListCollectionView)CollectionViewSource.GetDefaultView(dgFieldsLink.ItemsSource);

            if (cvTasks != null)
            {
                if (cvTasks.IsAddingNew) cvTasks.CommitNew();
                if (cvTasks.IsEditingItem) cvTasks.CommitEdit();
            }

            if (cvTasks != null && cvTasks.CanSort == true)
            {
                cvTasks.SortDescriptions.Clear();
                cvTasks.SortDescriptions.Add(new SortDescription("FKOrder", ListSortDirection.Ascending));
            }

            dgFieldsLink.Items.Refresh();

            FKNameRefresh();
        }

        internal void FKNameRefresh()
        {
            string fields = "";

            foreach (var item in FieldsLinkList
                .Where(x => 
                    !string.IsNullOrWhiteSpace(x.FieldName) &&
                    (
                        !string.IsNullOrWhiteSpace(x.FKField) ||
                        !string.IsNullOrWhiteSpace(tbCHECK.Text)
                    )
                )
                .OrderBy(x => x.FKOrder)
            )
            {
                fields += string.IsNullOrWhiteSpace(fields) ? item.FieldName : "_" + item.FieldName;
            }

            if (winTable != null)
            {
                tbFKName.Text = winTable.Table.TableEdit.GetFKNameDefault(fields);
            }

            if (string.IsNullOrWhiteSpace (tbFKName.Text)) 
            {
                tbFKName.Text = $"fk_{this.TableName}_{fields}";
            }
        }

        /// <summary>
        /// скопировать строку из "Список полей таблицы" в "Связь полей"
        /// </summary>
        /// <param name="_fieldName">имя поля внешнего ключа</param>
        /// <param name="_fkField">PK внешнего ключа</param>
        /// <param name="_fkOrder">N по порядку</param>
        public void AddFieldToLink(string _fieldName, string _fkField, int _fkOrder)
        {
            if (_fkOrder == 0)
            {
                if (FieldsLinkList.Count == 0)
                {
                    _fkOrder = 1;
                }
                else
                {
                    int maxOrder = FieldsLinkList.Max(x => x.FKOrder);
                    _fkOrder = maxOrder + 1;
                }
            }

            _fieldName = _fieldName ?? "";
            _fkField = _fkField ?? "";

            var found = FieldsLinkList
                .Where(x =>
                    x.FieldName.ToLower() == _fieldName.ToLower() 
                )
                .FirstOrDefault();

            if (found == null)
            {
                FieldsLinkList.Add(new FKItem(this)
                {
                    FieldName = _fieldName,
                    FKField = _fkField,
                    FKOrder = _fkOrder
                });
            }

            dgFieldsLinkRefresh();
        }

        /// <summary>
        /// Выбор двойным кликом мыши строки в "Список полей таблицы" и копирование в "Связь полей"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgFields_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;

            btRight_Click(null, null);
        }

        /// <summary>
        /// Нажата кнопка "Вправо" - копирование строки из "Список полей таблицы" в "Связь полей"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btRight_Click(object sender, RoutedEventArgs e)
        {
            if (!dgFieldsLink.IsFocused) dgFieldsLink.Focus();

            if (dgFields.SelectedIndex >= 0)
            {
                AddFieldToLink((dgFields.SelectedItem as FieldToList).Name, "", 0);
            }
        }

        /// <summary>
        /// Нажата кнопка "Влево" - убрать строку из "Связь полей"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btLeft_Click(object sender, RoutedEventArgs e)
        {
            if (!dgFieldsLink.IsFocused) dgFieldsLink.Focus();

            if (dgFieldsLink.SelectedIndex >= 0)
            {
                var link = dgFieldsLink.SelectedItem as FKItem;

                FieldsLinkList.Remove(link);

                dgFieldsLinkRefresh();
            }
        }

        /// <summary>
        /// Нажата кнопка "Вверх" - поднять выше строку в "Связь полей"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btUp_Click(object sender, RoutedEventArgs e)
        {
            if (!dgFieldsLink.IsFocused) dgFieldsLink.Focus();

            if (dgFieldsLink.SelectedIndex >= 0)
            {
                // выбранная строка
                var link = dgFieldsLink.SelectedItem as FKItem;

                if (link != null)
                {
                    var prev_link = FieldsLinkList
                        .Where(x => x.FKOrder < link.FKOrder)
                        .OrderByDescending(x => x.FKOrder)
                        .FirstOrDefault();

                    if (prev_link != null)
                    {
                        // есть предыдущая строка
                        var prev_order = prev_link.FKOrder;
                        prev_link.FKOrder = link.FKOrder;
                        link.FKOrder = prev_order;
                    }
                }

                dgFieldsLinkRefresh();
            }
        }

        /// <summary>
        /// Нажата кнопка "Вниз" - опустить ниже строку в "Связь полей"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btDown_Click(object sender, RoutedEventArgs e)
        {
            if (!dgFieldsLink.IsFocused) dgFieldsLink.Focus();

            if (dgFieldsLink.SelectedIndex >= 0)
            {
                // выбранная строка
                var link = dgFieldsLink.SelectedItem as FKItem;

                if (link != null)
                {
                    var next_link = FieldsLinkList
                        .Where(x => x.FKOrder > link.FKOrder)
                        .OrderBy(x => x.FKOrder)
                        .FirstOrDefault();

                    if (next_link != null)
                    {
                        // есть предыдущая строка
                        var next_order = next_link.FKOrder;
                        next_link.FKOrder = link.FKOrder;
                        link.FKOrder = next_order;
                    }
                }

                dgFieldsLinkRefresh();
            }
        }

        private void ViewFK(bool isEnabled)
        {
            tbFKTable.IsEnabled = isEnabled;
            btChooseFKTable.IsEnabled = isEnabled;
            tbCHECK.IsEnabled = !isEnabled;
        }

        private void ViewCHECK(bool isEnabled)
        {
            tbFKTable.IsEnabled = !isEnabled;
            btChooseFKTable.IsEnabled = !isEnabled;
            tbCHECK.IsEnabled = isEnabled;
        }

        /// <summary>
        /// Выбор "Прочие FK"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void rbFKOther_Checked(object sender, RoutedEventArgs e)
        {
            ViewFK(true);

            tbFKTable.Text = "";
            FKTableChanged();
            tbCHECK.Text = "";
        }

        private void rbFKOther_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewFK(false);
        }

        /// <summary>
        /// Выбор "CHECK"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void rbCHECK_Checked(object sender, RoutedEventArgs e)
        {
            ViewCHECK(true);
            tbFKTable.Text = "";
            FKTableChanged();
            FKNameRefresh();
        }

        /// <summary>
        /// Снят выбор "CHECK"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rbCHECK_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewCHECK(false);
        }

        // выход из поля "Таблица FOREIGN KEY"
        private void tbFKTable_LostFocus(object sender, RoutedEventArgs e)
        {
            FKTableChanged();
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            if (winTable != null)
            {
                foreach (var link in FieldsLinkList.OrderBy(x => x.FKOrder))
                {
                    var row = winTable.Table.TableEdit.ListField
                        .Where(x => x.FieldNameCompare == link.FieldName.ToLower())
                        .FirstOrDefault();

                    if (row != null)
                    {
                        if (
                            (!string.IsNullOrWhiteSpace(tbFKTable.Text)) &&
                            (!string.IsNullOrWhiteSpace(link.FKField))
                        )
                        {
                            row.FKName = tbFKName.Text;
                            row.FKTable = tbFKTable.Text;
                            row.FKField = link.FKField;
                            if (link.FKOrder > 0)
                            {
                                row.FKOrder = link.FKOrder.ToString();
                            }
                            else
                            {
                                row.FKOrder = "";
                            }
                            row.FieldCheck = "";
                        }

                        if (!string.IsNullOrWhiteSpace(tbCHECK.Text))
                        {
                            row.FKName = tbFKName.Text;
                            row.FKTable = "";
                            row.FKField = "";
                            row.FKOrder = "";
                            row.FieldCheck = tbCHECK.Text;
                        }
                    }
                }

                winTable.dgFieldsRefresh();
            }

            this.Close();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void dgFieldsLink_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            /*
            e.Handled = true;

            btLeft_Click(null, null);
            */
        }

        /// <summary>
        /// После выбора поля primary key таблицы внешнего ключа
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FKField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FKNameRefresh();
        }

        /// <summary>
        /// После выбора поля таблицы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FieldName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FKNameRefresh();
        }

        private void tbCHECK_LostFocus(object sender, RoutedEventArgs e)
        {
            FKNameRefresh();
        }

        private void dgFieldsLink_LostFocus(object sender, RoutedEventArgs e)
        {
            FKNameRefresh();
        }

        private void dgFieldsLink_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FKNameRefresh();
        }
    }

    /// <summary>
    /// Класс для хранения Foreign Key
    /// </summary>
    public class FKItem : INotifyPropertyChanged
    {
        /// <summary>
        /// реализация NotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// реализация NotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName">propertyName</param>
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); //-V3083
            }
        }

        private WinAddFK winAddFK;

        /// <summary>
        /// конструктор класса FKItem
        /// </summary>
        /// <param name="_winAddFK"></param>
        public FKItem(WinAddFK _winAddFK)
        {
            this.winAddFK = _winAddFK;
        }


        private string _fieldname;
        /// <summary>
        /// Имя поля внешнего ключа
        /// </summary>
        public string FieldName
        {
            get
            {
                return _fieldname ?? "";
            }
            set
            {
                string newvalue = value;
                if (string.IsNullOrWhiteSpace(newvalue)) newvalue = "";
                newvalue = newvalue.Trim();

                if ((_fieldname ?? "") != newvalue)
                {
                    _fieldname = newvalue;

                    NotifyPropertyChanged("FieldName");

                    if (winAddFK != null)
                    {
                        winAddFK.FKNameRefresh();
                    }
                }
            }
        }

        private string _fkfield;
        /// <summary>
        /// PK таблицы внешнего ключа
        /// </summary>
        public string FKField
        {
            get
            {
                return _fkfield ?? "";
            }
            set
            {
                string newvalue = value;
                if (string.IsNullOrWhiteSpace(newvalue)) newvalue = "";
                newvalue = newvalue.Trim();

                if ( (_fkfield ?? "") != newvalue)
                {
                    _fkfield = newvalue;

                    NotifyPropertyChanged("FKField");

                    if (winAddFK != null)
                    {
                        winAddFK.FKNameRefresh();
                    }
                }
            }
        }

        /// <summary>
        /// Номер по порядку
        /// </summary>
        public int FKOrder { get; set; }
    }

    /// <summary>
    /// Класс для хранения имен полей
    /// </summary>
    public class FieldToList
    {
        /// <summary>
        /// Номер по порядку
        /// </summary>
        public int Order { get; set; }
        /// <summary>
        /// имя поля
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Наименование для отображения
        /// </summary>
        public string DisplayName
        {
            get
            {
                return Order.ToString() + " - " + Name; 
            }
        }
    }
}
