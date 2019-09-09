using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    class RefGates : ARef
    {
        protected override string TypeObj { get { return "Ворота"; } }

        public RefSection FirstAdress
        {
            get
            {
                if (!Selected)
                {
                    return new RefSection(SS);
                }
                string TextQuery = "select dbo.fn_GetFirstAdressZone(:zona) as id";
                SQL1S.QuerySetParam(ref TextQuery, "zona", ID);
                DataTable DT;
                //Тут следует использовать ExecuteWithReadNew, но так как мы боимся что данная хуйня используется в старой архитектуре,
                //  то это может наебнууть всю программу. Поэтому пока так. Когда перепишем все - перепишем и тут.
                if (!SS.ExecuteWithRead(TextQuery, out DT)) { return new RefSection(SS); }   //не срослось что-то, вернем пустую хуйню

                if (DT.Rows.Count == 0)
                {
                    //Нет строк, ну и что, бывает. Значит нет ни одного адреса с этой зоной
                    return new RefSection(SS);
                }
                RefSection val = new RefSection(SS);
                val.FoundID(DT.Rows[0]["id"].ToString());
                return val;
            }
        } // FirstAdress
        public RefSection LastAdress
        {
            get
            {
                if (!Selected)
                {
                    return new RefSection(SS);
                }
                string TextQuery = "select dbo.fn_GetLastAdressZone(:zona) as id";
                SQL1S.QuerySetParam(ref TextQuery, "zona", ID);
                DataTable DT;

                //SS.ExecuteWithReadNew(TextQuery, out DT);
                if (!SS.ExecuteWithRead(TextQuery, out DT)) { return new RefSection(SS); }   //не срослось что-то, вернем пустую хуйню
                
                if (DT.Rows.Count == 0)
                {
                    //Нет строк, ну и что, бывает. Значит нет ни одного адреса с этой зоной
                    return new RefSection(SS);
                }
                RefSection val = new RefSection(SS);
                val.FoundID(DT.Rows[0]["id"].ToString());
                return val;
            }
        } // LastAdress
        public DataTable Ranges
        {
            get
            {
                DataTable Result = new DataTable();

                if (!Selected)
                {
                    throw new NullReferenceException("Element not selected!");
                }

                Result.Columns.Add("First", Type.GetType("System.String"));
                Result.Columns.Add("Last", Type.GetType("System.String"));

                string TextQuery =
                    "select " +
                        "ref.descr name, " +
                        // это оригинал, при котором зона выводится списком её кусков "(select top 1 $Спр.Секции.ЗонаАдресов from $Спр.Секции (nolock) where descr > ref.descr and isfolder = 2 and ismark = 0 order by descr) nextZone " +
                        "(select top 1 $Спр.Секции.ЗонаАдресов from $Спр.Секции as ref2 (nolock) where ref2.descr > ref.descr and ref2.isfolder = 2 and ref2.ismark = 0 and ref2.$Спр.Секции.ЗонаАдресов = :zone  order by descr) nextZone " +  //это изменённая строка при которой выводятся только начало и конец зоны
                    "from $Спр.Секции as ref (nolock) " +
                    "where " +
                        "ref.ismark = 0 " +
                        "and ref.$Спр.Секции.ЗонаАдресов = :zone " + 
                    "order by ref.descr";
                DataTable DT;
                SQL1S.QuerySetParam(ref TextQuery, "zone", ID);
                SS.ExecuteWithRead(TextQuery, out DT);

                bool inside = false;
                DataRow newRow = Result.NewRow();
                for (int i = 0; i < DT.Rows.Count; i++)
                {
                    if (!inside)
                    {
                        //начало диапазона
                        newRow["First"] = DT.Rows[i]["name"].ToString();
                        inside = true;
                    }

                    string nextZonde = DT.Rows[i]["nextZone"].ToString();
                    if (nextZonde != ID)
                    {
                        //Конец диапазона
                        newRow["Last"] = DT.Rows[i]["name"].ToString();
                        Result.Rows.Add(newRow);
                        newRow = Result.NewRow();
                        inside = false;
                    }
                }
                return Result;
            } // Get
        }   // Ranges

        /// <summary>
        ///  constructor
        /// </summary>
        /// <param name="ReadySS"></param>
        public RefGates(SQL1S ReadySS)
            : base(ReadySS)
        {
            //TypeObj = "Ворота";
            HaveCode = true;
            HaveName = true;
        } // RefGates (constructor)
    }
}
