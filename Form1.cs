using System;
using System.Data;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Cihaz_Takip_Uygulaması
{
    public partial class Form1 : Form
    {
        private Timer pingTimer; // Timer değişkeni
        private DataTable downCihazlarTable; // Down cihazları saklamak için tablo
        private Dictionary<int, Timer> downCihazTimers = new Dictionary<int, Timer>(); // Her cihaz için ayrı timer
                                                                                      
        private Dictionary<int, string> cihazDurumlari = new Dictionary<int, string>(); // Cihazların son bilinen durumlarını tutan sözlük

        public Form1()
        {
            InitializeComponent();
            InitializeDownCihazlarGrid(); // Down cihazlar için grid hazırla
            VerileriYukle();
            InitializeTimer(); // Timer'ı başlat
        }

        private void InitializeDownCihazlarGrid()
        {
            // Down cihazlar için yeni DataTable oluştur
            downCihazlarTable = new DataTable();
            downCihazlarTable.Columns.Add("RecNo", typeof(int));
            downCihazlarTable.Columns.Add("GrupRecNo", typeof(int));
            downCihazlarTable.Columns.Add("IPNo", typeof(string));
            downCihazlarTable.Columns.Add("Aciklama", typeof(string));
            downCihazlarTable.Columns.Add("DownZamani", typeof(DateTime));
            downCihazlarTable.Columns.Add("BeklemeSuresi", typeof(int));
            downCihazlarTable.Columns.Add("KalanSure", typeof(string));
            downCihazlarTable.Columns.Add("Durum", typeof(string));

            // DataGridView'e veri kaynağını bağla
            downCihazlar.DataSource = downCihazlarTable;

            // Otomatik sütun ve satır boyutlandırma
            downCihazlar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            downCihazlar.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Tarih formatını ayarla
            downCihazlar.Columns["DownZamani"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";

            // Sütun başlıklarını ayarla
            downCihazlar.Columns["RecNo"].HeaderText = "Cihaz No";
            downCihazlar.Columns["IPNo"].HeaderText = "IP Adresi";
            downCihazlar.Columns["DownZamani"].HeaderText = "Down Zamanı";
            downCihazlar.Columns["KalanSure"].HeaderText = "Kalan Süre";
        }

        private void InitializeTimer()
        {
            pingTimer = new Timer();
            pingTimer.Interval = 1000; // 1 saniyede bir kontrol et
            pingTimer.Tick += PingTimer_Tick; // Her zaman diliminde yapılacak işlemi belirle
        }

        private async void PingTimer_Tick(object sender, EventArgs e)//down ve up mesajlarını burada bastırıyorum mesajlar kısmında değişilikiği burada yapabilirim
        {
            MesajlarRchTxt.Clear();
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

                        int grupRecNo = Convert.ToInt32(row.Cells["RecNo"].Value);
                        int CihazinGrupNumarasi = Convert.ToInt32(row.Cells["GrupRecNo"].Value);
                        string ip = row.Cells["IPNo"].Value?.ToString();
                        string aciklama = row.Cells["Aciklama"].Value?.ToString();

                        // Ping işlemine başlamadan önce satırı sarıya boyama
                        Invoke(new Action(() =>
                        {
                            row.DefaultCellStyle.BackColor = Color.Yellow;
                        }));

                        // Ping atma işlemi
                        bool pingSonucu = await PingAt(ip);

                        if (pingSonucu)
                        {
                            // Ping başarılıysa
                            Invoke(new Action(() =>
                            {
                                row.Cells["Durum"].Value = "UP";
                                row.DefaultCellStyle.BackColor = Color.Green; // Yeşil renk
                                AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı Up durumunda.", Color.Green);

                                // Eğer cihaz down cihazlar listesindeyse çıkar
                                RemoveFromDownCihazlar(grupRecNo);
                            }));
                            DBHelper.GuncelleDurum(grupRecNo, "UP");
                        }
                        else // Ping başarısızsa
                        {
                            Invoke(new Action(() =>
                            {
                                row.Cells["Durum"].Value = "Down oldu, mail atılacak";
                                row.DefaultCellStyle.BackColor = Color.Red; // Kırmızı renk
                                AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı Down oldu.", Color.Red);

                                // Down cihazlar listesine ekle
                                AddToDownCihazlar(grupRecNo, CihazinGrupNumarasi, ip, aciklama);
                            }));
                            DBHelper.GuncelleDurum(grupRecNo, "Down oldu, mail atılacak");

                            // Cihaz "Down" olduğunda log kaydı ekle
                            DBHelper.CihazDownKaydi(grupRecNo);
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

                // Down cihazların kalan sürelerini güncelle
                UpdateDownCihazlarKalanSure();
            }));
        }

        private void AddToDownCihazlar(int recNo, int grupRecNo, string ip, string aciklama)
        {
            // Eğer cihaz zaten eklenmiş ise tekrar ekleme
            foreach (DataRow row in downCihazlarTable.Rows)
            {
                if (Convert.ToInt32(row["RecNo"]) == recNo)
                    return;
            }

            // Bekleme süresini veritabanından al
            int beklemeSuresi = DBHelper.GetMailBeklemeSuresi(grupRecNo);
            DateTime downZamani = DateTime.Now;

            // Yeni satır ekle
            DataRow newRow = downCihazlarTable.NewRow();
            newRow["RecNo"] = recNo;
            newRow["GrupRecNo"] = grupRecNo;
            newRow["IPNo"] = ip;
            newRow["Aciklama"] = aciklama;
            newRow["DownZamani"] = downZamani;
            newRow["BeklemeSuresi"] = beklemeSuresi;
            newRow["KalanSure"] = beklemeSuresi.ToString() + " dk";
            newRow["Durum"] = "Mail bekleniyor";
            downCihazlarTable.Rows.Add(newRow);

            // Bu cihaz için ayrı bir timer başlat
            StartDownCihazTimer(recNo, grupRecNo, beklemeSuresi, downZamani, ip, aciklama);

            // Log ekle
            AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı Down listesine eklendi. Bekleme süresi: {beklemeSuresi} dk", Color.Orange);
        }

        private void StartDownCihazTimer(int recNo, int grupRecNo, int beklemeSuresi, DateTime downZamani, string ip, string aciklama)
        {
            // Eğer bu cihaz için zaten bir timer varsa önce onu durdur ve kaldır
            if (downCihazTimers.ContainsKey(recNo))
            {
                downCihazTimers[recNo].Stop();
                downCihazTimers[recNo].Dispose();
                downCihazTimers.Remove(recNo);
            }

            // Yeni timer oluştur
            Timer cihazTimer = new Timer();
            cihazTimer.Interval = 1000; // 1 saniyede bir güncelleme yap

            // Bekleme süresini saniyeye çevir
            int kalanSaniye = beklemeSuresi * 60;

            cihazTimer.Tick += (sender, e) =>
            {
                // Kalan süreyi 1 saniye azalt
                kalanSaniye--;

                // Kalan süreyi güncelle
                UpdateKalanSure(recNo, kalanSaniye);

                // Tüm down cihazların sürelerini güncelle
                UpdateDownCihazlarKalanSure();

                // Bekleme süresi dolduysa mail gönder
                if (kalanSaniye <= 0)
                {
                    SendMailAndUpdateStatus(recNo, grupRecNo, ip, aciklama, downZamani, beklemeSuresi);

                    // Timer'ı durdur
                    cihazTimer.Stop();
                    cihazTimer.Dispose();
                    downCihazTimers.Remove(recNo);
                }
            };

            // Timer'ı başlat ve sözlüğe ekle
            cihazTimer.Start();
            downCihazTimers.Add(recNo, cihazTimer);
        }

        private async void SendMailAndUpdateStatus(int recNo, int grupRecNo, string ip, string aciklama, DateTime downZamani, double gecenDakika)
        {
            try
            {
                string mailAdres = DBHelper.GetMailAdres(grupRecNo);
                string konu = $"[CIHAZ DOWN] {aciklama}";
                string icerik = $"{aciklama} cihazı {downZamani} tarihinde erişilemez oldu.\n{gecenDakika:F1} dakikadır bağlantı sağlanamıyor.\nIP Adresi: {ip}";

                // Mail gönderimi
                await MailHelper.GonderAsync(mailAdres, konu, icerik);

                // Veritabanında durum güncelleme
                DBHelper.GuncelleDurum(grupRecNo, "Down durumda, mail gönderildi");

                // DataGridView'de durum güncelleme
                foreach (DataRow row in downCihazlarTable.Rows)
                {
                    if (Convert.ToInt32(row["RecNo"]) == recNo)
                    {
                        row["Durum"] = "Mail gönderildi";
                        break;
                    }
                }

                // Cihazlar DataGridView'de durum güncelleme
                foreach (DataGridViewRow row in Cihazlar.Rows)
                {
                    if (Convert.ToInt32(row.Cells["RecNo"].Value) == recNo)
                    {
                        row.Cells["Durum"].Value = "Down mail atıldı";
                        break;
                    }
                }

                // rchTextBildirimler kontrolüne bildirim mesajı ekleme
                AppendToBildirimler($"[{DateTime.Now:HH:mm:ss}] {ip} için mail gönderildi. Konu: {konu}, Adres: {mailAdres}");

                // Loglama ve kullanıcıya bilgi verme
                AppendColoredText($"[{DateTime.Now:HH:mm:ss}] {ip} için bekleme süresi aşıldı. Mail gönderildi ve durum güncellendi. IP Adresi: {ip}", Color.Orange);
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama ve renkli mesaj
                AppendColoredText($"[{DateTime.Now:HH:mm:ss}] Mail gönderirken hata: {ex.Message}. IP Adresi: {ip}", Color.Red);
            }
        }

        private void UpdateKalanSure(int recNo, int kalanDakika)
        {
            foreach (DataRow row in downCihazlarTable.Rows)
            {
                if (Convert.ToInt32(row["RecNo"]) == recNo)
                {
                    row["KalanSure"] = kalanDakika.ToString() + " dk";
                    break;
                }
            }
        }

        private void UpdateDownCihazlarKalanSure()//kalan süreyi hesaplıyoruz
        {
            // Her satır için kalan süreyi güncelle
            foreach (DataRow row in downCihazlarTable.Rows)
            {
                int recNo = Convert.ToInt32(row["RecNo"]);
                if (downCihazTimers.ContainsKey(recNo))
                {
                    // Down zamanını ve bekleme süresini al
                    DateTime downZamani = (DateTime)row["DownZamani"];
                    int beklemeSuresi = Convert.ToInt32(row["BeklemeSuresi"]);

                    // Kalan süreyi hesapla (saniye bazında)
                    TimeSpan gecenSure = DateTime.Now - downZamani;
                    int kalanSaniye = (beklemeSuresi * 60) - (int)gecenSure.TotalSeconds;
                    if (kalanSaniye < 0) kalanSaniye = 0;

                    // Kalan süreyi güncelle
                    int dakika = kalanSaniye / 60;
                    int saniye = kalanSaniye % 60;
                    row["KalanSure"] = $"{dakika:D2}:{saniye:D2}";
                }
            }

            // DataGridView'ı yenile
            downCihazlar.Refresh();
        }

        private void RemoveFromDownCihazlar(int recNo)
        {
            // Down cihazlar tablosunda bu cihazı ara
            DataRow rowToDelete = null;
            foreach (DataRow row in downCihazlarTable.Rows)
            {
                if (Convert.ToInt32(row["RecNo"]) == recNo)
                {
                    rowToDelete = row;
                    break;
                }
            }

            // Eğer cihaz bulunduysa
            if (rowToDelete != null)
            {
                string ip = rowToDelete["IPNo"].ToString();

                // Timer'ı durdur ve kaldır
                if (downCihazTimers.ContainsKey(recNo))
                {
                    downCihazTimers[recNo].Stop();
                    downCihazTimers[recNo].Dispose();
                    downCihazTimers.Remove(recNo);
                }

                // Satırı tablodan sil
                downCihazlarTable.Rows.Remove(rowToDelete);

                AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı aktif duruma geçti ve down listesinden çıkarıldı.", Color.Green);
            }
        }

        private async Task<bool> PingAt(string ip)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ip, 1000); // 1 saniye de timeout
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private async void PingAtBtn_Click(object sender, EventArgs e)
        {
            pingTimer.Start();

            // Ping işlemi başlatılıyor
            AppendColoredText("Ping işlemi başlatılıyor...", Color.Blue);

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

                        int grupRecNo = Convert.ToInt32(row.Cells["RecNo"].Value);
                        int CihazinGrupNumarasi = Convert.ToInt32(row.Cells["GrupRecNo"].Value);
                        string ip = row.Cells["IPNo"].Value?.ToString();
                        string aciklama = row.Cells["Aciklama"].Value?.ToString();

                        // Ping işlemine başlamadan önce satırı sarıya boyama
                        Invoke(new Action(() =>
                        {
                            row.DefaultCellStyle.BackColor = Color.Yellow;
                        }));

                        // Ping atma işlemi
                        bool pingSonucu = await PingAt(ip);

                        if (pingSonucu)
                        {
                            // Ping başarılıysa
                            Invoke(new Action(() =>
                            {
                                row.Cells["Durum"].Value = "UP";
                                row.DefaultCellStyle.BackColor = Color.Green; // Yeşil renk
                                AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı Up durumunda.", Color.Green);

                                // Eğer cihaz down cihazlar listesindeyse çıkar
                                RemoveFromDownCihazlar(grupRecNo);
                            }));
                            DBHelper.GuncelleDurum(grupRecNo, "UP");
                        }
                        else
                        {
                            // Ping başarısızsa
                            Invoke(new Action(() =>
                            {
                                row.Cells["Durum"].Value = "Down oldu, mail atılacak";
                                row.DefaultCellStyle.BackColor = Color.Red;
                                AppendColoredText($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı Down oldu, mail atılacak.", Color.Red);

                                // Down cihazlar listesine ekle
                                AddToDownCihazlar(grupRecNo, CihazinGrupNumarasi, ip, aciklama);
                            }));
                            DBHelper.GuncelleDurum(grupRecNo, "Down oldu, mail atılacak");

                            // Cihaz "Down" olduğunda log kaydı ekle
                            DBHelper.CihazDownKaydi(grupRecNo);
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
                downCihazlar.Refresh();

                // Durum renklendirme için HücreRenkleme sınıfını kullan
                HücreRenkleme.DurumRenklendir(Cihazlar);
            }));

            // Ping işlemini başlat
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
                // Eğer kullanıcı "Evet" derse, ping timer'ı durduruyoruz
                pingTimer.Stop();
                AppendColoredText("Ping işlemi durduruldu.", Color.Blue);

                // Tüm down cihazlar için çalışan timer'ları durdur
                foreach (var timer in downCihazTimers.Values)
                {
                    timer.Stop();
                }

                AppendColoredText("Tüm geri sayım işlemleri durduruldu.", Color.Blue);
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

            // Durum renklendirme için HücreRenkleme sınıfını kullandım
            HücreRenkleme.DurumRenklendir(Cihazlar);

            AppendColoredText("Veriler başarıyla yenilendi.", Color.Green);
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
        private void AppendToBildirimler(string text)
        {
            rchTextBildirimler.SelectionStart = MesajlarRchTxt.TextLength;
            rchTextBildirimler.SelectionLength = 0;
            rchTextBildirimler.SelectionColor = Color.Orange; // Bildirimler için özel renk
            rchTextBildirimler.AppendText(text + Environment.NewLine);
            rchTextBildirimler.SelectionColor = MesajlarRchTxt.ForeColor;

            // Otomatik kaydırma
            rchTextBildirimler.ScrollToCaret();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            AppendColoredText("Uygulama başlatıldı.", Color.Blue);
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
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

        // Down cihazlar gridindeki bir satırı tıklama işlemi
        private void downCihazlar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int recNo = Convert.ToInt32(downCihazlar.Rows[e.RowIndex].Cells["RecNo"].Value);
                string ip = downCihazlar.Rows[e.RowIndex].Cells["IPNo"].Value.ToString();
                string durum = downCihazlar.Rows[e.RowIndex].Cells["Durum"].Value.ToString();

                AppendColoredText($"[{DateTime.Now:HH:mm:ss}] Seçilen down cihaz: {ip}, Durum: {durum}", Color.Blue);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form frm = new Harita();
            frm.Show();
        }
    }
}