using System;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Cihaz_Takip_Uygulaması
{
    public static class MailHelper
    {

        //Veritabanı kontrolleri yapılacak mail gönderildi yazıyorsa tekrardam mail gönderilmeyecek bunu düzenlemem gerekiyor
        private static string connectionString = "Data Source=ES-BT14\\SQLEXPRESS;Initial Catalog=CihazTakip;Integrated Security=True";

        public static async Task GonderAsync(string alici, string konu, string icerik, int cihazRecNo)//mail gönderiminde gerekli olan metot
        {
            try
            {
                // Cihaz grubunun "Enerji panosu" olup olmadığını kontrol et
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string checkQuery = @"
                SELECT cg.Kod
                FROM Cihaz c
                INNER JOIN CihazGrup cg ON c.GrupRecNo = cg.RecNo
                WHERE c.RecNo = @RecNo"; // Cihazın bağlı olduğu grup kodunu alıyoruz

                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@RecNo", cihazRecNo);
                        var result = await checkCommand.ExecuteScalarAsync();

                        // Eğer grup kodu "Enerji panosu" ise, mail gönderilmesin
                        if (result != null && result.ToString() == "Enerji panosu")
                        {
                            return; 
                        }
                    }
                }

                // Mail gönderme işlemi
                MailMessage mail = new MailMessage
                {
                    From = new MailAddress("wfmailer@egeseramik.com"),
                    Subject = konu,
                    Body = icerik
                };
                mail.To.Add(alici);

                SmtpClient smtp = new SmtpClient("eposta.egeseramik.com", 25)
                {
                    EnableSsl = false,
                    UseDefaultCredentials = true
                };

                await smtp.SendMailAsync(mail);

                // Mail başarılı şekilde gönderildiyse cihaz durumunu güncelle
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "UPDATE Cihaz SET Durum = @Durum WHERE RecNo = @RecNo";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Durum", "Down, mail gönderildi");
                        command.Parameters.AddWithValue("@RecNo", cihazRecNo);
                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Mail gönderme hatası: " + ex.Message);
            }
        }


    }
}
