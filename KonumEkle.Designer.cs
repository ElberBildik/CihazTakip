namespace Cihaz_Takip_Uygulaması
{
    partial class KonumEkle
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblKoordinat;
        private System.Windows.Forms.Button btnCihazEkle;
        private System.Windows.Forms.Button btnIptal;
        private System.Windows.Forms.Panel pnlHeader;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.lblKoordinat = new System.Windows.Forms.Label();
            this.btnCihazEkle = new System.Windows.Forms.Button();
            this.btnIptal = new System.Windows.Forms.Button();
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.pnlHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblKoordinat
            // 
            this.lblKoordinat.AutoSize = true;
            this.lblKoordinat.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblKoordinat.ForeColor = System.Drawing.Color.White;
            this.lblKoordinat.Location = new System.Drawing.Point(15, 15);
            this.lblKoordinat.Name = "lblKoordinat";
            this.lblKoordinat.Size = new System.Drawing.Size(211, 21);
            this.lblKoordinat.TabIndex = 0;
            this.lblKoordinat.Text = "Tıklanan Konum: X=0, Y=0";
            // 
            // btnCihazEkle
            // 
            this.btnCihazEkle.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.btnCihazEkle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCihazEkle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnCihazEkle.ForeColor = System.Drawing.Color.White;
            this.btnCihazEkle.Location = new System.Drawing.Point(37, 117);
            this.btnCihazEkle.Name = "btnCihazEkle";
            this.btnCihazEkle.Size = new System.Drawing.Size(120, 40);
            this.btnCihazEkle.TabIndex = 1;
            this.btnCihazEkle.Text = "📍 Cihaz Ekle";
            this.btnCihazEkle.UseVisualStyleBackColor = false;
            this.btnCihazEkle.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnIptal
            // 
            this.btnIptal.BackColor = System.Drawing.Color.IndianRed;
            this.btnIptal.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnIptal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnIptal.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.btnIptal.ForeColor = System.Drawing.Color.White;
            this.btnIptal.Location = new System.Drawing.Point(176, 117);
            this.btnIptal.Name = "btnIptal";
            this.btnIptal.Size = new System.Drawing.Size(90, 40);
            this.btnIptal.TabIndex = 2;
            this.btnIptal.Text = "❌ İptal";
            this.btnIptal.UseVisualStyleBackColor = false;
            this.btnIptal.Click += new System.EventHandler(this.button2_Click);
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.Teal;
            this.pnlHeader.Controls.Add(this.lblKoordinat);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(361, 73);
            this.pnlHeader.TabIndex = 3;
            // 
            // KonumEkle
            // 
            this.AcceptButton = this.btnCihazEkle;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.CancelButton = this.btnIptal;
            this.ClientSize = new System.Drawing.Size(361, 179);
            this.Controls.Add(this.btnIptal);
            this.Controls.Add(this.btnCihazEkle);
            this.Controls.Add(this.pnlHeader);
            this.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KonumEkle";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "📌 Konum Bilgisi";
            this.Load += new System.EventHandler(this.KonumEkle_Load);
            this.pnlHeader.ResumeLayout(false);
            this.pnlHeader.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
    }
}
