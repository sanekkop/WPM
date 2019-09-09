using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class ChoiseWorkAddressCard : ABaseStandart
    {
        public ChoiseWorkAddressCard(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.ChoiseWorkAddressCard;
        } // ChoiseWorkAddressCard (constructor)
        protected override void ReactionKeyDo(Keys key)
        {
            if (key == Keys.D0 || key == Keys.Escape)
            {
                Cancel();
            }           
        } // ReactionKeyDo()
        internal override ABaseMode Init()
        {
            return Positive("Товары в адресе");
        } // Init()
        internal override ABaseMode Cancel()
        {
            SS.OnReport(new ReportEventArgs("Отмена..."));
            return JumpTo(new ChoiseWork(SS, this));
        } // Cancel
    }
}
