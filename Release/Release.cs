// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SQLGen.Utilities;
using Path = System.IO.Path;

namespace SQLGen
{
    /// <summary>
    /// Типы merge задач
    /// </summary>
    public enum MergeType
    {
        /// <summary>
        /// Влить все задачи
        /// </summary>
        ALL,
        /// <summary>
        /// Влить только те, что еще не влиты
        /// </summary>
        CONTINUE,
        /// <summary>
        /// Влить одну задачу
        /// </summary>
        SINGLE
    }
    
    /// <summary>
    /// Вспомогательные функции для работы с релизами
    /// </summary>
    public static class Release
    {
        /// <summary>
        /// Список дублей
        /// </summary>
        /// <param name="ListYML"></param>
        /// <returns></returns>
        public static string ListDoubles(List<YMLLine> ListYML)
        {
            List<YMLLine> objects = new List<YMLLine>();
            List<YMLLine> datas = new List<YMLLine>();

            int cnt = 0;

            // перебираем yml-файлы
            foreach (YMLLine yml in ListYML.Where(x => x.type == YMLLineType.TASK).OrderBy(x => x.order))
            {
                // перебираем скрипты по структуре
                foreach (var item in yml.loadYMLStruct.Lines.Where(x => x.type == YMLLineType.SQLSTRUCT))
                {
                    // определяем тег объекта 
                    string _tag = item.ObjectName;

                    // ищем среди уже встретившихся объектов
                    var found = objects.Where(x =>
                        (x.Tag.ToLower() == _tag.ToLower()) ||
                        (x.Tag.ToLower() == _tag.ToLower() + "s") ||
                        (x.Tag.ToLower() == _tag.ToLower() + "l") ||
                        (x.Tag.ToLower() == _tag.ToLower() + "ss") ||
                        (x.Tag.ToLower() == _tag.ToLower() + "ll") ||
                        (x.Tag.ToLower() == _tag.ToLower() + "ls") ||
                        (x.Tag.ToLower() + "s" == _tag.ToLower()) ||
                        (x.Tag.ToLower() + "l" == _tag.ToLower()) ||
                        (x.Tag.ToLower() + "ss" == _tag.ToLower()) ||
                        (x.Tag.ToLower() + "ll" == _tag.ToLower()) ||
                        (x.Tag.ToLower() + "ls" == _tag.ToLower())
                        ).FirstOrDefault();

                    // фиксируем тег
                    if (found != null)
                    {
                        _tag = found.Tag;
                    }

                    // добавляем объект в список
                    found = item;
                    found.Tag = _tag;
                    cnt++;
                    found.order = cnt;
                    objects.Add(found);
                }

                // перебираем скрипты по данным
                foreach (var item in yml.loadYMLStruct.Lines.Where(x => x.type == YMLLineType.SQLDATA))
                {
                    // определяем тег объекта 
                    string _tag = item.ObjectName;

                    // ищем среди уже встретившихся объектов
                    var found = datas.Where(x =>
                        (x.Tag.ToLower() == _tag.ToLower()) ||
                        (x.Tag.ToLower() == _tag.ToLower() + "s") ||
                        (x.Tag.ToLower() == _tag.ToLower() + "l") ||
                        (x.Tag.ToLower() == _tag.ToLower() + "ss") ||
                        (x.Tag.ToLower() == _tag.ToLower() + "ll") ||
                        (x.Tag.ToLower() == _tag.ToLower() + "ls") ||
                        (x.Tag.ToLower() + "s" == _tag.ToLower()) ||
                        (x.Tag.ToLower() + "l" == _tag.ToLower()) ||
                        (x.Tag.ToLower() + "ss" == _tag.ToLower()) ||
                        (x.Tag.ToLower() + "ll" == _tag.ToLower()) ||
                        (x.Tag.ToLower() + "ls" == _tag.ToLower())
                        ).FirstOrDefault();

                    // фиксируем тег
                    if (found != null)
                    {
                        _tag = found.Tag;
                    }

                    // добавляем объект в список
                    found = item;
                    found.Tag = _tag;
                    cnt++;
                    found.order = cnt;
                    datas.Add(found);
                }
            }

            string result = "";

            string all_info = "";
            string tag_info = "";
            string last_tag = "";
            string last_info = "";
            List<string> object_list = new List<string>();
            cnt = 0;

            // выводим список дублей по структуре, в порядке тегов и N п/п
            foreach (var yml in objects.OrderBy(x => x.TagOrder))
            {
                if (yml.Tag != last_tag)
                {
                    if (!string.IsNullOrEmpty(last_tag))
                    {
                        if (cnt > 1)
                        {
                            // тег сменился - добавляем в общий список информацию по тегу
                            if (!string.IsNullOrWhiteSpace(all_info)) all_info += Environment.NewLine;

                            foreach (var obj in object_list)
                            {
                                all_info += obj + ", ";
                            }
                            all_info = all_info.TrimEnd(new char[] { ' ', ',' });
                            all_info += Environment.NewLine + Environment.NewLine;
                            all_info += tag_info;
                        }

                    }
                    last_tag = yml.Tag;
                    tag_info = "";
                    object_list = new List<string>();
                    cnt = 0;
                }

                if (last_info != yml.PrintDoubleInfo)
                {
                    if (!object_list.Contains(yml.ObjectName)) object_list.Add(yml.ObjectName);
                    tag_info += yml.PrintDoubleInfo + Environment.NewLine;
                    cnt++;
                    last_info = yml.PrintDoubleInfo;
                }
            }

            if (!string.IsNullOrWhiteSpace(all_info))
            {
                if (!string.IsNullOrWhiteSpace(result)) result += Environment.NewLine; //-V3022

                result +=
                "--------------------------------------------------------------------" + Environment.NewLine +
                $"Дубли по структуре" + Environment.NewLine +
                "--------------------------------------------------------------------" + Environment.NewLine +
                Environment.NewLine +
                all_info;
            }

            all_info = "";
            tag_info = "";
            last_tag = "";
            last_info = "";
            object_list = new List<string>();
            cnt = 0;

            // выводим список дублей по данным, в порядке тегов и N п/п
            foreach (var yml in datas.OrderBy(x => x.TagOrder).Distinct())
            {
                if (yml.Tag != last_tag)
                {
                    if (!string.IsNullOrEmpty(last_tag))
                    {
                        if (cnt > 1)
                        {
                            // тег сменился - добавляем в общий список информацию по тегу
                            if (!string.IsNullOrWhiteSpace(all_info)) all_info += Environment.NewLine;

                            foreach (var obj in object_list)
                            {
                                all_info += obj + ", ";
                            }
                            all_info = all_info.TrimEnd(new char[] { ' ', ',' });
                            all_info += Environment.NewLine + Environment.NewLine;
                            all_info += tag_info;
                        }
                    }
                    last_tag = yml.Tag;
                    tag_info = "";
                    object_list = new List<string>();
                    cnt = 0;
                }

                if (last_info != yml.PrintDoubleInfo)
                {
                    if (!object_list.Contains(yml.ObjectName)) object_list.Add(yml.ObjectName);
                    tag_info += yml.PrintDoubleInfo + Environment.NewLine;
                    cnt++;
                    last_info = yml.PrintDoubleInfo;
                }
            }

            if (!string.IsNullOrWhiteSpace(all_info))
            {
                if (!string.IsNullOrWhiteSpace(result)) result += Environment.NewLine;

                result +=
                "--------------------------------------------------------------------" + Environment.NewLine +
                $"Дубли по данным" + Environment.NewLine +
                "--------------------------------------------------------------------" + Environment.NewLine +
                Environment.NewLine +
                all_info;
            }

            return result;
        }

