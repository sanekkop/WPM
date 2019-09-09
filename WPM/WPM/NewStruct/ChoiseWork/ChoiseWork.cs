using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class ChoiseWork : ABaseStandart
    {
        private RefEmployer NewEmployer;

        public ChoiseWork(Model ReadySS, ABaseMode CallObj, string IDD)
            : base(ReadySS, CallObj)
        {
            Inicialize();
            NewEmployer = new RefEmployer(SS);
            NewEmployer.FoundIDD(IDD);
        } // ChoiseWork (constructor)
        public ChoiseWork(Model ReadySS, ABaseMode CallObj)
            : base(ReadySS, CallObj)
        {
            Inicialize();
            CurrentMode = Mode.ChoiseWork;
            NewEmployer = Employer;
        } // ChoiseWork (constructor 2)
        private void Inicialize()
        {
            CurrentMode = Mode.ChoiseWork;
        } // Inicialize

        protected override void ReactionDocDo(Doc doc)
        {
            if (doc.TypeDoc == "КонтрольНабора")
            {
                JumpTo(new SetTransfer(SS, this, doc));
                return;
            }
            base.ReactionDocDo(doc);
        }

        protected override void ReactionSCSectionDo(RefSection Section)
        {
            //подтянем данные адреса
            string TextQuery =
                "DECLARE @curdate DateTime; " +
                "SELECT @curdate = DATEADD(DAY, 1 - DAY(curdate), curdate) FROM _1ssystem (nolock); " +
                "SELECT " +
                    "RegAOT.$Рег.АдресОстаткиТоваров.Товар as ID, " +
                    "min(Goods.descr) as ItemName, " +
                    "min(Goods.$Спр.Товары.ИнвКод ) as InvCode, " +
                    "min(Goods.$Спр.Товары.Артикул ) as Article, " +
                    "cast(sum(RegAOT.$Рег.АдресОстаткиТоваров.Количество ) as int) as Count, " +
                    "min(RegAOT.$Рег.АдресОстаткиТоваров.Адрес ) as Adress, " +
                    "CASE " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = -10 THEN '-10 Автокорректировка' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = -2 THEN '-2 В излишке' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = -1 THEN '-1 В излишке (пересчет)' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 0 THEN '00 Не существует' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 1 THEN '01 Приемка' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 2 THEN '02 Хороший на месте' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 3 THEN '03 Хороший (пересчет)' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 4 THEN '04 Хороший (движение)' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 7 THEN '07 Бракованный на месте' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 8 THEN '08 Бракованный (пересчет)' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 9 THEN '09 Бракованный (движение)' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 12 THEN '12 Недостача' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 13 THEN '13 Недостача (пересчет)' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 14 THEN '14 Недостача (движение)' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 17 THEN '17 Недостача подтвержденная' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 18 THEN '18 Недостача подт.(пересчет)' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 19 THEN '19 Недостача подт.(движение)' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 22 THEN '22 Пересорт излишек' " +
                    "WHEN RegAOT.$Рег.АдресОстаткиТоваров.Состояние = 23 THEN '23 Пересорт недостача' " +
                    "ELSE rtrim(cast(RegAOT.$Рег.АдресОстаткиТоваров.Состояние as char)) + ' <неизвестное состояние>' END as Condition, " + 
                    "min(Sections.descr) as AdressName " +
                "FROM " +
                    "RG$Рег.АдресОстаткиТоваров as RegAOT (nolock) " + 
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.ID = RegAOT.$Рег.АдресОстаткиТоваров.Товар " +
                    "LEFT JOIN $Спр.Секции as Sections (nolock) " +
                        "ON Sections.ID = RegAOT.$Рег.АдресОстаткиТоваров.Адрес " +                    
                "WHERE " +
                    "RegAOT.period = @curdate " +
                    "and RegAOT.$Рег.АдресОстаткиТоваров.Адрес = :Address " +
                "GROUP BY RegAOT.$Рег.АдресОстаткиТоваров.Товар , " + 
                "RegAOT.$Рег.АдресОстаткиТоваров.Состояние " +
                "ORDER BY min(Goods.$Спр.Товары.ИнвКод )";
            SQL1S.QuerySetParam(ref TextQuery, "Address", Section.ID);
            if (SS.ExecuteWithRead(TextQuery, out SS.AddressCardItems))
            {
                //покажем содержание адреса
                SS.OnReport(new ReportEventArgs("Задача выбрана..."));
                JumpTo(new ChoiseWorkAddressCard(SS, this));
             }
            base.ReactionSCSectionDo(Section);
        }
        private Mode CheckOrder(Mode NowMode, RefEmployer Who)
        {
            Mode result = NowMode;
            //ЗАДАНИЕ СПУСКА
            if (Who.CanDown)
            {
                string TextQuery =
                    "select top 1 " +
                            "Ref.id " +
                        "from $Спр.МестаПогрузки as Ref (nolock) " +
                            "where " +
                                "Ref.$Спр.МестаПогрузки.Сотрудник4 = :Employer " +
                                "and Ref.ismark = 0 " +
                                "and not Ref.$Спр.МестаПогрузки.Дата40 = :EmptyDate " +
                                "and not Ref.$Спр.МестаПогрузки.Адрес3 = :EmptyID " +
                                "and Ref.$Спр.МестаПогрузки.Дата5 = :EmptyDate ";
                SQL1S.QuerySetParam(ref TextQuery, "Employer", Who.ID);
                DataTable DT;
                if (SS.ExecuteWithRead(TextQuery, out DT))
                {
                    if (DT.Rows.Count > 0)
                    {
                        return Mode.Down;
                    }
                }
            }
            //ЗАДАНИЕ СВОБОДНОГО СПУСКА/КОМПЛЕКТАЦИИ
            if (Who.CanComplectation)
            {
                string TextQuery =
                    "select top 1 " +
                            "Ref.id " +
                        "from $Спр.МестаПогрузки as Ref (nolock) " +
                            "where " +
                                "Ref.ismark = 0 " +
                                "and (" +
                                        "Ref.$Спр.МестаПогрузки.Сотрудник4 = :Employer " +
                                        "and Ref.$Спр.МестаПогрузки.Дата5 = :EmptyDate " +
                                        "and Ref.$Спр.МестаПогрузки.Адрес3 = :EmptyID " +
                                        "and not Ref.$Спр.МестаПогрузки.Дата40 = :EmptyDate" +
                                    " or " +
                                        "Ref.$Спр.МестаПогрузки.Сотрудник8 = :Employer " +
                                        "and Ref.$Спр.МестаПогрузки.Дата9 = :EmptyDate " +
                                        "and Ref.$Спр.МестаПогрузки.Адрес7 = :EmptyID " +
                                        "and not Ref.$Спр.МестаПогрузки.Дата80 = :EmptyDate)";
                SQL1S.QuerySetParam(ref TextQuery, "Employer", Who.ID);
                DataTable DT;
                if (SS.ExecuteWithRead(TextQuery, out DT))
                {
                    if (DT.Rows.Count > 0)
                    {
                        return Mode.FreeDownComplete;
                    }
                }
            }
            //ЗАДАНИЕ ОТБОРА КОМПЛЕКТАЦИИ
            if (Who.CanComplectation)
            {
                string TextQuery =
                    "select top 1 " +
                            "Ref.id " +
                        "from $Спр.МестаПогрузки as Ref (nolock) " +
                            "where " +
                                "Ref.ismark = 0 " +
                                "and Ref.$Спр.МестаПогрузки.Сотрудник8 = :Employer " +
                                "and Ref.$Спр.МестаПогрузки.Дата9 = :EmptyDate " +
                                "and not Ref.$Спр.МестаПогрузки.Адрес7 = :EmptyID " +
                                "and not Ref.$Спр.МестаПогрузки.Дата80 = :EmptyDate";
                SQL1S.QuerySetParam(ref TextQuery, "Employer", Who.ID);
                DataTable DT;
                if (SS.ExecuteWithRead(TextQuery, out DT))
                {
                    if (DT.Rows.Count > 0)
                    {
                        return Mode.NewComplectation;
                    }
                }
            }

            return result;
        } // CheckOrder
        internal override ABaseMode Init()
        {
            if (!NewEmployer.Selected)
            {
                return Negative("Чудовищная ошибка программы! СРОЧНО ЗВОНИТЕ ПУТИНУ!");
            }
            if (NewEmployer.IsMark)
            {
                return Negative("Вы, вероятно, уволены!");
            }
            Mode ToMode = CheckOrder(Mode.ChoiseWork, NewEmployer);

            //Пишем команду login!
            if (Employer == null || NewEmployer.ID != Employer.ID)
            {
                //Это перелогин или первый логин
                if (Employer != null && NewEmployer.ID != Employer.ID)
                {
                    //Перелогин, нужно сделать логаут предыдущему челу
                    if (!Logout())
                    {
                        return Negative(SS.ExcStr);
                    }
                }
                RefEmployer OldEmployer = FEmployer;
                FEmployer = NewEmployer;
                if (!Login())
                {
                    FEmployer = OldEmployer;
                    if (OldEmployer != null)
                    {
                        FEmployer.Refresh();
                    }
                    return Negative(SS.ExcStr);
                }
            }

            SS.Employer = FEmployer;    //ВРЕМЕННО, Чтобы работала обратная совместимость

            Const.Refresh();

            if (ToMode == Mode.Down)
            {
                if (!SS.ToModeDown())
                {
                    return Negative(SS.ExcStr);
                }
                return Positive();
            }
            else if (ToMode == Mode.FreeDownComplete)
            {
                if (!SS.ToModeFreeDownComplete())
                {
                    return Negative(SS.ExcStr);
                }
                return Positive();
            }
            else if (ToMode == Mode.NewComplectation)
            {
                if (!SS.ToModeNewComplectation())
                {
                    return Negative(SS.ExcStr);
                }
                return Positive();
            }
            else
            {
                return Positive();
            }
        } // Init
        internal override ABaseMode Cancel()
        {
            return JumpTo(new Waiting(SS, this));
        } // Cancel
        protected override void ReactionKeyDo(Keys key)
        {
            Mode DesireMode;

            if (key == Keys.D0 && Employer.CanSet)
            {
                DesireMode = Mode.SetInicialization;
            }
            else if (key == Keys.D1 && Employer.CanAcceptance)
            {
                SS.OnReport(new ReportEventArgs("Задача выбрана..."));
                JumpTo(new ChoiseWorkAcceptance(SS, this));
                return;
                //DesireMode = Mode.Acceptance;
            }
            else if (key == Keys.D2 && Employer.CanTransfer)
            {
                DesireMode = Mode.TransferInicialize;
            }
            else if (key == Keys.D3 && (Employer.CanComplectation || Employer.CanDown))
            {
                SS.OnReport(new ReportEventArgs("Задача выбрана..."));
                JumpTo(new ChoiseWorkShipping(SS, this));
                return;
            }
            else if (key == Keys.D4 && Employer.CanInventory)
            {
                DesireMode = Mode.NewChoiseInventory;
            }
            else if (key == Keys.D5 && Employer.CanLayOutSample)    //было CanGiveSample. Изменение по заявке #661(вход в неактивные режимы 5 и 6)
            {
                DesireMode = Mode.SampleInventory;
            }
            else if (key == Keys.D6 && Employer.CanLayOutSample)    //было CanGiveSample. Изменение по заявке #661(вход в неактивные режимы 5 и 6)
            {
                SS.OnReport(new ReportEventArgs("Задача выбрана..."));
                JumpTo(new ChoiseWorkSample(SS, this));
                return;
            }
            else if (key == Keys.D7 && (Employer.CanLoad || Employer.CanSupply))
            {
                SS.OnReport(new ReportEventArgs("Задача выбрана..."));
                JumpTo(new ChoiseWorkSupply(SS, this));
                return;
            }
            else if (key == Keys.D8 && true)
            {
                DesireMode = Mode.ControlCollect;
            }
            else if (key == Keys.D9 && true && Employer.CanHarmonization)
            {
                DesireMode = Mode.HarmonizationInicialize;
            }
            else
            {
                //просто нихуя не происходит или что там в родительском классе
                base.ReactionKeyDo(key);
                return;
            }

            //Обработка выбора работы
            SS.OnReport(new ReportEventArgs("Задача выбрана..."));
            if (!SS.ChoiseWork(DesireMode))
            {
                Negative(SS.ExcStr);
                return;
            }
            else if (DesireMode == Mode.SampleInventory)
            {
                if (SS.ForPrint.Rows.Count > 0)
                {
                    DialogResult DialRes = MessageBox.Show("Очистить список товаров для печати?", "Список для печати не пустой", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                    if (DialRes == DialogResult.Yes)
                    {
                        SS.ForPrint.Rows.Clear();
                        SS.OnChangeMode(new ChangeModeEventArgs(DesireMode));   //Костылина, обратная совместимость
                    }
                }
            }
            Positive();
        } // ReactionKeyDo
    }
}
