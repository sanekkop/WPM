using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    partial class Model
    {
        public bool ToModeSampleSetCorrect()
        {
            FExcStr = "Укажите причину корректировки!";
            FCurrentMode = Mode.SampleSetCorrect;
            return true;
        }
        public bool CompleteCorrectSample(int Choise, int CountCorrect)
        {
            //Заглушка, рефрешим позицию, чтобы не было проблем, если оборвется связь
            if (!RDSampleSet(DocSet.ID))
            {
                FCurrentMode = Mode.SampleSetCorrect;
                return false;
            }
            FCurrentMode = Mode.SampleSetCorrect;
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
            string TextQuery;
            if (CountCorrect == CCItem.Count) //меняем скорректированную строку
            {
                TextQuery =
                "begin tran; " +
                    "update DT$АдресПеремещение " +
                        "set $АдресПеремещение.Количество = :CountCorrect, $АдресПеремещение.Состояние1 = :AdressCode, $АдресПеремещение.Адрес1 = $АдресПеремещение.Адрес0 ," +
                        "$АдресПеремещение.Дата1 = :Date1, $АдресПеремещение.Время1 = :Time1 " +
                        "where DT$АдресПеремещение .iddoc = :iddoc and DT$АдресПеремещение .lineno_ = :currline; " +
                "if @@rowcount = 0 rollback tran else commit tran ";
            }
            else   //меняем скорректированную строку и добавляем новую, исходя из количества
            {
                TextQuery =
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
                            "lineno_, iddoc, $АдресПеремещение.ЕдиницаШК ) " +
                            "select $АдресПеремещение.Товар , :CountCorrect ," +
                                "$АдресПеремещение.Единица , $АдресПеремещение.Коэффициент ," +
                                "$АдресПеремещение.Состояние0 , :AdressCode , :Employer , $АдресПеремещение.Адрес0 ," +
                                "$АдресПеремещение.Адрес0 , $АдресПеремещение.Дата0 , :Date1 , $АдресПеремещение.Время0 , :Time1 , $АдресПеремещение.Док , $АдресПеремещение.НомерСтрокиДока , $АдресПеремещение.ФлагДопроведения , $АдресПеремещение.ФлагОбязательногоАдреса , " +
                                "(select max(lineno_) + 1 from DT$АдресПеремещение where iddoc = :iddoc), iddoc, $АдресПеремещение.ЕдиницаШК " +
                            "from DT$АдресПеремещение as ForInst where ForInst.iddoc = :iddoc and ForInst.lineno_ = :currline; " +
                           "if @@rowcount = 0 rollback tran else commit tran " +
                         "end " +
                     "else rollback";
            }

            QuerySetParam(ref TextQuery, "iddoc", DocSet.ID);
            QuerySetParam(ref TextQuery, "currline", CCItem.CurrLine);
            QuerySetParam(ref TextQuery, "CountCorrect", CountCorrect);
            QuerySetParam(ref TextQuery, "Date1", DateTime.Now);
            QuerySetParam(ref TextQuery, "Time1", APIManager.NowSecond());
            QuerySetParam(ref TextQuery, "count", CCItem.Count - CountCorrect);
            QuerySetParam(ref TextQuery, "Reason", CorrectReason);
            QuerySetParam(ref TextQuery, "AdressCode", AdressCode);
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);

            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            PreviousAction = "Корректировка принята " + CCItem.InvCode.Trim() + " - " + CountCorrect.ToString() + " шт. (" + What + ")";

            return ToModeSampleSet();
        } // CompleteCorrectSample
    }
}
