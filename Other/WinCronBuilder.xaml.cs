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
using CronExpressionDescriptor;
using NCrontab;

namespace SQLGen
{
    /// <summary>
    /// Окно добавления\изменения FK
    /// </summary>
    public partial class WinCronBuilder : Window
    {
        /// <summary>
        /// Признак, что можно сохранить изменения
        /// </summary>
        public bool isOk = false;

        /// <summary>
        /// Итоговое раписание крон
        /// </summary>
        public string CronExpression;

        /// <summary>Конструктор WinCronBuilder</summary>
        public WinCronBuilder()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("WinCronBuilder", this, mainGrid, null, null, null, MainWindow.Task.LogFile);
        }

        /// <summary>При открытии окна WinCronBuilder</summary>
        private void winCronBuilder_Activated(object sender, EventArgs e)
        {
            //dgFields.Focus();
        }

        /// <summary>При закрытии окна WinCronBuilder</summary>
        private void winCronBuilder_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinCronBuilder", this, null);
        }

        private void btSave_Click(object sender, RoutedEventArgs e)
        {
            isOk = true;
            CronExpression = CronBuilderViewModel.CronToDeployment(WpfCronBuilder.CronExpression);
            this.Close();
        }

        private void btCancel_Click(object sender, RoutedEventArgs e)
        {
            isOk = false;
            CronExpression = null;
            this.Close();
        }

        private void btClear_Click(object sender, RoutedEventArgs e)
        {
            WpfCronBuilder.CronExpression = "* * * * * * *";
        }

        private void btDefault1Day_Click(object sender, RoutedEventArgs e)
        {
            WpfCronBuilder.CronExpression = "* 0 20 * * * *";
        }

        private void btDefault1Hour_Click(object sender, RoutedEventArgs e)
        {
            WpfCronBuilder.CronExpression = "* 0 19-6 * * * *";
        }

        private void btDefault30Min_Click(object sender, RoutedEventArgs e)
        {
            WpfCronBuilder.CronExpression = "* 0/30 19-6 * * * *";
        }

        private void btDefault10Min_Click(object sender, RoutedEventArgs e)
        {
            WpfCronBuilder.CronExpression = "* 0/10 19-6 * * * *";
        }

        private void btDefault1Min_Click(object sender, RoutedEventArgs e)
        {
            WpfCronBuilder.CronExpression = "* * 19-6 * * * *";
        }
    }

    /// <summary>
    /// ViewModel для редактора cron
    /// </summary>
    public class CronBuilderViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// реализация OnPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// реализация OnPropertyChanged
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //--------------------------------------------------------------------------------------

        private string _cronFromBuilder = "* * * * * * *";

        /// <summary>
        /// Поле для сохранения результата сборки cron
        /// </summary>
        public string CronFromBuilder
        {
            get { return _cronFromBuilder; }
            set
            {
                // CronExpression
                _cronFromBuilder = value;
                OnPropertyChanged(nameof(CronFromBuilder));
                OnPropertyChanged(nameof(CronFromBuilderByRus));
                OnPropertyChanged(nameof(CronExamples));
            }
        }

        /// <summary>
        /// Расписание по русски
        /// </summary>
        public string CronFromBuilderByRus
        {
            get
            {
                return ExpressionDescriptor.GetDescription(CronToDeployment(CronFromBuilder),
                    new Options()
                    {
                        Locale = "ru",
                        Use24HourTimeFormat = true,
                        ThrowExceptionOnParseError = false
                    });
            }
        }

        /// <summary>
        /// Примеры расписания
        /// </summary>
        public string CronExamples
        {
            get
            {
                try
                {
                    var s = CrontabSchedule.Parse(CronToDeployment(CronFromBuilder));
                    var start = DateTime.Now;
                    var end = start.AddYears(1);
                    var occurrences = s.GetNextOccurrences(start, end)
                        .Skip(0)
                        .Take(100);

                    return string.Join(Environment.NewLine,
                        from t in occurrences
                        select $"{t:ddd, dd-MM-yyyy HH:mm}");
                }
                catch (Exception)
                {
                    return "расписание не распознано!";
                }
            }
        }


        /// <summary>
        /// Строка расписания cron для добавления в Deployment
        /// </summary>
        /// <param name="_cron">строка расписания из калькулятора</param>
        /// <returns></returns>
        public static string CronToDeployment(string _cron)
        {
            if (string.IsNullOrWhiteSpace(_cron))
            {
                // по умолчанию 1 раз в минуту
                return "* * * * *";
            }

            string result = _cron.TrimInner().Trim();

            var arr = result
                    .Replace("?", "*")
                    .Replace("TODO", "*")
                    .Split(new char[] { ' ' });

            if (arr.Length == 7)
            {
                if (arr[2].Contains("-"))
                {
                    string min_s = arr[2].Split(new char[] { '-' })[0];
                    string max_s = arr[2].Split(new char[] { '-' })[1];

                    int.TryParse(min_s, out int min_n);
                    int.TryParse(max_s, out int max_n);

                    if (min_n > max_n)
                    {
                        arr[2] = $"{min_n}-23,0-{max_n}";
                    }
                }

                if (arr[1].Contains("-"))
                {
                    string min_s = arr[1].Split(new char[] { '-' })[0];
                    string max_s = arr[1].Split(new char[] { '-' })[1];

                    int.TryParse(min_s, out int min_n);
                    int.TryParse(max_s, out int max_n);

                    if (min_n > max_n)
                    {
                        arr[1] = $"{min_n}-59,0-{max_n}";
                    }
                }

                result = arr[1] + " " + arr[2] + " " + arr[3] + " " + arr[4] + " " + arr[5];
            }

            return result;
        }

        /// <summary>
        /// Строка расписания cron для добавления в калькулятор
        /// </summary>
        /// <param name="_cron">строка расписания из deployment</param>
        /// <returns></returns>
        public static string CronToBuilder(string _cron)
        {
            if (string.IsNullOrWhiteSpace(_cron))
            {
                // по умолчанию 1 раз в минуту
                return "* * * * * * *";
            }

            string result = _cron.TrimInner().Trim();

            var arr = result
                    .Split(new char[] { ' ' });

            if (arr.Length == 5)
            {
                result = $"* {result} *";
            }

            arr = result
                    .Replace("?", "*")
                    .Replace("TODO", "*")
                    .Split(new char[] { ' ' });

            result = string.Join(" ", arr);

            return result;
        }
    }

}
