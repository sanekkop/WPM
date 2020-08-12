using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class AcceptanceCross : ABaseStandart
    {

        public RefSection Adress;
        
        public AcceptanceCross(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.AcceptanceCross;
            Adress = new RefSection(SS);            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal override ABaseMode Init()
        {
            SS.OnReport(new ReportEventArgs("Получаю задание..."));
            string TextQuery = "select * from dbo.WPM_fn_GetTaskAccemtanceCross()";
            if (!SS.ExecuteWithRead(TextQuery, out SS.AcceptedCross))
            {
                Negative("Не удалось получить задание");
                return JumpTo(new ChoiseWorkAcceptance(SS, this));
            }
            if (SS.AcceptedCross.Rows.Count == 0)
            {
                Negative("Нет заданий");
                return JumpTo(new ChoiseWorkAcceptance(SS, this));
            }
            return Positive();
        }   // Init
        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        protected override void ReactionKeyDo(Keys key)
        {
            if (Helper.IsGreenKey(key) || key == Keys.Enter)
            {
                EnterCount();
            }
            else if (key == Keys.Escape)
            {
                SS.OnReport(new ReportEventArgs("Задача выбрана..."));
                JumpTo(new ChoiseWorkAcceptance(SS, this));
                return;
            }
            else if (key == Keys.Space)
            {
                CompleteLine();
            }

        } // ReactionKeyDo
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void ReactionSCSectionDo(RefSection e)
        {
            //Проверки зон!!!
            if (!e.AdressZone.Selected)
            {
                Negative("Нельзя! Адбрес без зоны!");
                return;
            }
            DataTable DT;
            
            string TextQuery = "SELECT VALUE as val FROM _1sconst (nolock) WHERE ID = $Константа.ЗонаВременногоТовара ";
            if (!SS.ExecuteWithRead(TextQuery, out DT))
            {
                return;
            }
            string IDTempZone = DT.Rows[0]["val"].ToString();

            //Проверка
            if (e.AdressZone.ID.ToString() != IDTempZone)
            {
                //НЕ та зона
                Negative("Нельзя! Адрес другой зоны!");
            }
           
            Adress = e;
            Positive("Адрес принят! Продолжайте ...");
        } // ReactionSCSectionDo
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void ReactionDocDo(Doc doc)
        {
            DataRow[] DR = SS.AcceptedCross.Select("IDDOC = '" + doc.ID + "'");
            if (DR.Length > 0 && (int)DR[0]["CountPack"] < (int)DR[0]["CountPackFull"])
            {
                DR[0]["CountPack"] = (int)DR[0]["CountPack"] + 1;              
            }
            Positive();
        } // ReactionSCSectionDo
       
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private ABaseMode Refresh()
        {
            SS.AcceptedCross = null;
            string TextQuery = "select * from dbo.WPM_fn_GetTaskAccemtanceCross()";
            if (!SS.ExecuteWithRead(TextQuery, out SS.AcceptedCross))
            {
                Negative("Не удалось получить задание");
                return JumpTo(new ChoiseWorkAcceptance(SS, this));
            }
            if (SS.AcceptedCross.Rows.Count == 0)
            {
                Negative("Нет заданий");
                return JumpTo(new ChoiseWorkAcceptance(SS, this));
            }
            return Positive();
        } // Refresh
        /// <summary>
        /// 
        /// </summary>
        private void CompleteLine()
        {
            if (!Adress.Selected)
            {
                Negative("Не указан адрес товара!");
                return;
            }

            string taskComplCross;
            taskComplCross = "";
            foreach (DataRow dr in SS.AcceptedCross.Rows)
            {
                if ((int)dr["CountPack"] == (int)dr["CountPackFull"])
                {
                    //полностью сделана
                    taskComplCross += dr["IDDOC"].ToString() + ",";
                }
            } 
            //отрежим последнюю запятую
            taskComplCross = taskComplCross.Substring(0, taskComplCross.Length - 1);
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = SS.ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход2"] = SS.ExtendID(Adress.ID, "Спр.Секции");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = taskComplCross;

            Dictionary<string, object> DataMapRead;
            List<string> FieldList = new List<string>();
            FieldList.Add("Спр.СинхронизацияДанных.ДатаРез1");
            if (!SS.ExecCommand("CompleteAcceptanceCross", DataMapWrite, FieldList, out DataMapRead))
            {
                Negative();
                return;
            }
            if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] == -3)
            {
                Negative(DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString());
                return;
            }
            if ((int)(decimal)DataMapRead["Спр.СинхронизацияДанных.ФлагРезультата"] != 3)
            {
                Negative("Не известный ответ робота... я озадачен...");
                return;
            }
            FExcStr = DataMapRead["Спр.СинхронизацияДанных.ДатаРез1"].ToString();
                       
            Refresh();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Count"></param>
        private void EnterCount()
        {
            //if (CurrentAction != ActionSet.EnterCount)
            //{
            //    Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
            //    return;
            //}
            //if (Amount != InputedCount)
            //{
            //    //Negative("Количество указано неверно! (нужно " + Amount.ToString() + ")");
            //    Negative("Количество указано неверно!");
            //    return;
            //}
            //SS.OnReport(new ReportEventArgs("Фиксирую..."));
            CompleteLine();
        } // EnterCount
    }
}
