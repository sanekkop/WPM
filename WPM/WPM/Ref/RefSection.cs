using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class RefSection : ARef
    {
        protected override string TypeObj { get { return "Секции"; } }

        public int Type
        {
            get
            {
                return Selected ? (int)(decimal)GetAttribute("ТипСекции") : -1;
            }
        } // Type

        private RefGates FAdressZone;
        public RefGates AdressZone { get { return GetGatesProperty("ЗонаАдресов", ref FAdressZone); } } //Зона адреса
        
        public RefSection(SQL1S ReadySS)
            : base(ReadySS)
        {
            //TypeObj = "Секции";
            HaveCode = false;
            HaveName = true;
        } // RefSection (constructor)
    }
}
