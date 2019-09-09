using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class RefItem : ARef
    {
        protected override string TypeObj { get { return "Товары"; } }

        public decimal PricePurchase { get { return (decimal)GetAttribute("Прих_Цена"); } } // приходная цена
        public decimal Price { get { return (decimal)GetAttribute("Опт_Цена"); } }  //Оптовая цена 
        //public decimal %name% { get { return (decimal)GetAttribute("Мел_Опт_Цена"); } }  // Мелкооптовая цена, нахуй не здалась никому
        public string InvCode { get { return GetAttribute("ИнвКод").ToString().Trim(); } }
        public int Details { get { return (int)(decimal)GetAttribute("КоличествоДеталей"); } }  // Количество деталей

        private RefGates FZonaHand;
        public RefGates ZonaHand { get { return GetGatesProperty("ЗонаР", ref FZonaHand); } } //Зона в адресах ручного набора

        private RefGates FZonaTech;
        public RefGates ZonaTech { get { return GetGatesProperty("ЗонаТ", ref FZonaTech); } } //Зона в адресах машинного набора

        public RefItem(SQL1S ReadySS)
            : base(ReadySS)
        {
            //TypeObj = "Товары";
            HaveCode = true;
            HaveName = true;
        } // RefItem (constructor)
        public bool FoundBarcode(string Barcode)
        {
            string TextQuery = "select top 1 PARENTEXT as ID from $Спр.ЕдиницыШК (nolock) where $Спр.ЕдиницыШК.Штрихкод = :barcode";
            SQL1S.QuerySetParam(ref TextQuery, "barcode", Barcode);
            DataTable DT;
            if (!SS.ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                return false;
            }

            FID = DT.Rows[0]["ID"].ToString();
            FName = null;
            Refresh();
            if (FName != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        } // FoundBarcode
    }
}
