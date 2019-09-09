using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.Data;
using System.IO;
using System.Threading;

namespace WPM
{
    public partial class FMain : Form
    {
        private void ModeAcceptanceReView()
        {
            DataGrid dgNAI = pnlCurrent.GetDataGridByName("dgNotAcceptedItems");
            DataGrid dgAI = pnlCurrent.GetDataGridByName("dgAcceptedItems");
            DataGrid dgC = pnlCurrent.GetDataGridByName("dgConsignment");
            //dgAI.DataSource = SS.AcceptedItems;
            Label lblPrinter = pnlCurrent.GetLabelByName("lblPrinter");
            lblPrinter.Text = SS.Printer.Description;
            //теперь добавим еще и паллету с адресом
            if (SS.FPalletID == "")
            {
                lblPrinter.Text += " НЕТ ПАЛЛЕТЫ";
            }
            else
            {
                lblPrinter.Text += " " + SS.FBarcodePallet.Substring(8, 4);//надо бы адрес паллеты +"(" + SS.ZoneName + ")";
            }
            
            if (dgNAI.CurrentRowIndex != -1)
            {
                pnlCurrent.GetLabelByName("lblItem").Text = ((dgNAI.DataSource as BindingSource).Current as DataRowView).Row["ItemName"].ToString();
            }
            else
            {
                pnlCurrent.GetLabelByName("lblItem").Text = "";
                
            }
            if (dgAI.CurrentRowIndex != - 1)
            {
                pnlCurrent.GetTextBoxByName("tbLabelCount").Text = SS.AcceptedItems.Rows[dgAI.CurrentRowIndex]["LabelCount"].ToString();
                pnlCurrent.GetLabelByName("lblInvCode").Text = SS.AcceptedItems.Rows[dgAI.CurrentRowIndex]["InvCode"].ToString();
            }
            else
            {
                pnlCurrent.GetTextBoxByName("tbLabelCount").Text = ""; 
                pnlCurrent.GetLabelByName("lblInvCode").Text = "";
            }
            if (dgC.CurrentRowIndex != -1)
            {
                pnlCurrent.GetLabelByName("lblClient").Text = SS.Consignment.Rows[dgC.CurrentRowIndex]["Client"].ToString();                   
            }
        }
        private void ModeAcceptanceItemReView()
        {
            ViewUnits.Rows.Clear();
            FillBarcode(SS.Units.Select("OKEI = '" + SS.OKEIUnit + "'"), SS.OKEIUnit);
            FillBarcode(SS.Units.Select("OKEI = '" + SS.OKEIPack + "'"), SS.OKEIPack);
            FillBarcode(SS.Units.Select("OKEI = '" + SS.OKEIPackage + "'"), SS.OKEIPackage);
            FillBarcode(SS.Units.Select("OKEI = '" + SS.OKEIKit + "'"), SS.OKEIKit);
            FillBarcode(
                SS.Units.Select(
                    "not OKEI = '" + SS.OKEIUnit +
                    "' and not OKEI = '" + SS.OKEIPack +
                    "' and not OKEI = '" + SS.OKEIPackage + 
                    "' and not OKEI = '" + SS.OKEIKit + "'")
                , SS.OKEIOthers);
            for (int i = 0; i < ViewUnits.Rows.Count - 1; ++i)
            {
                int Coef        = (int)ViewUnits.Rows[i]["Coef"];
                int Count       = (int)(Coef == 0 ? 0 : SS.AcceptedItem.AcceptCount/Coef);
                int CountBegin  = (int)(Coef == 0 ? 0 : SS.AcceptedItem.Count/Coef);
                Label lblBarcodes = pnlCurrent.GetLabelByName("lblBarcodes" + i.ToString());
                pnlCurrent.GetLabelByName("lblCoef" + i.ToString()).Text        = ViewUnits.Rows[i]["Coef"].ToString();
                pnlCurrent.GetLabelByName("lblCount" + i.ToString()).Text       = Count.ToString();
                pnlCurrent.GetLabelByName("lblCountBegin" + i.ToString()).Text  = CountBegin.ToString();
                lblBarcodes.Text = ViewUnits.Rows[i]["Barcodes"].ToString();
                if (i == CurrentRowItem)
                {
                    lblBarcodes.BackColor = Color.LightBlue;
                }
                else
                {
                    lblBarcodes.BackColor = Color.LightGray;
                }

                //Раскрашивать?
                if (Coef != 0)
                {
                    if (Coef*Count != SS.AcceptedItem.AcceptCount)
                    {
                        pnlCurrent.GetLabelByName("lblCount" + i.ToString()).ForeColor = Color.Red;
                    }
                    else
                    {
                        pnlCurrent.GetLabelByName("lblCount" + i.ToString()).ForeColor = Color.Black;
                    }
                }

            }
            if (!SS.AcceptedItem.Acceptance)
            {
                lblAction.ForeColor = Color.Blue;
            }
            else if (SS.FlagBarcode == 0)
            {
                lblAction.ForeColor = Color.Red;
            }
            else //SS.FlagBarcode == 2 or SS.FlagBarcode == 1
            {
                lblAction.ForeColor = Color.Green;
            }
            if (SS.AcceptedItem.IsRepeat)
            {
                lblAction.ForeColor = Color.Red;
            }
        }
        private void ModeTransferInicializeReView()
        {
            Label lblFrom   = pnlCurrent.GetLabelByName("lblFrom");
            Label lblTo     = pnlCurrent.GetLabelByName("lblTo");
            lblFrom.Text    = SS.WarehousesFrom.Rows[pnlCurrent.GetDataGridByName("dgFrom").CurrentRowIndex]["Name"].ToString();
            lblTo.Text      = SS.WarehousesTo.Rows[pnlCurrent.GetDataGridByName("dgTo").CurrentRowIndex]["Name"].ToString();
        }
        private void ModeTransferReView()
        {
            Label lblPrinter = pnlCurrent.GetLabelByName("lblPrinter");
            lblPrinter.Text = SS.Printer.Description;
            DataGrid dgTI = pnlCurrent.GetDataGridByName("dgTransferItems");
            DataGrid dgTRI = pnlCurrent.GetDataGridByName("dgTransferReadyItems");
            dgTI.DataSource = SS.TransferItems;
            dgTRI.DataSource = SS.TransferReadyItems;
            if (dgTI.CurrentRowIndex != -1)
            {
                pnlCurrent.GetLabelByName("lblItem").Text = SS.TransferItems.Rows[dgTI.CurrentRowIndex]["InvCode"].ToString().Trim() + " "
                                                            + SS.TransferItems.Rows[dgTI.CurrentRowIndex]["ItemName"].ToString();
                pnlCurrent.GetLabelByName("lblAdress").Text = SS.TransferItems.Rows[dgTI.CurrentRowIndex]["AdressName"].ToString();
            }
            else
            {
                pnlCurrent.GetLabelByName("lblItem").Text = "";
                pnlCurrent.GetLabelByName("lblAdress").Text = "";
            }
            if ((dgTI.CurrentRowIndex != SS.OkRowIndex) || SS.OkRowIndex == -1)
            {
                lblAction.ForeColor = Color.Blue;
            }
            else
            {
                lblAction.ForeColor = Color.Blue;
                //lblAction.ForeColor = Color.DarkGreen;
            }
        }
        private void ModeHarmonizationReView()
        {
            DataGrid dgTI = pnlCurrent.GetDataGridByName("dgTransferItems");
            dgTI.DataSource = SS.TransferItems;
            if (dgTI.CurrentRowIndex != -1)
            {
                pnlCurrent.GetLabelByName("lblItem").Text = SS.TransferItems.Rows[dgTI.CurrentRowIndex]["InvCode"].ToString().Trim() + " "
                                                            + SS.TransferItems.Rows[dgTI.CurrentRowIndex]["ItemName"].ToString();
                pnlCurrent.GetLabelByName("lblAdress").Text = SS.TransferItems.Rows[dgTI.CurrentRowIndex]["AdressName"].ToString();
            }
            else
            {
                pnlCurrent.GetLabelByName("lblItem").Text = "";
                pnlCurrent.GetLabelByName("lblAdress").Text = "";
            }

        }
        private void ModeInventoryReView()
        {
            //Новая хуйня
            Label lblPrinter = pnlCurrent.GetLabelByName("lblPrinter");
            lblPrinter.Text = SS.Printer.Description;
            DataGrid dgC = pnlCurrent.GetDataGridByName("dgC");
            dgC.DataSource = SS.ForPrint;
            //Конец новой хуйни

            DataGrid dgI = pnlCurrent.GetDataGridByName("dgInventory");
            if (dgI.CurrentRowIndex != -1)
            {
                pnlCurrent.GetLabelByName("lblItem").Text           = ((dgI.DataSource as BindingSource).Current as DataRowView).Row["ItemName"].ToString();
                pnlCurrent.GetLabelByName("lblHeaderPrice").Text    = ((dgI.DataSource as BindingSource).Current as DataRowView).Row["Price"].ToString();
            }
            else
            {
                pnlCurrent.GetLabelByName("lblItem").Text           = "";
                pnlCurrent.GetLabelByName("lblHeaderPrice").Text    = "";
            }
        }
        private void ModeSamplePutReView()
        {
            DataGrid dgTRI = pnlCurrent.GetDataGridByName("dgTransferReadyItems");
            Label lblPrinter = pnlCurrent.GetLabelByName("lblPrinter");
            lblPrinter.Text = SS.Printer.Description;
            dgTRI.DataSource = SS.TransferReadyItems;
            DataGrid dgSamples = pnlCurrent.GetDataGridByName("dgSamples");
            (dgSamples.DataSource as BindingSource).DataSource = SS.SampleItems;
            dgTRI.DataSource = SS.TransferReadyItems;
            if (dgSamples.CurrentRowIndex != -1)
            {
                pnlCurrent.GetLabelByName("lblItem").Text = ((dgSamples.DataSource as BindingSource).Current as DataRowView).Row["ItemName"].ToString();
            }
            else
            {
                pnlCurrent.GetLabelByName("lblItem").Text = "";
            }

        }
        private void ModeControlCollectReView()
        {
            if (SS.LabelControlCC != null)
            {
                lblState.Text = SS.LabelControlCC;
            }
            DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
            dgGoodsCC.DataSource = SS.GoodsCC;
        }
        private void ModeHarmonizationInicializeReView()
        {
            Label lblFrom   = pnlCurrent.GetLabelByName("lblFrom");
            lblFrom.Text    = SS.WarehousesFrom.Rows[pnlCurrent.GetDataGridByName("dgFrom").CurrentRowIndex]["Name"].ToString();
        }
        private void ModeLoadingInicializationReView()
        {
            Label lblPlacer = pnlCurrent.GetLabelByName("lblPlacer");
            lblPlacer.Text = (SS.Placer.Selected ? Helper.GetShortFIO(SS.Placer.Name) : "<не выбран>");
            Label lblWayBill = pnlCurrent.GetLabelByName("lblWayBill");
            lblWayBill.Text = (SS.WayBill.View == null ? "<не выбран>" : SS.WayBill.View);
            lblAction.ForeColor = Color.Blue;
        }
        private void ModeLoadingReView(bool MoveScrean)
        {
            int ToBeScrean = Screan;
            if (MoveScrean)
            {
                ToBeScrean = Math.Abs(Screan - 1);
            }
            if (ToBeScrean == 0)
            {
                lblState.Text = SS.WayBill.View + " (погрузка)";
            }
            else
            {
                //На всякий случай, вдруг что нибудь накроется ниже и не отобразится заголовок заявки
                lblState.Text = SS.WayBill.View + " (состояние)";
            }

            DataGrid dgWayBill = pnlCurrent.GetDataGridByName("dgWayBill");
            int needIndex = SS.CurrentLine;
            dgWayBill.DataSource       = SS.WayBillDT;
            if (SS.WayBillDT.Rows.Count <= needIndex)
            {
                needIndex = SS.WayBillDT.Rows.Count - 1;
            }
            if (dgWayBill.CurrentRowIndex != -1)
            {
                dgWayBill.CurrentRowIndex = needIndex;
            }

            string currProposal;
            
            if (dgWayBill.CurrentRowIndex != -1)
            {
                currProposal = SS.WayBillDT.Rows[dgWayBill.CurrentRowIndex]["Doc"].ToString();
                if (ToBeScrean == 1)
                {
                    lblState.Text = "Документ " + SS.WayBillDT.Rows[dgWayBill.CurrentRowIndex]["ProposalNumber"].ToString() + " (" + ((DateTime)SS.WayBillDT.Rows[dgWayBill.CurrentRowIndex]["ProposalDate"]).ToString("dd.MM.yy") + ")";
                }
            }
            else
            {
                return;
            }
            if (currProposal != ViewProposal)
            {
                if (ToBeScrean == 1)
                {
                    ShowSectionsNew(currProposal);
                }
            }

        }
        private void ShowSectionsNew(string currProposal)
        {
            ViewProposal = currProposal;
            ExPanel pnlInfoNew = pnlCurrent.GetControlByName("pnlInfoNew") as ExPanel;
            pnlInfoNew.Controls.Clear();
            //Нужно дойти до контроля расходной
            string DocType;
            string IDCB = ""; //ИД контроля расходной
            List<string> SubjectDocs;
            if (!SS.GetSubjectDocs(currProposal, "Счет", out SubjectDocs))
            {
                return;
            }
            //Если пустой, то это счет, а не заявка
            if (SubjectDocs.Count == 0)
            {
                return;
            }
            //Не пустой, ищем контроль расходной или адрес набор
            bool IsFound = false;
            foreach (string CurrentIDDoc in SubjectDocs)
            {
                if (SS.GetDocType(CurrentIDDoc, out DocType))
                {
                    if (DocType == "КонтрольРасходной")
                    {
                        IDCB = CurrentIDDoc;
                        IsFound = true;
                        break;  //ПОЕХАЛИ ДАЛЬШЕ
                    }
                }
                else
                {
                    return;
                }
            }
            if (!IsFound)
            {
                return;
            }
            Dictionary<string, object> DaemondData = new Dictionary<string,object>();
            if (!SS.GetDoc(currProposal, out DaemondData))
            {
                return;
            }
            Dictionary<string, object> DataMap;
            List<string> CCListID;
            //теперь получаем список всех контролей набора
            if (!SS.GetCCListNew(IDCB, out CCListID, out DataMap))
            {
                return;
            }
            //Тяжелая работа по выводу данных
            List<string> FieldList = new List<string>();
            Dictionary<string, object> DataMapRead;
            FieldList.Add("DESCR");
            const int FieldOnRow = 8; //Число полей на одну строку одного сектора

            Label[,] Lbl = new Label[FieldOnRow, CCListID.Count];
            int i = 0;
            int dY;                 //Это смещение, м/у строками
            int FirstIndex = -1;         //Индекс строки первой комплектации
            int LastIndex  = -1;          //Индекс строки последней комплектации
            DateTime MinDate = new DateTime(2020, 01, 01);  //Минимальная дата
            DateTime MaxDate = new DateTime(1753, 01, 01);  //Максимальная дата
            int MinTime      = 86400;                       //Минимальное время
            int MaxTime      = 0;                           //Максимальное время
            bool AllComplete = true;                        //Последнюю комплектацию не имеет смысла считать, если где-то, что-то не скомплектовано
            foreach (string ID in CCListID)
            {
                dY  = i*30;
                //Создадим метки для этого сектора
                for (int j = 0; j < FieldOnRow; j++)
                {
                    Lbl[j, i]           = new Label();
                    Lbl[j, i].Name      = "_dyn" + j.ToString() + i.ToString();
                    Lbl[j, i].BackColor = Color.White;
                    pnlInfoNew.Controls.Add(Lbl[j, i]);
                }

                //ОПРЕДЕЛИМСЯ С СОДЕРЖАНИЕМ И ЦВЕТАМИ ИСХОДЯ ИЗ СОСТОЯНИЯ
                //Ниже используются запросы к БД, не очень красиво, но в целях оптимизации (т.к. заглядывают сюда не всякий раз)
                //Поэтому, если что-то не срастается, то выводим пустые поля, там где несраслось
                if (SQL1S.IsVoidDate((DateTime)DataMap[ID + ".КонтрольНабора.Дата1"]))
                {
                    //ОЖИДАНИЕ НАБОРА
                    Lbl[1, i].Text = ".";
                    Lbl[2, i].Text = "<< ожидание >>";
                    Lbl[3, i].Text = "__:__";
                    Lbl[4, i].Text = "  неопределен";
                    Lbl[5, i].Text = "__:__";
                    //Цвета
                    Lbl[0, i].ForeColor = Color.Red;
                    Lbl[1, i].ForeColor = Color.Red;
                    Lbl[2, i].ForeColor = Color.LightPink;
                    Lbl[3, i].ForeColor = Color.LightGray;
                    Lbl[4, i].ForeColor = Color.LightGray;
                    Lbl[5, i].ForeColor = Color.LightGray;

                    AllComplete = false;
                }
                else if (SQL1S.IsVoidDate((DateTime)DataMap[ID + ".КонтрольНабора.Дата2"]))
                {
                    //В НАБОРЕ
                    Lbl[1, i].Text = "...";
                    if (!SS.GetSCData(DataMap[ID + ".КонтрольНабора.Наборщик"].ToString(), "Сотрудники", FieldList, out DataMapRead, true))
                    {
                        Lbl[2,i].Text = "< не задан >";
                    }
                    else
                    {
                        Lbl[2,i].Text = Helper.GetShortFIO(DataMapRead["DESCR"].ToString());
                    }
                    Lbl[3, i].Text = "(" + ((DateTime)DataMap[ID + ".КонтрольНабора.Дата1"]).ToString("dd.MM") + ") "
                                     + Helper.SecondToString((int)(decimal)DataMap[ID + ".КонтрольНабора.Время1"]) + " - ...";
                    Lbl[4, i].Text = "  неопределен";
                    Lbl[5, i].Text = "__:__";
                    //Цвета
                    Lbl[0, i].ForeColor = Color.Red;
                    Lbl[1, i].ForeColor = Color.Red;
                    Lbl[2, i].ForeColor = Color.Red;
                    Lbl[3, i].ForeColor = Color.Red;
                    Lbl[4, i].ForeColor = Color.LightGray;
                    Lbl[5, i].ForeColor = Color.LightGray;

                    AllComplete = false;
                }
                else if (SQL1S.IsVoidDate((DateTime)DataMap[ID + ".КонтрольНабора.Дата3"]))
                {
                    //НАБРАНА
                    Lbl[1, i].Text = "--";
                    if (!SS.GetSCData(DataMap[ID + ".КонтрольНабора.Наборщик"].ToString(), "Сотрудники", FieldList, out DataMapRead, true))
                    {
                        Lbl[2,i].Text = "< не задан >";
                    }
                    else
                    {
                        Lbl[2,i].Text = Helper.GetShortFIO(DataMapRead["DESCR"].ToString());
                    }
                    Lbl[3, i].Text = "(" + ((DateTime)DataMap[ID + ".КонтрольНабора.Дата2"]).ToString("dd.MM") + ") "
                                     + Helper.SecondToString((int)(decimal)DataMap[ID + ".КонтрольНабора.Время1"]) + " - "
                                     + Helper.SecondToString((int)(decimal)DataMap[ID + ".КонтрольНабора.Время2"]);
                    Lbl[4, i].Text = "<< ожидание >>";
                    Lbl[5, i].Text = "__:__";
                    //Цвета
                    Lbl[0, i].ForeColor = Color.Red;
                    Lbl[1, i].ForeColor = Color.Red;
                    Lbl[2, i].ForeColor = Color.Red;
                    Lbl[3, i].ForeColor = Color.Red;
                    Lbl[4, i].ForeColor = Color.LightPink;
                    Lbl[5, i].ForeColor = Color.LightGray;

                    AllComplete = false;
                }
                else
                {
                    //СКОМПЛЕКТОВАНА
                    DateTime CurrentDate = (DateTime)DataMap[ID + ".КонтрольНабора.Дата3"];
                    int CurrentTime      = (int)(decimal)DataMap[ID + ".КонтрольНабора.Время3"];
                    //Скорректируем максималный момент комплектации
                    if (AllComplete && CurrentDate > MaxDate)
                    {
                        MaxDate = CurrentDate;
                        MaxTime = CurrentTime;
                        LastIndex = i;
                    }
                    else if (AllComplete && CurrentDate == MaxDate && CurrentTime > MaxTime)
                    {
                        MaxTime = CurrentTime;
                        LastIndex = i;
                    }
                    //Скорректируем максималный момент комплектации
                    if (CurrentDate < MinDate)
                    {
                        MinDate = CurrentDate;
                        MinTime = CurrentTime;
                        FirstIndex = i;
                    }
                    else if (CurrentDate == MinDate && CurrentTime < MinTime)
                    {
                        MinTime = CurrentTime;
                        FirstIndex = i;
                    }
                    //собственно рисовалка
                    Lbl[1, i].Text = "+";
                    if (!SS.GetSCData(DataMap[ID + ".КонтрольНабора.Наборщик"].ToString(), "Сотрудники", FieldList, out DataMapRead, true))
                    {
                        Lbl[2,i].Text = "< не задан >";
                    }
                    else
                    {
                        Lbl[2,i].Text = Helper.GetShortFIO(DataMapRead["DESCR"].ToString());
                    }
                    Lbl[3, i].Text = "(" + ((DateTime)DataMap[ID + ".КонтрольНабора.Дата2"]).ToString("dd.MM") + ") "
                                     + Helper.SecondToString((int)(decimal)DataMap[ID + ".КонтрольНабора.Время1"]) + " - "
                                     + Helper.SecondToString((int)(decimal)DataMap[ID + ".КонтрольНабора.Время2"]);
                    if (!SS.GetSCData(DataMap[ID + ".КонтрольНабора.Комплектовщик"].ToString(), "Сотрудники", FieldList, out DataMapRead, true))
                    {
                        Lbl[4,i].Text = "< не задан >";
                    }
                    else
                    {
                        Lbl[4,i].Text = Helper.GetShortFIO(DataMapRead["DESCR"].ToString());
                    }
                    Lbl[5, i].Text = "(" + CurrentDate.ToString("dd.MM") + ") " + Helper.SecondToString(CurrentTime);
                                     
                    //Цвета
                    Lbl[0, i].ForeColor = Color.Black;
                    Lbl[1, i].ForeColor = Color.Black;
                    Lbl[2, i].ForeColor = Color.Black;
                    Lbl[3, i].ForeColor = Color.Black;
                    Lbl[4, i].ForeColor = Color.Black;
                    Lbl[5, i].ForeColor = Color.Black;
                }

                //Заголовок
                Lbl[0,i].Font       = FontTahoma14Bold;
                Lbl[0,i].TextAlign  = ContentAlignment.TopCenter;
                Lbl[0,i].Location   = new Point(3, 3 + dY);
                Lbl[0,i].Size       = new Size(50, 27);
                if (DataMap[ID + ".КонтрольНабора.Сектор"].ToString() == SQL1S.GetVoidID())
                {
                    //Пустая
                    Lbl[0,i].Text = "Пу";
                }
                else
                {
                    Lbl[0,i].Text = DataMap[DataMap[ID + ".КонтрольНабора.Сектор"] + ".@Сектор.Имя"].ToString().Trim();
                }
                Lbl[0,i].Text += "-" + DataMap[ID + ".КонтрольНабора.НомерЛиста"];
                
                //Мест погружено
                Lbl[1,i].Font       = FontTahoma9Bold;
                Lbl[1,i].TextAlign  = ContentAlignment.TopRight;
                Lbl[1,i].Location   = new Point(56, 3 + dY);
                Lbl[1,i].Size       = new Size(25, 13);
                //Мест всего
                Lbl[7,i].Font       = FontTahoma9Bold;
                Lbl[7,i].TextAlign  = ContentAlignment.TopRight;
                Lbl[7,i].Location   = new Point(56, 17 + dY);
                Lbl[7,i].Size       = new Size(25, 13);
                //Наборщик 
                Lbl[2,i].Font       = FontTahoma9Regular;
                Lbl[2,i].TextAlign  = ContentAlignment.TopLeft;
                Lbl[2,i].Location   = new Point(84, 3 + dY);
                Lbl[2,i].Size       = new Size(112, 13);
                //Время наборщика
                Lbl[3,i].Font       = FontTahoma8Regular;
                Lbl[3,i].TextAlign  = ContentAlignment.TopLeft;
                Lbl[3,i].Location   = new Point(199, 3 + dY);
                Lbl[3,i].Size       = new Size(110, 13);
                //Комплектовщик
                Lbl[4,i].Font       = FontTahoma9Regular;
                Lbl[4,i].TextAlign  = ContentAlignment.TopLeft;
                Lbl[4,i].Location   = new Point(84, 17 + dY);
                Lbl[4,i].Size       = new Size(112, 13);
                //Время комплектовщика
                Lbl[5,i].Font       = FontTahoma8Regular;
                Lbl[5,i].TextAlign  = ContentAlignment.TopLeft;
                Lbl[5,i].Location   = new Point(199, 17 + dY);
                Lbl[5,i].Size       = new Size(77, 13);
                //Количество строк
                Lbl[6,i].Font       = FontTahoma8Bold;
                Lbl[6,i].TextAlign  = ContentAlignment.TopRight;
                Lbl[6,i].Location   = new Point(279, 17 + dY);
                Lbl[6,i].Size       = new Size(30, 13);
                Lbl[6,i].Text       = DataMap[ID + ".КонтрольНабора.КолСтрок"].ToString();
                Lbl[6,i].ForeColor  = Color.Navy;

                //Места
                Lbl[7,i].Text       = DataMap[ID + ".КонтрольНабора.КолМест"].ToString();
                Lbl[1,i].Text       = DataMap[ID + ".КонтрольНабора.МестФакт"].ToString();

                if (DataMap[ID + ".КонтрольНабора.КолМест"].ToString() != DataMap[ID + ".КонтрольНабора.МестФакт"].ToString())
                {
                    Lbl[1,i].ForeColor = Color.Red;
                }

                i++;
            }

            //Подкрасим первую и последнюю комплектации
            if (FirstIndex >= 0)
            {
                Lbl[1, FirstIndex].BackColor = Color.PaleGoldenrod;
                Lbl[7, FirstIndex].BackColor = Color.PaleGoldenrod;
            }
            if (AllComplete && LastIndex >= 0)
            {
                Lbl[1, LastIndex].BackColor = Color.PaleGreen;
                Lbl[7, LastIndex].BackColor = Color.PaleGreen;
            }

            pnlInfoNew.BringToFront();

            //Заголовок с информацией по заявке
            if (DaemondData.Count > 0)
            {
                string txt = DaemondData["НомерДок"].ToString() + " (" + ((DateTime)DaemondData["ДатаДок"]).ToString("dd.MM.yy") + ")";
                //Label lblDescr  = pnlCurrent.GetLabelByName("lblDescr");
                //lblDescr.Text = txt;
            }
        }
        private void ModeSampleSetReView()
        {
            lblState.Text = SS.DocSet.View; //Обновляем главную надпись
            DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
            dgGoodsCC.DataSource = SS.ATTable;


            pnlCurrent.GetLabelByName("lblPrevious").Text = SS.PreviousAction;
            Label lblInvCode = pnlCurrent.GetLabelByName("lblInvCode");
            Label lblAdress = pnlCurrent.GetLabelByName("lblAdress");
            Label lblCount = pnlCurrent.GetLabelByName("lblCount");
            TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
            Label lblPrevious = pnlCurrent.GetLabelByName("lblPrevious");
            Label lblHeader = pnlCurrent.GetLabelByName("lblHeader");
            Label lblItem = pnlCurrent.GetLabelByName("lblItem");
            Label lblBox = pnlCurrent.GetLabelByName("lblBox");
            Label lblHeaderPrice = pnlCurrent.GetLabelByName("lblHeaderPrice");
            Label lblHeaderBalance = pnlCurrent.GetLabelByName("lblHeaderBalance");
            Label lblHeaderSum = pnlCurrent.GetLabelByName("lblHeaderSum");
            Label lblDetailsCount = pnlCurrent.GetLabelByName("lblDetailsCount");
            PictureBox pbPhoto = pnlCurrent.GetPictureBoxByName("pbPhoto");

            lblInvCode.ForeColor = Color.Black;
            lblAdress.ForeColor = Color.Black;
            lblCount.ForeColor = Color.Black;
            lblBox.ForeColor = Color.Black;

            lblPrevious.Text = SS.PreviousAction;
            lblHeader.Text = "Строка " + SS.CCItem.CurrLine.ToString() + " из " + SS.DocSet.Rows.ToString() + " (ост " + SS.AllSetsRow.ToString() + ")";
            //lblHeaderPrice.Text = "Цена: " + SS.CCItem.Price.ToString().Trim();
            lblHeaderBalance.Text = "Ост-ок " + SS.CCItem.Balance.ToString().Trim();
            //lblHeaderSum.Text = "Сумма: " + SS.DocSet.Sum.ToString();
            lblInvCode.Text = SS.CCItem.InvCode.Trim();
            lblAdress.Text = SS.CCItem.AdressName;
            lblItem.Text = SS.CCItem.Name;
            lblBox.Text = SS.DocSet.Box;
            //lblCount.Text       = SS.CCItem.OKEI2Count.ToString() + " " + SS.CCItem.OKEI2.Trim() + " по " + SS.CCItem.OKEI2Coef.ToString();
            lblCount.Text = SS.CCItem.Count.ToString() + " шт по 1";
            lblDetailsCount.Text = "Деталей: " + SS.CCItem.Details.ToString();
            lblAction.Text = SS.ExcStr;
            switch (SS.CurrentAction)
            {
                case ActionSet.ScanAdress:
                    tbCount.Visible = false;
                    lblAdress.ForeColor = Color.Blue;
                    break;

                case ActionSet.ScanItem:
                    pbPhoto.Visible = false;
                    lblInvCode.ForeColor = Color.Blue;
                    break;

                case ActionSet.EnterCount:
                    lblCount.ForeColor = Color.Blue;
                    tbCount.Visible = true;
                    tbCount.BringToFront();
                    tbCount.Focus();
                    break;
            }
        }
        private void ModeSetReView()
        {
            lblState.Text = SS.DocSet.View; //Обновляем главную надпись
            DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
            dgGoodsCC.DataSource       = SS.CCRP;

            pnlCurrent.GetLabelByName("lblPrevious").Text = SS.PreviousAction;
            Label lblInvCode    = pnlCurrent.GetLabelByName("lblInvCode");
            Label lblAdress     = pnlCurrent.GetLabelByName("lblAdress");
            Label lblCount      = pnlCurrent.GetLabelByName("lblCount");
            TextBox tbCount     = pnlCurrent.GetTextBoxByName("tbCount");
            Label lblPrevious   = pnlCurrent.GetLabelByName("lblPrevious");
            Label lblHeader     = pnlCurrent.GetLabelByName("lblHeader");
            Label lblItem       = pnlCurrent.GetLabelByName("lblItem");
            Label lblBox        = pnlCurrent.GetLabelByName("lblBox");
            Label lblHeaderPrice = pnlCurrent.GetLabelByName("lblHeaderPrice");
            Label lblHeaderBalance = pnlCurrent.GetLabelByName("lblHeaderBalance");
            Label lblHeaderSum  = pnlCurrent.GetLabelByName("lblHeaderSum");
            Label lblDetailsCount  = pnlCurrent.GetLabelByName("lblDetailsCount");
            PictureBox pbPhoto = pnlCurrent.GetPictureBoxByName("pbPhoto");

            lblInvCode.ForeColor    = Color.Black;
            lblAdress.ForeColor     = Color.Black;
            lblCount.ForeColor      = Color.Black;
            lblBox.ForeColor        = Color.Black;
            
            lblPrevious.Text    = SS.PreviousAction;
            lblHeader.Text      = "Строка " + SS.CCItem.CurrLine.ToString() + " из " + SS.DocSet.Rows.ToString() + " (ост " + SS.AllSetsRow.ToString() + ")";
            lblHeaderPrice.Text = "Цена: " + SS.CCItem.Price.ToString().Trim();
            lblHeaderBalance.Text = "Ост-ок " + SS.CCItem.Balance.ToString().Trim();
            lblHeaderSum.Text   = "Сумма: " + SS.DocSet.Sum.ToString();
            lblInvCode.Text     = SS.CCItem.InvCode.Trim();
            lblAdress.Text      = SS.CCItem.AdressName;
            lblItem.Text        = SS.CCItem.Name;
            lblBox.Text         = SS.DocSet.Box;
            //lblCount.Text       = SS.CCItem.OKEI2Count.ToString() + " " + SS.CCItem.OKEI2.Trim() + " по " + SS.CCItem.OKEI2Coef.ToString();
            lblCount.Text       = SS.CCItem.Count.ToString() + " шт по 1";
            lblDetailsCount.Text      = "Деталей: " + SS.CCItem.Details.ToString();
            switch (SS.CurrentAction)
            {
                case ActionSet.ScanAdress:
                    tbCount.Visible = false;
                    lblAdress.ForeColor = Color.Blue;
                    break;

                case ActionSet.ScanItem:
                    pbPhoto.Visible = false;
                    lblInvCode.ForeColor = Color.Blue;
                    break;

                case ActionSet.EnterCount:
                    lblCount.ForeColor = Color.Blue;
                    tbCount.Visible = true;
                    tbCount.BringToFront();
                    tbCount.Focus();
                    break;

                case ActionSet.ScanBox:
                    tbCount.Visible = false;
                    lblBox.ForeColor = Color.Blue;
                    break;

                case ActionSet.ScanPart:
                    if (!pbPhoto.Visible)
                    {
                        pbPhoto.Visible = true;
                        pbPhoto.BringToFront();
                        LoadImage(pbPhoto, SS.CCItem.InvCode);
                    }
                    break;
            }

            lblAction.Text = SS.ExcStr;
        }//ModeSetReView
        private void ModeSetCorrectReView()
        {
            if (ChoiseCorrect == 0)
            {
                //пока не сделан выбор - все тлен
                return;
            }
            if (SS.CurrentMode == Mode.SetCorrect)
            {
                for (int j = 0; j < 5; j++)
                {
                    Label lblTmp = pnlCurrent.GetLabelByName("lblAnswer" + j.ToString());
                    if (j == ChoiseCorrect)
                    {
                        lblTmp.ForeColor = Color.Green;
                    }
                    else
                    {
                        lblTmp.Visible = false;
                    }
                }
            }
            else if (SS.CurrentMode == Mode.SampleSetCorrect)   // поскольку в режиме набора образцов отсутсвует "отказ"
            {
                for (int j = 0; j < 3; j++)
                {
                    Label lblTmp = pnlCurrent.GetLabelByName("lblAnswer" + j.ToString());
                    if (j == ChoiseCorrect)
                    {
                        lblTmp.ForeColor = Color.Green;
                    }
                    else
                    {
                        lblTmp.Visible = false;
                    }
                }
            }
            pnlCurrent.GetLabelByName("lblKey1").Visible = true;
            TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
            tbCount.Visible = true;
            tbCount.Focus();
        } // ModeSetCorrectReView
        private void ModeSetCompleteReView()
        {
            lblState.Text = "Завершение набора";
            Label lblResult         = pnlCurrent.GetLabelByName("lblResult");
            Label lblPrinter        = pnlCurrent.GetLabelByName("lblPrinter");
            Label lblHeader         = pnlCurrent.GetLabelByName("lblHeader");
            Label lblClient         = pnlCurrent.GetLabelByName("lblClient");
            Label lblSelfRemovel    = pnlCurrent.GetLabelByName("lblSelfRemovel");
            TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
            if (SS.DocSet.ID != null)
            {
                if (SS.BoxForSet == null)
                {
                    lblSelfRemovel.Text = SS.DocSet.SelfRemovel == 1 ? "САМОВЫВОЗ" : "ДОСТАВКА";
                }
                else
                {
                    lblSelfRemovel.Text = SS.DocSet.SelfRemovel == 1 ? "САМОВЫВОЗ" : "ДОСТАВКА" + " (" + SS.BoxForSet.Name.Trim() + ")";
                }
                lblHeader.Text = "Отобрано " + SS.DocSet.Rows.ToString() + " строк";
                lblClient.Text = SS.DocSet.Client;
                lblState.Text = SS.DocSet.View;
                if (!SS.BoxOk)
                {
                    tbCount.Visible = true;
                    tbCount.Focus();
                }
            }
            lblPrinter.Text = SS.Printer.Description;
            if (SS.BoxOk)
            {
                pnlCurrent.GetLabelByName("lblResult").Text = "Всего " + SS.DocSet.Boxes.ToString() + " мест! Отсканируйте ШК предкомплектации!";
                tbCount.Visible = false;
            }
        } // ModeSetCompleteReView
        private void ModeDownReView()
        {
            int remain = SS.DocDown.AllBoxes - SS.DocDown.Boxes;
            Label lblPrinter    = pnlCurrent.GetLabelByName("lblPrinter");
            Label lblInfo1      = pnlCurrent.GetLabelByName("lblInfo1");
            Label lblKey1       = pnlCurrent.GetLabelByName("lblKey1");
            Label lblNumber       = pnlCurrent.GetLabelByName("lblNumber");
            Label lblAdress       = pnlCurrent.GetLabelByName("lblAdress");
            Label lblSetter       = pnlCurrent.GetLabelByName("lblSetter");
            lblPrinter.Text = SS.Printer.Description;
            lblInfo1.Text   = "Отобрано " + remain.ToString() + " из " + SS.DocDown.AllBoxes.ToString();
            lblNumber.Text      = SS.DocDown.NumberBill.Substring(SS.DocDown.NumberBill.Length - 5, 2) + " "
                                    + SS.DocDown.NumberBill.Substring(SS.DocDown.NumberBill.Length - 3)
                                    + " сектор: " + SS.DocDown.MainSectorName.Trim() + "-" + SS.DocDown.NumberCC.ToString();
            lblAdress.Text      = SS.DocDown.AdressCollect;
            lblSetter.Text      = "отборщик: " + Helper.GetShortFIO(SS.DocDown.SetterName);
            lblKey1.Visible = SS.DocDown.MaxStub <= remain ? true : false;
            lblAction.Text = SS.ExcStr;
        }
        private void ModeDownCompleteReView()
        {
            Label lblPrinter    = pnlCurrent.GetLabelByName("lblPrinter");
            Label lblNumber     = pnlCurrent.GetLabelByName("lblNumber");
            Label lblKey1       = pnlCurrent.GetLabelByName("lblKey1");
            lblKey1.Visible     = SS.FlagPrintPallete ? false : true;
            lblPrinter.Text = SS.Printer.Description;
            if ((decimal)SS.DownSituation.Rows[0]["NumberOfOrder"] != 0)
            {
                string Number = SS.DownSituation.Rows[0]["NumberOfOrder"].ToString();
                lblNumber.Text = Number.Substring((Number.Length - 4) > 0 ? Number.Length - 4 : 0);
            }
            lblAction.Text = SS.ExcStr;
        }
        private void ModeNewComplectationReView()
        {
            int remain = SS.DocDown.AllBoxes - SS.DocDown.Boxes;
            Label lblInfo1      = pnlCurrent.GetLabelByName("lblInfo1");
            Label lblKey1       = pnlCurrent.GetLabelByName("lblKey1");
            Label lblNumber       = pnlCurrent.GetLabelByName("lblNumber");
            Label lblAdress       = pnlCurrent.GetLabelByName("lblAdress");
            Label lblSetter       = pnlCurrent.GetLabelByName("lblSetter");
            lblInfo1.Text   = "Отобрано " + remain.ToString() + " из " + SS.DocDown.AllBoxes.ToString();
            lblNumber.Text      = SS.DocDown.NumberBill.Substring(SS.DocDown.NumberBill.Length - 5, 2) + " "
                                    + SS.DocDown.NumberBill.Substring(SS.DocDown.NumberBill.Length - 3)
                                    + " сектор: " + SS.DocDown.MainSectorName.Trim() + "-" + SS.DocDown.NumberCC.ToString();
            lblAdress.Text      = SS.DocDown.AdressCollect;
            lblSetter.Text      = "отборщик: " + Helper.GetShortFIO(SS.DocDown.SetterName);
            lblKey1.Visible = SS.DocDown.MaxStub <= remain ? true : false;
            if (Screan == 1)
            {
                ShowInfoNewComp();
            }
            lblAction.Text = SS.ExcStr;
        }
        private void ModeNewComplectationCompleteReView()
        {
            DataGrid dgRoute    = pnlCurrent.GetDataGridByName("dgRoute");
            Label lblKey1       = pnlCurrent.GetLabelByName("lblKey1");
            Label lblKey2       = pnlCurrent.GetLabelByName("lblKey2");
            dgRoute.DataSource       = SS.CCRP;
            lblKey1.Visible = SS.LastGoodAdress == null ? false : true;
            //очередная ебливая заглушка
            if (TimerFind.Enabled)
            {
                return;
            }
            Label lblNumber     = pnlCurrent.GetLabelByName("lblNumber");
            //Label lblInfo2      = pnlCurrent.GetLabelByName("lblInfo2");
            Label lblSetter     = pnlCurrent.GetLabelByName("lblSetter");
            lblKey2.Visible = false;
            if ((decimal)SS.DownSituation.Rows[0]["NumberOfOrder"] != 0)
            {
                string Number = SS.DownSituation.Rows[0]["NumberOfOrder"].ToString();
                lblNumber.Text = Number.Substring((Number.Length - 4) > 0 ? Number.Length - 4 : 0);
            }
            if (SS.LastGoodAdress != null)
            {
                lblSetter.ForeColor = Color.DarkGray;
                lblSetter.Text = "'все' --> " + SS.NameLastGoodAdress;
            }
            else
            {
                lblSetter.ForeColor = Color.Black;
            }

            if (Screan == 1)
            {
                ShowInfoNewComp();
            }

            lblAction.Text = SS.ExcStr;
        }

