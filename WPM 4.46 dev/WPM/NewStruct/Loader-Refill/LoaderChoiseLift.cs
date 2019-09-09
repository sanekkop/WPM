using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class LoaderChoiseLift : LoaderChoise
    {
        public DataTable LiftTaskList;

        public LoaderChoiseLift(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.LoaderChoiseLift;
        } // LoaderChoise (constructor)
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Init()
        {
            LiftTaskList = null;
            string TextQuery = "select * from WPM_fn_ToModeLoaderChoiseLift()";
            SS.ExecuteWithReadNew(TextQuery, out LiftTaskList);

            return Positive("Выберите что будете делать...");
        } // Init
        protected override void ReactionKeyDo(Keys key)
        {
            if (key == Keys.Escape || key == Keys.D0)
            {
                Cancel();
                return;
            }
            else if (key == Keys.Enter || Helper.IsGreenKey(key))
            {
                SS.OnReport(new ReportEventArgs("Обновляю список..."));
                Init();
                return;
            }

            int Choise = Helper.WhatInt(key);
            if (Choise > 0 && Choise <= LiftTaskList.Rows.Count)
            {
                GoLiftTask(LiftTaskList.Rows[Choise - 1]["sector"].ToString());
            }
        } // ReactionKeyDo
        internal override ABaseMode Cancel()
        {
            return JumpTo(new LoaderChoise(SS, this));
        } // Cancel
    }
}
