using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class RefillLayoutComplete : RefillChoise
    {
        //public string PalleteDescr { get { return PalleteBarcode == null ? "" : "Паллета " + PalleteBarcode.ToString(); } }
        //private string PalleteBarcode;
        //private ActionSet CurrentAction;
        //private RefSection Adress0;
        //public Doc SubjectDoc;  //Документ второй части выкладки

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReadySS"></param>
        /// <param name="CallObj"></param>
        public RefillLayoutComplete(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.RefillLayoutComplete;
            //PalleteBarcode = null;
            DocAP = (CallObj as RefillChoise).DocAP;
            //CurrentAction = ActionSet.ScanPallete;
        } // RefillSetComplete (constructor)
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Init()
        {
            return Positive("'Зеленая' - завершить");
        } // Init
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        protected override void ReactionKeyDo(Keys key)
        {
            if (key == Keys.Escape)
            {
                Cancel();
                return;
            }
            else if (Helper.IsGreenKey(key))
            {
                Complete();
            }
        } // ReactionKeyDo
        /// <summary>
        /// 
        /// </summary>
        private void Complete()
        {
            SS.OnReport(new ReportEventArgs("Фиксирую..."));
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"] = SS.ExtendID(DocAP.ID, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = SS.ExtendID(Employer.ID, "Спр.Сотрудники");
            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!SS.ExecCommand("RefillSetComplete", DataMapWrite, FieldList, out DataMapRead))
            {
                Negative(SS.ExcStr);
                return;
            }
            if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] == -3)
            {
                Negative(DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString());
                return;
            }
            JumpTo(new RefillChoise(SS, this));
        } // Complete

    }
}
