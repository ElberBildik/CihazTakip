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

        // Zoom için değişkenler
        private float zoomFactor = 1.0f;
        private float previousZoomFactor = 1.0f;
        private const float zoomIncrement = 0.1f;
        private const float minZoom = 0.5f;
        private const float maxZoom = 3.0f;
        private Point lastPanPoint;
        private bool isPanning = false;
        private Image backgroundImage = null;
        private Image originalBackgroundImage = null;
        private Size originalImageSize;

        // Viewport takibi için eklenen değişkenler
        private PointF viewportCenter;

        public Harita()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            // AutoScroll özelliğini aktif et
            this.panel1.AutoScroll = true;
        
            this.panel1.Paint += Harita_Paint;
            this.panel1.MouseClick += Harita_MouseClick;

            // Zoom ve Pan için olay işleyicileri
            this.panel1.MouseWheel += Panel1_MouseWheel;
            this.panel1.MouseDown += Panel1_MouseDown;
            this.panel1.MouseMove += Panel1_MouseMove;
            this.panel1.MouseUp += Panel1_MouseUp;

            // Arka plan resmini yükle
            string imagePath = @"C:\Users\ebildik\Desktop\Genel Layout.PNG";
            try
            {
                backgroundImage = Image.FromFile(imagePath);
                originalBackgroundImage = Image.FromFile(imagePath);
                originalImageSize = backgroundImage.Size;

                // Başlangıçta viewport merkezini ayarla
                viewportCenter = new PointF(panel1.Width / 2f, panel1.Height / 2f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arka plan resmi yüklenirken hata oluştu: " + ex.Message,
                    "Resim Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            VeritabanindanCihazlariYukle();
            durumGuncellemeTimer = new Timer();
            durumGuncellemeTimer.Interval = 100;
            durumGuncellemeTimer.Tick += DurumGuncellemeTimer_Tick;
            durumGuncellemeTimer.Start();

            this.Resize += Harita_Resize;

            // Başlangıçta biraz zoom yap
            zoomFactor = 1.1f;
            UpdateScrollBars();

            // Zoom durumunu gösteren label ekle
            Label lblZoomStatus = new Label();
            lblZoomStatus.Name = "lblZoomStatus";
            lblZoomStatus.Text = "Zoom: 110%";
            lblZoomStatus.AutoSize = true;
            lblZoomStatus.Location = new Point(10, 10); // Konumu ayarlayın
            this.Controls.Add(lblZoomStatus);
        }

        private void UpdateScrollBars()
        {
            if (backgroundImage != null)
            {
                // Zoom'a bağlı olarak sanal boyutları hesapla
                int virtualWidth = (int)(originalImageSize.Width * zoomFactor);
                int virtualHeight = (int)(originalImageSize.Height * zoomFactor);

                // Kaydırma çubuklarını sanal boyutlara göre ayarla
                panel1.AutoScrollMinSize = new Size(Math.Max(virtualWidth, panel1.Width), Math.Max(virtualHeight, panel1.Height));

                // Scrollbar'ların hemen görünmesi için
                panel1.PerformLayout();
            }
        }

        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldZoom = zoomFactor;

            // Scroll pozisyonu ve fare konumunu belge (sanal) koordinatlarına dönüştür
            Point scrollPos = new Point(-panel1.AutoScrollPosition.X, -panel1.AutoScrollPosition.Y);
            PointF mouseDocPos = new PointF(
                (e.Location.X + scrollPos.X) / oldZoom,
                (e.Location.Y + scrollPos.Y) / oldZoom
            );

            // Zoom yönünü belirle
            if (e.Delta > 0 && zoomFactor < maxZoom)
            {
                zoomFactor += zoomIncrement;
            }
            else if (e.Delta < 0 && zoomFactor > minZoom)
            {
                zoomFactor -= zoomIncrement;
            }
            else
            {
                return; // Zoom sınırlarında değişiklik olmadı
            }

            // Yeni sanal pozisyonları hesapla
            PointF newScrollPos = new PointF(
                mouseDocPos.X * zoomFactor - e.Location.X,
                mouseDocPos.Y * zoomFactor - e.Location.Y
            );

            // Kaydırma çubuklarını güncelle
            UpdateScrollBars();

            // Yeni kaydırma pozisyonunu uygula (negative değer olmamalı)
            panel1.AutoScrollPosition = new Point(
                Math.Max((int)newScrollPos.X, 0),
                Math.Max((int)newScrollPos.Y, 0)
            );

            // Zoom durumunu güncelle ve yeniden çiz
            UpdateZoomStatus();
            panel1.Invalidate();
        }

        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            // Fare orta tuşu veya sol tuş + Ctrl ile pan yapmaya başla
            if (e.Button == MouseButtons.Middle || (e.Button == MouseButtons.Left && ModifierKeys == Keys.Control))
            {
                isPanning = true;
                lastPanPoint = e.Location;
                panel1.Cursor = Cursors.Hand;
            }
        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.Controls["lblZoomStatus"] is Label lblZoom)
            {
                // lblZoom'u fare pozisyonuna taşır
                lblZoom.Location = new Point(e.X + 10, e.Y + 10); // Fare konumuna göre ayarla
            }

            if (isPanning)
            {
                // Pan işlemi sırasında kaydırma yap
                int deltaX = lastPanPoint.X - e.X;
                int deltaY = lastPanPoint.Y - e.Y;

                Point currentPos = new Point(-panel1.AutoScrollPosition.X, -panel1.AutoScrollPosition.Y);
                Point newPos = new Point(
                    currentPos.X + deltaX,
                    currentPos.Y + deltaY
                );

                panel1.AutoScrollPosition = newPos;
                lastPanPoint = e.Location;
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

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Transformasyon matrixi ayarla
            Matrix transformMatrix = new Matrix();

            // Panel'in scrollbar pozisyonunu al
            Point scrollPos = new Point(-panel1.AutoScrollPosition.X, -panel1.AutoScrollPosition.Y);

            // Önce scroll pozisyonunu uygula, sonra zoom faktörünü
            transformMatrix.Translate(-scrollPos.X, -scrollPos.Y);
            transformMatrix.Scale(zoomFactor, zoomFactor);

            g.Transform = transformMatrix;


            if (backgroundImage != null)
            {
                g.DrawImage(backgroundImage, new Rectangle(0, 0, this.Width, this.Height), 0, 0, originalImageSize.Width, originalImageSize.Height, GraphicsUnit.Pixel);
            }

            DrawConnections();
            foreach (var cihaz in cihazlar)
            {
                if (!clientleriGoster && !IsAllowedDeviceType(cihaz.GrupKod))
                    continue;

                string pngFilePath = GetPngFilePath(cihaz.GrupKod);
                if (!File.Exists(pngFilePath))
                {
                    Console.WriteLine($"PNG bulunamadı: {pngFilePath}");
                    continue;
                }

                using (Image cihazImage = Image.FromFile(pngFilePath))
                {
                    Bitmap coloredImage = new Bitmap(cihazImage.Width, cihazImage.Height);

                    Color tintColor;
                    if (!string.IsNullOrEmpty(cihaz.Durum) &&
                        cihaz.Durum.IndexOf("UP", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        tintColor = Color.FromArgb(170, Color.Green); // Yarı saydam yeşil
                    }
                    else if (!string.IsNullOrEmpty(cihaz.Durum) &&
                             cihaz.Durum.IndexOf("Down", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        tintColor = Color.FromArgb(170, Color.Red); // Yarı saydam kırmızı
                    }
                    else
                    {
                        tintColor = Color.FromArgb(170, Color.Gray); // Bilinmeyen için gri
                    }

                    using (Graphics imageGraphics = Graphics.FromImage(coloredImage))
                    {
                        imageGraphics.DrawImage(cihazImage, 0, 0, cihazImage.Width, cihazImage.Height);

                        using (Brush overlay = new SolidBrush(tintColor))
                        {
                            imageGraphics.FillRectangle(overlay, 0, 0, coloredImage.Width, coloredImage.Height);
                        }
                    }

                    float iconSize = 32;
                    RectangleF cihazRect = new RectangleF(
                        cihaz.X - iconSize / 2,
                        cihaz.Y - iconSize / 2,
                        iconSize,
                        iconSize);

                    Rectangle cihazRectInt = new Rectangle(
                        (int)cihazRect.X, (int)cihazRect.Y,
                        (int)cihazRect.Width, (int)cihazRect.Height);

                    g.DrawImage(coloredImage, cihazRectInt);

                    coloredImage.Dispose();
                }
            }
        }

        private bool IsAllowedDeviceType(string grupKod)
        {
            // İzin verilen grup kodlarını kontrol et
            string[] allowedDeviceTypes = {
                "KGS", "Yazıcı", "Kamera", "Data Switch", "Enerji panosu", "Bilgisayarlar", "PLC", "Kamera Switch"
            };
            return allowedDeviceTypes.Any(type => string.Equals(type, grupKod, StringComparison.OrdinalIgnoreCase));
        }

        private string GetPngFilePath(string grupKod)
        {
            string basePath = @"C:\Users\ebildik\Desktop\PNG"; // PNG dosyalarının bulunduğu dizin proje kodlarının içine koymayı unutma 
            string fileName = $"{grupKod}.png"; // Örnek: "Kamera.png" veya "EnerjiPano.png"
            return Path.Combine(basePath, fileName);
        }

        private void DrawConnections()
        {

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
        }

        private void Harita_MouseClick(object sender, MouseEventArgs e)
        {
            // Mouse'un belge üzerindeki gerçek pozisyonunu hesapla
            Point scrollPos = new Point(-panel1.AutoScrollPosition.X, -panel1.AutoScrollPosition.Y);
            float docX = (e.X + scrollPos.X) / zoomFactor;
            float docY = (e.Y + scrollPos.Y) / zoomFactor;

            Console.WriteLine($"Click at screen: {e.X},{e.Y}, doc: {docX},{docY}");

            CihazBilgi enYakinCihaz = null;
            double enKucukMesafe = double.MaxValue;

            foreach (var cihaz in cihazlar)
            {
                double dx = docX - cihaz.X;
                double dy = docY - cihaz.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                float clickRadius = pointRadius + 12;

                if (distance <= clickRadius && distance < enKucukMesafe)
                {
                    enKucukMesafe = distance;
                    enYakinCihaz = cihaz;
                }
            }

            if (enYakinCihaz != null)
            {
                Console.WriteLine($"Device found: {enYakinCihaz.Aciklama} (RecNo: {enYakinCihaz.RecNo})");
                GuncelCihazBilgisiGoster(enYakinCihaz.RecNo);
            }
            else
            {
                // Tıklanan nokta hakkında bilgi verirken KonumEkle formunu aç
                KonumEkle konumForm = new KonumEkle(docX, docY);
                konumForm.ShowDialog();  // ShowDialog() penceresini modal açar, kullanıcı etkileşimini bekler
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
            public string GrupKod { get; set; }
            public string EnerjiPanoNo { get; set; }
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

        private void DownCihazlar_CheckedChanged_1(object sender, EventArgs e)
        {
            // İşlem eklenecek
        }

        private void CizgiKaldirChckBox_CheckedChanged_1(object sender, EventArgs e)
        {
            cizgileriGoster = !CizgiKaldirChckBox.Checked;
            panel1.Invalidate();
        }

        private void Harita_Resize(object sender, EventArgs e)
        {
            UpdateScrollBars();
            panel1.Invalidate();
        }

        private void Harita_Load_1(object sender, EventArgs e)
        {

        }
    }
}