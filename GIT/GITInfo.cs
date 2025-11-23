// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Windows;
using System;
using SQLGen.Utilities;

namespace SQLGen
{
    // -------------------------------------------------------------------------------------------------------
    /// <summary>Класс описания проекта GIT</summary>
    public class GITInfo
    {
        /// <summary>Проект GIT</summary>
        public string GITProject { get; set; }

        /// <summary>Каталог с проектом GIT</summary>
        public string GITProjectFolder { get; set; }

        /// <summary>Префикс в имени sql-файлов для проекта GIT</summary>
        public string PrefixFileSQL { get; set; }

        /// <summary>Префикс в имени yml-файла релизной версии для проекта GIT</summary>
        public string PrefixFileRelease { get; set; }

        /// <summary>Постфикс в имени yml-файла релизной версии для проекта GIT</summary>
        public string PostfixFileRelease { get; set; }

        /// <summary>URL проекта в GIT</summary>
        public string GITUrl { get; set; }

        /// <summary>альтернативный URL проекта в GIT</summary>
        public string GITUrlAlt { get; set; }

        /// <summary>Имя поля для проекта GIT в форме сборки релиза</summary>
        public string GITYMLField { get; set; }

        string _gitdatafolder;
        /// <summary>Папка для данных для проекта GIT</summary>
        public string GITDataFolder
        {
            get
            {
                return _gitdatafolder ?? (GITProjectFolder == "liquibase_project_new" ? "data_new" : "data");
            }
            set
            {
                _gitdatafolder = value;
                if (string.IsNullOrWhiteSpace(_gitdatafolder)) _gitdatafolder = (GITProjectFolder == "liquibase_project_new" ? "data_new" : "data");
                _gitdatafolder = _gitdatafolder.Trim();

            }
        }

        string _gitissinglescript;
        /// <summary>=YES/ДА - Один объект, один скрипт - для проекта GIT</summary>
        public string GITisSingleScript
        {
            get
            {
                return _gitissinglescript ?? "NO";
            }
            set
            {
                _gitissinglescript = value.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(_gitissinglescript)) _gitissinglescript = "YES";
                if (_gitissinglescript == "ДА") _gitissinglescript = "YES";
                if (_gitissinglescript != "YES") _gitissinglescript = "NO";
            }
        }

        string _dbtype;
        /// <summary>Тип БД проекта GIT</summary>
        public string DBType
        {
            get
            {
                return _dbtype ?? "MSSQL";
            }
            set
            {
                _dbtype = value;
                if (string.IsNullOrWhiteSpace(_dbtype)) _dbtype = "MSSQL";
                _dbtype = _dbtype.Trim();

            }
        }

        /// <summary>Проект GIT для разработки</summary>
        public string DEVProject { get; set; }

        /// <summary>Каталог с проектом GIT для разработки</summary>
        public string DEVProjectFolder { get; set; }

        /// <summary>URL проекта в GIT для разработки</summary>
        public string DEVUrl { get; set; }
        /// <summary>альтернативный URL проекта в GIT для разработки</summary>
        public string DEVUrlAlt { get; set; }

        /// <summary>Имя поля для проекта DEV в форме сборки релиза</summary>
        public string DEVYMLField { get; set; }

        string _devdatafolder;
        /// <summary>Папка для данных для проекта DEV</summary>
        public string DEVDataFolder
        {
            get
            {
                return _devdatafolder ?? "data";
            }
            set
            {
                _devdatafolder = value;
                if (string.IsNullOrWhiteSpace(_devdatafolder)) _devdatafolder = "data";
                _devdatafolder = _devdatafolder.Trim();

            }
        }

