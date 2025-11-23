// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System.ComponentModel;
using System.Runtime.CompilerServices;
using SQLGen.Utilities;

namespace SQLGen
{
    // -------------------------------------------------------------------------------------------------------
    /// <summary>Типы операций с маркерами</summary>
    public enum MarkerOperType
    {
        /// <summary>
        /// добавить
        /// </summary>
        ADD,
        /// <summary>
        /// изменить
        /// </summary>
        EDIT,
        /// <summary>
        /// скопировать и сделать дубликат
        /// </summary>
        COPY
    }

    // =========================================================================================================
    /// <summary>Класс Маркер</summary>
    public class FreeDocMarker : INotifyPropertyChanged
    {
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

        /// <summary>
        /// Конструктор FreeDocMarker
        /// </summary>
        /// <param name="freeDocMarker_id">идентификатор FreeDocMarker</param>
        /// <param name="evnClass_SysNick">Класс события</param>
        /// <param name="freeDocMarker_Name">Наименование маркера</param>
        /// <param name="freeDocMarker_TableAlias">Алиас</param>
        /// <param name="freeDocMarker_Field">Поле</param>
        /// <param name="freeDocMarker_Query">Запрос</param>
        /// <param name="freeDocMarker_Description">Описание</param>
        /// <param name="freeDocMarker_IsTableValue_string">="true" иди ="2" - табличный маркер</param>
        /// <param name="freeDocMarker_Options">Options</param>
        public FreeDocMarker(
            string freeDocMarker_id,
            string evnClass_SysNick,
            string freeDocMarker_Name,
            string freeDocMarker_TableAlias,
            string freeDocMarker_Field,
            string freeDocMarker_Query,
            string freeDocMarker_Description,
            string freeDocMarker_IsTableValue_string,
            string freeDocMarker_Options
            )
        {
            FreeDocMarker_id = freeDocMarker_id;
            original_FreeDocMarker_id = FreeDocMarker_id;
            EvnClass_SysNick = evnClass_SysNick;
            original_EvnClass_SysNick = EvnClass_SysNick;
            FreeDocMarker_Name = freeDocMarker_Name;
            original_FreeDocMarker_Name = FreeDocMarker_Name;
            FreeDocMarker_TableAlias = freeDocMarker_TableAlias;
            original_FreeDocMarker_TableAlias = FreeDocMarker_TableAlias;
            FreeDocMarker_Field = freeDocMarker_Field;
            original_FreeDocMarker_Field = FreeDocMarker_Field;
            FreeDocMarker_Query = freeDocMarker_Query;
            original_FreeDocMarker_Query = FreeDocMarker_Query;
            FreeDocMarker_Description = freeDocMarker_Description;
            original_FreeDocMarker_Description = FreeDocMarker_Description;
            FreeDocMarker_Options = freeDocMarker_Options;
            original_FreeDocMarker_Options = FreeDocMarker_Options;
            FreeDocMarker_IsTableValue_string = freeDocMarker_IsTableValue_string;
            original_FreeDocMarker_IsTableValue = FreeDocMarker_IsTableValue;
            isFiltered = true;
            isError = false;
        }

        /// <summary>Флаг - в маркеры внесены изменения</summary>
        public bool isChanged
        {
            get
            {
                return FreeDocMarker_id.StartsWith("новый") || (!(
                    (FreeDocMarker_id == original_FreeDocMarker_id) &&
                    (EvnClass_SysNick == original_EvnClass_SysNick) &&
                    (FreeDocMarker_Name == original_FreeDocMarker_Name) &&
                    (FreeDocMarker_TableAlias == original_FreeDocMarker_TableAlias) &&
                    (FreeDocMarker_Field == original_FreeDocMarker_Field) &&
                    (FreeDocMarker_Query == original_FreeDocMarker_Query) &&
                    (FreeDocMarker_Description == original_FreeDocMarker_Description) &&
                    (FreeDocMarker_Options == original_FreeDocMarker_Options) &&
                    (FreeDocMarker_IsTableValue == original_FreeDocMarker_IsTableValue)
                    ));
            }
        }

        /// <summary>Флаг - в связи внесены изменения</summary>
        public bool isChangedRelation { get; set; }

        /// <summary>Флаг - включено в фильтр</summary>
        public bool isFiltered { get; set; }

        /// <summary>Флаг - ошибка</summary>
        public bool isError { get; set; }


