using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class ChoiseWorkSupply : ABaseStandart
    {
        public ChoiseWorkSupply(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.ChoiseWorkSupply;
        } // ChoiseWorkSupply (constructor)
        protected override void ReactionKeyDo(Keys key)
        {
            if (key == Keys.D0 || key == Keys.Escape)
            {
                Cancel();
            }
            else if (key == Keys.D1 && Employer.CanLoad)
            {
                SS.OnReport(new ReportEventArgs("Задача выбрана..."));
                JumpTo(new LoaderChoise(SS, this));
            }
            else if (key == Keys.D2 && Employer.CanSupply)
            {
                SS.OnReport(new ReportEventArgs("Задача выбрана..."));
                JumpTo(new RefillChoise(SS, this));
            }
        } // ReactionKeyDo()
        internal override ABaseMode Init()
        {
            return Positive("Выберите режим работы");
        } // Init()
        internal override ABaseMode Cancel()
        {
            SS.OnReport(new ReportEventArgs("Отмена..."));
            return JumpTo(new ChoiseWork(SS, this));
        } // Cancel
    }
}
