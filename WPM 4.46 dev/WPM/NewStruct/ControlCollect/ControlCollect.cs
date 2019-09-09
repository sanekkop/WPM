using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class ControlCollect : ABaseMode
    {
        public DataTable GoodsCC;

        public ControlCollect(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.ControlCollect;
            //ControlCC = null;
            //LabelControlCC = null;
            GoodsCC = new DataTable();
            GoodsCC.Columns.Add("Number", Type.GetType("System.Int32"));
            GoodsCC.Columns.Add("NumberInDaemond", Type.GetType("System.Int32"));
            GoodsCC.Columns.Add("InvCode", Type.GetType("System.String"));
            GoodsCC.Columns.Add("Adress", Type.GetType("System.String"));
            GoodsCC.Columns.Add("Count", Type.GetType("System.Int32"));
            GoodsCC.Columns.Add("Descr", Type.GetType("System.String"));
        }
        internal override ABaseMode Init()
        {

            return this;
        }

    }
}
