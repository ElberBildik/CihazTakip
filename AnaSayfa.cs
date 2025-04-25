using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Cihaz_Takip_Uygulaması
{
    public partial class AnaSayfa : Form
    {
        // Gerekli yöneticileri tanımlar (UI, cihaz durumu, ping işlemleri vs.)
        private readonly clsGridveTextIslemleri GorselIslemler;
        private readonly clsCihazDurumuTakip cihazDurumu;
        private readonly clsPingAt pingAt;
        private readonly clsCihazIzle cihazIzle;
        private readonly DataTable downCihazlarınTablosu; // Down (erişilemeyen) cihazları tutan tablo

        public AnaSayfa()
        {
            InitializeComponent(); // Form bileşenlerini başlatır

            // Down cihazlar için tabloyu oluşturur
            downCihazlarınTablosu = new DataTable();

            // UIManager sınıfı, kullanıcı arayüzü ile ilgili işlemleri yönetir
            GorselIslemler = new clsGridveTextIslemleri(
                Cihazlar,
                downCihazlar,
                rchTextMesajlar,
                rchTextBildirimler);

            // Down cihazlar için DataGridView’i yapılandırır
            GorselIslemler.CihazlariGrideEkle(downCihazlarınTablosu);

            // Cihaz durumlarını kontrol eden ve yöneten sınıf
            cihazDurumu = new clsCihazDurumuTakip(
                downCihazlarınTablosu,
                Cihazlar,
                downCihazlar,
                GorselIslemler.LogEkle,
                GorselIslemler.BildirimEkle);

            // Ping atmakla sorumlu sınıf
            pingAt = new clsPingAt();

            // Cihazları izleyen ve ping atma işlemini yöneten sınıf
            cihazIzle = new clsCihazIzle(
                pingAt,
                cihazDurumu,
                GorselIslemler,
                Cihazlar);

            // Cihaz verilerini yükleyip arayüzde gösterir
            GorselIslemler.VerileriGrideYükle();
        }

        // Form yüklendiğinde çalışan olay
        private void Form1_Load(object sender, EventArgs e)
        {
            GorselIslemler.LogEkle("Uygulama başlatıldı.", Color.Blue); // Log mesajı göster
        }

        // Ping At butonuna basıldığında çalışan olay
        private async void BtnPingAt_Click(object sender, EventArgs e)
        {
            cihazIzle.Start(); // Cihaz izlemeyi başlatır
            await cihazIzle.ScanAllDevicesAsync(); // Tüm cihazlara ping atar (asenkron)
        }

        // Ping İptal butonuna basıldığında çalışan olay
        private void BtnPingIptal_Click(object sender, EventArgs e)
        {
            // Kullanıcıya onay penceresi gösterilir
            DialogResult result = MessageBox.Show(
                "Ping atma işlemini durdurmak istiyor musunuz?",
                "İşlem İptali",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                cihazIzle.Stop(); // Cihaz izleme işlemini durdurur
            }
            else
            {
                GorselIslemler.LogEkle("Ping işlemi devam ediyor.", Color.Green); // İşlemin devam ettiğini bildirir
            }
        }

        // Yenile butonu 
        private void BtnYenile_Click(object sender, EventArgs e)
        {
            GorselIslemler.LogEkle("Veriler yenileniyor...", Color.Blue); // Log mesajı
            GorselIslemler.CihazlarGridiniYenile(); // Cihaz verilerini yeniler
            GorselIslemler.LogEkle("Veriler başarıyla yenilendi.", Color.Green); // Başarılı mesaj
        }

        // IP ile cihaz arama kutusuna basıldığında çalışan olay
        private void BtnCihazAra_Click(object sender, EventArgs e)
        {
            string ipNo = TxtBoxAra.Text; // Arama kutusundaki IP alınır
            GorselIslemler.IpyeGöreCihazAra(ipNo); // IP adresine göre cihaz aranır
        }

        // Harita butonuna tıklandığında çalışan olay
        private void BtnHaritaOpen_Click(object sender, EventArgs e)
        {
            // Daha önce açılmış bir Harita formu var mı diye kontrol edilir
            Form existingForm = Application.OpenForms.OfType<Harita>().FirstOrDefault();

            if (existingForm != null)
            {
                // Eğer zaten açıksa kullanıcıya bilgi verilir
                MessageBox.Show("Harita ekranı zaten açık.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Eğer form minimize durumdaysa normal boyuta getirilir
                if (existingForm.WindowState == FormWindowState.Minimized)
                {
                    existingForm.WindowState = FormWindowState.Normal;
                }

                // Ön plana getirilir ve odaklanılır
                existingForm.BringToFront();
                existingForm.Focus();
            }
            else
            {
                // Eğer açık değilse yeni bir Harita formu oluşturulur ve gösterilir
                Form haritaForm = new Harita();
                haritaForm.Show();
            }
        }
    }
}
