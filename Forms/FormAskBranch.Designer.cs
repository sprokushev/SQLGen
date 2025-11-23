// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormAskBranch
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
            this.btDev = new System.Windows.Forms.Button();
            this.btMaster = new System.Windows.Forms.Button();
            this.btChoose = new System.Windows.Forms.Button();
            this.btTask = new System.Windows.Forms.Button();
            this.btAbort = new System.Windows.Forms.Button();
            this.btCurrentBranch = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btDev
            // 
            this.btDev.Location = new System.Drawing.Point(35, 111);
            this.btDev.Margin = new System.Windows.Forms.Padding(4);
            this.btDev.Name = "btDev";
            this.btDev.Size = new System.Drawing.Size(100, 43);
            this.btDev.TabIndex = 1;
            this.btDev.Text = "dev";
            this.btDev.UseVisualStyleBackColor = true;
            this.btDev.Click += new System.EventHandler(this.btDev_Click);
            // 
            // btMaster
            // 
            this.btMaster.Location = new System.Drawing.Point(158, 111);
            this.btMaster.Margin = new System.Windows.Forms.Padding(4);
            this.btMaster.Name = "btMaster";
            this.btMaster.Size = new System.Drawing.Size(100, 43);
            this.btMaster.TabIndex = 2;
            this.btMaster.Text = "master";
            this.btMaster.UseVisualStyleBackColor = true;
            this.btMaster.Click += new System.EventHandler(this.btMaster_Click);
            // 
            // btChoose
            // 
            this.btChoose.Location = new System.Drawing.Point(285, 111);
            this.btChoose.Margin = new System.Windows.Forms.Padding(4);
            this.btChoose.Name = "btChoose";
            this.btChoose.Size = new System.Drawing.Size(100, 43);
            this.btChoose.TabIndex = 3;
            this.btChoose.Text = "Другая";
            this.btChoose.UseVisualStyleBackColor = true;
            this.btChoose.Click += new System.EventHandler(this.btChoose_Click);
            // 
            // btTask
            // 
            this.btTask.Location = new System.Drawing.Point(37, 15);
            this.btTask.Margin = new System.Windows.Forms.Padding(4);
            this.btTask.Name = "btTask";
            this.btTask.Size = new System.Drawing.Size(349, 41);
            this.btTask.TabIndex = 0;
            this.btTask.Text = "Ветка задачи";
            this.btTask.UseVisualStyleBackColor = true;
            this.btTask.Click += new System.EventHandler(this.btTask_Click);
            // 
            // btAbort
            // 
            this.btAbort.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btAbort.Location = new System.Drawing.Point(159, 168);
            this.btAbort.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btAbort.Name = "btAbort";
            this.btAbort.Size = new System.Drawing.Size(99, 44);
            this.btAbort.TabIndex = 4;
            this.btAbort.Text = "Прервать";
            this.btAbort.UseVisualStyleBackColor = true;
            this.btAbort.Click += new System.EventHandler(this.btAbort_Click);
            // 
            // btCurrentBranch
            // 
            this.btCurrentBranch.Location = new System.Drawing.Point(37, 62);
            this.btCurrentBranch.Margin = new System.Windows.Forms.Padding(4);
            this.btCurrentBranch.Name = "btCurrentBranch";
            this.btCurrentBranch.Size = new System.Drawing.Size(349, 41);
            this.btCurrentBranch.TabIndex = 5;
            this.btCurrentBranch.Text = "Текущая ветка";
            this.btCurrentBranch.UseVisualStyleBackColor = true;
            this.btCurrentBranch.Click += new System.EventHandler(this.btCurrentBranch_Click);
            // 
            // FormAskBranch
            // 
            this.AcceptButton = this.btTask;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btAbort;
            this.ClientSize = new System.Drawing.Size(460, 277);
            this.Controls.Add(this.btCurrentBranch);
            this.Controls.Add(this.btAbort);
            this.Controls.Add(this.btChoose);
            this.Controls.Add(this.btMaster);
            this.Controls.Add(this.btDev);
            this.Controls.Add(this.btTask);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormAskBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Выбрать ветку";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAskBranch_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btDev;
        private System.Windows.Forms.Button btMaster;
        private System.Windows.Forms.Button btChoose;
        private System.Windows.Forms.Button btAbort;
        private System.Windows.Forms.Button btCurrentBranch;
        /// <summary>
        /// btTask
        /// </summary>
        public System.Windows.Forms.Button btTask;
    }
}