using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;

namespace WPM
{
    /// <summary>
    /// Базовый класс для всех режимов
    /// </summary>
    abstract class ABaseMode
    {
        private int JumpCounter;    //Сколько было прыжков из этого объекта (передается в другой объект)
        //public bool JumpSuccsess { get { return JumpCounter == 0 ? false : true; } }
        protected Model SS;
        protected bool FVoiceOn;
        public bool VoiceOn { get { return FVoiceOn; } }    //Издавать звуки при ответах положительном и отрицательном

        protected string Name
        {
            get 
            {
                //я не придумал как избавиться от namespace в имени, поэтому тупо вырезаю ее из строки
                //Если что-то пойдет не так - имей это в виду!
                return ToString().Replace("WPM.", "");
            }
        }
        
        protected RefEmployer FEmployer;
        public RefEmployer Employer
        {
            set { FEmployer = value; }
            get { return FEmployer; }
        }

        protected string FExcStr;
        public string ExcStr { get { return FExcStr; } }
        public Mode CurrentMode;

        public ConstantsDepot Const;

        private ABaseMode ResultObject;
        private MMResult FResult;
        //Результат обработки клавиатуры или ШК. Может реагировать положительно, отрицательно или вообще никак...
        public MMResult Result { get { return FResult; } }
        //public bool Result;
        private bool ThereHandler;     //Сработала подписка или нет на какое-либо событие
        private bool ThereHandlerDo;   //Вспомогательная хуйня, чтобы контролировать подписку по функциям (суффикс Do)


        //private void MarkThereHandler { ThereHandler = ThereHandler ? ThereHandler : false;

        protected delegate void ReactionSCEventHandler(object sender, ReactionSCEventArgs e);
        protected event ReactionSCEventHandler ReactionSC;
        virtual protected void ReactionSCDo(ReactionSCEventArgs e) { ThereHandlerDo = false;  } // ReactionSCDo
        private void OnReactionSC(ReactionSCEventArgs e)
        {
            ThereHandlerDo = true;  //Если функцию ниже никто не наследует, то это значение поменяется на false
            ReactionSCDo(e);
            ThereHandler = ThereHandler ? ThereHandler : ThereHandlerDo;    //если уже взведен, то не трогаем, иначе берет значение предыдущего флага
            if (ReactionSC != null)
            {
                ReactionSC(this, e);
                ThereHandler = true;
            }
        } // OnReactionSC
        
        protected delegate void ReactionBarcodeEventHandler(object sender, string Barcode);
        protected event ReactionBarcodeEventHandler ReactionBarcode;
        virtual protected void ReactionBarcodeDo(string barcode) { ThereHandlerDo = false;  } // ReactionBarcodeDo
        private void OnReactionBarcode(string e)
        {
            ThereHandlerDo = true;  //Если функцию ниже никто не наследует, то это значение поменяется на false
            ReactionBarcodeDo(e);
            ThereHandler = ThereHandler ? ThereHandler : ThereHandlerDo;    //если уже взведен, то не трогаем, иначе берет значение предыдущего флага
            if (ReactionBarcode != null)
            {
                ReactionBarcode(this, e);
                ThereHandler = true;
            }
        } // OnReactionBarcode
        
        protected delegate void ReactionSCEmployersHandler(object sender, RefEmployer employer);
        protected event ReactionSCEmployersHandler ReactionSCEmployers;
        virtual protected void ReactionSCEmployersDo(RefEmployer employer) { ThereHandlerDo = false; } // ReactionSCEmployersDo
        private void OnReactionSCEmployers(RefEmployer e)
        {
            ThereHandlerDo = true;  //Если функцию ниже никто не наследует, то это значение поменяется на false
            ReactionSCEmployersDo(e);
            ThereHandler = ThereHandler ? ThereHandler : ThereHandlerDo;    //если уже взведен, то не трогаем, иначе берет значение предыдущего флага
            if (ReactionSCEmployers != null)
            {
                ReactionSCEmployers(this, e);
                ThereHandler = true;
            }
        } // OnReactionSCEmployers
        
        protected delegate void ReactionSCSectionsHandler(object sender, RefSection section);
        protected event ReactionSCSectionsHandler ReactionSCSection;
        virtual protected void ReactionSCSectionDo(RefSection e) { ThereHandlerDo = false; } // ReactionSCDo
        private void OnReactionSCSection(RefSection e)
        {
            ThereHandlerDo = true;  //Если функцию ниже никто не наследует, то это значение поменяется на false
            ReactionSCSectionDo(e);
            ThereHandler = ThereHandler ? ThereHandler : ThereHandlerDo;    //если уже взведен, то не трогаем, иначе берет значение предыдущего флага
            if (ReactionSCSection != null)
            {
                ReactionSCSection(this, e);
                ThereHandler = true;
            }
        } // OnReactionSCSection
        
