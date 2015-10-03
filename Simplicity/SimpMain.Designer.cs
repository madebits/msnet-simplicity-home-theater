namespace Simplicity
{
    partial class SimpMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimpMain));
            this.browser = new Simplicity.SControls.BrowserControl();
            this.SuspendLayout();
            // 
            // browser
            // 
            this.browser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.browser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.browser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.browser.Location = new System.Drawing.Point(0, 0);
            this.browser.Name = "browser";
            this.browser.Size = new System.Drawing.Size(605, 556);
            this.browser.TabIndex = 0;
            // 
            // SimpMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(605, 556);
            this.Controls.Add(this.browser);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimizeBox = false;
            this.Name = "SimpMain";
            this.Text = "Simplicity File Browser for Home Theater";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SimpMain_FormClosing);
            this.Load += new System.EventHandler(this.Simplicity_Load);
            this.ResumeLayout(false);

        }

        #endregion Windows Form Designer generated code

        private SControls.BrowserControl browser;

    }
}