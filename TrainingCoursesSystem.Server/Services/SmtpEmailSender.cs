using System.Net;
using System.Net.Mail;

namespace TrainingCoursesSystem.Server.Services
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public SmtpEmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpEnabled = _configuration.GetValue<bool>("Smtp:Enabled");

            if (smtpEnabled == false)
            {
                Console.WriteLine("SMTP is disabled. Email content:");
                Console.WriteLine($"To: {toEmail}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine(body);
                return;
            }

            var host = _configuration["Smtp:Host"];
            var port = _configuration.GetValue<int>("Smtp:Port");
            var username = _configuration["Smtp:Username"];
            var password = _configuration["Smtp:Password"];
            var fromEmail = _configuration["Smtp:FromEmail"];
            var fromName = _configuration["Smtp:FromName"];

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail!, fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}