        /// <summary>
        /// Сделать резервную копию списка YML-файлов
        /// </summary>
        /// <param name="numVersion">номер версии</param>
        /// <param name="logFile">лог-файл</param>
        public static void BackupYMLFiles(string numVersion, string logFile)
        {
            // получаем уникальное имя файла
            string filename = Path.GetFileNameWithoutExtension(MainWindow.Task.TaskFile) + DateTime.Now.ToString("_yyyy_MM_dd_HH_mm_ss_fff") + ".backup";

            if (!string.IsNullOrWhiteSpace(filename)) //-V3022
            {
                // создаем папку для резервных копий
                Directory.CreateDirectory(Path.Combine(MainWindow.Task.TaskPath, "BACKUP"));

                filename = Path.Combine(MainWindow.Task.TaskPath, "BACKUP", filename);

                // создаем резервную копию
                try
                {
                    string jsonString = JsonSerializer.Serialize<List<YMLFileInfo>>(MainWindow.Task.ReleaseYMLFiles, new JsonSerializerOptions { IgnoreReadOnlyProperties = true, WriteIndented = true });

                    File.WriteAllText(filename, jsonString);

                    App.AddLog($"Создан {filename} с резервной копией YML\\JSON-файлов версии {numVersion}", null, App.ShowMessageMode.NONE, true, logFile);
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile);
                }
            }
        }

