using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class LoaderDownAntrisole : Loader
    {

        public LoaderDownAntrisole(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
        } // LoaderDown
        internal override ABaseMode Init()
        {
            CurrentAction = ActionSet.ScanPallete;
            return Positive(SS.WhatUNeed(CurrentAction));
        } // Init()
        //protected override void ReactionSCSectionDo(RefSection e)
        //{
        //    if (CurrentAction != ActionSet.ScanAdress)
        //    {
        //        Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
        //        return;
        //    }
        //    if (e.ID != Task.Adress0.ID)
        //    {
        //        Negative("Неверный адрес!");
        //        return;
        //    }
        //    CurrentAction = ActionSet.ScanPallete;
        //    Positive(SS.WhatUNeed(CurrentAction));
        //} // ReactionSCSectionDo
        //protected override void ReactionPalleteDo(string barcode)
        //{
        //    if (CurrentAction != ActionSet.ScanPallete)
        //    {
        //        Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
        //        return;
        //    }
        //    if (Task.PalleteBarcode != barcode)
        //    {
        //        Negative("Неверная паллета! Где вы это взяли?");
        //        return;
        //    }
        //    PalleteBarcode = barcode;
        //    TaskEnd();
        //} // ReactionPalleteDo

        override protected bool TaskComplete()
        {
            string TextQuery = "declare @result int; exec WPM_TaskLoaderDownAntrisoleComplete :id, :Adress1, @result out; select @result;";
            SQL1S.QuerySetParam(ref TextQuery, "id", Task.ID);
            SQL1S.QuerySetParam(ref TextQuery, "Adress1", Adress1.ID);
            return (int)SS.ExecuteScalar(TextQuery) == 0 ? false : true;
        } // DownTaskComplete
        protected override void GoNextTask()
        {
            GoDownTask();
        } // GoNextTask
    }
}
