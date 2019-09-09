using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace WPM
{
    /// <summary>
    /// Extended class Panel, may replace controls inside panel, scroll similarity
    /// </summary>
    class ExPanel : Panel
    {
        private List<string> FStaticControl;

        public List<string> StaticControl
        {
            get
            {
                return FStaticControl;
            }
            set
            {
                FStaticControl = value;
            }
        }
        public ExPanel()
        {
            FStaticControl = new List<string>();
        }

        /// <summary>
        /// Move all controls on specified difference
        /// </summary>
        /// <param name="dX">difference on X axis</param>
        /// <param name="dY">difference on Y axis</param>
        public void MoveAllControls(int dX, int dY)
        {
            foreach (Control Ctrl in Controls)
            {
                Ctrl.Left = Ctrl.Left + dX;
                Ctrl.Top  = Ctrl.Top  + dY;
            }
        }
        /// <summary>
        /// Move all controls on specified difference
        /// </summary>
        /// <param name="dX">difference on X axis</param>
        /// <param name="dY">difference on Y axis</param>
        public void MoveControls(int dX, int dY)
        {
            foreach (Control Ctrl in Controls)
            {
                if (StaticControl.FindIndex(
                    delegate (string str)
                    {
                        return str == Ctrl.Name;
                    }
                    ) == -1)
                {
                    Ctrl.Left = Ctrl.Left + dX;
                    Ctrl.Top  = Ctrl.Top  + dY;
                }
            }
        }
        public void Sweep(int route)
        {
            int curX = 0;
            int dX = (int)(Width/5*route);
            while (curX*route < Width)
            {
                curX += dX;
                if (curX*route > Width)
                    MoveControls(dX - curX + Width*route, 0);
                else
                    MoveControls(dX, 0);
                Refresh();
            }
        }
        /// <summary>
        /// Get object Control by name
        /// </summary>
        /// <param name="ByName">control name</param>
        /// <returns>itself control or null, if control with the same name not found</returns>
        public Control GetControlByName(string ByName)
        {
            foreach (Control Ctrl in Controls)
            {
                if (Ctrl.Name == ByName)
                {
                    return Ctrl;
                }
            }
            return null;
        }
        /// <summary>
        /// Get object Label by name
        /// </summary>
        /// <param name="ByName">label name</param>
        /// <returns>itself label, null (if control with the same name not found) or break if control with this name - not label</returns>
        public Label GetLabelByName(string ByName)
        {
            return GetControlByName(ByName) as Label;
        }
        public DataGrid GetDataGridByName(string ByName)
        {
            return GetControlByName(ByName) as DataGrid;
        }
        public TextBox GetTextBoxByName(string ByName)
        {
            return GetControlByName(ByName) as TextBox;
        }
        public PictureBox GetPictureBoxByName(string ByName)
        {
            return GetControlByName(ByName) as PictureBox;
        }
        /// <summary>
        /// Get focused control
        /// </summary>
        /// <returns>focused control</returns>
        public Control GetFocused()
        {
            foreach (Control Ctrl in Controls)
            {
                if (Ctrl.Focused)
                {
                    return Ctrl;
                }
            }
            return null;
        }
    }//class ExPanel
}
