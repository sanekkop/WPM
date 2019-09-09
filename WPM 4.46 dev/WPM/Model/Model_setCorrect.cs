using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    partial class Model
    {
        //BASE METHOD's
        public bool ToModeSetCorrect()
        {
            FExcStr = "Укажите причину корректировки!";
            FCurrentMode = Mode.SetCorrect;
            return true;
        }

        //SPECIAL METHOD's
        public bool CompleteCorrect(int Choise, int CountCorrect)
        {
            //Заглушка, рефрешим позицию, чтобы не было проблем, если оборвется связь
            if (!ToModeSet(CCItem.AdressID, DocSet.ID))
            {
                FCurrentMode = Mode.SetCorrect;
                return false;
            }
            FCurrentMode = Mode.SetCorrect;
            //конец заглушки

            if (CountCorrect <= 0 || CountCorrect > CCItem.Count)
            {
                FExcStr = "Нельзя скорректировать столько!";
                return false;
            }

            int AdressCode;
            string CorrectReason;
            string What;
            switch (Choise)
            {
                case 1:
                    AdressCode = 7;
                    CorrectReason = "   2EU   ";
                    What = "брак";
                    break;
                case 2:
                    AdressCode = 12;
                    CorrectReason = "   2EV   ";
                    What = "недостача";
                    break;
                case 3:
                    AdressCode = 2;
                    CorrectReason = "   2EW   ";
                    What = "отказ";
                    break;
                case 4:
                    AdressCode = 2;
                    CorrectReason = "   4MG   ";
                    What = "отказ по ШК";
                    break;
                default:
                    FExcStr = "Неясная причина корректировки!";
                    return false;
            }

            string TextQuery =
                "begin tran; " +
                "update DT$КонтрольНабора " +
                    "set $КонтрольНабора.Количество = :count, " +
                        "$КонтрольНабора.Сумма = $КонтрольНабора.Цена *:count " +
                    "where DT$КонтрольНабора .iddoc = :iddoc and DT$КонтрольНабора .lineno_ = :currline; " +
                "if @@rowcount > 0 begin " +
                    "insert into DT$КонтрольНабора ($КонтрольНабора.СтрокаИсх , $КонтрольНабора.Товар , $КонтрольНабора.Количество ," +
                        "$КонтрольНабора.Единица , $КонтрольНабора.Цена , $КонтрольНабора.Коэффициент , $КонтрольНабора.Сумма ," +
                        "$КонтрольНабора.Секция , $КонтрольНабора.Корректировка , $КонтрольНабора.ПричинаКорректировки , $КонтрольНабора.ЕдиницаШК ," +
                        "$КонтрольНабора.Состояние0 , $КонтрольНабора.Адрес0 , $КонтрольНабора.СостояниеКорр , $КонтрольНабора.АдресКорр ," +
                        "$КонтрольНабора.ДокБлокировки , $КонтрольНабора.Дата5 , $КонтрольНабора.Время5 , $КонтрольНабора.Контейнер , " +
                        "lineno_, iddoc, $КонтрольНабора.Контроль ) " +
                        "select $КонтрольНабора.СтрокаИсх , $КонтрольНабора.Товар , :CountCorrect ," +
                            "$КонтрольНабора.Единица , $КонтрольНабора.Цена , $КонтрольНабора.Коэффициент , $КонтрольНабора.Цена * :CountCorrect A," +
                            "$КонтрольНабора.Секция , :CountCorrect , :Reason, $КонтрольНабора.ЕдиницаШК ," +
                            "$КонтрольНабора.Состояние0 , $КонтрольНабора.Адрес0 , :AdressCode , $КонтрольНабора.Адрес0 ," +
                            "$КонтрольНабора.ДокБлокировки , $КонтрольНабора.Дата5 , $КонтрольНабора.Время5 , $КонтрольНабора.Контейнер , " +
                            "(select max(lineno_) + 1 from DT$КонтрольНабора where iddoc = :iddoc), iddoc, 0 " +
                        "from DT$КонтрольНабора as ForInst where ForInst.iddoc = :iddoc and ForInst.lineno_ = :currline; " +
                       "if @@rowcount = 0 rollback tran else commit tran " +
                     "end " +
                  "else rollback";
            QuerySetParam(ref TextQuery, "count", CCItem.Count - CountCorrect);
            QuerySetParam(ref TextQuery, "CountCorrect", CountCorrect);
            QuerySetParam(ref TextQuery, "iddoc", DocSet.ID);
            QuerySetParam(ref TextQuery, "currline", CCItem.CurrLine);
            QuerySetParam(ref TextQuery, "Reason", CorrectReason);
            QuerySetParam(ref TextQuery, "AdressCode", AdressCode);

            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            PreviousAction = "Корректировка принята " + CCItem.InvCode.Trim() + " - " + CountCorrect.ToString() + " шт. (" + What + ")";

            if (CountCorrect == CCItem.Count)
            {
                return ToModeSet(null, null);   //похуй на какую, т.к. скорректировали полностью
            }
            else
            {
                return ToModeSet(null, DocSet.ID);  //на определенную, так как что-то еще осталось
            }
        } // CompleteCorrect
    }
}
