// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using SQLGen.Controls;


namespace SQLGen
{
    /// <summary>
    /// Окно добавления\изменения FK
    /// </summary>
    public partial class WinAddCron : Window
    {
        /// <summary>
        /// Признак, что можно сохранить изменения
        /// </summary>
        public bool isOk = false;

        /// <summary>ссылка на экземпляр основного окна программы</summary>
        public MainWindow mainWindow;

        /// <summary>
        /// лог-файл
        /// </summary>
        public string logFile;

        /// <summary>
        /// ссылка на редактируемое Действие при обновлении
        /// </summary>
        Cron cron = new Cron();

        /// <summary>Конструктор WinAddCron</summary>
        public WinAddCron()
        {
            InitializeComponent();

            tbTask.InitHistory("HistoryAddCron.json", "TaskNumber");
            tbApplicationName.InitHistory("HistoryAddCron.json", "ApplicationName");
            tbTeam.InitHistory("HistoryAddCron.json", "Team");

            // пользовательские настройки GUI
            Default.InitGUI("WinAddCron", this, mainGrid, null, null, null, MainWindow.Task.LogFile);
        }

        /// <summary>При открытии окна WinAddCron</summary>
        private void winAddCron_Activated(object sender, EventArgs e)
        {
            cron = this.DataContext as Cron;
        }

        /// <summary>При закрытии окна WinAddCron</summary>
        private void winAddCron_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinAddCron", this, null);
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            isOk = false;

            if (string.IsNullOrWhiteSpace(tbTask.Text))
            {
                App.AddLog("Укажите номер задачи", null, App.ShowMessageMode.SHOW, false, "");
                tbTask.Focus();
                return;
            }

            if (!Task.IsTaskNumberCorrect(tbTask.Text))
            {
                MessageBox.Show("Номер задачи " + tbTask.Text + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита в верхнем регистре (A-Z), знак подчеркивания (_), знак тире (-)");
                tbTask.Focus();
                return;
            }

            if (cbDBRegions.SelectedIndex == -1)
            {
                App.AddLog("Выберите тип региона по типу основной БД", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbApplicationName.Text))
            {
                App.AddLog("Укажите наименование задания", null, App.ShowMessageMode.SHOW, false, "");
                tbApplicationName.Focus();
                return;
            }

            if (!Cron.IsApplicationNameCorrect(tbApplicationName.Text))
            {
                MessageBox.Show("Наименование задания " + tbApplicationName.Text + " содержит не разрешенные символы!" + Environment.NewLine + "Разрешены символы латинского алфавита (a-z A-Z), знак подчеркивания (_), знак тире (-)");
                tbApplicationName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(tbCommand.Text))
            {
                App.AddLog("Укажите команду (скрипт) задания", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbSchedule.Text))
            {
                App.AddLog("Укажите расписание задания", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (cbState.SelectedIndex == -1)
            {
                App.AddLog("Выберите статус задания", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (cbDatabases.SelectedIndex == -1)
            {
                App.AddLog("Выберите целевую БД", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (cbStages.SelectedIndex == -1)
            {
                App.AddLog("Выберите тип целевой БД", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            if (
                cbTimeout.IsChecked == true &&
                tbHour.Text == "0" &&
                tbMinute.Text == "0" &&
                tbSecond.Text == "0"
            )
            {
                App.AddLog("Укажите продолжительность таймаута (часы, минуты, секунды)", null, App.ShowMessageMode.SHOW, false, "");
                return;
            }

            isOk = true;

            // Добавить в историю
            tbTask.AddHistory(tbTask.Text);
            tbApplicationName.AddHistory(tbApplicationName.Text);
            tbTeam.AddHistory(tbTeam.Text);

            this.Close();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            isOk = false;
            this.Close();
        }

        private static readonly Regex regex_time = new Regex("[^0-9]+"); 
        private static bool IsTimeElement(string text)
        {
            return regex_time.IsMatch(text.Trim());
        }

        private new void PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = IsTimeElement(e.Text);
        }

        // Use the DataObject.Pasting Handler 
        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (IsTimeElement(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void btSetSchedule_Click(object sender, RoutedEventArgs e)
        {
            WinCronBuilder WinCronBuilder = new WinCronBuilder();
            WinCronBuilder.ShowDialog();

            if (WinCronBuilder.isOk == true)
            {
                tbSchedule.Text = WinCronBuilder.CronExpression;
            }
        }

        private void cbDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (
                cbDatabases.SelectedIndex != -1 &&
                cron.database == "lis"
            )
            {
                cron.Set_lcbListRegions(new List<string> { "Башкирия" });
                lcbRegions.Items.Refresh();
            }
        }

        /// <summary>
        /// Выход из поля Номер задачи
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTask_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbTask.Text))
            {
                tbTask.Text = tbTask.Text
                .ToUpper()
                .Replace("https://jira.is-mis.ru/browse/".ToUpper(), "")
                .Replace("https://jira.rtmis.ru/browse/".ToUpper(), "")
                .Trim();
            }
        }

        /// <summary>
        /// Выход из поля Наименование
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbApplicationName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbApplicationName.Text))
            {
                tbApplicationName.Text = tbApplicationName.Text.Trim();
            }
        }

        /// <summary>
        /// Выход из поля Команда
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbTeam_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbTeam.Text))
            {
                tbTeam.Text = tbTeam.Text.Trim();
            }
        }

        /// <summary>Выбран номер задачи из истории</summary>
        private void tbTask_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbTask.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbTask.SelectedItem;

                tbTask.Text = (string)cbItem.Tag;
            }
        }

        /// <summary>Выбрано наименование из истории</summary>
        private void tbApplicationName_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbApplicationName.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbApplicationName.SelectedItem;

                tbApplicationName.Text = (string)cbItem.Tag;
            }
        }

        /// <summary>Выбрана команда из истории</summary>
        private void tbTeam_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tbTeam.SelectedIndex != -1)
            {
                ComboBoxItem cbItem = (ComboBoxItem)tbTeam.SelectedItem;

                tbTeam.Text = (string)cbItem.Tag;
            }
        }

        /// <summary>Закрылся список номеров задач</summary>
        private void tbTask_DropDownClosed(object sender, EventArgs e)
        {
            tbTask_LostFocus(null, null);
        }

        /// <summary>Закрылся список наименований</summary>
        private void tbApplicationName_DropDownClosed(object sender, EventArgs e)
        {
            tbApplicationName_LostFocus(null, null);
        }

        /// <summary>Закрылся список команд</summary>
        private void tbTeam_DropDownClosed(object sender, EventArgs e)
        {
            tbTeam_LostFocus(null, null);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            cron.Set_regions();
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            cron.Set_regions();
        }

        private void CheckBox_Unchecked_1(object sender, RoutedEventArgs e)
        {
            cron.Set_exclude_regions();
        }

        private void CheckBox_Checked_1(object sender, RoutedEventArgs e)
        {
            cron.Set_exclude_regions();
        }

        private void tbCheck_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(cron.check))
            {
                // если есть проверочный запрос, значит задание временное
                cbIsTemp.IsChecked = true;
            }
        }
    }
}
