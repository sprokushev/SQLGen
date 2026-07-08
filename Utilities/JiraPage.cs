// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using AngleSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;
using SQLGen.Utilities;
using System.Windows.Documents;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>Класс для загрузки страниц Jira</summary>
    public partial class JiraHTML
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Окно подключения к Jira</summary>
        /// <param name="_logfile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static bool OpenLoginJira(string _logfile)
        {
            FormLoginJira dlg1 = new FormLoginJira(_logfile);

            dlg1.tbUsername.Text = MainWindow.APPinfo.UsernameJira;
            dlg1.tbPassword.Text = MainWindow.APPinfo.PasswordJira;
            dlg1.cbSavePassword.Checked = MainWindow.APPinfo.isSavePasswordJira;

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                MainWindow.APPinfo.UsernameJira = dlg1.tbUsername.Text;
                MainWindow.APPinfo.PasswordJira = dlg1.tbPassword.Text;
                MainWindow.APPinfo.isSavePasswordJira = dlg1.cbSavePassword.Checked == true;

                dlg1.Dispose();
                return true;
            }

            dlg1.Dispose();
            return false;
        }

        /// <summary>
        /// Вырезать из url элементы
        /// </summary>
        /// <param name="url"></param>
        /// <param name="tree"></param>
        /// <param name="branch"></param>
        /// <param name="path"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public static string ExtractElementsFromURL(string url, out string tree, out string branch, out string path, out string file)
        {
            string result = "";
            path = "";
            file = "";
            branch = "";
            tree = "";

            if (string .IsNullOrWhiteSpace(url))
            {
                return "";
            }

            url = url.Replace(System.IO.Path.DirectorySeparatorChar, '/');

            var arr = url.Trim().Split('/');

            // выделяем имя файла
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(arr[i]))
                {
                    if (arr[i].Contains("."))
                    {
                        file = arr[i].Trim();
                        arr[i] = "";
                    }
                    break;
                }
            }

            // выделяем имя папки
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(arr[i]))
                {
                    path = arr[i].Trim();
                    arr[i] = "";
                    break;
                }
            }

            // выделяем имя ветки
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(arr[i]))
                {
                    branch = arr[i].Trim();
                    arr[i] = "";
                    break;
                }
            }

            if (branch == "cron")
            {
                path = branch + "/" + path;
                branch = "";

                for (int i = arr.Length - 1; i >= 0; i--)
                {
                    if (!string.IsNullOrWhiteSpace(arr[i]))
                    {
                        branch = arr[i].Trim();
                        arr[i] = "";
                        break;
                    }
                }
            }

            // выделяем blob tree
            for (int i = arr.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrWhiteSpace(arr[i]))
                {
                    tree = arr[i].Trim();
                    arr[i] = "";
                    break;
                }
            }

            result = string.Join("/", 
                arr.Where(x => !string.IsNullOrWhiteSpace(x))
                ).Trim();
            if (!string.IsNullOrWhiteSpace(result))
            {
                result = result + "/";
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выделить из url имя yml-файла или json-файла
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="mask_task">url до папки task</param>
        /// <param name="file">yml-файл или json-файл</param>
        /// <param name="path">папка, где лежит файл</param>
        /// <param name="branch">ветка из url</param>
        /// <param name="rest">что остается в url после вырезания branch, path, yml</param>
        /// <returns></returns>
        public static bool GetYMLFileFromURL(string url, string mask_task, out string file, out string path, out string branch, out string rest)
        {
            url = url.Replace("?ref_type=heads", "");
            file = "";
            path = "";
            branch = "";
            rest = url;

            if (string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(mask_task))
            {
                return false;
            }

            rest = ExtractElementsFromURL(url, out string url_tree, out string url_branch, out string url_path, out string url_file);
            mask_task = ExtractElementsFromURL(mask_task, out string mask_tree, out string mask_branch, out string mask_path, out string mask_file);

            if (
                mask_path != "task" ||
                ( 
                    url_path != "task" && 
                    url_path != "deployment" && 
                    url_path != "version" &&
                    url_path != "cron" &&
                    !url_path.StartsWith("cron") && 
                    url_path != "Report"
                ) ||
                (
                    mask_branch != "release" &&
                    mask_branch != "%BRANCH%"
                ) ||
                (
                    mask_branch == "release" && 
                    url_branch != "release"
                ) ||
                (
                    url_tree != "blob" &&
                    url_tree != "tree"
                ) ||
                (
                    mask_tree != "blob" &&
                    mask_tree != "tree"
                ) ||
                rest != mask_task
            )
            {
                // url не соответствует mask или mask не корректный или url не корректный
                return false;
            }

            file = url_file;
            path = url_path.Replace('/', System.IO.Path.DirectorySeparatorChar);
            branch = url_branch;
            rest = rest + url_tree + "/";
            return true;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Регулярное выражение, соответствующее любому символу, не являющемуся десятичной цифрой
        /// </summary>
        public static Regex regex_order = new Regex(@"\D");

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// функция для сортировки задач
        /// </summary>
        /// <param name="_ymlfile">имя файла</param>
        /// <returns></returns>
        public static string SetTaskOrder(string _ymlfile)
        {
            if (string.IsNullOrWhiteSpace(_ymlfile)) return "";

            if (_ymlfile.StartsWith("PROMEDWEB-"))
            {
                _ymlfile = _ymlfile.Replace("PROMEDWEB-", "");
                char[] chars = { '.', '_', '-' };
                int i = _ymlfile.IndexOfAny(chars);
                if (i > 0) _ymlfile = _ymlfile.Substring(0, i);
            }

            if (_ymlfile.StartsWith("BIP-"))
            {
                _ymlfile = _ymlfile.Replace("BIP-", "");
                char[] chars = { '.', '_', '-' };
                int i = _ymlfile.IndexOfAny(chars);
                if (i > 0) _ymlfile = _ymlfile.Substring(0, i);
            }

            if (_ymlfile.StartsWith("SMP-"))
            {
                _ymlfile = _ymlfile.Replace("SMP-", "");
                char[] chars = { '.', '_', '-' };
                int i = _ymlfile.IndexOfAny(chars);
                if (i > 0) _ymlfile = _ymlfile.Substring(0, i);
            }

            if (_ymlfile.StartsWith("RPMS-"))
            {
                _ymlfile = _ymlfile.Replace("RPMS-", "");
                char[] chars = { '.', '_', '-' };
                int i = _ymlfile.IndexOfAny(chars);
                if (i > 0) _ymlfile = _ymlfile.Substring(0, i);
            }

            return regex_order.Replace(_ymlfile, "").PadLeft(10, '0');
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Получить HTML-страницу в виде строки
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        /// <param name="login_url">страница для login</param>
        /// <returns></returns>
        public async static Task<string> SendRequest(string url, string logFile, string login_url = "https://jira.rtmis.ru/login.jsp")
        {
            App.AddLog($"Парсинг {url}", null, App.ShowMessageMode.NONE, true, logFile);

            string data = "";

            bool isexit = false;

            while (!isexit)
            {
                var baseAddress = new Uri(url); //Базовый адрес 
                var loginAddress = new Uri(login_url); //Страница для login
                //var cookieContainer = new CookieContainer(); //Показываю как отправлять Cookie (для примера, если необходимо). Можно убрать.

                FormUrlEncodedContent contentJira = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("os_username", MainWindow.APPinfo.UsernameJira),
                    new KeyValuePair<string, string>("os_password", MainWindow.APPinfo.PasswordJira),
                    new KeyValuePair<string, string>("os_destination", ""),
                    new KeyValuePair<string, string>("user_role", ""),
                    new KeyValuePair<string, string>("atl_token", ""),
                    new KeyValuePair<string, string>("login", "Вход")
                });

                HttpClientHandler handler = null;
                HttpClient client = null;
                try
                {
                    handler = new HttpClientHandler() /*{ CookieContainer = cookieContainer }*/;

                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true; // Принять сертификат

                    client = new HttpClient(handler) { BaseAddress = baseAddress };

                    //Добавляем нужные нам параметры в запрос (на примере UserAgent'a)
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/62.0.3202.94 Safari/537.36 OPR/49.0.2725.64");

                    //Добавляем необходимые Cookie
                    //cookieContainer.Add(baseAddress, new Cookie("lang", "en")); 

                    await client.PostAsync(loginAddress, contentJira); //Отправляем на нужную страницу POST запрос для логина
                    var result = await client.GetAsync(baseAddress);
                    var bytes = await result.Content.ReadAsByteArrayAsync();
                    Encoding encoding = Encoding.GetEncoding("utf-8");
                    data = encoding.GetString(bytes, 0, bytes.Length);

                    result.EnsureSuccessStatusCode();

                    isexit = true;

                    if (string.IsNullOrWhiteSpace(data))
                    {
                        throw new Exception("Пустая html-страница");
                    }

                    //для теста throw new Exception("Пустая html-страница");
                }
                catch (Exception ex)
                {
                    App.AddLog($"Ошибка парсинга {url}", ex, App.ShowMessageMode.NONE, true, logFile);

                    isexit = (System.Windows.Forms.MessageBox.Show($"Ошибка парсинга {url}\n\n(Да\\Yes) - Выполнить парсинг повторно?\nили\n(Нет\\No) - Пропустить парсинг {url} ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No);

                    data = "";

                    if (isexit)
                    {
                        App.AddLog($"Пользователь выбрал: Пропустить парсинг {url}", null, App.ShowMessageMode.NONE, true, logFile);
                        data = "skip";
                    }
                }
                finally
                {
                    if (handler != null) handler.Dispose();
                    if (client != null) client.Dispose();
                }
            }

            return data;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Извлечение значения поля
        /// </summary>
        /// <param name="_value">строка, в которой хранится значение</param>
        /// <returns></returns>
        public static string ExtractFieldValue(string _value)
        {
            string result = _value.Trim();

            var arr = result.Replace("Показать", "|").Split('|');
            if (arr.Length > 1) result = arr[1];
            while (result != result.Replace("  ", " "))
            {
                result = result.Replace("  ", " ").Trim();
            }
            while (result != result.Replace(", ,", ","))
            {
                result = result.Replace(", ,", ",").Trim();
            }
            while (result != result.Replace(",,", ","))
            {
                result = result.Replace(",,", ",").Trim();
            }
            result = result
                .Trim()
                .Trim(',')
                .Trim();

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Список распознанных страниц
        /// </summary>
        public List<JiraPage> JiraPages = new List<JiraPage>();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Парсинг страниц по номерам задач и url
        /// </summary>
        /// <param name="_listtask">список tasknumber:url</param>
        /// <param name="_win">экземпляр окна</param>
        /// <param name="_action_before_all">применить действие _action_before_all перед всеми задачами</param>
        /// <param name="_action_task">применить действие _action_task к каждому экземпляру JiraPage, до парсинга</param>
        /// <param name="_action_after_task">применить действие _action_task_after к каждому экземпляру JiraPage, после парсинга</param>
        /// <param name="_action_after_all">применить действие _action_after поcле парсинга задач</param>
        /// <param name="_action_finish">применить финишное действие _action_finish при любом результате</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        public async System.Threading.Tasks.Task LoadJiraPages(Dictionary<string, string> _listtask, System.Windows.Window _win,
            System.Action<JiraHTML> _action_before_all,
            System.Action<JiraPage> _action_task,
            System.Action<JiraPage> _action_after_task,
            System.Action<JiraHTML> _action_after_all,
            System.Action<JiraHTML> _action_finish,
            string logFile
        )
        {
            // проверки
            if (
                (_listtask == null) ||
                (_listtask.Count == 0)
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

                // выполнить действие перед парсингом задач
                if (_action_before_all != null)
                {
                    _action_before_all(this);
                }
            });

            JiraPages.Clear();

            foreach (var item in _listtask.Where(x => !string.IsNullOrWhiteSpace(x.Key)))
            {
                string _task = item.Key;
                string _url = item.Value;

                var jiraPage = new JiraPage();

                jiraPage.TaskNumber = _task.Trim();

                if (string.IsNullOrEmpty(_url))
                {
                    _url = MainWindow.APPinfo.TaskUrlDefault + jiraPage.TaskNumber;
                }

                jiraPage.URL = _url;
                jiraPage.isLoaded = false;

                // выполнить до парсинга
                if (jiraPage != null) //-V3022
                {
                    // выполнить действие с экземпляром JiraPage
                    if (_action_task != null)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _action_task(jiraPage);
                        });
                    }
                }

                string html = "";

                bool isexit = false;
                while (!isexit)
                {
                    html = "";

                    // парсинг страницы
                    try
                    {
                        html = await JiraHTML.SendRequest(jiraPage.URL, logFile);

                        if (html == "skip")
                        {
                            // ранее был выбран вариант "Пропустить парсинг страницы"
                            throw new Exception();
                        }

                        //для теста throw new Exception("Пустая html-страница");

                        var config = Configuration.Default;
                        var context = BrowsingContext.New(config);
                        var doc = await context.OpenAsync(req => req.Content(html));

                        var li = doc.QuerySelectorAll("li");
                        var span = doc.QuerySelectorAll("span");
                        var head = doc.QuerySelectorAll("head");
                        var div = doc.QuerySelectorAll("div");

                        // пытаюсь определить, авторизация успешная или нет
                        jiraPage.ErrorInfo = "Ошибка при парсинге задачи " + jiraPage.TaskNumber + ", возможно неверный логин/пароль или сбой Jira" + Environment.NewLine;

                        if (
                            (!html.Contains("Вы должны войти в систему для доступа к этой странице")) &&
                            (head != null) &&
                            (head.Length > 0) &&
                            (head[0].Children != null)
                        )
                        {
                            foreach (var element in head[0].Children)
                            {
                                if (element.GetAttribute("name") == "ajs-issue-key")
                                {
                                    jiraPage.ErrorInfo = "";
                                }
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(jiraPage.ErrorInfo))
                        {
                            // ошибка авторизации
                            throw new Exception(jiraPage.ErrorInfo);
                        }

                        // по всей видимости авторизация успешная, продолжаю пасинг
                        jiraPage.isLoaded = true;

                        IEnumerable<AngleSharp.Dom.IElement> e;

                        // Title
                        jiraPage.Title = doc.Title;

                        // TaskType
                        e = span.Where(t => t.Id == "type-val");
                        if (e.Count() > 0)
                        {
                            jiraPage.TaskType = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace('\t', ',')
                                );
                        }
                        
                        // Priority
                        e = span.Where(t => t.Id == "priority-val");
                        if (e.Count() > 0)
                        {
                            jiraPage.Priority = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace('\t', ',')
                                );
                        }

                        // TaskStatus
                        e = span.Where(t => t.Id == "status-val");
                        if (e.Count() > 0)
                        {
                            jiraPage.TaskStatus = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace('\t', ',')
                                );
                        }

                        // Resolution
                        e = span.Where(t => t.Id == "resolution-val");
                        if (e.Count() > 0)
                        {
                            jiraPage.Resolution = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace('\t', ',')
                                );
                        }

                        // FixInVersion
                        e = span.Where(t => t.Id == "fixVersions-field");
                        if (e.Count() > 0)
                        {
                            jiraPage.FixInVersion = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace('\t', ',')
                                .Replace("Исправить в версиях:", "")
                                );
                        }

                        // AffectedVersion
                        e = span.Where(t => t.Id == "versions-val");
                        if (e.Count() > 0)
                        {
                            jiraPage.AffectedVersion = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace('\t', ',')
                                );
                        }

                        // PlannedVersion
                        e = li.Where(t => t.Id == "rowForcustomfield_10806");
                        if (e.Count() > 0)
                        {
                            jiraPage.PlannedVersion = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace('\t', ',')
                                .Replace("Планируемая версия:", "")
                                );
                        }

                        // Components

                        // Labels

                        // Description
                        e = div.Where(t => t.Id == "descriptionmodule");
                        if (e.Count() > 0)
                        {
                            jiraPage.Description = ExtractFieldValue(
                                e.First().TextContent
                                //.Replace('\r', ' ')
                                //.Replace('\n', ' ')
                                .Replace('\t', ' ')
                                .Replace("Описание", "")
                                );
                        }

                        // EpicLink

                        // Timesheet

                        // Module

                        // Source

                        // Region
                        e = li.Where(t => t.Id == "rowForcustomfield_11949");
                        if (e.Count() > 0)
                        {
                            jiraPage.Region = string.Join("\n",
                                ExtractFieldValue(
                                    e.First().TextContent
                                    .Replace('\r', ',')
                                    .Replace('\n', ',')
                                    .Replace('\t', ',')
                                    .Replace("Регион:", "")
                                )
                                .Split(',')
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToArray()
                            );
                        }

                        // CommitmentType

                        // CommitmentYear

                        // RequirementType

                        // Sprint

                        // DiscoveryPlace

                        // TestingResult

                        // Cause

                        // TZLinks

                        // BranchInGIT

                        // DataBD
                        e = li.Where(t => t.Id == "rowForcustomfield_11912");
                        if (e.Count() > 0)
                        {
                            jiraPage.DataBD = string.Join("\n",
                                ExtractFieldValue(
                                    e.First().TextContent
                                    .Replace('\r', ',')
                                    .Replace('\n', ',')
                                    .Replace('\t', ',')
                                    .Replace("Данные в БД:", "")
                                )
                                .Split(',')
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToArray()
                            );
                        }

                        // ObjectsBD
                        e = li.Where(t => t.Id == "rowForcustomfield_11914");
                        if (e.Count() > 0)
                        {
                            jiraPage.ObjectsBD = string.Join("\n",
                                ExtractFieldValue(
                                    e.First().TextContent
                                    .Replace('\r', ',')
                                    .Replace('\n', ',')
                                    .Replace('\t', ',')
                                    .Replace("Объекты в БД:", "")
                                )
                                .Split(',')
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToArray()
                            );
                        }

                        // BaseRegionBD
                        e = li.Where(t => t.Id == "rowForcustomfield_13707");
                        if (e.Count() > 0)
                        {
                            jiraPage.BaseRegionBD = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace('\t', ' ')
                                .Replace("Базовая региональность БД:", "")
                                )
                                .Split(',')
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .FirstOrDefault();
                        }

                        // YMLLinks
                        e = li.Where(t => t.Id == "rowForcustomfield_12502");
                        if (e.Count() > 0)
                        {
                            jiraPage.YMLLinks = ExtractFieldValue(
                                    e.First().TextContent
                                    .Replace('\r', ',')
                                    .Replace('\n', ',')
                                    .Replace('\t', ',')
                                    .Replace("Ссылка на yml:", "")
                                    .Trim()
                                    .Replace(' ', ',')
                                )
                                .Split(',')
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .ToArray();
                        }

                        // Downtime
                        e = li.Where(t => t.Id == "rowForcustomfield_13311");
                        if (e.Count() > 0)
                        {
                            jiraPage.Downtime = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace('\t', ' ')
                                .Replace("Требуется ДТ (Down Time):", "")
                                )
                                .Split(',')
                                .Where(x => !string.IsNullOrWhiteSpace(x))
                                .FirstOrDefault();
                        }

                        // UpdActions
                        e = li.Where(t => t.Id == "rowForcustomfield_11983");
                        if (e.Count() > 0)
                        {
                            jiraPage.UpdActions = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\t', ' ')
                                .Replace("Действия при обновлении:", "")
                                );
                        }

                        // EpicName
                        e = div.Where(t => t.Id == "customfield_10007-val");
                        if (e.Count() > 0)
                        {
                            jiraPage.EpicName = ExtractFieldValue(
                                e.First().TextContent
                                .Replace('\t', ' ')
                                );
                        }

                        // добавить страницу в список
                        if (jiraPage != null) //-V3022
                        {
                            JiraPages.Add(jiraPage);

                            // выполнить действие с экземпляром JiraPage после парсинга
                            if (_action_after_task != null)
                            {
                                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                                {
                                    _action_after_task(jiraPage);
                                });
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

                            isexit = (System.Windows.Forms.MessageBox.Show(error + $"\n\n(Да\\Yes) - Выполнить парсинг повторно?\nили\n(Нет\\No) - Пропустить парсинг задачи {jiraPage.TaskNumber} ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No);

                            if (isexit)
                            {
                                App.AddLog($"Пользователь выбрал: Пропустить парсинг задачи {jiraPage.TaskNumber}", null, App.ShowMessageMode.NONE, true, logFile);
                            }
                        }

                        if (
                            isexit &&
                            (System.Windows.Forms.MessageBox.Show(
                                                    "Завершить парсинг задач из Jira?",
                                                    "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                        )
                        {
                            App.AddLog("Пользователь выбрал: Завершить парсинг задач из Jira", null, App.ShowMessageMode.NONE, true, logFile);

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
                // выполнить действие после парсинга задач
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

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Список задач 
        /// </summary>
        public List<string> TaskList { get; set; } = new List<string>();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Парсинг списка задач со релизной страницы
        /// </summary>
        /// <param name="url">страница со списком задач</param>
        /// <param name="_win">экземпляр окна</param>
        /// <param name="_action">применить действие _action</param>
        /// <param name="_action_finish">применить финишное действие _action_finish при любом результате</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        public async System.Threading.Tasks.Task LoadTaskListJiraPages(
            string url,
            System.Windows.Window _win,
            System.Action<JiraHTML> _action,
            System.Action<JiraHTML> _action_finish,
            string logFile
        )
        {
            // проверки
            if (string.IsNullOrWhiteSpace(url))
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
            });

            TaskList.Clear();

            string html = "";
            string base_url = url;
            int cnt = 0;
            bool isexit = false;
            while (!isexit)
            {
                html = "";

                // парсинг страницы
                try
                {
                    if (cnt>0)
                    {
                        url = base_url + $"&startIndex={cnt}";
                    }

                    html = await JiraHTML.SendRequest(url, logFile);

                    if (html == "skip")
                    {
                        // ранее был выбран вариант "Пропустить парсинг страницы"
                        throw new Exception();
                    }

                    //для теста throw new Exception("Пустая html-страница");

                    var config = Configuration.Default;
                    var context = BrowsingContext.New(config);
                    var doc = await context.OpenAsync(req => req.Content(html));

                    //var li = doc.QuerySelectorAll("li");
                    //var span = doc.QuerySelectorAll("span");
                    //var head = doc.QuerySelectorAll("head");
                    var td = doc.QuerySelectorAll("td");

                    int add = 0;
                    foreach (var item in td.Where(x => x.ClassName == "issuekey"))
                    {
                        string tasknum = ExtractFieldValue(
                                item.TextContent
                                .Replace('\r', ',')
                                .Replace('\n', ',')
                                .Replace('\t', ',')
                                ).
                                ToUpper();

                        if (
                            (!string.IsNullOrWhiteSpace(tasknum)) &&
                            (!TaskList.Contains(tasknum))
                        )
                        {
                            TaskList.Add(tasknum);
                            add++;
                        }
                    }

                    if (add > 0)
                    {
                        // были добавлены новые задачи - парсим следующую страницу
                        cnt += 50;
                    }
                    else
                    {
                        // новых задач не было - выходим
                        isexit = true;
                    }
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

                        isexit = (System.Windows.Forms.MessageBox.Show(error + $"\n\n(Да\\Yes) - Выполнить парсинг повторно?\nили\n(Нет\\No) - Пропустить парсинг {url} ?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No);

                        if (isexit)
                        {
                            App.AddLog($"Пользователь выбрал: Пропустить парсинг {url}", null, App.ShowMessageMode.NONE, true, logFile);
                        }
                    }

                    if (
                        isexit &&
                        (System.Windows.Forms.MessageBox.Show(
                                                "Завершить парсинг из Jira?",
                                                "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    )
                    {
                        App.AddLog("Пользователь выбрал: Завершить парсинг из Jira", null, App.ShowMessageMode.NONE, true, logFile);

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

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // выполнить действие после парсинга
                if (_action != null)
                {
                    _action(this);
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
    /// страница Jira
    /// </summary>
    public class JiraPage
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// инициализация экземпляра JiraPage
        /// </summary>
        public JiraPage()
        {
            Clear();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// копирование экземпляра JiraPage
        /// </summary>
        /// <returns></returns>
        public JiraPage Copy()
        {
            return (JiraPage)this.MemberwiseClone();
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// очистка всех полей
        /// </summary>
        public void Clear()
        {
            isLoaded = false;
            ErrorInfo = "";
            URL = "";

            TaskNumber = "";
            Title = "";
            TaskType = "";
            Priority = "";
            TaskStatus = "";
            Resolution = "";
            FixInVersion = "";
            AffectedVersion = "";
            PlannedVersion = "";
            Components = "";
            Labels = "";
            Description = "";
            EpicLink = "";
            Timesheet = "";
            Module = "";
            Source = "";
            Region = "";
            CommitmentType = "";
            CommitmentYear = "";
            RequirementType = "";
            Sprint = "";
            DiscoveryPlace = "";
            TestingResult = "";
            Cause = "";
            TZLinks = "";
            BranchInGIT = "";
            DataBD = "";
            ObjectsBD = "";
            BaseRegionBD = "";
            YMLLinks = null;
            Downtime = "";
            UpdActions = "";
            EpicName = "";
        }

        //-- Служебные поля -----------------------------------------------------------------------------------
        /// <summary>
        /// URL страницы
        /// </summary>
        public string URL { get; set; }

        /// <summary>
        /// результат парсинга страницы
        /// </summary>
        public bool isLoaded { get; set; }

        /// <summary>Ошибка при парсинге</summary>
        public string ErrorInfo { get; set; }


        //-- Поля из страницы Jira -----------------------------------------------------------------------------------

        /// <summary>
        /// Номер задачи:
        /// </summary>
        public string TaskNumber { get; set; }

        /// <summary>
        /// Заголовок задачи:
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Тип задачи:
        /// </summary>
        public string TaskType { get; set; }

        /// <summary>
        /// Приоритет задачи:
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// Статус задачи:
        /// </summary>
        public string TaskStatus { get; set; }

        /// <summary>
        /// Решение:
        /// </summary>
        public string Resolution { get; set; }

        /// <summary>
        /// Исправить в версиях:
        /// </summary>
        public string FixInVersion { get; set; }

        /// <summary>
        /// Затронуты версии:
        /// </summary>
        public string AffectedVersion { get; set; }

        /// <summary>
        /// Планируемая версия:
        /// </summary>
        public string PlannedVersion { get; set; }

        /// <summary>
        /// Компоненты:
        /// </summary>
        public string Components { get; set; }

        /// <summary>
        /// Метки:
        /// </summary>
        public string Labels { get; set; }

        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Epic Link:
        /// </summary>
        public string EpicLink { get; set; }

        /// <summary>
        /// Проект TS:
        /// </summary>
        public string Timesheet { get; set; }

        /// <summary>
        /// Подсистема/Модуль:
        /// </summary>
        public string Module { get; set; }

        /// <summary>
        /// Источник:
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// Регион:
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// Тип обязательств:
        /// </summary>
        public string CommitmentType { get; set; }

        /// <summary>
        /// Год обязательств:
        /// </summary>
        public string CommitmentYear { get; set; }

        /// <summary>
        /// Тип требования:
        /// </summary>
        public string RequirementType { get; set; }

        /// <summary>
        /// Sprint
        /// </summary>
        public string Sprint { get; set; }

        /// <summary>
        /// Среда обнаружения:
        /// </summary>
        public string DiscoveryPlace { get; set; }

        /// <summary>
        /// Результат тестирования:
        /// </summary>
        public string TestingResult { get; set; }

        /// <summary>
        /// Причина возникновения:
        /// </summary>
        public string Cause { get; set; }

        /// <summary>
        /// Ссылки на ТЗ:
        /// </summary>
        public string TZLinks { get; set; }

        /// <summary>
        /// Ветка в Git:
        /// </summary>
        public string BranchInGIT { get; set; }

        /// <summary>
        /// Данные в БД:
        /// </summary>
        public string DataBD { get; set; }

        /// <summary>
        /// Объекты в БД:
        /// </summary>
        public string ObjectsBD { get; set; }

        /// <summary>
        /// Базовая региональность БД
        /// </summary>
        public string BaseRegionBD { get; set; }

        /// <summary>
        /// Ссылка на yml:
        /// </summary>
        public string[] YMLLinks { get; set; }

        /// <summary>
        /// Требуется Downtime
        /// </summary>
        public string Downtime { get; set; }

        /// <summary>
        /// Действия при обновлении
        /// </summary>
        public string UpdActions { get; set; }

        /// <summary>
        /// Epic Name:
        /// </summary>
        public string EpicName { get; set; }

        // -- вычисляемые поля --------------------------------------------------------------------

        /// <summary>
        /// Список yml-файлов в задаче
        /// </summary>
        public List<JiraYMLInfo> ListYML
        {
            get
            {
                List<JiraYMLInfo> result = new List<JiraYMLInfo>();

                if (YMLLinks != null)
                {
                    int CountYml = 0;

                    foreach (string item in YMLLinks.Where(x => !string.IsNullOrWhiteSpace(x)))
                    {
                        var info = new JiraYMLInfo();

                        CountYml++;
                        info.OrderInTask = CountYml.ToString().PadLeft(5, '0');
                        info.URLFile = item.Trim();
                        info.Project = "unknown";
                        info.Branch = "";
                        info.Filename = "";
                        info.Path = "";

                        // перебираем проекты GIT, выделяем составляющие из URLFile
                        foreach (var project in MainWindow.APPinfo.GITProjects
                            .Where(x=>!string.IsNullOrWhiteSpace(x.DEVUrl))
                            .Select(x => x.DEVProject)
                            .Union(
                                MainWindow.APPinfo.GITProjects
                                    .Where(x => !string.IsNullOrWhiteSpace(x.GITUrl))
                                    .Select(x => x.GITProject)
                            )
                            .Where(x => !string.IsNullOrWhiteSpace(x))
                        )
                        {
                            string yml = "";
                            string branch = "";
                            string path = "";
                            string rest = "";

                            string mask = Utilities.GITProjects.GetURLTaskByProject(project);

                            if (JiraHTML.GetYMLFileFromURL(info.URLFile, mask, out yml, out path, out branch, out rest))
                            {
                                info.Filename = yml;
                                info.Project = project;
                                info.Branch = branch;
                                info.Path = path;
                                break;
                            }
                        }

                        result.Add(info);
                    }
                }

                return result;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Поле для первоначального упорядочивания = по имени первого yml-файла в задаче
        /// </summary>
        public string Order
        {
            get
            {
                foreach (var item in ListYML.Where(x => !string.IsNullOrWhiteSpace(x.Filename)))
                {
                    return JiraHTML.SetTaskOrder(item.Filename);
                }
                return JiraHTML.SetTaskOrder(TaskNumber);
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Базовая региональность БД
        /// </summary>
        public bool IsBaseRegionBD
        {
            get
            {
                return BaseRegionBD.ToLower() == "да";
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Требуется Downtime</summary>
        public bool IsDowntime
        {
            get
            {
                return Downtime.ToLower() == "да";
            }
        }
    }

    // =========================================================================================================
    /// <summary>
    /// описание yml-файла из поля Ссылка на yml:
    /// </summary>
    public class JiraYMLInfo
    {
        /// <summary>
        /// Поле для упорядочивания yml внутри задачи
        /// </summary>
        public string OrderInTask { get; set; }

        /// <summary>
        /// Строка url к файлу в GIT
        /// </summary>
        public string URLFile { get; set; }

        /// <summary>Проект GIT</summary>
        public string Project { get; set; }

        /// <summary>Каталог проекта GIT</summary>
        public string ProjectFolder
        {
            get
            {
                return Utilities.GITProjects.GetFolderByProject(Project);
            }
        }

        /// <summary>Ветка GIT</summary>
        public string Branch { get; set; }

        /// <summary>Папка, где лежат YML-файлы</summary>
        public string Path { get; set; }

        /// <summary>Имя YML-файла</summary>
        public string Filename { get; set; }
    }
}