        /// <summary>
        /// Загрузить yml-файл filepath (если он существует), добавить в него задачи из tasklist. 
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <param name="filepath">полный путь к yml-файлу версии</param>
        /// <param name="tasklist">список задач</param>
        /// <param name="changeset_before">стартовый changeset</param>
        /// <param name="changeset_after">финальный changeset</param>
        /// <param name="isNew">=true - заново собрать yml (если AUTOGEN), =false - добавить новые задачи в конец существующего</param>
        /// <param name="logFile">лог-файл</param>
        public static YMLStruct AddTasksToYML(string project, string filepath, List<YMLFileInfo> tasklist, YMLChangeset changeset_before, YMLChangeset changeset_after, bool isNew, string logFile)
        {
            YMLStruct loadyml = new YMLStruct(null, logFile);
            YMLStruct newyml = new YMLStruct(null, logFile);
            string ymlfield = Utilities.GITProjects.GetYMLFieldByProject(project);
            string folder = Path.Combine(MainWindow.APPinfo.GITFolder, Utilities.GITProjects.GetFolderByProject(project));
            string path = Path.GetDirectoryName(filepath).Replace(folder + Path.DirectorySeparatorChar, "");

            newyml.Project = project;
            newyml.Filepath = path;
            newyml.Filename = Path.GetFileName(filepath);
            newyml.IsFileExist = false;

            // считываем существующий файл
            if (
                (!string.IsNullOrWhiteSpace(filepath)) &&
                File.Exists(filepath)
                )
            {
                loadyml.LoadYML(project, path, Path.GetFileName(filepath), false, null, true, false);

                newyml.IsAutogen = loadyml.IsAutogen;
                newyml.IsFileExist = loadyml.IsFileExist;
                newyml.changesetPreConditions = new List<YMLChangeset>();

                if (loadyml.changesetPreConditions != null)
                {
                    foreach (var item in loadyml.changesetPreConditions)
                    {
                        newyml.changesetPreConditions.Add(item.Copy());
                    }
                }

                if (newyml.IsAutogen && (!isNew))
                {
                    // если текущий файл AUTOGEN но было выбрано "Добавить в релиз" - снимаем флаг автогенности
                    newyml.IsAutogen = false;
                }

                if (loadyml.Lines.Where(x => (x.type == YMLLineType.TASK) || (x.type == YMLLineType.VERSION)).Count() == 0)
                {
                    // файл пустой, генерим новый
                    newyml.IsAutogen = true;
                }
            }
            else
            {
                // файла нет, генерим новый
                newyml.IsAutogen = true;
            }

            int cnt = 0;

            // добавляем ссылки на предыдущие версии, если они были загружены
            foreach (var item in loadyml.PrevVersions)
            {
                cnt++;

                newyml.Lines.Add(new YMLLine(newyml, logFile)
                {
                    order = cnt,
                    type = item.type,
                    text = item.text,
                    path = item.path,
                    file = item.file,
                    relativeToChangelogFile = item.relativeToChangelogFile,
                    isLoaded = true
                });
            }

            if (newyml.IsAutogen)
            {
                // загруженный файл сгенерен автоматически - берем за основу tasklist, добавляем в него loadyml

                // обновляем changeset'ы
                newyml.changesetBefore = changeset_before;
                newyml.changesetAfter = changeset_after;
                string project_dev = Utilities.GITProjects.GetDEVProject(project);

                if (tasklist != null)
                {
                    // перебираем tasklist
                    string prevFile = "";
                    foreach (var info in tasklist.Where(x => x.PathInGIT.ToLower() == "task"))
                    {
                        string file = info.GetYMLFile(ymlfield);

                        if (!string.IsNullOrWhiteSpace(file))
                        {
                            if (file.Contains(".yml"))
                            {
                                var arrf = file.Replace(".yml", "|").Split('|');
                                file = arrf[0] + ".yml";


                                if (prevFile != file)
                                {
                                    cnt++;

                                    newyml.Lines.Add(new YMLLine(newyml, logFile)
                                    {
                                        order = cnt,
                                        type = YMLLineType.TASK,
                                        text = "",
                                        path = "task",
                                        file = file,
                                        relativeToChangelogFile = "false",
                                        isLoaded = false,
                                    });
                                }

                                prevFile = file;
                            }
                        }
                    }
                }

                // перебираем loadyml.Lines
                bool isAdded = false;
                foreach (var item in loadyml.Lines
                    .Where(x =>
                        (x.type == YMLLineType.TASK) ||
                        (x.type == YMLLineType.COMMENT) ||
                        (x.type == YMLLineType.UNKNOWN)
                        )
                    )
                {
                    var found = newyml.Lines
                        .Where(x =>
                            (x.type == YMLLineType.TASK) &&
                            (x.path == item.path) &&
                            (x.file.ToLower() == item.file.ToLower())
                        ).FirstOrDefault();

                    if (found == null)
                    {
                        if (
                            Utilities.GITProjects.IsGITProject(project) &&
                            (!isAdded)
                        )
                        {
                            cnt++;

                            // добавляем комментарий, чтобы выделить добавленное
                            newyml.Lines.Add(new YMLLine(newyml, logFile)
                            {
                                order = cnt,
                                type = YMLLineType.COMMENT,
                                text = $"#added from {project_dev} and probably not exist in {project}",
                                isLoaded = false,
                            });
                            isAdded = true;
                        }

                        item.isLoaded = true;
                        cnt++;
                        item.order = cnt;
                        newyml.Lines.Add(item);
                    }
                }
            }
            else
            {
                // дополняем существующий файл - берем за основу loadyml, дополняем из tasklist

                // обновляем changeset'ы, если они НЕ были загружены
                newyml.changesetBefore = loadyml.changesetBefore;
                if (newyml.changesetBefore == null)
                {
                    newyml.changesetBefore = changeset_before;
                }

                newyml.changesetAfter = loadyml.changesetAfter;
                if (newyml.changesetAfter == null)
                {
                    newyml.changesetAfter = changeset_after;
                }

                // добавляем ссылки на задачи, если они были загружены
                foreach (var item in loadyml.Lines.Where(x =>
                    (x.type == YMLLineType.TASK) ||
                    (x.type == YMLLineType.COMMENT) ||
                    (x.type == YMLLineType.EMPTYLINE) ||
                    (x.type == YMLLineType.UNKNOWN)
                ).OrderBy(x => x.order))
                {
                    cnt++;

                    newyml.Lines.Add(new YMLLine(newyml, logFile)
                    {
                        order = cnt,
                        type = item.type,
                        text = item.text,
                        path = item.path,
                        file = item.file,
                        relativeToChangelogFile = item.relativeToChangelogFile,
                        isLoaded = true
                    });
                }

                // уточняем максимальный N п/п
                if (newyml.Lines.Count > 0)
                {
                    cnt = newyml.Lines.Max(x => x.order);
                }
                else
                {
                    cnt = 0;
                }

                // добавляем новые задачи
                if (tasklist != null)
                {
                    //убираем последнюю строку, если она была пустой
                    if (newyml.Lines.Count > 0)
                    {
                        if (newyml.Lines[newyml.Lines.Count - 1].type == YMLLineType.EMPTYLINE)
                        {
                            //если последняя строка пустая - удаляем ее
                            newyml.Lines.RemoveAt(newyml.Lines.Count - 1);
                        }
                    }

                    // перебираем tasklist
                    bool isAdded = false;
                    foreach (var info in tasklist.Where(x => x.PathInGIT.ToLower() == "task"))
                    {
                        string file = info.GetYMLFile(ymlfield);

                        if (!string.IsNullOrWhiteSpace(file))
                        {
                            if (file.Contains(".yml"))
                            {
                                var arrf = file.Replace(".yml", "|").Split('|');
                                file = arrf[0] + ".yml";


                                var found = newyml.Lines
                                    .Where(x =>
                                        (x.type == YMLLineType.TASK) &&
                                        (x.path == "task") &&
                                        (x.file.ToLower() == file.ToLower())
                                    ).FirstOrDefault();

                                if (found == null)
                                {
                                    if (!isAdded)
                                    {
                                        cnt++;

                                        // добавляем комментарий, чтобы выделить добавленное
                                        newyml.Lines.Add(new YMLLine(newyml, logFile)
                                        {
                                            order = cnt,
                                            type = YMLLineType.COMMENT,
                                            text = $"#{MainWindow.Task.TaskExecutor}: added " + DateTime.Now.ToString("dd-MM-yyyy"),
                                            isLoaded = false,
                                        });
                                        isAdded = true;
                                    }

                                    cnt++;

                                    newyml.Lines.Add(new YMLLine(newyml, logFile)
                                    {
                                        order = cnt,
                                        type = YMLLineType.TASK,
                                        text = "",
                                        path = "task",
                                        file = file,
                                        relativeToChangelogFile = loadyml.relativeToChangelogFile,
                                        isLoaded = false,
                                    });
                                }
                            }
                        }
                    }
                }
            }

            if (Utilities.GITProjects.IsGITProject(project))
            {
                // для всех yml в старых проектах сбрасываем флан автогенности (все последующие добавления пойдут в конец файла)
                newyml.IsAutogen = false;
            }

            return newyml;
        }