        private void ModeFreeDownCompleteReView()
        {
            if (Screan == 1)
            {
                ShowInfoNewComp();
            }
            lblAction.Text = SS.ExcStr;
        }
        private void ModeSetSelfControlReView()
        {
            DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
            dgGoodsCC.DataSource = SS.TableSet;

            pnlCurrent.GetLabelByName("lblPrevious").Text = SS.PreviousAction;
            Label lblInvCode = pnlCurrent.GetLabelByName("lblInvCode");
            TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
            Label lblPrevious = pnlCurrent.GetLabelByName("lblPrevious");
            Label lblItem = pnlCurrent.GetLabelByName("lblItem");
            Label lblHeaderPrice = pnlCurrent.GetLabelByName("lblHeaderPrice");
            Label lblDetailsCount = pnlCurrent.GetLabelByName("lblDetailsCount");

            lblInvCode.ForeColor = Color.Black;
            lblPrevious.Text = SS.PreviousAction;
            lblHeaderPrice.Text = "Цена: " + SS.CCItem.Price.ToString().Trim();
            if (SS.CCItem.InvCode != null)
            {
                lblInvCode.Text = SS.CCItem.InvCode.Trim();
                lblItem.Text = SS.CCItem.Name;
            }
            else
            {
                lblInvCode.Text = "";
                lblItem.Text = "";
            }
            lblDetailsCount.Text = "Деталей: " + SS.CCItem.Details.ToString();
            switch (SS.CurrentAction)
            {

                case ActionSet.ScanItem:
                    tbCount.Visible = false;
                    lblInvCode.ForeColor = Color.Blue;
                    break;

                case ActionSet.EnterCount:
                    tbCount.Visible = true;
                    tbCount.BringToFront();
                    tbCount.Focus();
                    break;
            }

            lblAction.Text = SS.ExcStr;
        }

