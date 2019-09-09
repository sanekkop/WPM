using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    partial class Model
    {
        //VARIABLES
        public ActionSet CurrentAction;
        //use DocSet in setInicialization
        public int AllSetsRow;

        //BASE METHOD's
        private bool ToModeSet(string AdressID, string iddoc)
        {
            foreach (string id in DocsSet)
            {
                if (!LockDoc(id))
                {
                    return false;
                }
            }

            string TextQuery =
                "DECLARE @curdate DateTime; " +
                    "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " +
                "select top 1 " +
                    "DocCC.$КонтрольНабора.Товар as ID, " +
                    "DocCC.lineno_ as LINENO_, " +
                    "Goods.descr as ItemName, " +
                    "Goods.$Спр.Товары.ИнвКод as InvCode, " +
                    "Goods.$Спр.Товары.КоличествоДеталей as Details, " +
                    "DocCC.$КонтрольНабора.Количество as Count, " +
                    "DocCC.$КонтрольНабора.Адрес0 as Adress, " +
                    "DocCC.$КонтрольНабора.Цена as Price, " +
                    "Sections.descr as AdressName, " +
                    "ISNULL(AOT.Balance, 0) as Balance, " +
                    //Реквизиты документа
                    "DocCC.iddoc as IDDOC, " +
                    "journForBill.docno as DocNo, " +
                    "CONVERT(char(8), CAST(LEFT(journForBill.date_time_iddoc, 8) as datetime), 4) as DateDoc, " +
                    "journForBill.iddoc as Bill, " +
                    "DocCCHead.$КонтрольНабора.КолСтрок as Rows, " +
                    "Sector.descr as Sector, " +
                    "DocCCHead.$КонтрольНабора.Сумма as Sum, " +
                    "DocCCHead.$КонтрольНабора.НомерЛиста as Number, " +
                    "DocCCHead.$КонтрольНабора.ФлагСамовывоза as SelfRemovel, " +
                    "Clients.descr as Client, " +
                    "Bill.$Счет.ТипНакладной as TypeNakl, " +
                    "isnull(DocCCHead.$КонтрольНабора.Коробка , :EmptyID) as BoxID, " +
                    "AdressBox.descr as Box " +
                "from " +
                    "DT$КонтрольНабора as DocCC (nolock) " +
                    "LEFT JOIN DH$КонтрольНабора as DocCCHead (nolock) " +
                        "ON DocCCHead.iddoc = DocCC.iddoc " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.id = DocCC.$КонтрольНабора.Товар " +
                    "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                        "ON Sections.id = DocCC.$КонтрольНабора.Адрес0 " +
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
                        "ON AOT.item = DocCC.$КонтрольНабора.Товар and AOT.adress = DocCC.$КонтрольНабора.Адрес0 " +
                    "LEFT JOIN DH$КонтрольРасходной as DocCB (nolock) " +
                        "ON DocCB.iddoc = DocCCHead.$КонтрольНабора.ДокументОснование " +
                    "LEFT JOIN DH$Счет as Bill (nolock) " +
                        "ON Bill.iddoc = DocCB.$КонтрольРасходной.ДокументОснование " +
                    "LEFT JOIN _1sjourn as journForBill (nolock) " +
                        "ON journForBill.iddoc = Bill.iddoc " +
                    "LEFT JOIN $Спр.Секции as Sector (nolock) " +
                        "ON Sector.id = DocCCHead.$КонтрольНабора.Сектор " +
                    "LEFT JOIN $Спр.Клиенты as Clients (nolock) " +
                        "ON Bill.$Счет.Клиент = Clients.id " +
                    "LEFT JOIN $Спр.Секции as AdressBox (nolock) " +
                        "ON AdressBox.id = DocCCHead.$КонтрольНабора.Коробка " +
                "where " +
                    "DocCC.iddoc in (:Docs) " +
                    "and DocCC.$КонтрольНабора.Дата5 = :EmptyDate " +
                    "and DocCC.$КонтрольНабора.Корректировка = 0 " +
                    "and DocCC.$КонтрольНабора.Количество > 0 " +
                    (AdressID == null ? "" : "and DocCC.$КонтрольНабора.Адрес0 = :Adress ") +
                    (iddoc == null ? "" : "and DocCC.iddoc = :iddoc ") +
                "order by " +
                    "DocCCHead.$КонтрольНабора.Сектор , Sections.$Спр.Секции.Маршрут , LINENO_";

            DataTable DT;
            TextQuery = TextQuery.Replace(":Docs", Helper.ListToStringWithQuotes(DocsSet));
            QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
            QuerySetParam(ref TextQuery, "Warehouse",   Const.MainWarehouse);
            if (iddoc != null)
            {
                QuerySetParam(ref TextQuery, "iddoc", iddoc);
            }
            if (AdressID != null)
            {
                QuerySetParam(ref TextQuery, "Adress", AdressID);
            }
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count == 0)
            {
                if (AdressID == null)
                {
                    return ToModeSetComplete();
                }
                else
                {
                    //Это попытка перепрыгнуть
                    FExcStr = "Нет такого адреса в сборочном!";
                    return false;
                }
            }

            RefreshRowSum();    //Подтянем циферки

            CCItem = new StructItemSet();
            CCItem.ID = DT.Rows[0]["id"].ToString();
            CCItem.InvCode = DT.Rows[0]["InvCode"].ToString();
            CCItem.Name = DT.Rows[0]["ItemName"].ToString();
            CCItem.Price = (decimal)DT.Rows[0]["Price"];
            CCItem.Count = (int)(decimal)DT.Rows[0]["Count"];
            CCItem.CountFact = (int)(decimal)DT.Rows[0]["Count"];
            CCItem.AdressID = DT.Rows[0]["Adress"].ToString();
            CCItem.AdressName = DT.Rows[0]["AdressName"].ToString();
            CCItem.CurrLine = (short)DT.Rows[0]["LINENO_"];
            CCItem.Balance = (int)(decimal)DT.Rows[0]["Balance"];
            CCItem.Details = (int)(decimal)DT.Rows[0]["Details"];

            if (!MultiplesOKEI2(ref CCItem))
            {
                return false;
            }

            DocSet = new StrictDoc();
            DocSet.ID = DT.Rows[0]["IDDOC"].ToString();
            DocSet.SelfRemovel = (int)(decimal)DT.Rows[0]["SelfRemovel"];
            DocSet.View = (DocSet.SelfRemovel == 1 ? "(C) " : "") + DT.Rows[0]["Sector"].ToString().Trim() + "-" + DT.Rows[0]["Number"].ToString() + " Заявка " + DT.Rows[0]["DocNo"].ToString() + " (" + DT.Rows[0]["DateDoc"].ToString() + ")";
            DocSet.Rows = (int)(decimal)DT.Rows[0]["Rows"];
            DocSet.FromWarehouseID = DT.Rows[0]["Bill"].ToString();
            DocSet.Client = DT.Rows[0]["Client"].ToString().Trim();
            DocSet.Sum = (decimal)DT.Rows[0]["Sum"];
            DocSet.Special = (int)(decimal)DT.Rows[0]["TypeNakl"] == 2 ? true : false;
            DocSet.Box = DT.Rows[0]["Box"].ToString();
            DocSet.BoxID = DT.Rows[0]["BoxID"].ToString();

            CurrentAction = ActionSet.ScanAdress;
            FExcStr = WhatUNeed();
            FCurrentMode = Mode.Set;
            return true;
        } // ToModeSet
        private bool QuitModesSet()
        {
            foreach (string id in DocsSet)
            {
                if (!LockoutDoc(id))
                {
                    return false;
                }
            }
            return true;
        } // QuitModesSet
        private bool RSCSet(string IDDorID, bool thisID)
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
                            //Переход на другую строку
                            return ToModeSet(Adress.ID, null);
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
                    else if (CurrentAction == ActionSet.ScanBox)
                    {
                        //СКАНИРОВАНИЕ КОРОБКИ
                        if (Adress.Type != 12)
                        {
                            FExcStr = "Неверно! " + WhatUNeed();
                            return false;
                        }
                        if (Adress.ID != DocSet.BoxID)
                        {
                            FExcStr = "Неверная коробка! " + WhatUNeed();
                            return false;
                        }
                        return CompleteLineSet();   //ВСЕ ЗАВЕРШАЕМ СТРОКУ ТУТ
                    }
                    else
                    {
                        //Какой-то другой режим вероятно?
                        FExcStr = "Неверно! " + WhatUNeed();
                        return false;
                    }
                }
                else if (IsSC(IDDorID, "Принтеры"))
                {
                    if (!Printer.FoundIDD(IDDorID))
                    {
                        return false;
                    }
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
        } // RSCSet
        private bool RBSet(string Barcode)
        {
            if (CurrentAction != ActionSet.ScanItem)
            {
                FExcStr = "Неверно! " + WhatUNeed();
                return false;
            }
            string TextQuery =
                "SELECT " +
                    "Units.parentext as ItemID, " +
                    "Goods.$Спр.Товары.ИнвКод as InvCode, " +
                    "Units.$Спр.ЕдиницыШК.ОКЕИ as OKEI " +
                "FROM $Спр.ЕдиницыШК as Units (nolock) " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.id = Units.parentext " +
                "WHERE Units.$Спр.ЕдиницыШК.Штрихкод = :Barcode ";
            QuerySetParam(ref TextQuery, "Barcode", Barcode);
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

        //SPECIAL METHOD's
        private bool LoadDocSet(string iddoc)
        {
            string TextQuery =
                "SELECT top 1 " +
                    "journ.iddoc as IDDOC, " +
                    "journForBill.docno as DocNo, " +
                    "CONVERT(char(8), CAST(LEFT(journForBill.date_time_iddoc, 8) as datetime), 4) as DateDoc, " +
                    "journForBill.iddoc as Bill, " +
                    "DocCC.$КонтрольНабора.КолСтрок as Rows, " +
                    "Section.descr as Sector, " +
                    "DocCC.$КонтрольНабора.Сумма as Sum, " +
                    "DocCC.$КонтрольНабора.НомерЛиста as Number, " +
                    "DocCC.$КонтрольНабора.ФлагСамовывоза as SelfRemovel, " +
                    "Clients.descr as Client, " +
                    "Bill.$Счет.ТипНакладной as TypeNakl " +
                "FROM " +
                    "_1sjourn as journ (nolock) " +
                    "INNER JOIN DH$КонтрольНабора as DocCC (nolock) " +
                        "ON DocCC.iddoc = journ.iddoc " +
                    "LEFT JOIN DH$КонтрольРасходной as DocCB (nolock) " +
                        "ON DocCB.iddoc = DocCC.$КонтрольНабора.ДокументОснование " +
                    "LEFT JOIN DH$Счет as Bill (nolock) " +
                        "ON Bill.iddoc = DocCB.$КонтрольРасходной.ДокументОснование " +
                    "LEFT JOIN _1sjourn as journForBill (nolock) " +
                        "ON journForBill.iddoc = Bill.iddoc " +
                    "LEFT JOIN $Спр.Секции as Section (nolock) " +
                        "ON Section.id = DocCC.$КонтрольНабора.Сектор " +
                    "LEFT JOIN $Спр.Клиенты as Clients (nolock) " +
                        "ON Bill.$Счет.Клиент = Clients.id " +
                    "WHERE " +
                        "journ.iddoc = :iddoc ";
            QuerySetParam(ref TextQuery, "iddoc", iddoc);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count > 0)
            {
                DocSet = new StrictDoc();
                DocSet.ID = DT.Rows[0]["IDDOC"].ToString();
                DocSet.SelfRemovel = (int)(decimal)DT.Rows[0]["SelfRemovel"];
                DocSet.View = (DocSet.SelfRemovel == 1 ? "(C) " : "") + DT.Rows[0]["Sector"].ToString().Trim() + "-" + DT.Rows[0]["Number"].ToString() + " Заявка " + DT.Rows[0]["DocNo"].ToString() + " (" + DT.Rows[0]["DateDoc"].ToString() + ")";
                DocSet.Rows = (int)(decimal)DT.Rows[0]["Rows"];
                DocSet.FromWarehouseID = DT.Rows[0]["Bill"].ToString();
                DocSet.Client = DT.Rows[0]["Client"].ToString().Trim();
                DocSet.Sum = (decimal)DT.Rows[0]["Sum"];
                DocSet.Special = (int)(decimal)DT.Rows[0]["TypeNakl"] == 2 ? true : false;
                return true;
            }

            return false;
        }

        private bool MultiplesOKEI2(ref StructItemSet Item)
        {
            string TextQuery =
                "SELECT " +
                    "isnull(OKEI2.descr, OKEI.descr) as Name, " +
                    "CAST(Units.$Спр.ЕдиницыШК.Коэффициент as int) as Coef, " +
                    "ceiling(:amount/$Спр.ЕдиницыШК.Коэффициент ) as AmountOKEI2 " +
                "FROM " +
                    "$Спр.ЕдиницыШК as Units (nolock) " +
                    "inner join $Спр.ОКЕИ as OKEI (nolock) " +
                        "on OKEI.id = Units.$Спр.ЕдиницыШК.ОКЕИ " +
                    "left join $Спр.ОКЕИ as OKEI2 (nolock) " + 
                        "on OKEI2.id = Units.$Спр.ЕдиницыШК.ОКЕИ2 " +
                "WHERE " +
                    "Units.parentext = :CurrentItem " +
                    "and OKEI.id <> :OKEIKit" + 
                    "and Units.ismark = 0 " +
                    "and ceiling(:amount/$Спр.ЕдиницыШК.Коэффициент )*$Спр.ЕдиницыШК.Коэффициент = :amount " + 
                "order by AmountOKEI2";
            QuerySetParam(ref TextQuery, "CurrentItem", Item.ID);
            QuerySetParam(ref TextQuery, "amount", Item.Count);
            QuerySetParam(ref TextQuery, "OKEIKit", OKEIKit);

            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                Item.OKEI2Count = Item.Count;
                Item.OKEI2 = "шт";
                Item.OKEI2Coef = 1;
            }
            else
            {
                //Бред какой-то, но так, чтобы не звонили по ночам
                Item.OKEI2Count = (int)(decimal)DT.Rows[0]["AmountOKEI2"];
                Item.OKEI2 = DT.Rows[0]["Name"].ToString();
                Item.OKEI2Coef = (int)DT.Rows[0]["Coef"];
            }
            return true;
        } // MultiplesOKEI2

        public string WhatUNeed(ActionSet currAction)
        {
            string Result = "";
            switch (currAction)
            {
                case ActionSet.ScanAdress:
                    Result = "Отсканируйте адрес!";
                    break;

                case ActionSet.ScanItem:
                    Result = "Отсканируйте товар!";
                    break;

                case ActionSet.EnterCount:
                    Result = "Введите количество! (подтверждать - 'зеленой')";
                    break;

                case ActionSet.ScanPart:
                    Result = "Отсканируйте спец. ШК деталей! " + CCItem.InvCode.Trim() + (CCItem.Details == 99 ? " (особая)" : " (деталей: " + CCItem.Details.ToString() + ")");
                    break;

                case ActionSet.ScanBox:
                    Result = "Отсканируйте коробку!";
                    break;

                case ActionSet.ScanPallete:
                    Result = "Отсканируйте паллету!";
                    break;
            }
            return Result;
        }
        private string WhatUNeed()
        {
            return WhatUNeed(CurrentAction);
        }  // WhatUNeed
        private bool RefreshRowSum()
        {
            string TextQuery =
                "select " +
                    "sum(DocCC.$КонтрольНабора.Сумма ) as Sum, " +
                    "count(*) Amount " +
                "from " +
                    "DT$КонтрольНабора as DocCC (nolock) " +
                "where " +
                    "DocCC.iddoc in (:Docs ) " +
                    "and DocCC.$КонтрольНабора.Дата5 = :EmptyDate " +
                    "and DocCC.$КонтрольНабора.Корректировка = 0 " +
                    "and DocCC.$КонтрольНабора.Количество > 0 ";
            TextQuery = TextQuery.Replace(":Docs", Helper.ListToStringWithQuotes(DocsSet));
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                DocSet.Sum = (decimal)DT.Rows[0]["Sum"];
                AllSetsRow = (int)DT.Rows[0]["Amount"];
            }
            else
            {
                DocSet.Sum = 0;
                AllSetsRow = 0;
            }
            return true;
        }
        public bool RefreshCCRP()
        {
            RefreshRowSum();

            string TextQuery =
                "select " +
                    "DocCC.lineno_ as Number, " +
                    "Goods.$Спр.Товары.ИнвКод as InvCode, " +
                    "DocCC.$КонтрольНабора.Сумма as Sum, " +
                    "DocCC.$КонтрольНабора.Количество as Count, " +
                    "Sections.descr as Adress " +
                "from " +
                    "DT$КонтрольНабора as DocCC (nolock) " +
                    "LEFT JOIN DH$КонтрольНабора as DocCCHead (nolock) " +
                        "ON DocCCHead.iddoc = DocCC.iddoc " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.id = DocCC.$КонтрольНабора.Товар " +
                    "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                        "ON Sections.id = DocCC.$КонтрольНабора.Адрес0 " +
                "where " +
                    "DocCC.iddoc in ( :Docs) " +
                    "and DocCC.$КонтрольНабора.Дата5 = :EmptyDate " +
                    "and DocCC.$КонтрольНабора.Корректировка = 0 " +
                    "and DocCC.$КонтрольНабора.Количество > 0 " +
                "order by " +
                    "DocCCHead.$КонтрольНабора.Сектор , Sections.$Спр.Секции.Маршрут , Number";
            TextQuery = TextQuery.Replace(":Docs", Helper.ListToStringWithQuotes(DocsSet));
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            if (!ExecuteWithRead(TextQuery, out CCRP))
            {
                return false;
            }
            return true;
        } // RefreshCCRP
        public bool CompleteLineSet()
        {
            string TextQuery;
            //Заглушка, рефрешим позицию, чтобы не было проблем, если оборвется связь
            int CountFact = CCItem.CountFact;
            if (!ToModeSet(CCItem.AdressID, DocSet.ID))
            {
                CCItem.CountFact = CountFact;
                CurrentAction = DocSet.BoxID == SQL1S.GetVoidID() ? ActionSet.ScanAdress : ActionSet.ScanBox;   //Отключение по константе
                return false;
            }
            CCItem.CountFact = CountFact;
            CurrentAction = DocSet.BoxID == SQL1S.GetVoidID() ? ActionSet.ScanAdress : ActionSet.ScanBox;   //Отключение по константе
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
                "update DT$КонтрольНабора " +
                    "set $КонтрольНабора.Количество = :remaincount, " +
                          "$КонтрольНабора.Сумма =  :remaincount*$КонтрольНабора.Цена " +
                    "where DT$КонтрольНабора .iddoc = :iddoc and DT$КонтрольНабора .lineno_ = :currline; " +
                "if @@rowcount > 0 begin " +
                    "insert into DT$КонтрольНабора ($КонтрольНабора.СтрокаИсх , $КонтрольНабора.Товар , $КонтрольНабора.Количество ," +
                        "$КонтрольНабора.Единица , $КонтрольНабора.Цена , $КонтрольНабора.Коэффициент , $КонтрольНабора.Сумма ," +
                        "$КонтрольНабора.Секция , $КонтрольНабора.Корректировка , $КонтрольНабора.ПричинаКорректировки , $КонтрольНабора.ЕдиницаШК ," +
                        "$КонтрольНабора.Состояние0 , $КонтрольНабора.Адрес0 , $КонтрольНабора.СостояниеКорр , $КонтрольНабора.АдресКорр ," +
                        "$КонтрольНабора.ДокБлокировки , $КонтрольНабора.Дата5 , $КонтрольНабора.Время5 , $КонтрольНабора.Контейнер , " +
                        "lineno_, iddoc, $КонтрольНабора.Контроль ) " +
                        "select $КонтрольНабора.СтрокаИсх , $КонтрольНабора.Товар , :count ," +
                            "$КонтрольНабора.Единица , $КонтрольНабора.Цена , $КонтрольНабора.Коэффициент , :count*$КонтрольНабора.Цена ," +
                            "$КонтрольНабора.Секция , $КонтрольНабора.Корректировка , $КонтрольНабора.ПричинаКорректировки , $КонтрольНабора.ЕдиницаШК ," +
                            "$КонтрольНабора.Состояние0 , $КонтрольНабора.Адрес0 , $КонтрольНабора.СостояниеКорр , $КонтрольНабора.АдресКорр ," +
                            "$КонтрольНабора.ДокБлокировки , $КонтрольНабора.Дата5 , $КонтрольНабора.Время5 , $КонтрольНабора.Контейнер , " +
                            "(select max(lineno_) + 1 from DT$КонтрольНабора where iddoc = :iddoc), iddoc, 0 " +
                        "from DT$КонтрольНабора as ForInst where ForInst.iddoc = :iddoc and ForInst.lineno_ = :currline; " +
                        "select max(lineno_) as newline from DT$КонтрольНабора where iddoc = :iddoc; " +
                        "if @@rowcount = 0 rollback tran else commit tran " +
                     "end " +
                  "else rollback";
                QuerySetParam(ref TextQuery, "count", CCItem.CountFact);
                QuerySetParam(ref TextQuery, "remaincount", CCItem.Count - CCItem.CountFact);
                QuerySetParam(ref TextQuery, "iddoc", DocSet.ID);
                QuerySetParam(ref TextQuery, "currline", CCItem.CurrLine);

                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }

                //Писать будем в добалвенную, так лучше! Поэтому обновляем корректную строчку
                CCItem.CurrLine = (short)DT.Rows[0]["newline"];
            }

            //фиксируем строку
            TextQuery = "UPDATE DT$КонтрольНабора WITH (rowlock) " +
                            "SET $КонтрольНабора.Дата5 = :Date5, " +
                              "$КонтрольНабора.Время5 = :Time5, " +
                              "$КонтрольНабора.Контейнер = :id " +
                            "WHERE DT$КонтрольНабора .iddoc = :DocCC and DT$КонтрольНабора .lineno_ = :lineno_; ";
            QuerySetParam(ref TextQuery, "id", GetVoidID());
            QuerySetParam(ref TextQuery, "DocCC", DocSet.ID);
            QuerySetParam(ref TextQuery, "Date5", DateTime.Now);
            QuerySetParam(ref TextQuery, "Time5", APIManager.NowSecond());
            QuerySetParam(ref TextQuery, "lineno_", CCItem.CurrLine);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            //Запись прошла успешно
            CurrentAction = ActionSet.ScanAdress;   //на всякий случай, если там что-нибудь наебнется, то во вьюхе по крайней мере нельзя будет повторно ввести количество
            PreviousAction = "Отобрано " + CCItem.InvCode.Trim() + " - " + CCItem.CountFact.ToString() + " шт. (строка " + Line.ToString() + ")";
            return ToModeSet(null, null);
        } // CompleteLineSet
        public bool EnterCountSet(int Count)
        {
            if (CurrentAction != ActionSet.EnterCount)
            {
                FExcStr = "Неверно! " + WhatUNeed();
                return false;
            }
            //if (Count <= 0 || (CCItem.OKEI2Count < Count))
            if (Count <= 0 || (CCItem.Count < Count))
            {
                FExcStr = "Количество указано неверно! (максимум " + CCItem.Count.ToString() + ")";
                //FExcStr = "Количество указано неверно! (максимум " + CCItem.OKEI2Count.ToString() + ")";
                return false;
            }
            CCItem.CountFact = Count;
            //CCItem.CountFact = Count* CCItem.OKEI2Coef;
            if (DocSet.BoxID == SQL1S.GetVoidID())
            {//Отключение коробок по константе
                return CompleteLineSet();
            }
            CurrentAction = ActionSet.ScanBox;
            FExcStr = WhatUNeed();
            return true;

        } // EnterCountSet
        public bool ScanPartBarcode(int CountPart)
        {
            if (CurrentAction != ActionSet.ScanPart)
            {
                FExcStr = "Неверно! " + WhatUNeed();
                return false;
            }
            else if (CountPart != CCItem.Details)
            {
                FExcStr = "Количество деталей неверно! " + WhatUNeed();
                return false;
            }
            CurrentAction = ActionSet.ScanItem;
            FExcStr = WhatUNeed();
            return true;
        } // ScanPartBarcode
        
    }
}
