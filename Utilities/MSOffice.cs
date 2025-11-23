// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.Office.Interop.Excel;

namespace SQLGen.Utilities
{
    /// <summary>
    /// Вспомогательные функции для работы с MS Office
    /// </summary>
    public static class MSOffice
    {
        /// <summary>Список ключевых слов в excel, которые означают автогенерацию по максимальному значению</summary>
        public static List<string> List_LoadIdentity = new List<string>
            {
            { "identity" }
            };

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Считать страницу excel в DataTable</summary>
        /// <param name="Connect">Подключение к БД</param>
        /// <param name="tablename">Таблица</param>
        /// <param name="xlsFile">Полное имя файла</param>
        /// <param name="sheetNum">Номер страницы в книге</param>
        /// <param name="isTypeRow">=true - есть строка с типами полей</param>
        public static System.Data.DataTable LoadExcel(ConnectDB Connect, string tablename, string xlsFile, int sheetNum, bool isTypeRow)
        {
            System.Data.DataTable shablon = null; // информация о структуре таблицы из БД

            if ((Connect != null) && (tablename != ""))
            {
                try
                {
                    if (Connect.ConnType == Utilities.ConnType.MSSQL)
                    {
                        shablon = Connect.FillDataTable("select top(1) * from " + tablename, out string Messages);
                    }
                    if (Connect.ConnType == Utilities.ConnType.PGSQL)
                    {
                        shablon = Connect.FillDataTable("select * from " + tablename + " limit 1", out string Messages);
                    }
                }
                catch
                {
                    shablon = null;
                }
            }

            if ((shablon == null) && (!isTypeRow))
            {
                App.AddLog("В БД отсутствует таблица " + tablename + ", а для excel-файла " + xlsFile + " не указана строка с типами !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                return shablon;
            }

            // считываем excel-таблицу в DataSet
            System.Data.DataTable Excel = new System.Data.DataTable();

            using (var stream = File.Open(xlsFile, FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader reader;
                reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);
                var conf = new ExcelDataSetConfiguration
                {
                    ConfigureDataTable = _ => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                };

                var dataSet = reader.AsDataSet(conf);

                // Now you can get data from each sheet by its index or its "name"  
                if (
                    (dataSet != null) &&
                    (dataSet.Tables.Count >= sheetNum)
                    )
                {
                    Excel = dataSet.Tables[sheetNum - 1];
                }
            }

            if (
                (Excel == null) ||
                (Excel.Rows.Count < 1) ||
                (Excel.Columns.Count < 1)
                )
            {
                App.AddLog("Excel-файл " + xlsFile + " не был прочитан успешно или он пустой) !", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                shablon = null;
                return shablon;
            }

            // целевая таблица
            System.Data.DataTable dataTable = null;
            int startRow = 0;
            bool isExistInsID = false;
            bool isExistUpdID = false;
            bool isExistInsDT = false;
            bool isExistUpdDT = false;
            bool isExistInsDTTZ = false;
            bool isExistUpdDTTZ = false;

            try
            {
                if ((dataTable == null) && (shablon != null) && (shablon.Columns.Count > 0) && (!isTypeRow)) //-V3063
                {
                    // таблица есть в БД, а в excel нет информации о типах, возьмем из БД

                    dataTable = new System.Data.DataTable(tablename);

                    foreach (DataColumn col0 in Excel.Columns)
                    {
                        string columnname = col0.ColumnName;
                        if (columnname != "")
                        {
                            isExistInsID = isExistInsID || columnname.ToLower() == "pmuser_insid";
                            isExistUpdID = isExistUpdID || columnname.ToLower() == "pmuser_updid";
                            isExistInsDT = isExistInsDT || columnname.ToLower().EndsWith("_insdt");
                            isExistUpdDT = isExistUpdDT || columnname.ToLower().EndsWith("_upddt");
                            isExistInsDTTZ = isExistInsDTTZ || columnname.ToLower().EndsWith("_insdttz");
                            isExistUpdDTTZ = isExistUpdDTTZ || columnname.ToLower().EndsWith("_upddttz");

                            var column = new DataColumn(columnname);
                            foreach (DataColumn item in shablon.Columns)
                            {
                                if (item.ColumnName.ToLower() == columnname.ToLower())
                                {
                                    // взять тип поля из БД
                                    column.DataType = item.DataType;
                                    dataTable.Columns.Add(column);
                                    break;
                                }
                            }
                        }
                    }

                    startRow = 0;
                }

                if ((dataTable == null) && isTypeRow)
                {
                    // в excel есть информация о типах и именах полей

                    dataTable = new System.Data.DataTable(tablename);

                    // считать из excel строки с именами полей и типами полей
                    foreach (DataColumn col0 in Excel.Columns)
                    {
                        DataRow typ1 = Excel.Rows[0];

                        string columnname = col0.ColumnName;
                        string typename = typ1[col0].ToString().Trim();

                        if (
                            (columnname != "") &&
                            (typename != "")
                            )
                        {
                            isExistInsID = isExistInsID || columnname.ToLower() == "pmuser_insid";
                            isExistUpdID = isExistUpdID || columnname.ToLower() == "pmuser_updid";
                            isExistInsDT = isExistInsDT || columnname.ToLower().EndsWith("_insdt");
                            isExistUpdDT = isExistUpdDT || columnname.ToLower().EndsWith("_upddt");
                            isExistInsDTTZ = isExistInsDTTZ || columnname.ToLower().EndsWith("_insdttz");
                            isExistUpdDTTZ = isExistUpdDTTZ || columnname.ToLower().EndsWith("_upddttz");

                            var column = new DataColumn(columnname);
                            column.DataType = Utilities.Databases.ConvertType(typename);
                            dataTable.Columns.Add(column);
                        }
                    }

                    startRow = 1;
                }

                // Добавляем insid, updid, insdt, upddt, insdttz, upddttz, если они есть в таблице БД
                if (
                    (shablon != null) &&
                    (shablon.Columns.Count > 0) &&
                    (dataTable != null)
                    )
                {
                    foreach (DataColumn item in shablon.Columns)
                    {
                        string columnname = item.ColumnName;

                        if (
                            (!isExistInsID) &&
                            (columnname.ToLower() == "pmuser_insid")
                            )
                        {
                            var column = new DataColumn(columnname);
                            column.DataType = item.DataType;
                            dataTable.Columns.Add(column);
                        }

                        if (
                            (!isExistUpdID) &&
                            (columnname.ToLower() == "pmuser_updid")
                            )
                        {
                            var column = new DataColumn(columnname);
                            column.DataType = item.DataType;
                            dataTable.Columns.Add(column);
                        }

                        if (
                            (!isExistInsDT) &&
                            columnname.ToLower().EndsWith("_insdt")
                            )
                        {
                            var column = new DataColumn(columnname);
                            column.DataType = item.DataType;
                            dataTable.Columns.Add(column);
                        }

                        if (
                            (!isExistUpdDT) &&
                            columnname.ToLower().EndsWith("_upddt")
                            )
                        {
                            var column = new DataColumn(columnname);
                            column.DataType = item.DataType;
                            dataTable.Columns.Add(column);
                        }

                        if (
                            (!isExistInsDTTZ) &&
                            columnname.ToLower().EndsWith("_insdttz")
                            )
                        {
                            var column = new DataColumn(columnname);
                            column.DataType = item.DataType;
                            dataTable.Columns.Add(column);
                        }

                        if (
                            (!isExistUpdDTTZ) &&
                            columnname.ToLower().EndsWith("_upddttz")
                            )
                        {
                            var column = new DataColumn(columnname);
                            column.DataType = item.DataType;
                            dataTable.Columns.Add(column);
                        }
                    }
                }

                // Копируем данные
                if (dataTable != null)
                {
                    int maxValue = 0;

                    for (int i = startRow; i < Excel.Rows.Count; i++)
                    {
                        // новая строка
                        DataRow row = dataTable.NewRow();

                        foreach (DataColumn col in dataTable.Columns)
                        {
                            string value = "";

                            // найти колонку в excel
                            if (Excel.Columns.Contains(col.ColumnName))
                            {
                                DataColumn col0 = Excel.Columns[col.ColumnName];

                                // найти значение в excel
                                value = Excel.Rows[i][col0].ToString().Trim();
                            }

                            // реализация автоинкремента
                            if (List_LoadIdentity.Contains(value.Trim(), StringComparer.OrdinalIgnoreCase) && (
                                    (col.DataType == Type.GetType("System.Int32")) ||
                                    (col.DataType == Type.GetType("System.Int64"))
                               ))
                            {
                                if ((maxValue == 0) && (shablon != null))
                                {
                                    // считываем из БД максимальное значение
                                    string queryString = "select max(" + col.ColumnName + ") from " + tablename + " where " + col.ColumnName + " < 20000000";

                                    using (DbDataReader reader = Connect.OpenQuery(queryString))
                                    {
                                        if (reader != null)
                                        {
                                            while (reader.Read())
                                            {
                                                string s = reader[0].ToString();
                                                if (string.IsNullOrWhiteSpace(s)) s = "0";
                                                else if (s.Trim().ToLower() == "null") s = "0";
                                                maxValue = int.Parse(s);
                                                break; //-V3020
                                            }
                                        }
                                    }
                                }
                                // автоинкремент
                                maxValue++;
                                value = maxValue.ToString();
                            }

                            // заполним значение
                            if (string.IsNullOrWhiteSpace(value)) row[col] = DBNull.Value;
                            else if (value.Trim().ToLower() == "null") row[col] = DBNull.Value;
                            else row[col] = value;
                        }

                        dataTable.Rows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                dataTable = null;
            }

            if ((dataTable != null) && (dataTable.Columns.Count > 0) && (dataTable.Rows.Count > 0))
            {
                App.AddLog($"Файл {xlsFile} загружен!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }
            else
            {
                App.AddLog($"Файл {xlsFile} НЕ загружен!", null, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
            }

            return dataTable;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Считать значение ячейки из excel</summary>
        /// <param name="xlsfile">Полное имя файла</param>
        /// <param name="sheetnum">Номер страницы в книге</param>
        /// <param name="column">Столбец</param>
        /// <param name="row">Столбец</param>
        public static string GetValueExcel(string xlsfile, int sheetnum, int row, int column)
        {
            string result = "";

            Application excelApp = null;
            Workbook excelWorkBook = null;
            try
            {
                excelApp = new Application();
                excelApp.Visible = false;
                excelWorkBook = excelApp.Workbooks.Open(xlsfile, null, true);
                Worksheet excelWorkSheet = excelWorkBook.Sheets[sheetnum];

                // считать из excel ячейку
                result = excelWorkSheet.Cells[row, column].Text;
                result = result.Trim();
            }
            catch (Exception ex)
            {
                App.AddLog("", ex, App.ShowMessageMode.SHOW, true, MainWindow.Task.LogFile);
                result = "";
            }

            if (excelWorkBook != null) excelWorkBook.Close();
            if (excelApp != null) excelApp.Quit();

            return result;
        }

        // -------------------------------------------------------------------------------------------------------
        /// <summary>Сохранить DataTable в excel</summary>
        /// <param name="dataTable">DataTable</param>
        /// <param name="isSave">=true - сохранить в файл</param>
        /// <param name="path">Полное имя файла</param>
        /// <param name="isShow">=true - оставить excel открытым</param>
        public static void GenerateExcel(System.Data.DataTable dataTable, bool isSave, string path, bool isShow)
        {
            DataSet dataSet = new DataSet();
            dataSet.Tables.Add(dataTable);

            // create a excel app along side with workbook and worksheet and give a name to it
            Application excelApp = new Application();
            Workbook excelWorkBook = excelApp.Workbooks.Add();
            _Worksheet xlWorksheet = excelWorkBook.Sheets[1];
            Range xlRange = xlWorksheet.UsedRange;
            foreach (System.Data.DataTable table in dataSet.Tables)
            {
                //Add a new worksheet to workbook with the Datatable name
                Worksheet excelWorkSheet = excelWorkBook.Sheets.Add();
                excelWorkSheet.Name = table.TableName;

                // add all the columns
                for (int i = 1; i < table.Columns.Count + 1; i++)
                {
                    excelWorkSheet.Cells[1, i] = table.Columns[i - 1].ColumnName;
                }

                // add all the rows
                for (int j = 0; j < table.Rows.Count; j++)
                {
                    for (int k = 0; k < table.Columns.Count; k++)
                    {
                        excelWorkSheet.Cells[j + 2, k + 1] = table.Rows[j].ItemArray[k].ToString();
                    }
                }
            }
            if (isSave) excelWorkBook.SaveAs(path);

            if (!isShow)
            {
                excelWorkBook.Close();
                excelApp.Quit();
            }
            else excelApp.Visible = true;
        }

    }
}
