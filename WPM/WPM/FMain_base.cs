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
        const string Vers = "4.48";     //Номер версии      
        // const string BaseName = "int9999001rab";
        const string BaseName = "int9999001ad1"; //debug_true
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
                        else if (SS.CurrentMode == Mode.AcceptanceCross && SS.ExcStr != null && (dicBarcode["Type"] == "pallete" || SS.IsSC(dicBarcode["IDD"], "Секции")))
                        {
                            lblAction.Text = SS.ExcStr;
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
                        lblAction.Text = SS.ExcStr;
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
            else if (dicBarcode["Type"] == "pallete")
            {
                //отсканированна паллета
                if (SS.CurrentMode == Mode.Acceptance)
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
                else if (SS.CurrentMode == Mode.AcceptedItem && SS.AcceptedItem.ToMode == Mode.AcceptanceCross)
                {
                    //если это кроссдокинг, надо проверить а нет ли текущих заказов на этой паллете
                    if (SS.EnterPalletAcceptanceCross(strBarcode))
                    {
                        lblAction.Text = SS.ExcStr;
                        GoodDone();
                    }
                    else
                    {
                        lblAction.Text = SS.ExcStr;
                        BadDone();
                    }
                    return;
                }
                else if (SS.CurrentMode == Mode.AcceptanceCross)
                {
                    //запишем данные в нее
                    string TextQuery =
                            "SELECT " +
                                "Pallets.ID as ID " +
                            "FROM " +
                                "$Спр.ПеремещенияПаллет as Pallets (nolock) " +
                            "WHERE " +
                                "Pallets.$Спр.ПеремещенияПаллет.ШКПаллеты = :Pallet ";
                    //            "AND Pallets.$Спр.ПеремещенияПаллет.ФлагОперации = 1 " +
                    //            "AND Pallets.$Спр.ПеремещенияПаллет.Адрес0 = :EmptyID ";
                    SQL1S.QuerySetParam(ref TextQuery, "EmptyDate", SQL1S.GetVoidID());
                    SQL1S.QuerySetParam(ref TextQuery, "Pallet", strBarcode);
                    DataTable DT;
                    SS.ExecuteWithRead(TextQuery, out DT);
                    if (DT.Rows.Count == 0)
                    {
                        lblAction.Text = "Паллета не найдена!";
                        BadDone();
                        return;
                    }
                    DataGrid currDG = pnlCurrent.GetDataGridByName("dgAcceptedItems");
                    currDG.CurrentRowIndex = 0;
                    bool findePallet = false;                            
                    foreach (DataRow DTdr in DT.Rows)
                    {
                        SS.FPalletID = DTdr["id"].ToString();
                        SS.FBarcodePallet = strBarcode;
                        SS.PalletAcceptedItem = new RefPalleteMove(SS);
                        SS.PalletAcceptedItem.FoundID(SS.FPalletID);
                        int num = 0;
                        foreach (DataRow dr in SS.AcceptedItems.Rows)
                        {
                            if (dr["PalletID"].ToString() == SS.FPalletID.ToString())
                            {
                                currDG.CurrentRowIndex = num;
                                findePallet = true;
                                break;
                            }
                            num++;
                        }
                        if (findePallet)
                        {
                            break;
                        }
                    }
                    currDG.Focus();
                    if (!findePallet)
                    {
                        //не нашли паллету в табличке
                        lblAction.Text = "Нет такой паллеты в принятых товарах!";
                        SS.FPalletID = "";
                        SS.FBarcodePallet = "";
                        SS.PalletAcceptedItem = null;
                        BadDone();
                        return;
                    }
                    lblAction.Text = "Отсканируйте адресс паллеты!";
                    GoodDone();
                    return;
                }
                else if (SS.CurrentMode == Mode.Transfer)
                {
                    //отcканировали паллету надо запомнить ее ШК дальше он нам пригодится
                    SS.FBarcodePallet = strBarcode;

                }
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
                if (SS.CurrentMode == Mode.AcceptedItem)
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
                else if (dicBarcode["Type"] == "6" && (SS.CurrentMode == Mode.SetSelfContorl || SS.CurrentMode == Mode.Down || SS.CurrentMode == Mode.Loading || SS.CurrentMode == Mode.SampleSet || SS.CurrentMode == Mode.Set || SS.CurrentMode == Mode.FreeDownComplete || SS.CurrentMode == Mode.NewComplectation || SS.CurrentMode == Mode.NewComplectationComplete))
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
            if (SS.AcceptedItem.ToMode == Mode.AcceptanceCross ? SS.CompleteAcceptCross() : SS.CompleteAccept())
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
        private void DBOCAcceptanceCross(Button currBtn)
        {
            lblAction.Text = "Команда печати в обработке, подождите...";
            Refresh();
            
            if (currBtn.Name == "btnPrint")
            {
                SS.PrintLabelsCross(true);
                if (Screan == 1)
                {
                    pnlCurrent.MoveControls(2 * pnlCurrent.Width, 0);
                    Screan = -1;
                    pnlCurrent.GetControlByName("dgNotAcceptedItems").Focus();
                }
                View();
                lblAction.Text = SS.ExcStr;
            }
            else if (currBtn.Name == "btnPrintCondition")
            {
                SS.PrintLabelsCross(false);
                if (Screan == 1)
                {
                    pnlCurrent.MoveControls(2 * pnlCurrent.Width, 0);
                    Screan = -1;
                    pnlCurrent.GetControlByName("dgNotAcceptedItems").Focus();
                }
                View();
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
                    if (SS.RequestHarmonization(SS.WarehousesFrom.Rows[pnlCurrent.GetDataGridByName("dgFrom").CurrentRowIndex]["ID"].ToString()))
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
                case Mode.AcceptanceCross:

                    //КРОСС-ДОКИНГ

                    if (SS.PalletAcceptedItem == null || !SS.PalletAcceptedItem.Selected )
                    {
                        //косяк, нет паллеты
                        lblAction.Text = "Отсканируйте паллету заказа!";
                        break;
                    }
                    SS.PalletAcceptedItem.Refresh();
                    if (!SS.PalletAcceptedItem.Adress0.Selected)
                    { 
                        //косяк, нет адреса
                        lblAction.Text = "Отсканируйте адрес паллеты!";
                        break;
                    }
                    DBOCAcceptanceCross(currBtn); 
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
                    if (currBtn.Name == "btnPrint")
                    {
                        if (SS.CompleteLodading())
                        {
                            View();
                        }
                        else
                        {
                            lblAction.Text = SS.ExcStr;
                        }
                    }
                    else
                    {
                        if (SS.RepreshLodading())
                        {
                            View();
                        }
                        else
                        {
                            lblAction.Text = SS.ExcStr;
                        }
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
            else if (SS.CurrentMode == Mode.AcceptanceCross)
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