        private string _id;
        /// <summary>Идентификатор</summary>
        public string FreeDocMarker_id
        {
            get
            {
                return _id ?? "";
            }
            set
            {
                _id = value;
                if (string.IsNullOrWhiteSpace(_id)) _id = "";
                _id = _id.Trim();
                OnPropertyChanged("FreeDocMarker_id");
            }
        }

        private string original_FreeDocMarker_id { get; set; }


        private string _sysnick;
        /// <summary>Класс события</summary>
        public string EvnClass_SysNick
        {
            get
            {
                return _sysnick ?? "";
            }
            set
            {
                _sysnick = value;
                if (string.IsNullOrWhiteSpace(_sysnick)) _sysnick = "";
                _sysnick = _sysnick.Trim();
                OnPropertyChanged("EvnClass_SysNick");
            }
        }

        private string original_EvnClass_SysNick { get; set; }

        private string _name;
        /// <summary>Наименование</summary>
        public string FreeDocMarker_Name
        {
            get
            {
                return _name ?? "";
            }
            set
            {
                _name = value;
                if (string.IsNullOrWhiteSpace(_name)) _name = "";
                _name = _name.Trim();
                OnPropertyChanged("FreeDocMarker_Name");
            }
        }

        private string original_FreeDocMarker_Name { get; set; }

        private string _tablealias;
        /// <summary>Псевдоним целевой таблицы</summary>
        public string FreeDocMarker_TableAlias
        {
            get
            {
                return _tablealias ?? "";
            }
            set
            {
                _tablealias = value;
                if (string.IsNullOrWhiteSpace(_tablealias)) _tablealias = "";
                _tablealias = _tablealias.Trim();
                OnPropertyChanged("FreeDocMarker_TableAlias");
            }
        }

        private string original_FreeDocMarker_TableAlias { get; set; }

        private string _field;
        /// <summary>Поле в целевой таблице содержащее данные</summary>
        public string FreeDocMarker_Field
        {
            get
            {
                return _field ?? "";
            }
            set
            {
                _field = value;
                if (string.IsNullOrWhiteSpace(_field)) _field = "";
                _field = _field.Trim();
                OnPropertyChanged("FreeDocMarker_Field");
            }
        }

        private string original_FreeDocMarker_Field { get; set; }

        private string _query;
        /// <summary>Запрос для извлечения данных</summary>
        public string FreeDocMarker_Query
        {
            get
            {
                return _query ?? "";
            }
            set
            {
                _query = value;
                if (string.IsNullOrWhiteSpace(_query)) _query = "";
                _query = _query
                    .Trim()
                    .Replace("\r\n", "\n")
                    .Replace("\n", "\r\n");
                OnPropertyChanged("FreeDocMarker_Query");
            }
        }

        private string original_FreeDocMarker_Query { get; set; }

        private string _desc;
        /// <summary>Запрос для извлечения данных</summary>
        public string FreeDocMarker_Description
        {
            get
            {
                return _desc ?? "";
            }
            set
            {
                _desc = value;
                if (string.IsNullOrWhiteSpace(_desc)) _desc = "";
                _desc = _desc.Trim();
                OnPropertyChanged("FreeDocMarker_Description");
            }
        }

        private string original_FreeDocMarker_Description { get; set; }

        private string _option;
        /// <summary>Запрос для извлечения данных</summary>
        public string FreeDocMarker_Options
        {
            get
            {
                return _option ?? "";
            }
            set
            {
                _option = value;
                if (string.IsNullOrWhiteSpace(_option)) _option = "";
                _option = _option.Trim();
                OnPropertyChanged("FreeDocMarker_Options");
            }
        }

        private string original_FreeDocMarker_Options { get; set; }

        bool _istablevalue;
        /// <summary>FreeDocMarker_IsTableValue</summary>
        public bool FreeDocMarker_IsTableValue
        {
            get
            {
                return _istablevalue;
            }
            set
            {
                _istablevalue = value;
                OnPropertyChanged("FreeDocMarker_IsTableValue");
            }
        }

        private bool original_FreeDocMarker_IsTableValue { get; set; }

