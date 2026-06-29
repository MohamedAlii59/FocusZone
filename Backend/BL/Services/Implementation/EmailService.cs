using System;
using System.Net;
using System.Net.Mail;
using BL.Services.Abstraction;
using Microsoft.Extensions.Configuration;

namespace BL.Services.Implementation
{
    //tsem gpwa xwpy fznk

 

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("EmailSettings:Smtp");
                var host = smtpSettings["Host"];
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var senderEmail = smtpSettings["SenderEmail"];
                var senderPassword = smtpSettings["SenderPassword"];
                var senderName = smtpSettings["SenderName"] ?? "StuckIn";
                var enableSsl = bool.Parse(smtpSettings["EnableSSL"] ?? "true");

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
                    throw new InvalidOperationException("Email settings are not configured properly");

                using (var smtpClient = new SmtpClient(host, port))
                {
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = enableSsl;
                    smtpClient.Timeout = 30000;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
                        Subject = subject,
                        Body = htmlBody,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(to);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to send email to {to}", ex);
            }
        }
    }
}
