using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cihaz_Takip_Uygulaması
{
    public partial class Harita : Form
    {
        private List<CihazBilgi> cihazlar = new List<CihazBilgi>();
        private int pointRadius = 8;
        private string connectionString = "Data Source=ES-BT14\\SQLEXPRESS;Initial Catalog=CihazTakip;Integrated Security=True";
        private Timer durumGuncellemeTimer;

        public Harita()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            this.panel1.Paint += Harita_Paint;
            this.panel1.MouseClick += Harita_MouseClick;

            VeritabanindanCihazlariYukle();

            durumGuncellemeTimer = new Timer();
            durumGuncellemeTimer.Interval = 1000;
            durumGuncellemeTimer.Tick += DurumGuncellemeTimer_Tick;
            durumGuncellemeTimer.Start();
        }

        private void DurumGuncellemeTimer_Tick(object sender, EventArgs e)
        {
            VeritabanindanCihazlariYukle();
        }

        private class CihazBilgi
        {
            public int RecNo { get; set; }
            public int X { get; set; }
            public int Y { get; set; }
            public string IPNo { get; set; }
            public string Aciklama { get; set; }
            public string Durum { get; set; }
            public string MarkaModel { get; set; }
            public Color PointColor { get; set; }
            public int SwitchRecNo { get; set; }
            public Shape Shape { get; set; }
            public string GrupKod { get; set; }
            public string EnerjiPanoNo { get; set; } // Yeni özellik
        }


        private enum Shape
        {
            Circle,
            Rectangle,
            Triangle,
            Diamond
        }

        private void VeritabanindanCihazlariYukle()
        {
            try
            {
                var yeniCihazlar = new List<CihazBilgi>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // EnerjiPanoNo'yu almak için SQL sorgusunu güncelleyin
                    string query = @"
                SELECT c.RecNo, c.X, c.Y, c.IPNo, c.Aciklama, c.Durum, c.MarkaModel, 
                       c.SwitchRecNo, cg.Kod AS GrupKod, c.EnerjiPanoNo
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
                                GrupKod = reader.GetString(8),
                                // EnerjiPanoNo'yu okuyun
                                EnerjiPanoNo = reader.IsDBNull(9) ? null : reader.GetString(9)
                            };

                            cihaz.PointColor = cihaz.Durum.Equals("UP", StringComparison.OrdinalIgnoreCase) ? Color.Green : Color.Red;

                            switch (cihaz.GrupKod)
                            {
                                case "Enerji panosu":
                                    cihaz.Shape = Shape.Rectangle;
                                    break;
                                case "Data Switch":
                                case "Kamera Switch":
                                    cihaz.Shape = Shape.Diamond;
                                    break;
                                case "Kamera":
                                    cihaz.Shape = Shape.Triangle;
                                    break;
                                default:
                                    cihaz.Shape = Shape.Circle;
                                    break;
                            }

                            yeniCihazlar.Add(cihaz);
                        }
                    }
                }

                // Güncellendi mi kontrol et
                bool degisiklikVar = yeniCihazlar.Count != cihazlar.Count;
                if (!degisiklikVar)
                {
                    for (int i = 0; i < yeniCihazlar.Count; i++)
                    {
                        var a = yeniCihazlar[i];
                        var b = cihazlar[i];
                        if (a.RecNo != b.RecNo || a.X != b.X || a.Y != b.Y || a.Durum != b.Durum || a.EnerjiPanoNo != b.EnerjiPanoNo)
                        {
                            degisiklikVar = true;
                            break;
                        }
                    }
                }

                if (degisiklikVar)
                {
                    cihazlar = yeniCihazlar;
                    this.panel1.Invalidate();
                }
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

            // 1. Enerji panosuna bağlı cihazları çiz
            foreach (var cihaz in cihazlar)
            {
                if (!string.IsNullOrEmpty(cihaz.EnerjiPanoNo))
                {
                    // Bağlı olduğu enerji panosunu bul
                    var enerjiPanosu = cihazlar.FirstOrDefault(p => p.Aciklama == cihaz.EnerjiPanoNo && p.GrupKod == "Enerji panosu");

                    if (enerjiPanosu != null)
                    {
                        using (Pen redPen = new Pen(Color.Red, 2))
                        {
                            g.DrawLine(redPen, cihaz.X, cihaz.Y, enerjiPanosu.X, enerjiPanosu.Y);
                        }
                    }
                }
            }

            // 2. Switch bağlantılarını çiz
            foreach (var cihaz in cihazlar)
            {
                if (cihaz.SwitchRecNo != 0 && cihaz.GrupKod != "Enerji panosu") //Enerji panosuna bağlı cihazları atla
                {
                    var switchCihaz = cihazlar.FirstOrDefault(s => s.RecNo == cihaz.SwitchRecNo && (s.GrupKod == "Data Switch" || s.GrupKod == "Kamera Switch"));

                    if (switchCihaz != null)
                    {
                        Color renk = Color.Gray;

                        switch (cihaz.GrupKod)
                        {
                            case "Kamera":
                                renk = Color.Chartreuse; // Kameralar için yeşil
                                break;
                            case "Yazıcı":
                                renk = Color.DarkOrange;
                                break;
                            case "KGS":
                                renk = Color.Purple;
                                break;
                            default:
                                renk = Color.BlueViolet;
                                break;
                        }

                        using (Pen kalem = new Pen(renk, 3))
                        {
                            g.DrawLine(kalem, switchCihaz.X, switchCihaz.Y, cihaz.X, cihaz.Y);
                        }
                    }
                }
            }

            // 3. Birbirine bağlı switch'leri çiz
            foreach (var cihaz in cihazlar)
            {
                if (cihaz.GrupKod == "Data Switch" || cihaz.GrupKod == "Kamera Switch")
                {
                    // Switch'in bağlı olduğu switch'i bul
                    var bagliSwitch = cihazlar.FirstOrDefault(s => s.RecNo == cihaz.SwitchRecNo && (s.GrupKod == "Data Switch" || s.GrupKod == "Kamera Switch"));

                    if (bagliSwitch != null)
                    {
                        using (Pen kahverengiKalem = new Pen(Color.Brown, 2))
                        {
                            g.DrawLine(kahverengiKalem, cihaz.X, cihaz.Y, bagliSwitch.X, bagliSwitch.Y);
                        }
                    }
                }
            }

            // 4. Cihazları çiz (şekiller)
            foreach (var cihaz in cihazlar)
            {
                using (Brush brush = new SolidBrush(cihaz.PointColor))
                {
                    int diameter = pointRadius * 2;

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
            MessageBox.Show($"Tıklanan Nokta:\nX: {e.X}, Y: {e.Y}", "Lokasyon");

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

                            MessageBox.Show(sb.ToString(), "Cihaz Bilgisi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Cihaz bilgisi bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void Harita_Load(object sender, EventArgs e)
        {
        }
    }
}
