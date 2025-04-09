using System;
using System.Data;
using System.Data.SqlClient;

namespace Cihaz_Takip_Uygulaması
{
    public static class DBHelper
    {
        // Cihaz durumunu günceller
        public static void GuncelleDurum(int cihazRecNo, string durum)
        {
            try
            {
                string query = "UPDATE Cihaz SET Durum = @Durum WHERE RecNo = @RecNo";

                using (SqlConnection conn = new SqlConnection(ConnectionString.Get))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RecNo", cihazRecNo);
                    cmd.Parameters.AddWithValue("@Durum", durum);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cihaz durumu güncellenirken hata oluştu: " + ex.Message);
            }
        }

        // Cihazın son ping zamanını alır
        public static DateTime GetSonPingZamani(int cihazRecNo)
        {
            try
            {
                string query = @"
                SELECT MAX(DownTime)
                FROM Log
                WHERE CihazRecNo = @CihazRecNo";

                using (SqlConnection conn = new SqlConnection(ConnectionString.Get))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CihazRecNo", cihazRecNo);
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToDateTime(result) : DateTime.MinValue;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Son ping zamanı alınırken hata oluştu: " + ex.Message);
            }
        }

        // CihazGrup tablosundan mail bekleme süresini alır
        public static int GetMailBeklemeSuresi(int recNo)
        {
            try
            {
                string query = "SELECT MailBeklemeSüresi FROM CihazGrup WHERE RecNo = @RecNo";

                using (SqlConnection conn = new SqlConnection(ConnectionString.Get))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Parametreyi doğru türde ekliyoruz
                    cmd.Parameters.Add(new SqlParameter("@RecNo", SqlDbType.Int) { Value = recNo });

                    conn.Open();

                    object result = cmd.ExecuteScalar();

                    // Sonuç null veya DBNull ise 0 döndürüyoruz
                    return result != DBNull.Value && result != null ? Convert.ToInt32(result) : 0;
                }
            }
            catch (Exception ex)
            {
                // Hata mesajını daha açıklayıcı yapabiliriz
                throw new Exception("Mail bekleme süresi alınırken hata oluştu. Sorgu: ", ex);
            }
        }


        // CihazGrup tablosundan mail adresini aldık 
        public static string GetMailAdres(int grupRecNo)
        {
            try
            {
                string query = "SELECT ToMailAdress FROM CihazGrup WHERE RecNo = @GrupRecNo";

                using (SqlConnection conn = new SqlConnection(ConnectionString.Get))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GrupRecNo", grupRecNo);
                    conn.Open();

                    object result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? result.ToString() : string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Mail adresi alınırken hata oluştu: " + ex.Message);
            }
        }


        public static void CihazDownKaydi(int cihazRecNo)
        {
            try
            {
                // Daha önce "down" kaydı var mı kontrol et
                DateTime sonPingZamani = GetSonPingZamani(cihazRecNo);

                // Eğer cihaz daha önce "down" olduysa yeni bir kayıt eklemeye gerek yok
                if (sonPingZamani != DateTime.MinValue)
                {
                    return; // Cihazın down kaydı zaten var
                }

                // Cihaz down olduysa Log tablosuna kaydet
                string query = @"
                INSERT INTO Log (CihazRecNo, DownTime)
                VALUES (@CihazRecNo, @DownTime)";

                using (SqlConnection conn = new SqlConnection(ConnectionString.Get))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CihazRecNo", cihazRecNo);
                    cmd.Parameters.AddWithValue("@DownTime", DateTime.Now); // Şu anki zaman

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cihazın down kaydı eklenirken hata oluştu: " + ex.Message);
            }
        }
    }
}