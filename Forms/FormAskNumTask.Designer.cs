// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormAskNumTask
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
            this.tbNumTask = new System.Windows.Forms.TextBox();
            this.btOk = new System.Windows.Forms.Button();
            this.lbTitle = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbNumTask
            // 
            this.tbNumTask.AcceptsReturn = true;
            this.tbNumTask.AcceptsTab = true;
            this.tbNumTask.Location = new System.Drawing.Point(16, 32);
            this.tbNumTask.Multiline = true;
            this.tbNumTask.Name = "tbNumTask";
            this.tbNumTask.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbNumTask.Size = new System.Drawing.Size(376, 310);
            this.tbNumTask.TabIndex = 0;
            this.tbNumTask.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbNumTask_KeyDown);
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(139, 361);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(111, 32);
            this.btOk.TabIndex = 1;
            this.btOk.Text = "Ok";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Location = new System.Drawing.Point(14, 7);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(92, 15);
            this.lbTitle.TabIndex = 2;
            this.lbTitle.Text = "Номера задач:";
            // 
            // FormAskNumTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(418, 416);
            this.Controls.Add(this.lbTitle);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.tbNumTask);
            this.Name = "FormAskNumTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Список задач для добавления";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAskNumTask_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btOk;
        /// <summary>
        /// tbNumTask
        /// </summary>
        public System.Windows.Forms.TextBox tbNumTask;
        /// <summary>
        /// lbTitle
        /// </summary>
        public System.Windows.Forms.Label lbTitle;
    }
}