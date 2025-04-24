using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;

namespace Cihaz_Takip_Uygulaması
{
    public partial class Form1 : Form
    {
        private Timer _pingTimer;
        private DataTable _downCihazlarTable;
        private readonly Dictionary<int, Timer> _downCihazTimers;
        private readonly Dictionary<int, string> _cihazDurumlari;
        private const int PING_INTERVAL = 1000; // 1 saniye
        private const int PING_TIMEOUT = 1000; // 1 saniye

        public Form1()
        {
            InitializeComponent();
            _downCihazTimers = new Dictionary<int, Timer>();
            _cihazDurumlari = new Dictionary<int, string>();
            InitializeDownCihazlarGrid();
            InitializePingTimer();
            LoadDeviceData();
        }

        private void InitializeDownCihazlarGrid()
        {
            _downCihazlarTable = new DataTable();
            _downCihazlarTable.Columns.AddRange(new[]
            {
                new DataColumn("RecNo", typeof(int)),
                new DataColumn("GrupRecNo", typeof(int)),
                new DataColumn("IPNo", typeof(string)),
                new DataColumn("Aciklama", typeof(string)),
                new DataColumn("DownZamani", typeof(DateTime)),
                new DataColumn("BeklemeSuresi", typeof(int)),
                new DataColumn("KalanSure", typeof(string)),
                new DataColumn("Durum", typeof(string))
            });

            downCihazlar.DataSource = _downCihazlarTable;
            ConfigureDownCihazlarGridView();
        }

        private void ConfigureDownCihazlarGridView()
        {
            downCihazlar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            downCihazlar.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            downCihazlar.Columns["DownZamani"].DefaultCellStyle.Format = "dd.MM.yyyy HH:mm:ss";
            downCihazlar.Columns["RecNo"].HeaderText = "Cihaz No";
            downCihazlar.Columns["IPNo"].HeaderText = "IP Adresi";
            downCihazlar.Columns["DownZamani"].HeaderText = "Down Zamanı";
            downCihazlar.Columns["KalanSure"].HeaderText = "Kalan Süre";
        }

        private void InitializePingTimer()
        {
            _pingTimer = new Timer { Interval = PING_INTERVAL };
            _pingTimer.Tick += async (sender, e) => await PingTimer_Tick(sender, e);
        }

        private async Task PingTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                MesajlarRchTxt.Clear();

                await Task.Run(() =>
                {
                    Invoke(new Action(() =>
                    {
                        DataTable dt = VeriErisim.VerileriGetir();
                        Cihazlar.DataSource = dt;
                        ConfigureDevicesGridView();
                    }));
                });

                var tasks = new List<Task>();

                foreach (DataGridViewRow row in Cihazlar.Rows)
                {
                    tasks.Add(ProcessDeviceRowAsync(row));
                }

                await Task.WhenAll(tasks);

