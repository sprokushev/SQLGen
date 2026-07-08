// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Web.UI.WebControls;
using Microsoft.SqlServer.Server;
using Microsoft.VisualStudio.VCProjectEngine;
using SQLGen.Utilities;
using Path = System.IO.Path;

namespace SQLGen
{

    // -------------------------------------------------------------------------------------------------------
    /// <summary>Тип копирования или проверки</summary>
    public enum CopyType 
    {
        /// <summary>
        /// проверяем содержимое sql и yml
        /// </summary>
        CHECK,
        /// <summary>
        /// совмещает CHECK и COPY
        /// </summary>
        CHECKCOPY,
        /// <summary>
        /// копируем yml и sql
        /// </summary>
        COPY
    }

    /// <summary>
    /// Описание структуры yml-файла
    /// </summary>
    public class YMLStruct 
    {
        /// <summary>
        /// полный путь к лог-файлу
        /// </summary>
        public string logFile = "";

        /// <summary>
        /// Конструктор класса YMLStruct
        /// </summary>
        /// <param name="parent">ссылка на строку родительского yml-файла</param>
        /// <param name="_logfile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        public YMLStruct(YMLLine parent, string _logfile)
        {
            Clear();

            ParentYMLLine = parent;

            if (parent != null)
            {
                logFile = parent.logFile;
            }

            if (!string.IsNullOrWhiteSpace(_logfile))
            {
                logFile = _logfile;
            }
        }

        /// <summary>
        /// Очистить содержимое полей в YMLStruct
        /// </summary>
        public void Clear()
        {
            IsAutogen = false;
            IsNoCumulative = false;
            changesetBefore = null;
            Lines = new List<YMLLine>();
            changesetAfter = null;
            IsFileExist = false;
            relativeToChangelogFile = MainWindow.APPinfo.relativeToChangelogFile;
            Project = "";
            Filepath = "";
            Filename = "";
            IsEndLF = true;
            databaseChangeLogEOL = "\n";
            IsIgnore = false;
            changesetPreConditions = new List<YMLChangeset>();
        }

        /// <summary>
        /// Клонирование экземпляра YMLStruct
        /// </summary>
        /// <param name="_parent">ссылка на строку родительского yml-файла</param>
        /// <returns></returns>
        public YMLStruct Copy(YMLLine _parent)
        {
            YMLStruct copy = (YMLStruct)this.MemberwiseClone();

            copy.ParentYMLLine = _parent;

            if (this.changesetBefore != null)
            {
                copy.changesetBefore = (YMLChangeset)this.changesetBefore.Copy();
            }

            if (this.changesetAfter != null)
            {
                copy.changesetAfter = (YMLChangeset)this.changesetAfter.Copy();
            }

            if (this.Lines != null)
            {
                foreach (var item in this.Lines)
                {
                    copy.Lines.Add(item.Copy(copy));
                }
            }

            if (this.changesetPreConditions != null)
            {
                foreach (var item in this.changesetPreConditions)
                {
                    copy.changesetPreConditions.Add(item.Copy());
                }
            }

            return copy;
        }

        //-----------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Признак наличия yml-файла
        /// </summary>
        public bool IsFileExist { get; set; }

        /// <summary>
        /// Признак наличия перевода строки в конце yml-файла
        /// </summary>
        public bool IsEndLF { get; set; }

        /// <summary>
        /// что использовать для перевода строки после databaseChangeLog, по умолчанию \n
        /// </summary>
        public string databaseChangeLogEOL { get; set; }

        string _project;
        /// <summary>
        /// Проект GIT
        /// </summary>
        public string Project 
        { 
            get
            {
                if (string.IsNullOrWhiteSpace(_project) && this.ParentYMLLine != null && this.ParentYMLLine.parentYMLStruct != null)
                {
                    return this.ParentYMLLine.parentYMLStruct.Project ?? ""; //-V3022
                }
                else return _project ?? "";
            }
            
            set
            {
                _project = value;
            } 
        }

        /// <summary>
        /// Путь к проекту GIT
        /// </summary>
        public string ProjectPath
        {
            get
            {
                return Path.Combine(MainWindow.APPinfo.GITFolder, Utilities.GITProjects.GetFolderByProject(Project));
            }
        }

        /// <summary>
        /// Префикс версии
        /// </summary>
        public string Prefix => Utilities.GITProjects.GetPrefixFileReleaseByProject(Project);

