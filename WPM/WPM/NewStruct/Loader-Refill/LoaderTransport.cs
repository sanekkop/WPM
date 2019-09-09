using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class LoaderTransport : LoaderDownAntrisole
    {
        public LoaderTransport(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
        } // LoaderDown
        internal override ABaseMode Init()
        {
            CurrentAction = ActionSet.ScanPallete;
            return Positive(SS.WhatUNeed(CurrentAction));
        } // Init()

        override protected bool TaskComplete()
        {
            string TextQuery = "declare @result int; exec WPM_TaskLoaderTransportComplete :id, :Adress1, @result out; select @result;";
            SQL1S.QuerySetParam(ref TextQuery, "id", Task.ID);
            SQL1S.QuerySetParam(ref TextQuery, "Adress1", Adress1.ID);
            return (int)SS.ExecuteScalar(TextQuery) == 0 ? false : true;
        } // DownTaskComplete
        protected override void GoNextTask()
        {
            JumpTo(new RefillChoise(SS, this));
        } // GoNextTask
    }
}
