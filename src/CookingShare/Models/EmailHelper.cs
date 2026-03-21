using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace CookingShare.Models
{
    public static class EmailHelper
    {
        public static bool SendEmail(string toEmail, string subject, string body)
        {
            try
            {
                // Lấy thông tin từ Web.config để bảo mật tuyệt đối
                string senderEmail = ConfigurationManager.AppSettings["SmtpEmail"];
                string senderPassword = ConfigurationManager.AppSettings["SmtpPassword"];
                string siteName = ConfigurationManager.AppSettings["SiteName"] ?? "CookingShare System";

                // Nếu chưa cấu hình email trong Web.config thì báo lỗi luôn
                if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                {
                    return false;
                }

                var fromAddress = new MailAddress(senderEmail, siteName);
                var toAddress = new MailAddress(toEmail);

                // Dùng khối using cho SmtpClient để giải phóng tài nguyên mạng
                using (var smtp = new SmtpClient())
                {
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.EnableSsl = true;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(fromAddress.Address, senderPassword);

                    using (var message = new MailMessage(fromAddress, toAddress))
                    {
                        message.Subject = subject;
                        message.SubjectEncoding = Encoding.UTF8; // Chống lỗi font Tiếng Việt

                        message.Body = body;
                        message.BodyEncoding = Encoding.UTF8;    // Chống lỗi font Tiếng Việt
                        message.IsBodyHtml = true;

                        smtp.Send(message);
                    }
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}