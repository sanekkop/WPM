using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Collections;
using System.Data;
using System.IO;
using System.Threading;

namespace WPM
{
    public partial class FMain : Form
    {
        //Эти переменные нужны для обхода неведомого ебанного глюка новой прошивки моторолы
        Font FontTahoma6Regular;
        Font FontTahoma6Bold;
        Font FontTahoma7Regular;
        Font FontTahoma7Bold;
        Font FontTahoma8Regular;
        Font FontTahoma8Bold;
        Font FontTahoma9Regular;
        Font FontTahoma9Bold;
        Font FontTahoma10Regular;
        Font FontTahoma10Bold;
        Font FontTahoma11Regular;
        Font FontTahoma12Regular;
        Font FontTahoma12Bold;
        Font FontTahoma13Regular;
        Font FontTahoma14Regular;
        Font FontTahoma14Bold;
        Font FontTahoma16Regular;
        Font FontTahoma16Bold;
        Font FontTahoma18Bold;
        Font FontTahoma20Bold;
        private void InicializeFont()
        {
            FontTahoma6Regular = new Font("Tahoma", 6, FontStyle.Regular);
            FontTahoma6Bold = new Font("Tahoma", 6, FontStyle.Bold);
            FontTahoma7Regular = new Font("Tahoma", 7, FontStyle.Regular);
            FontTahoma7Bold = new Font("Tahoma", 7, FontStyle.Bold);
            FontTahoma8Regular = new Font("Tahoma", 8, FontStyle.Regular);
            FontTahoma8Bold = new Font("Tahoma", 8, FontStyle.Bold);
            FontTahoma9Regular = new Font("Tahoma", 9, FontStyle.Regular);
            FontTahoma9Bold = new Font("Tahoma", 9, FontStyle.Bold);
            FontTahoma10Regular = new Font("Tahoma", 10, FontStyle.Regular);
            FontTahoma10Bold = new Font("Tahoma", 10, FontStyle.Bold);
            FontTahoma11Regular = new Font("Tahoma", 11, FontStyle.Regular);
            FontTahoma12Regular = new Font("Tahoma", 12, FontStyle.Regular);
            FontTahoma12Bold = new Font("Tahoma", 12, FontStyle.Bold);
            FontTahoma13Regular = new Font("Tahoma", 13, FontStyle.Regular);
            FontTahoma14Regular = new Font("Tahoma", 14, FontStyle.Regular);
            FontTahoma14Bold = new Font("Tahoma", 14, FontStyle.Bold);
            FontTahoma16Regular = new Font("Tahoma", 16, FontStyle.Regular);
            FontTahoma16Bold = new Font("Tahoma", 16, FontStyle.Bold);
            FontTahoma18Bold = new Font("Tahoma", 18, FontStyle.Bold);
            FontTahoma20Bold = new Font("Tahoma", 20, FontStyle.Bold);
        } // InicializeFont
    }
}
