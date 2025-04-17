
namespace Cihaz_Takip_Uygulaması
{
    partial class CihazEklemeEkrani
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
            this.CmbKod = new System.Windows.Forms.ComboBox();
            this.cihazGrupBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.kodlariGetir = new Cihaz_Takip_Uygulaması.KodlariGetir();
            this.EnerjiPanoSigortaNoTxtBox = new System.Windows.Forms.TextBox();
            this.EnerjiPanoNoTxtBox = new System.Windows.Forms.TextBox();
            this.MarkaTxtBox = new System.Windows.Forms.TextBox();
            this.SwitchPortNoTxtBox = new System.Windows.Forms.TextBox();
            this.SwitchRecNoTxtBox = new System.Windows.Forms.TextBox();
            this.IPTxtBox = new System.Windows.Forms.TextBox();
            this.aciklamaTxtBox = new System.Windows.Forms.TextBox();
            this.CihazEkleBtn = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cihazGrupTableAdapter = new Cihaz_Takip_Uygulaması.KodlariGetirTableAdapters.CihazGrupTableAdapter();
            this.YTxtBox = new System.Windows.Forms.TextBox();
            this.XTxtBox = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.cihazGrupBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.kodlariGetir)).BeginInit();
            this.SuspendLayout();
            // 
            // CmbKod
            // 
            this.CmbKod.DataBindings.Add(new System.Windows.Forms.Binding("SelectedValue", this.cihazGrupBindingSource, "Kod", true));
            this.CmbKod.DataSource = this.cihazGrupBindingSource;
            this.CmbKod.DisplayMember = "Kod";
            this.CmbKod.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.CmbKod.FormattingEnabled = true;
            this.CmbKod.Location = new System.Drawing.Point(329, 23);
            this.CmbKod.Name = "CmbKod";
            this.CmbKod.Size = new System.Drawing.Size(184, 28);
            this.CmbKod.TabIndex = 34;
            this.CmbKod.ValueMember = "Kod";
            // 
            // cihazGrupBindingSource
            // 
            this.cihazGrupBindingSource.DataMember = "CihazGrup";
            this.cihazGrupBindingSource.DataSource = this.kodlariGetir;
            // 
            // kodlariGetir
            // 
            this.kodlariGetir.DataSetName = "KodlariGetir";
            this.kodlariGetir.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // EnerjiPanoSigortaNoTxtBox
            // 
            this.EnerjiPanoSigortaNoTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EnerjiPanoSigortaNoTxtBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.EnerjiPanoSigortaNoTxtBox.Location = new System.Drawing.Point(330, 317);
            this.EnerjiPanoSigortaNoTxtBox.Name = "EnerjiPanoSigortaNoTxtBox";
            this.EnerjiPanoSigortaNoTxtBox.Size = new System.Drawing.Size(184, 29);
            this.EnerjiPanoSigortaNoTxtBox.TabIndex = 33;
            // 
            // EnerjiPanoNoTxtBox
            // 
            this.EnerjiPanoNoTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.EnerjiPanoNoTxtBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.EnerjiPanoNoTxtBox.Location = new System.Drawing.Point(329, 275);
            this.EnerjiPanoNoTxtBox.Name = "EnerjiPanoNoTxtBox";
            this.EnerjiPanoNoTxtBox.Size = new System.Drawing.Size(184, 29);
            this.EnerjiPanoNoTxtBox.TabIndex = 32;
            // 
            // MarkaTxtBox
            // 
            this.MarkaTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MarkaTxtBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.MarkaTxtBox.Location = new System.Drawing.Point(330, 233);
            this.MarkaTxtBox.Name = "MarkaTxtBox";
            this.MarkaTxtBox.Size = new System.Drawing.Size(184, 29);
            this.MarkaTxtBox.TabIndex = 31;
            // 
            // SwitchPortNoTxtBox
            // 
            this.SwitchPortNoTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SwitchPortNoTxtBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.SwitchPortNoTxtBox.Location = new System.Drawing.Point(329, 196);
            this.SwitchPortNoTxtBox.Name = "SwitchPortNoTxtBox";
            this.SwitchPortNoTxtBox.Size = new System.Drawing.Size(184, 29);
            this.SwitchPortNoTxtBox.TabIndex = 30;
            // 
            // SwitchRecNoTxtBox
            // 
            this.SwitchRecNoTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SwitchRecNoTxtBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.SwitchRecNoTxtBox.Location = new System.Drawing.Point(329, 159);
            this.SwitchRecNoTxtBox.Name = "SwitchRecNoTxtBox";
            this.SwitchRecNoTxtBox.Size = new System.Drawing.Size(184, 29);
            this.SwitchRecNoTxtBox.TabIndex = 29;
            // 
            // IPTxtBox
            // 
            this.IPTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.IPTxtBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.IPTxtBox.Location = new System.Drawing.Point(330, 122);
            this.IPTxtBox.Name = "IPTxtBox";
            this.IPTxtBox.Size = new System.Drawing.Size(184, 29);
            this.IPTxtBox.TabIndex = 28;
            // 
            // aciklamaTxtBox
            // 
            this.aciklamaTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.aciklamaTxtBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.aciklamaTxtBox.Location = new System.Drawing.Point(330, 58);
            this.aciklamaTxtBox.Multiline = true;
            this.aciklamaTxtBox.Name = "aciklamaTxtBox";
            this.aciklamaTxtBox.Size = new System.Drawing.Size(184, 62);
            this.aciklamaTxtBox.TabIndex = 27;
            // 
            // CihazEkleBtn
            // 
            this.CihazEkleBtn.BackColor = System.Drawing.Color.DarkSlateGray;
            this.CihazEkleBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CihazEkleBtn.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.CihazEkleBtn.ForeColor = System.Drawing.Color.White;
            this.CihazEkleBtn.Location = new System.Drawing.Point(330, 445);
            this.CihazEkleBtn.Name = "CihazEkleBtn";
            this.CihazEkleBtn.Size = new System.Drawing.Size(165, 37);
            this.CihazEkleBtn.TabIndex = 26;
            this.CihazEkleBtn.Text = "Cihaz Ekle";
            this.CihazEkleBtn.UseVisualStyleBackColor = false;
            this.CihazEkleBtn.Click += new System.EventHandler(this.CihazEkleBtn_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label8.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label8.Location = new System.Drawing.Point(106, 317);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(221, 25);
            this.label8.TabIndex = 25;
            this.label8.Text = "Enerji Pano Sigorta No:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label7.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label7.Location = new System.Drawing.Point(177, 275);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(150, 25);
            this.label7.TabIndex = 24;
            this.label7.Text = "Enerji Pano No:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label6.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label6.Location = new System.Drawing.Point(188, 233);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(139, 25);
            this.label6.TabIndex = 23;
            this.label6.Text = "Marka/Model:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label5.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label5.Location = new System.Drawing.Point(177, 196);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(151, 25);
            this.label5.TabIndex = 22;
            this.label5.Text = "Switch Port No:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label4.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label4.Location = new System.Drawing.Point(184, 158);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(143, 25);
            this.label4.TabIndex = 21;
            this.label4.Text = "Switch Rec No:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label3.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label3.Location = new System.Drawing.Point(230, 121);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 25);
            this.label3.TabIndex = 20;
            this.label3.Text = "IP Giriniz:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label2.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label2.Location = new System.Drawing.Point(227, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(97, 25);
            this.label2.TabIndex = 19;
            this.label2.Text = "Açıklama:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label1.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label1.Location = new System.Drawing.Point(198, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 25);
            this.label1.TabIndex = 18;
            this.label1.Text = "Grup Seçiniz:";
            // 
            // cihazGrupTableAdapter
            // 
            this.cihazGrupTableAdapter.ClearBeforeFill = true;
            // 
            // YTxtBox
            // 
            this.YTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.YTxtBox.Enabled = false;
            this.YTxtBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.YTxtBox.Location = new System.Drawing.Point(329, 389);
            this.YTxtBox.Name = "YTxtBox";
            this.YTxtBox.Size = new System.Drawing.Size(81, 29);
            this.YTxtBox.TabIndex = 38;
            // 
            // XTxtBox
            // 
            this.XTxtBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.XTxtBox.Enabled = false;
            this.XTxtBox.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.XTxtBox.Location = new System.Drawing.Point(330, 352);
            this.XTxtBox.Name = "XTxtBox";
            this.XTxtBox.Size = new System.Drawing.Size(80, 29);
            this.XTxtBox.TabIndex = 37;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label9.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label9.Location = new System.Drawing.Point(206, 389);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(117, 25);
            this.label9.TabIndex = 36;
            this.label9.Text = "Y Kordinatı:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.label10.ForeColor = System.Drawing.Color.DarkSlateGray;
            this.label10.Location = new System.Drawing.Point(207, 356);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(117, 25);
            this.label10.TabIndex = 35;
            this.label10.Text = "X Kordinatı:";
            // 
            // CihazEklemeEkrani
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 500);
            this.Controls.Add(this.YTxtBox);
            this.Controls.Add(this.XTxtBox);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.CmbKod);
            this.Controls.Add(this.EnerjiPanoSigortaNoTxtBox);
            this.Controls.Add(this.EnerjiPanoNoTxtBox);
            this.Controls.Add(this.MarkaTxtBox);
            this.Controls.Add(this.SwitchPortNoTxtBox);
            this.Controls.Add(this.SwitchRecNoTxtBox);
            this.Controls.Add(this.IPTxtBox);
            this.Controls.Add(this.aciklamaTxtBox);
            this.Controls.Add(this.CihazEkleBtn);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "CihazEklemeEkrani";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cihaz Eklemek için Özelleikleri Girin";
            this.Load += new System.EventHandler(this.CihazEklemeEkrani_Load);
            ((System.ComponentModel.ISupportInitialize)(this.cihazGrupBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.kodlariGetir)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox CmbKod;
        private System.Windows.Forms.TextBox EnerjiPanoSigortaNoTxtBox;
        private System.Windows.Forms.TextBox EnerjiPanoNoTxtBox;
        private System.Windows.Forms.TextBox MarkaTxtBox;
        private System.Windows.Forms.TextBox SwitchPortNoTxtBox;
        private System.Windows.Forms.TextBox SwitchRecNoTxtBox;
        private System.Windows.Forms.TextBox IPTxtBox;
        private System.Windows.Forms.TextBox aciklamaTxtBox;
        private System.Windows.Forms.Button CihazEkleBtn;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private KodlariGetir kodlariGetir;
        private System.Windows.Forms.BindingSource cihazGrupBindingSource;
        private KodlariGetirTableAdapters.CihazGrupTableAdapter cihazGrupTableAdapter;
        private System.Windows.Forms.TextBox YTxtBox;
        private System.Windows.Forms.TextBox XTxtBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
    }
}