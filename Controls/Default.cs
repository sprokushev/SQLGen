// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using AngleSharp.Dom;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using SQLGen.Utilities;

namespace SQLGen.Controls
{
    /// <summary>
    /// Базовые функции для работы с элементами интерфейса
    /// </summary>
    public static class Default
    {
        /// <summary>
        /// Флаг сброса настроек GUI на значения по умолчанию
        /// </summary>
        public static bool isResetGUI = false;

        /// <summary>
        /// настраиваем окно с учетом сохраненных ранее значений (WPF)
        /// </summary>
        /// <param name="windowName">название окна</param>
        /// <param name="window">ссылка на экземпляр Window</param>
        /// <param name="mainGrid">ссылка на экземпляр Grid</param>
        /// <param name="listScript">список боксов со скриптами</param>
        /// <param name="listFontFamily">список боксов с названием шрифтов</param>
        /// <param name="listFontSize">список боксов с размерами шрифтов</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static void InitGUI(
            string windowName,
            System.Windows.Window window,
            System.Windows.Controls.Grid mainGrid,
            List<System.Windows.Controls.Control> listScript,
            List<System.Windows.Controls.ComboBox> listFontFamily,
            List<System.Windows.Controls.ComboBox> listFontSize,
            string logFile
            )
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            App.AddLog($"--------------------------------------------------------------", null, App.ShowMessageMode.NONE, true, logFile);
            App.AddLog($"Инициализация {windowName}", null, App.ShowMessageMode.NONE, true, logFile);
            App.AddLog($"--------------------------------------------------------------", null, App.ShowMessageMode.NONE, true, logFile);

            isResetGUI = false;

