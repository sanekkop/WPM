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
                case Mode.AcceptanceCross:
                    RKAcceptenceCross(Key, currControl);
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
        private void RKAcceptenceCross(Keys Key, Control currControl)
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
            else if (Key == Keys.Left && Screan == -1)
            {
                DataGrid currDG = currControl as DataGrid;
                if (currDG.CurrentRowIndex != -1)
                {
                    DataRowView currDRV = (currDG.DataSource as BindingSource).Current as DataRowView;

                    BindingSource BS = pnlCurrent.GetDataGridByName("dgNotAcceptedItemsCross").DataSource as BindingSource;
                    BS.Filter = "ClientID = '" + currDRV.Row["ClientID"].ToString() + "'"
                          + " and OrderID = '" + currDRV.Row["OrderID"].ToString() + "'";
                    pnlCurrent.Sweep(1);
                    Screan -= 1;
                    pnlCurrent.GetControlByName("dgNotAcceptedItemsCross").Focus();
                }
            }
            else if (Key == Keys.Right && Screan == -2)
            {
                BindingSource BS = pnlCurrent.GetDataGridByName("dgNotAcceptedItemsCross").DataSource as BindingSource;
                BS.Filter = "";
                pnlCurrent.Sweep(-1);
                Screan += 1;
                pnlCurrent.GetControlByName("dgNotAcceptedItems").Focus();

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
                else if (currControl.Name == "tbLabelCount")
                {
                    return;
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
                        SS.ChoiseItemAcceptenceCross(
                            currDRV.Row["ID"].ToString(),
                            currDRV.Row["IDDOC"].ToString(),
                            currDRV.Row["OrderID"].ToString());
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
            if (Key == Keys.Escape && (SS.CurrentAction != ActionSet.Waiting))
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
            else if (Key == Keys.Space && Screan == 0)
            {
                DynamicButtonOnClick(pnlCurrent.GetControlByName("btnRefresh"), null);
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
        
    }
}
