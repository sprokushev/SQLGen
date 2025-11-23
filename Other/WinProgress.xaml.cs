// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Windows;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Interaction logic for WinProgress.xaml
    /// </summary>
    public partial class WinProgress : Window
    {
        /// <summary>
        /// Инициализация окна WinProgress
        /// </summary>
        public WinProgress()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            //Default.InitGUI("WinProgress", this, mainGrid, null, null, null);
        }

        /// <summary>
        /// Закрытие окна WinProgress
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closed(object sender, EventArgs e)
        {
            // пользовательские настройки GUI
            //Default.SaveGUI("WinProgress", this, null);
        }
    }
}
