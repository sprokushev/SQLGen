// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

namespace SQLGen
{
    partial class FormLoad
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
            this.tbFilename = new System.Windows.Forms.TextBox();
            this.btOpen = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbTableName = new System.Windows.Forms.TextBox();
            this.btLoad = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.tbNumSheet = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.cbTypeRow = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tbNumSheet)).BeginInit();
            this.SuspendLayout();
            // 
            // tbFilename
            // 
            this.tbFilename.Location = new System.Drawing.Point(81, 12);
            this.tbFilename.Name = "tbFilename";
            this.tbFilename.Size = new System.Drawing.Size(573, 20);
            this.tbFilename.TabIndex = 7;
            // 
            // btOpen
            // 
            this.btOpen.Location = new System.Drawing.Point(672, 12);
            this.btOpen.Name = "btOpen";
            this.btOpen.Size = new System.Drawing.Size(128, 34);
            this.btOpen.TabIndex = 8;
            this.btOpen.Text = "Выбрать файл";
            this.btOpen.UseVisualStyleBackColor = true;
            this.btOpen.Click += new System.EventHandler(this.btOpen_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 15);
            this.label1.TabIndex = 6;
            this.label1.Text = "Файл:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 15);
            this.label2.TabIndex = 9;
            this.label2.Text = "Таблица:";
            // 
            // tbTableName
            // 
            this.tbTableName.Location = new System.Drawing.Point(81, 46);
            this.tbTableName.Name = "tbTableName";
            this.tbTableName.Size = new System.Drawing.Size(310, 20);
            this.tbTableName.TabIndex = 10;
            // 
            // btLoad
            // 
            this.btLoad.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btLoad.Location = new System.Drawing.Point(217, 202);
            this.btLoad.Name = "btLoad";
            this.btLoad.Size = new System.Drawing.Size(154, 35);
            this.btLoad.TabIndex = 12;
            this.btLoad.Text = "Добавить";
            this.btLoad.UseVisualStyleBackColor = true;
            this.btLoad.Click += new System.EventHandler(this.btLoad_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(456, 202);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(151, 35);
            this.btCancel.TabIndex = 13;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 84);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(131, 15);
            this.label3.TabIndex = 14;
            this.label3.Text = "Номер листа в книге:";
            // 
            // tbNumSheet
            // 
            this.tbNumSheet.Location = new System.Drawing.Point(180, 84);
            this.tbNumSheet.Name = "tbNumSheet";
            this.tbNumSheet.Size = new System.Drawing.Size(120, 20);
            this.tbNumSheet.TabIndex = 15;
            this.tbNumSheet.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.tbNumSheet.ValueChanged += new System.EventHandler(this.tbNumSheet_ValueChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 120);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(742, 15);
            this.label5.TabIndex = 18;
            this.label5.Text = "ВНИМАНИЕ! Первой строкой должны быть имена полей. Не забудьте добавить insid, upd" +
    "id, insdt, upddt (можно без значений)!";
            // 
            // cbTypeRow
            // 
            this.cbTypeRow.AutoSize = true;
            this.cbTypeRow.Location = new System.Drawing.Point(331, 85);
            this.cbTypeRow.Name = "cbTypeRow";
            this.cbTypeRow.Size = new System.Drawing.Size(191, 19);
            this.cbTypeRow.TabIndex = 19;
            this.cbTypeRow.Text = "2-я строка - с типами полей";
            this.cbTypeRow.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(93, 149);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(487, 15);
            this.label6.TabIndex = 18;
            this.label6.Text = "Если значение ячейки = identity - значение будет сформировано автоинкрементно";
            // 
            // FormLoad
            // 
            this.AcceptButton = this.btLoad;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(824, 287);
            this.Controls.Add(this.cbTypeRow);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbNumSheet);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btLoad);
            this.Controls.Add(this.tbTableName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbFilename);
            this.Controls.Add(this.btOpen);
            this.Controls.Add(this.label1);
            this.Name = "FormLoad";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Загрузить файл";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLoad_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.tbNumSheet)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        /// <summary>
        /// tbFilename
        /// </summary>
        public System.Windows.Forms.TextBox tbFilename;
        private System.Windows.Forms.Button btOpen;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btLoad;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label3;
        /// <summary>
        /// tbTableName
        /// </summary>
        public System.Windows.Forms.TextBox tbTableName;
        /// <summary>
        /// tbNumSheet
        /// </summary>
        public System.Windows.Forms.NumericUpDown tbNumSheet;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        /// <summary>
        /// cbTypeRow
        /// </summary>
        public System.Windows.Forms.CheckBox cbTypeRow;
    }
}