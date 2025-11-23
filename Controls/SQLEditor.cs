// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using Microsoft.SqlServer.Management.XEvent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Редактор SQL
    /// </summary>
    public class SQLEditor : TextEditor
    {
        /// <summary>
        /// Виды кнопок в ToolBar
        /// </summary>
        public enum ToolbarButtonType
        {
            /// <summary>
            /// Открыть еще один экземпляр для запросов
            /// </summary>
            ADD,
            /// <summary>
            /// Очистить окно запроса
            /// </summary>
            NEW,
            /// <summary>
            /// Открыть существующий файл с запросом
            /// </summary>
            OPEN,
            /// <summary>
            /// Сохранить изменения в существующем файле с запросом
            /// </summary>
            SAVE,
            /// <summary>
            /// Сохранить скрипт в файл с новым именем
            /// </summary>
            SAVEAS,
            /// <summary>
            /// Вырезать в буфер выделенную часть текста
            /// </summary>
            CUT,
            /// <summary>
            /// Скопировать в буфер выделенную часть текста
            /// </summary>
            COPY,
            /// <summary>
            /// Вставить из буфера часть текста
            /// </summary>
            PASTE,
            /// <summary>
            /// Разделитель
            /// </summary>
            SELECTOR,
            /// <summary>
            /// Вернуть вставку
            /// </summary>
            REDO,
            /// <summary>
            /// Отменить вставку
            /// </summary>
            UNDO,
            /// <summary>
            /// Найти
            /// </summary>
            SEARCH,
            /// <summary>
            /// Заменить
            /// </summary>
            REPLACE
        }


        string _filename;
        /// <summary>
        /// Загруженный файл
        /// </summary>
        public string Filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;

                if (string.IsNullOrWhiteSpace(_filename))
                {
                    _filename = "";
                }

                // найдем строку для отображения имени файла
                try
                {
                    // найти Label в общем DockPanel
                    FrameworkElement framework = this;
                    while (framework != null && (framework as DockPanel) == null && framework.Parent != null)
                    {
                        framework = framework.Parent as FrameworkElement;
                    }
                    if (framework != null && (framework as DockPanel) != null)
                    {
                        // нашли DockPanel

                        // ищем в нем Label
                        List<System.Windows.Controls.Label> listLabels = new List<System.Windows.Controls.Label>();
                        Utilities.Controls.GetLogicalChildCollection(framework, listLabels);
                        foreach (var item in listLabels.Where(x => x.Name.ToLower().Contains("filename")))
                        {
                            // нашли Label
                            item.Content = _filename.Replace("_", "__");

                            if (string.IsNullOrWhiteSpace(_filename))
                            {
                                item.Background = Brushes.Yellow;
                                item.FontWeight = FontWeights.Bold;
                                item.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                item.Background = Brushes.LightGreen;
                                item.FontWeight = FontWeights.Bold;
                                item.Visibility = Visibility.Visible;
                            }

                            break; //-V3020
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                }
            }
        }

        /// <summary>
        /// панель поиска
        /// </summary>
        SearchPanel searchPanel;

        //==============================================================================================================================================
        /// <summary>
        /// Конструктор
        /// </summary>
        public SQLEditor()
        {
            // Загрузка описания для выделения синтаксиса
            try
            {
                IHighlightingDefinition customHighlighting;
                using (FileStream s = File.Open(Path.Combine(App.AppPath, "SQLHighlighting.xshd"), FileMode.Open))
                {
                    if (s != null)
                    {
                        using (XmlReader reader = new XmlTextReader(s))
                        {
                            customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                                HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        }

                        // and register it in the HighlightingManager
                        HighlightingManager.Instance.RegisterHighlighting("SQL", new string[] { ".sql" }, customHighlighting);
                    }
                }

                using (FileStream s = File.Open(Path.Combine(App.AppPath, "RUS.xshd"), FileMode.Open))
                {
                    if (s != null)
                    {
                        using (XmlReader reader = new XmlTextReader(s))
                        {
                            customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                                HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        }

                        // and register it in the HighlightingManager
                        HighlightingManager.Instance.RegisterHighlighting("RUS", new string[] { ".rus" }, customHighlighting);
                    }
                }

                using (FileStream s = File.Open(Path.Combine(App.AppPath, "LOG.xshd"), FileMode.Open))
                {
                    if (s != null)
                    {
                        using (XmlReader reader = new XmlTextReader(s))
                        {
                            customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                                HighlightingLoader.Load(reader, HighlightingManager.Instance);
                        }

                        // and register it in the HighlightingManager
                        HighlightingManager.Instance.RegisterHighlighting("LOG", new string[] { ".log" }, customHighlighting);
                    }
                }
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
            }

            // панель поиска
            searchPanel = SearchPanel.Install(this);
            searchPanel.MatchCase = false;
            searchPanel.WholeWords = false;

            // показывать номера строк
            this.ShowLineNumbers = true;

            // выделять гиперссылки
            this.Options.EnableEmailHyperlinks = false;
            this.Options.EnableHyperlinks = false;

            // имя файла по умолчанию
            Filename = "";

            // выделение синтаксиса, по умолчанию SQL
            this.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("SQL");

            // параметры панели поиска и замены
            WinFindReplaceDialog.caseSensitive = false;
            WinFindReplaceDialog.wholeWord = false;

            // 
            this.TextChanged += (o, e) =>
            {
                // найдем строку для отображения имени файла
                try
                {
                    // найти Label в общем DockPanel
                    FrameworkElement framework = this;
                    while (framework != null && (framework as DockPanel) == null && framework.Parent != null)
                    {
                        framework = framework.Parent as FrameworkElement;
                    }
                    if (framework != null && (framework as DockPanel) != null)
                    {
                        // нашли DockPanel

                        // ищем в нем Label
                        List<System.Windows.Controls.Label> listLabels = new List<System.Windows.Controls.Label>();
                        Utilities.Controls.GetLogicalChildCollection(framework, listLabels);
                        foreach (var item in listLabels.Where(x => x.Name.ToLower().Contains("filename")))
                        {
                            // нашли Label
                            item.Background = Brushes.Yellow;
                            item.FontWeight = FontWeights.Bold;
                            break; //-V3020
                        }
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                }
            };
        }
       
        //==============================================================================================================================================
        /// <summary>
        /// Добавить кнопку в Toolbar
        /// </summary>
        /// <param name="toolBar">toolbar для кнопок</param>
        /// <param name="buttonType">тип кнопки</param>
        /// <param name="buttonAction">дополнительные действия после нажатия кнопки</param>
        /// <param name="keyControl">элемент управления для KeyDown</param>
        public void AddToolbar(ToolBar toolBar, ToolbarButtonType buttonType, System.Action buttonAction, Control keyControl)
        {
            if (toolBar is null) return;

            Button btn = null;

            switch (buttonType)
            {
                case ToolbarButtonType.SELECTOR:
                    toolBar.Items.Add(new Separator());
                    break;
                case ToolbarButtonType.ADD:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Новое окно для SQL-запроса",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/AddSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            MainWindow.NewQuery(MainWindow.MainConnect, "");
                        };
                    }
                    break;
                case ToolbarButtonType.NEW:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Новый скрипт",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/NewSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            this.Text = "";
                            this.Filename = "";
                        };
                    }
                    break;
                case ToolbarButtonType.OPEN:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Открыть файл (Ctrl + O)",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/OpenSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // добавляем обработчик горячей клавишы
                        if (keyControl != null)
                        {
                            keyControl.KeyDown += (s, e) =>
                            {
                                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.O)
                                {
                                    OpenFile();
                                    e.Handled = true;
                                }
                            };
                        }

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            OpenFile();
                        };
                    }
                    break;
                case ToolbarButtonType.SAVE:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Сохранить файл (Ctrl + S)",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/SaveSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // добавляем обработчик горячей клавишы
                        if (keyControl != null)
                        {
                            keyControl.KeyDown += (s, e) =>
                            {
                                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.S)
                                {
                                    SaveFile();
                                    e.Handled = true;
                                }
                            };
                        }

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            SaveFile();
                        };
                    }
                    break;
                case ToolbarButtonType.SAVEAS:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Сохранить файл с новым именем (F8)",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/SaveAsSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // добавляем обработчик горячей клавишы
                        if (keyControl != null)
                        {
                            keyControl.KeyDown += (s, e) =>
                            {
                                if (e.Key == Key.F8)
                                {
                                    SaveAsFile();
                                    e.Handled = true;
                                }
                            };
                        }

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            SaveAsFile();
                        };
                    }
                    break;
                case ToolbarButtonType.CUT:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Вырезать (Ctrl + X)",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/CutSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // добавляем обработчик горячей клавишы
                        if (keyControl != null)
                        {
                            keyControl.KeyDown += (s, e) =>
                            {
                                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.X)
                                {
                                    this.Cut();
                                    e.Handled = true;
                                }
                            };
                        }

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            this.Cut();
                        };
                    }
                    break;
                case ToolbarButtonType.COPY:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Копировать (Ctrl + C)",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/CopySQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // добавляем обработчик горячей клавишы
                        if (keyControl != null)
                        {
                            keyControl.KeyDown += (s, e) =>
                            {
                                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.C)
                                {
                                    this.Copy();
                                    e.Handled = true;
                                }
                            };
                        }

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            this.Copy();
                        };
                    }
                    break;
                case ToolbarButtonType.PASTE:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Вставить (Ctrl + V)",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/PasteSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // добавляем обработчик горячей клавишы
                        if (keyControl != null)
                        {
                            keyControl.KeyDown += (s, e) =>
                            {
                                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.V)
                                {
                                    this.Paste();
                                    e.Handled = true;
                                }
                            };
                        }

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            this.Paste();
                        };
                    }
                    break;
                case ToolbarButtonType.UNDO:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Отменить вставку (Ctrl + Z)",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/UndoSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // добавляем обработчик горячей клавишы
                        if (keyControl != null)
                        {
                            keyControl.KeyDown += (s, e) =>
                            {
                                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Z)
                                {
                                    this.Undo();
                                    e.Handled = true;
                                }
                            };
                        }

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            this.Undo();
                        };
                    }
                    break;
                case ToolbarButtonType.REDO:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Вернуть вставку (Ctrl + Y)",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/RedoSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // добавляем обработчик горячей клавишы
                        if (keyControl != null)
                        {
                            keyControl.KeyDown += (s, e) =>
                            {
                                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.Y)
                                {
                                    this.Redo();
                                    e.Handled = true;
                                }
                            };
                        }

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            this.Redo();
                        };
                    }
                    break;
                case ToolbarButtonType.SEARCH:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Поиск (Ctrl + F)",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/SearchSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // добавляем обработчик горячей клавишы
                        if (keyControl != null)
                        {
                            keyControl.KeyDown += (s, e) =>
                            {
                                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.F)
                                {
                                    if (searchPanel != null)
                                    {
                                        searchPanel.Open();
                                    }
                                    e.Handled = true;
                                }
                            };
                        }

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            if (searchPanel != null)
                            {
                                searchPanel.Open();
                            }
                        };
                    }
                    break;
                case ToolbarButtonType.REPLACE:
                    {
                        // создаем кнопку
                        btn = new Button()
                        {
                            Height = 24,
                            ToolTip = "Поиск и замена (Ctrl + H или Ctrl + R)",

                            Content = new Image()
                            {
                                Source = new BitmapImage(new Uri("../Images/ReplaceSQL.png", UriKind.Relative)),
                                VerticalAlignment = VerticalAlignment.Center
                            }
                        };

                        // добавляем обработчик горячей клавишы
                        if (keyControl != null)
                        {
                            keyControl.KeyDown += (s, e) =>
                            {
                                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.H)
                                {
                                    WinFindReplaceDialog.ShowForReplace(this, true);
                                    e.Handled = true;
                                }
                                if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.R)
                                {
                                    WinFindReplaceDialog.ShowForReplace(this, true);
                                    e.Handled = true;
                                }
                            };
                        }

                        // Добавляем обработчик нажатия на кнопку
                        btn.Click += (o, e) =>
                        {
                            if (searchPanel != null)
                            {
                                WinFindReplaceDialog.ShowForReplace(this, true);
                            }
                        };
                    }
                    break;
                default:
                    break;
            }

            if (btn != null)
            {
                toolBar.Items.Add(btn);
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // выполнить действие
                if (buttonAction != null)
                {
                    buttonAction();
                }
            });
        }

        //==============================================================================================================================================
        /// <summary>
        /// Заполнить toolbar набором кнопок по умолчанию
        /// </summary>
        /// <param name="toolBar">toolbar для кнопок</param>
        /// <param name="keyControl">элемент управления для KeyDown</param>
        /// <param name="isAddSave">добавить кнопку Сохранить</param>
        /// <param name="isAddSaveAs">добавить кнопку Сохранить как</param>
        public void AddToolbarDefault(ToolBar toolBar, Control keyControl, bool isAddSave, bool isAddSaveAs)
        {
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.SELECTOR, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.NEW, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.OPEN, null, keyControl);
            if (isAddSave)
            {
                this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.SAVE, null, keyControl);
            }
            if (isAddSaveAs)
            {
                this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.SAVEAS, null, keyControl);
            }
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.SELECTOR, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.SEARCH, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.REPLACE, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.CUT, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.COPY, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.PASTE, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.SELECTOR, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.UNDO, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.REDO, null, keyControl);
            this.AddToolbar(toolBar, SQLEditor.ToolbarButtonType.SELECTOR, null, keyControl);
        }

        //==============================================================================================================================================
        /// <summary>
        /// Открыть файл
        /// </summary>
        void OpenFile()
        {
            Filename = "";
            try
            {
                string file = Controls.Dialogs.OpenFileDialog(MainWindow.Task.TaskPath);
                if (File.Exists(file))
                {
                    this.Text = File.ReadAllText(file);
                    Filename = file;
                };
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
        }

        /// <summary>
        /// Сохранить файл с новым именем
        /// </summary>
        void SaveAsFile()
        {
            FileStream fs = null;

            try
            {
                string _path = "";
                string _file = "";

                if (string.IsNullOrWhiteSpace(Filename))
                {
                    _path = MainWindow.Task.TaskPath;
                    _file = "";
                }
                else
                {
                    _path = Path.GetFullPath(Filename);
                    _file = Path.GetFileName(Filename);
                }

                // имя файла для скрипта
                _file = Controls.Dialogs.SaveSQLDialog(_path, _file, out fs, out FileMode fileMode);

                //сохранить файл
                if (fs != null)
                {
                    Filename = "";
                    Utilities.Files.WriteScript(_file, fs, this.Text, false, out string err, fileMode);
                    Filename = _file;
                }
            }
            finally
            {
                if (fs != null) fs.Dispose();
            }
        }

        /// <summary>
        /// Сохранить файл
        /// </summary>
        void SaveFile()
        {
            if (!string.IsNullOrWhiteSpace(Filename))
            {
                string _file = Filename;
                Filename = "";
                Utilities.Files.WriteScript(_file, null, this.Text, false, out string err, FileMode.Create);
                Filename = _file;
            }
            else
            {
                SaveAsFile();
            }
        }

    }
}
