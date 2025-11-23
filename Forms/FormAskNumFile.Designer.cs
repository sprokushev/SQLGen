// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormAskNumFile
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
            this.tbNumFile = new System.Windows.Forms.TextBox();
            this.btOk = new System.Windows.Forms.Button();
            this.lbNumFile = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbNumFile
            // 
            this.tbNumFile.Location = new System.Drawing.Point(86, 21);
            this.tbNumFile.Name = "tbNumFile";
            this.tbNumFile.Size = new System.Drawing.Size(169, 20);
            this.tbNumFile.TabIndex = 0;
            this.tbNumFile.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbNumFile_KeyDown);
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(280, 14);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(75, 33);
            this.btOk.TabIndex = 1;
            this.btOk.Text = "Ok";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // lbNumFile
            // 
            this.lbNumFile.AutoSize = true;
            this.lbNumFile.Location = new System.Drawing.Point(13, 24);
            this.lbNumFile.Name = "lbNumFile";
            this.lbNumFile.Size = new System.Drawing.Size(49, 15);
            this.lbNumFile.TabIndex = 2;
            this.lbNumFile.Text = "Номер:";
            // 
            // FormAskNumFile
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 69);
            this.Controls.Add(this.lbNumFile);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.tbNumFile);
            this.Name = "FormAskNumFile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Номер файла ?";
            this.TopMost = true;
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAskNumFile_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btOk;
        /// <summary>
        /// tbNumFile
        /// </summary>
        public System.Windows.Forms.TextBox tbNumFile;
        /// <summary>
        /// lbNumFile
        /// </summary>
        public System.Windows.Forms.Label lbNumFile;
    }
}