        /// <summary>
        /// =true - номер версии корректный
        /// </summary>
        /// <param name="name">номер версии</param>
        /// <returns></returns>
        public static bool IsNumVersionCorrect(string name) =>
            string.IsNullOrWhiteSpace(name) ||
            name.IsMatch(@"^[a-z0-9/.]+$");

        /// <summary>
        /// Исправление версии
        /// </summary>
        /// <param name="value">строка с номером версии</param>
        /// <returns></returns>
        public static string CorrectVersion(string value)
        {
            if (value.StartsWith("9.13.0-pg15"))
            {
                return "9.13.0.99";
            }
            if (value.Contains("-pg15"))
            {
                return value
                    .Replace(".1-pg15", ".101")
                    .Replace(".3-pg15", ".103")
                    .Replace(".4-pg15", ".104")
                    .Replace(".8-pg15", ".108")
                    .Replace(".11-pg15", ".111")
                    .Replace(".16-pg15", ".116")
                    .Trim();
            }
            return value.Trim();
        }

        /// <summary>
        /// Номер версии как число
        /// </summary>
        /// <param name="ver_num">строка с номером версии</param>
        /// <returns></returns>
        public static double VerAsNum(string ver_num)
        {
            double s = 0;

            ver_num = ver_num.Trim();

            if (!string.IsNullOrWhiteSpace(ver_num))
            {
                var arr = ver_num.Split(new char[] { '.', '-' });
                int step = -1;

                for (int i = 0; i < arr.Length; i++)
                {
                    double n = 0;
                    double.TryParse(arr[i], out n);
                    if (s == 0 && n > 0) //-V3024
                    {
                        //первая значимая цифра
                        step = 0;
                    }

                    if (step >= 0)
                    {
                        double t = 0;

                        if (step == 0) t = n * 1000000000000;
                        if (step == 1) t = n * 1000000000;
                        if (step == 2) t = n * 1000000;
                        if (step == 3) t = n * 1000;
                        if (step == 4) t = n * 1;

                        s = s + t;

                        step++;
                    }
                }
            }

            return s;
        }

