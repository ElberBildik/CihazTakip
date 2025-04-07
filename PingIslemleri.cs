using System;
using System.Data.SqlClient;
using System.Net.NetworkInformation;
using System.Windows.Forms;

namespace Cihaz_Takip_Uygulaması
{
    public static class PingIslemleri
    {
        public static void PingVeRenklendir(DataGridView grid, RichTextBox mesajKutusu, int currentRowIndex)
        {
            if (currentRowIndex >= grid.Rows.Count)
                return;

            DataGridViewRow row = grid.Rows[currentRowIndex];

            // ✅ Anlık işlem yapılan satırı geçici renkle vurgula
            foreach (DataGridViewCell cell in row.Cells)
            {
                cell.Style.BackColor = System.Drawing.Color.Orange; // veya istediğin renk
                cell.Style.ForeColor = System.Drawing.Color.Black;
            }

            Application.DoEvents(); // UI'nin hemen güncellenmesini sağlar

            if (row.Cells["IPNo"] != null && row.Cells["IPNo"].Value != null)
            {
                string ipNo = row.Cells["IPNo"].Value.ToString();
                Ping pingSender = new Ping();
                try
                {
                    PingReply reply = pingSender.Send(ipNo, 500);
                    bool pingBasarili = (reply.Status == IPStatus.Success);
                    string yeniDurum = pingBasarili ? "UP" : "Down oldu, mail atılacak";

                    CihazDurumGuncelle(ipNo, yeniDurum);

                    if (row.Cells["Durum"] != null)
                        row.Cells["Durum"].Value = yeniDurum;

                    if (pingBasarili)
                        mesajKutusu.AppendText($"IP {ipNo} başarıyla pinglendi. Durum: Başarılı\n");
                    else
                        mesajKutusu.AppendText($"IP {ipNo} ping atılamadı. Durum: Başarısız\n");
                }
                catch (Exception)
                {
                    string yeniDurum = "Down oldu, mail atılacak";
                    CihazDurumGuncelle(ipNo, yeniDurum);

                    if (row.Cells["Durum"] != null)
                        row.Cells["Durum"].Value = yeniDurum;

                    mesajKutusu.AppendText($"IP {ipNo} ping atılamadı. Hata: Geçersiz IP\n");
                }
                HücreRenkleme.DurumRenklendir(grid, currentRowIndex);
            }
        }


        public static void CihazDurumGuncelle(string ipNo, string yeniDurum)//veritabanı işlemlerini burada yaptım
        {
            try
            {
                int cihazRecNo = -1; // Cihaz kayıt numarasını saklayacak değişken

                using (SqlConnection baglanti = new SqlConnection(ConnectionString.Get))
                {
                    baglanti.Open();

                    // Önce Cihaz tablosunu güncelle
                    string sorgu = "UPDATE Cihaz SET Durum = @durum WHERE IPNo = @ip; SELECT RecNo FROM Cihaz WHERE IPNo = @ip";
                    using (SqlCommand cmd = new SqlCommand(sorgu, baglanti))
                    {
                        cmd.Parameters.AddWithValue("@durum", yeniDurum);
                        cmd.Parameters.AddWithValue("@ip", ipNo);

                        // Bu sorgu hem güncelleme yapacak hem de RecNo değerini döndürecek
                        var result = cmd.ExecuteScalar();
                        if (result != null)
                        {
                            cihazRecNo = Convert.ToInt32(result);
                        }
                    }

                    // Eğer durum DOWN ise ve cihaz kaydı bulunduysa, log tablosuna ekle
                    if (yeniDurum.Contains("Down") && cihazRecNo > 0)
                    {
                        LogIslemleri logIslemleri = new LogIslemleri(baglanti);
                        logIslemleri.LogEkle(cihazRecNo);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Durum güncellenirken hata oluştu: " + ex.Message);
            }
        }
    }

}