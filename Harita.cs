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
        private List<CihazBilgileri> tumCihazlar = new List<CihazBilgileri>();
        private List<CihazBilgileri> cihazlar = new List<CihazBilgileri>();
        private int pointRadius = 8;
        private string connectionString = "Data Source=ES-BT14\\SQLEXPRESS;Initial Catalog=CihazTakip;Integrated Security=True";
        private Timer durumGuncellemeTimer;
        private bool cizgileriGoster = true;
        private bool tumCizgileriGoster = true;
        private bool tumCihazlarıGoster = true;
        private bool switchKameraCizgileriGoster = false;
        private bool switchYaziciCizgileriGoster = false;
        private bool switchKGSCizgileriGoster = false;
        private bool enerjiPanolariniGoster = true;
        private bool clientleriGoster = true;
        private float zoomFactor = 1.0f;
        private const float zoomIncrement = 0.1f;
        private const float minZoom = 0.5f;
        private const float maxZoom = 4.0f;
        private Image backgroundImage;
        private Size originalImageSize;
        private bool isPanning = false;
        private Point panStartMouse;
        private Point panStartScroll;
        private bool ctrlPressed = false;
        private Panel panelBilgi;

        //menü değişkenleri
        private ContextMenuStrip haritaMenu;
        private ToolStripMenuItem menuZoom;
        private ToolStripMenuItem menuKGS;
        private ToolStripMenuItem menuYazici;
        private ToolStripMenuItem menuEnerjiPanosu;
        private ToolStripMenuItem menuCizgiGoster;
        private ToolStripMenuItem menuTumCizgiler;
        private ToolStripMenuItem menuSwitchKamera;
        private ToolStripMenuItem menuSwitchYazici;
        private ToolStripMenuItem menuSwitchKGS;
        private ToolStripMenuItem menuZoomFormat;
        private ToolStripMenuItem menuZoom120;
        private ToolStripMenuItem menuZoom140;
        private ToolStripMenuItem menuZoom160;
        private ToolStripMenuItem menuZoom180;
        private ToolStripMenuItem menuZoom200;
        private ToolStripMenuItem menuZoom300;
        private ToolStripMenuItem menuZoom400;
        private ToolStripMenuItem menuSwitchGöster;
        private ToolStripMenuItem menuPLC;
        private ToolStripMenuItem menuBilgisayar;
        private ToolStripMenuItem menuDownCihazlar;
        private ToolStripMenuItem menuTumCihazlar;

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
            CizgiPaneliniYukle();

            // Menü Kurulumu
            haritaMenu = new ContextMenuStrip();
            menuZoom = new ToolStripMenuItem("Yakınlaştır");
            menuZoomFormat = new ToolStripMenuItem("%100", null, (s, e) => SetZoomWithCheck(menuZoomFormat, 1f))
            {
                Checked = true // Varsayılan olarak %100 yakınlaştırma seçili
            };
            menuZoom120 = new ToolStripMenuItem("%120", null, (s, e) => SetZoomWithCheck(menuZoom120, 1.2f));
            menuZoom140 = new ToolStripMenuItem("%140", null, (s, e) => SetZoomWithCheck(menuZoom140, 1.4f));
            menuZoom160 = new ToolStripMenuItem("%160", null, (s, e) => SetZoomWithCheck(menuZoom160, 1.6f));
            menuZoom180 = new ToolStripMenuItem("%180", null, (s, e) => SetZoomWithCheck(menuZoom180, 1.8f));
            menuZoom200 = new ToolStripMenuItem("%200", null, (s, e) => SetZoomWithCheck(menuZoom200, 2.0f));
            menuZoom300 = new ToolStripMenuItem("%300", null, (s, e) => SetZoomWithCheck(menuZoom300, 3.0f));
            menuZoom400 = new ToolStripMenuItem("%400", null, (s, e) => SetZoomWithCheck(menuZoom400, 4.0f));
            // Menü öğelerini bir araya getiriyoruz
            menuZoom.DropDownItems.AddRange(new ToolStripItem[] {
                menuZoomFormat, menuZoom120, menuZoom140, menuZoom160, menuZoom180, menuZoom200, menuZoom300, menuZoom400
            });

            // Tıklanan menüyü işaretlemek ve zoom oranını ayarlamak için metot
            void SetZoomWithCheck(ToolStripMenuItem selectedMenu, float zoomFactor)
            {
                // Tüm menü öğelerinin tiklerini kaldır
                foreach (ToolStripMenuItem item in menuZoom.DropDownItems)
                {
                    item.Checked = false;
                }

                // Tıklanan menüye tik at
                selectedMenu.Checked = true;

                // Zoom değerini ayarla
                SetZoom(zoomFactor);
            }

            // Çizgi Gösterme Menüsü ve Alt Menüleri
            menuCizgiGoster = new ToolStripMenuItem("Çizgileri Göster");

            // Alt menü öğelerini oluşturun
            menuTumCizgiler = new ToolStripMenuItem("Bütün Çizgileri Göster", null, MenuTumCizgiler_Click)
            {
                Checked = tumCizgileriGoster,
                CheckOnClick = true
            };
            menuTumCihazlar = new ToolStripMenuItem("Bütün Cihazları Göster", null, MenuTumCihazlar_Click)
            {
                Checked = tumCizgileriGoster,
                CheckOnClick = true
            };
            menuSwitchKamera = new ToolStripMenuItem("Switch-Kamera Çizgilerini Göster", null, MenuSwitchKamera_Click)
            {
                Checked = switchKameraCizgileriGoster,
                CheckOnClick = true
            };

            menuSwitchYazici = new ToolStripMenuItem("Switch-Yazıcı Çizgilerini Göster", null, MenuSwitchYazici_Click)
            {
                Checked = switchYaziciCizgileriGoster,
                CheckOnClick = true
            };

            menuSwitchKGS = new ToolStripMenuItem("Switch-KGS Çizgilerini Göster", null, MenuSwitchKGS_Click)
            {
                Checked = switchKGSCizgileriGoster,
                CheckOnClick = true
            };

            // Alt menüleri ana çizgi menüsüne ekleyin
            menuCizgiGoster.DropDownItems.AddRange(new ToolStripItem[]
            {
                menuTumCizgiler,
                menuSwitchKamera,
                menuSwitchYazici,
                menuSwitchKGS
            });

            // Ana Menü: Cihazları Göster
            var menuCihazlariGoster = new ToolStripMenuItem("Cihazları Göster");

            // Alt Menüler
            menuKGS = new ToolStripMenuItem("KGS Cihazlarını Göster", null, MenuKGS_Click)
            {
                CheckOnClick = true
            };

            menuYazici = new ToolStripMenuItem("Yazıcıları Göster", null, MenuYazici_Click)
            {
                CheckOnClick = true
            };

            menuEnerjiPanosu = new ToolStripMenuItem("Enerji Panolarını Göster", null, menuEnerjiPanosu_Click)
            {
                CheckOnClick = true
            };

            menuSwitchGöster = new ToolStripMenuItem("Data Switchleri Göster", null, menuSwitchGöster_Click)
            {
                CheckOnClick = true
            };

            menuPLC = new ToolStripMenuItem("PLC'leri Göster", null, menuPLC_Click)
            {
                CheckOnClick = true
            };

            menuBilgisayar = new ToolStripMenuItem("Bilgisayarları Göster", null, menuBilgisayar_Click)
            {
                CheckOnClick = true
            };

            menuDownCihazlar = new ToolStripMenuItem("Down Cihazları Göster", null, menuDownCihazlar_Click)
            {
                CheckOnClick = true
            };

            // Alt Menüler Ana Menüye Eklendi
            menuCihazlariGoster.DropDownItems.AddRange(new ToolStripItem[]
            {
                menuTumCihazlar,
                menuKGS,
                menuYazici,
                menuEnerjiPanosu,
                menuSwitchGöster,
                menuPLC,
                menuBilgisayar,
                menuDownCihazlar
            });

            // Harita Menüsüne Ana Menüyü Ekleyin
            haritaMenu.Items.AddRange(new ToolStripItem[]
            {
                menuZoom,
                menuCizgiGoster,
                menuCihazlariGoster
            });

            string imagePath = @"C:\Users\ebildik\Desktop\Genel Layout.PNG";
            try
            {
                backgroundImage = Image.FromFile(imagePath);
                originalImageSize = backgroundImage.Size;
                panel1.AutoScrollMinSize = originalImageSize;
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
        private void MenuTumCihazlar_Click(object sender, EventArgs e)
        {
            tumCihazlarıGoster = ((ToolStripMenuItem)sender).Checked;

            if (tumCihazlarıGoster)
            {
                cihazlar = new List<CihazBilgileri>(tumCihazlar); // Tüm cihazları göster
            }
            else
            {
                cihazlar = new List<CihazBilgileri>(); // Hiçbir cihazı gösterme
            }

            panel1.Invalidate();
        }

        // Yeni eklenen metotlar - Çizgi gösterme ayarları için
        private void MenuTumCizgiler_Click(object sender, EventArgs e)
        {
            tumCizgileriGoster = ((ToolStripMenuItem)sender).Checked;
            cizgileriGoster = tumCizgileriGoster;

            if (tumCizgileriGoster)
            {
                // Tüm çizgiler gösterilecekse diğer çizgi seçeneklerini devre dışı bırak
                switchKameraCizgileriGoster = false;
                switchYaziciCizgileriGoster = false;
                switchKGSCizgileriGoster = false;

                // Diğer menülerin işaretlerini kaldır
                menuSwitchKamera.Checked = false;
                menuSwitchYazici.Checked = false;
                menuSwitchKGS.Checked = false;
            }
            panel1.Invalidate(); // Panel'i yeniden çiz
        }

        private void MenuSwitchKamera_Click(object sender, EventArgs e)
        {
            switchKameraCizgileriGoster = ((ToolStripMenuItem)sender).Checked;

            if (switchKameraCizgileriGoster)
            {
                // Özel bir çizgi seçilirse tüm çizgileri devre dışı bırak
                tumCizgileriGoster = false;
                menuTumCizgiler.Checked = false;
            }

            // Herhangi bir çizgi seçiliyse, cizgileriGoster'i aktif et, hiçbiri seçili değilse kapat
            cizgileriGoster = switchKameraCizgileriGoster || switchYaziciCizgileriGoster ||
                              switchKGSCizgileriGoster || tumCizgileriGoster;

            panel1.Invalidate();
        }

        private void MenuSwitchYazici_Click(object sender, EventArgs e)
        {
            switchYaziciCizgileriGoster = ((ToolStripMenuItem)sender).Checked;

            if (switchYaziciCizgileriGoster)
            {
                // Özel bir çizgi seçilirse tüm çizgileri devre dışı bırak
                tumCizgileriGoster = false;
                menuTumCizgiler.Checked = false;
            }

            // Herhangi bir çizgi seçiliyse, cizgileriGoster'i aktif et, hiçbiri seçili değilse kapat
            cizgileriGoster = switchKameraCizgileriGoster || switchYaziciCizgileriGoster ||
                              switchKGSCizgileriGoster || tumCizgileriGoster;

            panel1.Invalidate();
        }

        private void MenuSwitchKGS_Click(object sender, EventArgs e)
        {
            switchKGSCizgileriGoster = ((ToolStripMenuItem)sender).Checked;

            if (switchKGSCizgileriGoster)
            {
                // Özel bir çizgi seçilirse tüm çizgileri devre dışı bırak
                tumCizgileriGoster = false;
                menuTumCizgiler.Checked = false;
            }

            // Herhangi bir çizgi seçiliyse, cizgileriGoster'i aktif et, hiçbiri seçili değilse kapat
            cizgileriGoster = switchKameraCizgileriGoster || switchYaziciCizgileriGoster ||
                              switchKGSCizgileriGoster || tumCizgileriGoster;

            panel1.Invalidate();
        }

        private void menuDownCihazlar_Click(object sender, EventArgs e)
        {
            if (menuDownCihazlar.Checked)
            {
                cihazlar = tumCihazlar
                    .Where(c => !string.IsNullOrEmpty(c.Durum) &&
                                c.Durum.IndexOf("Down", StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();
                panel1.Invalidate();
            }
            else
            {
                cihazlar = new List<CihazBilgileri>(tumCihazlar);
            }
            panel1.Invalidate();
        }

        private void menuPLC_Click(object sender, EventArgs e)
        {
            if (menuPLC.Checked)
            {
                cihazlar = tumCihazlar.Where(c => c.GrupKod.Equals("PLC", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                cihazlar = new List<CihazBilgileri>(tumCihazlar);
            }
            panel1.Invalidate();
        }

        private void Panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                // Menü gösterilmeden önce çizgi seçeneklerinin durumlarını ayarla
                menuTumCizgiler.Checked = tumCizgileriGoster;
                menuSwitchKamera.Checked = switchKameraCizgileriGoster;
                menuSwitchYazici.Checked = switchYaziciCizgileriGoster;
                menuSwitchKGS.Checked = switchKGSCizgileriGoster;
                haritaMenu.Show(panel1, e.Location);
            }

            if (isPanning)
            {
                isPanning = false;
                panel1.Cursor = Cursors.Default;
            }
        }

        private void menuSwitchGöster_Click(object sender, EventArgs e)
        {
            if (menuSwitchGöster.Checked)
            {
                cihazlar = tumCihazlar.Where(c => c.GrupKod.Equals("Data Switch", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                cihazlar = new List<CihazBilgileri>(tumCihazlar);
            }
            panel1.Invalidate();
        }

        private void menuBilgisayar_Click(object sender, EventArgs e)
        {
            if (menuBilgisayar.Checked)
            {
                cihazlar = tumCihazlar.Where(c => c.GrupKod.Equals("Bilgisayarlar", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                cihazlar = new List<CihazBilgileri>(tumCihazlar);
            }
            panel1.Invalidate();
        }

        private void menuEnerjiPanosu_Click(object sender, EventArgs e)
        {
            if (menuEnerjiPanosu.Checked)
            {
                cihazlar = tumCihazlar.Where(c => c.GrupKod.Equals("Enerji panosu", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                cihazlar = new List<CihazBilgileri>(tumCihazlar);
            }
            panel1.Invalidate();
        }
        private void MenuKGS_Click(object sender, EventArgs e)
        {
            if (menuKGS.Checked)
            {
                menuYazici.Checked = false;
                cihazlar = tumCihazlar.Where(c => c.GrupKod.Equals("KGS", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                cihazlar = new List<CihazBilgileri>(tumCihazlar);
            }
            panel1.Invalidate();
        }

        private void MenuYazici_Click(object sender, EventArgs e)
        {
            if (menuYazici.Checked)
            {
                menuKGS.Checked = false;
                cihazlar = tumCihazlar.Where(c => c.GrupKod.Equals("Yazıcı", StringComparison.OrdinalIgnoreCase)).ToList();
            }
            else
            {
                cihazlar = new List<CihazBilgileri>(tumCihazlar);
            }
            panel1.Invalidate();
        }

        private void SetZoom(float factor)
        {
            zoomFactor = Math.Min(Math.Max(factor, minZoom), maxZoom);

            int genislik = Math.Max((int)(originalImageSize.Width * zoomFactor), panel1.ClientSize.Width + 1);
            int yukseklik = Math.Max((int)(originalImageSize.Height * zoomFactor), panel1.ClientSize.Height + 1);
            panel1.AutoScrollMinSize = new Size(genislik, yukseklik);

            panel1.AutoScrollPosition = new Point(MousePosition.X, MousePosition.Y); // mouse'un x ve y konumumuna göre yapacak
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

            CizgileriCiz(g);

            foreach (var cihaz in cihazlar)
            {
                if (!clientleriGoster && !Cihazlar(cihaz.GrupKod))
                    continue;

                string pngFilePath = ArkaPlanResminiAl(cihaz.GrupKod);
                if (!File.Exists(pngFilePath))
                    continue;

                using (Image cihazImage = Image.FromFile(pngFilePath))
                {
                    using (Bitmap coloredImage = new Bitmap(cihazImage.Width, cihazImage.Height))
                    using (Graphics imageGraphics = Graphics.FromImage(coloredImage))
                    {
                        imageGraphics.DrawImage(cihazImage, 0, 0, cihazImage.Width, cihazImage.Height);

                        Color tintColor;
                        if (cihaz.GrupKod == "Enerji panosu")
                        {
                            tintColor = Color.FromArgb(170, Color.Orange);
                        }
                        else if (!string.IsNullOrEmpty(cihaz.Durum) &&
                            cihaz.Durum.IndexOf("UP", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            tintColor = Color.FromArgb(170, Color.Green);
                        }
                        else if (!string.IsNullOrEmpty(cihaz.Durum) &&
                                 cihaz.Durum.IndexOf("Down", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            tintColor = Color.FromArgb(170, Color.Red);
                        }
                        else
                        {
                            tintColor = Color.FromArgb(170, Color.BlueViolet);
                        }

                        using (Brush overlay = new SolidBrush(tintColor))
                            imageGraphics.FillRectangle(overlay, 0, 0, coloredImage.Width, coloredImage.Height);

                        float iconBoyutu = 8;
                        RectangleF cihazRect = new RectangleF(
                            cihaz.X - iconBoyutu / 2,
                            cihaz.Y - iconBoyutu / 2,
                            iconBoyutu, iconBoyutu);

                        g.DrawImage(coloredImage,
                            new Rectangle((int)cihazRect.X, (int)cihazRect.Y, (int)cihazRect.Width, (int)cihazRect.Height));
                    }
                }
            }
        }

        private void Harita_MouseClick(object sender, MouseEventArgs e)
        {
            Point scrollPos = new Point(-panel1.AutoScrollPosition.X, -panel1.AutoScrollPosition.Y);
            float docX = (e.X + scrollPos.X) / zoomFactor;
            float docY = (e.Y + scrollPos.Y) / zoomFactor;

            CihazBilgileri enYakinCihaz = null;
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
                    GuncelCihazBilgisiGoster(enYakinCihaz.RecNo);
                }
                // Sağ tıkta menü açılıyor
            }
            else if (e.Button == MouseButtons.Left)
            {
                frmKonumEkle konumForm = new frmKonumEkle(docX, docY);
                konumForm.ShowDialog();
            }
        }

        private bool Cihazlar(string grupKod)//cihazları buraya gireceğiz
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

        private string ArkaPlanResminiAl(string grupKod)
        {
            string basePath = @"C:\Users\ebildik\Desktop\PNG";
            string fileName = $"{grupKod}.png";
            return Path.Combine(basePath, fileName);
        }

        private void CizgileriCiz(Graphics g)
        {
            float baseSwitchLineWidth = 2.0f;
            float baseRedLineWidth = 2.0f;
            float baseBrownLineWidth = 2.0f;
            float switchLineWidth = baseSwitchLineWidth / zoomFactor;
            float redLineWidth = baseRedLineWidth / zoomFactor;
            float brownLineWidth = baseBrownLineWidth / zoomFactor;

            if (enerjiPanolariniGoster)
            {
                foreach (var cihaz in cihazlar)
                {
                    if (!string.IsNullOrEmpty(cihaz.EnerjiPanoNo))
                    {
                        var enerjiPanosu = cihazlar.FirstOrDefault(p => p.Aciklama == cihaz.EnerjiPanoNo && p.GrupKod == "Enerji panosu");
                        if (enerjiPanosu != null)
                        {
                            using (Pen redPen = new Pen(Color.Red, redLineWidth))
                                g.DrawLine(redPen, cihaz.X, cihaz.Y, enerjiPanosu.X, enerjiPanosu.Y);
                        }
                    }
                }
            }

            if (cizgileriGoster) // ÇİZGİLER BURADA ÇİZİLİYOR
            {
                foreach (var cihaz in cihazlar)
                {
                    if (cihaz.SwitchRecNo != 0 && cihaz.GrupKod != "Enerji panosu")
                    {
                        var switchCihaz = cihazlar.FirstOrDefault(s => s.RecNo == cihaz.SwitchRecNo && (s.GrupKod == "Data Switch" || s.GrupKod == "Kamera Switch"));
                        if (switchCihaz != null)
                        {
                            // Hangi çizgilerin gösterileceğine karar ver
                            bool cizgiCizilecek = tumCizgileriGoster;

                            // Filtrelemeleri uygula
                            if (!cizgiCizilecek)
                            {
                                if (switchKameraCizgileriGoster && cihaz.GrupKod == "Kamera")
                                    cizgiCizilecek = true;

                                if (switchYaziciCizgileriGoster && cihaz.GrupKod == "Yazıcı")
                                    cizgiCizilecek = true;

                                if (switchKGSCizgileriGoster && cihaz.GrupKod == "KGS")
                                    cizgiCizilecek = true;
                            }

                            if (cizgiCizilecek)
                            {
                                Color renk = Color.BlueViolet;
                                if (cihaz.GrupKod == "Kamera")
                                    renk = Color.Chartreuse;//Fosforlu sarı
                                else if (cihaz.GrupKod == "Yazıcı")
                                    renk = Color.DarkOrange;
                                else if (cihaz.GrupKod == "KGS")
                                    renk = Color.Fuchsia;

                                using (Pen kalem = new Pen(renk, switchLineWidth))
                                    g.DrawLine(kalem, switchCihaz.X, switchCihaz.Y, cihaz.X, cihaz.Y);
                            }
                        }
                    }
                }

                // Switch-Switch bağlantıları her zaman göster
                if (tumCizgileriGoster)
                {
                    foreach (var cihaz in cihazlar)
                    {
                        if (cihaz.GrupKod == "Data Switch" || cihaz.GrupKod == "Kamera Switch")
                        {
                            var bagliSwitch = cihazlar.FirstOrDefault(s => s.RecNo == cihaz.SwitchRecNo && (s.GrupKod == "Data Switch" || s.GrupKod == "Kamera Switch"));
                            if (bagliSwitch != null)
                            {
                                using (Pen kahverengiKalem = new Pen(Color.Brown, brownLineWidth))
                                    g.DrawLine(kahverengiKalem, cihaz.X, cihaz.Y, bagliSwitch.X, bagliSwitch.Y);
                            }
                        }
                    }
                }
            }
        }

        public class CihazBilgileri
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
          
        }
        private void VeritabanindanCihazlariYukle()
        {
            try
            {
                var yeniCihazlar = new List<CihazBilgileri>();

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
                                yeniCihazlar.Add(new CihazBilgileri
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

                bool degisiklikVar = !tumCihazlar.SequenceEqual(yeniCihazlar, new CihazBilgiComparer());
                if (degisiklikVar)
                {
                    tumCihazlar = yeniCihazlar;

                    // KGS veya Yazıcı filtresi aktifse ona göre cihazları göster
                    if (menuKGS != null && menuKGS.Checked)
                        cihazlar = tumCihazlar.Where(c => c.GrupKod.Equals("KGS", StringComparison.OrdinalIgnoreCase)).ToList();
                    else if (menuYazici != null && menuYazici.Checked)
                        cihazlar = tumCihazlar.Where(c => c.GrupKod.Equals("Yazıcı", StringComparison.OrdinalIgnoreCase)).ToList();
                    else
                        cihazlar = new List<CihazBilgileri>(tumCihazlar);

                    panel1.Invalidate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Cihazlar yüklenirken hata oluştu: " + ex.Message,
                    "Veritabanı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public class CihazBilgiComparer : IEqualityComparer<CihazBilgileri>
        {
            public bool Equals(CihazBilgileri x, CihazBilgileri y)
            {
                return x.RecNo == y.RecNo &&
                       x.X == y.X &&
                       x.Y == y.Y &&
                       x.IPNo == y.IPNo &&
                       x.Aciklama == y.Aciklama &&
                       x.Durum == y.Durum &&
                       x.MarkaModel == y.MarkaModel &&
                       x.SwitchRecNo == y.SwitchRecNo &&
                       x.GrupKod == y.GrupKod &&
                       x.EnerjiPanoNo == y.EnerjiPanoNo;
            }
            public int GetHashCode(CihazBilgileri obj)
            {
                return obj.RecNo.GetHashCode();
            }
        }
        private void GuncelCihazBilgisiGoster(int cihazRecNo)//açılan popupda cihazın bilgilerini gösteriyorum
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = @"
                SELECT c.RecNo, c.IPNo, c.Aciklama, c.Durum, c.MarkaModel,
                       c.SwitchPortNo, c.EnerjiPanoNo, c.EnerjiPanoSigortaNo,
                       c.X, c.Y,
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
                                sb.AppendLine($"X Koordinat: {reader["X"] ?? "N/A"}");
                                sb.AppendLine($"Y Koordinat: {reader["Y"] ?? "N/A"}");
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
            this.cihazGrupTableAdapter.Fill(this.kodlariGetir.CihazGrup);
        }
        private void Harita_Resize(object sender, EventArgs e)
        {
            if (backgroundImage != null)
            {
                int genislik = Math.Max((int)(originalImageSize.Width * zoomFactor), panel1.ClientSize.Width + 1);
                int yukseklik = Math.Max((int)(originalImageSize.Height * zoomFactor), panel1.ClientSize.Height + 1);
                panel1.AutoScrollMinSize = new Size(genislik, yukseklik);
            }
            panel1.Invalidate();
            if (panelBilgi != null)
            {
                panelBilgi.Location = new Point(panel1.Width - 250, 10);
            }
        }
        private void Panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (ctrlPressed && e.Button == MouseButtons.Left)
            {
                isPanning = true;
                panStartMouse = e.Location;
                panStartScroll = new Point(-panel1.AutoScrollPosition.X, -panel1.AutoScrollPosition.Y);
                panel1.Cursor = Cursors.Hand;
            }
        }
        private void Panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                int dx = e.X - panStartMouse.X;
                int dy = e.Y - panStartMouse.Y;
                int newScrollX = panStartScroll.X - dx;
                int newScrollY = panStartScroll.Y - dy;
                panel1.AutoScrollPosition = new Point(newScrollX, newScrollY);
            }
        }
        private void Panel1_MouseWheel(object sender, MouseEventArgs e)
        {
          
            if (!ctrlPressed)
            {
                
                if (e is HandledMouseEventArgs hme)
                    hme.Handled = false;
                return;
            }

            float oldZoom = zoomFactor;

            if (e.Delta > 0 && zoomFactor < maxZoom)
                zoomFactor += zoomIncrement;
            else if (e.Delta < 0 && zoomFactor > minZoom)
                zoomFactor -= zoomIncrement;
            else
                return;

            Point scrollPos = new Point(MousePosition.X, MousePosition.Y);
            float mouseDocX = (e.X + scrollPos.X) / oldZoom;
            float mouseDocY = (e.Y + scrollPos.Y) / oldZoom;

            int newMouseScreenX = (int)(mouseDocX * zoomFactor);
            int newMouseScreenY = (int)(mouseDocY * zoomFactor);
            int newScrollX = newMouseScreenX - e.X;
            int newScrollY = newMouseScreenY - e.Y;
            int genislik = (int)(originalImageSize.Width * zoomFactor);
            int yukseklik = (int)(originalImageSize.Height * zoomFactor);
            panel1.AutoScrollMinSize = new Size(genislik, yukseklik);
            panel1.AutoScrollPosition = new Point(newScrollX, newScrollY);
            panel1.Invalidate();


            if (e is HandledMouseEventArgs hme2)
                hme2.Handled = true;
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
        private void Harita_Load(object sender, EventArgs e)
        {
        }
        private void CizgiPaneliniYukle()
        {
            panelBilgi = new Panel
            {
                Location = new Point(panel1.Width - 250, 10),
                Size = new Size(240, 150),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            // Panel içeriğini çizecek olay
            panelBilgi.Paint += CizgiPaneliniCiz;

            // Paneli form içine ekleyin
            panel1.Controls.Add(panelBilgi);

            // Panel her zaman en üstte görünsün
            panelBilgi.BringToFront();
        }
        private void CizgiPaneliniCiz(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Font font = new Font("Arial", 10);
            int startY = 10;
            int lineLength = 100;

            // Kamera Çizgisi
            ItemleriCizgiPaneliniEkle(g, "Switch", "Kamera", Color.Chartreuse, startY, font, lineLength);

            // Yazıcı Çizgisi
            ItemleriCizgiPaneliniEkle(g, "Switch", "Yazıcı", Color.DarkOrange, startY + 30, font, lineLength);

            // KGS Çizgisi
            ItemleriCizgiPaneliniEkle(g, "Switch", "KGS", Color.Fuchsia, startY + 60, font, lineLength);

            // Switch-Switch Çizgisi
            ItemleriCizgiPaneliniEkle(g, "Switch", "Switch", Color.Brown, startY + 90, font, lineLength);
        }
        private void ItemleriCizgiPaneliniEkle(Graphics g, string text1, string text2, Color lineColor, int yPos, Font font, int lineLength)
        {
            // Solda metni çiz
            g.DrawString(text1, font, Brushes.Black, 10, yPos);

            // Ortada çizgiyi çiz
            using (Pen pen = new Pen(lineColor, 3))
            {
                g.DrawLine(pen, 70, yPos + 8, 70 + lineLength, yPos + 8);
            }
            // Sağda metni çiz
            g.DrawString(text2, font, Brushes.Black, 180, yPos);
        }
    }
}
