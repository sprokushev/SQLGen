// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using SQLGen.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace SQLGen
{
    /// <summary>
    ///  Поиск разных данных
    /// </summary>
    public static class Search
    {
        // -------------------------------------------------------------------------------------------------------
        /// <summary>Список поисковых запросов из файла ListSearches.json</summary>
        public static List<SearchInfo> ListSearches = new List<SearchInfo>();

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Загрузить список поисковых запросов</summary>
        public static void LoadSearches()
        {
            // загрузить поисковые запросы из файла
            /*string filename = Path.Combine(App.AppPath, "ListSearches.json");
            if (File.Exists(filename))
            {
                try
                {
                    string jsonString = File.ReadAllText(filename);
                    ListSearches = JsonSerializer.Deserialize<List<SearchInfo>>(jsonString).OrderBy(x => x.Tag).ToList();
                }
                catch (Exception ex)
                {
                    App.AddLog("", ex, App.ShowMessageMode.NONE);
                }
            }*/

            // добавить запросы по умолчанию
            SearchInfo find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "PROCTEXT") && (x.DBType == "MSSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "PROCTEXT";
                find.DBType = "MSSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: искать в процедурах и функциях Microsoft SQL";
            find.FieldText = "proctext";
/*            find.Text = @"
SELECT 
	sp.type_desc as proctype,
	s.name as schemaname,
	sp.name as procname,
	case when sm.definition LIKE '%autogen%' then 'AUTOGEN' else '' end as autogen,
	sm.definition as proctext
FROM sys.procedures sp
JOIN sys.sql_modules sm ON sp.object_id = sm.object_id
JOIN sys.schemas s ON s.schema_id = sp.schema_id
WHERE (
    sm.definition LIKE '%%%SEARCH%%%'
    OR s.name + '.' + sp.name LIKE '%%%SEARCH%%%'
)
AND s.name NOT IN ('information_schema', 'sys', 'tmp', 'TABLE tmp', 'raw', 'symdict', 'SWN\savage', 'audit', 'public', 'liquibase', 'diff', 'box')
AND (%%WITHAUTOGEN%%=2 OR sm.definition NOT LIKE '%autogen%')
AND (%%WITHREGISTRY%%=2 OR sp.name NOT LIKE '%registry%')
ORDER BY s.name, sp.name
";*/
            find.Text = @"
with cte as (
SELECT 
	ROUTINE_TYPE as proctype,
	ROUTINE_SCHEMA as schemaname,
	ROUTINE_NAME as procname,
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
    end as Autogen,
	comments.description as proctext
from information_schema.ROUTINES
outer apply (
    select OBJECT_DEFINITION(object_id(ROUTINE_SCHEMA + '.' + ROUTINE_NAME)) as description
) comments
WHERE (
    comments.description LIKE '%%%SEARCH%%%'
    OR ROUTINE_SCHEMA + '.' + ROUTINE_NAME LIKE '%%%SEARCH%%%'
)
AND ROUTINE_SCHEMA NOT IN ('information_schema', 'sys', 'tmp', 'TABLE tmp', 'raw', 'symdict', 'SWN\savage', 'audit', 'public', 'liquibase', 'diff')
)
select *
from cte
where (%%WITHAUTOGEN%%=2 OR proctext NOT LIKE '%autogen%')
AND (%%WITHREGISTRY%%=2 OR procname NOT LIKE '%registry%' OR procname LIKE '%registryes%' OR procname LIKE '%registry_evnstick%' OR procname LIKE '%registry_lvn%')
ORDER BY schemaname, procname
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "PROCTEXT") && (x.DBType == "PGSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "PROCTEXT";
                find.DBType = "PGSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: искать в процедурах и функциях Postgres";
            find.FieldText = "proctext";
            find.Text = @"
DROP TABLE IF EXISTS tmp_sqlgen_proc;

CREATE TEMP TABLE IF NOT EXISTS tmp_sqlgen_proc ON COMMIT DROP AS
WITH cte AS (
select
	case 
		when p.prokind='f' then 'FUNCTION'
		when p.prokind='p' then 'PROCEDURE'
		else '?'
	end as proctype,
	n.nspname as schemaname, 
	p.proname as procname, 
	pg_get_functiondef(p.oid) as proctext,
	obj_description(p.oid, 'pg_proc') as description
from pg_proc p
INNER JOIN pg_namespace n ON n.oid = p.pronamespace
WHERE n.nspname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'liquibase', 'diff')
AND n.nspname not like '%\_old'
AND n.nspname not like 'pg\_%'
AND exists (select 1 from pg_language l where l.oid = p.prolang and l.lanname in ('sql', 'plpgsql'))
)
SELECT
	proctype,
	schemaname, 
	procname, 
	case 
		when proctext iLIKE '%autoge%' then 'AUTOGEN'
		when description iLIKE '%autoge%' then 'AUTOGEN'
		else ''
	end as autogen,
	proctext
from cte;

SELECT * 
FROM tmp_sqlgen_proc 
WHERE (
    proctext iLIKE '%%%SEARCH%%%'
    OR schemaname || '.' || procname iLIKE '%%%SEARCH%%%'
)
AND (%%WITHAUTOGEN%%=2 OR autogen <> 'AUTOGEN')
AND (%%WITHREGISTRY%%=2 OR procname NOT iLIKE '%registry%' OR procname iLIKE '%registryes%' OR procname iLIKE '%registry_evnstick%' OR procname iLIKE '%registry_lvn%')
ORDER BY schemaname, procname;
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "VIEWTEXT") && (x.DBType == "MSSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "VIEWTEXT";
                find.DBType = "MSSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: искать в представлениях Microsoft SQL";
            find.FieldText = "viewtext";
            find.Text = @"
SELECT 
	sv.type_desc as viewtype,
	s.name as schemaname,
	sv.name as viewname,
	case when sm.definition LIKE '%autogen%' then 'AUTOGEN' else '' end as autogen,
	sm.definition as viewtext
FROM sys.views sv
JOIN sys.schemas s ON s.schema_id = sv.schema_id
outer apply (
    select OBJECT_DEFINITION(sv.object_id) as definition
) sm
WHERE (
    sm.definition LIKE '%%%SEARCH%%%'
    OR s.name + '.' + sv.name LIKE '%%%SEARCH%%%'
)
AND s.name NOT IN ('information_schema', 'sys', 'tmp', 'TABLE tmp', 'raw', 'symdict', 'SWN\savage', 'audit', 'public', 'liquibase', 'diff')
AND (%%WITHAUTOGEN%%=2 OR sm.definition NOT LIKE '%autogen%')
AND (%%WITHREGISTRY%%=2 OR sv.name NOT LIKE '%registry%')
ORDER BY s.name, sv.name
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "VIEWTEXT") && (x.DBType == "PGSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "VIEWTEXT";
                find.DBType = "PGSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: искать в представлениях Postgres";
            find.FieldText = "viewtext";
            find.Text = @"
DROP TABLE IF EXISTS tmp_sqlgen_view;

CREATE TEMP TABLE IF NOT EXISTS tmp_sqlgen_view ON COMMIT DROP AS
WITH cte AS (
select 
	'VIEW' as viewtype,
	schemaname, 
	viewname, 
	definition as viewtext,
	obj_description(to_regclass(quote_ident(schemaname) || '.' || quote_ident(viewname)), 'pg_class') as description
from pg_views
WHERE schemaname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'liquibase', 'diff')
AND schemaname not like '%\_old'
AND schemaname not like 'pg\_%'

union

select
	'MATERIALIZED VIEW' as viewtype,
	schemaname,
	matviewname as viewname,
	definition as viewtext,
	obj_description(to_regclass(quote_ident(schemaname) || '.' || quote_ident(matviewname)), 'pg_class') as description
from pg_matviews 
where schemaname not in ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff')
and schemaname not like '%\_old'
AND schemaname not like 'pg\_%'
)

SELECT
	viewtype,
	schemaname, 
	viewname, 
	case 
		when description iLIKE '%autoge%' then 'AUTOGEN'
		else ''
	end as autogen,
	viewtext
from cte
;

SELECT * 
FROM tmp_sqlgen_view 
WHERE (
    viewtext iLIKE '%%%SEARCH%%%'
    OR schemaname || '.' || viewname iLIKE '%%%SEARCH%%%'
)
AND (%%WITHAUTOGEN%%=2 OR autogen <> 'AUTOGEN')
AND (%%WITHREGISTRY%%=2 OR viewname NOT iLIKE '%registry%')
ORDER BY schemaname, viewname;
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "TABLE") && (x.DBType == "MSSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "TABLE";
                find.DBType = "MSSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: искать в таблицах Microsoft SQL";
            find.FieldTable = "tablename";
            find.Text = @"
SELECT 
    c.type_name as typename,
    c.column_length as columnlength,
    c.column_name as columnname,
    c.column_description as columndescription,
    c.schema_name + '.' + c.table_name as tablename,
    c.table_description as tabledescription
FROM dbo.v_columns c with (NOLOCK) 
WHERE (1=1)
AND c.table_type='U' 
AND c.schema_name NOT IN ('information_schema', 'sys', 'tmp', 'TABLE tmp', 'raw', 'symdict', 'SWN\savage', 'audit', 'public', 'liquibase', 'diff')
AND (
    c.schema_name + '.' + table_name LIKE '%%%SEARCH%%%'
    OR c.table_description LIKE '%%%SEARCH%%%'
    OR c.column_name LIKE '%%%SEARCH%%%'
    OR c.column_description LIKE '%%%SEARCH%%%'
)
AND (%%WITHREGISTRY%%=2 OR c.table_name NOT LIKE '%registry%')
ORDER BY c.schema_name, c.table_name, c.column_order
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "TABLE") && (x.DBType == "PGSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "TABLE";
                find.DBType = "PGSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: искать в таблицах Postgres";
            find.FieldTable = "tablename";
            find.Text = @"
SELECT 
    c.type as typename,
    c.length as columnlength,
    c.column_name as columnname,
    c.description as columndescription,
    c.schema_name || '.' || c.table_name as tablename,
    c.table_description as tabledescription
FROM dbo.v_columns c
inner join pg_catalog.pg_class p on p.oid = c.tbloid
WHERE (1=1)
AND p.relkind IN ('r','f','p')
AND c.schema_name NOT IN ('information_schema', 'pg_catalog', 'sys', 'tmp', 'TABLE tmp', 'public', 'audit', 'pg_toast', 'eyes', 'liquibase', 'diff')
AND c.schema_name not like '%\_old'
AND c.schema_name not like 'pg\_%'
AND (
    c.schema_name || '.' || table_name iLIKE '%%%SEARCH%%%'
    OR c.table_description iLIKE '%%%SEARCH%%%'
    OR c.column_name iLIKE '%%%SEARCH%%%'
    OR c.description iLIKE '%%%SEARCH%%%'
)
AND (%%WITHREGISTRY%%=2 OR c.table_name NOT iLIKE '%registry%')
ORDER BY c.schema_name, c.table_name, c.index
;
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "FK_DEPEND") && (x.DBType == "MSSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "FK_DEPEND";
                find.DBType = "MSSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: искать в foreign key Microsoft SQL (что зависит от таблицы)";
            find.FieldTable = "tablename";
            find.Text = @"

SELECT
    fk.name as foreignkeyname,
    s_par.name + '.' + t_par.name as tablename,
    ep1.value as tabledescription,
    c.name as columnname,
    ep2.value as columndescription,
	s_ref.name + '.' + t_ref.name as searchname
from sys.foreign_keys fk
inner join sys.foreign_key_columns fkc on fkc.constraint_object_id=fk.object_id

inner join sys.columns c on c.object_id=fkc.parent_object_id and c.column_id=fkc.parent_column_id
left join sys.extended_properties ep2 on ep2.major_id = c.object_id and ep2.minor_id = c.column_id

inner join sys.tables t_par on t_par.object_id=fk.parent_object_id
inner join sys.schemas s_par on s_par.schema_id=t_par.schema_id
left join sys.extended_properties ep1 on ep1.major_id = t_par.object_id and ep1.minor_id = 0 and ep1.name <> 'SWAN_RegionalTable' 

inner join sys.tables t_ref on t_ref.object_id=fk.referenced_object_id
inner join sys.schemas s_ref on s_ref.schema_id=t_ref.schema_id
left join sys.extended_properties ep3 on ep3.major_id = t_ref.object_id and ep3.minor_id = 0 and ep3.name <> 'SWAN_RegionalTable' 

where fk.referenced_object_id=object_id('%%SEARCH%%')
ORDER BY s_par.name, t_par.name
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "FK_ONDEPEND") && (x.DBType == "MSSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "FK_ONDEPEND";
                find.DBType = "MSSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: искать в foreign key Microsoft SQL (от чего зависит таблица)";
            find.FieldTable = "tablename";
            find.Text = @"
SELECT
    fk.name as foreignkeyname,
    s_par.name + '.' + t_par.name as searchname,
    c.name as columnname,
    ep2.value as columndescription,
	s_ref.name + '.' + t_ref.name as tablename,
    ep3.value as tabledescription	
from sys.foreign_keys fk
inner join sys.foreign_key_columns fkc on fkc.constraint_object_id=fk.object_id

inner join sys.columns c on c.object_id=fkc.parent_object_id and c.column_id=fkc.parent_column_id
left join sys.extended_properties ep2 on ep2.major_id = c.object_id and ep2.minor_id = c.column_id

inner join sys.tables t_par on t_par.object_id=fk.parent_object_id
inner join sys.schemas s_par on s_par.schema_id=t_par.schema_id
left join sys.extended_properties ep1 on ep1.major_id = t_par.object_id and ep1.minor_id = 0 and ep1.name <> 'SWAN_RegionalTable' 

inner join sys.tables t_ref on t_ref.object_id=fk.referenced_object_id
inner join sys.schemas s_ref on s_ref.schema_id=t_ref.schema_id
left join sys.extended_properties ep3 on ep3.major_id = t_ref.object_id and ep3.minor_id = 0 and ep3.name <> 'SWAN_RegionalTable' 

where fk.parent_object_id=object_id('%%SEARCH%%')
ORDER BY c.column_id
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "FK_DEPEND") && (x.DBType == "PGSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "FK_DEPEND";
                find.DBType = "PGSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: искать в foreign key Postgres (что зависит от таблицы)";
            find.FieldTable = "tablename";
            find.Text = @"
with cte as (
SELECT
  o.conname AS constraint_name,
  (SELECT nspname FROM pg_namespace WHERE oid=m.relnamespace) AS schema_name,
  m.relname AS table_name,
  (SELECT nspname FROM pg_namespace WHERE oid=m.relnamespace)||'.'|| m.relname AS source_table,
  o.conkey[1] AS source_column_order,
  (SELECT a.attname FROM pg_attribute a WHERE a.attrelid = m.oid AND a.attnum = o.conkey[1] AND a.attisdropped = false) AS source_column,
  (SELECT nspname FROM pg_namespace WHERE oid=f.relnamespace)||'.'|| f.relname AS target_table,
  o.confkey[1] AS traget_column_order,
  (SELECT a.attname FROM pg_attribute a WHERE a.attrelid = f.oid AND a.attnum = o.confkey[1] AND a.attisdropped = false) AS target_column
FROM
  pg_constraint o 
  LEFT JOIN pg_class f ON f.oid = o.confrelid 
  LEFT JOIN pg_class m ON m.oid = o.conrelid
WHERE
  o.contype = 'f' AND o.conrelid IN (SELECT oid FROM pg_class c WHERE c.relkind IN ('r','f','p'))
)
select
    constraint_name as foreignkeyname, 
    source_table as tablename,
    obj_description(to_regclass(quote_ident(cte.schema_name) || '.' || quote_ident(cte.table_name)), 'pg_class') as tabledescription,
    source_column as columnname,
    col_description(to_regclass(quote_ident(cte.schema_name) || '.' || quote_ident(cte.table_name)), source_column_order) AS columndescription,
    target_table as searchname
from cte
where target_table iLIKE '%%SEARCH%%'
ORDER BY source_table
;
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "FK_ONDEPEND") && (x.DBType == "PGSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "FK_ONDEPEND";
                find.DBType = "PGSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: искать в foreign key Postgres (от чего зависит таблица)";
            find.FieldTable = "tablename";
            find.Text = @"
with cte as (
SELECT
  o.conname AS constraint_name,
  (SELECT nspname FROM pg_namespace WHERE oid=m.relnamespace) AS source_schema_name,
  m.relname AS source_table_name,
  (SELECT nspname FROM pg_namespace WHERE oid=m.relnamespace)||'.'|| m.relname AS source_table,
  o.conkey[1] AS source_column_order,
  (SELECT a.attname FROM pg_attribute a WHERE a.attrelid = m.oid AND a.attnum = o.conkey[1] AND a.attisdropped = false) AS source_column,
  (SELECT nspname FROM pg_namespace WHERE oid=f.relnamespace) AS target_schema_name,
  f.relname AS target_table_name,
  (SELECT nspname FROM pg_namespace WHERE oid=f.relnamespace)||'.'|| f.relname AS target_table,
  o.confkey[1] AS traget_column_order,
  (SELECT a.attname FROM pg_attribute a WHERE a.attrelid = f.oid AND a.attnum = o.confkey[1] AND a.attisdropped = false) AS target_column
FROM
  pg_constraint o 
  LEFT JOIN pg_class f ON f.oid = o.confrelid 
  LEFT JOIN pg_class m ON m.oid = o.conrelid
WHERE
  o.contype = 'f' AND o.conrelid IN (SELECT oid FROM pg_class c WHERE c.relkind IN ('r','f','p'))
)
select
    constraint_name as foreignkeyname, 
    source_table as searchname,
    source_column as columnname,
    col_description(to_regclass(quote_ident(cte.source_schema_name) || '.' || quote_ident(cte.source_table_name)), source_column_order) AS columndescription,
    target_table as tablename,
    obj_description(to_regclass(quote_ident(cte.target_schema_name) || '.' || quote_ident(cte.target_table_name)), 'pg_class') as tabledescription
from cte
where source_table iLIKE '%%SEARCH%%'
ORDER BY source_column_order
;
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "ALTEROBJECTLOG") && (x.DBType == "MSSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "ALTEROBJECTLOG";
                find.DBType = "MSSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: Поиск в AlterObjectLog Microsoft SQL";
            find.FieldText = "AlterObjectLog_CommandText";
            find.Text = @"
SELECT TOP (100) *
FROM AlterObjectLog
WHERE alterobjectlog_schemaname + '.' + alterobjectlog_objectname LIKE '%%%SEARCH%%%'
ORDER BY alterobjectlog_insdt desc
";

            find = ListSearches.Find(x => (x.Type == "SQL") && (x.Tag == "ALTEROBJECTLOG") && (x.DBType == "PGSQL"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Tag = "ALTEROBJECTLOG";
                find.DBType = "PGSQL";
                find.Type = "SQL";
                ListSearches.Add(find);
            }
            find.Name = "БД: Поиск в AlterObjectLog Postgres";
            find.FieldText = "alterobjectlog_commandtext";
            find.Text = @"
SELECT *
FROM AlterObjectLog
WHERE alterobjectlog_schemaname || '.' || alterobjectlog_objectname iLIKE '%%%SEARCH%%%'
ORDER BY alterobjectlog_insdt desc
limit 1000
";

            find = ListSearches.Find(x => (x.Type == "GIT") && (x.Tag == "STRUCTNAME"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Type = "GIT";
                find.Tag = "STRUCTNAME";
                ListSearches.Add(find);
            }
            find.Name = "GIT: искать в названиях скриптов";

            find = ListSearches.Find(x => (x.Type == "GIT") && (x.Tag == "STRUCTCONTENT"));
            if (find == null)
            {
                find = new SearchInfo();
                find.Type = "GIT";
                find.Tag = "STRUCTCONTENT";
                ListSearches.Add(find);
            }
            find.Name = "GIT: искать в содержимом скриптов";
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Сохранить список поисковых запросов</summary>
        public static void SaveSearches()
        {
            string filename = Path.Combine(App.AppPath, "ListSearches.json");
            string jsonString = "";

            Utilities.Files.BackupFile(filename);

            try
            {
                var list = new List<SearchInfo>();
                foreach (var conn in ListSearches)
                {
                    var item = conn.Copy();
                    list.Add(item);
                }

                jsonString = JsonSerializer.Serialize<List<SearchInfo>>(list, Other.OptionsJSON);
                File.WriteAllText(filename, jsonString);
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
        }


    }




    /// <summary>
    /// Информация о поисковых запросах
    /// </summary>
    public class SearchInfo
    {
        /// <summary>
        /// Название поискового запроса
        /// </summary>
        public string Name { get; set; }

        string _tag;
        /// <summary>
        ///  Уникальный тег поискового запроса
        /// </summary>
        public string Tag
        {
            get
            {
                return _tag ?? "UNKNOWN";
            }
            set
            {
                _tag = value;
                if (string.IsNullOrWhiteSpace(_tag)) _tag = "UNKNOWN";
                _tag = _tag.Trim().ToUpper();
            }
        }

        string _searchtype;
        /// <summary>Тип поискового запроса</summary>
        public string Type
        {
            get
            {
                return (_searchtype ?? "SQL");
            }
            set
            {
                _searchtype = value.Trim().ToUpper();
                if (_searchtype == "GIT")
                {
                }
                else
                {
                    _searchtype = "SQL";
                }
            }
        }

        string _dbtype;
        /// <summary>Тип БД</summary>
        public string DBType
        {
            get
            {
                return (_dbtype ?? "PGSQL");
            }
            set
            {
                _dbtype = value.Trim().ToUpper();
                if (_dbtype == "MSSQL")
                {
                }
                else
                {
                    _dbtype = "PGSQL";
                }
            }
        }

        /// <summary>
        /// Тип соединения
        /// </summary>
        public Utilities.ConnType ConnType
        {
            get
            {
                if (DBType == "MSSQL")
                {
                    return Utilities.ConnType.MSSQL;
                }
                else
                {
                    return Utilities.ConnType.PGSQL;
                }
            }
        }

        /// <summary>
        /// Текст поискового запроса
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Поле из результата, в котором содержится текст для просмотра
        /// </summary>
        public string FieldText { get; set; }

        /// <summary>
        /// Поле из результата, в котором содержится имя таблицы
        /// </summary>
        public string FieldTable { get; set; }

        /// <summary>
        /// Копирование экземпляра SearchInfo
        /// </summary>
        /// <returns></returns>
        public SearchInfo Copy()
        {
            return (SearchInfo)this.MemberwiseClone();
        }
    }
}
