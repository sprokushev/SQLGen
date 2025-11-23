// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormLocalDBList
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
            this.tbPrefix = new System.Windows.Forms.TextBox();
            this.tbSchema = new System.Windows.Forms.TextBox();
            this.tbNick = new System.Windows.Forms.TextBox();
            this.tbDescr = new System.Windows.Forms.TextBox();
            this.btOk = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.ComboBox();
            this.btFind = new System.Windows.Forms.Button();
            this.tbMSSQL = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tbPGSQL = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.tbKey = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.tbModule = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.tbRegion = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.btFromPG = new System.Windows.Forms.Button();
            this.btFromPromedadygea = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // tbPrefix
            // 
            this.tbPrefix.Location = new System.Drawing.Point(202, 38);
            this.tbPrefix.Name = "tbPrefix";
            this.tbPrefix.Size = new System.Drawing.Size(329, 20);
            this.tbPrefix.TabIndex = 3;
            // 
            // tbSchema
            // 
            this.tbSchema.Location = new System.Drawing.Point(202, 64);
            this.tbSchema.Name = "tbSchema";
            this.tbSchema.Size = new System.Drawing.Size(329, 20);
            this.tbSchema.TabIndex = 4;
            // 
            // tbNick
            // 
            this.tbNick.Location = new System.Drawing.Point(202, 90);
            this.tbNick.Name = "tbNick";
            this.tbNick.Size = new System.Drawing.Size(329, 20);
            this.tbNick.TabIndex = 5;
            // 
            // tbDescr
            // 
            this.tbDescr.Location = new System.Drawing.Point(202, 431);
            this.tbDescr.Name = "tbDescr";
            this.tbDescr.Size = new System.Drawing.Size(599, 20);
            this.tbDescr.TabIndex = 10;
            // 
            // btOk
            // 
            this.btOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btOk.Location = new System.Drawing.Point(162, 499);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(171, 31);
            this.btOk.TabIndex = 12;
            this.btOk.Text = "Сгенерировать скрипт";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(439, 499);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(175, 31);
            this.btCancel.TabIndex = 13;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(14, 38);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(114, 15);
            this.label8.TabIndex = 32;
            this.label8.Text = "LocalDBList_Prefix:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(14, 15);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(117, 15);
            this.label7.TabIndex = 29;
            this.label7.Text = "LocalDBList_Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 431);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 15);
            this.label3.TabIndex = 20;
            this.label3.Text = "LocalDBList_Descr:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 90);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 15);
            this.label2.TabIndex = 18;
            this.label2.Text = "LocalDBList_Nick:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 64);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 15);
            this.label1.TabIndex = 14;
            this.label1.Text = "LocalDBList_Schema:";
            // 
            // tbName
            // 
            this.tbName.CausesValidation = false;
            this.tbName.FormattingEnabled = true;
            this.tbName.Location = new System.Drawing.Point(202, 11);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(329, 21);
            this.tbName.TabIndex = 1;
            this.tbName.SelectionChangeCommitted += new System.EventHandler(this.tbName_SelectionChangeCommitted);
            // 
            // btFind
            // 
            this.btFind.Location = new System.Drawing.Point(547, 64);
            this.btFind.Name = "btFind";
            this.btFind.Size = new System.Drawing.Size(105, 46);
            this.btFind.TabIndex = 2;
            this.btFind.Text = "Найти";
            this.btFind.UseVisualStyleBackColor = true;
            this.btFind.Click += new System.EventHandler(this.btFind_Click);
            // 
            // tbMSSQL
            // 
            this.tbMSSQL.AcceptsReturn = true;
            this.tbMSSQL.Location = new System.Drawing.Point(202, 116);
            this.tbMSSQL.Multiline = true;
            this.tbMSSQL.Name = "tbMSSQL";
            this.tbMSSQL.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbMSSQL.Size = new System.Drawing.Size(602, 116);
            this.tbMSSQL.TabIndex = 6;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(14, 116);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(149, 15);
            this.label10.TabIndex = 36;
            this.label10.Text = "RegionalLocalDBList_sql:";
            // 
            // tbPGSQL
            // 
            this.tbPGSQL.AcceptsReturn = true;
            this.tbPGSQL.Location = new System.Drawing.Point(202, 238);
            this.tbPGSQL.Multiline = true;
            this.tbPGSQL.Name = "tbPGSQL";
            this.tbPGSQL.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.tbPGSQL.Size = new System.Drawing.Size(602, 129);
            this.tbPGSQL.TabIndex = 7;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(14, 238);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(163, 15);
            this.label11.TabIndex = 38;
            this.label11.Text = "RegionalLocalDBList_pgsql:";
            // 
            // tbKey
            // 
            this.tbKey.Location = new System.Drawing.Point(202, 380);
            this.tbKey.Name = "tbKey";
            this.tbKey.Size = new System.Drawing.Size(329, 20);
            this.tbKey.TabIndex = 8;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(14, 380);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(103, 15);
            this.label12.TabIndex = 40;
            this.label12.Text = "LocalDBList_Key:";
            // 
            // tbModule
            // 
            this.tbModule.Location = new System.Drawing.Point(202, 405);
            this.tbModule.Name = "tbModule";
            this.tbModule.Size = new System.Drawing.Size(329, 20);
            this.tbModule.TabIndex = 9;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(14, 405);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(125, 15);
            this.label13.TabIndex = 42;
            this.label13.Text = "LocalDBList_Module:";
            // 
            // tbRegion
            // 
            this.tbRegion.Location = new System.Drawing.Point(202, 457);
            this.tbRegion.Name = "tbRegion";
            this.tbRegion.Size = new System.Drawing.Size(329, 20);
            this.tbRegion.TabIndex = 11;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(14, 457);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(67, 15);
            this.label14.TabIndex = 44;
            this.label14.Text = "Region_id:";
            // 
            // btFromPG
            // 
            this.btFromPG.Location = new System.Drawing.Point(17, 256);
            this.btFromPG.Name = "btFromPG";
            this.btFromPG.Size = new System.Drawing.Size(160, 51);
            this.btFromPG.TabIndex = 45;
            this.btFromPG.Text = "Обновить из promedtest ПГ";
            this.btFromPG.UseVisualStyleBackColor = true;
            this.btFromPG.Click += new System.EventHandler(this.btFromPG_Click);
            // 
            // btFromPromedadygea
            // 
            this.btFromPromedadygea.Location = new System.Drawing.Point(17, 313);
            this.btFromPromedadygea.Name = "btFromPromedadygea";
            this.btFromPromedadygea.Size = new System.Drawing.Size(160, 54);
            this.btFromPromedadygea.TabIndex = 46;
            this.btFromPromedadygea.Text = "Обновить из promedadygea";
            this.btFromPromedadygea.UseVisualStyleBackColor = true;
            this.btFromPromedadygea.Click += new System.EventHandler(this.btFromPromedadygea_Click);
            // 
            // FormLocalDBList
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(849, 560);
            this.Controls.Add(this.btFromPromedadygea);
            this.Controls.Add(this.btFromPG);
            this.Controls.Add(this.tbRegion);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.tbModule);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.tbKey);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.tbPGSQL);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.tbMSSQL);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.btFind);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.tbPrefix);
            this.Controls.Add(this.tbSchema);
            this.Controls.Add(this.tbNick);
            this.Controls.Add(this.tbDescr);
            this.Controls.Add(this.btOk);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "FormLocalDBList";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Скрипт для stg.LocalDBList и stg.RegionalLocalDBList";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormLocalDBList_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        /// <summary>
        /// tbPrefix
        /// </summary>
        public System.Windows.Forms.TextBox tbPrefix;
        /// <summary>
        /// tbSchema
        /// </summary>
        public System.Windows.Forms.TextBox tbSchema;
        /// <summary>
        /// tbNick
        /// </summary>
        public System.Windows.Forms.TextBox tbNick;
        /// <summary>
        /// tbDescr
        /// </summary>
        public System.Windows.Forms.TextBox tbDescr;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        /// <summary>
        /// tbName
        /// </summary>
        public System.Windows.Forms.ComboBox tbName;
        /// <summary>
        /// tbMSSQL
        /// </summary>
        public System.Windows.Forms.TextBox tbMSSQL;
        private System.Windows.Forms.Label label10;
        /// <summary>
        /// tbPGSQL
        /// </summary>
        public System.Windows.Forms.TextBox tbPGSQL;
        private System.Windows.Forms.Label label11;
        /// <summary>
        /// tbKey
        /// </summary>
        public System.Windows.Forms.TextBox tbKey;
        private System.Windows.Forms.Label label12;
        /// <summary>
        /// tbModule
        /// </summary>
        public System.Windows.Forms.TextBox tbModule;
        private System.Windows.Forms.Label label13;
        /// <summary>
        /// tbRegion
        /// </summary>
        public System.Windows.Forms.TextBox tbRegion;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button btFromPG;
        private System.Windows.Forms.Button btFind;
        private System.Windows.Forms.Button btFromPromedadygea;
    }
}