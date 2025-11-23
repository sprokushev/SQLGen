// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormLogin
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
            this.cbConnectionHistory = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbConnectionName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cbTypeDB = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbServerName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbDatabaseName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cbAuthentication = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.btConnect = new System.Windows.Forms.Button();
            this.btDel = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.tbTimeout = new System.Windows.Forms.NumericUpDown();
            this.cbSavePassword = new System.Windows.Forms.CheckBox();
            this.btChooseDB = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.tbConnectionAdd = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.cbTrustServerCertificate = new System.Windows.Forms.CheckBox();
            this.btSave = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.tbTimeout)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 26);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Список подключений:";
            // 
            // cbConnectionHistory
            // 
            this.cbConnectionHistory.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbConnectionHistory.FormattingEnabled = true;
            this.cbConnectionHistory.Location = new System.Drawing.Point(249, 22);
            this.cbConnectionHistory.Margin = new System.Windows.Forms.Padding(4);
            this.cbConnectionHistory.Name = "cbConnectionHistory";
            this.cbConnectionHistory.Size = new System.Drawing.Size(685, 24);
            this.cbConnectionHistory.TabIndex = 1;
            this.cbConnectionHistory.SelectedIndexChanged += new System.EventHandler(this.cbConnectionHistory_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 66);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Название подключения:";
            // 
            // tbConnectionName
            // 
            this.tbConnectionName.Location = new System.Drawing.Point(249, 62);
            this.tbConnectionName.Margin = new System.Windows.Forms.Padding(4);
            this.tbConnectionName.Name = "tbConnectionName";
            this.tbConnectionName.Size = new System.Drawing.Size(685, 22);
            this.tbConnectionName.TabIndex = 3;
            this.tbConnectionName.TextChanged += new System.EventHandler(this.tbConnectionName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 107);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Тип БД:";
            // 
            // cbTypeDB
            // 
            this.cbTypeDB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTypeDB.FormattingEnabled = true;
            this.cbTypeDB.Items.AddRange(new object[] {
            "Microsoft SQL",
            "Postgre SQL",
            "DBF"});
            this.cbTypeDB.Location = new System.Drawing.Point(249, 101);
            this.cbTypeDB.Margin = new System.Windows.Forms.Padding(4);
            this.cbTypeDB.Name = "cbTypeDB";
            this.cbTypeDB.Size = new System.Drawing.Size(391, 24);
            this.cbTypeDB.TabIndex = 5;
            this.cbTypeDB.SelectedIndexChanged += new System.EventHandler(this.cbTypeDB_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 148);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 16);
            this.label4.TabIndex = 6;
            this.label4.Text = "Сервер:";
            // 
            // tbServerName
            // 
            this.tbServerName.Location = new System.Drawing.Point(249, 143);
            this.tbServerName.Margin = new System.Windows.Forms.Padding(4);
            this.tbServerName.Name = "tbServerName";
            this.tbServerName.Size = new System.Drawing.Size(391, 22);
            this.tbServerName.TabIndex = 7;
            this.tbServerName.TextChanged += new System.EventHandler(this.tbServerName_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 188);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 16);
            this.label5.TabIndex = 8;
            this.label5.Text = "База данных:";
            // 
            // tbDatabaseName
            // 
            this.tbDatabaseName.Location = new System.Drawing.Point(249, 183);
            this.tbDatabaseName.Margin = new System.Windows.Forms.Padding(4);
            this.tbDatabaseName.Name = "tbDatabaseName";
            this.tbDatabaseName.Size = new System.Drawing.Size(391, 22);
            this.tbDatabaseName.TabIndex = 9;
            this.tbDatabaseName.TextChanged += new System.EventHandler(this.tbDatabaseName_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(27, 280);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(125, 16);
            this.label6.TabIndex = 10;
            this.label6.Text = "Тип авторизации:";
            // 
            // cbAuthentication
            // 
            this.cbAuthentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbAuthentication.FormattingEnabled = true;
            this.cbAuthentication.Items.AddRange(new object[] {
            "Windows",
            "Database"});
            this.cbAuthentication.Location = new System.Drawing.Point(249, 275);
            this.cbAuthentication.Margin = new System.Windows.Forms.Padding(4);
            this.cbAuthentication.Name = "cbAuthentication";
            this.cbAuthentication.Size = new System.Drawing.Size(391, 24);
            this.cbAuthentication.TabIndex = 11;
            this.cbAuthentication.SelectedIndexChanged += new System.EventHandler(this.cbAuthentication_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(27, 321);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(105, 16);
            this.label7.TabIndex = 12;
            this.label7.Text = "Пользователь:";
            // 
            // tbUsername
            // 
            this.tbUsername.Location = new System.Drawing.Point(249, 318);
            this.tbUsername.Margin = new System.Windows.Forms.Padding(4);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(391, 22);
            this.tbUsername.TabIndex = 13;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(27, 361);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 16);
            this.label8.TabIndex = 14;
            this.label8.Text = "Пароль:";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(249, 356);
            this.tbPassword.Margin = new System.Windows.Forms.Padding(4);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(391, 22);
            this.tbPassword.TabIndex = 15;
            // 
            // btConnect
            // 
            this.btConnect.Location = new System.Drawing.Point(116, 421);
            this.btConnect.Margin = new System.Windows.Forms.Padding(4);
            this.btConnect.Name = "btConnect";
            this.btConnect.Size = new System.Drawing.Size(275, 41);
            this.btConnect.TabIndex = 16;
            this.btConnect.Text = "Подключиться";
            this.btConnect.UseVisualStyleBackColor = true;
            this.btConnect.Click += new System.EventHandler(this.btConnect_Click);
            // 
            // btDel
            // 
            this.btDel.Location = new System.Drawing.Point(960, 20);
            this.btDel.Margin = new System.Windows.Forms.Padding(4);
            this.btDel.Name = "btDel";
            this.btDel.Size = new System.Drawing.Size(115, 38);
            this.btDel.TabIndex = 17;
            this.btDel.Text = "DEL";
            this.btDel.UseVisualStyleBackColor = true;
            this.btDel.Click += new System.EventHandler(this.btDel_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(659, 145);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(59, 16);
            this.label9.TabIndex = 19;
            this.label9.Text = "Timeout:";
            // 
            // tbTimeout
            // 
            this.tbTimeout.Location = new System.Drawing.Point(741, 143);
            this.tbTimeout.Margin = new System.Windows.Forms.Padding(4);
            this.tbTimeout.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.tbTimeout.Name = "tbTimeout";
            this.tbTimeout.Size = new System.Drawing.Size(87, 22);
            this.tbTimeout.TabIndex = 20;
            this.tbTimeout.Value = new decimal(new int[] {
            120,
            0,
            0,
            0});
            // 
            // cbSavePassword
            // 
            this.cbSavePassword.AutoSize = true;
            this.cbSavePassword.Location = new System.Drawing.Point(663, 355);
            this.cbSavePassword.Margin = new System.Windows.Forms.Padding(4);
            this.cbSavePassword.Name = "cbSavePassword";
            this.cbSavePassword.Size = new System.Drawing.Size(148, 20);
            this.cbSavePassword.TabIndex = 21;
            this.cbSavePassword.Text = "Сохранить пароль";
            this.cbSavePassword.UseVisualStyleBackColor = true;
            this.cbSavePassword.CheckedChanged += new System.EventHandler(this.cbSavePassword_CheckedChanged);
            // 
            // btChooseDB
            // 
            this.btChooseDB.Location = new System.Drawing.Point(663, 182);
            this.btChooseDB.Margin = new System.Windows.Forms.Padding(4);
            this.btChooseDB.Name = "btChooseDB";
            this.btChooseDB.Size = new System.Drawing.Size(123, 37);
            this.btChooseDB.TabIndex = 22;
            this.btChooseDB.Text = "Выбрать";
            this.btChooseDB.UseVisualStyleBackColor = true;
            this.btChooseDB.Click += new System.EventHandler(this.btChooseDB_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(443, 421);
            this.btCancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(275, 41);
            this.btCancel.TabIndex = 23;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // tbConnectionAdd
            // 
            this.tbConnectionAdd.Location = new System.Drawing.Point(30, 245);
            this.tbConnectionAdd.Margin = new System.Windows.Forms.Padding(4);
            this.tbConnectionAdd.Name = "tbConnectionAdd";
            this.tbConnectionAdd.Size = new System.Drawing.Size(1052, 22);
            this.tbConnectionAdd.TabIndex = 9;
            this.tbConnectionAdd.TextChanged += new System.EventHandler(this.tbDatabaseName_TextChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(27, 225);
            this.label10.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(522, 16);
            this.label10.TabIndex = 24;
            this.label10.Text = "Дополнительные параметры для строки подключения (через точку с запятой):";
            // 
            // cbTrustServerCertificate
            // 
            this.cbTrustServerCertificate.AutoSize = true;
            this.cbTrustServerCertificate.Location = new System.Drawing.Point(831, 355);
            this.cbTrustServerCertificate.Margin = new System.Windows.Forms.Padding(4);
            this.cbTrustServerCertificate.Name = "cbTrustServerCertificate";
            this.cbTrustServerCertificate.Size = new System.Drawing.Size(298, 20);
            this.cbTrustServerCertificate.TabIndex = 21;
            this.cbTrustServerCertificate.Text = "Trust Server Certificate (только для MS SQL)";
            this.cbTrustServerCertificate.UseVisualStyleBackColor = true;
            // 
            // btSave
            // 
            this.btSave.DialogResult = System.Windows.Forms.DialogResult.Ignore;
            this.btSave.Location = new System.Drawing.Point(761, 421);
            this.btSave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btSave.Name = "btSave";
            this.btSave.Size = new System.Drawing.Size(275, 41);
            this.btSave.TabIndex = 23;
            this.btSave.Text = "Сохранить";
            this.btSave.UseVisualStyleBackColor = true;
            this.btSave.Click += new System.EventHandler(this.btSave_Click);
            // 
            // FormLogin
            // 
            this.AcceptButton = this.btConnect;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(1187, 570);
            this.Controls.Add(this.btConnect);
            this.Controls.Add(this.tbPassword);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btSave);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btChooseDB);
            this.Controls.Add(this.cbTrustServerCertificate);
            this.Controls.Add(this.cbSavePassword);
            this.Controls.Add(this.tbTimeout);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btDel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tbUsername);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.cbAuthentication);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbConnectionAdd);
            this.Controls.Add(this.tbDatabaseName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbServerName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cbTypeDB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbConnectionName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.cbConnectionHistory);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Подключение к БД";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLogin_FormClosed);
            this.Shown += new System.EventHandler(this.FormLogin_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.tbTimeout)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        /// <summary>
        /// cbConnectionHistory
        /// </summary>
        public System.Windows.Forms.ComboBox cbConnectionHistory;
        private System.Windows.Forms.Label label2;
        /// <summary>
        /// tbConnectionName
        /// </summary>
        public System.Windows.Forms.TextBox tbConnectionName;
        private System.Windows.Forms.Label label3;
        /// <summary>
        /// cbTypeDB
        /// </summary>
        public System.Windows.Forms.ComboBox cbTypeDB;
        private System.Windows.Forms.Label label4;
        /// <summary>
        /// tbServerName
        /// </summary>
        public System.Windows.Forms.TextBox tbServerName;
        private System.Windows.Forms.Label label5;
        /// <summary>
        /// tbDatabaseName
        /// </summary>
        public System.Windows.Forms.TextBox tbDatabaseName;
        private System.Windows.Forms.Label label6;
        /// <summary>
        /// cbAuthentication
        /// </summary>
        public System.Windows.Forms.ComboBox cbAuthentication;
        private System.Windows.Forms.Label label7;
        /// <summary>
        /// tbUsername
        /// </summary>
        public System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button btConnect;
        private System.Windows.Forms.Button btDel;
        /// <summary>
        /// tbPassword
        /// </summary>
        public System.Windows.Forms.TextBox tbPassword;
        private System.Windows.Forms.Label label9;
        /// <summary>
        /// tbTimeout
        /// </summary>
        public System.Windows.Forms.NumericUpDown tbTimeout;
        /// <summary>
        /// cbSavePassword
        /// </summary>
        public System.Windows.Forms.CheckBox cbSavePassword;
        private System.Windows.Forms.Button btChooseDB;
        private System.Windows.Forms.Button btCancel;
        /// <summary>
        /// tbConnectionAdd
        /// </summary>
        public System.Windows.Forms.TextBox tbConnectionAdd;
        private System.Windows.Forms.Label label10;
        /// <summary>
        /// cbTrustServerCertificate
        /// </summary>
        public System.Windows.Forms.CheckBox cbTrustServerCertificate;
        private System.Windows.Forms.Button btSave;
    }
}