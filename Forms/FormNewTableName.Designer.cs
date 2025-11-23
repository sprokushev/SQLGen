// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    public partial class FormNewTableName
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
            this.tbOldTableName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbNewTableName = new System.Windows.Forms.TextBox();
            this.btReplace = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "Текущее имя таблицы:";
            // 
            // tbOldTableName
            // 
            this.tbOldTableName.Location = new System.Drawing.Point(176, 13);
            this.tbOldTableName.Name = "tbOldTableName";
            this.tbOldTableName.ReadOnly = true;
            this.tbOldTableName.Size = new System.Drawing.Size(280, 20);
            this.tbOldTableName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(22, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Новое имя таблицы:";
            // 
            // tbNewTableName
            // 
            this.tbNewTableName.Location = new System.Drawing.Point(176, 41);
            this.tbNewTableName.Name = "tbNewTableName";
            this.tbNewTableName.Size = new System.Drawing.Size(280, 20);
            this.tbNewTableName.TabIndex = 1;
            // 
            // btReplace
            // 
            this.btReplace.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btReplace.Location = new System.Drawing.Point(124, 86);
            this.btReplace.Name = "btReplace";
            this.btReplace.Size = new System.Drawing.Size(107, 33);
            this.btReplace.TabIndex = 3;
            this.btReplace.Text = "Сменить";
            this.btReplace.UseVisualStyleBackColor = true;
            this.btReplace.Click += new System.EventHandler(this.btReplace_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(310, 86);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(108, 33);
            this.btCancel.TabIndex = 4;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // FormNewTableName
            // 
            this.AcceptButton = this.btReplace;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(517, 142);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btReplace);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbNewTableName);
            this.Controls.Add(this.tbOldTableName);
            this.Controls.Add(this.label1);
            this.Name = "FormNewTableName";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Смена имени таблицы";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormNewTableName_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        /// <summary>
        /// tbOldTableName
        /// </summary>
        public System.Windows.Forms.TextBox tbOldTableName;
        private System.Windows.Forms.Label label2;
        /// <summary>
        /// tbNewTableName
        /// </summary>
        public System.Windows.Forms.TextBox tbNewTableName;
        private System.Windows.Forms.Button btReplace;
        private System.Windows.Forms.Button btCancel;
    }
}