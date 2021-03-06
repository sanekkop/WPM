﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Data;


namespace WPM
{

    struct StructItem
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
    struct StructItemSet
    {
        public string Name;
        public string InvCode;
        public string ID;
        public int Count;
        public string AdressID;
        public string AdressName;
        public int CurrLine;
        public int CountFact;
        public decimal Price;
        public int Balance;
        public int Details;
        public int OKEI2Count;
        public string OKEI2;
        public int OKEI2Coef;
    }
    struct StrictDoc
    {
        public string View;
        public string ID;
        public string FromWarehouseID;
        public string FromWarehouseName;
        public string ToWarehouseID;
        public string ToWarehouseName;
        public bool ToWarehouseSingleAdressMode;
        public string FoundDoc; //документ основание
        public int Rows;
        public decimal Sum;
        public int Boxes;
        public int AllBoxes;
        public string Client;
        public int SelfRemovel;
        public string AdressCollect;
        public string Sector;
        public int MaxStub;
        public string NumberBill;
        public int NumberCC;
        public string MainSectorName;
        public string SetterName;
        public bool Special;
        public bool IsFirstOrder;
        public string Box;
        public string BoxID;
    }

    /// <summary>
    /// Какой-то непонятный, страшный монстр...
    /// </summary>
    partial class Model : SQL1S
    {
        #region variables
        public bool Connect;
        public bool colorSwitcher; //Это такая поеботина для переключения цветов при погрузки
        private const string FOKEIUnit      = "     1   ";
        private const string FOKEIPack      = "     2   ";
        private const string FOKEIPackage   = "     E   ";
        private const string FOKEIKit       = "     A   ";
        private const string FOKEIOthers    = "     0   ";
        public string OKEIUnit { get { return FOKEIUnit; } }
        public string OKEIPack { get { return FOKEIPack; } }
        public string OKEIPackage { get { return FOKEIPackage; } }
        public string OKEIKit { get { return FOKEIKit; } }
        public string OKEIOthers { get { return FOKEIOthers; } }
        private int ResponceTime;                           //время ожидания отклика 1С
        
        private string BufferWarehouse; //Склад буффер - для него будут считатьс остатки в карточке товара
        private string ComingWarehouse; //Склад прихода - для определения возможности редактирования карточки товара
        private string BarcodePrefix;   //Префикс собственно генерируемых штрихкодов
        public int CurrentLine;         //
        public int OkRowIndex;
        public RefWarehouse InventoryWarehouse;        //Склад по которому пробивается недостача
        public RefWarehouse WarehouseForAddressItem;   //Склад по которому выводятся остатки и адреса
        private int FFlagBarcode;         
        public int FlagBarcode { get { return FFlagBarcode; } }// Определяет насколько удачная идентийикация товара для режима AcceptedItem
        private List<string> NewBarcodes;   //Список ШК введенных за сессию приемки

        private RefEmployer FEmployer;
        public RefEmployer Employer
        {
            get { return FEmployer; }
            set { FEmployer = value; }  //Временно (а то новая архитектура не взлетает)
        }

        private Mode FCurrentMode;
        public Mode CurrentMode
        {
            get
            {
                return FCurrentMode;
            }
            set
            {
                FCurrentMode = value;
            }
        }
        
        private DataTable FConsignment;
        private RefPrinter FPrinter;
        private DataTable FPallets;
        
        public string FPalletID;
        public string FOrderID;
        public string FBarcodePallet;
        public RefPalleteMove PalletAcceptedItem;
        public RefPalleteMove CurrentPalletAcceptedItem;
        public RefPalleteMove PalletEmptyOrder;
        private RefSection FAdressZoneTransfer;
        private string FZoneName;
        private string FZoneID;
        private DataTable FNotAcceptedItems;
        private DataTable FAcceptedItems;
        private DataTable FUnits;
        private DataRow CurrentRowAcceptedItem;
        private string FVers;
        private DataTable FWarehousesFrom;
        private DataTable FWarehousesTo;
        private DataTable FTransferItems;
        private DataTable FTransferReadyItems;  //Товар в тележке у разносчика или товар в тележке у приемщика образцов
        public DataTable AddressCardItems;  //Товар в адресе на остатках
        private DataTable FInventory;
        public DataTable ForPrint;   //Для печати на принтер
        private DataTable FSampleItems;
        private Mode InsideCurrentMode
        {
            get { return FCurrentMode; }
        }

        private string SampleDoc;   //Адрес перемещение по выкладке образцов
        public StrictDoc ATDoc;

        public RefPrinter Printer
        {
            get { return FPrinter; }
        }
        public RefSection ZoneAdressTransfer
        {
            get { return FAdressZoneTransfer; }
        }
        public string ZoneName
        {
            get { return FZoneName; }
        }
        public DataTable Consignment
        {
            get { return FConsignment; }
        }
        public DataTable NotAcceptedItems
        {
            get { return FNotAcceptedItems; }
        }
        public DataTable AcceptedItems
        {
            get { return FAcceptedItems; }
        }
        public DataTable Pallets
        {
            get { return FPallets; }
        }
        public DataTable Units
        {
            get { return FUnits; }
        }
        public string Vers
        {
            get { return FVers; }
        }
        public StructItem AcceptedItem;
        public DataTable WarehousesFrom
        {
            get { return FWarehousesFrom; }
        }
        public DataTable WarehousesTo
        {
            get { return FWarehousesTo; }
        }
        public DataTable TransferItems
        {
            get { return FTransferItems; }
        }
        public DataTable TransferReadyItems
        {
            get { return FTransferReadyItems; }
        }
        public DataTable Inventory
        {
            get { return FInventory; }
        }
        public DataTable SampleItems
        {
            get {return FSampleItems; }
        }
        public DataTable Sections;
        public DataTable AdressConditionItem;    //инфа по адресам, состояниям и количеству товара в карточке
        public DataTable GoodsCC;   //Control collect
        private string ControlCC;   //сборочный лист контролируемый
        public string LabelControlCC;   //Представление сборочного листа
        public DataTable CCRP;  //Таблица сборочного листа
        public DataTable DownSituation; //Таблица показывает ситуацию на секторах
        public DataTable ATTable;   //для табл. части АП

        public DataTable SampleTransfers;

        public string SampleAdress;

        public string DeviceName;

        public DataTable CCListSample;

        public RefEmployer Placer;
        public StrictDoc WayBill;

        public DataTable WayBillDT;
        public StructItemSet CCItem;
        public string PreviousAction;
        public StrictDoc DocDown;
        public StrictDoc BadDoc;
        public string ScaningBox;
        public string ScaningBoxIddoc;
        public string NeedAdressComplete;
        public string LastGoodAdress;
        public string NameLastGoodAdress;
        public bool FlagPrintPallete;
        public string PicturePath;

        public ConstantsDepot Const;
        #endregion
        
        //begin test
        public delegate void ChangeModeEventHandler(object sender, ChangeModeEventArgs e);
        public event ChangeModeEventHandler ChangeMode;
        public delegate void ScanBoxEventHandler(object sender, EventArgs e);
        public event ScanBoxEventHandler ScanBox;
        public delegate void ReportEventHandler(object sender, ReportEventArgs e);
        public event ReportEventHandler Report;
        public delegate void UpdaterEventHandler(object sender, UpdateEventArgs e);
        public event UpdaterEventHandler Update;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnUpdate(UpdateEventArgs e)
        {
            if (Update != null)
            {
                Update(this, e);
            }
        } // OnUpdate
        /// <summary>
        /// Even report
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnReport(ReportEventArgs e)
        {
            if (Report != null)
            {
                Report(this, e);
            }
        } // OnReport
        /// <summary>
        /// Event change mode
        /// </summary>
        /// <param name="e"></param>
        public virtual void OnChangeMode(ChangeModeEventArgs e)
        {
            FCurrentMode = e.ToMode;
            if (ChangeMode != null)
            {
                ChangeMode(this, e);
            }
        } // OnChangeMode
        /// <summary>
        /// Even change mode
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnScanBox(EventArgs e)
        {
            if (ScanBox != null)
            {
                ScanBox(this, e);
            }
        } // OnScanBox

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ServerName">server name</param>
        /// <param name="DBName">date base name</param>
        /// <param name="Vers">current version</param>
        public Model(string ServerName, string DBName, string Vers) : base (ServerName, DBName)
        {
            //Список с новой структурой
            FillNewStructList();

            ResponceTime = 30;
            FCurrentMode = Mode.None;
            FVers = Vers;
            Connect = false;
        }
        #region init and IBS
        public bool Initialize()
        {
            if (!LoadAliases())
            {
                return false;
            }
            if (!SynhDateTime())
            {
                return false;
            }
            Connect = true;

            //а тут подсасываем пути картинок
            string TextQuery = "SELECT VALUE as val FROM _1sconst (nolock) WHERE ID = $Константа.ПутьКартинокТерминала ";
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            PicturePath = DT.Rows[0]["val"].ToString();

            //а тут подсасываем констатну склада-буффера
            TextQuery = "SELECT VALUE as val FROM _1sconst (nolock) WHERE ID = $Константа.ОснЦентрСклад ";
            DT.Clear();
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            BufferWarehouse = DT.Rows[0]["val"].ToString();

            //а тут подсасываем констатну склада-прихода
            TextQuery = "SELECT VALUE as val FROM _1sconst (nolock) WHERE ID = $Константа.СкладПрихода ";
            DT.Clear();
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            ComingWarehouse = DT.Rows[0]["val"].ToString();

            //Префикс ШК
            TextQuery = "SELECT VALUE as val FROM _1sconst (nolock) WHERE ID = $Константа.ПрефиксШК ";
            DT.Clear();
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            BarcodePrefix = DT.Rows[0]["val"].ToString().Trim();

            Const = new ConstantsDepot(this);
            
            //Адрес образцов
            TextQuery = "SELECT $Спр.Склады.АдресОбразцов as val FROM $Спр.Склады (nolock) WHERE $Спр.Склады .id = :MainWarehouse";
            QuerySetParam(ref TextQuery, "MainWarehouse", Const.MainWarehouse);
            DT.Clear();
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            SampleAdress = DT.Rows[0]["val"].ToString();

            FPrinter = new RefPrinter(this);       //где нибудь да пригодится

            return Init(new Waiting(this, null));
        }

        //test debug
        public void xyu()
        {
            LabelControlCC = Employer.GetAttribute("ТабельныйНомер").ToString();
            LabelControlCC = LabelControlCC + " " + Employer.GetAttribute("ФлагКладовщика").ToString();
            return;
            //Model tmpSS = new Model(ServerName, DBName, Vers);
            //string TextQuery = "select mess as mess from tt";
            //DataTable DT;
            //if (!tmpSS.ExecuteWithRead(TextQuery, out DT))
            //{
            //    tmpSS.MyConnection.Close();
            //    return;
            //}
            //tmpSS.MyConnection.Close();
            //LabelControlCC = DT.Rows[0]["mess"].ToString();
        }
        //end test debug

        public bool IBS_Inicialization(RefEmployer Emp)
        {
            string TextQuery = 
                "set nocount on; " +
			    "declare @id bigint; " +
			    "exec IBS_Inicialize_with_DeviceID_new :Employer, :HostName, :DeviceID, @id output; " +
			    "select @id as ID;";
            QuerySetParam(ref TextQuery, "Employer", Emp.ID);
            QuerySetParam(ref TextQuery, "HostName", DeviceID.GetDeviceName());
            QuerySetParam(ref TextQuery, "DeviceID", DeviceID.GetDeviceID());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if ((long)DT.Rows[0]["ID"] > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public void IBS_Finalization()
        {
            ExecuteWithoutRead("exec IBS_Finalize");
        }
        private bool IBS_AbsoluteLock(string BlockText)
        {
            string TextQuery =
                "set nocount on; " +
                "exec IBS_AbsoluteLock :BlockText;";
            QuerySetParam(ref TextQuery, "BlockText", BlockText);
            return ExecuteWithoutRead(TextQuery);
        }
        private bool IBS_Lock(string BlockText)
        {
            string TextQuery = 
                "set nocount on; " +
			    "declare @result int; " +
			    "exec IBS_Lock :BlockText, @result output; " +
			    "select @result as result;";
            QuerySetParam(ref TextQuery, "BlockText", BlockText);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if ((int)DT.Rows[0]["result"] > 0)
            {
                return true;
            }
            else
            {
                FExcStr = "Объект заблокирован!";   //Ответ по умолчанию
                //Покажем кто заблокировал
                TextQuery = 
                    "SELECT " +
	                    "rtrim(Collation.HostName) as HostName, " +
	                    "rtrim(Collation.UserName) as UserName, " +
                        "convert(char(8), Block.date_time, 4) as Date, " +
	                    "substring(convert(char, Block.date_time, 21), 12, 8) as Time " +
	                "FROM " +
	                    "IBS_Block as Block " +
	                    "INNER JOIN IBS_Collation as Collation " +
	                        "ON Collation.ID = Block.ProcessID " + 
	                    "WHERE " +
	                        "left(Block.BlockText, len(:BlockText)) = :BlockText ";
                QuerySetParam(ref TextQuery, "BlockText", BlockText);
                if (ExecuteWithRead(TextQuery, out DT))
                {
                    if (DT.Rows.Count > 0)
                    {
                        FExcStr =
                            "Объект заблокирован! " + DT.Rows[0]["UserName"] + ", " + DT.Rows[0]["HostName"] +
                            ", в " + DT.Rows[0]["Time"] + " (" + DT.Rows[0]["Date"] + ")";                    }
                }
                return false;
            }
        }
        private bool IBS_Lockuot(string BlockText)
        {
            string TextQuery = "exec IBS_Lockout :BlockText";
            QuerySetParam(ref TextQuery, "BlockText", BlockText);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return true;
        }

        //Locks
        private bool LockItem(string ItemID)
        {
            string BlockText1 = "int_ref_Товары_" + ItemID;
            string BlockText2 = BlockText1 + "_unit";
            bool Lock1 = IBS_Lock(BlockText1);
            bool Lock2 = IBS_Lock(BlockText2);
            //Только при отсутствии любой из этих блокировок
            if (Lock1 && Lock2)
            {
                //Обе могут, ништяк! Снимем лишний и довольные выходим!
                IBS_Lockuot(BlockText1);
                return true;
            }
            else
            {
                //Не могут обе, нужно снять свои блокировки
                if (Lock1)
                {
                    IBS_Lockuot(BlockText1);
                }
                if (Lock2)
                {
                    IBS_Lockuot(BlockText2);
                }
                return false;
            }
        }
        private bool LockDocAccept(string IDDoc)
        {
            string BlockText1 = "int_doc_" + IDDoc;
            string BlockText2 = BlockText1 + "_accept";
            bool Lock1 = IBS_Lock(BlockText1);
            bool Lock2 = IBS_AbsoluteLock(BlockText2);
            if (Lock1 && Lock2)
            {
                //Обе могут, ништяк! Снимем лишний и довольные выходим!
                IBS_Lockuot(BlockText1);
                return true;
            }
            else
            {
                //Не могут обе, нужно снять свои блокировки
                if (Lock1)
                {
                    IBS_Lockuot(BlockText1);
                }
                if (Lock2)
                {
                    IBS_Lockuot(BlockText2);
                }
                return false;
            }
        }
        private bool LockoutDocAccept(string IDDoc)
        {
            return IBS_Lockuot("int_doc_" + IDDoc + "_accept");
        }
        public bool LockDoc(string IDDoc)
        {
            return IBS_Lock("int_doc_" + IDDoc);
        }
        public bool LockoutDoc(string IDDoc)
        {
            return IBS_Lockuot("int_doc_" + IDDoc);
        }
        
        protected override void IsOpened()
        {
            bool OldPermission = permission;
            permission = true;  //По умолчанию все разрешено!
            if (CurrentMode != Mode.None && CurrentMode != Mode.Waiting)
            {
                //В режимах, в которых логин уже произошел нужно восстановить ИБС инициализацию
                if (!IBS_Inicialization(Employer))
                {
                    permission = false;
                    return;
                }
            }
            //Восстанавливаем блокировки приходных накладных в режиме приемки
            bool Acceptance = false;
            if (CurrentMode == Mode.AcceptedItem)
            {
                if (AcceptedItem.ToMode == Mode.Acceptance)
                {
                    Acceptance = true;
                }
            }
            if (CurrentMode == Mode.Acceptance)
            {
                Acceptance = true;
            }
            if (Acceptance)
            {
                List<string> DieACID = new List<string>();
                foreach(DataRow dr in FConsignment.Rows)
                {
                    if (!LockDocAccept(dr["ACID"].ToString()))
                    {
                        //Неудачников - выкидываеем из списка
                        DieACID.Add(dr["ACID"].ToString());
                    }
                }
                foreach(string str in DieACID)
                {
                    DeleteConsignment(str);
                }
                if (DieACID.Count > 0)
                {
                    FExcStr = "Из списка принимаемых удалено " + DieACID.Count.ToString() + ", т.к. они заблокированы!";
                }
            }

            //Если обрыв произошел в карточке товара, то не даем ничего делать, кроме выхода из карточки
            if (CurrentMode == Mode.AcceptedItem)
            {
                permission = false;
            }
        }
        #endregion

        #region to mode, quit mode...

        private Mode CheckOrder(Mode NowMode, RefEmployer Who)
        {
            Mode result = NowMode;
            //ЗАДАНИЕ СПУСКА
            if (Who.CanDown)
            {
                string TextQuery = 
                    "select top 1 " + 
                            "Ref.id " +
                        "from $Спр.МестаПогрузки as Ref (nolock) " +
                            "where " + 
                                "Ref.$Спр.МестаПогрузки.Сотрудник4 = :Employer " + 
                                "and Ref.ismark = 0 " + 
                                "and not Ref.$Спр.МестаПогрузки.Дата40 = :EmptyDate " + 
                                "and not Ref.$Спр.МестаПогрузки.Адрес3 = :EmptyID " + 
                                "and Ref.$Спр.МестаПогрузки.Дата5 = :EmptyDate ";
                QuerySetParam(ref TextQuery, "EmptyID",     GetVoidID());
                QuerySetParam(ref TextQuery, "Employer",    Who.ID);
                QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
                DataTable DT;
                if (ExecuteWithRead(TextQuery, out DT))
                {
                    if (DT.Rows.Count > 0)
                    {
                        return Mode.Down;
                    }
                }
            }
            //ЗАДАНИЕ СВОБОДНОГО СПУСКА/КОМПЛЕКТАЦИИ
            if (Who.CanComplectation)
            {
                string TextQuery = 
                    "select top 1 " + 
                            "Ref.id " +
                        "from $Спр.МестаПогрузки as Ref (nolock) " +
                            "where " + 
                                "Ref.ismark = 0 " + 
                                "and (" +
                                        "Ref.$Спр.МестаПогрузки.Сотрудник4 = :Employer " +
                                        "and Ref.$Спр.МестаПогрузки.Дата5 = :EmptyDate " + 
                                        "and Ref.$Спр.МестаПогрузки.Адрес3 = :EmptyID " + 
                                        "and not Ref.$Спр.МестаПогрузки.Дата40 = :EmptyDate" +
                                    " or " +
                                        "Ref.$Спр.МестаПогрузки.Сотрудник8 = :Employer " +
                                        "and Ref.$Спр.МестаПогрузки.Дата9 = :EmptyDate " + 
                                        "and Ref.$Спр.МестаПогрузки.Адрес7 = :EmptyID " + 
                                        "and not Ref.$Спр.МестаПогрузки.Дата80 = :EmptyDate)";
                QuerySetParam(ref TextQuery, "Employer",    Who.ID);
                QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
                QuerySetParam(ref TextQuery, "EmptyID",     GetVoidID());
                DataTable DT;
                if (ExecuteWithRead(TextQuery, out DT))
                {
                    if (DT.Rows.Count > 0)
                    {
                        return Mode.FreeDownComplete;
                    }
                }
            }
            //ЗАДАНИЕ ОТБОРА КОМПЛЕКТАЦИИ
            if (Who.CanComplectation)
            {
                string TextQuery = 
                    "select top 1 " + 
                            "Ref.id " +
                        "from $Спр.МестаПогрузки as Ref (nolock) " +
                            "where " + 
                                "Ref.ismark = 0 " + 
                                "and Ref.$Спр.МестаПогрузки.Сотрудник8 = :Employer " +
                                "and Ref.$Спр.МестаПогрузки.Дата9 = :EmptyDate " + 
                                "and not Ref.$Спр.МестаПогрузки.Адрес7 = :EmptyID " + 
                                "and not Ref.$Спр.МестаПогрузки.Дата80 = :EmptyDate";
                QuerySetParam(ref TextQuery, "Employer",    Who.ID);
                QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
                QuerySetParam(ref TextQuery, "EmptyID",     GetVoidID());
                DataTable DT;
                if (ExecuteWithRead(TextQuery, out DT))
                {
                    if (DT.Rows.Count > 0)
                    {
                        return Mode.NewComplectation;
                    }
                }
            }

            return result;
        }

        
        public void AddToForPrint(DataRow currRow)
        {
            if (currRow != null)
            {
                DataRow[] tmpDR = ForPrint.Select("ID = '" + currRow["ID"].ToString() + "'");
                if (tmpDR.Length == 0)
                {
                    //DataRow dr = ForPrint.NewRow();
                    ForPrint.ImportRow(currRow);
                    FExcStr = currRow["InvCode"].ToString().Trim() + " добавлен в список для печати добавл";
                }
            }
            for (int row=0; row<ForPrint.Rows.Count; row++)
            {
                ForPrint.Rows[row]["Number"] = row+1;
            }
        } //AddToForPrint
        public bool PrintSampleLables()
        {
            if (ForPrint.Rows.Count == 0)
            {
                FExcStr = "Не выбрано позиций для печати!";
                return false;
            }
            //Формируем строку с ид-шниками АдресовПоступления
            string strACID = "";
            for (int i = 0; i < ForPrint.Rows.Count; i++)
            {
                strACID += ForPrint.Rows[i]["ID"].ToString().Trim() + ",";
            }
            strACID = strACID.Substring(0, strACID.Length - 1);

            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = strACID;
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(Printer.ID, "Спр.Принтеры");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");

            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("PrintSample", DataMapWrite, FieldList, out DataMapRead ))
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
            FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
            //ForPrint.Rows.Clear();    //А надо?
            return true;
        } // PrintSampleLables

       
        
        public bool RefreshRoute()
        {
            if (CCRP != null)
            {
                CCRP.Clear();
            }
            string TextQuery =
                "select " +
                    "right(min(journForBill.docno), 5) as Bill, " +
                    "rtrim(min(isnull(Sections.descr, 'Пу'))) + '-' + cast(min(DocCC.$КонтрольНабора.НомерЛиста ) as char) as CC, " +
                    "max(AllTab.CountAllBox) as Boxes, " +
                    "rtrim(max(RefAdress9.descr)) as Adress, " +
                    "max(Gate.descr) as Gate " +
                "from $Спр.МестаПогрузки as Ref (nolock) " +
                    "inner join DH$КонтрольНабора as DocCC (nolock) " + 
                        "on DocCC.iddoc = Ref.$Спр.МестаПогрузки.КонтрольНабора " + 
                    "left join $Спр.Секции as Sections (nolock) " + 
                        "on Sections.id = DocCC.$КонтрольНабора.Сектор " +
                    "inner join DH$КонтрольРасходной as DocCB (nolock) " + 
                        "on DocCB .iddoc = DocCC.$КонтрольНабора.ДокументОснование " +
                    "inner JOIN DH$Счет as Bill (nolock) " +
                        "on Bill.iddoc = DocCB.$КонтрольРасходной.ДокументОснование " + 
                    "inner join _1sjourn as journForBill (nolock) " + 
                        "on journForBill.iddoc = Bill.iddoc " + 
                    "left join $Спр.Секции as RefAdress9 (nolock) " + 
                        "on RefAdress9.id = dbo.WMP_fn_GetAdressComplete(Ref.id) " + 
                    "left join $Спр.Ворота as Gate (nolock) " +
                        "on Gate.id = DocCB.$КонтрольРасходной.Ворота " + 
                    "inner join ( " +
                        "select " + 
                            "DocCC.iddoc as iddoc, " + 
                            "count(*) as CountAllBox " + 
                        "from $Спр.МестаПогрузки as Ref (nolock) " +
                        "inner join DH$КонтрольНабора as DocCC (nolock) " + 
                            "on DocCC.iddoc = Ref.$Спр.МестаПогрузки.КонтрольНабора " + 
                        "where " +
                            "Ref.ismark = 0 " +
                            "and Ref.$Спр.МестаПогрузки.Сотрудник8 = :Employer " +
                            "and Ref.$Спр.МестаПогрузки.Дата9 = :EmptyDate " +
                            "and not Ref.$Спр.МестаПогрузки.Дата8 = :EmptyDate " +
                        "group by DocCC.iddoc ) as AllTab " + 
                        "on AllTab.iddoc = DocCC.iddoc " +
                "where " + 
                    "Ref.ismark = 0 " +
                    "and Ref.$Спр.МестаПогрузки.Сотрудник8 = :Employer " + 
                    "and not Ref.$Спр.МестаПогрузки.Дата8 = :EmptyDate " +
                    "and Ref.$Спр.МестаПогрузки.Дата9 = :EmptyDate " +
                "group by DocCC.iddoc";
            QuerySetParam(ref TextQuery, "Employer",    Employer.ID);
            QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
            if (!ExecuteWithRead(TextQuery, out CCRP))
            {
                return false;
            }
            return true;
        }

        public bool ChoiseDownComplete(int CurrLine)
        {
            if (DownSituation.Rows.Count < CurrLine)
            {
                FExcStr = CurrLine.ToString() + " - нет в списке!";
                return false;
            }
            if ((int)DownSituation.Rows[CurrLine - 1]["Allowed"] == 0)
            {
                FExcStr = "Пока нельзя! Рано!";
                return false;
            }
            Employer.Refresh();
            RefSection SectorPriory = new RefSection(this);
            if (SectorPriory.FoundID(Employer.GetAttribute("ПосланныйСектор").ToString()))
            {
                if (DownSituation.Rows[CurrLine - 1]["Sector"].ToString().Trim() != SectorPriory.Name.Trim())
                {
                    FExcStr = "Нельзя! Можно только " + SectorPriory.Name.Trim() + " сектор!";
                    return false;
                }
            }
            return ChoiseDownComplete(DownSituation.Rows[CurrLine - 1]["Sector"].ToString().Trim());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CurrParent">NAME, NAME!!! of paretn sector</param>
        /// <returns></returns>
        private bool ChoiseDownComplete(string CurrParent)
        {
            string TextQuery = 
                "declare @res int " +
                "begin tran " +
                "exec WPM_GetOrderDown :Employer, :NameParent, @res output " +
                "if @res = 0 rollback tran else commit tran " +
                "";
            QuerySetParam(ref TextQuery, "Employer",    Employer.ID);
            QuerySetParam(ref TextQuery, "NameParent",  CurrParent);

            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return ToModeDown();
        }
        public bool NewComplectationGetFirstOrder()
        {
            string TextQuery = 
                "declare @res int " +
                "begin tran " +
                "exec WPM_GetFirstOrderComplectationNew :Employer, @res output " +
                "if @res = 0 rollback tran else commit tran " +
                "";
            QuerySetParam(ref TextQuery, "Employer",    Employer.ID);

            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return ToModeNewComplectation();
        }

        public bool RepealNewComplectation()
        {
            if (DocDown.IsFirstOrder)
            {
                FExcStr = "Нельзя отказаться от этого задания!";
                return false;
            }
            string TextQuery = 
                "declare @res int; exec WPM_RepealNewComplectation :iddoc, @res output; " +
                "";
            QuerySetParam(ref TextQuery, "iddoc",    DocDown.ID);

            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return ToModeNewComplectation();
        }
        public bool AdressFull()
        {
            string TextQuery = 
                "begin tran " +
                "declare @res int " + 
                "exec WPM_AdressCompleteFull :iddoc, @res output " +
                "if @res = 0 rollback tran else commit tran " +
                "";
            QuerySetParam(ref TextQuery, "iddoc",    DocDown.ID);

            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return ToModeNewComplectation();
        }

        public bool RepealDown()
        {
            string TextQuery = 
                "declare @res int; exec WPM_RepealSetDown :iddoc, @res output; " +
                "select @res as result" + 
                "";
            QuerySetParam(ref TextQuery, "iddoc",    DocDown.ID);

            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return ToModeDown();
        }
        public bool PrintPallete()
        {
            if (!Printer.Selected)
            {
                FExcStr = "Принтер не выбран!";
                return false;
            }
            if ((decimal)DownSituation.Rows[0]["NumberOfOrder"] == 0)
            {
                string TextQuery = 
                    "declare @res int; exec WPM_GetNumberOfOrder :employer, @res output; " +
                "select @res as result" + 
                "";
                QuerySetParam(ref TextQuery, "employer",    Employer.ID);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                if (!ToModeDownComplete())
                {
                    return false;
                }
                //Повторно проверим, должно было присвоится!
                if ((decimal)DownSituation.Rows[0]["NumberOfOrder"] == 0)
                {
                    FExcStr = "Не удается присвоить номер задания!";
                    return false;
                }
            }
            FExcStr = "Отсканируйте адрес палетты!";
            if (!FlagPrintPallete)
            {
                string no = DownSituation.Rows[0]["NumberOfOrder"].ToString();
                Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Printer.ID,  "Спр.Принтеры");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "LabelRT.ert";
                DataMapWrite["Спр.СинхронизацияДанных.ДатаВход2"]       = no.Substring((no.Length - 4) < 0 ? 0 : no.Length - 4);
                if (!ExecCommandNoFeedback("Print", DataMapWrite))
                {
                    return false;
                }
                FlagPrintPallete = true;
            }
            return true;
        }
        public bool EndCC()
        {
            string TextQuery = 
                "begin tran; " + 
                "UPDATE $Спр.МестаПогрузки " +
                    "SET " + 
                        "$Спр.МестаПогрузки.Дата4 = :Date , " +
                        "$Спр.МестаПогрузки.Время4 = :Time " + 
                "WHERE " +
                    "ismark = 0 and $Спр.МестаПогрузки.КонтрольНабора = :iddoc ; " + 
                "if @@rowcount = 0 rollback tran " +
                "else begin " + 
                    "declare @res int; " +
                    "exec WPM_GetOrderDown :Employer, :NameParent, @res OUTPUT; " +
                    "if @res = 0 rollback tran " +
                    "else commit tran " +
                "end ";

            QuerySetParam(ref TextQuery, "Employer",    Employer.ID);
            QuerySetParam(ref TextQuery, "Date",        DateTime.Now);
            QuerySetParam(ref TextQuery, "Time",        APIManager.NowSecond());
            QuerySetParam(ref TextQuery, "iddoc",       DocDown.ID);
            QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
            QuerySetParam(ref TextQuery, "NameParent",  DocDown.Sector.Trim());
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return ToModeDown();
        }
        public bool EndCCNewComp()
        {
            string TextQuery = 
                "begin tran; " +
                "UPDATE $Спр.МестаПогрузки " +
                    "SET " +
                        "$Спр.МестаПогрузки.Дата8 = :Date , " +
                        "$Спр.МестаПогрузки.Время8 = :Time " +
                "WHERE " +
                    "ismark = 0 and id in (" +
                            "select Ref.id from $Спр.МестаПогрузки as Ref " + 
                                "inner join $Спр.Секции as SectionsCollect (nolock) " + 
                                    "ON SectionsCollect.id = Ref.$Спр.МестаПогрузки.Адрес7 " +
                                "inner join $Спр.Секции as RefSectionsParent (nolock) " +
                                    "on left(SectionsCollect.descr, 2) = RefSectionsParent.descr " +
                            "where " + 
                                "RefSectionsParent.descr = :NameParent " + 
                                "and Ref.$Спр.МестаПогрузки.КонтрольНабора = :iddoc " + 
                                "and Ref.$Спр.МестаПогрузки.Дата8 = :EmptyDate ) " + 
                "if @@rowcount = 0 rollback tran " +
                "else begin " +
                    "declare @res int; " +
                    "exec WPM_GetOrderComplectationNew :Employer, :NameParent, 0, @res OUTPUT; " +
                    "if @res = 0 rollback tran " +
                    "else commit tran " +
                "end ";

            QuerySetParam(ref TextQuery, "Employer",    Employer.ID);
            QuerySetParam(ref TextQuery, "Date",        DateTime.Now);
            QuerySetParam(ref TextQuery, "Time",        APIManager.NowSecond());
            QuerySetParam(ref TextQuery, "iddoc",       DocDown.ID);
            QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
            QuerySetParam(ref TextQuery, "NameParent",  DocDown.Sector.Trim());
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return ToModeNewComplectation();
        }
        public bool CompleteAll()
        {
            string TextQuery = 
                "begin tran; " + 
                "declare @res int; " +
                "declare @box char(9) " +
                "exec WPM_CompleteAll :Employer, :Adress, :iddoc, @res OUTPUT, @box output; " +
                "if @res = 0 rollback tran " +
                "else begin commit tran select @box as box end";

            QuerySetParam(ref TextQuery, "Employer",    Employer.ID);
            QuerySetParam(ref TextQuery, "Adress",      LastGoodAdress);
            QuerySetParam(ref TextQuery, "iddoc",       DocDown.ID);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                CheckFullNewComplete(DT.Rows[0]["box"].ToString());
            }
            LastGoodAdress = null;
            return ToModeNewComplectationComplete();
        }

