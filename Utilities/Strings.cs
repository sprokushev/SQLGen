// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SQLGen.Utilities
{
    /// <summary>
    /// Работа со строками
    /// </summary>
    public class Strings
    {
        /// <summary>
        /// Преобразовать список строк в одну строку
        /// </summary>
        /// <param name="list">список строк</param>
        /// <param name="divider">разделитель</param>
        /// <param name="quotes">заключать в этот символ каждый элемент</param>
        /// <returns></returns>
        public static string ListToString(List<string> list, string divider, string quotes = "")
        {
            if (list == null || list.Count == 0) return "";

            if (string.IsNullOrEmpty(quotes))
            {
                quotes = "";
            }

            if (string.IsNullOrEmpty(divider))
            {
                divider = "";
            }

            return string.Join(divider, list.Select(x => quotes + x + quotes).ToArray()).Trim();
        } 
    }

    /// <summary>
    /// расширение для String
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Обрезать text до количества символов maxLength
        /// </summary>
        /// <param name="text">текст</param>
        /// <param name="maxLength">размер</param>
        /// <returns></returns>
        public static string Truncate(this string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;
            else return text.Length <= maxLength ? text : text.Substring(0, maxLength);
        }

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

        /// <summary>
        /// удаляем [[:space:]] - пробелы, табуляции, переводы строк - в начале text
        /// </summary>
        /// <param name="text">текст</param>
        /// <returns></returns>
        public static string TrimStartAllSpace(this string text)
        {
            if (text == null) return null;
            else if (string.IsNullOrWhiteSpace(text)) return "";
            else return text.TrimStart(new char[] { '\n', '\r', '\t', '\v', '\f', ' ' });
        }

        /// <summary>
        /// удаляем [[:space:]] - пробелы, табуляции, переводы строк - в конце text и добавляем postfix в конце
        /// </summary>
        /// <param name="text">текст</param>
        /// <param name="postfix">добавить в конец текста</param>
        /// <returns></returns>
        public static string TrimEndAllSpace(this string text, string postfix = "")
        {
            if (text == null) return null;
            else if (string.IsNullOrWhiteSpace(text)) return "" + postfix;
            else return text.TrimEnd(new char[] { '\n', '\r', '\t', '\v', '\f', ' ' }) + postfix;
        }

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

        /// <summary>
        /// убираем все переводы строки и возвраты коретки в начале text
        /// </summary>
        /// <param name="text">текст</param>
        /// <returns></returns>
        public static string TrimStartNewLine(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            else return text.TrimStart(new char[] { '\n', '\r' });
        }

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

        /// <summary>
        /// убрать больше count_nl подряд переводов строки внутри text
        /// </summary>
        /// <param name="text">текст</param>
        /// <param name="count_nl">кол-во переводов строки подряд</param>
        /// <returns></returns>
        public static string TrimInnerNewLine(this string text, int count_nl = 2)
        {
            if (string.IsNullOrEmpty(text)) return text;

            while (text.Contains(Environment.NewLine + Environment.NewLine + Environment.NewLine))
            {
                text = text.Replace(Environment.NewLine + Environment.NewLine + Environment.NewLine, Environment.NewLine + Environment.NewLine);
            }

            while (text.Contains("\r\r\r"))
            {
                text = text.Replace("\r\r\r", "\r\r");
            }

            while (text.Contains("\n\n\n"))
            {
                text = text.Replace("\n\n\n", "\n\n");
            }

            if (count_nl == 1)
            {
                while (text.Contains(Environment.NewLine + Environment.NewLine))
                {
                    text = text.Replace(Environment.NewLine + Environment.NewLine, Environment.NewLine);
                }

                while (text.Contains("\r\r"))
                {
                    text = text.Replace("\r\r", "\r");
                }

                while (text.Contains("\n\n"))
                {
                    text = text.Replace("\n\n", "\n");
                }
            }

            return text;
        }

        /// <summary>
        /// убрать больше 1-го подряд пробела или tab внутри text
        /// </summary>
        /// <param name="text">текст</param>
        /// <returns></returns>
        public static string TrimInner(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            while (text.Contains("\t\t"))
            {
                text = text.Replace("\t\t", "\t");
            }

            while (text.Contains(" \t"))
            {
                text = text.Replace(" \t", " ");
            }

            while (text.Contains("\t "))
            {
                text = text.Replace("\t ", "\t");
            }

            return text;
        }

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

        /// <summary>Проверяет, что текст text соответствует маске mask</summary>
        /// <param name="text">текст</param>
        /// <param name="mask">маска</param>
        public static bool IsMatch(this string text, string mask)
        {
            if (
                string.IsNullOrWhiteSpace(text) &&
                string.IsNullOrWhiteSpace(mask)
            )
            {
                return true;
            }

            if (
                string.IsNullOrWhiteSpace(text) ||
                string.IsNullOrWhiteSpace(mask)
            )
            {
                return false;
            }

            try
            {
                Regex reg = new Regex(mask);
                return reg.IsMatch(text);
            }
            catch
            {
            }

            return false;
        }

        /// <summary>Форматировать текст text</summary>
        /// <param name="text">текст</param>
        /// <param name="separator">разделитель</param>
        /// <param name="prefix">префикс</param>
        public static string FormatList(this string text, string separator, string prefix)
        {
            if (text == null) return null;
            if (prefix == null) prefix = "";
            if (separator == null) return prefix + text;

            var arr = text.Split(new string[] { separator }, StringSplitOptions.None);

            string result = "";

            for (int i = 0; i < arr.Length; i++)
            {
                result += prefix + arr[i].Trim();
                if (i != arr.Length - 1) result += separator + Environment.NewLine;
            }

            return result;
        }

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
    }
}
