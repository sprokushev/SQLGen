// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLGen.Utilities
{
    /// <summary>
    /// Вспомогательные функции для работы с реестром Windows
    /// </summary>
    public static class Registry
    {
        /// <summary>
        /// Прочитать из реестра значение ключа
        /// </summary>
        /// <param name="keyName">keyName</param>
        /// <param name="valueName">valueName</param>
        /// <param name="defaultValue">defaultValue</param>
        /// <returns></returns>
        public static string GetRegistryValue(string keyName, string valueName, object defaultValue)
        {
            return (string)Microsoft.Win32.Registry.GetValue(keyName, valueName, defaultValue);
        }

        /// <summary>
        /// Записать в реестр значение ключа
        /// </summary>
        /// <param name="keyName">keyName</param>
        /// <param name="valueName">valueName</param>
        /// <param name="value">value</param>
        public static void SetRegistryValue(string keyName, string valueName, string value)
        {
            Microsoft.Win32.Registry.SetValue(keyName, valueName, value);
        }
    }
}
