// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormAskProc
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
            this.tbProcText = new System.Windows.Forms.TextBox();
            this.btOk = new System.Windows.Forms.Button();
            this.lbTitle = new System.Windows.Forms.Label();
            this.tbProcFile = new System.Windows.Forms.TextBox();
            this.btCancel = new System.Windows.Forms.Button();
            this.tbWarning = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btAbort = new System.Windows.Forms.Button();
            this.tbProcName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btExec = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tbConnection = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tbProcText
            // 
            this.tbProcText.AcceptsReturn = true;
            this.tbProcText.AcceptsTab = true;
            this.tbProcText.Location = new System.Drawing.Point(19, 111);
            this.tbProcText.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbProcText.Multiline = true;
            this.tbProcText.Name = "tbProcText";
            this.tbProcText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbProcText.Size = new System.Drawing.Size(956, 291);
            this.tbProcText.TabIndex = 6;
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(19, 666);
            this.btOk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(199, 59);
            this.btOk.TabIndex = 0;
            this.btOk.Text = "Сохранить";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Location = new System.Drawing.Point(16, 57);
            this.lbTitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(57, 16);
            this.lbTitle.TabIndex = 2;
            this.lbTitle.Text = "Скрипт:";
            this.lbTitle.Click += new System.EventHandler(this.lbTitle_Click);
            // 
            // tbProcFile
            // 
            this.tbProcFile.Location = new System.Drawing.Point(179, 54);
            this.tbProcFile.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbProcFile.Name = "tbProcFile";
            this.tbProcFile.Size = new System.Drawing.Size(797, 22);
            this.tbProcFile.TabIndex = 5;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(237, 666);
            this.btCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(252, 59);
            this.btCancel.TabIndex = 1;
            this.btCancel.Text = "Пропустить текущий";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // tbWarning
            // 
            this.tbWarning.Location = new System.Drawing.Point(19, 426);
            this.tbWarning.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbWarning.Multiline = true;
            this.tbWarning.Name = "tbWarning";
            this.tbWarning.ReadOnly = true;
            this.tbWarning.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbWarning.Size = new System.Drawing.Size(956, 168);
            this.tbWarning.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 82);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(255, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Текст процедуры или представления:";
            this.label1.Click += new System.EventHandler(this.lbTitle_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 406);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(271, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Предупреждения, требующие внимания:";
            this.label2.Click += new System.EventHandler(this.lbTitle_Click);
            // 
            // btAbort
            // 
            this.btAbort.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.btAbort.Location = new System.Drawing.Point(749, 666);
            this.btAbort.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btAbort.Name = "btAbort";
            this.btAbort.Size = new System.Drawing.Size(225, 59);
            this.btAbort.TabIndex = 3;
            this.btAbort.Text = "Прекратить разбор";
            this.btAbort.UseVisualStyleBackColor = true;
            this.btAbort.Click += new System.EventHandler(this.btAbort_Click);
            // 
            // tbProcName
            // 
            this.tbProcName.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tbProcName.Location = new System.Drawing.Point(179, 12);
            this.tbProcName.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbProcName.Name = "tbProcName";
            this.tbProcName.Size = new System.Drawing.Size(797, 27);
            this.tbProcName.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 15);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(109, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Наименование:";
            // 
            // btExec
            // 
            this.btExec.Location = new System.Drawing.Point(509, 666);
            this.btExec.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btExec.Name = "btExec";
            this.btExec.Size = new System.Drawing.Size(217, 59);
            this.btExec.TabIndex = 2;
            this.btExec.Text = "Выполнить";
            this.btExec.UseVisualStyleBackColor = true;
            this.btExec.Click += new System.EventHandler(this.btExec_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 612);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(104, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "Подключение: ";
            this.label4.Click += new System.EventHandler(this.lbTitle_Click);
            // 
            // tbConnection
            // 
            this.tbConnection.Location = new System.Drawing.Point(144, 612);
            this.tbConnection.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tbConnection.Name = "tbConnection";
            this.tbConnection.ReadOnly = true;
            this.tbConnection.Size = new System.Drawing.Size(797, 22);
            this.tbConnection.TabIndex = 8;
            // 
            // FormAskProc
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(1008, 757);
            this.Controls.Add(this.tbProcName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.tbConnection);
            this.Controls.Add(this.tbProcFile);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbTitle);
            this.Controls.Add(this.btAbort);
            this.Controls.Add(this.btExec);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.tbWarning);
            this.Controls.Add(this.tbProcText);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormAskProc";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Сохранить хранимую процедуру или представление";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAskProc_FormClosed);
            this.Shown += new System.EventHandler(this.FormAskProc_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        /// <summary>
        /// tbProcText
        /// </summary>
        public System.Windows.Forms.TextBox tbProcText;
        /// <summary>
        /// lbTitle
        /// </summary>
        public System.Windows.Forms.Label lbTitle;
        /// <summary>
        /// tbProcFile
        /// </summary>
        public System.Windows.Forms.TextBox tbProcFile;
        private System.Windows.Forms.Button btCancel;
        /// <summary>
        /// tbWarning
        /// </summary>
        public System.Windows.Forms.TextBox tbWarning;
        /// <summary>
        /// btOk
        /// </summary>
        public System.Windows.Forms.Button btOk;
        /// <summary>
        /// label1
        /// </summary>
        public System.Windows.Forms.Label label1;
        /// <summary>
        /// label2
        /// </summary>
        public System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btAbort;
        /// <summary>
        /// tbProcName
        /// </summary>
        public System.Windows.Forms.TextBox tbProcName;
        /// <summary>
        /// label3
        /// </summary>
        public System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btExec;
        /// <summary>
        /// label4
        /// </summary>
        public System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbConnection;
    }
}