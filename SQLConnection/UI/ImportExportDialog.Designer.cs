namespace SQLConnection.UI
{
 partial class ImportExportDialog
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
 this.btnImport = new System.Windows.Forms.Button();
 this.btnExport = new System.Windows.Forms.Button();
 this.btnCancel = new System.Windows.Forms.Button();
 this.progressBar = new System.Windows.Forms.ProgressBar();
 this.lblStatus = new System.Windows.Forms.Label();
 this.SuspendLayout();
 // 
 // btnImport
 // 
 this.btnImport.Location = new System.Drawing.Point(24,22);
 this.btnImport.Name = "btnImport";
 this.btnImport.Size = new System.Drawing.Size(112,34);
 this.btnImport.TabIndex =0;
 this.btnImport.Text = "Import";
 this.btnImport.UseVisualStyleBackColor = true;
 this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
 // 
 // btnExport
 // 
 this.btnExport.Location = new System.Drawing.Point(152,22);
 this.btnExport.Name = "btnExport";
 this.btnExport.Size = new System.Drawing.Size(112,34);
 this.btnExport.TabIndex =1;
 this.btnExport.Text = "Export";
 this.btnExport.UseVisualStyleBackColor = true;
 this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
 // 
 // btnCancel
 // 
 this.btnCancel.Location = new System.Drawing.Point(280,22);
 this.btnCancel.Name = "btnCancel";
 this.btnCancel.Size = new System.Drawing.Size(112,34);
 this.btnCancel.TabIndex =2;
 this.btnCancel.Text = "Cancel";
 this.btnCancel.UseVisualStyleBackColor = true;
 this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
 // 
 // progressBar
 // 
 this.progressBar.Location = new System.Drawing.Point(24,80);
 this.progressBar.Name = "progressBar";
 this.progressBar.Size = new System.Drawing.Size(368,23);
 this.progressBar.TabIndex =3;
 // 
 // lblStatus
 // 
 this.lblStatus.AutoSize = true;
 this.lblStatus.Location = new System.Drawing.Point(24,120);
 this.lblStatus.Name = "lblStatus";
 this.lblStatus.Size = new System.Drawing.Size(0,25);
 this.lblStatus.TabIndex =4;
 // 
 // ImportExportDialog
 // 
 this.AutoScaleDimensions = new System.Drawing.SizeF(10F,25F);
 this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
 this.ClientSize = new System.Drawing.Size(414,166);
 this.Controls.Add(this.lblStatus);
 this.Controls.Add(this.progressBar);
 this.Controls.Add(this.btnCancel);
 this.Controls.Add(this.btnExport);
 this.Controls.Add(this.btnImport);
 this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
 this.MaximizeBox = false;
 this.MinimizeBox = false;
 this.Name = "ImportExportDialog";
 this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
 this.Text = "Import / Export";
 this.ResumeLayout(false);
 this.PerformLayout();
 }

 #endregion

 private System.Windows.Forms.Button btnImport;
 private System.Windows.Forms.Button btnExport;
 private System.Windows.Forms.Button btnCancel;
 private System.Windows.Forms.ProgressBar progressBar;
 private System.Windows.Forms.Label lblStatus;
 }
}