        /// <summary>FreeDocMarker_IsTableValue в виде текста ("true"/"2" или "false"/"1"/""/null)</summary>
        public string FreeDocMarker_IsTableValue_string
        {
            get { if (FreeDocMarker_IsTableValue == true) return "2"; else return "1"; }
            set
            {
                FreeDocMarker_IsTableValue = (!string.IsNullOrWhiteSpace(value)) && ((value.Trim().ToLower() == "true") || (value.Trim().ToLower() == "2"));
                OnPropertyChanged("FreeDocMarker_IsTableValue");
            }
        }

        /// <summary>
        /// копирование экземпляра FreeDocMarker
        /// </summary>
        /// <returns></returns>
        public FreeDocMarker Copy()
        {
            return (FreeDocMarker)this.MemberwiseClone();
        }
    }


    // =========================================================================================================
    /// <summary>Класс Связи маркеров и источников данных</summary>
    public class FreeDocRelationship : INotifyPropertyChanged
    {

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

        /// <summary>
        /// Конструктор FreeDocRelationship
        /// </summary>
        /// <param name="freeDocRelationship_id">идентификатор FreeDocRelationship</param>
        /// <param name="evnClass_SysNick">Класс события</param>
        /// <param name="freeDocRelationship_AliasName">Алиас</param>
        /// <param name="freeDocRelationship_AliasTable">Таблица</param>
        /// <param name="freeDocRelationship_AliasQuery">Запрос</param>
        /// <param name="freeDocRelationship_LinkedAlias">Ссылка на Алиас другой таблицы</param>
        /// <param name="freeDocRelationship_LinkDescription">Описание</param>
        public FreeDocRelationship(
            string freeDocRelationship_id,
            string evnClass_SysNick,
            string freeDocRelationship_AliasName,
            string freeDocRelationship_AliasTable,
            string freeDocRelationship_AliasQuery,
            string freeDocRelationship_LinkedAlias,
            string freeDocRelationship_LinkDescription
            )
        {
            FreeDocRelationship_id = freeDocRelationship_id;
            original_FreeDocRelationship_id = FreeDocRelationship_id;
            EvnClass_SysNick = evnClass_SysNick;
            original_EvnClass_SysNick = EvnClass_SysNick;
            FreeDocRelationship_AliasName = freeDocRelationship_AliasName;
            original_FreeDocRelationship_AliasName = FreeDocRelationship_AliasName;
            FreeDocRelationship_AliasTable = freeDocRelationship_AliasTable;
            original_FreeDocRelationship_AliasTable = FreeDocRelationship_AliasTable;
            FreeDocRelationship_AliasQuery = freeDocRelationship_AliasQuery;
            original_FreeDocRelationship_AliasQuery = FreeDocRelationship_AliasQuery;
            FreeDocRelationship_LinkDescription = freeDocRelationship_LinkDescription;
            original_FreeDocRelationship_LinkDescription = FreeDocRelationship_LinkDescription;
            FreeDocRelationship_LinkedAlias = freeDocRelationship_LinkedAlias;
            original_FreeDocRelationship_LinkedAlias = FreeDocRelationship_LinkedAlias;
            Order = 0;
            isError = false;
        }

        /// <summary>Флаг - ошибка</summary>
        public bool isError { get; set; }


        /// <summary>Поле для сортировки и фильтрации</summary>
        public int Order { get; set; }

        /// <summary>Флаг - в данные внесены изменения</summary>
        public bool isChanged
        {
            get
            {
                return FreeDocRelationship_id.StartsWith("новый") || (!(
                    (FreeDocRelationship_id == original_FreeDocRelationship_id) &&
                    (EvnClass_SysNick == original_EvnClass_SysNick) &&
                    (FreeDocRelationship_AliasName == original_FreeDocRelationship_AliasName) &&
                    (FreeDocRelationship_AliasTable == original_FreeDocRelationship_AliasTable) &&
                    (FreeDocRelationship_AliasQuery == original_FreeDocRelationship_AliasQuery) &&
                    (FreeDocRelationship_LinkDescription == original_FreeDocRelationship_LinkDescription) &&
                    (FreeDocRelationship_LinkedAlias == original_FreeDocRelationship_LinkedAlias)
                    ));
            }
        }


        private string _id;
        /// <summary>Идентификатор</summary>
        public string FreeDocRelationship_id
        {
            get
            {
                return _id ?? "";
            }
            set
            {
                _id = value;
                if (string.IsNullOrWhiteSpace(_id)) _id = "";
                _id = _id.Trim();
                OnPropertyChanged("FreeDocRelationship_id");
            }
        }

