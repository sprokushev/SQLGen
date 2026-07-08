// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using ExcelDataReader;
using ICSharpCode.AvalonEdit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;


namespace SQLGen.Utilities
{
    // =========================================================================================================
    /// <summary>Прочие вспомогательные функции</summary>
    public static class Other
    {
        /// <summary>
        /// принимать действие action и применять его ко всем элементам массива array
        /// Примеры: 
        ///     ForEach(myarray, (int x) => { Console.WriteLine("Элемент: " + x); });
        ///     ForEach(myarray, x => Console.WriteLine("Элемент: " + x));
        /// </summary>
        /// <param name="array">массив</param>
        /// <param name="action">действие</param>
        public static void ForEach(int[] array, Action<int> action)
        {
            for (int i = 0; i < array.Length; ++i)
                action(array[i]);
        }

        /// <summary>
        /// принимать действие action и применять его ко всем элементам массива array
        /// Примеры: 
        ///     ForEach(myarray, (string x) => { Console.WriteLine("Элемент: " + x); });
        ///     ForEach(myarray, x => Console.WriteLine("Элемент: " + x));
        /// </summary>
        /// <param name="array">массив</param>
        /// <param name="action">действие</param>
        public static void ForEach(string[] array, Action<string> action)
        {
            for (int i = 0; i < array.Length; ++i)
                action(array[i]);
        }

        /// <summary>
        /// принимать действие action и применять его ко всем элементам списка list
        /// Примеры: 
        ///     ForEach(mylist, (string x) => { Console.WriteLine("Элемент: " + x); });
        ///     ForEach(mylist, x => Console.WriteLine("Элемент: " + x));
        /// </summary>
        /// <param name="list">список</param>
        /// <param name="action">действие</param>
        public static void ForEach(List<string> list, Action<string> action)
        {
            for (int i = 0; i < list.Count; ++i)
                action(list[i]);
        }

        /// <summary>
        /// Параметры сериализации на json-файлов настроек SQLGen (устареший)
        /// </summary>
        public static readonly JsonSerializerOptions oldOptionsJSON = new JsonSerializerOptions
        {
            IgnoreReadOnlyProperties = true,
            WriteIndented = true
        };

        /// <summary>
        /// Параметры сериализации на json-файлов настроек SQLGen
        /// </summary>
        public static readonly JsonSerializerOptions OptionsJSON = new JsonSerializerOptions
        {
            IgnoreReadOnlyProperties = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// параметры сериализации для version\xxx_cron.json (с пустыми полями)
        /// </summary>
        public static readonly JsonSerializerOptions VersionJSON = new JsonSerializerOptions
        {
            IgnoreReadOnlyProperties = true,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// параметры сериализации для PROMEDWEB-xxx.json (без пустых полей)
        /// </summary>
        public static readonly JsonSerializerOptions TaskJSON = new JsonSerializerOptions
        {
            IgnoreReadOnlyProperties = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }
}
