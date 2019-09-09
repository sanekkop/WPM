using System;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    /// <summary>
    /// Клас для управления системным временем и датой
    /// </summary>
    class APIManager
    {
        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        public extern static void GetSystemTime(ref SYSTEMTIME lpSystemTime);
        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        public extern static uint SetSystemTime(ref SYSTEMTIME lpSystemTime);
        public struct SYSTEMTIME 
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }
        [System.Runtime.InteropServices.DllImport("coredll.dll")]
        public static extern void MessageBeep(uint BeepType);
        /// <summary>
        /// возвращает секунды с начала дня
        /// </summary>
        /// <returns></returns>
        public static int NowSecond()
        {
            DateTime DT = DateTime.Now;
            return DT.Hour*3600 + DT.Minute*60 + DT.Second;
        }
        /// <summary>
        /// возвращает секунды с начала дня
        /// </summary>
        /// <returns></returns>
        public static int NowMillisecond()
        {
            DateTime DT = DateTime.Now;
            return DT.Hour*3600000 + DT.Minute*60000 + DT.Second*1000 + DT.Millisecond;
        }
        /// <summary>
        /// возвращает секунды с начала дня
        /// </summary>
        /// <returns></returns>
        public static int NowSecondSystem()
        {
            SYSTEMTIME systime = new SYSTEMTIME();
            GetSystemTime(ref systime);
            return systime.wHour*3600 + systime.wMinute*60 + systime.wSecond;
        }
        /// <summary>
        /// возвращает милисекунды с начала дня
        /// </summary>
        /// <param name="systime"></param>
        /// <returns></returns>
        public static int Second(SYSTEMTIME systime)
        {
            return systime.wHour*3600 + systime.wMinute*60 + systime.wSecond;
        }
        /// <summary>
        /// возвращает текущее дату/время с начала дня
        /// </summary>
        /// <returns></returns>
        public static SYSTEMTIME Now()
        {
            SYSTEMTIME result = new SYSTEMTIME();
            GetSystemTime(ref result);
            return result;
        }
    }
}
