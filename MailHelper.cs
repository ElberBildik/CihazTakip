using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Cihaz_Takip_Uygulaması
{
    public class MailHelper
    {
        public static async Task GonderAsync(string toMailAdres, string konu, string mesaj)
        {
            try
            {
                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("elberbildik277@gmail.com", "vrwithzqihngwuci"),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("elberbildik4568521@gmail.com"),
                    Subject = konu,
                    Body = mesaj,
                    IsBodyHtml = true,
                };

                mailMessage.To.Add(toMailAdres);
                await smtpClient.SendMailAsync(mailMessage);
                Console.WriteLine("Mail başarıyla gönderildi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Mail gönderimi sırasında bir hata oluştu: {ex.Message}");
            }
        }
    }
}
