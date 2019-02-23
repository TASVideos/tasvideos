using System.Collections.Generic;
using System.Linq;
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
		/// Sends an email to the given email address,
		/// with the given subject and message
		/// </summary>
		Task SendEmail(IEmail email);

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

		public Task SendEmail(IEmail email)
		{
			return Execute(email);
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

			await Execute(new StandardEmail
			{
				Recipients = new[] { "adelikat@tasvideos.org" }, // TODO: hardcoding this for now to avoid accidental spam
				Subject = subject,
				Message = message
			});
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
				msg.AddTo(new EmailAddress(recipient));
			}

			// Disable click tracking.
			// See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
			msg.SetClickTracking(false, false);

			return client.SendEmailAsync(msg);
		}
	}
}
