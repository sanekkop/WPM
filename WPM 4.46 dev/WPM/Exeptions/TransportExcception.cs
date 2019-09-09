using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class TransportExcception : Exception
    {
        public TransportExcception(string mess)
            : base(mess)
        {
        }
    }
}
