using System;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class UpdateEventArgs : EventArgs
    {
        private string FVers;
        /// <summary>
        /// Version on update
        /// </summary>
        public string Vers { get { return FVers; } }

        public UpdateEventArgs(string Vers)
        {
            FVers = Vers;
        } // Constructor
    }
}