        private string original_FreeDocRelationship_id { get; set; }

        private string _sysnick;
        /// <summary>Класс события</summary>
        public string EvnClass_SysNick
        {
            get
            {
                return _sysnick ?? "";
            }
            set
            {
                _sysnick = value;
                if (string.IsNullOrWhiteSpace(_sysnick)) _sysnick = "";
                _sysnick = _sysnick.Trim();
                OnPropertyChanged("EvnClass_SysNick");
            }
        }

        private string original_EvnClass_SysNick { get; set; }

        private string _aliasname;
        /// <summary>наименование псевдонима связи</summary>
        public string FreeDocRelationship_AliasName
        {
            get
            {
                return _aliasname ?? "";
            }
            set
            {
                _aliasname = value;
                if (string.IsNullOrWhiteSpace(_aliasname)) _aliasname = "";
                _aliasname = _aliasname.Trim();
                OnPropertyChanged("FreeDocRelationship_AliasName");
            }
        }

        private string original_FreeDocRelationship_AliasName { get; set; }

        private string _aliastable;
        /// <summary>наименование целевой таблицы связи</summary>
        public string FreeDocRelationship_AliasTable
        {
            get
            {
                return _aliastable ?? "";
            }
            set
            {
                _aliastable = value;
                if (string.IsNullOrWhiteSpace(_aliastable)) _aliastable = "";
                _aliastable = _aliastable.Trim();
                OnPropertyChanged("FreeDocRelationship_AliasTable");
            }
        }

        private string original_FreeDocRelationship_AliasTable { get; set; }

        private string _query;
        /// <summary>текст целевого запросах</summary>
        public string FreeDocRelationship_AliasQuery
        {
            get
            {
                return _query ?? "";
            }
            set
            {
                _query = value;
                if (string.IsNullOrWhiteSpace(_query)) _query = "";
                _query = _query
                    .Trim()
                    .Replace("\r\n", "\n")
                    .Replace("\n", "\r\n");
                OnPropertyChanged("FreeDocRelationship_AliasQuery");
            }
        }

        private string original_FreeDocRelationship_AliasQuery { get; set; }


        private string _desc;
        /// <summary>элемент запроса для связи целевой таблицы/запроса и связанного псевдонима</summary>
        public string FreeDocRelationship_LinkDescription
        {
            get
            {
                return _desc ?? "";
            }
            set
            {
                _desc = value;
                if (string.IsNullOrWhiteSpace(_desc)) _desc = "";
                _desc = _desc.Trim();
                OnPropertyChanged("FreeDocRelationship_LinkDescription");
            }
        }

        private string original_FreeDocRelationship_LinkDescription { get; set; }

        private string _linkedalias;
        /// <summary>наименование связанного псевдонима</summary>
        public string FreeDocRelationship_LinkedAlias
        {
            get
            {
                return _linkedalias ?? "";
            }
            set
            {
                _linkedalias = value;
                if (string.IsNullOrWhiteSpace(_linkedalias)) _linkedalias = "";
                _linkedalias = _linkedalias.Trim();
                OnPropertyChanged("FreeDocRelationship_LinkedAlias");
            }
        }

        private string original_FreeDocRelationship_LinkedAlias { get; set; }

        /// <summary>
        /// Копирование экземпляра FreeDocRelationship
        /// </summary>
        /// <returns></returns>
        public FreeDocRelationship Copy()
        {
            return (FreeDocRelationship)this.MemberwiseClone();
        }
    }


    // =========================================================================================================
    /// <summary>Класс события</summary>
    public class EvnClass
    {
        private string _sysnick;
        /// <summary>
        /// Таблица класса события
        /// </summary>
        public string SysNick
        {
            get
            {
                return _sysnick ?? "";
            }
            set
            {
                _sysnick = value;
                if (string.IsNullOrWhiteSpace(_sysnick)) _sysnick = "";
                _sysnick = _sysnick.Trim();
            }
        }

        private string _parent;
        /// <summary>
        /// Таблица родительского класса события
        /// </summary>
        public string ParentSysNick
        {
            get
            {
                return _parent ?? "";
            }
            set
            {
                _parent = value;
                if (string.IsNullOrWhiteSpace(_parent)) _parent = "";
                _parent = _parent.Trim();
            }
        }
    }

}
