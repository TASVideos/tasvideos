using System.ComponentModel.DataAnnotations;

namespace TASVideos.Models.ManageViewModels
{
	public class IndexViewModel
	{
		public string Username { get; set; }

		public bool IsEmailConfirmed { get; set; }

		[Required]
		[EmailAddress]
		public string Email { get; set; }

		public string StatusMessage { get; set; }
	}
}
