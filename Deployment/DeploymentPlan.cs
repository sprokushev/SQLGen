// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using AngleSharp;
using AngleSharp.Dom;
using Microsoft.VisualStudio.VCProjectEngine;
using Newtonsoft.Json.Linq;
using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net.Http;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Markup;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>Класс для загрузки страниц Confluence</summary>
    public partial class ConfluenceHTML
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Извлечение значения Order
        /// </summary>
        /// <param name="value">строка, в которой хранится значение</param>
        /// <returns></returns>
        public static int ExtractOrderValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }

            int.TryParse(value, out var result);

            return result;
        }

        /// <summary>
        /// Извлечение значения Task
        /// </summary>
        /// <param name="value">строка, в которой хранится значение</param>
        /// <returns></returns>
        public static string ExtractTaskValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string result = value
                .TrimAllSpace()
                .Split(new char[] { '\n', '\r' })[0]
                .TrimAllSpace();

            return result;
        }

        /// <summary>
        /// Извлечение значения DBType
        /// </summary>
        /// <param name="value">строка, в которой хранится значение</param>
        /// <returns></returns>
        public static string ExtractDBTypeValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            string result = value.Trim();
            return result;
        }

        /// <summary>
        /// заменяем блоки на divider
        /// </summary>
        /// <param name="value">строка</param>
        /// <param name="divider">разделитель</param>
        /// <returns></returns>
        public static string ReplaceBlockToDivider(string value, string divider)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            if (string.IsNullOrEmpty(divider))
            {
                divider = "";
            }

            // заменяем блоки <> на divider
            int ii;
            bool flag;
            do
            {
                // флаг повторного выполнения
                flag = false;

                // словарь символов <>, найденных в строке
                SortedDictionary<int, string> com = new SortedDictionary<int, string>();

                // заполняем словарь
                ii = 0;
                while (ii > -1)
                {
                    ii = value.IndexOf("<", ii);
                    if (ii > -1)
                    {
                        com.Add(ii, "<");
                        ii++;
                    }
                }
                ii = 0;
                while (ii > -1)
                {
                    ii = value.IndexOf(">", ii);
                    if (ii > -1)
                    {
                        com.Add(ii, ">");
                        ii++;
                    }
                }

                // анализируем наличие в строке символов <>
                if (com.Count > 0)
                {
                    // первый символ с начала строки
                    var first = com.First();

                    if (first.Value == "<")
                    {
                        // если комментарий начинается с <, ищем в этой же строке символ >
                        var next = com
                            .Where(x => (x.Key > first.Key) && (x.Value == ">"))
                            .FirstOrDefault();

                        if (!next.Equals(default(KeyValuePair<int, string>)))
                        {
                            // выделим блок, найдем в нем внешнюю ссылку
                            string block = value.Substring(first.Key + 1, next.Key - first.Key - 1) ?? ""; //-V3022
                            string block_class = "";
                            string block_href = "";

                            foreach (var item in block.Split(' '))
                            {
                                string s = KeyWord.KeyValue(item, "class", new char[] { '=' });
                                if (!string.IsNullOrWhiteSpace(s))
                                {
                                    block_class = s;
                                }

                                s = KeyWord.KeyValue(item, "href", new char[] { '=' });
                                if (!string.IsNullOrWhiteSpace(s))
                                {
                                    block_href = s;
                                }
                            }

                            if (
                                block_class == "external-link" &&
                                !string.IsNullOrWhiteSpace(block_href)
                            )
                            {
                                // нашли ссылку на файл, сверим, есть ли различия с последующим отображением этой ссылки
                                string last = value.Substring(next.Key + 1).TrimAllSpace();
                                if (last.StartsWith(block_href))
                                {
                                    // ссылка и ее отображение совпадают
                                    block_href = "";
                                }
                                else
                                {
                                    // ссылка и ее отображение различаются
                                    block_href = $"ВНИМАНИЕ: скрытая ссылка {block_href} отличается:{Environment.NewLine}";
                                }
                            }

                            // если комментарий начинается и завершается в этой же строке - заменяем <> на перевод строки
                            value = (value.Substring(0, first.Key) + divider + block_href + value.Substring(next.Key + 1)).Trim();
                            // и начинаем анализ заново
                            flag = true;
                            continue;
                        }
                        else
                        {
                            // если комментарий начинается <, но не завершается > в этой же строке - убираем все начиная с < до конца строки
                            value = value.Substring(0, first.Key).Trim();
                        }
                    }
                    else
                    {
                        // если первым идет символ > - вырезаем все с начала строки по > включительно
                        value = value.Substring(first.Key + 1).Trim();
                        // и начинаем анализ заново
                        flag = true;
                        continue;
                    }
                }
            } while (flag);

            return value;
        }

        /// <summary>
        /// заменяем url на команду ликвибота
        /// </summary>
        /// <param name="value">строка</param>
        /// <param name="stand">стенд</param>
        /// <param name="version">версия с префиксом</param>
        /// <returns></returns>
        public static string ReplaceUrlToLiquibot(string value, string stand, string version)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(stand))
            {
                return "";
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                return "";
            }

            stand = stand.ToUpper();

            string result = "";

            bool flag;
            do
            {
                // флаг повторного выполнения
                flag = false;

                // начало url
                int start_ii = value.IndexOf("https://", 0);
                if (start_ii == -1)
                {
                    // нет url в строке
                    break;
                }

                // конец url
                int finish_ii = value.IndexOf(".yml", start_ii);
                if (finish_ii == -1)
                {
                    // нет url в строке
                    break;
                }
                finish_ii = finish_ii + 3;

                // получаем url
                string url = value.Substring(start_ii, finish_ii - start_ii + 1);

                // получим проект из url
                string project = Deployment.GetGITProjectByScript(url);
                string new_url = "";

                // преобразуем url в файл
                string filepath = Utilities.GITProjects.ConvertUrlToFilepath(project, url, out string url_branch, false);

                var list_alias = GITProjects.GetLuquibotAliasByProject(project, stand);


                if (
                    !string.IsNullOrWhiteSpace(filepath) &&
                    list_alias.Count > 0
                )
                {
                    new_url = "";

                    foreach (var alias in list_alias)
                    {
                        if (
                            project == "liquibase_project_new" ||
                            project == "msdbupdate_new"
                         )
                        {
                            if (
                                stand == "QA" ||
                                stand == "QA-REL"
                            )
                            {
                                new_url += Environment.NewLine + $"/update {filepath} {alias}";
                            }
                        }
                        else
                        {
                            new_url += Environment.NewLine + $"/update {filepath} {alias} {version}";
                        }
                    }
                }

                new_url = new_url.TrimAllSpace();

                if (
                    !string.IsNullOrWhiteSpace(url) &&
                    string.IsNullOrWhiteSpace(new_url) &&
                    string.IsNullOrWhiteSpace(project)
                )
                {
                    new_url = $"ВНИМАНИЕ: неизвестный проект, команда для ликвибота не собрана:{Environment.NewLine}{url}";
                }

                // заменим в url в строке
                result = value.Substring(0, start_ii) + new_url;
                value = value.Substring(finish_ii + 1);

                // и начинаем анализ заново
                flag = true;

            } while (flag);

            return (result + value)
                .TrimInnerNewLine(1)
                .TrimAllSpace();
        }

        /// <summary>
        /// Извлечение значения Action
        /// </summary>
        /// <param name="value">строка, в которой хранится значение</param>
        /// <param name="stand">стенд</param>
        /// <param name="version">версия с префиксом</param>
        /// <param name="isReplaceURLtoLiquibot">=true - заменить url на команду локвибота</param>
        /// <returns></returns>
        public static string ExtractActionValue(string value, string stand, string version, bool isReplaceURLtoLiquibot)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            // заменяем блоки <> на перевод строки
            value = ReplaceBlockToDivider(value, Environment.NewLine);

            // заменяем url на команду ликвибота
            if (isReplaceURLtoLiquibot)
            {
                value = ReplaceUrlToLiquibot(value, stand, version);
            }

            string result = value
                .Replace("&nbsp;", "")
                .TrimInnerNewLine()
                .TrimAllSpace();

            return result;
        }

        /// <summary>
        /// Извлечение значения Regions
        /// </summary>
        /// <param name="value">строка, в которой хранится значение</param>
        /// <returns></returns>
        public static List<string> ExtractRegionsValue(string value)
        {
            // заменяем блоки <> на перевод строки
            value = ReplaceBlockToDivider(value, Environment.NewLine);

            if (
                string.IsNullOrWhiteSpace(value) ||
                value.ToLower() == "базовый" ||
                value.ToLower() == "ецп"
            )
            {
                return new List<string>();
            }

            return value.ToList(new char[] { ',', '\n', '\r' }, true);
        }

        /// <summary>
        /// парсинг списка действий
        /// </summary>
        /// <param name="ListDeployment">список действий</param>
        /// <param name="DBREGION">тип БД</param>
        /// <param name="div_block">часть страницы</param>
        /// <param name="DIV_BLOCK_ID">идентификатор</param>
        /// <param name="DIV_DB_ID">идентификатор</param>
        /// <param name="stand">стенд</param>
        /// <param name="version">версия с префиксом</param>
        private void ParsingDeployment(List<Deployment> ListDeployment, string DBREGION, IElement div_block, string DIV_BLOCK_ID, string DIV_DB_ID, string stand, string version)
        {
            if (
                div_block.Children != null &&
                div_block.Children.Length > 1 &&
                div_block.Children[0] != null &&
                div_block.Children[0].Id == DIV_BLOCK_ID &&
                div_block.Children[1] != null &&
                div_block.Children[1].Children != null &&
                div_block.Children[1].Children.Length > 0
            )
            {
                foreach (var div_db in div_block.Children[1].Children
                    .Where(x => x.NodeName == "DIV")
                )
                {
                    if (
                        div_db.Children != null &&
                        div_db.Children.Length > 1 &&
                        div_db.Children[0] != null &&
                        div_db.Children[0].Id == DIV_DB_ID &&
                        div_db.Children[1] != null &&
                        div_db.Children[1].Children != null &&
                        div_db.Children[1].Children.Length > 0
                    )
                    {
                        foreach (var div_db_par in div_db.Children[1].Children
                            .Where(x =>
                                x.NodeName == "DIV" &&
                                x.Children != null &&
                                x.Children.Length > 0
                            )
                        )
                        {
                            foreach (var div_db_par2 in div_db_par.Children
                                .Where(x =>
                                    x.NodeName == "DIV" &&
                                    x.Children != null &&
                                    x.Children.Length > 0
                                )
                            )
                            {
                                foreach (var div_table in div_db_par2.Children
                                        .Where(x =>
                                            x.NodeName == "TABLE" &&
                                            x.Children != null &&
                                            x.Children.Length > 0
                                        )
                                )
                                {
                                    foreach (var div_tbody in div_table.Children
                                        .Where(x =>
                                            x.NodeName == "TBODY" &&
                                            x.Children != null &&
                                            x.Children.Length > 0
                                        )
                                    )
                                    {
                                        foreach (var div_tr in div_tbody.Children
                                            .Where(x =>
                                                x.NodeName == "TR" &&
                                                x.Children != null &&
                                                x.Children.Length >= 6 
                                            )
                                        )
                                        {
                                            var DP = new Deployment();

                                            int _order = ExtractOrderValue(div_tr.Children[0].TextContent);
                                            string _task = ExtractTaskValue(div_tr.Children[1].TextContent);
                                            string _dbtype = ExtractDBTypeValue(div_tr.Children[2].TextContent);
                                            string _action = ExtractActionValue(div_tr.Children[3].InnerHtml, stand, version, false);
                                            List<string> _regions = ExtractRegionsValue(div_tr.Children[5].InnerHtml);


                                            if (_task != "Задача/Документ")
                                            {
                                                DP.SetDeployment(_task, _order, DBREGION, "after", "sql", _action, null, "promed", "all", _regions);

                                                DP.Html = div_block.InnerHtml;
                                                DP.file = ExtractActionValue(div_tr.Children[3].InnerHtml, stand, version, true);

                                                ListDeployment.Add(DP);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Парсинг страниц по номерам версий и url
        /// </summary>
        /// <param name="_stand">стенд</param>
        /// <param name="_listDP">список Deployment Plan</param>
        /// <param name="_win">экземпляр окна</param>
        /// <param name="_action_before_all">применить действие _action_before_all перед всеми версиями</param>
        /// <param name="_action_version">применить действие _action_version к каждому экземпляру DeploymentPlan, до парсинга</param>
        /// <param name="_action_after_version">применить действие _action_after_version к каждому экземпляру DeploymentPlan, после парсинга</param>
        /// <param name="_action_after_all">применить действие _action_after поcле парсинга всех версий</param>
        /// <param name="_action_finish">применить финишное действие _action_finish при любом результате</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        public async System.Threading.Tasks.Task LoadConfluencePages(string _stand, List<DeploymentPlan> _listDP, System.Windows.Window _win,
            System.Action<ConfluenceHTML> _action_before_all,
            System.Action<DeploymentPlan> _action_version,
            System.Action<DeploymentPlan> _action_after_version,
            System.Action<ConfluenceHTML> _action_after_all,
            System.Action<ConfluenceHTML> _action_finish,
            string logFile
        )
        {
            _stand = (_stand??"").ToUpper();

            // проверки
            if (
                (_listDP == null) ||
                (_listDP.Count == 0)
            )
            {
                // выполнить финишное действие
                if (_action_finish != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        _action_finish(this);
                    });
                }

                // курсор нормальный
                if (_win != null)
                {
                    _win.Cursor = System.Windows.Input.Cursors.Arrow;
                }

                return;
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // курсор в ожидание
                if (_win != null)
                {
                    _win.Cursor = System.Windows.Input.Cursors.Wait;
                }

                // выполнить действие перед парсингом версий
                if (_action_before_all != null)
                {
                    _action_before_all(this);
                }
            });

            foreach (var confDP in _listDP)
            {
                if (confDP == null)
                {
                    continue;
                }

                confDP.isLoaded = false;

                // выполнить до парсинга действие с экземпляром ConfluencePage
                if (_action_version != null)
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        _action_version(confDP);
                    });
                }

                string html = "";

                bool isexit = false;
                while (!isexit)
                {
                    html = "";

                    // парсинг страницы
                    try
                    {
                        html = await JiraHTML.SendRequest(confDP.URL, logFile, "https://confluence.rtmis.ru/login.action");

                        if (html == "skip")
                        {
                            // ранее был выбран вариант "Пропустить парсинг страницы"
                            throw new Exception();
                        }

                        //для теста throw new Exception("Пустая html-страница");

                        var config = Configuration.Default;
                        var context = BrowsingContext.New(config);
                        var doc = await context.OpenAsync(req => req.Content(html));

                        var div = doc.QuerySelectorAll("div");

                        // пытаюсь определить, авторизация успешная или нет
                        confDP.ErrorInfo = "Ошибка при парсинге Deployment Plan версии " + confDP.NumVersion + ", возможно неверный логин/пароль или сбой Confluence" + Environment.NewLine;

                        if (!html.Contains("Зарегистрироваться - Confluence РТ МИС"))
                        {
                            confDP.ErrorInfo = "";
                        }

                        if (!string.IsNullOrWhiteSpace(confDP.ErrorInfo))
                        {
                            // ошибка авторизации
                            throw new Exception(confDP.ErrorInfo);
                        }

                        // по всей видимости авторизация успешная, продолжаю пасинг
                        confDP.isLoaded = true;

                        if (
                            div != null &&
                            div.Length > 0
                        )
                        {
                            foreach (var div_block in div)
                            {
                                if (div_block.ClassName == "rwui_expandable_item rw_open conf-macro output-block")
                                {
                                    if (confDP.isAddMS)
                                    {
                                        // Парсинг раздела MS SQL
                                        ParsingDeployment(confDP.ListDBAction, "MS SQL", div_block, "rwui_expand-MSSQL", "rwui_expand-31", _stand, confDP.NumVersion);
                                    }

                                    if (confDP.isAddPG)
                                    {
                                        // Парсинг раздела PG SQL
                                        ParsingDeployment(confDP.ListDBAction, "PG SQL", div_block, "rwui_expand-PGSQL", "rwui_expand-21", _stand, confDP.NumVersion);
                                    }
                                }
                            }
                        }

                        isexit = true;
                    }
                    catch (Exception ex)
                    {
                        if (html == "skip")
                        {
                            isexit = true;
                        }
                        else
                        {
                            App.AddLog(html, null, App.ShowMessageMode.NONE, true, logFile);
                            string error = App.AddLog("", ex, App.ShowMessageMode.NONE, true, logFile)
                                .showMessage
                                .TrimStartNewLine();

                            isexit = (System.Windows.Forms.MessageBox.Show(error + $"\n\n(Да\\Yes) - Выполнить парсинг повторно?\nили\n(Нет\\No) - Пропустить парсинг версии {confDP.NumVersion} ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No);

                            if (isexit)
                            {
                                App.AddLog($"Пользователь выбрал: Пропустить парсинг версии {confDP.NumVersion}", null, App.ShowMessageMode.NONE, true, logFile);
                            }
                        }

                        if (
                            isexit &&
                            (System.Windows.Forms.MessageBox.Show(
                                                    "Завершить парсинг версий из Confluence?",
                                                    "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        )
                        {
                            App.AddLog("Пользователь выбрал: Завершить парсинг версий из Confluence", null, App.ShowMessageMode.NONE, true, logFile);

                            System.Windows.Application.Current.Dispatcher.Invoke(() =>
                            {
                                // выполнить финишное действие
                                if (_action_finish != null)
                                {
                                    _action_finish(this);
                                }

                                // курсор нормальный
                                if (_win != null)
                                {
                                    _win.Cursor = System.Windows.Input.Cursors.Arrow;
                                }
                            });
                            return;
                        }
                    }
                }
            }

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // выполнить действие после парсинга всех версий
                if (_action_after_all != null)
                {
                    _action_after_all(this);
                }

                // выполнить финишное действие
                if (_action_finish != null)
                {
                    _action_finish(this);
                }

                // курсор нормальный
                if (_win != null)
                {
                    _win.Cursor = System.Windows.Input.Cursors.Arrow;
                }
            });
        }
    }

    // =========================================================================================================
    /// <summary>
    /// страница Deployment Plan в Confluence
    /// </summary>
    public class DeploymentPlan
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// инициализация экземпляра DeploymentPlan
        /// </summary>
        public DeploymentPlan()
        {
            Clear();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// копирование экземпляра DeploymentPlan
        /// </summary>
        /// <returns></returns>
        public DeploymentPlan Copy()
        {
            return (DeploymentPlan)this.MemberwiseClone();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// очистка всех полей
        /// </summary>
        public void Clear()
        {
            isLoaded = false;
            isAddMS = false;
            isAddPG = false;
            ErrorInfo = "";
            URL = "";
            NumVersion = "";

            ListDBAction = new List<Deployment>();
        }

        //-- Служебные поля -----------------------------------------------------------------------------------

        /// <summary>
        /// Номер версии с префиксом:
        /// </summary>
        public string NumVersion { get; set; }

        /// <summary>
        /// URL страницы
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// Распознать MS
        /// </summary>
        public bool isAddMS { get; set; }

        /// <summary>
        /// Распознать PG
        /// </summary>
        public bool isAddPG { get; set; }

        /// <summary>
        /// результат парсинга страницы
        /// </summary>
        public bool isLoaded { get; set; }

        /// <summary>Ошибка при парсинге</summary>
        public string ErrorInfo { get; set; }


        //-- Поля из страницы Confluence -----------------------------------------------------------------------------------

        /// <summary>
        /// Список действий с БД
        /// </summary>
        public List<Deployment> ListDBAction {  get; set; }
    }
}
