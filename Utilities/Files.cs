// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static SQLGen.App;

namespace SQLGen.Utilities
{
    // -------------------------------------------------------------------------------------------------------
    /// <summary>Тип разделения файла</summary>
    public enum SplitType
    {
        /// <summary>
        /// по кол-ву байт
        /// </summary>
        BYTE,
        /// <summary>
        /// по границе символа
        /// </summary>
        CHAR,
        /// <summary>
        /// по заверешнию строки
        /// </summary>
        LINE,
        /// <summary>
        /// по ключевому слову
        /// </summary>
        KEYWORDS
    }

    /// <summary>
    /// Работа с файлами
    /// </summary>
    public static class Files
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Запись скрипта с предварительной его подготовкой
        /// </summary>
        /// <param name="fullpath">полное имя файла</param>
        /// <param name="fs">поток для записи файла</param>
        /// <param name="array">массив строк string[] с текстом скрипта</param>
        /// <param name="isTrimInnerNewLine">убрать внутри текста больше 2-х подряд идущих переводов строки</param>
        /// <param name="error">ошибка</param>
        /// <param name="mode">режим открытия файла</param>
        /// <param name="isTrimOuterNewLine">убрать переводы строк в начале и в конце, добавить в конце 1 перевод строки \n</param>
        public static void WriteScript(string fullpath, FileStream fs, string[] array, bool isTrimInnerNewLine, out string error, FileMode mode, bool isTrimOuterNewLine = true)
        {
            string text = string.Join("\n", array);
            WriteScript(fullpath, fs, text, isTrimInnerNewLine, out error, mode, isTrimOuterNewLine);
        }

        /// <summary>
        /// Запись скрипта с предварительной его подготовкой
        /// </summary>
        /// <param name="fullpath">полное имя файла</param>
        /// <param name="fs">поток для записи файла</param>
        /// <param name="list">список строк List(string) с текстом скрипта</param>
        /// <param name="isTrimInnerNewLine">убрать внутри текста больше 2-х подряд идущих переводов строки</param>
        /// <param name="error">ошибка</param>
        /// <param name="mode">режим открытия файла</param>
        /// <param name="isTrimOuterNewLine">убрать переводы строк в начале и в конце, добавить в конце 1 перевод строки \n</param>
        public static void WriteScript(string fullpath, FileStream fs, List<string> list, bool isTrimInnerNewLine, out string error, FileMode mode, bool isTrimOuterNewLine = true)
        {
            string text = string.Join("\n", list.ToArray());
            WriteScript(fullpath, fs, text, isTrimInnerNewLine, out error, mode, isTrimOuterNewLine);
        }

