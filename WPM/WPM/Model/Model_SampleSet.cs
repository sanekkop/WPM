using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    partial class Model
    {
        private bool ToModeSampleSet()
        {
            FExcStr = null;
            PreviousAction = "";

            // проверим, есть ли сведения в регистре АдресЗаданияСотрудников об уже полученных заданиях на набор образцов
            DataTable DT;
            string TextQuery =
                "SELECT " +
                    "SP4361 as DocID " +
                "FROM " +
                    "RG4365 as Reg " +
                "WHERE " +
                    "SP4360 = :Employer and SP4362 = 4 and SP4364 = 1";
            SQL1S.QuerySetParam(ref TextQuery, "Employer", Employer.ID);

            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                DocSet.ID = DT.Rows[0]["DocID"].ToString().Substring(4);
            }
            // если за сотрудником заданий не числится, запрошиваем у 1с
            else
            {
                Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Employer.ID, "Спр.Сотрудники");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"] = ExtendID(Employer.GetAttribute("РоднойСектор").ToString(), "Спр.Секции");

                Dictionary<string, object> DataMapRead;
                List<string> FieldList = new List<string>();
                FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
                if (!ExecCommand("QuestPicingSample", DataMapWrite, FieldList, out DataMapRead))
                {
                    return false;
                }
                if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] == -3)
                {
                    FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
                    return false;
                }
                if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] != 3)
                {
                    FExcStr = "Не известный ответ робота... я озадачен...";
                    return false;
                }
                string SampleDoc = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
                DocSet.ID = SampleDoc.Substring(0, 7);
            }
            FCurrentMode = Mode.SampleSet;

            ReactionDoc(DocSet.ID);
            return true;
        }

        private bool CompleteLineSampleSet()
        {

            string TextQuery;
            //Заглушка, рефрешим позицию, чтобы не было проблем, если оборвется связь
            int CountFact = CCItem.CountFact;
            if (!RDSampleSet(DocSet.ID))
            {
                CCItem.CountFact = CountFact;
                return false;
            }
            CCItem.CountFact = CountFact;
            // текущим событием снова должен встать СканАдреса, тк набор образцов продолжается
            CurrentAction = ActionSet.ScanAdress;   //Отключение по константе
            //конец заглушки

            int Line = CCItem.CurrLine;
            if (CCItem.Count > CCItem.CountFact)
            {
                if (Const.StopCorrect)
                {
                    FExcStr = "Возможность дробить строку отключена!";
                    return false;
                }
                //добавить строчку надо
                TextQuery =
                    "begin tran; " +
                    "update DT$АдресПеремещение " +
                        "set $АдресПеремещение.Количество = :remaincount " +
                        "where DT$АдресПеремещение .iddoc = :iddoc and DT$АдресПеремещение .lineno_ = :currline; " +
                    "if @@rowcount > 0 begin " +
                        "insert into DT$АдресПеремещение ($АдресПеремещение.Товар , $АдресПеремещение.Количество ," +
                            "$АдресПеремещение.Единица , $АдресПеремещение.Коэффициент , $АдресПеремещение.Состояние0 ," +
                            "$АдресПеремещение.Состояние1 , $АдресПеремещение.Сотрудник0 , $АдресПеремещение.Адрес0 , $АдресПеремещение.Адрес1 ," +
                            "$АдресПеремещение.Дата0 , $АдресПеремещение.Дата1 , $АдресПеремещение.Время0 , $АдресПеремещение.Время1 ," +
                            "$АдресПеремещение.Док , $АдресПеремещение.НомерСтрокиДока , $АдресПеремещение.ФлагДопроведения , $АдресПеремещение.ФлагОбязательногоАдреса , " +
                            "lineno_, iddoc, $АдресПеремещение.ЕдиницаШК ) " +
                            "select $АдресПеремещение.Товар , :count ," +
                                "$АдресПеремещение.Единица , $АдресПеремещение.Коэффициент ," +
                                "$АдресПеремещение.Состояние0 , $АдресПеремещение.Состояние1 , :Employer , $АдресПеремещение.Адрес0 ," +
                                " :AdressCollect , $АдресПеремещение.Дата0 , :Date1 , $АдресПеремещение.Время0 , :Time1 , $АдресПеремещение.Док , $АдресПеремещение.НомерСтрокиДока , $АдресПеремещение.ФлагДопроведения , $АдресПеремещение.ФлагОбязательногоАдреса , " +
                                "(select max(lineno_) + 1 from DT$АдресПеремещение where iddoc = :iddoc), iddoc, $АдресПеремещение.ЕдиницаШК " +
                            "from DT$АдресПеремещение as ForInst where ForInst.iddoc = :iddoc and ForInst.lineno_ = :currline; " +
                           "if @@rowcount = 0 rollback tran else commit tran " +
                         "end " +
                     "else rollback";

                SQL1S.QuerySetParam(ref TextQuery, "count", CCItem.CountFact);
                SQL1S.QuerySetParam(ref TextQuery, "remaincount", CCItem.Count - CCItem.CountFact);
                SQL1S.QuerySetParam(ref TextQuery, "iddoc", DocSet.ID);
                SQL1S.QuerySetParam(ref TextQuery, "currline", CCItem.CurrLine);
                SQL1S.QuerySetParam(ref TextQuery, "AdressCollect", DocSet.AdressCollect);
                SQL1S.QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                SQL1S.QuerySetParam(ref TextQuery, "Date1", DateTime.Now);
                SQL1S.QuerySetParam(ref TextQuery, "Time1", APIManager.NowSecond());
                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
            }
            else
            {
                //фиксируем строку
                TextQuery = "UPDATE DT$АдресПеремещение WITH (rowlock) " +
                                "SET $АдресПеремещение.Дата1 = :Date1, " +
                                  "$АдресПеремещение.Время1 = :Time1, " +
                                  "$АдресПеремещение.Адрес1 = :AdressCollect " +
                                "WHERE DT4327.iddoc = :Doc and DT4327.lineno_ = :lineno_; ";
                SQL1S.QuerySetParam(ref TextQuery, "Doc", DocSet.ID);
                SQL1S.QuerySetParam(ref TextQuery, "Date1", DateTime.Now);
                SQL1S.QuerySetParam(ref TextQuery, "Time1", APIManager.NowSecond());
                SQL1S.QuerySetParam(ref TextQuery, "AdressCollect", DocSet.AdressCollect);
                SQL1S.QuerySetParam(ref TextQuery, "lineno_", CCItem.CurrLine);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }
            //Запись прошла успешно

            RefreshAmount();
            CurrentAction = ActionSet.ScanAdress;   //на всякий случай, если там что-нибудь наебнется, то во вьюхе по крайней мере нельзя будет повторно ввести количество
            PreviousAction = "Отобрано " + CCItem.InvCode.Trim() + " - " + CCItem.CountFact.ToString() + " шт. (строка " + Line.ToString() + ")";
            if (AllSetsRow > 0)
            {
                RDSampleSet(DocSet.ID);
            }
            //если в доке больше нет не отобранных, оповестим 1с о завершении набора образцов
            if (AllSetsRow == 0)
            {
                FExcStr = "Отсканируйте принтер";
            }
            return true;
        } //CompleteLineSampleSet

        public bool EnterCountSampleSet(int Count)
        {
            if (CurrentAction != ActionSet.EnterCount)
            {
                FExcStr = "Неверно! " + WhatUNeed();
                return false;
            }
            if (Count <= 0 || (CCItem.Count < Count))
            {
                FExcStr = "Количество указано неверно! (максимум " + CCItem.Count.ToString() + ")";
                return false;
            }
            CCItem.CountFact = Count;

            return CompleteLineSampleSet();

        } // EnterCountSampleSet
        private bool RDSampleSet(string IDD)
        {
            string IDDoc;
            string DocType;
            Dictionary<string, object> DataMap;
            if (!GetDoc(IDD, out IDDoc, out DocType, out DataMap))
            {
                return false;
            }
            if (DocType != "АдресПеремещение")
            {
                FExcStr = "Неверный тип документа!";
                return false;
            }
            DataTable DT;
            string TextQuery =
                "DECLARE @curdate DateTime; " +
                    "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " +
                "SELECT " +
                    "journ.iddoc as IDDOC, " +
                    "journ.docno as DocNo, " +
                    "journ.date_time_iddoc as DateDoc, " +
                    "DocAT.$АдресПеремещение.Товар as ID, " +
                    "DocAT.lineno_ as LINENO_, " +
                    "Goods.$Спр.Товары.КоличествоДеталей as Details, " +
                    "DocAT.$АдресПеремещение.Адрес0 as Adress0, " +
                    "min(Goods.descr) as ItemName, " +
                    "min(Goods.$Спр.Товары.ИнвКод ) as InvCode, " +
                    "sum(DocAT.$АдресПеремещение.Количество ) as Count, " +
                    "DocATStrings.$АдресПеремещение.КолСтрок as Rows, " +
                    "min(Sections.descr) as AdressName, " +
                    "ISNULL(AOT.Balance, 0) as Balance " +
                "FROM " +
                    "_1sjourn as journ (nolock) " +
                    "LEFT JOIN DT4327 as DocAT (nolock) ON DocAT.iddoc = journ.iddoc " +
                    "LEFT JOIN DH4327 as DocATStrings (nolock)ON DocAT.iddoc = DocATStrings.iddoc " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.ID = DocAT.$АдресПеремещение.Товар " +
                    "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                        "ON Sections.ID = DocAT.$АдресПеремещение.Адрес0 " +
                    "LEFT JOIN ( " +
                        "select " +
                            "RegAOT.$Рег.АдресОстаткиТоваров.Товар as item, " +
                            "RegAOT.$Рег.АдресОстаткиТоваров.Адрес as adress, " +
                            "sum(RegAOT.$Рег.АдресОстаткиТоваров.Количество ) as balance " +
                        "from " +
                            "RG$Рег.АдресОстаткиТоваров as RegAOT (nolock) " +
                        "where " +
                            "period = @curdate " +
                            "and $Рег.АдресОстаткиТоваров.Склад = :Warehouse " +
                            "and $Рег.АдресОстаткиТоваров.Состояние = 2 " +
                        "group by RegAOT.$Рег.АдресОстаткиТоваров.Товар , RegAOT.$Рег.АдресОстаткиТоваров.Адрес " +
                        ") as AOT " +
                        "ON AOT.item = DocAT.$АдресПеремещение.Товар and AOT.adress = DocAT.$АдресПеремещение.Адрес0 " +
                "WHERE " +
                    "DocAT.iddoc = :Doc " +
                    "and journ.ismark = 0 " +
                    "and DocATStrings.$АдресПеремещение.ТипДокумента = '3' " +
                    "and DocAT.$АдресПеремещение.Дата1 = :EmptyDate " +
                "GROUP BY journ.iddoc, journ.docno, journ.date_time_iddoc, DocAT.$АдресПеремещение.Товар ,LINENO_ ,  Goods.$Спр.Товары.КоличествоДеталей , DocAT.$АдресПеремещение.Адрес0 , DocATStrings.$АдресПеремещение.КолСтрок , Balance " +
                "ORDER BY max(DocAT.$АдресПеремещение.Дата0 ), max(DocAT.$АдресПеремещение.Время0 )";
            SQL1S.QuerySetParam(ref TextQuery, "Doc", IDD);
            SQL1S.QuerySetParam(ref TextQuery, "EmptyDate", SQL1S.GetVoidDate());
            SQL1S.QuerySetParam(ref TextQuery, "Warehouse", Const.MainWarehouse);

            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                ATDoc.ID = DT.Rows[0]["IDDOC"].ToString();
            }
            else
            {
                FExcStr = "Документ не найден!";
                return false;
            }

            DocSet = new StrictDoc();
            DocSet.View = " АдресПеремещение " + DT.Rows[0]["DocNo"].ToString() + " (" + DT.Rows[0]["DateDoc"].ToString() + ")";
            DocSet.ID = DT.Rows[0]["IDDOC"].ToString();
            DocSet.Rows = (int)(decimal)DT.Rows[0]["Rows"];

            //обновим количество не отобранных
            RefreshAmount();

            CCItem = new StructItemSet();
            CCItem.ID = DT.Rows[0]["ID"].ToString();
            CCItem.InvCode = DT.Rows[0]["InvCode"].ToString();
            CCItem.Name = DT.Rows[0]["ItemName"].ToString();
            CCItem.Count = (int)(decimal)DT.Rows[0]["Count"];
            CCItem.CountFact = (int)(decimal)DT.Rows[0]["Count"];
            CCItem.AdressID = DT.Rows[0]["Adress0"].ToString();
            CCItem.AdressName = DT.Rows[0]["AdressName"].ToString();
            CCItem.CurrLine = (short)DT.Rows[0]["LINENO_"];
            CCItem.Details = (int)(decimal)DT.Rows[0]["Details"];
            CCItem.Balance = (int)(decimal)DT.Rows[0]["Balance"];

            CurrentAction = ActionSet.ScanAdress;

            //получим АдресВременныый из склада в АдресПеремещениии
            TextQuery =
                "SELECT " +
                "WareHouse.SP4537 as AdresTemp " +
                "FROM DH4327 " +
                "LEFT JOIN sc31 as WareHouse on WareHouse.id = DH4327.SP4308 " +
                "WHERE DH4327.iddoc = :Doc ";
            SQL1S.QuerySetParam(ref TextQuery, "Doc", IDD);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            DocSet.AdressCollect = DT.Rows[0]["AdresTemp"].ToString();
            FExcStr = WhatUNeed();
            return true;
        }
        private bool RBSampleSet(string Barcode)
        {
            if (CurrentAction != ActionSet.ScanItem)
            {
                FExcStr = "Неверно! " + WhatUNeed();
                return false;
            }
            string TextQuery =
                "SELECT " +
                "Units.parentext as ItemID, Goods.$Спр.Товары.ИнвКод as InvCode " +
                "FROM _1sjourn as journ (nolock) " +
                    "LEFT JOIN DT4327 as DocAT (nolock) ON DocAT.iddoc = journ.iddoc " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) ON Goods.ID = DocAT.SP4312 " +
                    "LEFT JOIN $Спр.ЕдиницыШК as Units (nolock) ON Units.parentext = Goods.ID " +
                    "WHERE DocAT.SP4322 = '17530101 00:00:00.000' and Units.SP2233 = :Barcode and DocAT.SP4320 = :EmptyID and DocAT.iddoc = '" + DocSet.ID + "'";

            SQL1S.QuerySetParam(ref TextQuery, "EmptyID", SQL1S.GetVoidID());
            SQL1S.QuerySetParam(ref TextQuery, "Barcode", Barcode);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "С таким штрихкодом товар не найден! " + WhatUNeed();
                return false;
            }
            if (DT.Rows[0]["ItemID"].ToString() != CCItem.ID)
            {
                FExcStr = "Не тот товар! (отсканирован " + DT.Rows[0]["InvCode"].ToString().Trim() + ") " + WhatUNeed();
                return false;
            }
            CurrentAction = ActionSet.EnterCount;

            FExcStr = WhatUNeed();
            return true;
        }

        private bool RSCSampleSet(string IDDorID, bool thisID)
        {
            if (!thisID)
            {
                if (IsSC(IDDorID, "Сотрудники"))
                {
                    return ReactionCancel();
                }
                else if (IsSC(IDDorID, "Секции"))
                {
                    RefSection Adress = new RefSection(this);
                    if (!Adress.FoundIDD(IDDorID))
                    {
                        return false;
                    }

                    if (CurrentAction == ActionSet.ScanAdress)
                    {
                        //СКАНИРОВАНИЕ АДРЕСА
                        if (Adress.Type == 12)
                        {
                            FExcStr = "Неверно! " + WhatUNeed();
                            return false;
                        }
                        if (Adress.ID != CCItem.AdressID)
                        {
                            FExcStr = "Неверный адрес, отсканируйте заново!";
                            return false;
                        }
                        if (CCItem.Details > 0 && Const.ImageOn)
                        {
                            CurrentAction = ActionSet.ScanPart;
                        }
                        else
                        {
                            CurrentAction = ActionSet.ScanItem;
                        }
                        FExcStr = WhatUNeed();
                        return true;
                    }
                    else
                    {
                        //Какой-то другой режим вероятно?
                        FExcStr = "Неверно! " + WhatUNeed();
                    }
                }
                else if (IsSC(IDDorID, "Принтеры"))
                {
                    if (!Printer.FoundIDD(IDDorID))
                    {
                        return false;
                    }
                    //завершаем набор образцов и посылаем задание 1с

                    Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
                    DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"] = ExtendID(ATDoc.ID, "АдресПеремещение");
                    DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Printer.ID, "Спр.Принтеры");


                    Dictionary<string, object> DataMapRead;
                    List<string> FieldList = new List<string>();
                    FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
                    if (!ExecCommand("PicingSampleComplete", DataMapWrite, FieldList, out DataMapRead))
                    {
                        return false;
                    }
                    if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] == -3)
                    {
                        FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
                        return false;
                    }
                    if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] != 3)
                    {
                        FExcStr = "Не известный ответ робота... я озадачен...";
                        return false;
                    }
                    //отбор образцов окончен, выйдем в меню Выбора работы (образцы)
                    CurrentMode = Mode.ChoiseWorkSample;
                    OnChangeMode(new ChangeModeEventArgs(MM));
                    
                }
                else
                {
                    FExcStr = "Неверно! " + WhatUNeed();
                    return false;
                }
                return true;
            }
            else
            {
                FExcStr = "Нет действий с данным штрихкодом!";
                return false;
            }
        }
        private bool RefreshAmount()
        {
            string TextQuery =
                "select " +
                    "count(*) Amount " +
                "from " +
                    "DT$АдресПеремещение as Doc (nolock) " +
                "where " +
                    "Doc.iddoc =:Doc " +
                    "and Doc.$АдресПеремещение.Дата1 = :EmptyDate " +
                    "and Doc.$АдресПеремещение.Количество > 0 ";
            //TextQuery = TextQuery.Replace(":Docs", Helper.ListToStringWithQuotes(DocsSet));
            SQL1S.QuerySetParam(ref TextQuery, "Doc", DocSet.ID);
            SQL1S.QuerySetParam(ref TextQuery, "EmptyDate", SQL1S.GetVoidDate());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                AllSetsRow = (int)DT.Rows[0]["Amount"];
            }
            else
            {
                AllSetsRow = 0;
            }
            return true;
        }
    }
}
