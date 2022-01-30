using System.Text.Encodings.Web;
using Microsoft.Extensions.Hosting;

namespace TASVideos.Core.Services.Email;

public interface IEmailService
{
	Task ResetPassword(string recipient, string link);
	Task EmailConfirmation(string recipient, string link);
	Task PasswordResetConfirmation(string recipient, string resetLink);
	Task TopicReplyNotification(IEnumerable<string> recipients, TopicReplyNotificationTemplate template);
}

internal class EmailService : IEmailService
{
	private readonly IHostEnvironment _env;
	private readonly IEmailSender _emailSender;

	public EmailService(IHostEnvironment env, IEmailSender emailSender)
	{
		_env = env;
		_emailSender = emailSender;
	}

	public async Task ResetPassword(string recipient, string link)
	{
		await _emailSender.SendEmail(new SingleEmail
		{
			Recipient = recipient,
			Subject = "TASVideos - Reset Password",
			Message = $"Please reset your password for your TASVideos user account by clicking here: <a href='{link}'>link</a>",
			ContainsHtml = true
		});
	}

	public async Task EmailConfirmation(string recipient, string link)
	{
		await _emailSender.SendEmail(new SingleEmail
		{
			Recipient = recipient,
			Subject = "TASVideos - Confirm your email",
			Message = $"Please confirm the e-mail address for your TASVideos user account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>",
			ContainsHtml = true
		});
	}

	public async Task PasswordResetConfirmation(string recipient, string resetLink)
	{
		await _emailSender.SendEmail(new SingleEmail
		{
			Recipient = recipient,
			Subject = "TASVideos - Your Password Was Changed",
			Message = $"This email is to inform you that your TASVideos user account password was changed. If you have received this message in error, you can reset your password and reclaim your account with this <a href='{HtmlEncoder.Default.Encode(resetLink)}'>link</a>",
			ContainsHtml = true
		});
	}

	public async Task TopicReplyNotification(IEnumerable<string> recipients, TopicReplyNotificationTemplate template)
	{
		var recipientsList = recipients.ToList();
		if (!recipientsList.Any())
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

{template.BaseUrl}/Forum/Posts/{template.PostId}

If you no longer wish to watch this topic you can either click the ""Stop watching this topic link"" found at the top of the topic above, or by clicking the following link:

{template.BaseUrl}/Forum/Topics/20848?handler=Unwatch

--
Thanks,
on behalf of TASVideos staff";

		await _emailSender.SendEmail(new StandardEmail
		{
			Recipients = recipientsList,
			Subject = subject,
			Message = message,
			ContainsHtml = false
		});
	}
}

public record TopicReplyNotificationTemplate(
	int PostId,
	int TopicId,
	string TopicTitle,
	string BaseUrl);
