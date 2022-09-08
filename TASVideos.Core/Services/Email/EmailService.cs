using System.Text.Encodings.Web;
using Microsoft.Extensions.Hosting;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.Email;

public interface IEmailService
{
	Task ResetPassword(string recipient, string link);
	Task EmailConfirmation(string recipient, string link);
	Task PasswordResetConfirmation(string recipient, string resetLink);
	Task TopicReplyNotification(IEnumerable<string> recipients, TopicReplyNotificationTemplate template);
	Task NewPrivateMessage(string recipient, string userName);
}

internal class EmailService : IEmailService
{
	private readonly IHostEnvironment _env;
	private readonly IEmailSender _emailSender;
	private readonly string _baseUrl;

	public EmailService(
		IHostEnvironment env,
		IEmailSender emailSender,
		AppSettings appSettings)
	{
		_env = env;
		_emailSender = emailSender;
		_baseUrl = appSettings.BaseUrl;
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

<a href=""{template.BaseUrl}/Forum/Posts/{template.PostId}"">A new post</a> has been sent to the {siteName} forum topic <a href=""{template.BaseUrl}/Forum/Topics/{template.TopicId}"">{template.TopicTitle}</a> since your last visit.

Notification emails for this topic will not be sent until you visit it.
<hr />
<p><a href=""{template.BaseUrl}/Forum/Topics/{template.TopicId}?handler=Unwatch"">Stop this topic from notifying you</a></p>
<p>To stop all topic notification emails, follow <a href=""{template.BaseUrl}/Profile/WatchedTopics"">this link</a> and press ""Stop Watching All"".</p>
<hr />
Thanks,
TASVideos staff";

		await _emailSender.SendEmail(new StandardEmail
		{
			Recipients = recipientsList,
			Subject = subject,
			Message = message,
			ContainsHtml = true
		});
	}

	public async Task NewPrivateMessage(string recipient, string userName)
	{
		string link = $"{_baseUrl}/Messages/Inbox";

		await _emailSender.SendEmail(new SingleEmail
		{
			ContainsHtml = false,
			Recipient = recipient,
			Subject = "New Private Message has arrived",
			Message = $@"Hello {userName}

You have received a new private message to your account on ""TASVideos"" and you have requested that you be notified on this event. You can view your new message by clicking on the following link:

{link}

Remember that you can always choose not to be notified of new messages by changing the appropriate setting in your profile."
		});
	}
}

public record TopicReplyNotificationTemplate(
	int PostId,
	int TopicId,
	string TopicTitle,
	string BaseUrl);
