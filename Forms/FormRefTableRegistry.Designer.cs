// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

namespace SQLGen
{
    partial class FormRefTableRegistry
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
            this.tbOID = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbFullName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbShortName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tbTableName = new System.Windows.Forms.TextBox();
            this.tbVersion = new System.Windows.Forms.TextBox();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.tbPublishDate = new System.Windows.Forms.DateTimePicker();
            this.tbCreateDate = new System.Windows.Forms.DateTimePicker();
            this.label8 = new System.Windows.Forms.Label();
            this.tbRefTableRegistry_id = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.tbRefTableRegistryVersion_id = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 62);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(31, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "OID:";
            // 
            // tbOID
            // 
            this.tbOID.Location = new System.Drawing.Point(203, 62);
            this.tbOID.Name = "tbOID";
            this.tbOID.Size = new System.Drawing.Size(329, 20);
            this.tbOID.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 88);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Полное наименование:";
            // 
            // tbFullName
            // 
            this.tbFullName.Location = new System.Drawing.Point(203, 88);
            this.tbFullName.Name = "tbFullName";
            this.tbFullName.Size = new System.Drawing.Size(560, 20);
            this.tbFullName.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(17, 114);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(148, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "Краткое наименование:";
            // 
            // tbShortName
            // 
            this.tbShortName.Location = new System.Drawing.Point(203, 114);
            this.tbShortName.Name = "tbShortName";
            this.tbShortName.Size = new System.Drawing.Size(560, 20);
            this.tbShortName.TabIndex = 4;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 140);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(241, 15);
            this.label4.TabIndex = 5;
            this.label4.Text = "Дата публикации 1 версии справочника:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 191);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(102, 15);
            this.label5.TabIndex = 6;
            this.label5.Text = "Текущая версия:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 217);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(282, 15);
            this.label6.TabIndex = 7;
            this.label6.Text = "Дата публикации текущей версии справочника:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(17, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(166, 15);
            this.label7.TabIndex = 8;
            this.label7.Text = "Таблица справочника в БД:";
            // 
            // tbTableName
            // 
            this.tbTableName.Enabled = false;
            this.tbTableName.Location = new System.Drawing.Point(203, 10);
            this.tbTableName.Name = "tbTableName";
            this.tbTableName.ReadOnly = true;
            this.tbTableName.Size = new System.Drawing.Size(329, 20);
            this.tbTableName.TabIndex = 1;
            // 
            // tbVersion
            // 
            this.tbVersion.Location = new System.Drawing.Point(203, 191);
            this.tbVersion.Name = "tbVersion";
            this.tbVersion.Size = new System.Drawing.Size(234, 20);
            this.tbVersion.TabIndex = 7;
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(132, 260);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(179, 31);
            this.btOk.TabIndex = 9;
            this.btOk.Text = "Сгенерировать скрипт";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(427, 260);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(187, 31);
            this.btCancel.TabIndex = 10;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // tbPublishDate
            // 
            this.tbPublishDate.CustomFormat = "dd.MM.yyyy HH.mm";
            this.tbPublishDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.tbPublishDate.Location = new System.Drawing.Point(314, 217);
            this.tbPublishDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.tbPublishDate.Name = "tbPublishDate";
            this.tbPublishDate.Size = new System.Drawing.Size(188, 20);
            this.tbPublishDate.TabIndex = 8;
            // 
            // tbCreateDate
            // 
            this.tbCreateDate.CustomFormat = "dd.MM.yyyy HH.mm";
            this.tbCreateDate.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.tbCreateDate.Location = new System.Drawing.Point(314, 140);
            this.tbCreateDate.MinDate = new System.DateTime(1900, 1, 1, 0, 0, 0, 0);
            this.tbCreateDate.Name = "tbCreateDate";
            this.tbCreateDate.Size = new System.Drawing.Size(188, 20);
            this.tbCreateDate.TabIndex = 5;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(17, 36);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(121, 15);
            this.label8.TabIndex = 11;
            this.label8.Text = "RefTableRegistry_id:";
            // 
            // tbRefTableRegistry_id
            // 
            this.tbRefTableRegistry_id.Location = new System.Drawing.Point(203, 36);
            this.tbRefTableRegistry_id.Name = "tbRefTableRegistry_id";
            this.tbRefTableRegistry_id.Size = new System.Drawing.Size(329, 20);
            this.tbRefTableRegistry_id.TabIndex = 1;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(17, 165);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(162, 15);
            this.label9.TabIndex = 13;
            this.label9.Text = "RefTableRegistryVersion_id:";
            // 
            // tbRefTableRegistryVersion_id
            // 
            this.tbRefTableRegistryVersion_id.Location = new System.Drawing.Point(203, 165);
            this.tbRefTableRegistryVersion_id.Name = "tbRefTableRegistryVersion_id";
            this.tbRefTableRegistryVersion_id.Size = new System.Drawing.Size(329, 20);
            this.tbRefTableRegistryVersion_id.TabIndex = 6;
            // 
            // FormRefTableRegistry
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(800, 315);
            this.Controls.Add(this.tbRefTableRegistry_id);
            this.Controls.Add(this.tbOID);
            this.Controls.Add(this.tbFullName);
            this.Controls.Add(this.tbShortName);
            this.Controls.Add(this.tbCreateDate);
            this.Controls.Add(this.tbRefTableRegistryVersion_id);
            this.Controls.Add(this.tbVersion);
            this.Controls.Add(this.tbPublishDate);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbTableName);
            this.Controls.Add(this.label1);
            this.Name = "FormRefTableRegistry";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Скрипт для nsi.RefTableRegistry";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormRefTableRegistry_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        /// <summary>
        /// tbOID
        /// </summary>
        public System.Windows.Forms.TextBox tbOID;
        private System.Windows.Forms.Label label2;
        /// <summary>
        /// tbFullName
        /// </summary>
        public System.Windows.Forms.TextBox tbFullName;
        private System.Windows.Forms.Label label3;
        /// <summary>
        /// tbShortName
        /// </summary>
        public System.Windows.Forms.TextBox tbShortName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        /// <summary>
        /// tbTableName
        /// </summary>
        public System.Windows.Forms.TextBox tbTableName;
        /// <summary>
        /// tbVersion
        /// </summary>
        public System.Windows.Forms.TextBox tbVersion;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        /// <summary>
        /// tbPublishDate
        /// </summary>
        public System.Windows.Forms.DateTimePicker tbPublishDate;
        /// <summary>
        /// tbCreateDate
        /// </summary>
        public System.Windows.Forms.DateTimePicker tbCreateDate;
        private System.Windows.Forms.Label label8;
        /// <summary>
        /// tbRefTableRegistry_id
        /// </summary>
        public System.Windows.Forms.TextBox tbRefTableRegistry_id;
        private System.Windows.Forms.Label label9;
        /// <summary>
        /// tbRefTableRegistryVersion_id
        /// </summary>
        public System.Windows.Forms.TextBox tbRefTableRegistryVersion_id;
    }
}