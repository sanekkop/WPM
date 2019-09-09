using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class ReactionSCEventArgs : EventArgs
    {
        private string FBarcode;
        private ARef FRef;
        public string Barcode
        {
            get { return FBarcode; }
        }
        public ARef Ref
        {
            get { return FRef; }
        }
        public ReactionSCEventArgs(string Barcode, ARef Ref)
        {
            FBarcode = Barcode;
            FRef = Ref;
        }
    }
}
