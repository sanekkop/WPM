using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    partial class Model
    {

        public bool ReactionCancel()
        {
            //Заглушка временнаяы
            if (NewStructModes.Contains(FCurrentMode))
            {
                ABaseMode tmpMM = MM;
                MM = MM.Cancel();
                FEmployer = MM.Employer;    //Временно
                Const = MM.Const;           //Временно, рефрешим константы
                if (tmpMM.CurrentMode != MM.CurrentMode)
                {
                    OnChangeMode(new ChangeModeEventArgs(MM.CurrentMode));
                }
                else
                {
                    OnReport(new ReportEventArgs(MM.ExcStr));
                }
                return MM.Result == MMResult.Positive ? true : false;
            }
            FExcStr = null;
            //Обновим параметры сотрудника, если он задан
            if (FEmployer != null)
            {
                FEmployer.Refresh();
            }
            switch (FCurrentMode)
            {
                case Mode.LoaderChoise:
                    return ToModeChoiseWork();
                case Mode.FreeDownComplete:
                    if (DocDown.ID == null)
                    {
                        return ToModeChoiseWork();
                    }
                    else
                    {
                        return ToModeWaiting();
                    };
                case Mode.SetCorrect:
                    return ToModeSet(null, null);
                case Mode.ChoiseWork:
                    return ToModeWaiting();
                case Mode.Set:
                    QuitModesSet();
                    if (Const.BoxSetOn)
                    {
                        return ToModeSetInicializationRequest();
                    }
                    else
                    {
                        return ToModeChoiseWork();
                    }
                case Mode.SetSelfContorl:
                    QuitModesSet();
                    return ToModeChoiseWork();
                case Mode.SetInicialization:
                    return ToModeChoiseWork();
                case Mode.SetComplete:
                    QuitModesSet();
                    return ToModeChoiseWork();
                case Mode.Acceptance:
                    QuitModeAcceptance();
                    return ToModeChoiseWork();
                case Mode.Inventory:
                    return ToModeChoiseWork();
                case Mode.NewInventory:
                    QuitModeTransfer();
                    return ToModeChoiseWork();
                case Mode.ChoiseInventory:
                    return ToModeChoiseWork();
                case Mode.NewChoiseInventory:
                    return ToModeChoiseWork();
                case Mode.TransferInicialize:
                    return ToModeChoiseWork();
                case Mode.SampleInventory:
                    return ToModeChoiseWork();
                case Mode.Transfer:
                    QuitModeTransfer();
                    return ToModeChoiseWork();
                case Mode.AcceptedItem:
                    QuitModeAcceptedItem(); //Тут не важно смог снять или нет блокировку, выходить то как-то надо
                    if (AcceptedItem.ToMode == Mode.Acceptance)
                    {
                        if (!ToModeAcceptance())
                        {
                            //Если не получилось обновить, то вылетаем в главное меню!
                            FCurrentMode = Mode.Acceptance;
                            return ReactionCancel();
                        }
                    }
                    else
                    {
                        FCurrentMode = AcceptedItem.ToMode;
                    }
                    return true;
                case Mode.SamplePut:
                    QuitModeSamplePut();
                    return ToModeChoiseWork();
                case Mode.SampleSet:
                    return ToModeChoiseWork();
                case Mode.SampleSetCorrect:
                    return ToModeSampleSet();
                case Mode.HarmonizationInicialize:
                    return ToModeChoiseWork();
                case Mode.Harmonization:
                    return ToModeChoiseWork();
                case Mode.HarmonizationPut:
                    return ToModeChoiseWork();
                case Mode.Loading:
                    QuitModeLoading();
                    return ToModeChoiseWork();
                case Mode.LoadingInicialization:
                    return ToModeChoiseWork();
                case Mode.ChoiseDown:
                    return ToModeChoiseWork();
                case Mode.Down:
                    return ToModeWaiting();
                case Mode.DownComplete:
                    return ToModeWaiting();
                case Mode.NewComplectation:
                    return ToModeWaiting();
                case Mode.NewComplectationComplete:
                    return ToModeWaiting();
                default:
                    FExcStr = "Недопустимая команда в текущем режиме!";
                    return false;
            }
        }
        public bool ReactionSC(string IDD)
        {
            return ReactionSC(IDD, false);
        }
        public bool ReactionSC(string IDD, bool thisID)
        {
            FExcStr = null;
            switch (CurrentMode)
            {
                case Mode.Waiting:
                    if (IsSC(IDD, "Сотрудники"))
                    {
                        return ToModeChoiseWork(IDD);
                    }
                    else
                    {
                        FExcStr = "Не верный тип справочника!";
                        return false;
                    }

                case Mode.NewComplectationComplete:
                    return RSCNewComplectationComplete(IDD, thisID);

                case Mode.NewComplectation:
                    return RSCNewComplectation(IDD, thisID);

                case Mode.FreeDownComplete:
                    return RSCFreeDownComplete(IDD, thisID);

                case Mode.DownComplete:
                    return RSCDownComplete(IDD);

                case Mode.Down:
                    return RSCDown(IDD, thisID);

                case Mode.Set:
                    return RSCSet(IDD, thisID);

                case Mode.SetInicialization:
                    return RSCSetInicialization(IDD);

                case Mode.SetComplete:
                    return RSCSetComplete(IDD);

                case Mode.Acceptance:
                    return RSCAcceptance(IDD);

                case Mode.TransferInicialize:
                    return ToModeChoiseWork(IDD);

                case Mode.Transfer:
                    return RSCTransfer(IDD);

                case Mode.NewInventory:
                    return RSCTransfer(IDD);

                case Mode.Inventory:
                    return ToModeChoiseWork(IDD);

                case Mode.ChoiseInventory:
                    return ToModeChoiseWork(IDD);

                case Mode.SampleInventory:
                    return RSCSampleInventory(IDD);
                //goto case Mode.ChoiseWork;

                case Mode.SamplePut:
                    return RSCSamplePut(IDD);

                case Mode.AcceptedItem:
                    return RSCAcceptanceItem(IDD);

                case Mode.SampleSet:
                    return RSCSampleSet(IDD, thisID);

                case Mode.LoadingInicialization:
                    return RSCLoadingInicialization(IDD);

                case Mode.Loading:
                    return RSCLoading(IDD); //может быть и ID справочника

                case Mode.ChoiseDown:
                    return RSCChoiseDown(IDD);

                default:
                    FExcStr = "Нет действий с данным справочником в данном режиме!";
                    return false;
            }
        }
        public bool ReactionDoc(string IDD)
        {
            FExcStr = null;
            switch (CurrentMode)
            {
                case Mode.Waiting:
                    FExcStr = "В данном режиме документы не обрабатываются!";
                    return false;

                case Mode.ChoiseWork:
                    goto case Mode.Waiting;

                case Mode.ChoiseWorkShipping:
                    goto case Mode.Waiting;

                case Mode.Acceptance:
                    return RDAcceptance(IDD);

                case Mode.SamplePut:
                    return RDSamplePut(IDD);

                case Mode.SampleSet:
                    return RDSampleSet(IDD, null);

                case Mode.LoadingInicialization:
                    return RDLoadingInicialization(IDD);

                case Mode.SetInicialization:
                    return RDSetInicialization(IDD);

				case Mode.ChoiseWorkAcceptance:
                    goto case Mode.Waiting;
                default:
                    FExcStr = "Нет действий с данным документом в данном режиме!";
                    return false;
            }
        }
        public bool ReactionBarcode(string Barcode, int Screan)
        {
            string TextQuery;
            DataTable DT;
            FExcStr = null;
            switch (CurrentMode)
            {
                case Mode.SampleInventory:
                    return RBSample(Barcode, Mode.SampleInventory, Screan);
                case Mode.SamplePut:
                    return RBSample(Barcode, Mode.SamplePut, 0);
                case Mode.Acceptance:
                    TextQuery =
                        "SELECT " +
                            "Units.parentext as ItemID, " +
                            "Units.$Спр.ЕдиницыШК.ОКЕИ as OKEI " +
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
                    FFlagBarcode = (DT.Rows[0]["OKEI"].ToString() == OKEIPackage ? 2 : 1);
                    return ToModeAcceptedItem(DT.Rows[0]["ItemID"].ToString(), "");

                case Mode.Set:
                    return RBSet(Barcode);

                case Mode.SampleSet:
                    return RBSampleSet(Barcode);

                case Mode.SetSelfContorl:
                    return RBSetSelfControl(Barcode);

                case Mode.AcceptedItem:
                    TextQuery =
                        "SELECT " +
                            "Goods.$Спр.Товары.ИнвКод as InvCode " +
                        "FROM " +
                            "$Спр.ЕдиницыШК as Units (nolock) " +
                            "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                                "ON Goods.ID = Units.parentext " +
                        "WHERE " +
                            "Units.$Спр.ЕдиницыШК.Штрихкод = :Barcode " +
                            " and not Units.parentext = :CurrentItem ";
                    QuerySetParam(ref TextQuery, "Barcode", Barcode);
                    QuerySetParam(ref TextQuery, "CurrentItem", AcceptedItem.ID);
                    if (!ExecuteWithRead(TextQuery, out DT))
                    {
                        return false;
                    }
                    if (DT.Rows.Count > 0)
                    {
                        FExcStr = "Данный штрихкод принадлежит другому товару: " + DT.Rows[0]["InvCode"].ToString() + "!";
                        return false;
                    }
                    return true;

                case Mode.Transfer:
                    return RBTransfer(Barcode);

                case Mode.NewInventory:
                    return RBTransfer(Barcode);

                case Mode.Inventory:
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
                    return ToModeAcceptedItem(DT.Rows[0]["ItemID"].ToString(), "", Mode.Inventory);

                case Mode.Harmonization:
                    return RBHarmonization(Barcode, Mode.Harmonization);

                case Mode.HarmonizationPut:
                    return RBHarmonization(Barcode, Mode.HarmonizationPut);

                default:
                    FExcStr = "Нет действий с этим штирхкодом в данном режиме!";
                    return false;
            }
        }
    }
}
