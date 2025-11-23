// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com
namespace SQLGen.Forms
{
    partial class FormAskVersion
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnRefresh = new System.Windows.Forms.Button();
            this.cbList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnMerge = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dgList = new System.Windows.Forms.DataGridView();
            this.btnAllLog = new System.Windows.Forms.Button();
            this.project = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.branch = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.listNoMergedVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mergeStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.logMerge = new System.Windows.Forms.DataGridViewButtonColumn();
            this.projectStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.logFile = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mergeOk = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgList)).BeginInit();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnAllLog);
            this.panel2.Controls.Add(this.btnRefresh);
            this.panel2.Controls.Add(this.cbList);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.btnCancel);
            this.panel2.Controls.Add(this.btnMerge);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 566);
            this.panel2.Margin = new System.Windows.Forms.Padding(4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1504, 150);
            this.panel2.TabIndex = 7;
            // 
            // btnRefresh
            // 
            this.btnRefresh.Location = new System.Drawing.Point(656, 28);
            this.btnRefresh.Margin = new System.Windows.Forms.Padding(4);
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(189, 28);
            this.btnRefresh.TabIndex = 7;
            this.btnRefresh.Text = "Обновить";
            this.btnRefresh.UseVisualStyleBackColor = true;
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // cbList
            // 
            this.cbList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbList.FormattingEnabled = true;
            this.cbList.Location = new System.Drawing.Point(124, 32);
            this.cbList.Margin = new System.Windows.Forms.Padding(4);
            this.cbList.Name = "cbList";
            this.cbList.Size = new System.Drawing.Size(508, 24);
            this.cbList.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 32);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(57, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "Версия:";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(656, 83);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(189, 28);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Закрыть";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnMerge
            // 
            this.btnMerge.Location = new System.Drawing.Point(124, 83);
            this.btnMerge.Margin = new System.Windows.Forms.Padding(4);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(189, 28);
            this.btnMerge.TabIndex = 3;
            this.btnMerge.Text = "Влить";
            this.btnMerge.UseVisualStyleBackColor = true;
            this.btnMerge.Click += new System.EventHandler(this.btnMerge_Click);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Controls.Add(this.dgList);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Margin = new System.Windows.Forms.Padding(4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1504, 566);
            this.panel3.TabIndex = 8;
            // 
            // dgList
            // 
            this.dgList.AllowUserToAddRows = false;
            this.dgList.AllowUserToDeleteRows = false;
            this.dgList.AllowUserToResizeRows = false;
            this.dgList.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.project,
            this.branch,
            this.listNoMergedVersion,
            this.mergeStatus,
            this.logMerge,
            this.projectStatus,
            this.logFile,
            this.mergeOk});
            this.dgList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgList.Location = new System.Drawing.Point(0, 0);
            this.dgList.Margin = new System.Windows.Forms.Padding(4);
            this.dgList.MultiSelect = false;
            this.dgList.Name = "dgList";
            this.dgList.ReadOnly = true;
            this.dgList.RowHeadersVisible = false;
            this.dgList.RowHeadersWidth = 51;
            this.dgList.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.White;
            this.dgList.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgList.Size = new System.Drawing.Size(1500, 562);
            this.dgList.TabIndex = 1;
            this.dgList.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgList_CellContentClick);
            this.dgList.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgList_CellFormatting);
            // 
            // btnAllLog
            // 
            this.btnAllLog.Location = new System.Drawing.Point(443, 83);
            this.btnAllLog.Margin = new System.Windows.Forms.Padding(4);
            this.btnAllLog.Name = "btnAllLog";
            this.btnAllLog.Size = new System.Drawing.Size(189, 28);
            this.btnAllLog.TabIndex = 8;
            this.btnAllLog.Text = "Общий лог";
            this.btnAllLog.UseVisualStyleBackColor = true;
            this.btnAllLog.Click += new System.EventHandler(this.btnAllLog_Click);
            // 
            // project
            // 
            this.project.DataPropertyName = "project";
            this.project.HeaderText = "Проект";
            this.project.MinimumWidth = 6;
            this.project.Name = "project";
            this.project.ReadOnly = true;
            this.project.Width = 180;
            // 
            // branch
            // 
            this.branch.DataPropertyName = "branch";
            this.branch.HeaderText = "Текущая ветка";
            this.branch.MinimumWidth = 6;
            this.branch.Name = "branch";
            this.branch.ReadOnly = true;
            this.branch.Width = 150;
            // 
            // listNoMergedVersion
            // 
            this.listNoMergedVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.listNoMergedVersion.DataPropertyName = "listNoMergedVersion";
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.listNoMergedVersion.DefaultCellStyle = dataGridViewCellStyle1;
            this.listNoMergedVersion.HeaderText = "Список версий НЕ влитых в master";
            this.listNoMergedVersion.MinimumWidth = 6;
            this.listNoMergedVersion.Name = "listNoMergedVersion";
            this.listNoMergedVersion.ReadOnly = true;
            this.listNoMergedVersion.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // mergeStatus
            // 
            this.mergeStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.mergeStatus.DataPropertyName = "mergeStatus";
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.mergeStatus.DefaultCellStyle = dataGridViewCellStyle2;
            this.mergeStatus.HeaderText = "Статус merge";
            this.mergeStatus.MinimumWidth = 6;
            this.mergeStatus.Name = "mergeStatus";
            this.mergeStatus.ReadOnly = true;
            // 
            // logMerge
            // 
            this.logMerge.HeaderText = "Лог-файл";
            this.logMerge.MinimumWidth = 6;
            this.logMerge.Name = "logMerge";
            this.logMerge.ReadOnly = true;
            this.logMerge.Text = "Лог-файл";
            this.logMerge.UseColumnTextForButtonValue = true;
            // 
            // projectStatus
            // 
            this.projectStatus.DataPropertyName = "projectStatus";
            this.projectStatus.HeaderText = "projectStatus";
            this.projectStatus.MinimumWidth = 6;
            this.projectStatus.Name = "projectStatus";
            this.projectStatus.ReadOnly = true;
            this.projectStatus.Visible = false;
            this.projectStatus.Width = 200;
            // 
            // logFile
            // 
            this.logFile.DataPropertyName = "logFile";
            this.logFile.HeaderText = "logFile";
            this.logFile.MinimumWidth = 6;
            this.logFile.Name = "logFile";
            this.logFile.ReadOnly = true;
            this.logFile.Visible = false;
            this.logFile.Width = 125;
            // 
            // mergeOk
            // 
            this.mergeOk.DataPropertyName = "mergeOk";
            this.mergeOk.HeaderText = "mergeOk";
            this.mergeOk.MinimumWidth = 6;
            this.mergeOk.Name = "mergeOk";
            this.mergeOk.ReadOnly = true;
            this.mergeOk.Visible = false;
            this.mergeOk.Width = 125;
            // 
            // FormAskVersion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1504, 716);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "FormAskVersion";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Выбери версию, вливаемую в master";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormAskVersion_FormClosed);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgList)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnMerge;
        /// <summary>
        /// список версий
        /// </summary>
        public System.Windows.Forms.ComboBox cbList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel3;
        /// <summary>
        /// Список проектов
        /// </summary>
        public System.Windows.Forms.DataGridView dgList;
        private System.Windows.Forms.Button btnRefresh;
        private System.Windows.Forms.Button btnAllLog;
        private System.Windows.Forms.DataGridViewTextBoxColumn project;
        private System.Windows.Forms.DataGridViewTextBoxColumn branch;
        private System.Windows.Forms.DataGridViewTextBoxColumn listNoMergedVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn mergeStatus;
        private System.Windows.Forms.DataGridViewButtonColumn logMerge;
        private System.Windows.Forms.DataGridViewTextBoxColumn projectStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn logFile;
        private System.Windows.Forms.DataGridViewTextBoxColumn mergeOk;
    }
}