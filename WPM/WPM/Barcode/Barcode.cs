using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    class Barcode
    {
        private Form FOwnerForm;
        private KeyEventArgs[] KeyBuffer;       //клавиатурный буфер FIFO(используется для возможности перехватывать ввод сосканера в разрыв клавиатуры)
        private TextBox WasTB;                  //Такой контрол был в фокусе на момент первой записи в буфер
        int WasCursor;                  //Позиция курсора в TextBox
        int WasSelected;                //сколько было выделенно
        string WasTBText;               //а это что в нем было записано
        int KeyBufferPointer;           //указывает на текущую позицию для записи в клавиатурный буфер
        int MaxBarcodeLength;           //в каком варианте посылается штрихкод (15 или 13 - символов)
        Timer BarcodeTimer;

        public delegate void BarcodeReadEventHandler (object sender, BarcodeReadEventArgs e);
        public event BarcodeReadEventHandler BarcodeRead;
        public delegate void HotKeyEventHandler (object sender, KeyEventArgs e);
        public event HotKeyEventHandler HotKeyEvent;

        protected virtual void OnBarcodeRead(BarcodeReadEventArgs e)
        {
            if (BarcodeRead != null)
            {
                BarcodeRead(this, e);
            }
        }
        protected virtual void OnHotKey(KeyEventArgs e)
        {
            if (HotKeyEvent != null)
            {
                HotKeyEvent(this, e);
            }
        }
        public Barcode(Form OwnerForm, int Sensitivity)
        {
            FOwnerForm      = OwnerForm;
            BarcodeTimer = new Timer();
            BarcodeTimer.Interval = Sensitivity;
            BarcodeTimer.Tick += new EventHandler(BarcodeTimer_Tick);
            MaxBarcodeLength    = 30;
            KeyBuffer           = new KeyEventArgs[MaxBarcodeLength];
            WasTB               = null;
            KeyBufferPointer    = 0;
        }

        void BarcodeTimer_Tick(object sender, EventArgs e)
        {
            BarcodeTimer.Enabled     = false;
            int BarcodeLength   = 0;
            for (int i = 0; i < MaxBarcodeLength; ++i)
            {
                if (KeyBuffer[i] == null)
                {
                    BarcodeLength = i;
                    break;
                }
            }
            if (BarcodeLength > 7)
            {
                //проверим все ли оставшиеся - цифры
                string Barcode = "";
                int OnlyNumberLength = 0;   //Для определения количества цифр в ШК (чистит от них текстовые поля)
                for (int i = 0; i < BarcodeLength; ++i)
                {

                    if ((KeyBuffer[i].KeyCode.GetHashCode() >= 48 && KeyBuffer[i].KeyCode.GetHashCode() <= 57)
                        || (KeyBuffer[i].KeyCode.GetHashCode() >= 65 && KeyBuffer[i].KeyCode.GetHashCode() <= 90))
                    {
                        //В ШК пишем только буквы и цифры, весь остальной адовый ужос - пропускаем
                        Barcode += (Char)KeyBuffer[i].KeyCode;
                        if (KeyBuffer[i].KeyCode.GetHashCode() >= 48 && KeyBuffer[i].KeyCode.GetHashCode() <= 57)
                        {
                            ++OnlyNumberLength;
                        }
                    }
                }
                //Если фокус стоял на текстовом поле, то нам не удасться избежать его засерания,
                //поэтому код ниже очищает такое говно, правда очищает только от цифр
                //Если же поле может содержать и цифры и буквы, то будет работать некорректно
                TextBox FTB = GetFocused() as TextBox;
                if (FTB != null && WasTB.Name == FTB.Name)
                {
                    //был один и тот же элемент в фокусе
                    FTB.Text = WasTBText;
                    FTB.Select(WasCursor, WasSelected);
                }
                //инициализируем ввод ШК, обнуляем буфер и сваливаем
                if (Barcode.Length >= 2)
                {
                    OnBarcodeRead(new BarcodeReadEventArgs(Barcode));
                }
                for (int j = 0; j < BarcodeLength; j++)
                {
                    KeyBuffer[j] = null;
                }
                
                WasTB = null;
                KeyBufferPointer = 0;
                return;
            }

            //обрабатываем как нажатие
            for (int i = 0; i < MaxBarcodeLength; ++i)
            {
                if (KeyBuffer[i] == null)
                {
                    break;
                }
                OnHotKey(KeyBuffer[i]);
                KeyBuffer[i] = null;
            }
            WasTB = null;
            KeyBufferPointer = 0;
        }

        private Control GetFocused()
        {
            //Проверим на форме
            foreach (Control Ctrl in FOwnerForm.Controls)
            {
                if (Ctrl.Focused)
                {
                    return Ctrl;
                }
            }
            foreach(Control Ctrl in FOwnerForm.Controls)
            {
                if (Ctrl is ExPanel)
                {
                    return (Ctrl as ExPanel).GetFocused();
                }
            }
            return null;
        }
        public void KeyDown(KeyEventArgs e)
        {
            if (KeyBufferPointer == MaxBarcodeLength)
            {
                return; //буфер переполнен, ничего запоминать не будем
            }
            if (KeyBufferPointer == 0)
            {
                WasTB = GetFocused() as TextBox;
                if (WasTB != null)
                {
                    WasTBText = WasTB.Text;
                    WasCursor = WasTB.SelectionStart;
                    WasSelected = WasTB.SelectionLength;
                }
            }
            KeyBuffer[KeyBufferPointer] = e;
            KeyBufferPointer += 1;
            BarcodeTimer.Enabled = true;
        }

    }
}
