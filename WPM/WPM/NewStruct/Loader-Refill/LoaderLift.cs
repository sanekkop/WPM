using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class LoaderLift : Loader
    {

        public LoaderLift(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
        } // LoaderLift
        internal override ABaseMode Init()
        {
            CurrentAction = ActionSet.ScanPallete;
            return Positive(SS.WhatUNeed(CurrentAction));
        } // Init()
        internal override ABaseMode Cancel()
        {
            base.Cancel();
            return JumpTo(new LoaderChoiseLift(SS, this));
        } // Cancel


        protected override bool TaskComplete()
        {
            string TextQuery =
                    "update $Спр.ПеремещенияПаллет set " +
                        "$Спр.ПеремещенияПаллет.Дата1 = :NowDate, " +
                        "$Спр.ПеремещенияПаллет.Время1 = :NowTime, " +
                        "$Спр.ПеремещенияПаллет.Адрес1 = :Adress1, " +
                        "$Спр.ПеремещенияПаллет.ФлагОперации = 2 " +
                    "where id = :id; " +
                    "select @@rowcount;";
            SQL1S.QuerySetParam(ref TextQuery, "id", Task.ID);
            SQL1S.QuerySetParam(ref TextQuery, "Adress1", Adress1.ID);
            return (int)SS.ExecuteScalar(TextQuery) == 0 ? false : true;
        }
        protected override void GoNextTask()
        {
            JumpTo(new LoaderChoiseLift(SS, this));
        } // GoNextTask
    }
}
