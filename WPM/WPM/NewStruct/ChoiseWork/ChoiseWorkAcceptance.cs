using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class ChoiseWorkAcceptance : ABaseStandart
    {
        public ChoiseWorkAcceptance(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.ChoiseWorkAcceptance;
        } // ChoiseWorkShipping (constructor)
        internal override ABaseMode Init()
        {
            return Positive("Выберите режим работы");
        } // Init()
        protected override void ReactionKeyDo(Keys key)
        {
            Mode DesireMode;
            if (key == Keys.Escape || key == Keys.D0)
            {
                Cancel();
                return;
            }
            else if (key == Keys.D1 && Employer.CanAcceptance)
            {
                DesireMode = Mode.Acceptance;
            }
            else if (key == Keys.D2 && Employer.CanAcceptance)
            {
                DesireMode = Mode.AcceptanceCross;
            }
            else
            {
                return;
            }
            SS.OnReport(new ReportEventArgs("Задача выбрана..."));
            if (!SS.ChoiseWork(DesireMode))
            {
                Negative(SS.ExcStr);
                return;
            }
            Positive();
        } // ReactionKey
        internal override ABaseMode Cancel()
        {
            SS.OnReport(new ReportEventArgs("Отмена..."));
            return JumpTo(new ChoiseWork(SS, this));
        } // Cancel
    }
}
