using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models.AccountViewModels
{
	public class ExternalLoginViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
