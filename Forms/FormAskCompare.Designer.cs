// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormAskCompare
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
            this.btOverwrite = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.btAppend = new System.Windows.Forms.Button();
            this.tbGITFilename = new System.Windows.Forms.TextBox();
            this.lbDEVNameObject = new System.Windows.Forms.Label();
            this.btCompare = new System.Windows.Forms.Button();
            this.tbChangesetName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btOverwrite
            // 
            this.btOverwrite.DialogResult = System.Windows.Forms.DialogResult.Yes;
            this.btOverwrite.Location = new System.Drawing.Point(249, 84);
            this.btOverwrite.Margin = new System.Windows.Forms.Padding(4);
            this.btOverwrite.Name = "btOverwrite";
            this.btOverwrite.Size = new System.Drawing.Size(179, 66);
            this.btOverwrite.TabIndex = 16;
            this.btOverwrite.Text = "Перезаписать";
            this.btOverwrite.UseVisualStyleBackColor = true;
            this.btOverwrite.Click += new System.EventHandler(this.btOwerwrite_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(673, 84);
            this.btCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(156, 66);
            this.btCancel.TabIndex = 19;
            this.btCancel.Text = "Оставить без изменений";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btAppend
            // 
            this.btAppend.DialogResult = System.Windows.Forms.DialogResult.No;
            this.btAppend.Location = new System.Drawing.Point(460, 84);
            this.btAppend.Margin = new System.Windows.Forms.Padding(4);
            this.btAppend.Name = "btAppend";
            this.btAppend.Size = new System.Drawing.Size(173, 66);
            this.btAppend.TabIndex = 17;
            this.btAppend.Text = "Добавить в конец файла";
            this.btAppend.UseVisualStyleBackColor = true;
            this.btAppend.Click += new System.EventHandler(this.btAppend_Click);
            // 
            // tbGITFilename
            // 
            this.tbGITFilename.Enabled = false;
            this.tbGITFilename.Location = new System.Drawing.Point(162, 15);
            this.tbGITFilename.Margin = new System.Windows.Forms.Padding(4);
            this.tbGITFilename.Name = "tbGITFilename";
            this.tbGITFilename.Size = new System.Drawing.Size(694, 22);
            this.tbGITFilename.TabIndex = 26;
            // 
            // lbDEVNameObject
            // 
            this.lbDEVNameObject.AutoSize = true;
            this.lbDEVNameObject.Location = new System.Drawing.Point(46, 21);
            this.lbDEVNameObject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbDEVNameObject.Name = "lbDEVNameObject";
            this.lbDEVNameObject.Size = new System.Drawing.Size(45, 16);
            this.lbDEVNameObject.TabIndex = 43;
            this.lbDEVNameObject.Text = "Файл:";
            // 
            // btCompare
            // 
            this.btCompare.Location = new System.Drawing.Point(50, 84);
            this.btCompare.Margin = new System.Windows.Forms.Padding(4);
            this.btCompare.Name = "btCompare";
            this.btCompare.Size = new System.Drawing.Size(169, 66);
            this.btCompare.TabIndex = 19;
            this.btCompare.Text = "Сравнить c GIT";
            this.btCompare.UseVisualStyleBackColor = true;
            this.btCompare.Click += new System.EventHandler(this.btCompare_Click);
            // 
            // tbChangesetName
            // 
            this.tbChangesetName.Enabled = false;
            this.tbChangesetName.Location = new System.Drawing.Point(162, 45);
            this.tbChangesetName.Margin = new System.Windows.Forms.Padding(4);
            this.tbChangesetName.Name = "tbChangesetName";
            this.tbChangesetName.Size = new System.Drawing.Size(694, 22);
            this.tbChangesetName.TabIndex = 26;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(46, 48);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 16);
            this.label2.TabIndex = 43;
            this.label2.Text = "Changeset:";
            // 
            // FormAskCompare
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(903, 231);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lbDEVNameObject);
            this.Controls.Add(this.tbChangesetName);
            this.Controls.Add(this.tbGITFilename);
            this.Controls.Add(this.btAppend);
            this.Controls.Add(this.btOverwrite);
            this.Controls.Add(this.btCompare);
            this.Controls.Add(this.btCancel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormAskCompare";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Файл (changeset) уже существует, но отличается";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAskCompare_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btOverwrite;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btAppend;
        /// <summary>
        /// tbGITFilename
        /// </summary>
        public System.Windows.Forms.TextBox tbGITFilename;
        private System.Windows.Forms.Label lbDEVNameObject;
        private System.Windows.Forms.Button btCompare;
        /// <summary>
        /// tbChangesetName
        /// </summary>
        public System.Windows.Forms.TextBox tbChangesetName;
        private System.Windows.Forms.Label label2;
    }
}