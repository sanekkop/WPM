using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class RefillSet : RefillChoise
    {
        public RefSection Adress0;
        public RefSection Adress1;
        public int Amount;
        public ActionSet CurrentAction;
        public RefItem Item;
        public int lineno_;
        public string PreviousAction;
        public int InputedCount;
        private bool RobotWaiting { get { return (int)(decimal)DocAP.GetAttributeHeader("ФлагОжиданияРобота") == 1 ? true : false; } } // RobotWaiting

        /// <summary>
        /// 
        /// </summary>
        public int CollectedLines
        {
            get
            {
                //Недостачу исключаем
                string TextQuery =
                "select count(*) " + 
                "from DT$АдресПеремещение as DocAP (nolock) " +
                "where " +
                    "not DocAP.$АдресПеремещение.Дата1 = :EmptyDate " +
                    "and DocAP.iddoc = :iddoc " +
                    "and DocAP.$АдресПеремещение.Состояние1 <> 12 ";
                SQL1S.QuerySetParam(ref TextQuery, "iddoc", DocAP.ID);
                return (int)SS.ExecuteScalar(TextQuery);
            }
        } // CollectedLines
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReadySS"></param>
        /// <param name="CallObj"></param>
        public RefillSet(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            //Стандартные включалки
            CurrentMode = Mode.RefillSet;
            ReactionSCItem += new ReactionSCItemHandler(RefillSet_ReactionSCItem);
            DocAP = (CallObj as RefillChoise).DocAP;

            //Просто
            Adress0     = new RefSection(SS);
            Adress1     = new RefSection(SS);
            Item        = new RefItem(SS);
            PreviousAction = "";
        } // RefillSet (constructor)
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Init()
        {
            if (!DocAP.Selected)
            {
                //Такой вот костыль, не знаю как обыграть пока
                ABaseMode tmp = GoTransportTask();
                if (Result == MMResult.Positive)
                {
                    FExcStr = tmp.ExcStr;
                    return tmp;
                }
                SS.OnReport(new ReportEventArgs("Запрашиваю задание у 1С..."));
                //Нет заданий кидаем для одинесины хуйню
                Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = SS.ExtendID(Employer.ID, "Спр.Сотрудники");

                string resultMessage;
                if (!SS.ExecCommandOnlyResultNew("RefillSetAddItem1", DataMapWrite, out resultMessage))
                {
                    return Negative(resultMessage);
                }
                //Попытка перейти опять сюда (в набор)
                if (!FindDocAP(true))
                {
                    return Negative("Не удалось получить задание через 1С робота!");
                }
            } // !FindDocAP()
            return Refresh();
        } // Init
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Cancel()
        {
            SS.OnReport(new ReportEventArgs("Отказ от задания..."));
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"] = SS.ExtendID(DocAP.ID, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = SS.ExtendID(Employer.ID, "Спр.Сотрудники");
            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!SS.ExecCommand("RefillSetDellItem", DataMapWrite, FieldList, out DataMapRead))
            {
                return Negative(SS.ExcStr);
            }
            if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] == -3)
            {
                return Negative(DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString());
            }
            return Refresh();
        } // Cancel
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        protected override void ReactionKeyDo(Keys key)
        {
            if (CurrentAction == ActionSet.Waiting)
            {
                if (Helper.IsGreenKey(key))
                {
                    SS.OnReport(new ReportEventArgs("Проверим..."));
                    Refresh();
                }
            }
            else if (key == Keys.Escape && CollectedLines > 0)
            {
                Cancel();
                return;
            }
            else if (Helper.IsGreenKey(key) || key == Keys.Enter)
            {
                EnterCount();
            }
            else if (key == Keys.D9 && CurrentAction != ActionSet.EnterCount)
            {
                JumpTo(new RefillSetCorrect(SS, this));
            }
        } // ReactionKeyDo
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void ReactionSCSectionDo(RefSection e)
        {
            if (CurrentAction != ActionSet.ScanAdress)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            else if (e.ID != Adress0.ID)
            {
                Negative("Неверный адрес! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            CurrentAction = ActionSet.ScanItem;
            Positive(SS.WhatUNeed(CurrentAction));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void RefillSet_ReactionSCItem(object sender, ReactionSCEventArgs e)
        {
            if (CurrentAction != ActionSet.ScanItem)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }

            RefItem item = e.Ref as RefItem;
            if (item.ID != Item.ID)
            {
                Negative("Не тот товар! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            CurrentAction = ActionSet.EnterCount;
            Positive(SS.WhatUNeed(CurrentAction));
        } // RefillSet_ReactionSCItem
        
        //----------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ABaseMode Refresh()
        {
            DocAP.Refresh();
            if (RobotWaiting)
            {
                CurrentAction = ActionSet.Waiting;
                return Negative("1С не ответила! (флаг ожидания)");
            }
            string TextQuery =
                "select top 1 " +
                    "DocAP.$АдресПеремещение.Адрес0 as Adress0, " +
                    "DocAP.$АдресПеремещение.Адрес1 as Adress1, " +
                    "DocAP.$АдресПеремещение.Количество as Amount, " +
                    "DocAP.$АдресПеремещение.Товар as Item, " +
                    "DocAP.lineno_ as lineno_ " +
                "from DT$АдресПеремещение as DocAP (nolock) " +
                "where " +
                    "DocAP.$АдресПеремещение.Дата1 = :EmptyDate " + 
                    "and DocAP.iddoc = :iddoc " + 
                    "and DocAP.$АдресПеремещение.Количество > 0 ";
            SQL1S.QuerySetParam(ref TextQuery, "iddoc", DocAP.ID);
            DataTable DT;
            SS.ExecuteWithReadNew(TextQuery, out DT);
            if (DT.Rows.Count == 0)
            {
                return JumpTo(new RefillSetComplete(SS, this));
            }
            //Всасываем все говно в наши объекты
            Adress0.FoundID(DT.Rows[0]["Adress0"].ToString());
            Adress1.FoundID(DT.Rows[0]["Adress1"].ToString());
            Amount = (int)(decimal)DT.Rows[0]["Amount"];
            Item = new RefItem(SS); //Есть бага в объекте ARef, поэтому пересоздаем
            Item.FoundID(DT.Rows[0]["Item"].ToString());
            lineno_ = (short)DT.Rows[0]["lineno_"];

            CurrentAction = ActionSet.ScanAdress;
            return Positive(SS.WhatUNeed(CurrentAction));
        } // Refresh
        /// <summary>
        /// 
        /// </summary>
        private void CompleteLine()
        {
            string TextQuery =
                "Begin tran; " +
                "UPDATE DH$АдресПеремещение SET " +
                    "$АдресПеремещение.ФлагОжиданияРобота = 1 " +
                "WHERE " +
                    "DH$АдресПеремещение .iddoc = :iddoc; " +
                "if @@rowcount > 0 begin " +
                    "UPDATE DT$АдресПеремещение SET " +
                        "$АдресПеремещение.Дата1 = :NowDate, " +
                        "$АдресПеремещение.Время1 = :NowTime " +
                    "WHERE " +
                        "DT$АдресПеремещение .iddoc = :iddoc " +
                        "and DT$АдресПеремещение .lineno_ = :lineno_; " +
                    "if @@rowcount > 0 commit tran " +
                    "else rollback tran; " +
                    "end " +
                "else rollback tran;";
            SQL1S.QuerySetParam(ref TextQuery, "iddoc", DocAP.ID);
            SQL1S.QuerySetParam(ref TextQuery, "lineno_", lineno_);
            SS.ExecuteWithoutReadNew(TextQuery);

            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"] = SS.ExtendID(DocAP.ID, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = SS.ExtendID(Employer.ID, "Спр.Сотрудники");

            string resultMessage;
            if (SS.ExecCommandOnlyResultNew("RefillSetAddItem2", DataMapWrite, out resultMessage))
            {
                PreviousAction = "Отобрано " + Item.InvCode + " - " + Amount.ToString() + " шт. (строка " + lineno_.ToString() + ")";
            }
            Refresh();
        } // CompleteLine
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Count"></param>
        private void EnterCount()
        {
            if (CurrentAction != ActionSet.EnterCount)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            if (Amount != InputedCount)
            {
                //Negative("Количество указано неверно! (нужно " + Amount.ToString() + ")");
                Negative("Количество указано неверно!");
                return;
            }
            SS.OnReport(new ReportEventArgs("Фиксирую..."));
            CompleteLine();
        } // EnterCount
    }
}
