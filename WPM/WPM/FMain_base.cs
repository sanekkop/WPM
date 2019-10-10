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
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace WPM
{
    public partial class FMain : Form
    {
        //const string FileName = @"..\..\Settings.txt";
        const string Vers = "4.46";     //Номер версии
        const string BaseName = "int9999001rab";
        //const string BaseName = "int9999001ad1"; //debug_true
        Model SS;                       //через данный класс осуществляем общение с БД
        private ExPanel pnlCurrent;
        private int Screan;
        private int ScreanAcceptedItem;
        private int CurrentRowItem;
        private DataTable ViewUnits;
        private string FileName
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Settings.txt"; }
        }
        private Image DefaultPict
        {
            get
            {
                return pictureBox1.Image;
            }
        }
        //
        private Mode ViewMode;
        private string LastTBText;
        bool DGCellChange;
        int CurrWidth;
        string ViewProposal;
        int ChoiseCorrect;
        int CurrLineSet;

        Barcode Br;

        //test
        Thread GT;
        string OldText;
        //test

        /// <summary>
        /// Main is so main
        /// </summary>
        public FMain()
        {
            InitializeComponent();
        } // FMain
        private string GetServName(string DefaultName)
        {
            string result;
            if (File.Exists(this.FileName))
            {
                StreamReader sr = new StreamReader(this.FileName);
                result = sr.ReadLine();
                sr.Close();
            }
            else
            {
                //По умолчанию
                result = DefaultName;
            }
            return result;
        } // GetServName
        private void SaveServName(string ServName)
        {
            StreamWriter sw = new StreamWriter(this.FileName, false);
            sw.WriteLine(ServName);
            sw.Close();
        } // SaveServName
        /// <summary>
        /// create main obj SS and init all subscription
        /// </summary>
        /// <param name="ServName">server name</param>
        private void InitSS(string ServName)
        {
            SS = new Model(ServName, BaseName, Vers);
            SS.ChangeMode += OnChangeMode;
            SS.ScanBox += OnScanBox;
            SS.Update += new Model.UpdaterEventHandler(SS_Update);
            SS.Report += new Model.ReportEventHandler(SS_Report);
        } // InitSS
        /// <summary>
        /// Здесь будем все инициализировать (форма уже отстроена, только не видно ее)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FMainOnLoad(object sender, EventArgs e)
        {
            //Text = "zhopa";
            //return;
            InicializeFont();
            CurrWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            //СОЗДАЕМ ПАНЕЛИ
            pnlCurrent = new ExPanel();
            //pnlCurrent.BackColor = SystemColors.ActiveBorder;
            pnlCurrent.BackColor = Color.White;
            pnlCurrent.Location = new Point(0, 16);
            pnlCurrent.Name = "pnlCurrent";
            pnlCurrent.Size = new Size(CurrWidth - 2, 185);
            this.Controls.Add(pnlCurrent);
            pnlCurrent.BringToFront();
            //pnlSections.BackColor = System.Drawing.SystemColors.ActiveBorder;

            //ИНИЦИАЛИЗАЦИЮ РАЗМЕСТИМ ЗДЕСЬ
            DGCellChange = false;
            Text = "WPM " + Vers;
            WindowState = FormWindowState.Maximized;
            ViewMode = Mode.None;
            Screan = 0;
            Br = new Barcode(this, 200);
            Br.BarcodeRead += ReactionBarcode;
            Br.HotKeyEvent += HotKeyEvent;

            //MessageBox.Show();
            //string[] names = Assembly.GetExecutingAssembly().GetModules

            //MessageBox.Show(Assembly.GetExecutingAssembly().Location);
            //MessageBox.Show(Path.GetFullPath(FileName));
            //return;
            //MessageBox.Show("Hyper");
            int CountServers = 2;
            string[] NamesOfServers;
            NamesOfServers = new string[CountServers];
            NamesOfServers[0] = @"192.168.8.4,57068";
            NamesOfServers[1] = @"192.168.8.5,57068";
            //NamesOfServers[0] = @"192.168.8.11\sqlcl1";
            //NamesOfServers[1] = @"192.168.8.12\sqlcl1";
            //DO NOT FORGOT CHANGE PARAMETR: CountServers!!!

            if (false)
            {
                //ТЕСТ ЭКРАНА
                pnlCurrent.Controls.Clear();
                ABaseMode Obj = new Waiting(SS, null);
                //RefillSet_view();
                RefillLayout_view();
                foreach (Control Ctrl in pnlCurrent.Controls)
                {
                    if (Ctrl.GetType().Equals(typeof(Label)))
                    {
                        (Ctrl as Label).Text = "00000000000000000000000000000000000000";
                    }
                    else if (Ctrl.GetType().Equals(typeof(DataGrid)))
                    {
                        (Ctrl as DataGrid).Visible = false;
                    }
                }
                return;
            }



            string FirstServName = GetServName(NamesOfServers[0]);
            InitSS(FirstServName);

            if (!SS.Initialize())
            {
                for (int i = 0; i < CountServers; ++i)
                {
                    if (FirstServName == NamesOfServers[i])
                    {
                        continue;
                    }

                    InitSS(NamesOfServers[i]);
                    if (SS.Initialize())
                    {
                        SaveServName(NamesOfServers[i]);
                        break;
                    }
                }
            }
            if (!SS.Connect)
            {
                MessageBox.Show("Не удалось синхронизовать (версия: " + Vers + ")! ОШИБКА: " + SS.ExcStr);
                Close();
                return;
            }
            View();
            if (SS.DeviceName == null || SS.DeviceName.Trim() == "")
            {
                lblName.Text = Helper.SuckDigits(DeviceID.GetDeviceName());
            }
            else
            {
                lblName.Text = Helper.SuckDigits(SS.DeviceName);
            }
            pnlCurrent.GetLabelByName("lblResult").Text = "";
            lblAction.Width = CurrWidth - 2;
            //lblAction.Font = new Font("Tahoma", CurrWidth < 320 ? 11 : 14, FontStyle.Bold);
            lblName.Location = new Point(CurrWidth - 24, 0);
            lblState.Width = CurrWidth - 2;
            lblState.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
        }

        void SS_Update(object sender, UpdateEventArgs e)
        {
            string file = @"Application\WPM\Updater.exe";
            string FromFile = SS.PicturePath.Trim() + @"\WPM\" + e.Vers + @"\WPM.exe";
            string ToFile = @"Application\WPM\WPM.exe";
            string ProcessID = Process.GetCurrentProcess().Id.ToString();
            if (!File.Exists(FromFile))
            {
                MessageBox.Show("ФАЙЛ ОБНОВЛЕНИЯ НЕДОСТУПЕН!");
            }
            else
            {
                if (!File.Exists(file))
                {
                    MessageBox.Show("ФАЙЛ updater.exe НЕДОСТУПЕН!");
                }
                else
                {
                    Process.Start(file, FromFile + " " + ToFile + " " + ProcessID);
                }
            }
            //В любом случае самовыпиливаемся
            Process.GetCurrentProcess().Kill();
        } // SS_Update
        void SS_Report(object sender, ReportEventArgs e)
        {
            if (!e.EventActive)
            {
                return;
            }
            ReView();
            if (e.Message != null)
            {
                lblAction.Text = e.Message;
            }
            MakeSound(e.Voice);
            if (e.ForeColor != Color.Empty)
            {
                lblAction.ForeColor = e.ForeColor;
            }
            else
            {
                lblAction.ForeColor = Color.Blue;   //Цвет по умолчанию
            }
            Refresh();
        } // SS_Report
        private void OnChangeMode(object sender, ChangeModeEventArgs e)
        {
            ViewMode = Mode.None;
            View();
            MakeSound(e.Voice);
        } // OnChangeMode

        private void RKAcceptence(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if ((Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189) && Screan == 1)
            {
                if (currControl.Name == "tbLabelCount")
                {
                    DynamicLostFocus(currControl, null);
                }
                else
                {
                    DynamicButtonOnClick(pnlCurrent.GetControlByName("btnPrintCondition"), null);
                }
            }
            else if (Key == Keys.Space && Screan == 1)
            {
                DynamicButtonOnClick(pnlCurrent.GetControlByName("btnPrint"), null);
            }
            else if (Key == Keys.Left && Screan == 0)
            {
                pnlCurrent.Sweep(1);
                Screan -= 1;
                pnlCurrent.GetControlByName("dgNotAcceptedItems").Focus();
            }
            else if (Key == Keys.Left && Screan == 1)
            {
                pnlCurrent.Sweep(1);
                Screan -= 1;
                pnlCurrent.GetControlByName("dgConsignment").Focus();
            }
            else if (Key == Keys.Right && Screan == 0)
            {
                pnlCurrent.Sweep(-1);
                Screan += 1;
                pnlCurrent.GetControlByName("dgAcceptedItems").Focus();
            }
            else if (Key == Keys.Right && Screan == -1)
            {
                pnlCurrent.Sweep(-1);
                Screan += 1;
                pnlCurrent.GetControlByName("dgConsignment").Focus();
            }
            else if (Key == Keys.Back && Screan == 0)
            {
                if (currControl.Name == "dgConsignment" && SS.Consignment.Rows.Count > 0)
                {
                    DataGrid currDG = currControl as DataGrid;
                    SS.DeleteConsignment(SS.Consignment.Rows[currDG.CurrentRowIndex]["ACID"].ToString());
                }
            }
            else if (Key == Keys.Back && Screan == 1)
            {
                if (currControl.Name == "dgAcceptedItems")
                {
                    DataGrid currDG = pnlCurrent.GetDataGridByName("dgAcceptedItems");
                    if (currDG.CurrentRowIndex != -1)
                    {
                        string InvCode = SS.AcceptedItems.Rows[currDG.CurrentRowIndex]["InvCode"].ToString();
                        if (!SS.DeleteRowAcceptedItems(SS.AcceptedItems.Rows[currDG.CurrentRowIndex]))
                        {
                            lblAction.Text = SS.ExcStr;
                            return;
                        }
                        else
                        {
                            lblAction.Text = InvCode.Trim() + " - приемка отменена!";
                        }
                    }
                }
            }
            else if (Key == Keys.Back && Screan == -1)
            {
                if (currControl.Name == "dgNotAcceptedItems")
                {
                    pnlCurrent.GetTextBoxByName("tbFind").Text = "";
                }
            }

            else if ((Key == Keys.Enter || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189) && Screan == -1)
            {
                if (currControl.Name == "dgNotAcceptedItems")
                {
                    DataGrid currDG = currControl as DataGrid;
                    if (currDG.CurrentRowIndex != -1)
                    {
                        DataRowView currDRV = (currDG.DataSource as BindingSource).Current as DataRowView;
                        SS.ChoiseItem(
                            currDRV.Row["ID"].ToString(),
                            currDRV.Row["IDDOC"].ToString());
                        View();
                        lblAction.Text = SS.ExcStr;
                        return;
                    }
                }
            }
            else if (Key == Keys.Enter && Screan == 1)
            {
                if (currControl.Name == "dgAcceptedItems")
                {
                    TextBox currTB = pnlCurrent.GetTextBoxByName("tbLabelCount");
                    currTB.Focus();
                    currTB.Select(0, currTB.Text.Length);
                    return;
                }
                else if (currControl.Name == "tbLabelCount")
                {
                    DynamicLostFocus(currControl, null);
                }
            }
            else if (Screan == 1 && Key.GetHashCode() >= 48 && Key.GetHashCode() <= 57)
            {
                //Реакция на нажатие цифр на экране принятого товара
                if (currControl.Name == "dgAcceptedItems")
                {
                    DataGrid currDG = currControl as DataGrid;
                    if (currDG.CurrentRowIndex != -1)
                    {
                        //валим редактировать тексбокс и сразу запихаем туда это число
                        TextBox currTB = pnlCurrent.GetTextBoxByName("tbLabelCount");
                        currTB.Text = "" + (char)Key;
                        currTB.Focus();
                        currTB.Select(1, 0);
                        return;
                    }
                }
                else if (currControl.Name == "tbLabelCount")
                {
                    return;
                }
            }
            else if (Screan == -1 && Key.GetHashCode() >= 48 && Key.GetHashCode() <= 57)
            {
                //Реакция на нажатие цифр на экране не принятого товара
                if (currControl.Name == "dgNotAcceptedItems")
                {
                    DataGrid currDG = currControl as DataGrid;
                    if (currDG.CurrentRowIndex != -1)
                    {
                        TextBox currTB = pnlCurrent.GetTextBoxByName("tbFind");
                        currTB.Text = currTB.Text + (char)Key;
                        currTB.Focus();
                        currTB.Select(currTB.Text.Length, 0);
                    }
                }
            }
            else if (Screan == 1 && (Key == Keys.Down || Key == Keys.Up))
            {
                if (SS.AcceptedItems.Rows.Count > 0)
                {
                    if (currControl.Name == "tbLabelCount")
                    {
                        //Хочет свалить с окошка - зафиксируем то что там накалякано
                        ReactionKey(Keys.Enter);
                    }
                }
            }
            else if (Screan == -1 && (Key == Keys.Down || Key == Keys.Up))
            {
                if (currControl.Name == "tbFind")
                {
                    pnlCurrent.GetControlByName("dgNotAcceptedItems").Focus();
                }
                else if (currControl.Name == "dgNotAcceptedItems")
                {
                    DataGrid currDG = currControl as DataGrid;
                    if (!DGCellChange)
                    {
                        if (currDG.CurrentRowIndex == 0)
                        {
                            currDG.CurrentRowIndex = (currDG.DataSource as BindingSource).Count - 1;
                        }
                        else if ((currDG.CurrentRowIndex >= 0) && (currDG.CurrentRowIndex == (currDG.DataSource as BindingSource).Count - 1))
                        {
                            currDG.CurrentRowIndex = 0;
                        }
                    }
                }
            }
            View();
        }
        private void RKAcceptenceItem(Keys Key, Control currControl)
        {
            //Фотка закрывается от любого нажатия
            PictureBox pbPhoto = pnlCurrent.GetPictureBoxByName("pbPhoto");
            if (pbPhoto.Visible)
            {
                pbPhoto.Visible = false;
                return;
            }

            //Дальше все как обычно
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.Up)
            {
                if (CurrentRowItem > 0)
                {
                    TextBox tbCoef = pnlCurrent.GetTextBoxByName("tbCoef");
                    TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
                    tbCoef.Top -= 26;
                    tbCount.Top -= 26;
                    DynamicLostFocus(tbCoef, null);
                    DynamicLostFocus(tbCount, null);
                    --CurrentRowItem;
                    DynamicGotFocus(tbCoef, null);
                    DynamicGotFocus(tbCount, null);
                }
                else if (CurrentRowItem == 0 && pnlCurrent.GetFocused().Name != "tbDetails")
                {
                    //kill it
                    TextBox tbCoef = pnlCurrent.GetTextBoxByName("tbCoef");
                    TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
                    TextBox tbDetails = pnlCurrent.GetTextBoxByName("tbDetails");
                    DynamicLostFocus(tbCoef, null);
                    DynamicLostFocus(tbCount, null);
                    pnlCurrent.GetControlByName("tbDetails").Focus();
                }
            }
            else if (Key == Keys.Down)
            {
                if (pnlCurrent.GetFocused().Name == "tbDetails")
                {
                    //kill it
                    TextBox tbCoef = pnlCurrent.GetTextBoxByName("tbCoef");
                    TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
                    TextBox tbDetails = pnlCurrent.GetTextBoxByName("tbDetails");
                    DynamicLostFocus(tbDetails, null);
                    DynamicGotFocus(tbCoef, null);
                    DynamicGotFocus(tbCount, null);
                    if (SS.AcceptedItem.Acceptance)
                    {
                        pnlCurrent.GetControlByName("tbCount").Focus();
                    }
                    else
                    {
                        pnlCurrent.GetControlByName("tbCoef").Focus();
                    }

                }
                else if (CurrentRowItem < ViewUnits.Rows.Count - 1 - 1)  //убрал прочее (-1)
                {
                    TextBox tbCoef = pnlCurrent.GetTextBoxByName("tbCoef");
                    TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
                    tbCoef.Top += 26;
                    tbCount.Top += 26;
                    DynamicLostFocus(tbCoef, null);
                    DynamicLostFocus(tbCount, null);
                    ++CurrentRowItem;
                    DynamicGotFocus(tbCoef, null);
                    DynamicGotFocus(tbCount, null);
                }
            }
            else if (!SS.AcceptedItem.Acceptance && Key == Keys.Left && ScreanAcceptedItem >= 0)
            {
                pnlCurrent.Sweep(1);
                ScreanAcceptedItem -= 1;
            }
            else if (!SS.AcceptedItem.Acceptance && Key == Keys.Right && ScreanAcceptedItem <= 0)
            {
                pnlCurrent.Sweep(-1);
                ScreanAcceptedItem += 1;
            }
            else if (Key == Keys.Left || Key == Keys.Right || Key == Keys.Enter)
            {
                if (currControl.Name == "tbCoef")
                {
                    pnlCurrent.GetControlByName("tbCount").Focus();
                }
                else if (currControl.Name == "tbCount")
                {
                    pnlCurrent.GetControlByName("tbCoef").Focus();
                }
            }
            else if (ScreanAcceptedItem == 0 && (Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189))
            {
                //Убрал в условие выше (Key == Keys.Space) из компании кнопок F1, F2 и т.д.
                DynamicLostFocus(currControl, null);
                DynamicButtonOnClick(pnlCurrent.GetControlByName("bntOk"), null);
            }
            else if (Key == Keys.Space && ScreanAcceptedItem == 0)
            {
                if (pbPhoto.Image == null)
                {
                    LoadImage(pbPhoto, SS.AcceptedItem.InvCode);
                }
                pbPhoto.Visible = true;
                pbPhoto.BringToFront();
                return;
            }
            View();
        }
        private void RKTransferInicialize(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.Enter || Key == Keys.Space)
            {
                if (currControl.Name == "dgFrom")
                {
                    pnlCurrent.GetControlByName("dgTo").Focus();
                }
                else if (currControl.Name == "dgTo")
                {
                    pnlCurrent.GetControlByName("btnOk").Focus();
                }
                else if (currControl.Name == "btnOk")
                {
                    DynamicButtonOnClick(pnlCurrent.GetControlByName("btnOk"), null);
                }
                ReView();
            }
        }
        private void RKHarmonizationInicialize(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.Enter || Key == Keys.Space)
            {
                if (currControl.Name == "dgFrom")
                {
                    pnlCurrent.GetControlByName("btnOk").Focus();
                }
                else if (currControl.Name == "btnOk")
                {
                    DynamicButtonOnClick(pnlCurrent.GetControlByName("btnOk"), null);
                }
                ReView();
            }
        }
        private void RKLoadingInicialization(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                DynamicButtonOnClick(pnlCurrent.GetControlByName("btnPrint"), null);
                ReView();
            }
        }
        private void RKTransfer(Keys Key, Control currControl)
        {
            int cri = -1;
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.Right && Screan == 0)
            {
                pnlCurrent.Sweep(-1);
                Screan += 1;
                pnlCurrent.GetControlByName("dgTransferReadyItems").Focus();
            }
            else if (Key == Keys.Left && Screan == 1)
            {
                pnlCurrent.Sweep(1);
                Screan -= 1;
                pnlCurrent.GetControlByName("dgTransferItems").Focus();
            }
            else if ((Key == Keys.Enter || Key == Keys.Space) && Screan == 1)
            {
                DynamicButtonOnClick(pnlCurrent.GetControlByName("btnPrint"), null);
            }
            else if ((Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189) && SS.CurrentMode == Mode.NewInventory)
            {
                if (currControl.Name == "dgTransferItems")
                {
                    DataGrid currDG = pnlCurrent.GetDataGridByName("dgTransferItems");
                    if (currDG.CurrentRowIndex != -1)
                    {
                        cri = currDG.CurrentRowIndex;
                        if (SS.CheckParty(SS.TransferItems.Rows[currDG.CurrentRowIndex]["ID"].ToString()))
                        {
                            View();
                        }
                        lblAction.Text = SS.ExcStr;
                        return;
                    }
                }
            }
            else if (Key == Keys.Back)
            {
                if (currControl.Name == "dgTransferItems" && SS.CurrentMode != Mode.NewInventory)
                {
                    DataGrid currDG = pnlCurrent.GetDataGridByName("dgTransferItems");
                    if (currDG.CurrentRowIndex != -1)
                    {
                        string InvCode = SS.TransferItems.Rows[currDG.CurrentRowIndex]["InvCode"].ToString();
                        cri = currDG.CurrentRowIndex;
                        if (!SS.DeleteRowTransferItem(SS.TransferItems.Rows[currDG.CurrentRowIndex]))
                        {
                            lblAction.Text = SS.ExcStr;
                            return;
                        }
                        else
                        {
                            lblAction.Text = InvCode.Trim() + " - удален из тележки!";
                        }
                    }
                }
            }
            ReView();
            if (cri != -1)
            {
                DataGrid currDG = pnlCurrent.GetDataGridByName("dgTransferItems");
                if (SS.TransferItems.Rows.Count >= cri + 1)
                {
                    currDG.CurrentRowIndex = cri;
                }
                else if (SS.TransferItems.Rows.Count > 0)
                {
                    currDG.CurrentRowIndex = cri - 1;
                }
            }
        }
        private void RKHarmonization(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if ((Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189))
            {
                DynamicButtonOnClick(pnlCurrent.GetControlByName("btnPrint"), null);
            }
            ReView();
        }
        private void RKInventory(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.Enter || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                if (Screan == 1)
                {
                    lblAction.Text = "Выполняется печать...";
                    Refresh();
                    if (!SS.PrintSampleLables())
                    {
                        lblAction.Text = SS.ExcStr;
                        return;
                    }
                    lblAction.Text = SS.ExcStr;
                }
                else if (currControl.Name == "tbFindItem")
                {
                    TextBox tb = currControl as TextBox;
                    if (tb.Text.Length > 6)
                    {
                        lblAction.Text = "Выполняется поиск '" + tb.Text.Substring(0, 6) + "', ждите...";
                    }
                    else
                    {
                        lblAction.Text = "Выполняется поиск '" + tb.Text + "', ждите...";
                    }
                    Refresh();

                    //if (SS.CurrentMode == Mode.SampleInventory)
                    //{
                    //    SS.FindItemsOnBalances((currControl as TextBox).Text);
                    //    lblAction.Text = "(ИНВ.КОД.) " + SS.ExcStr;
                    //}
                    //else
                    //{
                    List<string> FieldName = new List<string>();
                    if (Key == Keys.Enter)
                    {
                        //FieldName.Add("descr");
                        FieldName.Add("$Спр.Товары.Артикул ");
                        FieldName.Add("$Спр.Товары.АртикулНаУпаковке ");
                        //SS.FindItems((currControl as TextBox).Text, FieldName, 4);
                        SS.FindItemsOnBalances((currControl as TextBox).Text, FieldName);
                        lblAction.Text = "(АРТИКУЛЫ) " + SS.ExcStr;
                    }
                    else
                    {
                        //По зеленым ищем код
                        FieldName.Add("$Спр.Товары.ИнвКод ");
                        //SS.FindItems((currControl as TextBox).Text, FieldName, 6);
                        SS.FindItemsOnBalances((currControl as TextBox).Text, FieldName);
                        lblAction.Text = "(ИНВ.КОД.) " + SS.ExcStr;
                    }
                    //}
                    pnlCurrent.GetDataGridByName("dgInventory").Focus();
                }
                else if (currControl.Name == "dgInventory" && Key == Keys.Enter)
                {
                    pnlCurrent.GetControlByName("tbFindItem").Focus();
                }
                else if (currControl.Name == "dgInventory")
                {
                    DataGrid currDG = currControl as DataGrid;
                    if (currDG.CurrentRowIndex != -1)
                    {
                        lblAction.Text = "Загрузка информации о товаре...";
                        Refresh();
                        DataRowView currDRV = (currDG.DataSource as BindingSource).Current as DataRowView;
                        SS.ChoiseItemInventory(currDRV.Row["ID"].ToString());
                        View();
                        lblAction.Text = SS.ExcStr;
                        return;
                    }
                }
            }
            else if (Key == Keys.Down || Key == Keys.Up)
            {
                if (currControl.Name == "dgInventory")
                {
                    DataGrid currDG = currControl as DataGrid;
                    if (!DGCellChange)
                    {
                        if (currDG.CurrentRowIndex == 0)
                        {
                            currDG.CurrentRowIndex = (currDG.DataSource as BindingSource).Count - 1;
                        }
                        else if ((currDG.CurrentRowIndex >= 0) && (currDG.CurrentRowIndex == (currDG.DataSource as BindingSource).Count - 1))
                        {
                            currDG.CurrentRowIndex = 0;
                        }
                    }
                }
            }
            else if (Key == Keys.Right && Screan == 1)
            {
                pnlCurrent.Sweep(-1);
                Screan -= 1;
                if (SS.Inventory.Rows.Count == 0)
                {
                    pnlCurrent.GetControlByName("tbFindItem").Focus();
                }
                else
                {
                    pnlCurrent.GetDataGridByName("dgInventory").Focus();
                }
            }
            else if (Key == Keys.Left && Screan == 0)
            {
                pnlCurrent.Sweep(1);
                Screan += 1;
                pnlCurrent.GetDataGridByName("dgC").Focus();
            }
            else if (Key == Keys.Back && Screan == 0)
            {
                Control currFocused = pnlCurrent.GetFocused();
                if (currFocused.Name == "dgInventory")
                {
                    DataGrid currDG = currFocused as DataGrid;
                    if (currDG.CurrentRowIndex != -1)
                    {
                        SS.AddToForPrint(SS.Inventory.Rows[currDG.CurrentRowIndex]);
                        lblAction.Text = SS.ExcStr;
                    }
                }
            }
            else if (Key == Keys.Back && Screan == 1)
            {
                Control currFocused = pnlCurrent.GetFocused();
                if (currFocused.Name == "dgC")
                {
                    DataGrid currDG = currFocused as DataGrid;
                    if (currDG.CurrentRowIndex != -1)
                    {
                        SS.ForPrint.Rows[currDG.CurrentRowIndex].Delete();
                        SS.AddToForPrint(null);
                    }
                }
            }
            View();
        }
        private void RKChoiseInventory(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.Enter || Key == Keys.Space || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                DynamicButtonOnClick(pnlCurrent.GetControlByName("btnOk"), null);
            }
        }
        private void RKChoiseNewInventory(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                DynamicButtonOnClick(pnlCurrent.GetControlByName("btnOk"), null);
            }
        }
        private void RKSampleSet(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            
            if (Screan == 0 && (Key == Keys.Enter || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189) && SS.CurrentAction == ActionSet.EnterCount)
            {
                int tmpCount;
                try
                {
                    string tmpTxt = pnlCurrent.GetTextBoxByName("tbCount").Text;
                    if (tmpTxt.Substring(tmpTxt.Length - 1, 1) == "-")
                    {
                        tmpTxt = tmpTxt.Substring(0, tmpTxt.Length - 1);
                    }
                    tmpCount = Convert.ToInt32(tmpTxt);
                }
                catch
                {
                    tmpCount = 0;
                }
                if (SS.EnterCountSampleSet(tmpCount))
                {
                    View();
                    GoodDone();
                    pnlCurrent.GetTextBoxByName("tbCount").Text = "";
                }
                else
                {
                    View();
                    lblAction.Text = SS.ExcStr;
                    BadDone();
                }
            }
            if (Key == Keys.Right && Screan == 0)
            {
                if (!SS.Employer.CanRoute)
                {
                    string appendix = " (нельзя посмотреть маршрут)";
                    if (lblAction.Text.IndexOf(appendix) < 0)
                    {
                        lblAction.Text = lblAction.Text + appendix;
                    }
                    return;
                }
                string tmp = lblAction.Text;
                lblAction.Text = "Подгружаю список...";
                Refresh();
                SS.RefreshAMT();
                View();
                //Вернем текущую строку
                DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
                try
                {
                    DataGridCell currCell = dgGoodsCC.CurrentCell;
                    currCell.ColumnNumber = 2;
                    dgGoodsCC.CurrentCell = currCell;
                    dgGoodsCC.CurrentRowIndex = CurrLineSet;
                }
                catch
                {
                }
                pnlCurrent.Sweep(-1);
                Screan -= 1;
                dgGoodsCC.Focus();
                lblAction.Text = tmp;
            }
            else if (Key == Keys.Left && Screan == -1)
            {
                DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
                CurrLineSet = dgGoodsCC.CurrentRowIndex;
                pnlCurrent.Sweep(1);
                Screan += 1;
                TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
                if (tbCount.Visible)
                {
                    pnlCurrent.GetControlByName("tbCount").Focus();
                }
            }
            if (Screan == 0 && Key == Keys.D9 && SS.CurrentAction != ActionSet.EnterCount && !SS.DocSet.Special)
            {
                if (SS.Const.StopCorrect)
                {
                    //StopCorrect - ВРЕМЕНННАЯ ЗАГЛУШКА
                    lblAction.Text = "Возможность корректировать отключена!";
                    return;
                }
                SS.ToModeSampleSetCorrect();
                BadVoice();
                View();
            }
        }
        private void RKSamplePut(Keys Key, Control currControl)
        {
            int cri = -1;
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.Back && Screan == -1)
            {
                if (currControl.Name == "dgConsignment" && SS.SampleTransfers.Rows.Count > 0)
                {
                    DataGrid currDG = currControl as DataGrid;
                    string DocNo = SS.SampleTransfers.Rows[currDG.CurrentRowIndex]["DOCNO"].ToString();
                    SS.DeleteSampleTransfer(SS.SampleTransfers.Rows[currDG.CurrentRowIndex]["ATID"].ToString());
                    lblAction.Text = "Перемещение " + DocNo + " - удалено из списка!";
                }
            }
            else if (Key == Keys.Right && Screan == 0)
            {
                pnlCurrent.Sweep(-1);
                Screan += 1;
                pnlCurrent.GetControlByName("dgTransferReadyItems").Focus();
            }
            else if (Key == Keys.Left && Screan == 1)
            {
                pnlCurrent.Sweep(1);
                Screan -= 1;
                pnlCurrent.GetControlByName("dgSamples").Focus();
            }
            else if (Key == Keys.Left && Screan == 0)
            {
                pnlCurrent.Sweep(1);
                Screan -= 1;
                pnlCurrent.GetControlByName("dgConsignment").Focus();
            }
            else if (Key == Keys.Right && Screan == -1)
            {
                pnlCurrent.Sweep(-1);
                Screan += 1;
                pnlCurrent.GetControlByName("dgSamples").Focus();
            }
            else if ((Key == Keys.Space || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189) && Screan == -1)
            {
                DynamicButtonOnClick(pnlCurrent.GetControlByName("btnPrintPrices"), null);
            }
            else if ((Key == Keys.Space || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189) && Screan == 1)
            {
                DynamicButtonOnClick(pnlCurrent.GetControlByName("btnPrint"), null);
            }
            else if ((Key == Keys.Space || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189) && Screan == 0)
            {
                if (currControl.Name == "dgSamples")
                {
                    DataGrid currDG = currControl as DataGrid;
                    if (currDG.CurrentRowIndex != -1)
                    {
                        lblAction.Text = "Загрузка информации о товаре...";
                        Refresh();
                        DataRowView currDRV = (currDG.DataSource as BindingSource).Current as DataRowView;
                        SS.ChoiseItemInventory(currDRV.Row["ID"].ToString());
                        View();
                        lblAction.Text = SS.ExcStr;
                        return;
                    }
                }

            }
            else if (Screan == 0 && Key.GetHashCode() >= 48 && Key.GetHashCode() <= 57)
            {
                //Реакция на нажатие цифр на экране образцов
                if (currControl.Name == "dgSamples")
                {
                    DataGrid currDG = currControl as DataGrid;
                    if (currDG.CurrentRowIndex != -1)
                    {
                        TextBox currTB = pnlCurrent.GetTextBoxByName("tbFind");
                        currTB.Text = currTB.Text + (char)Key;
                        currTB.Focus();
                        currTB.Select(currTB.Text.Length, 0);
                    }
                }
            }
            else if (Screan == 0 && (Key == Keys.Down || Key == Keys.Up))
            {
                if (currControl.Name == "tbFind")
                {
                    pnlCurrent.GetControlByName("dgSamples").Focus();
                }
                else if (currControl.Name == "dgSamples")
                {
                    DataGrid currDG = currControl as DataGrid;
                    if (!DGCellChange)
                    {
                        if (currDG.CurrentRowIndex == 0)
                        {
                            currDG.CurrentRowIndex = (currDG.DataSource as BindingSource).Count - 1;
                        }
                        else if ((currDG.CurrentRowIndex >= 0) && (currDG.CurrentRowIndex == (currDG.DataSource as BindingSource).Count - 1))
                        {
                            currDG.CurrentRowIndex = 0;
                        }
                    }
                }
            }
            else if (Screan == 1 && (Key == Keys.Down || Key == Keys.Up))
            {
                if (currControl.Name == "dgTransferReadyItems")
                {
                    DataGrid currDG = currControl as DataGrid;
                    if (!DGCellChange)
                    {
                        if (currDG.CurrentRowIndex == 0)
                        {
                            currDG.CurrentRowIndex = (currDG.DataSource as DataTable).Rows.Count - 1;
                        }
                        else if ((currDG.CurrentRowIndex >= 0) && (currDG.CurrentRowIndex == (currDG.DataSource as DataTable).Rows.Count - 1))
                        {
                            currDG.CurrentRowIndex = 0;
                        }
                    }
                }
            }
            else if (Key == Keys.Back)
            {
                if (currControl.Name == "dgTransferReadyItems")
                {
                    DataGrid currDG = pnlCurrent.GetDataGridByName("dgTransferReadyItems");
                    if (currDG.CurrentRowIndex != -1)
                    {
                        string InvCode = SS.TransferReadyItems.Rows[currDG.CurrentRowIndex]["InvCode"].ToString();
                        cri = currDG.CurrentRowIndex;
                        if (!SS.DeleteRowSampleItem(SS.TransferReadyItems.Rows[currDG.CurrentRowIndex]))
                        {
                            lblAction.Text = SS.ExcStr;
                            return;
                        }
                        else
                        {
                            lblAction.Text = InvCode.Trim() + " - выкладка отменена!";
                        }
                    }
                }
            }
            ReView();
            if (cri != -1)
            {
                DataGrid currDG = pnlCurrent.GetDataGridByName("dgTransferReadyItems");
                if (SS.TransferReadyItems.Rows.Count >= cri + 1)
                {
                    currDG.CurrentRowIndex = cri;
                }
                else if (SS.TransferReadyItems.Rows.Count > 0)
                {
                    currDG.CurrentRowIndex = cri - 1;
                }
            }
        }// RKSamplePut()
        private void RKLoading(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.Right && Screan == 0)
            {
                DataGrid tmpDG = pnlCurrent.GetControlByName("dgWayBill") as DataGrid;
                SS.CurrentLine = tmpDG.CurrentRowIndex;
                ModeLoadingReView(true);
                pnlCurrent.Sweep(-1);
                Screan += 1;
                tmpDG.Focus();
            }
            else if (Key == Keys.Left && Screan == 1)
            {
                DataGrid tmpDG = pnlCurrent.GetControlByName("dgWayBill") as DataGrid;
                SS.CurrentLine = tmpDG.CurrentRowIndex;
                ModeLoadingReView(true);
                pnlCurrent.Sweep(1);
                Screan -= 1;
                tmpDG.Focus();
            }
            else if (Key.GetHashCode() == 40 && Screan == 1)
            {
                ExPanel pnlInfoNew = pnlCurrent.GetControlByName("pnlInfoNew") as ExPanel;
                pnlInfoNew.MoveControls(0, -15);
            }
            else if (Key.GetHashCode() == 38 && Screan == 1)
            {
                ExPanel pnlInfoNew = pnlCurrent.GetControlByName("pnlInfoNew") as ExPanel;
                pnlInfoNew.MoveControls(0, +15);
            }
            else if (Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                DynamicButtonOnClick(pnlCurrent.GetControlByName("btnPrint"), null);
            }
        }
        private void RKSet(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }

            if (Key == Keys.Right && Screan == 0)
            {
                if (!SS.Employer.CanRoute)
                {
                    string appendix = " (нельзя посмотреть маршрут)";
                    if (lblAction.Text.IndexOf(appendix) < 0)
                    {
                        lblAction.Text = lblAction.Text + appendix;
                    }
                    return;
                }
                string tmp = lblAction.Text;
                lblAction.Text = "Подгружаю список...";
                Refresh();
                SS.RefreshCCRP();
                View();
                //Вернем текущую строку
                DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
                try
                {
                    DataGridCell currCell = dgGoodsCC.CurrentCell;
                    currCell.ColumnNumber = 2;
                    dgGoodsCC.CurrentCell = currCell;
                    dgGoodsCC.CurrentRowIndex = CurrLineSet;
                }
                catch
                {
                }
                pnlCurrent.Sweep(-1);
                Screan -= 1;
                dgGoodsCC.Focus();
                lblAction.Text = tmp;
            }
            else if (Key == Keys.Left && Screan == -1)
            {
                DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
                CurrLineSet = dgGoodsCC.CurrentRowIndex;
                pnlCurrent.Sweep(1);
                Screan += 1;
                TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
                if (tbCount.Visible)
                {
                    pnlCurrent.GetControlByName("tbCount").Focus();
                }
            }
            else if (Screan == 0 && Key == Keys.D9 && SS.CurrentAction != ActionSet.EnterCount && !SS.DocSet.Special)
            {
                if (SS.Const.StopCorrect)
                {
                    //StopCorrect - ВРЕМЕНННАЯ ЗАГЛУШКА
                    lblAction.Text = "Возможность корректировать отключена!";
                    return;
                }
                SS.ToModeSetCorrect();
                BadVoice();
                View();
            }
            else if (Screan == 0 && (Key == Keys.Enter || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189) && SS.CurrentAction == ActionSet.EnterCount)
            {
                int tmpCount;
                try
                {
                    string tmpTxt = pnlCurrent.GetTextBoxByName("tbCount").Text;
                    if (tmpTxt.Substring(tmpTxt.Length - 1, 1) == "-")
                    {
                        tmpTxt = tmpTxt.Substring(0, tmpTxt.Length - 1);
                    }
                    tmpCount = Convert.ToInt32(tmpTxt);
                }
                catch
                {
                    tmpCount = 0;
                }
                if (SS.EnterCountSet(tmpCount))
                {
                    View();
                    GoodDone();
                    pnlCurrent.GetTextBoxByName("tbCount").Text = "";
                }
                else
                {
                    View();
                    lblAction.Text = SS.ExcStr;
                    BadDone();
                }
            }
        }
        private void RKSetCopmlete(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }

            if (Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189 || Key == Keys.Enter)
            {
                int tmpCount;
                try
                {
                    string tmpTxt = pnlCurrent.GetTextBoxByName("tbCount").Text;
                    if (tmpTxt.Substring(tmpTxt.Length - 1, 1) == "-")
                    {
                        tmpTxt = tmpTxt.Substring(0, tmpTxt.Length - 1);
                    }
                    tmpCount = Convert.ToInt32(tmpTxt);
                }
                catch
                {
                    lblAction.Text = "Не верно указано количество мест!";
                    BadVoice();
                    return;
                }
                if (SS.EnterCountBox(tmpCount))
                {
                    pnlCurrent.GetTextBoxByName("tbCount").Visible = false;
                    View();
                }
                else
                {
                    pnlCurrent.GetTextBoxByName("tbCount").Text = "";
                    BadVoice();
                }
                lblAction.Text = SS.ExcStr;
            }
        } // RKSetCopmlete
        private void RKSetInicialization(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }

            if (Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                lblAction.Text = "Запрос задания...";
                Refresh();
                if (SS.CompleteSetInicialization())
                {
                    View();
                }
                else
                {
                    lblAction.Text = SS.ExcStr;
                    BadDone();
                }
            }
        }
        private void RKSampleSetCorrect(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape || (Key == Keys.D0 && ChoiseCorrect == 0))
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            if (ChoiseCorrect == 0)
            {
                ChoiseCorrect = Helper.WhatInt(Key);
                if (ChoiseCorrect > 0 && ChoiseCorrect < 3)
                {
                    lblAction.Text = "Укажите количество в штуках";
                }
                else
                {
                    ChoiseCorrect = 0;
                }
                ReView();
            }
            else if (Key == Keys.Enter || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                int tmpCount;
                try
                {
                    string tmpTxt = pnlCurrent.GetTextBoxByName("tbCount").Text;
                    if (tmpTxt.Substring(tmpTxt.Length - 1, 1) == "-")
                    {
                        tmpTxt = tmpTxt.Substring(0, tmpTxt.Length - 1);
                    }
                    tmpCount = Convert.ToInt32(tmpTxt);
                }
                catch
                {
                    tmpCount = 0;
                }
                lblAction.Text = "Обработка корректировки...";
                Refresh();
                if (SS.CompleteCorrectSample(ChoiseCorrect, tmpCount))
                {
                    View();
                    GoodVoice();
                }
                else
                {
                    lblAction.Text = SS.ExcStr;
                }
            }
        } //RKSampleSetCorrect
        private void RKSetCorrect(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape || (Key == Keys.D0 && ChoiseCorrect == 0))
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }

            if (ChoiseCorrect == 0)
            {
                ChoiseCorrect = Helper.WhatInt(Key);
                ReView();
                if (ChoiseCorrect > 0 && ChoiseCorrect < 4)
                {
                    lblAction.Text = "Укажите количество в штуках";
                }
                else
                {
                    ChoiseCorrect = 0;
                }
            }
            else if (Key == Keys.Enter || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                int tmpCount;
                try
                {
                    string tmpTxt = pnlCurrent.GetTextBoxByName("tbCount").Text;
                    if (tmpTxt.Substring(tmpTxt.Length - 1, 1) == "-")
                    {
                        tmpTxt = tmpTxt.Substring(0, tmpTxt.Length - 1);
                    }
                    tmpCount = Convert.ToInt32(tmpTxt);
                }
                catch
                {
                    tmpCount = 0;
                }
                lblAction.Text = "Обработка корректировки...";
                Refresh();
                if (SS.CompleteCorrect(ChoiseCorrect, tmpCount))
                {
                    View();
                    GoodVoice();
                }
                else
                {
                    lblAction.Text = SS.ExcStr;
                }
            }
        } // RKSetCorrect
        private void RKChoiseDown(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape || Key == Keys.D0)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.Enter || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                lblAction.Text = "Обновляю список...";
                Refresh();
                SS.ToModeChoiseDown();
                if (SS.Employer.CanDown && (SS.Employer.CanComplectation || !SS.Employer.CanComplectation))
                {
                    ModeChoiseDownView();
                }
            }

            int Choise = Helper.WhatInt(Key);
            if (Choise > 0 && Choise < 8)
            {
                lblAction.Text = "Получаю задание...";
                Refresh();
                if (!SS.ChoiseDownComplete(Choise))
                {
                    lblAction.Text = SS.ExcStr;
                    BadVoice();
                }
                else
                {
                    GoodVoice();
                }
                View();
            }
            else if (Choise == 8 && SS.Employer.CanComplectation)
            {
                lblAction.Text = "Получаю задание...";
                Refresh();
                if (!SS.NewComplectationGetFirstOrder())
                {
                    BadDone();
                    lblAction.Text = SS.ExcStr;
                    return;
                }
                GoodDone();
            }
            else if (Choise == 9 && SS.Employer.CanComplectation)
            {
                //отгрузка пока не работает
            }
        }
        private void RKDown(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!TimerFind.Enabled)
                {
                    lblAction.ForeColor = Color.DarkGreen;
                    lblAction.Text = "Нажмите 'ENT' для отказа от задания...";
                    Refresh();
                    TimerFind.Enabled = true;
                }
                return;
            }
            else if (Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                int remain = SS.DocDown.AllBoxes - SS.DocDown.Boxes;
                if (SS.DocDown.MaxStub <= remain)
                {
                    //Можно завершить
                    lblAction.Text = "Закрываю остальные " + remain.ToString() + " места...";
                    Refresh();
                    SS.EndCC();
                    View();
                }
            }
            else if (Key == Keys.Enter && TimerFind.Enabled)
            {
                lblAction.ForeColor = Color.Blue;
                lblAction.Text = "Отказ от задания...";
                Refresh();
                if (!SS.RepealDown())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
            }
        }
        private void RKDownComplete(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }
            else if (Key == Keys.Enter || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                if (!SS.FlagPrintPallete)
                {
                    lblAction.Text = "Печатаю паллет...";
                    Refresh();
                    SS.PrintPallete();
                    ReView();
                }
            }
        }
        private void RKNewComplectation(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!TimerFind.Enabled)
                {
                    lblAction.ForeColor = Color.DarkGreen;
                    lblAction.Text = "Нажмите 'ENT' для отказа от задания...";
                    Refresh();
                    TimerFind.Enabled = true;
                }
                return;
            }
            else if (Key == Keys.Enter && TimerFind.Enabled)
            {
                TimerFind.Enabled = false;
                lblAction.ForeColor = Color.Blue;
                lblAction.Text = "Отказ от задания...";
                Refresh();
                if (!SS.RepealNewComplectation())
                {
                    lblAction.Text = SS.ExcStr;
                    BadVoice();
                }
                else
                {
                    View();
                }
            }
            else if (Key == Keys.Left && Screan == 0)
            {
                TimerFind.Enabled = false;
                string tmp = lblAction.Text;
                lblAction.Text = "Подгружаю состояние...";
                Refresh();
                if (!SS.LoadCC())
                {
                    lblAction.Text = SS.ExcStr;
                    return;
                }
                Screan += 1;    //обязательно до View()
                lblAction.Text = tmp;
                View();
                pnlCurrent.Sweep(1);
            }
            else if (Key == Keys.Right && Screan == 1)
            {
                pnlCurrent.Sweep(-1);
                Screan -= 1;
            }
            else if (Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                int remain = SS.DocDown.AllBoxes - SS.DocDown.Boxes;
                if (SS.DocDown.MaxStub <= remain)
                {
                    //Можно завершить
                    lblAction.Text = "Закрываю остальные " + remain.ToString() + " места...";
                    Refresh();
                    SS.EndCCNewComp();
                    View();
                }
            }
            //Прокрутка секций
            else if (Key.GetHashCode() == 40)
            {
                (pnlCurrent.GetControlByName("pnlNewInfo") as ExPanel).MoveControls(0, -30);
            }
            else if (Key.GetHashCode() == 38)
            {
                (pnlCurrent.GetControlByName("pnlNewInfo") as ExPanel).MoveControls(0, +30);
            }
        }
        private void RKNewComplectationComplete(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (SS.NeedAdressComplete != SQL1S.GetVoidID() && TimerFind.Enabled)
                {
                    lblAction.ForeColor = Color.Blue;
                    lblAction.Text = "Адрес полон, фиксирую...";
                    Refresh();
                    if (!SS.AdressFull())
                    {
                        lblAction.Text = SS.ExcStr;
                    }
                    else
                    {
                        View();
                    }
                }
            }
            else if (Key == Keys.Right && Screan == 0)
            {
                TimerFind.Enabled = false;
                SS.ScaningBox = null;
                string tmp = lblAction.Text;
                lblAction.Text = "Подгружаю маршрут...";
                Refresh();
                SS.RefreshRoute();
                View();
                DataGrid dgRoute = pnlCurrent.GetDataGridByName("dgRoute");
                try
                {
                    DataGridCell currCell = dgRoute.CurrentCell;
                    currCell.ColumnNumber = 2;
                    dgRoute.CurrentCell = currCell;
                    dgRoute.CurrentRowIndex = CurrLineSet;
                }
                catch
                {
                }
                pnlCurrent.Sweep(-1);
                Screan -= 1;
                dgRoute.Focus();
                lblAction.Text = tmp;
            }
            else if (Key == Keys.Left && Screan == 0)
            {
                TimerFind.Enabled = false;
                SS.ScaningBox = null;
                string tmp = lblAction.Text;
                lblAction.Text = "Подгружаю состояние...";
                Refresh();
                if (!SS.LoadCC())
                {
                    lblAction.Text = SS.ExcStr;
                    return;
                }
                Screan += 1;    //обязательно до View()
                lblAction.Text = tmp;
                View();
                pnlCurrent.Sweep(1);
            }
            else if (Key == Keys.Right && Screan == 1)
            {
                pnlCurrent.Sweep(-1);
                Screan -= 1;
            }
            else if (Key == Keys.Left && Screan == -1)
            {
                DataGrid dgRoute = pnlCurrent.GetDataGridByName("dgRoute");
                CurrLineSet = dgRoute.CurrentRowIndex;
                pnlCurrent.Sweep(1);
                Screan += 1;
            }
            else if (Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189)
            {
                if (SS.LastGoodAdress != null)
                {
                    //Можно завершить
                    lblAction.Text = "Комплектую остальные...";
                    Refresh();
                    if (SS.CompleteAll())
                    {
                        GoodVoice();
                    }
                    else
                    {
                        BadVoice();
                    }
                    View();
                }
            }
            //Прокрутка секций
            else if (Key.GetHashCode() == 40)
            {
                (pnlCurrent.GetControlByName("pnlNewInfo") as ExPanel).MoveControls(0, -30);
            }
            else if (Key.GetHashCode() == 38)
            {
                (pnlCurrent.GetControlByName("pnlNewInfo") as ExPanel).MoveControls(0, +30);
            }
        }
        private void RKFreeDownComplete(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
            }
            if (Key == Keys.Left && Screan == 0)
            {
                TimerFind.Enabled = false;
                string tmp = lblAction.Text;
                lblAction.Text = "Подгружаю состояние...";
                Refresh();
                if (!SS.LoadCC())
                {
                    lblAction.Text = SS.ExcStr;
                    return;
                }
                Screan += 1;    //обязательно до View()
                lblAction.Text = tmp;
                View();
                pnlCurrent.Sweep(1);
            }
            else if (Key == Keys.Right && Screan == 1)
            {
                pnlCurrent.Sweep(-1);
                Screan -= 1;
            }
            //Прокрутка секций
            else if (Key.GetHashCode() == 40)
            {
                (pnlCurrent.GetControlByName("pnlNewInfo") as ExPanel).MoveControls(0, -30);
            }
            else if (Key.GetHashCode() == 38)
            {
                (pnlCurrent.GetControlByName("pnlNewInfo") as ExPanel).MoveControls(0, +30);
            }
        }
        private void RKSetSelfControl(Keys Key, Control currControl)
        {
            if (Key == Keys.Escape)
            {
                if (!SS.ReactionCancel())
                {
                    lblAction.Text = SS.ExcStr;
                }
                else
                {
                    View();
                }
                return;
            }

            if (Key == Keys.Right && Screan == 0)
            {
                //Вернем текущую строку
                DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
                try
                {
                    DataGridCell currCell = dgGoodsCC.CurrentCell;
                    currCell.ColumnNumber = 2;
                    dgGoodsCC.CurrentCell = currCell;
                    dgGoodsCC.CurrentRowIndex = CurrLineSet;
                }
                catch
                {
                }
                pnlCurrent.Sweep(-1);
                Screan -= 1;
                dgGoodsCC.Focus();
            }
            else if (Key == Keys.Left && Screan == -1)
            {
                DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
                CurrLineSet = dgGoodsCC.CurrentRowIndex;
                pnlCurrent.Sweep(1);
                Screan += 1;
                TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
                if (tbCount.Visible)
                {
                    pnlCurrent.GetControlByName("tbCount").Focus();
                }
            }
            else if (Screan == 0 && (Key == Keys.Enter || Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189) && SS.CurrentAction == ActionSet.EnterCount)
            {
                int tmpCount;
                try
                {
                    string tmpTxt = pnlCurrent.GetTextBoxByName("tbCount").Text;
                    if (tmpTxt.Substring(tmpTxt.Length - 1, 1) == "-")
                    {
                        tmpTxt = tmpTxt.Substring(0, tmpTxt.Length - 1);
                    }
                    tmpCount = Convert.ToInt32(tmpTxt);
                }
                catch
                {
                    tmpCount = 0;
                }
                if (SS.EnterCountSetSelfControl(tmpCount))
                {
                    GoodDone();
                }
                else
                {
                    BadDone();
                }
                pnlCurrent.GetTextBoxByName("tbCount").Text = "";   //В любом случае очистим
            }
        }
        private int tbCountGet()
        {
            int tmpCount;
            try
            {
                string tmpTxt = pnlCurrent.GetTextBoxByName("tbCount").Text;
                if (tmpTxt.Substring(tmpTxt.Length - 1, 1) == "-")
                {
                    tmpTxt = tmpTxt.Substring(0, tmpTxt.Length - 1);
                }
                tmpCount = Convert.ToInt32(tmpTxt);
            }
            catch
            {
                tmpCount = 0;
            }
            return tmpCount;
        } // tbCountGet
        private void RKRefillSet(Keys Key, Control currControl)
        {
            RefillSet Obj = SS.MM as RefillSet;
            Obj.InputedCount = tbCountGet();
        } // RKRefillSet
        private void RKRefillLayout(Keys Key, Control currControl)
        {
            RefillLayout Obj = SS.MM as RefillLayout;
            Obj.InputedCount = tbCountGet();
        } // RKRefillLayout
        private void RKRefillSetCorrect(Keys Key, Control currControl)
        {
            RefillSet Obj = SS.MM as RefillSetCorrect;
            Obj.InputedCount = tbCountGet();
        } // RKRefillSet

        private void ReactionKey(Keys Key)
        {
            Control currControl = pnlCurrent.GetFocused();
            //Дабы не было косяков с неопределненным фокусным объектом...
            //ЭТО ЧУДОВИЩЬНО, но я чето очкую это удалять...
            if (currControl == null)
            {
                currControl = new Control();
            }
            switch (SS.CurrentMode)
            {
                case Mode.FreeDownComplete:
                    RKFreeDownComplete(Key, currControl);
                    break;
                case Mode.NewComplectation:
                    RKNewComplectation(Key, currControl);
                    break;
                case Mode.NewComplectationComplete:
                    RKNewComplectationComplete(Key, currControl);
                    break;
                case Mode.SetCorrect:
                    RKSetCorrect(Key, currControl);
                    break;
                case Mode.DownComplete:
                    RKDownComplete(Key, currControl);
                    break;
                case Mode.Set:
                    RKSet(Key, currControl);
                    break;
                case Mode.SetInicialization:
                    RKSetInicialization(Key, currControl);
                    break;
                case Mode.Acceptance:
                    RKAcceptence(Key, currControl);
                    break;
                case Mode.AcceptedItem:
                    RKAcceptenceItem(Key, currControl);
                    break;
                case Mode.TransferInicialize:
                    RKTransferInicialize(Key, currControl);
                    break;
                case Mode.Transfer:
                    RKTransfer(Key, currControl);
                    break;
                case Mode.NewInventory:
                    RKTransfer(Key, currControl);
                    break;
                case Mode.Inventory:
                    RKInventory(Key, currControl);
                    break;
                case Mode.SampleInventory:
                    RKInventory(Key, currControl);
                    break;
                case Mode.ChoiseInventory:
                    RKChoiseInventory(Key, currControl);
                    break;
                case Mode.SampleSet:
                    RKSampleSet(Key, currControl);
                    break;
                case Mode.SampleSetCorrect:
                    RKSampleSetCorrect(Key, currControl);
                    break;
                case Mode.SamplePut:
                    RKSamplePut(Key, currControl);
                    break;
                case Mode.NewChoiseInventory:
                    RKChoiseNewInventory(Key, currControl);
                    break;
                case Mode.HarmonizationInicialize:
                    RKHarmonizationInicialize(Key, currControl);
                    break;
                case Mode.Harmonization:
                    RKHarmonization(Key, currControl);
                    break;
                case Mode.HarmonizationPut:
                    RKHarmonization(Key, currControl);
                    break;
                case Mode.LoadingInicialization:
                    RKLoadingInicialization(Key, currControl);
                    break;
                case Mode.Loading:
                    RKLoading(Key, currControl);
                    break;
                case Mode.SetComplete:
                    RKSetCopmlete(Key, currControl);
                    break;
                case Mode.ChoiseDown:
                    RKChoiseDown(Key, currControl);
                    break;
                case Mode.Down:
                    RKDown(Key, currControl);
                    break;
                case Mode.SetSelfContorl:
                    RKSetSelfControl(Key, currControl);
                    break;
                default:
                    if (SS.NewStructModes.Contains(SS.CurrentMode))
                    {
                        //НОВАЯ СТРУКТУРА. Для начала посмотрим, возможно нужны какие-то подготовительные действия, типа контролер
                        switch (SS.MM.CurrentMode)
                        {
                            case Mode.RefillSet:
                                RKRefillSet(Key, currControl);
                                break;
                            case Mode.RefillSetCorrect:
                                RKRefillSetCorrect(Key, currControl);
                                break;
                            case Mode.RefillLayout:
                                RKRefillLayout(Key, currControl);
                                break;
                        }
                        SS.ReactionKey(Key);
                    }
                    else if (Key == Keys.Escape)
                    {
                        SS.ReactionCancel();
                    }
                    break;
            }
        } // ReactionKey

        private void ShowAddParty()
        {
            if (SS.OkRowIndex == -1)
            {
                Label lblResult = pnlCurrent.GetLabelByName("lblResult");
                Button btnOk = pnlCurrent.GetControlByName("btnOk") as Button;
                btnOk.Visible = true;
                lblResult.Visible = true;
                lblResult.BringToFront();
                btnOk.BringToFront();
                lblResult.Text = SS.ExcStr;
            }
            if (SS.CurrentLine != -1)
            {
                pnlCurrent.GetDataGridByName("dgTransferItems").DataSource = SS.TransferItems;
                pnlCurrent.GetDataGridByName("dgTransferItems").CurrentRowIndex = SS.CurrentLine;
                if (SS.OkRowIndex == SS.CurrentLine)
                {
                    //Это подтверждение позиции, а не ее добавление, включаем разрешающий таймер
                    TimerFind.Enabled = true;
                }
            }
            else
            {
                pnlCurrent.GetDataGridByName("dgTransferItems").DataSource = SS.TransferItems;
                if (SS.TransferItems.Rows.Count > 0)
                {
                    pnlCurrent.GetDataGridByName("dgTransferItems").CurrentRowIndex = SS.TransferItems.Rows.Count - 1;
                }
            }
            ReView();
            lblAction.Text = SS.ExcStr;
        } // ShowAddParty

        private void MakeSound(Voice voice)
        {
            if (voice == Voice.Good)
            {
                GoodVoice();
            }
            else if (voice == Voice.Bad)
            {
                BadVoice();
            }
        }

        private void GoodDone()
        {
            lblAction.ForeColor = Color.Blue;
            GoodVoice();
            ReView();
            Refresh();
        }
        private void BadDone()
        {
            lblAction.ForeColor = Color.Red;
            BadVoice();
            ReView();
            Refresh();
        }
        private void ReactionBarcode(object sender, BarcodeReadEventArgs e)
        {
            string strBarcode = e.Barcode;
            //Особые режимы у которых все просто
            if (SS.NewStructModes.Contains(SS.CurrentMode))
            {
                SS.BarcodeReaction(strBarcode);
                return;
            }
            Dictionary<string, string> dicBarcode = Helper.DisassembleBarcode(strBarcode);
            if ((SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.SampleSet) && Screan != 0)
            {
                lblAction.Text = "ШК не работают на данном экране!";
                return;
            }
            if (SS.CurrentMode == Mode.SetInicialization || SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Loading || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete)
            {
                lblAction.ForeColor = Color.Blue;
            }
            lblAction.Text = "ШК принят, обработка...";
            Refresh();

            bool IsObject = true;
            if (strBarcode.Substring(0, 2) == "25" && dicBarcode["Type"] == "113")
            {
                #region EAN 13 Reference 25 type 113
                if (!SS.IsSC(dicBarcode["IDD"], "Сотрудники"))
                {
                    if (!SS.IsSC(dicBarcode["IDD"], "Секции"))
                    {
                        if (!SS.IsSC(dicBarcode["IDD"], "Принтеры"))
                        {
                            IsObject = false;
                        }
                    }
                }

                if (IsObject)
                {
                    if (SS.CurrentMode == Mode.AcceptedItem)
                    {
                        //Не спрашивайте меня зачем это здесь...
                        DynamicLostFocus(pnlCurrent.GetFocused(), null);
                    }
                    if (SS.ReactionSC(dicBarcode["IDD"]))
                    {
                        View();
                        if (SS.CurrentMode == Mode.SampleSet && SS.ExcStr != null)
                        {
                            lblAction.Text = SS.ExcStr;
                        }
                        else if (SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete)
                        {
                            GoodDone();
                            return;
                        }
                        else if (SS.CurrentMode != Mode.DownComplete)
                        {
                            lblAction.Text = "Ожидание команды";
                        }
                    }
                    else
                    {
                        if (SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete)
                        {
                            BadDone();
                            return;
                        }
                        else
                        {
                            lblAction.Text = SS.ExcStr;
                        }
                    }
                    return;
                }
                #endregion
            }
            else if (strBarcode.Substring(0, 2) == "26" && dicBarcode["Type"] == "113")
            {
                #region EAN 13 Reference 26 type 113
                string tmpIDDoc;
                string tmpDocType;
                IsObject = SS.GetDoc(dicBarcode["IDD"], out tmpIDDoc, out tmpDocType);
                if (IsObject)
                {
                    if (SS.ReactionDoc(dicBarcode["IDD"]))
                    {
                        View();
                        if (SS.CurrentMode != Mode.ControlCollect)
                        {
                            lblAction.Text = "Ожидание команды";
                        }
                        else
                        {
                            lblAction.Text = SS.ExcStr;
                        }
                    }
                    else
                    {
                        if (SS.CurrentMode == Mode.SetInicialization || SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete)
                        {
                            BadDone();
                            return;
                        }
                        else
                        {
                            lblAction.Text = SS.ExcStr;
                        }
                    }
                    return;
                }
                #endregion
            }
            else if (dicBarcode["Type"] == "part" && (SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.SampleSet))
            {
                //отcканировали количество деталей
                //Количество деталей
                if (SS.ScanPartBarcode(Convert.ToInt32(dicBarcode["count"])))
                {
                    GoodDone();
                }
                else
                {
                    BadDone();
                }
                return;
            }
            else if (dicBarcode["Type"] == "pallete" && SS.CurrentMode == Mode.Acceptance)
            {
                //отcканировали паллету надо запомнить ее ID
                if (SS.ScanPalletBarcode(strBarcode))
                {
                    GoodDone();
                }
                else
                {
                    BadDone();
                }
                return;                
            }
            else if (dicBarcode["Type"] == "pallete" && SS.CurrentMode == Mode.Transfer)
            {
                //отcканировали паллету надо запомнить ее ШК дальше он нам пригодится
                SS.FBarcodePallet = strBarcode;                
              
            }

            Mode tmpMode = SS.CurrentMode;
            if (SS.CurrentMode == Mode.Transfer && Screan == 1)
            {
                //Не доспускаем сканирование в экране разнесенных товаров
                lblAction.Text = "Нет действий с этим штирхкодом в данном экране! Переместитесь в экран ТЕЛЕЖКА!";
                return;
            }
            if (SS.ReactionBarcode(strBarcode, Screan))
            {
                #region reaction any barcode
                if (SS.CurrentMode == Mode.AcceptedItem && tmpMode == Mode.AcceptedItem)
                {
                    if (SS.AcceptedItem.ToMode == Mode.Transfer || SS.AcceptedItem.ToMode == Mode.NewInventory || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut)
                    {
                        lblAction.Text = "В данном режиме нельзя менять ШК!";
                        return;
                    }
                    else
                    {
                        if (!SS.ChangeUnitBarcode((int)ViewUnits.Rows[CurrentRowItem]["Coef"], strBarcode,
                                ViewUnits.Rows[CurrentRowItem]["OKEI"].ToString()))
                        {
                            lblAction.Text = SS.ExcStr;
                            return;
                        }
                    }
                }
                if (SS.CurrentMode == Mode.SampleSet || SS.CurrentMode == Mode.SamplePut)
                {
                    lblAction.Text = SS.ExcStr;
                }
                View();
                if (SS.CurrentMode == Mode.AcceptedItem || SS.CurrentMode == Mode.ControlCollect)
                {
                    lblAction.Text = SS.ExcStr;
                }
                else if (SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete || SS.CurrentMode == Mode.SampleSet)
                {
                    GoodDone();
                    return;
                }
                else
                {
                    lblAction.Text = "Ожидание команды";
                }
                #endregion
            }
            else
            {
                if (dicBarcode["Type"] == "5")
                {
                    if (SS.CurrentMode == Mode.Transfer || SS.CurrentMode == Mode.NewInventory || SS.CurrentMode == Mode.Acceptance)
                    {
                        if (SS.AddPartyParty(dicBarcode["IDD"], Convert.ToInt32(dicBarcode["LineNo"])))
                        {
                            View();
                        }
                    }
                }
                else if (dicBarcode["Type"] == "6" && (SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.Loading || SS.CurrentMode == Mode.ControlCollect || SS.CurrentMode == Mode.SampleSet || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete))
                {
                    if (SS.ReactionSC(dicBarcode["ID"], true))
                    {
                        if (SS.CurrentMode == Mode.Loading)
                        {
                            if (SS.colorSwitcher)
                            {
                                lblAction.ForeColor = Color.DarkGreen;
                            }
                            else
                            {
                                lblAction.ForeColor = Color.Blue;
                            }
                            GoodVoice();
                            ViewProposal = null;
                            ReView();
                            Refresh();
                        }
                        else if (SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete)
                        {
                            GoodDone();
                            return;
                        }
                        else
                        {
                            View();
                        }
                    }
                    else
                    {
                        if (SS.CurrentMode == Mode.Loading)
                        {
                            BadVoice();
                            lblAction.ForeColor = Color.Red;
                        }
                        else if (SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete)
                        {
                            BadDone();
                            return;
                        }
                    }
                }
                if (SS.ExcStr == null)
                {
                    lblAction.Text = "Ожидание команды";
                }
                else
                {
                    if (SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete)
                    {
                        BadDone();
                        return;
                    }
                    lblAction.Text = SS.ExcStr;
                }
            }
        } // ReactionBarcode

        private void DBOCAcceptedItem(Button currBtn)
        {
            if ((SS.AcceptedItem.ToMode == Mode.Transfer || SS.AcceptedItem.ToMode == Mode.NewInventory || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut) && SS.AcceptedItem.OnShelf)
            {
                //Это выкладка на полку, делаем только через сканирование адреса, а тут забиваем ржавый гвоздь
                return;
            }
            if (SS.AcceptedItem.ToMode == Mode.SampleInventory || SS.AcceptedItem.ToMode == Mode.SamplePut)
            {
                DataRow[] DR = SS.Units.Select("OKEI = '" + SS.OKEIUnit + "' and not Barcode = ''");
                if (DR.Length == 0)
                {
                    DialogResult DialRes = MessageBox.Show("Не задан штрихкод единицы! Он будет сгенерирован! Сгенерировать?", "Генерация единиц", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (DialRes == DialogResult.No || DialRes == DialogResult.Cancel)
                    {
                        return;
                    }
                    SS.AcceptedItem.GenerateBarcode = true;
                }
            }
            if (SS.CompleteAccept())
            {
                if (SS.AcceptedItem.ToMode == Mode.SamplePut)
                {
                    Screan = 0;
                    lblAction.Text = SS.ExcStr;
                }
                else if (SS.AcceptedItem.Acceptance)
                {
                    Screan = 1;
                }
                else
                {
                    Screan = -1;
                }
                View();
            }
            else
            {
                lblAction.Text = SS.ExcStr;
            }
        }
        private void DBOCTransfer(Button currBtn)
        {
            if (currBtn.Name == "btnPrint")
            {
                lblAction.Text = "Команда в обработке, подождите...";
                Refresh();
                if (SS.CompleteTransfer())
                {
                    View();
                }
                else
                {
                    lblAction.Text = SS.ExcStr;
                }
            }
        }
        private void DBOCHarmonization(Button currBtn)
        {
            if (currBtn.Name == "btnPrint")
            {
                lblAction.Text = "Команда в обработке, подождите...";
                Refresh();
                if (SS.CompleteHarmonization())
                {
                    View();
                }
                else
                {
                    lblAction.Text = SS.ExcStr;
                }
            }
        }
        private void DBOCHarmonizationPut(Button currBtn)
        {
            if (currBtn.Name == "btnPrint")
            {
                lblAction.Text = "Команда в обработке, подождите...";
                Refresh();
                if (SS.CompleteHarmonizationPut())
                {
                    View();
                }
                else
                {
                    lblAction.Text = SS.ExcStr;
                }
            }
        }
        private void DBOCNewInventory(Button currBtn)
        {
            if (currBtn.Name == "btnPrint")
            {
                lblAction.Text = "Команда в обработке, подождите...";
                Refresh();
                if (SS.CompleteNewInventory())
                {
                    View();
                }
                else
                {
                    lblAction.Text = SS.ExcStr;
                }
            }
        }
        private void DBOCSamplePut(Button currBtn)
        {
            lblAction.Text = "Команда в обработке, подождите...";
            Refresh();
            if (currBtn.Name == "btnPrint")
            {
                if (SS.CompleteSamplePut())
                {
                    lblAction.Text = "Выкладка образцов зафиксирована успешно!";
                    pnlCurrent.GetTextBoxByName("tbFind").Text = "";
                    pnlCurrent.GetControlByName("dgSamples").Focus();
                    View();
                    pnlCurrent.Sweep(1);
                    Screan -= 1;
                }
                else
                {
                    lblAction.Text = SS.ExcStr;
                }
            }
            else
            {
                //btnPrintPrices - печать ценников
                SS.PrintPrices();
                lblAction.Text = SS.ExcStr;
            }
        }
        private void DynamicButtonOnClick(object sender, EventArgs e)
        {
            Button currBtn = sender as Button;

            switch (SS.CurrentMode)
            {
                case Mode.HarmonizationInicialize:
                    lblAction.Text = "Выполняю запрос, подождите...";
                    Refresh();
                    if (SS.RequestHarmonization())
                    {
                        View();
                    }
                    else
                    {
                        lblAction.Text = SS.ExcStr;
                    }
                    break;
                case Mode.TransferInicialize:
                    if (SS.CompleteTransferInicialize(
                            SS.WarehousesFrom.Rows[pnlCurrent.GetDataGridByName("dgFrom").CurrentRowIndex]["ID"].ToString(),
                            SS.WarehousesTo.Rows[pnlCurrent.GetDataGridByName("dgTo").CurrentRowIndex]["ID"].ToString()))
                    {
                        View();
                    }
                    else
                    {
                        lblAction.Text = SS.ExcStr;
                    }
                    break;
                case Mode.AcceptedItem:
                    DBOCAcceptedItem(currBtn);
                    break;
                case Mode.Acceptance:
                    if (currBtn.Name == "btnPrint" || currBtn.Name == "btnPrintCondition")
                    {
                        lblAction.Text = "Команда печати в обработке, подождите...";
                        Refresh();
                        SS.PrintLabels((currBtn.Name == "btnPrint" ? false : true));
                        if (Screan == 1)
                        {
                            pnlCurrent.MoveControls(2 * pnlCurrent.Width, 0);
                            Screan = -1;
                            pnlCurrent.GetControlByName("tbFind").Focus();
                        }
                        View();
                        lblAction.Text = SS.ExcStr;
                    }
                    break;

                case Mode.Transfer:
                    DBOCTransfer(currBtn);
                    break;

                case Mode.Harmonization:
                    DBOCHarmonization(currBtn);
                    break;

                case Mode.HarmonizationPut:
                    DBOCHarmonizationPut(currBtn);
                    break;

                case Mode.NewInventory:
                    DBOCNewInventory(currBtn);
                    break;

                case Mode.ChoiseInventory:
                    if (SS.CompleteChoiseInventory(SS.WarehousesTo.Rows[pnlCurrent.GetDataGridByName("dgTo").CurrentRowIndex]["ID"].ToString()))
                    {
                        View();
                    }
                    else
                    {
                        lblAction.Text = SS.ExcStr;
                    }
                    break;
                case Mode.NewChoiseInventory:
                    lblAction.Text = "Выполняю запрос задания, подождите...";
                    Refresh();
                    if (SS.RequestJob())
                    {
                        View();
                    }
                    else
                    {
                        lblAction.Text = SS.ExcStr;
                    }
                    break;
                case Mode.SamplePut:
                    DBOCSamplePut(currBtn);
                    break;
                case Mode.LoadingInicialization:
                    if (SS.CompleteLoadingInicialization())
                    {
                        View();
                    }
                    else
                    {
                        lblAction.Text = SS.ExcStr;
                    }
                    break;
                case Mode.Loading:
                    if (SS.CompleteLodading())
                    {
                        View();
                    }
                    else
                    {
                        lblAction.Text = SS.ExcStr;
                    }
                    break;
            }
        }

        private void DynamicLostFocus(object sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            string senderName = (sender as Control).Name;
            if (senderName == "tbLabelCount")
            {
                //Изменим число этикеток в текущей строке
                DataGrid currDG = pnlCurrent.GetDataGridByName("dgAcceptedItems");
                TextBox tbLabelCount = sender as TextBox;
                if (currDG.CurrentRowIndex != -1)
                    if (SS.ChangeLabelCount(currDG.CurrentRowIndex, Convert.ToInt32((tbLabelCount.Text == "" ? "0" : tbLabelCount.Text))))
                    {
                        currDG.Focus();
                    }
                    else
                    {
                        lblAction.Text = SS.ExcStr;
                    }
            }
            if (SS.CurrentMode == Mode.AcceptedItem)
            {
                if (senderName == "tbCoef")
                {
                    string textTB = (sender as TextBox).Text;
                    if (LastTBText == textTB)
                    {
                        return; //Если текст не менялся, то пох
                    }
                    textTB = (textTB == "" ? "0" : textTB);
                    ViewUnits.Rows[CurrentRowItem]["Coef"] = Convert.ToInt32(textTB);
                    SS.ChangeUnitCoef((int)ViewUnits.Rows[CurrentRowItem]["Coef"],
                                ViewUnits.Rows[CurrentRowItem]["Barcodes"].ToString().TrimEnd(),
                                ViewUnits.Rows[CurrentRowItem]["OKEI"].ToString());
                    View();
                }
                else if (senderName == "tbCount")
                {
                    int Coef = (int)ViewUnits.Rows[CurrentRowItem]["Coef"];
                    string textTB = (sender as TextBox).Text;
                    (sender as TextBox).ForeColor = Color.Black;
                    if (LastTBText == textTB)
                    {
                        return; //Если текст не менялся, то пох
                    }
                    if (Coef != 0)
                    {
                        textTB = (textTB == "" ? "0" : textTB);
                        int NewAcceptCount = Convert.ToInt32(textTB) * Coef;
                        SS.AcceptedItem.AcceptCount = (NewAcceptCount == 0 ? SS.AcceptedItem.AcceptCount : NewAcceptCount);
                    }
                    View();
                }
                else if (senderName == "tbDetails")
                {
                    string textTB = (sender as TextBox).Text;
                    textTB = (textTB == "" ? "0" : textTB);
                    int Det = Convert.ToInt32(textTB);
                    if (Det >= 0 && Det < 99)
                    {
                        SS.AcceptedItem.NowDetails = Det;
                    }
                    (sender as TextBox).Text = SS.AcceptedItem.NowDetails.ToString();
                }
            }
        }
        private void DynamicGotFocus(object sender, EventArgs e)
        {
            if (sender == null)
            {
                return;
            }
            string senderName = (sender as Control).Name;
            if (senderName == "tbCount")
            {
                int Coef = (int)ViewUnits.Rows[CurrentRowItem]["Coef"];
                int Count = (int)(Coef == 0 ? 0 : SS.AcceptedItem.AcceptCount / Coef);
                TextBox currTB = sender as TextBox;
                currTB.Text = (Count == 0 ? "" : Count.ToString());
                currTB.Select(currTB.Text.Length, 0);
                LastTBText = currTB.Text;
                if (Coef != 0)
                {
                    if (Count * Coef != SS.AcceptedItem.AcceptCount)
                    {
                        currTB.ForeColor = Color.Red;
                    }
                    else
                    {
                        currTB.ForeColor = Color.Black;
                    }
                }
            }
            else if (senderName == "tbCoef")
            {
                TextBox currTB = sender as TextBox;
                currTB.Text = ((int)ViewUnits.Rows[CurrentRowItem]["Coef"] == 0 ? "" : ViewUnits.Rows[CurrentRowItem]["Coef"].ToString());
                currTB.Select(currTB.Text.Length, 0);
                LastTBText = currTB.Text;
            }
            else if (senderName == "tbDetails")
            {
                TextBox currTB = sender as TextBox;
                currTB.Text = SS.AcceptedItem.NowDetails.ToString();
                currTB.Select(currTB.Text.Length, 0);
            }
        }
        private void DynamicOnKeyPress(object sender, KeyPressEventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb.Name == "tbCoef" && CurrentRowItem == 0)
            {
                //Коэффициент единиц нельзя редактировать вообще
                e.Handled = true;
                return;
            }
            //В карточке товара, если открыта фотография, то не будем ничего засерать, чтобы не было проблем
            if (SS.CurrentMode == Mode.AcceptedItem)
            {
                PictureBox pbPhoto = pnlCurrent.GetPictureBoxByName("pbPhoto");
                if (pbPhoto.Visible)
                {
                    e.Handled = true;
                    return;
                }
            }
            //Обычное дело, поля в которые нельзя вставлять ничего кроме цифр
            if (tb.Name == "tbFindItem" || tb.Name == "tbFind" || tb.Name == "tbCoef" || tb.Name == "tbCount" || tb.Name == "tbLabelCount" || tb.Name == "tbDetails")
            {
                if (!((e.KeyChar >= '0' && e.KeyChar <= '9') || e.KeyChar == (Char)Keys.Back || e.KeyChar == (Char)Keys.Delete))
                {
                    e.Handled = true;
                    return;
                }
            }
        }
        private void DynamicOnTextChanged(object sender, EventArgs e)
        {
            TextBox currTB = sender as TextBox;
            if (currTB.Name == "tbFind")
            {
                TimerFind.Enabled = true;
            }
        }

        #region function reaction on click, barcode, key and other...
        /// <summary>
        /// Обработка нажатия клавиши
        /// используется для перехвата кода вводимого со сканера (в разрыв клавиатуры)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FMainOnKeyDown(object sender, KeyEventArgs e)
        {
            DGCellChange = false;
            Br.KeyDown(e);
        }
        /// <summary>
        /// handle global key down event ("KeyDown" event form)
        /// </summary>
        /// <param name="Key">down key</param>
        private void HotKeyEvent(object sender, KeyEventArgs Key)
        {
            ReactionKey(Key.KeyData);
        }
        #endregion

        private void TimerFind_Tick(object sender, EventArgs e)
        {
            TimerFind.Enabled = false;
            if (SS.CurrentMode == Mode.Acceptance)
            {
                TextBox currTB = pnlCurrent.GetTextBoxByName("tbFind");
                BindingSource BS = pnlCurrent.GetDataGridByName("dgNotAcceptedItems").DataSource as BindingSource;
                BS.Filter = "ArticleFind LIKE '" + currTB.Text + "*'"
                        + " or ArticleOnPackFind LIKE '" + currTB.Text + "*'"
                        + " or ItemNameFind LIKE '" + currTB.Text + "*'";
            }
            else if (SS.CurrentMode == Mode.Transfer)
            {
                SS.OkRowIndex = -1;
                lblAction.ForeColor = Color.Blue;
                lblAction.Text = "Ожидание команды";
            }
            else if (SS.CurrentMode == Mode.SamplePut)
            {
                if (SS.SampleItems.Rows.Count != 0)
                {
                    TextBox currTB = pnlCurrent.GetTextBoxByName("tbFind");
                    BindingSource BS = pnlCurrent.GetDataGridByName("dgSamples").DataSource as BindingSource;
                    BS.Filter = "InvCodeFind LIKE '" + currTB.Text + "*'";
                }
            }
            else if (SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete)
            {
                if (SS.CurrentMode == Mode.NewComplectationComplete)
                {
                    Label lblInfo2 = pnlCurrent.GetLabelByName("lblInfo2");
                    lblInfo2.ForeColor = Color.Black;
                    SS.ScaningBox = null;
                    ReView();
                }
                lblAction.ForeColor = Color.Blue;
                lblAction.Text = "Ожидание команды";
            }
        }
        private void OnCurrentCellChanged(object sender, EventArgs e)
        {
            DGCellChange = true;
        }

        private void OnScanBox(object sender, EventArgs e)
        {
            Label lblNumber = pnlCurrent.GetLabelByName("lblNumber");
            Label lblSetter = pnlCurrent.GetLabelByName("lblSetter");
            Label lblKey2 = pnlCurrent.GetLabelByName("lblKey2");
            Label lblInfo2 = pnlCurrent.GetLabelByName("lblInfo2");
            if (SS.NeedAdressComplete != SQL1S.GetVoidID())
            {
                lblKey2.Visible = true;
            }
            lblInfo2.ForeColor = Color.Blue;
            lblInfo2.Text = "Ворота: " + SS.DocDown.Sector + "  " + SS.DocDown.Boxes.ToString() + " м";
            lblNumber.Text = SS.DocDown.NumberBill.Substring(SS.DocDown.NumberBill.Length - 5, 2) + " "
                                    + SS.DocDown.NumberBill.Substring(SS.DocDown.NumberBill.Length - 3)
                                    + " сектор: " + SS.DocDown.MainSectorName.Trim() + "-" + SS.DocDown.NumberCC.ToString();
            lblSetter.Text = SS.DocDown.AdressCollect;
            lblAction.Text = SS.ExcStr;
            lblAction.ForeColor = Color.Green;
            TimerFind.Enabled = true;
        }

        //private void OnChange

        private void GoodVoice()
        {
            for (int i = 0; i < 2; i++)
            {
                APIManager.MessageBeep(10000);
            }

        }
        private void BadVoice()
        {
            bool curr = false;
            for (int i = 0; i < 100; i++)
            {
                if (curr)
                {
                    APIManager.MessageBeep(1000000);
                }
                else
                {
                    APIManager.MessageBeep(100000);
                }
                curr = !curr;
            }
        }

    }//class FMain
}