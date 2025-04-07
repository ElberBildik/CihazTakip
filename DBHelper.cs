using System;
using System.Data;
using System.Data.SqlClient;

namespace Cihaz_Takip_Uygulaması
{
    public static class DBHelper
    {
        // Cihaz durumunu günceller
        public static void GuncelleDurum(int cihazRecNo)
        {
            string query = "UPDATE Cihaz SET Durum = 'down oldu, mail atıldı' WHERE RecNo = @RecNo";

            using (SqlConnection conn = new SqlConnection(ConnectionString.Get))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@RecNo", cihazRecNo);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        // Cihazın son ping zamanını alır
        public static DateTime GetSonPingZamani(int cihazRecNo)
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

        // CihazGrup tablosundan mail bekleme süresini alır
        public static int GetMailBeklemeSuresi(int recNo)
        {
            int beklemeSuresi = 0;

            try
            {
                // SQL sorgusu ile MailBeklemeSüresi değerini al
                string query = "SELECT MailBeklemeSüresi FROM CihazGrup WHERE RecNo = @RecNo";

                using (SqlConnection conn = new SqlConnection(ConnectionString.Get))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@RecNo", recNo);
                    conn.Open();

                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        beklemeSuresi = Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata işleme
                Console.WriteLine($"MailBeklemeSüresi alma hatası: {ex.Message}");
            }

            return beklemeSuresi;
        }

        // CihazGrup tablosundan mail adresini alır
        public static string GetMailAdres(int grupRecNo)
        {
            string mailAdres = null;

            try
            {
                // SQL sorgusu ile ToMailAdress değerini al
                string query = "SELECT ToMailAdress FROM CihazGrup WHERE RecNo = @GrupRecNo";

                using (SqlConnection conn = new SqlConnection(ConnectionString.Get))
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GrupRecNo", grupRecNo);
                    conn.Open();

                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        mailAdres = result.ToString();
                    }
                    else
                    {
                        Console.WriteLine($"Mail adresi bulunamadı, GrupRecNo: {grupRecNo}");
                        mailAdres = string.Empty; // Boş döndürerek mail gönderiminden kaçınılabilir
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                Console.WriteLine($"Mail adresi alma hatası: {ex.Message}");
                mailAdres = null; // Eğer hata oluşursa null döndürüyoruz
            }

            return mailAdres;
        }

    }
}
