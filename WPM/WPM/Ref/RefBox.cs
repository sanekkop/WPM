using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class RefBox : ARef
    {
        protected override string TypeObj { get { return "МестаПогрузки"; } }

        public RefBox(SQL1S ReadySS)
            : base(ReadySS)
        {
            HaveCode = false;
            HaveName = false;
        }
    }
}