        string _devissinglescript;
        /// <summary>=YES/ДА - Один объект, один скрипт - для проекта DEV (устарело)</summary>
        public string DEVisSingleScript
        {
            get
            {
                return _devissinglescript ?? "YES";
            }
            set
            {
                _devissinglescript = value.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(_devissinglescript)) _devissinglescript = "YES";
                if (_devissinglescript == "ДА") _devissinglescript = "YES";
                if (_devissinglescript != "YES") _devissinglescript = "NO";
            }
        }

        string _issinglescriptstruct;
        /// <summary>=YES/ДА - Один объект, один скрипт - для таблиц, схем, сиквенсов, индексов, типов проекта DEV</summary>
        public string DEVisSingleScriptStruct
        {
            get
            {
                return _issinglescriptstruct ?? "YES";
            }
            set
            {
                _issinglescriptstruct = value.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(_issinglescriptstruct)) _issinglescriptstruct = "YES";
                if (_issinglescriptstruct == "ДА") _issinglescriptstruct = "YES";
                if (_issinglescriptstruct != "YES") _issinglescriptstruct = "NO";
            }
        }

        string _issinglescriptcode;
        /// <summary>=YES/ДА - Один объект, один скрипт - для процедур, функций, вьюх, триггеров проекта DEV</summary>
        public string DEVisSingleScriptCode
        {
            get
            {
                return _issinglescriptcode ?? "YES";
            }
            set
            {
                _issinglescriptcode = value.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(_issinglescriptcode)) _issinglescriptcode = "YES";
                if (_issinglescriptcode == "ДА") _issinglescriptcode = "YES";
                if (_issinglescriptcode != "YES") _issinglescriptcode = "NO";
            }
        }

        string _issinglescriptdata;
        /// <summary>=YES/ДА - Один объект, один скрипт - для данных проекта DEV</summary>
        public string DEVisSingleScriptData
        {
            get
            {
                return _issinglescriptdata ?? "YES";
            }
            set
            {
                _issinglescriptdata = value.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(_issinglescriptdata)) _issinglescriptdata = "YES";
                if (_issinglescriptdata == "ДА") _issinglescriptdata = "YES";
                if (_issinglescriptdata != "YES") _issinglescriptdata = "NO";
            }
        }
        /// <summary>Номер версии, с которой начали собирать релизы в проекте DEV</summary>
        public string DEVStartVer { get; set; }

        /// <summary>
        /// Список версий, на которых разрывается кумулятивность в данном проекте, через запятую
        /// </summary>
        public string CumulativeGap { get; set; }

        string _isevninherit;
        /// <summary>=YES/ДА - Таблицы событий используют наследование</summary>
        public string isEvnInherit
        {
            get
            {
                return _isevninherit ?? "YES";
            }
            set
            {
                _isevninherit = value.Trim().ToUpper();
                if (string.IsNullOrWhiteSpace(_isevninherit)) _isevninherit = "YES";
                if (_isevninherit == "ДА") _isevninherit = "YES";
                if (_isevninherit != "YES") _isevninherit = "NO";
            }
        }

        /// <summary>Алиас базы</summary>
        public string DBAlias { get; set; }

        /// <summary>В какой тип региона по типу основной БД моежт быть включен этот проект: MS SQL - только Промед, PG SQL - только ЕЦП, ALL - все регионы</summary>
        public string DBRegion { get; set; }

        /// <summary>
        /// Алиас бота старый
        /// </summary>
        public string LuquibotAliasOld { get; set; }

        /// <summary>
        /// Алиас бота старый Уфа
        /// </summary>
        public string LuquibotAliasOldUfa { get; set; }

        /// <summary>
        /// Алиас бота для SP
        /// </summary>
        public string LuquibotAliasSP { get; set; }

        /// <summary>
        /// Алиас бота для SP Уфа
        /// </summary>
        public string LuquibotAliasSPUfa { get; set; }

        /// <summary>
        /// Алиас бота для HF
        /// </summary>
        public string LuquibotAliasHF { get; set; }

        /// <summary>
        /// Алиас бота для HF Уфа
        /// </summary>
        public string LuquibotAliasHFUfa { get; set; }

        /// <summary>
        /// Алиас бота для EHF актуальный
        /// </summary>
        public string LuquibotAliasEHFAct { get; set; }

        /// <summary>
        /// Алиас бота для EHF актуальный Уфа
        /// </summary>
        public string LuquibotAliasEHFActUfa { get; set; }

        /// <summary>
        /// Алиас бота для EHF не актуальный
        /// </summary>
        public string LuquibotAliasEHFUnAct { get; set; }

        /// <summary>
        /// Алиас бота для EHF не актуальный Уфа
        /// </summary>
        public string LuquibotAliasEHFUnActUfa { get; set; }

        /// <summary>
        /// Алиас бота для LTS
        /// </summary>
        public string LuquibotAliasLTS { get; set; }

        /// <summary>
        /// Алиас бота для LTS Уфа
        /// </summary>
        public string LuquibotAliasLTSUfa { get; set; }

        /// <summary>
        /// Проект для хранения действий при обновлении MS
        /// </summary>
        public string ProjectDeploymentMS { get; set; }

        /// <summary>
        /// Проект для хранения действий при обновлении PG
        /// </summary>
        public string ProjectDeploymentPG { get; set; }

        /// <summary>
        /// Проект для хранения заданий MS
        /// </summary>
        public string ProjectCronMS { get; set; }

        /// <summary>
        /// Проект для хранения заданий PG
        /// </summary>
        public string ProjectCronPG { get; set; }
    }
}