        private string _filepath;
        /// <summary>
        /// Путь к файлу внутри проекта GIT
        /// </summary>
        public string Filepath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_filepath) && this.ParentYMLLine != null)
                {
                    return this.ParentYMLLine.path ?? "";
                }
                else return _filepath ?? "";
            }

            set
            {
                _filepath = value;
            }
        }

        string _filename;
        /// <summary>
        /// Имя yml-файла 
        /// </summary>
        public string Filename
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_filename) && this.ParentYMLLine != null)
                {
                    return this.ParentYMLLine.file ?? "";
                }
                else return _filename ?? "";
            }

            set
            {
                _filename = value;
            }
        }

        /// <summary>
        /// "уникальный" ключ для поиска
        /// </summary>
        public string search
        {
            get
            {
                return this.Filename.Replace(Path.DirectorySeparatorChar, '/').ToLower().Trim();
            }
        }


        /// <summary>
        /// Полный путь к yml-файлу
        /// </summary>
        public string FullFilename
        {
            get
            {
                return Path.Combine(ProjectPath, Filepath, Filename);
            }
        }

        /// <summary>
        /// Родительский yml-файл
        /// </summary>
        public YMLLine ParentYMLLine { get; set; } = null;

        /// <summary>
        /// Флаг автогенного yml-файла (есть строка #AUTOGEN)
        /// </summary>
        public bool IsAutogen { get; set; }

        /// <summary>
        /// =true - НЕ кумулятивная версия, в yml-файл есть строка #NOCUMULATIVE
        /// </summary>
        public bool IsNoCumulative { get; set; }

        /// <summary>
        /// =true - кумулятивная версия, в yml-файл НЕТ строки #NOCUMULATIVE
        /// </summary>
        public bool IsCumulative => IsNoCumulative != true;

        /// <summary>
        /// Флаг yml-файла, который необходимо игнорировать (есть строка #IGNORE)
        /// </summary>
        public bool IsIgnore { get; set; }

        /// <summary>
        /// Заголовок yml-файла
        /// </summary>
        public YMLChangeset changesetBefore { get; set; }

        /// <summary>
        /// Список строк в yml-файле
        /// </summary>
        public List<YMLLine> Lines { get; set; }

        // 
        /// <summary>
        /// Предыдущие версии - список строк include из папки version
        /// </summary>
        public List<YMLLine> PrevVersions
        {
            get
            {
                return Lines.Where(x => x.type == YMLLineType.VERSION).ToList();
            }
        }

        /// <summary>
        /// Первая предыдущая версия
        /// </summary>
        public YMLLine FirstPrevVersion
        {
            get
            {
                return Lines.Where(x => x.type == YMLLineType.VERSION).FirstOrDefault();
            }
        }

        /// <summary>
        /// Подвал yml-файла
        /// </summary>
        public YMLChangeset changesetAfter { get; set; }

        /// <summary>
        /// Проверки yml-файла
        /// </summary>
        public List<YMLChangeset> changesetPreConditions { get; set; }

        /// <summary>
        /// Есть проверочный changeset с именем cumulative_gap
        /// </summary>
        public bool hasCumulativeGap
        {
            get
            {
                if (changesetPreConditions == null) return false;

                var found = changesetPreConditions
                    .Where(x => x.isPreConditions && x.id.ToLower() == "cumulative_gap")
                    .FirstOrDefault();

                return found != null;
            }
        }

        /// <summary>
        /// true - при сохранении yml-файла пути внутри него будут записаны в относительном виде от места расположения yml-файла
        /// false - при сохранении yml-файла пути внутри него будут записаны в абсолютном виде от места расположения корня проекта
        /// </summary>
        public string relativeToChangelogFile { get; set; }

        /// <summary>
        /// Номер версии БЕЗ префикса по содержимому файла
        /// </summary>
        public string NumVersionFromChangeset
        {
            get
            {
                string version_no_prefix = "";

                // определяем номер по changeSet начальный, поле id
                if (
                    (changesetBefore != null) &&
                    (!string.IsNullOrWhiteSpace(changesetBefore.id)) &&
                    string.IsNullOrWhiteSpace(version_no_prefix) //-V3063
                )
                {
                    version_no_prefix = Release.GetNumVersion(Prefix, changesetBefore.id);
                    if (Release.VerAsNum(version_no_prefix) == 0) //-V3024
                    {
                        version_no_prefix = "";
                    }
                }

                // определяем номер по changeSet начальный, поле comment
                if (
                    (changesetBefore != null) &&
                    (!string.IsNullOrWhiteSpace(changesetBefore.comment)) &&
                    string.IsNullOrWhiteSpace(version_no_prefix)
                )
                {
                    version_no_prefix = Release.GetNumVersion(Prefix, changesetBefore.comment);
                    if (Release.VerAsNum(version_no_prefix) == 0) //-V3024
                    {
                        version_no_prefix = "";
                    }
                }

                // определяем номер по changeSet финальный, поле id
                if (
                    (changesetAfter != null) &&
                    (!string.IsNullOrWhiteSpace(changesetAfter.id)) &&
                    string.IsNullOrWhiteSpace(version_no_prefix)
                )
                {
                    version_no_prefix = Release.GetNumVersion(Prefix, changesetAfter.id);
                    if (Release.VerAsNum(version_no_prefix) == 0) //-V3024
                    {
                        version_no_prefix = "";
                    }
                }

                // определяем номер по changeSet финальный, поле comment
                if (
                    (changesetAfter != null) &&
                    (!string.IsNullOrWhiteSpace(changesetAfter.comment)) &&
                    string.IsNullOrWhiteSpace(version_no_prefix)
                )
                {
                    version_no_prefix = Release.GetNumVersion(Prefix, changesetAfter.comment);
                    if (Release.VerAsNum(version_no_prefix) == 0) //-V3024
                    {
                        version_no_prefix = "";
                    }
                }

                return version_no_prefix.Trim();
            }
        }

        /// <summary>
        /// Номер версии БЕЗ префикса по имени файла
        /// </summary>
        public string NumVersionFromFilename => Release.GetNumVersion(Prefix, Path.GetFileName(Filename));

        /// <summary>
        /// Номер версии БЕЗ префикса
        /// </summary>
        public string NumVersion
        {
            get
            {
                // определяем номер по changeSet
                string Num = NumVersionFromChangeset;

                // определяем номер версии по имени файла
                if (string.IsNullOrWhiteSpace(Num))
                {
                    Num = NumVersionFromFilename;
                    if (Release.VerAsNum(Num) == 0) //-V3024
                    {
                        Num = "";
                    }
                }

                return Num.Trim();
            }
        }

        /// <summary>
        /// Номер версии для сортировки
        /// </summary>
        public double NumVersionOrder
        {
            get
            {
                return Release.VerAsNum(NumVersion);
            }
        }

        /// <summary>
        /// =true - это Service Pack
        /// </summary>
        public static bool isSP (double Num)
        {
            int x = (int)(Num % 1000000000);
            return x == 0;
        }

        /// <summary>
        /// =true - это HotFix
        /// </summary>
        public static bool isHF (double Num)
        {
            int x = (int)(Num % 1000000);
            return (x == 0) && !isSP(Num);
        }

        /// <summary>
        /// =true - это Extra HotFix
        /// </summary>
        public static bool isEHF(double Num)
        {
            int x = (int)(Num % 1000);
            return (x == 0) && !isHF(Num);
        }

        /// <summary>
        /// Удалить строку из yml-файла
        /// </summary>
        /// <param name="item">строка yml-файла</param>
        public void DeleteYML(YMLLine item)
        {
            if (item != null) Lines.Remove(item);
        }

        /// <summary>
        /// Генерация текста yml-файла
        /// </summary>
        /// <returns>текст yml-файла</returns>
        public override string ToString()
        {
            string result = "";

            if (IsAutogen)
            {
                result += $"#AUTOGEN\n";
            }

            if (IsNoCumulative)
            {
                result += $"#NOCUMULATIVE\n";
            }

            if (IsIgnore)
            {
                result += $"#IGNORE\n";
            }

            result += "databaseChangeLog:" + databaseChangeLogEOL;
            string lineEOL = "\n";
            bool isLastEmpty = false;

            if (changesetPreConditions != null)
            {
                foreach (var item in changesetPreConditions)
                {
                    if (!string.IsNullOrWhiteSpace(item.ToString()))
                    {
                        result += item.ToString() + lineEOL + lineEOL;
                        isLastEmpty = true;
                    }
                }
            }

            if (changesetBefore != null && (!string.IsNullOrWhiteSpace(changesetBefore.ToString())))
            {
                result += changesetBefore.ToString()+ lineEOL + lineEOL;
                isLastEmpty = true;
            }

            // сначала предыдущие версии
            foreach (var item in PrevVersions.OrderBy(x => x.order))
            {
                if (this.relativeToChangelogFile == "true")
                {
                    result += 
                    "- include: { file: \"../" + item.path + "/" + item.file + "\", relativeToChangelogFile: \"true\" }" + item.EOL;
                }
                else
                {
                    result += 
                    "- include: { file: \"" + item.path + "/" + item.file + "\", relativeToChangelogFile: \"false\" }" + item.EOL;
                }

                isLastEmpty = false;
            }

            if (PrevVersions.Count > 0)
            {
                result += lineEOL;
                isLastEmpty = true;
            }

            // потом все остальное
            foreach (var item in Lines.Where(x=> x.type != YMLLineType.VERSION).OrderBy(x => x.order))
            {
                isLastEmpty = false;

                string start_spaces = "";
                if (
                    (item.text != null) &&
                    item.text.StartsWith(" ")
                    )
                {
                    for (int i = 0; i < item.text.Length; i++)
                    {
                        if (item.text[i] == ' ') start_spaces += " ";
                        else break;
                    }
                }

                string finish_spaces = "";
                if (
                    (item.text != null) &&
                    item.text.EndsWith(" ")
                    )
                {
                    for (int i = item.text.Length-1; i >= 0; i--)
                    {
                        if (item.text[i] == ' ') finish_spaces += " ";
                        else break;
                    }
                }

                switch (item.type)
                {
                    case YMLLineType.VERSION:
                        break;
                    case YMLLineType.TASK:
                    case YMLLineType.REPORT:
                    case YMLLineType.SQLDATA:
                    case YMLLineType.SQLSTRUCT:
                        if (this.relativeToChangelogFile == "true")
                        {
                            result += start_spaces +
                            "- include: { file: \"../" + item.path + "/" + item.file + "\", relativeToChangelogFile: \"true\" }" + finish_spaces + item.EOL;
                        }
                        else
                        {
                            result += start_spaces +
                            "- include: { file: \"" + item.path + "/" + item.file + "\", relativeToChangelogFile: \"false\" }" + finish_spaces + item.EOL;
                        }
                        break;
                    case YMLLineType.EMPTYLINE:
                        result += start_spaces + item.EOL;
                        isLastEmpty = true;
                        break;
                    case YMLLineType.COMMENT:
                    case YMLLineType.UNKNOWN:
                    default:
                        result += item.text + item.EOL;
                        break;
                }
            }

            if (changesetAfter != null && (!string.IsNullOrWhiteSpace(changesetAfter.ToString())))
            {
                if (!isLastEmpty) result += lineEOL;

                result += changesetAfter.ToString() + lineEOL;
            }

            // если в конце файла не было символа перевода строки, убираем его
            if (!IsEndLF)
            {
                if (result.EndsWith("\n"))
                {
                    result = result.Substring(0, result.Length - 1);
                }
                if (result.EndsWith("\r"))
                {
                    result = result.Substring(0, result.Length - 1);
                }
            }

            return result;
        }

        /// <summary>Загрузить повторно YML-файл</summary>
        /// <param name="isLoadPrevVersion">скачать предыдущие версии</param>
        /// <param name="Versions">история загрузки версий - чтобы исключить зацикливание</param>
        /// <param name="isLoadTask">скачать задачи версии</param>
        /// <param name="isLoadReport">скачать отчетные yml версии</param>
        public void ReLoadYML(bool isLoadPrevVersion, List<string> Versions, bool isLoadTask, bool isLoadReport)
        {
            LoadYML(this.Project, this.Filepath, this.Filename, isLoadPrevVersion, Versions, isLoadTask, isLoadReport);
        }

        /// <summary>Загрузка YML-файла по его имени из папки проекта GIT</summary>
        /// <param name="Project">проект GIT</param>
        /// <param name="YMLPath">папка в проекте, где лежит yml-файл (Report или task или version)</param>
        /// <param name="YMLFile">YML-файл</param>
        /// <param name="isLoadPrevVersion">скачать предыдущие версии</param>
        /// <param name="Versions">история загрузки версий - чтобы исключить зацикливание</param>
        /// <param name="isLoadTask">скачать задачи версии</param>
        /// <param name="isLoadReport">скачать отчетные yml версии</param>
        public void LoadYML(string Project, string YMLPath, string YMLFile, bool isLoadPrevVersion, List<string> Versions, bool isLoadTask, bool isLoadReport)
        {
            Clear();

            if (Versions == null) Versions = new List<string>();

            string ProjectPath = Path.Combine(MainWindow.APPinfo.GITFolder, Utilities.GITProjects.GetFolderByProject(Project));
            string ff = Path.Combine(ProjectPath, YMLPath, YMLFile);
            if (File.Exists(ff))
            {
                this.IsFileExist = true;
                this.Project = Project;
                this.Filepath = YMLPath;

                // определить реальное имя YML-файла
                YMLFile = Path.GetFileName(
                    Utilities.Files.GetRealFilename(ff)
                    );

                this.Filename = YMLFile;

                // считываем файл в буфер
                string buffer = File.ReadAllText(ff, Encoding.UTF8);
                if (string.IsNullOrWhiteSpace(buffer))
                {
                    buffer = "";
                }

                // проверяем наличие в конце файла символа перевода строки, убираем его
                IsEndLF = buffer.EndsWith("\n");
                if (IsEndLF)
                {
                    buffer = buffer.Substring(0, buffer.Length - 1);
                }

                // получаем список строк
                //List<string> lines = File.ReadAllLines(this.FullFilename, Encoding.UTF8).ToList();
                List<string> lines = buffer.ToList(new char[] { '\n' }, false);

                // счетчик строк
                int cnt = 0;

                // флаг первого include (кроме версии)
                bool _isinclude_task = false;

                // текущий changeset
                YMLChangeset _changeset = null;
                YMLPreConditions _preConditions = null;
                YMLChangeSetExecuted _changeSetExecuted = null;
                YMLSqlCheck _sqlCheck = null;

                // перебираем строки файла, распознаем и заполняем структуру файла
                foreach (string ll in lines)
                {
                    string item = ll.Replace("\r", "");

                    string line_trim = item.Trim(new[] { ' ', '\t' });
                    string line_seek = line_trim
                                            .ToLower()
                                            .Replace(" ", "")
                                            .Replace("\t", "");
                    string _id = KeyWord.KeyValue(item, "id", new char[] { ':' });
                    string _author = KeyWord.KeyValue(item, "author", new char[] { ':' });
                    string _labels = KeyWord.KeyValue(item, "labels", new char[] { ':' });
                    string _comment = KeyWord.KeyValue(item, "comment", new char[] { ':' });
                    string _runAlways = KeyWord.KeyValue(item, "runAlways", new char[] { ':' });
                    string _onFail = KeyWord.KeyValue(item, "- onFail", new char[] { ':' });
                    string _onFailMessage = KeyWord.KeyValue(item, "- onFailMessage", new char[] { ':' });
                    string _changeLogFile = KeyWord.KeyValue(item, "changeLogFile", new char[] { ':' });
                    string _expectedResult = KeyWord.KeyValue(item, "expectedResult", new char[] { ':' });
                    string _sql = KeyWord.KeyValue(item, "sql", new char[] { ':' });

                    cnt++;

                    YMLLine ymlline = new YMLLine(this, this.logFile);
                    ymlline.order = cnt;
                    ymlline.text = item;
                    ymlline.isLoaded = true;
                    ymlline.EOL = ll.EndsWith("\r") ? Environment.NewLine : "\n";

                    if (line_seek.StartsWith("databasechangelog:"))
                    {
                        databaseChangeLogEOL = ll.EndsWith("\r") ? Environment.NewLine : "\n"; //-V3086
                    }
                    else if (line_seek.StartsWith("-changeset:"))
                    {
                        // changeSet

                        if (_changeset != null)
                        {
                            if (_preConditions != null)
                            {
                                if (_changeSetExecuted != null)
                                {
                                    _preConditions.changeSetExecuted = _changeSetExecuted.Copy();
                                }
                                if (_sqlCheck != null)
                                {
                                    _preConditions.sqlCheck = _sqlCheck.Copy();
                                }
                                _changeset.preConditions = _preConditions.Copy();
                            }

                            if (_changeset.isPreConditions)
                            {
                                // проверочный changeSet
                                changesetPreConditions.Add(_changeset.Copy());
                            }
                            else if (changesetBefore == null)
                            {
                                // начальный changeSet
                                changesetBefore = _changeset.Copy();
                            }
                            else if (changesetAfter == null)
                            {
                                // финальный changeSet
                                changesetAfter = _changeset.Copy();
                            }
                            _preConditions = null;
                            _changeSetExecuted = null;
                            _sqlCheck = null;
                        }

                        _changeset = new YMLChangeset();
                    }
                    else if (line_seek.StartsWith("id:"))
                    {
                        if (_changeSetExecuted != null)
                        {
                            // id для changeSetExecuted
                            _changeSetExecuted.id = _id;
                        }
                        else if (_changeset != null)
                        {
                            // id для changeSet
                            _changeset.id = _id;
                        }
                    }
                    else if (line_seek.StartsWith("author:"))
                    {
                        if (_changeSetExecuted != null)
                        {
                            // id для changeSetExecuted
                            _changeSetExecuted.author = _author;
                        }
                        else if (_changeset != null)
                        {
                            // author для changeSet
                            _changeset.author = _author;
                        }
                    }
                    else if (line_seek.StartsWith("labels:"))
                    {
                        if (_changeset != null)
                        {
                            // labels для changeSet
                            _changeset.labels = _labels;
                        }
                    }
                    else if (line_seek.StartsWith("changelogfile:"))
                    {
                        if (_changeSetExecuted != null)
                        {
                            // changeLogFile для changeSetExecuted
                            _changeSetExecuted.changeLogFile = _changeLogFile;
                        }
                    }
                    else if (line_seek.StartsWith("comment:"))
                    {
                        // comment для changeSet
                        if (_changeset != null)
                        {
                            _changeset.comment = _comment;
                        }
                        
                    }
                    else if (line_seek.StartsWith("runalways:"))
                    {
                        // runAlways для changeSet
                        if (_changeset != null)
                        {
                            _changeset.runAlways = _runAlways;
                        }

                    }
                    else if (line_seek.StartsWith("preconditions:"))
                    {
                        // preConditions для changeSet

                        if (
                            (_changeset != null) &&
                            (_preConditions != null)
                        )
                        {
                            if (_changeSetExecuted != null)
                            {
                                _preConditions.changeSetExecuted = _changeSetExecuted.Copy();
                            }
                            if (_sqlCheck != null)
                            {
                                _preConditions.sqlCheck = _sqlCheck.Copy();
                            }

                            _changeset.preConditions = _preConditions.Copy();

                            _changeSetExecuted = null;
                            _sqlCheck = null;
                        }

                        _preConditions = new YMLPreConditions();
                    }
                    else if (line_seek.StartsWith("-onfail:"))
                    {
                        if (_preConditions != null)
                        {
                            // onFail для preConditions
                            _preConditions.onFail = _onFail;
                        }
                    }
                    else if (line_seek.StartsWith("-onfailmessage:"))
                    {
                        if (_preConditions != null)
                        {
                            // onFailMessage для preConditions
                            _preConditions.onFailMessage = _onFailMessage;
                        }
                    }
                    else if (line_seek.StartsWith("-changesetexecuted:"))
                    {
                        // changeSetExecuted для preConditions
                        if (_preConditions != null)
                        {
                            if (_changeSetExecuted != null)
                            {
                                _preConditions.changeSetExecuted = _changeSetExecuted.Copy();
                            }

                            _changeSetExecuted = new YMLChangeSetExecuted();
                        }
                    }
                    else if (line_seek.StartsWith("-sqlcheck:"))
                    {
                        // sqlCheck для preConditions
                        if (_preConditions != null)
                        {
                            if (_sqlCheck != null)
                            {
                                _preConditions.sqlCheck = _sqlCheck.Copy();
                            }

                            _sqlCheck = new YMLSqlCheck();
                        }
                    }
                    else if (line_seek.StartsWith("expectedresult:"))
                    {
                        if (_sqlCheck != null)
                        {
                            // expectedResult для sqlCheck
                            _sqlCheck.expectedResult = _expectedResult;
                        }
                    }
                    else if (line_seek.StartsWith("sql:"))
                    {
                        if (_sqlCheck != null)
                        {
                            // sql для sqlCheck
                            _sqlCheck.sql = _sql;
                        }
                    }
                    else if (line_seek.StartsWith("#autogen"))
                    {
                        // признак, что файл сгенерирован автоматически
                        this.IsAutogen = true;
                    }
                    else if (line_seek.StartsWith("#nocumulative"))
                    {
                        // признак, что версия НЕ кумулятивная
                        this.IsNoCumulative = true;
                    }
                    else if (line_seek.StartsWith("#ignore"))
                    {
                        // признак, что версию надо игнорировать
                        this.IsIgnore = true;
                    }
                    else if (line_seek.StartsWith("#"))
                    {
                        // закомментированная строка 
                        ymlline.type = YMLLineType.COMMENT;
                        this.Lines.Add(ymlline);
                    }
                    else if (string.IsNullOrWhiteSpace(line_trim))
                    {
                        if (
                            _isinclude_task &&
                            (changesetAfter == null)
                            )
                        {
                            // пустая строка  (после первого include задачи, но до появления финального changeset)
                            ymlline.type = YMLLineType.EMPTYLINE;
                            this.Lines.Add(ymlline);
                        }
                    }
                    else if (line_seek.StartsWith("-include:"))
                    {
                        // строка include
                        var arr = item.Split(new char[] { '{', '}' });
                        if (arr.Length > 1)
                        {
                            var keys = arr[1].Split(',');

                            for (int i = 0; i < keys.Length; i++)
                            {
                                string file = KeyWord.KeyValue(keys[i], "file", new char[] { ':' });
                                string rtcf = KeyWord.KeyValue(keys[i], "relativeToChangelogFile", new char[] { ':' });

                                if (!string.IsNullOrWhiteSpace(rtcf))
                                {
                                    ymlline.relativeToChangelogFile = rtcf;
                                }
                                else if (!string.IsNullOrWhiteSpace(file))
                                {
                                    if (file.ToLower().EndsWith(".yml"))
                                    {
                                        file = file.Replace(Path.DirectorySeparatorChar, '/').Replace("//", "/").Replace("../", "").Replace("./", "");

                                        if (
                                            file.ToLower().StartsWith("task/") ||
                                            file.ToLower().StartsWith("/task/")
                                            )
                                        {
                                            _isinclude_task = true;
                                            ymlline.type = YMLLineType.TASK;
                                            ymlline.path = "task";
                                            ymlline.file = Path.GetFileName(file);
                                            this.Lines.Add(ymlline);
                                        }
                                        else if (
                                            file.ToLower().StartsWith("version/") ||
                                            file.ToLower().StartsWith("/version/")
                                            )
                                        {
                                            ymlline.type = YMLLineType.VERSION;
                                            ymlline.path = "version";
                                            ymlline.file = Path.GetFileName(file);
                                            this.Lines.Add(ymlline);
                                        }
                                        else if (
                                            file.StartsWith("/") &&
                                            (!file.Substring(1).Contains("/")) 
                                        )
                                        {
                                            ymlline.type = YMLLineType.VERSION;
                                            ymlline.path = "version";
                                            ymlline.file = file.Substring(1);
                                            this.Lines.Add(ymlline);
                                        }
                                        else if (!file.Contains("/"))
                                        {
                                            ymlline.type = YMLLineType.VERSION;
                                            ymlline.path = "version";
                                            ymlline.file = file;
                                            this.Lines.Add(ymlline);
                                        }
                                        else if (
                                            file.StartsWith("Report/") ||
                                            file.StartsWith("/Report/")
                                            )
                                        {
                                            _isinclude_task = true;
                                            ymlline.type = YMLLineType.REPORT;
                                            ymlline.path = "Report";
                                            ymlline.file = Path.GetFileName(file);
                                            this.Lines.Add(ymlline);
                                        }
                                        else
                                        {
                                            _isinclude_task = true;
                                            // нераспознанная строка 
                                            ymlline.type = YMLLineType.UNKNOWN;
                                            ymlline.text = "# нераспознанная строка: " + item;
                                            this.Lines.Add(ymlline);
                                        }
                                    }
                                    else if (file.ToLower().EndsWith(".sql"))
                                    {
                                        _isinclude_task = true;
                                        file = file
                                            .Replace(Path.DirectorySeparatorChar, '/')
                                            .Replace("//", "/")
                                            .Replace("../", "")
                                            .Replace("./", "");
                                        if (file.StartsWith("/")) file = file.Substring(1);

                                        ymlline.path = Path.GetDirectoryName(file).Replace(Path.DirectorySeparatorChar, '/');
                                        ymlline.file = Path.GetFileName(file);

                                        if (
                                            ymlline.path.ToLower().StartsWith("data/") ||
                                            ymlline.path.ToLower().StartsWith("data_new/") 
                                            )
                                        {
                                            ymlline.type = YMLLineType.SQLDATA;
                                        }
                                        else if (string.IsNullOrWhiteSpace(ymlline.path))
                                        {
                                            ymlline.type = YMLLineType.SQLDATA;
                                            ymlline.path = "Report";
                                        }
                                        else
                                        {
                                            ymlline.type = YMLLineType.SQLSTRUCT;
                                        }

                                        this.Lines.Add(ymlline);
                                    }
                                    else
                                    {
                                        _isinclude_task = true;
                                        // нераспознанная строка 
                                        ymlline.type = YMLLineType.UNKNOWN;
                                        ymlline.text = "# нераспознанная строка: " + item;
                                        this.Lines.Add(ymlline);
                                    }
                                }
                            }
                        }
                        else
                        {
                            _isinclude_task = true;
                            // нераспознанная строка 
                            ymlline.type = YMLLineType.UNKNOWN;
                            ymlline.text = "# нераспознанная строка: " + item;
                            this.Lines.Add(ymlline);
                        }
                    }
                    else
                    {
                        // нераспознанная строка 
                        ymlline.type = YMLLineType.UNKNOWN;
                        ymlline.text = "# нераспознанная строка: " + item;
                        this.Lines.Add(ymlline);
                    }
                }

                if (
                    (_preConditions != null) &&
                    (_sqlCheck != null)
                )
                {
                    // sqlCheck для preConditions
                    _preConditions.sqlCheck = _sqlCheck.Copy();
                    _sqlCheck = null;
                }

                if (
                    (_preConditions != null) &&
                    (_changeSetExecuted != null)
                )
                {
                    // changeSetExecuted для preConditions
                    _preConditions.changeSetExecuted = _changeSetExecuted.Copy();
                    _changeSetExecuted = null;
                }

                if (
                    (_changeset != null) &&
                    (_preConditions != null)
                )
                {
                    // preConditions для changeSet
                    _changeset.preConditions = _preConditions.Copy();
                    _preConditions = null;
                }

                if (_changeset != null)
                {
                    if (_changeset.isPreConditions)
                    {
                        // проверочный changeset
                        changesetPreConditions.Add(_changeset.Copy());
                    }
                    else if (changesetBefore == null)
                    {
                        // начальный changeset
                        changesetBefore = _changeset.Copy();
                    }
                    else if (changesetAfter == null)
                    {
                        // финальный changeset
                        changesetAfter = _changeset.Copy();
                    }
                    _changeset = null;
                }


                /*
                if (this.Lines.Count > 0)
                {
                    if (this.Lines[this.Lines.Count - 1].type == YMLLineType.EMPTYLINE)
                    {
                        //если последняя строка пустая - удаляем ее
                        this.Lines.RemoveAt(this.Lines.Count - 1);
                    }
                }
                */

                // читаем вложенные yml из task
                if (isLoadTask)
                {
                    foreach (var taskyml in Lines.Where(x => x.type == YMLLineType.TASK))
                    {
                        taskyml.loadYMLStruct.LoadYML(Project, taskyml.path, taskyml.file, false, null, isLoadTask, isLoadReport);
                    }
                }

                // читаем вложенные yml из Report
                if (isLoadReport)
                {
                    foreach (var repyml in Lines.Where(x => x.type == YMLLineType.REPORT))
                    {
                        repyml.loadYMLStruct.LoadYML(Project, repyml.path, repyml.file, false, null, isLoadTask, isLoadReport);
                    }
                }

                // сохраняем в историю уже прочитанных версий
                if (YMLPath == "version")
                {
                    Versions.Add(YMLFile);
                }

                // читаем вложенные yml из version
                if (isLoadPrevVersion)
                {
                    foreach (var versionyml in Lines.Where(x => x.type == YMLLineType.VERSION))
                    {
                        // ищем, может уже распознавали эту версию (чтобы исключить зацикливание)
                        var found = Versions.Where(x => x.ToLower() == versionyml.file.ToLower()).Any();
                        if (!found)
                        {
                            versionyml.loadYMLStruct.LoadYML(Project, versionyml.path, versionyml.file, isLoadPrevVersion, Versions, isLoadTask, isLoadReport);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Загрузка YML-файла версии по номеру версии
        /// </summary>
        /// <param name="Project">проект GIT</param>
        /// <param name="version_no_prefix">номер версии БЕЗ префикса</param>
        /// <param name="isLoadPrevVersion">скачать предыдущие версии</param>
        /// <param name="Versions">история загрузки версий - чтобы исключить зацикливание</param>
        /// <param name="isLoadTask">скачать задачи версии</param>
        /// <param name="isLoadReport">скачать отчетные yml версии</param>
        public void LoadYMLByNumVersion(string Project, string version_no_prefix, bool isLoadPrevVersion, List<string> Versions, bool isLoadTask, bool isLoadReport)
        {
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(Project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);

            // ищем файл с версией, если он существует
            var files = Directory.GetFiles(Path.Combine(path, "version"), "*." + version_no_prefix + "_*.yml").ToList();
            if (files == null) files = new List<string>(); //-V3022
            files.AddRange(Directory.GetFiles(Path.Combine(path, "version"), "*." + version_no_prefix + ".yml").ToList());
            foreach (var fullfilename in files)
            {
                if (
                    (!fullfilename.ToLower().Contains("_rpt")) &&
                    (!fullfilename.ToLower().Contains("_ots"))
                    )
                {
                    string filename = Path.GetFileName(fullfilename);

                    this.LoadYML(Project, "version", filename, isLoadPrevVersion, Versions, isLoadTask, isLoadReport);

                    if (
                        (! this.IsIgnore) &&
                        (this.IsFileExist) &&
                        (Release.VerAsNum(version_no_prefix) == this.NumVersionOrder) //-V3024
                    )
                    {
                        return;
                    }
                }
            }

            Clear();
        }

        /// <summary>
        /// Загрузка yml-файла по полному пути к файлу в формате КаталогGIT\task|version|Report\yml-файл
        /// </summary>
        /// <param name="Filepath">Полный путь к yml-файлу</param>
        /// <param name="isLoadPrevVersion">скачать предыдущие версии</param>
        /// <param name="Versions">история загрузки версий - чтобы исключить зацикливание</param>
        /// <param name="isLoadTask">скачать задачи версии</param>
        /// <param name="isLoadReport">скачать отчетные yml версии</param>
        public void LoadYMLByFilepath(string Filepath, bool isLoadPrevVersion, List<string> Versions, bool isLoadTask, bool isLoadReport)
        {
            if (File.Exists(Filepath))
            {
                // разберем путь к файлу
                string filename = Path.GetFileName(Filepath);
                string ymlpath = "";
                string projectFolder = Path.GetDirectoryName(Filepath).Replace('/', Path.DirectorySeparatorChar).ToLower();
                string project = "";

                if (projectFolder.EndsWith(Path.DirectorySeparatorChar + "task"))
                {
                    ymlpath = "task";
                    projectFolder = projectFolder.Replace(Path.DirectorySeparatorChar + "task", "");
                }
                else if (projectFolder.EndsWith(Path.DirectorySeparatorChar + "version"))
                {
                    ymlpath = "version";
                    projectFolder = projectFolder.Replace(Path.DirectorySeparatorChar + "version", "");
                }
                else if (projectFolder.EndsWith(Path.DirectorySeparatorChar + "report"))
                {
                    ymlpath = "Report";
                    projectFolder = projectFolder.Replace(Path.DirectorySeparatorChar + "report", "");
                }

                // проверяем, что файл лежит в каталоге GIT
                if (projectFolder.StartsWith(MainWindow.APPinfo.GITFolder.ToLower()))
                {

                    // исключаем путь к каталогу GIT
                    projectFolder = projectFolder.Substring(MainWindow.APPinfo.GITFolder.ToLower().Length);
                    if (projectFolder.StartsWith(Path.DirectorySeparatorChar + ""))
                    {
                        projectFolder = projectFolder.Substring(1);
                    }

                    // определяем проект
                    project = Utilities.GITProjects.GetProjectByFolder(projectFolder);

                    this.LoadYML(project, ymlpath, filename, isLoadPrevVersion, Versions, isLoadTask, isLoadReport);
                    return;
                }
                else
                {
                    App.AddLog($"Неизвестный проект GIT для {Filepath}", null, App.ShowMessageMode.SHOW, true, logFile);
                }
            }

            Clear();
        }


        /// <summary>
        /// Проверка на зацикливание
        /// </summary>
        /// <param name="err">место зацикливания</param>
        /// <returns>=true - есть зацикливание</returns>
        public bool isLooping(out string err)
        {
            err = "";
            List<string> versions = new List<string>();
            versions.Add(NumVersion);

            foreach (var prev in PrevVersions)
            {
                var _prev = prev;

                do
                {
                    if (versions.Contains(_prev.NumVersionLine))
                    {
                        if (_prev.parentYMLStruct != null)
                        {
                            err = $"{_prev.parentYMLStruct.NumVersion} ( файл {_prev.parentYMLStruct.Filename} )";
                        }
                        return true;
                    }
                    versions.Add(_prev.NumVersionLine);

                    _prev = _prev.loadYMLStruct.FirstPrevVersion;

                } while (_prev != null);

            }
            return false;
        }

        /// <summary>
        /// Цепочка кумулятивности для текущей версии
        /// </summary>
        /// <returns></returns>
        public SortedDictionary<double, string> СumulativeChain()
        {
            SortedDictionary<double, string> chain = new SortedDictionary<double, string>();

            // добавляем предыдущие версии
            if (!isLooping(out string err))
            {
                foreach (var prev in PrevVersions)
                {
                    var _prev = prev;

                    do
                    {
                        if (!chain.ContainsKey(_prev.NumVersionLineOrder))
                        {
                            chain.Add(_prev.NumVersionLineOrder, $"{_prev.NumVersionLine} ( файл {_prev.file} )");
                        }

                        foreach (var item in _prev.loadYMLStruct.PrevVersions)
                        {
                            if (!chain.ContainsKey(item.NumVersionLineOrder))
                            {
                                chain.Add(item.NumVersionLineOrder, $"{item.NumVersionLine} ( файл {item.file} )");
                            }
                        }

                        // далее идем только по первой ссылке
                        _prev = _prev.loadYMLStruct.FirstPrevVersion;

                    } while (_prev != null);

                    // проверяем только первую ссылку на предыдущую версию
                    break; //-V3020
                }
            }
            else
            {
                App.AddLog($"Зацикливание в версии {err} !", null, App.ShowMessageMode.SHOW, true, logFile);
            }

            // добавляем текущую версию
            chain.Add(NumVersionOrder, $"{NumVersion} ( файл {Filename} )");

            // добавляем следующие версии
            if (ParentYMLLine != null)
            {
                var _next = ParentYMLLine;

                do
                {
                    if (!chain.ContainsKey(_next.NumVersionLineOrder))
                    {
                        chain.Add(_next.NumVersionLineOrder, $"{_next.NumVersionLine} ( файл {_next.file} )");
                    }

                    if ((_next.parentYMLStruct.NumVersionOrder > 0) &&
                        (!chain.ContainsKey(_next.parentYMLStruct.NumVersionOrder))
                    )
                    {
                        chain.Add(_next.parentYMLStruct.NumVersionOrder, $"{_next.parentYMLStruct.NumVersion} ( файл {_next.parentYMLStruct.Filename} )");
                    }

                    _next = _next.parentYMLStruct.ParentYMLLine;

                } while (_next != null);
            }

            return chain;
        }


        /// <summary>
        /// Вернуть версию, c которой начинается кумулятивность
        /// </summary>
        /// <returns>версия</returns>
        public string СumulativeBegin()
        {
            foreach (var ver in СumulativeChain().OrderBy(x => x.Key))
            {
                return ver.Value;
            }

            return "";
        }

        /// <summary>
        /// Вернуть версию, на которой завершается кумулятивность
        /// </summary>
        /// <returns>версия</returns>
        public string СumulativeEnd()
        {
            string result = "";

            foreach (var ver in СumulativeChain().OrderBy(x => x.Key))
            {
                result = ver.Value;
            }

            return result;
        }

        /// <summary>
        /// Список yml-файлов, входящих в состав текущего yml-файла
        /// </summary>
        /// <param name="isListPrevVersion">добавлять yml-файлы из предыдущих версий</param>
        /// <returns></returns>
        public List<YMLStruct> ListYMLStruct(bool isListPrevVersion)
        {
            List<YMLStruct> result = new List<YMLStruct>();

            // читаем вложенные yml из version
            if (isListPrevVersion)
            {
                foreach (var versionyml in Lines.Where(x => x.type == YMLLineType.VERSION).OrderBy(x => x.order))
                {
                    if (versionyml.loadYMLStruct != null)
                    {
                        if (versionyml.loadYMLStruct.IsFileExist)
                        {
                            // добавляем список yml-файлов из предыдущих версий
                            result.AddRange(versionyml.loadYMLStruct.ListYMLStruct(isListPrevVersion));
                        }
                        result.Add(versionyml.loadYMLStruct);
                    }
                }
            }

            // читаем вложенные yml из task
            foreach (var taskyml in Lines.Where(x => x.type == YMLLineType.TASK).OrderBy(x => x.order))
            {
                if (taskyml.loadYMLStruct != null)
                {
                    if (taskyml.loadYMLStruct.IsFileExist)
                    {
                        // добавляем список yml-файлов из задачи
                        result.AddRange(taskyml.loadYMLStruct.ListYMLStruct(isListPrevVersion));
                    }
                    result.Add(taskyml.loadYMLStruct);
                }
            }

            // читаем вложенные yml из Report
            foreach (var repyml in Lines.Where(x => x.type == YMLLineType.REPORT).OrderBy(x => x.order))
            {
                if (repyml.loadYMLStruct != null)
                {
                    if (repyml.loadYMLStruct.IsFileExist)
                    {
                        // добавляем список yml-файлов из задачи
                        result.AddRange(repyml.loadYMLStruct.ListYMLStruct(isListPrevVersion));
                    }
                    result.Add(repyml.loadYMLStruct);
                }
            }

            return result;
        }

        /// <summary>
        /// Список yml-файлов, входящих в состав текущего yml-файла
        /// </summary>
        /// <param name="isListPrevVersion">добавлять sql-файлы из предыдущих версий</param>
        /// <returns></returns>
        public Dictionary<string, string> ListYML(bool isListPrevVersion)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            foreach (var item in ListYMLStruct(isListPrevVersion))
            {
                if (!result.ContainsKey(item.search))
                {
                    result.Add(item.search, item.FullFilename.Trim());
                }
            }

            return result;
        }

        /// <summary>
        /// =true - содержит yml-файл
        /// </summary>
        /// <param name="ymlfile">искомый yml-файл (имя файл без указания пути внутри проекта, ищем только в version|task|Report)</param>
        /// <returns></returns>
        public bool ContainsYML(string ymlfile)
        {
            // перебираем вложенные sql
            foreach (var item in this.ListYML(false).Where(x => x.Key == ymlfile.ToLower().Trim()))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Список sql-файлов, входящих в состав yml-файла
        /// </summary>
        /// <param name="isListPrevVersion">добавлять sql-файлы из предыдущих версий</param>
        /// <returns></returns>
        public List<YMLLine> ListSQLLine(bool isListPrevVersion)
        {
            List<YMLLine> result = new List<YMLLine> ();

            // читаем sql из вложенных yml из version 
            if (isListPrevVersion)
            {
                foreach (var versionyml in Lines.Where(x => x.type == YMLLineType.VERSION).OrderBy(x => x.order))
                {
                    if (
                        (versionyml.loadYMLStruct != null) &&
                        versionyml.loadYMLStruct.IsFileExist
                    )
                    {
                        // добавляем список sql-файлов из предыдущих версий
                        result.AddRange(versionyml.loadYMLStruct.ListSQLLine(isListPrevVersion));
                    }
                }
            }

            // читаем sql из вложенных yml из task
            foreach (var taskyml in Lines.Where(x => x.type == YMLLineType.TASK).OrderBy(x => x.order))
            {
                if (
                        (taskyml.loadYMLStruct != null) &&
                        taskyml.loadYMLStruct.IsFileExist
                    )
                {
                    // добавляем список sql-файлов из задачи
                    result.AddRange(taskyml.loadYMLStruct.ListSQLLine(isListPrevVersion));
                }
            }

            // читаем sql из вложенных yml из Report
            foreach (var repyml in Lines.Where(x => x.type == YMLLineType.REPORT).OrderBy(x => x.order))
            {
                if (
                        (repyml.loadYMLStruct != null) &&
                        repyml.loadYMLStruct.IsFileExist
                    )
                {
                    // добавляем список sql-файлов из задачи
                    result.AddRange(repyml.loadYMLStruct.ListSQLLine(isListPrevVersion));
                }
            }

            // добавляем sql текущего yml
            foreach (var item in Lines
                .Where(x => x.type == YMLLineType.SQLDATA || x.type == YMLLineType.SQLSTRUCT)
                .OrderBy(x => x.order)
            )
            {
                result.Add(item);
            }

            return result;
        }


        /// <summary>
        /// Список sql-файлов, входящих в состав yml-файла
        /// </summary>
        /// <param name="isListPrevVersion">добавлять sql-файлы из предыдущих версий</param>
        /// <returns></returns>
        public Dictionary<string, YMLLine> ListSQL(bool isListPrevVersion)
        {
            Dictionary<string, YMLLine> result = new Dictionary<string, YMLLine>();

            foreach (var item in ListSQLLine(isListPrevVersion))
            {
                if (
                    (!string.IsNullOrWhiteSpace(item.search)) &&
                    (!string.IsNullOrWhiteSpace(item.FullFilename)) &&
                    (!result.ContainsKey(item.search))
                    )
                {
                    result.Add(item.search, item);
                }
            }

            return result;
        }


        /// <summary>
        /// =true - содержит sql-файл 
        /// </summary>
        /// <param name="sqlfile">искомый sql-Файл (включая путь внутри проекта)</param>
        /// <returns></returns>
        public bool ContainsSQL(string sqlfile)
        {
            // перебираем вложенные sql
            foreach (var item in this.ListSQL(false).Where(x => x.Key == sqlfile.Replace(Path.DirectorySeparatorChar, '/').ToLower().Trim()))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Выполнить yml-файл
        /// </summary>
        /// <param name="isExecutePrevVersion">выполнять sql-файлы из предыдущих версий</param>
        /// <param name="_ConnectSQL">подключение к БД в которой надо выполнить скрипт</param>
        /// <param name="_ConnectLog">подключение к БД в которой находится dbo.SQLGenDBLog</param>
        /// <param name="_isExecuteOnce">=true Фиксировать результат выполнения в dbo.SQLGenDBLog и выполнять скрипт однократно</param>
        /// <param name="_isWait">=true Ожидать завершения</param>
        /// <param name="_idle">максимальное время ожидания (сек)</param>
        /// <returns></returns>
        public void ExecuteYML(bool isExecutePrevVersion, bool _isWait, ConnectDB _ConnectSQL, ConnectDB _ConnectLog = null, bool _isExecuteOnce = false, int _idle = 600)
        {
            WinSQLExecute WinSQLExecute = new WinSQLExecute();
            WinSQLExecute.Title = $"Выполнение sql-скриптов из {Filename}";
            WinSQLExecute.execLogFile = logFile;

            // перебираем sql-скрипты
            foreach (var file in this.ListSQL(isExecutePrevVersion))
            {
                YMLLine line = file.Value;

                if (
                    (line != null) &&
                    (!string.IsNullOrWhiteSpace(line.FullFilename))
                )
                {
                    WinSQLExecute.AddSQL(file.Key, line.FullFilename, line.GITKindObject);
                }
            }

            // выполняем
            if (WinSQLExecute.ListSQL.Count > 0)
            {
                WinSQLExecute.Start(_isWait, _ConnectSQL, _ConnectSQL, _isExecuteOnce, Filename, _idle); //-V3038
            }
            else
            {
                WinSQLExecute.Close();
            }
        }

        /// <summary>Проверка YML-файла</summary>
        /// <param name="isPrevVersion">Копировать/проверять файлы из предыдущих версий</param>
        /// <param name="isCorrectEncoding">=true - исправлять кодировку</param>
        /// <param name="isCheckBOM">=true - проверять наличие BOM если кодировка UTF8</param>
        /// <param name="YMLFiles">список yml-файлов с информацией из Jira</param>
        /// <param name="Errors">список ошибок (накопительный список)</param>
        /// <param name="isError">true - есть ошибки (накопительный флаг)</param>
        /// <param name="isRelease">=true - проверка релиза</param>
        /// <param name="isSkipLargeFile">=true - НЕ проверять большие файлы (>10Мб)</param>
        /// <param name="ListVersions">список файлов версий</param>
        /// <param name="CheckLabel">=false - не проверять метки</param>
        public void CheckYML(bool isPrevVersion, bool isCorrectEncoding, bool isCheckBOM, List<YMLFileInfo> YMLFiles, ref string Errors, ref bool isError, bool isRelease, bool isSkipLargeFile, ref List<YMLText> ListVersions, bool CheckLabel)
        {
            // проверяем вложенные yml из version 
            if (isPrevVersion)
            {
                foreach (var line in Lines.Where(x => x.type == YMLLineType.VERSION))
                {
                    if (!string.IsNullOrWhiteSpace(line.file))
                    {
                        if (File.Exists(line.FullFilename))
                        {
                            line.loadYMLStruct.CheckYML(isPrevVersion, isCorrectEncoding, isCheckBOM, YMLFiles, ref Errors, ref isError, isRelease, isSkipLargeFile, ref ListVersions, CheckLabel);
                        }
                        else
                        {
                            Errors += Environment.NewLine + "ОШИБКА: Файл " + line.FullFilename + " не существует !";
                            isError = true;
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(Errors)) Errors = "";
            if (ListVersions == null) ListVersions = new List<YMLText>();
            string DBType = Utilities.GITProjects.GetDBTypeByProject(Project); // тип БД = MSSQL или PGSQL
            Encoding encoding = new UTF8Encoding(false);

            // проверяем текущий yml
            Errors += Environment.NewLine + this.Filename + Environment.NewLine;

            string errinfo = "";
            if (this.IsYMLFileBAD(ref errinfo, ref ListVersions, true))
            {
                isError = true;

                try
                {
                    if (!string.IsNullOrWhiteSpace(logFile))
                    {
                        File.AppendAllText(logFile, Environment.NewLine + errinfo, encoding);
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, false, null);
                }
            }

            if (!string.IsNullOrWhiteSpace(errinfo)) Errors += Environment.NewLine + errinfo;

            // проверяем вложенные yml из task или Report
            foreach (var line in Lines.Where(x => x.type == YMLLineType.TASK || x.type == YMLLineType.REPORT))
            {
                if (!string.IsNullOrWhiteSpace(line.file))
                {
                    string _path = line.path.Replace('/', Path.DirectorySeparatorChar);
                    string fullpath = line.FullFilename;
                    if (File.Exists(fullpath))
                    {
                        try
                        {
                            if (!string.IsNullOrWhiteSpace(logFile))
                            {
                                File.AppendAllText(logFile, Environment.NewLine + Path.Combine(_path, line.file) + ":" + Environment.NewLine, encoding);
                            }
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.SHOW, false, null);
                        }

                        line.loadYMLStruct.CheckYML(isPrevVersion, isCorrectEncoding, isCheckBOM, YMLFiles, ref Errors, ref isError, isRelease, isSkipLargeFile, ref ListVersions, CheckLabel);
                    }
                    else
                    {
                        Errors += Environment.NewLine + "ОШИБКА: Файл " + fullpath + " не существует !";
                        isError = true;
                    }
                }
            }

            // проверяем sql текущего yml
            foreach (var line in Lines
                .Where(x => x.type == YMLLineType.SQLDATA || x.type == YMLLineType.SQLSTRUCT)
            )
            {
                if (!string.IsNullOrWhiteSpace(line.file))
                {
                    string _path = line.path.Replace('/', Path.DirectorySeparatorChar);
                    string fullpath = line.FullFilename;
                    if (File.Exists(fullpath))
                    {
                        bool isCheckRegion = false;
                        bool isBaseRegion = false;
                        var arr = line.file.Split('_');
                        if ((arr.Length >= 1) && Utilities.Databases.regex_region.IsMatch(arr[0])) isCheckRegion = true; //-V3063
                        arr = _path.Split(Path.DirectorySeparatorChar);
                        if ((arr.Length >= 1) && Utilities.Databases.regex_region.IsMatch(arr[0])) isCheckRegion = true; //-V3063

                        if (YMLFiles != null)
                        {
                            foreach (var ymlfile in YMLFiles
                                        .Where(x => 
                                            x.GetYMLFileDefault.ToLower() == this.Filename.ToLower() &&
                                            x.PathInGIT.ToLower() == "task"
                                        )
                            )
                            {
                                if (!ymlfile.Region.ToLower().Contains("базовый")) isCheckRegion = true;
                                if (ymlfile.IsBaseRegion) isBaseRegion = true;
                            }
                        }

                        string errors = "";
                        if (Utilities.YML.IsSQLFileBAD(Project, _path, this.FullFilename, fullpath, isCorrectEncoding, isCheckBOM, isCheckRegion, isBaseRegion, isRelease, ref errors, isSkipLargeFile, logFile, CheckLabel))
                        {
                            isError = true;

                            try
                            {
                                if (!string.IsNullOrWhiteSpace(logFile))
                                {
                                    File.AppendAllText(logFile, Environment.NewLine + errors, encoding);
                                }
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("", ex, App.ShowMessageMode.SHOW, false, null);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(errors)) Errors += Environment.NewLine + errors;
                    }
                    else
                    {
                        Errors += Environment.NewLine + "ОШИБКА: Файл " + fullpath + " не существует !";
                        isError = true;
                    }
                }
            }
        }


        /// <summary>Копирование YML-файла</summary>
        /// <param name="isPrevVersion">Копировать файлы из предыдущих версий</param>
        /// <param name="root_to">Каталог, куда сохраняем файлы</param>
        /// <param name="FullSQLFile">Сводный SQL-файл, в который собираются все скрипты из YML-файла</param>
        /// <param name="listfile">Файл со списком скопированных скриптов</param>
        public bool CopyYML(bool isPrevVersion, string root_to, string FullSQLFile, string listfile)
        {
            bool result = false;

            // копируем вложенные yml из version 
            if (isPrevVersion)
            {
                foreach (var line in Lines.Where(x => x.type == YMLLineType.VERSION))
                {
                    if (
                        (!string.IsNullOrWhiteSpace(line.file)) &&
                        (line.loadYMLStruct != null)
                    )
                    {
                        if (File.Exists(line.FullFilename))
                        {
                            line.loadYMLStruct.CopyYML(isPrevVersion, root_to, FullSQLFile, listfile);
                        }
                        else
                        {
                            App.AddLog("Файл " + line.FullFilename + " не существует !", null, App.ShowMessageMode.SHOW, true, logFile);
                        }
                    }
                }
            }

            string DBType = Utilities.GITProjects.GetDBTypeByProject(Project); // тип БД = MSSQL или PGSQL
            Encoding encoding = new UTF8Encoding(false);

            // копируем вложенные yml из task или Report
            foreach (var line in Lines.Where(x => x.type == YMLLineType.TASK || x.type == YMLLineType.REPORT))
            {
                if (
                    (!string.IsNullOrWhiteSpace(line.file)) &&
                    (line.loadYMLStruct != null)
                )
                {
                    if (File.Exists(line.FullFilename))
                    {
                        if (!string.IsNullOrWhiteSpace(FullSQLFile))
                        {
                            try
                            {
                                File.AppendAllText(FullSQLFile, Environment.NewLine + "--" + line.file + Environment.NewLine, encoding);
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile);
                            }
                            
                        }

                        if (!string.IsNullOrWhiteSpace(listfile))
                        {
                            try
                            {
                                File.AppendAllText(listfile, Environment.NewLine + line.file + ":" + Environment.NewLine, encoding);
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile);
                            }
                        }

                        line.loadYMLStruct.CopyYML(isPrevVersion, root_to, FullSQLFile, listfile);
                    }
                    else
                    {
                        App.AddLog("Файл " + line.FullFilename + " не существует !", null, App.ShowMessageMode.SHOW, true, logFile);
                    }
                }
            }

            // копируем sql текущего yml
            foreach (var line in Lines
                .Where(x => x.type == YMLLineType.SQLDATA || x.type == YMLLineType.SQLSTRUCT)
            )
            {
                if (!string.IsNullOrWhiteSpace(line.file))
                {
                    string _path = line.path.Replace('/', Path.DirectorySeparatorChar);
                    string fullpath = line.FullFilename;
                    if (File.Exists(fullpath))
                    {
                        if (!string.IsNullOrWhiteSpace(FullSQLFile))
                        {
                            try
                            {
                                File.AppendAllText(FullSQLFile, Environment.NewLine + "--" + line.file + Environment.NewLine, encoding);
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile);
                            }
                            
                        }

                        Directory.CreateDirectory(Path.Combine(root_to, _path));
                        Utilities.Files.CopyFile(Path.Combine(ProjectPath, _path), line.file, Path.Combine(root_to, _path), line.file, true, true);

                        if (!string.IsNullOrWhiteSpace(FullSQLFile))
                        {
                            try
                            {
                                File.AppendAllText(FullSQLFile, File.ReadAllText(fullpath), encoding);

                                if (DBType == "MSSQL")
                                {
                                    File.AppendAllText(FullSQLFile, Environment.NewLine + "-------------------------------------------------------------------------------------------------------------------------------------------------", encoding);
                                    File.AppendAllText(FullSQLFile, Environment.NewLine + "GO", encoding);
                                }
                                File.AppendAllText(FullSQLFile, Environment.NewLine + "-------------------------------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine, encoding);
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(listfile))
                        {
                            try
                            {
                                File.AppendAllText(listfile, Path.Combine(_path, line.file) + Environment.NewLine, encoding);
                            }
                            catch (Exception ex)
                            {
                                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile);
                            }
                        }
                    }
                    else
                    {
                        App.AddLog("Файл " + fullpath + " не существует !", null, App.ShowMessageMode.SHOW, true, logFile);
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(this.Filename))
            {
                if (File.Exists(this.FullFilename))
                {
                    Utilities.Files.CopyFile(Path.Combine(ProjectPath, this.Filepath), this.Filename, Path.Combine(root_to, this.Filepath), this.Filename, true, true);
                    result = true;
                }
            }

            return result;
        }


        /// <summary>
        /// Сменить в yml-файле формат пути
        /// </summary>
        /// <param name="isRelativePath">=true - относительно yml-файла, =false - относительно корня проекта</param>
        public void SetRelativeToChangelogFile(bool isRelativePath)
        {
            this.relativeToChangelogFile = isRelativePath == true ? "true" : "false";

            foreach (var item in this.ListYMLStruct(true))
            {
                item.relativeToChangelogFile = this.relativeToChangelogFile;
            }
        }

        /// <summary>Сохранить YML-файл</summary>
        /// <param name="isReSavePrevVersion">true - Пересохранить файлы предыдущих версий</param>
        /// <param name="isReSaveTask">true - Пересохранить файлы задач</param>
        /// <param name="isRelativePath">true - сохранять пути относительно yml-файла, false - сохранять пути относительно корня папки проекта</param>
        /// <param name="isTrimInnerNewLine">убрать внутри текста больше 2-х подряд идущих переводов строки</param>
        /// <param name="isReSaveReport">true - Пересохранить файлы отчетов</param>
        /// <param name="isImproveSQLinVersion">=true - Улучшаем sql-скрипты в yml-файле (проставляем labels, дополняем changeset и пр)</param>
        /// <param name="root_to">если != "" - альтернативная папка, в которую надо сохранить yml-файл (в подпапке version или task или Report) </param>
        /// <param name="isAddVersion">=true - добавить номер версии к имени changeset</param>
        /// <param name="version_no_prefix">номер версии БЕЗ префикса</param>
        public void SaveYML(bool isReSavePrevVersion, bool isReSaveTask, bool isRelativePath, bool isTrimInnerNewLine, bool isReSaveReport, bool isImproveSQLinVersion, string root_to, bool isAddVersion, string version_no_prefix)
        {
            // принудительно меняем relativeToChangelogFile во всех yml-файлах
            this.SetRelativeToChangelogFile(isRelativePath);

            // определяем, куда сохраняем текущий yml-файл
            string _file = Path.GetFileName(this.FullFilename);
            string _destination = Path.GetDirectoryName(this.FullFilename);
            string _ymlpath = "";
            if (_destination.EndsWith("/version"))
            {
                _ymlpath = "version";
                _destination = _destination.Substring(0, _destination.Length - 8);
            }
            else if (_destination.EndsWith("/task"))
            {
                _ymlpath = "task";
                _destination = _destination.Substring(0, _destination.Length - 5);
            }
            if (_destination.EndsWith("/Report"))
            {
                _ymlpath = "Report";
                _destination = _destination.Substring(0, _destination.Length - 7);
            }
            if (!string.IsNullOrWhiteSpace(root_to))
            {
                _destination = root_to;
            }
            _destination = Path.Combine(_destination, _ymlpath);

            if (!Directory.Exists(_destination))
            {
                Directory.CreateDirectory(_destination);
            }

            _destination = Path.Combine(_destination, _file);

            // сохраняем текущий yml-файл
            Utilities.Files.WriteScript(_destination, null, this.ToString(), isTrimInnerNewLine, out string err, FileMode.Create, false, false);

            if (string.IsNullOrWhiteSpace(root_to))
            {
                this.IsFileExist = true;
            }

            // пересохраняем вложенные yml-файлы 
            foreach (var item in this.ListYMLStruct(isReSavePrevVersion))
            {
                if (
                    isReSaveTask &&
                    (item.Filepath == "task") &&
                    (! string.IsNullOrWhiteSpace(item.Filename))
                    )
                {
                    item.SaveYML(false, false, isRelativePath, isTrimInnerNewLine, isReSaveReport, isImproveSQLinVersion, root_to, isAddVersion, version_no_prefix);
                }

                if (
                    isReSaveReport &&
                    (item.Filepath == "Report") &&
                    (!string.IsNullOrWhiteSpace(item.Filename))
                    )
                {
                    item.SaveYML(false, false, isRelativePath, isTrimInnerNewLine, isReSaveReport, false, root_to, false, "");
                }

                if (
                    isReSavePrevVersion &&
                    (item.Filepath == "version") &&
                    (!string.IsNullOrWhiteSpace(item.Filename))
                    )
                {
                    item.SaveYML(false, false, isRelativePath, isTrimInnerNewLine, isReSaveReport, false, root_to, false, "");
                }
            }

            // Улучшаем sql-скрипты в текущем yml-файле (проставляем labels, дополняем changeset и пр)
            if (isImproveSQLinVersion) 
            {
                foreach (var line in this.Lines
                    .Where(x => x.type == YMLLineType.SQLDATA || x.type == YMLLineType.SQLSTRUCT)
                )
                {
                    if (File.Exists(line.FullFilename))
                    {
                        string ScriptType = line.GITTypeObject;
                        if (ScriptType == "" && line.type == YMLLineType.SQLSTRUCT)
                        {
                            ScriptType = "SCHEMA";
                        }
                        string DBType = Utilities.GITProjects.GetDBTypeByProject(Project); // тип БД = MSSQL или PGSQL
                        SQLChangeset.ImproveSQLinVersion(ScriptType, DBType, line.FullFilename, logFile, isAddVersion, version_no_prefix);
                    }
                }
            }
        }


        /// <summary>Получаем список для проверки и компиляции хранимок из YML-файла</summary>
        /// <param name="isCheckPrevVersion">Проверять хранимки из предыдущих версий</param>
        public List<string> ListCheckProc(bool isCheckPrevVersion)
        {
            List<string> result = new List<string>();

            // компилируем\проверяем вложенные yml из version 
            if (isCheckPrevVersion)
            {
                foreach (var line in Lines.Where(x => x.type == YMLLineType.VERSION))
                {
                    result.AddRange(line.loadYMLStruct.ListCheckProc(isCheckPrevVersion));
                }
            }

            // компилируем\проверяем текущий yml
            string GITProjectFolder = Utilities.GITProjects.GetFolderByProject(Project);
            string ProjectPath = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder);
            string DBType = Utilities.GITProjects.GetDBTypeByProject(Project); // тип БД = MSSQL или PGSQL

            // полное имя текущего yml-файла
            string fullYMLFile = Path.Combine(ProjectPath, this.Filepath, this.Filename);

            // компилируем\проверяем вложенные yml из task
            foreach (var line in Lines.Where(x => x.type == YMLLineType.TASK))
            {
                if (File.Exists(line.FullFilename))
                {
                    result.AddRange(line.loadYMLStruct.ListCheckProc(false));
                }
            }

            // компилируем\проверяем хранимки текущего yml
            foreach (var line in Lines
                .Where(x => x.type == YMLLineType.SQLSTRUCT)
            )
            {
                if (File.Exists(line.FullFilename))
                {
                    if (!string.IsNullOrWhiteSpace(line.CheckProc))
                    {
                        result.Add(line.CheckProc);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// разрешенные символы для имен файлов в yml-файлах
        /// </summary>
        public char[] FilenameInYmlRange = "# -:{}\",/._0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToArray();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Проверка yml-файла</summary>
        /// <param name="Errors">список обнаруженных ошибок</param>
        /// <param name="ListVersions">список файлов версий</param>
        /// <param name="isCheckExistInOtherVer">=true - проверять на наличие yml-файла в других версиях</param>
        /// <returns>true - есть ошибки</returns>
        public bool IsYMLFileBAD(ref string Errors, ref List<YMLText> ListVersions, bool isCheckExistInOtherVer)
        {
            if (Errors == null) Errors = "";
            if (ListVersions == null) ListVersions = new List<YMLText>();

            bool result = false;

            if (
                this.Filename.Contains(" ") ||
                this.Filename.Contains("\t")
            )
            {
                result = true;
                Errors += $"ОШИБКА: В имени файла {this.Filename} есть пробелы или символы табуляции !" + Environment.NewLine;
            }

            if (this.IsFileExist)
            {
                string YMLFile = Path.GetFileName(this.FullFilename); // текущий проверяемый yml-файл

                // ================================================================
                // Анализ содержимого скрипта
                // ================================================================

                foreach (var item in this.Lines.Where(x => x.type == YMLLineType.UNKNOWN))
                {
                    result = true;
                    Errors += $"ОШИБКА: {YMLFile} : {item.text}!" + Environment.NewLine;
                }

                if (this.Lines.Where(x => x.hasGoodInclude).Count() == 0)
                {
                    result = true;
                    Errors += $"ОШИБКА: {YMLFile} - файл пустой или поврежден" + Environment.NewLine;
                }

                // проверим пути
                foreach (var item in this.Lines.Where(x =>
                    x.type == YMLLineType.TASK ||
                    x.type == YMLLineType.REPORT ||
                    x.type == YMLLineType.VERSION ||
                    x.type == YMLLineType.SQLDATA ||
                    x.type == YMLLineType.SQLSTRUCT
                ))
                {
                    if (MainWindow.APPinfo.relativeToChangelogFile == "false")
                    {
                        if (item.relativeToChangelogFile != "false")
                        {
                            result = true;
                            Errors += $"ОШИБКА: {YMLFile} : строка {item.text} содержит relativeToChangelogFile <> \"false\"" + Environment.NewLine;
                        }

                        if (item.text.Contains(".."))
                        {
                            result = true;
                            Errors += $"ОШИБКА: {YMLFile} : строка {item.text} содержит .." + Environment.NewLine;
                        }
                    }

                    // ищем кириллицу
                    if (
                        !item.text.Contains("dbo/freedocmarker") &&
                        !item.text.Contains("dbo/freedocrelationship") &&
                        !item.text.All(FilenameInYmlRange.Contains)
                        )
                    {
                        result = true;
                        Errors += $"ОШИБКА: {YMLFile} : строка {item.text} возможно содержит символы кириллицы или другие неразрешенные символы" + Environment.NewLine;
                    }
                }

                // ================================================================
                // Проверка на наличие yml в других версиях
                // ================================================================
                if (isCheckExistInOtherVer)
                {
                    if (ListVersions.Count == 0)
                    {
                        foreach (var filever in Utilities.Files.ListFilesInDir(Path.Combine(ProjectPath, "version"), false, true, false))
                        {
                            // перебираем версии
                            string file = Path.Combine(ProjectPath, "version", filever);

                            try
                            {
                                string text = File.ReadAllText(file);
                                ListVersions.Add(new YMLText() { Text = text, File = Path.GetFileName(file) });
                            }
                            catch //(Exception ex)
                            {
                                //result = true;
                                //Errors += file + ": не удалось прочитать содержимое - " + ex.Message +Environment.NewLine;
                            }
                        }
                    }

                    // yml-файл версии, в которой указан проверяемый файл
                    string ReleaseYMLFile = "";
                    if (
                        (this.ParentYMLLine != null) &&
                        (this.ParentYMLLine.parentYMLStruct != null)
                    )
                    {
                        ReleaseYMLFile = this.ParentYMLLine.parentYMLStruct.Filename;
                    }

                    if (!string.IsNullOrWhiteSpace(ReleaseYMLFile))
                    {
                        // перебираем ДРУГИЕ версии
                        foreach (var filever in ListVersions.Where(x => x.File.ToLower() != ReleaseYMLFile.ToLower()))
                        {
                            if (filever.Text.Contains(YMLFile))
                            {
                                result = true;
                                Errors += "ВНИМАНИЕ: " + YMLFile + " - уже включен в версию " + filever.File + Environment.NewLine;
                            }
                        }
                    }
                }
            }
            else
            {
                result = true;
                Errors += "ОШИБКА: " + this.FullFilename + " не существует" + Environment.NewLine;
            }

            return result;
        }
    }

    /// <summary>
    /// Элемент структуры yml-файла - changeset
    /// </summary>
    public class YMLChangeset
    {
        /// <summary>
        /// Клонирование экземпляра YMLChangeset
        /// </summary>
        /// <returns></returns>
        public YMLChangeset Copy() => (YMLChangeset)MemberwiseClone();

        /// <summary>
        /// поле id в yml-файле
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// поле author в yml-файле
        /// </summary>
        public string author { get; set; }
        /// <summary>
        /// поле comment в yml-файле
        /// </summary>
        public string comment { get; set; }
        /// <summary>
        /// поле labels в yml-файле
        /// </summary>
        public string labels { get; set; }
        /// <summary>
        /// поле runAlways в yml-файле
        /// </summary>
        public string runAlways { get; set; }
        /// <summary>
        /// поле preConditions в yml-файле
        /// </summary>
        public YMLPreConditions preConditions { get; set; }

        /// <summary>
        /// Генерация текста для включения в yml-файл
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return
                "- changeSet:" +
                (string.IsNullOrWhiteSpace(this.id) ? "" : $"\n      id: {this.id}") +
                (string.IsNullOrWhiteSpace(this.author) ? "" : $"\n      author: {this.author}") +
                (string.IsNullOrWhiteSpace(this.comment) ? "" : $"\n      comment: {this.comment}") +
                (string.IsNullOrWhiteSpace(this.labels) ? "" : $"\n      labels: {this.labels}") +
                (string.IsNullOrWhiteSpace(this.runAlways) ? "" : $"\n      runAlways: {this.runAlways}") +
                (this.preConditions == null || string.IsNullOrWhiteSpace(this.preConditions.ToString()) ? "" : this.preConditions.ToString());
        }

        /// <summary>
        /// =true - это проверочный changeset
        /// </summary>
        public bool isPreConditions => (preConditions != null) && (preConditions.onFail != null);
    }

    /// <summary>
    /// раздел preConditions в yml-файле
    /// </summary>
    public class YMLPreConditions
    {
        /// <summary>
        /// Клонирование экземпляра YMLChangeset
        /// </summary>
        /// <returns></returns>
        public YMLPreConditions Copy() => (YMLPreConditions)MemberwiseClone();

        /// <summary>
        /// preConditions/onFail 
        /// </summary>
        public string onFail { get; set; }
        /// <summary>
        /// preConditions/changeSetExecuted
        /// </summary>
        public YMLChangeSetExecuted changeSetExecuted { get; set; }
        /// <summary>
        /// preConditions/sqlCheck
        /// </summary>
        public YMLSqlCheck sqlCheck { get; set; }
        /// <summary>
        /// preConditions/onFailMessage
        /// </summary>
        public string onFailMessage { get; set; }

        /// <summary>
        /// Генерация текста для включения в yml-файл
        /// </summary>
        /// <returns>текст</returns>
        public override string ToString()
        {
            return
                "\n      preConditions:" +
                (string.IsNullOrWhiteSpace(this.onFail) ? "" : $"\n       - onFail: {this.onFail}") +
                (this.changeSetExecuted == null || string.IsNullOrWhiteSpace(this.changeSetExecuted.ToString()) ? "" : this.changeSetExecuted.ToString()) +
                (this.sqlCheck == null || string.IsNullOrWhiteSpace(this.sqlCheck.ToString()) ? "" : this.sqlCheck.ToString()) +
                (string.IsNullOrWhiteSpace(this.onFailMessage) ? "" : $"\n       - onFailMessage: {this.onFailMessage}");
        }
    }

    /// <summary>
    /// раздел preConditions/changeSetExecuted в yml-файле
    /// </summary>
    public class YMLChangeSetExecuted
    {
        /// <summary>
        /// Клонирование экземпляра YMLChangeset
        /// </summary>
        /// <returns></returns>
        public YMLChangeSetExecuted Copy() => (YMLChangeSetExecuted)MemberwiseClone();

        /// <summary>
        /// preConditions/changeSetExecuted/id
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// preConditions/changeSetExecuted/author
        /// </summary>
        public string author { get; set; }
        /// <summary>
        /// preConditions/changeSetExecuted/changeLogFile
        /// </summary>
        public string changeLogFile { get; set; }

        /// <summary>
        /// Генерация текста для включения в yml-файл
        /// </summary>
        /// <returns>текст</returns>
        public override string ToString()
        {
            return
                "\n       - changeSetExecuted:" +
                (string.IsNullOrWhiteSpace(this.id) ? "" : $"\n          id: {this.id}") +
                (string.IsNullOrWhiteSpace(this.author) ? "" : $"\n          author: {this.author}") +
                (string.IsNullOrWhiteSpace(this.changeLogFile) ? "" : $"\n          changeLogFile: {this.changeLogFile}");
        }
    }

    /// <summary>
    /// раздел preConditions/sqlCheck в yml-файле
    /// </summary>
    public class YMLSqlCheck
    {
        /// <summary>
        /// Клонирование экземпляра YMLChangeset
        /// </summary>
        /// <returns></returns>
        public YMLSqlCheck Copy() => (YMLSqlCheck)MemberwiseClone();

        /// <summary>
        /// preConditions/sqlCheck/expectedResult
        /// </summary>
        public string expectedResult;
        /// <summary>
        /// preConditions/sqlCheck/sql
        /// </summary>
        public string sql;

        /// <summary>
        /// Генерация текста для включения в yml-файл
        /// </summary>
        /// <returns>текст</returns>
        public override string ToString()
        {
            return
                "\n       - sqlCheck:" +
                (string.IsNullOrWhiteSpace(this.expectedResult) ? "" : $"\n          expectedResult: {this.expectedResult}") +
                (string.IsNullOrWhiteSpace(this.sql) ? "" : $"\n          sql: {this.sql}");
        }

    }

    /// <summary>
    /// Типы строк в yml-файле
    /// </summary>
    public enum YMLLineType
    {
        /// <summary>
        /// версия
        /// </summary>
        VERSION,
        /// <summary>
        /// задача
        /// </summary>
        TASK,
        /// <summary>
        /// sql-файл с данными
        /// </summary>
        SQLDATA,
        /// <summary>
        /// sql-файл - таблица, хранимка, представление
        /// </summary>
        SQLSTRUCT,
        /// <summary>
        /// комментарий
        /// </summary>
        COMMENT,
        /// <summary>
        /// пустая строка
        /// </summary>
        EMPTYLINE,
        /// <summary>
        /// неизвестный тип
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// yml-файл отчетников
        /// </summary>
        REPORT
    }

    /// <summary>
    /// Элемент структуры yml-файла - строка
    /// </summary>
    public class YMLLine 
    {
        /// <summary>
        /// полный путь к лог-файлу
        /// </summary>
        public string logFile = "";

        /// <summary>
        /// Конструктор YMLLine
        /// </summary>
        /// <param name="parent">ссылка на родительский yml-файл</param>
        /// <param name="_logfile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        public YMLLine(YMLStruct parent, string _logfile)
        {
            parentYMLStruct = parent;

            if (parent != null)
            {
                logFile = parent.logFile;
            }
            
            if (!string.IsNullOrWhiteSpace(_logfile))
            {
                logFile = _logfile;
            }

            loadYMLStruct = new YMLStruct(this, logFile);

            EOL = "\n";
        }

        /// <summary>
        /// Клонирование экземпляра YMLLine
        /// </summary>
        /// <param name="parent">ссылка на родительский yml-файл</param>
        /// <returns></returns>
        public YMLLine Copy(YMLStruct parent)
        {
            YMLLine copy = (YMLLine)this.MemberwiseClone();

            copy.parentYMLStruct = parent;

            if (this.loadYMLStruct != null)
            {
                copy.loadYMLStruct = this.loadYMLStruct.Copy(copy);
            }

            return copy;
        }

        //-----------------------------------------------------------------------------------------

        /// <summary>
        /// Тип строки
        /// </summary>
        public YMLLineType type { get; set; }

        /// <summary>
        /// N по порядку
        /// </summary>
        public int order { get; set; }

        /// <summary>
        /// Текст строки (до распознавания)
        /// </summary>
        public string text { get; set; }

        /// <summary>
        /// Путь к файлу внутри проекта GIT
        /// </summary>
        public string path { get; set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string file { get; set; }

        /// <summary>
        /// путь к файлу внутри проекта
        /// </summary>
        public string path_to_file
        {
            get
            {
                return Path.Combine(this.path, this.file).Replace(Path.DirectorySeparatorChar, '/').Trim();
            }
        }

        /// <summary>
        /// "уникальный" ключ для поиска
        /// </summary>
        public string search
        {
            get
            {
                return path_to_file.ToLower();
            }
        }

        /// <summary>
        /// Значение параметра relativeToChangelogFile
        /// </summary>
        public string relativeToChangelogFile { get; set; }

        /// <summary>
        /// Флаг, что строка загружена из существующего файла, а не добавлена
        /// </summary>
        public bool isLoaded { get; set; } = false;

        /// <summary>
        /// Состав загруженного файла
        /// </summary>
        public YMLStruct loadYMLStruct { get; set; }

        /// <summary>
        /// Родительский yml-файл
        /// </summary>
        public YMLStruct parentYMLStruct { get; set; }

        /// <summary>
        /// что использовать для перевода строки, по умолчанию \n
        /// </summary>
        public string EOL {  get; set; }

        /// <summary>
        /// Тег для группировки
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Тег для сортировки
        /// </summary>
        public string TagOrder
        {
            get
            {
                string numstr = order.ToString().PadLeft(10, '0');
                return Tag + " - " + numstr;
            }
        }

        string _releasetask;
        /// <summary>
        ///  Номер релизной задачи
        /// </summary>
        public string ReleaseTaskNumber
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_releasetask))
                {
                    if ((parentYMLStruct != null) && (parentYMLStruct.ParentYMLLine != null))
                    {
                        return parentYMLStruct.ParentYMLLine.ReleaseTaskNumber ?? ""; //-V3022
                    }
                }

                return _releasetask ?? "";

            }
            set
            {
                _releasetask = value;
            }
        }

        /// <summary>
        /// Информация о дублях
        /// </summary>
        public string PrintDoubleInfo
        {
            get
            {
                string task = "?";
                if (parentYMLStruct != null)
                {
                    task = parentYMLStruct.Filename;
                }

                /*string project = "";

                if (parentYMLStruct != null)
                {
                    project = parentYMLStruct.Project;
                }
                if (string.IsNullOrWhiteSpace(project))
                {
                    project = loadYMLStruct.Project;
                }*/

                return "задача " + ReleaseTaskNumber.PadRight(20, ' ') + " файл " + task + " скрипт " + this.path_to_file;

            }
        }

        /// <summary>
        /// Полный путь к файлу
        /// </summary>
        public string FullFilename
        {
            get
            {
                string project = "";

                if (parentYMLStruct != null)
                {
                    project = parentYMLStruct.Project;
                }
                if (string.IsNullOrWhiteSpace(project))
                {
                    project = loadYMLStruct.Project;
                }

                string folder = Utilities.GITProjects.GetFolderByProject(project);

                return Path.Combine(MainWindow.APPinfo.GITFolder, folder, path.Replace('/', Path.DirectorySeparatorChar), file);
            }
        }

        /// <summary>
        /// Название объекта в БД
        /// </summary>
        public string ObjectName
        {
            get
            {
                string project = "";

                if (parentYMLStruct != null)
                {
                    project = parentYMLStruct.Project;
                }
                if (string.IsNullOrWhiteSpace(project))
                {
                    project = loadYMLStruct.Project;
                }

                switch (this.type)
                {
                    case YMLLineType.VERSION:
                    case YMLLineType.TASK:
                    case YMLLineType.REPORT:
                        return this.file;
                    case YMLLineType.SQLDATA:
                        {
                            return Utilities.Databases.GetFullTableName(
                                this.path
                                .Replace(Path.DirectorySeparatorChar, '/')
                                .Replace("data/", "")
                                .Replace("data_new/", "")
                                .Replace("Report/", "")
                            );
                        }
                    case YMLLineType.SQLSTRUCT:
                        {
                            string result = "";

                            if (Utilities.GITProjects.IsGITProject(project))
                            {
                                result = this.path;
                            }
                            else
                            {
                                result = this.path + "/" + Path.GetFileNameWithoutExtension(this.file);
                            }

                            return Utilities.Databases.GetFullTableName(
                                result
                                .Replace(Path.DirectorySeparatorChar, '/')
                                .Replace("FUNCTION/", "")
                                .Replace("PROCEDURE/", "")
                                .Replace("SEQUENCE/", "")
                                .Replace("TABLE/", "")
                                .Replace("TRIGGER/", "")
                                .Replace("TYPE/", "")
                                .Replace("VIEW/", "")
                                .Replace('/', '.')
                            );
                        }
                    case YMLLineType.COMMENT:
                    case YMLLineType.EMPTYLINE:
                    case YMLLineType.UNKNOWN:
                    default:
                        return "";
                }

            }
        }

        /// <summary>
        /// Тип объекта в проекте GIT
        /// </summary>
        public string GITTypeObject
        {
            get
            {
                switch (this.type)
                {
                    case YMLLineType.VERSION:
                    case YMLLineType.TASK:
                    case YMLLineType.REPORT:
                        return "";
                    case YMLLineType.SQLDATA:
                        {
                            if (this.path.Replace(Path.DirectorySeparatorChar, '/').StartsWith("data_new/"))
                            {
                                return "data_new";
                            }
                            else if (this.path.Replace(Path.DirectorySeparatorChar, '/').StartsWith("Report/"))
                            {
                                return "Report";
                            }
                            else
                            {
                                return "data";
                            }
                        }
                    case YMLLineType.SQLSTRUCT:
                        {
                            var arr = this.path.Replace(Path.DirectorySeparatorChar, '/').Split('/');
                            if (arr.Length > 1)
                            {
                                return arr[1];
                            }
                            else
                            {
                                return "";
                            }
                        }
                    case YMLLineType.COMMENT:
                    case YMLLineType.EMPTYLINE:
                    case YMLLineType.UNKNOWN:
                    default:
                        return "";
                }

            }
        }

        /// <summary>
        /// Вид объекта в проекте GIT
        /// </summary>
        public string GITKindObject
        {
            get
            {
                switch (this.type)
                {
                    case YMLLineType.VERSION:
                    case YMLLineType.TASK:
                    case YMLLineType.REPORT:
                        return "";
                    case YMLLineType.SQLDATA:
                        {
                            return "DATA";
                        }
                    case YMLLineType.SQLSTRUCT:
                        {
                            string type_lower = GITTypeObject.ToLower();
                            if (
                                type_lower == "procedure" ||
                                type_lower == "function" ||
                                type_lower == "view" ||
                                type_lower == "trigger" ||
                                type_lower == "freedocmarker" ||
                                type_lower == "freedocrelationship"
                            )
                            {
                                return "CODE";
                            }
                            else
                            {
                                return "STRUCT";
                            }
                        }
                    case YMLLineType.COMMENT:
                    case YMLLineType.EMPTYLINE:
                    case YMLLineType.UNKNOWN:
                    default:
                        return "";
                }
            }
        }

        /// <summary>
        /// Схема объекта в проекте GIT
        /// </summary>
        public string GITSchemaObject
        {
            get
            {
                switch (this.type)
                {
                    case YMLLineType.VERSION:
                    case YMLLineType.TASK:
                    case YMLLineType.REPORT:
                    case YMLLineType.SQLDATA:
                        return Utilities.Databases.GetSchemaName(
                                this.path
                                .Replace(Path.DirectorySeparatorChar, '/')
                                .Replace("data/", "")
                                .Replace("data_new/", "")
                                .Replace("Report/", "")
                                );
                    case YMLLineType.SQLSTRUCT:
                        {
                            var arr = this.path.Replace(Path.DirectorySeparatorChar, '/').Split('/');
                            if (arr.Length > 0) //-V3022
                            {
                                return arr[0];
                            }
                            else
                            {
                                return "";
                            }
                        }
                    case YMLLineType.COMMENT:
                    case YMLLineType.EMPTYLINE:
                    case YMLLineType.UNKNOWN:
                    default:
                        return "";
                }

            }
        }

        /// <summary>
        /// Имя объекта в проекте GIT
        /// </summary>
        public string GITNameObject
        {
            get
            {
                switch (this.type)
                {
                    case YMLLineType.VERSION:
                    case YMLLineType.TASK:
                    case YMLLineType.REPORT:
                        return "";
                    case YMLLineType.SQLDATA:
                        {
                            return Utilities.Databases.GetTableName(
                                this.path
                                .Replace(Path.DirectorySeparatorChar, '/')
                                .Replace("data/", "")
                                .Replace("data_new/", "")
                                .Replace("Report/", "")
                                );
                        }
                    case YMLLineType.SQLSTRUCT:
                        {
                            var arr = this.path.Replace(Path.DirectorySeparatorChar, '/').Split('/');
                            if (arr.Length > 2)
                            {
                                return arr[2];
                            }
                            else
                            {
                                return Path.GetFileNameWithoutExtension(this.file);
                            }
                        }
                    case YMLLineType.COMMENT:
                    case YMLLineType.EMPTYLINE:
                    case YMLLineType.UNKNOWN:
                    default:
                        return "";
                }

            }
        }


        /// <summary>
        /// Строка содержит ссылку на другой файл
        /// </summary>
        public bool hasGoodInclude
        {
            get
            {
                switch (this.type)
                {
                    case YMLLineType.VERSION:
                    case YMLLineType.TASK:
                    case YMLLineType.REPORT:
                    case YMLLineType.SQLDATA:
                    case YMLLineType.SQLSTRUCT:
                        {
                            return true;
                        }
                    case YMLLineType.COMMENT:
                    case YMLLineType.EMPTYLINE:
                    case YMLLineType.UNKNOWN:
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Команда для проверки/компиляции хранимки
        /// </summary>
        public string CheckProc
        {
            get
            {
                string project = "";

                if (parentYMLStruct != null)
                {
                    project = parentYMLStruct.Project;
                }
                if (string.IsNullOrWhiteSpace(project))
                {
                    project = loadYMLStruct.Project;
                }

                if (
                    GITTypeObject == "FUNCTION" ||
                    GITTypeObject == "PROCEDURE"
                    )
                {
                    if (Utilities.GITProjects.GetDBTypeByProject(project) == "PGSQL")
                    {
                        return $"SELECT FROM dbo.xp_CheckScript (Obj := '{GITSchemaObject}.{GITNameObject}', isNoChild := 2);";
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Команда для проверки/компиляции всех хранимок таблицы
        /// </summary>
        public string CheckTable
        {
            get
            {
                string project = "";

                if (parentYMLStruct != null)
                {
                    project = parentYMLStruct.Project;
                }
                if (string.IsNullOrWhiteSpace(project))
                {
                    project = loadYMLStruct.Project;
                }

                if (GITTypeObject == "TABLE")
                {
                    if (Utilities.GITProjects.GetDBTypeByProject(project) == "PGSQL")
                    {
                        return $"SELECT FROM dbo.xp_CheckScript (Obj := '{GITSchemaObject}.{GITNameObject}', isNoChild := 2);";
                    }
                    else
                    {
                        return $"EXEC dbo.xp_CheckScript @Obj = '{GITSchemaObject}.{GITNameObject}', @isNoChild = 2";
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        /// <summary>
        /// Номер версии для строки с типом VERSION
        /// </summary>
        public string NumVersionLine
        {
            get
            {
                if (type == YMLLineType.VERSION)
                {
                    if (!string.IsNullOrWhiteSpace(loadYMLStruct.NumVersion))
                    {
                        // определяем номер версии из загруженной структуры yml-файла
                        return loadYMLStruct.NumVersion;
                    }

                    // если структура не загружена - определяем по имени файла
                    string project = "";

                    if (parentYMLStruct != null)
                    {
                        project = parentYMLStruct.Project;
                    }
                    if (string.IsNullOrWhiteSpace(project))
                    {
                        project = loadYMLStruct.Project;
                    }

                    string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);

                    // определяем номер версии по имени файла
                    return Release.GetNumVersion(prefix, Path.GetFileName(file));
                }

                return "";
            }
        }

        /// <summary>
        /// Номер версии для сортировки
        /// </summary>
        public double NumVersionLineOrder
        {
            get
            {
                return Release.VerAsNum(NumVersionLine);
            }
        }

        /// <summary>
        /// =true - это Service Pack
        /// </summary>
        public bool isSP
        {
            get
            {
                int x = (int)(NumVersionLineOrder % 1000000000);
                return x == 0;
            }
        }

        /// <summary>
        /// =true - это HotFix
        /// </summary>
        public bool isHF
        {
            get
            {
                int x = (int)(NumVersionLineOrder % 1000000);
                return (x == 0) && !isSP;
            }
        }

        /// <summary>
        /// =true - это Extra HotFix
        /// </summary>
        public bool isEHF
        {
            get
            {
                int x = (int)(NumVersionLineOrder % 1000);
                return (x == 0) && !isHF;
            }
        }
    }

    /// <summary>
    /// Строка yml-файла (упрощенный вариант)
    /// </summary>
    public class YMLText
    {
        /// <summary>
        /// Файл
        /// </summary>
        public string File { get; set; }
        /// <summary>
        /// Строка yml-файла
        /// </summary>
        public string Text { get; set; }
    }
}
