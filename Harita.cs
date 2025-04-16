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
        private Size virtualSize;

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

                // İlk açılışta virtual size'ı ayarla
                UpdateVirtualSize();
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


            this.BackgroundImageLayout = ImageLayout.Zoom;
        }


        private void UpdateVirtualSize()
        {
            if (backgroundImage != null)
            {

                int virtualWidth = (int)(this.Width * zoomFactor);
                int virtualHeight = (int)(this.Height * zoomFactor);
                panel1.AutoScrollMinSize = new Size(virtualWidth, virtualHeight);
                if (previousZoomFactor > 0 && previousZoomFactor != zoomFactor)
                {
                    float zoomRatio = zoomFactor / previousZoomFactor;

                    Point currentPos = panel1.AutoScrollPosition;
                    int newX = (int)((-currentPos.X) * zoomRatio);
                    int newY = (int)((-currentPos.Y) * zoomRatio);

                    panel1.AutoScrollPosition = new Point(newX, newY);
                }

                virtualSize = new Size(virtualWidth, virtualHeight);
            }
        }

        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            // Fare tekerleği ile zoom yapma
            float oldZoom = zoomFactor;

            // Mevcut scroll konumunu al ve pozitif değerlere çevir
            Point scrollPos = panel1.AutoScrollPosition;
            scrollPos.X = -scrollPos.X; // Negatif değerleri pozitife çevir
            scrollPos.Y = -scrollPos.Y;

            // Fare imlecinin içerik (belge) üzerindeki gerçek pozisyonunu hesapla
            PointF docPoint = new PointF(
                (e.Location.X + scrollPos.X) / oldZoom,
                (e.Location.Y + scrollPos.Y) / oldZoom
            );

            // Zoom yönü
            if (e.Delta > 0)
            {
                if (zoomFactor < maxZoom)
                {
                    previousZoomFactor = zoomFactor;
                    zoomFactor += zoomIncrement;
                }
            }
            else
            {
                if (zoomFactor > minZoom)
                {
                    previousZoomFactor = zoomFactor;
                    zoomFactor -= zoomIncrement;
                }
            }

            // Zoom değiştiyse, farenin aynı pozisyonda kalması için scroll pozisyonunu ayarla
            if (oldZoom != zoomFactor)
            {
                // Yeni zoom faktörü için sanal boyutu ayarla
                UpdateVirtualSize();

                // Zoom sonrası fare imlecinin belge üzerindeki yeni pozisyonunu hesapla
                PointF newDocPoint = new PointF(
                    docPoint.X * zoomFactor,
                    docPoint.Y * zoomFactor
                );

                // Yeni scroll pozisyonunu hesapla
                Point newScrollPos = new Point(
                    (int)(newDocPoint.X - e.Location.X),
                    (int)(newDocPoint.Y - e.Location.Y)
                );

                // Yeni scroll pozisyonunu uygula
                panel1.AutoScrollPosition = newScrollPos;

                // Zoom durumunu güncelle ve yeniden çizimi tetikle
                UpdateZoomStatus();
                panel1.Invalidate();
            }
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
            if (isPanning)
            {
                // Ne kadar kaydırılacağını hesapla
                int deltaX = lastPanPoint.X - e.X;
                int deltaY = lastPanPoint.Y - e.Y;

                // Mevcut scroll pozisyonunu al (AutoScrollPosition değerleri negatiftir)
                Point currentPos = panel1.AutoScrollPosition;
                currentPos.X = -currentPos.X; // Pozitife çevir
                currentPos.Y = -currentPos.Y; // Pozitife çevir

                // Yeni scroll pozisyonunu hesapla
                Point newPos = new Point(
                    currentPos.X + deltaX,
                    currentPos.Y + deltaY
                );

                // Yeni scroll pozisyonunu ayarla
                panel1.AutoScrollPosition = newPos;

                // Son pan noktasını güncelle
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

            Matrix transformMatrix = new Matrix();
            Point scrollPos = panel1.AutoScrollPosition;
            transformMatrix.Translate(-scrollPos.X, -scrollPos.Y);
            transformMatrix.Scale(zoomFactor, zoomFactor);
            g.Transform = transformMatrix;

            if (backgroundImage != null)
            {
                g.DrawImage(backgroundImage, 0, 0, this.Width, this.Height);
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

                        // PNG'nin üzerine yarı saydam renk maskesi uygula
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
            string basePath = @"C:\Users\ebildik\Desktop\PNG"; // PNG dosyalarının bulunduğu dizin
            string fileName = $"{grupKod}.png"; // Örnek: "Kamera.png" veya "EnerjiPano.png"
            return Path.Combine(basePath, fileName);
        }

        private void DrawConnections()
        {
            // Çizgi çizme mantığını buraya taşıdık (mevcut kodda varsa)
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
            // Convert screen coordinates to document coordinates
            Point scrollPos = panel1.AutoScrollPosition;
            // Note: AutoScrollPosition returns negative values
            float docX = (e.X - scrollPos.X) / zoomFactor;
            float docY = (e.Y - scrollPos.Y) / zoomFactor;

            // For debugging
            Console.WriteLine($"Click at screen: {e.X},{e.Y}, doc: {docX},{docY}");

            CihazBilgi enYakinCihaz = null;
            double enKucukMesafe = double.MaxValue;

            foreach (var cihaz in cihazlar)
            {
                // Calculate distance to device center
                double dx = docX - cihaz.X;
                double dy = docY - cihaz.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);

                // Increase detection radius to make clicking easier
                float clickRadius = pointRadius + 12; // Increased from 5 to 12

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
                Console.WriteLine("No device found near click point");
                // Optional: Display coordinates for debugging
                MessageBox.Show($"Tıklanan Nokta:\nX: {docX}, Y: {docY}", "Lokasyon");
            }
        }

        // Arka plan resmi ayarlamak için metod
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

                    // Resim değiştiğinde virtual size'ı güncelle
                    UpdateVirtualSize();

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
            //panel1.Controls.Clear();
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
