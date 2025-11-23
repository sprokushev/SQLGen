// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen
{
    partial class FormAddScript
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
            this.tbScriptFilename = new System.Windows.Forms.TextBox();
            this.btOpen = new System.Windows.Forms.Button();
            this.btAdd = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.tbGITFolder = new System.Windows.Forms.TextBox();
            this.btAddAndNext = new System.Windows.Forms.Button();
            this.btNext = new System.Windows.Forms.Button();
            this.isAddToGIT = new System.Windows.Forms.CheckBox();
            this.tbGITFilename = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cbGITProject = new System.Windows.Forms.ComboBox();
            this.cbTypeObject = new System.Windows.Forms.ComboBox();
            this.cbShemaObject = new System.Windows.Forms.ComboBox();
            this.cbGITNameObject = new System.Windows.Forms.ComboBox();
            this.lbGITNameObject = new System.Windows.Forms.Label();
            this.lbShemaObject = new System.Windows.Forms.Label();
            this.lbTypeObject = new System.Windows.Forms.Label();
            this.lbGITProject = new System.Windows.Forms.Label();
            this.isAddToDEV = new System.Windows.Forms.CheckBox();
            this.tbBranch = new System.Windows.Forms.TextBox();
            this.cbDEVNameObject = new System.Windows.Forms.ComboBox();
            this.lbDEVNameObject = new System.Windows.Forms.Label();
            this.lbBranch = new System.Windows.Forms.Label();
            this.lbDEVProject = new System.Windows.Forms.Label();
            this.cbDEVProject = new System.Windows.Forms.ComboBox();
            this.btCompare = new System.Windows.Forms.Button();
            this.cbCheck = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 47);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Файл:";
            // 
            // tbScriptFilename
            // 
            this.tbScriptFilename.Location = new System.Drawing.Point(76, 43);
            this.tbScriptFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbScriptFilename.Name = "tbScriptFilename";
            this.tbScriptFilename.ReadOnly = true;
            this.tbScriptFilename.Size = new System.Drawing.Size(791, 22);
            this.tbScriptFilename.TabIndex = 2;
            this.tbScriptFilename.TabStop = false;
            // 
            // btOpen
            // 
            this.btOpen.Location = new System.Drawing.Point(899, 11);
            this.btOpen.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btOpen.Name = "btOpen";
            this.btOpen.Size = new System.Drawing.Size(184, 57);
            this.btOpen.TabIndex = 3;
            this.btOpen.Text = "Выбрать файл";
            this.btOpen.UseVisualStyleBackColor = true;
            this.btOpen.Click += new System.EventHandler(this.btOpen_Click);
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(20, 345);
            this.btAdd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(179, 66);
            this.btAdd.TabIndex = 16;
            this.btAdd.Text = "Добавить и завершить";
            this.btAdd.UseVisualStyleBackColor = true;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(727, 345);
            this.btCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(156, 66);
            this.btCancel.TabIndex = 19;
            this.btCancel.Text = "Завершить";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(16, 16);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(88, 16);
            this.label7.TabIndex = 14;
            this.label7.Text = "Каталог GIT:";
            // 
            // tbGITFolder
            // 
            this.tbGITFolder.Location = new System.Drawing.Point(132, 11);
            this.tbGITFolder.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbGITFolder.Name = "tbGITFolder";
            this.tbGITFolder.ReadOnly = true;
            this.tbGITFolder.Size = new System.Drawing.Size(735, 22);
            this.tbGITFolder.TabIndex = 1;
            this.tbGITFolder.TabStop = false;
            // 
            // btAddAndNext
            // 
            this.btAddAndNext.Location = new System.Drawing.Point(233, 345);
            this.btAddAndNext.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btAddAndNext.Name = "btAddAndNext";
            this.btAddAndNext.Size = new System.Drawing.Size(216, 66);
            this.btAddAndNext.TabIndex = 17;
            this.btAddAndNext.Text = "Добавить и перейти к следующему файлу";
            this.btAddAndNext.UseVisualStyleBackColor = true;
            this.btAddAndNext.Click += new System.EventHandler(this.btAddAndNext_Click);
            // 
            // btNext
            // 
            this.btNext.Location = new System.Drawing.Point(477, 345);
            this.btNext.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btNext.Name = "btNext";
            this.btNext.Size = new System.Drawing.Size(208, 66);
            this.btNext.TabIndex = 18;
            this.btNext.Text = "Пропустить и перейти к следующему файлу";
            this.btNext.UseVisualStyleBackColor = true;
            this.btNext.Click += new System.EventHandler(this.btNext_Click);
            // 
            // isAddToGIT
            // 
            this.isAddToGIT.AutoSize = true;
            this.isAddToGIT.Location = new System.Drawing.Point(587, 86);
            this.isAddToGIT.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.isAddToGIT.Name = "isAddToGIT";
            this.isAddToGIT.Size = new System.Drawing.Size(310, 20);
            this.isAddToGIT.TabIndex = 32;
            this.isAddToGIT.Text = "Добавлять в версионный проект (\"старый\")";
            this.isAddToGIT.UseVisualStyleBackColor = true;
            this.isAddToGIT.CheckedChanged += new System.EventHandler(this.isAddToGIT_CheckedChanged);
            // 
            // tbGITFilename
            // 
            this.tbGITFilename.Location = new System.Drawing.Point(347, 286);
            this.tbGITFilename.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbGITFilename.Name = "tbGITFilename";
            this.tbGITFilename.Size = new System.Drawing.Size(737, 22);
            this.tbGITFilename.TabIndex = 26;
            this.tbGITFilename.Leave += new System.EventHandler(this.tbGITFilename_Leave);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(17, 286);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(245, 16);
            this.label5.TabIndex = 31;
            this.label5.Text = "Имя файла для GIT, без расширения:";
            // 
            // cbGITProject
            // 
            this.cbGITProject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbGITProject.FormattingEnabled = true;
            this.cbGITProject.Location = new System.Drawing.Point(705, 126);
            this.cbGITProject.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbGITProject.Name = "cbGITProject";
            this.cbGITProject.Size = new System.Drawing.Size(380, 24);
            this.cbGITProject.TabIndex = 22;
            this.cbGITProject.SelectedIndexChanged += new System.EventHandler(this.cbGITProject_SelectedIndexChanged);
            this.cbGITProject.Leave += new System.EventHandler(this.cbGITProject_Leave);
            // 
            // cbTypeObject
            // 
            this.cbTypeObject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTypeObject.FormattingEnabled = true;
            this.cbTypeObject.Location = new System.Drawing.Point(164, 187);
            this.cbTypeObject.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbTypeObject.MaxDropDownItems = 12;
            this.cbTypeObject.Name = "cbTypeObject";
            this.cbTypeObject.Size = new System.Drawing.Size(380, 24);
            this.cbTypeObject.TabIndex = 23;
            this.cbTypeObject.SelectedIndexChanged += new System.EventHandler(this.cbTypeObject_SelectedIndexChanged);
            this.cbTypeObject.Leave += new System.EventHandler(this.cbTypeObject_Leave);
            // 
            // cbShemaObject
            // 
            this.cbShemaObject.FormattingEnabled = true;
            this.cbShemaObject.Location = new System.Drawing.Point(164, 222);
            this.cbShemaObject.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbShemaObject.Name = "cbShemaObject";
            this.cbShemaObject.Size = new System.Drawing.Size(380, 24);
            this.cbShemaObject.TabIndex = 24;
            this.cbShemaObject.Leave += new System.EventHandler(this.cbShemaObject_Leave);
            // 
            // cbGITNameObject
            // 
            this.cbGITNameObject.FormattingEnabled = true;
            this.cbGITNameObject.Location = new System.Drawing.Point(705, 254);
            this.cbGITNameObject.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbGITNameObject.Name = "cbGITNameObject";
            this.cbGITNameObject.Size = new System.Drawing.Size(380, 24);
            this.cbGITNameObject.TabIndex = 25;
            this.cbGITNameObject.Leave += new System.EventHandler(this.cbGITNameObject_Leave);
            // 
            // lbGITNameObject
            // 
            this.lbGITNameObject.AutoSize = true;
            this.lbGITNameObject.Location = new System.Drawing.Point(584, 256);
            this.lbGITNameObject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbGITNameObject.Name = "lbGITNameObject";
            this.lbGITNameObject.Size = new System.Drawing.Size(94, 16);
            this.lbGITNameObject.TabIndex = 30;
            this.lbGITNameObject.Text = "Имя объекта:";
            // 
            // lbShemaObject
            // 
            this.lbShemaObject.AutoSize = true;
            this.lbShemaObject.Location = new System.Drawing.Point(17, 224);
            this.lbShemaObject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbShemaObject.Name = "lbShemaObject";
            this.lbShemaObject.Size = new System.Drawing.Size(50, 16);
            this.lbShemaObject.TabIndex = 29;
            this.lbShemaObject.Text = "Схема:";
            // 
            // lbTypeObject
            // 
            this.lbTypeObject.AutoSize = true;
            this.lbTypeObject.Location = new System.Drawing.Point(17, 190);
            this.lbTypeObject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbTypeObject.Name = "lbTypeObject";
            this.lbTypeObject.Size = new System.Drawing.Size(93, 16);
            this.lbTypeObject.TabIndex = 28;
            this.lbTypeObject.Text = "Тип объекта:";
            // 
            // lbGITProject
            // 
            this.lbGITProject.AutoSize = true;
            this.lbGITProject.Location = new System.Drawing.Point(583, 129);
            this.lbGITProject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbGITProject.Name = "lbGITProject";
            this.lbGITProject.Size = new System.Drawing.Size(83, 16);
            this.lbGITProject.TabIndex = 27;
            this.lbGITProject.Text = "Проект GIT:";
            // 
            // isAddToDEV
            // 
            this.isAddToDEV.AutoSize = true;
            this.isAddToDEV.Location = new System.Drawing.Point(20, 86);
            this.isAddToDEV.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.isAddToDEV.Name = "isAddToDEV";
            this.isAddToDEV.Size = new System.Drawing.Size(302, 20);
            this.isAddToDEV.TabIndex = 45;
            this.isAddToDEV.Text = "Добавлять в проект разработки (\"новый\")";
            this.isAddToDEV.UseVisualStyleBackColor = true;
            this.isAddToDEV.CheckedChanged += new System.EventHandler(this.isAddToDEV_CheckedChanged);
            // 
            // tbBranch
            // 
            this.tbBranch.Location = new System.Drawing.Point(164, 158);
            this.tbBranch.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.tbBranch.Name = "tbBranch";
            this.tbBranch.ReadOnly = true;
            this.tbBranch.Size = new System.Drawing.Size(380, 22);
            this.tbBranch.TabIndex = 35;
            this.tbBranch.TabStop = false;
            // 
            // cbDEVNameObject
            // 
            this.cbDEVNameObject.FormattingEnabled = true;
            this.cbDEVNameObject.Location = new System.Drawing.Point(163, 254);
            this.cbDEVNameObject.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbDEVNameObject.Name = "cbDEVNameObject";
            this.cbDEVNameObject.Size = new System.Drawing.Size(380, 24);
            this.cbDEVNameObject.TabIndex = 38;
            this.cbDEVNameObject.Leave += new System.EventHandler(this.cbDEVNameObject_Leave);
            // 
            // lbDEVNameObject
            // 
            this.lbDEVNameObject.AutoSize = true;
            this.lbDEVNameObject.Location = new System.Drawing.Point(16, 257);
            this.lbDEVNameObject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbDEVNameObject.Name = "lbDEVNameObject";
            this.lbDEVNameObject.Size = new System.Drawing.Size(94, 16);
            this.lbDEVNameObject.TabIndex = 43;
            this.lbDEVNameObject.Text = "Имя объекта:";
            // 
            // lbBranch
            // 
            this.lbBranch.AutoSize = true;
            this.lbBranch.Location = new System.Drawing.Point(16, 161);
            this.lbBranch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbBranch.Name = "lbBranch";
            this.lbBranch.Size = new System.Drawing.Size(49, 16);
            this.lbBranch.TabIndex = 39;
            this.lbBranch.Text = "Ветка:";
            // 
            // lbDEVProject
            // 
            this.lbDEVProject.AutoSize = true;
            this.lbDEVProject.Location = new System.Drawing.Point(16, 129);
            this.lbDEVProject.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lbDEVProject.Name = "lbDEVProject";
            this.lbDEVProject.Size = new System.Drawing.Size(89, 16);
            this.lbDEVProject.TabIndex = 34;
            this.lbDEVProject.Text = "Проект DEV:";
            // 
            // cbDEVProject
            // 
            this.cbDEVProject.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDEVProject.FormattingEnabled = true;
            this.cbDEVProject.Location = new System.Drawing.Point(164, 121);
            this.cbDEVProject.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.cbDEVProject.Name = "cbDEVProject";
            this.cbDEVProject.Size = new System.Drawing.Size(380, 24);
            this.cbDEVProject.TabIndex = 22;
            this.cbDEVProject.SelectedValueChanged += new System.EventHandler(this.cbDEVProject_SelectedValueChanged);
            this.cbDEVProject.Leave += new System.EventHandler(this.cbDEVProject_Leave);
            // 
            // btCompare
            // 
            this.btCompare.Location = new System.Drawing.Point(916, 345);
            this.btCompare.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btCompare.Name = "btCompare";
            this.btCompare.Size = new System.Drawing.Size(169, 66);
            this.btCompare.TabIndex = 19;
            this.btCompare.Text = "Сравнить c GIT";
            this.btCompare.UseVisualStyleBackColor = true;
            this.btCompare.Click += new System.EventHandler(this.btCompare_Click);
            // 
            // cbCheck
            // 
            this.cbCheck.AutoSize = true;
            this.cbCheck.Checked = true;
            this.cbCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbCheck.Location = new System.Drawing.Point(24, 429);
            this.cbCheck.Name = "cbCheck";
            this.cbCheck.Size = new System.Drawing.Size(148, 20);
            this.cbCheck.TabIndex = 46;
            this.cbCheck.Text = "Проверять скрипт";
            this.cbCheck.UseVisualStyleBackColor = true;
            // 
            // FormAddScript
            // 
            this.AcceptButton = this.btAdd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(1147, 466);
            this.Controls.Add(this.cbCheck);
            this.Controls.Add(this.isAddToDEV);
            this.Controls.Add(this.tbBranch);
            this.Controls.Add(this.cbDEVNameObject);
            this.Controls.Add(this.lbDEVNameObject);
            this.Controls.Add(this.lbBranch);
            this.Controls.Add(this.lbDEVProject);
            this.Controls.Add(this.isAddToGIT);
            this.Controls.Add(this.tbGITFilename);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbDEVProject);
            this.Controls.Add(this.cbGITProject);
            this.Controls.Add(this.cbTypeObject);
            this.Controls.Add(this.cbShemaObject);
            this.Controls.Add(this.cbGITNameObject);
            this.Controls.Add(this.lbGITNameObject);
            this.Controls.Add(this.lbShemaObject);
            this.Controls.Add(this.lbTypeObject);
            this.Controls.Add(this.lbGITProject);
            this.Controls.Add(this.tbGITFolder);
            this.Controls.Add(this.btNext);
            this.Controls.Add(this.btAddAndNext);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbScriptFilename);
            this.Controls.Add(this.btOpen);
            this.Controls.Add(this.btAdd);
            this.Controls.Add(this.btCompare);
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormAddScript";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Новый скрипт для отправки в GIT";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAddScript_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        /// <summary>
        /// tbScriptFilename
        /// </summary>
        public System.Windows.Forms.TextBox tbScriptFilename;
        private System.Windows.Forms.Button btOpen;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Label label7;
        /// <summary>
        /// tbGITFolder
        /// </summary>
        public System.Windows.Forms.TextBox tbGITFolder;
        private System.Windows.Forms.Button btAddAndNext;
        private System.Windows.Forms.Button btNext;
        /// <summary>
        /// isAddToGIT
        /// </summary>
        public System.Windows.Forms.CheckBox isAddToGIT;
        /// <summary>
        /// tbGITFilename
        /// </summary>
        public System.Windows.Forms.TextBox tbGITFilename;
        private System.Windows.Forms.Label label5;
        /// <summary>
        /// cbGITProject
        /// </summary>
        public System.Windows.Forms.ComboBox cbGITProject;
        /// <summary>
        /// cbTypeObject
        /// </summary>
        public System.Windows.Forms.ComboBox cbTypeObject;
        /// <summary>
        /// cbShemaObject
        /// </summary>
        public System.Windows.Forms.ComboBox cbShemaObject;
        /// <summary>
        /// cbGITNameObject
        /// </summary>
        public System.Windows.Forms.ComboBox cbGITNameObject;
        private System.Windows.Forms.Label lbGITNameObject;
        private System.Windows.Forms.Label lbShemaObject;
        private System.Windows.Forms.Label lbTypeObject;
        private System.Windows.Forms.Label lbGITProject;
        /// <summary>
        /// isAddToDEV
        /// </summary>
        public System.Windows.Forms.CheckBox isAddToDEV;
        /// <summary>
        /// tbBranch
        /// </summary>
        public System.Windows.Forms.TextBox tbBranch;
        /// <summary>
        /// cbDEVNameObject
        /// </summary>
        public System.Windows.Forms.ComboBox cbDEVNameObject;
        private System.Windows.Forms.Label lbDEVNameObject;
        private System.Windows.Forms.Label lbBranch;
        private System.Windows.Forms.Label lbDEVProject;
        /// <summary>
        /// cbDEVProject
        /// </summary>
        public System.Windows.Forms.ComboBox cbDEVProject;
        private System.Windows.Forms.Button btCompare;
        private System.Windows.Forms.CheckBox cbCheck;
    }
}