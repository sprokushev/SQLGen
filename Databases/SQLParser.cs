// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
using System;
using System.Collections.Generic;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using SQLGen.Utilities;

namespace SQLGen.Databases
{
    /// <summary>
    /// типы лексических единиц
    /// </summary>
    public enum SQLPhraseType
    {
        /// <summary>
        /// комментарий
        /// </summary>
        COMMENT,
        /// <summary>
        /// строковый литерал в кавычках
        /// </summary>
        LITERAL,
        /// <summary>
        /// Выражение
        /// </summary>
        EXPRESSION,
        /// <summary>
        /// оператор OR или AND
        /// </summary>
        OPERATOR,
        /// <summary>
        /// левая скобка
        /// </summary>
        LEFTBRACKET,
        /// <summary>
        /// правая скобка
        /// </summary>
        RIGHTBRACKET
    }

    /// <summary>Класс для получения дерева фраз в условии типа Where</summary>
    public class SQLPhraseWhere
    {
        /// <summary>
        /// логические операторы
        /// </summary>
        private static List<string> listOperator = new List<string>() { "AND", "OR" };

        /// <summary>
        /// родительская фраза
        /// </summary>
        public SQLPhraseWhere RootPhrase;
        /// <summary>
        /// предыдущая фраза
        /// </summary>
        private SQLPhraseWhere _prevPhrase;
        /// <summary>
        /// тип фразы
        /// </summary>
        public SQLPhraseType Type;
        /// <summary>
        /// значение фразы
        /// </summary>
        public string Value;
        /// <summary>
        /// следующая фраза
        /// </summary>
        private SQLPhraseWhere _nextPhrase;
        /// <summary>
        /// номер фразы по порядку (могут быть отрицательные значения)
        /// </summary>
        private long _orderPhrase;
        /// <summary>
        /// уровень фразы в дереве
        /// </summary>
        public long Level;
        /// <summary>
        /// текст
        /// </summary>
        public string Text;

