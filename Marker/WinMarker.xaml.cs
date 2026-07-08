// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using Newtonsoft.Json.Linq;
using SQLGen.Controls;
using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using static SQLGen.Utilities.External;

namespace SQLGen
{
    /// <summary>
    /// Окно для сборки скриптов для маркеров
    /// </summary>
    public partial class WinMarker : Window
    {
        /// <summary>Подключение к БД</summary>
        public ConnectDB ConnectSQL = null;

        /// <summary>Флаг инициализации окна</summary>
        public bool isStart = true;

        /// <summary>Список маркеров</summary>
        public List<FreeDocMarker> ListFreeDocMarker;

        /// <summary>Список связей маркеров</summary>
        public List<FreeDocRelationship> ListFreeDocRelationship;

        /// <summary>Список EvnClass</summary>
        public List<EvnClass> ListEvnClass;

        /// <summary>проект GIT</summary>
        public string GITProject { get; set; }

        /// <summary>тип целевой БД</summary>
        public Utilities.ConnType ConnType
        {
            get
            {
                return ConnectDB.GetConnTypeByProject(this.GITProject);
            }
        }

        /// <summary>целевая БД</summary>
        public Utilities.TargetDBType TargetDB
        {
            get
            {
                return ConnectDB.GetTargetDBTypeByProject(this.GITProject);
            }
        }

        /// <summary>префикс для использования в имени файла</summary>
        public string PrefixToFilename
        {
            get
            {
                return Utilities.GITProjects.GetPrefixFileSQLByProject(this.GITProject);
            }
        }

        /// <summary>имя файла со скриптом</summary>
        public string ScriptFilename (int _num, string _table)
        {
            return $"{this.PrefixToFilename}  {MainWindow.Task.TaskNumberToFilename}  {_num.ToString()} marker dbo {_table}.sql";
        }

        /// <summary>Конструктор WinMarker</summary>
        public WinMarker()
        {
            InitializeComponent();

            // загрузить историю
            tbFilter.InitHistory("HistoryMarker.json", "");

            // пользовательские настройки GUI
            Default.InitGUI(
                "WinMarker",
                this,
                mainGrid,
                new List<System.Windows.Controls.Control> { tbScript },
                new List<ComboBox> { cbScriptFontFamily },
                new List<ComboBox> { cbScriptFontSize },
                MainWindow.Task.LogFile
                );

            // заполняем toolbar'ы
            tbScript.AddToolbarDefault(toolbarScript, tiScript, true, false);
        }

        /// <summary>При открытии окна WinMarker</summary>
        private void winMarker_Activated(object sender, EventArgs e)
        {
            this.Title = "Спецмаркеры, задача " + MainWindow.Task.TaskNumber;
        }

        /// <summary>При закрытии окна WinMarker</summary>
        private void winMarker_Closed(object sender, EventArgs e)
        {
            if (ConnectSQL != null)
            {
                ConnectSQL.CloseConnect();
            }

            // пользовательские настройки GUI
            Default.SaveGUI(
                "WinMarker",
                this,
                new List<System.Windows.Controls.Control> { tbScript }
                );
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>
        /// Переоткрытие основного подключения формы
        /// </summary>
        /// <returns>=true - успешное переподключение</returns>
        private bool CheckConnectSQL()
        {
            if (ConnectSQL == null)
            {
                ConnectSQL = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, false);
            }

            if (ConnectSQL.isNotConnected)
            {
                ConnectSQL.ReConnect();
            }

            return ConnectSQL.isConnected;
        }

        /// <summary>
        /// Нажата кнопка Обновить
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void btRefresh_Click(object sender, RoutedEventArgs e)
        {
            this.Cursor = Cursors.Wait;

            cbAllRel.IsChecked = false;
            cbChanged.IsChecked = false;
            cbError.IsChecked = false;

            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
            }
            else
            {
                try
                {
                    ConnectSQL.FillFreeDocMarkers(ref ListFreeDocMarker, ref ListFreeDocRelationship, ref ListEvnClass);

                    dgFreeDocMarkers.ItemsSource = ListFreeDocMarker;

                    dgFreeDocMarkersRefresh();
                }
                catch (Exception ex)
                {
                    this.Cursor = Cursors.Arrow;
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Фильтрация маркеров
        /// </summary>
        /// <param name="de">объект</param>
        /// <returns></returns>
        public bool MarkerContains(object de)
        {
            FreeDocMarker marker = de as FreeDocMarker;
            return (marker.isFiltered == true) &&
                   ((cbChanged.IsChecked == false) || (marker.isChanged == true) || (marker.isChangedRelation == true)) &&
                   ((cbError.IsChecked == false) || (marker.isError == true));
        }

        /// <summary>Обновить список полей</summary>
        private void dgFreeDocMarkersRefresh()
        {
            this.Cursor = Cursors.Wait;

            string filter = tbFilter.Text.Trim().ToLower();
            bool isFilteredAll = string.IsNullOrWhiteSpace(filter);

            foreach (var marker in ListFreeDocMarker)
            {
                marker.isFiltered = isFilteredAll;
                marker.isError = false;
                marker.isChangedRelation = false;

                // проверяем маркеры на ошибки
                if (cbError.IsChecked == true)
                {
                    // дублирование по имени
                    foreach (var chk in ListFreeDocMarker.Where(x => x.FreeDocMarker_id != marker.FreeDocMarker_id))
                    {
                        if ((chk.FreeDocMarker_Name.ToLower() == marker.FreeDocMarker_Name.ToLower()) &&
                            (chk.EvnClass_SysNick.ToLower() == marker.EvnClass_SysNick.ToLower())
                            )
                        {
                            chk.isError = true;
                            marker.isError = true;
                        }
                    }

                    // Отсутствие связей
                    if ((!string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias)) &&
                        (ListFreeDocRelationship.Where(x => (x.FreeDocRelationship_AliasName == marker.FreeDocMarker_TableAlias) && EvnClassLinked(x.EvnClass_SysNick, marker.EvnClass_SysNick)).ToList().Count == 0)
                        )
                    {
                        marker.isError = true;
                    }
                }

                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var arr_filter = filter.Split(' ');

                    // фильтрация маркеров
                    for (int i = 0; i < arr_filter.Length; i++)
                    {
                        string s_filter = arr_filter[i].Trim();

                        if (marker.FreeDocMarker_id.ToLower().Contains(s_filter) ||
                            marker.FreeDocMarker_Name.ToLower().Contains(s_filter) ||
                            marker.FreeDocMarker_TableAlias.ToLower().Contains(s_filter) || //-V3125 //-V3095
                            marker.FreeDocMarker_Field.ToLower().Contains(s_filter) ||
                            marker.FreeDocMarker_Query.ToLower().Contains(s_filter) ||
                            marker.FreeDocMarker_Description.ToLower().Contains(s_filter) ||
                            marker.FreeDocMarker_Options.ToLower().Contains(s_filter) ||
                            marker.EvnClass_SysNick.ToLower().Contains(s_filter)
                        )
                        {
                            marker.isFiltered = true;
                        }

                        if (!string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias))
                        {
                            foreach (var rel in ListFreeDocRelationship.Where(x => (x.FreeDocRelationship_AliasName == marker.FreeDocMarker_TableAlias) && EvnClassLinked(x.EvnClass_SysNick, marker.EvnClass_SysNick)))
                            {
                                if (rel.FreeDocRelationship_id.ToLower().Contains(s_filter) ||
                                    rel.FreeDocRelationship_AliasTable.ToLower().Contains(s_filter) ||
                                    rel.FreeDocRelationship_AliasQuery.ToLower().Contains(s_filter) ||
                                    rel.FreeDocRelationship_LinkedAlias.ToLower().Contains(s_filter) ||
                                    rel.FreeDocRelationship_LinkDescription.ToLower().Contains(s_filter) ||
                                    rel.EvnClass_SysNick.ToLower().Contains(s_filter)
                                )
                                {
                                    marker.isFiltered = true;
                                }
                            }
                        }
                    }
                }
            }

