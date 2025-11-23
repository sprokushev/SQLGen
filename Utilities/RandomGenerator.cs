// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Text;
using SQLGen.Utilities;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>Класс RandomGenerator для генерации случайных числе и строк</summary>
    public class RandomGenerator
    {
        private readonly Random _random = new Random();

        /// <summary>
        /// Generates a random number within a range
        /// </summary>
        /// <param name="min">минимальное значение</param>
        /// <param name="max">максимальное значение</param>
        /// <returns></returns>
        public int RandomNumber(int min, int max)
        {
            return _random.Next(min, max);
        }

        /// <summary>
        /// Generates a random string with a given size
        /// </summary>
        /// <param name="size">размер строки</param>
        /// <param name="lowerCase">=true - результат в нижнем регистре</param>
        /// <returns></returns>
        public string RandomString(int size, bool lowerCase = false)
        {
            var builder = new StringBuilder(size);

            // Unicode/ASCII Letters are divided into two blocks
            // (Letters 65–90 / 97–122):   
            // The first group containing the uppercase letters and
            // the second group containing the lowercase.  

            // char is a single Unicode character  
            char offset = lowerCase ? 'a' : 'A';
            const int lettersOffset = 26; // A...Z or a..z: length = 26  

            for (var i = 0; i < size; i++)
            {
                var @char = (char)_random.Next(offset, offset + lettersOffset);
                builder.Append(@char);
            }

            return lowerCase ? builder.ToString().ToLower() : builder.ToString();
        }

        /// <summary>
        /// Generates a random password. 4-LowerCase + 4-Digits + 2-UpperCase
        /// </summary>
        /// <returns></returns>
        public string RandomPassword()
        {
            var passwordBuilder = new StringBuilder();

            // 4-Letters lower case   
            passwordBuilder.Append(RandomString(4, true));

            // 4-Digits between 1000 and 9999  
            passwordBuilder.Append(RandomNumber(1000, 9999));

            // 2-Letters upper case  
            passwordBuilder.Append(RandomString(2));
            return passwordBuilder.ToString();
        }
    }
}
