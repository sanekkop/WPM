using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class SetInicialization : ABaseStandart
    {
        public StrictDoc DocSet;        //Документ набора (Контроль набора в основном)

        public SetInicialization(Model tmpSS, ABaseMode CallObject)
            : base(tmpSS, CallObject)
        {
            CurrentMode = Mode.SetInicialization;
        } // SetInicialization (constructor)

        override internal ABaseMode Init()
        {
            return Negative("not complete!");
        }
    }
}
