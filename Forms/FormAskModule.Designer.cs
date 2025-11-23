// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen.Forms
{
    partial class FormAskModule
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
            this.btnPRMD = new System.Windows.Forms.Button();
            this.btnRPMS = new System.Windows.Forms.Button();
            this.btnSMP = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnBI = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnPRMD
            // 
            this.btnPRMD.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnPRMD.Location = new System.Drawing.Point(13, 64);
            this.btnPRMD.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnPRMD.Name = "btnPRMD";
            this.btnPRMD.Size = new System.Drawing.Size(210, 28);
            this.btnPRMD.TabIndex = 1;
            this.btnPRMD.Text = "PRMD - ЕЦП\\Промед";
            this.btnPRMD.UseVisualStyleBackColor = true;
            this.btnPRMD.Click += new System.EventHandler(this.btnPRMD_Click);
            // 
            // btnRPMS
            // 
            this.btnRPMS.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btnRPMS.Location = new System.Drawing.Point(241, 64);
            this.btnRPMS.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnRPMS.Name = "btnRPMS";
            this.btnRPMS.Size = new System.Drawing.Size(210, 28);
            this.btnRPMS.TabIndex = 2;
            this.btnRPMS.Text = "RPMS - Портал";
            this.btnRPMS.UseVisualStyleBackColor = true;
            this.btnRPMS.Click += new System.EventHandler(this.btnRPMS_Click);
            // 
            // btnSMP
            // 
            this.btnSMP.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btnSMP.Location = new System.Drawing.Point(472, 64);
            this.btnSMP.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnSMP.Name = "btnSMP";
            this.btnSMP.Size = new System.Drawing.Size(210, 28);
            this.btnSMP.TabIndex = 3;
            this.btnSMP.Text = "SMP2 - СМП 2 версии";
            this.btnSMP.UseVisualStyleBackColor = true;
            this.btnSMP.Click += new System.EventHandler(this.btnSMP_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(353, 124);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(229, 28);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Отмена";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnBI
            // 
            this.btnBI.DialogResult = System.Windows.Forms.DialogResult.Retry;
            this.btnBI.Location = new System.Drawing.Point(708, 64);
            this.btnBI.Margin = new System.Windows.Forms.Padding(4);
            this.btnBI.Name = "btnBI";
            this.btnBI.Size = new System.Drawing.Size(210, 28);
            this.btnBI.TabIndex = 3;
            this.btnBI.Text = "BI";
            this.btnBI.UseVisualStyleBackColor = true;
            this.btnBI.Click += new System.EventHandler(this.btnBI_Click);
            // 
            // FormAskModule
            // 
            this.AcceptButton = this.btnPRMD;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(964, 225);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnBI);
            this.Controls.Add(this.btnSMP);
            this.Controls.Add(this.btnRPMS);
            this.Controls.Add(this.btnPRMD);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormAskModule";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Какой модуль (префикс) версии?";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAskModule_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnPRMD;
        private System.Windows.Forms.Button btnRPMS;
        private System.Windows.Forms.Button btnSMP;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnBI;
    }
}