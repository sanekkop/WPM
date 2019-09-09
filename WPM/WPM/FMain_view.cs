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

//Старое отображение 

namespace WPM
{
    public partial class FMain : Form
    {
        private void LoadImage(PictureBox Wither, string InvCode)
        {
            string fn = SS.PicturePath.Trim() + Helper.GetPictureFileName(InvCode);
            if (File.Exists(fn))
            {
                Wither.Image = new Bitmap(fn);
            }
            else
            {
                //ФАЙЛА НЕТ, ПОКАЖЕМ КАРТИНКУ ПО УМОЛЧАНИЮ
                fn = SS.PicturePath.Trim() + "__nopic.jpg";
                if (File.Exists(fn))
                {
                    //Картинка по умолчанию
                    Wither.Image = new Bitmap(fn);
                }
                else
                {
                    //нет связи с расшаренной папкой - картинка из сохраненного битмапа на форме (с красным кругом)
                    Wither.Image = this.DefaultPict;
                }
            }
            Wither.SizeMode = PictureBoxSizeMode.StretchImage;
        }
        private void DrawlblResult()
        {
            Label lblResult = new Label();
            lblResult.Font      = CurrWidth < 320 ? FontTahoma13Regular : FontTahoma16Regular;
            //lblResult.Font      = new Font("Tahoma", 16, FontStyle.Regular);
            lblResult.ForeColor = Color.Red;
            lblResult.Name      = "lblResult";
            lblResult.TextAlign = ContentAlignment.TopCenter; 
            lblResult.Location  = new Point(0, 105);
            lblResult.Size      = new Size (CurrWidth - 2, 78);
            pnlCurrent.Controls.Add(lblResult);
        }
        private void FillBarcode(DataRow[] DR, string OKEI)
        {
            DataRow newRow = ViewUnits.NewRow();
            string Barcodes = "";
            int Coef        = 0;
            foreach(DataRow dr in DR)
            {
                Barcodes += dr["Barcode"].ToString().Trim();
                Barcodes += (dr["Barcode"].ToString().Trim() == "" ? "" : " ");
                Coef = (int)dr["Coef"];
                OKEI = dr["OKEI"].ToString();
            }

            newRow["Barcodes"]  = Barcodes;
            newRow["Coef"]      = Coef;
            newRow["OKEI"]      = OKEI;
            ViewUnits.Rows.Add(newRow);
        }
        private void DrawAdressesZone(RefItem item)
        {

            if (!item.ZonaHand.Selected)
            {
                //Зона товара - не задана
                Label lblInfo = new Label();
                lblInfo.Font = FontTahoma16Regular;
                lblInfo.ForeColor = Color.Red;
                lblInfo.Name = "lblInfo";
                lblInfo.Text = "У товара не задана ручная зона!";
                lblInfo.TextAlign = ContentAlignment.TopCenter;
                lblInfo.Location = new Point(1 - CurrWidth, 50);
                lblInfo.Size = new Size(CurrWidth - 2, 50);
                pnlCurrent.Controls.Add(lblInfo);
                return;
            }

            Label lblHeaderZone = new Label();
            lblHeaderZone.Font = FontTahoma14Bold;
            lblHeaderZone.ForeColor = Color.Black;
            lblHeaderZone.Name = "lblHeaderZone";
            lblHeaderZone.Text = "Ручная зона " + item.ZonaHand.Name;
            lblHeaderZone.TextAlign = ContentAlignment.TopCenter;
            lblHeaderZone.Location = new Point(1 - CurrWidth, 52);
            lblHeaderZone.Size = new Size(CurrWidth - 2, 36);
            pnlCurrent.Controls.Add(lblHeaderZone);

            DataTable DT = item.ZonaHand.Ranges;    //Вместо этого подтягивать начало и конец зоны (под вопросом, может и не нужно)

            for (int i = 0; i < DT.Rows.Count; i++)
            {
                Label lblV = new Label();
                lblV.Font = FontTahoma14Regular;
                lblV.ForeColor = Color.Black;
                lblV.BackColor = Color.Yellow;
                lblV.Name = "lblV" + i.ToString();
                lblV.Text = DT.Rows[i]["First"].ToString().Trim() + " .. " + DT.Rows[i]["Last"].ToString().Trim();  //Тут строка где выводятся начало и конец зоны (под вопросом, может и не нужно)
                lblV.TextAlign = ContentAlignment.TopCenter;
                lblV.Location = new Point(5 - CurrWidth, 90 + i * 22);
                lblV.Size = new Size(CurrWidth - 10, 21);
                pnlCurrent.Controls.Add(lblV);

            }

            //Зона таки есть, будем ее отрисовыывать по диапазонам

            //DataTable DT = i
            
            //Label lblResult = new Label();
            //lblResult.Font = CurrWidth < 320 ? FontTahoma13Regular : FontTahoma16Regular;
            ////lblResult.Font      = new Font("Tahoma", 16, FontStyle.Regular);
            //lblResult.ForeColor = Color.Red;
            //lblResult.Name = "lblResult";
            //lblResult.TextAlign = ContentAlignment.TopCenter;
            //lblResult.Location = new Point(0, 105);
            //lblResult.Size = new Size(CurrWidth - 2, 78);
            //pnlCurrent.Controls.Add(lblResult);
        }

        private void ModeAcceptanceView()
        {
            TimerFind.Interval  = 150;
            TimerFind.Enabled   = false;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            BindingSource BS    = new BindingSource();
            BS.DataSource       = SS.NotAcceptedItems;
            BS.Filter           = null;

            DataGrid dgNAI = new DataGrid();
            dgNAI.BackgroundColor = Color.White;
            dgNAI.Location = new Point(-1*Screan*pnlCurrent.Width - (CurrWidth - 6), 20);
            dgNAI.Name = "dgNotAcceptedItems";
            dgNAI.Size = new Size(CurrWidth - 10, 100);
            dgNAI.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgNAI.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgNAI.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№";
            columnStyle.MappingName = "LINENO_";
            columnStyle.Width = 26;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Накл.";
            columnStyle.MappingName = "DOCNO";
            columnStyle.Width = CurrWidth < 320 ? 36 : 40;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Артикул";
            columnStyle.MappingName = "Article";
            columnStyle.Width = CurrWidth < 320 ? 70 : 76;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Арт. на упак.";
            columnStyle.MappingName = "ArticleOnPack";
            columnStyle.Width = CurrWidth < 320 ? 6 : 76;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Количество";
            columnStyle.MappingName = "CountPackage";
            columnStyle.Width = 38;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Коэф";
            columnStyle.MappingName = "CoefView";
            columnStyle.Width = 32;
            dgts.GridColumnStyles.Add(columnStyle);
            dgNAI.TableStyles.Add(dgts);
            #endregion
            dgNAI.DataSource = BS;
            dgNAI.CurrentCellChanged += OnCurrentCellChanged;
            pnlCurrent.Controls.Add(dgNAI);

            DataGrid dgAI = new DataGrid();
            dgAI.BackgroundColor = Color.White;
            dgAI.Location = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth + 2), 20);
            dgAI.Name = "dgAcceptedItems";
            dgAI.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgAI.Size = new Size(CurrWidth - 10, 140);
            dgAI.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgAI.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№";
            columnStyle.MappingName = "LINENO_";
            columnStyle.Width = 24;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Накладная";
            columnStyle.MappingName = "DOCNO";
            columnStyle.Width = 38;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.Код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 53 : 57;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Наименование";
            columnStyle.MappingName = "ItemName";
            columnStyle.Width = CurrWidth < 320 ? 6 : 80;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Количество";
            columnStyle.MappingName = "CountPackage";
            columnStyle.Width = 38;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Коэф";
            columnStyle.MappingName = "CoefView";
            columnStyle.Width = CurrWidth < 320 ? 30 : 32;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Этикеток";
            columnStyle.MappingName = "LabelCount";
            columnStyle.Width = 20;
            dgts.GridColumnStyles.Add(columnStyle);
            dgAI.TableStyles.Add(dgts);
            #endregion
            dgAI.DataSource = SS.AcceptedItems;
            pnlCurrent.Controls.Add(dgAI);

            DataGrid dgC = new DataGrid();
            dgC.BackgroundColor = Color.White;
            dgC.Location = new Point(-1*Screan*pnlCurrent.Width + 9, 20);
            dgC.Name = "dgConsignment";
            dgC.Size = new Size(CurrWidth - 20, 140);
            dgC.Font = FontTahoma10Regular;
            dgC.RowHeadersVisible = false;
            //Настройка отображения
            #region Styles;
            dgC.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№";
            columnStyle.MappingName = "Number";
            columnStyle.Width = 25;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Накладная";
            columnStyle.MappingName = "DOCNO";
            columnStyle.Width = 75;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Дата";
            columnStyle.MappingName = "DateDocText";
            columnStyle.Width = CurrWidth < 320 ? 6 : 55;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Осталось";
            columnStyle.MappingName = "CountNotAcceptRow";
            columnStyle.Width = 55;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Поставщик";
            columnStyle.MappingName = "Client";
            columnStyle.Width = 80;
            dgts.GridColumnStyles.Add(columnStyle);
            dgC.TableStyles.Add(dgts);
            #endregion
            dgC.DataSource = SS.Consignment;
            pnlCurrent.Controls.Add(dgC);

