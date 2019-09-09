using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    class ConstantsDepot
    {
        private SQL1S SS;
        private double FUpdateInterval;

        /// <summary>
        /// интервал обновления значений в базе данных (в секундах)
        /// </summary>
        public double UpdateInterval
        {
            get { return FUpdateInterval; }
            set
            {
                if (value > 0)
                {
                    FUpdateInterval = value;
                }
            }
        }
        /// <summary>
        /// order control
        /// </summary>
        public bool OrderControl { get { CondRefresh(); return FSettingsMOD.Substring(13, 1) == "0" ? false : true; } }
        public bool BoxSetOn { get { CondRefresh(); return FSettingsMOD.Substring(30, 1) == "0" ? true : false; } }
        /// <summary>
        /// Включить режим жмака картинок
        /// </summary>
        public bool ImageOn
        {
            get { CondRefresh(); return FSettingsMOD.Substring(24, 1) == "0" ? false : true; }
        }
        /// <summary>
        /// Если включена (true), то запрещена корректировка любых сборочных
        /// </summary>
        public bool StopCorrect
        {
            //get { CondRefresh(); return FSettingsMOD.Substring(30, 1) == "0" ? false : true; }
            get { return false; }   //Отключена!
        }
        /// <summary>
        /// Количество машин, что бы это не значило
        /// </summary>
        public int CarsCount
        {
            get { CondRefresh(); return Convert.ToInt32(FSettingsMOD.Substring(26, 1)); }
        }
        /// <summary>
        /// Главный склад - для него будут считаться образцы и остатки в карточке товара
        /// </summary>
        public string MainWarehouse
        {
            get { return FMainWarehouse; }
        }
        /// <summary>
        /// Товар для единиц из подчинения которого будет подсасывать новые единицы
        /// </summary>
        public string ItemForUnits
        {
            get { return FItemForUnits; }
        }

        /// <summary>
        /// Строка настройки обменна мод высосанная из базы
        /// </summary>
        private string FSettingsMOD;
        private string FMainWarehouse;
        private string FItemForUnits;

        /// <summary>
        /// Штамп последнего обновления данных из конфы
        /// </summary>
        private DateTime RefreshTimestamp;

        /// <summary>
        /// Обновляет значения, только если превышено время хранения
        /// </summary>
        private void CondRefresh()
        {
            if ((DateTime.Now - RefreshTimestamp).TotalSeconds > FUpdateInterval)
            {
                Refresh(false);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReadySS">prepared for use object</param>
        public ConstantsDepot(SQL1S ReadySS)
        {
            FUpdateInterval = 600; //Раз в 10 минут
            FSettingsMOD = "0000000000000000000000000000000000000000000000000000000";    //default value
            SS = ReadySS;
            Refresh();
        }
        /// <summary>
        /// Обновляет данные, сосет из базы (все данные обновляются)
        /// </summary>
        public void Refresh()
        {
            Refresh(true);
        }
        /// <summary>
        /// непосредственно сосет из базы
        /// </summary>
        /// <param name="RefreshAll">обновлять все</param>
        private void Refresh(bool RefreshAll)
        {
            DataTable DT;
            //Настройки обмена МОД
            string TextQuery = "SELECT VALUE as val FROM _1sconst (nolock) WHERE ID = $Константа.НастройкиОбменаМОД ";
            if (!SS.ExecuteWithRead(TextQuery, out DT))
            {
                return;
            }
            RefreshTimestamp = DateTime.Now;
            FSettingsMOD = DT.Rows[0]["val"].ToString();

            //Эти обновляются только в принудиловку
            if (RefreshAll)
            {
                //а тут подсасываем констатну главного склада
                TextQuery = "SELECT VALUE as val FROM _1sconst (nolock) WHERE ID = $Константа.ОснСклад ";
                DT.Clear();
                if (!SS.ExecuteWithRead(TextQuery, out DT))
                {
                    return;
                }
                FMainWarehouse = DT.Rows[0]["val"].ToString();

                //тут подсасываем константу товар для единиц
                TextQuery = "SELECT VALUE as val FROM _1sconst (nolock) WHERE ID = $Константа.ТоварДляЕдиниц ";
                DT.Clear();
                if (!SS.ExecuteWithRead(TextQuery, out DT))
                {
                    return;
                }
                FItemForUnits = DT.Rows[0]["val"].ToString();
            }
        }
    }
}
