// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Path = System.IO.Path;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>Класс YML-файл для включения в релиз</summary>
    public class YMLFileInfo
    {
        /// <summary>
        /// реализация NotifyPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// реализация NotifyPropertyChanged
        /// </summary>
        /// <param name="propertyName">propertyName</param>
        public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName)); //-V3083
            }
        }

        int _ymlorder;
        /// <summary>Номер по порядку</summary>
        public int YMLOrder
        {
            get { return _ymlorder; }
            set
            {
                _ymlorder = value;
                NotifyPropertyChanged("YMLOrder");
            }
        }

        private string _isupdated;
        /// <summary>Изменился после обновления из Jira</summary>
        [JsonIgnore]
        public string isUpdated
        {
            get
            {
                return _isupdated ?? "";
            }
            set
            {
                _isupdated = value;
                if (string.IsNullOrWhiteSpace(_isupdated)) _isupdated = "";

                NotifyPropertyChanged("isUpdated");
            }
        }


        private string _tasknumber;
        /// <summary>Номер релизной задачи</summary>
        public string TaskNumber
        {
            get
            {
                return _tasknumber ?? "";
            }
            set
            {
                _tasknumber = value;
                if (string.IsNullOrWhiteSpace(_tasknumber)) _tasknumber = "";

                NotifyPropertyChanged("TaskNumber");
            }
        }

        private string _errorinfo;
        /// <summary>Ошибка при парсинге</summary>
        public string ErrorInfo
        {
            get
            {
                return _errorinfo ?? "";
            }
            set
            {
                _errorinfo = value;
                if (string.IsNullOrWhiteSpace(_errorinfo)) _errorinfo = "";

                NotifyPropertyChanged("ErrorInfo");
            }
        }

        private string _taskstatus;
        /// <summary>Статус релизной задачи</summary>
        public string TaskStatus
        {
            get
            {
                return _taskstatus ?? "";
            }
            set
            {
                _taskstatus = value;
                if (string.IsNullOrWhiteSpace(_taskstatus)) _taskstatus = "";

                NotifyPropertyChanged("TaskStatus");
            }
        }

        private string _region;
        /// <summary>Регион(ы)</summary>
        public string Region
        {
            get
            {
                return _region ?? "";
            }
            set
            {
                _region = value;
                if (string.IsNullOrWhiteSpace(_region)) _region = "";

                NotifyPropertyChanged("Region");
            }
        }

        private string _version;
        /// <summary>Версия</summary>
        public string Version
        {
            get
            {
                return _version ?? "";
            }
            set
            {
                _version = value;
                if (string.IsNullOrWhiteSpace(_version)) _version = "";

                NotifyPropertyChanged("Version");
            }
        }


        private string _branch;
        /// <summary>Ветка</summary>
        public string Branch
        {
            get
            {
                return _branch ?? "";
            }
            set
            {
                _branch = value;
                if (string.IsNullOrWhiteSpace(_branch)) _branch = "";

                _branch = Utilities.Task.GetTaskNumber(_branch);

                NotifyPropertyChanged("BranchName");
                NotifyPropertyChanged("Branch");
            }
        }

        /// <summary>Сборное имя ветки</summary>
        public string BranchName
        {
            get
            {
                // ветка из url
                if (!string.IsNullOrWhiteSpace(Branch))
                {
                    return Branch;
                }

                // ветка из yml-файла
                string _yml = GetYMLFileDefault.Split('.')[0];
                _yml = Utilities.Task.GetTaskNumber(_yml);
                return _yml;
            }
        }

        private string _pathingit;
        /// <summary>Папка в GIT</summary>
        public string PathInGIT
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_pathingit))
                {
                    return "task";
                }
                else
                {
                    return _pathingit;
                }
            }
            set
            {
                _pathingit = value;

                if (!string.IsNullOrWhiteSpace(_pathingit))
                {
                    _pathingit = _pathingit
                        .Trim()
                        .Replace('/', Path.DirectorySeparatorChar);
                }

                NotifyPropertyChanged("PathInGIT");
            }
        }

        private string _mergestatus;
        /// <summary>Статус merge</summary>
        public string MergeStatus
        {
            get
            {
                return _mergestatus ?? "";
            }
            set
            {
                _mergestatus = value;
                if (string.IsNullOrWhiteSpace(_mergestatus)) _mergestatus = "";

                NotifyPropertyChanged("MergeStatus");
            }
        }

        /// <summary>Имя YML-файла для включения в релиз MS</summary>
        public string YMLFile_MS { get; set; }
        /// <summary>Имя YML-файла для включения в релиз PG</summary>
        public string YMLFile_PG { get; set; }
        /// <summary>Имя YML-файла для включения в релиз EMD</summary>
        public string YMLFile_EMD { get; set; }
        /// <summary>Имя YML-файла для включения в релиз LIS</summary>
        public string YMLFile_LIS { get; set; }
        /// <summary>Имя YML-файла для включения в релиз userportal (ms)</summary>
        public string YMLFile_userportal_ms { get; set; }
        /// <summary>Имя YML-файла для включения в релиз userportal (pg)</summary>
        public string YMLFile_userportal_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз log_service (ms)</summary>
        public string YMLFile_log_service_ms { get; set; }
        /// <summary>Имя YML-файла для включения в релиз log_service (pg)</summary>
        public string YMLFile_log_service_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз php_log (ms)</summary>
        public string YMLFile_php_log_ms { get; set; }
        /// <summary>Имя YML-файла для включения в релиз php_log (pg)</summary>
        public string YMLFile_php_log_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз fer_log</summary>
        public string YMLFile_fer_log { get; set; }
        /// <summary>Имя YML-файла для включения в релиз ac_mlo_pg</summary>
        public string YMLFile_ac_mlo_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз ac_mlo_ms</summary>
        public string YMLFile_ac_mlo_ms { get; set; }

        /// <summary>Имя YML-файла для включения в релиз DEV MS</summary>
        public string YMLFile_dev_MS { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV PG</summary>
        public string YMLFile_dev_PG { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV EMD</summary>
        public string YMLFile_dev_EMD { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV LIS</summary>
        public string YMLFile_dev_LIS { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV userportal (ms)</summary>
        public string YMLFile_dev_userportal_ms { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV userportal (pg)</summary>
        public string YMLFile_dev_userportal_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV log_service (ms)</summary>
        public string YMLFile_dev_log_service_ms { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV log_service (pg)</summary>
        public string YMLFile_dev_log_service_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV php_log (ms)</summary>
        public string YMLFile_dev_php_log_ms { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV php_log (pg)</summary>
        public string YMLFile_dev_php_log_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV fer_log</summary>
        public string YMLFile_dev_fer_log { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV ac_mlo_pg</summary>
        public string YMLFile_dev_ac_mlo_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV ac_mlo_ms</summary>
        public string YMLFile_dev_ac_mlo_ms { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV smp2_pg</summary>
        public string YMLFile_dev_smp2_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV gar_pg</summary>
        public string YMLFile_dev_gar_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV proxy_pg</summary>
        public string YMLFile_dev_proxy_pg { get; set; }
        /// <summary>Имя YML-файла для включения в релиз DEV bi</summary>
        public string YMLFile_dev_bi { get; set; }


        /// <summary>Имя YML-файла неизвестного проекта</summary>
        public string YMLFile_unknown { get; set; }

        /// <summary>Комментарий для YML-файла релиза MS</summary>
        public string YMLFile_MS_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза PG</summary>
        public string YMLFile_PG_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза EMD</summary>
        public string YMLFile_EMD_Comment { get; set; }
        /// <summary>ИмКомментарий для я YML-файла релиза LIS</summary>
        public string YMLFile_LIS_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза userportal (ms)</summary>
        public string YMLFile_userportal_ms_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза userportal (pg)</summary>
        public string YMLFile_userportal_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза log_service (ms)</summary>
        public string YMLFile_log_service_ms_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза log_service (pg)</summary>
        public string YMLFile_log_service_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза php_log (ms)</summary>
        public string YMLFile_php_log_ms_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза php_log (pg)</summary>
        public string YMLFile_php_log_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза fer_log</summary>
        public string YMLFile_fer_log_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза ac_mlo_pg</summary>
        public string YMLFile_ac_mlo_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза ac_mlo_ms</summary>
        public string YMLFile_ac_mlo_ms_Comment { get; set; }

        /// <summary>Комментарий для YML-файла релиза DEV MS</summary>
        public string YMLFile_dev_MS_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV PG</summary>
        public string YMLFile_dev_PG_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV EMD</summary>
        public string YMLFile_dev_EMD_Comment { get; set; }
        /// <summary>ИмКомментарий для я YML-файла релиза DEV LIS</summary>
        public string YMLFile_dev_LIS_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV userportal (ms)</summary>
        public string YMLFile_dev_userportal_ms_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV userportal (pg)</summary>
        public string YMLFile_dev_userportal_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV log_service (ms)</summary>
        public string YMLFile_dev_log_service_ms_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV log_service (pg)</summary>
        public string YMLFile_dev_log_service_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV php_log (ms)</summary>
        public string YMLFile_dev_php_log_ms_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV php_log (pg)</summary>
        public string YMLFile_dev_php_log_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV fer_log</summary>
        public string YMLFile_dev_fer_log_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV ac_mlo_pg</summary>
        public string YMLFile_dev_ac_mlo_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV ac_mlo_ms</summary>
        public string YMLFile_dev_ac_mlo_ms_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV smp2_pg</summary>
        public string YMLFile_dev_smp2_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV gar_pg</summary>
        public string YMLFile_dev_gar_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV proxy_pg</summary>
        public string YMLFile_dev_proxy_pg_Comment { get; set; }
        /// <summary>Комментарий для  YML-файла релиза DEV bi</summary>
        public string YMLFile_dev_bi_Comment { get; set; }

        private string _unknown_comment;
        /// <summary>Комментарий для  YML-файла неизвестного проекта</summary>
        public string YMLFile_unknown_Comment
        {
            get
            {
                if (string.IsNullOrWhiteSpace(YMLFile_unknown)) return "";
                else return _unknown_comment ?? "";
            }
            set
            {
                _unknown_comment = value;
            }
        }

        /// <summary>
        /// список yml-файлов и комментариев к ним
        /// </summary>
        /// <returns></returns>
        public IEnumerable<ymlpair> ListYMLFiles()
        {
            yield return new ymlpair() { ymlfield = "YMLFile_MS", ymlfile = YMLFile_MS ?? "", ymlfile_comment = YMLFile_MS_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_PG", ymlfile = YMLFile_PG ?? "", ymlfile_comment = YMLFile_PG_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_EMD", ymlfile = YMLFile_EMD ?? "", ymlfile_comment = YMLFile_EMD_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_LIS", ymlfile = YMLFile_LIS ?? "", ymlfile_comment = YMLFile_LIS_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_log_service_pg", ymlfile = YMLFile_log_service_pg ?? "", ymlfile_comment = YMLFile_log_service_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_log_service_ms", ymlfile = YMLFile_log_service_ms ?? "", ymlfile_comment = YMLFile_log_service_ms_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_php_log_pg", ymlfile = YMLFile_php_log_pg ?? "", ymlfile_comment = YMLFile_php_log_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_php_log_ms", ymlfile = YMLFile_php_log_ms ?? "", ymlfile_comment = YMLFile_php_log_ms_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_userportal_pg", ymlfile = YMLFile_userportal_pg ?? "", ymlfile_comment = YMLFile_userportal_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_userportal_ms", ymlfile = YMLFile_userportal_ms ?? "", ymlfile_comment = YMLFile_userportal_ms_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_fer_log", ymlfile = YMLFile_fer_log ?? "", ymlfile_comment = YMLFile_fer_log_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_ac_mlo_pg", ymlfile = YMLFile_ac_mlo_pg ?? "", ymlfile_comment = YMLFile_ac_mlo_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_ac_mlo_ms", ymlfile = YMLFile_ac_mlo_ms ?? "", ymlfile_comment = YMLFile_ac_mlo_ms_Comment ?? "", ymlpath = PathInGIT };

            yield return new ymlpair() { ymlfield = "YMLFile_dev_MS", ymlfile = YMLFile_dev_MS ?? "", ymlfile_comment = YMLFile_dev_MS_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_PG", ymlfile = YMLFile_dev_PG ?? "", ymlfile_comment = YMLFile_dev_PG_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_EMD", ymlfile = YMLFile_dev_EMD ?? "", ymlfile_comment = YMLFile_dev_EMD_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_LIS", ymlfile = YMLFile_dev_LIS ?? "", ymlfile_comment = YMLFile_dev_LIS_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_log_service_pg", ymlfile = YMLFile_dev_log_service_pg ?? "", ymlfile_comment = YMLFile_dev_log_service_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_log_service_ms", ymlfile = YMLFile_dev_log_service_ms ?? "", ymlfile_comment = YMLFile_dev_log_service_ms_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_php_log_pg", ymlfile = YMLFile_dev_php_log_pg ?? "", ymlfile_comment = YMLFile_dev_php_log_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_php_log_ms", ymlfile = YMLFile_dev_php_log_ms ?? "", ymlfile_comment = YMLFile_dev_php_log_ms_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_userportal_pg", ymlfile = YMLFile_dev_userportal_pg ?? "", ymlfile_comment = YMLFile_dev_userportal_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_userportal_ms", ymlfile = YMLFile_dev_userportal_ms ?? "", ymlfile_comment = YMLFile_dev_userportal_ms_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_fer_log", ymlfile = YMLFile_dev_fer_log ?? "", ymlfile_comment = YMLFile_dev_fer_log_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_ac_mlo_pg", ymlfile = YMLFile_dev_ac_mlo_pg ?? "", ymlfile_comment = YMLFile_dev_ac_mlo_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_ac_mlo_ms", ymlfile = YMLFile_dev_ac_mlo_ms ?? "", ymlfile_comment = YMLFile_dev_ac_mlo_ms_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_smp2_pg", ymlfile = YMLFile_dev_smp2_pg ?? "", ymlfile_comment = YMLFile_dev_smp2_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_gar_pg", ymlfile = YMLFile_dev_gar_pg ?? "", ymlfile_comment = YMLFile_dev_gar_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_proxy_pg", ymlfile = YMLFile_dev_proxy_pg ?? "", ymlfile_comment = YMLFile_dev_proxy_pg_Comment ?? "", ymlpath = PathInGIT };
            yield return new ymlpair() { ymlfield = "YMLFile_dev_bi", ymlfile = YMLFile_dev_bi ?? "", ymlfile_comment = YMLFile_dev_bi_Comment ?? "", ymlpath = PathInGIT };

            yield return new ymlpair() { ymlfield = "YMLFile_unknown", ymlfile = YMLFile_unknown ?? "", ymlfile_comment = YMLFile_unknown_Comment ?? "", ymlpath = PathInGIT }; //-V3022
        }

        /// <summary>
        /// Вернуть имя yml-файла
        /// </summary>
        /// <param name="YMLField">поле на форме "Сборка релиза"</param>
        /// <returns></returns>
        public string GetYMLFile(string YMLField)
        {
            foreach (ymlpair item in ListYMLFiles())
            {
                if (item.ymlfield == YMLField)
                {
                    return item.ymlfile ?? "";
                }
            }
            return YMLFile_unknown ?? "";
        }

        /// <summary>
        /// Изменить имя yml-файла
        /// </summary>
        /// <param name="YMLField">поле на форме "Сборка релиза"</param>
        /// <param name="Value">новое имя</param>
        public void SetYMLFile(string YMLField, string Value)
        {
            if (YMLField == "YMLFile_MS") YMLFile_MS = Value;
            else if (YMLField == "YMLFile_PG") YMLFile_PG = Value;
            else if (YMLField == "YMLFile_EMD") YMLFile_EMD = Value;
            else if (YMLField == "YMLFile_LIS") YMLFile_LIS = Value;
            else if (YMLField == "YMLFile_log_service_pg") YMLFile_log_service_pg = Value;
            else if (YMLField == "YMLFile_log_service_ms") YMLFile_log_service_ms = Value;
            else if (YMLField == "YMLFile_php_log_pg") YMLFile_php_log_pg = Value;
            else if (YMLField == "YMLFile_php_log_ms") YMLFile_php_log_ms = Value;
            else if (YMLField == "YMLFile_userportal_pg") YMLFile_userportal_pg = Value;
            else if (YMLField == "YMLFile_userportal_ms") YMLFile_userportal_ms = Value;
            else if (YMLField == "YMLFile_fer_log") YMLFile_fer_log = Value;
            else if (YMLField == "YMLFile_ac_mlo_pg") YMLFile_ac_mlo_pg = Value;
            else if (YMLField == "YMLFile_ac_mlo_ms") YMLFile_ac_mlo_ms = Value;
            else if (YMLField == "YMLFile_dev_MS") YMLFile_dev_MS = Value;
            else if (YMLField == "YMLFile_dev_PG") YMLFile_dev_PG = Value;
            else if (YMLField == "YMLFile_dev_EMD") YMLFile_dev_EMD = Value;
            else if (YMLField == "YMLFile_dev_LIS") YMLFile_dev_LIS = Value;
            else if (YMLField == "YMLFile_dev_log_service_pg") YMLFile_dev_log_service_pg = Value;
            else if (YMLField == "YMLFile_dev_log_service_ms") YMLFile_dev_log_service_ms = Value;
            else if (YMLField == "YMLFile_dev_php_log_pg") YMLFile_dev_php_log_pg = Value;
            else if (YMLField == "YMLFile_dev_php_log_ms") YMLFile_dev_php_log_ms = Value;
            else if (YMLField == "YMLFile_dev_userportal_pg") YMLFile_dev_userportal_pg = Value;
            else if (YMLField == "YMLFile_dev_userportal_ms") YMLFile_dev_userportal_ms = Value;
            else if (YMLField == "YMLFile_dev_fer_log") YMLFile_dev_fer_log = Value;
            else if (YMLField == "YMLFile_dev_ac_mlo_pg") YMLFile_dev_ac_mlo_pg = Value;
            else if (YMLField == "YMLFile_dev_ac_mlo_ms") YMLFile_dev_ac_mlo_ms = Value;
            else if (YMLField == "YMLFile_dev_smp2_pg") YMLFile_dev_smp2_pg = Value;
            else if (YMLField == "YMLFile_dev_gar_pg") YMLFile_dev_gar_pg = Value;
            else if (YMLField == "YMLFile_dev_proxy_pg") YMLFile_dev_proxy_pg = Value;
            else if (YMLField == "YMLFile_dev_bi") YMLFile_dev_bi = Value;
            else YMLFile_unknown = Value;
        }

        /// <summary>
        /// Вернуть комментарий
        /// </summary>
        /// <param name="YMLField">поле на форме "Сборка релиза"</param>
        /// <returns></returns>
        public string GetYMLFile_Comment(string YMLField)
        {
            foreach (ymlpair item in ListYMLFiles())
            {
                if (item.ymlfield == YMLField)
                {
                    return item.ymlfile_comment ?? "";
                }
            }
            if (!string.IsNullOrWhiteSpace(YMLField)) return "Неизвестный проект GIT";
            else return "";
        }

        /// <summary>
        /// Записать комментарий
        /// </summary>
        /// <param name="YMLField">поле на форме "Сборка релиза"</param>
        /// <param name="Value">новое значение</param>
        public void SetYMLFile_Comment(string YMLField, string Value)
        {
            if (YMLField == "YMLFile_MS") YMLFile_MS_Comment = Value;
            else if (YMLField == "YMLFile_PG") YMLFile_PG_Comment = Value;
            else if (YMLField == "YMLFile_EMD") YMLFile_EMD_Comment = Value;
            else if (YMLField == "YMLFile_LIS") YMLFile_LIS_Comment = Value;
            else if (YMLField == "YMLFile_log_service_pg") YMLFile_log_service_pg_Comment = Value;
            else if (YMLField == "YMLFile_log_service_ms") YMLFile_log_service_ms_Comment = Value;
            else if (YMLField == "YMLFile_php_log_pg") YMLFile_php_log_pg_Comment = Value;
            else if (YMLField == "YMLFile_php_log_ms") YMLFile_php_log_ms_Comment = Value;
            else if (YMLField == "YMLFile_userportal_pg") YMLFile_userportal_pg_Comment = Value;
            else if (YMLField == "YMLFile_userportal_ms") YMLFile_userportal_ms_Comment = Value;
            else if (YMLField == "YMLFile_fer_log") YMLFile_fer_log_Comment = Value;
            else if (YMLField == "YMLFile_ac_mlo_pg") YMLFile_ac_mlo_pg_Comment = Value;
            else if (YMLField == "YMLFile_ac_mlo_ms") YMLFile_ac_mlo_ms_Comment = Value;
            else if (YMLField == "YMLFile_dev_MS") YMLFile_dev_MS_Comment = Value;
            else if (YMLField == "YMLFile_dev_PG") YMLFile_dev_PG_Comment = Value;
            else if (YMLField == "YMLFile_dev_EMD") YMLFile_dev_EMD_Comment = Value;
            else if (YMLField == "YMLFile_dev_LIS") YMLFile_dev_LIS_Comment = Value;
            else if (YMLField == "YMLFile_dev_log_service_pg") YMLFile_dev_log_service_pg_Comment = Value;
            else if (YMLField == "YMLFile_dev_log_service_ms") YMLFile_dev_log_service_ms_Comment = Value;
            else if (YMLField == "YMLFile_dev_php_log_pg") YMLFile_dev_php_log_pg_Comment = Value;
            else if (YMLField == "YMLFile_dev_php_log_ms") YMLFile_dev_php_log_ms_Comment = Value;
            else if (YMLField == "YMLFile_dev_userportal_pg") YMLFile_dev_userportal_pg_Comment = Value;
            else if (YMLField == "YMLFile_dev_userportal_ms") YMLFile_dev_userportal_ms_Comment = Value;
            else if (YMLField == "YMLFile_dev_fer_log") YMLFile_dev_fer_log_Comment = Value;
            else if (YMLField == "YMLFile_dev_ac_mlo_pg") YMLFile_dev_ac_mlo_pg_Comment = Value;
            else if (YMLField == "YMLFile_dev_ac_mlo_ms") YMLFile_dev_ac_mlo_ms_Comment = Value;
            else if (YMLField == "YMLFile_dev_smp2_pg") YMLFile_dev_smp2_pg_Comment = Value;
            else if (YMLField == "YMLFile_dev_gar_pg") YMLFile_dev_gar_pg_Comment = Value;
            else if (YMLField == "YMLFile_dev_proxy_pg") YMLFile_dev_proxy_pg_Comment = Value;
            else if (YMLField == "YMLFile_dev_bi") YMLFile_dev_bi_Comment = Value;
            else if (YMLField == "YMLFile_unknown") YMLFile_unknown_Comment = Value;
            else if ((!string.IsNullOrWhiteSpace(YMLField)) && (!string.IsNullOrWhiteSpace(Value))) YMLFile_unknown_Comment = Value;
            else if (!string.IsNullOrWhiteSpace(YMLField)) YMLFile_unknown_Comment = "Неизвестный проект GIT";
        }

        private string _order;
        /// <summary>Поле для упорядочивания = по имени yml-Файла, но с учетом порядка yml-файлов в одной задаче</summary>
        public string Order
        {
            get
            {
                return _order ?? "";
            }
            set
            {
                _order = value;
                if (string.IsNullOrWhiteSpace(_order)) _order = "";

                NotifyPropertyChanged("Order");
            }
        }

        private string _databd;
        /// <summary>Данные БД</summary>
        public string DataBD
        {
            get
            {
                return _databd ?? "";
            }
            set
            {
                _databd = value;
                if (string.IsNullOrWhiteSpace(_databd)) _databd = "";

                NotifyPropertyChanged("DataBD");
            }
        }

        private string _objectbd;
        /// <summary>Объекты БД</summary>
        public string ObjectsBD
        {
            get
            {
                return _objectbd ?? "";
            }
            set
            {
                _objectbd = value;
                if (string.IsNullOrWhiteSpace(_objectbd)) _objectbd = "";

                NotifyPropertyChanged("ObjectsBD");
            }
        }

        private string _updactions;
        /// <summary>Действия при обновлении</summary>
        public string UpdActions
        {
            get
            {
                return _updactions ?? "";
            }
            set
            {
                _updactions = value;
                if (string.IsNullOrWhiteSpace(_updactions)) _updactions = "";

                NotifyPropertyChanged("UpdActions");
            }
        }

        private bool _isdowntime;
        /// <summary>Требуется Downtime</summary>
        public bool IsDowntime
        {
            get
            {
                return _isdowntime;
            }
            set
            {
                _isdowntime = value == true;

                NotifyPropertyChanged("IsDowntime");
            }
        }


        private bool _isaddrelease;
        /// <summary>Добавить в релиз</summary>
        public bool IsAddRelease
        {
            get
            {
                return _isaddrelease;
            }
            set
            {
                _isaddrelease = value == true;

                NotifyPropertyChanged("IsAddRelease");
            }
        }

        private bool _isfiltered1;
        /// <summary>Включен в фильтр 1</summary>
        public bool IsFiltered1
        {
            get
            {
                return _isfiltered1;
            }
            set
            {
                _isfiltered1 = value == true;

                NotifyPropertyChanged("IsFiltered1");
            }
        }

        private bool _isfiltered2;
        /// <summary>Включен в фильтр 2</summary>
        public bool IsFiltered2
        {
            get
            {
                return _isfiltered2;
            }
            set
            {
                _isfiltered2 = value == true;

                NotifyPropertyChanged("IsFiltered2");
            }
        }

        private bool _isfiltered3;
        /// <summary>Включен в фильтр 3</summary>
        public bool IsFiltered3
        {
            get
            {
                return _isfiltered3;
            }
            set
            {
                _isfiltered3 = value == true;

                NotifyPropertyChanged("IsFiltered3");
            }
        }

        private bool _isfiltered4;
        /// <summary>Включен в фильтр 4</summary>
        public bool IsFiltered4
        {
            get
            {
                return _isfiltered4;
            }
            set
            {
                _isfiltered4 = value == true;

                NotifyPropertyChanged("IsFiltered4");
            }
        }

        private bool _isbaseregion;
        /// <summary>Базовая региональность БД</summary>
        public bool IsBaseRegion
        {
            get
            {
                return _isbaseregion;
            }
            set
            {
                _isbaseregion = value == true;

                NotifyPropertyChanged("IsBaseRegion");
            }
        }

        /// <summary>=false - не был обновлен из Jira</summary>
        public bool IsRefreshed;

        /// <summary>
        /// Копирование экземпляра YMLFileInfo
        /// </summary>
        /// <returns></returns>
        public YMLFileInfo Copy()
        {
            return (YMLFileInfo)this.MemberwiseClone();
        }

        /// <summary>
        /// YML-файл (первый попавшийся)
        /// </summary>
        public string GetYMLFileDefault
        {
            get
            {
                foreach (var item in ListYMLFiles())
                {
                    if (!string.IsNullOrWhiteSpace(item.ymlfile)) return item.ymlfile;
                }
                return "";
            }
        }

        /// <summary>
        /// Есть yml-файл в задаче
        /// </summary>
        public bool IsYMLFileExist
        {
            get
            {
                return !string.IsNullOrWhiteSpace(GetYMLFileDefault);
            }
        }

        /// <summary>
        /// Нет yml-файла в задаче
        /// </summary>
        public bool IsYMLFileNotExist
        {
            get
            {
                return string.IsNullOrWhiteSpace(GetYMLFileDefault);
            }
        }


        /// <summary>
        /// Есть ошибки при проверке
        /// </summary>
        public bool IsYMLFileCommentExist
        {
            get
            {
                foreach (var item in ListYMLFiles())
                {
                    if (!string.IsNullOrWhiteSpace(item.ymlfile_comment)) return true;
                }
                return false;
            }
        }

        private string _taskcommitdate;
        /// <summary>Дата последнего коммита в задаче</summary>
        public string TaskCommitDate
        {
            get
            {
                return _taskcommitdate;
            }
            set
            {
                _taskcommitdate = value;

                NotifyPropertyChanged("TaskCommitDate");
            }
        }

        /// <summary>
        /// Список проектов по типу региона основной БД
        /// </summary>
        /// <param name="_dbregion">тип региона основной БД</param>
        /// <returns></returns>
        public List<string> ListProjectsByDBRegion(string _dbregion)
        {
            List<string> result = new List<string>();

            foreach (var info in ListYMLFiles())
            {
                if (
                    !string.IsNullOrWhiteSpace(info.ymlfile) &&
                    !string.IsNullOrWhiteSpace(info.ymlfield)
                )
                {
                    string project = Utilities.GITProjects.GetProjectByYMLField(info.ymlfield);

                    if (
                        _dbregion == "MS SQL" &&
                        MainWindow.APPinfo.ListProjectsMS.Contains(project) &&
                        !result.Contains(project) &&
                        !string.IsNullOrWhiteSpace(project)
                    )
                    {
                        result.Add(project);
                    }

                    if (
                        _dbregion == "PG SQL" &&
                        MainWindow.APPinfo.ListProjectsPG.Contains(project) &&
                        !result.Contains(project) &&
                        !string.IsNullOrWhiteSpace(project)
                    )
                    {
                        result.Add(project);
                    }
                }
            }

            return result;
        }
    }

    // =========================================================================================================
    /// <summary>связь поля на форме "Сборка релиза", yml-файла и результатов проверки yml-файла</summary>
    public class ymlpair
    {
        /// <summary>
        /// поле на форме "Сборка релиза"
        /// </summary>
        public string ymlfield { get; set; }
        /// <summary>
        /// папка с yml-файлом (task, deployment, cron)
        /// </summary>
        public string ymlpath { get; set; }
        /// <summary>
        /// yml-файл
        /// </summary>
        public string ymlfile { get; set; }
        /// <summary>
        /// результат проверки yml-файла
        /// </summary>
        public string ymlfile_comment { get; set; }
    }
}
