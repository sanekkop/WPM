using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class RefillSetComplete : RefillChoise
    {
        public string PalleteDescr { get { return PalleteBarcode == null ? "" : "Паллета " + PalleteBarcode.ToString(); } }
        private string PalleteBarcode;
        private ActionSet CurrentAction;
        private RefSection Adress0;
        public Doc SubjectDoc;  //Документ второй части выкладки

        public RefillSetComplete(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.RefillSetComplete;
            PalleteBarcode = null;
            DocAP = (CallObj as RefillChoise).DocAP;
            CurrentAction = ActionSet.ScanPallete;
        } // RefillSetComplete (constructor)
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Init()
        {
            string TextQuery =
                "SELECT top 1 _1SJOURN.IDDOC iddoc " +
                "FROM _1SJOURN (NOLOCK INDEX=ACDATETIME), _1SCRDOC (NOLOCK INDEX=PARENT) " +
                "WHERE " +
                    "_1SJOURN.DATE_TIME_IDDOC=_1SCRDOC.CHILD_DATE_TIME_IDDOC " +
                    "and _1SJOURN.ismark = 0 " +
                    "and _1SJOURN.date_time_iddoc < '19800101Z' " +
                    "and _1SCRDOC.MDID=0 and _1SCRDOC.PARENTVAL='O1" + SS.ExtendID(DocAP.ID, DocAP.TypeDoc) + "' ";
            DataTable DT;
            SS.ExecuteWithReadNew(TextQuery, out DT);
            if (DT.Rows.Count == 0)
            {
                return Negative("Нет документа второй части выкладки!");
            }
            //Документ есть! мы его получили!
            SubjectDoc = Doc.GiveDocById(DT.Rows[0]["iddoc"].ToString(), SS);
            return Positive(SS.WhatUNeed(CurrentAction));
        } // Init
        /// <summary>
        /// 
        /// </summary>
        /// <param name="section"></param>
        protected override void ReactionSCSectionDo(RefSection section)
        {
            if (CurrentAction != ActionSet.ScanAdress)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            Adress0 = section;
            CompletePallete();
            JumpTo(new RefillChoise(SS, this));
        } // ReactionSCSection
        protected override void ReactionPalleteDo(string barcode)
        {
            if (!CheckPallete(barcode))
            {
                return;
            }
            //Это паллета, проверим на вшивость
            CurrentAction = ActionSet.ScanAdress;
            if (PalleteBarcode == null)
            {
                Positive(SS.WhatUNeed(CurrentAction));
            }
            else
            {
                Negative(SS.WhatUNeed(CurrentAction));
            }
            PalleteBarcode = barcode;
        } // ReactionPalleteDo
        /// <summary>
        /// 
        /// </summary>
        private void CompletePallete()
        {
            SS.OnReport(new ReportEventArgs("Фиксирую окончание отбора..."));

            //Для начала определим будет ли после отбора сразу выкладка или будет что-то еще
            // я хз как лучше, поэтому буду тянуть информацию из таблички движка, по документу DocAP
            bool AuthorEmpty = true;
            string TextQuery = "select top 1 sector0_type sector0_type, sector1_down sector1_down from RT_refill (nolock) where DocAP = :iddoc ";
            SQL1S.QuerySetParam(ref TextQuery, "iddoc", DocAP.ID);
            DataTable DT;
            SS.ExecuteWithReadNew(TextQuery, out DT);
            if (DT.Rows.Count > 0)
            {
                if ((int)DT.Rows[0]["sector0_type"] == 3 || ((int)DT.Rows[0]["sector1_down"] == 0 && (int)DT.Rows[0]["sector0_type"] == 1))
                {
                    //Это с высокой полки или это на сектор у которого нет спуска
                    // значит мы сразу будем переходить к выкладке, после завершения отбора
                    AuthorEmpty = false;
                }
            }

            //Заполним шапку найденного документа
            SubjectDoc.SetAttributeHeader("ШКПаллеты", PalleteBarcode);
            SubjectDoc.SetAttributeHeader("КолСтрок", DocAP.RowCount);
            SubjectDoc.Save();

            //получим список всех полей табличной части документа адрес перемещения
            string columns;
            //ПО СТАРОМУ
            //SS.GetColumns("DT$АдресПеремещение", out columns);
            //columns = columns.Replace("IDDOC,", "");    //уберем iddoc, его мы перезаписывать не будем
            //КОНЕЦ ПО СТАРОМУ

            //ПО НОВОМУ
            SS.GetColumns("DT$АдресПеремещение", out columns, "min");
            columns = columns.Replace("min(IDDOC),", "");    //уберем iddoc, его мы перезаписывать не будем
            string tmpItem =  SS.QueryParser("$АдресПеремещение.Товар ").Trim();
            string tmpAmount = SS.QueryParser("$АдресПеремещение.Количество ").Trim();
            columns = columns.Replace("min(" + tmpItem + ")", tmpItem);    //группировка по товару
            columns = columns.Replace("min(" + tmpAmount, "sum(" + tmpAmount);    //Сумма по количество
            columns = columns.Replace("min(IDDOC),", "");    //уберем iddoc, его мы перезаписывать не будем
            //Теперь скопируем строки из одного документа в другой, а потом изменим некоторые поля
            //Будем делать так, без транзакции, пока. А то ну его на хуй вставлять с изменениями слишком сложно
            TextQuery =
                "insert into DT$АдресПеремещение " +
                //"select :target," + columns + " from DT$АдресПеремещение where iddoc = :source; " +   //ПО СТАРОМУ
                "select :target," + columns + " from DT$АдресПеремещение " +
                    "where " + 
                        "iddoc = :source " + 
                        "and $АдресПеремещение.Состояние1 = :goodMove " +
                        "and not $АдресПеремещение.Дата1 = :EmptyDate " + 
                    "group by $АдресПеремещение.Товар ; " +
                "if @@rowcount > 0 begin " + 
                    "update _1sjourn set $Автор = :employer where iddoc = :target; " + 
                    "update DT$АдресПеремещение SET " +
                        "$АдресПеремещение.Состояние0 = $АдресПеремещение.Состояние1 , " +
                        "$АдресПеремещение.Состояние1 = 2 , " +
                        "$АдресПеремещение.Сотрудник0 = :EmptyID , " +
                        "$АдресПеремещение.Адрес0 = $АдресПеремещение.Адрес1 , " +
                        "$АдресПеремещение.Дата0 = :NowDate , " +
                        "$АдресПеремещение.Дата1 = :EmptyDate , " +
                        "$АдресПеремещение.Время0 = :NowTime , " +
                        "$АдресПеремещение.Время1 = 0 , " +
                        "$АдресПеремещение.Док = :source " +
                    "where iddoc = :target; " + 
                    "exec WPM_RefillSetComplete :source, :adress0, :barcode_pallete " +
                "end else begin " +
                    "update _1sjourn set $Автор = :EmptyID where iddoc = :target; " + 
                    //флаг под удаление возводим, иначе херня выходит
                    "update RT_Refill set flag = 5 where docAP = :source " + 
                    "end";
            SQL1S.QuerySetParam(ref TextQuery, "target", SubjectDoc.ID);
            SQL1S.QuerySetParam(ref TextQuery, "source", DocAP.ID);
            SQL1S.QuerySetParam(ref TextQuery, "barcode_pallete", PalleteBarcode);
            SQL1S.QuerySetParam(ref TextQuery, "adress0", Adress0.ID);
            SQL1S.QuerySetParam(ref TextQuery, "employer", AuthorEmpty ? SQL1S.GetVoidID() : Employer.ID);
            SQL1S.QuerySetParam(ref TextQuery, "goodMove", 4);  //Хороший движение
            SS.ExecuteWithoutReadNew(TextQuery);
        } // CompletePallete
    }
}
