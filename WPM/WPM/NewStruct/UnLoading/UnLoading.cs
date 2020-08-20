using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class UnLoading : ABaseStandart
    {
        public RefBox BoxUnLoad;
        public RefSection AdressUnLoad;
        public struct StructBox
        {
            public string View;
            public int AllBoxes;
            public string Adress;
            public string NumberBill;
            public string MainSectorName;
            public string NumberCC;
            public string Gate;
            public string NumberBox;
        }

        public StructBox DocCC;
        public ActionSet CurrentAction;
        public string PreviousAction;
        public Doc DocUnload;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReadySS"></param>
        /// <param name="CallObj"></param>
        public UnLoading(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            //Стандартные включалки
            CurrentMode = Mode.UnLoading;
            
            //Просто
            AdressUnLoad = new RefSection(SS);
            BoxUnLoad = new RefBox(SS);
            DocUnload = new Doc(SS);
            DocCC = new StructBox();
            PreviousAction = "";
        } // RefillSet (constructor)
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Init()
        {
            CurrentAction = ActionSet.ScanBox;
            CurrentMode = Mode.UnLoading;
            return Refresh();
        } // Init
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Cancel()
        {

            return JumpTo(new ChoiseWorkShipping(SS, this));
        } // Cancel
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
            Refresh();
        } // ReactionKeyDo
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void ReactionSCDo(ReactionSCEventArgs e)
        {

            if (CurrentAction == ActionSet.ScanAdress)
            {
                if (e.Ref.GetType() != new RefSection(SS).GetType())
                {
                    Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                    return;
                }
                RefSection Section = e.Ref as RefSection;
                AdressUnLoad.FoundID(Section.ID);
                string TextQuery =
                         "UPDATE $Спр.МестаПогрузки " +
                         "SET " +
                            "$Спр.МестаПогрузки.Адрес9 = :AdressID ," +
                            "$Спр.МестаПогрузки.Сотрудник8 = :EmployerID ," +
                            "$Спр.МестаПогрузки.Дата9 = :Date ," +
                            "$Спр.МестаПогрузки.Время9 = :Time " +
                            "WHERE  ";
                if (DocUnload.Selected)
                {

                    TextQuery += " $Спр.МестаПогрузки .КонтрольНабора = :DocUnload ";
                    SQL1S.QuerySetParam(ref TextQuery, "DocUnload", DocUnload.ID);
                
                }
                else
                {
                    TextQuery += " $Спр.МестаПогрузки .ID = :ID ";
                    SQL1S.QuerySetParam(ref TextQuery, "ID", BoxUnLoad.ID);
                
                }
                SQL1S.QuerySetParam(ref TextQuery, "AdressID",      AdressUnLoad.ID);
                SQL1S.QuerySetParam(ref TextQuery, "EmployerID",    Employer.ID);
                SQL1S.QuerySetParam(ref TextQuery, "Date",          DateTime.Now);
                SQL1S.QuerySetParam(ref TextQuery, "Time",          APIManager.NowSecond());
                if (!SS.ExecuteWithoutRead(TextQuery))
                {
                    Negative("Не удалось зафиксировать! " + SS.WhatUNeed(CurrentAction));
                    return;
                }
                CurrentAction = ActionSet.ScanBox;
                Refresh();
                return;
            }
            else if (CurrentAction != ActionSet.ScanBox)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return; 
                
            }
            if (e.Ref.GetType() != new RefBox(SS).GetType())
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;                
            }
            RefBox Box = e.Ref as RefBox;

            BoxUnLoad.FoundID(Box.ID);
            DocUnload = new Doc(SS);
            CurrentAction = ActionSet.ScanAdress;
            AdressUnLoad = new RefSection(SS);
            Refresh();

        }

        protected override void ReactionDocDo(Doc doc)
        {
            if (CurrentAction != ActionSet.ScanBox)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;

            }
            if (doc.TypeDoc == "КонтрольНабора")
            {
                //нам нужны все места этого документа, запомним документ и найдем первое место
                DocUnload.FoundID(doc.ID);
                string TextQuery =
                     "SELECT TOP 1 Ref.ID as ID " +
                     "FROM " +
                        "$Спр.МестаПогрузки as Ref (nolock) " +
                     "WHERE " +
                        "$Спр.МестаПогрузки.КонтрольНабора = :DocUnload ";
                SQL1S.QuerySetParam(ref TextQuery, "DocUnload", DocUnload.ID);
                DataTable DT;
                if (!SS.ExecuteWithRead(TextQuery, out DT))
                {
                    Negative("Ошибка запроса! " + SS.WhatUNeed(CurrentAction));
                    return;
                }

                if (DT.Rows.Count == 0)
                {
                    Negative("Не найдено место! " + SS.WhatUNeed(CurrentAction));
                    return;
                }
                BoxUnLoad.FoundID(DT.Rows[0]["ID"].ToString());
                CurrentAction = ActionSet.ScanAdress;
                AdressUnLoad = new RefSection(SS);
                Refresh();

            }
            else
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }

            base.ReactionDocDo(doc);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ABaseMode Refresh()
        {
            if (!BoxUnLoad.Selected)
            {
                return Positive(SS.WhatUNeed(CurrentAction));
            }
            string TextQuery =
            "Select " +
                "isnull(Sections.descr, 'Пу') as Sector, " +
                "CONVERT(char(8), CAST(LEFT(journForBill.date_time_iddoc, 8) as datetime), 4) as DateDoc, " +
                "journForBill.docno as DocNo, " +
                "DocCC.$КонтрольНабора.НомерЛиста as Number, " +
                "isnull(Adress.descr, 'Пу') as Adress, " +
                "TabBox.CountAllBox as CountAllBox, " +
                "Ref.$Спр.МестаПогрузки.НомерМеста as NumberBox, " +
                "Gate.descr as Gate " +
            "from $Спр.МестаПогрузки as Ref (nolock) " +
                "inner join DH$КонтрольНабора as DocCC (nolock) " +
                    "on DocCC.iddoc = Ref.$Спр.МестаПогрузки.КонтрольНабора " +
                "left join $Спр.Секции as Sections (nolock) " +
                    "on Sections.id = DocCC.$КонтрольНабора.Сектор " +
                "left join $Спр.Секции as Adress (nolock) " +
                    "on Adress.id = Ref.$Спр.МестаПогрузки.Адрес9 " +
                "inner join DH$КонтрольРасходной as DocCB (nolock) " +
                    "on DocCB.iddoc = DocCC.$КонтрольНабора.ДокументОснование " +
                "inner JOIN DH$Счет as Bill (nolock) " +
                    "on Bill.iddoc = DocCB.$КонтрольРасходной.ДокументОснование " + 
                "INNER JOIN _1sjourn as journForBill (nolock) " + 
                    "on journForBill.iddoc = Bill.iddoc " + 
                "left join $Спр.Ворота as Gate (nolock) " +
                    "on Gate.id = DocCB.$КонтрольРасходной.Ворота " + 
                "left join ( " +
                    "select " + 
                        "DocCB.iddoc as iddoc, " + 
                        "count(*) as CountAllBox " + 
                    "from $Спр.МестаПогрузки as Ref (nolock) " +
                    "inner join DH$КонтрольНабора as DocCC (nolock) " + 
                        "on DocCC.iddoc = Ref.$Спр.МестаПогрузки.КонтрольНабора " + 
                    "inner join DH$КонтрольРасходной as DocCB (nolock) " +
                        "on DocCB.iddoc = DocCC.$КонтрольНабора.ДокументОснование " +
                    "where " +
                        "Ref.ismark = 0 " +
                    "group by DocCB.iddoc ) as TabBox " +
                    "on TabBox.iddoc = DocCB.iddoc " +
                "where Ref.id = :id";
            SQL1S.QuerySetParam(ref TextQuery, "id", BoxUnLoad.ID);
            DataTable DT;
            if (!SS.ExecuteWithRead(TextQuery, out DT))
            {
                return Negative("Ошибка запроса! " + SS.WhatUNeed(CurrentAction));
            }

            if (DT.Rows.Count == 0)
            {
                return Negative("Не найдено место! " + SS.WhatUNeed(CurrentAction));
            }
                       
            DocCC.View = DT.Rows[0]["Sector"].ToString().Trim() + "-" + DT.Rows[0]["Number"].ToString() + " Заявка " + DT.Rows[0]["DocNo"].ToString() + " (" + DT.Rows[0]["DateDoc"].ToString() + ")";
            DocCC.Adress = DT.Rows[0]["Adress"].ToString();
            DocCC.AllBoxes = (int)DT.Rows[0]["CountAllBox"];
            DocCC.NumberBill = DT.Rows[0]["DocNo"].ToString().Trim();
            DocCC.NumberCC = DT.Rows[0]["Number"].ToString();
            DocCC.MainSectorName = DT.Rows[0]["Sector"].ToString();
            DocCC.Gate = DT.Rows[0]["Gate"].ToString();
            DocCC.NumberBox = DT.Rows[0]["NumberBox"].ToString();               
            
            return Positive(SS.WhatUNeed(CurrentAction));
        } // Refresh
       
    }
}
