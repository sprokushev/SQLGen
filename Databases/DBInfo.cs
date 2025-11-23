// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using SQLGen.Utilities;

namespace SQLGen
{
    /// <summary>Роли БД</summary>
    public enum DBRoleType
    {
        /// <summary>
        /// тестовая
        /// </summary>
        TEST,
        /// <summary>
        /// релизная
        /// </summary>
        RELEASE,
        /// <summary>
        /// продлайк
        /// </summary>
        PRODLIKE,
        /// <summary>
        /// прод
        /// </summary>
        PROD,
        /// <summary>
        /// отчетная
        /// </summary>
        REPORT,
        /// <summary>
        /// реестровая
        /// </summary>
        REESTR
    }

    /// <summary>Класс описания БД</summary>
    public class DBInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Копирование описания БД
        /// </summary>
        /// <returns></returns>
        public DBInfo Copy()
        {
            return (DBInfo)this.MemberwiseClone();
        }

        /// <summary>
        /// реализация OnPropertyChanged
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// реализация OnPropertyChanged
        /// </summary>
        /// <param name="prop">prop</param>
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop)); //-V3083
        }

        private string _servername;
        /// <summary>Имя сервера или ip-адрес базы данных вместе с портом</summary>
        public string ServerName
        {
            get
            {
                return _servername ?? "";
            }
            set
            {
                _servername = value;
                if (string.IsNullOrWhiteSpace(_servername)) _servername = "";
                _servername = _servername.Replace(" ", "").Trim();

                OnPropertyChanged("ServerName");
            }
        }

        /// <summary>
        /// адрес сервера без порта
        /// </summary>
        public string ServerAddr => Utilities.Databases.GetAddrFromServerName(ServerName);

        /// <summary>
        /// порт сервера
        /// </summary>
        public string ServerPort => Utilities.Databases.GetPortFromServerName(ServerName);


        private string _dbname;
        /// <summary>Имя базы данных</summary>
        public string DBName
        {
            get
            {
                return _dbname ?? "";
            }
            set
            {
                _dbname = value;
                if (string.IsNullOrWhiteSpace(_dbname)) _dbname = "";
                _dbname = _dbname.Replace(" ", "").Trim();

                OnPropertyChanged("DBName");
            }
        }

        private string _gitproject;
        /// <summary>Проект GIT</summary>
        public string GITProject
        {
            get
            {
                return _gitproject ?? "";
            }
            set
            {
                _gitproject = value;
                if (string.IsNullOrWhiteSpace(_gitproject)) _gitproject = "dev_promed_pg";
                _gitproject = _gitproject.Replace(" ", "").Trim();

                OnPropertyChanged("GITProject");
            }
        }

        /// <summary>Роль БД</summary>
        [JsonIgnore]
        public DBRoleType DBRoleType;

        /// <summary>Вид БД - строка</summary>
        public string DBRole
        {
            get
            {
                switch (DBRoleType)
                {
                    case DBRoleType.RELEASE:
                        return "RELEASE";
                    case DBRoleType.PROD:
                        return "PROD";
                    case DBRoleType.PRODLIKE:
                        return "PRODLIKE";
                    case DBRoleType.REPORT:
                        return "REPORT";
                    case DBRoleType.REESTR:
                        return "REESTR";
                    case DBRoleType.TEST:
                    default:
                        return "TEST";
                }

            }

            set
            {
                string _role = value;
                if (string.IsNullOrWhiteSpace(_role)) _role = "";
                _role = _role.Replace(" ", "").Trim().ToUpper();

                if (_role == "RELEASE") DBRoleType = DBRoleType.RELEASE;
                else if (_role == "PROD") DBRoleType = DBRoleType.PROD;
                else if (_role == "PRODLIKE") DBRoleType = DBRoleType.PRODLIKE;
                else if (_role == "REPORT") DBRoleType = DBRoleType.REPORT;
                else if (_role == "REESTR") DBRoleType = DBRoleType.REESTR;
                else DBRoleType = DBRoleType.TEST;

                OnPropertyChanged("DBRole");
            }
        }

        private bool _ismaintest;
        /// <summary>Основная тестовая БД для соответствующего проекта GIT</summary>
        public bool isMainTest
        {
            get
            {
                return _ismaintest;
            }
            set
            {
                _ismaintest = value == true;

                OnPropertyChanged("isMainTest");
            }
        }

        private string _dbtype;
        /// <summary>Тип базы данных</summary>
        public string DBType
        {
            get
            {
                return _dbtype ?? "";
            }
            set
            {
                _dbtype = value;
                if (string.IsNullOrWhiteSpace(_dbtype)) _dbtype = "";
                _dbtype = _dbtype.Replace(" ", "").Trim().ToUpper();

                if (
                    (_dbtype == "PGSQL") ||
                    (_dbtype == "MSSQL")
                )
                {
                    // ничего не делаем
                }
                else
                {
                    // пытаемся автоматически определить тип БД
                    _dbtype = Utilities.GITProjects.GetDBTypeByProject(GITProject);
                }

                if (string.IsNullOrWhiteSpace(_dbtype))
                {
                    _dbtype = "PGSQL";
                }

                OnPropertyChanged("DBType");
            }
        }
    }
}
