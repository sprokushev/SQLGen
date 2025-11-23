// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormAskServer
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
            this.tbNewName = new System.Windows.Forms.TextBox();
            this.btOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbOldName = new System.Windows.Forms.TextBox();
            this.tbDBType = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbNewName
            // 
            this.tbNewName.Location = new System.Drawing.Point(308, 82);
            this.tbNewName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbNewName.Name = "tbNewName";
            this.tbNewName.Size = new System.Drawing.Size(381, 22);
            this.tbNewName.TabIndex = 0;
            this.tbNewName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbNewName_KeyDown);
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(308, 154);
            this.btOk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(112, 43);
            this.btOk.TabIndex = 1;
            this.btOk.Text = "Ok";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 85);
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
            // tbOldName
            // 
            this.tbOldName.Location = new System.Drawing.Point(308, 22);
            this.tbOldName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbOldName.Name = "tbOldName";
            this.tbOldName.ReadOnly = true;
            this.tbOldName.Size = new System.Drawing.Size(381, 22);
            this.tbOldName.TabIndex = 0;
            this.tbOldName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbNewName_KeyDown);
            // 
            // tbDBType
            // 
            this.tbDBType.Location = new System.Drawing.Point(308, 52);
            this.tbDBType.Margin = new System.Windows.Forms.Padding(4);
            this.tbDBType.Name = "tbDBType";
            this.tbDBType.ReadOnly = true;
            this.tbDBType.Size = new System.Drawing.Size(381, 22);
            this.tbDBType.TabIndex = 0;
            this.tbDBType.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbNewName_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 55);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Тип БД:";
            // 
            // FormAskServer
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(763, 238);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.tbDBType);
            this.Controls.Add(this.tbOldName);
            this.Controls.Add(this.tbNewName);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormAskServer";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Смена имени или адреса сервера для всех БД";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAskServer_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btOk;
        /// <summary>
        /// tbNewName
        /// </summary>
        public System.Windows.Forms.TextBox tbNewName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        /// <summary>
        /// tbOldName
        /// </summary>
        public System.Windows.Forms.TextBox tbOldName;
        /// <summary>
        /// tbDBType
        /// </summary>
        public System.Windows.Forms.TextBox tbDBType;
        private System.Windows.Forms.Label label3;
    }
}