                await UpdateUIAsync();
            }
            catch (Exception ex)
            {
                LogMessage($"[{DateTime.Now:HH:mm:ss}] Hata: {ex.Message}", Color.Red);
            }
        }

        private async Task UpdateUIAsync()
        {
            await Task.Run(() =>
            {
                Invoke(new Action(() =>
                {
                    Cihazlar.Refresh();
                    HücreRenkleme.DurumRenklendir(Cihazlar);
                    UpdateDownDevicesRemainingTime();
                }));
            });
        }

        private async Task ProcessDeviceRowAsync(DataGridViewRow row)
        {
            try
            {
                if (row.Cells["Durum"].Value == null) return;

                int cihazRecNo = Convert.ToInt32(row.Cells["RecNo"].Value);
                int cihazGrupRecNo = Convert.ToInt32(row.Cells["GrupRecNo"].Value);
                string ip = row.Cells["IPNo"].Value?.ToString();
                string aciklama = row.Cells["Aciklama"].Value?.ToString();

                await Task.Run(() => Invoke(new Action(() => row.DefaultCellStyle.BackColor = Color.Yellow)));

                bool pingResult = await SendPingAsync(ip);
                await UpdateDeviceStatusAsync(row, cihazRecNo, cihazGrupRecNo, ip, aciklama, pingResult);
            }
            catch (Exception ex)
            {
                await Task.Run(() => Invoke(new Action(() =>
                    LogMessage($"[{DateTime.Now:HH:mm:ss}] Hata: {ex.Message}", Color.Red)
                )));
            }
        }

        private async Task<bool> SendPingAsync(string ip)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = await ping.SendPingAsync(ip, PING_TIMEOUT);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private async Task UpdateDeviceStatusAsync(DataGridViewRow row, int cihazRecNo, int cihazGrupRecNo,
           string ip, string aciklama, bool isOnline)
        {
            if (isOnline)
            {
                await HandleDeviceOnlineAsync(row, cihazRecNo, ip);
            }
            else
            {
                await HandleDeviceOfflineAsync(row, cihazRecNo, cihazGrupRecNo, ip, aciklama);
            }
        }

        private async Task HandleDeviceOnlineAsync(DataGridViewRow row, int cihazRecNo, string ip)
        {
            await Task.Run(() =>
            {
                Invoke(new Action(() =>
                {
                    row.Cells["Durum"].Value = "UP";
                    row.DefaultCellStyle.BackColor = Color.Green;
                    LogMessage($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı Up durumunda.", Color.Green);
                    RemoveFromDownDevices(cihazRecNo);
                }));
            });

            await Task.Run(() => DBHelper.GuncelleDurum(cihazRecNo, "UP"));
        }

        private async Task HandleDeviceOfflineAsync(DataGridViewRow row, int cihazRecNo,
    int cihazGrupRecNo, string ip, string aciklama)
        {
            await Task.Run(() =>
            {
                Invoke(new Action(() =>
                {
                    try
                    {
                        if (row.DataGridView != null && row.DataGridView.Columns.Contains("Durum"))
                        {
                            row.Cells["Durum"].Value = "Down oldu, mail atılacak";
                        }
                        else
                        {
                            LogMessage($"[{DateTime.Now:HH:mm:ss}] 'Durum' sütunu bulunamadı!", Color.Red);
                        }

                        row.DefaultCellStyle.BackColor = Color.Red;
                        LogMessage($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı Down oldu.", Color.Red);
                        AddToDownDevices(cihazRecNo, cihazGrupRecNo, ip, aciklama);
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"[{DateTime.Now:HH:mm:ss}] Hata: {ex.Message}", Color.Red);
                    }
                }));
            });

            await Task.Run(() =>
            {
                DBHelper.GuncelleDurum(cihazRecNo, "Down oldu, mail atılacak");
                DBHelper.CihazDownKaydi(cihazRecNo);
            });
        }

        private void AddToDownDevices(int cihazRecNo, int grupRecNo, string ip, string aciklama)
        {
            
            foreach (DataRow row in _downCihazlarTable.Rows)
            {
                if (Convert.ToInt32(row["RecNo"]) == cihazRecNo)
                    return;
            }

            int beklemeSuresi = DBHelper.GetMailBeklemeSuresi(grupRecNo);
            DateTime downZamani = DateTime.Now;

            DataRow newRow = _downCihazlarTable.NewRow();
            newRow["RecNo"] = cihazRecNo;
            newRow["GrupRecNo"] = grupRecNo;
            newRow["IPNo"] = ip;
            newRow["Aciklama"] = aciklama;
            newRow["DownZamani"] = downZamani;
            newRow["BeklemeSuresi"] = beklemeSuresi;
            newRow["KalanSure"] = beklemeSuresi.ToString() + " dk";
            newRow["Durum"] = "Mail bekleniyor";
            _downCihazlarTable.Rows.Add(newRow);

            StartDownDeviceTimer(cihazRecNo, grupRecNo, beklemeSuresi, downZamani, ip, aciklama);
            LogMessage($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı Down listesine eklendi. Bekleme süresi: {beklemeSuresi} dk", Color.Orange);
        }

        private void StartDownDeviceTimer(int cihazRecNo, int grupRecNo, int beklemeSuresi,
            DateTime downZamani, string ip, string aciklama)
        {
            if (_downCihazTimers.ContainsKey(cihazRecNo))
            {
                _downCihazTimers[cihazRecNo].Stop();
                _downCihazTimers[cihazRecNo].Dispose();
                _downCihazTimers.Remove(cihazRecNo);
            }

            Timer cihazTimer = new Timer { Interval = 1000 };
            int kalanSaniye = beklemeSuresi * 60;

            cihazTimer.Tick += (sender, e) =>
            {
                kalanSaniye--;
                UpdateRemainingTime(cihazRecNo, kalanSaniye);
                UpdateDownDevicesRemainingTime();

                if (kalanSaniye <= 0)
                {
                    SendMailAndUpdateStatus(cihazRecNo, grupRecNo, ip, aciklama, downZamani, beklemeSuresi);
                    cihazTimer.Stop();
                    cihazTimer.Dispose();
                    _downCihazTimers.Remove(cihazRecNo);
                }
            };

            cihazTimer.Start();
            _downCihazTimers.Add(cihazRecNo, cihazTimer);
        }
        private async void SendMailAndUpdateStatus(int cihazRecNo, int grupRecNo, string ip,
            string aciklama, DateTime downZamani, double gecenDakika)
        {
            try
            {
                string mailAdres = DBHelper.GetMailAdres(grupRecNo);
                string konu = $"[CIHAZA ERISILEMIYOR] {aciklama}";
                string icerik = $"{aciklama} cihazı {downZamani} tarihinde erişilemez oldu.\n" +
                                $"{gecenDakika:F1} dakikadır bağlantı sağlanamıyor.\nIP Adresi: {ip}";

                await MailHelper.GonderAsync(mailAdres, konu, icerik, cihazRecNo);
                await Task.Run(() => DBHelper.GuncelleDurum(cihazRecNo, "Down durumda, mail gönderildi"));

                UpdateStatusAfterMailSent(cihazRecNo, ip, konu, mailAdres);
            }
            catch (Exception ex)
            {
                LogMessage($"[{DateTime.Now:HH:mm:ss}] Mail gönderirken hata: {ex.Message}. IP Adresi: {ip}", Color.Red);
            }
        }

        private void UpdateStatusAfterMailSent(int cihazRecNo, string ip, string konu, string mailAdres)
        {
            foreach (DataRow row in _downCihazlarTable.Rows)
            {
                if (Convert.ToInt32(row["RecNo"]) == cihazRecNo)
                {
                    row["Durum"] = "Mail gönderildi";
                    break;
                }
            }

            foreach (DataGridViewRow row in Cihazlar.Rows)
            {
                if (Convert.ToInt32(row.Cells["RecNo"].Value) == cihazRecNo)
                {
                    row.Cells["Durum"].Value = "Down mail atıldı";
                    break;
                }
            }

            AddNotification($"[{DateTime.Now:HH:mm:ss}] {ip} için mail gönderildi. Konu: {konu}, Adres: {mailAdres}");
            LogMessage($"[{DateTime.Now:HH:mm:ss}] {ip} için bekleme süresi aşıldı. Mail gönderildi ve durum güncellendi. IP Adresi: {ip}", Color.Orange);
        }

        private void UpdateRemainingTime(int cihazRecNo, int kalanSaniye)
        {
            foreach (DataRow row in _downCihazlarTable.Rows)
            {
                if (Convert.ToInt32(row["RecNo"]) == cihazRecNo)
                {
                    int dakika = kalanSaniye / 60;
                    int saniye = kalanSaniye % 60;
                    row["KalanSure"] = $"{dakika:D2}:{saniye:D2}";
                    break;
                }
            }
        }

        private void UpdateDownDevicesRemainingTime()
        {
            foreach (DataRow row in _downCihazlarTable.Rows)
            {
                int cihazRecNo = Convert.ToInt32(row["RecNo"]);
                if (_downCihazTimers.ContainsKey(cihazRecNo))
                {
                    DateTime downZamani = (DateTime)row["DownZamani"];
                    int beklemeSuresi = Convert.ToInt32(row["BeklemeSuresi"]);

                    TimeSpan gecenSure = DateTime.Now - downZamani;
                    int kalanSaniye = (beklemeSuresi * 60) - (int)gecenSure.TotalSeconds;
                    if (kalanSaniye < 0) kalanSaniye = 0;

                    int dakika = kalanSaniye / 60;
                    int saniye = kalanSaniye % 60;
                    row["KalanSure"] = $"{dakika:D2}:{saniye:D2}";
                }
            }

            downCihazlar.Refresh();
        }

        private void RemoveFromDownDevices(int cihazRecNo)//cihaz downdan up a geçerse listeden çıkartacağız
        {
            DataRow rowToDelete = null;
            foreach (DataRow row in _downCihazlarTable.Rows)
            {
                if (Convert.ToInt32(row["RecNo"]) == cihazRecNo)
                {
                    rowToDelete = row;
                    break;
                }
            }

            if (rowToDelete != null)
            {
                string ip = rowToDelete["IPNo"].ToString();

                if (_downCihazTimers.ContainsKey(cihazRecNo))
                {
                    _downCihazTimers[cihazRecNo].Stop();
                    _downCihazTimers[cihazRecNo].Dispose();
                    _downCihazTimers.Remove(cihazRecNo);
                }

                _downCihazlarTable.Rows.Remove(rowToDelete);
                LogMessage($"[{DateTime.Now:HH:mm:ss}] [{ip}] cihazı aktif duruma geçti ve down listesinden çıkarıldı.", Color.Green);
            }
        }

        private async void PingAtBtn_Click_1(object sender, EventArgs e)
        {
            _pingTimer.Start();
            LogMessage("Ping işlemi başlatılıyor...", Color.Blue);

            var tasks = new List<Task>();
            foreach (DataGridViewRow row in Cihazlar.Rows)
            {
                tasks.Add(ProcessDeviceRowAsync(row));
            }

            await Task.WhenAll(tasks);

            Invoke(new Action(() =>
            {
                Cihazlar.Refresh();
                downCihazlar.Refresh();
                HücreRenkleme.DurumRenklendir(Cihazlar);
            }));

            _pingTimer.Start();
        }
        private void LoadDeviceData()
        {
            try
            {
                DataTable dt = VeriErisim.VerileriGetir();
                Cihazlar.DataSource = dt;

                ConfigureDevicesGridView();
                LogMessage("Cihaz verileri başarıyla yüklendi.", Color.Green);
            }
            catch (Exception ex)
            {
                LogMessage($"Veriler yüklenirken hata oluştu: {ex.Message}", Color.Red);
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message);
            }
        }

        private void ConfigureDevicesGridView()
        {
            Cihazlar.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            Cihazlar.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            Cihazlar.Refresh();
            HücreRenkleme.DurumRenklendir(Cihazlar);
        }

        private void PingIptalBtn_Click_1(object sender, EventArgs e) // ping atmayı durduran buton
        {
            // Öncelikle ping timer'ın çalışıp çalışmadığını kontrol edelim
            if (_pingTimer.Enabled)
            {
                DialogResult result = MessageBox.Show(
                    "Ping atma işlemini durdurmak istiyor musunuz?",
                    "İşlem İptali",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _pingTimer.Stop();
                    LogMessage("Ping işlemi durduruldu.", Color.Blue);

                    foreach (var timer in _downCihazTimers.Values)
                    {
                        timer.Stop();
                    }

                    LogMessage("Tüm geri sayım işlemleri durduruldu.", Color.Blue);
                }
                else
                {
                    LogMessage("Ping işlemi devam ediyor.", Color.Green);
                }
            }
            else
            {
                // Ping işlemi zaten çalışmıyor, kullanıcıya bilgi verelim
                MessageBox.Show(
                    "Ping atma işlemi henüz başlatılmamış.",
                    "Bilgi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                LogMessage("Ping atma işlemi başlatılmamış.", Color.Orange);
            }
        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {
            LogMessage("Veriler yenileniyor...", Color.Blue);
            RefreshDeviceData();
            LogMessage("Veriler başarıyla yenilendi.", Color.Green);
        }

        private void RefreshDeviceData()
        {
            DataTable dt = VeriErisim.VerileriGetir();
            Cihazlar.DataSource = dt;
            ConfigureDevicesGridView();
        }

        private void LogMessage(string text, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => LogMessage(text, color)));
                return;
            }

            MesajlarRchTxt.SelectionStart = MesajlarRchTxt.TextLength;
            MesajlarRchTxt.SelectionLength = 0;
            MesajlarRchTxt.SelectionColor = color;
            MesajlarRchTxt.AppendText(text + Environment.NewLine);
            MesajlarRchTxt.SelectionColor = MesajlarRchTxt.ForeColor;
            MesajlarRchTxt.ScrollToCaret();
        }

        private void AddNotification(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => AddNotification(text)));
                return;
            }

            rchTextBildirimler.SelectionStart = rchTextBildirimler.TextLength;
            rchTextBildirimler.SelectionLength = 0;
            rchTextBildirimler.SelectionColor = Color.Orange;
            rchTextBildirimler.AppendText(text + Environment.NewLine);
            rchTextBildirimler.SelectionColor = rchTextBildirimler.ForeColor;
            rchTextBildirimler.ScrollToCaret();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            LogMessage("Uygulama başlatıldı.", Color.Blue);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            string ipNo = araTxtBox.Text;

            if (string.IsNullOrWhiteSpace(ipNo))
            {
                LogMessage("Lütfen bir IP numarası girin.", Color.Red);
                MessageBox.Show("Lütfen bir IP numarası girin.");
                return;
            }

            SearchByIpAddress(ipNo);
        }

        private void SearchByIpAddress(string ipNo)
        {
            LogMessage($"'{ipNo}' IP numarası aranıyor...", Color.Blue);

            try
            {
                DataTable dt = VeriErisim.VerileriGetir();
                string filterExpression = $"IPNo LIKE '%{ipNo}%'";
                DataRow[] filteredRows = dt.Select(filterExpression);

                DataTable filteredDataTable = dt.Clone();
                foreach (DataRow row in filteredRows)
                {
                    filteredDataTable.ImportRow(row);
                }

                Cihazlar.DataSource = filteredDataTable;
                Cihazlar.Refresh();
                HücreRenkleme.DurumRenklendir(Cihazlar);

                LogMessage($"Arama tamamlandı. {filteredDataTable.Rows.Count} sonuç bulundu.", Color.Green);
            }
            catch (Exception ex)
            {
                LogMessage($"Filtreleme işlemi sırasında hata oluştu: {ex.Message}", Color.Red);
                MessageBox.Show("Filtreleme işlemi sırasında hata oluştu: " + ex.Message);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {

            Form existingForm = Application.OpenForms.OfType<Harita>().FirstOrDefault();

            if (existingForm != null)
            {
                
                MessageBox.Show("Harita ekranı zaten açık.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (existingForm.WindowState == FormWindowState.Minimized)
                {
                    existingForm.WindowState = FormWindowState.Normal; 
                }
                existingForm.BringToFront(); 
                existingForm.Focus(); 
            }
            else
            {
               
                Form haritaForm = new Harita();
                haritaForm.Show();
            }
        }

    }
}
