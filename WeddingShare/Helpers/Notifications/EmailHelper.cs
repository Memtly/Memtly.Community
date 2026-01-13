using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Localization;
using Razor.Templating.Core;
using WeddingShare.Resources.Templates.Email;

namespace WeddingShare.Helpers.Notifications
{
    public class EmailHelper : INotificationHelper
    {
        private readonly ISettingsHelper _settings;
        private readonly ISmtpClientWrapper _client;
        private readonly ILogger _logger;
        private readonly IStringLocalizer<Lang.Translations> _localizer;

        public EmailHelper(ISettingsHelper settings, ISmtpClientWrapper client, ILogger<EmailHelper> logger, IStringLocalizer<Lang.Translations> localizer)
        {
            _settings = settings;
            _client = client;
            _logger = logger;
            _localizer = localizer;
        }

        public async Task<bool> Send(string title, string message, string? actionLink = null)
        {
            return await this.SendTo(await _settings.GetOrDefault(Constants.Notifications.Smtp.Recipient, string.Empty), title, new BasicEmail()
            {
                Title = title,
                Message = message,
                Link = !string.IsNullOrWhiteSpace(actionLink) ? new BasicEmailLink()
                {
                    Heading = _localizer["Visit"].Value,
                    Value = actionLink
                } : null
            });
        }

        public async Task<bool> SendTo(string recipients, string title, BasicEmail model)
        {
            var body = await RazorTemplateEngine.RenderAsync("~/Resources/Templates/Email/Basic.cshtml", model);
            return await this.SendTo(recipients, title, body);
        }

        public async Task<bool> SendTo(string recipients, string title, string message)
        {
            if (await _settings.GetOrDefault(Constants.Notifications.Smtp.Enabled, false))
            {
                try
                { 
                    var host = await _settings.GetOrDefault(Constants.Notifications.Smtp.Host, string.Empty);
                    var port = await _settings.GetOrDefault(Constants.Notifications.Smtp.Port, 587);
                    var from = await _settings.GetOrDefault(Constants.Notifications.Smtp.From, string.Empty);
                    var displayName = await _settings.GetOrDefault(Constants.Notifications.Smtp.DisplayName, "WeddingShare");
                    var enableSSL = await _settings.GetOrDefault(Constants.Notifications.Smtp.UseSSL, false);

                    var username = await _settings.GetOrDefault(Constants.Notifications.Smtp.Username, string.Empty);
                    var password = await _settings.GetOrDefault(Constants.Notifications.Smtp.Password, string.Empty);

                    NetworkCredential? credentials = null;
                    if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
                    {
                        credentials = new NetworkCredential(username, password);
                    }

                    return await SendTo(host, port, from, displayName, enableSSL, credentials, recipients, title, message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email with title '{title}' - {ex?.Message}");
                }
            }

            return false;
        }

        public async Task<bool> SendTo(string host, int port, string from, string displayName, bool enableSSL, NetworkCredential? credentials, string recipients, string title, string message)
        {
            var addressList = recipients?.Split(new char[] { ';', ',' }, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)?.Select(x => new MailAddress(x));
            
            if (addressList != null && addressList.Any())
            { 
                if (!string.IsNullOrWhiteSpace(host))
                {
                    if (port > 0)
                    {
                        if (!string.IsNullOrWhiteSpace(from))
                        {
                            var sentToAll = true;
                            using (var smtp = new SmtpClient(host, port))
                            {
                                if (!string.IsNullOrWhiteSpace(credentials?.UserName) && !string.IsNullOrWhiteSpace(credentials?.Password))
                                {
                                    smtp.UseDefaultCredentials = false;
                                    smtp.Credentials = credentials;
                                }

                                smtp.EnableSsl = enableSSL;

                                var sender = new MailAddress(from, displayName);
                                foreach (var to in addressList)
                                {
                                    try
                                    {
                                        await _client.SendMailAsync(smtp, new MailMessage(sender, to)
                                        {
                                            Sender = sender,
                                            Subject = title,
                                            Body = message,
                                            IsBodyHtml = true,
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogWarning(ex, $"Failed to send email to '{to}' - {ex.Message}");
                                        sentToAll = false;
                                    }
                                }
                            }
                
                            return sentToAll;
                        }
                        else
                        { 
                            _logger.LogWarning($"Invalid SMTP sender specified");
                        }
                    }
                    else
                    { 
                        _logger.LogWarning($"Invalid SMTP port specified");
                    }
                }
                else
                {
                    _logger.LogWarning($"Invalid SMTP host specified");
                }
            }
            else
            {
                _logger.LogWarning($"Invalid SMTP recipient specified");
            }

            return false;
        }
    }
}