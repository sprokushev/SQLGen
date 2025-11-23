# Microsoft.SqlServer.Management.SqlParser

This assembly provides basic syntax parsing and binding for the TSQL language for many versions and editions of Microsoft SQL Server and Azure SQL Database.

Localized resource DLLs are provided in a [companion package](https://www.nuget.org/packages/Microsoft.SqlServer.Management.SqlParser.loc)

## Getting started

For basic syntax parsing of a TSQL fragment, use 

```C#
public static Microsoft.SqlServer.Management.SqlParser.Parser.ParseResult Parse (string sql, Microsoft.SqlServer.Management.SqlParser.Parser.ParseOptions options, out Microsoft.SqlServer.Management.SqlParser.Parser.ParseOptions resultOptions);
```
