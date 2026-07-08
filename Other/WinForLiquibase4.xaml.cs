// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Highlighting;
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

namespace SQLGen
{
    /// <summary>
    /// Окно выгрузки процедур и представлений из БД
    /// </summary>
    public partial class WinForLiquibase4 : Window
    {
        /// <summary>ссылка на экземпляр основного окна программы</summary>
        public MainWindow mainWindow;

        /// <summary>
        /// признак выполнения через командную строку
        /// </summary>
        private bool isAutoChange = false;

        /// <summary>
        /// флаг выполнения git pull
        /// </summary>
        bool isRefreshed = false;

        /// <summary>Конструктор WinForLiquibase4</summary>
        public WinForLiquibase4()
        {
            InitializeComponent();

            cbIncludeTask.IsChecked = true;
            cbIncludePrevVersion.IsChecked = true;
            rbLiquibase4.IsChecked = true;

            // пользовательские настройки GUI
            Default.InitGUI("WinForLiquibase4", this, mainGrid, null, null, null, MainWindow.Task.LogFile);
        }

        /// <summary>При открытии окна WinForLiquibase4</summary>
        private void winForLiquibase4_Activated(object sender, EventArgs e)
        {
            this.Title = "Обновить yml-файлы в проекте GIT для совместимости с Liquibase 4";
        }

        /// <summary>При закрытии окна WinForLiquibase4</summary>
        private void winForLiquibase4_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("WinForLiquibase4", this, null);
        }

        // выполнить git pull, если он еще не сделан
        private void GitPull()
        {
            if (
                (!isRefreshed) &&
                (cbGITProject != null) &&
                (cbGITProject.SelectedIndex != -1)
                )
            {
                string project = "";
                ComboBoxItem cbItem = null;
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem; //-V3022
                if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                else project = cbItem.Tag.ToString();

                string err = "";
                string branch = GIT.GitCurrentBranch(project, out err, MainWindow.Task.LogFile);

                btChangeBranch.Content = branch.Replace("_", "__");
                btChangeBranch.IsEnabled = Utilities.GITProjects.IsDEVProject(project);

                // git pull
                GIT.GitPull(new string[] { project }, branch, false, false, false, MainWindow.Task.LogFile, false);

                isRefreshed = true;
            }
        }


        private void ChangeClick()
        {
            if (!File.Exists(tbYMLFile.Text))
            {
                MessageBox.Show($"Файл {tbYMLFile.Text} не существует");
                return;
            }

            GitPull();

            if (File.Exists(tbYMLFile.Text))
            {
                // загружаем yml-файл
                YMLStruct yml = new YMLStruct(null, MainWindow.Task.LogFile);
                List<string> versions = new List<string>();
                yml.LoadYMLByFilepath(tbYMLFile.Text, cbIncludePrevVersion.IsChecked == true, versions, cbIncludeTask.IsChecked == true, cbIncludeTask.IsChecked == true);

                if (yml.IsFileExist)
                {
                    // сохраняем yml-файл
                    yml.SaveYML(cbIncludePrevVersion.IsChecked == true, cbIncludeTask.IsChecked == true, rbLiquibase3.IsChecked == true, false, cbIncludeTask.IsChecked == true, false, "", false, "");
                }

                if (isAutoChange)
                {
                    Application.Current.Shutdown();
                }
                else
                {
                    App.AddLog($"Файл {tbYMLFile.Text} сохранен", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }
        }

        /// <summary>Нажата кнопка Изменить</summary>
        private void btChange_Click(object sender, RoutedEventArgs e)
        {
            if ((cbGITProject == null) || (cbGITProject.SelectedIndex == -1)) return;

            if (System.Windows.Forms.MessageBox.Show($"Изменить файл {tbYMLFile.Text} ?",
               "ВНИМАНИЕ",
               System.Windows.Forms.MessageBoxButtons.YesNo
                ) == System.Windows.Forms.DialogResult.Yes
            )
            {
                ChangeClick();
            }
        }

        /// <summary>Нажата кнопка Выбрать yml-файл</summary>
        private void btOpen_Click(object sender, RoutedEventArgs e)
        {
            // выбранный проект
            ComboBoxItem cbItem = null;
            string GITProject = "";
            if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem;
            if ((cbItem == null) || (cbItem.Tag == null)) GITProject = "";
            else GITProject = cbItem.Tag.ToString();

            // выбор файла
            string file = Dialogs.OpenYMLDialog(Path.Combine(MainWindow.APPinfo.GITFolder, GITProject, "version"));
            if (!string.IsNullOrWhiteSpace(file))
            {
                tbYMLFile.Text = file;
            }
        }

        /// <summary>
        /// Выбор проекта GIT
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public void cbGITProject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((cbGITProject != null) && (cbGITProject.SelectedIndex != -1))
            {
                string project = "";
                ComboBoxItem cbItem = null;
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem; //-V3022
                if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                else project = cbItem.Tag.ToString();

                string err = "";
                string branch = GIT.GitCurrentBranch(project, out err, MainWindow.Task.LogFile);

                btChangeBranch.Content = branch.Replace("_", "__");
                btChangeBranch.IsEnabled = Utilities.GITProjects.IsDEVProject(project);

                isRefreshed = false;
            }
            else
            {
                btChangeBranch.Content = "";
                btChangeBranch.IsEnabled = false;
            }
        }

        /// <summary>
        /// Нажата кнопка по смене ветки
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        private void btChangeBranch_Click(object sender, RoutedEventArgs e)
        {
            if ((cbGITProject != null) && (cbGITProject.SelectedIndex != -1))
            {
                string project = "";
                ComboBoxItem cbItem = null;
                if (cbGITProject != null) cbItem = (ComboBoxItem)cbGITProject.SelectedItem; //-V3022
                if ((cbItem == null) || (cbItem.Tag == null)) project = "";
                else project = cbItem.Tag.ToString();

                // переключение на выбранную ветку
                if (GIT.SelectGITBranch(project, null, out string branch, MainWindow.Task.LogFile, true, false, ""))
                {
                    isRefreshed = true;

                    // Показать ветку
                    btChangeBranch.Content = branch.Replace("_", "__");
                    btChangeBranch.IsEnabled = Utilities.GITProjects.IsDEVProject(project);
                }
                else
                {
                    cbGITProject_SelectionChanged(null, null);
                }
            }
        }

        private void btGitPull_Click(object sender, RoutedEventArgs e)
        {
            isRefreshed = false;
            GitPull();
        }


        /// <summary>
        /// Выполнение на основании командной строки
        /// </summary>
        public void AutoChange()
        {
            if (App.Args.Length >= 3)
            {
                tbYMLFile.Text = App.Args[2];
            }
            if (App.Args.Length >= 4)
            {
                cbIncludeTask.IsChecked = App.Args[3].ToLower() == "true";
            }
            if (App.Args.Length >= 5)
            {
                cbIncludePrevVersion.IsChecked = App.Args[4].ToLower() == "true";
            }

            isAutoChange = true;

            ChangeClick();
        }
    }
}
