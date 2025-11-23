// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: https://pvs-studio.com

namespace SQLGen
{
    partial class FormEditDB
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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btChangeServerInAllConnects = new System.Windows.Forms.Button();
            this.btMoveDBtoOtherServer = new System.Windows.Forms.Button();
            this.btEdit = new System.Windows.Forms.Button();
            this.btDel = new System.Windows.Forms.Button();
            this.btAdd = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btCancel = new System.Windows.Forms.Button();
            this.btOk = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.dgListDB = new System.Windows.Forms.DataGridView();
            this.GITProjectsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.AppInfoBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.ListDatabasesBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.serverNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dBNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gITProjectDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dBRoleDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isMainTestDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.DBType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgListDB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.GITProjectsBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.AppInfoBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ListDatabasesBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btChangeServerInAllConnects);
            this.panel1.Controls.Add(this.btMoveDBtoOtherServer);
            this.panel1.Controls.Add(this.btEdit);
            this.panel1.Controls.Add(this.btDel);
            this.panel1.Controls.Add(this.btAdd);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1352, 84);
            this.panel1.TabIndex = 5;
            // 
            // btChangeServerInAllConnects
            // 
            this.btChangeServerInAllConnects.Location = new System.Drawing.Point(903, 15);
            this.btChangeServerInAllConnects.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btChangeServerInAllConnects.Name = "btChangeServerInAllConnects";
            this.btChangeServerInAllConnects.Size = new System.Drawing.Size(372, 49);
            this.btChangeServerInAllConnects.TabIndex = 4;
            this.btChangeServerInAllConnects.Text = "Сменился адрес у сервера для всех БД";
            this.btChangeServerInAllConnects.UseVisualStyleBackColor = true;
            this.btChangeServerInAllConnects.Click += new System.EventHandler(this.btChangeServerInAllConnects_Click);
            // 
            // btMoveDBtoOtherServer
            // 
            this.btMoveDBtoOtherServer.Location = new System.Drawing.Point(560, 15);
            this.btMoveDBtoOtherServer.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btMoveDBtoOtherServer.Name = "btMoveDBtoOtherServer";
            this.btMoveDBtoOtherServer.Size = new System.Drawing.Size(308, 49);
            this.btMoveDBtoOtherServer.TabIndex = 3;
            this.btMoveDBtoOtherServer.Text = "Переезд БД на другой сервер";
            this.btMoveDBtoOtherServer.UseVisualStyleBackColor = true;
            this.btMoveDBtoOtherServer.Click += new System.EventHandler(this.btMoveDBtoOtherServer_Click);
            // 
            // btEdit
            // 
            this.btEdit.Location = new System.Drawing.Point(187, 15);
            this.btEdit.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btEdit.Name = "btEdit";
            this.btEdit.Size = new System.Drawing.Size(167, 49);
            this.btEdit.TabIndex = 2;
            this.btEdit.Text = "Редактировать";
            this.btEdit.UseVisualStyleBackColor = true;
            this.btEdit.Click += new System.EventHandler(this.btEdit_Click);
            // 
            // btDel
            // 
            this.btDel.Location = new System.Drawing.Point(381, 15);
            this.btDel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btDel.Name = "btDel";
            this.btDel.Size = new System.Drawing.Size(148, 49);
            this.btDel.TabIndex = 1;
            this.btDel.Text = "Удалить";
            this.btDel.UseVisualStyleBackColor = true;
            this.btDel.Click += new System.EventHandler(this.btDel_Click);
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(16, 15);
            this.btAdd.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(140, 49);
            this.btAdd.TabIndex = 0;
            this.btAdd.Text = "Добавить";
            this.btAdd.UseVisualStyleBackColor = true;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btCancel);
            this.panel2.Controls.Add(this.btOk);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 478);
            this.panel2.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1352, 89);
            this.panel2.TabIndex = 6;
            // 
            // btCancel
            // 
            this.btCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btCancel.Location = new System.Drawing.Point(823, 23);
            this.btCancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(191, 44);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "Сохранить";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // btOk
            // 
            this.btOk.Location = new System.Drawing.Point(277, 23);
            this.btOk.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.btOk.Name = "btOk";
            this.btOk.Size = new System.Drawing.Size(184, 44);
            this.btOk.TabIndex = 4;
            this.btOk.Text = "Выбрать";
            this.btOk.UseVisualStyleBackColor = true;
            this.btOk.Click += new System.EventHandler(this.btOk_Click);
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.panel3.Controls.Add(this.dgListDB);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 84);
            this.panel3.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1352, 394);
            this.panel3.TabIndex = 7;
            // 
            // dgListDB
            // 
            this.dgListDB.AllowUserToAddRows = false;
            this.dgListDB.AllowUserToDeleteRows = false;
            this.dgListDB.AllowUserToResizeRows = false;
            this.dgListDB.AutoGenerateColumns = false;
            this.dgListDB.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgListDB.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.serverNameDataGridViewTextBoxColumn,
            this.dBNameDataGridViewTextBoxColumn,
            this.gITProjectDataGridViewTextBoxColumn,
            this.dBRoleDataGridViewTextBoxColumn,
            this.isMainTestDataGridViewCheckBoxColumn,
            this.DBType});
            this.dgListDB.DataSource = this.ListDatabasesBindingSource;
            this.dgListDB.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgListDB.Location = new System.Drawing.Point(0, 0);
            this.dgListDB.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.dgListDB.MultiSelect = false;
            this.dgListDB.Name = "dgListDB";
            this.dgListDB.ReadOnly = true;
            this.dgListDB.RowHeadersWidth = 51;
            this.dgListDB.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgListDB.Size = new System.Drawing.Size(1348, 390);
            this.dgListDB.TabIndex = 0;
            this.dgListDB.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgListDB_CellDoubleClick);
            // 
            // GITProjectsBindingSource
            // 
            this.GITProjectsBindingSource.DataMember = "GITProjects";
            this.GITProjectsBindingSource.DataSource = this.AppInfoBindingSource;
            // 
            // AppInfoBindingSource
            // 
            this.AppInfoBindingSource.DataSource = typeof(SQLGen.APPinfo);
            // 
            // ListDatabasesBindingSource
            // 
            this.ListDatabasesBindingSource.DataMember = "ListDatabases";
            this.ListDatabasesBindingSource.DataSource = this.AppInfoBindingSource;
            // 
            // serverNameDataGridViewTextBoxColumn
            // 
            this.serverNameDataGridViewTextBoxColumn.DataPropertyName = "ServerName";
            this.serverNameDataGridViewTextBoxColumn.HeaderText = "Имя сервера или IP-адрес";
            this.serverNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.serverNameDataGridViewTextBoxColumn.Name = "serverNameDataGridViewTextBoxColumn";
            this.serverNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.serverNameDataGridViewTextBoxColumn.Width = 150;
            // 
            // dBNameDataGridViewTextBoxColumn
            // 
            this.dBNameDataGridViewTextBoxColumn.DataPropertyName = "DBName";
            this.dBNameDataGridViewTextBoxColumn.HeaderText = "Имя базы данных";
            this.dBNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.dBNameDataGridViewTextBoxColumn.Name = "dBNameDataGridViewTextBoxColumn";
            this.dBNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.dBNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // gITProjectDataGridViewTextBoxColumn
            // 
            this.gITProjectDataGridViewTextBoxColumn.DataPropertyName = "GITProject";
            this.gITProjectDataGridViewTextBoxColumn.HeaderText = "Проект GIT";
            this.gITProjectDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.gITProjectDataGridViewTextBoxColumn.Name = "gITProjectDataGridViewTextBoxColumn";
            this.gITProjectDataGridViewTextBoxColumn.ReadOnly = true;
            this.gITProjectDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.gITProjectDataGridViewTextBoxColumn.Width = 200;
            // 
            // dBRoleDataGridViewTextBoxColumn
            // 
            this.dBRoleDataGridViewTextBoxColumn.DataPropertyName = "DBRole";
            this.dBRoleDataGridViewTextBoxColumn.HeaderText = "Роль БД";
            this.dBRoleDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.dBRoleDataGridViewTextBoxColumn.Name = "dBRoleDataGridViewTextBoxColumn";
            this.dBRoleDataGridViewTextBoxColumn.ReadOnly = true;
            this.dBRoleDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dBRoleDataGridViewTextBoxColumn.Width = 125;
            // 
            // isMainTestDataGridViewCheckBoxColumn
            // 
            this.isMainTestDataGridViewCheckBoxColumn.DataPropertyName = "isMainTest";
            this.isMainTestDataGridViewCheckBoxColumn.HeaderText = "Основная тестовая БД для проекта GIT";
            this.isMainTestDataGridViewCheckBoxColumn.MinimumWidth = 6;
            this.isMainTestDataGridViewCheckBoxColumn.Name = "isMainTestDataGridViewCheckBoxColumn";
            this.isMainTestDataGridViewCheckBoxColumn.ReadOnly = true;
            this.isMainTestDataGridViewCheckBoxColumn.Width = 125;
            // 
            // DBType
            // 
            this.DBType.DataPropertyName = "DBType";
            this.DBType.HeaderText = "Тип БД";
            this.DBType.MinimumWidth = 6;
            this.DBType.Name = "DBType";
            this.DBType.ReadOnly = true;
            this.DBType.Width = 125;
            // 
            // FormEditDB
            // 
            this.AcceptButton = this.btOk;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btCancel;
            this.ClientSize = new System.Drawing.Size(1352, 567);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "FormEditDB";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Выбрать базу данных";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormEditDB_FormClosed);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgListDB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.GITProjectsBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.AppInfoBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ListDatabasesBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.Button btOk;
        private System.Windows.Forms.Panel panel3;
        /// <summary>
        /// dgListDB
        /// </summary>
        public System.Windows.Forms.DataGridView dgListDB;
        private System.Windows.Forms.BindingSource AppInfoBindingSource;
        /// <summary>
        /// GITProjectsBindingSource
        /// </summary>
        public System.Windows.Forms.BindingSource GITProjectsBindingSource;
        /// <summary>
        /// ListDatabasesBindingSource
        /// </summary>
        public System.Windows.Forms.BindingSource ListDatabasesBindingSource;
        private System.Windows.Forms.Button btDel;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.Button btEdit;
        private System.Windows.Forms.Button btMoveDBtoOtherServer;
        private System.Windows.Forms.Button btChangeServerInAllConnects;
        private System.Windows.Forms.DataGridViewTextBoxColumn serverNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dBNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gITProjectDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dBRoleDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isMainTestDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn DBType;
    }
}