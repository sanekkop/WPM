using System;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class ResponseEventArgs : EventArgs
    {
        private string FText;
        /// <summary>
        /// Answer text
        /// </summary>
        public string Text
        {
            get { return FText; }
        }

        public ResponseEventArgs(string Text)
        {
            FText = Text;
        }
    }
}
