// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

namespace SQLGen
{
    partial class FormAddDB
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
            this.label1 = new System.Windows.Forms.Label();
            this.tbServerName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbDBName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbGITProject = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cbDBRole = new System.Windows.Forms.ComboBox();
            this.isMainTest = new System.Windows.Forms.CheckBox();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(162, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Имя сервера или IP-адрес:";
            // 
            // tbServerName
            // 
            this.tbServerName.Location = new System.Drawing.Point(252, 19);
            this.tbServerName.Name = "tbServerName";
            this.tbServerName.Size = new System.Drawing.Size(413, 20);
            this.tbServerName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(33, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Имя базы данных:";
            // 
            // tbDBName
            // 
            this.tbDBName.Location = new System.Drawing.Point(252, 64);
            this.tbDBName.Name = "tbDBName";
            this.tbDBName.Size = new System.Drawing.Size(413, 20);
            this.tbDBName.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(33, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Проект GIT:";
            // 
            // cbGITProject
            // 
            this.cbGITProject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGITProject.FormattingEnabled = true;
            this.cbGITProject.Location = new System.Drawing.Point(251, 111);
            this.cbGITProject.Name = "cbGITProject";
            this.cbGITProject.Size = new System.Drawing.Size(414, 21);
            this.cbGITProject.TabIndex = 5;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(34, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 15);
            this.label4.TabIndex = 6;
            this.label4.Text = "Роль БД:";
            // 
            // cbDBRole
            // 
            this.cbDBRole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDBRole.FormattingEnabled = true;
            this.cbDBRole.Items.AddRange(new object[] {
            "TEST",
            "RELEASE",
            "PRODLIKE",
            "PROD",
            "REPORT",
            "REESTR"});
            this.cbDBRole.Location = new System.Drawing.Point(251, 164);
            this.cbDBRole.Name = "cbDBRole";
            this.cbDBRole.Size = new System.Drawing.Size(414, 21);
            this.cbDBRole.TabIndex = 7;
            this.cbDBRole.SelectedIndexChanged += new System.EventHandler(this.cbDBRole_SelectedIndexChanged);
            // 
            // isMainTest
            // 
            this.isMainTest.AutoSize = true;
            this.isMainTest.Location = new System.Drawing.Point(255, 214);
            this.isMainTest.Name = "isMainTest";
            this.isMainTest.Size = new System.Drawing.Size(265, 19);
            this.isMainTest.TabIndex = 8;
            this.isMainTest.Text = "Основная тестовая БД для проекта ГИТ";
            this.isMainTest.UseVisualStyleBackColor = true;
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(159, 285);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(154, 49);
            this.btOk.TabIndex = 9;
            this.btOk.Text = "Сохранить";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(430, 285);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(136, 49);
            this.btCancel.TabIndex = 10;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // FormAddDB
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(759, 355);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.isMainTest);
            this.Controls.Add(this.cbDBRole);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbGITProject);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbDBName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbServerName);
            this.Controls.Add(this.label1);
            this.Name = "FormAddDB";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormAddDB";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAddDB_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        /// <summary>
        /// tbServerName
        /// </summary>
        public System.Windows.Forms.TextBox tbServerName;
        /// <summary>
        /// tbDBName
        /// </summary>
        public System.Windows.Forms.TextBox tbDBName;
        /// <summary>
        /// cbGITProject
        /// </summary>
        public System.Windows.Forms.ComboBox cbGITProject;
        /// <summary>
        /// cbDBRole
        /// </summary>
        public System.Windows.Forms.ComboBox cbDBRole;
        /// <summary>
        /// isMainTest
        /// </summary>
        public System.Windows.Forms.CheckBox isMainTest;
    }
}