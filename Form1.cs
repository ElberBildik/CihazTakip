using System;
using System.Data;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Cihaz_Takip_Uygulaması
{
    public partial class Form1 : Form
    {
        private Timer pingTimer; // Timer değişkeni
        private int currentRowIndex = 0; // Veritabanındaki satırın sırası

        public Form1()
        {
            InitializeComponent();
            VerileriYukle(); // Form ilk açıldığında verileri getir
            InitializeTimer(); // Timer'ı başlat
        }

        private void InitializeTimer()
        {
            pingTimer = new Timer();
            pingTimer.Interval = 1000; // 1 saniyede bir kontrol et
            pingTimer.Tick += PingTimer_Tick; // Her zaman diliminde yapılacak işlemi belirle
        }

        private async void PingTimer_Tick(object sender, EventArgs e)
        {
            var tasks = new List<Task>();

            // Tüm satırlar için paralel kontrol yap
            foreach (DataGridViewRow row in Cihazlar.Rows)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string durum = row.Cells["Durum"].Value?.ToString();

                        if (durum == null)
                            return;

                        int grupRecNo = Convert.ToInt32(row.Cells["GrupRecNo"].Value);
                        string ip = row.Cells["IPNo"].Value?.ToString();
                        string aciklama = row.Cells["Aciklama"].Value?.ToString();

                        // "Down oldu, mail atılacak" durumunu kontrol et
                        if (durum.Contains("Down oldu, mail atılacak"))
                        {
                            DateTime sonPing = DBHelper.GetSonPingZamani(grupRecNo);
                            int mailBeklemeSuresi = DBHelper.GetMailBeklemeSuresi(grupRecNo);

                            // Geçen süreyi hesapla
                            TimeSpan fark = DateTime.Now - sonPing;

                            // Log kaydını ekle
                            Invoke(new Action(() =>
                            {
                                AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] - Down durum kontrolü yapılıyor...", Color.Orange);
                                AppendColoredText($"      Son ping zamanı: {sonPing}", Color.DarkGray);
                                AppendColoredText($"      Geçen süre: {Math.Floor(fark.TotalMinutes)} dk", Color.DarkGray);
                                AppendColoredText($"      Bekleme süresi: {mailBeklemeSuresi} dk", Color.DarkGray);
                            }));

                            // Bekleme süresi geçtiyse mail at
                            if (fark.TotalMinutes >= mailBeklemeSuresi)
                            {
                                string toMailAdres = DBHelper.GetMailAdres(grupRecNo);

                                if (!string.IsNullOrEmpty(toMailAdres))
                                {
                                    string body = $@"CIHAZ UYARISI:

IP: {ip}
Açıklama: {aciklama}
Son Ping: {sonPing}
Bekleme Süresi: {mailBeklemeSuresi} dk
Durum: {durum}
";

                                    await MailHelper.GonderAsync(toMailAdres, "CIHAZ UYARISI", body);

                                    // Mail atıldıktan sonra durumu güncelle
                                    DBHelper.GuncelleDurum(grupRecNo);

                                    // DataGridView hücresinin rengini yeşil yap
                                    row.Cells["Durum"].Style.BackColor = Color.Green;

                                    // Log kayıt
                                    Invoke(new Action(() =>
                                    {
                                        AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı için mail gönderildi: {toMailAdres}", Color.Green);
                                        AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı için durum güncellendi: 'down oldu, mail atıldı'", Color.Blue);
                                    }));
                                }
                                else
                                {
                                    Invoke(new Action(() =>
                                    {
                                        AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı için mail adresi bulunamadı!", Color.Red);
                                    }));
                                }
                            }
                            else
                            {
                                // Henüz bekleme süresi dolmadı
                                Invoke(new Action(() =>
                                {
                                    AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı için bekleme süresi dolmadı. Kalan: {mailBeklemeSuresi - Math.Floor(fark.TotalMinutes)} dk", Color.Yellow);
                                    // Durum hücresini sarı yap
                                    row.Cells["Durum"].Style.BackColor = Color.Yellow;
                                }));
                            }
                        }
                        // "Up" durumunu kontrol et
                        else if (durum.Contains("Up"))
                        {
                            // Up durumunda güncelleme yap
                            Invoke(new Action(() =>
                            {
                                AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı Up durumunda.", Color.Green);
                                // Durum hücresini yeşil yap
                                row.Cells["Durum"].Style.BackColor = Color.Green;
                            }));
                        }
                    }
                    catch (Exception ex)
                    {
                        Invoke(new Action(() =>
                        {
                            AppendColoredText($"[{DateTime.Now:HH:mm:ss}] Hata: {ex.Message}", Color.Red);
                        }));
                    }
                }));
            }

            // Tüm işlemlerin tamamlanmasını bekle
            await Task.WhenAll(tasks);

            // DataGridView'ı güncelle ve renklendirme yap
            Invoke(new Action(() =>
            {
                Cihazlar.Refresh();
                // HücreRenkleme sınıfını kullanarak durum renklendir
                HücreRenkleme.DurumRenklendir(Cihazlar);
            }));
        }

        // RichTextBox'a renkli metin ekleme yardımcı metodu
        private void AppendColoredText(string text, Color color)
        {
            MesajlarRchTxt.SelectionStart = MesajlarRchTxt.TextLength;
            MesajlarRchTxt.SelectionLength = 0;
            MesajlarRchTxt.SelectionColor = color;
            MesajlarRchTxt.AppendText(text + Environment.NewLine);
            MesajlarRchTxt.SelectionColor = MesajlarRchTxt.ForeColor;

            // Otomatik kaydırma
            MesajlarRchTxt.ScrollToCaret();
        }

        private void PingAtBtn_Click(object sender, EventArgs e)
        {
            MesajlarRchTxt.Clear(); // Mesaj kutusunu temizle
            AppendColoredText("Ping işlemi başlatılıyor...", Color.Blue);

            // Timer'ı başlat
            pingTimer.Start();
        }

        private void StopPingBtn_Click(object sender, EventArgs e)
        {
            // Timer'ı durdur
            pingTimer.Stop();
            AppendColoredText("Ping işlemi durduruldu.", Color.Blue);
        }

        private void VerileriYukle()
        {
            try
            {
                DataTable dt = VeriErisim.VerileriGetir(); // Sınıftan verileri al
                Cihazlar.DataSource = dt;

                // Otomatik sütun ve satır boyutlandırma
                Cihazlar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                Cihazlar.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

                Cihazlar.Refresh();

                // Durum renklendirme için HücreRenkleme sınıfını kullan
                HücreRenkleme.DurumRenklendir(Cihazlar);

                AppendColoredText("Cihaz verileri başarıyla yüklendi.", Color.Green);
            }
            catch (Exception ex)
            {
                AppendColoredText($"Veriler yüklenirken hata oluştu: {ex.Message}", Color.Red);
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message);
            }
        }

        private void PingIptalBtn_Click(object sender, EventArgs e)
        {
            // Kullanıcıya onay sorusu sormak için MessageBox kullanıyoruz
            DialogResult result = MessageBox.Show("Ping atma işlemini durdurmak istiyor musunuz?",
                                                 "İşlem İptali",
                                                 MessageBoxButtons.YesNo,
                                                 MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Eğer kullanıcı "Evet" derse, timer'ı durduruyoruz
                pingTimer.Stop();
                AppendColoredText("Ping işlemi durduruldu.", Color.Blue);
            }
            else
            {
                // Kullanıcı "Hayır" derse, herhangi bir işlem yapılmaz
                AppendColoredText("Ping işlemi devam ediyor.", Color.Green);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e) // refresh butonu
        {
            AppendColoredText("Veriler yenileniyor...", Color.Blue);
            DataTable dt = VeriErisim.VerileriGetir(); // Sınıftan verileri al
            Cihazlar.DataSource = dt;

            // Otomatik sütun ve satır boyutlandırma
            Cihazlar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Cihazlar.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            Cihazlar.Refresh();

            // Durum renklendirme için HücreRenkleme sınıfını kullan
            HücreRenkleme.DurumRenklendir(Cihazlar);

            AppendColoredText("Veriler başarıyla yenilendi.", Color.Green);
        }

        private void pictureBox2_Click(object sender, EventArgs e) // arama yapma butonu
        {
            string ipNo = araTxtBox.Text; // TextBox'a girilen IP numarasını al

            // Eğer kullanıcı boş bir değer girerse, filtreleme yapılmaz
            if (string.IsNullOrWhiteSpace(ipNo))
            {
                AppendColoredText("Lütfen bir IP numarası girin.", Color.Red);
                MessageBox.Show("Lütfen bir IP numarası girin.");
                return;
            }

            AppendColoredText($"'{ipNo}' IP numarası aranıyor...", Color.Blue);

            DataTable dt = VeriErisim.VerileriGetir(); // Veritabanından verileri al

            // Buradaki LIKE ifadesinin doğru sözdizimi ile kullanılması gerekiyor.
            string filterExpression = string.Format("IPNo LIKE '%{0}%'", ipNo); // Doğru sözdizimi

            try
            {
                DataRow[] filteredRows = dt.Select(filterExpression); // Filtreleme işlemi

                // Filtrelenmiş satırları yeni bir DataTable'a aktar
                DataTable filteredDataTable = dt.Clone(); // Yeni bir DataTable oluşturuyoruz
                foreach (DataRow row in filteredRows)
                {
                    filteredDataTable.ImportRow(row); // Filtrelenmiş satırları ekliyoruz
                }

                // Filtrelenmiş verileri DataGridView'e atıyoruz
                Cihazlar.DataSource = filteredDataTable;
                Cihazlar.Refresh();

                // Durum renklendirme için HücreRenkleme sınıfını kullan
                HücreRenkleme.DurumRenklendir(Cihazlar);

                AppendColoredText($"Arama tamamlandı. {filteredDataTable.Rows.Count} sonuç bulundu.", Color.Green);
            }
            catch (SyntaxErrorException ex)
            {
                AppendColoredText($"Filtreleme işlemi sırasında hata oluştu: {ex.Message}", Color.Red);
                MessageBox.Show("Filtreleme işlemi sırasında hata oluştu: " + ex.Message);
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {
            // Bu metod boş kalabilir
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AppendColoredText("Uygulama başlatıldı.", Color.Blue);
        }
    }
}