        //Выход из режимов (должны вызываться, когда есть уверенность, что в другой режим уже зашли!)
        private bool QuitModeAcceptedItem()
        {
            bool was_permission = permission;
            permission = true;
            if (!IBS_Lockuot("int_ref_Товары_" + AcceptedItem.ID + "_unit"))
            {
                permission = was_permission;
                return false;
            }
            return true;
        }
        private void QuitModeAcceptance()
        {
            foreach(DataRow dr in FConsignment.Rows)
            {
                LockoutDocAccept(dr["ACID"].ToString());
            }
        }
        private void QuitModeAcceptanceCross()
        {
            foreach (DataRow dr in FConsignment.Rows)
            {
                LockoutDocAccept(dr["ACID"].ToString());
            }
        }
        private bool QuitModeTransfer()
        {
            //теперь укажем, что мы в зоне работаем
            string TextQuery =
               "UPDATE $Спр.Ворота " +
                   "SET " +
                       "$Спр.Ворота.ВРазносеСотрудников = (SELECT TOP 1 $Спр.Ворота.ВРазносеСотрудников - 1 FROM $Спр.Ворота WHERE id = :id) " +
               "WHERE " +
                   "id = :id ; ";

            QuerySetParam(ref TextQuery, "id", FZoneID.ToString());
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            FZoneID = null;
            FAdressZoneTransfer = null;
            FZoneName = null;
            FZoneID = null;

            return LockoutDoc(ATDoc.ID);
        }
        private bool QuitModeSamplePut()
        {
            if (!LockoutDoc(SampleDoc))
            {
                return false;
            }
            SampleDoc = null;
            return true;
        }
        private bool QuitModeLoading()
        {
            //временно отключено с релиза 4.47
            //if (!LockoutDoc(WayBill.ID))
            //{
            //    return false;
            //}
            return true;
        }

        #endregion
        #region Logic
        public bool LoadCC()
        {
            if (BadDoc.ID == null)
            {
                FExcStr = "Нет текущего сборочного!";
                return false;
            }
            string TextQuery = "select * from WPM_fn_ViewBillStatus(:iddoc)";

            QuerySetParam(ref TextQuery, "iddoc", BadDoc.ID);
            if (CCRP != null)
            {
                CCRP.Clear();
            }
            if (!ExecuteWithRead(TextQuery, out CCRP))
            {
                return false;
            }
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
            QuerySetParam(ref TextQuery, "CurrentItem", ItemID);
            if (!ExecuteWithRead(TextQuery, out FUnits))
            {
                return false;
            }
            return true;
        }
        private void AddPackNorm(ref string PackNorm, string OKEI)
        {
            DataRow[] DR = FUnits.Select("OKEI = '" + OKEI + "'");
            if (DR.Length > 0 && (int)DR[0]["Coef"] != 0)
            {
                PackNorm += "/" + DR[0]["Coef"].ToString();
                if (OKEI == OKEIKit)
                {
                    PackNorm += "!";
                }
                else if (OKEI == OKEIPack)
                {
                    PackNorm += "*";
                }
            }
        }
        private bool GetCoefPackage(string ItemID, out int Coef)
        {
            Coef = 0;
            string TextQuery =
                "SELECT TOP 1 " + 
                    "Units.$Спр.ЕдиницыШК.Коэффициент as Coef " +
                "FROM " + 
                    "$Спр.ЕдиницыШК as Units (nolock) " + 
                "WHERE " + 
                    "Units.$Спр.ЕдиницыШК.ОКЕИ = '" + OKEIPackage + "' " +
                    "and Units.ismark = 0 " + 
                    "and Units.parentext = '" + ItemID + "' " +
                "ORDER BY Units.$Спр.ЕдиницыШК.Коэффициент ";
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                Coef = 1;
            }
            else
            {
                Coef = (int)(decimal)DT.Rows[0]["Coef"];
            }
            return true;
        }
        
        private bool CreateUnit(string ItemID, string OKEI, int Coef, string Barcode)
        {
            //Нужно создать новую единицу
            string TextQuery =
                "UPDATE $Спр.ЕдиницыШК " +
                "SET " +
                    "$Спр.ЕдиницыШК.Штрихкод = :Barcode, " + 
                    "$Спр.ЕдиницыШК.Коэффициент = :Coef, " + 
                    "$Спр.ЕдиницыШК.ОКЕИ = :OKEI, " + 
                    "$Спр.ЕдиницыШК.ФлагРегистрацииМОД = 1, " + 
                    "parentext = :ItemID " + 
                "WHERE $Спр.ЕдиницыШК .id in (" + 
                    "SELECT top 1 Units.id FROM $Спр.ЕдиницыШК as Units (nolock) " +
                        "WHERE Units.parentext = :ItemForUnits " + 
                        "ORDER BY Units.ismark DESC)";
            QuerySetParam(ref TextQuery, "Barcode", Barcode);
            QuerySetParam(ref TextQuery, "Coef", Coef);
            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            QuerySetParam(ref TextQuery, "OKEI", OKEI);
            QuerySetParam(ref TextQuery, "ItemForUnits", Const.ItemForUnits);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return true;
        }
        public bool FindItemsOnBalances(string FindText, List<string> FieldName)
        {
            //Эта процедура сильно избыточна, пожалуйста, сделай с ней что нибудь!
            if (FindText.Length == 0)
            {
                FExcStr = "Пустая строка поиска!";
                return false;
            }
            //List<string> FieldName = new List<string>();
            //FieldName.Add("$Спр.Товары.ИнвКод ");
            //FExcStr - несет смысл
            int AccurateCount = 6;
            bool Accurate = true;
            string FindText6 = FindText;
            if (FindText.Length >= AccurateCount)
            {
                Accurate = false;
            }
            if (FindText.Length > 6)
            {
                FindText6 = FindText.Substring(0, 6);
            }
            string TextSubQuery = "";
            foreach (string fn in FieldName)
            {
                string tmpTextQuery =
                    " (SELECT " + 
                        "RegOT.$Рег.ОстаткиТоваров.Товар as id, " + 
                        "min(Goods." + fn + ") as " + fn + 
                        "FROM RG$Рег.ОстаткиТоваров as RegOT (nolock) " +
                        "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                            "ON Goods.id = RegOT.$Рег.ОстаткиТоваров.Товар " + 
                        "WHERE " + 
                            "period = @curdate " +
                            "and RegOT.$Рег.ОстаткиТоваров.Склад = :Warehouse " +
                        "GROUP BY RegOT.$Рег.ОстаткиТоваров.Товар " +
                        "HAVING not sum(RegOT.$Рег.ОстаткиТоваров.ОстатокТовара ) = 0) as F ";
                string digit;
                string tmpFindText = FindText6;
                while (tmpFindText.Length > 1)
                {
                    digit = tmpFindText.Substring(0, 1);
                    tmpTextQuery =
                        "(SELECT " + 
                            "id,substring(" + fn + ",charindex('" + digit + "'," + fn + ")+1,len(" + fn + ")) as " + fn + " " +
                            "FROM " + tmpTextQuery +
                            "WHERE charindex('" + digit + "'," + fn + ")>0 ";
                    for (int i = 0; i < 10; ++i)
                    {
                        if (Convert.ToInt32(digit) == i)
                        {
                            continue;
                        }
                        tmpTextQuery += "and charindex('" + i.ToString() + "',left(" + fn + ",charindex('" + digit + "'," + fn + ")-1))=0 ";
                    }
                    tmpTextQuery += ")as F ";
                    tmpFindText = tmpFindText.Substring(1);
                }
                digit = tmpFindText.Substring(0, 1);
                tmpTextQuery =
                    "SELECT id FROM " + tmpTextQuery + 
                    "WHERE charindex('" + digit + "'," + fn + ")>0 ";

                if (Accurate)
                {
                    tmpTextQuery += "and charindex('" + digit + "',substring(" + fn + ",charindex('" + digit + "'," + fn + ")+1,len(" + fn + ")))=0 ";
                    for (int i = 0; i < 10; ++i)
                    {
                        if (Convert.ToInt32(digit) == i)
                        {
                            continue;
                        }
                        tmpTextQuery += "and charindex('" + i.ToString() + "'," + fn + ")=0 ";
                    }
                }
                else
                {
                    //Если поисковая строка больше 6-и символов, то ищем не точное соответствие а соответствие на первые 6
                    for (int i = 0; i < 10; ++i)
                    {
                        if (Convert.ToInt32(digit) == i)
                        {
                            continue;
                        }
                        tmpTextQuery += "and charindex('" + i.ToString() + "',left(" + fn + ",charindex('" + digit + "'," + fn + ")-1))=0 ";
                    }
                }
                TextSubQuery += tmpTextQuery + "UNION ALL ";
            }
            TextSubQuery = TextSubQuery.Substring(0, TextSubQuery.Length - 10); //убираем последнее "UNION ALL "
            string TextQuery =
                "DECLARE @curdate DateTime; " + 
                "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " + 
                "SELECT " +
                    "Goods.id as ID, " + 
                    "Goods.Descr as ItemName," + 
                    "Goods.$Спр.Товары.ИнвКод as InvCode," + 
                    "Goods.$Спр.Товары.Артикул as Article," + 
                    "Goods.$Спр.Товары.АртикулНаУпаковке as ArticleOnPack," +
                    "Goods.$Спр.Товары.Опт_Цена as Price " + 
                "FROM " + 
                    "(SELECT distinct id FROM (" + TextSubQuery + ") as F ) as F " +
                "LEFT JOIN $Спр.Товары as Goods (nolock) " + 
                    "ON Goods.id = F.id " + 
                "ORDER BY " + FieldName[0];
            QuerySetParam(ref TextQuery, "Warehouse", Const.MainWarehouse);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            DT.Columns.Add("Number",          Type.GetType("System.Int32"));
            for (int row = 0; row < DT.Rows.Count; ++row)
            {
                DT.Rows[row]["Number"] = row + 1;
            }
            FInventory.Rows.Clear();
            FInventory.Merge(DT, false, MissingSchemaAction.Ignore);
            FExcStr = "По строке '" + FindText6 + "' найдено " + FInventory.Rows.Count.ToString() + " позиций" + (Accurate ? "" : " (по первым символам)");
            return true;
        }
        public bool FindItems(string FindText, List<string> FieldName, int AccurateCount)
        {
            if (FindText.Length == 0)
            {
                FExcStr = "Пустая строка поиска!";
                return false;
            }
            //FExcStr - несет смысл
            AccurateCount = AccurateCount == 0 ? 6 : AccurateCount;
            bool Accurate = true;
            string FindText6 = FindText;
            if (FindText.Length >= AccurateCount)
            {
                Accurate = false;
            }
            if (FindText.Length > 6)
            {
                FindText6 = FindText.Substring(0, 6);
            }
            string TextSubQuery = "";
            foreach (string fn in FieldName)
            {
                string tmpTextQuery = "$Спр.Товары (nolock) ";
                string digit;
                string tmpFindText = FindText6;
                while (tmpFindText.Length > 1)
                {
                    digit = tmpFindText.Substring(0, 1);
                    tmpTextQuery =
                        "(SELECT " + 
                            "id,substring(" + fn + ",charindex('" + digit + "'," + fn + ")+1,len(" + fn + ")) as " + fn + " " +
                            "FROM " + tmpTextQuery +
                            "WHERE charindex('" + digit + "'," + fn + ")>0 ";
                    for (int i = 0; i < 10; ++i)
                    {
                        if (Convert.ToInt32(digit) == i)
                        {
                            continue;
                        }
                        tmpTextQuery += "and charindex('" + i.ToString() + "',left(" + fn + ",charindex('" + digit + "'," + fn + ")-1))=0 ";
                    }
                    tmpTextQuery += ")as F ";
                    tmpFindText = tmpFindText.Substring(1);
                }
                digit = tmpFindText.Substring(0, 1);
                tmpTextQuery =
                    "SELECT id FROM " + tmpTextQuery + 
                    "WHERE charindex('" + digit + "'," + fn + ")>0 ";

                if (Accurate)
                {
                    tmpTextQuery += "and charindex('" + digit + "',substring(" + fn + ",charindex('" + digit + "'," + fn + ")+1,len(" + fn + ")))=0 ";
                    for (int i = 0; i < 10; ++i)
                    {
                        if (Convert.ToInt32(digit) == i)
                        {
                            continue;
                        }
                        tmpTextQuery += "and charindex('" + i.ToString() + "'," + fn + ")=0 ";
                    }
                }
                else
                {
                    //Если поисковая строка больше 6-и символов, то ищем не точное соответствие а соответствие на первые 6
                    for (int i = 0; i < 10; ++i)
                    {
                        if (Convert.ToInt32(digit) == i)
                        {
                            continue;
                        }
                        tmpTextQuery += "and charindex('" + i.ToString() + "',left(" + fn + ",charindex('" + digit + "'," + fn + ")-1))=0 ";
                    }
                }
                TextSubQuery += tmpTextQuery + "UNION ALL ";
            }
            TextSubQuery = TextSubQuery.Substring(0, TextSubQuery.Length - 10); //убираем последнее "UNION ALL "
            string TextQuery =
                "SELECT " +
                    "Goods.id as ID, " + 
                    "Goods.Descr as ItemName," + 
                    "Goods.$Спр.Товары.ИнвКод as InvCode," + 
                    "Goods.$Спр.Товары.Артикул as Article," + 
                    "Goods.$Спр.Товары.АртикулНаУпаковке as ArticleOnPack," +
                    "Goods.$Спр.Товары.Опт_Цена as Price " + 
                "FROM " + 
                    "(SELECT distinct id FROM (" + TextSubQuery + ") as F ) as F " +
                "LEFT JOIN $Спр.Товары as Goods (nolock) " + 
                    "ON Goods.id = F.id " + 
                "ORDER BY " + FieldName[0];
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            DT.Columns.Add("Number",          Type.GetType("System.Int32"));
            for (int row = 0; row < DT.Rows.Count; ++row)
            {
                DT.Rows[row]["Number"] = row + 1;
            }
            FInventory.Rows.Clear();
            FInventory.Merge(DT, false, MissingSchemaAction.Ignore);
            FExcStr = "По строке '" + FindText6 + "' найдено " + FInventory.Rows.Count.ToString() + " позиций" + (Accurate ? "" : " (по первым символам)");
            return true;
        }
        public bool ChoiseItem(string ItemID, string IDDoc)
        {
            FExcStr = null;
            FFlagBarcode = 0;
            return ToModeAcceptedItem(ItemID, IDDoc);
        }
        public bool ChoiseItemAcceptenceCross(string ItemID, string IDDoc, string OrderID)
        {
            FExcStr = null;
            FFlagBarcode = 0;
            FOrderID = OrderID;
            return ToModeAcceptedItem(ItemID, IDDoc);
        }
        public bool ChoiseItemInventory(string ItemID)
        {
            FExcStr = "Do not releaseed";
            return ToModeAcceptedItem(ItemID, "", CurrentMode);
        }

