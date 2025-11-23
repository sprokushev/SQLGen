drop table if exists tmp_psv_inf;
create temp table tmp_psv_inf as 
select 
    t.table_schema,
    t.table_name,
    c.ordinal_position as field_order,
    c.column_name as field_name,
    col_description(to_regclass(quote_ident(t.table_schema) || '.' || quote_ident(t.table_name)), c.ordinal_position) as field_desc,
    Upper(
        case 
            when c.data_type = 'character' or c.data_type = '""char""' or c.data_type = 'bpchar' then 'char'
            when c.data_type = 'character varying' then 'varchar'
            else c.data_type
        end
    ) as field_type,
    case 
        when c.data_type = 'bit' or c.data_type = 'character' or c.data_type = 'character varying' or c.data_type = '""char""' then cast(c.character_maximum_length as varchar)
        when c.data_type = 'numeric' then cast(c.numeric_precision as varchar)
        else ''
    end as field_size,
    case 
        when c.data_type = 'numeric' then cast(c.numeric_scale as varchar)
        else ''
    end as field_dec,
    case when c.is_nullable = 'YES' then 'false' else 'true' end as IsNotNull,
    case when c.is_identity = 'YES' then 'true' else 'false' end as IsIdentity,
    c.column_default as field_default
from information_schema.tables t
inner join information_schema.columns c on c.table_schema = t.table_schema and c.table_name = t.table_name
where t.table_schema not in ('pg_catalog','information_schema')
and t.table_type IN ('BASE TABLE','FOREIGN')
;

drop table if exists tmp_psv_attr;
create temp table tmp_psv_attr as
Select 
  n.nspname as table_schema,
  t.relname as table_name,
    c.attnum as field_order,
    c.attname as field_name,
    col_description(t.oid, c.attnum) as field_desc,

    case 
      when typename in ('BPCHAR','CHARACTER') then 'CHAR'
      when typename in ('CHARACTER VARYING') then 'VARCHAR'
      else typename
     end as field_type,

    case 
      when typename in ('TIMESTAMP WITH TIME ZONE','TIMESTAMP WITHOUT TIME ZONE','TIME WITHOUT TIME ZONE','TIME WITH TIME ZONE','TIME','TIMESTAMP', 'TIMESTAMPTZ','INTERVAL') then ''
      else precision 
     end as field_size,
     scale as field_dec,
    
    case when c.attnotnull then 'true' else 'false' end as IsNotNull,
    case when c.attidentity = '' or c.attidentity is null then 'false' else 'true' end as IsIdentity,
    def.definition as field_default
FROM pg_class t 
INNER JOIN pg_catalog.pg_namespace n ON n.oid = t.relnamespace
INNER JOIN pg_catalog.pg_attribute c ON c.attrelid = t.oid and c.attnum > 0 and NOT c.attisdropped
inner join lateral (
  select 
    typeinfo,
    regexp_replace(typeinfo, '\([^)]*\)', '', 'g') as typename,
    split_part(
    CASE 
        WHEN typeinfo ~ '\(.*\)' THEN regexp_replace(typeinfo, '.*\((.*)\).*', '\1') 
        ELSE ''
    END, ',', 1) as precision,

    split_part(
    CASE 
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
left join lateral (
  select pg_get_expr(d.adbin, d.adrelid) as definition
  from pg_attrdef d
  where d.adrelid = c.attrelid
  and d.adnum = c.attnum
) def on true
WHERE 1=1
and t.relkind in ('r', 'f', 'p')
and n.nspname not in ('pg_catalog','information_schema')
;


select *
from tmp_psv_attr attr
inner join tmp_psv_inf inf on inf.table_schema = attr.table_schema
  and inf.table_name = attr.table_name
  and inf.field_name = attr.field_name
where 1=1
and (
coalesce(attr.field_order,-1) <> coalesce(inf.field_order,-1)
or coalesce(attr.field_desc,'') <> coalesce(inf.field_desc,'')
or coalesce(attr.field_type,'') <> coalesce(inf.field_type,'')
or coalesce(attr.field_size,'') <> coalesce(inf.field_size,'')
or coalesce(attr.field_dec,'') <> coalesce(inf.field_dec,'')
or coalesce(attr.IsNotNull,'') <> coalesce(inf.IsNotNull,'')
or coalesce(attr.IsIdentity,'') <> coalesce(inf.IsIdentity,'')
or coalesce(attr.field_default,'') <> coalesce(inf.field_default,'')
)
order by attr.table_schema, attr.table_name
limit 100
