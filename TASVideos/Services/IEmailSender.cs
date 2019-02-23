using System.Collections.Generic;
using System.Threading.Tasks;

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
		Task SendTopicNotification(IEnumerable<string> emailAddresses);
	}

	public class EmailSender : IEmailSender
	{
		public Task SendEmail(string email, string subject, string message)
		{
			return Task.CompletedTask;
		}

		public Task SendTopicNotification(IEnumerable<string> emailAddresses)
		{
			return Task.CompletedTask;
		}
	}
}