        private void CheckFullNewComplete(string Box)
        {
            string TextQuery = 
                "declare @docCB char(9) " +
                "select @DocCB = DocCC.$КонтрольНабора.ДокументОснование " + 
                    "from $Спр.МестаПогрузки as Ref " +
                        "inner join DH$КонтрольНабора as DocCC (nolock) " +
                            "on DocCC.iddoc = Ref.$Спр.МестаПогрузки.КонтрольНабора " +
                    "where Ref.ismark = 0 and Ref.id = :box " +
                "if exists (" + 
                    "select Ref.id from $Спр.МестаПогрузки as Ref (nolock) " +
                        "inner join DH$КонтрольНабора as DocCC (nolock) " +
                            "on DocCC.iddoc = Ref.$Спр.МестаПогрузки.КонтрольНабора " +
                        "where " + 
                            "Ref.ismark = 0 " + 
                            "and DocCC.$КонтрольНабора.ДокументОснование = @docCB " +
                            "and Ref.$Спр.МестаПогрузки.Дата9 = :EmptyDate ) " +
                    " select 1 as result " +
                    " else select 0 as result, @DocCB as DocCB ";

            QuerySetParam(ref TextQuery, "box",         Box);
            QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return;
            }
            if (DT.Rows.Count == 0)
            {
                return;
            }
            if ((int)DT.Rows[0]["result"] == 0)
            {
                //Тут проверяем все ли места по накладной скомплектованы и если все то хуячим!!!
                Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(LastGoodAdress, "Спр.Секции");
                DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(DT.Rows[0]["DocCB"].ToString(), "КонтрольРасходной");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = 1;    //Как????
                ExecCommandNoFeedback("Complete", DataMapWrite);
            }
        }

        
        private void LoadBadDoc(string ID)
        {
            string TextQuery =
            "Select " +
                "isnull(Sections.descr, 'Пу') as Sector, " +
                "CONVERT(char(8), CAST(LEFT(journForBill.date_time_iddoc, 8) as datetime), 4) as DateDoc, " +
                "journForBill.docno as DocNo, " +
                "DocCC.$КонтрольНабора.НомерЛиста as Number, " +
                "Ref.$Спр.МестаПогрузки.КонтрольНабора as Doc,  " +
                "TabBox.CountAllBox as CountBox, " +
                "Gate.descr as Gate " +
            "from $Спр.МестаПогрузки as Ref (nolock) " +
                "inner join DH$КонтрольНабора as DocCC (nolock) " +
                    "on DocCC.iddoc = Ref.$Спр.МестаПогрузки.КонтрольНабора " +
                "left join $Спр.Секции as Sections (nolock) " +
                    "on Sections.id = DocCC.$КонтрольНабора.Сектор " +
                "inner join DH$КонтрольРасходной as DocCB (nolock) " +
                    "on DocCB.iddoc = DocCC.$КонтрольНабора.ДокументОснование " +
                "inner JOIN DH$Счет as Bill (nolock) " +
                    "on Bill.iddoc = DocCB.$КонтрольРасходной.ДокументОснование " + 
                "INNER JOIN _1sjourn as journForBill (nolock) " + 
                    "on journForBill.iddoc = Bill.iddoc " + 
                "left join $Спр.Ворота as Gate (nolock) " +
                    "on Gate.id = DocCB.$КонтрольРасходной.Ворота " + 
                "left join ( " +
                    "select " + 
                        "DocCB.iddoc as iddoc, " + 
                        "count(*) as CountAllBox " + 
                    "from $Спр.МестаПогрузки as Ref (nolock) " +
                    "inner join DH$КонтрольНабора as DocCC (nolock) " + 
                        "on DocCC.iddoc = Ref.$Спр.МестаПогрузки.КонтрольНабора " + 
                    "inner join DH$КонтрольРасходной as DocCB (nolock) " +
                        "on DocCB.iddoc = DocCC.$КонтрольНабора.ДокументОснование " +
                    "where " +
                        "Ref.ismark = 0 " +
                    "group by DocCB.iddoc ) as TabBox " +
                    "on TabBox.iddoc = DocCB.iddoc " +
                "where Ref.id = :id";
            QuerySetParam(ref TextQuery, "id", ID);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return;
            }
            if (DT.Rows.Count > 0)
            {
                BadDoc = new StrictDoc();
                BadDoc.ID          = DT.Rows[0]["Doc"].ToString();
                BadDoc.View        = DT.Rows[0]["Sector"].ToString().Trim() + "-" + DT.Rows[0]["Number"].ToString() + " " + DT.Rows[0]["DocNo"].ToString() + " (" + DT.Rows[0]["DateDoc"].ToString() + ") мест " + DT.Rows[0]["CountBox"].ToString();
            }
        } // LoadBadDoc()
        
        private bool RDAcceptance(string IDD)
        {
            if (Vers != "4.44")
            {
                return false;
            }

            DataRow[] dr = FConsignment.Select("ParentIDD = '" + IDD + "'");
            if (dr.Length > 0)
            {
                FExcStr = "Документ уже включен!";
                return false;
            }
            //Подсасываем данные документа
            Dictionary<string, object> DataDocAC;
            if (!GetDoc(IDD, out DataDocAC, false))
            {
                return false;
            }
            if (DataDocAC["IDDOCDEF"].ToString() != "АдресПоступление")
            {
                FExcStr = "Не верный тип документа!";
                return false;
            }
            if ((bool)DataDocAC["ISMARK"])
            {
                FExcStr = "Документ помечен на удаление!";
                return false;
            }

            List<string> FieldList = new List<string>();
            FieldList.Add("ДокументОснование");
            FieldList.Add("КоличествоСтрок");

            string TextQuery =
                "SELECT " +
                    ":Number as Number, " +
                    "AC.iddoc as ACID," + 
                    ":ParentIDD as ParentIDD," + 
                    "AC.$АдресПоступление.КолСтрок as CountRow," +
                    "journ.docno as DocNo," +
                    "CAST(LEFT(journ.date_time_iddoc, 8) as datetime) as DateDoc," +
                    "CONVERT(char(8), CAST(LEFT(journ.date_time_iddoc,8) as datetime), 4) as DateDocText " +
                "FROM DH$АдресПоступление as AC (nolock) " + 
                "LEFT JOIN _1sjourn as journ (nolock) " + 
                "   ON journ.iddoc = right(AC.$АдресПоступление.ДокументОснование , 9) " + 
                "WHERE AC.iddoc = :Doc ";
            DataTable DT;
            QuerySetParam(ref TextQuery, "Number", FConsignment.Rows.Count + 1);
            QuerySetParam(ref TextQuery, "Doc", DataDocAC["ID"]);
            QuerySetParam(ref TextQuery, "Client", DataDocAC["ID"]);
            QuerySetParam(ref TextQuery, "ParentIDD", IDD);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (!LockDocAccept(DataDocAC["ID"].ToString()))
            {
                return false;
            }
            FConsignment.Merge(DT, false, MissingSchemaAction.Ignore);

            return ToModeAcceptance();
        }

        private bool RDAcceptanceCross(string IDD)
        {
            DataRow[] dr = FConsignment.Select("ParentIDD = '" + IDD + "'");
            if (dr.Length > 0)
            {
                FExcStr = "Документ уже включен!";
                return false;
            }
            //Подсасываем данные документа
            Dictionary<string, object> DataDocAC;
            if (!GetDoc(IDD, out DataDocAC, false))
            {
                return false;
            }
            if (DataDocAC["IDDOCDEF"].ToString() != "АдресПоступление")
            {
                FExcStr = "Не верный тип документа!";
                return false;
            }
            if ((bool)DataDocAC["ISMARK"])
            {
                FExcStr = "Документ помечен на удаление!";
                return false;
            }

            List<string> FieldList = new List<string>();
            FieldList.Add("ДокументОснование");
            FieldList.Add("КоличествоСтрок");

            string TextQuery =
                "SELECT " +
                    ":Number as Number, " +
                    "AC.iddoc as ACID," +
                    ":ParentIDD as ParentIDD," +
                    "AC.$АдресПоступление.КолСтрок as CountRow," +
                    "journ.docno as DocNo," +
                    "CAST(LEFT(journ.date_time_iddoc, 8) as datetime) as DateDoc," +
                    "CONVERT(char(8), CAST(LEFT(journ.date_time_iddoc,8) as datetime), 4) as DateDocText " +
                "FROM DH$АдресПоступление as AC (nolock) " +
                "LEFT JOIN _1sjourn as journ (nolock) " +
                "   ON journ.iddoc = right(AC.$АдресПоступление.ДокументОснование , 9) " +
                "WHERE AC.iddoc = :Doc ";
            DataTable DT;
            QuerySetParam(ref TextQuery, "Number", FConsignment.Rows.Count + 1);
            QuerySetParam(ref TextQuery, "Doc", DataDocAC["ID"]);
            QuerySetParam(ref TextQuery, "ParentIDD", IDD);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (!LockDocAccept(DataDocAC["ID"].ToString()))
            {
                return false;
            }
            FConsignment.Merge(DT, false, MissingSchemaAction.Ignore);

            return ToModeAcceptanceCross();
        }
        
        private bool RDSamplePut(string IDD)
        {
            string ATID;
            string TextQuery;
            DataTable DT;
            //Подсасываем данные документа
            Dictionary<string, object> DataDocAT;
            if (!GetDoc(IDD, out DataDocAT, false))
            {
                return false;
            }
            if (DataDocAT["IDDOCDEF"].ToString() == "АдресПеремещение")
            {
                ATID =  DataDocAT["ID"].ToString();
            }
            else if (DataDocAT["IDDOCDEF"].ToString() == "Перемещение" || DataDocAT["IDDOCDEF"].ToString() == "ПриходныйОрдер")
            {
                TextQuery =
                    "SELECT " + 
                        "journ.IDDOC as ID, " + 
                        "journ.$IDD as IDD " + 
                    "FROM _1SJOURN as journ (NOLOCK INDEX=ACDATETIME) " +
                    "INNER JOIN _1SCRDOC as crdoc (NOLOCK INDEX=PARENT) " +
                        "ON journ.DATE_TIME_IDDOC = crdoc.CHILD_DATE_TIME_IDDOC " +
                    "LEFT JOIN DH$АдресПеремещение as DocAT (nolock) " + 
                        "ON DocAT.iddoc = journ.iddoc " + 
                    "WHERE " + 
                        "crdoc.MDID=0 " +
                        "and crdoc.PARENTVAL='O1" + ExtendID(DataDocAT["ID"].ToString(), DataDocAT["IDDOCDEF"].ToString()) + "' " +
                        "and journ.CLOSED&1 = 1 " + 
                        "and DocAT.$АдресПеремещение.ТипДокумента = 3 " + 
                    "ORDER BY journ.IDDOC";
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                if (DT.Rows.Count == 0)
                {
                    FExcStr = "Не верный тип документа!";
                    return false;
                }
                foreach (DataRow dr in DT.Rows)
                {
                    if (!RDSamplePut(dr["IDD"].ToString()))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                //Тут нужно допилить код, который выведет на документ АдресПеремещение с других докуменов ветки иерерхии
                FExcStr = "Не верный тип документа!";
                return false;
            }
            TextQuery =
                "SELECT " +
                    ":Number as Number, " +
                    "AT.iddoc as ATID," + 
                    "AT.$АдресПеремещение.КолСтрок as CountRow," +
                    "ISNULL(journ.docno, journ_self.docno) as DocNo," +
                    "CAST(LEFT(ISNULL(journ.date_time_iddoc, journ_self.date_time_iddoc), 8) as datetime) as DateDoc," +
                    "CONVERT(char(8), CAST(LEFT(ISNULL(journ.date_time_iddoc, journ_self.date_time_iddoc),8) as datetime), 4) as DateDocText " +
                "FROM DH$АдресПеремещение as AT (nolock) " + 
                "LEFT JOIN _1sjourn as journ (nolock) " + 
                "   ON journ.iddoc = right(AT.$АдресПеремещение.ДокументОснование , 9) " + 
                "LEFT JOIN _1sjourn as journ_self (nolock) " +
                "   ON journ_self.iddoc = AT.iddoc " +
                "WHERE AT.iddoc = :Doc " +
                    "and AT.$АдресПеремещение.ТипДокумента = 3";
            QuerySetParam(ref TextQuery, "Number", SampleTransfers.Rows.Count + 1);
            QuerySetParam(ref TextQuery, "Doc", ATID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "Неверный тип перемещения!";
                return false;
            }
            DataRow[] DR = SampleTransfers.Select("ATID = '" + DT.Rows[0]["ATID"].ToString() + "'");
            if (DR.Length > 0)
            {
                FExcStr = "Документ уже включен!";
                return false;
            }
            SampleTransfers.Merge(DT, false, MissingSchemaAction.Ignore);

            return ToModeSamplePut();
        }
        
        //СТАРЫЙ РЕЖИМ
        //private bool RDSampleSet(string IDD)
        //{
        //    string IDDoc;
        //    string DocType;
        //    Dictionary <string, object> DataMap;
        //    if (!GetDoc(IDD, out IDDoc, out DocType, out DataMap))
        //    {
        //        return false;
        //    }
        //    if (DocType != "КонтрольНабора")
        //    {
        //        FExcStr = "Неверный тип документа!";
        //        return false;
        //    }

        //    DataRow[] DR = CCListSample.Select("ID = '" + IDDoc + "'");
        //    if (DR.Length > 0)
        //    {
        //        FExcStr = "Документ уже включен!";
        //        return false;
        //    }

        //    DataMap.Clear();
        //    List<string> FieldList = new List<string>();
        //    FieldList.Add("КонтрольНабора.Дата2");
        //    FieldList.Add("КонтрольНабора.АдресСобранного");
        //    FieldList.Add("КонтрольНабора.Сектор");
        //    FieldList.Add("КонтрольНабора.ДокументОснование");
        //    FieldList.Add("КонтрольНабора.НомерЛиста");
        //    if (!GetDocData(IDDoc, "КонтрольНабора", FieldList, out DataMap))
        //    {
        //        return false;
        //    }
        //    if (SQL1S.IsVoidDate((DateTime)DataMap["КонтрольНабора.Дата2"]))
        //    {
        //        FExcStr = "Набор не завершен!";
        //        return false;
        //    }
        //    if (SQL1S.GetVoidID() != DataMap["КонтрольНабора.АдресСобранного"].ToString())
        //    {
        //        FExcStr = "Адрес собранного уже задан!";
        //        return false;
        //    }
        //    string NumberCC = DataMap["КонтрольНабора.НомерЛиста"].ToString();
        //    string sector   = DataMap["КонтрольНабора.Сектор"].ToString();
        //    string CBID     = DataMap["КонтрольНабора.ДокументОснование"].ToString();
        //    DataMap.Clear();
        //    FieldList.Clear();
        //    FieldList.Add("DESCR");
        //    if (!GetSCData(sector, "Секции", FieldList, out DataMap, true))
        //    {
        //        return false;
        //    }
        //    sector = DataMap["DESCR"].ToString().Trim();

        //    FieldList.Clear();
        //    DataMap.Clear();
        //    FieldList.Add("КонтрольРасходной.ДокументОснование");
        //    if (!GetDocData(CBID, "КонтрольРасходной", FieldList, out DataMap))
        //    {
        //        return false;
        //    }
        //    string DaemondID = DataMap["КонтрольРасходной.ДокументОснование"].ToString();
        //    DataMap.Clear();
        //    if (!GetDoc(DaemondID, out DataMap))
        //    {
        //        return false;
        //    }

        //    DataRow newRow = CCListSample.NewRow();
        //    newRow["ID"]    = IDDoc;
        //    newRow["View"]  = DataMap["НомерДок"].ToString() + " (" + ((DateTime)DataMap["ДатаДок"]).ToString("dd.MM") + ") СЕКЦИЯ: " + sector + "-" + NumberCC;
        //    CCListSample.Rows.Add(newRow);
        //    return true;
        //}
        
        private bool RDLoadingInicialization(string IDD)
        {
            string IDDoc;
            string DocType;
            Dictionary <string, object> DataMap;
            if (!GetDoc(IDD, out IDDoc, out DocType, out DataMap))
            {
                return false;
            }
            if (DocType != "ПутевойЛист")
            {
                FExcStr = "Неверный тип документа!";
                return false;
            }

            WayBill.ID = IDDoc;
            WayBill.View = DataMap["НомерДок"].ToString() + " (" + ((DateTime)DataMap["ДатаДок"]).ToString("dd.MM.yy") + ")";
            return true;
        }
        private bool RBSample(string Barcode, Mode ToMode, int Screan)
        {
            string TextQuery;
            DataTable DT;
            //Супер китайская процедура!
            if (Screan == 1)
            {
                TextQuery =
                    "SELECT top 1 " +
                        "Goods.id as ID, " + 
                        "Goods.Descr as ItemName," + 
                        "Goods.$Спр.Товары.ИнвКод as InvCode," + 
                        "Goods.$Спр.Товары.Артикул as Article," + 
                        "Goods.$Спр.Товары.АртикулНаУпаковке as ArticleOnPack," +
                        "Goods.$Спр.Товары.Опт_Цена as Price " + 
                    "FROM $Спр.ЕдиницыШК as Units (nolock) " +
                        "LEFT JOIN $Спр.Товары as Goods (nolock) " + 
                            "ON Goods.id = Units.parentext " +
                    "WHERE Units.$Спр.ЕдиницыШК.Штрихкод = :Barcode ";
                QuerySetParam(ref TextQuery, "Barcode", Barcode);
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                if (DT.Rows.Count == 0)
                {
                    FExcStr = "С таким штрихкодом товар не найден!";
                    return false;
                }
                DT.Columns.Add("Number",          Type.GetType("System.Int32"));
                AddToForPrint(DT.Rows[0]);
                return true;
            }


            FExcStr = "";   //Передает информацию дальше, поэтому должен не быть null-ом
            TextQuery = 
                "SELECT " + 
                    "Units.parentext as ItemID, " + 
                    "Units.$Спр.ЕдиницыШК.ОКЕИ as OKEI " +
                "FROM $Спр.ЕдиницыШК as Units (nolock) " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " + 
                        "ON Goods.id = Units.parentext " +
                "WHERE Units.$Спр.ЕдиницыШК.Штрихкод = :Barcode ";
            QuerySetParam(ref TextQuery, "Barcode", Barcode);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "С таким штрихкодом товар не найден!";
                return false;
            }
            if (DT.Rows[0]["OKEI"].ToString() != OKEIUnit)
            {
                FExcStr = "(отсканированный ШК - не ШТУК)";
            }
            string ItemID = DT.Rows[0]["ItemID"].ToString();
            if (ToMode == Mode.SampleInventory || DT.Rows[0]["OKEI"].ToString() != OKEIUnit)
            {
                return ToModeAcceptedItem(ItemID, "", ToMode);
            }
            else
            {
                return AddSample(ItemID);
            }
        }
        private bool RBHarmonization(string Barcode, Mode ToMode)
        {
            string TextQuery = 
                "SELECT " + 
                    "Units.parentext as ItemID " + 
                "FROM $Спр.ЕдиницыШК as Units (nolock) " +
                "WHERE Units.$Спр.ЕдиницыШК.Штрихкод = :Barcode ";
            QuerySetParam(ref TextQuery, "Barcode", Barcode);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "С таким штрихкодом товар не найден!";
                return false;
            }
            return ToModeAcceptedItem(DT.Rows[0]["ItemID"].ToString(), "", ToMode, 0, true);
        }
        private bool RBTransfer(string Barcode)
        {
            Employer.Refresh();
            string TextQuery;
            DataTable DT;
            if (Barcode == FBarcodePallet)
            {
                //Паллету можно добавлять только когда весь товар разнесен
                if (TransferItems.Rows.Count > 0)
                {
                    FExcStr = "В телеге есть товар, добавлять нельзя!";
                    return false;
                }
                //подтянем все что принято и что лежит в этой зоне и на этой паллете

                TextQuery = "DECLARE @curdate DateTime; " +
                "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock) ; " +
                "SELECT " +
                    "RegAC.$Рег.АдресОстаткиПоступления.Товар as ItemID, " +
                    "sum(DocAP.$АдресПоступление.Количество ) as Count, " +
                    "sum(RegAC.$Рег.АдресОстаткиПоступления.Количество ) as ACount " +
                "FROM " +
                    "RG$Рег.АдресОстаткиПоступления as RegAC (nolock) " +
                    "INNER JOIN DT$АдресПоступление as DocAP (nolock) " +
                        "ON (RegAC.$Рег.АдресОстаткиПоступления.Док = DocAP.IDDOC ) " +
                        "and (RegAC.$Рег.АдресОстаткиПоступления.Товар = DocAP.$АдресПоступление.Товар ) " +
                    "INNER JOIN $Спр.ПеремещенияПаллет as RefPallet (nolock) " +
                        "ON (DocAP.$АдресПоступление.Паллета = RefPallet.ID) " +

                "WHERE " +
                    "RegAC.period = @curdate " +
                    "and RegAC.$Рег.АдресОстаткиПоступления.Адрес = :AdressID " +
                    "and RegAC.$Рег.АдресОстаткиПоступления.Склад = :Warehouse " +
                    "and RefPallet.$Спр.ПеремещенияПаллет.ШКПаллеты = :BarcodePallet " +
                    "and RegAC.$Рег.АдресОстаткиПоступления.ТипДействия = 2 " +
                    "and NOT DocAP.$АдресПоступление.Дата0 = :EmptyDate " +
                    "and RegAC.$Рег.АдресОстаткиПоступления.Количество > 0 " +

                "GROUP BY " +
                    "RegAC.$Рег.АдресОстаткиПоступления.Товар ";

                QuerySetParam(ref TextQuery, "Warehouse", ATDoc.FromWarehouseID);
                QuerySetParam(ref TextQuery, "AdressID", ZoneAdressTransfer.ID);
                QuerySetParam(ref TextQuery, "BarcodePallet", Barcode);
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                if (DT.Rows.Count == 0)
                {
                    FExcStr = "С таким ШК паллета в зоне не найдена!";
                    return false;
                }
                //а теперь надо добавить весь товар с этой паллеты в телегу
                int lastnorm = DT.Rows.Count;
                for (int i = 0; i < DT.Rows.Count; ++i)
                {

                    AcceptedItem.ToMode = Mode.Transfer;
                    if (!AddParty(DT.Rows[i]["ItemID"].ToString(), Int32.Parse(DT.Rows[i]["Count"].ToString())))
                    {
                        break;
                    }
                    lastnorm = i;
                }
                if (lastnorm < DT.Rows.Count - 1)
                {
                    for (int i = 0; i <= lastnorm; ++i)
                    {
                        DeleteRowTransferItem(DT.Rows[i]["ItemID"].ToString(), DT.Rows[i]["Count"].ToString());
                    }
                    return false;
                }
                return true;
            }
            TextQuery =
                "SELECT " +
                    "Units.parentext as ItemID " +
                "FROM $Спр.ЕдиницыШК as Units (nolock) " +
                "WHERE Units.$Спр.ЕдиницыШК.Штрихкод = :Barcode ";
            QuerySetParam(ref TextQuery, "Barcode", Barcode);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "С таким штрихкодом товар не найден!";
                return false;
            }
            //раньше было так
            //return CheckParty(DT.Rows[0]["ItemID"].ToString());
            OkRowIndex = -1;
            int Count = 0;
            DataRow[] DR = FTransferItems.Select("ID = '" + DT.Rows[0]["ItemID"].ToString() + "'");
            foreach (DataRow dr in DR)
            {
                Count += (int)(decimal)dr["Coef"] * (int)(decimal)dr["CountPackage"];
            }
            if (Count > 0)
            {
                FExcStr = DR[0]["InvCode"].ToString().Trim() + " - есть в тележке " + GetStrPackageCount(Count, (int)(decimal)DR[0]["Coef"]);
                CurrentLine = FTransferItems.Rows.IndexOf(DR[0]);
                OkRowIndex = CurrentLine;
                return ToModeAcceptedItem(DT.Rows[0]["ItemID"].ToString(), "", CurrentMode, Count, true);
            }
            
            //товара нет в телеге, проверим где он есть
            TextQuery = "DECLARE @curdate DateTime; " +
            "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock) ; " +
            "SELECT " +
                "substring(RefPallet.$Спр.ПеремещенияПаллет.ШКПаллеты ,9,4) as Pallet, " +
                "max(RegAC.$Рег.АдресОстаткиПоступления.Адрес ) as AdressID " +
            "FROM " +
                "RG$Рег.АдресОстаткиПоступления as RegAC (nolock) " +
                "INNER JOIN DT$АдресПоступление as DocAP (nolock) " +
                    "ON (RegAC.$Рег.АдресОстаткиПоступления.Док = DocAP.IDDOC ) " +
                    "and (RegAC.$Рег.АдресОстаткиПоступления.Товар = DocAP.$АдресПоступление.Товар ) " +
                "INNER JOIN $Спр.ПеремещенияПаллет as RefPallet (nolock) " +
                    "ON (DocAP.$АдресПоступление.Паллета = RefPallet.ID) " +

            "WHERE " +
                "RegAC.period = @curdate " +
                "and RegAC.$Рег.АдресОстаткиПоступления.Склад = :Warehouse " +
                "and RegAC.$Рег.АдресОстаткиПоступления.Товар = :ItemID " +
                "and RegAC.$Рег.АдресОстаткиПоступления.ТипДействия = 2 " +
                "and NOT DocAP.$АдресПоступление.Дата0 = :EmptyDate " +
            "GROUP BY " +
                "RefPallet.$Спр.ПеремещенияПаллет.ШКПаллеты ";
            QuerySetParam(ref TextQuery, "Warehouse", ATDoc.FromWarehouseID);
            QuerySetParam(ref TextQuery, "AdressID", ZoneAdressTransfer.ID);
            QuerySetParam(ref TextQuery, "ItemID", DT.Rows[0]["ItemID"].ToString());
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "С таким ШК товара нет на паллетах!";
            }
            else
            {
                FExcStr = "Товар с паллеты " + DT.Rows[0]["Pallet"].ToString();
            }
            return false;
        }
        
        
        public bool WriteGeneratedBarcode(string ItemID)
        {
            string TextQuery = "exec GenerateParcode :ItemID";
            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return true;
        }
        public void DeleteConsignment(string ACID)
        {
            FExcStr = null;
            DataRow[] DR = FConsignment.Select("ACID = '" + ACID + "'");
            foreach (DataRow curDR in DR)
            {
                curDR.Delete();
            }
            for (int row=0; row<FConsignment.Rows.Count; row++)
            {
                FConsignment.Rows[row]["Number"] = row+1;
            }

            DR = FAcceptedItems.Select("IDDOC = '" + ACID + "'");
            foreach (DataRow curDR in DR)
            {
                curDR.Delete();
            }
            DR = FNotAcceptedItems.Select("IDDOC = '" + ACID + "'");
            foreach (DataRow curDR in DR)
            {
                curDR.Delete();
            }
            LockoutDocAccept(ACID);
        }
        public void DeleteSampleTransfer(string ATID)
        {
            FExcStr = null;
            DataRow[] DR = SampleTransfers.Select("ATID = '" + ATID + "'");
            foreach (DataRow curDR in DR)
            {
                curDR.Delete();
            }
            for (int row=0; row<SampleTransfers.Rows.Count; row++)
            {
                SampleTransfers.Rows[row]["Number"] = row+1;
            }

            DR = FSampleItems.Select("Doc = '" + ATID + "'");
            foreach (DataRow curDR in DR)
            {
                curDR.Delete();
            }
        }
        public bool CompleteAccept()
        {
            FExcStr = null;
            string TextQuery;

            int BeginCount = 0;

            if (CurrentMode != Mode.AcceptedItem)
            {
                FExcStr = "Недопустимая команда в текущем режиме!";
                return false;
            }
            if (AcceptedItem.Acceptance)
            {
                BeginCount = (int)CurrentRowAcceptedItem["Count"];  //Сколько в накладной изначально

                if (AcceptedItem.AcceptCount > (int)CurrentRowAcceptedItem["Count"])
                {
                    FExcStr = "Нельзя принять по данной накладной более " + CurrentRowAcceptedItem["Count"].ToString() + " штук!";
                    return false;
                }
                else if (AcceptedItem.AcceptCount == 0)
                {
                    FExcStr = "Нельзя принять нулевое количество!";
                    return false;
                }

                //Теперь проверим не поменялась ли ситуация в самой накладной, пока мы курили бамбук!
                TextQuery = 
                    "SELECT " + 
                        "ACDT.$АдресПоступление.Количество as Count " +
                    "FROM " +
                        "DT$АдресПоступление as ACDT (nolock) " +
                    "WHERE " +
                        "ACDT.iddoc = :Doc " +
                        "and ACDT.$АдресПоступление.Товар = :Item " + 
                        "and ACDT.$АдресПоступление.Состояние0 = 0 " +
                        "and ACDT.lineno_ = :LineNo_ ";
                QuerySetParam(ref TextQuery, "Doc",         CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "Item",        CurrentRowAcceptedItem["ID"]);
                QuerySetParam(ref TextQuery, "LineNo_",     CurrentRowAcceptedItem["LINENO_"]);

                DataTable DTCurrState;
                if (!ExecuteWithRead(TextQuery, out DTCurrState))
                {
                    return false;
                }
                if (DTCurrState.Rows.Count == 0)
                {
                    FExcStr = "Недопустимое количество! Повторите приемку позиции!";
                    return false;
                }
                if ((int)(decimal)DTCurrState.Rows[0]["Count"] < AcceptedItem.AcceptCount)
                {
                    FExcStr = "Недопустимое количество! Повторите приемку позиции!";
                    return false;
                }
                //Скорректируем начальное количество
                BeginCount = (int)(decimal)DTCurrState.Rows[0]["Count"];
            }

            int NeedNew     = 0;
            int CoefPlace   = 0;  //Коэффициент мест, по нему будет расчитывать количество этикеток
            DataTable tmpDT = new DataTable();
            tmpDT.Columns.Add("Coef", Type.GetType("System.Int32"));
            tmpDT.Columns.Add("OKEI", Type.GetType("System.String"));
            foreach (DataRow dr in FUnits.Rows)
            {
                if ((int)dr["Coef"] == 1 && dr["OKEI"].ToString() != OKEIUnit)
                {
                    FExcStr = "Коэффициент 1 может быть только у штуки! Пожалуйста исправьте...";
                    return false;
                }
                if (dr["OKEI"].ToString() == OKEIPackage)
                {
                    CoefPlace = (int)dr["Coef"];
                }
                if (dr["ID"].ToString() != GetVoidID())
                {
                    //Имеющаяся единица
                    if ((int)dr["Coef"] == 0)
                    {
                        TextQuery = 
                            "UPDATE $Спр.ЕдиницыШК " +
                            "SET " +
                                "ismark = 1 " +
                            "WHERE $Спр.ЕдиницыШК .id = :ID ";
                        QuerySetParam(ref TextQuery, "ID", dr["ID"]);
                    }
                    else
                    {
                        TextQuery =
                            "UPDATE $Спр.ЕдиницыШК " +
                            "SET " +
                                "$Спр.ЕдиницыШК.Штрихкод = :Barcode, " +
                                "$Спр.ЕдиницыШК.Коэффициент = :Coef, " + 
                                "$Спр.ЕдиницыШК.ОКЕИ = :OKEI, " + 
                                "$Спр.ЕдиницыШК.ФлагРегистрацииМОД = 1 " + 
                            "WHERE $Спр.ЕдиницыШК .id = :ID ";
                        QuerySetParam(ref TextQuery, "Barcode", dr["Barcode"]);
                        QuerySetParam(ref TextQuery, "Coef", dr["Coef"]);
                        QuerySetParam(ref TextQuery, "ID", dr["ID"]);
                        QuerySetParam(ref TextQuery, "OKEI", dr["OKEI"]);
                    }
                    if (!ExecuteWithoutRead(TextQuery))
                    {
                        return false;
                    }
                }
                else
                {
                    DataRow[] tmpDR = tmpDT.Select("Coef = " + dr["Coef"].ToString() + " and OKEI = '" + dr["OKEI"] + "'");
                    if (dr["Barcode"].ToString().Trim() != "" || tmpDR.Length == 0)
                    {
                        NeedNew++;
                    }
                }
                DataRow tmpdr = tmpDT.NewRow();
                tmpdr["Coef"] = (int)dr["Coef"];
                tmpdr["OKEI"] = dr["OKEI"].ToString();
                tmpDT.Rows.Add(tmpdr);
            }
            tmpDT.Clear();

            if (NeedNew > 0)
            {
                //Есть новые...
                int CurrentRow = 0;
                //Теперь также пишем новые
                foreach (DataRow dr in FUnits.Rows)
                {
                    if ((int)dr["Coef"] == 0)
                    {
                        continue;
                    }
                    if (dr["ID"].ToString() == GetVoidID())
                    {
                        DataRow[] tmpDR = tmpDT.Select("Coef = " + dr["Coef"].ToString() + " and OKEI = '" + dr["OKEI"] + "'");
                        if (!(dr["Barcode"].ToString().Trim() != "" || tmpDR.Length == 0))
                        {
                            continue;
                        }
                        if (!CreateUnit(AcceptedItem.ID, dr["OKEI"].ToString(), (int)dr["Coef"], dr["Barcode"].ToString()))
                        {
                            return false;
                        }
                        CurrentRow++;
                    }
                    DataRow tmpdr = tmpDT.NewRow();
                    tmpdr["Coef"] = (int)dr["Coef"];
                    tmpdr["OKEI"] = dr["OKEI"].ToString();
                    tmpDT.Rows.Add(tmpdr);
                }
            }
            //Запишем норму упаковки в товар
            string PackNorm = "";
            AddPackNorm(ref PackNorm, OKEIKit);
            AddPackNorm(ref PackNorm, OKEIPack);
            AddPackNorm(ref PackNorm, OKEIPackage);
            if (PackNorm == "")
            {
                PackNorm = "1";
            }
            else
            {
                PackNorm = PackNorm.Substring(1);  //Отрезаем первый символ "/"
            }
            if (AcceptedItem.MinParty > 0)
            {
                PackNorm = ">" + AcceptedItem.MinParty.ToString() + "/" + PackNorm;
            }
            TextQuery = 
                "UPDATE $Спр.Товары " +
                    "SET $Спр.Товары.НормаУпаковки = :PackNorm , " +
                        "$Спр.Товары.КоличествоДеталей = :Details " +
                "WHERE $Спр.Товары .id = :ItemID";
            QuerySetParam(ref TextQuery, "ItemID",      AcceptedItem.ID);
            QuerySetParam(ref TextQuery, "PackNorm",    PackNorm);
            QuerySetParam(ref TextQuery, "Details",     AcceptedItem.NowDetails);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }

            //Если ШК с базовой единицы слетел, то поставим его назад этим чудовищным запросом
            TextQuery = 
                "declare @idbase char(9) " + 
                "declare @idbar char(9) " + 
                "declare @barcode varchar(50) " + 
                "select @idbase = RefE.id from $Спр.Товары as RefG (nolock) " +
                    "inner join $Спр.ЕдиницыШК as RefE (nolock) " +
                        "on RefE.id = RefG.$Спр.Товары.БазоваяЕдиницаШК " + 
                    "where RefG.id = :ItemID and RefE.$Спр.ЕдиницыШК.Штрихкод = ''; " +
                "if @idbase is not null begin " +
                    "select @idbar = RefE.id, @barcode = RefE.$Спр.ЕдиницыШК.Штрихкод from $Спр.ЕдиницыШК as RefE (nolock) " +
                        "where " + 
                            "RefE.parentext = :ItemID " + 
                            "and RefE.$Спр.ЕдиницыШК.ОКЕИ = :OKEI " +
                            "and not RefE.$Спр.ЕдиницыШК.Штрихкод = ''; " +
                    "if @idbar is not null begin " +
                        "begin tran; " +
                        "update $Спр.ЕдиницыШК " + 
                            "set $Спр.ЕдиницыШК.Штрихкод = @barcode " + 
                            "where id = @idbase; " + 
                        "if @@rowcount = 0 " + 
                            "rollback tran " + 
                        "else begin " + 
                            "update $Спр.ЕдиницыШК " + 
                                "set $Спр.ЕдиницыШК.Штрихкод = '' " + 
                                "where id = @idbar; " + 
                            "if @@rowcount = 0 " + 
                                "rollback tran " + 
                            "else " + 
                                "commit tran " +
                        "end " +
                    "end " +
                "end";
            QuerySetParam(ref TextQuery, "OKEI",   OKEIUnit);
            QuerySetParam(ref TextQuery, "ItemID", AcceptedItem.ID);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            //Конец хуйни с заменой ШК у базовой единицы
            
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            if (!AcceptedItem.Acceptance)
            {
                //begin internal command
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(AcceptedItem.ID, "Спр.Товары");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "SaveItem (Сохранил единицы)";
                if (!ExecCommandNoFeedback("Internal", DataMapWrite))
                {
                    return false;
                }
                //end internal command
                //Это не приемка, а просто правка карточки
                FExcStr = AcceptedItem.InvCode.Trim() + " карточка товара сохранена!";
                if (AcceptedItem.ToMode == Mode.Acceptance)
                {
                    if (!ToModeAcceptance())
                    {
                        return false;
                    }
                    return QuitModeAcceptedItem();
                }
                else if (AcceptedItem.ToMode == Mode.Inventory)
                {
                    if (!ToModeInventory())
                    {
                        return false;
                    }
                    return QuitModeAcceptedItem();
                }
                else if (AcceptedItem.ToMode == Mode.Transfer || AcceptedItem.ToMode == Mode.NewInventory)
                {
                    if (AcceptedItem.OnShelf)
                    {
                        //На полку!
                        //все уже сделали, можно расслабиться!
                    }
                    else
                    {
                        if (AcceptedItem.ToMode == Mode.Transfer)
                        {
                            if (!AddParty(AcceptedItem.ID, AcceptedItem.AcceptCount))
                            {
                                return false;
                            }
                        }
                        else
                        {
                            //Добавим строчку
                            TextQuery =
                                "SELECT ISNULL(max(DT$АдресПеремещение .lineno_) + 1, 1) as LineNo_ " +
                                "FROM DT$АдресПеремещение " + 
                                "WHERE DT$АдресПеремещение .iddoc = :Doc";
                            QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
                            DataTable DT;
                            if (!ExecuteWithRead(TextQuery, out DT))
                            {
                                return false;
                            }
                            int LineNo_ = (int)DT.Rows[0]["LineNo_"];

                            TextQuery =
                                "INSERT INTO DT$АдресПеремещение VALUES " +
                                    "(:Doc, :LineNo_, :Item, :Count, :EmptyID, :Coef, :State0, 2, :Employer, " + 
                                    ":Adress0, :Adress1, :Date0, :EmptyDate, :Time0, 0, :ACDoc, :Number, " +
                                    ":SampleCount, :BindingAdressFlag, :UnitID); ";
                            QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
                            QuerySetParam(ref TextQuery, "LineNo_", LineNo_);
                            QuerySetParam(ref TextQuery, "Item", AcceptedItem.ID);
                            QuerySetParam(ref TextQuery, "Count", AcceptedItem.AcceptCount);
                            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
                            QuerySetParam(ref TextQuery, "Coef", 1);
                            QuerySetParam(ref TextQuery, "State0", 0);
                            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                            QuerySetParam(ref TextQuery, "Adress0", GetVoidID());
                            QuerySetParam(ref TextQuery, "Adress1", GetVoidID());
                            QuerySetParam(ref TextQuery, "Date0", DateTime.Now);
                            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                            QuerySetParam(ref TextQuery, "Time0", APIManager.NowSecond());
                            QuerySetParam(ref TextQuery, "ACDoc", ExtendID(GetVoidID(), "АдресПеремещение"));
                            QuerySetParam(ref TextQuery, "Number", 0);
                            QuerySetParam(ref TextQuery, "UnitID", AcceptedItem.BaseUnitID);
                            QuerySetParam(ref TextQuery, "SampleCount", 0);
                            QuerySetParam(ref TextQuery, "BindingAdressFlag", 0);
                            if (!ExecuteWithoutRead(TextQuery))
                            {
                                return false;
                            }

                        }
                    }
                    if (!ToModeTransfer(ATDoc, AcceptedItem.ToMode))
                    {
                        return false;
                    }
                    return QuitModeAcceptedItem();
                }
                else if (AcceptedItem.ToMode == Mode.SampleInventory || AcceptedItem.ToMode == Mode.SamplePut)
                {
                    if (AcceptedItem.GenerateBarcode)
                    {
                        //Нужно сгенерировать штрихкод для базовой единицы
                        WriteGeneratedBarcode(AcceptedItem.ID);
                        AcceptedItem.GenerateBarcode = false;
                    }
                    if (AcceptedItem.ToMode == Mode.SampleInventory)
                    {
                        if (!ToModeSampleInventory())
                        {
                            return false;
                        }
                        return QuitModeAcceptedItem();
                    }
                    else //Mode.SamplePut
                    {
                        if (!QuitModeAcceptedItem())
                        {
                            return false;
                        }
                        if (!AddSample(AcceptedItem.ID))
                        {
                            return ToModeSamplePut();
                        }
                        else
                        {
                            return true;
                        }
                    }
                    
                }
            }

            //ТЕПЕРЬ ПОЕХАЛА ЗАПИСЬ ДОКУМЕНТА
            //Расчитаем число этикеток
            int LabelCount;
            if (FlagBarcode != 0)
            {
                LabelCount = 0;
            }
            else
            {
                LabelCount = 1;
                //if (CoefPlace > 0 && CountAccepted >= CoefPlace)
                //{
                //    LabelCount = (int)CountAccepted/CoefPlace;
                //}
            }

            //Для начала подсосем есть ли уже принятые и не напечатанные строки в накладной
            TextQuery = 
                "SELECT " + 
                    "ACDT.LineNo_ as LineNo_, " +
                    "ACDT.$АдресПоступление.Количество as Count, " +
                    "ACDT.$АдресПоступление.КоличествоЭтикеток as LabelCount " +
                "FROM " +
                    "DT$АдресПоступление as ACDT (nolock) " +
                "WHERE " +
                    "ACDT.iddoc = :Doc " +
                    "and ACDT.$АдресПоступление.Товар = :Item " + 
                    "and ACDT.$АдресПоступление.ФлагПечати = 1 " + 
                    "and ACDT.$АдресПоступление.Сотрудник0 = :Employer";
            QuerySetParam(ref TextQuery, "Doc",         CurrentRowAcceptedItem["IDDOC"]);
            QuerySetParam(ref TextQuery, "Item",        CurrentRowAcceptedItem["ID"]);
            QuerySetParam(ref TextQuery, "Employer",    Employer.ID);
            DataTable AlreadyDT;
            if (!ExecuteWithRead(TextQuery, out AlreadyDT))
            {
                return false;
            }

            int AllCountAccepted = AcceptedItem.AcceptCount;
            if (AlreadyDT.Rows.Count == 0 && AcceptedItem.AcceptCount < BeginCount)
            {
                //Нуно создать новую строку
                TextQuery =
                    "SELECT max(DT$АдресПоступление .lineno_) + 1 as NewLineNo_ " +
                    "FROM DT$АдресПоступление WHERE  DT$АдресПоступление .iddoc = :Doc";
                QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["IDDOC"].ToString());
                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                int NewLineNo_ = (int)DT.Rows[0]["NewLineNo_"];

                TextQuery =
                    "INSERT INTO DT$АдресПоступление VALUES " +
                    "(:Doc, :LineNo_, :Number, :Item, :Count, :EmptyID, :Coef, 1, :Employer, " +
                    ":Adress, :EmptyDate, :Time0, 1, :LabelCount, :UnitID, 0, 0, :PalletID); " +
                    "UPDATE DT$АдресПоступление " +
                    "SET $АдресПоступление.Количество = :RemainedCount" +
                    "WHERE DT$АдресПоступление .iddoc = :Doc and DT$АдресПоступление .lineno_ = :RemainedLineNo_";
                QuerySetParam(ref TextQuery, "Doc",         CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "LineNo_",     NewLineNo_);
                QuerySetParam(ref TextQuery, "Number",      CurrentRowAcceptedItem["Number"]);
                QuerySetParam(ref TextQuery, "Item",        CurrentRowAcceptedItem["ID"]);
                QuerySetParam(ref TextQuery, "Count",       AcceptedItem.AcceptCount);
                QuerySetParam(ref TextQuery, "EmptyID",     GetVoidID());
                QuerySetParam(ref TextQuery, "Coef",        1);
                QuerySetParam(ref TextQuery, "Employer",    Employer.ID);
                QuerySetParam(ref TextQuery, "Adress",      GetVoidID());
                QuerySetParam(ref TextQuery, "Date0",       DateTime.Now);
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());                            
                QuerySetParam(ref TextQuery, "Time0",       APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "LabelCount",  LabelCount);
                QuerySetParam(ref TextQuery, "UnitID",      CurrentRowAcceptedItem["Unit"]);
                QuerySetParam(ref TextQuery, "RemainedLineNo_", CurrentRowAcceptedItem["LINENO_"]);
                QuerySetParam(ref TextQuery, "RemainedCount",   BeginCount - AcceptedItem.AcceptCount);
                QuerySetParam(ref TextQuery, "PalletID",    FPalletID);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }
            else if (AlreadyDT.Rows.Count == 0 && AcceptedItem.AcceptCount >= BeginCount)
            {
                //Товар, будем писать в туже стоку
                TextQuery =
                    "UPDATE DT$АдресПоступление " +
                    "SET " +
                        "$АдресПоступление.Количество = :Count," +
                        "$АдресПоступление.Сотрудник0 = :Employer," +
                        "$АдресПоступление.Дата0 = :EmptyDate," + 
                        "$АдресПоступление.Время0 = :Time0," + 
                        "$АдресПоступление.Состояние0 = 1," + 
                        "$АдресПоступление.КоличествоЭтикеток = :LabelCount," + 
                        "$АдресПоступление.ФлагПечати = 1," +
                        "$АдресПоступление.Паллета = :PalletID " +
                    "WHERE " +
                        "DT$АдресПоступление .iddoc = :Doc " + 
                        "and DT$АдресПоступление .lineno_ = :LineNo_";
                QuerySetParam(ref TextQuery, "Count",           AcceptedItem.AcceptCount);
                QuerySetParam(ref TextQuery, "Employer",        Employer.ID);
                QuerySetParam(ref TextQuery, "Date0",           DateTime.Now);
                QuerySetParam(ref TextQuery, "EmptyDate",       GetVoidDate());
                QuerySetParam(ref TextQuery, "Time0",           APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "LabelCount",      LabelCount);
                QuerySetParam(ref TextQuery, "Doc",             CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "LineNo_",         CurrentRowAcceptedItem["LINENO_"]);
                QuerySetParam(ref TextQuery, "PalletID",        FPalletID);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }
            else if (AlreadyDT.Rows.Count > 0 && AcceptedItem.AcceptCount < BeginCount)
            {
                //Нуно создать новую строку на новую паллету
                TextQuery =
                    "SELECT max(DT$АдресПоступление .lineno_) + 1 as NewLineNo_ " +
                    "FROM DT$АдресПоступление WHERE  DT$АдресПоступление .iddoc = :Doc";
                QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["IDDOC"].ToString());
                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                int NewLineNo_ = (int)DT.Rows[0]["NewLineNo_"];

                TextQuery =
                    "INSERT INTO DT$АдресПоступление VALUES " +
                    "(:Doc, :LineNo_, :Number, :Item, :Count, :EmptyID, :Coef, 1, :Employer, " +
                    ":Adress, :EmptyDate, :Time0, 1, :LabelCount, :UnitID, 0, 0, :PalletID); " +
                    "UPDATE DT$АдресПоступление " +
                    "SET $АдресПоступление.Количество = :RemainedCount" +
                    "WHERE DT$АдресПоступление .iddoc = :Doc and DT$АдресПоступление .lineno_ = :RemainedLineNo_";
                QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "LineNo_", NewLineNo_);
                QuerySetParam(ref TextQuery, "Number", CurrentRowAcceptedItem["Number"]);
                QuerySetParam(ref TextQuery, "Item", CurrentRowAcceptedItem["ID"]);
                QuerySetParam(ref TextQuery, "Count", AcceptedItem.AcceptCount);
                QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
                QuerySetParam(ref TextQuery, "Coef", 1);
                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "Adress", GetVoidID());
                QuerySetParam(ref TextQuery, "Date0", DateTime.Now);
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                QuerySetParam(ref TextQuery, "Time0", APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "LabelCount", (int)(decimal)AlreadyDT.Rows[0]["LabelCount"]);
                QuerySetParam(ref TextQuery, "UnitID", CurrentRowAcceptedItem["Unit"]);
                QuerySetParam(ref TextQuery, "RemainedLineNo_", CurrentRowAcceptedItem["LINENO_"]);
                QuerySetParam(ref TextQuery, "RemainedCount", BeginCount - AcceptedItem.AcceptCount);
                QuerySetParam(ref TextQuery, "PalletID", FPalletID);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                //теперь еще обновим непринятые строки
                AllCountAccepted = (int)(decimal)AlreadyDT.Rows[0]["Count"] + AcceptedItem.AcceptCount;
                TextQuery =
                    "UPDATE DT$АдресПоступление " +
                    "SET " + 
                        "$АдресПоступление.Количество = :RemainedCount " +
                    "WHERE " +
                        "DT$АдресПоступление .iddoc = :Doc " + 
                        "and DT$АдресПоступление .lineno_ = :RemainedLineNo_";
                QuerySetParam(ref TextQuery, "Doc",             CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "RemainedLineNo_", CurrentRowAcceptedItem["LINENO_"]);
                QuerySetParam(ref TextQuery, "RemainedCount",   BeginCount - AcceptedItem.AcceptCount);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }
            else 
            {
                if ((short)AlreadyDT.Rows[0]["LineNo_"] == (int)CurrentRowAcceptedItem["LINENO_"])
                {
                    FExcStr = "Состояние позиции изменилось! Повторите приемку!";
                    return false;
                }
                //Уже есть строка принятого, будем писать в изначальную (не принятую)
                TextQuery =
                    "UPDATE DT$АдресПоступление " +
                    "SET " +
                        "$АдресПоступление.Количество = :Count," +
                        "$АдресПоступление.Сотрудник0 = :Employer," +
                        "$АдресПоступление.Дата0 = :EmptyDate," + 
                        "$АдресПоступление.Время0 = :Time0," + 
                        "$АдресПоступление.Состояние0 = 1," + 
                        "$АдресПоступление.КоличествоЭтикеток = :LabelCount," + 
                        "$АдресПоступление.ФлагПечати = 1," +
                        "$АдресПоступление.Паллета = :PalletID " +                   
                    "WHERE " + 
                        "DT$АдресПоступление .iddoc = :Doc " + 
                        "and DT$АдресПоступление .lineno_ = :LineNo_; ";
                QuerySetParam(ref TextQuery, "Count",           AcceptedItem.AcceptCount);
                QuerySetParam(ref TextQuery, "Employer",        Employer.ID);
                QuerySetParam(ref TextQuery, "Date0",           DateTime.Now);
                QuerySetParam(ref TextQuery, "EmptyDate",       GetVoidDate());
                QuerySetParam(ref TextQuery, "Time0",           APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "LabelCount",      (int)(decimal)AlreadyDT.Rows[0]["LabelCount"]);
                QuerySetParam(ref TextQuery, "Doc",             CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "LineNo_",         CurrentRowAcceptedItem["LINENO_"]);
                QuerySetParam(ref TextQuery, "PalletID",        FPalletID);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }
            //Выведем в строку состояния сколько мы приняли за этот раз
            int tmpCoef;
            if (!GetCoefPackage(AcceptedItem.ID, out tmpCoef))
            {
                tmpCoef = (int)CurrentRowAcceptedItem["Coef"];
            }
            FExcStr = AcceptedItem.InvCode.Trim() + " принят в количестве " +
                        GetStrPackageCount(AcceptedItem.AcceptCount, tmpCoef);
            //begin internal command
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(AcceptedItem.ID, "Спр.Товары");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(CurrentRowAcceptedItem["IDDOC"].ToString(), "АдресПоступление");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "AcceptItem (Принял товар)";
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход2"]       = AcceptedItem.AcceptCount;
            if (!ExecCommandNoFeedback("Internal", DataMapWrite))
            {
                return false;
            }
            //end internal command

            if (!ToModeAcceptance())
            {
                return false;
            }
            return QuitModeAcceptedItem();
        }
        public bool CompleteHarmonizationInicialize(string IDFrom)
        {
            FExcStr = null;
            string TextQuery = "BEGIN TRAN; " +
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
                        "$АдресПеремещение.СкладПолучатель = :IDFrom, " +
                        "$АдресПеремещение.ТипДокумента = 16 " +
                    "WHERE DH$АдресПеремещение .iddoc = @iddoc and DH$АдресПеремещение .$АдресПеремещение.ТипДокумента = 0; " +
                    "if @@rowcount > 0 begin " +
                        "COMMIT TRAN; " +
                        "select 1 as result; " +
                    "end else begin ROLLBACK TRAN; select 0 as result; end; " +
                "end else begin ROLLBACK TRAN; select 0 as result; end; ";
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
            QuerySetParam(ref TextQuery, "IDFrom", IDFrom);
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if ((int)DT.Rows[0]["result"] == 0)
            {
                FExcStr = "Не удалось захватить документ. Жмакните повторно!";
                return false;
            }

            //Тут должен быть переход в Transfer, но не будем передавать ИД-шник дока, пусть это сделает функция ниже
            return ToModeHarmonizationInicialize();

        }
        public bool CompleteTransferInicialize(string IDFrom, string IDIn)
        {
            FExcStr = null;
            //заполним склад выкладки
            WarehouseForAddressItem = new RefWarehouse(this);
            WarehouseForAddressItem.FoundID(IDIn);

            string TextQuery = "BEGIN TRAN; " +
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
                        "$АдресПеремещение.СкладПолучатель = :IDIn, " +
                        "$АдресПеремещение.ТипДокумента = 2 " +
                    "WHERE DH$АдресПеремещение .iddoc = @iddoc and DH$АдресПеремещение .$АдресПеремещение.ТипДокумента = 0; " +
                    "if @@rowcount > 0 begin " +
                        "COMMIT TRAN; " +
                        "select 1 as result; " +
                    "end else begin ROLLBACK TRAN; select 0 as result; end; " +
                "end else begin ROLLBACK TRAN; select 0 as result; end; ";
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
            QuerySetParam(ref TextQuery, "IDFrom", IDFrom);
            QuerySetParam(ref TextQuery, "IDIn", IDIn);
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if ((int)DT.Rows[0]["result"] == 0)
            {
                FExcStr = "Не удалось захватить документ. Жмакните повторно!";
                return false;
            }

            //Тут должен быть переход в Transfer, но не будем передавать ИД-шник дока, пусть это сделает функция ниже
            return ToModeTransferInicialize();
        }
        public bool CompleteChoiseInventory(string ID)
        {
            DataRow[] DR = WarehousesTo.Select("ID = '" + ID + "'");
            InventoryWarehouse = new RefWarehouse(this);
            InventoryWarehouse.FoundID(ID);
            return ToModeInventory();
        }
        public bool CompleteAcceptCross()
        {
            FExcStr = null;
            string TextQuery;

            int BeginCount = 0;

            if (CurrentMode != Mode.AcceptedItem)
            {
                FExcStr = "Недопустимая команда в текущем режиме!";
                return false;
            }

            if (CurrentRowAcceptedItem["PalletID"].ToString() == "" && PalletAcceptedItem == null)
            {
                //проверка на пустое значение адреса
                FExcStr = "Не заполнена паллета заказа!";
                return false;
            }
            if (PalletAcceptedItem == null)
            {
                //проверка на пустое значение адреса
                FExcStr = "Отсканируйте паллету!";
                return false;
            }
            if (PalletAcceptedItem.ID.ToString() != CurrentRowAcceptedItem["PalletID"].ToString() && CurrentRowAcceptedItem["PalletID"].ToString() != "")
            {
                //косяк, не тот адрес
                PalletAcceptedItem = null;
                FExcStr = "не та паллета! Надо " + CurrentRowAcceptedItem["PalletName"].ToString();
                return false;
            }
           
            if (AcceptedItem.Acceptance)
            {
                BeginCount = (int)CurrentRowAcceptedItem["Count"];  //Сколько в накладной изначально

                if (AcceptedItem.AcceptCount > (int)CurrentRowAcceptedItem["Count"])
                {
                    FExcStr = "Нельзя принять по данной накладной более " + CurrentRowAcceptedItem["Count"].ToString() + " штук!";
                    return false;
                }
                else if (AcceptedItem.AcceptCount == 0)
                {
                    FExcStr = "Нельзя принять нулевое количество!";
                    return false;
                }

                //Теперь проверим не поменялась ли ситуация в самой накладной, пока мы курили бамбук!
                TextQuery =
                    "SELECT " +
                        "ACDT.$АдресПоступление.Количество as Count " +
                    "FROM " +
                        "DT$АдресПоступление as ACDT (nolock) " +
                    "WHERE " +
                        "ACDT.iddoc = :Doc " +
                        "and ACDT.$АдресПоступление.Товар = :Item " +
                        "and ACDT.$АдресПоступление.Состояние0 = 0 " +
                        "and ACDT.lineno_ = :LineNo_ ";
                QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "Item", CurrentRowAcceptedItem["ID"]);
                QuerySetParam(ref TextQuery, "LineNo_", CurrentRowAcceptedItem["LINENO_"]);

                DataTable DTCurrState;
                if (!ExecuteWithRead(TextQuery, out DTCurrState))
                {
                    return false;
                }
                if (DTCurrState.Rows.Count == 0)
                {
                    FExcStr = "Недопустимое количество! Повторите приемку позиции!";
                    return false;
                }
                if ((int)(decimal)DTCurrState.Rows[0]["Count"] < AcceptedItem.AcceptCount)
                {
                    FExcStr = "Недопустимое количество! Повторите приемку позиции!";
                    return false;
                }
                //Скорректируем начальное количество
                BeginCount = (int)(decimal)DTCurrState.Rows[0]["Count"];
            }

            int NeedNew = 0;
            int CoefPlace = 0;  //Коэффициент мест, по нему будет расчитывать количество этикеток
            DataTable tmpDT = new DataTable();
            tmpDT.Columns.Add("Coef", Type.GetType("System.Int32"));
            tmpDT.Columns.Add("OKEI", Type.GetType("System.String"));
            foreach (DataRow dr in FUnits.Rows)
            {
                if ((int)dr["Coef"] == 1 && dr["OKEI"].ToString() != OKEIUnit)
                {
                    FExcStr = "Коэффициент 1 может быть только у штуки! Пожалуйста исправьте...";
                    return false;
                }
                if (dr["OKEI"].ToString() == OKEIPackage)
                {
                    CoefPlace = (int)dr["Coef"];
                }
                if (dr["ID"].ToString() != GetVoidID())
                {
                    //Имеющаяся единица
                    if ((int)dr["Coef"] == 0)
                    {
                        TextQuery =
                            "UPDATE $Спр.ЕдиницыШК " +
                            "SET " +
                                "ismark = 1 " +
                            "WHERE $Спр.ЕдиницыШК .id = :ID ";
                        QuerySetParam(ref TextQuery, "ID", dr["ID"]);
                    }
                    else
                    {
                        TextQuery =
                            "UPDATE $Спр.ЕдиницыШК " +
                            "SET " +
                                "$Спр.ЕдиницыШК.Штрихкод = :Barcode, " +
                                "$Спр.ЕдиницыШК.Коэффициент = :Coef, " +
                                "$Спр.ЕдиницыШК.ОКЕИ = :OKEI, " +
                                "$Спр.ЕдиницыШК.ФлагРегистрацииМОД = 1 " +
                            "WHERE $Спр.ЕдиницыШК .id = :ID ";
                        QuerySetParam(ref TextQuery, "Barcode", dr["Barcode"]);
                        QuerySetParam(ref TextQuery, "Coef", dr["Coef"]);
                        QuerySetParam(ref TextQuery, "ID", dr["ID"]);
                        QuerySetParam(ref TextQuery, "OKEI", dr["OKEI"]);
                    }
                    if (!ExecuteWithoutRead(TextQuery))
                    {
                        return false;
                    }
                }
                else
                {
                    DataRow[] tmpDR = tmpDT.Select("Coef = " + dr["Coef"].ToString() + " and OKEI = '" + dr["OKEI"] + "'");
                    if (dr["Barcode"].ToString().Trim() != "" || tmpDR.Length == 0)
                    {
                        NeedNew++;
                    }
                }
                DataRow tmpdr = tmpDT.NewRow();
                tmpdr["Coef"] = (int)dr["Coef"];
                tmpdr["OKEI"] = dr["OKEI"].ToString();
                tmpDT.Rows.Add(tmpdr);
            }
            tmpDT.Clear();

            if (NeedNew > 0)
            {
                //Есть новые...
                int CurrentRow = 0;
                //Теперь также пишем новые
                foreach (DataRow dr in FUnits.Rows)
                {
                    if ((int)dr["Coef"] == 0)
                    {
                        continue;
                    }
                    if (dr["ID"].ToString() == GetVoidID())
                    {
                        DataRow[] tmpDR = tmpDT.Select("Coef = " + dr["Coef"].ToString() + " and OKEI = '" + dr["OKEI"] + "'");
                        if (!(dr["Barcode"].ToString().Trim() != "" || tmpDR.Length == 0))
                        {
                            continue;
                        }
                        if (!CreateUnit(AcceptedItem.ID, dr["OKEI"].ToString(), (int)dr["Coef"], dr["Barcode"].ToString()))
                        {
                            return false;
                        }
                        CurrentRow++;
                    }
                    DataRow tmpdr = tmpDT.NewRow();
                    tmpdr["Coef"] = (int)dr["Coef"];
                    tmpdr["OKEI"] = dr["OKEI"].ToString();
                    tmpDT.Rows.Add(tmpdr);
                }
            }
            //Запишем норму упаковки в товар
            string PackNorm = "";
            AddPackNorm(ref PackNorm, OKEIKit);
            AddPackNorm(ref PackNorm, OKEIPack);
            AddPackNorm(ref PackNorm, OKEIPackage);
            if (PackNorm == "")
            {
                PackNorm = "1";
            }
            else
            {
                PackNorm = PackNorm.Substring(1);  //Отрезаем первый символ "/"
            }
            if (AcceptedItem.MinParty > 0)
            {
                PackNorm = ">" + AcceptedItem.MinParty.ToString() + "/" + PackNorm;
            }
            TextQuery =
                "UPDATE $Спр.Товары " +
                    "SET $Спр.Товары.НормаУпаковки = :PackNorm , " +
                        "$Спр.Товары.КоличествоДеталей = :Details " +
                "WHERE $Спр.Товары .id = :ItemID";
            QuerySetParam(ref TextQuery, "ItemID", AcceptedItem.ID);
            QuerySetParam(ref TextQuery, "PackNorm", PackNorm);
            QuerySetParam(ref TextQuery, "Details", AcceptedItem.NowDetails);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }

            //Если ШК с базовой единицы слетел, то поставим его назад этим чудовищным запросом
            TextQuery =
                "declare @idbase char(9) " +
                "declare @idbar char(9) " +
                "declare @barcode varchar(50) " +
                "select @idbase = RefE.id from $Спр.Товары as RefG (nolock) " +
                    "inner join $Спр.ЕдиницыШК as RefE (nolock) " +
                        "on RefE.id = RefG.$Спр.Товары.БазоваяЕдиницаШК " +
                    "where RefG.id = :ItemID and RefE.$Спр.ЕдиницыШК.Штрихкод = ''; " +
                "if @idbase is not null begin " +
                    "select @idbar = RefE.id, @barcode = RefE.$Спр.ЕдиницыШК.Штрихкод from $Спр.ЕдиницыШК as RefE (nolock) " +
                        "where " +
                            "RefE.parentext = :ItemID " +
                            "and RefE.$Спр.ЕдиницыШК.ОКЕИ = :OKEI " +
                            "and not RefE.$Спр.ЕдиницыШК.Штрихкод = ''; " +
                    "if @idbar is not null begin " +
                        "begin tran; " +
                        "update $Спр.ЕдиницыШК " +
                            "set $Спр.ЕдиницыШК.Штрихкод = @barcode " +
                            "where id = @idbase; " +
                        "if @@rowcount = 0 " +
                            "rollback tran " +
                        "else begin " +
                            "update $Спр.ЕдиницыШК " +
                                "set $Спр.ЕдиницыШК.Штрихкод = '' " +
                                "where id = @idbar; " +
                            "if @@rowcount = 0 " +
                                "rollback tran " +
                            "else " +
                                "commit tran " +
                        "end " +
                    "end " +
                "end";
            QuerySetParam(ref TextQuery, "OKEI", OKEIUnit);
            QuerySetParam(ref TextQuery, "ItemID", AcceptedItem.ID);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            //Конец хуйни с заменой ШК у базовой единицы

            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            if (!AcceptedItem.Acceptance)
            {
                //begin internal command
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Employer.ID, "Спр.Сотрудники");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"] = ExtendID(AcceptedItem.ID, "Спр.Товары");
                DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = "SaveItem (Сохранил единицы)";
                if (!ExecCommandNoFeedback("Internal", DataMapWrite))
                {
                    return false;
                }
                //end internal command
                //Это не приемка, а просто правка карточки
                FExcStr = AcceptedItem.InvCode.Trim() + " карточка товара сохранена!";
                if (AcceptedItem.ToMode == Mode.AcceptanceCross)
                {
                    if (!ToModeAcceptanceCross())
                    {
                        return false;
                    }
                    return QuitModeAcceptedItem();
                }               
            }

            //ТЕПЕРЬ ПОЕХАЛА ЗАПИСЬ ДОКУМЕНТА
            //Расчитаем число этикеток
            int LabelCount;
            if (FlagBarcode != 0)
            {
                LabelCount = 0;
            }
            else
            {
                LabelCount = 1;
                //if (CoefPlace > 0 && CountAccepted >= CoefPlace)
                //{
                //    LabelCount = (int)CountAccepted/CoefPlace;
                //}
            }

            //Для начала подсосем есть ли уже принятые и не напечатанные строки в накладной
            TextQuery =
                "SELECT " +
                    "ACDT.LineNo_ as LineNo_, " +
                    "ACDT.$АдресПоступление.Количество as Count, " +
                    "ACDT.$АдресПоступление.КоличествоЭтикеток as LabelCount " +
                "FROM " +
                    "DT$АдресПоступление as ACDT (nolock) " +
                "WHERE " +
                    "ACDT.iddoc = :Doc " +
                    "and ACDT.$АдресПоступление.Товар = :Item " +
                    "and ACDT.$АдресПоступление.ФлагПечати = 1 " +
                    "and ACDT.$АдресПоступление.Сотрудник0 = :Employer";
            QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["IDDOC"]);
            QuerySetParam(ref TextQuery, "Item", CurrentRowAcceptedItem["ID"]);
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            DataTable AlreadyDT;
            if (!ExecuteWithRead(TextQuery, out AlreadyDT))
            {
                return false;
            }

            int AllCountAccepted = AcceptedItem.AcceptCount;
            if (AlreadyDT.Rows.Count == 0 && AcceptedItem.AcceptCount < BeginCount)
            {
                //Нуно создать новую строку
                TextQuery =
                    "SELECT max(DT$АдресПоступление .lineno_) + 1 as NewLineNo_ " +
                    "FROM DT$АдресПоступление WHERE  DT$АдресПоступление .iddoc = :Doc";
                QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["IDDOC"].ToString());
                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                int NewLineNo_ = (int)DT.Rows[0]["NewLineNo_"];

                TextQuery =
                    "INSERT INTO DT$АдресПоступление VALUES " +
                    "(:Doc, :LineNo_, :Number, :Item, :Count, :EmptyID, :Coef, 1, :Employer, " +
                    ":Adress, :Date0, :Time0, 1, :LabelCount, :UnitID, 0, 0, :EmptyID); " +
                    "UPDATE DT$АдресПоступление " +
                    "SET $АдресПоступление.Количество = :RemainedCount " +
                    "WHERE DT$АдресПоступление .iddoc = :Doc and DT$АдресПоступление .lineno_ = :RemainedLineNo_";
                QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "LineNo_", NewLineNo_);
                QuerySetParam(ref TextQuery, "Number", CurrentRowAcceptedItem["Number"]);
                QuerySetParam(ref TextQuery, "Item", CurrentRowAcceptedItem["ID"]);
                QuerySetParam(ref TextQuery, "Count", AcceptedItem.AcceptCount);
                QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
                QuerySetParam(ref TextQuery, "Coef", 1);
                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "Adress", GetVoidID());
                QuerySetParam(ref TextQuery, "Date0", DateTime.Now);
                QuerySetParam(ref TextQuery, "Time0", APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "LabelCount", LabelCount);
                QuerySetParam(ref TextQuery, "UnitID", CurrentRowAcceptedItem["Unit"]);
                QuerySetParam(ref TextQuery, "RemainedLineNo_", CurrentRowAcceptedItem["LINENO_"]);
                QuerySetParam(ref TextQuery, "RemainedCount", BeginCount - AcceptedItem.AcceptCount);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }
            else if (AlreadyDT.Rows.Count == 0 && AcceptedItem.AcceptCount >= BeginCount)
            {
                //Товар, будем писать в туже стоку
                TextQuery =
                    "UPDATE DT$АдресПоступление " +
                    "SET " +
                        "$АдресПоступление.Количество = :Count," +
                        "$АдресПоступление.Сотрудник0 = :Employer," +
                        "$АдресПоступление.Дата0 = :Date0," +
                        "$АдресПоступление.Время0 = :Time0," +
                        "$АдресПоступление.Состояние0 = 1," +
                        "$АдресПоступление.КоличествоЭтикеток = :LabelCount," +
                        "$АдресПоступление.ФлагПечати = 1 " +
                    "WHERE " +
                        "DT$АдресПоступление .iddoc = :Doc " +
                        "and DT$АдресПоступление .lineno_ = :LineNo_";
                QuerySetParam(ref TextQuery, "Count", AcceptedItem.AcceptCount);
                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "Date0", DateTime.Now);
                QuerySetParam(ref TextQuery, "Time0", APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "LabelCount", LabelCount);
                QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "LineNo_", CurrentRowAcceptedItem["LINENO_"]);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }
            else if (AlreadyDT.Rows.Count > 0 && AcceptedItem.AcceptCount < BeginCount)
            {
                //Уже есть строка принятого, будем писать в нее и не принятую строчку тоже оставим
                AllCountAccepted = (int)(decimal)AlreadyDT.Rows[0]["Count"] + AcceptedItem.AcceptCount;
                TextQuery =
                    "UPDATE DT$АдресПоступление " +
                    "SET " +
                        "$АдресПоступление.Количество = :Count," +
                        "$АдресПоступление.Дата0 = :Date0," +
                        "$АдресПоступление.Время0 = :Time0," +
                        "$АдресПоступление.КоличествоЭтикеток = :LabelCount " +
                    "WHERE " +
                        "DT$АдресПоступление .iddoc = :Doc " +
                        "and DT$АдресПоступление .lineno_ = :LineNo_; " +
                    "UPDATE DT$АдресПоступление " +
                    "SET " +
                        "$АдресПоступление.Количество = :RemainedCount " +
                    "WHERE " +
                        "DT$АдресПоступление .iddoc = :Doc " +
                        "and DT$АдресПоступление .lineno_ = :RemainedLineNo_";
                QuerySetParam(ref TextQuery, "Count", AllCountAccepted);
                QuerySetParam(ref TextQuery, "Date0", DateTime.Now);
                QuerySetParam(ref TextQuery, "Time0", APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "LabelCount", (int)(decimal)AlreadyDT.Rows[0]["LabelCount"]);
                QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "LineNo_", AlreadyDT.Rows[0]["LineNo_"]);
                QuerySetParam(ref TextQuery, "RemainedLineNo_", CurrentRowAcceptedItem["LINENO_"]);
                QuerySetParam(ref TextQuery, "RemainedCount", BeginCount - AcceptedItem.AcceptCount);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }
            else
            {
                if ((short)AlreadyDT.Rows[0]["LineNo_"] == (int)CurrentRowAcceptedItem["LINENO_"])
                {
                    FExcStr = "Состояние позиции изменилось! Повторите приемку!";
                    return false;
                }
                //Уже есть строка принятого, будем писать в изначальную (не принятую) и вдобавок удалим принятую
                AllCountAccepted = (int)(decimal)AlreadyDT.Rows[0]["Count"] + AcceptedItem.AcceptCount;
                TextQuery =
                    "UPDATE DT$АдресПоступление " +
                    "SET " +
                        "$АдресПоступление.Количество = :Count," +
                        "$АдресПоступление.Сотрудник0 = :Employer," +
                        "$АдресПоступление.Дата0 = :Date0," +
                        "$АдресПоступление.Время0 = :Time0," +
                        "$АдресПоступление.Состояние0 = 1," +
                        "$АдресПоступление.КоличествоЭтикеток = :LabelCount," +
                        "$АдресПоступление.ФлагПечати = 1 " +
                    "WHERE " +
                        "DT$АдресПоступление .iddoc = :Doc " +
                        "and DT$АдресПоступление .lineno_ = :LineNo_; " +
                    "DELETE FROM DT$АдресПоступление " +
                    "WHERE " +
                        "DT$АдресПоступление .lineno_ = :DeleteLineNo_ " +
                        "and DT$АдресПоступление .iddoc = :Doc;";
                QuerySetParam(ref TextQuery, "Count", AllCountAccepted);
                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "Date0", DateTime.Now);
                QuerySetParam(ref TextQuery, "Time0", APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "LabelCount", (int)(decimal)AlreadyDT.Rows[0]["LabelCount"]);
                QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["IDDOC"]);
                QuerySetParam(ref TextQuery, "LineNo_", CurrentRowAcceptedItem["LINENO_"]);
                QuerySetParam(ref TextQuery, "DeleteLineNo_", AlreadyDT.Rows[0]["LineNo_"]);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }

            //теперь запишем адрес в заказ на клиента если его там нет
            if (CurrentRowAcceptedItem["OrderID"].ToString() == "На склад")
            {

            }
            else
            {
                if (CurrentRowAcceptedItem["PalletID"].ToString() == "")
                {
                    //не указан, значить надо записать его туда
                    TextQuery =
                       "UPDATE DH$ЗаказНаКлиента " +
                       "SET " +
                           "$ЗаказНаКлиента.Паллета = :Pallet " +
                       "WHERE " +
                           "DH$ЗаказНаКлиента .iddoc = :Doc ";
                    QuerySetParam(ref TextQuery, "Pallet", PalletAcceptedItem.ID);
                    QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["OrderID"]);
                    if (!ExecuteWithoutRead(TextQuery))
                    {
                        return false;
                    }
                    //почистим паллетки от греха
                    FPalletID = "";
                    FBarcodePallet = "";
                }
                //теперь обновим данные по принятому количеству по заказу
                TextQuery =
                    "UPDATE DT$ЗаказНаКлиента " +
                    "SET " +
                         "$ЗаказНаКлиента.Принято = $ЗаказНаКлиента.Принято + :Count " +
                    "WHERE " +
                        "DT$ЗаказНаКлиента .iddoc = :Doc " +
                        "and $ЗаказНаКлиента.Товар = :Item";
                QuerySetParam(ref TextQuery, "Item", CurrentRowAcceptedItem["ID"]);
                QuerySetParam(ref TextQuery, "Count", AcceptedItem.AcceptCount);
                QuerySetParam(ref TextQuery, "Doc", CurrentRowAcceptedItem["OrderID"]);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
            }
            
            PalletAcceptedItem = null;

            //Выведем в строку состояния сколько мы приняли за этот раз
            int tmpCoef;
            if (!GetCoefPackage(AcceptedItem.ID, out tmpCoef))
            {
                tmpCoef = (int)CurrentRowAcceptedItem["Coef"];
            }
            FExcStr = AcceptedItem.InvCode.Trim() + " принят в количестве " +
                        GetStrPackageCount(AcceptedItem.AcceptCount, tmpCoef);
            //begin internal command
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"] = ExtendID(AcceptedItem.ID, "Спр.Товары");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"] = ExtendID(CurrentRowAcceptedItem["IDDOC"].ToString(), "АдресПоступление");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = "AcceptItem (Принял товар)";
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход2"] = AcceptedItem.AcceptCount;
            if (!ExecCommandNoFeedback("Internal", DataMapWrite))
            {
                return false;
            }
            //end internal command
            if (!ToModeAcceptanceCross())
            {
                return false;
            }
            return QuitModeAcceptedItem();
        }
        
        public bool PrintLabels(bool Condition)
        {
            FExcStr = null;
            if (FConsignment.Rows.Count == 0)
            {
                FExcStr = "Не выбраны накладные для приемки!";
                return false;
            }
            if (FAcceptedItems.Rows.Count == 0)
            {
                FExcStr = "Нет принятых товаров в текущей сессии!";
                return false;
            }
            //Формируем строку с ид-шниками АдресовПоступления
            string strACID = "";
            for (int i = 0; i < FConsignment.Rows.Count; i++)
            {
                strACID += FConsignment.Rows[i]["ACID"].ToString().Trim() + ",";
            }
            strACID = strACID.Substring(0, strACID.Length - 1);

            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = strACID;
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(Printer.ID, "Спр.Принтеры");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");

            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("AdressAcceptance" + (Condition ? "Condition" : ""), DataMapWrite, FieldList, out DataMapRead ))
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
            if (ToModeAcceptance())
            {
                FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool PrintLabelsCross(bool Condition)
        {
            FExcStr = null;
            PalletAcceptedItem = null;
            if (FConsignment.Rows.Count == 0)
            {
                FExcStr = "Не выбраны накладные для приемки!";
                return false;
            }
            if (FAcceptedItems.Rows.Count == 0)
            {
                FExcStr = "Нет принятых товаров в текущей сессии!";
                return false;
            }
            if (!Printer.Selected)
            {
                FExcStr = "Принтер не выбран!";
                return false;
            }
            //Формируем строку с ид-шниками АдресовПоступления
            string strACID = "";
            for (int i = 0; i < FConsignment.Rows.Count; i++)
            {
                strACID += FConsignment.Rows[i]["ACID"].ToString().Trim() + ",";
            }
            strACID = strACID.Substring(0, strACID.Length - 1);

            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = strACID;
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход2"] = Condition.ToString();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"] = ExtendID(Printer.ID, "Спр.Принтеры");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Employer.ID, "Спр.Сотрудники");

            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("CompleteAcceptanceCross", DataMapWrite, FieldList, out DataMapRead))
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
            if (ToModeAcceptanceCross())
            {
                FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
                return true;
            }
            else
            {
                return false;
            }            
        }
        public bool ChangeLabelCount(int currRow, int LabelCount)
        {
            FExcStr = null;
            if (CurrentMode == Mode.AcceptanceCross)
            {
                string TextQuery =
                    "UPDATE DH$ЗаказНаКлиента " +
                    "SET $ЗаказНаКлиента.КолМест = :LabelCount " +
                    "WHERE DH$ЗаказНаКлиента .iddoc = :Doc";
                QuerySetParam(ref TextQuery, "LabelCount", LabelCount);
                QuerySetParam(ref TextQuery, "Doc", FAcceptedItems.Rows[currRow]["OrderID"]);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }

                for (int row = 0; row < FAcceptedItems.Rows.Count; row++)
                {
                    if (FAcceptedItems.Rows[row]["OrderID"].ToString() == FAcceptedItems.Rows[currRow]["OrderID"].ToString())
                    {
                        FAcceptedItems.Rows[row]["BoxCount"] = LabelCount;
                    }
                }
                return true;
            }
            else
            {
                string TextQuery =
                    "UPDATE DT$АдресПоступление " +
                    "SET $АдресПоступление.КоличествоЭтикеток = :LabelCount " +
                    "WHERE DT$АдресПоступление .iddoc = :Doc and DT$АдресПоступление .lineno_ = :LineNo_";
                //"UPDATE DT$АдресПоступление " +
                //"SET $АдресПоступление.КоличествоЭтикеток = 0 " +
                //"WHERE " + 
                //    "DT$АдресПоступление .iddoc = :Doc " +
                //    " and not DT$АдресПоступление .lineno_ = :LineNo_ " + 
                //    " and DT$АдресПоступление .$АдресПоступление.Товар = :ItemID " +
                //    " and DT$Адреспоступление .$АдресПоступление.ФлагПечати = 1;";
                QuerySetParam(ref TextQuery, "LabelCount", LabelCount);
                QuerySetParam(ref TextQuery, "Doc", FAcceptedItems.Rows[currRow]["IDDOC"]);
                QuerySetParam(ref TextQuery, "LineNo_", FAcceptedItems.Rows[currRow]["LineNo_"]);
                QuerySetParam(ref TextQuery, "ItemID", FAcceptedItems.Rows[currRow]["ID"]);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                FAcceptedItems.Rows[currRow]["LabelCount"] = LabelCount;
                return true;
            }
        }
        private bool EngineChangeBarcode(int Coef, string Barcode, string OKEI)
        {
            //FExcStr - несет информацию
            FExcStr = "Штрихкод " + Barcode + " принят!";   //По умолчанию так
            //Поиск по ШК
            DataRow[] DR = FUnits.Select("Barcode = '" + Barcode + "'");
            if (NewBarcodes.Contains(Barcode))
            {
                NewBarcodes.Remove(Barcode);
            }
            NewBarcodes.Add(Barcode);
            foreach(DataRow dr in DR)
            {
                string currOKEI = dr["OKEI"].ToString();
                if ((int)dr["Coef"] == Coef && currOKEI == OKEI)
                {
                    FExcStr = "Штрихкод " + Barcode + " уже задан у данной единицы!";
                    return true; //С этим коэффициентом и штрихкодом и ОКЕИ уже есть единица
                }
                else if (currOKEI == OKEIUnit || currOKEI == OKEIPack || currOKEI == OKEIPackage || currOKEI == OKEIKit)
                {
                    //Из специальной - ЛЮБУЮ (нельзя менять коэффициент)
                    if (dr["ID"].ToString() == GetVoidID())
                    {
                        dr["Barcode"] = "";
                        break;
                    }
                    //Это уже существующая единица, нужно попробывать "пересосать" ШК из несуществующей
                    bool yes = false;
                    DataRow[] tmpDR = FUnits.Select("OKEI = '" + currOKEI + "'");
                    foreach (DataRow tmpdr in tmpDR)
                    {
                        if (tmpdr["ID"].ToString() == GetVoidID() && tmpdr["Barcode"].ToString().Trim() != "")
                        {
                            dr["Barcode"]       = tmpdr["Barcode"];
                            tmpdr["Barcode"]    = "";
                            yes = true;
                            break;
                        }
                    }
                    if (!yes)
                    {
                        dr["Barcode"] = "";
                    }
                    break;
                }
                else
                {
                    //Из НЕ специальной - ЛЮБУЮ
                    if ((int)dr["Coef"] == Coef)
                    {
                        //Коэффициенты совпадают - просто изменим ОКЕИ
                        dr["OKEI"] = OKEI;
                        return true;
                    }
                    //Если не совпадают, то нужно очистить баркод
                    //ТУТ ТУПОЧИСТИМ, Т.К. ВОЗМОЖНОСТИ ЗАВОДИТЬ НОВЫЕ ГОВНОЕДЕНИЦИ МЫ НЕ СДЕЛАЛИ ЕЩЕ
                    dr["Barcode"] = "";
                    break;
                }
            }

            //Поиск по коэффициенту (Среди НЕ новых единиц - они приоритетные)
            DR = FUnits.Select("Coef = " + Coef.ToString());
            foreach(DataRow dr in DR)
            {
                string currOKEI = dr["OKEI"].ToString();
                if (dr["Barcode"].ToString().Trim() == "" && currOKEI == OKEI && dr["ID"].ToString() != GetVoidID())
                {
                    //есть единица с пустым штрихкодом - будем использовать ее
                    dr["Barcode"] = Barcode;
                    return true;
                }
            }
            //Поиск по коэффициенту
            DR = FUnits.Select("Coef = " + Coef.ToString());
            foreach(DataRow dr in DR)
            {
                string currOKEI = dr["OKEI"].ToString();
                if (dr["Barcode"].ToString().Trim() == "" && currOKEI == OKEI)
                {
                    //есть единица с пустым штрихкодом - будем использовать ее
                    dr["Barcode"] = Barcode;
                    return true;
                }
            }
            //Поиск по ОКЕИ (Среди НЕ новых единиц - они приоритетные)
            DR = FUnits.Select("OKEI = '" + OKEI + "'");
            foreach(DataRow dr in DR)
            {
                string currOKEI = dr["OKEI"].ToString();
                if (dr["Barcode"].ToString().Trim() == "" && currOKEI != OKEIUnit && dr["ID"].ToString() != GetVoidID())
                {
                    //Есть с пустым ШК и другим коэффициентом, имзеним это...
                    dr["Barcode"] = Barcode;
                    dr["Coef"] = Coef;
                    return true;
                }
            }
            //Поиск по ОКЕИ
            DR = FUnits.Select("OKEI = '" + OKEI + "'");
            foreach(DataRow dr in DR)
            {
                string currOKEI = dr["OKEI"].ToString();
                if (dr["Barcode"].ToString().Trim() == "" && currOKEI != OKEIUnit)
                {
                    //Есть с пустым ШК и другим коэффициентом, имзеним это...
                    dr["Barcode"] = Barcode;
                    dr["Coef"] = Coef;
                    return true;
                }
            }
            //Нельзя использовать имеющуюся, будем создавать новую
            DataRow newRow = FUnits.NewRow();
            newRow["ID"] = GetVoidID();
            newRow["Coef"] = Coef;
            newRow["Barcode"] = Barcode;
            newRow["OKEI"] = OKEI;
            FUnits.Rows.Add(newRow);
            return true;
        }
        public bool ChangeUnitBarcode(int Coef, string Barcode, string OKEI)
        {
            FExcStr = null;
            if (Coef == 0)
            {
                FExcStr = "Нельзя задать ШК единице с нулевым коэффициентом!";
                return false;
            }

            bool result = EngineChangeBarcode(Coef, Barcode, OKEI);

            if (NewBarcodes.Count > 0)
            {
                FFlagBarcode = 1;
                //may bee flag even better? 
                DataRow[] DR = FUnits.Select("OKEI = '" + OKEIPackage + "'");
                foreach (DataRow dr in DR)
                {
                    if (NewBarcodes.Contains(dr["Barcode"].ToString().Trim()))
                    {
                        FFlagBarcode = 2;
                        break;
                    }
                }
            }
            return result;
        }
        public bool ChangeUnitCoef(int Coef, string Barcode, string OKEI)
        {
            FExcStr = null;
            if (OKEI == OKEIUnit)
            {
                FExcStr = "Нельзя менять коэффициент базовой единицы!";
                return false;
            }
            DataRow[] DR;
            if (OKEI == OKEIPack || OKEI == OKEIPackage || OKEI == OKEIKit)
            {
                //Это единица специального вида - меняем коэффициент у всех единиц такого вида
                DR = FUnits.Select("OKEI = '" + OKEI + "'");
                foreach(DataRow dr in DR)
                {
                    dr["Coef"] = Coef;
                    return true;
                }
                //Не найдено...
                if (Coef != 0)
                {
                    DataRow newRow = FUnits.NewRow();
                    newRow["ID"]        = GetVoidID();
                    newRow["Coef"]      = Coef;
                    newRow["OKEI"]      = OKEI;
                    newRow["Barcode"]   = Barcode;
                    FUnits.Rows.Add(newRow);
                }
            }
            else
            {
                //меняем коэффициент у всех говноединицс с таким ШК, по идее это всегда одна единица, т.к. ШК - уникален!
                DR = FUnits.Select("Barcode = '" + Barcode + "'");
                foreach(DataRow dr in DR)
                {
                    if (!(dr["OKEI"].ToString() == OKEIPack || dr["OKEI"].ToString() == OKEIPackage || dr["OKEI"].ToString() == OKEIUnit || dr["OKEI"].ToString() == OKEIKit))
                    {
                        dr["Coef"] = Coef;
                    }
                }
                //Ввод новых для говноединиц пока не поддерживаем
            }
            return true;
        }
        public bool ScanPalletBarcode(string strBarcode)
        {
           
            string TextQuery = "declare @result char(9); exec WPM_GetIDNewPallet :Barcode, :Employer, @result out; select @result;";
            SQL1S.QuerySetParam(ref TextQuery, "Barcode", strBarcode);
            SQL1S.QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            FBarcodePallet = strBarcode;
            FPalletID = (string)ExecuteScalar(TextQuery);

            if (FPalletID == SQL1S.GetVoidID())
            {
                return false;
            }           

            //запишем данные в нее
            TextQuery =
                    "UPDATE $Спр.ПеремещенияПаллет " +
                    "SET " +
                        "$Спр.ПеремещенияПаллет.Сотрудник1 = :EmployerID, " +
                        "$Спр.ПеремещенияПаллет.ФлагОперации = 1, " +
                        "$Спр.ПеремещенияПаллет.Дата10 = :Date, " +
                        "$Спр.ПеремещенияПаллет.Время10 = :Time, " +
                        "$Спр.ПеремещенияПаллет.ТипДвижения = 4 " +
                    "WHERE $Спр.ПеремещенияПаллет .id = :Pallet ";
            SQL1S.QuerySetParam(ref TextQuery, "EmptyDate", SQL1S.GetVoidDate());
            SQL1S.QuerySetParam(ref TextQuery, "EmployerID", Employer.ID);
            SQL1S.QuerySetParam(ref TextQuery, "Date", DateTime.Now);
            SQL1S.QuerySetParam(ref TextQuery, "Time", APIManager.NowSecond());
            SQL1S.QuerySetParam(ref TextQuery, "Pallet", FPalletID);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            DataRow[] DR = FPallets.Select("ID = '" + FPalletID + "'");
            if (DR.Length == 0)
            {
                //нет строки, значит надо добавить
                DataRow tmpdr = FPallets.NewRow();
                tmpdr["ID"] = FPalletID;
                tmpdr["Barcode"] = strBarcode;
                tmpdr["Name"] = strBarcode.Substring(8, 4);
                tmpdr["AdressID"] = SQL1S.GetVoidID();
                FPallets.Rows.Add(tmpdr);
            }          
            return true;

        } // ScanPartBarcode
        private string GetStrPackageCount(int Count, int Coef)
        {
            string result = Count.ToString() + " ШТУК"; //По умолчанию штуки
            if (Coef > 1)
            {
                if (((int)(Count/Coef))*Coef == Count)
                {
                    //Делится по коробкам
                    Count   = Count/Coef;
                    result  = Count.ToString() + " м.";
                }
            }
            return result;
        }
        public bool AddPartyParty(string IDD, int LineNo)
        {
            FExcStr = null;
            string TextQuery =
                "SELECT " +
                    "DocAC.$АдресПоступление.Товар as ID, " + 
                    "journ.iddoc as IDDOC " + 
                "FROM " +
                    "_1sjourn as journ (nolock) " + 
                    "LEFT JOIN DT$АдресПоступление as DocAC (nolock) " +
                        " ON journ.iddoc = DocAC.iddoc " +
                "WHERE " + 
                    "journ.$IDD = :IDD " + 
                    "and DocAC.LineNo_ = :LineNo_ " + 
                    "and not DocAC.$АдресПоступление.Дата0 = :EmptyDate ";
            QuerySetParam(ref TextQuery, "IDD", IDD);
            QuerySetParam(ref TextQuery, "LineNo_", LineNo);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count == 0)
            {
                FExcStr = "Партия не принята или ошибочный ШК!";
                return false;
            }
            
            if (CurrentMode == Mode.Transfer || CurrentMode == Mode.NewInventory)
            {
                return CheckParty(DT.Rows[0]["ID"].ToString());
            }
            else if (CurrentMode == Mode.Acceptance)
            {
                return ToModeAcceptedItem(DT.Rows[0]["ID"].ToString(), DT.Rows[0]["IDDOC"].ToString());
            }
            else if (CurrentMode == Mode.AcceptanceCross)
            {
                return ToModeAcceptedItem(DT.Rows[0]["ID"].ToString(), DT.Rows[0]["IDDOC"].ToString());
            }
            else if (CurrentMode == Mode.Inventory)
            {
                return ToModeAcceptedItem(DT.Rows[0]["ID"].ToString(), "", Mode.Inventory);
            }
            else
            {
                FExcStr = "Нет действий с ШК партии в данном режиме!";
                return false;
            }
        }
        public bool CheckParty(string ItemID)
        {
            //FExcStr - несет смысл
            if (FTransferItems.Rows.Count >= 10 && CurrentMode != Mode.NewInventory)
            {
                FExcStr = "Нельзя больше 10 строк в тележку!!!";
                return false;
            }
            OkRowIndex  = -1;
            int Count   = 0;
            DataRow[] DR = FTransferItems.Select("ID = '" + ItemID + "'");
            foreach (DataRow dr in DR)
            {
                Count   += (int)(decimal)dr["Coef"]*(int)(decimal)dr["CountPackage"];
            }
            if (Count > 0)
            {
                //Если в телеге хоть что-то аналогичное есть, то запрещаем, если край приперло - то пусть удаляет все
                FExcStr     = DR[0]["InvCode"].ToString().Trim() + " - есть в тележке " + GetStrPackageCount(Count, (int)(decimal)DR[0]["Coef"]);
                CurrentLine = FTransferItems.Rows.IndexOf(DR[0]);
                OkRowIndex  = CurrentLine;
                return ToModeAcceptedItem(ItemID, "", CurrentMode, Count, true);
            }

            if (CurrentMode == Mode.NewInventory)
            {
                return ToModeAcceptedItem(ItemID, "", CurrentMode, 0, false);
            }

            //ТОВАРА В ТЕЛЕГЕ НЕТ, НАЧИНАЕМ РАСЧЕТ (Для разноса)
            //Подсосем инвентарный код, чтобы плевать его в сообщения
            string TextQuery = 
                "SELECT " +
                    "Goods.$Спр.Товары.ИнвКод as InvCode, " +
                    "ISNULL(Package.Coef, 1) as Coef " +
                "FROM " + 
                    "$Спр.Товары as Goods (nolock) " +
                    "LEFT JOIN ( " +
                            "SELECT " + 
                                "Units.parentext as id, " + 
                                "min(Units.$Спр.ЕдиницыШК.Коэффициент ) as Coef " +
                            "FROM " + 
                                "$Спр.ЕдиницыШК as Units (nolock) " +
                            "WHERE " + 
                                "Units.parentext = :ItemID " +
                                "and Units.$Спр.ЕдиницыШК.ОКЕИ = :OKEIPackage " +
                                "and Units.ismark = 0 " +
                            "GROUP BY " +
                                "Units.parentext " + 
                            ") as Package " +
                        "ON Package.id = Goods.id " + 
                "WHERE " + 
                    "Goods.id = :ItemID ";
            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            QuerySetParam(ref TextQuery, "OKEIPackage", OKEIPackage);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            string InvCode  = DT.Rows[0]["InvCode"].ToString().Trim();
            int Coef        = (int)(decimal)DT.Rows[0]["Coef"];

            //ПРОБИВАЕМ СКОЛЬКО УЖЕ РАЗНОСИТСЯ
            //Для простоты будем считать что на каждого сотрудника максимум по одному документу на 80-ом,
            //  вообщем то, исходя из логике движка - так и есть
            TextQuery = 
                "SELECT " + 
                    "Employers.id as EmployerID, " +
                    "Employers.descr as Employer, " +
                    "DocAT.$АдресПеремещение.Количество  as Count, " +
                    "substring(DocAT.$АдресПеремещение.Док , 5, 9) as ACID " + 
                "FROM " +
                    "_1sjourn as Journ (nolock) " +
                    "INNER JOIN DT$АдресПеремещение as DocAT (nolock) " + 
                        "ON DocAT.iddoc = Journ.iddoc " +
                    "LEFT JOIN DH$АдресПеремещение as DocATHeader (nolock) " + 
                        "ON DocATHeader.iddoc = Journ.iddoc " +
                    "LEFT JOIN $Спр.Сотрудники as Employers (nolock) " + 
                        "ON Employers.id = Journ.$Автор " + 
                "WHERE " + 
                    "Journ.date_time_iddoc < '19800101Z' " + 
                    "and not Journ.$Автор = :EmptyID " + 
                    "and DocAT.$АдресПеремещение.Товар = :ItemID " +
                    "and DocATHeader.$АдресПеремещение.ТипДокумента = 2 " +
                //"GROUP BY " +
                    //"Journ.iddoc " + 
                "ORDER BY " + 
                    "Employers.id ";
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            DataTable TransferDT;
            if (!ExecuteWithRead(TextQuery, out TransferDT))
            {
                return false;
            }

            string strTransfer  = "";
            int CountTransfer   = 0;
            if (TransferDT.Rows.Count > 0)
            {
                string CurrEmployerID   = TransferDT.Rows[0]["EmployerID"].ToString();
                string CurrEmployerFIO  = Helper.GetShortFIO(TransferDT.Rows[0]["Employer"].ToString());
                int CurrCountTransfer   = 0;
                for (int row = 0; row < TransferDT.Rows.Count; row++)
                {
                    if (CurrEmployerID != TransferDT.Rows[row]["EmployerID"].ToString())
                    {
                        strTransfer         += ", " + CurrEmployerFIO + " " + GetStrPackageCount(CurrCountTransfer, Coef);
                        CurrCountTransfer   = 0;
                        CurrEmployerID      = TransferDT.Rows[row]["EmployerID"].ToString();
                        CurrEmployerFIO     = Helper.GetShortFIO(TransferDT.Rows[row]["Employer"].ToString());
                    }
                    CountTransfer       += (int)(decimal)TransferDT.Rows[row]["Count"];
                    CurrCountTransfer   += (int)(decimal)TransferDT.Rows[row]["Count"];
                }
                strTransfer += ", " + CurrEmployerFIO + " " + GetStrPackageCount(CurrCountTransfer, Coef);
                strTransfer = "Уже в разносе: " + strTransfer.Substring(2);
            }

            //ПОДСОСЕМ ВАЛОВЫЕ ОСТАТКИ
            TextQuery = "DECLARE @curdate DateTime; " + 
                "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock) ; " + 
                "SELECT " +
                    ":ItemID as ID, " + 
                    "sum(RegAC.$Рег.АдресОстаткиПоступления.Количество ) as Count, " + 
                    "sum(CASE WHEN " +
                        "RegAC.$Рег.АдресОстаткиПоступления.ТипДействия = 1 " +
                        "THEN RegAC.$Рег.АдресОстаткиПоступления.Количество ELSE 0 END) as NACount, " +
                    "sum(CASE WHEN " +
                        "RegAC.$Рег.АдресОстаткиПоступления.ТипДействия = 2 " +
                        "THEN RegAC.$Рег.АдресОстаткиПоступления.Количество ELSE 0 END) as ACount " +
                "FROM " + 
                    "RG$Рег.АдресОстаткиПоступления as RegAC (nolock) " +
                "WHERE " +
                    "RegAC.period = @curdate " + 
                    "and RegAC.$Рег.АдресОстаткиПоступления.Товар = :ItemID " +
                    "and RegAC.$Рег.АдресОстаткиПоступления.Склад = :Warehouse " +
                    (CurrentMode == Mode.Transfer ? "and RegAC.$Рег.АдресОстаткиПоступления.Адрес = :AdressID "  : "") +
                        
                "GROUP BY " +
                    "RegAC.$Рег.АдресОстаткиПоступления.Товар ";

            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            QuerySetParam(ref TextQuery, "Warehouse", ATDoc.FromWarehouseID);
            QuerySetParam(ref TextQuery, "AdressID", ZoneAdressTransfer.ID);

            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count == 0)
            {
                FExcStr = InvCode + ": 0 м., нет в приемке. " + strTransfer;
                return false;
            }
            int AllNACount  = (int)(decimal)DT.Rows[0]["NACount"];
            int AllACount   = (int)(decimal)DT.Rows[0]["ACount"];
            int AllCount    = (int)(decimal)DT.Rows[0]["Count"];

            //ПОЕХАЛИ ОПЯТЬ ПРИЕМКУ РАСЧИТЫВАТЬ
            if (AllNACount > 0)
            {
                //часть товара не принята
                FExcStr = InvCode + ": 0 м., принят неполностью - " +
                            GetStrPackageCount(AllACount, Coef) + " из " +
                            GetStrPackageCount(AllNACount + AllACount, Coef) + ". " + strTransfer;
                return false;
            }

            int CountAdd = AllACount - CountTransfer; //Осталось разнести
            if (CountAdd <= 0)
            {
                FExcStr = InvCode + ": 0 м.. " + strTransfer;
                return false;
            }
            
            return ToModeAcceptedItem(ItemID, "", Mode.Transfer, CountAdd, false);
        }
        private bool AddParty(string ItemID, int InPartyCount)
        {
            //Подсосем инвентарный код, чтобы плевать его в сообщения
            string TextQuery = 
                "SELECT " +
                    "Goods.$Спр.Товары.ИнвКод as InvCode, " +
                    "ISNULL(Package.Coef, 1) as Coef " +
                "FROM " + 
                    "$Спр.Товары as Goods (nolock) " +
                    "LEFT JOIN ( " +
                            "SELECT " + 
                                "Units.parentext as id, " + 
                                "min(Units.$Спр.ЕдиницыШК.Коэффициент ) as Coef " +
                            "FROM " + 
                                "$Спр.ЕдиницыШК as Units (nolock) " +
                            "WHERE " + 
                                "Units.parentext = :ItemID " +
                                "and Units.$Спр.ЕдиницыШК.ОКЕИ = :OKEIPackage " +
                                "and Units.ismark = 0 " +
                            "GROUP BY " +
                                "Units.parentext " + 
                            ") as Package " +
                        "ON Package.id = Goods.id " + 
                "WHERE " + 
                    "Goods.id = :ItemID ";
            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            QuerySetParam(ref TextQuery, "OKEIPackage", OKEIPackage);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            string InvCode  = DT.Rows[0]["InvCode"].ToString().Trim();
            int Coef        = (int)(decimal)DT.Rows[0]["Coef"];

            //ПРОБИВАЕМ ОСТАТКИ ПО АДРЕС ПОСТУПЛЕНИЕ
            TextQuery = 
                "DECLARE @curdate DateTime; " + 
                "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem; " + 
                "SELECT " +
                    ":ItemID as ID, " + 
                    "max(DocAC.lineno_) as lineno_, " +
                    "min(DocAC.$АдресПоступление.ЕдиницаШК ) as UnitID, " +
                    "min(DocAC.$АдресПоступление.Состояние0 ) as State0, " +
                    "min(DocAC.$АдресПоступление.Адрес0 ) as Adress0, " + 
                    "DocAC.iddoc as ACID, " + 
                    "min(RegAC.Count) as Count " + 
                "FROM " + 
                    "DT$АдресПоступление as DocAC (nolock) " +
                    "INNER JOIN ( " + 
                                "SELECT " + 
                                    "sum(RegAC.$Рег.АдресОстаткиПоступления.Количество ) as Count, " + 
                                    "RegAC.$Рег.АдресОстаткиПоступления.Док as ACID " + 
                                "FROM " +
                                    "RG$Рег.АдресОстаткиПоступления as RegAC (nolock) " +
                                "WHERE " +
                                    "RegAC.period = @curdate " + 
                                    "and RegAC.$Рег.АдресОстаткиПоступления.Товар = :ItemID " +
                                    "and RegAC.$Рег.АдресОстаткиПоступления.Склад = :Warehouse " +
                                "GROUP BY " +
                                    "RegAC.$Рег.АдресОстаткиПоступления.Товар , " + 
                                    "RegAC.$Рег.АдресОстаткиПоступления.Док " + 
                                "HAVING " +
                                    "sum(RegAC.$Рег.АдресОстаткиПоступления.Количество ) <> 0 " +
                                ") as RegAC " +
                        "ON DocAC.iddoc = RegAC.ACID " +
                "WHERE " +
                    "DocAC.$АдресПоступление.Товар = :ItemID " +
                    "and DocAC.$АдресПоступление.Состояние0 = 1 " +
                "GROUP BY " +
                    "DocAC.iddoc ";
            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            QuerySetParam(ref TextQuery, "Warehouse", ATDoc.FromWarehouseID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            DataTable AddPartyDT;
            if (!ExecuteWithRead(TextQuery, out AddPartyDT))
            {
                return false;
            }

            //ПРОБИВАЕМ СКОЛЬКО УЖЕ РАЗНОСИТСЯ
            //Для простоты будем считать что на каждого сотрудника максимум по одному документу на 80-ом,
            //  вообщем то, исходя из логике движка - так и есть
            TextQuery = 
                "SELECT " + 
                    "Employers.id as EmployerID, " +
                    "Employers.descr as Employer, " +
                    "DocAT.$АдресПеремещение.Количество  as Count, " +
                    "substring(DocAT.$АдресПеремещение.Док , 5, 9) as ACID " + 
                "FROM " +
                    "_1sjourn as Journ (nolock) " +
                    "INNER JOIN DT$АдресПеремещение as DocAT (nolock) " + 
                        "ON DocAT.iddoc = Journ.iddoc " +
                    "LEFT JOIN DH$АдресПеремещение as DocATHeader (nolock) " + 
                        "ON DocATHeader.iddoc = Journ.iddoc " +
                    "LEFT JOIN $Спр.Сотрудники as Employers (nolock) " + 
                        "ON Employers.id = Journ.$Автор " + 
                "WHERE " + 
                    "Journ.date_time_iddoc < '19800101Z' " + 
                    "and not Journ.$Автор = :EmptyID " + 
                    "and DocAT.$АдресПеремещение.Товар = :ItemID " +
                    "and DocATHeader.$АдресПеремещение.ТипДокумента = 2 " +
                //"GROUP BY " +
                    //"Journ.iddoc " + 
                "ORDER BY " + 
                    "Employers.id ";
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            DataTable TransferDT;
            if (!ExecuteWithRead(TextQuery, out TransferDT))
            {
                return false;
            }

            string strTransfer  = "";
            int CountTransfer   = 0;
            if (TransferDT.Rows.Count > 0)
            {
                string CurrEmployerID   = TransferDT.Rows[0]["EmployerID"].ToString();
                string CurrEmployerFIO  = Helper.GetShortFIO(TransferDT.Rows[0]["Employer"].ToString());
                int CurrCountTransfer   = 0;
                for (int row = 0; row < TransferDT.Rows.Count; row++)
                {
                    if (CurrEmployerID != TransferDT.Rows[row]["EmployerID"].ToString())
                    {
                        strTransfer         += ", " + CurrEmployerFIO + " " + GetStrPackageCount(CurrCountTransfer, Coef);
                        CurrCountTransfer   = 0;
                        CurrEmployerID      = TransferDT.Rows[row]["EmployerID"].ToString();
                        CurrEmployerFIO     = Helper.GetShortFIO(TransferDT.Rows[row]["Employer"].ToString());
                    }
                    CountTransfer       += (int)(decimal)TransferDT.Rows[row]["Count"];
                    CurrCountTransfer   += (int)(decimal)TransferDT.Rows[row]["Count"];

                    if (CurrEmployerID != Employer.ID && CurrCountTransfer > 0)
                    {
                        //нельзя разность никому, кроме самого авторизованного кента
                        FExcStr = InvCode + " нельзя взять! Уже разносится " + CurrEmployerFIO;
                        return false;
                    }
                }
                strTransfer += ", " + CurrEmployerFIO + " " + GetStrPackageCount(CurrCountTransfer, Coef);
                strTransfer = "Уже в разносе: " + strTransfer.Substring(2);
            }

            //ПОДСОСЕМ ВАЛОВЫЕ ОСТАТКИ
            TextQuery = "DECLARE @curdate DateTime; " + 
                "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " + 
                "SELECT " +
                    ":ItemID as ID, " + 
                    "sum(RegAC.$Рег.АдресОстаткиПоступления.Количество ) as Count, " + 
                    "sum(CASE WHEN " +
                        "RegAC.$Рег.АдресОстаткиПоступления.ТипДействия = 1 " +
                        "THEN RegAC.$Рег.АдресОстаткиПоступления.Количество ELSE 0 END) as NACount, " +
                    "sum(CASE WHEN " +
                        "RegAC.$Рег.АдресОстаткиПоступления.ТипДействия = 2 " +
                        "THEN RegAC.$Рег.АдресОстаткиПоступления.Количество ELSE 0 END) as ACount " +
                "FROM " + 
                    "RG$Рег.АдресОстаткиПоступления as RegAC (nolock) " +
                "WHERE " +
                    "RegAC.period = @curdate " + 
                    "and RegAC.$Рег.АдресОстаткиПоступления.Товар = :ItemID " +
                    "and RegAC.$Рег.АдресОстаткиПоступления.Склад = :Warehouse " +
                "GROUP BY " +
                    "RegAC.$Рег.АдресОстаткиПоступления.Товар ";

            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            QuerySetParam(ref TextQuery, "Warehouse", ATDoc.FromWarehouseID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count == 0)
            {
                FExcStr = InvCode + ": 0 м., нет в приемке. " + strTransfer;
                return false;
            }
            int AllNACount  = (int)(decimal)DT.Rows[0]["NACount"];
            int AllACount   = (int)(decimal)DT.Rows[0]["ACount"];
            int AllCount    = (int)(decimal)DT.Rows[0]["Count"];

            //ПОЕХАЛИ ОПЯТЬ ПРИЕМКУ РАСЧИТЫВАТЬ
            if (AllNACount > 0)
            {
                //часть товара не принята
                FExcStr = InvCode + ": 0 м., принят неполностью - " +
                            GetStrPackageCount(AllACount, Coef) + " из " +
                            GetStrPackageCount(AllNACount + AllACount, Coef) + ". " + strTransfer;
                return false;
            }

            int CountAdd = AllACount - CountTransfer; //Осталось разнести
            if (CountAdd <= 0)
            {
                FExcStr = InvCode + ": 0 м.. " + strTransfer;
                return false;
            }
            if (CountAdd < InPartyCount)
            {
                FExcStr = "Нет столько к разносу! Доступно " + GetStrPackageCount(CountAdd, Coef) + " " + strTransfer;
                return false;
            }

            //Весь товар принят! Можно добавлять его!
            /////////////////////////////////////////////////////
            //А ТЕПЕРЬ ПИШЕМ ЭТО ГОВНО В ДОКУМЕНТ

            //ОПРЕДЕЛЯЕМ ОСТАТКИ
            TextQuery =
                "DECLARE @curdate DateTime; " + 
                "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " + 
                "SELECT " + 
                    "CAST(sum($Рег.ОстаткиТоваров.ОстатокТовара ) as int) as Balance " +
                "FROM " + 
                    "RG$Рег.ОстаткиТоваров (nolock) " +
                "WHERE " + 
                    "period = @curdate " +
                    "and $Рег.ОстаткиТоваров.Товар = :Item " +
                    "and $Рег.ОстаткиТоваров.Склад = :Warehouse " +
                "GROUP BY $Рег.ОстаткиТоваров.Товар ";
            QuerySetParam(ref TextQuery, "Item", AddPartyDT.Rows[0]["ID"]);
            QuerySetParam(ref TextQuery, "Warehouse", ATDoc.ToWarehouseID);
            DataTable DTReg;
            if (!ExecuteWithRead(TextQuery, out DTReg))
            {
                return false;
            }

            //ОПРЕДЕЛИМ РЕКОМЕНДУЕМЫЙ АДРЕС
            TextQuery =
                "SELECT top 1 " +
                " left(value, 9) as Adress1 " +
                "FROM _1sconst (nolock) " + 
                "WHERE " + 
                    "id = $Спр.ТоварныеСекции.Секция " + 
                    "and date <= :DateNow " + 
                    "and OBJID in (" + 
                                    "SELECT id FROM $Спр.ТоварныеСекции (nolock) " +
                                    "WHERE " + 
                                        "$Спр.ТоварныеСекции.Склад = :Warehouse " + 
                                        "and parentext = :Item)" + 
                "ORDER BY " + 
                    "date DESC, time DESC, docid DESC ";
            QuerySetParam(ref TextQuery, "DateNow", DateTime.Now);
            QuerySetParam(ref TextQuery, "Item", ItemID);
            QuerySetParam(ref TextQuery, "Warehouse", ATDoc.ToWarehouseID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            string Adress1 = (DT.Rows.Count == 0 ? GetVoidID() : DT.Rows[0]["Adress1"].ToString());

            //ДОБАВЛЯЕМ НОВУЮ или даже НОВЫЕ СТРОКИ
            TextQuery =
                "SELECT ISNULL(max(DT$АдресПеремещение .lineno_) + 1, 1) as LineNo_ " +
                "FROM DT$АдресПеремещение (nolock) " + 
                "WHERE DT$АдресПеремещение .iddoc = :Doc";
            QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
            DT.Clear();
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            int LineNo_ = (int)DT.Rows[0]["LineNo_"];

            string strAccepted = "";
            DataRow[] DR = AddPartyDT.Select();

            foreach (DataRow dr in DR)
            {
                if (AllACount == 0)
                {
                    continue; //тут должно быть break, но впадлу менять перед отпуском, т.к. 100% уверенности - нет
                }
                int Count           = (int)(decimal)dr["Count"];    //По данной строке документа висит столько
                if (Count > AllACount)
                {
                    Count       = AllACount;
                    AllACount   = 0;
                }
                else
                {
                    AllACount -= Count;
                }
                CountTransfer   = 0;                            //Сколько разносится по данному документу
                DataRow[] tmpDR = TransferDT.Select("ACID = '" + dr["ACID"].ToString() + "'");
                foreach (DataRow tmpdr in tmpDR)
                {
                    CountTransfer += (int)(decimal)tmpdr["Count"];
                }
                if (Count <= CountTransfer)
                {
                    CountTransfer -= Count;
                    continue;   //Все разносится, добалвять не будем
                }
                Count -= CountTransfer; //Уменьшим на разносимое количество
                if (Count >= InPartyCount)
                {
                    Count = InPartyCount;
                }
                InPartyCount -= Count;

                CountTransfer = 0;
                strAccepted += " + " + GetStrPackageCount((int)(decimal)Count, Coef); //Формируем строку вывода
                TextQuery =
                    "INSERT INTO DT$АдресПеремещение VALUES " +
                        "(:Doc, :LineNo_, :Item, :Count, :EmptyID, :Coef, :State0, 2, :Employer, " + 
                        ":Adress0, :Adress1, :Date0, :EmptyDate, :Time0, 0, :ACDoc, :Number, " +
                        ":SampleCount, :BindingAdressFlag, :UnitID); ";
                QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
                QuerySetParam(ref TextQuery, "LineNo_", LineNo_);
                QuerySetParam(ref TextQuery, "Item", dr["ID"]);
                QuerySetParam(ref TextQuery, "Count", Count);
                QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
                QuerySetParam(ref TextQuery, "Coef", 1);
                QuerySetParam(ref TextQuery, "State0", dr["State0"]);
                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "Adress0", dr["Adress0"]);
                QuerySetParam(ref TextQuery, "Adress1", Adress1);
                QuerySetParam(ref TextQuery, "Date0", DateTime.Now);
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                QuerySetParam(ref TextQuery, "Time0", APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "ACDoc", ExtendID(dr["ACID"].ToString(), "АдресПоступление"));
                QuerySetParam(ref TextQuery, "Number", dr["lineno_"]);
                QuerySetParam(ref TextQuery, "UnitID", dr["UnitID"]);
                QuerySetParam(ref TextQuery, "SampleCount", 0);
                if (DTReg.Rows.Count > 0)
                {
                    QuerySetParam(ref TextQuery, "BindingAdressFlag", ((int)DTReg.Rows[0]["Balance"] == 0 ? 0 : 1));
                }
                else
                {
                    QuerySetParam(ref TextQuery, "BindingAdressFlag", 0);
                }
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                ++LineNo_;
                if (InPartyCount == 0)
                {
                    break;
                }
            }
            strAccepted = strAccepted.Substring(3);
            FExcStr = InvCode + ": " + GetStrPackageCount(CountAdd, Coef);
                        //+ ", ДОБАВЛЕНО: " + strAccepted + ". " + strTransfer;

            //begin internal command
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(ItemID, "Спр.Товары");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(ATDoc.ID, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "LoadItem (Загрузил в тележку)";
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход2"]       = CountAdd.ToString();  //Всего добавлено
            if (!ExecCommandNoFeedback("Internal", DataMapWrite))
            {
                return false;
            }
            //end internal command

            return ToModeTransfer(ATDoc, AcceptedItem.ToMode);   //рефрешим табличку
            
        } // AddParty()
        public bool EnterAdressHarmonization(string IDD)
        {
            Dictionary<string, object> DataMap;
            List<string> FieldList = new List<string>();
            FieldList.Add("ID");
            FieldList.Add("DESCR");
            if (!GetSCData(IDD, "Секции", FieldList, out DataMap))
            {
                return false;
            }

            //ОПРЕДЕЛИМ РОДИТЕЛЯ И ПРОВЕРИМ СКЛАД
            string TextQuery =
                "DECLARE @parent varchar(9); " +
                "DECLARE @curid varchar(9); " +
                "SET @curid = :Adress1; " +
                "WHILE not @curid = '     0   ' BEGIN " +
                    "SET @parent = @curid; " +
                    "SELECT @curid = parentid FROM $Спр.Секции (nolock) WHERE id = @curid; " +
                "END; " +
                "SELECT $Спр.Секции.Склад as Warehouse " +
                    "FROM $Спр.Секции (nolock) WHERE id = @parent; ";
            QuerySetParam(ref TextQuery, "Adress1", DataMap["ID"]);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (ATDoc.ToWarehouseID != DT.Rows[0]["Warehouse"].ToString())
            {
                FExcStr = "Адрес другого склада!";
                return false;
            }
            if (AcceptedItem.AcceptCount == 0)
            {
                FExcStr = "Не указано количество!";
                return false;
            }

            TextQuery =
                "SELECT isnull(max(DT$АдресПеремещение .lineno_) + 1, 1) as NewLineNo_ " +
                "FROM DT$АдресПеремещение (nolock) WHERE  DT$АдресПеремещение .iddoc = :Doc";
            QuerySetParam(ref TextQuery, "Doc", ATDoc.FoundDoc);
            DataTable tmpDT;
            if (!ExecuteWithRead(TextQuery, out tmpDT))
            {
                return false;
            }
            int NewLineNo_ = (int)tmpDT.Rows[0]["NewLineNo_"];

            TextQuery =
            "DELETE FROM DT$АдресПеремещение WHERE $АдресПеремещение.Товар = :Item and $АдресПеремещение.ФлагДопроведения = 0 and iddoc = :Doc " + 
            "INSERT INTO DT$АдресПеремещение VALUES " +
                "(:Doc, :LineNo_, :Item, :Count, :EmptyID, :Coef, :State0, :State1, :Employer, " + 
                ":Adress0, :Adress1, :Date0, :Date0, :Time0, :Time0, :ACDoc, :Number, " +
                "0, :BindingAdressFlag, :UnitID); ";
            QuerySetParam(ref TextQuery, "Doc", ATDoc.FoundDoc);
            QuerySetParam(ref TextQuery, "LineNo_", NewLineNo_);
            QuerySetParam(ref TextQuery, "Item", AcceptedItem.ID);
            QuerySetParam(ref TextQuery, "Count", AcceptedItem.AcceptCount);
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
            QuerySetParam(ref TextQuery, "Coef", 1);
            QuerySetParam(ref TextQuery, "State0", 2);
            QuerySetParam(ref TextQuery, "State1", 3);
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            QuerySetParam(ref TextQuery, "Adress0", DataMap["ID"]);
            QuerySetParam(ref TextQuery, "Adress1", DataMap["ID"]);
            QuerySetParam(ref TextQuery, "Date0", DateTime.Now);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "Time0", APIManager.NowSecond());
            QuerySetParam(ref TextQuery, "ACDoc", ExtendID(GetVoidID(), "АдресПеремещение"));
            QuerySetParam(ref TextQuery, "Number", 0);
            QuerySetParam(ref TextQuery, "UnitID", GetVoidID());
            QuerySetParam(ref TextQuery, "BindingAdressFlag", 0);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }

            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(ATDoc.ID, "АдресПеремещение");;
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "HarmonizationAddItem";

            Dictionary<string, object> DataMapRead;
            FieldList.Clear();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("HarmonizationAddItem", DataMapWrite, FieldList, out DataMapRead ))
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
            FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
            return ToModeHarmonizationInicialize();
        }//EnterAdressHarmonization
        public bool EnterAdressHarmonizationPut(string IDD)
        {
            if (AcceptedItem.AcceptCount == 0)
            {
                FExcStr = "Не указано количество!";
                return false;
            }

            Dictionary<string, object> DataMap;
            List<string> FieldList = new List<string>();
            FieldList.Add("ID");
            FieldList.Add("DESCR");
            if (!GetSCData(IDD, "Секции", FieldList, out DataMap))
            {
                return false;
            }

            //ОПРЕДЕЛИМ РОДИТЕЛЯ И ПРОВЕРИМ СКЛАД
            string TextQuery =
                "DECLARE @parent varchar(9); " +
                "DECLARE @curid varchar(9); " +
                "SET @curid = :Adress1; " +
                "WHILE not @curid = '     0   ' BEGIN " +
                    "SET @parent = @curid; " +
                    "SELECT @curid = parentid FROM $Спр.Секции (nolock) WHERE id = @curid; " +
                "END; " +
                "SELECT $Спр.Секции.Склад as Warehouse " +
                    "FROM $Спр.Секции (nolock) WHERE id = @parent; ";
            QuerySetParam(ref TextQuery, "Adress1", DataMap["ID"]);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (ATDoc.ToWarehouseID != DT.Rows[0]["Warehouse"].ToString())
            {
                FExcStr = "Адрес другого склада!";
                return false;
            }

            //Провем можно ли выставлять в зону товар, если это не машинная или не ручная хона (хорошие) то нихуя проверять не будем!
            RefSection ScanAdress = new RefSection(this);
            if (!ScanAdress.FoundIDD(IDD))
            {
                return false;
            }
            RefItem Item = new RefItem(this);
            Item.FoundID(AcceptedItem.ID);
            RefGates ItemZone;
            if (ScanAdress.Type == 2)
            {
                ItemZone = Item.ZonaTech;
            }
            else
            {
                //Для всех остальных видов адреса будем считать ручную зону
                ItemZone = Item.ZonaHand;
            }

            if (ScanAdress.Type == 1 || ScanAdress.Type == 2)
            {
                //Для типа секции - хороший ручная и хороший брак - проверка актуально, для остальных - нет
                //Проверка
                if (ItemZone.Selected && ScanAdress.AdressZone.Selected)
                {
                    //У обоих задана зона
                    if (ItemZone.ID != ScanAdress.AdressZone.ID)
                    {
                        FExcStr = "Нельзя! Товар: " + ItemZone.Name + ", адрес: " + ScanAdress.AdressZone.Name;
                        return false;
                    }
                }
                else if (ItemZone.Selected)
                {
                    //только товар задан
                    FExcStr = "Нельзя! Товар: " + ItemZone.Name;
                    return false;
                }
                else if (ScanAdress.AdressZone.Selected)
                {
                    //Только у адреса задана зона
                    FExcStr = "Нельзя! Адрес: " + ScanAdress.AdressZone.Name;
                    return false;
                }
            }
            
            //Подсосем таблицу с товаром
            TextQuery =
                "SELECT " +
                    "DocAT.lineno_ as lineno_, " + 
                    "DocAT.$АдресПеремещение.Количество as Count, " +
                    "DocAT.$АдресПеремещение.Состояние1 as State1, " +
                    "DocAT.$АдресПеремещение.Состояние0 as State0, " +
                    "DocAT.$АдресПеремещение.Адрес0 as Adress0, " +
                    "DocAT.$АдресПеремещение.Адрес1 as Adress1, " +
                    "DocAT.$АдресПеремещение.Дата0 as Date0, " +
                    "DocAT.$АдресПеремещение.Время0 as Time0, " +
                    "DocAT.$АдресПеремещение.Док as ACID, " +
                    "DocAT.$АдресПеремещение.НомерСтрокиДока as Number, " +
                    "DocAT.$АдресПеремещение.ЕдиницаШК as UnitID, " +
                    "DocAT.$АдресПеремещение.ФлагОбязательногоАдреса as BindingAdressFlag " +
                "FROM " +
                    "DT$АдресПеремещение as DocAT (nolock) " +
                "WHERE " + 
                    "DocAT.iddoc = :Doc " + 
                    "and not DocAT.$АдресПеремещение.Дата0 = :EmptyDate " + 
                    "and DocAT.$АдресПеремещение.Дата1 = :EmptyDate " + 
                    "and DocAT.$АдресПеремещение.Товар = :Item " + 
                "ORDER BY lineno_";
            QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "Item", AcceptedItem.ID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "Товара нет в телеге!";
                return false;
            }

            int AllCount = 0;
            DataRow[] DR = DT.Select();
            foreach (DataRow dr in DR)
            {
                AllCount += (int)(decimal)dr["Count"];
            }

            if (AllCount < AcceptedItem.AcceptCount)
            {
                FExcStr = "Товара недостаточно в телеге!";
                return false;
            }

            int Count = AcceptedItem.AcceptCount;   //Сколько нужно переместить на полку
            DR = DT.Select();
            foreach (DataRow dr in DR)
            {
                int CurrCount = (int)(decimal)dr["Count"];
                int UpdateCount;
                if (CurrCount <= Count)
                {
                    //Тупо апдейтем строчку
                    Count -= CurrCount;
                    UpdateCount = CurrCount;
                }
                else
                {
                    UpdateCount = Count;
                    Count = 0;
                }

                TextQuery =
                    "UPDATE DT$АдресПеремещение " +
                    "SET " +
                        "$АдресПеремещение.Количество = :Count," +
                        "$АдресПеремещение.Дата1 = :Date1," + 
                        "$АдресПеремещение.Время1 = :Time1," + 
                        "$АдресПеремещение.Состояние1 = :State, " + 
                        "$АдресПеремещение.Адрес1 = :Adress1 " + 
                    "WHERE " +
                        "DT$АдресПеремещение .iddoc = :Doc " + 
                        "and DT$АдресПеремещение .lineno_ = :LineNo_";
                QuerySetParam(ref TextQuery, "Count",           UpdateCount);
                QuerySetParam(ref TextQuery, "Employer",        Employer.ID);
                QuerySetParam(ref TextQuery, "Date1",           DateTime.Now);
                QuerySetParam(ref TextQuery, "Time1",           APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "Adress1",         DataMap["ID"]);
                if ((int)(decimal)dr["State0"] == -1 || (int)(decimal)dr["State0"] == 0)
                {
                    QuerySetParam(ref TextQuery, "State",       -2);
                }
                else if ((int)(decimal)dr["State0"] == 8)
                {
                    QuerySetParam(ref TextQuery, "State",       7);
                }
                else
                {
                    QuerySetParam(ref TextQuery, "State",       2);
                }
                QuerySetParam(ref TextQuery, "Doc",             ATDoc.ID);
                QuerySetParam(ref TextQuery, "LineNo_",         dr["lineno_"]);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }

                if (UpdateCount != CurrCount)
                {
                    //Нуно создать новую строку
                    TextQuery =
                        "SELECT max(DT$АдресПеремещение .lineno_) + 1 as NewLineNo_ " +
                        "FROM DT$АдресПеремещение (nolock) WHERE  DT$АдресПеремещение .iddoc = :Doc";
                    QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
                    DataTable tmpDT;
                    if (!ExecuteWithRead(TextQuery, out tmpDT))
                    {
                        return false;
                    }
                    int NewLineNo_ = (int)tmpDT.Rows[0]["NewLineNo_"];

                    TextQuery =
                    "INSERT INTO DT$АдресПеремещение VALUES " +
                        "(:Doc, :LineNo_, :Item, :Count, :EmptyID, :Coef, :State0, :State1, :Employer, " + 
                        ":Adress0, :Adress1, :Date0, :EmptyDate, :Time0, 0, :ACDoc, :Number, " +
                        "1, :BindingAdressFlag, :UnitID); ";
                    QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
                    QuerySetParam(ref TextQuery, "LineNo_", NewLineNo_);
                    QuerySetParam(ref TextQuery, "Item", AcceptedItem.ID);
                    QuerySetParam(ref TextQuery, "Count", CurrCount - UpdateCount);
                    QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
                    QuerySetParam(ref TextQuery, "Coef", 1);
                    QuerySetParam(ref TextQuery, "State0", dr["State0"]);
                    QuerySetParam(ref TextQuery, "State1", dr["State1"]);
                    QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                    QuerySetParam(ref TextQuery, "Adress0", dr["Adress0"]);
                    QuerySetParam(ref TextQuery, "Adress1", dr["Adress1"]);
                    QuerySetParam(ref TextQuery, "Date0", dr["Date0"]);
                    QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                    QuerySetParam(ref TextQuery, "Time0", dr["Time0"]);
                    QuerySetParam(ref TextQuery, "ACDoc", dr["ACID"]);
                    QuerySetParam(ref TextQuery, "Number", dr["Number"]);
                    QuerySetParam(ref TextQuery, "UnitID", dr["UnitID"]);
                    QuerySetParam(ref TextQuery, "BindingAdressFlag", dr["BindingAdressFlag"]);
                    if (!ExecuteWithoutRead(TextQuery))
                    {
                        return false;
                    }
                }

                if (Count == 0)
                {
                    break;
                }
            }
            return ToModeHarmonizationInicialize();
        }//EnterAdressHarmonizationPut
        public bool EnterPalletAcceptanceCross(string strBarcode)
        {
            //сначала проверим может паллета уже есть в заказе и тогда просто проверим ШК
            string TextQuery;
            string ID = CurrentRowAcceptedItem["PalletID"].ToString();
            FBarcodePallet = strBarcode;
            DataTable DT;
            if (ID != "" && ID != SQL1S.GetVoidID())
            {
                TextQuery =
                    "SELECT " +
                        "Pallets.ID " +
                    "FROM " +
                        "$Спр.ПеремещенияПаллет as Pallets (nolock) " +
                    "WHERE " +
                        "Pallets.$Спр.ПеремещенияПаллет.ШКПаллеты = :Barcode " +
                        "and Pallets.ID = :Pallet";
                SQL1S.QuerySetParam(ref TextQuery, "Pallet", ID);
                SQL1S.QuerySetParam(ref TextQuery, "Barcode", strBarcode);
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    FExcStr = "Не та паллета! Надо " + CurrentRowAcceptedItem["PalletName"].ToString();
                    return false;
                }
                FPalletID = ID;
            }
            else
            {
                //нет еще паллеты, надо создавать
                TextQuery = "declare @result char(9); exec WPM_GetIDNewPallet :Barcode, :Employer, @result out; select @result;";
                SQL1S.QuerySetParam(ref TextQuery, "Barcode", strBarcode);
                SQL1S.QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                FPalletID = (string)ExecuteScalar(TextQuery);

                if (FPalletID == SQL1S.GetVoidID())
                {
                    FExcStr = "Не удалось найти паллету!";
                    return false;
                }
            }
            PalletAcceptedItem = new RefPalleteMove(this);
            if (!PalletAcceptedItem.FoundID(FPalletID))
            {
                PalletAcceptedItem = null;
                FExcStr = "Не найдена паллета!";
                return false;
            }
            //проверим а нет ли заказов на ней, может это паллета другого клиента
            TextQuery =
                "SELECT " +
                    "Orders.IDDOC as OrderID, " +
                    "right(Journ.docno,5) as OrderName " +
                "FROM " +
                    "DH$ЗаказНаКлиента as Orders (nolock) " +
                    "JOIN _1sjourn as Journ (nolock) " +
                         "on Journ.IDDOC = Orders.IDDOC " +
                "WHERE " +
                    "Orders.$ЗаказНаКлиента.Паллета = :Pallet " +
                    "and NOT Orders.IDDOC = :OrderID ";
            SQL1S.QuerySetParam(ref TextQuery, "OrderID", CurrentRowAcceptedItem["OrderID"].ToString());
            SQL1S.QuerySetParam(ref TextQuery, "Pallet", ID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                PalletAcceptedItem = null;
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                PalletAcceptedItem = null;
                FExcStr = "Паллета уже с другим заказом " + DT.Rows[0]["OrderName"];
                return false;
            }
            if (CurrentRowAcceptedItem["PalletID"].ToString() != "")
            {
                //сравниваем ее тогда с паллетой отсканированной
                if (PalletAcceptedItem.ID.ToString() != CurrentRowAcceptedItem["PalletID"].ToString())
                {
                    //косяк, не та паллета
                    PalletAcceptedItem = null;
                    FExcStr = "Не та паллета! Надо " + CurrentRowAcceptedItem["PalletName"].ToString();
                    return false;
                }
            }
            //запишим данные в паллету тепреь
            TextQuery =
                    "UPDATE $Спр.ПеремещенияПаллет " +
                    "SET " +
                        "$Спр.ПеремещенияПаллет.Сотрудник1 = :EmployerID, " +
                        "$Спр.ПеремещенияПаллет.ФлагОперации = 1, " +
                        "$Спр.ПеремещенияПаллет.Дата10 = :Date, " +
                        "$Спр.ПеремещенияПаллет.Время10 = :Time, " +
                        "$Спр.ПеремещенияПаллет.ТипДвижения = 4 " +
                    "WHERE $Спр.ПеремещенияПаллет .id = :Pallet ";
            SQL1S.QuerySetParam(ref TextQuery, "EmptyDate", SQL1S.GetVoidDate());
            SQL1S.QuerySetParam(ref TextQuery, "EmployerID", Employer.ID);
            SQL1S.QuerySetParam(ref TextQuery, "Date", DateTime.Now);
            SQL1S.QuerySetParam(ref TextQuery, "Time", APIManager.NowSecond());
            SQL1S.QuerySetParam(ref TextQuery, "Pallet", FPalletID);
            if (!ExecuteWithoutRead(TextQuery))
            {
                PalletAcceptedItem = null;
                return false;
            }
            FExcStr = "Паллета " + PalletAcceptedItem.Pallete;
            return true;
        }
        public bool EnterAdress(string IDD)
        {
            if (AcceptedItem.ToMode == Mode.Harmonization)
            {
                return EnterAdressHarmonization(IDD);
            }
            else if (AcceptedItem.ToMode == Mode.HarmonizationPut)
            {
                return EnterAdressHarmonizationPut(IDD);
            }

            int InCartCount   = 0;
            DataRow[] DR = FTransferItems.Select("ID = '" + AcceptedItem.ID + "'");
            foreach (DataRow dr in DR)
            {
                InCartCount   += (int)(decimal)dr["Count"];
            }
            if (AcceptedItem.AcceptCount > InCartCount)
            {
                FExcStr = "Столько товара нет в тележке!";
                return false;
            }

            RefSection ScanAdress = new RefSection(this);
            if (!ScanAdress.FoundIDD(IDD))
            {
                return false;
            }

            //ОПРЕДЕЛИМ РОДИТЕЛЯ И ПРОВЕРИМ СКЛАД
            string TextQuery =
                "DECLARE @parent varchar(9); " +
                "DECLARE @curid varchar(9); " +
                "SET @curid = :Adress1; " +
                "WHILE not @curid = '     0   ' BEGIN " +
                    "SET @parent = @curid; " +
                    "SELECT @curid = parentid FROM $Спр.Секции (nolock) WHERE id = @curid; " +
                "END; " +
                "SELECT $Спр.Секции.Склад as Warehouse, $Спр.Секции.ЗапретитьВыкладкуБезШК as StopWithoutBarcode " +
                    "FROM $Спр.Секции (nolock) WHERE id = @parent; ";
            QuerySetParam(ref TextQuery, "Adress1", ScanAdress.ID);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (ATDoc.ToWarehouseID != DT.Rows[0]["Warehouse"].ToString())
            {
                FExcStr = "Адрес другого склада!";
                return false;
            }
            
            DR = Units.Select("not (Barcode = '' or substring(Barcode, 1, " + BarcodePrefix.Length.ToString() + ") = '" + BarcodePrefix + "')");
            if (DR.Length == 0)
            {
                if ((int)(decimal)DT.Rows[0]["StopWithoutBarcode"] == 1)
                {
                    FExcStr = "Нельзя выкладывать без ШК!";
                    return false;
                }
            }

            //Делаю это выше (см. ниже)

            bool CheckBind = ATDoc.ToWarehouseSingleAdressMode && AcceptedItem.BindingAdressFlag && ScanAdress.Type != 2;
            RefSection BindingAdress = new RefSection(this);
            BindingAdress.FoundID(AcceptedItem.BindingAdress);

            //Провем можно ли выставлять в зону товар, если это не машинная или не ручная хона (хорошие) то нихуя проверять не будем!
            RefItem Item = new RefItem(this);
            Item.FoundID(AcceptedItem.ID);
            RefGates ItemZone;
            if (ScanAdress.Type == 2)
            {
                ItemZone = Item.ZonaTech;
            }
            else
            {
                //Для всех остальных видов адреса будем считать ручную зону
                ItemZone = Item.ZonaHand;
            }

            //if (CheckBind && ItemZone.Selected && BindingAdress. )

            //На обазательный адрес
            if (CheckBind && ItemZone.Selected && BindingAdress.AdressZone.Selected)
            {
                //у товара есть зона и у обязательного адреса есть зона и нужеа выкладка в обязательный адерс
                if (ItemZone.ID == BindingAdress.AdressZone.ID && ScanAdress.ID != BindingAdress.ID)
                {
                    //Зоны у товара и обязательного адреса совпадают, а обязательный не совпадает со сканированным, значит хуй вам!
                    //иначе нам похуй, т.к. проверки ниже пропустят если все совпадает и ругаться не будут
                    FExcStr = "Неверный адрес! Обязателен: " + BindingAdress.Name;
                    return false;
                }
            }
            //Проверка
            if (AcceptedItem.ToMode != Mode.NewInventory)
            {

                if (ItemZone.Selected && ScanAdress.AdressZone.Selected)
                {
                    //У обоих задана зона
                    if (ItemZone.ID != ScanAdress.AdressZone.ID)
                    {
                        FExcStr = "Нельзя! Товар: " + ItemZone.Name + ", адрес: " + ScanAdress.AdressZone.Name;
                        return false;
                    }
                }
                else if (ItemZone.Selected)
                {
                    //только товар задан
                    FExcStr = "Нельзя! Товар: " + ItemZone.Name;
                    return false;
                }
                else if (ScanAdress.AdressZone.Selected)
                {
                    //Только у адреса задана зона
                    FExcStr = "Нельзя! Адрес: " + ScanAdress.AdressZone.Name;
                    return false;
                }
                else if (CheckBind && ScanAdress.ID != BindingAdress.ID && !BindingAdress.AdressZone.Selected)
                {
                    //нихуя не задано, проверяем по старинке обязательный адрес
                    //дополнительно убедимся что у обязательного адреса не задана зона. Потому как елси она задана, а у товара - нет,
                    //  то этот обязательный адрес мы на хую вертим!
                    FExcStr = "Неверный адрес! Обязателен: " + BindingAdress.Name;
                    return false;
                }
                //else в остальных случая - нам похуй
            }    

            //--------------------------------------------
            //НИЖЕ НЕТ ПРОВЕРОК АДРЕСОВ И ПРОЧЕЙ ХУЙНИ
            //  КОРОЧЕ НИЖЕ УЖЕ ЗАПИСЬ. ЕСЛИ ЧТО-ТО НАДО ПРОВЕРИТЬ, ДЕЛАЙ ЭТО ВЫШЕ

            //Подсосем таблицу с товаром
            TextQuery =
                "SELECT " +
                    "DocAT.lineno_ as lineno_, " + 
                    "DocAT.$АдресПеремещение.Количество as Count, " +
                    "DocAT.$АдресПеремещение.Состояние1 as State1, " +
                    "DocAT.$АдресПеремещение.Состояние0 as State0, " +
                    "DocAT.$АдресПеремещение.Адрес0 as Adress0, " +
                    "DocAT.$АдресПеремещение.Адрес1 as Adress1, " +
                    "DocAT.$АдресПеремещение.Дата0 as Date0, " +
                    "DocAT.$АдресПеремещение.Время0 as Time0, " +
                    "DocAT.$АдресПеремещение.Док as ACID, " +
                    "DocAT.$АдресПеремещение.НомерСтрокиДока as Number, " +
                    "DocAT.$АдресПеремещение.ЕдиницаШК as UnitID, " +
                    "DocAT.$АдресПеремещение.ФлагОбязательногоАдреса as BindingAdressFlag " +
                "FROM " +
                    "DT$АдресПеремещение as DocAT (nolock) " +
                "WHERE " + 
                    "DocAT.iddoc = :Doc " + 
                    "and not DocAT.$АдресПеремещение.Дата0 = :EmptyDate " + 
                    "and DocAT.$АдресПеремещение.Дата1 = :EmptyDate " + 
                    "and DocAT.$АдресПеремещение.Товар = :Item " + 
                "ORDER BY lineno_";
            QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "Item", AcceptedItem.ID);
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            int Count = AcceptedItem.AcceptCount;   //Сколько нужно переместить на полку
            DR = DT.Select();
            foreach (DataRow dr in DR)
            {
                int CurrCount = (int)(decimal)dr["Count"];
                int UpdateCount;
                if (CurrCount <= Count)
                {
                    //Тупо апдейтем строчку
                    Count -= CurrCount;
                    UpdateCount = CurrCount;
                }
                else
                {
                    UpdateCount = Count;
                    Count = 0;
                }

                TextQuery =
                    "UPDATE DT$АдресПеремещение " +
                    "SET " +
                        "$АдресПеремещение.Количество = :Count," +
                        "$АдресПеремещение.Дата1 = :Date1," + 
                        "$АдресПеремещение.Время1 = :Time1," + 
                        "$АдресПеремещение.Состояние1 = :State, " + 
                        "$АдресПеремещение.Адрес1 = :Adress1 " + 
                    "WHERE " +
                        "DT$АдресПеремещение .iddoc = :Doc " + 
                        "and DT$АдресПеремещение .lineno_ = :LineNo_";
                QuerySetParam(ref TextQuery, "Count",           UpdateCount);
                QuerySetParam(ref TextQuery, "Employer",        Employer.ID);
                QuerySetParam(ref TextQuery, "Date1",           DateTime.Now);
                QuerySetParam(ref TextQuery, "Time1",           APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "Adress1",         ScanAdress.ID);
                if ((int)(decimal)dr["State0"] == -1 || (int)(decimal)dr["State0"] == 0)
                {
                    QuerySetParam(ref TextQuery, "State",       -2);
                }
                else if ((int)(decimal)dr["State0"] == 8)
                {
                    QuerySetParam(ref TextQuery, "State",       7);
                }
                else
                {
                    QuerySetParam(ref TextQuery, "State",       2);
                }
                QuerySetParam(ref TextQuery, "Doc",             ATDoc.ID);
                QuerySetParam(ref TextQuery, "LineNo_",         dr["lineno_"]);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }

                if (UpdateCount != CurrCount)
                {
                    //Нуно создать новую строку
                    TextQuery =
                        "SELECT max(DT$АдресПеремещение .lineno_) + 1 as NewLineNo_ " +
                        "FROM DT$АдресПеремещение (nolock) WHERE  DT$АдресПеремещение .iddoc = :Doc";
                    QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
                    DataTable tmpDT;
                    if (!ExecuteWithRead(TextQuery, out tmpDT))
                    {
                        return false;
                    }
                    int NewLineNo_ = (int)tmpDT.Rows[0]["NewLineNo_"];

                    TextQuery =
                    "INSERT INTO DT$АдресПеремещение VALUES " +
                        "(:Doc, :LineNo_, :Item, :Count, :EmptyID, :Coef, :State0, :State1, :Employer, " + 
                        ":Adress0, :Adress1, :Date0, :EmptyDate, :Time0, 0, :ACDoc, :Number, " +
                        "0, :BindingAdressFlag, :UnitID); ";
                    QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
                    QuerySetParam(ref TextQuery, "LineNo_", NewLineNo_);
                    QuerySetParam(ref TextQuery, "Item", AcceptedItem.ID);
                    QuerySetParam(ref TextQuery, "Count", CurrCount - UpdateCount);
                    QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
                    QuerySetParam(ref TextQuery, "Coef", 1);
                    QuerySetParam(ref TextQuery, "State0", dr["State0"]);
                    QuerySetParam(ref TextQuery, "State1", dr["State1"]);
                    QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                    QuerySetParam(ref TextQuery, "Adress0", dr["Adress0"]);
                    QuerySetParam(ref TextQuery, "Adress1", dr["Adress1"]);
                    QuerySetParam(ref TextQuery, "Date0", dr["Date0"]);
                    QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                    QuerySetParam(ref TextQuery, "Time0", dr["Time0"]);
                    QuerySetParam(ref TextQuery, "ACDoc", dr["ACID"]);
                    QuerySetParam(ref TextQuery, "Number", dr["Number"]);
                    QuerySetParam(ref TextQuery, "UnitID", dr["UnitID"]);
                    QuerySetParam(ref TextQuery, "BindingAdressFlag", dr["BindingAdressFlag"]);
                    if (!ExecuteWithoutRead(TextQuery))
                    {
                        return false;
                    }
                }

                if (Count == 0)
                {
                    break;
                }
            }

            //begin internal command
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(AcceptedItem.ID, "Спр.Товары");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(ATDoc.ID, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "PutItem (Выложил на полку)";
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход2"]       = ScanAdress.ID;
            if (!ExecCommandNoFeedback("Internal", DataMapWrite))
            {
                return false;
            }
            //end internal command
            return CompleteAccept();
        } // EnterAdress()
        public bool DeleteRowTransferItem(DataRow currRow)
        {
            //удалять из телеги нельзя, пикнул паллету мучийся
            FExcStr = "Нельзя удалять товар из телеги"; 
            return false;
            //return DeleteRowTransferItem(currRow["ID"].ToString(), currRow["Count"].ToString());           
        } // DeleteRowTransferItem()
        private bool DeleteRowTransferItem(string ItemID, string DelCount)
        {
            FExcStr = null;
            string TextQuery = "DELETE FROM DT$АдресПеремещение " +
                "WHERE DT$АдресПеремещение .iddoc = :Doc " +
                "and DT$АдресПеремещение .$АдресПеремещение.Товар = :Item " +
                "and DT$АдресПеремещение .$АдресПеремещение.Дата1 = :EmptyDate ";
            QuerySetParam(ref TextQuery, "Doc", ATDoc.ID);
            QuerySetParam(ref TextQuery, "Item", ItemID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            //begin internal command
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"] = ExtendID(ItemID, "Спр.Товары");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"] = ExtendID(ATDoc.ID, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = "RemovedItem (Удалил из тележки)";
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход2"] = DelCount;
            if (!ExecCommandNoFeedback("Internal", DataMapWrite))
            {
                return false;
            }
            //end internal command
            return ToModeTransfer(ATDoc, Mode.Transfer);   //рефрешим табличку
        } // DeleteRowTransferItem()
        public bool DeleteRowSampleItem(DataRow currRow)
        {
            FExcStr = null;
            string TextQuery = "DELETE FROM DT$АдресПеремещение " + 
                "WHERE DT$АдресПеремещение .iddoc = :Doc and DT$АдресПеремещение .LineNo_= :LineNo_";
            QuerySetParam(ref TextQuery, "Doc", SampleDoc);
            QuerySetParam(ref TextQuery, "LineNo_", currRow["LINENO_"]);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            //begin internal command
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(currRow["ID"].ToString(), "Спр.Товары");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(SampleDoc, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "RemovedSample (Отменил выкладку образца)";
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход2"]       = currRow["Count"].ToString();
            if (!ExecCommandNoFeedback("Internal", DataMapWrite))
            {
                return false;
            }
            //end internal command
            return ToModeSamplePut();   //рефрешим табличку
        } // DeleteRowTransferItem()
        public bool DeleteRowAcceptedItems(DataRow currRow)
        {
            if (CurrentMode != Mode.Acceptance)
            {
               return DeleteRowAcceptedItemsCross(currRow);
            }

            string TextQuery = 
                "BEGIN TRAN; " +
                "IF EXISTS(SELECT LineNo_ FROM DT$АдресПоступление as ACDT " + 
                                "WHERE ACDT.IDDOC = :ACID " +
                                    "and ACDT.$АдресПоступление.Товар = :ItemID " + 
                                    "and ACDT.$АдресПоступление.Состояние0 = 0) " +
                    "BEGIN " +
                    "UPDATE DT$АдресПоступление " +
                        "SET $АдресПоступление.Количество = $АдресПоступление.Количество + :Count " +
                        "WHERE DT$АдресПоступление .iddoc = :ACID " + 
                            "and DT$АдресПоступление .$АдресПоступление.Товар = :ItemID " + 
                            "and DT$АдресПоступление .$АдресПоступление.Состояние0 = 0; " +
                    "DELETE FROM DT$АдресПоступление " + 
                        "WHERE DT$АдресПоступление .iddoc = :ACID " + 
                            "and DT$АдресПоступление .lineno_ = :lineno_ " +
                    //Закомментировано соблюдение порядка строк, т.к. это опасно!
                    //"UPDATE DT$АдресПоступление " + 
                    //    "SET lineno_ = lineno_ - 1 " +
                    //    "WHERE DT$АдресПоступление .iddoc = :ACID " +
                    //        "and DT$АдресПоступление .lineno_ > :lineno_ " + 
                    "END ELSE BEGIN " +
                        "UPDATE DT$АдресПоступление " + 
                        "SET " +
                            "$АдресПоступление.Сотрудник0 = :EmptyID," + 
                            "$АдресПоступление.Дата0 = :VoidDate," + 
                            "$АдресПоступление.Время0 = 0," + 
                            "$АдресПоступление.Состояние0 = 0," + 
                            "$АдресПоступление.КоличествоЭтикеток = 0," + 
                            "$АдресПоступление.ФлагПечати = 0, " +
                            "$АдресПоступление.Паллета = :EmptyID " +
                        "WHERE " +
                            "DT$АдресПоступление .iddoc = :ACID " + 
                            "and DT$АдресПоступление .lineno_ = :lineno_ ; " + 
                    "END; " +
                "COMMIT TRAN;";
            QuerySetParam(ref TextQuery, "ACID",       currRow["IDDOC"]);
            QuerySetParam(ref TextQuery, "Count",      currRow["Count"]);
            QuerySetParam(ref TextQuery, "ItemID",     currRow["ID"]);
            QuerySetParam(ref TextQuery, "lineno_",    currRow["LINENO_"]);
            QuerySetParam(ref TextQuery, "EmptyID",    GetVoidID());
            QuerySetParam(ref TextQuery, "VoidDate",   GetVoidDate());
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            if (CurrentMode != Mode.Acceptance)
            {
                //если это кросс докинг, то надо еще из заказа удалить
                TextQuery =
                "UPDATE DT$ЗаказНаКлиента " +
                "SET " +
                     "$ЗаказНаКлиента.Принято = $ЗаказНаКлиента.Принято - :Count " +
                "WHERE " +
                    "DT$ЗаказНаКлиента .iddoc = :Doc " +
                    "and $ЗаказНаКлиента.Товар = :Item";
                QuerySetParam(ref TextQuery, "Item", currRow["ID"]);
                QuerySetParam(ref TextQuery, "Count", currRow["Count"]);
                QuerySetParam(ref TextQuery, "Doc", currRow["OrderID"]);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }

                return ToModeAcceptanceCross();
            }
            else
            {
                return ToModeAcceptance();
            }
        } // DeleteRowAcceptedItems()
        public bool DeleteRowAcceptedItemsCross(DataRow currRow)
        {
            //Нуно создать новую строку
            string TextQuery =
                "SELECT max(DT$АдресПоступление .lineno_) + 1 as NewLineNo_ " +
                "FROM DT$АдресПоступление WHERE  DT$АдресПоступление .iddoc = :Doc";
            QuerySetParam(ref TextQuery, "Doc", currRow["IDDOC"].ToString());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            int NewLineNo_ = (int)DT.Rows[0]["NewLineNo_"];

            TextQuery =
                "BEGIN TRAN; " +
                "IF EXISTS(SELECT LineNo_ FROM DT$АдресПоступление as ACDT " +
                                "WHERE ACDT.IDDOC = :ACID " +
                                    "and ACDT.$АдресПоступление.Товар = :ItemID " +
                                    "and ACDT.$АдресПоступление.Состояние0 = 0) " +
                    "BEGIN " +
                //увеличиваем количество не принятого
                    "UPDATE DT$АдресПоступление " +
                        "SET $АдресПоступление.Количество = $АдресПоступление.Количество + :Count " +
                        "WHERE DT$АдресПоступление .iddoc = :ACID " +
                            "and DT$АдресПоступление .$АдресПоступление.Товар = :ItemID " +
                            "and DT$АдресПоступление .$АдресПоступление.Состояние0 = 0; " +
                //уменьшаем количество принятого
                    "UPDATE DT$АдресПоступление " +
                        "SET $АдресПоступление.Количество = $АдресПоступление.Количество - :Count " +
                            "WHERE DT$АдресПоступление .iddoc = :ACID " +
                                "and DT$АдресПоступление .lineno_ = :lineno_ " +
                //Удаляем пустые строки
                    "DELETE FROM DT$АдресПоступление " +
                        "WHERE DT$АдресПоступление .iddoc = :ACID " +
                            "and $АдресПоступление.Количество = 0 " +
                    "END ELSE BEGIN " +
                //сначала делаем строку принятого товара внизу
                        "INSERT INTO DT$АдресПоступление " +
                        "SELECT DocAP.IDDOC, :NewLineNo_, :Number, " +
                            ":ItemID, DocAP.$АдресПоступление.Количество - :Count, " +
                            ":EmptyID, DocAP.$АдресПоступление.Коэффициент , 1, " +
                            "DocAP.$АдресПоступление.Сотрудник0 , DocAP.$АдресПоступление.Адрес0 , DocAP.$АдресПоступление.Дата0 , " +
                            "DocAP.$АдресПоступление.Время0 , DocAP.$АдресПоступление.ФлагПечати , DocAP.$АдресПоступление.КоличествоЭтикеток , " +
                            "DocAP.$АдресПоступление.ЕдиницаШК , 0, 0, DocAP.$АдресПоступление.Паллета " +                            
                        "FROM DT$АдресПоступление as DocAP " +
                            "WHERE DocAP.IDDOC = :ACID " +
                                "and DocAP.lineno_ = :lineno_ " +
                //Делаем строку не принятого товара
                        "UPDATE DT$АдресПоступление " +
                        "SET $АдресПоступление.Количество = :Count, " +
                            "$АдресПоступление.Сотрудник0 = :EmptyID, "+
                            "$АдресПоступление.Состояние0 = 0, " +
                            "$АдресПоступление.Адрес0 = :EmptyID, " +
                            "$АдресПоступление.Дата0 = :EmptyDate, " +
                            "$АдресПоступление.Время0 = 0, " +
                            "$АдресПоступление.ФлагПечати = 0, " +
                            "$АдресПоступление.КоличествоЭтикеток = 0, " +
                            "$АдресПоступление.Паллета = :EmptyID " +
                        "WHERE DT$АдресПоступление .iddoc = :ACID " +
                                "and DT$АдресПоступление .lineno_ = :lineno_ " +
                //Удаляем пустые строки
                    "DELETE FROM DT$АдресПоступление " +
                        "WHERE DT$АдресПоступление .iddoc = :ACID " +
                            "and $АдресПоступление.Количество = 0 " +

                    "END; " +
                //обновляем заказ
                    "UPDATE DT$ЗаказНаКлиента " +
                        "SET " +
                        "$ЗаказНаКлиента.Принято = $ЗаказНаКлиента.Принято - :Count " +
                    "WHERE " +
                        "DT$ЗаказНаКлиента .iddoc = :OrderID " +
                        "and $ЗаказНаКлиента.Товар = :ItemID " +
                "COMMIT TRAN;";
            QuerySetParam(ref TextQuery, "ACID", currRow["IDDOC"]);
            QuerySetParam(ref TextQuery, "Count", currRow["Count"]);
            QuerySetParam(ref TextQuery, "ItemID", currRow["ID"]);
            QuerySetParam(ref TextQuery, "lineno_", currRow["LINENO_"]);
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
            QuerySetParam(ref TextQuery, "NewLineNo_", NewLineNo_);
            QuerySetParam(ref TextQuery, "VoidDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "Number", currRow["Number"]);
            QuerySetParam(ref TextQuery, "OrderID", currRow["OrderID"]);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            return ToModeAcceptanceCross();
        }// DeleteRowAcceptedItems()
        public bool CompleteTransfer()
        {
            FExcStr = null;
            string TextQuery;
            if (FTransferItems.Rows.Count == 0 && FTransferReadyItems.Rows.Count == 0)
            {
                //Все пусто! это отмена разноса!
                TextQuery =
                    "BEGIN TRAN; " +
                    "UPDATE DH$АдресПеремещение " +
                        "SET " + 
                            "$АдресПеремещение.Склад = :EmptyID, " +
                            "$АдресПеремещение.СкладПолучатель = :EmptyID, " +
                            "$АдресПеремещение.ТипДокумента = 0 " +
                        "WHERE " + 
                            "DH$АдресПеремещение .iddoc = :IDDoc; " +
                    "UPDATE _1sjourn " +
                        "SET _1sjourn.$Автор = :EmptyID WHERE _1sjourn.iddoc = :IDDoc; " +
                    "COMMIT TRAN;";
                    QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
                    QuerySetParam(ref TextQuery, "IDDoc", ATDoc.ID);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                return ToModeTransferInicialize();
            }
            if (FTransferReadyItems.Rows.Count == 0)
            {
                FExcStr = "Нет разнесенных позиций!";
                return false;
            }
            if (FTransferItems.Rows.Count != 0)
            {
                FExcStr = "Выкладка не завершена!";
                return false;
            }

            //Проверим, возможно такая команда уже была послана
            TextQuery =
                "SELECT ID as ID " + 
                    "FROM $Спр.СинхронизацияДанных (nolock INDEX=VI" + GetSynh("Спр.СинхронизацияДанных.ДокументВход").Substring(2) + ") " + 
                    "WHERE " +
                        "$Спр.СинхронизацияДанных.ФлагРезультата in (1,2,3) " + 
                        "and $Спр.СинхронизацияДанных.ДокументВход = :IDDoc " + 
                        "and descr = 'AdressDistribution'";
            QuerySetParam(ref TextQuery, "IDDoc", ExtendID(ATDoc.ID, "АдресПеремещение"));
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            //Дожидаемся ответа команды
            Dictionary<string, object> DataMapRead;
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(ATDoc.ID, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Printer.ID, "Спр.Принтеры");
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            //Либо создаем новую, либо цепляемся к уже посланной
            if (DT.Rows.Count > 0)
            {
                if (!ExecCommand("AdressDistribution", DataMapWrite, FieldList, out DataMapRead, DT.Rows[0]["ID"].ToString()))
                {
                    return false;
                }
            }
            else
            {
                if (!ExecCommand("AdressDistribution", DataMapWrite, FieldList, out DataMapRead))
                {
                    return false;
                }
            }
            //анализ результата
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
            if (!ToModeTransferInicialize())
            {
                return false;
            }
            QuitModeTransfer();
            return true;
        } // CompleteTransfer()
        public bool CompleteNewInventory()
        {
            FExcStr = null;
            string TextQuery;

            //Проверим, возможно такая команда уже была послана
            TextQuery =
                "SELECT ID as ID " + 
                    "FROM $Спр.СинхронизацияДанных (nolock INDEX=VI" + GetSynh("Спр.СинхронизацияДанных.ДокументВход").Substring(2) + ") " + 
                    "WHERE " +
                        "$Спр.СинхронизацияДанных.ФлагРезультата in (1,2,3) " + 
                        "and $Спр.СинхронизацияДанных.ДокументВход = :IDDoc " + 
                        "and descr = 'Inventory'";
            QuerySetParam(ref TextQuery, "IDDoc", ExtendID(ATDoc.ID, "АдресПеремещение"));
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            //Дожидаемся ответа команды
            Dictionary<string, object> DataMapRead;
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(ATDoc.ID, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Printer.ID, "Спр.Принтеры");
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            //Либо создаем новую, либо цепляемся к уже посланной
            if (DT.Rows.Count > 0)
            {
                if (!ExecCommand("Inventory", DataMapWrite, FieldList, out DataMapRead, DT.Rows[0]["ID"].ToString()))
                {
                    return false;
                }
            }
            else
            {
                if (!ExecCommand("Inventory", DataMapWrite, FieldList, out DataMapRead))
                {
                    return false;
                }
            }
            //анализ результата
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
            if (!ToModeNewChoiseInventory())
            {
                return false;
            }
            QuitModeTransfer();
            return true;
        } // CompleteNewInventory()
        public bool CompleteSamplePut()
        {
            FExcStr = null;
            //условие ниже - это костыль, коих тут очень много... мда, печально, но что поделать?
            //Когда-нибудь я все тут перепишу и все тут будет красиво, очень! Я верю в это...
            if (SampleDoc == null)
            {
                FExcStr = "Ошибка связи, перезайдите в режим выкладки образцов!";
                return false;
            }
            string TextQuery;
            if (FTransferReadyItems.Rows.Count == 0)
            {
                FExcStr = "Нет выложенных образцов!";
                return false;
            }

            //Проверим, возможно такая команда уже была послана
            TextQuery =
                "SELECT ID as ID " + 
                    "FROM $Спр.СинхронизацияДанных (nolock INDEX=VI" + GetSynh("Спр.СинхронизацияДанных.ДокументВход").Substring(2) + ") " + 
                    "WHERE " +
                        "$Спр.СинхронизацияДанных.ФлагРезультата in (1,2,3) " + 
                        "and $Спр.СинхронизацияДанных.ДокументВход = :IDDoc " + 
                        "and descr = 'AdressDistributionSample'";
            QuerySetParam(ref TextQuery, "IDDoc", ExtendID(SampleDoc, "АдресПеремещение"));
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            //Дожидаемся ответа команды
            Dictionary<string, object> DataMapRead;
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(SampleDoc, "АдресПеремещение");
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            //Либо создаем новую, либо цепляемся к уже посланной
            if (DT.Rows.Count > 0)
            {
                if (!ExecCommand("AdressDistributionSample", DataMapWrite, FieldList, out DataMapRead, DT.Rows[0]["ID"].ToString()))
                {
                    return false;
                }
            }
            else
            {
                if (!ExecCommand("AdressDistributionSample", DataMapWrite, FieldList, out DataMapRead))
                {
                    return false;
                }
            }
            //анализ результата
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
            QuitModeSamplePut();
            //Если команда ниже не отработает - то наступит как бэ пиздец, но да ладно...
            if (!ToModeSamplePut())
            {
                return false;
            }
            return true;
        } // CompleteSamplePut()
        public bool AddSample(string ItemID)
        {
            //FExcStr - несет смысл
            DataRow[] DR = FSampleItems.Select("ID = '" + ItemID + "'");
            if (DR.Length == 0)
            {
                //нет в списке образцов (рефрешим табличку)
                if (!ToModeSamplePut())
                {
                    return false;
                }
                DR = FSampleItems.Select("ID = '" + ItemID + "'");
                if (DR.Length == 0)
                {
                    //После обновления - один хуй нет, пишем бороду
                    FExcStr = "Товара нет в списке выставляемых на образец!";
                    return false;
                }
            }
            string InvCode = DR[0]["InvCode"].ToString().Trim();
            string Adress = DR[0]["Adress"].ToString();

            //Фильтр по документам!!!
            DR = SampleTransfers.Select();
            string Docs = "";
            foreach (DataRow dr in DR)
            {
                Docs += ", '" + dr["ATID"].ToString() + "'";
            }
            Docs = Docs.Substring(2);   //Убираем спедери запятые

            //ПРОБИВАЕМ ОСТАТКИ ПО АДРЕС ПЕРЕМЕЩЕНИЕ
            string TextQuery = 
                "DECLARE @curdate DateTime; " + 
                "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " + 
                "SELECT " +
                    ":ItemID as ID, " + 
                    "RegAT.$Рег.АдресОстаткиПеремещения.Док as ATID, " + 
                    "sum(RegAT.$Рег.АдресОстаткиПеремещения.Количество ) as Count, " + 
                    "min(Goods.$Спр.Товары.БазоваяЕдиницаШК ) as UnitID " +
                    //"DocAT.$АдресПеремещение.Состояние1 as State1 " + 
                "FROM " + 
                    "RG$Рег.АдресОстаткиПеремещения as RegAT (nolock) " +
                "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                    "ON Goods.id = RegAT.$Рег.АдресОстаткиПеремещения.Товар " + 
                "WHERE " +
                    "RegAT.period = @curdate " + 
                    "and RegAT.$Рег.АдресОстаткиПеремещения.Товар = :ItemID " +
                    "and RegAT.$Рег.АдресОстаткиПеремещения.Склад = :Warehouse " +
                    "and RegAT.$Рег.АдресОстаткиПеремещения.ТипДействия = 4 " +
                    "and not RegAT.$Рег.АдресОстаткиПеремещения.Количество = 0 " +
                    "and RegAT.$Рег.АдресОстаткиПеремещения.Док in (:Docs) " + 
                "GROUP BY " +
                    "RegAT.$Рег.АдресОстаткиПеремещения.Товар , " + 
                    "RegAT.$Рег.АдресОстаткиПеремещения.Док " + 
                    //"DocAC.lineno_ " +
                "ORDER BY " + 
                    "DocAC.Count desc";
            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            QuerySetParam(ref TextQuery, "Warehouse", Const.MainWarehouse);
            TextQuery = TextQuery.Replace(":Docs", Docs);
            DataTable AddPartyDT;
            if (!ExecuteWithRead(TextQuery, out AddPartyDT))
            {
                return false;
            }
            if (AddPartyDT.Rows.Count == 0)
            {
                FExcStr = "Товара нет в списке выставляемых на образец!";
                return false;
            }

            //ПРОБИВАЕМ СКОЛЬКО УЖЕ РАЗНОСИТСЯ
            //Для простоты будем считать что на каждого сотрудника максимум по одному документу на 80-ом,
            //  вообщем то, исходя из логике движка - так и есть
            TextQuery = 
                "SELECT " + 
                    "Employers.id as EmployerID, " +
                    "Employers.descr as Employer, " +
                    "DocAT.$АдресПеремещение.Количество  as Count, " +
                    "substring(DocAT.$АдресПеремещение.Док , 5, 9) as ATID " + 
                "FROM " +
                    "_1sjourn as Journ (nolock) " +
                    "INNER JOIN DT$АдресПеремещение as DocAT (nolock) " + 
                        "ON DocAT.iddoc = Journ.iddoc " +
                    "INNER JOIN DH$АдресПеремещение as DocATHead (nolock) " + 
                        "ON DocATHead.iddoc = Journ.iddoc " +
                    "LEFT JOIN $Спр.Сотрудники as Employers (nolock) " + 
                        "ON Employers.id = Journ.$Автор " + 
                "WHERE " + 
                    "Journ.date_time_iddoc < '19800101Z' " + 
                    "and not Journ.$Автор = :EmptyID " + 
                    "and DocAT.$АдресПеремещение.Товар = :ItemID " +
                    "and DocATHead.$АдресПеремещение.ТипДокумента = 4 " +
                //"GROUP BY " +
                    //"Journ.iddoc " + 
                "ORDER BY " + 
                    "Employers.id ";
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
            QuerySetParam(ref TextQuery, "ItemID", ItemID);
            DataTable TransferDT;
            if (!ExecuteWithRead(TextQuery, out TransferDT))
            {
                return false;
            }

            string strTransfer  = "";
            int CountTransfer   = 0;
            if (TransferDT.Rows.Count > 0)
            {
                string CurrEmployerID   = TransferDT.Rows[0]["EmployerID"].ToString();
                string CurrEmployerFIO  = Helper.GetShortFIO(TransferDT.Rows[0]["Employer"].ToString());
                int CurrCountTransfer   = 0;
                for (int row = 0; row < TransferDT.Rows.Count; row++)
                {
                    if (CurrEmployerID != TransferDT.Rows[row]["EmployerID"].ToString())
                    {
                        strTransfer         += ", " + CurrEmployerFIO + " " + GetStrPackageCount(CurrCountTransfer, 1);
                        CurrCountTransfer   = 0;
                        CurrEmployerID      = TransferDT.Rows[row]["EmployerID"].ToString();
                        CurrEmployerFIO     = Helper.GetShortFIO(TransferDT.Rows[row]["Employer"].ToString());
                    }
                    CountTransfer       += (int)(decimal)TransferDT.Rows[row]["Count"];
                    CurrCountTransfer   += (int)(decimal)TransferDT.Rows[row]["Count"];
                }
                strTransfer += ", " + CurrEmployerFIO + " " + GetStrPackageCount(CurrCountTransfer, 1);
                strTransfer = "Уже выкладывается: " + strTransfer.Substring(2);
            }

            //ПОЕХАЛИ ОПЯТЬ ПРИЕМКУ РАСЧИТЫВАТЬ
            int AllCount    = 0;
            for (int row = 0; row < AddPartyDT.Rows.Count; row++)
            {
                AllCount    += (int)(decimal)AddPartyDT.Rows[row]["Count"];
            }

            int CountAdd = AllCount - CountTransfer; //Осталось выложить
            if (CountAdd <= 0)
            {
                FExcStr = InvCode + ": 0 шт.. " + strTransfer;
                return false;
            }
            
            /////////////////////////////////////////////////////
            //А ТЕПЕРЬ ПИШЕМ ЭТО ГОВНО В ДОКУМЕНТ
            //
            //ДОБАВЛЯЕМ НОВУЮ или даже НОВЫЕ СТРОКИ
            TextQuery =
                "SELECT ISNULL(max(DT$АдресПеремещение .lineno_) + 1, 1) as LineNo_ " +
                "FROM DT$АдресПеремещение (nolock) " + 
                "WHERE DT$АдресПеремещение .iddoc = :Doc";
            QuerySetParam(ref TextQuery, "Doc", SampleDoc);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            int LineNo_ = (int)DT.Rows[0]["LineNo_"];

            string CurrATID = "";
            string strAccepted = "";
            int Count;
            DR = AddPartyDT.Select();
            foreach (DataRow dr in DR)
            {
                if (AllCount <= 0)
                {
                    break;
                }
                Count           = (int)(decimal)dr["Count"];    //По данной строке документа висит столько
                if (Count > AllCount)
                {
                    Count       = AllCount;
                    AllCount   = 0;
                }
                else
                {
                    AllCount -= Count;
                }

                if (CurrATID != dr["ATID"].ToString())
                {
                    CountTransfer   = 0;                            //Сколько разносится по данному документу
                    DataRow[] tmpDR = TransferDT.Select("ATID = '" + dr["ATID"].ToString() + "'");
                    foreach (DataRow tmpdr in tmpDR)
                    {
                        CountTransfer += (int)(decimal)tmpdr["Count"];
                    }
                    CurrATID = dr["ATID"].ToString();
                }
                if (Count <= CountTransfer)
                {
                    CountTransfer -= Count;
                    continue;   //Все разносится, добалвять не будем
                }
                Count -= CountTransfer; //Уменьшим на разносимое количество
                CountTransfer = 0;
                strAccepted += " + " + GetStrPackageCount((int)(decimal)Count, 1); //Формируем строку вывода
                TextQuery =
                    "INSERT INTO DT$АдресПеремещение VALUES " +
                        "(:Doc, :LineNo_, :Item, :Count, :EmptyID, :Coef, 3, 2, :Employer, " + 
                        ":Adress0, :Adress1, :Date0, :EmptyDate, :Time0, 0, :ACDoc, :Number, " +
                        ":SampleCount, :BindingAdressFlag, :UnitID); ";
                QuerySetParam(ref TextQuery, "Doc", SampleDoc);
                QuerySetParam(ref TextQuery, "LineNo_", LineNo_);
                QuerySetParam(ref TextQuery, "Item", dr["ID"]);
                QuerySetParam(ref TextQuery, "Count", Count);
                QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
                QuerySetParam(ref TextQuery, "Coef", 1);
                //QuerySetParam(ref TextQuery, "State0", dr["State1"]);
                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "Adress0", Adress);
                QuerySetParam(ref TextQuery, "Adress1", SampleAdress);
                QuerySetParam(ref TextQuery, "Date0", DateTime.Now);
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                QuerySetParam(ref TextQuery, "Time0", APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "ACDoc", ExtendID(dr["ATID"].ToString(), "АдресПеремещение"));
                QuerySetParam(ref TextQuery, "Number", LineNo_);
                QuerySetParam(ref TextQuery, "UnitID", dr["UnitID"]);
                QuerySetParam(ref TextQuery, "SampleCount", 0);
                QuerySetParam(ref TextQuery, "BindingAdressFlag", 0);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                ++LineNo_;
            }
            strAccepted = strAccepted.Substring(3);
            FExcStr = InvCode + ": " + GetStrPackageCount(CountAdd, 1) +
                        ", ДОБАВЛЕНО: " + strAccepted + ". " + strTransfer;

            //begin internal command
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(ItemID, "Спр.Товары");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(SampleDoc, "АдресПеремещение");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "PutSample (Выложил образец)";
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход2"]       = CountAdd.ToString();  //Сколько
            if (!ExecCommandNoFeedback("Internal", DataMapWrite))
            {
                return false;
            }
            //end internal command

            return ToModeSamplePut();   //рефрешим табличку
        } // AddSample()
        public bool PrintPrices()
        {
            //Формируем строку с ид-шниками АдресовПоступления
            string strACID = "";
            for (int i = 0; i < SampleTransfers.Rows.Count; i++)
            {
                strACID += SampleTransfers.Rows[i]["ATID"].ToString().Trim() + ",";
            }
            strACID = strACID.Substring(0, strACID.Length - 1);

            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = strACID;
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(Printer.ID, "Спр.Принтеры");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");

            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("AdressPrintLabels", DataMapWrite, FieldList, out DataMapRead ))
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
            FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
            return true;
        }
        public bool RequestHarmonization()
        {
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"]    = ExtendID(WarehouseForAddressItem.ID, "Спр.Склады");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "Harmonization";
            
            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("QuestHarmonization", DataMapWrite, FieldList, out DataMapRead ))
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
            FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
            FCurrentMode = Mode.ChoiseWork; //На всякий случай
            return ToModeHarmonizationInicialize();
        }
        public bool RequestHarmonization(string ID)
        {
            DataRow[] DR = WarehousesFrom.Select("ID = '" + ID + "'");
            WarehouseForAddressItem = new RefWarehouse(this);
            WarehouseForAddressItem.FoundID(ID);
            return RequestHarmonization();
        }
        public bool RequestJob()
        {
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "Inventory";

            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("QuestInventory", DataMapWrite, FieldList, out DataMapRead ))
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
            FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
            return ToModeNewChoiseInventory();
        }
        public bool CompleteHarmonization()
        {
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(ATDoc.ID, "АдресПеремещение");;
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "HarmonizationComplete1";

            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("HarmonizationComplete1", DataMapWrite, FieldList, out DataMapRead ))
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
            FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
            FCurrentMode = Mode.ChoiseWork; //инициируем вылет в выбор работы, т.к. строка ниже может выдать перл!
            return ToModeHarmonizationInicialize();
        }
        public bool CompleteHarmonizationPut()
        {
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"]    = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"]    = ExtendID(ATDoc.ID, "АдресПеремещение");;
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"]       = "HarmonizationComplete2";

            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("HarmonizationComplete2", DataMapWrite, FieldList, out DataMapRead ))
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
            FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
            return ToModeHarmonizationInicialize();
        }
        
        private string GiveSomeOneQueryText()
        {
            string idSchet = "   " + Translation.DecTo36(GetSynh("Счет"));
            idSchet = idSchet.Substring(idSchet.Length - 4);
            string idClaim = "   " + Translation.DecTo36(GetSynh("ПретензияОтКлиента"));
            idClaim = idClaim.Substring(idClaim.Length - 4);

            return "SELECT " +
                        "PL.$ПутевойЛист.ИндексРазгрузки as AdressCounter, " + 
                        "CASE " + 
                            "WHEN journ.iddocdef = $Счет THEN '" + idSchet + "' + journ.iddoc " +
                            "WHEN journ.iddocdef = $ПретензияОтКлиента THEN '" + idClaim + "' + Claim.iddoc " +
                            "WHEN journ.iddocdef = $РасходнаяРеализ THEN '" + idSchet + "' + journProposal_RR.iddoc " +
                            "WHEN journ.iddocdef = $Перемещение THEN '" + idSchet + "' + journProposal_Per.iddoc " +
                            "WHEN not journProposal.iddoc is null THEN '" + idSchet + "' + journProposal.iddoc " +
                            "ELSE '   0' + :EmptyID END as DocFull, " +
                        "CASE " + 
                            "WHEN journ.iddocdef = $Счет THEN journ.iddoc " +
                            "WHEN journ.iddocdef = $ПретензияОтКлиента THEN Claim.iddoc " +
                            "WHEN journ.iddocdef = $РасходнаяРеализ THEN journProposal_RR.iddoc " +
                            "WHEN journ.iddocdef = $Перемещение THEN journProposal_Per.iddoc " +
                            "WHEN not journProposal.iddoc is null THEN journProposal.iddoc " +
                            "ELSE :EmptyID END as Doc, " +
                        "ISNULL(RK.$РасходнаяКредит.АдресДоставки , Bill.$Счет.АдресДоставки ) as Adress, " + 
                        "PL.lineno_ as Number " + 
                    "FROM DT$ПутевойЛист as PL (nolock) " +
                        "LEFT JOIN _1sjourn as journ (nolock) " + 
                            "ON journ.iddoc = right(PL.$ПутевойЛист.ДокументДоставки , 9) " +
                        "LEFT JOIN DH$РасходнаяКредит as RK (nolock) " + 
                            "ON RK.iddoc = journ.iddoc " +
                        "LEFT JOIN DH$РасходнаяРеализ as RR (nolock) " + 
                            "ON RR.iddoc = journ.iddoc " +
                        "LEFT JOIN DH$Перемещение as Per (nolock) " + 
                            "ON Per.iddoc = journ.iddoc " +
                        "LEFT JOIN _1sjourn as journProposal (nolock) " +
                            "ON right(RK.$РасходнаяКредит.ДокументОснование , 9) = journProposal.iddoc " +
                        "LEFT JOIN _1sjourn as journProposal_RR (nolock) " +
                            "ON right(RR.$РасходнаяРеализ.ДокументОснование , 9) = journProposal_RR.iddoc " +
                        "LEFT JOIN _1sjourn as journProposal_Per (nolock) " +
                            "ON right(Per.$Перемещение.ДокументОснование , 9) = journProposal_Per.iddoc " +
                        "LEFT JOIN DH$Счет as Bill (nolock) " +
                            "ON Bill.iddoc = journProposal.iddoc or Bill.iddoc = journ.iddoc " +
                        "LEFT JOIN DH$ПретензияОтКлиента as Claim (nolock) " + 
                            "ON Claim.iddoc = journ.iddoc " + 
                    "WHERE " +
                        "PL.iddoc = :iddoc " +
                        "and journ.iddocdef in ($Счет , $РасходнаяКредит , $ПретензияОтКлиента , $РасходнаяРеализ , $Перемещение )";
        }
        public bool CompleteLoadingInicialization()
        {
            if (Placer.ID == Employer.ID)
            {
                FExcStr = "Пользователь совпадает с укладчиком! Извини друг, так нельзя!";
                return false;
            }

            if (WayBill.ID == null)
            {
                FExcStr = "Не выбран путевой лист!";
                return false;
            }
            //Проверяем блокировку
            //временно отключено с релиза 4.47
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
            if (Int32.Parse(ExecuteScalar(TextQuery).ToString()) == 0)
            {
                //погрузка не разрешена
                FExcStr = "Погрузка запрещена!";
                return false;
            }          

            //Проверим еще раз не засрал ли путевой кто-нибудь еще
             TextQuery =
                "SELECT " +
                    "PL.iddoc as iddoc " +
                "FROM DH$ПутевойЛист as PL (nolock) " +
                    "INNER JOIN _1sjourn as journ (nolock) " +
                        "ON journ.iddoc = PL.iddoc " +
                "WHERE " +
                    "PL.$ПутевойЛист.Дата1 = :EmptyDate " +
                    "and journ.ismark = 0 " +
                    "and journ.iddoc = :iddoc " +
                "ORDER BY journ.date_time_iddoc";
            QuerySetParam(ref TextQuery, "EmptyDate",   GetVoidDate());
            QuerySetParam(ref TextQuery, "iddoc",       WayBill.ID);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count == 0)
            {
                //временно отключено с релиза 4.47
                //LockoutDoc(WayBill.ID);
                //FExcStr = "Этот путевой уже сцапали или он помечен на удаление!";
                //return false;

                //уже кто-то взял поймем куда нас пихать если он еще не закрыт
                TextQuery =
                "SELECT " +
                    "$ПутевойЛист.Грузчик as Loader, " +
                    "$ПутевойЛист.Укладчик as Placer, " +
                    "$ПутевойЛист.Укладчик2 as Placer2, " +
                    "$ПутевойЛист.Укладчик3 as Placer3 " +
                "FROM DH$ПутевойЛист as PL (nolock) " +
                    "INNER JOIN _1sjourn as journ (nolock) " +
                        "ON journ.iddoc = PL.iddoc " +
                "WHERE " +
                    "PL.$ПутевойЛист.Дата2 = :EmptyDate " +
                    "and journ.ismark = 0 " +
                    "and journ.iddoc = :iddoc " +
                    "and ($ПутевойЛист.Грузчик = :EmptyLoader " +
                    "or $ПутевойЛист.Укладчик = :EmptyLoader " +
                    "or $ПутевойЛист.Укладчик2 = :EmptyLoader " +
                    "or $ПутевойЛист.Укладчик3 = :EmptyLoader )" +
                "ORDER BY journ.date_time_iddoc";
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                QuerySetParam(ref TextQuery, "iddoc", WayBill.ID);
                QuerySetParam(ref TextQuery, "EmptyLoader", GetVoidID());
                
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                if (DT.Rows.Count == 0)
                {
                    FExcStr = "Путевой закрыт, удален или укомплектован сотрудникаим!";
                    return false;
                }

                TextQuery =
                   "UPDATE DH$ПутевойЛист " +
                       "SET " +
                           "$ПутевойЛист.Грузчик    = :Loader, " +
                           "$ПутевойЛист.Укладчик   = :Placer_1, " +
                           "$ПутевойЛист.Укладчик2  = :Placer_2, " +
                           "$ПутевойЛист.Укладчик3  = :Placer_3 " +
                       "WHERE " +
                           "DH$ПутевойЛист .iddoc = :iddoc;";
                bool findeEmpty = false;
                if (DT.Rows[0]["Loader"].ToString() == GetVoidID())
                {
                    QuerySetParam(ref TextQuery, "Loader", Employer.ID);
                    findeEmpty = true;
                }
                else
                {
                    QuerySetParam(ref TextQuery, "Loader", DT.Rows[0]["Loader"].ToString());
                }
                if (DT.Rows[0]["Placer"].ToString() == GetVoidID() && !findeEmpty)
                {
                    QuerySetParam(ref TextQuery, "Placer_1", Employer.ID);
                    findeEmpty = true;
                }
                else
                {
                    QuerySetParam(ref TextQuery, "Placer_1", DT.Rows[0]["Placer"].ToString());
                }
                if (DT.Rows[0]["Placer2"].ToString() == GetVoidID() && !findeEmpty)
                {
                    QuerySetParam(ref TextQuery, "Placer_2", Employer.ID);
                    findeEmpty = true;
                }
                else
                {
                    QuerySetParam(ref TextQuery, "Placer_2", DT.Rows[0]["Placer2"].ToString());
                }
                if (DT.Rows[0]["Placer3"].ToString() == GetVoidID() && !findeEmpty)
                {
                    QuerySetParam(ref TextQuery, "Placer_3", Employer.ID);
                    findeEmpty = true;
                }
                else
                {
                    QuerySetParam(ref TextQuery, "Placer_3", DT.Rows[0]["Placer3"].ToString());
                }
                QuerySetParam(ref TextQuery, "iddoc", WayBill.ID);
                if (!findeEmpty)
                {
                    //все строки заполнены и нас там нет облом
                    FExcStr = "Этот путевой уже укомплектован сотрудникаим!";
                    return false;
                }
                               
            }
            else
            {
                TextQuery =
                    "UPDATE DH$ПутевойЛист " +
                        "SET " +
                            "$ПутевойЛист.Грузчик   = :Loader, " +
                            "$ПутевойЛист.Укладчик  = :Placer, " +
                            "$ПутевойЛист.Дата1     = :Date1, " +
                            "$ПутевойЛист.Время1    = :Time1 " +
                        "WHERE " +
                            "DH$ПутевойЛист .iddoc = :iddoc;";
                //"select @@rowcount as result;";

                QuerySetParam(ref TextQuery, "Loader", Employer.ID);
                QuerySetParam(ref TextQuery, "Placer", (Placer.Selected ? Placer.ID : GetVoidID()));
                QuerySetParam(ref TextQuery, "Date1", DateTime.Now);
                QuerySetParam(ref TextQuery, "Time1", APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "iddoc", WayBill.ID);
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            }
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }

            if (!ToModeLoading(WayBill.ID))
            {
                //временно отключено с релиза 4.47
                //LockoutDoc(WayBill.ID);
                //return false;
            }

            return true;
        }
        public bool CompleteLodading()
        {
            string idSchet = "   " + Translation.DecTo36(GetSynh("Счет"));
            idSchet = idSchet.Substring(idSchet.Length - 4);
            string idClaim = "   " + Translation.DecTo36(GetSynh("ПретензияОтКлиента"));
            idClaim = idClaim.Substring(idClaim.Length - 4);

            //проверим чек погрузки
            string TextQuery =
            "select DocWayBill.$ПутевойЛист.ЧекПогрузка " +
            "from DH$ПутевойЛист as DocWayBill (nolock) " +
            "where " +
                "DocWayBill.iddoc = :iddoc ";
            QuerySetParam(ref TextQuery, "iddoc", WayBill.ID);
            if (Int32.Parse(ExecuteScalar(TextQuery).ToString()) == 0)
            {
                //погрузка не разрешена
                FExcStr = "Погрузка запрещена!";
                return false;
            }  

            TextQuery = 
                "SELECT " + 
                    "Main.DocFull as DocFull " + 
                "FROM (" + GiveSomeOneQueryText() + 
                    ") as Main " +
                    "INNER JOIN (" +
                                "SELECT " +
                                    "Boxes.$Спр.МестаПогрузки.Док as DocID " + 
                                "FROM $Спр.МестаПогрузки as Boxes (nolock) " +
                                "WHERE Boxes.ismark = 0 and Boxes.$Спр.МестаПогрузки.Дата6 = :EmptyDate " + 
                                "GROUP BY Boxes.$Спр.МестаПогрузки.Док " +
                                ") as Boxes " +
                        "ON Boxes.DocID = Main.DocFull " + 
                    "";
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
            QuerySetParam(ref TextQuery, "iddoc", WayBill.ID);
            if (!ExecuteWithRead(TextQuery, out WayBillDT))
            {
                return false;
            }
            if (WayBillDT.Rows.Count > 0)
            {
                FExcStr = "Не все погружено";
                return false;
            }

            TextQuery = 
                "UPDATE DH$ПутевойЛист " + 
                    "SET " +
                        "$ПутевойЛист.Дата2 = :Date, " +
                        "$ПутевойЛист.Время2 = :Time " +
                "WHERE " +
                    "DH$ПутевойЛист .iddoc = :iddoc";

            QuerySetParam(ref TextQuery, "Date", DateTime.Now);
            QuerySetParam(ref TextQuery, "Time", APIManager.NowSecond());
            QuerySetParam(ref TextQuery, "iddoc", WayBill.ID);

            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            //временно отключим блокировку с релиза 4.47
            //LockoutDoc(WayBill.ID);
            //Есл команда ниже вернет false - то наступит не меньший пиздец, чем описанный где-то в похожем комменте...
            BadDoc = new StrictDoc();
            return ToModeChoiseWork(Employer.IDD);
        }

        public bool RepreshLodading()
        {
            return ToModeLoading(WayBill.ID);
        }
        #endregion

        public bool GetCCForIDPlace(string ID, out string IDD)
        {
            IDD = null;
            string TextQuery =
            "Select " +
                "$Спр.МестаПогрузки.КонтрольНабора as DocCC " +
            "from $Спр.МестаПогрузки (nolock) where id = :id";
            QuerySetParam(ref TextQuery, "id", ID);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "Нет действий с данным штрихкодом в этом режиме!";
                return false;
            }
            Dictionary <string, object> tmpDic = new Dictionary<string,object>();
            if (!GetDoc(DT.Rows[0]["DocCC"].ToString(), out tmpDic, true))
            {
                return false;
            }
            IDD = tmpDic["IDD"].ToString();
            return true;
        }
        /// <summary>
        /// формирует строку присвоений для инструкции SET в UPDATE из переданной таблицы
        /// Поддерживает типы - int, DateTime, string
        /// </summary>
        /// <param name="DataMap"></param>
        /// <returns></returns>
        private string ToSetString(Dictionary<string, object> DataMap)
        {
            string result = "";
            foreach (KeyValuePair<string, object> pair in DataMap)
            {
                result += GetSynh(pair.Key) + "=" + ValueToQuery(pair.Value) + ",";
            }
            //удаляем последнюю запятую
            if (result.Length > 0)
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }
        /// <summary>
        /// Проверяет есть ли элемент с таким IDD в указанном справочнике
        /// </summary>
        /// <param name="IDD"></param>
        /// <param name="SCType"></param>
        /// <returns></returns>
        public bool IsSC(string IDD, string SCType)
        {
            string tmpID;
            return GetSC(IDD, SCType, out tmpID);
        }
        /// <summary>
        /// Возвращает первичный ключ секции по ее шмени
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool GetIDSectionByName(string Name, out string ID)
        {
            ID = null;

            if (!ExecuteQuery("SELECT ID FROM " + GetSynh("Спр.Секции") + " (nolock)" +
                            " WHERE DESCR='" + Name + "' AND " + GetSynh("Спр.Секции.ТипСекции") + "=10"))//padright
            {
                return false;
            }
            if (MyReader.Read())
            {
                ID = MyReader[0].ToString();
            }
            MyReader.Close();
            return true;
        }
        /// <summary>
        /// определяет ID справочника по его IDD
        /// </summary>
        /// <param name="IDD"></param>
        /// <param name="SCType"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool GetSC(string IDD, string SCType, out string ID)
        {
            ID = GetVoidID();
            Dictionary<string, object> DataMap = new Dictionary<string,object>();
            List<string> FieldList = new List<string>();
            FieldList.Add("ID");
            if (!GetSCData(IDD, SCType, FieldList, out DataMap))
            {
                return false;
            }
            ID = DataMap["ID"].ToString();
            return true;
        }
        /// <summary>
        ///  отсылает команду в 1С и не ждет ответа
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="DataMapWrite"></param>
        /// <returns></returns>
        public bool ExecCommandNoFeedback(string Command, Dictionary<string, object> DataMapWrite)
        {
            string Query =
                "UPDATE " + GetSynh("Спр.СинхронизацияДанных") +
                " SET DESCR='" + Command + "'," + ToSetString(DataMapWrite) + (DataMapWrite.Count == 0 ? "" : ",") +
                GetSynh("Спр.СинхронизацияДанных.Дата") + " = '" + DateTimeToSQL(DateTime.Now) + "', " +
                GetSynh("Спр.СинхронизацияДанных.Время") + " = " + APIManager.NowSecond().ToString() + ", " +
                GetSynh("Спр.СинхронизацияДанных.ФлагРезультата") + " = 1," +
                GetSynh("Спр.СинхронизацияДанных.ИДТерминала") + " = '" + DeviceID.GetDeviceID() + "'" + 
                    " WHERE ID = (SELECT TOP 1 ID FROM " + GetSynh("Спр.СинхронизацияДанных") +
                                    " WHERE " + GetSynh("Спр.СинхронизацияДанных.ФлагРезультата") + "=0)";
            if (!ExecuteQuery(Query, false))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="DataMapWrite"></param>
        /// <param name="FieldList"></param>
        /// <param name="CommandID"></param>
        /// <returns></r0eturns>
        private bool SendCommand(string Command, Dictionary<string, object> DataMapWrite, List<string> FieldList, out string CommandID)
        {
            CommandID = null;
            string Query =
                "BEGIN TRAN; " +
                "DECLARE @CommandID as varchar(9); " + 
                "SELECT TOP 1 @CommandID = ID FROM " + GetSynh("Спр.СинхронизацияДанных") + " (tablockx) " +
                    "WHERE " + GetSynh("Спр.СинхронизацияДанных.ФлагРезультата") + "=0; " + 
                "UPDATE " + GetSynh("Спр.СинхронизацияДанных") + 
                " SET DESCR='" + Command + "'," + ToSetString(DataMapWrite) + (DataMapWrite.Count == 0 ? "" : ",") +
                GetSynh("Спр.СинхронизацияДанных.Дата") + " = '" + DateTimeToSQL(DateTime.Now) + "', " +
                GetSynh("Спр.СинхронизацияДанных.Время") + " = " + APIManager.NowSecond().ToString() + ", " +
                GetSynh("Спр.СинхронизацияДанных.ФлагРезультата") + " = 1," +
                GetSynh("Спр.СинхронизацияДанных.ИДТерминала") + " = '" + DeviceID.GetDeviceID() + "'" + 
                " WHERE ID=@CommandID; " + 
                " SELECT @@rowcount, @CommandID; " +
                "COMMIT TRAN;";
            if (!ExecuteQuery(Query))
            {
                return false;
            }
            MyReader.Read();
            if ((int)MyReader[0] > 0)
            {
                CommandID = MyReader[1].ToString();
                MyReader.Close();
                return true;
            }
            else
            {
                MyReader.Close();
                FExcStr = "Нет доступных команд! Ошибка робота!";
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="DataMapWrite"></param>
        /// <param name="FieldList"></param>
        /// <param name="DataMapRead"></param>
        /// <returns></returns>
        public bool ExecCommand(string Command, Dictionary<string, object> DataMapWrite, List<string> FieldList, out Dictionary<string, object> DataMapRead)
        {
            return ExecCommand(Command, DataMapWrite, FieldList, out DataMapRead, null);
        }
        /// <summary>
        /// отсылает команду пацанам в 1С
        /// </summary>
        /// <param name="Command"></param>
        /// <param name="DataMapWrite"></param>
        /// <param name="FieldList"></param>
        /// <param name="DataMapRead"></param>
        /// <returns></returns>
        public bool ExecCommand(string Command, Dictionary<string, object> DataMapWrite, List<string> FieldList, out Dictionary<string, object> DataMapRead, string CommandID)
        {
            int Beda = 0;
            FExcStr = null;
            DataMapRead = new Dictionary<string, object>();
            if (CommandID == null)
            {
                if (!SendCommand(Command, DataMapWrite, FieldList, out CommandID))
                {
                    return false;
                }
            }

            //Ждем выполнения или отказа
            string Query = "SELECT " + GetSynh("Спр.СинхронизацияДанных.ФлагРезультата") + (FieldList.Count == 0 ? "" : "," + ToFieldString(FieldList)) +
                        " FROM " + GetSynh("Спр.СинхронизацияДанных") + " (nolock)" +
                            " WHERE ID='" + CommandID + "'";
            bool WaitRobotWork = false;
            int TimeBegin = APIManager.NowSecond();
            while (Math.Abs(TimeBegin - APIManager.NowSecond()) < ResponceTime)
            {
                MyReader.Close();
                if (!ExecuteQuery(Query))
                {
                    return false;
                }
                if (MyReader.Read())
                {
                    if (!((decimal)MyReader[0] == 1))
                    {
                        if ((decimal)MyReader[0] == 2)
                        {
                            if (!WaitRobotWork)
                            {
                                //1C получила команду, сбросим время ожидания
                                TimeBegin = APIManager.NowSecond();
                                WaitRobotWork = true;
                            }
                            continue;
                        }
                        DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] = MyReader[0];
                        for (int i = 1; i < MyReader.FieldCount; i++)
                        {
                            DataMapRead[FieldList[i - 1]] = MyReader[i];
                        }
                        MyReader.Close();
                        return true;
                    }
                }
                else
                {
                    //Тут должно быть что-то типа генерации собственного исключения
                    //MyReader.Close();
                    //FExcStr = "Команда испарилась!";
                    //return false;
                    Beda++;
                    continue;   //Бред какой-то, попробуем еще раз
                }
                if (TimeBegin + 1 < APIManager.NowSecond())
                {
                    //Пауза в 1, после первой секунды беспрерывной долбежки! Да, сука! ДА!
                    int tb = APIManager.NowSecond();
                    while (Math.Abs(tb - APIManager.NowSecond()) < 1)
                    {
                    }
                }
            }
            MyReader.Close();
            FExcStr = "1C не ответила! " + (Beda == 0 ? "" : " Испарений: " + Beda.ToString());
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Command">имя команды</param>
        /// <param name="DataMapWrite">что передать 1С-ке</param>
        /// <param name="ResultMessage">ответ 1С-ки в виде строки</param>
        /// <returns>true - 1C отчиталась что все ништяк инче - хуево, но хуево может быть по разным причинам из-за обрыва связи, например</returns>
        public bool ExecCommandOnlyResultNew(string Command, Dictionary<string, object> DataMapWrite, out string ResultMessage)
        {
            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand(Command, DataMapWrite, FieldList, out DataMapRead))
            {
                ResultMessage = ExcStr;
                return false;
            }
            ResultMessage = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
            if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] == -3)
            {
                return false;
            }
            return true;
        } // ExecCommandOnlyResultNew
        /// <summary>
        /// Проверяет, не находится ли документ в обработке
        /// </summary>
        /// <param name="IDDocCB"></param>
        /// <returns>true - есть необработанный, false - нет</returns>
        public bool CheckRuns(string IDDocCB)
        {
            if (!ExecuteQuery("SELECT ID FROM " + GetSynh("Спр.СинхронизацияДанных") + " (nolock INDEX=VI" + GetSynh("Спр.СинхронизацияДанных.ДокументВход").Substring(2) + ")" + 
                                    " WHERE not descr = 'Internal' and " //Эту, информирующую команду - игнорируем!
                                        + "(" + GetSynh("Спр.СинхронизацияДанных.ФлагРезультата") + "=1 or " 
                                        + GetSynh("Спр.СинхронизацияДанных.ФлагРезультата") + "=2) and "
                                        + GetSynh("Спр.СинхронизацияДанных.ДокументВход") + "='" + ExtendID(IDDocCB, "КонтрольРасходной") + "'"))
            {
                return false;
            }

            bool result = MyReader.Read();
            MyReader.Close();
            return result;
        }
        /// <summary>
        /// синхронизирует дату и время терминала с датой и временем сервера (делается через 1С-ку)
        /// </summary>
        /// <returns></returns>
        public bool SynhDateTime()
        {
            string Vers = FVers;
            FExcStr = null;
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = Vers;
            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаВход2");
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез2");
            if (ExecCommand("GetDateTime", DataMapWrite, FieldList, out DataMapRead))
            {
                if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] == 3)
                {
                    string StrDate = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
                    int Sec        = Convert.ToInt32(DataMapRead["Спр.СинхронизацияДанных.ДатаРез2"].ToString());
                    DeviceName     = DataMapRead["Спр.СинхронизацияДанных.ДатаВход2"].ToString();
                    //добавим в случае необходимости год до 4-х значного, для разбора даты
                    if ((StrDate.Trim()).Length == 8)
                    {
                        StrDate = StrDate.Insert(6, "20");
                    }
                    int Hour = (int)(Sec/3600);
                    Sec -= 3600*(int)(Sec / 3600);    //уменьшаем до минут
                    int Minute = (int)(Sec / 60);
                    Sec -= 60*(int)(Sec / 60);        //уменьшаем до секунд
                    //1С возвращает нам время в локальном формате, нужно переделать его в UTC для записи системного
                    //Определим разницу между системным временем и установленным с учетом часового пояса
                    //Т.е. нам неважно - какой часовой пояс настроен на терминале
                    DateTime Now = DateTime.Now;
                    APIManager.SYSTEMTIME ST = APIManager.Now();
                    int SysSec = APIManager.Second(ST);
                    //Для корректного расчета дельты при смене дня
                    if (ST.wDay > Now.Day || ST.wMonth > Now.Month || ST.wYear > Now.Year)
                    {
                        SysSec += 86400;
                    }
                    else if (ST.wDay < Now.Day || ST.wMonth < Now.Month || ST.wYear < Now.Year)
                    {
                        SysSec -= 86400;
                    }
                    int delta = SysSec - Now.Hour*3600 - Now.Minute*60 - Now.Second;
                    //Переменка для удобного добавления смещения ко времени - это новое значение системной даты/времени
                    DateTime tmpDT = new DateTime(Convert.ToInt32(StrDate.Substring(6, 4)),
                                                  Convert.ToInt32(StrDate.Substring(3, 2)),
                                                  Convert.ToInt32(StrDate.Substring(0, 2)),
                                                  Hour, Minute, Sec);
                    tmpDT = tmpDT.AddSeconds(delta);
                    //Устанавливаем системное время
                    APIManager.SYSTEMTIME systime = new APIManager.SYSTEMTIME();
                    systime.wYear   = (ushort)tmpDT.Year;
                    systime.wMonth  = (ushort)tmpDT.Month;
                    systime.wDay    = (ushort)tmpDT.Day;
                    systime.wHour   = (ushort)tmpDT.Hour;
                    systime.wMinute = (ushort)tmpDT.Minute;
                    systime.wSecond = (ushort)tmpDT.Second;
                    APIManager.SetSystemTime(ref systime);
                    return true;
                }
                else if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] == -3)
                {
                    //в тексте исключения будет ответ 1С - например о том, что версия не подходит
                    FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
                    return false;
                }
            }
            return false;
        }

        public bool UpdateProgram()
        {
            string TextQuery = "select vers from RT_Settings where terminal_id = :id";
            QuerySetParam(ref TextQuery, "id", DeviceID.GetDeviceID());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "Нет настройка версий!";
                return true;    //Да похуй
            }
            string NewVers = DT.Rows[0]["vers"].ToString().Trim();
            if (FVers == NewVers)
            //if (FVers.CompareTo(NewVers) >= 0)
            {
               //Все ок, не нуждается в обновлении
               return true;
            }
            //Нуждается !!!
            OnUpdate(new UpdateEventArgs(NewVers));
            return true;
        }

        #region Garbage
        //БАРАХЛО!!!!!!!
        public bool GetDocType(string IDDoc, out string DocType)
        {
            bool result = false;
            DocType     = "";
            
            if (!ExecuteQuery("SELECT IDDOCDEF FROM _1SJOURN (nolock) WHERE IDDOC='" + IDDoc + "'"))
            {
                return false;
            }
            if (MyReader.Read())
            {
                DocType = To1CName(MyReader[0].ToString());
                result  = true;
            }            
            MyReader.Close();
            return result;
        }
        public bool GetDoc(string IDD, out string IDDoc, out string DocType)
        {
            bool result = false;
            IDDoc       = GetVoidID();
            DocType     = "";

            if (!ExecuteQuery("SELECT IDDOC, IDDOCDEF FROM _1SJOURN (nolock) WHERE " + GetSynh("IDD") + "='" + IDD + "'"))
            {
                return false;
            }
            if (MyReader.Read())
            {
                IDDoc   = MyReader[0].ToString();
                DocType = To1CName(MyReader[1].ToString());
                result  = true;
            }
            MyReader.Close();
            return result;
        }
        private bool GetDoc(string IDDorID, out string IDDoc, out string DocType, out Dictionary<string, object> DataMap, bool ThisID)
        {
            bool result = false;
            IDDoc       = GetVoidID();
            DocType     = "";
            DataMap     = new Dictionary<string,object>();
            if (ThisID)
            {
                //Если ID - расширенный, то переведем его в обычный, 9-и символьный
                if (IDDorID.Length > 9)
                {
                    IDDorID = IDDorID.Substring(4);
                }
            }
            if (!ExecuteQuery("SELECT IDDOC, IDDOCDEF, DATE_TIME_IDDOC, DOCNO, ISMARK, " + GetSynh("IDD").ToString() +
                " FROM _1SJOURN (nolock) WHERE ISMARK = 0 and " + (ThisID ? "IDDOC" : GetSynh("IDD")) + "='" + IDDorID + "'"))
            {
                return false;
            }
            if (MyReader.Read())
            {
                IDDoc               = MyReader[0].ToString();
                DocType             = To1CName(MyReader[1].ToString());
                DataMap["ДатаДок"]  = SQLToDateTime(MyReader[2].ToString());
                DataMap["НомерДок"] = MyReader[3];
                DataMap["ПометкаУдаления"] = MyReader[4];
                DataMap["IDD"]      = MyReader[5];
                result  = true;
            }
            MyReader.Close();
            return result;
        }
        private bool GetDocByID(string IDDorID, out string IDDoc, out string DocType, out Dictionary<string, object> DataMap, bool ThisID)
        {
            bool result = false;
            IDDoc = GetVoidID();
            DocType = "";
            DataMap = new Dictionary<string, object>();
            if (ThisID)
            {
                //Если ID - расширенный, то переведем его в обычный, 9-и символьный
                if (IDDorID.Length > 9)
                {
                    IDDorID = IDDorID.Substring(4);
                }
            }
            if (!ExecuteQuery("SELECT IDDOC, IDDOCDEF, DATE_TIME_IDDOC, DOCNO, ISMARK, " + GetSynh("IDD").ToString() +
                " FROM _1SJOURN (nolock) WHERE ISMARK = 0 and " + (ThisID ? "IDDOC" : GetSynh("IDD")) + "='" + IDDorID + "'"))
            {
                return false;
            }
            if (MyReader.Read())
            {
                IDDoc = MyReader[0].ToString();
                DocType = To1CName(MyReader[1].ToString());
                DataMap["ДатаДок"] = SQLToDateTime(MyReader[2].ToString());
                DataMap["НомерДок"] = MyReader[3];
                DataMap["ПометкаУдаления"] = MyReader[4];
                DataMap["IDD"] = MyReader[5];
                result = true;
            }
            MyReader.Close();
            return result;
        }
        /// <summary>
        /// Возвращает основные реквизиты документа по его ID-шнику
        /// </summary>
        /// <param name="IDDoc"></param>
        /// <param name="DataMap"></param>
        /// <returns></returns>
        public bool GetDoc(string IDDoc, out Dictionary<string, object> DataMap)
        {
            string tmpID, tmpDocType;
            return GetDoc(IDDoc, out tmpID, out tmpDocType, out DataMap, true);
        }
        public bool GetDoc(string IDD, out string IDDoc, out string DocType, out Dictionary<string, object> DataMap)
        {
            return GetDoc(IDD, out IDDoc, out DocType, out DataMap, false);
        }
        public bool GetDocNew(string IDD, out string IDDoc, out string DocType, out Dictionary<string, object> DataMap)
        {
            return GetDocByID(IDD, out IDDoc, out DocType, out DataMap, true);
        }
        public bool GetSubjectDocs(string IDDoc, string DocType, out List<string> SubjectDocs)
        {
            SubjectDocs = new List<string>();
            //O1 - означает что это документ
            if (!ExecuteQuery("SELECT _1SJOURN.IDDOC FROM _1SJOURN (NOLOCK INDEX=ACDATETIME), _1SCRDOC (NOLOCK INDEX=PARENT)" + 
                              " WHERE _1SJOURN.DATE_TIME_IDDOC=_1SCRDOC.CHILD_DATE_TIME_IDDOC and _1SCRDOC.MDID=0 and _1SCRDOC.PARENTVAL='O1" + ExtendID(IDDoc, DocType) + "' ORDER BY IDDOC"))
            {
                return false;
            }
            while (MyReader.Read())
            {
                SubjectDocs.Add(MyReader[0].ToString());
            }
            MyReader.Close();
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IDDoc"></param>
        /// <param name="CCListID"></param>
        /// <param name="CCList"></param>
        /// <returns></returns>
        public bool GetCCListNew(string IDDoc, out List<string> CCListID, out Dictionary<string, object> CCList)
        {
            //Подготовим необходимые структуры для таблицы КНt
            CCList    = new Dictionary<string, object>();
            CCListID  = new List<string>();
            List<string> FieldList = new List<string>();
            
            //ВНИМАТЕЛЬНО С ПОРЯДКОМ ПОЛУЧЕНИЯ ДАННЫХ
            FieldList.Add("КонтрольНабора.Дата1");
            FieldList.Add("КонтрольНабора.Дата2");
            FieldList.Add("КонтрольНабора.Дата3");
            FieldList.Add("КонтрольНабора.Время1");
            FieldList.Add("КонтрольНабора.Время2");
            FieldList.Add("КонтрольНабора.Время3");
            FieldList.Add("КонтрольНабора.НомерЛиста");
            FieldList.Add("КонтрольНабора.КолМест");
            FieldList.Add("КонтрольНабора.Сектор"); // ЭТА ПОЗИЦИЯ ИСПОЛЬЗУЕТСЯ В ЧИТАЛКЕ НИЖЕ
            FieldList.Add("КонтрольНабора.Наборщик");
            FieldList.Add("КонтрольНабора.Комплектовщик");
            FieldList.Add("КонтрольНабора.КолСтрок");
            //ЗАПРОС
            if (!ExecuteQuery("SELECT IDDOC," + ToFieldString(FieldList) + ", ISNULL(Places.Count,0) FROM DH" + GetSynh("КонтрольНабора") + " (nolock)" +
                                " LEFT JOIN ( SELECT Count(*) as Count, " + GetSynh("Спр.МестаПогрузки.КонтрольНабора") + " as DocCC " + 
                                                "FROM " + GetSynh("Спр.МестаПогрузки") + " (nolock) " +
                                                "WHERE ismark = 0 and not " + GetSynh("Спр.МестаПогрузки.Дата6") + " = " + ValueToQuery(GetVoidDate()) + 
                                                " GROUP BY " + GetSynh("Спр.МестаПогрузки.КонтрольНабора") +
                                    ") as Places ON Places.DocCC = IDDOC " + 
                                " WHERE IDDOC in (SELECT _1SJOURN.IDDOC FROM _1SJOURN (NOLOCK INDEX=ACDATETIME), _1SCRDOC (NOLOCK INDEX=PARENT)" + 
                                " WHERE _1SJOURN.ISMARK = 0 and _1SJOURN.DATE_TIME_IDDOC=_1SCRDOC.CHILD_DATE_TIME_IDDOC and _1SCRDOC.MDID=0 and _1SCRDOC.PARENTVAL='O1" + ExtendID(IDDoc, "КонтрольРасходной") + "') ORDER BY IDDOC" ))
            {
                return false;
            }
            string section = "";
            string tmpID;
            while (MyReader.Read())
            {
                tmpID = MyReader[0].ToString();
                CCListID.Add(tmpID);
                for (int i = 1; i < MyReader.FieldCount - 1; i++)
                {
                    CCList[tmpID + "." + FieldList[i - 1]] = MyReader[i];
                }
                CCList[tmpID + ".КонтрольНабора.МестФакт"] = MyReader[MyReader.FieldCount - 1];
                section += "'" + MyReader[9].ToString() + "',"; //формируем строку для запроса по секциям
            }
            MyReader.Close();
            if (CCListID.Count == 0)
            {
                //Подчиненных нет!
                return true;
                
            }
            section = section.Substring(0, section.Length - 1); //удаляем последнюю запятую
            //A теперь поехали секции!
            if (!ExecuteQuery("SELECT ID,DESCR FROM " + GetSynh("Спр.Секции") + " (nolock) WHERE ID in (" + section + ")"))
            {
                return false;
            }
            while (MyReader.Read())
            {
                CCList[MyReader[0].ToString() + ".@Сектор.Имя"] = MyReader[1].ToString();
            }
            MyReader.Close();
            return true;
        }
        public bool SetDocData(string IDDoc, string DocType, Dictionary<string, object> DataMap)
        {
            return ExecuteQuery("UPDATE DH" + GetSynh(DocType) + " SET " + ToSetString(DataMap) + " WHERE IDDOC='" + IDDoc + "'", false);
        }
        #endregion


    }//class SQLSynhronizer
}
