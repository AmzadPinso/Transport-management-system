using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Transport_Management_System.Repository.Interface;

namespace Transport_Management_System.Repository.Application
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var smtpSection = _configuration.GetSection("SmtpSettings");
            var host = smtpSection.GetValue<string>("Host") ?? "smtp.gmail.com";
            var port = smtpSection.GetValue<int>("Port");
            var enableSsl = smtpSection.GetValue<bool>("EnableSsl");
            var senderEmail = smtpSection.GetValue<string>("SenderEmail") ?? "";
            var senderName = smtpSection.GetValue<string>("SenderName") ?? "TMS PRO Support";
            var appPassword = smtpSection.GetValue<string>("AppPassword") ?? "";

            using (var message = new MailMessage())
            {
                message.From = new MailAddress(senderEmail, senderName);
                message.To.Add(new MailAddress(toEmail));
                message.Subject = subject;
                message.Body = htmlMessage;
                message.IsBodyHtml = true;

                using (var client = new SmtpClient(host, port))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(senderEmail, appPassword);
                    client.EnableSsl = enableSsl;

                    await client.SendMailAsync(message);
                }
            }
        }
    }
}