            Label lblPrinter = new Label();
            lblPrinter.Font      = FontTahoma10Bold;
            lblPrinter.ForeColor = Color.DarkGreen;
            lblPrinter.BackColor = Color.LightGray;
            lblPrinter.Name      = "lblPrinter";
            lblPrinter.TextAlign = ContentAlignment.TopCenter; 
            lblPrinter.Location  = new Point(4, 0);
            lblPrinter.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblPrinter);

            Label lblClient = new Label();
            lblClient.Font = FontTahoma10Bold;
            lblClient.ForeColor = Color.DarkGreen;
            lblClient.BackColor = Color.White;
            lblClient.Name = "lblClient";
            lblClient.TextAlign = ContentAlignment.TopCenter;
            lblClient.Location = new Point(-1 * Screan * pnlCurrent.Width + 9,160);
            lblClient.Size = new Size(CurrWidth - 2, 40);
            pnlCurrent.Controls.Add(lblClient);

            Label lblItem = new Label();
            lblItem.Font      = FontTahoma10Regular;
            lblItem.ForeColor = Color.DarkGreen;
            lblItem.BackColor = Color.White;
            lblItem.Name      = "lblItem";
            lblItem.TextAlign = ContentAlignment.TopLeft;
            lblItem.Location  = new Point(-1*Screan*pnlCurrent.Width - (CurrWidth - 2), 145);
            lblItem.Size      = new Size (CurrWidth - 2, 40);
            pnlCurrent.Controls.Add(lblItem);

            //Поле для ввода артикула
            TextBox tbFind = new TextBox();
            tbFind.Name      = "tbFind";
            tbFind.Location  = new Point(-1*Screan*pnlCurrent.Width - (CurrWidth - 6), 120);
            tbFind.Size      = new Size (CurrWidth < 320 ? 100 : 120, 24);
            tbFind.Font      = FontTahoma10Regular;
            tbFind.KeyPress    += DynamicOnKeyPress;
            tbFind.TextChanged += DynamicOnTextChanged;
            pnlCurrent.Controls.Add(tbFind);

            Label lblLabelFind = new Label();
            lblLabelFind.Font      = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            lblLabelFind.BackColor = Color.White;
            lblLabelFind.Name      = "lblLabelFind";
            lblLabelFind.TextAlign = ContentAlignment.TopLeft;
            lblLabelFind.Location  = new Point(-1*Screan*pnlCurrent.Width - (CurrWidth < 320 ? 130 : 190), 120);
            lblLabelFind.Size      = new Size (CurrWidth < 320 ? 130 : 150, 20);
            lblLabelFind.Text      = " <-- поиск по артикулам";
            pnlCurrent.Controls.Add(lblLabelFind);

            //Поле для ввода количества этикеток
            TextBox tbLabelCount = new TextBox();
            tbLabelCount.Name      = "tbLabelCount";
            tbLabelCount.Location  = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth - 2) + (CurrWidth < 320 ? 194 : 264), 160);
            tbLabelCount.Size      = new Size (CurrWidth < 320 ? 40 : 50, 24);
            tbLabelCount.Font      = FontTahoma10Regular;
            tbLabelCount.KeyPress    += DynamicOnKeyPress;
            tbLabelCount.TextChanged += DynamicOnTextChanged;
            tbLabelCount.LostFocus   += DynamicLostFocus;
            pnlCurrent.Controls.Add(tbLabelCount);

            Label lblInvCode = new Label();
            lblInvCode.Font      = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            lblInvCode.ForeColor = Color.DarkGreen;
            lblInvCode.BackColor = Color.White;
            lblInvCode.Name      = "lblInvCode";
            lblInvCode.TextAlign = ContentAlignment.TopRight;
            lblInvCode.Location  = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth - 2) + (CurrWidth < 320 ? 144 : 204), 160);
            lblInvCode.Size      = new Size (CurrWidth < 320 ? 50 : 60, 24);
            pnlCurrent.Controls.Add(lblInvCode);

            //Кнопка распечатать этикетки
            Button btnPrintCondition = new Button();
            btnPrintCondition.Name      = "btnPrintCondition";
            btnPrintCondition.Text      = CurrWidth < 320 ? "Этик." : "Этикетки";
            btnPrintCondition.Location  = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth - 2) + (CurrWidth < 320 ? 93 : 143), 160);
            btnPrintCondition.Size      = new Size (CurrWidth < 320 ? 50 : 60, 24);
            btnPrintCondition.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnPrintCondition);

            //Кнопка распечатать этикетки полюбас
            Button btnPrint = new Button();
            btnPrint.Name      = "btnPrint";
            btnPrint.Text      = CurrWidth < 320 ? "Полюбому" : "Этикетки полюбому";
            btnPrint.Location  = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth + 2), 160);
            btnPrint.Size      = new Size (CurrWidth < 320 ? 85 : 135, 24);
            btnPrint.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnPrint);
            pnlCurrent.StaticControl.Add("lblPrinter");
            lblAction.ForeColor = Color.Blue;
            lblAction.Text      = "Ожидание команды";
            if (SS.ExcStr != null)
            {
                if (SS.ExcStr != "")
                {
                    lblAction.Text = SS.ExcStr;
                }
            }
            
            if (Screan == -1)
            {
                dgNAI.Focus();
            }
            else if (Screan == 1)
            {
                dgAI.Focus();
            }
            else
            {
                dgC.Focus();
            }
        }
        private void ModeTransferInicializeView()
        {
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgFrom = new DataGrid();
            dgFrom.Location = new Point(9, 10);
            dgFrom.Name = "dgFrom";
            dgFrom.Size = new System.Drawing.Size(CurrWidth < 320 ? 105 : 145, 100);
            dgFrom.DataSource = SS.WarehousesFrom;
            dgFrom.RowHeadersVisible = false;
            #region Styles
            dgFrom.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Склад-источник";
            columnStyle.MappingName = "Name";
            columnStyle.Width = CurrWidth < 320 ? 90 : 130;
            dgts.GridColumnStyles.Add(columnStyle);
            dgFrom.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgFrom);

            DataGrid dgTo = new DataGrid();
            dgTo.Location = new Point(CurrWidth < 320 ? 124 : 164, 10);
            dgTo.Name = "dgTo";
            dgTo.Size = new System.Drawing.Size(CurrWidth < 320 ? 105 : 145, 100);
            dgTo.DataSource = SS.WarehousesTo;
            dgTo.RowHeadersVisible = false;
            #region Styles
            dgTo.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Склад-получатель";
            columnStyle.MappingName = "Name";
            columnStyle.Width = CurrWidth < 320 ? 90 : 130;
            dgts.GridColumnStyles.Add(columnStyle);
            dgTo.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgTo);

            Label lblFrom = new Label();
            lblFrom.Font      = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma12Bold;
            lblFrom.BackColor = Color.White;
            lblFrom.Name      = "lblFrom";
            lblFrom.TextAlign = ContentAlignment.TopCenter; 
            lblFrom.Location  = new Point(9, 115);
            lblFrom.Size      = new Size (CurrWidth < 320 ? 105 : 145, 25);
            pnlCurrent.Controls.Add(lblFrom);

            Label lblTo = new Label();
            lblTo.Font      = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma12Bold;
            lblTo.BackColor = Color.White;
            lblTo.Name      = "lblTo";
            lblTo.TextAlign = ContentAlignment.TopCenter; 
            lblTo.Location  = new Point(CurrWidth < 320 ? 124 : 164, 115);
            lblTo.Size      = new Size (CurrWidth < 320 ? 105 : 145, 25);
            pnlCurrent.Controls.Add(lblTo);

            Button btnOk = new Button();
            btnOk.Name      = "btnOk";
            btnOk.Text      = "Я согласен";
            btnOk.Location  = new Point(CurrWidth < 320 ? 44 : 84, 150);
            btnOk.Size      = new Size (150, 25);
            btnOk.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnOk);
            lblAction.Text = "Выберите склад источник и получатель!";
            dgFrom.Focus();
        }
        private void ModeTransferView()
        {
            Screan = 0;
            TimerFind.Interval  = 20000;
            TimerFind.Enabled   = false;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgTI = new DataGrid();
            dgTI.BackgroundColor = Color.White;
            dgTI.Location = new Point(-1*Screan*pnlCurrent.Width + 4, 0);
            dgTI.Name = "dgTransferItems";
            dgTI.Size = new System.Drawing.Size(CurrWidth - 10, 165);
            dgTI.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgTI.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgTI.TableStyles.Clear();
            dgts = new DataGridTableStyle();

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 58 : 78;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Кол-во";
            columnStyle.MappingName = "CountPackage";
            columnStyle.Width = CurrWidth < 320 ? 38 : 55;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Коэф";
            columnStyle.MappingName = "CoefView";
            columnStyle.Width = CurrWidth < 320 ? 32 : 45;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Адрес";
            columnStyle.MappingName = "AdressName";
            columnStyle.Width = CurrWidth < 320 ? 80 : 110;
            dgts.GridColumnStyles.Add(columnStyle);

            dgTI.TableStyles.Add(dgts);
            #endregion
            dgTI.DataSource = SS.TransferItems;
            pnlCurrent.Controls.Add(dgTI);

            DataGrid dgTRI = new DataGrid();
            dgTRI.BackgroundColor = Color.White;
            dgTRI.Location = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth + 2), 0);
            dgTRI.Name = "dgTransferReadyItems";
            dgTRI.Size = new System.Drawing.Size(CurrWidth - 10, 120);
            dgTRI.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgTRI.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgTRI.TableStyles.Clear();
            dgts = new DataGridTableStyle();

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = 57;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Артикул";
            columnStyle.MappingName = "Article";
            columnStyle.Width = CurrWidth < 320 ? 6 : 75;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Количество";
            columnStyle.MappingName = "CountPackage";
            columnStyle.Width = 38;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Коэф";
            columnStyle.MappingName = "CoefView";
            columnStyle.Width = 32;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Адрес";
            columnStyle.MappingName = "AdressName";
            columnStyle.Width = CurrWidth < 320 ? 64 : 76;
            dgts.GridColumnStyles.Add(columnStyle);

            dgTRI.TableStyles.Add(dgts);
            #endregion
            dgTRI.DataSource = SS.TransferReadyItems;
            pnlCurrent.Controls.Add(dgTRI);

            Label lblWarehouse = new Label();
            lblWarehouse.Font      = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Bold;
            lblWarehouse.BackColor = Color.White;
            lblWarehouse.Name      = "lblWarehouse";
            lblWarehouse.TextAlign = ContentAlignment.TopLeft;
            lblWarehouse.Location  = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth + 2), 120);
            lblWarehouse.Size      = new Size (CurrWidth - 10, 20);
            lblWarehouse.Text      = SS.ATDoc.FromWarehouseName + " --> " + SS.ATDoc.ToWarehouseName;
            pnlCurrent.Controls.Add(lblWarehouse);

            Label lblPrinter = new Label();
            lblPrinter.Font      = FontTahoma10Bold;
            lblPrinter.ForeColor = Color.DarkGreen;
            lblPrinter.BackColor = Color.LightBlue;
            lblPrinter.Name      = "lblPrinter";
            lblPrinter.TextAlign = ContentAlignment.TopCenter; 
            lblPrinter.Location  = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth + 2), 140);
            lblPrinter.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblPrinter);

            Label lblItem = new Label();
            lblItem.Font      = CurrWidth < 320 ? FontTahoma7Regular : FontTahoma10Regular;
            lblItem.ForeColor = Color.DarkGreen;
            lblItem.BackColor = Color.White;
            lblItem.Name      = "lblItem";
            lblItem.TextAlign = ContentAlignment.TopLeft;
            lblItem.Location  = new Point(-1*Screan*pnlCurrent.Width + 110, 165);
            lblItem.Size      = new Size (CurrWidth < 320 ? 128 : 208, 20);
            pnlCurrent.Controls.Add(lblItem);

            Label lblAdress = new Label();
            lblAdress.Font      = FontTahoma12Bold;
            lblAdress.ForeColor = Color.DarkRed;
            lblAdress.BackColor = Color.White;
            lblAdress.Name      = "lblAdress";
            lblAdress.TextAlign = ContentAlignment.TopRight;
            lblAdress.Location  = new Point(-1*Screan*pnlCurrent.Width, 165);
            lblAdress.Size      = new Size (110, 20);
            pnlCurrent.Controls.Add(lblAdress);

            //Кнопка распечатать телегу
            Button btnPrint = new Button();
            btnPrint.Name      = "btnPrint";
            btnPrint.Text      = "Завершить";
            btnPrint.Location  = new Point(-1*Screan*pnlCurrent.Width + 322, 160);
            btnPrint.Size      = new Size (150, 24);
            btnPrint.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnPrint);

            //Кнопка подтверждения погрузки в телегу
            Button btnOk = new Button();
            btnOk.Name      = "btnOk";
            btnOk.Text      = "ОК!";
            btnOk.Location  = new Point(Screan*pnlCurrent.Width + (CurrWidth < 320 ? 64 : 94), 140);
            btnOk.Size      = new Size (110, 24);
            btnOk.Click     += DynamicButtonOnClick;
            btnOk.Visible   = false;
            pnlCurrent.Controls.Add(btnOk);

            Label lblResult = new Label();
            lblResult.Font      = FontTahoma16Regular;
            lblResult.ForeColor = Color.Green;
            lblResult.BackColor = Color.LightSteelBlue;
            lblResult.Name      = "lblResult";
            lblResult.TextAlign = ContentAlignment.TopCenter; 
            lblResult.Location  = new Point(-1*Screan*pnlCurrent.Width + 4, 0);
            lblResult.Size      = new Size (CurrWidth - 10, 165);
            lblResult.Visible   = false;
            pnlCurrent.Controls.Add(lblResult);

            lblAction.Text = "Ожидание команды";
            if (Screan == 1)
            {
                dgTRI.Focus();
            }
            else
            {
                dgTI.Focus();
            }
        } // ModeTransferView()
        private void ModeAcceptanceItemView()
        {
            ScreanAcceptedItem = 0;
            SS.AcceptedItem.AcceptCount     = SS.AcceptedItem.Count;
            if (SS.AcceptedItem.ToMode == Mode.SampleInventory || SS.AcceptedItem.ToMode == Mode.SamplePut || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut)
            {
                CurrentRowItem = 0;//У образцов и гармонизации - главная штука
            }
            else
            {
                CurrentRowItem  = 2;//У остальных - главное место
            }
            ViewUnits = new DataTable();
            ViewUnits.Columns.Add("Coef",       Type.GetType("System.Int32"));
            ViewUnits.Columns.Add("OKEI",       Type.GetType("System.String"));
            ViewUnits.Columns.Add("Barcodes",   Type.GetType("System.String"));
            //Заполняем таблицу
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

            TextBox tbCoef = new TextBox();
            tbCoef.Name      = "tbCoef";
            tbCoef.Location  = new Point(-1*ScreanAcceptedItem*pnlCurrent.Width + (CurrWidth < 320 ? 30 : 40), 30 + 26*(CurrentRowItem + 1));
            tbCoef.Size      = new Size (CurrWidth < 320 ? 40 : 60, 27);
            tbCoef.Font      = CurrWidth < 320 ? FontTahoma10Bold : FontTahoma12Bold;
            tbCoef.Enabled      = SS.AcceptedItem.ToMode == Mode.Transfer || SS.AcceptedItem.ToMode == Mode.NewInventory || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut? false : true;
            //tbCoef.Text         = ViewUnits.Rows[CurrentRowItem]["Coef"].ToString();
            tbCoef.KeyPress     += DynamicOnKeyPress;
            tbCoef.TextChanged  += DynamicOnTextChanged;
            tbCoef.LostFocus    += DynamicLostFocus;
            tbCoef.GotFocus     += DynamicGotFocus;
            pnlCurrent.Controls.Add(tbCoef);

            TextBox tbCount = new TextBox();
            tbCount.Name      = "tbCount";
            tbCount.Location  = new Point(-1*ScreanAcceptedItem*pnlCurrent.Width + (CurrWidth < 320 ? 75 : 105), 30 + 26*(CurrentRowItem + 1));
            tbCount.Size      = new Size (CurrWidth < 320 ? 48 : 60, 27);
            tbCount.Font        = CurrWidth < 320 ? FontTahoma10Bold : FontTahoma12Bold;
            if (SS.AcceptedItem.ToMode == Mode.Transfer || SS.AcceptedItem.ToMode == Mode.NewInventory || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut)
            {
                if (SS.AcceptedItem.ToMode == Mode.Transfer)
                {
                    tbCount.Enabled = SS.Employer.CanDiffParty;
                }
                else
                {
                    tbCount.Enabled = true;
                }
            }
            else
            {
                tbCount.Enabled = SS.AcceptedItem.Acceptance;
            }
            //tbCount.Text        = AcceptCount.ToString();
            tbCount.KeyPress    += DynamicOnKeyPress;
            tbCount.TextChanged += DynamicOnTextChanged;
            tbCount.LostFocus   += DynamicLostFocus;
            tbCount.GotFocus    += DynamicGotFocus;
            pnlCurrent.Controls.Add(tbCount);

            ////Раскрашивать?
            //int Coef = (int)ViewUnits.Rows[CurrentRowItem]["Coef"];
            //string textTB = tbCount.Text;
            //if (Coef != 0)
            //{
            //    textTB = (textTB == "" ? "0" : textTB);
            //    int NewAcceptCount = Convert.ToInt32(textTB)*Coef;
            //    if (NewAcceptCount != SS.AcceptedItem.AcceptCount)
            //    {
            //        tbCount.ForeColor = Color.Red;
            //    }
            //    else
            //    {
            //        tbCount.ForeColor = Color.Black;
            //    }
            //}

            //Заголовки таблички
            #region Header table
            Label lblUnit = new Label();
            lblUnit.Font      = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma8Regular;
            lblUnit.BackColor = Color.White;
            lblUnit.Name      = "lblUnit";
            lblUnit.TextAlign = ContentAlignment.TopRight; 
            lblUnit.Location  = new Point(4, 58);
            lblUnit.Size      = new Size (CurrWidth < 320 ? 26 : 36, 24);
            lblUnit.Text      = "Шт.";
            pnlCurrent.Controls.Add(lblUnit);

            Label lblPack = new Label();
            lblPack.Font      = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma8Regular;
            lblPack.BackColor = Color.White;
            lblPack.Name      = "lblPack";
            lblPack.TextAlign = ContentAlignment.TopRight; 
            lblPack.Location  = new Point(4, 83);
            lblPack.Size      = new Size (CurrWidth < 320 ? 26 : 36, 24);
            lblPack.Text      = "Упак.";
            pnlCurrent.Controls.Add(lblPack);

            Label lblPlace = new Label();
            lblPlace.Font      = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma8Regular;
            lblPlace.BackColor = Color.White;
            lblPlace.Name      = "lblPlace";
            lblPlace.TextAlign = ContentAlignment.TopRight; 
            lblPlace.Location  = new Point(4, 108);
            lblPlace.Size      = new Size (CurrWidth < 320 ? 26 : 36, 24);
            lblPlace.Text      = "Место";
            pnlCurrent.Controls.Add(lblPlace);

            Label lblKit = new Label();
            lblKit.Font      = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma8Regular;
            lblKit.ForeColor = Color.Red;
            lblKit.BackColor = Color.White;
            lblKit.Name      = "lblKit";
            lblKit.TextAlign = ContentAlignment.TopRight; 
            lblKit.Location  = new Point(4, 133);
            lblKit.Size      = new Size (CurrWidth < 320 ? 26 : 36, 24);
            lblKit.Text      = "Набор";
            pnlCurrent.Controls.Add(lblKit);

            //Label lblOther = new Label();
            //lblOther.Font      = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma8Regular;
            //lblOther.BackColor = Color.White;
            //lblOther.Name      = "lblOther";
            //lblOther.TextAlign = ContentAlignment.TopRight; 
            //lblOther.Location  = new Point(4, 133);
            //lblOther.Size      = new Size (CurrWidth < 320 ? 26 : 36, 24);
            //lblOther.Text      = "Проч.";
            //pnlCurrent.Controls.Add(lblOther);

            Label lblC = new Label();
            lblC.Font      = FontTahoma14Bold;
            lblC.BackColor = Color.Thistle;
            lblC.Name      = "lblC";
            lblC.TextAlign = ContentAlignment.TopCenter; 
            lblC.Location  = new Point(CurrWidth < 320 ? 30 : 40, 160);
            lblC.Size      = new Size (CurrWidth < 320 ? 50 : 60, 25);
            lblC.Text      = SS.AcceptedItem.StoregeSize.ToString();
            lblC.Visible = SS.AcceptedItem.StoregeSize != 0;
            pnlCurrent.Controls.Add(lblC);

            Label lblA = new Label();
            lblA.Font      = FontTahoma10Regular;
            lblA.BackColor = Color.White;
            lblA.Name      = "lblA";
            lblA.TextAlign = ContentAlignment.TopCenter; 
            lblA.Location  = new Point(CurrWidth < 320 ? 75 : 110, 160);
            lblA.Size      = new Size (50, 25);
            lblA.Text      = "Кол-во";
            pnlCurrent.Controls.Add(lblA);
            #endregion;

            if (SS.CurrentMode == Mode.AcceptedItem && SS.AcceptedItem.FlagFarWarehouse != 0)
            {
                Label lblGrSeas = new Label();
                lblGrSeas.Font = FontTahoma18Bold;
                lblGrSeas.BackColor = Color.Red;
                lblGrSeas.ForeColor = Color.Yellow;
                lblGrSeas.Name = "lblGrSeas";
                lblGrSeas.TextAlign = ContentAlignment.TopCenter;
                lblGrSeas.Location = new Point(2, 155);
                lblGrSeas.Size = new Size(40, 30);
                lblGrSeas.Text = SS.AcceptedItem.SeasonGroup.ToString();
                pnlCurrent.Controls.Add(lblGrSeas);
            }

            //Рисуем табличку ШК
            for (int i = 0; i < ViewUnits.Rows.Count - 1; ++i)  //убрал прочее 
            {
                Label lblCoef = new Label();
                lblCoef.Font      = CurrWidth < 320 ? FontTahoma10Bold : FontTahoma12Bold;
                lblCoef.BackColor = Color.LightGray;
                lblCoef.Name      = "lblCoef" + i.ToString();
                lblCoef.TextAlign = ContentAlignment.TopLeft;
                lblCoef.Location  = new Point(CurrWidth < 320 ? 30 : 40, 30 + 26*(i+1));
                lblCoef.Size      = new Size (CurrWidth < 320 ? 40 : 60, 25);
                pnlCurrent.Controls.Add(lblCoef);

                Label lblCount = new Label();
                lblCount.Font      = CurrWidth < 320 ? FontTahoma10Bold : FontTahoma12Bold;
                lblCount.BackColor = Color.LightGray;
                lblCount.Name      = "lblCount" + i.ToString();
                lblCount.TextAlign = ContentAlignment.TopLeft;
                lblCount.Location  = new Point(CurrWidth < 320 ? 75 : 105, 30 + 26*(i+1));
                lblCount.Size      = new Size (CurrWidth < 320 ? 48 : 60, 25);
                pnlCurrent.Controls.Add(lblCount);

                Label lblCountBegin = new Label();
                lblCountBegin.Font      = CurrWidth < 320 ? FontTahoma10Regular : FontTahoma12Regular;
                lblCountBegin.ForeColor  = (i == 3 ? Color.White : Color.Black);
                lblCountBegin.BackColor  = (i == 3 ? Color.Red : Color.LightGray);
                lblCountBegin.Name      = "lblCountBegin" + i.ToString();
                lblCountBegin.TextAlign = ContentAlignment.TopLeft;
                lblCountBegin.Location  = new Point(CurrWidth < 320 ? 128 : 170, 30 + 26*(i+1));
                lblCountBegin.Size      = new Size (CurrWidth < 320 ? 35 : 50, 25);
                pnlCurrent.Controls.Add(lblCountBegin);

                Label lblBarcodes = new Label();
                lblBarcodes.Font      = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma6Regular;
                lblBarcodes.BackColor = Color.LightGray;
                lblBarcodes.Name      = "lblBarcodes" + i.ToString();
                lblBarcodes.TextAlign = ContentAlignment.TopLeft;
                lblBarcodes.Location  = new Point(CurrWidth < 320 ? 165 : 225, 30 + 26*(i+1));
                lblBarcodes.Size      = new Size (CurrWidth < 320 ? 72 : 89, 25);
                pnlCurrent.Controls.Add(lblBarcodes);
            }

            Button btnOk = new Button();
            btnOk.Name      = "btnOk";
            btnOk.Text      = (SS.AcceptedItem.Acceptance ? "Принять" : "Сохранить");
            btnOk.Location  = new Point(CurrWidth < 320 ? 167 : 225, 160);
            btnOk.Size      = new Size (CurrWidth < 320 ? 68 : 89, 25);
            btnOk.Click     += DynamicButtonOnClick;
            if ((SS.AcceptedItem.ToMode == Mode.Transfer || SS.AcceptedItem.ToMode == Mode.NewInventory || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut) && SS.AcceptedItem.OnShelf)
            {
                btnOk.Enabled = false;
            }
            pnlCurrent.Controls.Add(btnOk);

            Label lblHeaderPrice = new Label();
            lblHeaderPrice.Font      = CurrWidth < 320 ? FontTahoma7Bold : FontTahoma8Bold;
            lblHeaderPrice.ForeColor = Color.DarkRed;
            lblHeaderPrice.Name      = "lblHeaderPrice";
            lblHeaderPrice.TextAlign = ContentAlignment.TopRight; 
            lblHeaderPrice.Location  = new Point(CurrWidth < 320 ? 96 : 155, 159);
            lblHeaderPrice.Size      = new Size (70, 26);
            lblHeaderPrice.Text      = "Цена: " + SS.AcceptedItem.Price.ToString().Trim();
            pnlCurrent.Controls.Add(lblHeaderPrice);

            //Заголовок артикул
            Label lblHeaderArticle = new Label();
            lblHeaderArticle.Font      = FontTahoma8Regular;
            //lblHeaderArticle.BackColor = Color.White;
            lblHeaderArticle.BackColor = Color.Cyan;
            lblHeaderArticle.Name      = "lblHeaderArticle";
            lblHeaderArticle.TextAlign = ContentAlignment.TopLeft; 
            lblHeaderArticle.Location  = new Point(CurrWidth < 320 ? 0 : 4, 0);
            lblHeaderArticle.Size      = new Size (CurrWidth < 320 ? 130 : 205, 50);
            lblHeaderArticle.Text      = SS.AcceptedItem.Acticle.Trim() + " " + SS.AcceptedItem.Name.Trim();
            pnlCurrent.Controls.Add(lblHeaderArticle);

            Label lblMain = new Label();
            lblMain.Font      = FontTahoma8Regular;
            lblMain.ForeColor = Color.DarkBlue;
            lblMain.BackColor = Color.Yellow;
            lblMain.Name      = "lblMain";
            lblMain.TextAlign = ContentAlignment.TopRight; 
            lblMain.Location  = new Point(CurrWidth < 320 ? 129 : 209, 0);
            lblMain.Size      = new Size (105, 15);
            lblMain.Text      = SS.AcceptedItem.AdressMain.Trim() + ": " + SS.AcceptedItem.BalanceMain.ToString() + " шт";
            pnlCurrent.Controls.Add(lblMain);

            Label lblBuffer = new Label();
            lblBuffer.Font      = FontTahoma8Regular;
            lblBuffer.BackColor = Color.Yellow;
            lblBuffer.Name      = "lblBuffer";
            lblBuffer.TextAlign = ContentAlignment.TopRight; 
            lblBuffer.Location  = new Point(CurrWidth < 320 ? 129 : 209, 15);
            lblBuffer.Size      = new Size (105, 15);
            lblBuffer.Text      = SS.AcceptedItem.AdressBuffer.Trim() + ": " + SS.AcceptedItem.BalanceBuffer.ToString() + " шт";
            pnlCurrent.Controls.Add(lblBuffer);

            if ((int)ViewUnits.Rows[CurrentRowItem]["Coef"] > 0 && SS.AcceptedItem.Acceptance || SS.AcceptedItem.ToMode == Mode.Transfer || SS.AcceptedItem.ToMode == Mode.NewInventory || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut)
            {
                DynamicGotFocus(tbCoef, null);
                tbCount.Focus();
            }
            else
            {
                DynamicGotFocus(tbCount, null);
                tbCoef.Focus();
            }
            tbCoef.BringToFront();
            tbCount.BringToFront();

            //begin govnocode
            Label lblHeaderDetail = new Label();
            lblHeaderDetail.Font = FontTahoma8Regular;
            lblHeaderDetail.ForeColor = Color.DarkBlue;
            lblHeaderDetail.Name = "lblHeaderDetail";
            lblHeaderDetail.TextAlign = ContentAlignment.TopRight;
            lblHeaderDetail.Location = new Point(CurrWidth < 320 ? 129 : 209, 30);
            lblHeaderDetail.Size = new Size(70, 16);
            lblHeaderDetail.Text = " Деталей: ";
            pnlCurrent.Controls.Add(lblHeaderDetail);

            TextBox tbDetails = new TextBox();
            tbDetails.Name = "tbDetails";
            tbDetails.Location = new Point(CurrWidth < 320 ? 204 : 284, 30);
            tbDetails.Size = new Size(30, 27);
            tbDetails.Font = FontTahoma10Bold;
            tbDetails.Text = SS.AcceptedItem.NowDetails.ToString();
            tbDetails.Enabled = SS.Employer.CanAcceptance;  //Детали редактировать может только приемщик

            //begin govnocode
            Label lblZoneAdress = new Label();
            lblZoneAdress.Font = FontTahoma8Regular;
            lblZoneAdress.ForeColor = Color.Black;
            lblZoneAdress.Name = "lblZoneAdress";
            lblZoneAdress.TextAlign = ContentAlignment.TopRight;
            lblZoneAdress.Location = new Point(CurrWidth < 320 ? 129 : 209, 43);
            lblZoneAdress.Size = new Size(70, 16);
            lblZoneAdress.Text = "<-- зоны там";
            pnlCurrent.Controls.Add(lblZoneAdress);

            //Костыль, временно, чтобы не прортить FExcStr

            string tmpExc = SS.ExcStr;  //Это тоже жесть, смотри в ExcStr самого родительского класса SQLSynchronizer

            RefItem Item = new RefItem(SS);
            Item.FoundID(SS.AcceptedItem.ID);

            
            bool IsAndressHand = false;
            if (Item.ZonaHand.Selected)
            {
                if (Item.ZonaHand.FirstAdress.Selected)
                {
                    IsAndressHand = true;
                }
            }
            SS.ExcStr = tmpExc;
            if (SS.AcceptedItem.ToMode == Mode.Transfer && IsAndressHand)
            {
                lblZoneAdress.BackColor = Color.Yellow;
                lblBuffer.BackColor = Color.White;
                lblMain.BackColor = Color.White;
            }
            tbDetails.KeyPress += DynamicOnKeyPress;
            tbDetails.TextChanged += DynamicOnTextChanged;
            tbDetails.LostFocus += DynamicLostFocus;
            tbDetails.GotFocus += DynamicGotFocus;
            pnlCurrent.Controls.Add(tbDetails);
            //end add code

            if (!SS.AcceptedItem.Acceptance) 
            {
                //Это не приемка, а редактирование карточки, значит будем выводить кучу разной фигни одполнительно
                DataGridTextBoxColumn columnStyle;
                DataGridTableStyle dgts;

                DataGrid dgInfo = new DataGrid();
                dgInfo.Location = new Point(-1*ScreanAcceptedItem*pnlCurrent.Width + (CurrWidth + 2), 50);
                dgInfo.Name = "dgInfo";
                dgInfo.Size = new System.Drawing.Size(CurrWidth - 10, 110);
                dgInfo.DataSource = SS.AdressConditionItem;
                dgInfo.RowHeadersVisible = false;
                #region Styles
                dgInfo.TableStyles.Clear();
                dgts = new DataGridTableStyle();
                columnStyle = new DataGridTextBoxColumn();
                columnStyle.HeaderText = "Адрес";
                columnStyle.MappingName = "Adress";
                columnStyle.Width = CurrWidth < 320 ? 80 : 100;
                dgts.GridColumnStyles.Add(columnStyle);
                columnStyle = new DataGridTextBoxColumn();
                columnStyle.HeaderText = "Состояние";
                columnStyle.MappingName = "Condition";
                columnStyle.Width = CurrWidth < 320 ? 80 : 130;
                dgts.GridColumnStyles.Add(columnStyle);
                dgInfo.TableStyles.Add(dgts);
                columnStyle = new DataGridTextBoxColumn();
                columnStyle.HeaderText = "Кол-во";
                columnStyle.MappingName = "Count";
                columnStyle.Width = CurrWidth < 320 ? 50 : 60;
                dgts.GridColumnStyles.Add(columnStyle);
                dgInfo.TableStyles.Add(dgts);
                #endregion
                pnlCurrent.Controls.Add(dgInfo);

                pnlCurrent.StaticControl.Add("lblMain");
                pnlCurrent.StaticControl.Add("lblBuffer");
                pnlCurrent.StaticControl.Add("lblHeaderArticle");
            }

            PictureBox pbPhoto = new PictureBox();
            pbPhoto.Name = "pbPhoto";
            pbPhoto.Location = new Point(0, 0);
            pbPhoto.Size = new Size(320, 185);
            pbPhoto.Visible = false;
            pnlCurrent.Controls.Add(pbPhoto);

            DrawAdressesZone(Item);

        }//ModeAcceptanceItemView()
        private void ModeChoiseInventoryView()
        {
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgTo = new DataGrid();
            dgTo.Location = new Point(CurrWidth < 320 ? 44 : 84, 10);
            dgTo.Name = "dgTo";
            dgTo.Size = new System.Drawing.Size(150, 140);
            dgTo.DataSource = SS.WarehousesTo;
            dgTo.RowHeadersVisible = false;
            #region Styles
            dgTo.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Склад инвентаризации";
            columnStyle.MappingName = "Name";
            columnStyle.Width = 130;
            dgts.GridColumnStyles.Add(columnStyle);
            dgTo.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgTo);

            Button btnOk = new Button();
            btnOk.Name      = "btnOk";
            btnOk.Text      = "Я согласен";
            btnOk.Location  = new Point(CurrWidth < 320 ? 44 : 84, 150);
            btnOk.Size      = new Size (150, 25);
            btnOk.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnOk);
            lblAction.Text = "Выберите склад!";
            dgTo.Focus();

        }
        private void ModeInventoryView()
        {
            Screan = 0;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;
            BindingSource BS    = new BindingSource();
            BS.DataSource       = SS.Inventory;

            DataGrid dgI = new DataGrid();
            dgI.BackgroundColor = Color.White;
            dgI.Location = new Point(-1*Screan*pnlCurrent.Width + 4, 0);
            dgI.Name = "dgInventory";
            dgI.Size = new Size(CurrWidth - 10, 130);
            dgI.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgI.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgI.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№";
            columnStyle.MappingName = "Number";
            columnStyle.Width = CurrWidth < 320 ? 24 : 30;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 48 : 60;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Артикул";
            columnStyle.MappingName = "Article";
            columnStyle.Width = CurrWidth < 320 ? 70 : 100;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Арт. на упак.";
            columnStyle.MappingName = "ArticleOnPack";
            columnStyle.Width = CurrWidth < 320 ? 70 : 100;
            dgts.GridColumnStyles.Add(columnStyle);
            dgI.TableStyles.Add(dgts);
            #endregion
            dgI.DataSource = BS;
            dgI.CurrentCellChanged += OnCurrentCellChanged;
            pnlCurrent.Controls.Add(dgI);

            Label lblItem = new Label();
            lblItem.Font      = FontTahoma10Regular;
            lblItem.ForeColor = Color.DarkGreen;
            lblItem.BackColor = Color.White;
            lblItem.Name      = "lblItem";
            lblItem.TextAlign = ContentAlignment.TopLeft;
            lblItem.Location  = new Point(-1*Screan*pnlCurrent.Width, 130);
            lblItem.Size      = new Size (CurrWidth - 2, 30);
            pnlCurrent.Controls.Add(lblItem);

            //Поле для ввода поисковой информации
            TextBox tbFindItem = new TextBox();
            tbFindItem.Name      = "tbFindItem";
            tbFindItem.Location  = new Point(-1*Screan*pnlCurrent.Width + 4, 160);
            tbFindItem.Size      = new Size (120, 24);
            tbFindItem.Font      = FontTahoma10Regular;
            tbFindItem.KeyPress  += DynamicOnKeyPress;
            pnlCurrent.Controls.Add(tbFindItem);

            Label lblHeaderPrice = new Label();
            lblHeaderPrice.Font      = FontTahoma10Bold;
            lblHeaderPrice.ForeColor = Color.DarkRed;
            lblHeaderPrice.Name      = "lblHeaderPrice";
            lblHeaderPrice.TextAlign = ContentAlignment.TopRight; 
            lblHeaderPrice.Location  = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth < 320 ? 138 : 218), 160);
            lblHeaderPrice.Size      = new Size (100, 24);
            pnlCurrent.Controls.Add(lblHeaderPrice);

            //Новая хуйня
            DataGrid dgC = new DataGrid();
            dgC.BackgroundColor = Color.White;
            dgC.Location = new Point(-1*Screan*pnlCurrent.Width - (CurrWidth - 6), 0);
            dgC.Name = "dgC";
            dgC.Size = new Size(CurrWidth - 10, 140);
            dgC.Font = FontTahoma10Regular;
            dgC.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgC.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№";
            columnStyle.MappingName = "Number";
            columnStyle.Width = CurrWidth < 320 ? 24 : 30;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 48 : 60;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Артикул";
            columnStyle.MappingName = "Article";
            columnStyle.Width = CurrWidth < 320 ? 80 : 110;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Цена";
            columnStyle.MappingName = "Price";
            columnStyle.Width = CurrWidth < 320 ? 60 : 90;
            dgts.GridColumnStyles.Add(columnStyle);
            dgC.TableStyles.Add(dgts);
            #endregion
            dgC.DataSource = SS.ForPrint;
            pnlCurrent.Controls.Add(dgC);

            Label lblPrinter = new Label();
            lblPrinter.Font      = FontTahoma10Bold;
            lblPrinter.ForeColor = Color.DarkGreen;
            lblPrinter.BackColor = Color.LightBlue;
            lblPrinter.Name      = "lblPrinter";
            lblPrinter.TextAlign = ContentAlignment.TopCenter; 
            lblPrinter.Location  = new Point(-1*Screan*pnlCurrent.Width - (CurrWidth - 11), 140);
            lblPrinter.Size      = new Size (CurrWidth - 20, 20);
            pnlCurrent.Controls.Add(lblPrinter);

            //Кнопка распечатать ценники
            Button btnPrintPrices = new Button();
            btnPrintPrices.Name      = "btnPrintPrices";
            btnPrintPrices.Text      = "Ценники";
            btnPrintPrices.Location  = new Point(-1*Screan*pnlCurrent.Width - (CurrWidth - 11), 160);
            btnPrintPrices.Size      = new Size (150, 24);
            btnPrintPrices.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnPrintPrices);
            //Конец новой хуйни

            tbFindItem.Focus();
            lblAction.Text      = "Ожидание команды";
            if (SS.ExcStr != null)
            {
                if (SS.ExcStr != "")
                {
                    lblAction.Text = SS.ExcStr;
                }
            }
        }
        private void ModeSamplePutView()
        {
            TimerFind.Interval  = 150;
            TimerFind.Enabled   = false;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            BindingSource BS    = new BindingSource();
            BS.DataSource       = SS.SampleItems;
            BS.Filter           = null;

            DataGrid dgSamples = new DataGrid();
            dgSamples.BackgroundColor = Color.White;
            dgSamples.Location = new Point(-1*Screan*pnlCurrent.Width + 4, 0);
            dgSamples.Name = "dgSamples";
            dgSamples.Size = new Size(CurrWidth - 10, 120);
            dgSamples.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgSamples.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgSamples.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 57 : 80;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Артикул";
            columnStyle.MappingName = "Article";
            columnStyle.Width = CurrWidth < 320 ? 105 : 138;
            dgts.GridColumnStyles.Add(columnStyle);
            //columnStyle = new DataGridTextBoxColumn();
            //columnStyle.HeaderText = "Наим.";
            //columnStyle.MappingName = "ItemName";
            //columnStyle.Width = 108;
            //dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Кол-во";
            columnStyle.MappingName = "Count";
            columnStyle.Width = CurrWidth < 320 ? 50 : 70;
            dgts.GridColumnStyles.Add(columnStyle);
            dgSamples.TableStyles.Add(dgts);
            #endregion
            dgSamples.DataSource = BS;
            dgSamples.CurrentCellChanged += OnCurrentCellChanged;
            pnlCurrent.Controls.Add(dgSamples);

            DataGrid dgTRI = new DataGrid();
            dgTRI.BackgroundColor = Color.White;
            dgTRI.Location = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth + 2), 0);
            dgTRI.Name = "dgTransferReadyItems";
            dgTRI.Size = new System.Drawing.Size(CurrWidth - 10, 160);
            dgTRI.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgTRI.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgTRI.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 57 : 80;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Артикул";
            columnStyle.MappingName = "Article";
            columnStyle.Width = CurrWidth < 320 ? 105 : 138;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Кол-во";
            columnStyle.MappingName = "Count";
            columnStyle.Width = CurrWidth < 320 ? 50 : 70;
            dgts.GridColumnStyles.Add(columnStyle);
            dgTRI.TableStyles.Add(dgts);
            #endregion
            dgTRI.DataSource = SS.TransferReadyItems;
            dgTRI.CurrentCellChanged += OnCurrentCellChanged;
            pnlCurrent.Controls.Add(dgTRI);

            DataGrid dgC = new DataGrid();
            dgC.BackgroundColor = Color.White;
            dgC.Location = new Point(-1*Screan*pnlCurrent.Width - (CurrWidth - 11), 0);
            dgC.Name = "dgConsignment";
            dgC.Size = new Size(CurrWidth - 20, 140);
            dgC.Font = FontTahoma10Regular;
            dgC.RowHeadersVisible = false;
            //Настройка отображения
            #region Styles;
            dgC.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№";
            columnStyle.MappingName = "Number";
            columnStyle.Width = 30;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Накладная";
            columnStyle.MappingName = "DOCNO";
            columnStyle.Width = 90;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Дата";
            columnStyle.MappingName = "DateDocText";
            columnStyle.Width = CurrWidth < 320 ? 6 : 90;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Осталось";
            columnStyle.MappingName = "CountNotAcceptRow";
            columnStyle.Width = 84;
            dgts.GridColumnStyles.Add(columnStyle);
            dgC.TableStyles.Add(dgts);
            #endregion
            dgC.DataSource = SS.SampleTransfers;
            pnlCurrent.Controls.Add(dgC);

            Label lblPrinter = new Label();
            lblPrinter.Font      = FontTahoma10Bold;
            lblPrinter.ForeColor = Color.DarkGreen;
            lblPrinter.BackColor = Color.LightBlue;
            lblPrinter.Name      = "lblPrinter";
            lblPrinter.TextAlign = ContentAlignment.TopCenter; 
            lblPrinter.Location  = new Point(-1*Screan*pnlCurrent.Width - (CurrWidth - 11), 140);
            lblPrinter.Size      = new Size (CurrWidth - 20, 20);
            pnlCurrent.Controls.Add(lblPrinter);

            //Кнопка распечатать ценники
            Button btnPrintPrices = new Button();
            btnPrintPrices.Name      = "btnPrintPrices";
            btnPrintPrices.Text      = "Ценники";
            btnPrintPrices.Location  = new Point(-1*Screan*pnlCurrent.Width - (CurrWidth - 11), 160);
            btnPrintPrices.Size      = new Size (150, 24);
            btnPrintPrices.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnPrintPrices);

            Label lblItem = new Label();
            lblItem.Font      = FontTahoma10Regular;
            lblItem.ForeColor = Color.DarkGreen;
            lblItem.BackColor = Color.White;
            lblItem.Name      = "lblItem";
            lblItem.TextAlign = ContentAlignment.TopLeft;
            lblItem.Location  = new Point(-1*Screan*pnlCurrent.Width, 145);
            lblItem.Size      = new Size (CurrWidth - 2, 40);
            pnlCurrent.Controls.Add(lblItem);

            //Поле для ввода инв. код
            TextBox tbFind = new TextBox();
            tbFind.Name      = "tbFind";
            tbFind.Location  = new Point(-1*Screan*pnlCurrent.Width + 4, 120);
            tbFind.Size      = new Size (120, 24);
            tbFind.Font      = FontTahoma10Regular;
            tbFind.KeyPress    += DynamicOnKeyPress;
            tbFind.TextChanged += DynamicOnTextChanged;
            pnlCurrent.Controls.Add(tbFind);

            Label lblLabelFind = new Label();
            lblLabelFind.Font      = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            lblLabelFind.BackColor = Color.White;
            lblLabelFind.Name      = "lblLabelFind";
            lblLabelFind.TextAlign = ContentAlignment.TopLeft;
            lblLabelFind.Location  = new Point(-1*Screan*pnlCurrent.Width + 128, 120);
            lblLabelFind.Size      = new Size (CurrWidth < 320 ? 112 : 150, 20);
            lblLabelFind.Text      = " <-- поиск по инв.коду";
            pnlCurrent.Controls.Add(lblLabelFind);

            //Кнопка "я принял эту кучу образцов"
            Button btnPrint = new Button();
            btnPrint.Name      = "btnPrint";
            btnPrint.Text      = "Завершить";
            btnPrint.Location  = new Point(-1*Screan*pnlCurrent.Width + (CurrWidth + 2), 160);
            btnPrint.Size      = new Size (150, 24);
            btnPrint.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnPrint);
            lblAction.Text      = "Ожидание команды";
            if (SS.ExcStr != null)
            {
                if (SS.ExcStr != "")
                {
                    lblAction.Text = SS.ExcStr;
                }
            }
            if (Screan == 0)
            {
                dgSamples.Focus();
            }
            else // Screan == 1
            {
                dgTRI.Focus();
            }

        } // ModeSamplePutView()
        private void ModeNewChoiseInventoryView()
        {
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgTo = new DataGrid();
            dgTo.Location = new Point(CurrWidth < 320 ? 44 : 84, 10);
            dgTo.Name = "dgTo";
            dgTo.Size = new System.Drawing.Size(150, 120);
            dgTo.DataSource = SS.Sections;
            dgTo.RowHeadersVisible = false;
            #region Styles
            dgTo.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Сектора набора";
            columnStyle.MappingName = "Name";
            columnStyle.Width = 130;
            dgts.GridColumnStyles.Add(columnStyle);
            dgTo.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgTo);

            Button btnOk = new Button();
            btnOk.Name      = "btnOk";
            btnOk.Text      = "Запросить задание";
            btnOk.Location  = new Point(CurrWidth < 320 ? 44 : 84, 150);
            btnOk.Size      = new Size (150, 25);
            btnOk.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnOk);
            lblAction.Text = "Запрос задания на указанные секции...";
            btnOk.Focus();

        }
        private void ModeSampleSetView()
        {
            lblState.Text = "Набор образцов";
            lblAction.Text = "Ожидание команды";

            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgGoodsCC = new DataGrid();
            dgGoodsCC.Location = new Point(CurrWidth + 40, 0);
            dgGoodsCC.Name = "dgGoodsCC";
            dgGoodsCC.Size = new System.Drawing.Size(CurrWidth - 80, 165);
            dgGoodsCC.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgGoodsCC.DataSource = SS.ATTable;
            dgGoodsCC.RowHeadersVisible = false;
            #region Styles
            dgGoodsCC.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№";
            columnStyle.MappingName = "Number";
            columnStyle.Width = CurrWidth < 320 ? 20 : 35;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Адрес";
            columnStyle.MappingName = "Adress";
            columnStyle.Width = CurrWidth < 320 ? 60 : 90;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 49 : 68;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Кол.";
            columnStyle.MappingName = "Count";
            columnStyle.Width = CurrWidth < 320 ? 30 : 40;
            dgts.GridColumnStyles.Add(columnStyle);

            dgGoodsCC.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgGoodsCC);

            Label lblHeaderSum = new Label();
            lblHeaderSum.Font = FontTahoma8Bold;
            lblHeaderSum.ForeColor = Color.DarkRed;
            lblHeaderSum.Name = "lblHeaderSum";
            lblHeaderSum.TextAlign = ContentAlignment.TopCenter;
            lblHeaderSum.Location = new Point(2 * CurrWidth - 120 - 3, 165 + 2);
            lblHeaderSum.Size = new Size(120, 20);
            pnlCurrent.Controls.Add(lblHeaderSum);

            Label lblHeader = new Label();
            lblHeader.Font = FontTahoma12Bold;
            lblHeader.Name = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter;
            lblHeader.Location = new Point(4, 0);
            lblHeader.Size = new Size(CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblHeader);

            Label lblPrevious = new Label();
            lblPrevious.Font = FontTahoma10Bold;
            lblPrevious.BackColor = Color.LightGray;
            lblPrevious.Name = "lblPrevious";
            lblPrevious.TextAlign = ContentAlignment.TopCenter;
            lblPrevious.Location = new Point(4, 20 + 1);
            lblPrevious.Size = new Size(CurrWidth - 10, 32);
            pnlCurrent.Controls.Add(lblPrevious);

            Label lblHeaderPrice = new Label();
            lblHeaderPrice.Font = FontTahoma8Bold;
            lblHeaderPrice.ForeColor = Color.DarkRed;
            lblHeaderPrice.Name = "lblHeaderPrice";
            lblHeaderPrice.TextAlign = ContentAlignment.TopCenter;
            lblHeaderPrice.Location = new Point(0, 50 + 4);
            lblHeaderPrice.Size = new Size(50, 30);
            pnlCurrent.Controls.Add(lblHeaderPrice);

            Label lblHeaderBalance = new Label();
            lblHeaderBalance.Font = FontTahoma8Bold;
            lblHeaderBalance.ForeColor = Color.DarkRed;
            lblHeaderBalance.Name = "lblHeaderBalance";
            lblHeaderBalance.TextAlign = ContentAlignment.TopCenter;
            lblHeaderBalance.Location = new Point(CurrWidth - 50, 50 + 4);
            lblHeaderBalance.Size = new Size(50, 30);
            pnlCurrent.Controls.Add(lblHeaderBalance);

            Label lblAdress = new Label();
            lblAdress.Font = FontTahoma16Bold;
            lblAdress.Name = "lblAdress";
            lblAdress.TextAlign = ContentAlignment.TopCenter;
            lblAdress.Location = new Point(50, 50 + 4);
            lblAdress.Size = new Size(CurrWidth - 100, 25);
            pnlCurrent.Controls.Add(lblAdress);

            Label lblInvCode = new Label();
            lblInvCode.Font = FontTahoma16Bold;
            lblInvCode.Name = "lblInvCode";
            lblInvCode.TextAlign = ContentAlignment.TopRight;
            lblInvCode.Location = new Point(4, 75 + 6);
            lblInvCode.Size = new Size(100, 25 + 6);
            pnlCurrent.Controls.Add(lblInvCode);

            Label lblItem = new Label();
            lblItem.Font = FontTahoma10Regular;
            lblItem.Name = "lblItem";
            lblItem.TextAlign = ContentAlignment.TopLeft;
            lblItem.Location = new Point(106, 75 + 6 - 2);
            lblItem.Size = new Size(CurrWidth - 106, 25 + 6);
            pnlCurrent.Controls.Add(lblItem);

            Label lblCount = new Label();
            lblCount.Font = FontTahoma16Bold;
            lblCount.Name = "lblCount";
            lblCount.TextAlign = ContentAlignment.TopCenter;
            lblCount.Location = new Point(4, 100 + 8 + 2);
            lblCount.Size = new Size(CurrWidth - 10, 25);
            pnlCurrent.Controls.Add(lblCount);

            Label lblBox = new Label();
            lblBox.Font = FontTahoma12Bold;
            lblBox.Name = "lblBox";
            lblBox.TextAlign = ContentAlignment.TopCenter;
            lblBox.Location = new Point(50, 125 + 10);
            lblBox.Size = new Size(CurrWidth - 100, 20);
            pnlCurrent.Controls.Add(lblBox);

            Label lblDetailsCount = new Label();
            lblDetailsCount.Font = FontTahoma8Bold;
            //lblDetailsCountForeColor = Color.DarkRed;
            lblDetailsCount.BackColor = Color.Yellow;
            lblDetailsCount.Name = "lblDetailsCount";
            lblDetailsCount.TextAlign = ContentAlignment.TopCenter;
            lblDetailsCount.Location = new Point(CurrWidth < 320 ? 80 : 120, 158);
            lblDetailsCount.Size = new Size(80, 30);
            pnlCurrent.Controls.Add(lblDetailsCount);
            lblDetailsCount.BringToFront();

            //Поле для ввода количества
            TextBox tbCount = new TextBox();
            tbCount.Name = "tbCount";
            tbCount.Location = new Point(130, 140 + 10);
            tbCount.Size = new Size(60, 28);
            tbCount.Font = FontTahoma14Bold;
            tbCount.Text = "";
            pnlCurrent.Controls.Add(tbCount);
            tbCount.Visible = false;

            Label lblKey1 = new Label();
            lblKey1.BackColor = Color.LightGray;
            lblKey1.Font = FontTahoma10Regular;
            lblKey1.Name = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft;
            lblKey1.Location = new Point(4, 160);
            lblKey1.Size = new Size(90, 18);
            lblKey1.Text = SS.DocSet.Special ? "ОСОБЕННАЯ" : "9 - коррект.";
            pnlCurrent.Controls.Add(lblKey1);

            Label lblKey2 = new Label();
            lblKey2.Font = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight;
            lblKey2.Location = new Point(CurrWidth - 96, 160);
            lblKey2.Size = new Size(90, 18);
            lblKey2.Text = "Esc - выход";
            pnlCurrent.Controls.Add(lblKey2);

            PictureBox pbPhoto = new PictureBox();
            pbPhoto.Name = "pbPhoto";
            pbPhoto.Location = new Point(0, 0);
            pbPhoto.Size = new Size(320, 185);
            pbPhoto.Visible = false;
            pnlCurrent.Controls.Add(pbPhoto);

            lblAction.ForeColor = Color.Blue;

            ModeSampleSetReView();
        }   //ModeSampleSetView
        private void ModeControlCollectView()
        {
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgGoodsCC = new DataGrid();
            dgGoodsCC.Location = new Point(2, 0);
            dgGoodsCC.Name = "dgGoodsCC";
            dgGoodsCC.Size = new System.Drawing.Size(CurrWidth - 6, 165);
            dgGoodsCC.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgGoodsCC.DataSource = SS.GoodsCC;
            dgGoodsCC.RowHeadersVisible = false;
            #region Styles
            dgGoodsCC.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№";
            columnStyle.MappingName = "Number";
            columnStyle.Width = CurrWidth < 320 ? 25 : 40;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 54: 76;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Адрес";
            columnStyle.MappingName = "Adress";
            columnStyle.Width = CurrWidth < 320 ? 70: 100;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Кол-во";
            columnStyle.MappingName = "Count";
            columnStyle.Width = CurrWidth < 320 ? 30: 50;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№ исх";
            columnStyle.MappingName = "NumberInDaemond";
            columnStyle.Width = CurrWidth < 320 ? 25 : 40;
            dgts.GridColumnStyles.Add(columnStyle);

            dgGoodsCC.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgGoodsCC);

            lblAction.Text  = "Ожидание команды";
        }
        private void ModeHarmonizationInicializeView()
        {
            
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgFrom = new DataGrid();
            dgFrom.Location = new Point(CurrWidth < 320 ? 47 : 87, 10);
            dgFrom.Name = "dgFrom";
            dgFrom.Size = new System.Drawing.Size(146, 100);
            dgFrom.DataSource = SS.WarehousesFrom;
            dgFrom.RowHeadersVisible = false;
            #region Styles
            dgFrom.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Склад-источник";
            columnStyle.MappingName = "Name";
            columnStyle.Width = CurrWidth < 320 ? 90 : 130;
            dgts.GridColumnStyles.Add(columnStyle);
            dgFrom.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgFrom);

            Label lblFrom = new Label();
            lblFrom.Font      = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma12Bold;
            lblFrom.BackColor = Color.White;
            lblFrom.Name      = "lblFrom";
            lblFrom.TextAlign = ContentAlignment.TopCenter; 
            lblFrom.Location  = new Point(CurrWidth < 320 ? 47 : 87, 115);
            lblFrom.Size      = new Size (146, 25);
            pnlCurrent.Controls.Add(lblFrom);

            Button btnOk = new Button();
            btnOk.Name      = "btnOk";
            btnOk.Text      = "Я согласен";
            btnOk.Location  = new Point(CurrWidth < 320 ? 44 : 84, 150);
            btnOk.Size      = new Size (150, 25);
            btnOk.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnOk);
            lblAction.Text = "Выберите склад!";
            dgFrom.Focus();

        }
        private void ModeHarmonizationView()
        {            
            Screan = 0;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgTI = new DataGrid();
            dgTI.BackgroundColor = Color.White;
            dgTI.Location = new Point(-1*Screan*pnlCurrent.Width + 4, 0);
            dgTI.Name = "dgTransferItems";
            dgTI.Size = new System.Drawing.Size(CurrWidth - 10, 145);
            dgTI.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgTI.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgTI.TableStyles.Clear();
            dgts = new DataGridTableStyle();

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 68 : 88;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Кол-во";
            columnStyle.MappingName = "Count";
            columnStyle.Width = CurrWidth < 320 ? 48 : 65;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Адрес";
            columnStyle.MappingName = "AdressName";
            columnStyle.Width = CurrWidth < 320 ? 90 : 130;
            dgts.GridColumnStyles.Add(columnStyle);

            dgTI.TableStyles.Add(dgts);
            #endregion
            dgTI.DataSource = SS.TransferItems;
            pnlCurrent.Controls.Add(dgTI);

            Label lblItem = new Label();
            lblItem.Font      = CurrWidth < 320 ? FontTahoma7Regular : FontTahoma10Regular;
            lblItem.ForeColor = Color.DarkGreen;
            lblItem.BackColor = Color.White;
            lblItem.Name      = "lblItem";
            lblItem.TextAlign = ContentAlignment.TopLeft;
            lblItem.Location  = new Point(-1*Screan*pnlCurrent.Width + 110, 145);
            lblItem.Size      = new Size (CurrWidth < 320 ? 128 : 208, 20);
            pnlCurrent.Controls.Add(lblItem);

            Label lblAdress = new Label();
            lblAdress.Font      = FontTahoma12Bold;
            lblAdress.ForeColor = Color.DarkRed;
            lblAdress.BackColor = Color.White;
            lblAdress.Name      = "lblAdress";
            lblAdress.TextAlign = ContentAlignment.TopRight;
            lblAdress.Location  = new Point(-1*Screan*pnlCurrent.Width, 145);
            lblAdress.Size      = new Size (110, 20);
            pnlCurrent.Controls.Add(lblAdress);

            //Кнопка распечатать телегу
            Button btnPrint = new Button();
            btnPrint.Name      = "btnPrint";
            btnPrint.Text      = "Завершить";
            btnPrint.Location  = new Point(-1*Screan*pnlCurrent.Width, 165);
            btnPrint.Size      = new Size (150, 20);
            btnPrint.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnPrint);

            lblAction.Text = "Ожидание команды";
            dgTI.Focus();
        }
        private void ModeHarmonizationPutView()
        {            
            Screan = 0;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgTI = new DataGrid();
            dgTI.BackgroundColor = Color.White;
            dgTI.Location = new Point(-1*Screan*pnlCurrent.Width + 4, 0);
            dgTI.Name = "dgTransferItems";
            dgTI.Size = new System.Drawing.Size(CurrWidth - 10, 145);
            dgTI.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgTI.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgTI.TableStyles.Clear();
            dgts = new DataGridTableStyle();

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 68 : 88;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Кол-во";
            columnStyle.MappingName = "Count";
            columnStyle.Width = CurrWidth < 320 ? 48 : 65;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Адрес";
            columnStyle.MappingName = "AdressName";
            columnStyle.Width = CurrWidth < 320 ? 90 : 130;
            dgts.GridColumnStyles.Add(columnStyle);

            dgTI.TableStyles.Add(dgts);
            #endregion
            dgTI.DataSource = SS.TransferItems;
            pnlCurrent.Controls.Add(dgTI);

            Label lblItem = new Label();
            lblItem.Font      = CurrWidth < 320 ? FontTahoma7Regular : FontTahoma10Regular;
            lblItem.ForeColor = Color.DarkGreen;
            lblItem.BackColor = Color.White;
            lblItem.Name      = "lblItem";
            lblItem.TextAlign = ContentAlignment.TopLeft;
            lblItem.Location  = new Point(-1*Screan*pnlCurrent.Width + 110, 145);
            lblItem.Size      = new Size (CurrWidth < 320 ? 128 : 208, 20);
            pnlCurrent.Controls.Add(lblItem);

            Label lblAdress = new Label();
            lblAdress.Font      = FontTahoma12Bold;
            lblAdress.ForeColor = Color.DarkRed;
            lblAdress.BackColor = Color.White;
            lblAdress.Name      = "lblAdress";
            lblAdress.TextAlign = ContentAlignment.TopRight;
            lblAdress.Location  = new Point(-1*Screan*pnlCurrent.Width, 145);
            lblAdress.Size      = new Size (110, 20);
            pnlCurrent.Controls.Add(lblAdress);

            //Кнопка распечатать телегу
            Button btnPrint = new Button();
            btnPrint.Name      = "btnPrint";
            btnPrint.Text      = "Завершить";
            btnPrint.Location  = new Point(-1*Screan*pnlCurrent.Width, 165);
            btnPrint.Size      = new Size (150, 20);
            btnPrint.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnPrint);

            lblAction.Text = "Ожидание команды";
            dgTI.Focus();
        }
        private void ModeLoadingInicializationView()
        {
            Label lblPlacerText = new Label();
            lblPlacerText.Font      = FontTahoma10Bold;
            lblPlacerText.ForeColor = Color.DarkBlue;
            lblPlacerText.BackColor = Color.White;
            lblPlacerText.Name      = "lblPlacerText";
            lblPlacerText.TextAlign = ContentAlignment.TopLeft;
            lblPlacerText.Location  = new Point(10, 20);
            lblPlacerText.Size      = new Size (300, 20);
            lblPlacerText.Text      = "Укладчик:";
            pnlCurrent.Controls.Add(lblPlacerText);

            Label lblPlacer = new Label();
            lblPlacer.Font      = FontTahoma12Bold;
            lblPlacer.BackColor = Color.White;
            lblPlacer.Name      = "lblPlacer";
            lblPlacer.TextAlign = ContentAlignment.TopLeft;
            lblPlacer.Location  = new Point(10, 40);
            lblPlacer.Size      = new Size (300, 20);
            pnlCurrent.Controls.Add(lblPlacer);

            Label lblWayBillText = new Label();
            lblWayBillText.Font      = FontTahoma10Bold;
            lblWayBillText.ForeColor = Color.DarkBlue;
            lblWayBillText.BackColor = Color.White;
            lblWayBillText.Name      = "lblWayBillText";
            lblWayBillText.TextAlign = ContentAlignment.TopLeft;
            lblWayBillText.Location  = new Point(10, 70);
            lblWayBillText.Size      = new Size (300, 20);
            lblWayBillText.Text      = "Путевой лист:";
            pnlCurrent.Controls.Add(lblWayBillText);

            Label lblWayBill = new Label();
            lblWayBill.Font      = FontTahoma12Bold;
            lblWayBill.BackColor = Color.White;
            lblWayBill.Name      = "lblWayBill";
            lblWayBill.TextAlign = ContentAlignment.TopLeft;
            lblWayBill.Location  = new Point(10, 90);
            lblWayBill.Size      = new Size (300, 20);
            pnlCurrent.Controls.Add(lblWayBill);

            //Кнопка зафиксировать выбор укладчика и погрузки
            Button btnPrint = new Button();
            btnPrint.Name      = "btnPrint";
            btnPrint.Text      = "Завершить";
            btnPrint.Location  = new Point(10, 160);
            btnPrint.Size      = new Size (150, 20);
            btnPrint.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnPrint);

            lblAction.Text = "Отсканируйте ШК погрузки и укладчика";
        }
        private void ModeLoadingView()
        {
            ViewProposal = null;
            Screan = 0;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgWayBill = new DataGrid();
            dgWayBill.BackgroundColor = Color.White;
            dgWayBill.Location = new Point(-1*Screan*pnlCurrent.Width + 4, 0);
            dgWayBill.Name = "dgWayBill";
            dgWayBill.Size = new System.Drawing.Size(CurrWidth - 10, 157);
            dgWayBill.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgWayBill.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgWayBill.TableStyles.Clear();
            dgts = new DataGridTableStyle();

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№ Адреса";
            columnStyle.MappingName = "AdressCounter";
            columnStyle.Width = CurrWidth < 320 ? 48 : 68;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Документ";
            columnStyle.MappingName = "ProposalNumber";
            columnStyle.Width = CurrWidth < 320 ? 80 : 110;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Мест";
            columnStyle.MappingName = "Boxes";
            columnStyle.Width = CurrWidth < 320 ? 38 : 55;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Факт";
            columnStyle.MappingName = "BoxesFact";
            columnStyle.Width = CurrWidth < 320 ? 38 : 55;
            dgts.GridColumnStyles.Add(columnStyle);

            dgWayBill.TableStyles.Add(dgts);
            #endregion
            dgWayBill.DataSource = SS.WayBillDT;
            pnlCurrent.Controls.Add(dgWayBill);

            ExPanel pnlInfoNew = new ExPanel();
            pnlInfoNew.BackColor = System.Drawing.SystemColors.ActiveBorder;
            pnlInfoNew.Location = new System.Drawing.Point(-1*Screan*pnlCurrent.Width + pnlCurrent.Width + 3, 20);
            pnlInfoNew.Name = "pnlInfoNew";
            pnlInfoNew.Size = new System.Drawing.Size(CurrWidth - 8, 222);
            pnlCurrent.Controls.Add(pnlInfoNew);

            //Кнопка завершения погрузки
            Button btnPrint = new Button();
            btnPrint.Name      = "btnPrint";
            btnPrint.Text      = "Завершить";
            btnPrint.Location  = new Point(-1*Screan*pnlCurrent.Width + 4, 160);
            btnPrint.Size      = new Size (150, 20);
            btnPrint.Click     += DynamicButtonOnClick;
            pnlCurrent.Controls.Add(btnPrint);

            lblAction.Text = "Ожидание команды";
            dgWayBill.Focus();
        }
        private void ModeSetView()
        {
            Screan = 0;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgGoodsCC = new DataGrid();
            dgGoodsCC.Location = new Point(CurrWidth + 2, 0);
            dgGoodsCC.Name = "dgGoodsCC";
            dgGoodsCC.Size = new System.Drawing.Size(CurrWidth - 6, 165);
            dgGoodsCC.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgGoodsCC.DataSource = SS.CCRP;
            dgGoodsCC.RowHeadersVisible = false;
            #region Styles
            dgGoodsCC.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "№";
            columnStyle.MappingName = "Number";
            columnStyle.Width = CurrWidth < 320 ? 20 : 35;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Адрес";
            columnStyle.MappingName = "Adress";
            columnStyle.Width = CurrWidth < 320 ? 60: 90;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 49: 68;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Кол.";
            columnStyle.MappingName = "Count";
            columnStyle.Width = CurrWidth < 320 ? 30: 40;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Сумма";
            columnStyle.MappingName = "Sum";
            columnStyle.Width = CurrWidth < 320 ? 35: 60;
            dgts.GridColumnStyles.Add(columnStyle);
            //columnStyle.Width = CurrWidth < 320 ? 25 : 40;

            dgGoodsCC.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgGoodsCC);

            Label lblHeaderSum = new Label();
            lblHeaderSum.Font      = FontTahoma8Bold;
            lblHeaderSum.ForeColor = Color.DarkRed;
            lblHeaderSum.Name      = "lblHeaderSum";
            lblHeaderSum.TextAlign = ContentAlignment.TopCenter; 
            lblHeaderSum.Location  = new Point(2*CurrWidth - 120 - 3, 165 + 2);
            lblHeaderSum.Size      = new Size (120, 20);
            pnlCurrent.Controls.Add(lblHeaderSum);

            Label lblHeader = new Label();
            lblHeader.Font      = FontTahoma12Bold;
            lblHeader.Name      = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter; 
            lblHeader.Location  = new Point(4, 0);
            lblHeader.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblHeader);

            Label lblPrevious = new Label();
            lblPrevious.Font      = FontTahoma10Bold;
            lblPrevious.BackColor = Color.LightGray;
            lblPrevious.Name      = "lblPrevious";
            lblPrevious.TextAlign = ContentAlignment.TopCenter; 
            lblPrevious.Location  = new Point(4, 20 + 1);
            lblPrevious.Size      = new Size (CurrWidth - 10, 32);
            pnlCurrent.Controls.Add(lblPrevious);

            Label lblHeaderPrice = new Label();
            lblHeaderPrice.Font      = FontTahoma8Bold;
            lblHeaderPrice.ForeColor = Color.DarkRed;
            lblHeaderPrice.Name      = "lblHeaderPrice";
            lblHeaderPrice.TextAlign = ContentAlignment.TopCenter; 
            lblHeaderPrice.Location  = new Point(0, 50 + 4);
            lblHeaderPrice.Size      = new Size (50, 30);
            pnlCurrent.Controls.Add(lblHeaderPrice);

            Label lblHeaderBalance = new Label();
            lblHeaderBalance.Font      = FontTahoma8Bold;
            lblHeaderBalance.ForeColor = Color.DarkRed;
            lblHeaderBalance.Name      = "lblHeaderBalance";
            lblHeaderBalance.TextAlign = ContentAlignment.TopCenter; 
            lblHeaderBalance.Location  = new Point(CurrWidth - 50, 50 + 4);
            lblHeaderBalance.Size      = new Size (50, 30);
            pnlCurrent.Controls.Add(lblHeaderBalance);

            Label lblAdress = new Label();
            lblAdress.Font      = FontTahoma16Bold;
            lblAdress.Name      = "lblAdress";
            lblAdress.TextAlign = ContentAlignment.TopCenter; 
            lblAdress.Location  = new Point(50, 50 + 4);
            lblAdress.Size      = new Size (CurrWidth - 100, 25);
            pnlCurrent.Controls.Add(lblAdress);

            Label lblInvCode = new Label();
            lblInvCode.Font      = FontTahoma16Bold;
            lblInvCode.Name      = "lblInvCode";
            lblInvCode.TextAlign = ContentAlignment.TopRight;
            lblInvCode.Location  = new Point(4, 75 + 6);
            lblInvCode.Size      = new Size (100, 25 + 6);
            pnlCurrent.Controls.Add(lblInvCode);

            Label lblItem = new Label();
            lblItem.Font      = FontTahoma10Regular;
            lblItem.Name      = "lblItem";
            lblItem.TextAlign = ContentAlignment.TopLeft; 
            lblItem.Location  = new Point(106, 75 + 6 - 2);
            lblItem.Size      = new Size (CurrWidth - 106, 25 + 6);
            pnlCurrent.Controls.Add(lblItem);

            Label lblCount = new Label();
            lblCount.Font      = FontTahoma16Bold;
            lblCount.Name      = "lblCount";
            lblCount.TextAlign = ContentAlignment.TopCenter; 
            lblCount.Location  = new Point(4, 100 + 8 + 2);
            lblCount.Size      = new Size (CurrWidth - 10, 25);
            pnlCurrent.Controls.Add(lblCount);

            Label lblBox = new Label();
            lblBox.Font = FontTahoma12Bold;
            lblBox.Name = "lblBox";
            lblBox.TextAlign = ContentAlignment.TopCenter;
            lblBox.Location = new Point(50, 125 + 10);
            lblBox.Size = new Size(CurrWidth - 100, 20);
            pnlCurrent.Controls.Add(lblBox);

            Label lblDetailsCount = new Label();
            lblDetailsCount.Font      = FontTahoma8Bold;
            //lblDetailsCountForeColor = Color.DarkRed;
            lblDetailsCount.BackColor = Color.Yellow;
            lblDetailsCount.Name      = "lblDetailsCount";
            lblDetailsCount.TextAlign = ContentAlignment.TopCenter; 
            lblDetailsCount.Location  = new Point(CurrWidth < 320 ? 80 : 120, 158);
            lblDetailsCount.Size      = new Size (80, 30);
            pnlCurrent.Controls.Add(lblDetailsCount);
            lblDetailsCount.BringToFront();

            //Поле для ввода количества
            TextBox tbCount = new TextBox();
            tbCount.Name      = "tbCount";
            tbCount.Location  = new Point(130, 140 + 10);
            tbCount.Size      = new Size (60, 28);
            tbCount.Font      = FontTahoma14Bold;
            tbCount.Text      = "";
            pnlCurrent.Controls.Add(tbCount);
            tbCount.Visible = false;

            Label lblKey1 = new Label();
            lblKey1.BackColor = Color.LightGray;
            lblKey1.Font = FontTahoma10Regular;
            lblKey1.Name = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft;
            lblKey1.Location = new Point(4, 160);
            lblKey1.Size = new Size(90, 18);
            lblKey1.Text = SS.DocSet.Special ? "ОСОБЕННАЯ" : "9 - коррект.";
            pnlCurrent.Controls.Add(lblKey1);

            Label lblKey2 = new Label();
            lblKey2.Font      = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name      = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight; 
            lblKey2.Location  = new Point(CurrWidth - 96, 160);
            lblKey2.Size      = new Size (90, 18);
            lblKey2.Text      = "Esc - выход";
            pnlCurrent.Controls.Add(lblKey2);

            PictureBox pbPhoto  = new PictureBox();
            pbPhoto.Name        = "pbPhoto";
            pbPhoto.Location    = new Point(0, 0);
            pbPhoto.Size        = new Size(320, 185);
            pbPhoto.Visible     = false;
            pnlCurrent.Controls.Add(pbPhoto);

            lblAction.ForeColor = Color.Blue;
            ModeSetReView();
        } // ModeSetView
        private void ModeSetInicializationView()
        {
            Label lblAdress = new Label();
            lblAdress.Font = FontTahoma16Bold;
            lblAdress.Name = "lblAdress";
            lblAdress.TextAlign = ContentAlignment.TopCenter;
            lblAdress.Location = new Point(5, 20);
            lblAdress.Size = new Size(CurrWidth - 10, 30);
            if (SS.Const.BoxSetOn)
            {
                lblAdress.Text = SS.BoxForSet == null ? "< Отсканируйте коробку >" : SS.BoxForSet.Name;
            }
            else
            {
                lblAdress.Text = "";
            }
            pnlCurrent.Controls.Add(lblAdress);

            Label lblKey1 = new Label();
            lblKey1.Font      = FontTahoma10Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name      = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft; 
            lblKey1.Location  = new Point(4, 160);
            lblKey1.Size      = new Size (110, 18);
            lblKey1.Text      = "'зеленая' - отбор";
            pnlCurrent.Controls.Add(lblKey1);

            Label lblKey2 = new Label();
            lblKey2.Font      = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name      = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight; 
            lblKey2.Location  = new Point(CurrWidth - 116, 160);
            lblKey2.Size      = new Size (110, 18);
            lblKey2.Text      = "Esc - выход";
            pnlCurrent.Controls.Add(lblKey2);

            DrawlblResult();
            pnlCurrent.GetLabelByName("lblResult").Text = "Для получения задания отбора - нажмите зеленую кнопку";

            lblAction.Text = "Ожидание команды";
        }
        private void ModeSetCompleteView()
        {
            Label lblHeader = new Label();
            lblHeader.Font      = FontTahoma12Bold;
            lblHeader.Name      = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter; 
            lblHeader.Location  = new Point(4, 0);
            lblHeader.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblHeader);

            Label lblPrinter = new Label();
            lblPrinter.Font      = FontTahoma10Bold;
            lblPrinter.ForeColor = Color.DarkGreen;
            lblPrinter.Name      = "lblPrinter";
            lblPrinter.TextAlign = ContentAlignment.TopCenter; 
            lblPrinter.Location  = new Point(4, 20+2);
            lblPrinter.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblPrinter);

            Label lblClient = new Label();
            lblClient.Font      = FontTahoma12Bold;
            lblClient.Name      = "lblClient";
            lblClient.TextAlign = ContentAlignment.TopCenter; 
            lblClient.Location  = new Point(4, 40+4);
            lblClient.Size      = new Size (CurrWidth - 10, 40);
            pnlCurrent.Controls.Add(lblClient);

            Label lblSelfRemovel = new Label();
            lblSelfRemovel.Font      = FontTahoma14Bold;
            lblSelfRemovel.Name      = "lblSelfRemovel";
            lblSelfRemovel.ForeColor = Color.Blue;
            lblSelfRemovel.TextAlign = ContentAlignment.TopCenter; 
            lblSelfRemovel.Location  = new Point(4, 80+4);
            lblSelfRemovel.Size      = new Size (CurrWidth - 10, 24);
            pnlCurrent.Controls.Add(lblSelfRemovel);

            //Поле для ввода количества
            TextBox tbCount = new TextBox();
            tbCount.Name      = "tbCount";
            tbCount.Location  = new Point(120, 140 + 10);
            tbCount.Size      = new Size (60, 28);
            tbCount.Font      = FontTahoma14Bold;
            tbCount.Text      = "";
            tbCount.Visible   = false;
            pnlCurrent.Controls.Add(tbCount);

            DrawlblResult();
            if (SS.EmpbtyBox)
            {
                pnlCurrent.GetLabelByName("lblResult").Text = "Введите количество мест!";
            }
            else
            {
                pnlCurrent.GetLabelByName("lblResult").Text = "Отсканируйте коробку и введите количество мест!";
            }
            lblAction.Text = "Ожидание команды";
        } // ModeSetCompleteView
        private void ModeSetCorrectView()
        {
            Label lblHeader = new Label();
            lblHeader.Font      = FontTahoma12Bold;
            lblHeader.ForeColor = Color.Red;
            lblHeader.Name      = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter; 
            lblHeader.Location  = new Point(4, 0);
            lblHeader.Size      = new Size (CurrWidth - 10, 20);
            lblHeader.Text      = "Корректировка позиции " + SS.CCItem.InvCode;
            pnlCurrent.Controls.Add(lblHeader);

            Label lblAnswer0 = new Label();
            lblAnswer0.Font      = FontTahoma12Bold;
            lblAnswer0.Name      = "lblAnswer0";
            lblAnswer0.TextAlign = ContentAlignment.TopLeft; 
            lblAnswer0.Location  = new Point(100, 20+2);
            lblAnswer0.Size      = new Size (CurrWidth - 50, 20);
            lblAnswer0.Text      = "0 - назад (отмена)";
            pnlCurrent.Controls.Add(lblAnswer0);

            Label lblAnswer1 = new Label();
            lblAnswer1.Font      = FontTahoma12Bold;
            lblAnswer1.Name      = "lblAnswer1";
            lblAnswer1.TextAlign = ContentAlignment.TopLeft; 
            lblAnswer1.Location  = new Point(100, 40+4);
            lblAnswer1.Size      = new Size (CurrWidth - 50, 20);
            lblAnswer1.Text      = "1 - брак";
            pnlCurrent.Controls.Add(lblAnswer1);

            Label lblAnswer2 = new Label();
            lblAnswer2.Font      = FontTahoma12Bold;
            lblAnswer2.Name      = "lblAnswer2";
            lblAnswer2.TextAlign = ContentAlignment.TopLeft; 
            lblAnswer2.Location  = new Point(100, 60+6);
            lblAnswer2.Size      = new Size (CurrWidth - 50, 20);
            lblAnswer2.Text      = "2 - недостача";
            pnlCurrent.Controls.Add(lblAnswer2);

            

            //Поле для ввода количества
            TextBox tbCount = new TextBox();
            tbCount.Name      = "tbCount";
            tbCount.Location  = new Point(120, 120 + 12);
            tbCount.Size      = new Size (60, 28);
            tbCount.Font      = FontTahoma14Bold;
            tbCount.Text      = "";
            pnlCurrent.Controls.Add(tbCount);
            tbCount.Visible = false;

            Label lblKey1 = new Label();
            lblKey1.Font      = FontTahoma10Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name      = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft; 
            lblKey1.Location  = new Point(4, 160);
            lblKey1.Size      = new Size (110, 18);
            lblKey1.Text      = "'зеленая' - отбор";
            pnlCurrent.Controls.Add(lblKey1);
            lblKey1.Visible = false;

            Label lblKey2 = new Label();
            lblKey2.Font      = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name      = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight; 
            lblKey2.Location  = new Point(CurrWidth - 116, 160);
            lblKey2.Size      = new Size (110, 18);
            lblKey2.Text      = "0, Esc - назад";
            pnlCurrent.Controls.Add(lblKey2);

            lblAction.ForeColor = Color.Blue;
            lblAction.Text = SS.ExcStr;
            ChoiseCorrect = 0;
        } // ModeSetCorrectView
        private void ModeSampleSetCorrectView()
        {
            Label lblHeader = new Label();
            lblHeader.Font = FontTahoma12Bold;
            lblHeader.ForeColor = Color.Red;
            lblHeader.Name = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter;
            lblHeader.Location = new Point(4, 0);
            lblHeader.Size = new Size(CurrWidth - 10, 20);
            lblHeader.Text = "Корректировка позиции " + SS.CCItem.InvCode;
            pnlCurrent.Controls.Add(lblHeader);

            Label lblAnswer0 = new Label();
            lblAnswer0.Font = FontTahoma12Bold;
            lblAnswer0.Name = "lblAnswer0";
            lblAnswer0.TextAlign = ContentAlignment.TopLeft;
            lblAnswer0.Location = new Point(100, 20 + 2);
            lblAnswer0.Size = new Size(CurrWidth - 50, 20);
            lblAnswer0.Text = "0 - назад (отмена)";
            pnlCurrent.Controls.Add(lblAnswer0);

            Label lblAnswer1 = new Label();
            lblAnswer1.Font = FontTahoma12Bold;
            lblAnswer1.Name = "lblAnswer1";
            lblAnswer1.TextAlign = ContentAlignment.TopLeft;
            lblAnswer1.Location = new Point(100, 40 + 4);
            lblAnswer1.Size = new Size(CurrWidth - 50, 20);
            lblAnswer1.Text = "1 - брак";
            pnlCurrent.Controls.Add(lblAnswer1);

            Label lblAnswer2 = new Label();
            lblAnswer2.Font = FontTahoma12Bold;
            lblAnswer2.Name = "lblAnswer2";
            lblAnswer2.TextAlign = ContentAlignment.TopLeft;
            lblAnswer2.Location = new Point(100, 60 + 6);
            lblAnswer2.Size = new Size(CurrWidth - 50, 20);
            lblAnswer2.Text = "2 - недостача";
            pnlCurrent.Controls.Add(lblAnswer2);

            //Поле для ввода количества
            TextBox tbCount = new TextBox();
            tbCount.Name = "tbCount";
            tbCount.Location = new Point(120, 120 + 12);
            tbCount.Size = new Size(60, 28);
            tbCount.Font = FontTahoma14Bold;
            tbCount.Text = "";
            pnlCurrent.Controls.Add(tbCount);
            tbCount.Visible = false;

            Label lblKey1 = new Label();
            lblKey1.Font = FontTahoma10Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft;
            lblKey1.Location = new Point(4, 160);
            lblKey1.Size = new Size(110, 18);
            lblKey1.Text = "'зеленая' - отбор";
            pnlCurrent.Controls.Add(lblKey1);
            lblKey1.Visible = false;

            Label lblKey2 = new Label();
            lblKey2.Font = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight;
            lblKey2.Location = new Point(CurrWidth - 116, 160);
            lblKey2.Size = new Size(110, 18);
            lblKey2.Text = "0, Esc - назад";
            pnlCurrent.Controls.Add(lblKey2);

            lblAction.ForeColor = Color.Blue;
            lblAction.Text = SS.ExcStr;
            ChoiseCorrect = 0;


        } // ModeSampleSetCorrectView
        private void ModeChoiseDownView()
        {
            lblState.Text = "Спуск выбор (" + (SS.Const.CarsCount == 0 ? "нет ограничений" : SS.Const.CarsCount.ToString() + " авто") + ")";

            lblAction.ForeColor = Color.Blue;
            pnlCurrent.Controls.Clear();
            pnlCurrent.StaticControl.Clear();

            for (int i = 0; i < 7; i++)
            {
                if (SS.DownSituation.Rows.Count <= i)
                {
                    break;
                }
                string txt1 =
                    (i + 1).ToString() + ". " + SS.DownSituation.Rows[i]["Sector"].ToString().Trim() +
                    " - " + SS.DownSituation.Rows[i]["CountBox"].ToString();
                string txt2 =
                    " мест " + SS.DownSituation.Rows[i]["CountEmployers"].ToString().Trim() + " сотр.";

                bool Allowed = (int)SS.DownSituation.Rows[i]["Allowed"] == 1 ? true : false;

                Label lblString1 = new Label();
                lblString1.Font      = FontTahoma14Bold;
                lblString1.ForeColor = Allowed ? Color.Black : Color.Silver;
                lblString1.Name      = "lblString" + i.ToString();
                lblString1.TextAlign = ContentAlignment.TopLeft; 
                lblString1.Location  = new Point(CurrWidth < 320 ? 0 : 40, 18*i);
                lblString1.Size      = new Size (CurrWidth - (CurrWidth < 320 ? 135 : 215), 21);
                lblString1.Text      = txt1;
                pnlCurrent.Controls.Add(lblString1);

                Label lblString2 = new Label();
                lblString2.Font      = FontTahoma14Bold;
                lblString2.ForeColor = Allowed ? Color.Black : Color.Silver;
                lblString2.Name      = "lblStringP" + i.ToString();
                lblString2.TextAlign = ContentAlignment.TopLeft; 
                lblString2.Location  = new Point(40 + 105, 18*i);
                lblString2.Size      = new Size (CurrWidth - (CurrWidth < 320 ? 105 : 185), 21);
                lblString2.Text      = txt2;
                pnlCurrent.Controls.Add(lblString2);
            }

            Label lblString8 = new Label();
            lblString8.Font      = FontTahoma14Bold;
            lblString8.ForeColor = SS.Employer.CanComplectation ? Color.Black : Color.Silver;
            lblString8.Name      = "lblString8";
            lblString8.TextAlign = ContentAlignment.TopLeft; 
            lblString8.Location  = new Point(CurrWidth < 320 ? 0 : 40, 18*7);
            lblString8.Size      = new Size (CurrWidth < 320 ? 160 : 240, 21);
            lblString8.Text      = "8. КМ: " + SS.PreviousAction;
            pnlCurrent.Controls.Add(lblString8);

            Label lblString9 = new Label();
            lblString9.Font      = FontTahoma14Bold;
            lblString9.ForeColor = Color.Silver;
            lblString9.Name      = "lblString9";
            lblString9.TextAlign = ContentAlignment.TopLeft; 
            lblString9.Location  = new Point(CurrWidth < 320 ? 0 : 40, 18*8);
            lblString9.Size      = new Size (CurrWidth < 320 ? 160 : 240, 21);
            lblString9.Text      = "9. <reserved>";
            pnlCurrent.Controls.Add(lblString9);

            Label lblKey1 = new Label();
            lblKey1.Font      = FontTahoma8Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name      = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft; 
            lblKey1.Location  = new Point(4, 165);
            lblKey1.Size      = new Size (110, 18);
            lblKey1.Text      = "'зеленая' - обновить";
            pnlCurrent.Controls.Add(lblKey1);

            Label lblKey2 = new Label();
            lblKey2.Font      = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name      = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight; 
            lblKey2.Location  = new Point(CurrWidth - 116, 165);
            lblKey2.Size      = new Size (110, 18);
            lblKey2.Text      = "0, Esc - назад";
            pnlCurrent.Controls.Add(lblKey2);

            lblAction.Text = SS.ExcStr;
        }
        private void ModeDownView()
        {
            TimerFind.Interval  = 2000;
            TimerFind.Enabled   = false;

            Text = "WPM " + Vers + " (" + Helper.GetShortFIO(SS.Employer.Name) + ")";
            Label lblPrinter = new Label();
            lblPrinter.Font      = FontTahoma10Bold;
            lblPrinter.ForeColor = Color.DarkGreen;
            lblPrinter.Name      = "lblPrinter";
            lblPrinter.TextAlign = ContentAlignment.TopCenter; 
            lblPrinter.Location  = new Point(4, 0);
            lblPrinter.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblPrinter);

            Label lblInfo1 = new Label();
            lblInfo1.Font      = FontTahoma14Bold;
            //lblInfo1.ForeColor = Color.DarkGreen;
            lblInfo1.Name      = "lblInfo1";
            lblInfo1.TextAlign = ContentAlignment.TopCenter; 
            lblInfo1.Location  = new Point(4, 24);
            lblInfo1.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblInfo1);

            Label lblNumber = new Label();
            lblNumber.Font      = FontTahoma18Bold;
            lblNumber.ForeColor = Color.Blue;
            lblNumber.Name      = "lblNumber";
            lblNumber.TextAlign = ContentAlignment.TopCenter; 
            lblNumber.Location  = new Point(4, 58);
            lblNumber.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblNumber);

            Label lblAdress = new Label();
            lblAdress.Font      = FontTahoma16Bold;
            lblAdress.Name      = "lblAdress";
            lblAdress.TextAlign = ContentAlignment.TopCenter; 
            lblAdress.Location  = new Point(4, 92);
            lblAdress.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblAdress);

            Label lblSetter = new Label();
            lblSetter.Font      = FontTahoma12Regular;
            lblSetter.Name      = "lblSetter";
            lblSetter.TextAlign = ContentAlignment.TopCenter; 
            lblSetter.Location  = new Point(4, 126);
            lblSetter.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblSetter);

            Label lblKey1 = new Label();
            lblKey1.Font      = FontTahoma10Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name      = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft;
            lblKey1.Location  = new Point(4, 160);
            lblKey1.Size      = new Size (110, 18);
            lblKey1.Text      = "'зеленая' - все";
            pnlCurrent.Controls.Add(lblKey1);
            lblKey1.Visible = false;

            Label lblKey2 = new Label();
            lblKey2.Font      = FontTahoma10Bold;
            lblKey2.ForeColor   = Color.White;
            lblKey2.BackColor   = Color.Red;
            lblKey2.Name      = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight; 
            lblKey2.Location  = new Point(CurrWidth - 116, 160);
            lblKey2.Size      = new Size (110, 18);
            lblKey2.Text      = "Esc - ОТКАЗ";
            pnlCurrent.Controls.Add(lblKey2);

            lblAction.Text = SS.ExcStr;
        }
        private void ModeDownCompleteView()
        {
            Text = "WPM " + Vers + " (" + Helper.GetShortFIO(SS.Employer.Name) + ")";
            Label lblPrinter = new Label();
            lblPrinter.Font      = FontTahoma10Bold;
            lblPrinter.ForeColor = Color.DarkGreen;
            lblPrinter.Name      = "lblPrinter";
            lblPrinter.TextAlign = ContentAlignment.TopCenter; 
            lblPrinter.Location  = new Point(4, 0);
            lblPrinter.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblPrinter);

            Label lblInfo1 = new Label();
            lblInfo1.Font      = FontTahoma14Bold;
            //lblInfo1.ForeColor = Color.DarkGreen;
            lblInfo1.Text      = "Всего " + SS.DownSituation.Rows[0]["AllBox"].ToString() + " мест";
            lblInfo1.Name      = "lblInfo1";
            lblInfo1.TextAlign = ContentAlignment.TopCenter; 
            lblInfo1.Location  = new Point(4, 24);
            lblInfo1.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblInfo1);

            Label lblNumber = new Label();
            lblNumber.Font      = FontTahoma20Bold;
            lblNumber.ForeColor = Color.Red;
            lblNumber.Name      = "lblNumber";
            lblNumber.TextAlign = ContentAlignment.TopCenter; 
            lblNumber.Location  = new Point(4, 60);
            lblNumber.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblNumber);

            Label lblKey1 = new Label();
            lblKey1.Font      = FontTahoma10Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name      = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft; 
            lblKey1.Location  = new Point(4, 160);
            lblKey1.Size      = new Size (110, 18);
            lblKey1.Text      = "'зеленая' - печать";
            pnlCurrent.Controls.Add(lblKey1);

            Label lblKey2 = new Label();
            lblKey2.Font      = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name      = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight; 
            lblKey2.Location  = new Point(CurrWidth - 116, 160);
            lblKey2.Size      = new Size (110, 18);
            lblKey2.Text      = "Esc - выход";
            pnlCurrent.Controls.Add(lblKey2);

            lblAction.Text = SS.ExcStr;
        }

        private void ModeFreeDownCompleteView()
        {
            // pnlNewInfo
            ExPanel pnlNewInfo = new ExPanel();
            pnlNewInfo.BackColor = System.Drawing.SystemColors.ActiveBorder;
            pnlNewInfo.Location = new System.Drawing.Point(-1*CurrWidth + 3, 20);
            pnlNewInfo.Name = "pnlNewInfo";
            pnlNewInfo.Size = new System.Drawing.Size(CurrWidth - 8, 260);
            pnlCurrent.Controls.Add(pnlNewInfo);

            Label lblDocInfo = new Label();
            lblDocInfo.Font      = FontTahoma10Bold;
            lblDocInfo.Name      = "lblDocInfo";
            lblDocInfo.TextAlign = ContentAlignment.TopCenter; 
            lblDocInfo.Location  = new Point(-1*CurrWidth + 4, 0);
            lblDocInfo.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblDocInfo);

            Screan = 0;
            lblState.Text = "Свободный спуск или комплектация";

            Label lblInfo1 = new Label();
            lblInfo1.Font      = FontTahoma14Bold;
            lblInfo1.Name      = "lblInfo1";
            lblInfo1.TextAlign = ContentAlignment.TopCenter; 
            lblInfo1.Location  = new Point(4, 24);
            lblInfo1.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblInfo1);

            Label lblInfo2 = new Label();
            lblInfo2.Font      = FontTahoma12Regular;
            lblInfo2.Name      = "lblInfo1";
            lblInfo2.TextAlign = ContentAlignment.TopCenter; 
            lblInfo2.Location  = new Point(4, 58);
            lblInfo2.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblInfo2);
            if (SS.DocDown.ID != null)
            {
                lblInfo1.Text = SS.DocDown.NumberBill.Substring(SS.DocDown.NumberBill.Length - 5, 2) + " "
                                    + SS.DocDown.NumberBill.Substring(SS.DocDown.NumberBill.Length - 3)
                                    + " сектор: " + SS.DocDown.MainSectorName.Trim() + "-" + SS.DocDown.NumberCC.ToString();
                lblInfo2.Text = " место № " + SS.DocDown.Boxes.ToString();
            }

            Label lblKey2 = new Label();
            lblKey2.Font      = FontTahoma10Bold;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name      = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight; 
            lblKey2.Location  = new Point(CurrWidth - 116, 160);
            lblKey2.Size      = new Size (110, 18);
            lblKey2.Text      = "Esc - выход";
            pnlCurrent.Controls.Add(lblKey2);

            lblAction.Text = SS.ExcStr;
        }
        private void ModeNewComplectation()
        {
            // pnlNewInfo
            ExPanel pnlNewInfo = new ExPanel();
            pnlNewInfo.BackColor = System.Drawing.SystemColors.ActiveBorder;
            pnlNewInfo.Location = new System.Drawing.Point(-1*CurrWidth + 3, 20);
            pnlNewInfo.Name = "pnlNewInfo";
            pnlNewInfo.Size = new System.Drawing.Size(CurrWidth - 8, 260);
            pnlCurrent.Controls.Add(pnlNewInfo);

            Label lblDocInfo = new Label();
            lblDocInfo.Font      = FontTahoma10Bold;
            lblDocInfo.Name      = "lblDocInfo";
            lblDocInfo.TextAlign = ContentAlignment.TopCenter; 
            lblDocInfo.Location  = new Point(-1*CurrWidth + 4, 0);
            lblDocInfo.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblDocInfo);

            Screan = 0;
            lblState.Text = "Комплектация в тележку (новая)";
            TimerFind.Interval  = 2000;
            TimerFind.Enabled   = false;

            Label lblInfo1 = new Label();
            lblInfo1.Font      = FontTahoma14Bold;
            //lblInfo1.ForeColor = Color.DarkGreen;
            lblInfo1.Name      = "lblInfo1";
            lblInfo1.TextAlign = ContentAlignment.TopCenter; 
            lblInfo1.Location  = new Point(4, 24);
            lblInfo1.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblInfo1);

            Label lblNumber = new Label();
            lblNumber.Font      = FontTahoma18Bold;
            lblNumber.ForeColor = Color.Blue;
            lblNumber.Name      = "lblNumber";
            lblNumber.TextAlign = ContentAlignment.TopCenter; 
            lblNumber.Location  = new Point(4, 58);
            lblNumber.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblNumber);

            Label lblAdress = new Label();
            lblAdress.Font      = FontTahoma16Bold;
            lblAdress.Name      = "lblAdress";
            lblAdress.TextAlign = ContentAlignment.TopCenter; 
            lblAdress.Location  = new Point(4, 92);
            lblAdress.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblAdress);

            Label lblSetter = new Label();
            lblSetter.Font      = FontTahoma12Regular;
            lblSetter.Name      = "lblSetter";
            lblSetter.TextAlign = ContentAlignment.TopCenter; 
            lblSetter.Location  = new Point(4, 126);
            lblSetter.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblSetter);

            Label lblKey1 = new Label();
            lblKey1.Font      = FontTahoma10Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name      = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft;
            lblKey1.Location  = new Point(4, 160);
            lblKey1.Size      = new Size (110, 18);
            lblKey1.Text      = "'зеленая' - все";
            pnlCurrent.Controls.Add(lblKey1);
            lblKey1.Visible = false;

            Label lblKey2 = new Label();
            lblKey2.Font      = FontTahoma10Bold;
            lblKey2.ForeColor   = Color.White;
            lblKey2.BackColor   = Color.Red;
            lblKey2.Name      = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight; 
            lblKey2.Location  = new Point(CurrWidth - 116, 160);
            lblKey2.Size      = new Size (110, 18);
            lblKey2.Text      = "Esc - ОТКАЗ";
            pnlCurrent.Controls.Add(lblKey2);

            lblAction.Text = SS.ExcStr;
        }
        private void ModeNewComplectationCompleteView()
        {
            // pnlNewInfo
            ExPanel pnlNewInfo = new ExPanel();
            pnlNewInfo.BackColor = System.Drawing.SystemColors.ActiveBorder;
            pnlNewInfo.Location = new System.Drawing.Point(-1*CurrWidth + 3, 20);
            pnlNewInfo.Name = "pnlNewInfo";
            pnlNewInfo.Size = new System.Drawing.Size(CurrWidth - 8, 260);
            pnlCurrent.Controls.Add(pnlNewInfo);

            Label lblDocInfo = new Label();
            lblDocInfo.Font      = FontTahoma10Bold;
            lblDocInfo.Name      = "lblDocInfo";
            lblDocInfo.TextAlign = ContentAlignment.TopCenter; 
            lblDocInfo.Location  = new Point(-1*CurrWidth + 4, 0);
            lblDocInfo.Size      = new Size (CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblDocInfo);

            Screan = 0;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgRoute = new DataGrid();
            dgRoute.Location = new Point(CurrWidth + 2, 0);
            dgRoute.Name = "dgRoute";
            dgRoute.Size = new System.Drawing.Size(CurrWidth - 6, 180);
            dgRoute.Font = FontTahoma14Regular;
            dgRoute.DataSource = SS.CCRP;
            dgRoute.RowHeadersVisible = false;
            #region Styles
            dgRoute.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Вр";
            columnStyle.MappingName = "Gate";
            columnStyle.Width = CurrWidth < 320 ? 30 : 30;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Звк";
            columnStyle.MappingName = "Bill";
            columnStyle.Width = CurrWidth < 320 ? 55 : 55;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Лст";
            columnStyle.MappingName = "CC";
            columnStyle.Width = CurrWidth < 320 ? 55: 55;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "М";
            columnStyle.MappingName = "Boxes";
            columnStyle.Width = CurrWidth < 320 ? 35: 35;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Адрес";
            columnStyle.MappingName = "Adress";
            columnStyle.Width = CurrWidth < 320 ? 105: 105;
            dgts.GridColumnStyles.Add(columnStyle);

            dgRoute.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgRoute);

            Text = "WPM " + Vers + " (" + Helper.GetShortFIO(SS.Employer.Name) + ")";
            lblState.Text = "Комплектация в адрес (новая)";
            TimerFind.Interval  = 20000;    //20 second
            TimerFind.Enabled   = false;

            Label lblInfo1 = new Label();
            lblInfo1.Font      = FontTahoma14Bold;
            //lblInfo1.ForeColor = Color.DarkGreen;
            lblInfo1.Text      = "Всего " + SS.DownSituation.Rows[0]["AllBox"].ToString() + " мест";
            lblInfo1.Name      = "lblInfo1";
            lblInfo1.TextAlign = ContentAlignment.TopCenter; 
            lblInfo1.Location  = new Point(4, 24);
            lblInfo1.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblInfo1);

            Label lblNumber = new Label();
            lblNumber.Font      = FontTahoma20Bold;
            lblNumber.ForeColor = Color.Red;
            lblNumber.Name      = "lblNumber";
            lblNumber.TextAlign = ContentAlignment.TopCenter; 
            lblNumber.Location  = new Point(4, 60);
            lblNumber.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblNumber);

            Label lblInfo2 = new Label();
            lblInfo2.Font      = FontTahoma14Bold;
            //lblInfo1.ForeColor = Color.DarkGreen;
            lblInfo2.Name      = "lblInfo2";
            lblInfo2.TextAlign = ContentAlignment.TopCenter; 
            lblInfo2.Location  = new Point(4, 94);
            lblInfo2.Size      = new Size (CurrWidth - 10, 26);
            pnlCurrent.Controls.Add(lblInfo2);

            Label lblSetter = new Label();
            lblSetter.Font      = FontTahoma14Bold;
            lblSetter.Name      = "lblSetter";
            lblSetter.TextAlign = ContentAlignment.TopCenter; 
            lblSetter.Location  = new Point(4, 126);
            lblSetter.Size      = new Size (CurrWidth - 10, 30);
            pnlCurrent.Controls.Add(lblSetter);

            Label lblKey1 = new Label();
            lblKey1.Font      = FontTahoma10Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name      = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft; 
            lblKey1.Location  = new Point(4, 160);
            lblKey1.Size      = new Size (110, 18);
            lblKey1.Text      = "'зеленая' - все";
            pnlCurrent.Controls.Add(lblKey1);
            lblKey1.Visible   = false;

            Label lblKey2 = new Label();
            lblKey2.Font      = FontTahoma10Bold;
            lblKey2.ForeColor   = Color.White;
            lblKey2.BackColor   = Color.Red;
            lblKey2.Name      = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight; 
            lblKey2.Location  = new Point(CurrWidth - 116, 160);
            lblKey2.Size      = new Size (110, 18);
            lblKey2.Text      = "Esc - ПОЛОН";
            pnlCurrent.Controls.Add(lblKey2);
            lblKey2.Visible = false;

            lblAction.Text = SS.ExcStr;
        }

        private void ModeSetSelfControlView()
        {
            Screan = 0;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgGoodsCC = new DataGrid();
            dgGoodsCC.Location = new Point(CurrWidth + 2, 0);
            dgGoodsCC.Name = "dgGoodsCC";
            dgGoodsCC.Size = new System.Drawing.Size(CurrWidth - 6, 165);
            dgGoodsCC.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgGoodsCC.DataSource = SS.TableSet;
            dgGoodsCC.RowHeadersVisible = false;
            #region Styles
            dgGoodsCC.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            //columnStyle = new DataGridTextBoxColumn();
            //columnStyle.HeaderText = "№";
            //columnStyle.MappingName = "Number";
            //columnStyle.Width = CurrWidth < 320 ? 20 : 35;
            //dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = 70;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Товар";
            columnStyle.MappingName = "Descr";
            columnStyle.Width = 220;
            dgts.GridColumnStyles.Add(columnStyle);
            //columnStyle = new DataGridTextBoxColumn();
            //columnStyle.HeaderText = "Кол.";
            //columnStyle.MappingName = "Count";
            //columnStyle.Width = CurrWidth < 320 ? 30 : 40;
            //dgts.GridColumnStyles.Add(columnStyle);

            dgGoodsCC.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgGoodsCC);

            Label lblHeader = new Label();
            lblHeader.Font = FontTahoma12Bold;
            lblHeader.Name = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter;
            lblHeader.Location = new Point(4, 0);
            lblHeader.Size = new Size(CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblHeader);

            Label lblAdress = new Label();
            lblAdress.Font = FontTahoma16Bold;
            lblAdress.ForeColor = Color.Red;
            lblAdress.Name = "lblAdress";
            lblAdress.TextAlign = ContentAlignment.TopCenter;
            lblAdress.Location = new Point(50, 50 + 4);
            lblAdress.Size = new Size(CurrWidth - 100, 30);
            lblAdress.Text = "РЕЖИМ КОНТРОЛЯ";
            pnlCurrent.Controls.Add(lblAdress);

            Label lblPrevious = new Label();
            lblPrevious.Font = FontTahoma10Bold;
            lblPrevious.BackColor = Color.LightGray;
            lblPrevious.Name = "lblPrevious";
            lblPrevious.TextAlign = ContentAlignment.TopCenter;
            lblPrevious.Location = new Point(4, 20 + 1);
            lblPrevious.Size = new Size(CurrWidth - 10, 32);
            pnlCurrent.Controls.Add(lblPrevious);

            Label lblHeaderPrice = new Label();
            lblHeaderPrice.Font = FontTahoma8Bold;
            lblHeaderPrice.ForeColor = Color.DarkRed;
            lblHeaderPrice.Name = "lblHeaderPrice";
            lblHeaderPrice.TextAlign = ContentAlignment.TopCenter;
            lblHeaderPrice.Location = new Point(0, 50 + 4);
            lblHeaderPrice.Size = new Size(50, 30);
            pnlCurrent.Controls.Add(lblHeaderPrice);

            Label lblInvCode = new Label();
            lblInvCode.Font = FontTahoma16Bold;
            lblInvCode.Name = "lblInvCode";
            lblInvCode.TextAlign = ContentAlignment.TopRight;
            lblInvCode.Location = new Point(4, 80 + 6);
            lblInvCode.Size = new Size(100, 30);
            pnlCurrent.Controls.Add(lblInvCode);

            Label lblItem = new Label();
            lblItem.Font = FontTahoma10Regular;
            lblItem.Name = "lblItem";
            lblItem.TextAlign = ContentAlignment.TopLeft;
            lblItem.Location = new Point(106, 80 + 6);
            lblItem.Size = new Size(CurrWidth - 106, 30);
            pnlCurrent.Controls.Add(lblItem);

            Label lblDetailsCount = new Label();
            lblDetailsCount.Font = FontTahoma8Bold;
            lblDetailsCount.BackColor = Color.Yellow;
            lblDetailsCount.Name = "lblDetailsCount";
            lblDetailsCount.TextAlign = ContentAlignment.TopCenter;
            lblDetailsCount.Location = new Point(CurrWidth < 320 ? 90 : 130, 148);
            lblDetailsCount.Size = new Size(60, 30);
            pnlCurrent.Controls.Add(lblDetailsCount);
            lblDetailsCount.BringToFront();

            //Поле для ввода количества
            TextBox tbCount = new TextBox();
            tbCount.Name = "tbCount";
            tbCount.Location = new Point(120, 140 + 10);
            tbCount.Size = new Size(60, 28);
            tbCount.Font = FontTahoma14Bold;
            tbCount.Text = "";
            pnlCurrent.Controls.Add(tbCount);
            tbCount.Visible = false;

            Label lblKey2 = new Label();
            lblKey2.Font = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight;
            lblKey2.Location = new Point(CurrWidth - 96, 160);
            lblKey2.Size = new Size(90, 18);
            lblKey2.Text = "Esc - выход";
            pnlCurrent.Controls.Add(lblKey2);
        }

        private void ShowInfoNewComp()
        {
            
            Label lblDocInfo = pnlCurrent.GetLabelByName("lblDocInfo");
            if (SS.BadDoc.View != lblDocInfo.Text)
            {
                SS.LoadCC();
            }
            lblDocInfo.Text = SS.BadDoc.View;

            ExPanel pnlNewInfo = (pnlCurrent.GetControlByName("pnlNewInfo") as ExPanel);
            pnlNewInfo.Controls.Clear();
            const int FieldOnRow = 7; //Число полей на одну строку одного сектора
            //ExPanel pnlInfo = new P
            Label[,] Lbl = new Label[FieldOnRow, SS.CCRP.Rows.Count];
            int i = 0;
            int dY;                 //Это смещение, м/у строками
            int delta = -1;              //Где наш сборочный начинается?
            DataRow[] DR = SS.CCRP.Select();
            foreach (DataRow row in DR)
            {
                dY  = i*30;
                //Создадим метки для этого сектора
                for (int j = 0; j < FieldOnRow; j++)
                {
                    Lbl[j, i]           = new Label();
                    Lbl[j, i].Name      = "_dyn" + j.ToString() + i.ToString();
                    Lbl[j, i].BackColor = Color.White;
                    pnlNewInfo.Controls.Add(Lbl[j, i]);
                }
                if (row["iddoc"].ToString() == SS.BadDoc.ID)
                {
                    if (delta == -1)
                    {
                        delta = i;
                    }
                    Lbl[0, i].BackColor = Color.LightGray;
                }
                //ОПРЕДЕЛИМСЯ С СОДЕРЖАНИЕМ И ЦВЕТАМИ ИСХОДЯ ИЗ СОСТОЯНИЯ
                //Ниже используются запросы к БД, не очень красиво, но в целях оптимизации (т.к. заглядывают сюда не всякий раз)
                //Поэтому, если что-то не срастается, то выводим пустые поля, там где несраслось
                if (SQL1S.IsVoidDate((DateTime)row["date1"]))
                {
                    //ОЖИДАНИЕ НАБОРА
                    Lbl[6, i].Text = ".";
                    Lbl[2, i].Text = "<< ожидание >>";
                    Lbl[4, i].Text = "__:__";
                    //Цвета
                    Lbl[0, i].ForeColor = Color.Red;
                    Lbl[1, i].ForeColor = Color.Red;
                    Lbl[2, i].ForeColor = Color.LightPink;
                    Lbl[4, i].ForeColor = Color.LightGray;
                    Lbl[6, i].ForeColor = Color.Red;
                }
                else if (SQL1S.IsVoidDate((DateTime)row["date2"]))
                {
                    //В НАБОРЕ
                    Lbl[6, i].Text = "...";
                    if (row["employer"] == null)
                    {
                        Lbl[2,i].Text = "< не задан >";
                    }
                    else
                    {
                        Lbl[2,i].Text = Helper.GetShortFIO(row["employer"].ToString());
                    }
                    Lbl[4, i].Text = "(" + ((DateTime)row["date1"]).ToString("dd.MM") + ") "
                                     + Helper.SecondToString((int)row["time1"]) + " - ...";
                    //Цвета
                    Lbl[0, i].ForeColor = Color.Red;
                    Lbl[1, i].ForeColor = Color.Red;
                    Lbl[2, i].ForeColor = Color.Red;
                    Lbl[4, i].ForeColor = Color.Red;
                    Lbl[6, i].ForeColor = Color.Red;

                }
                else
                {
                    //НАБРАНА
                    Lbl[6, i].Text = "--";
                    if (row["employer"] == null)
                    {
                        Lbl[2,i].Text = "< не задан >";
                    }
                    else
                    {
                        Lbl[2,i].Text = Helper.GetShortFIO(row["employer"].ToString());
                    }
                    Lbl[4, i].Text = "(" + ((DateTime)row["date2"]).ToString("dd.MM") + ") "
                                     + Helper.SecondToString((int)row["time1"]) + " - "
                                     + Helper.SecondToString((int)row["time2"]);
                    //Цвета
                    Lbl[0, i].ForeColor = Color.Black;
                    Lbl[1, i].ForeColor = Color.Black;
                    Lbl[2, i].ForeColor = Color.Black;
                    Lbl[4, i].ForeColor = Color.Black;
                    Lbl[6, i].ForeColor = Color.Black;
                }

                
                //Заголовок
                Lbl[0,i].Font       = FontTahoma14Bold;
                Lbl[0,i].TextAlign  = ContentAlignment.TopCenter;
                Lbl[0,i].Location   = new Point(2, 3 + dY);
                Lbl[0,i].Size       = new Size(50, 27);
                Lbl[0,i].Text       = row["sector"].ToString() + "-" + row["number"].ToString();
                //Состояние
                Lbl[1,i].Font       = FontTahoma14Bold;
                Lbl[1,i].TextAlign  = ContentAlignment.TopCenter;
                Lbl[1,i].Location   = new Point(54, 3 + dY);
                Lbl[1,i].Size       = new Size(24, 27);
                Lbl[1,i].Text       = row["cond"].ToString();
                //Сотрудник
                Lbl[2,i].Font       = FontTahoma9Regular;
                Lbl[2,i].TextAlign  = ContentAlignment.TopLeft;
                Lbl[2,i].Location   = new Point(80, 3 + dY);
                Lbl[2,i].Size       = new Size(112, 13);
                //Адрес
                Lbl[3,i].Font       = FontTahoma9Regular;
                Lbl[3,i].TextAlign  = ContentAlignment.TopLeft;
                Lbl[3,i].Location   = new Point(194, 3 + dY);
                Lbl[3,i].Size       = new Size(70, 13);
                Lbl[3,i].Text       = row["adress"].ToString();
                //Время сотрудника
                Lbl[4,i].Font       = FontTahoma8Regular;
                Lbl[4,i].TextAlign  = ContentAlignment.TopLeft;
                Lbl[4,i].Location   = new Point(80, 17 + dY);
                Lbl[4,i].Size       = new Size(112, 13);
                //Мест
                Lbl[5,i].Font       = FontTahoma14Bold;
                Lbl[5,i].TextAlign  = ContentAlignment.TopRight;
                Lbl[5,i].Location   = new Point(266, 3 + dY);
                Lbl[5,i].Size       = new Size(41, 27);
                Lbl[5,i].Text       = row["boxes"].ToString();
                //Старое состояние
                Lbl[6,i].Font       = FontTahoma10Bold;
                Lbl[6,i].TextAlign  = ContentAlignment.TopCenter;
                Lbl[6,i].Location   = new Point(194, 17 + dY);
                Lbl[6,i].Size       = new Size(70, 13);

                i++;
            }

            pnlNewInfo.MoveAllControls(0, -30*delta);

            //Подкрасим первую и последнюю комплектации
            //if (FirstIndex >= 0)
            //{
            //    Lbl[1, FirstIndex].BackColor = Color.PaleGoldenrod;
            //}
            //if (AllComplete && LastIndex >= 0)
            //{
            //    Lbl[1, LastIndex].BackColor = Color.PaleGreen;
            //}
            //StartTick = Environment.TickCount - StartTick;
            //MessageBox.Show(StartTick.ToString());          //debug
        }

        private void ViewClear()
        {
            pnlCurrent.Controls.Clear();
            pnlCurrent.StaticControl.Clear();
            lblDownDescr.Visible = false;
        } // ViewClear

        private void View()
        {
            if (ViewMode == SS.CurrentMode)
            {
                ReView();
                return;
            }
            ViewClear();
            switch (SS.CurrentMode)
            {
                case Mode.NewComplectationComplete:
                    ModeNewComplectationCompleteView();
                    break;
                case Mode.NewComplectation:
                    ModeNewComplectation();
                    break;
                case Mode.FreeDownComplete:
                    ModeFreeDownCompleteView();
                    break;
                case Mode.Acceptance:
                    ModeAcceptanceView();
                    break;
                case Mode.TransferInicialize:
                    lblState.Text = "Разнос инициализация";
                    ModeTransferInicializeView();
                    break;
                case Mode.AcceptedItem:
                    lblState.Text =
                        SS.AcceptedItem.InvCode.Trim() +
                        (SS.AcceptedItem.Acceptance ? " Приемка товара" : " Редактирование карточки");
                    ModeAcceptanceItemView();
                    break;
                case Mode.Transfer:
                    ModeTransferView();
                    break;
                case Mode.NewInventory:
                    ModeTransferView();
                    break;
                case Mode.NewChoiseInventory:
                    lblState.Text = "Инвентаризация запрос задания";
                    ModeNewChoiseInventoryView();
                    break;
                case Mode.Inventory:
                    lblState.Text = "Инвентаризация (" + SS.InventoryWarehouse.Name.Trim() + ")";
                    ModeInventoryView();
                    break;
                case Mode.ChoiseInventory:
                    lblState.Text = "Инвентарицазия. Выбор склада";
                    ModeChoiseInventoryView();
                    break;
                case Mode.SampleInventory:
                    lblState.Text = "Сверка товаров";
                    ModeInventoryView();
                    break;
                case Mode.SamplePut:
                    ModeSamplePutView();
                    break;
                case Mode.SampleSet:
                    ModeSampleSetView();
                    break;
                case Mode.SampleSetCorrect:
                    lblState.Text = SS.DocDown.View;
                    ModeSampleSetCorrectView();
                    break;
                case Mode.ControlCollect:
                    lblState.Text = "Просмотр сборочных листов";
                    ModeControlCollectView();
                    break;
                case Mode.HarmonizationInicialize:
                    lblState.Text = "Выбор склада, гармонизация";
                    ModeHarmonizationInicializeView();
                    break;
                case Mode.Harmonization:
                    lblState.Text = "ТЕЛЕЖКА (гармонизация)";
                    ModeHarmonizationView();
                    break;
                case Mode.HarmonizationPut:
                    lblState.Text = "ПОЛКА (гармонизация)";
                    ModeHarmonizationPutView();
                    break;
                case Mode.Loading:
                    ModeLoadingView();
                    break;
                case Mode.LoadingInicialization:
                    ModeLoadingInicializationView();
                    break;
                case Mode.Set:
                    ModeSetView();
                    break;
                case Mode.SetInicialization:
                    lblState.Text = "Инициализация отбора";
                    ModeSetInicializationView();
                    break;
                case Mode.SetComplete:
                    ModeSetCompleteView();
                    break;
                case Mode.SetCorrect:
                    lblState.Text = SS.DocDown.View;
                    ModeSetCorrectView();
                    break;
                case Mode.ChoiseDown:
                    ModeChoiseDownView();
                    break;
                case Mode.Down:
                    lblState.Text = SS.DocDown.View;
                    ModeDownView();
                    break;
                case Mode.DownComplete:
                    ModeDownCompleteView();
                    break;
                case Mode.SetSelfContorl:
                    lblState.Text = SS.DocSet.View;
                    ModeSetSelfControlView();
                    break;

                ////DEBUG!!!!!!!
                //case Mode.Loader:
                //    Loader_view();
                //    break;
                default:
                    if (SS.NewStructModes.Contains(SS.CurrentMode))
                    {
                        //string nameViewMethod = (SS.MM.GetType()).Name + "_view";
                        string nameViewMethod = SS.MM.CurrentMode.ToString() + "_view";
                        GetType().GetMethod(nameViewMethod, BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, null);
                    }
                    break;
            }
            ViewMode    = SS.CurrentMode;
            ReView();
        } // View
        
    }
}
