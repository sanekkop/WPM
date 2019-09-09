using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class ChoiseWorkSample : ABaseStandart
    {
        public ChoiseWorkSample(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.ChoiseWorkSample;
        } // ChoiseWorkSample (constructor)
        protected override void ReactionKeyDo(Keys key)
        {
            Mode DesireMode;
            if (key == Keys.D0 || key == Keys.Escape)
            {
                Cancel();
                return;
            }
            else if (key == Keys.D1 && Employer.CanGiveSample)
            {
                DesireMode = Mode.SampleSet;    // режим отбора образцов
            }
            else if (key == Keys.D2 && Employer.CanLayOutSample)
            {
                DesireMode = Mode.SamplePut;    // режим выкладки образцов
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
