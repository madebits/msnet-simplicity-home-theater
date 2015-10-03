namespace Simplicity.SControls
{
    partial class DPathDetails
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
            this.list = new Simplicity.SControls.SList();
            this.SuspendLayout();
            // 
            // list
            // 
            this.list.ActivateItemOnClick = false;
            this.list.ActivateOnSpaceKey = false;
            this.list.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.list.CurrentItemIndex = -1;
            this.list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(245)))), ((int)(((byte)(245)))));
            this.list.ImageResolver = null;
            this.list.Location = new System.Drawing.Point(0, 0);
            this.list.Margin = new System.Windows.Forms.Padding(2);
            this.list.Name = "list";
            this.list.PageItemCount = 10;
            this.list.Size = new System.Drawing.Size(297, 275);
            this.list.TabIndex = 0;
            this.list.ItemActivated += new Simplicity.SControls.SList.ItemActivatedHandler(this.list_ItemActivated);
            // 
            // DPathDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(297, 275);
            this.Controls.Add(this.list);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.KeyPreview = true;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DPathDetails";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Path";
            this.Load += new System.EventHandler(this.DPathDetails_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private SList list;
    }
}