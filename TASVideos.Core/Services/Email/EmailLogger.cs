using Microsoft.Extensions.Logging;

namespace TASVideos.Core.Services.Email;

/// <summary>
/// An implementation of <see cref="IEmailSender" /> that simply logs email content
/// </summary>
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
