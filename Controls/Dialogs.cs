// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLGen.Utilities;

namespace SQLGen.Controls
{
    /// <summary>
    /// Диалоговые окна для работы с файлами
    /// </summary>
    public static class Dialogs
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для сохранения JSON-файла</summary>
        /// <param name="defaultext">Расширение по умолчанию, например: .sql</param>
        /// <param name="filter">Список возможных расширений, например: (*.sql)|*.sql|Все файлы (*.*)|*.*</param>
        /// <param name="path">Начальный каталог</param>
        /// <param name="filename">Начальное имя файла</param>
        /// <param name="isCreateFS">=true - создавать экземпляр FileStream для последующей записи в файл</param>
        /// <param name="FS">возвращаемый параметр с экземпляром FileStream для последующей записи в файл</param>
        /// <param name="fileMode">как был открыт файл (новый или добавление)</param>
        /// <param name="isPathCanChanged">=true - путь и имя файла можно изменить</param>
        /// <param name="isForceSave">=true - перезаписывать файл без вопросов</param>
        /// <returns>Итоговое имя файла</returns>
        public static string SaveDialog(string defaultext, string filter, string path, string filename, bool isCreateFS, out FileStream FS, out FileMode fileMode, bool isPathCanChanged, bool isForceSave)
        {
            bool result = true;
            FS = null;
            fileMode = FileMode.Create;

            if (isPathCanChanged)
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
                dlg.DefaultExt = defaultext; // Default file extension
                dlg.Filter = filter; // Filter files by extension
                dlg.CheckFileExists = false;
                dlg.OverwritePrompt = false;
                dlg.InitialDirectory = path;
                if (path == "") dlg.InitialDirectory = Path.GetDirectoryName(filename);
                dlg.FileName = Path.GetFileName(filename); // Default file name

                // Show save file dialog box
                result = dlg.ShowDialog() == true;

                filename = dlg.FileName;
            }
            else
            {
                filename = Path.Combine(path, filename);

                if (path != "")
                {
                    try
                    {
                        Directory.CreateDirectory(path);
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("Ошибка при создании каталога " + path, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                        return "";
                    }
                }
            }

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                if (File.Exists(filename))
                {
                    if (isForceSave || System.Windows.Forms.MessageBox.Show("Файл " + filename + " уже существует!" + Environment.NewLine + "Перезаписать ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        App.AddLog("Файл " + filename + " уже существует, выбрана перезапись", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        fileMode = FileMode.Create;
                    }
                    else if (System.Windows.Forms.MessageBox.Show("Добавить в конец существующего файла ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        App.AddLog("Файл " + filename + " уже существует, выбрано добавление в конец существующего файла", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        fileMode = FileMode.Append;
                    }
                    else
                    {
                        App.AddLog("Файл " + filename + " уже существует, пользователь отказался от сохранения", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        return "";
                    }
                }

                if (isCreateFS)
                {
                    FS = new FileStream(filename, fileMode);
                }

                return filename;
            }

            return "";
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для сохранения SQL-файла</summary>
        /// <param name="path">Начальный каталог</param>
        /// <param name="filename">Начальное имя файла</param>
        /// <param name="fs">возвращаемый параметр с экземпляром FileStream для последующей записи в файл</param>
        /// <param name="fileMode">как был открыт файл (новый или добавление)</param>
        /// <returns>Итоговое имя файла</returns>
        public static string SaveSQLDialog(string path, string filename, out FileStream fs, out FileMode fileMode)
        {
            return SaveDialog(".sql", "(*.sql)|*.sql|Все файлы (*.*)|*.*", path, filename, true, out fs, out fileMode, true, false);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для сохранения ZIP-файла</summary>
        /// <param name="path">Начальный каталог</param>
        /// <param name="filename">Начальное имя файла</param>
        /// <returns>Итоговое имя файла</returns>
        public static string SaveZIPDialog(string path, string filename)
        {
            return SaveDialog(".zip", "(*.zip)|*.zip|Все файлы (*.*)|*.*", path, filename, false, out FileStream fs, out FileMode fileMode, true, true);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для сохранения CSV-файла</summary>
        /// <param name="path">Начальный каталог</param>
        /// <param name="filename">Начальное имя файла</param>
        /// <param name="fs">возвращаемый параметр с экземпляром FileStream для последующей записи в файл</param>
        /// <param name="fileMode">как был открыт файл (новый или добавление)</param>
        /// <returns>Итоговое имя файла</returns>
        public static string SaveCSVDialog(string path, string filename, out FileStream fs, out FileMode fileMode)
        {
            return SaveDialog(".csv", "(*.csv)|*.csv|Все файлы (*.*)|*.*", path, filename, true, out fs, out fileMode, true, false);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для открытия файла</summary>
        /// <param name="pathname">Путь к начальному каталогу</param>
        /// <param name="defaultExt">Расширение по умолчанию</param>
        /// <param name="filter">Список расширений</param>
        /// <returns>Итоговое имя файла</returns>
        public static string OpenFileDialog(string pathname, string defaultExt = ".sql", string filter = "(*.sql)|*.sql|Все файлы (*.*)|*.*")
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.InitialDirectory = pathname;
            dlg.DefaultExt = defaultExt; // Default file extension
            dlg.Filter = filter; // Filter files by extension
            dlg.CheckFileExists = true;

            var result = dlg.ShowDialog();

            if (result == true)
            {
                string filename = dlg.FileName;

                if (File.Exists(filename))
                {
                    return filename;
                }
            }

            return "";
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для открытия исполняемого файла</summary>
        /// <param name="pathname">Путь к начальному каталогу</param>
        /// <returns>Итоговое имя файла</returns>
        public static string OpenExeDialog(string pathname)
        {
            return OpenFileDialog(pathname, ".exe", "(*.exe)|*.exe|(*.bat)|*.bat|(*.cmd)|*.cmd|Все файлы (*.*)|*.*");
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для открытия файла с задачей</summary>
        /// <param name="pathname">Путь к начальному каталогу</param>
        /// <returns>Итоговое имя файла</returns>
        public static string OpenTaskDialog(string pathname)
        {
            return OpenFileDialog(pathname, ".task", "(*.task)|*.task|Все файлы (*.*)|*.*");
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для открытия файла Excel</summary>
        /// <param name="pathname">Путь к начальному каталогу</param>
        /// <returns>Итоговое имя файла</returns>
        public static string OpenExcelDialog(string pathname)
        {
            return OpenFileDialog(pathname, ".xlsx", "(*.xlsx)|*.xlsx|(*.xls)|*.xls|Все файлы (*.*)|*.*");
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для открытия файла DBF</summary>
        /// <param name="pathname">Путь к начальному каталогу</param>
        /// <returns>Итоговое имя файла</returns>
        public static string OpenDbfDialog(string pathname)
        {
            return OpenFileDialog(pathname, ".dbf", "(*.dbf)|*.dbf|Все файлы (*.*)|*.*");
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для открытия файла YML</summary>
        /// <param name="pathname">Путь к начальному каталогу</param>
        /// <returns>Итоговое имя файла</returns>
        public static string OpenYMLDialog(string pathname)
        {
            return OpenFileDialog(pathname, ".yml", "(*.yml)|*.yml|Все файлы (*.*)|*.*");
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Диалоговое окно для выбора каталога</summary>
        /// <param name="pathname">Путь к начальному каталогу</param>
        /// <returns>Итоговое имя каталога</returns>
        public static string FolderBrowserDialog(string pathname)
        {

            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                fbd.SelectedPath = pathname;
                fbd.ShowNewFolderButton = true;
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    return fbd.SelectedPath;
                }
            }

            return "";
        }
    }
}