        /// <summary>
        /// Запись скрипта с предварительной его подготовкой
        /// </summary>
        /// <param name="fullpath">полное имя файла</param>
        /// <param name="fs">поток для записи файла</param>
        /// <param name="text">текст скрипта</param>
        /// <param name="isTrimInnerNewLine">убрать внутри текста больше 2-х подряд идущих переводов строки</param>
        /// <param name="error">ошибка</param>
        /// <param name="mode">режим открытия файла</param>
        /// <param name="isRemoveCR">убрать символ возврата каретки \r</param>
        /// <param name="isTrimOuterNewLine">убрать переводы строк в начале и в конце, добавить в конце 1 перевод строки \n</param>
        public static void WriteScript(string fullpath, FileStream fs, string text, bool isTrimInnerNewLine, out string error, FileMode mode, bool isRemoveCR = true, bool isTrimOuterNewLine = true)
        {
            error = "";

            if (string.IsNullOrWhiteSpace(fullpath))
            {
                App.AddLog("Попытка записи в файл без имени", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                return;
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                text = "";
            }

            // подготовка
            if (isTrimInnerNewLine)
            {
                // убираем больше 2-х подряд идущих переводов строки
                text = text.TrimInnerNewLine();
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

            if (mode == FileMode.Append)
            {
                // если добавляем в существующий файл, добавим дополнительную пустую строку
                text = "\n" + text;
            }

            // запись
            Encoding encoding = new UTF8Encoding(false);

            if (fs != null)
            {
                using (StreamWriter file = new StreamWriter(fs, encoding))
                {
                    try
                    {
                        file.Write(text);
                    }
                    catch (Exception ex)
                    {
                        error = App.AddLog("Ошибка при записи файла " + fullpath, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile).showMessage;
                    }
                }
            }
            else
            {
                try
                {
                    File.WriteAllText(fullpath, text, encoding);
                }
                catch (Exception ex)
                {
                    error = App.AddLog("Ошибка при записи файла " + fullpath, ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile).showMessage;
                }
            }
        }

        /// <summary>
        /// Возвращает реальное имя файла
        /// </summary>
        /// <param name="fullname">полное имя файла</param>
        /// <returns></returns>
        public static string GetRealFilename(string fullname)
        {
            string path = Path.GetDirectoryName(fullname);
            string filename = Path.GetFileName(fullname);

            var files = Directory.GetFiles(path);
            if (
                (files == null) ||
                (files.Length == 0)
                )
            {
                // если в папке нет файлов - вернем исходное имя
                return fullname;
            }
            else
            {
                var found = files.Where(x => Path.GetFileName(x).ToUpper().Equals(filename.ToUpper()))
                        .First();
                if (string.IsNullOrWhiteSpace(found))
                {
                    // если файл не найден - вернем исходное имя
                    return fullname;
                }
                else
                {
                    return found;
                }
            }
        }

        /// <summary>
        /// Получаем список каталогов по маске
        /// </summary>
        /// <param name="pathmask">маска каталога</param>
        /// <returns>список существующих каталогов</returns>
        public static List<string> ListDirectoryByMask(string pathmask)
        {
            // Собираем список каталогов
            List<string> dir = new List<string>();

            if (string.IsNullOrWhiteSpace (pathmask))
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
        /// <summary>Определить максимальный номер скрипта в файлах, что лежат в каталоге задачи</summary>
        /// <param name="dir">Каталог задачи</param>
        /// <returns>список строк</returns>
        public static int MaxScriptNumber(string dir)
        {
            int res = 0;
            if (string.IsNullOrWhiteSpace(dir)) return res;

            // список файлов в каталоге
            var Files = ListFilesInDir(dir, false, true, true);

            // перебираем файлы
            foreach (var filename in Files)
            {
                // имя файла разбиваем на части
                var arr = filename.Split(' ');
                // ищем максимальный номер
                if ((arr.Length >= 3) && int.TryParse(arr[2], out int num) && (num > res)) res = num;
            }

            return res;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Создание резервной копии файла в папке path (или в том же каталоге, что и filename, если path пустой), но с расширением bak, bak1, bak2 и т.д.</summary>
        /// <param name="filename">резервируемый файл</param>
        /// <param name="backup_path">каталог для резервных копий</param>
        /// <param name="count">максимальное количество копий</param>
        public static string BackupFile(string filename, string backup_path = "", int count = 10)
        {
            string backfile = "";

            string _file = Path.GetFileName(filename);
            string _path = Path.GetDirectoryName(filename);
            if (
                !string.IsNullOrWhiteSpace(backup_path) &&
                Directory.Exists(backup_path)
            )
            {
                _path = backup_path;
            }

            if ((filename != "") && File.Exists(filename))
                try
                {
                    backfile = Path.Combine(_path, _file) + ".bak";
                    string prev = "";
                    int max = 0;

                    while (File.Exists(backfile))
                    {
                        max++;
                        backfile = Path.Combine(_path, _file) + ".bak" + max.ToString();
                        if (max == count) break;
                    };

                    for (int i = max - 1; i >= 0; i--)
                    {
                        if (i == 0) prev = Path.Combine(_path, _file) + ".bak";
                        else prev = Path.Combine(_path, _file) + ".bak" + i.ToString();

                        File.Copy(prev, backfile, true);

                        backfile = prev;
                    }

                    File.Copy(filename, backfile, true);

                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }

            return backfile;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Копирование файла</summary>
        /// <param name="path_from">каталог, откуда копируем файл</param>
        /// <param name="file_from">файл, который копируем</param>
        /// <param name="path_to">каталог, куда копируем файл</param>
        /// <param name="file_to">новое имя файла</param>
        /// <param name="overwrite">true - перезаписать существующий файл</param>
        /// <param name="copytimeattr">true - сохранить атрибуты времени (создания и последней записи) файла-источника</param>
        public static void CopyFile(string path_from, string file_from, string path_to, string file_to, bool overwrite, bool copytimeattr)
        {
            Directory.CreateDirectory(path_to);

            file_from = Path.Combine(path_from, file_from);
            file_to = Path.Combine(path_to, file_to);

            File.Copy(file_from, file_to, overwrite);

            if (!copytimeattr)
            {

                File.SetLastWriteTime(file_to, DateTime.Now);
                File.SetCreationTime(file_to, DateTime.Now);

            }

        }

        /// <summary>
        /// Определение кодировки файла
        /// </summary>
        /// <param name="filename">Файл</param>
        /// <param name="isBOM">=true есть BOM</param>
        /// <returns>The detected encoding or "unknown".</returns>
        public static string GetEncoding(string filename, out bool isBOM)
        {
            isBOM = false;

            var encodingByBOM = GetEncodingByBOM(filename);
            if (encodingByBOM != "unknown")
            {
                isBOM = true;
                return encodingByBOM;
            }

            // BOM not found :(, so try to parse characters into several encodings

            // UTF8
            var encodingByParsingUTF8 = GetEncodingByParsing(filename, Encoding.UTF8);
            if ((encodingByParsingUTF8 != null) && (encodingByParsingUTF8.ToString() == Encoding.UTF8.ToString()))
                return "UTF8";

            // 1251 ANSI-кириллица
            var encodingByParsing1251 = GetEncodingByParsing(filename, Encoding.GetEncoding("windows-1251"));
            if ((encodingByParsing1251 != null) && (encodingByParsing1251.ToString() == Encoding.GetEncoding("windows-1251").ToString()))
                return "1251";

            /*
            var encodingByParsingLatin1 = GetEncodingByParsing(filename, Encoding.GetEncoding("iso-8859-1"));
            if ((encodingByParsingLatin1 != null) && (encodingByParsingLatin1.ToString() == Encoding.GetEncoding("iso-8859-1").ToString()))
                return "iso-8859-1";

            var encodingByParsingUTF7 = GetEncodingByParsing(filename, Encoding.UTF7);
            if (encodingByParsingUTF7 != null)
                return "UTF7";
            */

            return "unknown";   // no encoding found
        }

        private static string GetEncodingByBOM(string filename)
        {
            // Read the BOM
            var byteOrderMark = new byte[4];
            using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                file.Read(byteOrderMark, 0, 4);
            }

            // Analyze the BOM
            if (byteOrderMark[0] == 0x2b && byteOrderMark[1] == 0x2f && byteOrderMark[2] == 0x76) return "UTF7";
            if (byteOrderMark[0] == 0xef && byteOrderMark[1] == 0xbb && byteOrderMark[2] == 0xbf) return "UTF8";
            if (byteOrderMark[0] == 0xff && byteOrderMark[1] == 0xfe) return "UTF-16LE";
            if (byteOrderMark[0] == 0xfe && byteOrderMark[1] == 0xff) return "UTF-16LE";
            if (byteOrderMark[0] == 0 && byteOrderMark[1] == 0 && byteOrderMark[2] == 0xfe && byteOrderMark[3] == 0xff) return "UTF32";

            return "unknown";    // no BOM found
        }

        private static Encoding GetEncodingByParsing(string filename, Encoding encoding)
        {
            var encodingVerifier = Encoding.GetEncoding(encoding.BodyName, new EncoderExceptionFallback(), new DecoderExceptionFallback());

            try
            {
                using (var textReader = new StreamReader(filename, encodingVerifier, detectEncodingFromByteOrderMarks: true))
                {
                    while (!textReader.EndOfStream)
                    {
                        textReader.ReadLine();   // in order to increment the stream position
                    }

                    // all text parsed ok
                    return textReader.CurrentEncoding;
                }
            }
            catch { }

            return null;    // 
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Смена кодировки на UTF8</summary>
        /// <param name="fullFilename">полный путь к проверяемому файлу</param>
        /// <param name="Errors">список обнаруженных ошибок</param>
        /// <returns>true - есть ошибки</returns>
        public static bool SetEncodingToUTF8(ref string fullFilename, ref string Errors) //-V3203
        {
            if ((fullFilename == null) || (fullFilename == "")) return false;

            if (Errors == null) Errors = "";

            bool res = false;

            string[] buffer = null;

            var encodingByParsing1251 = GetEncodingByParsing(fullFilename, Encoding.GetEncoding("windows-1251"));
            if ((encodingByParsing1251 != null) && (encodingByParsing1251.ToString() == Encoding.GetEncoding("windows-1251").ToString()))
            {
                // считываем  файл
                buffer = File.ReadAllLines(fullFilename, Encoding.GetEncoding("windows-1251"));
            }

            var encodingByParsingUTF8 = GetEncodingByParsing(fullFilename, Encoding.UTF8);
            if ((encodingByParsingUTF8 != null) && (encodingByParsingUTF8.ToString() == Encoding.UTF8.ToString()))
            {
                // считываем  файл
                buffer = File.ReadAllLines(fullFilename, Encoding.UTF8);
            }

            string backupfileSQL = "";

            if ((buffer != null) && (buffer.Length > 0))
                try
                {
                    // делаем архивную копию
                    backupfileSQL = BackupFile(fullFilename);

                    // перезаписываем файл в кодировке UTF8 без BOM
                    Encoding encodingUTF8 = new UTF8Encoding(false);
                    File.WriteAllLines(fullFilename, buffer, encodingUTF8);
                    res = true;

                    // удаляем архивную копию
                    if (backupfileSQL != "")
                    {
                        try { File.Delete(backupfileSQL); } catch { }
                        backupfileSQL = "";
                    }

                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                    res = false;
                }

            if (res)
            {
                string info = "В файле " + fullFilename + " кодировка изменена на UTF8";
                Errors += Environment.NewLine + info + Environment.NewLine;
            }
            else
            {
                string info = "Не удалось исправить кодировку файла " + fullFilename + " !";
                if (backupfileSQL != "") info = info + "\nОсталась архивная копия - " + backupfileSQL;
                Errors += Environment.NewLine + info + Environment.NewLine;
            }

            return res;
        }

        /// <summary>Подсчет хеш-суммы файла</summary>
        /// <param name="file">Путь к файлу</param>
        public static string ComputeMD5ChecksumFile(string file)
        {
            using (FileStream fs = System.IO.File.OpenRead(file))
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] checkSum = md5.ComputeHash(fs);
                string result = BitConverter.ToString(checkSum)
                    .Replace("-", String.Empty)
                    .ToLower();
                return result;
            }
        }

        /// <summary>Подсчет хеш-суммы строки</summary>
        /// <param name="text">Строка</param>
        public static string ComputeMD5ChecksumText(string text)
        {
            using (MD5 md5 = new MD5CryptoServiceProvider())
            {
                byte[] checkSum = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                string result = BitConverter.ToString(checkSum)
                    .Replace("-", String.Empty)
                    .ToLower();
                return result;
            }
        }

        /// <summary>
        /// Поиск в файлах
        /// </summary>
        /// <param name="folder">папка с файлами</param>
        /// <param name="search">искомая фраза</param>
        /// <returns>список найденных файлов с искомой фразой</returns>
        public static List<string> WindowsSearchInDir(string folder, string search)
        {
            List<string> files = new List<string>();

            string connectionString = "Provider=Search.CollatorDSO.1;Extended Properties=\"Application=Windows\"";
            OleDbConnection connection = new OleDbConnection(connectionString);

            string query = @"SELECT Top 1 System.ItemUrl FROM SystemIndex " +
               //@"WHERE scope ='file:" + folder + "' and CONTAINS('" + search + "')";
               @"WHERE scope ='file:E:\'";
            OleDbCommand command = new OleDbCommand(query, connection);
            connection.Open();

            OleDbDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                string file = reader.GetString(0).Replace("file:", "");
                files.Add(file);
            }

            connection.Close();

            return files;
        }

        /// <summary>
        /// Обрезать файл, оставив с конца файла maxSize_mb мегабайт
        /// </summary>
        /// <param name="fileName">путь к файлу</param>
        /// <param name="maxSize_mb">предельный размер в Мб. По умолчанию - 10 Мб</param>
        public static void CutEndFileMaxSize(string fileName, long maxSize_mb = 10)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return;
            }

            if (maxSize_mb == 0)
            {
                return;
            }

            if (File.Exists(fileName))
            {
                Encoding encoding = new UTF8Encoding(false);

                // max file size in byte
                long maxsize = maxSize_mb * 1024 * 1024;

                try
                {
                    // контроль размера лог-файла
                    FileInfo txtfile = new FileInfo(fileName);
                    if (txtfile.Length > maxsize)
                    {
                        var lines = File.ReadAllLines(fileName);
                        long size = 0;
                        for (int i = lines.Count() - 1; i >= 0; i--)
                        {
                            size += encoding.GetByteCount(lines[i]);
                            if (size >= maxsize)
                            {
                                lines = lines.Skip(i).ToArray();
                                File.WriteAllLines(fileName, lines);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex) 
                {
                    App.AddLog("", ex, ShowMessageMode.SHOW, false, null);
                }
            }
        }
    }
}
