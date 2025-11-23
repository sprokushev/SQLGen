// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormAddIndex
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
            this.tbIndexName = new System.Windows.Forms.TextBox();
            this.cbIsUnique = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbIndexPredicat = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tbIndexInclude = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tbIndexWhere = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbIndexToDel = new System.Windows.Forms.TextBox();
            this.cbIsProd = new System.Windows.Forms.CheckBox();
            this.cbIsReg = new System.Windows.Forms.CheckBox();
            this.cbIsReport = new System.Windows.Forms.CheckBox();
            this.btGenerate = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.btPredicatFill = new System.Windows.Forms.Button();
            this.btIncludeFill = new System.Windows.Forms.Button();
            this.btAutoName = new System.Windows.Forms.Button();
            this.cbScriptType = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.cbIsNullsNotDistinct = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(23, 262);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Наименование:";
            // 
            // tbIndexName
            // 
            this.tbIndexName.Location = new System.Drawing.Point(161, 260);
            this.tbIndexName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbIndexName.Name = "tbIndexName";
            this.tbIndexName.Size = new System.Drawing.Size(481, 22);
            this.tbIndexName.TabIndex = 7;
            this.tbIndexName.TextChanged += new System.EventHandler(this.tbIndexName_TextChanged);
            this.tbIndexName.Leave += new System.EventHandler(this.tbIndexName_Leave);
            // 
            // cbIsUnique
            // 
            this.cbIsUnique.AutoSize = true;
            this.cbIsUnique.Location = new System.Drawing.Point(23, 210);
            this.cbIsUnique.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbIsUnique.Name = "cbIsUnique";
            this.cbIsUnique.Size = new System.Drawing.Size(158, 20);
            this.cbIsUnique.TabIndex = 6;
            this.cbIsUnique.Text = "Уникальный индекс";
            this.cbIsUnique.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 15);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(74, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Предикат:";
            // 
            // tbIndexPredicat
            // 
            this.tbIndexPredicat.Location = new System.Drawing.Point(116, 15);
            this.tbIndexPredicat.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbIndexPredicat.Multiline = true;
            this.tbIndexPredicat.Name = "tbIndexPredicat";
            this.tbIndexPredicat.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbIndexPredicat.Size = new System.Drawing.Size(553, 56);
            this.tbIndexPredicat.TabIndex = 1;
            this.tbIndexPredicat.TextChanged += new System.EventHandler(this.tbIndexPredicat_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 82);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 16);
            this.label3.TabIndex = 6;
            this.label3.Text = "Include:";
            // 
            // tbIndexInclude
            // 
            this.tbIndexInclude.Location = new System.Drawing.Point(116, 82);
            this.tbIndexInclude.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbIndexInclude.Multiline = true;
            this.tbIndexInclude.Name = "tbIndexInclude";
            this.tbIndexInclude.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbIndexInclude.Size = new System.Drawing.Size(553, 56);
            this.tbIndexInclude.TabIndex = 3;
            this.tbIndexInclude.TextChanged += new System.EventHandler(this.tbIndexInclude_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 149);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 16);
            this.label4.TabIndex = 8;
            this.label4.Text = "Where:";
            // 
            // tbIndexWhere
            // 
            this.tbIndexWhere.Location = new System.Drawing.Point(116, 149);
            this.tbIndexWhere.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbIndexWhere.Multiline = true;
            this.tbIndexWhere.Name = "tbIndexWhere";
            this.tbIndexWhere.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbIndexWhere.Size = new System.Drawing.Size(553, 56);
            this.tbIndexWhere.TabIndex = 5;
            this.tbIndexWhere.TextChanged += new System.EventHandler(this.tbIndexWhere_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 309);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(214, 16);
            this.label5.TabIndex = 10;
            this.label5.Text = "Список индексов для удаления:";
            // 
            // tbIndexToDel
            // 
            this.tbIndexToDel.Location = new System.Drawing.Point(23, 331);
            this.tbIndexToDel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbIndexToDel.Multiline = true;
            this.tbIndexToDel.Name = "tbIndexToDel";
            this.tbIndexToDel.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbIndexToDel.Size = new System.Drawing.Size(739, 67);
            this.tbIndexToDel.TabIndex = 9;
            this.tbIndexToDel.TextChanged += new System.EventHandler(this.tbIndexToDel_TextChanged);
            // 
            // cbIsProd
            // 
            this.cbIsProd.AutoSize = true;
            this.cbIsProd.Location = new System.Drawing.Point(29, 415);
            this.cbIsProd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbIsProd.Name = "cbIsProd";
            this.cbIsProd.Size = new System.Drawing.Size(133, 20);
            this.cbIsProd.TabIndex = 10;
            this.cbIsProd.Text = "Для рабочей БД";
            this.cbIsProd.UseVisualStyleBackColor = true;
            // 
            // cbIsReg
            // 
            this.cbIsReg.AutoSize = true;
            this.cbIsReg.Location = new System.Drawing.Point(240, 415);
            this.cbIsReg.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbIsReg.Name = "cbIsReg";
            this.cbIsReg.Size = new System.Drawing.Size(155, 20);
            this.cbIsReg.TabIndex = 11;
            this.cbIsReg.Text = "Для реестровой БД";
            this.cbIsReg.UseVisualStyleBackColor = true;
            // 
            // cbIsReport
            // 
            this.cbIsReport.AutoSize = true;
            this.cbIsReport.Location = new System.Drawing.Point(497, 415);
            this.cbIsReport.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbIsReport.Name = "cbIsReport";
            this.cbIsReport.Size = new System.Drawing.Size(139, 20);
            this.cbIsReport.TabIndex = 12;
            this.cbIsReport.Text = "Для отчетной БД";
            this.cbIsReport.UseVisualStyleBackColor = true;
            // 
            // btGenerate
            // 
            this.btGenerate.Location = new System.Drawing.Point(135, 500);
            this.btGenerate.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btGenerate.Name = "btGenerate";
            this.btGenerate.Size = new System.Drawing.Size(239, 36);
            this.btGenerate.TabIndex = 14;
            this.btGenerate.Text = "Сгенерировать скрипт";
            this.btGenerate.UseVisualStyleBackColor = true;
            this.btGenerate.Click += new System.EventHandler(this.btGenerate_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(511, 500);
            this.btCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(240, 36);
            this.btCancel.TabIndex = 15;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btPredicatFill
            // 
            this.btPredicatFill.Location = new System.Drawing.Point(684, 18);
            this.btPredicatFill.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btPredicatFill.Name = "btPredicatFill";
            this.btPredicatFill.Size = new System.Drawing.Size(147, 53);
            this.btPredicatFill.TabIndex = 2;
            this.btPredicatFill.Text = "Выбор полей";
            this.btPredicatFill.UseVisualStyleBackColor = true;
            this.btPredicatFill.Click += new System.EventHandler(this.btPredicatFill_Click);
            // 
            // btIncludeFill
            // 
            this.btIncludeFill.Location = new System.Drawing.Point(684, 86);
            this.btIncludeFill.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btIncludeFill.Name = "btIncludeFill";
            this.btIncludeFill.Size = new System.Drawing.Size(147, 53);
            this.btIncludeFill.TabIndex = 4;
            this.btIncludeFill.Text = "Выбор полей";
            this.btIncludeFill.UseVisualStyleBackColor = true;
            this.btIncludeFill.Click += new System.EventHandler(this.btIncludeFill_Click);
            // 
            // btAutoName
            // 
            this.btAutoName.Location = new System.Drawing.Point(684, 249);
            this.btAutoName.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btAutoName.Name = "btAutoName";
            this.btAutoName.Size = new System.Drawing.Size(176, 38);
            this.btAutoName.TabIndex = 8;
            this.btAutoName.Text = "Сгенерировать";
            this.btAutoName.UseVisualStyleBackColor = true;
            this.btAutoName.Click += new System.EventHandler(this.btAutoName_Click);
            // 
            // cbScriptType
            // 
            this.cbScriptType.FormattingEnabled = true;
            this.cbScriptType.Items.AddRange(new object[] {
            "CREATE",
            "ALTER",
            "DROP"});
            this.cbScriptType.Location = new System.Drawing.Point(135, 449);
            this.cbScriptType.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbScriptType.Name = "cbScriptType";
            this.cbScriptType.Size = new System.Drawing.Size(239, 24);
            this.cbScriptType.TabIndex = 13;
            this.cbScriptType.SelectedIndexChanged += new System.EventHandler(this.cbScriptType_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 453);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(91, 16);
            this.label6.TabIndex = 21;
            this.label6.Text = "Тип скрипта:";
            // 
            // cbIsNullsNotDistinct
            // 
            this.cbIsNullsNotDistinct.AutoSize = true;
            this.cbIsNullsNotDistinct.Location = new System.Drawing.Point(216, 210);
            this.cbIsNullsNotDistinct.Margin = new System.Windows.Forms.Padding(4);
            this.cbIsNullsNotDistinct.Name = "cbIsNullsNotDistinct";
            this.cbIsNullsNotDistinct.Size = new System.Drawing.Size(129, 20);
            this.cbIsNullsNotDistinct.TabIndex = 22;
            this.cbIsNullsNotDistinct.Text = "Nulls Not Distinct";
            this.cbIsNullsNotDistinct.UseVisualStyleBackColor = true;
            // 
            // FormAddIndex
            // 
            this.AcceptButton = this.btGenerate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(897, 575);
            this.Controls.Add(this.cbIsNullsNotDistinct);
            this.Controls.Add(this.tbIndexPredicat);
            this.Controls.Add(this.btPredicatFill);
            this.Controls.Add(this.tbIndexInclude);
            this.Controls.Add(this.btIncludeFill);
            this.Controls.Add(this.tbIndexWhere);
            this.Controls.Add(this.cbIsUnique);
            this.Controls.Add(this.tbIndexName);
            this.Controls.Add(this.btAutoName);
            this.Controls.Add(this.tbIndexToDel);
            this.Controls.Add(this.cbIsProd);
            this.Controls.Add(this.cbIsReg);
            this.Controls.Add(this.cbIsReport);
            this.Controls.Add(this.cbScriptType);
            this.Controls.Add(this.btGenerate);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormAddIndex";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Новый индекс";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAddIndex_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        /// <summary>
        /// tbIndexName
        /// </summary>
        public System.Windows.Forms.TextBox tbIndexName;
        /// <summary>
        /// cbIsUnique
        /// </summary>
        public System.Windows.Forms.CheckBox cbIsUnique;
        private System.Windows.Forms.Label label2;
        /// <summary>
        /// tbIndexPredicat
        /// </summary>
        public System.Windows.Forms.TextBox tbIndexPredicat;
        private System.Windows.Forms.Label label3;
        /// <summary>
        /// tbIndexInclude
        /// </summary>
        public System.Windows.Forms.TextBox tbIndexInclude;
        private System.Windows.Forms.Label label4;
        /// <summary>
        /// tbIndexWhere
        /// </summary>
        public System.Windows.Forms.TextBox tbIndexWhere;
        private System.Windows.Forms.Label label5;
        /// <summary>
        /// tbIndexToDel
        /// </summary>
        public System.Windows.Forms.TextBox tbIndexToDel;
        /// <summary>
        /// cbIsProd
        /// </summary>
        public System.Windows.Forms.CheckBox cbIsProd;
        /// <summary>
        /// cbIsReg
        /// </summary>
        public System.Windows.Forms.CheckBox cbIsReg;
        /// <summary>
        /// cbIsReport
        /// </summary>
        public System.Windows.Forms.CheckBox cbIsReport;
        private System.Windows.Forms.Button btGenerate;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btPredicatFill;
        private System.Windows.Forms.Button btIncludeFill;
        private System.Windows.Forms.Button btAutoName;
        private System.Windows.Forms.Label label6;
        /// <summary>
        /// cbScriptType
        /// </summary>
        public System.Windows.Forms.ComboBox cbScriptType;
        /// <summary>
        /// cbIsNullsNotDistinct
        /// </summary>
        public System.Windows.Forms.CheckBox cbIsNullsNotDistinct;
    }
}