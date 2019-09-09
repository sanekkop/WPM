using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace WPM
{
    //ПОКА НЕ ИСПОЛЬЗОВАТЬ! НЕ ПРИДУМАЛ КАК РЕШИТЬ МОЗГОДРОЧКУ С ПОДПИСКАМИ!!!
    //Класс который умеет отвечать на ескейп выходом в Cancel, а на сканирование бейджика выходом в Waiting
    abstract class ABaseStandart : ABaseMode
    {
        public ABaseStandart(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.None;
        } // ABaseStandart (constructor)
        protected override void ReactionSCEmployersDo(RefEmployer employer)
        {
            JumpTo(new Waiting(SS, this));
        } // ReactionSCEmployersDo
        protected override void ReactionKeyDo(Keys key)
        {
            if (key == Keys.Escape)
            {
                Cancel();
                return;
            }
        } // ReactionKeyDo
        internal override ABaseMode Cancel()
        {
            SS.OnReport(new ReportEventArgs("Выход..."));
            return JumpTo(new ChoiseWork(SS, this));
        } // Cancel
    }
}
