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

//Новое отображение

namespace WPM
{
    public partial class FMain : Form
    {
        private void Waiting_view()
        {
            lblState.Text = "Авторизация";
            DrawlblResult();
            Text = "WPM " + Vers;
            pnlCurrent.GetLabelByName("lblResult").Text = "Сессия закрыта";
            lblAction.Text = "Ожидание авторизации";
            lblAction.ForeColor = Color.Blue;
        } //Отображение страницы ожидания авторизации
        private void Waiting_review() { }

        private void ChoiseWork_view()
        {
            lblState.Text = "Выбор работы";
            Screan = 0;
            DrawlblResult();

            #region 0 - Отбор/Set
            Label btnSet = new Label();
            btnSet.Name = "btnSet";
            btnSet.Text = "0 - Отбор";
            btnSet.TextAlign = ContentAlignment.TopCenter;
            btnSet.Location = new Point(6, 0);
            btnSet.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnSet.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnSet.ForeColor = SS.Employer.CanSet ? Color.Black : Color.DarkGray;
            btnSet.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnSet);
            #endregion
            #region 1 - Приёмка/Take
            Label btnTake = new Label();
            btnTake.Name = "btnTake";
            btnTake.Text = "1 - Приемка";
            btnTake.TextAlign = ContentAlignment.TopCenter;
            btnTake.Location = new Point(6, 19);
            btnTake.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnTake.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnTake.ForeColor = SS.Employer.CanAcceptance ? Color.Black : Color.DarkGray;
            btnTake.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnTake);
            #endregion
            #region 2 - Разнос/Transfer
            Label btnTransfer = new Label();
            btnTransfer.Name = "btnTransfer";
            btnTransfer.Text = "2 - Разнос";
            btnTransfer.TextAlign = ContentAlignment.TopCenter;
            btnTransfer.Location = new Point(6, 38);
            btnTransfer.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnTransfer.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnTransfer.ForeColor = SS.Employer.CanTransfer ? Color.Black : Color.DarkGray;
            btnTransfer.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnTransfer);
            #endregion
            #region 3 - Отгрузка/Shipping
            Label btnShipping = new Label();
            btnShipping.Name = "btnShipping";
            btnShipping.Text = "3 - Отгрузка ***";
            btnShipping.TextAlign = ContentAlignment.TopCenter;
            btnShipping.Location = new Point(6, 57);
            btnShipping.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnShipping.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnShipping.ForeColor = SS.Employer.CanComplectation || SS.Employer.CanDown ? Color.Black : Color.DarkGray;
            btnShipping.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnShipping);
            #endregion
            #region 4 - Инвентарицазия/Inventory
            Label btnInventory = new Label();
            btnInventory.Name = "btnInventory";
            btnInventory.Text = "4 - Инвентарицазия";
            btnInventory.TextAlign = ContentAlignment.TopCenter;
            btnInventory.Location = new Point(6, 76);
            btnInventory.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnInventory.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnInventory.ForeColor = SS.Employer.CanInventory ? Color.Black : Color.DarkGray;
            btnInventory.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnInventory);
            #endregion
            #region 5 - Сверка товара/SampleInventory
            Label btnSampleInventory = new Label();
            btnSampleInventory.Name = "btnSampleInventory";
            btnSampleInventory.Text = "5 - Сверка товара";
            btnSampleInventory.TextAlign = ContentAlignment.TopCenter;
            btnSampleInventory.Location = new Point(CurrWidth < 320 ? 122 : 162, 0);
            btnSampleInventory.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnSampleInventory.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnSampleInventory.ForeColor = SS.Employer.CanLayOutSample ? Color.Black : Color.DarkGray;
            btnSampleInventory.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnSampleInventory);
            #endregion
            #region 6 - Образцы ***./Samples
            Label btnSamples = new Label();
            btnSamples.Name = "btnSamples";
            btnSamples.Text = "6 - Образцы ***";
            btnSamples.TextAlign = ContentAlignment.TopCenter;
            btnSamples.Location = new Point(CurrWidth < 320 ? 122 : 162, 19);
            btnSamples.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnSamples.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnSamples.ForeColor = SS.Employer.CanLayOutSample && SS.Employer.CanGiveSample ? Color.Black : Color.DarkGray;
            btnSamples.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnSamples);
            #endregion
            #region //Набор Образцов(было до подпитки)
            //Label btnSampleSet = new Label();
            //btnSampleSet.Name      = "btnSampleSet";
            //btnSampleSet.Text      = "7 - Набор образцов";
            //btnSampleSet.TextAlign = ContentAlignment.TopCenter;
            //btnSampleSet.Location  = new Point(CurrWidth < 320 ? 122 : 162, 38);
            //btnSampleSet.Size      = new Size (CurrWidth < 320 ? 110 : 150, 18);
            //btnSampleSet.Font      = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            ////btnSampleSet.ForeColor = SS.Employer.CanGiveSample ? Color.Black : Color.DarkGray;
            //btnSampleSet.ForeColor = Color.DarkGray;
            //btnSampleSet.BackColor = Color.LightGray;
            //pnlCurrent.Controls.Add(btnSampleSet);
            #endregion
            #region 7 - Подпитка/Supply
            Label btnSupply = new Label();
            btnSupply.Name = "btnSupply";
            btnSupply.Text = "7 - Подпитка ***";
            btnSupply.TextAlign = ContentAlignment.TopCenter;
            btnSupply.Location = new Point(CurrWidth < 320 ? 122 : 162, 38);
            btnSupply.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnSupply.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnSupply.ForeColor = SS.Employer.CanLoad || SS.Employer.CanSupply ? Color.Black : Color.DarkGray;
            btnSupply.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnSupply);
            #endregion
            #region 8 - Контроль/ControlCollect
            Label btnControlCollect = new Label();
            btnControlCollect.Name = "btnControlCollect";
            btnControlCollect.Text = "8 - Контроль";
            btnControlCollect.TextAlign = ContentAlignment.TopCenter;
            btnControlCollect.Location = new Point(CurrWidth < 320 ? 122 : 162, 57);
            btnControlCollect.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnControlCollect.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnControlCollect.ForeColor = Color.Black;
            btnControlCollect.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnControlCollect);
            #endregion
            #region 9 - Гармонь/Harmonization
            Label btnHarmonization = new Label();
            btnHarmonization.Name = "btnHarmonization";
            btnHarmonization.Text = "9 - Гармонь";
            btnHarmonization.TextAlign = ContentAlignment.TopCenter;
            btnHarmonization.Location = new Point(CurrWidth < 320 ? 122 : 162, 76);
            btnHarmonization.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnHarmonization.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnHarmonization.ForeColor = SS.Employer.CanHarmonization ? Color.Black : Color.DarkGray;
            btnHarmonization.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnHarmonization);
            #endregion

            Text = "WPM " + Vers + " (" + Helper.GetShortFIO(SS.Employer.Name) + ")";
            pnlCurrent.GetLabelByName("lblResult").Text = SS.Employer.Name + " Сессия открыта";
            lblAction.Text = "Выберите режим работы";
            lblAction.ForeColor = Color.Blue;
        }               //Отображение выбора работы (главная страница)
        private void ChoiseWork_review() { }

        private void ChoiseWorkShipping_view()
        {
            lblState.Text = "Выбор работы (отгрузка)";
            ChoiseWorkShipping Obj = SS.MM as ChoiseWorkShipping;

            Screan = 0;
            DrawlblResult();
            Label btnCancel = new Label();
            btnCancel.Name = "btnCancel";
            btnCancel.Text = "0 - Отмена";
            btnCancel.TextAlign = ContentAlignment.TopCenter;
            btnCancel.Location = new Point(6, 0);
            btnCancel.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnCancel.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnCancel.ForeColor = Color.Black;
            btnCancel.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnCancel);

            Label btnLoad = new Label();
            btnLoad.Name = "btnLoad";
            btnLoad.Text = "1 - Погрузка";
            btnLoad.TextAlign = ContentAlignment.TopCenter;
            btnLoad.Location = new Point(6, 19);
            btnLoad.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnLoad.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnLoad.ForeColor = SS.Employer.CanComplectation ? Color.Black : Color.DarkGray;
            btnLoad.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnLoad);

            Label btnDown = new Label();
            btnDown.Name = "btnDown";
            btnDown.Text = "3 - Спуск";
            btnDown.TextAlign = ContentAlignment.TopCenter;
            btnDown.Location = new Point(6, 57);
            btnDown.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnDown.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnDown.ForeColor = SS.Employer.CanDown ? Color.Black : Color.DarkGray;
            btnDown.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnDown);

            Label btnFree = new Label();
            btnFree.Name = "btnFree";
            btnFree.Text = "4 - св. спуск/компл.";
            btnFree.TextAlign = ContentAlignment.TopCenter;
            btnFree.Location = new Point(6, 76);
            btnFree.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnFree.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnFree.ForeColor = SS.Employer.CanComplectation ? Color.Black : Color.DarkGray;
            btnFree.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnFree);

            Text = "WPM " + Vers + " (" + Helper.GetShortFIO(SS.Employer.Name) + ")";
            pnlCurrent.GetLabelByName("lblResult").Text = SS.Employer.Name + " Сессия открыта";
            lblAction.Text = Obj.ExcStr;
            lblAction.ForeColor = Color.Blue;
        }        //Выбор работы (отгрузка)
        private void ChoiseWorkShipping_review() { }

        private void ChoiseWorkSupply_view()
        {
            lblState.Text = "Выбор работы (подпитка)";
            ChoiseWorkSupply Obj = SS.MM as ChoiseWorkSupply;

            Screan = 0;
            DrawlblResult();
            Label btnCancel = new Label();
            btnCancel.Name = "btnCancel";
            btnCancel.Text = "0 - Отмена";
            btnCancel.TextAlign = ContentAlignment.TopCenter;
            btnCancel.Location = new Point(6, 0);
            btnCancel.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnCancel.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnCancel.ForeColor = Color.Black;
            btnCancel.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnCancel);

            Label btnLoader = new Label();
            btnLoader.Name = "btnLoader";
            btnLoader.Text = "1 - Погрузчик";
            btnLoader.TextAlign = ContentAlignment.TopCenter;
            btnLoader.Location = new Point(6, 19);
            btnLoader.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnLoader.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnLoader.ForeColor = Obj.Employer.CanLoad ? Color.Black : Color.DarkGray;
            btnLoader.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnLoader);

            Label btnLayOut = new Label();
            btnLayOut.Name = "btnLayOut";
            btnLayOut.Text = "2 - Слотчик";
            btnLayOut.TextAlign = ContentAlignment.TopCenter;
            btnLayOut.Location = new Point(6, 38);
            btnLayOut.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnLayOut.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnLayOut.ForeColor = Obj.Employer.CanSupply ? Color.Black : Color.DarkGray;
            btnLayOut.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnLayOut);

            Text = "WPM " + Vers + " (" + Helper.GetShortFIO(Obj.Employer.Name) + ")";
            pnlCurrent.GetLabelByName("lblResult").Text = Obj.Employer.Name + " Сессия открыта";
            lblAction.Text = "Выберите режим работы";
            lblAction.ForeColor = Color.Blue;
        }         //Выбор работы (подпитка)
        private void ChoiseWorkSupply_review() { }

        private void ChoiseWorkSample_view()
        {
            lblState.Text = "Выбор работы (образцы)";
            ChoiseWorkSample Obj = SS.MM as ChoiseWorkSample;

            Screan = 0;
            DrawlblResult();
            Label btnCancel = new Label();
            btnCancel.Name = "btnCancel";
            btnCancel.Text = "0 - Отмена";
            btnCancel.TextAlign = ContentAlignment.TopCenter;
            btnCancel.Location = new Point(6, 0);
            btnCancel.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnCancel.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnCancel.ForeColor = Color.Black;
            btnCancel.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnCancel);

            Label btnSet = new Label();
            btnSet.Name = "btnSet";
            btnSet.Text = "1 - Отбор";
            btnSet.TextAlign = ContentAlignment.TopCenter;
            btnSet.Location = new Point(6, 19);
            btnSet.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnSet.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnSet.ForeColor = Obj.Employer.CanGiveSample ? Color.Black : Color.DarkGray;
            btnSet.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnSet);

            Label btnPut = new Label();
            btnPut.Name = "btnPut";
            btnPut.Text = "2 - Выкладка";
            btnPut.TextAlign = ContentAlignment.TopCenter;
            btnPut.Location = new Point(6, 38);
            btnPut.Size = new Size(CurrWidth < 320 ? 110 : 150, 18);
            btnPut.Font = CurrWidth < 320 ? FontTahoma8Bold : FontTahoma10Bold;
            btnPut.ForeColor = Obj.Employer.CanLayOutSample ? Color.Black : Color.DarkGray;
            btnPut.BackColor = Color.LightGray;
            pnlCurrent.Controls.Add(btnPut);

            Text = "WPM " + Vers + " (" + Helper.GetShortFIO(Obj.Employer.Name) + ")";
            pnlCurrent.GetLabelByName("lblResult").Text = Obj.Employer.Name + " Сессия открыта";
            lblAction.Text = "Выберите режим работы";
            lblAction.ForeColor = Color.Blue;
        }         //Выбор работы (образцы)
        private void ChoiseWorkSample_review() 
        {
            
        }

        private void ChoiseWorkAddressCard_view()
        {
            ChoiseWorkAddressCard Obj = SS.MM as ChoiseWorkAddressCard;
            ChoiseWorkAddressCard_review ();
            lblAction.Text = Obj.ExcStr;

        }         //Выбор работы карточка адреса
        private void ChoiseWorkAddressCard_review() 
        {
            ViewClear();
            ChoiseWorkAddressCard Obj = SS.MM as ChoiseWorkAddressCard;
            lblState.Text = "Товары в адресе";

            //Screan = 0;
            //DrawlblResult();

            Screan = 0;
            TimerFind.Interval = 20000;
            TimerFind.Enabled = false;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgAC = new DataGrid();
            dgAC.BackgroundColor = Color.White;
            dgAC.Location = new Point(-1 * Screan * pnlCurrent.Width + 4, 0);
            dgAC.Name = "dgAddressCard";
            dgAC.Size = new System.Drawing.Size(CurrWidth - 10, 165);
            dgAC.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgAC.RowHeadersVisible = false;
            #region Styles;
            //Настройка отображения
            dgAC.TableStyles.Clear();
            dgts = new DataGridTableStyle();

            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 58 : 78;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Кол-во";
            columnStyle.MappingName = "Count";
            columnStyle.Width = CurrWidth < 320 ? 38 : 58;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Состояние";
            columnStyle.MappingName = "Condition";
            columnStyle.Width = CurrWidth < 320 ? 80 : 155;
            dgts.GridColumnStyles.Add(columnStyle);

            dgAC.TableStyles.Add(dgts);
            #endregion
            dgAC.DataSource = SS.AddressCardItems;
            pnlCurrent.Controls.Add(dgAC);
        }

        private void LoaderChoise_view()
        {
            LoaderChoise Obj = SS.MM as LoaderChoise;
            LoaderChoise_review();
            lblAction.Text = Obj.ExcStr;
        } 
        private void LoaderChoise_review()
        {
            ViewClear();
            LoaderChoise Obj = SS.MM as LoaderChoise;
            lblState.Text = "Погрузчик выбор задания";

            lblAction.ForeColor = Color.Blue;

            for (int i = 0; i < Obj.TaskList.Rows.Count; i++)
            {
                bool Allowed = (int)Obj.TaskList.Rows[i]["allowed"] == 1 ? true : false;

                Label lblString1 = new Label();
                lblString1.Font = FontTahoma14Bold;
                lblString1.ForeColor = Allowed ? Color.Black : Color.Silver;
                lblString1.Name = "lblString" + i.ToString();
                lblString1.TextAlign = ContentAlignment.TopLeft;
                lblString1.Location = new Point(CurrWidth < 320 ? 0 : 40, 18 * i);
                lblString1.Size = new Size(240, 21);
                //if (i == 1)
                //{
                //    //Меняем текст с тройкой на текст с двойкой (чтобы не переписывать хранимую процедуру)
                //    lblString1.Text = Obj.TaskList.Rows[i]["name"].ToString() + " *подъем";
                //}
                //else
                //{
                //    lblString1.Text = Obj.TaskList.Rows[i]["name"].ToString();
                //}
                lblString1.Text = Obj.TaskList.Rows[i]["name"].ToString();
                pnlCurrent.Controls.Add(lblString1);
            }

            Label lblKey1 = new Label();
            lblKey1.Font = FontTahoma8Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft;
            lblKey1.Location = new Point(4, 165);
            lblKey1.Size = new Size(110, 18);
            lblKey1.Text = "'зеленая' - обновить";
            pnlCurrent.Controls.Add(lblKey1);

            Label lblKey2 = new Label();
            lblKey2.Font = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight;
            lblKey2.Location = new Point(CurrWidth - 116, 165);
            lblKey2.Size = new Size(110, 18);
            lblKey2.Text = "0, Esc - назад";
            pnlCurrent.Controls.Add(lblKey2);
        }           //Погрузчик выбор

        private void Loader_view()
        {
            Loader Obj = SS.MM as Loader;
            lblState.Text = Obj.Task.TypeMoveDescr;

            //спустите, снимите, возвратите и тп.
            Label lblHeader = new Label();
            lblHeader.Font = FontTahoma12Bold;
            lblHeader.Name = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter;
            lblHeader.Location = new Point(4, 0);
            lblHeader.Size = new Size(CurrWidth - 10, 20);
            lblHeader.Text = Obj.Task.TypeMoveActionDescr;
            pnlCurrent.Controls.Add(lblHeader);

            //Адрес 0
            Label lblAdress0 = new Label();
            lblAdress0.Font = FontTahoma16Bold;
            lblAdress0.Name = "lblAdress0";
            lblAdress0.TextAlign = ContentAlignment.TopCenter;
            lblAdress0.Location = new Point(4, 50 + 4);
            lblAdress0.Size = new Size(CurrWidth - 8, 30);
            lblAdress0.Text = Obj.Task.Adress0.Name;
            pnlCurrent.Controls.Add(lblAdress0);

            //Адрес 1
            Label lblAdress1 = new Label();
            lblAdress1.Font = FontTahoma14Bold;
            lblAdress1.Name = "lblAdress1";
            lblAdress1.TextAlign = ContentAlignment.TopCenter;
            lblAdress1.Location = new Point(4, 80 + 6);
            lblAdress1.Size = new Size(CurrWidth - 8, 30);
            //lblAdress1.Text = Obj.Task.Adress1.Selected ? Obj.Task.Adress1.Name : Obj.Task.PalleteBarcode;
            lblAdress1.Text = "Паллета: " + Obj.Task.Pallete;
            pnlCurrent.Controls.Add(lblAdress1);

            //Адрес сектор
            Label lblAdressSektor = new Label();
            lblAdressSektor.Font = FontTahoma16Bold;
            lblAdressSektor.Name = "lblAdressSektor";
            lblAdressSektor.TextAlign = ContentAlignment.TopCenter;
            lblAdressSektor.Location = new Point(4, 110 + 8);
            lblAdressSektor.Size = new Size(CurrWidth - 8, 28);
            lblAdressSektor.Text = Obj.Task.GetAttribute("Адрес1_сектор").ToString().Trim();
            pnlCurrent.Controls.Add(lblAdressSektor);

            Label lblKey2 = new Label();
            lblKey2.Font = FontTahoma10Bold;
            lblKey2.ForeColor = Color.White;
            lblKey2.BackColor = Color.Red;
            lblKey2.Name = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight;
            lblKey2.Location = new Point(CurrWidth - 116, 160);
            lblKey2.Size = new Size(110, 18);
            lblKey2.Text = "Esc - ОТКАЗ";
            pnlCurrent.Controls.Add(lblKey2);

            lblAction.ForeColor = Color.Blue;
            lblAction.Text = Obj.ExcStr;
        }                   //Погрузчик
        private void Loader_review()
        {
            Loader Obj = SS.MM as Loader;
            Label lblAdress0 = pnlCurrent.GetLabelByName("lblAdress0");
            Label lblAdress1 = pnlCurrent.GetLabelByName("lblAdress1");

            lblAdress0.ForeColor = Color.Black;
            lblAdress1.ForeColor = Color.Black;
            if (Obj.CurrentAction == ActionSet.ScanAdress)
            {
                lblAdress0.ForeColor = Color.Blue;
            }
            else if (Obj.CurrentAction == ActionSet.ScanPallete)
            {
                lblAdress1.ForeColor = Color.Blue;
            }
        } 

        private void LoaderChoiseLift_view()
        {
            LoaderChoiseLift Obj = SS.MM as LoaderChoiseLift;
            LoaderChoiseLift_review();
            lblAction.Text = Obj.ExcStr;
        }         //Погрузчик выбор (лифт)
        private void LoaderChoiseLift_review()
        {
            LoaderChoiseLift Obj = SS.MM as LoaderChoiseLift;
            lblState.Text = "Погрузчик выбор (лифт)";

            lblAction.ForeColor = Color.Blue;

            for (int i = 0; i < Obj.LiftTaskList.Rows.Count; i++)
            {
                Label lblString1 = new Label();
                lblString1.Font = FontTahoma14Bold;
                lblString1.ForeColor = Color.Black;
                lblString1.Name = "lblString" + i.ToString();
                lblString1.TextAlign = ContentAlignment.TopLeft;
                lblString1.Location = new Point(CurrWidth < 320 ? 0 : 40, 18 * i);
                lblString1.Size = new Size(200, 21);
                lblString1.Text = (i + 1).ToString() + " - " + Obj.LiftTaskList.Rows[i]["name"].ToString();
                pnlCurrent.Controls.Add(lblString1);

            }

            Label lblKey1 = new Label();
            lblKey1.Font = FontTahoma8Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft;
            lblKey1.Location = new Point(4, 165);
            lblKey1.Size = new Size(110, 18);
            lblKey1.Text = "'зеленая' - обновить";
            pnlCurrent.Controls.Add(lblKey1);

            Label lblKey2 = new Label();
            lblKey2.Font = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight;
            lblKey2.Location = new Point(CurrWidth - 116, 165);
            lblKey2.Size = new Size(110, 18);
            lblKey2.Text = "0, Esc - назад";
            pnlCurrent.Controls.Add(lblKey2);
        } 

        //Всё, что с "Refill" - относится к подпитке
        private void RefillChoise_view()
        {
            RefillChoise Obj = SS.MM as RefillChoise;
            RefillChoise_review();
            lblAction.Text = Obj.ExcStr;
        } 
        private void RefillChoise_review()
        {
            ViewClear();
            RefillChoise Obj = (SS.MM as RefillChoise);

            lblState.Text = "Слотчик выбор задания";

            lblAction.ForeColor = Color.Blue;

            for (int i = 0; i < Obj.TaskList.Rows.Count; i++)
            {
                bool Allowed = (int)Obj.TaskList.Rows[i]["allowed"] == 1 ? true : false;

                Label lblString1 = new Label();
                lblString1.Font = FontTahoma14Bold;
                lblString1.ForeColor = Allowed ? Color.Black : Color.Silver;
                lblString1.Name = "lblString" + i.ToString();
                lblString1.TextAlign = ContentAlignment.TopLeft;
                lblString1.Location = new Point(CurrWidth < 320 ? 0 : 40, 18 * i);
                lblString1.Size = new Size(200, 21);
                lblString1.Text = Obj.TaskList.Rows[i]["name"].ToString();
                pnlCurrent.Controls.Add(lblString1);
            }

            Label lblKey1 = new Label();
            lblKey1.Font = FontTahoma8Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft;
            lblKey1.Location = new Point(4, 165);
            lblKey1.Size = new Size(110, 18);
            lblKey1.Text = "'зеленая' - обновить";
            pnlCurrent.Controls.Add(lblKey1);

            Label lblKey2 = new Label();
            lblKey2.Font = FontTahoma10Regular;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight;
            lblKey2.Location = new Point(CurrWidth - 116, 165);
            lblKey2.Size = new Size(110, 18);
            lblKey2.Text = "0, Esc - назад";
            pnlCurrent.Controls.Add(lblKey2);

            lblAction.Text = Obj.ExcStr;
        } //Слотчик выбор задания

        private void RefillSet_view()
        {
            RefillSet Obj = SS.MM as RefillSet;

            //Screan = 0;
            //DataGridTextBoxColumn columnStyle;
            //DataGridTableStyle dgts;

            //DataGrid dgGoodsCC = new DataGrid();
            //dgGoodsCC.Location = new Point(CurrWidth + 2, 0);
            //dgGoodsCC.Name = "dgGoodsCC";
            //dgGoodsCC.Size = new System.Drawing.Size(CurrWidth - 6, 165);
            //dgGoodsCC.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            //dgGoodsCC.DataSource = SS.CCRP;
            //dgGoodsCC.RowHeadersVisible = false;
            #region Styles
            //dgGoodsCC.TableStyles.Clear();
            //dgts = new DataGridTableStyle();
            //columnStyle = new DataGridTextBoxColumn();
            //columnStyle.HeaderText = "№";
            //columnStyle.MappingName = "Number";
            //columnStyle.Width = CurrWidth < 320 ? 20 : 35;
            //dgts.GridColumnStyles.Add(columnStyle);
            //columnStyle = new DataGridTextBoxColumn();
            //columnStyle.HeaderText = "Адрес";
            //columnStyle.MappingName = "Adress";
            //columnStyle.Width = CurrWidth < 320 ? 60 : 90;
            //dgts.GridColumnStyles.Add(columnStyle);
            //columnStyle = new DataGridTextBoxColumn();
            //columnStyle.HeaderText = "Инв.код";
            //columnStyle.MappingName = "InvCode";
            //columnStyle.Width = CurrWidth < 320 ? 49 : 68;
            //dgts.GridColumnStyles.Add(columnStyle);
            //columnStyle = new DataGridTextBoxColumn();
            //columnStyle.HeaderText = "Кол.";
            //columnStyle.MappingName = "Count";
            //columnStyle.Width = CurrWidth < 320 ? 30 : 40;
            //dgts.GridColumnStyles.Add(columnStyle);
            //columnStyle = new DataGridTextBoxColumn();
            //columnStyle.HeaderText = "Сумма";
            //columnStyle.MappingName = "Sum";
            //columnStyle.Width = CurrWidth < 320 ? 35 : 60;
            //dgts.GridColumnStyles.Add(columnStyle);
            ////columnStyle.Width = CurrWidth < 320 ? 25 : 40;

            //dgGoodsCC.TableStyles.Add(dgts);
            #endregion
            //pnlCurrent.Controls.Add(dgGoodsCC);

            //Label lblHeaderSum = new Label();
            //lblHeaderSum.Font = FontTahoma8Bold;
            //lblHeaderSum.ForeColor = Color.DarkRed;
            //lblHeaderSum.Name = "lblHeaderSum";
            //lblHeaderSum.TextAlign = ContentAlignment.TopCenter;
            //lblHeaderSum.Location = new Point(2 * CurrWidth - 120 - 3, 165 + 2);
            //lblHeaderSum.Size = new Size(120, 20);
            //pnlCurrent.Controls.Add(lblHeaderSum);

            //Label lblHeader = new Label();
            //lblHeader.Font = FontTahoma12Bold;
            //lblHeader.Name = "lblHeader";
            //lblHeader.TextAlign = ContentAlignment.TopCenter;
            //lblHeader.Location = new Point(4, 0);
            //lblHeader.Size = new Size(CurrWidth - 10, 20);
            //pnlCurrent.Controls.Add(lblHeader);

            Label lblAdressZone = new Label();
            lblAdressZone.Font = FontTahoma12Bold;
            lblAdressZone.BackColor = Color.Yellow;
            lblAdressZone.Name = "lblAdressZone";
            lblAdressZone.TextAlign = ContentAlignment.TopCenter;
            lblAdressZone.Location = new Point(30, 0);
            lblAdressZone.Size = new Size(CurrWidth - 60, 20);
            lblAdressZone.Visible = false;
            pnlCurrent.Controls.Add(lblAdressZone);

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

            //Label lblHeaderBalance = new Label();
            //lblHeaderBalance.Font = FontTahoma8Bold;
            //lblHeaderBalance.ForeColor = Color.DarkRed;
            //lblHeaderBalance.Name = "lblHeaderBalance";
            //lblHeaderBalance.TextAlign = ContentAlignment.TopCenter;
            //lblHeaderBalance.Location = new Point(CurrWidth - 50, 50 + 4);
            //lblHeaderBalance.Size = new Size(50, 30);
            //pnlCurrent.Controls.Add(lblHeaderBalance);

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

            Label lblAdress1 = new Label();
            lblAdress1.Font = FontTahoma12Bold;
            lblAdress1.Name = "lblAdress1";
            lblAdress1.TextAlign = ContentAlignment.TopCenter;
            lblAdress1.Location = new Point(70, 125 + 10);
            lblAdress1.Size = new Size(CurrWidth - 140, 20);
            pnlCurrent.Controls.Add(lblAdress1);

            Label lblDetailsCount = new Label();
            lblDetailsCount.Font = FontTahoma8Bold;
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
            lblKey1.Font = FontTahoma10Regular;
            lblKey1.BackColor = Color.Green;
            lblKey1.ForeColor = Color.White;
            lblKey1.Name = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft;
            lblKey1.Location = new Point(4, 160);
            lblKey1.Size = new Size(90, 18);
            lblKey1.Text = "ПРОВЕРИТЬ";
            pnlCurrent.Controls.Add(lblKey1);

            Label lblKey2 = new Label();
            lblKey2.Font = FontTahoma10Bold;
            lblKey2.BackColor = Color.Red;
            lblKey2.ForeColor = Color.White;
            lblKey2.Name = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight;
            lblKey2.Location = new Point(CurrWidth - 96, 160);
            lblKey2.Size = new Size(90, 18);
            lblKey2.Text = "Esc - ОТКАЗ";
            pnlCurrent.Controls.Add(lblKey2);

            Label lblKey3 = new Label();
            lblKey3.Font = FontTahoma9Regular;
            lblKey3.BackColor = Color.LightGray;
            lblKey3.Name = "lblKey2";
            lblKey3.TextAlign = ContentAlignment.TopRight;
            lblKey3.Location = new Point(CurrWidth - 66, 140);
            lblKey3.Size = new Size(60, 18);
            lblKey3.Text = "9 - корр";
            pnlCurrent.Controls.Add(lblKey3);

            //PictureBox pbPhoto = new PictureBox();
            //pbPhoto.Name = "pbPhoto";
            //pbPhoto.Location = new Point(0, 0);
            //pbPhoto.Size = new Size(320, 185);
            //pbPhoto.Visible = false;
            //pnlCurrent.Controls.Add(pbPhoto);

            lblAction.ForeColor = Color.Blue;
            lblAction.Text = Obj.ExcStr;
        } 
        private void RefillSet_review()
        {
            RefillSet Obj = SS.MM as RefillSet;
            lblState.Text = Obj.DocAP.View; //Обновляем главную надпись
            //DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
            //dgGoodsCC.DataSource = SS.CCRP;

            //pnlCurrent.GetLabelByName("lblPrevious").Text = SS.PreviousAction;
            Label lblInvCode = pnlCurrent.GetLabelByName("lblInvCode");
            Label lblAdress = pnlCurrent.GetLabelByName("lblAdress");
            Label lblAdress1 = pnlCurrent.GetLabelByName("lblAdress1");
            Label lblCount = pnlCurrent.GetLabelByName("lblCount");
            TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
            Label lblPrevious = pnlCurrent.GetLabelByName("lblPrevious");
            //Label lblHeader = pnlCurrent.GetLabelByName("lblHeader");
            Label lblItem = pnlCurrent.GetLabelByName("lblItem");
            Label lblHeaderPrice = pnlCurrent.GetLabelByName("lblHeaderPrice");
            //Label lblHeaderBalance = pnlCurrent.GetLabelByName("lblHeaderBalance");
            //Label lblHeaderSum = pnlCurrent.GetLabelByName("lblHeaderSum");
            Label lblDetailsCount = pnlCurrent.GetLabelByName("lblDetailsCount");
            //PictureBox pbPhoto = pnlCurrent.GetPictureBoxByName("pbPhoto");
            Label lblKey1 = pnlCurrent.GetLabelByName("lblKey1");
            Label lblKey2 = pnlCurrent.GetLabelByName("lblKey2");
            Label lblAdressZone = pnlCurrent.GetLabelByName("lblAdressZone");

            lblKey1.Visible = false;
            try
            {
                lblKey2.Visible = Obj.CollectedLines > 0 ? true : false;
            }
            catch (Exception e)
            {
                lblKey2.Visible = false;
            }

            lblInvCode.ForeColor = Color.Black;
            lblAdress.ForeColor = Color.Black;
            lblCount.ForeColor = Color.Black;
            lblAdress1.ForeColor = Color.Black;

            lblPrevious.Text = Obj.PreviousAction;
            //lblHeader.Text = "Строка " + SS.CCItem.CurrLine.ToString() + " из " + SS.DocSet.Rows.ToString() + " (ост " + SS.AllSetsRow.ToString() + ")";
            lblHeaderPrice.Text = "Цена: " + Obj.Item.Price;
            //lblHeaderBalance.Text = "Ост-ок " + SS.CCItem.Balance.ToString().Trim();
            //lblHeaderSum.Text = "Сумма: " + SS.DocSet.Sum.ToString();
            lblInvCode.Text = Obj.Item.InvCode;
            lblAdress.Text = Obj.Adress0.Name;
            lblAdress1.Text = Obj.Adress1.Name;
            //
            if (Obj.Item.ZonaHand.Selected)
            {
                lblAdressZone.Text = Obj.Item.ZonaHand.FirstAdress.Name + " .. " + Obj.Item.ZonaHand.LastAdress.Name;
                lblAdressZone.Visible = true;
            }
            else
            {
                lblAdressZone.Visible = false;
            }
            lblItem.Text = Obj.Item.Name;
            lblCount.Text = Obj.Amount.ToString() + " шт по 1";
            lblDetailsCount.Text = "Деталей: " + Obj.Item.Details.ToString();
            switch (Obj.CurrentAction)
            {
                case ActionSet.ScanAdress:
                    tbCount.Visible = false;
                    tbCount.Text = "";
                    lblAdress.ForeColor = Color.Blue;
                    break;

                case ActionSet.ScanItem:
                    //pbPhoto.Visible = false;
                    lblInvCode.ForeColor = Color.Blue;
                    break;

                case ActionSet.EnterCount:
                    lblCount.ForeColor = Color.Blue;
                    tbCount.Visible = true;
                    tbCount.BringToFront();
                    tbCount.Focus();
                    break;

                case ActionSet.Waiting:
                    //Зависла команда робота
                    tbCount.Visible = false;
                    lblKey1.Visible = true;
                    lblKey2.Visible = false;
                    break;
            }

            lblAction.Text = Obj.ExcStr;
        } 

        private void RefillSetComplete_view()
        {
            RefillSetComplete Obj = SS.MM as RefillSetComplete;

            Label lblHeader = new Label();
            lblHeader.Font = FontTahoma12Bold;
            lblHeader.Name = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter;
            lblHeader.Location = new Point(4, 0);
            lblHeader.Size = new Size(CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblHeader);

            //Label lblPrinter = new Label();
            //lblPrinter.Font = FontTahoma10Bold;
            //lblPrinter.ForeColor = Color.DarkGreen;
            //lblPrinter.Name = "lblPrinter";
            //lblPrinter.TextAlign = ContentAlignment.TopCenter;
            //lblPrinter.Location = new Point(4, 20 + 2);
            //lblPrinter.Size = new Size(CurrWidth - 10, 20);
            //pnlCurrent.Controls.Add(lblPrinter);

            Label lblClient = new Label();
            lblClient.Font = FontTahoma12Bold;
            lblClient.Name = "lblClient";
            lblClient.TextAlign = ContentAlignment.TopCenter;
            lblClient.Location = new Point(4, 40 + 4);
            lblClient.Size = new Size(CurrWidth - 10, 40);
            pnlCurrent.Controls.Add(lblClient);

            Label lblSelfRemovel = new Label();
            lblSelfRemovel.Font = FontTahoma14Bold;
            lblSelfRemovel.Name = "lblSelfRemovel";
            lblSelfRemovel.ForeColor = Color.Blue;
            lblSelfRemovel.TextAlign = ContentAlignment.TopCenter;
            lblSelfRemovel.Location = new Point(4, 80 + 4);
            lblSelfRemovel.Size = new Size(CurrWidth - 10, 24);
            pnlCurrent.Controls.Add(lblSelfRemovel);

            ////Поле для ввода количества
            //TextBox tbCount = new TextBox();
            //tbCount.Name = "tbCount";
            //tbCount.Location = new Point(120, 140 + 10);
            //tbCount.Size = new Size(60, 28);
            //tbCount.Font = FontTahoma14Bold;
            //tbCount.Text = "";
            //tbCount.Visible = false;
            //pnlCurrent.Controls.Add(tbCount);

            //DrawlblResult();
            //pnlCurrent.GetLabelByName("lblResult").Text = "Отсканируйте коробку и введите количество мест!";
            lblAction.Text = Obj.ExcStr;
        } 
        private void RefillSetComplete_review()
        {
            RefillSetComplete Obj = SS.MM as RefillSetComplete;

            lblState.Text = "Завершение отбора";
            //Label lblResult = pnlCurrent.GetLabelByName("lblResult");
            //Label lblPrinter = pnlCurrent.GetLabelByName("lblPrinter");
            Label lblHeader = pnlCurrent.GetLabelByName("lblHeader");
            Label lblClient = pnlCurrent.GetLabelByName("lblClient");
            Label lblSelfRemovel = pnlCurrent.GetLabelByName("lblSelfRemovel");
            //TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
            lblSelfRemovel.Text = Obj.PalleteDescr;
            lblHeader.Text = "Отобрано " + Obj.DocAP.RowCount.ToString() + " строк";
            lblClient.Text = Obj.DocAP.View;
            //lblState.Text = SS.DocSet.View;
            //if (!SS.BoxOk)
            //{
            //    tbCount.Visible = true;
            //    tbCount.Focus();
            //}
            //lblPrinter.Text = SS.Printer.Description;
            //if (SS.BoxOk)
            //{
            //    pnlCurrent.GetLabelByName("lblResult").Text = "Всего " + SS.DocSet.Boxes.ToString() + " мест! Отсканируйте ШК предкомплектации!";
            //    tbCount.Visible = false;
            //}
        } 

        private void RefillLayout_view()
        {
            RefillLayout Obj = SS.MM as RefillLayout;
            //Screan = 0;
            DataGridTextBoxColumn columnStyle;
            DataGridTableStyle dgts;

            DataGrid dgGoodsCC = new DataGrid();
            dgGoodsCC.Location = new Point(2, 0);
            dgGoodsCC.Name = "dgGoodsCC";
            dgGoodsCC.Size = new System.Drawing.Size(CurrWidth - 6, 185);
            dgGoodsCC.Font = CurrWidth < 320 ? FontTahoma8Regular : FontTahoma10Regular;
            dgGoodsCC.DataSource = Obj.RemainItems;
            dgGoodsCC.RowHeadersVisible = false;
            #region Styles
            dgGoodsCC.TableStyles.Clear();
            dgts = new DataGridTableStyle();
            //columnStyle = new DataGridTextBoxColumn();
            //columnStyle.HeaderText = "№";
            //columnStyle.MappingName = "Number";
            //columnStyle.Width = CurrWidth < 320 ? 20 : 35;
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Инв.код";
            columnStyle.MappingName = "InvCode";
            columnStyle.Width = CurrWidth < 320 ? 49 : 68;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Кол.";
            columnStyle.MappingName = "Amount";
            columnStyle.Width = CurrWidth < 320 ? 30 : 40;
            dgts.GridColumnStyles.Add(columnStyle);
            columnStyle = new DataGridTextBoxColumn();
            columnStyle.HeaderText = "Адрес";
            columnStyle.MappingName = "Adress";
            columnStyle.Width = CurrWidth < 320 ? 60 : 90;
            dgts.GridColumnStyles.Add(columnStyle);

            dgGoodsCC.TableStyles.Add(dgts);
            #endregion
            pnlCurrent.Controls.Add(dgGoodsCC);

            //Label lblHeaderSum = new Label();
            //lblHeaderSum.Font = FontTahoma8Bold;
            //lblHeaderSum.ForeColor = Color.DarkRed;
            //lblHeaderSum.Name = "lblHeaderSum";
            //lblHeaderSum.TextAlign = ContentAlignment.TopCenter;
            //lblHeaderSum.Location = new Point(2 * CurrWidth - 120 - 3, 165 + 2);
            //lblHeaderSum.Size = new Size(120, 20);
            //pnlCurrent.Controls.Add(lblHeaderSum);

            //Label lblHeader = new Label();
            //lblHeader.Font = FontTahoma12Bold;
            //lblHeader.Name = "lblHeader";
            //lblHeader.TextAlign = ContentAlignment.TopCenter;
            //lblHeader.Location = new Point(4, 0);
            //lblHeader.Size = new Size(CurrWidth - 10, 20);
            //pnlCurrent.Controls.Add(lblHeader);

            Label lblAdressZone = new Label();
            lblAdressZone.Font = FontTahoma12Bold;
            lblAdressZone.BackColor = Color.Yellow;
            lblAdressZone.Name = "lblAdressZone";
            lblAdressZone.TextAlign = ContentAlignment.TopCenter;
            lblAdressZone.Location = new Point(30, 0);
            lblAdressZone.Size = new Size(CurrWidth - 60, 20);
            lblAdressZone.Visible = false;
            pnlCurrent.Controls.Add(lblAdressZone);

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

            //Label lblHeaderBalance = new Label();
            //lblHeaderBalance.Font = FontTahoma8Bold;
            //lblHeaderBalance.ForeColor = Color.DarkRed;
            //lblHeaderBalance.Name = "lblHeaderBalance";
            //lblHeaderBalance.TextAlign = ContentAlignment.TopCenter;
            //lblHeaderBalance.Location = new Point(CurrWidth - 50, 50 + 4);
            //lblHeaderBalance.Size = new Size(50, 30);
            //pnlCurrent.Controls.Add(lblHeaderBalance);

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

            Label lblAdress1 = new Label();
            lblAdress1.Font = FontTahoma12Bold;
            lblAdress1.Name = "lblAdress1";
            lblAdress1.TextAlign = ContentAlignment.TopCenter;
            lblAdress1.Location = new Point(50, 125 + 10);
            lblAdress1.Size = new Size(CurrWidth - 100, 20);
            pnlCurrent.Controls.Add(lblAdress1);

            Label lblDetailsCount = new Label();
            lblDetailsCount.Font = FontTahoma8Bold;
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
            tbCount.Location = new Point(130, 154);
            tbCount.Size = new Size(60, 28);
            tbCount.Font = FontTahoma14Bold;
            tbCount.Text = "";
            pnlCurrent.Controls.Add(tbCount);
            tbCount.Visible = false;

            //if (!SS.DocSet.Special)
            //{
            //    Label lblKey1 = new Label();
            //    lblKey1.Font = FontTahoma10Regular;
            //    lblKey1.BackColor = Color.LightGray;
            //    lblKey1.Name = "lblKey1";
            //    lblKey1.TextAlign = ContentAlignment.TopLeft;
            //    lblKey1.Location = new Point(4, 160);
            //    lblKey1.Size = new Size(90, 18);
            //    lblKey1.Text = "9 - коррект.";
            //    pnlCurrent.Controls.Add(lblKey1);
            //}

            Label lblKey2 = new Label();
            lblKey2.Font = FontTahoma10Bold;
            lblKey2.BackColor = Color.LightGray;
            lblKey2.Name = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight;
            lblKey2.Location = new Point(CurrWidth - 96, 160);
            lblKey2.Size = new Size(90, 18);
            lblKey2.Text = "Esc - назад";
            pnlCurrent.Controls.Add(lblKey2);

            //PictureBox pbPhoto = new PictureBox();
            //pbPhoto.Name = "pbPhoto";
            //pbPhoto.Location = new Point(0, 0);
            //pbPhoto.Size = new Size(320, 185);
            //pbPhoto.Visible = false;
            //pnlCurrent.Controls.Add(pbPhoto);

            lblAction.ForeColor = Color.Blue;
            lblAction.Text = Obj.ExcStr;

        } 
        private void RefillLayout_review()
        {
            RefillLayout Obj = SS.MM as RefillLayout;
            lblAction.Text = Obj.ExcStr;
            if (Obj.LastMove.Selected)
            {

                if (Obj.LastMove.Adress1.Selected)
                {
                    lblState.Text = "(П " + Obj.LastMove.Pallete.Trim() + ")  " + Obj.LastMove.Adress1.Name.Trim() + "   " + Obj.DocAP.NumberDoc; //Обновляем главную надпись
                }
                else
                {
                    lblState.Text = "(П " + Obj.LastMove.Pallete.Trim() + ")  <без адреса> " + Obj.DocAP.NumberDoc; //Обновляем главную надпись
                }
            }
            else
            {
                lblState.Text = "(П нету)  <нет>   " + Obj.DocAP.NumberDoc; //Обновляем главную надпись
            }
            DataGrid dgGoodsCC = pnlCurrent.GetDataGridByName("dgGoodsCC");
            dgGoodsCC.DataSource = Obj.RemainItems;
            if (!Obj.ReadyPut)
            {
                dgGoodsCC.Visible = true;
                dgGoodsCC.BringToFront();
                return;
            }

            //Режим выкладки строки
            dgGoodsCC.Visible = false;

            //pnlCurrent.GetLabelByName("lblPrevious").Text = SS.PreviousAction;
            Label lblInvCode = pnlCurrent.GetLabelByName("lblInvCode");
            Label lblAdress = pnlCurrent.GetLabelByName("lblAdress");
            Label lblAdress1 = pnlCurrent.GetLabelByName("lblAdress1");
            Label lblCount = pnlCurrent.GetLabelByName("lblCount");
            TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
            Label lblPrevious = pnlCurrent.GetLabelByName("lblPrevious");
            //Label lblHeader = pnlCurrent.GetLabelByName("lblHeader");
            Label lblItem = pnlCurrent.GetLabelByName("lblItem");
            Label lblHeaderPrice = pnlCurrent.GetLabelByName("lblHeaderPrice");
            //Label lblHeaderBalance = pnlCurrent.GetLabelByName("lblHeaderBalance");
            //Label lblHeaderSum = pnlCurrent.GetLabelByName("lblHeaderSum");
            Label lblDetailsCount = pnlCurrent.GetLabelByName("lblDetailsCount");
            //PictureBox pbPhoto = pnlCurrent.GetPictureBoxByName("pbPhoto");
            Label lblKey2 = pnlCurrent.GetLabelByName("lblKey2");
            Label lblAdressZone = pnlCurrent.GetLabelByName("lblAdressZone");

            //lblKey2.Visible = Obj.CollectedLines > 0 ? true : false;

            lblInvCode.ForeColor = Color.Black;
            lblAdress.ForeColor = Color.Black;
            lblCount.ForeColor = Color.Black;
            lblAdress1.ForeColor = Color.Black;

            lblPrevious.Text = Obj.PreviousAction;
            //lblHeader.Text = "Строка " + SS.CCItem.CurrLine.ToString() + " из " + SS.DocSet.Rows.ToString() + " (ост " + SS.AllSetsRow.ToString() + ")";
            lblHeaderPrice.Text = "Цена: " + Obj.Item.Price;
            //lblHeaderBalance.Text = "Ост-ок " + SS.CCItem.Balance.ToString().Trim();
            //lblHeaderSum.Text = "Сумма: " + SS.DocSet.Sum.ToString();
            lblInvCode.Text = Obj.Item.InvCode;
            if (Obj.Item.ZonaHand.Selected)
            {
                lblAdressZone.Text = Obj.Item.ZonaHand.FirstAdress.Name + " .. " + Obj.Item.ZonaHand.LastAdress.Name;
                lblAdressZone.Visible = true;
            }
            else
            {
                lblAdressZone.Visible = false;
            }
            lblAdress.Text = Obj.Adress0.Name;
            lblAdress1.Text = Obj.Adress1.Name;
            lblItem.Text = Obj.Item.Name;
            lblCount.Text = Obj.Amount.ToString() + " шт по 1";
            lblDetailsCount.Text = "Деталей: " + Obj.Item.Details.ToString();
            switch (Obj.CurrentAction)
            {
                case ActionSet.ScanAdress:
                    tbCount.Visible = false;
                    tbCount.Text = "";
                    lblAdress.ForeColor = Color.Blue;
                    break;

                case ActionSet.ScanItem:
                    //pbPhoto.Visible = false;
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

        private void RefillLayoutComplete_view()
        {
            RefillLayoutComplete Obj = SS.MM as RefillLayoutComplete;

            Label lblHeader = new Label();
            lblHeader.Font = FontTahoma12Bold;
            lblHeader.Name = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter;
            lblHeader.Location = new Point(4, 0);
            lblHeader.Size = new Size(CurrWidth - 10, 20);
            pnlCurrent.Controls.Add(lblHeader);

            //Label lblPrinter = new Label();
            //lblPrinter.Font = FontTahoma10Bold;
            //lblPrinter.ForeColor = Color.DarkGreen;
            //lblPrinter.Name = "lblPrinter";
            //lblPrinter.TextAlign = ContentAlignment.TopCenter;
            //lblPrinter.Location = new Point(4, 20 + 2);
            //lblPrinter.Size = new Size(CurrWidth - 10, 20);
            //pnlCurrent.Controls.Add(lblPrinter);

            Label lblClient = new Label();
            lblClient.Font = FontTahoma12Bold;
            lblClient.Name = "lblClient";
            lblClient.TextAlign = ContentAlignment.TopCenter;
            lblClient.Location = new Point(4, 40 + 4);
            lblClient.Size = new Size(CurrWidth - 10, 40);
            pnlCurrent.Controls.Add(lblClient);

            Label lblSelfRemovel = new Label();
            lblSelfRemovel.Font = FontTahoma14Bold;
            lblSelfRemovel.Name = "lblSelfRemovel";
            lblSelfRemovel.ForeColor = Color.Blue;
            lblSelfRemovel.TextAlign = ContentAlignment.TopCenter;
            lblSelfRemovel.Location = new Point(4, 80 + 4);
            lblSelfRemovel.Size = new Size(CurrWidth - 10, 24);
            lblSelfRemovel.Text = "Подтвердите, что все готово";
            pnlCurrent.Controls.Add(lblSelfRemovel);

            ////Поле для ввода количества
            //TextBox tbCount = new TextBox();
            //tbCount.Name = "tbCount";
            //tbCount.Location = new Point(120, 140 + 10);
            //tbCount.Size = new Size(60, 28);
            //tbCount.Font = FontTahoma14Bold;
            //tbCount.Text = "";
            //tbCount.Visible = false;
            //pnlCurrent.Controls.Add(tbCount);

            Label lblKey1 = new Label();
            lblKey1.Font = FontTahoma8Regular;
            lblKey1.ForeColor = Color.White;
            lblKey1.BackColor = Color.Green;
            lblKey1.Name = "lblKey1";
            lblKey1.TextAlign = ContentAlignment.TopLeft;
            lblKey1.Location = new Point(4, 165);
            lblKey1.Size = new Size(110, 18);
            lblKey1.Text = "ЗАВЕРШИТЬ";
            pnlCurrent.Controls.Add(lblKey1);

            //DrawlblResult();
            //pnlCurrent.GetLabelByName("lblResult").Text = "Отсканируйте коробку и введите количество мест!";
            lblAction.Text = Obj.ExcStr;
        } 
        private void RefillLayoutComplete_review()
        {
            RefillLayoutComplete Obj = SS.MM as RefillLayoutComplete;

            lblState.Text = "Завершение выкладки";
            //Label lblResult = pnlCurrent.GetLabelByName("lblResult");
            //Label lblPrinter = pnlCurrent.GetLabelByName("lblPrinter");
            Label lblHeader = pnlCurrent.GetLabelByName("lblHeader");
            Label lblClient = pnlCurrent.GetLabelByName("lblClient");
            Label lblSelfRemovel = pnlCurrent.GetLabelByName("lblSelfRemovel");
            //TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
            //lblSelfRemovel.Text = Obj.PalleteDescr;
            lblHeader.Text = "Отобрано " + Obj.DocAP.RowCount.ToString() + " строк";
            lblClient.Text = Obj.DocAP.View;
            //lblState.Text = SS.DocSet.View;
            //if (!SS.BoxOk)
            //{
            //    tbCount.Visible = true;
            //    tbCount.Focus();
            //}
            //lblPrinter.Text = SS.Printer.Description;
            //if (SS.BoxOk)
            //{
            //    pnlCurrent.GetLabelByName("lblResult").Text = "Всего " + SS.DocSet.Boxes.ToString() + " мест! Отсканируйте ШК предкомплектации!";
            //    tbCount.Visible = false;
            //}
        } 

        private void RefillSetCorrect_view()
        {
            RefillSetCorrect Obj = SS.MM as RefillSetCorrect;

            Label lblHeader = new Label();
            lblHeader.Font = FontTahoma12Bold;
            lblHeader.ForeColor = Color.Red;
            lblHeader.Name = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter;
            lblHeader.Location = new Point(4, 0);
            lblHeader.Size = new Size(CurrWidth - 10, 20);
            lblHeader.Text = "Корректировка позиции " + Obj.Item.InvCode;
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
            lblAnswer1.ForeColor = Color.LightGray;
            pnlCurrent.Controls.Add(lblAnswer1);

            Label lblAnswer2 = new Label();
            lblAnswer2.Font = FontTahoma12Bold;
            lblAnswer2.Name = "lblAnswer2";
            lblAnswer2.TextAlign = ContentAlignment.TopLeft;
            lblAnswer2.Location = new Point(100, 60 + 6);
            lblAnswer2.Size = new Size(CurrWidth - 50, 20);
            lblAnswer2.Text = "2 - недостача";
            pnlCurrent.Controls.Add(lblAnswer2);

            Label lblAnswer3 = new Label();
            lblAnswer3.Font = FontTahoma12Bold;
            lblAnswer3.Name = "lblAnswer3";
            lblAnswer3.TextAlign = ContentAlignment.TopLeft;
            lblAnswer3.Location = new Point(100, 80 + 8);
            lblAnswer3.Size = new Size(CurrWidth - 50, 20);
            lblAnswer3.Text = "3 - отказ";
            lblAnswer3.ForeColor = Color.LightGray;
            pnlCurrent.Controls.Add(lblAnswer3);

            Label lblAnswer4 = new Label();
            lblAnswer4.Font = FontTahoma12Bold;
            lblAnswer4.Name = "lblAnswer4";
            lblAnswer4.TextAlign = ContentAlignment.TopLeft;
            lblAnswer4.Location = new Point(100, 100 + 10);
            lblAnswer4.Size = new Size(CurrWidth - 50, 20);
            lblAnswer4.Text = "4 - отказ без ШК";
            lblAnswer4.ForeColor = Color.LightGray;
            lblAnswer4.Visible = false;
            pnlCurrent.Controls.Add(lblAnswer4);

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
            lblAction.Text = Obj.ExcStr;
        } 
        private void RefillSetCorrect_review()
        {
            RefillSetCorrect Obj = SS.MM as RefillSetCorrect;

            if (Obj.ChoiseCorrect == 0)
            {
                //пока не сделан выбор - все тлен
                return;
            }
            for (int j = 0; j < 5; j++)
            {
                Label lblTmp = pnlCurrent.GetLabelByName("lblAnswer" + j.ToString());
                if (j == Obj.ChoiseCorrect)
                {
                    lblTmp.ForeColor = Color.Green;
                }
                else
                {
                    lblTmp.Visible = false;
                }
            }
            pnlCurrent.GetLabelByName("lblKey1").Visible = true;
            TextBox tbCount = pnlCurrent.GetTextBoxByName("tbCount");
            tbCount.Visible = true;
            tbCount.Focus();
        } 

        private void SetTransfer_view()
        {
            SetTransfer Obj = SS.MM as SetTransfer;

            lblState.Text = "Заявка " + Obj.Proposal.NumberDoc + " (" + Obj.Proposal.DateDoc.ToString("dd.MM.yy") + ")";
            Label lblHeader = new Label();
            lblHeader.Font = FontTahoma12Bold;
            lblHeader.Name = "lblHeader";
            lblHeader.TextAlign = ContentAlignment.TopCenter;
            lblHeader.Location = new Point(4, 0);
            lblHeader.Size = new Size(CurrWidth - 10, 20);
            lblHeader.Text = Obj.SectorCC.Name.Trim() + "-" + Obj.DocCC.GetAttributeHeader("НомерЛиста").ToString();
            pnlCurrent.Controls.Add(lblHeader);

            Label lblPrinter = new Label();
            lblPrinter.Font = FontTahoma10Bold;
            lblPrinter.ForeColor = Color.DarkGreen;
            lblPrinter.Name = "lblPrinter";
            lblPrinter.TextAlign = ContentAlignment.TopCenter;
            lblPrinter.Location = new Point(4, 20 + 2);
            lblPrinter.Size = new Size(CurrWidth - 10, 20);
            lblPrinter.Text = "Наборщик - " + Helper.GetShortFIO(Obj.Setter.Name);
            pnlCurrent.Controls.Add(lblPrinter);

            Label lblClient = new Label();
            lblClient.Font = FontTahoma12Bold;
            lblClient.Name = "lblClient";
            lblClient.ForeColor = Color.Blue;
            lblClient.TextAlign = ContentAlignment.TopCenter;
            lblClient.Location = new Point(4, 40 + 4);
            lblClient.Size = new Size(CurrWidth - 10, 40);
            pnlCurrent.Controls.Add(lblClient);

            Label lblSelfRemovel = new Label();
            lblSelfRemovel.Font = FontTahoma14Bold;
            lblSelfRemovel.Name = "lblSelfRemovel";
            lblSelfRemovel.ForeColor = Color.Blue;
            lblSelfRemovel.TextAlign = ContentAlignment.TopCenter;
            lblSelfRemovel.Location = new Point(4, 80 + 4);
            lblSelfRemovel.Size = new Size(CurrWidth - 10, 24);
            pnlCurrent.Controls.Add(lblSelfRemovel);

            Label lblKey2 = new Label();
            lblKey2.Font = FontTahoma10Bold;
            lblKey2.BackColor = Color.Red;
            lblKey2.ForeColor = Color.White;
            lblKey2.Name = "lblKey2";
            lblKey2.TextAlign = ContentAlignment.TopRight;
            lblKey2.Location = new Point(CurrWidth - 96, 160);
            lblKey2.Size = new Size(90, 18);
            lblKey2.Text = "Esc - ОТКАЗ";
            pnlCurrent.Controls.Add(lblKey2);

            DrawlblResult();
            lblAction.Text = "Ожидание команды";
            //if (SS.GetWindow.Selected)
            //{
            //    pnlCurrent.GetLabelByName("lblResult").Text = "Выдайте товар!";
            //}
            //else
            //{
            //    pnlCurrent.GetLabelByName("lblResult").Text = "Отложите товар в зону самовывоза товар!";
            //}
        } 
        private void SetTransfer_review()
        {
            SetTransfer Obj = SS.MM as SetTransfer;

            Label lblResult = pnlCurrent.GetLabelByName("lblResult");
            Label lblPrinter = pnlCurrent.GetLabelByName("lblPrinter");
            Label lblHeader = pnlCurrent.GetLabelByName("lblHeader");
            Label lblClient = pnlCurrent.GetLabelByName("lblClient");
            Label lblSelfRemovel = pnlCurrent.GetLabelByName("lblSelfRemovel");

            if (Obj.CompleteAdress.Selected && !Obj.TransferWindow.Selected)
            {
                lblSelfRemovel.Text = "комплектовать в " + Obj.CompleteAdress.Name;
            }

            if (Obj.TransferWindow.Selected)
            {
                lblClient.Text = "Подтвердтите выдачу товара клиенту сканированием заявки!";
            }
            else
            {
                lblClient.Text = "Положите товар на паллету!";
            }
            lblResult.Text = "Окно выдачи " + (Obj.TransferWindow.Selected ? Obj.TransferWindow.Name : "не задано");
            //if (SS.DocSet.ID != null)
            //{
            //    lblHeader.Text = "Отобрано " + SS.DocSet.Rows.ToString() + " строк";
            //    lblClient.Text = SS.DocSet.Client;
            //    lblState.Text = SS.DocSet.View;
            //}
            //lblPrinter.Text = SS.Printer.Description;
        }

        private void ItemCard_view()
        {
            ItemCard Obj = SS.MM as ItemCard;

            ItemCard_review();

            lblAction.Text = Obj.ExcStr;
        }         //Погрузчик выбор (лифт)
        private void ItemCard_review()
        {
            ItemCard Obj = SS.MM as ItemCard;
            lblState.Text = Obj.AcceptedItem.InvCode + " Карточка товара";
            ScreanAcceptedItem = 1;
            SS.AcceptedItem.AcceptCount = SS.AcceptedItem.Count;
            if (SS.AcceptedItem.ToMode == Mode.SampleInventory || SS.AcceptedItem.ToMode == Mode.SamplePut || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut)
            {
                CurrentRowItem = 0;//У образцов и гармонизации - главная штука
            }
            else
            {
                CurrentRowItem = 2;//У остальных - главное место
            }
            #region шняга 
            /*ViewUnits = new DataTable();
            ViewUnits.Columns.Add("Coef", Type.GetType("System.Int32"));
            ViewUnits.Columns.Add("OKEI", Type.GetType("System.String"));
            ViewUnits.Columns.Add("Barcodes", Type.GetType("System.String"));

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
            tbCoef.Name = "tbCoef";
            tbCoef.Location = new Point(-1 * ScreanAcceptedItem * pnlCurrent.Width + (CurrWidth < 320 ? 30 : 40), 30 + 26 * (CurrentRowItem + 1));
            tbCoef.Size = new Size(CurrWidth < 320 ? 40 : 60, 27);
            tbCoef.Font = CurrWidth < 320 ? FontTahoma10Bold : FontTahoma12Bold;
            tbCoef.Enabled = SS.AcceptedItem.ToMode == Mode.Transfer || SS.AcceptedItem.ToMode == Mode.NewInventory || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut ? false : true;
            //tbCoef.Text         = ViewUnits.Rows[CurrentRowItem]["Coef"].ToString();
            tbCoef.KeyPress += DynamicOnKeyPress;
            tbCoef.TextChanged += DynamicOnTextChanged;
            tbCoef.LostFocus += DynamicLostFocus;
            tbCoef.GotFocus += DynamicGotFocus;
            pnlCurrent.Controls.Add(tbCoef);

            TextBox tbCount = new TextBox();
            tbCount.Name = "tbCount";
            tbCount.Location = new Point(-1 * ScreanAcceptedItem * pnlCurrent.Width + (CurrWidth < 320 ? 75 : 105), 30 + 26 * (CurrentRowItem + 1));
            tbCount.Size = new Size(CurrWidth < 320 ? 48 : 60, 27);
            tbCount.Font = CurrWidth < 320 ? FontTahoma10Bold : FontTahoma12Bold;
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
            tbCount.KeyPress += DynamicOnKeyPress;
            tbCount.TextChanged += DynamicOnTextChanged;
            tbCount.LostFocus += DynamicLostFocus;
            tbCount.GotFocus += DynamicGotFocus;
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
            lblUnit.Font = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma8Regular;
            lblUnit.BackColor = Color.White;
            lblUnit.Name = "lblUnit";
            lblUnit.TextAlign = ContentAlignment.TopRight;
            lblUnit.Location = new Point(4, 58);
            lblUnit.Size = new Size(CurrWidth < 320 ? 26 : 36, 24);
            lblUnit.Text = "Шт.";
            pnlCurrent.Controls.Add(lblUnit);

            Label lblPack = new Label();
            lblPack.Font = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma8Regular;
            lblPack.BackColor = Color.White;
            lblPack.Name = "lblPack";
            lblPack.TextAlign = ContentAlignment.TopRight;
            lblPack.Location = new Point(4, 83);
            lblPack.Size = new Size(CurrWidth < 320 ? 26 : 36, 24);
            lblPack.Text = "Упак.";
            pnlCurrent.Controls.Add(lblPack);

            Label lblPlace = new Label();
            lblPlace.Font = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma8Regular;
            lblPlace.BackColor = Color.White;
            lblPlace.Name = "lblPlace";
            lblPlace.TextAlign = ContentAlignment.TopRight;
            lblPlace.Location = new Point(4, 108);
            lblPlace.Size = new Size(CurrWidth < 320 ? 26 : 36, 24);
            lblPlace.Text = "Место";
            pnlCurrent.Controls.Add(lblPlace);

            Label lblKit = new Label();
            lblKit.Font = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma8Regular;
            lblKit.ForeColor = Color.Red;
            lblKit.BackColor = Color.White;
            lblKit.Name = "lblKit";
            lblKit.TextAlign = ContentAlignment.TopRight;
            lblKit.Location = new Point(4, 133);
            lblKit.Size = new Size(CurrWidth < 320 ? 26 : 36, 24);
            lblKit.Text = "Набор";
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
            lblC.Font = FontTahoma14Bold;
            lblC.BackColor = Color.Thistle;
            lblC.Name = "lblC";
            lblC.TextAlign = ContentAlignment.TopCenter;
            lblC.Location = new Point(CurrWidth < 320 ? 30 : 40, 160);
            lblC.Size = new Size(CurrWidth < 320 ? 50 : 60, 25);
            lblC.Text = SS.AcceptedItem.StoregeSize.ToString();
            lblC.Visible = SS.AcceptedItem.StoregeSize != 0;
            pnlCurrent.Controls.Add(lblC);

            Label lblA = new Label();
            lblA.Font = FontTahoma10Regular;
            lblA.BackColor = Color.White;
            lblA.Name = "lblA";
            lblA.TextAlign = ContentAlignment.TopCenter;
            lblA.Location = new Point(CurrWidth < 320 ? 75 : 110, 160);
            lblA.Size = new Size(50, 25);
            lblA.Text = "Кол-во";
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
                lblCoef.Font = CurrWidth < 320 ? FontTahoma10Bold : FontTahoma12Bold;
                lblCoef.BackColor = Color.LightGray;
                lblCoef.Name = "lblCoef" + i.ToString();
                lblCoef.TextAlign = ContentAlignment.TopLeft;
                lblCoef.Location = new Point(CurrWidth < 320 ? 30 : 40, 30 + 26 * (i + 1));
                lblCoef.Size = new Size(CurrWidth < 320 ? 40 : 60, 25);
                pnlCurrent.Controls.Add(lblCoef);

                Label lblCount = new Label();
                lblCount.Font = CurrWidth < 320 ? FontTahoma10Bold : FontTahoma12Bold;
                lblCount.BackColor = Color.LightGray;
                lblCount.Name = "lblCount" + i.ToString();
                lblCount.TextAlign = ContentAlignment.TopLeft;
                lblCount.Location = new Point(CurrWidth < 320 ? 75 : 105, 30 + 26 * (i + 1));
                lblCount.Size = new Size(CurrWidth < 320 ? 48 : 60, 25);
                pnlCurrent.Controls.Add(lblCount);

                Label lblCountBegin = new Label();
                lblCountBegin.Font = CurrWidth < 320 ? FontTahoma10Regular : FontTahoma12Regular;
                lblCountBegin.ForeColor = (i == 3 ? Color.White : Color.Black);
                lblCountBegin.BackColor = (i == 3 ? Color.Red : Color.LightGray);
                lblCountBegin.Name = "lblCountBegin" + i.ToString();
                lblCountBegin.TextAlign = ContentAlignment.TopLeft;
                lblCountBegin.Location = new Point(CurrWidth < 320 ? 128 : 170, 30 + 26 * (i + 1));
                lblCountBegin.Size = new Size(CurrWidth < 320 ? 35 : 50, 25);
                pnlCurrent.Controls.Add(lblCountBegin);

                Label lblBarcodes = new Label();
                lblBarcodes.Font = CurrWidth < 320 ? FontTahoma6Regular : FontTahoma6Regular;
                lblBarcodes.BackColor = Color.LightGray;
                lblBarcodes.Name = "lblBarcodes" + i.ToString();
                lblBarcodes.TextAlign = ContentAlignment.TopLeft;
                lblBarcodes.Location = new Point(CurrWidth < 320 ? 165 : 225, 30 + 26 * (i + 1));
                lblBarcodes.Size = new Size(CurrWidth < 320 ? 72 : 89, 25);
                pnlCurrent.Controls.Add(lblBarcodes);
            }

            Button btnOk = new Button();
            btnOk.Name = "btnOk";
            btnOk.Text = (SS.AcceptedItem.Acceptance ? "Принять" : "Сохранить");
            btnOk.Location = new Point(CurrWidth < 320 ? 167 : 225, 160);
            btnOk.Size = new Size(CurrWidth < 320 ? 68 : 89, 25);
            btnOk.Click += DynamicButtonOnClick;
            if ((SS.AcceptedItem.ToMode == Mode.Transfer || SS.AcceptedItem.ToMode == Mode.NewInventory || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut) && SS.AcceptedItem.OnShelf)
            {
                btnOk.Enabled = false;
            }
            pnlCurrent.Controls.Add(btnOk);
            */
            #endregion
            
            Label lblHeaderPrice = new Label();
            lblHeaderPrice.Font = CurrWidth < 320 ? FontTahoma7Bold : FontTahoma8Bold;
            lblHeaderPrice.ForeColor = Color.DarkRed;
            lblHeaderPrice.Name = "lblHeaderPrice";
            lblHeaderPrice.TextAlign = ContentAlignment.TopRight;
            lblHeaderPrice.Location = new Point(CurrWidth < 320 ? 96 : 155, 159);
            lblHeaderPrice.Size = new Size(70, 26);
            lblHeaderPrice.Text = "Цена: " + Obj.AcceptedItem.Price.ToString().Trim();
            pnlCurrent.Controls.Add(lblHeaderPrice);

            //Заголовок артикул
            Label lblHeaderArticle = new Label();
            lblHeaderArticle.Font = FontTahoma8Regular;
            //lblHeaderArticle.BackColor = Color.White;
            lblHeaderArticle.BackColor = Color.Cyan;
            lblHeaderArticle.Name = "lblHeaderArticle";
            lblHeaderArticle.TextAlign = ContentAlignment.TopLeft;
            lblHeaderArticle.Location = new Point(CurrWidth < 320 ? 0 : 4, 0);
            lblHeaderArticle.Size = new Size(CurrWidth < 320 ? 130 : 205, 50);
            lblHeaderArticle.Text = Obj.AcceptedItem.Acticle.Trim() + " " + Obj.AcceptedItem.Name.Trim();
            pnlCurrent.Controls.Add(lblHeaderArticle);

            Label lblMain = new Label();
            lblMain.Font = FontTahoma8Regular;
            lblMain.ForeColor = Color.DarkBlue;
            lblMain.BackColor = Color.Yellow;
            lblMain.Name = "lblMain";
            lblMain.TextAlign = ContentAlignment.TopRight;
            lblMain.Location = new Point(CurrWidth < 320 ? 129 : 209, 0);
            lblMain.Size = new Size(105, 15);
            lblMain.Text = Obj.AcceptedItem.AdressMain.Trim() + ": " + Obj.AcceptedItem.BalanceMain.ToString() + " шт";
            pnlCurrent.Controls.Add(lblMain);

            Label lblBuffer = new Label();
            lblBuffer.Font = FontTahoma8Regular;
            lblBuffer.BackColor = Color.Yellow;
            lblBuffer.Name = "lblBuffer";
            lblBuffer.TextAlign = ContentAlignment.TopRight;
            lblBuffer.Location = new Point(CurrWidth < 320 ? 129 : 209, 15);
            lblBuffer.Size = new Size(105, 15);
            lblBuffer.Text = Obj.AcceptedItem.AdressBuffer.Trim() + ": " + Obj.AcceptedItem.BalanceBuffer.ToString() + " шт";
            pnlCurrent.Controls.Add(lblBuffer);

           /* if ((int)ViewUnits.Rows[CurrentRowItem]["Coef"] > 0 && SS.AcceptedItem.Acceptance || SS.AcceptedItem.ToMode == Mode.Transfer || SS.AcceptedItem.ToMode == Mode.NewInventory || SS.AcceptedItem.ToMode == Mode.Harmonization || SS.AcceptedItem.ToMode == Mode.HarmonizationPut)
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
            tbCount.BringToFront();*/

            //begin govnocode
            /*Label lblHeaderDetail = new Label();
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
            tbDetails.Text = Obj.AcceptedItem.NowDetails.ToString();
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
            pnlCurrent.Controls.Add(lblZoneAdress);*/

            //Костыль, временно, чтобы не прортить FExcStr

            string tmpExc = Obj.ExcStr;  //Это тоже жесть, смотри в ExcStr самого родительского класса SQLSynchronizer

            RefItem Item = new RefItem(SS);
            Item.FoundID(Obj.AcceptedItem.ID);


            bool IsAndressHand = false;
            if (Item.ZonaHand.Selected)
            {
                if (Item.ZonaHand.FirstAdress.Selected)
                {
                    IsAndressHand = true;
                }
            }
            SS.ExcStr = tmpExc;
            if (Obj.AcceptedItem.ToMode == Mode.Transfer && IsAndressHand)
            {
                //lblZoneAdress.BackColor = Color.Yellow;
                lblBuffer.BackColor = Color.White;
                lblMain.BackColor = Color.White;
            }
            /*tbDetails.KeyPress += DynamicOnKeyPress;
            tbDetails.TextChanged += DynamicOnTextChanged;
            tbDetails.LostFocus += DynamicLostFocus;
            tbDetails.GotFocus += DynamicGotFocus;
            pnlCurrent.Controls.Add(tbDetails);*/
            //end add code

            if (!Obj.AcceptedItem.Acceptance)
            {
                //Это не приемка, а редактирование карточки, значит будем выводить кучу разной фигни одполнительно
                DataGridTextBoxColumn columnStyle;
                DataGridTableStyle dgts;

                DataGrid dgInfo = new DataGrid();
                dgInfo.Location = new Point(-1 * ScreanAcceptedItem * pnlCurrent.Width + (CurrWidth + 2), 50);
                dgInfo.Name = "dgInfo";
                dgInfo.Size = new System.Drawing.Size(CurrWidth - 10, 110);
                dgInfo.DataSource = Obj.AdressConditionItem;
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
             
           /*
            Label lblItemTest = new Label();
            lblItemTest.Font = FontTahoma14Bold;
            lblItemTest.Name = "lblItemTest";
            lblItemTest.ForeColor = Color.Blue;
            lblItemTest.TextAlign = ContentAlignment.TopCenter;
            lblItemTest.Location = new Point(4, 80 + 4);
            lblItemTest.Size = new Size(CurrWidth - 10, 24);
            lblItemTest.Text = Obj.Item.Name;
            pnlCurrent.Controls.Add(lblItemTest);*/

        } 

    
    }
}
