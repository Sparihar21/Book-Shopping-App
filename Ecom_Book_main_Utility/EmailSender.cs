using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Ecom_Book_main_Utility
{
    public class EmailSender : IEmailSender
    {
        private EmailSettings _emailSettings { get; }
        public EmailSender(IOptions<EmailSettings> emailSetting)
        {
            _emailSettings = emailSetting.Value;
        }

        public async Task Execute (string email,string subject,string message)
        {
            try
            {
                string toEmail = string.IsNullOrEmpty(email) ? _emailSettings.ToEmail : email;
                MailMessage mailMessage = new MailMessage()
                {
                    From = new MailAddress(_emailSettings.UserNameEmail, "My Email Name")
                };
                mailMessage.To.Add(toEmail);
                mailMessage.CC.Add(_emailSettings.CCEmail);
                mailMessage.Subject = "Shopping App : " + subject;
                mailMessage.Body = message;
                mailMessage.IsBodyHtml = true;
                mailMessage.Priority = MailPriority.High;
                using (SmtpClient smtp=new SmtpClient(_emailSettings.PrimaryDomain,_emailSettings.PrimaryPort))
                {
                    smtp.Credentials=new NetworkCredential(_emailSettings.UserNameEmail,_emailSettings.UserNamePassword);
                    smtp.EnableSsl = true;
                    await smtp.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex) 
            {
                string str = ex.Message;
            }
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Execute(email, subject, htmlMessage).Wait();
            return Task.FromResult(0);
        }
    }
}