        /// <summary>
        /// Вытащить номер версии из:
        /// 1) из имени файла версии - префикс.номер_*
        /// игнорируем _deployment и _cron в имени, игнорируем расширения .yml и .json
        /// 2) из ветки версии - префикс.номер
        /// 3) из changeset версии 
        /// Version_номер_begin или Version_номер_end
        /// Version_номер_*_begin или Version_номер_*_end
        /// Version_*_номер_begin или Version_*_номер_end
        /// перфикс.номер
        /// </summary>
        /// <param name="prefix">префикс</param>
        /// <param name="num">строка с номером версии или именем ветки</param>
        /// <returns></returns>
        public static string GetNumVersion(string prefix, string num)
        {
            if (!string.IsNullOrWhiteSpace(num))
            {
                num = num.Trim().ToLower();
                if (num.EndsWith(".yml"))
                {
                    num = num.Substring(0, num.Length - 4);
                }
                if (num.EndsWith(".json"))
                {
                    num = num.Substring(0, num.Length - 5);
                }
                prefix = prefix.ToLower().Trim();

                // убираем ключевые слова в changeset или в имени файла
                num = num
                    .Replace("version_", "")
                    .Replace("_begin", "")
                    .Replace("_end", "")
                    .Replace("_deployment", "")
                    .Replace("_cron", "")
                    .Trim();

                // убираем префикс
                if (num.StartsWith(prefix + "."))
                {
                    num = num.Substring(prefix.Length + 1).Trim();
                }
                else if (num.StartsWith("rpms."))
                {
                    num = num.Substring(5).Trim();
                }
                else if (num.StartsWith("bi."))
                {
                    num = num.Substring(3).Trim();
                }
                else if (num.StartsWith("smp."))
                {
                    num = num.Substring(4).Trim();
                }
                else if (num.StartsWith("prmd."))
                {
                    num = num.Substring(5).Trim();
                }

                // выделяем номер, содержащий точку 
                var arr = num.Split('_');
                if (arr[0].Contains("."))
                {
                    num = arr[0].Trim();
                }
                else
                {
                    if ((arr.Length > 1) && arr[1].Contains("."))
                    {
                        num = arr[1].Trim();
                    }
                }

                // подмена номеров версий
                if (num.Contains("-pg15"))
                {
                    num = CorrectVersion(num);
                }

                return num.Trim();
            }

            return "";
        }

