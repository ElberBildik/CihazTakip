using System;
using System.Collections.Generic;
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

            // Panel üzerine paint ve mouseclick eventlerini bağla
            this.panel1.Paint += Harita_Paint;
            this.panel1.MouseClick += Harita_MouseClick;

            VeritabanindanCihazlariYukle();

            durumGuncellemeTimer = new Timer();
            durumGuncellemeTimer.Interval = 1000; // 1 saniye
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
            public int SwitchRecNo { get; set; }  // Switch ID
            public Shape Shape { get; set; }      // Cihazın şekli
            public string GrupKod { get; set; }   // Grup kodu
        }


        // Cihazların şekillerini tanımlamak için enum
        private enum Shape
        {
            Circle,
            Rectangle,
            Triangle,
            Diamond
        }

        private void VeritabanindanCihazlariYukle()//cihazın özelliklerini alıp hangi simgeyle uyumlu oldugunu görüntüledik 
        {
            try
            {
                cihazlar.Clear(); // Mevcut cihazları temizle

                using (SqlConnection connection = new SqlConnection(connectionString))//grup kodu burada aldım
                {
                    string query = @"
                SELECT c.RecNo, c.X, c.Y, c.IPNo, c.Aciklama, c.Durum, c.MarkaModel, 
                       c.SwitchRecNo, cg.Kod AS GrupKod
                FROM Cihaz c
                INNER JOIN CihazGrup cg ON c.GrupRecNo = cg.RecNo
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
                                MarkaModel = reader.IsDBNull(6) ? "N/A" : reader.GetString(6),
                                SwitchRecNo = reader.GetInt32(7),
                                GrupKod = reader.GetString(8) // Grup kodunu al
                            };

                            // Duruma göre renk belirleme
                            if (cihaz.Durum.Equals("UP", StringComparison.OrdinalIgnoreCase))
                                cihaz.PointColor = Color.Green;
                            else if (cihaz.Durum.Contains("Down"))
                                cihaz.PointColor = Color.Red;
                            else
                                cihaz.PointColor = Color.Orange;

                            // Grup koduna göre şekil belirleme
                            switch (cihaz.GrupKod)
                            {
                                case "KGS":
                                    cihaz.Shape = Shape.Triangle;  // KGS için üçgen
                                    break;
                                case "Kamera":
                                    cihaz.Shape = Shape.Rectangle; // Kamera için dikdörtgen
                                    break;
                                case "Switch":
                                    cihaz.Shape = Shape.Diamond; // Kamera için dikdörtgen
                                    break;
                                default:
                                    cihaz.Shape = Shape.Circle;    // Diğer gruplar için daire
                                    break;

                            }

                            cihazlar.Add(cihaz);
                        }
                    }
                }

                this.panel1.Invalidate(); // Haritayı yeniden çiz
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cihazlar yüklenirken hata oluştu: " + ex.Message,
                    "Veritabanı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Harita_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            // Tüm cihazları çiz
            foreach (var cihaz in cihazlar)
            {
                using (Brush brush = new SolidBrush(cihaz.PointColor))
                {
                    int diameter = pointRadius * 2;

                    // Cihazın türüne göre şekil çiz
                    switch (cihaz.Shape)
                    {
                        case Shape.Triangle:
                            Point[] trianglePoints = {
                        new Point(cihaz.X, cihaz.Y - pointRadius),
                        new Point(cihaz.X - pointRadius, cihaz.Y + pointRadius),
                        new Point(cihaz.X + pointRadius, cihaz.Y + pointRadius)
                    };
                            g.FillPolygon(brush, trianglePoints);
                            break;

                        case Shape.Rectangle:
                            g.FillRectangle(brush, cihaz.X - pointRadius, cihaz.Y - pointRadius, diameter, diameter);
                            break;

                        case Shape.Diamond:
                            Point[] diamondPoints = {
                        new Point(cihaz.X, cihaz.Y - pointRadius),
                        new Point(cihaz.X - pointRadius, cihaz.Y),
                        new Point(cihaz.X, cihaz.Y + pointRadius),
                        new Point(cihaz.X + pointRadius, cihaz.Y)
                    };
                            g.FillPolygon(brush, diamondPoints);
                            break;

                        case Shape.Circle:
                            g.FillEllipse(brush, cihaz.X - pointRadius, cihaz.Y - pointRadius, diameter, diameter);
                            break;
                    }
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
                            sb.AppendLine($"IP: {reader["IPNo"] ?? "N/A"}");
                            sb.AppendLine($"Cihaz: {reader["Aciklama"] ?? "N/A"}");

                            string durum = reader["Durum"].ToString() ?? "N/A";
                            sb.AppendLine($"Durum: {durum}");

                            sb.AppendLine($"Model: {reader["MarkaModel"] ?? "N/A"}");
                            sb.AppendLine($"Grup: {reader["GrupAdi"] ?? "N/A"}");
                            sb.AppendLine($"Switch Port: {reader["SwitchPortNo"] ?? "N/A"}");
                            sb.AppendLine($"Enerji Pano: {reader["EnerjiPanoNo"] ?? "N/A"}");
                            sb.AppendLine($"Sigorta No: {reader["EnerjiPanoSigortaNo"] ?? "N/A"}");

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

        private void Harita_Load(object sender, EventArgs e)
        {
        }
    }
}
