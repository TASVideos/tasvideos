using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Subforum.Models;

public class ForumEditModel
{
	[Required]
	[StringLength(50)]
	public string Name { get; set; } = "";

	[Required]
	[StringLength(10)]
	[Display(Name = "Short Name", Description = "Used for IRC notifications and other external posts")]
	public string ShortName { get; set; } = "";

	[Required]
	[StringLength(1000)]
	public string? Description { get; set; }

	[Display(Name = "Category")]
	[Required]
	public int CategoryId { get; set; }

	[Display(Name = "Restricted Access", Description = "If set, only users with permission to restricted forums will be allowed to see this forum")]
	public bool Restricted { get; set; }
}
