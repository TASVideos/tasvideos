using System.ComponentModel.DataAnnotations;

namespace TASVideos.Pages.AwardsEditor;

public class AwardAssignmentModel
{
	[Required]
	public string? Award { get; set; }

	public IEnumerable<int> Users { get; set; } = [];

	public IEnumerable<int> Publications { get; set; } = [];
}
