using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    //Тут собраны методы необходимые для перехода в режимы
    partial class Model
    {
        //Reaction
        private bool RSCAcceptance(string IDD)
        {
            if (IsSC(IDD, "Сотрудники"))
            {
                if (!ToModeChoiseWork(IDD))
                {
                    return false;
                }
                QuitModeAcceptance();
                return true;
            }
            else if (IsSC(IDD, "Принтеры"))
            {
                if (Printer.GetAttribute("IDD").ToString() == IDD)
                {
                    FPrinter = new RefPrinter(this);
                    FExcStr = "Обнулили принтер!";
                    return true;
                }
                if (!Printer.FoundIDD(IDD))
                {
                    return false;
                }
                return true;
            }
            else if (IsSC(IDD, "Секции"))
            {
                if (FPalletID == "")
                {
                    FExcStr = "Не выбрана паллета!";
                    return false;
                }
                string SectionsID;
                GetSC(IDD, "Секции", out SectionsID);

                string StrPallets = "";
                foreach (DataRow dr in FPallets.Rows)
                {
                    StrPallets += ", '" + dr["ID"].ToString() + "'";
                }
                StrPallets = StrPallets.Substring(2);   //Убираем спедери запятые

                string TextQuery =
                            "UPDATE $Спр.ПеремещенияПаллет " +
                            "SET " +
                                "$Спр.ПеремещенияПаллет.Адрес0 = :ID, " +
                                "$Спр.ПеремещенияПаллет.ФлагОперации = 2 " +
                            "WHERE $Спр.ПеремещенияПаллет .id in (:Pallet) ";
                TextQuery = TextQuery.Replace(":Pallet", StrPallets);
                QuerySetParam(ref TextQuery, "ID", SectionsID);
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                QuerySetParam(ref TextQuery, "EmployerID", Employer.ID);
                QuerySetParam(ref TextQuery, "Date", DateTime.Now);
                QuerySetParam(ref TextQuery, "Time", APIManager.NowSecond());
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                //почистим табличку паллет от греха и почистим паллеты

                FPallets.Rows.Clear();
                FPalletID = "";
                FBarcodePallet = "";

                return true;
            }
            else
            {
                FExcStr = "Не верный тип справочника!";
                return false;
            }
        } // SCAcceptance
        private bool RSCAcceptanceCross(string IDD)
        {
            if (IsSC(IDD, "Сотрудники"))
            {
                if (!ToModeChoiseWork(IDD))
                {
                    return false;
                }
                QuitModeAcceptance();
                return true;
            }
            else if (IsSC(IDD, "Принтеры"))
            {
                if (Printer.GetAttribute("IDD").ToString() == IDD)
                {
                    FPrinter = new RefPrinter(this);
                    FExcStr = "Обнулили принтер!";
                    return true;
                }
                if (!Printer.FoundIDD(IDD))
                {
                    return false;
                }
                return true;
            }
            else if (IsSC(IDD, "Секции"))
            {
                if (FPalletID == "")
                {
                    FExcStr = "Не выбрана паллета!";
                    return false;
                }
                string SectionsID;
                GetSC(IDD, "Секции", out SectionsID);

                string TextQuery = "BEGIN TRAN; " +
                "UPDATE $Спр.ПеремещенияПаллет WITH (rowlock)" +
                "SET " +
                    "$Спр.ПеремещенияПаллет.Адрес0 = :ID, " +
                    "$Спр.ПеремещенияПаллет.ФлагОперации = 2 " +
                "WHERE " + 
                    "$Спр.ПеремещенияПаллет .id  = :Pallet; " +
                "UPDATE DH$ЗаказНаКлиента WITH (rowlock) " +
                "SET " + 
                    "$ЗаказНаКлиента.Адрес = :ID " +
                "WHERE " + 
                    "$ЗаказНаКлиента.Паллета = :Pallet; " +
                "COMMIT TRAN; ";
                QuerySetParam(ref TextQuery, "Pallet", FPalletID);
                QuerySetParam(ref TextQuery, "ID", SectionsID);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                //почистим паллеты от греха и почистим паллеты
                FExcStr = "Укажите количество мест!";
                return true;
            }
            else
            {
                FExcStr = "Не верный тип справочника!";
                return false;
            }
        } // SCAcceptance
        private bool RSCTransfer(string IDD)
        {
            if (IsSC(IDD, "Сотрудники"))
            {
                return ToModeChoiseWork(IDD);
            }
            else if (IsSC(IDD, "Принтеры"))
            {
                if (!Printer.FoundIDD(IDD))
                {
                    return false;
                }
                if (FPrinter.PrinterType == 1)
                {
                    FPrinter = new RefPrinter(this);
                    FExcStr = "Недопустим принтер этикеток!";
                    return false;
                }
                return true;
            }
            else if (IsSC(IDD, "Секции"))
            {
                FExcStr = "Не верный тип справочника!";
                return false;
            }
            else
            {
                FExcStr = "Не верный тип справочника!";
                return false;
            }

        }
        private bool RSCSamplePut(string IDD)
        {
            if (IsSC(IDD, "Сотрудники"))
            {
                return ToModeChoiseWork(IDD);
            }
            else if (IsSC(IDD, "Принтеры"))
            {
                if (!Printer.FoundIDD(IDD))
                {
                    return false;
                }
                if (FPrinter.PrinterType == 1)
                {
                    FPrinter = new RefPrinter(this);
                    FExcStr = "Не допустим принтер этикеток!";
                    return false;
                }
                return true;
            }
            else
            {
                FExcStr = "Не верный тип справочника!";
                return false;
            }
        }
        private bool RSCAcceptanceItem(string IDD)
        {
            if (IsSC(IDD, "Секции"))
            {
                if (!AcceptedItem.OnShelf)
                {
                    FExcStr = "Товар добавляется в тележку по кнопке!";
                    return false;
                }
                return EnterAdress(IDD);
            }
            else
            {
                FExcStr = "Не верный тип справочника!";
                return false;
            }

        }
        private bool RSCLoadingInicialization(string IDD)
        {
            if (IsSC(IDD, "Сотрудники"))
            {
                return Placer.FoundIDD(IDD);
            }
            else
            {
                FExcStr = "Не верный тип справочника!";
                return false;
            }
            //return true;
        }
        private bool RSCLoading(string ID)
        {
            //FExcStr - несет смысл
            if (IsSC(ID, "Сотрудники"))
            {
                //Временно отключим блокировку путевого при погрузке версия 4.47
                //LockoutDoc(WayBill.ID);
                return ToModeChoiseWork(ID);
            }

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
                "Select " +
                    "$Спр.МестаПогрузки.Дата6 as Date, " +
                    "right($Спр.МестаПогрузки.Док , 9) as Doc " +
                "from $Спр.МестаПогрузки (nolock) where id = :id";
            QuerySetParam(ref TextQuery, "id", ID);
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());

            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {

                FExcStr = "Нет действий с данным штрихкодом в этом режиме!";
                //блокируем путевой
                /*TextQuery =
                               "UPDATE DH$ПутевойЛист " +
                                   "SET " +
                                       "$ПутевойЛист.ЧекПогрузка = 0 " +
                               "WHERE " +
                                   "DH$ПутевойЛист .iddoc = :id";

                QuerySetParam(ref TextQuery, "id", WayBill.ID);
                ExecuteWithoutRead(TextQuery);
                */
                return false;
            }
            if ((DateTime)DT.Rows[0]["Date"] != GetVoidDate())
            {
                FExcStr = "Ошибка! Место уже погружено!";
                //блокируем путевой
                /*TextQuery =
                               "UPDATE DH$ПутевойЛист " +
                                   "SET " +
                                       "$ПутевойЛист.ЧекПогрузка = 0 " +
                               "WHERE " +
                                   "DH$ПутевойЛист .iddoc = :id";

                QuerySetParam(ref TextQuery, "id", WayBill.ID);
                ExecuteWithoutRead(TextQuery);
                */
                return false;
            }   
        
           //Тут какая-то неведомая ошибка возникает, не знаю в чем дело засунул в попытку
            DataRow[] DR;
            try
            {
                DR = WayBillDT.Select("Doc = '" + DT.Rows[0]["Doc"].ToString() + "'");
            }
            catch
            {
                FExcStr = "Неведомая ошибка! Шайтан!";
                //блокируем путевой
                /*TextQuery =
                               "UPDATE DH$ПутевойЛист " +
                                   "SET " +
                                       "$ПутевойЛист.ЧекПогрузка = 0 " +
                               "WHERE " +
                                   "DH$ПутевойЛист .iddoc = :id";

                QuerySetParam(ref TextQuery, "id", WayBill.ID);
                ExecuteWithoutRead(TextQuery);
                */ 
                return false;
            }
            if (DR.Length == 0)
            {
                FExcStr = "Не числится в данном путевом!";
                //блокируем путевой
                TextQuery =
                               "UPDATE DH$ПутевойЛист " +
                                   "SET " +
                                       "$ПутевойЛист.ЧекПогрузка = 0 " +
                               "WHERE " +
                                   "DH$ПутевойЛист .iddoc = :id";

                QuerySetParam(ref TextQuery, "id", WayBill.ID);
                ExecuteWithoutRead(TextQuery);
                 
                return false;
            }
            
            if (Const.OrderControl)
            {
                string currCounter = DR[0]["AdressCounter"].ToString();
                DataRow[] tmpDR = WayBillDT.Select("AdressCounter > " + currCounter);
                if (tmpDR.Length > 0)
                {
                    tmpDR = WayBillDT.Select("AdressCounter = " + currCounter);
                    if (tmpDR.Length != WayBillDT.Rows.Count)
                    {
                        FExcStr = "Нарушена последовательность погрузки!";
                        //блокируем путевой
                        /*TextQuery =
                                       "UPDATE DH$ПутевойЛист " +
                                           "SET " +
                                               "$ПутевойЛист.ЧекПогрузка = 0 " +
                                       "WHERE " +
                                           "DH$ПутевойЛист .iddoc = :id";

                        QuerySetParam(ref TextQuery, "id", WayBill.ID);
                        ExecuteWithoutRead(TextQuery);
                         */ 
                        return false;
                    }
                }
            }

            TextQuery =
                "UPDATE $Спр.МестаПогрузки " +
                    "SET " +
                        "$Спр.МестаПогрузки.Дата6 = :Date, " +
                        "$Спр.МестаПогрузки.Время6 = :Time " +
                "WHERE " +
                    "$Спр.МестаПогрузки .id = :id";

            QuerySetParam(ref TextQuery, "Date", DateTime.Now);
            QuerySetParam(ref TextQuery, "Time", APIManager.NowSecond());
            QuerySetParam(ref TextQuery, "id", ID);

            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }

            int needCurrentLine = WayBillDT.Rows.IndexOf(DR[0]);
            if (!ToModeLoading(WayBill.ID))
            {
                return false;
            }
            CurrentLine = needCurrentLine;

            if (colorSwitcher)
            {
                FExcStr = "Погрузка МЕСТА зафиксирована";
            }
            else
            {
                FExcStr = "ПОГРУЗКА места ЗАФИКСИРОВАНА";
            }
            colorSwitcher = !colorSwitcher;
            return true;
        }
        private bool RSCSampleInventory(string IDD)
        {
            if (IsSC(IDD, "Сотрудники"))
            {
                return ToModeChoiseWork(IDD);
            }
            else if (IsSC(IDD, "Принтеры"))
            {
                if (!Printer.FoundIDD(IDD))
                {
                    return false;
                }
                return true;
            }
            else
            {
                FExcStr = "Нет действий с данным ШК в этом режиме!";
                return false;
            }
        }
        private bool RSCDown(string IDDorID, bool thisID)
        {
            if (!thisID)
            {
                if (IsSC(IDDorID, "Сотрудники"))
                {
                    return ReactionCancel();
                }
                else if (IsSC(IDDorID, "Принтеры"))
                {
                    if (!Printer.FoundIDD(IDDorID))
                    {
                        return false;
                    }
                    else
                    {
                        FExcStr = "Принтер выбран!";
                        return true;
                    }
                }
                else
                {
                    FExcStr = "Нет действий с данным штрихкодом!";
                    return false;
                }
            }
            else
            {
                string TextQuery =
                "Select " +
                    "$Спр.МестаПогрузки.Дата4 as Date, " +
                    "$Спр.МестаПогрузки.КонтрольНабора as Doc " +
                "from $Спр.МестаПогрузки (nolock) where id = :id";
                QuerySetParam(ref TextQuery, "id", IDDorID);

                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                if (DT.Rows.Count == 0)
                {
                    FExcStr = "Нет действий с данным штрихкодом!";
                    return false;
                }
                if (DT.Rows[0]["Doc"].ToString() != DocDown.ID)
                {
                    FExcStr = "Место от другого сборочного!";
                    return false;
                }
                if ((DateTime)DT.Rows[0]["Date"] != GetVoidDate())
                {
                    FExcStr = "Место уже отобрано!";
                    return false;
                }

                //Лютый пиздец начинается!
                TextQuery =
                    "begin tran; " +
                    "UPDATE $Спр.МестаПогрузки " +
                        "SET " +
                            "$Спр.МестаПогрузки.Дата4 = :Date , " +
                            "$Спр.МестаПогрузки.Время4 = :Time " +
                    "WHERE " +
                        "id = :itemid; " +
                    "if @@rowcount = 0 rollback tran " +
                    "else begin " +
                        "if exists ( select top 1 id from $Спр.МестаПогрузки as Ref " +
                            "where " +
                                "Ref.ismark = 0 " +
                                "and Ref.$Спр.МестаПогрузки.КонтрольНабора = :iddoc " +
                                "and Ref.$Спр.МестаПогрузки.Дата4 = :EmptyDate ) " +
                            "commit tran " +
                        "else begin " +
                            "declare @res int; " +
                            "exec WPM_GetOrderDown :Employer, :NameParent, @res OUTPUT; " +
                            "if @res = 0 rollback tran else commit tran " +
                        "end " +
                    "end ";

                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "Date", DateTime.Now);
                QuerySetParam(ref TextQuery, "Time", APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "itemid", IDDorID);
                QuerySetParam(ref TextQuery, "iddoc", DocDown.ID);
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                QuerySetParam(ref TextQuery, "NameParent", DocDown.Sector.Trim());

                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                bool res = ToModeDown();
                OnChangeMode(new ChangeModeEventArgs(FCurrentMode));
                return res;
            }
        }
        private bool RSCChoiseDown(string IDD)
        {
            if (IsSC(IDD, "Сотрудники"))
            {
                return ToModeChoiseWork(IDD);
            }
            else if (IsSC(IDD, "Принтеры"))
            {
                if (!Printer.FoundIDD(IDD))
                {
                    return false;
                }
                else
                {
                    FExcStr = "Принтер выбран!";
                    return true;
                }
            }
            FExcStr = "Нет действий с данным штрихкодом!";
            return false;
        }
        private bool RSCDownComplete(string IDD)
        {
            if (IsSC(IDD, "Сотрудники"))
            {
                return ReactionCancel();
            }
            else if (IsSC(IDD, "Принтеры"))
            {
                if (!Printer.FoundIDD(IDD))
                {
                    return false;
                }
                else
                {
                    FExcStr = "Принтер выбран!";
                    FlagPrintPallete = false;
                    return true;
                }
            }
            else if (IsSC(IDD, "Секции"))
            {
                string ID;
                if (!GetSC(IDD, "Секции", out ID))
                {
                    return false;
                }
                if ((decimal)DownSituation.Rows[0]["NumberOfOrder"] == 0)
                {
                    FExcStr = "Не присвоен номер задания! Напечатайте этикетку!";
                    return false;
                }
                string TextQuery =
                    "declare @res int; exec WPM_CompletePallete :employer, :adress, @res output; ";
                QuerySetParam(ref TextQuery, "employer", Employer.ID);
                QuerySetParam(ref TextQuery, "adress", ID);
                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                FlagPrintPallete = false;
                return ToModeDown();
            }
            FExcStr = "Не знаю что делать с этим ШК!";
            return false;
        }
        private bool RSCFreeDownComplete(string IDDorID, bool thisID)
        {
            if (!thisID)
            {
                if (IsSC(IDDorID, "Сотрудники"))
                {
                    return ReactionCancel();
                }
                else if (IsSC(IDDorID, "Секции"))
                {
                    if (DocDown.ID == null)
                    {
                        FExcStr = "Отсканируйте место !!!";
                        return false;
                    }
                    string AdressID;
                    if (!GetSC(IDDorID, "Секции", out AdressID))
                    {
                        return false;
                    }
                    //Прописываем адрес
                    string TextQuery = "declare @res int; exec WPM_PutInAdress :employer, :adress, @res output; select @res as result; ";
                    QuerySetParam(ref TextQuery, "employer", Employer.ID);
                    QuerySetParam(ref TextQuery, "adress", AdressID);
                    DataTable DT;
                    if (!ExecuteWithRead(TextQuery, out DT))
                    {
                        return false;
                    }
                    if ((int)DT.Rows[0]["result"] != 1)
                    {
                        ToModeFreeDownComplete();
                        return false;
                    }
                    bool res = ToModeFreeDownComplete();
                    OnChangeMode(new ChangeModeEventArgs(Mode.FreeDownComplete));
                    return res;
                }
                else
                {
                    FExcStr = "Нет действий с данным штрихкодом!";
                    return false;
                }
            }
            else
            {
                string TextQuery = "declare @res int; exec WPM_TakeBoxFDC :employer, :box, @res output; select @res as result;";
                QuerySetParam(ref TextQuery, "employer", Employer.ID);
                QuerySetParam(ref TextQuery, "box", IDDorID);

                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }

                LoadBadDoc(IDDorID);//Подсосем данные по документу для просмотра состояния

                if ((int)DT.Rows[0]["result"] != 1)
                {
                    ToModeFreeDownComplete();
                    return false;
                }

                bool res = ToModeFreeDownComplete();
                OnChangeMode(new ChangeModeEventArgs(Mode.FreeDownComplete));
                return res;
            }
        } // RSCFreeDownComplete
        private bool RSCNewComplectation(string IDDorID, bool thisID)
        {
            if (!thisID)
            {
                if (IsSC(IDDorID, "Сотрудники"))
                {
                    return ReactionCancel();
                }
                else
                {
                    FExcStr = "Нет действий с данным штрихкодом!";
                    return false;
                }
            }
            else
            {
                string TextQuery =
                "Select " +
                    "$Спр.МестаПогрузки.Дата8 as Date, " +
                    "$Спр.МестаПогрузки.КонтрольНабора as Doc, " +
                    "$Спр.МестаПогрузки.Сотрудник8 as Employer " +
                "from $Спр.МестаПогрузки (nolock) where id = :id";
                QuerySetParam(ref TextQuery, "id", IDDorID);

                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                if (DT.Rows.Count == 0)
                {
                    FExcStr = "Нет действий с данным штрихкодом!";
                    return false;
                }

                LoadBadDoc(IDDorID);//Подсосем данные по документу для просмотра состояния

                if (DT.Rows[0]["Doc"].ToString() != DocDown.ID)
                {
                    FExcStr = "Место от другого сборочного!";
                    return false;
                }
                if ((DateTime)DT.Rows[0]["Date"] != GetVoidDate())
                {
                    FExcStr = "Место уже отобрано!";
                    return false;
                }
                if (DT.Rows[0]["Employer"].ToString() != Employer.ID)
                {
                    FExcStr = "Этого места нет в задании!";
                    return false;
                }

                //Лютый пиздец начинается!
                TextQuery =
                    "begin tran; " +
                    "UPDATE $Спр.МестаПогрузки " +
                        "SET " +
                            "$Спр.МестаПогрузки.Дата8 = :Date , " +
                            "$Спр.МестаПогрузки.Время8 = :Time " +
                    //"$Спр.МестаПогрузки.Адрес9 = dbo.WMP_fn_GetAdressComplete(id) " + 
                    "WHERE " +
                        "id = :itemid; " +
                    "if @@rowcount = 0 rollback tran " +
                    "else begin " +
                        "if exists ( " +
                            "select top 1 Ref.id from $Спр.МестаПогрузки as Ref " +
                                "inner join $Спр.Секции as SectionsCollect (nolock) " +
                                    "ON SectionsCollect.id = Ref.$Спр.МестаПогрузки.Адрес7 " +
                                "inner join $Спр.Секции as RefSectionsParent (nolock) " +
                                    "on left(SectionsCollect.descr, 2) = RefSectionsParent.descr " +
                            "where " +
                                "Ref.ismark = 0 " +
                                "and RefSectionsParent.descr = :NameParent " +
                                "and Ref.$Спр.МестаПогрузки.КонтрольНабора = :iddoc " +
                                "and Ref.$Спр.МестаПогрузки.Дата8 = :EmptyDate ) " +
                            "commit tran " +
                        "else begin " +
                            "declare @res int; " +
                            "exec WPM_GetOrderComplectationNew :Employer, :NameParent, 0, @res OUTPUT; " +
                            "if @res = 0 rollback tran else commit tran " +
                        "end " +
                    "end ";

                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "Date", DateTime.Now);
                QuerySetParam(ref TextQuery, "Time", APIManager.NowSecond());
                QuerySetParam(ref TextQuery, "itemid", IDDorID);
                QuerySetParam(ref TextQuery, "iddoc", DocDown.ID);
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
                QuerySetParam(ref TextQuery, "NameParent", DocDown.Sector.Trim());

                if (!ExecuteWithoutRead(TextQuery))
                {
                    return false;
                }
                bool res = ToModeNewComplectation();
                OnChangeMode(new ChangeModeEventArgs(FCurrentMode));
                return res;
            }
        }//RSCNewComplectation
        private bool RSCNewComplectationComplete(string IDDorID, bool thisID)
        {
            LastGoodAdress = null;
            if (!thisID)
            {
                if (IsSC(IDDorID, "Сотрудники"))
                {
                    return ReactionCancel();
                }
                else if (IsSC(IDDorID, "Секции"))
                {
                    if (ScaningBox == null)
                    {
                        FExcStr = "Отсканируйте место!";
                        return false;
                    }
                    string ID;
                    string TextQuery;
                    DataTable DT;                        
                    Dictionary<string, object> DataMap;
                    if (!GetSCData(IDDorID, "Секции", "ID,DESCR", out DataMap, false))
                    //if (!GetSC(IDDorID, "Секции", out ID))
                    {
                        return false;
                    }
                    ID = DataMap["ID"].ToString();
                    if (NeedAdressComplete != GetVoidID())
                    {
                        if (ID != NeedAdressComplete)
                        {
                            FExcStr = "Неверный адрес!";
                            return false;
                        }
                    }
                    else
                    {
                        //нужно проверить зону ворот, к тем ли воротам он относится
                        TextQuery =
                        "Select " +
                            "Zone.$Спр.ЗоныВорот.Секция as Section, " +
                            "Gate.descr as Gate " +
                        "from $Спр.МестаПогрузки as Ref (nolock) " +
                        "inner join DH$КонтрольНабора as DocCC (nolock) " +
                            "on DocCC.iddoc = Ref.$Спр.МестаПогрузки.КонтрольНабора " +
                        "inner join DH$КонтрольРасходной as DocCB (nolock) " +
                            "on DocCB.iddoc = DocCC.$КонтрольНабора.ДокументОснование " +
                        "inner join $Спр.Ворота as Gate (nolock) " +
                            "on Gate.id = DocCB.$КонтрольРасходной.Ворота " +
                        "inner join $Спр.ЗоныВорот as Zone (nolock) " +
                            "on Gate.id = Zone.parentext " +
                        "where Ref.id = :id";
                        QuerySetParam(ref TextQuery, "id", ScaningBox);
                        if (!ExecuteWithRead(TextQuery, out DT))
                        {
                            return false;
                        }
                        if (DT.Rows.Count > 0)
                        {
                            //зона задана, надо проверять адреса
                            bool findAdres = false;
                            try
                            {
                                //попробуем по быстрому
                                DataRow[] DR;
                                DR = DT.Select("Section = '" + ID + "'");
                                findAdres = DR.Length > 0;
                            }
                            catch
                            {
                                //ен получилось будем по долгому
                                foreach (DataRow DR in DT.Rows)
                                {
                                    if (ID == DR["Section"].ToString())
                                    {
                                        findAdres = true;
                                        break;
                                    }
                                }

                            }
                            if (!findAdres)
                            {
                                //нет такого адреса в зоне
                                FExcStr = "Нужен адрес из зоны " + DT.Rows[0]["Gate"].ToString().Trim();
                                return false;
                            }
                        }
                    }
                    TextQuery =
                        "declare @res int; " +
                        "begin tran; " +
                        "exec WPM_CompleteBox :box, :adress, @res output; " +
                        "if @res = 0 rollback tran else commit tran; " +
                        "select @res as result; ";
                    QuerySetParam(ref TextQuery, "box", ScaningBox);
                    QuerySetParam(ref TextQuery, "adress", ID);
                    if (!ExecuteWithRead(TextQuery, out DT))
                    {
                        return false;
                    }
                    if ((int)DT.Rows[0]["result"] == 0)
                    {
                        FExcStr = "Не удалось зафиксировать комплектацию места!";
                        return false;
                    }
                    LastGoodAdress = ID;
                    NameLastGoodAdress = DataMap["DESCR"].ToString();

                    CheckFullNewComplete(ScaningBox);
                    return ToModeNewComplectationComplete();
                }
                FExcStr = "Нет действий с этим вашим ШК!";
                return false;
            }
            else
            {
                string TextQuery =
                "Select " +
                    "isnull(Sections.descr, 'Пу') as Sector, " +
                    "DocCC.$КонтрольНабора.НомерЛиста as Number, " +
                    "CONVERT(char(8), CAST(LEFT(journForBill.date_time_iddoc, 8) as datetime), 4) as DateDoc, " +
                    "journForBill.docno as DocNo, " +
                    "Ref.$Спр.МестаПогрузки.Дата9 as Date, " +
                    "Ref.$Спр.МестаПогрузки.КонтрольНабора as Doc, " +
                    "Ref.$Спр.МестаПогрузки.Сотрудник8 as Employer, " +
                    "isnull(dbo.WMP_fn_GetAdressComplete(Ref.id), :EmptyID ) as Adress9, " +
                    "isnull(Adress.Descr, 'нет адреса') as Adress, " +
                    "AllTab.CountAllBox as CountBox, " +
                    "Gate.descr as Gate " +
                "from $Спр.МестаПогрузки as Ref (nolock) " +
                    "left join $Спр.Секции as Adress (nolock) " +
                        "on Adress.id = dbo.WMP_fn_GetAdressComplete(Ref.id) " +
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
                    "where Ref.id = :id";
                QuerySetParam(ref TextQuery, "id", IDDorID);
                QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
                QuerySetParam(ref TextQuery, "Employer", Employer.ID);
                QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());

                LoadBadDoc(IDDorID);//Подсосем данные по документу для просмотра состояния

                DataTable DT;
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }

                if (DT.Rows.Count == 0)
                {
                    FExcStr = "Нет действий с данным штрихкодом!";
                    return false;
                }
                if ((DateTime)DT.Rows[0]["Date"] != GetVoidDate())
                {
                    FExcStr = "Место уже скомплектовано!";
                    return false;
                }
                if (DT.Rows[0]["Employer"].ToString() != Employer.ID)
                {
                    FExcStr = "Этого места нет в задании!";
                    return false;
                }

                DocDown = new StrictDoc();
                DocDown.ID = DT.Rows[0]["Doc"].ToString();
                DocDown.Boxes = (int)DT.Rows[0]["CountBox"];
                DocDown.View = DT.Rows[0]["Sector"].ToString().Trim() + "-" + DT.Rows[0]["Number"].ToString() + " Заявка " + DT.Rows[0]["DocNo"].ToString() + " (" + DT.Rows[0]["DateDoc"].ToString() + ")";
                DocDown.AdressCollect = DT.Rows[0]["Adress"].ToString();
                DocDown.Sector = DT.Rows[0]["Gate"].ToString().Trim();
                DocDown.NumberBill = DT.Rows[0]["DocNo"].ToString().Trim();
                DocDown.NumberCC = (int)(decimal)DT.Rows[0]["Number"];
                DocDown.MainSectorName = DT.Rows[0]["Sector"].ToString();

                ScaningBox = IDDorID;
                ScaningBoxIddoc = DT.Rows[0]["Doc"].ToString();
                NeedAdressComplete = DT.Rows[0]["Adress9"].ToString();
                FExcStr = "Отсканируйте адрес!";
                OnScanBox(new EventArgs());
                return true;
            }
        }

    }
}
