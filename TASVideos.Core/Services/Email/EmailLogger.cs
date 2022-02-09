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
		if (_logger.IsEnabled(LogLevel.Information))
		{
			_logger.LogInformation(
				"Email Generated:\nRecipients: {recipients)}\nSubject: {subject}\nMessage: {message}",
				string.Join(",", email.Recipients),
				email.Subject,
				email.Message);
		}
		
		return Task.CompletedTask;
	}
}
