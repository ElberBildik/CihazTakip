namespace Cihaz_Takip_Uygulaması
{
    partial class KonumEkle
    {
        private System.ComponentModel.IContainer components = null;

        private System.Windows.Forms.Label lblKoordinat;

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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblKoordinat
            // 
            this.lblKoordinat.AutoSize = true;
            this.lblKoordinat.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.lblKoordinat.Location = new System.Drawing.Point(20, 20);
            this.lblKoordinat.Name = "lblKoordinat";
            this.lblKoordinat.Size = new System.Drawing.Size(173, 19);
            this.lblKoordinat.TabIndex = 0;
            this.lblKoordinat.Text = "Tıklanan Konum: X=0, Y=0";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(24, 96);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 30);
            this.button1.TabIndex = 3;
            this.button1.Text = "Cihaz Ekle";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(135, 96);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 30);
            this.button2.TabIndex = 4;
            this.button2.Text = "İptal";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // KonumEkle
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(346, 181);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lblKoordinat);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KonumEkle";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Konum Bilgisi";
            this.Load += new System.EventHandler(this.KonumEkle_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}
