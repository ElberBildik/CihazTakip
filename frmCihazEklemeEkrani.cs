using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Cihaz_Takip_Uygulaması
{
    public partial class frmCihazEklemeEkrani : Form
    {
        private int x;
        private int y;
        private int? currentRecNo;
        private bool isUpdateMode;

        public class ConnectionString
        {
            public string baglanti { get; } = "Data Source=ES-BT14\\SQLEXPRESS;Initial Catalog=CihazTakip;Integrated Security=True";
        }

        // Yeni cihaz ekleme
        public frmCihazEklemeEkrani(float xKoordinat, float yKoordinat)
        {
            InitializeComponent();
            InitializeCustomControls();
            this.x = (int)xKoordinat;
            this.y = (int)yKoordinat;
            isUpdateMode = false;
            XTxtBox.Text = x.ToString();
            YTxtBox.Text = y.ToString();
            SetupForm();
        }

        // Güncelleme
        public frmCihazEklemeEkrani(float xKoordinat, float yKoordinat, int recNo)
        {
            InitializeComponent();
            InitializeCustomControls();
            this.x = (int)xKoordinat;
            this.y = (int)yKoordinat;
            this.currentRecNo = recNo;
            isUpdateMode = true;
            XTxtBox.Text = x.ToString();
            YTxtBox.Text = y.ToString();
            SetupForm();
        }

        private void InitializeCustomControls()
        {
            // ComboBox'ları DropDownList yerine DropDown moduna ayarla
            if (cmbBoxMarka != null)
                cmbBoxMarka.DropDownStyle = ComboBoxStyle.DropDown;
        }

        // Bu fonksiyon hem seçilen değeri hem de yazılan değeri kontrol eder
        private string GetComboBoxValue(ComboBox comboBox)
        {
            // Kullanıcı kendi bir değer girmişse veya dropdown'dan seçmişse, metin değerini al
            return comboBox.Text.Trim();
        }

        private void SetupForm()
        {
            if (!Controls.ContainsKey("lblKoordinat"))
            {
                Label lblKoordinat = new Label
                {
                    Name = "lblKoordinat",
                    Location = new System.Drawing.Point(12, 9),
                    Size = new System.Drawing.Size(200, 40),
                    Text = "Koordinatlar"
                };
                this.Controls.Add(lblKoordinat);
            }

            if (isUpdateMode)
            {
                CihazEkleBtn.Visible = false;
                //btnKoordinat.Visible = true;
                btnCihazUpdate.Visible = true;
                btnCihazSil.Visible = true;
                this.Text = "Cihaz Güncelleme";
                CihazBilgileriniYükle(currentRecNo.Value);
            }
            else
            {
                CihazEkleBtn.Visible = true;
                btnKoordinat.Visible = true;
                btnCihazUpdate.Visible = false;
                btnCihazSil.Visible = false;
                this.Text = "Yeni Cihaz Ekle";
            }
        }
        private void LoadSwitchAndPanelData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(new ConnectionString().baglanti))
                {
                    conn.Open();

                    // Switch cihazlarını yükle
                    string switchQuery = @"
                SELECT RecNo, Aciklama 
                FROM dbo.Cihaz 
                WHERE GrupRecNo = 4 
                ORDER BY Aciklama";

                    using (SqlCommand cmd = new SqlCommand(switchQuery, conn))
                    {
                        DataTable switchDT = new DataTable();
                        switchDT.Columns.Add("RecNo", typeof(int));
                        switchDT.Columns.Add("Aciklama", typeof(string));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                switchDT.Rows.Add(
                                    reader["RecNo"],
                                    reader["Aciklama"]
                                );
                            }
                        }

                        cmbBoxSwitch.DataSource = switchDT;
                        cmbBoxSwitch.DisplayMember = "Aciklama";
                        cmbBoxSwitch.ValueMember = "RecNo";
                    }

                    // Enerji Panolarını yükle
                    string panoQuery = @"
                SELECT RecNo, Aciklama 
                FROM dbo.Cihaz 
                WHERE GrupRecNo = 5 
                ORDER BY Aciklama";

                    using (SqlCommand cmd = new SqlCommand(panoQuery, conn))
                    {
                        DataTable panoDT = new DataTable();
                        panoDT.Columns.Add("RecNo", typeof(int));
                        panoDT.Columns.Add("Aciklama", typeof(string));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                panoDT.Rows.Add(
                                    reader["RecNo"],
                                    reader["Aciklama"]
                                );
                            }
                        }

                        cmbBoxEnerjiPano.DataSource = panoDT;
                        cmbBoxEnerjiPano.DisplayMember = "Aciklama";
                        cmbBoxEnerjiPano.ValueMember = "RecNo";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Cihaz bilgileri yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void LoadMarkaModelData()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(new ConnectionString().baglanti))
                {
                    string query = "SELECT DISTINCT MarkaModel FROM Cihaz WHERE MarkaModel IS NOT NULL AND MarkaModel <> '' ORDER BY MarkaModel";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        dt.Columns.Add("MarkaModel", typeof(string));

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string markaModel = reader["MarkaModel"].ToString();
                                if (!string.IsNullOrWhiteSpace(markaModel))
                                {
                                    dt.Rows.Add(markaModel);
                                }
                            }
                        }

                        cmbBoxMarka.DataSource = dt;
                        cmbBoxMarka.DisplayMember = "MarkaModel";
                        cmbBoxMarka.ValueMember = "MarkaModel";

                        // ComboBox'ın DropDownStyle özelliğini DropDown olarak ayarla
                        cmbBoxMarka.DropDownStyle = ComboBoxStyle.DropDown;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Marka modeller yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CihazBilgileriniYükle(int recNo)
        {
            using (SqlConnection connection = new SqlConnection(new ConnectionString().baglanti))
            {
                string query = @"
            SELECT 
                c.*,
                cg.Kod as GrupKod,
                cg.RecNo as GrupRecNo,
                s.Aciklama as SwitchAdi,
                ep.Aciklama as EnerjiPanoAdi,
                s.PortSayisi as SwitchPortSayisi,
                s.GBICPortSayisi as SwitchGBICPortSayisi
            FROM Cihaz c
            LEFT JOIN CihazGrup cg ON c.GrupRecNo = cg.RecNo
            LEFT JOIN Cihaz s ON c.SwitchRecNo = s.RecNo
            LEFT JOIN Cihaz ep ON c.EnerjiPanoNo = ep.RecNo
            WHERE c.RecNo = @RecNo";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RecNo", recNo);
                    try
                    {
                        connection.Open();

                        // Önce tüm ComboBox'ları temizle
                        CmbKod.SelectedIndex = -1;
                        cmbBoxSwitch.SelectedIndex = -1;
                        cmbBoxEnerjiPano.SelectedIndex = -1;
                        cmbBoxMarka.SelectedIndex = -1;
                        cmbBosPort.Items.Clear();

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Temel bilgileri doldur
                                IPTxtBox.Text = reader["IPNo"]?.ToString();
                                aciklamaTxtBox.Text = reader["Aciklama"]?.ToString();
                                EnerjiPanoSigortaNoTxtBox.Text = reader["EnerjiPanoSigortaNo"]?.ToString();

                                // Port sayıları
                                if (reader["PortSayisi"] != DBNull.Value)
                                    txtPortSayisi.Text = reader["PortSayisi"].ToString();

                                if (reader["GBICPortSayisi"] != DBNull.Value)
                                    txtGBICPortSayisiniGir.Text = reader["GBICPortSayisi"].ToString();

                                // Grup Kodunu ayarla
                                string grupKod = reader["GrupKod"]?.ToString();
                                int grupRecNo = Convert.ToInt32(reader["GrupRecNo"]);

                                foreach (DataRowView item in CmbKod.Items)
                                {
                                    if (Convert.ToInt32(item["RecNo"]) == grupRecNo)
                                    {
                                        CmbKod.SelectedItem = item;
                                        break;
                                    }
                                }

                                // Switch seçimini ayarla
                                if (reader["SwitchRecNo"] != DBNull.Value)
                                {
                                    int switchRecNo = Convert.ToInt32(reader["SwitchRecNo"]);
                                    foreach (DataRowView item in cmbBoxSwitch.Items)
                                    {
                                        if (Convert.ToInt32(item["RecNo"]) == switchRecNo)
                                        {
                                            cmbBoxSwitch.SelectedItem = item;
                                            break;
                                        }
                                    }
                                }

                                // Switch Port No'yu ayarla
                                if (reader["SwitchPortNo"] != DBNull.Value)
                                {
                                    string portNo = reader["SwitchPortNo"].ToString();
                                    cmbBosPort.Items.Add(portNo); // Mevcut port numarasını ekle
                                    cmbBosPort.SelectedItem = portNo; // Seç
                                }

                                // Enerji Panosu seçimini ayarla
                                if (reader["EnerjiPanoNo"] != DBNull.Value)
                                {
                                    int enerjiPanoNo = Convert.ToInt32(reader["EnerjiPanoNo"]);
                                    foreach (DataRowView item in cmbBoxEnerjiPano.Items)
                                    {
                                        if (Convert.ToInt32(item["RecNo"]) == enerjiPanoNo)
                                        {
                                            cmbBoxEnerjiPano.SelectedItem = item;
                                            break;
                                        }
                                    }
                                }

                                // MarkaModel için sadece Text özelliğini kullan
                                string markaModel = reader["MarkaModel"]?.ToString();
                                if (!string.IsNullOrWhiteSpace(markaModel))
                                {
                                    cmbBoxMarka.Text = markaModel;
                                }

                                // Grup koduna göre kontrollerin görünürlüğünü ayarla
                                bool showPortSayisiControls = grupKod == "Data Switch" || grupKod == "Kamera Switch";

                                // Port sayısı kontrollerinin görünürlüğünü ayarla
                                lblPortSayisi.Visible = showPortSayisiControls;
                                txtPortSayisi.Visible = showPortSayisiControls;
                                txtGBICPortSayisiniGir.Visible = showPortSayisiControls;
                                lblGBICPortSayisiniGiriniz.Visible = showPortSayisiControls;

                                // Switch kontrollerinin görünürlüğünü ayarla
                                SetSwitchControlsVisibility(showPortSayisiControls);

                                // Boş portları güncelle
                                if (cmbBoxSwitch.SelectedItem != null)
                                {
                                    UpdateBosPortlarForSelectedSwitch();
                                }

                                // X ve Y koordinatlarını ayarla
                                if (reader["X"] != DBNull.Value)
                                {
                                    XTxtBox.Text = reader["X"].ToString();
                                }
                                if (reader["Y"] != DBNull.Value)
                                {
                                    YTxtBox.Text = reader["Y"].ToString();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Cihaz bilgileri yüklenirken hata: {ex.Message}", "Hata",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Switch kontrollerinin görünürlüğünü ayarlayan yardımcı metod
        private void SetSwitchControlsVisibility(bool visible)
        {
            // Port sayısı kontrolleri için görünürlük
            if (lblPortSayisi != null) lblPortSayisi.Visible = visible;
            if (txtPortSayisi != null) txtPortSayisi.Visible = visible;

            // Boş port kontrolleri her zaman görünür kalmalı
            if (lblBosPort != null) lblBosPort.Visible = true;
            if (cmbBosPort != null) cmbBosPort.Visible = true;
        }

        // Boş portları güncelleyen metod
        private void UpdateBosPortlar()
        {
            if (cmbBosPort == null) return;

            cmbBosPort.Items.Clear();
            if (int.TryParse(txtPortSayisi.Text, out int portSayisi) && portSayisi > 0)
            {
                using (SqlConnection conn = new SqlConnection(new ConnectionString().baglanti))
                {
                    try
                    {
                        conn.Open();
                        string query = @"
                    SELECT SwitchPortNo 
                    FROM Cihaz 
                    WHERE SwitchRecNo = @SwitchRecNo 
                    AND SwitchPortNo IS NOT NULL";

                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@SwitchRecNo", currentRecNo ?? 0);

                            HashSet<string> kullanilanPortlar = new HashSet<string>();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    kullanilanPortlar.Add(reader["SwitchPortNo"].ToString());
                                }
                            }

                            // Boş portları ekle
                            for (int i = 1; i <= portSayisi; i++)
                            {
                                if (!kullanilanPortlar.Contains(i.ToString()))
                                {
                                    cmbBosPort.Items.Add(i);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Boş portlar yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CihazEklemeEkrani_Load(object sender, EventArgs e)
        {
            // TODO: Bu kod satırı 'enerjiPanoDataSet.Cihaz' tablosuna veri yükler. Bunu gerektiği şekilde taşıyabilir, veya kaldırabilirsiniz.
            this.cihazTableAdapter1.Fill(this.enerjiPanoDataSet.Cihaz);
            // TODO: Bu kod satırı 'switchleriGetir.CihazGrup' tablosuna veri yükler. Bunu gerektiği şekilde taşıyabilir, veya kaldırabilirsiniz.
            this.cihazGrupTableAdapter1.Fill(this.switchleriGetir.CihazGrup);
            try
            {
                LoadSwitchAndPanelData();
                this.cihazTableAdapter.Fill(this.cihazTakipDataSet1.Cihaz);
                LoadMarkaModelData();
                this.cihazGrupTableAdapter.Fill(this.kodlariGetir1.CihazGrup);
                CmbKod.DisplayMember = "Kod";
                if (CmbKod.DataSource == null)
                {
                    CmbKod.DataSource = this.kodlariGetir1.CihazGrup;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Veri yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Event handler'ları bağla
            CmbKod.SelectedIndexChanged += CmbKod_SelectedIndexChanged;
            cmbBoxSwitch.SelectedIndexChanged += cmbBoxSwitch_SelectedIndexChanged;
            txtPortSayisi.TextChanged += (s, evt) => UpdateBosPortlar();

            // Sadece sayı girişi için event handler
            txtPortSayisi.KeyPress += (s, evt) =>
            {
                if (!char.IsControl(evt.KeyChar) && !char.IsDigit(evt.KeyChar))
                {
                    evt.Handled = true;
                }
            };

            // Port sayısı kontrolleri başlangıçta gizli
            lblPortSayisi.Visible = false;
            txtPortSayisi.Visible = false;


            lblGBICPortSayisiniGiriniz.Visible = false;
            txtGBICPortSayisiniGir.Visible = false;

            // Boş port kontrolleri her zaman görünür
            lblBosPort.Visible = true;
            cmbBosPort.Visible = true;
        }

        private int CihazınRecNosunuGetir()
        {
            if (CmbKod.SelectedItem == null)
            {
                MessageBox.Show("Lütfen geçerli bir Grup seçiniz.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return 0;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(new ConnectionString().baglanti))
                {
                    connection.Open();
                    DataRowView selectedRow = (DataRowView)CmbKod.SelectedItem;
                    string selectedKod = selectedRow["Kod"].ToString();
                    string query = "SELECT RecNo FROM CihazGrup WHERE Kod = @Kod";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Kod", selectedKod);
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            return Convert.ToInt32(result);
                        }
                        else
                        {
                            MessageBox.Show("Seçilen grup için RecNo bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Grup RecNo alınırken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }
        private void btnCihazEkle_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(aciklamaTxtBox.Text) || CmbKod.SelectedItem == null)
            {
                MessageBox.Show("Lütfen gerekli alanları doldurunuz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string aciklama = aciklamaTxtBox.Text;
            string ipNo = IPTxtBox.Text;
            string markaModel = GetComboBoxValue(cmbBoxMarka);

            // Switch RecNo'yu ComboBox'tan al
            int? switchRecNo = null;
            if (cmbBoxSwitch.SelectedValue != null)
            {
                switchRecNo = Convert.ToInt32(cmbBoxSwitch.SelectedValue);
            }

            // Switch port numarasını al
            string switchPortNo = null;
            if (cmbBosPort.SelectedItem != null)
            {
                switchPortNo = cmbBosPort.SelectedItem.ToString(); // Direkt formatlanmış değeri al
            }

            // Enerji Pano RecNo'yu ComboBox'tan al
            int? enerjiPanoRecNo = null;
            if (cmbBoxEnerjiPano.SelectedValue != null)
            {
                enerjiPanoRecNo = Convert.ToInt32(cmbBoxEnerjiPano.SelectedValue);
            }

            string enerjiPanoSigortaNo = EnerjiPanoSigortaNoTxtBox.Text;

            // Port sayılarını al
            int? portSayisi = null;
            if (!string.IsNullOrWhiteSpace(txtPortSayisi.Text))
            {
                portSayisi = Convert.ToInt32(txtPortSayisi.Text);
            }

            int? gbicPortSayisi = null;
            if (!string.IsNullOrWhiteSpace(txtGBICPortSayisiniGir.Text))
            {
                gbicPortSayisi = Convert.ToInt32(txtGBICPortSayisiniGir.Text);
            }

            int grupRecNo = CihazınRecNosunuGetir();
            if (grupRecNo == 0)
            {
                MessageBox.Show("Geçerli bir grup seçmelisiniz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection con = new SqlConnection(new ConnectionString().baglanti))
            {
                try
                {
                    con.Open();

                    string query = @"
                INSERT INTO Cihaz (
                    Aciklama, IPNo, SwitchRecNo, SwitchPortNo, 
                    MarkaModel, EnerjiPanoNo, EnerjiPanoSigortaNo, 
                    GrupRecNo, X, Y, PortSayisi, GBICPortSayisi
                )
                VALUES (
                    @Aciklama, @IPNo, @SwitchRecNo, @SwitchPortNo,
                    @MarkaModel, @EnerjiPanoNo, @EnerjiPanoSigortaNo,
                    @GrupRecNo, @X, @Y, @PortSayisi, @GBICPortSayisi
                )";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Aciklama", aciklama);
                        cmd.Parameters.AddWithValue("@IPNo", !string.IsNullOrWhiteSpace(ipNo) ? ipNo : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@SwitchRecNo", (object)switchRecNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SwitchPortNo", (object)switchPortNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@MarkaModel", !string.IsNullOrWhiteSpace(markaModel) ? markaModel : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@EnerjiPanoNo", (object)enerjiPanoRecNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@EnerjiPanoSigortaNo", !string.IsNullOrWhiteSpace(enerjiPanoSigortaNo) ? enerjiPanoSigortaNo : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@GrupRecNo", grupRecNo);
                        cmd.Parameters.AddWithValue("@X", x);
                        cmd.Parameters.AddWithValue("@Y", y);
                        cmd.Parameters.AddWithValue("@PortSayisi", (object)portSayisi ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@GBICPortSayisi", (object)gbicPortSayisi ?? DBNull.Value);

                        int affected = cmd.ExecuteNonQuery();
                        if (affected > 0)
                        {
                            MessageBox.Show("Cihaz başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Ekleme yapılamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ekleme sırasında hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnCihazGuncelle_Click(object sender, EventArgs e)
        {
            if (!currentRecNo.HasValue || string.IsNullOrWhiteSpace(aciklamaTxtBox.Text))
            {
                MessageBox.Show("Lütfen gerekli alanları doldurunuz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string aciklama = aciklamaTxtBox.Text;
            string ipNo = IPTxtBox.Text;
            string switchPortNo = GetComboBoxValue(cmbBosPort);
            string markaModel = GetComboBoxValue(cmbBoxMarka);

            // Switch RecNo'yu ComboBox'tan al
            int? switchRecNo = null;
            if (cmbBoxSwitch.SelectedItem != null)
            {
                DataRowView dr = cmbBoxSwitch.SelectedItem as DataRowView;
                if (dr != null)
                {
                    switchRecNo = Convert.ToInt32(dr["RecNo"]);
                }
            }
            else if (!string.IsNullOrWhiteSpace(cmbBoxSwitch.Text))
            {
                // Kullanıcı manuel bir değer girdiyse, önce switch kaydı yapılmasını iste
                MessageBox.Show(
                    "Girdiğiniz switch sistemde kayıtlı değil. Lütfen önce yeni switch kaydı yapınız.",
                    "Switch Bulunamadı",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return; // İşlemi sonlandır
            }

            // Enerji Pano RecNo'yu ComboBox'tan al
            int? enerjiPanoRecNo = null;
            if (cmbBoxEnerjiPano.SelectedItem != null)
            {
                DataRowView dr = cmbBoxEnerjiPano.SelectedItem as DataRowView;
                if (dr != null)
                {
                    enerjiPanoRecNo = Convert.ToInt32(dr["RecNo"]);
                }
            }

            string enerjiPanoSigortaNo = EnerjiPanoSigortaNoTxtBox.Text;

            // X ve Y değerlerini TextBox'lardan al
            int xKoordinat = int.Parse(XTxtBox.Text);
            int yKoordinat = int.Parse(YTxtBox.Text);

            using (SqlConnection con = new SqlConnection(new ConnectionString().baglanti))
            {
                string query = @"
            UPDATE Cihaz SET
                Aciklama = @Aciklama,
                IPNo = @IPNo,
                SwitchRecNo = @SwitchRecNo,
                SwitchPortNo = @SwitchPortNo,
                MarkaModel = @MarkaModel,
                EnerjiPanoNo = @EnerjiPanoNo,
                EnerjiPanoSigortaNo = @EnerjiPanoSigortaNo,
                X = @X,
                Y = @Y
            WHERE RecNo = @RecNo";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@RecNo", currentRecNo.Value);
                    cmd.Parameters.AddWithValue("@Aciklama", aciklama);
                    cmd.Parameters.AddWithValue("@IPNo", !string.IsNullOrWhiteSpace(ipNo) ? ipNo : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@SwitchRecNo", (object)switchRecNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SwitchPortNo", !string.IsNullOrWhiteSpace(switchPortNo) ? switchPortNo : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@MarkaModel", !string.IsNullOrWhiteSpace(markaModel) ? markaModel : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@EnerjiPanoNo", (object)enerjiPanoRecNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EnerjiPanoSigortaNo", !string.IsNullOrWhiteSpace(enerjiPanoSigortaNo) ? enerjiPanoSigortaNo : (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@X", xKoordinat);
                    cmd.Parameters.AddWithValue("@Y", yKoordinat);

                    try
                    {
                        con.Open();
                        int affected = cmd.ExecuteNonQuery();
                        if (affected > 0)
                        {
                            MessageBox.Show("Cihaz başarıyla güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Güncelleme yapılamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Güncelleme sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnCihazSil_Click(object sender, EventArgs e)
        {
            if (!currentRecNo.HasValue)
            {
                MessageBox.Show("Silinecek cihaz bilgisi bulunamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DialogResult result = MessageBox.Show("Bu cihazı silmek istediğinizden emin misiniz?", "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                using (SqlConnection con = new SqlConnection(new ConnectionString().baglanti))
                {
                    string query = "DELETE FROM Cihaz WHERE RecNo = @RecNo";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@RecNo", currentRecNo.Value);
                        try
                        {
                            con.Open();
                            int affected = cmd.ExecuteNonQuery();
                            if (affected > 0)
                            {
                                MessageBox.Show("Cihaz başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                this.DialogResult = DialogResult.OK;
                                this.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Silme sırasında hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }
        private void btnKoordinat_Click(object sender, EventArgs e)
        {
            if (KoordinatHelper.KoordinatlarKopyalandi)
            {
                // Kopyalanan koordinatları form değişkenlerine ata
                this.x = (int)KoordinatHelper.KopyalananX.Value;
                this.y = (int)KoordinatHelper.KopyalananY.Value;

                // TextBox'lara değerleri yaz
                XTxtBox.Text = x.ToString();
                YTxtBox.Text = y.ToString();

                MessageBox.Show("Koordinatlar yapıştırıldı!", "Bilgi",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Yapıştırılacak koordinat bulunamadı!\nÖnce bir koordinat kopyalamalısınız.",
                    "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void switchleriGetirToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.cihazGrupTableAdapter1.switchleriGetir(this.switchleriGetir.CihazGrup);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void enerjiPanoGetirToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.cihazTableAdapter1.enerjiPanoGetir(this.enerjiPanoDataSet.Cihaz);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }


        private void switchleriGetirToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void enerjiPanoGetir1ToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                this.cihazTableAdapter1.enerjiPanoGetir(this.enerjiPanoDataSet.Cihaz);
            }
            catch (System.Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

        }

        private void enerjiPanoGetir1ToolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }
        // CmbKod seçimi değiştiğinde sadece port sayısı kontrollerini yöneten event handler
        private void CmbKod_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (CmbKod.SelectedItem != null)
            {
                DataRowView drv = CmbKod.SelectedItem as DataRowView;
                if (drv != null)
                {
                    string selectedKod = drv["Kod"].ToString();

                    // Sadece Data Switch veya Kamera Switch için port sayısı kontrollerini göster
                    bool showPortSayisiControls = selectedKod == "Data Switch" || selectedKod == "Kamera Switch";

                    // Port sayısı kontrollerinin görünürlüğünü ayarla
                    lblPortSayisi.Visible = showPortSayisiControls;
                    txtPortSayisi.Visible = showPortSayisiControls;
                    lblGBICPortSayisiniGiriniz.Visible = showPortSayisiControls;
                    txtGBICPortSayisiniGir.Visible = showPortSayisiControls;

                    if (!showPortSayisiControls)
                    {
                        txtPortSayisi.Clear();
                        txtGBICPortSayisiniGir.Clear();
                    }

                    // Boş port kontrolleri her zaman görünür kalmalı
                    lblBosPort.Visible = true;
                    cmbBosPort.Visible = true;
                }
            }
        }
        // Switch seçimi değiştiğinde boş portları güncelleyen event handler
        private void cmbBoxSwitch_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Boş portları her zaman güncelle
            UpdateBosPortlarForSelectedSwitch();

            // cmbBosPort'u her zaman görünür yap
            cmbBosPort.Visible = true;
            lblBosPort.Visible = true;
        }
        // Seçili switch için boş portları güncelleyen metod
        private void UpdateBosPortlarForSelectedSwitch()
        {
            if (cmbBoxSwitch.SelectedItem == null)
            {
                cmbBosPort.Items.Clear();
                return;
            }

            cmbBosPort.Items.Clear();

            DataRowView dr = cmbBoxSwitch.SelectedItem as DataRowView;
            if (dr != null)
            {
                int switchRecNo = Convert.ToInt32(dr["RecNo"]);

                using (SqlConnection conn = new SqlConnection(new ConnectionString().baglanti))
                {
                    try
                    {
                        conn.Open();

                        // Seçili switch'in port sayılarını al
                        string portSayisiQuery = "SELECT PortSayisi, GBICPortSayisi FROM Cihaz WHERE RecNo = @SwitchRecNo";
                        int normalPorts = 0;
                        int gbicPorts = 0;

                        using (SqlCommand portCmd = new SqlCommand(portSayisiQuery, conn))
                        {
                            portCmd.Parameters.AddWithValue("@SwitchRecNo", switchRecNo);
                            using (SqlDataReader reader = portCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    normalPorts = reader["PortSayisi"] != DBNull.Value ? Convert.ToInt32(reader["PortSayisi"]) : 0;
                                    gbicPorts = reader["GBICPortSayisi"] != DBNull.Value ? Convert.ToInt32(reader["GBICPortSayisi"]) : 0;
                                }
                            }
                        }

                        // Kullanılan portları al
                        string usedPortsQuery = @"
                SELECT SwitchPortNo
                FROM Cihaz 
                WHERE SwitchRecNo = @SwitchRecNo 
                AND SwitchPortNo IS NOT NULL";

                        HashSet<string> kullanilanPortlar = new HashSet<string>();
                        using (SqlCommand cmd = new SqlCommand(usedPortsQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@SwitchRecNo", switchRecNo);
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    kullanilanPortlar.Add(reader["SwitchPortNo"].ToString());
                                }
                            }
                        }

                        // Normal portları ekle
                        for (int i = 1; i <= normalPorts; i++)
                        {
                            string portNo = $"D{i}";
                            if (!kullanilanPortlar.Contains(portNo))
                            {
                                cmbBosPort.Items.Add(portNo);
                            }
                        }

                        // GBIC portlarını, normal port numaralarının üstüne ekle
                        for (int i = 1; i <= gbicPorts; i++)
                        {
                            string portNo = $"G{normalPorts + i}";
                            if (!kullanilanPortlar.Contains(portNo))
                            {
                                cmbBosPort.Items.Add(portNo);
                            }
                        }

                        // Portları sıralı bir şekilde görüntüle
                        cmbBosPort.Sorted = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Boş portlar yüklenirken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
