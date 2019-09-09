using System;
using System.Collections.Generic;
using System.Text;

namespace WPM
{
    abstract class ARef
    {
        protected SQL1S SS;
        protected string FID;
        protected string FName;
        protected string FCode;
        protected bool FIsMark;
        /// <summary>
        /// xyueim
        /// </summary>
        protected bool HaveName;
        protected bool HaveCode;
        private Dictionary<string, object> Attributes;

        protected abstract string TypeObj { get; }

        public string ID
        {
            get { return FID; }
        }
        public string Name
        {
            get { return FName; }
        }
        public string Code
        {
            get { return FCode; }
        }
        public bool Selected
        {
            get
            {
                return FID == null ? false : true;
            }
        }
        public bool IsMark
        {
            get { return FIsMark; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ReadySS">prepared for use object</param>
        public ARef(SQL1S ReadySS)
        {
            SS          = ReadySS;
            Attributes  = new Dictionary<string,object>();
        }
        private bool FoundIDDorID(string IDDorID, bool ThisID)
        {
            FID = null;
            string prefix = "Спр." + TypeObj + ".";
            Dictionary<string, object> DataMap;
            List<string> FieldList = new List<string>();
            FieldList.Add("ID");
            FieldList.Add("ISMARK");
            if (HaveName)
            {
                FieldList.Add("DESCR");
            }
            if (HaveCode)
            {
                FieldList.Add("CODE");
            }
            if (!ThisID)
            {
                FieldList.Add(prefix + "IDD");
            }
            int ServCount = FieldList.Count;    //Количество сервисных полей
            SS.AddKnownAttributes(prefix, ref FieldList);
            if (!SS.GetSCData(IDDorID, TypeObj, FieldList, out DataMap, ThisID))
            {
                return false;
            }
            FID = DataMap["ID"].ToString();
            FIsMark = (bool)DataMap["ISMARK"];
            FCode = HaveCode ? DataMap["CODE"].ToString() : null;
            FName = HaveName ? DataMap["DESCR"].ToString().Trim() : null;
            //Добавляем оставшиеся поля в словарик
            for (int i = ServCount + 1; i < FieldList.Count; i++)
            {
                Attributes[FieldList[i].Substring(prefix.Length)] = DataMap[FieldList[i]];
            }
            return true;
        }
        public bool FoundIDD(string IDD)
        {
            return FoundIDDorID(IDD, false);
        }
        public bool FoundID(string ID)
        {
            return FoundIDDorID(ID, true);
        }
        public object GetAttribute(string Name)
        {
            //Классная штука ниже, но опасная, может что-то наебнуться. Неохота думать
            //if (!Selected)
            //{
            //    throw new NullReferenceException("Reference element not selected");
            //}
            if (Attributes.ContainsKey(Name))
            {
                return Attributes[Name];
            }
            //Подгружаем недостающий атрибут (он добавится в карту соответствия и будет в дальнейшем подгружаться сразу)
            Dictionary<string, object> DataMap;
            if (!SS.GetSCData(FID, TypeObj, Name, out DataMap, true))
            {
                return "";    //не срослось
            }
            object result = DataMap["Спр." + TypeObj + "." + Name];
            Attributes[Name] = result;
            return result;
        }
        public virtual void Refresh()
        {
            if (Selected)
            {
                FoundIDDorID(ID, true);
            }
        }


        //---------------
        //Две процедуры ниже нужно переработать как-то в одну, я хз как пока

        /// <summary>
        /// write in you property: %Prop% get { return GetSectionProperty(%name%, ref %FProp%); }
        /// </summary>
        /// <param name="name">1C name of prop</param>
        /// <param name="val">field stored object</param>
        /// <returns>value of property</returns>
        protected RefSection GetSectionProperty(string name, ref RefSection val)
        {
            if (val == null)
            {
                val = new RefSection(SS);
            }
            string currId = GetAttribute(name).ToString();
            if (val.ID != currId)
            {
                val.FoundID(currId);
            }
            return val;
        } // GetGatesProperty
        /// <summary>
        /// write in you property: %Prop% get { return GetSectionProperty(%name%, ref %FProp%); }
        /// </summary>
        /// <param name="name">1C name of prop</param>
        /// <param name="val">field stored object</param>
        /// <returns>value of property</returns>
        protected RefGates GetGatesProperty(string name, ref RefGates val)
        {
            if (val == null)
            {
                val = new RefGates(SS);
            }
            string currId = GetAttribute(name).ToString();
            if (val.ID != currId)
            {
                val.FoundID(currId);
            }
            return val;
        } // GetGatesProperty
    }
}
