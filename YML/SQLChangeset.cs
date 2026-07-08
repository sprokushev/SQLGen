// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>
    /// Changeset в sql-файле
    /// </summary>
    public class SQLChangeset
    {
        /// <summary>
        /// Шаблон корректного имени changeset
        /// </summary>
        public static Regex regex_changesetname = new Regex(
            @"^(promedweb|rpms|ops|smp|rm|cm|ferdtm|bip|pharmacy1c)-(\d+)(.*)", 
            RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Имя changeset соответствует шаблону корректого имени
        /// </summary>
        /// <param name="_changeset"></param>
        /// <returns></returns>
        public static bool IsGoodChangesetName(string _changeset)
        {
            return regex_changesetname.IsMatch(_changeset);
        }

        /// <summary>
        /// Конструктор SQLChangeset
        /// </summary>
        /// <param name="_name">название changeset</param>
        /// <param name="_author">автор changeset</param>
        /// <param name="_text">содержимое changeset</param>
        /// <param name="_startLine">строка начала changeset в файле</param>
        /// <param name="_endLine">строка окончания changeset в файле</param>
        /// <param name="_changesetline">содержимое строки --changeset</param>
        public SQLChangeset(string _name, string _author, string _text, long _startLine, long _endLine, string _changesetline)
        {
            name = _name;
            author = _author;
            text = _text;
            startLine = _startLine;
            endLine = _endLine;
            hasLiquibaseFormattedSQL = false;

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "unknown";
            }

            if (string.IsNullOrWhiteSpace(author))
            {
                author = "unknown";
            }

            if (!string.IsNullOrWhiteSpace(text)) 
            {
                isMarkRun = text.Contains("--preConditions onFail:MARK_RAN");
            }
            isContextNewDb = false;
            isTestChangeset = false;
            var regextest = new Regex(@"--changeset(.*)(\s+)dev:test(_\S+)?");

            if (
                string.IsNullOrWhiteSpace(_changesetline) &&
                !string.IsNullOrWhiteSpace(text)
            )
            {
                foreach (var item in text.Split('\n'))
                {
                    string line = item.Trim(new char[] { '\t', '\n', '\r', ' ' });

                    if (line.ToLower().StartsWith("--changeset"))
                    {
                        _changesetline = line;
                        break;
                    }
                }
            }

            this.changesetline = _changesetline;

            if (
                this.Tags.TryGetValue("context", out string tags_value) &&
                tags_value == "newdb"
            )
            {
                isContextNewDb = true;
            }

            if (regextest.IsMatch(this.changesetline))
            {
                isTestChangeset = true;
            }
        }

        /// <summary>
        /// название changeset
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// автор changeset
        /// </summary>
        public string author { get; set; }
        /// <summary>
        /// содержимое changeset
        /// </summary>
        public string text { get; set; }
        /// <summary>
        /// строка начала changeset в файле
        /// </summary>
        public long startLine { get; set; }
        /// <summary>
        /// строка окончания changeset в файле
        /// </summary>
        public long endLine { get; set; }
        /// <summary>
        /// содержимое строки --changeset
        /// </summary>
        public string changesetline { get; set; }
        /// <summary>
        /// =true - changeset, который содержит тег context:newdb
        /// </summary>
        public bool isContextNewDb { get; set; }
        /// <summary>
        /// =true - changeset, который содержит тег --preConditions onFail:MARK_RAN
        /// </summary>
        public bool isMarkRun { get; set; }
        /// <summary>
        /// =true - changeset, который при исполнении надо пропустить
        /// </summary>
        public bool isExecuteSkip => isContextNewDb || isMarkRun;
        /// <summary>
        /// =true - перед --changeset есть строка --liquibase formatted sql
        /// </summary>
        public bool hasLiquibaseFormattedSQL { get; set; }
        /// <summary>
        /// =true - тестовый changeset dev:test
        /// </summary>
        public bool isTestChangeset { get; set; }

        /// <summary>
        /// список тегов в строке --changeset
        /// </summary>
        public Dictionary<string, string> Tags
        {
            get
            {
                return YML.GetTagsFromChangeset(changesetline);
            }
        }

        /// <summary>
        /// Удалить в тексте text строку с тегом liquibase formatted sql
        /// </summary>
        /// <param name="text">текст скрипта</param>
        /// <returns></returns>
        public static string RemoveLiquibaseTag(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";

            int start = text.IndexOf("--liquibase formatted sql");
            if (start != -1)
            {
                int finish = text.IndexOfAny(new char[] { '\r', '\n' }, start);
                if (finish > start)
                {
                    string exclude = text.Substring(start, finish - start);
                    text = text.Replace(exclude, "");
                }
            }
            return text.TrimStartNewLine();
        }

        /// <summary>
        /// Выделить в целевом файле кусок с нужным changeset и сравнить с копируемым файлом
        /// </summary>
        /// <param name="FromFile">Файл, который надо скопировать</param>
        /// <param name="ToFile">Файл в GIT, в котором ищем changeset</param>
        /// <param name="changesetName">номер changeset</param>
        /// <param name="changesetText">текст найденного changeset</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <returns>=-1 - найден, не совпадает хеш; =0 - найден, совпадает хеш; =1 - НЕ найден</returns>
        public static int FindChangeset(string FromFile, string ToFile, string changesetName, out string changesetText, string logFile)
        {
            changesetText = "";

            if (string.IsNullOrWhiteSpace(FromFile)) return 1;
            if (string.IsNullOrWhiteSpace(ToFile)) return 1;
            if (string.IsNullOrWhiteSpace(changesetName)) return 1;

            if (!File.Exists(ToFile)) return 1;

            // исходный скрипт 
            string fromText = File.ReadAllText(FromFile);

            if (string.IsNullOrWhiteSpace(fromText))
            {
                fromText = "";
            }
            else
            {
                fromText =
                    // убираем тег ликвибейз
                    RemoveLiquibaseTag(fromText)
                    // убираем начальные и конечные переводы строки
                    .TrimNewLine()
                    // убираем возврат каретки (CR)
                    .Replace(Environment.NewLine, "\n");
            }

            // целевой скрипт
            changesetText = ReadChangeset(ToFile, changesetName, logFile);

            if (string.IsNullOrWhiteSpace(changesetText))
            {
                // нужный changeset НЕ найден
                return 1;
            }
            else
            {
                // нужный changeset найден, сравниваем хеш
                if (Utilities.Files.ComputeMD5ChecksumText(fromText) == Utilities.Files.ComputeMD5ChecksumText(changesetText))
                {
                    //хеш совпадает
                    return 0;
                }
                else
                {
                    // хеш не совпадает
                    return -1;
                }
            }
        }

        /// <summary>
        /// список changeset в файле (только имя и автор)
        /// </summary>
        /// <param name="Filename">файл</param>
        /// <returns></returns>
        private static IEnumerable<SQLChangeset> ListChangesetName(string Filename)
        {
            if (string.IsNullOrWhiteSpace(Filename)) yield break;
            if (!File.Exists(Filename)) yield break;

            //if (MainWindow.APPinfo.isExtendexLog) App.AddLog($"Читаем следующий changeset из файла {Filename}", null, App.ShowMessageMode.NONE);

            string _name = "";
            string _author = "";
            string _changesetline = "";

            using (var streamReader = new StreamReader(Filename, true))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    line = line.Trim(new char[] { '\t', '\n', '\r', ' ' });

                    // Process line
                    if (line.ToLower().StartsWith("--changeset"))
                    {
                        if (!string.IsNullOrWhiteSpace(_name))
                        {
                            var changeset = new SQLChangeset(_name, _author, "", -1, -1, _changesetline);
                            yield return changeset;
                        }

                        _name = "unknown";
                        _author = "unknown";
                        _changesetline = line;

                        var arr = line.TrimInner().Split(new char[] { ' ', '\t' });
                        if (arr.Length >= 2)
                        {
                            // определяем имя changeset
                            _name = arr[1];

                            var arr2 = _name.Split(':');

                            if (arr2.Length > 1)
                            {
                                //убираем имя автора
                                _name = arr2[1];
                                _author = arr2[0];
                            }

                            if (string.IsNullOrWhiteSpace(_name))
                            {
                                _name = "unknown";
                            }
                        }
                        else
                        {
                            _name = "unknown";
                        }
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(_name))
            {
                _name = "unknown";
            }

            var result = new SQLChangeset(_name, _author, "", -1, -1, _changesetline);
            yield return result;
        }

        /// <summary>
        /// имя первого changeset в файле
        /// </summary>
        /// <param name="Filename">файл</param>
        /// <param name="changeset">вернуть changeset</param>
        /// <returns></returns>
        public static string FirstChangesetName(string Filename, out SQLChangeset changeset)
        {
            changeset = null;
            string result = "";

            if (string.IsNullOrWhiteSpace(Filename)) return result;
            if (!File.Exists(Filename)) return result;

            changeset = ListChangesetName(Filename).FirstOrDefault();

            if (
                (changeset == null) ||
                string.IsNullOrWhiteSpace(changeset.name)
            )
            {
                result = "unknown";
            }
            else
            {
                result = changeset.name;
            }

            //if (MainWindow.APPinfo.isExtendexLog) App.AddLog($"Имя первого changeset {result} в файле {Filename}   ", null, App.ShowMessageMode.NONE);

            return result;
        }

        /// <summary>
        /// Прочитать chageset из файла
        /// </summary>
        /// <param name="Filename">Файл</param>
        /// <param name="Changeset">changeset</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <returns></returns>
        public static string ReadChangeset(string Filename, string Changeset, string logFile)
        {
            if (string.IsNullOrWhiteSpace(Filename)) return "";
            if (string.IsNullOrWhiteSpace(Changeset)) return "";

            if (!File.Exists(Filename)) return "";

            foreach (var item in ReadChangeset(Filename, false, logFile))
            {
                if (item.name.ToLower() == Changeset.ToLower())
                {
                    return item.text;
                }
            }

            return "";
        }

        /// <summary>
        /// Заменить chageset в файле
        /// </summary>
        /// <param name="fileName">Файл</param>
        /// <param name="changesetName">changeset</param>
        /// <param name="changesetText">changeset</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <returns></returns>
        public static void ReSaveChangeset(string fileName, string changesetName, string changesetText, string logFile)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;
            if (string.IsNullOrWhiteSpace(changesetName)) return;

            if (!File.Exists(fileName)) return;

            foreach (var item in ReadChangeset(fileName, false, logFile))
            {
                if (item.name.ToLower() == changesetName.ToLower())
                {
                    // changeset найден

                    if (
                        (item.startLine > -1) &&
                        (item.endLine >= item.startLine)
                        )
                    {
                        // читаем файл
                        string[] lines = null;
                        try
                        {
                            lines = File.ReadAllLines(fileName);
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, logFile);
                            return;
                        }

                        string text = "";

                        // добавляем часть файла до changeset
                        if (item.startLine > 0)
                        {
                            string[] buffer = new string[item.startLine];
                            Array.Copy(lines, 0, buffer, 0, item.startLine);
                            string s = String.Join("\n", buffer) + "\n";

                            text += s;
                        }

                        // добавляем новую редакцию changeset
                        if (!string.IsNullOrWhiteSpace(changesetText))
                        {
                            // убираем начальные и конечные переводы строки
                            string s = changesetText.TrimNewLine("\n\n");

                            text += s;
                        }

                        // добавляем часть после changeset
                        long cnt = lines.LongLength - item.endLine - 1;
                        if (cnt > 0)
                        {
                            string[] buffer = new string[cnt];
                            Array.Copy(lines, item.endLine + 1, buffer, 0, cnt);
                            string s = String.Join("\n", buffer) + "\n";

                            text += s;
                        }

                        // убираем возврат каретки (CR), убираем лишние переводы строки в конце файла
                        text = text
                            .Replace(Environment.NewLine, "\n")
                            .TrimEndNewLine("\n");

                        try
                        {
                            File.WriteAllText(fileName, text);
                        }
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.NONE, true, logFile);
                            return;
                        }

                    }

                    return;
                }
            }

            return;
        }

        /// <summary>
        /// Сохранить ранее загруженный chageset в файл
        /// </summary>
        /// <param name="fileName">Файл</param>
        /// <param name="isAddLiquibase">=true - Добавить тег liquibase в начало файла</param>
        /// <param name="isAppend">=true - Дописать в существующий файл</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <returns></returns>
        public void SaveFileWithChangeset(string fileName, bool isAddLiquibase, bool isAppend, string logFile)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return;

            if (isAppend) isAddLiquibase = false;

            if (
                (this.startLine > -1) &&
                (this.endLine >= this.startLine)
                )
            {
                string text = "";
                if (isAddLiquibase) text += "--liquibase formatted sql\n";
                text += this.text;

                // убираем возврат каретки (CR), убираем лишние переводы строки в конце файла
                text = text
                    .Replace(Environment.NewLine, "\n")
                    .TrimEndNewLine("\n");

                try
                {
                    if (isAppend)
                    {
                        File.AppendAllText(fileName, "\n" + text);
                    }
                    else
                    {
                        File.WriteAllText(fileName, text);
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, logFile);
                    return;
                }

            }
            return;
        }

        /// <summary>
        /// Сохранить chageset в файл
        /// </summary>
        /// <param name="list">Список changeset</param>
        /// <param name="from">с</param>
        /// <param name="to">по</param>
        /// <param name="fileName">Файл</param>
        /// <param name="isAddLiquibase">Добавить тег liquibase</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <returns></returns>
        public static void SaveFileWithChangeset(List<SQLChangeset> list, int from, int to, string fileName, bool isAddLiquibase, string logFile)
        {
            if (list == null || list.Count == 0) return;
            if (from < 0) return;
            if (to < 0 || to < from) return;
            if (string.IsNullOrWhiteSpace(fileName)) return;

            if (to > list.Count - 1) to = list.Count - 1;

            string text = "";
            if (isAddLiquibase) text += "--liquibase formatted sql\n";

            for (int i = from; i <= to; i++)
            {
                var item = list[i];

                text += item.text + "\n" + "\n";
            }

            // убираем возврат каретки (CR), убираем лишние переводы строки в конце файла
            text = text
                .Replace(Environment.NewLine, "\n")
                .TrimEndNewLine("\n");

            try
            {
                File.WriteAllText(fileName, text);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.NONE, true, logFile);
                return;
            }

            return;
        }

        /// <summary>
        /// Прочитать все chageset из файла (или из текста)
        /// </summary>
        /// <param name="FilenameOrText">Файл</param>
        /// <param name="isText">=true - В первом параметре текст скрипта</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <returns></returns>
        public static IEnumerable<SQLChangeset> ReadChangeset(string FilenameOrText, bool isText, string logFile)
        {
            if (string.IsNullOrWhiteSpace(FilenameOrText)) yield break;

            if ((!isText) && (!File.Exists(FilenameOrText))) yield break;

            long _startLine = -1;
            long _endLine = -1;
            string _changeset = "unknown";
            string _author = "unknown";
            string[] lines = null;
            bool _hasLiquibaseFormattedSQL = false;

            if (isText)
            {
                // читаем из текста
                FilenameOrText = FilenameOrText.Replace(Environment.NewLine, "\n");
                lines = FilenameOrText
                    .ToList(new char[] { '\n' }, false)
                    .ToArray();
            }
            else
            {
                // читаем из файла
                try
                {
                    lines = File.ReadAllLines(FilenameOrText);
                }
                catch (Exception ex)
                {
                    lines = null;
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, logFile);
                }
            }

            if (
                (lines != null) &&
                (lines.LongLength > 0)
            )
            {
                for (long i = 0; i < lines.LongLength; i++)
                {
                    string line = lines[i].Trim(new char[] { '\n', '\r', '\t', ' '});
                    //string line_lower = line.ToLower();

                    if (line.StartsWith("--liquibase formatted sql"))
                    {
                        _hasLiquibaseFormattedSQL = true;
                    }

                    if (
                        (_startLine == -1) &&
                        line.StartsWith("--changeset")
                        )
                    {
                        // нашли начало первого changeset
                        _startLine = i;
                    }

                    if (
                        (_startLine > -1) &&
                        (i > _startLine) &&
                        line.StartsWith("--changeset")
                        )
                    {
                        // нашли начало следующего changeset
                        _endLine = i - 1;

                        // выделяем changeset
                        string[] dest = new string[_endLine - _startLine + 1];
                        Array.Copy(lines, _startLine, dest, 0, _endLine - _startLine + 1);

                        string Text = String.Join("\n", dest)
                            // убираем начальные и конечные переводы строки
                            .TrimNewLine()
                            // убираем возврат каретки (CR)
                            .Replace(Environment.NewLine, "\n");

                        // возвращаем changeset
                        var changeset = new SQLChangeset(_changeset, _author, Text, _startLine, _endLine, lines[_startLine]);
                        changeset.hasLiquibaseFormattedSQL = _hasLiquibaseFormattedSQL;
                        yield return changeset;

                        // сдвигаем начало changeset
                        _startLine = i;
                        _endLine = -1;
                        _hasLiquibaseFormattedSQL = false;
                    }

                    if (i == _startLine)
                    {
                        var arr = lines[i]
                            .TrimInner()
                            .Split(new char[] { ' ', '\t' });

                        if (arr.Length >= 2)
                        {
                            // определяем имя changeset
                            _changeset = arr[1];

                            var arr2 = _changeset.Split(':');

                            if (arr2.Length > 1)
                            {
                                //есть имя автора
                                _author = arr2[0];
                                _changeset = arr2[1];
                            }
                            else
                            {
                                _author = "unknown";
                            }

                            if (string.IsNullOrWhiteSpace(_changeset))
                            {
                                _author = "unknown";
                                _changeset = "unknown";
                            }
                        }
                        else
                        {
                            _author = "unknown";
                            _changeset = "unknown";
                        }
                    }
                }

                if (_startLine == -1)
                {
                    // тег changeset НЕ найден, возвращаем весь файл без тега liquibase
                    string Text =
                        RemoveLiquibaseTag(
                            String.Join("\n", lines)
                        )
                        // убираем начальные и конечные переводы строки
                        .TrimNewLine()
                        // убираем возврат каретки (CR)
                        .Replace(Environment.NewLine, "\n");

                    var changeset = new SQLChangeset(_changeset, _author, Text, -1, -1, "");
                    changeset.hasLiquibaseFormattedSQL = _hasLiquibaseFormattedSQL;
                    yield return changeset;
                    yield break;
                }
                else
                {
                    // тег changeset найден, выделяем финальный
                    if (_endLine == -1) //-V3022
                    {
                        // следующего changeset нет, берем до конца файла
                        _endLine = lines.LongLength - 1;
                    }
                    string[] dest = new string[_endLine - _startLine + 1];
                    Array.Copy(lines, _startLine, dest, 0, _endLine - _startLine + 1);

                    string Text =
                        String.Join("\n", dest)
                        // убираем начальные и конечные переводы строки
                        .TrimNewLine()
                        // убираем возврат каретки (CR)
                        .Replace(Environment.NewLine, "\n");

                    var changeset = new SQLChangeset(_changeset, _author, Text, _startLine, _endLine, lines[_startLine]);
                    changeset.hasLiquibaseFormattedSQL = _hasLiquibaseFormattedSQL;
                    yield return changeset;
                    yield break;
                }
            }

            yield return new SQLChangeset(_changeset, _author, "", -1, -1, "");
            yield break;
        }

        /// <summary>
        /// Улучшить SQL-скрипт в YML-файле: проставить метку labels (если ее нет) во всех changeset sql-файла или дополнить имя changeset номером версии, при этом максимально сохранить текущее содержимое, в т.ч. переводы строк
        /// </summary>
        /// <param name="ScriptType">тип скрипта или папка в проекте git</param>
        /// <param name="DBType">тип БД</param>
        /// <param name="Filename">Файл</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <param name="isAddVersion">=true - добавить номер версии к имени changeset</param>
        /// <param name="version_no_prefix">номер версии БЕЗ префикса</param>
        /// <returns></returns>
        public static void ImproveSQLinVersion(string ScriptType, string DBType, string Filename, string logFile, bool isAddVersion, string version_no_prefix)
        {
            if (!MainWindow.APPinfo.isImproveSQLinVersion) return;

            if (string.IsNullOrWhiteSpace(Filename)) return;

            if (!File.Exists(Filename)) return;

            string Text = "";

            // читаем из файла
            try
            {
                Text = File.ReadAllText(Filename);
                if (string.IsNullOrWhiteSpace(Text)) return;
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.NONE, true, logFile);
                return;
            }

            int _startIndex = 0;
            int _endIndexCR = 0;
            int _endIndexLF = 0;
            int _endIndex = 0;
            bool isChanged = false;

            do
            {
                // ищем начало changeset
                _startIndex = Text.IndexOf("--changeset", _startIndex);

                if (_startIndex > -1)
                {
                    // проверим, что это начало строки
                    if (
                        (_startIndex == 0) ||
                        (Text[_startIndex - 1] == '\n')
                    )
                    {
                        // ищем конец changeset
                        _endIndexCR = Text.IndexOf('\r', _startIndex);
                        _endIndexLF = Text.IndexOf('\n', _startIndex);

                        if (_endIndexCR == -1) _endIndexCR = Text.Count();
                        if (_endIndexLF == -1) _endIndexLF = Text.Count();

                        if (_endIndexCR < _endIndexLF)
                        {
                            _endIndex = _endIndexCR;
                        }
                        else
                        {
                            _endIndex = _endIndexLF;
                        }

                        if (_endIndex > _startIndex)
                        {
                            // выделяем строку changeset
                            string changeset = Text.Substring(_startIndex, _endIndex - _startIndex);

                            var Tags = YML.GetTagsFromChangeset(changeset);

                            if (!Tags.ContainsKey("labels") || isAddVersion)
                            {
                                // пересоберем строку changeset, в т.ч. добавится labels
                                changeset = YML.MakeChangeset(changeset, false, false, ScriptType, DBType, isAddVersion, version_no_prefix);

                                // заменим строку changeset
                                Text =
                                    Text.Substring(0, _startIndex) +
                                    changeset +
                                    (_endIndex >= Text.Count() ? "" : Text.Substring(_endIndex));

                                isChanged = true;
                            }
                        }

                    }

                    // сдвигаем, чтобы найти следующий changeset
                    _startIndex++;
                }
            } while (_startIndex > -1 && _startIndex < Text.Count());

            if (isChanged)
            {
                // сохраняем в файл
                try
                {
                    File.WriteAllText(Filename, Text);
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile);
                }
            }
        }
    }
}
