using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
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

        private bool cizgileriGoster = true;
        private bool enerjiPanolariniGoster = true;
        private bool clientleriGoster = true;
        //private bool downDurumuGoster = true;

        private float zoomFactor = 1.0f;
        private const float zoomIncrement = 0.1f;
        private const float minZoom = 0.5f;
        private const float maxZoom = 3.0f;
        private Image backgroundImage;
        private Size originalImageSize;


        private bool isPanning = false;
        private Point panStartMouse;
        private Point panStartScroll;
        private bool ctrlPressed = false;

        public Harita()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.panel1.AutoScroll = true;
            this.panel1.Paint += Harita_Paint;
            this.panel1.MouseClick += Harita_MouseClick;
            this.panel1.MouseUp += Panel1_MouseUp;
            this.panel1.MouseDown += Panel1_MouseDown;
            this.panel1.MouseMove += Panel1_MouseMove;
            this.Resize += Harita_Resize;
            this.panel1.MouseWheel += Panel1_MouseWheel;
            this.panel1.MouseEnter += (s, e) => panel1.Focus();
            this.panel1.TabStop = true;
            this.KeyPreview = true;
            this.KeyDown += Harita_KeyDown;
            this.KeyUp += Harita_KeyUp;

            string imagePath = @"C:\Users\ebildik\Desktop\Genel Layout.PNG";
            try
            {
                backgroundImage = Image.FromFile(imagePath);
                originalImageSize = backgroundImage.Size;
                panel1.AutoScrollMinSize = originalImageSize; // Başlangıç kaydırma alanı
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arka plan resmi yüklenirken hata oluştu: " + ex.Message,
                    "Resim Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            VeritabanindanCihazlariYukle();
            durumGuncellemeTimer = new Timer { Interval = 1000 };
            durumGuncellemeTimer.Tick += (s, e) => VeritabanindanCihazlariYukle();
            durumGuncellemeTimer.Start();
        }
        private void CihazaZoomYap(CihazBilgi cihaz, float hedefZoomFaktor)
        {
            if (cihaz == null) return;

            // Zoom faktörünü güncelle (sınırlar içinde)
            float yeniZoom = Math.Min(Math.Max(hedefZoomFaktor, minZoom), maxZoom);
            float eskiZoom = zoomFactor;
            zoomFactor = yeniZoom;

            // Cihazın belge koordinatları
            float cihazDocX = cihaz.X;
            float cihazDocY = cihaz.Y;

            // Yeni zoom ile cihazın ekran koordinatlarını hesapla
            int yeniCihazEkranX = (int)(cihazDocX * zoomFactor);
            int yeniCihazEkranY = (int)(cihazDocY * zoomFactor);

            // Panel merkezine göre scroll pozisyonunu hesapla
            int panelMerkezX = panel1.ClientSize.Width / 2;
            int panelMerkezY = panel1.ClientSize.Height / 2;

            int yeniScrollX = yeniCihazEkranX - panelMerkezX;
            int yeniScrollY = yeniCihazEkranY - panelMerkezY;

            // Kaydırma alanını güncelle
            int genislik = Math.Max((int)(originalImageSize.Width * zoomFactor), panel1.ClientSize.Width + 1);
            int yukseklik = Math.Max((int)(originalImageSize.Height * zoomFactor), panel1.ClientSize.Height + 1);
            panel1.AutoScrollMinSize = new Size(genislik, yukseklik);

            // Yeni scroll pozisyonunu uygula
            panel1.AutoScrollPosition = new Point(yeniScrollX, yeniScrollY);

            // Haritayı yeniden çiz
            panel1.Invalidate();
        }
        private void Harita_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            var transformMatrix = new Matrix();
            transformMatrix.Scale(zoomFactor, zoomFactor);
            transformMatrix.Translate(panel1.AutoScrollPosition.X / zoomFactor, panel1.AutoScrollPosition.Y / zoomFactor);
            g.Transform = transformMatrix;

            if (backgroundImage != null)
            {
                g.DrawImage(backgroundImage, 0, 0, this.Width, this.Height);
            }

            DrawConnections(g);

            foreach (var cihaz in cihazlar)
            {
                if (!clientleriGoster && !IsAllowedDeviceType(cihaz.GrupKod))
                    continue;

                string pngFilePath = GetPngFilePath(cihaz.GrupKod);
                if (!File.Exists(pngFilePath))
                    continue;

                using (Image cihazImage = Image.FromFile(pngFilePath))
                {
                    using (Bitmap coloredImage = new Bitmap(cihazImage.Width, cihazImage.Height))
                    using (Graphics imageGraphics = Graphics.FromImage(coloredImage))
                    {
                        imageGraphics.DrawImage(cihazImage, 0, 0, cihazImage.Width, cihazImage.Height);

                        Color tintColor;
                        if (!string.IsNullOrEmpty(cihaz.Durum) &&
                            cihaz.Durum.IndexOf("UP", StringComparison.OrdinalIgnoreCase) >= 0)
                            tintColor = Color.FromArgb(170, Color.Green);
                        else if (!string.IsNullOrEmpty(cihaz.Durum) &&
                                 cihaz.Durum.IndexOf("Down", StringComparison.OrdinalIgnoreCase) >= 0)
                            tintColor = Color.FromArgb(170, Color.Red);
                        else
                            tintColor = Color.FromArgb(170, Color.Gray);

                        using (Brush overlay = new SolidBrush(tintColor))
                            imageGraphics.FillRectangle(overlay, 0, 0, coloredImage.Width, coloredImage.Height);

                        float iconSize = 32;
                        RectangleF cihazRect = new RectangleF(
                            cihaz.X - iconSize / 2,
                            cihaz.Y - iconSize / 2,
                            iconSize, iconSize);

                        g.DrawImage(coloredImage,
                            new Rectangle((int)cihazRect.X, (int)cihazRect.Y, (int)cihazRect.Width, (int)cihazRect.Height));
                    }
                }
            }
        }

        // --- Cihazı ekrana getir ---
        private void CihaziGorunecekSekildeKaydir(CihazBilgi cihaz)
        {
            if (cihaz == null) return;
            // Cihazın ekrandaki konumunu bul
            int hedefX = (int)(cihaz.X * zoomFactor);
            int hedefY = (int)(cihaz.Y * zoomFactor);

            // Panelde görünür alanın üst sol köşesi
            int scrollX = -panel1.AutoScrollPosition.X;
            int scrollY = -panel1.AutoScrollPosition.Y;

            // Panelin görünür boyutu
            int panelGenislik = panel1.ClientSize.Width;
            int panelYukseklik = panel1.ClientSize.Height;
            float iconSize = 32 * zoomFactor;

            // Şekil ekrandan çıkmış mı?
            bool disarda = hedefX + iconSize < scrollX || hedefX > scrollX + panelGenislik ||
                           hedefY + iconSize < scrollY || hedefY > scrollY + panelYukseklik;

            if (disarda)
            {
                // Ortalamak için
                int yeniScrollX = (int)(hedefX - (panelGenislik - iconSize) / 2);
                int yeniScrollY = (int)(hedefY - (panelYukseklik - iconSize) / 2);

                if (yeniScrollX < 0) yeniScrollX = 0;
                if (yeniScrollY < 0) yeniScrollY = 0;

                panel1.AutoScrollPosition = new Point(yeniScrollX, yeniScrollY);
                panel1.Invalidate();
            }
        }

        private void Harita_MouseClick(object sender, MouseEventArgs e)
        {
            Point scrollPos = new Point(-panel1.AutoScrollPosition.X, -panel1.AutoScrollPosition.Y);
            float docX = (e.X + scrollPos.X) / zoomFactor;
            float docY = (e.Y + scrollPos.Y) / zoomFactor;

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
                if (e.Button == MouseButtons.Left)
                {
                    // Normal tıklama - sadece bilgileri göster
                    GuncelCihazBilgisiGoster(enYakinCihaz.RecNo);
                }
                else if (e.Button == MouseButtons.Right)
                {
                    // Sağ tıklama - cihaza zoom yap (örneğin 1.5x zoom ile)
                    CihazaZoomYap(enYakinCihaz, 1.5f);
                }
            }
            else if (e.Button == MouseButtons.Left)
            {
                KonumEkle konumForm = new KonumEkle(docX, docY);
                konumForm.ShowDialog();
            }
        }

        private bool IsAllowedDeviceType(string grupKod)
        {
            string[] allowedDeviceTypes = {
                "KGS", "Yazıcı", "Kamera", "Data Switch", "Enerji panosu", "Bilgisayarlar", "PLC", "Kamera Switch"
            };
            foreach (var type in allowedDeviceTypes)
            {
                if (string.Equals(type, grupKod, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
        private string GetPngFilePath(string grupKod)//resim yolu
        {
            string basePath = @"C:\Users\ebildik\Desktop\PNG";
            string fileName = $"{grupKod}.png";
            return Path.Combine(basePath, fileName);
        }
        private void DrawConnections(Graphics g)
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
                                g.DrawLine(redPen, cihaz.X, cihaz.Y, enerjiPanosu.X, enerjiPanosu.Y);
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
                            Color renk = Color.BlueViolet;
                            if (cihaz.GrupKod == "Kamera")
                                renk = Color.Chartreuse;
                            else if (cihaz.GrupKod == "Yazıcı")
                                renk = Color.DarkOrange;
                            else if (cihaz.GrupKod == "KGS")
                                renk = Color.Purple;
                            using (Pen kalem = new Pen(renk, 3))
                                g.DrawLine(kalem, switchCihaz.X, switchCihaz.Y, cihaz.X, cihaz.Y);
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
                                g.DrawLine(kahverengiKalem, cihaz.X, cihaz.Y, bagliSwitch.X, bagliSwitch.Y);
                        }
                    }
                }
            }
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
            public int SwitchRecNo { get; set; }
            public string GrupKod { get; set; }
            public string EnerjiPanoNo { get; set; }
            public Color PointColor { get; set; } = Color.Black;
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

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                yeniCihazlar.Add(new CihazBilgi
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
                                });
                            }
                        }
                    }
                }

                // Optimize et: Sadece değişiklik olan cihazları kontrol et
                bool degisiklikVar = false;

                foreach (var yeniCihaz in yeniCihazlar)
                {
                    var mevcutCihaz = cihazlar.FirstOrDefault(c => c.RecNo == yeniCihaz.RecNo);
                    if (mevcutCihaz == null)
                    {
                        // Yeni cihaz eklendi
                        cihazlar.Add(yeniCihaz);
                        degisiklikVar = true;
                    }
                    else
                    {
                        // Mevcut cihazın durumu veya diğer bilgileri değişti mi?
                        if (mevcutCihaz.X != yeniCihaz.X || mevcutCihaz.Y != yeniCihaz.Y ||
                            mevcutCihaz.Durum != yeniCihaz.Durum || mevcutCihaz.EnerjiPanoNo != yeniCihaz.EnerjiPanoNo)
                        {
                            mevcutCihaz.X = yeniCihaz.X;
                            mevcutCihaz.Y = yeniCihaz.Y;
                            mevcutCihaz.Durum = yeniCihaz.Durum;
                            mevcutCihaz.EnerjiPanoNo = yeniCihaz.EnerjiPanoNo;
                            mevcutCihaz.PointColor = yeniCihaz.Durum.Equals("UP", StringComparison.OrdinalIgnoreCase) ? Color.Green : Color.Red;
                            degisiklikVar = true;
                        }
                    }
                }

                // Çizgi gösterme durumu kontrolü
                if (cizgileriGoster)
                {
                    // Eğer çizgiler gösteriliyorsa haritayı yeniden çiz
                    if (degisiklikVar)
                    {
                        panel1.Invalidate();
                    }
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
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RecNo", cihazRecNo);
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                StringBuilder sb = new StringBuilder();
                                sb.AppendLine($"IP: {reader["IPNo"] ?? "N/A"}");
                                sb.AppendLine($"Cihaz: {reader["Aciklama"] ?? "N/A"}");
                                sb.AppendLine($"Durum: {reader["Durum"]?.ToString() ?? "N/A"}");
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
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cihaz bilgisi alınırken hata oluştu: " + ex.Message,
                    "Veritabanı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void Harita_Load_1(object sender, EventArgs e)
        {

        }
        private void CizgiKaldirChckBox_CheckedChanged_1(object sender, EventArgs e)
        {
            cizgileriGoster = !CizgiKaldirChckBox.Checked;
            panel1.Invalidate();
        }
        private void DownCihazlar_CheckedChanged_1(object sender, EventArgs e)
        {
            if (DownCihazlar.Checked) // Eğer checkbox işaretliyse
            {
                var downCihazlar = cihazlar
    .Where(c => !string.IsNullOrEmpty(c.Durum) && c.Durum.IndexOf("Down", StringComparison.OrdinalIgnoreCase) >= 0)
    .ToList();

                // Sadece "Down" cihazları göstermek için listeyi güncelle
                cihazlar = downCihazlar;

                // Haritayı yeniden çiz
                panel1.Invalidate();
            }
            else // Checkbox işaretli değilse tüm cihazları tekrar yükle
            {
                VeritabanindanCihazlariYukle();
                panel1.Invalidate();
            }
        }
        private void Harita_Resize(object sender, EventArgs e)
        {
            if (backgroundImage != null)
            {
                // Dinamik kaydırma çubuğu alanı
                int genislik = Math.Max((int)(originalImageSize.Width * zoomFactor), panel1.ClientSize.Width + 1);
                int yukseklik = Math.Max((int)(originalImageSize.Height * zoomFactor), panel1.ClientSize.Height + 1);
                panel1.AutoScrollMinSize = new Size(genislik, yukseklik);
            }

            panel1.Invalidate();
        }

        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (ctrlPressed && e.Button == MouseButtons.Left)
            {
                isPanning = true;
                panStartMouse = e.Location;
                panStartScroll = new Point(-panel1.AutoScrollPosition.X, -panel1.AutoScrollPosition.Y);
                panel1.Cursor = Cursors.Hand; // El aracı şeklinde bir imleç gösterebilirsiniz
            }
        }

        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                int dx = e.X - panStartMouse.X;
                int dy = e.Y - panStartMouse.Y;

                // Yeni kaydırma pozisyonlarını hesapla
                int newScrollX = panStartScroll.X - dx;
                int newScrollY = panStartScroll.Y - dy;

                // Kaydırma pozisyonlarını uygula
                panel1.AutoScrollPosition = new Point(newScrollX, newScrollY);
            }
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                isPanning = false;
                panel1.Cursor = Cursors.Default; // İmleci normale döndür
            }
        }

        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldZoom = zoomFactor;

            // Zoom miktarını sınırla
            if (e.Delta > 0 && zoomFactor < maxZoom)
                zoomFactor += zoomIncrement;
            else if (e.Delta < 0 && zoomFactor > minZoom)
                zoomFactor -= zoomIncrement;
            else
                return;

            // Fare konumunu belge koordinatlarına dönüştür (zoom öncesi)
            Point scrollPos = new Point(-panel1.AutoScrollPosition.X, -panel1.AutoScrollPosition.Y);
            float mouseDocX = (e.X + scrollPos.X) / oldZoom;
            float mouseDocY = (e.Y + scrollPos.Y) / oldZoom;

            // Yeni zoom ile fare pozisyonunun ekran koordinatlarını hesapla
            int newMouseScreenX = (int)(mouseDocX * zoomFactor);
            int newMouseScreenY = (int)(mouseDocY * zoomFactor);

            // Yeni scroll pozisyonu hesapla (fare pozisyonunu sabit tutacak şekilde)
            int newScrollX = newMouseScreenX - e.X;
            int newScrollY = newMouseScreenY - e.Y;

            int genislik = (int)(originalImageSize.Width * 4); // Fotoğrafın 3 katı genişlik
            int yukseklik = (int)(originalImageSize.Height * 4); // Fotoğrafın 3 katı yükseklik
            panel1.AutoScrollMinSize = new Size(genislik, yukseklik);
            // Kaydırma alanını güncelle, kaydırma alanı 
            // int genislik = Math.Max((int)(originalImageSize.Width * zoomFactor), panel1.ClientSize.Width + 1);
            //int yukseklik = Math.Max((int)(originalImageSize.Height * zoomFactor), panel1.ClientSize.Height + 1);
            // panel1.AutoScrollMinSize = new Size(genislik, yukseklik);

            // Yeni scroll pozisyonunu uygula
            panel1.AutoScrollPosition = new Point(newScrollX, newScrollY);

            // Haritayı yeniden çiz
            panel1.Invalidate();
        }

        private void Harita_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                ctrlPressed = true;
        }

        private void Harita_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.ControlKey)
                ctrlPressed = false;
        }
    }
}