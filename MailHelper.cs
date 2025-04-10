using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Cihaz_Takip_Uygulaması
{
    public static class MailHelper
    {
        public static async Task GonderAsync(string alici, string konu, string icerik)
        {
            try
            {
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress("wfmailer@egeseramik.com"); // Gönderen adres
                mail.To.Add(alici); // Alıcı
                mail.Subject = konu;
                mail.Body = icerik;

                SmtpClient smtp = new SmtpClient("eposta.egeseramik.com", 25)// 587 yerine 25 şifresiz gönderim için
                {
                    EnableSsl = false, // SSL kapalı
                    UseDefaultCredentials = true
                }; 
                await smtp.SendMailAsync(mail);
            }
            catch (Exception ex)
            {//mail gönderildi gönderilemedi kodunu da eklemek istiyorum buraya
                // Hata loglama yapılabilir
                System.Windows.Forms.MessageBox.Show("Mail gönderme hatası: " + ex.Message);
            }
        }
    }
}
