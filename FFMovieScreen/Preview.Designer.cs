namespace MediaPreview
{
    partial class Preview
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
            mfiles.Dispose();
            ClearPainBuffers();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Preview));
            this.timerSlide = new System.Windows.Forms.Timer(this.components);
            this.timerMouse = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // timerSlide
            // 
            this.timerSlide.Interval = 6000;
            this.timerSlide.Tick += new System.EventHandler(this.timerSlide_Tick);
            // 
            // timerMouse
            // 
            this.timerMouse.Enabled = true;
            this.timerMouse.Interval = 5000;
            this.timerMouse.Tick += new System.EventHandler(this.timerMouse_Tick);
            // 
            // Preview
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Cursor = System.Windows.Forms.Cursors.NoMove2D;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimizeBox = false;
            this.Name = "Preview";
            this.Text = "Media Preview";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Preview_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Preview_FormClosed);
            this.Load += new System.EventHandler(this.Preview_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Preview_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Preview_DragEnter);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Preview_MouseDown);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Preview_MouseMove);
            this.Resize += new System.EventHandler(this.Preview_Resize);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timerSlide;
        private System.Windows.Forms.Timer timerMouse;



    }
}

