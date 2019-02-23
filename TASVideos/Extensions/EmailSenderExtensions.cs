using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace TASVideos.Services
{
	public static class EmailSenderExtensions
	{
		public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
		{
			return emailSender.SendEmail(
				email,
				"Confirm your email",
				$"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>");
		}
	}
}
