namespace Simplicity.SControls
{
    partial class SListItem
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
            this.components = new System.ComponentModel.Container();
            this.timerMarque = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // timerMarque
            // 
            this.timerMarque.Interval = 250;
            this.timerMarque.Tick += new System.EventHandler(this.timerMarque_Tick);
            // 
            // SListItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "SListItem";
            this.Load += new System.EventHandler(this.SListItem_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.SListItem_Paint);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SListItem_MouseDown);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SListItem_MouseUp);
            this.ResumeLayout(false);

        }

        #endregion Component Designer generated code

        private System.Windows.Forms.Timer timerMarque;
    }
}