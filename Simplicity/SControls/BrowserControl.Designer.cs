namespace Simplicity.SControls
{
    partial class BrowserControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.sbtnLocation = new Simplicity.SControls.SButton();
            this.list = new Simplicity.SControls.SList();
            this.SuspendLayout();
            //
            // sbtnLocation
            //
            this.sbtnLocation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.sbtnLocation.BType = Simplicity.SControls.SButton.ButtonType.Text;
            this.sbtnLocation.Clickable = true;
            this.sbtnLocation.Location = new System.Drawing.Point(32, 286);
            this.sbtnLocation.Name = "sbtnLocation";
            this.sbtnLocation.Size = new System.Drawing.Size(93, 39);
            this.sbtnLocation.TabIndex = 3;
            this.sbtnLocation.TabStop = false;
            this.sbtnLocation.Click += new System.EventHandler(this.sbtnLocation_Click);
            this.sbtnLocation.KeyDown += new System.Windows.Forms.KeyEventHandler(this.sbtnLocation_KeyDown);
            //
            // list
            //
            this.list.ActivateItemOnClick = true;
            this.list.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(64)))));
            this.list.CurrentItemIndex = -1;
            this.list.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(250)))));
            this.list.ImageResolver = null;
            this.list.Location = new System.Drawing.Point(0, 0);
            this.list.Name = "list";
            this.list.PageItemCount = 10;
            this.list.Size = new System.Drawing.Size(377, 280);
            this.list.TabIndex = 0;
            this.list.KeyDown += new System.Windows.Forms.KeyEventHandler(this.list_KeyDown);
            //
            // BrowserControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.sbtnLocation);
            this.Controls.Add(this.list);
            this.Name = "BrowserControl";
            this.Size = new System.Drawing.Size(380, 358);
            this.Load += new System.EventHandler(this.BrowserControl_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BrowserControl_KeyDown);
            this.Resize += new System.EventHandler(this.BrowserControl_Resize);
            this.ResumeLayout(false);
        }

        #endregion Component Designer generated code

        private SList list;
        private SButton sbtnLocation;
    }
}