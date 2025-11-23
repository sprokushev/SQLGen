// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormAskDB
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
            this.tbNewServer = new System.Windows.Forms.TextBox();
            this.btOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbOldServer = new System.Windows.Forms.TextBox();
            this.tbOldDB = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbNewDB = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbDBType = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbNewServer
            // 
            this.tbNewServer.Location = new System.Drawing.Point(299, 128);
            this.tbNewServer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbNewServer.Name = "tbNewServer";
            this.tbNewServer.Size = new System.Drawing.Size(381, 22);
            this.tbNewServer.TabIndex = 0;
            this.tbNewServer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbNewServer_KeyDown);
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(314, 206);
            this.btOk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(125, 46);
            this.btOk.TabIndex = 1;
            this.btOk.Text = "Ok";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 131);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Новое имя или адрес сервера:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 22);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(221, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Текущее имя или адрес сервера:";
            // 
            // tbOldServer
            // 
            this.tbOldServer.Location = new System.Drawing.Point(299, 22);
            this.tbOldServer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbOldServer.Name = "tbOldServer";
            this.tbOldServer.ReadOnly = true;
            this.tbOldServer.Size = new System.Drawing.Size(381, 22);
            this.tbOldServer.TabIndex = 0;
            this.tbOldServer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbNewServer_KeyDown);
            // 
            // tbOldDB
            // 
            this.tbOldDB.Location = new System.Drawing.Point(299, 50);
            this.tbOldDB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbOldDB.Name = "tbOldDB";
            this.tbOldDB.ReadOnly = true;
            this.tbOldDB.Size = new System.Drawing.Size(381, 22);
            this.tbOldDB.TabIndex = 0;
            this.tbOldDB.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbNewServer_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 50);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Текущее имя БД:";
            // 
            // tbNewDB
            // 
            this.tbNewDB.Location = new System.Drawing.Point(299, 157);
            this.tbNewDB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbNewDB.Name = "tbNewDB";
            this.tbNewDB.Size = new System.Drawing.Size(381, 22);
            this.tbNewDB.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 159);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 16);
            this.label4.TabIndex = 2;
            this.label4.Text = "Новое имя БД:";
            // 
            // tbDBType
            // 
            this.tbDBType.Location = new System.Drawing.Point(299, 80);
            this.tbDBType.Margin = new System.Windows.Forms.Padding(4);
            this.tbDBType.Name = "tbDBType";
            this.tbDBType.ReadOnly = true;
            this.tbDBType.Size = new System.Drawing.Size(381, 22);
            this.tbDBType.TabIndex = 0;
            this.tbDBType.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbNewServer_KeyDown);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 83);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 16);
            this.label5.TabIndex = 2;
            this.label5.Text = "Тип БД:";
            // 
            // FormAskDB
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(753, 285);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.tbDBType);
            this.Controls.Add(this.tbOldDB);
            this.Controls.Add(this.tbOldServer);
            this.Controls.Add(this.tbNewDB);
            this.Controls.Add(this.tbNewServer);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormAskDB";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Перенос БД с одного сервера на другой";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAskDB_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btOk;
        /// <summary>
        /// tbNewServer
        /// </summary>
        public System.Windows.Forms.TextBox tbNewServer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        /// <summary>
        /// tbOldServer
        /// </summary>
        public System.Windows.Forms.TextBox tbOldServer;
        /// <summary>
        /// tbOldDB
        /// </summary>
        public System.Windows.Forms.TextBox tbOldDB;
        private System.Windows.Forms.Label label3;
        /// <summary>
        /// tbNewDB
        /// </summary>
        public System.Windows.Forms.TextBox tbNewDB;
        private System.Windows.Forms.Label label4;
        /// <summary>
        /// tbDBType
        /// </summary>
        public System.Windows.Forms.TextBox tbDBType;
        private System.Windows.Forms.Label label5;
    }
}