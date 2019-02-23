using System.Text.Encodings.Web;
using System.Threading.Tasks;

using TASVideos.Services.Email;

namespace TASVideos.Services
{
	public static class EmailSenderExtensions
	{
		public static Task SendEmailConfirmationAsync(this IEmailSender emailSender, string email, string link)
		{
			return emailSender.SendEmail(new SingleEmail
			{
				Recipient = email,
				Subject = "Confirm your email",
				Message = $"Please confirm your account by clicking this link: <a href='{HtmlEncoder.Default.Encode(link)}'>link</a>"
			});
		}
	}
}