            // проверяем связи на ошибки
            if (cbError.IsChecked == true)
            {
                foreach (var rel in ListFreeDocRelationship)
                {
                    rel.isError = false;

                    // дублирование по имени
                    foreach (var chk in ListFreeDocRelationship.Where(x => x.FreeDocRelationship_id != rel.FreeDocRelationship_id))
                    {
                        if ((chk.FreeDocRelationship_AliasName.ToLower() == rel.FreeDocRelationship_AliasName.ToLower()) &&
                            (chk.EvnClass_SysNick.ToLower() == rel.EvnClass_SysNick.ToLower())
                            )
                        {
                            chk.isError = true;
                            rel.isError = true;
                        }
                    }

                    // Отсутствие связей
                    if ((!string.IsNullOrWhiteSpace(rel.FreeDocRelationship_LinkedAlias)) &&
                        (ListFreeDocRelationship.Where(x => (x.FreeDocRelationship_AliasName == rel.FreeDocRelationship_LinkedAlias) && EvnClassLinked(x.EvnClass_SysNick, rel.EvnClass_SysNick)).ToList().Count == 0)
                        )
                    {
                        rel.isError = true;
                    }
                }

                // если есть ошибка в связях
                foreach (var rel in ListFreeDocRelationship.Where(x => x.isError == true))
                {
                    foreach (var marker in ListFreeDocMarker.Where(x => (x.FreeDocMarker_TableAlias == rel.FreeDocRelationship_AliasName) && EvnClassLinked(x.EvnClass_SysNick, rel.EvnClass_SysNick)))
                    {
                        marker.isError = true;
                    }
                }
            }

            if (cbChanged.IsChecked == true)
            {
                // если изменилась связь
                foreach (var rel in ListFreeDocRelationship.Where(x => x.isChanged == true))
                {
                    foreach (var marker in ListFreeDocMarker.Where(x => (x.FreeDocMarker_TableAlias == rel.FreeDocRelationship_AliasName) && EvnClassLinked(x.EvnClass_SysNick, rel.EvnClass_SysNick)))
                    {
                        marker.isChangedRelation = true;
                    }
                }
            }

            ListCollectionView cvTasks = (ListCollectionView)CollectionViewSource.GetDefaultView(dgFreeDocMarkers.ItemsSource);

            if (cvTasks != null && cvTasks.CanFilter == true)
            {
                cvTasks.Filter = new Predicate<object>(MarkerContains);
            }

            dgFreeDocMarkers.Items.Refresh();
            dgFreeDocMarkersSelect();

            this.Cursor = Cursors.Arrow;
        }

        /// <summary>
        /// Фильтрация связей
        /// </summary>
        /// <param name="de">объект</param>
        /// <returns></returns>
        public bool RelationshipContains(object de)
        {
            FreeDocRelationship rel = de as FreeDocRelationship;
            return (rel.Order != 0) &&
                ((cbChanged.IsChecked == false) || (rel.isChanged == true)) &&
                ((cbError.IsChecked == false) || (rel.isError == true));
        }

        /// <summary>Обновить список связей</summary>
        private void dgFreeDocRelationshipsRefresh()
        {
            ListCollectionView cvTasks = (ListCollectionView)CollectionViewSource.GetDefaultView(dgFreeDocRelationships.ItemsSource);

            if (cvTasks != null && cvTasks.CanFilter == true)
            {
                cvTasks.Filter = new Predicate<object>(RelationshipContains);
            }

            if (cvTasks != null && cvTasks.CanSort == true)
            {
                cvTasks.SortDescriptions.Clear();
                cvTasks.SortDescriptions.Add(new SortDescription("Order", ListSortDirection.Ascending));
            }

            dgFreeDocRelationships.Items.Refresh();
        }


        /// <summary>Проверить, что evnclass1 и evnclass2 связаны друг с другом (либо равны, либо потомок-предок, либо предок-потомок)</summary>
        private bool EvnClassLinked(string evnclass1, string evnclass2)
        {
            if (evnclass1 == evnclass2) return true;

            foreach (var item in ListEvnClass.Where(x => x.ParentSysNick == evnclass2))
            {
                if (EvnClassLinked(evnclass1, item.SysNick)) return true;
            }

            foreach (var item in ListEvnClass.Where(x => x.ParentSysNick == evnclass1))
            {
                if (EvnClassLinked(evnclass2, item.SysNick)) return true;
            }

            return false;
        }

        /// <summary>Проверить, что evnclass_child - это потомок у evnclass_parent или тот же самый</summary>
        private bool EvnClassParent(string evnclass_child, string evnclass_parent)
        {
            if (evnclass_child == evnclass_parent) return true;

            foreach (var item in ListEvnClass.Where(x => x.ParentSysNick == evnclass_parent))
            {
                if (EvnClassParent(evnclass_child, item.SysNick)) return true;
            }

            return false;
        }


        /// <summary>Проставить порядок для связанных связей (либо равны, либо потомок-предок, либо предок-потомок)</summary>
        private void SetOrderLinked(List<FreeDocRelationship> list, int order, string sysnick, string alias)
        {
            foreach (var item in list.Where(x => (x.FreeDocRelationship_AliasName == alias) && EvnClassLinked(sysnick, x.EvnClass_SysNick)))
            {
                order--;
                item.Order = order;

                if ((!string.IsNullOrWhiteSpace(item.FreeDocRelationship_LinkedAlias)) && (item.FreeDocRelationship_LinkedAlias != item.FreeDocRelationship_AliasName))
                {
                    SetOrderLinked(list, order, sysnick, item.FreeDocRelationship_LinkedAlias);
                }
            }
        }


        /// <summary>Проставить порядок для связанных связей (потомок-предок)</summary>
        private void SetOrderParent(List<FreeDocRelationship> list, int order, string sysnick, string alias)
        {
            foreach (var item in list.Where(x => (x.FreeDocRelationship_AliasName == alias) && EvnClassParent(sysnick, x.EvnClass_SysNick)))
            {
                order--;
                item.Order = order;

                if ((!string.IsNullOrWhiteSpace(item.FreeDocRelationship_LinkedAlias)) && (item.FreeDocRelationship_LinkedAlias != item.FreeDocRelationship_AliasName))
                {
                    SetOrderParent(list, order, sysnick, item.FreeDocRelationship_LinkedAlias);
                }
            }
        }

        /// <summary>Проставить порядок для точных связей</summary>
        private void SetOrder(List<FreeDocRelationship> list, int order, string sysnick, string alias)
        {
            foreach (var item in list.Where(x => (x.FreeDocRelationship_AliasName == alias) && (sysnick == x.EvnClass_SysNick)))
            {
                order--;
                item.Order = order;

                if ((!string.IsNullOrWhiteSpace(item.FreeDocRelationship_LinkedAlias)) && (item.FreeDocRelationship_LinkedAlias != item.FreeDocRelationship_AliasName))
                {
                    SetOrder(list, order, sysnick, item.FreeDocRelationship_LinkedAlias);
                }
            }
        }

        /// <summary>Выбрать маркер</summary>
        private void dgFreeDocMarkersSelect()
        {
            if ((cbAllRel.IsChecked == false) && (dgFreeDocMarkers.SelectedIndex >= 0))
            {
                var marker = dgFreeDocMarkers.SelectedItem as FreeDocMarker;

                foreach (var item in ListFreeDocRelationship) item.Order = 0;

                int order = 0;

                if (!string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias))
                {
                    SetOrderLinked(ListFreeDocRelationship, order, marker.EvnClass_SysNick, marker.FreeDocMarker_TableAlias);
                }

                dgFreeDocRelationships.ItemsSource = ListFreeDocRelationship;
                dgFreeDocRelationshipsRefresh();
            }

