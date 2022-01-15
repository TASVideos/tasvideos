using System;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.Email
{
	/// <summary>
	/// An implementation of <see cref="IEmailSender"/> that uses a configured SMTP server
	/// </summary>
	internal class GmailSender : IEmailSender
	{
		private readonly IHostEnvironment _env;
		private readonly AppSettings _settings;
		private readonly ILogger<GmailSender> _logger;
		private readonly IGoogleAuthService _googleAuthService;

		public GmailSender(IHostEnvironment env,
			AppSettings settings,
			ILogger<GmailSender> logger,
			IGoogleAuthService googleAuthService)
		{
			_env = env;
			_settings = settings;
			_logger = logger;
			_googleAuthService = googleAuthService;
		}

		public async Task SendEmail(IEmail email)
		{
			if (!_settings.Gmail.IsEnabled())
			{
				_logger.LogWarning("Attempting to send email without email address configured");
				return;
			}

			var token = await _googleAuthService.GetGmailAccessToken();

			if (string.IsNullOrWhiteSpace(token))
			{
				_logger.LogError("Unable to acquire get gmail token, skipping email: subject: {0} message: {1}", email.Subject, email.Message);
				return;
			}

			try
			{
				using var client = new SmtpClient();
				await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

				// use the access token
				var oauth2 = new SaslMechanismOAuth2(_settings.Gmail.From, token);
				await client.AuthenticateAsync(oauth2);
				var message = BccList(email);
				await client.SendAsync(message);
				await client.DisconnectAsync(true);
			}
			catch (Exception ex)
			{
				_logger.LogError("Unable to authenticate email, skipping email: subject: {0} message: {1} exception: {2}", email.Subject, email.Message, ex);
			}
		}

		private MimeMessage BccList(IEmail email)
		{
			var recipients = email.Recipients.ToList();
			var from = _env.IsProduction() ? "noreply" : $"TASVideos {_env.EnvironmentName} environment noreply";
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(from, _settings.Gmail.From));

			if (recipients.Count == 1)
			{
				message.To.Add(new MailboxAddress(recipients[0], recipients[0]));
			}
			else
			{
				foreach (var recipient in email.Recipients)
				{
					message.Bcc.Add(new MailboxAddress(recipient, recipient));
				}
			}

			var bodyBuilder = new BodyBuilder();

			if (email.ContainsHtml)
			{
				bodyBuilder.HtmlBody = email.Message;
			}
			else
			{
				bodyBuilder.TextBody = email.Message;
			}
			
			message.Body = bodyBuilder.ToMessageBody();

			message.Subject = email.Subject;

			return message;
		}
	}
}
