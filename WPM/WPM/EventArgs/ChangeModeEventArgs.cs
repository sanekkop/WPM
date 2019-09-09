using System;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class ChangeModeEventArgs : ABaseModeEventArgs
    {
        public ChangeModeEventArgs(Mode ToMode)
        {
            FToMode = ToMode;
        } // ChangeModeEventArgs (constructor)
        public ChangeModeEventArgs(ABaseMode MM) : base (MM)
        {
            FToMode = MM.CurrentMode;
        }
        private Mode FToMode;
        /// <summary>
        /// What Mode you want man?
        /// </summary>
        public Mode ToMode { get { return FToMode; } }
    }
}
