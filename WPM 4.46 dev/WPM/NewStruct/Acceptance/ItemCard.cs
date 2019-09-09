using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class ItemCard : ABaseStandart
    {
        /*struct StructItem
        {
            public int Details;
            public int NowDetails;

            public string Name;
            public string Acticle;
            public string InvCode;
            public string ID;
            public int Count;
            public int AcceptCount; //Сколько в итоге приняли (выставили, короче после редактирования)
            public decimal Price;
            public bool Acceptance;
            public int MinParty;
            public Mode ToMode; //В какой режим возвращаться из карточки
            public string AdressMain;
            public string AdressBuffer;
            public int BalanceMain;
            public int BalanceBuffer;
            //Дополнительная фигня
            public int CurrentBalance;
            public string CurrentWarehouseName;
            public bool GenerateBarcode;    //Генерировать или нет штрихкод для базовой единицы (нужен для развязки кривой структуры программы)
            public bool OnShelf;
            public string BaseUnitID;
            public bool IsRepeat;
            public string BindingAdress;
            public string BindingAdressName;
            public bool BindingAdressFlag;
            //Еще одна дополнительная фигня
            public int SeasonGroup; //Группа сезона
            public int FlagFarWarehouse;    //Дальний склад
            public int StoregeSize;         //Размер хранения
        }
        */
        public StructItem AcceptedItem;

        public ABaseMode OldMode;   //режим из которого пришли
        public RefItem Item;        //товар
        public DataTable AdressConditionItem;   //инфа по адресам, состояниям и количеству товара в карточке
        private DataTable FUnits;
        private string BufferWarehouse;         //Склад буффер - для него будут считатьс остатки в карточке товара
        private string ComingWarehouse;         //Склад прихода - для определения возможности редактирования карточки товара
        public bool OnShelf;
        public RefWarehouse InventoryWarehouse;   //Склад по которому пробивается недостача
        public StrictDoc ATDoc;

        public ItemCard(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.ItemCard;
            OldMode = CallObj;
            Item = (CallObj as RefillLayout).Item;
            if (!Item.Selected)
            {
                //нет товара, значит достанем последний товар
                DataTable MyRemainItems = (CallObj as RefillLayout).RemainItems;
                Item.FoundID(MyRemainItems.Rows[0]["Item"].ToString());       

            }

        } // ItemCard (constructor)
        protected override void ReactionKeyDo(Keys key)
        {
            if (key == Keys.D0 || key == Keys.Escape || key == Keys.Left)
            {
                Cancel();
            }           
        } // ReactionKeyDo()
        internal override ABaseMode Init()
        {
            QuestItem();

            return Positive("Карточка товара");
        } // Init()
        internal override ABaseMode Cancel()
        {
            SS.OnReport(new ReportEventArgs("Отмена..."));
            return JumpTo(OldMode);
        } // Cancel

        //Подсасываем всю необходимую инфу для отображения товара (скопипизжено ToModeAcceptedItem())
        protected bool QuestItem()
        { 
            //private bool ToModeAcceptedItem(string ItemID, string IDDoc, Mode ToMode, int InPartyCount, bool OnShelf)
            
            //DataRow[] DR;
            AdressConditionItem = null;

            string TextQuery = "SELECT VALUE as val FROM _1sconst (nolock) WHERE ID = $Константа.ОснЦентрСклад ";
            DataTable DT;
            if (!SS.ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            BufferWarehouse = DT.Rows[0]["val"].ToString();

            //Если был дисконнект, то это проявиться после нижеследующего запроса
            //и дальше будет, не приемка, а редактирование карточки, для этого запрос и помещен в начало
            if (!LoadUnits(Item.InvCode))
            {
                return false;
            }

            //FExcStr - несет смысл
            AcceptedItem = new StructItem();
            AcceptedItem.GenerateBarcode = false;

            #region Вроде не нужно
            /* if (NewBarcodes == null)
            {
                NewBarcodes = new List<string>();
            }
            else
            {
                NewBarcodes.Clear();
            }*/

            //Определяем имеется ли данный товар в списке принимаемых
            //CurrentRowAcceptedItem = null;
            //int AllCount = 0;
           

           /* if (ToMode == Mode.Acceptance)
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
            }*/



            //БЛОКИРУЕМ ТОВАР
            //if (!LockItem(ItemID))
            //{
            //    return false;
            //}
           #endregion

           #region Запрос ОПРЕДЕЛЯЕМ ОСТАТКИ И АДРЕСА
           TextQuery =
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
            SQL1S.QuerySetParam(ref TextQuery, "DateNow", DateTime.Now);
            SQL1S.QuerySetParam(ref TextQuery, "Item", Item.ID);
            SQL1S.QuerySetParam(ref TextQuery, "BufferWarehouse", BufferWarehouse);
            SQL1S.QuerySetParam(ref TextQuery, "MainWarehouse", Const.MainWarehouse);
            DT.Clear();
            if (!SS.ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            #endregion

            AcceptedItem.BalanceMain    = (int)DT.Rows[0]["BalanceMain"];
            AcceptedItem.BalanceBuffer  = (int)DT.Rows[0]["BalanceBuffer"];
            AcceptedItem.AdressMain     = DT.Rows[0]["AdressMain"].ToString();
            AcceptedItem.AdressBuffer   = DT.Rows[0]["AdressBuffer"].ToString();
            AcceptedItem.IsRepeat       = false;

            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();

            #region Запрос Подсосем остатки в разрезе адресов и состояний
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
                    "ORDER BY Adress, Condition" ;
            SQL1S.QuerySetParam(ref TextQuery, "DateNow",     DateTime.Now);
            SQL1S.QuerySetParam(ref TextQuery, "ItemID", Item.ID);
            SQL1S.QuerySetParam(ref TextQuery, "Warehouse", Const.MainWarehouse);
            if (!SS.ExecuteWithRead(TextQuery, out AdressConditionItem))
            {
                AdressConditionItem = null;
            }
            #endregion

            #region Запрос Я не знаю что это...
            TextQuery = 
                "SELECT " + 
                    "Goods.Descr as ItemName," + 
                    "Goods.$Спр.Товары.ИнвКод as InvCode, " + 
                    "Goods.$Спр.Товары.Артикул as Article, " +
                    "Goods.$Спр.Товары.КоличествоДеталей as Details, " + 
                    "Goods.$Спр.Товары.БазоваяЕдиницаШК as BaseUnitID, " + 
                    "Goods.$Спр.Товары.МинПартия as MinParty, " +
                    "Goods.$Спр.Товары.Опт_Цена as Price,  " +
                    //"Goods.$Спр.Товары." + (ToMode == Mode.Acceptance ? "Прих_Цена" : "Опт_Цена") + " as Price,  " +
                    //"isnull(RefSections.$Спр.ТоварныеСекции.РазмерХранения , 0) as StoregeSize " +
                    "isnull(RefSections.$Спр.ТоварныеСекции.РасчетныйРХ , 0) as StoregeSize " + 
                "FROM $Спр.Товары as Goods (nolock) " + 
                    "left join $Спр.ТоварныеСекции as RefSections (nolock) " +
                        "on RefSections.parentext = Goods.id and RefSections.$Спр.ТоварныеСекции.Склад = :warehouse " +
                "WHERE Goods.id = :Item ";
            SQL1S.QuerySetParam(ref TextQuery, "Item", Item.ID);
            SQL1S.QuerySetParam(ref TextQuery, "warehouse", Const.MainWarehouse);
            DT.Clear();
            if (!SS.ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            #endregion

            AcceptedItem.Details = (int)(decimal)DT.Rows[0]["Details"];
            AcceptedItem.NowDetails = AcceptedItem.Details;

            AcceptedItem.ID         = Item.InvCode;
            AcceptedItem.Name       = DT.Rows[0]["ItemName"].ToString();
            AcceptedItem.InvCode    = DT.Rows[0]["InvCode"].ToString();
            AcceptedItem.Acticle    = DT.Rows[0]["Article"].ToString();
            AcceptedItem.BaseUnitID = DT.Rows[0]["BaseUnitID"].ToString();
            AcceptedItem.MinParty   = (int)(decimal)DT.Rows[0]["MinParty"];
            AcceptedItem.Count      = 0;
            AcceptedItem.Price      = (decimal)DT.Rows[0]["Price"];
            AcceptedItem.Acceptance = false;
            AcceptedItem.ToMode = OldMode.CurrentMode;
            AcceptedItem.BindingAdressFlag = false;
            AcceptedItem.StoregeSize = (int)(decimal)DT.Rows[0]["StoregeSize"];
            AcceptedItem.CurrentBalance = AcceptedItem.BalanceMain;
            
           
            #region Если это необходимо, то определяем количество товара для склада инвентаризации
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
                SQL1S.QuerySetParam(ref TextQuery, "DateNow", DateTime.Now);
                SQL1S.QuerySetParam(ref TextQuery, "ItemID", Item.ID);
                SQL1S.QuerySetParam(ref TextQuery, "Warehouse", InventoryWarehouse.ID);
                if (!SS.ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                AcceptedItem.CurrentBalance = (int)(decimal)DT.Rows[0]["Balance"];
            }
            #endregion

            #region А теперь имя склада!
            if (AcceptedItem.ToMode != Mode.Inventory)
            {
                TextQuery = 
                    "SELECT descr as Name FROM $Спр.Склады (nolock) WHERE ID = :Warehouse";
                SQL1S.QuerySetParam(ref TextQuery, "Warehouse", Const.MainWarehouse);
                if (!SS.ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                AcceptedItem.CurrentWarehouseName = DT.Rows[0]["Name"].ToString();
            }
            else
            {
                AcceptedItem.CurrentWarehouseName = InventoryWarehouse.Name;
            }
            #endregion

            #region c Mode.Transfer думаю пока не нужно
            /*if (OldMode.CurrentMode == Mode.Transfer)
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
                        SQL1S.QuerySetParam(ref TextQuery, "DateNow", DateTime.Now);
                        SQL1S.QuerySetParam(ref TextQuery, "Item", Item.ID);
                        SQL1S.QuerySetParam(ref TextQuery, "Warehouse", ATDoc.ToWarehouseID);
                        if (!SS.ExecuteWithRead(TextQuery, out DT))
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
            }*/
            #endregion

            #region Заполнение FExcStr. Для подпитки это не нужно
            /*if (AcceptedItem.ToMode == Mode.Acceptance)
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
                   // RefItem Item = new RefItem(this); (уже объялен)
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
                AcceptedItem.Count      = InPartyCount;
                AcceptedItem.OnShelf    = OnShelf;
            }*/
            #endregion

            #region Пишем в обслуживание скс. Для подпитки - не нужно
            /*
            FCurrentMode = Mode.AcceptedItem;
            //begin internal command
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(ItemID, "Спр.Товары");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "OpenItem (Открыл карточку)";
            if (!ExecCommandNoFeedback("Internal", DataMapWrite))
            {
                return false;
            }
            //end internal command
            return true;
                        
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
                DR = FUnits.Select("OKEI = '"  + OKEIPackage + "'");
                foreach(DataRow dr in DR)
                {
                    Coef = (int)dr["Coef"];
                    break;
                }
                FExcStr += " " + GetStrPackageCount(AcceptedItem.Count, Coef) + " из " + GetStrPackageCount(AllCount, Coef);
            }
            //begin internal command
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(ItemID, "Спр.Товары");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(IDDoc, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "OpenItemAccept (Открыл карточку для приемки)";
            if (!ExecCommandNoFeedback("Internal", DataMapWrite))
            {
                return false;
            }*/
            //end internal command
            #endregion

            return true;       
        }

        private bool LoadUnits(string ItemID)
        {
            //Загружает единицы товара в таблицу FUnits
            string TextQuery =
                "SELECT " +
                    "Units.id as ID, " +
                    "CAST(Units.$Спр.ЕдиницыШК.Коэффициент as int) as Coef, " +
                    "Units.$Спр.ЕдиницыШК.Штрихкод as Barcode, " +
                    "Units.$Спр.ЕдиницыШК.ОКЕИ as OKEI " +
                "FROM " +
                    "$Спр.ЕдиницыШК as Units (nolock) " +
                "WHERE " +
                    "Units.parentext = :CurrentItem and Units.ismark = 0";
            SQL1S.QuerySetParam(ref TextQuery, "CurrentItem", ItemID);
            if (!SS.ExecuteWithRead(TextQuery, out FUnits))
            {
                return false;
            }
            return true;
        }

    }
}
