using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    partial class Model
    {
        //VARIABLES
        private bool RepeatCountBox;    //хуйня какая-то
        public bool BoxOk;             //Не понял что это
        public bool EmpbtyBox;      //Есть ли сборочный с пустой коробкой

        //BASE METHOD's
        private bool ToModeSetComplete()
        {
            EmpbtyBox = false;
            //Проверим нет ли сборочного с пустой коробкой, его в первую очередь будем закрывать
            string TextQuery =
               "SELECT " +
                   "journ.iddoc as IDDOC " +
               "FROM " +
                   "_1sjourn as journ (nolock) " +
                   "INNER JOIN DH$КонтрольНабора as DocCC (nolock) " +
                       "ON DocCC.iddoc = journ.iddoc " +
                   "WHERE " +
                       "DocCC.$КонтрольНабора.Наборщик = :Employer " +
                       "and journ.iddocdef = $КонтрольНабора " +
                       "and DocCC.$КонтрольНабора.Дата2 = :EmptyDate " +
                       "and not DocCC.$КонтрольНабора.Дата1 = :EmptyDate " +
                       "and DocCC.$КонтрольНабора.Коробка = :EmptyID " + 
                       "and journ.ismark = 0 ";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count > 0)
            {
                if (!ToModeSetCompleteAfrerBox(null))
                {
                    return false;
                }
                OnChangeMode(new ChangeModeEventArgs(Mode.SetComplete));
                return true;
            }
            BoxOk = false;
            DocSet.ID = null;   //Нет конкретного документа
            FExcStr = "Отсканируйте коробку!";
            OnChangeMode(new ChangeModeEventArgs(Mode.SetComplete));
            return true;
        }
        private bool ToModeSetCompleteAfrerBox(RefSection Adress)
        {
            EmpbtyBox = false;
            string TextQuery = "select iddoc from DH$КонтрольНабора where iddoc in (:Docs) and $КонтрольНабора.Коробка = :box";
            TextQuery = TextQuery.Replace(":Docs", Helper.ListToStringWithQuotes(DocsSet));
            if (Adress == null)
            {
                EmpbtyBox = true;
                QuerySetParam(ref TextQuery, "box", GetVoidID());
            }
            else
            {
                QuerySetParam(ref TextQuery, "box", Adress.ID);
            }
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
                
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "Сборочный с такой коробкой не найдет!";
                return false;
            }
            if (!LoadDocSet(DT.Rows[0]["iddoc"].ToString()))
            {
                return false;
            }

            RepeatCountBox = false;
            BoxOk = false;
            DocSet.Boxes = 0;
            TextQuery =
            "Select " +
                "count(*) as boxes " +
            "from $Спр.МестаПогрузки (nolock) " +
                "where $Спр.МестаПогрузки.КонтрольНабора = :iddoc";
            QuerySetParam(ref TextQuery, "iddoc", DocSet.ID);
            DT.Clear();
            if (ExecuteWithRead(TextQuery, out DT))
            {
                if (DT.Rows.Count > 0)
                {
                    DocSet.Boxes = (int)DT.Rows[0]["boxes"];
                }
            }

            TextQuery =
            "Select " +
                "ISNULL(RefSection.$Спр.Секции.МаксКорешков , 0) as MaxStub " +
            "from DH$КонтрольНабора as DocCC (nolock) " +
                "left join $Спр.Секции as RefSection (nolock) " +
                    " on DocCC.$КонтрольНабора.Сектор = RefSection.id " +
                "where DocCC.iddoc = :iddoc";
            QuerySetParam(ref TextQuery, "iddoc", DocSet.ID);
            DT.Clear();
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            DocSet.MaxStub = (int)(decimal)DT.Rows[0]["MaxStub"];

            BoxForSet = Adress;
            if (Employer.SelfControl)
            {
                TextQuery =
                    "select top 1 iddoc " +
                    "from DT$КонтрольНабора as DocT (nolock) " +
                    "where " +
                        "iddoc = :iddoc " +
                        "and DocT.$КонтрольНабора.Состояние0 = 2 " +
                        "and DocT.$КонтрольНабора.Контроль <= 0 " +
                        "and DocT.$КонтрольНабора.СостояниеКорр = 0 " +
                        "and DocT.$КонтрольНабора.Количество > 0 ";
                QuerySetParam(ref TextQuery, "iddoc", DocSet.ID);
                DT.Clear();
                if (!ExecuteWithRead(TextQuery, out DT))
                {
                    return false;
                }
                if (DT.Rows.Count > 0)
                {
                    //Включен самоконтроль и осталось что-то "поконтролить"
                    return ToModeSetSelfControl();
                }
            }
            return true;
        } // ToModeSetCompleteAfterBox
        private bool RSCSetComplete(string IDD)
        {
            if (IsSC(IDD, "Сотрудники"))
            {
                return ReactionCancel();
            }
            else if (IsSC(IDD, "Принтеры"))
            {
                if (!Printer.FoundIDD(IDD))
                {
                    return false;
                }
                return true;
            }
            else if (!IsSC(IDD, "Секции"))
            {
                FExcStr = "Нужна коробка и адрес предкомплектации, а не это!";
                return false;
            }
            if (Printer.Path == null)
            {
                FExcStr = "Не выбран принтер!";
                return false;
            }
            RefSection Adress = new RefSection(this);
            if (!Adress.FoundIDD(IDD))
            {
                return false;
            }
            if (DocSet.ID == null)
            {
                //Документа еще нет - значит ждем коробку
                if (Adress.Type != 12)
                {
                    FExcStr = "Для начала нужна коробка!";
                    return false;
                }
                //Это таки коробка
                return ToModeSetCompleteAfrerBox(Adress);
            }
            else if (!BoxOk && Adress.Type == 12)
            {
                //Документ уже есть, но может меняем его?
                return ToModeSetCompleteAfrerBox(Adress);
            }
            
            //Эти действия уже с адресом предкомплектации должны быть
            if (Adress.Type == 12)
            {
                FExcStr = "Отсканируйте адрес предкопмплектации!";
                return false;
            }
            if (!BoxOk)
            {
                FExcStr = "Подтвердите места по 'зеленой'!";
                return false;
            }
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДокументВход"] = ExtendID(DocSet.ID, "КонтрольНабора");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"] = ExtendID(Adress.ID, "Спр.Секции");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = DocSet.Boxes.ToString();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход2"] = Printer.Path;

            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("PicingComplete", DataMapWrite, FieldList, out DataMapRead))
            {
                return false;
            }
            if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] == -3)
            {
                FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
                return false;
            }
            if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] != 3)
            {
                FExcStr = "Не известный ответ робота... я озадачен...";
                return false;
            }
            FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
            QuitModesSet();
            return ToModeSetInicialization();
            //MM = new SetInicialization(this, tmpMM);
            //return MM.Init();
        } // RSCSetComplete

        //SPECIAL METHOD's
        public bool EnterCountBox(int Count)
        {
            if (DocSet.MaxStub <= Count)
            {
                if (DocSet.Boxes != Count)
                {
                    RepeatCountBox = false;
                }
                if (!RepeatCountBox)
                {
                    FExcStr = "Подтвердите ввод числа мест!";
                    DocSet.Boxes = Count;
                    RepeatCountBox = true;
                    BoxOk = false;
                    return false;
                }
            }
            DocSet.Boxes = Count;
            RepeatCountBox = true;
            BoxOk = true;
            FExcStr = "Отсканируйте адрес...";
            return true;
        } // EnterCountBox
    }
}
