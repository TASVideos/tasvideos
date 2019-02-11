using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Subforum.Models
{
	public class ForumEditModel
	{
		[Required]
		[StringLength(50)]
		public string Name { get; set; }

		[Required]
		[StringLength(1000)]
		public string Description { get; set; }
	}
}
