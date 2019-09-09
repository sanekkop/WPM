using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace WPM
{
    class ReportEventArgs : ABaseModeEventArgs
    {
        public ReportEventArgs(string mess) : base()
        {
            FMessage = mess;
            FVoice = Voice.None;
            FForeColor = Color.Empty;
        }
        public ReportEventArgs(string mess, Voice voice) : base()
        {
            FMessage = mess;
            FVoice = voice;
            FForeColor = Color.Empty;
        }
        public ReportEventArgs(string mess, Voice voice, Color color) : base()
        {
            FMessage = mess;
            FVoice = voice;
            FForeColor = color;
        }
        public ReportEventArgs(ABaseMode MM) : base(MM)
        {
            if (MM.Result == MMResult.None)
            {
                FMessage = null;
                return;
            }
            FMessage    = MM.ExcStr;
        } // ReportEventArgs  (constructor 4)
        private string FMessage;

        /// <summary>
        /// What Mode you want man?
        /// </summary>
        public string Message { get { return FMessage; } }
        
        public bool EventActive { get { return FMessage == null ? false : true;  } }  //Событие активированно!
    }
}
