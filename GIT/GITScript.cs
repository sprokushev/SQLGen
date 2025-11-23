// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Highlighting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SQLGen.Utilities;
using System.ComponentModel;
using System.Windows.Data;

namespace SQLGen
{

    // =========================================================================================================
    /// <summary>Класс описания скрипта для отправки в GIT</summary>
    public class GITScript
    {
        /// <summary>Инициализация значениями по умолчанию</summary>
        public GITScript()
        {
            GITScriptname = "";
            GITProject = "";
            GITTypeObject = "";
            GITShemaObject = "";
            GITNameObject = "";
            GITFilename = "";
            IsExistInGIT = false;
            FirstChangesetName = "";
        }

        /// <summary>
        /// Клонирование экземпляра GITScript
        /// </summary>
        /// <returns></returns>
        public GITScript Copy()
        {
            GITScript copy = (GITScript)this.MemberwiseClone();

            return copy;
        }

        /// <summary>
        /// Номер по порядку
        /// </summary>
        public int? GITOrder { get; set; }

        // --------------------------------------------------------------------------------------------------------
        private string _script;
        /// <summary>Имя исходного файла для отправки в GIT</summary>
        public string GITScriptname
        {
            get { return _script ?? ""; }
            set
            {
                _script = value;
                if (string.IsNullOrWhiteSpace(_script)) _script = "";
                _script = _script.Trim();
            }
        }

        /// <summary>Полное имя исходного скрипта, подготовленного для отправки в GIT</summary>
        public string FullSourceScriptname_TO_GIT
        {
            get
            {
                return Path.Combine(MainWindow.Task.TaskTO_GIT, Path.GetFileName(this.GITScriptname));
            }
        }

        /// <summary>Имя исходного скрипта, подготовленного для отправки в GIT, без каталога</summary>
        public string ShortSourceScriptname_TO_GIT
        {
            get
            {
                return Path.GetFileName(FullSourceScriptname_TO_GIT);
            }
        }

        /// <summary>Проект GIT</summary>
        public string GITProject { get; set; }

        /// <summary>Каталог проекта GIT</summary>
        public string GITProjectFolder
        {
            get
            {
                return Utilities.GITProjects.GetFolderByProject(GITProject);
            }
        }

        /// <summary>Тип объекта БД</summary>
        public string GITTypeObject { get; set; }

        /// <summary>
        /// Вид объекта БД - структура, код или данные
        /// </summary>
        public string GITKindObject
        {
            get
            {
                if (
                    (GITTypeObject == "DATA") ||
                    (GITTypeObject == "data") ||
                    (GITTypeObject == "data_new")
                    )
                {
                    // данные
                    return "DATA";
                }
                else if
                    (
                    (GITTypeObject == "FUNCTION") ||
                    (GITTypeObject == "PROCEDURE") ||
                    (GITTypeObject == "TRIGGER") ||
                    (GITTypeObject == "VIEW")
                ) 
                {
                    // код
                    return "CODE";
                }
                else
                {
                    // структура
                    return "STRUCT";
                }
            }
        }



        /// <summary>
        /// Тип объекта БД для поиска в проекте GIT
        /// </summary>
        public string GITTypeObject_TO_GIT
        {
            get
            {
                if (GITKindObject == "DATA") 
                {
                    // данные
                    return DataFolder;
                }
                else if (GITTypeObject == "SCHEMA")
                {
                    // схема
                    return "";
                }
                else
                {
                    // прочее
                    return GITTypeObject;
                }
            }
        }

        /// <summary>Схема БД</summary>
        public string GITShemaObject { get; set; }

        /// <summary>Имя объекта БД</summary>
        public string GITNameObject { get; set; }

        /// <summary>Имя файла скрипта для GIT</summary>
        public string GITFilename { get; set; }

        /// <summary>
        /// имя первого changeset
        /// </summary>
        public string FirstChangesetName { get; set; }

        /// <summary>
        /// вернуть тип БД проекта
        /// </summary>
        public string DBType
        {
            get
            {
                return Utilities.GITProjects.GetDBTypeByProject(GITProject);
            }
        }

        /// <summary>
        /// вернуть папку для данных проекта
        /// </summary>
        public string DataFolder
        {
            get
            {
                return Utilities.GITProjects.GetDataFolderByProject(GITProject);
            }
        }

        /// <summary>
        /// вернуть значение флага "один объект-один скрипт" для таблиц, схем, сиквенсов, индексов, типов
        /// </summary>
        private bool isSingleScriptStruct
        {
            get
            {
                return Utilities.GITProjects.GetisSingleScriptStructByProject(GITProject);
            }
        }

        /// <summary>
        /// вернуть значение флага "один объект-один скрипт" для процедур, функций, вьюх, триггеров
        /// </summary>
        private bool isSingleScriptCode
        {
            get
            {
                return Utilities.GITProjects.GetisSingleScriptCodeByProject(GITProject);
            }
        }

        /// <summary>
        /// вернуть значение флага "один объект-один скрипт" для данных
        /// </summary>
        private bool isSingleScriptData
        {
            get
            {
                return Utilities.GITProjects.GetisSingleScriptDataByProject(GITProject);
            }
        }

        /// <summary>
        /// вернуть значение флага "один объект-один скрипт" для вида скрипта
        /// </summary>
        public bool isSingleScript
        {
            get
            {
                if (GITKindObject == "DATA")
                {
                    // данные
                    return isSingleScriptData;
                }
                else if (GITKindObject == "CODE")
                {
                    // код
                    return isSingleScriptCode;
                }
                else
                {
                    // структура
                    return isSingleScriptStruct;
                }
            }
        }

        /// <summary>
        /// Скрипт уже есть GIT
        /// </summary>
        public bool IsExistInGIT { get; set; }

        /// <summary>
        /// Путь к папке GIT в которой будет сохранен скрипт
        /// </summary>
        public string GITFilepath
        {
            get
            {
                string path = "";
                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder);

                // для скриптов с данными
                if (GITKindObject == "DATA") 
                {
                    if (isSingleScript) // сохраняем в один файл
                    {
                        path = Path.Combine(folder, DataFolder);
                    }
                    else // сохраняем в разные файлы
                    {
                        path = Path.Combine(folder, DataFolder, GITNameObject);
                    }
                }

                // для скриптов по созданию схем
                else if (GITTypeObject == "SCHEMA")
                {
                    path = Path.Combine(folder, GITShemaObject);
                }
                // для скриптов по структуре (кроме схем) или коду
                else
                {
                    if (isSingleScript) // сохраняем в один файл
                    {
                        path = Path.Combine(folder, GITShemaObject, GITTypeObject);
                    }
                    else // сохраняем в разные файлы
                    {
                        path = Path.Combine(folder, GITShemaObject, GITTypeObject, GITNameObject);
                    }
                }

                return path;
            }
        }