            if (
                (window != null) &&
                (mainGrid != null) &&
                (!string.IsNullOrWhiteSpace(windowName))
                )
            {
                // для textbox с редактированием
                if (listFontFamily != null)
                {
                    foreach (var item in listFontFamily)
                    {
                        item.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
                    }

                    // устанавливаем шрифты по умолчанию в окнах со скриптами
                    if (listScript != null)
                    {
                        foreach (var item in listFontFamily)
                        {
                            try
                            {
                                var _script = listScript.First();

                                if (
                                    (_script != null) &&
                                    ((_script as System.Windows.Controls.RichTextBox) != null)
                                )
                                {
                                    item.SelectedItem = (_script as System.Windows.Controls.RichTextBox).Document.FontFamily;
                                }

                                if (
                                    (_script != null) &&
                                    ((_script as TextEditor) != null)
                                )
                                {
                                    item.SelectedItem = (_script as TextEditor).FontFamily;
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }

                if (listFontSize != null)
                {
                    foreach (var item in listFontSize)
                    {
                        item.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
                    }

                    // устанавливаем размеры шрифтов по умолчанию в окнах со скриптами
                    if (listScript != null)
                    {
                        foreach (var item in listFontSize)
                        {
                            try
                            {
                                var _script = listScript.First();

                                if (
                                    (_script != null) &&
                                    ((_script as System.Windows.Controls.RichTextBox) != null)
                                )
                                {
                                    item.Text = (_script as System.Windows.Controls.RichTextBox).Document.FontSize.ToString();
                                }

                                if (
                                    (_script != null) &&
                                    ((_script as TextEditor) != null)
                                )
                                {
                                    item.Text = (_script as TextEditor).FontSize.ToString();
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }

                if (listScript != null)
                {
                    for (int i = 0; i < listScript.Count(); i++)
                    {
                        listScript[i].TabIndex = 1;
                    }
                }

                // устанавливаем масштаб окна
                var nameScope = NameScope.GetNameScope(window);
                var scaleTransform = new ScaleTransform();
                scaleTransform.ScaleX = MainWindow.APPinfo.GUI.scaleWindow.ScaleX;
                scaleTransform.ScaleY = MainWindow.APPinfo.GUI.scaleWindow.ScaleY;
                nameScope.RegisterName("st", scaleTransform);

                var transformGroup = new TransformGroup();
                transformGroup.Children.Add(scaleTransform);
                mainGrid.LayoutTransform = transformGroup;

                // устанавливаем шрифты в textbox редактирования
                if (
                    (listScript != null) &&
                    (listFontFamily != null) &&
                    (listFontFamily.Count == listScript.Count) &&
                    (listFontSize != null) &&
                    (listFontSize.Count == listScript.Count)
                )
                {
                    for (int i = 0; i < listScript.Count(); i++)
                    {
                        try
                        {
                            var _font = new System.Windows.Media.FontFamily(MainWindow.APPinfo.GUI.scriptBoxDefault.ScriptBoxFont);
                            listFontFamily[i].SelectedItem = _font;
                            if ((listScript[i] as System.Windows.Controls.RichTextBox) != null)
                            {
                                (listScript[i] as System.Windows.Controls.RichTextBox).Document.FontFamily = _font;
                            }
                            if ((listScript[i] as TextEditor) != null)
                            {
                                (listScript[i] as TextEditor).FontFamily = _font;
                            }

                            double d = 0;
                            double.TryParse(MainWindow.APPinfo.GUI.scriptBoxDefault.ScriptBoxFontSize, out d);
                            if (d > 0)
                            {
                                listFontSize[i].Text = MainWindow.APPinfo.GUI.scriptBoxDefault.ScriptBoxFontSize;
                                if ((listScript[i] as System.Windows.Controls.RichTextBox) != null)
                                {
                                    (listScript[i] as System.Windows.Controls.RichTextBox).Document.FontSize = d;
                                }
                                if ((listScript[i] as TextEditor) != null)
                                {
                                    (listScript[i] as TextEditor).FontSize = d;
                                }
                            }

                            // Добавляем обработчики на выбор шрифта для textbox редактирования
                            listFontFamily[i].SelectionChanged += (o, e) =>
                            {
                                var cb = o as System.Windows.Controls.ComboBox;

                                if (cb.SelectedItem != null)
                                {
                                    try
                                    {
                                        // найти окно-редактор в общем DockPanel
                                        FrameworkElement framework = cb;
                                        while (framework != null && (framework as DockPanel) == null && framework.Parent != null)
                                        {
                                            framework = framework.Parent as FrameworkElement;
                                        }
                                        if (framework != null && (framework as DockPanel) != null)
                                        {
                                            // нашли DockPanel

                                            // ищем в нем RichTextBox
                                            List<System.Windows.Controls.RichTextBox> listBox = new List<System.Windows.Controls.RichTextBox>();
                                            Utilities.Controls.GetLogicalChildCollection(framework, listBox);
                                            foreach (var item in listBox)
                                            {
                                                // нашли RichTextBox
                                                item.Document.FontFamily = (System.Windows.Media.FontFamily)cb.SelectedItem;
                                            }

                                            // ищем в нем TextEditor
                                            List<TextEditor> listEditor = new List<TextEditor>();
                                            Utilities.Controls.GetLogicalChildCollection(framework, listEditor);
                                            foreach (var item in listEditor)
                                            {
                                                // нашли TextEditor
                                                item.FontFamily = (System.Windows.Media.FontFamily)cb.SelectedItem;
                                            }

                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                                    }
                                }
                            };

                            listFontSize[i].AddHandler(System.Windows.Controls.Primitives.TextBoxBase.TextChangedEvent, new RoutedEventHandler((o, e) =>
                            {
                                var cb = o as System.Windows.Controls.ComboBox;

                                if (
                                    (!string.IsNullOrWhiteSpace(cb.Text)) &&
                                    double.TryParse(cb.Text, out double dd)
                                )
                                {
                                    // найти окно-редактор в общем DockPanel
                                    FrameworkElement framework = cb;
                                    while (framework != null && (framework as DockPanel) == null && framework.Parent != null)
                                    {
                                        framework = framework.Parent as FrameworkElement;
                                    }
                                    if (framework != null && (framework as DockPanel) != null)
                                    {
                                        // нашли DockPanel

                                        // ищем в нем RichTextBox
                                        List<System.Windows.Controls.RichTextBox> listBox = new List<System.Windows.Controls.RichTextBox>();
                                        Utilities.Controls.GetLogicalChildCollection(framework, listBox);
                                        foreach (var item in listBox)
                                        {
                                            // нашли RichTextBox
                                            item.Document.FontSize = dd;
                                        }

                                        // ищем в нем TextEditor
                                        List<TextEditor> listEditor = new List<TextEditor>();
                                        Utilities.Controls.GetLogicalChildCollection(framework, listEditor);
                                        foreach (var item in listEditor)
                                        {
                                            // нашли TextEditor
                                            item.FontSize = dd;
                                        }
                                    }
                                }
                            }));

                        }
                        catch (Exception)
                        {
                        }
                    }
                }

                // Найдем настройки окна
                var found = MainWindow.APPinfo.GUI.ListWindows.Where(x => x.Name.ToLower() == windowName.ToLower()).FirstOrDefault();
                if (found != null)
                {
                    // устанавливаем размеры окна
                    window.WindowState = found.State;
                    if (found.Height > 1)
                    {
                        var _new = found.ScaleHeight(MainWindow.APPinfo.GUI.scaleWindow);
                        if (_new > window.Height) window.Height = _new;
                    }
                    if (found.Width > 1)
                    {
                        var _new = found.ScaleWidth(MainWindow.APPinfo.GUI.scaleWindow);
                        if (_new > window.Width) window.Width = _new;
                    }

                    // устанавливаем шрифт в textbox редактирования
                    foreach (var scriptbox in found.ListScriptBox)
                    {
                        if (
                            (!string.IsNullOrWhiteSpace(scriptbox.ScriptBoxName)) &&
                            (scriptbox.ScriptBoxName != "default") &&
                            (listScript != null) &&
                            (listFontFamily != null) &&
                            (listFontFamily.Count == listScript.Count) &&
                            (listFontSize != null) &&
                            (listFontSize.Count == listScript.Count)
                            )
                        {
                            // ищем textbox редактирования, устанавливаем шрфит и размер
                            for (int i = 0; i < listScript.Count(); i++)
                            {
                                if (listScript[i].Name.ToLower() == scriptbox.ScriptBoxName.ToLower())
                                {
                                    try
                                    {
                                        var _font = new System.Windows.Media.FontFamily(scriptbox.ScriptBoxFont);
                                        listFontFamily[i].SelectedItem = _font;
                                        if ((listScript[i] as System.Windows.Controls.RichTextBox) != null)
                                        {
                                            (listScript[i] as System.Windows.Controls.RichTextBox).Document.FontFamily = _font;
                                        }
                                        if ((listScript[i] as TextEditor) != null)
                                        {
                                            (listScript[i] as TextEditor).FontFamily = _font;
                                        }

                                        double d = 0;
                                        double.TryParse(scriptbox.ScriptBoxFontSize, out d);
                                        if (d > 0)
                                        {
                                            listFontSize[i].Text = scriptbox.ScriptBoxFontSize;

                                            if ((listScript[i] as System.Windows.Controls.RichTextBox) != null)
                                            {
                                                (listScript[i] as System.Windows.Controls.RichTextBox).Document.FontSize = d;
                                            }

                                            if ((listScript[i] as TextEditor) != null)
                                            {
                                                (listScript[i] as TextEditor).FontSize = d;
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                    }

                                }
                            }
                        }
                    }
                }

                // Добавляем обработчик событий мыши для всех Button и CheckBox в окне
                List<System.Windows.Controls.Button> listButtons = new List<System.Windows.Controls.Button>();
                Utilities.Controls.GetLogicalChildCollection(window, listButtons);
                foreach (var item in listButtons)
                {
                    item.MouseEnter += new MouseEventHandler(wpf_MouseEnterEvent);
                    item.MouseLeave += new MouseEventHandler(wpf_MouseLeaveEvent);
                }

                List<Fluent.Button> listButtons2 = new List<Fluent.Button>();
                Utilities.Controls.GetLogicalChildCollection(window, listButtons2);
                foreach (var item in listButtons2)
                {
                    item.MouseEnter += new MouseEventHandler(wpf_MouseEnterEvent);
                    item.MouseLeave += new MouseEventHandler(wpf_MouseLeaveEvent);
                }

                List<System.Windows.Controls.CheckBox> listCheckBox = new List<System.Windows.Controls.CheckBox>();
                Utilities.Controls.GetLogicalChildCollection(window, listCheckBox);
                foreach (var item in listCheckBox)
                {
                    item.MouseEnter += new MouseEventHandler(wpf_MouseEnterEvent);
                    item.MouseLeave += new MouseEventHandler(wpf_MouseLeaveEvent);
                }

                List<Fluent.CheckBox> listCheckBox2 = new List<Fluent.CheckBox>();
                Utilities.Controls.GetLogicalChildCollection(window, listCheckBox2);
                foreach (var item in listCheckBox2)
                {
                    item.MouseEnter += new MouseEventHandler(wpf_MouseEnterEvent);
                    item.MouseLeave += new MouseEventHandler(wpf_MouseLeaveEvent);
                }
            }
        }

        /// <summary>
        /// настраиваем окно с учетом сохраненных ранее значений (WinForm)
        /// </summary>
        /// <param name="windowName">название окна</param>
        /// <param name="window">ссылка на экземпляр Window</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static void InitGUI(
            string windowName,
            System.Windows.Forms.Form window,
            string logFile
            )
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            App.AddLog($"--------------------------------------------------------------", null, App.ShowMessageMode.NONE, true, logFile);
            App.AddLog($"Инициализация {windowName}", null, App.ShowMessageMode.NONE, true, logFile);
            App.AddLog($"--------------------------------------------------------------", null, App.ShowMessageMode.NONE, true, logFile);

            isResetGUI = false;

            if (
                (window != null) &&
                (!string.IsNullOrWhiteSpace(windowName))
                )
            {
                // устанавливаем масштаб окна
                window.Font = new System.Drawing.Font(window.Font.Name, (float)(window.Font.Size * MainWindow.APPinfo.GUI.scaleWindow.ScaleX));
                //window.Height = (int)(window.Height * MainWindow.APPinfo.GUI.scaleWindow.ScaleY);
                //window.Width = (int)(window.Width * MainWindow.APPinfo.GUI.scaleWindow.ScaleX);

                // Найдем настройки окна
                var found = MainWindow.APPinfo.GUI.ListWindows.Where(x => x.Name.ToLower() == windowName.ToLower()).FirstOrDefault();
                if (found != null)
                {
                    // устанавливаем размеры окна
                    if (found.Height > 1)
                    {
                        int _new = (int)found.ScaleHeight(MainWindow.APPinfo.GUI.scaleWindow);
                        if (_new > window.Height) window.Height = _new;
                    }
                    if (found.Width > 1)
                    {
                        int _new = (int)found.ScaleWidth(MainWindow.APPinfo.GUI.scaleWindow);
                        if (_new > window.Width) window.Width = _new;
                    }
                }

                // Добавляем обработчик событий мыши для всех Button и CheckBox в окне
                List<System.Windows.Forms.Button> listButtons = window.Controls.OfType<System.Windows.Forms.Button>().ToList();
                foreach (var item in listButtons)
                {
                    item.MouseEnter += new System.EventHandler(form_MouseEnterEvent);
                    item.MouseLeave += new System.EventHandler(form_MouseLeaveEvent);
                }

                List<System.Windows.Forms.CheckBox> listCheckBox = window.Controls.OfType<System.Windows.Forms.CheckBox>().ToList();
                foreach (var item in listCheckBox)
                {
                    item.MouseEnter += new System.EventHandler(form_MouseEnterEvent);
                    item.MouseLeave += new System.EventHandler(form_MouseLeaveEvent);
                }
            }
        }

        /// <summary>
        /// сохраняем настройки окна
        /// </summary>
        /// <param name="windowName">имя окна</param>
        /// <param name="window">экземпляр System.Windows.Forms.Form</param>
        /// <param name="listScript">список редакторов текста</param>
        public static void SaveGUI(
            string windowName,
            System.Windows.Window window,
            List<System.Windows.Controls.Control> listScript
            )
        {
            if (
                (window != null) &&
                (!string.IsNullOrWhiteSpace(windowName)) &&
                (!isResetGUI)
                )
            {
                // Найдем настройки окна
                var found = MainWindow.APPinfo.GUI.ListWindows.Where(x => x.Name.ToLower() == windowName.ToLower()).FirstOrDefault();
                if (found == null)
                {
                    // настройки окна не найдены, добавим
                    found = new WindowInfo();
                    found.Name = windowName;
                    MainWindow.APPinfo.GUI.ListWindows.Add(found);
                }

                // сохраняем состояние окна
                found.State = window.WindowState;

                // сохраняем размеры
                found.SetHeight(MainWindow.APPinfo.GUI.scaleWindow, window.Height);
                found.SetWidth(MainWindow.APPinfo.GUI.scaleWindow, window.Width);

                // сохраняем шрифт и размер из textbox редактирования
                if (listScript != null)
                {
                    foreach (var scriptBox in listScript)
                    {
                        var infoBox = found.ListScriptBox.Where(x => x.ScriptBoxName.ToLower() == scriptBox.Name.ToLower()).FirstOrDefault();
                        if (infoBox == null)
                        {
                            infoBox = new ScriptBoxInfo();
                            infoBox.ScriptBoxName = scriptBox.Name;
                            found.ListScriptBox.Add(infoBox);
                        }

                        if ((scriptBox as System.Windows.Controls.RichTextBox) != null)
                        {
                            infoBox.ScriptBoxFont = (scriptBox as System.Windows.Controls.RichTextBox).Document.FontFamily.ToString();
                            infoBox.ScriptBoxFontSize = (scriptBox as System.Windows.Controls.RichTextBox).Document.FontSize.ToString();
                        }

                        if ((scriptBox as TextEditor) != null)
                        {
                            infoBox.ScriptBoxFont = (scriptBox as TextEditor).FontFamily.ToString();
                            infoBox.ScriptBoxFontSize = (scriptBox as TextEditor).FontSize.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// сохраняем настройки окна
        /// </summary>
        /// <param name="windowName">имя окна</param>
        /// <param name="window">экземпляр System.Windows.Forms.Form</param>
        public static void SaveGUI(string windowName, System.Windows.Forms.Form window)
        {
            if (
                (window != null) &&
                (!string.IsNullOrWhiteSpace(windowName)) &&
                (!isResetGUI)
                )
            {
                // Найдем настройки окна
                var found = MainWindow.APPinfo.GUI.ListWindows.Where(x => x.Name.ToLower() == windowName.ToLower()).FirstOrDefault();
                if (found == null)
                {
                    // настройки окна не найдены, добавим
                    found = new WindowInfo();
                    found.Name = windowName;
                    MainWindow.APPinfo.GUI.ListWindows.Add(found);
                }

                // сохраняем размеры
                found.SetHeight(MainWindow.APPinfo.GUI.scaleWindow, window.Height);
                found.SetWidth(MainWindow.APPinfo.GUI.scaleWindow, window.Width);
            }
        }

        /// <summary>
        /// Курсор мыши над кнопкой (WPF)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public static void wpf_MouseEnterEvent(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (
               (sender is System.Windows.Controls.Button) ||
               (sender is System.Windows.Controls.CheckBox)
               )
            {
                System.Windows.Controls.Control ctrl = sender as System.Windows.Controls.Control;
                ctrl.FontWeight = FontWeights.Bold;
            }

            /*if (sender is System.Windows.Controls.Button)
            {
                var ctrl = sender as System.Windows.Controls.Button;
                ctrl.Background = MainWindow.MouseOverBrush;
            }*/
        }

        /// <summary>
        /// Курсор мыши над кнопкой (WinForms)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public static void form_MouseEnterEvent(object sender, EventArgs e)
        {
            if (
               (sender is System.Windows.Forms.Button) ||
               (sender is System.Windows.Forms.CheckBox)
               )
            {
                System.Windows.Forms.Control ctrl = sender as System.Windows.Forms.Control;
                ctrl.Font = new System.Drawing.Font(ctrl.Font.Name, ctrl.Font.Size, System.Drawing.FontStyle.Bold);
            }

            /*if (sender is System.Windows.Controls.Button)
            {
                var ctrl = sender as System.Windows.Controls.Button;
                ctrl.Background = MainWindow.MouseOverBrush;
            }*/
        }

        /// <summary>
        /// Курсор мыши вышел из области кнопки (WPF)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public static void wpf_MouseLeaveEvent(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (
                (sender is System.Windows.Controls.Button) ||
                (sender is System.Windows.Controls.CheckBox)
                )
            {
                var ctrl = sender as System.Windows.Controls.Control;
                ctrl.FontWeight = FontWeights.Normal;
            }

            /*if (sender is System.Windows.Controls.Button)
            {
                var ctrl = sender as System.Windows.Controls.Button;
                ctrl.Background = new SolidColorBrush(Color.FromRgb(229, 229, 229));
            }*/
        }

        /// <summary>
        /// Курсор мыши вышел из области кнопки (WinForms)
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">event</param>
        public static void form_MouseLeaveEvent(object sender, EventArgs e)
        {
            if (
                (sender is System.Windows.Forms.Button) ||
                (sender is System.Windows.Forms.CheckBox)
                )
            {
                var ctrl = sender as System.Windows.Forms.Control;
                ctrl.Font = new System.Drawing.Font(ctrl.Font.Name, ctrl.Font.Size, System.Drawing.FontStyle.Regular);
            }

            /*if (sender is System.Windows.Controls.Button)
            {
                var ctrl = sender as System.Windows.Controls.Button;
                ctrl.Background = new SolidColorBrush(Color.FromRgb(229, 229, 229));
            }*/
        }
    }
}
