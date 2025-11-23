// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    public partial class FormNewBranch
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
            this.label2 = new System.Windows.Forms.Label();
            this.tbNewBranchName = new System.Windows.Forms.TextBox();
            this.btCreate = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.tbParentBranchName = new System.Windows.Forms.TextBox();
            this.btChooseParent = new System.Windows.Forms.Button();
            this.btChoose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 22);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Имя ветки:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 68);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "Создаем от ветки:";
            // 
            // tbNewBranchName
            // 
            this.tbNewBranchName.Location = new System.Drawing.Point(185, 19);
            this.tbNewBranchName.Margin = new System.Windows.Forms.Padding(4);
            this.tbNewBranchName.Name = "tbNewBranchName";
            this.tbNewBranchName.Size = new System.Drawing.Size(315, 22);
            this.tbNewBranchName.TabIndex = 3;
            // 
            // btCreate
            // 
            this.btCreate.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btCreate.Location = new System.Drawing.Point(31, 131);
            this.btCreate.Margin = new System.Windows.Forms.Padding(4);
            this.btCreate.Name = "btCreate";
            this.btCreate.Size = new System.Drawing.Size(297, 41);
            this.btCreate.TabIndex = 1;
            this.btCreate.Text = "Создать или переключиться";
            this.btCreate.UseVisualStyleBackColor = true;
            this.btCreate.Click += new System.EventHandler(this.btCreate_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(356, 131);
            this.btCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(144, 41);
            this.btCancel.TabIndex = 2;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // tbParentBranchName
            // 
            this.tbParentBranchName.Location = new System.Drawing.Point(185, 68);
            this.tbParentBranchName.Margin = new System.Windows.Forms.Padding(4);
            this.tbParentBranchName.Name = "tbParentBranchName";
            this.tbParentBranchName.Size = new System.Drawing.Size(315, 22);
            this.tbParentBranchName.TabIndex = 5;
            // 
            // btChooseParent
            // 
            this.btChooseParent.Location = new System.Drawing.Point(519, 63);
            this.btChooseParent.Margin = new System.Windows.Forms.Padding(4);
            this.btChooseParent.Name = "btChooseParent";
            this.btChooseParent.Size = new System.Drawing.Size(122, 41);
            this.btChooseParent.TabIndex = 6;
            this.btChooseParent.Text = "Выбрать";
            this.btChooseParent.UseVisualStyleBackColor = true;
            this.btChooseParent.Click += new System.EventHandler(this.btChooseParent_Click);
            // 
            // btChoose
            // 
            this.btChoose.Location = new System.Drawing.Point(519, 13);
            this.btChoose.Margin = new System.Windows.Forms.Padding(4);
            this.btChoose.Name = "btChoose";
            this.btChoose.Size = new System.Drawing.Size(122, 41);
            this.btChoose.TabIndex = 4;
            this.btChoose.Text = "Выбрать";
            this.btChoose.UseVisualStyleBackColor = true;
            this.btChoose.Click += new System.EventHandler(this.btChoose_Click);
            // 
            // FormNewBranch
            // 
            this.AcceptButton = this.btCreate;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(689, 251);
            this.Controls.Add(this.tbNewBranchName);
            this.Controls.Add(this.btChoose);
            this.Controls.Add(this.btChooseParent);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btCreate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.tbParentBranchName);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormNewBranch";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Создать новую ветку или переключиться на текущую";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormNewBranch_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        /// <summary>
        /// tbNewTableName
        /// </summary>
        public System.Windows.Forms.TextBox tbNewBranchName;
        private System.Windows.Forms.Button btCreate;
        private System.Windows.Forms.Button btCancel;
        /// <summary>
        /// tbParentBranchName
        /// </summary>
        public System.Windows.Forms.TextBox tbParentBranchName;
        private System.Windows.Forms.Button btChooseParent;
        private System.Windows.Forms.Button btChoose;
    }
}