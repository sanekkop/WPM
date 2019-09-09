using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace WPM
{
    class Helper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        static public string SuckDigits(string str)
        {
            string Numbers  = "01234567890";
            string result   = "";
            while (str.Length > 0)
            {
                if (Numbers.IndexOf(str.Substring(0, 1)) != -1)
                {
                    result += str.Substring(0, 1);
                }
                str = str.Substring(1);
            }
            return result;
        }
        /// <summary>
        /// делает из полного имени формат типа: Иванов И.И.
        /// </summary>
        /// <param name="FIO"></param>
        /// <returns></returns>
        static public string GetShortFIO(string FIO)
        {
            string result = "";
            FIO = FIO.Trim();
            bool space = false;
            bool surname = false;
            for (int i = 0; i < FIO.Length; i++)
            {
                string ch = FIO.Substring(i, 1);
                if (!surname)
                {
                    result += ch;
                }
                if (space)
                {
                    result += ch + ".";
                }
                surname = ch == " " ? true : surname;
                space = ch == " " ? true : false;
            }
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Barcode"></param>
        /// <returns></returns>
        static public string GetIDD(string Barcode)
        {
            string IDD = "";

            if (Barcode.Length == 18)
                IDD = "9999" + Barcode.Substring(5, 13);
            else //13 symbols
                IDD = "99990" + Barcode.Substring(2, 2) + "00" + Barcode.Substring(4, 8);

            return IDD;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Barcode"></param>
        /// <returns></returns>
        static public Dictionary<string, string> DisassembleBarcode (string Barcode)
        {
            Dictionary<string, string> result = new Dictionary<string,string>();
            result["Type"]  = "999";    //Код ошибки
            if (Barcode.Length == 18)
            {
                result["Type"]  = "118";
                result["IDD"]   = "9999" + Barcode.Substring(5, 13);
            }
            else if (Barcode.Length == 13)
            {
                if (Barcode.Substring(0, 4) == "2599")
                {
                    result["Type"] = "6";
                    //В следующих 8 разрядах - закодированный ИД справочника
                    string encodedID   = Translation.DecTo36(Barcode.Substring(4, 8));
                    encodedID = "      " + encodedID;
                    encodedID = encodedID.Substring(encodedID.Length - 6) + "   ";
                    result["ID"] = encodedID;
                }
                else if (Barcode.Substring(0, 9) == "259000000")
                {
                    result["Type"]  = "part";
                    result["count"] = Barcode.Substring(9, 3);
                }
                else if (Barcode.Substring(0, 4) == "2580")
                {
                    result["Type"] = "pallete";
                    result["number"] = Barcode.Substring(4, 8);
                }
                else
                {
                    result["Type"] = "113";
                    result["IDD"] = "99990" + Barcode.Substring(2, 2) + "00" + Barcode.Substring(4, 8);
                }
            }
            else
            {
                //128-Code (поехали, будем образать задние разряды полсе их обработки
                string binaryBar    = "00000" + Translation.DecTo2(Translation._36ToDec(Barcode));
                //Последние пять разрядов - тип
                string type         = Translation._2ToDec(binaryBar.Substring(binaryBar.Length - 6)).ToString();
                if (type == "5")
                {
                    result["Type"]      = type;
                    //В следующие 34 разряда - закодированный ИДД документа
                    binaryBar           = binaryBar.Substring(0, binaryBar.Length - 6);
                    string encodedIDD   = Translation._2ToDec(binaryBar.Substring(binaryBar.Length - 34)).ToString();
                    encodedIDD          = "000000000" + encodedIDD;
                    encodedIDD          = encodedIDD.Substring(encodedIDD.Length - 10); //получаем 10 правых символов
                    result["IDD"]       = "99990" + encodedIDD.Substring(0, 2) + "00" + encodedIDD.Substring(2);
                    //В следующих 20 разрядах кроется строка документа (вот так с запасом взял)
                    binaryBar           = "0000000000000000000" + binaryBar.Substring(0, binaryBar.Length - 34);
                    result["LineNo"]    = Translation._2ToDec(binaryBar.Substring(binaryBar.Length - 20)).ToString();
                }
            }
            return result;
        }
        /// <summary>
        /// Преобразует число секунд в строку вида ЧЧ:ММ
        /// </summary>
        /// <param name="Sec">Seconds since the beginning of the day</param>
        /// <returns></returns>
        static public string SecondToString(int Sec)
        {
            string Hours   = "0" + ((int)(Sec/3600)).ToString();
            Sec -= 3600*(int)(Sec/3600);
            string Minutes = "0" + ((int)(Sec/60)).ToString();
            Sec -= Sec - 60*(int)(Sec/60);
            return Hours.Substring(Hours.Length - 2, 2) + ":" + Minutes.Substring(Minutes.Length - 2, 2);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Barcode"></param>
        /// <returns></returns>
        static public int ControlSymbolEAN(string strBarcode)
        {
            int even = 0;
            int odd = 0;
            for (int i = 0; i < 6; i++)
            {
                even += Convert.ToInt32(strBarcode.Substring(2 * i + 1, 1));
                odd += Convert.ToInt32(strBarcode.Substring(2 * i, 1));
            }
            return (10 - (even * 3 + odd) % 10) % 10;
        }
        /// <summary>
        /// Pause. Empty cycle
        /// </summary>
        /// <param name="millisecond">how long</param>
        static public void Pause(int millisecond)
        {
            int Start = Environment.TickCount;
            while (Environment.TickCount - Start < millisecond)
            {
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="SourceStr"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        static public List<string> StringToList(string SourceStr, string separator)
        {
            SourceStr = SourceStr.Replace(" ", "");
            List<string> result = new List<string>();
            while (true)
            {
                int index = SourceStr.IndexOf(separator);
                index = index == -1 ? 0 : index;
                string thispart = SourceStr.Substring(0, index);
                if (thispart.Length > 0)
                {
                    result.Add(thispart);
                }
                if (index > 0)
                {
                    SourceStr = SourceStr.Substring(index + separator.Length);
                }
                else
                {
                    break;
                }
            }
            if (SourceStr.Length > 0)
            {
                result.Add(SourceStr);
            }
            return result;
        }
        static public List<string> StringToList(string SourceStr)
        {
            return StringToList(SourceStr, ",");
        }
        static public string ListToStringWithQuotes(List<string> SourceList)
        {
            string result = "";
            foreach (string element in SourceList)
            {
                result += ", '" + element + "'";
            }
            result = result.Substring(2);   //Убираем спедери запятые
            return result;
        }
        static public int WhatInt(Keys Key)
        {
            int result = -1;
            switch (Key)
            {
                case Keys.D0:
                    result = 0;
                    break;
                case Keys.D1:
                    result = 1;
                    break;
                case Keys.D2:
                    result = 2;
                    break;
                case Keys.D3:
                    result = 3;
                    break;
                case Keys.D4:
                    result = 4;
                    break;
                case Keys.D5:
                    result = 5;
                    break;
                case Keys.D6:
                    result = 6;
                    break;
                case Keys.D7:
                    result = 7;
                    break;
                case Keys.D8:
                    result = 8;
                    break;
                case Keys.D9:
                    result = 9;
                    break;
            }
            return result;
        }
        static public string ReverseString(string s)
        {
            char[] arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }
        static public string GetPictureFileName(string InvCode)
        {
            InvCode = InvCode.ToLower();
            string result = "";
            for (int i = 0; i < InvCode.Length; i++)
            {
                string symbol = InvCode.Substring(i, 1);
                switch (symbol)
                {
                    case "й"
			            :result += "iy";break;
		            case "ц" 
			            :result += "cc";break;
		            case "у" 
			            :result += "u";break;
		            case "к" 
			            :result += "k";break;
		            case "е" 
			            :result += "e";break;
		            case "н" 
			            :result += "n";break;
		            case "г" 
			            :result += "g";break;
		            case "ш" 
			            :result += "h";break;
		            case "щ" 
			            :result += "dg";break;
		            case "з" 
			            :result += "z";break;
		            case "х" 
			            :result += "x";break;
		            case "ъ" 
			            :result += "dl";break;
		            case "ф" 
			            :result += "f";break;
		            case "ы" 
			            :result += "y";break;
		            case "в" 
			            :result += "v";break;
		            case "а" 
			            :result += "a";break;
		            case "п" 
			            :result += "p";break;
		            case "р" 
			            :result += "r";break;
		            case "о" 
			            :result += "o";break;
		            case "л" 
			            :result += "l";break;
		            case "д" 
			            :result += "d";break;
		            case "ж" 
			            :result += "j";break;
		            case "э" 
			            :result += "w";break;
		            case "я" 
			            :result += "ya";break;
		            case "ч" 
			            :result += "ch";break;
		            case "с" 
			            :result += "s";break;
		            case "м" 
			            :result += "m";break;
		            case "и" 
			            :result += "i";break;
		            case "т" 
			            :result += "t";break;
		            case "ь" 
			            :result += "zz";break;
		            case "б" 
			            :result += "b";break;
		            case "ю" 
			            :result += "q";break;
		            case @"\" 
			            :result += "ls";break;
		            case @"/" 
			            :result += "ps";break;
		            case "1" 
			            :result += "1";break;
		            case "2" 
			            :result += "2";break;
		            case "3" 
			            :result += "3";break;
		            case "4" 
			            :result += "4";break;
		            case "5" 
			            :result += "5";break;
		            case "6" 
			            :result += "6";break;
		            case "7" 
			            :result += "7";break;
		            case "8" 
			            :result += "8";break;
		            case "9" 
			            :result += "9";break;
		            case "0" 
			            :result += "0";break;
                }
            }
            return result + ".gif";
        }
        static public bool IsGreenKey(Keys Key)
        {
            return (Key == Keys.F14 || Key == Keys.F2 || Key == Keys.F1 || Key.GetHashCode() == 189) ? true : false;
        }
    }
}
