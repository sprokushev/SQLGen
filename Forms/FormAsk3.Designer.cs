// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen.Forms
{
    partial class FormAsk3
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
            this.btYes = new System.Windows.Forms.Button();
            this.btNo = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.tbText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btYes
            // 
            this.btYes.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btYes.Location = new System.Drawing.Point(65, 300);
            this.btYes.Margin = new System.Windows.Forms.Padding(4);
            this.btYes.Name = "btYes";
            this.btYes.Size = new System.Drawing.Size(211, 28);
            this.btYes.TabIndex = 1;
            this.btYes.Text = "Да";
            this.btYes.UseVisualStyleBackColor = true;
            this.btYes.Click += new System.EventHandler(this.btYes_Click);
            // 
            // btNo
            // 
            this.btNo.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btNo.Location = new System.Drawing.Point(325, 300);
            this.btNo.Margin = new System.Windows.Forms.Padding(4);
            this.btNo.Name = "btNo";
            this.btNo.Size = new System.Drawing.Size(205, 28);
            this.btNo.TabIndex = 2;
            this.btNo.Text = "Нет";
            this.btNo.UseVisualStyleBackColor = true;
            this.btNo.Click += new System.EventHandler(this.btNo_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(574, 300);
            this.btCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(341, 28);
            this.btCancel.TabIndex = 3;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // tbText
            // 
            this.tbText.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.tbText.Location = new System.Drawing.Point(62, 28);
            this.tbText.Multiline = true;
            this.tbText.Name = "tbText";
            this.tbText.ReadOnly = true;
            this.tbText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbText.Size = new System.Drawing.Size(853, 240);
            this.tbText.TabIndex = 4;
            this.tbText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // FormAsk3
            // 
            this.AcceptButton = this.btYes;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(964, 356);
            this.Controls.Add(this.tbText);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btNo);
            this.Controls.Add(this.btYes);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormAsk3";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ВНИМАНИЕ";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAsk3_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        /// <summary>
        /// btYes
        /// </summary>
        public System.Windows.Forms.Button btYes;
        /// <summary>
        /// btNo
        /// </summary>
        public System.Windows.Forms.Button btNo;
        /// <summary>
        /// btCancel
        /// </summary>
        public System.Windows.Forms.Button btCancel;
        /// <summary>
        /// tbText
        /// </summary>
        public System.Windows.Forms.TextBox tbText;
    }
}