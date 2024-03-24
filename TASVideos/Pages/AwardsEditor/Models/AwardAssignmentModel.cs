using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.AwardsEditor;

public class AwardAssignmentModel
{
	[Required]
	public string? Award { get; set; }

	public List<int> Users { get; set; } = [];

	public List<int> Publications { get; set; } = [];
}
