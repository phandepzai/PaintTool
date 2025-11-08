namespace ImageMerger
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.Button btnMerge;
        private System.Windows.Forms.Button btnAddText; // Nút mới để thêm văn bản
        private System.Windows.Forms.ComboBox cmbResolution;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Label lblHint;
        private System.Windows.Forms.Label lblStatus;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnSelect = new System.Windows.Forms.Button();
            this.btnMerge = new System.Windows.Forms.Button();
            this.btnAddText = new System.Windows.Forms.Button();
            this.cmbResolution = new System.Windows.Forms.ComboBox();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.lblHint = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelect
            // 
            this.btnSelect.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSelect.Location = new System.Drawing.Point(233, 14);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(110, 35);
            this.btnSelect.TabIndex = 0;
            this.btnSelect.Text = "Chọn ảnh";
            this.btnSelect.UseVisualStyleBackColor = true;
            // 
            // btnMerge
            // 
            this.btnMerge.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnMerge.Location = new System.Drawing.Point(637, 14);
            this.btnMerge.Name = "btnMerge";
            this.btnMerge.Size = new System.Drawing.Size(110, 35);
            this.btnMerge.TabIndex = 1;
            this.btnMerge.Text = "Xuất ảnh";
            this.btnMerge.UseVisualStyleBackColor = true;
            // 
            // btnAddText
            // 
            this.btnAddText.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnAddText.Location = new System.Drawing.Point(43, 14);
            this.btnAddText.Name = "btnAddText";
            this.btnAddText.Size = new System.Drawing.Size(110, 35);
            this.btnAddText.TabIndex = 7;
            this.btnAddText.Text = "Thêm chữ";
            this.btnAddText.UseVisualStyleBackColor = true;
            // 
            // cmbResolution
            // 
            this.cmbResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbResolution.Font = new System.Drawing.Font("Segoe UI Semibold", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbResolution.FormattingEnabled = true;
            this.cmbResolution.Location = new System.Drawing.Point(388, 17);
            this.cmbResolution.Name = "cmbResolution";
            this.cmbResolution.Size = new System.Drawing.Size(196, 29);
            this.cmbResolution.TabIndex = 2;
            // 
            // picPreview
            // 
            this.picPreview.BackColor = System.Drawing.Color.LightGray;
            this.picPreview.Location = new System.Drawing.Point(10, 99);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(960, 540);
            this.picPreview.TabIndex = 5;
            this.picPreview.TabStop = false;
            // 
            // lblHint
            // 
            this.lblHint.AutoSize = true;
            this.lblHint.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Italic);
            this.lblHint.Location = new System.Drawing.Point(344, 54);
            this.lblHint.Name = "lblHint";
            this.lblHint.Size = new System.Drawing.Size(293, 15);
            this.lblHint.TabIndex = 4;
            this.lblHint.Text = "👉 Kéo thả ảnh trong khung bên dưới để sắp xếp tự do";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Green;
            this.lblStatus.Location = new System.Drawing.Point(13, 77);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 17);
            this.lblStatus.TabIndex = 6;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(980, 650);
            this.Controls.Add(this.btnAddText);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.picPreview);
            this.Controls.Add(this.lblHint);
            this.Controls.Add(this.cmbResolution);
            this.Controls.Add(this.btnMerge);
            this.Controls.Add(this.btnSelect);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "GHÉP ẢNH";
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}