using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

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
            // Bu, hem seçim hem de elle yazma yapılabilmesini sağlar
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
                ep.Aciklama as EnerjiPanoAdi
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

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Temel bilgileri doldur
                        IPTxtBox.Text = reader["IPNo"]?.ToString();
                        aciklamaTxtBox.Text = reader["Aciklama"]?.ToString();
                        SwitchPortNoTxtBox.Text = reader["SwitchPortNo"]?.ToString();
                        EnerjiPanoNoTxtBox.Text = reader["EnerjiPanoNo"]?.ToString();
                        EnerjiPanoSigortaNoTxtBox.Text = reader["EnerjiPanoSigortaNo"]?.ToString();

                        // Grup Kodunu ayarla
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

        // Switch cihazının açıklamasını RecNo'ya göre getiren yeni metod
        private string GetSwitchAciklamaFromRecNo(int switchRecNo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(new ConnectionString().baglanti))
                {
                    connection.Open();
                    string query = "SELECT Aciklama FROM Cihaz WHERE RecNo = @RecNo AND GrupRecNo IN (SELECT RecNo FROM CihazGrup WHERE Kod = 'SWITCH')";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@RecNo", switchRecNo);
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            return result.ToString();
                        }

                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Switch açıklama bilgisi alınırken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        // Switch cihazının RecNo'sunu açıklamaya göre getiren yeni metod
        private int? GetSwitchRecNoFromDescription(string switchDescription)
        {
            if (string.IsNullOrWhiteSpace(switchDescription))
                return null;

            try
            {
                using (SqlConnection connection = new SqlConnection(new ConnectionString().baglanti))
                {
                    connection.Open();
                    string query = "SELECT RecNo FROM Cihaz WHERE Aciklama = @SwitchDescription AND GrupRecNo IN (SELECT RecNo FROM CihazGrup WHERE Kod = 'SWITCH')";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@SwitchDescription", switchDescription);
                        object result = cmd.ExecuteScalar();

                        if (result != null && result != DBNull.Value)
                        {
                            return Convert.ToInt32(result);
                        }

                        // If no match found by description, try direct numeric conversion as fallback
                        if (int.TryParse(switchDescription, out int directRecNo))
                        {
                            // Verify if this RecNo exists and is a switch
                            string verifyQuery = "SELECT COUNT(*) FROM Cihaz WHERE RecNo = @RecNo AND GrupRecNo IN (SELECT RecNo FROM CihazGrup WHERE Kod = 'SWITCH')";
                            using (SqlCommand verifyCmd = new SqlCommand(verifyQuery, connection))
                            {
                                verifyCmd.Parameters.AddWithValue("@RecNo", directRecNo);
                                int count = (int)verifyCmd.ExecuteScalar();

                                if (count > 0)
                                {
                                    return directRecNo;
                                }
                            }
                        }

                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Switch RecNo alınırken hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void SelectComboBoxItem(string grupKod)
        {
            for (int i = 0; i < CmbKod.Items.Count; i++)
            {
                var item = CmbKod.Items[i] as DataRowView;
                if (item != null && item["Kod"].ToString() == grupKod)
                {
                    CmbKod.SelectedIndex = i;
                    break;
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
            string switchPortNo = SwitchPortNoTxtBox.Text;
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
            if (cmbBoxEnerjiPano.SelectedValue != null)
            {
                enerjiPanoRecNo = Convert.ToInt32(cmbBoxEnerjiPano.SelectedValue); 
            }

            string enerjiPanoSigortaNo = EnerjiPanoSigortaNoTxtBox.Text;

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
                    GrupRecNo, X, Y
                )
                VALUES (
                    @Aciklama, @IPNo, @SwitchRecNo, @SwitchPortNo,
                    @MarkaModel, @EnerjiPanoNo, @EnerjiPanoSigortaNo,
                    @GrupRecNo, @X, @Y
                )";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@Aciklama", aciklama);
                        cmd.Parameters.AddWithValue("@IPNo", ipNo);
                        cmd.Parameters.AddWithValue("@SwitchRecNo", (object)switchRecNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@SwitchPortNo", switchPortNo);
                        cmd.Parameters.AddWithValue("@MarkaModel", markaModel);
                        cmd.Parameters.AddWithValue("@EnerjiPanoNo", (object)enerjiPanoRecNo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@EnerjiPanoSigortaNo", enerjiPanoSigortaNo);
                        cmd.Parameters.AddWithValue("@GrupRecNo", grupRecNo);
                        cmd.Parameters.AddWithValue("@X", x);
                        cmd.Parameters.AddWithValue("@Y", y);

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
        private void btnCihazUpdate_Click(object sender, EventArgs e)
        {
            if (!currentRecNo.HasValue || string.IsNullOrWhiteSpace(aciklamaTxtBox.Text))
            {
                MessageBox.Show("Lütfen gerekli alanları doldurunuz.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string aciklama = aciklamaTxtBox.Text;
            string ipNo = IPTxtBox.Text;
            string switchPortNo = SwitchPortNoTxtBox.Text;

            // Güncelleme için combo box değerini al - seçilmiş veya yazılmış olabilir
            string markaModel = GetComboBoxValue(cmbBoxMarka);

            string enerjiPanoNo = EnerjiPanoNoTxtBox.Text;
            string enerjiPanoSigortaNo = EnerjiPanoSigortaNoTxtBox.Text;

            // Switch RecNo'yu açıklamadan veya direct değerden al
            int? switchRecNo = GetSwitchRecNoFromDescription(SwitchRecNoTxtBox.Text);

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
            X = @X,                    -- X koordinatı eklendi
            Y = @Y                     -- Y koordinatı eklendi
        WHERE RecNo = @RecNo";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@RecNo", currentRecNo.Value);
                    cmd.Parameters.AddWithValue("@Aciklama", aciklama);
                    cmd.Parameters.AddWithValue("@IPNo", ipNo);
                    cmd.Parameters.AddWithValue("@SwitchRecNo", (object)switchRecNo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SwitchPortNo", switchPortNo);
                    cmd.Parameters.AddWithValue("@MarkaModel", markaModel);
                    cmd.Parameters.AddWithValue("@EnerjiPanoNo", enerjiPanoNo);
                    cmd.Parameters.AddWithValue("@EnerjiPanoSigortaNo", enerjiPanoSigortaNo);
                    cmd.Parameters.AddWithValue("@X", xKoordinat);  // X parametresi eklendi
                    cmd.Parameters.AddWithValue("@Y", yKoordinat);  // Y parametresi eklendi

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
    }
}
