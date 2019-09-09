using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    /// <summary>
    /// Класс организующий доступ и синхронизацию с базой данных компании
    /// </summary>
    class SQL1S : SQLSynchronizer
    {
        private Dictionary<string, string> SynhMap;         //хеш-таблица, сопоставляет имена 1С с именами SQL
        private List<string> ExclusionFields;
        public int DebugHowLong;    //debug

        public SQL1S(string ServerName, string DBName) : base (ServerName, DBName)
        {
            SynhMap = new Dictionary<string, string>();
            //стандартные поля, не будем извафлятся типа "Наименование" или "Код", дабы не ломать пальцы переключая раскладку
            ExclusionFields = new List<string>();
            ExclusionFields.Add("ID");
            ExclusionFields.Add("DESCR");
            ExclusionFields.Add("CODE");
            ExclusionFields.Add("ISMARK");
            ExclusionFields.Add("DATE_TIME_IDDOC");
            ExclusionFields.Add("IDDOCDEF");
            ExclusionFields.Add("DOCNO");

            foreach(string curr in ExclusionFields)
            {
                SynhMap[curr] = curr;
            }
        }
        /// <summary>
        /// load Aliases. list of aliases inside this method
        /// </summary>
        /// <returns></returns>
        public bool LoadAliases()
        {
            //Начальная загрузка псевдонимов. Лениво делать список...
            //в принципе метод - нахуй не нужный.
            List<string> DefaultAlies = new List<string>();
            #region to chto srazu podsoset v kesh
            //Таблица синхронизации имен
            DefaultAlies.Add("Константа.ТоварДляЕдиниц");
            DefaultAlies.Add("Константа.ОснСклад");
            #endregion

            string result = "'";
            for (int i = 0; i < DefaultAlies.Count; i++)
            {
                result += DefaultAlies[i] + "','";
            }
            //удаляем последнюю запятую и ковычки "','"
            result = result.Substring(0, result.Length - 2);

            string TextQuery = 
                "select Name1C as Name1C, NameSQL as NameSQL from RT_Aliases (nolock) where Name1C in (" + result + ")";
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            DataRow[] DR = DT.Select();
            foreach(DataRow dr in DR)
            {
                SynhMap[dr["Name1C"].ToString().Trim()] = dr["NameSQL"].ToString().Trim();
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="AttributeList"></param>
        public void AddKnownAttributes(string parent, ref List<string> AttributeList)
        {
            foreach (KeyValuePair<string, string> pair in SynhMap)
            {
                if (pair.Key.Length >= parent.Length)
                {
                    if (pair.Key.Substring(0, parent.Length) == parent)
                    {
                        string part = pair.Key.Substring(parent.Length);
                        if (part.Length > 0)
                        {
                            AttributeList.Add(pair.Key);
                            //Вообще по хорошему надо так как ниже, но слишком много где засветится эта хуйня...
                            //AttributeList.Add(part);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Alias"></param>
        /// <returns></returns>
        public int DebugGetSyng(string Alias)
        {
            int Start = Environment.TickCount;
            
            //DataRow[] DR = DTAlias.Select("substring(Name1C, 1, 14) = 'Спр.Сотрудники'");

            int result = 0;
            

            DebugHowLong = Environment.TickCount - Start;
            //return DR.Length;
            return result;
        }
        /// <summary>
        /// Get sql name by 1C Alias
        /// </summary>
        /// <param name="Alias">1C name</param>
        /// <returns>SQL name</returns>
        public string GetSynh(string Alias)
        {
            if (SynhMap.ContainsKey(Alias))
            {
                return SynhMap[Alias];
            }
            string TextQuery = 
                "select top 1 NameSQL as NameSQL from RT_Aliases (nolock) where Name1C = :Alias";
            QuerySetParam(ref TextQuery, "Alias", Alias);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                throw new Exception("Cant connect for load this KEY " + Alias + "! Sheet!");
            }
            if (DT.Rows.Count == 0)
            {
                throw new Exception("Cant find this KEY " + Alias + "! Sheet!");
            }
            string result = DT.Rows[0]["NameSQL"].ToString().Trim();
            SynhMap[Alias] = result;    //add in dictionary
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextQuery"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public bool ExecuteWithRead(string TextQuery, out DataTable result)
        {
            result = new DataTable();
            if (!ExecuteQuery(QueryParser(TextQuery)))
            {
                return false;
            }
            for (int i = 0; i < MyReader.FieldCount; i++)
            {
                result.Columns.Add(MyReader.GetName(i), MyReader.GetFieldType(i));
            }
            
            while (MyReader.Read())
            {
                DataRow dr = result.NewRow();
                for (int col = 0; col < MyReader.FieldCount; col++)
                {
                    dr[col] = MyReader.GetValue(col);
                }
                result.Rows.Add(dr);
            }
            MyReader.Close();

            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextQuery"></param>
        /// <returns></returns>
        public bool ExecuteWithoutRead(string TextQuery)
        {
            return ExecuteQuery(QueryParser(TextQuery), false);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextQuery"></param>
        /// <param name="result"></param>
        public void ExecuteWithReadNew(string TextQuery, out DataTable result)
        {
            result = new DataTable();
            if (!ExecuteQuery(QueryParser(TextQuery)))
            {
                throw new TransportExcception(ExcStr);
            }
            for (int i = 0; i < MyReader.FieldCount; i++)
            {
                result.Columns.Add(MyReader.GetName(i), MyReader.GetFieldType(i));
            }

            while (MyReader.Read())
            {
                DataRow dr = result.NewRow();
                for (int col = 0; col < MyReader.FieldCount; col++)
                {
                    dr[col] = MyReader.GetValue(col);
                }
                result.Rows.Add(dr);
            }
            MyReader.Close();
        } // Обратная совместимость
        /// <summary>
        /// Возвращает просто конкретное значение без всякой таблицы
        /// </summary>
        /// <param name="TextQuery"></param>
        /// <returns></returns>
        public object ExecuteScalar(string TextQuery)
        {
            if (!ExecuteQuery(QueryParser(TextQuery)))
            {
                throw new TransportExcception(ExcStr);
            }
            object result = null;
            if (MyReader.Read())
            {
                result = MyReader.GetValue(0);
            }
            MyReader.Close();
            return result;
        } // ExecuteScalar
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextQuery"></param>
        public void ExecuteWithoutReadNew(string TextQuery)
        {
            if (!ExecuteQuery(QueryParser(TextQuery), false))
            {
                throw new TransportExcception(ExcStr);
            }
        } // Обратная совместимость
        /// <summary>
        /// Преобразует имя поля или таблицы SQL в имя 1С
        /// </summary>
        /// <param name="SQLName"></param>
        /// <returns></returns>
        protected string To1CName(string SQLName)
        {
            string result = "";
            foreach (KeyValuePair<string, string> pair in SynhMap)
            {
                if (pair.Value == SQLName)
                {
                    result = pair.Key;
                    return result;
                }
            }

            SQL1S tmpSS = new SQL1S(ServerName, DBName);
            //нихуя не найдено, подсосем из базы!
            string TextQuery = 
                "select top 1 Name1C as Name1C from RT_Aliases (nolock) where NameSQL = :SQLName";
            QuerySetParam(ref TextQuery, "SQLName", SQLName);
            DataTable DT;
            if (!tmpSS.ExecuteWithRead(TextQuery, out DT))
            {
                throw new Exception("Cant connect for load this SQL name! Sheet!");
            }
            if (DT.Rows.Count == 0)
            {
                throw new Exception("Cant find this SQL name! Sheet!");
            }
            tmpSS.MyConnection.Close();
            result = DT.Rows[0]["Name1C"].ToString().Trim();
            SynhMap[result] = SQLName;    //add in dictionary
            return result;
        }
        /// <summary>
        /// Преобразует дату из DateTime в формат в котором будем писать его в SQL, вот он: YYYY-DD-MM 05:20:00.000
        /// </summary>
        /// <param name="DateTime"></param>
        /// <returns></returns>
        static protected string DateTimeToSQL(DateTime DateTime)
        {
            //YYYYMMDD hh:mm:ss.nnn
            return DateTime.Year.ToString() +
                DateTime.Month.ToString().PadLeft(2, '0') +
                DateTime.Day.ToString().PadLeft(2, '0') + " 00:00:00.000";

            //старый код
            //return DateTime.Year.ToString() + "-" +
            //        DateTime.Day.ToString().PadLeft(2, '0') + "-" +
            //        DateTime.Month.ToString().PadLeft(2, '0') + " 00:00:00.000";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TextQuery"></param>
        /// <returns></returns>
        public string QueryParser(string TextQuery)
        {
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            QuerySetParam(ref TextQuery, "EmptyID", GetVoidID());
            QuerySetParam(ref TextQuery, "NowDate", DateTime.Now);
            QuerySetParam(ref TextQuery, "NowTime", APIManager.NowSecond());

            string result = TextQuery;
            int curI = result.IndexOf('$');
            while (curI != -1)
            {
                int endI = result.Substring(curI+1).IndexOf(' ');
                string part = result.Substring(curI+1, endI);
                result = result.Replace("$" + part + " ", GetSynh(part) + " ");
                curI = result.IndexOf('$');
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        static protected string ValueToQuery (object Value)
        {
            string result = "";
            if (Value.GetType() == Type.GetType("System.Int32")
                || Value.GetType() == Type.GetType("System.Decimal")
                || Value.GetType() == Type.GetType("System.Int16")
                || Value.GetType() == Type.GetType("System.Int64"))
            {
                result = Value.ToString();
            }
            else if (Value.GetType() == Type.GetType("System.DateTime"))
            {
                result = "'" + DateTimeToSQL((DateTime)Value) + "'";
            }
            else if (Value.GetType() == Type.GetType("System.String"))
            {
                result = "'" + Value + "'";
            }
            return result;
        }
        /// <summary>
        /// reserved words:
        ///     EmptyDate
        ///     EmptyID
        ///     NowDate
        ///     NowTime
        /// </summary>
        /// <param name="TextQuery"></param>
        /// <param name="NameParam"></param>
        /// <param name="Value"></param>
        static public void QuerySetParam(ref string TextQuery, string NameParam, object Value)
        {
            TextQuery = TextQuery.Replace(":"+NameParam, ValueToQuery(Value));
        }
        /// <summary>
        /// Преобразует текстовое представление дыты как она есть в полях DATE_TIME таблиц (они текстовые) в тип DateTime
        /// </summary>
        /// <param name="StrDateTime"></param>
        /// <returns></returns>
        static protected DateTime SQLToDateTime(string StrDateTime)
        {
            //Пока что без времени
            return Convert.ToDateTime(StrDateTime.Substring(0, 4) + "." +
                                      StrDateTime.Substring(4, 2) + "." +
                                      StrDateTime.Substring(6, 2));
        }
        /// <summary>
        /// Get extend ID, include ID and 4 symbols determining the type (in 36-dimension system)
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Type"></param>
        /// <returns>Extend ID (13 symbols)</returns>
        public string ExtendID(string ID, string Type)
        {
            if (GetSynh(Type).Substring(0, 2) == "SC")
            {
                return Translation.DecTo36(GetSynh(Type).Substring(2, GetSynh(Type).Length - 2)).PadLeft(4) + ID;
            }
            else
            {
                return Translation.DecTo36(GetSynh(Type)).PadLeft(4) + ID;
            }
        }
        public bool GetColumns(string table_name, out string columns, string SQLfunc)
        {
            string separator = ",";
            string tail = "";   //В конце что добавим
            columns = "";
            if (SQLfunc != null)
            {
                separator = ")," + SQLfunc + "(";
                columns = SQLfunc + "(";
                tail = ")";
            }
            string TextQuery =
                "declare @ColumnList varchar(1000); " +
                "select @ColumnList = COALESCE(@ColumnList + '" + separator + "', '') + column_name " +
                    "from INFORMATION_SCHEMA.Columns " + 
                "where table_name = :table_name; " +
                "select @ColumnList as ColumnList";
            SQL1S.QuerySetParam(ref TextQuery, "table_name", table_name + " "); //Пробел в конце, чтобы парсер нормально отработал
            DataTable DT;
            ExecuteWithReadNew(TextQuery, out DT);
            if (DT.Rows.Count == 0)
            {
                return false;
            }

            columns += DT.Rows[0]["ColumnList"].ToString();
            columns += tail;
            return true;
        } // GetColumns
        public bool GetCollumns(string table_name, out string columns, string SQLfunc)
        {
            return GetColumns(table_name, out columns, null);
        } // GetCollumns

        /// <summary>
        /// Возвращает пустой ID
        /// </summary>
        /// <returns></returns>
        static public string GetVoidID()
        {
            return "     0   ";
        }
        /// <summary>
        /// Check DateTime on concept 1C
        /// </summary>
        /// <param name="DateTime">Verifiable DateTime</param>
        /// <returns>true - if DateTime is void on concept 1C</returns>
        static public bool IsVoidDate(DateTime DateTime)
        {
            //Тут можно и по красивей написать...
            if (DateTime.Year == 1753)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        static public DateTime GetVoidDate()
        {
            return new DateTime(1753, 1, 1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="DateTime"></param>
        /// <returns></returns>
        static public string GetPeriodSQL(DateTime DateTime)
        {
            return "{d '" + DateTime.Year.ToString() + "-" + DateTime.Month.ToString().PadLeft(2, '0') + "-01'}";
        }
        /// <summary>
        /// Приводит передаваемый список строк в строку разделенную запятыми
        /// </summary>
        /// <param name="DataList"></param>
        /// <returns></returns>
        protected string ToFieldString(List<string> DataList)
        {
            string result = "";
            for (int i = 0; i < DataList.Count; i++)
            {
                result += GetSynh(DataList[i]) + ",";
            }
            //удаляем последнюю запятую
            if (result.Length > 0)
            {
                result = result.Substring(0, result.Length - 1);
            }
            return result;
        }


        /// <summary>
        /// возвращает список данных элемента справочника по его IDD или же ID (регулируется параметром ThisID)
        /// </summary>
        /// <param name="IDDorID"></param>
        /// <param name="SCType"></param>
        /// <param name="FieldList"></param>
        /// <param name="DataMap"></param>
        /// <param name="ThisID"></param>
        /// <returns></returns>
        public bool GetSCData(string IDDorID, string SCType, List<string> FieldList, out Dictionary<string, object> DataMap, bool ThisID)
        {
            SCType = "Спр." + SCType;
            DataMap = new Dictionary<string, object>();
            if (!ExecuteQuery("SELECT " + ToFieldString(FieldList) + " FROM " + GetSynh(SCType) + " (nolock)" +
                            " WHERE " + (ThisID ? "ID" : GetSynh(SCType + ".IDD")) + "='" + IDDorID + "'"))
            {
                return false;
            }
            if (MyReader.Read())
            {
                for (int i = 0; i < MyReader.FieldCount; i++)
                {
                    DataMap[FieldList[i]] = MyReader[i];
                }
                MyReader.Close();
                return true;
            }
            else
            {
                MyReader.Close();
                FExcStr = "Элемент справочника не найден!";
                return false;
            }
        }
        /// <summary>
        /// возвращает список данных элемента справочника по его IDD
        /// </summary>
        /// <param name="IDD"></param>
        /// <param name="SCType"></param>
        /// <param name="FieldList"></param>
        /// <param name="DataMap"></param>
        /// <returns></returns>
        public bool GetSCData(string IDD, string SCType, List<string> FieldList, out Dictionary<string, object> DataMap)
        {
            return GetSCData(IDD, SCType, FieldList, out DataMap, false);
        }
        public bool GetSCData(string IDD, string SCType, string ListStr, out Dictionary<string, object> DataMap, bool ThisID)
        {
            List<string> FieldList = Helper.StringToList(ListStr);
            for (int i = 0; i < FieldList.Count; i++)
            {
                string curr = FieldList[i];
                if (!ExclusionFields.Contains(curr))
                {
                    curr = "Спр." + SCType + "." + curr;
                    FieldList.RemoveAt(i);
                    FieldList.Insert(i, curr);
                }
            }
            return GetSCData(IDD, SCType, FieldList, out DataMap, ThisID);
        }
        public bool GetDoc(string IDDorID, out Dictionary<string, object> DataDoc, bool ThisID)
        {
            DataDoc = new Dictionary<string, object>();
            if (ThisID)
            {
                //Если ID - расширенный, то переведем его в обычный, 9-и символьный
                if (IDDorID.Length > 9)
                {
                    IDDorID = IDDorID.Substring(4);
                }
            }
            if (!ExecuteQuery("SELECT IDDOC, IDDOCDEF, DATE_TIME_IDDOC, DOCNO, ISMARK, " + GetSynh("IDD").ToString() +
                " FROM _1SJOURN (nolock) WHERE " + (ThisID ? "IDDOC" : GetSynh("IDD")) + "='" + IDDorID + "'"))
            {
                return false;
            }
            bool result = false;
            if (MyReader.Read())
            {
                DataDoc["ID"] = MyReader[0].ToString();
                DataDoc["IDDOCDEF"] = To1CName(MyReader[1].ToString());
                DataDoc["DATE_TIME_IDDOC"] = SQLToDateTime((MyReader[2].ToString()).Substring(0, 14));
                DataDoc["DOCNO"] = MyReader[3].ToString();
                DataDoc["ISMARK"] = MyReader[4];
                DataDoc["IDD"] = MyReader[5];
                result = true;
            }
            else
            {
                FExcStr = "Не найден документ!";
                result = false;
            }
            MyReader.Close();
            return result;
        } // GetDoc
        public bool GetDocData(string IDDoc, string DocType, List<string> FieldList, out Dictionary<string, object> DataMap)
        {
            bool result = false;
            DataMap = new Dictionary<string, object>();

            if (!ExecuteQuery("SELECT " + ToFieldString(FieldList) + " FROM DH" + GetSynh(DocType) + " (nolock) WHERE IDDOC='" + IDDoc + "'"))
            {
                return false;
            }
            if (MyReader.Read())
            {
                for (int i = 0; i < MyReader.FieldCount; i++)
                {
                    DataMap[FieldList[i]] = MyReader[i];
                }
                result = true;
            }
            MyReader.Close();
            return result;
        } // GetDocData

    }//class SQLSynhronizer
}
