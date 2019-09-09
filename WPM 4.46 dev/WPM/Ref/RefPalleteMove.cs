using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class RefPalleteMove : ARef
    {
        protected override string TypeObj { get { return "ПеремещенияПаллет"; } }

        private RefSection FAdress0;
        public RefSection Adress0 { get { return GetSectionProperty("Адрес0", ref FAdress0); } }
        
        private RefSection FAdress1;
        public RefSection Adress1 { get { return GetSectionProperty("Адрес1", ref FAdress1); } }

        public int TypeMove { get { return (int)(decimal)GetAttribute("ТипДвижения"); } }

        public string PalleteBarcode { get { return GetAttribute("ШКПаллеты").ToString(); } }

        /// <summary>
        /// Последние четыре цыфры паллеты
        /// </summary>
        public string Pallete
        {
            get
            {
                if (PalleteBarcode.Trim().Length == 0)
                {
                    return "<..>";
                }
                string tmp = "   " + Convert.ToInt32(PalleteBarcode.Substring(4, 8)).ToString();
                return tmp.Substring(tmp.Length - 4, 4);    //4 символа справа
            }
        } // PalleteNumber
        public string TypeMoveActionDescr
        {
            get
            {
                string Action;
                GetTypeMoveDescr(out Action);
                return Action;
            } // get
        } // TypeMoveActionDescr
        public string TypeMoveDescr
        {
            get
            {
                string Action;
                return GetTypeMoveDescr(out Action); ;
            } // get
        } // TypeMoveDescr
                

        public RefPalleteMove(SQL1S ReadySS)
            : base(ReadySS)
        {
            //TypeObj = "ПеремещенияПаллет";
            HaveCode = false;
            HaveName = false;
        } // RefPalleteMove (constructor)

        private string GetTypeMoveDescr(out string Action)
        {
            switch (TypeMove)
            {
                case 1:
                    Action = "Спустите паллету";
                    return "Спуск с адреса";
                case 2:
                    Action = "Возвратите паллету";
                    return "Подъем";
                case 3:
                    Action = "Снимите паллету";
                    return "Спуск с антрисоли";
                case 4:
                    Action = "Перевезите к лифту";
                    return "Транспорт к лифту";
                case 5:
                    Action = "Поднимите паллету";
                    return "Подъем лифт";
                default:
                    Action = "<неизвестно>";
                    return "<неизвестно>";
            }
        } // GetTypeMoveDescr
    }
}
