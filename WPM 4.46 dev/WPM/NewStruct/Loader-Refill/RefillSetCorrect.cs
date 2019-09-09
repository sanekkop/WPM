using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class RefillSetCorrect : RefillSet
    {
        public int ChoiseCorrect;

        public RefillSetCorrect(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.RefillSetCorrect;
            DocAP = (CallObj as RefillSet).DocAP;
            Amount = (CallObj as RefillSet).Amount;
            lineno_ = (CallObj as RefillSet).lineno_;
            Item = (CallObj as RefillSet).Item;
            ChoiseCorrect = 0;
        } // RefillSetComplete (constructor)
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Init()
        {
            return Positive("Выберите причину корректировки");
        } // Init
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
            else if (ChoiseCorrect == 0 && key == Keys.D0)
            {
                Cancel();
                return;
            }
            else if (Helper.IsGreenKey(key) || key == Keys.Enter)
            {
                // тут что-то должно быть
                if (ChoiseCorrect == 2)
                {
                    EnterCount();
                }
            }
            else if (key == Keys.D2)
            {
                if (ChoiseCorrect == 0)
                {
                    ChoiseCorrect = 2;
                    Positive("Укажите количество для корректировки");
                    return;
                }
            }
        } // ReactionKeyDo

        internal override ABaseMode Cancel()
        {
            return JumpTo(new RefillChoise(SS, this));
        } // Cancel
        
        //----------------------------------
        /// <summary>
        /// 
        /// </summary>
        private void CorrectLine()
        {
            string TextQuery =
                "begin tran; " +
                "update DT$АдресПеремещение " +
                    "set $АдресПеремещение.Количество = :count " +
                    "where DT$АдресПеремещение .iddoc = :iddoc and DT$АдресПеремещение .lineno_ = :currline; " +
                "if @@rowcount > 0 begin " +
                    "insert into DT$АдресПеремещение ($АдресПеремещение.Товар , $АдресПеремещение.Количество ," +
                        "$АдресПеремещение.Единица , $АдресПеремещение.Коэффициент , $АдресПеремещение.Состояние0 ," +
                        "$АдресПеремещение.Состояние1 , $АдресПеремещение.Сотрудник0 , $АдресПеремещение.Адрес0 , $АдресПеремещение.Адрес1 ," +
                        "$АдресПеремещение.Дата0 , $АдресПеремещение.Дата1 , $АдресПеремещение.Время0 , $АдресПеремещение.Время1 ," +
                        "$АдресПеремещение.Док , $АдресПеремещение.НомерСтрокиДока , $АдресПеремещение.ФлагДопроведения , $АдресПеремещение.ФлагОбязательногоАдреса , " +
                        "$АдресПеремещение.ЕдиницаШК , lineno_, iddoc) " +
                        "select $АдресПеремещение.Товар , :CountCorrect ," +
                            "$АдресПеремещение.Единица , $АдресПеремещение.Коэффициент , $АдресПеремещение.Состояние0 ," +
                            ":AdressCode , :Employer , $АдресПеремещение.Адрес0 , $АдресПеремещение.Адрес0 ," +
                            "$АдресПеремещение.Дата0 , :NowDate , $АдресПеремещение.Время0 , :NowTime ," +
                            "$АдресПеремещение.Док , $АдресПеремещение.НомерСтрокиДока , $АдресПеремещение.ФлагДопроведения , $АдресПеремещение.ФлагОбязательногоАдреса , " +
                            "$АдресПеремещение.ЕдиницаШК , (select max(lineno_) + 1 from DT$АдресПеремещение where iddoc = :iddoc), iddoc " +
                        "from DT$АдресПеремещение as ForInst where ForInst.iddoc = :iddoc and ForInst.lineno_ = :currline; " +
                        "if @@rowcount > 0 begin " +
                            //Удалим строчку если она иссякла...
                            "delete from DT$АдресПеремещение " + 
                                "where iddoc = :iddoc and lineno_ = :currline and $АдресПеремещение.Количество <= 0 " +
                            " commit tran " + 
                        "end else rollback tran " +
                     "end " +
                  "else rollback";
            SQL1S.QuerySetParam(ref TextQuery, "count",         Amount - InputedCount);
            SQL1S.QuerySetParam(ref TextQuery, "CountCorrect",  InputedCount);
            SQL1S.QuerySetParam(ref TextQuery, "iddoc",         DocAP.ID);
            SQL1S.QuerySetParam(ref TextQuery, "currline",      lineno_);
            SQL1S.QuerySetParam(ref TextQuery, "Reason",        "   2EV   ");
            SQL1S.QuerySetParam(ref TextQuery, "AdressCode",    17);
            SQL1S.QuerySetParam(ref TextQuery, "Employer",      Employer.ID);

            SS.ExecuteWithoutReadNew(TextQuery);
            PreviousAction = "Корректировка принята " + Item.InvCode + " - " + InputedCount.ToString() + " шт. (недостача)";

            //Все скорректировали к хуям!
            if (Amount - InputedCount <= 0)
            {
                Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
                DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"] = SS.ExtendID(DocAP.ID, "АдресПеремещение");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = SS.ExtendID(Employer.ID, "Спр.Сотрудники");
                string resultMessage;
                SS.ExecCommandOnlyResultNew("RefillSetAddItem2", DataMapWrite, out resultMessage);
            }

            Cancel();
        } // CorrectLine
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Count"></param>
        private void EnterCount()
        {
            if (ChoiseCorrect == 0)
            {
                Negative("Неверно! Укажите причину!");
                return;
            }
            if (Amount < InputedCount || InputedCount <= 0)
            {
                Negative("Количество указано неверно!");
                return;
            }
            SS.OnReport(new ReportEventArgs("Фиксирую..."));
            CorrectLine();
        } // EnterCount
    }
}
