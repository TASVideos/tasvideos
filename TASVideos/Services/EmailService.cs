using System.Collections.Generic;
using System.Threading.Tasks;

namespace TASVideos.Services
{
	public interface IEmailService
	{
		/// <summary>
		/// Sends a topic reply notification email to the given email addresses
		/// </summary>
		Task SendTopicNotification(IEnumerable<string> emailAddresses);
	}

	public class EmailService : IEmailService
	{
		public async Task SendTopicNotification(IEnumerable<string> emailAddresses)
		{
			// TODO
		}
	}
}