        /// <summary>
        /// конструктор фразы
        /// </summary>
        /// <param name="_sqlText">текст с условиями типа Where</param>
        /// <param name="_root">родительская фразы</param>
        /// <param name="_order">номер фразы</param>
        /// <param name="_level">уровень фразы в дереве</param>
        public SQLPhraseWhere(string _sqlText, SQLPhraseWhere _root, long _order, long _level)
        {
            this.RootPhrase = _root;
            this._prevPhrase = null;
            this.Type = SQLPhraseType.EXPRESSION;
            this.Value = "";
            this._nextPhrase = null;
            this._orderPhrase = _order;
            this.Level = _level;

            if (string.IsNullOrWhiteSpace(_sqlText))
            {
                _sqlText = "";
            }

            // заменим \t, \n, \r на пробел, уберем лишние пробелы
            this.Text = _sqlText
                .Replace('\t', ' ')
                .Replace('\n', ' ')
                .Replace('\r', ' ')
                .TrimInner()
                .Trim();

            int ii;

            // -------------------------------------------------------------------------------
            // выделим комментарий

            // словарь символов комментария, найденных в строке
            SortedDictionary<int, string> dict = new SortedDictionary<int, string>();

            // заполняем словарь
            ii = 0;
            while (ii > -1)
            {
                ii = this.Text.IndexOf("/*", ii);
                if (ii > -1)
                {
                    dict.Add(ii, "/*");
                    ii++;
                }
            }
            ii = 0;
            while (ii > -1)
            {
                ii = this.Text.IndexOf("*/", ii);
                if (ii > -1)
                {
                    dict.Add(ii, "*/");
                    ii++;
                }
            }
            ii = 0;
            while (ii > -1)
            {
                ii = this.Text.IndexOf("--", ii);
                if (ii > -1)
                {
                    dict.Add(ii, "--");
                    ii++;
                }
            }

            // анализируем наличие в строке символов комментария
            if (dict.Count > 0)
            {
                // первый символ с начала строки
                var first = dict.First();

                if (first.Value == "--")
                {
                    // если первый комментарий начинается с --, в текущей фразе оставляем этот комментарий (до конца строки), а остальную часть строки оформляем как предыдущую фразу (до)
                    this.Type = SQLPhraseType.COMMENT;
                    this.Value = this.Text.Substring(first.Key).Trim();
                    if (first.Key > 0)
                    {
                        this._prevPhrase = new SQLPhraseWhere(this.Text.Substring(0, first.Key), this, -1, Level + 1);
                    }
                    this._nextPhrase = null;
                    return;
                }
                else if (first.Value == "/*")
                {
                    // если первый комментарий начинается с /*, ищем в этой же строке символ */
                    var next = dict.Where(x => (x.Key > first.Key) && (x.Value == "*/")).FirstOrDefault();
                    if (!next.Equals(default(KeyValuePair<int, string>)))
                    {
                        // если комментарий начинаяется и завершается в этой же строке - в текущей фразе оставляем этот комментарий, а остальную часть строки оформляем как предыдущую фразу (до) и последующую фразу (после)
                        this.Type = SQLPhraseType.COMMENT;
                        this.Value = this.Text.Substring(first.Key, next.Key + 2 - first.Key).Trim();
                        if (first.Key > 0)
                        {
                            this._prevPhrase = new SQLPhraseWhere(this.Text.Substring(0, first.Key), this, -1, Level + 1);
                        }
                        if (next.Key + 2 < this.Text.Length)
                        {
                            this._nextPhrase = new SQLPhraseWhere(this.Text.Substring(next.Key + 2), this, +1, Level + 1);
                        }
                        return;
                    }
                    else
                    {
                        // если комментарий начинается /*, но не завершается */ в этой же строке - в текущей фразе оставляем этот комментарий, а остальную часть строки оформляем как предыдущую фразу (до)
                        this.Type = SQLPhraseType.COMMENT;
                        this.Value = this.Text.Substring(first.Key).Trim();
                        if (first.Key > 0)
                        {
                            this._prevPhrase = new SQLPhraseWhere(this.Text.Substring(0, first.Key), this, -1, Level + 1);
                        }
                        this._nextPhrase = null;
                        return;
                    }
                }
                else if (first.Value == "*/")
                {
                    // если первым идет символ */ -  в текущей фразе оставляем этот комментарий, а остальную часть строки оформляем как следующую фразу (после)
                    this.Type = SQLPhraseType.COMMENT;
                    this.Value = this.Text.Substring(0, first.Key + 2).Trim();
                    this._prevPhrase = null;
                    if (first.Key + 2 < this.Text.Length)
                    {
                        this._nextPhrase = new SQLPhraseWhere(this.Text.Substring(first.Key + 2), this, +1, Level + 1);
                    }
                    return;
                }
            }

            // -------------------------------------------------------------------------------
            // выделяем строковые литералы в кавычках
            int _first = 0;
            _first = this.Text.IndexOf("'", _first);
            if (_first > -1)
            {
                int _next = this.Text.IndexOf("'", _first);
                if (_next > -1)
                {
                    // выделяем строковый литерал, а остальную часть строки оформляем как предыдущую фразу (до) и последующую фразу (после)
                    this.Type = SQLPhraseType.LITERAL;
                    this.Value = this.Text.Substring(_first, _next + 1 - _first).Trim();
                    if (_first > 0)
                    {
                        this._prevPhrase = new SQLPhraseWhere(this.Text.Substring(0, _first), this, -1, Level + 1);
                    }
                    if (_next + 1 < this.Text.Length)
                    {
                        this._nextPhrase = new SQLPhraseWhere(this.Text.Substring(_next + 1), this, +1, Level + 1);
                    }
                    return;
                }
            }

            // -------------------------------------------------------------------------------
            // находим левую скобку
            _first = 0;
            _first = this.Text.IndexOf("(", _first);
            if (_first > -1)
            {
                // выделяем скобку, а остальную часть строки оформляем как предыдущую фразу (до) и последующую фразу (после)
                this.Type = SQLPhraseType.LEFTBRACKET;
                this.Value = this.Text.Substring(_first, 1).Trim();
                if (_first > 0)
                {
                    this._prevPhrase = new SQLPhraseWhere(this.Text.Substring(0, _first), this, -1, Level + 1);
                }
                if (_first + 1 < this.Text.Length)
                {
                    this._nextPhrase = new SQLPhraseWhere(this.Text.Substring(_first + 1), this, +1, Level + 1);
                }
                return;
            }

            // -------------------------------------------------------------------------------
            // находим правую скобку
            _first = 0;
            _first = this.Text.IndexOf(")", _first);
            if (_first > -1)
            {
                // выделяем скобку, а остальную часть строки оформляем как предыдущую фразу (до) и последующую фразу (после)
                this.Type = SQLPhraseType.RIGHTBRACKET;
                this.Value = this.Text.Substring(_first, 1).Trim();
                if (_first > 0)
                {
                    this._prevPhrase = new SQLPhraseWhere(this.Text.Substring(0, _first), this, -1, Level + 1);
                }
                if (_first + 1 < this.Text.Length)
                {
                    this._nextPhrase = new SQLPhraseWhere(this.Text.Substring(_first + 1), this, +1, Level + 1);
                }
                return;
            }

            // -------------------------------------------------------------------------------
            // находим логические операторы
            foreach (var oper in listOperator)
            {
                if (this.Text.ToUpper() == oper.ToUpper())
                {
                    // остался только оператор
                    this.Type = SQLPhraseType.OPERATOR;
                    this.Value = oper.Trim();
                    this._prevPhrase = null;
                    this._nextPhrase = null;
                    return;
                }

                if (this.Text.StartsWith(oper + " ", StringComparison.OrdinalIgnoreCase))
                {
                    // строка начинаяется с оператора
                    this.Type = SQLPhraseType.OPERATOR;
                    this.Value = this.Text.Substring(0, oper.Length).Trim();
                    this._prevPhrase = null;
                    if (oper.Length + 1 < this.Text.Length)
                    {
                        this._nextPhrase = new SQLPhraseWhere(this.Text.Substring(oper.Length + 1), this, +1, Level + 1);
                    }
                    return;
                }

                if (this.Text.EndsWith(" " + oper, StringComparison.OrdinalIgnoreCase))
                {
                    // строка заканчивается оператором
                    this.Type = SQLPhraseType.OPERATOR;
                    int _finish = this.Text.LastIndexOf(" ");
                    this.Value = this.Text.Substring(_finish + 1).Trim();
                    if (_finish > 0)
                    {
                        this._prevPhrase = new SQLPhraseWhere(this.Text.Substring(0, _finish), this, -1, Level + 1);
                    }
                    this._nextPhrase = null;
                    return;
                }

                _first = this.Text.IndexOf(" " + oper + " ", 0, StringComparison.OrdinalIgnoreCase);
                if (_first > -1)
                {
                    // оператор внутри строки
                    this.Type = SQLPhraseType.OPERATOR;
                    this.Value = this.Text.Substring(_first, oper.Length + 2).Trim();
                    if (_first > 0)
                    {
                        this._prevPhrase = new SQLPhraseWhere(this.Text.Substring(0, _first), this, -1, Level + 1);
                    }
                    if (_first + oper.Length + 2 < this.Text.Length)
                    {
                        this._nextPhrase = new SQLPhraseWhere(this.Text.Substring(_first + oper.Length + 2), this, +1, Level + 1);
                    }
                    return;
                }
            }

            // -------------------------------------------------------------------------------
            // все что осталось - это какое-то выражение
            this.Type = SQLPhraseType.EXPRESSION;
            this.Value = this.Text;
            this._prevPhrase = null;
            this._nextPhrase = null;
            return;
        }

