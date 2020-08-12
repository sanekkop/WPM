using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    //Тут собраны методы необходимые для перехода в режимы
    partial class Model
    {
        //Вход в режимы
        private bool ToModeAcceptance()
        {
            if (FConsignment == null)
            {
                FConsignment = new DataTable();
                FConsignment.Columns.Add("Number", Type.GetType("System.Int32"));
                FConsignment.Columns.Add("ACID", Type.GetType("System.String"));
                FConsignment.Columns.Add("ParentIDD", Type.GetType("System.String"));
                FConsignment.Columns.Add("DOCNO", Type.GetType("System.String"));
                FConsignment.Columns.Add("DateDoc", Type.GetType("System.DateTime"));
                FConsignment.Columns.Add("DateDocText", Type.GetType("System.String"));
                FConsignment.Columns.Add("Client", Type.GetType("System.String"));
                FConsignment.Columns.Add("CountRow", Type.GetType("System.Int32"));
                FConsignment.Columns.Add("CountNotAcceptRow", Type.GetType("System.Int32"));
            }

            if (FNotAcceptedItems == null)
            {
                FNotAcceptedItems = new DataTable();
                FNotAcceptedItems.Columns.Add("ID", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("LINENO_", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("Number", Type.GetType("System.Int32")); //Номер строки документа
                FNotAcceptedItems.Columns.Add("ItemName", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("IDDOC", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("DOCNO", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("InvCode", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("Article", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("ArticleOnPack", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("ArticleFind", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("ArticleOnPackFind", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("ItemNameFind", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("Count", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("CountPackage", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("Coef", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("CoefView", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("LabelCount", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("Unit", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("Price", Type.GetType("System.Decimal"));
                FNotAcceptedItems.Columns.Add("Details", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("SeasonGroup", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("FlagFarWarehouse", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("StoregeSize", Type.GetType("System.Int32"));
                FAcceptedItems = FNotAcceptedItems.Clone();
            }
            else
            {
                FNotAcceptedItems.Rows.Clear();
                FAcceptedItems.Rows.Clear();
            }

            if (FPallets == null)
            {
                FPallets = new DataTable();
                FPallets.Columns.Add("ID", Type.GetType("System.String"));
                FPallets.Columns.Add("Barcode", Type.GetType("System.String"));
                FPallets.Columns.Add("Name", Type.GetType("System.String"));
                FPallets.Columns.Add("AdressID", Type.GetType("System.String"));

            }
            //занулим паллету
            //if (FCurrentMode != Mode.AcceptedItem)
            // {
            //    FPalletID = "";
            //}

            if (FConsignment.Rows.Count == 0)
            {
                //получим задание еще раз
                //Запрос задания у 1С
                Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Employer.ID, "Спр.Сотрудники");

                string resultMessage;
                if (!ExecCommandOnlyResultNew("QuestAcceptance", DataMapWrite, out resultMessage))
                {
                    FExcStr = "Ошибка запроса заданий!";
                    return false;
                }
                if (resultMessage.TrimEnd() == "Заданий нет!")
                {
                    FExcStr = "Заданий нет!";
                    return false;
                }
                //теперь подтянем смами документы
                String TextQuery = "SELECT " +
                "identity(int, 1, 1) as Number, " +
                "AC.iddoc as ACID," +
                "journ.iddoc as ParentIDD," +
                "Clients.descr as Client," +
                "AC.$АдресПоступление.КолСтрок as CountRow," +
                "journ.docno as DocNo," +
                "CAST(LEFT(journ.date_time_iddoc, 8) as datetime) as DateDoc," +
                "CONVERT(char(8), CAST(LEFT(journ.date_time_iddoc,8) as datetime), 4) as DateDocText " +
                "into #temp " +
                "FROM DH$АдресПоступление as AC (nolock) " +
                "LEFT JOIN _1sjourn as journ (nolock) " +
                "     ON journ.iddoc = right(AC.$АдресПоступление.ДокументОснование , 9) " +
                "LEFT JOIN DH$ПриходнаяКредит as PK (nolock) " +
                "     ON journ.iddoc = PK.iddoc " +
                "LEFT JOIN $Спр.Клиенты as Clients (nolock) " +
                "     ON PK.$ПриходнаяКредит.Клиент = Clients.id " +
                "WHERE AC.iddoc in (:Docs) " +
                " select * from #temp " +
                " drop table #temp ";
                TextQuery = TextQuery.Replace(":Docs", resultMessage);
                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                foreach (DataRow dr in DT.Rows)
                {
                    if (!LockDocAccept(dr["ACID"].ToString()))
                    {
                        DT.Rows.Remove(dr);
                    }
                }
                FConsignment.Merge(DT, false, MissingSchemaAction.Ignore);
                DT.Clear();

            }

            if (FConsignment.Rows.Count > 0)
            {
                DataRow[] DR = FConsignment.Select();
                string Docs = "";
                foreach (DataRow dr in DR)
                {
                    Docs += ", '" + dr["ACID"].ToString() + "'";
                }
                Docs = Docs.Substring(2);   //Убираем спедери запятые

                //Непринятый товар
                string TextQuery =
                    "SELECT " +
                        "right(Journ.docno,5) as DOCNO," +
                        "Supply.iddoc as iddoc," +
                        "Goods.id as id," +
                        "Goods.Descr as ItemName," +
                        "Goods.$Спр.Товары.ИнвКод as InvCode," +
                        "Goods.$Спр.Товары.Артикул as Article," +
                        "Goods.$Спр.Товары.АртикулНаУпаковке as ArticleOnPack," +
                        "Goods.$Спр.Товары.Прих_Цена as Price," +
                        "Goods.$Спр.Товары.КоличествоДеталей as Details, " +
                        "CASE WHEN round(Supply.$АдресПоступление.Количество /ISNULL(Package.Coef, 1), 0)*ISNULL(Package.Coef, 1) = Supply.$АдресПоступление.Количество " +
                            "THEN ISNULL(Package.Coef, 1) ELSE 1 END as Coef, " +
                        "CASE WHEN round(Supply.$АдресПоступление.Количество /ISNULL(Package.Coef, 1), 0)*ISNULL(Package.Coef, 1) = Supply.$АдресПоступление.Количество " +
                            "THEN round(Supply.$АдресПоступление.Количество /ISNULL(Package.Coef, 1), 0) " +
                            "ELSE Supply.$АдресПоступление.Количество END as CountPackage, " +
                        "Supply.$АдресПоступление.Количество as Count," +
                        "Supply.$АдресПоступление.ЕдиницаШК as Unit," +
                        "Supply.$АдресПоступление.КоличествоЭтикеток as LabelCount," +
                        "Supply.$АдресПоступление.НомерСтрокиДока as Number," +
                        "Supply.$АдресПоступление.ГруппаСезона as SeasonGroup, " +
                        "SypplyHeader.$АдресПоступление.ДальнийСклад as FlagFarWarehouse," +
                        "Supply.LineNO_ as LineNO_, " +
                    //"isnull(GS.$Спр.ТоварныеСекции.РазмерХранения , 0) as StoregeSize " +
                        "isnull(GS.$Спр.ТоварныеСекции.РасчетныйРХ , 0) as StoregeSize " +
                    "FROM " +
                        "DT$АдресПоступление as Supply (nolock) " +
                        "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                            "ON Goods.ID = Supply.$АдресПоступление.Товар " +
                        "LEFT JOIN DH$АдресПоступление as SypplyHeader (nolock) " +
                            "ON SypplyHeader.iddoc = Supply.iddoc " +
                        "LEFT JOIN _1sjourn as Journ (nolock) " +
                            "ON Journ.iddoc = Right(SypplyHeader.$АдресПоступление.ДокументОснование , 9) " +
                        "LEFT JOIN ( " +
                                "SELECT " +
                                    "Units.parentext as ItemID, " +
                                    "min(Units.$Спр.ЕдиницыШК.Коэффициент ) as Coef " +
                                "FROM " +
                                    "$Спр.ЕдиницыШК as Units (nolock) " +
                                "WHERE " +
                                    "Units.$Спр.ЕдиницыШК.ОКЕИ = :OKEIPackage " +
                                    "and Units.ismark = 0 " +
                                    "and not Units.$Спр.ЕдиницыШК.Коэффициент = 0 " +
                                "GROUP BY " +
                                    "Units.parentext " +
                                ") as Package " +
                            "ON Package.ItemID = Goods.ID " +
                        "LEFT JOIN $Спр.ТоварныеСекции as GS (nolock) " +
                            "on GS.parentext = goods.id and gs.$Спр.ТоварныеСекции.Склад = :Warehouse " +
                    "WHERE " +
                        "Supply.IDDOC in (:Docs) " +
                        "and Supply.$АдресПоступление.Состояние0 = 0 " +
                    "ORDER BY " +
                        "Journ.docno, Supply.LineNO_ ";
                TextQuery = TextQuery.Replace(":Docs", Docs);
                QuerySetParam(ref TextQuery, "OKEIPackage", OKEIPackage);
                QuerySetParam(ref TextQuery, "Warehouse", Const.MainWarehouse);
                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                DT.Columns.Add("ArticleFind", Type.GetType("System.String"));
                DT.Columns.Add("ArticleOnPackFind", Type.GetType("System.String"));
                DT.Columns.Add("ItemNameFind", Type.GetType("System.String"));
                DT.Columns.Add("CoefView", Type.GetType("System.String"));
                for (int row = 0; row < DT.Rows.Count; row++)
                {
                    DT.Rows[row]["ArticleFind"] = Helper.SuckDigits(DT.Rows[row]["Article"].ToString().Trim());
                    DT.Rows[row]["ArticleOnPackFind"] = Helper.SuckDigits(DT.Rows[row]["ArticleOnPack"].ToString().Trim());
                    DT.Rows[row]["ItemNameFind"] = Helper.SuckDigits(DT.Rows[row]["ItemName"].ToString().Trim());
                    DT.Rows[row]["CoefView"] = ((int)(decimal)DT.Rows[row]["Coef"] == 1 ? "??  " : "")
                                                            + ((int)(decimal)DT.Rows[row]["Coef"]).ToString();
                }
                FNotAcceptedItems.Merge(DT, false, MissingSchemaAction.Ignore);

                //Теперь принятый товар
                TextQuery =
                    "SELECT " +
                        "right(Journ.docno,5) as DOCNO," +
                        "Supply.iddoc as iddoc," +
                        "Goods.id as id," +
                        "Goods.Descr as ItemName," +
                        "Goods.$Спр.Товары.ИнвКод as InvCode," +
                        "Goods.$Спр.Товары.Артикул as Article," +
                        "Goods.$Спр.Товары.АртикулНаУпаковке as ArticleOnPack," +
                        "Goods.$Спр.Товары.Прих_Цена as Price," +
                        "Goods.$Спр.Товары.КоличествоДеталей as Details, " +
                        "CASE WHEN round(Supply.$АдресПоступление.Количество /ISNULL(Package.Coef, 1), 0)*ISNULL(Package.Coef, 1) = Supply.$АдресПоступление.Количество " +
                            "THEN ISNULL(Package.Coef, 1) ELSE 1 END as Coef, " +
                        "CASE WHEN round(Supply.$АдресПоступление.Количество /ISNULL(Package.Coef, 1), 0)*ISNULL(Package.Coef, 1) = Supply.$АдресПоступление.Количество " +
                            "THEN round(Supply.$АдресПоступление.Количество /ISNULL(Package.Coef, 1), 0) " +
                            "ELSE Supply.$АдресПоступление.Количество END as CountPackage, " +
                        "Supply.$АдресПоступление.Количество as Count," +
                        "Supply.$АдресПоступление.ЕдиницаШК as Unit," +
                        "Supply.$АдресПоступление.КоличествоЭтикеток as LabelCount," +
                        "Supply.$АдресПоступление.НомерСтрокиДока as Number," +
                        "Supply.$АдресПоступление.ГруппаСезона as SeasonGroup," +
                        "SypplyHeader.$АдресПоступление.ДальнийСклад as FlagFarWarehouse," +
                        "Supply.LineNO_ as LineNO_, " +
                    //"isnull(GS.$Спр.ТоварныеСекции.РазмерХранения , 0) StoregeSize " + 
                        "isnull(GS.$Спр.ТоварныеСекции.РасчетныйРХ , 0) StoregeSize " +
                     "FROM DT$АдресПоступление as Supply (nolock) " +
                        "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                            "ON Goods.ID = Supply.$АдресПоступление.Товар " +
                        "LEFT JOIN DH$АдресПоступление as SypplyHeader (nolock) " +
                            "ON SypplyHeader.iddoc = Supply.iddoc " +
                        "LEFT JOIN _1sjourn as Journ (nolock) " +
                            "ON Journ.iddoc = Right(SypplyHeader.$АдресПоступление.ДокументОснование , 9) " +
                        "LEFT JOIN ( " +
                                "SELECT " +
                                    "Units.parentext as ItemID, " +
                                    "min(Units.$Спр.ЕдиницыШК.Коэффициент ) as Coef " +
                                "FROM " +
                                    "$Спр.ЕдиницыШК as Units (nolock) " +
                                "WHERE " +
                                    "Units.$Спр.ЕдиницыШК.ОКЕИ = :OKEIPackage " +
                                    "and Units.ismark = 0 " +
                                    "and not Units.$Спр.ЕдиницыШК.Коэффициент = 0 " +
                                "GROUP BY " +
                                    "Units.parentext " +
                                ") as Package " +
                            "ON Package.ItemID = Goods.ID " +
                        "LEFT JOIN $Спр.ТоварныеСекции as GS (nolock) " +
                            "on GS.parentext = goods.id and gs.$Спр.ТоварныеСекции.Склад = :Warehouse " +
                    "WHERE Supply.IDDOC in (:Docs) " +
                        "and Supply.$АдресПоступление.Состояние0 = 1 " +
                        "and Supply.$АдресПоступление.ФлагПечати = 1 " +
                        "and Supply.$АдресПоступление.Сотрудник0 = :Employer " +
                    "ORDER BY Journ.docno, Supply.$АдресПоступление.Дата0 , Supply.$АдресПоступление.Время0 ";
                TextQuery = TextQuery.Replace(":Docs", Docs);
                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "OKEIPackage", OKEIPackage);
                QuerySetParam(ref TextQuery, "Warehouse", Const.MainWarehouse);
                DT.Clear();
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                DT.Columns.Add("CoefView", Type.GetType("System.String"));
                for (int row = 0; row < DT.Rows.Count; row++)
                {
                    DT.Rows[row]["CoefView"] = ((int)(decimal)DT.Rows[row]["Coef"] == 1 ? "??  " : "")
                                                            + ((int)(decimal)DT.Rows[row]["Coef"]).ToString();
                }
                FAcceptedItems.Merge(DT, false, MissingSchemaAction.Ignore);
                if (FPallets.Rows.Count == 0)
                {
                    //Теперь принятые паллеты
                    TextQuery =
                        "SELECT " +
                            "Supply.$АдресПоступление.Паллета as ID, " +
                            "min(Pallets.$Спр.ПеремещенияПаллет.ШКПаллеты ) as Barcode, " +
                            "min(SUBSTRING(Pallets.$Спр.ПеремещенияПаллет.ШКПаллеты ,8,4)) as Name, " +
                            "min(Pallets.$Спр.ПеремещенияПаллет.Адрес0 ) as AdressID " +
                         "FROM DT$АдресПоступление as Supply (nolock) " +
                            "INNER JOIN $Спр.ПеремещенияПаллет as Pallets (nolock) " +
                                "ON Pallets.ID = Supply.$АдресПоступление.Паллета " +
                        "WHERE Supply.IDDOC in (:Docs) " +
                            "and Supply.$АдресПоступление.Состояние0 = 1 " +
                            "and Supply.$АдресПоступление.ФлагПечати = 1 " +
                            "and Supply.$АдресПоступление.Сотрудник0 = :Employer " +
                        "GROUP BY " +
                            "Supply.$АдресПоступление.Паллета " +
                        "ORDER BY " +
                            "Supply.$АдресПоступление.Паллета ";
                    TextQuery = TextQuery.Replace(":Docs", Docs);
                    QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                    DT.Clear();
                    if (!ExecuteWithRead(TextQuery, out DT))
                    {
                        return false;
                    }
                    FPallets.Merge(DT, false, MissingSchemaAction.Ignore);
                    if (DT.Rows.Count > 0)
                    {
                        FPalletID = FPallets.Rows[DT.Rows.Count - 1]["ID"].ToString();
                        FBarcodePallet = FPallets.Rows[DT.Rows.Count - 1]["Barcode"].ToString();
                    }
                    else
                    {
                        FPalletID = "";
                        FBarcodePallet = "";
                    }
                }
                //Расчитываем строчечки
                DR = FConsignment.Select();
                foreach (DataRow dr in DR)
                {
                    DataRow[] tmpDR = FNotAcceptedItems.Select("IDDOC = '" + dr["ACID"].ToString() + "'");
                    dr["CountNotAcceptRow"] = tmpDR.Length;
                }
            }

            FCurrentMode = Mode.Acceptance;
            return true;
        } // ToModeAcceptance
        private bool ToModeAcceptedItem(string ItemID, string IDDoc, Mode ToMode)
        {
            return ToModeAcceptedItem(ItemID, IDDoc, ToMode, 0, false);
        }
        private bool ToModeAcceptedItem(string ItemID, string IDDoc, Mode ToMode, int InPartyCount, bool OnShelf)
        {
            //проверяем наличие отсканированной паллеты
            if ((FPalletID == "") && (ToMode == Mode.Acceptance))
            {
                FExcStr = "Не выбрана паллета";
                return false;
            }
            AdressConditionItem = null;
            //Если был дисконнект, то это проявиться после нижеследующего запроса
            //и дальше будет, не приемка, а редактирование карточки, для этого запрос и помещен в начало
            if (!LoadUnits(ItemID))
            {
                return false;
            }

            //FExcStr - несет смысл
            AcceptedItem = new StructItem();
            AcceptedItem.GenerateBarcode = false;
            if (NewBarcodes == null)
            {
                NewBarcodes = new List<string>();
            }
            else
            {
                NewBarcodes.Clear();
            }

            //Определяем имеется ли данный товар в списке принимаемых
            CurrentRowAcceptedItem = null;
            int AllCount = 0;
            DataRow[] DR;

            if (ToMode == Mode.Acceptance)
            {
                DR = FNotAcceptedItems.Select("ID = '" + ItemID + "'");
                if (DR.Length > 1 && IDDoc != "")
                {
                    foreach (DataRow dr in DR)
                    {
                        if (dr["IDDOC"].ToString() == IDDoc)
                        {
                            if (CurrentRowAcceptedItem == null)
                            {
                                CurrentRowAcceptedItem = dr;
                            }
                        }
                        AllCount += (int)dr["Count"];
                    }
                }
                else if (DR.Length > 0) //Один товар или не указана строка документа
                {
                    CurrentRowAcceptedItem = DR[0];
                    foreach (DataRow dr in DR)
                    {
                        AllCount += (int)dr["Count"];
                    }
                }
                //иначе это скан товара не из списка!
            }
            else if ( ToMode == Mode.AcceptanceCross)
            {
                DR = FNotAcceptedItems.Select("ID = '" + ItemID + "'");
                if (DR.Length > 1 && IDDoc != "")
                {
                    foreach (DataRow dr in DR)
                    {
                        if (dr["IDDOC"].ToString() == IDDoc && dr["OrderID"].ToString() == FOrderID)
                        {
                            if (CurrentRowAcceptedItem == null)
                            {
                                CurrentRowAcceptedItem = dr;
                            }
                        }
                        AllCount += (int)dr["Count"];
                    }
                }
                else if (DR.Length > 0) //Один товар или не указана строка документа
                {
                    CurrentRowAcceptedItem = DR[0];
                    foreach (DataRow dr in DR)
                    {
                        AllCount += (int)dr["Count"];
                    }
                }
                //иначе это скан товара не из списка!
            }
            //БЛОКИРУЕМ ТОВАР
            if (!LockItem(ItemID))
            {
                return false;
            }
            RefWarehouse WorkWarehouse = new RefWarehouse(this);

            if (WarehouseForAddressItem == null)
            {
                WorkWarehouse.FoundID(Const.MainWarehouse);
            }
            else if (WarehouseForAddressItem.Selected)
            {
                WorkWarehouse.FoundID(WarehouseForAddressItem.ID);
            }
            else
            {
                WorkWarehouse.FoundID(Const.MainWarehouse);
            }
            //ОПРЕДЕЛЯЕМ ОСТАТКИ И АДРЕСА
            string TextQuery =
                "DECLARE @curdate DateTime; " +
                "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " +
                "SELECT " +
                    "CAST(sum(CASE WHEN Main.Warehouse = :MainWarehouse THEN Main.Balance ELSE 0 END) as int) as BalanceMain, " +
                    "CAST(sum(CASE WHEN Main.Warehouse = :BufferWarehouse THEN Main.Balance ELSE 0 END) as int) as BalanceBuffer, " +
                    "ISNULL((" +
                        "SELECT top 1 " +
                            "Section.descr " +
                        "FROM _1sconst as Const (nolock) " +
                            "LEFT JOIN $Спр.Секции as Section (nolock) " +
                                "ON Section.id = left(Const.value, 9) " +
                        "WHERE " +
                            "Const.id = $Спр.ТоварныеСекции.Секция " +
                            "and Const.date <= :DateNow " +
                            "and Const.OBJID in (" +
                                    "SELECT id FROM $Спр.ТоварныеСекции " +
                                    "WHERE " +
                                        "$Спр.ТоварныеСекции.Склад = :MainWarehouse " +
                                        "and parentext = :Item)" +
                        "ORDER BY " +
                            "Const.date DESC, Const.time DESC, Const.docid DESC), '<не задан>') as AdressMain, " +
                    "ISNULL((" +
                        "SELECT top 1 " +
                            "Section.descr " +
                        "FROM _1sconst as Const (nolock) " +
                            "LEFT JOIN $Спр.Секции as Section (nolock) " +
                                "ON Section.id = left(Const.value, 9) " +
                        "WHERE " +
                            "Const.id = $Спр.ТоварныеСекции.Секция " +
                            "and Const.date <= :DateNow " +
                            "and Const.OBJID in (" +
                                    "SELECT id FROM $Спр.ТоварныеСекции " +
                                    "WHERE " +
                                        "$Спр.ТоварныеСекции.Склад = :BufferWarehouse " +
                                        "and parentext = :Item)" +
                        "ORDER BY " +
                            "Const.date DESC, Const.time DESC, Const.docid DESC), '<не задан>') as AdressBuffer " +
                "FROM (" +
                        "SELECT " +
                            "$Рег.ОстаткиТоваров.Склад as Warehouse, " +
                            "$Рег.ОстаткиТоваров.Товар as Item, " +
                            "$Рег.ОстаткиТоваров.ОстатокТовара as Balance " +
                        "FROM " +
                            "RG$Рег.ОстаткиТоваров (nolock) " +
                        "WHERE " +
                            "period = @curdate " +
                            "and $Рег.ОстаткиТоваров.Товар = :Item " +
                            "and $Рег.ОстаткиТоваров.Склад in (:MainWarehouse, :BufferWarehouse) " +
                        "UNION ALL " +
                        "SELECT " +
                            ":MainWarehouse, :Item, 0 " +
                        ") as Main " +
                "GROUP BY Main.Item";
            QuerySetParam(ref TextQuery, "DateNow", DateTime.Now);
            QuerySetParam(ref TextQuery, "Item", ItemID);
            QuerySetParam(ref TextQuery, "BufferWarehouse", BufferWarehouse);
            QuerySetParam(ref TextQuery, "MainWarehouse", WorkWarehouse.ID);
            
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            AcceptedItem.BalanceMain = (int)DT.Rows[0]["BalanceMain"];
            AcceptedItem.BalanceBuffer = (int)DT.Rows[0]["BalanceBuffer"];
            AcceptedItem.AdressMain = DT.Rows[0]["AdressMain"].ToString();
            AcceptedItem.AdressBuffer = DT.Rows[0]["AdressBuffer"].ToString();
            AcceptedItem.IsRepeat = false;

            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            if (CurrentRowAcceptedItem == null)
            {
                //Подсосем остатки в разрезе адресов и состояний
                TextQuery =
                    "DECLARE @curdate DateTime; " +
                    "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " +
                    "SELECT " +
                            "min(Section.descr) as Adress, " +
                            "CASE " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = -10 THEN '-10 Автокорректировка' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = -2 THEN '-2 В излишке' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = -1 THEN '-1 В излишке (пересчет)' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 0 THEN '00 Не существует' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 1 THEN '01 Приемка' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 2 THEN '02 Хороший на месте' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 3 THEN '03 Хороший (пересчет)' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 4 THEN '04 Хороший (движение)' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 7 THEN '07 Бракованный на месте' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 8 THEN '08 Бракованный (пересчет)' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 9 THEN '09 Бракованный (движение)' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 12 THEN '12 Недостача' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 13 THEN '13 Недостача (пересчет)' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 14 THEN '14 Недостача (движение)' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 17 THEN '17 Недостача подтвержденная' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 18 THEN '18 Недостача подт.(пересчет)' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 19 THEN '19 Недостача подт.(движение)' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 22 THEN '22 Пересорт излишек' " +
                                "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 23 THEN '23 Пересорт недостача' " +
                                "ELSE rtrim(cast(RegAOT.$Рег.АдресОстаткиТоваров.Состояние as char)) + ' <неизвестное состояние>' END as Condition, " +
                            "cast(sum(RegAOT.$Рег.АдресОстаткиТоваров.Количество ) as int) as Count " +
                        "FROM " +
                            "RG$Рег.АдресОстаткиТоваров as RegAOT (nolock) " +
                            "LEFT JOIN $Спр.Секции as Section (nolock) " +
                                "ON Section.id = RegAOT.$Рег.АдресОстаткиТоваров.Адрес " +
                        "WHERE " +
                            "RegAOT.period = @curdate " +
                            "and RegAOT.$Рег.АдресОстаткиТоваров.Товар = :ItemID " +
                            "and RegAOT.$Рег.АдресОстаткиТоваров.Склад = :Warehouse " +
                        "GROUP BY " +
                            "RegAOT.$Рег.АдресОстаткиТоваров.Адрес , " +
                            "RegAOT.$Рег.АдресОстаткиТоваров.Товар , " +
                            "RegAOT.$Рег.АдресОстаткиТоваров.Состояние " +
                        "HAVING sum(RegAOT.$Рег.АдресОстаткиТоваров.Количество ) <> 0 " +
                        "ORDER BY Adress, Condition";
                QuerySetParam(ref TextQuery, "DateNow", DateTime.Now);
                QuerySetParam(ref TextQuery, "ItemID", ItemID);
                QuerySetParam(ref TextQuery, "Warehouse", WorkWarehouse.ID);
                if (!ExecuteWithRead(TextQuery, out AdressConditionItem))
                {
                    AdressConditionItem = null;
                }

                //Я не знаю что это...
                TextQuery =
                    "SELECT " +
                        "Goods.Descr as ItemName," +
                        "Goods.$Спр.Товары.ИнвКод as InvCode, " +
                        "Goods.$Спр.Товары.Артикул as Article, " +
                        "Goods.$Спр.Товары.КоличествоДеталей as Details, " +
                        "Goods.$Спр.Товары.БазоваяЕдиницаШК as BaseUnitID, " +
                        "Goods.$Спр.Товары.МинПартия as MinParty, " +
                        "Goods.$Спр.Товары." + (ToMode == Mode.Acceptance ? "Прих_Цена" : "Опт_Цена") + " as Price,  " +
                    //"isnull(RefSections.$Спр.ТоварныеСекции.РазмерХранения , 0) as StoregeSize " +
                        "isnull(RefSections.$Спр.ТоварныеСекции.РасчетныйРХ , 0) as StoregeSize " +
                    "FROM $Спр.Товары as Goods (nolock) " +
                        "left join $Спр.ТоварныеСекции as RefSections (nolock) " +
                            "on RefSections.parentext = Goods.id and RefSections.$Спр.ТоварныеСекции.Склад = :warehouse " +
                    "WHERE Goods.id = :Item ";
                QuerySetParam(ref TextQuery, "Item", ItemID);
                QuerySetParam(ref TextQuery, "warehouse", WorkWarehouse.ID);
                DT.Clear();
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                AcceptedItem.Details = (int)(decimal)DT.Rows[0]["Details"];
                AcceptedItem.NowDetails = AcceptedItem.Details;

                AcceptedItem.ID = ItemID;
                AcceptedItem.Name = DT.Rows[0]["ItemName"].ToString();
                AcceptedItem.InvCode = DT.Rows[0]["InvCode"].ToString();
                AcceptedItem.Acticle = DT.Rows[0]["Article"].ToString();
                AcceptedItem.BaseUnitID = DT.Rows[0]["BaseUnitID"].ToString();
                AcceptedItem.MinParty = (int)(decimal)DT.Rows[0]["MinParty"];
                AcceptedItem.Count = 0;
                AcceptedItem.Price = (decimal)DT.Rows[0]["Price"];
                AcceptedItem.Acceptance = false;
                AcceptedItem.ToMode = ToMode;
                AcceptedItem.BindingAdressFlag = false;
                AcceptedItem.StoregeSize = (int)(decimal)DT.Rows[0]["StoregeSize"];
                if (AcceptedItem.ToMode == Mode.AcceptanceCross)
                {
                    CurrentPalletAcceptedItem = new RefPalleteMove(this);
                                     
                }

                //Если это необходимо, то определяем количество товара для склада инвентаризации
                if (AcceptedItem.ToMode != Mode.Inventory || InventoryWarehouse.ID == Const.MainWarehouse)
                {
                    AcceptedItem.CurrentBalance = AcceptedItem.BalanceMain;
                }
                else if (InventoryWarehouse.ID == BufferWarehouse)
                {
                    AcceptedItem.CurrentBalance = AcceptedItem.BalanceBuffer;
                }
                else
                {
                    //Остатков этого склада нет!
                    TextQuery =
                        "DECLARE @curdate DateTime; " +
                        "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " +
                        "SELECT sum(Main.Balance) as Balance " +
                            "FROM " +
                                "(SELECT " +
                                    "$Рег.ОстаткиТоваров.Товар as Item, " +
                                    "$Рег.ОстаткиТоваров.ОстатокТовара as Balance " +
                                "FROM " +
                                    "RG$Рег.ОстаткиТоваров (nolock) " +
                                "WHERE " +
                                    "period = @curdate " +
                                    "and $Рег.ОстаткиТоваров.Товар = :ItemID " +
                                    "and $Рег.ОстаткиТоваров.Склад = :Warehouse " +
                                "UNION ALL " +
                                "SELECT :ItemID, 0 " +
                                ") as Main " +
                            "GROUP BY Main.Item;";
                    QuerySetParam(ref TextQuery, "DateNow", DateTime.Now);
                    QuerySetParam(ref TextQuery, "ItemID", ItemID);
                    QuerySetParam(ref TextQuery, "Warehouse", InventoryWarehouse.ID);
                    if (!ExecuteWithRead(TextQuery, out DT))
                    {
                        return false;
                    }
                    AcceptedItem.CurrentBalance = (int)(decimal)DT.Rows[0]["Balance"];
                }
                //А теперь имя склада!
                if (AcceptedItem.ToMode != Mode.Inventory)
                {
                    TextQuery =
                        "SELECT descr as Name FROM $Спр.Склады (nolock) WHERE ID = :Warehouse";
                    QuerySetParam(ref TextQuery, "Warehouse", WorkWarehouse.ID);
                    if (!ExecuteWithRead(TextQuery, out DT))
                    {
                        return false;
                    }
                    AcceptedItem.CurrentWarehouseName = DT.Rows[0]["Name"].ToString();
                }
                else
                {
                    AcceptedItem.CurrentWarehouseName = InventoryWarehouse.Name;
                }

                //
                if (ToMode == Mode.Transfer)
                {
                    if (ATDoc.ToWarehouseSingleAdressMode)
                    {
                        DR = FTransferReadyItems.Select("ID = '" + AcceptedItem.ID + "'");
                        if (DR.Length > 0)
                        {
                            AcceptedItem.BindingAdress = DR[0]["Adress1"].ToString();
                            AcceptedItem.BindingAdressName = DR[0]["AdressName"].ToString();
                            AcceptedItem.BindingAdressFlag = true;
                        }
                        else if (!Employer.CanMultiadress && AcceptedItem.CurrentBalance > 0)
                        {
                            //ОПРЕДЕЛИМ РЕКОМЕНДУЕМЫЙ АДРЕС
                            TextQuery =
                                "SELECT top 1 " +
                                " left(const.value, 9) as Adress, " +
                                " section.descr as AdressName " +
                                "FROM _1sconst as const(nolock) " +
                                "LEFT JOIN $Спр.Секции as Section (nolock) " +
                                        "ON Section.id = left(value, 9) " +
                                "WHERE " +
                                    "const.id = $Спр.ТоварныеСекции.Секция " +
                                    "and const.date <= :DateNow " +
                                    "and const.OBJID in (" +
                                                   "SELECT id FROM $Спр.ТоварныеСекции (nolock) " +
                                                    "WHERE " +
                                                        "$Спр.ТоварныеСекции.Склад = :Warehouse " +
                                                        "and parentext = :Item) " +
                                "ORDER BY " +
                                    "const.date DESC, const.time DESC, const.docid DESC ";
                            QuerySetParam(ref TextQuery, "DateNow", DateTime.Now);
                            QuerySetParam(ref TextQuery, "Item", ItemID);
                            QuerySetParam(ref TextQuery, "Warehouse", ATDoc.ToWarehouseID);
                            if (!ExecuteWithRead(TextQuery, out DT))
                            {
                                return false;
                            }
                            if (DT.Rows.Count == 1)
                            {
                                AcceptedItem.BindingAdress = DT.Rows[0]["Adress"].ToString();
                                AcceptedItem.BindingAdressName = DT.Rows[0]["AdressName"].ToString();
                                AcceptedItem.BindingAdressFlag = true;
                            }
                        }
                    }
                }


                if (AcceptedItem.ToMode == Mode.Acceptance || AcceptedItem.ToMode == Mode.AcceptanceCross)
                {
                    FExcStr = "РЕДАКТИРОВАНИЕ КАРТОЧКИ! ТОВАРА НЕТ В СПИСКЕ ПРИНИМАЕМЫХ!";
                }
                else if (AcceptedItem.ToMode == Mode.Inventory)
                {
                    FExcStr = "РЕДАКТИРОВАНИЕ КАРТОЧКИ!";
                }
                else if (AcceptedItem.ToMode == Mode.SampleInventory)
                {
                    FExcStr = "ОБРАЗЕЦ! " + FExcStr;
                }
                else if (AcceptedItem.ToMode == Mode.SamplePut)
                {
                    FExcStr = "ОБРАЗЕЦ (выкладка)! " + FExcStr;
                }
                else if (AcceptedItem.ToMode == Mode.Transfer || AcceptedItem.ToMode == Mode.NewInventory || AcceptedItem.ToMode == Mode.Harmonization || AcceptedItem.ToMode == Mode.HarmonizationPut)
                {
                    if (OnShelf)
                    {
                        RefItem Item = new RefItem(this);
                        Item.FoundID(AcceptedItem.ID);
                        RefSection BindingAdress = new RefSection(this);
                        BindingAdress.FoundID(AcceptedItem.BindingAdress);

                        if (AcceptedItem.BindingAdressFlag)
                        {
                            FExcStr = "НА ПОЛКУ! Отсканируйте адрес!"; // по умолчинию так ставим, а ниже условия которые могут этот текст поменять

                            if (!Item.ZonaHand.Selected && !BindingAdress.AdressZone.Selected)
                            {
                                FExcStr = "НА ПОЛКУ! Отсканируйте адрес: " + AcceptedItem.BindingAdressName;
                            }
                            else if (Item.ZonaHand.Selected && BindingAdress.AdressZone.Selected)
                            {
                                if (Item.ZonaHand.ID == BindingAdress.AdressZone.ID)
                                {
                                    FExcStr = "НА ПОЛКУ! Отсканируйте адрес: " + AcceptedItem.BindingAdressName;
                                }
                            }
                        }
                        else if (AcceptedItem.ToMode == Mode.Harmonization)
                        {
                            //ну не пиздец ли это???
                            FExcStr = "В ТЕЛЕЖКУ! Отсканируйте адрес!";
                        }
                        else
                        {
                            FExcStr = "НА ПОЛКУ! Отсканируйте адрес!";
                        }
                    }
                    else
                    {
                        DR = FTransferReadyItems.Select("ID = '" + AcceptedItem.ID + "'");
                        if (DR.Length == 0)
                        {
                            FExcStr = "В ТЕЛЕЖКУ!";
                        }
                        else
                        {
                            AcceptedItem.IsRepeat = true;
                            FExcStr = "ВНИМАНИЕ! УЖЕ СЧИТАЛСЯ! (В ТЕЛЕЖКУ)";
                        }
                    }
                    AcceptedItem.Count = InPartyCount;
                    AcceptedItem.OnShelf = OnShelf;
                }

                FCurrentMode = Mode.AcceptedItem;
                //begin internal command
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Employer.ID, "Спр.Сотрудники");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"] = ExtendID(ItemID, "Спр.Товары");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = "OpenItem (Открыл карточку)";
                if (!ExecCommandNoFeedback("Internal", DataMapWrite))
                {
                    return false;
                }
                //end internal command
                return true;
            }

            AcceptedItem.ID = CurrentRowAcceptedItem["ID"].ToString();
            AcceptedItem.Name = CurrentRowAcceptedItem["ItemName"].ToString();
            AcceptedItem.InvCode = CurrentRowAcceptedItem["InvCode"].ToString();
            AcceptedItem.Acticle = CurrentRowAcceptedItem["Article"].ToString();
            AcceptedItem.Count = (int)CurrentRowAcceptedItem["Count"];
            AcceptedItem.Price = (decimal)CurrentRowAcceptedItem["Price"];
            AcceptedItem.Acceptance = true;
            AcceptedItem.Details = (int)CurrentRowAcceptedItem["Details"];
            AcceptedItem.NowDetails = AcceptedItem.Details;
            AcceptedItem.ToMode = ToMode == Mode.AcceptanceCross ? Mode.AcceptanceCross : Mode.Acceptance;
            AcceptedItem.BindingAdressFlag = false;
            AcceptedItem.SeasonGroup = (int)CurrentRowAcceptedItem["SeasonGroup"];
            AcceptedItem.FlagFarWarehouse = (int)CurrentRowAcceptedItem["FlagFarWarehouse"];
            AcceptedItem.StoregeSize = (int)CurrentRowAcceptedItem["StoregeSize"];
            if (AcceptedItem.ToMode == Mode.AcceptanceCross)
            {
                CurrentPalletAcceptedItem = new RefPalleteMove(this);
                if (CurrentRowAcceptedItem["PalletID"].ToString() != "")
                {
                    CurrentPalletAcceptedItem.FoundID(CurrentRowAcceptedItem["PalletID"].ToString());
                }
            }

            if (FlagBarcode == 0)
            {
                FExcStr = AcceptedItem.InvCode.Trim() + " найден в ручную!";
            }
            else if (FlagBarcode == 1)
            {
                FExcStr = AcceptedItem.InvCode.Trim() + " найден по штрихкоду!";
            }
            else //FlagBarcode == 2
            {
                FExcStr = AcceptedItem.InvCode.Trim() + " найден по ШК МЕСТА!";
            }
            DataRow[] DRAI = FAcceptedItems.Select("ID = '" + ItemID + "' and IDDOC = '" + IDDoc + "'");
            if (DRAI.Length > 0)
            {
                FExcStr = "ПОВТОРНАЯ приемка!!! " + FExcStr;
            }
            //Добавляем что принимается не все
            if (AllCount > AcceptedItem.Count)
            {
                int Coef = 1;
                DR = FUnits.Select("OKEI = '" + OKEIPackage + "'");
                foreach (DataRow dr in DR)
                {
                    Coef = (int)dr["Coef"];
                    break;
                }
                FExcStr += " " + GetStrPackageCount(AcceptedItem.Count, Coef) + " из " + GetStrPackageCount(AllCount, Coef);
            }
            //begin internal command
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"] = ExtendID(ItemID, "Спр.Товары");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"] = ExtendID(IDDoc, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = "OpenItemAccept (Открыл карточку для приемки)";
            if (!ExecCommandNoFeedback("Internal", DataMapWrite))
            {
                return false;
            }
            //end internal command
            FCurrentMode = Mode.AcceptedItem;
            return true;
        } // ToModeAcceptedItem
        private bool ToModeAcceptedItem(string ItemID, string IDDoc)
        {
            return ToModeAcceptedItem(ItemID, IDDoc, FCurrentMode == Mode.AcceptanceCross ? Mode.AcceptanceCross : Mode.Acceptance);
        }
        private bool ToModeTransfer(StrictDoc Doc, Mode ToMode)
        {
            string TextQuery =
                "DECLARE @curdate DateTime; " +
                "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " +
                "SELECT " +
                    "DocAT.$АдресПеремещение.Товар as ID, " +
                    "min(Goods.descr) as ItemName, " +
                    "min(Goods.$Спр.Товары.ИнвКод ) as InvCode, " +
                    "min(Goods.$Спр.Товары.Артикул ) as Article, " +
                    "sum(DocAT.$АдресПеремещение.Количество ) as Count, " +
                    "DocAT.$АдресПеремещение.Адрес0 as Adress0, " +
                    "CASE WHEN round(sum(DocAT.$АдресПеремещение.Количество )/ISNULL(min(Package.Coef), 1), 0)*ISNULL(min(Package.Coef), 1) = sum(DocAT.$АдресПеремещение.Количество )" +
                        "THEN ISNULL(min(Package.Coef), 1) ELSE 1 END as Coef, " +
                    "CASE WHEN round(sum(DocAT.$АдресПеремещение.Количество )/ISNULL(min(Package.Coef), 1), 0)*ISNULL(min(Package.Coef), 1) = sum(DocAT.$АдресПеремещение.Количество )" +
                        "THEN cast(round(sum(DocAT.$АдресПеремещение.Количество )/ISNULL(min(Package.Coef), 1), 0) as int) " +
                        "ELSE sum(DocAT.$АдресПеремещение.Количество ) END as CountPackage, " +
                        (ToMode == Mode.Transfer ? "CASE WHEN min(DocAT.$АдресПеремещение.ФлагОбязательногоАдреса ) = 1 THEN min(Sections.descr) ELSE NULL END as AdressName " : "min(Sections.descr) as AdressName ") +
                "FROM " +
                    "DT$АдресПеремещение as DocAT (nolock) " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.ID = DocAT.$АдресПеремещение.Товар " +
                    "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                        "ON Sections.ID = DocAT.$АдресПеремещение.Адрес1 " +
                    "LEFT JOIN ( " +
                                "SELECT " +
                                    "Units.parentext as ItemID, " +
                                    "min(Units.$Спр.ЕдиницыШК.Коэффициент ) as Coef " +
                                "FROM " +
                                    "$Спр.ЕдиницыШК as Units (nolock) " +
                                "WHERE " +
                                    "Units.$Спр.ЕдиницыШК.ОКЕИ = :OKEIPackage " +
                                    "and Units.ismark = 0 " +
                                "GROUP BY " +
                                    "Units.parentext ) as Package " +
                        "ON Package.ItemID = Goods.ID " +
                "WHERE " +
                    "DocAT.iddoc = :Doc " +
                    "and not DocAT.$АдресПеремещение.Дата0 = :EmptyDate " +
                    "and DocAT.$АдресПеремещение.Дата1 = :EmptyDate " +
                "GROUP BY DocAT.$АдресПеремещение.Товар , DocAT.$АдресПеремещение.Адрес0 " +
                "ORDER BY min(DocAT.$АдресПеремещение.Дата0 ), min(DocAT.$АдресПеремещение.Время0 )";
            QuerySetParam(ref TextQuery, "Doc", Doc.ID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "OKEIPackage", OKEIPackage);
            QuerySetParam(ref TextQuery, "Warehouse", Doc.ToWarehouseID);
            if (!ExecuteWithRead(TextQuery, out FTransferItems))
            {
                return false;
            }
            FTransferItems.Columns.Add("CoefView", Type.GetType("System.String"));
            for (int row = 0; row < FTransferItems.Rows.Count; row++)
            {
                FTransferItems.Rows[row]["CoefView"] = ((int)(decimal)FTransferItems.Rows[row]["Coef"] == 1 ? "??  " : "")
                                                            + ((int)(decimal)FTransferItems.Rows[row]["Coef"]).ToString();
            }
            TextQuery =
                "SELECT " +
                    "DocAT.$АдресПеремещение.Товар as ID, " +
                    "min(Goods.descr) as ItemName, " +
                    "min(Goods.$Спр.Товары.ИнвКод ) as InvCode, " +
                    "min(Goods.$Спр.Товары.Артикул ) as Article, " +
                    "sum(DocAT.$АдресПеремещение.Количество ) as Count, " +
                    "DocAT.$АдресПеремещение.Адрес1 as Adress1, " +
                    "CASE WHEN round(sum(DocAT.$АдресПеремещение.Количество )/ISNULL(min(Package.Coef), 1), 0)*ISNULL(min(Package.Coef), 1) = sum(DocAT.$АдресПеремещение.Количество )" +
                        "THEN ISNULL(min(Package.Coef), 1) ELSE 1 END as Coef, " +
                    "CASE WHEN round(sum(DocAT.$АдресПеремещение.Количество )/ISNULL(min(Package.Coef), 1), 0)*ISNULL(min(Package.Coef), 1) = sum(DocAT.$АдресПеремещение.Количество )" +
                        "THEN cast(round(sum(DocAT.$АдресПеремещение.Количество )/ISNULL(min(Package.Coef), 1), 0) as int) " +
                        "ELSE sum(DocAT.$АдресПеремещение.Количество ) END as CountPackage, " +
                    "min(Sections.descr) as AdressName " +
                "FROM " +
                    "DT$АдресПеремещение as DocAT (nolock) " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.ID = DocAT.$АдресПеремещение.Товар " +
                    "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                        "ON Sections.ID = DocAT.$АдресПеремещение.Адрес1 " +
                    "LEFT JOIN ( " +
                            "SELECT " +
                                "Units.parentext as ItemID, " +
                                "min(Units.$Спр.ЕдиницыШК.Коэффициент ) as Coef " +
                            "FROM " +
                                "$Спр.ЕдиницыШК as Units (nolock) " +
                            "WHERE " +
                                "Units.$Спр.ЕдиницыШК.ОКЕИ = :OKEIPackage " +
                                "and Units.ismark = 0 " +
                            "GROUP BY " +
                                "Units.parentext ) as Package " +
                        "ON Package.ItemID = Goods.ID " +
                "WHERE " +
                    "DocAT.iddoc = :Doc " +
                    "and not DocAT.$АдресПеремещение.Дата0 = :EmptyDate " +
                    "and not DocAT.$АдресПеремещение.Дата1 = :EmptyDate " +
                "GROUP BY DocAT.$АдресПеремещение.Товар , DocAT.$АдресПеремещение.Адрес1 " +
                "ORDER BY min(DocAT.$АдресПеремещение.Дата0 ), min(DocAT.$АдресПеремещение.Время0 )";
            QuerySetParam(ref TextQuery, "Doc", Doc.ID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "OKEIPackage", OKEIPackage);
            if (!ExecuteWithRead(TextQuery, out FTransferReadyItems))
            {
                return false;
            }
            FTransferReadyItems.Columns.Add("CoefView", Type.GetType("System.String"));
            for (int row = 0; row < FTransferReadyItems.Rows.Count; row++)
            {
                FTransferReadyItems.Rows[row]["CoefView"] = ((int)(decimal)FTransferReadyItems.Rows[row]["Coef"] == 1 ? "??  " : "")
                                                                + ((int)(decimal)FTransferReadyItems.Rows[row]["Coef"]).ToString();
            }

            ATDoc = Doc;
            CurrentLine = -1;
            OkRowIndex = -1;
            if (FPrinter.PrinterType == 1)
            {
                FPrinter = new RefPrinter(this); //обнуляем принтер, т.к. принтер этикеток нам не подходит
            }

            //Подтянем нужную зону для разноса
            if (FAdressZoneTransfer == null)
            {
                TextQuery =
                    "DECLARE @curdate DateTime; " +
                    "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock) ; " +
                    "SELECT TOP 1 " +
                        "RegAC.$Рег.АдресОстаткиПоступления.Адрес as Adress1 , " +
                        "max(Gate.ID ) as GateID , " +
                        "max(Gate.Descr ) as GateName, " +
                        "max(Gate.$Спр.Ворота.КоличествоСотрудников ) as CountEmployer " +
                    "FROM " +
                        "RG$Рег.АдресОстаткиПоступления as RegAC (nolock) " +
                        "LEFT JOIN $Спр.ЗоныВорот as ZoneGate (nolock) " +
                            "ON RegAC.$Рег.АдресОстаткиПоступления.Адрес = ZoneGate.$Спр.ЗоныВорот.Секция " +
                        "LEFT JOIN $Спр.Ворота as Gate (nolock) " +
                            "ON (ZoneGate.ParentExt = Gate.ID) " +
                    "WHERE " +
                        "RegAC.period = @curdate " +
                        "and RegAC.$Рег.АдресОстаткиПоступления.ТипДействия = 2 " +
                        "and RegAC.$Рег.АдресОстаткиПоступления.Склад = :Warehouse " +
                        "and ((Gate.ID is NULL) OR (Gate.$Спр.Ворота.ВРазносеСотрудников < Gate.$Спр.Ворота.КоличествоСотрудников )) " +

                    "GROUP BY " +
                        "RegAC.$Рег.АдресОстаткиПоступления.Адрес " +
                    "ORDER BY " +
                        "CASE WHEN (max(Gate.ID ) = :EmptyRef)OR(max(Gate.ID ) is NULL) THEN 999999 ELSE max(Gate.$Спр.Ворота.Приоритет ) END , " +
                        "sum(1) ";
                SQL1S.QuerySetParam(ref TextQuery, "EmptyRef", SQL1S.GetVoidID());
                SQL1S.QuerySetParam(ref TextQuery, "Warehouse", Doc.FromWarehouseID);
                DataTable DT = new DataTable();
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                if (DT.Rows.Count == 0)
                {
                    return false;
                }
                FAdressZoneTransfer = new RefSection(this);
                FAdressZoneTransfer.FoundID(DT.Rows[0]["Adress1"].ToString());
                FZoneName = DT.Rows[0]["GateName"].ToString();
                FZoneID = DT.Rows[0]["GateID"].ToString();

                //теперь укажем, что мы в зоне работаем
                TextQuery =
                   "UPDATE $Спр.Ворота " +
                       "SET " +
                           "$Спр.Ворота.ВРазносеСотрудников = (SELECT TOP 1 $Спр.Ворота.ВРазносеСотрудников + 1 FROM $Спр.Ворота WHERE id = :id) " +
                   "WHERE " +
                       "id = :id ; ";

                QuerySetParam(ref TextQuery, "id", DT.Rows[0]["GateID"].ToString());
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }

            if (!LockDoc(ATDoc.ID))
            {
                return false;
            }

            FCurrentMode = ToMode;
            return true;
        } // ToModeTransfer
        private bool ToModeTransferInicialize()
        {
            DataTable DT;
            string TextQuery =
                "SELECT " +
                    "journ.iddoc as IDDOC, " +
                    "FromWarehouse.descr as FromWarehouseName, " +
                    "FromWarehouse.id as FromWarehouse, " +
                    "ToWarehouse.descr as ToWarehouseName, " +
                    "ToWarehouse.id as ToWarehouse " +
                "FROM " +
                    "_1sjourn as journ (nolock) " +
                    "LEFT JOIN DH$АдресПеремещение as DocAT (nolock) " +
                        "ON DocAT.iddoc = journ.iddoc " +
                    "LEFT JOIN $Спр.Склады as FromWarehouse (nolock) " +
                        "ON FromWarehouse.id = DocAT.$АдресПеремещение.Склад " +
                    "LEFT JOIN $Спр.Склады as ToWarehouse (nolock) " +
                        "ON ToWarehouse.id = DocAT.$АдресПеремещение.СкладПолучатель " +

                "WHERE " +
                    "journ.date_time_iddoc < '19800101Z' " +
                    "and journ.$Автор = :Employer " +
                    "and journ.iddocdef = $АдресПеремещение " +
                    "and DocAT.$АдресПеремещение.ТипДокумента = 2 " +
                    "and journ.ismark = 0 " +
                    "and journ.iddoc in (" +
                        "SELECT right($Спр.СинхронизацияДанных.ДокументВход , 9) " +
                            "FROM $Спр.СинхронизацияДанных (nolock INDEX=VI" + GetSynh("Спр.СинхронизацияДанных.ДокументВход").Substring(2) + ")" +
                                " WHERE not descr = 'Internal' and $Спр.СинхронизацияДанных.ФлагРезультата in (1,2))";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                //Тут сверху и снизу - два почти идентичных запроса. Один проверяет не в обработке ли документ,
                //а второй (ниже) - подтягивает его
                FExcStr = "Подождите, обрабатывается предыдущий документ!";
                return false;
            }
            TextQuery =
                "SELECT " +
                    "journ.iddoc as IDDOC, " +
                    "FromWarehouse.descr as FromWarehouseName, " +
                    "FromWarehouse.id as FromWarehouse, " +
                    "ToWarehouse.descr as ToWarehouseName, " +
                    "ToWarehouse.id as ToWarehouse, " +
                    "ToWarehouse.$Спр.Склады.ОдноадресныйРежим as ToWarehouseSingleAdressMode " +
                "FROM " +
                    "_1sjourn as journ (nolock) " +
                    "LEFT JOIN DH$АдресПеремещение as DocAT (nolock) " +
                        "ON DocAT.iddoc = journ.iddoc " +
                    "LEFT JOIN $Спр.Склады as FromWarehouse (nolock) " +
                        "ON FromWarehouse.id = DocAT.$АдресПеремещение.Склад " +
                    "LEFT JOIN $Спр.Склады as ToWarehouse (nolock) " +
                        "ON ToWarehouse.id = DocAT.$АдресПеремещение.СкладПолучатель " +

                "WHERE " +
                    "journ.date_time_iddoc < '19800101Z' " +
                    "and journ.$Автор = :Employer " +
                    "and journ.iddocdef = $АдресПеремещение " +
                    "and DocAT.$АдресПеремещение.ТипДокумента = 2 " +
                    "and journ.ismark = 0 ";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count != 0) //Документ есть!
            {
                StrictDoc Doc = new StrictDoc();
                Doc.ID = DT.Rows[0]["IDDOC"].ToString();
                Doc.FromWarehouseID = DT.Rows[0]["FromWarehouse"].ToString();
                Doc.FromWarehouseName = DT.Rows[0]["FromWarehouseName"].ToString().Trim();
                Doc.ToWarehouseID = DT.Rows[0]["ToWarehouse"].ToString();
                Doc.ToWarehouseName = DT.Rows[0]["ToWarehouseName"].ToString().Trim();
                Doc.ToWarehouseSingleAdressMode = (int)(decimal)DT.Rows[0]["ToWarehouseSingleAdressMode"] == 1 ? true : false;
                return ToModeTransfer(Doc, Mode.Transfer);
            }

            TextQuery =
                "SELECT " +
                    "Warehouse.id as ID, " +
                    "Warehouse.descr as Name " +
                "FROM " +
                    "$Спр.Склады as Warehouse (nolock) " +
                "WHERE " +
                    "Warehouse.ismark = 0 " +
                    "and Warehouse.$Спр.Склады.ТипСклада = 3 " +
                "ORDER BY " +
                    "Warehouse.descr";
            if (!ExecuteWithRead(TextQuery, out FWarehousesFrom))
            {
                return false;
            }

            TextQuery =
                "SELECT " +
                    "Warehouse.id as ID, " +
                    "Warehouse.descr as Name " +
                "FROM " +
                    "$Спр.Склады as Warehouse (nolock) " +
                "WHERE " +
                    "Warehouse.ismark = 0 " +
                    "and Warehouse.$Спр.Склады.ТипСклада = 0 " +
                "ORDER BY " +
                    "Warehouse.descr";
            if (!ExecuteWithRead(TextQuery, out FWarehousesTo))
            {
                return false;
            }

            FCurrentMode = Mode.TransferInicialize;
            return true;
        } // ToModeTransferInicialize
        private bool ToModeNewChoiseInventory()
        {
            DataTable DT;
            string TextQuery =
                "SELECT " +
                    "journ.iddoc as IDDOC, " +
                    "FromWarehouse.descr as FromWarehouseName, " +
                    "FromWarehouse.id as FromWarehouse, " +
                    "ToWarehouse.descr as ToWarehouseName, " +
                    "ToWarehouse.id as ToWarehouse " +
                "FROM " +
                    "_1sjourn as journ (nolock) " +
                    "LEFT JOIN DH$АдресПеремещение as DocAT (nolock) " +
                        "ON DocAT.iddoc = journ.iddoc " +
                    "LEFT JOIN $Спр.Склады as FromWarehouse (nolock) " +
                        "ON FromWarehouse.id = DocAT.$АдресПеремещение.Склад " +
                    "LEFT JOIN $Спр.Склады as ToWarehouse (nolock) " +
                        "ON ToWarehouse.id = DocAT.$АдресПеремещение.СкладПолучатель " +

                "WHERE " +
                    "journ.date_time_iddoc < '19800101Z' " +
                    "and journ.$Автор = :Employer " +
                    "and journ.iddocdef = $АдресПеремещение " +
                    "and DocAT.$АдресПеремещение.ТипДокумента = 8 " +
                    "and journ.ismark = 0 " +
                    "and journ.iddoc in (" +
                        "SELECT right($Спр.СинхронизацияДанных.ДокументВход , 9) " +
                            "FROM $Спр.СинхронизацияДанных (nolock INDEX=VI" + GetSynh("Спр.СинхронизацияДанных.ДокументВход").Substring(2) + ")" +
                                " WHERE not descr = 'Internal' and $Спр.СинхронизацияДанных.ФлагРезультата in (1,2))";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                //Тут сверху и снизу - два почти идентичных запроса. Один проверяет не в обработке ли документ,
                //а второй (ниже) - подтягивает его
                FExcStr = "Подождите, обрабатывается предыдущий документ!";
                return false;
            }

            TextQuery =
                "SELECT " +
                    "journ.iddoc as IDDOC, " +
                    "FromWarehouse.descr as FromWarehouseName, " +
                    "FromWarehouse.id as FromWarehouse, " +
                    "ToWarehouse.descr as ToWarehouseName, " +
                    "ToWarehouse.id as ToWarehouse, " +
                    "ToWarehouse.$Спр.Склады.ОдноадресныйРежим as ToWarehouseSingleAdressMode " +
                "FROM " +
                    "_1sjourn as journ (nolock) " +
                    "LEFT JOIN DH$АдресПеремещение as DocAT (nolock) " +
                        "ON DocAT.iddoc = journ.iddoc " +
                    "LEFT JOIN $Спр.Склады as FromWarehouse (nolock) " +
                        "ON FromWarehouse.id = DocAT.$АдресПеремещение.Склад " +
                    "LEFT JOIN $Спр.Склады as ToWarehouse (nolock) " +
                        "ON ToWarehouse.id = DocAT.$АдресПеремещение.СкладПолучатель " +
                "WHERE " +
                    "journ.date_time_iddoc < '19800101Z' " +
                    "and journ.$Автор = :Employer " +
                    "and journ.iddocdef = $АдресПеремещение " +
                    "and DocAT.$АдресПеремещение.ТипДокумента = 8 " +
                    "and journ.ismark = 0 " +
                    "and journ.iddoc not in (" +
                        "SELECT right($Спр.СинхронизацияДанных.ДокументВход , 9) " +
                            "FROM $Спр.СинхронизацияДанных (nolock INDEX=VI" + GetSynh("Спр.СинхронизацияДанных.ДокументВход").Substring(2) + ")" +
                                " WHERE not descr = 'Internal' and $Спр.СинхронизацияДанных.ФлагРезультата in (1,2))";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count != 0) //Документ есть!
            {
                StrictDoc Doc = new StrictDoc();
                Doc.ID = DT.Rows[0]["IDDOC"].ToString();
                Doc.FromWarehouseID = DT.Rows[0]["FromWarehouse"].ToString();
                Doc.FromWarehouseName = DT.Rows[0]["FromWarehouseName"].ToString().Trim();
                Doc.ToWarehouseID = DT.Rows[0]["ToWarehouse"].ToString();
                Doc.ToWarehouseName = DT.Rows[0]["ToWarehouseName"].ToString().Trim();
                Doc.ToWarehouseSingleAdressMode = (int)(decimal)DT.Rows[0]["ToWarehouseSingleAdressMode"] == 1 ? true : false;
                return ToModeTransfer(Doc, Mode.NewInventory);
            }

            TextQuery =
                "SELECT " +
                    "min(Sections.descr) as Name " +
                "FROM " +
                    "$Спр.СектораНабора as SetSections (nolock) " +
                    "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                        "ON Sections.id = SetSections.$Спр.СектораНабора.Сектор  " +
                "WHERE " +
                    "SetSections.ismark = 0 " +
                    "and SetSections.parentext = :Employer" +
                "GROUP BY Sections.id " +
                "ORDER BY Name";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            if (!ExecuteWithRead(TextQuery, out Sections))
            {
                return false;
            }

            FCurrentMode = Mode.NewChoiseInventory;
            return true;
        }
        private bool ToModeInventory()
        {
            FExcStr = null;
            if (FInventory == null)
            {
                FInventory = new DataTable();
                FInventory.Columns.Add("ID", Type.GetType("System.String"));
                FInventory.Columns.Add("Number", Type.GetType("System.Int32"));
                FInventory.Columns.Add("ItemName", Type.GetType("System.String"));
                FInventory.Columns.Add("InvCode", Type.GetType("System.String"));
                FInventory.Columns.Add("Article", Type.GetType("System.String"));
                FInventory.Columns.Add("ArticleOnPack", Type.GetType("System.String"));
                FInventory.Columns.Add("Price", Type.GetType("System.Decimal"));
            }
            else
            {
                FInventory.Rows.Clear();
            }
            if (ForPrint == null)
            {
                ForPrint = new DataTable();
                ForPrint.Columns.Add("ID", Type.GetType("System.String"));
                ForPrint.Columns.Add("Number", Type.GetType("System.Int32"));
                ForPrint.Columns.Add("ItemName", Type.GetType("System.String"));
                ForPrint.Columns.Add("InvCode", Type.GetType("System.String"));
                ForPrint.Columns.Add("Article", Type.GetType("System.String"));
                ForPrint.Columns.Add("ArticleOnPack", Type.GetType("System.String"));
                ForPrint.Columns.Add("Price", Type.GetType("System.Decimal"));
            }
            else
            {
                //ForPrint.Rows.Clear();
            }
            FCurrentMode = Mode.Inventory;
            return true;
        } // ToModeInventory
        private bool ToModeChoiseInventory()
        {
            string TextQuery =
                "SELECT " +
                    "Warehouse.id as ID, " +
                    "Warehouse.descr as Name " +
                "FROM " +
                    "$Спр.Склады as Warehouse (nolock) " +
                "WHERE " +
                    "Warehouse.ismark = 0 " +
                    "and Warehouse.$Спр.Склады.ТипСклада = 0 " +
                "ORDER BY " +
                    "Warehouse.descr";
            if (!ExecuteWithRead(TextQuery, out FWarehousesTo))
            {
                return false;
            }
            FCurrentMode = Mode.ChoiseInventory;
            return true;
        }
        private bool ToModeSampleInventory()
        {
            ToModeInventory();
            FCurrentMode = Mode.SampleInventory;
            return true;
        }
        private bool ToModeSamplePut()
        {
            //FExcStr - несет смысл
            string TextQuery;
            DataTable DT;
            if (SampleDoc == null)
            {
                TextQuery =
                    "SELECT " +
                        "journ.iddoc as IDDOC " +
                    "FROM " +
                        "_1sjourn as journ (nolock) " +
                        "LEFT JOIN DH$АдресПеремещение as DocAT (nolock) " +
                            "ON DocAT.iddoc = journ.iddoc " +
                    "WHERE " +
                        "journ.date_time_iddoc < '19800101Z' " +
                        "and journ.$Автор = :Employer " +
                        "and journ.iddocdef = $АдресПеремещение " +
                        "and DocAT.$АдресПеремещение.ТипДокумента = 4 " +
                        "and journ.ismark = 0 ";
                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                if (DT.Rows.Count == 0) //Документа нет!!! Создадим его
                {
                    TextQuery = "BEGIN TRAN; " +
                        "DECLARE @iddoc varchar(9); " +
                        "SELECT TOP 1 @iddoc=journ.iddoc FROM _1sjourn as journ (nolock)" +
                        "   INNER JOIN DH$АдресПеремещение as DocAT (nolock) " +
                                "ON DocAT.iddoc = journ.iddoc " +
                            "WHERE " +
                                "DocAT.$АдресПеремещение.ТипДокумента = 0 " +
                                "and journ.date_time_iddoc < '19800101Z' " +
                                "and journ.iddocdef = $АдресПеремещение " +
                                "and journ.$Автор = :EmptyID " +
                                "and journ.ismark = 0; " +
                        "UPDATE _1sjourn WITH (rowlock) " +
                                "SET _1sjourn.$Автор = :Employer WHERE _1sjourn.iddoc = @iddoc; " +
                        "if @@rowcount > 0 begin " +
                            "UPDATE DH$АдресПеремещение WITH (rowlock) " +
                            "SET $АдресПеремещение.Склад = :IDFrom, " +
                                "$АдресПеремещение.СкладПолучатель = :EmptyID, " +
                                "$АдресПеремещение.ТипДокумента = 4 " +
                            "WHERE DH$АдресПеремещение .iddoc = @iddoc and DH$АдресПеремещение .$АдресПеремещение.ТипДокумента = 0; " +
                            "if @@rowcount > 0 begin " +
                                "COMMIT TRAN; " +
                                "select 1 as result; " +
                            "end else begin ROLLBACK TRAN; select 0 as result; end; " +
                        "end else begin ROLLBACK TRAN; select 0 as result; end; ";
                    QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
                    QuerySetParam(ref TextQuery, "IDFrom", Const.MainWarehouse);
                    QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                    if (!ExecuteWithRead(TextQuery, out DT))
                    {
                        return false;
                    }

                    if ((int)DT.Rows[0]["result"] == 0)
                    {
                        FExcStr = "Не удалось захватить документ. Жмакните повторно!";
                        return false;
                    }
                    //Рекурсивно вызовем, пусть отработает ветка else
                    return ToModeSamplePut();
                }
                else
                {
                    //Документ есть
                    SampleDoc = DT.Rows[0]["IDDOC"].ToString();
                }
            }

            //Подсосем данные уже выложенных позиций
            TextQuery =
                "SELECT " +
                    "DocAT.$АдресПеремещение.Товар as ID, " +
                    "Goods.descr as ItemName, " +
                    "Goods.$Спр.Товары.ИнвКод as InvCode, " +
                    "Goods.$Спр.Товары.Артикул as Article, " +
                    "cast(DocAT.$АдресПеремещение.Количество as int) as Count, " +
                    "DocAT.LINENO_ as LINENO_ " +
                "FROM " +
                    "DT$АдресПеремещение as DocAT (nolock) " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.ID = DocAT.$АдресПеремещение.Товар " +
                "WHERE " +
                    "DocAT.iddoc = :Doc " +
                    "and not DocAT.$АдресПеремещение.Дата0 = :EmptyDate " +
                //"and not DocAT.$АдресПеремещение.Дата1 = :EmptyDate " + 
                "ORDER BY DocAT.$АдресПеремещение.Дата0 , DocAT.$АдресПеремещение.Время0 ";
            QuerySetParam(ref TextQuery, "Doc", SampleDoc);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            if (!ExecuteWithRead(TextQuery, out FTransferReadyItems))
            {
                return false;
            }

            if (SampleTransfers == null)
            {
                SampleTransfers = new DataTable();
                SampleTransfers.Columns.Add("Number", Type.GetType("System.Int32"));
                SampleTransfers.Columns.Add("ATID", Type.GetType("System.String"));
                SampleTransfers.Columns.Add("DOCNO", Type.GetType("System.String"));
                SampleTransfers.Columns.Add("DateDoc", Type.GetType("System.DateTime"));
                SampleTransfers.Columns.Add("DateDocText", Type.GetType("System.String"));
                SampleTransfers.Columns.Add("CountRow", Type.GetType("System.Int32"));
                SampleTransfers.Columns.Add("CountNotAcceptRow", Type.GetType("System.Int32"));
            }
            if (SampleTransfers.Rows.Count > 0)
            {
                DataRow[] DR = SampleTransfers.Select();
                string Docs = "";
                foreach (DataRow dr in DR)
                {
                    Docs += ", '" + dr["ATID"].ToString() + "'";
                }
                Docs = Docs.Substring(2);   //Убираем спедери запятые

                //Подсосем данные регистра остатков
                TextQuery =
                    "DECLARE @curdate DateTime; " +
                    "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " +
                    "SELECT " +
                        "RegAT.$Рег.АдресОстаткиПеремещения.Товар as ID, " +
                        "cast(sum(RegAT.$Рег.АдресОстаткиПеремещения.Количество ) as int) as Count, " +
                        "RegAT.$Рег.АдресОстаткиПеремещения.Док as Doc, " +
                        "RegAT.$Рег.АдресОстаткиПеремещения.Адрес as Adress, " +
                        "min(Goods.descr) as ItemName, " +
                        "min(Goods.$Спр.Товары.ИнвКод ) as InvCode, " +
                        "min(Goods.$Спр.Товары.Артикул ) as Article " +
                    "FROM RG$Рег.АдресОстаткиПеремещения as RegAT (nolock) " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.id = RegAT.$Рег.АдресОстаткиПеремещения.Товар " +
                    "WHERE " +
                        "period = @curdate " +
                        "and RegAT.$Рег.АдресОстаткиПеремещения.Док in (:Docs) " +
                        "and RegAT.$Рег.АдресОстаткиПеремещения.ТипДействия = 4 " +
                        "and RegAT.$Рег.АдресОстаткиПеремещения.Склад = :Warehouse " +
                    "GROUP BY " +
                        "RegAT.$Рег.АдресОстаткиПеремещения.Товар , RegAT.$Рег.АдресОстаткиПеремещения.Док , RegAT.$Рег.АдресОстаткиПеремещения.Адрес " +
                    "HAVING " +
                        "not sum(RegAT.$Рег.АдресОстаткиПеремещения.Количество ) = 0 " +
                    "ORDER BY " +
                        "min(Goods.$Спр.Товары.ИнвКод )";
                QuerySetParam(ref TextQuery, "Warehouse", Const.MainWarehouse);
                TextQuery = TextQuery.Replace(":Docs", Docs);
                if (!ExecuteWithRead(TextQuery, out FSampleItems))
                {
                    return false;
                }
                FSampleItems.Columns.Add("InvCodeFind", Type.GetType("System.String"));
                for (int row = 0; row < FSampleItems.Rows.Count; row++)
                {
                    FSampleItems.Rows[row]["InvCodeFind"] = Helper.SuckDigits(FSampleItems.Rows[row]["InvCode"].ToString().Trim());
                }
                //Расчитываем строчечки
                DR = SampleTransfers.Select();
                foreach (DataRow dr in DR)
                {
                    DataRow[] tmpDR = FSampleItems.Select("Doc = '" + dr["ATID"].ToString() + "'");
                    dr["CountNotAcceptRow"] = tmpDR.Length;
                }
            }
            if (FSampleItems == null)
            {
                FSampleItems = new DataTable();
                FSampleItems.Columns.Add("ID", Type.GetType("System.Int32"));
                FSampleItems.Columns.Add("Count", Type.GetType("System.String"));
                FSampleItems.Columns.Add("Doc", Type.GetType("System.String"));
                FSampleItems.Columns.Add("Adress", Type.GetType("System.String"));
                FSampleItems.Columns.Add("ItemName", Type.GetType("System.String"));
                FSampleItems.Columns.Add("InvCode", Type.GetType("System.String"));
                FSampleItems.Columns.Add("Article", Type.GetType("System.String"));
            }
            if (!LockDoc(SampleDoc))
            {
                return false;
            }

            FCurrentMode = Mode.SamplePut;
            return true;
        } //ToModeSamplePut()
        private bool ToModeHarmonizationInicialize()
        {
            DataTable DT;
            string TextQuery =
                "SELECT " +
                    "journ.iddoc as IDDOC, " +
                    "FromWarehouse.descr as FromWarehouseName, " +
                    "FromWarehouse.id as FromWarehouse, " +
                    "ToWarehouse.descr as ToWarehouseName, " +
                    "ToWarehouse.id as ToWarehouse " +
                "FROM " +
                    "_1sjourn as journ (nolock) " +
                    "LEFT JOIN DH$АдресПеремещение as DocAT (nolock) " +
                        "ON DocAT.iddoc = journ.iddoc " +
                    "LEFT JOIN $Спр.Склады as FromWarehouse (nolock) " +
                        "ON FromWarehouse.id = DocAT.$АдресПеремещение.Склад " +
                    "LEFT JOIN $Спр.Склады as ToWarehouse (nolock) " +
                        "ON ToWarehouse.id = DocAT.$АдресПеремещение.СкладПолучатель " +

                "WHERE " +
                    "journ.date_time_iddoc < '19800101Z' " +
                    "and journ.$Автор = :Employer " +
                    "and journ.iddocdef = $АдресПеремещение " +
                    "and DocAT.$АдресПеремещение.ТипДокумента = 16 " +
                    "and journ.ismark = 0 " +
                    "and journ.iddoc in (" +
                        "SELECT right($Спр.СинхронизацияДанных.ДокументВход , 9) " +
                            "FROM $Спр.СинхронизацияДанных (nolock INDEX=VI" + GetSynh("Спр.СинхронизацияДанных.ДокументВход").Substring(2) + ")" +
                                " WHERE not descr = 'Internal' and $Спр.СинхронизацияДанных.ФлагРезультата in (1,2))";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                //Тут сверху и снизу - два почти идентичных запроса. Один проверяет не в обработке ли документ,
                //а второй (ниже) - подтягивает его
                FExcStr = "Подождите, обрабатывается предыдущий документ!";
                return false;
            }
            TextQuery =
                "SELECT " +
                    "journ.iddoc as IDDOC, " +
                    "min(FromWarehouse.descr) as FromWarehouseName, " +
                    "min(FromWarehouse.id) as FromWarehouse, " +
                    "min(ToWarehouse.descr) as ToWarehouseName, " +
                    "min(ToWarehouse.id) as ToWarehouse, " +
                    "min(ToWarehouse.$Спр.Склады.ОдноадресныйРежим ) as ToWarehouseSingleAdressMode, " +
                    "substring(min(DocAT.$АдресПеремещение.ДокументОснование ), 5, 9) as FoundDoc, " +
                    "ISNULL(max(DocATLines.lineno_), 0) as HaveTab " +
                "FROM " +
                    "_1sjourn as journ (nolock) " +
                    "LEFT JOIN DH$АдресПеремещение as DocAT (nolock) " +
                        "ON DocAT.iddoc = journ.iddoc " +
                    "LEFT JOIN $Спр.Склады as FromWarehouse (nolock) " +
                        "ON FromWarehouse.id = DocAT.$АдресПеремещение.Склад " +
                    "LEFT JOIN $Спр.Склады as ToWarehouse (nolock) " +
                        "ON ToWarehouse.id = DocAT.$АдресПеремещение.СкладПолучатель " +
                    "LEFT JOIN DT$АдресПеремещение as DocATLines (nolock) " +
                        "ON DocATLines.iddoc = DocAT.iddoc " +
                "WHERE " +
                    "journ.date_time_iddoc < '19800101Z' " +
                    "and journ.$Автор = :Employer " +
                    "and journ.iddocdef = $АдресПеремещение " +
                    "and DocAT.$АдресПеремещение.ТипДокумента = 16 " +
                    "and journ.ismark = 0 " +
                 "GROUP BY journ.iddoc";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count != 0) //Документ есть!
            {
                StrictDoc Doc = new StrictDoc();
                Doc.ID = DT.Rows[0]["IDDOC"].ToString();
                Doc.FromWarehouseID = DT.Rows[0]["FromWarehouse"].ToString();
                Doc.FromWarehouseName = DT.Rows[0]["FromWarehouseName"].ToString().Trim();
                Doc.ToWarehouseID = DT.Rows[0]["ToWarehouse"].ToString();
                Doc.ToWarehouseName = DT.Rows[0]["ToWarehouseName"].ToString().Trim();
                Doc.ToWarehouseSingleAdressMode = (int)(decimal)DT.Rows[0]["ToWarehouseSingleAdressMode"] == 1 ? true : false;
                Doc.FoundDoc = DT.Rows[0]["FoundDoc"].ToString();
                if ((short)DT.Rows[0]["HaveTab"] == 0)
                {
                    //Набор гармонизации
                    return ToModeHarmonization(Doc);
                }
                else
                {
                    //Это выкладка гармонизации
                    return ToModeHarmonizationPut(Doc);
                }
            }

            TextQuery =
                "SELECT " +
                    "Warehouse.id as ID, " +
                    "Warehouse.descr as Name " +
                "FROM " +
                    "$Спр.Склады as Warehouse (nolock) " +
                "WHERE " +
                    "Warehouse.ismark = 0 " +
                    "and Warehouse.$Спр.Склады.ТипСклада = 0 " +
                "ORDER BY " +
                    "Warehouse.descr";
            if (!ExecuteWithRead(TextQuery, out FWarehousesFrom))
            {
                return false;
            }

            FCurrentMode = Mode.HarmonizationInicialize;
            return true;
        }
        private bool ToModeHarmonization(StrictDoc Doc)
        {
            string TextQuery =
                "SELECT " +
                    "DocAT.$АдресПеремещение.Товар as ID, " +
                    "Goods.descr as ItemName, " +
                    "Goods.$Спр.Товары.ИнвКод  as InvCode, " +
                    "Goods.$Спр.Товары.Артикул  as Article, " +
                    "DocAT.$АдресПеремещение.Количество  as Count, " +
                    "DocAT.$АдресПеремещение.Адрес0 as Adress0, " +
                    "CASE WHEN round(DocAT.$АдресПеремещение.Количество /ISNULL(Package.Coef, 1), 0)*ISNULL(Package.Coef, 1) = DocAT.$АдресПеремещение.Количество " +
                        "THEN ISNULL(Package.Coef, 1) ELSE 1 END as Coef, " +
                    "CASE WHEN round(DocAT.$АдресПеремещение.Количество /ISNULL(Package.Coef, 1), 0)*ISNULL(Package.Coef, 1) = DocAT.$АдресПеремещение.Количество " +
                        "THEN cast(round(DocAT.$АдресПеремещение.Количество /ISNULL(Package.Coef, 1), 0) as int) " +
                        "ELSE DocAT.$АдресПеремещение.Количество  END as CountPackage, " +
                        "Sections.descr as AdressName " +
                "FROM " +
                    "DT$АдресПеремещение as DocAT (nolock) " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.ID = DocAT.$АдресПеремещение.Товар " +
                    "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                        "ON Sections.ID = DocAT.$АдресПеремещение.Адрес1 " +
                    "LEFT JOIN ( " +
                                "SELECT " +
                                    "Units.parentext as ItemID, " +
                                    "min(Units.$Спр.ЕдиницыШК.Коэффициент ) as Coef " +
                                "FROM " +
                                    "$Спр.ЕдиницыШК as Units (nolock) " +
                                "WHERE " +
                                    "Units.$Спр.ЕдиницыШК.ОКЕИ = :OKEIPackage " +
                                    "and Units.ismark = 0 " +
                                "GROUP BY " +
                                    "Units.parentext ) as Package " +
                        "ON Package.ItemID = Goods.ID " +
                "WHERE " +
                    "DocAT.iddoc = :Doc " +
                    "and DocAT.$АдресПеремещение.ФлагДопроведения = 1 " +
                "ORDER BY DocAT.$АдресПеремещение.Дата0 , DocAT.$АдресПеремещение.Время0 ";
            QuerySetParam(ref TextQuery, "Doc", Doc.FoundDoc);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "OKEIPackage", OKEIPackage);
            QuerySetParam(ref TextQuery, "Warehouse", Doc.ToWarehouseID);
            if (!ExecuteWithRead(TextQuery, out FTransferItems))
            {
                return false;
            }
            ATDoc = Doc;
            FCurrentMode = Mode.Harmonization;
            return true;
        } // ToModeHarmonization
        private bool ToModeHarmonizationPut(StrictDoc Doc)
        {
            string TextQuery =
                "SELECT " +
                    "DocAT.$АдресПеремещение.Товар as ID, " +
                    "DocAT.$АдресПеремещение.Адрес1 as Adress0, " +
                    "min(Goods.descr) as ItemName, " +
                    "min(Goods.$Спр.Товары.ИнвКод ) as InvCode, " +
                    "sum(DocAT.$АдресПеремещение.Количество ) as Count, " +
                    "min(Sections.descr) as AdressName " +
                "FROM " +
                    "DT$АдресПеремещение as DocAT (nolock) " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.ID = DocAT.$АдресПеремещение.Товар " +
                    "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                        "ON Sections.ID = DocAT.$АдресПеремещение.Адрес1 " +
                "WHERE " +
                    "DocAT.iddoc = :Doc " +
                    "and not DocAT.$АдресПеремещение.Дата1 = :EmptyDate " +
                "GROUP BY DocAT.$АдресПеремещение.Товар , DocAT.$АдресПеремещение.Адрес1 " +
                "ORDER BY max(DocAT.$АдресПеремещение.Дата0 ), max(DocAT.$АдресПеремещение.Время0 )";
            QuerySetParam(ref TextQuery, "Doc", Doc.ID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "OKEIPackage", OKEIPackage);
            QuerySetParam(ref TextQuery, "Warehouse", Doc.ToWarehouseID);
            if (!ExecuteWithRead(TextQuery, out FTransferItems))
            {
                return false;
            }
            ATDoc = Doc;
            FCurrentMode = Mode.HarmonizationPut;
            return true;
        }
        private bool ToModeLoadingInicialization()
        {
            string TextQuery =
                "SELECT " +
                    "PL.iddoc as iddoc " +
                "FROM DH$ПутевойЛист as PL (nolock) " +
                    "INNER JOIN _1sjourn as journ (nolock) " +
                        "ON journ.iddoc = PL.iddoc " +
                "WHERE " +
                    "(PL.$ПутевойЛист.Грузчик = :Employer " +
                    "OR PL.$ПутевойЛист.Укладчик = :Employer " +
                    "OR PL.$ПутевойЛист.Укладчик2 = :Employer " +
                    "OR PL.$ПутевойЛист.Укладчик3 = :Employer )" + 
                    "and not PL.$ПутевойЛист.Дата1 = :EmptyDate " +
                    "and PL.$ПутевойЛист.Дата2 = :EmptyDate " +
                    "and journ.ismark = 0 " +
                "ORDER BY journ.date_time_iddoc";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count > 0)
            {
                //существует документ!
                return ToModeLoading(DT.Rows[0]["iddoc"].ToString());
            }

            FCurrentMode = Mode.LoadingInicialization;
            Placer = new RefEmployer(this);
            WayBill.ID = null;
            WayBill.View = null;
            return true;
        }
        private bool ToModeLoading(string iddoc)
        {
            //Если wayBill еще не выбран, то испавим это недоразумение
            string IDDoc;
            string DocType;
            Dictionary<string, object> DataMap;
            if (WayBill.ID != iddoc)
            {
                if (!GetDoc(iddoc, out IDDoc, out DocType, out DataMap, true))
                {
                    return false;
                }
                WayBill.View = DataMap["НомерДок"].ToString() + " (" + ((DateTime)DataMap["ДатаДок"]).ToString("dd.MM.yy") + ")";
                WayBill.ID = iddoc;
            }

            //отменим блокировку документа временно с 4.47 версия
            //if (!LockDoc(WayBill.ID))
            //{
            //    return false;
            //}

            //проверим чек погрузки
            string TextQuery =
            "select DocWayBill.$ПутевойЛист.ЧекПогрузка " +
            "from DH$ПутевойЛист as DocWayBill (nolock) " +
            "where " +
                "DocWayBill.iddoc = :iddoc ";
            QuerySetParam(ref TextQuery, "iddoc", WayBill.ID);
            if (Int32.Parse(ExecuteScalar(TextQuery).ToString())==0)               
            {
                //погрузка не разрешена
                FExcStr = "Погрузка запрещена!";
                return false;
            }          

            TextQuery =
                "SELECT " +
                    "Main.AdressCounter as AdressCounter, " +
                    "Main.Adress as Adress, " +
                    "ISNULL(RefSection.descr, 'Нет адреса') as AdressCompl, " +
                    "Main.Doc as Doc, " +
                    "Journ.docno as ProposalNumber, " +
                    "CAST(LEFT(journ.date_time_iddoc,8) as DateTime) as ProposalDate, " +
                    "ISNULL(Boxes.CountBox, 0) as Boxes, " +
                    "ISNULL(BoxesComplete.CountBox, 0) as BoxesFact, " +
                    "Main.Number as Number " +
                "FROM ( " + GiveSomeOneQueryText() +
                    ") as Main " +
                    "LEFT JOIN (" +
                                "SELECT " +
                                    "Boxes.$Спр.МестаПогрузки.Док as DocID, " +
                                    "Boxes.$Спр.МестаПогрузки.Адрес9  as AdressCompl, " +
                                    "Count(*) as CountBox " +
                                "FROM $Спр.МестаПогрузки as Boxes (nolock) " +
                                "WHERE Boxes.ismark = 0 " +
                                "GROUP BY Boxes.$Спр.МестаПогрузки.Док , Boxes.$Спр.МестаПогрузки.Адрес9 " +
                                ") as Boxes " +
                        "ON Boxes.DocID = Main.DocFull " +
                    "LEFT JOIN (" +
                                "SELECT " +
                                    "Boxes.$Спр.МестаПогрузки.Док as DocID, " +
                                    "Boxes.$Спр.МестаПогрузки.Адрес9 as AdressCompl, " +
                                    "Count(*) as CountBox " +
                                "FROM $Спр.МестаПогрузки as Boxes (nolock) " +
                                "WHERE Boxes.ismark = 0 and not Boxes.$Спр.МестаПогрузки.Дата6 = :EmptyDate " +
                                "GROUP BY Boxes.$Спр.МестаПогрузки.Док , Boxes.$Спр.МестаПогрузки.Адрес9 " +
                                ") as BoxesComplete " +
                        "ON BoxesComplete.DocID = Main.DocFull " +
                        "and Boxes.AdressCompl = BoxesComplete.AdressCompl " +                            
                    "LEFT JOIN _1sjourn as journ (nolock) " +
                        "ON journ.iddoc = Main.Doc " +
                    "LEFT JOIN $Спр.Секции as RefSection (nolock) " +
                        "ON RefSection.id = Boxes.AdressCompl OR RefSection.id = BoxesComplete.AdressCompl " +
                "WHERE " +
                    "not ISNULL(BoxesComplete.CountBox, 0) = ISNULL(Boxes.CountBox, 0) " +
                    "and not ((ISNULL(journ.iddocdef,'') = $ПретензияОтКлиента ) " +
                    "and (SUBSTRING(ISNULL(RefSection.descr, '80'),1,2) = '80')) " +
                "ORDER BY Main.AdressCounter desc, Main.Number desc " +
                    "";

            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
            QuerySetParam(ref TextQuery, "iddoc", WayBill.ID);
            if (!ExecuteWithRead(TextQuery, out WayBillDT))
            {
                return false;
            }

            //int counter = 0;
            //DataRow[] DR = WayBillDT.Select();
            //string Adress = "";
            //for (int i = WayBillDT.Rows.Count - 1; i >= 0; i--)
            //{
            //    if (WayBillDT.Rows[i]["Adress"].ToString() != Adress)
            //    {
            //        counter++;
            //        Adress = WayBillDT.Rows[i]["Adress"].ToString();
            //    }
            //    WayBillDT.Rows[i]["AdressCounter"] = counter;
            //}

            //foreach(DataRow dr in DR)
            //{
            //    if ((int)dr["Boxes"] == (int)dr["BoxesFact"])
            //    {
            //        dr.Delete();
            //    }
            //}

            CurrentLine = 0;
            FCurrentMode = Mode.Loading;
            return true;
        }
        public bool ToModeChoiseDown()
        {
            LastGoodAdress = null;
            Const.Refresh();
            Employer.Refresh();     // проверим не изменились ли галки на спуск/комплектацию
            if (!Employer.CanDown && !Employer.CanComplectation)
            {
                CurrentMode = Mode.ChoiseWork;
                FCurrentMode = CurrentMode;
                OnChangeMode(new ChangeModeEventArgs(MM));
                return true;
            }
            if (!Employer.CanDown && Employer.CanComplectation)
            {
                CurrentMode = Mode.ChoiseWork;
                FCurrentMode = CurrentMode;
                OnChangeMode(new ChangeModeEventArgs(MM));
                return true;
            }
            //Сам запрос
            string TextQuery = "select * from WPM_fn_GetChoiseDown()";
            if (!ExecuteWithRead(TextQuery, out DownSituation))
            {
                return false;
            }

            DataTable DT;
            TextQuery = "select * from dbo.WPM_fn_ComplectationInfo()";
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                PreviousAction = " < error > ";
            }
            else
            {
                PreviousAction = DT.Rows[0]["pallets"].ToString() + " п, " + DT.Rows[0]["box"].ToString() + " м, " + DT.Rows[0]["CountEmployers"].ToString() + " с";
            }

            if (DownSituation.Rows.Count == 0)
            {
                FExcStr = "Нет заданий к спуску...";
            }
            else
            {
                FExcStr = "Выберите сектор спуска...";
            }
            FCurrentMode = Mode.ChoiseDown;
            return true;
        }
        public bool ToModeDown()
        {

            DocDown = new StrictDoc();
            string TextQuery = "select * from dbo.WPM_fn_ToModeDown(:Employer)";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            //QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count > 0)
            {
                DocDown.ID = DT.Rows[0]["iddoc"].ToString();
                DocDown.Boxes = (int)DT.Rows[0]["CountBox"];
                DocDown.View = DT.Rows[0]["Sector"].ToString().Trim() + "-" + DT.Rows[0]["Number"].ToString() + " Заявка " + DT.Rows[0]["DocNo"].ToString() + " (" + DT.Rows[0]["DateDoc"].ToString() + ")";
                DocDown.AdressCollect = DT.Rows[0]["AdressCollect"].ToString();
                DocDown.Sector = DT.Rows[0]["ParentSector"].ToString();
                DocDown.MaxStub = (int)DT.Rows[0]["MaxStub"];
                DocDown.AllBoxes = (int)DT.Rows[0]["CountAllBox"];
                DocDown.NumberBill = DT.Rows[0]["DocNo"].ToString().Trim();
                DocDown.NumberCC = (int)DT.Rows[0]["Number"];
                DocDown.MainSectorName = DT.Rows[0]["Sector"].ToString();
                DocDown.SetterName = DT.Rows[0]["SetterName"].ToString();
                FExcStr = "Сканируйте места";
                FCurrentMode = Mode.Down;
                return true;
            }
            //Собирать - нечего,
            return ToModeDownComplete();

        }
        private bool ToModeDownComplete()
        {
            if (DownSituation != null)
            {
                DownSituation.Clear();
            }
            //В этот режим попадает только если нечего собирать по Дате4, так что если и тут нет ничего - то уходит на режим ChoiseDown
            string TextQuery =
                "select " +
                        "min(Ref.$Спр.МестаПогрузки.НомерЗадания5 ) as NumberOfOrder, " +
                        "Count(*) as AllBox " +
                    "from $Спр.МестаПогрузки as Ref (nolock) " +
                    "where " +
                        "Ref.ismark = 0 " +
                        "and Ref.$Спр.МестаПогрузки.Сотрудник4 = :Employer " +
                        "and not Ref.$Спр.МестаПогрузки.Дата4 = :EmptyDate " +
                        "and Ref.$Спр.МестаПогрузки.Дата5 = :EmptyDate ";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            if (!ExecuteWithRead(TextQuery, out DownSituation))
            {
                return false;
            }
            if ((int)DownSituation.Rows[0]["AllBox"] == 0)
            {
                //Нету нихуя!
                bool res = ToModeChoiseDown();
                OnChangeMode(new ChangeModeEventArgs(FCurrentMode));
                return res;
            }
            FExcStr = "Напечатать этикетку, отсканировать адрес паллеты";
            FCurrentMode = Mode.DownComplete;
            return true;
        }
        public bool ToModeFreeDownComplete()
        {
            DocDown = new StrictDoc();
            string TextQuery = "select * from dbo.WPM_fn_ToModeFreeDownComplete(:Employer)";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            //QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
            //QuerySetParam(ref TextQuery, "EmptyID",     GetVoidID());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                if (BadDoc.ID == null)
                {
                    LoadBadDoc(DT.Rows[0]["id"].ToString());
                }

                DocDown.ID = DT.Rows[0]["id"].ToString();
                DocDown.View = DT.Rows[0]["Sector"].ToString().Trim() + "-" + DT.Rows[0]["Number"].ToString() + " Заявка " + DT.Rows[0]["DocNo"].ToString() + " (" + DT.Rows[0]["DateDoc"].ToString() + ")";
                DocDown.NumberBill = DT.Rows[0]["DocNo"].ToString().Trim();
                DocDown.NumberCC = (int)DT.Rows[0]["Number"];
                DocDown.MainSectorName = DT.Rows[0]["Sector"].ToString();
                DocDown.SetterName = DT.Rows[0]["SetterName"].ToString();
                DocDown.Boxes = (int)DT.Rows[0]["NumberBox"];
                FExcStr = "Отсканируйте адрес!";
            }
            else
            {
                FExcStr = "Отсканируйте место!";
            }
            FCurrentMode = Mode.FreeDownComplete;
            return true;
        }
        public bool ToModeNewComplectation()
        {
            DocDown = new StrictDoc();
            string TextQuery = "select * from dbo.WPM_fn_ToModeNewComplectation(:Employer)";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            //QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
            //QuerySetParam(ref TextQuery, "EmptyID",     GetVoidID());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count > 0)
            {
                if (BadDoc.ID == null)
                {
                    LoadBadDoc(DT.Rows[0]["RefID"].ToString());
                }

                DocDown.ID = DT.Rows[0]["iddoc"].ToString();
                DocDown.Boxes = (int)DT.Rows[0]["CountBox"];
                DocDown.View = DT.Rows[0]["Sector"].ToString().Trim() + "-" + DT.Rows[0]["Number"].ToString() + " Заявка " + DT.Rows[0]["DocNo"].ToString() + " (" + DT.Rows[0]["DateDoc"].ToString() + ")";
                DocDown.AdressCollect = DT.Rows[0]["AdressCollect"].ToString();
                DocDown.Sector = DT.Rows[0]["ParentSector"].ToString();
                DocDown.MaxStub = (int)DT.Rows[0]["MaxStub"];
                DocDown.AllBoxes = (int)DT.Rows[0]["CountAllBox"];
                DocDown.NumberBill = DT.Rows[0]["DocNo"].ToString().Trim();
                DocDown.NumberCC = (int)DT.Rows[0]["Number"];
                DocDown.MainSectorName = DT.Rows[0]["Sector"].ToString();
                DocDown.SetterName = DT.Rows[0]["SetterName"].ToString();
                DocDown.IsFirstOrder = (int)DT.Rows[0]["FlagFirstOrder"] == 1 ? true : false;
                FExcStr = "Сканируйте место";
                OnChangeMode(new ChangeModeEventArgs(Mode.NewComplectation));
                return true;
            }
            //Ничего нет, может есть паллета?
            return ToModeNewComplectationComplete();
        }
        private bool ToModeNewComplectationComplete()
        {
            if (DownSituation != null)
            {
                DownSituation.Clear();
            }
            string TextQuery =
                "select " +
                        "min(Ref.$Спр.МестаПогрузки.НомерЗадания7 ) as NumberOfOrder, " +
                        "Count(*) as AllBox " +
                    "from $Спр.МестаПогрузки as Ref (nolock) " +
                    "where " +
                        "Ref.ismark = 0 " +
                        "and Ref.$Спр.МестаПогрузки.Сотрудник8 = :Employer " +
                        "and not Ref.$Спр.МестаПогрузки.Дата8 = :EmptyDate " +
                        "and Ref.$Спр.МестаПогрузки.Дата9 = :EmptyDate ";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            if (!ExecuteWithRead(TextQuery, out DownSituation))
            {
                return false;
            }
            if ((int)DownSituation.Rows[0]["AllBox"] == 0)
            {
                //Нету нихуя!
                bool res = ToModeChoiseDown();
                OnChangeMode(new ChangeModeEventArgs(FCurrentMode));
                return res;
            }

            //Подсосем маршруты
            RefreshRoute();

            FExcStr = "Сканируйте коробки и адрес комплектации!";
            ScaningBox = null;
            FCurrentMode = Mode.NewComplectationComplete;
            OnChangeMode(new ChangeModeEventArgs(Mode.NewComplectationComplete));
            return true;
        }
        private bool ToModeAcceptanceCross()
        {
            if (FConsignment == null)
            {
                FConsignment = new DataTable();
                FConsignment.Columns.Add("Number", Type.GetType("System.Int32"));
                FConsignment.Columns.Add("ACID", Type.GetType("System.String"));
                FConsignment.Columns.Add("ParentIDD", Type.GetType("System.String"));
                FConsignment.Columns.Add("DOCNO", Type.GetType("System.String"));
                FConsignment.Columns.Add("DateDoc", Type.GetType("System.DateTime"));
                FConsignment.Columns.Add("DateDocText", Type.GetType("System.String"));
                FConsignment.Columns.Add("CountRow", Type.GetType("System.Int32"));
                FConsignment.Columns.Add("CountNotAcceptRow", Type.GetType("System.Int32"));
            }

            if (FNotAcceptedItems == null)
            {
                FNotAcceptedItems = new DataTable();
                FNotAcceptedItems.Columns.Add("ID", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("LINENO_", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("Number", Type.GetType("System.Int32")); //Номер строки документа
                FNotAcceptedItems.Columns.Add("ItemName", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("IDDOC", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("DOCNO", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("InvCode", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("Article", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("ArticleOnPack", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("ArticleFind", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("ArticleOnPackFind", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("ItemNameFind", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("Count", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("CountPackage", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("Coef", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("CoefView", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("LabelCount", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("Unit", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("Price", Type.GetType("System.Decimal"));
                FNotAcceptedItems.Columns.Add("Details", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("SeasonGroup", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("FlagFarWarehouse", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("StoregeSize", Type.GetType("System.Int32"));

                FNotAcceptedItems.Columns.Add("ClientName", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("ClientID", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("OrderName", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("OrderID", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("AddressID", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("AddressName", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("PalletID", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("PalletName", Type.GetType("System.String"));
                FNotAcceptedItems.Columns.Add("BoxCount", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("CountAll", Type.GetType("System.Int32"));
                FNotAcceptedItems.Columns.Add("CloseOrder", Type.GetType("System.String"));                
                
                FAcceptedItems = FNotAcceptedItems.Clone();
            }
            else
            {
                FNotAcceptedItems.Rows.Clear();
                FAcceptedItems.Rows.Clear();
            }

            if (FConsignment.Rows.Count > 0)
            {
                DataRow[] DR = FConsignment.Select();
                string Docs = "";
                foreach (DataRow dr in DR)
                {
                    Docs += ", '" + dr["ACID"].ToString() + "'";
                }
                Docs = Docs.Substring(2);   //Убираем спедери запятые

                //Непринятый товар
                string TextQuery =
                    "DECLARE @curdate DateTime; " +
                    "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " +
                    "SELECT " +
                        "right(Journ.docno,5) as DOCNO," +
                        "Supply.iddoc as iddoc," +
                        "Goods.id as id," +
                        "Goods.Descr as ItemName," +
                        "Goods.$Спр.Товары.ИнвКод as InvCode," +
                        "Goods.$Спр.Товары.Артикул as Article," +
                        "Goods.$Спр.Товары.АртикулНаУпаковке as ArticleOnPack," +
                        "Goods.$Спр.Товары.Прих_Цена as Price," +
                        "Goods.$Спр.Товары.КоличествоДеталей as Details, " +
                        "CASE WHEN round(Supply.$АдресПоступление.Количество /ISNULL(Package.Coef, 1), 0)*ISNULL(Package.Coef, 1) = Supply.$АдресПоступление.Количество " +
                            "THEN ISNULL(Package.Coef, 1) ELSE 1 END as Coef, " +
                        "CASE WHEN round( " +
                        "(CASE WHEN Supply.$АдресПоступление.Количество <= ISNULL(OrderOnClients.Balance,0) THEN Supply.$АдресПоступление.Количество ELSE ISNULL(OrderOnClients.Balance,0) END) " +
                        "/ISNULL(Package.Coef, 1), 0)*ISNULL(Package.Coef, 1) = " +
                        "(CASE WHEN Supply.$АдресПоступление.Количество <= ISNULL(OrderOnClients.Balance,0) THEN Supply.$АдресПоступление.Количество ELSE ISNULL(OrderOnClients.Balance,0) END) " +
                            "THEN round( " +
                        "(CASE WHEN Supply.$АдресПоступление.Количество <= ISNULL(OrderOnClients.Balance,0) THEN Supply.$АдресПоступление.Количество ELSE ISNULL(OrderOnClients.Balance,0) END) " +
                        "/ISNULL(Package.Coef, 1), 0) " +
                            "ELSE (CASE WHEN Supply.$АдресПоступление.Количество <= ISNULL(OrderOnClients.Balance,0) THEN Supply.$АдресПоступление.Количество ELSE ISNULL(OrderOnClients.Balance,0) END) END as CountPackage , " +
                        "(CASE WHEN Supply.$АдресПоступление.Количество <= ISNULL(OrderOnClients.Balance,0) THEN Supply.$АдресПоступление.Количество ELSE ISNULL(OrderOnClients.Balance,0) END) as Count," +
                        "Supply.$АдресПоступление.ЕдиницаШК as Unit," +
                        "Supply.$АдресПоступление.КоличествоЭтикеток as LabelCount," +
                        "Supply.$АдресПоступление.НомерСтрокиДока as Number," +
                        "Supply.$АдресПоступление.ГруппаСезона as SeasonGroup, " +
                        "SypplyHeader.$АдресПоступление.ДальнийСклад as FlagFarWarehouse," +
                        "Supply.LineNO_ as LineNO_, " +
                    //"isnull(GS.$Спр.ТоварныеСекции.РазмерХранения , 0) as StoregeSize " +
                        "isnull(GS.$Спр.ТоварныеСекции.РасчетныйРХ , 0) as StoregeSize, " +
                        "OrderOnClients.ClientID as ClientID, " +
                        "OrderOnClients.ClientName as ClientName, " +
                        "OrderOnClients.OrderID as OrderID, " +
                        "OrderOnClients.OrderName as OrderName, " +
                        "OrderOnClients.AddressID as AddressID, " +
                        "OrderOnClients.AddressName as AddressName, " +
                        "OrderOnClients.PalletID as PalletID , " +
                        "OrderOnClients.PalletName as PalletName , " +                        
                        "isNull(OrderOnClients.BoxCount,0) as BoxCount, " +
                        "Supply.$АдресПоступление.Количество as CountAll " +
                    "FROM " +
                        "DT$АдресПоступление as Supply (nolock) " +
                        "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                            "ON Goods.ID = Supply.$АдресПоступление.Товар " +
                        "LEFT JOIN DH$АдресПоступление as SypplyHeader (nolock) " +
                            "ON SypplyHeader.iddoc = Supply.iddoc " +
                        "LEFT JOIN _1sjourn as Journ (nolock) " +
                            "ON Journ.iddoc = Right(SypplyHeader.$АдресПоступление.ДокументОснование , 9) " +
                        "LEFT JOIN ( " +
                                "SELECT " +
                                    "Units.parentext as ItemID, " +
                                    "min(Units.$Спр.ЕдиницыШК.Коэффициент ) as Coef " +
                                "FROM " +
                                    "$Спр.ЕдиницыШК as Units (nolock) " +
                                "WHERE " +
                                    "Units.$Спр.ЕдиницыШК.ОКЕИ = :OKEIPackage " +
                                    "and Units.ismark = 0 " +
                                    "and not Units.$Спр.ЕдиницыШК.Коэффициент = 0 " +
                                "GROUP BY " +
                                    "Units.parentext " +
                                ") as Package " +
                            "ON Package.ItemID = Goods.ID " +
                        "LEFT JOIN $Спр.ТоварныеСекции as GS (nolock) " +
                            "on GS.parentext = goods.id and gs.$Спр.ТоварныеСекции.Склад = :Warehouse " +
                        "LEFT JOIN ( " +
                            "SELECT " +
                                "$Рег.ЗаказыНаКлиентов.Клиент as ClientID , " +
                                "min(Clients.Descr ) as ClientName , " +
                                "$Рег.ЗаказыНаКлиентов.Товар as Item , " +
                                "$Рег.ЗаказыНаКлиентов.Склад as Warehouse , " +
                                "sum(OrderOnClientItem.$ЗаказНаКлиента.Количество ) - sum(OrderOnClientItem.$ЗаказНаКлиента.Принято ) as Balance , " +
                                "min(Journ.IDDOC) as OrderID , " +
                                "min(right(Journ.docno,5)) as OrderName , " +                        
                                "min(Sections.ID ) as AddressID , " +
                                "min($ЗаказНаКлиента.КолМест ) as BoxCount , " +
                                "min(Pallets.ID ) as PalletID , " +
                                "min(SUBSTRING(Pallets.$Спр.ПеремещенияПаллет.ШКПаллеты ,9,4) ) as PalletName , " +
                                "min(Sections.Descr ) as AddressName , " +
                                "min(OrderOnClient.$ЗаказНаКлиента.ДокументОснование ) as OrderDocOsn " +
                            "FROM " +
                                "RG$Рег.ЗаказыНаКлиентов as RegOrderOnClient (nolock) " +
                                "LEFT JOIN $Спр.Клиенты as Clients (nolock) " +
                                    "on Clients.ID = $Рег.ЗаказыНаКлиентов.Клиент " + 
                                "LEFT JOIN DH$ЗаказНаКлиента as OrderOnClient (nolock) " +
                                       "on OrderOnClient.IDDOC = Right($Рег.ЗаказыНаКлиентов.Док , 9) " +
                                "LEFT JOIN DT$ЗаказНаКлиента as OrderOnClientItem (nolock) " +
                                       "on OrderOnClient.IDDOC = OrderOnClientItem.IDDOC " +
                                       "and OrderOnClientItem.$ЗаказНаКлиента.Товар = $Рег.ЗаказыНаКлиентов.Товар " +
                                "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                                    "on Sections.ID = OrderOnClient.$ЗаказНаКлиента.Адрес " +
                                "LEFT JOIN $Спр.ПеремещенияПаллет as Pallets (nolock) " +
                                    "on Pallets.ID = OrderOnClient.$ЗаказНаКлиента.Паллета " +
                                "LEFT JOIN _1sjourn as Journ (nolock) " +
                                    "on Journ.IDDOC = Right($Рег.ЗаказыНаКлиентов.Док , 9) " +
                            "WHERE " +
                                "RegOrderOnClient.period = @curdate " +
                            "GROUP BY " + 
                                "$Рег.ЗаказыНаКлиентов.Клиент , " +
                                "$Рег.ЗаказыНаКлиентов.Товар , " +
                                "$Рег.ЗаказыНаКлиентов.Склад , " +
                                "$Рег.ЗаказыНаКлиентов.Док " +
                             "HAVING " +
                                "sum(OrderOnClientItem.$ЗаказНаКлиента.Количество ) - sum(OrderOnClientItem.$ЗаказНаКлиента.Принято ) > 0 " +
                                "and sum(RegOrderOnClient.$Рег.ЗаказыНаКлиентов.Количество ) > 0 " +
                                ") as OrderOnClients " +
                            "on Goods.ID = OrderOnClients.Item " +
                            "and SypplyHeader.$АдресПоступление.Склад = OrderOnClients.Warehouse " +
                            "and SypplyHeader.$АдресПоступление.ДокументОснование = OrderOnClients.OrderDocOsn " +  
                    "WHERE " +
                        "Supply.IDDOC in (:Docs) " +
                        "and Supply.$АдресПоступление.Состояние0 = 0 " +
                    "ORDER BY " +
                        "Journ.docno, OrderOnClients.OrderID, Supply.LineNO_ ";
                TextQuery = TextQuery.Replace(":Docs", Docs);
                QuerySetParam(ref TextQuery, "OKEIPackage", OKEIPackage);
                QuerySetParam(ref TextQuery, "Warehouse", Const.MainWarehouse);
                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                DT.Columns.Add("ArticleFind", Type.GetType("System.String"));
                DT.Columns.Add("ArticleOnPackFind", Type.GetType("System.String"));
                DT.Columns.Add("ItemNameFind", Type.GetType("System.String"));
                DT.Columns.Add("CoefView", Type.GetType("System.String"));
                DT.Columns.Add("CloseOrder", Type.GetType("System.String"));
                for (int row = 0; row < DT.Rows.Count; row++)
                {
                    DT.Rows[row]["ArticleFind"] = Helper.SuckDigits(DT.Rows[row]["Article"].ToString().Trim());
                    DT.Rows[row]["ArticleOnPackFind"] = Helper.SuckDigits(DT.Rows[row]["ArticleOnPack"].ToString().Trim());
                    DT.Rows[row]["ItemNameFind"] = Helper.SuckDigits(DT.Rows[row]["ItemName"].ToString().Trim());
                    DT.Rows[row]["CoefView"] = ((int)(decimal)DT.Rows[row]["Coef"] == 1 ? "??  " : "")
                                                            + ((int)(decimal)DT.Rows[row]["Coef"]).ToString();
                    DT.Rows[row]["CloseOrder"] = "";
                }
                FNotAcceptedItems.Merge(DT, false, MissingSchemaAction.Ignore);

                //Теперь принятый товар
                TextQuery =
                    "DECLARE @curdate DateTime; " +
                    "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " +
                    "SELECT " +
                        "right(Journ.docno,5) as DOCNO," +
                        "Supply.iddoc as iddoc," +
                        "Goods.id as id," +
                        "Goods.Descr as ItemName," +
                        "Goods.$Спр.Товары.ИнвКод as InvCode," +
                        "Goods.$Спр.Товары.Артикул as Article," +
                        "Goods.$Спр.Товары.АртикулНаУпаковке as ArticleOnPack," +
                        "Goods.$Спр.Товары.Прих_Цена as Price," +
                        "Goods.$Спр.Товары.КоличествоДеталей as Details, " +
                        "CASE WHEN round(Supply.$АдресПоступление.Количество /ISNULL(Package.Coef, 1), 0)*ISNULL(Package.Coef, 1) = Supply.$АдресПоступление.Количество " +
                            "THEN ISNULL(Package.Coef, 1) ELSE 1 END as Coef, " +
                        "CASE WHEN round( " +
                        "(CASE WHEN Supply.$АдресПоступление.Количество <= ISNULL(OrderOnClients.Balance,0) THEN Supply.$АдресПоступление.Количество ELSE ISNULL(OrderOnClients.Balance,0) END) " +
                        "/ISNULL(Package.Coef, 1), 0)*ISNULL(Package.Coef, 1) = " +
                        "(CASE WHEN Supply.$АдресПоступление.Количество <= ISNULL(OrderOnClients.Balance,0) THEN Supply.$АдресПоступление.Количество ELSE ISNULL(OrderOnClients.Balance,0) END) " +
                            "THEN round( " +
                        "(CASE WHEN Supply.$АдресПоступление.Количество <= ISNULL(OrderOnClients.Balance,0) THEN Supply.$АдресПоступление.Количество ELSE ISNULL(OrderOnClients.Balance,0) END) " +
                        "/ISNULL(Package.Coef, 1), 0) " +
                            "ELSE (CASE WHEN Supply.$АдресПоступление.Количество <= ISNULL(OrderOnClients.Balance,0) THEN Supply.$АдресПоступление.Количество ELSE ISNULL(OrderOnClients.Balance,0) END) END as CountPackage , " +
                        "(CASE WHEN Supply.$АдресПоступление.Количество <= ISNULL(OrderOnClients.Balance,0) THEN Supply.$АдресПоступление.Количество ELSE ISNULL(OrderOnClients.Balance,0) END) as Count," +
                        "Supply.$АдресПоступление.ЕдиницаШК as Unit," +
                        "Supply.$АдресПоступление.КоличествоЭтикеток as LabelCount," +
                        "Supply.$АдресПоступление.НомерСтрокиДока as Number," +
                        "Supply.$АдресПоступление.ГруппаСезона as SeasonGroup," +
                        "SypplyHeader.$АдресПоступление.ДальнийСклад as FlagFarWarehouse," +
                        "Supply.LineNO_ as LineNO_, " +
                        "isnull(GS.$Спр.ТоварныеСекции.РасчетныйРХ , 0) StoregeSize, " +
                        "OrderOnClients.ClientID as ClientID, " +
                        "OrderOnClients.ClientName as ClientName, " +
                        "OrderOnClients.OrderID as OrderID, " +
                        "OrderOnClients.OrderName as OrderName, " +
                        "OrderOnClients.AddressID as AddressID, " +
                        "OrderOnClients.AddressName as AddressName, " +
                        "OrderOnClients.PalletID as PalletID , " +
                        "OrderOnClients.PalletName as PalletName , " +
                        "isNull(OrderOnClients.BoxCount,0) as BoxCount, " +
                        "Supply.$АдресПоступление.Количество as CountAll " +
                     "FROM DT$АдресПоступление as Supply (nolock) " +
                        "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                            "ON Goods.ID = Supply.$АдресПоступление.Товар " +
                        "LEFT JOIN DH$АдресПоступление as SypplyHeader (nolock) " +
                            "ON SypplyHeader.iddoc = Supply.iddoc " +
                        "LEFT JOIN _1sjourn as Journ (nolock) " +
                            "ON Journ.iddoc = Right(SypplyHeader.$АдресПоступление.ДокументОснование , 9) " +
                        "LEFT JOIN ( " +
                                "SELECT " +
                                    "Units.parentext as ItemID, " +
                                    "min(Units.$Спр.ЕдиницыШК.Коэффициент ) as Coef " +
                                "FROM " +
                                    "$Спр.ЕдиницыШК as Units (nolock) " +
                                "WHERE " +
                                    "Units.$Спр.ЕдиницыШК.ОКЕИ = :OKEIPackage " +
                                    "and Units.ismark = 0 " +
                                    "and not Units.$Спр.ЕдиницыШК.Коэффициент = 0 " +
                                "GROUP BY " +
                                    "Units.parentext " +
                                ") as Package " +
                            "ON Package.ItemID = Goods.ID " +
                        "LEFT JOIN $Спр.ТоварныеСекции as GS (nolock) " +
                            "on GS.parentext = goods.id and gs.$Спр.ТоварныеСекции.Склад = :Warehouse " +
                            "LEFT JOIN ( " +
                            "SELECT " +
                                "$Рег.ЗаказыНаКлиентов.Клиент as ClientID , " +
                                "min(Clients.Descr) as ClientName , " +
                                "$Рег.ЗаказыНаКлиентов.Товар as Item , " +
                                "$Рег.ЗаказыНаКлиентов.Склад as Warehouse , " +
                                "sum(OrderOnClientItem.$ЗаказНаКлиента.Принято ) as Balance , " +
                                "min(Journ.IDDOC) as OrderID , " +
                                "min(right(Journ.docno,5)) as OrderName , " +
                                "min(Sections.ID ) as AddressID , " +
                                "min($ЗаказНаКлиента.КолМест ) as BoxCount , " +
                                "min(Pallets.ID ) as PalletID , " +
                                "min(SUBSTRING(Pallets.$Спр.ПеремещенияПаллет.ШКПаллеты ,9,4) ) as PalletName , " +
                                "min(Sections.Descr ) as AddressName , " +
                                "min(OrderOnClient.$ЗаказНаКлиента.ДокументОснование ) as OrderDocOsn " +
                            "FROM " +
                                "RG$Рег.ЗаказыНаКлиентов as RegOrderOnClient (nolock) " +
                                "LEFT JOIN $Спр.Клиенты as Clients (nolock) " +
                                    "on Clients.ID = $Рег.ЗаказыНаКлиентов.Клиент " +
                                "LEFT JOIN DH$ЗаказНаКлиента as OrderOnClient (nolock) " +
                                       "on OrderOnClient.IDDOC = Right($Рег.ЗаказыНаКлиентов.Док , 9) " +
                                "LEFT JOIN DT$ЗаказНаКлиента as OrderOnClientItem (nolock) " +
                                       "on OrderOnClient.IDDOC = OrderOnClientItem.IDDOC " +
                                       "and OrderOnClientItem.$ЗаказНаКлиента.Товар = $Рег.ЗаказыНаКлиентов.Товар " +
                                "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                                    "on Sections.ID = OrderOnClient.$ЗаказНаКлиента.Адрес " +
                                "LEFT JOIN $Спр.ПеремещенияПаллет as Pallets (nolock) " +
                                    "on Pallets.ID = OrderOnClient.$ЗаказНаКлиента.Паллета " +
                                "LEFT JOIN _1sjourn as Journ (nolock) " +
                                    "on Journ.IDDOC = Right($Рег.ЗаказыНаКлиентов.Док , 9) " +
                            "WHERE " +
                                "RegOrderOnClient.period = @curdate " +
                            "GROUP BY " +
                                "$Рег.ЗаказыНаКлиентов.Клиент , " +
                                "$Рег.ЗаказыНаКлиентов.Товар , " +
                                "$Рег.ЗаказыНаКлиентов.Склад , " +
                                "$Рег.ЗаказыНаКлиентов.Док " +
                             "HAVING " +
                                "sum(OrderOnClientItem.$ЗаказНаКлиента.Принято ) > 0 " +
                                "and sum(RegOrderOnClient.$Рег.ЗаказыНаКлиентов.Количество ) > 0 " +
                                ") as OrderOnClients " +
                            "on Goods.ID = OrderOnClients.Item " +
                            "and SypplyHeader.$АдресПоступление.Склад = OrderOnClients.Warehouse " +
                            "and SypplyHeader.$АдресПоступление.ДокументОснование = OrderOnClients.OrderDocOsn " +  
                    "WHERE Supply.IDDOC in (:Docs) " +
                        "and Supply.$АдресПоступление.Состояние0 = 1 " +
                        "and Supply.$АдресПоступление.ФлагПечати = 1 " +
                        "and Supply.$АдресПоступление.Сотрудник0 = :Employer " +
                    "ORDER BY Journ.docno, OrderOnClients.OrderID, Supply.$АдресПоступление.Дата0 , Supply.$АдресПоступление.Время0 ";
                TextQuery = TextQuery.Replace(":Docs", Docs);
                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "OKEIPackage", OKEIPackage);
                QuerySetParam(ref TextQuery, "Warehouse", Const.MainWarehouse);
                DT.Clear();
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                DT.Columns.Add("CoefView", Type.GetType("System.String"));
                DT.Columns.Add("CloseOrder", Type.GetType("System.String"));
                for (int row = 0; row < DT.Rows.Count; row++)
                {
                    DT.Rows[row]["CoefView"] = ((int)(decimal)DT.Rows[row]["Coef"] == 1 ? "??  " : "")
                                                            + ((int)(decimal)DT.Rows[row]["Coef"]).ToString();
                    DataRow[] tmpDROrderID = FNotAcceptedItems.Select("OrderID = '" + DT.Rows[row]["OrderID"] + "'");
                    if (tmpDROrderID.Length == 0)
                    {
                        DT.Rows[row]["CloseOrder"] = "X";
                    }
                    else
                    {
                        DT.Rows[row]["CloseOrder"] = "";
                    }
                }
                FAcceptedItems.Merge(DT, false, MissingSchemaAction.Ignore);

                //Расчитываем строчечки
                DR = FConsignment.Select();
                foreach (DataRow dr in DR)
                {
                    DataRow[] tmpDR = FNotAcceptedItems.Select("IDDOC = '" + dr["ACID"].ToString() + "'");
                    dr["CountNotAcceptRow"] = tmpDR.Length;
                }
            }

            FCurrentMode = Mode.AcceptanceCross;
            return true;
        } // ToModeAcceptanceCross
        
    }
}
