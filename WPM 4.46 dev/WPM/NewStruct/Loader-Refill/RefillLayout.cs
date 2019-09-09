using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class RefillLayout : RefillChoise
    {

        public DataTable RemainItems;
        public RefSection Adress0;
        public RefSection Adress1;
        public int Amount;
        public ActionSet CurrentAction;
        public RefItem Item;
        public string PreviousAction;
        public int InputedCount;
        private int lineno_;
        public RefPalleteMove LastMove;  //Последний адрес элемента справочник паллеты по этому документу

        public bool ReadyPut { get { return CurrentAction == ActionSet.ScanItem ? false : true; } }

        public RefillLayout(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.RefillLayout;
            DocAP = (CallObj as RefillChoise).DocAP;
            lineno_ = 0;
            Adress0 = new RefSection(SS);
            Adress1 = new RefSection(SS);
            LastMove = new RefPalleteMove(SS);
            Item = new RefItem(SS);
            PreviousAction = "";
            RemainItems = new DataTable();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void ReactionSCItemDo(ReactionSCEventArgs e)
        {
            if (CurrentAction != ActionSet.ScanItem)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }

            RefItem item = e.Ref as RefItem;
            DataRow[] DR = RemainItems.Select("item = '" + item.ID + "'");
            if (DR.Length == 0)
            {
                Negative("Товар " + item.InvCode + " не найден. Возможно уже выложили?");
                return;
            }

            //Всасываем все говно в наши объекты
            Adress0.FoundID(DR[0]["Adress0"].ToString());
            Amount = (int)(decimal)DR[0]["Amount"];
            Item = item;
            lineno_ = (short)DR[0]["lineno_"];
            CurrentAction = ActionSet.ScanAdress;
            Positive(SS.WhatUNeed(CurrentAction));
        } // ReactionSCItemDo
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Init()
        {
            if (!DocAP.Selected)
            {
                SS.OnReport(new ReportEventArgs("Получаю задание..."));
                string TextQuery =
                "select top 1 DocAP.iddoc as iddoc " +
                "from _1sjourn as journ (nolock) " +
                    "inner join DH$АдресПеремещение as DocAP (nolock) " +
                        "on DocAP.iddoc = journ.iddoc " +
                    "inner join DT$АдресПеремещение as DocAPTab (nolock) " + 
                        "on DocAP.iddoc = DocAPTab.iddoc " +
                        //Условие ниже, чтобы с пустой табличной частью отсеить документы
                    "inner join _1sjourn as sub_journ (nolock) " + 
                        "on substring(DocAP.$АдресПеремещение.ДокументОснование , 5, 9) = sub_journ.iddoc " +
                    "left join $Спр.ПеремещенияПаллет as Ref (nolock) " +
                        "on Ref.$Спр.ПеремещенияПаллет.ШКПаллеты = DocAP.$АдресПеремещение.ШКПаллеты " + 
                            "and Ref.$Спр.ПеремещенияПаллет.ФлагОперации in (0, 1) " +
                            "and Ref.ismark = 0 " +
                "where " +
                    "journ.date_time_iddoc < '19800101Z' " +
                    "and journ.ismark = 0 " +
                    "and journ.$Автор = :EmptyID " +
                    "and DocAP.$АдресПеремещение.ТипДокумента = 13 " +
                    "and Ref.id is null " +
                "order by sub_journ.date_time_iddoc";
                DataTable DT;
                SS.ExecuteWithReadNew(TextQuery, out DT);
                if (DT.Rows.Count == 0)
                {
                    return Negative("Не удалось найти заданий выкладки!");
                }
                //Будем вешать на себя это задание
                TextQuery =
                    "update _1sjourn set " +
                        "$Автор = :employer " +
                    "where iddoc = :iddoc; ";
                SQL1S.QuerySetParam(ref TextQuery, "iddoc", DT.Rows[0]["iddoc"].ToString());
                SQL1S.QuerySetParam(ref TextQuery, "employer", Employer.ID);
                SS.ExecuteWithoutReadNew(TextQuery);
                //Если задание получилось, то
                if (!FindDocAP(false))
                {
                    return Negative("Не удалось зафиксировать задание выклдаки!");
                }
            }
            return Refresh();
        }   // Init
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        protected override void ReactionKeyDo(Keys key)
        {
            if (Helper.IsGreenKey(key) || key == Keys.Enter)
            {
                EnterCount();
            }
            else if (key == Keys.Escape)
            {
                Refresh();
            }
            else if (key == Keys.Right)
            {
                JumpTo(new ItemCard(SS, this));
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
            //Проверки зон!!!
            RefGates ItemZone;
            if (e.Type == 2)
            {
                ItemZone = Item.ZonaTech;
            }
            else
            {
                //Для всех остальных видов адреса будем считать ручную зону
                ItemZone = Item.ZonaHand;
            }
            //Проверка
            if (ItemZone.Selected && e.AdressZone.Selected)
            {
                //У обоих задана зона
                if (ItemZone.ID != e.AdressZone.ID)
                {
                    Negative("Нельзя! Товар: " + ItemZone.Name + ", адрес: " + e.AdressZone.Name);
                    return;
                }
            }
            else if (ItemZone.Selected)
            {
                //только товар задан
                Negative("Нельзя! Товар: " + ItemZone.Name);
                return;
            }
            else if (e.AdressZone.Selected)
            {
                //Только у адреса задана зона
                Negative("Нельзя! Адрес: " + e.AdressZone.Name);
                return;
            }
            //else в остальных случая - нам похуй

            Adress1 = e;
            CurrentAction = ActionSet.EnterCount;
            Positive(SS.WhatUNeed(CurrentAction));
        } // ReactionSCSectionDo
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ABaseMode Refresh()
        {
            string TextQuery =
                "select " +
                    "DocAP.$АдресПеремещение.Адрес0 as Adress0, " +
                    "RefSections.descr as Adress, " +
                    "DocAP.$АдресПеремещение.Количество as Amount, " +
                    "DocAP.$АдресПеремещение.Товар as Item, " +
                    "RefItems.Descr as Name, " +
                    "RefItems.$Спр.Товары.ИнвКод as InvCode, " +
                    "DocAP.lineno_ as lineno_ " +
                "from DT$АдресПеремещение as DocAP (nolock) " +
                    "left join $Спр.Товары as RefItems (nolock) " +
                        "on RefItems.id = DocAP.$АдресПеремещение.Товар " +
                    "left join $Спр.Секции as RefSections (nolock) " +
                        "on RefSections.id = DocAP.$АдресПеремещение.Адрес0 " +
                "where " +
                    "DocAP.$АдресПеремещение.Дата1 = :EmptyDate " +
                    "and DocAP.iddoc = :iddoc";
            SQL1S.QuerySetParam(ref TextQuery, "iddoc", DocAP.ID);
            RemainItems.Clear();
            SS.ExecuteWithReadNew(TextQuery, out RemainItems);
            if (RemainItems.Rows.Count == 0)
            {
                return JumpTo(new RefillLayoutComplete(SS, this));
            }

            string tmpBarcodePallete = DocAP.GetAttributeHeader("ШКПаллеты").ToString();

            TextQuery =
                "select top 1 " +
                    "Ref.id as id " +
                "from $Спр.ПеремещенияПаллет as Ref (nolock) " +
                "where " +
                    "Ref.$Спр.ПеремещенияПаллет.ШКПаллеты = :PalleteBarcode " + 
                    "and Ref.ismark = 0 " +
                "order by " + 
                    "Ref.id desc ";
            SQL1S.QuerySetParam(ref TextQuery, "PalleteBarcode", tmpBarcodePallete);
            DataTable DT;
            SS.ExecuteWithReadNew(TextQuery, out DT);
            if (DT.Rows.Count > 0)
            {
                LastMove.FoundID(DT.Rows[0]["id"].ToString());
            }

            CurrentAction = ActionSet.ScanItem;
            return Positive(SS.WhatUNeed(CurrentAction));
        } // Refresh
        /// <summary>
        /// 
        /// </summary>
        private void CompleteLine()
        {
            string TextQuery =
                "UPDATE DT$АдресПеремещение SET " +
                    "$АдресПеремещение.Дата1 = :NowDate, " +
                    "$АдресПеремещение.Время1 = :NowTime, " +
                    "$АдресПеремещение.Сотрудник0 = :employer, " +
                    "$АдресПеремещение.Адрес1 = :Adress1 " +
                "WHERE " +
                    "DT$АдресПеремещение .iddoc = :iddoc " +
                    "and DT$АдресПеремещение .lineno_ = :lineno_; ";
            SQL1S.QuerySetParam(ref TextQuery, "iddoc", DocAP.ID);
            SQL1S.QuerySetParam(ref TextQuery, "lineno_", lineno_);
            SQL1S.QuerySetParam(ref TextQuery, "employer", Employer.ID);
            SQL1S.QuerySetParam(ref TextQuery, "Adress1", Adress1.ID);
            SS.ExecuteWithoutReadNew(TextQuery);
            PreviousAction = "Выложено " + Item.InvCode + " - " + Amount.ToString() + " шт. (строка " + lineno_.ToString() + ")";
            Refresh();
        }
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
