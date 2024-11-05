using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;
using TASVideos.Core.Settings;

namespace TASVideos.Core.Services.Email;

/// <summary>
/// An implementation of <see cref="IEmailSender"/> that uses a configured SMTP server
/// and user/password authentication
/// </summary>
internal class SmtpSender(
	IHostEnvironment env,
	AppSettings settings,
	ILogger<SmtpSender> logger)
	: IEmailSender
{
	private readonly AppSettings.EmailBasicAuthSettings _settings = settings.Email;

	public async Task SendEmail(IEmail email)
	{
		if (!_settings.IsEnabled())
		{
			logger.LogWarning("Attempting to send email without email address configured");
			return;
		}

		try
		{
			using var client = new SmtpClient();
			await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpServerPort, SecureSocketOptions.StartTls);
			await client.AuthenticateAsync(_settings.Email, _settings.Password);
			var message = BccList(email);
			await client.SendAsync(message);
			await client.DisconnectAsync(true);
		}
		catch (Exception ex)
		{
			logger.LogError("Unable to send email, subject: {subject} message: {message} exception: {ex}", email.Subject, email.Message, ex);
		}
	}

	private MimeMessage BccList(IEmail email)
	{
		var recipients = email.Recipients.ToList();
		var from = env.IsProduction() ? "TASVideos" : $"TASVideos {env.EnvironmentName} environment";
		var message = new MimeMessage();
		message.From.Add(new MailboxAddress(from, _settings.Email));

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
