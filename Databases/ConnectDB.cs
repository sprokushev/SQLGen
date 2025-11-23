// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using SQLGen.Utilities;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.SqlServer.Management.Smo.RegSvrEnum;
using System.Windows.Documents;

namespace SQLGen
{

    public partial class MainWindow
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Коннект к текущей БД</summary>
        public static ConnectDB MainConnect;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список с историей коннектов из файла ListConnect.json</summary>
        public static List<ConnectDB> ListConnects = new List<ConnectDB>();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Загрузить список коннектов</summary>
        public static void LoadConnects()
        {
            string filename = Path.Combine(App.AppPath, "ListConnects.json");
            if (File.Exists(filename))
            {
                try
                {
                    string jsonString = File.ReadAllText(filename);
                    ListConnects = JsonSerializer.Deserialize<List<ConnectDB>>(jsonString).OrderBy(x => x.DBConnectionName).ToList();

                    // проставляем дефолтные значения для пустых полей
                    foreach (var item in ListConnects)
                    {
                        item.ServerName = item.ServerName;
                        item.DBName = item.DBName;
                        item.Username = item.Username;
                    }

                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Сохранить список коннектов</summary>
        public static void SaveConnects()
        {
            var options = new JsonSerializerOptions
            {
                IgnoreReadOnlyProperties = true,
                WriteIndented = true
            };

            string filename = Path.Combine(App.AppPath, "ListConnects.json");
            string jsonString = "";

            Utilities.Files.BackupFile(filename);

            if (!string.IsNullOrWhiteSpace(filename)) //-V3022
            {
                try
                {
                    var list = new List<ConnectDB>();
                    foreach (var conn in ListConnects)
                    {
                        var item = conn.Copy();
                        if (!item.isSavePassword) item.Password = "";
                        list.Add(item);
                    }

                    jsonString = JsonSerializer.Serialize<List<ConnectDB>>(list, options);
                    File.WriteAllText(filename, jsonString);
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                }
            }
        }

        /// <summary>
        /// Вернуть ConnectDB основной тестовой для проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <param name="db">наименование БД</param>
        /// <returns></returns>
        public static ConnectDB GetMainTestConnectByGITProject(string project, string db)
        {
            foreach (var con in ListConnects.Where(x => (x.GITProject == project) && string.IsNullOrWhiteSpace(db) && (x.isMainTest == true)))
            {
                return con;
            }

            foreach (var con in ListConnects.Where(x => (x.GITProject == project) && (!string.IsNullOrWhiteSpace(db)) && (x.isTest == true) && (x.DBName.ToLower() == db.ToLower())))
            {
                return con;
            }

            foreach (var con in ListConnects.Where(x => (x.GITProject == project) && (!string.IsNullOrWhiteSpace(db)) && (x.isRelease == true) && (x.DBName.ToLower() == db.ToLower())))
            {
                return con;
            }

            return null;
        }
    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>Класс подключения к БД</summary>
    public class ConnectDB : IDisposable //-V3073
    {
        // -------------------------------------------------------------------------------------------------------
        // Чтобы закрывать сессию при удалении объекта

        private bool disposed = false;

        /// <summary>
        /// Реализация интерфейса IDisposable
        /// </summary>
        public void Dispose()
        {
            // освобождаем неуправляемые ресурсы
            Dispose(true);
            // подавляем финализацию
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Реализация интерфейса IDisposable
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
                // Освобождаем управляемые ресурсы
                CloseConnect();
            }
            // освобождаем неуправляемые объекты
            disposed = true;
        }

        /// <summary>
        /// Деструктор ConnectDB
        /// </summary>
        ~ConnectDB()
        {
            Dispose(false);
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>конструктор класса ConnectDB</summary>
        public ConnectDB()
        {
            this.ConnType = Utilities.ConnType.None;
            this.isConnected = false;
            this.AuthType = Utilities.AuthType.WINDOWS;
            this.DbConn = null;
            this.DBConnectionName = "";
            this.isSavePassword = false;
            this.isTrustServerCertificate = true;
            this.connectionAdd = "";
            this.Password = "";
            this.Timeout = 120;
        }

        // -------------------------------------------------------------------------------------------------------
        Utilities.ConnType _connType;
        /// <summary>Тип соединения</summary>
        public Utilities.ConnType ConnType
        {
            get
            {
                return _connType;
            }
            set
            {
                _connType = value;
                if (_connType == Utilities.ConnType.None)
                {
                    _connType = Utilities.ConnType.PGSQL;
                }

                if (_connType != Utilities.ConnType.MSSQL) 
                {
                    isTrustServerCertificate = false;
                }
            }
        }

        /// <summary>Тип базы данных</summary>
        [JsonIgnore]
        public string DBType
        {
            get
            {
                switch (ConnType)
                {
                    case Utilities.ConnType.DBF:
                        return "DBF";
                    case Utilities.ConnType.MSSQL:
                        return "MSSQL";
                    case Utilities.ConnType.PGSQL:
                    case Utilities.ConnType.None:
                    default:
                        return "PGSQL";
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Если успешный коннект к текущей базе данных</summary>
        [JsonIgnore]
        public bool isConnected;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Если НЕ успешный коннект к текущей базе данных</summary>
        [JsonIgnore]
        public bool isNotConnected
        {
            get
            {
                return !isConnected;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>DbConnection для использования в приложении</summary>
        [JsonIgnore]
        internal DbConnection DbConn;

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Название соединения</summary>
        public string DBConnectionName { get; set; }

        // -------------------------------------------------------------------------------------------------------
        private string _server;
        /// <summary>Сервер текущей базы данных (имя сервера или адрес с портом)</summary>
        public string ServerName
        {
            get
            {
                return _server ?? "";
            }
            set
            {
                _server = value;
                if (string.IsNullOrWhiteSpace(_server))
                {
                    _server = "172.29.3.254";
                }
                _server = _server.Replace(" ","").Trim();
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

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Сервер текущей базы данных (для добавления в строку подключения)</summary>
        public string ServerNameToConnect
        {
            get
            {
                return this.ServerName;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        string _db;
        /// <summary>Имя текущей базы данных</summary>
        public string DBName
        {
            get
            {
                return _db ?? "";
            }
            set
            {
                _db = value;
                if (string.IsNullOrWhiteSpace(_db))
                {
                    _db = "promedtest";
                }
                _db = _db.Replace(" ","").Trim();
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Имя текущей базы данных (для добавления в строку подключения)</summary>
        public string DBNameToConnect
        {
            get
            {
                return this.DBName;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Тип авторизации</summary>
        public Utilities.AuthType AuthType { get; set; }

        // -------------------------------------------------------------------------------------------------------
        string _user;
        /// <summary>Имя пользователя</summary>
        public string Username
        {
            get
            {
                return _user ?? "";
            }
            set
            {
                _user = value;
                if (string.IsNullOrWhiteSpace(_user)) _user = "";
                _user = _user.Trim();
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Зашифрованный пароль</summary>
        public string CryptedPassword { get; set; }

        /// <summary>Пароль</summary>
        [JsonIgnore]
        public string Password
        {
            get
            {
                return CryptoClass.decrypt_from_string(CryptedPassword);
            }
            set
            {
                CryptedPassword = CryptoClass.encrypt_to_string(value);
            }
        }

        /// <summary>Флаг сохранения пароля</summary>
        public bool isSavePassword { get; set; }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Таймаут (сек)</summary>
        public int Timeout { get; set; }

        // -------------------------------------------------------------------------------------------------------
        string _connectionadd;
        /// <summary>Дополнительные параметры в строку подключения</summary>
        public string connectionAdd
        {
            get
            {
                return _connectionadd ?? "";
            }
            set
            {
                _connectionadd = value;
                if (string.IsNullOrWhiteSpace(_connectionadd)) _connectionadd = "";
                _connectionadd = _connectionadd.Replace('\t', ' ').Replace(Environment.NewLine, ";").Replace('\n', ';');
                _connectionadd = _connectionadd.Trim(new char[] { ' ', ';' });
            }
        }

        /// <summary>Флаг доверия сертификатам сервера</summary>
        public bool isTrustServerCertificate { get; set; }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Строка подключения</summary>
        public string connectionString
        {
            get
            {
                string result = "";

                string _password = "'" + Password.Replace("'", "''") + "'";

                switch (ConnType)
                {
                    case Utilities.ConnType.MSSQL:
                        result =  $"data source={ServerNameToConnect};initial catalog={DBNameToConnect};connect timeout={Timeout.ToString()};application name=SQLGen";

                        if (isTrustServerCertificate)
                        {
                            result += ";trust server certificate=True";
                        }

                        if (!string.IsNullOrWhiteSpace(connectionAdd))
                        {
                            result += $";{connectionAdd}";
                        }

                        switch (AuthType)
                        {
                            case Utilities.AuthType.WINDOWS:
                                result += ";integrated security=true";
                                break;
                            case Utilities.AuthType.DATABASE:
                                result += ";User ID=" + Username + ";Password=" + _password;
                                break;
                            default:
                                break;
                        }
                        break;
                    case Utilities.ConnType.PGSQL:
                        result = $"Timeout={Timeout};CommandTimeout={Timeout};Host={ServerNameToConnect};Database={DBNameToConnect};ApplicationName=SQLGen";

                        if (!string.IsNullOrWhiteSpace(connectionAdd))
                        {
                            result += $";{connectionAdd}";
                        }

                        switch (AuthType)
                        {
                            case Utilities.AuthType.WINDOWS:
                                //result += ";Integrated Security=true";
                                break;
                            case Utilities.AuthType.DATABASE:
                                result += ";Username=" + Username + ";Password=" + _password;
                                break;
                            default:
                                break;
                        }
                        break;
                    case Utilities.ConnType.DBF:
                        result = MainWindow.APPinfo.DBFConn.Replace("%PATH%", DBNameToConnect);
                        break;
                    case Utilities.ConnType.None:
                    default:
                        break;
                }

                return result;
            }
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Имя пользователя (для добавления в строку подключения)</summary>
        public string UsernameToConnect
        {
            get
            {
                return this.Username;
            }
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Сервер из списка серверов</summary>
        public DBInfo ServerDB
        {
            get
            {
                DBInfo result = null;

                try
                {
                    switch (this.ConnType)
                    {
                        case Utilities.ConnType.MSSQL:
                            result = MainWindow.APPinfo.FindDatabase(ServerName, DBName, "MSSQL");
                            break;
                        case Utilities.ConnType.DBF:
                            result = null;
                            break;
                        case Utilities.ConnType.PGSQL:
                        case Utilities.ConnType.None:
                        default:
                            result = MainWindow.APPinfo.FindDatabase(ServerName, DBName, "PGSQL");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    result = null;
                    App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                }

                if (result != null) return result;
                else
                {
                    // сервер не найден в списке серверов, заполним значениями по умолчанию
                    string _project;

                    switch (this.ConnType)
                    {
                        case ConnType.MSSQL:
                            _project = "dev_promed_ms";
                            break;
                        case ConnType.DBF:
                            _project = "unknown";
                            break;
                        case ConnType.PGSQL:
                        case ConnType.None:
                        default:
                            _project = "dev_promed_pg";
                            break;
                    }

                    return new DBInfo() { ServerName = this.ServerName, DBName = this.DBName, isMainTest = false, DBRoleType = DBRoleType.PROD, GITProject = _project };
                }
            }
        }


        // -------------------------------------------------------------------------------------------------------
        /// <summary>Тестовая БД</summary>
        public bool isTest
        {
            get
            {
                return ServerDB.DBRoleType == DBRoleType.TEST;
            }
        }

        /// <summary>Релизная БД</summary>
        public bool isRelease
        {
            get
            {
                return ServerDB.DBRoleType == DBRoleType.RELEASE;
            }
        }

        /// <summary>Основная тестовая БД</summary>
        public bool isMainTest
        {
            get
            {
                return (ServerDB.DBRoleType == DBRoleType.TEST) && ServerDB.isMainTest;
            }
        }

        /// <summary>Проект GIT</summary>
        public string GITProject
        {
            get
            {
                return Utilities.GITProjects.GetProjectByProject(ServerDB.GITProject);
            }
        }

        /// <summary>
        /// Это база promed
        /// </summary>
        public bool isPromed
        {
            get
            {
                return
                    (Utilities.GITProjects.GetDEVProject(this.GITProject) == "dev_promed_ms") ||
                    (Utilities.GITProjects.GetDEVProject(this.GITProject) == "dev_promed_pg");
            }
        }

        /// <summary>
        /// Тип целевой БД для проекта GIT
        /// </summary>
        /// <param name="project">проект</param>
        /// <returns></returns>
        public static Utilities.TargetDBType GetTargetDBTypeByProject(string project)
        {
            string DBType = Utilities.GITProjects.GetDBTypeByProject(project);

            if (DBType == "MSSQL")
            {
                return Utilities.TargetDBType.MSSQL;
            }
            else if (
                (DBType == "PGSQL") &&
                (
                    (project == "emd") ||
                    (project == "dev_emd_pg")
                )
            )
            {
                return Utilities.TargetDBType.EMD;
            }
            else
            {
                return Utilities.TargetDBType.PGSQL;
            }
               
        }

        /// <summary>
        /// Тип соединения для проекта GIT
        /// </summary>
        /// <param name="project">проект GIT</param>
        /// <returns></returns>
        public static Utilities.ConnType GetConnTypeByProject(string project)
        {
            string DBType = Utilities.GITProjects.GetDBTypeByProject(project);

            if (DBType == "MSSQL")
                return Utilities.ConnType.MSSQL;
            else
                return Utilities.ConnType.PGSQL;
        }

        /// <summary>Целевая БД</summary>
        public Utilities.TargetDBType TargetDBType
        {
            get
            {
                return GetTargetDBTypeByProject(this.GITProject);
            }
        }

        /// <summary>
        /// Тип экземпляра БД
        /// </summary>
        public InstanceDBType InstanceDBType
        {
            get
            {
                if (isTest && isPromed && (ConnType == ConnType.MSSQL) && (DBName.ToLower() == "promeddev")) return InstanceDBType.PROMEDDEV_MS;
                else if (isTest && isPromed && (ConnType == ConnType.MSSQL) && (DBName.ToLower() == "promedtest")) return InstanceDBType.PROMEDTEST_MS;
                else if (isTest && isPromed && (ConnType == ConnType.MSSQL) && (DBName.ToLower() == "promedufa")) return InstanceDBType.PROMEDUFA_MS;
                else if (isRelease && isPromed && (ConnType == ConnType.MSSQL) && (DBName.ToLower() == "promedwebrelease")) return InstanceDBType.PROMEDWEBRELEASE_MS;
                else if (isRelease && isPromed && (ConnType == ConnType.MSSQL) && (DBName.ToLower() == "promedwebufarelease")) return InstanceDBType.PROMEDWEBUFARELEASE_MS;
                else if (isTest && isPromed && (ConnType == ConnType.PGSQL) && (DBName.ToLower() == "promedtest")) return InstanceDBType.PROMEDTEST_PG;
                else if (isTest && isPromed && (ConnType == ConnType.PGSQL) && (DBName.ToLower() == "promedadygea")) return InstanceDBType.PROMEDADYGEA_PG;
                else if (isRelease && isPromed && (ConnType == ConnType.PGSQL) && (DBName.ToLower() == "promedrelease")) return InstanceDBType.PROMEDRELEASE_PG;
                else return InstanceDBType.OTHER;
            }
        }



        // -------------------------------------------------------------------------------------------------------
        ///<summary>Закрыть подключение</summary>
        public void CloseConnect()
        {
            this.isConnected = false;
            if (this.DbConn != null)
            {
                try
                {
                    this.DbConn.Close();
                }
                finally
                {
                    this.DbConn.Dispose();
                }
            }
        }

        // -------------------------------------------------------------------------------------------------------
        ///<summary>Открыть соединение с БД</summary>
        private bool DBConnOpen()
        {
            // записать в лог-файл строку подключения без пароля
            string s = this.connectionString;
            int p = s.IndexOf(";Password");
            if (p == 0) s = "";
            if (p > 0) s = s.Substring(0, p);
            App.AddLog(s, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);

            try
            {
                switch (ConnType)
                {
                    case Utilities.ConnType.MSSQL:
                        this.DbConn = new SqlConnection(this.connectionString);
                        break;
                    case Utilities.ConnType.PGSQL:
                        this.DbConn = new NpgsqlConnection(this.connectionString);
                        break;
                    case Utilities.ConnType.DBF:
                        this.DbConn = new OdbcConnection(this.connectionString);
                        break;
                    case Utilities.ConnType.None:
                    default:
                        this.DbConn = null;
                        break;
                }

                if (this.DbConn != null)
                {
                    this.DbConn.Open();
                    if (this.DbConn.State == System.Data.ConnectionState.Open)
                    {
                        App.AddLog("ЕСТЬ подключение", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                this.DbConn = null;
            };

            App.AddLog("НЕТ подключения", null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
            return false;
        }

        // -------------------------------------------------------------------------------------------------------
        ///<summary>Переоткрыть подключение</summary>
        public void ReConnect()
        {
            OpenConnect(false);
        }

        // -------------------------------------------------------------------------------------------------------
        ///<summary>Открыть подключение</summary>
        ///<param name="isNew">=true принудительно открыть форму "Подключение к БД"</param>
        public void OpenConnect(bool isNew)
        {
            // закрываем соединение, если оно открыто
            CloseConnect();

            if (isNew)
                this.isConnected = false;
            else
            {
                if ((this.AuthType == Utilities.AuthType.DATABASE) && string.IsNullOrWhiteSpace(this.Password))
                {
                    // еще не подключались в этом сеансе
                    this.isConnected = false;
                }
                else
                {
                    // если строка подключения уже заполнена - попробуем подключиться
                    this.isConnected = this.DBConnOpen();
                }
            }

            if (this.isNotConnected)
            {
                // если подключение не открыто, надо открыть форму "Подключение к БД" и заново собрать строку подключения

                FormLogin dlg1 = new FormLogin(MainWindow.Task.LogFile);

                switch (this.ConnType)
                {
                    case Utilities.ConnType.DBF:
                        dlg1.cbTypeDB.SelectedIndex = 2;
                        break;
                    case Utilities.ConnType.MSSQL:
                        dlg1.cbTypeDB.SelectedIndex = 0;
                        break;
                    case Utilities.ConnType.PGSQL:
                    default:
                        dlg1.cbTypeDB.SelectedIndex = 1;
                        break;
                };
                dlg1.tbServerName.Text = this.ServerName;
                dlg1.tbDatabaseName.Text = this.DBName;

                switch (this.AuthType)
                {
                    case Utilities.AuthType.DATABASE:
                        dlg1.cbAuthentication.SelectedIndex = 1;
                        break;
                    case Utilities.AuthType.WINDOWS:
                    default:
                        dlg1.cbAuthentication.SelectedIndex = 0;
                        break;
                };

                dlg1.tbUsername.Text = this.Username;
                dlg1.tbPassword.Text = this.Password;
                dlg1.cbSavePassword.Checked = this.isSavePassword == true;
                dlg1.tbConnectionAdd.Text = this.connectionAdd;
                dlg1.cbTrustServerCertificate.Checked = this.isTrustServerCertificate == true;

                dlg1.cbConnectionHistory.Items.Clear();
                foreach (var item in MainWindow.ListConnects.OrderBy(x => x.DBConnectionName))
                {
                    dlg1.cbConnectionHistory.Items.Add(item.DBConnectionName);
                }

                //dlg1.connetionString = this.connectionString;

                dlg1.currentDBConnectionName = this.DBConnectionName;
                dlg1.tbTimeout.Value = this.Timeout;

                dlg1.tbPassword.Focus();

                var result = dlg1.ShowDialog();

                if (
                    (result == System.Windows.Forms.DialogResult.OK) ||
                    (result == System.Windows.Forms.DialogResult.Ignore)
                )
                {
                    switch (dlg1.cbTypeDB.SelectedIndex)
                    {
                        case 0:
                            this.ConnType = Utilities.ConnType.MSSQL;
                            break;
                        case 1:
                            this.ConnType = Utilities.ConnType.PGSQL;
                            break;
                        case 2:
                            this.ConnType = Utilities.ConnType.DBF;
                            break;
                        default:
                            this.ConnType = Utilities.ConnType.None;
                            break;
                    }

                    this.ServerName = dlg1.tbServerName.Text;
                    this.DBName = dlg1.tbDatabaseName.Text;

                    switch (dlg1.cbAuthentication.SelectedIndex)
                    {
                        case 1:
                            this.AuthType = Utilities.AuthType.DATABASE;
                            break;
                        case 0:
                        default:
                            this.AuthType = Utilities.AuthType.WINDOWS;
                            break;
                    };

                    this.Username = dlg1.tbUsername.Text;
                    this.Password = dlg1.tbPassword.Text;
                    this.DBConnectionName = dlg1.tbConnectionName.Text;
                    this.Timeout = (int)dlg1.tbTimeout.Value;
                    this.isSavePassword = dlg1.cbSavePassword.Checked == true;
                    this.connectionAdd = dlg1.tbConnectionAdd.Text;
                    this.isTrustServerCertificate = dlg1.cbTrustServerCertificate.Checked == true;

                    // открываем подключение
                    if (result == System.Windows.Forms.DialogResult.OK)
                    {
                        this.isConnected = this.DBConnOpen();
                    }

                    // сохранить новое подключение в список подключений
                    var connect = MainWindow.ListConnects.Find(x => x.DBConnectionName == this.DBConnectionName);
                    if (connect == null)
                    {
                        MainWindow.ListConnects.Add(this);
                    }
                    else
                    {
                        connect.ConnType = this.ConnType;
                        connect.ServerName = this.ServerName;
                        connect.DBName = this.DBName;
                        connect.AuthType = this.AuthType;
                        connect.Username = this.Username;
                        connect.Password = this.Password;
                        connect.Timeout = this.Timeout;
                        connect.isSavePassword = this.isSavePassword == true;
                        connect.connectionAdd = this.connectionAdd;
                        connect.isTrustServerCertificate = this.isTrustServerCertificate == true;
                    }
                    MainWindow.SaveConnects();
                }

                dlg1.Dispose();
            }

            if (this.isConnected && (FieldDB.ListEvn.Count() == 0))
            {
                // заполняем список Evn-таблиц 
                FieldDB.ListEvn = this.GetListEvnChilds("", false, false);
            }

        }

        // -------------------------------------------------------------------------------------------------------
        ///<summary>Выполнить SQL-запрос и вернуть строки в <see cref="DbDataReader" /></summary>
        ///<param name="queryString">Текст SQL-запроса</param>
        ///<param name="isAutoReconnect">=true выполнять попытку reconnect'а при ошибке</param>
        ///<returns>список строк в <see cref="DbDataReader" /></returns>
        public DbDataReader OpenQuery(string queryString, bool isAutoReconnect = true)
        {
            DbDataReader result = null;
            if (string.IsNullOrWhiteSpace(queryString)) return result;

            if (DbConn == null) ReConnect();

            try
            {
                switch (ConnType)
                {
                    case Utilities.ConnType.MSSQL:
                        result = new SqlCommand(queryString, (SqlConnection)DbConn).ExecuteReader();
                        break;
                    case Utilities.ConnType.PGSQL:
                        result = new NpgsqlCommand(queryString, (NpgsqlConnection)DbConn).ExecuteReader();
                        break;
                    case Utilities.ConnType.DBF:
                        result = new OdbcCommand(queryString, (OdbcConnection)DbConn).ExecuteReader();
                        break;
                    case Utilities.ConnType.None:
                    default:
                        result = null;
                        break;
                }

            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                result = null;

                if (
                    (ConnType == Utilities.ConnType.MSSQL || ConnType == Utilities.ConnType.PGSQL) &&
                    (isAutoReconnect == true)
                    )
                {
                    ReConnect();
                    if (this.isConnected)
                    {
                        //try
                        //{
                        switch (ConnType)
                        {
                            case Utilities.ConnType.MSSQL:
                                result = new SqlCommand(queryString, (SqlConnection)DbConn).ExecuteReader();
                                break;
                            case Utilities.ConnType.PGSQL:
                                result = new NpgsqlCommand(queryString, (NpgsqlConnection)DbConn).ExecuteReader();
                                break;
                            case Utilities.ConnType.DBF:
                                result = new OdbcCommand(queryString, (OdbcConnection)DbConn).ExecuteReader();
                                break;
                            case Utilities.ConnType.None:
                            default:
                                result = null;
                                break;
                        }
                        /*}
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.SHOW);
                            result = null;
                        }*/
                    }
                }
                else throw ex;
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        ///<summary>Выполнить SQL-запрос и вернуть значение из поля указанной строки</summary>
        ///<param name="queryString">Текст SQL-запроса</param>
        ///<param name="columnName">имя поля</param>
        ///<param name="numRow">номер строки</param>
        ///<param name="isAutoReconnect">=true выполнять попытку reconnect'а при ошибке</param>
        ///<returns>значение в виде строки</returns>
        public string GetValueFromQuery(string queryString, string columnName, int numRow, bool isAutoReconnect = true)
        {
            string result = "";

            if (string.IsNullOrWhiteSpace(columnName)) columnName = "";
            columnName = columnName.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                var dbtable = this.FillDataTable(queryString, out string Messages);

                if (dbtable != null) //-V3022
                {
                    for (int i = 0; i < dbtable.Columns.Count; i++)
                    {
                        if (dbtable.Columns[i].ColumnName.ToLower() == columnName)
                        {
                            if (numRow <= dbtable.Rows.Count)
                            {
                                var row = dbtable.Rows[numRow - 1];
                                result = row[i].ToString();
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        ///<summary>Выполнить скрипт</summary>
        ///<param name="queryString">Текст SQL-скрипта</param>
        ///<param name="Messages">Текст Messages после запуска скрипта</param>
        ///<returns>кол-во строк или -1</returns>
        public int ExecuteNonQuery(string queryString, out string Messages)
        {
            int result = -1;
            Messages = "";
            if (string.IsNullOrWhiteSpace(queryString)) return result;

            string mes = "";

            try
            {
                if (DbConn == null) ReConnect();

                switch (ConnType)
                {
                    case Utilities.ConnType.MSSQL:
                        {
                            SqlConnection connection = (SqlConnection)DbConn;
                            Microsoft.SqlServer.Management.Smo.Server server = new Microsoft.SqlServer.Management.Smo.Server(new Microsoft.SqlServer.Management.Common.ServerConnection(connection));
                            server.ConnectionContext.InfoMessage += delegate (object obj, SqlInfoMessageEventArgs err)
                            {
                                mes += Environment.NewLine + err.Message;
                            };
                            result = server.ConnectionContext.ExecuteNonQuery(queryString);
                        }
                        break;
                    case Utilities.ConnType.PGSQL:
                        {
                            NpgsqlConnection connection = (NpgsqlConnection)DbConn;
                            connection.Notice += delegate (object sender, NpgsqlNoticeEventArgs err) //-V3080
                            {
                                mes += Environment.NewLine + err.Notice.MessageText;
                            };
                            result = new NpgsqlCommand(queryString, connection).ExecuteNonQuery();
                        }
                        break;
                    case Utilities.ConnType.DBF:
                        {
                            OdbcConnection connection = (OdbcConnection)DbConn;
                            result = new OdbcCommand(queryString, connection).ExecuteNonQuery();
                        }
                        break;
                    case Utilities.ConnType.None:
                    default:
                        result = -1;
                        break;
                }

            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                result = -1;
                mes = "";

                if (ConnType == Utilities.ConnType.MSSQL || ConnType == Utilities.ConnType.PGSQL)
                {
                    ReConnect();
                    if (this.isConnected)
                    {
                        //try
                        //{
                        switch (ConnType)
                        {
                            case Utilities.ConnType.MSSQL:
                                {
                                    SqlConnection connection = (SqlConnection)DbConn;
                                    Microsoft.SqlServer.Management.Smo.Server server = new Microsoft.SqlServer.Management.Smo.Server(new Microsoft.SqlServer.Management.Common.ServerConnection(connection));
                                    server.ConnectionContext.InfoMessage += delegate (object obj, SqlInfoMessageEventArgs err)
                                    {
                                        mes += Environment.NewLine + err.Message;
                                    };
                                    result = server.ConnectionContext.ExecuteNonQuery(queryString);
                                }
                                break;
                            case Utilities.ConnType.PGSQL:
                                {
                                    NpgsqlConnection connection = (NpgsqlConnection)DbConn;
                                    connection.Notice += delegate (object sender, NpgsqlNoticeEventArgs err) //-V3080
                                    {
                                        mes += Environment.NewLine + err.Notice.MessageText;
                                    };
                                    result = new NpgsqlCommand(queryString, connection).ExecuteNonQuery();
                                }
                                break;
                            case Utilities.ConnType.DBF:
                                {
                                    OdbcConnection connection = (OdbcConnection)DbConn;
                                    result = new OdbcCommand(queryString, connection).ExecuteNonQuery();
                                }
                                break;
                            case Utilities.ConnType.None:
                            default:
                                result = -1;
                                break;
                        }
                        /*}
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.SHOW);
                            result = -1;
                        }*/
                    }
                }
                else throw ex;
            }

            Messages = mes;

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        ///<summary>Выполнить SQL-запрос и вернуть строки в <see cref="DataTable" /></summary>
        ///<param name="query">Текст SQL-запроса</param>
        ///<returns>список строк в <see cref="DataTable" /></returns>
        ///<param name="Messages">Текст Messages после запуска скрипта</param>
        public DataTable FillDataTable(string query, out string Messages)
        {
            DataTable result = new DataTable();
            Messages = "";
            if (string.IsNullOrWhiteSpace(query)) return result;
            string mes = "";

            try
            {
                if (DbConn == null) ReConnect();

                switch (ConnType)
                {
                    case Utilities.ConnType.MSSQL:
                        {
                            SqlConnection connection = (SqlConnection)DbConn;
                            Microsoft.SqlServer.Management.Smo.Server server = new Microsoft.SqlServer.Management.Smo.Server(new Microsoft.SqlServer.Management.Common.ServerConnection(connection));
                            server.ConnectionContext.InfoMessage += delegate (object obj, SqlInfoMessageEventArgs err)
                            {
                                mes += Environment.NewLine + err.Message;
                            };

                            var command = new SqlCommand(query, connection);
                            SqlDataAdapter sda = new SqlDataAdapter(command);
                            sda.Fill(result);
                        }
                        break;
                    case Utilities.ConnType.PGSQL:
                        {
                            NpgsqlConnection connection = (NpgsqlConnection)DbConn;
                            connection.Notice += delegate (object sender, NpgsqlNoticeEventArgs err) //-V3080
                            {
                                mes += Environment.NewLine + err.Notice.MessageText;
                            };

                            var command = new NpgsqlCommand(query, connection);
                            NpgsqlDataAdapter sda = new NpgsqlDataAdapter(command);
                            sda.Fill(result);
                        }
                        break;
                    case Utilities.ConnType.DBF:
                        {
                            OdbcConnection connection = (OdbcConnection)DbConn;
                            var command = new OdbcCommand(query, connection);
                            OdbcDataAdapter sda = new OdbcDataAdapter(command);
                            sda.Fill(result);
                        }
                        break;
                    case Utilities.ConnType.None:
                    default:
                        result = null;
                        break;
                }

            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                result = null;

                if (ConnType == Utilities.ConnType.MSSQL || ConnType == Utilities.ConnType.PGSQL)
                {
                    ReConnect();
                    if (this.isConnected)
                    {
                        //try
                        //{
                        switch (ConnType)
                        {
                            case Utilities.ConnType.MSSQL:
                                {
                                    SqlConnection connection = (SqlConnection)DbConn;
                                    Microsoft.SqlServer.Management.Smo.Server server = new Microsoft.SqlServer.Management.Smo.Server(new Microsoft.SqlServer.Management.Common.ServerConnection(connection));
                                    server.ConnectionContext.InfoMessage += delegate (object obj, SqlInfoMessageEventArgs err)
                                    {
                                        mes += Environment.NewLine + err.Message;
                                    };

                                    result = new DataTable();
                                    var command = new SqlCommand(query, connection);
                                    SqlDataAdapter sda = new SqlDataAdapter(command);
                                    sda.Fill(result);
                                }
                                break;
                            case Utilities.ConnType.PGSQL:
                                {
                                    NpgsqlConnection connection = (NpgsqlConnection)DbConn;
                                    connection.Notice += delegate (object sender, NpgsqlNoticeEventArgs err) //-V3080
                                    {
                                        mes += Environment.NewLine + err.Notice.MessageText;
                                    };

                                    result = new DataTable();
                                    var command = new NpgsqlCommand(query, connection);
                                    NpgsqlDataAdapter sda = new NpgsqlDataAdapter(command);
                                    sda.Fill(result);
                                }
                                break;
                            case Utilities.ConnType.DBF:
                                {
                                    OdbcConnection connection = (OdbcConnection)DbConn;
                                    var command = new OdbcCommand(query, connection);
                                    OdbcDataAdapter sda = new OdbcDataAdapter(command);
                                    sda.Fill(result);
                                }
                                break;
                            case Utilities.ConnType.None:
                            default:
                                result = null;
                                break;
                        }
                        /*}
                        catch (Exception ex)
                        {
                            App.AddLog("", ex, App.ShowMessageMode.SHOW);
                            result = null;
                        }*/
                    }
                }
                else throw ex;
            }

            if (result == null) result = new DataTable();
            Messages = mes;

            return result;
        }

        /// <summary>
        /// Копирование подключения
        /// </summary>
        /// <returns></returns>
        public ConnectDB Copy()
        {
            return (ConnectDB)this.MemberwiseClone();
        }

        /// <summary>Список полей в PK по имени таблицы</summary>
        /// <param name="fulltablename">Имя таблицы</param>
        public PKInfo GetTablePK(string fulltablename)
        {
            string queryString = "";
            PKInfo result = new PKInfo();

            fulltablename = Utilities.Databases.GetFullTableName(fulltablename).Replace("\"",String.Empty);
            string schemaname = Utilities.Databases.GetSchemaName(fulltablename).Replace("\"", String.Empty);
            string tablename = Utilities.Databases.GetTableName(fulltablename).Replace("\"", String.Empty);

            // корректная обработка символа подчеркивания в like\ilike
            if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                schemaname = schemaname.Replace("_", "\\_");
                tablename = tablename.Replace("_", "\\_");
            }

            if (this.ConnType == Utilities.ConnType.MSSQL)
                queryString = @"
select 
	c.name as PKColumn,
	upper(t.name) as PKType,
	CASE WHEN c.is_identity = 1 THEN 2 ELSE 1 END as is_identity,
    schema_name(schema_id(N'" + schemaname + @"')) + '.' + object_name(object_id(N'" + fulltablename + @"', N'U')) as tablename
from sys.indexes i with(nolock)
inner join sys.index_columns ic with(nolock) on i.object_id = ic.object_id 
	and i.index_id = ic.index_id 
inner join sys.columns c with(nolock) on i.object_id = c.object_id 
	and ic.column_id = c.column_id 
inner join sys.types t with(nolock) on t.user_type_id = c.user_type_id
where i.object_id = OBJECT_ID(N'" + fulltablename + @"', 'U') 
and i.is_primary_key = 1 
order by ic.key_ordinal; 
";
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"
with pkcons as (
	SELECT
		o.conname AS constraint_name,
		s.nspname AS table_schema,
		m.relname AS table_name,
		a.attname AS column_name,
		array_position(o.conkey, a.attnum) AS ordinal_position,
		upper(case 
			when t.typname = 'character' or t.typname = '""char""' or t.typname = 'bpchar' then 'char'
			when t.typname = 'character varying' then 'varchar'
			when t.typname = 'int8' then 'bigint'
			when t.typname = 'serial8' then 'bigserial'
			when t.typname = 'bit varying' then 'varbit'
			when t.typname = 'float8' then 'double precision'
			when t.typname = 'int4' then 'int'
			when t.typname = 'float4' then 'real'
			when t.typname = 'int2' then 'smallint'
			when t.typname = 'serial4' then 'serial'
			else t.typname
		end) as column_type,
		case when a.attidentity != '' then 2 else 1 end is_identity
	FROM pg_constraint o 
	inner JOIN pg_class m ON m.oid = o.conrelid AND m.relkind in ('r','f','p')
	inner join pg_namespace s ON s.oid = m.relnamespace
	inner join pg_attribute a ON a.attrelid = m.oid 
		AND a.attnum = ANY(o.conkey) 
		AND a.attisdropped = false -- не удалено
	inner join pg_type t ON t.oid = a.atttypid
	WHERE o.contype = 'p'
)
SELECT
	pkcons.column_name as PKColumn,
	upper(pkcons.column_type) as PKType,
	pkcons.is_identity as is_identity,
    pkcons.table_schema || '.' || pkcons.table_name as tablename
from pkcons
where pkcons.table_schema ilike '" + schemaname + @"' 
and pkcons.table_name ilike '" + tablename + @"'
ORDER BY pkcons.ordinal_position;";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    int _cnt = 0;

                    while (reader.Read())
                    {
                        _cnt++;

                        PKField field = new PKField();

                        field.FieldName = reader[0].ToString();
                        field.FieldType = reader[1].ToString();
                        field.IsIdentity = reader[2].ToString() == "2";
                        field.PKOrder = _cnt;
                        result.TableName = reader[3].ToString();

                        result.Fields.Add(field);
                    }
                }
            }

            return result;
        }

        /// <summary>Список полей (через запятую) по имени таблицы</summary>
        /// <param name="fulltablename">Имя таблицы</param>
        public string GetListFields(string fulltablename)
        {
            string queryString = "";
            string result = "";

            fulltablename = Utilities.Databases.GetFullTableName(fulltablename).Replace("\"", "");
            string schemaname = Utilities.Databases.GetSchemaName(fulltablename);
            string tablename = Utilities.Databases.GetTableName(fulltablename);

            // корректная обработка символа подчеркивания в like\ilike
            if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                schemaname = schemaname.Replace("_", "\\_");
                tablename = tablename.Replace("_", "\\_");
            }

            if (this.ConnType == Utilities.ConnType.MSSQL)
                queryString = @"
DECLARE @F varchar(max); 

SELECT @F = ISNULL(@F + ',', '') + c.name 
FROM sys.columns c WITH(nolock)
WHERE c.object_id = object_id(N'" + fulltablename + @"', 'U')
ORDER BY c.column_id; 

SELECT @F AS ColumnList;
";
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"
SELECT array_to_string(array( 
	SELECT attr.attname as column_name
	FROM pg_catalog.pg_attribute as attr
	WHERE EXISTS (
		SELECT 1
		FROM pg_catalog.pg_class as cls
		INNER JOIN pg_catalog.pg_namespace as ns on ns.oid = cls.relnamespace
		WHERE ns.nspname ilike '" + schemaname + @"'
		AND cls.relname ilike '" + tablename + @"'
		AND cls.relkind in ('r','f','p')
		AND cls.oid = attr.attrelid 
		LIMIT 1
	) 
	AND attr.attisdropped = false -- не удалено
	AND attr.attnum > 0 -- без служебных полей
	ORDER BY attr.attnum
),',');
";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        result = reader[0].ToString();
                        break; //-V3020
                    }
                }
            }

            return result;
        }


        /// <summary>Получить из БД описание таблицы</summary>
        /// <param name="tablename">Имя таблицы</param>
        public string GetTableDecription(string tablename)
        {
            string queryString = "";
            string result = "";

            tablename = Utilities.Databases.GetFullTableName(tablename).Replace("\"", String.Empty);
            string table = Utilities.Databases.GetTableName(tablename).Replace("\"", String.Empty);
            string schema = Utilities.Databases.GetSchemaName(tablename).Replace("\"", String.Empty);

            // корректная обработка символа подчеркивания в like\ilike
            if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                schema = schema.Replace("_", "\\_");
                table = table.Replace("_", "\\_");
            }

            if (this.ConnType == Utilities.ConnType.MSSQL)
                queryString = @"
Select distinct ep.value as table_descr 
from sys.tables t with(nolock)
left join sys.extended_properties ep with(nolock) on ep.major_id = t.object_id 
	and ep.minor_id = 0 
where t.object_id = object_id(N'" + tablename + @"', 'U') 
and ep.name <> 'SWAN_RegionalTable';";
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"
SELECT distinct pg_catalog.obj_description(t.oid, 'pg_class') AS table_descr
FROM pg_catalog.pg_class t
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
WHERE t.relkind in ('r','f','p')
and t.relname ilike '" + table + @"'
and n.nspname ilike '" + schema + @"';";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        result = reader[0].ToString();
                        break; //-V3020
                    }
                }
            }

            return result;
        }

        /// <summary>Заполнить в TableInfo данные из БД о таблице</summary>
        /// <param name="table">Ссылка на экземпляр TableInfo, у которого заполнены SchemaName, TableName и TargetDBType </param>
        public bool FillTableInfo(TableInfo table)
        {
            bool isfound = false;

            string queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
                queryString = @"
DECLARE @EvnClass TABLE (EvnClass_id BIGINT, EvnClass_pid BIGINT, evnclass_sysnick VARCHAR(max))

IF OBJECT_ID('dbo.EvnClass', 'U') IS NOT NULL
BEGIN
	insert into @EvnClass (
		EvnClass_id, 
		EvnClass_pid, 
		evnclass_sysnick
	)
	select 
		l.EvnClass_id, 
		l.EvnClass_pid, 
		l.evnclass_sysnick
	from dbo.EvnClass l with (nolock)
	where not exists (
		select 1 
		from @EvnClass t 
		where t.EvnClass_id = l.EvnClass_id
	)
END;

Select 
	schema_name(t.schema_id) as schema_name, 
	t.name as table_name, 
	ep.value as table_descr, 
	pk.name as pk_name,
	parent.evnclass_sysnick as parentevntable_name,
	'' as foreign_table,
	'' as foreign_server,
	'' as foreign_options,
	coalesce(descr.hasRegionDescr,1) as hasRegionDescr,
	1 as hasSequence,
	1 as hasInherit
from sys.tables t with (nolock)
left join sys.extended_properties ep with (nolock) on ep.major_id = t.object_id 
	and ep.minor_id = 0 
left join sys.indexes pk with (nolock) on pk.object_id = t.object_id 
	and pk.is_primary_key = 1 
outer apply (
	select top(1) ec_parent.evnclass_sysnick
	from @EvnClass ec_parent
	inner join @EvnClass ec_child on ec_child.evnclass_pid = ec_parent.evnclass_id 
		and ec_child.evnclass_sysnick = t.name
) parent
outer apply (
	Select top(1) 2 as hasRegionDescr
	from sys.tables tt with (nolock)
	inner join sys.extended_properties ep with (nolock) on ep.major_id = tt.object_id 
		and ep.minor_id = 0 
		and ep.name = 'SWAN_RegionalTable'
	where tt.object_id = t.object_id
) descr
where t.object_id = object_id(N'" + table.FullTableNameToScript + "', 'U');";
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"
CREATE TEMP TABLE IF NOT EXISTS tmp_evnclass (
	evnclass_id bigint, 
	evnclass_pid bigint, 
	evnclass_sysnick varchar
);

DO $$ BEGIN
IF to_regclass('dbo.evnclass') IS NOT NULL THEN
	INSERT INTO tmp_evnclass (
		evnclass_id, 
		evnclass_pid, 
		evnclass_sysnick
	)
	SELECT 
		l.evnclass_id, 
		l.evnclass_pid, 
		l.evnclass_sysnick
	FROM dbo.evnclass l
	where not exists (
		select 1 
		from tmp_evnclass t 
		where t.EvnClass_id = l.EvnClass_id
	);
END IF;
END; $$;

SELECT
	n.nspname as schema_name,
	t.relname as table_name,
	pg_catalog.obj_description(t.oid, 'pg_class') AS table_descr,
	pk.conname as pk_name,
	parent.evnclass_sysnick as parentevntable_name,
	case when t.relkind = 'f' then 'FOREIGN' else '' end as foreign_table,
	fs.srvname as foreign_server,
	ft.ftoptions as foreign_options,
	1 as hasRegionDescr,
	coalesce(def.hasSequence,1) as hasSequence,
	coalesce(inh.hasInherit,1) as hasInherit
FROM pg_catalog.pg_class t
LEFT JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
left join pg_catalog.pg_constraint pk on pk.conrelid = t.oid 
	and pk.contype = 'p'
left join lateral (
	select 2 as hasInherit 
	from pg_catalog.pg_inherits inh 
	where inh.inhrelid = t.oid 
	limit 1
) inh on true
left join lateral (
	select ec_parent.evnclass_sysnick
	from tmp_evnclass ec_parent
	inner join tmp_evnclass ec_child on ec_child.evnclass_pid = ec_parent.evnclass_id 
		and ec_child.evnclass_sysnick ilike t.relname
	limit 1
) parent on true
left join lateral (
	select 
		ft.ftserver, 
		array_to_string(ft.ftoptions,',','') as ftoptions 
	from pg_foreign_table ft 
	where ft.ftrelid  = t.oid 
	limit 1
) ft on true
left join lateral (
	select fs.srvname 
	from pg_foreign_server fs 
	where fs.oid  = ft.ftserver 
	limit 1
) fs on true
left join lateral (
	select 2 as hasSequence
	from (    	
		select 
			adnum, 
			pg_get_expr(adbin, adrelid) as adsrc
		from pg_attrdef def
		where def.adrelid = t.oid
	) def
	where strpos(lower(def.adsrc), 'nextval') > 0
	and replace(replace(replace(def.adsrc,' ',''),'nextval(''',''),'''::regclass)','') != ''
	order by def.adnum
	limit 1
) def on true  
where t.relkind in ('r','f','p')
and t.relname ilike '" + table.TableNameToSeekForLike + @"'
and n.nspname ilike '" + table.SchemaNameToSeekForLike + @"';";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        table.SchemaName = reader[0].ToString();
                        table.TableName = reader[1].ToString();
                        table.TableDesc = reader[2].ToString();
                        table.PKName = reader[3].ToString();
                        table.ParentEvnTable = reader[4].ToString();
                        table.ForeignWord = reader[5].ToString();
                        table.ForeignServer = reader[6].ToString();

                        // отформатировать _foreign_options
                        string _foreign_options = reader[7].ToString()
                            .Trim()
                            .Replace("\r\r", "\r");

                        if (_foreign_options.ToLower().StartsWith("query="))
                        {
                            var arr_key = _foreign_options.Split(new char[] { '=' }, 2);

                            if (arr_key.Length > 1)
                            {
                                arr_key[1] = arr_key[1].Replace("'", "''");

                                _foreign_options = arr_key[0] + " '" + arr_key[1] + "'";
                            }
                        }
                        else
                        {
                            var arr = _foreign_options.Split(',');

                            if (arr.Length > 0) //-V3022
                            {
                                _foreign_options = "";

                                for (int i = 0; i < arr.Length; i++)
                                {
                                    string item = arr[i].Trim();
                                    var arr_key = item.Split(new char[] { '=' }, 2);

                                    if (arr_key.Length > 1)
                                    {
                                        arr[i] = arr_key[0] + " '" + arr_key[1] + "'";
                                    }

                                    if (!string.IsNullOrWhiteSpace(arr[i]))
                                    {
                                        if (!string.IsNullOrWhiteSpace(_foreign_options))
                                        {
                                            _foreign_options += ", ";
                                        }
                                        _foreign_options += arr[i];
                                    }
                                }
                            }
                        }

                        table.ForeignOptions = _foreign_options;
                        table.HasRegionDescr = reader[8].ToString() == "2";
                        table.HasSequence = reader[9].ToString() == "2";
                        table.HasInherit = reader[10].ToString() == "2";

                        if ((this.ConnType == Utilities.ConnType.PGSQL) && (table.SchemaNameReady == "EMD"))
                        {
                            table.SchemaName = "\"" + table.SchemaNameReady + "\"";
                            table.TableName = "\"" + table.TableNameReady + "\"";
                            table.PKName = "\"" + table.PKNameReady + "\"";
                        }

                        isfound = true;
                        break; //-V3020
                    }
                }
            }

            return isfound;
        }

        /// <summary>
        /// Найтие в БД список похожих таблиц
        /// </summary>
        /// <param name="tablename">имя таблицы</param>
        /// <returns></returns>
        public List<string> FillAlternateTable(string tablename)
        {
            List<string> result = new List<string>();

            string queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
                queryString = @"
Select s.name + '.' + t.name as table_name
from sys.tables t with (nolock)
JOIN sys.schemas s with (nolock) ON s.schema_id = t.schema_id
where s.name NOT IN ('information_schema', 'sys', 'tmp', 'TABLE tmp', 'raw', 'symdict', 'SWN\savage', 'audit', 'public', 'liquibase', 'diff', 'box')
and s.name + '.' + t.name like '%" + tablename + @"%'
order by s.name + '.' + t.name";
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"
SELECT n.nspname || '.' || t.relname as table_name
FROM pg_catalog.pg_class t
LEFT JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('r','f','p')
AND n.nspname NOT IN ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname || '.' || t.relname ilike '%" + tablename + @"%'
order by n.nspname || '.' || t.relname;";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        if (!result.Contains(reader[0].ToString()))
                        {
                            result.Add(reader[0].ToString());
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>Заполнить в TableInfo данные из БД о списке полей ListField</summary>
        /// <param name="table">Ссылка на экземпляр TableInfo, у которого заполнены SchemaName, TableName и TargetDBType </param>
        public bool FillListField(TableInfo table)
        {
            bool isfound = false;

            string queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
                queryString = @"
Select
    c.column_id as field_order,
    c.name as field_name,
    ep.value as field_desc,
    Upper(typ.name) as field_type,
    case 
        when typ.name in ('nvarchar', 'varchar', 'varbinary') and c.max_length = -1 then 'max'
        when typ.name in ('nvarchar') and c.max_length <> -1 then cast(c.max_length/2 as varchar)
        when typ.name in ('nchar') then cast(c.max_length/2 as varchar)
        when typ.name in ('varchar', 'varbinary') and c.max_length <> -1 then cast(c.max_length as varchar)
        when typ.name in ('binary', 'char') then cast(c.max_length as varchar)
        when typ.name in ('decimal', 'numeric') then cast(c.precision as varchar)
        when typ.name in ('time') and c.scale = 7 then ''
        when typ.name in ('datetime2', 'datetimeoffset', 'time') then cast(c.scale as varchar)
        else ''
    end as field_size,
    case 
        when typ.name in ('decimal', 'numeric') then cast(c.scale as varchar)
        else ''
    end as field_dec,
    case when c.is_nullable = 1 then 'false' else 'true' end as IsNotNull,
    case when c.is_identity = 1 then 'true' else 'false' end as IsIdentity,
    case when pk.object_id is not null then 'true' else 'false' end as IsPK,
    def.definition as field_default,
    fk.name as fk_name,
    schema_name(rt.schema_id) + '.' + rt.name as fk_table,
    rc.name as fk_field,
    '' as foreign_column,
    '' as field_check,
    pk.key_ordinal as PKOrder,
    fkc.constraint_column_id as FKOrder,
    'false' as IsInherit,
    '' as InheritParentTable
from sys.tables t with (nolock)
inner join sys.columns c with (nolock) on c.object_id = t.object_id
inner join sys.types typ with (nolock) on typ.system_type_id = c.system_type_id and typ.user_type_id = c.user_type_id
left join sys.extended_properties ep with (nolock) on ep.major_id = t.object_id and ep.minor_id = c.column_id
left join sys.default_constraints def with (nolock) on def.parent_object_id = t.object_id and def.parent_column_id = c.column_id
outer apply(
    select 
        i.object_id, 
        ic.column_id,
        ic.key_ordinal
    from sys.indexes i with (nolock)
    inner join sys.index_columns ic with (nolock) on i.object_id = ic.object_id and i.index_id = ic.index_id
    where i.is_primary_key = 1
    and i.object_id = c.object_id 
    and ic.column_id = c.column_id
) pk
outer apply (
	select top(1) *
	from sys.foreign_key_columns fkc with (nolock) 
	where fkc.parent_object_id = c.object_id 
	and fkc.parent_column_id = c.column_id 
	order by case 
		when c.name = 'server_id' then 1
		else 0
	end
) fkc	
left join sys.foreign_keys fk with (nolock) on fk.object_id = fkc.constraint_object_id
left join sys.tables rt with (nolock) on rt.object_id = fkc.referenced_object_id
left join sys.columns rc with (nolock) on rc.object_id = fkc.referenced_object_id and rc.column_id = fkc.referenced_column_id 
where t.object_id = object_id(N'" + table.FullTableNameToScript + @"', 'U')
and c.name not like '%[_]rowversion'
order by c.column_id;";
            else if (this.ConnType == Utilities.ConnType.PGSQL)
                queryString = @"
-- таблица
WITH tab AS MATERIALIZED (
	select m.oid
	from pg_class m
	where m.oid = to_regclass(quote_ident('" + table.SchemaNameToSeek + @"') || '.' || quote_ident('" + table.TableNameToSeek + @"'))
	and m.relkind in ('r','f','p')
	limit 1
),
-- цепочка наследования
inherited AS MATERIALIZED (
	WITH RECURSIVE r AS (
		with all_inherits as (
			select distinct inhrelid, inhparent from pg_inherits
			union all
			select distinct inh.inhparent as inhrelid, null::oid as inhparent 
			from pg_inherits inh 
			where not exists (select 1 from pg_inherits l where l.inhrelid = inh.inhparent)
		)

		select 
			inh.inhrelid, 
			inh.inhparent,
			s.nspname || '.' || t.relname as tablename
		from all_inherits inh
		inner join pg_class t on t.oid = inh.inhrelid
		INNER JOIN pg_namespace s ON s.oid = t.relnamespace
		where inh.inhrelid = ANY(select oid from tab)

		union

		select 
			inh.inhrelid, 
			inh.inhparent,
			s.nspname || '.' || t.relname as tablename
		from all_inherits inh
		inner join pg_class t on t.oid = inh.inhrelid
		INNER JOIN pg_namespace s ON s.oid = t.relnamespace
		inner join r on inh.inhrelid = r.inhparent 
	)
	select * from r
)
SELECT
	c.attnum as field_order,
	c.attname as field_name,
	col_description(t.oid, c.attnum) as field_desc,
	case 
		when typ.typename in ('BPCHAR','CHARACTER') then 'CHAR'
		when typ.typename in ('CHARACTER VARYING') then 'VARCHAR'
		else typ.typename
	end as field_type,
	case 
		when typ.typename in ('TIMESTAMP WITH TIME ZONE','TIMESTAMP WITHOUT TIME ZONE','TIME WITHOUT TIME ZONE','TIME WITH TIME ZONE','TIME','TIMESTAMP', 'TIMESTAMPTZ','INTERVAL') then ''
		else typ.precision 
	end as field_size,
	typ.scale as field_dec,
	case when c.attnotnull then 'true' else 'false' end as IsNotNull,
	case when c.attidentity = '' or c.attidentity is null then 'false' else 'true' end as IsIdentity,
	case when pk.contype = 'p' then 'true' else 'false' end as IsPK,
	def.definition as field_default,
	coalesce(fk.constraint_name, chk.constraint_name) as fk_name,
	fk.table_schema || '.' || fk.table_name as fk_table,
	fk.column_name as fk_field,
	array_to_string(c.attfdwoptions,',','') as foreign_column,
	chk.field_check,
	pk.PKOrder,
	fk.FKOrder,
	case when c.attinhcount = 0 then 'false' else 'true' end as IsInherit,
	InheritParentTable.tablename as InheritParentTable
FROM tab t
INNER JOIN pg_catalog.pg_attribute c ON c.attrelid = t.oid 
	AND c.attnum > 0 -- без служебных полей
	AND c.attisdropped = false -- не удалено
	AND c.attname not ilike '%\_rowversion'
LEFT JOIN LATERAL (
	select 
		typeinfo,
		regexp_replace(typeinfo, '\([^)]*\)', '', 'g') as typename,
		split_part(CASE 
			WHEN typeinfo ~ '\(.*\)' THEN regexp_replace(typeinfo, '.*\((.*)\).*', '\1') 
			ELSE ''
		END, ',', 1) as precision,
		split_part(CASE 
			WHEN typeinfo ~ '\(.*\)' THEN regexp_replace(typeinfo, '.*\((.*)\).*', '\1') 
			ELSE ''
		END, ',', 2) as scale
	from (
		select 
			UPPER(FORMAT_TYPE(
				COALESCE(NULLIF(typ.typbasetype,0),typ.oid), 
				COALESCE(NULLIF(typ.typtypmod,-1),c.atttypmod)
			)::VARCHAR) as typeinfo
		from pg_type typ 
		where typ.oid = c.atttypid
		limit 1
	) tt
) typ on true
LEFT JOIN LATERAL (
	select pg_get_expr(d.adbin, d.adrelid) as definition
	from pg_attrdef d
	where d.adrelid = c.attrelid
	and d.adnum = c.attnum
) def on true
LEFT JOIN LATERAL (
	SELECT r.tablename
	FROM inherited r
	JOIN pg_attribute r_a ON r_a.attrelid = r.inhrelid
	WHERE r_a.attname = c.attname
	AND r_a.attnum > 0 -- без служебных полей
	AND r_a.attisdropped = false -- не удалено
	AND r_a.attinhcount = 0 -- ищем среди не унаследованных полей
	AND c.attinhcount > 0 -- имя родительской таблицы ищем только для унаследованного поля
	limit 1
) InheritParentTable on true
-- primary key
LEFT JOIN LATERAL (
	SELECT 
		o.contype,
		array_position(o.conkey, a.attnum) as PKOrder
	FROM pg_constraint o 
	inner join pg_attribute a ON a.attrelid = t.oid
		AND a.attnum = ANY(o.conkey) 
		AND a.attisdropped = false -- не удалено
		AND a.attname = c.attname
	WHERE o.contype = 'p'
	AND o.conrelid = t.oid
	limit 1
) pk on true
-- foreign key constraint
LEFT JOIN LATERAL (
	select 
		o.conname as constraint_name, 
		fs.nspname as table_schema, 
		f.relname as table_name, 
		fc.FKField as column_name,
		fc.FKOrder
	FROM pg_constraint o
	INNER JOIN pg_class f ON f.oid = o.confrelid
	INNER JOIN pg_namespace fs ON fs.oid = f.relnamespace
	-- поля primary key в таблице внешнего ключа
	INNER JOIN LATERAL (
		SELECT
			array_position(o.confkey, a.attnum) as FKOrder,
			a.attname as FKField
		FROM pg_attribute a 
		WHERE a.attrelid = o.confrelid 
		AND a.attnum = ANY(o.confkey)
		AND a.attisdropped = false -- не удалено
	) fc on fc.FKOrder = array_position(o.conkey, c.attnum)
	WHERE o.conrelid = t.oid
	AND o.contype = 'f'
	AND c.attnum = ANY(o.conkey)
	limit 1
) fk on true
-- check constraint
LEFT JOIN LATERAL (
	select 
		o.conname as constraint_name, 
		REPLACE(pg_get_constraintdef(o.oid),'CHECK ','') as field_check
	from pg_constraint o
	where o.conrelid = t.oid
	AND o.contype = 'c' 
	AND c.attnum = ANY(o.conkey)
	and fk.constraint_name is null -- нет fk
	limit 1
) chk on true
ORDER BY c.attnum
";

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    table.ListField.Clear();

                    while (reader.Read())
                    {

                        string _foreign_column = reader["foreign_column"].ToString()
                                .Replace(" ", "")
                                .Replace("''", "'")
                                .Replace("=", " '")
                                .Replace(",", "', ")
                                ;

                        if (!string.IsNullOrWhiteSpace(_foreign_column))
                        {
                            _foreign_column += "'";
                        }

                        string _def = reader["field_default"].ToString();
                        /*
                        if (
                               _def.StartsWith("((") &&
                               _def.EndsWith("))")
                               )
                        {
                            _def = _def.Substring(2, _def.Length - 4);
                        }
                        if (
                               _def.StartsWith("(") &&
                               _def.EndsWith(")")
                               )
                        {
                            _def = _def.Substring(1, _def.Length - 2);
                        }*/

                        table.AddField(
                            reader["field_order"].ToString(),
                            reader["field_name"].ToString(),
                            reader["field_type"].ToString(),
                            reader["field_size"].ToString(),
                            reader["field_dec"].ToString(),
                            reader["field_desc"].ToString(),
                            reader["IsNotNull"].ToString(),
                            reader["IsIdentity"].ToString(),
                            reader["IsPK"].ToString(),
                            reader["PKOrder"].ToString(),
                            _def,
                            reader["fk_name"].ToString(),
                            reader["fk_table"].ToString(),
                            reader["fk_field"].ToString(),
                            reader["FKOrder"].ToString(),
                            _foreign_column,
                            reader["field_check"].ToString(),
                            reader["IsInherit"].ToString(),
                            reader["InheritParentTable"].ToString()
                            );

                        isfound = true;
                    }

                }
            }

            return isfound;
        }


        /// <summary>Заполнить в TableDB данные из БД о списке индексов ListIndex</summary>
        /// <param name="table">Ссылка на экземпляр TableDB, у которого заполнены SchemaName, TableName и TargetDBType</param>
        /// <param name="_indexname">Имя индекса (если нужен только один индекс)</param>
        public bool FillListIndex(TableDB table, string _indexname)
        {
            bool isfound = false;

            string queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                queryString = @"
DECLARE
	@idx_indexid BIGINT = NULL,";

                if (!string.IsNullOrWhiteSpace(_indexname))
                {
                    queryString += Environment.NewLine + "@idx_name VARCHAR(max) = '" + _indexname + "',";
                }
                else
                {
                    queryString += Environment.NewLine + "@idx_name VARCHAR(max) = NULL,";
                }
                queryString += Environment.NewLine + @"
    @idx_isunique VARCHAR(10) = NULL,
    @idx_where VARCHAR(max) = NULL,
    @table_id BIGINT = object_id(N'" + table.TableEdit.FullTableNameToScript + @"', 'U'),
    @p VARCHAR(max) = NULL,
    @i VARCHAR(max) = NULL

DECLARE @existsindex TABLE (
	IndexName VARCHAR(max), 
	IsUnique VARCHAR(10),
	IndexPredicat VARCHAR(max), 
	IndexInclude VARCHAR(max), 
	IndexWhere VARCHAR(max) 
	)

DECLARE cursp CURSOR LOCAL FOR 
    SELECT 
        Idx.index_id, 
        Idx.name, 
        Idx.filter_definition, 
        case when idx.is_unique=1 then 'true' else 'false' end
    FROM sys.indexes Idx (NOLOCK)
    inner join sys.TABLES t (NOLOCK) on t.object_id = Idx.object_id
    inner join sys.schemas s (NOLOCK) on s.schema_id = t.schema_id
    INNER JOIN sys.filegroups FG (NOLOCK) ON FG.data_space_id = Idx.data_space_id
    WHERE Idx.object_id = @table_id 
    AND Idx.type <> 1";

                if (!string.IsNullOrWhiteSpace(_indexname))
                {
                    queryString += Environment.NewLine + "AND Idx.name = @idx_name";
                }

                queryString += Environment.NewLine + @"
			ORDER BY Idx.object_id, Idx.index_id
OPEN cursp
FETCH NEXT FROM cursp
INTO @idx_indexid, @idx_name, @idx_where, @idx_isunique
WHILE @@FETCH_STATUS = 0
	BEGIN 
		set @p = null
		set @i = null

		SELECT  @p = ISNULL(@p + ', ','') + QUOTENAME(SysCol.name COLLATE Cyrillic_General_CI_AS ) 
			+ CASE WHEN IdxCol.is_descending_key = 1 THEN ' DESC' ELSE '' end
		FROM sys.index_columns IdxCol (NOLOCK)
		INNER JOIN sys.columns SysCol(NOLOCK) ON SysCol.object_id = IdxCol.object_id AND SysCol.column_id = IdxCol.column_id
		WHERE IdxCol.object_id = @table_id AND IdxCol.index_id = @idx_indexid  AND IdxCol.is_included_column = 0
		ORDER BY IdxCol.index_column_id

		SELECT  @i = ISNULL(@i + ', ','') + QUOTENAME(SysCol.name COLLATE Cyrillic_General_CI_AS )
			+ CASE WHEN IdxCol.is_descending_key = 1 THEN ' DESC' ELSE '' end
		FROM sys.index_columns IdxCol (NOLOCK)
		INNER JOIN sys.columns SysCol(NOLOCK) ON SysCol.object_id = IdxCol.object_id AND SysCol.column_id = IdxCol.column_id
		WHERE IdxCol.object_id = @table_id AND IdxCol.index_id = @idx_indexid  AND IdxCol.is_included_column = 1
		ORDER BY IdxCol.index_column_id
  
		INSERT INTO @existsindex (IndexName, IndexPredicat, IndexInclude, IndexWhere, IsUnique )
		VALUES (@idx_name, REPLACE(REPLACE(@p,']',''),'[',''), REPLACE(REPLACE(ISNULL(@i,''),']',''),'[',''), REPLACE(REPLACE(ISNULL(@idx_where,''),']',''),'[',''), @idx_isunique )

		FETCH NEXT FROM cursp
		INTO @idx_indexid, @idx_name, @idx_where, @idx_isunique
	END

CLOSE cursp
DEALLOCATE cursp

SELECT IndexName, IsUnique, IndexPredicat, IndexInclude, IndexWhere, '' from @existsindex";

            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"
CREATE OR REPLACE FUNCTION pg_temp.fn_getindexlist(
	p_schemaname character varying, 
	p_tablename character varying
)
 RETURNS TABLE(
 	indexrelid bigint, 
 	schemaname character varying, 
 	tablename character varying, 
 	indexname character varying, 
 	indexcreate character varying, 
 	countcol integer, 
 	countpredicat integer, 
 	countinclude integer, 
 	isprimary boolean, 
 	isunique boolean, 
 	isindisvalid boolean,
 	predicat character varying, 
 	include character varying, 
 	inwhere character varying,
	indnullsnotdistinct boolean
 )
 LANGUAGE plpgsql
AS $function$
BEGIN
    p_schemaname = trim(replace(replace(lower(p_schemaname),' ',''),'""',''));
    p_tablename = trim(replace(replace(lower(p_tablename),' ',''),'""',''));

    RETURN QUERY
    WITH tbl_indexlist AS (
        SELECT
            idx.indexrelid AS indexrelid,
            schema_class.nspname AS schemaname,
            table_class.relname AS tablename,
            index_class.relname AS indexname,
            pg_get_indexdef(idx.indexrelid) AS indexcreate,
            idx.indnatts AS countcol,
            idx.indnkeyatts AS countpredicat,
            idx.indnatts - idx.indnkeyatts AS countinclude,
            --PROMEDWEB-221082 split_part(lower(pg_get_indexdef(idx.indexrelid)), ' where ', 2) AS inwhere,
            pg_get_expr(idx.indpred, idx.indrelid, false) AS inwhere,
            idx.indisprimary AS isprimary,
            idx.indisunique AS isunique,
            case when (idx.indisvalid and idx.indisready) then false else true end isindisvalid,
            idx.indoption,
            idx.indclass,
            idx.indnullsnotdistinct
        FROM
            pg_index idx
            INNER JOIN pg_class index_class ON index_class.oid = idx.indexrelid
            INNER JOIN pg_class table_class ON table_class.oid = idx.indrelid
            INNER JOIN pg_namespace schema_class ON schema_class.oid = table_class.relnamespace
        WHERE
            1 = 1
            AND schema_class.nspname NOT LIKE 'pg\_%'
            AND schema_class.nspname <> 'audit'
            AND (lower(table_class.relname) = lower(p_tablename) OR p_tablename IS NULL)
            AND (lower(schema_class.nspname) = lower(p_schemaname) OR p_schemaname IS NULL)
    )
    SELECT
        t.indexrelid::bigint,
        t.schemaname::character varying,
        t.tablename::character varying,
        t.indexname::character varying,
        t.indexcreate::character varying,
        t.countcol::integer,
        t.countpredicat::integer,
        t.countinclude::integer,
        t.isprimary,
        t.isunique,
        t.isindisvalid,
        COALESCE (p_predicat.x,'')::character VARYING AS p_predicat,
        COALESCE (p_include.x,'')::character VARYING AS p_include,
        t.inwhere::character varying,
        t.indnullsnotdistinct
    FROM
        tbl_indexlist t
    LEFT  JOIN LATERAL (
            SELECT string_agg(
                concat_ws(
                    ' ',
                    pg_get_indexdef(t.indexrelid, p_i, true),
                    (SELECT opcname FROM pg_opclass WHERE oid = t.indclass[p_i-1] AND opcdefault = false),
                    CASE WHEN t.indoption[p_i-1]&1=1 THEN 'DESC'  END ,
                    CASE WHEN t.indoption[p_i-1]&1<>1 AND t.indoption[p_i-1]&2=2 THEN 'NULLS FIRST'  END , 
                    CASE WHEN t.indoption[p_i-1]&1=1 AND t.indoption[p_i-1]&2<>2 THEN 'NULLS LAST'  END
                ),
                ', '
                ORDER BY p_i)
            FROM generate_series(1, t.countpredicat) AS p_i
            
        ) AS p_predicat(x) ON TRUE 
    LEFT  JOIN LATERAL (
            SELECT string_agg(
                pg_get_indexdef(t.indexrelid, p_i, true),', '
                ORDER BY p_i )
            FROM generate_series(t.countpredicat + 1, t.countcol) AS p_i
        ) AS p_include(x) ON TRUE;
    
    END;
$function$
;

with tbl as (
    select 
        s.nspname as table_schema,
        t.relname as table_name
    from
        pg_class t
        inner join pg_namespace s on s.oid = t.relnamespace
    where s.nspname  = '" + table.TableEdit.SchemaNameToSeek + @"'
    and t.relname = '" + table.TableEdit.TableNameToSeek + @"'
)
select 
    idx.indexname, 
    case when idx.isunique then 'true' else 'false' end, 
    idx.predicat, 
    idx.include, 
    idx.inwhere,
    case when idx.indnullsnotdistinct then 'true' else 'false' end
from tbl
inner join pg_temp.fn_getindexlist(tbl.table_schema::varchar,tbl.table_name::varchar) idx on true
where not idx.isprimary";

                if (!string.IsNullOrWhiteSpace(_indexname))
                {
                    queryString += Environment.NewLine + "AND Idx.indexname = '" + _indexname + "'";
                }
                queryString += Environment.NewLine + ";";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    table.ListIndex.Clear();
                    while (reader.Read())
                    {
                        table.AddIndex(reader[0].ToString(), reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), reader[4].ToString(), reader[5].ToString(), "", "true", "", "");
                        isfound = true;
                    }
                }
            }

            return isfound;
        }

        /// <summary>Определить тип таблицы TableType по имени таблицы</summary>
        /// <param name="table">Ссылка на экземпляр TableInfo, у которого заполнены SchemaName, TableName и TargetDBType </param>
        public Utilities.TableType GetTableType(TableInfo table)
        {
            string queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                queryString += @"
DECLARE @EvnClass TABLE (EvnClass_id BIGINT, EvnClass_pid BIGINT, evnclass_sysnick VARCHAR(max))
DECLARE @PersonEvnClass TABLE (PersonEvnClass_id BIGINT, PersonEvnClass_sysnick VARCHAR(max))
DECLARE @MorbusClass TABLE (MorbusClass_id BIGINT, MorbusClass_sysnick VARCHAR(max))

IF OBJECT_ID('dbo.EvnClass', 'U') IS NOT NULL
BEGIN
    insert into @EvnClass (EvnClass_id, EvnClass_pid, evnclass_sysnick)
    select l.EvnClass_id, l.EvnClass_pid, l.evnclass_sysnick
    from dbo.EvnClass l with (nolock)
    where not exists (select 1 from @EvnClass t where t.EvnClass_id = l.EvnClass_id)
END
IF OBJECT_ID('dbo.PersonEvnClass', 'U') IS NOT NULL
BEGIN
    insert into @PersonEvnClass (PersonEvnClass_id, PersonEvnClass_sysnick)
    select l.PersonEvnClass_id, l.PersonEvnClass_sysnick
    from dbo.PersonEvnClass l with (nolock)
    where not exists (select 1 from @PersonEvnClass t where t.PersonEvnClass_id = l.PersonEvnClass_id)
END
IF OBJECT_ID('dbo.MorbusClass', 'U') IS NOT NULL
BEGIN
    insert into @MorbusClass (MorbusClass_id, MorbusClass_sysnick)
    select l.MorbusClass_id, l.MorbusClass_sysnick
    from dbo.MorbusClass l with (nolock)
    where not exists (select 1 from @MorbusClass t where t.MorbusClass_id = l.MorbusClass_id)
END;

    Select table_type 
        from (
            select 
                'Evn' as table_type,
                'dbo' as schema_name,
                EvnClass_SysNick as table_name
            from @EvnClass
            union all
            select 
                'PersonEvn' as table_type,
                'dbo' as schema_name,
                PersonEvnClass_SysNick as table_name
            from @PersonEvnClass
            union all
            select 
                'Morbus' as table_type,
                'dbo' as schema_name,
                MorbusClass_SysNick as table_name
            from @MorbusClass
        ) t
    where lower(t.schema_name) = '" + table.SchemaNameToSeek + @"'
    and lower(t.table_name) = '" + table.TableNameToSeek + "'";
            }

            if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                // заглушка для баз SMP
                if (
                    (table.SchemaNameCompare == "dbo") &&
                    table.TableNameCompare.StartsWith("evn") &&
                    this.DBName.ToLower().StartsWith("smp")
                    )
                {
                    return Utilities.TableType.DICT;
                }

                queryString += @"
CREATE TEMP TABLE IF NOT EXISTS tmp_evnclass (evnclass_id bigint, evnclass_pid bigint, evnclass_sysnick varchar);
CREATE TEMP TABLE IF NOT EXISTS tmp_PersonEvnClass (PersonEvnClass_id bigint, PersonEvnClass_SysNick varchar);
CREATE TEMP TABLE IF NOT EXISTS tmp_MorbusClass (MorbusClass_id bigint, MorbusClass_SysNick varchar);

DO $$ BEGIN
IF to_regclass('dbo.evnclass') IS NOT NULL THEN
    INSERT INTO tmp_evnclass (evnclass_id, evnclass_pid , evnclass_sysnick)
    SELECT l.evnclass_id, l.evnclass_pid, l.evnclass_sysnick
    FROM dbo.evnclass l
    where not exists (select 1 from tmp_evnclass t where t.EvnClass_id = l.EvnClass_id);
END IF;

IF to_regclass('dbo.personevnclass') IS NOT NULL THEN
    insert into tmp_PersonEvnClass (PersonEvnClass_id, PersonEvnClass_sysnick)
    select l.PersonEvnClass_id, l.PersonEvnClass_sysnick
    from dbo.PersonEvnClass l
    where not exists (select 1 from tmp_PersonEvnClass t where t.PersonEvnClass_id = l.PersonEvnClass_id);
END IF;

IF to_regclass('dbo.morbusclass') IS NOT NULL THEN
    insert into tmp_MorbusClass (MorbusClass_id, MorbusClass_sysnick)
    select l.MorbusClass_id, l.MorbusClass_sysnick
    from dbo.MorbusClass l
    where not exists (select 1 from tmp_MorbusClass t where t.MorbusClass_id = l.MorbusClass_id);
END IF;
END; $$;

Select table_type 
    from (
        select 
            'Evn' as table_type,
            'dbo' as schema_name,
            EvnClass_SysNick as table_name
        from tmp_EvnClass
        union all
        select 
            'PersonEvn' as table_type,
            'dbo' as schema_name,
            PersonEvnClass_SysNick as table_name
        from tmp_PersonEvnClass
        union all
        select 
            'Morbus' as table_type,
            'dbo' as schema_name,
            MorbusClass_SysNick as table_name
        from tmp_MorbusClass
    ) t
where lower(t.schema_name) = lower('" + table.SchemaNameToSeek + @"') 
and lower(t.table_name) = lower('" + table.TableNameToSeek + "');";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        string s = reader[0].ToString();
                        switch (s)
                        {
                            case "Evn": return Utilities.TableType.EVN;
                            case "PersonEvn": return Utilities.TableType.PERSONEVN;
                            case "Morbus": return Utilities.TableType.MORBUS;
                            default: return Utilities.TableType.DICT;
                        }
                    }
                }
            }
            return Utilities.TableType.DICT;
        }

        /// <summary>Список дочерних Evn-таблиц</summary>
        /// <param name="EvnClass_SysNick_pid">Ссылка на родительский класс</param>
        /// <param name="isAutoReconnect">=true - переподключение при ошибке</param>
        /// <param name="checkInherit">=true - вернуть только те дочерние таблицы, которые созданы от текущей как наследники</param>
        public List<string> GetListEvnChilds(string EvnClass_SysNick_pid, bool isAutoReconnect, bool checkInherit)
        {
            List<string> result = new List<string>();

            string queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                if (!string.IsNullOrWhiteSpace(EvnClass_SysNick_pid) && checkInherit) return result;

                queryString += @"
DECLARE @EvnClass TABLE (EvnClass_id BIGINT, EvnClass_pid BIGINT, evnclass_sysnick VARCHAR(max))

IF OBJECT_ID('dbo.EvnClass', 'U') IS NOT NULL
BEGIN
    insert into @EvnClass (EvnClass_id, EvnClass_pid, evnclass_sysnick)
    select l.EvnClass_id, l.EvnClass_pid, l.evnclass_sysnick
    from dbo.EvnClass l with (nolock)
    where not exists (select 1 from @EvnClass t where t.EvnClass_id = l.EvnClass_id)
END;
";
                if (!string.IsNullOrWhiteSpace(EvnClass_SysNick_pid))
                {
                    queryString += @"
WITH r AS (
    select t.* 
    from @EvnClass t 
    where Evnclass_SysNick = '" + EvnClass_SysNick_pid + @"'
union all
    select t.* 
    from @EvnClass t 
    join r on t.EvnClass_pid = coalesce(r.EvnClass_id, 0)
),
child AS (
    select r.EvnClass_SysNick 
    from r
    where not (r.EvnClass_SysNick = '" + EvnClass_SysNick_pid + @"')
)";
                }

                queryString += Environment.NewLine + @"
select 
    EvnClass_SysNick as table_name
from @EvnClass
where coalesce(EvnClass_SysNick,'')<>''";

                if (!string.IsNullOrWhiteSpace(EvnClass_SysNick_pid))
                {
                    queryString += Environment.NewLine + @"and EvnClass_SysNick in (select EvnClass_SysNick from child)";
                }

                queryString += Environment.NewLine + @"order by EvnClass_SysNick";
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString += @"
CREATE TEMP TABLE IF NOT EXISTS tmp_evnclass (evnclass_id bigint, evnclass_pid bigint, evnclass_sysnick varchar);

DO $$ BEGIN
IF to_regclass('dbo.evnclass') IS NOT NULL THEN
    INSERT INTO tmp_evnclass (evnclass_id, evnclass_pid , evnclass_sysnick)
    SELECT l.evnclass_id, l.evnclass_pid, l.evnclass_sysnick
    FROM dbo.evnclass l
    where not exists (select 1 from tmp_evnclass t where t.EvnClass_id = l.EvnClass_id);
END IF;
END; $$;
";

                if (!string.IsNullOrWhiteSpace(EvnClass_SysNick_pid))
                {
                    queryString += @"
WITH RECURSIVE r AS (
    select t.* 
    from tmp_EvnClass t 
    where EvnClass_SysNick ilike '" + EvnClass_SysNick_pid + @"'
union
    select t.* 
    from tmp_EvnClass t 
    join r on t.EvnClass_pid = coalesce(r.EvnClass_id, 0)
),
child AS (
    select r.EvnClass_SysNick 
    from r
    where not (r.EvnClass_SysNick ilike '" + EvnClass_SysNick_pid + @"')
)";
                }

                if (string.IsNullOrWhiteSpace(EvnClass_SysNick_pid))
                {

                    queryString += Environment.NewLine + @"
select 
    EvnClass_SysNick as table_name
from tmp_EvnClass
where coalesce(EvnClass_SysNick,'')<>''";
                }
                else 
                {
                    queryString += Environment.NewLine + @"
select 
    EvnClass_SysNick as table_name
from child
where coalesce(EvnClass_SysNick,'')<>''";

                    if (checkInherit)
                    {
                        queryString += Environment.NewLine + @"and exists (
select 1 
from pg_catalog.pg_inherits inh 
inner join pg_catalog.pg_class p on inh.inhparent = p.oid and p.relname ilike '" + EvnClass_SysNick_pid + @"' and p.relnamespace = to_regnamespace('dbo')
inner join pg_catalog.pg_class c on inh.inhrelid = c.oid and c.relname ilike EvnClass_SysNick and c.relnamespace = to_regnamespace('dbo')
limit 1
)";
                    }

                }

                queryString += Environment.NewLine + @"order by EvnClass_SysNick";
            }

            try
            {
                using (DbDataReader reader = this.OpenQuery(queryString, isAutoReconnect))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            string s = reader[0].ToString();
                            result.Add(s);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
        }

        /// <summary>Вернуть список DataTable - кто менял таблицу</summary>
        /// <param name="table">Ссылка на экземпляр TableInfo, у которого заполнены SchemaName и TableName</param>
        /// <param name="limit">Кол-во записей в списке</param>
        public DataTable GetLastChangeList(TableInfo table, int limit = 20)
        {
            string queryString = "";
            if (this.ConnType == Utilities.ConnType.MSSQL)
                queryString = @"SELECT TOP (" + limit.ToString() + @") *
FROM AlterObjectLog
WHERE alterobjectlog_schemaname='" + table.SchemaNameToSeek + @"'
AND alterobjectlog_objectname = '" + table.TableNameToSeek + @"'
ORDER BY alterobjectlog_insdt desc";
            else if (this.ConnType == Utilities.ConnType.PGSQL)
                queryString = @"SELECT *
FROM AlterObjectLog
WHERE alterobjectlog_schemaname='" + table.SchemaNameToSeek + @"'
AND alterobjectlog_objectname = '" + table.FullTableNameToSeek + @"'
ORDER BY alterobjectlog_insdt desc limit " + limit.ToString();

            return this.FillDataTable(queryString, out string Messages);
        }


        /// <summary>Вернуть DataTable - список FK таблицы</summary>
        /// <param name="_schema">схема</param>
        /// <param name="_table">таблица</param>
        public DataTable GetTableFKList(string _schema, string _table)
        {
            string queryString = "";
            _schema = _schema.Replace("\"","");
            _table = _table.Replace("\"", "");

            if (this.ConnType == Utilities.ConnType.MSSQL)
                queryString = @"
select c.name as field, s.name as fkschema, t.name as fktable, fk_c.name as fkfield
from sys.foreign_keys fk
inner join sys.foreign_key_columns fkc on fkc.constraint_object_id=fk.object_id
inner join sys.columns c on c.object_id=fkc.parent_object_id and c.column_id=fkc.parent_column_id
inner join sys.columns fk_c on fk_c.object_id=fkc.referenced_object_id and fk_c.column_id=fkc.referenced_column_id
inner join sys.tables t on t.object_id=fk.referenced_object_id
inner join sys.schemas s on s.schema_id=t.schema_id
where s.name + '.' + t.name not in ('dbo.yesno', 'dbo.sex','dbo.address','dbo.klarea','dbo.klstreet','dbo.klcountry')
and c.name <> 'server_id'
and fk.parent_object_id = object_id(N'" + _schema + "." + _table + @"', 'U')
order by c.column_id";
            else if (this.ConnType == Utilities.ConnType.PGSQL)
                queryString = @" 
SELECT
	c.attname as field, 
	fk.table_schema as fkschema, 
	fk.table_name as fktable, 
	fk.column_name as fkfield
FROM pg_class t
INNER JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
INNER JOIN pg_catalog.pg_attribute c ON c.attrelid = t.oid 
	AND c.attnum > 0 -- без служебных полей
	AND c.attisdropped = false -- не удалено
INNER JOIN LATERAL (
	select 
		o.conname as constraint_name, 
		fs.nspname as table_schema, 
		f.relname as table_name, 
		fc.FKField as column_name,
		fc.FKOrder
	FROM pg_constraint o
	INNER JOIN pg_class f ON f.oid = o.confrelid
	INNER JOIN pg_namespace fs ON fs.oid = f.relnamespace
	-- поля primary key в таблице внешнего ключа
	INNER JOIN LATERAL (
		SELECT
			array_position(o.confkey, a.attnum) as FKOrder,
			a.attname as FKField
		FROM pg_attribute a 
		WHERE a.attrelid = o.confrelid 
		AND a.attnum = ANY(o.confkey)
		AND a.attisdropped = false -- не удалено
	) fc on fc.FKOrder = array_position(o.conkey, c.attnum)
	WHERE o.conrelid = t.oid
	AND o.contype = 'f'
	AND c.attnum = ANY(o.conkey)
	limit 1
) fk on true
WHERE n.nspname iLIKE '" + _schema + @"'
AND t.relname iLIKE '" + _table + @"'
and t.relkind in ('r','f','p')
and n.nspname || '.' || t.relname not in ('dbo.yesno','dbo.sex','dbo.address','dbo.klarea','dbo.klstreet','dbo.klcountry')
and c.attname <> 'server_id'
order by c.attnum
";
            return this.FillDataTable(queryString, out string Messages);
        }

        /// <summary>Вернуть список найденных объектов БД (таблицы, хранимки, представления и т.п.)</summary>
        /// <param name="partname">часть имени</param>
        /// <param name="isExact">true - точное имя</param>
        /// <param name="isAddTables">true - добавить в список таблицы</param>
        /// <param name="isAddForeignTables">true - добавить в список внешние таблицы</param>
        /// <param name="isAddViews">true - добавить в список представления</param>
        /// <param name="isAddProcs">true - добавить в список процедуры, функции (SQL)</param>
        /// <param name="isAddProcsNotSQL">true - добавить в список процедуры, функции (не SQL)</param>
        /// <param name="isAddTriggers">true - добавить в список триггера</param>
        /// <param name="isAddSequences">true - добавить в список сиквенсы</param>
        /// <param name="isAddIndexes">true - добавить в список сиквенсы</param>
        public List<DBObjectInfo> GetObjectList(string partname, bool isExact, bool isAddTables, bool isAddForeignTables, bool isAddViews, bool isAddProcs, bool isAddProcsNotSQL, bool isAddTriggers, bool isAddSequences, bool isAddIndexes)
        {
            string queryString = "";

            if (!isExact)
            {
                partname = "%" + partname + "%";
            }

            List<DBObjectInfo> result = new List<DBObjectInfo>();

            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                if (isAddTables)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine; //-V3022

                    queryString += @"
select distinct
    10 as ord,
    'TABLE' as typ,
    TABLE_SCHEMA as schemaname,
    TABLE_NAME as name,
    '' as Autogen
from information_schema.TABLES
where TABLE_SCHEMA NOT IN ('information_schema', 'sys', 'tmp', 'TABLE tmp', 'raw', 'symdict', 'SWN\savage', 'audit', 'public', 'eyes', 'liquibase', 'diff', 'box')
and TABLE_SCHEMA + '.' + TABLE_NAME like '" + partname + @"'
and TABLE_TYPE = 'BASE TABLE'
";
                }

                if (isAddViews)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine; //-V3187

                    queryString += @"
select distinct
    20 as ord,
    'VIEW' as typ,
    TABLE_SCHEMA as schemaname,
    TABLE_NAME as name,
    case 
        when CharIndex('AUTOGEN', 
                Substring(comments.description, 1, CharIndex('create view', comments.description, 0) ), 
                0) > 0
            then 'AUTOGEN'
        else ''
    end as Autogen
from information_schema.VIEWS
outer apply (
    select top 1 text as description
    from sys.syscomments with (NOLOCK) 
    where id = object_id(TABLE_SCHEMA + '.' + TABLE_NAME)
) comments
where TABLE_SCHEMA NOT IN ('information_schema', 'sys', 'tmp', 'TABLE tmp', 'raw', 'symdict', 'SWN\savage', 'audit', 'public', 'eyes', 'liquibase', 'diff', 'box')
and TABLE_SCHEMA + '.' + TABLE_NAME like '" + partname + @"'
";
                }

                if (isAddProcs)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select distinct
    30 as ord,
    ROUTINE_TYPE as typ,
    ROUTINE_SCHEMA as schemaname,
    ROUTINE_NAME as name,
    case 
        when CharIndex('AUTOGEN', 
                Substring(comments.description, 1, CharIndex('create proc', comments.description, 0) ), 
                0) > 0
            or
            CharIndex('AUTOGEN', 
                Substring(comments.description, 1, CharIndex('create func', comments.description, 0) ), 
                0) > 0
            then 'AUTOGEN'
        else ''
    end as Autogen
from information_schema.ROUTINES
outer apply (
    select top 1 text as description
    from sys.syscomments with (NOLOCK) 
    where id = object_id(ROUTINE_SCHEMA + '.' + ROUTINE_NAME)
) comments
where ROUTINE_SCHEMA NOT IN ('information_schema', 'sys', 'tmp', 'TABLE tmp', 'raw', 'symdict', 'SWN\savage', 'audit', 'public', 'eyes', 'liquibase', 'diff', 'box')
and ROUTINE_SCHEMA + '.' + ROUTINE_NAME like '" + partname + @"'
";
                }

                if (isAddIndexes)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
SELECT distinct
	60 as ord, 
	'INDEX' as typ,
	s.name as schemaname,
	i.name as name, 
	'' as isAotogen
FROM sys.indexes i (NOLOCK)
inner join sys.TABLES t (NOLOCK) on t.object_id = i.object_id
inner join sys.schemas s (nolock) on s.schema_id = t.schema_id
where s.name NOT IN ('information_schema', 'sys', 'tmp', 'TABLE tmp', 'raw', 'symdict', 'SWN\savage', 'audit', 'public', 'eyes', 'liquibase', 'diff', 'box')
and s.name + '.' + t.name like '" + partname + @"'
AND i.[type] <> 1
";
                }

                if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "order by ord, typ, schemaname, name";
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                if (isAddTables)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) //-V3022
                    {
                        queryString += Environment.NewLine + "union" + Environment.NewLine; 
                    }

                    queryString += Environment.NewLine + @"
SELECT distinct
    11 as ord,
    'TABLE' as typ,
    n.nspname as schemaname,
    t.relname as name,
    '' as Autogen
FROM pg_catalog.pg_class t
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('r','p')
and n.nspname || '.' || t.relname ilike '" + partname + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
";
                }

                if (isAddForeignTables)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine; //-V3187

                    queryString += Environment.NewLine + @"
SELECT distinct
    12 as ord,
    'TABLE' as typ,
    n.nspname as schemaname,
    t.relname as name,
    '' as Autogen
FROM pg_catalog.pg_class t
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('f')
and n.nspname || '.' || t.relname ilike '" + partname + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
";
                }

                if (isAddViews)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select distinct
    21 as ord,
    'VIEW' as typ,
    schemaname as schemaname,
    viewname as name,
    case 
        when strpos(lower(comments.description), 'autogen') > 0 OR strpos(lower(comments.description), 'autoge') > 0 then 'AUTOGEN'
        else ''
    end as Autogen
from pg_views 
left join lateral (
    SELECT
        obj_description as description
    FROM obj_description(
        to_regclass(quote_ident(schemaname) || '.' || quote_ident(viewname))
    )
) comments on true
where schemaname || '.' || viewname ilike '" + partname + @"'
and schemaname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and schemaname not like '%\_old'
AND schemaname not like 'pg\_%'

union

select distinct
    22 as ord,
    'MATERIALIZED VIEW' as typ,
    schemaname as schemaname,
    matviewname as name,
    case 
        when strpos(lower(comments.description), 'autogen') > 0 OR strpos(lower(comments.description), 'autoge') > 0 then 'AUTOGEN'
        else ''
    end as Autogen
from pg_matviews 
left join lateral (
    SELECT
        obj_description as description
    FROM obj_description(
        to_regclass(quote_ident(schemaname) || '.' || quote_ident(matviewname))
    )
) comments on true
where schemaname || '.' || matviewname ilike '" + partname + @"'
and schemaname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and schemaname not like '%\_old'
AND schemaname not like 'pg\_%'
";
                }

                if (isAddProcs)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select distinct
    31 as ord,
    case 
        when p.prokind='f' then 'FUNCTION'
        when p.prokind='p' then 'PROCEDURE'
        else '?'
    end as typ,
    n.nspname as schemaname,
    p.proname as name,
    case 
        when strpos(lower(p.prosrc), 'autogen') > 0 OR strpos(lower(p.prosrc), 'autoge') > 0 then 'AUTOGEN'
        else ''
    end as Autogen
from pg_proc p INNER JOIN pg_namespace n ON n.oid = p.pronamespace
where n.nspname || '.' || p.proname ilike '" + partname + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
and exists (select 1 from pg_language l where l.oid = p.prolang and l.lanname in ('sql', 'plpgsql'))
";
                }

                if (isAddProcsNotSQL)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select distinct
    32 as ord,
    case 
        when p.prokind='f' then 'FUNCTION'
        when p.prokind='p' then 'PROCEDURE'
        else '?'
    end as typ,
    n.nspname as schemaname,
    p.proname as name,
    case 
        when strpos(lower(p.prosrc), 'autogen') > 0 OR strpos(lower(p.prosrc), 'autoge') > 0 then 'AUTOGEN'
        else ''
    end as Autogen
from pg_proc p INNER JOIN pg_namespace n ON n.oid = p.pronamespace
where n.nspname || '.' || p.proname ilike '" + partname + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
and exists (select 1 from pg_language l where l.oid = p.prolang and not l.lanname in ('sql', 'plpgsql'))
";
                }

                if (isAddTriggers)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select distinct
	40 as ord,
    'TRIGGER' as typ,
    n.nspname as schemaname,
    trg.tgname as name,
    '' as Autogen
from pg_trigger trg
inner join pg_class t on t.oid = trg.tgrelid
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('r','f','p')
and n.nspname || '.' || trg.tgname ilike '" + partname + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
AND trg.tgname not like '%\_audit\_%'
AND trg.tgname not like '%\_replicator\_%'
and NOT trg.tgisinternal
";
                }

                if (isAddSequences)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select distinct
	50 as ord,
    'SEQUENCE' as typ,
    n.nspname as schemaname,
    t.relname as name,
    '' as Autogen
from pg_class t 
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('S')
and n.nspname || '.' || t.relname ilike '" + partname + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
";
                }

                if (isAddIndexes)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select distinct
	60 as ord,
    'INDEX' as typ,
    n.nspname as schemaname,
    t.relname as name,
    '' as Autogen
from pg_class t 
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
inner join pg_index i on i.indexrelid = t.oid and not indisprimary
where t.relkind in ('i')
and n.nspname || '.' || t.relname ilike '" + partname + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
";
                }

                if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "order by ord, typ, schemaname, name";
            }

            result.Clear();

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        result.Add(new DBObjectInfo()
                        {
                            Type = reader[1].ToString(),
                            Schema = reader[2].ToString(),
                            Name = reader[3].ToString(),
                            isAutogen = reader[4].ToString()
                        }
                        );
                    }
                }
            }

            return result;
        }

        /// <summary>Вернуть OID найденных объектов БД по имени</summary>
        /// <param name="schema">схема объекта</param>
        /// <param name="name">имя объекта</param>
        public List<DBObjectOID> GetObjectOIDList(string schema, string name)
        {
            string queryString = "";

            List<DBObjectOID> result = new List<DBObjectOID>();

            if (this.ConnType == Utilities.ConnType.PGSQL)
            {

                // корректная обработка символа подчеркивания в like\ilike
                schema = schema.Replace("_", "\\_");
                name = name.Replace("_", "\\_");

                //if (isAddTables)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) //-V3022
                    {
                        queryString += Environment.NewLine + "union" + Environment.NewLine;
                    }

                    queryString += Environment.NewLine + @"
SELECT 
    11 as ord,
    null::oid as oid,
    n.nspname as schemaname,
    t.relname as name
FROM pg_catalog.pg_class t
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('r','p')
and n.nspname ilike '" + schema + @"'
and t.relname ilike '" + name + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
";
                }

                //if (isAddForeignTables)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine; //-V3187

                    queryString += Environment.NewLine + @"
SELECT 
    12 as ord,
    null::oid as oid,
    n.nspname as schemaname,
    t.relname as name
FROM pg_catalog.pg_class t
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('f')
and n.nspname ilike '" + schema + @"'
and t.relname ilike '" + name + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
";
                }

                //if (isAddViews)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select   
    21 as ord,
    null::oid as oid,
    schemaname as schemaname,
    viewname as name
from pg_views 
where schemaname ilike '" + schema + @"'
and viewname ilike '" + name + @"'
and schemaname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and schemaname not like '%\_old'
AND schemaname not like 'pg\_%'

union

select 
    22 as ord,
    null::oid as oid,
    schemaname as schemaname,
    matviewname as name
from pg_matviews 
where schemaname ilike '" + schema + @"'
and matviewname ilike '" + name + @"'
and schemaname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and schemaname not like '%\_old'
AND schemaname not like 'pg\_%'
";
                }

                //if (isAddProcs)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select 
    31 as ord,
    p.oid as oid,
    n.nspname as schemaname,
    p.proname as name
from pg_proc p 
INNER JOIN pg_namespace n ON n.oid = p.pronamespace
where n.nspname ilike '" + schema + @"'
and p.proname ilike '" + name + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
and exists (select 1 from pg_language l where l.oid = p.prolang and l.lanname in ('sql', 'plpgsql'))
";
                }

                //if (isAddProcsNotSQL)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select 
    32 as ord,
    p.oid as oid,
    n.nspname as schemaname,
    p.proname as name
from pg_proc p 
INNER JOIN pg_namespace n ON n.oid = p.pronamespace
where n.nspname ilike '" + schema + @"'
and p.proname ilike '" + name + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
and exists (select 1 from pg_language l where l.oid = p.prolang and not l.lanname in ('sql', 'plpgsql'))
";
                }

                //if (isAddTriggers)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select 
    40 as ord,
    trg.oid,
    n.nspname as schemaname,
    trg.tgname as name
from pg_trigger trg
inner join pg_class t on t.oid = trg.tgrelid
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('r','f','p')
and n.nspname ilike '" + schema + @"'
and trg.tgname ilike '" + name + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
AND trg.tgname not like '%\_audit\_%'
AND trg.tgname not like '%\_replicator\_%'
and NOT trg.tgisinternal
";
                }

                //if (isAddSequences)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select 
    50 as ord,
    null::oid as oid,
    n.nspname as schemaname,
    t.relname as name
from pg_class t 
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('S')
and n.nspname ilike '" + schema + @"'
and t.relname ilike '" + name + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
";
                }

                //if (isAddIndexes)
                {
                    if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "union" + Environment.NewLine;

                    queryString += @"
select 
    60 as ord,
    null::oid as oid,
    n.nspname as schemaname,
    t.relname as name
from pg_class t 
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
inner join pg_index i on i.indexrelid = t.oid and not indisprimary
where t.relkind in ('i')
and n.nspname ilike '" + schema + @"'
and t.relname ilike '" + name + @"'
and n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff', 'box')
and n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
";
                }

                if (!string.IsNullOrWhiteSpace(queryString)) queryString += Environment.NewLine + "order by schemaname, name, ord, oid nulls last";
            }

            result.Clear();

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            result.Add(new DBObjectOID()
                            {
                                OID = reader[1].ToString(),
                                Schema = reader[2].ToString(),
                                Name = reader[3].ToString()
                            }
                            );
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>Вернуть текст хранимки</summary>
        /// <param name="_schema">схема</param>
        /// <param name="_objectname">имя объекта</param>
        /// <param name="_schemaseek">схема - для поиска в БД</param>
        /// <param name="_objectseek">имя объекта - для поиска в БД</param>
        /// <param name="isAddRegion">true - добавляем проверку региональности</param>
        /// <param name="txtRegion">номер региона</param>
        /// <param name="_scripttype">тип хранимк</param>
        /// <param name="_objecttype">аббревиатура типа хранимки для MS</param>
        /// <param name="_for_tablename">если индекс или триггер - для какой таблицы</param>
        /// <param name="project">проект GIT</param>
        /// <param name="OID">oid конкретного объекта, если есть перегрузки (для ПГ)</param>
        /// <param name="isIndexCreate">=true создание индексов командой CREATE INDEX</param>
        public List<string> GetProcText(ref string _schema, ref string _objectname, ref string _schemaseek, ref string _objectseek, bool isAddRegion, string txtRegion, ref string _scripttype, ref string _objecttype, ref string _for_tablename, string project, string OID, bool isIndexCreate)
        {
            _objecttype = "";
            List<string> result = new List<string>();
            _for_tablename = "";
            _objectseek = "";
            _schemaseek = "";


            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                // определяем тип
                string queryString = @"
select top(1) 
    s.name as schemaname,
    o.name as objectname,
    rtrim(ltrim(o.type)) as objecttype,
    case when o.type = 'U' then o.name else '' end as tablename
from sys.objects o with (nolock)
inner join sys.schemas s with (nolock) on s.schema_id = o.schema_id
where o.object_id = OBJECT_ID(N'" + _schema + "." + _objectname + @"')
and o.type in ('P', 'TF', 'IF', 'FN', 'V', 'U')
union
SELECT top(1)
    s.name as schemaname,
    i.name as objectname,
	'I' as objecttype,
    t.name as tablename
FROM sys.indexes i with (NOLOCK)
inner join sys.TABLES t with (NOLOCK) on t.object_id = i.object_id
inner join sys.schemas s with (nolock) on s.schema_id = t.schema_id
where s.name + '.' + i.name = '" + _schema + "." + _objectname + @"'
AND i.[type] <> 1
";

                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            _schema = reader[0].ToString();
                            _objectname = reader[1].ToString();
                            _objecttype = reader[2].ToString();
                            if (_objecttype.ToUpper() == "V") _scripttype = "view";
                            if (_objecttype.ToUpper() == "TF") _scripttype = "function";
                            if (_objecttype.ToUpper() == "IF") _scripttype = "function";
                            if (_objecttype.ToUpper() == "FN") _scripttype = "function";
                            if (_objecttype.ToUpper() == "P") _scripttype = "procedure";
                            if (_objecttype.ToUpper() == "U") _scripttype = "table";
                            if (_objecttype.ToUpper() == "I") _scripttype = "index";
                            _for_tablename = reader[3].ToString();

                            break; //-V3020
                        }
                    }
                }

                _schemaseek = _schema;
                _objectseek = _objectname;

                if (_scripttype == "table")
                {
                    // генерим текст скрипта для таблицы
                    TableDB table = new TableDB();

                    table.GITProject = project;
                    table.ScriptType = Utilities.ScriptType.CREATE;
                    table.TableEdit.SchemaName = _schema;
                    table.TableEdit.TableName = _objectname;
                    table.TableType = Utilities.TableType.DICT;

                    bool isfound = this.FillTableInfo(table.TableEdit);
                    if (isfound)
                    {
                        try
                        {
                            table.TableType = this.GetTableType(table.TableEdit);
                        }
                        catch (Exception)
                        {
                            table.TableType = Utilities.TableType.DICT;
                        }

                        try
                        {
                            this.FillListIndex(table, "");
                        }
                        catch (Exception)
                        {
                        }

                        isfound = this.FillListField(table.TableEdit);

                        if (isfound)
                        {
                            List<string> ProcCommand = new List<string>();
                            int ProcCommandNum = -1;
                            table.isOnlyExist = true;
                            string tabletext = TableDB.GenerateTableScript(this, false, isAddRegion, txtRegion, out ProcCommand, out ProcCommandNum, false, out string RowInfo, table);
                            var arr = tabletext.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                            result.AddRange(arr);
                        }
                    }
                }

                if (_scripttype == "index")
                {
                    // генерим текст скрипта для индекса
                    TableDB table = new TableDB();

                    table.GITProject = project;
                    table.ScriptType = Utilities.ScriptType.CREATE;
                    table.TableEdit.SchemaName = _schema;
                    table.TableEdit.TableName = _for_tablename;
                    table.TableType = Utilities.TableType.DICT;

                    bool isfound = this.FillListIndex(table, _objectseek);
                    if (isfound)
                    {
                        foreach (var item in table.ListIndex)
                        {
                            string indextext = TableDB.GenerateIndexScript(Utilities.ScriptType.CREATE, false, isIndexCreate, isAddRegion, txtRegion, item);
                            var arr = indextext.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                            result.AddRange(arr);
                        }
                    }
                }

                if ((_scripttype == "view") || (_scripttype == "function") || (_scripttype == "procedure"))
                {
                    // ищем и выгружаем текст представления или хранимки MSSQL
                    queryString = "SELECT OBJECT_DEFINITION(OBJECT_ID(N'" + _schema + "." + _objectname + "'))";

                    result.Clear();

                    using (DbDataReader reader = this.OpenQuery(queryString))
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                string proctext = reader[0].ToString();

                                // исправляем форматирование хранимки
                                if (!string.IsNullOrWhiteSpace(proctext))
                                {
                                    proctext = proctext.Replace("\r\n", "\n");
                                    proctext = proctext.Replace("\r", "");

                                    var arr = proctext.Split(new string[] { "\n" }, StringSplitOptions.None);

                                    for (int i = 0; i < arr.Length; i++)
                                    {
                                        if (
                                            arr[i].Trim().ToUpper().StartsWith("CREATE PROC") ||
                                            arr[i].Trim().ToUpper().StartsWith("CREATE FUNC") ||
                                            arr[i].Trim().ToUpper().StartsWith("CREATE VIEW")
                                            )
                                        {
                                            arr[i] = arr[i].Replace("[", "");
                                            arr[i] = arr[i].Replace("]", "");
                                        }
                                    }

                                    result.AddRange(arr);
                                }
                            }
                        }
                    }
                }

            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                // корректная обработка символа подчеркивания в like\ilike
                string _schemalike = _schema.Replace("_", "\\_");
                string _objectlike = _objectname.Replace("_", "\\_");

                // запрос для поиска объекта в БД
                string queryString = "";
                string queryString_def = @"
SELECT
    null::oid as oid,
    n.nspname as schemaname,
    t.relname as objectname,
    CASE 
        WHEN t.relkind in ('r','f','p') THEN 'TABLE'
        WHEN t.relkind in ('S') THEN 'SEQUENCE'
        WHEN t.relkind in ('v') THEN 'VIEW'
        WHEN t.relkind in ('m') THEN 'MATERIALIZED VIEW'
    END as objecttype,
    CASE 
        WHEN t.relkind in ('r','f','p') THEN t.relname
        WHEN t.relkind in ('S') THEN ''
        WHEN t.relkind in ('v') THEN ''
        WHEN t.relkind in ('m') THEN ''
    END as tablename
FROM pg_catalog.pg_class t
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('r','f','p','S','v','m')
and n.nspname ilike '" + _schemalike + @"'
and t.relname ilike '" + _objectlike + @"'

union

SELECT
    null::oid as oid,
    n.nspname as schemaname,
    t.relname as objectname,
    'INDEX' as objecttype,
    i.tablename
FROM pg_catalog.pg_class t
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
inner join lateral (
	select i.tablename
    from pg_indexes i
    where i.schemaname = n.nspname
    and i.indexname =  t.relname
) i on true 
where t.relkind in ('i')
and n.nspname ilike '" + _schemalike + @"'
and t.relname ilike '" + _objectlike + @"'

union

SELECT
    p.oid,
    n.nspname as schemaname,
    p.proname as objectname,
    case 
        when p.prokind='f' then 'FUNCTION'
        when p.prokind='p' then 'PROCEDURE'
    end as objecttype,
    '' as tablename
from pg_proc p 
INNER JOIN pg_namespace n ON n.oid = p.pronamespace
where p.prokind in ('f', 'p')
and n.nspname ilike '" + _schemalike + @"'
and p.proname ilike '" + _objectlike + @"'";

                if (!string.IsNullOrWhiteSpace(OID))
                {
                    queryString_def += Environment.NewLine + "and p.oid = " + OID;
                }

                queryString_def += @"

union

select
    trg.oid,
    n.nspname as schemaname,
    trg.tgname as objectname,
    'TRIGGER' as objecttype,
    t.relname as tablename
from pg_trigger trg
inner join pg_class t on t.oid = trg.tgrelid
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('r','f','p')
and n.nspname ilike '" + _schemalike + @"'
and trg.tgname ilike '" + _objectlike + @"'";

                if (!string.IsNullOrWhiteSpace(OID))
                {
                    queryString_def += Environment.NewLine + "and trg.oid = " + OID;
                }

                if (!string.IsNullOrWhiteSpace(OID))
                {
                    queryString_def += Environment.NewLine + "order by oid nulls last";
                }
                else
                {
                    queryString_def += Environment.NewLine + "order by oid nulls first";
                }

                // 1. Ищем точное соответствие
                bool isExact = false;
                queryString = queryString_def.Replace("ilike", "=").Replace("\\_", "_");

                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            _schema = reader[1].ToString();
                            _objectname = reader[2].ToString();
                            _scripttype = reader[3].ToString().ToLower();
                            _for_tablename = reader[4].ToString();
                            isExact = true;

                            break; //-V3020
                        }
                    }
                }

                // 2. Если не нашли по точному соответствию - ищем без учета регистра
                if (!isExact)
                {
                    queryString = queryString_def;
                    using (DbDataReader reader = this.OpenQuery(queryString))
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                _schema = reader[1].ToString();
                                _objectname = reader[2].ToString();
                                _scripttype = reader[3].ToString().ToLower();
                                _for_tablename = reader[4].ToString();

                                break; //-V3020
                            }
                        }
                    }
                }

                _schemaseek = _schema.Replace("\"", "");
                _objectseek = _objectname.Replace("\"", "");

                _schemalike = _schemaseek.Replace("_", "\\_");
                _objectlike = _objectseek.Replace("_", "\\_");

                if (_objectname.ToLower() != _objectname)
                {
                    _objectname = "\"" + _objectname.Replace("\"", "") + "\"";
                }
                if (_schema.ToLower() != _schema)
                {
                    _schema = "\"" + _schema.Replace("\"", "") + "\"";
                }

                if ((_scripttype == "procedure") || (_scripttype == "function"))
                {

                    // ищем и выгружаем текст хранимки PGSQL
                    queryString = @"
select 
    pg_get_functiondef(p.oid), 
    provolatile,
    case when proisstrict then '2' else '1' end as proisstrict,
    case when prosecdef then '2' else '1' end as prosecdef,
    procost
from pg_proc p
INNER JOIN pg_namespace n ON n.oid = p.pronamespace
where p.proname ilike '" + _objectlike + @"'
and n.nspname ilike '" + _schemalike + @"'";

                    if (!string.IsNullOrWhiteSpace(OID))
                    {
                        queryString += Environment.NewLine + "and p.oid = " + OID;
                    }

                    queryString += @"
limit 1
";
                    // если найдено точное соответствие
                    if (isExact) queryString = queryString.Replace("ilike", "=").Replace("\\_", "_");

                    result.Clear();

                    using (DbDataReader reader = this.OpenQuery(queryString))
                    {
                        if (reader != null)
                        {

                            while (reader.Read())
                            {
                                string proctext = reader[0].ToString();
                                string _provolatile = reader[1].ToString();
                                string _proisstrict = reader[2].ToString();
                                string _prosecdef = reader[3].ToString();
                                string _procost = reader[4].ToString();

                                proctext = (proctext ?? "").TrimEndNewLine();

                                //bool isExistSTABLE = false;
                                //bool isExistIMMUTABLE = false;
                                //bool isExistNULLINPUT = false;
                                //bool isExistDEFINER = false;
                                bool isExistCOST = false;

                                // исправляем форматирование хранимки
                                if (!string.IsNullOrWhiteSpace(proctext))
                                {
                                    proctext = proctext.Replace("\r\n", "\n");
                                    proctext = proctext.Replace("\r", "");

                                    var arr = proctext.Split(new string[] { "\n" }, StringSplitOptions.None);

                                    for (int i = 0; i < arr.Length; i++)
                                    {
                                        if (
                                            arr[i].Trim().ToUpper().StartsWith("RETURNS") ||
                                            arr[i].Trim().ToUpper().StartsWith("LANGUAGE")
                                            )
                                        {
                                            arr[i] = arr[i].Trim();
                                        }

                                        if (
                                            arr[i].Trim().ToUpper().StartsWith("CREATE OR REPLACE")
                                            )
                                        {
                                            arr[i] = arr[i].Replace(" (", "(");
                                            arr[i] = arr[i].Replace("( ", "(");
                                            arr[i] = arr[i].Replace(" ,", ",");
                                            arr[i] = arr[i].Replace(", ", ",");
                                            arr[i] = arr[i].Replace(" )", ")");
                                            arr[i] = arr[i].Replace(") ", ")");
                                            if (arr[i].Contains("()"))
                                            {
                                                arr[i] = arr[i].Replace("(", " (");
                                            }
                                            else
                                            {
                                                arr[i] = arr[i].Replace("(", " (" + Environment.NewLine + "\t");
                                            }
                                            arr[i] = arr[i].Replace(",", "," + Environment.NewLine + "\t");
                                            arr[i] = arr[i].Replace(")", Environment.NewLine + ")");
                                        }

                                        //if (arr[i].Trim().ToUpper() == "STABLE") isExistSTABLE = true;
                                        //if (arr[i].Trim().ToUpper() == "IMMUTABLE") isExistIMMUTABLE = true;
                                        //if (arr[i].Trim().ToUpper() == "RETURNS NULL ON NULL INPUT") isExistNULLINPUT = true;
                                        //if (arr[i].Trim().ToUpper() == "SECURITY DEFINER") isExistDEFINER = true;
                                        if (arr[i].Trim().ToUpper().StartsWith("COST ")) isExistCOST = true;
                                    }

                                    result.AddRange(arr);

                                    // добавляем модификаторы, если они не были добавлены ранее
                                    /*if ( 
                                        (_provolatile == "s") && 
                                        (!isExistSTABLE) 
                                        )
                                    {
                                        result.Add("STABLE");
                                    }
                                    
                                    if ( 
                                        (_provolatile == "i") && 
                                        (!isExistIMMUTABLE)
                                        )
                                    {
                                        result.Add("IMMUTABLE");
                                    }

                                    if ( 
                                        (_proisstrict == "2") && 
                                        (!isExistNULLINPUT)
                                        )
                                    {
                                        result.Add("RETURNS NULL ON NULL INPUT");
                                    }

                                    if (
                                        (_prosecdef == "2") &&
                                        (!isExistDEFINER)
                                        )
                                    {
                                        result.Add("SECURITY DEFINER");
                                    }
                                    */
                                    if (
                                        (!string.IsNullOrWhiteSpace(_procost)) &&
                                        (_procost != "100") &&
                                        (!isExistCOST)
                                        )
                                    {
                                        result.Add("COST " + _procost);
                                    }

                                    result.Add(";");
                                }
                            }
                        }
                    }
                }

                if ((_scripttype == "view") || (_scripttype == "materialized view"))
                {
                    // ищем и выгружаем текст представления PGSQL
                    queryString = @"
SELECT 
    array_to_string(ARRAY(
            select attr.attname as column_name
            from pg_catalog.pg_attribute as attr
            join pg_catalog.pg_class as cls on cls.oid = attr.attrelid
            join pg_catalog.pg_namespace as ns on ns.oid = cls.relnamespace
            where ns.nspname = n.nspname 
            and cls.relname = v.relname 
            and attr.attisdropped = false -- не удалено
            and attr.attnum > 0 -- без служебных полей
            order by attr.attnum
	    ),',') as columns,
	pg_catalog.obj_description(v.oid, 'pg_class') AS comments,
	pg_catalog.pg_get_viewdef(v.oid, true) AS query
FROM pg_catalog.pg_class v
LEFT JOIN pg_catalog.pg_namespace n ON (n.oid = v.relnamespace)
WHERE v.relkind  in ('v','m')
and v.relname ilike '" + _objectlike + @"'
and n.nspname ilike '" + _schemalike + @"' 
limit 1";

                    // если найдено точное соответствие
                    if (isExact) queryString = queryString.Replace("ilike", "=").Replace("\\_", "_");

                    result.Clear();

                    using (DbDataReader reader = this.OpenQuery(queryString))
                    {
                        if (reader != null)
                        {

                            while (reader.Read())
                            {
                                string _columns = reader[0].ToString();
                                string _comment = reader[1].ToString();
                                string viewtext = reader[2].ToString();

                                // исправляем форматирование вьюхи
                                if (!string.IsNullOrWhiteSpace(viewtext))
                                {

                                    if (!string.IsNullOrWhiteSpace(_columns))
                                    {
                                        _columns = _columns.Replace(" ,", ",");
                                        _columns = _columns.Replace(", ", ",");
                                        _columns = _columns.Replace(",", "," + Environment.NewLine + "\t");
                                    }

                                    if (_scripttype == "materialized view")
                                    {
                                        viewtext = "CREATE " + _scripttype.ToUpper() + " " + _schema + "." + _objectname + " (" +
                                                           Environment.NewLine + "\t" + _columns +
                                                           Environment.NewLine + ")" +
                                                           Environment.NewLine + "AS" +
                                                           Environment.NewLine + viewtext;
                                    }
                                    else
                                    {
                                        viewtext = "CREATE OR REPLACE " + _scripttype.ToUpper() + " " + _schema + "." + _objectname + " (" +
                                                           Environment.NewLine + "\t" + _columns +
                                                           Environment.NewLine + ")" +
                                                           Environment.NewLine + "AS" +
                                                           Environment.NewLine + viewtext;
                                    }

                                    if (!string.IsNullOrWhiteSpace(_comment))
                                    {
                                        viewtext += Environment.NewLine + //-V3086
                                        Environment.NewLine + "COMMENT ON " + _scripttype.ToUpper() + " " + _schema + "." + _objectname + " IS '" + _comment + "';";
                                    }

                                    viewtext = viewtext.Replace("\r\n", "\n");
                                    viewtext = viewtext.Replace("\r", "");

                                    var arr = viewtext.Split(new string[] { "\n" }, StringSplitOptions.None);

                                    for (int i = 0; i < arr.Length; i++)
                                    {

                                    }

                                    result.AddRange(arr);
                                }
                            }
                        }
                    }

                    if (_scripttype == "materialized view")
                    {
                        // дополнительно для мат.представлений выгружаем индекс
                        queryString = @"
SELECT 
	pg_catalog.PG_GET_INDEXDEF(v.oid) AS index
FROM pg_catalog.pg_class v
LEFT JOIN pg_catalog.pg_namespace n ON (n.oid = v.relnamespace)
WHERE v.relkind  in ('m')
and v.relname ilike '" + _objectlike + @"'
and n.nspname ilike '" + _schemalike + @"'";

                        // если найдено точное соответствие
                        if (isExact) queryString = queryString.Replace("ilike", "=").Replace("\\_", "_");

                        using (DbDataReader reader = this.OpenQuery(queryString))
                        {
                            if (reader != null)
                            {
                                while (reader.Read())
                                {
                                    string indextext = reader[0].ToString();

                                    indextext = Environment.NewLine + indextext;

                                    // исправляем форматирование вьюхи
                                    if (!string.IsNullOrWhiteSpace(indextext))
                                    {
                                        indextext = indextext.Replace("\r\n", "\n");
                                        indextext = indextext.Replace("\r", "");

                                        var arr = indextext.Split(new string[] { "\n" }, StringSplitOptions.None);

                                        for (int i = 0; i < arr.Length; i++)
                                        {

                                        }

                                        result.AddRange(arr);
                                    }
                                }
                            }
                        }
                    }
                }

                if (_scripttype == "table")
                {
                    // генерим текст скрипта для таблицы
                    TableDB table = new TableDB();

                    table.GITProject = project;
                    table.ScriptType = Utilities.ScriptType.CREATE;
                    table.TableEdit.SchemaName = _schema;
                    table.TableEdit.TableName = _objectname;
                    table.TableType = Utilities.TableType.DICT;

                    bool isfound = this.FillTableInfo(table.TableEdit);
                    if (isfound)
                    {
                        try
                        {
                            table.TableType = this.GetTableType(table.TableEdit);
                        }
                        catch (Exception)
                        {
                            table.TableType = Utilities.TableType.DICT;
                        }

                        try
                        {
                            this.FillListIndex(table, "");
                        }
                        catch (Exception)
                        {
                        }

                        isfound = this.FillListField(table.TableEdit);

                        if (isfound)
                        {
                            List<string> ProcCommand = new List<string>();
                            int ProcCommandNum = -1;
                            table.isOnlyExist = true;
                            string tabletext = TableDB.GenerateTableScript(this, false, isAddRegion, txtRegion, out ProcCommand, out ProcCommandNum, false, out string RowInfo, table);
                            var arr = tabletext.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                            result.AddRange(arr);
                        }
                    }

                }

                if (_scripttype == "index")
                {
                    // генерим текст скрипта для индекса
                    TableDB table = new TableDB();

                    table.GITProject = project;
                    table.ScriptType = Utilities.ScriptType.CREATE;
                    table.TableEdit.SchemaName = _schema;
                    table.TableEdit.TableName = _for_tablename;
                    table.TableType = Utilities.TableType.DICT;

                    bool isfound = this.FillListIndex(table, _objectseek);
                    if (isfound)
                    {
                        foreach (var item in table.ListIndex)
                        {
                            string indextext = TableDB.GenerateIndexScript(Utilities.ScriptType.CREATE, false, isIndexCreate, isAddRegion, txtRegion, item);
                            var arr = indextext.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                            result.AddRange(arr);
                        }
                    }
                }

                if (_scripttype == "sequence")
                {
                    // ищем информацию по сиквенсу
                    queryString = @"
SELECT 
    start_value,
    increment_by,
    min_value,
    max_value,
    cache_size
FROM pg_sequences 
where sequencename ilike '" + _objectlike + @"'
and schemaname ilike '" + _schemalike + @"' 
limit 1";

                    // если найдено точное соответствие
                    if (isExact) queryString = queryString.Replace("ilike", "=").Replace("\\_", "_");

                    result.Clear();

                    using (DbDataReader reader = this.OpenQuery(queryString))
                    {
                        if (reader != null)
                        {

                            while (reader.Read())
                            {
                                string _start_value = reader[0].ToString();
                                string _increment_by = reader[1].ToString();
                                string _minvalue = reader[2].ToString();
                                string _maxvalue = reader[3].ToString();
                                string _cache = reader[4].ToString();

                                string seqtext =
                                    "CREATE SEQUENCE IF NOT EXISTS " + _schema + "." + _objectname + Environment.NewLine +
                                    "\tSTART WITH " + _start_value + Environment.NewLine +
                                    "\tINCREMENT BY " + _increment_by + Environment.NewLine +
                                    "\tMINVALUE " + _minvalue + Environment.NewLine +
                                    "\tMAXVALUE " + _maxvalue + Environment.NewLine +
                                    "\tCACHE " + _cache + ";";

                                seqtext = seqtext.Replace("\r\n", "\n");
                                seqtext = seqtext.Replace("\r", "");

                                var arr = seqtext.Split(new string[] { "\n" }, StringSplitOptions.None);

                                for (int i = 0; i < arr.Length; i++)
                                {

                                }

                                result.AddRange(arr);
                            }
                        }
                    }

                }

                if (_scripttype == "trigger")
                {
                    // ищем и выгружаем текст триггера PGSQL
                    queryString = @"
select 
    pg_get_triggerdef (trg.oid)
from pg_trigger trg
inner join pg_class t on t.oid = trg.tgrelid
inner JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
where t.relkind in ('r','f','p')
and n.nspname ilike '" + _schemalike + @"'
and trg.tgname ilike '" + _objectlike + @"'";

                    if (!string.IsNullOrWhiteSpace(OID))
                    {
                        queryString += Environment.NewLine + "and trg.oid = " + OID;
                    }

                    queryString += @"
limit 1";

                    // если найдено точное соответствие
                    if (isExact) queryString = queryString.Replace("ilike", "=").Replace("\\_", "_");

                    result.Clear();

                    using (DbDataReader reader = this.OpenQuery(queryString))
                    {
                        if (reader != null)
                        {

                            while (reader.Read())
                            {
                                string triggertext = reader[0].ToString().Trim().TrimEnd(';') + ";";

                                // исправляем форматирование 
                                if (!string.IsNullOrWhiteSpace(triggertext))
                                {
                                    triggertext = triggertext.Replace(" BEFORE ", Environment.NewLine + "\tBEFORE ");
                                    triggertext = triggertext.Replace(" AFTER ", Environment.NewLine + "\tAFTER ");
                                    triggertext = triggertext.Replace(" INSTEAD ", Environment.NewLine + "\tINSTEAD ");
                                    triggertext = triggertext.Replace(" ON ", Environment.NewLine + "\tON ");
                                    triggertext = triggertext.Replace(" FROM ", Environment.NewLine + "\tFROM ");
                                    triggertext = triggertext.Replace(" FOR ", Environment.NewLine + "\tFOR ");
                                    triggertext = triggertext.Replace(" WHEN ", Environment.NewLine + "\tWHEN ");
                                    triggertext = triggertext.Replace(" EXECUTE ", Environment.NewLine + "\tEXECUTE ");

                                    triggertext = triggertext.Replace("\r\n", "\n");
                                    triggertext = triggertext.Replace("\r", "");

                                    var arr = triggertext.Split(new string[] { "\n" }, StringSplitOptions.None);

                                    for (int i = 0; i < arr.Length; i++)
                                    {

                                    }

                                    result.AddRange(arr);
                                }
                            }
                        }
                    }


                }

            }

            return result;
        }


        /// <summary>Вернуть текст тестового changeset</summary>
        /// <param name="_schemaseek">схема - для поиска в БД</param>
        /// <param name="_objectseek">имя объекта - для поиска в БД</param>
        /// <param name="isAddRegion">true - добавляем проверку региональности</param>
        /// <param name="txtRegion">номер региона</param>
        public string GetTestChangeset(string _schemaseek, string _objectseek, bool isAddRegion, string txtRegion)
        {
            string result = "";

            string name_lower = _objectseek.ToLower();
            string comment = "--";

            if (name_lower.StartsWith("p_"))
            {
                if (
                    name_lower.EndsWith("_ins") ||
                    name_lower.EndsWith("_upd") ||
                    name_lower.EndsWith("_del") ||
                    name_lower.EndsWith("_set")
                )
                {
                    _objectseek = _objectseek.Substring(2, name_lower.Length - 6);
                    comment = "";
                }

                if (name_lower.EndsWith("_setdel"))
                {
                    _objectseek = _objectseek.Substring(2, name_lower.Length - 9);
                    comment = "";
                }

                if (name_lower.EndsWith("_setdelafter"))
                {
                    _objectseek = _objectseek.Substring(2, name_lower.Length - 14);
                    comment = "";
                }
            }

            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                // определяем тип
                string queryString = @"
declare @res varchar(max)
IF OBJECT_ID(N'dbo.xp_GenScriptTest', N'P') IS NOT NULL
exec dbo.xp_GenScriptTest @TableName='" + _schemaseek + "." + _objectseek + @"', @res = @res output
select @res
";
                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            result = reader[0].ToString();
                            if (result == null) result = "";
                            result = result.Replace(Environment.NewLine,"\n");

                            bool isFound = false;

                            if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion))
                            {
                                List<string> lines = result.ToList(new char[] { '\n' }, false);

                                for (int i = 0; i < lines.Count(); i++)
                                {
                                    if (lines[i].Trim().StartsWith("exec"))
                                    {
                                        string shift = "";

                                        if (lines[i].IndexOf("exec") > 0)
                                        {
                                            shift = lines[i].Substring(0, lines[i].IndexOf("exec"));
                                        }

                                        lines[i] = lines[i].Replace("exec",
                                            comment + "IF (dbo.getregion() = " + txtRegion + " OR db_name() IN ('ProMedWebRelease', 'ProMedTest', 'promeddev'))" + Environment.NewLine +
                                            shift + comment + "AND EXISTS (SELECT 1 FROM sys.schemas s WITH (nolock) WHERE s.name = '" + _schemaseek + "')" + Environment.NewLine +
                                            shift + comment + "BEGIN" + Environment.NewLine +
                                            shift + comment + "\texec"
                                            ) + Environment.NewLine +
                                            shift + comment + "END";

                                        isFound = true;
                                    }
                                }

                                if (isFound)
                                {
                                    result = string.Join("\n", lines.ToArray());
                                }
                            }
                            else
                            {
                                if (!string.IsNullOrWhiteSpace(comment))
                                {
                                    result = result.Replace("exec", comment + "exec");
                                }
                            }

                            break; //-V3020
                        }
                    }
                }
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                comment = "";

                var ListResults = this.GetObjectOIDList("dbo", "xp_genscripttest");
                if (ListResults.Count == 0)
                {
                    return result;
                }

                string queryString = "select xp_genscripttest from dbo.xp_genscripttest('" + _schemaseek + "." + _objectseek + "');";

                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            result = reader[0].ToString();
                            if (result == null) result = "";
                            result = result.Replace(Environment.NewLine, "\n");

                            bool isFound = false;

                            if (isAddRegion && !string.IsNullOrWhiteSpace(txtRegion))
                            {
                                List<string> lines = result.ToList(new char[] { '\n' }, false);

                                for (int i = 0; i < lines.Count(); i++)
                                {
                                    if (lines[i].Trim().StartsWith("perform"))
                                    {
                                        string shift = "";

                                        if (lines[i].IndexOf("perform") > 0)
                                        {
                                            shift = lines[i].Substring(0, lines[i].IndexOf("perform"));
                                        }

                                        lines[i] = lines[i].Replace("perform",
                                            comment + "IF (dbo.getregion() = " + txtRegion + " OR current_database() IN ('promedrelease', 'promedadygea', 'promedtest'))" + Environment.NewLine +
                                            shift + comment + "AND EXISTS (SELECT 1 FROM pg_namespace WHERE nspname = '" + _schemaseek + "')" + Environment.NewLine +
                                            shift + comment + "THEN" + Environment.NewLine +
                                            shift + comment + "\tperform"
                                            ) + Environment.NewLine +
                                            shift + comment + "END IF;";

                                        isFound = true;
                                    }
                                }

                                if (isFound)
                                {
                                    result = string.Join("\n", lines.ToArray());
                                }
                            }

                            break; //-V3020
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>Вернуть список DataTable - Информация о справочнике из nsi.RefTableRegistry</summary>
        /// <param name="tablename">Имя таблицы</param>
        public DataTable GetRefTableRegistry(string tablename)
        {
            tablename = Utilities.Databases.GetFullTableName(tablename);

            string queryString = @"with maxid as (
select
	coalesce((select max(RefTableRegistry_id) from nsi.RefTableRegistry where RefTableRegistry_id < 10000000),0) as RefTableRegistry_maxid,
	coalesce((select max(RefTableRegistryVersion_id) from nsi.RefTableRegistryVersion where RefTableRegistryVersion_id < 10000000),0) as RefTableRegistryVersion_maxid
)
select 
maxid.RefTableRegistry_maxid,
maxid.RefTableRegistryVersion_maxid,
t.RefTableRegistry_id,
t.RefTableRegistry_Oid,
t.RefTableRegistry_createDT,
t.RefTableRegistry_FullName,
t.RefTableRegistry_Nick
from maxid
left join nsi.RefTableRegistry t on lower(t.RefTableRegistry_SysNick) = lower('" + tablename + "');";

            return this.FillDataTable(queryString, out string Messages);
        }

        /// <summary>Вернуть список DataTable - Информация о справочнике из stg.LocalDBList</summary>
        /// <param name="module">Имя модуля</param>
        /// <param name="tables">Список таблиц</param>
        /// <param name="queryString">Текст запроса</param>
        /// <param name="Messages">Сообщения и предупреждения после выполнения</param>
        public DataTable GetLocalDBList(string module, List<string> tables, out string queryString, out string Messages)
        {
            queryString = @"select 
    l.LocalDbList_id, 
    l.LocalDbList_name, 
    l.LocalDbList_prefix, 
    l.LocalDbList_nick, 
    l.LocalDbList_schema, 
    l.LocalDbList_key, 
    l.LocalDbList_module, 
    l.LocalDbList_Descr, 
    r.RegionalLocalDbList_id, 
    r.Region_id, 
    r.RegionalLocalDbList_Sql";
            if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString += @",
    r.RegionalLocalDbList_PgSql";
            }

            queryString += @"
from stg.localdblist l
left join stg.regionallocaldblist r on r.localdblist_id = l.localdblist_id
where lower(l.LocalDbList_module) like lower('" + module + @"')
and (";
            bool isFirst = true;
            string order = "";
            foreach (var item in tables)
            {
                string schemaname = Utilities.Databases.GetSchemaName(item);
                string tablename = Utilities.Databases.GetTableName(item);

                if (isFirst)
                {
                    queryString += @"
   ";
                    order += @"
   ";
                    isFirst = false;
                }
                else
                {
                    queryString += @"
or ";
                    order += @"
or ";
                }

                queryString += @"(lower(l.localdblist_schema) like lower('" + schemaname + @"') and lower(l.localdblist_nick) like lower('" + tablename + @"'))
or (lower(l.localdblist_schema) like lower('" + schemaname + @"') and lower(l.localdblist_nick) like lower('v_" + tablename + @"'))
or (lower(l.localdblist_schema) like lower('" + schemaname + @"') and lower(l.localdblist_name) like lower('" + tablename + @"'))
or (lower(l.localdblist_schema) like lower('" + schemaname + @"') and lower(l.localdblist_name) like lower('" + schemaname + "_" + tablename + @"'))
or (lower(l.localdblist_schema) like lower('" + schemaname + @"') and lower(l.localdblist_name) like lower('" + schemaname + tablename + @"'))
or (lower(l.localdblist_schema) like lower('" + schemaname + @"') and lower(l.localdblist_name) like lower('" + tablename + schemaname + @"'))";

                order += @"(lower(l.localdblist_schema) like 'dbo' and lower(l.localdblist_name) like lower('" + tablename + @"'))
or (lower(l.localdblist_schema) like lower('" + schemaname + @"') and lower(l.localdblist_name) like lower('" + schemaname + "_" + tablename + @"'))";

            }

            queryString += @"
)
order by
case when " + order + @"
then 0 else 1 end,
l.LocalDbList_name";

            Messages = "";

            return this.FillDataTable(queryString, out Messages);
        }


        /// <summary>Вернуть RegionalLocalDbList_PgSql</summary>
        /// <param name="module">Имя модуля</param>
        /// <param name="schemaname">Имя схемы</param>
        /// <param name="name">Имя справочника</param>
        public string GetRegionalLocalDbList_PgSql(string module, string schemaname, string name)
        {
            string queryString = "";

            if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"select r.RegionalLocalDbList_PgSql
from stg.localdblist l
left join stg.regionallocaldblist r on r.localdblist_id = l.localdblist_id
where l.LocalDbList_module ilike '" + module + "' and l.localdblist_schema ilike '" + schemaname + "' and l.localdblist_name ilike '" + name + @"' 
limit 1";
            }

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            string s = reader[0].ToString();

                            return s //-V3020
                                .Replace("\r\n", "\n")
                                .Replace("\n", "\r\n");
                        }
                    }
                }
            }

            return "";
        }

        /// <summary>Вернуть данные из БД о списке маркеров</summary>
        /// <param name="ListFreeDocMarker">список маркеров</param>
        /// <param name="ListFreeDocRelationship">список связей маркеров</param>
        /// <param name="ListEvnClass">список EvnClass</param>
        public void FillFreeDocMarkers(ref List<FreeDocMarker> ListFreeDocMarker, ref List<FreeDocRelationship> ListFreeDocRelationship, ref List<EvnClass> ListEvnClass)
        {
            if (ListFreeDocMarker == null) ListFreeDocMarker = new List<FreeDocMarker>();
            if (ListFreeDocRelationship == null) ListFreeDocRelationship = new List<FreeDocRelationship>();
            if (ListEvnClass == null) ListEvnClass = new List<EvnClass>();

            string queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                queryString = @"select 
m.FreeDocMarker_id, 
c.EvnClass_SysNick,
m.FreeDocMarker_Name,
m.FreeDocMarker_TableAlias,
m.FreeDocMarker_Field,
m.FreeDocMarker_Query,
m.FreeDocMarker_Description,
m.FreeDocMarker_IsTableValue,
m.FreeDocMarker_Options
from dbo.v_FreeDocMarker m with (nolock) 
inner join dbo.v_evnclass c with (nolock) on m.evnclass_id = c.evnclass_id
where 1=1";
                queryString += " order by m.FreeDocMarker_Name";
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"select 
m.FreeDocMarker_id, 
c.EvnClass_SysNick,
m.FreeDocMarker_Name,
m.FreeDocMarker_TableAlias,
m.FreeDocMarker_Field,
m.FreeDocMarker_Query,
m.FreeDocMarker_Description,
m.FreeDocMarker_IsTableValue,
m.FreeDocMarker_Options
from dbo.v_FreeDocMarker m
inner join dbo.v_evnclass c on m.evnclass_id = c.evnclass_id
where 1=1";
                queryString += " order by m.FreeDocMarker_Name";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    ListFreeDocMarker.Clear();
                    while (reader.Read())
                    {
                        ListFreeDocMarker.Add(new FreeDocMarker(
                            reader[0].ToString(),
                            reader[1].ToString(),
                            reader[2].ToString(),
                            reader[3].ToString(),
                            reader[4].ToString(),
                            reader[5].ToString(),
                            reader[6].ToString(),
                            reader[7].ToString(),
                            reader[8].ToString()
                        ));
                    }
                }
            }

            queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
                queryString = @"select 
r.FreeDocRelationship_id, 
c.EvnClass_SysNick,
r.FreeDocRelationship_AliasName,
r.FreeDocRelationship_AliasTable,
r.FreeDocRelationship_AliasQuery,
r.FreeDocRelationship_LinkedAlias,
r.FreeDocRelationship_LinkDescription
from dbo.FreeDocRelationship r with (nolock)
inner join dbo.v_evnclass c with (nolock) on r.evnclass_id = c.evnclass_id";
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"select 
r.FreeDocRelationship_id, 
c.EvnClass_SysNick,
r.FreeDocRelationship_AliasName,
r.FreeDocRelationship_AliasTable,
r.FreeDocRelationship_AliasQuery,
r.FreeDocRelationship_LinkedAlias,
r.FreeDocRelationship_LinkDescription
from dbo.FreeDocRelationship r
inner join dbo.v_evnclass c on r.evnclass_id = c.evnclass_id";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    ListFreeDocRelationship.Clear();
                    while (reader.Read())
                    {
                        ListFreeDocRelationship.Add(new FreeDocRelationship(
                            reader[0].ToString(),
                            reader[1].ToString(),
                            reader[2].ToString(),
                            reader[3].ToString(),
                            reader[4].ToString(),
                            reader[5].ToString(),
                            reader[6].ToString()
                        ));
                    }
                }
            }

            queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
                queryString = @"select 
c.evnclass_sysnick,
p.evnclass_sysnick as parent_evnclass_sysnick
from evnclass c with (nolock)
left join evnclass p with (nolock) on p.evnclass_id = c.evnclass_pid";
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"select 
c.evnclass_sysnick,
p.evnclass_sysnick as parent_evnclass_sysnick
from evnclass c
left join evnclass p on p.evnclass_id = c.evnclass_pid";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    ListEvnClass.Clear();
                    while (reader.Read())
                    {
                        ListEvnClass.Add(new EvnClass { SysNick = reader[0].ToString(), ParentSysNick = reader[1].ToString() });
                    }
                }
            }
        }

        /// <summary>Заполнить информацию о списке полей, унаследованных в evnTable из evnParent</summary>
        /// <param name="evnTable">Таблица из БД </param>
        /// <param name="evnParent">Таблица-предок из БД </param>
        /// <param name="hasInherit">=true таблица создана через наследование</param>
        public List<ParentFieldInfo> FillListEvnParentFields(string evnTable, string evnParent, bool hasInherit)
        {
            List<ParentFieldInfo> result = new List<ParentFieldInfo>();

            string queryString = "";

            // поля в текущей таблице evnTable, унаследованные из evnParent
            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                // для МС информацию берем из родительских таблиц
                queryString = @"
DECLARE @EvnClass TABLE (EvnClass_id BIGINT, EvnClass_pid BIGINT, evnclass_sysnick VARCHAR(max))

IF OBJECT_ID('dbo.EvnClass', 'U') IS NOT NULL
BEGIN
    insert into @EvnClass (EvnClass_id, EvnClass_pid, evnclass_sysnick)
    select l.EvnClass_id, l.EvnClass_pid, l.evnclass_sysnick
    from dbo.EvnClass l with (nolock)
    where not exists (select 1 from @EvnClass t where t.EvnClass_id = l.EvnClass_id)
END;

WITH r AS (
    select 
        s.*,
        0 as _level
    from @evnclass s
    where s.evnclass_sysnick = '" + Utilities.Databases.GetTableName(evnParent) + @"'
    union all
    select 
        s.*,
        _level + 1 as _level
    from @evnclass s
    inner join r on r.evnclass_pid = s.evnclass_id
)
Select
    c.name as field_name,
    ep.value as field_desc
from r
inner join sys.tables t with (nolock) on t.schema_id = schema_id('" + Utilities.Databases.GetSchemaName(evnParent) + @"') and t.name = r.evnclass_sysnick
inner join sys.columns c with (nolock) on c.object_id = t.object_id
left join sys.extended_properties ep with (nolock) on ep.major_id = t.object_id and ep.minor_id = c.column_id
where (
        c.name not in (select r.evnclass_sysnick + '_id' from r) 
        or (t.name = 'Evn' and c.name = 'Evn_id')
    )
order by r._level desc, c.column_id";
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                if (hasInherit)
                {
                    // для ПГ - если таблица событий создана как наследник (есть запись в pg_inherits), первоначально мы эту информацию берем из текущей таблицы (из унаследованных полей)
                    queryString = @"
Select
    c.attname as field_name,
    col_description(t.oid, c.attnum) as field_desc
FROM pg_class t
INNER JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
INNER JOIN pg_catalog.pg_attribute c ON c.attrelid = t.oid 
  AND c.attnum > 0 -- без служебных полей
  AND c.attisdropped = false -- не удалено
  AND c.attinhcount > 0 -- унаследовано
WHERE n.nspname iLIKE '" + Utilities.Databases.GetSchemaName(evnTable) + @"'
AND t.relname iLIKE '" + Utilities.Databases.GetTableName(evnTable) + @"'
and t.relkind in ('r','f','p')
order by c.attnum
";
                }
                else
                {
                    // для ПГ - если таблица событий создана без наследования, первоначально мы эту информацию берем из родительских таблиц
                    queryString = @"
CREATE TEMP TABLE IF NOT EXISTS tmp_evnclass(evnclass_id bigint, evnclass_pid bigint, evnclass_sysnick varchar);

DO $$ BEGIN
IF to_regclass('dbo.evnclass') IS NOT NULL THEN
    INSERT INTO tmp_evnclass(evnclass_id, evnclass_pid, evnclass_sysnick)
    SELECT l.evnclass_id, l.evnclass_pid, l.evnclass_sysnick
    FROM dbo.evnclass l
    where not exists (select 1 from tmp_evnclass t where t.EvnClass_id = l.EvnClass_id);
END IF;
END; $$;

WITH recursive r AS(
    select
        s.*,
        0 as _level
    from tmp_evnclass s
    where s.evnclass_sysnick ilike '" + Utilities.Databases.GetTableName(evnParent) + @"'
    union
    select
        s.*,
        _level + 1 as _level
    from tmp_evnclass s
    inner
    join r on r.evnclass_pid = s.evnclass_id
)
Select
    c.attname as field_name,
    col_description(to_regclass(quote_ident(n.nspname) || '.' || quote_ident(t.relname)), c.attnum) as field_desc
from r, pg_catalog.pg_class t
inner join pg_catalog.pg_namespace n ON n.oid = t.relnamespace
inner join lateral (
        select a.attnum, a.attname
    FROM pg_attribute a
    WHERE a.attrelid = to_regclass(quote_ident(n.nspname) || '.' || quote_ident(t.relname))
    AND a.attisdropped = false -- не удалено
    AND a.attinhcount = 0 -- не унаследовано
    AND a.attnum > 0 -- без служебных полей
) c on true
where n.nspname ilike '" + Utilities.Databases.GetSchemaName(evnParent) + @"'
and t.relname ilike r.evnclass_sysnick
and t.relkind = 'r'
and (
        c.attname not ilike all (select r.evnclass_sysnick || '_id' from r)
        or (t.relname = 'evn' and c.attname = 'evn_id')
    )
order by r._level desc, c.attnum";
                }
            }

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            result.Add(new ParentFieldInfo() { 
                                isUpdatedFromParent = false, 
                                Name = reader[0].ToString(), 
                                Desc = reader[1].ToString() 
                            });
                        }
                    }
                }
            }

            if (
                (!string.IsNullOrWhiteSpace(evnParent)) && 
                (this.ConnType == Utilities.ConnType.PGSQL) && 
                hasInherit
            )
            {
                // для ПГ - дополняем данными из родительских таблиц, от которых создана текущая таблица
                queryString = @"
WITH RECURSIVE r AS (

    with all_inherits as (
        select distinct inhrelid, inhparent from pg_inherits
        union all
        select distinct inh.inhparent as inhrelid, null::oid as inhparent 
        from pg_inherits inh 
        where not exists (select 1 from pg_inherits l where l.inhrelid = inh.inhparent)
    )

	select 
		inh.inhrelid, 
		inh.inhparent,
		0 as inhlevel
	from all_inherits inh
    where inh.inhrelid = to_regclass('" + evnParent + @"')
    union
	select 
		inh.inhrelid, 
		inh.inhparent,
		inhlevel - 1 as inhlevel
	from all_inherits inh
	inner join r on inh.inhrelid = r.inhparent
)
Select 
	a.attname as field_name,
    col_description(a.attrelid, a.attnum) as field_desc
from r
inner join pg_attribute a on a.attrelid = r.inhrelid
	AND a.attisdropped = false -- не удалено
    AND a.attnum > 0 -- без служебных полей
order by r.inhlevel desc, a.attnum
";

                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            string _name = reader[0].ToString();
                            string _desc = reader[1].ToString();

                            if (!string.IsNullOrWhiteSpace(_desc))
                            {
                                var found = result
                                        .Where(x =>
                                            x.Name.ToLower() == _name.ToLower()
                                        )
                                        .FirstOrDefault();

                                if (found == null)
                                {
                                    result.Add(new ParentFieldInfo() { isUpdatedFromParent = true, Name = _name, Desc = _desc });
                                }
                                else
                                {
                                    if (string.IsNullOrWhiteSpace(found.Desc))
                                    {
                                        found.Desc = _desc;
                                        found.isUpdatedFromParent = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>Заполнить информацию о списке FK таблицы</summary>
        /// <param name="Table">Таблица из БД </param>
        public List<TableFKInfo> FillListTableFKs(string Table)
        {
            List<TableFKInfo> result = new List<TableFKInfo>();

            string queryString = "";
            string schema = Utilities.Databases.GetSchemaName(Table);
            string tablename = Utilities.Databases.GetTableName(Table);

            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                queryString = @"
Select
    c.name as FieldName,
	fk.name as FKName,
	schema_name(rt.schema_id) + '.' + rt.name as FKTable,
	fkc.constraint_column_id as FKOrder,
    rc.name as FKField
from sys.tables t with (nolock) 
inner join sys.columns c with (nolock) on c.object_id = t.object_id
inner join sys.foreign_key_columns fkc with (nolock) on fkc.parent_object_id = c.object_id and fkc.parent_column_id = c.column_id
inner join sys.foreign_keys fk with (nolock) on fk.object_id = fkc.constraint_object_id
inner join sys.tables rt with (nolock) on rt.object_id = fkc.referenced_object_id
inner join sys.columns rc with (nolock) on rc.object_id = fkc.referenced_object_id and rc.column_id = fkc.referenced_column_id 
where t.schema_id = schema_id('" + schema + @"') 
and t.name = '" + tablename + @"'
order by c.column_id, FKName, FKOrder";
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"
SELECT
  c.FieldName,
  o.conname AS FKName,
  fs.nspname || '.' || f.relname AS FKTable,
  fc.FKOrder,
  fc.FKField
FROM pg_constraint o 
INNER JOIN pg_class m ON m.oid = o.conrelid AND m.relkind in ('r','f','p')
INNER JOIN pg_namespace ms ON ms.oid = m.relnamespace
INNER JOIN pg_class f ON f.oid = o.confrelid 
INNER JOIN pg_namespace fs ON fs.oid = f.relnamespace
-- поля внешнего ключа в основной таблице
INNER JOIN LATERAL (
  SELECT
    array_position(o.conkey, a.attnum) as FKOrder,
    a.attname as FieldName
  FROM pg_attribute a 
  WHERE a.attrelid = m.oid 
  AND a.attnum = ANY(o.conkey)
  AND a.attisdropped = false -- не удалено
) c on true
-- поля primary key в таблице внешнего ключа
INNER JOIN LATERAL (
  SELECT
    array_position(o.confkey, a.attnum) as FKOrder,
    a.attname as FKField
  FROM pg_attribute a 
  WHERE a.attrelid = f.oid 
  AND a.attnum = ANY(o.confkey)
  AND a.attisdropped = false -- не удалено
) fc on fc.FKOrder = c.FKOrder
WHERE o.contype = 'f' 
AND ms.nspname iLIKE '" + schema + @"'
AND m.relname iLIKE '" + tablename + @"'
order by c.attnum, o.conname, fc.FKOrder
";
            }

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {
                            string _FieldName = reader[0].ToString();
                            string _FKName = reader[1].ToString();
                            string _FKTable = reader[2].ToString();
                            string _FKOrder = reader[3].ToString();
                            string _FKField = reader[4].ToString();

                            var fk = result.Where(x =>
                                (x.FKName.ToLower() == _FKName.ToLower()) &&
                                (x.FKField.ToLower() == _FKField.ToLower())
                            ).FirstOrDefault();

                            if (fk == null)
                            {
                                result.Add(new TableFKInfo()
                                {
                                    FieldName = _FieldName,
                                    FKName = _FKName,
                                    FKTable = _FKTable,
                                    FKOrder = _FKOrder,
                                    FKField = _FKField
                                });
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>Заполнить информацию о списке FK родительских таблиц</summary>
        /// <param name="isOnlyExist">Генерировать скрипт по существующим данным таблицы, не пытаться ее "улучшить"</param>
        /// <param name="evnTable">Таблица из БД </param>
        /// <param name="evnParent">Таблица-предок из БД </param>
        /// <param name="scriptType">тип генерации скрипта </param>
        public List<TableFKInfo> FillListEvnParentFKs(bool isOnlyExist, string evnTable, string evnParent, Utilities.ScriptType scriptType)
        {
            List<TableFKInfo> result = new List<TableFKInfo>();

            string queryString = "";
            string tablename = Utilities.Databases.GetTableName(evnTable);
            string schema = Utilities.Databases.GetSchemaName(evnTable);


            // поля в текущей таблице evnTable, унаследованные из evnParent
            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                // для МС информацию берем из родительских таблиц
                queryString = @"
DECLARE @EvnClass TABLE (EvnClass_id BIGINT, EvnClass_pid BIGINT, evnclass_sysnick VARCHAR(max))

IF OBJECT_ID('dbo.EvnClass', 'U') IS NOT NULL
BEGIN
    insert into @EvnClass (EvnClass_id, EvnClass_pid, evnclass_sysnick)
    select l.EvnClass_id, l.EvnClass_pid, l.evnclass_sysnick
    from dbo.EvnClass l with (nolock)
    where not exists (select 1 from @EvnClass t where t.EvnClass_id = l.EvnClass_id)
END;

WITH r AS (
    select 
        s.*,
        0 as _level
    from @evnclass s
    where s.evnclass_sysnick = '" + tablename + @"'
    union all
    select 
        s.*,
        _level + 1 as _level
    from @evnclass s
    inner join r on r.evnclass_pid = s.evnclass_id
)
Select
    c.name as FieldName,
    case 
        when fk.name like 'fk[_]' + t.name + '[_]%' 
            then 'fk_" + tablename + @"_' + substring(fk.name,CHARINDEX('_',fk.name,4)+1,len(fk.name))
        else 'fk_" + tablename + @"_' + c.name
	end as FKName,
    schema_name(rt.schema_id) + '.' + rt.name as FKTable,
    fkc.constraint_column_id as FKOrder,
    rc.name as FKField
from r
inner join sys.tables t with (nolock) on t.schema_id = schema_id('" + schema + @"') and t.name = r.evnclass_sysnick
inner join sys.columns c with (nolock) on c.object_id = t.object_id
inner join sys.foreign_key_columns fkc with (nolock) on fkc.parent_object_id = c.object_id and fkc.parent_column_id = c.column_id --and c.name <> 'server_id'
inner join sys.foreign_keys fk with (nolock) on fk.object_id = fkc.constraint_object_id
inner join sys.tables rt with (nolock) on rt.object_id = fkc.referenced_object_id
inner join sys.columns rc with (nolock) on rc.object_id = fkc.referenced_object_id and rc.column_id = fkc.referenced_column_id 
where r._level <> 0
and rt.object_id <> OBJECT_ID('dbo.evn', 'U') -- не создаем fk на dbo.evn!
order by r._level desc, c.column_id, FKName, FKOrder";
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                // для ПГ 
                if (isOnlyExist)
                {
                    // берем существующие констрейны для унаследованных полей
                    queryString = @"
SELECT
  c.FieldName,
  o.conname AS FKName,
  fs.nspname || '.' || f.relname AS FKTable,
  fc.FKOrder,
  fc.FKField
FROM pg_constraint o 
INNER JOIN pg_class m ON m.oid = o.conrelid AND m.relkind in ('r','f','p')
INNER JOIN pg_namespace ms ON ms.oid = m.relnamespace
INNER JOIN pg_class f ON f.oid = o.confrelid 
INNER JOIN pg_namespace fs ON fs.oid = f.relnamespace
-- поля внешнего ключа в основной таблице
INNER JOIN LATERAL (
  SELECT
    a.attnum,
    array_position(o.conkey, a.attnum) as FKOrder,
    a.attname as FieldName
  FROM pg_attribute a 
  WHERE a.attrelid = m.oid 
  AND a.attnum = ANY(o.conkey)
  AND a.attisdropped = false -- не удалено
  AND a.attinhcount > 0 -- унаследовано
) c on true
-- поля primary key в таблице внешнего ключа
INNER JOIN LATERAL (
  SELECT
    array_position(o.confkey, a.attnum) as FKOrder,
    a.attname as FKField
  FROM pg_attribute a 
  WHERE a.attrelid = f.oid 
  AND a.attnum = ANY(o.confkey)
  AND a.attisdropped = false -- не удалено
) fc on fc.FKOrder = c.FKOrder
WHERE o.contype = 'f' 
AND ms.nspname iLIKE '" + schema + @"'
AND m.relname iLIKE '" + tablename + @"'
and not (fs.nspname = 'dbo' and f.relname = 'evn') -- не создаем fk на dbo.evn!
order by c.attnum, FKName, FKOrder
";
                }
                else
                {
                    // берем констрейны родительских таблиц
                    if ((scriptType == Utilities.ScriptType.ALTER) || isOnlyExist) //-V3063
                    {
                        queryString = @"
WITH RECURSIVE r AS (

    with all_inherits as (
        select distinct inhrelid, inhparent from pg_inherits
        union all
        select distinct inh.inhparent as inhrelid, null::oid as inhparent 
        from pg_inherits inh 
        where not exists (select 1 from pg_inherits l where l.inhrelid = inh.inhparent)
    )

	select 
		inh.inhrelid, 
		inh.inhparent,
		0 as inhlevel
	from all_inherits inh
    where inh.inhrelid = to_regclass('" + evnTable + "')";
                    }
                    else
                    {
                        if (evnParent.ToLower() == "dbo.evn")
                        {
                            queryString = @"
WITH RECURSIVE r AS (
	select 
		m.oid as inhrelid, 
		null::oid as inhparent,
		0 as inhlevel
	from pg_class m
    where m.oid = to_regclass('" + evnParent + "')";
                        }
                        else
                        {
                            queryString = @"
WITH RECURSIVE r AS (
	select 
		inh.inhrelid, 
		inh.inhparent,
		0 as inhlevel
	from pg_inherits inh
    where inh.inhrelid = to_regclass('" + evnParent + "')";
                        }
                    }

                    queryString += @"
    union
	select 
		inh.inhrelid, 
		inh.inhparent,
		inhlevel - 1 as inhlevel
	from pg_inherits inh
	inner join r on inh.inhrelid = r.inhparent
),
fkcons as (
    SELECT
        o.conrelid,
		o.confrelid,
		o.conname AS constraint_name,
		ms.nspname AS source_schema,
		m.relname AS source_table,
        c.attnum AS source_position,
		c.attname AS source_column,
		fs.nspname AS target_schema,
		f.relname AS target_table,
		fc.FKOrder,
		fc.attname AS target_column
	FROM pg_constraint o 
	INNER JOIN pg_class m ON m.oid = o.conrelid AND m.relkind in ('r','f','p')
	INNER JOIN pg_namespace ms ON ms.oid = m.relnamespace
	INNER JOIN pg_class f ON f.oid = o.confrelid 
	INNER JOIN pg_namespace fs ON fs.oid = f.relnamespace
	-- поля внешнего ключа в основной таблице
	INNER JOIN LATERAL (
		SELECT 
			a.attnum,
			array_position(o.conkey, a.attnum) as FKOrder,
			a.attname
		FROM pg_attribute a 
		WHERE a.attrelid = m.oid 
		AND a.attnum = ANY(o.conkey)
		AND a.attisdropped = false -- не удалено
	) c on true
	-- поля primary key в таблице внешнего ключа
	INNER JOIN LATERAL (
		SELECT
			array_position(o.confkey, a.attnum) as FKOrder,
			a.attname
		FROM pg_attribute a 
		WHERE a.attrelid = f.oid 
		AND a.attnum = ANY(o.confkey)
		AND a.attisdropped = false -- не удалено
	) fc on fc.FKOrder = c.FKOrder
	WHERE o.contype = 'f' 
),
checkcons as (
	SELECT
		o.conrelid,
		o.confrelid,
		o.conname AS constraint_name,
		ms.nspname AS source_schema,
		m.relname AS source_table,
		attr.attnum AS source_position,
		attr.attname AS source_column,
		'dbo' AS target_schema,
		'yesno' AS target_table,
		null::int AS FKOrder,
		'yesno_id' AS target_column
	FROM pg_constraint o 
	INNER JOIN pg_class m ON m.oid = o.conrelid AND m.relkind in ('r','f','p')
	INNER JOIN pg_namespace ms ON ms.oid = m.relnamespace
	-- поля внешнего ключа в основной таблице
	INNER JOIN LATERAL (
		SELECT 
			a.attnum,
			a.attname
		FROM pg_attribute a 
		WHERE a.attrelid = m.oid 
		AND a.attnum = ANY(o.conkey)
		AND a.attisdropped = false -- не удалено
	) attr on true
	WHERE o.contype = 'c' 
	AND o.conname NOT iLIKE '%\_evnclass'
	AND o.conname NOT iLIKE '%\_envclass'
)
select 
    fk.source_column as FieldName,
    case 
        when fk.constraint_name iLIKE 'fk\_' || fk.source_table || '\_%' 
            then regexp_replace(fk.constraint_name, '^fk\_' || fk.source_table || '\_', 'fk_" + tablename + @"_', 'i')
        else 'fk_" + tablename + @"_' || fk.source_column
	end as FKName,
    fk.target_schema || '.' || fk.target_table as FKTable,
    fk.FKOrder,
    fk.target_column as FKField
from r";

                    if ((scriptType == Utilities.ScriptType.ALTER) || isOnlyExist) //-V3063
                    {
                        queryString += Environment.NewLine + @"
inner join lateral(
    select *
    from fkcons
    where fkcons.conrelid = r.inhparent
    union
    select *
    from checkcons
    where checkcons.conrelid = r.inhparent";
                    }
                    else
                    {
                        queryString += Environment.NewLine + @"
inner join lateral(
    select *
    from fkcons
    where fkcons.conrelid = r.inhrelid
    union
    select *
    from checkcons
    where checkcons.conrelid = r.inhrelid";
                    }

                    queryString += @"
) fk on true
where not (fk.target_schema = 'dbo' and fk.target_table = 'evn') -- не создаем fk на dbo.evn!
order by r.inhlevel, fk.source_position, FKName, FKOrder
";
                }
            }

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {

                            string _FieldName = reader[0].ToString();
                            string _FKName = reader[1].ToString();
                            string _FKTable = reader[2].ToString();
                            string _FKOrder = reader[3].ToString();
                            string _FKField = reader[4].ToString();

                            var fk = result.Where(x =>
                                (x.FKName.ToLower() == _FKName.ToLower()) &&
                                (x.FKField.ToLower() == _FKField.ToLower())
                            ).FirstOrDefault();

                            if (fk == null)
                            {
                                result.Add(new TableFKInfo()
                                {
                                    FieldName = _FieldName,
                                    FKName = _FKName,
                                    FKTable = _FKTable,
                                    FKOrder = _FKOrder,
                                    FKField = _FKField
                                });
                            }
                        }

                    }
                }
            }

            return result;
        }


        /// <summary>Заполнить информацию о списке CHECK родительских таблиц</summary>
        /// <param name="isOnlyExist">Генерировать скрипт по существующим данным таблицы, не пытаться ее "улучшить"</param>
        /// <param name="evnTable">Таблица из БД </param>
        /// <param name="evnParent">Таблица-предок из БД </param>
        /// <param name="scriptType">тип генерации скрипта </param>
        public List<TableCHECKInfo> FillListEvnParentCHECKs(bool isOnlyExist, string evnTable, string evnParent, Utilities.ScriptType scriptType)
        {
            List<TableCHECKInfo> result = new List<TableCHECKInfo>();

            string queryString = "";
            string tablename = Utilities.Databases.GetTableName(evnTable);

            // поля в текущей таблице evnTable, унаследованные из evnParent
            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                // для ПГ 
                if (isOnlyExist)
                {
                    // берем существующие констрейны для унаследованных полей
                    queryString = @"
SELECT
    attr.attname AS FieldName,
    o.conname AS FKName,
    REPLACE(pg_get_constraintdef(o.oid),'CHECK ','') AS FieldCheck
FROM pg_constraint o 
INNER JOIN pg_class m ON m.oid = o.conrelid AND m.relkind in ('r','f','p')
INNER JOIN pg_namespace ms ON ms.oid = m.relnamespace
-- поля внешнего ключа в основной таблице
INNER JOIN LATERAL (
	SELECT 
		a.attnum,
		a.attname
		FROM pg_attribute a 
		WHERE a.attrelid = m.oid 
		AND a.attnum = ANY(o.conkey)
		AND a.attisdropped = false -- не удалено
		AND a.attinhcount > 0 -- унаследовано
		LIMIT 1
	) attr on true
	WHERE o.contype = 'c' 
)
where o.conrelid = to_regclass('" + evnTable + @"')
order by attr.attnum, FKName
";
                }
                else
                {
                    // берем констрейны родительских таблиц
                    queryString = @"
WITH RECURSIVE r AS (
	select 
		inh.inhrelid, 
		inh.inhparent,
		0 as inhlevel
	from pg_inherits inh";

                    if ((scriptType == Utilities.ScriptType.ALTER) || isOnlyExist) //-V3063
                    {
                        queryString += Environment.NewLine + "where inh.inhrelid = to_regclass('" + evnTable + "')";
                    }
                    else
                    {
                        queryString += Environment.NewLine + "where inh.inhrelid = to_regclass('" + evnParent + "')";
                    }

                    queryString += @"
    union
	select 
		inh.inhrelid, 
		inh.inhparent,
		inhlevel - 1 as inhlevel
	from pg_inherits inh
	inner join r on inh.inhrelid = r.inhparent
),
checkcons as (
	SELECT
		o.conrelid,
		o.confrelid,
		o.conname AS constraint_name,
		ms.nspname AS source_schema,
		m.relname AS source_table,
		attr.attnum AS source_position,
		attr.attname AS source_column,
		REPLACE(pg_get_constraintdef(o.oid),'CHECK ','') AS consrc
	FROM pg_constraint o 
	INNER JOIN pg_class m ON m.oid = o.conrelid AND m.relkind in ('r','f','p')
	INNER JOIN pg_namespace ms ON ms.oid = m.relnamespace
	-- поля внешнего ключа в основной таблице
	INNER JOIN LATERAL (
		SELECT 
			a.attnum,
			a.attname
		FROM pg_attribute a 
		WHERE a.attrelid = m.oid 
		AND a.attnum = ANY(o.conkey)
		AND a.attisdropped = false
		LIMIT 1
	) attr on true
	WHERE o.contype = 'c' 
)
select 
    chk.source_column as FieldName,
    case 
        when chk.constraint_name iLIKE 'fk\_' || chk.source_table || '\_%' 
            then regexp_replace(chk.constraint_name, '^fk\_' || chk.source_table || '\_', 'fk_" + tablename + @"_', 'i')
        else 'fk_" + tablename + @"_' || chk.source_column
	end as FKName,
    chk.consrc as FieldCheck
from r
inner join lateral(
    select *
    from checkcons";
                    if ((scriptType == Utilities.ScriptType.ALTER) || isOnlyExist) //-V3063
                    {
                        queryString += Environment.NewLine + "where checkcons.conrelid = r.inhparent";
                    }
                    else
                    {
                        queryString += Environment.NewLine + "where checkcons.conrelid = r.inhrelid";
                    }

                    queryString += @"
) chk on true
order by r.inhlevel, chk.source_position, FKName
";
                }
            }

            if (!string.IsNullOrWhiteSpace(queryString))
            {
                using (DbDataReader reader = this.OpenQuery(queryString))
                {
                    if (reader != null)
                    {
                        while (reader.Read())
                        {

                            string _FieldName = reader[0].ToString();
                            string _FKName = reader[1].ToString();
                            string _FieldCheck = reader[2].ToString();

                            var fk = result.Where(x =>
                                (x.FKName.ToLower() == _FKName.ToLower())
                            ).FirstOrDefault();

                            if (fk == null)
                            {
                                result.Add(new TableCHECKInfo()
                                {
                                    FieldName = _FieldName,
                                    FKName = _FKName,
                                    FieldCheck = _FieldCheck
                                });
                            }
                        }

                    }
                }
            }

            return result;
        }


        /// <summary>
        /// проверяем наличие dbo.SQLGenDBLog 
        /// </summary>
        /// <returns></returns>
        public bool SQLGenDBLogExist()
        {
            string isexist = "";

            string queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                queryString = @"select top(1) 2 as isExist from sys.objects o with (nolock) where o.object_id = OBJECT_ID('dbo.SQLGenDBLog', 'U')";
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"select 2 as isExist from pg_tables where schemaname='dbo' and tablename='sqlgendblog' limit 1";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        isexist = reader[0].ToString();
                        break; //-V3020
                    }
                }
            }

            return isexist.Trim() == "2";
        }

        /// <summary>
        /// создаем dbo.SQLGenDBLog, если его нет 
        /// </summary>
        /// <param name="Error">текст ошибки</param>
        public bool SQLGenDBLogInit(out string Error)
        {
            if (!SQLGenDBLogExist())
            {
                string queryString = "";

                if (this.ConnType == Utilities.ConnType.MSSQL)
                {
                    queryString = @"IF OBJECT_ID('dbo.SQLGenDBLog', 'U') IS NULL
BEGIN
CREATE TABLE dbo.SQLGenDBLog (
	SQLGenDBLog_id BIGINT IDENTITY(1,1) NOT NULL,
	SQLGenDBLog_DT DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
	SQLGenDBLog_Uniqname NVARCHAR(255) NOT NULL,
	SQLGenDBLog_Changeset NVARCHAR(255) NOT NULL,
	SQLGenDBLog_Checksum NVARCHAR(50) NOT NULL,
	CONSTRAINT pk_SQLGenDBLog_id PRIMARY KEY CLUSTERED (SQLGenDBLog_id),
	CONSTRAINT uk_SQLGenDBLog UNIQUE (SQLGenDBLog_Uniqname, SQLGenDBLog_Changeset, SQLGenDBLog_Checksum)
)
END";
                }
                else if (this.ConnType == Utilities.ConnType.PGSQL)
                {
                    queryString = @"CREATE TABLE IF NOT EXISTS dbo.sqlgendblog (
	sqlgendblog_id BIGINT GENERATED BY DEFAULT AS IDENTITY NOT NULL,
	sqlgendblog_dt TIMESTAMP WITHOUT TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
	sqlgendblog_uniqname VARCHAR(255) NOT NULL,
	sqlgendblog_changeset VARCHAR(255) NOT NULL,
	sqlgendblog_checksum VARCHAR(50) NOT NULL,
    CONSTRAINT pk_SQLGenDBLog_id PRIMARY KEY (SQLGenDBLog_id),
    CONSTRAINT uk_SQLGenDBLog UNIQUE (SQLGenDBLog_Uniqname, SQLGenDBLog_Changeset, SQLGenDBLog_Checksum)
)
WITH (oids = false);";
                }

                Error = "";
                try
                {
                    ExecuteNonQuery(queryString, out Error);
                    return SQLGenDBLogExist();
                }
                catch (Exception ex)
                {
                    Error = App.GetFullExceptionMessage(ex).showMessage;
                }

                return false;
            }
            else
            {
                Error = "";
                return true;
            }
        }

        /// <summary>
        /// Ищем в dbo.SQLGenDBLog факт успешного выполнения скрипта
        /// </summary>
        /// <param name="uniqname">уникальное имя скрипта</param>
        /// <param name="changeset">имя changeset</param>
        /// <param name="checksum">контрольная сумма</param>
        /// <returns></returns>
        public bool SQLGenDBLogExecuted(string uniqname, string changeset, string checksum)
        {
            string isexist = "";

            string queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                queryString = @"
select top(1) 2 as isExist 
from dbo.SQLGenDBLog with (nolock)
where SQLGenDBLog_Uniqname = '" + uniqname + @"'
and SQLGenDBLog_Changeset = '" + changeset + @"'
and SQLGenDBLog_Checksum = '" + checksum + @"'
";
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"
select 2 as isExist 
from dbo.SQLGenDBLog
where SQLGenDBLog_Uniqname = '" + uniqname + @"'
and SQLGenDBLog_Changeset = '" + changeset + @"'
and SQLGenDBLog_Checksum = '" + checksum + @"'
limit 1;
";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        isexist = reader[0].ToString();
                        break; //-V3020
                    }
                }
            }

            return isexist.Trim() == "2";
        }

        /// <summary>
        /// Запись в dbo.SQLGenDBLog факта успешного выполнения скрипта
        /// </summary>
        /// <param name="uniqname">уникальное имя скрипта</param>
        /// <param name="changeset">имя changeset</param>
        /// <param name="checksum">контрольная сумма</param>
        public void SQLGenDBLogWrite(string uniqname, string changeset, string checksum)
        {
            string queryString = "";

            if (this.ConnType == Utilities.ConnType.MSSQL)
            {
                queryString = @"insert into dbo.SQLGenDBLog with (rowlock) 
(SQLGenDBLog_Uniqname, SQLGenDBLog_Changeset, SQLGenDBLog_Checksum) 
select '" + uniqname + @"', '" + changeset + @"', '" + checksum + @"'
where not exists (
    select top(1) 1 from dbo.SQLGenDBLog with (nolock) 
    where SQLGenDBLog_Uniqname = '" + uniqname + @"' and SQLGenDBLog_Changeset = '" + changeset + @"' and SQLGenDBLog_Checksum = '" + checksum + @"'
)";
            }
            else if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"insert into dbo.SQLGenDBLog
(SQLGenDBLog_Uniqname, SQLGenDBLog_Changeset, SQLGenDBLog_Checksum) 
select '" + uniqname + @"', '" + changeset + @"', '" + checksum + @"'
where not exists (
    select 1 from dbo.SQLGenDBLog
    where SQLGenDBLog_Uniqname = '" + uniqname + @"' and SQLGenDBLog_Changeset = '" + changeset + @"' and SQLGenDBLog_Checksum = '" + checksum + @"'
    limit 1
)";
            }

            try
            {
                ExecuteNonQuery(queryString, out string Error);
                if (!string.IsNullOrWhiteSpace(Error))
                {
                    App.AddLog(Error, null, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
                }
            }
            catch (Exception ex)
            {
                App.AddLog(null, ex, App.ShowMessageMode.NONE, true, MainWindow.Task.LogFile);
            }
        }


        /// <summary>
        /// Получаем команды по пересозданию EvnClass CHECK
        /// </summary>
        /// <param name="tablename">имя таблицы</param>
        /// <returns></returns>
        public string GetEvnClassCheck(string tablename)
        {
            string result = string.Empty;

            tablename = Utilities.Databases.GetTableName(tablename);

            string queryString = "";

            if (this.ConnType == Utilities.ConnType.PGSQL)
            {
                queryString = @"
CREATE OR REPLACE FUNCTION pg_temp.psv_createevnclasscheck(
	evnclass_sysnick character varying DEFAULT NULL::character varying,
	isrebuild integer DEFAULT NULL::integer
)
RETURNS varchar
LANGUAGE plpgsql
AS $function$
DECLARE
	rec RECORD; 
	p_consrc varchar;
	p_sql varchar;
	p_evnclass_sysnick ALIAS FOR evnclass_sysnick;
begin
	isrebuild := coalesce(isrebuild, 1);
	if trim(p_evnclass_sysnick)='' then
		p_evnclass_sysnick := NULL;
	end if;

	p_sql := '';

	for rec in (
		WITH RECURSIVE r AS (
			select e.evnclass_id, e.evnclass_pid, e.evnclass_sysnick from dbo.evnclass e where e.evnclass_sysnick ilike p_evnclass_sysnick
			union
			select e.evnclass_id, e.evnclass_pid, e.evnclass_sysnick from dbo.evnclass e join r on e.evnclass_id = r.evnclass_pid
		),
		l AS (
			select r.evnclass_id, r.evnclass_pid, r.evnclass_sysnick from r where p_evnclass_sysnick is not null
			union all
			select e.evnclass_id, e.evnclass_pid, e.evnclass_sysnick from dbo.evnclass e where p_evnclass_sysnick is null
		)
		select 
			lower(l.evnclass_sysnick) as tbl_name,
			lower(l.evnclass_sysnick) || '_envclass' as constraint_name,
			dbo.getevnclass_list(l.evnclass_id) classlist,
			'ALTER TABLE IF EXISTS dbo.' || lower(l.evnclass_sysnick) || ' ADD CONSTRAINT ' || lower(l.evnclass_sysnick) || '_envclass CHECK ('
			    || dbo.getevnclass_list(l.evnclass_id) || ');' AS sql_txt 
		from l
		where l.evnclass_id != 1 
		and exists (
			select 1 
			from pg_class t
			INNER JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
			where n.nspname='dbo' 
			and t.relname=lower(l.evnclass_sysnick)		
		)
		order by tbl_name 
	)
	loop
		select pg_get_expr(r.conbin, r.conrelid) consrc
		into p_consrc
		from pg_catalog.pg_constraint r
		inner join pg_class pgct on r.conrelid = pgct.oid
		inner join pg_catalog.pg_namespace pgns on pgct.relnamespace = pgns.oid
		where pgns.nspname = 'dbo'
		and pgct.relname = rec.tbl_name
		and contype = 'c'
		and conname=lower(rec.constraint_name);

		if p_consrc is null AND isrebuild = 1 then
			p_sql = p_sql || rec.sql_txt || chr(13) || chr(10);
		else
			if p_consrc != rec.classlist OR isrebuild = 2 then
				p_sql = p_sql || 'ALTER TABLE IF EXISTS dbo.' || rec.tbl_name || ' DROP CONSTRAINT IF EXISTS ' || rec.constraint_name || ';' || chr(13) || chr(10);
				p_sql = p_sql || rec.sql_txt || chr(13) || chr(10);
			end if;
		end if;
	end loop;

	return p_sql;
END;
$function$;

select psv_createevnclasscheck from pg_temp.psv_createevnclasscheck(evnclass_sysnick := '" + tablename + "', isrebuild := 2)";
            }

            using (DbDataReader reader = this.OpenQuery(queryString))
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        result = reader[0].ToString().Trim();
                        break; //-V3020
                    }
                }
            }

            return result;
        }

    }

    // -------------------------------------------------------------------------------------------------------
    /// <summary>Тип экземпляра БД</summary>
    public enum InstanceDBType
    {
        /// <summary>
        /// тестовая МС promeddev
        /// </summary>
        PROMEDDEV_MS,
        /// <summary>
        /// тестовая МС promedtest
        /// </summary>
        PROMEDTEST_MS,
        /// <summary>
        /// тестовая МС promedufa
        /// </summary>
        PROMEDUFA_MS,
        /// <summary>
        /// релизная МС promedwebrelease
        /// </summary>
        PROMEDWEBRELEASE_MS,
        /// <summary>
        /// релизная МС promedwebufarelease
        /// </summary>
        PROMEDWEBUFARELEASE_MS,
        /// <summary>
        /// тестовая ПГ promedtest
        /// </summary>
        PROMEDTEST_PG,
        /// <summary>
        /// тестовая ПГ promedadygea
        /// </summary>
        PROMEDADYGEA_PG,
        /// <summary>
        /// релизная ПГ promedrelease
        /// </summary>
        PROMEDRELEASE_PG,
        /// <summary>
        /// прочие
        /// </summary>
        OTHER
    }

    /// <summary>
    /// Информация об объекте БД
    /// </summary>
    public class DBObjectInfo
    {
        /// <summary>
        /// Тип объекта БД
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Схема объекта БД
        /// </summary>
        public string Schema { get; set; }

        /// <summary>
        /// Имя объекта БД
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Полное имя объекта БД
        /// </summary>
        public string FullName
        {
            get
            {
                return (Schema ?? "") + "." + (Name ?? "");
            }
        }

        /// <summary>
        /// Признак автогенности
        /// </summary>
        public string isAutogen { get; set; }

        /// <summary>
        /// Скрипт объекта БД
        /// </summary>
        public string Text { get; set; }
    }

    /// <summary>
    /// Объект БД с OID
    /// </summary>
    public class DBObjectOID
    {
        /// <summary>
        /// схема
        /// </summary>
        public string Schema;
        /// <summary>
        /// Название объекта
        /// </summary>
        public string Name;
        /// <summary>
        /// oid
        /// </summary>
        public string OID;
    }

    /// <summary>
    /// Информация о primary key
    /// </summary>
    public class PKInfo
    {
        /// <summary>
        /// Имя таблицы
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Список с информацией о полях PK
        /// </summary>
        public List<PKField> Fields { get; set; } = new List<PKField>();

        /// <summary>
        /// Список имен полей PK
        /// </summary>
        public List<string> ListFieldNames => Fields
                .OrderBy(x => x.PKOrder)
                .Select(x => x.FieldName)
                .ToList();

        /// <summary>
        /// Список типов полей PK
        /// </summary>
        public List<string> ListFieldTypes => Fields
                .OrderBy(x => x.PKOrder)
                .Select(x => x.FieldType)
                .ToList();

        /// <summary>
        /// Список имен полей PK через запятую
        /// </summary>
        /// <returns></returns>
        public string FieldNamesToString => String.Join(",", Fields
                .OrderBy(x => x.PKOrder)
                .Select(x => x.FieldName)
                .ToArray()
            );

        /// <summary>
        /// Список типов полей PK через запятую
        /// </summary>
        public string FieldTypesToString => String.Join(",", Fields
                .OrderBy(x => x.PKOrder)
                .Select(x => x.FieldType)
                .ToArray()
            );

        /// <summary>
        /// Имя первого поля PK
        /// </summary>
        public string FirstFieldName => Fields
                .OrderBy(x => x.PKOrder)
                .Select(x => x.FieldName)
                .FirstOrDefault() ?? "";

        /// <summary>
        /// Тип первого поля PK
        /// </summary>
        public string FirstFieldType => Fields
                .OrderBy(x => x.PKOrder)
                .Select(x => x.FieldType)
                .FirstOrDefault() ?? "";

        /// <summary>
        /// Получим поле PK по имени
        /// </summary>
        /// <param name="_fieldName">имя поля</param>
        /// <returns></returns>
        public PKField GetField (string _fieldName) => Fields
                .Where(x => x.FieldName.ToLower() == _fieldName)
                .FirstOrDefault();

        /// <summary>
        /// =true - поле есть в PK
        /// </summary>
        /// <param name="_fieldName"></param>
        /// <returns></returns>
        public bool Exists (string _fieldName) => GetField(_fieldName) != null;

        /// <summary>
        /// =true - одно из полей PK - identity
        /// </summary>
        public bool HasIdentity
        {
            get
            {
                return Fields.Any(x => x.IsIdentity);
            }
        }

        /// <summary>
        /// =true - PK найден
        /// </summary>
        public bool HasPK
        {
            get
            {
                return Fields.Any();
            }
        }
    }

    /// <summary>
    /// Информация о поле PK
    /// </summary>
    public class PKField
    {
        /// <summary>
        /// Номер по порядку
        /// </summary>
        public int PKOrder { get; set; }

        /// <summary>
        /// Имя поля
        /// </summary>
        public string FieldName { get; set; } = "";

        /// <summary>
        /// Тип поля
        /// </summary>
        public string FieldType { get; set; } = "";

        /// <summary>
        /// =true - поле identity
        /// </summary>
        public bool IsIdentity { get; set; } = false;
    }

}