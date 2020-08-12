using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Drawing;

namespace WPM
{
    partial class Model
    {
        public ABaseMode MM;
        public List<Mode> NewStructModes;

        private void FillNewStructList()
        {
            NewStructModes = new List<Mode>();
            NewStructModes.Add(Mode.ControlCollect);
            NewStructModes.Add(Mode.Waiting);
            NewStructModes.Add(Mode.ChoiseWork);
            NewStructModes.Add(Mode.ChoiseWorkSupply);
            NewStructModes.Add(Mode.ChoiseWorkShipping);
            NewStructModes.Add(Mode.ChoiseWorkSample);
            NewStructModes.Add(Mode.ChoiseWorkAddressCard);
            NewStructModes.Add(Mode.RefillChoise);
            NewStructModes.Add(Mode.RefillSet);
            NewStructModes.Add(Mode.RefillSetComplete);
            NewStructModes.Add(Mode.RefillLayout);
            NewStructModes.Add(Mode.RefillLayoutComplete);
            NewStructModes.Add(Mode.LoaderChoise);
            NewStructModes.Add(Mode.Loader);
            NewStructModes.Add(Mode.LoaderChoiseLift);
            NewStructModes.Add(Mode.RefillSetCorrect);
            NewStructModes.Add(Mode.SetTransfer);
            NewStructModes.Add(Mode.ItemCard);
            NewStructModes.Add(Mode.ChoiseWorkAcceptance);
            NewStructModes.Add(Mode.UnLoading);
                        
        } // FillNewStructList
        private void Reaction(string Barcode, Keys Key)
        {
            Mode OldMode = FCurrentMode;
            ABaseMode oldMM = MM;
            try
            {
                if (Barcode != null)
                {
                    MM = MM.ReactionBarcodeBase(Barcode);
                }
                else
                {
                    MM = MM.ReactionKeyBase(Key);
                }
            }
            catch (Exception e)
            {
                //Накрылось пиздой по связи. Будем ругаться и выходить,
                //в будущем нужно наверное переход в режим waiting замутить при таком раскладе, я хз или что-то еще в этом духе
                //MM = oldMM;
                try
                {
                    MyReader.Close();
                }
                catch (Exception eInside) { }
                Init(new Waiting(this, null));  //Эта функция, по логике упасть не может никогда и выкинет нас в режим Waiting
                OnChangeMode(new ChangeModeEventArgs(FCurrentMode));
                OnReport(new ReportEventArgs(e.Message, Voice.Bad, Color.Red));
                return;
            }
            FEmployer = MM.Employer;    //Временно
            Const = MM.Const;           //Временно, рефрешим константы
            if (oldMM.CurrentMode != MM.CurrentMode)
            {
                if (MM.Result != MMResult.Positive)
                {
                    //Что-то полшо не так в методе Init нового режима!
                    ReportEventArgs tmpREA = new ReportEventArgs(MM);   //Создадим с результирующим сфейлившим режимом сообщение, а потом заменим его
                    MM = oldMM;
                    OnReport(tmpREA);
                    return;
                }
                //Произошла смена режима, дальше идет говнокод, который обеспечивает совместимость режимов
                if (FCurrentMode != OldMode)
                {
                    //Код курильщика
                    //ТРЕШАК! РЕЖИМ ЭТОГО ОБЪЕКТА ПОМЕНЯЛСЯ ВНУТРИ НОВОЙ АРХИТИКТУРЫ
                    OnChangeMode(new ChangeModeEventArgs(FCurrentMode));
                }
                else
                {
                    //Код здорового человека
                    OnChangeMode(new ChangeModeEventArgs(MM));
                }
            }
            else if (MM.Result != MMResult.None)
            {
                //Если хоть что-то было, то сообщаем
                OnReport(new ReportEventArgs(MM));
            }
        } // Reaction
        public void BarcodeReaction(string Barcode)
        {
            OnReport(new ReportEventArgs("ШК принят, обработка..."));
            Reaction(Barcode, Keys.None);
        } // BarcodeReaction
        public void ReactionKey(Keys Key)
        {
            Reaction(null, Key);
        } // ReactionKey
        private bool Init(ABaseMode NewMode)
        {
            NewMode.Const = Const;
            NewMode = NewMode.Init();
            MMResult Result = NewMode.Result;
            if (Result == MMResult.Positive)
            {
                MM = NewMode;
            }
            FCurrentMode = MM.CurrentMode;
            Const = MM.Const;   //Временно, рефрешим константы
            FEmployer = MM.Employer;    //Временно
            return Result == MMResult.Positive ? true : false;
        } // Init
    }
}
