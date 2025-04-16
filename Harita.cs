using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Cihaz_Takip_Uygulaması
{
    
    public partial class Harita : Form
    {
        Graphics g;
        private List<CihazBilgi> cihazlar = new List<CihazBilgi>();
        private int pointRadius = 8;
        private string connectionString = "Data Source=ES-BT14\\SQLEXPRESS;Initial Catalog=CihazTakip;Integrated Security=True";
        private Timer durumGuncellemeTimer;
        
        

        private bool cizgileriGoster = true;
        private bool enerjiPanolariniGoster = true;
        private bool clientleriGoster = true;
        private bool downDurumuGoster = true;

        // Zoom için yeni değişkenler
        private float zoomFactor = 1.0f;
        private const float zoomIncrement = 0.1f;
        private const float minZoom = 0.5f;
        private const float maxZoom = 3.0f;
        private Point lastPanPoint;
        private bool isPanning = false;
        private Point panOffset = new Point(0, 0);
        private Image backgroundImage = null;
        private Image originalBackgroundImage = null; // Orijinal resmi saklamak için
        private Size originalImageSize; // Orijinal boyutları saklamak için

        public Harita()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.panel1.Paint += Harita_Paint;
            this.panel1.MouseClick += Harita_MouseClick;

            // Zoom ve Pan için yeni olay işleyicileri ekleyin
            this.panel1.MouseWheel += Panel1_MouseWheel;
            this.panel1.MouseDown += Panel1_MouseDown;
            this.panel1.MouseMove += Panel1_MouseMove;
            this.panel1.MouseUp += Panel1_MouseUp;

            // Arka plan resmini yükleyin (eğer varsa)
            string imagePath = @"C:\Users\ebildik\Desktop\Genel Layout.PNG";
            backgroundImage = Image.FromFile(imagePath);
            originalBackgroundImage = Image.FromFile(imagePath);


            VeritabanindanCihazlariYukle();
            durumGuncellemeTimer = new Timer();
            durumGuncellemeTimer.Interval = 1000;
            durumGuncellemeTimer.Tick += DurumGuncellemeTimer_Tick;
            durumGuncellemeTimer.Start();

            // Zoom kontrolleri için butonlar ekleyin
            InitializeZoomControls();

            // Formun Resize olayını işle
            this.Resize += Harita_Resize;

            // Formun BackgroundImageLayout özelliğini Zoom olarak ayarla
            this.BackgroundImageLayout = ImageLayout.Zoom;
        }

        private void InitializeZoomControls()
        {
            // Zoom In butonu
            Button btnZoomIn = new Button();
            btnZoomIn.Text = "+";
            btnZoomIn.Size = new Size(30, 30);
            btnZoomIn.Location = new Point(10, 10);
            btnZoomIn.Click += (s, e) => {
                ZoomIn();
            };
            this.Controls.Add(btnZoomIn);

            // Zoom Out butonu
            Button btnZoomOut = new Button();
            btnZoomOut.Text = "-";
            btnZoomOut.Size = new Size(30, 30);
            btnZoomOut.Location = new Point(45, 10);
            btnZoomOut.Click += (s, e) => {
                ZoomOut();
            };
            this.Controls.Add(btnZoomOut);

            // Reset Zoom butonu
            Button btnResetZoom = new Button();
            btnResetZoom.Text = "1:1";
            btnResetZoom.Size = new Size(30, 30);
            btnResetZoom.Location = new Point(80, 10);
            btnResetZoom.Click += (s, e) => {
                ResetZoom();
            };
            this.Controls.Add(btnResetZoom);

            // İsteğe bağlı: Zoom durumunu gösteren label
            Label lblZoomStatus = new Label();
            lblZoomStatus.Text = "Zoom: 100%";
            lblZoomStatus.AutoSize = true;
            lblZoomStatus.Location = new Point(120, 15);
            lblZoomStatus.Name = "lblZoomStatus";
            this.Controls.Add(lblZoomStatus);
        }

        private void ZoomIn()
        {
            if (zoomFactor < maxZoom)
            {
                zoomFactor += zoomIncrement;
                UpdateZoomStatus();
                panel1.Invalidate();
            }
        }

        private void ZoomOut()
        {
            if (zoomFactor > minZoom)
            {
                zoomFactor -= zoomIncrement;
                UpdateZoomStatus();
                panel1.Invalidate();
            }
        }

        private void ResetZoom()
        {
            zoomFactor = 1.0f;
            panOffset = new Point(0, 0);
            UpdateZoomStatus();
            panel1.Invalidate();
        }

        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Fare tekerleği ile zoom yapma
            float oldZoom = zoomFactor;

            // Zoom yönü
            if (e.Delta > 0)
            {
                if (zoomFactor < maxZoom)
                    zoomFactor += zoomIncrement;
            }
            else
            {
                if (zoomFactor > minZoom)
                    zoomFactor -= zoomIncrement;
            }

            // Zoom merkezi olarak fare pozisyonunu kullan
            if (oldZoom != zoomFactor)
            {
                // Fare konumunu odak noktası olarak ayarla
                Point mousePoint = e.Location;

                // Zoom öncesi fare konumunun dönüşümü
                Point oldPoint = new Point(
                    (int)((mousePoint.X - panOffset.X) / oldZoom),
                    (int)((mousePoint.Y - panOffset.Y) / oldZoom)
                );

                // Zoom sonrası fare konumunun dönüşümü
                Point newPoint = new Point(
                    (int)((mousePoint.X - panOffset.X) / zoomFactor),
                    (int)((mousePoint.Y - panOffset.Y) / zoomFactor)
                );

                // Kaydırma ofsetini ayarla
                panOffset.X += (int)((newPoint.X - oldPoint.X) * zoomFactor);
                panOffset.Y += (int)((newPoint.Y - oldPoint.Y) * zoomFactor);

                UpdateZoomStatus();
                panel1.Invalidate();
            }
        }

        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            // Fare orta tuşu veya sol tuş ile pan yapmaya başla
            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && ModifierKeys == Keys.Control))
            {
                isPanning = true;
                lastPanPoint = e.Location;
                panel1.Cursor = Cursors.Hand;
            }
        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                // Pan yaparken ofseti güncelle
                panOffset.X += e.X - lastPanPoint.X;
                panOffset.Y += e.Y - lastPanPoint.Y;
                lastPanPoint = e.Location;
                panel1.Invalidate();
            }
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            // Pan'ı durdur
            if (isPanning)
            {
                isPanning = false;
                panel1.Cursor = Cursors.Default;
            }
        }

        private void Harita_Paint(object sender, PaintEventArgs e)
        {
             g = e.Graphics;

            // Yüksek kaliteli render için ayarlar
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Dönüşüm matrisini ayarla
            Matrix transformMatrix = new Matrix();
            transformMatrix.Translate(panOffset.X, panOffset.Y); // Önce pan
            transformMatrix.Scale(zoomFactor, zoomFactor); // Sonra zoom
            g.Transform = transformMatrix;

            // Arka plan resmini çiz (eğer varsa)
            if (backgroundImage != null)
            {
                // Arka plan resmini çizme
                g.DrawImage(backgroundImage, 0, 0, this.Width, this.Height);
                
                
            }
            if (enerjiPanolariniGoster)
            {
                foreach (var cihaz in cihazlar)
                {
                    if (!string.IsNullOrEmpty(cihaz.EnerjiPanoNo))
                    {
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
            }

            if (cizgileriGoster)
            {
                foreach (var cihaz in cihazlar)
                {
                    if (cihaz.SwitchRecNo != 0 && cihaz.GrupKod != "Enerji panosu")
                    {
                        var switchCihaz = cihazlar.FirstOrDefault(s => s.RecNo == cihaz.SwitchRecNo && (s.GrupKod == "Data Switch" || s.GrupKod == "Kamera Switch"));

                        if (switchCihaz != null)
                        {
                            Color renk = Color.Gray;

                            switch (cihaz.GrupKod)
                            {
                                case "Kamera":
                                    renk = Color.Chartreuse;
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
            }

            if (cizgileriGoster)
            {
                foreach (var cihaz in cihazlar)
                {
                    if (cihaz.GrupKod == "Data Switch" || cihaz.GrupKod == "Kamera Switch")
                    {
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
            }

            foreach (var cihaz in cihazlar)
            {
                if (!clientleriGoster && cihaz.GrupKod != "Enerji panosu" && cihaz.GrupKod != "Data Switch" && cihaz.GrupKod != "Kamera Switch" && cihaz.GrupKod != "Kamera" && cihaz.GrupKod != "PLC")
                {
                    continue;
                }

                if (!downDurumuGoster && cihaz.Durum.Equals("DOWN", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

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

                        case Shape.Star:
                            Point[] starPoints = {
                                new Point(cihaz.X, cihaz.Y - pointRadius),
                                new Point(cihaz.X + (int)(pointRadius * 0.4), cihaz.Y - (int)(pointRadius * 0.4)),
                                new Point(cihaz.X + pointRadius, cihaz.Y - (int)(pointRadius * 0.4)),
                                new Point(cihaz.X + (int)(pointRadius * 0.6), cihaz.Y + (int)(pointRadius * 0.2)),
                                new Point(cihaz.X + (int)(pointRadius * 0.8), cihaz.Y + pointRadius),
                                new Point(cihaz.X, cihaz.Y + (int)(pointRadius * 0.6)),
                                new Point(cihaz.X - (int)(pointRadius * 0.8), cihaz.Y + pointRadius),
                                new Point(cihaz.X - (int)(pointRadius * 0.6), cihaz.Y + (int)(pointRadius * 0.2)),
                                new Point(cihaz.X - pointRadius, cihaz.Y - (int)(pointRadius * 0.4)),
                                new Point(cihaz.X - (int)(pointRadius * 0.4), cihaz.Y - (int)(pointRadius * 0.4)),
                            };
                            g.FillPolygon(brush, starPoints);
                            break;

                        case Shape.PLC:
                            using (Brush plcBrush = new SolidBrush(Color.Orange))
                            {
                                g.FillRectangle(plcBrush, cihaz.X - pointRadius - 4, cihaz.Y - pointRadius, pointRadius * 2 + 8, pointRadius * 2);
                            }

                            using (Brush portBrush = new SolidBrush(Color.DarkSlateGray))
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    g.FillRectangle(portBrush, cihaz.X - 6 + i * 6, cihaz.Y - 2, 3, 4);
                                }
                            }

                            using (Font font = new Font("Arial", 6))
                            {
                                g.DrawString("PLC", font, Brushes.Black, cihaz.X - 8, cihaz.Y - 12);
                            }
                            break;

                        case Shape.Rectangle:
                            using (GraphicsPath path = new GraphicsPath())
                            {
                                Rectangle rect = new Rectangle(cihaz.X - pointRadius, cihaz.Y - pointRadius, pointRadius * 2, pointRadius * 2 + 10);
                                int cornerRadius = 6;

                                path.AddArc(rect.X, rect.Y, cornerRadius, cornerRadius, 180, 90);
                                path.AddArc(rect.Right - cornerRadius, rect.Y, cornerRadius, cornerRadius, 270, 90);
                                path.AddArc(rect.Right - cornerRadius, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 0, 90);
                                path.AddArc(rect.X, rect.Bottom - cornerRadius, cornerRadius, cornerRadius, 90, 90);
                                path.CloseFigure();

                                using (Brush brushBody = new SolidBrush(Color.Cyan))
                                using (Pen borderPen = new Pen(Color.Blue, 2))
                                {
                                    g.FillPath(brushBody, path);
                                    g.DrawPath(borderPen, path);
                                }
                            }

                            using (Pen kapakKalem = new Pen(Color.Black, 2))
                            {
                                g.DrawLine(kapakKalem, cihaz.X - pointRadius + 2, cihaz.Y, cihaz.X + pointRadius - 2, cihaz.Y);
                            }

                            using (Brush sigortaBrush = new SolidBrush(Color.Black))
                            {
                                for (int i = 0; i < 3; i++)
                                {
                                    g.FillEllipse(sigortaBrush, cihaz.X - 6 + i * 6, cihaz.Y + 4, 3, 3);
                                }
                            }
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
            // Mouse tıklaması koordinatlarını zoom faktöründen geri çevirin
            Point transformedPoint = new Point(
                (int)((e.X - panOffset.X) / zoomFactor),
                (int)((e.Y - panOffset.Y) / zoomFactor)
            );

            MessageBox.Show($"Tıklanan Nokta:\nX: {transformedPoint.X}, Y: {transformedPoint.Y}", "Lokasyon");

            CihazBilgi enYakinCihaz = null;
            double enKucukMesafe = double.MaxValue;

            foreach (var cihaz in cihazlar)
            {
                int dx = transformedPoint.X - cihaz.X;
                int dy = transformedPoint.Y - cihaz.Y;
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

        // Arka plan resmi ayarlamak için yeni metod
        public void SetBackgroundImage(string imagePath)
        {
            try
            {
                if (System.IO.File.Exists(imagePath))
                {
                    if (backgroundImage != null)
                    {
                        backgroundImage.Dispose(); // Önceki resmi temizle
                    }

                    backgroundImage = Image.FromFile(imagePath);
                    originalBackgroundImage = Image.FromFile(imagePath); // Orijinal resmi sakla
                    originalImageSize = backgroundImage.Size; // Orijinal boyutları sakla
                    panel1.Invalidate();
                }
                else
                {
                    MessageBox.Show("Belirtilen arka plan resmi bulunamadı: " + imagePath,
                        "Resim Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arka plan resmi yüklenirken hata oluştu: " + ex.Message,
                    "Resim Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void UpdateZoomStatus()
        {
            // Form üzerindeki zoom etiketini güncelle
            if (this.Controls["lblZoomStatus"] is Label lblZoom)
            {
                lblZoom.Text = $"Zoom: {zoomFactor * 100:0}%";
            }
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
            public string EnerjiPanoNo { get; set; }
        }

        private enum Shape
        {
            Circle,
            Rectangle,
            Triangle,
            Diamond,
            Star,
            Camera,
            PLC
        }

        private void VeritabanindanCihazlariYukle()
        {
            try
            {
                var yeniCihazlar = new List<CihazBilgi>();

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
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
                                EnerjiPanoNo = reader.IsDBNull(9) ? null : reader.GetString(9)
                            };

                            cihaz.PointColor = cihaz.Durum.Equals("UP", StringComparison.OrdinalIgnoreCase) ? Color.Green : Color.Red;

                            switch (cihaz.GrupKod)
                            {
                                case "Enerji panosu":
                                    cihaz.Shape = Shape.Rectangle;
                                    break;
                                case "Data Switch":
                                    cihaz.Shape = Shape.Star;
                                    break;
                                case "Kamera Switch":
                                    cihaz.Shape = Shape.Diamond;
                                    break;
                                case "Kamera":
                                    cihaz.Shape = Shape.Triangle;
                                    break;
                                case "PLC":
                                    cihaz.Shape = Shape.PLC;
                                    break;
                                default:
                                    cihaz.Shape = Shape.Circle;
                                    break;
                            }

                            yeniCihazlar.Add(cihaz);
                        }
                    }
                }

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
            // Form yüklendiğinde yapılacak işlemler
        }

        private void CizgiKaldirChckBox_CheckedChanged(object sender, EventArgs e)
        {
            cizgileriGoster = !CizgiKaldirChckBox.Checked;
            panel1.Invalidate();
        }

        private void DownCihazlar_CheckedChanged(object sender, EventArgs e)
        {
            // Harita panelini temizle
            panel1.Controls.Clear();
        }


        private void CizgiKaldirChckBox_CheckedChanged_1(object sender, EventArgs e)
        {
            cizgileriGoster = !CizgiKaldirChckBox.Checked;
            panel1.Invalidate();
        }

        private void Harita_Resize(object sender, EventArgs e)
        {
            panel1.Invalidate(); // Yeniden çizimi tetikle
        }

    }
}
