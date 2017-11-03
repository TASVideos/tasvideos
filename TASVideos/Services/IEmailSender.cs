using System.Threading.Tasks;

namespace TASVideos.Services
{
	public interface IEmailSender
	{
		Task SendEmailAsync(string email, string subject, string message);
	}
}