        /// <summary>Имя для "Данные в БД" в JIRA</summary>
        public string JIRADataBD
        {
            get
            {
                if (GITKindObject == "DATA") 
                {
                    // собираем имя объекта
                    string s = GITNameObject;
                    if (!string.IsNullOrWhiteSpace(GITShemaObject)) s = GITShemaObject + "." + s;
                    s = Utilities.Databases.GetFullTableName(s);

                    if (string.IsNullOrWhiteSpace(s))
                    {
                        // получаем имя объекта из имени файла
                        var arr = GITFilename.Split('_');
                        if (arr.Length > 1) s = Utilities.Databases.GetFullTableName(arr[0] + "." + arr[1]);
                        else s = Utilities.Databases.GetFullTableName(GITFilename);
                    }

                    return s;
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>Имя для "Объекты в БД" в JIRA</summary>
        public string JIRAObjectBD
        {
            get
            {
                if (GITKindObject == "DATA") 
                {
                    return "";
                }
                else
                {
                    // собираем имя объекта
                    string s = GITNameObject;
                    if (!string.IsNullOrWhiteSpace(GITShemaObject)) s = GITShemaObject + "." + s;
                    return Utilities.Databases.GetFullTableName(s);
                }
            }
        }
    }


    // =========================================================================================================
    public partial class MainWindow
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Обновить на экране список сриптов для отправки GIT</summary>
        public void dgScriptsInTaskRefresh()
        {
            ListCollectionView cvTasks = (ListCollectionView)CollectionViewSource.GetDefaultView(dgScriptsInTask.ItemsSource);

            if (cvTasks != null)
            {
                if (cvTasks.IsAddingNew) cvTasks.CommitNew();
                if (cvTasks.IsEditingItem) cvTasks.CommitEdit();
            }

            if (cvTasks != null && cvTasks.CanSort == true)
            {
                cvTasks.SortDescriptions.Clear();
                cvTasks.SortDescriptions.Add(new SortDescription("GITOrder", ListSortDirection.Ascending));
            }

            dgScriptsInTask.Items.Refresh();
            lbGITCount.Content = "Кол-во: " + dgScriptsInTask.Items.Count.ToString();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Удалить (скрипты для GIT)</summary>
        private void DeleteScript_Click(object sender, RoutedEventArgs e)
        {
            if (!dgScriptsInTask.IsFocused) dgScriptsInTask.Focus();

            if (dgScriptsInTask.SelectedIndex >= 0)
            {
                GITScript script = dgScriptsInTask.SelectedItem as GITScript;
                string file_in_task = Path.Combine(Task.TaskPath, script.ShortSourceScriptname_TO_GIT);

                try
                {
                    if (!script.IsExistInGIT)
                    {
                        // возвращаем из каталога на отправку
                        try
                        {
                            File.Move(script.FullSourceScriptname_TO_GIT, file_in_task);
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("Ошибка при возврате файла " + script.FullSourceScriptname_TO_GIT + " в каталог " + Task.TaskPath, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                        }
                    }

                    foreach (var item in dgScriptsInTask.Items
                        .OfType<GITScript>()
                        .ToList()
                        .Where(x => x.GITScriptname == script.GITScriptname)
                        )
                    {
                        // удаляем скрипт из перечня
                        Task.Scripts.Remove(item);
                    }

                    // сохранить текущую задачу
                    SaveTask(Task);
                }
                catch (Exception ex)
                {
                    App.AddLog("Ошибка при возврате файла " + script.FullSourceScriptname_TO_GIT + " в каталог " + Task.TaskPath, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

                dgScriptsInTaskRefresh();
            }
        }

        /// <summary>Нажата кнопка Добавить (скрипты для GIT)</summary>
        private void AddScript_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            if (!dgScriptsInTask.IsFocused) dgScriptsInTask.Focus();

            FormAddScript dlg1 = new FormAddScript();

            dlg1.gitScript = new GITScript();
            dlg1.tbGITFolder.Text = APPinfo.GITFolder;
            dlg1.listScripts = Task.Scripts;
            dlg1.isAddToDEV.Checked = (APPinfo.IsAddToDEV == "true");
            dlg1.isAddToGIT.Checked = (APPinfo.IsAddToGIT == "true");

            foreach (var item in Utilities.Files.ListFilesInDir(MainWindow.APPinfo.GITFolder, true, false, false))
            {
                string project = Utilities.GITProjects.GITProjectsParam("GITProjectFolder", item, "GITProject");

                if (
                    (!string.IsNullOrWhiteSpace(project)) &&
                    Utilities.GITProjects.IsGITProject(project)
                    )
                {
                    string DBType = Utilities.GITProjects.GITProjectsParam("GITProjectFolder", item, "DBType");
                    if ((DBType == "MSSQL") || (DBType == "PGSQL"))
                    {
                        dlg1.cbGITProject.Items.Add(project);
                    }
                }
                else
                {
                    project = Utilities.GITProjects.GITProjectsParam("DEVProjectFolder", item, "DEVProject");
                    string DBType = Utilities.GITProjects.GITProjectsParam("DEVProjectFolder", item, "DBType");
                    if ((DBType == "MSSQL") || (DBType == "PGSQL"))
                    {
                        dlg1.cbDEVProject.Items.Add(project);
                    }
                }
            }

            while (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.Retry)
            {
                // обновить список скриптов задачи
                dgScriptsInTaskRefresh();

                // выбрать последний
                if (dgScriptsInTask.Items.Count > 0)
                {
                    dgScriptsInTask.ScrollIntoView(dgScriptsInTask.Items[dgScriptsInTask.Items.Count - 1]);
                }

                // сохранить текущую задачу
                SaveTask(Task);

                // найти следующий файл и заполнить поля
                dlg1.GetNextFileAndFillForm();
            }

            dlg1.Dispose();

            // обновить список скриптов задачи
            dgScriptsInTaskRefresh();

            // выбрать последний
            if (dgScriptsInTask.Items.Count > 0)
            {
                dgScriptsInTask.ScrollIntoView(dgScriptsInTask.Items[dgScriptsInTask.Items.Count - 1]);
            }

            // сохранить текущую задачу
            SaveTask(Task);

            // обновить текущую ветку
            cbGITProject_SelectionChanged(null, null);

        }

        /*
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Процедура копирования скриптов текущей задачи в GIT для "старых" проектов</summary>
        /// <param name="project">Проект GIT</param>
        /// <param name="Info">лог копирования</param>
        private bool SendGIT(GITScript project, ref string Info)
        {
            bool result = true;
            if (string.IsNullOrWhiteSpace(Info)) Info = "";

            // каталог проекта GIT
            string GITProjectPath = Path.Combine(MainWindow.APPinfo.GITFolder, project.GITProjectFolder);
            // каталог задач в проекте GIT
            string GITYmlPath = Path.Combine(GITProjectPath, "task");
            // полный путь к yml-файлу задачи в проекте GIT
            string GITYmlFile = Path.Combine(GITYmlPath, tbYMLFile.Text);
            // временный yml-файл 
            string TmpYmlPath = Path.Combine(Task.TaskTO_GIT, project.GITProjectFolder);
            string TmpYmlFile = Path.Combine(TmpYmlPath, tbYMLFile.Text);

            Info += Environment.NewLine + "".PadRight(100, '-');
            Info += Environment.NewLine + project.GITProject;
            Info += Environment.NewLine + "".PadRight(100, '-');

            if (!Directory.Exists(Task.TaskTO_GIT))
            {
                try
                {
                    Directory.CreateDirectory(Task.TaskTO_GIT);
                }
                catch (Exception ex)
                {
                    App.AddLog("Ошибка при создании папки " + Task.TaskTO_GIT, ex, App.ShowMessageMode.SHOW);
                    Info += Environment.NewLine + "ОШИБКА: Ошибка при создании папки " + Task.TaskTO_GIT;
                    return false;
                }
            }
            if (!Directory.Exists(TmpYmlPath))
            {
                try
                {
                    Directory.CreateDirectory(TmpYmlPath);
                }
                catch (Exception ex)
                {
                    App.AddLog("Ошибка при создании папки " + TmpYmlPath, ex, App.ShowMessageMode.SHOW);
                    Info += Environment.NewLine + "ОШИБКА: Ошибка при создании папки " + TmpYmlPath;
                    return false;
                }
            }

            // Формируем содержимое yml-файла, параллельно проверяем существование sql-файлов и их идентичность

            List<CopyInfo> ListCopyFiles = new List<CopyInfo>();
            List<string> YmlList = new List<string>();
            YmlList.Add("databaseChangeLog:");

            foreach (var script in Task.Scripts.Where(s => s.GITProject == project.GITProject))
            {
                if (!File.Exists(script.FullSourceScriptname_TO_GIT))
                {
                    App.AddLog("Файл " + script.FullSourceScriptname_TO_GIT + " не существует и не будет скопирован!", null, App.ShowMessageMode.SHOW);
                    Info += Environment.NewLine + "НЕ СКОПИРОВАН: Файл " + script.FullSourceScriptname_TO_GIT + " не существует и не будет скопирован!";
                    result = false;
                }
                else
                {
                    string FileInGIT = Path.Combine(script.GITFilepath, script.GITFilename);
                    string YmlRow = "- include: { file: \".." + FileInGIT.Replace(GITProjectPath, "").Replace(Path.DirectorySeparatorChar, '/') +
                        "\", relativeToChangelogFile: \"true\" }";
                    YmlList.Add(YmlRow);

                    if (File.Exists(FileInGIT))
                    {
                        // sql-файл уже существует в проекте GIT, оставляем без изменений
                        try
                        {
                            if (Utilities.ComputeMD5ChecksumFile(script.FullSourceScriptname_TO_GIT) != Utilities.ComputeMD5ChecksumFile(FileInGIT))
                            {
                                Info += Environment.NewLine + "ВНИМАНИЕ: Файл " + FileInGIT + " уже есть в папке GIT и он отличается от того, что мы сейчас копируем - " + script.FullSourceScriptname_TO_GIT;
                                result = false;
                            }
                            else
                            {
                                Info += Environment.NewLine + "Файл " + FileInGIT + " уже есть в папке GIT, идентичен и копироваться не будет";
                            }
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("Ошибка при сравнении файлов " + script.FullSourceScriptname_TO_GIT + " и " + FileInGIT, ex, App.ShowMessageMode.SHOW);
                            Info += Environment.NewLine + "ОШИБКА: Ошибка при сравнении файлов " + script.FullSourceScriptname_TO_GIT + " и " + FileInGIT;
                        }
                    }
                    else
                    {
                        if (!Directory.Exists(script.GITFilepath))
                        {
                            try
                            {
                                Directory.CreateDirectory(script.GITFilepath);
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("Ошибка при создании папки " + script.GITFilepath, ex, App.ShowMessageMode.SHOW);
                                Info += Environment.NewLine + "ОШИБКА: Ошибка при создании папки " + script.GITFilepath;
                                return false;
                            }
                        }
                        // новый sql-файл, копируем
                        ListCopyFiles.Add(new CopyInfo { FromFile = script.FullSourceScriptname_TO_GIT, ToFile = FileInGIT, isOwerwrite = false });
                    }
                }
            }

            // формируем временный yml-файл
            if (YmlList.Count > 1)
            {
                Utilities.WriteScript(TmpYmlFile, null, YmlList, false, out string err);

                if (!string.IsNullOrWhiteSpace(err))
                {
                    Info += Environment.NewLine + "ОШИБКА: Ошибка при создании временного yml-файла " + TmpYmlFile;
                    return false;
                }
            }

            if (File.Exists(TmpYmlFile))
            {
                // проверяем существование и идентичность yml-файла в проекте GIT
                if (File.Exists(GITYmlFile))
                {
                    bool isIdentical = false;

                    try
                    {
                        if (Utilities.ComputeMD5ChecksumFile(TmpYmlFile) != Utilities.ComputeMD5ChecksumFile(GITYmlFile))
                        {
                            Info += Environment.NewLine + "ВНИМАНИЕ: Файл " + GITYmlFile + " уже есть в папке GIT и он отличается от того, что мы сейчас пытаемся создать - " + TmpYmlFile;
                            result = false;
                        }
                        else
                        {
                            // файлы идентичны, можно не копировать
                            isIdentical = true;
                            Info += Environment.NewLine + "Файл " + GITYmlFile + " уже есть в папке GIT, идентичен и копироваться не будет";
                        }
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("Ошибка при сравнении файлов " + TmpYmlFile + " и " + GITYmlFile, ex, App.ShowMessageMode.SHOW);
                        Info += Environment.NewLine + "ОШИБКА: Ошибка при сравнении файлов " + TmpYmlFile + " и " + GITYmlFile;
                    }

                    if (!isIdentical)
                    {
                        if (System.Windows.Forms.MessageBox.Show("Файл " + GITYmlFile + " уже есть в папке GIT!" + Environment.NewLine + Environment.NewLine +
                            "Добавить отдельную версию (Да/Yes) или прервать копирование в папку GIT (Нет/No)?", "Файл существует!", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        {
                            App.AddLog("Файл " + GITYmlFile + " уже есть в папке GIT, выбрано добавление отдельной версии", null, App.ShowMessageMode.NONE);

                            int i = 0;
                            do
                            {
                                i++;
                                GITYmlFile = Path.GetFileNameWithoutExtension(GITYmlFile);
                                GITYmlFile = Path.Combine(GITYmlPath, GITYmlFile + "_" + i.ToString() + ".yml");
                            } while (File.Exists(GITYmlFile));

                            App.AddLog("Будет добавлен файл " + GITYmlFile, null, App.ShowMessageMode.NONE);
                        }
                        else
                        {
                            Info += Environment.NewLine + "ВНИМАНИЕ: Файл " + GITYmlFile + " уже есть в папке GIT, отличается, Пользователь прервал копирование в GIT!";
                            return false;
                        }
                    }
                }

                // копируем sql-файлы
                foreach (var item in ListCopyFiles)
                {
                    try
                    {
                        File.Copy(item.FromFile, item.ToFile, item.isOwerwrite);
                        File.SetLastWriteTime(item.ToFile, DateTime.Now);
                        File.SetCreationTime(item.ToFile, DateTime.Now);
                        Info += Environment.NewLine + item.FromFile + " скопирован в " + item.ToFile;
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("Ошибка при копировании " + item.FromFile + " в " + item.ToFile, ex, App.ShowMessageMode.SHOW);
                        Info += Environment.NewLine + "ОШИБКА: Ошибка при копировании " + item.FromFile + " в " + item.ToFile;
                        result = false;
                    }
                }


                // копируем yml-файл
                if (!File.Exists(GITYmlFile))
                {
                    try
                    {
                        File.Copy(TmpYmlFile, GITYmlFile, false);
                        File.SetLastWriteTime(GITYmlFile, DateTime.Now);
                        File.SetCreationTime(GITYmlFile, DateTime.Now);
                        Info += Environment.NewLine + TmpYmlFile + " скопирован в " + GITYmlFile;
                        // обновить имя yml-файла на форме
                        tbYMLFile.Text = Path.GetFileName(GITYmlFile);
                        // дополнить историю задач
                        AddHistoryYMLFile(tbYMLFile.Text);
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("Ошибка при копировании " + TmpYmlFile + " в " + GITYmlFile, ex, App.ShowMessageMode.SHOW);
                        Info += Environment.NewLine + "ОШИБКА: Ошибка при копировании " + TmpYmlFile + " в " + GITYmlFile;
                        return false;
                    }
                }
            }

            return result;
        }
        */


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Процедура копирования скриптов текущей задачи в GIT</summary>
        /// <param name="project">Проект GIT</param>
        /// <param name="Info">лог копирования</param>
        /// <param name="isForceName">принудительное назначение имени</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        private bool SendGIT(GITScript project, ref string Info, bool isForceName, string logFile) //-V3203
        {
            bool result = true;
            if (string.IsNullOrWhiteSpace(Info)) Info = "";

            // каталог проекта GIT
            string GITProjectPath = Path.Combine(MainWindow.APPinfo.GITFolder, project.GITProjectFolder);
            // каталог задач в проекте GIT
            string GITYmlPath = Path.Combine(GITProjectPath, "task");

            // полный путь к yml-файлу задачи в проекте GIT
            string GITYmlFile = Path.Combine(GITYmlPath, tbYMLFile.Text);
            /*
            if (!isForceName)
            {
                if (Task.SendYMLFiles.ContainsKey(project.GITProject))
                {
                    // возьмем имя yml-файла из последней отправки
                    GITYmlFile = Path.Combine(GITYmlPath, Task.SendYMLFiles[project.GITProject]);
                }
            }
            */

            // временный yml-файл 
            string TmpYmlPath = Path.Combine(Task.TaskTO_GIT, project.GITProjectFolder);
            string TmpYmlFile = Path.Combine(TmpYmlPath, tbYMLFile.Text);

            Info += Environment.NewLine + "".PadRight(100, '-');
            Info += Environment.NewLine + project.GITProject;
            Info += Environment.NewLine + "".PadRight(100, '-');

            if (!Directory.Exists(Task.TaskTO_GIT))
            {
                try
                {
                    Directory.CreateDirectory(Task.TaskTO_GIT);
                }
                catch (Exception ex)
                {
                    App.AddLog("Ошибка при создании папки " + Task.TaskTO_GIT, ex, App.ShowMessageMode.SHOW, true, logFile);
                    Info += Environment.NewLine + "ОШИБКА: Ошибка при создании папки " + Task.TaskTO_GIT;
                    return false;
                }
            }
            if (!Directory.Exists(TmpYmlPath))
            {
                try
                {
                    Directory.CreateDirectory(TmpYmlPath);
                }
                catch (Exception ex)
                {
                    App.AddLog("Ошибка при создании папки " + TmpYmlPath, ex, App.ShowMessageMode.SHOW, true, logFile);
                    Info += Environment.NewLine + "ОШИБКА: Ошибка при создании папки " + TmpYmlPath;
                    return false;
                }
            }

            // Формируем содержимое yml-файла, параллельно проверяем существование sql-файлов и их идентичность

            // список на копирование
            List<CopyInfo> ListCopyFiles = new List<CopyInfo>();

            // список на дописывание в существующий файл
            List<CopyInfo> ListAppendFiles = new List<CopyInfo>();

            // список на сравнение и принятие решения
            List<CopyInfo> ListCompareFiles = new List<CopyInfo>();

            // список строк yml-файла
            List<string> YmlList = new List<string>();
            YmlList.Add("databaseChangeLog:");

            // перебираем отправляемые файлы
            foreach (var script in Task.Scripts
                .Where(s => s.GITProject == project.GITProject)
                .OrderBy(x => x.GITOrder)
            )
            {
                if (!File.Exists(script.FullSourceScriptname_TO_GIT))
                {
                    App.AddLog("Файл " + script.FullSourceScriptname_TO_GIT + " не существует и не будет скопирован!", null, App.ShowMessageMode.SHOW, true, logFile);
                    Info += Environment.NewLine + "НЕ СКОПИРОВАН: Файл " + script.FullSourceScriptname_TO_GIT + " не существует и не будет скопирован!";
                    result = false;
                }
                else
                {
                    string FileInGIT = Path.Combine(script.GITFilepath, script.GITFilename);

                    if (File.Exists(FileInGIT))
                    {
                        // sql-файл уже существует в проекте GIT

                        // на всякий случай получим реальное имя файла
                        FileInGIT = Utilities.Files.GetRealFilename(FileInGIT);

                        if (!script.isSingleScript) // если сохраняем в разные файлы, надо сравнить с существующим файлом
                        {
                            try
                            {
                                // сверим хеш 
                                if (Utilities.Files.ComputeMD5ChecksumFile(script.FullSourceScriptname_TO_GIT) != Utilities.Files.ComputeMD5ChecksumFile(FileInGIT))
                                {
                                    //Info += Environment.NewLine + "ВНИМАНИЕ: Файл " + FileInGIT + " уже есть в папке GIT и он отличается от того, что мы сейчас копируем - " + script.FullSourceScriptname_TO_GIT;
                                    //result = false;

                                    // добавляем в список для сравнения
                                    ListCompareFiles.Add(new CopyInfo { fromFile = script.FullSourceScriptname_TO_GIT, toFile = FileInGIT, actionType = ActionType.SKIP, changesetName = "", changesetText = "" });
                                }
                                else
                                {
                                    Info += Environment.NewLine + "Файл " + FileInGIT + " уже есть в папке GIT, идентичен и копироваться не будет";
                                }
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("Ошибка при сравнении файлов " + script.FullSourceScriptname_TO_GIT + " и " + FileInGIT, ex, App.ShowMessageMode.SHOW, true, logFile);
                                Info += Environment.NewLine + "ОШИБКА: Ошибка при сравнении файлов " + script.FullSourceScriptname_TO_GIT + " и " + FileInGIT;
                                result = false;
                            }
                        }
                        else if (script.isSingleScript && (script.GITKindObject == "CODE")) // если код и сохраняем в один файл
                        {
                            // заменяем полностью
                            ListCopyFiles.Add(new CopyInfo { fromFile = script.FullSourceScriptname_TO_GIT, toFile = FileInGIT, actionType = ActionType.OWERWRITE });
                        }
                        else
                        {
                            // если сохраняем в один файл и это не код - надо дописать в конец, но сначала проверим существование

                            // сначала определим имя changeset
                            string changeset_name = SQLChangeset.FirstChangesetName(script.FullSourceScriptname_TO_GIT, out SQLChangeset _changeset);
                            if (
                                (changeset_name == "unknown") ||
                                string.IsNullOrWhiteSpace(changeset_name)
                            )
                            {
                                changeset_name = Task.TaskNumber;
                            }

                            // поищем и выделим кусок с нужным changeset и сравним с копируемым файлом
                            // =-1 - найден, не совпадает хеш
                            // =0 - найден, совпадает хеш
                            // =1 - НЕ найден 
                            int isFound = SQLChangeset.FindChangeset(script.FullSourceScriptname_TO_GIT, FileInGIT, changeset_name, out string changeset_text, logFile);

                            if (isFound == -1)
                            {
                                // changeset найден, не совпадает хеш
                                // Info += Environment.NewLine + "ВНИМАНИЕ: Changeset " + chageset + " уже есть в файле " + FileInGIT + " в папке GIT и он отличается от того, что мы сейчас копируем - " + script.FullSourceScriptname_TO_GIT;
                                //result = false;

                                // добавляем в список для сравнения
                                ListCompareFiles.Add(new CopyInfo { fromFile = script.FullSourceScriptname_TO_GIT, toFile = FileInGIT, actionType = ActionType.SKIP, changesetName = changeset_name, changesetText = changeset_text });
                            }
                            else if (isFound == 0)
                            {
                                // changeset найден, совпадает хеш
                                Info += Environment.NewLine + "Changeset " + changeset_name + " уже есть в файле " + FileInGIT + " в папке GIT, идентичен и копироваться не будет";
                            }
                            else
                            {
                                // changeset не найден, допишем в конец
                                ListAppendFiles.Add(new CopyInfo { fromFile = script.FullSourceScriptname_TO_GIT, toFile = FileInGIT, actionType = ActionType.APPEND });
                            }
                        }
                    }
                    else
                    {
                        // sql-файл НЕ существует в проекте GIT

                        if (!Directory.Exists(script.GITFilepath))
                        {
                            try
                            {
                                Directory.CreateDirectory(script.GITFilepath);
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("Ошибка при создании папки " + script.GITFilepath, ex, App.ShowMessageMode.SHOW, true, logFile);
                                Info += Environment.NewLine + "ОШИБКА: Ошибка при создании папки " + script.GITFilepath;
                                return false;
                            }
                        }

                        // новый sql-файл, копируем
                        ListCopyFiles.Add(new CopyInfo { fromFile = script.FullSourceScriptname_TO_GIT, toFile = FileInGIT, actionType = ActionType.OWERWRITE });

                        // на случай, если будут дополнительные changeset в задаче, создаем пустой файл, чтобы последующие changeset проходили по алгоритму добавления
                        try
                        {
                            File.WriteAllText(FileInGIT,"");
                        }
                        catch (Exception ex)
                        {
                            App.AddLog($"Ошибка при создании нового файла {FileInGIT}", ex, App.ShowMessageMode.SHOW, true, logFile);
                            Info += Environment.NewLine + $"ОШИБКА: Ошибка при создании нового файла {FileInGIT}";
                            return false;
                        }
                    }

                    string YmlRow = "";

                    if (MainWindow.APPinfo.relativeToChangelogFile == "true")
                    {
                        YmlRow = "- include: { file: \".." + FileInGIT.Replace(GITProjectPath, "").Replace(Path.DirectorySeparatorChar, '/') +
        "\", relativeToChangelogFile: \"true\" }";
                    }
                    else
                    {
                        YmlRow = "- include: { file: \"" + FileInGIT.Replace(GITProjectPath + Path.DirectorySeparatorChar, "").Replace(Path.DirectorySeparatorChar, '/') +
        "\", relativeToChangelogFile: \"false\" }";
                    }
                    YmlList.Add(YmlRow);

                }
            }

            // формируем временный yml-файл
            if (YmlList.Count > 1)
            {
                Utilities.Files.WriteScript(TmpYmlFile, null, YmlList, false, out string err, FileMode.Create);

                if (!string.IsNullOrWhiteSpace(err))
                {
                    Info += Environment.NewLine + "ОШИБКА: Ошибка при создании временного yml-файла " + TmpYmlFile;
                    return false;
                }
            }

            if (File.Exists(TmpYmlFile))
            {
                bool isOwerwrite = false;

                // проверяем существование и идентичность yml-файла в проекте GIT
                if (File.Exists(GITYmlFile))
                {
                    bool isIdentical = false;

                    try
                    {
                        if (Utilities.Files.ComputeMD5ChecksumFile(TmpYmlFile) != Utilities.Files.ComputeMD5ChecksumFile(GITYmlFile))
                        {
                            //Info += Environment.NewLine + "ВНИМАНИЕ: Файл " + GITYmlFile + " уже есть в папке GIT и он отличается от того, что мы сейчас пытаемся создать - " + TmpYmlFile;
                            //result = false;
                        }
                        else
                        {
                            // файлы идентичны, можно не копировать
                            isIdentical = true;
                            Info += Environment.NewLine + "Файл " + GITYmlFile + " уже есть в папке GIT, идентичен и копироваться не будет";
                        }
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("Ошибка при сравнении файлов " + TmpYmlFile + " и " + GITYmlFile, ex, App.ShowMessageMode.SHOW, true, logFile);
                        Info += Environment.NewLine + "ОШИБКА: Ошибка при сравнении файлов " + TmpYmlFile + " и " + GITYmlFile;
                        result = false;
                    }

                    if (!isIdentical)
                    {
                        var ask = System.Windows.Forms.MessageBox.Show("Файл " + GITYmlFile + " уже есть в папке GIT!" + Environment.NewLine + Environment.NewLine +
                                    "(Да/Yes) - Перезаписать существующий файл ?" + Environment.NewLine +
                                    "(Нет/No) - Добавить отдельную версию  ?" + Environment.NewLine +
                                    "(Отмена/Cancel) - Прервать копирование в папку GIT ?", "Файл существует!", System.Windows.Forms.MessageBoxButtons.YesNoCancel);

                        if (ask == System.Windows.Forms.DialogResult.No)
                        {
                            App.AddLog("Файл " + GITYmlFile + " уже есть в папке GIT, выбрано добавление отдельной версии.", null, App.ShowMessageMode.NONE, true, logFile);
                            Info += Environment.NewLine + "ВНИМАНИЕ: Файл " + GITYmlFile + " уже есть в папке GIT и он отличается от того, что мы сейчас пытаемся создать - " + TmpYmlFile;

                            int i = 0;
                            string _ymlfile = Path.GetFileNameWithoutExtension(GITYmlFile);
                            do
                            {
                                i++;
                                GITYmlFile = Path.Combine(GITYmlPath, _ymlfile + "_" + i.ToString() + ".yml");
                            } while (File.Exists(GITYmlFile));

                            App.AddLog("Будет создан файл " + GITYmlFile, null, App.ShowMessageMode.NONE, true, logFile);
                            Info += Environment.NewLine + "ВНИМАНИЕ: Будет создан файл " + GITYmlFile;
                            result = false;
                            isOwerwrite = true;

                            tbYMLFile.Text = Path.GetFileName(GITYmlFile);
                        }
                        else if (ask == System.Windows.Forms.DialogResult.Cancel)
                        {
                            Info += Environment.NewLine + "ВНИМАНИЕ: Файл " + GITYmlFile + " уже есть в папке GIT, отличается, Пользователь прервал копирование в GIT!";
                            return false;
                        }
                        else
                        {
                            Info += Environment.NewLine + "ВНИМАНИЕ: Файл " + GITYmlFile + " уже есть в папке GIT, отличается, Пользователь выбрал перезапись файла!";
                            isOwerwrite = true;
                        }
                    }
                }
                else
                {
                    isOwerwrite = true;
                }

                // для sql-файлов с различиями предлагем пользователю выбрать "Перезаписать" или "Не изменять"
                foreach (var item in ListCompareFiles)
                {
                    // открываем окно со списком файлов и выбором действий
                    FormAskCompare dlg1 = new FormAskCompare();
                    dlg1.tbGITFilename.Text = item.toFile;
                    dlg1.tbChangesetName.Text = item.changesetName;
                    dlg1.fromFile = item.fromFile;
                    dlg1.changesetText = item.changesetText;
                    var ask = dlg1.ShowDialog();
                    dlg1.Dispose();

                    if (ask == System.Windows.Forms.DialogResult.Yes)
                    {
                        if (string.IsNullOrWhiteSpace(item.changesetName))
                        {
                            // пользователь выбрал - перезаписать существующий файл
                            ListCopyFiles.Add(new CopyInfo { fromFile = item.fromFile, toFile = item.toFile, actionType = ActionType.OWERWRITE });
                        }
                        else
                        {
                            // пользователь выбрал - перезаписать существующий changeset
                            try
                            {
                                string text = SQLChangeset.RemoveLiquibaseTag(File.ReadAllText(item.fromFile));

                                // заменить changeset в файле
                                SQLChangeset.ReSaveChangeset(item.toFile, item.changesetName, text, logFile);

                                File.SetLastWriteTime(item.toFile, DateTime.Now);
                                File.SetCreationTime(item.toFile, DateTime.Now);
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("Ошибка при перезаписи " + item.fromFile + " в changeset " + item.changesetName + " в файле " + item.toFile, ex, App.ShowMessageMode.SHOW, true, logFile);
                                Info += Environment.NewLine + "ОШИБКА: Ошибка при перезаписи " + item.fromFile + " в changeset " + item.changesetName + " в файле " + item.toFile;
                                result = false;
                            }
                        }
                    }
                    else if (ask == System.Windows.Forms.DialogResult.No)
                    {
                        // пользователь выбрал - добавить в конец существующего файла
                        ListAppendFiles.Add(new CopyInfo { fromFile = item.fromFile, toFile = item.toFile, actionType = ActionType.APPEND });
                    }
                    else
                    {
                        result = false;

                        // по всем оставшимся файлам "Не изменять" добавим в лог информацию
                        if (string.IsNullOrWhiteSpace(item.changesetName))
                        {
                            Info += Environment.NewLine + "ВНИМАНИЕ: Файл " + item.toFile + " уже есть в папке GIT и он отличается от того, что мы сейчас копируем - " + item.fromFile;
                        }
                        else
                        {
                            Info += Environment.NewLine + "ВНИМАНИЕ: Changeset " + item.changesetName + " уже есть в файле " + item.toFile + " в папке GIT и он отличается от того, что мы сейчас копируем - " + item.fromFile;
                        }
                    }
                }

                // копируем новые sql-файлы
                foreach (var item in ListCopyFiles)
                {
                    try
                    {
                        File.Copy(item.fromFile, item.toFile, true);
                        File.SetLastWriteTime(item.toFile, DateTime.Now);
                        File.SetCreationTime(item.toFile, DateTime.Now);
                        Info += Environment.NewLine + item.fromFile + " скопирован в " + item.toFile;
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("Ошибка при копировании " + item.fromFile + " в " + item.toFile, ex, App.ShowMessageMode.SHOW, true, logFile);
                        Info += Environment.NewLine + "ОШИБКА: Ошибка при копировании " + item.fromFile + " в " + item.toFile;
                        result = false;
                    }
                }

                // дописываем в конец существующих sql-файлов
                foreach (var item in ListAppendFiles)
                {
                    try
                    {
                        string text = "\n" +
                            SQLChangeset.RemoveLiquibaseTag(
                                File.ReadAllText(item.fromFile)
                            )
                            .TrimEndNewLine("\n");

                        File.AppendAllText(item.toFile, text);
                        File.SetLastWriteTime(item.toFile, DateTime.Now);
                        File.SetCreationTime(item.toFile, DateTime.Now);
                        Info += Environment.NewLine + item.fromFile + " дописан в конец " + item.toFile;
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("Ошибка при дописывании " + item.fromFile + " в конец " + item.toFile, ex, App.ShowMessageMode.SHOW, true, logFile);
                        Info += Environment.NewLine + "ОШИБКА: Ошибка при дописывании " + item.fromFile + " в конец " + item.toFile;
                        result = false;
                    }
                }

                if (Task.SendYMLFiles.ContainsKey(project.GITProject))
                {
                    Task.SendYMLFiles[project.GITProject] = Path.GetFileName(GITYmlFile);
                }
                else
                {
                    Task.SendYMLFiles.Add(project.GITProject, Path.GetFileName(GITYmlFile));
                }

                // копируем yml-файл
                if (isOwerwrite)
                {
                    try
                    {
                        File.Copy(TmpYmlFile, GITYmlFile, isOwerwrite);
                        File.SetLastWriteTime(GITYmlFile, DateTime.Now);
                        File.SetCreationTime(GITYmlFile, DateTime.Now);
                        Info += Environment.NewLine + TmpYmlFile + " скопирован в " + GITYmlFile;
                        // обновить имя yml-файла на форме
                        // tbYMLFile.Text = Task.SendYMLFiles[project.GITProject];
                        // дополнить историю задач
                        AddHistoryYMLFile(Task.SendYMLFiles[project.GITProject]);
                    }
                    catch (Exception ex)
                    {
                        App.AddLog("Ошибка при копировании " + TmpYmlFile + " в " + GITYmlFile, ex, App.ShowMessageMode.SHOW, true, logFile);
                        Info += Environment.NewLine + "ОШИБКА: Ошибка при копировании " + TmpYmlFile + " в " + GITYmlFile;
                        return false;
                    }
                }
            }

            return result;
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Собрать YML</summary>
        private void btSendGIT_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            if (System.Windows.Forms.MessageBox.Show("Собрать YML для задачи " + MainWindow.Task.TaskNumber + " ?",
                                "ВНИМАНИЕ",
                                System.Windows.Forms.MessageBoxButtons.YesNo
                            ) == System.Windows.Forms.DialogResult.No
                        )
            {
                return;
            }

            List<GITScript> Projects;
            bool result = true;
            bool has_to_send = false;
            string Info = "";

            // --------------------------------------------------------------------
            // "Старый" проект
            // --------------------------------------------------------------------
            bool isForceName = false;

            if (cbGITProject.SelectedItem.ToString().Trim() == "ВСЕ")
            {
                Projects = Task.Scripts
                    .Where(w => Utilities.GITProjects.IsGITProject(w.GITProject))
                    .GroupBy(p => p.GITProject)
                    .Select(g => g.First())
                    .ToList();

                isForceName = false;
            }
            else
            {
                Projects = Task.Scripts
                    .Where(w =>
                        (w.GITProject == cbGITProject.SelectedItem.ToString().Trim()) &&
                        Utilities.GITProjects.IsGITProject(w.GITProject)
                        )
                    .GroupBy(p => p.GITProject)
                    .Select(g => g.First())
                    .ToList();

                isForceName = true;
            }

            foreach (var project in Projects)
            {
                has_to_send = true;

                if (!SendGIT(project, ref Info, isForceName, Task.LogFile))
                {
                    result = false;
                    Info += Environment.NewLine;
                }
            }

            // --------------------------------------------------------------------
            // "Новый" проект
            // --------------------------------------------------------------------
            if (cbGITProject.SelectedItem.ToString().Trim() == "ВСЕ")
            {
                Projects = Task.Scripts
                    .Where(w => Utilities.GITProjects.IsDEVProject(w.GITProject))
                    .GroupBy(p => p.GITProject)
                    .Select(g => g.First())
                    .ToList();

                isForceName = false;
            }
            else
            {
                Projects = Task.Scripts
                    .Where(w =>
                        (w.GITProject == cbGITProject.SelectedItem.ToString().Trim()) &&
                        Utilities.GITProjects.IsDEVProject(w.GITProject)
                        )
                    .GroupBy(p => p.GITProject)
                    .Select(g => g.First())
                    .ToList();

                isForceName = true;
            }

            if (Projects.Count > 0)
            {
                // есть новые проекты, надо создать/переключиться на ветку задачи
                GIT.GitNewBranch(Projects.Select(x => x.GITProject).ToArray(), Task.TaskNumber, "master", Task.LogFile);

                // копируем файлы
                foreach (var project in Projects)
                {
                    has_to_send = true;

                    if (!SendGIT(project, ref Info, isForceName, Task.LogFile))
                    {
                        result = false;
                        Info += Environment.NewLine;
                    }
                }
            }


            // --------------------------------------------------------------------
            // Результат
            // --------------------------------------------------------------------
            if (has_to_send)
            {
                // сохранить задачу
                SaveTask(Task);

                if (result)
                {
                    if (System.Windows.Forms.MessageBox.Show("Файлы успешно собраны в YML!" + Environment.NewLine + Environment.NewLine +
                                "Посмотреть лог ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        WinInfo WinInfo = new WinInfo(Task.LogFile);
                        WinInfo.Title = "Лог копирования";
                        WinInfo.tbInfo.Text = Info;
                        WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                        WinInfo.Show();
                    }
                }
                else
                {
                    if (System.Windows.Forms.MessageBox.Show("Файлы НЕ скопированы в папку GIT, либо есть ошибки при копировании!" + Environment.NewLine + Environment.NewLine +
                                "Посмотреть лог ?", "ОШИБКА", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        WinInfo WinInfo = new WinInfo(Task.LogFile);
                        WinInfo.Title = "Лог копирования";
                        WinInfo.tbInfo.Text = Info;
                        WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                        WinInfo.Show();
                    }
                }
            }
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Скопировать из GIT в папку задачи</summary>
        private void btCopyYMLtoTASK_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            //сборка имени yml-файла 
            string YMLFile = tbYMLFile.Text.Trim();
            if ((YMLFile.Length < 4) || (YMLFile.Substring(YMLFile.Length - 4, 4).ToLower() != ".yml")) YMLFile += ".yml";
            tbYMLFile.Text = YMLFile;

            // спросить
            if (System.Windows.Forms.MessageBox.Show("Скопировать " + tbYMLFile.Text + " в каталог задачи ?",
                    "ВНИМАНИЕ",
                    System.Windows.Forms.MessageBoxButtons.YesNo
                ) == System.Windows.Forms.DialogResult.No
            )
            {
                return;
            }

            // выбрать проекты
            FormCheckedListBox dlg1 = new FormCheckedListBox();

            // заполнить список проектов
            Utilities.Controls.FillCheckedListBoxProjects(
                dlg1.clbList,
                MainWindow.ListExistedProjects,
                null,
                null,
                true,
                true,
                false
                );

            // заполнить список выбранных проектов
            List<string> Projects = new List<string>();

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    Projects.Add(itemChecked.ToString());
                }
            }
            dlg1.Dispose();

            if (Projects.Count > 0)
            {
                List<string> DevProjects = new List<string>();
                foreach (var item in Projects.Where(x => Utilities.GITProjects.IsDEVProject(x)))
                {
                    DevProjects.Add(item);
                }

                string branch = Task.TaskNumber;
                if (DevProjects.Count > 0)
                {
                    // выбрать ветку (в новых проектах)
                    FormAskBranch dlg2 = new FormAskBranch(null, null, Task.LogFile);

                    foreach (var project in DevProjects)
                    {
                        // Заполнить ListBranches
                        foreach (var item in GIT.GitListBranches(project, "git_listbranch.cmd", Task.LogFile, true))
                        {
                            string _branch = item.Replace("*", "").Trim();

                            if (
                                (!string.IsNullOrWhiteSpace(_branch)) &&
                                (!dlg2.ListBranches.Contains(_branch))
                                )
                            {
                                dlg2.ListBranches.Add(_branch);
                            }
                        }
                    }

                    var res = dlg2.ShowDialog();

                    if (!string.IsNullOrWhiteSpace(dlg2.Branch))
                    {
                        branch = dlg2.Branch;
                    }

                    dlg2.Dispose();

                    if (res == System.Windows.Forms.DialogResult.Abort)
                    {
                        return;
                    }
                }

                // git pull и переключение на выбранную ветку
                GIT.GitPull(Projects.ToArray(), branch, false, true, false, Task.LogFile, false);

                // копирование
                this.Cursor = Cursors.Wait;
                List<YMLText> ListVersions = new List<YMLText>();

                bool isError = !Utilities.YML.GetYmlFromGIT(
                    Projects, 
                    ref YMLFile, 
                    out bool isFound, 
                    isCheck.IsChecked==true ? CopyType.CHECKCOPY : CopyType.COPY,
                    false,
                    isCheckBOM.IsChecked == true, 
                    out string errors, 
                    ref ListVersions,
                    isCopyPrevVersion.IsChecked == true,
                    Task.LogFile
                    );

                this.Cursor = Cursors.Arrow;

                if (!isFound)
                {
                    if (Projects.Count > 1)
                    {
                        App.AddLog("Файл " + tbYMLFile.Text + " не найден в папках task или version в проектах GIT !", null, App.ShowMessageMode.SHOW, true, Task.LogFile);
                    }
                    else
                    {
                        App.AddLog("Файл " + tbYMLFile.Text + " не найден в папках task или version в проекте " + Projects[0] + " !", null, App.ShowMessageMode.SHOW, true, Task.LogFile);
                    }
                }
                else
                {
                    // дополнить историю yml
                    AddHistoryYMLFile(tbYMLFile.Text);

                    // сохранить текущую задачу
                    SaveTask(Task);

                    if (isError)
                    {
                        WinInfo WinInfo = new WinInfo(Task.LogFile);
                        WinInfo.Title = "Есть ошибки в скриптах!!!";
                        WinInfo.tbInfo.Text = errors;
                        WinInfo.Show();
                    }
                    else App.AddLog("Файлы скопированы", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }

        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Данные для Jira</summary>
        private void btForJira_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            WinInfo WinInfo = new WinInfo(null);
            WinInfo.Title = "Информация для Jira";
            WinInfo.tbInfo.Text = Task.TaskInfoForJira();
            WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("RUS");
            WinInfo.Show();
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Нажата кнопка Tortoise GIT</summary>
        private void btTortoiseGIT_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Task.TaskNumber))
            {
                MessageBox.Show("Необходимо заполнить Номер задачи !");
                if (!tabTask.IsSelected) tabTask.IsSelected = true;
                if (!tbTaskNumber.IsFocused) tbTaskNumber.Focus();
                return;
            }

            List<GITScript> Projects;

            // добавляем проекты из sql задачи
            if (cbGITProject.SelectedItem.ToString() == "ВСЕ")
            {
                Projects = Task.Scripts.GroupBy(p => p.GITProject).Select(g => g.First()).ToList();
            }
            else
            {
                Projects = Task.Scripts.Where(w => w.GITProject == cbGITProject.SelectedItem.ToString()).GroupBy(p => p.GITProject).Select(g => g.First()).ToList();
            }

            var projects_arr = Projects.Select(x => x.GITProject).ToList();

            // Спросим, для какого модуля (префикса) добавляем: prmd, rpms, smp, bi
            if (
                Task.ListDeploymentMS.Count() > 0 ||
                Task.ListDeploymentPG.Count() > 0 ||
                Task.ListCronMS.Count() > 0 ||
                Task.ListCronPG.Count() > 0
            )
            {
                string prefix = GIT.SelectGITModule(Task.LogFile);
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    // добавляем проекты из deployment задачи
                    string projectDeploymentMS = Utilities.GITProjects.GetProjectDeployment("MS SQL", prefix);
                    string projectDeploymentPG = Utilities.GITProjects.GetProjectDeployment("PG SQL", prefix);

                    if (
                        (Task.ListDeploymentMS.Count() > 0) &&
                        (
                            cbGITProject.SelectedItem.ToString() == "ВСЕ" ||
                            cbGITProject.SelectedItem.ToString() == projectDeploymentMS
                        )
                    )
                    {
                        if (!projects_arr.Contains(projectDeploymentMS))
                        {
                            projects_arr.Add(projectDeploymentMS);
                        }
                    }

                    if (
                        (Task.ListDeploymentPG.Count() > 0) &&
                        (
                        cbGITProject.SelectedItem.ToString() == "ВСЕ" ||
                        cbGITProject.SelectedItem.ToString() == projectDeploymentPG
                        )
                    )
                    {
                        if (!projects_arr.Contains(projectDeploymentPG))
                        {
                            projects_arr.Add(projectDeploymentPG);
                        }
                    }

                    // добавляем проекты из cron задачи
                    string projectCronMS = Utilities.GITProjects.GetProjectCron("MS SQL", prefix);
                    string projectCronPG = Utilities.GITProjects.GetProjectCron("PG SQL", prefix);


                    if (
                        (Task.ListCronMS.Count() > 0) &&
                        (
                            cbGITProject.SelectedItem.ToString() == "ВСЕ" ||
                            cbGITProject.SelectedItem.ToString() == projectCronMS
                        )
                    )
                    {
                        if (!projects_arr.Contains(projectCronMS))
                        {
                            projects_arr.Add(projectCronMS);
                        }
                    }

                    if (
                        (Task.ListCronPG.Count() > 0) &&
                        (
                        cbGITProject.SelectedItem.ToString() == "ВСЕ" ||
                        cbGITProject.SelectedItem.ToString() == projectCronPG
                        )
                    )
                    {
                        if (!projects_arr.Contains(projectCronPG))
                        {
                            projects_arr.Add(projectCronPG);
                        }
                    }
                }
            }

            // Отправляем в GIT
            if (projects_arr.Count() > 0)
            {
                GIT.GitAdd(projects_arr.ToArray(), Task.TaskNumber, false, true, MainWindow.Task.LogFile);
            }
        }
    }
}
