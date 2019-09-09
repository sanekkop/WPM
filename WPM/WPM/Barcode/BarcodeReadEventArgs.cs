using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class BarcodeReadEventArgs
    {
        private string FBarcode;
        public string Barcode
        {
            get { return FBarcode; }
        }
        public BarcodeReadEventArgs (string Barcode)
        {
            FBarcode = Barcode;
        }
    }
}
