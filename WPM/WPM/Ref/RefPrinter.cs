using System;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class RefPrinter : ARef
    {
        protected override string TypeObj { get { return "Принтеры"; } }
        /// <summary>
        /// String descriprion of path printer, if printer not selected return null
        /// </summary>
        public string Path
        {
            get
            {
                return Selected ? GetAttribute("Путь").ToString() : null;
            }
        }
        /// <summary>
        /// printer type 1 = labels, any other - standart, if printer not selected return -1
        /// </summary>
        public int PrinterType
        {
            get
            {
                return Selected ? (int)(decimal)GetAttribute("ТипПринтера") : -1;
            }
        }
        /// <summary>
        /// Строка описания принтера, если не выбран то выдает <принтер не выбран>
        /// </summary>
        public string Description
        {
            get { return Selected ? Path.Trim() + " " + (PrinterType == 1 ? "этикеток" : "обычный") : "<принтер не выбран>";}
        }
        public RefPrinter(SQL1S ReadySS) : base (ReadySS)
        {
            HaveCode    = true;
            HaveName    = true;
        }
    }
}
