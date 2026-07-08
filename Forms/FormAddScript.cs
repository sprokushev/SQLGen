// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
//using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AngleSharp.Dom;
using SQLGen.Controls;
using SQLGen.Utilities;

namespace SQLGen
{

    /// <summary>
    /// Форма добавления скриптов в задачу
    /// </summary>
    public partial class FormAddScript : Form
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Добавляемый скрипт в проект GIT</summary>
        public GITScript gitScript = new GITScript();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Добавляемый скрипт в проект DEV</summary>
        public GITScript devScript = new GITScript();

        /// <summary>
        /// список на случай если в одном файле будет несколько changeset
        /// </summary>
        public List<GITScript> listDevScripts = new List<GITScript>();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Скрипт с индексом</summary>
        public string IndexStr = "";

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список уже добавленных в задачу скриптов</summary>
        public List<GITScript> listScripts;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Последний файл</summary>
        public string lastFile = "";

        // -------------------------------------------------------------------------------------------------------
        /// <summary>конструктор Формы добавления скриптов в задачу</summary>
        public FormAddScript()
        {
            InitializeComponent();

            // пользовательские настройки GUI
            Default.InitGUI("FormAddScript", this, MainWindow.Task.LogFile);

            ClearFileds();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Собрать имя файла для GIT из имени рабочего файла-скрипта
        /// </summary>
        /// <param name="filename">имя файла</param>
        /// <returns></returns>
        private string GetGITFilenameFromScriptFilename(string filename)
        {
            var arr = filename.Split(' ');
            string res = "";

            for (int i = 0; i < arr.Length; i++)
            {
                var s = arr[i].ToLower();

                string project = Utilities.GITProjects.GITProjectsParam("PrefixFileSQL", s, "DEVProject");
                if ((i == 0) && (!string.IsNullOrWhiteSpace(project)))
                {
                    arr[i] = "";
                }

                if (
                    s.StartsWith("promedweb") ||
                    s.StartsWith("rpms") ||
                    s.StartsWith("rm") ||
                    s.StartsWith("ops") ||
                    s.StartsWith("smp") ||
                    s.StartsWith("cm") ||
                    s.StartsWith("bip")
                )
                {
                    arr[i] = "";
                }

                if (int.TryParse(s, out int j))
                {
                    arr[i] = "";
                }

                if (GITProjects.List_ScriptType_GITType.ContainsKey(s))
                {
                    arr[i] = "";
                }

                if (!string.IsNullOrWhiteSpace(arr[i]))
                {
                    if (!string.IsNullOrWhiteSpace(res)) res = res + "_";
                    res = res + arr[i];
                }
            }

            res = res.Replace(".", "_").Replace(" ", "_").Replace("-", "");
            //+ "_" + DateTime.Now.ToString("yyyyMMdd");
            return res.Trim();
        }

        /// <summary>
        /// Очистить все поля
        /// </summary>
        private void ClearFileds()
        {
            gitScript = new GITScript();
            devScript = new GITScript();

            // очистить все поля
            tbScriptFilename.Text = "";

            cbGITProject.SelectedIndex = -1;
            cbGITProject.Text = "";

            cbDEVProject.SelectedIndex = -1;
            cbDEVProject.Text = "";

            cbTypeObject.SelectedIndex = -1;
            cbTypeObject.Items.Clear();
            cbTypeObject.Items.Add("DATA");
            cbTypeObject.Items.Add("FUNCTION");
            cbTypeObject.Items.Add("PROCEDURE");
            cbTypeObject.Items.Add("SCHEMA");
            cbTypeObject.Items.Add("SEQUENCE");
            cbTypeObject.Items.Add("TABLE");
            cbTypeObject.Items.Add("TRIGGER");
            cbTypeObject.Items.Add("TYPE");
            cbTypeObject.Items.Add("VIEW");
            cbTypeObject.Items.Add("freedocmarker");
            cbTypeObject.Items.Add("freedocrelationship");
            cbTypeObject.Text = "";

            cbShemaObject.SelectedIndex = -1;
            cbShemaObject.Items.Clear();
            cbShemaObject.Text = "";

            cbGITNameObject.SelectedIndex = -1;
            cbGITNameObject.Items.Clear();
            cbGITNameObject.Text = "";

            cbDEVNameObject.SelectedIndex = -1;
            cbDEVNameObject.Items.Clear();
            cbDEVNameObject.Text = "";

            tbGITFilename.Text = "";
        }

        /// <summary>
        /// Заранее подготовленный список типов объектов
        /// </summary>
        private List<string> listTypeObject = new List<string>();

        /// <summary>
        /// Заполнение списка типов объектов
        /// </summary>
        /// <param name="list">глобальный список</param>
        /// <param name="dboFolder">каталог схемы dbo</param>
        private List<string> fillListTypeObject(List<string> list, string dboFolder)
        {
            List<string> result = list;
            if (result == null) result = new List<string>();

            if (!Directory.Exists(dboFolder)) return result;

            App.AddLog($"Заполняем типы объектов из {dboFolder}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

            foreach (var item in Utilities.Files.ListFilesInDir(dboFolder, true, false, false)
                .Where(x =>
                    (x.ToLower() != "data") &&
                    (x.ToLower() != "data_new") &&
                    (x.ToLower() != "v_lpu_all")
                    ))
            {
                if (!result.Contains(item))
                {
                    result.Add(item);
                }
            }

            App.AddLog($"Заполнен список типов объектов из {dboFolder}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

            return result;
        }

        /// <summary>
        /// Заранее подготовленный список схем
        /// </summary>
        private List<string> listSchema = new List<string>();


        /// <summary>
        /// Заполнение списка схем
        /// </summary>
        /// <param name="list">глобальный список</param>
        /// <param name="folder">каталог со схемами</param>
        private List<string> fillListSchema(List<string> list, string folder)
        {
            List<string> result = list;
            if (result == null) result = new List<string>();

            if (!Directory.Exists(folder)) return result;

            App.AddLog($"Заполняем список схем из {folder} ", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

            foreach (var item in Utilities.Files.ListFilesInDir(folder, true, false, false)
            .Where(x =>
                (x.ToLower() != "data") &&
                (x.ToLower() != "data_new") &&
                (x.ToLower() != "data(copy)") &&
                (x.ToLower() != "data(bulk)") &&
                (x.ToLower() != "task") &&
                (x.ToLower() != "deployment") &&
                (x.ToLower() != "cron") &&
                (x.ToLower() != "report") &&
                (x.ToLower() != "version") &&
                (x.ToLower() != "sqlint") &&
                (x.ToLower() != "utility") &&
                (x.ToLower() != ".git")
                )
            )
            {
                if (!result.Contains(item))
                {
                    result.Add(item);
                }
            }

            App.AddLog($"Заполнен список схем из {folder}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

            return result;
        }

        /// <summary>
        /// Открыть выбранный файл и заполнить поля на форме
        /// </summary>
        /// <param name="file">Выбранный файл</param>
        /// <param name="errors">Строка с ошибками</param>
        /// <returns>true - открыт успешно</returns>
        private bool OpenScript(string file, out string errors)
        {
            errors = "";

            if (string.IsNullOrWhiteSpace(file))
            {
                errors = "Файл не выбран!";
                MessageBox.Show(errors);
                btOpen.Focus();
                return false;
            }

            if (!File.Exists(file))
            {
                errors = "Файл " + file + " не существует!";
                MessageBox.Show(errors);
                btOpen.Focus();
                return false;
            }

            App.AddLog($"----------------------------------------------------", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
            App.AddLog($"Открываем файл {file}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

            // очистим для перечитки
            listTypeObject.Clear();
            listSchema.Clear();

            IndexStr = "";

            // очистить все поля
            ClearFileds();

            // заполняем заново
            tbScriptFilename.Text = file;
            gitScript.GITScriptname = tbScriptFilename.Text.Trim();
            devScript.GITScriptname = gitScript.GITScriptname;

            file = Path.GetFileNameWithoutExtension(tbScriptFilename.Text.Trim()).Replace(".", "_");
            var arr = file.ToLower().Split(' ');

            // Заполняем Имя файла
            tbGITFilename.Text = GetGITFilenameFromScriptFilename(file.ToLower());

            string git_project = "";
            string dev_project = "";

            // Заполняем Проект GIT
            for (int i = 0; (i < 1) && (i < arr.Length); i++) //-V3063
            {
                var s = arr[i].ToLower();

                git_project = Utilities.GITProjects.GITProjectsParam("PrefixFileSQL", s, "GITProject");
                dev_project = Utilities.GITProjects.GITProjectsParam("PrefixFileSQL", s, "DEVProject");

                if (
                    (!string.IsNullOrWhiteSpace(git_project)) &&
                    (!string.IsNullOrWhiteSpace(dev_project))
                    )
                {
                    // прочитаем список схем каждого из проектов
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, Utilities.GITProjects.GetFolderByProject(git_project));
                    listSchema = fillListSchema(listSchema, folder);
                    folder = Path.Combine(MainWindow.APPinfo.GITFolder, Utilities.GITProjects.GetFolderByProject(dev_project));
                    listSchema = fillListSchema(listSchema, folder);

                    // прочитаем список типов из схемы dbo каждого из проектов
                    folder = Path.Combine(MainWindow.APPinfo.GITFolder, Utilities.GITProjects.GetFolderByProject(git_project), "dbo");
                    listTypeObject = fillListTypeObject(listTypeObject, folder);
                    folder = Path.Combine(MainWindow.APPinfo.GITFolder, Utilities.GITProjects.GetFolderByProject(dev_project), "dbo");
                    listTypeObject = fillListTypeObject(listTypeObject, folder);

                    // заполняем поля формы
                    cbGITProject.Text = git_project;
                    cbGITProject_Sync();

                    cbDEVProject.Text = dev_project;
                    cbDEVProject_Sync();

                    break;
                }
            }

            string DBType = Utilities.GITProjects.GetDBTypeByProject(dev_project);

            if (
                isAddToDEV.Checked &&
                string.IsNullOrWhiteSpace(DBType)
                )
            {
                errors = $"Неизвестный тип БД для проекта {dev_project}";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                btOpen.Focus();
                return false;
            }

            // Заполняем Тип объекта
            for (int i = 0; i < arr.Length; i++)
            {
                var s = arr[i].ToLower();

                if (GITProjects.List_ScriptType_GITType.ContainsKey(s))
                {
                    cbTypeObject.Text = GITProjects.List_ScriptType_GITType[s];
                    cbTypeObject_Sync(true, true);

                    if (
                            (s == "index") ||
                            (s == "idx")
                    )
                    {
                        IndexStr = "_idx";
                    }

                    if (
                            (s == "bulk") ||
                            (s == "copy")
                    )
                    {
                        if (devScript.DBType == "MSSQL")
                            IndexStr = "_bulk";
                        else
                            IndexStr = "_copy";
                    }

                    break;
                }
            }

            // Заполняем Схему
            if (
                    (arr.Length >= 5) &&
                    (cbTypeObject.Text != "DATA")
            )
            {
                cbShemaObject.Text = arr[4].ToLower();
                cbShemaObject_Sync(true, true);
            }

            // Заполняем Имя объекта
            if (
                    (arr.Length >= 6) &&
                    (cbTypeObject.Text != "DATA")
            )
            {
                cbGITNameObject.Text = arr[5];
                cbGITNameObject_Sync();

                cbDEVNameObject.Text = arr[5];
                cbDEVNameObject_Sync();
            }

            if (
                (arr.Length >= 6) &&
                (cbTypeObject.Text == "DATA")
            )
            {
                cbGITNameObject.Text = arr[4] + "." + arr[5];
                cbGITNameObject_Sync();

                cbDEVNameObject.Text = arr[4] + "." + arr[5];
                cbDEVNameObject_Sync();
            }

            if (!string.IsNullOrWhiteSpace(gitScript.GITNameObject))
            {
                tbGITFilename.Text = GetGITFilenameFromForm(gitScript, tbGITFilename.Text);
            }
            tbGITFilename_Sync();

            // очистим, чтобы при ручном выборе на форме все считывалось заново
            listTypeObject.Clear();
            listSchema.Clear();

            App.AddLog($"Файл {file} открыт, поля на форме \"Новый скрипт для отправки в GIT\" заполнены", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

            return true;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Кнопка Выбрать файл</summary>
        private void btOpen_Click(object sender, EventArgs e)
        {
            string file = Dialogs.OpenFileDialog(MainWindow.Task.TaskPath);
            string errors = "";
            if (OpenScript(file, out errors))
            {
                lastFile = ForOrderBy(Path.GetFileName(file));
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Собрать имя файла для GIT из полей формы</summary>
        private string GetGITFilenameFromForm(GITScript script, string filename)
        {
            filename = filename.Trim().Replace("_", ".");
            string res = filename;

            // определяем имя файла для структуры, кода, маркеров
            if (
                    (script.GITProject != "") &&
                    (script.GITTypeObject != "") &&
                    (script.GITTypeObject != "SCHEMA") &&
                    (script.GITKindObject != "DATA") &&
                    (script.GITShemaObject != "") &&
                    (script.GITNameObject != "")
            )
            {
                if (
                    (filename != script.GITNameObject) &&
                    (filename != script.GITNameObject + "s") &&
                    (filename != script.GITNameObject + "l")
                    )
                {
                    res = script.GITNameObject;

                    if (res.EndsWith("_in"))
                    {
                        res = res + "s";
                    }

                    if (res.EndsWith("_de"))
                    {
                        res = res + "l";
                    }
                }
            }
            // определяем имя файла для create schema
            else if (
                    (script.GITProject != "") &&
                    (script.GITTypeObject == "SCHEMA")
            )
            {
                res = script.GITShemaObject;
            }
            // определяем имя файла для data
            else if (
                    (script.GITProject != "") &&
                    (script.GITKindObject == "DATA") &&
                    (!string.IsNullOrWhiteSpace(script.GITNameObject))
            )
            {
                string schema = script.GITShemaObject;
                string checkname = "";

                if (string.IsNullOrWhiteSpace(schema))
                {
                    schema = "dbo";
                }

                if (script.GITNameObject.Contains("."))
                {
                    checkname = script.GITNameObject;
                }
                else
                {
                    checkname = schema + "." + script.GITNameObject;
                }

                if (
                    (filename != checkname) &&
                    (filename != checkname + "s") &&
                    (filename != checkname + "l")
                )
                {
                    res = checkname;

                    if (res.EndsWith("_in"))
                    {
                        res = res + "s";
                    }

                    if (res.EndsWith("_de"))
                    {
                        res = res + "l";
                    }
                }
            }

            res = res.Replace(".", "_").Replace(" ", "_").Replace("-", "");
            return res.Trim();
        }

        /// <summary>
        /// Исправить имя объекта в соответствии с "реальным" из списка
        /// </summary>
        /// <param name="cb">список</param>
        /// <param name="objectname">имя объекта</param>
        /// <returns></returns>
        private string CorrectNameObject(ComboBox cb, string objectname)
        {
            if (string.IsNullOrWhiteSpace(objectname))
            {
                return "";
            }

            List<string> objects = cb.Items.OfType<string>().ToList();

            string find = objectname.Trim();

            string found = objects.Where(x => x.ToUpper() == find.ToUpper()).FirstOrDefault();

            if (string.IsNullOrWhiteSpace(found))
            {
                // если не нашли, попробуем убрать последнюю букву s или l или q
                find = objectname.Trim();
                if (
                        find.ToLower().EndsWith("s") ||
                        find.ToLower().EndsWith("l") ||
                        find.ToLower().EndsWith("q")
                    )
                {
                    find = find.Substring(0, find.Length - 1);
                    found = objects.Where(x => x.ToUpper() == find.ToUpper()).FirstOrDefault();
                }
            }

            if (string.IsNullOrWhiteSpace(found))
            {
                // если не нашли, попробуем убрать 2-е последние буквы ss или ll или ls
                find = objectname.Trim();
                if (
                        find.ToLower().EndsWith("ss") ||
                        find.ToLower().EndsWith("ll") ||
                        find.ToLower().EndsWith("ls")
                    )
                {
                    find = find.Substring(0, find.Length - 2);
                    found = objects.Where(x => x.ToUpper() == find.ToUpper()).FirstOrDefault();
                }
            }

            if (string.IsNullOrWhiteSpace(found))
            {
                // если не нашли, попробуем добавить последнюю букву s
                find = objectname.Trim() + "s";
                found = objects.Where(x => x.ToUpper() == find.ToUpper()).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    // если не нашли, попробуем добавить еще одну букву s
                    find = find + "s";
                    found = objects.Where(x => x.ToUpper() == find.ToUpper()).FirstOrDefault();
                }
            }

            if (string.IsNullOrWhiteSpace(found))
            {
                // если не нашли, попробуем добавить последнюю букву l
                find = objectname.Trim() + "l";
                found = objects.Where(x => x.ToUpper() == find.ToUpper()).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(found))
                {
                    // если не нашли, попробуем добавить еще одну букву l
                    string find1 = find + "l";
                    found = objects.Where(x => x.ToUpper() == find1.ToUpper()).FirstOrDefault();
                }

                if (string.IsNullOrWhiteSpace(found))
                {
                    // если не нашли, попробуем добавить еще одну букву s
                    string find1 = find + "s";
                    found = objects.Where(x => x.ToUpper() == find1.ToUpper()).FirstOrDefault();
                }
            }

            if (string.IsNullOrWhiteSpace(found))
            {
                // если не нашли, попробуем добавить последнюю букву q
                find = objectname.Trim() + "q";
                found = objects.Where(x => x.ToUpper() == find.ToUpper()).FirstOrDefault();
            }

            if (string.IsNullOrWhiteSpace(found))
            {
                found = objectname;
            }

            return found;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Добавить скрипт в список
        /// </summary>
        /// <param name="errors">ошибки</param>
        /// <returns></returns>
        public bool AddScirpt(out string errors)
        {
            if (string.IsNullOrWhiteSpace(devScript.GITScriptname) || (!File.Exists(devScript.GITScriptname)))
            {
                errors = $"Файл {devScript.GITScriptname} не существует, необходимо выбрать существующий файл для добавления в GIT!";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tbScriptFilename.Focus();
                return false;
            }

            if (
                (!isAddToGIT.Checked) &&
                (!isAddToDEV.Checked)
                )
            {
                errors = "Надо выбрать добавление хотя бы в один из проектов!";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                isAddToDEV.Focus();
                return false;
            }

            if (
                isAddToDEV.Checked &&
                string.IsNullOrWhiteSpace(devScript.GITProject)
                )
            {
                errors = $"Необходимо выбрать проект разработки";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                cbDEVProject.Focus();
                return false;
            }

            if (
                isAddToDEV.Checked &&
                string.IsNullOrWhiteSpace(devScript.DBType)
                )
            {
                errors = $"Неизвестный тип БД для проекта {devScript.GITProject}";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tbScriptFilename.Focus();
                return false;
            }

            if (
                isAddToGIT.Checked &&
                string.IsNullOrWhiteSpace(gitScript.GITProject)
             )
            {
                errors = $"Необходимо выбрать версионный проект";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                cbGITProject.Focus();
                return false;
            }

            if (
                isAddToGIT.Checked &&
                string.IsNullOrWhiteSpace(gitScript.DBType)
            )
            {
                errors = $"Неизвестный тип БД для проекта {gitScript.GITProject}";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tbScriptFilename.Focus();
                return false;
            }

            bool isGITExist = false;
            bool isDEVExist = false;
            errors = "";
            bool isChecked = false;

            // -------------------------------------------------------------------------------------------------------------------
            // "новый" проект разработки
            // -------------------------------------------------------------------------------------------------------------------
            if (isAddToDEV.Checked)
            {
                App.AddLog("------------------------------------------------------------------", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                App.AddLog("Добавляем скрипт из нового проекта", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                // проверим наличие проекта GIT
                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, devScript.GITProjectFolder);

                if (!Directory.Exists(folder))
                {
                    errors = "Проект " + devScript.GITProject + " НЕ клонирован!";
                    App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    tbScriptFilename.Focus();

                    return false;

                }

                // проверим наличие папки в проекте GIT
                string path = devScript.GITFilepath;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    App.AddLog($"Сейчас создадим папку {path}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                    Directory.CreateDirectory(path);
                }

                if (!Directory.Exists(path))
                {
                    errors = $"Не найдена папка {path} для проекта {devScript.GITProject} !";
                    App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    tbScriptFilename.Focus();

                    return false;
                }

                App.AddLog($"Папка {path} существует", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                // проверим его текущую ветку
                string err = "";
                string branch = GIT.GitCurrentBranch(devScript.GITProject, out err, MainWindow.Task.LogFile);
                tbBranch.Text = branch;

                if (string.IsNullOrWhiteSpace(branch))
                {
                    App.AddLog("У проекта " + devScript.GITProject + " не определилась ветка!" + Environment.NewLine
                        + Environment.NewLine + err, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);

                    return false;
                }

                // Проверка SQL-скрипта
                bool isCheckRegion = false;

                var arr = devScript.GITFilename.Split('_');
                if (
                        (arr.Length >= 1) && //-V3063
                        Utilities.Databases.regex_region.IsMatch(arr[0])
                )
                {
                    isCheckRegion = true;
                }

                if (Utilities.Databases.regex_region.IsMatch(devScript.GITShemaObject))
                {
                    isCheckRegion = true;
                }

                if (
                    cbCheck.Checked == true &&
                    Utilities.YML.IsSQLFileBAD(devScript.GITProject, devScript.GITTypeObject, null, devScript.GITScriptname, true, true, isCheckRegion, false, false, ref errors, false, MainWindow.Task.LogFile, true)
                    )
                {
                    if (System.Windows.Forms.MessageBox.Show(errors + Environment.NewLine + Environment.NewLine +
                        $"Добавить файл {devScript.GITScriptname} для последующей отправки в GIT ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                    {
                        App.AddLog(errors, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        App.AddLog($"Выбрано - Не добавлять файл {devScript.GITScriptname} для последующей отправки в {devScript.GITProject}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                        return false;
                    }
                }

                isChecked = true;

                //предложим переключиться или создать ветку текущей задачи
                while (branch != MainWindow.Task.TaskNumber)
                {
                    // ветка другая, надо сменить
                    if (System.Windows.Forms.MessageBox.Show("Сейчас в проекте " + devScript.GITProject + " текущая ветка " + branch + Environment.NewLine +
                            Environment.NewLine +
                            "Сменить ветку в проекте " + devScript.GITProject + " на " + MainWindow.Task.TaskNumber + " ?",
                            "ВНИМАНИЕ",
                            System.Windows.Forms.MessageBoxButtons.YesNo
                        ) == System.Windows.Forms.DialogResult.Yes
                    )
                    {
                        App.AddLog($"В проекте {devScript.GITProject} текущая ветка {branch}, выбрана смена ветки на {MainWindow.Task.TaskNumber}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                        // есть новые проекты, надо создать/переключиться на ветку задачи
                        GIT.GitNewBranch(new string[] { devScript.GITProject }, MainWindow.Task.TaskNumber, "master", MainWindow.Task.LogFile);

                        // покажем текущую ветку
                        err = "";
                        branch = GIT.GitCurrentBranch(devScript.GITProject, out err, MainWindow.Task.LogFile);
                        tbBranch.Text = branch;
                    }
                    else
                    {
                        App.AddLog($"В проекте {devScript.GITProject} текущая ветка {branch}, пользователь отказался менять нана {MainWindow.Task.TaskNumber}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                        return false;
                    }
                }

                // если проект клонирован и есть текущая ветка
                isDEVExist = true;

                // Добавить в список для GIT
                listDevScripts.Clear();
                bool isFirst = true;
                string uniq_from_name = devScript.GITScriptname;
                int uniq_from_cnt = 0;
                string file = "";
                string uniq_to_name = devScript.GITFilename;
                int uniq_to_cnt = 0;
                string firstChangesetName = "";

                do
                {
                    // проверяем на уникальность файла (как в текущей задаче, так и в папке GIT)
                    string newGITFilename = Path.GetFileNameWithoutExtension(uniq_to_name).ToLower();

                    if (!devScript.isSingleScript) // сохраняем в разные файлы
                    {
                        file = Path.Combine(path, newGITFilename) + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + MainWindow.Task.TaskNumber.ToLower() + IndexStr + ".sql";

                        App.AddLog($"Проверяем уникальность файла {file}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                        while (
                                File.Exists(file) ||
                                (
                                    (listScripts != null) &&
                                    listScripts.Exists(s =>
                                            (s.GITFilename == Path.GetFileName(file).ToLower()) &&
                                            (s.GITProject == devScript.GITProject)
                                         )
                                )
                        )
                        {
                            uniq_to_cnt++;
                            file = Path.Combine(path, newGITFilename + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + MainWindow.Task.TaskNumber.ToLower() + IndexStr + "_" + uniq_to_cnt.ToString()) + ".sql";
                        };
                    }
                    else
                    {
                        file = Path.Combine(path, newGITFilename) + ".sql";
                    }

                    newGITFilename = Path.GetFileName(file).ToLower();

                    firstChangesetName = "";

                    // проверим уникальность changeset таблицы в рамках текущей задачи
                    if (devScript.GITTypeObject == "TABLE")
                    {
                        SQLChangeset.FirstChangesetName(devScript.GITScriptname, out SQLChangeset _changeset);
                        if (_changeset != null)
                        {
                            firstChangesetName =
                                (string.IsNullOrWhiteSpace(_changeset.author) ? "" : _changeset.author + ":") +
                                _changeset.name;
                        }
                    }

                    App.AddLog($"В итоге будет добавлен новый changeset {firstChangesetName} в файл {file}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                    if (
                        (!string.IsNullOrWhiteSpace(firstChangesetName)) &&
                        (listScripts != null)
                    )
                    {
                        var found = listScripts.Find(s =>
                                            (s.FirstChangesetName == firstChangesetName) &&
                                            (s.GITFilename == newGITFilename) &&
                                            (s.GITShemaObject == devScript.GITShemaObject) &&
                                            (s.GITProject == devScript.GITProject)
                                         );
                        if (found == null)
                        {
                            found = listDevScripts.Find(s =>
                                            (s.FirstChangesetName == firstChangesetName) &&
                                            (s.GITFilename == newGITFilename) &&
                                            (s.GITShemaObject == devScript.GITShemaObject) &&
                                            (s.GITProject == devScript.GITProject)
                                         );
                        }

                        if (found != null) 
                        {
                            App.AddLog($"В файле {devScript.GITScriptname} - первый changeset {firstChangesetName}\nТакой же changeset уже есть в файле {found.GITScriptname} для проекта {devScript.GITProject}\n\nИзмените имя changeset для добавления в список отправки в GIT", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);

                            break;
                        }
                    }

                    devScript.FirstChangesetName = firstChangesetName;
                    devScript.GITFilename = newGITFilename;

                    // добавляем в список для отправки
                    listDevScripts.Add(devScript.Copy());

                    if (devScript.isSingleScript) // добавляем все changeset в один файл
                    {
                        break;
                    }

                    // Проверяем, есть ли в файле несколько changeset
                    var list = SQLChangeset.ReadChangeset(devScript.GITScriptname, false, MainWindow.Task.LogFile).ToList();

                    if (list.Count() <= 1)
                    {
                        // НЕ больше одного changeset
                        break;
                    }

                    if (isFirst)
                    {
                        // сделаем резервную копию
                        Utilities.Files.BackupFile(devScript.GITScriptname);
                    }

                    // подбираем новое имя
                    string fromfile = uniq_from_name;
                    string tofile = Path.Combine(MainWindow.Task.TaskTO_GIT, Path.GetFileName(uniq_from_name));

                    while (
                        File.Exists(fromfile) ||
                        File.Exists(tofile)
                    )
                    {
                        uniq_from_cnt++;
                        fromfile = Path.Combine(Path.GetDirectoryName(uniq_from_name), Path.GetFileNameWithoutExtension(uniq_from_name) + " " + uniq_from_cnt.ToString()) + ".sql";
                        tofile = Path.Combine(MainWindow.Task.TaskTO_GIT, Path.GetFileNameWithoutExtension(uniq_from_name) + " " + uniq_from_cnt.ToString()) + ".sql";
                    };

                    // сохраним 1-й changeset
                    list[0].SaveFileWithChangeset(devScript.GITScriptname, isFirst, false, MainWindow.Task.LogFile);

                    // все последующие тестовые changeset добавляем к 1-му
                    /* 
                    for (i = 1; i < list.Count(); i++)
                    {
                        if (!list[i].isTestChangeset)
                        {
                            i--;
                            break;
                        }
                        list[i].SaveFileWithChangeset(devScript.GITScriptname, false, true);
                    }
                    */

                    // сохраняем в новом файле остальные changeset (без тега liquibase)
                    SQLChangeset.SaveFileWithChangeset(list, 1, list.Count() - 1, fromfile, false, MainWindow.Task.LogFile);

                    // для обработки остальных changeset
                    devScript.GITScriptname = fromfile;
                    isFirst = false;

                } while (true);

            }

            // -------------------------------------------------------------------------------------------------------------------
            // "старый" версионный проект
            // -------------------------------------------------------------------------------------------------------------------
            if (isAddToGIT.Checked)
            {
                // проверим наличие проекта GIT
                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, gitScript.GITProjectFolder);

                if (!Directory.Exists(folder))
                {
                    errors = "Проект " + gitScript.GITProject + " НЕ клонирован!";
                    App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    tbScriptFilename.Focus();
                    return false;

                }

                // проверим наличие папки в проекте GIT
                string path = gitScript.GITFilepath;
                if (!string.IsNullOrWhiteSpace(path))
                {
                    Directory.CreateDirectory(path);
                }

                if (!Directory.Exists(path))
                {
                    errors = $"Не найдена папка GIT {path} для проекта {gitScript.GITProject} !";
                    App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    tbScriptFilename.Focus();
                    return false;
                }

                // Проверка SQL-скрипта
                if (!isChecked)
                {
                    // чтобы 2-й раз не проверять

                    bool isCheckRegion = false;

                    var arr = gitScript.GITFilename.Split('_');
                    if (
                            (arr.Length >= 1) && //-V3063
                            Utilities.Databases.regex_region.IsMatch(arr[0])
                    )
                    {
                        isCheckRegion = true;
                    }

                    if (Utilities.Databases.regex_region.IsMatch(gitScript.GITShemaObject))
                    {
                        isCheckRegion = true;
                    }

                    // проверка содержания файла
                    if (
                        cbCheck.Checked == true &&
                        Utilities.YML.IsSQLFileBAD(gitScript.GITProject, gitScript.GITTypeObject, null, gitScript.GITScriptname, true, true, isCheckRegion, false, false, ref errors, false, MainWindow.Task.LogFile, true)
                        )
                    {
                        if (System.Windows.Forms.MessageBox.Show(errors + Environment.NewLine + Environment.NewLine +
                            $"Добавить файл {gitScript.GITScriptname} для последующей отправки в GIT ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                        {
                            App.AddLog(errors, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                            App.AddLog($"Выбрано - Не добавлять файл {gitScript.GITScriptname} для последующей отправки в {gitScript.GITProject}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                            return false;
                        }
                    }

                    isChecked = true; //-V3137
                }

                // если проект клонирован
                isGITExist = true;

                gitScript.GITFilename = Path.GetFileNameWithoutExtension(gitScript.GITFilename).ToLower();

                // проверяем на уникальность файла (как в текущей задаче, так и в папке GIT)
                string file = Path.Combine(path, gitScript.GITFilename) + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + MainWindow.Task.TaskNumber.ToLower() + IndexStr + ".sql";
                int i = 0;

                while (
                        File.Exists(file) ||
                        (
                            (listScripts != null) &&
                            listScripts.Exists(s =>
                                    (s.GITFilename == Path.GetFileName(file).ToLower()) &&
                                    (s.GITProject == gitScript.GITProject)
                                    )
                        )
                )
                {
                    i++;
                    file = Path.Combine(path, gitScript.GITFilename + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + MainWindow.Task.TaskNumber.ToLower() + IndexStr + "_" + i.ToString()) + ".sql";
                };

                gitScript.GITFilename = Path.GetFileName(file).ToLower();
            }

            // -------------------------------------------------------------------------------------------------------------------
            // добавляю скрипты в перечень скриптов задачи
            // -------------------------------------------------------------------------------------------------------------------
            if (MainWindow.Task.Scripts == null)
            {
                MainWindow.Task.Scripts = new List<GITScript>();
            }

            if (isDEVExist && isAddToDEV.Checked)
            {
                foreach (var item in listDevScripts)
                {
                    try
                    {
                        App.AddLog($"Создаем папку {MainWindow.Task.TaskTO_GIT}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                        Directory.CreateDirectory(MainWindow.Task.TaskTO_GIT);

                        // переносим в каталог на отправку
                        if (File.Exists(item.GITScriptname))
                        {
                            App.AddLog($"Переносим файл {item.GITScriptname} в {item.FullSourceScriptname_TO_GIT}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                            File.Move(item.GITScriptname, item.FullSourceScriptname_TO_GIT);
                        }

                        // добавляем в перечень скрипт для "нового" проекта (если он клонирован)
                        MainWindow.Task.AddScript(
                            null,
                            item.GITScriptname,
                            item.GITProject,
                            item.GITTypeObject_TO_GIT,
                            item.GITShemaObject,
                            item.GITNameObject,
                            item.GITFilename,
                            item.IsExistInGIT,
                            item.FirstChangesetName
                            );
                    }
                    catch (Exception ex)
                    {
                        errors = App.AddLog("Ошибка при переносе файла " + item.GITScriptname + " в каталог " + Path.GetDirectoryName(item.FullSourceScriptname_TO_GIT), ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile).showMessage;
                        return false;
                    }
                }
            }

            if (isGITExist && isAddToGIT.Checked)
            {
                try
                {
                    App.AddLog($"Создаем папку {MainWindow.Task.TaskTO_GIT}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                    Directory.CreateDirectory(MainWindow.Task.TaskTO_GIT);

                    // переносим в каталог на отправку
                    if (File.Exists(gitScript.GITScriptname))
                    {
                        App.AddLog($"Переносим файл {gitScript.GITScriptname} в {gitScript.FullSourceScriptname_TO_GIT}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                        File.Move(gitScript.GITScriptname, gitScript.FullSourceScriptname_TO_GIT);
                    }

                    // добавляем в перечень скрипт для "старого" проекта (если он клонирован)
                    MainWindow.Task.AddScript(
                        null,
                        gitScript.GITScriptname,
                        gitScript.GITProject,
                        gitScript.GITTypeObject_TO_GIT,
                        gitScript.GITShemaObject,
                        gitScript.GITNameObject,
                        gitScript.GITFilename,
                        gitScript.IsExistInGIT,
                        gitScript.FirstChangesetName
                        );
                }
                catch (Exception ex)
                {
                    errors = App.AddLog("Ошибка при переносе файла " + gitScript.GITScriptname + " в каталог " + Path.GetDirectoryName(gitScript.FullSourceScriptname_TO_GIT), ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile).showMessage;
                    return false;
                }
            }

            return true;
        }



        // -------------------------------------------------------------------------------------------------------
        /// <summary>Кнопка Добавить и завершить</summary>
        private void btAdd_Click(object sender, EventArgs e)
        {
            string errors = "";

            if (AddScirpt(out errors))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        /// <summary>Кнопка Завершить</summary>
        private void btCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// Конвертация имени файла в вид, удобный для сортировки
        /// </summary>
        /// <param name="filename">имя файла без пути</param>
        /// <returns>строка для использования в сортировке</returns>
        private string ForOrderBy(string filename)
        {
            filename = Path.GetFileNameWithoutExtension(filename)
                .ToLower()
                .Replace('\t', ' ')
                .TrimInner()
                .Trim();

            var arr = filename.ToLower().Split(' ');
            int cnt = 0;

            string res = "";

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != "")
                {
                    cnt++;
                }

                if (cnt == 3)
                {
                    arr[i] = arr[i].PadLeft(20, '0');
                }

                if (cnt == 7)
                {
                    arr[i] = arr[i].PadLeft(3, '0');
                }

                if (arr[i] != "")
                {
                    res += arr[i] + " ";
                }
            }

            for (int i = cnt; i < 7; i++)
            {
                res += "000" + " ";
            }

            return res.Trim();
        }

        /// <summary>
        /// определить следующий файл-скрипт в папке задачи для отправки в GIT
        /// </summary>
        /// <returns></returns>
        private string GetNextFile()
        {
            string file = "";
            Regex regex_file = new Regex(@"^(\w+)(\u0020+)(\w+)(\u0020+)(\d+)(\u0020+)(\w+)(\u0020+)(\w+)(\u0020+)(\w+)(.*).sql$");

            App.AddLog($"Ищем следующий файл в {MainWindow.Task.TaskPath} для добавления", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

            // соберем список подходящих файлов, приведем имя файла для удобства сортировки
            List<FileSQLInfo> files = new List<FileSQLInfo>();
            foreach (var item in Utilities.Files.ListFilesInDir(MainWindow.Task.TaskPath, false, true, false))
            {
                if (regex_file.IsMatch(item))
                {
                    FileSQLInfo fi = new FileSQLInfo();
                    fi.Name = item;
                    fi.Order = ForOrderBy(item);
                    files.Add(fi);
                }
            }

            // найдем следующий подходящий файл
            file = "";
            foreach (var item in files.OrderBy(x => x.Order).Where(x => string.CompareOrdinal(x.Order, lastFile) > 0))
            {
                file = Path.Combine(MainWindow.Task.TaskPath, item.Name);
                lastFile = item.Order;
                break;
            }

            App.AddLog($"Найден: {file}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

            return file;
        }

        /// <summary>
        /// выбрать следующий файл и заполнить поля на форме
        /// </summary>
        public void GetNextFileAndFillForm()
        {
            // 1. Очистить форму
            ClearFileds();

            // 2. Найти следующий файл
            string file = GetNextFile();

            if (string.IsNullOrWhiteSpace(file))
            {
                MessageBox.Show("Нет следующего файла - кандидата на добавление !");

                // если следующий файл не найден - очищаем поля на форме
                ClearFileds();
                return;
            }

            if (!File.Exists(file))
            {
                MessageBox.Show("Файл " + file + " не существует!");

                // если следующий файл не существует - очищаем поля на форме
                ClearFileds();
                return;
            }

            // 3. Выбрать следующий файл
            string errors = "";
            if (!OpenScript(file, out errors))
            {
                MessageBox.Show("Файл " + file + " не может быть добавлен: " + errors);

                // если файл не был успешно открыт - очищаем поля на форме
                ClearFileds();
                return;
            }
        }

        /// <summary>Кнопка Добавить и перейти к следующему</summary>
        private void btAddAndNext_Click(object sender, EventArgs e)
        {
            string errors = "";

            // Добавить
            if (AddScirpt(out errors))
            {
                // закроем форму чтобы обновить список скриптов
                this.DialogResult = DialogResult.Retry;
                this.Close();
            }
        }

        /// <summary>Кнопка Пропустить и перейти к следующему</summary>
        private void btNext_Click(object sender, EventArgs e)
        {
            // найти следующий файл и заполнить поля
            GetNextFileAndFillForm();
        }

        private void AddToGit_Sync()
        {
            lbDEVProject.Visible = isAddToDEV.Checked;
            cbDEVProject.Visible = isAddToDEV.Checked;

            lbGITProject.Visible = isAddToGIT.Checked;
            cbGITProject.Visible = isAddToGIT.Checked;

            lbBranch.Visible = isAddToDEV.Checked;
            tbBranch.Visible = isAddToDEV.Checked;

            lbTypeObject.Visible = isAddToDEV.Checked || isAddToGIT.Checked;
            cbTypeObject.Visible = isAddToDEV.Checked || isAddToGIT.Checked;

            lbShemaObject.Visible = isAddToDEV.Checked || isAddToGIT.Checked;
            cbShemaObject.Visible = isAddToDEV.Checked || isAddToGIT.Checked;

            lbDEVNameObject.Visible = isAddToDEV.Checked;
            cbDEVNameObject.Visible = isAddToDEV.Checked;

            lbGITNameObject.Visible = isAddToGIT.Checked;
            cbGITNameObject.Visible = isAddToGIT.Checked;

            btCompare.Enabled = isAddToDEV.Checked && devScript.isSingleScript;

            if (isAddToGIT.Checked)
            {
            }

            if (!isAddToGIT.Checked)
            {
            }

            if (isAddToDEV.Checked)
            {
                lbTypeObject.Location = new Point(lbDEVProject.Location.X, lbTypeObject.Location.Y);
                cbTypeObject.Location = new Point(cbDEVProject.Location.X, cbTypeObject.Location.Y);

                lbShemaObject.Location = new Point(lbDEVProject.Location.X, lbShemaObject.Location.Y);
                cbShemaObject.Location = new Point(cbDEVProject.Location.X, cbShemaObject.Location.Y);
            }

            if (!isAddToDEV.Checked)
            {
                if (isAddToGIT.Checked)
                {
                    lbTypeObject.Location = new Point(lbGITProject.Location.X, lbTypeObject.Location.Y);
                    cbTypeObject.Location = new Point(cbGITProject.Location.X, cbTypeObject.Location.Y);

                    lbShemaObject.Location = new Point(lbGITProject.Location.X, lbShemaObject.Location.Y);
                    cbShemaObject.Location = new Point(cbGITProject.Location.X, cbShemaObject.Location.Y);
                }
            }
        }

        /// <summary>
        /// Добавляем в проект разработки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isAddToGIT_CheckedChanged(object sender, EventArgs e)
        {
            AddToGit_Sync();
        }

        /// <summary>
        /// Добавляем в версионный проект
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void isAddToDEV_CheckedChanged(object sender, EventArgs e)
        {
            AddToGit_Sync();
        }

        private void FormAddScript_FormClosed(object sender, FormClosedEventArgs e)
        {
            // пользовательские настройки GUI
            Default.SaveGUI("FormAddScript", this);

            // сохраняем значения 
            if (isAddToDEV.Checked)
            {
                MainWindow.APPinfo.IsAddToDEV = "true";
            }
            else
            {
                MainWindow.APPinfo.IsAddToDEV = "false";
            }

            if (isAddToGIT.Checked)
            {
                MainWindow.APPinfo.IsAddToGIT = "true";
            }
            else
            {
                MainWindow.APPinfo.IsAddToGIT = "false";
            }

        }




        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выход из поля Проект DEV - проверки значения текущего поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDEVProject_Leave(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Изменение значения в поле Проект DEV - проверки значения текущего поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDEVProject_SelectedValueChanged(object sender, EventArgs e)
        {
            // проверка на изменение
            if (devScript.GITProject == cbDEVProject.Text.Trim())
            {
                return;
            }
            cbDEVProject_Sync();

            string git_project = Utilities.GITProjects.GITProjectsParam("DEVProject", devScript.GITProject, "GITProject");

            if (cbGITProject.Items.Contains(git_project)) cbGITProject.Text = git_project;
            else
            {
                gitScript.GITProject = "";
                cbGITProject.SelectedIndex = -1;
            }

            if (gitScript.GITProject == cbGITProject.Text.Trim())
            {
                return;
            }
            cbGITProject_Sync();
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Синхронизация с текущим значением поля cbDEVProject - действия с другими полями после изменения текущего
        /// </summary>
        private void cbDEVProject_Sync()
        {
            // проставляем проект DEV
            devScript.GITProject = cbDEVProject.Text.Trim();
            string folder = Path.Combine(MainWindow.APPinfo.GITFolder, devScript.GITProjectFolder);
            string branch = GIT.GitCurrentBranch(devScript.GITProject, out string err, MainWindow.Task.LogFile);
            tbBranch.Text = branch;

            // очищаем остальные поля (кроме имени файла)
            cbTypeObject.SelectedIndex = -1;
            cbTypeObject.Text = "";

            cbShemaObject.SelectedIndex = -1;
            cbShemaObject.Text = "";

            cbDEVNameObject.SelectedIndex = -1;
            cbDEVNameObject.Items.Clear();
            cbDEVNameObject.Text = "";

            List<string> list = null;

            if (listTypeObject.Count > 0)
            {
                // Список типов уже подготовлен
                list = listTypeObject;
            }
            else
            {
                // перечитаем заново из папки dbo проекта
                string dboFolder = Path.Combine(folder, "dbo");
                list = fillListTypeObject(null, dboFolder);
            }

            // дополняем список типов объектов
            foreach (var item in list)
            {
                if (!cbTypeObject.Items.Contains(item))
                {
                    cbTypeObject.Items.Add(item);
                }
            }

            cbTypeObject_Sync(false, true);
            cbShemaObject_Sync(false, true);
            cbDEVNameObject_Sync();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выход из поля Проект GIT - проверки значения текущего поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGITProject_Leave(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Изменение значения в поле Проект GIT - проверки значения текущего поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGITProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            // проверка на изменение
            if (gitScript.GITProject == cbGITProject.Text.Trim())
            {
                return;
            }
            cbGITProject_Sync();

            string dev_project = Utilities.GITProjects.GITProjectsParam("GITProject", gitScript.GITProject, "DEVProject");

            if (cbDEVProject.Items.Contains(dev_project)) cbDEVProject.Text = dev_project;
            else
            {
                devScript.GITProject = "";
                cbDEVProject.SelectedIndex = -1;
            }

            if (devScript.GITProject == cbDEVProject.Text.Trim())
            {
                return;
            }
            cbDEVProject_Sync();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Синхронизация с текущим значением поля cbGITProject - действия с другими полями после изменения текущего
        /// </summary>
        private void cbGITProject_Sync()
        {
            // проставляем проект GIT
            gitScript.GITProject = cbGITProject.Text.Trim();

            string folder = Path.Combine(MainWindow.APPinfo.GITFolder, gitScript.GITProjectFolder);

            // очищаем остальные поля (кроме имени файла)
            cbTypeObject.SelectedIndex = -1;
            cbTypeObject.Text = "";

            cbShemaObject.SelectedIndex = -1;
            cbShemaObject.Text = "";

            cbGITNameObject.SelectedIndex = -1;
            cbGITNameObject.Items.Clear();
            cbGITNameObject.Text = "";

            List<string> list = null;

            if (listTypeObject.Count > 0)
            {
                // Список типов уже подготовлен
                list = listTypeObject;
            }
            else
            {
                // перечитаем заново из папки dbo проекта
                string dboFolder = Path.Combine(folder, "dbo");
                list = fillListTypeObject(null, dboFolder);
            }

            // дополняем список типов объектов
            foreach (var item in list)
            {
                if (!cbTypeObject.Items.Contains(item))
                {
                    cbTypeObject.Items.Add(item);
                }
            }

            cbTypeObject_Sync(true, false);
            cbShemaObject_Sync(true, false);
            cbGITNameObject_Sync();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// При выходе из поля Тип объекта - проверки значения текущего поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTypeObject_Leave(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Изменение значения в поле Тип объекта - проверки значения текущего поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbTypeObject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (gitScript.GITTypeObject == cbTypeObject.Text.Trim())
            {
                return;
            }
            cbTypeObject_Sync(true, true);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Синхронизация с текущим значением поля cbTypeObject - действия с другими полями после изменения текущего
        /// </summary>
        private void cbTypeObject_Sync(bool isGIT, bool isDEV)
        {
            // проставляем тип
            if (isGIT) gitScript.GITTypeObject = cbTypeObject.Text.Trim();
            if (isDEV) devScript.GITTypeObject = cbTypeObject.Text.Trim();

            btCompare.Enabled = isAddToDEV.Checked && devScript.isSingleScript;

            // очищаем остальные поля (кроме имени файла)
            cbShemaObject.SelectedIndex = -1;
            cbShemaObject.Items.Clear();
            cbShemaObject.Text = "";

            if (isGIT)
            {
                cbGITNameObject.SelectedIndex = -1;
                cbGITNameObject.Items.Clear();
                cbGITNameObject.Text = "";
            }

            if (isDEV)
            {
                cbDEVNameObject.SelectedIndex = -1;
                cbDEVNameObject.Items.Clear();
                cbDEVNameObject.Text = "";
            }

            // заполняем список объектов для скриптов по данным
            if (isGIT &&
                    (!string.IsNullOrWhiteSpace(gitScript.GITProject)) &&
                    (gitScript.GITKindObject == "DATA") &&
                    (!string.IsNullOrWhiteSpace(gitScript.GITProjectFolder))
            )
            {
                if (!isDEV) cbShemaObject.Items.Clear();

                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, gitScript.GITProjectFolder, gitScript.DataFolder);

                App.AddLog($"Заполняем из {folder} список объектов для скриптов по данным", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                foreach (var item in Utilities.Files.ListFilesInDir(folder, true, false, false))
                {
                    cbGITNameObject.Items.Add(item);
                }

                App.AddLog($"Заполнен список объектов для скриптов по данным", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
            }

            if (
                    isDEV &&
                    (!string.IsNullOrWhiteSpace(devScript.GITProject)) &&
                    (devScript.GITKindObject == "DATA") &&
                    (!string.IsNullOrWhiteSpace(devScript.GITProjectFolder))
            )
            {
                cbShemaObject.Items.Clear();

                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, devScript.GITProjectFolder, devScript.DataFolder);

                App.AddLog($"Заполняем из {folder} список объектов для скриптов по данным", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                foreach (var item in Utilities.Files.ListFilesInDir(folder, true, false, false))
                {
                    cbDEVNameObject.Items.Add(item);
                }

                App.AddLog($"Заполнен список объектов для скриптов по данным", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
            }

            // заполняем список схем для скриптов маркеров
            if (isGIT &&
                    (!string.IsNullOrWhiteSpace(gitScript.GITProject)) &&
                    (
                        (gitScript.GITTypeObject == "freedocmarker") ||
                        (gitScript.GITTypeObject == "freedocrelationship")
                    )
            )
            {
                if (!isDEV)
                {
                    cbShemaObject.Items.Clear();
                    cbShemaObject.Items.Add("dbo");
                }
            }

            if (
                    isDEV &&
                    (!string.IsNullOrWhiteSpace(devScript.GITProject)) &&
                    (
                        (devScript.GITTypeObject == "freedocmarker") ||
                        (devScript.GITTypeObject == "freedocrelationship")
                    )
            )
            {
                cbShemaObject.Items.Clear();
                cbShemaObject.Items.Add("dbo");
            }

            // заполняем список схем для всего кроме скриптов по данным и маркеров
            if (isGIT &&
                    (!string.IsNullOrWhiteSpace(gitScript.GITProject)) &&
                    (gitScript.GITKindObject != "DATA") &&
                    (gitScript.GITTypeObject != "freedocmarker") &&
                    (gitScript.GITTypeObject != "freedocrelationship") &&
                    (gitScript.GITTypeObject != "") &&
                    (!string.IsNullOrWhiteSpace(gitScript.GITProjectFolder))
            )
            {
                List<string> list = null;

                if (listSchema.Count > 0)
                {
                    // Список схем уже подготовлен
                    list = listSchema;
                }
                else
                {
                    // перечитаем схемы заново из проекта
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, gitScript.GITProjectFolder);
                    list = fillListSchema(null, folder);
                }

                // дополняем список схем
                foreach (var item in list)
                {
                    if (!cbShemaObject.Items.Contains(item))
                    {
                        cbShemaObject.Items.Add(item);
                    }
                }
            }

            if (
                   isDEV &&
                   (!string.IsNullOrWhiteSpace(devScript.GITProject)) &&
                   (devScript.GITKindObject != "DATA") &&
                   (devScript.GITTypeObject != "freedocmarker") &&
                   (devScript.GITTypeObject != "freedocrelationship") &&
                   (devScript.GITTypeObject != "") &&
                   (!string.IsNullOrWhiteSpace(devScript.GITProjectFolder))
            )
            {
                List<string> list = null;

                if (listSchema.Count > 0)
                {
                    // Список схем уже подготовлен
                    list = listSchema;
                }
                else
                {
                    // перечитаем схемы заново из проекта
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, devScript.GITProjectFolder);
                    list = fillListSchema(null, folder);
                }

                // дополняем список схем
                foreach (var item in list)
                {
                    if (!cbShemaObject.Items.Contains(item))
                    {
                        cbShemaObject.Items.Add(item);
                    }
                }
            }

            cbShemaObject_Sync(isGIT, isDEV);
            if (isGIT) cbGITNameObject_Sync();
            if (isDEV) cbDEVNameObject_Sync();
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// При выходе из поля Схема - проверки значения текущего поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbShemaObject_Leave(object sender, EventArgs e)
        {
            if (gitScript.GITShemaObject == cbShemaObject.Text.Trim())
            {
                return;
            }
            cbShemaObject_Sync(true, true);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Синхронизация с текущим значением поля cbShemaObject - действия с другими полями после изменения текущего
        /// </summary>
        private void cbShemaObject_Sync(bool isGIT, bool isDEV)
        {
            // проставляем схему
            if (isGIT) gitScript.GITShemaObject = cbShemaObject.Text.Trim();
            if (isDEV) devScript.GITShemaObject = cbShemaObject.Text.Trim();

            // очищаем остальные поля (кроме имени файла)
            cbGITNameObject.SelectedIndex = -1;
            cbGITNameObject.Text = "";

            cbDEVNameObject.SelectedIndex = -1;
            cbDEVNameObject.Text = "";

            // заполняем список объектов для скриптов по структуре, коду, маркерам
            if (isGIT &&
                    (gitScript.GITProject != "") &&
                    (
                        (gitScript.GITKindObject == "STRUCT") ||
                        (gitScript.GITKindObject == "CODE")
                    ) &&
                    (gitScript.GITShemaObject != "") 
            )
            {
                cbGITNameObject.Items.Clear();

                string typeFolder = Path.Combine(MainWindow.APPinfo.GITFolder, gitScript.GITProjectFolder, gitScript.GITShemaObject, gitScript.GITTypeObject);

                App.AddLog($"Заполняем из {typeFolder} список объектов для скриптов по структуре, коду, маркерам", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                if (gitScript.isSingleScript) // сохраняем в один файл
                {
                    // файлы
                    foreach (var item in Utilities.Files.ListFilesInDir(typeFolder, false, true, false))
                    {
                        string file = Path.GetFileNameWithoutExtension(item);
                        cbGITNameObject.Items.Add(file);
                    }
                }
                else // сохраняем в разные файлы
                {
                    // папки
                    foreach (var item in Utilities.Files.ListFilesInDir(typeFolder, true, false, false))
                    {
                        cbGITNameObject.Items.Add(item);
                    }
                }

                App.AddLog($"Заполнен список объектов для скриптов по структуре, коду, маркерам", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
            }

            // заполняем список объектов для скриптов по структуре, коду, маркерам
            if (
                    isDEV &&
                    (devScript.GITProject != "") &&
                    (
                        (devScript.GITKindObject == "STRUCT") ||
                        (devScript.GITKindObject == "CODE")
                    ) &&
                    (devScript.GITShemaObject != "")
            )
            {
                cbDEVNameObject.Items.Clear();

                string typeFolder = Path.Combine(MainWindow.APPinfo.GITFolder, devScript.GITProjectFolder, devScript.GITShemaObject, devScript.GITTypeObject);

                App.AddLog($"Заполняем из {typeFolder} список объектов для скриптов по структуре, коду, маркерам", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                if (devScript.isSingleScript) // сохраняем в один файл
                {
                    // файлы
                    foreach (var item in Utilities.Files.ListFilesInDir(typeFolder, false, true, false))
                    {
                        string file = Path.GetFileNameWithoutExtension(item);
                        cbDEVNameObject.Items.Add(file);
                    }
                }
                else // сохраняем в разные файлы
                {
                    // папки
                    foreach (var item in Utilities.Files.ListFilesInDir(typeFolder, true, false, false))
                    {
                        cbDEVNameObject.Items.Add(item);
                    }
                }

                App.AddLog($"Заполнен список объектов для скриптов по структуре, коду, маркерам", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
            }

            if (isGIT) cbGITNameObject_Sync();
            if (isDEV) cbDEVNameObject_Sync();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// При выходе из поля Имя объекта проекта DEV - проверки значения текущего поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbDEVNameObject_Leave(object sender, EventArgs e)
        {
            cbDEVNameObject.Text = CorrectNameObject(cbDEVNameObject, cbDEVNameObject.Text.ToLower());

            if (devScript.GITNameObject == cbDEVNameObject.Text.Trim())
            {
                return;
            }
            cbDEVNameObject_Sync();

            cbGITNameObject.Text = CorrectNameObject(cbGITNameObject, cbDEVNameObject.Text.ToLower());
            cbGITNameObject_Sync();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Синхронизация с текущим значением поля cbDEVNameObject - действия с другими полями после изменения текущего
        /// </summary>
        private void cbDEVNameObject_Sync()
        {
            devScript.GITNameObject = cbDEVNameObject.Text.Trim();

            if (isAddToDEV.Checked)
            {
                tbGITFilename.Text = GetGITFilenameFromForm(devScript, tbGITFilename.Text);
                tbGITFilename_Sync();
            }
        }



        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// При выходе из поля Имя объекта проекта GIT - проверки значения текущего поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbGITNameObject_Leave(object sender, EventArgs e)
        {
            cbGITNameObject.Text = CorrectNameObject(cbGITNameObject, cbGITNameObject.Text.ToLower());

            if (gitScript.GITNameObject == cbGITNameObject.Text.Trim())
            {
                return;
            }
            cbGITNameObject_Sync();

            cbDEVNameObject.Text = CorrectNameObject(cbDEVNameObject, cbGITNameObject.Text.ToLower());
            cbDEVNameObject_Sync();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Синхронизация с текущим значением поля cbGITNameObject - действия с другими полями после изменения текущего
        /// </summary>
        private void cbGITNameObject_Sync()
        {
            gitScript.GITNameObject = cbGITNameObject.Text.Trim();

            if (
                (!isAddToDEV.Checked) &&
                isAddToGIT.Checked
                )
            {
                tbGITFilename.Text = GetGITFilenameFromForm(gitScript, tbGITFilename.Text);
                tbGITFilename_Sync();
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// При выходе из поля Имя файла для GIT - проверки значения текущего поля
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbGITFilename_Leave(object sender, EventArgs e)
        {
            if (gitScript.GITFilename == tbGITFilename.Text.Trim())
            {
                return;
            }

            tbGITFilename_Sync();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Синхронизация с текущим значением поля tbGITFilename - действия с другими полями после изменения текущего
        /// </summary>
        private void tbGITFilename_Sync()
        {
            gitScript.GITFilename = tbGITFilename.Text.Trim();
            devScript.GITFilename = gitScript.GITFilename;
        }

        private void btCompare_Click(object sender, EventArgs e)
        {
            string errors = "";

            if (string.IsNullOrWhiteSpace(devScript.GITScriptname) || (!File.Exists(devScript.GITScriptname)))
            {
                errors = $"Файл {devScript.GITScriptname} не существует, необходимо выбрать существующий файл для сравнения с GIT!";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tbScriptFilename.Focus();
                return;
            }

            if (
                isAddToDEV.Checked &&
                string.IsNullOrWhiteSpace(devScript.GITProject)
                )
            {
                errors = $"Необходимо выбрать проект разработки";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                cbDEVProject.Focus();
                return;
            }

            string folder = Path.Combine(MainWindow.APPinfo.GITFolder, devScript.GITProjectFolder);

            if (!Directory.Exists(folder))
            {
                errors = "Проект " + gitScript.GITProject + " НЕ клонирован!";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tbScriptFilename.Focus();
                return;

            }

            // проверим его текущую ветку и предложим переключиться или создать ветку текущей задачи
            string err = "";
            string branch = GIT.GitCurrentBranch(devScript.GITProject, out err, MainWindow.Task.LogFile);
            tbBranch.Text = branch;

            if (string.IsNullOrWhiteSpace(branch))
            {
                App.AddLog("У проекта " + devScript.GITProject + " не определилась ветка!" + Environment.NewLine
                    + Environment.NewLine + err, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                return;
            }

            if (branch != MainWindow.Task.TaskNumber)
            {
                // ветка другая, надо сменить
                if (System.Windows.Forms.MessageBox.Show("Сейчас в проекте " + devScript.GITProject + " текущая ветка " + branch + Environment.NewLine +
                        Environment.NewLine +
                        "Сменить ветку в проекте " + devScript.GITProject + " на " + MainWindow.Task.TaskNumber + " ?",
                        "ВНИМАНИЕ",
                        System.Windows.Forms.MessageBoxButtons.YesNo
                    ) == System.Windows.Forms.DialogResult.Yes
                )
                {
                    App.AddLog($"В проекте {devScript.GITProject} текущая ветка {branch}, выбрана смена ветки на {MainWindow.Task.TaskNumber}", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

                    // есть новые проекты, надо создать/переключиться на ветку задачи
                    GIT.GitNewBranch(new string[] { devScript.GITProject }, MainWindow.Task.TaskNumber, "master", MainWindow.Task.LogFile);

                    // покажем текущую ветку
                    err = "";
                    tbBranch.Text = GIT.GitCurrentBranch(devScript.GITProject, out err, MainWindow.Task.LogFile);
                }
            }

            string path = devScript.GITFilepath;

            if (!Directory.Exists(path))
            {
                errors = $"Не найдена папка {path} для проекта {devScript.GITProject} !";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                tbScriptFilename.Focus();
                return;
            }

            string new_file = gitScript.GITScriptname;
            if (new_file.Contains(" ")) new_file = "\"" + new_file + "\"";
            string git_file = "";

            // ищем файл в GIT
            git_file = Path.Combine(path, devScript.GITFilename) + ".sql";

            if (string.IsNullOrWhiteSpace(git_file) || (!File.Exists(git_file)))
            {
                errors = $"Файл {git_file} не существует!";
                App.AddLog(errors, null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                return;
            }

            if (git_file.Contains(" ")) git_file = "\"" + git_file + "\"";

            // сравнение
            Utilities.External.ExecuteFile(
                App.AppPath,
                Path.Combine(App.AppPath, "git_compare.cmd"),
                git_file + " " + new_file,
                true,
                true,
                false,
                true,
                MainWindow.Task.LogFile
            );
        }
    }

    /// <summary>
    /// sql-файл для отправки в GIT
    /// </summary>
    internal class FileSQLInfo
    {
        /// <summary>
        /// имя файла
        /// </summary>
        public string Name;
        /// <summary>
        /// номер по порядку
        /// </summary>
        public string Order;
    }
}