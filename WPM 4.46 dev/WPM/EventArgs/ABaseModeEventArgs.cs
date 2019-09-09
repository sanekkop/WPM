using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace WPM
{
    abstract class ABaseModeEventArgs : EventArgs
    {
        public ABaseModeEventArgs() : base()
        {
            FForeColor = Color.Black;
            FVoice = Voice.None;
        } // ABaseModeEventArgs (default constructor
        public ABaseModeEventArgs(ABaseMode MM) : base()
        {
            if (MM.Result == MMResult.Positive)
            {
                FForeColor = Color.Blue;
                FVoice = MM.VoiceOn ? Voice.Good : Voice.None;
            }
            else
            {
                FForeColor = Color.Red;
                FVoice = Voice.Bad;
            }
        } // ABaseModeEventArgs (constructor)

        protected Voice FVoice;
        protected Color FForeColor;
        public Voice Voice { get { return FVoice; } }
        public Color ForeColor { get { return FForeColor; } }
    }
}
