using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.Email
{
	public interface IEmailSender
	{
		/// <summary>
		/// Sends an email to the given recipients,
		/// with the given subject and message
		/// </summary>
		Task SendEmail(IEmail email);
	}

	/// <summary>
	/// Standard implementation of <see cref="IEmailSender"/>
	/// that uses SendGrid
	/// </summary>
	internal class SendGridSender : IEmailSender
	{
		private readonly AppSettings _settings;
		private readonly IWebHostEnvironment _env;
		private readonly ILogger<SendGridSender> _logger;

		public SendGridSender(
			IWebHostEnvironment environment,
			AppSettings settings,
			ILogger<SendGridSender> logger)
		{
			_settings = settings;
			_env = environment;
			_logger = logger;
		}

		public Task SendEmail(IEmail email)
		{
			return Execute(email);
		}

		private async Task Execute(IEmail email)
		{
			string apiKey = _settings.SendGridKey;
			if (string.IsNullOrWhiteSpace(_settings.SendGridKey) || string.IsNullOrWhiteSpace(_settings.SendGridFrom))
			{
				return;
			}

			string from = "noreply";
			string subject = email.Subject;
			if (!_env.IsProduction())
			{
				from = $"TASVideos {_env.EnvironmentName} environment {from}";
				subject = $"TASVideos {_env.EnvironmentName} environment - {subject}";
			}

			var client = new SendGridClient(apiKey);
			var msg = new SendGridMessage
			{
				From = new EmailAddress(_settings.SendGridFrom, from),
				Subject = subject,
				PlainTextContent = email.Message,
				HtmlContent = email.Message
			};

			foreach (var recipient in email.Recipients)
			{
				msg.AddTo(new EmailAddress(recipient));
			}

			// Disable click tracking. See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
			msg.SetClickTracking(false, false);

			var result = await client.SendEmailAsync(msg);
			if (!result.IsSuccessStatusCode)
			{
				var body = await result.Body.ReadAsStringAsync();
				_logger.LogError($"Unable to send email. {body}");
			}
		}
	}

	internal class EmailLogger : IEmailSender
	{
		private readonly ILogger<EmailLogger> _logger;

		public EmailLogger(ILogger<EmailLogger> logger)
		{
			_logger = logger;
		}

		public Task SendEmail(IEmail email)
		{
			string message = $"Email Generated:\nRecipients: {string.Join(",", email.Recipients)}\nSubject: {email.Subject}\nMessage: {email.Message}";
			_logger.LogInformation(message);
			return Task.CompletedTask;
		}
	}
}
