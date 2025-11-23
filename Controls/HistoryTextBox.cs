// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// TextBox с историей значений
    /// </summary>
    public class HistoryTextBox : ComboBox
    {
        /// <summary>
        /// Файл с историей
        /// </summary>
        string HistoryFile;

        /// <summary>
        /// Текущая группа
        /// </summary>
        string HistoryGroup;

        /// <summary>
        /// История (все группы)
        /// </summary>
        List<HistoryGroup> HistoryList;

        /// <summary>
        /// Максимальное кол-во запоминаемых фраз в группе
        /// </summary>
        int MaxHistoryItem;

        /// <summary>Конструктор HistoryTextBox</summary>
        public HistoryTextBox()
        {
            SetResourceReference(StyleProperty, typeof(ComboBox));

            this.IsSynchronizedWithCurrentItem = false;
            this.IsTextSearchEnabled = false;
            this.IsEditable = true;
            this.IsReadOnly = false;
        }

        /// <summary>
        /// Стартовая инициализация
        /// </summary>
        /// <param name="_historyfile">Файл с историей</param>
        /// <param name="_group">Группа</param>
        /// <param name="max">Максимальное кол-во в истории</param>
        public void InitHistory(string _historyfile, string _group, int max = 20)
        {
            if (string.IsNullOrWhiteSpace(_historyfile))
            {
                var generator = new RandomGenerator();
                HistoryFile = generator.RandomString(8) + ".json";
            }
            else
            {
                HistoryFile = _historyfile;
            }

            if (string.IsNullOrWhiteSpace(_group))
            {
                HistoryGroup = "DEFAULT";
            }
            else
            {
                HistoryGroup = _group;
            }

            HistoryList = new List<HistoryGroup>();
            MaxHistoryItem = max;

            LoadHistory();
        }


        /// <summary>Загрузить историю</summary>
        public void LoadHistory()
        {
            string filename = Path.Combine(App.AppPath, HistoryFile);
            if (File.Exists(filename))
                try
                {
                    string jsonString = File.ReadAllText(filename);
                    HistoryList = JsonSerializer.Deserialize<List<HistoryGroup>>(jsonString, new JsonSerializerOptions { IgnoreReadOnlyProperties = true, WriteIndented = true });
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                }

            RefreshHistory();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Обновить историю</summary>
        public void RefreshHistory()
        {
            string old_value = this.Text;
            if (string.IsNullOrWhiteSpace(old_value))
            {
                old_value = "";
            }

            this.SelectedIndex = -1;
            this.Items.Clear();
            ComboBoxItem cbItem = new ComboBoxItem { Content = "", Tag = "" };
            this.Items.Add(cbItem);

            var grp = HistoryList.Find(x => x.Group.ToLower().Trim() == this.HistoryGroup.ToLower().Trim());
            if (grp != null)
            {
                foreach (var line in grp.List)
                {
                    cbItem = new ComboBoxItem { Content = line.Replace(Environment.NewLine, " "), Tag = line };
                    this.Items.Add(cbItem);
                }
            }

            this.Text = old_value;
            cbItem = new ComboBoxItem { Content = old_value.Replace(Environment.NewLine, " "), Tag = old_value };
            if (this.Items.Contains(cbItem)) this.SelectedItem = cbItem;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить значение в историю и Сохранить историю в файл
        /// </summary>
        /// <param name="_value">значение</param>
        public void AddHistory(string _value)
        {
            // ищем группу
            var grp = HistoryList.Find(x => x.Group.ToLower().Trim() == this.HistoryGroup.ToLower().Trim());
            if (grp == null)
            {
                HistoryList.Add(new HistoryGroup() { Group = this.HistoryGroup });
                grp = HistoryList.Find(x => x.Group.ToLower().Trim() == this.HistoryGroup.ToLower().Trim());
            }

            if (grp != null)
            {
                // добавляем фразу в начало списка фраз группы
                if (!string.IsNullOrWhiteSpace(_value))
                {
                    if (
                        (grp.List.Count == 0) ||
                        (grp.List[0] != _value)
                        )
                    {
                        grp.List.Insert(0, _value);
                    }
                }

                // следим чтобы не превышало максимальное кол-во
                if (grp.List.Count > MaxHistoryItem) grp.List.RemoveAt(grp.List.Count - 1);
            }

            // сразу записываем в файл
            string filename = Path.Combine(App.AppPath, HistoryFile);
            string jsonString = "";

            if (!string.IsNullOrWhiteSpace(filename)) //-V3022
            {
                try
                {
                    jsonString = JsonSerializer.Serialize<List<HistoryGroup>>(HistoryList, new JsonSerializerOptions { IgnoreReadOnlyProperties = true, WriteIndented = true });
                    File.WriteAllText(filename, jsonString);
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                }
            }

            RefreshHistory();
        }
    }

    /// <summary>
    /// История фраз в группе
    /// </summary>
    class HistoryGroup
    {
        /// <summary>
        /// Название группы
        /// </summary>
        public string Group { get; set; }

        /// <summary>
        /// Список фраз группы
        /// </summary>
        public List<string> List { get; set; }

        /// <summary>
        /// Конструктор HistoryGroup
        /// </summary>
        public HistoryGroup()
        {
            List = new List<string>();
        }

    }
}
