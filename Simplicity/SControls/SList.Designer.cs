namespace Simplicity.SControls
{
    partial class SList
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
            this.panelContent = new System.Windows.Forms.Panel();
            this.panelScroll = new System.Windows.Forms.Panel();
            this.sbtnEnd = new Simplicity.SControls.SButton();
            this.sbtnHome = new Simplicity.SControls.SButton();
            this.sbtnDown = new Simplicity.SControls.SButton();
            this.sbtnUp = new Simplicity.SControls.SButton();
            this.scrollBar = new System.Windows.Forms.VScrollBar();
            this.panelScroll.SuspendLayout();
            this.SuspendLayout();
            //
            // panelContent
            //
            this.panelContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelContent.Location = new System.Drawing.Point(0, 0);
            this.panelContent.Name = "panelContent";
            this.panelContent.Size = new System.Drawing.Size(520, 448);
            this.panelContent.TabIndex = 1;
            //
            // panelScroll
            //
            this.panelScroll.Controls.Add(this.sbtnEnd);
            this.panelScroll.Controls.Add(this.sbtnHome);
            this.panelScroll.Controls.Add(this.sbtnDown);
            this.panelScroll.Controls.Add(this.sbtnUp);
            this.panelScroll.Controls.Add(this.scrollBar);
            this.panelScroll.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelScroll.Location = new System.Drawing.Point(450, 0);
            this.panelScroll.Name = "panelScroll";
            this.panelScroll.Size = new System.Drawing.Size(70, 448);
            this.panelScroll.TabIndex = 2;
            //
            // sbtnEnd
            //
            this.sbtnEnd.BType = Simplicity.SControls.SButton.ButtonType.End;
            this.sbtnEnd.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.sbtnEnd.Location = new System.Drawing.Point(0, 384);
            this.sbtnEnd.Name = "sbtnEnd";
            this.sbtnEnd.Size = new System.Drawing.Size(70, 64);
            this.sbtnEnd.TabIndex = 5;
            this.sbtnEnd.TabStop = false;
            this.sbtnEnd.Click += new System.EventHandler(this.sbtnEnd_Click);
            this.sbtnEnd.KeyDown += new System.Windows.Forms.KeyEventHandler(this.item_KeyDown);
            //
            // sbtnHome
            //
            this.sbtnHome.BType = Simplicity.SControls.SButton.ButtonType.Home;
            this.sbtnHome.Dock = System.Windows.Forms.DockStyle.Top;
            this.sbtnHome.Location = new System.Drawing.Point(0, 0);
            this.sbtnHome.Name = "sbtnHome";
            this.sbtnHome.Size = new System.Drawing.Size(70, 64);
            this.sbtnHome.TabIndex = 4;
            this.sbtnHome.TabStop = false;
            this.sbtnHome.Click += new System.EventHandler(this.sbtnHome_Click);
            this.sbtnHome.KeyDown += new System.Windows.Forms.KeyEventHandler(this.item_KeyDown);
            //
            // sbtnDown
            //
            this.sbtnDown.BType = Simplicity.SControls.SButton.ButtonType.Down;
            this.sbtnDown.Location = new System.Drawing.Point(0, 311);
            this.sbtnDown.Name = "sbtnDown";
            this.sbtnDown.Size = new System.Drawing.Size(64, 64);
            this.sbtnDown.TabIndex = 3;
            this.sbtnDown.TabStop = false;
            this.sbtnDown.Click += new System.EventHandler(this.sbtnDown_Click);
            this.sbtnDown.KeyDown += new System.Windows.Forms.KeyEventHandler(this.item_KeyDown);
            //
            // sbtnUp
            //
            this.sbtnUp.BType = Simplicity.SControls.SButton.ButtonType.Up;
            this.sbtnUp.Location = new System.Drawing.Point(1, 73);
            this.sbtnUp.Name = "sbtnUp";
            this.sbtnUp.Size = new System.Drawing.Size(64, 64);
            this.sbtnUp.TabIndex = 2;
            this.sbtnUp.TabStop = false;
            this.sbtnUp.Click += new System.EventHandler(this.sbtnUp_Click);
            this.sbtnUp.KeyDown += new System.Windows.Forms.KeyEventHandler(this.item_KeyDown);
            //
            // scrollBar
            //
            this.scrollBar.LargeChange = 5;
            this.scrollBar.Location = new System.Drawing.Point(0, 140);
            this.scrollBar.Name = "scrollBar";
            this.scrollBar.Size = new System.Drawing.Size(64, 168);
            this.scrollBar.TabIndex = 1;
            this.scrollBar.ValueChanged += new System.EventHandler(this.scrollBar_ValueChanged);
            this.scrollBar.KeyDown += new System.Windows.Forms.KeyEventHandler(this.item_KeyDown);
            //
            // SList
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelScroll);
            this.Controls.Add(this.panelContent);
            this.Name = "SList";
            this.Size = new System.Drawing.Size(520, 448);
            this.Load += new System.EventHandler(this.SList_Load);
            this.SizeChanged += new System.EventHandler(this.SList_SizeChanged);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SList_KeyDown);
            this.panelScroll.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        #endregion Component Designer generated code

        private System.Windows.Forms.Panel panelContent;
        private System.Windows.Forms.Panel panelScroll;
        private System.Windows.Forms.VScrollBar scrollBar;
        private SButton sbtnUp;
        private SButton sbtnDown;
        private SButton sbtnHome;
        private SButton sbtnEnd;
    }
}