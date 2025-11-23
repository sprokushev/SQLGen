// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SQLGen.Utilities
{
    /// <summary>
    /// Вспомогательные функции для задач (Task)
    /// </summary>
    public static class Task
    {
        /// <summary>регулярное выражение для номера задачи</summary>
        public static Regex regex_task = new Regex(@"^(promedweb|rpms|ops|smp|rm|cm|ferdtm|bip|pharmacy1c)-(\d+)(_?)(\w*)", RegexOptions.IgnoreCase);

        /// <summary>
        /// вернуть номер задачи
        /// </summary>
        /// <param name="_task">строка, содержащая номер задачи</param>
        /// <returns>номер задачи</returns>
        public static string GetTaskNumber(string _task)
        {
            /*if (_task.Contains("124302"))
            {
                int test = 0;
            }*/

            Match m = regex_task.Match(_task);

            if (m.Success)
            {
                string result = (m.Value ?? "").Trim().TrimEnd('_').Trim();
                return result;
            }
            else
            {
                return "";
            }
        }
    }
}
