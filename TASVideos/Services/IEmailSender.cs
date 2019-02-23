using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;

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
		Task SendTopicNotification(int postId, int topicId, string topicTitle, string baseUrl, IEnumerable<string> emailAddresses);
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

		public Task SendEmail(string email, string subject, string message)
		{
			return Execute(_settings.SendGridKey, subject, message, email);
		}

		public async Task SendTopicNotification(int postId, int topicId, string topicTitle, string baseUrl, IEnumerable<string> emailAddresses)
		{
			string siteName = "TASVideos";
			if (!_env.IsProduction())
			{
				siteName += $" - {_env.EnvironmentName} environment";
			}

			string subject = "Topic Reply Notification - " + topicTitle;
			string message = $@"Hello,

You are receiving this email because you are watching the topic, ""{topicTitle}"" at {siteName}. This topic has received a reply since your last visit. You can use the following link to view the replies made, no more notifications will be sent until you visit the topic.

{baseUrl}/Forum/p/{postId}#{postId}

If you no longer wish to watch this topic you can either click the ""Stop watching this topic link"" found at the top of the topic above, or by clicking the following link:

{baseUrl}/Forum/Topics/20848?handler=Unwatch";

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

			string from = "noreply";
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
