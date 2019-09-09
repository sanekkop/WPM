using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    //Это режим погрузчика. Все остальные режимы связанные с погрузкой будут наследоваться от этого режима,
    //а может и нет, как пойдет
    class LoaderChoise : ABaseStandart
    {
        public RefPalleteMove Task;
        public DataTable TaskList;

        public LoaderChoise(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.LoaderChoise;
            FVoiceOn = true;
        } // LoaderChoise (constructor)
        protected override void ReactionKeyDo(Keys key)
        {
            if (key == Keys.Escape || key == Keys.D0)
            {
                Cancel();
                return;
            }
            else if (key == Keys.Enter || Helper.IsGreenKey(key))
            {
                SS.OnReport(new ReportEventArgs("Обновляю список..."));
                Init();
                return;
            }

            int Choise = Helper.WhatInt(key);
            if (Choise > 0 && Choise < 9 && Choise <= TaskList.Rows.Count)
            {
                //Номер строки это и есть код
                bool allowed = (int)TaskList.Rows[Choise - 1]["allowed"] == 1 ? true : false;

                if (allowed)
                {
                    //Переход в режимы
                    if (Choise == 1)
                    {
                        SS.OnReport(new ReportEventArgs("Выбран лифт..."));
                        JumpTo(new LoaderChoiseLift(SS, this));
                    }
                    else if (Choise == 2)
                    {
                        SS.OnReport(new ReportEventArgs("Выбран спуск..."));
                        //SS.OnReport(new ReportEventArgs("Выбран спуск и подъем..."));
                        GoDownTask();
                    }
                    else if (Choise == 3)
                    {
                        SS.OnReport(new ReportEventArgs("Выбран подъем..."));
                        GoUpTask();
                    }
                }
            }
        } // ReactionKeyDo
        internal override ABaseMode Init()
        {
            Task = new RefPalleteMove(SS);
            if (FindTaskId())
            {
                return JumpTo(new Loader(SS, this));
            }
            TaskList = null;
            string TextQuery = "select * from WPM_fn_ToModeLoaderChoise(:Employer)";
            SQL1S.QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            SS.ExecuteWithReadNew(TextQuery, out TaskList);
            return Positive("Выберите что будете делать...");
        } // Init()
        internal override ABaseMode Cancel()
        {
            return JumpTo(new ChoiseWorkSupply(SS, this));
        } // Cancel()

        //SPECIAL METHOD's
        private bool FindTaskId()
        {
            string TextQuery =
                "select top 1 " +
                    "Ref.id as id " +
                "from $Спр.ПеремещенияПаллет as Ref (nolock) " +
                "where " +
                    "Ref.$Спр.ПеремещенияПаллет.ФлагОперации = 1 " + 
                    "and Ref.ismark = 0 " + 
                    "and Ref.$Спр.ПеремещенияПаллет.Сотрудник1 = :Employer";
            SQL1S.QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            DataTable DT;
            SS.ExecuteWithReadNew(TextQuery, out DT);
            if (DT.Rows.Count == 0)
            {
                return false;
            }
            Task.FoundID(DT.Rows[0]["id"].ToString());
            return true;
        } // GetOrderId
        private ABaseMode GoLodaer()
        {
            if (Task.Selected)
            {
                return JumpTo(new Loader(SS, this));
            }
            //Все равно жопа  (палец + нос)
            return Negative("Никаких заданий нет!");
        } // GoLodaer
        protected ABaseMode GoUpTask()
        {
            Task = new RefPalleteMove(SS);
            string Tail =
               "select top 1 Ref.id " +
                    "from $Спр.ПеремещенияПаллет as Ref (nolock) " +
                        "left join RT_refill as refill (nolock) " +
                            "on refill.adress0 = Ref.$Спр.ПеремещенияПаллет.Адрес0 " +
                                "and (refill.flag in (0,1) " +
                                "and refill.id <= dbo.fn_RT_GetBoundID(:warehouse)) " +
                    "where " +
                        "Ref.$Спр.ПеремещенияПаллет.ФлагОперации = 0 " +
                        "and Ref.$Спр.ПеремещенияПаллет.ТипДвижения = :MoveType " +
                        "and refill.id is null " +
                        "and ref.ismark = 0 " +
                        "and ref.$Спр.ПеремещенияПаллет.Склад = :warehouse " + 
                    "order by Ref.id";
            SQL1S.QuerySetParam(ref Tail, "warehouse", Employer.Warehouse.ID);
            FixTask(2, Tail);
            return GoLodaer();
        } // GoUpTask
        protected ABaseMode GoLiftTask(string sector)
        {
            Task = new RefPalleteMove(SS);
            string Tail =
                "select top 1 Ref.id from $Спр.ПеремещенияПаллет as Ref (nolock) " +
                        "where " +
                            "Ref.$Спр.ПеремещенияПаллет.ФлагОперации = 0 " +
                            "and Ref.$Спр.ПеремещенияПаллет.ТипДвижения = :MoveType " +
                            "and Ref.$Спр.ПеремещенияПаллет.Адрес1_Сектор = :sector " + 
                            "and Ref.ismark = 0 " +
                    "order by Ref.id";
            SQL1S.QuerySetParam(ref Tail, "sector", sector);
            FixTask(5, Tail);
            return GoLodaer();
        } // GoUpTask
        protected ABaseMode GoDownTask()
        {
            Task = new RefPalleteMove(SS);
            //Задания нет - будем пытаться его выдать через справочник
            //Первую пытаемся выдать антрисоль
            FixTask(3);
            if (!Task.Selected)
            {
                //Антрисоли нет - теперь пробуем подъем
                //GoUpTask();
                if (!Task.Selected)
                {
                    //все равно - нихуя! пытаемся еще раз через хранимую процедуру (обычный спуск)
                    GetTaskLoaderDown();
                }
            }
            return GoLodaer();
        } // GoDownTask
        protected ABaseMode GoTransportTask()
        {
            Task = new RefPalleteMove(SS);
            FixTask(4);
            return GoLodaer();
        } // GoTransportTask
        private void FixTask(int MoveType, string Tail)
        {
            string TextQuery =
                "update $Спр.ПеремещенияПаллет " +
                    "set $Спр.ПеремещенияПаллет.Сотрудник1 = :Employer, " +
                        "$Спр.ПеремещенияПаллет.Дата10 = :NowDate, " +
                        "$Спр.ПеремещенияПаллет.Время10 = :NowTime, " +
                        "$Спр.ПеремещенияПаллет.ФлагОперации = 1 " +
                    "where id in (" + Tail + ")";
            SQL1S.QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            SQL1S.QuerySetParam(ref TextQuery, "MoveType", MoveType);
            SS.ExecuteWithoutReadNew(TextQuery);
            FindTaskId();
        } // FixTask (int MoveType, string Tail)
        private void FixTask(int MoveType)
        {
            string Tail =
                "select top 1 Ref.id from $Спр.ПеремещенияПаллет as Ref (nolock) " +
                    "where " +
                        "Ref.$Спр.ПеремещенияПаллет.ФлагОперации = 0 " +
                        "and Ref.$Спр.ПеремещенияПаллет.ТипДвижения = :MoveType " +
                        "and Ref.ismark = 0 " + 
                        "and Ref.$Спр.ПеремещенияПаллет.Склад = :warehouse " +
                    "order by Ref.id";
            SQL1S.QuerySetParam(ref Tail, "warehouse", Employer.Warehouse.ID);
            FixTask(MoveType, Tail);
        } // FixTask (int MoveType)
        private void GetTaskLoaderDown()
        {
            string TextQuery = "exec WPM_GetTaskLoaderDown :Employer ";
            SQL1S.QuerySetParam(ref TextQuery, "Employer", Employer.ID);
            SS.ExecuteWithoutReadNew(TextQuery);
            FindTaskId();
        } // GetTaskLoaderDown

        protected bool CheckPallete(string barcode)
        {
            string TextQuery =
                "select count(*) from $Спр.ПеремещенияПаллет (nolock) " +
                "where " + 
                    "$Спр.ПеремещенияПаллет.ФлагОперации in (1,0) " +
                    "and $Спр.ПеремещенияПаллет.ШКПаллеты = :barcode " + 
                    "and ismark = 0";
            SQL1S.QuerySetParam(ref TextQuery, "barcode", barcode);
            if ((int)SS.ExecuteScalar(TextQuery) > 0)
            {
                Negative("Возможна только одна паллета с одним ШК!");
                return false;
            }
            return true;
        } // CheckPallete
    }
}
