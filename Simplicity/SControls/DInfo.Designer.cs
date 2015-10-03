namespace Simplicity.SControls
{
    partial class DInfo
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
            this.components = new System.ComponentModel.Container();
            this.lblSize = new System.Windows.Forms.Label();
            this.lblText = new System.Windows.Forms.Label();
            this.timerTime = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // lblSize
            // 
            this.lblSize.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.lblSize.Location = new System.Drawing.Point(0, 308);
            this.lblSize.Name = "lblSize";
            this.lblSize.Size = new System.Drawing.Size(375, 53);
            this.lblSize.TabIndex = 1;
            this.lblSize.Click += new System.EventHandler(this.lblSize_Click);
            // 
            // lblText
            // 
            this.lblText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblText.Location = new System.Drawing.Point(0, 0);
            this.lblText.Name = "lblText";
            this.lblText.Size = new System.Drawing.Size(375, 308);
            this.lblText.TabIndex = 2;
            this.lblText.Click += new System.EventHandler(this.lblText_Click);
            // 
            // timerTime
            // 
            this.timerTime.Interval = 500;
            this.timerTime.Tick += new System.EventHandler(this.timerTime_Tick);
            // 
            // DInfo
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(375, 361);
            this.Controls.Add(this.lblText);
            this.Controls.Add(this.lblSize);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DInfo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "DInfo";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DInfo_FormClosing);
            this.Load += new System.EventHandler(this.DInfo_Load);
            this.Resize += new System.EventHandler(this.DInfo_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblSize;
        private System.Windows.Forms.Label lblText;
        private System.Windows.Forms.Timer timerTime;
    }
}