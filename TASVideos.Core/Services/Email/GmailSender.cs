using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TASVideos.Core.Services.Youtube;
using TASVideos.Core.Settings;
using Org.BouncyCastle.Cms;

namespace TASVideos.Core.Services.Email
{
	/// <summary>
	/// An implementation of <see cref="IEmailSender"/> that uses a configured SMTP server
	/// </summary>
	internal class GmailSender : IEmailSender
	{
		private readonly IWebHostEnvironment _env;
		private readonly AppSettings _settings;
		private readonly ILogger<GmailSender> _logger;
		private readonly IGoogleAuthService _googleAuthService;

		public GmailSender(IWebHostEnvironment env,
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

			using var client = new SmtpClient();
			await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

			// use the access token
			var oauth2 = new SaslMechanismOAuth2(_settings.Gmail.From, token);
			await client.AuthenticateAsync(oauth2);

			var message = BccList(email);
			await client.SendAsync(message);
			await client.DisconnectAsync(true);
		}

		private MimeMessage BccList(IEmail email)
		{
			var from = _env.IsProduction() ? "noreply" : $"TASVideos {_env.EnvironmentName} environment noreply";
			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(from, _settings.Gmail.From));
			
			foreach (var recipient in email.Recipients)
			{
				message.Bcc.Add(new MailboxAddress(recipient, recipient));
			}

			message.Body = new TextPart("plain")
			{
				Text = email.Message
			};
			message.Subject = email.Subject;

			return message;
		}
	}
}
