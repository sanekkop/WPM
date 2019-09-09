using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    class Doc
    {
        protected SQL1S SS;
        private int FRowCount;
        private string FID;
        private Dictionary<string, object> HeaderAttributes;  //Атрибуты ШАПКИ, известные на данный момент.
        private Dictionary<string, object> CommonAttributes;  //Атрибуты общие, из таблицы _1sjourn
        private bool FModified;

        public string ID { get { return FID; } }
        public string IDD { get { return CommonAttributes["IDD"].ToString(); } }
        public string TypeDoc { get { return CommonAttributes["IDDOCDEF"].ToString(); } }
        public bool IsMark { get { return (bool)CommonAttributes["ISMARK"]; } }
        public DateTime DateDoc { get { return (DateTime)CommonAttributes["DATE_TIME_IDDOC"]; } }
        public string NumberDoc { get { return CommonAttributes["DOCNO"].ToString(); } }
        public bool Selected { get { return FID == null ? false : true; } }
        public string View { get { return CommonAttributes["IDDOCDEF"].ToString() + " " + CommonAttributes["DOCNO"].ToString() + " (" + ((DateTime)CommonAttributes["DATE_TIME_IDDOC"]).ToString("dd.MM.yy") + ")"; } }
        public int RowCount
        {
            get
            {
                CheckSelect();
                string TextQuery =
                    "select count(*) row_count " +
                    "from DT$" + CommonAttributes["IDDOCDEF"].ToString() + " (nolock) " +
                    "where iddoc = :iddoc ";
                SQL1S.QuerySetParam(ref TextQuery, "iddoc", ID);
                DataTable DT;
                if (!SS.ExecuteWithRead(TextQuery, out DT)) { return FRowCount; }   //не срослось что-то, вернем сохраненное значение
                FRowCount = (int)DT.Rows[0]["row_count"];
                return FRowCount;
            }
        } // RowCount
        public bool Modified { get { return FModified; } }

        /// <summary>
        /// return ready object or generate exception
        /// </summary>
        /// <param name="ID">iddoc</param>
        /// <param name="SS"> Ready SQL connect object</param>
        /// <returns></returns>
        static public Doc GiveDocById(string ID, SQL1S SS)
        {
            Doc result = new Doc(SS);
            if (!result.FoundID(ID))
            {
                throw new NullReferenceException("Doc with iddoc = '" + ID + "' - not find!!!");
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReadySS">prepared for use object</param>
        public Doc(SQL1S ReadySS)
        {
            SS = ReadySS;
            HeaderAttributes = new Dictionary<string, object>();
            FRowCount = 0;
        } // Doc (constrictor
        /// <summary>
        /// 
        /// </summary>
        private void CheckSelect()
        {
            if (!Selected)
            {
                throw new NullReferenceException("Document not selected");
            }
        } // CheckSelect
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IDDorID"></param>
        /// <param name="ThisID"></param>
        /// <returns></returns>
        private bool FoundIDDorID(string IDDorID, bool ThisID)
        {
            if (!SS.GetDoc(IDDorID, out CommonAttributes, ThisID))
            {
                return false;
            }
            FID = CommonAttributes["ID"].ToString();
            FModified = false;
            return true;
        } // FoundIDDorID
        /// <summary>
        /// 
        /// </summary>
        /// <param name="IDD"></param>
        /// <returns></returns>
        public bool FoundIDD(string IDD)
        {
            HeaderAttributes.Clear();   //при перепозиционировании все очищается
            return FoundIDDorID(IDD, false);
        } // FoundIDD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        public bool FoundID(string ID)
        {
            HeaderAttributes.Clear();   //при перепозиционировании все очищается
            return FoundIDDorID(ID, true);
        } // FoundID
        /// <summary>
        /// Если указанный атрибут будет неверно задан или это будет атрибут табличной части, то все наебнется
        /// </summary>
        /// <param name="Name">name header attribute</param>
        /// <returns>value</returns>
        public object GetAttributeHeader(string Name)
        {
            CheckSelect();
            if (HeaderAttributes.ContainsKey(Name))
            {
                return HeaderAttributes[Name];
            }
            HeaderAttributes[TypeDoc + "." + Name] = null;
            Refresh();
            return HeaderAttributes[TypeDoc + "." + Name];
        } // GetAttributeHeader
        /// <summary>
        /// modifies attribute value
        /// </summary>
        /// <param name="Name">attribute name</param>
        /// <param name="value">attribute value</param>
        public void SetAttributeHeader(string Name, object value)
        {
            CheckSelect();
            HeaderAttributes[TypeDoc + "." + Name] = value;
            FModified = true;
        } // SetAttributeHeader
        /// <summary>
        /// reload data from database, all modified attributes is refill
        /// </summary>
        public void Refresh()
        {
            CheckSelect();
            FModified = false;
            //ПОДСОСЕМ ИЗ ЖУРНАЛА
            FoundIDDorID(ID, true);

            //ПОДСОСЕМ ИЗ ШАПКИ
            //формируем список атрибутов шапки
            if (HeaderAttributes.Count > 0)
            {
                List<string> FieldList = new List<string>();
                foreach (KeyValuePair<string, object> pair in HeaderAttributes)
                {
                    FieldList.Add(pair.Key);
                }
                if (!SS.GetDocData(ID, TypeDoc, FieldList, out HeaderAttributes))
                {
                    return; //не срослось
                }
            }
        } // Refresh
        /// <summary>
        /// save document header date in database
        /// </summary>
        /// <returns></returns>
        public bool Save()
        {
            CheckSelect();
            if (!Modified)
            {
                return true;    //документ не был изменен
            }
            string TextQuery =
                "update DH$" + TypeDoc + " set "; 
            foreach (KeyValuePair<string, object> pair in HeaderAttributes)
            {
                TextQuery += "$" + pair.Key + " = :param, ";
                SQL1S.QuerySetParam(ref TextQuery, "param", pair.Value);
            }
            //режим последнюю запятую (с пробелом)
            TextQuery = TextQuery.Substring(0, TextQuery.Length - 2);
            TextQuery += " where iddoc = :iddoc";
            SQL1S.QuerySetParam(ref TextQuery, "iddoc", ID);
            return SS.ExecuteWithoutRead(TextQuery);
        } // Save
    }
}
