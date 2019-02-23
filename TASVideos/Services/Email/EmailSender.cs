using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace TASVideos.Services.Email
{
	public interface IEmailSender
	{
		/// <summary>
		/// Sends an email to the given recipients,
		/// with the given subject and message
		/// </summary>
		Task SendEmail(IEmail email);
	}

	public class EmailSender : IEmailSender
	{
		private readonly AppSettings _settings;
		private readonly IHostingEnvironment _env;

		public EmailSender(
			IHostingEnvironment environment,
			IOptions<AppSettings> settings)
		{
			_settings = settings.Value;
			_env = environment;
		}

		public Task SendEmail(IEmail email)
		{
			return Execute(email);
		}

		private Task Execute(IEmail email)
		{
			string apiKey = _settings.SendGridKey;
			if (string.IsNullOrWhiteSpace(_settings.SendGridKey))
			{
				return Task.CompletedTask;
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
				////msg.AddTo(new EmailAddress(recipient));
				msg.AddTo(new EmailAddress("adelikat@tasvideos.org")); // TODO: for now to avoid accidental spam
			}

			// Disable click tracking.
			// See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
			msg.SetClickTracking(false, false);

			return client.SendEmailAsync(msg);
		}
	}
}
