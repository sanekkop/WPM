using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class LoaderUp : Loader
    {
        private int countScan;
        public LoaderUp(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
        } // LoaderDown
        internal override ABaseMode Init()
        {
            countScan = 0;
            CurrentAction = ActionSet.ScanPallete;
            //Заглушка, обратная совместимость
            if (Task.PalleteBarcode.Trim().Length == 0)
            {
                CurrentAction = ActionSet.ScanAdress;
                return Positive("Отсканируйте адрес дважды!");
            }
            //Конец обратной совместимости
            return Positive(SS.WhatUNeed(CurrentAction));
        } // Init()
        protected override void ReactionSCSectionDo(RefSection e)
        {
            if (CurrentAction != ActionSet.ScanAdress)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            if (e.ID != Task.Adress0.ID)
            {
                Negative("Не верный адрес!");
                return;
            }
            if (Task.PalleteBarcode.Trim().Length == 0 && countScan < 1)
            {
                countScan++;
                Positive("Ok! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            Adress1 = e;
            TaskEnd();
        } // ReactionSCSectionDo
        protected override void ReactionPalleteDo(string barcode)
        {
            if (Task.PalleteBarcode.Trim().Length == 0)
            {
                CurrentAction = ActionSet.ScanAdress;
                Negative("Паллета не оклеина! Сканируйте адрес дважды!");
                return;
            }
            if (CurrentAction != ActionSet.ScanPallete)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            if (Task.PalleteBarcode != barcode)
            {
                Negative("Неверная паллета! Где вы это взяли?");
                return;
            }
            CurrentAction = ActionSet.ScanAdress;
            PalleteBarcode = barcode;
            Positive(SS.WhatUNeed(CurrentAction));
        } // ReactionPalleteDo
        protected override void GoNextTask()
        {
            GoUpTask();
        } // GoNextTask
        
    }
}