        private void ReView()
        {
            switch (SS.CurrentMode)
            {
                //case Mode.RefillSet:
                //    ModeRefillSetReView();
                //    break;
                case Mode.Acceptance:
                    lblState.Text = "Приемка (" + (Screan == 0 ? "накладные" : (Screan == -1 ? "НЕ принятый товар" : "принятый, печать этикеток")) + ")" ;
                    ModeAcceptanceReView();
                    break;
                case Mode.AcceptedItem:
                    ModeAcceptanceItemReView();
                    break;
                case Mode.TransferInicialize:
                    ModeTransferInicializeReView();
                    break;
                case Mode.Transfer:
                    lblState.Text = "Разнос (" + (Screan == 0 ? "ТЕЛЕЖКА" : "ПОЛКА") + ") " + SS.ZoneName.TrimEnd() + " " + SS.ZoneAdressTransfer.Name;
                    ModeTransferReView();
                    break;
                case Mode.NewInventory:
                    lblState.Text = "Инвентаризация (" + (Screan == 0 ? "ТЕЛЕЖКА" : "ПОЛКА") + ")";
                    ModeTransferReView();
                    break;
                case Mode.Inventory:
                    ModeInventoryReView();
                    break;
                case Mode.SampleInventory:
                    ModeInventoryReView();
                    break;
                case Mode.SampleSet:
                    lblState.Text = "Набор образцов";
                    ModeSampleSetReView();
                    break;
                case Mode.SampleSetCorrect:
                    ModeSetCorrectReView();
                    break;
                case Mode.SamplePut:
                    lblState.Text = "Выкладка образцов (" + SS.SampleItems.Rows.Count.ToString() + ")";
                    ModeSamplePutReView();
                    break;
                case Mode.ControlCollect:
                    ModeControlCollectReView();
                    break;
                case Mode.HarmonizationInicialize:
                    ModeHarmonizationInicializeReView();
                    break;
                case Mode.Harmonization:
                    ModeHarmonizationReView();
                    break;
                case Mode.HarmonizationPut:
                    ModeHarmonizationReView();
                    break;
                case Mode.LoadingInicialization:
                    ModeLoadingInicializationReView();
                    break;
                case Mode.Loading:
                    ModeLoadingReView(false);
                    break;
                case Mode.Set:
                    ModeSetReView();
                    break;
                case Mode.SetCorrect:
                    ModeSetCorrectReView();
                    break;
                case Mode.SetComplete:
                    ModeSetCompleteReView();
                    break;
                case Mode.Down:
                    ModeDownReView();
                    break;
                case Mode.DownComplete:
                    ModeDownCompleteReView();
                    break;
                case Mode.FreeDownComplete:
                    ModeFreeDownCompleteReView();
                    break;
                case Mode.NewComplectation:
                    ModeNewComplectationReView();
                    break;
                case Mode.NewComplectationComplete:
                    ModeNewComplectationCompleteReView();
                    break;
                case Mode.SetSelfContorl:
                    ModeSetSelfControlReView();
                    break;

                //DEBUG!!!!!!!
                case Mode.RefillLayout:
                    RefillLayout_review();
                    break;
                default:
                    if (SS.NewStructModes.Contains(SS.CurrentMode))
                    {
                        //string nameViewMethod = (SS.MM.GetType()).Name + "_review";
                        string nameViewMethod = SS.MM.CurrentMode.ToString() + "_review";
                        GetType().GetMethod(nameViewMethod, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, null);
                    }
                    break;
            }
        }
    }
}
