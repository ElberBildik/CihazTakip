using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Cihaz_Takip_Uygulaması
{
    public class MailHelper
    {
        // Mail gönderme metodu, mail adresi DB'den alınıyor
        public static async Task GonderAsync(string toMailAdres, string konu, string mesaj)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("elberbildik277@gmail.com", "dvuy vsjl mskr itwx"), // E-posta ve şifre
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("elberbildik277@gmail.com"),
                    Subject = konu,
                    Body = mesaj,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toMailAdres); // Alıcı e-posta adresi


                // SMTP debug logging
                smtpClient.SendCompleted += (sender, e) =>
                {
                    if (e.Error != null)
                    {
                        Console.WriteLine($"Mail gönderimi sırasında hata: {e.Error.Message}");
                    }
                    else
                    {
                        Console.WriteLine("Mail başarıyla gönderildi.");
                    }
                };

                // Mail gönderme
                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SMTP Mail gönderim hatası: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mail gönderimi sırasında hata: {ex.Message}");
            }
        }
    }
}
