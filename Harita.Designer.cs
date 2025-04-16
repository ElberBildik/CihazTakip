
namespace Cihaz_Takip_Uygulaması
{
    partial class Harita
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Harita));
            this.panel1 = new System.Windows.Forms.Panel();
            this.DownCihazlar = new System.Windows.Forms.CheckBox();
            this.CizgiKaldirChckBox = new System.Windows.Forms.CheckBox();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.DownCihazlar);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(2383, 1328);
            this.panel1.TabIndex = 0;
            // 
            // DownCihazlar
            // 
            this.DownCihazlar.AutoSize = true;
            this.DownCihazlar.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.DownCihazlar.Location = new System.Drawing.Point(159, 12);
            this.DownCihazlar.Name = "DownCihazlar";
            this.DownCihazlar.Size = new System.Drawing.Size(249, 24);
            this.DownCihazlar.TabIndex = 0;
            this.DownCihazlar.Text = "Down Olan Cihazları Göster";
            this.DownCihazlar.UseVisualStyleBackColor = true;
            this.DownCihazlar.CheckedChanged += new System.EventHandler(this.DownCihazlar_CheckedChanged_1);
            // 
            // CizgiKaldirChckBox
            // 
            this.CizgiKaldirChckBox.AutoSize = true;
            this.CizgiKaldirChckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(162)));
            this.CizgiKaldirChckBox.Location = new System.Drawing.Point(12, 12);
            this.CizgiKaldirChckBox.Name = "CizgiKaldirChckBox";
            this.CizgiKaldirChckBox.Size = new System.Drawing.Size(141, 24);
            this.CizgiKaldirChckBox.TabIndex = 1;
            this.CizgiKaldirChckBox.Text = "Çizgileri Kaldır";
            this.CizgiKaldirChckBox.UseVisualStyleBackColor = true;
            this.CizgiKaldirChckBox.CheckedChanged += new System.EventHandler(this.CizgiKaldirChckBox_CheckedChanged_1);
            // 
            // Harita
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(2383, 1328);
            this.Controls.Add(this.CizgiKaldirChckBox);
            this.Controls.Add(this.panel1);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "Harita";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Harita";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.Harita_Paint);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox CizgiKaldirChckBox;
        private System.Windows.Forms.CheckBox DownCihazlar;
    }
}