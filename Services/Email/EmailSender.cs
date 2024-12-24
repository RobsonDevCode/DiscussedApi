
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;
using Org.BouncyCastle.Asn1.Tsp;
using DiscussedApi.Configuration;
using DiscussedApi.Models;
using static DiscussedApi.Models.EmailTypeToGenertate;
using Mailjet.Client;
using Newtonsoft.Json.Linq;
using Mailjet.Client.Resources;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Html;
using DiscussedApi.Reopisitory.Auth;
using FluentEmail.Core;

namespace DiscussedApi.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly NLog.ILogger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _sender;
        public EmailSender()
        {
            _apiKey = Settings.EmailSettings.Key;
            _apiSecret = Settings.EmailSettings.Secret;
            _sender = Settings.EmailSettings.Sender;
            _httpClient = new HttpClient();

            // Set up basic authentication
            var authToken = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_apiKey}:{_apiSecret}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authToken);
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task SendAsync(string toEmail, string subject, string? htmlBody, string? textContent = null)
        {
            var payload = new Dictionary<string, object>
            {
            { "FromEmail", _sender },
            { "FromName", "Discussed Help" },
            { "Subject", subject },
            { "Recipients", new[] { new { Email = toEmail } } }
            };

            if (!string.IsNullOrEmpty(htmlBody))
                payload["Html-part"] = htmlBody;

            if (!string.IsNullOrEmpty(textContent))
                payload["Text-part"] = textContent;


            var jsonPayload = JsonConvert.SerializeObject(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.mailjet.com/v3/send", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to send email. Status code: {response.StatusCode}, Response: {responseContent}");


        }



        public async Task<string> GenerateTemplateHtmlBodyAsync(EmailType emailType)
        {

            switch (emailType)
            {
                case EmailType.Recovery:
                    return await File.ReadAllTextAsync(Settings.EmailSettings.RecoveryHtmlBodyFilePath);

                case EmailType.Confirmation:
                    return await File.ReadAllTextAsync(Settings.EmailSettings.ConfirmationBodyFilePath);

                default:
                    throw new NotImplementedException("Email Type given is invalid or isn't implimented on the version being used");
            }

        }
    }
}
