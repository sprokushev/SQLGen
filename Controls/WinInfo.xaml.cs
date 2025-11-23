// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SQLGen.Controls;
using SQLGen.Utilities;
using static SQLGen.App;

namespace SQLGen
{
    /// <summary>
    /// Форма для вывода информационных сообщений
    /// </summary>
    public partial class WinInfo : Window
    {
        /// <summary>Конструктор WinInfo</summary>
        public WinInfo(string _logfile)
        {
            InitializeComponent();

            LogFile = _logfile;

            // пользовательские настройки GUI
            Default.InitGUI(
                "WinInfo",
                this,
                mainGrid,
                new List<System.Windows.Controls.Control> { tbInfo },
                new List<ComboBox> { cbInfoFontFamily },
                new List<ComboBox> { cbInfoFontSize },
                LogFile
                );

            // заполняем toolbar'ы
            //tbInfo.AddToolbarDefault(toolbarInfo, null, true);

            // отключаем выделение синтаксиса
            tbInfo.SyntaxHighlighting = null;
        }

        /// <summary>
        ///  Полный путь к файлу для лога выполнения
        /// </summary>
        private string LogFile = "";

        /// <summary>
        /// Вернуть лог 
        /// </summary>
        /// <returns></returns>
        public string GetLog()
        {
            string result = "";

            if (tbInfo != null)
            {
                result = tbInfo.Text;
            }

            return result;
        }

        /// <summary>
        /// записать лог в LogFile
        /// </summary>
        private void SaveLog()
        {
            if (!string.IsNullOrWhiteSpace(LogFile) && !string.IsNullOrWhiteSpace(tbInfo.Text))
            {
                // Контроль размера лог-файла
                Files.CutEndFileMaxSize(LogFile);

                try
                {
                    // Дописываем в конец лог-файла
                    File.AppendAllText(LogFile, Environment.NewLine + Environment.NewLine + tbInfo.Text);
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, ShowMessageMode.SHOW, false, null);
                }
            }
        }

        /// <summary>Нажата кнопка Ok</summary>
        private void btOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void tbInfo_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void tbInfo_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.End)
            {
                tbInfo.ScrollToEnd();
                e.Handled = true;
            }
            else if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Home)
            {
                tbInfo.ScrollToHome();
                e.Handled = true;
            }
            else if (e.Key == Key.PageUp)
            {
                tbInfo.PageUp();
                e.Handled = true;
            }
            else if (e.Key == Key.PageDown)
            {
                tbInfo.PageDown();
                e.Handled = true;
            }
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            tbInfo.Focus();
        }

        // просмотреть во внешнем редакторе
        private void btExternal_Click(object sender, RoutedEventArgs e)
        {
            string filename = "";
            try
            {
                if (System.IO.File.Exists(this.Title))
                {
                    filename = this.Title;
                }
            }
            catch (Exception)
            {
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                // имя временного файла для скрипта
                var generator = new RandomGenerator();
                filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), generator.RandomString(8) + ".tmp");
            }

            // создаем и заполняем временный файл
            Encoding encoding = new UTF8Encoding(false);
            System.IO.File.WriteAllText(filename, tbInfo.Text, encoding);

            // открываем файл
            Utilities.External.OpenExternalFile(filename);

            // удаляем временный файл
            /*
            try
            {
                if (System.IO.File.Exists(filename)) System.IO.File.Delete(filename);
            }
            catch
            {
            }
            */

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            SaveLog();

            // пользовательские настройки GUI
            Default.SaveGUI(
                "WinInfo",
                this,
                new List<System.Windows.Controls.Control> { tbInfo }
                );
        }
    }
}
