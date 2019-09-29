using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;

namespace TASVideos.Pages.UserFiles.Models
{
	public class UserFileUploadModel
	{
		[Required]
		public IFormFile File { get; set; }

		[Required]
		public string Title { get; set; }

		[Required]
		public string Description { get; set; }

		public bool Hidden { get; set; }
	}
}
