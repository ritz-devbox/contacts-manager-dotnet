namespace SQLConnection
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            label2 = new Label();
            tb_name = new TextBox();
            tb_mobile = new TextBox();
            label3 = new Label();
            tb_email = new TextBox();
            label4 = new Label();
            btn_save = new Button();
            btn_update = new Button();
            btn_delete = new Button();
            btnImportExport = new Button();
            Contactsdatagridview = new DataGridView();
            labelStatus = new Label();
            ((System.ComponentModel.ISupportInitialize)Contactsdatagridview).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(357, 52);
            label1.Name = "label1";
            label1.Size = new Size(139, 25);
            label1.TabIndex = 2;
            label1.Text = "SQL Connection";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label2.Location = new Point(40, 112);
            label2.Name = "label2";
            label2.Size = new Size(62, 25);
            label2.TabIndex = 3;
            label2.Text = "Name";
            // 
            // tb_name
            // 
            tb_name.Location = new Point(40, 149);
            tb_name.Name = "tb_name";
            tb_name.Size = new Size(286, 31);
            tb_name.TabIndex = 4;
            // 
            // tb_mobile
            // 
            tb_mobile.Location = new Point(40, 331);
            tb_mobile.Name = "tb_mobile";
            tb_mobile.Size = new Size(286, 31);
            tb_mobile.TabIndex = 6;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label3.Location = new Point(40, 294);
            label3.Name = "label3";
            label3.Size = new Size(71, 25);
            label3.TabIndex = 5;
            label3.Text = "Mobile";
            // 
            // tb_email
            // 
            tb_email.Location = new Point(40, 246);
            tb_email.Name = "tb_email";
            tb_email.Size = new Size(286, 31);
            tb_email.TabIndex = 8;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            label4.Location = new Point(40, 209);
            label4.Name = "label4";
            label4.Size = new Size(58, 25);
            label4.TabIndex = 7;
            label4.Text = "Email";
            // 
            // btn_save
            // 
            btn_save.Location = new Point(40, 392);
            btn_save.Name = "btn_save";
            btn_save.Size = new Size(112, 34);
            btn_save.TabIndex = 9;
            btn_save.Text = "Save";
            btn_save.UseVisualStyleBackColor = true;
            btn_save.Click += btn_save_Click;
            // 
            // btn_update
            // 
            btn_update.Location = new Point(198, 392);
            btn_update.Name = "btn_update";
            btn_update.Size = new Size(112, 34);
            btn_update.TabIndex = 10;
            btn_update.Text = "Update";
            btn_update.UseVisualStyleBackColor = true;
            btn_update.Click += btn_update_Click;
            // 
            // btn_delete
            // 
            btn_delete.Location = new Point(120, 454);
            btn_delete.Name = "btn_delete";
            btn_delete.Size = new Size(112, 34);
            btn_delete.TabIndex = 11;
            btn_delete.Text = "Delete";
            btn_delete.UseVisualStyleBackColor = true;
            btn_delete.Click += btn_delete_Click;
            // 
            // btnImportExport
            // 
            btnImportExport.Location = new Point(24, 452);
            btnImportExport.Name = "btnImportExport";
            btnImportExport.Size = new Size(150, 34);
            btnImportExport.TabIndex = 14;
            btnImportExport.Text = "Import / Export";
            btnImportExport.UseVisualStyleBackColor = true;
            btnImportExport.Click += new EventHandler(this.btnImportExport_Click);
            // 
            // Contactsdatagridview
            // 
            Contactsdatagridview.BackgroundColor = SystemColors.Control;
            Contactsdatagridview.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Contactsdatagridview.Location = new Point(392, 112);
            Contactsdatagridview.Name = "Contactsdatagridview";
            Contactsdatagridview.RowHeadersWidth = 62;
            Contactsdatagridview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Contactsdatagridview.Size = new Size(505, 358);
            Contactsdatagridview.TabIndex = 12;
            Contactsdatagridview.CellClick += new DataGridViewCellEventHandler(Contactsdatagridview_CellClick);
            // 
            // labelStatus
            // 
            labelStatus.AutoSize = true;
            labelStatus.Location = new Point(40, 510);
            labelStatus.Name = "labelStatus";
            labelStatus.Size = new Size(0, 25);
            labelStatus.TabIndex = 13;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(959, 629);
            Controls.Add(labelStatus);
            Controls.Add(btnImportExport);
            Controls.Add(Contactsdatagridview);
            Controls.Add(btn_delete);
            Controls.Add(btn_update);
            Controls.Add(btn_save);
            Controls.Add(tb_email);
            Controls.Add(label4);
            Controls.Add(tb_mobile);
            Controls.Add(label3);
            Controls.Add(tb_name);
            Controls.Add(label2);
            Controls.Add(label1);
            Text = "Database Connection";
            ((System.ComponentModel.ISupportInitialize)Contactsdatagridview).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Label label1;
        private Label label2;
        private TextBox tb_name;
        private TextBox tb_mobile;
        private Label label3;
        private TextBox tb_email;
        private Label label4;
        private Button btn_save;
        private Button btn_update;
        private Button btn_delete;
        private Button btnImportExport;
        private DataGridView Contactsdatagridview;
        private Label labelStatus;
    }
}
