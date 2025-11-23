// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using AngleSharp.Text;
using SQLGen.Utilities;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>Идентификация ключевых слов</summary>
    public class KeyWord
    {
        /// <summary>
        /// Метод проверки
        /// </summary>
        /// <param name="line">строка</param>
        /// <returns></returns>
        private bool Contains(string line)
        {
            if ((Words != null) && (!string.IsNullOrWhiteSpace(line)))
            {
                for (int i = 0; i < WordsLength; i++)
                {
                    if (!string.IsNullOrWhiteSpace(Words[i]))
                    {
                        int start = line.IndexOf(Words[i]);
                        if (start != -1)
                        {
                            // слово найдено, попробуем проверить символы до и после

                            int next = start + Words[i].Length;
                            int prev = start - 1;

                            if (
                                (
                                    // если после - конец строки или любой символ кроме 0..9a..zA..Z
                                    (line.Length == next) ||
                                    (
                                        (line.Length > next) &&
                                        (!line[next].IsAlphanumericAscii())
                                    )
                                ) &&
                                (
                                    // если до - начало строки или любой символ кроме 0..9a..zA..Z
                                    (prev < 0) ||
                                    (!line[prev].IsAlphanumericAscii())
                                )
                            )
                            {
                                return true;
                            }
                            else
                            {
                                // слово в составе другого слова
                                return false;
                            }
                        }
                        else
                        {
                            // слово не найдено
                            return false;
                        }
                    }
                }
            }
            if ((Regexes != null) && (!string.IsNullOrWhiteSpace(line)))
            {
                for (int i = 0; i < RegexesLength; i++)
                {
                    if ((Regexes[i] != null) && Regexes[i].IsMatch(line)) return true;
                }
            }
            return false;
        }

        /// <summary>Сброс списка найденных слов перед началом очередной проверки</summary>
        private void InitRows()
        {
            Rows = new SortedDictionary<int, string>();
        }


        /// <summary>
        /// Конструктор для ключевого слова
        /// </summary>
        /// <param name="words">список слов</param>
        /// <param name="regexes">список регулярных выражений</param>
        /// <param name="check_in_comment">=true - искать ключевое слово в комментариях</param>
        public KeyWord(string[] words, string[] regexes, bool check_in_comment)
        {
            InitRows();

            Words = null;
            Regexes = null;
            WordsLength = 0;
            RegexesLength = 0;
            CheckWithComment = check_in_comment;

            if ((words != null) && (words.Length > 0))
            {
                Words = new string[words.Length];

                foreach (var item in words)
                {
                    string word = item;

                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        WordsLength++;
                        Words[WordsLength - 1] = word.ToLower().Trim();
                    }
                }
            }

            if ((regexes != null) && (regexes.Length > 0))
            {
                Regexes = new Regex[regexes.Length];

                foreach (var item in regexes)
                {
                    string word = item;

                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        RegexesLength++;
                        Regexes[RegexesLength - 1] = new Regex(word);
                    }
                }
            }
        }

        /// <summary>Флаг для выделения ключевых слов, которые надо проверять в комментариях</summary>
        public bool CheckWithComment { get; set; }

        /// <summary>Общее кол-во шаблонов ключевых слов и регулярных выражений</summary>
        public int Length
        {
            get
            {
                return WordsLength + RegexesLength;
            }
        }
        /// <summary>Кол-во шаблонов ключевых слов</summary>
        private int WordsLength { get; set; }

        /// <summary>Кол-во шаблонов регулярных выражений</summary>
        private int RegexesLength { get; set; }

        /// <summary>список ключевых слов</summary>
        private string[] Words;

        /// <summary>списк регулярных выражений</summary>
        private Regex[] Regexes;

        /// <summary>Номера строк, где это слово или регулярное выражение найдены</summary>
        public SortedDictionary<int, string> Rows { get; set; }

        /// <summary>
        /// Проверка строки на наличие ключевого слова
        /// </summary>
        /// <param name="NumRow">номер строки</param>
        /// <param name="line">строка</param>
        public void Check(int NumRow, string line)
        {
            if (Contains(line))
            {
                Rows.Add(NumRow, line);
            }
        }

        /// <summary>=true ключевое слово найдено в файле</summary>
        public bool isFound
        {
            get
            {
                return Rows.Count > 0;
            }
        }

        /// <summary>Кол-во строк в файле, где найдены ключевые слова</summary>
        public int CountRows
        {
            get
            {
                return Rows.Count;
            }
        }

        /// <summary>Первая строка, где найдено слово</summary>
        public KeyValuePair<int, string> MinRow
        {
            get
            {
                return Rows.FirstOrDefault();
            }
        }

        /// <summary>Последняя строка, где найдено слово</summary>
        public KeyValuePair<int, string> MaxRow
        {
            get
            {
                return Rows.LastOrDefault();
            }
        }

        // =================================================================================================================
        /// <summary>
        /// Значение пары ключ:значение
        /// </summary>
        /// <param name="keyvaluepair">пара</param>
        /// <param name="key">ключ</param>
        /// <param name="separator">разделитель (кроме пробел, таб, двойная кавычка)</param>
        /// <returns></returns>
        public static string KeyValue(string keyvaluepair, string key, char[] separator)
        {
            string line = keyvaluepair.Trim(new[] { ' ', '\t' });
            var arr = line.Split(separator);
            if (arr.Length > 1)
            {
                string arr_key = arr[0].Trim(new[] { ' ', '\t', '\"' });
                if (arr_key.ToLower() == key.ToLower())
                {
                    return arr[1].Trim(new[] { ' ', '\t', '\"' });
                }
            }

            return "";
        }


        /// <summary>Словарь ключевых слов</summary>
        public static Dictionary<string, KeyWord> ListKeyWords = new Dictionary<string, KeyWord>();

        /// <summary>Заполнение словаря ключевых слов</summary>
        public static void FillKeyWords()
        {
            ListKeyWords.Clear();

            // ключевые слова, которые не могут быть в комментариях
            ListKeyWords.Add("INSERT", new KeyWord(null, new string[] { @"insert(\s+)into" }, false));
            ListKeyWords.Add("ON_CONFLICT", new KeyWord(null, new string[] { @"on(\s+)conflict" }, false));
            ListKeyWords.Add("COMMENT", new KeyWord(new string[] { "ms_description" }, new string[] { @"comment(\s+)on" }, false));
            ListKeyWords.Add("DROP", new KeyWord(null, new string[] { @"(\s*)drop(\s+)(proc|procedure|func|function|view)" }, false));
            ListKeyWords.Add("CREATE", new KeyWord(null, new string[] { @"(\s*)create(\s+)table" }, false));
            ListKeyWords.Add("CREATE_NOT_EXISTS", new KeyWord(null, new string[] { @"(\s*)create(\s+)table(\s+)if(\s+)not(\s+)exists" }, false));
            ListKeyWords.Add("CREATE_STORED", new KeyWord(null, new string[] { @"(\s*)create(\s+)(proc|procedure|func|function|view)" }, false));
            ListKeyWords.Add("SET_IDENTITY_ON", new KeyWord(null, new string[] { @"(\s*)set(\s+)identity_insert(\s+)(\S*)(\s+)on" }, false));
            ListKeyWords.Add("SET_IDENTITY_OFF", new KeyWord(null, new string[] { @"(\s*)set(\s+)identity_insert(\s+)(\S*)(\s+)off" }, false));
            ListKeyWords.Add("DROP_COLUMN", new KeyWord(null, new string[] { @"drop(\s+)column" }, false));
            ListKeyWords.Add("RENAME_COLUMN", new KeyWord(null, new string[] { @"rename(\s+)column" }, false));
            ListKeyWords.Add("SET", new KeyWord(null, new string[] { @"\A([\s|;]*)set([\s|;]*)\Z",
                                                                                @"\A([\s|;]*)set(\s+)",
                                                                                @"([\s|;]+)set([\s|;]*)\Z",
                                                                                @"([\s|;]+)set(\s+)" }, false));
            ListKeyWords.Add("GET_REGION", new KeyWord(null, new string[] { @"(\s*)if(.*)getregion(\s*)\(" }, false));
            ListKeyWords.Add("GET_REGION_RELEASE", new KeyWord(null, new string[] { @"(\s*)if(.*)getregion(\s*)\((.*)release" }, false));
            ListKeyWords.Add("SET_DATE", new KeyWord(null, new string[] { @"(\s*)set(\s+)@(.*)=(.*)getdate(\s*)\(",
                                                                                @"(\s*)declare(\s+)@(.*)=(.*)getdate(\s*)\(",
                                                                                @"(\s*)@(.*)=(.*)getdate(\s*)\(",
                                                                                @"(.*)=(\s*)now(\s*)\(",
                                                                                @"(.*)=(\s*)localtimestamp"}, false));
            ListKeyWords.Add("SET_REGION", new KeyWord(null, new string[] { @"(.*)=(.*)getregion(\s*)\(" }, false));
            ListKeyWords.Add("GO", new KeyWord(null, new string[] { @"\A([\s|;]*)go([\s|;]*)\Z",
                                                                                @"\A([\s|;]*)go([\s|;]+)",
                                                                                @"([\s|;]+)go([\s|;]*)\Z",
                                                                                @"([\s|;]+)go([\s|;]+)" }, false));
            ListKeyWords.Add("xp_gen_view", new KeyWord(new string[] { "xp_gen_view" }, null, false));
            ListKeyWords.Add("xp_dropfns", new KeyWord(new string[] { "xp_dropfns" }, null, false));
            ListKeyWords.Add("xp_genidentity", new KeyWord(new string[] { "xp_genidentity" }, null, false));
            ListKeyWords.Add("sp_rename", new KeyWord(new string[] { "sp_rename" }, null, false));


            // ключевые слова, которые могут быть в комментариях
            ListKeyWords.Add("--liquibase formatted sql", new KeyWord(new string[] { "--liquibase formatted sql" }, null, true));
            ListKeyWords.Add("--changeset", new KeyWord(new string[] { "--changeset" }, null, true));
            ListKeyWords.Add("stripcomments:false", new KeyWord(new string[] { "stripcomments:false" }, null, true));
            ListKeyWords.Add("dbms:mssql", new KeyWord(new string[] { "dbms:mssql" }, null, true));
            ListKeyWords.Add("dbms:postgresql", new KeyWord(new string[] { "dbms:postgresql" }, null, true));
            ListKeyWords.Add("enddelimiter:go", new KeyWord(new string[] { "enddelimiter:go" }, null, true));
            ListKeyWords.Add("enddelimiter:;;", new KeyWord(new string[] { "enddelimiter:;;" }, null, true));
            ListKeyWords.Add("autogen", new KeyWord(new string[] { "autogen" }, null, true));

            // проверяем метки
            if (MainWindow.APPinfo.isImproveSQLinVersion)
            {
                ListKeyWords.Add("labels:struct", new KeyWord(new string[] { "labels:struct" }, null, true));
                ListKeyWords.Add("labels:code", new KeyWord(new string[] { "labels:code" }, null, true));
                ListKeyWords.Add("labels:data", new KeyWord(new string[] { "labels:data" }, null, true));
                ListKeyWords.Add("labels:finish", new KeyWord(new string[] { "labels:finish" }, null, true));
            }
        }

        /// <summary>Инициализация словаря ключевых слов перед проверкой</summary>
        public static void InitKeyWords()
        {
            foreach (var item in ListKeyWords)
            {
                if (item.Value != null) item.Value.InitRows();
            }
        }

        /// <summary>
        /// Ключевое слово
        /// </summary>
        /// <param name="key">ключевое слово</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static KeyWord GetKeyWord(string key)
        {
            if (string.IsNullOrEmpty(key)) key = "";

            foreach (var item in ListKeyWords)
            {
                if ((item.Key != null) && (item.Key == key) && (item.Value != null)) return item.Value;
            }

            // сюда не должен дойти!
            throw new ArgumentException(String.Format("{0} ключевое слово не найдено", key), "key");
        }
    }
}
