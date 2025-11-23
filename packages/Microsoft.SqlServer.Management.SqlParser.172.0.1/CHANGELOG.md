# Microsoft.SqlServer.Management.SqlParser update history

- Replace recursion with a stack to eliminate stack overflow exceptions when parsing complex queries
- Add new database level permission ALTER ANY EXTERNAL MIRROR
- Add `net8.0` binary
- Fix error text for invalid data type in system scalar functions
- Add Parser support for Graph edge constraints
- Add IntelliSense support for Json Aggregator (Json_arrayagg and Json_objectagg builtins) with Json Null Qualifier syntax
- Add IntelliSense support for UNISTR built-in function
- Add FORCESCAN table hint keyword. Fix keyword coloring for FORCESEEK and SPATIAL_WINDOW_MAX_CELLS table hints.
- Add IntelliSense support for SUBSTRING with 2 arguments (optional 3rd argument)

## 170.9.0

- Handle system columns in INSERT

## 170.7.0

- Replace `netcoreapp3.1` binary with `net6.0`
- Merge the localized resource DLLs into the same nuget package as the code binary
- Add support for INLINE option on CREATE/ALTER FUNCTION
- Add support for ODBC DateTime syntax
- Add Base64 builtin functions
- Add DATABASEPROPERTY and VERSION builtin functions
- Add CHANGE, TRACKING and MEMBER keywords
- Fix CREATE/ALTER table index option checks

## 160.22524.0

- Add support for `TRIM` variant that has 3 arguments

## 160.22523.0

- Add missing permissions to database permissions enumeration

## 160.22522.0

- Add permission ALTER ANY DATABASE EVENT SESSION
- Add support for additional parameter to `isjson`
- Add support for SQL 19 and SQL 22 database permissions and IntelliSense support for permissions with more than 5 words

## 160.22521.0

- Add support for GENERATE_SERIES relational operator
- Added IntelliSense support for Json Constructors (Json_object and Json_array builtins) with Json Null Qualifier syntax
- Updated CREATE DATABASE to recognize BACKUP_STORAGE_REDUNDANCY as an option for AS COPY OF
- Updated ALTER DATABASE to recognize BACKUP_STORAGE_REDUNDANCY as an option for ADD SECONDARY ON SERVER
- Add parsing of `SID` parameter for `CREATE USER`
- Added support for Parameter Sensitive Plan Optimization (https://docs.microsoft.com/en-us/sql/relational-databases/performance/parameter-sensitivity-plan-optimization?view=sql-server-ver16)
- Added IntelliSense support for LTRIM with 2 arguments.
- Added support for External Language
- Added support for External Stream
- Implemented the concept of context based colorization [CREATE EXTERNAL STREAM](https://docs.microsoft.com/en-us/azure/azure-sql-edge/create-external-stream-transact-sql)
- Updated External file format and External data source to show the right colors when used as keywords
- IntelliSense support for Greatest and Least built-in functions
- Add NetCoreApp3.1 build
- Intellisense changes for built-in Date_Bucket
- Major version rev to 160
- Recognize BACKUP_STORAGE_REDUNDANCY in [CREATE DATABASE](https://docs.microsoft.com/sql/t-sql/statements/create-database-transact-sql)
  and [ALTER DATABASE](https://docs.microsoft.com/sql/t-sql/statements/alter-database-transact-sql)
- Updated ALTER DATABASE to recognize ASYNC in FORCE_FAILOVER_ALLOW_DATA_LOSS and FAILOVER.
- Updated ALTER DATABASE to recognize DATABASE_NAME and SECONDARY_TYPE as options for ADD SECONDARY ON SERVER.
- Added support for ledger syntax in [CREATE TABLE](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql)

## 150.37051.0

- Recognize [ACCELERATED_DATABASE_RECOVERY](https://docs.microsoft.com/sql/relational-databases/accelerated-database-recovery-management?view=sql-server-ver15) syntax
