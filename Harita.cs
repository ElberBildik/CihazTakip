using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Cihaz_Takip_Uygulaması
{
    public partial class Harita : Form
    {
        // Cihazları saklamak için liste
        private List<CihazBilgi> cihazlar = new List<CihazBilgi>();
        private int pointRadius = 5; // Nokta büyüklüğü

        // Connection string 
        private string connectionString = "Data Source=ES-BT14\\SQLEXPRESS;Initial Catalog=CihazTakip;Integrated Security=True";

        // Durum güncelleme için Timer
        private Timer durumGuncellemeTimer;

        public Harita()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.Paint += Harita_Paint;
            this.MouseClick += Harita_MouseClick;
            VeritabanindanCihazlariYukle();

            durumGuncellemeTimer = new Timer();
            durumGuncellemeTimer.Interval = 10000; // 10 saniye
            durumGuncellemeTimer.Tick += DurumGuncellemeTimer_Tick;
            durumGuncellemeTimer.Start();
        }

        // Timer Tick event
        private void DurumGuncellemeTimer_Tick(object sender, EventArgs e)
        {
            VeritabanindanCihazlariYukle(); // Her saniyede güncelle
        }

        // Cihaz bilgilerini temsil eden iç sınıf
        private class CihazBilgi
        {
            public int RecNo { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string IPNo { get; set; }
            public string Aciklama { get; set; }
            public string Durum { get; set; }
            public string MarkaModel { get; set; }
            public Color PointColor { get; set; } // Cihazın durumuna göre renk
        }

        private void VeritabanindanCihazlariYukle()
        {
            try
            {
                cihazlar.Clear(); // Mevcut cihazları temizle

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT c.RecNo, c.X, c.Y, c.IPNo, c.Aciklama, c.Durum, c.MarkaModel
                        FROM Cihaz c
                        WHERE c.X IS NOT NULL AND c.Y IS NOT NULL";

                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CihazBilgi cihaz = new CihazBilgi
                            {
                                RecNo = reader.GetInt32(0),
                                X = reader.GetInt32(1),
                                Y = reader.GetInt32(2),
                                IPNo = reader.IsDBNull(3) ? "N/A" : reader.GetString(3),
                                Aciklama = reader.IsDBNull(4) ? "N/A" : reader.GetString(4),
                                Durum = reader.IsDBNull(5) ? "N/A" : reader.GetString(5),
                                MarkaModel = reader.IsDBNull(6) ? "N/A" : reader.GetString(6)
                            };

                            // Duruma göre renk belirleme
                            if (cihaz.Durum != null)
                            {
                                if (cihaz.Durum.Equals("UP", StringComparison.OrdinalIgnoreCase))
                                    cihaz.PointColor = Color.Green;
                                else if (cihaz.Durum.Contains("Down"))
                                    cihaz.PointColor = Color.Red;
                                else
                                    cihaz.PointColor = Color.Orange;
                            }
                            else
                            {
                                cihaz.PointColor = Color.Gray;
                            }

                            cihazlar.Add(cihaz);
                        }
                    }
                }

                this.Invalidate(); // Haritayı yeniden çiz
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cihazlar yüklenirken hata oluştu: " + ex.Message,
                    "Veritabanı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Harita_Paint(object sender, PaintEventArgs e)//cihazları çizdik 
        {
            Graphics g = e.Graphics;

            // Tüm cihazları çiz
            foreach (var cihaz in cihazlar)
            {
                using (Brush brush = new SolidBrush(cihaz.PointColor))
                {
                    int diameter = pointRadius * 2;
                    g.FillEllipse(brush,
                        cihaz.X - pointRadius,
                        cihaz.Y - pointRadius,
                        diameter,
                        diameter);
                }
            }
        }

        private void Harita_MouseClick(object sender, MouseEventArgs e)
        {
            // Tıklanan yerin koordinatını göster
            MessageBox.Show($"Tıklanan Nokta:\nX: {e.X}, Y: {e.Y}", "Lokasyon");

            // Tıklanan yere en yakın cihazı bul
            CihazBilgi enYakinCihaz = null;
            double enKucukMesafe = double.MaxValue;

            foreach (var cihaz in cihazlar)
            {
                int dx = e.X - cihaz.X;
                int dy = e.Y - cihaz.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                if (distance <= pointRadius + 5 && distance < enKucukMesafe)
                {
                    enKucukMesafe = distance;
                    enYakinCihaz = cihaz;
                }
            }

            // Eğer yakında bir cihaz varsa bilgilerini göster
            if (enYakinCihaz != null)
            {
                GuncelCihazBilgisiGoster(enYakinCihaz.RecNo);
            }
        }


        private void GuncelCihazBilgisiGoster(int cihazRecNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT c.RecNo, c.IPNo, c.Aciklama, c.Durum, c.MarkaModel, 
                               c.SwitchPortNo, c.EnerjiPanoNo, c.EnerjiPanoSigortaNo,
                               cg.Aciklama as GrupAdi 
                        FROM Cihaz c
                        LEFT JOIN CihazGrup cg ON c.GrupRecNo = cg.RecNo
                        WHERE c.RecNo = @RecNo";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@RecNo", cihazRecNo);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            StringBuilder sb = new StringBuilder();
                            sb.AppendLine($"IP: {reader["IPNo"]}");
                            sb.AppendLine($"Cihaz: {reader["Aciklama"]}");

                            string durum = reader["Durum"].ToString();
                            sb.AppendLine($"Durum: {durum}");

                            sb.AppendLine($"Model: {reader["MarkaModel"]}");
                            sb.AppendLine($"Grup: {reader["GrupAdi"]}");
                            sb.AppendLine($"Switch Port: {reader["SwitchPortNo"]}");
                            sb.AppendLine($"Enerji Pano: {reader["EnerjiPanoNo"]}");
                            sb.AppendLine($"Sigorta No: {reader["EnerjiPanoSigortaNo"]}");

                            MessageBox.Show(sb.ToString(), "Cihaz Bilgisi",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Cihaz bilgisi bulunamadı.", "Bilgi",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cihaz bilgisi alınırken hata oluştu: " + ex.Message,
                    "Veritabanı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Form üzerindeki yenile butonuna basıldığında manuel olarak da yenilenebilir
        private void Harita_Refresh_Click(object sender, EventArgs e)
        {
            VeritabanindanCihazlariYukle();
        }
    }
}
