using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    /// <summary>
    /// Обеспечивает коннект, чтение/запись и т.п. 
    /// </summary>
    /// 
    class SQLSynchronizer
    {
        protected string FServerName;
        protected string FDBName;
        protected System.Data.SqlClient.SqlConnection MyConnection;
        protected System.Data.SqlClient.SqlCommand MyCommand;
        protected System.Data.SqlClient.SqlDataReader MyReader;
        protected string FExcStr;
        public bool permission;
        public delegate void OpenedEventHendler(object sender, EventArgs e);
        public event OpenedEventHendler Opened;

        /// <summary>
        /// Server name
        /// </summary>
        public string ServerName
        {
            get { return FServerName; }
            set { FServerName = value; }
        }
        /// <summary>
        /// Data base name
        /// </summary>
        public string DBName
        {
            get { return FDBName; }
            set { FDBName = value; }
        }
        /// <summary>
        /// Except string
        /// </summary>
        public string ExcStr
        {
            get { return FExcStr; }
            set { FExcStr = value; }    //УБЕРИ ОТСЮДА ЭТОТ ПИЗДЕЦ, ПОЖАЛУЙСТА!!!
        }
        /// <summary>
        /// Get connection string for SQLClient
        /// </summary>
        /// <returns>Connection string</returns>
        private string GetConnectStr()
        {
            return "Persist Security Info=False;Data Source=" + FServerName + ";User ID=sa;pwd=1419176;Initial Catalog=" + FDBName + ";";
        }
        /// <summary>
        /// Конструктор класса
        /// </summary>
        public SQLSynchronizer(string ServerName, string DBName)
        {
            FServerName  = ServerName;
            FDBName      = DBName;
            MyConnection = new System.Data.SqlClient.SqlConnection(GetConnectStr());
            MyCommand    = new System.Data.SqlClient.SqlCommand("", MyConnection);
            FExcStr = null;
            OpenConnection();
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void IsOpened()
        {
            permission = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnOpened(EventArgs e)
        {
            if (Opened != null)
            {
                Opened(this, e);
            }
        }
        /// <summary>
        /// Выполняет открытие соединения, или ничего не выполняет, если соединение уже открыто
        /// </summary>
        /// <returns></returns>
        private bool OpenConnection()
        {
            //FExcStr = null;
            //Проверим открыт ли коннект
            if (MyConnection.State != ConnectionState.Open)
            {
                try
                {
                    MyConnection.Close();
                    MyConnection.Open();
                }
                catch (Exception e)
                {
                    FExcStr = e.ToString();
                    return false;
                }
                OnOpened(new EventArgs());
                IsOpened();   //коннект открылся
            }
            return true;
        }

        /// <summary>
        /// Выполняет команду на чтение или запись
        /// </summary>
        /// <param name="Query"></param>
        /// <param name="Read"></param>
        /// <returns></returns>
        protected bool ExecuteQuery(string Query, bool Read)
        {
            //FExcStr = null;
            //Сохраним первоначальое состояние соединения
            ConnectionState CS = MyConnection.State;
            if (!OpenConnection())
            {
                return false;
            }
            if (!permission)
            {
                FExcStr = "Had reconnection, query execution forbidden!";
                return false;
            }
            try
            {
                MyCommand.CommandText = Query;
                if (Read)
                {
                    MyReader = MyCommand.ExecuteReader();
                }
                else
                {
                    MyCommand.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                if (CS == ConnectionState.Open && MyConnection.State != ConnectionState.Open)
                {
                    //Таким образом, если соединение до выполнения запроса было открыто,
                    //а после выполнения обвалилось (см. выше). Это наверняка эффект "уснувшего" терминала!
                    //От бесконечной рекурсии мы избавились так - был открыт, стал не открыт - повторяем, 
                    //при повторе CS полюбому - не открыт, значит дополнительный вызов не произойдет!
                    return ExecuteQuery(Query, Read);
                }
                FExcStr = e.ToString();
                return false;
            }
            return true;
        }
        /// <summary>
        /// Выполняет команду на чтение
        /// </summary>
        /// <param name="Query"></param>
        /// <returns></returns>
        protected bool ExecuteQuery(string Query)
        {
            return ExecuteQuery(Query, true);
        }
    }
}