        protected delegate void ReactionSCPrinterHandler(object sender, RefPrinter printer);
        protected event ReactionSCPrinterHandler ReactionSCPrinter;
        private void OnReactionSCPrinter(RefPrinter e)
        {
            if (ReactionSCPrinter != null)
            {
                ReactionSCPrinter(this, e);
                ThereHandler = true;
            }
        } // OnReactionSCPrinter
        
        protected delegate void ReactionSCBoxHandler(object sender, RefBox box);
        protected event ReactionSCBoxHandler ReactionSCBox;
        private void OnReactionSCBox(RefBox e)
        {
            if (ReactionSCBox != null)
            {
                ReactionSCBox(this, e);
                ThereHandler = true;
            }
        } // OnReactionSCBox
        
        protected delegate void ReactionSCItemHandler(object sender, ReactionSCEventArgs e);
        protected event ReactionSCItemHandler ReactionSCItem;
        virtual protected void ReactionSCItemDo(ReactionSCEventArgs e) { ThereHandlerDo = false; } // ReactionSCItemDo
        private void OnReactionSCItem(ReactionSCEventArgs e)
        {
            ThereHandlerDo = true;  //Если функцию ниже никто не наследует, то это значение поменяется на false
            ReactionSCItemDo(e);
            ThereHandler = ThereHandler ? ThereHandler : ThereHandlerDo;    //если уже взведен, то не трогаем, иначе берет значение предыдущего флага
            if (ReactionSCItem != null)
            {
                ReactionSCItem(this, e);
                ThereHandler = true;
            }
        } // OnReactionSCItem

        protected delegate void ReactionPalleteHandler(object sender, string barcode);
        protected event ReactionPalleteHandler ReactionPallete;
        virtual protected void ReactionPalleteDo(string barcode) { ThereHandlerDo = false; } // ReactionSCPalleteDo
        private void OnReactionPallete(string e)
        {
            ThereHandlerDo = true;  //Если функцию ниже никто не наследует, то это значение поменяется на false
            ReactionPalleteDo(e);
            ThereHandler = ThereHandler ? ThereHandler : ThereHandlerDo;    //если уже взведен, то не трогаем, иначе берет значение предыдущего флага
            if (ReactionPallete != null)
            {
                ReactionPallete(this, e);
                ThereHandler = true;
            }
        } // OnReactionPallete

        protected delegate void ReactionDocHandler(object sender, Doc doc);
        protected event ReactionDocHandler ReactionDoc;
        virtual protected void ReactionDocDo(Doc doc) { ThereHandlerDo = false; } // ReactionSCItemDo
        private void OnReactionDoc(Doc e)
        {
            ThereHandlerDo = true;  //Если функцию ниже никто не наследует, то это значение поменяется на false
            ReactionDocDo(e);
            ThereHandler = ThereHandler ? ThereHandler : ThereHandlerDo;    //если уже взведен, то не трогаем, иначе берет значение предыдущего флага
            if (ReactionDoc != null)
            {
                ReactionDoc(this, e);
                ThereHandler = true;
            }
        } // OnReactionDoc

        protected delegate void ReactionKeyHandler(object sender, Keys key);
        protected event ReactionKeyHandler ReactionKey;
        virtual protected void ReactionKeyDo(Keys key) { } // ReactionKeyDo
        private void OnReactionKey(Keys e)
        {
            ReactionKeyDo(e);
            if (ReactionKey != null)
            {
                ReactionKey(this, e);
            }
        } // OnReactionKey

        public ABaseMode(Model ReadySS, ABaseMode CallObj)
        {
            FVoiceOn = false;
            CurrentMode = Mode.None;
            SS = ReadySS;
            if (CallObj != null)
            {
                JumpCounter = CallObj.JumpCounter;
                Const = CallObj.Const;
                FEmployer = CallObj.Employer;
            }
            else
            {
                JumpCounter = 0;
            }
        } // ABaseMode (constructor)

