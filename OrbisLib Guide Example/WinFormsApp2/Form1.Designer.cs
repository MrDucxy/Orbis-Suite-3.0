namespace WinFormsApp2
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.targetListBox = new System.Windows.Forms.ListBox();
            this.refreshTargetsBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // targetListBox
            // 
            this.targetListBox.FormattingEnabled = true;
            this.targetListBox.ItemHeight = 25;
            this.targetListBox.Location = new System.Drawing.Point(12, 12);
            this.targetListBox.Name = "targetListBox";
            this.targetListBox.Size = new System.Drawing.Size(180, 179);
            this.targetListBox.TabIndex = 0;
            this.targetListBox.SelectedIndexChanged += new System.EventHandler(this.targetListBox_SelectedIndexChanged);
            // 
            // refreshTargetsBtn
            // 
            this.refreshTargetsBtn.Location = new System.Drawing.Point(12, 197);
            this.refreshTargetsBtn.Name = "refreshTargetsBtn";
            this.refreshTargetsBtn.Size = new System.Drawing.Size(180, 34);
            this.refreshTargetsBtn.TabIndex = 1;
            this.refreshTargetsBtn.Text = "Refresh";
            this.refreshTargetsBtn.UseVisualStyleBackColor = true;
            this.refreshTargetsBtn.Click += new System.EventHandler(this.refreshTargetsBtn_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(788, 655);
            this.Controls.Add(this.refreshTargetsBtn);
            this.Controls.Add(this.targetListBox);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private ListBox targetListBox;
        private Button refreshTargetsBtn;
    }
}