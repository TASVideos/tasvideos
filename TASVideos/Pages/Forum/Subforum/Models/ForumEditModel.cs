using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Subforum.Models
{
	public class ForumEditModel
	{
		[Required]
		public string Name { get; set; }

		[Required]
		public string Description { get; set; }
	}
}