        /// <summary>
        /// Сделана для предотвращения ошибок, типа если забудешь присвоить ResultObject результатом Init()
        /// </summary>
        /// <param name="ToMode"></param>
        /// <returns></returns>
        protected ABaseMode JumpTo(ABaseMode ToMode)
        {
            ResultObject = ToMode.Init();
            FResult = ResultObject.Result;
            return ResultObject;
        } // JumpTo
        protected ABaseMode Negative()
        {
            ResultObject = this;
            FResult = MMResult.Negative;
            return ResultObject;
        } // Negative
        protected ABaseMode Negative(string mess)
        {
            FExcStr = mess;
            return Negative();
        } // Negative
        protected ABaseMode Positive()
        {
            ResultObject = this;
            FResult = MMResult.Positive;
            return ResultObject;
        } // Positive
        protected ABaseMode Positive(string mess)
        {
            FExcStr = mess;
            return Positive();
        } // Positive
        virtual internal ABaseMode ReactionBarcodeBase(string Barcode)
        {
            JumpCounter  = 0;
            FResult      = MMResult.None;
            ResultObject = this;
            ThereHandler = false;
            Dictionary<string, string> dicBarcode = Helper.DisassembleBarcode(Barcode);
            bool IsRef = false;
            bool IsObject = false;  //Это реально существующий объект

            //Это может быть справочник!
            ARef Ref = null;
            if (Barcode.Substring(0, 2) == "25" && dicBarcode["Type"] == "113")
            {
                Ref = new RefEmployer(SS);
                if (Ref.FoundIDD(dicBarcode["IDD"]))
                {
                    IsRef = true;
                    OnReactionSCEmployers(Ref as RefEmployer);
                }
                if (!IsRef)
                {
                    Ref = new RefSection(SS);
                    if (Ref.FoundIDD(dicBarcode["IDD"]))
                    {
                        IsRef = true;
                        OnReactionSCSection(Ref as RefSection);
                    }
                }
                if (!IsRef)
                {
                    Ref = new RefPrinter(SS);
                    if (Ref.FoundIDD(dicBarcode["IDD"]))
                    {
                        IsRef = true;
                        OnReactionSCPrinter(Ref as RefPrinter);
                    }
                }
            }
            else if (dicBarcode["Type"] == "6")
            {
                Ref = new RefBox(SS);
                if (Ref.FoundID(dicBarcode["ID"]))
                {
                    IsRef = true;
                    OnReactionSCBox(Ref as RefBox);
                }
            }
            else if (Barcode.Substring(0, 2) == "26" && dicBarcode["Type"] == "113")
            {
                Doc Doc = new Doc(SS);
                if (Doc.FoundIDD(dicBarcode["IDD"]))
                {
                    IsObject = true;
                    OnReactionDoc(Doc);
                }
            }
            else if (dicBarcode["Type"] == "pallete")
            {
                //ВОТ ЭТА ХУЕТА НИ КАК НЕ УЧИТЫВАЕТСЯ (КАК ОБЪЕКТ, ВСМЫСЛЕ)
                //  ТАК ЧТО ПОТЕНЦИАЛЬНО МОГУТ БЫТЬ ПРОБЛЕМЫ НА ВСЯКИЙ СЛУЧАЙ ВЗВОДИМ ФЛАГ
                IsObject = true;
                //OnReactionPallete(Convert.ToInt32(dicBarcode["pallete"]));
                OnReactionPallete(Barcode);
            }

            IsObject = IsRef ? true : IsObject; //Если это справочник, то полюбому объект
            if (!IsObject)
            {
                //Товаром он может быть при любом раскладе, так что если не определился как объект, то будем искать товар
                Ref = new RefItem(SS);
                if ((Ref as RefItem).FoundBarcode(Barcode))
                {
                    {
                        IsRef = true;
                        OnReactionSCItem(new ReactionSCEventArgs(Barcode, Ref));
                    }
                }
            }
            if (IsRef)
            {
                //Вверху обрабатывали только справочники и в какой-то из них попали
                OnReactionSC(new ReactionSCEventArgs(Barcode, Ref));
            }
            OnReactionBarcode(Barcode); // просто реакция на штрихкод

            if (!ThereHandler)
            {
                //никакой обработчик не сработал! Отобъем исходя из настроек по умолчанию
                FExcStr = "Нет действий с этим ШК в данном режиме!";
                FResult = MMResult.Negative;
            }
            return ResultObject;
        } // ReactionBarcodeBase
        virtual internal ABaseMode ReactionKeyBase(Keys Key)
        {
            JumpCounter = 0;
            ResultObject = this;
            FResult      = MMResult.None;
            OnReactionKey(Key);
            return ResultObject;
        }
        abstract internal ABaseMode Init();
        virtual internal ABaseMode Cancel()
        {
            FExcStr = "No responses (Cancel)!";
            return Negative();
        }// Cancel
        protected bool Logout()
        {
            //Пишем команду logout!
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = SS.ExtendID(FEmployer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = DeviceID.GetDeviceName();
            if (!SS.ExecCommandNoFeedback("Logout", DataMapWrite))
            {
                return false;
            }
            SS.ExecuteWithoutRead("exec IBS_Finalize");
            return true;
        } // Logout
        protected bool Login()
        {
            if (!SS.UpdateProgram())
            {
                return false;
            }
            if (!SS.SynhDateTime())
            {
                return false;
            }
            if (!SS.IBS_Inicialization(Employer))
            {
                return false;
            }
            Dictionary<string, object> DataMapWrite = new Dictionary<string, object>();
            DataMapWrite["Спр.СинхронизацияДанных.ДатаСпрВход1"] = SS.ExtendID(Employer.ID, "Спр.Сотрудники");
            DataMapWrite["Спр.СинхронизацияДанных.ДатаВход1"] = DeviceID.GetDeviceName();
            if (!SS.ExecCommandNoFeedback("Login", DataMapWrite))
            {
                return false;
            }
            return true;
        } // Login
    }
}
