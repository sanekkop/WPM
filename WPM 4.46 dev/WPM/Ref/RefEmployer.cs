using System;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class RefEmployer : ARef
    {
        protected override string TypeObj { get { return "Сотрудники"; } }
        private string settings;

        public bool CanLoad
        {
            get { GetDataEmployer(); return (settings.Substring(22, 1) == "1" ? true : false); }
        }
        public bool SelfControl
        {
            get { GetDataEmployer(); return (settings.Substring(20, 1) == "1" ? true : false); }
        }
        public bool CanRoute
        {
            get { GetDataEmployer(); return (settings.Substring(19, 1) == "0" ? true : false); }
        }
        public bool CanHarmonization
        {
            get {GetDataEmployer(); return (settings.Substring(17, 1) == "1" ? true : false); }
        }
        public bool CanSupply
        {
            get { GetDataEmployer(); return (settings.Substring(14, 1) == "1" ? true : false); }
        }
        public bool CanCellInventory
        {
            get { GetDataEmployer(); return (settings.Substring(13, 1) == "1" ? true : false); }
        }
        public bool CanDiffParty
        {
            get { GetDataEmployer(); return (settings.Substring(12, 1) == "1" ? true : false); }
        }
        public bool CanAcceptance
        {
            get { GetDataEmployer(); return (settings.Substring(11, 1) == "1" ? true : false); }
        }
        public bool CanTransfer
        {
            get { GetDataEmployer(); return (settings.Substring(10, 1) == "1" ? true : false); }
        }
        public bool CanMultiadress
        {
            get { GetDataEmployer(); return (settings.Substring(9, 1) == "1" ? true : false); }
        }
        public bool CanGiveSample
        {
            get { GetDataEmployer(); return (settings.Substring(7, 1) == "1" ? true : false); }
        }
        public bool CanLayOutSample
        {
            get { GetDataEmployer(); return (settings.Substring(6, 1) == "1" ? true : false); }
        }
        public bool CanInventory
        {
            get { GetDataEmployer(); return (settings.Substring(5, 1) == "1" ? true : false); }
        }
        public bool CanComplectation
        {
            get { GetDataEmployer(); return (settings.Substring(4, 1) == "1" ? true : false); }
        }
        public bool CanSet
        {
            get { GetDataEmployer(); return (settings.Substring(1, 1) == "1" ? true : false); }
        }
        public bool CanDown
        {
            get { GetDataEmployer(); return (settings.Substring(0, 1) == "1" ? true : false); }
        }
        public string IDD
        {
            get
            {
                return GetAttribute("IDD").ToString();
            }
        }
        /// <summary>
        /// "Родной склад" сотрудника
        /// </summary>
        public RefWarehouse Warehouse
        {
            get
            {
                if (!Selected)
                {
                    return new RefWarehouse(SS);
                }
                string TextQuery = "select dbo.WPM_fn_GetNativeWarehouse(:employer)";
                SQL1S.QuerySetParam(ref TextQuery, "employer", ID);
                RefWarehouse result = new RefWarehouse(SS);
                result.FoundID(SS.ExecuteScalar(TextQuery).ToString());
                return result;
            }
        } // Warehouse


        public RefEmployer(SQL1S ReadySS) : base (ReadySS)
        {
            HaveName    = true;
            HaveCode    = true;
        }

        private bool GetDataEmployer()
        {
            if (settings != null)
            {
                return true;
            }
            bool result;
            Dictionary<string, object> DataMap;
            settings = "000000000000000000000000000000";
            if (!SS.GetSCData(ID, "Сотрудники", "Настройки", out DataMap, true))
            {
                result = false;
            }
            else
            {
                settings += Translation.DecTo2((long)(decimal)DataMap["Спр.Сотрудники.Настройки"]);
                result = true;
            }
            settings = settings.Substring(settings.Length - 23);    //23 правых символов
            settings = Helper.ReverseString(settings);              //Отразим, чтобы было удобнее добавлять новые флажки
            return result;
        }
        public override void Refresh()
        {
            settings = null;
            GetDataEmployer(); 
        }
    }
}
