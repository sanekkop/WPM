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
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace WPM
{
    public partial class FMain : Form
    {
        //private void xyu()
        //{
        //    while(true)
        //    {
        //        Helper.Pause(500);
        //        SS.xyu();
        //    }
        //}

        //Debug procedure!
        private void button1_Click(object sender, EventArgs e)
        {

            //SS.xyu();
            //MessageBox.Show(SS.LabelControlCC);
            //return;
            //int res = SS.DebugGetSyng("Спр.Принтеры");

            //MessageBox.Show(SS.DebugHowLong.ToString() + " - " + res.ToString());
            //return;
            //GT = new Thread(xyu);
            //GT.Priority = ThreadPriority.Lowest;
            //GT.Start();
            #region comment
            //uint curr = 10000;
            //for (int i=0; i < 200; i++)
            //{
            //    APIManager.MessageBeep(curr);
            //    curr += 5000;
            //}
            //for (int i=0; i < 200; i++)
            //{
            //    APIManager.MessageBeep(curr);
            //    curr -= 5000;
            //}
            //return;
            ////StreamReader streamReader = new StreamReader("N:\\int9999001rab\\syslog\\SettingsSCS.txt");
            ////StreamReader streamReader = new StreamReader("\\\\cluster\\configuration\\int9999001rab\\1.txt");
            ////StreamReader streamReader = new StreamReader("\\\\cluster\\configuration\\1.txt");
            //string str = "";

            //while (!streamReader.EndOfStream)
            //{
            //    str += streamReader.ReadLine();
            //}
            //MessageBox.Show(str);
            //return;
            //MessageBox.Show(CurrWidth.ToString());
            //return;
            //List<string> tmpList = new List<string>();
            //tmpList.Add("xyu");
            //tmpList.Add("pizda");
            //tmpList.Add("zhopa");
            //tmpList.Remove("xxx");
            //tmpList.Remove("pizda");
            //MessageBox.Show(tmpList.Count.ToString() + " - " + tmpList[tmpList.Count - 1]);
            //tmpList.Add("pizda");
            //MessageBox.Show(tmpList.Count.ToString() + " - " + tmpList[tmpList.Count - 1]);
            //return;
            //if (dataGrid1.Visible)
            //{
            //    dataGrid1.Visible = false;
            //    return;
            //}
            //dataGrid1.DataSource = SS.Units;
            //dataGrid1.Visible = true;
            //dataGrid1.BringToFront();
            #endregion
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GT.Abort();
        }
        private void TimerDuty_Tick(object sender, EventArgs e)
        {
            if (OldText != null)
            {
                if (OldText != SS.LabelControlCC)
                {
                    OldText = SS.LabelControlCC;
                    lblAction.Text = OldText;
                    BadVoice();
                }
            }
            else
            {
                OldText = "";
                SS.LabelControlCC = "";
            }
        }

        private void FMainOnClosed(object sender, EventArgs e)
        {
            if (GT != null)
            {
                GT.Abort();
            }
        }

        protected delegate void MyDelegate(string pi, string part2);
        //private MyDelegate DD;
        private void xyu()
        //private void xyu(string part1, string part2)
        {
            MessageBox.Show("xyu ");
            //MessageBox.Show("xyu " + part1 + part2);
        }
        private void pizda(string xyu, string part2)
        {
            MessageBox.Show("pizda " + xyu + part2);
        }
        private void button1_Click_2(object sender, EventArgs e)
        {
            foreach (Control Ctrl in pnlCurrent.Controls)
            {
                if (Ctrl.GetType() == typeof(Label))
                {
                    MessageBox.Show((Ctrl as Label).Text);
                }
            }
                
            return;
            //MessageBox.Show(Mode.Acceptance.ToString());
            //return;
            ////MessageBox.Show(typeof(ChoiseWork).Name);
            //string nameViewMethod = (SS.MM.GetType()).Name + "_view";
            //MessageBox.Show(nameViewMethod);
            //return;
            //List<object> xxx = new List<object>();
            //MessageBox.Show(GetType().GetMethod("xyu", BindingFlags.NonPublic | BindingFlags.Instance).ToString());
            ////object[] obj = new object[0]();
            //GetType().GetMethod("xyu", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(this, xxx.ToArray());
            //return;
            //DD = xyu;
            //Invoke(DD, new object[]{"zzz", "asdf"});
            //return;
            //string columns;
            //if (!SS.GetColumns("DT$АдресПеремещение", out columns))
            //{
            //    MessageBox.Show("error!");
            //    return;
            //}
            //columns = columns.Replace("IDDOC,", "");    //уберем iddoc, его мы перезаписывать не будем
            //MessageBox.Show(columns);
            //return;
            //Delegate DD;

        }

        private void debuger_template_Tick(object sender, EventArgs e)
        {
            try
            {
                lblDebug.Text = SS.CurrentMode.ToString() + " " + (SS.MM.GetType()).Name;
            }
            catch { }
        }
    }
}
