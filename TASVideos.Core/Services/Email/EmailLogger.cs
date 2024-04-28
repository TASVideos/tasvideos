using Microsoft.Extensions.Logging;

namespace TASVideos.Core.Services.Email;

/// <summary>
/// An implementation of <see cref="IEmailSender" /> that simply logs email content
/// </summary>
internal class EmailLogger(ILogger<EmailLogger> logger) : IEmailSender
{
	public Task SendEmail(IEmail email)
	{
		if (logger.IsEnabled(LogLevel.Information))
		{
			logger.LogInformation(
				"Email Generated:\nRecipients: {recipients)}\nSubject: {subject}\nMessage: {message}",
				string.Join(",", email.Recipients),
				email.Subject,
				email.Message);
		}

		return Task.CompletedTask;
	}
}
