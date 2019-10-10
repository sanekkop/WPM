using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    //Тут собраны методы необходимые для перехода от старой педальной архитектуры приложения к новой классной
    partial class Model
    {
        private bool ToModeChoiseWork(string IDDEmployer)
        {
            return Init(new ChoiseWork(this, null, IDDEmployer));
        } // ToModeChoiseWork
        private bool ToModeChoiseWork()
        {
            return ToModeChoiseWork(Employer.IDD);
        } // ToModeChoiseWork
        public bool ChoiseWork(Mode DesireMode)
        {
            FExcStr = null;
            if (!(FCurrentMode == Mode.ChoiseWork || FCurrentMode == Mode.ChoiseWorkShipping || FCurrentMode == Mode.ChoiseWorkSupply || FCurrentMode == Mode.ChoiseWorkSample || FCurrentMode == Mode.ChoiseWorkAcceptance))
            {
                FExcStr = "Недопустимая команда в текущем режиме!";
                return false;
            }

            bool result = false;

            switch (DesireMode)
            {
                case Mode.Acceptance:
                    if (FConsignment != null)
                    {
                        FConsignment.Rows.Clear();
                    }
                    result = ToModeAcceptance();
                    break;
                case Mode.TransferInicialize:
                    result = ToModeTransferInicialize();
                    break;
                case Mode.ChoiseInventory:
                    result = ToModeChoiseInventory();
                    break;
                case Mode.SampleInventory:
                    result = ToModeSampleInventory();
                    break;
                case Mode.SamplePut:
                    SampleDoc = null;
                    result = ToModeSamplePut();
                    break;
                case Mode.NewChoiseInventory:
                    result = ToModeNewChoiseInventory();
                    break;
                case Mode.SampleSet:
                    result = ToModeSampleSet();
                    break;
                case Mode.HarmonizationInicialize:
                    result = ToModeHarmonizationInicialize();
                    break;
                case Mode.LoadingInicialization:
                    result = ToModeLoadingInicialization();
                    break;
                case Mode.SetInicialization:
                    FEmployer.Refresh();
                    result = ToModeSetInicialization();
                    break;
                case Mode.ChoiseDown:
                    result = ToModeChoiseDown();
                    break;
                case Mode.FreeDownComplete:
                    result = ToModeFreeDownComplete();
                    break;
                default:
                    FExcStr = "Не возможно перейти в указанный режим!";
                    return false;
            }
            if (result)
            {
                OnChangeMode(new ChangeModeEventArgs(FCurrentMode));
            }
            return result;
        } // ChoiseWork
        private bool ToModeWaiting()
        {
            ABaseMode tmpMM = new Waiting(this, null);  //Тут любой класс, просто чтото нужно в констуктор засунуть
            tmpMM.Const = Const;
            tmpMM.Employer = Employer;
            return Init(new Waiting(this, tmpMM));
        } // ToModeWaiting
    }
}
