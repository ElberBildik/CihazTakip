using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Cihaz_Takip_Uygulaması
{
    public partial class CihazEklemeEkrani : Form
    {
        private int x;
        private int y;

        // 1. Ekleyin - ConnectionString sınıfını burada tanımlayın
        public class ConnectionString
        {
            public string baglanti { get; } = "Data Source=ES-BT14\\SQLEXPRESS;Initial Catalog=CihazTakip;Integrated Security=True";
        }

        public CihazEklemeEkrani(float xKoordinat, float yKoordinat)
        {
            InitializeComponent();
            this.x = (int)xKoordinat;
            this.y = (int)yKoordinat;
            XTxtBox.Text = x.ToString();
            YTxtBox.Text = y.ToString();

            // 2. Ekleyin - lblKoordinat kontrolünü oluşturun (eğer form tasarımında yoksa)
            if (!Controls.ContainsKey("lblKoordinat"))
            {
                Label lblKoordinat = new Label();
                lblKoordinat.Name = "lblKoordinat";
                lblKoordinat.Location = new System.Drawing.Point(12, 9);
                lblKoordinat.Size = new System.Drawing.Size(200, 40);
                lblKoordinat.Text = "Koordinatlar";
                this.Controls.Add(lblKoordinat);
            }
        }

        private void CihazEklemeEkrani_Load(object sender, EventArgs e)
        {
            this.cihazGrupTableAdapter.Fill(this.kodlariGetir.CihazGrup);
            


        }

        private void CihazEkleBtn_Click(object sender, EventArgs e)
        {
            // Formdan diğer bilgileri al
            string aciklama = aciklamaTxtBox.Text;
            string ipNo = IPTxtBox.Text;

            // 3. Değişiklik - nullable referans tipi hatası giderildi
            int? switchRecNo = null;
            if (!string.IsNullOrWhiteSpace(SwitchRecNoTxtBox.Text))
            {
                switchRecNo = Convert.ToInt32(SwitchRecNoTxtBox.Text);
            }

            string durum = "NULL";
            string switchPortNo = SwitchPortNoTxtBox.Text;
            string markaModel = MarkaTxtBox.Text;
            string enerjiPanoNo = EnerjiPanoNoTxtBox.Text;
            string enerjiPanoSigortaNo = EnerjiPanoSigortaNoTxtBox.Text;

            // 1. Adım - ComboBox'tan 'Kod' alınarak, ilgili 'RecNo''yu alalım
            int grupRecNo = 0;
            var selectedItem = CmbKod.SelectedItem as DataRowView;  // ComboBox'tan seçilen item'ı DataRowView olarak alıyoruz.

            if (selectedItem != null)
            {
                // DataRowView'den Kod değerini alıyoruz
                string selectedKod = selectedItem["Kod"].ToString();  // Kod kolonunu kullanıyoruz.

                // CihazGrup tablosundan ilgili RecNo'yu almak için SQL sorgusu yazalım
                ConnectionString baglanti = new ConnectionString();
                using (SqlConnection con = new SqlConnection(baglanti.baglanti))
                {
                    string query = @"SELECT RecNo FROM CihazGrup WHERE Kod = @Kod";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Kod", selectedKod);

                    try
                    {
                        con.Open();
                        var result = cmd.ExecuteScalar(); // İlk satırı döndürür
                        if (result != null)
                        {
                            grupRecNo = Convert.ToInt32(result); // RecNo'yu alıyoruz
                        }
                        else
                        {
                            MessageBox.Show("CihazGrup'ta belirtilen Kod bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Veritabanı hatası: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            else
            {
                MessageBox.Show("Lütfen geçerli bir Grup seçiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Veritabanı bağlantısı
            ConnectionString baglanti2 = new ConnectionString();
            using (SqlConnection con = new SqlConnection(baglanti2.baglanti))
            {
                string query = @"INSERT INTO Cihaz 
        (GrupRecNo, Aciklama, IPNo, X, Y, SwitchRecNo, Durum, SwitchPortNo, MarkaModel, EnerjiPanoNo, EnerjiPanoSigortaNo)
        VALUES 
        (@GrupRecNo, @Aciklama, @IPNo, @X, @Y, @SwitchRecNo, @Durum, @SwitchPortNo, @MarkaModel, @EnerjiPanoNo, @EnerjiPanoSigortaNo)";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@GrupRecNo", grupRecNo);
                cmd.Parameters.AddWithValue("@Aciklama", aciklama);
                cmd.Parameters.AddWithValue("@IPNo", ipNo);
                cmd.Parameters.AddWithValue("@X", x);
                cmd.Parameters.AddWithValue("@Y", y);

                // 4. Değişiklik - Nullable parametre sorunu düzeltildi
                cmd.Parameters.AddWithValue("@SwitchRecNo", switchRecNo.HasValue ? (object)switchRecNo.Value : DBNull.Value);

                cmd.Parameters.AddWithValue("@Durum", durum);
                cmd.Parameters.AddWithValue("@SwitchPortNo", switchPortNo);
                cmd.Parameters.AddWithValue("@MarkaModel", markaModel);
                cmd.Parameters.AddWithValue("@EnerjiPanoNo", enerjiPanoNo);
                cmd.Parameters.AddWithValue("@EnerjiPanoSigortaNo", enerjiPanoSigortaNo);

                try
                {
                    con.Open();
                    int rows = cmd.ExecuteNonQuery();
                    if (rows > 0)
                    {
                        MessageBox.Show("Cihaz başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Cihaz eklenemedi!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


    }
}