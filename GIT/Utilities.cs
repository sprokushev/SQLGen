// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using SQLGen.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace SQLGen.Utilities
{
    /// <summary>
    /// Работа с проектами GIT
    /// </summary>
    public static class GITProjects
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список возможных типов скриптов и соответствующих им папок в проекте GIT</summary>
        public static Dictionary<string, string> List_ScriptType_GITType = new Dictionary<string, string>
            {
            { "alter", "TABLE" },
            { "create", "TABLE" },
            { "table", "TABLE" },
            { "drop", "TABLE" },
            { "index", "TABLE" },
            { "idx", "TABLE" },
            { "struct", "TABLE" },
            { "trigger", "TRIGGER" },
            { "schema", "SCHEMA" },
            { "update", "DATA" },
            { "insert", "DATA" },
            { "delete", "DATA" },
            { "upsert", "DATA" },
            { "merge", "DATA" },
            { "bulk", "DATA" },
            { "copy", "DATA" },
            { "data", "DATA" },
            { "view", "VIEW" },
            { "proc", "PROCEDURE" },
            { "procedure", "PROCEDURE" },
            { "func", "FUNCTION" },
            { "function", "FUNCTION" },
            { "sequence", "SEQUENCE" },
            { "seq", "SEQUENCE" },
            { "type", "TYPE" }
            };

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Проекты GIT</summary>
        public static DataTable ListGITProjects;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Значение параметра проекта GIT
        /// </summary>
        /// <param name="columnfilter">имя искомого параметра</param>
        /// <param name="filtervalue">искомое значение</param>
        /// <param name="columnresult">имя параметра, чье значение надо вернуть</param>
        /// <returns></returns>
        public static string GITProjectsParam(string columnfilter, string filtervalue, string columnresult)
        {

            string res = "";
            if (columnfilter == null) columnfilter = "";
            if (columnresult == null) columnresult = "";
            if (filtervalue == null) filtervalue = "";

            if (
                (columnfilter != "GITProject") &&
                (columnfilter != "GITProjectFolder") &&
                (columnfilter != "PrefixFileSQL") &&
                (columnfilter != "PrefixFileRelease") &&
                (columnfilter != "PostfixFileRelease") &&
                (columnfilter != "GITUrl") &&
                (columnfilter != "GITUrlAlt") &&
                (columnfilter != "GITYMLField") &&
                (columnfilter != "GITDataFolder") &&
                (columnfilter != "GITisSingleScript") &&
                (columnfilter != "DBType") &&
                (columnfilter != "DEVProject") &&
                (columnfilter != "DEVProjectFolder") &&
                (columnfilter != "DEVUrl") &&
                (columnfilter != "DEVUrlAlt") &&
                (columnfilter != "DEVYMLField") &&
                (columnfilter != "DEVDataFolder") &&
                (columnfilter != "DEVisSingleScript") &&
                (columnfilter != "DEVisSingleScriptStruct") &&
                (columnfilter != "DEVisSingleScriptCode") &&
                (columnfilter != "DEVisSingleScriptData") &&
                (columnfilter != "DEVStartVer") &&
                (columnfilter != "CumulativeGap") &&
                (columnfilter != "isEvnInherit") &&
                (columnfilter != "DBAlias") &&
                (columnfilter != "LuquibotAliasOld") &&
                (columnfilter != "LuquibotAliasOldUfa") &&
                (columnfilter != "LuquibotAliasSP") &&
                (columnfilter != "LuquibotAliasSPUfa") &&
                (columnfilter != "LuquibotAliasHF") &&
                (columnfilter != "LuquibotAliasHFUfa") &&
                (columnfilter != "LuquibotAliasEHFAct") &&
                (columnfilter != "LuquibotAliasEHFActUfa") &&
                (columnfilter != "LuquibotAliasEHFUnAct") &&
                (columnfilter != "LuquibotAliasEHFUnActUfa") &&
                (columnfilter != "LuquibotAliasLTS") &&
                (columnfilter != "LuquibotAliasLTSUfa") &&
                (columnfilter != "ProjectDeploymentMS") &&
                (columnfilter != "ProjectDeploymentPG") &&
                (columnfilter != "ProjectCronMS") &&
                (columnfilter != "ProjectCronPG")

                )
            {
                App.AddLog("Ошибка в программе - неизвестный параметр проекта GIT " + columnfilter, null, App.ShowMessageMode.SHOW, true, null);
                return res;
            }

            if (
                (columnresult != "GITProject") &&
                (columnresult != "GITProjectFolder") &&
                (columnresult != "PrefixFileSQL") &&
                (columnresult != "PrefixFileRelease") &&
                (columnresult != "PostfixFileRelease") &&
                (columnresult != "GITUrl") &&
                (columnresult != "GITUrlAlt") &&
                (columnresult != "GITYMLField") &&
                (columnresult != "GITDataFolder") &&
                (columnresult != "GITisSingleScript") &&
                (columnresult != "DBType") &&
                (columnresult != "DEVProject") &&
                (columnresult != "DEVProjectFolder") &&
                (columnresult != "DEVUrl") &&
                (columnresult != "DEVUrlAlt") &&
                (columnresult != "DEVYMLField") &&
                (columnresult != "DEVDataFolder") &&
                (columnresult != "DEVisSingleScript") &&
                (columnresult != "DEVisSingleScriptStruct") &&
                (columnresult != "DEVisSingleScriptCode") &&
                (columnresult != "DEVisSingleScriptData") &&
                (columnresult != "DEVStartVer") &&
                (columnresult != "CumulativeGap") &&
                (columnresult != "isEvnInherit") &&
                (columnresult != "DBAlias") &&
                (columnresult != "LuquibotAliasOld") &&
                (columnresult != "LuquibotAliasOldUfa") &&
                (columnresult != "LuquibotAliasSP") &&
                (columnresult != "LuquibotAliasSPUfa") &&
                (columnresult != "LuquibotAliasHF") &&
                (columnresult != "LuquibotAliasHFUfa") &&
                (columnresult != "LuquibotAliasEHFAct") &&
                (columnresult != "LuquibotAliasEHFActUfa") &&
                (columnresult != "LuquibotAliasEHFUnAct") &&
                (columnresult != "LuquibotAliasEHFUnActUfa") &&
                (columnresult != "LuquibotAliasLTS") &&
                (columnresult != "LuquibotAliasLTSUfa") &&
                (columnresult != "ProjectDeploymentMS") &&
                (columnresult != "ProjectDeploymentPG") &&
                (columnresult != "ProjectCronMS") &&
                (columnresult != "ProjectCronPG")
                )
            {
                App.AddLog("Ошибка в программе - неизвестный параметр проекта GIT " + columnresult, null, App.ShowMessageMode.SHOW, true, null);
                return res;
            }

            if (string.IsNullOrWhiteSpace(filtervalue))
            {
                return res;
            }

            bool old = ListGITProjects.CaseSensitive;

            try
            {
                if (
                    (columnfilter == "GITProjectFolder") ||
                    (columnfilter == "DEVProjectFolder")
                    )
                {
                    ListGITProjects.CaseSensitive = false;
                }

                foreach (var row in ListGITProjects.Select(columnfilter + " = '" + filtervalue.Trim() + "'"))
                {
                    res = (string)row[columnresult];
                    // позвращаем первое значение
                    break; 
                }
            }
            catch
            {
            }

            ListGITProjects.CaseSensitive = old;

            if (res == null) res = "";
            return res.Trim();
        }

        /// <summary>
        /// Проект GIT - "старый"
        /// </summary>
        /// <param name="project">проект</param>
        /// <returns></returns>
        public static bool IsGITProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return false;
            return !string.IsNullOrWhiteSpace(GITProjectsParam("GITProject", project, "GITProject"));
        }

        /// <summary>
        /// Вернуть "старый" проект GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetGITProject(string project)
        {
            if (IsGITProject(project))
            {
                return project;
            }
            else
            {
                return GITProjectsParam("DEVProject", project, "GITProject");
            }
        }

        /// <summary>
        /// Проект DEV - "новый"
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static bool IsDEVProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return false;
            return !string.IsNullOrWhiteSpace(GITProjectsParam("DEVProject", project, "DEVProject"));
        }

        /// <summary>
        /// Вернуть "новый" проект GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetDEVProject(string project)
        {
            if (IsDEVProject(project))
            {
                return project;
            }
            else
            {
                return GITProjectsParam("GITProject", project, "DEVProject");
            }
        }

        /// <summary>
        /// Вернуть проект GIT, если он существует
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetProjectByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            string result = Utilities.GITProjects.GITProjectsParam("GITProject", project, "GITProject");
            if (string.IsNullOrWhiteSpace(result))
            {
                result = Utilities.GITProjects.GITProjectsParam("DEVProject", project, "DEVProject");
            }

            return result;
        }

        /// <summary>
        /// Вернуть папку проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetFolderByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "GITProjectFolder");
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "DEVProjectFolder");
            return "";
        }

        /// <summary>
        /// Вернуть URL для проекта GIT (включая явное имя ветки или маску имени ветки %BRANCH%)
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetURLByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "GITUrl");
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "DEVUrl");
            return "";
        }

        /// <summary>
        /// Вернуть URL до папки task в проекте GIT (включая явное имя ветки или маску имени ветки %BRANCH%)
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetURLTaskByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            string url = GetURLByProject(project);

            return url + "task/";
        }

        /// <summary>
        /// Вернуть URL до папки version в проекте GIT (включая явное имя ветки или маску имени ветки %BRANCH%)
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetURLVersionByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            string url = GetURLByProject(project);

            return url + "version/";
        }

        /// <summary>
        /// Вернуть URL до папки deployment в проекте GIT (включая явное имя ветки или маску имени ветки %BRANCH%)
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetURLDeploymentByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            string url = GetURLByProject(project);

            return url + "deployment/";
        }

        /// <summary>
        /// Вернуть URL до папки cron в проекте GIT (включая явное имя ветки или маску имени ветки %BRANCH%)
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetURLCronByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            string url = GetURLByProject(project);

            return url + "cron/";
        }

        /// <summary>
        /// Преобразовать путь к файлу в локальной папке GIT в url в web-репозитории
        /// </summary>
        /// <param name="project">проект</param>
        /// <param name="filepath">путь к файлу в локальной папке проекта</param>
        /// <param name="branch">ветка в проекте GIT</param>
        /// <param name="logFile">лог-файл</param>
        /// <returns></returns>
        public static string ConvertFilepathToUrl(string project, string filepath, string branch, string logFile)
        {
            string result = "";

            // путь к локальной папке проекта
            string folder = Utilities.GITProjects.GetFolderByProject(project);
            folder = Path.Combine(MainWindow.APPinfo.GITFolder, folder);

            if (
                Utilities.GITProjects.IsDEVProject(project) &&
                string.IsNullOrWhiteSpace(branch)
            )
            {
                // текущая ветка в локальной папке проекта
                string err = "";
                branch = GIT.GitCurrentBranch(project, out err, logFile);
                if (string.IsNullOrWhiteSpace(branch))
                {
                    App.AddLog("У проекта " + project + " не определилась ветка!" + Environment.NewLine
                        + Environment.NewLine + err, null, App.ShowMessageMode.SHOW, true, logFile);

                    return result;
                }
            }

            // url проекта, включая имя текущей ветки
            string baseurl = GetURLByProject(project);
            baseurl = baseurl.Replace("%BRANCH%", branch);

            // собираем итоговый url
            if (filepath.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
            {
                result = baseurl +
                    Regex.Replace(filepath, folder.Replace(@"\", @"\\"), "", RegexOptions.IgnoreCase)
                    .TrimStart(new char[] { Path.DirectorySeparatorChar });

                result = result
                    .Replace(Path.DirectorySeparatorChar,'/');
            }

            return result;
        }

        /// <summary>
        /// Преобразовать url в web-репозитории в путь к файлу в локальной папке GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <param name="url">url в web-репозитории</param>
        /// <returns></returns>
        public static string ConvertUrlToFilepath(string project, string url)
        {
            string result = "";

            // костыль
            url = url.Replace("//blob/", "/-/blob/");
            url = url.Replace("//tree/", "/-/tree/");

            // url проекта, без имени ветки
            string baseurl = GetURLByProject(project);
            baseurl = baseurl.Replace("%BRANCH%/", "");

            // путь к локальной папке проекта
            string folder = Utilities.GITProjects.GetFolderByProject(project);
            folder = Path.Combine(MainWindow.APPinfo.GITFolder, folder);

            // собираем итоговый путь к файлу в локальной папке проекта
            if (url.StartsWith(baseurl, StringComparison.OrdinalIgnoreCase))
            {
                // убираем url проекта
                url = Regex.Replace(url, baseurl, "", RegexOptions.IgnoreCase)
                    .Replace('/', Path.DirectorySeparatorChar);

                // убираем имя ветки
                var arr = url.Split(Path.DirectorySeparatorChar);
                url = "";
                for (int i = 1; i < arr.Length; i++)
                {
                    url = url + Path.DirectorySeparatorChar + arr[i];
                }

                result = folder + url;
            }

            return result;
        }

        /// <summary>
        /// Вернуть префикс версии для проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetPrefixFileReleaseByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "PrefixFileRelease").ToLower();
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "PrefixFileRelease").ToLower();
            return "";
        }

         /// <summary>
        /// Вернуть Проект для хранения действий при обновлении
        /// </summary>
        /// <param name="dbregion">тип основной БД в регионе</param>
        /// <param name="prefix_or_project">префикс версии или проект</param>
        /// <returns></returns>
        public static string GetProjectDeployment(string dbregion, string prefix_or_project)
        {
            if (string.IsNullOrWhiteSpace(prefix_or_project)) return "";

            string res = null;

            // если prefix_or_project - префикс
            foreach (var row in ListGITProjects.Select("PrefixFileRelease = '" + prefix_or_project.Trim() + "'"))
            {
                res = null;
                try
                {
                    if (
                        dbregion == "MS SQL" &&
                        row["ProjectDeploymentMS"] != DBNull.Value
                    )
                    {
                        res = (string)row["ProjectDeploymentMS"];
                    }

                    if (
                        dbregion == "PG SQL" &&
                        row["ProjectDeploymentPG"] != DBNull.Value
                    )
                    {
                        res = (string)row["ProjectDeploymentPG"];
                    }
                }
                catch
                {
                }

                // позвращаем первое не пустое значение
                if (!string.IsNullOrWhiteSpace(res))
                {
                    break;
                }
            }

            // если prefix_or_project - проект
            if (
                dbregion == "MS SQL" &&
                string.IsNullOrWhiteSpace(res)
            )
            {
                res = GITProjectsParam("DEVProject", prefix_or_project, "ProjectDeploymentMS");
            }
            if (
                dbregion == "PG SQL" &&
                string.IsNullOrWhiteSpace(res)
            )
            {
                res = GITProjectsParam("DEVProject", prefix_or_project, "ProjectDeploymentPG");
            }

            if (
                dbregion == "MS SQL" &&
                string.IsNullOrWhiteSpace(res)
            )
            {
                res = GITProjectsParam("GITProject", prefix_or_project, "ProjectDeploymentMS");
            }
            if (
                dbregion == "PG SQL" &&
                string.IsNullOrWhiteSpace(res)
            )
            {
                res = GITProjectsParam("GITProject", prefix_or_project, "ProjectDeploymentPG");
            }

            if (string.IsNullOrWhiteSpace(res))
            {
                res = "";
            }

            return res.Trim().ToLower();
        }

        /// <summary>
        /// Вернуть Проект для хранения заданий 
        /// </summary>
        /// <param name="dbregion">тип основной БД в регионе</param>
        /// <param name="prefix_or_project">префикс версии или проект или DBAlias</param>
        /// <returns></returns>
        public static string GetProjectCron(string dbregion, string prefix_or_project)
        {
            if (string.IsNullOrWhiteSpace(prefix_or_project)) return "";

            string res = null;

            // если prefix_or_project - префикс
            foreach (var row in ListGITProjects.Select("PrefixFileRelease = '" + prefix_or_project.Trim() + "'"))
            {
                res = null;
                try
                {
                    if (
                        dbregion == "MS SQL" &&
                        row["ProjectCronMS"] != DBNull.Value
                    )
                    {
                        res = (string)row["ProjectCronMS"];
                    }

                    if (
                        dbregion == "PG SQL" &&
                        row["ProjectCronPG"] != DBNull.Value
                    )
                    {
                        res = (string)row["ProjectCronPG"];
                    }
                }
                catch
                {
                }

                // позвращаем первое не пустое значение
                if (!string.IsNullOrWhiteSpace(res))
                {
                    break;
                }
            }

            // если prefix_or_project - DBAlias
            foreach (var row in ListGITProjects.Select("DBAlias = '" + prefix_or_project.Trim() + "'"))
            {
                res = null;
                try
                {
                    if (
                        dbregion == "MS SQL" &&
                        row["ProjectCronMS"] != DBNull.Value
                    )
                    {
                        res = (string)row["ProjectCronMS"];
                    }

                    if (
                        dbregion == "PG SQL" &&
                        row["ProjectCronPG"] != DBNull.Value
                    )
                    {
                        res = (string)row["ProjectCronPG"];
                    }
                }
                catch
                {
                }

                // позвращаем первое не пустое значение
                if (!string.IsNullOrWhiteSpace(res))
                {
                    break;
                }
            }

            // если prefix_or_project - проект
            if (
                dbregion == "MS SQL" && 
                string.IsNullOrWhiteSpace(res)
            )
            {
                res = GITProjectsParam("DEVProject", prefix_or_project, "ProjectCronMS");
            }
            if (
                dbregion == "PG SQL" &&
                string.IsNullOrWhiteSpace(res)
            )
            {
                res = GITProjectsParam("DEVProject", prefix_or_project, "ProjectCronPG");
            }

            if (
                dbregion == "MS SQL" &&
                string.IsNullOrWhiteSpace(res)
            )
            {
                res = GITProjectsParam("GITProject", prefix_or_project, "ProjectCronMS");
            }
            if (
                dbregion == "PG SQL" &&
                string.IsNullOrWhiteSpace(res)
            )
            {
                res = GITProjectsParam("GITProject", prefix_or_project, "ProjectCronPG");
            }

            if (string.IsNullOrWhiteSpace(res))
            {
                res = "";
            }

            return res.Trim().ToLower();
        }

        /// <summary>
        /// Вернуть постфикс версии для проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetPostfixFileReleaseByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "PostfixFileRelease");
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "PostfixFileRelease");
            return "";
        }

        /// <summary>
        /// Вернуть префикс SQL для проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetPrefixFileSQLByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "PrefixFileSQL");
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "PrefixFileSQL");
            return "";
        }


        /// <summary>
        /// Вернуть имя поля YMLField для проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetYMLFieldByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "GITYMLField");
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "DEVYMLField");
            return "";
        }

        /// <summary>
        /// Вернуть тип БД для проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetDBTypeByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "DBType").ToUpper();
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "DBType").ToUpper();
            return "";
        }

        /// <summary>
        /// Вернуть алиас БД для проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetDBAliasByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "DBAlias").ToLower();
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "DBAlias").ToLower();
            return "";
        }

        /// <summary>
        /// Вернуть алиас бота для проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <param name="type">тип алиаса</param>
        /// <returns></returns>
        public static string GetLuquibotAliasByProject(string project, string type)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, type);
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, type);
            return "";
        }

        /// <summary>
        /// Вернуть папку для данных проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetDataFolderByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "GITDataFolder");
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "DEVDataFolder");
            return "";
        }

        /// <summary>
        /// Вернуть значение флага "Один объект-один скрипт" для таблиц, схем, сиквенсов, индексов, типов проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static bool GetisSingleScriptStructByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return false;

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "GITisSingleScript").ToUpper() == "YES";
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "DEVisSingleScriptStruct").ToUpper() == "YES";
            return false;
        }

        /// <summary>
        /// Вернуть значение флага "Один объект-один скрипт" для процедур, функций, вьюх, триггеров проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static bool GetisSingleScriptCodeByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return false;

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "GITisSingleScript").ToUpper() == "YES";
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "DEVisSingleScriptCode").ToUpper() == "YES";
            return false;
        }

        /// <summary>
        /// Вернуть значение флага "Один объект-один скрипт" для данных проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static bool GetisSingleScriptDataByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return false;

            if (IsGITProject(project)) return false;
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "DEVisSingleScriptData").ToUpper() == "YES";
            return false;
        }

        /// <summary>
        /// Вернуть проект GIT для поля YMLField
        /// </summary>
        /// <param name="YMLField">поле YMLField</param>
        /// <returns></returns>
        public static string GetProjectByYMLField(string YMLField)
        {
            if (string.IsNullOrWhiteSpace(YMLField)) return "";

            string project = Utilities.GITProjects.GITProjectsParam("GITYMLField", YMLField, "GITProject");
            if (string.IsNullOrWhiteSpace(project))
            {
                project = Utilities.GITProjects.GITProjectsParam("DEVYMLField", YMLField, "DEVProject");
            }
            return project;
        }

        /// <summary>
        /// Вернуть проект GIT для папки 
        /// </summary>
        /// <param name="folder">папка с клонированным проектом GIT</param>
        /// <returns></returns>
        public static string GetProjectByFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) return "";

            string project = Utilities.GITProjects.GITProjectsParam("GITProjectFolder", folder, "GITProject");
            if (string.IsNullOrWhiteSpace(project))
            {
                project = Utilities.GITProjects.GITProjectsParam("DEVProjectFolder", folder, "DEVProject");
            }
            return project;
        }

        /// <summary>
        /// Вернуть номер версии для сортировки, с которой начался разрыв кумулятивности для текущей версии
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <param name="current_version">текущая версия</param>
        /// <returns></returns>
        public static double GetFirstVersionOrderByProject(string project, string current_version)
        {
            if (string.IsNullOrWhiteSpace(project)) return 0;
            if (string.IsNullOrWhiteSpace(current_version)) return 0;

            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, current_version));

            string txt = "";
            if (IsGITProject(project)) txt = Utilities.GITProjects.GITProjectsParam("GITProject", project, "CumulativeGap");
            if (IsDEVProject(project)) txt = Utilities.GITProjects.GITProjectsParam("DEVProject", project, "CumulativeGap");

            var list = txt.ToList(new char[] { ',', ';' }, true);

            foreach (var item in list
                .Where(x => Release.VerAsNum(x) <= numversion)
                .OrderByDescending(x => Release.VerAsNum(x)))
            {
                return Release.VerAsNum(item);
            }
            return 0;
        }

        /// <summary>
        /// Вернуть Номер версии, с которой начали собирать релизы в проекте DEV
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static string GetDEVStartVerByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return "";

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "DEVStartVer");
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "DEVStartVer");
            return "";
        }

        /// <summary>
        /// Вернуть значение флага "Таблицы событий используют наследование" для данных проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static bool GetisEvnInheritByProject(string project)
        {
            if (string.IsNullOrWhiteSpace(project)) return false;

            if (IsGITProject(project)) return GITProjectsParam("GITProject", project, "isEvnInherit").ToUpper() == "YES";
            if (IsDEVProject(project)) return GITProjectsParam("DEVProject", project, "isEvnInherit").ToUpper() == "YES";
            return false;
        }
    }

    /// <summary>
    /// Работа с GIT
    /// </summary>
    public static class GIT
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Переключиться или создать ветку (текущая задача) для указанных проектов
        /// </summary>
        /// <param name="projects">Список проектов</param>
        /// <param name="BranchName">Ветка</param>
        /// <param name="ParentBranch">Родительская ветка, от которой надо создать новую ветку</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static void GitNewBranch(string[] projects, string BranchName, string ParentBranch, string logFile)
        {
            if (projects == null) return;
            if (projects.Length == 0) return;

            if (string.IsNullOrWhiteSpace(BranchName))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            WinExecute WinExecute = new WinExecute(logFile);
            WinExecute.Title = "Создаем или переключаемся на ветку " + BranchName;

            for (int i = 0; i < projects.Length; i++)
            {
                string ProjectFolder = Utilities.GITProjects.GITProjectsParam("DEVProject", projects[i], "DEVProjectFolder");

                if (!string.IsNullOrWhiteSpace(ProjectFolder))
                {
                    // "новый" проект
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                    if (folder.Contains(" ")) folder = "\"" + folder + "\"";
                    string err = "";
                    string branch = GitCurrentBranch(projects[i], out err, logFile);
                    string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(projects[i]);
                    string noupper = "";

                    if (
                            BranchName.ToUpper().StartsWith(prefix.ToUpper() + ".") ||
                            MainWindow.APPinfo.NoUpperBranch.Contains(BranchName, StringComparer.OrdinalIgnoreCase)
                        )
                    {
                        noupper = "NOUPPERCASE";
                    }

                    // проверить текущую ветку
                    if (string.IsNullOrWhiteSpace(branch))
                    {
                        App.AddLog("У проекта " + projects[i] + " не определилась ветка!" + Environment.NewLine
                        + Environment.NewLine +
                        err, null, App.ShowMessageMode.SHOW, true, logFile);
                    }
                    else if (branch == BranchName)
                    {
                        // ветка уже выбрана, делаем git pull
                        WinExecute.AddCommand(
                            App.AppPath,
                            Path.Combine(App.AppPath, "git_switch.cmd"),
                            folder + " " + BranchName + " " + noupper
                            );
                    }
                    else
                    {
                        // ветка НЕ выбрана, создаем ветку или переключаемся на нее
                        WinExecute.AddCommand(
                            App.AppPath,
                            Path.Combine(App.AppPath, "git_newbranch.cmd"),
                            folder + " " + BranchName + " " + ParentBranch + " " + noupper
                        );
                    }
                }
                else
                {
                    // "старый" проект
                    ProjectFolder = Utilities.GITProjects.GITProjectsParam("GITProject", projects[i], "GITProjectFolder");
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                    if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                    WinExecute.AddCommand(
                        App.AppPath,
                        Path.Combine(App.AppPath, "git_pull.cmd"),
                        folder
                    );
                }
            }

            if (WinExecute.ListCommands.Count > 0)
            {
                App.AddLog($"Сейчас переключимся или создадим ветку {BranchName}", null, App.ShowMessageMode.NONE, true, logFile);

                WinExecute.Start(true);
            }
            else
            {
                WinExecute.Close();
            }
        }

        // время последнего запуска git-refresh.sh для проектов
        private static Dictionary<string, DateTime> lastCall = new Dictionary<string, DateTime>();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// выполнить git-refresh.sh
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        public static void GitRefresh(string project, string logFile, bool isForcedGitRefresh)
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            if (!isForcedGitRefresh)
            {
                if (lastCall.ContainsKey(project))
                {
                    // проверим время последнего запуска и если еще рано - выходим
                    var lastDT = lastCall[project];
                    if (lastDT >= DateTime.Now.AddMinutes(-MainWindow.APPinfo.GitRefreshDelay))
                    {
                        return;
                    }
                }
            }

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            if (!string.IsNullOrWhiteSpace(ProjectFolder))
            {
                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                // скачать ветки
                if (File.Exists(Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "git-refresh.sh")))
                {
                    App.AddLog($"Сейчас вызовем git-refresh.sh для проекта {project}", null, App.ShowMessageMode.NONE, true, logFile);

                    Utilities.External.ExecuteFile(
                            App.AppPath,
                            Path.Combine(App.AppPath, "git_runsh.cmd"),
                            folder + " git-refresh.sh",
                            true,
                            false,
                            false,
                            false,
                            logFile
                        );

                    App.AddLog($"git-refresh.sh выполнен", null, App.ShowMessageMode.NONE, true, logFile);

                    // обновим время последнего запуска
                    if (!lastCall.ContainsKey(project))
                    {
                        lastCall.Add(project, DateTime.Now);
                    }
                    else
                    {
                        lastCall[project] = DateTime.Now;
                    }
                }
                else
                {
                    // получим текущую ветку
                    string branch = GitCurrentBranch(project, out string error, logFile);

                    App.AddLog($"Ветка {branch} в проекте {project} устарела - в ней отсутствует git-refresh.sh", null, App.ShowMessageMode.SHOW, true, logFile);
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выполнить GIT FETCH --all для указанных проектов
        /// </summary>
        /// <param name="projects">Список проектов</param>
        /// <param name="isShowAllErrors">Выводить все сообщения об ошибках</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static void GitFetchAll(string[] projects, bool isShowAllErrors, string logFile)
        {
            if (projects == null) return;
            if (projects.Length == 0) return;

            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            WinExecute WinExecute = new WinExecute(logFile);
            WinExecute.Title = "Выполняем GIT FETCH --all";
            WinExecute.isShowAllErrors = isShowAllErrors;

            for (int i = 0; i < projects.Length; i++)
            {
                string ProjectFolder = Utilities.GITProjects.GetFolderByProject(projects[i]);

                if (!string.IsNullOrWhiteSpace(ProjectFolder))
                {

                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                    if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                    WinExecute.AddCommand(
                                    App.AppPath,
                                    Path.Combine(App.AppPath, "git_fetchall.cmd"),
                                    folder
                                    );
                }
            }

            if (WinExecute.ListCommands.Count > 0)
            {
                App.AddLog($"Сейчас выполним git fetch --all", null, App.ShowMessageMode.NONE, true, logFile);

                WinExecute.Start(true);
            }
            else
            {
                WinExecute.Close();
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выполнить GIT PULL для указанных проектов и для текущей задачи
        /// </summary>
        /// <param name="projects">Список проектов</param>
        /// <param name="BranchName">Ветка</param>
        /// <param name="isShowAllErrors">Выводить все сообщения об ошибках</param>
        /// <param name="isGitRefresh">=true добавить выполнение git-refresh.sh</param>
        /// <param name="isOnlyPull">=true выполнение git pull без переключения ветки</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        public static void GitPull(string[] projects, string BranchName, bool isShowAllErrors, bool isGitRefresh, bool isOnlyPull, string logFile, bool isForcedGitRefresh)
        {
            if (projects == null) return;
            if (projects.Length == 0) return;

            if (string.IsNullOrWhiteSpace(BranchName))
            {
                BranchName = "";
            }
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            WinExecute WinExecute = new WinExecute(logFile);
            WinExecute.Title = "Выполняем GIT PULL";
            WinExecute.isShowAllErrors = isShowAllErrors;

            for (int i = 0; i < projects.Length; i++)
            {
                string ProjectFolder = Utilities.GITProjects.GITProjectsParam("GITProject", projects[i], "GITProjectFolder");

                if (!string.IsNullOrWhiteSpace(ProjectFolder))
                {
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                    if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                    WinExecute.AddCommand(
                        App.AppPath,
                        Path.Combine(App.AppPath, "git_pull.cmd"),
                        folder
                    );
                }
                else
                {
                    ProjectFolder = Utilities.GITProjects.GITProjectsParam("DEVProject", projects[i], "DEVProjectFolder");

                    if (!string.IsNullOrWhiteSpace(ProjectFolder))
                    {
                        string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                        if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                        string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(projects[i]);
                        string noupper = "";

                        if (
                                BranchName.ToUpper().StartsWith(prefix.ToUpper() + ".") ||
                                MainWindow.APPinfo.NoUpperBranch.Contains(BranchName, StringComparer.OrdinalIgnoreCase)
                            )
                        {
                            noupper = "NOUPPERCASE";
                        }

                        if (isGitRefresh)
                        {
                            GitRefresh(projects[i], logFile, isForcedGitRefresh);

                            if (
                                (BranchName == "master") &&
                                (Utilities.GIT.GitCurrentBranch(projects[i], out string err, logFile) == "master")
                            )
                            {
                                continue;
                            }
                        }

                        if (isOnlyPull)
                        {
                            WinExecute.AddCommand(
                                App.AppPath,
                                Path.Combine(App.AppPath, "git_pull.cmd"),
                                folder
                            );
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(BranchName))
                            {
                                WinExecute.AddCommand(
                                    App.AppPath,
                                    Path.Combine(App.AppPath, "git_switch.cmd"),
                                    folder + " " + BranchName + " " + noupper
                                );
                            }
                        }
                    }
                }
            }

            if (WinExecute.ListCommands.Count > 0)
            {
                App.AddLog($"Сейчас выполним git pull ветки {BranchName}", null, App.ShowMessageMode.NONE, true, logFile);

                WinExecute.Start(true);
            }
            else
            {
                WinExecute.Close();
            }
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выполнить GIT PUSH для указанных проектов и для указанной ветки
        /// </summary>
        /// <param name="projects">Список проектов</param>
        /// <param name="BranchName">Ветка</param>
        /// <param name="isShowAllErrors">Выводить все сообщения об ошибках</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static void GitPush(string[] projects, string BranchName, bool isShowAllErrors, string logFile)
        {
            if (projects == null) return;
            if (projects.Length == 0) return;

            if (string.IsNullOrWhiteSpace(BranchName))
            {
                BranchName = "";
            }
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            WinExecute WinExecute = new WinExecute(logFile);
            WinExecute.Title = "Выполняем GIT PUSH";
            WinExecute.isShowAllErrors = isShowAllErrors;

            for (int i = 0; i < projects.Length; i++)
            {
                string ProjectFolder = Utilities.GITProjects.GetFolderByProject(projects[i]);
                if (!string.IsNullOrWhiteSpace(ProjectFolder))
                {
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                    if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                    string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(projects[i]);
                    string noupper = "";

                    if (
                            BranchName.ToUpper().StartsWith(prefix.ToUpper() + ".") ||
                            MainWindow.APPinfo.NoUpperBranch.Contains(BranchName, StringComparer.OrdinalIgnoreCase)
                        )
                    {
                        noupper = "NOUPPERCASE";
                    }

                    if (!string.IsNullOrWhiteSpace(BranchName))
                    {
                        WinExecute.AddCommand(
                            App.AppPath,
                            Path.Combine(App.AppPath, "git_push.cmd"),
                            folder + " " + BranchName + " " + noupper
                        );
                    }
                }
            }

            if (WinExecute.ListCommands.Count > 0)
            {
                App.AddLog($"Сейчас выполним git push ветки {BranchName}", null, App.ShowMessageMode.NONE, true, logFile);

                WinExecute.Start(true);
            }
            else
            {
                WinExecute.Close();
            }
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выполнить GIT MERGE
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="FromBranch">Ветка, которую вливаем</param>
        /// <param name="ToBranch">Ветка, в которую вливаем</param>
        /// <param name="isNoUppercase">=true - запустить с параметром NOUPPERCASE</param>
        /// <param name="isOrigin">=true - запустить с параметром ORIGIN</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в MainWindow.Task.LogFileMerge, если пустой, значит - в App.AppLogFile</param>
        /// <param name="askShowLog">=true - показывать лог</param>
        public static bool GitMerge(string project, string FromBranch, string ToBranch, bool isNoUppercase, bool isOrigin, string logFile, bool askShowLog)
        {
            if (string.IsNullOrWhiteSpace(project)) return false;
            if (string.IsNullOrWhiteSpace(FromBranch)) return false;
            if (string.IsNullOrWhiteSpace(ToBranch)) return false;
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = MainWindow.Task.LogFileMerge;
            }
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            // проверяем текущую ветку
            string err = "";
            string test = GIT.GitCurrentBranch(project, out err, logFile);
            if (test.ToLower() != ToBranch.ToLower())
            {
                App.AddLog($"Текущая ветка {test}, а должна быть {ToBranch}. Merge прерван!", null, App.ShowMessageMode.SHOW, true, logFile);
                return false;
            }

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
            if (folder.Contains(" ")) folder = "\"" + folder + "\"";

            WinExecute WinExecute = new WinExecute(logFile);

            string noUppercase = "";
            if (isNoUppercase) noUppercase = " NOUPPERCASE";

            string Origin = "";
            if (isOrigin) Origin = " ORIGIN";

            // Добавляем задание
            WinExecute.Title = $"Merge ветки {FromBranch} в ветку {ToBranch}";
            WinExecute.AddCommand(
                App.AppPath,
                Path.Combine(App.AppPath, "git_merge.cmd"),
                folder + " " + FromBranch + noUppercase + Origin
            );

            App.AddLog($"Сейчас выполним git merge ветки {FromBranch} в ветку {ToBranch} в проекте {project}", null, App.ShowMessageMode.NONE, true, logFile);

            // выполняем
            WinExecute.Start(true, -1, true);

            App.AddLog($"Git merge ветки {FromBranch} в ветку {ToBranch} в проекте {project} завершен", null, App.ShowMessageMode.NONE, true, logFile);

            // показываем
            if (
                askShowLog &&
                (System.Windows.Forms.MessageBox.Show("Посмотреть лог merge ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            )
            {
                WinInfo WinInfo = new WinInfo(null);
                WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                WinInfo.tbInfo.Text = WinExecute.GetLog();
                if (!string.IsNullOrWhiteSpace(WinExecute.execLogFile)) 
                {
                    WinInfo.Title = "Лог merge в файле " + WinExecute.execLogFile;
                }
                else
                {
                    WinInfo.Title = "Лог merge";
                }
                WinInfo.ShowDialog();
            }

            return true;
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выполнить GIT GREP для указанного проекта и для текущей ветки
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="path">папка внутри проекта</param>
        /// <param name="search">Искомая строка</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static List<string> GitGrep(string project, string path, string search, string logFile)
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(project)) return result;
            if (string.IsNullOrWhiteSpace(search)) return result;

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            if (!string.IsNullOrWhiteSpace(ProjectFolder))
            {
                App.AddLog($"Сейчас выполним git grep в текущей ветке в проекте {project}", null, App.ShowMessageMode.NONE, true, logFile);

                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                if (string.IsNullOrWhiteSpace(path))
                {
                    path = "";
                }
                else
                {
                    path = path.Replace(Path.DirectorySeparatorChar, '/');
                    if (!path.EndsWith("/"))
                    {
                        path = path + "/";
                    }
                }

                // собираем pattern для поиска
                string pattern = "";
                foreach (var item in search
                    .TrimInner()
                    .Split(' ')
                    .ToList()
                )
                {
                    if (!string.IsNullOrWhiteSpace(pattern))
                    {
                        pattern += " --and ";
                    }
                    pattern += "-e " + item;
                }

                // параметры команды git
                string param = $"-C {folder} --no-pager grep -i -I --name-only --full-name {pattern} -- {path}*.yml {path}*.sql";

                string list = Utilities.External.ExecuteFile(
                    App.AppPath,
                    "git",
                    param,
                    true,
                    false,
                    true,
                    false,
                    logFile,
                    600
                );

                if (string.IsNullOrWhiteSpace(list))
                {
                    list = "";
                }
                else
                {
                    list = list.TrimAllSpace();
                }

                if (
                    list.ToLower().StartsWith("ошибка") ||
                    list.ToLower().StartsWith("error") ||
                    list.ToLower().StartsWith("fatal")
                    )
                {
                    list = "";
                }

                list = list.Replace('\n', '|').Replace('\r', '|');

                App.AddLog($"Git grep выполнен", null, App.ShowMessageMode.NONE, true, logFile);

                // Заполнить result
                foreach (var item in list.Split('|').ToList())
                {
                    string file = item.Replace(Path.DirectorySeparatorChar, '/');
                    if (file.StartsWith("./"))
                    {
                        file = file.Substring(2);
                    }

                    if (
                        (!string.IsNullOrWhiteSpace(file)) &&
                        (!result.Contains(file))
                        )
                    {
                        result.Add(file);
                    }
                }
            }

            return result;
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выполнить поиск файлов по имени для указанного проекта и для текущей ветки
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="path">папка внутри проекта</param>
        /// <param name="search">Искомая строка</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static List<string> GitFind(string project, string path, string search, string logFile)
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(project)) return result;
            if (string.IsNullOrWhiteSpace(search)) return result;

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            if (!string.IsNullOrWhiteSpace(ProjectFolder))
            {
                App.AddLog($"Сейчас выполним поиск файлов по имени в текущей ветке в проекте {project}", null, App.ShowMessageMode.NONE, true, logFile);

                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                if (string.IsNullOrWhiteSpace(path))
                {
                    path = ".";
                }
                else
                {
                    path = path.Replace(Path.DirectorySeparatorChar, '/');
                    if (!path.StartsWith("./"))
                    {
                        path = "./" + path;
                    }
                }

                // получить список файлов, в имени которых есть искомая строка
                string list = Utilities.External.ExecuteFile(
                        App.AppPath,
                        Path.Combine(App.AppPath, "git_find.cmd"),
                        folder + " " + path + " " + search,
                        true,
                        false,
                        true,
                        false,
                        logFile,
                        600
                    );

                if (string.IsNullOrWhiteSpace(list))
                {
                    list = "";
                }

                list = list
                    .TrimAllSpace()
                    .Replace('\n', '|')
                    .Replace('\r', '|');

                App.AddLog($"Поиск выполнен", null, App.ShowMessageMode.NONE, true, logFile);

                // Заполнить result
                foreach (var item in list.Split('|').ToList())
                {
                    string file = item.Replace(Path.DirectorySeparatorChar, '/');
                    if (file.StartsWith("./"))
                    {
                        file = file.Substring(2);
                    }

                    if (
                        (!string.IsNullOrWhiteSpace(file)) &&
                        (!result.Contains(file))
                        )
                    {
                        result.Add(file);
                    }
                }
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Список веток
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="cmd">cmd-файл для получения списка веток</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        /// <param name="isFetchAll">=true - выполнить git fetch --all</param>
        public static List<string> GitListBranches(string project, string cmd, string logFile, bool isFetchAll)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrWhiteSpace(cmd))
            {
                return result;
            }

            if (string.IsNullOrWhiteSpace(project))
            {
                return result;
            }

            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            if (!string.IsNullOrWhiteSpace(ProjectFolder))
            {
                if (isFetchAll)
                {
                    // обновим проект
                    GitFetchAll(new string[] { project }, false, logFile);
                }

                App.AddLog($"Сейчас выполним {cmd} в проекте {project}", null, App.ShowMessageMode.NONE, true, logFile);

                // готовим параметры
                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                string prefix = "";
                if (cmd.ToLower() == "git_listversion.cmd")
                {
                    prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
                }

                // получить список веток
                string listbranches = Utilities.External.ExecuteFile(
                        App.AppPath,
                        Path.Combine(App.AppPath, cmd),
                        (folder + " " + prefix).Trim(),
                        true,
                        false,
                        true,
                        false,
                        logFile
                );

                if (string.IsNullOrWhiteSpace(listbranches))
                {
                    listbranches = "";
                }
                else
                {
                    listbranches = listbranches.TrimAllSpace();
                }

                if (
                    listbranches.ToLower().Contains("ошибка") ||
                    listbranches.ToLower().Contains("error") ||
                    listbranches.ToLower().Contains("fatal")
                )
                {
                    listbranches = "";
                }

                listbranches = listbranches.Replace('\n', '|').Replace('\r', '|');

                App.AddLog($"Список веток получен", null, App.ShowMessageMode.NONE, true, logFile);
                if (cmd == "git_listversion.cmd")
                {
                    App.AddLog($"Результат: {listbranches}", null, App.ShowMessageMode.NONE, true, logFile);
                }

                foreach (var item in listbranches.Split('|'))
                {
                    string branch = item.Replace("*", "").Trim().Split(' ')[0];

                    if (branch.StartsWith("origin/"))
                    {
                        branch = branch.Substring(7).Trim();
                    }

                    if (
                        !string.IsNullOrWhiteSpace(branch) &&
                        !result.Contains(branch)
                    )
                    {
                        result.Add(branch);
                    }
                }
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Последняя версия в ветке задачи
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="branch">Ветка задачи</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static string GitLastVersionInTask(string project, string branch, string logFile)
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            string result = "";

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            if (
                (!string.IsNullOrWhiteSpace(ProjectFolder)) &&
                (!string.IsNullOrWhiteSpace(branch))
            )
            {
                App.AddLog($"Сейчас получим последнюю версию в ветке {branch} в проекте {project}", null, App.ShowMessageMode.NONE, true, logFile);

                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";
                string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);

                // получить список веток
                string listbranches = Utilities.External.ExecuteFile(
                        App.AppPath,
                        Path.Combine(App.AppPath, "git_lastverintask.cmd"),
                        folder + " " + prefix + " " + branch,
                        true,
                        false,
                        true,
                        false,
                        logFile
                    );

                if (string.IsNullOrWhiteSpace(listbranches))
                {
                    listbranches = "";
                }
                else
                {
                    listbranches = listbranches.TrimAllSpace();
                }

                if (
                    listbranches.ToLower().Contains("ошибка") ||
                    listbranches.ToLower().Contains("error") ||
                    listbranches.ToLower().Contains("fatal")
                )
                {
                    listbranches = "";
                }

                listbranches = listbranches.Replace('\n', '|').Replace('\r', '|');

                App.AddLog($"Результат: {listbranches}", null, App.ShowMessageMode.NONE, true, logFile);

                foreach (var item in listbranches.Split('|'))
                {
                    string txt = item.Replace("*", "").Trim().Split(' ')[0];

                    if (txt.StartsWith("origin/"))
                    {
                        txt = txt.Substring(7).Trim();
                    }

                    if (project == "dev_userportal_pg" && txt == "rpms.9.20.0")
                    {
                        // игнорируем неправильную ветку
                        continue;
                    }


                    if (!string.IsNullOrWhiteSpace(txt))
                    {
                        result = txt;
                        break;
                    }
                }
            }

            return result;
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Дата последнего комита в ветке
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="branch">Ветка задачи</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static DateTime GitLastCommit(string project, string branch, string logFile)
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            DateTime result = DateTime.MinValue;

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            if (
                (!string.IsNullOrWhiteSpace(ProjectFolder)) &&
                (!string.IsNullOrWhiteSpace(branch))
            )
            {
                App.AddLog($"Сейчас получим дату последнего commit ветке {branch} в проекте {project}", null, App.ShowMessageMode.NONE, true, logFile);

                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                // получить список веток
                string _date = Utilities.External.ExecuteFile(
                        App.AppPath,
                        Path.Combine(App.AppPath, "git_lastcommit.cmd"),
                        folder + " " + branch,
                        true,
                        false,
                        true,
                        false,
                        logFile
                    );

                if (string.IsNullOrWhiteSpace(_date))
                {
                    _date = "";
                }
                else
                {
                    _date = _date.TrimAllSpace();
                }

                if (
                    _date.ToLower().Contains("ошибка") ||
                    _date.ToLower().Contains("error") ||
                    _date.ToLower().Contains("fatal")
                )
                {
                    _date = "";
                }

                App.AddLog($"Результат: {_date}", null, App.ShowMessageMode.NONE, true, logFile);

                if (DateTime.TryParseExact(_date, "yyyy-MM-dd", null, DateTimeStyles.None, out result))
                {
                    // успешная конвертация
                }
                else
                {
                    result = DateTime.MinValue;
                }
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Список веток, еще не влитых в master
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="isGitRefresh">=true - выполнить git-refresh.sh</param>
        /// <param name="listBadBranch">список проблемных веток версий</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        public static List<string> GitListNoMergedVersions(string project, bool isGitRefresh, out string listBadBranch, string logFile, bool isForcedGitRefresh)
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            List<string> result = new List<string>();
            listBadBranch = "";

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            if (!string.IsNullOrWhiteSpace(ProjectFolder))
            {
                if (isGitRefresh) GitRefresh(project, logFile, isForcedGitRefresh);

                App.AddLog($"Сейчас получим список веток еще не влитых в master в проекте {project}", null, App.ShowMessageMode.NONE, true, logFile);

                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";
                string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);

                // получить список веток
                string listbranches = Utilities.External.ExecuteFile(
                        App.AppPath,
                        Path.Combine(App.AppPath, "git_nomergedversion.cmd"),
                        folder + " " + prefix,
                        true,
                        false,
                        true,
                        false,
                        logFile
                    );

                if (string.IsNullOrWhiteSpace(listbranches))
                {
                    listbranches = "";
                }
                else
                {
                    listbranches = listbranches.TrimAllSpace();
                }

                if (
                    listbranches.ToLower().Contains("ошибка") ||
                    listbranches.ToLower().Contains("error") ||
                    listbranches.ToLower().Contains("fatal")
                )
                {
                    listbranches = "";
                }

                listbranches = listbranches.Replace('\n', '|').Replace('\r', '|');

                App.AddLog($"Результат: {listbranches}", null, App.ShowMessageMode.NONE, true, logFile);

                foreach (var item in listbranches.Split('|'))
                {
                    string txt = item.Replace("*", "").Trim();

                    if (txt.ToLower().StartsWith("origin/"))
                    {
                        txt = txt.Substring(7).Trim();
                    }

                    if (txt.ToLower().Contains("(not contain"))
                    {
                        listBadBranch += listBadBranch == "" ? txt : (Environment.NewLine + txt);
                    }

                    txt = txt.Split(' ')[0];

                    if (
                        !string.IsNullOrWhiteSpace(txt) &&
                        !result.Contains(txt)
                    )
                    {
                        result.Add(txt);
                    }
                }
            }

            return result;
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Определить текущую ветку проекта
        /// </summary>
        /// <param name="project">Поект</param>
        /// <param name="error">Текст ошибки</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static string GitCurrentBranch(string project, out string error, string logFile)
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            error = "";

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);

            if (!string.IsNullOrWhiteSpace(ProjectFolder))
            {
                App.AddLog($"Сейчас получим текущую ветку в проекте {project}", null, App.ShowMessageMode.NONE, true, logFile);

                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                // проверить текущую ветку
                string listbranches = Utilities.External.ExecuteFile(
                        App.AppPath,
                        Path.Combine(App.AppPath, "git_currentbranch.cmd"),
                        folder,
                        true,
                        false,
                        true,
                        false,
                        logFile
                    );

                if (string.IsNullOrWhiteSpace(listbranches))
                {
                    listbranches = "";
                }
                else
                {
                    listbranches = listbranches.TrimAllSpace();
                }

                App.AddLog($"Результат: {listbranches}", null, App.ShowMessageMode.NONE, true, logFile);

                if (
                    listbranches.ToLower().Contains("ошибка") ||
                    listbranches.ToLower().Contains("error") ||
                    listbranches.ToLower().Contains("fatal")
                    )
                {
                    error = listbranches;

                    return "";
                }

                listbranches = listbranches.Replace('\n', '|').Replace('\r', '|');

                string branch = "";

                foreach (var item in listbranches.Split('|'))
                {
                    string txt = item.Replace("*", "").Trim();

                    if (txt.ToLower().StartsWith("origin/"))
                    {
                        txt = txt.Substring(7).Trim();
                    }

                    txt = txt.Split(' ')[0];

                    if (!string.IsNullOrWhiteSpace(txt))
                    {
                        branch = txt;
                    }
                }

                App.AddLog($"Текущая ветка {branch}", null, App.ShowMessageMode.NONE, true, logFile);

                return branch;
            }
            else
            {
                return "";
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Переключиться на ветку
        /// </summary>
        /// <param name="project">Поект</param>
        /// <param name="branch">Ветка</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        /// <param name="current_branch">Текущая ветка после переключения</param>
        /// <param name="error">Ошибки при переключении ветки</param>
        public static bool GitSwitch(string project, string branch, string logFile, out string current_branch, out string error)
        {
            current_branch = "?";
            error = "";

            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);

            if (
                (!string.IsNullOrWhiteSpace(ProjectFolder)) &&
                (!string.IsNullOrWhiteSpace(branch))
                )
            {
                App.AddLog($"Сейчас переключим текущую ветку в проекте {project} на {branch}", null, App.ShowMessageMode.NONE, true, logFile);

                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
                string noupper = "";

                if (
                        branch.ToLower().StartsWith(prefix.ToLower() + ".") ||
                        MainWindow.APPinfo.NoUpperBranch.Contains(branch, StringComparer.OrdinalIgnoreCase)
                    )
                {
                    noupper = "NOUPPERCASE";
                }

                // переключить ветку
                error = Utilities.External.ExecuteFile(
                        App.AppPath,
                        Path.Combine(App.AppPath, "git_switch.cmd"),
                        folder + " " + branch + " " + noupper,
                        true,
                        true,
                        false,
                        true,
                        logFile
                    );

                if (string.IsNullOrWhiteSpace(error))
                {
                    error = "";
                }

                // проверить текущую ветку
                current_branch = GitCurrentBranch(project, out string err, logFile);
                if (string.IsNullOrWhiteSpace(err))
                {
                    err = "";
                }

                error = (error + Environment.NewLine + err).TrimAllSpace();

                return current_branch.ToLower() == branch.ToLower();
            }

            return false;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Выполнить GIT ADD/DIFF + COMMIT + PUSH + Merge Request для указанных проектов и для текущей задачи
        /// </summary>
        /// <param name="projects">Список проектов</param>
        /// <param name="BranchName">Ветка</param>
        /// <param name="isNoMergeRequest">Вместо MergeRequest используем push</param>
        /// <param name="isWait">=true ждать завершение</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static void GitAdd(string[] projects, string BranchName, bool isNoMergeRequest, bool isWait, string logFile)
        {
            if (projects == null) return;
            if (projects.Length == 0) return;

            if (string.IsNullOrWhiteSpace(BranchName))
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            WinExecute WinExecute = new WinExecute(logFile);
            WinExecute.Title = "Добавление в индекс GIT + COMMIT + PUSH + Merge Request для ветки " + BranchName;

            for (int i = 0; i < projects.Length; i++)
            {
                // "старый" проект
                string GITProjectFolder = Utilities.GITProjects.GITProjectsParam("GITProject", projects[i], "GITProjectFolder");

                if (!string.IsNullOrWhiteSpace(GITProjectFolder))
                {
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, GITProjectFolder);
                    if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                    WinExecute.AddCommand(
                        App.AppPath,
                        Path.Combine(App.AppPath, "tortoisegit.cmd"),
                        folder + " " + MainWindow.Task.TaskNumber
                    );
                }

                // "новый" проект
                string DEVProjectFolder = Utilities.GITProjects.GITProjectsParam("DEVProject", projects[i], "DEVProjectFolder");

                if (!string.IsNullOrWhiteSpace(DEVProjectFolder))
                {
                    string folder = Path.Combine(MainWindow.APPinfo.GITFolder, DEVProjectFolder);
                    if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                    string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(projects[i]);
                    string noupper = "";

                    if (
                            BranchName.ToUpper().StartsWith(prefix.ToUpper() + ".") ||
                            MainWindow.APPinfo.NoUpperBranch.Contains(BranchName, StringComparer.OrdinalIgnoreCase)
                        )
                    {
                        noupper = "NOUPPERCASE";
                    }

                    // проверить текущую ветку
                    string err = "";
                    string branch = GitCurrentBranch(projects[i], out err, logFile);

                    if (string.IsNullOrWhiteSpace(branch))
                    {
                        App.AddLog("У проекта " + projects[i] + " не определилась ветка!" + Environment.NewLine +
                            Environment.NewLine +
                            err,
                            null, App.ShowMessageMode.SHOW, true, logFile);
                    }
                    else
                    {
                        if (branch != BranchName)
                        {
                            // ветка другая, надо сменить
                            if (System.Windows.Forms.MessageBox.Show("Сейчас в проекте " + projects[i] + " текущая ветка " + branch + Environment.NewLine +
                                    Environment.NewLine +
                                    "Сменить ветку в проекте " + projects[i] + " на " + BranchName + " ?",
                                    "ВНИМАНИЕ",
                                    System.Windows.Forms.MessageBoxButtons.YesNo
                                ) == System.Windows.Forms.DialogResult.Yes
                            )
                            {
                                App.AddLog("Выбрана смена ветки в проекте " + projects[i] + " на " + BranchName, null, App.ShowMessageMode.NONE, true, logFile);

                                // надо создать/переключиться на ветку задачи
                                GitNewBranch(new string[] { projects[i] }, BranchName, "master", logFile);

                                // определяем текущую ветку после смены
                                branch = GitCurrentBranch(projects[i], out err, logFile);
                            }
                        }

                        if (branch != BranchName)
                        {
                            App.AddLog("Ошибка при отправке в GIT: " + Environment.NewLine +
                            Environment.NewLine +
                            "Текущая задача - " + BranchName + ", но у проекта " + projects[i] + " осталась текущая ветка - " + branch + " !",
                            null, App.ShowMessageMode.SHOW, true, logFile);
                        }
                        else
                        {
                            string nomr = "";
                            if (isNoMergeRequest)
                            {
                                nomr = "NOMERGEREQUEST";
                            }

                            WinExecute.AddCommand(
                                App.AppPath,
                                Path.Combine(App.AppPath, "git_add.cmd"),
                                folder + " " + BranchName + " " + noupper + " " + nomr
                            );
                        }
                    }
                }
            }

            if (WinExecute.ListCommands.Count > 0)
            {
                App.AddLog($"Сейчас выполним GIT ADD/DIFF + COMMIT + PUSH + Merge Request для ветки {BranchName}", null, App.ShowMessageMode.NONE, true, logFile);

                WinExecute.Start(isWait);
            }
            else
            {
                WinExecute.Close();
            }
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Проверить необходимость commit
        /// </summary>
        /// <param name="project">Поект</param>
        /// <param name="logFile">полный путь к лог-файлу. Если не указан, значит в App.AppLogFile</param>
        public static bool GitNeedCommit(string project, string logFile)
        {
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);

            if (!string.IsNullOrWhiteSpace(ProjectFolder))
            {
                App.AddLog($"Сейчас проверим необходимость commit в проекте {project}", null, App.ShowMessageMode.NONE, true, logFile);

                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                // проверить текущую ветку
                string output = Utilities.External.ExecuteFile(
                        App.AppPath,
                        Path.Combine(App.AppPath, "git_nocommit.cmd"),
                        folder,
                        true,
                        false,
                        true,
                        false,
                        logFile
                    );

                if (string.IsNullOrWhiteSpace(output))
                {
                    output = "";
                }
                else
                {
                    output = output.TrimAllSpace();
                }

                App.AddLog($"{output}", null, App.ShowMessageMode.NONE, true, logFile);

                return output.ToLower().Contains("needcommit");
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Сохранить текст в файл в локальной папке проекта GIT, с возможностью выбора ветки и папки
        /// </summary>
        /// <param name="ProjectDefault">проект GIT по умолчанию</param>
        /// <param name="BranchDefault">ветка в проекте GIT</param>
        /// <param name="PathDefault">папка в проекте GIT по умолчанию</param>
        /// <param name="FilenameDefault">Имя файла по умолчанию</param>
        /// <param name="isBranchCanChanged">=true - ветку можно изменить</param>
        /// <param name="isPathCanChanged">=true - путь можно изменить</param>
        /// <param name="isFileCanChanged">=true - имя файла можно изменить</param>
        /// <param name="textScript">текст скрипта</param>
        /// <param name="filepath">полный путь к сохраненному файлу в локальной папке GIT</param>
        /// <param name="fileurl">url к сохраненному файлу в проекте GIT</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        /// <param name="DefaultExt">Расширение по умолчанию для сохраняемого файла, например: .sql</param>
        /// <param name="FilterExt">Список возможных расширений для сохраняемого файла, например: (*.sql)|*.sql|Все файлы (*.*)|*.*</param>
        /// <param name="isForceSave">=true - перезаписывать файл без вопросов</param>
        /// <returns></returns>
        public static bool SaveFileToGIT(string ProjectDefault, string BranchDefault, string PathDefault, string FilenameDefault, bool isBranchCanChanged, bool isPathCanChanged, bool isFileCanChanged, string textScript, out string filepath, out string fileurl, string logFile, bool isForcedGitRefresh, string DefaultExt, string FilterExt, bool isForceSave)
        {
            filepath = "";
            fileurl = "";
            bool result = false;

            string project = ProjectDefault;
            string NewBranch = BranchDefault;
            string path = PathDefault;
            string file = Path.GetFileName(FilenameDefault);
            var res = System.Windows.Forms.DialogResult.OK;

            if (isBranchCanChanged || isPathCanChanged || isFileCanChanged)
            {
                // выбрать ветку и имя файла
                FormSaveToGIT dlg2 = new FormSaveToGIT(logFile);
                dlg2.tbProject.Text = project;
                dlg2.tbBranch.Text = NewBranch;
                if (isBranchCanChanged) dlg2.tbBranch.Enabled = true;
                dlg2.tbPath.Text = path;
                if (isPathCanChanged) dlg2.tbPath.Enabled = true;
                dlg2.tbFilename.Text = file;
                if (isFileCanChanged) dlg2.tbFilename.Enabled = true;

                res = dlg2.ShowDialog();

                if (res == System.Windows.Forms.DialogResult.OK)
                {
                    project = dlg2.tbProject.Text.Trim();
                    if (isBranchCanChanged) NewBranch = dlg2.tbBranch.Text.Trim();
                    if (isPathCanChanged) path = dlg2.tbPath.Text.Trim();
                    if (isFileCanChanged) file = dlg2.tbFilename.Text.Trim();
                }

                dlg2.Dispose();
            }

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                // определяем текущую ветку
                string branch = GIT.GitCurrentBranch(project, out string err, logFile);

                if (!string.IsNullOrWhiteSpace(err))
                {
                    App.AddLog($"Ошибка определения текущей ветки в проекте {project}:\n{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                    return false;
                }

                // если это другая ветка
                if (branch != NewBranch)
                {
                    // проверим, нужен ли commit в текущую ветку
                    if (!GIT.CheckCommit(project, logFile, $"Создание\\переключение на ветку {NewBranch} прервано"))
                    {
                        return false;
                    }

                    // git pull и переключение на ветку NewBranch
                    GIT.GitPull(new string[] { project }, NewBranch, false, true, false, logFile, isForcedGitRefresh);

                    // определяем текущую ветку
                    err = "";
                    branch = GIT.GitCurrentBranch(project, out err, logFile);
                    if (!string.IsNullOrWhiteSpace(err))
                    {
                        App.AddLog($"Ошибка определения текущей ветки в проекте {project}:\n{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                        return false;
                    }
                }

                if (branch != NewBranch)
                {
                    // новой ветки нет в проекте
                    if (System.Windows.Forms.MessageBox.Show($"Создать новую ветку {NewBranch} от ветки master в проекте {project}?", "ВНИМАНИЕ", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                    {
                        // создание ветки NewBranch
                        GIT.GitNewBranch(new string[] { project }, NewBranch, "master", logFile);

                        // сновая определяем текущую ветку
                        err = "";
                        branch = GIT.GitCurrentBranch(project, out err, logFile);
                        if (!string.IsNullOrWhiteSpace(err))
                        {
                            App.AddLog($"Ошибка определения текущей ветки в проекте {project}:\n{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                            return false;
                        }

                        if (branch != NewBranch)
                        {
                            App.AddLog($"Создание ветки {NewBranch} в проекте {project} НЕ было выполнено\n\nВ проекте {project} текущая ветка - {branch}", null, App.ShowMessageMode.SHOW, true, logFile);

                            return false;
                        }
                        else
                        {
                            App.AddLog($"Создание ветки {NewBranch} в проекте {project} выполнено успешно", null, App.ShowMessageMode.NONE, true, logFile);
                        }
                    }
                    else
                    {
                        App.AddLog($"Пользователь отказался создавать ветку {NewBranch} в проекте {project}\n\nВ проекте {project} текущая ветка - {branch}", null, App.ShowMessageMode.NONE, true, logFile);

                        return false;
                    }
                }

                FileStream fs = null;

                try
                {
                    // имя файла для скрипта
                    file = SQLGen.Controls.Dialogs.SaveDialog(
                        DefaultExt, FilterExt,
                        Path.Combine(
                            MainWindow.APPinfo.GITFolder,
                            Utilities.GITProjects.GetFolderByProject(project),
                            path
                        ),
                        file, out fs, out FileMode fileMode, false, isForceSave);

                    //сохранить файл
                    if (fs != null)
                    {
                        err = "";
                        Utilities.Files.WriteScript(file, fs, textScript, false, out err, fileMode);

                        filepath = file;
                        fileurl = Utilities.GITProjects.ConvertFilepathToUrl(project, filepath, branch, logFile);
                        result = true;
                    }
                }
                catch (Exception ex)
                {
                    App.AddLog(null, ex, App.ShowMessageMode.SHOW, true, logFile);
                    result = false;
                }
                finally
                {
                    if (fs != null) fs.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// Выбор ветки в проекте
        /// </summary>
        /// <param name="project">проект</param>
        /// <param name="BranchDefault">ветка по умолчанию</param>
        /// <param name="branch">выбранная ветка</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="isGitRefresh">=true - выполнить git-refresh.sh</param>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        /// <returns></returns>
        public static bool SelectGITBranch(string project, string BranchDefault, out string branch, string logFile, bool isGitRefresh, bool isForcedGitRefresh)
        {
            branch = "";

            if (!string.IsNullOrWhiteSpace(project))
            {
                // определяем текущую ветку
                string err = "";
                branch = GIT.GitCurrentBranch(project, out err, logFile);
                if (
                    !string.IsNullOrWhiteSpace(err) ||
                    string.IsNullOrWhiteSpace(branch)
                )
                {
                    App.AddLog($"Ошибка определения текущей ветки в проекте {project}:\n{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                    branch = "";

                    return false;
                }

                if (Utilities.GITProjects.IsDEVProject(project))
                {
                    // выбрать ветку (в новых проектах)
                    FormAskBranch dlg2 = new FormAskBranch(project, BranchDefault, logFile);

                    // существует ли ветка по умолчанию
                    bool isBranchDefaultFound = false;

                    // Заполнить ListBranches
                    foreach (var item in GIT.GitListBranches(project, "git_listbranch.cmd", logFile, true))
                    {
                        string _branch = item.Replace("*", "").Trim();

                        if (
                            (!string.IsNullOrWhiteSpace(_branch)) &&
                            (!dlg2.ListBranches.Contains(_branch))
                            )
                        {
                            dlg2.ListBranches.Add(_branch);

                            if (_branch.ToLower() == dlg2.Branch.ToLower())
                            {
                                isBranchDefaultFound = true;
                            }
                        }
                    }

                    if (!isBranchDefaultFound)
                    {
                        dlg2.btTask.Visible = false;
                        dlg2.btTask.Text = "";
                    }

                    var res = dlg2.ShowDialog();

                    if (res != System.Windows.Forms.DialogResult.Abort)
                    {
                        if (!string.IsNullOrWhiteSpace(dlg2.Branch))
                        {
                            branch = dlg2.Branch;
                        }
                    }

                    if (res == System.Windows.Forms.DialogResult.Abort)
                    {
                        // определяем текущую ветку
                        err = "";
                        string cur_branch = GIT.GitCurrentBranch(project, out err, logFile);

                        // вернем начальную ветку
                        if (cur_branch != branch)
                        {
                            GIT.GitSwitch(project, branch, logFile, out cur_branch, out err);
                        }

                        return false;
                    }

                    dlg2.Dispose();
                }

                // git pull и переключение на выбранную ветку
                GIT.GitPull(new string[] { project }, branch, false, true, false, logFile, false);

                // определяем текущую ветку
                err = "";
                string current_branch = GIT.GitCurrentBranch(project, out err, logFile);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    App.AddLog($"Ошибка определения текущей ветки в проекте {project}:\n{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                    branch = "";

                    return false;
                }

                if (current_branch != branch)
                {
                    App.AddLog($"В проекте {project} осталась ветка {current_branch}", null, App.ShowMessageMode.SHOW, true, logFile);

                    branch = current_branch;

                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Выбрать модуль (префикс) версии: prmd, rpms, smp, bi
        /// </summary>
        /// <param name="logFile"></param>
        /// <returns></returns>
        public static string SelectGITModule(string logFile)
        {
            FormAskModule dlg1 = new FormAskModule();
            string prefix = "";

            var res = dlg1.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                prefix = "prmd";
            }
            else if (res == System.Windows.Forms.DialogResult.Yes)
            {
                prefix = "rpms";
            }
            else if (res == System.Windows.Forms.DialogResult.No)
            {
                prefix = "smp";
            }
            else if (res == System.Windows.Forms.DialogResult.Retry)
            {
                prefix = "bi";
            }
            else if (res == System.Windows.Forms.DialogResult.Cancel)
            {
                prefix = "";
            }
            else
            {
                App.AddLog("Модуль не выбран!", null, App.ShowMessageMode.SHOW, true, logFile);
            }

            dlg1.Dispose();

            return prefix;
        }

        /// <summary>
        /// Выбор проекта и ветки в нем
        /// </summary>
        /// <param name="listProjects">список проектов. Если пустой, то по умолчанию все dev-проекты</param>
        /// <param name="BranchDefault">ветка по умолчанию</param>
        /// <param name="project">выбранный проект</param>
        /// <param name="branch">выбранная ветка</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        /// <returns></returns>
        public static bool SelectGITProject(List<string> listProjects, string BranchDefault, out string project, out string branch, string logFile, bool isForcedGitRefresh)
        {
            project = "";
            branch = "";

            if (listProjects == null)
            {
                listProjects = new List<string>();
            }
                    
            if (listProjects.Count == 0)
            {
                // заполнить список возможных dev-проектов
                foreach (var item in MainWindow.ListExistedProjects
                    .Where(x => Utilities.GITProjects.IsDEVProject(x)) // только новые проекты
                    .OrderBy(x =>
                    {
                        string ord = "999";
                        if (x == "dev_promed_pg") ord = "001";
                        else if (x == "dev_promed_ms") ord = "003";
                        else if (x == "dev_lis_pg") ord = "005";
                        else if (x == "dev_emd_pg") ord = "007";
                        return ord + x;
                    }
                    ))
                {
                    if (!listProjects.Contains(item))
                    {
                        listProjects.Add(item);
                    }
                }
            }

            // выбрать проект
            FormFindInList dlg1 = new FormFindInList(logFile);

            dlg1.AddItems(listProjects);

            if (dlg1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string result = "";

                foreach (System.Windows.Forms.DataGridViewRow row in dlg1.dgListValues.SelectedRows)
                {
                    result = row.Cells[0].Value.ToString();
                    // берем только первую
                    break; //-V3020
                }

                if (!string.IsNullOrWhiteSpace(result))
                {
                    project = result;
                }
            }

            dlg1.Dispose();

            // выберем ветку
            return SelectGITBranch(project, BranchDefault, out branch, logFile, true, isForcedGitRefresh);
        }

        /// <summary>
        /// Выбрать json-файл в локальной папке проекта GIT
        /// </summary>
        /// <param name="mainWindow">ссылка на основное окно программы</param>
        /// <param name="_dbregion">Тип региона по типу основной БД</param>
        /// <param name="jsonfile">полный путь к json-файлу</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="isForcedGitRefresh">=true - выполнять принудительно git-refresh.sh, =false - выполнять git-refresh.sh только если после предыдущего вызова прошло MainWindow.APPinfo.GitRefreshDelay минут</param>
        /// <returns></returns>
        public static bool OpenJson(MainWindow mainWindow, string _dbregion, out string jsonfile, string logFile, bool isForcedGitRefresh)
        {
            jsonfile = "";

            // заполнить список возможных проектов
            List<string> ListProjects = new List<string>();

            foreach (var item in MainWindow.ListExistedProjects
                .OrderBy(x =>
                {
                    string ord = "999";
                    if (x == "dev_promed_pg") ord = "001";
                    else if (x == "dev_promed_ms") ord = "002";
                    else if (x == "dev_lis_pg") ord = "003";
                    else if (x == "dev_emd_pg") ord = "004";
                    else if (x == "liquibase_project_new") ord = "005";
                    else if (x == "msdbupdate_new") ord = "006";
                    else if (x == "promedlistest2") ord = "007";
                    else if (x == "emd") ord = "008";
                    return ord + x;
                }
                ))
            {
                string _dbtype = Utilities.GITProjects.GetDBTypeByProject(item);

                if (
                    (_dbregion == "PG SQL" &&
                    (
                        _dbtype == "MSSQL" ||
                        MainWindow.APPinfo.ListProjects_ONLY_MS.Contains(item)
                    )) ||
                    (
                        _dbregion == "MS SQL" &&
                        MainWindow.APPinfo.ListProjects_ONLY_PG.Contains(item)
                    )
                )
                {
                    // пропускаем
                }
                else
                {
                    if (!ListProjects.Contains(item))
                    {
                        ListProjects.Add(item);
                    }
                }
            }

            // выбрать проект и ветку
            if (
                !SelectGITProject(ListProjects, null, out string project, out string branch, logFile, isForcedGitRefresh) ||
                string.IsNullOrWhiteSpace(project)
                )
            {
                return false;
            }

            // выбрать файл в проекте
            string folder = Utilities.GITProjects.GetFolderByProject(project);
            string path = Path.Combine(MainWindow.APPinfo.GITFolder, folder);
            try
            {
                jsonfile = SQLGen.Controls.Dialogs.OpenFileDialog(path, ".json", "(*.json)|*.json|Все файлы (*.*)|*.*");
                return true;
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, logFile);
            }

            return false;
        }

        /// <summary>
        /// true - комит не нужен, можно продолжать; false - нужен комит или что-то с git
        /// </summary>
        /// <param name="project">проект</param>
        /// <param name="logFile">лог-файл</param>
        /// <param name="infoerror">Сообщение при ошибке</param>
        /// <returns></returns>
        public static bool CheckCommit(string project, string logFile, string infoerror)
        {
            if (!string.IsNullOrWhiteSpace(infoerror))
            {
                infoerror = Environment.NewLine + Environment.NewLine + infoerror;
            }
            else
            {
                infoerror = "";
            }

            // проверим, нужен ли commit в текущей ветке
            if (GIT.GitNeedCommit(project, logFile))
            {
                // определяем текущую ветку
                string current_branch = GitCurrentBranch(project, out string err, logFile);
                if (string.IsNullOrWhiteSpace(current_branch))
                {
                    App.AddLog("У проекта " + project + " не определилась ветка." + Environment.NewLine
                        + Environment.NewLine + err + infoerror, null, App.ShowMessageMode.SHOW, true, logFile);

                    return false;
                }

                App.AddLog($"Для текущей ветки {current_branch} требуется commit.", null, App.ShowMessageMode.SHOW, true, logFile);

                // Отправляем в GIT
                GIT.GitAdd(new string[] { project }, current_branch, true, true, logFile);

                // еще раз проверим, нужен ли commit
                if (GIT.GitNeedCommit(project, logFile))
                {
                    App.AddLog($"По прежнему, для текущей ветки {current_branch} требуется commit." + infoerror, null, App.ShowMessageMode.SHOW, true, logFile);

                    return false;
                }

                // снова определяем текущую ветку
                err = "";
                string branch = GitCurrentBranch(project, out err, logFile);
                if (string.IsNullOrWhiteSpace(branch))
                {
                    App.AddLog("У проекта " + project + " не определилась ветка!" + Environment.NewLine
                        + Environment.NewLine + err + infoerror, null, App.ShowMessageMode.SHOW, true, logFile);

                    return false;
                }

                // вернем начальную ветку
                if (current_branch != branch)
                {
                    if (!GIT.GitSwitch(project, current_branch, logFile, out string now_branch, out err))
                    {
                        App.AddLog($"В проекте {project} не получилось вернуться на ветку {current_branch}{Environment.NewLine}Ошибка {err}{Environment.NewLine}Текущая ветка {now_branch}{infoerror}", null, App.ShowMessageMode.SHOW, true, logFile);

                        return false;
                    }
                }
            }

            return true;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Влить версию во все последующие версии
        /// </summary>
        /// <param name="project">Проект</param>
        /// <param name="branch">Ветка, которую вливаем</param>
        /// <param name="isNoCumulative">=true - текущая версия НЕ кумулятивная</param>
        /// <param name="Versions">Список версий</param>
        /// <param name="logFile">полный путь к лог-файлу. Если пустой, значит в MainWindow.Task.LogFileMerge, если пустой, значит - в App.AppLogFile</param>
        public static bool GitMergeNextVersion(string project, string branch, bool isNoCumulative, SortedDictionary<double, Version> Versions, string logFile)
        {
            if (string.IsNullOrWhiteSpace(project))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(branch))
            {
                return false;
            }

            if (
                Versions == null ||
                Versions.Count == 0
            )
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = MainWindow.Task.LogFileMerge;
            }
            if (string.IsNullOrWhiteSpace(logFile))
            {
                logFile = App.AppLogFile;
            }

            // ----------------------------------------------------------------------------
            // Данные текущей версии
            string ProjectFolder = Utilities.GITProjects.GetFolderByProject(project);
            string prefix = Utilities.GITProjects.GetPrefixFileReleaseByProject(project);
            double numversion = Release.VerAsNum(Release.GetNumVersion(prefix, branch));
            bool isCumulative = isNoCumulative != true;

            // ----------------------------------------------------------------------------
            // Составляем список веток
            var ListNextVersion = new List<Version>();

            // сначала добавим текущую
            foreach (var item in Versions
                    .Where(x =>
                        (x.Value != null) &&
                        (x.Value.YMLFile != null) &&
                        x.Value.YMLFile.IsFileExist && // есть файл-версии
                        (!x.Value.YMLFile.IsIgnore) && // нет флага-исключения
                        (x.Value.NumOrder == numversion) 
                    )
            )
            {
                ListNextVersion.Add(item.Value);
                break;
            }

            // затем добавим следующие
            double cur_num = numversion;
            do
            {
                Version found = null;

                foreach (var item in Versions
                    .Where(x =>
                        (x.Value != null) &&
                        (x.Value.YMLFile != null) &&
                        x.Value.YMLFile.IsFileExist && // есть файл-версии
                        (!x.Value.YMLFile.IsIgnore) && // нет флага-исключения
                        (x.Value.NumOrder > cur_num) && // следующая версия после текущей
                        (x.Value.PrevNumOrder > 0) && // у нее есть ссылка на предыдущую версию
                        // либо текущая кумулятивная, либо текущая НЕ кумулятивная и все следующие только НЕ кумулятивные
                        (isCumulative || x.Value.YMLFile.IsNoCumulative)
                    )
                    .OrderBy(x => x.Value.NumOrder)
                )
                {
                    found = item.Value;
                    break;
                }

                if (found != null)
                {
                    ListNextVersion.Add(found);
                }
                else
                {
                    break;
                }

                cur_num = found.NumOrder;
            } while (true);

            if (ListNextVersion.Count == 0)
            {
                App.AddLog($"Для версии {branch} в проекте {project} нет следующих версий. Merge прерван", null, App.ShowMessageMode.SHOW, true, logFile);

                return false;
            }

            // ----------------------------------------------------------------------------
            // Соберем список merge
            var ListMerge = new List<MergeInfo>();

            foreach (var branch_to in ListNextVersion
                .Where(x => x.Branch != branch) // кроме текущей ветки
                .OrderBy(x => x.NumOrder)
                .Select(x => x.Branch)
                
            )
            {
                var info = new MergeInfo();

                // ветка - куда вливаем
                info.VersionTo = ListNextVersion
                    .Where(x =>
                        x.YMLFile != null &&
                        x.YMLFile.IsFileExist && // есть файл-версии
                        (!x.YMLFile.IsIgnore) && // нет флага-исключения
                        x.PrevNumOrder > 0 && // есть ссылка на предыдущую версию
                        x.Branch == branch_to
                    )
                    .FirstOrDefault();

                if (info.VersionTo == null)
                {
                    continue;
                }

                // ветка - которую вливаем
                info.VersionFrom = ListNextVersion
                    .Where(x =>
                        x.YMLFile != null &&
                        x.YMLFile.IsFileExist && // есть файл-версии
                        (!x.YMLFile.IsIgnore) && // нет флага-исключения
                        x.NumOrder > 0 &&
                        x.NumOrder < info.VersionTo.NumOrder &&
                        x.NumOrder == info.VersionTo.PrevNumOrder
                    )
                    .FirstOrDefault();

                if (info.VersionFrom == null)
                {
                    continue;
                }

                // Проверка - нельзя влить НЕ КУМУЛЯТИВНУЮ версию в КУМУЛЯТИВНУЮ
                if (info.VersionFrom.YMLFile.IsNoCumulative && info.VersionTo.YMLFile.IsCumulative)
                {
                    continue;
                }

                ListMerge.Add(info);
            }

            // ----------------------------------------------------------------------------
            // Выбираем ветки, в которые надо влить текущую
            FormCheckedListBox dlg1 = new FormCheckedListBox();
            dlg1.Text = $"Выбрать в проекте {project} следующие ветки, в которые надо влить {branch}";
            dlg1.clbList.Items.Clear();

            dlg1.clbList.Items.AddRange(ListMerge
                .Select(x => x.VisibleName) 
                .ToArray()
            );

            dlg1.SetAll();

            var ListBranch = new List<string>();

            var res = dlg1.ShowDialog();

            if (res == System.Windows.Forms.DialogResult.OK)
            {
                foreach (object itemChecked in dlg1.clbList.CheckedItems)
                {
                    ListBranch.Add(itemChecked.ToString());
                }
            }

            dlg1.Dispose();

            if (res != System.Windows.Forms.DialogResult.OK)
            {
                return false;
            }

            if (ListBranch.Count == 0)
            {
                App.AddLog($"Для версии {branch} в проекте {project} не выбраны версии, в которые надо ее влить. Merge прерван", null, App.ShowMessageMode.SHOW, true, logFile);

                return false;
            }

            // ----------------------------------------------------------------------------
            // определяем текущую ветку
            string start_branch = GIT.GitCurrentBranch(project, out string err, logFile);
            if (!string.IsNullOrWhiteSpace(err))
            {
                App.AddLog($"Ошибка при определении текущей ветки в проекте {project}. Merge прерван: {Environment.NewLine}{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                return false;
            }

            // ----------------------------------------------------------------------------
            // проверим, нужен ли commit в текущую ветку
            if (!GIT.CheckCommit(project, logFile, "Merge прерван"))
            {
                return false;
            }

            // ----------------------------------------------------------------------------
            // переключение на исходную ветку
            string cur_branch = "";
            if (start_branch.ToLower() != branch.ToLower())
            {
                if (!GIT.GitSwitch(project, branch, logFile, out cur_branch, out err))
                {
                    App.AddLog($"В проекте {project} текущая ветка {cur_branch}, а должна быть {branch}. Merge прерван: {Environment.NewLine}{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                    return false;
                }

                // ----------------------------------------------------------------------------
                // проверим, нужен ли commit в исходную ветку
                if (!GIT.CheckCommit(project, logFile, "Merge прерван"))
                {
                    return false;
                }
            }

            // ----------------------------------------------------------------------------
            // Выполняем последовательный merge
            foreach (var item in ListMerge)
            {
                if (!ListBranch.Contains(item.VisibleName)) 
                {
                    // эту ветку не выбрали
                    continue;
                }

                if (item.VersionTo == null)
                {
                    continue;
                }

                if (item.VersionFrom == null)
                {
                    continue;
                }

                // Проверка - нельзя влить НЕ КУМУЛЯТИВНУЮ версию в КУМУЛЯТИВНУЮ
                if (item.VersionFrom.YMLFile.IsNoCumulative && item.VersionTo.YMLFile.IsCumulative)
                {
                    continue;
                }

                // переключиться на следующую ветку
                if (!GIT.GitSwitch(project, item.VersionTo.Branch, logFile, out cur_branch, out err))
                {
                    App.AddLog($"Не смогли перелючиться на ветку {item.VersionTo.Branch} в проекте {project}\nТекущая ветка {cur_branch}\n{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                    return false;
                }

                // влить предыдущую
                if (!GIT.GitMerge(project, item.VersionFrom.Branch, item.VersionTo.Branch, true, false, logFile, false))
                {
                    App.AddLog($"Не смогли влить ветку {item.VersionFrom.Branch} в {item.VersionTo.Branch} в проекте {project}", null, App.ShowMessageMode.SHOW, true, logFile);

                    return false;
                }
            }

            // ----------------------------------------------------------------------------
            // показываем лог
            if (
                System.Windows.Forms.MessageBox.Show("Посмотреть лог merge ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes
            )
            {
                WinInfo WinInfo = new WinInfo(logFile);
                WinInfo.tbInfo.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("LOG");
                WinInfo.tbInfo.Text = File.ReadAllText(logFile);
                WinInfo.Title = "Лог в файле " + logFile;
                WinInfo.ShowDialog();
            }

            // ----------------------------------------------------------------------------
            // Выполнем последовательный push

            if (System.Windows.Forms.MessageBox.Show($"Сделаем итоговый push в проекте {project} всех веток, в которые был merge ?", "", System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                foreach (var item in ListMerge)
                {
                    if (!ListBranch.Contains(item.VisibleName))
                    {
                        // эту ветку не выбрали
                        continue;
                    }

                    if (item.VersionTo == null)
                    {
                        continue;
                    }

                    // переключиться на следующую ветку
                    if (!GIT.GitSwitch(project, item.VersionTo.Branch, logFile, out cur_branch, out err))
                    {
                        App.AddLog($"Не смогли перелючиться на ветку {item.VersionTo.Branch} в проекте {project}\nТекущая ветка {cur_branch}\n{err}", null, App.ShowMessageMode.SHOW, true, logFile);

                        return false;
                    }

                    // делаем push
                    GIT.GitPush(new string[] { project }, item.VersionTo.Branch, true, logFile);
                }
            }

            // ----------------------------------------------------------------------------
            // Возвращаем изначальную ветку
            GIT.GitSwitch(project, start_branch, logFile, out cur_branch, out err);

            // ----------------------------------------------------------------------------
            if (!string.IsNullOrWhiteSpace(ProjectFolder))
            {
                string folder = Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder);
                if (folder.Contains(" ")) folder = "\"" + folder + "\"";

                // Отображаем информацию о необходимости push
                Utilities.External.ExecuteFile(
                    App.AppPath,
                    Path.Combine(App.AppPath, "git_needpush.sh"),
                    folder,
                    false,
                    false,
                    false,
                    false,
                    logFile
                );

                // Запустим git-nomerged-ver.sh
                Utilities.External.ExecuteFile(
                    App.AppPath,
                    Path.Combine(MainWindow.APPinfo.GITFolder, ProjectFolder, "git-nomerged-ver.sh"),
                    folder,
                    false,
                    false,
                    false,
                    false,
                    logFile
                );
            }

            return true;
        }
    }


    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Тип действия при отправке changeset в GIT
    /// </summary>
    internal enum ActionType
    {
        /// <summary>
        /// Оставить changeset без изменений
        /// </summary>
        SKIP,
        /// <summary>
        /// Перезаписать changeset
        /// </summary>
        OWERWRITE,
        /// <summary>
        /// Дописать changeset в конец файла
        /// </summary>
        APPEND
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Описание changeset отправляемого в GIT
    /// </summary>
    internal class CopyInfo
    {
        /// <summary>
        /// Файл-источник
        /// </summary>
        public string fromFile { get; set; }

        /// <summary>
        /// Файл-назначение
        /// </summary>
        public string toFile { get; set; }

        /// <summary>
        /// Тип действия при отправке changeset в GIT
        /// </summary>
        public ActionType actionType { get; set; }

        /// <summary>
        /// Имя changeset из файла (для сравнения)
        /// </summary>
        public string changesetName { get; set; }

        /// <summary>
        /// Текст changeset из файла (для сравнения)
        /// </summary>
        public string changesetText { get; set; }
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Описание слияния версий
    /// </summary>
    internal class MergeInfo
    {
        /// <summary>
        /// из ветки
        /// </summary>
        public Version VersionFrom{ get; set; }

        /// <summary>
        /// в ветку
        /// </summary>
        public Version VersionTo { get; set; }

        /// <summary>
        /// Отображение
        /// </summary>
        public string VisibleName
        {
            get
            {
                string result = "";

                if (VersionFrom != null)
                {
                    result += VersionFrom.Num;

                    if (
                        VersionFrom.YMLFile != null &&
                        VersionFrom.YMLFile.IsNoCumulative
                    )
                    {
                        result += " (НЕ КУМУЛЯТИВНАЯ)";
                    }

                    result += " => ";
                }

                if (VersionTo != null)
                {
                    result += VersionTo.Num;

                    if (
                        VersionTo.YMLFile != null &&
                        VersionTo.YMLFile.IsNoCumulative
                    )
                    {
                        result += " (НЕ КУМУЛЯТИВНАЯ)";
                    }
                }

                if (
                    VersionFrom != null &&
                    VersionTo != null
                )
                {
                    if (
                        VersionFrom.YMLFile != null &&
                        VersionFrom.YMLFile.IsNoCumulative &&
                        VersionTo.YMLFile != null &&
                        VersionTo.YMLFile.IsCumulative
                    )
                    {
                        result += " - ВНИМАНИЕ, так нельзя!!!";
                    }
                }

                return result;
            }
        }
    }
}
