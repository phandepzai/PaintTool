namespace ImageMerger
{
    partial class ColorPickerForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlSV = new System.Windows.Forms.Panel();
            this.pnlHue = new System.Windows.Forms.Panel();
            this.pnlPreview = new System.Windows.Forms.Panel();
            this.lblPrev = new System.Windows.Forms.Label();
            this.txtHex = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblHex = new System.Windows.Forms.Label();
            this.pnlPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlSV
            // 
            this.pnlSV.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pnlSV.Location = new System.Drawing.Point(15, 15);
            this.pnlSV.Name = "pnlSV";
            this.pnlSV.Size = new System.Drawing.Size(220, 220);
            this.pnlSV.TabIndex = 0;
            this.pnlSV.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlSV_Paint);
            this.pnlSV.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlSV_MouseDown);
            this.pnlSV.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlSV_MouseMove);
            this.pnlSV.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlSV_MouseUp);
            // 
            // pnlHue
            // 
            this.pnlHue.Cursor = System.Windows.Forms.Cursors.Cross;
            this.pnlHue.Location = new System.Drawing.Point(250, 15);
            this.pnlHue.Name = "pnlHue";
            this.pnlHue.Size = new System.Drawing.Size(30, 220);
            this.pnlHue.TabIndex = 1;
            this.pnlHue.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlHue_Paint);
            this.pnlHue.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pnlHue_MouseDown);
            this.pnlHue.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pnlHue_MouseMove);
            this.pnlHue.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pnlHue_MouseUp);
            // 
            // pnlPreview
            // 
            this.pnlPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPreview.Controls.Add(this.lblPrev);
            this.pnlPreview.Location = new System.Drawing.Point(15, 250);
            this.pnlPreview.Name = "pnlPreview";
            this.pnlPreview.Size = new System.Drawing.Size(265, 50);
            this.pnlPreview.TabIndex = 2;
            // 
            // lblPrev
            // 
            this.lblPrev.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPrev.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblPrev.ForeColor = System.Drawing.Color.White;
            this.lblPrev.Location = new System.Drawing.Point(0, 0);
            this.lblPrev.Name = "lblPrev";
            this.lblPrev.Size = new System.Drawing.Size(263, 48);
            this.lblPrev.TabIndex = 0;
            this.lblPrev.Text = "XEM TRƯỚC";
            this.lblPrev.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // txtHex
            // 
            this.txtHex.Location = new System.Drawing.Point(97, 315);
            this.txtHex.Name = "txtHex";
            this.txtHex.Size = new System.Drawing.Size(100, 23);
            this.txtHex.TabIndex = 2;
            this.txtHex.Text = "#FFFFFF";
            this.txtHex.TextChanged += new System.EventHandler(this.txtHex_TextChanged);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(55, 353);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 30);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(140, 353);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 30);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Hủy";
            // 
            // lblHex
            // 
            this.lblHex.AutoSize = true;
            this.lblHex.Location = new System.Drawing.Point(52, 319);
            this.lblHex.Name = "lblHex";
            this.lblHex.Size = new System.Drawing.Size(32, 15);
            this.lblHex.TabIndex = 3;
            this.lblHex.Text = "HEX:";
            // 
            // ColorPickerForm
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(301, 396);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtHex);
            this.Controls.Add(this.lblHex);
            this.Controls.Add(this.pnlPreview);
            this.Controls.Add(this.pnlHue);
            this.Controls.Add(this.pnlSV);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColorPickerForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Chọn màu";
            this.pnlPreview.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Panel pnlSV;
        private System.Windows.Forms.Panel pnlHue;
        private System.Windows.Forms.Panel pnlPreview;
        private System.Windows.Forms.Label lblPrev;
        private System.Windows.Forms.Label lblHex;
        private System.Windows.Forms.TextBox txtHex;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}