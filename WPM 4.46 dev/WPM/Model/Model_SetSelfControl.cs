using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace WPM
{
    partial class Model
    {
        public DataTable TableSet;

        //BASE METHOD's
        public bool ToModeSetSelfControl()
        {
            if (!LoadTableSet())
            {
                return false;
            }
            if (TableSet.Rows.Count == 0)
            {
                //Ничего нет для самоконтроля, пойдем в завершение набора
                return ToModeSetComplete();
            }

            CCItem = new StructItemSet();
            PreviousAction = "";
            FExcStr = "Сканируйте товар!";
            FCurrentMode = Mode.SetSelfContorl;
            CurrentAction = ActionSet.ScanItem;
            return true;
        }

        private bool RBSetSelfControl(string Barcode)
        {
            if (CurrentAction != ActionSet.ScanItem)
            {
                FExcStr = "Неверно! " + WhatUNeed();
                return false;
            }
            string TextQuery =
                "SELECT " +
                    "Units.parentext as ItemID, " +
                    "Units.$Спр.ЕдиницыШК.ОКЕИ as OKEI " +
                "FROM $Спр.ЕдиницыШК as Units (nolock) " +
                    "LEFT JOIN $Спр.Товары as Goods (nolock) " +
                        "ON Goods.id = Units.parentext " +
                "WHERE Units.$Спр.ЕдиницыШК.Штрихкод = :Barcode ";
            QuerySetParam(ref TextQuery, "Barcode", Barcode);
            DataTable DT;
            if (!ExecuteWithRead(TextQuery, out DT))
            {
                return false;
            }
            if (DT.Rows.Count == 0)
            {
                FExcStr = "С таким штрихкодом товар не найден! " + WhatUNeed();
                return false;
            }
            DataRow[] DR = TableSet.Select("Item = '" + DT.Rows[0]["ItemID"].ToString() + "'");
            if (DR.Length == 0)
            {
                FExcStr = "Товара нет в сборочном или он уже проконтролирован!";
                return false;
            }
            CCItem = new StructItemSet();
            CCItem.ID       = DR[0]["Item"].ToString();
            CCItem.InvCode  = DR[0]["InvCode"].ToString();
            CCItem.Name     = DR[0]["Descr"].ToString();
            CCItem.Details  = (int)(decimal)DR[0]["Details"];
            CCItem.Count    = (int)(decimal)DR[0]["Amount"];
            CurrentAction = ActionSet.EnterCount;

            FExcStr = WhatUNeed();
            return true;
        }

        //SPECIAL METHOD's
        private bool LoadTableSet()
        {
            if (TableSet != null)
            {
                TableSet.Clear();
            }
            string TextQuery =
                "select " +
                    "min(Ref.$Спр.Товары.ИнвКод ) as InvCode, " +
                    "min($Спр.Товары.КоличествоДеталей ) as Details, " +
                    "DocT.$КонтрольНабора.Товар as Item, " +
                    "min(Ref.descr) as Descr, " + 
                    "sum(DocT.$КонтрольНабора.Количество ) as Amount " +
                "from DT$КонтрольНабора as DocT (nolock) " +
                    "left join $Спр.Товары as Ref (nolock) " + 
                        "on Ref.id = DocT.$КонтрольНабора.Товар " + 
                "where " +
                    "iddoc = :iddoc " +
                    "and DocT.$КонтрольНабора.Состояние0 = 2 " +
                    "and DocT.$КонтрольНабора.Контроль <= 0 " +
                    "and DocT.$КонтрольНабора.СостояниеКорр = 0 " +
                    "and DocT.$КонтрольНабора.Количество > 0 " + 
                "group by " +
                    "DocT.$КонтрольНабора.Товар ";
            QuerySetParam(ref TextQuery, "iddoc", DocSet.ID);
            if (!ExecuteWithRead(TextQuery, out TableSet))
            {
                return false;
            }
            return true;
        }
        public bool EnterCountSetSelfControl(int Count)
        {
            if (CurrentAction != ActionSet.EnterCount)
            {
                FExcStr = "Неверно! " + WhatUNeed();
                return false;
            }
            if (Count <= 0)
            {
                FExcStr = "Количество указано неверно!";
                return false;
            }
            string QueryPart;
            if (CCItem.Count == Count)
            {
                //Контроль удачно пройден, очистим таблицу
                QueryPart = "set $КонтрольНабора.Контроль = -1*($КонтрольНабора.Контроль -1) ";
                DataRow[] DR = TableSet.Select("Item = '" + CCItem.ID + "'");
                //Полюбому найдется строка и полюбому только одна
                DR[0].Delete();
            }
            else
            {
                QueryPart = "set $КонтрольНабора.Контроль = $КонтрольНабора.Контроль - 1 ";
            }
            string TextQuery =
                "update DT$КонтрольНабора " + QueryPart + 
                    "where " + 
                        "DT$КонтрольНабора .iddoc = :iddoc " + 
                        "and DT$КонтрольНабора .$КонтрольНабора.Товар = :item ";
            QuerySetParam(ref TextQuery, "iddoc", DocSet.ID);
            QuerySetParam(ref TextQuery, "item", CCItem.ID);
            if (!ExecuteWithoutRead(TextQuery))
            {
                return false;
            }
            PreviousAction  = "Проверено " + CCItem.InvCode.Trim() + " - " + Count.ToString() + ". " + (CCItem.Count == Count ? "" : "НЕВЕРНО!");
            CurrentAction   = ActionSet.ScanItem;
            FExcStr = WhatUNeed();
            bool result = CCItem.Count == Count ? true : false; //чтобы издать печальный звук
            CCItem = new StructItemSet();
            if (result && TableSet.Rows.Count == 0)
            {
                //все выбили, валим отсчюда!
                return ToModeSetComplete();
            }
            return result;
        } // EnterCountSetSelfControl
    }
}