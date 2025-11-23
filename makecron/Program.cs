// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace makecron
{
    /// <summary>
    /// Коды возврата из приложения
    /// </summary>
    public enum ExitCode
    {
        /// <summary>
        /// Успешно
        /// </summary>
        OK = 0,
        /// <summary>
        /// Неверные параметры
        /// </summary>
        BAD_ARGS = 1,
        /// <summary>
        /// Ошибка загрузки
        /// </summary>
        LOAD_ERROR = 2,
        /// <summary>
        /// Ошибка записи
        /// </summary>
        SAVE_ERROR = 3
    }

    internal class Program
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Каталог, откуда берем json-файлы отдельных заданий</summary>
        public static string SourceFolder;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Полный путь к общему json-файлу</summary>
        public static string TargetFile;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Главная функция приложения
        /// </summary>
        /// <param name="args">параметры командной строки</param>
        static void Main(string[] args)
        {
            // -------------------------------------------------------------------------------------------------------
            // Определим стартовые параметры приложения

            Console.OutputEncoding = Encoding.UTF8;

            if (
                args == null ||
                args.Length < 2
            )
            {
                AddLog("Утилита для объединения json-файлов отдельных заданий в общий json-файл:");
                AddLog("makecron.exe source target");
                AddLog("source - каталог, откуда берем json-файлы отдельных заданий");
                AddLog("target - полный путь к общему json-файлу");

                Environment.Exit((int)ExitCode.BAD_ARGS);
            }

            // -------------------------------------------------------------------------------------------------------
            // Загружаем задания

            SourceFolder = args[0]
                .Replace('/', Path.DirectorySeparatorChar)
                .TrimEnd(new char[] { Path.DirectorySeparatorChar });

            if (!Path.Exists(SourceFolder))
            {
                AddLog($"{SourceFolder} не существует");

                Environment.Exit((int)ExitCode.BAD_ARGS);
            }

            var list_cron = ListAllCron(SourceFolder);

            if (
                list_cron != null &&
                list_cron.Count > 0
            )
            {
                AddLog($"Файлы из {SourceFolder} загружены успешно");
            }
            else
            {
                AddLog($"Не найдены json-файлы в {SourceFolder}");

                Environment.Exit((int)ExitCode.LOAD_ERROR);
            }

            // -------------------------------------------------------------------------------------------------------
            // Сохраняем задания

            TargetFile = args[1]
                .Replace('/', Path.DirectorySeparatorChar);

            string _ext = Path.GetExtension(TargetFile);

            if (_ext != ".json")
            {
                AddLog($"{TargetFile} должен быть файлом с расширениме json");

                Environment.Exit((int)ExitCode.BAD_ARGS);
            }

            string _folder = Path.GetDirectoryName(TargetFile);

            if (!Path.Exists(_folder))
            {
                AddLog($"{_folder} не существует");

                Environment.Exit((int)ExitCode.BAD_ARGS);
            }

            SaveJSON(list_cron, TargetFile);

            AddLog($"Файл {TargetFile} записан успешно");

            //Environment.Exit((int)ExitCode.OK);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// сообщение из Exception с подробностями
        /// </summary>
        /// <param name="ex">Exception</param>
        /// <returns></returns>
        public static string GetFullExceptionMessage(Exception ex)
        {
            string result = "";

            if (ex == null) return result;

            string error = (ex.Message ?? "").Trim();
            string error_trace = "";

            try
            {
                error_trace =
                "\n====== Exception ============" + //-V3022
                "\n------ Source ---------------" +
                "\n" + (ex.Source ?? "") +
                "\n------ StackTrace -----------" +
                "\n" + (ex.StackTrace ?? "");

                if (ex.TargetSite != null)
                {
                    error_trace +=
                    "\n------ TargetSite -----------" +
                    "\n" + ex.TargetSite +
                    "\n=============================";
                }
            }
            catch
            {
            }

            string error1 = "";
            string error1_trace = "";
            var ex1 = ex.InnerException;

            try
            {
                if (
                    (ex1 != null) &&
                    (!string.IsNullOrWhiteSpace(ex1.Message))
                )
                {
                    error1 = ex1.Message.Trim();

                    error1_trace =
                        "\n====== Inner Exception ======" +
                        "\n------ Source ---------------" +
                        "\n" + (ex1.Source ?? "") +
                        "\n------ StackTrace -----------" +
                        "\n" + (ex1.StackTrace ?? "");

                    if (ex1.TargetSite != null)
                    {
                        error1_trace +=
                        "\n------ TargetSite -----------" +
                        "\n" + ex1.TargetSite +
                        "\n=============================";
                    }
                }
            }
            catch
            {
            }

            string error2 = "";
            string error2_trace = "";
            var ex2 = ex.GetBaseException();

            try
            {
                if (
                   (ex2 != null) &&
                    (!string.IsNullOrWhiteSpace(ex2.Message))
                )
                {
                    error2 = ex2.Message.Trim();

                    error2_trace =
                        "\n====== Base Exception =======" +
                        "\n------ Source ---------------" +
                        "\n" + (ex2.Source ?? "") +
                        "\n------ StackTrace -----------" +
                        "\n" + (ex2.StackTrace ?? "");

                    if (ex2.TargetSite != null)
                    {
                        error2_trace +=
                        "\n------ TargetSite -----------" +
                        "\n" + ex2.TargetSite +
                        "\n=============================";
                    }
                }
            }
            catch
            {
            }

            result = error;

            if (error != error1)
            {
                result += "\n" + error1;
            }

            if ((error != error2) && (error1 != error2))
            {
                result += "\n" + error2;
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Запись в лог-файл и вывод информационного сообщения</summary>
        /// <param name="info">Строка для добавления в лог-файл</param>
        /// <param name="ex">Exception с подробностями ошибки</param>
        /// <returns>текст сообщения для вывода на экран</returns>
        public static string AddLog(string info, Exception ex = null)
        {
            if (string.IsNullOrWhiteSpace(info))
            {
                info = "";
            }

            var result = GetFullExceptionMessage(ex);

            if (!string.IsNullOrWhiteSpace(info))
            {
                info += "\n";
            }

            if (!string.IsNullOrWhiteSpace(result))
            {
                result += "\n";
            }

            result = info + result;

            if (!string.IsNullOrWhiteSpace(result))
            {
                Console.Write(result);
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Получаем список каталогов по маске
        /// </summary>
        /// <param name="pathmask">маска каталога</param>
        /// <returns>список существующих каталогов</returns>
        public static List<string> ListDirectoryByMask(string pathmask)
        {
            // Собираем список каталогов
            List<string> dir = new List<string>();

            if (string.IsNullOrWhiteSpace(pathmask))
            {
                pathmask = "";
            }

            var list = pathmask.ToList(new char[] { Path.DirectorySeparatorChar, '/' }, true);

            for (int i = 0; i < list.Count; i++)
            {
                string s = list[i].Trim();

                if (
                    (i == 0) &&
                    (
                        s.Contains("*") ||
                        s.Contains("?") ||
                        (!s.Contains(":"))
                    )
                  )
                {
                    // стартовый каталог должен начинаться с диска
                    return dir;
                }

                List<string> newdir = new List<string>();

                if (
                    s.Contains("*") ||
                    s.Contains("?")
                   )
                {
                    foreach (var item in dir)
                    {
                        DirectoryInfo directory = new DirectoryInfo(item);
                        var dirList = directory.GetDirectories(s).Where(x => (x.Attributes & FileAttributes.Hidden) == 0).Select(x => x.FullName).ToList();
                        newdir.AddRange(dirList);
                    }
                }
                else
                {
                    if (dir.Count == 0)
                    {
                        if (!s.EndsWith("" + Path.DirectorySeparatorChar))
                        {
                            s += Path.DirectorySeparatorChar;
                        }
                        newdir.Add(s);
                    }
                    else
                    {
                        foreach (var item in dir)
                        {
                            string dd = Path.Combine(item, s);
                            if (Directory.Exists(dd))
                            {
                                newdir.Add(dd);
                            }
                        }
                    }
                }

                dir = newdir;
            }

            return dir;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Заполнить иерархический список файлов и подкаталогов в каталоге</summary>
        /// <param name="pathmask">Стартовый каталог (может быть маской, но обязательно должен начинаться с диска, например C:\)</param>
        /// <param name="isAddDir">true - добавлять подкаталоги</param>
        /// <param name="isAddFile">true - добавлять файлы</param>
        /// <param name="isRecursive">true - добавлять элементы в подкаталогах</param>
        /// <param name="pathexclude">Список исключаемых каталогов (может быть маской, но обязательно должен начинаться с диска, например C:\)</param>
        /// <param name="isFullPath">true - возвращает полный путь до файлов, по умолчанию false</param>
        /// <param name="filemask">Маска для файлов, по умолчанию *.*</param>
        /// <returns>
        /// возвращает список файлов: 
        /// если pathmask является маской или isFullPath=true, то функция всегда возвращает полные пути до файлов; 
        /// в противном случае функция возвращает относительные пути до файлов внутри pathmask
        /// </returns>
        public static List<string> ListFilesInDir(string pathmask, bool isAddDir, bool isAddFile, bool isRecursive, string pathexclude = "", bool isFullPath = false, string filemask = "*.*")
        {
            List<string> res = new List<string>();

            if (string.IsNullOrWhiteSpace(filemask)) filemask = "*.*";
            if (string.IsNullOrWhiteSpace(pathexclude)) pathexclude = "";

            pathmask = pathmask.Replace('/', Path.DirectorySeparatorChar);
            pathexclude = pathexclude.Replace('/', Path.DirectorySeparatorChar);


            var listmask = filemask.ToList(new char[] { ',', ';' }, true);
            List<string> listexclude = new List<string>();
            if (!string.IsNullOrWhiteSpace(pathexclude))
            {
                listexclude = pathexclude.ToList(new char[] { ',', ';' }, true);
            }

            if (
                pathmask.Contains("*") ||
                pathmask.Contains("?")
                )
            {
                // Стартовый каталог - это маска
                isFullPath = true;

                // Собираем список файлов в каталогах
                foreach (var item in ListDirectoryByMask(pathmask))
                {
                    res.AddRange(ProcessDirectory(item, "", isAddDir, isAddFile, isRecursive, listmask));
                }

                // исключаем часть файлов
                if (listexclude.Count > 0)
                {
                    foreach (var exclude in listexclude)
                    {
                        // список каталогов исключения
                        var list = ListDirectoryByMask(exclude);

                        // перебираем каталоги исключения
                        for (int i = 0; i < list.Count; i++)
                        {
                            string excl_dir = list[i].ToLower().Trim();
                            if (!excl_dir.EndsWith("" + Path.DirectorySeparatorChar))
                            {
                                excl_dir += Path.DirectorySeparatorChar;
                            }
                            // исключаем файлы
                            res = res.Where(x => !x.ToLower().StartsWith(excl_dir)).ToList();
                        }
                    }
                }
            }
            else
            {
                // Стартовый каталог - это абсолютный путь
                if (Directory.Exists(pathmask))
                {
                    string rootpath = "";
                    if (!isFullPath) rootpath = pathmask;
                    res.AddRange(ProcessDirectory(pathmask, rootpath, isAddDir, isAddFile, isRecursive, listmask));
                }
            }

            return res;
        }

        // -------------------------------------------------------------------------------------------------------
        private static List<string> ProcessDirectory(string path, string rootpath, bool AddDir, bool AddFile, bool IsRecursive, List<string> listmask)
        {
            List<string> res = new List<string>();

            bool isFullName = true;
            if (!string.IsNullOrWhiteSpace(rootpath)) isFullName = false;
            else rootpath = "";

            if (
                (!isFullName) &&
                (!rootpath.EndsWith("" + Path.DirectorySeparatorChar))
            )
            {
                rootpath += Path.DirectorySeparatorChar;
            }

            DirectoryInfo directory = new DirectoryInfo(path);

            if (AddFile == true)
            {
                foreach (var mask in listmask)
                {
                    // Process the list of files found in the directory.
                    var fileEntries = directory.GetFiles(mask).Where(x => (x.Attributes & FileAttributes.Hidden) == 0).Select(x => x.FullName).ToList();
                    foreach (string fileName in fileEntries)
                    {
                        if (isFullName)
                            res.Add(fileName);
                        else
                            res.Add(fileName.Replace(rootpath, string.Empty));
                    }
                }
            }

            // Recurse into subdirectories of this directory.
            var subdirectoryEntries = directory.GetDirectories().Where(x => (x.Attributes & FileAttributes.Hidden) == 0).Select(x => x.FullName).ToList();
            foreach (string subdirectory in subdirectoryEntries)
            {
                if (AddDir == true)
                {
                    if (isFullName)
                        res.Add(subdirectory);
                    else
                        res.Add(subdirectory.Replace(rootpath, string.Empty));
                }
                if (IsRecursive == true)
                {
                    res.AddRange(ProcessDirectory(subdirectory, rootpath, AddDir, AddFile, IsRecursive, listmask));
                }
            }

            return res;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// разобрать json-текст в CronToJsonForConf
        /// </summary>
        /// <param name="_jsontext">json-текст</param>
        /// <returns></returns>
        public static List<CronToJsonForConf> DeserializeJSON(string _jsontext)
        {
            List<CronToJsonForConf> result = null;

            if (!string.IsNullOrWhiteSpace(_jsontext))
            {
                bool isMulti = _jsontext
                    .TrimStart(new char[] { '\n', '\r', ' ', '\t' })
                    .StartsWith("[");

                if (isMulti)
                {
                    // если несколько заданий
                    result = JsonSerializer.Deserialize<List<CronToJsonForConf>>(_jsontext, new JsonSerializerOptions
                    {
                        IgnoreReadOnlyProperties = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });
                }
                else
                {
                    // если одно задание
                    result = new List<CronToJsonForConf>();
                    result.Add(JsonSerializer.Deserialize<CronToJsonForConf>(_jsontext, new JsonSerializerOptions
                    {
                        IgnoreReadOnlyProperties = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        WriteIndented = true,
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    }));
                }
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// по возможности исправляем содержимого загруженного json-файла
        /// </summary>
        /// <param name="cron_list">список заданий</param>
        /// <returns></returns>
        public static void CorrectJSON(List<CronToJsonForConf> cron_list)
        {
            if (cron_list == null)
            {
                return;
            }

            // перебираем содержимое загруженного файла
            foreach (CronToJsonForConf _json in cron_list.OrderBy(x => x.order))
            {
                // application_name
                if (string.IsNullOrWhiteSpace(_json.application_name))
                {
                    _json.application_name = null;
                }
                else
                {
                    _json.application_name = _json.application_name.Trim();
                }

                // order
                if (_json.order == 0)
                {
                    _json.order = 1;
                }

                // task
                if (string.IsNullOrWhiteSpace(_json.task))
                {
                    _json.task = null;
                }
                else
                {
                    _json.task = _json.task.Trim();
                }

                // comment
                if (string.IsNullOrWhiteSpace(_json.comment))
                {
                    _json.comment = null;
                }
                else
                {
                    _json.comment = _json.comment.Trim();
                }

                // command
                if (string.IsNullOrWhiteSpace(_json.command))
                {
                    _json.command = null;
                }
                else
                {
                    _json.command = _json.command.Trim();
                }

                // schedule
                if (string.IsNullOrWhiteSpace(_json.schedule))
                {
                    _json.schedule = null;
                }
                else
                {
                    _json.schedule = _json.schedule.Trim();
                }

                // state
                if (string.IsNullOrWhiteSpace(_json.state))
                {
                    _json.state = null;
                }

                if (_json.state != null)
                {
                    _json.state = _json.state.Trim().ToLower();
                }

                // database
                if (string.IsNullOrWhiteSpace(_json.database))
                {
                    _json.database = null;
                }

                if (_json.database != null)
                {
                    _json.database = _json.database.Trim().ToLower();
                }

                // stage
                if (string.IsNullOrWhiteSpace(_json.stage)) _json.stage = "all";

                _json.stage = _json.stage.Trim().ToLower();

                // regions
                if (_json.regions == null) _json.regions = new List<string>();
                if (_json.regions.Count == 0) _json.regions.Add("all");

                // exclude_regions
                if (_json.exclude_regions != null)
                {
                }

                // hosts
                if (string.IsNullOrWhiteSpace(_json.hosts)) _json.hosts = "single";

                _json.hosts = _json.hosts.Trim().ToLower();

                // check
                if (string.IsNullOrWhiteSpace(_json.check))
                {
                    _json.check = null;
                }
                else
                {
                    _json.check = _json.check.Trim();
                }

                // team
                if (string.IsNullOrWhiteSpace(_json.team))
                {
                    _json.team = null;
                }
                else
                {
                    _json.team = _json.team.Trim();
                }

                // istemp
                if (_json.istemp != 2)
                {
                    _json.istemp = 1;
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Загрузить json-файл со списком заданий
        /// </summary>
        /// <param name="json_filepath">загружаем задания из json-файла</param>
        /// <returns></returns>
        public static List<CronToJsonForConf> LoadJSON(string json_filepath)
        {
            // список заданий
            List<CronToJsonForConf> json_list = null;

            // загружаем json-файл
            if (
                !string.IsNullOrWhiteSpace(json_filepath) &&
                File.Exists(json_filepath)
            )
            {
                // загружаем файл
                try
                {
                    string jsonString = File.ReadAllText(json_filepath);

                    if (string.IsNullOrWhiteSpace(jsonString))
                    {
                        json_list = new List<CronToJsonForConf>();
                    }
                    else
                    {
                        json_list = DeserializeJSON(jsonString);

                        // исправляем содержимое загруженного файла
                        CorrectJSON(json_list);
                    }
                }
                catch (Exception ex)
                {
                    AddLog($"Ошибка загрузки файла {json_filepath} :", ex);

                    Environment.Exit((int)ExitCode.LOAD_ERROR);
                }
            }
            else
            {
                Environment.Exit((int)ExitCode.LOAD_ERROR);
            }

            return json_list;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Список заданий
        /// </summary>
        /// <returns></returns>
        public static List<CronToJsonForConf> ListAllCron(string folder)
        {
            // общий список заданий
            var ListCron = new List<CronToJsonForConf>();

            if (
                string.IsNullOrWhiteSpace(folder) ||
                !Path.Exists(folder)
            )
            {
                return ListCron;
            }

            // 1 - сначала постоянные
            // 2 - затем временные
            for (int _istemp = 1; _istemp <= 2; _istemp++)
            {
                // перебираем базы
                foreach (var db in ListFilesInDir(folder, true, false, false))
                {
                    int order = 0;

                    // перебираем задания
                    foreach (var file in ListFilesInDir(Path.Combine(folder, db), false, true, false, "", true, "*.json"))
                    {
                        // загружаем задания
                        var jsonlist_cron = LoadJSON(file);

                        foreach (var item in jsonlist_cron.Where(x => x.istemp == _istemp))
                        {
                            // перенумеровываем
                            order++;
                            item.order = order;

                            // добавляем
                            ListCron.Add(item);
                        }
                    }
                }
            }

            return ListCron;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Сгенерировать текст json-файла заданий
        /// </summary>
        /// <param name="cron_list">Список заданий</param>
        /// <returns></returns>
        public static string GenerateJSON(List<CronToJsonForConf> cron_list)
        {
            string result = "";

            if (
                cron_list != null &&
                cron_list.Count > 0
            )
            {
                var json = new CronVersionForConf();
                json.version = "Список заданий";
                json.listcron = cron_list.Where(x => x.istemp == 1).ToList();
                json.listtemp = cron_list.Where(x => x.istemp == 2).ToList();

                try
                {
                    result = JsonSerializer.Serialize<CronVersionForConf>(json,
                        new JsonSerializerOptions
                        {
                            IgnoreReadOnlyProperties = true,
                            WriteIndented = true,
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        });
                }
                catch (Exception ex)
                {
                    AddLog(null, ex);

                    Environment.Exit((int)ExitCode.SAVE_ERROR);
                }
            }

            if (string.IsNullOrWhiteSpace(result))
            {
                result = "";
            }

            result = result
                .Replace(Environment.NewLine, "\n")
                .TrimEndNewLine("\n");

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Сохранить задания в общий файл
        /// </summary>
        /// <param name="ListCron">список заданий</param>
        /// <returns></returns>
        public static void SaveJSON(List<CronToJsonForConf> ListCron, string Filename)
        {
            if (
                ListCron == null ||
                ListCron.Count == 0
            )
            {
                AddLog($"Попытка сохранить пустой файл {Filename}");

                Environment.Exit((int)ExitCode.SAVE_ERROR);
            }

            // сгенерить json со всеми заданиями
            string jsonText = GenerateJSON(ListCron);

            if (string.IsNullOrWhiteSpace(jsonText))
            {
                AddLog($"Попытка сохранить пустой файл {Filename}");

                Environment.Exit((int)ExitCode.SAVE_ERROR);
            }

            // сохранить json-файл с новым содержимым
            try
            {
                //сохранить файл
                WriteScript(Filename, jsonText);
            }
            catch (Exception ex)
            {
                AddLog(null, ex);

                Environment.Exit((int)ExitCode.SAVE_ERROR);
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Запись скрипта с предварительной его подготовкой
        /// </summary>
        /// <param name="fullpath">полное имя файла</param>
        /// <param name="text">текст скрипта</param>
        /// <param name="isRemoveCR">убрать символ возврата каретки \r</param>
        /// <param name="isTrimOuterNewLine">убрать переводы строк в начале и в конце, добавить в конце 1 перевод строки \n</param>
        public static void WriteScript(string fullpath, string text, bool isRemoveCR = true, bool isTrimOuterNewLine = true)
        {
            if (string.IsNullOrWhiteSpace(fullpath))
            {
                AddLog("Попытка записи в файл без имени");

                Environment.Exit((int)ExitCode.SAVE_ERROR);
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                text = "";
            }

            // убираем по тексту возврат каретки \r
            if (isRemoveCR)
            {
                text = text.ScriptPartReady();
            }

            // убираем переводы строк в начале и в конце, добавляем в конце 1 перевод строки \n
            if (isTrimOuterNewLine)
            {
                text = text.TrimNewLine("\n");
            }

            // запись
            Encoding encoding = new UTF8Encoding(false);
            File.WriteAllText(fullpath, text, encoding);
        }
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Класс для сохранения задания в json-файл со списком всех заданий (в ветке dev или в ветке версии) и для отображения в Confluence
    /// </summary>
    public class CronToJsonForConf
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public CronToJsonForConf()
        {
            this.istemp = 1;
            this.order = 0;
            this.regions = new List<string>();
            this.exclude_regions = new List<string>();
        }

        /// <summary>
        /// флаг временного задания
        /// </summary>
        public int istemp { get; set; }

        /// <summary>
        /// Целевая база данных
        /// </summary>
        public string database { get; set; }

        /// <summary>
        /// СУБД
        /// </summary>
        public string dbms { get; set; }

        /// <summary>
        /// Номер по порядку
        /// </summary>
        public int order { get; set; }

        /// <summary>
        /// наименование задания
        /// </summary>
        public string application_name { get; set; }

        /// <summary>
        /// описание задания
        /// </summary>
        public string comment { get; set; }

        /// <summary>
        /// признак актуальности задания
        /// </summary>
        public string state { get; set; }

        /// <summary>
        /// команда задания
        /// </summary>
        public string command { get; set; }

        /// <summary>
        /// расписание задания
        /// </summary>
        public string schedule { get; set; }

        /// <summary>
        /// Целевые регионы
        /// </summary>
        public List<string> regions { get; set; }

        /// <summary>
        /// Кроме указанных регионов
        /// </summary>
        public List<string> exclude_regions { get; set; }

        /// <summary>
        /// Тип целевой БД
        /// </summary>
        public string stage { get; set; }

        /// <summary>
        /// Ограничение времени выполнения скрипта или команды
        /// </summary>
        public int? timeout { get; set; }

        /// <summary>
        /// возможность параллельного запуска задачи
        /// </summary>
        public string hosts { get; set; }

        /// <summary>
        /// проверочный запрос
        /// </summary>
        public string check { get; set; }

        /// <summary>
        /// команда РТМИС, ответственная за задание
        /// </summary>
        public string team { get; set; }

        /// <summary>
        /// Задача
        /// </summary>
        public string task { get; set; }
    }

    /// <summary>
    /// Файл версии Cron для сохранения и отображения в Confluence
    /// </summary>
    public class CronVersionForConf
    {
        public CronVersionForConf()
        {
            this.listcron = new List<CronToJsonForConf>();
            this.listtemp = new List<CronToJsonForConf>();
        }

        /// <summary>
        /// номер версии
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// список постоянных заданий
        /// </summary>
        public List<CronToJsonForConf> listcron { get; set; }

        /// <summary>
        /// список временных заданий
        /// </summary>
        public List<CronToJsonForConf> listtemp { get; set; }
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// расширение для String
    /// </summary>
    public static class StringExtensions
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Преобразовать текст text в массив строк
        /// </summary>
        /// <param name="text">текст</param>
        /// <param name="divider">разделители</param>
        /// <param name="isTrim">убрать пустые строки и пробелы в начале и в конце каждой строки</param>
        /// <returns></returns>
        public static List<string> ToList(this string text, char[] divider, bool isTrim)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(text)) text = "";
            if (divider == null || divider.Count() == 0)
            {
                result.Add(text);
                return result;
            }

            var arr1 = text.Split(divider);

            foreach (var item in arr1)
            {
                var line = item;
                if (line == null) line = "";
                if (isTrim) line = line.TrimAllSpace();

                if (
                    (!string.IsNullOrWhiteSpace(line)) ||
                    (!isTrim)
                )
                {
                    result.Add(line);
                }
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// удаляем [[:space:]] - пробелы, табуляции, переводы строк - в начале и в конце text и добавляем postfix в конце
        /// </summary>
        /// <param name="text">текст</param>
        /// <param name="postfix">добавить в конец текста</param>
        /// <returns></returns>
        public static string TrimAllSpace(this string text, string postfix = "")
        {
            if (text == null) return null;
            else if (string.IsNullOrWhiteSpace(text)) return postfix;
            else return text.Trim(new char[] { '\n', '\r', '\t', '\v', '\f', ' ' }) + postfix;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// убираем все переводы строки и возвраты коретки в начале и в конце text и добавляем postfix в конце
        /// </summary>
        /// <param name="text">текст</param>
        /// <param name="postfix">добавить в конец текста</param>
        /// <returns></returns>
        public static string TrimNewLine(this string text, string postfix = "")
        {
            if (string.IsNullOrEmpty(text)) return text + postfix;
            else return text.Trim(new char[] { '\n', '\r' }) + postfix;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// убираем все переводы строки и возвраты коретки в конце text и добавляем postfix
        /// </summary>
        /// <param name="text">текст</param>
        /// <param name="postfix">добавить в конец текста</param>
        /// <returns></returns>
        public static string TrimEndNewLine(this string text, string postfix = "")
        {
            if (string.IsNullOrEmpty(text)) return text + postfix;
            else return text.TrimEnd(new char[] { '\n', '\r' }) + postfix;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// подготовка промежуточной части скрипта для записи в файл
        /// </summary>
        /// <param name="text">текст скрипта</param>
        /// <returns></returns>
        public static string ScriptPartReady(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            else return text.Replace("\r", "");
        }
    }
}
