using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;

namespace TASVideos.Services.Email
{
	public interface IEmailService
	{
		Task ResetPassword(string recipient, string link);
		Task EmailConfirmation(string recipient, string link);
		Task TopicReplyNotification(IEnumerable<string> recipients, TopicReplyNotificationTemplate template);
	}

	public class EmailService : IEmailService
	{
		private readonly IHostingEnvironment _env;
		private readonly IEmailSender _emailSender;

		public EmailService(IHostingEnvironment env, IEmailSender emailSender)
		{
			_env = env;
			_emailSender = emailSender;
		}

		public async Task ResetPassword(string recipient, string link)
		{
			await _emailSender.SendEmail(new SingleEmail
			{
				Recipient = recipient,
				Subject = "Reset Password",
				Message = $"Please reset your password by clicking here: <a href='{link}'>link</a>"
			});
		}

		public async Task EmailConfirmation(string recipient, string link)
		{
			await _emailSender.SendEmail(new SingleEmail
			{
				Recipient = recipient,
				Subject = "Confirm your email",
				Message = $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>"
			});
		}

		public async Task TopicReplyNotification(IEnumerable<string> recipients, TopicReplyNotificationTemplate template)
		{
			if (recipients == null || !recipients.Any())
			{
				return;
			}

			string siteName = "TASVideos";
			if (!_env.IsProduction())
			{
				siteName += $" - {_env.EnvironmentName} environment";
			}

			string subject = "Topic Reply Notification - " + template.TopicTitle;
			string message = $@"Hello,

You are receiving this email because you are watching the topic, ""{template.TopicTitle}"" at {siteName}. This topic has received a reply since your last visit. You can use the following link to view the replies made, no more notifications will be sent until you visit the topic.

{template.BaseUrl}/Forum/p/{template.PostId}#{template.PostId}

If you no longer wish to watch this topic you can either click the ""Stop watching this topic link"" found at the top of the topic above, or by clicking the following link:

{template.BaseUrl}/Forum/Topics/20848?handler=Unwatch

--
Thanks,
on behalf of TASVideos staff";

			await _emailSender.SendEmail(new StandardEmail
			{
				Recipients = recipients,
				Subject = subject,
				Message = message
			});
		}
	}

	public class TopicReplyNotificationTemplate
	{
		public int PostId { get; set; }
		public int TopicId { get; set; }
		public string TopicTitle { get; set; }
		public string BaseUrl { get; set; }
	}
}