        /// <summary>
        /// максимальный уровень вложений в дереве
        /// </summary>
        public long MaxLevel 
        { 
            get
            {
                long max = Level;

                if (_prevPhrase != null)
                {
                    long prev_max = _prevPhrase.MaxLevel;
                    if (prev_max > max)
                    {
                        max = prev_max;
                    }
                }

                if (_nextPhrase != null)
                {
                    long next_max = _nextPhrase.MaxLevel;
                    if (next_max > max)
                    {
                        max = next_max;
                    }
                }

                return max;
            }
        }

        /// <summary>
        /// Номер фразы по порядку
        /// </summary>
        public double OrderBy
        {
            get
            {
                double order = 0;

                if (RootPhrase == null)
                {
                    order = this._orderPhrase + Math.Pow(10, this.MaxLevel);
                }
                else
                {
                    order = RootPhrase.OrderBy + this._orderPhrase * Math.Pow(10, MaxLevel - Level);
                }

                return order;
            }
        }

        /// <summary>
        /// Плоский список всех фраз по порядку OrderBy
        /// </summary>
        public SortedDictionary<double, SQLPhraseWhere> FlatListPhrase
        {
            get
            {
                SortedDictionary<double, SQLPhraseWhere> result = new SortedDictionary<double, SQLPhraseWhere>();

                result.Add(this.OrderBy, this);

                if (this._prevPhrase != null)
                {
                    foreach (var item in this._prevPhrase.FlatListPhrase)
                    {
                        result.Add(item.Key, item.Value);
                    }
                }

                if (this._nextPhrase != null)
                {
                    foreach (var item in this._nextPhrase.FlatListPhrase)
                    {
                        result.Add(item.Key, item.Value);
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Предыдущая фраза, с учетом OrderBy и без учета пустых
        /// </summary>
        public SQLPhraseWhere PrevPhrase
        {
            get
            {
                // найдем верхушку дерева
                var _root = this;

                if (_root == null) return null; //-V3022

                while (_root.RootPhrase != null)
                {
                    _root = _root.RootPhrase;
                }

                // ищем предыдущую фразу
                return _root.FlatListPhrase
                    .Where(x => x.Key < this.OrderBy && !string.IsNullOrWhiteSpace(x.Value.Value))
                    .OrderByDescending(x => x.Key)
                    .Select(x => x.Value)
                    .FirstOrDefault();
            }
        }


        /// <summary>
        /// Следующая фраза, с учетом OrderBy и без учета пустых
        /// </summary>
        public SQLPhraseWhere NextPhrase
        {
            get
            {
                // найдем верхушку дерева
                var _root = this;

                if (_root == null) return null; //-V3022

                while (_root.RootPhrase != null)
                {
                    _root = _root.RootPhrase;
                }

                // ищем следующую фразу
                return _root.FlatListPhrase
                    .Where(x => x.Key > this.OrderBy && !string.IsNullOrWhiteSpace(x.Value.Value))
                    .OrderBy(x => x.Key)
                    .Select(x => x.Value)
                    .FirstOrDefault();
            }
        }

        /// <summary>
        /// Клонирование экземпляра SQLPhraseWhere
        /// </summary>
        /// <param name="_root">ссылка на root-фразу</param>
        /// <returns></returns>
        public SQLPhraseWhere Copy(SQLPhraseWhere _root)
        {
            SQLPhraseWhere copy = (SQLPhraseWhere)this.MemberwiseClone();

            copy.RootPhrase = _root;
            copy._prevPhrase = _prevPhrase == null ? null : _prevPhrase.Copy(copy);
            copy._nextPhrase = _nextPhrase == null ? null : _nextPhrase.Copy(copy);

            return copy;
        }


        /// <summary>
        /// текстовое представление пересобранного условия
        /// </summary>
        /// <param name="isDeleteRegionExclude">=true - убрать условия, связанные с полями _deleted</param>
        /// <returns></returns>
        public string AsText(bool isDeleteRegionExclude)
        {
            // копия
            var copy = this.Copy(null);

            if (isDeleteRegionExclude)
            {

                // перебираем все фразы по порядку, убираем условие с полем _deleted
                bool ischanged = false;
                do
                {
                    ischanged = false;
                    foreach (var item in copy.FlatListPhrase)
                    {
                        var _phrase = item.Value;

                        if (
                            _phrase.Type == SQLPhraseType.EXPRESSION &&
                            (
                                _phrase.Value.ToLower().Contains("_deleted") ||
                                _phrase.Value.ToLower().Contains("region_id")
                            )
                        )
                        {
                            _phrase.Value = "";
                            ischanged = true;

                            // теперь надо проверить, что справа или слева
                            if (
                                _phrase.PrevPhrase != null &&
                                _phrase.PrevPhrase.Type == SQLPhraseType.LEFTBRACKET &&
                                _phrase.NextPhrase != null &&
                                _phrase.NextPhrase.Type == SQLPhraseType.RIGHTBRACKET
                            )
                            {
                                // условие было окружено скобками - убираем скобки
                                _phrase.PrevPhrase.Type = SQLPhraseType.EXPRESSION;
                                _phrase.PrevPhrase.Value = "";
                                _phrase.NextPhrase.Type = SQLPhraseType.EXPRESSION;
                                _phrase.NextPhrase.Value = "";
                                ischanged = true;
                            }
                        }
                    }
                } while (ischanged);


                // перебираем все фразы по порядку, убираем лишние операторы и скобки
                do
                {
                    ischanged = false;
                    foreach (var item in copy.FlatListPhrase)
                    {
                        var _phrase = item.Value;

                        // если это оператор
                        if (_phrase.Type == SQLPhraseType.OPERATOR)
                        {
                            if (
                                _phrase.PrevPhrase != null &&
                                _phrase.PrevPhrase.Type == SQLPhraseType.LEFTBRACKET &&
                                _phrase.NextPhrase != null &&
                                _phrase.NextPhrase.Type == SQLPhraseType.RIGHTBRACKET
                            )
                            {
                                // оператор окружен скобками - убираем скобки и сам оператор
                                _phrase.Type = SQLPhraseType.EXPRESSION;
                                _phrase.Value = "";
                                _phrase.PrevPhrase.Type = SQLPhraseType.EXPRESSION;
                                _phrase.PrevPhrase.Value = "";
                                _phrase.NextPhrase.Type = SQLPhraseType.EXPRESSION;
                                _phrase.NextPhrase.Value = "";
                                ischanged = true;
                            }
                        }
                    }
                } while (ischanged);

                // перебираем все фразы по порядку, убираем лишние операторы в конце и начале
                do
                {
                    ischanged = false;
                    foreach (var item in copy.FlatListPhrase)
                    {
                        var _phrase = item.Value;

                        // если это оператор
                        if (_phrase.Type == SQLPhraseType.OPERATOR)
                        {
                            if (_phrase.PrevPhrase == null)
                            {
                                // оператор первый - убираем
                                _phrase.Type = SQLPhraseType.EXPRESSION;
                                _phrase.Value = "";
                                ischanged = true;
                            }

                            if (_phrase.NextPhrase == null)
                            {
                                // оператор последний - убираем
                                _phrase.Type = SQLPhraseType.EXPRESSION;
                                _phrase.Value = "";
                                ischanged = true;
                            }
                        }
                    }

                } while (ischanged);
            }

            string result = "";

            // перебираем все фразы по порядку, собираем строку
            foreach (var item in copy.FlatListPhrase)
            {
                if (!string.IsNullOrWhiteSpace(item.Value.Value))
                {
                    result += " " + item.Value.Value;
                }
            }

            return result.Trim();
        }
    }
}
