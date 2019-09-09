using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class Set : ABaseMode
    {
        public Set(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.Set;
            ReactionBarcode += new ReactionBarcodeEventHandler(Set_ReactionBarcode);
        }

        void Set_ReactionBarcode(object sender, string Barcode)
        {
            throw new NotImplementedException();
        }
        internal override ABaseMode Init()
        {
            return Negative("No responses (Init)!");
        }
    }
}
