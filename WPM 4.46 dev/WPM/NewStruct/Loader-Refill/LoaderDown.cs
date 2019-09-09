using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class LoaderDown : Loader
    {
        public LoaderDown(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
        } // LoaderDown
        internal override ABaseMode Init()
        {
            CurrentAction = ActionSet.ScanAdress;
            return Positive(SS.WhatUNeed(CurrentAction));
        } // Init()
        protected override void ReactionSCSectionDo(RefSection e)
        {
            if (CurrentAction != ActionSet.ScanAdress)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            if (e.ID != Task.Adress0.ID)
            {
                Negative("Неверный адрес!");
                return;
            }
            CurrentAction = ActionSet.ScanPallete;
            Positive(SS.WhatUNeed(CurrentAction));
        } // ReactionSCSectionDo
        protected override void ReactionPalleteDo(string barcode)
        {
            if (CurrentAction != ActionSet.ScanPallete)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            if (!CheckPallete(barcode))
            {
                return;
            }
            PalleteBarcode = barcode;
            TaskEnd();
        } // ReactionPalleteDo
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        override protected bool TaskComplete()
        {
            string TextQuery = "update $Спр.ПеремещенияПаллет set $Спр.ПеремещенияПаллет.ШКПаллеты = :pallete_barcode where id = :id ";
            SQL1S.QuerySetParam(ref TextQuery, "id", Task.ID);
            SQL1S.QuerySetParam(ref TextQuery, "pallete_barcode", PalleteBarcode);
            SS.ExecuteWithoutReadNew(TextQuery);   //Насрать что там

            //
            TextQuery = "declare @result int; exec WPM_TaskLoaderDownComplete :id, @result out; select @result;";
            SQL1S.QuerySetParam(ref TextQuery, "id", Task.ID);
            if ((int)SS.ExecuteScalar(TextQuery) == 0)
            {
                return false;
            }
            //Теперь дозапишем шк паллеты
            
            return true;
        } // DownTaskComplete
        /// <summary>
        /// 
        /// </summary>
        protected override void GoNextTask()
        {
            GoDownTask();
        } // GoNextTask
    }
}
