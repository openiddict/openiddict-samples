using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using newoidc.Models;
using MimeKit;
using Microsoft.AspNetCore.Mvc;
using MailKit.Security;
using Newtonsoft.Json;
namespace newoidc.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }

    public class communicator
    {
        private result_model result = new result_model();
        private result_error_model err = new result_error_model();
        
        
        public async Task<string> SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress("SwapKart Assistance", "assist@swapkart.com"));
                emailMessage.To.Add(new MailboxAddress("", email));
                emailMessage.Subject = subject;
                emailMessage.Body = new TextPart("html") { Text = message };

                using (var client = new SmtpClient())
                {
                    client.Connect("mail.swapkart.com", 587, false);

                    // Note: since we don't have an OAuth2 token, disable
                    // the XOAUTH2 authentication mechanism.
                    client.AuthenticationMechanisms.Remove("XOAUTH2");

                    // Note: only needed if the SMTP server requires authentication
                    await client.AuthenticateAsync("assist@swapkart.com", "polardevil");
                    await client.SendAsync(emailMessage);
                    await client.DisconnectAsync(true);
                    result.Succeeded = true;
                    return JsonConvert.SerializeObject(result);
                }
            }
            catch (Exception ex) {
                result.Succeeded = false;
                err.error= ex.Message;
                err.error_description = ex.InnerException.Message;
                result.errors.Add(err);
                return JsonConvert.SerializeObject( result);
            }
           
        }
    }
}
