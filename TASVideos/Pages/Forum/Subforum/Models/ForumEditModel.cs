using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.Forum.Subforum.Models;

public class ForumEditModel
{
	[StringLength(50)]
	public string Name { get; init; } = "";

	[StringLength(10)]
	[Display(Name = "Short Name", Description = "Used for IRC notifications and other external posts")]
	public string ShortName { get; init; } = "";

	[StringLength(1000)]
	public string? Description { get; init; }

	[Display(Name = "Category")]
	public int CategoryId { get; init; }

	[Display(Name = "Restricted Access", Description = "If set, only users with permission to restricted forums will be allowed to see this forum")]
	public bool Restricted { get; init; }
}
