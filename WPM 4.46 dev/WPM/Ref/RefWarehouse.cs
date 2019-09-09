using System;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class RefWarehouse : ARef
    {
        protected override string TypeObj { get { return "Склады"; } }

        public RefWarehouse(SQL1S ReadySS)
            : base(ReadySS)
        {
            HaveCode = true;
            HaveName = true;
        }
    }
}
