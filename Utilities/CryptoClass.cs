// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Security.Cryptography;
using SQLGen.Utilities;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>Класс CryptoClass для шифрации/дешифрации строк</summary>
    public static class CryptoClass
    {
        // Create byte array for additional entropy when using Protect method.
        static readonly byte[] s_aditionalEntropy = { 9, 2, 3, 7, 5 };

        /// <summary>
        /// Шифрация в байтовый массив
        /// </summary>
        /// <param name="secret">исходная строка</param>
        /// <returns></returns>
        public static byte[] encrypt_to_bytes(string secret)
        {
            if (secret == null) secret = "";

            System.Text.UTF8Encoding Byte_Transform = new System.Text.UTF8Encoding();

            //Just grabbing the bytes since most crypto functions need bytes.
            byte[] bytes = Byte_Transform.GetBytes(secret);

            //Encrypt the data.
            return ProtectedData.Protect(bytes, s_aditionalEntropy, DataProtectionScope.CurrentUser);
        }

        /// <summary>
        /// Шифрация в строку
        /// </summary>
        /// <param name="secret">исходная строка</param>
        /// <returns></returns>
        public static string encrypt_to_string(string secret)
        {
            if ((secret == null) || (secret == "")) return "";
            byte[] bytes = encrypt_to_bytes(secret);
            return Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);
        }

        /// <summary>
        /// Дешифровка байтового массива
        /// </summary>
        /// <param name="crypted_bytes">зашифрованный байтовый массив</param>
        /// <returns></returns>
        public static string decrypt_from_bytes(byte[] crypted_bytes)
        {
            byte[] bytes = ProtectedData.Unprotect(crypted_bytes, s_aditionalEntropy, DataProtectionScope.CurrentUser);

            System.Text.UTF8Encoding Byte_Transform = new System.Text.UTF8Encoding();

            return Byte_Transform.GetString(bytes);
        }

        /// <summary>
        /// Дешифровка строки
        /// </summary>
        /// <param name="crypted">зашифрованная строка</param>
        /// <returns></returns>
        public static string decrypt_from_string(string crypted)
        {
            if ((crypted == null) || (crypted == "")) return "";
            byte[] bytes = Convert.FromBase64String(crypted);
            return decrypt_from_bytes(bytes);
        }

    }
}
