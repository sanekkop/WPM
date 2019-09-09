using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class RefillChoise : LoaderChoise
    {
        public Doc DocAP;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReadySS"></param>
        /// <param name="CallObj"></param>
        public RefillChoise(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.RefillChoise;
            FVoiceOn = true;
            DocAP = new Doc(SS);
        } // constructor (RefillChoise)
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Init()
        {
            if (FindDocAP(true))//Ищем набор
            {
                return JumpTo(new RefillSet(SS, this));
            }
            else if (FindDocAP(false)) //Ищем выкладку
            {
                return JumpTo(new RefillLayout(SS, this));
            }

            //нет ничего
            TaskList = null;
            string TextQuery = "select * from WPM_fn_GetRefillChoise(:employer)";
            SQL1S.QuerySetParam(ref TextQuery, "employer", Employer.ID);
            SS.ExecuteWithReadNew(TextQuery, out TaskList);
            return Positive("Выберите что будете делать...");
        } // Init
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Cancel()
        {
            return JumpTo(new ChoiseWorkSupply(SS, this));
        } // Cancel()
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        protected override void ReactionKeyDo(Keys key)
        {
            if (key == Keys.Escape || key == Keys.D0)
            {
                Cancel();
            }
            else if (key == Keys.Enter || Helper.IsGreenKey(key))
            {
                SS.OnReport(new ReportEventArgs("Обновляю..."));
                Init();
                return;
            }
            else if (key == Keys.D1)
            {
                if ((int)TaskList.Rows[0]["allowed"] == 1)
                {
                    JumpTo(new RefillSet(SS, this));
                }
            }
            else if (key == Keys.D2)
            {
                if ((int)TaskList.Rows[1]["allowed"] == 1)
                {
                    JumpTo(new RefillLayout(SS, this));
                }
            }
        } // ReactionKeyDo

        //------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool FindDocAP(bool IsSet)
        {
            string TextQuery =
                "select top 1 " + 
                    (IsSet ? "substring(DocAP.$АдресПеремещение.ДокументОснование , 5, 9)" : " DocAP.iddoc") + " as iddoc " +
                "from _1sjourn as journ (nolock) " +
                    "inner join DH$АдресПеремещение as DocAP (nolock) " +
                        "on DocAP.iddoc = journ.iddoc " +
                    "left join DT$АдресПеремещение as DocAPTab (nolock) " + 
                        "on DocAP.iddoc = DocAPTab.iddoc " +
                "where " +
                    "journ.date_time_iddoc < '19800101Z' " +
                    "and journ.ismark = 0 " +
                    "and journ.$Автор = :employer " +
                    "and DocAP.$АдресПеремещение.ТипДокумента = 13 " +
                    "and DocAP.$АдресПеремещение.Склад = :warehouse " + 
                    //От параметра IsSet зависит что мы ищем набор или выкладку
                    "and DocAPTab.iddoc " + (IsSet ? "is null" : "is not null");
            SQL1S.QuerySetParam(ref TextQuery, "employer", Employer.ID);
            SQL1S.QuerySetParam(ref TextQuery, "warehouse", Employer.Warehouse.ID);
            DataTable DT;
            SS.ExecuteWithReadNew(TextQuery, out DT);
            if (DT.Rows.Count == 0)
            {
                return false;
            }
            DocAP = Doc.GiveDocById(DT.Rows[0]["iddoc"].ToString(), SS);
            return DocAP.Selected;
        } // FindDocAP

    }
}
