// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLGen
{
    // =========================================================================================================
    /// <summary>Класс Версия</summary>
    public class Version
    {
        /// <summary>
        /// конструктор Version
        /// </summary>
        /// <param name="_logfile">полный путь к лог-файлу. Если пустой, значит в App.AppLogFile</param>
        public Version(string _logfile)
        {
            YMLFile = new YMLStruct(null, _logfile);
        }

        /// <summary>Ветка версии</summary>
        public string Branch { get; set; } = "";

        /// <summary>Файл версии</summary>
        public YMLStruct YMLFile { get; set; }

        /// <summary>
        /// =true - ветка существует
        /// </summary>
        public bool isBranchExists { get; set; } = false;

        /// <summary>Отображаемое имя</summary>
        public string VisibleName
        {
            get
            {
                if (Utilities.GITProjects.IsDEVProject(YMLFile?.Project)) return Branch ?? "";
                else return File;
            }
        }

        /// <summary>
        /// Номер версии
        /// </summary>
        public string Num => YMLFile?.NumVersion ?? "";

        /// <summary>
        /// Номер версии для сортировки
        /// </summary>
        public double NumOrder => YMLFile?.NumVersionOrder ?? 0;

        /// <summary>Имя файла с версией</summary>
        public string File => YMLFile?.Filename ?? "";

        /// <summary>Номер предыдущей версии</summary>
        public string PrevNum => YMLFile?.FirstPrevVersion?.NumVersionLine ?? "";

        /// <summary>
        /// Номер предыдущей версии для сортировки
        /// </summary>
        public double PrevNumOrder => Release.VerAsNum(PrevNum);

        /// <summary>Имя файла с предыдущей версией</summary>
        public string PrevFile => YMLFile?.FirstPrevVersion?.file ?? "";

        /// <summary>
        /// признак НЕ кумулятивной версии
        /// </summary>
        public bool isNoCumulative => YMLFile?.IsNoCumulative ?? false;
    }
}
