using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class SetTransfer : ABaseStandart
    {
        public Doc DocCC; //Контроль набора с которым имеем дело
        public Doc Proposal; //Заявка по листу набора
        private Doc DocCB;   //Контроль расходной
        public RefSection TransferWindow;   //Окно выдачи
        public RefSection SectorCC;         //Сектор сборочного
        public RefEmployer Setter;      //Наборщик
        public RefSection CompleteAdress;   //Адрес комплектации

        private void Constructor()
        {
            Proposal = new Doc(SS);
            TransferWindow = new RefSection(SS);
            SectorCC = new RefSection(SS);
            Setter = new RefEmployer(SS);
            DocCB = new Doc(SS);
            CompleteAdress = new RefSection(SS);
            DocCB.FoundID(DocCC.GetAttributeHeader("ДокументОснование").ToString());
            Proposal.FoundID(DocCB.GetAttributeHeader("ДокументОснование").ToString());
            SectorCC.FoundID(DocCC.GetAttributeHeader("Сектор").ToString());
            TransferWindow.FoundID(DocCB.GetAttributeHeader("АдресОбразцов").ToString());
            CompleteAdress.FoundID(DocCB.GetAttributeHeader("АдресКомплектации").ToString());
            Setter.FoundID(DocCC.GetAttributeHeader("Наборщик").ToString());
            CurrentMode = Mode.SetTransfer;
            FVoiceOn = true;
        }
        public SetTransfer(Model ReadySS, ABaseMode CallObj, Doc doc)
            : base(ReadySS, CallObj)
        {
            DocCC = doc;
            Constructor();
        } // SetTransfer (constructor)
        public SetTransfer(Model ReadySS, ABaseMode CallObj, string IDD)
            : base(ReadySS, CallObj)
        {
            DocCC = new Doc(SS);
            DocCC.FoundIDD(IDD);
            Constructor();
        } // SetTransfer (constructor)
        private void TransferComplete()
        {
            //Любая комплектация фиксируется
            DocCC.SetAttributeHeader("Дата3", DateTime.Now);
            DocCC.SetAttributeHeader("Время3", APIManager.NowSecond());
            DocCC.SetAttributeHeader("Комплектовщик", Employer.ID);
            DocCC.Save();

            SS.Employer = Employer;
            SS.LockoutDoc(DocCC.ID);
            SS.OnChangeMode(new ChangeModeEventArgs(Mode.SetInicialization));
            Positive();
        } // TransferComplete
        private void SendToTablo(int flag)
        {
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"] = SS.ExtendID(DocCC.ID, "КонтрольНабора");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = flag;
            SS.ExecCommandNoFeedback("SendToTablo", DataMapWrite);
        }
        protected override void ReactionSCSectionDo(RefSection e)
        {
            if (TransferWindow.Selected)
            {
                Negative("Отдайте товар клиенту! Просканировав заявку!");
                return;
            }

            if (!CompleteAdress.Selected)
            {
                //Это первая комплектация!
                if (e.Type != 10)
                {
                    Negative("В такой тип адреса нельзя комплектовать самовывоз!");
                    return;
                }
                DocCB.SetAttributeHeader("АдресКомплектации", e.ID);
                DocCB.Save();
            }
            TransferComplete();
        }
        internal override ABaseMode Cancel()
        {
            if (TransferWindow.Selected)
            {
                //На всякий случай не будем отменять то, что не ставили, а то вдруг табло сломается
                SendToTablo(2); //Отменяем табло!
            }
            //DocCC.SetAttributeHeader("Дата4", SQL1S.GetVoidDate());
            //DocCC.SetAttributeHeader("Время4", 0);
            //DocCC.SetAttributeHeader("Комплектовщик2", SQL1S.GetVoidID());
            //DocCC.Save();

            SS.Employer = Employer; //ОБРАТНАЯ СОВМЕСТИМОСТЬ
            SS.LockoutDoc(DocCC.ID);
            SS.OnChangeMode(new ChangeModeEventArgs(Mode.SetInicialization));   //Костылина, обратная совместимость
            return Positive();
        } // Cancel
        protected override void ReactionDocDo(Doc doc)
        {
            if (doc.TypeDoc != "Счет")
            {
                Negative("Нет действий с этим документом!");
                return;
            }
            if (!TransferWindow.Selected)
            {
                Negative("Скомплектуйте товар на паллету! Окно выдачи не задано!");
                return;
            }
            if (doc.ID != Proposal.ID)
            {
                Negative("Не та заявка!");
                return;
            }

            //Фильтры пройдены. Это нужная заявка, мы отдали ее клиенту! Стираем табло и фиксируем комплектацию
            SendToTablo(2); //Отменяем табло!
            TransferComplete();
        } // ReactionDocDo
        internal override ABaseMode Init()
        {
            if (!SQL1S.IsVoidDate((DateTime)DocCC.GetAttributeHeader("Дата3")))
            {
                return Negative("Сборочный уже скомплектован!");
            }
            if (SQL1S.IsVoidDate((DateTime)DocCC.GetAttributeHeader("Дата2")))
            {
                return Negative("Сборочный не собран!");
            }
            if (!SS.LockDoc(DocCC.ID))
            {
                return Negative(SS.ExcStr);
            }
            if (SQL1S.IsVoidDate((DateTime)DocCC.GetAttributeHeader("Дата4")) && TransferWindow.Selected)
            {
                //Если предкомплектация еще не задана, а окно выдачи уже задано, то делаем предкомплектацию
                DocCC.SetAttributeHeader("Дата4", DateTime.Now);
                DocCC.SetAttributeHeader("Время4", APIManager.NowSecond());
                DocCC.SetAttributeHeader("Комплектовщик2", Employer.ID);
                DocCC.Save();
                //Если окно задано - то даем команду выводить на табло
                SendToTablo(1);
            }

            return Positive();
        } // ReactionSCEmployersDo
    }
}
