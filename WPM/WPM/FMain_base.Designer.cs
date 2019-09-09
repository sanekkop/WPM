namespace WPM
{
    partial class FMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FMain));
            this.lblDownDescr = new System.Windows.Forms.Label();
            this.lblAction = new System.Windows.Forms.Label();
            this.lblState = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.TimerFind = new System.Windows.Forms.Timer();
            this.TimerDuty = new System.Windows.Forms.Timer();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.debuger_template = new System.Windows.Forms.Timer();
            this.lblDebug = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lblDownDescr
            // 
            this.lblDownDescr.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblDownDescr.Location = new System.Drawing.Point(0, 247);
            this.lblDownDescr.Name = "lblDownDescr";
            this.lblDownDescr.Size = new System.Drawing.Size(208, 19);
            this.lblDownDescr.Text = "Down description";
            // 
            // lblAction
            // 
            this.lblAction.BackColor = System.Drawing.Color.White;
            this.lblAction.Font = new System.Drawing.Font("Tahoma", 14F, System.Drawing.FontStyle.Bold);
            this.lblAction.ForeColor = System.Drawing.Color.Blue;
            this.lblAction.Location = new System.Drawing.Point(0, 200);
            this.lblAction.Name = "lblAction";
            this.lblAction.Size = new System.Drawing.Size(318, 69);
            this.lblAction.Text = "i\'m a test text. what is love? tuc tuc tuc. Everyday im shuffling and more and mo" +
                "re and more and more and more and more and more and more and m ore and monre nor" +
                "e more more more";
            this.lblAction.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblState
            // 
            this.lblState.Font = new System.Drawing.Font("Tahoma", 10F, System.Drawing.FontStyle.Bold);
            this.lblState.Location = new System.Drawing.Point(0, 0);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(318, 15);
            this.lblState.Text = "State";
            this.lblState.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lblName
            // 
            this.lblName.ForeColor = System.Drawing.Color.DimGray;
            this.lblName.Location = new System.Drawing.Point(296, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(22, 15);
            this.lblName.Text = "88";
            this.lblName.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // TimerFind
            // 
            this.TimerFind.Tick += new System.EventHandler(this.TimerFind_Tick);
            // 
            // TimerDuty
            // 
            this.TimerDuty.Tick += new System.EventHandler(this.TimerDuty_Tick);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(320, 185);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(170, 212);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(72, 20);
            this.button1.TabIndex = 5;
            this.button1.Text = "button1";
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // debuger_template
            // 
            this.debuger_template.Enabled = true;
            this.debuger_template.Interval = 1000;
            this.debuger_template.Tick += new System.EventHandler(this.debuger_template_Tick);
            // 
            // lblDebug
            // 
            this.lblDebug.BackColor = System.Drawing.Color.White;
            this.lblDebug.Font = new System.Drawing.Font("Tahoma", 7F, System.Drawing.FontStyle.Regular);
            this.lblDebug.Location = new System.Drawing.Point(204, 245);
            this.lblDebug.Name = "lblDebug";
            this.lblDebug.Size = new System.Drawing.Size(111, 21);
            // 
            // FMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(318, 269);
            this.Controls.Add(this.lblDebug);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.lblDownDescr);
            this.Controls.Add(this.lblAction);
            this.Controls.Add(this.lblState);
            this.KeyPreview = true;
            this.Name = "FMain";
            this.Load += new System.EventHandler(this.FMainOnLoad);
            this.Closed += new System.EventHandler(this.FMainOnClosed);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FMainOnKeyDown);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblDownDescr;
        private System.Windows.Forms.Label lblAction;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.Timer TimerFind;
        private System.Windows.Forms.Timer TimerDuty;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Timer debuger_template;
        private System.Windows.Forms.Label lblDebug;
    }
}

