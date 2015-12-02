namespace SecureCalendarServer
{
    partial class ServerForm
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
            this.logForm = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // logForm
            // 
            this.logForm.BackColor = System.Drawing.SystemColors.MenuText;
            this.logForm.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logForm.ForeColor = System.Drawing.SystemColors.Window;
            this.logForm.Location = new System.Drawing.Point(0, 0);
            this.logForm.Multiline = true;
            this.logForm.Name = "logForm";
            this.logForm.ReadOnly = true;
            this.logForm.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logForm.Size = new System.Drawing.Size(816, 347);
            this.logForm.TabIndex = 0;
            this.logForm.TextChanged += new System.EventHandler(this.logForm_TextChanged);
            // 
            // ServerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(816, 347);
            this.Controls.Add(this.logForm);
            this.Name = "ServerForm";
            this.Text = "Secure Calendar Server";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox logForm;
    }
}

