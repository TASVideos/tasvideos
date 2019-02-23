using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace TASVideos.Services
{
	// This class is used by the application to send email for account confirmation and password reset.
	// For more details see https://go.microsoft.com/fwlink/?LinkID=532713
	public interface IEmailSender
	{
		/// <summary>
		/// Sends an email to the given email address,
		/// with the given subject and message
		/// </summary>
		Task SendEmail(string email, string subject, string message);

		/// <summary>
		/// Sends a topic reply notification email to the given email addresses
		/// </summary>
		Task SendTopicNotification(IEnumerable<string> emailAddresses);
	}

	public class EmailSender : IEmailSender
	{
		private readonly AppSettings _settings;
		public EmailSender(IOptions<AppSettings> settings)
		{
			_settings = settings.Value;
		}

		public Task SendEmail(string email, string subject, string message)
		{
			return Execute(_settings.SendGridKey, subject, message, email);
		}

		public async Task SendTopicNotification(IEnumerable<string> emailAddresses)
		{
			string subject = "Topic Reply Notification - EZGAmes69 situation";
			string message = "http://tasvideos.org/forum/viewtopic.php?p=481568#481568";

			string email = "adelikat@tasvideos.org";
			// TODO: Task.WhenAll, or WhenAny
			//foreach (var email in emailAddresses)
			//{
				await Execute(_settings.SendGridKey, subject, message, email);
			//}
		}

		private Task Execute(string apiKey, string subject, string message, string email)
		{
			if (string.IsNullOrWhiteSpace(_settings.SendGridKey))
			{
				return Task.CompletedTask;
			}

			var client = new SendGridClient(apiKey);
			var msg = new SendGridMessage()
			{
				From = new EmailAddress(_settings.SendGridFrom, "noreply"),
				Subject = subject,
				PlainTextContent = message,
				HtmlContent = message
			};
			msg.AddTo(new EmailAddress(email));

			// Disable click tracking.
			// See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
			msg.SetClickTracking(false, false);

			return client.SendEmailAsync(msg);
		}
	}
}
