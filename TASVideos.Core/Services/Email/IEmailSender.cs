namespace TASVideos.Core.Services.Email;

internal interface IEmailSender
{
	/// <summary>
	/// Sends an email to the given recipients,
	/// with the given subject and message
	/// </summary>
	Task SendEmail(IEmail email);
}
