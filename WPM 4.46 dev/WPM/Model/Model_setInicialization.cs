using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    partial class Model
    {
        //VARIABLES
        public StrictDoc DocSet;
        public RefSection BoxForSet;
        public List<string> DocsSet;

        private bool ToModeSetInicializationRequest()
        {
            BoxForSet = null;
            FCurrentMode = Mode.SetInicialization;
            return true;
        }
        //BASE METHOD's
        private bool MBGoToSetSelfControl()
        {
            return true;
            string TextQuery =
                "SELECT " +
                    "DocCC.iddoc as IDDOC " +
                "FROM " +
                    "$Спр.ПараметрыСотрудников as Param (nolock) " +
                    "INNER JOIN DH$КонтрольНабора as DocCC (nolock) " +
                        "ON substring(Param.$Спр.ПараметрыСотрудников.ПоследнийДок.iddoc, 5, 9) = DocCC.iddoc " +
                    "INNER JOIN _1sjourn as journ (nolock) " +
                        "ON DocCC.iddoc = journ.iddoc " +
                    "WHERE " +
                        "parentext = :Employers " +
                        "and DocCC.$КонтрольНабора.Дата3 = :EmptyDate " +
                        "and DocCC.$КонтрольНабора.Дата4 = :EmptyDate " +
                        "and DocCC.$КонтрольНабора.ФлагСамовывоза = 1 " +
                        "and journ.ismark = 0 " +
                        "DocCC.$КонтрольНабора.Наборщик = :Employer ";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count > 0)
            {
                //return ToModeSetSelfComplete(DT.Rows[0]["IDDOC"].ToString());
                //debug 
                FExcStr = "NOT WRITE!!!";
                return false;
            }
        }
        public bool ToModeSetInicialization()
        {
            FEmployer.Refresh();    //Обновим данные сотрудника
            Const.Refresh();        //Обновим константы

            PreviousAction = "";
            DocsSet = new List<string>();

            if (!MBGoToSetSelfControl())
            {
                return false;
            }

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
                        "and journ.ismark = 0 ";
            QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            QuerySetParam(ref TextQuery, "EmptyDate", GetVoidDate());
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }

            if (DT.Rows.Count > 0)
            {
                DataRow[] DR = DT.Select();
                foreach(DataRow dr in DR)
                {
                    DocsSet.Add(dr["IDDOC"].ToString());
                }
                return ToModeSet(null, null);
            }

            BoxForSet = null;
            FCurrentMode = Mode.SetInicialization;
            return true;
        } // ToModeSetInicialization
        private bool RSCSetInicialization(string IDD)
        {
            if (IsSC(IDD, "Сотрудники"))
            {
                return ReactionCancel();
            }
            else if (!IsSC(IDD, "Секции") || !Const.BoxSetOn)
            {
                FExcStr = "Нет действий с данным штрихкодом!";
                return false;
            }
            BoxForSet = new RefSection(this);
            BoxForSet.FoundIDD(IDD);
            if (BoxForSet.Type != 12)
            {
                FExcStr = "Неверный тип адреса! Отсканируйте коробку!";
                return false;
            }
            OnChangeMode(new ChangeModeEventArgs(Mode.SetInicialization));  //Чтобы вызвать обновление экранной формы
            return true;
        }
        private bool RDSetInicialization(string IDD)
        {
            string DocType;
            string ID;
            if (!GetDoc(IDD, out ID, out DocType))
            {
                return false;
            }
            if (DocType != "КонтрольНабора")
            {
                FExcStr = "Нет действий с таким видом документа!";
                return false;
            }

            ABaseMode tmpMM = new Waiting(this, null);  //Тут любой класс, просто чтото нужно в констуктор засунуть
            tmpMM.Const = Const;
            tmpMM.Employer = Employer;
            bool result = Init(new SetTransfer(this, tmpMM, IDD));
            if (!result)
            {
                FCurrentMode = Mode.SetInicialization;
                FExcStr = "Нет действий с этим документом! (возможно скомплектован уже?)";
                OnReport(new ReportEventArgs("Нет действий с этим документом! (возможно скомплектован уже?)"));
            }
            return result;
        } // RDSetInicialization

        //SPECIAL METHOD's
        public bool CompleteSetInicialization()
        {
            if (BoxForSet == null && Const.BoxSetOn)
            {
                FExcStr = "Не указана коробка!";
                return false;
            }
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = ExtendID(Employer.ID, "Спр.Сотрудники");
            if (BoxForSet != null)
            {
                DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"] = ExtendID(BoxForSet.ID, "Спр.Секции");
            }

            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!ExecCommand("QuestPicing", DataMapWrite, FieldList, out DataMapRead))
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

            return ToModeSetInicialization();
            //MM = new SetInicialization(this, tmpMM);
            //return MM.Init();
        } // CompleteSetInicialization
    }
}
