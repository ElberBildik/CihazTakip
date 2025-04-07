using System;
using System.Data.SqlClient;

namespace Cihaz_Takip_Uygulaması
{
    public class LogIslemleri
    {
        private readonly SqlConnection _baglanti;

        // Constructor, SqlConnection parametresi alır
        public LogIslemleri(SqlConnection baglanti)
        {
            _baglanti = baglanti;//bağlantı sınıfından connection sağladık.
        }

        // CihazRecNo'ya göre log eklemek için metod
        public void LogEkle(int cihazRecNo)
        {
            try
            {
                // Önce CihazRecNo'ya sahip bir kayıt var mı kontrol edelim
                string kontrolSorgu = "SELECT COUNT(1) FROM Log WHERE CihazRecNo = @cihazRecNo";

                using (SqlCommand kontrolCmd = new SqlCommand(kontrolSorgu, _baglanti))
                {
                    kontrolCmd.Parameters.AddWithValue("@cihazRecNo", cihazRecNo);

                    int mevcutKayitSayisi = Convert.ToInt32(kontrolCmd.ExecuteScalar());

                    // Eğer kayıt yoksa, yeni bir kayıt ekleyelim
                    if (mevcutKayitSayisi == 0)
                    {
                        string logSorgu = "INSERT INTO Log (CihazRecNo, DownTime) VALUES (@cihazRecNo, @downTime)";
                        using (SqlCommand logCmd = new SqlCommand(logSorgu, _baglanti))
                        {
                            logCmd.Parameters.AddWithValue("@cihazRecNo", cihazRecNo);
                            logCmd.Parameters.AddWithValue("@downTime", DateTime.Now);

                            logCmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj göster
                Console.WriteLine("Log kaydı eklenirken hata oluştu: " + ex.Message);
            }
        }
    }
}
