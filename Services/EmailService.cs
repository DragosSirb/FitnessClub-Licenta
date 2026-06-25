using System.Net;
using System.Net.Mail;

namespace FitnessClub.Services
{
    public class EmailService
    {
        private readonly string _senderEmail;
        private readonly string _appPassword;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration)
        {
            _senderEmail = configuration["Email:SenderEmail"]!;
            _appPassword = configuration["Email:AppPassword"]!;
            _senderName = configuration["Email:SenderName"] ?? "FitnessClub";
        }

        public async Task SendPaymentConfirmationAsync(string toEmail, string toName, string subject, string htmlContent)
        {
            if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_appPassword))
                return;

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_senderEmail, _appPassword),
                EnableSsl = true
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_senderEmail, _senderName),
                Subject = subject,
                Body = htmlContent,
                IsBodyHtml = true
            };
            message.To.Add(new MailAddress(toEmail, toName));

            await client.SendMailAsync(message);
        }
    }
}