            if (cbAllRel.IsChecked == true)
            {
                int cnt = 0;
                foreach (var item in ListFreeDocRelationship.OrderBy(x => x.FreeDocRelationship_AliasName))
                {
                    cnt++;
                    item.Order = cnt;
                }

                dgFreeDocRelationshipsRefresh();
            }
        }


        /// <summary>Выбрать маркер</summary>
        private void dgFreeDocMarkers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgFreeDocMarkersSelect();
        }

        /// <summary>Выбран регион</summary>
        private void cbRegion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbRegion.SelectedIndex != -1) isRegion.IsChecked = true;
        }

        /// <summary>
        /// изменилось подключение к БД
        /// </summary>
        public void cbConnectSQLChanged()
        {
            if (ConnectSQL != null)
            {
                ConnectSQL.CloseConnect();
            }

            ConnectSQL = Utilities.Controls.SetConnectFromComboBox(cbConnectSQL);

            if (ConnectSQL != null) //-V3022
            {
                Utilities.Controls.RefreshGITProjectItems(cbGITProject, ConnectSQL.GITProject, ConnectSQL.ConnType, true);
                cbGITProjectChanged();
            }
        }

        /// <summary>Выбрано подключение на вкладке Script</summary>
        private void cbConnectSQL_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isStart) cbConnectSQLChanged();
        }

        /// <summary>Выбрано подключение на вкладке Script</summary>
        private void cbConnect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        /// <summary>Изменился проект GIT</summary>
        private void cbGITProjectChanged()
        {
            ComboBoxItem cbItem = null;
            if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
            if ((cbItem == null) || (cbItem.Tag == null)) this.GITProject = "";
            else this.GITProject = cbItem.Tag.ToString();

            string connname = "";
            if ((cbConnectSQL != null) && (cbConnectSQL.SelectedItem != null))
            {
                cbItem = (ComboBoxItem)cbConnectSQL.SelectedItem;
                connname = cbItem.Content.ToString();
            }

            Utilities.Controls.RefreshConnectItems(cbConnect, connname, null, this.ConnType);
            Utilities.Controls.SetComboBoxConnectByDefaultProject(cbConnect, this.GITProject);
        }

        /// <summary>Выбран проект GIT</summary>
        private void cbGITProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbGITProjectChanged();
        }


        /// <summary>Добавить или изменить запись в FreeDocMarker</summary>
        private bool AddEditMarker(MarkerOperType oper, ref FreeDocMarker marker)
        {
            bool result = false;

            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                return result;
            }

            try
            {
                FormAddMarker dlg1 = new FormAddMarker(this);

                if ((marker == null) || (oper == MarkerOperType.ADD))
                {
                    var generator = new RandomGenerator();

                    marker = new FreeDocMarker(
                         "новый_" + generator.RandomNumber(1, 1000).ToString(),
                         "Evn",
                         "новый",
                         "",
                         "",
                         "",
                         "",
                         "",
                         ""
                        );
                    dlg1.Text = "Добавить маркер";
                }
                else if (oper == MarkerOperType.EDIT)
                {
                    dlg1.Text = "Изменить маркер";
                }
                else
                {
                    var generator = new RandomGenerator();

                    dlg1.Text = "Клонировать маркер";
                    marker.FreeDocMarker_id = "новый_" + generator.RandomNumber(1, 1000).ToString();
                    marker.FreeDocMarker_Name = marker.FreeDocMarker_Name + "_новый";
                }

                Utilities.Controls.RefreshComboBoxItems(dlg1.EvnClass_SysNick, ConnectSQL, "select EvnClass_SysNick from dbo.EvnClass order by EvnClass_SysNick");

                dlg1.FreeDocMarker_id.Text = marker.FreeDocMarker_id;
                dlg1.EvnClass_SysNick.SelectedItem = marker.EvnClass_SysNick;
                dlg1.FreeDocMarker_Name.Text = marker.FreeDocMarker_Name;
                dlg1.FreeDocMarker_TableAlias.Text = marker.FreeDocMarker_TableAlias;
                dlg1.FreeDocMarker_Field.Text = marker.FreeDocMarker_Field;
                dlg1.FreeDocMarker_Query.Text = marker.FreeDocMarker_Query;
                dlg1.FreeDocMarker_Description.Text = marker.FreeDocMarker_Description;
                dlg1.FreeDocMarker_IsTableValue.Checked = marker.FreeDocMarker_IsTableValue == true;
                dlg1.FreeDocMarker_Options.Text = marker.FreeDocMarker_Options;

                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    marker.FreeDocMarker_id = dlg1.FreeDocMarker_id.Text;
                    marker.EvnClass_SysNick = dlg1.EvnClass_SysNick.SelectedItem.ToString();
                    marker.FreeDocMarker_Name = dlg1.FreeDocMarker_Name.Text;

                    if ((oper == MarkerOperType.EDIT) && (marker.FreeDocMarker_TableAlias != dlg1.FreeDocMarker_TableAlias.Text.Trim())) //-V3095
                    {
                        if ((!string.IsNullOrWhiteSpace(dlg1.FreeDocMarker_TableAlias.Text)) &&
                            (!string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias))
                            )
                        {
                            // если при редактировании поменялся алиас - сменим его и в связях
                            string tablealias = marker.FreeDocMarker_TableAlias;
                            string sysnick = marker.EvnClass_SysNick;
                            foreach (var rel in ListFreeDocRelationship.Where(x => (x.FreeDocRelationship_AliasName == tablealias) && EvnClassLinked(x.EvnClass_SysNick, sysnick)))
                            {
                                rel.FreeDocRelationship_AliasName = dlg1.FreeDocMarker_TableAlias.Text;
                            }
                        }

                        marker.FreeDocMarker_TableAlias = dlg1.FreeDocMarker_TableAlias.Text;
                    }

                    marker.FreeDocMarker_Field = dlg1.FreeDocMarker_Field.Text;
                    marker.FreeDocMarker_Query = dlg1.FreeDocMarker_Query.Text;
                    marker.FreeDocMarker_Description = dlg1.FreeDocMarker_Description.Text;
                    marker.FreeDocMarker_IsTableValue = dlg1.FreeDocMarker_IsTableValue.Checked == true;
                    marker.FreeDocMarker_Options = dlg1.FreeDocMarker_Options.Text;
                    result = true;

                }
                else result = false;
                dlg1.Dispose();
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                result = false;
            }

            return result;
        }


        /// <summary>Добавление маркера</summary>
        private void AddMarker_Click(object sender, RoutedEventArgs e)
        {
            FreeDocMarker marker = null;

            if (AddEditMarker(MarkerOperType.ADD, ref marker) && (marker != null))
            {
                ListFreeDocMarker.Add(marker);

                dgFreeDocMarkersRefresh();

                dgFreeDocMarkers.SelectedItem = marker;
                if (dgFreeDocMarkers.SelectedItem != null) //-V3022
                {
                    dgFreeDocMarkers.UpdateLayout();
                    dgFreeDocMarkers.ScrollIntoView(dgFreeDocMarkers.SelectedItem);
                }
            }
        }

        /// <summary>Изменение маркера</summary>
        private void EditMarker_Click(object sender, RoutedEventArgs e)
        {
            if (dgFreeDocMarkers.SelectedIndex >= 0)
            {
                FreeDocMarker marker = dgFreeDocMarkers.SelectedItem as FreeDocMarker;

                if (AddEditMarker(MarkerOperType.EDIT, ref marker) && (marker != null))
                {
                    dgFreeDocMarkersRefresh();

                    dgFreeDocMarkers.SelectedItem = marker;
                    if (dgFreeDocMarkers.SelectedItem != null) //-V3022
                    {
                        dgFreeDocMarkers.UpdateLayout();
                        dgFreeDocMarkers.ScrollIntoView(dgFreeDocMarkers.SelectedItem);
                    }

                    dgFreeDocRelationshipsRefresh();
                }

            }
        }


        /// <summary>Вернуть список связей маркера для выбанного класса события</summary>
        private List<FreeDocRelationship> GetRelations(string sysnick, string alias)
        {
            List<FreeDocRelationship> list = new List<FreeDocRelationship>();
            foreach (var item in ListFreeDocRelationship)
            {
                list.Add(item.Copy());
            }

            foreach (var item in list) item.Order = 0;

            int order = 0;

            if (!string.IsNullOrWhiteSpace(alias))
            {
                foreach (var item in list.Where(x => (x.FreeDocRelationship_AliasName == alias) && EvnClassParent(sysnick, x.EvnClass_SysNick)))
                {
                    order--;
                    item.Order = order;

                    if ((!string.IsNullOrWhiteSpace(item.FreeDocRelationship_LinkedAlias)) && (item.FreeDocRelationship_LinkedAlias != item.FreeDocRelationship_AliasName))
                    {
                        SetOrder(list, order, sysnick, item.FreeDocRelationship_LinkedAlias);
                    }
                }
            }

            List<FreeDocRelationship> result = new List<FreeDocRelationship>();
            foreach (var item in list.Where(x => x.Order != 0).OrderBy(x => x.Order))
            {
                result.Add(item.Copy());
            }

            return result;
        }

        private string GenerateQuery(string sysnick, FreeDocMarker marker)
        {
            string result = "";

            if (string.IsNullOrEmpty(sysnick)) return result;
            if (marker == null) return result;
            if (string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias) && string.IsNullOrWhiteSpace(marker.FreeDocMarker_Field) && string.IsNullOrWhiteSpace(marker.FreeDocMarker_Query))
                return result;
            if ((!string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias)) && string.IsNullOrWhiteSpace(marker.FreeDocMarker_Field) && string.IsNullOrWhiteSpace(marker.FreeDocMarker_Query))
                return result;

            // связи
            string links = "";
            if (!string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias))
            {
                List<FreeDocRelationship> list = GetRelations(sysnick, marker.FreeDocMarker_TableAlias);

                foreach (FreeDocRelationship rel in list)
                {
                    if (!string.IsNullOrWhiteSpace(rel.FreeDocRelationship_AliasTable))
                    {
                        links += Environment.NewLine + "LEFT JOIN " + rel.FreeDocRelationship_AliasTable + " as " + rel.FreeDocRelationship_AliasName + " on " + rel.FreeDocRelationship_LinkDescription;
                    }
                    else
                    {
                        string sql = rel.FreeDocRelationship_AliasQuery;

                        sql = sql.Replace("{roottable_name}", sysnick);
                        sql = sql.Replace("{ROOTTABLE_NAME}", sysnick);

                        if (this.ConnType == Utilities.ConnType.PGSQL)
                        {
                            links += Environment.NewLine + "LEFT JOIN LATERAL (" + sql + ") " + rel.FreeDocRelationship_AliasName + " on true";
                        }
                        else
                        {
                            links += Environment.NewLine + "OUTER APPLY (" + sql + ") " + rel.FreeDocRelationship_AliasName;
                        }
                    }
                }
            }


            // алиас и поле
            if ((!string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias)) && (!string.IsNullOrWhiteSpace(marker.FreeDocMarker_Field)))
            {
                result += Environment.NewLine + "SELECT ";
                result += marker.FreeDocMarker_TableAlias + "." + marker.FreeDocMarker_Field + " as \"MarkerData_" + marker.FreeDocMarker_id + "\"";
                result += Environment.NewLine + "FROM dbo.v_" + sysnick + " as RootTable";
                if (!string.IsNullOrWhiteSpace(links))
                {
                    result += links;
                }
                result += Environment.NewLine + "WHERE RootTable." + sysnick + "_id = {evn_id}";
            }
            else 
            // запрос
            if ((string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias)) && (string.IsNullOrWhiteSpace(marker.FreeDocMarker_Field)) && (!string.IsNullOrWhiteSpace(marker.FreeDocMarker_Query)))
            {
                result += Environment.NewLine + "SELECT ";
                result += '(' + marker.FreeDocMarker_Query + ") as \"MarkerData_" + marker.FreeDocMarker_id + "\"";
                result += Environment.NewLine + "FROM dbo.v_" + sysnick + " as RootTable";
                result += Environment.NewLine + "WHERE RootTable." + sysnick + "_id = {evn_id}";
            }
            else
            // алиас и запрос
            if ((!string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias)) && (string.IsNullOrWhiteSpace(marker.FreeDocMarker_Field)) && (!string.IsNullOrWhiteSpace(marker.FreeDocMarker_Query)))
            {
                result += Environment.NewLine + "SELECT ";
                result += '(' + marker.FreeDocMarker_Query + ") as \"MarkerData_" + marker.FreeDocMarker_id + "\"";
                result += Environment.NewLine + "FROM dbo.v_" + sysnick + " as RootTable";
                if (!string.IsNullOrWhiteSpace(links))
                {
                    result += links;
                }
                result += Environment.NewLine + "WHERE RootTable." + sysnick + "_id = {evn_id}";
            }
            else
            // поле
            if ((string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias)) && (!string.IsNullOrWhiteSpace(marker.FreeDocMarker_Field)) && (string.IsNullOrWhiteSpace(marker.FreeDocMarker_Query)))
            {
                result += Environment.NewLine + "SELECT ";
                result += marker.FreeDocMarker_Field + " as \"MarkerData_" + marker.FreeDocMarker_id + "\"";
                result += Environment.NewLine + "FROM dbo.v_" + sysnick + " as RootTable";
                result += Environment.NewLine + "WHERE RootTable." + sysnick + "_id = {evn_id}";
            }

            if (
                (!string.IsNullOrWhiteSpace(result)) &&
                ConnectSQL.GetObjectList(
                    "dbo.p_freedocmarker_check",
                    true, // точный поиск
                    false,
                    false,
                    false,
                    true, // ищем хранимку
                    false,
                    false,
                    false,
                    false
                    ).Count() > 0
                )

            {
                if (this.ConnType == Utilities.ConnType.MSSQL)
                {
                    result = "EXEC dbo.p_FreeDocMarker_check @query = '" + result.Replace("'", "''") + Environment.NewLine + "'";
                }
                else
                {
                    result = "SELECT rows_json FROM dbo.p_freedocmarker_check(query := $marker$" + result + Environment.NewLine + "$marker$);";
                }
            }

            return result;
        }

        /// <summary>Собрать запрос маркера</summary>
        private void QueryMarker_Click(object sender, RoutedEventArgs e)
        {
            if (dgFreeDocMarkers.SelectedIndex >= 0)
            {
                FreeDocMarker marker = dgFreeDocMarkers.SelectedItem as FreeDocMarker;

                try
                {
                    FormEvnClass dlg1 = new FormEvnClass();

                    dlg1.EvnClass_SysNick.Items.Clear();

                    foreach (var item in ListEvnClass.Where(x => EvnClassParent(x.SysNick, marker.EvnClass_SysNick)).OrderBy(x => x.SysNick))
                    {
                        if (!dlg1.EvnClass_SysNick.Items.Contains(item.SysNick))
                            dlg1.EvnClass_SysNick.Items.Add(item.SysNick);
                    }

                    dlg1.EvnClass_SysNick.SelectedItem = marker.EvnClass_SysNick;

                    if ((dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK) && (dlg1.EvnClass_SysNick.SelectedIndex != -1))
                    {
                        tbScript.Text = GenerateQuery(dlg1.EvnClass_SysNick.SelectedItem.ToString(), marker);
                        tbScript.Filename = "";
                        tiScript.IsSelected = true;
                    }
                    dlg1.Dispose();
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }
        }

        /// <summary>Добавить или изменить запись в FreeDocRelationship</summary>
        private bool AddEditRelationship(string alias, ref FreeDocRelationship rel)
        {
            bool result = false;

            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                return result;
            }

            bool isAdd = rel == null;

            try
            {

                FormAddRelationship dlg1 = new FormAddRelationship(this);

                if (isAdd)
                {
                    var generator = new RandomGenerator();

                    rel = new FreeDocRelationship(
                         "новый_" + generator.RandomNumber(1, 1000).ToString(),
                         "Evn",
                         alias,
                         "",
                         "",
                         "",
                         ""
                        );
                    dlg1.Text = "Добавить связь маркера и источника данных";
                }
                else
                {
                    dlg1.Text = "Изменить связь маркера и источника данных";
                    dlg1.FreeDocRelationship_AliasName.ReadOnly = true;
                }

                Utilities.Controls.RefreshComboBoxItems(dlg1.EvnClass_SysNick, ConnectSQL, "select EvnClass_SysNick from dbo.EvnClass order by EvnClass_SysNick");

                dlg1.FreeDocRelationship_id.Text = rel.FreeDocRelationship_id;
                dlg1.EvnClass_SysNick.SelectedItem = rel.EvnClass_SysNick;
                dlg1.FreeDocRelationship_AliasName.Text = rel.FreeDocRelationship_AliasName;
                dlg1.FreeDocRelationship_AliasTable.Text = rel.FreeDocRelationship_AliasTable;
                dlg1.FreeDocRelationship_LinkedAlias.Text = rel.FreeDocRelationship_LinkedAlias;
                dlg1.FreeDocRelationship_AliasQuery.Text = rel.FreeDocRelationship_AliasQuery;
                dlg1.FreeDocRelationship_LinkDescription.Text = rel.FreeDocRelationship_LinkDescription;

                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {

                    rel.FreeDocRelationship_id = dlg1.FreeDocRelationship_id.Text;
                    rel.EvnClass_SysNick = dlg1.EvnClass_SysNick.SelectedItem.ToString();
                    rel.FreeDocRelationship_AliasName = dlg1.FreeDocRelationship_AliasName.Text;
                    rel.FreeDocRelationship_AliasTable = dlg1.FreeDocRelationship_AliasTable.Text;
                    rel.FreeDocRelationship_LinkedAlias = dlg1.FreeDocRelationship_LinkedAlias.Text;
                    rel.FreeDocRelationship_AliasQuery = dlg1.FreeDocRelationship_AliasQuery.Text;
                    rel.FreeDocRelationship_LinkDescription = dlg1.FreeDocRelationship_LinkDescription.Text;
                    result = true;

                }
                dlg1.Dispose();

            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                result = false;
            }

            return result;
        }

        /// <summary>Добавление связи маркера</summary>
        private void AddRelation_Click(object sender, RoutedEventArgs e)
        {
            if (dgFreeDocMarkers.SelectedIndex >= 0)
            {
                FreeDocMarker marker = dgFreeDocMarkers.SelectedItem as FreeDocMarker;
                FreeDocRelationship rel = null;

                if (AddEditRelationship(marker.FreeDocMarker_TableAlias, ref rel) && (rel != null)) //-V3095
                {
                    ListFreeDocRelationship.Add(rel);

                    dgFreeDocMarkersRefresh();
                    dgFreeDocMarkers.SelectedItem = marker;
                    if (dgFreeDocMarkers.SelectedItem != null) //-V3022
                    {
                        dgFreeDocMarkers.UpdateLayout();
                        dgFreeDocMarkers.ScrollIntoView(dgFreeDocMarkers.SelectedItem);
                    }

                    dgFreeDocMarkersSelect();

                    dgFreeDocRelationships.SelectedItem = rel;
                    if (dgFreeDocRelationships.SelectedItem != null) //-V3022
                    {
                        dgFreeDocRelationships.UpdateLayout();
                        dgFreeDocRelationships.ScrollIntoView(dgFreeDocRelationships.SelectedItem);
                    }
                }
            }
        }

        /// <summary>Изменение связи маркера</summary>
        private void EditRelation_Click(object sender, RoutedEventArgs e)
        {
            FreeDocMarker marker = null;
            string alias = "";

            if (dgFreeDocMarkers.SelectedIndex >= 0)
            {
                marker = dgFreeDocMarkers.SelectedItem as FreeDocMarker;
                alias = marker.FreeDocMarker_TableAlias;
            }
            else alias = "";

            if (dgFreeDocRelationships.SelectedIndex >= 0)
            {
                FreeDocRelationship rel = dgFreeDocRelationships.SelectedItem as FreeDocRelationship;

                if (AddEditRelationship(alias, ref rel) && (rel != null)) //-V3095
                {
                    dgFreeDocMarkersRefresh();
                    dgFreeDocMarkers.SelectedItem = marker;
                    if (dgFreeDocMarkers.SelectedItem != null) //-V3022
                    {
                        dgFreeDocMarkers.UpdateLayout();
                        dgFreeDocMarkers.ScrollIntoView(dgFreeDocMarkers.SelectedItem);
                    }

                    dgFreeDocMarkersSelect();

                    dgFreeDocRelationships.SelectedItem = rel;
                    if (dgFreeDocRelationships.SelectedItem != null) //-V3022
                    {
                        dgFreeDocRelationships.UpdateLayout();
                        dgFreeDocRelationships.ScrollIntoView(dgFreeDocRelationships.SelectedItem);
                    }

                }
            }
        }

        /// <summary>Двойной клик мышью на строке в dgFreeDocMarkers</summary>
        private void dgFreeDocMarkers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            EditMarker_Click(sender, e);
        }

        /// <summary>Двойной клик мышью на строке в dgFreeDocRelationships</summary>
        private void dgFreeDocRelationships_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            EditRelation_Click(sender, e);
        }

        /// <summary>Нажата кнопка "Добавить подключение" на вкладке Script</summary>
        private void btAddConnect_Click(object sender, RoutedEventArgs e)
        {
            var connect = Utilities.Controls.OpenConnectFromComboBox(cbConnect, true);

            if (connect.ConnType != Utilities.ConnType.None)
            {
                Utilities.Controls.RefreshConnectItems(cbConnect, connect.DBConnectionName, null, null);
            }

            connect.CloseConnect();
        }

        /// <summary>Нажата кнопка "Добавить подключение" на вкладке Таблица</summary>
        private void btAddConnectSQL_Click(object sender, RoutedEventArgs e)
        {
            var connect = Utilities.Controls.OpenConnectFromComboBox(cbConnectSQL, true);

            if (connect.ConnType != Utilities.ConnType.None)
            {
                Utilities.Controls.RefreshConnectItems(cbConnectSQL, connect.DBConnectionName, null, null);
            }

            connect.CloseConnect();
        }

        /// <summary>Нажата кнопка Сохранить в файл на вкладке Script</summary>
        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            string filename = "";

            try
            {
                // имя временного файла для скрипта
                var generator = new RandomGenerator();
                filename = System.IO.Path.Combine(MainWindow.Task.TaskPath, generator.RandomString(8) + ".tmp");

                // создаем и заполняем временный файл
                Encoding encoding = new UTF8Encoding(false);
                File.WriteAllText(filename, tbScript.Text, encoding);

                // определить номер скрипта 
                string ScriptNumber = (Utilities.Files.MaxScriptNumber(MainWindow.Task.TaskPath) + 1).ToString();

                // разбираем на хранимки временный файл
                Utilities.Databases.SaveProcScript(this.PrefixToFilename, MainWindow.Task.TaskNumber, filename, ScriptNumber, cbConnect);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }

            try
            {
                if (
                    (!string.IsNullOrWhiteSpace(filename)) &&
                    File.Exists(filename)
                )
                {
                    File.Delete(filename);
                }
            }
            catch
            {
            }
        }

        /// <summary>Нажата кнопка В буфер обмена на вкладке Script</summary>
        private void btClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(tbScript.Text);
        }

        /// <summary>Нажата кнопка "Выполнить скрипт" на вкладке Script</summary>
        private void btExecScript_Click(object sender, RoutedEventArgs e)
        {
            if (btExecScript.IsEnabled)
            {
                if (cbConnect.SelectedIndex == -1)
                {
                    MessageBox.Show("Для выполнения скрипта необходимо выбрать подключение к БД !");
                    cbConnect.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(tbScript.Text))
                {
                    MessageBox.Show("Скрипт пустой !");
                    tbScript.Focus();
                    return;
                }

                tbScriptMessages.Text = "";
                dgScriptResults.ItemsSource = null;
                lbScriptStatus.Content = "";
                Utilities.External.ExecType _type;
                string sql;

                if (!string.IsNullOrWhiteSpace(tbScript.SelectedText))
                {
                    sql = tbScript.SelectedText;
                }
                else
                {
                    sql = tbScript.Text;
                }

                if (
                    sql.ToLower().Contains("insert into ") ||
                    sql.ToLower().Contains("update ")
                )
                {
                    _type = Utilities.External.ExecType.DEFAULT;
                }
                else
                {
                    _type = Utilities.External.ExecType.QUERY;
                }

                if (
                    (_type == Utilities.External.ExecType.QUERY) &&
                    sql.Contains("{evn_id}")
                    )
                {
                    // запросить идентификатор
                    FormAskNumFile dlg1 = new FormAskNumFile();
                    dlg1.Text = "Значение идентификатора";
                    dlg1.lbNumFile.Text = "evn_id = ";
                    string id = "0";
                    if (
                        (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK) &&
                        (!string.IsNullOrWhiteSpace(dlg1.tbNumFile.Text))
                        )
                    {
                        id = dlg1.tbNumFile.Text;
                        if (string.IsNullOrWhiteSpace(id)) id = "0"; //-V3022
                    }
                    dlg1.Dispose();
                    sql = sql.Replace("{evn_id}", id);
                }

                if (_type == Utilities.External.ExecType.QUERY)
                {
                    Utilities.External.ExecScriptInThread(_type, this, cbConnect, sql, btExecScript, tiScriptResults, dgScriptResults, tiScriptMessages, tbScriptMessages, lbScriptStatus);
                }
                else
                {
                    Utilities.External.ExecScriptInThread(_type, this, cbConnect, sql, btExecScript, null, null, tiScriptMessages, tbScriptMessages, lbScriptStatus);
                }
            }
        }

        /// <summary>Изменилось значение в поле Script (при условии, что разрешено его редактирование) на вкладке Script</summary>
        private void tbScript_TextChanged(object sender, EventArgs e)
        {

        }

        /// <summary>Нажата Enter в строке фильтрации</summary>
        private void tbFilter_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                btFilter_Click(sender, e);
                e.Handled = true;
            }
        }

        /// <summary>Выделить измененные записи в dgFreeDocMarkers</summary>
        private void dgFreeDocMarkers_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            FreeDocMarker marker = (FreeDocMarker)e.Row.DataContext;

            if (marker.isChanged)
                e.Row.FontWeight = FontWeights.Bold;
            else
                e.Row.FontWeight = FontWeights.Normal;

            // нумерация строк
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        /// <summary>Выделить измененные записи в dgFreeDocRelationships</summary>
        private void dgFreeDocRelationships_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            FreeDocRelationship marker = (FreeDocRelationship)e.Row.DataContext;

            if (marker.isChanged)
                e.Row.FontWeight = FontWeights.Bold;
            else
                e.Row.FontWeight = FontWeights.Normal;

        }

        /// <summary>Установлен флаг "Измененные"</summary>
        private void cbChanged_Checked(object sender, RoutedEventArgs e)
        {
            dgFreeDocMarkersRefresh();
        }

        /// <summary>Снят флаг "Измененные"</summary>
        private void cbChanged_Unchecked(object sender, RoutedEventArgs e)
        {
            dgFreeDocMarkersRefresh();
        }

        /// <summary>Нажата кнопка "Фильтр"</summary>
        private void btFilter_Click(object sender, RoutedEventArgs e)
        {
            dgFreeDocMarkersRefresh();

            // Добавить в историю
            if (!string.IsNullOrWhiteSpace(tbFilter.Text))
            {
                tbFilter.AddHistory(tbFilter.Text);
            }
        }

        /// <summary>Копирование маркера</summary>
        private void btCopy_Click(object sender, RoutedEventArgs e)
        {
            if (dgFreeDocMarkers.SelectedIndex >= 0)
            {
                FreeDocMarker marker = dgFreeDocMarkers.SelectedItem as FreeDocMarker;
                marker = marker.Copy();

                if (AddEditMarker(MarkerOperType.COPY, ref marker) && (marker != null))
                {
                    ListFreeDocMarker.Add(marker);

                    dgFreeDocMarkersRefresh();

                    dgFreeDocMarkers.SelectedItem = marker;
                    if (dgFreeDocMarkers.SelectedItem != null) //-V3022
                    {
                        dgFreeDocMarkers.UpdateLayout();
                        dgFreeDocMarkers.ScrollIntoView(dgFreeDocMarkers.SelectedItem);
                    }

                    dgFreeDocRelationshipsRefresh();
                }

            }
        }

        /// <summary>Установлен флаг "С ошибками"</summary>
        private void cbError_Checked(object sender, RoutedEventArgs e)
        {
            dgFreeDocMarkersRefresh();
        }

        /// <summary>Снят флаг "С ошибками"</summary>
        private void cbError_Unchecked(object sender, RoutedEventArgs e)
        {
            dgFreeDocMarkersRefresh();
        }

        /// <summary>Нажата кнопка "Удалить"</summary>
        private void btDel_Click(object sender, RoutedEventArgs e)
        {
            if (dgFreeDocMarkers.SelectedIndex >= 0)
            {
                FreeDocMarker marker = dgFreeDocMarkers.SelectedItem as FreeDocMarker;

                if (marker.FreeDocMarker_id.StartsWith("новый"))
                {
                    if (System.Windows.Forms.MessageBox.Show("Удалить " + marker.FreeDocMarker_Name + " ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        ListFreeDocMarker.Remove(marker);

                        dgFreeDocMarkersRefresh();
                        dgFreeDocRelationshipsRefresh();
                    }
                }
            }
        }

        /// <summary>Нажата кнопка "Удалить"</summary>
        private void btDelRel_Click(object sender, RoutedEventArgs e)
        {
            if (dgFreeDocRelationships.SelectedIndex >= 0)
            {
                FreeDocRelationship rel = dgFreeDocRelationships.SelectedItem as FreeDocRelationship;

                if (rel.FreeDocRelationship_id.StartsWith("новый"))
                {
                    if (System.Windows.Forms.MessageBox.Show("Удалить " + rel.FreeDocRelationship_AliasName + " ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        ListFreeDocRelationship.Remove(rel);

                        dgFreeDocRelationshipsRefresh();
                    }
                }
            }
        }

        /// <summary>Нажата кнопка "Очистить фильтр"</summary>
        private void btDelFilter_Click(object sender, RoutedEventArgs e)
        {
            tbFilter.Text = "";
            dgFreeDocMarkersRefresh();
        }


        /// <summary>Выбрать существующую запись в FreeDocRelationship</summary>
        private string ChooseExistRelationship(FreeDocRelationship rel)
        {
            string result = "";

            string id;
            string alias;

            if (rel != null)
            {
                id = "where cast(FreeDocRelationship_id as varchar) != '" + rel.FreeDocRelationship_id + "'";
                alias = rel.FreeDocRelationship_LinkedAlias;
            }
            else
            {
                id = "";
                alias = "";
            }

            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                return "";
            }

            try
            {
                FormAddExistRel dlg1 = new FormAddExistRel(this);

                Utilities.Controls.RefreshComboBoxItems(dlg1.FreeDocRelationship_AliasName, ConnectSQL, "select FreeDocRelationship_AliasName from dbo.FreeDocRelationship " + id + " order by FreeDocRelationship_AliasName");

                if (string.IsNullOrWhiteSpace(alias))
                {
                    dlg1.FreeDocRelationship_AliasName.SelectedIndex = -1;
                }
                else
                {
                    dlg1.FreeDocRelationship_AliasName.SelectedItem = alias;
                }


                if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    result = dlg1.FreeDocRelationship_AliasName.Text;
                }
                dlg1.Dispose();

            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                result = "";
            }

            return result;
        }

        /// <summary>Привязать источник данных</summary>
        private void btAddExistRel_Click(object sender, RoutedEventArgs e)
        {
            if (dgFreeDocRelationships.SelectedIndex >= 0)
            {
                FreeDocMarker marker = dgFreeDocMarkers.SelectedItem as FreeDocMarker;
                FreeDocRelationship rel = dgFreeDocRelationships.SelectedItem as FreeDocRelationship;

                string alias = ChooseExistRelationship(rel);

                if ((rel != null) && (!string.IsNullOrWhiteSpace(alias)))
                {
                    rel.FreeDocRelationship_LinkedAlias = alias;

                    dgFreeDocMarkersRefresh();
                    dgFreeDocMarkers.SelectedItem = marker;
                    if (dgFreeDocMarkers.SelectedItem != null) //-V3022
                    {
                        dgFreeDocMarkers.UpdateLayout();
                        dgFreeDocMarkers.ScrollIntoView(dgFreeDocMarkers.SelectedItem);
                    }

                    dgFreeDocMarkersSelect();

                    dgFreeDocRelationships.SelectedItem = rel;
                    if (dgFreeDocRelationships.SelectedItem != null) //-V3022
                    {
                        dgFreeDocRelationships.UpdateLayout();
                        dgFreeDocRelationships.ScrollIntoView(dgFreeDocRelationships.SelectedItem);
                    }
                }
            }
        }

        /// <summary>Установлен флаг "Все связи"</summary>
        private void cbAllRel_Checked(object sender, RoutedEventArgs e)
        {
            FreeDocMarker marker = null;
            if (dgFreeDocMarkers.SelectedIndex >= 0)
            {
                marker = dgFreeDocMarkers.SelectedItem as FreeDocMarker;
            }

            dgFreeDocMarkersRefresh();

            if (marker != null)
            {
                dgFreeDocMarkers.SelectedItem = marker;
                if (dgFreeDocMarkers.SelectedItem != null) //-V3022
                {
                    dgFreeDocMarkers.UpdateLayout();
                    dgFreeDocMarkers.ScrollIntoView(dgFreeDocMarkers.SelectedItem);
                }
            }

            dgFreeDocMarkersSelect();
        }

        /// <summary>Снят флаг "Все связи"</summary>
        private void cbAllRel_Unchecked(object sender, RoutedEventArgs e)
        {
            dgFreeDocMarkersSelect();
        }

        /// <summary>
        /// генерация скрипта
        /// </summary>
        /// <param name="ChoosedRelation">выбранные связи</param>
        /// <param name="ChoosedMarker">выбранные маркеры</param>
        private string GenerateScript(List<FreeDocMarker> ChoosedMarker, List<FreeDocRelationship> ChoosedRelation)
        {
            tbScriptMessages.Text = "";
            dgScriptResults.ItemsSource = null;
            lbScriptStatus.Content = "";

            // текст скрипта
            string script = "";

            // хинт для MS SQL
            string mshint_sel = "";
            if (this.TargetDB == Utilities.TargetDBType.MSSQL) mshint_sel = "WITH (nolock) ";

            // ============================================== dbo.FreeDocRelationship ===================================================

            // соберем список измененных связей
            foreach (FreeDocRelationship rel in ChoosedRelation)
            {
                // определяем EvnClass_id
                string s = ConnectSQL.GetValueFromQuery("SELECT EvnClass_id FROM dbo.EvnClass " + mshint_sel + "WHERE EvnClass_SysNick = '" + rel.EvnClass_SysNick + "'", "EvnClass_id", 1);
                long EvnClass_id;
                if (string.IsNullOrWhiteSpace(s) || (!long.TryParse(s, out EvnClass_id)))
                {
                    EvnClass_id = 1;
                }

                // AliasName нужен всегда заполненным
                if (!string.IsNullOrWhiteSpace(rel.FreeDocRelationship_AliasName))
                {
                    string _command = "";

                    string _aliasname = rel.FreeDocRelationship_AliasName
                        .Replace("'", "");
                    if (string.IsNullOrWhiteSpace(_aliasname))
                    {
                        _aliasname = "";
                    }
                    string _aliastable = rel.FreeDocRelationship_AliasTable
                        .Replace("'","");
                    if (string.IsNullOrWhiteSpace(_aliastable))
                    {
                        _aliastable = "";
                    }
                    string _aliasquery = rel.FreeDocRelationship_AliasQuery
                        .Replace("'", "''");
                    if (string.IsNullOrWhiteSpace(_aliasquery))
                    {
                        _aliasquery = "";
                    }
                    string _linkedalias = rel.FreeDocRelationship_LinkedAlias
                        .Replace("'", "");
                    if (string.IsNullOrWhiteSpace(_linkedalias))
                    {
                        _linkedalias = "";
                    }
                    string _linkdescription = rel.FreeDocRelationship_LinkDescription
                        .Replace("'", "''");
                    if (string.IsNullOrWhiteSpace(_linkdescription))
                    {
                        _linkdescription = "";
                    }

                    if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD))
                    {
                        _aliasquery = _aliasquery.Replace("with(nolock)", "")
                            .Replace("with (nolock)", "")
                            .Replace("with(rowlock)", "")
                            .Replace("with (rowlock)", "")
                            .Replace("WITH(nolock)", "")
                            .Replace("WITH (nolock)", "")
                            .Replace("WITH(rowlock)", "")
                            .Replace("WITH (rowlock)", "")
                            .Replace("with(NOLOCK)", "")
                            .Replace("with (NOLOCK)", "")
                            .Replace("with(ROWLOCK)", "")
                            .Replace("with (ROWLOCK)", "")
                            .Replace("WITH(NOLOCK)", "")
                            .Replace("WITH (NOLOCK)", "")
                            .Replace("WITH(ROWLOCK)", "")
                            .Replace("WITH (ROWLOCK)", "")
                            .Replace("(nolock)", "")
                            .Replace("(rowlock)", "")
                            .Replace("(NOLOCK)", "")
                            .Replace("(ROWLOCK)", "");
                    }

                    if (this.TargetDB == Utilities.TargetDBType.MSSQL) // для MS SQL
                    {
                        _command = $"DECLARE @res VARCHAR(max); EXEC dbo.xp_Gen_FreeDocRelationship @EvnClass_id = {EvnClass_id.ToString()}, @FreeDocRelationship_AliasName = '{_aliasname}', @FreeDocRelationship_AliasTable = '{_aliastable}', @FreeDocRelationship_AliasQuery = '{_aliasquery}', @FreeDocRelationship_LinkedAlias = '{_linkedalias}', @FreeDocRelationship_LinkDescription = '{_linkdescription}', @isExec = NULL, @TaskNumber = '{MainWindow.Task.TaskNumber}', @res = @res OUTPUT; SELECT @res;";
                    }
                    else
                    {
                        _command = $"SELECT * FROM dbo.xp_gen_freedocrelationship(EvnClass_id := {EvnClass_id.ToString()}, FreeDocRelationship_AliasName := '{_aliasname}', FreeDocRelationship_AliasTable := '{_aliastable}', FreeDocRelationship_AliasQuery := '{_aliasquery}', FreeDocRelationship_LinkedAlias := '{_linkedalias}', FreeDocRelationship_LinkDescription := '{_linkdescription}', isExec := NULL, TaskNumber := '{MainWindow.Task.TaskNumber}');";
                    }

                    string _result = Environment.NewLine + $"-- SQLGen: FreeDocRelationship freedocrelationship_{_aliasname}_{EvnClass_id.ToString()} --";
                    try
                    {
                        using (DbDataReader reader = ConnectSQL.OpenQuery(_command))
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        _result += Environment.NewLine + reader[i].ToString();
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _result += Environment.NewLine + App.AddLog("Ошибка выполнения: " + Environment.NewLine + _command + Environment.NewLine, ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile).showMessage;
                    }

                    script += _result;

                    if (this.TargetDB == Utilities.TargetDBType.MSSQL) // для MS SQL
                    {
                        script +=
                            Environment.NewLine + "-- Проверка" +
                            Environment.NewLine + "-- SELECT * FROM dbo.FreeDocRelationship WHERE FreeDocRelationship_AliasName LIKE '" + rel.FreeDocRelationship_AliasName.Replace("'", "''") + "' AND EvnClass_id = " + EvnClass_id.ToString() + ";";
                    }
                    else
                    {
                        script +=
                            Environment.NewLine + "-- Проверка" +
                            Environment.NewLine + "-- SELECT * FROM dbo.FreeDocRelationship WHERE FreeDocRelationship_AliasName iLIKE '" + rel.FreeDocRelationship_AliasName.Replace("'", "''") + "' AND EvnClass_id = " + EvnClass_id.ToString() + ";";
                    }
                }
            }

            // ============================================== dbo.FreeDocMarker ===================================================

            // соберем список измененных маркеров
            foreach (FreeDocMarker marker in ChoosedMarker)
            {
                // определяем EvnClass_id
                string s = ConnectSQL.GetValueFromQuery("SELECT EvnClass_id FROM dbo.EvnClass " + mshint_sel + "WHERE EvnClass_SysNick = '" + marker.EvnClass_SysNick + "'", "EvnClass_id", 1);
                long EvnClass_id;
                if (string.IsNullOrWhiteSpace(s) || (!long.TryParse(s, out EvnClass_id)))
                {
                    EvnClass_id = 1;
                }

                // AliasName нужен всегда заполненным
                if (!string.IsNullOrWhiteSpace(marker.FreeDocMarker_Name))
                {
                    string _command = "";

                    string _name = marker.FreeDocMarker_Name
                        .Replace("'", "");
                    if (string.IsNullOrWhiteSpace(_name))
                    {
                        _name = "";
                    }
                    string _tablealias = marker.FreeDocMarker_TableAlias
                        .Replace("'", "");
                    if (string.IsNullOrWhiteSpace(_tablealias))
                    {
                        _tablealias = "";
                    }
                    string _field = marker.FreeDocMarker_Field
                        .Replace("'", "''");
                    if (string.IsNullOrWhiteSpace(_field))
                    {
                        _field = "";
                    }
                    string _query = marker.FreeDocMarker_Query
                        .Replace("'", "''");
                    if (string.IsNullOrWhiteSpace(_query))
                    {
                        _query = "";
                    }
                    string _description = marker.FreeDocMarker_Description
                        .Replace("'", "''");
                    if (string.IsNullOrWhiteSpace(_description))
                    {
                        _description = "";
                    }
                    string _istablevalue = marker.FreeDocMarker_IsTableValue_string;
                    if (string.IsNullOrWhiteSpace(_istablevalue)) //-V3022
                    {
                        _istablevalue = "NULL";
                    }
                    string _options = marker.FreeDocMarker_Options
                        .Replace("'", "''");
                    if (string.IsNullOrWhiteSpace(_options))
                    {
                        _options = "";
                    }

                    if ((this.TargetDB == Utilities.TargetDBType.PGSQL) || (this.TargetDB == Utilities.TargetDBType.EMD))
                    {
                        _query = _query.Replace("with(nolock)", "")
                            .Replace("with (nolock)", "")
                            .Replace("with(rowlock)", "")
                            .Replace("with (rowlock)", "")
                            .Replace("WITH(nolock)", "")
                            .Replace("WITH (nolock)", "")
                            .Replace("WITH(rowlock)", "")
                            .Replace("WITH (rowlock)", "")
                            .Replace("with(NOLOCK)", "")
                            .Replace("with (NOLOCK)", "")
                            .Replace("with(ROWLOCK)", "")
                            .Replace("with (ROWLOCK)", "")
                            .Replace("WITH(NOLOCK)", "")
                            .Replace("WITH (NOLOCK)", "")
                            .Replace("WITH(ROWLOCK)", "")
                            .Replace("WITH (ROWLOCK)", "")
                            .Replace("(nolock)", "")
                            .Replace("(rowlock)", "")
                            .Replace("(NOLOCK)", "")
                            .Replace("(ROWLOCK)", "");
                    }

                    if (this.TargetDB == Utilities.TargetDBType.MSSQL) // для MS SQL
                    {
                        _command = $"DECLARE @res VARCHAR(max); EXEC dbo.xp_Gen_FreeDocMarker @EvnClass_id = {EvnClass_id.ToString()}, @FreeDocMarker_Name = '{_name}', @FreeDocMarker_TableAlias = '{_tablealias}', @FreeDocMarker_Field = '{_field}', @FreeDocMarker_Query = '{_query}', @FreeDocMarker_Description = '{_description}', @FreeDocMarker_IsTableValue = {_istablevalue}, @FreeDocMarker_Options = '{_options}', @isExec = NULL, @TaskNumber = '{MainWindow.Task.TaskNumber}', @res = @res OUTPUT; SELECT @res;";
                    }
                    else
                    {
                        _command = $"SELECT * FROM dbo.xp_Gen_FreeDocMarker(EvnClass_id := {EvnClass_id.ToString()}, FreeDocMarker_Name := '{_name}', FreeDocMarker_TableAlias := '{_tablealias}', FreeDocMarker_Field := '{_field}', FreeDocMarker_Query := '{_query}', FreeDocMarker_Description := '{_description}', FreeDocMarker_IsTableValue := {_istablevalue}, FreeDocMarker_Options := '{_options}', isExec := NULL, TaskNumber := '{MainWindow.Task.TaskNumber}');";
                    }

                    string _result = Environment.NewLine + $"-- SQLGen: FreeDocMarker freedocmarker_{_name}_{EvnClass_id.ToString()} --";
                    try
                    {
                        using (DbDataReader reader = ConnectSQL.OpenQuery(_command))
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    for (int i = 0; i < reader.FieldCount; i++)
                                    {
                                        _result += Environment.NewLine + reader[i].ToString();
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _result += Environment.NewLine + App.AddLog("Ошибка выполнения: " + Environment.NewLine + _command + Environment.NewLine, ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile).showMessage;
                    }

                    script += _result;

                    if (this.TargetDB == Utilities.TargetDBType.MSSQL) // для MS SQL
                    {
                        script +=
                            Environment.NewLine + "-- Проверка" +
                            Environment.NewLine + "-- SELECT * FROM dbo.FreeDocMarker WHERE FreeDocMarker_Name LIKE '" + marker.FreeDocMarker_Name.Replace("'", "''") + "' AND EvnClass_id = " + EvnClass_id.ToString() + ";";
                    }
                    else
                    {
                        script +=
                            Environment.NewLine + "-- Проверка" +
                            Environment.NewLine + "-- SELECT * FROM dbo.FreeDocMarker WHERE FreeDocMarker_Name iLIKE '" + marker.FreeDocMarker_Name.Replace("'", "''") + "' AND EvnClass_id = " + EvnClass_id.ToString() + ";";
                    }
                }
            }

            // убираем лишние переводы строки в конце, оставляем один
            script = script
                .TrimStartNewLine()
                .TrimEndNewLine(Environment.NewLine);

            return script;
        }

        /// <summary>
        /// нажата кнопка Сгенерировать скрипт (по изменениям)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGenerateScript_Click(object sender, RoutedEventArgs e)
        {
            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                return;
            }

            // собираем скрипт для выбранных маркеров
            tbScript.Text = GenerateScript(
                ListFreeDocMarker.Where(x => x.isChanged == true).ToList(),
                ListFreeDocRelationship.Where(x => x.isChanged == true).ToList()
                );
            tbScript.Filename = "";

            tiScript.IsSelected = true;
        }


        /// <summary>
        /// нажата кнопка Сгенерировать скрипт (выбранный маркер)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btGenerateScriptMarker_Click(object sender, RoutedEventArgs e)
        {
            if ((isRegion.IsChecked == true) && (cbRegion.SelectedIndex == -1))
            {
                MessageBox.Show("Необходимо выбрать Регион !");
                cbRegion.Focus();
                return;
            }

            if (!CheckConnectSQL())
            {
                cbConnectSQL.Focus();
                MessageBox.Show("Подключение " + cbConnectSQL.Text + " не открыто !");
                return;
            }

            if (dgFreeDocMarkers.SelectedIndex >= 0)
            {
                FreeDocMarker marker = dgFreeDocMarkers.SelectedItem as FreeDocMarker;
                if (marker != null)
                {
                    try
                    {
                        FormEvnClass dlg1 = new FormEvnClass();

                        dlg1.EvnClass_SysNick.Items.Clear();

                        foreach (var item in ListEvnClass.Where(x => EvnClassParent(x.SysNick, marker.EvnClass_SysNick)).OrderBy(x => x.SysNick))
                        {
                            if (!dlg1.EvnClass_SysNick.Items.Contains(item.SysNick))
                                dlg1.EvnClass_SysNick.Items.Add(item.SysNick);
                        }

                        dlg1.EvnClass_SysNick.SelectedItem = marker.EvnClass_SysNick;

                        if ((dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK) && (dlg1.EvnClass_SysNick.SelectedIndex != -1))
                        {
                            // связи
                            List<FreeDocRelationship> list = new List<FreeDocRelationship>();

                            if (!string.IsNullOrWhiteSpace(marker.FreeDocMarker_TableAlias))
                            {
                                list = GetRelations(dlg1.EvnClass_SysNick.SelectedItem.ToString(), marker.FreeDocMarker_TableAlias);
                            }

                            // собираем скрипт для выбранных маркеров
                            tbScript.Text = GenerateScript(
                                new List<FreeDocMarker>() { marker },
                                list
                                );
                            tbScript.Filename = "";

                            tiScript.IsSelected = true;
                        }
                        dlg1.Dispose();
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    }
                }
            }
        }

        private void tiStructure_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F3)
            {
                btRefresh_Click(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.F5)
            {
                btGenerateScript_Click(null, null);
                e.Handled = true;
            }
            if (e.Key == Key.F6)
            {
                btGenerateScriptMarker_Click(null, null);
                e.Handled = true;
            }
        }

        private void tbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbFilter.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbFilter.SelectedItem;

                tbFilter.Text = (string)cbItem.Tag;
            }
        }

        private void tbFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            btFilter_Click(sender, e);
        }

        private void tbFilter_DropDownClosed(object sender, EventArgs e)
        {
            btFilter_Click(sender, null);
        }

        private void tiScript_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                btExecScript_Click(sender, e);
                e.Handled = true;
            }
            if (e.Key == Key.F8)
            {
                btSave_Click(sender, e);
                e.Handled = true;
            }
        }

        private void dgScriptResults_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            string header = e.Column.Header.ToString();

            // Replace all underscores with two underscores, to prevent AccessKey handling
            e.Column.Header = header.Replace("_", "__");

            if (e.PropertyType == typeof(System.DateTime))
                (e.Column as DataGridTextColumn).Binding.StringFormat = "dd.MM.yyyy HH:mm:ss.FFF";
        }

        void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }
    }
}