        /// <summary>
        /// Найти json-файл с Действиями при обновлении версии
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <param name="version">версия</param>
        /// <param name="type">тип файла: deployment, cron</param>
        /// <param name="isExist">=true - файл существует</param>
        /// <returns></returns>
        public static string GetJsonFile(string project, string version, string type, out bool isExist)
        {
            isExist = false;
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "version");
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            string postfix = Utilities.GITProjects.GetPostfixFileReleaseByProject(project);

            // текущая версия
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, version));
            string currentbranch = GIT.GitCurrentBranch(project, out string err, MainWindow.Task.LogFileRelease);

            // перебираем существующие в текущей ветке файлы
            foreach (var file in Utilities.Files.ListFilesInDir(path, false, true, false))
            {
                string item = Path.GetFileName(file);

                if (item.ToLower().EndsWith("_" + type + ".json"))
                {
                    // определяем номер версии из имени файла
                    string ver = Release.GetNumVersion(prefix, item);
                    double nn = Release.VerAsNum(ver);
                    if (nn == numversion) //-V3024
                    {
                        isExist = true;
                        return item;
                    }
                }
            }

            return prefix + "." + Release.GetNumVersion(prefix, version) + "_" + DateTime.Now.ToString("ddMM") + "_" + MainWindow.Task.TaskNumber + postfix + "_" + type + ".json";
        }

    }
}
