// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

namespace SQLGen
{
    partial class FormSplitFile
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
            this.btCancel = new System.Windows.Forms.Button();
            this.btSplit = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cbUnit = new System.Windows.Forms.ComboBox();
            this.tbSize = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rbKeywords = new System.Windows.Forms.RadioButton();
            this.rbChar = new System.Windows.Forms.RadioButton();
            this.rbLine = new System.Windows.Forms.RadioButton();
            this.rbByte = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rbANSI = new System.Windows.Forms.RadioButton();
            this.rbUTF16LE = new System.Windows.Forms.RadioButton();
            this.rbUTF8BOM = new System.Windows.Forms.RadioButton();
            this.rbUTF8 = new System.Windows.Forms.RadioButton();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbFilename
            // 
            this.tbFilename.Location = new System.Drawing.Point(81, 12);
            this.tbFilename.Name = "tbFilename";
            this.tbFilename.Size = new System.Drawing.Size(573, 20);
            this.tbFilename.TabIndex = 1;
            // 
            // btOpen
            // 
            this.btOpen.Location = new System.Drawing.Point(685, 10);
            this.btOpen.Name = "btOpen";
            this.btOpen.Size = new System.Drawing.Size(137, 29);
            this.btOpen.TabIndex = 2;
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
            this.label1.TabIndex = 9;
            this.label1.Text = "Файл:";
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(516, 179);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(163, 30);
            this.btCancel.TabIndex = 7;
            this.btCancel.Text = "Отмена";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btSplit
            // 
            this.btSplit.Location = new System.Drawing.Point(246, 180);
            this.btSplit.Name = "btSplit";
            this.btSplit.Size = new System.Drawing.Size(147, 29);
            this.btSplit.TabIndex = 6;
            this.btSplit.Text = "Разделить";
            this.btSplit.UseVisualStyleBackColor = true;
            this.btSplit.Click += new System.EventHandler(this.btSplit_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 44);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(90, 15);
            this.label3.TabIndex = 16;
            this.label3.Text = "Размер части:";
            // 
            // cbUnit
            // 
            this.cbUnit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUnit.FormattingEnabled = true;
            this.cbUnit.Items.AddRange(new object[] {
            "Байт",
            "Килобайт",
            "Мегабайт"});
            this.cbUnit.Location = new System.Drawing.Point(232, 41);
            this.cbUnit.Name = "cbUnit";
            this.cbUnit.Size = new System.Drawing.Size(151, 21);
            this.cbUnit.TabIndex = 4;
            // 
            // tbSize
            // 
            this.tbSize.Location = new System.Drawing.Point(104, 41);
            this.tbSize.Name = "tbSize";
            this.tbSize.Size = new System.Drawing.Size(122, 20);
            this.tbSize.TabIndex = 3;
            this.tbSize.Text = "30";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rbKeywords);
            this.groupBox1.Controls.Add(this.rbChar);
            this.groupBox1.Controls.Add(this.rbLine);
            this.groupBox1.Controls.Add(this.rbByte);
            this.groupBox1.Location = new System.Drawing.Point(18, 60);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(857, 68);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            // 
            // rbKeywords
            // 
            this.rbKeywords.AutoSize = true;
            this.rbKeywords.Location = new System.Drawing.Point(14, 39);
            this.rbKeywords.Name = "rbKeywords";
            this.rbKeywords.Size = new System.Drawing.Size(462, 19);
            this.rbKeywords.TabIndex = 3;
            this.rbKeywords.TabStop = true;
            this.rbKeywords.Text = "С учетом начала/окончания INSERT/UPDATE (экспериментальный режим!)";
            this.rbKeywords.UseVisualStyleBackColor = true;
            // 
            // rbChar
            // 
            this.rbChar.AutoSize = true;
            this.rbChar.Location = new System.Drawing.Point(237, 16);
            this.rbChar.Name = "rbChar";
            this.rbChar.Size = new System.Drawing.Size(333, 19);
            this.rbChar.TabIndex = 2;
            this.rbChar.TabStop = true;
            this.rbChar.Text = "Разделить точно по размеру, но с учетом кодировки";
            this.rbChar.UseVisualStyleBackColor = true;
            // 
            // rbLine
            // 
            this.rbLine.AutoSize = true;
            this.rbLine.Checked = true;
            this.rbLine.Location = new System.Drawing.Point(588, 16);
            this.rbLine.Name = "rbLine";
            this.rbLine.Size = new System.Drawing.Size(210, 19);
            this.rbLine.TabIndex = 1;
            this.rbLine.TabStop = true;
            this.rbLine.Text = "Разделить по окончанию строк";
            this.rbLine.UseVisualStyleBackColor = true;
            // 
            // rbByte
            // 
            this.rbByte.AutoSize = true;
            this.rbByte.Location = new System.Drawing.Point(14, 16);
            this.rbByte.Name = "rbByte";
            this.rbByte.Size = new System.Drawing.Size(196, 19);
            this.rbByte.TabIndex = 0;
            this.rbByte.Text = "Разделить точно по размеру";
            this.rbByte.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rbANSI);
            this.groupBox2.Controls.Add(this.rbUTF16LE);
            this.groupBox2.Controls.Add(this.rbUTF8BOM);
            this.groupBox2.Controls.Add(this.rbUTF8);
            this.groupBox2.Location = new System.Drawing.Point(18, 134);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Size = new System.Drawing.Size(702, 28);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            // 
            // rbANSI
            // 
            this.rbANSI.AutoSize = true;
            this.rbANSI.Location = new System.Drawing.Point(297, 7);
            this.rbANSI.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbANSI.Name = "rbANSI";
            this.rbANSI.Size = new System.Drawing.Size(94, 19);
            this.rbANSI.TabIndex = 3;
            this.rbANSI.Text = "ANSI (1251)";
            this.rbANSI.UseVisualStyleBackColor = true;
            // 
            // rbUTF16LE
            // 
            this.rbUTF16LE.AutoSize = true;
            this.rbUTF16LE.Location = new System.Drawing.Point(198, 8);
            this.rbUTF16LE.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbUTF16LE.Name = "rbUTF16LE";
            this.rbUTF16LE.Size = new System.Drawing.Size(83, 19);
            this.rbUTF16LE.TabIndex = 2;
            this.rbUTF16LE.Text = "UTF16 LE";
            this.rbUTF16LE.UseVisualStyleBackColor = true;
            // 
            // rbUTF8BOM
            // 
            this.rbUTF8BOM.AutoSize = true;
            this.rbUTF8BOM.Location = new System.Drawing.Point(85, 7);
            this.rbUTF8BOM.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbUTF8BOM.Name = "rbUTF8BOM";
            this.rbUTF8BOM.Size = new System.Drawing.Size(98, 19);
            this.rbUTF8BOM.TabIndex = 1;
            this.rbUTF8BOM.Text = "UTF8 с BOM";
            this.rbUTF8BOM.UseVisualStyleBackColor = true;
            // 
            // rbUTF8
            // 
            this.rbUTF8.AutoSize = true;
            this.rbUTF8.Checked = true;
            this.rbUTF8.Location = new System.Drawing.Point(14, 7);
            this.rbUTF8.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.rbUTF8.Name = "rbUTF8";
            this.rbUTF8.Size = new System.Drawing.Size(58, 19);
            this.rbUTF8.TabIndex = 0;
            this.rbUTF8.TabStop = true;
            this.rbUTF8.Text = "UTF8";
            this.rbUTF8.UseVisualStyleBackColor = true;
            // 
            // FormSplitFile
            // 
            this.AcceptButton = this.btSplit;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(909, 233);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.cbUnit);
            this.Controls.Add(this.tbSize);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btSplit);
            this.Controls.Add(this.tbFilename);
            this.Controls.Add(this.btOpen);
            this.Controls.Add(this.label1);
            this.Name = "FormSplitFile";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Разделить файл на части";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormSplitFile_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btSplit;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cbUnit;
        private System.Windows.Forms.TextBox tbSize;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton rbLine;
        private System.Windows.Forms.RadioButton rbByte;
        private System.Windows.Forms.RadioButton rbChar;
        private System.Windows.Forms.RadioButton rbKeywords;
        private System.Windows.Forms.GroupBox groupBox2;
        /// <summary>
        /// rbANSI
        /// </summary>
        public System.Windows.Forms.RadioButton rbANSI;
        /// <summary>
        /// rbUTF16LE
        /// </summary>
        public System.Windows.Forms.RadioButton rbUTF16LE;
        /// <summary>
        /// rbUTF8BOM
        /// </summary>
        public System.Windows.Forms.RadioButton rbUTF8BOM;
        /// <summary>
        /// rbUTF8
        /// </summary>
        public System.Windows.Forms.RadioButton rbUTF8;
    }
}