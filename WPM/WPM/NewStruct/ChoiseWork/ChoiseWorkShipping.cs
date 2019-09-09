using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class ChoiseWorkShipping : ABaseStandart
    {
        public ChoiseWorkShipping(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.ChoiseWorkShipping;
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
            else if (key == Keys.D1 && Employer.CanComplectation)
            {
                DesireMode = Mode.LoadingInicialization;
            }
            else if (key == Keys.D3 && Employer.CanDown)
            {
                DesireMode = Mode.ChoiseDown;
            }
            else if (key == Keys.D4 && Employer.CanComplectation)
            {
                DesireMode = Mode.FreeDownComplete;
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
