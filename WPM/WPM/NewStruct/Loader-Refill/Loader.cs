using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class Loader : LoaderChoise
    {
        public ActionSet CurrentAction;     //Текущее действие
        protected string PalleteBarcode;    //Паллета
        protected RefSection Adress1;       //Куда ставим

        public Loader(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            CurrentMode = Mode.Loader;
            Task = (CallObj as LoaderChoise).Task;
            PalleteBarcode = null;
        } // Loader (constructor)
        protected override void ReactionKeyDo(Keys key)
        {
            if (key == Keys.Escape)
            {
                Cancel();
            }
        } // ReactionKeyDo
        internal override ABaseMode Init()
        {
            //По логике - не нужная проверка, но пусть будет, потому как если что-то проскочит сюда без задания
            //то все распидарасит к хуям
            if (!Task.Selected)
            {
                return Negative("Нет задания, говорю же!");
            }
            //И куда же нам идти???
            switch (Task.TypeMove)
            {
                case 1:
                    return JumpTo(new LoaderDown(SS, this));
                case 2:
                    return JumpTo(new LoaderUp(SS, this));
                case 3:
                    return JumpTo(new LoaderDownAntrisole(SS, this));
                case 4:
                    return JumpTo(new LoaderTransport(SS, this));
                case 5:
                    return JumpTo(new LoaderLift(SS, this));
                default:
                    return Negative("Данный режим еще не прикручен!");
            }
        } // Init
        internal override ABaseMode Cancel()
        {
            string TextQuery;
            if (Task.TypeMove == 1)
            {
                //Это отмена задания на спуск - удаляем физически, т.к. при запросе задания запись создастся вновь
                TextQuery = "delete from $Спр.ПеремещенияПаллет where id = :id";
            }
            else if (Task.TypeMove == 2 || Task.TypeMove == 3 || Task.TypeMove == 4 || Task.TypeMove == 5)
            {
                //А это отмена задания подъема или спуска с антресоли. Т.к. при выдаче задания ничего не создается,
                //  а просто заполняется данными, то и при отмене нужно просто очистить все
                TextQuery =
                    "update $Спр.ПеремещенияПаллет set " +
                        "$Спр.ПеремещенияПаллет.Сотрудник1 = :EmptyID, " +
                        "$Спр.ПеремещенияПаллет.ФлагОперации = 0, " +
                        "$Спр.ПеремещенияПаллет.Дата10 = :EmptyDate, " +
                        "$Спр.ПеремещенияПаллет.Время10 = 0 " +
                    "where id = :id";
            }
            else
            {
                return Negative("Нельзя отменить данный тип задания");
            }
            SQL1S.QuerySetParam(ref TextQuery, "id", Task.ID);
            SS.ExecuteWithoutReadNew(TextQuery);
            return base.Cancel();
        } // Cancel
        /// <summary>
        /// LoaderLift, LoaderDownAntrisole
        /// </summary>
        /// <param name="e"></param>
        protected override void ReactionSCSectionDo(RefSection e)
        {
            if (CurrentAction != ActionSet.ScanAdress)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            Adress1 = e;
            TaskEnd();
        } // ReactionSCSectionDo
        protected override void ReactionPalleteDo(string barcode)
        {
            if (CurrentAction != ActionSet.ScanPallete)
            {
                Negative("Неверно! " + SS.WhatUNeed(CurrentAction));
                return;
            }
            if (Task.PalleteBarcode != barcode)
            {
                Negative("Неверная паллета! Где вы это взяли?");
                return;
            }
            CurrentAction = ActionSet.ScanAdress;
            PalleteBarcode = barcode;
            Positive(SS.WhatUNeed(CurrentAction));
        } // ReactionPalleteDo


        /// <summary>
        /// 
        /// </summary>
        protected void TaskEnd()
        {
            //просканировали сколько нужно раз (2 раза)
            if (!TaskComplete())
            {
                Negative("Не удалось зафиксировать " + Task.TypeMoveDescr + "! Попробуйте еще раз");
                return;
            }
            //Пытаемся получить новое задание
            GoNextTask();
            if (Result == MMResult.Negative)
            {
                JumpTo(new LoaderChoise(SS, this));
                return;
            }
            // т.к. мы остались в том же режиме, то принудительно вызовем событие смены режима
            SS.OnChangeMode(new ChangeModeEventArgs(CurrentMode));
        } // TaskEnd
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        virtual protected bool TaskComplete()
        {
            string TextQuery =
                    "update $Спр.ПеремещенияПаллет set " +
                        "$Спр.ПеремещенияПаллет.Дата1 = :NowDate, " +
                        "$Спр.ПеремещенияПаллет.Время1 = :NowTime, " +
                        "$Спр.ПеремещенияПаллет.ФлагОперации = 2 " +
                    "where id = :id; " +
                    "select @@rowcount;";
            SQL1S.QuerySetParam(ref TextQuery, "id", Task.ID);
            return (int)SS.ExecuteScalar(TextQuery) == 0 ? false : true;
        } // DownTaskComplete
        virtual protected void GoNextTask() { throw new NotImplementedException("Child class do not implemented method GoNextTask"); } // GoNextTask
    }
}
